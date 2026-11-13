// Stubs de compilação do URP/Core RP 17.3 (subset usado pelo projeto).
using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering
{
    public class VolumeParameter<T> { public T value; public bool overrideState; public virtual void Override(T x) { } }
    public class ClampedFloatParameter : VolumeParameter<float> { public ClampedFloatParameter(float v, float min, float max, bool o = false) { } }
    public class MinFloatParameter : VolumeParameter<float> { public MinFloatParameter(float v, float min, bool o = false) { } }
    public class FloatParameter : VolumeParameter<float> { public FloatParameter(float v, bool o = false) { } }
    public class ColorParameter : VolumeParameter<Color> { public ColorParameter(Color v, bool o = false) { } }
    public class BoolParameter : VolumeParameter<bool> { public BoolParameter(bool v, bool o = false) { } }

    public class VolumeComponent : ScriptableObject { public bool active = true; }

    public class VolumeProfile : ScriptableObject
    {
        public List<VolumeComponent> components = new List<VolumeComponent>();
        public T Add<T>(bool overrides = false) where T : VolumeComponent => null;
        public bool TryGet<T>(out T component) where T : VolumeComponent { component = null; return false; }
        public bool Has<T>() where T : VolumeComponent => false;
        public void Remove<T>() where T : VolumeComponent { }
    }

    public class Volume : MonoBehaviour
    {
        public bool isGlobal = true;
        public float priority;
        public float weight = 1f;
        public float blendDistance;
        public VolumeProfile sharedProfile;
        public VolumeProfile profile { get; set; }
    }
}

namespace UnityEngine.Rendering.Universal
{
    public sealed class Bloom : VolumeComponent
    {
        public MinFloatParameter threshold, intensity;
        public ClampedFloatParameter scatter;
        public ColorParameter tint;
    }
    public sealed class Vignette : VolumeComponent
    {
        public ColorParameter color;
        public ClampedFloatParameter intensity, smoothness;
    }
    public sealed class FilmGrain : VolumeComponent
    {
        public ClampedFloatParameter intensity, response;
    }
    public sealed class ColorAdjustments : VolumeComponent
    {
        public FloatParameter postExposure;
        public ClampedFloatParameter contrast, hueShift, saturation;
        public ColorParameter colorFilter;
    }
    public enum TonemappingMode { None, Neutral, ACES }
    public sealed class TonemappingModeParameter : VolumeParameter<TonemappingMode> { public TonemappingModeParameter(TonemappingMode v, bool o = false) { } }
    public sealed class Tonemapping : VolumeComponent { public TonemappingModeParameter mode; }
    public sealed class ChromaticAberration : VolumeComponent { public ClampedFloatParameter intensity; }
    public sealed class LensDistortion : VolumeComponent { public ClampedFloatParameter intensity; }

    public abstract class ScriptableRendererFeature : ScriptableObject { }
    public class ScriptableRendererData : ScriptableObject { public List<ScriptableRendererFeature> rendererFeatures = new List<ScriptableRendererFeature>(); public new void SetDirty() { } }
    public class UniversalRendererData : ScriptableRendererData { }

    public class UniversalRenderPipelineAsset : RenderPipelineAsset
    {
        public static UniversalRenderPipelineAsset Create(ScriptableRendererData rendererData = null) => null;
        public int msaaSampleCount { get; set; }
        public float renderScale { get; set; }
        public float shadowDistance { get; set; }
        public bool supportsHDR { get; set; }
        public bool supportsCameraDepthTexture { get; set; }
        public bool supportsCameraOpaqueTexture { get; set; }
        public int maxAdditionalLightsCount { get; set; }
        public int additionalLightsShadowmapResolution { get; set; }
        public int mainLightShadowmapResolution { get; set; }
        public override RenderPipeline CreatePipeline() => null;
    }

    public class UniversalAdditionalCameraData : MonoBehaviour
    {
        public bool renderPostProcessing { get; set; }
    }
}
