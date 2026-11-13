using System.Collections.Generic;

namespace ExtensaoZero.Anomaly
{
    /// <summary>
    /// Catálogo da fatia. Cada evento tem pista e reação possível; nenhum é uma morte
    /// arbitrária. Cooldowns longos evitam repetição e fadiga.
    /// </summary>
    public static class TensionCatalog
    {
        private static List<TensionEventDefinition> s_Events;

        public static IReadOnlyList<TensionEventDefinition> Events => s_Events ??= Build();

        private static List<TensionEventDefinition> Build()
        {
            return new List<TensionEventDefinition>
            {
                TensionEventDefinition.Create("light_flicker", TensionCategory.Presence, TensionEventKind.LightFlicker,
                    0.05f, 0.7f, 45f, 1.2f, 0.25f, 0, "As luzes piscam uma vez. Nada muda."),
                TensionEventDefinition.Create("phone_once", TensionCategory.Presence, TensionEventKind.PhoneRingOnce,
                    0.1f, 0.65f, 60f, 1f, 0.3f, 1, "Um toque curto. O telefone não está tocando."),
                TensionEventDefinition.Create("distant_steps", TensionCategory.Presence, TensionEventKind.DistantSteps,
                    0.15f, 0.8f, 70f, 1.1f, 0.35f, 1, "Passos no corredor. Param quando você para."),
                TensionEventDefinition.Create("whisper", TensionCategory.Contradiction, TensionEventKind.Whisper,
                    0.25f, 0.9f, 90f, 0.9f, 0.45f, 2, "Uma voz sussurra atrás da parede. Não há sala ali."),
                TensionEventDefinition.Create("light_sequence", TensionCategory.Contradiction, TensionEventKind.LightSequence,
                    0.35f, 0.95f, 110f, 0.85f, 0.55f, 3, "As luzes apagam em sequência, da central ao corredor."),
                TensionEventDefinition.Create("phones_together", TensionCategory.Threat, TensionEventKind.PhonesRingTogether,
                    0.55f, 1f, 120f, 0.7f, 0.7f, 4, "Todos os telefones tocam ao mesmo tempo."),
                TensionEventDefinition.Create("recording_alter", TensionCategory.Threat, TensionEventKind.RecordingAlteration,
                    0.55f, 1f, 150f, 0.6f, 0.65f, 4, "A gravação repete a sua voz com uma frase a mais."),
                TensionEventDefinition.Create("subscriber_manifest", TensionCategory.Threat, TensionEventKind.SubscriberManifestation,
                    0.6f, 1f, 180f, 0.5f, 0.85f, 5, "Uma silhueta no fim do corredor. Ela não se move.")
            };
        }
    }
}
