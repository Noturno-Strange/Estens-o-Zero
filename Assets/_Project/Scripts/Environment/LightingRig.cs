using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ExtensaoZero.Environment
{
    /// <summary>
    /// Iluminação (ART_DIRECTION §6): base fria e escura, poças quentes de tungstênio sob
    /// luminárias reais. As luzes principais são spots apontados para baixo e com sombra, então a
    /// luz não atravessa paredes. A lua só acende na área externa e na recepção. Orçamento:
    /// culling por setor (SectorSystem) mantém ~8 luzes ativas por região.
    /// </summary>
    public static class LightingRig
    {
        public static readonly Color FogColor = new Color(0.086f, 0.157f, 0.141f) * 0.45f;

        public static void Build(Transform root, WorldContext context)
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.05f, 0.06f, 0.07f);
            RenderSettings.ambientEquatorColor = new Color(0.035f, 0.042f, 0.04f);
            RenderSettings.ambientGroundColor = new Color(0.02f, 0.02f, 0.018f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.03f;
            RenderSettings.fogColor = FogColor;
            RenderSettings.skybox = null;

            // Lua: fria, fraca, com sombra (o telhado e as lajes bloqueiam o interior).
            var moonGo = new GameObject("Lua");
            moonGo.transform.SetParent(root, false);
            moonGo.transform.rotation = Quaternion.Euler(38f, 155f, 0f);
            var moon = moonGo.AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.color = new Color(0.55f, 0.66f, 0.85f);
            moon.intensity = 0.22f;
            moon.shadows = LightShadows.Soft;
            moon.shadowStrength = 0.9f;
            context.Moon = moon;

            // Interiores: luminária pendente sobre cada área de trabalho.
            Lamp(root, context, "Luz_Central_A", new Vector3(-4f, 3.4f, 1.6f), Warm, 2.4f, 9f, true);
            Lamp(root, context, "Luz_Central_B", new Vector3(3f, 3.4f, 2.6f), Warm, 2.1f, 8.5f, true);
            Lamp(root, context, "Luz_Central_C", new Vector3(0.5f, 3.4f, -3.4f), Cold, 1.2f, 7f, false);
            var desk = PropFactory.CreatePointLight(root, "Luz_Telefone", new Vector3(4.15f, 1.55f, 4.75f), new Color(1f, 0.68f, 0.32f), 0.6f, 2.4f);
            context.CeilingLights.Add(desk);
            Lamp(root, context, "Luz_Recepcao", new Vector3(1f, 3.2f, -10.5f), Warm, 2.0f, 8.5f, true);
            Lamp(root, context, "Luz_Alojamentos", new Vector3(-10.5f, 3f, -10f), Dim, 1.2f, 7f, true);
            Lamp(root, context, "Luz_Operacao_A", new Vector3(12.5f, 3.4f, -2f), Warm, 1.8f, 8f, true);
            Lamp(root, context, "Luz_Operacao_B", new Vector3(12.5f, 3.4f, 3.4f), Cold, 1.0f, 7f, false);
            Lamp(root, context, "Luz_Arquivo_A", new Vector3(-11f, 3f, 0f), Paper, 1.8f, 8f, true);
            Lamp(root, context, "Luz_Corredor_A", new Vector3(0f, 3f, 8.8f), Warm, 1.6f, 7f, true);
            Lamp(root, context, "Luz_Corredor_B", new Vector3(0f, 3f, 15.2f), Cold, 1.0f, 6.5f, false);
            Lamp(root, context, "Luz_Deposito", new Vector3(-6.5f, 3.2f, 12.5f), Dim, 1.4f, 8.5f, true);
            Lamp(root, context, "Luz_Arquivo_Morto", new Vector3(4.8f, 3f, 14.8f), Paper, 1.6f, 7.5f, true);
            SpotDown(root, context, "Luz_Rampa", new Vector3(0f, 2.9f, 21.2f), new Color(0.8f, 0.12f, 0.08f), 1.6f, 9f, 100f, false);

            // Subsolo: luminárias frias e esparsas; vermelho na sala de energia.
            Lamp(root, context, "Luz_Tunel_A", new Vector3(0f, -0.2f, 31f), TunnelBlue, 1.4f, 7f, true, 0.55f);
            Lamp(root, context, "Luz_Tunel_B", new Vector3(-7f, -0.2f, 37f), TunnelBlue, 1.1f, 7f, false, 0.55f);
            Lamp(root, context, "Luz_Tunel_C", new Vector3(5f, -0.2f, 37f), new Color(0.85f, 0.55f, 0.35f), 1.0f, 7f, false, 0.55f);
            Lamp(root, context, "Luz_Tunel_D", new Vector3(0f, -0.2f, 43.5f), TunnelBlue, 0.9f, 6.5f, false, 0.55f);
            Lamp(root, context, "Luz_Energia", new Vector3(16f, 0.2f, 37f), new Color(0.95f, 0.12f, 0.07f), 2.6f, 9f, true);
            Lamp(root, context, "Luz_Original_A", new Vector3(-2f, 0.4f, 51.5f), new Color(1f, 0.66f, 0.38f), 1.4f, 8f, true);
            Lamp(root, context, "Luz_Original_B", new Vector3(3f, 0.4f, 55f), new Color(0.9f, 0.22f, 0.16f), 1.0f, 7f, false);

            BuildVolume(root);
        }

        private static readonly Color Warm = new Color(1f, 0.8f, 0.56f);
        private static readonly Color Cold = new Color(0.78f, 0.86f, 0.95f);
        private static readonly Color Dim = new Color(0.95f, 0.78f, 0.6f);
        private static readonly Color Paper = new Color(1f, 0.86f, 0.62f);
        private static readonly Color TunnelBlue = new Color(0.62f, 0.74f, 0.95f);

        /// <summary>Luminária pendente (modelo CC0) com spot para baixo e lâmpada emissiva.</summary>
        public static Light Lamp(Transform root, WorldContext context, string name, Vector3 ceilingPoint, Color color, float intensity,
            float range, bool shadows, float fixtureScale = 0.75f)
        {
            ArtLibrary.Prop(root, "hanging_industrial_lamp", ceilingPoint, 0f, false, fixtureScale, new Color(0.15f, 0.16f, 0.15f));
            var bulbHeight = 1.28f * fixtureScale;
            PropFactory.CreateSphere(root, name + "_Lampada", ceilingPoint + Vector3.down * bulbHeight, 0.09f * fixtureScale / 0.75f,
                MaterialLibrary.Emissive("bulb_" + ColorUtility.ToHtmlStringRGB(color), color, 2.2f));
            return SpotDown(root, context, name, ceilingPoint + Vector3.down * (bulbHeight + 0.05f), color, intensity, range, 125f, shadows);
        }

        /// <summary>Spot apontado para baixo: ilumina o ambiente sem vazar pelas paredes.</summary>
        public static Light SpotDown(Transform root, WorldContext context, string name, Vector3 position, Color color, float intensity,
            float range, float angle, bool shadows)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var light = go.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.spotAngle = angle;
            light.innerSpotAngle = angle * 0.45f;
            light.shadows = shadows ? LightShadows.Soft : LightShadows.None;
            light.shadowStrength = 0.85f;
            light.shadowBias = 0.03f;
            light.shadowNormalBias = 0.3f;
            light.renderMode = LightRenderMode.ForcePixel;
            context.CeilingLights.Add(light);
            return light;
        }

        private static void BuildVolume(Transform root)
        {
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "EZ_VolumeProfile_Runtime";

            var tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.Override(TonemappingMode.ACES);

            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(0.45f);
            bloom.threshold.Override(0.95f);
            bloom.scatter.Override(0.68f);

            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.34f);
            vignette.smoothness.Override(0.5f);

            var grain = profile.Add<FilmGrain>(true);
            grain.intensity.Override(0.22f);

            var color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(0.35f);
            color.contrast.Override(14f);
            color.saturation.Override(-18f);
            color.colorFilter.Override(new Color(0.93f, 0.98f, 0.95f));

            var chroma = profile.Add<ChromaticAberration>(true);
            chroma.intensity.Override(0.08f);

            var go = new GameObject("GlobalVolume");
            go.transform.SetParent(root, false);
            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;
            volume.profile = profile;
        }
    }
}
