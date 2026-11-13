namespace ExtensaoZero.Campaign
{
    /// <summary>
    /// Prólogo + cinco capítulos. Uma instalação dividida em setores; cada capítulo libera
    /// novas áreas (ADR-010). Textos sem spoilers explícitos no briefing.
    /// </summary>
    public static class CampaignCatalog
    {
        public const int ChapterCount = 6;

        private static readonly ChapterInfo[] s_Chapters =
        {
            new ChapterInfo
            {
                Index = 0, Id = "prologue", Title = "PRÓLOGO — A PRIMEIRA LIGAÇÃO", Theme = "Tutorial e primeira chamada",
                Novelty = "Controles, central, camadas",
                Briefing = "Helena: \"A estação Serra do Silêncio voltou a registrar tráfego. Entrem pela recepção, " +
                           "verifiquem a central e anotem tudo. Ninguém deveria estar ligando para lá.\"",
                Recommended = new[] { EquipmentKind.ExtensionMap }
            },
            new ChapterInfo
            {
                Index = 1, Id = "chapter1", Title = "CAPÍTULO 1 — A CENTRAL", Theme = "Extensões inexistentes",
                Novelty = "Rastreamento, contaminação",
                Briefing = "Helena: \"Três extensões que não existem mais estão chamando a central. Rastreiem a origem " +
                           "de cada uma pelo medidor do painel e isolem o que estiver contaminado.\"",
                Recommended = new[] { EquipmentKind.Receiver, EquipmentKind.CircuitMeter }
            },
            new ChapterInfo
            {
                Index = 2, Id = "chapter2", Title = "CAPÍTULO 2 — ARQUIVO DE VOZES", Theme = "Gravações antigas",
                Novelty = "Gravador, comparação de versões",
                Briefing = "Helena: \"Nara encontrou fitas de 1979 e 1987 no arquivo morto. A mesma ligação aparece em " +
                           "versões diferentes. Comparem antes de conectar qualquer coisa.\"",
                Recommended = new[] { EquipmentKind.Recorder, EquipmentKind.ExtensionMap }
            },
            new ChapterInfo
            {
                Index = 3, Id = "chapter3", Title = "CAPÍTULO 3 — A LINHA MORTA", Theme = "Túneis e circuitos",
                Novelty = "Medidor, isolamento, Linha Ocupada",
                Briefing = "Helena: \"O sinal desce para os túneis de cabos. Há caixas de passagem contaminadas. " +
                           "Isolem os circuitos certos; os errados derrubam a linha de vocês.\"",
                Recommended = new[] { EquipmentKind.CircuitMeter, EquipmentKind.Receiver }
            },
            new ChapterInfo
            {
                Index = 4, Id = "chapter4", Title = "CAPÍTULO 4 — O OUTRO LADO", Theme = "Chamadas sobre os investigadores",
                Novelty = "Informação pessoal, contradições",
                Briefing = "Helena: \"As chamadas agora descrevem vocês. Algumas coisas que elas dizem ainda não " +
                           "aconteceram. Decidam juntos em que acreditar.\"",
                Recommended = new[] { EquipmentKind.Recorder, EquipmentKind.CircuitMeter }
            },
            new ChapterInfo
            {
                Index = 5, Id = "chapter5", Title = "CAPÍTULO 5 — EXTENSÃO ZERO", Theme = "Origem da Assinante",
                Novelty = "Manifestação, três finais",
                Briefing = "Helena: \"A linha zero termina na central original, debaixo de tudo. O que vocês fizerem " +
                           "lá não vai poder ser desfeito.\"",
                Recommended = new[] { EquipmentKind.Receiver, EquipmentKind.Recorder }
            }
        };

        public static ChapterInfo Get(int index)
        {
            if (index < 0) index = 0;
            if (index >= s_Chapters.Length) index = s_Chapters.Length - 1;
            return s_Chapters[index];
        }

        /// <summary>Um capítulo está liberado se for o prólogo ou se o anterior foi concluído.</summary>
        public static bool IsUnlocked(int index, System.Collections.Generic.ICollection<int> completed)
        {
            if (index <= 0) return true;
            if (index >= ChapterCount) return false;
            return completed != null && completed.Contains(index - 1);
        }

        /// <summary>Próximo capítulo a jogar: o primeiro ainda não concluído.</summary>
        public static int NextChapter(System.Collections.Generic.ICollection<int> completed)
        {
            for (var i = 0; i < ChapterCount; i++)
            {
                if (completed == null || !completed.Contains(i)) return i;
            }

            return ChapterCount - 1;
        }
    }
}
