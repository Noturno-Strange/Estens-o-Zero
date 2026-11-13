using ExtensaoZero.Anomaly;
using ExtensaoZero.Telephone;
using ExtensaoZero.UI;
using UnityEngine;
using Phone = ExtensaoZero.Telephone.Telephone;

namespace ExtensaoZero.Missions
{
    /// <summary>
    /// Prólogo — "A Primeira Ligação" (GDD §12). Máquina de estados linear com uma falha
    /// recuperável (214→9). Passos 0–7 preservados da fatia vertical.
    /// </summary>
    public sealed class FirstCallMission : MissionBase
    {
        public override int ChapterIndex => 0;

        private bool m_SecondCallScheduled;
        private bool m_CorridorHandled;
        private bool m_IsolatorHandled;
        private bool m_CablePicked;

        protected override void OnSetupWorld(int seed)
        {
            // A fatia mantém o cabo sempre no arquivo de vozes (tutorial sem variação).
            Command("pickup.show:CABO 214");
        }

        protected override void OnStart()
        {
            m_SecondCallScheduled = false;
            m_CorridorHandled = false;
            m_IsolatorHandled = false;
            m_CablePicked = false;
            SetStep(0, "Atenda o telefone da central.");
            Ring(World.OfficePhone != null ? World.OfficePhone.extension : 0, CallCatalog.FirstCallId, 1.6f);
            Hint("A central fica ao norte da recepção. Siga o som do telefone.");
        }

        public override void OnPhoneAnswered(Phone phone)
        {
            if (Completed || phone == null) return;

            if (StepIndex == 0)
            {
                SetStep(1, "Ouça as camadas com o sintonizador (Q).");
                Register("call_214_origin");
                Register("dtmf_214");
                Register("knocks_three");
                Register("voice_warning");
                Say("OBJETIVO", "A 214 não existe no painel. Ouça as camadas e encontre uma forma de reconectá-la.", UITheme.Amber, 7f);
                Manager?.Tension?.AddTension(0.08f);
                return;
            }

            if (StepIndex == 4)
            {
                SetStep(5, "Investigue o corredor ao lado da central.");
                Register("second_call");
                Manager?.Tension?.AddTension(0.16f);
            }
        }

        public override void OnCallFinished(Phone phone)
        {
            if (Completed) return;
            if (StepIndex == 1)
            {
                if (m_CablePicked)
                {
                    SetStep(3, "Conecte a extensão 214 ao tronco 0 na central.");
                    Say("OBJETIVO", "Você já tem o cabo 214. Conecte a extensão ao tronco 0.", UITheme.Amber, 6f);
                }
                else
                {
                    SetStep(2, "Encontre o cabo 214 no arquivo de vozes.");
                    Say("OBJETIVO", "O cabo de extensão está no arquivo de vozes, em uma gaveta etiquetada.", UITheme.Amber, 6f);
                }
            }
            else if (StepIndex == 5)
            {
                Manager?.SetObjective("Investigue o corredor ao lado da central.");
            }
        }

        public override void OnCablePicked(string label)
        {
            if (Completed) return;
            m_CablePicked = true;
            if (StepIndex < 2) return;
            Register("cable_214");
            SetStep(3, "Conecte a extensão 214 ao tronco 0 na central.");
            Say("OBJETIVO", "Selecione a extensão 214 e depois o tronco 0. Não use a linha 9.", UITheme.Amber, 7f);
        }

        public override void OnConnectionResult(ConnectionVerdict verdict, int a, int b)
        {
            if (Completed) return;

            if (verdict == ConnectionVerdict.Solution)
            {
                if (StepIndex < 4)
                {
                    SetStep(4, "Atenda a segunda chamada.");
                    Register("call_214_origin");
                    Say("CENTRAL", "A linha 214 responde. O sinal está estável.", UITheme.Green, 5f);
                }

                if (!m_SecondCallScheduled)
                {
                    m_SecondCallScheduled = true;
                    Ring(World.OfficePhone != null ? World.OfficePhone.extension : 0, CallCatalog.SecondCallId, 2.4f);
                }

                return;
            }

            if (verdict == ConnectionVerdict.Contaminated)
            {
                // GDD §12: 214→9 sempre gera contaminação, ameaça e Linha Ocupada (falha recuperável).
                Manager?.Contamination?.Add(60f, $"conexão contaminada {a:000}→{b:000}");
                Manager?.Tension?.ForceEvent(TensionCategory.Threat);
                Say("ERRO DE CONEXÃO", "A linha 9 contaminou o circuito. Desconecte e tente o tronco 0.", UITheme.Red, 7f);
            }
            else
            {
                Manager?.Contamination?.Add(10f * Difficulty.ContaminationMultiplier, $"conexão sem resposta {a:000}→{b:000}");
                Say("CENTRAL", "Essa conexão não responde. Verifique a etiqueta do cabo e a placa do tronco 0.", UITheme.Amber, 5f);
            }
        }

        public override void OnCorridorEntered()
        {
            if (Completed || m_CorridorHandled || StepIndex < 5) return;
            m_CorridorHandled = true;
            Register("silhouette");
            Say("CORREDOR", "As luzes apagam em sequência. Algo está no fim do corredor.", UITheme.Red, 5f);
            Manager?.Tension?.ForceEvent(TensionCategory.Contradiction);
            After(1.2f, () =>
            {
                Command("subscriber.manifest:corridor_end:" + AI.ManifestationMode.Silhouette);
                Manager?.Tension?.AddTension(0.15f);
                SetStep(6, "Isole a linha na central.");
            });
        }

        public override void OnIsolatorPulled()
        {
            if (Completed || m_IsolatorHandled) return;
            if (StepIndex < 6)
            {
                Say("CENTRAL", "Ainda não há nada para isolar. Conclua a reconexão primeiro.", UITheme.Dim, 4f);
                return;
            }

            m_IsolatorHandled = true;
            Command("isolator.lock");
            Manager?.Contamination?.Reduce(40f);
            Say("ISOLAMENTO", "A linha foi isolada. O sinal caiu.", UITheme.Green, 5f);
            SetStep(7, "Missão concluída.");
            Complete("A extensão 214 foi reconectada ao tronco zero e a linha contaminada foi isolada. " +
                     "A origem da chamada permanece desconhecida.");
        }
    }
}
