using ExtensaoZero.Core;
using UnityEngine;

namespace ExtensaoZero.Interaction
{
    public sealed class CorridorTrigger : MonoBehaviour
    {
        private bool m_Triggered;

        private void OnTriggerEnter(Collider other)
        {
            if (m_Triggered) return;
            var player = other.GetComponentInParent<Player.FirstPersonController>();
            if (player == null) return;
            m_Triggered = true;
            GameManager.Instance.OnCorridorEntered();
        }
    }
}
