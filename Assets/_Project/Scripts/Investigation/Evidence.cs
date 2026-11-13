using System;
using UnityEngine;

namespace ExtensaoZero.Investigation
{
    public enum EvidenceCategory
    {
        Document,
        Extension,
        Signal,
        Recording,
        Anomaly,
        Location
    }

    /// <summary>
    /// Fato descoberto. Nunca contém uma solução validada: apenas o que o jogador pode
    /// justificar a partir de uma fonte concreta (documento, chamada, observação).
    /// </summary>
    [Serializable]
    public sealed class EvidenceEntry
    {
        public string Id;
        public string Title;
        public string Body;
        public EvidenceCategory Category;
        public string Source;
        public float DiscoveredAt;

        public EvidenceEntry()
        {
        }

        public EvidenceEntry(string id, string title, string body, EvidenceCategory category, string source = "")
        {
            Id = id;
            Title = title;
            Body = body;
            Category = category;
            Source = source;
            DiscoveredAt = Time.realtimeSinceStartup;
        }

        public string CategoryLabel
        {
            get
            {
                switch (Category)
                {
                    case EvidenceCategory.Extension: return "EXTENSÃO";
                    case EvidenceCategory.Signal: return "SINAL";
                    case EvidenceCategory.Recording: return "GRAVAÇÃO";
                    case EvidenceCategory.Anomaly: return "ANOMALIA";
                    case EvidenceCategory.Location: return "LOCAL";
                    default: return "DOCUMENTO";
                }
            }
        }
    }
}
