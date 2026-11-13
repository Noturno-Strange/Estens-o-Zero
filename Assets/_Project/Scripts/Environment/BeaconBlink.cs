using UnityEngine;

namespace ExtensaoZero.Environment
{
    /// <summary>Baliza de topo da torre: pisca devagar (respeita "reduzir flashes").</summary>
    public sealed class BeaconBlink : MonoBehaviour
    {
        public Light target;
        public float period = 2.2f;
        private Renderer m_Renderer;

        private void Awake()
        {
            m_Renderer = GetComponent<Renderer>();
        }

        private void Update()
        {
            var settings = Core.GameManager.Instance != null ? Core.GameManager.Instance.Settings : null;
            var reduce = settings != null && settings.reduceFlashes;
            var phase = Time.time % period / period;
            var on = reduce || phase < 0.45f;
            if (target != null) target.intensity = reduce ? 1.2f : on ? 2.2f : 0.05f;
            if (m_Renderer != null) m_Renderer.enabled = on;
        }
    }
}
