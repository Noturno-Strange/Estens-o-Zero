using System.Collections.Generic;

namespace ExtensaoZero.Investigation
{
    /// <summary>
    /// Núcleo puro do caderno, sem dependência de cena. Testável em EditMode.
    /// </summary>
    public sealed class NotebookModel
    {
        private readonly List<EvidenceEntry> m_Entries = new List<EvidenceEntry>();
        private readonly HashSet<string> m_Ids = new HashSet<string>();

        public IReadOnlyList<EvidenceEntry> Entries => m_Entries;
        public int Count => m_Entries.Count;
        public string Notes { get; set; } = string.Empty;

        public bool Contains(string id)
        {
            return !string.IsNullOrEmpty(id) && m_Ids.Contains(id);
        }

        public bool Register(EvidenceEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.Id)) return false;
            if (!m_Ids.Add(entry.Id)) return false;
            m_Entries.Add(entry);
            return true;
        }

        public EvidenceEntry Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            for (var i = 0; i < m_Entries.Count; i++)
            {
                if (m_Entries[i].Id == id) return m_Entries[i];
            }

            return null;
        }

        public void Clear()
        {
            m_Entries.Clear();
            m_Ids.Clear();
            Notes = string.Empty;
        }
    }
}
