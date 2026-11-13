using ExtensaoZero.Core;
using UnityEngine;

namespace ExtensaoZero.Player
{
    /// <summary>
    /// Lanterna com bateria de aproximadamente quatro minutos. A luz é funcional, não
    /// decorativa: sem ela, o corredor é quase ilegível.
    /// </summary>
    public sealed class Flashlight : MonoBehaviour
    {
        public Light Spot { get; private set; }
        public float Battery { get; private set; } = 100f;
        public float Battery01 => Mathf.Clamp01(Battery / 100f);
        public bool IsOn { get; private set; }

        public float drainPerSecond = 0.42f;
        public float baseIntensity = 2.4f;

        private float m_NextUiUpdate;
        private float m_FlickerSeed;

        public static Flashlight Create(FirstPersonController controller)
        {
            if (controller == null) return null;
            var existing = controller.GetComponentInChildren<Flashlight>();
            if (existing != null) return existing;

            var go = new GameObject("Flashlight");
            go.transform.SetParent(controller.CameraTransform != null ? controller.CameraTransform : controller.transform, false);
            go.transform.localPosition = new Vector3(0.16f, -0.12f, 0.1f);
            go.transform.localRotation = Quaternion.identity;

            var light = go.AddComponent<Light>();
            light.type = LightType.Spot;
            light.range = 19f;
            light.spotAngle = 54f;
            light.innerSpotAngle = 30f;
            light.intensity = 2.4f;
            light.color = new Color(1f, 0.93f, 0.82f);
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.82f;

            var flashlight = go.AddComponent<Flashlight>();
            flashlight.Spot = light;
            flashlight.m_FlickerSeed = Random.value * 10f;
            flashlight.SetOn(true);
            return flashlight;
        }

        public void Toggle()
        {
            SetOn(!IsOn);
            GameManager.Instance?.Audio?.PlayUiClick();
        }

        public void SetOn(bool on)
        {
            IsOn = on && Battery > 0.5f;
            if (Spot != null) Spot.enabled = IsOn;
        }

        public void Recharge(float amount)
        {
            Battery = Mathf.Clamp(Battery + Mathf.Max(0f, amount), 0f, 100f);
            if (!IsOn && Battery > 5f) SetOn(true);
            PushUi();
        }

        public void SetBattery(float value)
        {
            Battery = Mathf.Clamp(value, 0f, 100f);
            if (Battery <= 0f) SetOn(false);
            PushUi();
        }

        private void Update()
        {
            var manager = GameManager.Instance;
            if (manager == null || manager.State != GameState.Playing) return;
            if (IsOn)
            {
                Battery = Mathf.Max(0f, Battery - drainPerSecond * Time.deltaTime);
                if (Battery <= 0f) SetOn(false);

                if (Spot != null)
                {
                    var low = Mathf.InverseLerp(22f, 0f, Battery);
                    var flicker = 1f;
                    if (low > 0f)
                    {
                        var noise = Mathf.PerlinNoise(Time.time * 9f, m_FlickerSeed);
                        flicker = Mathf.Lerp(1f, noise > 0.72f ? 0.35f : 1f, low);
                    }

                    Spot.intensity = baseIntensity * flicker;
                }

                if (Time.time >= m_NextUiUpdate)
                {
                    m_NextUiUpdate = Time.time + 0.5f;
                    PushUi();
                }
            }
        }

        private void PushUi()
        {
            GameManager.Instance?.UI?.SetBattery(Battery01);
        }
    }
}
