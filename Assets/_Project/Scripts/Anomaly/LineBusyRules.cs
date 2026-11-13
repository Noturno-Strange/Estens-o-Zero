using System;

namespace ExtensaoZero.Anomaly
{
    /// <summary>
    /// Regras puras da Linha Ocupada (GDD §10): limiar, duração 15–25 s e frequência de
    /// retorno determinística. Nunca é punição permanente.
    /// </summary>
    public static class LineBusyRules
    {
        public const float Threshold = 55f;
        public const float MinSeconds = 15f;
        public const float MaxSeconds = 25f;
        public const int FrequencySteps = 10;

        public static float ClampDuration(float seconds)
        {
            return Math.Max(MinSeconds, Math.Min(MaxSeconds, seconds));
        }

        public static bool ShouldTrigger(float before, float after, bool alreadyBusy)
        {
            return !alreadyBusy && before < Threshold && after >= Threshold;
        }

        /// <summary>Frequência de retorno (0–9), diferente da frequência inicial do sintonizador.</summary>
        public static int ReturnFrequency(int seed, int occurrence, int startFrequency)
        {
            unchecked
            {
                var value = (uint)(seed * 31 + occurrence * 7919 + 17);
                value ^= value >> 7;
                var frequency = (int)(value % FrequencySteps);
                if (frequency == startFrequency) frequency = (frequency + 3) % FrequencySteps;
                return frequency;
            }
        }

        public static int Step(int frequency, int delta)
        {
            var next = (frequency + delta) % FrequencySteps;
            return next < 0 ? next + FrequencySteps : next;
        }
    }
}
