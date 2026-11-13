using ExtensaoZero.Core;
using ExtensaoZero.Investigation;
using Unity.Netcode;
using UnityEngine;

namespace ExtensaoZero.Multiplayer
{
    /// <summary>
    /// Estado compartilhado (NETWORK_ARCHITECTURE §7). Host autoritativo: só ele escreve.
    /// Clientes recebem etapa, contaminação, comandos de mundo, chamadas, eventos de tensão,
    /// Linha Ocupada e conclusão. Interações aprovadas são executadas em todos os peers na
    /// mesma ordem, o que mantém o mundo (portas, cabos, central) igual em todos.
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
            Contamination.OnValueChanged += OnContaminationChanged;
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
            Contamination.OnValueChanged -= OnContaminationChanged;
        }

        private void Update()
        {
            if (!IsServer || !IsSpawned) return;
            if (Time.time < m_NextSync) return;
            m_NextSync = Time.time + 0.5f;
            var manager = GameManager.Instance;
            Contamination.Value = manager != null && manager.Contamination != null ? manager.Contamination.Normalized : 0f;
        }

        private void OnContaminationChanged(float previous, float current)
        {
            if (IsServer) return;
            GameManager.Instance?.Contamination?.SetFromNetwork(current);
        }

        // ------------------------------------------------------------ API do host

        public void SetMissionStep(int step)
        {
            if (IsServer) MissionStep.Value = Mathf.Max(0, step);
        }

        public void StartMissionForAll(int chapter, int seed, int difficulty)
        {
            if (!IsServer) return;
            Started.Value = true;
            MissionStartedClientRpc(chapter, seed, difficulty);
        }

        public void ExecuteInteraction(int interactionId, ulong actor)
        {
            if (IsServer) ExecuteInteractionClientRpc(interactionId, actor);
        }

        public void ExecuteDropCable(ulong actor)
        {
            if (IsServer) DropCableClientRpc(actor);
        }

        public void BroadcastEvidence(EvidenceEntry entry)
        {
            if (!IsServer || entry == null) return;
            EvidenceClientRpc(entry.Id, entry.Title, entry.Body, (int)entry.Category);
        }

        public void BroadcastSubtitle(string title, string body, Color color, float duration)
        {
            if (IsServer) SubtitleClientRpc(title ?? string.Empty, body ?? string.Empty, color.r, color.g, color.b, duration);
        }

        public void BroadcastObjective(string objective)
        {
            if (IsServer) ObjectiveClientRpc(objective ?? string.Empty);
        }

        public void BroadcastCommand(string command)
        {
            if (IsServer) CommandClientRpc(command ?? string.Empty);
        }

        public void BroadcastRing(int extension, string callId)
        {
            if (IsServer) RingClientRpc(extension, callId ?? string.Empty);
        }

        public void BroadcastTension(string eventId)
        {
            if (IsServer) TensionClientRpc(eventId ?? string.Empty);
        }

        public void BroadcastLineBusy(ulong target, float duration, int frequency, int extension)
        {
            if (IsServer) LineBusyClientRpc(target, duration, frequency, extension);
        }

        public void BroadcastLineBusyEnded(ulong target, int extension)
        {
            if (IsServer) LineBusyEndedClientRpc(target, extension);
        }

        public void NotifyMissionComplete(string summary, int ending)
        {
            if (!IsServer) return;
            Complete.Value = true;
            MissionCompleteClientRpc(summary ?? string.Empty, ending);
        }

        // ------------------------------------------------------------ RPCs (todos os peers)

        [ClientRpc]
        private void MissionStartedClientRpc(int chapter, int seed, int difficulty)
        {
            GameManager.Instance?.OnNetworkMissionStarted(chapter, seed, difficulty);
        }

        [ClientRpc]
        private void ExecuteInteractionClientRpc(int interactionId, ulong actor)
        {
            GameManager.Instance?.ExecuteReplicatedInteraction(interactionId, actor);
        }

        [ClientRpc]
        private void DropCableClientRpc(ulong actor)
        {
            var player = GameManager.Instance != null ? GameManager.Instance.FindPlayer(actor) : null;
            var equipment = player != null ? player.GetComponent<Player.PlayerEquipment>() : null;
            equipment?.DropCable();
        }

        [ClientRpc]
        private void LineBusyClientRpc(ulong target, float duration, int frequency, int extension)
        {
            GameManager.Instance?.OnLineBusyBroadcast(target, duration, frequency, extension);
        }

        [ClientRpc]
        private void LineBusyEndedClientRpc(ulong target, int extension)
        {
            GameManager.Instance?.OnLineBusyEndedBroadcast(target, extension);
        }

        // ------------------------------------------------------------ RPCs (só clientes)

        [ClientRpc]
        private void EvidenceClientRpc(string id, string title, string body, int category)
        {
            if (IsServer) return;
            GameManager.Instance?.RegisterEvidence(new EvidenceEntry(id, title, body, (EvidenceCategory)category, "Sessão"), true);
        }

        [ClientRpc]
        private void SubtitleClientRpc(string title, string body, float r, float g, float b, float duration)
        {
            if (IsServer) return;
            GameManager.Instance?.UI?.ShowSubtitle(title, body, new Color(r, g, b), duration);
        }

        [ClientRpc]
        private void ObjectiveClientRpc(string objective)
        {
            if (IsServer) return;
            GameManager.Instance?.UI?.SetObjective(objective);
        }

        [ClientRpc]
        private void CommandClientRpc(string command)
        {
            if (IsServer) return;
            var manager = GameManager.Instance;
            if (manager != null) Environment.WorldCommands.Apply(manager, command);
        }

        [ClientRpc]
        private void RingClientRpc(int extension, string callId)
        {
            if (IsServer) return;
            var manager = GameManager.Instance;
            var phone = manager != null && manager.World != null ? manager.World.GetPhone(extension) : null;
            var call = Telephone.CallCatalog.Get(callId);
            if (phone != null && call != null) phone.BeginRing(call);
        }

        [ClientRpc]
        private void TensionClientRpc(string eventId)
        {
            if (IsServer) return;
            GameManager.Instance?.Tension?.RunLocal(eventId);
        }

        [ClientRpc]
        private void MissionCompleteClientRpc(string summary, int ending)
        {
            if (IsServer) return;
            GameManager.Instance?.OnRemoteMissionComplete(summary, ending);
        }
    }
}
