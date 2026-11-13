using ExtensaoZero.Audio;
using ExtensaoZero.Core;
using ExtensaoZero.Player;
using UnityEngine;

namespace ExtensaoZero.Interaction
{
    /// <summary>Acionador genérico de missão (console, plugue, chave). Gera "eventId" no host.</summary>
    public sealed class MissionSwitch : InteractableBase
    {
        public string eventId = "switch";
        public string label = "Acionar";
        public bool oneShot;
        private bool m_Used;

        public override string GetPrompt(PlayerInteractor interactor)
        {
            return label;
        }

        public override bool CanInteract(PlayerInteractor interactor)
        {
            return !(oneShot && m_Used) && base.CanInteract(interactor);
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (oneShot && m_Used) return;
            m_Used = true;
            GameManager.Instance?.Audio?.PlayWorldOneShot(ProceduralAudio.RelayClick(), transform.position, 0.8f, AudioCategory.Sfx);
            GameManager.Instance?.RaiseEvent(eventId);
        }

        public void ResetSwitch(string newLabel = null)
        {
            m_Used = false;
            if (!string.IsNullOrEmpty(newLabel)) label = newLabel;
        }
    }
}
