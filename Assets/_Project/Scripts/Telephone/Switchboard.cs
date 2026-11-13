using System.Collections.Generic;
using ExtensaoZero.Core;
using ExtensaoZero.Missions;
using ExtensaoZero.Player;
using ExtensaoZero.UI;
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

        public static bool Matches(int a, int b, int x, int y)
        {
            return (a == x && b == y) || (a == y && b == x);
        }
    }

    /// <summary>
    /// Painel da central: 20 extensões + tronco 0 + linha 9, um cabo por vez. A missão ativa
    /// julga cada conexão; a central apenas executa e mostra.
    /// </summary>
    public sealed class Switchboard : MonoBehaviour
    {
        private readonly List<SwitchboardSocket> m_Sockets = new List<SwitchboardSocket>();
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

        public void SelectSocket(int extension, PlayerEquipment equipment, bool localActor)
        {
            var manager = GameManager.Instance;
            var socket = GetSocket(extension);
            if (socket != null && socket.Compromised && m_Selected < 0)
            {
                manager?.Audio?.PlayWorldOneShot(Audio.ProceduralAudio.RelayClick(), socket.transform.position, 0.8f, Audio.AudioCategory.Sfx);
                if (localActor) manager?.UI?.ShowSubtitle("CENTRAL", $"Extensão {extension} restaurada.", UITheme.Green, 3f);
                manager?.RestoreCompromisedExtension(extension);
                return;
            }

            if (equipment == null || !equipment.HasCable)
            {
                if (localActor) manager?.UI?.ShowSubtitle("CENTRAL", "Você precisa de um cabo para conectar extensões.", UITheme.Red, 3.5f);
                return;
            }

            if (m_Selected < 0)
            {
                m_Selected = extension;
                socket?.SetSelected(true);
                if (localActor)
                {
                    manager?.Audio?.PlayUiClick();
                    manager?.UI?.ShowSubtitle("CENTRAL", $"Extensão {extension:000} selecionada. Escolha o destino.", UITheme.Amber, 3f);
                }

                return;
            }

            if (m_Selected == extension)
            {
                ClearSelection();
                if (localActor) manager?.UI?.ShowSubtitle("CENTRAL", "Seleção cancelada.", UITheme.Dim, 2f);
                return;
            }

            var from = m_Selected;
            ClearSelection();
            Connect(from, extension, equipment);
        }

        private void ClearSelection()
        {
            if (m_Selected >= 0) GetSocket(m_Selected)?.SetSelected(false);
            m_Selected = -1;
        }

        public bool Connect(int a, int b, PlayerEquipment equipment)
        {
            var manager = GameManager.Instance;
            if (!SwitchboardRules.Validate(a, b, out var message))
            {
                manager?.UI?.ShowSubtitle("CENTRAL", message, UITheme.Red, 3f);
                return false;
            }

            if (HasConnection) Disconnect(false);

            m_ConnectedA = a;
            m_ConnectedB = b;
            if (equipment != null) equipment.PlaceCable();

            GetSocket(a)?.SetConnected(true);
            GetSocket(b)?.SetConnected(true);

            if (m_Visual != null)
            {
                var socketA = GetSocket(a);
                var socketB = GetSocket(b);
                m_Visual.SetEndpoints(socketA != null ? socketA.transform : null, socketB != null ? socketB.transform : null);
            }

            manager?.Audio?.PlayWorldOneShot(Audio.ProceduralAudio.RelayClick(), transform.position, 0.9f, Audio.AudioCategory.Sfx);
            var verdict = manager != null ? manager.EvaluateConnection(a, b) : MissionBase.DefaultVerdict(a, b);
            switch (verdict)
            {
                case ConnectionVerdict.Solution:
                    manager?.Announce("CENTRAL", $"Conexão {a:000} → {b:000} estabelecida.", UITheme.Green, 4f);
                    break;
                case ConnectionVerdict.Progress:
                    manager?.Announce("CENTRAL", $"Conexão {a:000} → {b:000}: o medidor do painel reage.", UITheme.Amber, 4f);
                    break;
                case ConnectionVerdict.Contaminated:
                    manager?.Announce("CENTRAL", "A linha aceitou a conexão. O sinal está errado.", UITheme.Red, 4.5f);
                    break;
                case ConnectionVerdict.NoAnswer:
                    manager?.Announce("CENTRAL", $"Conexão {a:000} → {b:000} sem resposta.", UITheme.Amber, 4f);
                    break;
            }

            manager?.OnConnectionResult(verdict, a, b);
            return true;
        }

        public void Disconnect(bool notify = true)
        {
            if (!HasConnection) return;
            var a = m_ConnectedA;
            var b = m_ConnectedB;
            GetSocket(a)?.SetConnected(false);
            GetSocket(b)?.SetConnected(false);
            m_ConnectedA = -1;
            m_ConnectedB = -1;
            if (m_Visual != null) m_Visual.Clear();
            if (!notify) return;
            GameManager.Instance?.UI?.ShowSubtitle("CENTRAL", "Conexão removida.", UITheme.Dim, 2.5f);
            GameManager.Instance?.RaiseEvent($"switch.disconnect:{Mathf.Min(a, b)}:{Mathf.Max(a, b)}");
        }

        public void SetCompromised(int extension, bool compromised)
        {
            GetSocket(extension)?.SetCompromised(compromised);
        }

        public void SetContaminatedMark(int extension, bool contaminated)
        {
            GetSocket(extension)?.SetContaminatedMark(contaminated);
        }

        public void ResetBoard()
        {
            Disconnect(false);
            ClearSelection();
            foreach (var socket in m_Sockets)
            {
                socket.SetCompromised(false);
                socket.SetContaminatedMark(socket.extension == 9);
                socket.SetConnected(false);
            }
        }

        public bool IsConnected(int x, int y)
        {
            return HasConnection && SwitchboardRules.Matches(m_ConnectedA, m_ConnectedB, x, y);
        }

        public string DescribeConnection()
        {
            if (!HasConnection) return "SEM CONEXÃO";
            return $"{m_ConnectedA:000} → {m_ConnectedB:000}";
        }
    }
}
