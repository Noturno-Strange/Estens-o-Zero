using System;
using ExtensaoZero.Core;
using ExtensaoZero.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace ExtensaoZero.Multiplayer
{
    /// <summary>
    /// Cria NetworkManager e UnityTransport em runtime. Host autoritativo, conexão
    /// direta por IP/porta, aprovação por hash de chave. Sem relay/STUN/UPnP.
    /// </summary>
    public sealed class NetworkBootstrap : MonoBehaviour
    {
        public bool IsRunning => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
        public bool IsHost => NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
        public bool MissionStarted { get; set; }
        public ushort Port { get; private set; }
        public string LastError { get; private set; } = string.Empty;

        private GameManager m_Manager;
        private NetworkManager m_Network;
        private NetworkGameState m_State;
        private uint m_SessionHash;
        private bool m_HostStarted;

        public static NetworkBootstrap Create(Transform parent, GameManager manager)
        {
            var go = new GameObject("NetworkBootstrap");
            if (parent != null) go.transform.SetParent(parent, false);
            var bootstrap = go.AddComponent<NetworkBootstrap>();
            bootstrap.m_Manager = manager;
            return bootstrap;
        }

        public void StartHost(string sessionKey, ushort port)
        {
            LastError = string.Empty;
            if (!SessionCodec.IsValid(sessionKey))
            {
                Fail("Chave de sessão inválida.");
                return;
            }

            m_SessionHash = SessionCodec.Hash(sessionKey);
            Port = port;
            var manager = EnsureNetworkManager();
            if (manager == null) return;

            var transport = manager.GetComponent<UnityTransport>();
            if (transport == null) transport = manager.gameObject.AddComponent<UnityTransport>();
            transport.SetConnectionData("0.0.0.0", port);

            manager.NetworkConfig.NetworkTransport = transport;
            manager.NetworkConfig.ConnectionApproval = true;
            manager.ConnectionApprovalCallback = ApprovalCheck;
            manager.NetworkConfig.EnableSceneManagement = false;
            manager.NetworkConfig.ConnectionData = Array.Empty<byte>();

            var prefab = Resources.Load<GameObject>("NetworkPlayer");
            if (prefab == null)
            {
                Fail("Prefab NetworkPlayer ausente. Rode Extensao Zero > Setup Project.");
                return;
            }

            manager.NetworkConfig.PlayerPrefab = prefab;
            manager.AddNetworkPrefab(prefab);

            var statePrefab = Resources.Load<GameObject>("NetworkGameState");
            if (statePrefab != null) manager.AddNetworkPrefab(statePrefab);

            manager.OnClientConnectedCallback -= OnClientConnected;
            manager.OnClientConnectedCallback += OnClientConnected;
            manager.OnClientDisconnectCallback -= OnClientDisconnected;
            manager.OnClientDisconnectCallback += OnClientDisconnected;

            if (!manager.StartHost())
            {
                Fail("Falha ao iniciar host. Verifique a porta e o firewall.");
                return;
            }

            m_HostStarted = true;
            if (statePrefab != null)
            {
                var stateGo = Instantiate(statePrefab);
                stateGo.name = "NetworkGameState";
                m_State = stateGo.GetComponent<NetworkGameState>();
                var networkObject = stateGo.GetComponent<NetworkObject>();
                if (networkObject != null) networkObject.Spawn();
            }

            m_Manager?.UI?.ShowSubtitle("REDE", $"Host ativo na porta {port}. Chave: {SessionCodec.Normalize(sessionKey)}", UITheme.Green, 5f);
        }

        public void StartClient(string ip, ushort port, string sessionKey)
        {
            LastError = string.Empty;
            if (string.IsNullOrEmpty(ip))
            {
                Fail("IP do host não informado.");
                return;
            }

            if (!SessionCodec.IsValid(sessionKey))
            {
                Fail("Chave de sessão inválida.");
                return;
            }

            Port = port;
            var manager = EnsureNetworkManager();
            if (manager == null) return;

            var transport = manager.GetComponent<UnityTransport>();
            if (transport == null) transport = manager.gameObject.AddComponent<UnityTransport>();
            transport.SetConnectionData(ip, port);

            manager.NetworkConfig.NetworkTransport = transport;
            manager.NetworkConfig.ConnectionApproval = true;
            manager.NetworkConfig.EnableSceneManagement = false;
            manager.NetworkConfig.ConnectionData = BitConverter.GetBytes(SessionCodec.Hash(sessionKey));

            var prefab = Resources.Load<GameObject>("NetworkPlayer");
            if (prefab == null)
            {
                Fail("Prefab NetworkPlayer ausente. Rode Extensao Zero > Setup Project.");
                return;
            }

            manager.NetworkConfig.PlayerPrefab = prefab;
            manager.AddNetworkPrefab(prefab);

            var statePrefab = Resources.Load<GameObject>("NetworkGameState");
            if (statePrefab != null) manager.AddNetworkPrefab(statePrefab);

            manager.OnClientConnectedCallback -= OnClientConnected;
            manager.OnClientConnectedCallback += OnClientConnected;
            manager.OnClientDisconnectCallback -= OnClientDisconnected;
            manager.OnClientDisconnectCallback += OnClientDisconnected;

            if (!manager.StartClient())
            {
                Fail("Falha ao iniciar cliente.");
                return;
            }

            m_Manager?.UI?.ShowSubtitle("REDE", $"Conectando a {ip}:{port}...", UITheme.Amber, 4f);
        }

        private NetworkManager EnsureNetworkManager()
        {
            if (NetworkManager.Singleton != null)
            {
                m_Network = NetworkManager.Singleton;
                return m_Network;
            }

            var go = new GameObject("NetworkManager");
            go.transform.SetParent(transform, false);
            go.AddComponent<UnityTransport>();
            m_Network = go.AddComponent<NetworkManager>();
            return m_Network;
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            response.Pending = false;
            if (request.Payload == null || request.Payload.Length < sizeof(uint))
            {
                response.Approved = false;
                response.Reason = "Chave de sessão ausente.";
                return;
            }

            var hash = BitConverter.ToUInt32(request.Payload, 0);
            if (hash != m_SessionHash)
            {
                response.Approved = false;
                response.Reason = "Chave de sessão incorreta.";
                return;
            }

            response.Approved = true;
            response.CreatePlayerObject = true;
            response.Position = new Vector3(0f, 0.12f, -4.2f);
            response.Rotation = Quaternion.identity;
        }

        private void OnClientConnected(ulong clientId)
        {
            m_Manager?.UI?.ShowSubtitle("REDE", $"Cliente {clientId} conectado.", UITheme.Green, 3f);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (m_Manager == null) return;
            if (m_Manager.IsHost)
            {
                m_Manager.UI?.ShowSubtitle("REDE", $"Cliente {clientId} desconectou.", UITheme.Amber, 3f);
            }
            else
            {
                m_Manager.UI?.ShowSubtitle("REDE", "Desconectado do host.", UITheme.Red, 4f);
                m_Manager.SetState(GameState.Disconnected);
            }
        }

        public void Shutdown()
        {
            if (m_State != null && m_State.IsSpawned) m_State.GetComponent<NetworkObject>()?.Despawn();
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }

            m_State = null;
            m_HostStarted = false;
            MissionStarted = false;
        }

        private void Fail(string message)
        {
            LastError = message;
            Debug.LogWarning("[EZ] " + message);
            m_Manager?.UI?.ShowSubtitle("REDE", message, UITheme.Red, 5f);
        }

        private void OnDestroy()
        {
            if (m_Network != null)
            {
                m_Network.OnClientConnectedCallback -= OnClientConnected;
                m_Network.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }
    }
}
