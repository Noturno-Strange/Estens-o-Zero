using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using ExtensaoZero.Core;
using ExtensaoZero.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace ExtensaoZero.Multiplayer
{
    /// <summary>
    /// Cria NetworkManager e UnityTransport em runtime. Host autoritativo, conexão direta por
    /// IP/porta, aprovação por versão + hash da chave + vagas + missão não iniciada.
    /// Sem relay, STUN, UPnP ou qualquer serviço externo.
    /// </summary>
    public sealed class NetworkBootstrap : MonoBehaviour
    {
        public bool IsRunning => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
        public bool IsHost => NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
        public bool MissionStarted { get; set; }
        public ushort Port { get; private set; }
        public string SessionKey { get; private set; } = string.Empty;
        public string Fingerprint { get; private set; } = string.Empty;
        public string LastError { get; private set; } = string.Empty;
        public LanDiscovery Discovery { get; private set; }

        private GameManager m_Manager;
        private NetworkManager m_Network;
        private NetworkGameState m_State;
        private uint m_SessionHash;
        private float m_ConnectStarted;
        private bool m_Connecting;

        public static NetworkBootstrap Create(Transform parent, GameManager manager)
        {
            var go = new GameObject("NetworkBootstrap");
            if (parent != null) go.transform.SetParent(parent, false);
            var bootstrap = go.AddComponent<NetworkBootstrap>();
            bootstrap.m_Manager = manager;
            bootstrap.Discovery = go.AddComponent<LanDiscovery>();
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

            SessionKey = SessionCodec.Normalize(sessionKey);
            m_SessionHash = SessionCodec.Hash(SessionKey);
            Port = port;
            MissionStarted = false;
            Fingerprint = SessionKeyGenerator.Generate();
            var manager = EnsureNetworkManager();
            if (manager == null) return;

            var transport = Configure(manager, "0.0.0.0", port);
            manager.NetworkConfig.ConnectionApproval = true;
            manager.ConnectionApprovalCallback = ApprovalCheck;
            // O host também passa pela aprovação com o próprio payload.
            manager.NetworkConfig.ConnectionData = ConnectionPayload.Build(SessionKey);
            if (!RegisterPrefabs(manager)) return;

            if (!manager.StartHost())
            {
                Fail($"Falha ao iniciar o host na porta UDP {port}. Ela pode estar em uso ou bloqueada pelo firewall.");
                return;
            }

            var statePrefab = Resources.Load<GameObject>("NetworkGameState");
            if (statePrefab != null)
            {
                var stateGo = Instantiate(statePrefab);
                stateGo.name = "NetworkGameState";
                m_State = stateGo.GetComponent<NetworkGameState>();
                stateGo.GetComponent<NetworkObject>()?.Spawn();
            }

            Discovery.StartBeacon(() => new LanBeacon
            {
                Name = "Serra do Silêncio",
                Port = Port,
                PlayerCount = m_Manager != null ? m_Manager.Players.Count : 1,
                MissionStarted = MissionStarted,
                Fingerprint = Fingerprint
            });

            m_Manager?.UI?.ShowSubtitle("REDE", $"Host ativo na porta UDP {port}. Chave: {SessionKey}", UITheme.Green, 5f);
            Debug.Log($"[EZ] Host iniciado na porta {port} (transporte {transport.GetType().Name}).");
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

            SessionKey = SessionCodec.Normalize(sessionKey);
            Port = port;
            var manager = EnsureNetworkManager();
            if (manager == null) return;

            Configure(manager, ip.Trim(), port);
            manager.NetworkConfig.ConnectionApproval = true;
            manager.NetworkConfig.ConnectionData = ConnectionPayload.Build(SessionKey);
            if (!RegisterPrefabs(manager)) return;

            if (!manager.StartClient())
            {
                Fail("Falha ao iniciar o cliente. Verifique o endereço informado.");
                return;
            }

            m_Connecting = true;
            m_ConnectStarted = Time.time;
            m_Manager?.UI?.ShowSubtitle("REDE", $"Conectando a {ip}:{port}...", UITheme.Amber, 4f);
        }

        private UnityTransport Configure(NetworkManager manager, string address, ushort port)
        {
            var transport = manager.GetComponent<UnityTransport>();
            if (transport == null) transport = manager.gameObject.AddComponent<UnityTransport>();
            transport.SetConnectionData(address, port);
            transport.ConnectTimeoutMS = 1000;
            transport.MaxConnectAttempts = 10;
            manager.NetworkConfig.NetworkTransport = transport;
            manager.NetworkConfig.EnableSceneManagement = false;
            manager.NetworkConfig.ProtocolVersion = ConnectionPayload.ProtocolVersion;

            manager.OnClientConnectedCallback -= OnClientConnected;
            manager.OnClientConnectedCallback += OnClientConnected;
            manager.OnClientDisconnectCallback -= OnClientDisconnected;
            manager.OnClientDisconnectCallback += OnClientDisconnected;
            return transport;
        }

        private bool RegisterPrefabs(NetworkManager manager)
        {
            var prefab = Resources.Load<GameObject>("NetworkPlayer");
            if (prefab == null)
            {
                Fail("Prefab NetworkPlayer ausente. Rode Extensao Zero > Setup Project.");
                return false;
            }

            manager.NetworkConfig.PlayerPrefab = prefab;
            manager.AddNetworkPrefab(prefab);
            var statePrefab = Resources.Load<GameObject>("NetworkGameState");
            if (statePrefab != null) manager.AddNetworkPrefab(statePrefab);
            return true;
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
            var network = NetworkManager.Singleton;
            var connected = network != null && network.ConnectedClientsIds != null ? network.ConnectedClientsIds.Count : 0;
            var verdict = ConnectionPayload.Evaluate(request.Payload, m_SessionHash, connected, MissionStarted);
            if (verdict != ConnectionPayload.Verdict.Approved)
            {
                response.Approved = false;
                response.Reason = ConnectionPayload.Reason(verdict);
                Debug.Log($"[EZ] Conexão {request.ClientNetworkId} rejeitada: {response.Reason}");
                return;
            }

            var spawn = m_Manager != null && m_Manager.World != null ? m_Manager.World.GetSpawn(0) : new Pose(new Vector3(0f, 0.12f, -12.5f), Quaternion.identity);
            response.Approved = true;
            response.CreatePlayerObject = true;
            response.Position = spawn.position + new Vector3(connected * 0.8f - 1.2f, 0f, 0f);
            response.Rotation = spawn.rotation;
        }

        private void Update()
        {
            if (!m_Connecting) return;
            var network = NetworkManager.Singleton;
            if (network != null && network.IsConnectedClient)
            {
                m_Connecting = false;
                return;
            }

            if (Time.time - m_ConnectStarted > 12f)
            {
                m_Connecting = false;
                Shutdown();
                m_Manager?.OnDisconnectedFromHost("Tempo esgotado: o host não respondeu. Verifique IP, porta UDP, firewall e encaminhamento de porta. " +
                                                  "CGNAT pode impedir a conexão direta.");
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            m_Connecting = false;
            if (m_Manager == null) return;
            if (m_Manager.IsHost) m_Manager.UI?.ShowSubtitle("REDE", "Um investigador entrou na sessão.", UITheme.Green, 3f);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (m_Manager == null) return;
            var network = NetworkManager.Singleton;
            if (network != null && network.IsServer)
            {
                if (clientId != network.LocalClientId)
                {
                    m_Manager.UI?.ShowSubtitle("REDE", "Um investigador saiu da sessão. As camadas foram redistribuídas.", UITheme.Amber, 4f);
                }

                return;
            }

            // Cliente: ou fomos rejeitados, ou o host encerrou a sessão.
            var reason = network != null ? network.DisconnectReason : string.Empty;
            m_Connecting = false;
            Shutdown();
            m_Manager.OnDisconnectedFromHost(string.IsNullOrEmpty(reason)
                ? "O host encerrou a sessão ou a conexão caiu. Não há migração de host nesta versão."
                : reason);
        }

        public void Shutdown()
        {
            Discovery?.StopAll();
            if (m_State != null && m_State.IsSpawned && NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                m_State.GetComponent<NetworkObject>()?.Despawn();
            }

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening) NetworkManager.Singleton.Shutdown();
            m_State = null;
            MissionStarted = false;
        }

        /// <summary>Endereços IPv4 locais (para exibir no lobby e montar o convite).</summary>
        public static List<string> LocalAddresses()
        {
            var result = new List<string>();
            try
            {
                foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus != OperationalStatus.Up || nic.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
                    foreach (var address in nic.GetIPProperties().UnicastAddresses)
                    {
                        if (address.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(address.Address))
                        {
                            var text = address.Address.ToString();
                            if (!result.Contains(text)) result.Add(text);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[EZ] Não foi possível listar endereços locais: " + e.Message);
            }

            if (result.Count == 0) result.Add("127.0.0.1");
            return result;
        }

        public string BuildInvite()
        {
            var addresses = LocalAddresses();
            return InviteCodec.Encode(addresses[0], Port, SessionKey);
        }

        private void Fail(string message)
        {
            LastError = message;
            Debug.LogWarning("[EZ] " + message);
            m_Manager?.UI?.ShowSubtitle("REDE", message, UITheme.Red, 6f);
            m_Manager?.UI?.ShowNetworkError(message);
        }

        private void OnDestroy()
        {
            Discovery?.StopAll();
            if (m_Network != null)
            {
                m_Network.OnClientConnectedCallback -= OnClientConnected;
                m_Network.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }
    }
}
