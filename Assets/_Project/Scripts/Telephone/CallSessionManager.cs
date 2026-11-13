using System;
using System.Collections.Generic;
using ExtensaoZero.Audio;
using ExtensaoZero.Core;
using ExtensaoZero.Multiplayer;
using ExtensaoZero.UI;
using UnityEngine;

namespace ExtensaoZero.Telephone
{
    /// <summary>
    /// Dono local da chamada ativa. O host envia apenas o pedido de tocar/atender; o áudio e
    /// as legendas são locais. Camadas são distribuídas por LayerDistributor a partir do
    /// índice do jogador na lista ordenada por ClientId.
    /// </summary>
    public sealed class CallSessionManager : MonoBehaviour
    {
        public static CallSessionManager Instance { get; private set; }

        public event Action<CallDefinition> CallStarted;
        public event Action<CallDefinition> CallEnded;
        public event Action<CallLayer> LayerChanged;

        public CallDefinition ActiveCall { get; private set; }
        public UnityEngine.Object Owner { get; private set; }
        public bool IsActive => ActiveCall != null;
        public float Elapsed { get; private set; }
        public float Duration { get; private set; }
        public float Remaining => ActiveCall != null ? Mathf.Max(0f, Duration - Elapsed) : 0f;
        public CallLayer CurrentLayer => m_Layers != null && m_Layers.Length > 0 ? m_Layers[Mathf.Clamp(m_LayerIndex, 0, m_Layers.Length - 1)] : CallLayer.Voice;
        public CallLayer[] AssignedLayers => m_Layers ?? Array.Empty<CallLayer>();

        private CallLayer[] m_Layers = Array.Empty<CallLayer>();
        private int m_LayerIndex;
        private GameManager m_Manager;
        private float m_NextSubtitle;
        private float m_NextSignal;
        private int m_DtmfIndex;
        private readonly Dictionary<string, int> m_HeardMasks = new Dictionary<string, int>();

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

        public void StartCall(CallDefinition call, UnityEngine.Object owner = null)
        {
            if (call == null) return;
            if (IsActive) StopCall();
            ActiveCall = call;
            Owner = owner;
            Elapsed = 0f;
            m_LayerIndex = 0;
            m_DtmfIndex = 0;
            m_NextSignal = Time.time + 0.6f;
            m_NextSubtitle = Time.time + 6f;

            var players = m_Manager != null ? m_Manager.PlayerCount : 1;
            Duration = m_Manager != null && m_Manager.Difficulty != null
                ? m_Manager.Difficulty.CallDurationFor(call.duration, players)
                : call.duration;

            AssignLayers();
            PresentLayer(true);
            CallStarted?.Invoke(call);

            if (m_Manager != null && m_Manager.Save != null && !m_Manager.Save.seenCalls.Contains(call.id))
            {
                m_Manager.Save.seenCalls.Add(call.id);
                SaveSystem.Save(m_Manager.Save);
            }
        }

        /// <summary>Recalcula as camadas quando alguém entra/sai durante a chamada (NETWORK §9).</summary>
        public void RefreshAssignment()
        {
            if (!IsActive) return;
            var previous = CurrentLayer;
            AssignLayers();
            var keep = Array.IndexOf(m_Layers, previous);
            m_LayerIndex = keep >= 0 ? keep : 0;
            m_Manager?.UI?.SetLayerLabel(LayerDistributor.Label(CurrentLayer));
            PresentLayer(true);
        }

        private void AssignLayers()
        {
            var playerCount = m_Manager != null ? m_Manager.PlayerCount : 1;
            var index = m_Manager != null ? m_Manager.LocalPlayerIndex : 0;
            m_Layers = LayerDistributor.Distribute(playerCount, index);
        }

        public void StopCall()
        {
            if (ActiveCall == null) return;
            var call = ActiveCall;
            ActiveCall = null;
            Owner = null;
            Elapsed = 0f;
            m_Layers = Array.Empty<CallLayer>();
            m_Manager?.Audio?.StopCall();
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

        /// <summary>Máscara (bits 0–3) das camadas que este peer já ouviu da chamada.</summary>
        public int HeardMask(string callId)
        {
            return !string.IsNullOrEmpty(callId) && m_HeardMasks.TryGetValue(callId, out var mask) ? mask : 0;
        }

        private void MarkHeard(CallLayer layer)
        {
            if (ActiveCall == null) return;
            var id = ActiveCall.id;
            var before = HeardMask(id);
            var after = before | (1 << (int)layer);
            if (after == before) return;
            m_HeardMasks[id] = after;
            m_Manager?.ReportLayersHeard(id, after);
        }

        private bool Blocked => m_Manager != null && m_Manager.Contamination != null && m_Manager.Contamination.IsLineBusy;

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
                var layer = CurrentLayer;
                m_NextSignal = Time.time + (layer == CallLayer.Signals ? 0.42f : 3.2f);
                PlayLayerAudio(layer, 0.7f);
            }

            // O telefone é quem encerra a chamada e informa a missão; aqui apenas encerramos a
            // sessão quando o tempo acaba (fitas, telefone destruído).
            if (Elapsed >= Duration) StopCall();
        }

        private void PresentLayer(bool force)
        {
            if (!IsActive || m_Manager == null) return;
            if (Blocked)
            {
                // Linha Ocupada: as camadas compartilhadas ficam bloqueadas para este jogador.
                m_Manager.UI?.ShowSubtitle("LINHA OCUPADA", "Só estática. Sintonize a frequência de retorno (Q) ou peça à equipe que restaure sua extensão.", UITheme.Red, 5f);
                if (force) m_Manager.Audio?.PlayCallLayer(CallLayer.Interference, ProceduralAudio.Static(), 0.6f);
                return;
            }

            var layer = CurrentLayer;
            var text = ActiveCall.GetText(layer);
            m_Manager.UI?.SetLayerLabel(LayerDistributor.Label(layer));
            if (string.IsNullOrEmpty(text)) return;

            MarkHeard(layer);
            var color = UITheme.LayerColor(layer);
            var label = ActiveCall.GetLayerLabel(layer);
            m_Manager.UI?.ShowCallSubtitle(label, text, color, force ? 7f : 6f, ActiveCall.GetClip(layer) == null);
            if (force) PlayLayerAudio(layer, 1f);
        }

        private void PlayLayerAudio(CallLayer layer, float volume)
        {
            if (m_Manager == null || m_Manager.Audio == null || ActiveCall == null || Blocked) return;
            var recorded = ActiveCall.GetClip(layer);
            if (recorded != null)
            {
                // Gravações reais tocam inteiras uma vez por apresentação da camada.
                if (volume >= 1f) m_Manager.Audio.PlayCallLayer(layer, recorded, 1f, true);
                return;
            }

            AudioClip clip;
            switch (layer)
            {
                case CallLayer.Ambience:
                    clip = ProceduralAudio.AmbienceLayer(ActiveCall.id);
                    break;
                case CallLayer.Signals:
                    clip = NextDtmfClip();
                    break;
                case CallLayer.Interference:
                    clip = ProceduralAudio.Whisper();
                    break;
                default:
                    clip = ProceduralAudio.VoicePlaceholder(StableHash(ActiveCall.id));
                    break;
            }

            m_Manager.Audio.PlayCallLayer(layer, clip, volume, false);
        }

        /// <summary>Toca a sequência DTMF inteira, tom a tom, com pausa entre repetições.</summary>
        private AudioClip NextDtmfClip()
        {
            var digits = ActiveCall != null ? ActiveCall.dtmf : string.Empty;
            if (string.IsNullOrEmpty(digits)) return ProceduralAudio.Beep(1020f, 0.25f);
            var index = m_DtmfIndex % (digits.Length + 2);
            m_DtmfIndex++;
            if (index >= digits.Length) return null;
            var c = digits[index];
            return char.IsDigit(c) ? ProceduralAudio.Dtmf(c - '0') : ProceduralAudio.Beep(1020f, 0.12f);
        }

        private static int StableHash(string text)
        {
            unchecked
            {
                var hash = 23;
                if (text != null) foreach (var c in text) hash = hash * 31 + c;
                return hash & 0x7FFF;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
