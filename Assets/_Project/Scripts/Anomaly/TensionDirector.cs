using ExtensaoZero.Core;
using ExtensaoZero.Environment;
using ExtensaoZero.UI;
using UnityEngine;

namespace ExtensaoZero.Anomaly
{
    /// <summary>
    /// Diretor de Tensão. O jogador nunca vê um medidor. O diretor decide quando o silêncio
    /// termina, respeitando cooldown global, histórico e etapa da missão.
    /// </summary>
    public sealed class TensionDirector : MonoBehaviour
    {
        public static TensionDirector Instance { get; private set; }

        public float Tension { get; private set; } = 0.22f;
        public int MissionStep { get; private set; }
        public string LastEventId { get; private set; } = string.Empty;
        public TensionEventDefinition LastEvent { get; private set; }

        public float globalCooldown = 12f;

        private const float MaxTension = 1f;

        private GameManager m_Manager;
        private WorldContext m_World;
        private TensionEventRunner m_Runner;
        private readonly TensionSelector m_Selector = new TensionSelector();
        private System.Random m_Random;
        private float m_NextEvaluation;
        private float m_NextGlobal;

        public static TensionDirector Create(Transform parent, GameManager manager, WorldContext world)
        {
            var go = new GameObject("TensionDirector");
            if (parent != null) go.transform.SetParent(parent, false);
            var director = go.AddComponent<TensionDirector>();
            director.m_Manager = manager;
            director.m_World = world;
            director.m_Random = new System.Random(world != null ? world.Seed : 1337);
            director.m_Runner = TensionEventRunner.Create(director.transform, manager, world);
            director.m_NextEvaluation = Time.time + 8f;
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

        public void SetMissionStep(int step)
        {
            MissionStep = step;
            // A missão avança a tensão de forma controlada, não aleatória.
            Tension = Mathf.Clamp01(0.2f + step * 0.09f);
            if (step >= 5) AddTension(0.1f);
        }

        public void AddTension(float amount)
        {
            Tension = Mathf.Clamp(Tension + amount, 0f, MaxTension);
        }

        public void ReduceTension(float amount)
        {
            Tension = Mathf.Clamp(Tension - amount, 0f, MaxTension);
        }

        public void ForceEvent(TensionCategory category)
        {
            TensionEventDefinition best = null;
            var events = TensionCatalog.Events;
            for (var i = 0; i < events.Count; i++)
            {
                var e = events[i];
                if (e.category != category) continue;
                if (MissionStep < e.minMissionStep) continue;
                if (e.id == LastEventId) continue;
                if (best == null || e.weight > best.weight) best = e;
            }

            if (best != null) Run(best);
        }

        private void Update()
        {
            var manager = m_Manager;
            if (manager == null || manager.State != GameState.Playing) return;

            // A tensão sobe devagar com o tempo de missão e com a contaminação.
            var contamination = manager.Contamination != null ? manager.Contamination.Normalized : 0f;
            var playerFactor = manager.Players.Count > 1 ? 0.0006f * manager.Players.Count : 0.0004f;
            Tension = Mathf.Clamp01(Tension + Time.deltaTime * (playerFactor + contamination * 0.002f));

            if (Time.time < m_NextEvaluation) return;
            m_NextEvaluation = Time.time + 2.5f;
            if (Time.time < m_NextGlobal) return;

            var eventDefinition = m_Selector.Select(
                TensionCatalog.Events, Tension, MissionStep, Time.time, LastEventId, m_Random);
            if (eventDefinition == null) return;
            Run(eventDefinition);
        }

        private void Run(TensionEventDefinition definition)
        {
            LastEvent = definition;
            LastEventId = definition.id;
            m_Selector.MarkFired(definition.id, Time.time);
            m_NextGlobal = Time.time + globalCooldown;
            m_NextEvaluation = Time.time + globalCooldown;
            Tension = Mathf.Clamp01(Tension - 0.12f);

            m_Runner?.Run(definition);

            if (!string.IsNullOrEmpty(definition.subtitle))
            {
                var color = definition.category == TensionCategory.Threat ? UITheme.Red
                    : definition.category == TensionCategory.Contradiction ? UITheme.Amber
                    : UITheme.Dim;
                m_Manager?.UI?.ShowSubtitle("", definition.subtitle, color, 5f);
            }

            if (definition.category == TensionCategory.Threat)
            {
                m_Manager?.Contamination?.Add(2.5f, "evento de ameaça");
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
