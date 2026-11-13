using ExtensaoZero.Interaction;
using ExtensaoZero.Player;
using UnityEngine;

namespace ExtensaoZero.Telephone
{
    /// <summary>
    /// Soquete físico. IDs são registrados na ordem de construção do mundo, iguais em
    /// todos os peers, então a interação pode ser referenciada por índice.
    /// </summary>
    public sealed class SwitchboardSocket : InteractableBase, ICircuitReadable
    {
        public int extension = 200;
        public Switchboard board;
        public Renderer indicator;
        public Color idleColor = new Color(0.16f, 0.14f, 0.11f);
        public Color connectedColor = new Color(0.35f, 0.9f, 0.45f);
        public Color selectedColor = new Color(1f, 0.68f, 0.2f);
        public Color compromisedColor = new Color(0.95f, 0.18f, 0.12f);

        private bool m_Connected;
        private bool m_Selected;
        private bool m_Compromised;
        private bool m_ContaminatedMark;

        public bool Connected => m_Connected;
        public bool Compromised => m_Compromised;
        public bool ContaminatedMark => m_ContaminatedMark;

        public CircuitReading Reading
        {
            get
            {
                if (m_Compromised || m_ContaminatedMark) return CircuitReading.Contaminated;
                return m_Connected ? CircuitReading.Energized : CircuitReading.Interrupted;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            prompt = $"Extensão {extension:000}";
            if (board != null) board.RegisterSocket(this);
            Refresh();
        }

        public override string GetPrompt(PlayerInteractor interactor)
        {
            if (board == null) return string.Empty;
            if (m_Compromised && board.SelectedExtension < 0) return $"Restaurar extensão {extension:000}";
            if (board.SelectedExtension == extension) return "Cancelar seleção";
            if (board.SelectedExtension >= 0) return $"Conectar a {Name}";
            return m_Connected ? $"{Name} (conectada)" : $"Selecionar {Name}";
        }

        private string Name => extension == 0 ? "tronco 0" : extension == 9 ? "linha 9" : $"extensão {extension:000}";

        public override void Interact(PlayerInteractor interactor)
        {
            board?.SelectSocket(extension, interactor != null ? interactor.Equipment : null, IsLocal(interactor));
        }

        public void SetConnected(bool connected)
        {
            m_Connected = connected;
            Refresh();
        }

        public void SetSelected(bool selected)
        {
            m_Selected = selected;
            Refresh();
        }

        public void SetCompromised(bool compromised)
        {
            m_Compromised = compromised;
            Refresh();
        }

        public void SetContaminatedMark(bool contaminated)
        {
            m_ContaminatedMark = contaminated;
            Refresh();
        }

        private void Refresh()
        {
            if (indicator == null) return;
            Color color;
            if (m_Compromised) color = compromisedColor;
            else if (m_Selected && !m_Connected) color = selectedColor;
            else if (m_Connected) color = connectedColor;
            else color = idleColor;

            var material = indicator.material;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.color = color;
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                var lit = m_Connected || m_Compromised || m_Selected;
                material.SetColor("_EmissionColor", lit ? color * 1.4f : Color.black);
            }
        }
    }
}
