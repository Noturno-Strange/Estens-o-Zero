using ExtensaoZero.Anomaly;
using ExtensaoZero.Telephone;

namespace ExtensaoZero.Player
{
    /// <summary>
    /// Estado local do sintonizador: camada atual (entre as atribuídas ao jogador) e a
    /// frequência usada para sair da Linha Ocupada.
    /// </summary>
    public sealed class Tuner
    {
        public CallLayer CurrentLayer { get; private set; } = CallLayer.Voice;
        public int Frequency { get; private set; }

        public string Label
        {
            get
            {
                switch (CurrentLayer)
                {
                    case CallLayer.Ambience: return "AMBIENTE";
                    case CallLayer.Signals: return "SINAIS";
                    case CallLayer.Interference: return "INTERFERÊNCIA";
                    default: return "VOZ";
                }
            }
        }

        public void SetLayer(CallLayer layer)
        {
            CurrentLayer = layer;
        }

        public void Cycle()
        {
            CurrentLayer = (CallLayer)(((int)CurrentLayer + 1) % 4);
        }

        public void StepFrequency(int delta)
        {
            Frequency = LineBusyRules.Step(Frequency, delta);
        }

        public void ResetFrequency()
        {
            Frequency = 0;
        }
    }
}
