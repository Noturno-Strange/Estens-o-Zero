using System.Collections.Generic;
using ExtensaoZero.Player;
using ExtensaoZero.Telephone;
using UnityEngine;

namespace ExtensaoZero.Environment
{
    /// <summary>
    /// Referências explícitas do mundo construído. Evita buscas por nome em runtime e
    /// mantém o contrato entre missão, diretor, áudio e UI.
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

        public ExtensaoZero.Telephone.Telephone OfficePhone;
        public Switchboard Switchboard;
        public Transform CableSpawn;
        public Transform CorridorAnchor;
        public Transform SubscriberAnchor;
        public Transform IsolatorAnchor;
        public readonly List<Light> CeilingLights = new List<Light>();
        public readonly List<SwitchboardSocket> SwitchboardSockets = new List<SwitchboardSocket>();

        public void SetPlayerActive(bool active)
        {
            if (Player != null) Player.SetActive(active);
        }
    }
}
