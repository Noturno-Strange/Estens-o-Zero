using ExtensaoZero.Interaction;
using ExtensaoZero.Player;
using UnityEngine;

namespace ExtensaoZero.Telephone
{
    /// <summary>
    /// Soquete físico. IDs são registrados na ordem de construção do mundo, iguais em
    /// todos os peers, então a interação pode ser referenciada por índice.
    /// </summary>
    public sealed class SwitchboardSocket : InteractableBase
    {
        public int extension = 200;
        public Switchboard board;
        public Renderer indicator;
        public Color idleColor = new Color(0.16f, 0.14f, 0.11f);
        public Color connectedColor = new Color(0.35f, 0.9f, 0.45f);
        public Color selectedColor = new Color(1f, 0.68f, 0.2f);

        private bool m_Connected;

        public bool Connected => m_Connected;

        protected override void Awake()
        {
            base.Awake();
            prompt = $"Extensão {extension:000}";
            if (board != null) board.RegisterSocket(this);
            SetConnected(false);
        }

        public override string GetPrompt(PlayerInteractor interactor)
        {
            if (board == null) return string.Empty;
            if (board.SelectedExtension == extension) return "Cancelar seleção";
            if (board.SelectedExtension >= 0) return $"Conectar a {extension:000}";
            return m_Connected ? $"Extensão {extension:000} (conectada)" : $"Selecionar {extension:000}";
        }

        public override void Interact(PlayerInteractor interactor)
        {
            board?.SelectSocket(extension, interactor != null ? interactor.Equipment : null);
        }

        public void SetConnected(bool connected)
        {
            m_Connected = connected;
            if (indicator == null) return;
            var color = connected ? connectedColor : idleColor;
            var material = indicator.material;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.color = color;
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", connected ? color * 1.4f : Color.black);
            }
        }

        public void SetSelected(bool selected)
        {
            if (indicator == null || m_Connected) return;
            var material = indicator.material;
            var color = selected ? selectedColor : idleColor;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.color = color;
        }
    }
}
