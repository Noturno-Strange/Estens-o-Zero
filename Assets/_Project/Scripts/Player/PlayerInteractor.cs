using ExtensaoZero.Campaign;
using ExtensaoZero.Core;
using ExtensaoZero.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ExtensaoZero.Player
{
    /// <summary>
    /// Raycast curto a partir do centro da câmera. Um único alvo por frame; prompt textual.
    /// Só o jogador local lê entrada; as cópias remotas existem para executar interações
    /// replicadas com o equipamento certo.
    /// </summary>
    public sealed class PlayerInteractor : MonoBehaviour
    {
        public float range = 2.6f;
        public LayerMask mask = ~0;
        public PlayerEquipment Equipment { get; set; }
        public IInteractable Current { get; private set; }
        public bool IsLocal { get; private set; } = true;

        private Camera m_Camera;
        private string m_LastPrompt = string.Empty;

        public void Initialize(Camera camera, PlayerEquipment equipment, bool isLocal)
        {
            m_Camera = camera;
            Equipment = equipment;
            IsLocal = isLocal;
        }

        private void Update()
        {
            if (!IsLocal) return;
            var manager = GameManager.Instance;
            if (manager == null || manager.State != GameState.Playing || Cursor.lockState != CursorLockMode.Locked)
            {
                SetPrompt(string.Empty);
                Current = null;
                return;
            }

            if (m_Camera == null) return;
            var ray = new Ray(m_Camera.transform.position, m_Camera.transform.forward);
            IInteractable found = null;
            if (Physics.Raycast(ray, out var hit, range, mask, QueryTriggerInteraction.Ignore))
            {
                found = hit.collider.GetComponentInParent<IInteractable>();
                if (found != null && !found.CanInteract(this) && !(found is ICircuitReadable)) found = null;
            }

            Current = found;
            var prompt = found != null && found.CanInteract(this) ? found.GetPrompt(this) : string.Empty;
            if (found is ICircuitReadable readable && Equipment != null && Equipment.Has(EquipmentKind.CircuitMeter))
            {
                prompt = (string.IsNullOrEmpty(prompt) ? string.Empty : prompt + "   ") + "[MEDIDOR: " + Describe(readable.Reading) + "]";
            }

            SetPrompt(prompt);

            var keyboard = Keyboard.current;
            if (found != null && keyboard != null && keyboard.eKey.wasPressedThisFrame && found.CanInteract(this))
            {
                manager.RequestInteraction(found, this);
            }
        }

        public static string Describe(CircuitReading reading)
        {
            switch (reading)
            {
                case CircuitReading.Contaminated: return "CONTAMINADA";
                case CircuitReading.Energized: return "ENERGIZADA";
                default: return "INTERROMPIDA";
            }
        }

        private void SetPrompt(string prompt)
        {
            if (prompt == m_LastPrompt) return;
            m_LastPrompt = prompt;
            GameManager.Instance?.UI?.SetInteractionPrompt(prompt);
        }
    }
}
