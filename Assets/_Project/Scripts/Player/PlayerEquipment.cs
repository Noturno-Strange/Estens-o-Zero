using ExtensaoZero.Audio;
using ExtensaoZero.Campaign;
using ExtensaoZero.Core;
using ExtensaoZero.Interaction;
using ExtensaoZero.Investigation;
using ExtensaoZero.Telephone;
using ExtensaoZero.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ExtensaoZero.Player
{
    /// <summary>
    /// Equipamento legível e sem inventário complexo: lanterna e sintonizador sempre presentes,
    /// até dois opcionais (receptor, gravador, medidor, mapa) e um cabo por vez.
    /// Teclas: Q sintonizador, F lanterna, G soltar cabo, R gravador, M mapa.
    /// </summary>
    public sealed class PlayerEquipment : MonoBehaviour
    {
        public Flashlight Flashlight { get; private set; }
        public Tuner Tuner { get; private set; } = new Tuner();
        public Recorder Recorder { get; private set; } = new Recorder();
        public CableItem CurrentCable { get; private set; }
        public bool HasCable => CurrentCable != null;
        public bool CableInUse { get; private set; }
        public bool IsLocal { get; private set; }
        public Loadout Loadout { get; private set; } = new Loadout();

        private float m_NextReceiverScan;
        private float m_TunedSince = -1f;

        public static PlayerEquipment Attach(FirstPersonController controller, bool isLocal)
        {
            if (controller == null) return null;
            var equipment = controller.gameObject.GetComponent<PlayerEquipment>();
            if (equipment == null) equipment = controller.gameObject.AddComponent<PlayerEquipment>();
            equipment.IsLocal = isLocal;
            if (isLocal && equipment.Flashlight == null) equipment.Flashlight = Flashlight.Create(controller);
            equipment.Tuner ??= new Tuner();
            equipment.Recorder ??= new Recorder();
            return equipment;
        }

        public void ApplyLoadout(Loadout loadout)
        {
            Loadout = loadout ?? new Loadout();
            if (!IsLocal) return;
            var ui = GameManager.Instance != null ? GameManager.Instance.UI : null;
            ui?.SetLoadoutLabel(Describe());
            ui?.SetReceiver(Has(EquipmentKind.Receiver) ? "RECEPTOR: sem sinal" : string.Empty);
        }

        public bool Has(EquipmentKind kind) => Loadout != null && Loadout.Has(kind);

        private string Describe()
        {
            var text = "LANTERNA + SINTONIZADOR";
            if (Loadout == null) return text;
            foreach (var item in Loadout.Items) text += " + " + Loadout.Label(item);
            return text;
        }

        // ------------------------------------------------------------ cabo

        public void PickUpCable(CableItem cable)
        {
            CurrentCable = cable;
            CableInUse = false;
            if (IsLocal) GameManager.Instance?.UI?.SetEquipmentLabel(cable != null ? cable.Label : "SEM CABO");
        }

        /// <summary>
        /// G: se o cabo está plugado na central, desconecta (o cabo continua na mão); senão,
        /// solta o cabo no chão, onde qualquer jogador pode pegá-lo.
        /// </summary>
        public void DropCable()
        {
            if (CurrentCable == null) return;
            var manager = GameManager.Instance;
            if (CableInUse)
            {
                CableInUse = false;
                manager?.World?.Switchboard?.Disconnect(true);
                if (IsLocal) manager?.UI?.SetEquipmentLabel(CurrentCable.Label);
                return;
            }

            var source = CurrentCable.Source;
            CurrentCable = null;
            if (source != null) source.DropAt(transform.position + transform.forward * 0.6f + Vector3.up * 0.15f);
            if (IsLocal)
            {
                manager?.UI?.SetEquipmentLabel("SEM CABO");
                manager?.UI?.ShowSubtitle("EQUIPAMENTO", "Cabo solto no chão.", UITheme.Dim, 2.5f);
            }
        }

        public void ClearCable()
        {
            CurrentCable = null;
            CableInUse = false;
            if (IsLocal) GameManager.Instance?.UI?.SetEquipmentLabel("SEM CABO");
        }

        /// <summary>O cabo permanece com o jogador enquanto está plugado: dá para desconectar e corrigir.</summary>
        public void PlaceCable()
        {
            if (CurrentCable == null) return;
            CableInUse = true;
            if (IsLocal) GameManager.Instance?.UI?.SetEquipmentLabel(CurrentCable.Label + " (em uso — G desconecta)");
        }

        public void RechargeFlashlight(float amount)
        {
            Flashlight?.Recharge(amount);
        }

        // ------------------------------------------------------------ sintonizador

        public void CycleLayer()
        {
            var manager = GameManager.Instance;
            if (manager == null) return;
            if (manager.Contamination != null && manager.Contamination.IsLineBusy)
            {
                // Linha Ocupada: o sintonizador só gira a frequência de retorno.
                Tuner.StepFrequency(1);
                m_TunedSince = Tuner.Frequency == manager.Contamination.ReturnFrequency ? Time.time : -1f;
                manager.Audio?.PlayScare(ProceduralAudio.Static(), 0.35f);
                manager.UI?.SetLineBusy(true, Tuner.Frequency, manager.Contamination.ReturnFrequency, manager.Contamination.BusyRemaining);
                return;
            }

            var session = manager.CallSession;
            if (session == null || !session.IsActive)
            {
                manager.UI?.ShowSubtitle("SINTONIZADOR", "Nenhuma chamada ativa.", UITheme.Dim, 2.5f);
                return;
            }

            if (session.AssignedLayers.Length <= 1)
            {
                manager.UI?.ShowSubtitle("SINTONIZADOR", "Sua camada é " + Tuner.Label + ". As outras estão com a equipe.", UITheme.Dim, 3f);
                return;
            }

            session.CycleLayer();
            Tuner.SetLayer(session.CurrentLayer);
            manager.UI?.SetLayerLabel(Tuner.Label);
            manager.Audio?.PlayUiClick();
        }

        public void ToggleFlashlight()
        {
            Flashlight?.Toggle();
        }

        // ------------------------------------------------------------ gravador

        public void UseRecorder()
        {
            var manager = GameManager.Instance;
            if (manager == null) return;
            if (!Has(EquipmentKind.Recorder))
            {
                manager.UI?.ShowSubtitle("GRAVADOR", "Você não trouxe o gravador nesta missão.", UITheme.Dim, 2.5f);
                return;
            }

            var session = manager.CallSession;
            if (session != null && session.IsActive && !manager.Contamination.IsLineBusy)
            {
                var call = session.ActiveCall;
                if (Recorder.IsOtherVersionOf(call))
                {
                    var current = call.GetText(Recorder.Layer);
                    manager.UI?.ShowSubtitle("COMPARAÇÃO — " + call.GetLayerLabel(Recorder.Layer),
                        $"Gravado ({Recorder.Title}): {Recorder.Text}\nAgora ({call.title}): {current}", UITheme.Amber, 10f);
                    var evidence = EvidenceCatalog.Find(call.group);
                    if (evidence != null) manager.RegisterEvidence(evidence);
                }

                Recorder.Record(call, session.CurrentLayer);
                manager.Audio?.PlayUiClick();
                manager.UI?.ShowSubtitle("GRAVADOR", $"Camada {Tuner.Label} de \"{call.title}\" gravada. (R reproduz depois)", UITheme.Paper, 4f);
                manager.UI?.SetRecorderLabel("GRAVADOR: " + call.title + " / " + Tuner.Label);
                return;
            }

            if (!Recorder.HasRecording)
            {
                manager.UI?.ShowSubtitle("GRAVADOR", "Fita vazia. Grave uma camada durante uma chamada.", UITheme.Dim, 3f);
                return;
            }

            manager.Audio?.PlayCallLayer(Recorder.Layer, ProceduralAudio.VoicePlaceholder(Recorder.CallId.Length * 13), 0.8f, false);
            manager.UI?.ShowSubtitle("GRAVADOR — " + Recorder.Title, Recorder.Text, UITheme.LayerColor(Recorder.Layer), 8f);
        }

        // ------------------------------------------------------------ atualização local

        private void Update()
        {
            if (!IsLocal) return;
            var manager = GameManager.Instance;
            if (manager == null || manager.State != GameState.Playing) return;

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.qKey.wasPressedThisFrame) CycleLayer();
                if (keyboard.fKey.wasPressedThisFrame) ToggleFlashlight();
                if (keyboard.gKey.wasPressedThisFrame && HasCable) manager.RequestDropCable(this);
                if (keyboard.rKey.wasPressedThisFrame) UseRecorder();
                if (keyboard.mKey.wasPressedThisFrame)
                {
                    if (Has(EquipmentKind.ExtensionMap)) manager.OpenMap();
                    else manager.UI?.ShowSubtitle("MAPA", "Você não trouxe o mapa de extensões nesta missão.", UITheme.Dim, 2.5f);
                }
            }

            UpdateLineBusyTuning(manager);
            if (Has(EquipmentKind.Receiver)) UpdateReceiver(manager);
        }

        private void UpdateLineBusyTuning(GameManager manager)
        {
            var contamination = manager.Contamination;
            if (contamination == null || !contamination.IsLineBusy)
            {
                m_TunedSince = -1f;
                return;
            }

            manager.UI?.SetLineBusy(true, Tuner.Frequency, contamination.ReturnFrequency, contamination.BusyRemaining);
            if (Tuner.Frequency != contamination.ReturnFrequency)
            {
                m_TunedSince = -1f;
                return;
            }

            if (m_TunedSince < 0f) m_TunedSince = Time.time;
            if (Time.time - m_TunedSince >= 1.2f)
            {
                m_TunedSince = -1f;
                contamination.TryRecoverByFrequency();
            }
        }

        private void UpdateReceiver(GameManager manager)
        {
            if (Time.time < m_NextReceiverScan) return;
            m_NextReceiverScan = Time.time + 0.25f;

            ISignalSource best = null;
            var bestDistance = 16f;
            var sources = SignalRegistry.Sources;
            for (var i = 0; i < sources.Count; i++)
            {
                var source = sources[i];
                if (source == null || !source.SignalActive) continue;
                var distance = Vector3.Distance(transform.position, source.SignalPosition);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = source;
                }
            }

            if (manager.Subscriber != null && manager.Subscriber.IsVisible)
            {
                var d = Vector3.Distance(transform.position, manager.Subscriber.transform.position);
                if (d < bestDistance)
                {
                    manager.UI?.SetReceiver("RECEPTOR: ■■■■ " + Arrow(manager.Subscriber.transform.position) + " ???");
                    return;
                }
            }

            if (best == null)
            {
                manager.UI?.SetReceiver("RECEPTOR: sem sinal");
                return;
            }

            var strength = Mathf.Clamp(Mathf.CeilToInt((1f - bestDistance / 16f) * 4f), 1, 4);
            var bars = new string('■', strength) + new string('□', 4 - strength);
            manager.UI?.SetReceiver($"RECEPTOR: {bars} {Arrow(best.SignalPosition)} {best.SignalLabel}");
        }

        private string Arrow(Vector3 target)
        {
            var camera = GameManager.Instance != null && GameManager.Instance.LocalPlayerController != null
                ? GameManager.Instance.LocalPlayerController.CameraTransform
                : transform;
            var to = target - transform.position;
            to.y = 0f;
            var forward = camera.forward;
            forward.y = 0f;
            var angle = Vector3.SignedAngle(forward, to, Vector3.up);
            if (Mathf.Abs(target.y - transform.position.y) > 2.2f) return target.y > transform.position.y ? "▲ (acima)" : "▼ (abaixo)";
            if (angle > 135f || angle < -135f) return "▼ (atrás)";
            if (angle > 35f) return "►";
            if (angle < -35f) return "◄";
            return "▲";
        }
    }
}
