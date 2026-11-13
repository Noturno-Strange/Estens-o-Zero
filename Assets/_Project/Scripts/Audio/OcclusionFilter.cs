using UnityEngine;

namespace ExtensaoZero.Audio
{
    /// <summary>
    /// Oclusão simplificada (AUDIO_DIRECTION §5): raycast do ouvinte até a fonte a cada 0,2 s;
    /// sem linha de visão, passa-baixa em 1,2 kHz; com linha de visão, 22 kHz. Só mexe no
    /// filtro — o volume continua sendo de quem toca o som.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class OcclusionFilter : MonoBehaviour
    {
        public Transform listener;
        public LayerMask occlusionMask = ~0;
        public float updateInterval = 0.2f;
        public float occludedCutoff = 1200f;

        private AudioSource m_Source;
        private AudioLowPassFilter m_Filter;
        private float m_NextUpdate;

        private void Awake()
        {
            m_Source = GetComponent<AudioSource>();
            m_Filter = GetComponent<AudioLowPassFilter>();
            if (m_Filter == null) m_Filter = gameObject.AddComponent<AudioLowPassFilter>();
            m_Filter.cutoffFrequency = 22000f;
        }

        private void Update()
        {
            if (Time.time < m_NextUpdate) return;
            m_NextUpdate = Time.time + updateInterval;

            if (listener == null)
            {
                var camera = Camera.main;
                if (camera == null) return;
                listener = camera.transform;
            }

            var origin = listener.position;
            var target = transform.position;
            var blocked = Physics.Linecast(origin, target, out var hit, occlusionMask, QueryTriggerInteraction.Ignore);
            var occluded = blocked && hit.transform != transform && !hit.transform.IsChildOf(transform);
            var cutoff = occluded ? occludedCutoff : 22000f;
            if (m_Filter != null) m_Filter.cutoffFrequency = Mathf.Lerp(m_Filter.cutoffFrequency, cutoff, 0.4f);
        }
    }
}
