using ExtensaoZero.Core;
using ExtensaoZero.Player;
using Unity.Netcode;
using UnityEngine;

namespace ExtensaoZero.Multiplayer
{
    /// <summary>
    /// Jogador de rede. O dono move localmente e envia posição ao host; o host replica
    /// para os demais. Não há previsão/rollback: a fatia prioriza simplicidade e
    /// comportamento previsível em LAN.
    /// </summary>
    public sealed class NetworkPlayer : NetworkBehaviour
    {
        public FirstPersonController Controller { get; private set; }
        public string DisplayName => $"Jogador {OwnerClientId}";

        private readonly NetworkVariable<Vector3> m_Position = new NetworkVariable<Vector3>(
            Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private readonly NetworkVariable<float> m_Yaw = new NetworkVariable<float>(
            0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private float m_NextSend;
        private bool m_Registered;

        private void Awake()
        {
            Controller = GetComponent<FirstPersonController>();
        }

        public override void OnNetworkSpawn()
        {
            Controller ??= GetComponent<FirstPersonController>();
            if (IsOwner)
            {
                if (Controller != null)
                {
                    Controller.SetNetworkControlled(true);
                    Controller.SetInputEnabled(true);
                    var equipment = PlayerEquipment.Attach(Controller);
                    var interactor = GetComponent<PlayerInteractor>();
                    if (interactor != null) interactor.Initialize(Controller.Camera, equipment);
                }

                GameManager.Instance?.OnNetworkPlayerSpawned(this);
                m_Registered = true;
            }
            else
            {
                if (Controller != null) Controller.SetNetworkControlled(false);
                ApplyRemoteTransform();
            }

            m_Position.OnValueChanged += OnRemoteTransformChanged;
            m_Yaw.OnValueChanged += OnRemoteYawChanged;
        }

        public override void OnNetworkDespawn()
        {
            m_Position.OnValueChanged -= OnRemoteTransformChanged;
            m_Yaw.OnValueChanged -= OnRemoteYawChanged;
            if (m_Registered) GameManager.Instance?.OnNetworkPlayerDespawned(this);
        }

        private void OnRemoteTransformChanged(Vector3 previous, Vector3 current)
        {
            if (!IsOwner) ApplyRemoteTransform();
        }

        private void OnRemoteYawChanged(float previous, float current)
        {
            if (!IsOwner) ApplyRemoteTransform();
        }

        private void Update()
        {
            if (!IsSpawned) return;
            if (IsOwner)
            {
                if (Time.time < m_NextSend) return;
                m_NextSend = Time.time + 0.066f;
                SubmitTransformServerRpc(transform.position, transform.eulerAngles.y);
            }
            else
            {
                // Interpolação curta para evitar teleporte visual em LAN.
                var target = m_Position.Value;
                if ((transform.position - target).sqrMagnitude < 400f)
                {
                    transform.position = Vector3.Lerp(transform.position, target, 18f * Time.deltaTime);
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                        Quaternion.Euler(0f, m_Yaw.Value, 0f), 18f * Time.deltaTime);
                }
            }
        }

        [ServerRpc]
        private void SubmitTransformServerRpc(Vector3 position, float yaw, ServerRpcParams rpcParams = default)
        {
            // Validação básica de limites; o host não confia cegamente no cliente.
            if (position.y < -5f || position.y > 20f) return;
            if (Mathf.Abs(position.x) > 80f || Mathf.Abs(position.z) > 80f) return;
            m_Position.Value = position;
            m_Yaw.Value = yaw;
        }

        private void ApplyRemoteTransform()
        {
            if (IsOwner) return;
            transform.position = m_Position.Value;
            transform.rotation = Quaternion.Euler(0f, m_Yaw.Value, 0f);
        }
    }
}
