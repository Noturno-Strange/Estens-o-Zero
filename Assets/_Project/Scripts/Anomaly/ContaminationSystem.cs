using System;
using ExtensaoZero.Core;
using ExtensaoZero.UI;
using UnityEngine;

namespace ExtensaoZero.Anomaly
{
    /// <summary>
    /// Contaminação 0–100. Sobe com conexões erradas e erros repetidos; desce com
    /// isolamento e tempo sem erro. Nunca mata o jogador: cria a Linha Ocupada.
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

        private const float BusyThreshold = 55f;
        private const float BusyDuration = 20f;

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
            Changed?.Invoke(Normalized);
            m_Manager?.UI?.SetContamination(Normalized);

            if (!string.IsNullOrEmpty(reason))
            {
                Debug.Log($"[EZ] Contaminação +{amount:0}: {reason} ({Value:0}/100)");
            }

            if (before < BusyThreshold && Value >= BusyThreshold && !IsLineBusy)
            {
                BeginLineBusy(BusyDuration);
            }
        }

        public void Reduce(float amount)
        {
            if (amount <= 0f) return;
            Value = Mathf.Clamp(Value - amount, 0f, 100f);
            Changed?.Invoke(Normalized);
            m_Manager?.UI?.SetContamination(Normalized);
        }

        public void Set(float value)
        {
            Value = Mathf.Clamp(value, 0f, 100f);
            Changed?.Invoke(Normalized);
            m_Manager?.UI?.SetContamination(Normalized);
        }

        public void BeginLineBusy(float duration = BusyDuration)
        {
            if (IsLineBusy) return;
            IsLineBusy = true;
            m_BusyUntil = Time.time + Mathf.Max(6f, duration);
            LineBusyStarted?.Invoke();
            m_Manager?.Audio?.PlayScare(Audio.ProceduralAudio.Static(), 0.65f);
            m_Manager?.UI?.ShowSubtitle("LINHA OCUPADA", "Sua linha foi isolada. Explore e sintonize a frequência de retorno.", UITheme.Red, 6f);
            m_Manager?.Tension?.AddTension(0.12f);
        }

        public void EndLineBusy()
        {
            if (!IsLineBusy) return;
            IsLineBusy = false;
            LineBusyEnded?.Invoke();
            m_Manager?.UI?.ShowSubtitle("LINHA RESTAURADA", "O sinal voltou. A extensão comprometida foi liberada.", UITheme.Green, 4f);
        }

        /// <summary>
        /// Recuperação alternativa: o jogador sintoniza a frequência de retorno exibida.
        /// </summary>
        public void TryRecoverByFrequency()
        {
            if (!IsLineBusy) return;
            EndLineBusy();
            Reduce(12f);
        }

        private void Update()
        {
            if (IsLineBusy && Time.time >= m_BusyUntil)
            {
                EndLineBusy();
            }

            // Decaimento lento e previsível; não esconde o erro do jogador.
            if (!IsLineBusy && Value > 0f && Time.time >= m_LastDecay)
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
