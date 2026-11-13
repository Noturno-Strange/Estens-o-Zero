using ExtensaoZero.Audio;
using ExtensaoZero.Core;
using ExtensaoZero.Player;
using ExtensaoZero.UI;
using UnityEngine;

namespace ExtensaoZero.Interaction
{
    public enum JunctionState
    {
        Clean,
        Contaminated,
        Isolated
    }

    /// <summary>
    /// Caixa de passagem / circuito isolável (túneis e central original). Circuitos
    /// contaminados zumbem e piscam de leve (pista visual sem o medidor).
    /// </summary>
    public sealed class JunctionBox : InteractableBase, ICircuitReadable, ISignalSource
    {
        public int index;
        public string label = "CX";
        public string eventPrefix = "junction";
        public Renderer led;
        public JunctionState State { get; private set; } = JunctionState.Clean;

        public CircuitReading Reading
        {
            get
            {
                switch (State)
                {
                    case JunctionState.Contaminated: return CircuitReading.Contaminated;
                    case JunctionState.Isolated: return CircuitReading.Interrupted;
                    default: return CircuitReading.Energized;
                }
            }
        }

        public bool SignalActive => isActiveAndEnabled && State == JunctionState.Contaminated;
        public Vector3 SignalPosition => transform.position;
        public string SignalLabel => label;

        private float m_NextHum;
        private float m_Phase;

        protected override void Awake()
        {
            base.Awake();
            SignalRegistry.Register(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SignalRegistry.Unregister(this);
        }

        public override string GetPrompt(PlayerInteractor interactor)
        {
            return State == JunctionState.Isolated ? $"{label} — isolada" : $"Isolar {label}";
        }

        public override bool CanInteract(PlayerInteractor interactor)
        {
            return State != JunctionState.Isolated && base.CanInteract(interactor);
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (State == JunctionState.Isolated) return;
            var wasContaminated = State == JunctionState.Contaminated;
            SetState(JunctionState.Isolated);
            GameManager.Instance?.Audio?.PlayWorldOneShot(ProceduralAudio.RelayClick(), transform.position, 0.9f, AudioCategory.Sfx);
            GameManager.Instance?.RaiseEvent($"{eventPrefix}:{index}:{(wasContaminated ? "contaminated" : "clean")}");
        }

        public void SetState(JunctionState state)
        {
            State = state;
            m_Phase = index * 1.37f;
            ApplyLed(1f);
        }

        private void ApplyLed(float pulse)
        {
            if (led == null) return;
            Color color;
            switch (State)
            {
                case JunctionState.Contaminated: color = Color.Lerp(new Color(0.45f, 0.12f, 0.08f), new Color(0.95f, 0.25f, 0.16f), pulse); break;
                case JunctionState.Isolated: color = new Color(0.08f, 0.08f, 0.07f); break;
                default: color = new Color(0.35f, 0.8f, 0.45f); break;
            }

            var material = led.material;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", State == JunctionState.Isolated ? Color.black : color * 1.2f);
            }
        }

        private void Update()
        {
            if (State != JunctionState.Contaminated) return;
            var reduce = GameManager.Instance != null && GameManager.Instance.Settings != null && GameManager.Instance.Settings.reduceFlashes;
            ApplyLed(reduce ? 0.7f : 0.5f + 0.5f * Mathf.Sin(Time.time * 3.1f + m_Phase));
            if (Time.time < m_NextHum) return;
            m_NextHum = Time.time + 3.5f;
            GameManager.Instance?.Audio?.PlayWorldOneShot(ProceduralAudio.Hum(), transform.position, 0.25f, AudioCategory.Ambience);
        }
    }
}
