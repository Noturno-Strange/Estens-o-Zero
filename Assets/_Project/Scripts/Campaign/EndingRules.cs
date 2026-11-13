using System.Collections.Generic;

namespace ExtensaoZero.Campaign
{
    /// <summary>
    /// Requisitos dos três finais (Story Bible §7). Lógica pura sobre flags da campanha.
    /// O Capítulo 5 sempre oferece meios de cumprir cada requisito dentro do próprio capítulo,
    /// para que nenhuma combinação deixe o jogador sem saída.
    /// </summary>
    public static class EndingRules
    {
        public const string FlagFireRecords = "fire_records";
        public const string FlagOriginalAllLayers = "original_all_layers";
        public const string FlagRestoredClean = "restored_clean";
        public const string FlagAnaName = "ana_name";
        public const string FlagAbandoned = "player_abandoned";
        public const int RequiredIsolatedCircuits = 3;

        public static bool IsAvailable(EndingKind ending, ICollection<string> flags, int isolatedCircuits)
        {
            flags ??= new List<string>();
            switch (ending)
            {
                case EndingKind.Isolate:
                    return flags.Contains(FlagFireRecords) && isolatedCircuits >= RequiredIsolatedCircuits;
                case EndingKind.Restore:
                    return flags.Contains(FlagOriginalAllLayers) && flags.Contains(FlagRestoredClean);
                case EndingKind.Answer:
                    return flags.Contains(FlagAnaName) && !flags.Contains(FlagAbandoned);
                default:
                    return false;
            }
        }

        public static string Requirement(EndingKind ending)
        {
            switch (ending)
            {
                case EndingKind.Isolate:
                    return "Encontrar os registros do incêndio e isolar pelo menos três circuitos contaminados.";
                case EndingKind.Restore:
                    return "Escutar as quatro camadas da gravação original e restaurar a linha sem contaminar o tronco.";
                case EndingKind.Answer:
                    return "Descobrir o nome dela e manter a confiança da equipe (ninguém isolado por abandono).";
                default:
                    return string.Empty;
            }
        }

        public static string Title(EndingKind ending)
        {
            switch (ending)
            {
                case EndingKind.Isolate: return "ISOLAR";
                case EndingKind.Restore: return "RESTAURAR";
                case EndingKind.Answer: return "ATENDER";
                default: return "—";
            }
        }

        public static string Epilogue(EndingKind ending)
        {
            switch (ending)
            {
                case EndingKind.Isolate:
                    return "A equipe corta a Extensão Zero e a energia da central. A Assinante se cala. As gravações param. " +
                           "A instalação é abandonada.\n\nMeses depois, em outra cidade, um telefone público isolado toca.";
                case EndingKind.Restore:
                    return "A linha original é reconectada. Ana completa a chamada de 1987. A rede se estabiliza e a Assinante desaparece.\n\n" +
                           "A equipe sai com as gravações. No inventário da estação, alguém anota uma extensão nova: 0.";
                case EndingKind.Answer:
                    return "Um investigador assume a linha. A central aceita a troca. A equipe sai pela recepção.\n\n" +
                           "Semanas depois, os telefones da equipe tocam ao mesmo tempo. A voz do outro lado é conhecida. Ela pede para não desligar.";
                default:
                    return string.Empty;
            }
        }
    }
}
