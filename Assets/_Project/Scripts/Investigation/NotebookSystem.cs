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
    /// Catálogo de fatos conhecidos pela fatia. Mantido em código para evitar assets frágeis.
    /// </summary>
    public static class EvidenceCatalog
    {
        public static EvidenceEntry Find(string id)
        {
            switch (id)
            {
                case "cable_214":
                    return new EvidenceEntry(id, "Cabo 214", "Cabo de extensão etiquetado 214, guardado no arquivo de vozes.", EvidenceCategory.Extension, "Arquivo de Vozes");
                case "plate_trunk0":
                    return new EvidenceEntry(id, "Placa do tronco 0", "TRONCO 0 — LINHA MORTA. A placa indica que o tronco zero não pertence à rede ativa.", EvidenceCategory.Document, "Central");
                case "note_line9":
                    return new EvidenceEntry(id, "Anotação na central", "\"Não conecte o 9.\" Escrita à mão, tinta recente.", EvidenceCategory.Document, "Central");
                case "dtmf_214":
                    return new EvidenceEntry(id, "Sequência DTMF 2-1-4", "Tons decodificados na camada de sinais da primeira chamada.", EvidenceCategory.Signal, "Chamada 1");
                case "knocks_three":
                    return new EvidenceEntry(id, "Três batidas", "Três batidas e uma porta na camada de ambiente. O som vem de dentro do prédio.", EvidenceCategory.Anomaly, "Chamada 1");
                case "voice_warning":
                    return new EvidenceEntry(id, "Aviso na interferência", "\"Não use a linha nove.\" Voz sobreposta, não identificada.", EvidenceCategory.Recording, "Chamada 1");
                case "call_214_origin":
                    return new EvidenceEntry(id, "Origem da 214", "A extensão 214 não existe no painel. A chamada chega pelo tronco zero.", EvidenceCategory.Extension, "Chamada 1");
                case "second_call":
                    return new EvidenceEntry(id, "Segunda chamada", "A mesma voz agradece e avisa: \"ela já sabe que vocês estão aqui\".", EvidenceCategory.Recording, "Chamada 2");
                case "silhouette":
                    return new EvidenceEntry(id, "Silhueta no corredor", "Uma figura no fim do corredor, imóvel, desaparece quando observada diretamente.", EvidenceCategory.Anomaly, "Corredor");
                default:
                    return null;
            }
        }
    }
}
