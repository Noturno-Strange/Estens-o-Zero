using System.Collections;
using ExtensaoZero.Audio;
using ExtensaoZero.Core;
using ExtensaoZero.Player;
using ExtensaoZero.UI;
using UnityEngine;

namespace ExtensaoZero.Interaction
{
    /// <summary>
    /// Porta com dobradiça: o componente fica no pivô (borda da porta) e o painel é filho.
    /// Pode ser trancada por capítulo; a missão destranca via comando de mundo.
    /// </summary>
    public sealed class DoorInteractable : InteractableBase
    {
        public string doorId = "porta";
        public float openAngle = 95f;
        public float speed = 2.2f;
        public bool locked;
        public string lockedMessage = "Trancada.";

        public bool IsOpen => m_Open;

        private bool m_Open;
        private bool m_Moving;
        private Quaternion m_ClosedRotation;
        private bool m_ClosedCaptured;

        protected override void Awake()
        {
            base.Awake();
            CaptureClosed();
            prompt = "Abrir porta";
        }

        private void CaptureClosed()
        {
            if (m_ClosedCaptured) return;
            m_ClosedRotation = transform.localRotation;
            m_ClosedCaptured = true;
        }

        public override string GetPrompt(PlayerInteractor interactor)
        {
            if (locked) return "Porta trancada";
            return m_Open ? "Fechar porta" : "Abrir porta";
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (m_Moving) return;
            if (locked)
            {
                TellActor(interactor, "PORTA", lockedMessage, UITheme.Dim, 3f);
                GameManager.Instance?.Audio?.PlayWorldOneShot(ProceduralAudio.RelayClick(), transform.position, 0.4f, AudioCategory.Sfx);
                return;
            }

            SetOpen(!m_Open, true);
            if (m_Open) GameManager.Instance?.RaiseEvent("door.open:" + doorId);
        }

        public void SetOpen(bool open, bool animate)
        {
            CaptureClosed();
            m_Open = open;
            var target = m_Open ? m_ClosedRotation * Quaternion.Euler(0f, openAngle, 0f) : m_ClosedRotation;
            if (!animate || !isActiveAndEnabled)
            {
                StopAllCoroutines();
                m_Moving = false;
                transform.localRotation = target;
                return;
            }

            StartCoroutine(RotateDoor(target));
            GameManager.Instance?.Audio?.PlayWorldOneShot(ProceduralAudio.DoorCreak(), transform.position, 0.5f, AudioCategory.Sfx);
        }

        public void SetLocked(bool value)
        {
            locked = value;
            if (locked && m_Open) SetOpen(false, false);
        }

        private IEnumerator RotateDoor(Quaternion target)
        {
            m_Moving = true;
            while (Quaternion.Angle(transform.localRotation, target) > 0.5f)
            {
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, target, speed * 60f * Time.deltaTime);
                yield return null;
            }

            transform.localRotation = target;
            m_Moving = false;
        }
    }
}
