using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace ExtensaoZero.Multiplayer
{
    /// <summary>
    /// Descoberta LAN por UDP broadcast (NETWORK_ARCHITECTURE §4). O host anuncia um beacon a
    /// cada 2 s na porta 47777; o cliente escuta por 10 s em uma thread e entrega os resultados
    /// na thread principal. Não é requisito: a conexão manual por IP está sempre disponível.
    /// </summary>
    public sealed class LanDiscovery : MonoBehaviour
    {
        public struct Found
        {
            public string Address;
            public LanBeacon Beacon;
        }

        public event Action<Found> SessionFound;
        public event Action SearchFinished;

        public bool Searching { get; private set; }

        private Func<LanBeacon> m_BeaconSource;
        private UdpClient m_Sender;
        private UdpClient m_Listener;
        private Thread m_Thread;
        private volatile bool m_Listening;
        private float m_NextBeacon;
        private float m_SearchUntil;
        private readonly ConcurrentQueue<Found> m_Queue = new ConcurrentQueue<Found>();

        public void StartBeacon(Func<LanBeacon> source)
        {
            StopBeacon();
            m_BeaconSource = source;
            try
            {
                m_Sender = new UdpClient { EnableBroadcast = true };
            }
            catch (Exception e)
            {
                Debug.LogWarning("[EZ] Beacon LAN indisponível: " + e.Message);
                m_Sender = null;
            }

            m_NextBeacon = 0f;
        }

        public void StopBeacon()
        {
            m_BeaconSource = null;
            try
            {
                m_Sender?.Close();
            }
            catch (Exception)
            {
                // Encerramento best-effort.
            }

            m_Sender = null;
        }

        public void StartSearch(float seconds = 10f)
        {
            StopSearch();
            try
            {
                m_Listener = new UdpClient();
                m_Listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                m_Listener.Client.Bind(new IPEndPoint(IPAddress.Any, LanBeacon.DiscoveryPort));
            }
            catch (Exception e)
            {
                Debug.LogWarning("[EZ] Não foi possível escutar a porta de descoberta: " + e.Message);
                m_Listener = null;
                SearchFinished?.Invoke();
                return;
            }

            m_Listening = true;
            Searching = true;
            m_SearchUntil = Time.unscaledTime + seconds;
            m_Thread = new Thread(ListenLoop) { IsBackground = true, Name = "EZ_LanDiscovery" };
            m_Thread.Start();
        }

        public void StopSearch()
        {
            m_Listening = false;
            Searching = false;
            try
            {
                m_Listener?.Close();
            }
            catch (Exception)
            {
                // Encerramento best-effort.
            }

            m_Listener = null;
            m_Thread = null;
        }

        public void StopAll()
        {
            StopBeacon();
            StopSearch();
        }

        private void ListenLoop()
        {
            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            while (m_Listening)
            {
                try
                {
                    var data = m_Listener.Receive(ref endpoint);
                    var text = Encoding.UTF8.GetString(data);
                    if (LanBeacon.TryParse(text, out var beacon))
                    {
                        m_Queue.Enqueue(new Found { Address = endpoint.Address.ToString(), Beacon = beacon });
                    }
                }
                catch (Exception)
                {
                    if (!m_Listening) break;
                }
            }
        }

        private void Update()
        {
            while (m_Queue.TryDequeue(out var found)) SessionFound?.Invoke(found);

            if (Searching && Time.unscaledTime >= m_SearchUntil)
            {
                StopSearch();
                SearchFinished?.Invoke();
            }

            if (m_Sender == null || m_BeaconSource == null || Time.unscaledTime < m_NextBeacon) return;
            m_NextBeacon = Time.unscaledTime + 2f;
            try
            {
                var payload = Encoding.UTF8.GetBytes(m_BeaconSource().Format());
                m_Sender.Send(payload, payload.Length, new IPEndPoint(IPAddress.Broadcast, LanBeacon.DiscoveryPort));
            }
            catch (Exception e)
            {
                Debug.LogWarning("[EZ] Falha ao enviar beacon LAN: " + e.Message);
                m_NextBeacon = Time.unscaledTime + 10f;
            }
        }

        private void OnDestroy()
        {
            StopAll();
        }
    }
}
