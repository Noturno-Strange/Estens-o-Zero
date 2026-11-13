namespace ExtensaoZero.Campaign
{
    /// <summary>
    /// Parâmetros de dificuldade (GDD §11). A dificuldade ajusta tempo, pistas, interferência
    /// e frequência da entidade — nunca "vida" de inimigos. Lógica pura, testável sem cena.
    /// </summary>
    public sealed class DifficultyProfile
    {
        public DifficultyLevel Level { get; private set; }
        public string Label { get; private set; }
        public string Description { get; private set; }

        /// <summary>Multiplicador da duração das chamadas (tempo para ouvir).</summary>
        public float CallDurationMultiplier { get; private set; }
        /// <summary>Multiplicador do cooldown global do Diretor de Tensão.</summary>
        public float TensionCooldownMultiplier { get; private set; }
        /// <summary>Multiplicador do peso de eventos de ameaça.</summary>
        public float ThreatWeightMultiplier { get; private set; }
        /// <summary>Multiplicador da contaminação causada por erros.</summary>
        public float ContaminationMultiplier { get; private set; }
        /// <summary>Duração da Linha Ocupada, em segundos (sempre dentro de 15–25 s).</summary>
        public float LineBusySeconds { get; private set; }
        /// <summary>Exibe dicas redundantes adicionais nos objetivos.</summary>
        public bool ExtraHints { get; private set; }

        public static DifficultyProfile Get(DifficultyLevel level)
        {
            switch (level)
            {
                case DifficultyLevel.Investigation:
                    return new DifficultyProfile
                    {
                        Level = level,
                        Label = "INVESTIGAÇÃO",
                        Description = "Mais tempo, pistas redundantes, interferência baixa, entidade rara.",
                        CallDurationMultiplier = 1.35f,
                        TensionCooldownMultiplier = 1.5f,
                        ThreatWeightMultiplier = 0.6f,
                        ContaminationMultiplier = 0.75f,
                        LineBusySeconds = 15f,
                        ExtraHints = true
                    };
                case DifficultyLevel.Rupture:
                    return new DifficultyProfile
                    {
                        Level = level,
                        Label = "RUPTURA",
                        Description = "Pouco tempo, pistas escassas, interferência alta, entidade frequente e controlada.",
                        CallDurationMultiplier = 0.85f,
                        TensionCooldownMultiplier = 0.8f,
                        ThreatWeightMultiplier = 1.4f,
                        ContaminationMultiplier = 1.25f,
                        LineBusySeconds = 25f,
                        ExtraHints = false
                    };
                default:
                    return new DifficultyProfile
                    {
                        Level = DifficultyLevel.Operation,
                        Label = "OPERAÇÃO",
                        Description = "Tempo padrão, pistas equilibradas, entidade presente.",
                        CallDurationMultiplier = 1f,
                        TensionCooldownMultiplier = 1f,
                        ThreatWeightMultiplier = 1f,
                        ContaminationMultiplier = 1f,
                        LineBusySeconds = 20f,
                        ExtraHints = false
                    };
            }
        }

        /// <summary>
        /// Ajuste pela quantidade de jogadores (GDD §12.1): em solo, mais tempo e pistas.
        /// </summary>
        public float CallDurationFor(float baseDuration, int playerCount)
        {
            var solo = playerCount <= 1 ? 1.15f : 1f;
            return baseDuration * CallDurationMultiplier * solo;
        }

        public bool HintsFor(int playerCount)
        {
            return ExtraHints || playerCount <= 1;
        }
    }
}
