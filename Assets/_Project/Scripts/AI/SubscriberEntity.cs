using System.Collections.Generic;
using ExtensaoZero.Core;
using ExtensaoZero.Environment;
using ExtensaoZero.Interaction;
using ExtensaoZero.UI;
using UnityEngine;

namespace ExtensaoZero.AI
{
    public enum SubscriberState
    {
        Dormant,
        Observing,
        Investigating,
        Interfering,
        Contaminating,
        Approaching,
        Manifesting,
        Retreating
    }

    /// <summary>
    /// A Assinante (GDD §9, Story Bible §3). Não é um perseguidor: é uma presença ligada à rede.
    /// Máquina de estados simples dirigida pela contaminação e pelo capítulo. A manifestação
    /// física é rara, distante e some quando observada por ~1,2 s ou quando alguém se aproxima.
    /// Nunca causa morte: o pior contato possível é a Linha Ocupada. A aparência completa só
    /// é revelada no Capítulo 5.
    /// </summary>
    public sealed class SubscriberEntity : MonoBehaviour
    {
        public static SubscriberEntity Instance { get; private set; }

        public SubscriberState State { get; private set; } = SubscriberState.Dormant;
        public ManifestationMode Mode { get; private set; } = ManifestationMode.Silhouette;
        public bool IsManifesting => State == SubscriberState.Manifesting || State == SubscriberState.Approaching;
        public bool IsVisible => m_Body != null && m_Body.activeSelf;

        public float manifestDuration = 6f;
        public float lookTimeToVanish = 1.2f;
        public float vanishDistance = 5f;
        public float approachSpeed = 0.75f;
        public float contactDistance = 2.2f;

        private static readonly string[] s_Anchors =
        {
            "corridor_end", "archive_dead", "reflection", "tunnel_east", "tunnel_north", "original_center"
        };

        private GameManager m_Manager;
        private WorldContext m_World;
        private GameObject m_Body;
        private readonly List<GameObject> m_FullFormParts = new List<GameObject>();
        private float m_Timer;
        private float m_LookedTimer;
        private float m_StateTimer;
        private int m_Chapter;

        public static SubscriberEntity Create(Transform parent, GameManager manager, WorldContext world)
        {
            var go = new GameObject("SubscriberEntity");
            if (parent != null) go.transform.SetParent(parent, false);
            var entity = go.AddComponent<SubscriberEntity>();
            entity.m_Manager = manager;
            entity.m_World = world;
            entity.BuildBody();
            return entity;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        /// <summary>Silhueta alta (2,3 m), estreita, membros longos, cabeça pequena demais (ART §7).</summary>
        private void BuildBody()
        {
            m_Body = new GameObject("Silhouette");
            m_Body.transform.SetParent(transform, false);
            var dark = MaterialLibrary.Get("subscriber", new Color(0.015f, 0.022f, 0.02f), 0f, 0f);

            Part(m_Body.transform, PrimitiveType.Capsule, "Torso", new Vector3(0f, 1.3f, 0f), new Vector3(0.42f, 0.62f, 0.24f), dark, false);
            Part(m_Body.transform, PrimitiveType.Sphere, "Cabeca", new Vector3(0f, 2.12f, 0f), new Vector3(0.17f, 0.2f, 0.17f), dark, false);
            Part(m_Body.transform, PrimitiveType.Capsule, "Braco_E", new Vector3(-0.3f, 1.15f, 0f), new Vector3(0.08f, 0.62f, 0.08f), dark, false);
            Part(m_Body.transform, PrimitiveType.Capsule, "Braco_D", new Vector3(0.3f, 1.15f, 0f), new Vector3(0.08f, 0.62f, 0.08f), dark, false);
            Part(m_Body.transform, PrimitiveType.Capsule, "Perna_E", new Vector3(-0.11f, 0.42f, 0f), new Vector3(0.1f, 0.45f, 0.1f), dark, false);
            Part(m_Body.transform, PrimitiveType.Capsule, "Perna_D", new Vector3(0.11f, 0.42f, 0f), new Vector3(0.1f, 0.45f, 0.1f), dark, false);

            // Forma completa: cabos saindo do corpo e um LED vermelho no lugar do rosto.
            var cable = MaterialLibrary.Get("subscriber_cable", new Color(0.05f, 0.06f, 0.055f), 0.4f, 0.2f);
            for (var i = 0; i < 7; i++)
            {
                var angle = i * 51f;
                var part = Part(m_Body.transform, PrimitiveType.Cylinder, "Cabo_" + i,
                    new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad) * 0.22f, 1.5f - i * 0.12f, Mathf.Cos(angle * Mathf.Deg2Rad) * 0.1f),
                    new Vector3(0.02f, 0.5f + i * 0.06f, 0.02f), cable, true);
                part.transform.localRotation = Quaternion.Euler(20f + i * 9f, angle, 30f);
            }

            Part(m_Body.transform, PrimitiveType.Sphere, "Led", new Vector3(0f, 2.14f, -0.09f), new Vector3(0.04f, 0.04f, 0.04f),
                MaterialLibrary.Emissive("subscriber_led", new Color(0.95f, 0.08f, 0.05f), 3f), true);
            m_Body.SetActive(false);
        }

        private GameObject Part(Transform parent, PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material, bool fullFormOnly)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            var collider = go.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            if (fullFormOnly) m_FullFormParts.Add(go);
            return go;
        }

        public void Configure(int chapter)
        {
            m_Chapter = chapter;
            HideBody();
            State = chapter > 0 ? SubscriberState.Observing : SubscriberState.Dormant;
            m_StateTimer = 0f;
        }

        public void SetState(SubscriberState state)
        {
            State = state;
        }

        /// <summary>Manifestação compatível com a fatia: corredor, silhueta.</summary>
        public void Manifest()
        {
            ManifestAt("corridor_end", ManifestationMode.Silhouette);
        }

        public void ManifestAt(string anchorId, ManifestationMode mode)
        {
            var anchor = m_World != null ? m_World.Anchor(anchorId) : null;
            if (anchor == null) return;
            Mode = mode;
            State = mode == ManifestationMode.Approach ? SubscriberState.Approaching : SubscriberState.Manifesting;
            m_Timer = mode == ManifestationMode.Approach ? 40f : mode == ManifestationMode.FullForm ? 30f : manifestDuration;
            m_LookedTimer = 0f;
            transform.SetPositionAndRotation(anchor.position, anchor.rotation);
            foreach (var part in m_FullFormParts) part.SetActive(mode == ManifestationMode.FullForm || m_Chapter >= 5);
            m_Body.SetActive(true);

            var reduce = m_Manager != null && m_Manager.Settings != null && m_Manager.Settings.reduceScareIntensity;
            m_Manager?.Audio?.PlayScare(Audio.ProceduralAudio.Whisper(), reduce ? 0.35f : 0.7f);
            switch (mode)
            {
                case ManifestationMode.Reflection:
                    m_Manager?.UI?.ShowEventSubtitle("No vidro, atrás do seu reflexo, alguém está parado.", UITheme.Red, 5f);
                    break;
                case ManifestationMode.Approach:
                    m_Manager?.UI?.ShowEventSubtitle("Ela está mais perto toda vez que você desvia o olhar.", UITheme.Red, 5f);
                    break;
                case ManifestationMode.FullForm:
                    m_Manager?.UI?.ShowEventSubtitle("Ela está inteira na sala. Cabos onde deveria haver um rosto, e uma luz vermelha.", UITheme.Red, 6f);
                    break;
                default:
                    m_Manager?.UI?.ShowEventSubtitle("Há uma silhueta no fim do corredor. Ela não se move.", UITheme.Red, 5f);
                    break;
            }
        }

        /// <summary>Manifestação do Diretor de Tensão: a âncora válida mais próxima, sempre a distância.</summary>
        public void ManifestNearPlayer()
        {
            var controller = m_Manager != null ? m_Manager.LocalPlayerController : null;
            if (controller == null || m_World == null || IsVisible) return;
            string best = null;
            var bestDistance = float.MaxValue;
            foreach (var id in s_Anchors)
            {
                var anchor = m_World.Anchor(id);
                if (anchor == null) continue;
                var distance = Vector3.Distance(anchor.position, controller.transform.position);
                if (distance < 8f || distance > 22f || Mathf.Abs(anchor.position.y - controller.transform.position.y) > 2f) continue;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = id;
                }
            }

            if (best != null) ManifestAt(best, best == "reflection" ? ManifestationMode.Reflection : ManifestationMode.Silhouette);
        }

        public void Retreat()
        {
            if (!IsVisible && State != SubscriberState.Manifesting && State != SubscriberState.Approaching) return;
            State = SubscriberState.Retreating;
            HideBody();
            m_StateTimer = 0f;
            if (m_Manager != null)
            {
                if (m_Manager.IsHost) m_Manager.RaiseEvent("subscriber.retreated");
                else m_Manager.LocalNetworkPlayer?.ReportZone("subscriber_retreated");
            }
        }

        private void HideBody()
        {
            if (m_Body != null) m_Body.SetActive(false);
        }

        private void Update()
        {
            if (m_Manager == null || m_Manager.State != GameState.Playing) return;
            UpdatePresence();
            if (!IsVisible) return;

            var controller = m_Manager.LocalPlayerController;
            if (controller == null || controller.CameraTransform == null) return;

            m_Timer -= Time.deltaTime;
            if (m_Timer <= 0f)
            {
                Retreat();
                return;
            }

            var head = transform.position + Vector3.up * 1.5f;
            var toEntity = head - controller.CameraTransform.position;
            var distance = toEntity.magnitude;
            var dot = Vector3.Dot(controller.CameraTransform.forward, toEntity.normalized);
            var looking = distance < 26f && dot > 0.965f && !Occluded(controller.CameraTransform.position, head);

            if (Mode == ManifestationMode.Approach)
            {
                if (!looking && dot < 0.6f)
                {
                    var target = controller.transform.position;
                    target.y = transform.position.y;
                    transform.position = Vector3.MoveTowards(transform.position, target, approachSpeed * Time.deltaTime);
                    var flat = target - transform.position;
                    if (flat.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(flat);
                }

                if (Vector3.Distance(transform.position, controller.transform.position) < contactDistance)
                {
                    Contact();
                    return;
                }
            }
            else if (Mode != ManifestationMode.FullForm && distance < vanishDistance)
            {
                // Some quando alguém se aproxima (ART §7).
                m_Manager.UI?.ShowEventSubtitle("Quando você chega perto, não há nada.", UITheme.Dim, 3.5f);
                Retreat();
                return;
            }

            if (looking)
            {
                m_LookedTimer += Time.deltaTime;
                if (m_LookedTimer >= lookTimeToVanish && Mode != ManifestationMode.FullForm)
                {
                    m_Manager.UI?.ShowEventSubtitle("Quando você olha de novo, não há nada.", UITheme.Dim, 4f);
                    Retreat();
                }
            }
            else
            {
                m_LookedTimer = Mathf.Max(0f, m_LookedTimer - Time.deltaTime * 0.5f);
            }
        }

        private bool Occluded(Vector3 from, Vector3 to)
        {
            // O vidro do túnel não bloqueia o olhar: o reflexo é visto através dele.
            return Mode != ManifestationMode.Reflection && Physics.Linecast(from, to, out var hit, ~0, QueryTriggerInteraction.Ignore)
                   && hit.transform != transform && !hit.transform.IsChildOf(transform);
        }

        /// <summary>Contato: nunca mata. Isola a linha de quem foi alcançado e recua.</summary>
        private void Contact()
        {
            m_Manager.UI?.ShowEventSubtitle("Um estalo no seu receptor. A linha cai.", UITheme.Red, 4f);
            if (m_Manager.IsMultiplayer && !m_Manager.IsHost) m_Manager.LocalNetworkPlayer?.ReportZone("subscriber_contact");
            else ZoneTrigger.ApplyOnHost(m_Manager, "subscriber_contact");
            Retreat();
        }

        /// <summary>Estados de fundo (sem corpo): refletem a contaminação e reagem às decisões.</summary>
        private void UpdatePresence()
        {
            if (IsVisible || m_Chapter <= 0 && State == SubscriberState.Dormant) return;
            m_StateTimer += Time.deltaTime;
            if (m_StateTimer < 2f) return;
            m_StateTimer = 0f;
            var contamination = m_Manager.Contamination != null ? m_Manager.Contamination.Value : 0f;
            if (contamination >= 60f) State = SubscriberState.Contaminating;
            else if (contamination >= 30f) State = SubscriberState.Interfering;
            else if (m_Manager.CallSession != null && m_Manager.CallSession.IsActive) State = SubscriberState.Investigating;
            else State = SubscriberState.Observing;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
