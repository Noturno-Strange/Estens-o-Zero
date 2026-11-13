using ExtensaoZero.Audio;
using ExtensaoZero.Core;
using ExtensaoZero.Telephone;
using ExtensaoZero.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ExtensaoZero.Player
{
    /// <summary>
    /// Equipamento mínimo e legível: lanterna sempre presente, sintonizador para camadas,
    /// um cabo por vez. Sem inventário complexo.
    /// </summary>
    public sealed class PlayerEquipment : MonoBehaviour
    {
        public Flashlight Flashlight { get; private set; }
        public Tuner Tuner { get; private set; }
        public CableItem CurrentCable { get; private set; }
        public bool HasCable => CurrentCable != null;

        public static PlayerEquipment Attach(FirstPersonController controller)
        {
            if (controller == null) return null;
            var equipment = controller.gameObject.GetComponent<PlayerEquipment>();
            if (equipment == null) equipment = controller.gameObject.AddComponent<PlayerEquipment>();
            equipment.Flashlight = Flashlight.Create(controller);
            equipment.Tuner = new Tuner();
            return equipment;
        }

        public void PickUpCable(CableItem cable)
        {
            CurrentCable = cable;
            GameManager.Instance?.UI?.SetEquipmentLabel(cable != null ? cable.Label : "SEM CABO");
        }

        public void DropCable()
        {
            CurrentCable = null;
            GameManager.Instance?.UI?.SetEquipmentLabel("SEM CABO");
        }

        /// <summary>
        /// O cabo permanece com o jogador enquanto está plugado; isso permite desconectar
        /// e corrigir uma conexão errada sem softlock.
        /// </summary>
        public void PlaceCable()
        {
            if (CurrentCable == null) return;
            GameManager.Instance?.UI?.SetEquipmentLabel(CurrentCable.Label + " (em uso)");
        }

        public void RechargeFlashlight(float amount)
        {
            Flashlight?.Recharge(amount);
        }

        public void CycleLayer()
        {
            var manager = GameManager.Instance;
            if (manager == null) return;
            if (manager.Contamination != null && manager.Contamination.IsLineBusy)
            {
                manager.Audio?.PlayScare(ProceduralAudio.Static(), 0.5f);
                manager.UI?.ShowSubtitle("LINHA OCUPADA", "O sintonizador não responde. Aguarde o isolamento ou sintonize a frequência de retorno.", UITheme.Red, 4f);
                return;
            }

            var session = manager.CallSession;
            if (session == null || !session.IsActive)
            {
                manager.UI?.ShowSubtitle("SINTONIZADOR", "Nenhuma chamada ativa.", UITheme.Dim, 2.5f);
                return;
            }

            session.CycleLayer();
            Tuner.SetLayer(session.CurrentLayer);
            manager.UI.SetLayerLabel(Tuner.Label);
            manager.Audio.PlayUiClick();
        }

        public void ToggleFlashlight()
        {
            Flashlight?.Toggle();
        }

        private void Update()
        {
            var manager = GameManager.Instance;
            if (manager == null || manager.State != GameState.Playing) return;
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.tKey.wasPressedThisFrame) CycleLayer();
            if (keyboard.fKey.wasPressedThisFrame) ToggleFlashlight();
        }
    }

    /// <summary>
    /// Estado local do sintonizador. Não escolhe camadas que não pertencem ao jogador.
    /// </summary>
    public sealed class Tuner
    {
        public CallLayer CurrentLayer { get; private set; } = CallLayer.Voice;

        public string Label
        {
            get
            {
                switch (CurrentLayer)
                {
                    case CallLayer.Ambience: return "AMBIENTE";
                    case CallLayer.Signals: return "SINAIS";
                    case CallLayer.Interference: return "INTERFERÊNCIA";
                    default: return "VOZ";
                }
            }
        }

        public void SetLayer(CallLayer layer)
        {
            CurrentLayer = layer;
        }

        public void Cycle()
        {
            CurrentLayer = (CallLayer)(((int)CurrentLayer + 1) % 4);
        }
    }
}
