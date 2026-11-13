using ExtensaoZero.Audio;
using ExtensaoZero.Core;
using ExtensaoZero.Player;
using UnityEngine;

namespace ExtensaoZero.Interaction
{
    /// <summary>Disjuntor da sala de energia (ligado/desligado).</summary>
    public sealed class BreakerSwitch : InteractableBase, ICircuitReadable
    {
        public string breakerId = "A";
        public Transform handle;
        public bool IsOn { get; private set; } = true;

        public CircuitReading Reading => IsOn ? CircuitReading.Energized : CircuitReading.Interrupted;

        public override string GetPrompt(PlayerInteractor interactor)
        {
            return IsOn ? $"Desligar disjuntor {breakerId}" : $"Ligar disjuntor {breakerId}";
        }

        public override void Interact(PlayerInteractor interactor)
        {
            SetOn(!IsOn);
            GameManager.Instance?.Audio?.PlayWorldOneShot(ProceduralAudio.Impact(), transform.position, 0.6f, AudioCategory.Sfx);
            GameManager.Instance?.RaiseEvent($"breaker:{breakerId}:{(IsOn ? "on" : "off")}");
        }

        public void SetOn(bool on)
        {
            IsOn = on;
            if (handle != null) handle.localRotation = Quaternion.Euler(on ? -35f : 35f, 0f, 0f);
        }
    }
}
