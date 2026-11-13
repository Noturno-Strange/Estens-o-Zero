using ExtensaoZero.Campaign;

namespace ExtensaoZero.UI
{
    /// <summary>Dados da tela de conclusão (tempo, contaminação, evidências, final).</summary>
    public sealed class MissionReport
    {
        public string ChapterTitle;
        public string Summary;
        public float Seconds;
        public float PeakContamination;
        public int EvidenceCount;
        public EndingKind Ending;
        public bool HasNextChapter;
    }
}
