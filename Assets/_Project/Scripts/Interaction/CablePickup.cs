using ExtensaoZero.Core;
using ExtensaoZero.Player;
using ExtensaoZero.Telephone;
using ExtensaoZero.UI;
using UnityEngine;

namespace ExtensaoZero.Interaction
{
    public sealed class CablePickup : InteractableBase
    {
        public string cableLabel = "CABO 214";
        public int cableLength = 1;
        private bool m_Taken;

        public override string GetPrompt(PlayerInteractor interactor)
        {
            if (m_Taken) return string.Empty;
            if (interactor.Equipment != null && interactor.Equipment.HasCable) return "Você já carrega um cabo";
            return $"Pegar {cableLabel}";
        }

        public override bool CanInteract(PlayerInteractor interactor)
        {
            return !m_Taken && base.CanInteract(interactor);
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (m_Taken || interactor.Equipment == null) return;
            m_Taken = true;
            interactor.Equipment.PickUpCable(new CableItem(cableLabel, cableLength));
            gameObject.SetActive(false);
            GameManager.Instance.Audio.PlayUiClick();
            GameManager.Instance.OnCablePicked();
            GameManager.Instance.UI.ShowSubtitle("EQUIPAMENTO", $"{cableLabel} recolhido. Conecte-o à central.", UITheme.Amber, 4f);
        }
    }
}
