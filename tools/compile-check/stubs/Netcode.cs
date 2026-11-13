// Stubs de compilação baseados nas assinaturas públicas do NGO 2.14.0 (não executáveis).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Netcode
{
    public enum NetworkVariableReadPermission { Everyone, Owner }
    public enum NetworkVariableWritePermission { Server, Owner }
    public enum RpcDelivery { Unreliable = 0, Reliable }
    public enum RpcInvokePermission { Everyone, Server, Owner }

    public abstract class NetworkVariableBase : IDisposable
    {
        protected NetworkVariableBase(NetworkVariableReadPermission r, NetworkVariableWritePermission w) { }
        public virtual void Dispose() { }
    }

    public class NetworkVariable<T> : NetworkVariableBase
    {
        public delegate void OnValueChangedDelegate(T previousValue, T newValue);
        public OnValueChangedDelegate OnValueChanged;
        public NetworkVariable(T value = default, NetworkVariableReadPermission readPerm = NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission writePerm = NetworkVariableWritePermission.Server) : base(readPerm, writePerm) { Value = value; }
        public virtual T Value { get; set; }
    }

    public struct ServerRpcSendParams { }
    public struct ServerRpcReceiveParams { public ulong SenderClientId; }
    public struct ServerRpcParams { public ServerRpcSendParams Send; public ServerRpcReceiveParams Receive; }
    public struct ClientRpcSendParams { public IReadOnlyList<ulong> TargetClientIds; }
    public struct ClientRpcReceiveParams { }
    public struct ClientRpcParams { public ClientRpcSendParams Send; public ClientRpcReceiveParams Receive; }

    public abstract class RpcAttribute : Attribute
    {
        public RpcDelivery Delivery = RpcDelivery.Reliable;
        public RpcInvokePermission InvokePermission;
        [Obsolete("RequireOwnership is deprecated. Please use InvokePermission instead.")]
        public bool RequireOwnership;
        public bool DeferLocal;
    }

    [AttributeUsage(AttributeTargets.Method)] public class ServerRpcAttribute : RpcAttribute { }
    [AttributeUsage(AttributeTargets.Method)] public class ClientRpcAttribute : RpcAttribute { }

    public struct NetworkTime
    {
        public double Time => 0d;
        public float TimeAsFloat => 0f;
    }

    public class NetworkObject : MonoBehaviour
    {
        public bool IsSpawned { get; }
        public ulong OwnerClientId { get; }
        public ulong NetworkObjectId { get; }
        public bool IsOwner { get; }
        public void Spawn(bool destroyWithScene = false) { }
        public void SpawnWithOwnership(ulong clientId, bool destroyWithScene = false) { }
        public void Despawn(bool destroy = true) { }
    }

    public abstract class NetworkBehaviour : MonoBehaviour
    {
        public NetworkManager NetworkManager => null;
        public bool IsLocalPlayer { get; }
        public bool IsOwner { get; }
        public bool IsServer { get; private set; }
        public bool IsClient { get; private set; }
        public bool IsHost { get; private set; }
        public bool IsSpawned { get; }
        public bool IsOwnedByServer { get; }
        public ulong OwnerClientId { get; }
        public ulong NetworkObjectId { get; }
        public NetworkObject NetworkObject => null;
        public virtual void OnNetworkSpawn() { }
        public virtual void OnNetworkDespawn() { }
        public virtual void OnDestroy() { }
    }

    public class NetworkTransport : MonoBehaviour { }

    public class NetworkConfig
    {
        public NetworkTransport NetworkTransport;
        public GameObject PlayerPrefab;
        public bool ConnectionApproval;
        public byte[] ConnectionData = new byte[0];
        public bool EnableSceneManagement = true;
        public int TickRate = 30;
        public int ClientConnectionBufferTimeout = 10;
        public ushort ProtocolVersion;
    }

    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Singleton { get; private set; }
        public NetworkConfig NetworkConfig = new NetworkConfig();
        public ulong LocalClientId { get; }
        public IReadOnlyList<ulong> ConnectedClientsIds => null;
        public bool IsServer => false;
        public bool IsClient => false;
        public bool IsHost => false;
        public bool IsListening => false;
        public bool IsConnectedClient => false;
        public bool ShutdownInProgress => false;
        public string DisconnectReason => null;
        public NetworkTime ServerTime => default;
        public NetworkTime LocalTime => default;
        public Action<ConnectionApprovalRequest, ConnectionApprovalResponse> ConnectionApprovalCallback { get; set; }
        public event Action<ulong> OnClientConnectedCallback { add { } remove { } }
        public event Action<ulong> OnClientDisconnectCallback { add { } remove { } }
        public event Action OnServerStarted { add { } remove { } }
        public event Action<bool> OnServerStopped { add { } remove { } }
        public event Action<bool> OnClientStopped { add { } remove { } }
        public event Action<NetworkManager, ConnectionEventData> OnConnectionEvent { add { } remove { } }
        public void AddNetworkPrefab(GameObject prefab) { }
        public bool StartHost() => false;
        public bool StartClient() => false;
        public bool StartServer() => false;
        public void Shutdown(bool discardMessageQueue = false) { }
        public void DisconnectClient(ulong clientId) { }
        public void DisconnectClient(ulong clientId, string reason = null) { }

        public class ConnectionApprovalResponse
        {
            public bool Approved;
            public bool CreatePlayerObject;
            public uint? PlayerPrefabHash;
            public Vector3? Position;
            public Quaternion? Rotation;
            public bool Pending;
            public string Reason;
        }

        public struct ConnectionApprovalRequest
        {
            public byte[] Payload;
            public ulong ClientNetworkId;
        }
    }

    public enum ConnectionEvent { ClientConnected, PeerConnected, ClientDisconnected, PeerDisconnected }
    public struct ConnectionEventData { public ConnectionEvent EventType; public ulong ClientId; }
}

namespace Unity.Netcode.Transports.UTP
{
    public class UnityTransport : Unity.Netcode.NetworkTransport
    {
        public void SetConnectionData(string ipv4Address, ushort port, string listenAddress = null) { }
        public void SetConnectionData(bool forceOverrideCommandLineArgs, string ipv4Address, ushort port, string listenAddress = null) { }
        public int MaxConnectAttempts;
        public int ConnectTimeoutMS;
        public int DisconnectTimeoutMS;
    }
}
