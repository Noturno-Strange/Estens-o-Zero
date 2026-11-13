using System.Collections.Generic;
using ExtensaoZero.Interaction;
using ExtensaoZero.Player;
using ExtensaoZero.Telephone;
using UnityEngine;
using Phone = ExtensaoZero.Telephone.Telephone;

namespace ExtensaoZero.Environment
{
    /// <summary>
    /// Referências explícitas do mundo construído. Evita buscas por nome em runtime e
    /// mantém o contrato entre missões, diretor, áudio e UI.
    /// </summary>
    public sealed class WorldContext
    {
        public GameObject Root;
        public GameObject Player;
        public FirstPersonController PlayerController;
        public Vector3 PlayerSpawn;
        public Quaternion PlayerSpawnRotation;
        public string AmbientZone = "sala_central";
        public int Seed = 1337;

        public Phone OfficePhone;
        public Switchboard Switchboard;
        public Transform CableSpawn;
        public Transform CorridorAnchor;
        public Transform SubscriberAnchor;
        public Transform IsolatorAnchor;
        public IsolatorLever Isolator;
        public Light Moon;
        public CorridorTrigger CorridorTrigger;

        public readonly List<Light> CeilingLights = new List<Light>();
        public readonly List<SwitchboardSocket> SwitchboardSockets = new List<SwitchboardSocket>();
        public readonly Dictionary<int, Phone> Phones = new Dictionary<int, Phone>();
        public readonly Dictionary<string, DoorInteractable> Doors = new Dictionary<string, DoorInteractable>();
        public readonly Dictionary<string, Transform> Anchors = new Dictionary<string, Transform>();
        public readonly Dictionary<string, CablePickup> Pickups = new Dictionary<string, CablePickup>();
        public readonly Dictionary<string, TapeReel> Tapes = new Dictionary<string, TapeReel>();
        public readonly Dictionary<string, BreakerSwitch> Breakers = new Dictionary<string, BreakerSwitch>();
        public readonly Dictionary<string, ExamineInteractable> Documents = new Dictionary<string, ExamineInteractable>();
        public readonly Dictionary<string, MissionSwitch> Switches = new Dictionary<string, MissionSwitch>();
        public readonly List<JunctionBox> TunnelBoxes = new List<JunctionBox>();
        public readonly List<JunctionBox> OriginalCircuits = new List<JunctionBox>();
        public readonly List<EndingConsole> EndingConsoles = new List<EndingConsole>();
        public readonly List<ZoneTrigger> Zones = new List<ZoneTrigger>();
        public readonly List<SectorInfo> Sectors = new List<SectorInfo>();

        public void SetPlayerActive(bool active)
        {
            if (Player != null) Player.SetActive(active);
        }

        public Phone GetPhone(int extension)
        {
            return Phones.TryGetValue(extension, out var phone) ? phone : null;
        }

        public Transform Anchor(string id)
        {
            return !string.IsNullOrEmpty(id) && Anchors.TryGetValue(id, out var anchor) ? anchor : null;
        }

        public SectorInfo GetSector(string id)
        {
            for (var i = 0; i < Sectors.Count; i++)
            {
                if (Sectors[i].Id == id) return Sectors[i];
            }

            return null;
        }

        public SectorInfo SectorAt(Vector3 position)
        {
            for (var i = 0; i < Sectors.Count; i++)
            {
                if (Sectors[i].Bounds.Contains(position)) return Sectors[i];
            }

            return null;
        }

        /// <summary>Ponto de partida de cada capítulo (recepção, corredor ou túneis).</summary>
        public Pose GetSpawn(int chapter)
        {
            string id;
            switch (chapter)
            {
                case 3: id = "spawn_corridor"; break;
                case 4:
                case 5: id = "spawn_tunnels"; break;
                default: id = "spawn_reception"; break;
            }

            var anchor = Anchor(id);
            return anchor != null ? new Pose(anchor.position, anchor.rotation) : new Pose(PlayerSpawn, PlayerSpawnRotation);
        }
    }
}
