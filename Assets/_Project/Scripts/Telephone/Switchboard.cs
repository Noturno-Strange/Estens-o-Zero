using System;
using System.Collections.Generic;
using ExtensaoZero.Core;
using ExtensaoZero.Player;
using UnityEngine;

namespace ExtensaoZero.Telephone
{
    /// <summary>
    /// Regras puras da central. Ficam separadas da geometria para permitir testes EditMode.
    /// </summary>
    public static class SwitchboardRules
    {
        public static bool IsValidExtension(int extension)
        {
            return extension == 0 || extension == 9 || (extension >= 200 && extension <= 219);
        }

        public static bool Validate(int a, int b, out string message)
        {
            if (!IsValidExtension(a) || !IsValidExtension(b))
            {
                message = "Extensão inválida.";
                return false;
            }

            if (a == b)
            {
                message = "Não é possível conectar uma extensão a ela mesma.";
                return false;
            }

            message = "Conexão física aceita.";
            return true;
        }

        public static bool IsContaminated(int a, int b)
        {
            return (a == 9 || b == 9) && a != b;
        }

        public static bool IsSolution(int a, int b)
        {
            return (a == 214 && b == 0) || (a == 0 && b == 214);
        }
    }

    public sealed class Switchboard : MonoBehaviour
    {
        private readonly List<SwitchboardSocket> m_Sockets = new List<SwitchboardSocket>();
        private CableItem m_Cable;
        private int m_Selected = -1;
        private int m_ConnectedA = -1;
        private int m_ConnectedB = -1;
        private CableVisual m_Visual;

        public bool HasConnection => m_ConnectedA >= 0 && m_ConnectedB >= 0;
        public int ConnectedA => m_ConnectedA;
        public int ConnectedB => m_ConnectedB;
        public int SelectedExtension => m_Selected;
        public IReadOnlyList<SwitchboardSocket> Sockets => m_Sockets;

        public void RegisterSocket(SwitchboardSocket socket)
        {
            if (socket != null && !m_Sockets.Contains(socket)) m_Sockets.Add(socket);
        }

        public SwitchboardSocket GetSocket(int extension)
        {
            for (var i = 0; i < m_Sockets.Count; i++)
            {
                if (m_Sockets[i].extension == extension) return m_Sockets[i];
            }

            return null;
        }

        public void AttachVisual(CableVisual visual)
        {
            m_Visual = visual;
        }

        public void SelectSocket(int extension, PlayerEquipment equipment)
        {
            var manager = GameManager.Instance;
            if (equipment == null || !equipment.HasCable)
            {
                manager?.UI?.ShowSubtitle("CENTRAL", "Você precisa de um cabo para conectar extensões.", UI.UITheme.Red, 3.5f);
                return;
            }

            if (m_Selected < 0)
            {
                m_Selected = extension;
                manager?.Audio?.PlayUiClick();
                manager?.UI?.ShowSubtitle("CENTRAL", $"Extensão {extension:000} selecionada. Escolha o destino.", UI.UITheme.Amber, 3f);
                return;
            }

            if (m_Selected == extension)
            {
                m_Selected = -1;
                manager?.UI?.ShowSubtitle("CENTRAL", "Seleção cancelada.", UI.UITheme.Dim, 2f);
                return;
            }

            var from = m_Selected;
            m_Selected = -1;
            Connect(from, extension, equipment);
        }

        public bool Connect(int a, int b, PlayerEquipment equipment)
        {
            if (!SwitchboardRules.Validate(a, b, out var message))
            {
                GameManager.Instance?.UI?.ShowSubtitle("CENTRAL", message, UI.UITheme.Red, 3f);
                return false;
            }

            if (HasConnection) Disconnect(false);

            m_ConnectedA = a;
            m_ConnectedB = b;
            m_Cable = equipment != null ? equipment.CurrentCable : null;
            if (equipment != null) equipment.PlaceCable();

            GetSocket(a)?.SetConnected(true);
            GetSocket(b)?.SetConnected(true);

            if (m_Visual != null)
            {
                var socketA = GetSocket(a);
                var socketB = GetSocket(b);
                m_Visual.SetEndpoints(socketA != null ? socketA.transform : null, socketB != null ? socketB.transform : null);
            }

            var correct = SwitchboardRules.IsSolution(a, b);
            var contaminated = SwitchboardRules.IsContaminated(a, b);
            var manager = GameManager.Instance;
            manager?.Audio?.PlayWorldOneShot(Audio.ProceduralAudio.RelayClick(), transform.position, 0.9f, Audio.AudioCategory.Sfx);
            manager?.OnConnectionResult(correct, a, b);

            if (correct)
            {
                manager?.UI?.ShowSubtitle("CENTRAL", $"Conexão {a:000} → {b:000} estabelecida.", UI.UITheme.Green, 4f);
            }
            else if (contaminated)
            {
                manager?.UI?.ShowSubtitle("CENTRAL", "A linha 9 aceitou a conexão. O sinal está errado.", UI.UITheme.Red, 4.5f);
            }
            else
            {
                manager?.UI?.ShowSubtitle("CENTRAL", $"Conexão {a:000} → {b:000} sem resposta.", UI.UITheme.Amber, 4f);
            }

            return true;
        }

        public void Disconnect(bool notify = true)
        {
            if (!HasConnection) return;
            GetSocket(m_ConnectedA)?.SetConnected(false);
            GetSocket(m_ConnectedB)?.SetConnected(false);
            m_ConnectedA = -1;
            m_ConnectedB = -1;
            m_Cable = null;
            if (m_Visual != null) m_Visual.Clear();
            if (notify)
            {
                GameManager.Instance?.UI?.ShowSubtitle("CENTRAL", "Conexão removida.", UI.UITheme.Dim, 2.5f);
            }
        }

        public string DescribeConnection()
        {
            if (!HasConnection) return "SEM CONEXÃO";
            return $"{m_ConnectedA:000} → {m_ConnectedB:000}";
        }
    }
}
