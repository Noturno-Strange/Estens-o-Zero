using UnityEngine;

namespace ExtensaoZero.Anomaly
{
    public enum TensionCategory
    {
        Presence,
        Contradiction,
        Threat
    }

    public enum TensionEventKind
    {
        LightFlicker,
        PhoneRingOnce,
        DistantSteps,
        Whisper,
        LightSequence,
        PhonesRingTogether,
        SubscriberManifestation,
        RecordingAlteration
    }

    /// <summary>
    /// Evento configurável do Diretor de Tensão. ScriptableObject para conteúdo real;
    /// a fatia usa TensionCatalog em memória.
    /// </summary>
    [CreateAssetMenu(menuName = "Extensao Zero/Tension Event", fileName = "TensionEvent")]
    public sealed class TensionEventDefinition : ScriptableObject
    {
        public string id = "event";
        public TensionCategory category = TensionCategory.Presence;
        public TensionEventKind kind = TensionEventKind.LightFlicker;
        [Range(0f, 1f)] public float minTension = 0f;
        [Range(0f, 1f)] public float maxTension = 1f;
        public float cooldown = 60f;
        public float weight = 1f;
        [Range(0f, 1f)] public float intensity = 0.5f;
        public int minMissionStep = 0;
        [TextArea(1, 3)] public string subtitle = "";

        public static TensionEventDefinition Create(
            string id,
            TensionCategory category,
            TensionEventKind kind,
            float minTension,
            float maxTension,
            float cooldown,
            float weight,
            float intensity,
            int minMissionStep,
            string subtitle)
        {
            var definition = CreateInstance<TensionEventDefinition>();
            definition.name = "Tension_" + id;
            definition.id = id;
            definition.category = category;
            definition.kind = kind;
            definition.minTension = minTension;
            definition.maxTension = maxTension;
            definition.cooldown = cooldown;
            definition.weight = weight;
            definition.intensity = intensity;
            definition.minMissionStep = minMissionStep;
            definition.subtitle = subtitle;
            return definition;
        }
    }
}
