using System.Collections.Generic;
using ExtensaoZero.Core;
using UnityEngine;

namespace ExtensaoZero.Environment
{
    /// <summary>
    /// Culling por setor (PERFORMANCE_TARGETS §3): mantém acesas só as luzes do setor atual e
    /// dos vizinhos, troca a ambiência ao mudar de sala e registra setores descobertos (mapa).
    /// Atualiza a cada 0,4 s, não por frame.
    /// </summary>
    public sealed class SectorSystem : MonoBehaviour
    {
        public SectorInfo Current { get; private set; }

        private GameManager m_Manager;
        private WorldContext m_World;
        private float m_Next;
        private readonly HashSet<string> m_Active = new HashSet<string>();
        private readonly Dictionary<Light, bool> m_Suppressed = new Dictionary<Light, bool>();

        public static SectorSystem Create(Transform parent, GameManager manager, WorldContext world)
        {
            var go = new GameObject("SectorSystem");
            if (parent != null) go.transform.SetParent(parent, false);
            var system = go.AddComponent<SectorSystem>();
            system.m_Manager = manager;
            system.m_World = world;
            return system;
        }

        /// <summary>Luzes desligadas por comando de missão não são religadas pelo culling.</summary>
        public void SetSuppressed(Light light, bool suppressed)
        {
            if (light == null) return;
            if (suppressed) m_Suppressed[light] = true;
            else m_Suppressed.Remove(light);
            light.enabled = !suppressed && IsLightInActiveSector(light);
        }

        public void ClearSuppressed()
        {
            m_Suppressed.Clear();
        }

        private bool IsLightInActiveSector(Light light)
        {
            foreach (var sector in m_World.Sectors)
            {
                if (sector.Lights.Contains(light)) return m_Active.Contains(sector.Id);
            }

            return true;
        }

        private void Update()
        {
            if (Time.time < m_Next) return;
            m_Next = Time.time + 0.4f;
            Refresh(false);
        }

        public void Refresh(bool force)
        {
            if (m_World == null || m_Manager == null) return;
            var controller = m_Manager.LocalPlayerController;
            if (controller == null) return;
            var sector = m_World.SectorAt(controller.transform.position + Vector3.up * 0.9f);
            if (sector == null) return;
            if (sector == Current && !force) return;

            Current = sector;
            if (!sector.Discovered)
            {
                sector.Discovered = true;
                m_Manager.UI?.RefreshMap();
            }

            m_Active.Clear();
            m_Active.Add(sector.Id);
            foreach (var neighbor in sector.Neighbors) m_Active.Add(neighbor);

            foreach (var s in m_World.Sectors)
            {
                var on = m_Active.Contains(s.Id);
                foreach (var light in s.Lights)
                {
                    if (light == null) continue;
                    light.enabled = on && !m_Suppressed.ContainsKey(light);
                }
            }

            // A lua só ilumina quem está do lado de fora (ou vendo a entrada da recepção).
            if (m_World.Moon != null) m_World.Moon.enabled = sector.Id == "exterior" || sector.Id == "reception";

            if (m_Manager.State == GameState.Playing) m_Manager.Audio?.PlayAmbient(sector.Ambience);
        }
    }
}
