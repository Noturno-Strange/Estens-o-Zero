using ExtensaoZero.AI;
using ExtensaoZero.Campaign;
using ExtensaoZero.Telephone;
using ExtensaoZero.UI;
using Phone = ExtensaoZero.Telephone.Telephone;

namespace ExtensaoZero.Missions
{
    /// <summary>
    /// Capítulo 2 — Arquivo de Vozes. Duas fitas da mesma ligação (1979 e 1987) com números
    /// diferentes. A escala de plantão de 1987 confirma a versão verdadeira (212); a versão
    /// alterada (207) contamina. O gravador permite comparar as versões; sem ele, o caderno
    /// registra as duas transcrições e a diferença.
    /// </summary>
    public sealed class VoiceArchiveMission : MissionBase
    {
        public override int ChapterIndex => 2;

        private const int TrueExtension = 212;
        private const int AlteredExtension = 207;

        private bool m_Heard1979;
        private bool m_Heard1987;
        private bool m_Roster;
        private bool m_Connected;

        protected override void OnSetupWorld(int seed)
        {
            Local("pickup.show:CABO 214:cable_dead_archive");
        }

        protected override void OnStart()
        {
            m_Heard1979 = false;
            m_Heard1987 = false;
            m_Roster = false;
            m_Connected = false;
            SetStep(0, "Encontre as fitas no arquivo morto (porta lateral do corredor).");
            Say("RÁDIO — HELENA", CampaignCatalog.Get(2).Briefing.Replace("Helena: ", string.Empty), UITheme.Paper, 8f);
            Hint("O gravador de rolo fica na mesa do arquivo morto. Ouça as duas fitas.");
        }

        public override void HandleEvent(string id)
        {
            if (Completed) return;
            if (id == "tape:tape_1979")
            {
                m_Heard1979 = true;
                Register("tape_1979");
                if (StepIndex < 1) SetStep(1, "Ouça a outra fita e compare as versões.");
                CheckComparison();
            }
            else if (id == "tape:tape_1987")
            {
                m_Heard1987 = true;
                Register("tape_1987");
                Register("celina_voice");
                Say("RÁDIO — NARA", "Essa voz... é a Celina Prado. Minha tia-avó trabalhou aqui. Ninguém da família fala dela.", UITheme.Paper, 7f);
                if (StepIndex < 1) SetStep(1, "Ouça a outra fita e compare as versões.");
                CheckComparison();
            }
            else if (id == "examine:cut_order_1998")
            {
                Manager?.AddCampaignFlag("zero_never_cut");
            }
        }

        public override void OnEvidence(string evidenceId)
        {
            if (Completed) return;
            if (evidenceId == "duty_roster_1987")
            {
                m_Roster = true;
                CheckComparison();
            }
            else if (evidenceId == "tape_diff")
            {
                CheckComparison();
            }
        }

        private void CheckComparison()
        {
            if (m_Connected) return;
            if (m_Heard1979 && m_Heard1987 && StepIndex < 2)
            {
                Register("tape_diff");
                SetStep(2, "As versões discordam. Procure um documento de 1987 que confirme o número.");
                Manager?.Tension?.AddTension(0.1f);
            }

            if (StepIndex == 2 && m_Roster)
            {
                SetStep(3, "Conecte a extensão da emergência de 1987 ao tronco 0 (você precisa do cabo).");
                Hint("A escala de plantão de 1987 diz por qual extensão a emergência foi roteada.");
            }
        }

        public override ConnectionVerdict EvaluateConnection(int a, int b)
        {
            if (SwitchboardRules.IsContaminated(a, b)) return ConnectionVerdict.Contaminated;
            if (SwitchboardRules.Matches(a, b, TrueExtension, 0)) return ConnectionVerdict.Solution;
            if (SwitchboardRules.Matches(a, b, AlteredExtension, 0)) return ConnectionVerdict.Contaminated;
            return ConnectionVerdict.NoAnswer;
        }

        public override void OnConnectionResult(ConnectionVerdict verdict, int a, int b)
        {
            if (Completed) return;
            if (verdict == ConnectionVerdict.Contaminated)
            {
                Contaminate(40f, a, b);
                Say("ERRO DE CONEXÃO", a == AlteredExtension || b == AlteredExtension
                    ? "A 207 atende com a voz da fita, mas o sinal volta errado. Essa é a versão que ela alterou."
                    : "A linha 9 contaminou o circuito.", UITheme.Red, 7f);
                return;
            }

            if (verdict != ConnectionVerdict.Solution || m_Connected) return;
            m_Connected = true;
            SetStep(4, "Algo responde no arquivo morto. Atenda a extensão 207.");
            Say("CENTRAL", "O tronco zero aceita a 212. Um telefone toca no fim do corredor.", UITheme.Green, 5f);
            Ring(AlteredExtension, "ch2_reply", 2.5f);
        }

        public override void OnPhoneAnswered(Phone phone)
        {
            if (Completed || phone == null || phone.CurrentCall == null) return;
            if (phone.CurrentCall.id == "ch2_reply")
            {
                Register("ana_voice");
                SetStep(5, "Ouça até o fim.");
            }
        }

        public override void OnCallFinished(Phone phone)
        {
            if (Completed || phone == null) return;
            if (StepIndex == 5 && phone.extension == AlteredExtension)
            {
                Command("subscriber.manifest:archive_dead:" + ManifestationMode.Silhouette);
                SetStep(6, "Volte à central e isole a linha.");
                Manager?.Tension?.AddTension(0.2f);
            }
        }

        public override void OnIsolatorPulled()
        {
            if (Completed) return;
            if (StepIndex < 6)
            {
                Say("CENTRAL", "Ainda não. A linha precisa terminar o que começou.", UITheme.Dim, 4f);
                return;
            }

            Command("isolator.lock");
            Manager?.Contamination?.Reduce(30f);
            SetStep(7, "Capítulo concluído.");
            Complete("As duas fitas registram a mesma ligação com números trocados. A versão de 1987 era a verdadeira. " +
                     "Uma voz jovem chamou \"mãe\" pela extensão 207. A ordem de corte do tronco zero nunca foi executada.");
        }
    }
}
