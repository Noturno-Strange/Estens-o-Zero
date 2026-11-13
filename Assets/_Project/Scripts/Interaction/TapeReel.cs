using ExtensaoZero.Core;
using ExtensaoZero.Player;
using ExtensaoZero.Telephone;
using ExtensaoZero.UI;
using UnityEngine;

namespace ExtensaoZero.Interaction
{
    /// <summary>
    /// Fita em um gravador de rolo. Reproduz uma gravação antiga como chamada com camadas
    /// (o áudio é local de cada peer, como nas ligações).
    /// </summary>
    public sealed class TapeReel : InteractableBase, ISignalSource
    {
        public string tapeId = "tape";
        public string label = "FITA";
        public string callId = "";

        public bool SignalActive => isActiveAndEnabled && IsPlaying;
        public Vector3 SignalPosition => transform.position;
        public string SignalLabel => label;

        private bool IsPlaying
        {
            get
            {
                var session = GameManager.Instance != null ? GameManager.Instance.CallSession : null;
                return session != null && session.IsActive && session.ActiveCall != null && session.ActiveCall.id == callId;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            SignalRegistry.Register(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SignalRegistry.Unregister(this);
        }

        public override string GetPrompt(PlayerInteractor interactor)
        {
            return IsPlaying ? $"Parar {label}" : $"Reproduzir {label}";
        }

        public override void Interact(PlayerInteractor interactor)
        {
            var manager = GameManager.Instance;
            if (manager == null || manager.CallSession == null) return;
            if (IsPlaying)
            {
                manager.CallSession.StopCall();
                return;
            }

            var call = CallCatalog.Get(callId);
            if (call == null)
            {
                TellActor(interactor, label, "A fita está em branco.", UITheme.Dim, 3f);
                return;
            }

            if (manager.CallSession.IsActive)
            {
                TellActor(interactor, label, "Há outra chamada ativa. Termine-a antes de reproduzir a fita.", UITheme.Dim, 3.5f);
                return;
            }

            manager.Audio?.PlayWorldOneShot(Audio.ProceduralAudio.RelayClick(), transform.position, 0.6f, Audio.AudioCategory.Sfx);
            manager.CallSession.StartCall(call);
            manager.RaiseEvent("tape:" + tapeId);
        }
    }
}
