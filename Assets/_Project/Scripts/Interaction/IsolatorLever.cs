using System.Collections;
using ExtensaoZero.Core;
using ExtensaoZero.Player;
using UnityEngine;

namespace ExtensaoZero.Interaction
{
    /// <summary>
    /// Alavanca de isolamento geral. A missão decide se o isolamento é aceito; se não for,
    /// a alavanca volta à posição original (sem softlock).
    /// </summary>
    public sealed class IsolatorLever : InteractableBase
    {
        public Transform handle;
        public float pulledAngle = -55f;
        private bool m_Locked;
        private bool m_Moving;

        public bool Locked => m_Locked;

        public override string GetPrompt(PlayerInteractor interactor)
        {
            return m_Locked ? "Linha isolada" : "Puxar alavanca de isolamento";
        }

        public override bool CanInteract(PlayerInteractor interactor)
        {
            return !m_Locked && !m_Moving && base.CanInteract(interactor);
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (m_Locked || m_Moving) return;
            SetHandle(true);
            GameManager.Instance?.Audio?.PlayWorldOneShot(
                Audio.ProceduralAudio.RelayClick(), transform.position, 1f, Audio.AudioCategory.Sfx);
            GameManager.Instance?.OnIsolatorPulled();
            if (!m_Locked && isActiveAndEnabled) StartCoroutine(ReturnHandle());
        }

        /// <summary>Trava a alavanca puxada (isolamento aceito pela missão).</summary>
        public void Lock(bool locked)
        {
            m_Locked = locked;
            interactionEnabled = !locked;
            SetHandle(locked);
        }

        private void SetHandle(bool pulled)
        {
            if (handle != null) handle.localRotation = Quaternion.Euler(pulled ? pulledAngle : 0f, 0f, 0f);
        }

        private IEnumerator ReturnHandle()
        {
            m_Moving = true;
            yield return new WaitForSeconds(1.1f);
            if (!m_Locked) SetHandle(false);
            m_Moving = false;
        }
    }
}
