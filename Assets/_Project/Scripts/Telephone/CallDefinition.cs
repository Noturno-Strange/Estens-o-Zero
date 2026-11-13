using UnityEngine;

namespace ExtensaoZero.Telephone
{
    public enum CallLayer
    {
        Voice = 0,
        Ambience = 1,
        Signals = 2,
        Interference = 3
    }

    /// <summary>
    /// Conteúdo de uma chamada. ScriptableObject para permitir conteúdo real em produção;
    /// o jogo cria instâncias em memória via CallCatalog. Quando um AudioClip gravado é
    /// atribuído a uma camada, ele substitui o placeholder procedural daquela camada.
    /// </summary>
    [CreateAssetMenu(menuName = "Extensao Zero/Call Definition", fileName = "CallDefinition")]
    public sealed class CallDefinition : ScriptableObject
    {
        public string id = "call";
        public string title = "CHAMADA";
        [Tooltip("Chamadas do mesmo grupo podem ser comparadas pelo gravador (versões da mesma gravação).")]
        public string group = "";
        public string voiceSpeaker = "";
        [TextArea(2, 5)] public string voice = "";
        [TextArea(2, 5)] public string ambience = "";
        [TextArea(2, 5)] public string signals = "";
        [TextArea(2, 5)] public string interference = "";
        public float duration = 26f;
        public string dtmf = "";
        public int targetExtension = 214;
        public bool isPlaceholder = true;

        [Header("Gravações (opcionais)")]
        public AudioClip voiceClip;
        public AudioClip ambienceClip;
        public AudioClip signalsClip;
        public AudioClip interferenceClip;

        public string GetText(CallLayer layer)
        {
            switch (layer)
            {
                case CallLayer.Ambience: return ambience;
                case CallLayer.Signals: return signals;
                case CallLayer.Interference: return interference;
                default: return voice;
            }
        }

        public AudioClip GetClip(CallLayer layer)
        {
            switch (layer)
            {
                case CallLayer.Ambience: return ambienceClip;
                case CallLayer.Signals: return signalsClip;
                case CallLayer.Interference: return interferenceClip;
                default: return voiceClip;
            }
        }

        public string GetLayerLabel(CallLayer layer)
        {
            switch (layer)
            {
                case CallLayer.Ambience: return "AMBIENTE";
                case CallLayer.Signals: return "SINAIS";
                case CallLayer.Interference: return "INTERFERÊNCIA";
                default: return string.IsNullOrEmpty(voiceSpeaker) ? "VOZ" : "VOZ — " + voiceSpeaker;
            }
        }
    }
}
