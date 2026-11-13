using ExtensaoZero.Campaign;
using ExtensaoZero.Core;
using ExtensaoZero.Environment;
using UnityEngine;

namespace ExtensaoZero.Anomaly
{
    /// <summary>
    /// Diretor de Tensão (GDD §8). O jogador nunca vê um medidor. Roda só no host: decide quando
    /// o silêncio termina, respeitando cooldown global, histórico, fase da missão e dificuldade.
    /// Depois de uma ameaça, impõe 8–20 s de silêncio deliberado (AUDIO_DIRECTION §8).
    /// Os efeitos são executados localmente em cada peer.
    /// </summary>
    public sealed class TensionDirector : MonoBehaviour
    {
        public static TensionDirector Instance { get; private set; }

        public float Tension { get; private set; } = 0.22f;
        public int MissionStep { get; private set; }
        public string LastEventId { get; private set; } = string.Empty;
        public TensionEventDefinition LastEvent { get; private set; }
        public bool InSilence => Time.time < m_SilenceUntil;

        public float globalCooldown = 12f;

        private const float MaxTension = 1f;

        private GameManager m_Manager;
        private WorldContext m_World;
        private TensionEventRunner m_Runner;
        private readonly TensionSelector m_Selector = new TensionSelector();
        private System.Random m_Random;
        private float m_NextEvaluation;
        private float m_NextGlobal;
        private float m_SilenceUntil;
        private float m_CooldownMultiplier = 1f;

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

        public void Configure(int seed, DifficultyProfile difficulty)
        {
            m_Random = new System.Random(seed);
            m_Selector.Clear();
            m_CooldownMultiplier = difficulty != null ? difficulty.TensionCooldownMultiplier : 1f;
            m_Selector.ThreatWeightMultiplier = difficulty != null ? difficulty.ThreatWeightMultiplier : 1f;
            Tension = 0.22f;
            LastEventId = string.Empty;
            LastEvent = null;
            m_SilenceUntil = 0f;
            m_NextGlobal = Time.time + 10f;
            m_NextEvaluation = Time.time + 8f;
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

        /// <summary>Evento forçado pela missão (ignora o silêncio, respeita etapa e repetição).</summary>
        public void ForceEvent(TensionCategory category)
        {
            if (m_Manager != null && !m_Manager.IsHost) return;
            TensionEventDefinition best = null;
            var events = TensionCatalog.Events;
            for (var i = 0; i < events.Count; i++)
            {
                var e = events[i];
                if (e.category != category) continue;
                if (MissionStep < e.minMissionStep) continue;
                if (e.id == LastEventId) continue;
                if (e.kind == TensionEventKind.SubscriberManifestation) continue;
                if (best == null || e.weight > best.weight) best = e;
            }

            if (best != null) Run(best);
        }

        /// <summary>Executa os efeitos de um evento neste peer (comando do host ou da missão).</summary>
        public void RunLocal(string eventId)
        {
            var definition = TensionCatalog.Find(eventId);
            if (definition != null) m_Runner?.Run(definition);
        }

        private void Update()
        {
            var manager = m_Manager;
            if (manager == null || manager.State != GameState.Playing || !manager.IsHost) return;

            // A tensão sobe devagar com o tempo de missão, a contaminação e o tamanho da equipe.
            var contamination = manager.Contamination != null ? manager.Contamination.Normalized : 0f;
            var playerFactor = manager.PlayerCount > 1 ? 0.0006f * manager.PlayerCount : 0.0004f;
            Tension = Mathf.Clamp01(Tension + Time.deltaTime * (playerFactor + contamination * 0.002f + Separation(manager) * 0.0015f));

            if (Time.time < m_NextEvaluation) return;
            m_NextEvaluation = Time.time + 2.5f;
            if (Time.time < m_NextGlobal || InSilence) return;

            var eventDefinition = m_Selector.Select(TensionCatalog.Events, Tension, MissionStep, Time.time, LastEventId, m_Random);
            if (eventDefinition == null) return;
            Run(eventDefinition);
        }

        /// <summary>Separação da equipe (0–1): maior distância entre jogadores acima de 10 m.</summary>
        private static float Separation(GameManager manager)
        {
            if (!manager.IsMultiplayer || manager.Players.Count < 2) return 0f;
            var max = 0f;
            for (var i = 0; i < manager.Players.Count; i++)
            {
                for (var j = i + 1; j < manager.Players.Count; j++)
                {
                    var a = manager.Players[i];
                    var b = manager.Players[j];
                    if (a == null || b == null) continue;
                    max = Mathf.Max(max, Vector3.Distance(a.transform.position, b.transform.position));
                }
            }

            return Mathf.Clamp01((max - 10f) / 20f);
        }

        private void Run(TensionEventDefinition definition)
        {
            LastEvent = definition;
            LastEventId = definition.id;
            m_Selector.MarkFired(definition.id, Time.time);
            var cooldown = globalCooldown * m_CooldownMultiplier;
            m_NextGlobal = Time.time + cooldown;
            m_NextEvaluation = Time.time + cooldown;
            Tension = Mathf.Clamp01(Tension - 0.12f);

            if (definition.category == TensionCategory.Threat)
            {
                // Silêncio deliberado depois de um evento importante.
                m_SilenceUntil = Time.time + Mathf.Lerp(8f, 20f, (float)m_Random.NextDouble());
                m_NextGlobal = Mathf.Max(m_NextGlobal, m_SilenceUntil);
                m_Manager?.Contamination?.Add(2.5f, "evento de ameaça");
            }

            m_Runner?.Run(definition);
            if (m_Manager != null && m_Manager.IsMultiplayer) m_Manager.NetworkState?.BroadcastTension(definition.id);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
