using ExtensaoZero.Core;
using ExtensaoZero.Player;
using ExtensaoZero.UI;
using UnityEngine;

namespace ExtensaoZero.Interaction
{
    public sealed class BatteryStation : InteractableBase
    {
        public float rechargeAmount = 100f;
        private float m_NextRecharge;

        public override string GetPrompt(PlayerInteractor interactor)
        {
            return "Recarregar lanterna";
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (Time.time < m_NextRecharge) return;
            m_NextRecharge = Time.time + 2f;
            interactor.Equipment?.RechargeFlashlight(rechargeAmount);
            GameManager.Instance.Audio.PlayUiClick();
            GameManager.Instance.UI.ShowSubtitle("ENERGIA", "Lanterna recarregada.", UITheme.Green, 2.5f);
        }
    }
}
