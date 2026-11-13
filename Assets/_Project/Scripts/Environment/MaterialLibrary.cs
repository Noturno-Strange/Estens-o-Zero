using System.Collections.Generic;
using UnityEngine;

namespace ExtensaoZero.Environment
{
    /// <summary>
    /// Paleta controlada e materiais compartilhados. Nada de texturas fotorrealistas:
    /// ruído sutil + cores chapadas para leitura clara em 1080p.
    /// </summary>
    public static class MaterialLibrary
    {
        private static readonly Dictionary<string, Material> s_Cache = new Dictionary<string, Material>();
        private static Texture2D s_Noise;

        public static Material Wall => Get("wall", new Color(0.115f, 0.125f, 0.115f), 0.08f, 0f, true);
        public static Material WallDark => Get("wall_dark", new Color(0.075f, 0.08f, 0.075f), 0.05f, 0f, true);
        public static Material Floor => Get("floor", new Color(0.16f, 0.135f, 0.11f), 0.12f, 0f, true);
        public static Material Ceiling => Get("ceiling", new Color(0.055f, 0.06f, 0.058f), 0.03f, 0f);
        public static Material Metal => Get("metal", new Color(0.20f, 0.225f, 0.215f), 0.55f, 0.65f);
        public static Material MetalDark => Get("metal_dark", new Color(0.10f, 0.115f, 0.11f), 0.4f, 0.5f);
        public static Material Wood => Get("wood", new Color(0.26f, 0.19f, 0.12f), 0.18f, 0f, true);
        public static Material WoodDark => Get("wood_dark", new Color(0.14f, 0.10f, 0.07f), 0.12f, 0f, true);
        public static Material Paper => Get("paper", new Color(0.72f, 0.67f, 0.54f), 0.03f, 0f);
        public static Material Glass => Get("glass", new Color(0.12f, 0.16f, 0.17f), 0.9f, 0.1f);
        public static Material Cable => Get("cable", new Color(0.85f, 0.55f, 0.12f), 0.35f, 0.1f);
        public static Material IndicatorIdle => Get("indicator_idle", new Color(0.12f, 0.10f, 0.08f), 0.4f, 0f);
        public static Material IndicatorOn => Emissive("indicator_on", new Color(0.95f, 0.62f, 0.16f), 1.8f);
        public static Material Emergency => Emissive("emergency", new Color(0.75f, 0.12f, 0.09f), 1.4f);

        public static Material Get(string key, Color color, float smoothness = 0.2f, float metallic = 0f, bool noisy = false)
        {
            if (s_Cache.TryGetValue(key, out var existing) && existing != null) return existing;

            var shader = ResolveLitShader();
            var material = new Material(shader) { name = "EZ_" + key };
            SetColor(material, color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (noisy) SetNoise(material);
            s_Cache[key] = material;
            return material;
        }

        public static Material Emissive(string key, Color color, float intensity = 1.5f)
        {
            if (s_Cache.TryGetValue(key, out var existing) && existing != null) return existing;
            var shader = ResolveLitShader();
            var material = new Material(shader) { name = "EZ_" + key };
            SetColor(material, color * 0.6f);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * intensity);
            }

            s_Cache[key] = material;
            return material;
        }

        public static Material Unlit(string key, Color color)
        {
            if (s_Cache.TryGetValue(key, out var existing) && existing != null) return existing;
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            var material = new Material(shader) { name = "EZ_" + key };
            SetColor(material, color);
            s_Cache[key] = material;
            return material;
        }

        public static void Clear()
        {
            s_Cache.Clear();
        }

        private static Shader ResolveLitShader()
        {
            // O material em Resources garante que o shader URP Lit seja incluído no build.
            var defaultMaterial = Resources.Load<Material>("EZ_Default");
            if (defaultMaterial != null && defaultMaterial.shader != null) return defaultMaterial.shader;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Hidden/InternalErrorShader");
            return shader;
        }

        private static void SetColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        }

        private static void SetNoise(Material material)
        {
            if (s_Noise == null)
            {
                const int size = 32;
                s_Noise = new Texture2D(size, size, TextureFormat.RGBA32, true)
                {
                    wrapMode = TextureWrapMode.Repeat,
                    filterMode = FilterMode.Bilinear,
                    name = "EZ_Noise"
                };
                var random = new System.Random(451);
                for (var y = 0; y < size; y++)
                {
                    for (var x = 0; x < size; x++)
                    {
                        var v = 0.82f + (float)random.NextDouble() * 0.18f;
                        s_Noise.SetPixel(x, y, new Color(v, v, v, 1f));
                    }
                }

                s_Noise.Apply();
            }

            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", s_Noise);
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", s_Noise);
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTextureScale("_BaseMap", new Vector2(2.5f, 2.5f));
            }
        }
    }
}
