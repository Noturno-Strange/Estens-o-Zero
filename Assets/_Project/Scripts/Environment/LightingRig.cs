using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ExtensaoZero.Environment
{
    /// <summary>
    /// Iluminação contida: ambiente baixo, poucas luzes pontuais e névoa curta.
    /// O objetivo é legibilidade, não escuridão gratuita.
    /// </summary>
    public static class LightingRig
    {
        public static void Build(Transform root, WorldContext context)
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.055f, 0.062f, 0.068f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.028f;
            RenderSettings.fogColor = new Color(0.045f, 0.05f, 0.055f);
            RenderSettings.skybox = null;

            var directional = new GameObject("DirectionalFill");
            directional.transform.SetParent(root, false);
            directional.transform.rotation = Quaternion.Euler(52f, -38f, 0f);
            var dirLight = directional.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(0.55f, 0.62f, 0.72f);
            dirLight.intensity = 0.11f;
            dirLight.shadows = LightShadows.None;

            Add(context, root, "Luz_Central_A", new Vector3(-4f, 3.05f, -0.5f), new Color(1f, 0.82f, 0.58f), 1.35f, 8.5f, LightShadows.Soft);
            Add(context, root, "Luz_Central_B", new Vector3(2.8f, 3.05f, 2.2f), new Color(1f, 0.86f, 0.64f), 1.1f, 7.5f);
            Add(context, root, "Luz_Central_C", new Vector3(4.4f, 3.05f, -2.2f), new Color(0.82f, 0.9f, 1f), 0.8f, 6.5f);
            Add(context, root, "Luz_Arquivo_A", new Vector3(-10.8f, 2.75f, 0f), new Color(0.86f, 0.9f, 0.78f), 1.0f, 7f);
            Add(context, root, "Luz_Arquivo_B", new Vector3(-13.4f, 2.75f, -1.6f), new Color(0.75f, 0.8f, 0.9f), 0.55f, 5f);
            Add(context, root, "Luz_Corredor_A", new Vector3(0f, 2.72f, 8.5f), new Color(1f, 0.78f, 0.52f), 0.95f, 6.5f);
            Add(context, root, "Luz_Corredor_B", new Vector3(0f, 2.72f, 13.5f), new Color(1f, 0.8f, 0.55f), 0.75f, 6f);
            Add(context, root, "Luz_Corredor_C", new Vector3(0f, 2.72f, 18f), new Color(0.72f, 0.82f, 1f), 0.6f, 5.5f);

            Add(context, root, "Luz_Telefone", new Vector3(3.4f, 1.65f, 4.35f), new Color(1f, 0.68f, 0.32f), 0.5f, 2.6f);

            BuildVolume(root);
        }

        private static void Add(WorldContext context, Transform root, string name, Vector3 position, Color color, float intensity, float range, LightShadows shadows = LightShadows.None)
        {
            var light = PropFactory.CreatePointLight(root, name, position, color, intensity, range, shadows);
            context.CeilingLights.Add(light);
        }

        private static void BuildVolume(Transform root)
        {
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "EZ_VolumeProfile_Runtime";

            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(0.32f);
            bloom.threshold.Override(1.05f);
            bloom.scatter.Override(0.62f);

            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.3f);
            vignette.smoothness.Override(0.45f);

            var grain = profile.Add<FilmGrain>(true);
            grain.intensity.Override(0.16f);

            var color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(-0.15f);
            color.contrast.Override(12f);
            color.saturation.Override(-14f);
            color.colorFilter.Override(new Color(0.94f, 0.97f, 0.93f));

            var go = new GameObject("GlobalVolume");
            go.transform.SetParent(root, false);
            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;
            volume.profile = profile;
        }
    }
}
