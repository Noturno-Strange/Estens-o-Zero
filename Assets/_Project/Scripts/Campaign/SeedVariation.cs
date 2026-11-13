using System;
using System.Collections.Generic;

namespace ExtensaoZero.Campaign
{
    /// <summary>
    /// Variações controladas pela seed do host (GDD §14). Determinístico: a mesma seed gera o
    /// mesmo resultado em todos os peers. Nunca produz combinação impossível.
    /// </summary>
    public static class SeedVariation
    {
        /// <summary>Escolhe um índice em [0, count).</summary>
        public static int Pick(int seed, string salt, int count)
        {
            if (count <= 1) return 0;
            return (int)(Mix(seed, salt) % (uint)count);
        }

        /// <summary>Escolhe <paramref name="take"/> índices distintos em [0, count), em ordem crescente.</summary>
        public static int[] PickDistinct(int seed, string salt, int count, int take)
        {
            take = Math.Max(0, Math.Min(take, count));
            var pool = new List<int>(count);
            for (var i = 0; i < count; i++) pool.Add(i);
            var random = new Random((int)Mix(seed, salt));
            for (var i = pool.Count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                var tmp = pool[i];
                pool[i] = pool[j];
                pool[j] = tmp;
            }

            var result = pool.GetRange(0, take);
            result.Sort();
            return result.ToArray();
        }

        /// <summary>Embaralha uma cópia da lista de forma determinística.</summary>
        public static List<T> Shuffle<T>(int seed, string salt, IList<T> items)
        {
            var copy = new List<T>(items);
            var random = new Random((int)Mix(seed, salt));
            for (var i = copy.Count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                var tmp = copy[i];
                copy[i] = copy[j];
                copy[j] = tmp;
            }

            return copy;
        }

        public static int NewSeed()
        {
            var bytes = new byte[4];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create()) rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0) & 0x7FFFFFFF;
        }

        private static uint Mix(int seed, string salt)
        {
            unchecked
            {
                var hash = 2166136261u ^ (uint)seed;
                hash *= 16777619u;
                if (!string.IsNullOrEmpty(salt))
                {
                    foreach (var c in salt)
                    {
                        hash ^= c;
                        hash *= 16777619u;
                    }
                }

                hash ^= hash >> 15;
                hash *= 2246822519u;
                hash ^= hash >> 13;
                return hash & 0x7FFFFFFF;
            }
        }
    }
}
