using System.Collections.Generic;
using System.Text;
using ExtensaoZero.Core;
using ExtensaoZero.Multiplayer;
using ExtensaoZero.Telephone;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ExtensaoZero.UI
{
    /// <summary>
    /// Interface construída em runtime com uGUI e fonte built-in. Centraliza telas,
    /// legendas, prompt de interação e caderno. Sem assets de layout externos.
    /// </summary>
    public sealed class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private GameManager m_Manager;
        private Canvas m_Canvas;
        private Font m_Font;

        private GameObject m_MainMenu;
        private GameObject m_HostPanel;
        private GameObject m_JoinPanel;
        private GameObject m_Lobby;
        private GameObject m_HUD;
        private GameObject m_Pause;
        private GameObject m_Notebook;
        private GameObject m_MissionComplete;
        private GameObject m_Settings;

        private Text m_InteractionPrompt;
        private Text m_Objective;
        private Text m_Layer;
        private Text m_Battery;
        private Text m_Equipment;
        private Text m_Subtitle;
        private Text m_LobbyInfo;
        private Text m_LobbyPlayers;
        private Text m_NotebookBody;
        private Text m_MissionSummary;
        private Text m_HostError;
        private Text m_JoinError;
        private Image m_SubtitleBackground;
        private Image m_ContaminationFill;
        private GameObject m_Crosshair;
        private CanvasGroup m_SubtitleGroup;
        private Button m_StartMissionButton;

        private InputField m_HostKey;
        private InputField m_HostPort;
        private InputField m_JoinIp;
        private InputField m_JoinPort;
        private InputField m_JoinKey;
        private InputField m_Notes;

        private Slider m_Master;
        private Slider m_Call;
        private Slider m_Sfx;
        private Slider m_Ambience;
        private Slider m_Scare;
        private Slider m_Sensitivity;
        private Slider m_Fov;
        private Toggle m_Subtitles;
        private Toggle m_ReduceFlashes;
        private Toggle m_ReduceScare;

        private float m_SubtitleUntil;
        private bool m_SubtitleVisible;
        private GameState m_SettingsReturn = GameState.MainMenu;

        private const string NotesKey = "EZ_notebook_notes";

        public static UIManager Create(Transform parent, GameManager manager)
        {
            var go = new GameObject("UIManager");
            if (parent != null) go.transform.SetParent(parent, false);
            var ui = go.AddComponent<UIManager>();
            ui.m_Manager = manager;
            ui.Build();
            return ui;
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

        private void Build()
        {
            m_Font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (m_Font == null) m_Font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            UITheme.Font = m_Font;

            var canvasGo = new GameObject("UI_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            m_Canvas = canvasGo.GetComponent<Canvas>();
            m_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            m_Canvas.sortingOrder = 100;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            if (EventSystem.current == null)
            {
                var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                eventSystem.transform.SetParent(transform, false);
            }

            BuildMainMenu();
            BuildHostPanel();
            BuildJoinPanel();
            BuildLobby();
            BuildHUD();
            BuildPause();
            BuildNotebook();
            BuildMissionComplete();
            BuildSettings();
            BindSettings();

            if (m_Manager != null && m_Manager.Notebook != null)
            {
                m_Manager.Notebook.Notes = PlayerPrefs.GetString(NotesKey, string.Empty);
            }

            ShowMainMenu();
        }

        // ---------------------------------------------------------------- telas

        private void BuildMainMenu()
        {
            m_MainMenu = CreatePanel("MainMenu", m_Canvas.transform, UITheme.Backdrop, true);
            CreateText(m_MainMenu.transform, "Title", "EXTENSÃO ZERO", 58, UITheme.Paper, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(1000f, 90f));
            CreateText(m_MainMenu.transform, "Subtitle", "uma instalação. quatro camadas. nenhuma linha segura.", 18, UITheme.Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -198f), new Vector2(1000f, 40f));

            var y = -286f;
            CreateButton(m_MainMenu.transform, "JOGAR SOZINHO", new Vector2(0f, y), new Vector2(380f, 54f), () => m_Manager.StartSolo());
            CreateButton(m_MainMenu.transform, "CRIAR SESSÃO", new Vector2(0f, y - 68f), new Vector2(380f, 54f), ShowHostPanel);
            CreateButton(m_MainMenu.transform, "ENTRAR EM SESSÃO", new Vector2(0f, y - 136f), new Vector2(380f, 54f), ShowJoinPanel);
            CreateButton(m_MainMenu.transform, "CONFIGURAÇÕES", new Vector2(0f, y - 204f), new Vector2(380f, 54f), () => ShowSettings(true));
            CreateButton(m_MainMenu.transform, "SAIR", new Vector2(0f, y - 272f), new Vector2(380f, 54f), () => m_Manager.QuitGame());

            CreateText(m_MainMenu.transform, "Footer", "FATIA VERTICAL • SEM CONTAS • SEM SERVIÇOS EXTERNOS", 13, UITheme.Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(1100f, 30f));
        }

        private void BuildHostPanel()
        {
            m_HostPanel = CreateCenteredPanel("HostPanel", m_Canvas.transform, new Vector2(560f, 400f));
            CreateText(m_HostPanel.transform, "Title", "CRIAR SESSÃO", 30, UITheme.Paper, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(480f, 46f));
            CreateText(m_HostPanel.transform, "Hint", "Chave de sessão (4–24 caracteres). Compartilhe com a equipe.", 14, UITheme.Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -86f), new Vector2(480f, 30f));

            m_HostKey = CreateInputField(m_HostPanel.transform, "HostKey", "CHAVE", "EZ-214", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -132f), new Vector2(380f, 46f));
            m_HostPort = CreateInputField(m_HostPanel.transform, "HostPort", "PORTA", "7777", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -190f), new Vector2(380f, 46f));
            m_HostError = CreateText(m_HostPanel.transform, "Error", string.Empty, 13, UITheme.Red, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -236f), new Vector2(460f, 26f));

            CreateButton(m_HostPanel.transform, "INICIAR HOST", new Vector2(0f, -292f), new Vector2(300f, 48f), StartHost);
            CreateButton(m_HostPanel.transform, "VOLTAR", new Vector2(0f, -350f), new Vector2(300f, 42f), () =>
            {
                m_HostPanel.SetActive(false);
                m_MainMenu.SetActive(true);
            });
        }

        private void BuildJoinPanel()
        {
            m_JoinPanel = CreateCenteredPanel("JoinPanel", m_Canvas.transform, new Vector2(560f, 470f));
            CreateText(m_JoinPanel.transform, "Title", "ENTRAR EM SESSÃO", 30, UITheme.Paper, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(480f, 46f));
            CreateText(m_JoinPanel.transform, "Hint", "Conexão direta por IP. Sem relay, sem lobby, sem conta.", 14, UITheme.Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -86f), new Vector2(480f, 30f));

            m_JoinIp = CreateInputField(m_JoinPanel.transform, "JoinIp", "IP DO HOST", "127.0.0.1", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -132f), new Vector2(380f, 46f));
            m_JoinPort = CreateInputField(m_JoinPanel.transform, "JoinPort", "PORTA", "7777", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -190f), new Vector2(380f, 46f));
            m_JoinKey = CreateInputField(m_JoinPanel.transform, "JoinKey", "CHAVE", "EZ-214", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -248f), new Vector2(380f, 46f));
            m_JoinError = CreateText(m_JoinPanel.transform, "Error", string.Empty, 13, UITheme.Red, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -294f), new Vector2(460f, 26f));

            CreateButton(m_JoinPanel.transform, "CONECTAR", new Vector2(0f, -350f), new Vector2(300f, 48f), StartJoin);
            CreateButton(m_JoinPanel.transform, "VOLTAR", new Vector2(0f, -408f), new Vector2(300f, 42f), () =>
            {
                m_JoinPanel.SetActive(false);
                m_MainMenu.SetActive(true);
            });
        }

        private void BuildLobby()
        {
            m_Lobby = CreatePanel("Lobby", m_Canvas.transform, UITheme.Backdrop, true);
            CreateText(m_Lobby.transform, "Title", "LOBBY", 42, UITheme.Paper, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(900f, 60f));
            m_LobbyInfo = CreateText(m_Lobby.transform, "Info", "Aguardando conexão...", 17, UITheme.Amber, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -180f), new Vector2(900f, 32f));
            m_LobbyPlayers = CreateText(m_Lobby.transform, "Players", "Nenhum jogador conectado.", 18, UITheme.Paper, TextAnchor.UpperLeft,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(620f, 280f));
            m_StartMissionButton = CreateButton(m_Lobby.transform, "INICIAR MISSÃO", new Vector2(0f, -150f), new Vector2(340f, 52f),
                () => m_Manager.StartNetworkMission());
            CreateButton(m_Lobby.transform, "SAIR DA SESSÃO", new Vector2(0f, -214f), new Vector2(340f, 44f), () => m_Manager.ReturnToMenu());
        }

        private void BuildHUD()
        {
            m_HUD = NewUI("HUD", m_Canvas.transform);
            Stretch(m_HUD.GetComponent<RectTransform>());

            var crosshair = NewUI("Crosshair", m_HUD.transform);
            var crossRect = crosshair.GetComponent<RectTransform>();
            crossRect.anchorMin = crossRect.anchorMax = new Vector2(0.5f, 0.5f);
            crossRect.sizeDelta = new Vector2(5f, 5f);
            crossRect.anchoredPosition = Vector2.zero;
            var crossImage = crosshair.AddComponent<Image>();
            crossImage.color = new Color(0.9f, 0.88f, 0.8f, 0.65f);
            crossImage.raycastTarget = false;
            m_Crosshair = crosshair;

            m_Objective = CreateText(m_HUD.transform, "Objective", "OBJETIVO: —", 17, UITheme.Amber, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -28f), new Vector2(660f, 84f));

            m_Layer = CreateText(m_HUD.transform, "Layer", "CAMADA: VOZ", 14, UITheme.Paper, TextAnchor.LowerLeft,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(28f, 76f), new Vector2(420f, 24f));
            m_Battery = CreateText(m_HUD.transform, "Battery", "LANTERNA: 100%", 14, UITheme.Dim, TextAnchor.LowerLeft,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(28f, 50f), new Vector2(420f, 24f));
            m_Equipment = CreateText(m_HUD.transform, "Equipment", "EQUIPAMENTO: —", 14, UITheme.Dim, TextAnchor.LowerLeft,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(28f, 24f), new Vector2(420f, 24f));

            CreateText(m_HUD.transform, "NotebookHint", "TAB — CADERNO   •   T — SINTONIZADOR   •   F — LANTERNA", 13, UITheme.Dim, TextAnchor.LowerRight,
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-28f, 26f), new Vector2(620f, 24f));

            m_InteractionPrompt = CreateText(m_HUD.transform, "Prompt", string.Empty, 18, UITheme.Paper, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -78f), new Vector2(760f, 34f));

            var contaminationBg = NewUI("ContaminationBG", m_HUD.transform);
            var cbgRect = contaminationBg.GetComponent<RectTransform>();
            cbgRect.anchorMin = cbgRect.anchorMax = new Vector2(1f, 0f);
            cbgRect.pivot = new Vector2(1f, 0f);
            cbgRect.sizeDelta = new Vector2(220f, 8f);
            cbgRect.anchoredPosition = new Vector2(-28f, 54f);
            var cbgImage = contaminationBg.AddComponent<Image>();
            cbgImage.color = new Color(0.1f, 0.12f, 0.1f, 0.9f);
            cbgImage.raycastTarget = false;

            var fill = NewUI("ContaminationFill", contaminationBg.transform);
            Stretch(fill.GetComponent<RectTransform>());
            m_ContaminationFill = fill.AddComponent<Image>();
            m_ContaminationFill.color = UITheme.Red;
            m_ContaminationFill.type = Image.Type.Filled;
            m_ContaminationFill.fillMethod = Image.FillMethod.Horizontal;
            m_ContaminationFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            m_ContaminationFill.fillAmount = 0f;
            m_ContaminationFill.raycastTarget = false;

            CreateText(m_HUD.transform, "ContaminationLabel", "CONTAMINAÇÃO", 11, UITheme.Dim, TextAnchor.LowerRight,
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-28f, 68f), new Vector2(220f, 20f));

            var subtitlePanel = CreatePanel("SubtitlePanel", m_HUD.transform, new Color(0.01f, 0.015f, 0.012f, 0.62f), false);
            var subRect = subtitlePanel.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0.5f, 0f);
            subRect.anchorMax = new Vector2(0.5f, 0f);
            subRect.pivot = new Vector2(0.5f, 0f);
            subRect.sizeDelta = new Vector2(1160f, 112f);
            subRect.anchoredPosition = new Vector2(0f, 118f);
            m_SubtitleBackground = subtitlePanel.GetComponent<Image>();
            m_SubtitleBackground.raycastTarget = false;
            m_SubtitleGroup = subtitlePanel.AddComponent<CanvasGroup>();
            m_SubtitleGroup.alpha = 0f;

            m_Subtitle = CreateText(subtitlePanel.transform, "Subtitle", string.Empty, 23, UITheme.Paper, TextAnchor.MiddleCenter,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(-48f, -24f));
            m_Subtitle.rectTransform.offsetMin = new Vector2(24f, 12f);
            m_Subtitle.rectTransform.offsetMax = new Vector2(-24f, -12f);
        }

        private void BuildPause()
        {
            m_Pause = CreatePanel("Pause", m_Canvas.transform, UITheme.Backdrop, true);
            var panel = CreateCenteredPanel("PausePanel", m_Pause.transform, new Vector2(460f, 440f));
            CreateText(panel.transform, "Title", "PAUSADO", 36, UITheme.Paper, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -54f), new Vector2(380f, 52f));
            CreateText(panel.transform, "Hint", "A instalação espera. O diretor também.", 14, UITheme.Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -98f), new Vector2(380f, 26f));

            CreateButton(panel.transform, "CONTINUAR", new Vector2(0f, -156f), new Vector2(300f, 48f), () => m_Manager.Resume());
            CreateButton(panel.transform, "CADERNO", new Vector2(0f, -212f), new Vector2(300f, 44f), () => m_Manager.OpenNotebook());
            CreateButton(panel.transform, "CONFIGURAÇÕES", new Vector2(0f, -264f), new Vector2(300f, 44f), () => ShowSettings(true));
            CreateButton(panel.transform, "VOLTAR AO MENU", new Vector2(0f, -324f), new Vector2(300f, 44f), () => m_Manager.ReturnToMenu());
        }

        private void BuildNotebook()
        {
            m_Notebook = CreatePanel("Notebook", m_Canvas.transform, UITheme.Backdrop, true);
            var panel = CreateCenteredPanel("NotebookPanel", m_Notebook.transform, new Vector2(1020f, 700f));
            CreateText(panel.transform, "Title", "CADERNO DE CAMPO", 32, UITheme.Paper, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -44f), new Vector2(600f, 44f));
            CreateText(panel.transform, "Hint", "Fatos descobertos. O caderno não valida hipóteses.", 13, UITheme.Dim, TextAnchor.MiddleRight,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-44f, -44f), new Vector2(500f, 30f));

            m_NotebookBody = CreateText(panel.transform, "Body", string.Empty, 16, UITheme.Paper, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(44f, -96f), new Vector2(-88f, 470f));
            m_NotebookBody.rectTransform.anchorMin = new Vector2(0f, 0f);
            m_NotebookBody.rectTransform.anchorMax = new Vector2(1f, 1f);
            m_NotebookBody.rectTransform.offsetMin = new Vector2(44f, 160f);
            m_NotebookBody.rectTransform.offsetMax = new Vector2(-44f, -96f);

            CreateText(panel.transform, "NotesLabel", "OBSERVAÇÕES PESSOAIS (LOCAIS, NÃO COMPARTILHADAS)", 12, UITheme.Amber, TextAnchor.LowerLeft,
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(44f, 118f), new Vector2(-88f, 22f));
            m_Notes = CreateInputField(panel.transform, "Notes", "Anote uma hipótese...", string.Empty,
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 58f), new Vector2(-88f, 50f));
            var notesRect = (RectTransform)m_Notes.transform;
            notesRect.offsetMin = new Vector2(44f, 58f);
            notesRect.offsetMax = new Vector2(-44f, 108f);
            m_Notes.onEndEdit.AddListener(SaveNotes);

            CreateButtonBottom(panel.transform, "FECHAR", new Vector2(0f, 28f), new Vector2(220f, 44f), () => m_Manager.CloseNotebook());
        }

        private void BuildMissionComplete()
        {
            m_MissionComplete = CreatePanel("MissionComplete", m_Canvas.transform, UITheme.Backdrop, true);
            var panel = CreateCenteredPanel("CompletePanel", m_MissionComplete.transform, new Vector2(760f, 460f));
            CreateText(panel.transform, "Title", "MISSÃO CONCLUÍDA", 38, UITheme.Green, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -62f), new Vector2(640f, 56f));
            CreateText(panel.transform, "Subtitle", "A PRIMEIRA LIGAÇÃO", 16, UITheme.Amber, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -104f), new Vector2(640f, 28f));
            m_MissionSummary = CreateText(panel.transform, "Summary", string.Empty, 18, UITheme.Paper, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -158f), new Vector2(640f, 160f));
            CreateButton(panel.transform, "VOLTAR AO MENU", new Vector2(0f, -174f), new Vector2(320f, 52f), () => m_Manager.ReturnToMenu());
        }

        private void BuildSettings()
        {
            m_Settings = CreatePanel("Settings", m_Canvas.transform, UITheme.Backdrop, true);
            var panel = CreateCenteredPanel("SettingsPanel", m_Settings.transform, new Vector2(780f, 680f));
            CreateText(panel.transform, "Title", "CONFIGURAÇÕES", 32, UITheme.Paper, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(600f, 46f));

            var y = -108f;
            m_Master = AddSliderRow(panel.transform, "Volume geral", y);
            m_Call = AddSliderRow(panel.transform, "Chamadas", y - 58f);
            m_Sfx = AddSliderRow(panel.transform, "Efeitos", y - 116f);
            m_Ambience = AddSliderRow(panel.transform, "Ambiente", y - 174f);
            m_Scare = AddSliderRow(panel.transform, "Sustos", y - 232f);
            m_Sensitivity = AddSliderRow(panel.transform, "Sensibilidade", y - 290f);
            m_Fov = AddSliderRow(panel.transform, "Campo de visão", y - 348f);

            m_Subtitles = AddToggleRow(panel.transform, "Legendas", y - 410f);
            m_ReduceFlashes = AddToggleRow(panel.transform, "Reduzir flashes", y - 452f);
            m_ReduceScare = AddToggleRow(panel.transform, "Reduzir intensidade de sustos", y - 494f);

            CreateButtonBottom(panel.transform, "FECHAR", new Vector2(0f, 30f), new Vector2(240f, 46f), () =>
            {
                m_Settings.SetActive(false);
                if (m_SettingsReturn == GameState.Paused) m_Pause.SetActive(true);
                else if (m_SettingsReturn == GameState.MainMenu) m_MainMenu.SetActive(true);
            });
        }

        // ------------------------------------------------------------- interação

        private void ShowHostPanel()
        {
            m_MainMenu.SetActive(false);
            m_JoinPanel.SetActive(false);
            m_HostPanel.SetActive(true);
        }

        private void ShowJoinPanel()
        {
            m_MainMenu.SetActive(false);
            m_HostPanel.SetActive(false);
            m_JoinPanel.SetActive(true);
        }

        private void StartHost()
        {
            var key = m_HostKey != null ? m_HostKey.text : string.Empty;
            if (!SessionCodec.IsValid(key))
            {
                if (m_HostError != null) m_HostError.text = "Chave inválida: use de 4 a 24 caracteres alfanuméricos.";
                return;
            }

            if (!ushort.TryParse(m_HostPort != null ? m_HostPort.text : "7777", out var port) || port < 1024)
            {
                if (m_HostError != null) m_HostError.text = "Porta inválida: use um valor entre 1024 e 65535.";
                return;
            }

            if (m_HostError != null) m_HostError.text = string.Empty;
            m_Manager.CreateHost(key, port);
        }

        private void StartJoin()
        {
            var ip = m_JoinIp != null ? m_JoinIp.text.Trim() : string.Empty;
            var key = m_JoinKey != null ? m_JoinKey.text : string.Empty;
            if (string.IsNullOrEmpty(ip))
            {
                if (m_JoinError != null) m_JoinError.text = "Informe o IP do host.";
                return;
            }

            if (!SessionCodec.IsValid(key))
            {
                if (m_JoinError != null) m_JoinError.text = "Chave inválida: use de 4 a 24 caracteres alfanuméricos.";
                return;
            }

            if (!ushort.TryParse(m_JoinPort != null ? m_JoinPort.text : "7777", out var port) || port < 1024)
            {
                if (m_JoinError != null) m_JoinError.text = "Porta inválida.";
                return;
            }

            if (m_JoinError != null) m_JoinError.text = string.Empty;
            m_Manager.JoinGame(ip, port, key);
        }

        // ---------------------------------------------------------------- API

        public void ShowMainMenu()
        {
            SetActive(m_MainMenu, true);
            SetActive(m_HostPanel, false);
            SetActive(m_JoinPanel, false);
            SetActive(m_Lobby, false);
            SetActive(m_HUD, false);
            SetActive(m_Pause, false);
            SetActive(m_Notebook, false);
            SetActive(m_MissionComplete, false);
            SetActive(m_Settings, false);
            SetCursorForMenu(true);
        }

        public void ShowHUD()
        {
            SetActive(m_MainMenu, false);
            SetActive(m_HostPanel, false);
            SetActive(m_JoinPanel, false);
            SetActive(m_Lobby, false);
            SetActive(m_HUD, true);
            SetActive(m_Pause, false);
            SetActive(m_Notebook, false);
            SetActive(m_MissionComplete, false);
            SetActive(m_Settings, false);
            if (m_Crosshair != null) m_Crosshair.SetActive(true);
        }

        public void ShowLobby(bool isHost)
        {
            SetActive(m_MainMenu, false);
            SetActive(m_HostPanel, false);
            SetActive(m_JoinPanel, false);
            SetActive(m_HUD, false);
            SetActive(m_Pause, false);
            SetActive(m_Notebook, false);
            SetActive(m_MissionComplete, false);
            SetActive(m_Settings, false);
            SetActive(m_Lobby, true);
            if (m_LobbyInfo != null)
            {
                m_LobbyInfo.text = isHost
                    ? "SESSÃO CRIADA — aguardando jogadores. Pressione INICIAR MISSÃO quando estiver pronto."
                    : "CONECTANDO — aguardando o host iniciar a missão.";
            }

            if (m_StartMissionButton != null) m_StartMissionButton.gameObject.SetActive(isHost);
            SetCursorForMenu(true);
        }

        public void ShowPause()
        {
            SetActive(m_Pause, true);
            SetActive(m_Notebook, false);
            SetActive(m_Settings, false);
        }

        public void ShowNotebook()
        {
            RefreshNotebook();
            SetActive(m_Pause, false);
            SetActive(m_Settings, false);
            SetActive(m_Notebook, true);
            SetCursorForMenu(true);
        }

        public void ShowMissionComplete(string summary)
        {
            SetActive(m_HUD, false);
            SetActive(m_Pause, false);
            SetActive(m_Notebook, false);
            SetActive(m_Settings, false);
            SetActive(m_MissionComplete, true);
            if (m_MissionSummary != null) m_MissionSummary.text = string.IsNullOrEmpty(summary) ? "A linha foi isolada." : summary;
            SetCursorForMenu(true);
        }

        public void HideOverlays()
        {
            SetActive(m_Pause, false);
            SetActive(m_Notebook, false);
            SetActive(m_Settings, false);
            SetActive(m_MissionComplete, false);
            SetActive(m_Lobby, false);
            SetActive(m_MainMenu, false);
            SetActive(m_HUD, true);
        }

        public void ShowSettings(bool show)
        {
            if (show)
            {
                m_SettingsReturn = m_Manager != null ? m_Manager.State : GameState.MainMenu;
                RefreshSettings();
                SetActive(m_Settings, true);
                SetCursorForMenu(true);
            }
            else
            {
                SetActive(m_Settings, false);
            }
        }

        public void OnStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    ShowMainMenu();
                    break;
                case GameState.Playing:
                    if (m_HUD != null && !m_HUD.activeSelf) ShowHUD();
                    break;
                case GameState.Paused:
                    ShowPause();
                    break;
                case GameState.Notebook:
                    ShowNotebook();
                    break;
            }
        }

        public void RefreshNotebook()
        {
            if (m_NotebookBody == null || m_Manager == null || m_Manager.Notebook == null) return;
            var model = m_Manager.Notebook.Model;
            var builder = new StringBuilder();
            if (model.Count == 0)
            {
                builder.Append("Nenhum fato registrado ainda.\n\n");
                builder.Append("Ouça as camadas da chamada, examine objetos e conecte extensões. ");
                builder.Append("O caderno registra fatos, não soluções.");
            }
            else
            {
                for (var i = 0; i < model.Entries.Count; i++)
                {
                    var entry = model.Entries[i];
                    builder.Append("• [").Append(entry.CategoryLabel).Append("] ").Append(entry.Title).Append('\n');
                    builder.Append(entry.Body).Append("\n\n");
                }
            }

            m_NotebookBody.text = builder.ToString();
            if (m_Notes != null)
            {
                m_Notes.text = PlayerPrefs.GetString(NotesKey, string.Empty);
            }
        }

        public void UpdatePlayerList(List<NetworkPlayer> players)
        {
            if (m_LobbyPlayers == null) return;
            if (players == null || players.Count == 0)
            {
                m_LobbyPlayers.text = "Nenhum jogador conectado.";
                return;
            }

            var builder = new StringBuilder();
            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player == null) continue;
                var host = player.IsServer ? "  [HOST]" : string.Empty;
                var local = player.IsOwner ? "  — VOCÊ" : string.Empty;
                builder.Append($"{i + 1}. Jogador {player.OwnerClientId}{host}{local}\n");
            }

            m_LobbyPlayers.text = builder.ToString();
        }

        public void ShowSubtitle(string title, string body, Color color, float duration = 4f)
        {
            if (m_Subtitle == null) return;
            if (string.IsNullOrEmpty(body)) return;
            var text = string.IsNullOrEmpty(title) ? body : title + "\n" + body;
            m_Subtitle.text = text;
            m_Subtitle.color = color;
            m_SubtitleUntil = Time.unscaledTime + Mathf.Max(1f, duration);
            m_SubtitleVisible = true;
            if (m_SubtitleGroup != null) m_SubtitleGroup.alpha = 1f;
            ApplyAccessibility(m_Manager != null ? m_Manager.Settings : null);
        }

        public void SetInteractionPrompt(string prompt)
        {
            if (m_InteractionPrompt == null) return;
            m_InteractionPrompt.text = prompt ?? string.Empty;
        }

        public void SetObjective(string objective)
        {
            if (m_Objective != null) m_Objective.text = "OBJETIVO: " + (objective ?? "—");
        }

        public void SetLayerLabel(string layer)
        {
            if (m_Layer != null) m_Layer.text = "CAMADA: " + (layer ?? "VOZ");
        }

        public void SetBattery(float normalized)
        {
            if (m_Battery != null) m_Battery.text = $"LANTERNA: {Mathf.RoundToInt(Mathf.Clamp01(normalized) * 100f)}%";
        }

        public void SetEquipmentLabel(string label)
        {
            if (m_Equipment != null) m_Equipment.text = "EQUIPAMENTO: " + (label ?? "—");
        }

        public void SetContamination(float normalized)
        {
            if (m_ContaminationFill == null) return;
            m_ContaminationFill.fillAmount = Mathf.Clamp01(normalized);
            m_ContaminationFill.color = Color.Lerp(UITheme.Amber, UITheme.Red, Mathf.Clamp01(normalized));
        }

        public void ApplyAccessibility(GameSettings settings)
        {
            if (settings == null || m_Subtitle == null) return;
            m_Subtitle.fontSize = Mathf.RoundToInt(23f * Mathf.Clamp(settings.subtitleScale, 0.7f, 1.6f));
        }

        private void SaveNotes(string notes)
        {
            PlayerPrefs.SetString(NotesKey, notes ?? string.Empty);
            PlayerPrefs.Save();
            if (m_Manager != null && m_Manager.Notebook != null) m_Manager.Notebook.Notes = notes ?? string.Empty;
        }

        private void BindSettings()
        {
            var settings = m_Manager != null ? m_Manager.Settings : null;
            if (settings == null) return;

            m_Master.value = settings.masterVolume;
            m_Call.value = settings.callVolume;
            m_Sfx.value = settings.sfxVolume;
            m_Ambience.value = settings.ambienceVolume;
            m_Scare.value = settings.scareVolume;
            m_Sensitivity.value = Mathf.InverseLerp(0.3f, 4f, settings.mouseSensitivity);
            m_Fov.value = Mathf.InverseLerp(60f, 100f, settings.fieldOfView);
            m_Subtitles.isOn = settings.subtitles;
            m_ReduceFlashes.isOn = settings.reduceFlashes;
            m_ReduceScare.isOn = settings.reduceScareIntensity;

            m_Master.onValueChanged.AddListener(v => { settings.masterVolume = v; settings.NotifyChanged(); });
            m_Call.onValueChanged.AddListener(v => { settings.callVolume = v; settings.NotifyChanged(); });
            m_Sfx.onValueChanged.AddListener(v => { settings.sfxVolume = v; settings.NotifyChanged(); });
            m_Ambience.onValueChanged.AddListener(v => { settings.ambienceVolume = v; settings.NotifyChanged(); });
            m_Scare.onValueChanged.AddListener(v => { settings.scareVolume = v; settings.NotifyChanged(); });
            m_Sensitivity.onValueChanged.AddListener(v => { settings.mouseSensitivity = Mathf.Lerp(0.3f, 4f, v); settings.NotifyChanged(); });
            m_Fov.onValueChanged.AddListener(v => { settings.fieldOfView = Mathf.Lerp(60f, 100f, v); settings.NotifyChanged(); });
            m_Subtitles.onValueChanged.AddListener(v => { settings.subtitles = v; settings.NotifyChanged(); });
            m_ReduceFlashes.onValueChanged.AddListener(v => { settings.reduceFlashes = v; settings.NotifyChanged(); });
            m_ReduceScare.onValueChanged.AddListener(v => { settings.reduceScareIntensity = v; settings.NotifyChanged(); });
        }

        private void RefreshSettings()
        {
            var settings = m_Manager != null ? m_Manager.Settings : null;
            if (settings == null) return;
            m_Master.SetValueWithoutNotify(settings.masterVolume);
            m_Call.SetValueWithoutNotify(settings.callVolume);
            m_Sfx.SetValueWithoutNotify(settings.sfxVolume);
            m_Ambience.SetValueWithoutNotify(settings.ambienceVolume);
            m_Scare.SetValueWithoutNotify(settings.scareVolume);
            m_Sensitivity.SetValueWithoutNotify(Mathf.InverseLerp(0.3f, 4f, settings.mouseSensitivity));
            m_Fov.SetValueWithoutNotify(Mathf.InverseLerp(60f, 100f, settings.fieldOfView));
            m_Subtitles.SetIsOnWithoutNotify(settings.subtitles);
            m_ReduceFlashes.SetIsOnWithoutNotify(settings.reduceFlashes);
            m_ReduceScare.SetIsOnWithoutNotify(settings.reduceScareIntensity);
        }

        private void Update()
        {
            if (m_SubtitleVisible && m_SubtitleGroup != null)
            {
                var remaining = m_SubtitleUntil - Time.unscaledTime;
                if (remaining <= 0f)
                {
                    m_SubtitleGroup.alpha = 0f;
                    m_SubtitleVisible = false;
                }
                else if (remaining < 0.45f)
                {
                    m_SubtitleGroup.alpha = Mathf.Clamp01(remaining / 0.45f);
                }
            }

            if (m_Manager == null) return;
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (m_Manager.State == GameState.Playing && keyboard.tabKey.wasPressedThisFrame)
            {
                m_Manager.OpenNotebook();
            }
            else if (m_Manager.State == GameState.Notebook && (keyboard.tabKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame))
            {
                m_Manager.CloseNotebook();
            }
        }

        private void SetCursorForMenu(bool menu)
        {
            Cursor.lockState = menu ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = menu;
        }

        // ------------------------------------------------------------- helpers

        private GameObject CreatePanel(string name, Transform parent, Color color, bool blockRaycasts)
        {
            var go = NewUI(name, parent);
            Stretch(go.GetComponent<RectTransform>());
            var image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = blockRaycasts;
            return go;
        }

        private GameObject CreateCenteredPanel(string name, Transform parent, Vector2 size)
        {
            var go = NewUI(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            var image = go.AddComponent<Image>();
            image.color = UITheme.Panel;
            image.raycastTarget = true;
            return go;
        }

        private Text CreateText(Transform parent, string name, string content, int size, Color color, TextAnchor anchor,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            var go = NewUI(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            if (anchorMin == anchorMax)
            {
                rect.pivot = anchorMin;
            }

            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            var text = go.AddComponent<Text>();
            text.font = m_Font;
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            shadow.effectDistance = new Vector2(1f, -1f);
            return text;
        }

        private Button CreateButton(Transform parent, string label, Vector2 position, Vector2 size, UnityAction action)
        {
            return CreateButtonInternal(parent, label, position, size, action, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        }

        private Button CreateButtonBottom(Transform parent, string label, Vector2 position, Vector2 size, UnityAction action)
        {
            return CreateButtonInternal(parent, label, position, size, action, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        }

        private Button CreateButtonInternal(Transform parent, string label, Vector2 position, Vector2 size, UnityAction action, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = NewUI("Button_" + label, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMin;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var image = go.AddComponent<Image>();
            image.color = new Color(0.09f, 0.105f, 0.095f, 0.96f);
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.25f, 1.18f, 1.05f);
            colors.pressedColor = new Color(0.8f, 0.75f, 0.65f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;
            if (action != null) button.onClick.AddListener(action);

            var text = CreateText(go.transform, "Label", label, 17, UITheme.Paper, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            return button;
        }

        private InputField CreateInputField(Transform parent, string name, string placeholder, string value,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            var go = NewUI(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMin;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var image = go.AddComponent<Image>();
            image.color = new Color(0.07f, 0.08f, 0.075f, 0.95f);
            var field = go.AddComponent<InputField>();
            field.targetGraphic = image;
            field.characterLimit = 64;

            var text = CreateText(go.transform, "Text", value, 16, UITheme.Paper, TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            text.rectTransform.offsetMin = new Vector2(12f, 4f);
            text.rectTransform.offsetMax = new Vector2(-12f, -4f);
            text.supportRichText = false;

            var placeholderText = CreateText(go.transform, "Placeholder", placeholder, 15, new Color(0.5f, 0.52f, 0.48f, 0.8f), TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            placeholderText.rectTransform.offsetMin = new Vector2(12f, 4f);
            placeholderText.rectTransform.offsetMax = new Vector2(-12f, -4f);

            field.textComponent = text;
            field.placeholder = placeholderText;
            field.text = value;
            return field;
        }

        private Slider AddSliderRow(Transform parent, string label, float y)
        {
            CreateText(parent, "Label_" + label, label, 15, UITheme.Paper, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, y), new Vector2(280f, 28f));
            var slider = CreateSlider(parent, new Vector2(0f, 1f), new Vector2(360f, y), new Vector2(340f, 22f));
            return slider;
        }

        private Slider CreateSlider(Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var go = NewUI("Slider", parent);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var slider = go.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.direction = Slider.Direction.LeftToRight;

            var background = NewUI("Background", go.transform);
            Stretch(background.GetComponent<RectTransform>());
            var bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.08f, 0.09f, 0.085f, 0.95f);

            var fillArea = NewUI("FillArea", go.transform);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRect.offsetMin = new Vector2(6f, 0f);
            fillAreaRect.offsetMax = new Vector2(-6f, 0f);
            var fill = NewUI("Fill", fillArea.transform);
            Stretch(fill.GetComponent<RectTransform>());
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = UITheme.Amber;

            var handleArea = NewUI("HandleArea", go.transform);
            var handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.anchorMin = new Vector2(0f, 0f);
            handleAreaRect.anchorMax = new Vector2(1f, 1f);
            handleAreaRect.offsetMin = new Vector2(8f, 0f);
            handleAreaRect.offsetMax = new Vector2(-8f, 0f);
            var handle = NewUI("Handle", handleArea.transform);
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(16f, 22f);
            var handleImage = handle.AddComponent<Image>();
            handleImage.color = UITheme.Paper;

            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            return slider;
        }

        private Toggle AddToggleRow(Transform parent, string label, float y)
        {
            CreateText(parent, "Label_" + label, label, 15, UITheme.Paper, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, y), new Vector2(360f, 28f));
            var toggle = CreateToggle(parent, new Vector2(0f, 1f), new Vector2(360f, y));
            return toggle;
        }

        private Toggle CreateToggle(Transform parent, Vector2 anchor, Vector2 position)
        {
            var go = NewUI("Toggle", parent);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(28f, 28f);

            var background = NewUI("Background", go.transform);
            Stretch(background.GetComponent<RectTransform>());
            var bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.08f, 0.09f, 0.085f, 0.95f);

            var check = NewUI("Checkmark", go.transform);
            var checkRect = check.GetComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0.2f, 0.2f);
            checkRect.anchorMax = new Vector2(0.8f, 0.8f);
            checkRect.offsetMin = Vector2.zero;
            checkRect.offsetMax = Vector2.zero;
            var checkImage = check.AddComponent<Image>();
            checkImage.color = UITheme.Amber;

            var toggle = go.AddComponent<Toggle>();
            toggle.targetGraphic = bgImage;
            toggle.graphic = checkImage;
            toggle.navigation = new Navigation { mode = Navigation.Mode.None };
            return toggle;
        }

        private static GameObject NewUI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetActive(GameObject go, bool active)
        {
            if (go != null) go.SetActive(active);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
