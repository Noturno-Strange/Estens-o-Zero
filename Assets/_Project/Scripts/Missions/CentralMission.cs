using System.Collections.Generic;
using ExtensaoZero.Campaign;
using ExtensaoZero.Telephone;
using ExtensaoZero.UI;
using Phone = ExtensaoZero.Telephone.Telephone;

namespace ExtensaoZero.Missions
{
    /// <summary>
    /// Capítulo 1 — A Central. Três extensões que não existem mais chamam a central. A equipe
    /// rastreia cada uma pelo tronco 0, atende o telefone de origem e isola o circuito que a
    /// chamada denunciou como contaminado. A ordem e o circuito contaminado vêm da seed.
    /// </summary>
    public sealed class CentralMission : MissionBase
    {
        public override int ChapterIndex => 1;

        private static readonly int[] Phantoms = { 205, 211, 216 };
        private static readonly string[] CableAnchors = { "cable_drawer_top", "cable_archive_table", "cable_archive_shelf" };

        private List<int> m_Order = new List<int>();
        private int m_Contaminated;
        private int m_Current;
        private int m_Traced;
        private bool m_WaitingSource;

        protected override void OnSetupWorld(int seed)
        {
            m_Order = SeedVariation.Shuffle(seed, "ch1_order", Phantoms);
            m_Contaminated = m_Order[SeedVariation.Pick(seed, "ch1_contaminated", m_Order.Count)];
            Local("pickup.show:CABO 214:" + CableAnchors[SeedVariation.Pick(seed, "ch1_cable", CableAnchors.Length)]);

            foreach (var ext in Phantoms)
            {
                var call = CallCatalog.Make("ch1_source_" + ext, "TELEFONE — " + LocationOf(ext), "DESCONHECIDA",
                    SourceVoice(ext), SourceAmbience(ext), "Tom contínuo. Nenhum dígito.",
                    ext == m_Contaminated ? "\"...a nove... estou presa na nove... me desliga daqui...\"" : "Estática limpa.",
                    18f, "");
                CallCatalog.Register(call);
            }

            CallCatalog.Register(CallCatalog.Make("ch1_final", "CENTRAL — A VOZ RESPONDE", "DESCONHECIDA",
                $"Primeiro {Place(m_Order[0])}. Depois {Place(m_Order[1])}. Por último {Place(m_Order[2])}. Eu estava em todos.",
                "Os mesmos três telefones tocando ao mesmo tempo, muito longe.",
                "DTMF: " + string.Join(" — ", m_Order[0] % 10, m_Order[1] % 10, m_Order[2] % 10) + ".",
                "\"...vocês fazem tudo na ordem que eu peço...\"",
                20f, $"{m_Order[0] % 10}{m_Order[1] % 10}{m_Order[2] % 10}"));
        }

        protected override void OnStart()
        {
            m_Current = -1;
            m_Traced = 0;
            m_WaitingSource = false;
            SetStep(0, "Atenda o telefone da central.");
            Say("RÁDIO — HELENA", CampaignCatalog.Get(1).Briefing.Replace("Helena: ", string.Empty), UITheme.Paper, 8f);
            Ring(0, "ch1_intro", 3f);
            Hint("Você vai precisar do cabo: ele foi guardado no arquivo de vozes.");
        }

        public override ConnectionVerdict EvaluateConnection(int a, int b)
        {
            if (SwitchboardRules.IsContaminated(a, b)) return ConnectionVerdict.Contaminated;
            if (StepIndex >= 2 && StepIndex <= 3 && m_Current >= 0 && SwitchboardRules.Matches(a, b, m_Order[m_Current], 0)) return ConnectionVerdict.Progress;
            if (StepIndex == 5 && (a == m_Contaminated || b == m_Contaminated)) return ConnectionVerdict.Progress;
            return ConnectionVerdict.NoAnswer;
        }

        public override void OnConnectionResult(ConnectionVerdict verdict, int a, int b)
        {
            if (Completed) return;
            if (verdict == ConnectionVerdict.Contaminated)
            {
                Contaminate(45f, a, b);
                Say("ERRO DE CONEXÃO", "A linha 9 contaminou o circuito. Desconecte (G) e tente de novo.", UITheme.Red, 6f);
                return;
            }

            if (verdict != ConnectionVerdict.Progress) return;
            if (StepIndex == 5)
            {
                Say("CENTRAL", $"O circuito {m_Contaminated} está no cabo. Puxe a alavanca de isolamento.", UITheme.Amber, 5f);
                return;
            }

            var ext = m_Order[m_Current];
            m_WaitingSource = true;
            SetStep(3, $"Atenda o telefone que está tocando: {LocationOf(ext)}.");
            Say("MEDIDOR DO PAINEL", $"Rastreio da {ext}: a agulha aponta para {LocationOf(ext)}.", UITheme.Amber, 6f);
            Ring(ext, "ch1_source_" + ext, 1.5f);
        }

        public override void OnPhoneAnswered(Phone phone)
        {
            if (Completed || phone == null) return;
            var callId = phone.CurrentCall != null ? phone.CurrentCall.id : string.Empty;

            if (callId == "ch1_intro")
            {
                SetStep(1, "Ouça a central. Três extensões fora de serviço estão chamando.");
                Register("ch1_traffic");
                return;
            }

            if (callId.StartsWith("ch1_phantom_", System.StringComparison.Ordinal))
            {
                SetStep(2, "Descubra qual extensão está chamando e rastreie-a: conecte-a ao tronco 0.");
                Hint($"A camada de sinais traz os três dígitos da extensão ({m_Order[m_Current]}).");
                return;
            }

            if (callId.StartsWith("ch1_source_", System.StringComparison.Ordinal) && m_WaitingSource && phone.extension == m_Order[m_Current])
            {
                m_WaitingSource = false;
                m_Traced++;
                Register("phantom_" + phone.extension);
                if (phone.extension == m_Contaminated)
                {
                    Register("ch1_contaminated");
                    Command($"socket.mark:{m_Contaminated}:1");
                }

                return;
            }

            if (callId == "ch1_final")
            {
                Register("ch1_order");
            }
        }

        public override void OnCallFinished(Phone phone)
        {
            if (Completed || phone == null) return;
            if (StepIndex == 1 && phone.extension == 0 && m_Current < 0)
            {
                NextPhantom(2f);
                return;
            }

            if (StepIndex == 3 && !m_WaitingSource && phone.extension == m_Order[m_Current])
            {
                if (m_Traced < m_Order.Count)
                {
                    SetStep(2, "Volte à central: outra extensão vai chamar.");
                    NextPhantom(3f);
                }
                else
                {
                    SetStep(5, "Isole o circuito contaminado: conecte-o no painel e puxe a alavanca de isolamento.");
                    Say("CENTRAL", "Uma das três extensões mencionou a linha nove. O LED dela está vermelho no painel.", UITheme.Amber, 7f);
                    Manager?.Tension?.AddTension(0.15f);
                }

                return;
            }

            if (StepIndex == 6 && phone.extension == 0)
            {
                SetStep(7, "Capítulo concluído.");
                Complete("Três extensões fantasmas foram rastreadas até telefones reais. O circuito contaminado foi isolado. " +
                         "A voz descreveu a ordem exata dos passos da equipe.");
            }
        }

        private void NextPhantom(float delay)
        {
            m_Current++;
            if (m_Current >= m_Order.Count) return;
            Ring(0, "ch1_phantom_" + m_Order[m_Current], delay);
        }

        public override void OnIsolatorPulled()
        {
            if (Completed) return;
            if (StepIndex != 5)
            {
                Say("CENTRAL", "Nada para isolar ainda.", UITheme.Dim, 3.5f);
                return;
            }

            var board = World != null ? World.Switchboard : null;
            var connected = board != null && board.HasConnection && (board.ConnectedA == m_Contaminated || board.ConnectedB == m_Contaminated);
            if (!connected)
            {
                Manager?.Contamination?.Add(15f * Difficulty.ContaminationMultiplier, "isolamento do circuito errado");
                Say("ISOLAMENTO", "O circuito no cabo não é o contaminado. O sinal piora.", UITheme.Red, 5f);
                return;
            }

            Command("isolator.lock");
            Command($"socket.mark:{m_Contaminated}:0");
            Manager?.AddIsolatedCircuit();
            Manager?.Contamination?.Reduce(30f);
            Say("ISOLAMENTO", $"O circuito {m_Contaminated} foi isolado. A central fica em silêncio. Então toca.", UITheme.Green, 6f);
            SetStep(6, "Atenda a central uma última vez.");
            Ring(0, "ch1_final", 3f);
        }

        private string Place(int ext)
        {
            switch (ext)
            {
                case 205: return "a recepção";
                case 211: return "o arquivo de vozes";
                default: return "o corredor";
            }
        }

        private static string SourceVoice(int ext)
        {
            switch (ext)
            {
                case 205: return "Você atendeu. Ninguém atendia aqui desde que fecharam.";
                case 211: return "As fichas de 1987 estão fora de ordem. Alguém procurou o mesmo nome muitas vezes.";
                default: return "Você contou as portas? Tem uma a mais quando ninguém olha.";
            }
        }

        private static string SourceAmbience(int ext)
        {
            switch (ext)
            {
                case 205: return "O mesmo relógio da chamada, agora atrás de você.";
                case 211: return "Papel. Uma gaveta fecha devagar.";
                default: return "Passos que param quando você para.";
            }
        }
    }
}
