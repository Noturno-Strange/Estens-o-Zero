using ExtensaoZero.Audio;
using ExtensaoZero.Core;
using ExtensaoZero.Interaction;
using ExtensaoZero.Player;
using ExtensaoZero.UI;
using UnityEngine;

namespace ExtensaoZero.Telephone
{
    public enum TelephoneState
    {
        Idle,
        Ringing,
        Answered
    }

    /// <summary>
    /// Telefone físico da central. Máquina de estados pequena e observável.
    /// A missão decide quando tocar; o telefone apenas apresenta e reporta.
    /// </summary>
    public sealed class Telephone : InteractableBase
    {
        public int extension = 0;
        public bool isOperatorPhone = true;
        public TelephoneState State { get; private set; } = TelephoneState.Idle;
        public CallDefinition CurrentCall => m_Call;

        private CallDefinition m_Call;
        private AudioSource m_Ring;
        private bool m_FinishedReported;

        protected override void Awake()
        {
            base.Awake();
            prompt = "Telefone";
            m_Ring = gameObject.AddComponent<AudioSource>();
            m_Ring.playOnAwake = false;
            m_Ring.loop = true;
            m_Ring.spatialBlend = 0.85f;
            m_Ring.rolloffMode = AudioRolloffMode.Linear;
            m_Ring.minDistance = 1f;
            m_Ring.maxDistance = 18f;
            m_Ring.volume = 0.55f;
        }

        public override string GetPrompt(PlayerInteractor interactor)
        {
            switch (State)
            {
                case TelephoneState.Ringing: return "Atender telefone";
                case TelephoneState.Answered: return "Desligar";
                default: return "Telefone";
            }
        }

        public override bool CanInteract(PlayerInteractor interactor)
        {
            return base.CanInteract(interactor) && State != TelephoneState.Idle;
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (State == TelephoneState.Ringing) Answer();
            else if (State == TelephoneState.Answered) HangUp();
        }

        public void BeginRing(CallDefinition call)
        {
            if (call == null || State == TelephoneState.Answered) return;
            m_Call = call;
            State = TelephoneState.Ringing;
            m_FinishedReported = false;
            m_Ring.clip = ProceduralAudio.Ring();
            m_Ring.Play();

            var manager = GameManager.Instance;
            manager?.UI?.ShowSubtitle("TELEFONE", "A central está tocando. Atenda.", UITheme.Amber, 4f);
        }

        public void Answer()
        {
            if (State != TelephoneState.Ringing || m_Call == null) return;
            State = TelephoneState.Answered;
            m_Ring.Stop();

            var manager = GameManager.Instance;
            manager?.Audio?.PlayUiClick();
            manager?.CallSession?.StartCall(m_Call);
            manager?.OnPhoneAnswered(this);
        }

        public void HangUp()
        {
            if (State != TelephoneState.Answered) return;
            State = TelephoneState.Idle;
            m_Ring.Stop();
            var manager = GameManager.Instance;
            manager?.CallSession?.StopCall();
            if (!m_FinishedReported)
            {
                m_FinishedReported = true;
                manager?.OnCallFinished(this);
            }

            m_Call = null;
        }

        public void ResetPhone()
        {
            State = TelephoneState.Idle;
            m_Call = null;
            m_FinishedReported = false;
            if (m_Ring != null) m_Ring.Stop();
        }

        private void Update()
        {
            if (State != TelephoneState.Answered) return;

            if (GameManager.Instance != null && GameManager.Instance.CallSession != null)
            {
                var session = GameManager.Instance.CallSession;
                if (!session.IsActive || session.Remaining <= 0.01f) HangUp();
            }
        }
    }
}
