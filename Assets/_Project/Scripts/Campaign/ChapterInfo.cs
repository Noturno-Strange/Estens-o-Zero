namespace ExtensaoZero.Campaign
{
    /// <summary>Metadados de um capítulo da campanha (GDD §13 / Story Bible §6).</summary>
    public sealed class ChapterInfo
    {
        public int Index;
        public string Id;
        public string Title;
        public string Theme;
        public string Novelty;
        public string Briefing;
        public EquipmentKind[] Recommended;
    }
}
