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
            if (interactor == null || Time.time < m_NextRecharge && IsLocal(interactor)) return;
            if (IsLocal(interactor)) m_NextRecharge = Time.time + 2f;
            interactor.Equipment?.RechargeFlashlight(rechargeAmount);
            if (!IsLocal(interactor)) return;
            GameManager.Instance?.Audio?.PlayUiClick();
            TellActor(interactor, "ENERGIA", "Lanterna recarregada.", UITheme.Green, 2.5f);
        }
    }
}
