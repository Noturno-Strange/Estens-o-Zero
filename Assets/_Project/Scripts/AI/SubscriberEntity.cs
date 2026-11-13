using ExtensaoZero.Core;
using ExtensaoZero.Environment;
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
    /// A Assinante. Não é um perseguidor: é uma presença ligada à rede. A manifestação
    /// física é rara, distante e some quando observada. Nunca causa morte arbitrária.
    /// </summary>
    public sealed class SubscriberEntity : MonoBehaviour
    {
        public static SubscriberEntity Instance { get; private set; }

        public SubscriberState State { get; private set; } = SubscriberState.Dormant;
        public bool IsManifesting => State == SubscriberState.Manifesting;

        public float manifestDuration = 6f;

        private GameManager m_Manager;
        private WorldContext m_World;
        private GameObject m_Silhouette;
        private float m_Timer;
        private float m_LookedTimer;

        public static SubscriberEntity Create(Transform parent, GameManager manager, WorldContext world)
        {
            var go = new GameObject("SubscriberEntity");
            if (parent != null) go.transform.SetParent(parent, false);
            var entity = go.AddComponent<SubscriberEntity>();
            entity.m_Manager = manager;
            entity.m_World = world;
            entity.BuildSilhouette();
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

        private void BuildSilhouette()
        {
            m_Silhouette = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            m_Silhouette.name = "Silhouette";
            m_Silhouette.transform.SetParent(transform, false);
            m_Silhouette.transform.localScale = new Vector3(0.52f, 0.92f, 0.28f);
            var collider = m_Silhouette.GetComponent<Collider>();
            if (collider != null) Destroy(collider);

            var material = Environment.MaterialLibrary.Get("subscriber", new Color(0.015f, 0.015f, 0.02f), 0f, 0f);
            var renderer = m_Silhouette.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
            m_Silhouette.SetActive(false);
        }

        public void SetState(SubscriberState state)
        {
            State = state;
        }

        public void Manifest()
        {
            if (m_World == null || m_World.SubscriberAnchor == null) return;
            State = SubscriberState.Manifesting;
            m_Timer = manifestDuration;
            m_LookedTimer = 0f;
            transform.position = m_World.SubscriberAnchor.position;
            transform.rotation = m_World.SubscriberAnchor.rotation;
            if (m_Silhouette != null) m_Silhouette.SetActive(true);
            m_Manager?.Audio?.PlayScare(Audio.ProceduralAudio.Whisper(), 0.7f);
            m_Manager?.UI?.ShowSubtitle("CORREDOR", "Há uma silhueta no fim do corredor. Ela não se move.", UITheme.Red, 5f);
            m_Manager?.RegisterEvidence(new Investigation.EvidenceEntry(
                "silhouette", "Silhueta no corredor",
                "Uma figura no fim do corredor, imóvel, desaparece quando observada diretamente.",
                Investigation.EvidenceCategory.Anomaly, "Corredor"));
        }

        public void Retreat()
        {
            if (State != SubscriberState.Manifesting && State != SubscriberState.Approaching) return;
            State = SubscriberState.Retreating;
            if (m_Silhouette != null) m_Silhouette.SetActive(false);
            State = SubscriberState.Dormant;
        }

        private void Update()
        {
            if (State != SubscriberState.Manifesting) return;

            m_Timer -= Time.deltaTime;
            if (m_Timer <= 0f)
            {
                Retreat();
                return;
            }

            var manager = m_Manager;
            var controller = manager != null ? manager.LocalPlayerController : null;
            if (controller == null || controller.CameraTransform == null) return;

            var toEntity = transform.position + Vector3.up * 0.9f - controller.CameraTransform.position;
            var distance = toEntity.magnitude;
            var dot = Vector3.Dot(controller.CameraTransform.forward, toEntity.normalized);
            var looking = distance < 24f && dot > 0.965f;
            if (looking)
            {
                m_LookedTimer += Time.deltaTime;
                if (m_LookedTimer > 0.45f)
                {
                    m_Manager?.UI?.ShowSubtitle("", "Quando você olha de novo, não há nada.", UITheme.Dim, 4f);
                    Retreat();
                }
            }
            else
            {
                m_LookedTimer = Mathf.Max(0f, m_LookedTimer - Time.deltaTime * 0.5f);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
