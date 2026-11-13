using UnityEngine;

namespace ExtensaoZero.Telephone
{
    /// <summary>
    /// Cabo visual com curva de Bézier. Sem física: apenas LineRenderer e pontos.
    /// </summary>
    public sealed class CableVisual : MonoBehaviour
    {
        public LineRenderer line;
        public float sag = 0.18f;
        public int segments = 18;

        private Transform m_A;
        private Transform m_B;

        public void SetEndpoints(Transform a, Transform b)
        {
            m_A = a;
            m_B = b;
            if (line != null) line.enabled = a != null && b != null;
        }

        public void Clear()
        {
            m_A = null;
            m_B = null;
            if (line != null) line.enabled = false;
        }

        private void LateUpdate()
        {
            if (line == null || m_A == null || m_B == null) return;
            line.positionCount = segments;
            var start = m_A.position;
            var end = m_B.position;
            var control = (start + end) * 0.5f + Vector3.down * sag;
            for (var i = 0; i < segments; i++)
            {
                var t = i / (float)(segments - 1);
                var point = Bezier(start, control, end, t);
                line.SetPosition(i, point);
            }
        }

        private static Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            var inv = 1f - t;
            return inv * inv * a + 2f * inv * t * b + t * t * c;
        }
    }

    /// <summary>
    /// Item carregável. Não é um MonoBehaviour: pertence ao equipamento do jogador.
    /// </summary>
    public sealed class CableItem
    {
        public string Label { get; }
        public int Length { get; }

        public CableItem(string label, int length = 1)
        {
            Label = string.IsNullOrEmpty(label) ? "CABO" : label;
            Length = Mathf.Max(1, length);
        }

        public override string ToString() => $"{Label} ({Length} m)";
    }
}
