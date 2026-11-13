using System.Security.Cryptography;
using System.Text;

namespace ExtensaoZero.Multiplayer
{
    /// <summary>
    /// Chave de sessão aleatória de 6 caracteres gerada com RandomNumberGenerator
    /// (NETWORK_ARCHITECTURE §6). Alfabeto sem caracteres ambíguos (0/O, 1/I).
    /// </summary>
    public static class SessionKeyGenerator
    {
        public const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        public const int Length = 6;

        public static string Generate()
        {
            var bytes = new byte[Length];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(bytes);
            var builder = new StringBuilder(Length);
            for (var i = 0; i < Length; i++) builder.Append(Alphabet[bytes[i] % Alphabet.Length]);
            return builder.ToString();
        }
    }
}
