using System;
using ExtensaoZero.Core;
using ExtensaoZero.UI;
using UnityEngine;

namespace ExtensaoZero.Anomaly
{
    /// <summary>
    /// Contaminação 0–100 (autoridade do host; clientes recebem o valor) e o estado local da
    /// Linha Ocupada do jogador. Sobe com conexões erradas; desce com isolamento e tempo sem
    /// erro. Nunca mata o jogador: cria a Linha Ocupada, que é sempre recuperável.
    /// </summary>
    public sealed class ContaminationSystem : MonoBehaviour
    {
        public static ContaminationSystem Instance { get; private set; }

        public event Action<float> Changed;
        public event Action LineBusyStarted;
        public event Action LineBusyEnded;

        public float Value { get; private set; }
        public float Normalized => Mathf.Clamp01(Value / 100f);
        public bool IsLineBusy { get; private set; }
        public float BusyRemaining => IsLineBusy ? Mathf.Max(0f, m_BusyUntil - Time.time) : 0f;
        public int ReturnFrequency { get; private set; }

        private GameManager m_Manager;
        private float m_BusyUntil;
        private float m_LastDecay;

        public static ContaminationSystem Create(Transform parent, GameManager manager)
        {
            var go = new GameObject("ContaminationSystem");
            if (parent != null) go.transform.SetParent(parent, false);
            var system = go.AddComponent<ContaminationSystem>();
            system.m_Manager = manager;
            return system;
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

        public void Add(float amount, string reason = "")
        {
            if (amount <= 0f) return;
            var before = Value;
            Value = Mathf.Clamp(Value + amount, 0f, 100f);
            Notify();

            if (!string.IsNullOrEmpty(reason)) Debug.Log($"[EZ] Contaminação +{amount:0}: {reason} ({Value:0}/100)");

            // Só o host decide quem entra em Linha Ocupada.
            if (m_Manager != null && m_Manager.IsHost && LineBusyRules.ShouldTrigger(before, Value, IsLineBusy))
            {
                m_Manager.TriggerLineBusy();
            }
        }

        public void Reduce(float amount)
        {
            if (amount <= 0f) return;
            Value = Mathf.Clamp(Value - amount, 0f, 100f);
            Notify();
        }

        public void Set(float value)
        {
            Value = Mathf.Clamp(value, 0f, 100f);
            Notify();
        }

        /// <summary>Valor replicado pelo host (clientes).</summary>
        public void SetFromNetwork(float normalized)
        {
            Value = Mathf.Clamp01(normalized) * 100f;
            Changed?.Invoke(Normalized);
            m_Manager?.UI?.SetContamination(Normalized);
        }

        private void Notify()
        {
            Changed?.Invoke(Normalized);
            m_Manager?.UI?.SetContamination(Normalized);
        }

        /// <summary>Isola o jogador local: estática, camadas bloqueadas, frequência de retorno.</summary>
        public void BeginLineBusy(float duration, int returnFrequency)
        {
            if (IsLineBusy) return;
            IsLineBusy = true;
            ReturnFrequency = Mathf.Clamp(returnFrequency, 0, LineBusyRules.FrequencySteps - 1);
            m_BusyUntil = Time.time + LineBusyRules.ClampDuration(duration);
            var equipment = m_Manager != null && m_Manager.LocalPlayerController != null
                ? m_Manager.LocalPlayerController.GetComponent<Player.PlayerEquipment>()
                : null;
            equipment?.Tuner.ResetFrequency();

            LineBusyStarted?.Invoke();
            m_Manager?.Audio?.PlayScare(Audio.ProceduralAudio.Static(), 0.65f);
            m_Manager?.UI?.ShowSubtitle("LINHA OCUPADA",
                $"Sua linha foi isolada. Gire o sintonizador (Q) até a frequência de retorno {ReturnFrequency} ou peça à equipe para restaurar sua extensão na central.",
                UITheme.Red, 7f);
            m_Manager?.UI?.SetLineBusy(true, 0, ReturnFrequency, BusyRemaining);
            m_Manager?.Tension?.AddTension(0.12f);
        }

        public void EndLineBusy(bool notify)
        {
            if (!IsLineBusy) return;
            IsLineBusy = false;
            LineBusyEnded?.Invoke();
            m_Manager?.UI?.SetLineBusy(false, 0, 0, 0f);
            if (m_Manager != null && !m_Manager.IsMultiplayer) m_Manager.World?.Switchboard?.SetCompromised(m_Manager.ExtensionOfClient(0), false);
            if (notify)
            {
                m_Manager?.UI?.ShowSubtitle("LINHA RESTAURADA", "O sinal voltou. A extensão comprometida foi liberada.", UITheme.Green, 4f);
            }
        }

        /// <summary>Recuperação pelo próprio jogador: sintonizou a frequência de retorno.</summary>
        public void TryRecoverByFrequency()
        {
            if (!IsLineBusy) return;
            EndLineBusy(true);
            if (m_Manager == null) return;
            if (m_Manager.IsMultiplayer && !m_Manager.IsHost)
            {
                m_Manager.LocalNetworkPlayer?.ReportLineRecovered();
            }
            else
            {
                var id = m_Manager.LocalNetworkPlayer != null ? m_Manager.LocalNetworkPlayer.OwnerClientId : 0UL;
                m_Manager.OnLineBusyRecovered(id);
            }
        }

        private void Update()
        {
            if (IsLineBusy && Time.time >= m_BusyUntil)
            {
                // Expirou sem recuperação: termina sozinha (nunca é punição permanente).
                EndLineBusy(true);
            }

            // Decaimento lento e previsível (host); não esconde o erro do jogador.
            if (m_Manager != null && m_Manager.IsHost && !IsLineBusy && Value > 0f && Time.time >= m_LastDecay)
            {
                m_LastDecay = Time.time + 8f;
                Reduce(0.8f);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
