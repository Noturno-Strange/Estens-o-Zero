using System.Collections.Generic;
using ExtensaoZero.AI;
using ExtensaoZero.Campaign;
using ExtensaoZero.Telephone;
using ExtensaoZero.UI;
using UnityEngine;
using Phone = ExtensaoZero.Telephone.Telephone;

namespace ExtensaoZero.Missions
{
    /// <summary>
    /// Capítulo 3 — A Linha Morta. Túneis de cabos com seis caixas de passagem; três estão
    /// contaminadas (seed). Pistas: sinais (DTMF com os números), ambiente (locais aproximados),
    /// medidor de circuito, LED que pisca e zumbido. A interferência traz uma pista falsa.
    /// Isolar uma caixa limpa derruba a linha de quem errou. Contaminação física cresce com o tempo.
    /// </summary>
    public sealed class DeadLineMission : MissionBase
    {
        public override int ChapterIndex => 3;

        private static readonly string[] BoxPlaces =
        {
            "logo depois da rampa, à esquerda",
            "no túnel principal, à direita",
            "na curva oeste, parede norte",
            "perto do vidro, parede sul",
            "no lado leste, parede norte",
            "no fim do túnel principal, à direita"
        };

        private int[] m_Contaminated = new int[0];
        private readonly HashSet<int> m_Isolated = new HashSet<int>();
        private float m_NextCreep;
        private bool m_ReflectionShown;

        protected override void OnSetupWorld(int seed)
        {
            m_Contaminated = SeedVariation.PickDistinct(seed, "ch3_boxes", 6, 3);
            foreach (var index in m_Contaminated) Local("junction.contaminate:" + index);

            var decoy = 0;
            for (var i = 0; i < 6; i++)
            {
                if (System.Array.IndexOf(m_Contaminated, i) < 0)
                {
                    decoy = i;
                    break;
                }
            }

            var digits = string.Join(" — ", (m_Contaminated[0] + 1).ToString(), (m_Contaminated[1] + 1).ToString(), (m_Contaminated[2] + 1).ToString());
            CallCatalog.Register(CallCatalog.Make("ch3_tunnel", "TÚNEL — EXTENSÃO 218", "DESCONHECIDA",
                "Seis caixas. Ela está em três. As dela zumbem e piscam como se respirassem.",
                $"Zumbido grave {BoxPlaces[m_Contaminated[0]]}. Outro {BoxPlaces[m_Contaminated[1]]}. O terceiro some no eco.",
                $"DTMF: {digits}.",
                $"\"...a caixa {decoy + 1} também... a {decoy + 1}...\" A voz parece ter pressa demais.",
                30f, $"{m_Contaminated[0] + 1}{m_Contaminated[1] + 1}{m_Contaminated[2] + 1}"));
        }

        protected override void OnStart()
        {
            m_Isolated.Clear();
            m_ReflectionShown = false;
            SetStep(0, "Desça ao subsolo pela porta no fim do corredor.");
            Say("RÁDIO — HELENA", CampaignCatalog.Get(3).Briefing.Replace("Helena: ", string.Empty), UITheme.Paper, 8f);
        }

        public override void HandleEvent(string id)
        {
            if (Completed) return;
            if (id == "zone:tunnels" && StepIndex == 0)
            {
                Register("tunnel_boxes");
                SetStep(1, "Um telefone toca nos túneis. Atenda a extensão 218.");
                Ring(218, "ch3_tunnel", 2f);
                return;
            }

            if (id.StartsWith("junction:", System.StringComparison.Ordinal))
            {
                var parts = id.Split(':');
                if (parts.Length < 3 || !int.TryParse(parts[1], out var index)) return;
                if (parts[2] == "contaminated") OnIsolatedContaminated(index);
                else OnIsolatedClean(index);
            }
        }

        private void OnIsolatedContaminated(int index)
        {
            if (!m_Isolated.Add(index)) return;
            Manager?.AddIsolatedCircuit();
            Manager?.Contamination?.Reduce(10f);
            Say("CIRCUITO", $"CX {index + 1} isolada. O zumbido cessa. A linha morta respira mais devagar.", UITheme.Green, 4f);

            if (m_Isolated.Count == 2 && !m_ReflectionShown)
            {
                m_ReflectionShown = true;
                Command("subscriber.manifest:reflection:" + ManifestationMode.Reflection);
                Register("reflection");
            }

            if (m_Isolated.Count >= 3 && StepIndex < 3)
            {
                SetStep(3, "Atenda a extensão 218 uma última vez.");
                Ring(218, "ch3_after", 3f);
            }
        }

        private void OnIsolatedClean(int index)
        {
            Manager?.Contamination?.Add(18f * Difficulty.ContaminationMultiplier, $"caixa limpa {index + 1} isolada");
            Say("CIRCUITO", $"CX {index + 1} estava limpa. A queda puxa a sua linha junto.", UITheme.Red, 5f);
            Manager?.Tension?.ForceEvent(Anomaly.TensionCategory.Contradiction);
        }

        public override void OnPhoneAnswered(Phone phone)
        {
            if (Completed || phone == null || phone.CurrentCall == null) return;
            if (phone.CurrentCall.id == "ch3_tunnel" && StepIndex < 2)
            {
                SetStep(2, "Isole as três caixas contaminadas (CX 1–6).");
                m_NextCreep = Time.time + 30f;
                Hint("Caixas contaminadas piscam em vermelho e zumbem. O medidor confirma.");
            }
        }

        public override void OnCallFinished(Phone phone)
        {
            if (Completed || phone == null) return;
            if (StepIndex == 3 && phone.extension == 218)
            {
                Register("dead_line");
                SetStep(4, "Capítulo concluído.");
                Complete("Três circuitos contaminados foram isolados nos túneis. Uma silhueta apareceu no vidro, atrás do reflexo. " +
                         "A porta da sala de energia continua trancada, e quente.");
            }
        }

        private void Update()
        {
            // Contaminação física: cresce devagar enquanto há caixas contaminadas ativas.
            if (Completed || StepIndex != 2 || Manager == null || !Manager.IsHost || Time.time < m_NextCreep) return;
            m_NextCreep = Time.time + 30f;
            var remaining = 3 - m_Isolated.Count;
            if (remaining > 0) Manager.Contamination?.Add(2.5f * remaining * Difficulty.ContaminationMultiplier, "linha morta");
        }
    }
}
