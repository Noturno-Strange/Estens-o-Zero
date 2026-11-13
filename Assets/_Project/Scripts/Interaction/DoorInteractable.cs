using System.Collections;
using ExtensaoZero.Audio;
using ExtensaoZero.Core;
using ExtensaoZero.Player;
using UnityEngine;

namespace ExtensaoZero.Interaction
{
    public sealed class DoorInteractable : InteractableBase
    {
        public float openAngle = 95f;
        public float speed = 2.2f;
        private bool m_Open;
        private bool m_Moving;
        private Quaternion m_ClosedRotation;

        protected override void Awake()
        {
            base.Awake();
            m_ClosedRotation = transform.localRotation;
            prompt = "Abrir porta";
        }

        public override string GetPrompt(PlayerInteractor interactor)
        {
            return m_Open ? "Fechar porta" : "Abrir porta";
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (m_Moving) return;
            m_Open = !m_Open;
            prompt = m_Open ? "Fechar porta" : "Abrir porta";
            StartCoroutine(RotateDoor());
            GameManager.Instance.Audio.PlayWorldOneShot(
                ProceduralAudio.DoorCreak(), transform.position, 0.5f, AudioCategory.Sfx);
        }

        private IEnumerator RotateDoor()
        {
            m_Moving = true;
            var target = m_Open ? m_ClosedRotation * Quaternion.Euler(0f, openAngle, 0f) : m_ClosedRotation;
            while (Quaternion.Angle(transform.localRotation, target) > 0.5f)
            {
                transform.localRotation = Quaternion.RotateTowards(
                    transform.localRotation, target, speed * 60f * Time.deltaTime);
                yield return null;
            }

            transform.localRotation = target;
            m_Moving = false;
        }
    }
}
