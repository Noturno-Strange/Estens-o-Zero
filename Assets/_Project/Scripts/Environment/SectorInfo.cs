using System.Collections.Generic;
using UnityEngine;

namespace ExtensaoZero.Environment
{
    /// <summary>Setor da instalação: limites, luzes, ambiência e vizinhos (culling simples por setor).</summary>
    public sealed class SectorInfo
    {
        public string Id;
        public string Label;
        public Bounds Bounds;
        public string Ambience;
        public readonly List<Light> Lights = new List<Light>();
        public string[] Neighbors = new string[0];
        public bool Discovered;
        public Vector2 MapPosition;
    }
}
