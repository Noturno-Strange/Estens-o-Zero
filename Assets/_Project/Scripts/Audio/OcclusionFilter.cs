using UnityEngine;

namespace ExtensaoZero.Audio
{
    /// <summary>
    /// Atenua fontes distantes atrás de geometria. Raycast periódico (não por frame)
    /// para manter o custo baixo em 4 GB de VRAM / 60 FPS.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class OcclusionFilter : MonoBehaviour
    {
        public Transform listener;
        public LayerMask occlusionMask = ~0;
        public float updateInterval = 0.2f;
        [Range(0.05f, 1f)] public float occludedCutoff = 0.22f;

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
            var cutoff = blocked && hit.transform != transform ? 900f * Mathf.Clamp(occludedCutoff, 0.05f, 1f) : 22000f;
            if (m_Filter != null) m_Filter.cutoffFrequency = Mathf.Lerp(m_Filter.cutoffFrequency, cutoff, 0.4f);
            if (m_Source != null) m_Source.volume = blocked ? 0.55f : 1f;
        }
    }
}
