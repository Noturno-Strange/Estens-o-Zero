using ExtensaoZero.Core;
using ExtensaoZero.Interaction;
using ExtensaoZero.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ExtensaoZero.Player
{
    /// <summary>
    /// Raycast curto a partir do centro da câmera. Um único alvo por frame; sem física
    /// de destaque caro, apenas prompt textual.
    /// </summary>
    public sealed class PlayerInteractor : MonoBehaviour
    {
        public float range = 2.6f;
        public LayerMask mask = ~0;
        public PlayerEquipment Equipment { get; set; }
        public IInteractable Current { get; private set; }

        private Camera m_Camera;
        private string m_LastPrompt = string.Empty;

        public void Initialize(Camera camera, PlayerEquipment equipment)
        {
            m_Camera = camera;
            Equipment = equipment;
        }

        private void Update()
        {
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
                if (found != null && !found.CanInteract(this)) found = null;
            }

            Current = found;
            var prompt = found != null ? found.GetPrompt(this) : string.Empty;
            SetPrompt(prompt);

            var keyboard = Keyboard.current;
            if (found != null && keyboard != null && keyboard.eKey.wasPressedThisFrame)
            {
                found.Interact(this);
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
