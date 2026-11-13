using System;
using System.Globalization;

namespace ExtensaoZero.Multiplayer
{
    /// <summary>
    /// Beacon de descoberta LAN: <c>EZLAN1|name|port|playerCount|missionStarted|fingerprint</c>.
    /// O fingerprint identifica a sessão; não deriva da chave (a chave nunca é anunciada).
    /// </summary>
    public struct LanBeacon
    {
        public const string Magic = "EZLAN1";
        public const ushort DiscoveryPort = 47777;

        public string Name;
        public ushort Port;
        public int PlayerCount;
        public bool MissionStarted;
        public string Fingerprint;

        public string Format()
        {
            return string.Join("|", Magic, Sanitize(Name), Port.ToString(CultureInfo.InvariantCulture),
                PlayerCount.ToString(CultureInfo.InvariantCulture), MissionStarted ? "1" : "0", Sanitize(Fingerprint));
        }

        public static bool TryParse(string text, out LanBeacon beacon)
        {
            beacon = default;
            if (string.IsNullOrEmpty(text)) return false;
            var parts = text.Split('|');
            if (parts.Length != 6 || parts[0] != Magic) return false;
            if (!ushort.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var port) || port < 1024) return false;
            if (!int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var count) || count < 0 || count > 4) return false;
            if (parts[4] != "0" && parts[4] != "1") return false;
            beacon = new LanBeacon
            {
                Name = parts[1],
                Port = port,
                PlayerCount = count,
                MissionStarted = parts[4] == "1",
                Fingerprint = parts[5]
            };
            return true;
        }

        private static string Sanitize(string value)
        {
            if (string.IsNullOrEmpty(value)) return "-";
            var clean = value.Replace("|", "/").Trim();
            return clean.Length > 32 ? clean.Substring(0, 32) : clean;
        }
    }
}
