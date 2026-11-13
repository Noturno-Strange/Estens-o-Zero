using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensaoZero.Audio
{
    /// <summary>
    /// Clips gerados em runtime. São placeholders declarados como tal: nenhuma voz humana
    /// foi gravada. A arquitetura aceita AudioClips reais sem mudar as chamadas.
    /// </summary>
    public static class ProceduralAudio
    {
        private const int SampleRate = 44100;
        private static readonly Dictionary<string, AudioClip> s_Cache = new Dictionary<string, AudioClip>();

        public static AudioClip RelayClick()
        {
            return Get("relay_click", 0.12f, t =>
            {
                var env = Mathf.Exp(-t * 55f);
                var noise = Noise(t) * 0.55f;
                var click = Mathf.Sin(2f * Mathf.PI * 1800f * t) * 0.5f;
                return (noise + click) * env;
            });
        }

        public static AudioClip DoorCreak()
        {
            return Get("door_creak", 1.15f, t =>
            {
                var env = Mathf.Clamp01(t * 6f) * Mathf.Clamp01((1.15f - t) * 2.2f);
                var freq = 90f + 45f * Mathf.Sin(t * 5.2f) + 18f * Mathf.Sin(t * 17f);
                var tone = Mathf.Sin(2f * Mathf.PI * freq * t);
                var grit = Noise(t) * 0.16f * (0.5f + 0.5f * Mathf.Sin(t * 61f));
                return (tone * 0.32f + grit) * env;
            });
        }

        public static AudioClip Ring()
        {
            return Get("ring", 2.2f, t =>
            {
                var cycle = t % 1.1f;
                if (cycle > 0.85f) return 0f;
                var env = Mathf.Clamp01(cycle * 30f) * Mathf.Clamp01((0.85f - cycle) * 20f);
                var a = Mathf.Sin(2f * Mathf.PI * 440f * t);
                var b = Mathf.Sin(2f * Mathf.PI * 480f * t);
                var trem = 0.75f + 0.25f * Mathf.Sin(2f * Mathf.PI * 20f * t);
                return (a + b) * 0.18f * env * trem;
            });
        }

        public static AudioClip Dtmf(int digit)
        {
            var key = "dtmf_" + digit;
            return Get(key, 0.16f, t =>
            {
                var (low, high) = DtmfPair(digit);
                var env = Mathf.Clamp01(t * 60f) * Mathf.Clamp01((0.16f - t) * 60f);
                var a = Mathf.Sin(2f * Mathf.PI * low * t);
                var b = Mathf.Sin(2f * Mathf.PI * high * t);
                return (a + b) * 0.22f * env;
            });
        }

        public static AudioClip Static()
        {
            return Get("static", 1.4f, t =>
            {
                var env = Mathf.Clamp01(t * 8f) * Mathf.Clamp01((1.4f - t) * 4f);
                return Noise(t) * 0.22f * env;
            });
        }

        public static AudioClip Hum()
        {
            return Get("hum", 3f, t =>
            {
                var env = Mathf.Clamp01(t * 2f) * Mathf.Clamp01((3f - t) * 2f);
                var a = Mathf.Sin(2f * Mathf.PI * 60f * t) * 0.12f;
                var b = Mathf.Sin(2f * Mathf.PI * 120f * t) * 0.06f;
                var c = Mathf.Sin(2f * Mathf.PI * 181f * t) * 0.03f;
                return (a + b + c) * env;
            });
        }

        public static AudioClip Whisper()
        {
            return Get("whisper", 2.6f, t =>
            {
                var env = Mathf.Clamp01(t * 2.5f) * Mathf.Clamp01((2.6f - t) * 1.6f);
                var formant = Mathf.Sin(2f * Mathf.PI * (420f + 120f * Mathf.Sin(t * 3.1f)) * t) * 0.12f;
                var breath = Noise(t) * 0.10f * (0.35f + 0.65f * Mathf.Abs(Mathf.Sin(t * 9f)));
                return (formant + breath) * env;
            });
        }

        public static AudioClip Footstep()
        {
            return Get("footstep", 0.18f, t =>
            {
                var env = Mathf.Exp(-t * 32f);
                var thump = Mathf.Sin(2f * Mathf.PI * (95f - 40f * t) * t) * 0.45f;
                return (thump + Noise(t) * 0.2f) * env;
            });
        }

        public static AudioClip Impact()
        {
            return Get("impact", 0.5f, t =>
            {
                var env = Mathf.Exp(-t * 12f);
                var low = Mathf.Sin(2f * Mathf.PI * 48f * t) * 0.55f;
                return (low + Noise(t) * 0.25f) * env;
            });
        }

        public static AudioClip Beep(float frequency = 880f, float duration = 0.12f)
        {
            var key = "beep_" + Mathf.RoundToInt(frequency);
            return Get(key, duration, t =>
            {
                var env = Mathf.Clamp01(t * 40f) * Mathf.Clamp01((duration - t) * 40f);
                return Mathf.Sin(2f * Mathf.PI * frequency * t) * 0.2f * env;
            });
        }

        public static AudioClip UiClick()
        {
            return Get("ui_click", 0.07f, t =>
            {
                var env = Mathf.Exp(-t * 90f);
                return Mathf.Sin(2f * Mathf.PI * 1200f * t) * 0.18f * env;
            });
        }

        /// <summary>
        /// "Voz" placeholder: formantes sintéticos, deliberadamente não inteligíveis.
        /// Legendas carregam a informação real.
        /// </summary>
        public static AudioClip VoicePlaceholder(int seed)
        {
            var key = "voice_" + seed;
            var random = new System.Random(seed);
            var baseFreq = 105f + (float)random.NextDouble() * 45f;
            return Get(key, 2.4f, t =>
            {
                var env = Mathf.Clamp01(t * 4f) * Mathf.Clamp01((2.4f - t) * 2f);
                var syllable = 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(t * 7.4f + seed));
                var f1 = Mathf.Sin(2f * Mathf.PI * baseFreq * t);
                var f2 = Mathf.Sin(2f * Mathf.PI * baseFreq * 2.4f * t) * 0.5f;
                var f3 = Mathf.Sin(2f * Mathf.PI * baseFreq * 3.7f * t) * 0.25f;
                var breath = Noise(t) * 0.05f;
                return (f1 + f2 + f3) * 0.16f * env * syllable + breath;
            });
        }

        public static AudioClip Ambience(string zone)
        {
            var seed = string.IsNullOrEmpty(zone) ? 0 : zone.GetHashCode();
            var key = "amb_" + seed;
            return Get(key, 6f, t =>
            {
                var env = Mathf.Clamp01(t * 0.8f) * Mathf.Clamp01((6f - t) * 0.8f);
                var low = Mathf.Sin(2f * Mathf.PI * 42f * t) * 0.05f;
                var mid = Mathf.Sin(2f * Mathf.PI * 83f * t) * 0.02f;
                var air = Noise(t) * 0.018f;
                var pulse = 0.85f + 0.15f * Mathf.Sin(t * 0.9f);
                return (low + mid + air) * env * pulse;
            });
        }

        private static (float low, float high) DtmfPair(int digit)
        {
            switch (digit)
            {
                case 1: return (697f, 1209f);
                case 2: return (697f, 1336f);
                case 3: return (697f, 1477f);
                case 4: return (770f, 1209f);
                case 5: return (770f, 1336f);
                case 6: return (770f, 1477f);
                case 7: return (852f, 1209f);
                case 8: return (852f, 1336f);
                case 9: return (852f, 1477f);
                case 0: return (941f, 1336f);
                default: return (770f, 1336f);
            }
        }

        private static AudioClip Get(string key, float duration, Func<float, float> generator)
        {
            if (s_Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var samples = Mathf.Max(1, Mathf.RoundToInt(duration * SampleRate));
            var data = new float[samples];
            for (var i = 0; i < samples; i++)
            {
                var t = i / (float)SampleRate;
                data[i] = Mathf.Clamp(generator(t), -1f, 1f);
            }

            var clip = AudioClip.Create(key, samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            s_Cache[key] = clip;
            return clip;
        }

        private static float Noise(float t)
        {
            // Ruído determinístico e barato: mistura de senos incomensuráveis.
            return Mathf.Sin(2f * Mathf.PI * 3719f * t) * 0.5f
                   + Mathf.Sin(2f * Mathf.PI * 7717f * t) * 0.3f
                   + Mathf.Sin(2f * Mathf.PI * 13337f * t) * 0.2f;
        }
    }
}
