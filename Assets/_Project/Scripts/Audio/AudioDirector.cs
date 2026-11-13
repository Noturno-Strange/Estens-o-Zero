using ExtensaoZero.Core;
using ExtensaoZero.Telephone;
using UnityEngine;

namespace ExtensaoZero.Audio
{
    public enum AudioCategory
    {
        Master,
        Call,
        Sfx,
        Ambience,
        Scare
    }

    /// <summary>
    /// Centraliza reprodução e volumes. Sem dependência de mixer asset: categorias são
    /// multiplicadores explícitos, o que mantém o build reprodutível.
    /// </summary>
    public sealed class AudioDirector : MonoBehaviour
    {
        public static AudioDirector Instance { get; private set; }

        public float MasterVolume { get; private set; } = 0.85f;
        public float CallVolume { get; private set; } = 0.9f;
        public float SfxVolume { get; private set; } = 0.8f;
        public float AmbienceVolume { get; private set; } = 0.6f;
        public float ScareVolume { get; private set; } = 0.7f;

        private AudioSource m_Ambient;
        private AudioSource m_Call;
        private AudioSource m_Scare;
        private AudioSource m_Ui;
        private string m_AmbientZone;

        public static AudioDirector Create(Transform parent)
        {
            var go = new GameObject("AudioDirector");
            if (parent != null) go.transform.SetParent(parent, false);
            var director = go.AddComponent<AudioDirector>();
            director.BuildSources();
            return director;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void BuildSources()
        {
            m_Ambient = CreateSource("Ambient", true, 0f);
            m_Call = CreateSource("Call", false, 0f);
            // Filtro telefônico (passa-banda 300–3400 Hz aproximado) no fone do jogador.
            var low = m_Call.gameObject.AddComponent<AudioLowPassFilter>();
            low.cutoffFrequency = 3400f;
            var high = m_Call.gameObject.AddComponent<AudioHighPassFilter>();
            high.cutoffFrequency = 300f;
            m_Scare = CreateSource("Scare", false, 0f);
            m_Ui = CreateSource("UI", false, 0f);
        }

        private AudioSource CreateSource(string name, bool loop, float spatialBlend)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = spatialBlend;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 1f;
            source.maxDistance = 22f;
            return source;
        }

        public void ApplySettings(GameSettings settings)
        {
            if (settings == null) return;
            MasterVolume = Mathf.Clamp01(settings.masterVolume);
            CallVolume = Mathf.Clamp01(settings.callVolume);
            SfxVolume = Mathf.Clamp01(settings.sfxVolume);
            AmbienceVolume = Mathf.Clamp01(settings.ambienceVolume);
            ScareVolume = Mathf.Clamp01(settings.scareVolume);

            AudioListener.volume = MasterVolume;
            if (m_Ambient != null) m_Ambient.volume = AmbienceVolume;
            if (m_Call != null) m_Call.volume = CallVolume;
            if (m_Scare != null) m_Scare.volume = ScareVolume;
            if (m_Ui != null) m_Ui.volume = SfxVolume;
        }

        public void PlayAmbient(string zone)
        {
            if (m_Ambient == null || string.IsNullOrEmpty(zone)) return;
            if (zone == m_AmbientZone && m_Ambient.isPlaying) return;
            m_AmbientZone = zone;
            m_Ambient.clip = ProceduralAudio.Ambience(zone);
            m_Ambient.volume = AmbienceVolume;
            m_Ambient.Play();
        }

        public void StopAmbient()
        {
            if (m_Ambient != null) m_Ambient.Stop();
            m_AmbientZone = null;
        }

        public void PlayUiClick()
        {
            if (m_Ui == null) return;
            m_Ui.PlayOneShot(ProceduralAudio.UiClick(), SfxVolume);
        }

        public void PlayWorldOneShot(AudioClip clip, Vector3 position, float volume = 1f, AudioCategory category = AudioCategory.Sfx)
        {
            if (clip == null) return;
            var go = new GameObject("OneShot_" + clip.name);
            go.transform.position = position;
            var source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.minDistance = 1f;
            source.maxDistance = 25f;
            source.volume = Mathf.Clamp01(volume) * CategoryVolume(category);
            source.Play();
            Destroy(go, clip.length + 0.1f);
        }

        /// <summary>
        /// Camada da chamada no "fone" do jogador (2D parcial). Gravações reais tocam inteiras
        /// (<paramref name="exclusive"/>); placeholders procedurais são sobrepostos em one-shot.
        /// </summary>
        public void PlayCallLayer(CallLayer layer, AudioClip clip, float volume = 1f, bool exclusive = false)
        {
            if (m_Call == null || clip == null) return;
            m_Call.volume = CallVolume * Mathf.Clamp01(volume);
            if (exclusive)
            {
                m_Call.Stop();
                m_Call.clip = clip;
                m_Call.Play();
                return;
            }

            m_Call.PlayOneShot(clip);
        }

        public void StopCall()
        {
            if (m_Call != null) m_Call.Stop();
        }

        /// <summary>
        /// Sustos: nunca mais de +6 dB acima da cena (AUDIO §4). Com "reduzir intensidade",
        /// o volume cai pela metade.
        /// </summary>
        public void PlayScare(AudioClip clip, float volume = 1f)
        {
            if (m_Scare == null || clip == null) return;
            var settings = GameManager.Instance != null ? GameManager.Instance.Settings : null;
            var reduce = settings != null && settings.reduceScareIntensity ? 0.5f : 1f;
            m_Scare.volume = Mathf.Min(ScareVolume * Mathf.Clamp01(volume) * reduce, Mathf.Max(AmbienceVolume, SfxVolume) * 2f);
            m_Scare.PlayOneShot(clip);
        }

        public void StopAll()
        {
            if (m_Ambient != null) m_Ambient.Stop();
            if (m_Call != null) m_Call.Stop();
            if (m_Scare != null) m_Scare.Stop();
            if (m_Ui != null) m_Ui.Stop();
        }

        private float CategoryVolume(AudioCategory category)
        {
            switch (category)
            {
                case AudioCategory.Call: return CallVolume;
                case AudioCategory.Ambience: return AmbienceVolume;
                case AudioCategory.Scare: return ScareVolume;
                case AudioCategory.Master: return MasterVolume;
                default: return SfxVolume;
            }
        }

        public string AmbientZone => m_AmbientZone;

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
