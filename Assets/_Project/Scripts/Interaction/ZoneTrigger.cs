using ExtensaoZero.Core;
using UnityEngine;

namespace ExtensaoZero.Interaction
{
    /// <summary>
    /// Volume que reporta a entrada do jogador local em uma zona ("zone:tunnels").
    /// Em rede, o dono reporta ao host; o host gera o evento de missão.
    /// </summary>
    public sealed class ZoneTrigger : MonoBehaviour
    {
        public string zoneId = "zone";
        public bool once = true;
        private bool m_Triggered;

        public void ResetTrigger()
        {
            m_Triggered = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (once && m_Triggered) return;
            var player = other.GetComponentInParent<Player.FirstPersonController>();
            var manager = GameManager.Instance;
            if (player == null || manager == null || player != manager.LocalPlayerController) return;
            if (manager.State != GameState.Playing) return;
            m_Triggered = true;
            Report(manager, zoneId);
        }

        public static void Report(GameManager manager, string zoneId)
        {
            if (manager == null) return;
            if (manager.IsMultiplayer && !manager.IsHost)
            {
                manager.LocalNetworkPlayer?.ReportZone(zoneId);
                return;
            }

            ApplyOnHost(manager, zoneId);
        }

        public static void ApplyOnHost(GameManager manager, string zoneId)
        {
            switch (zoneId)
            {
                case "corridor":
                    manager.OnCorridorEntered();
                    break;
                case "subscriber_contact":
                    manager.TriggerLineBusy();
                    manager.RaiseEvent("subscriber.contact");
                    break;
                case "subscriber_retreated":
                    manager.RaiseEvent("subscriber.retreated");
                    break;
                default:
                    manager.RaiseEvent("zone:" + zoneId);
                    break;
            }
        }
    }
}
