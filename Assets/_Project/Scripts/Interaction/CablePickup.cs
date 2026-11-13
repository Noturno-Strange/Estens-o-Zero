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
        private Vector3 m_HomePosition;
        private Quaternion m_HomeRotation;
        private bool m_HomeCaptured;

        public bool Taken => m_Taken;

        protected override void Awake()
        {
            base.Awake();
            CaptureHome();
        }

        private void CaptureHome()
        {
            if (m_HomeCaptured) return;
            m_HomePosition = transform.position;
            m_HomeRotation = transform.rotation;
            m_HomeCaptured = true;
        }

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
            if (m_Taken || interactor == null || interactor.Equipment == null || interactor.Equipment.HasCable) return;
            m_Taken = true;
            interactor.Equipment.PickUpCable(new CableItem(cableLabel, cableLength, this));
            gameObject.SetActive(false);
            if (IsLocal(interactor)) GameManager.Instance?.Audio?.PlayUiClick();
            GameManager.Instance?.OnCablePicked(cableLabel);
            TellActor(interactor, "EQUIPAMENTO", $"{cableLabel} recolhido. Conecte-o à central. (G solta o cabo)", UITheme.Amber, 4f);
        }

        /// <summary>O cabo volta ao mundo onde o jogador o soltou.</summary>
        public void DropAt(Vector3 position)
        {
            m_Taken = false;
            transform.position = position;
            gameObject.SetActive(true);
        }

        /// <summary>Prepara o cabo para um capítulo: visível na posição de origem ou em uma âncora.</summary>
        public void ResetTo(Transform anchor, bool visible)
        {
            CaptureHome();
            m_Taken = false;
            if (anchor != null) transform.SetPositionAndRotation(anchor.position, anchor.rotation);
            else transform.SetPositionAndRotation(m_HomePosition, m_HomeRotation);
            gameObject.SetActive(visible);
        }
    }
}
