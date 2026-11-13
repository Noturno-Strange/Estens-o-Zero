using System;
using ExtensaoZero.Audio;
using ExtensaoZero.UI;
using UnityEngine;

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
            QualitySettings.SetQualityLevel(Mathf.Clamp(qualityLevel, 0, QualitySettings.names.Length - 1), true);
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

        public void NotifyChanged()
        {
            ApplyAll();
            Save();
            Changed?.Invoke();
        }
    }
}
