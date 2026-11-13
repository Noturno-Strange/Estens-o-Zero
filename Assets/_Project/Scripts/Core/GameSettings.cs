using System;
using ExtensaoZero.Audio;
using ExtensaoZero.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ExtensaoZero.Core
{
    /// <summary>
    /// Configurações locais do jogador. Persistidas em PlayerPrefs e aplicadas em runtime.
    /// Não contém dados pessoais nem histórico de rede.
    /// </summary>
    public sealed class GameSettings : MonoBehaviour
    {
        public event Action Changed;

        [Header("Áudio")]
        [Range(0f, 1f)] public float masterVolume = 0.85f;
        [Range(0f, 1f)] public float callVolume = 0.9f;
        [Range(0f, 1f)] public float sfxVolume = 0.8f;
        [Range(0f, 1f)] public float ambienceVolume = 0.6f;
        [Range(0f, 1f)] public float scareVolume = 0.7f;

        [Header("Controles")]
        [Range(0.3f, 4f)] public float mouseSensitivity = 1.4f;
        [Range(60f, 100f)] public float fieldOfView = 72f;
        public bool invertY = false;

        [Header("Acessibilidade")]
        public bool subtitles = true;
        [Range(0.7f, 1.6f)] public float subtitleScale = 1f;
        public bool reduceFlashes = false;
        public bool reduceScareIntensity = false;

        [Header("Vídeo")]
        public int qualityLevel = 1;
        public bool fullscreen = true;
        public int resolutionWidth = 1920;
        public int resolutionHeight = 1080;
        public int targetFrameRate = 60;

        private const string Prefix = "EZ_";

        /// <summary>Níveis de qualidade do jogo (PERFORMANCE_TARGETS §4).</summary>
        public static readonly string[] QualityLabels = { "BAIXA", "MÉDIA", "ALTA" };
        private static readonly string[] UnityQualityNames = { "Low", "Medium", "High" };
        private static UniversalRenderPipelineAsset s_RuntimePipeline;

        public static GameSettings Create(Transform parent)
        {
            var go = new GameObject("GameSettings");
            if (parent != null) go.transform.SetParent(parent, false);
            var settings = go.AddComponent<GameSettings>();
            settings.Load();
            return settings;
        }

        public void Load()
        {
            masterVolume = PlayerPrefs.GetFloat(Prefix + "master", masterVolume);
            callVolume = PlayerPrefs.GetFloat(Prefix + "call", callVolume);
            sfxVolume = PlayerPrefs.GetFloat(Prefix + "sfx", sfxVolume);
            ambienceVolume = PlayerPrefs.GetFloat(Prefix + "amb", ambienceVolume);
            scareVolume = PlayerPrefs.GetFloat(Prefix + "scare", scareVolume);
            mouseSensitivity = PlayerPrefs.GetFloat(Prefix + "sens", mouseSensitivity);
            fieldOfView = PlayerPrefs.GetFloat(Prefix + "fov", fieldOfView);
            invertY = PlayerPrefs.GetInt(Prefix + "invert", invertY ? 1 : 0) == 1;
            subtitles = PlayerPrefs.GetInt(Prefix + "subs", subtitles ? 1 : 0) == 1;
            subtitleScale = PlayerPrefs.GetFloat(Prefix + "subscale", subtitleScale);
            reduceFlashes = PlayerPrefs.GetInt(Prefix + "reduceflash", reduceFlashes ? 1 : 0) == 1;
            reduceScareIntensity = PlayerPrefs.GetInt(Prefix + "reducescare", reduceScareIntensity ? 1 : 0) == 1;
            qualityLevel = PlayerPrefs.GetInt(Prefix + "quality", qualityLevel);
            fullscreen = PlayerPrefs.GetInt(Prefix + "fullscreen", fullscreen ? 1 : 0) == 1;
            resolutionWidth = PlayerPrefs.GetInt(Prefix + "resw", resolutionWidth);
            resolutionHeight = PlayerPrefs.GetInt(Prefix + "resh", resolutionHeight);
            targetFrameRate = PlayerPrefs.GetInt(Prefix + "fps", targetFrameRate);
        }

        public void Save()
        {
            PlayerPrefs.SetFloat(Prefix + "master", masterVolume);
            PlayerPrefs.SetFloat(Prefix + "call", callVolume);
            PlayerPrefs.SetFloat(Prefix + "sfx", sfxVolume);
            PlayerPrefs.SetFloat(Prefix + "amb", ambienceVolume);
            PlayerPrefs.SetFloat(Prefix + "scare", scareVolume);
            PlayerPrefs.SetFloat(Prefix + "sens", mouseSensitivity);
            PlayerPrefs.SetFloat(Prefix + "fov", fieldOfView);
            PlayerPrefs.SetInt(Prefix + "invert", invertY ? 1 : 0);
            PlayerPrefs.SetInt(Prefix + "subs", subtitles ? 1 : 0);
            PlayerPrefs.SetFloat(Prefix + "subscale", subtitleScale);
            PlayerPrefs.SetInt(Prefix + "reduceflash", reduceFlashes ? 1 : 0);
            PlayerPrefs.SetInt(Prefix + "reducescare", reduceScareIntensity ? 1 : 0);
            PlayerPrefs.SetInt(Prefix + "quality", qualityLevel);
            PlayerPrefs.SetInt(Prefix + "fullscreen", fullscreen ? 1 : 0);
            PlayerPrefs.SetInt(Prefix + "resw", resolutionWidth);
            PlayerPrefs.SetInt(Prefix + "resh", resolutionHeight);
            PlayerPrefs.SetInt(Prefix + "fps", targetFrameRate);
            PlayerPrefs.Save();
        }

        public void ApplyAll()
        {
            AudioListener.volume = Mathf.Clamp01(masterVolume);
            ApplyQuality();
            Application.targetFrameRate = Mathf.Clamp(targetFrameRate, 30, 240);

            if (Screen.width != resolutionWidth || Screen.height != resolutionHeight || Screen.fullScreen != fullscreen)
            {
                Screen.SetResolution(resolutionWidth, resolutionHeight, fullscreen);
            }

            var player = GameManager.Instance != null ? GameManager.Instance.LocalPlayerController : null;
            if (player != null)
            {
                player.SetFieldOfView(fieldOfView);
                player.SetSensitivity(mouseSensitivity);
                player.SetInvertY(invertY);
            }

            var audio = AudioDirector.Instance;
            if (audio != null) audio.ApplySettings(this);

            var ui = UIManager.Instance;
            if (ui != null) ui.ApplyAccessibility(this);

            Changed?.Invoke();
        }

        /// <summary>
        /// Baixa: render scale 0,8, 4 luzes adicionais, sombras curtas. Média: 1,0 / 6. Alta: 1,0 / 8.
        /// Usa uma cópia em runtime do asset URP para não alterar o asset do projeto.
        /// </summary>
        private void ApplyQuality()
        {
            qualityLevel = Mathf.Clamp(qualityLevel, 0, QualityLabels.Length - 1);
            var index = System.Array.IndexOf(QualitySettings.names, UnityQualityNames[qualityLevel]);
            if (index >= 0) QualitySettings.SetQualityLevel(index, true);

            var source = s_RuntimePipeline != null
                ? s_RuntimePipeline
                : (GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset) ?? (GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset);
            if (source == null) return;
            if (s_RuntimePipeline == null)
            {
                s_RuntimePipeline = Instantiate(source);
                s_RuntimePipeline.name = "EZ_URP_Runtime";
            }

            s_RuntimePipeline.renderScale = qualityLevel == 0 ? 0.8f : 1f;
            s_RuntimePipeline.maxAdditionalLightsCount = qualityLevel == 0 ? 4 : qualityLevel == 1 ? 6 : 8;
            s_RuntimePipeline.shadowDistance = qualityLevel == 0 ? 14f : qualityLevel == 1 ? 22f : 26f;
            QualitySettings.renderPipeline = s_RuntimePipeline;
        }

        public void NotifyChanged()
        {
            ApplyAll();
            Save();
            Changed?.Invoke();
        }
    }
}
