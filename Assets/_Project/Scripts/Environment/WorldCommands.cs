using System;
using ExtensaoZero.Core;
using ExtensaoZero.Interaction;
using UnityEngine;

namespace ExtensaoZero.Environment
{
    /// <summary>
    /// Comandos determinísticos de mundo ("door.unlock:porta_subsolo"). O host os aplica e
    /// replica; os clientes aplicam a mesma string. Também prepara o mundo de cada capítulo.
    /// </summary>
    public static class WorldCommands
    {
        /// <summary>Quais portas ficam livres em cada capítulo (setores liberados gradualmente).</summary>
        public static bool IsDoorOpenInChapter(string doorId, int chapter)
        {
            switch (doorId)
            {
                case "porta_arquivo_morto": return chapter >= 2;
                case "porta_subsolo": return chapter >= 3;
                case "porta_energia": return chapter >= 4;
                case "porta_central_original": return false; // a missão do capítulo 5 destranca
                default: return true;
            }
        }

        public static void ResetForChapter(GameManager manager, int chapter)
        {
            if (manager == null || manager.World == null) return;
            var world = manager.World;

            foreach (var pair in world.Doors)
            {
                pair.Value.SetOpen(false, false);
                pair.Value.SetLocked(!IsDoorOpenInChapter(pair.Key, chapter));
            }

            foreach (var pickup in world.Pickups.Values) pickup.ResetTo(null, false);
            foreach (var phone in world.Phones.Values) phone.ResetPhone();
            foreach (var box in world.TunnelBoxes) box.SetState(JunctionState.Clean);
            foreach (var box in world.OriginalCircuits) box.SetState(JunctionState.Clean);
            foreach (var breaker in world.Breakers.Values) breaker.SetOn(true);
            foreach (var console in world.EndingConsoles) console.Arm(false);
            foreach (var sw in world.Switches.Values) sw.ResetSwitch();
            foreach (var doc in world.Documents.Values) doc.ResetExamined();
            foreach (var zone in world.Zones) zone.ResetTrigger();
            world.CorridorTrigger?.ResetTrigger();
            world.Isolator?.Lock(false);
            world.Switchboard?.ResetBoard();
            manager.Sectors?.ClearSuppressed();
            foreach (var light in world.CeilingLights) if (light != null) light.enabled = true;
            manager.CallSession?.StopCall();
            manager.Contamination?.EndLineBusy(false);

            // Equipamento carregado de um capítulo anterior não atravessa capítulos.
            var equipment = manager.LocalPlayerController != null ? manager.LocalPlayerController.GetComponent<Player.PlayerEquipment>() : null;
            equipment?.ClearCable();
            foreach (var player in manager.Players)
            {
                var e = player != null ? player.GetComponent<Player.PlayerEquipment>() : null;
                e?.ClearCable();
            }
        }

        public static void Apply(GameManager manager, string command)
        {
            if (manager == null || manager.World == null || string.IsNullOrEmpty(command)) return;
            var world = manager.World;
            var parts = command.Split(':');
            var verb = parts[0];
            var arg = parts.Length > 1 ? parts[1] : string.Empty;
            var arg2 = parts.Length > 2 ? parts[2] : string.Empty;

            switch (verb)
            {
                case "door.unlock":
                    if (world.Doors.TryGetValue(arg, out var unlock)) unlock.SetLocked(false);
                    break;
                case "door.lock":
                    if (world.Doors.TryGetValue(arg, out var lockDoor)) lockDoor.SetLocked(true);
                    break;
                case "door.open":
                    if (world.Doors.TryGetValue(arg, out var open))
                    {
                        open.SetLocked(false);
                        open.SetOpen(true, true);
                    }
                    break;
                case "door.close":
                    if (world.Doors.TryGetValue(arg, out var close)) close.SetOpen(false, true);
                    break;
                case "pickup.show":
                    if (world.Pickups.TryGetValue(arg, out var show)) show.ResetTo(world.Anchor(arg2), true);
                    break;
                case "pickup.hide":
                    if (world.Pickups.TryGetValue(arg, out var hide)) hide.ResetTo(null, false);
                    break;
                case "isolator.lock":
                    world.Isolator?.Lock(true);
                    break;
                case "isolator.unlock":
                    world.Isolator?.Lock(false);
                    break;
                case "junction.contaminate":
                    SetJunction(world.TunnelBoxes, arg, JunctionState.Contaminated);
                    break;
                case "circuit.contaminate":
                    SetJunction(world.OriginalCircuits, arg, JunctionState.Contaminated);
                    break;
                case "socket.mark":
                    if (int.TryParse(arg, out var markExt)) world.Switchboard?.SetContaminatedMark(markExt, arg2 != "0");
                    break;
                case "ending.arm":
                    foreach (var console in world.EndingConsoles) console.Arm(true);
                    break;
                case "lights.off":
                    SetSectorLights(manager, arg, false);
                    break;
                case "lights.on":
                    SetSectorLights(manager, arg, true);
                    break;
                case "radio.reset":
                    if (world.Switches.TryGetValue("radio", out var radio)) radio.ResetSwitch();
                    break;
                case "lights.flicker":
                    manager.Tension?.RunLocal("light_flicker");
                    break;
                case "subscriber.manifest":
                    manager.Subscriber?.ManifestAt(arg, ParseMode(arg2));
                    break;
                case "subscriber.retreat":
                    manager.Subscriber?.Retreat();
                    break;
                default:
                    Debug.LogWarning("[EZ] Comando de mundo desconhecido: " + command);
                    break;
            }
        }

        public static AI.ManifestationMode ParseMode(string value)
        {
            return Enum.TryParse(value, out AI.ManifestationMode mode) ? mode : AI.ManifestationMode.Silhouette;
        }

        private static void SetJunction(System.Collections.Generic.List<JunctionBox> boxes, string arg, JunctionState state)
        {
            if (!int.TryParse(arg, out var index) || index < 0 || index >= boxes.Count) return;
            boxes[index].SetState(state);
        }

        private static void SetSectorLights(GameManager manager, string sectorId, bool on)
        {
            var sector = manager.World.GetSector(sectorId);
            if (sector == null) return;
            foreach (var light in sector.Lights) manager.Sectors?.SetSuppressed(light, !on);
        }
    }
}
