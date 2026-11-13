using System.Collections.Generic;

namespace ExtensaoZero.Anomaly
{
    /// <summary>
    /// Catálogo de eventos do Diretor de Tensão (GDD §8). Cada evento tem pista e reação
    /// possível; nenhum é uma morte arbitrária. Cooldowns longos evitam repetição e fadiga.
    /// A etapa mínima usa a fase da missão (0–7), comum a todos os capítulos.
    /// </summary>
    public static class TensionCatalog
    {
        private static List<TensionEventDefinition> s_Events;

        public static IReadOnlyList<TensionEventDefinition> Events => s_Events ??= Build();

        public static TensionEventDefinition Find(string id)
        {
            var events = Events;
            for (var i = 0; i < events.Count; i++)
            {
                if (events[i].id == id) return events[i];
            }

            return null;
        }

        private static List<TensionEventDefinition> Build()
        {
            return new List<TensionEventDefinition>
            {
                // A — Presença
                TensionEventDefinition.Create("light_flicker", TensionCategory.Presence, TensionEventKind.LightFlicker,
                    0.05f, 0.7f, 45f, 1.2f, 0.25f, 0, "As luzes piscam uma vez. Nada muda."),
                TensionEventDefinition.Create("phone_once", TensionCategory.Presence, TensionEventKind.PhoneRingOnce,
                    0.1f, 0.65f, 60f, 1f, 0.3f, 1, "Um toque curto. Nenhum telefone está tocando."),
                TensionEventDefinition.Create("distant_steps", TensionCategory.Presence, TensionEventKind.DistantSteps,
                    0.15f, 0.8f, 70f, 1.1f, 0.35f, 1, "Passos distantes. Param quando você para."),
                TensionEventDefinition.Create("led_disconnected", TensionCategory.Presence, TensionEventKind.LedDisconnected,
                    0.1f, 0.75f, 80f, 0.9f, 0.25f, 1, "Um LED do painel acende numa extensão sem cabo."),

                // B — Contradição
                TensionEventDefinition.Create("whisper", TensionCategory.Contradiction, TensionEventKind.Whisper,
                    0.25f, 0.9f, 90f, 0.9f, 0.45f, 2, "Uma voz sussurra atrás da parede. Não há sala ali."),
                TensionEventDefinition.Create("light_sequence", TensionCategory.Contradiction, TensionEventKind.LightSequence,
                    0.35f, 0.95f, 110f, 0.85f, 0.55f, 3, "As luzes apagam em sequência, de uma ponta à outra da sala."),
                TensionEventDefinition.Create("knocks_divergent", TensionCategory.Contradiction, TensionEventKind.KnocksDivergent,
                    0.3f, 0.9f, 120f, 0.9f, 0.45f, 2, ""),
                TensionEventDefinition.Create("phone_no_line", TensionCategory.Contradiction, TensionEventKind.PhoneDisconnectedRing,
                    0.3f, 0.95f, 100f, 0.8f, 0.5f, 2, "Um telefone sem linha toca. A extensão dele não está conectada."),
                TensionEventDefinition.Create("steps_mirror", TensionCategory.Contradiction, TensionEventKind.StepsMirror,
                    0.3f, 0.9f, 130f, 0.8f, 0.45f, 2, "Passos acompanham os seus. Quando você para, eles dão mais um."),

                // C — Ameaça
                TensionEventDefinition.Create("phones_together", TensionCategory.Threat, TensionEventKind.PhonesRingTogether,
                    0.55f, 1f, 120f, 0.7f, 0.7f, 4, "Todos os telefones tocam ao mesmo tempo."),
                TensionEventDefinition.Create("recording_alter", TensionCategory.Threat, TensionEventKind.RecordingAlteration,
                    0.55f, 1f, 150f, 0.6f, 0.65f, 4, "Uma gravação antiga repete a última frase anotada no caderno. Há uma palavra a mais."),
                TensionEventDefinition.Create("private_call", TensionCategory.Threat, TensionEventKind.PrivateCall,
                    0.6f, 1f, 160f, 0.55f, 0.6f, 4, ""),
                TensionEventDefinition.Create("subscriber_manifest", TensionCategory.Threat, TensionEventKind.SubscriberManifestation,
                    0.6f, 1f, 180f, 0.5f, 0.85f, 5, "")
            };
        }
    }
}
