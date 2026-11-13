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
    /// a fatia cria instâncias em memória via CallCatalog.
    /// </summary>
    [CreateAssetMenu(menuName = "Extensao Zero/Call Definition", fileName = "CallDefinition")]
    public sealed class CallDefinition : ScriptableObject
    {
        public string id = "call";
        public string title = "CHAMADA";
        [TextArea(2, 5)] public string voice = "";
        [TextArea(2, 5)] public string ambience = "";
        [TextArea(2, 5)] public string signals = "";
        [TextArea(2, 5)] public string interference = "";
        public float duration = 26f;
        public string dtmf = "";
        public int targetExtension = 214;
        public bool isPlaceholder = true;

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

        public string GetLayerLabel(CallLayer layer)
        {
            switch (layer)
            {
                case CallLayer.Ambience: return "AMBIENTE";
                case CallLayer.Signals: return "SINAIS";
                case CallLayer.Interference: return "INTERFERÊNCIA";
                default: return "VOZ";
            }
        }
    }
}
