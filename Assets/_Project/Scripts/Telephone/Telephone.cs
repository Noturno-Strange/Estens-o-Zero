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
    /// Telefone físico (mesa da central ou parede). Máquina de estados pequena e observável.
    /// A missão decide quando tocar; o telefone apenas apresenta e reporta.
    /// </summary>
    public sealed class Telephone : InteractableBase, ISignalSource
    {
        public int extension = 0;
        public bool isOperatorPhone = true;
        public string locationLabel = "CENTRAL";
        public TelephoneState State { get; private set; } = TelephoneState.Idle;
        public CallDefinition CurrentCall => m_Call;

        public bool SignalActive => isActiveAndEnabled && State == TelephoneState.Ringing;
        public Vector3 SignalPosition => transform.position;
        public string SignalLabel => isOperatorPhone ? "CENTRAL" : extension >= 1000 ? locationLabel : $"EXT {extension:000}";

        private CallDefinition m_Call;
        private AudioSource m_Ring;
        private bool m_FinishedReported;
        private float m_RingStarted;

        protected override void Awake()
        {
            base.Awake();
            prompt = "Telefone";
            m_Ring = gameObject.AddComponent<AudioSource>();
            m_Ring.playOnAwake = false;
            m_Ring.loop = true;
            m_Ring.spatialBlend = 1f;
            m_Ring.rolloffMode = AudioRolloffMode.Logarithmic;
            m_Ring.minDistance = 1f;
            m_Ring.maxDistance = 25f;
            m_Ring.volume = 0.6f;
            gameObject.AddComponent<OcclusionFilter>();
            SignalRegistry.Register(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SignalRegistry.Unregister(this);
        }

        public override string GetPrompt(PlayerInteractor interactor)
        {
            switch (State)
            {
                case TelephoneState.Ringing: return "Atender telefone";
                case TelephoneState.Answered: return "Desligar";
                default: return isOperatorPhone ? "Telefone da central" : extension >= 1000 ? $"Telefone — {locationLabel}" : $"Telefone — extensão {extension:000}";
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
            m_RingStarted = Time.time;
            m_Ring.clip = ProceduralAudio.Ring();
            var manager = GameManager.Instance;
            m_Ring.volume = 0.6f * (manager != null && manager.Audio != null ? manager.Audio.SfxVolume : 1f);
            m_Ring.Play();
            manager?.UI?.ShowSubtitle("TELEFONE", isOperatorPhone
                ? "O telefone da central está tocando."
                : $"Um telefone está tocando: {locationLabel}.", UITheme.Amber, 4f);
        }

        public void Answer()
        {
            if (State != TelephoneState.Ringing || m_Call == null) return;
            var manager = GameManager.Instance;
            if (manager != null && manager.CallSession != null && manager.CallSession.IsActive)
            {
                // Uma chamada por vez: a nova substitui a anterior (ex.: fita tocando).
                manager.CallSession.StopCall();
            }

            State = TelephoneState.Answered;
            m_Ring.Stop();
            manager?.Audio?.PlayUiClick();
            manager?.CallSession?.StartCall(m_Call, this);
            manager?.OnPhoneAnswered(this);
        }

        public void HangUp()
        {
            if (State != TelephoneState.Answered) return;
            State = TelephoneState.Idle;
            m_Ring.Stop();
            var manager = GameManager.Instance;
            if (manager != null && manager.CallSession != null && manager.CallSession.Owner == this) manager.CallSession.StopCall();
            if (!m_FinishedReported)
            {
                m_FinishedReported = true;
                manager?.OnCallFinished(this);
            }

            m_Call = null;
        }

        /// <summary>Toque único (evento do Diretor de Tensão) sem chamada associada.</summary>
        public void RingOnce(float volume)
        {
            if (State != TelephoneState.Idle) return;
            GameManager.Instance?.Audio?.PlayWorldOneShot(ProceduralAudio.Ring(), transform.position, volume, AudioCategory.Scare);
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
            if (State == TelephoneState.Ringing)
            {
                // Telefone ignorado não trava a missão: continua tocando e repete o aviso.
                if (Time.time - m_RingStarted > 25f)
                {
                    m_RingStarted = Time.time;
                    GameManager.Instance?.UI?.ShowSubtitle("TELEFONE", $"O telefone continua tocando: {locationLabel}.", UITheme.Amber, 3.5f);
                }

                return;
            }

            if (State != TelephoneState.Answered) return;
            var manager = GameManager.Instance;
            if (manager == null || manager.CallSession == null) return;
            var session = manager.CallSession;
            if (session.Owner != this || !session.IsActive || session.Remaining <= 0.01f) HangUp();
        }
    }
}
