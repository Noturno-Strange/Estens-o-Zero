using System;
using System.Collections.Generic;

namespace ExtensaoZero.Anomaly
{
    /// <summary>
    /// Seleção pura e determinística (dado o Random). Testável sem cena.
    /// Regras: cooldown global é responsabilidade do diretor; aqui valem faixa de tensão,
    /// etapa mínima, cooldown por evento, peso e proibição de repetir o último evento.
    /// </summary>
    public sealed class TensionSelector
    {
        private readonly Dictionary<string, float> m_LastFired = new Dictionary<string, float>();
        private readonly List<TensionEventDefinition> m_Eligible = new List<TensionEventDefinition>();

        public IReadOnlyDictionary<string, float> LastFired => m_LastFired;

        public TensionEventDefinition Select(
            IReadOnlyList<TensionEventDefinition> events,
            float tension,
            int missionStep,
            float now,
            string lastEventId,
            Random random)
        {
            if (events == null || events.Count == 0) return null;
            random ??= new Random(12345);

            m_Eligible.Clear();
            var totalWeight = 0f;
            for (var i = 0; i < events.Count; i++)
            {
                var e = events[i];
                if (e == null) continue;
                if (e.id == lastEventId) continue;
                if (tension < e.minTension || tension > e.maxTension) continue;
                if (missionStep < e.minMissionStep) continue;
                if (m_LastFired.TryGetValue(e.id, out var last) && now - last < e.cooldown) continue;
                if (e.weight <= 0f) continue;
                m_Eligible.Add(e);
                totalWeight += e.weight;
            }

            if (m_Eligible.Count == 0) return null;
            var roll = (float)random.NextDouble() * totalWeight;
            for (var i = 0; i < m_Eligible.Count; i++)
            {
                roll -= m_Eligible[i].weight;
                if (roll <= 0f) return m_Eligible[i];
            }

            return m_Eligible[m_Eligible.Count - 1];
        }

        public void MarkFired(string id, float now)
        {
            if (string.IsNullOrEmpty(id)) return;
            m_LastFired[id] = now;
        }

        public void Clear()
        {
            m_LastFired.Clear();
        }
    }
}
