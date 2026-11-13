using ExtensaoZero.Core;
using ExtensaoZero.Investigation;
using ExtensaoZero.UI;
using Unity.Netcode;
using UnityEngine;

namespace ExtensaoZero.Multiplayer
{
    /// <summary>
    /// Estado compartilhado da missão. Host autoritativo; clientes recebem passos,
    /// evidências e conclusão. Nenhuma decisão de gameplay é tomada no cliente.
    /// </summary>
    public sealed class NetworkGameState : NetworkBehaviour
    {
        public NetworkVariable<int> MissionStep { get; } = new NetworkVariable<int>(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public NetworkVariable<float> Contamination { get; } = new NetworkVariable<float>(
            0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public NetworkVariable<bool> Complete { get; } = new NetworkVariable<bool>(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public NetworkVariable<bool> Started { get; } = new NetworkVariable<bool>(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private float m_NextSync;

        public override void OnNetworkSpawn()
        {
            GameManager.Instance?.AttachNetworkState(this);
            MissionStep.OnValueChanged += OnMissionStepChanged;
            Contamination.OnValueChanged += OnContaminationChanged;
            Complete.OnValueChanged += OnCompleteChanged;
            Started.OnValueChanged += OnStartedChanged;
            if (IsServer)
            {
                MissionStep.Value = 0;
                Contamination.Value = 0f;
                Complete.Value = false;
                Started.Value = false;
            }
        }

        public override void OnNetworkDespawn()
        {
            MissionStep.OnValueChanged -= OnMissionStepChanged;
            Contamination.OnValueChanged -= OnContaminationChanged;
            Complete.OnValueChanged -= OnCompleteChanged;
            Started.OnValueChanged -= OnStartedChanged;
        }

        public void SetMissionStep(int step)
        {
            if (!IsServer) return;
            MissionStep.Value = Mathf.Max(0, step);
        }

        public void BroadcastEvidence(EvidenceEntry entry)
        {
            if (!IsServer || entry == null) return;
            EvidenceClientRpc(entry.Id, entry.Title, entry.Body, (int)entry.Category);
        }

        public void NotifyMissionComplete()
        {
            if (!IsServer) return;
            Complete.Value = true;
            MissionCompleteClientRpc();
        }

        public void NotifyMissionStarted()
        {
            if (!IsServer) return;
            Started.Value = true;
            MissionStartedClientRpc();
        }

        private void Update()
        {
            if (!IsServer || !IsSpawned) return;
            if (Time.time < m_NextSync) return;
            m_NextSync = Time.time + 0.5f;
            var contamination = GameManager.Instance != null && GameManager.Instance.Contamination != null
                ? GameManager.Instance.Contamination.Normalized
                : 0f;
            Contamination.Value = contamination;
        }

        [ClientRpc]
        private void EvidenceClientRpc(string id, string title, string body, int category)
        {
            if (IsServer) return;
            var entry = new EvidenceEntry(id, title, body, (EvidenceCategory)category, "Sessão");
            GameManager.Instance?.RegisterEvidence(entry);
        }

        [ClientRpc]
        private void MissionCompleteClientRpc()
        {
            if (IsServer) return;
            GameManager.Instance?.OnRemoteMissionComplete();
        }

        [ClientRpc]
        private void MissionStartedClientRpc()
        {
            if (IsServer) return;
            GameManager.Instance?.BeginNetworkMissionAsClient();
        }

        private void OnMissionStepChanged(int previous, int current)
        {
            if (IsServer) return;
            GameManager.Instance?.UI?.SetObjective($"Etapa da missão: {current}");
        }

        private void OnContaminationChanged(float previous, float current)
        {
            if (IsServer) return;
            GameManager.Instance?.UI?.SetContamination(current);
        }

        private void OnCompleteChanged(bool previous, bool current)
        {
            if (!IsServer && current) GameManager.Instance?.OnRemoteMissionComplete();
        }

        private void OnStartedChanged(bool previous, bool current)
        {
            if (!IsServer && current) GameManager.Instance?.BeginNetworkMissionAsClient();
        }
    }
}
