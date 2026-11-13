using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace ExtensaoZero.Multiplayer
{
    /// <summary>
    /// Descoberta LAN simples por UDP broadcast. Não é usada para conectar sozinha:
    /// a UI sempre permite IP manual. Implementação marcada como não testada em rede real.
    /// </summary>
    public sealed class LanDiscovery : MonoBehaviour
    {
        public event Action<string, ushort> ServerFound;

        private const string Magic = "EZ_DISCOVER_V1";
        private UdpClient m_Client;
        private Thread m_Thread;
        private volatile bool m_Running;
        private ushort m_Port;

        public void StartDiscovery(ushort port)
        {
            StopDiscovery();
            m_Port = port;
            m_Running = true;
            m_Client = new UdpClient { EnableBroadcast = true };
            m_Client.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
            m_Thread = new Thread(ListenLoop) { IsBackground = true, Name = "EZ_LanDiscovery" };
            m_Thread.Start();
            SendProbe();
        }

        public void StopDiscovery()
        {
            m_Running = false;
            try
            {
                m_Client?.Close();
            }
            catch (Exception)
            {
                // Encerramento best-effort; sem estado persistente.
            }

            m_Client = null;
            m_Thread = null;
        }

        private void SendProbe()
        {
            try
            {
                var data = Encoding.UTF8.GetBytes(Magic);
                m_Client?.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, m_Port));
            }
            catch (Exception e)
            {
                Debug.LogWarning("[EZ] LAN discovery: " + e.Message);
            }
        }

        private void ListenLoop()
        {
            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            while (m_Running)
            {
                try
                {
                    var data = m_Client.Receive(ref endpoint);
                    var text = Encoding.UTF8.GetString(data);
                    if (text.StartsWith("EZ_HOST:", StringComparison.Ordinal))
                    {
                        var parts = text.Split(':');
                        if (parts.Length >= 3 && ushort.TryParse(parts[2], out var port))
                        {
                            ServerFound?.Invoke(parts[1], port);
                        }
                    }
                }
                catch (Exception)
                {
                    if (!m_Running) break;
                }
            }
        }

        private void OnDestroy()
        {
            StopDiscovery();
        }
    }
}
