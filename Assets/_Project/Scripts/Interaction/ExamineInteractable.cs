using ExtensaoZero.Core;
using ExtensaoZero.Investigation;
using ExtensaoZero.Player;
using ExtensaoZero.UI;
using UnityEngine;

namespace ExtensaoZero.Interaction
{
    public sealed class ExamineInteractable : InteractableBase
    {
        public string title = "Documento";
        [TextArea(3, 8)] public string body = "";
        public string evidenceId = "";
        public EvidenceCategory category = EvidenceCategory.Document;
        private bool m_Examined;

        public override string GetPrompt(PlayerInteractor interactor)
        {
            return m_Examined ? "Reler" : "Examinar";
        }

        public override void Interact(PlayerInteractor interactor)
        {
            m_Examined = true;
            var manager = GameManager.Instance;
            if (manager == null) return;
            if (!string.IsNullOrEmpty(evidenceId))
            {
                manager.RegisterEvidence(new EvidenceEntry(evidenceId, title, body, category));
                manager.RaiseEvent("examine:" + evidenceId);
            }

            TellActor(interactor, title.ToUpperInvariant(), body, UITheme.Paper, 7f);
            if (IsLocal(interactor)) manager.Audio?.PlayUiClick();
        }

        public void ResetExamined()
        {
            m_Examined = false;
        }
    }
}
