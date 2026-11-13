using System.Collections.Generic;
using ExtensaoZero.Core;
using ExtensaoZero.Environment;
using ExtensaoZero.Interaction;
using ExtensaoZero.Investigation;
using ExtensaoZero.Player;
using Unity.Netcode;
using UnityEngine;

namespace ExtensaoZero.Multiplayer
{
    /// <summary>
    /// Jogador de rede. O dono move localmente e envia posição ao host, que valida limites e
    /// replica. Todas as ações do jogador viram pedidos (ServerRpc) que o host valida — distância
    /// ≤ 3,2 m, cooldown de 0,2 s e limite de mensagens — antes de mandar todos executarem.
    /// </summary>
    public sealed class NetworkPlayer : NetworkBehaviour
    {
        public const float MaxInteractionDistance = 3.2f;
        public const float InteractionCooldown = 0.2f;
        public const int MaxMessagesPerSecond = 30;

        public FirstPersonController Controller { get; private set; }
        public string DisplayName => $"Investigador {(GameManager.Instance != null ? GameManager.Instance.IndexOfClient(OwnerClientId) + 1 : 1)}";

        private readonly NetworkVariable<Vector3> m_Position = new NetworkVariable<Vector3>(
            Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private readonly NetworkVariable<float> m_Yaw = new NetworkVariable<float>(
            0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private float m_NextSend;
        private bool m_Registered;
        private GameObject m_Body;

        // Validação no host.
        private float m_LastInteraction = -10f;
        private float m_WindowStart;
        private int m_WindowCount;

        private void Awake()
        {
            Controller = GetComponent<FirstPersonController>();
        }

        public override void OnNetworkSpawn()
        {
            Controller ??= GetComponent<FirstPersonController>();
            var interactor = GetComponent<PlayerInteractor>();
            if (Controller != null)
            {
                Controller.SetNetworkControlled(IsOwner);
                var equipment = PlayerEquipment.Attach(Controller, IsOwner);
                if (interactor != null) interactor.Initialize(IsOwner ? Controller.Camera : null, equipment, IsOwner);
            }

            if (!IsOwner)
            {
                BuildRemoteBody();
                ApplyRemoteTransform();
            }

            m_Position.OnValueChanged += OnRemoteTransformChanged;
            m_Yaw.OnValueChanged += OnRemoteYawChanged;
            GameManager.Instance?.OnNetworkPlayerSpawned(this);
            m_Registered = true;
        }

        public override void OnNetworkDespawn()
        {
            m_Position.OnValueChanged -= OnRemoteTransformChanged;
            m_Yaw.OnValueChanged -= OnRemoteYawChanged;
            if (m_Registered) GameManager.Instance?.OnNetworkPlayerDespawned(this);
            m_Registered = false;
        }

        /// <summary>Corpo simples para os outros jogadores verem (não colide, não projeta sombra).</summary>
        private void BuildRemoteBody()
        {
            if (m_Body != null) return;
            m_Body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            m_Body.name = "CorpoRemoto";
            m_Body.transform.SetParent(transform, false);
            m_Body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            m_Body.transform.localScale = new Vector3(0.6f, 0.9f, 0.6f);
            var collider = m_Body.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            var renderer = m_Body.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = MaterialLibrary.Get("investigator", new Color(0.32f, 0.3f, 0.24f), 0.2f, 0f);
            var lamp = new GameObject("LanternaRemota");
            lamp.transform.SetParent(transform, false);
            lamp.transform.localPosition = new Vector3(0.16f, 1.5f, 0.3f);
            var light = lamp.AddComponent<Light>();
            light.type = LightType.Spot;
            light.range = 12f;
            light.spotAngle = 50f;
            light.intensity = 1.6f;
            light.shadows = LightShadows.None;
            light.color = new Color(1f, 0.93f, 0.82f);
        }

        private void OnRemoteTransformChanged(Vector3 previous, Vector3 current)
        {
            if (!IsOwner && (transform.position - current).sqrMagnitude > 400f) ApplyRemoteTransform();
        }

        private void OnRemoteYawChanged(float previous, float current)
        {
        }

        private void Update()
        {
            if (!IsSpawned) return;
            if (IsOwner)
            {
                if (Time.time < m_NextSend) return;
                m_NextSend = Time.time + 0.066f;
                SubmitTransformServerRpc(transform.position, transform.eulerAngles.y);
            }
            else
            {
                // Interpolação curta para evitar teleporte visual em LAN.
                var target = m_Position.Value;
                transform.position = Vector3.Lerp(transform.position, target, 18f * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, m_Yaw.Value, 0f), 18f * Time.deltaTime);
            }
        }

        private void ApplyRemoteTransform()
        {
            if (IsOwner) return;
            transform.position = m_Position.Value;
            transform.rotation = Quaternion.Euler(0f, m_Yaw.Value, 0f);
        }

        /// <summary>Limite de frequência de mensagens no host (NETWORK §6).</summary>
        private bool AllowMessage()
        {
            if (Time.time - m_WindowStart > 1f)
            {
                m_WindowStart = Time.time;
                m_WindowCount = 0;
            }

            m_WindowCount++;
            return m_WindowCount <= MaxMessagesPerSecond;
        }

        // ------------------------------------------------------------ pedidos do dono

        public void RequestInteract(int interactionId) => RequestInteractServerRpc(interactionId);
        public void RequestDropCable() => RequestDropCableServerRpc();
        public void ReportZone(string zoneId) => ReportZoneServerRpc(zoneId ?? string.Empty);
        public void ReportLineRecovered() => ReportLineRecoveredServerRpc();
        public void ReportLayersHeard(string callId, int mask) => ReportLayersHeardServerRpc(callId ?? string.Empty, mask);

        public void ReportEvidence(EvidenceEntry entry)
        {
            if (entry == null) return;
            ReportEvidenceServerRpc(entry.Id ?? string.Empty, entry.Title ?? string.Empty, entry.Body ?? string.Empty, (int)entry.Category);
        }

        [ServerRpc]
        private void SubmitTransformServerRpc(Vector3 position, float yaw, ServerRpcParams rpcParams = default)
        {
            // Validação básica de limites: o host não confia cegamente no cliente.
            if (position.y < -8f || position.y > 20f) return;
            if (Mathf.Abs(position.x) > 80f || position.z < -40f || position.z > 90f) return;
            m_Position.Value = position;
            m_Yaw.Value = yaw;
            if (!IsOwner) transform.position = position;
        }

        [ServerRpc]
        private void RequestInteractServerRpc(int interactionId, ServerRpcParams rpcParams = default)
        {
            var manager = GameManager.Instance;
            if (manager == null || manager.State != GameState.Playing || !AllowMessage()) return;
            if (Time.time - m_LastInteraction < InteractionCooldown) return;
            var target = InteractionRegistry.Get(interactionId);
            if (target == null || target.InteractionTransform == null) return;

            var position = IsOwner ? transform.position : m_Position.Value;
            var distance = Vector3.Distance(position + Vector3.up * 1.2f, target.InteractionTransform.position);
            if (distance > MaxInteractionDistance + 1.4f)
            {
                Debug.Log($"[EZ] Interação ignorada: cliente {OwnerClientId} longe demais ({distance:0.0} m).");
                return;
            }

            var interactor = GetComponent<PlayerInteractor>();
            if (interactor != null && !target.CanInteract(interactor)) return;
            m_LastInteraction = Time.time;
            manager.SetActor(OwnerClientId);
            manager.NetworkState?.ExecuteInteraction(interactionId, OwnerClientId);
        }

        [ServerRpc]
        private void RequestDropCableServerRpc(ServerRpcParams rpcParams = default)
        {
            if (!AllowMessage()) return;
            GameManager.Instance?.NetworkState?.ExecuteDropCable(OwnerClientId);
        }

        [ServerRpc]
        private void ReportZoneServerRpc(string zoneId, ServerRpcParams rpcParams = default)
        {
            var manager = GameManager.Instance;
            if (manager == null || !AllowMessage() || string.IsNullOrEmpty(zoneId) || zoneId.Length > 48) return;
            manager.SetActor(OwnerClientId);
            ZoneTrigger.ApplyOnHost(manager, zoneId);
        }

        [ServerRpc]
        private void ReportLineRecoveredServerRpc(ServerRpcParams rpcParams = default)
        {
            if (!AllowMessage()) return;
            GameManager.Instance?.OnLineBusyRecovered(OwnerClientId);
        }

        [ServerRpc]
        private void ReportLayersHeardServerRpc(string callId, int mask, ServerRpcParams rpcParams = default)
        {
            if (!AllowMessage() || string.IsNullOrEmpty(callId) || callId.Length > 48) return;
            GameManager.Instance?.OnLayersHeardOnHost(callId, mask & 0xF);
        }

        [ServerRpc]
        private void ReportEvidenceServerRpc(string id, string title, string body, int category, ServerRpcParams rpcParams = default)
        {
            if (!AllowMessage() || string.IsNullOrEmpty(id) || id.Length > 48) return;
            // Só fatos conhecidos pelo catálogo são aceitos: o cliente não inventa evidências.
            var known = EvidenceCatalog.Find(id);
            if (known == null) return;
            GameManager.Instance?.RegisterEvidence(known);
        }
    }
}
