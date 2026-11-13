using System.Text;

namespace ExtensaoZero.Multiplayer
{
    /// <summary>
    /// Normalização e hash da chave de sessão. A chave nunca é enviada em texto claro:
    /// o cliente envia o hash e o host compara. Não é criptografia; é controle de acesso
    /// casual para uma sessão LAN entre pessoas que combinaram a chave.
    /// </summary>
    public static class SessionCodec
    {
        public const int MinLength = 4;
        public const int MaxLength = 24;

        public static string Normalize(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return string.Empty;
            var builder = new StringBuilder(key.Length);
            foreach (var c in key.Trim().ToUpperInvariant())
            {
                if (char.IsLetterOrDigit(c) || c == '-') builder.Append(c);
            }

            if (builder.Length > MaxLength) builder.Length = MaxLength;
            return builder.ToString();
        }

        public static bool IsValid(string key)
        {
            var normalized = Normalize(key);
            return normalized.Length >= MinLength && normalized.Length <= MaxLength;
        }

        public static uint Hash(string key)
        {
            var normalized = Normalize(key);
            unchecked
            {
                var hash = 2166136261u;
                foreach (var c in normalized)
                {
                    hash ^= c;
                    hash *= 16777619u;
                }

                return hash;
            }
        }

        public static string HashHex(string key)
        {
            return Hash(key).ToString("x8");
        }
    }
}
