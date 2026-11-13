using ExtensaoZero.Telephone;

namespace ExtensaoZero.Player
{
    /// <summary>
    /// Gravador analógico: uma gravação por vez (GDD §6). Guarda o texto da camada ouvida
    /// (e o clip, quando existir) para reprodução e comparação de versões.
    /// </summary>
    public sealed class Recorder
    {
        public bool HasRecording => !string.IsNullOrEmpty(CallId);
        public string CallId { get; private set; }
        public string Group { get; private set; }
        public string Title { get; private set; }
        public CallLayer Layer { get; private set; }
        public string Text { get; private set; }

        public void Record(CallDefinition call, CallLayer layer)
        {
            if (call == null) return;
            CallId = call.id;
            Group = call.group;
            Title = call.title;
            Layer = layer;
            Text = call.GetText(layer);
        }

        /// <summary>Verdadeiro quando a gravação guardada é outra versão (mesmo grupo) da chamada dada.</summary>
        public bool IsOtherVersionOf(CallDefinition call)
        {
            return call != null && HasRecording && !string.IsNullOrEmpty(Group) && Group == call.group && CallId != call.id;
        }

        public void Clear()
        {
            CallId = null;
            Group = null;
            Title = null;
            Text = null;
        }
    }
}
