using System;
using System.Globalization;
using System.Text;

namespace ExtensaoZero.Multiplayer
{
    /// <summary>Dados de conexão contidos em um convite.</summary>
    public struct InviteData
    {
        public int Version;
        public string Ip;
        public ushort Port;
        public string Key;
    }

    /// <summary>
    /// Convite colável (NETWORK_ARCHITECTURE §3): <c>EZ1:&lt;base64url(JSON)&gt;</c> com
    /// JSON = {"v":1,"ip":"...","port":7777,"key":"..."}. É só uma representação dos dados
    /// de conexão: não descobre nada sozinho e não é criptografia. Lógica pura.
    /// </summary>
    public static class InviteCodec
    {
        public const string Prefix = "EZ1:";
        public const int Version = 1;

        public static string Encode(string ip, ushort port, string key)
        {
            var json = new StringBuilder();
            json.Append("{\"v\":").Append(Version.ToString(CultureInfo.InvariantCulture));
            json.Append(",\"ip\":\"").Append(Escape(ip ?? string.Empty)).Append('"');
            json.Append(",\"port\":").Append(port.ToString(CultureInfo.InvariantCulture));
            json.Append(",\"key\":\"").Append(Escape(SessionCodec.Normalize(key))).Append("\"}");
            return Prefix + ToBase64Url(Encoding.UTF8.GetBytes(json.ToString()));
        }

        public static bool TryDecode(string invite, out InviteData data, out string error)
        {
            data = default;
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(invite))
            {
                error = "Convite vazio.";
                return false;
            }

            invite = invite.Trim();
            if (!invite.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            {
                error = "Convite inválido: deve começar com EZ1:.";
                return false;
            }

            string json;
            try
            {
                json = Encoding.UTF8.GetString(FromBase64Url(invite.Substring(Prefix.Length)));
            }
            catch (FormatException)
            {
                error = "Convite corrompido.";
                return false;
            }

            if (!TryReadInt(json, "v", out var version))
            {
                error = "Convite sem versão.";
                return false;
            }

            if (version != Version)
            {
                error = $"Versão de convite incompatível ({version}).";
                return false;
            }

            if (!TryReadString(json, "ip", out var ip) || string.IsNullOrWhiteSpace(ip))
            {
                error = "Convite sem endereço.";
                return false;
            }

            if (!TryReadInt(json, "port", out var port) || port < 1024 || port > 65535)
            {
                error = "Porta inválida no convite.";
                return false;
            }

            if (!TryReadString(json, "key", out var key) || !SessionCodec.IsValid(key))
            {
                error = "Chave inválida no convite.";
                return false;
            }

            data = new InviteData { Version = version, Ip = ip.Trim(), Port = (ushort)port, Key = SessionCodec.Normalize(key) };
            return true;
        }

        private static string Escape(string value)
        {
            var builder = new StringBuilder(value.Length);
            foreach (var c in value)
            {
                if (c == '"' || c == '\\') builder.Append('\\');
                if (c >= ' ') builder.Append(c);
            }

            return builder.ToString();
        }

        private static bool TryReadString(string json, string field, out string value)
        {
            value = null;
            var token = "\"" + field + "\":\"";
            var start = json.IndexOf(token, StringComparison.Ordinal);
            if (start < 0) return false;
            start += token.Length;
            var builder = new StringBuilder();
            for (var i = start; i < json.Length; i++)
            {
                var c = json[i];
                if (c == '\\' && i + 1 < json.Length)
                {
                    builder.Append(json[++i]);
                    continue;
                }

                if (c == '"')
                {
                    value = builder.ToString();
                    return true;
                }

                builder.Append(c);
            }

            return false;
        }

        private static bool TryReadInt(string json, string field, out int value)
        {
            value = 0;
            var token = "\"" + field + "\":";
            var start = json.IndexOf(token, StringComparison.Ordinal);
            if (start < 0) return false;
            start += token.Length;
            var end = start;
            while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '-')) end++;
            return end > start && int.TryParse(json.Substring(start, end - start), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private static string ToBase64Url(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static byte[] FromBase64Url(string text)
        {
            var s = text.Trim().Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
                case 1: throw new FormatException();
            }

            return Convert.FromBase64String(s);
        }
    }
}
