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
            if (!string.IsNullOrEmpty(evidenceId))
            {
                GameManager.Instance.RegisterEvidence(new EvidenceEntry(evidenceId, title, body, category));
            }

            GameManager.Instance.UI.ShowSubtitle(title.ToUpperInvariant(), body, UITheme.Paper, 6f);
            GameManager.Instance.Audio.PlayUiClick();
        }
    }
}
