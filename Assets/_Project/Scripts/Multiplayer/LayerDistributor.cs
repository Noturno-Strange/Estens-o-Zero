using System;

namespace ExtensaoZero.Multiplayer
{
    /// <summary>
    /// Distribuição determinística das quatro camadas de áudio. Regra de ouro:
    /// nenhum puzzle exige quatro pessoas; em solo todas as camadas ficam disponíveis.
    /// </summary>
    public static class LayerDistributor
    {
        public static readonly CallLayerRef[] All =
        {
            CallLayerRef.Voice,
            CallLayerRef.Ambience,
            CallLayerRef.Signals,
            CallLayerRef.Interference
        };

        public enum CallLayerRef
        {
            Voice = 0,
            Ambience = 1,
            Signals = 2,
            Interference = 3
        }

        public static Telephone.CallLayer[] Distribute(int playerCount, int playerIndex)
        {
            playerCount = Math.Max(1, Math.Min(4, playerCount));
            playerIndex = Math.Max(0, Math.Min(playerCount - 1, playerIndex));

            switch (playerCount)
            {
                case 1:
                    return new[]
                    {
                        Telephone.CallLayer.Voice,
                        Telephone.CallLayer.Ambience,
                        Telephone.CallLayer.Signals,
                        Telephone.CallLayer.Interference
                    };
                case 2:
                    return playerIndex == 0
                        ? new[] { Telephone.CallLayer.Voice, Telephone.CallLayer.Ambience }
                        : new[] { Telephone.CallLayer.Signals, Telephone.CallLayer.Interference };
                case 3:
                    switch (playerIndex)
                    {
                        case 0: return new[] { Telephone.CallLayer.Voice, Telephone.CallLayer.Ambience };
                        case 1: return new[] { Telephone.CallLayer.Signals };
                        default: return new[] { Telephone.CallLayer.Interference };
                    }
                default:
                    return new[] { (Telephone.CallLayer)playerIndex };
            }
        }

        public static string Label(Telephone.CallLayer layer)
        {
            switch (layer)
            {
                case Telephone.CallLayer.Ambience: return "AMBIENTE";
                case Telephone.CallLayer.Signals: return "SINAIS";
                case Telephone.CallLayer.Interference: return "INTERFERÊNCIA";
                default: return "VOZ";
            }
        }
    }
}
