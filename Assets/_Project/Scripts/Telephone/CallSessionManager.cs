using System;
using ExtensaoZero.Audio;
using ExtensaoZero.Core;
using ExtensaoZero.Multiplayer;
using ExtensaoZero.UI;
using UnityEngine;

namespace ExtensaoZero.Telephone
{
    /// <summary>
    /// Dono local da chamada ativa. O host envia apenas id e instante de início;
    /// o áudio e as legendas são locais. Camadas são distribuídas por LayerDistributor.
    /// </summary>
    public sealed class CallSessionManager : MonoBehaviour
    {
        public static CallSessionManager Instance { get; private set; }

        public event Action<CallDefinition> CallStarted;
        public event Action<CallDefinition> CallEnded;
        public event Action<CallLayer> LayerChanged;

        public CallDefinition ActiveCall { get; private set; }
        public bool IsActive => ActiveCall != null;
        public float Elapsed { get; private set; }
        public float Remaining => ActiveCall != null ? Mathf.Max(0f, ActiveCall.duration - Elapsed) : 0f;
        public CallLayer CurrentLayer => m_Layers != null && m_Layers.Length > 0 ? m_Layers[m_LayerIndex] : CallLayer.Voice;
        public CallLayer[] AssignedLayers => m_Layers ?? Array.Empty<CallLayer>();

        private CallLayer[] m_Layers = Array.Empty<CallLayer>();
        private int m_LayerIndex;
        private GameManager m_Manager;
        private float m_NextSubtitle;
        private float m_NextSignal;

        public static CallSessionManager Create(Transform parent, GameManager manager)
        {
            var go = new GameObject("CallSessionManager");
            if (parent != null) go.transform.SetParent(parent, false);
            var session = go.AddComponent<CallSessionManager>();
            session.m_Manager = manager;
            return session;
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

        public void StartCall(CallDefinition call)
        {
            if (call == null) return;
            ActiveCall = call;
            Elapsed = 0f;
            m_LayerIndex = 0;

            var playerCount = m_Manager != null && m_Manager.IsMultiplayer
                ? Mathf.Max(1, m_Manager.Players.Count)
                : 1;
            var index = 0;
            if (m_Manager != null && m_Manager.LocalNetworkPlayer != null)
            {
                index = Mathf.Max(0, m_Manager.Players.IndexOf(m_Manager.LocalNetworkPlayer));
            }

            m_Layers = LayerDistributor.Distribute(playerCount, index);
            PresentLayer(true);
            CallStarted?.Invoke(call);

            if (m_Manager != null && m_Manager.Save != null && !m_Manager.Save.seenCalls.Contains(call.id))
            {
                m_Manager.Save.seenCalls.Add(call.id);
                SaveSystem.Save(m_Manager.Save);
            }
        }

        public void StopCall()
        {
            if (ActiveCall == null) return;
            var call = ActiveCall;
            ActiveCall = null;
            Elapsed = 0f;
            m_Layers = Array.Empty<CallLayer>();
            CallEnded?.Invoke(call);
        }

        public void CycleLayer()
        {
            if (!IsActive || m_Layers == null || m_Layers.Length <= 1) return;
            m_LayerIndex = (m_LayerIndex + 1) % m_Layers.Length;
            PresentLayer(true);
            LayerChanged?.Invoke(CurrentLayer);
        }

        public void SetLayer(CallLayer layer)
        {
            if (!IsActive || m_Layers == null) return;
            var found = Array.IndexOf(m_Layers, layer);
            if (found < 0) return;
            m_LayerIndex = found;
            PresentLayer(true);
            LayerChanged?.Invoke(CurrentLayer);
        }

        public bool HasLayer(CallLayer layer)
        {
            return m_Layers != null && Array.IndexOf(m_Layers, layer) >= 0;
        }

        private void Update()
        {
            if (!IsActive) return;
            Elapsed += Time.deltaTime;

            if (Time.time >= m_NextSubtitle)
            {
                m_NextSubtitle = Time.time + 6f;
                PresentLayer(false);
            }

            if (Time.time >= m_NextSignal)
            {
                m_NextSignal = Time.time + 3.2f;
                PlayLayerAudio(CurrentLayer, 0.7f);
            }

            // O telefone é quem encerra a chamada e informa a missão; aqui apenas
            // encerramos a sessão se ela ficou órfã (telefone destruído, por exemplo).
            if (Elapsed >= ActiveCall.duration) StopCall();
        }

        private void PresentLayer(bool force)
        {
            if (!IsActive || m_Manager == null) return;
            var layer = CurrentLayer;
            var text = ActiveCall.GetText(layer);
            if (string.IsNullOrEmpty(text)) return;

            var color = UITheme.LayerColor(layer);
            var label = ActiveCall.GetLayerLabel(layer);
            m_Manager.UI?.ShowSubtitle(label, text, color, force ? 7f : 6f);
            if (force) PlayLayerAudio(layer, 1f);
        }

        private void PlayLayerAudio(CallLayer layer, float volume)
        {
            if (m_Manager == null || m_Manager.Audio == null) return;
            AudioClip clip;
            switch (layer)
            {
                case CallLayer.Ambience:
                    clip = ProceduralAudio.Footstep();
                    break;
                case CallLayer.Signals:
                    clip = ProceduralAudio.Dtmf(ActiveCall != null && ActiveCall.dtmf.Length > 0 ? ActiveCall.dtmf[0] - '0' : 2);
                    break;
                case CallLayer.Interference:
                    clip = ProceduralAudio.Whisper();
                    break;
                default:
                    clip = ProceduralAudio.VoicePlaceholder(ActiveCall != null ? ActiveCall.id.GetHashCode() : 7);
                    break;
            }

            m_Manager.Audio.PlayCallLayer(layer, clip, volume);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
