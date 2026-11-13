using System.Collections.Generic;
using ExtensaoZero.Core;
using UnityEngine;

namespace ExtensaoZero.Investigation
{
    /// <summary>
    /// Caderno do jogador. Persistência local; o host replica apenas fatos descobertos
    /// durante a sessão, nunca as anotações pessoais.
    /// </summary>
    public sealed class NotebookSystem : MonoBehaviour
    {
        public static NotebookSystem Instance { get; private set; }

        public NotebookModel Model { get; private set; }
        public int Count => Model.Count;
        public string Notes
        {
            get => Model.Notes;
            set => Model.Notes = value;
        }

        private GameManager m_Manager;

        public static NotebookSystem Create(Transform parent, GameManager manager)
        {
            var go = new GameObject("NotebookSystem");
            if (parent != null) go.transform.SetParent(parent, false);
            var system = go.AddComponent<NotebookSystem>();
            system.Model = new NotebookModel();
            system.m_Manager = manager;
            return system;
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

        public bool Register(EvidenceEntry entry)
        {
            if (!Model.Register(entry)) return false;
            if (m_Manager != null) m_Manager.UI?.RefreshNotebook();
            return true;
        }

        public void RestoreById(string id)
        {
            var entry = EvidenceCatalog.Find(id);
            if (entry != null) Model.Register(entry);
        }

        public bool Contains(string id) => Model.Contains(id);

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }

    /// <summary>
    /// Catálogo de fatos conhecidos pela campanha. Mantido em código para evitar assets frágeis.
    /// Cada entrada descreve uma fonte concreta; nenhuma afirma qual é a solução.
    /// </summary>
    public static class EvidenceCatalog
    {
        private static Dictionary<string, EvidenceEntry> s_Entries;

        public static EvidenceEntry Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            Build();
            if (!s_Entries.TryGetValue(id, out var template)) return null;
            return new EvidenceEntry(template.Id, template.Title, template.Body, template.Category, template.Source);
        }

        private static void Add(string id, string title, string body, EvidenceCategory category, string source)
        {
            s_Entries[id] = new EvidenceEntry(id, title, body, category, source);
        }

        private static void Build()
        {
            if (s_Entries != null) return;
            s_Entries = new Dictionary<string, EvidenceEntry>();

            // Prólogo
            Add("cable_214", "Cabo 214", "Cabo de extensão etiquetado 214, guardado no arquivo de vozes.", EvidenceCategory.Extension, "Arquivo de Vozes");
            Add("plate_trunk0", "Placa do tronco 0", "TRONCO 0 — LINHA MORTA. A placa indica que o tronco zero não pertence à rede ativa.", EvidenceCategory.Document, "Central");
            Add("note_line9", "Anotação na central", "\"Não conecte o 9.\" Escrita à mão, tinta recente.", EvidenceCategory.Document, "Central");
            Add("dtmf_214", "Sequência DTMF 2-1-4", "Tons decodificados na camada de sinais da primeira chamada.", EvidenceCategory.Signal, "Chamada 1");
            Add("knocks_three", "Três batidas", "Três batidas e uma porta na camada de ambiente. Um zumbido de 60 Hz por baixo.", EvidenceCategory.Anomaly, "Chamada 1");
            Add("voice_warning", "Aviso na interferência", "\"Não use a linha nove.\" Voz sobreposta, não identificada.", EvidenceCategory.Recording, "Chamada 1");
            Add("call_214_origin", "Origem da 214", "A extensão 214 não existe no painel. A chamada chega pelo tronco zero.", EvidenceCategory.Extension, "Chamada 1");
            Add("second_call", "Segunda chamada", "A mesma voz agradece e avisa: \"ela já sabe que vocês estão aqui\".", EvidenceCategory.Recording, "Chamada 2");
            Add("silhouette", "Silhueta no corredor", "Uma figura no fim do corredor, imóvel, desaparece quando observada diretamente.", EvidenceCategory.Anomaly, "Corredor");
            Add("manual_trunk0", "Manual de operação", "\"Conexões de emergência usam o tronco zero.\" Página dobrada, assinada por Í. Vilar.", EvidenceCategory.Document, "Recepção");
            Add("map_board", "Planta da estação", "Recepção, central, arquivo de vozes, corredor técnico, arquivo morto e acesso ao subsolo. A central original aparece riscada.", EvidenceCategory.Location, "Recepção");

            Add("dorm_roster", "Escala de quartos — 1987", "Quarto 3: C. Prado e A. Prado (estagiária, 1983–1987). Um cobertor dobrado que ninguém recolheu.", EvidenceCategory.Document, "Alojamentos");
            Add("call_log_1987", "Registro de chamadas — 14/03/1987", "02:51 emergência do povoado para o tronco 0. 03:05 queda de tensão. 03:12 sem registro. O relógio da recepção parou às 03:12.", EvidenceCategory.Document, "Salas de operação");

            // Capítulo 1
            Add("ch1_traffic", "Tráfego fantasma", "Três extensões fora de serviço registram tráfego. O rastreio é feito pelo tronco zero.", EvidenceCategory.Signal, "Central");
            Add("phantom_205", "Extensão 205 — recepção", "Rastreada até o telefone da recepção. Relógio, chuva em vidro.", EvidenceCategory.Extension, "Rastreio");
            Add("phantom_211", "Extensão 211 — arquivo de vozes", "Rastreada até o telefone do arquivo de vozes. Gavetas, papel.", EvidenceCategory.Extension, "Rastreio");
            Add("phantom_216", "Extensão 216 — corredor", "Rastreada até o telefone do corredor técnico. Eco longo, canos.", EvidenceCategory.Extension, "Rastreio");
            Add("ch1_contaminated", "Circuito contaminado", "Uma das extensões rastreadas mencionou a linha nove. O LED dela acende vermelho no painel.", EvidenceCategory.Anomaly, "Central");
            Add("ch1_order", "A chamada sabia a ordem", "A voz descreveu, na ordem certa, os lugares por onde a equipe passou.", EvidenceCategory.Recording, "Central");

            // Capítulo 2
            Add("tape_1979", "Fita de 1979", "Celina Prado: \"transfira a emergência para a 207 e mantenha o tronco zero livre\".", EvidenceCategory.Recording, "Arquivo Morto");
            Add("tape_1987", "Fita de 1987", "Celina Prado: \"transfira a emergência para a 212 e mantenha o tronco zero aberto\". Uma voz jovem ao fundo.", EvidenceCategory.Recording, "Arquivo Morto");
            Add("tape_diff", "Duas versões da mesma ligação", "1979: 207 e tronco livre. 1987: 212 e tronco aberto. A frase é a mesma; os números mudaram.", EvidenceCategory.Recording, "Comparação");
            Add("celina_voice", "A voz é de Celina", "Nara reconhece a voz das fitas: Celina Prado, operadora de tráfego desde 1974.", EvidenceCategory.Recording, "Nara Prado");
            Add("duty_roster_1987", "Escala de plantão — 1987", "Noite da tempestade: C. Prado (operadora), A. Prado (estagiária). Emergência do povoado roteada pela 212.", EvidenceCategory.Document, "Arquivo Morto");
            Add("cut_order_1998", "Ordem de corte — 1998", "Desmontagem da rede analógica. Item 0 (tronco zero): \"não executado\". Sem assinatura.", EvidenceCategory.Document, "Arquivo Morto");
            Add("ana_voice", "Uma voz jovem chama \"mãe\"", "A extensão 207 respondeu com uma voz jovem: \"a linha zero ainda está aberta\".", EvidenceCategory.Recording, "Extensão 207");

            // Capítulo 3
            Add("tunnel_boxes", "Seis caixas de passagem", "A linha desce pelos túneis em seis caixas. As contaminadas zumbem e piscam.", EvidenceCategory.Location, "Túneis");
            Add("reflection", "Reflexo no vidro", "No vidro do fim do túnel oeste, uma silhueta aparece atrás de quem olha. Não há ninguém do outro lado.", EvidenceCategory.Anomaly, "Túneis");
            Add("dead_line", "A linha morta respira", "Depois de três isolamentos, a voz do túnel diz que respira mais devagar.", EvidenceCategory.Recording, "Túneis");

            // Capítulo 4
            Add("prediction_true", "A previsão se cumpriu", "A chamada disse que a porta seria aberta e a luz apagaria duas vezes. Aconteceu.", EvidenceCategory.Anomaly, "Sala de Energia");
            Add("breaker_manual", "Manual de desligamento", "\"Para isolar a linha zero: desligue A e C. Nunca desligue B sob carga — B alimenta a iluminação de emergência.\"", EvidenceCategory.Document, "Sala de Energia");
            Add("personal_call", "Ela leu o caderno", "Uma chamada citou frases do caderno da equipe, com as pausas erradas.", EvidenceCategory.Recording, "Chamada particular");

            // Capítulo 5
            Add("fire_records", "Registros do incêndio", "Relatório interno de 1987: a sobrecarga começou no tronco zero, mantido aberto além do protocolo. O laudo de 1979 foi copiado, com outra data.", EvidenceCategory.Document, "Central Original");
            Add("ana_badge", "Crachá queimado", "ANA PRADO — ESTAGIÁRIA DE OPERAÇÃO. O plástico derreteu sobre o painel.", EvidenceCategory.Document, "Central Original");
            Add("original_recording", "Gravação original", "A chamada de emergência de 1987 nunca foi encerrada. Celina pede a Ana que segure a linha.", EvidenceCategory.Recording, "Central Original");
            Add("restored_clean", "Tronco restaurado", "O tronco zero foi religado à linha original sem circuitos contaminados.", EvidenceCategory.Extension, "Central Original");
        }
    }
}
