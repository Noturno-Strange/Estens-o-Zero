using ExtensaoZero.Core;
using ExtensaoZero.Player;
using UnityEngine;

namespace ExtensaoZero.Interaction
{
    public sealed class IsolatorLever : InteractableBase
    {
        public Transform handle;
        public float pulledAngle = -55f;
        private bool m_Pulled;

        public override string GetPrompt(PlayerInteractor interactor)
        {
            return m_Pulled ? "Linha isolada" : "Puxar alavanca de isolamento";
        }

        public override bool CanInteract(PlayerInteractor interactor)
        {
            return !m_Pulled && base.CanInteract(interactor);
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (m_Pulled) return;
            m_Pulled = true;
            interactionEnabled = false;
            if (handle != null)
            {
                handle.localRotation = Quaternion.Euler(pulledAngle, 0f, 0f);
            }

            GameManager.Instance.Audio.PlayWorldOneShot(
                Audio.ProceduralAudio.RelayClick(), transform.position, 1f, Audio.AudioCategory.Sfx);
            GameManager.Instance.OnIsolatorPulled();
        }
    }
}
