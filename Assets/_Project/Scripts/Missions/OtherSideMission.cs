using ExtensaoZero.AI;
using ExtensaoZero.Campaign;
using ExtensaoZero.UI;
using Phone = ExtensaoZero.Telephone.Telephone;

namespace ExtensaoZero.Missions
{
    /// <summary>
    /// Capítulo 4 — O Outro Lado. As chamadas descrevem os investigadores antes de agirem.
    /// A primeira previsão se cumpre; a segunda pede confiança: a voz diz para manter o
    /// disjuntor B, a interferência diz para desligá-lo e o manual confirma a voz (A e C
    /// desligados, B ligado). Desligar B isola quem desligou (recuperável).
    /// Depois, a Assinante se aproxima quando ninguém olha.
    /// </summary>
    public sealed class OtherSideMission : MissionBase
    {
        public override int ChapterIndex => 4;

        private bool m_PredictionFulfilled;
        private bool m_BreakersSolved;
        private bool m_BOffWarned;

        protected override void OnStart()
        {
            m_PredictionFulfilled = false;
            m_BreakersSolved = false;
            m_BOffWarned = false;
            SetStep(0, "Atenda o telefone dos túneis.");
            Say("RÁDIO — HELENA", CampaignCatalog.Get(4).Briefing.Replace("Helena: ", string.Empty), UITheme.Paper, 8f);
            Ring(218, "ch4_prediction", 3f);
        }

        public override void OnPhoneAnswered(Phone phone)
        {
            if (Completed || phone == null || phone.CurrentCall == null) return;
            switch (phone.CurrentCall.id)
            {
                case "ch4_prediction":
                    if (StepIndex < 1) SetStep(1, "Vá até a sala de energia, no fim leste do túnel transversal.");
                    break;
                case "ch4_breakers":
                    if (StepIndex < 3) SetStep(3, "Isolem a linha zero pelos disjuntores. Decidam em quem acreditar.");
                    Hint("O manual na parede da sala de energia descreve o desligamento seguro.");
                    break;
                case "ch4_personal":
                    Register("personal_call");
                    break;
            }
        }

        public override void OnCallFinished(Phone phone)
        {
            if (Completed || phone == null) return;
            if (StepIndex == 5 && phone.extension == 209)
            {
                SetStep(6, "Ela está vindo pelo túnel. Não percam ela de vista.");
                Command("door.open:porta_energia");
                Command("subscriber.manifest:tunnel_east:" + ManifestationMode.Approach);
                Manager?.Tension?.AddTension(0.25f);
            }
        }

        public override void HandleEvent(string id)
        {
            if (Completed) return;
            if (id == "door.open:porta_energia" && !m_PredictionFulfilled)
            {
                m_PredictionFulfilled = true;
                // A luz vermelha apaga duas vezes, como a chamada disse.
                Command("lights.off:energy");
                After(0.35f, () => Command("lights.on:energy"));
                After(0.8f, () => Command("lights.off:energy"));
                After(1.15f, () => Command("lights.on:energy"));
                Register("prediction_true");
                SetStep(2, "A previsão se cumpriu. O telefone da sala de energia vai tocar.");
                Ring(209, "ch4_breakers", 3f);
                return;
            }

            if (id.StartsWith("breaker:", System.StringComparison.Ordinal)) EvaluateBreakers(id);

            if (id == "subscriber.retreated" && StepIndex == 6)
            {
                SetStep(7, "Capítulo concluído.");
                Complete("A chamada previu a porta e a luz. A equipe confiou na voz e no manual, não na interferência. " +
                         "Ela leu o caderno de vocês em voz alta e veio pelo túnel. Recuou quando foi encarada.");
            }
        }

        private void EvaluateBreakers(string id)
        {
            if (m_BreakersSolved || World == null) return;
            var a = World.Breakers.TryGetValue("A", out var ba) && ba.IsOn;
            var b = World.Breakers.TryGetValue("B", out var bb) && bb.IsOn;
            var c = World.Breakers.TryGetValue("C", out var bc) && bc.IsOn;

            if (!b)
            {
                if (!m_BOffWarned)
                {
                    m_BOffWarned = true;
                    Say("SALA DE ENERGIA", "A iluminação de emergência morre. Alguém do grupo some do rádio por alguns segundos.", UITheme.Red, 6f);
                }

                Command("lights.off:energy");
                Manager?.TriggerLineBusy();
                return;
            }

            Command("lights.on:energy");
            if (StepIndex < 3) return;
            if (!a && !c)
            {
                m_BreakersSolved = true;
                Manager?.AddCampaignFlag("trusted_prediction");
                Say("SALA DE ENERGIA", "A e C desligados, B ligado. O gerador baixa o tom. A linha zero perde força.", UITheme.Green, 6f);
                SetStep(5, "O telefone da sala de energia toca de novo. É para vocês.");
                Ring(209, "ch4_personal", 3f);
            }
        }
    }
}
