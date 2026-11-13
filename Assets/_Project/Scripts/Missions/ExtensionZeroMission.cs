using System.Collections.Generic;
using ExtensaoZero.AI;
using ExtensaoZero.Campaign;
using ExtensaoZero.UI;
using Phone = ExtensaoZero.Telephone.Telephone;

namespace ExtensaoZero.Missions
{
    /// <summary>
    /// Capítulo 5 — Extensão Zero. A central original. A verdade sobre o incêndio, o nome de Ana,
    /// a gravação original em quatro camadas e o tronco a restaurar. A Assinante se manifesta por
    /// completo e a equipe escolhe uma de três conclusões (Story Bible §7). Todos os requisitos
    /// podem ser cumpridos dentro deste capítulo, mesmo que algo tenha faltado antes.
    /// </summary>
    public sealed class ExtensionZeroMission : MissionBase
    {
        public override int ChapterIndex => 5;

        private int[] m_Contaminated = new int[0];
        private readonly HashSet<int> m_Isolated = new HashSet<int>();
        private bool m_Armed;

        protected override void OnSetupWorld(int seed)
        {
            m_Contaminated = SeedVariation.PickDistinct(seed, "ch5_circuits", 5, 3);
            foreach (var index in m_Contaminated) Local("circuit.contaminate:" + index);
        }

        protected override void OnStart()
        {
            m_Isolated.Clear();
            m_Armed = false;
            SetStep(0, "Siga a linha zero até a central original, no fim norte do túnel.");
            Say("RÁDIO — HELENA", CampaignCatalog.Get(5).Briefing.Replace("Helena: ", string.Empty), UITheme.Paper, 8f);
            Command("door.unlock:porta_central_original");
        }

        public override void HandleEvent(string id)
        {
            if (Completed || id == null) return;

            if (id == "zone:original" && StepIndex == 0)
            {
                SetStep(1, "Investigue a central original: registros, o painel de 1987 e a gravação.");
                Ring(1000, "ch5_operator", 8f);
                Hint("Três circuitos do painel antigo estão contaminados: isole-os antes de religar o tronco. " +
                     "A gravação original tem quatro camadas: ouçam todas (Q alterna; em grupo, cada um ouve as suas).");
                return;
            }

            if (id.StartsWith("circuit:", System.StringComparison.Ordinal))
            {
                var parts = id.Split(':');
                if (parts.Length < 3 || !int.TryParse(parts[1], out var index)) return;
                if (parts[2] == "contaminated")
                {
                    if (m_Isolated.Add(index))
                    {
                        Manager?.AddIsolatedCircuit();
                        Manager?.Contamination?.Reduce(8f);
                        Say("CIRCUITO", $"Circuito {index + 1} isolado.", UITheme.Green, 3.5f);
                    }
                }
                else
                {
                    Manager?.Contamination?.Add(15f * Difficulty.ContaminationMultiplier, "circuito limpo isolado");
                    Say("CIRCUITO", $"O circuito {index + 1} estava limpo. A sala esfria.", UITheme.Red, 4f);
                }

                return;
            }

            if (id == "restore.trunk")
            {
                RestoreTrunk();
                return;
            }

            if (id.StartsWith("heard:ch5_original:", System.StringComparison.Ordinal))
            {
                var mask = int.TryParse(id.Substring("heard:ch5_original:".Length), out var value) ? value : 0;
                if ((mask & 0xF) == 0xF)
                {
                    Manager?.AddCampaignFlag(EndingRules.FlagOriginalAllLayers);
                    Register("original_recording");
                    Say("GRAVAÇÃO ORIGINAL", "As quatro camadas foram ouvidas. A chamada de 1987 nunca terminou.", UITheme.Amber, 5f);
                }

                return;
            }

            if (id.StartsWith("ending:", System.StringComparison.Ordinal))
            {
                var kind = int.TryParse(id.Substring("ending:".Length), out var k) ? (EndingKind)k : EndingKind.None;
                ChooseEnding(kind);
            }
        }

        public override void OnEvidence(string evidenceId)
        {
            if (evidenceId == "fire_records") Manager?.AddCampaignFlag(EndingRules.FlagFireRecords);
            if (evidenceId == "ana_badge")
            {
                Manager?.AddCampaignFlag(EndingRules.FlagAnaName);
                Say("RÁDIO — NARA", "Ana. Ela tinha dezenove anos. A Celina nunca parou de procurar.", UITheme.Paper, 6f);
            }
        }

        private void RestoreTrunk()
        {
            if (m_Isolated.Count < m_Contaminated.Length)
            {
                World?.Switches["restore.trunk"]?.ResetSwitch();
                Contaminate(35f, 0, 0);
                Say("TRONCO ZERO", "O tronco puxa a contaminação dos circuitos ativos. Isolem os contaminados antes de religar.", UITheme.Red, 6f);
                return;
            }

            Manager?.AddCampaignFlag(EndingRules.FlagRestoredClean);
            Register("restored_clean");
            Say("TRONCO ZERO", "O tronco zero volta à linha original. Limpo. O tom de discagem para.", UITheme.Green, 6f);
        }

        public override void OnPhoneAnswered(Phone phone)
        {
            if (Completed || phone == null || phone.CurrentCall == null) return;
            if (phone.CurrentCall.id == "ch5_operator" && !m_Armed)
            {
                m_Armed = true;
                SetStep(2, "Ela está aqui. Escolham: isolar, restaurar ou atender.");
                Command("subscriber.manifest:original_center:" + ManifestationMode.FullForm);
                Command("ending.arm");
                Manager?.Tension?.AddTension(0.3f);
                Hint("Cada escolha tem um requisito. Os consoles mostram o que falta.");
            }
        }

        private void ChooseEnding(EndingKind kind)
        {
            if (!m_Armed || kind == EndingKind.None) return;
            var save = Manager != null ? Manager.Save : null;
            if (save == null || !EndingRules.IsAvailable(kind, save.flags, save.isolatedCircuits))
            {
                Say(EndingRules.Title(kind), "Requisito: " + EndingRules.Requirement(kind), UITheme.Dim, 6f);
                return;
            }

            Ending = kind;
            Command("subscriber.retreat");
            Manager?.AddCampaignFlag("ending_" + (int)kind);
            SetStep(3, "Fim.");
            Complete(EndingRules.Epilogue(kind), 3f);
        }
    }
}
