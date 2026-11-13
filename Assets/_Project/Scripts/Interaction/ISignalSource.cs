using System.Collections.Generic;
using UnityEngine;

namespace ExtensaoZero.Interaction
{
    /// <summary>Algo que o Receptor portátil detecta: telefones tocando, fitas, circuitos contaminados.</summary>
    public interface ISignalSource
    {
        bool SignalActive { get; }
        Vector3 SignalPosition { get; }
        string SignalLabel { get; }
    }

    public static class SignalRegistry
    {
        private static readonly List<ISignalSource> s_Sources = new List<ISignalSource>();

        public static IReadOnlyList<ISignalSource> Sources => s_Sources;

        public static void Register(ISignalSource source)
        {
            if (source != null && !s_Sources.Contains(source)) s_Sources.Add(source);
        }

        public static void Unregister(ISignalSource source)
        {
            s_Sources.Remove(source);
        }

        public static void Clear()
        {
            s_Sources.Clear();
        }
    }

    /// <summary>Leitura do Medidor de circuito.</summary>
    public enum CircuitReading
    {
        Interrupted,
        Energized,
        Contaminated
    }

    public interface ICircuitReadable
    {
        CircuitReading Reading { get; }
    }
}
