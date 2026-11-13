using System;

namespace ExtensaoZero.Multiplayer
{
    /// <summary>
    /// Payload de aprovação: "EZ" + versão do protocolo (ushort) + hash da chave (uint).
    /// O host rejeita versão incompatível, chave errada, sessão cheia e missão iniciada.
    /// </summary>
    public static class ConnectionPayload
    {
        public const ushort ProtocolVersion = 2;
        public const int MaxPlayers = 4;

        public enum Verdict
        {
            Approved,
            Malformed,
            VersionMismatch,
            WrongKey,
            SessionFull,
            MissionStarted
        }

        public static byte[] Build(string sessionKey)
        {
            var bytes = new byte[8];
            bytes[0] = (byte)'E';
            bytes[1] = (byte)'Z';
            var version = BitConverter.GetBytes(ProtocolVersion);
            bytes[2] = version[0];
            bytes[3] = version[1];
            var hash = BitConverter.GetBytes(SessionCodec.Hash(sessionKey));
            Array.Copy(hash, 0, bytes, 4, 4);
            return bytes;
        }

        public static Verdict Evaluate(byte[] payload, uint expectedHash, int connectedPlayers, bool missionStarted)
        {
            if (payload == null || payload.Length < 8 || payload[0] != (byte)'E' || payload[1] != (byte)'Z') return Verdict.Malformed;
            if (BitConverter.ToUInt16(payload, 2) != ProtocolVersion) return Verdict.VersionMismatch;
            if (BitConverter.ToUInt32(payload, 4) != expectedHash) return Verdict.WrongKey;
            if (connectedPlayers >= MaxPlayers) return Verdict.SessionFull;
            if (missionStarted) return Verdict.MissionStarted;
            return Verdict.Approved;
        }

        public static string Reason(Verdict verdict)
        {
            switch (verdict)
            {
                case Verdict.Malformed: return "Pedido de conexão inválido.";
                case Verdict.VersionMismatch: return "Versão do jogo incompatível com o host.";
                case Verdict.WrongKey: return "Chave de sessão incorreta.";
                case Verdict.SessionFull: return "Sessão cheia (máximo de 4 jogadores).";
                case Verdict.MissionStarted: return "A missão já começou; entrada bloqueada.";
                default: return string.Empty;
            }
        }
    }
}
