using ExtensaoZero.Core;
using UnityEngine;

namespace ExtensaoZero.Interaction
{
    /// <summary>Gatilho do corredor do prólogo. Só o jogador local reporta (em rede, ao host).</summary>
    public sealed class CorridorTrigger : MonoBehaviour
    {
        private bool m_Triggered;

        public void ResetTrigger()
        {
            m_Triggered = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (m_Triggered) return;
            var player = other.GetComponentInParent<Player.FirstPersonController>();
            var manager = GameManager.Instance;
            if (player == null || manager == null || player != manager.LocalPlayerController) return;
            m_Triggered = true;
            ZoneTrigger.Report(manager, "corridor");
        }
    }
}
