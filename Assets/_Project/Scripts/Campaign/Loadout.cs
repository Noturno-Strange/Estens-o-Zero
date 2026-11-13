using System.Collections.Generic;

namespace ExtensaoZero.Campaign
{
    /// <summary>Seleção de até dois equipamentos opcionais. Lógica pura.</summary>
    public sealed class Loadout
    {
        public const int MaxOptional = 2;
        private readonly List<EquipmentKind> m_Items = new List<EquipmentKind>();

        public IReadOnlyList<EquipmentKind> Items => m_Items;

        public bool Has(EquipmentKind kind) => m_Items.Contains(kind);

        /// <summary>Alterna o item. Retorna false se o limite impedir a adição.</summary>
        public bool Toggle(EquipmentKind kind)
        {
            if (m_Items.Remove(kind)) return true;
            if (m_Items.Count >= MaxOptional) return false;
            m_Items.Add(kind);
            return true;
        }

        public void Set(IEnumerable<EquipmentKind> kinds)
        {
            m_Items.Clear();
            if (kinds == null) return;
            foreach (var kind in kinds)
            {
                if (m_Items.Count >= MaxOptional) break;
                if (!m_Items.Contains(kind)) m_Items.Add(kind);
            }
        }

        public int ToMask()
        {
            var mask = 0;
            foreach (var item in m_Items) mask |= 1 << (int)item;
            return mask;
        }

        public static Loadout FromMask(int mask)
        {
            var loadout = new Loadout();
            for (var i = 0; i < 4; i++)
            {
                if ((mask & (1 << i)) != 0 && loadout.m_Items.Count < MaxOptional) loadout.m_Items.Add((EquipmentKind)i);
            }

            return loadout;
        }

        public static string Label(EquipmentKind kind)
        {
            switch (kind)
            {
                case EquipmentKind.Receiver: return "RECEPTOR PORTÁTIL";
                case EquipmentKind.Recorder: return "GRAVADOR ANALÓGICO";
                case EquipmentKind.CircuitMeter: return "MEDIDOR DE CIRCUITO";
                default: return "MAPA DE EXTENSÕES";
            }
        }

        public static string Description(EquipmentKind kind)
        {
            switch (kind)
            {
                case EquipmentKind.Receiver: return "Detecta sinais próximos e indica a direção aproximada.";
                case EquipmentKind.Recorder: return "Grava a camada ouvida (R) e a reproduz depois. Uma gravação por vez.";
                case EquipmentKind.CircuitMeter: return "Mostra se uma linha está energizada, interrompida ou contaminada.";
                default: return "Mostra setores e extensões já descobertos (M).";
            }
        }
    }
}
