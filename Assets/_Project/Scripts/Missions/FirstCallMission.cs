using System.Collections;
using ExtensaoZero.AI;
using ExtensaoZero.Anomaly;
using ExtensaoZero.Core;
using ExtensaoZero.Environment;
using ExtensaoZero.Investigation;
using ExtensaoZero.Telephone;
using ExtensaoZero.UI;
using UnityEngine;
using Phone = ExtensaoZero.Telephone.Telephone;

namespace ExtensaoZero.Missions
{
    /// <summary>
    /// "A Primeira Ligação" — missão da fatia. Máquina de estados linear com uma falha
    /// recuperável (214→9). Cada passo registra um fato, nunca uma solução validada.
    /// </summary>
    public sealed class FirstCallMission : MonoBehaviour
    {
        public static FirstCallMission Instance { get; private set; }

        public int StepIndex { get; private set; }
        public string Summary { get; private set; } = string.Empty;
        public bool Completed { get; private set; }

        private GameManager m_Manager;
        private WorldContext m_World;
        private bool m_SecondCallScheduled;
        private bool m_CorridorHandled;
        private bool m_IsolatorHandled;
        private bool m_CablePicked;

        public static FirstCallMission Create(Transform parent, GameManager manager, WorldContext world)
        {
            var go = new GameObject("FirstCallMission");
            if (parent != null) go.transform.SetParent(parent, false);
            var mission = go.AddComponent<FirstCallMission>();
            mission.m_Manager = manager;
            mission.m_World = world;
            return mission;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void StartMission()
        {
            StopAllCoroutines();
            Completed = false;
            m_SecondCallScheduled = false;
            m_CorridorHandled = false;
            m_IsolatorHandled = false;
            m_CablePicked = false;
            Summary = string.Empty;
            SetStep(0, "Atenda o telefone da central.");
            StartCoroutine(RingFirstCallAfterDelay(1.6f));
            Debug.Log("[EZ] Missão 'A Primeira Ligação' iniciada.");
        }

        private IEnumerator RingFirstCallAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (m_World == null || m_World.OfficePhone == null) yield break;
            if (StepIndex > 0) yield break;
            m_World.OfficePhone.BeginRing(CallCatalog.FirstCall);
        }

        public void OnPhoneAnswered(Phone phone)
        {
            if (Completed || phone == null) return;

            if (StepIndex == 0)
            {
                SetStep(1, "Ouça as camadas com o sintonizador (T).");
                Register("call_214_origin");
                Register("dtmf_214");
                Register("knocks_three");
                Register("voice_warning");
                m_Manager?.UI?.ShowSubtitle("OBJETIVO", "A 214 não existe no painel. Ouça as camadas e encontre uma forma de reconectá-la.", UITheme.Amber, 7f);
                m_Manager?.Tension?.AddTension(0.08f);
                return;
            }

            if (StepIndex == 4)
            {
                SetStep(5, "Investigue o corredor ao lado da central.");
                Register("second_call");
                m_Manager?.Tension?.AddTension(0.16f);
            }
        }

        public void OnCallFinished(Phone phone)
        {
            if (Completed) return;
            if (StepIndex == 1)
            {
                if (m_CablePicked)
                {
                    SetStep(3, "Conecte a extensão 214 ao tronco 0 na central.");
                    m_Manager?.UI?.ShowSubtitle("OBJETIVO", "Você já tem o cabo 214. Conecte a extensão ao tronco 0.", UITheme.Amber, 6f);
                }
                else
                {
                    SetStep(2, "Encontre o cabo 214 no arquivo de vozes.");
                    m_Manager?.UI?.ShowSubtitle("OBJETIVO", "O cabo de extensão está no arquivo de vozes, em uma gaveta etiquetada.", UITheme.Amber, 6f);
                }
            }
            else if (StepIndex == 5)
            {
                m_Manager?.UI?.SetObjective("Investigue o corredor ao lado da central.");
            }
        }

        public void OnCablePicked()
        {
            if (Completed) return;
            m_CablePicked = true;
            if (StepIndex < 2) return;
            Register("cable_214");
            SetStep(3, "Conecte a extensão 214 ao tronco 0 na central.");
            m_Manager?.UI?.ShowSubtitle("OBJETIVO", "Selecione a extensão 214 e depois o tronco 0. Não use a linha 9.", UITheme.Amber, 7f);
        }

        public void OnConnectionResult(bool correct, int a, int b)
        {
            if (Completed) return;

            if (correct)
            {
                if (StepIndex < 4)
                {
                    SetStep(4, "Atenda a segunda chamada.");
                    Register("call_214_origin");
                    m_Manager?.UI?.ShowSubtitle("CENTRAL", "A linha 214 responde. O sinal está estável.", UITheme.Green, 5f);
                }

                if (!m_SecondCallScheduled)
                {
                    m_SecondCallScheduled = true;
                    StartCoroutine(RingSecondCallAfterDelay(2.4f));
                }

                return;
            }

            var contaminated = SwitchboardRules.IsContaminated(a, b);
            if (contaminated)
            {
                m_Manager?.Contamination?.Add(60f, $"conexão contaminada {a:000}→{b:000}");
                m_Manager?.Tension?.ForceEvent(TensionCategory.Threat);
                m_Manager?.UI?.ShowSubtitle("ERRO DE CONEXÃO", "A linha 9 contaminou o circuito. Desconecte e tente o tronco 0.", UITheme.Red, 7f);
            }
            else
            {
                m_Manager?.Contamination?.Add(10f, $"conexão sem resposta {a:000}→{b:000}");
                m_Manager?.UI?.ShowSubtitle("CENTRAL", "Essa conexão não responde. Verifique a etiqueta do cabo e a placa do tronco 0.", UITheme.Amber, 5f);
            }
        }

        private IEnumerator RingSecondCallAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (Completed || StepIndex < 4) yield break;
            if (m_World == null || m_World.OfficePhone == null) yield break;
            m_World.OfficePhone.BeginRing(CallCatalog.SecondCall);
        }

        public void OnCorridorEntered()
        {
            if (Completed || m_CorridorHandled || StepIndex < 5) return;
            m_CorridorHandled = true;
            Register("silhouette");
            m_Manager?.UI?.ShowSubtitle("CORREDOR", "As luzes apagam em sequência. Algo está no fim do corredor.", UITheme.Red, 5f);
            m_Manager?.Tension?.ForceEvent(TensionCategory.Contradiction);
            StartCoroutine(ManifestAfterDelay(1.2f));
        }

        private IEnumerator ManifestAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (Completed) yield break;
            m_Manager?.Subscriber?.Manifest();
            m_Manager?.Tension?.AddTension(0.15f);
            SetStep(6, "Isole a linha na central.");
        }

        public void OnIsolatorPulled()
        {
            if (Completed || m_IsolatorHandled) return;
            if (StepIndex < 6)
            {
                m_Manager?.UI?.ShowSubtitle("CENTRAL", "Ainda não há nada para isolar. Conclua a reconexão primeiro.", UITheme.Dim, 4f);
                return;
            }

            m_IsolatorHandled = true;
            m_Manager?.Contamination?.Reduce(40f);
            m_Manager?.UI?.ShowSubtitle("ISOLAMENTO", "A linha foi isolada. O sinal caiu.", UITheme.Green, 5f);
            Summary = "A extensão 214 foi reconectada ao tronco zero e a linha contaminada foi isolada. " +
                      "A origem da chamada permanece desconhecida.";
            Completed = true;
            SetStep(7, "Missão concluída.");
            StartCoroutine(CompleteAfterDelay(2.2f));
        }

        private IEnumerator CompleteAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            m_Manager?.CompleteMission();
        }

        private void SetStep(int step, string objective)
        {
            StepIndex = step;
            m_Manager?.AdvanceMissionStep(step);
            m_Manager?.UI?.SetObjective(objective);
        }

        private void Register(string id)
        {
            var entry = EvidenceCatalog.Find(id);
            if (entry != null) m_Manager?.RegisterEvidence(entry);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
