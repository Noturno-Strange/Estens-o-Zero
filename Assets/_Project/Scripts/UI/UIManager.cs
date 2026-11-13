using System.Collections.Generic;
using System.Text;
using ExtensaoZero.Campaign;
using ExtensaoZero.Core;
using ExtensaoZero.Environment;
using ExtensaoZero.Investigation;
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
    /// Interface construída em runtime com uGUI e fonte built-in (canvas único, sem assets de
    /// layout). Centraliza telas, legendas por camada, prompt de interação, caderno, mapa,
    /// briefing de capítulo, lobby e acessibilidade.
    /// </summary>
    public sealed class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private GameManager m_Manager;
        private Canvas m_Canvas;
        private Font m_Font;

        private GameObject m_MainMenu;
        private GameObject m_Briefing;
        private GameObject m_HostPanel;
        private GameObject m_JoinPanel;
        private GameObject m_Lobby;
        private GameObject m_HUD;
        private GameObject m_Pause;
        private GameObject m_Notebook;
        private GameObject m_Map;
        private GameObject m_MissionComplete;
        private GameObject m_Settings;
        private GameObject m_Disconnected;
        private GameObject m_Stats;
        private GameObject m_LineBusyPanel;

        private Text m_InteractionPrompt;
        private Text m_Objective;
        private Text m_ChapterTitle;
        private Text m_Layer;
        private Text m_Battery;
        private Text m_Equipment;
        private Text m_Loadout;
        private Text m_Receiver;
        private Text m_Recorder;
        private Text m_Subtitle;
        private Text m_LobbyInfo;
        private Text m_LobbyPlayers;
        private Text m_LobbyInvite;
        private Text m_LobbyChapter;
        private Text m_NotebookBody;
        private Text m_MapBody;
        private Text m_MissionTitle;
        private Text m_MissionSubtitle;
        private Text m_MissionSummary;
        private Text m_MissionStats;
        private Text m_HostError;
        private Text m_JoinError;
        private Text m_JoinLanStatus;
        private Text m_DisconnectedText;
        private Text m_StatsText;
        private Text m_LineBusyText;
        private Text m_BriefingTitle;
        private Text m_BriefingBody;
        private Text m_BriefingDifficulty;
        private Text m_QualityLabel;
        private Image m_ContaminationFill;
        private GameObject m_Crosshair;
        private CanvasGroup m_SubtitleGroup;
        private Button m_StartMissionButton;
        private Button m_NextChapterButton;
        private Button m_LobbyChapterButton;
        private Button m_LobbyDifficultyButton;
        private readonly List<Button> m_ChapterButtons = new List<Button>();
        private readonly List<Toggle> m_LoadoutToggles = new List<Toggle>();
        private readonly List<Toggle> m_LobbyLoadoutToggles = new List<Toggle>();
        private readonly List<Button> m_LanButtons = new List<Button>();
        private readonly List<LanDiscovery.Found> m_LanFound = new List<LanDiscovery.Found>();

        private InputField m_HostKey;
        private InputField m_HostPort;
        private InputField m_JoinInvite;
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
        private Slider m_SubtitleScale;
        private Toggle m_Subtitles;
        private Toggle m_ReduceFlashes;
        private Toggle m_ReduceScare;
        private Toggle m_InvertY;
        private Toggle m_Fullscreen;

        private float m_SubtitleUntil;
        private bool m_SubtitleVisible;
        private GameState m_SettingsReturn = GameState.MainMenu;
        private int m_SelectedChapter;
        private int m_LobbyChapterIndex;
        private bool m_UpdatingLoadout;
        private float m_StatsNext;
        private int m_StatsFrames;
        private float m_StatsTime;
        private LanDiscovery m_LanSearch;
        private int m_MapOpenedFrame = -1;
        private Image m_NotebookCover;

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
            BuildBriefing();
            BuildHostPanel();
            BuildJoinPanel();
            BuildLobby();
            BuildHUD();
            BuildPause();
            BuildNotebook();
            BuildMap();
            BuildMissionComplete();
            BuildSettings();
            BuildDisconnected();
            BuildStats();
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
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(1000f, 90f));
            CreateText(m_MainMenu.transform, "Subtitle", "uma instalação. quatro camadas. nenhuma linha segura.", 18, UITheme.Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -188f), new Vector2(1000f, 40f));

            var y = -262f;
            CreateButton(m_MainMenu.transform, "CONTINUAR CAMPANHA", new Vector2(0f, y), new Vector2(380f, 54f),
                () => ShowBriefing(m_Manager.Save != null ? CampaignCatalog.NextChapter(m_Manager.Save.chaptersCompleted) : 0));
            CreateButton(m_MainMenu.transform, "CAPÍTULOS", new Vector2(0f, y - 64f), new Vector2(380f, 54f), () => ShowBriefing(m_SelectedChapter));
            CreateButton(m_MainMenu.transform, "CRIAR SESSÃO", new Vector2(0f, y - 128f), new Vector2(380f, 54f), ShowHostPanel);
            CreateButton(m_MainMenu.transform, "ENTRAR EM SESSÃO", new Vector2(0f, y - 192f), new Vector2(380f, 54f), ShowJoinPanel);
            CreateButton(m_MainMenu.transform, "CONFIGURAÇÕES", new Vector2(0f, y - 256f), new Vector2(380f, 54f), () => ShowSettings(true));
            CreateButton(m_MainMenu.transform, "SAIR", new Vector2(0f, y - 320f), new Vector2(380f, 54f), () => m_Manager.QuitGame());

            CreateText(m_MainMenu.transform, "Footer", "1–4 JOGADORES • SEM CONTAS • SEM SERVIÇOS EXTERNOS • F3 ESTATÍSTICAS", 13, UITheme.Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(1100f, 30f));
        }

        private void BuildBriefing()
        {
            m_Briefing = CreatePanel("Briefing", m_Canvas.transform, UITheme.Backdrop, true);
            var panel = CreateCenteredPanel("BriefingPanel", m_Briefing.transform, new Vector2(1240f, 780f));
            CreateText(panel.transform, "Header", "BRIEFING", 30, UITheme.Paper, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -40f), new Vector2(600f, 44f));

            for (var i = 0; i < CampaignCatalog.ChapterCount; i++)
            {
                var index = i;
                var button = CreateButtonInternal(panel.transform, ShortTitle(i), new Vector2(44f, -100f - i * 58f), new Vector2(340f, 48f),
                    () => SelectChapter(index), new Vector2(0f, 1f), new Vector2(0f, 1f));
                m_ChapterButtons.Add(button);
            }

            m_BriefingTitle = CreateText(panel.transform, "ChapterTitle", string.Empty, 24, UITheme.Amber, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(420f, -96f), new Vector2(760f, 40f));
            m_BriefingBody = CreateText(panel.transform, "ChapterBody", string.Empty, 16, UITheme.Paper, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(420f, -146f), new Vector2(760f, 200f));

            CreateText(panel.transform, "LoadoutTitle", "EQUIPAMENTO (ATÉ 2 — LANTERNA E SINTONIZADOR SEMPRE)", 13, UITheme.Amber, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(420f, -372f), new Vector2(760f, 24f));
            for (var i = 0; i < 4; i++)
            {
                var kind = (EquipmentKind)i;
                var toggle = AddToggleRowAt(panel.transform, Loadout.Label(kind) + " — " + Loadout.Description(kind), new Vector2(420f, -410f - i * 40f), 700f);
                toggle.onValueChanged.AddListener(v => OnLoadoutToggle(kind, v, m_LoadoutToggles));
                m_LoadoutToggles.Add(toggle);
            }

            m_BriefingDifficulty = CreateText(panel.transform, "Difficulty", string.Empty, 15, UITheme.Paper, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(420f, -590f), new Vector2(560f, 40f));
            CreateButtonInternal(panel.transform, "DIFICULDADE", new Vector2(990f, -584f), new Vector2(200f, 44f), CycleDifficulty,
                new Vector2(0f, 1f), new Vector2(0f, 1f));

            CreateButtonBottom(panel.transform, "COMEÇAR", new Vector2(160f, 30f), new Vector2(280f, 52f), () => m_Manager.StartChapter(m_SelectedChapter));
            CreateButtonBottom(panel.transform, "VOLTAR", new Vector2(-160f, 30f), new Vector2(220f, 46f), ShowMainMenu);
        }

        private void BuildHostPanel()
        {
            m_HostPanel = CreateCenteredPanel("HostPanel", m_Canvas.transform, new Vector2(620f, 430f));
            CreateText(m_HostPanel.transform, "Title", "CRIAR SESSÃO", 30, UITheme.Paper, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(540f, 46f));
            CreateText(m_HostPanel.transform, "Hint", "Chave aleatória gerada para você (4–24 caracteres). Porta UDP 1024–65535.", 14, UITheme.Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -86f), new Vector2(540f, 30f));

            m_HostKey = CreateInputField(m_HostPanel.transform, "HostKey", "CHAVE", SessionKeyGenerator.Generate(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -132f), new Vector2(380f, 46f));
            m_HostPort = CreateInputField(m_HostPanel.transform, "HostPort", "PORTA", "7777", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -190f), new Vector2(380f, 46f));
            m_HostError = CreateText(m_HostPanel.transform, "Error", string.Empty, 13, UITheme.Red, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -240f), new Vector2(540f, 40f));

            CreateButton(m_HostPanel.transform, "INICIAR HOST", new Vector2(0f, -300f), new Vector2(300f, 48f), StartHost);
            CreateButton(m_HostPanel.transform, "VOLTAR", new Vector2(0f, -360f), new Vector2(300f, 42f), ShowMainMenu);
        }

        private void BuildJoinPanel()
        {
            m_JoinPanel = CreateCenteredPanel("JoinPanel", m_Canvas.transform, new Vector2(1040f, 640f));
            CreateText(m_JoinPanel.transform, "Title", "ENTRAR EM SESSÃO", 30, UITheme.Paper, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(900f, 46f));
            CreateText(m_JoinPanel.transform, "Hint", "Conexão direta. Sem relay, sem lobby online, sem conta. O seu IP fica visível para o host.", 14, UITheme.Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -84f), new Vector2(900f, 30f));

            m_JoinInvite = CreateInputField(m_JoinPanel.transform, "Invite", "COLE O CONVITE (EZ1:...)", string.Empty, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(-120f, -130f), new Vector2(640f, 46f));
            CreateButton(m_JoinPanel.transform, "USAR CONVITE", new Vector2(330f, -130f), new Vector2(220f, 46f), UseInvite);

            m_JoinIp = CreateInputField(m_JoinPanel.transform, "JoinIp", "IP DO HOST", "127.0.0.1", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(-230f, -196f), new Vector2(420f, 46f));
            m_JoinPort = CreateInputField(m_JoinPanel.transform, "JoinPort", "PORTA", "7777", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(110f, -196f), new Vector2(160f, 46f));
            m_JoinKey = CreateInputField(m_JoinPanel.transform, "JoinKey", "CHAVE", string.Empty, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(330f, -196f), new Vector2(220f, 46f));
            m_JoinError = CreateText(m_JoinPanel.transform, "Error", string.Empty, 13, UITheme.Red, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -250f), new Vector2(900f, 36f));

            CreateButton(m_JoinPanel.transform, "CONECTAR", new Vector2(0f, -296f), new Vector2(300f, 48f), StartJoin);
            CreateButton(m_JoinPanel.transform, "PROCURAR NA REDE LOCAL", new Vector2(0f, -356f), new Vector2(360f, 44f), SearchLan);
            m_JoinLanStatus = CreateText(m_JoinPanel.transform, "LanStatus", string.Empty, 13, UITheme.Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -400f), new Vector2(900f, 26f));
            for (var i = 0; i < 4; i++)
            {
                var index = i;
                var button = CreateButton(m_JoinPanel.transform, "—", new Vector2(0f, -436f - i * 44f), new Vector2(620f, 38f), () => PickLan(index));
                button.gameObject.SetActive(false);
                m_LanButtons.Add(button);
            }

            CreateButtonBottom(m_JoinPanel.transform, "VOLTAR", new Vector2(0f, 22f), new Vector2(240f, 40f), () =>
            {
                m_LanSearch?.StopSearch();
                ShowMainMenu();
            });
        }

        private void BuildLobby()
        {
            m_Lobby = CreatePanel("Lobby", m_Canvas.transform, UITheme.Backdrop, true);
            CreateText(m_Lobby.transform, "Title", "SESSÃO", 42, UITheme.Paper, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(900f, 60f));
            m_LobbyInfo = CreateText(m_Lobby.transform, "Info", "Aguardando conexão...", 16, UITheme.Amber, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(1300f, 110f));
            m_LobbyInvite = CreateText(m_Lobby.transform, "Invite", string.Empty, 14, UITheme.Paper, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -232f), new Vector2(1300f, 50f));
            CreateButton(m_Lobby.transform, "COPIAR CONVITE", new Vector2(0f, -290f), new Vector2(260f, 40f), CopyInvite);

            m_LobbyPlayers = CreateText(m_Lobby.transform, "Players", "Nenhum jogador conectado.", 18, UITheme.Paper, TextAnchor.UpperLeft,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-330f, -350f), new Vector2(560f, 180f));

            m_LobbyChapter = CreateText(m_Lobby.transform, "Chapter", string.Empty, 16, UITheme.Paper, TextAnchor.UpperLeft,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(330f, -350f), new Vector2(560f, 60f));
            m_LobbyChapterButton = CreateButton(m_Lobby.transform, "TROCAR CAPÍTULO", new Vector2(240f, -420f), new Vector2(240f, 40f), CycleLobbyChapter);
            m_LobbyDifficultyButton = CreateButton(m_Lobby.transform, "DIFICULDADE", new Vector2(500f, -420f), new Vector2(220f, 40f), () =>
            {
                CycleDifficulty();
                RefreshLobbyChapter();
            });

            CreateText(m_Lobby.transform, "LoadoutTitle", "SEU EQUIPAMENTO (ATÉ 2)", 13, UITheme.Amber, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(330f, -548f), new Vector2(560f, 24f));
            for (var i = 0; i < 4; i++)
            {
                var kind = (EquipmentKind)i;
                var toggle = AddToggleRowAt(m_Lobby.transform, Loadout.Label(kind), new Vector2(330f, -580f - i * 38f), 400f);
                toggle.onValueChanged.AddListener(v => OnLoadoutToggle(kind, v, m_LobbyLoadoutToggles));
                m_LobbyLoadoutToggles.Add(toggle);
            }

            m_StartMissionButton = CreateButtonBottom(m_Lobby.transform, "INICIAR MISSÃO", new Vector2(170f, 60f), new Vector2(320f, 52f),
                () => m_Manager.StartNetworkMission(m_LobbyChapterIndex));
            CreateButtonBottom(m_Lobby.transform, "SAIR DA SESSÃO", new Vector2(-170f, 60f), new Vector2(300f, 46f), () => m_Manager.ReturnToMenu());
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

            m_ChapterTitle = CreateText(m_HUD.transform, "Chapter", string.Empty, 12, UITheme.Dim, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -10f), new Vector2(660f, 20f));
            m_Objective = CreateText(m_HUD.transform, "Objective", "OBJETIVO: —", 17, UITheme.Amber, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -32f), new Vector2(720f, 84f));

            m_Receiver = CreateText(m_HUD.transform, "Receiver", string.Empty, 14, UITheme.Green, TextAnchor.UpperRight,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-28f, -28f), new Vector2(520f, 24f));
            m_Recorder = CreateText(m_HUD.transform, "Recorder", string.Empty, 13, UITheme.Paper, TextAnchor.UpperRight,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-28f, -54f), new Vector2(520f, 24f));

            m_Loadout = CreateText(m_HUD.transform, "Loadout", string.Empty, 12, UITheme.Dim, TextAnchor.LowerLeft,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(28f, 102f), new Vector2(720f, 22f));
            m_Layer = CreateText(m_HUD.transform, "Layer", "CAMADA: —", 14, UITheme.Paper, TextAnchor.LowerLeft,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(28f, 76f), new Vector2(420f, 24f));
            m_Battery = CreateText(m_HUD.transform, "Battery", "LANTERNA: 100%", 14, UITheme.Dim, TextAnchor.LowerLeft,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(28f, 50f), new Vector2(420f, 24f));
            m_Equipment = CreateText(m_HUD.transform, "Equipment", "CABO: —", 14, UITheme.Dim, TextAnchor.LowerLeft,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(28f, 24f), new Vector2(520f, 24f));

            CreateText(m_HUD.transform, "Keys", "E INTERAGIR • Q SINTONIZADOR • F LANTERNA • G CABO • R GRAVADOR • M MAPA • TAB CADERNO", 12, UITheme.Dim, TextAnchor.LowerRight,
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-28f, 26f), new Vector2(900f, 22f));

            m_InteractionPrompt = CreateText(m_HUD.transform, "Prompt", string.Empty, 18, UITheme.Paper, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -78f), new Vector2(980f, 34f));

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

            m_LineBusyPanel = CreatePanel("LineBusy", m_HUD.transform, new Color(0.25f, 0.03f, 0.02f, 0.55f), false);
            var busyRect = m_LineBusyPanel.GetComponent<RectTransform>();
            busyRect.anchorMin = busyRect.anchorMax = new Vector2(0.5f, 1f);
            busyRect.pivot = new Vector2(0.5f, 1f);
            busyRect.sizeDelta = new Vector2(620f, 70f);
            busyRect.anchoredPosition = new Vector2(0f, -120f);
            m_LineBusyText = CreateText(m_LineBusyPanel.transform, "Text", string.Empty, 17, UITheme.Paper, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            m_LineBusyText.rectTransform.offsetMin = new Vector2(10f, 6f);
            m_LineBusyText.rectTransform.offsetMax = new Vector2(-10f, -6f);
            m_LineBusyPanel.SetActive(false);

            var subtitlePanel = CreatePanel("SubtitlePanel", m_HUD.transform, new Color(0.01f, 0.015f, 0.012f, 0.62f), false);
            var subRect = subtitlePanel.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0.5f, 0f);
            subRect.anchorMax = new Vector2(0.5f, 0f);
            subRect.pivot = new Vector2(0.5f, 0f);
            subRect.sizeDelta = new Vector2(1240f, 124f);
            subRect.anchoredPosition = new Vector2(0f, 128f);
            subtitlePanel.GetComponent<Image>().raycastTarget = false;
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
            CreateText(panel.transform, "Hint", "Em sessão, o jogo continua para os outros.", 14, UITheme.Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -98f), new Vector2(380f, 26f));

            CreateButton(panel.transform, "CONTINUAR", new Vector2(0f, -156f), new Vector2(300f, 48f), () => m_Manager.Resume());
            CreateButton(panel.transform, "CADERNO", new Vector2(0f, -212f), new Vector2(300f, 44f), () =>
            {
                m_Manager.Resume();
                m_Manager.OpenNotebook();
            });
            CreateButton(panel.transform, "CONFIGURAÇÕES", new Vector2(0f, -264f), new Vector2(300f, 44f), () => ShowSettings(true));
            CreateButton(panel.transform, "VOLTAR AO MENU", new Vector2(0f, -324f), new Vector2(300f, 44f), () => m_Manager.ReturnToMenu());
        }

        private void BuildNotebook()
        {
            m_Notebook = CreatePanel("Notebook", m_Canvas.transform, UITheme.Backdrop, true);
            var panel = CreateCenteredPanel("NotebookPanel", m_Notebook.transform, new Vector2(1180f, 820f));
            m_NotebookCover = panel.GetComponent<Image>();
            m_NotebookCover.color = new Color(0.16f, 0.15f, 0.12f, 0.97f);
            CreateText(panel.transform, "Title", "CADERNO DE CAMPO", 32, UITheme.Paper, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -44f), new Vector2(600f, 44f));
            CreateText(panel.transform, "Hint", "Fatos descobertos, por categoria. O caderno não valida hipóteses.", 13, UITheme.Dim, TextAnchor.MiddleRight,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-44f, -44f), new Vector2(560f, 30f));

            var scroll = NewUI("Scroll", panel.transform);
            var scrollRect = scroll.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0f, 0f);
            scrollRect.anchorMax = new Vector2(1f, 1f);
            scrollRect.offsetMin = new Vector2(44f, 170f);
            scrollRect.offsetMax = new Vector2(-44f, -92f);
            var scrollImage = scroll.AddComponent<Image>();
            scrollImage.color = new Color(0f, 0f, 0f, 0.15f);
            scroll.AddComponent<RectMask2D>();
            var scrollView = scroll.AddComponent<ScrollRect>();
            scrollView.horizontal = false;
            scrollView.scrollSensitivity = 30f;

            var content = NewUI("Content", scroll.transform);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = new Vector2(10f, 0f);
            contentRect.offsetMax = new Vector2(-10f, 0f);
            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollView.content = contentRect;

            m_NotebookBody = content.AddComponent<Text>();
            m_NotebookBody.font = m_Font;
            m_NotebookBody.fontSize = 16;
            m_NotebookBody.color = UITheme.Paper;
            m_NotebookBody.alignment = TextAnchor.UpperLeft;
            m_NotebookBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            m_NotebookBody.verticalOverflow = VerticalWrapMode.Overflow;
            m_NotebookBody.supportRichText = true;
            m_NotebookBody.raycastTarget = false;

            CreateText(panel.transform, "NotesLabel", "OBSERVAÇÕES PESSOAIS (LOCAIS, NÃO COMPARTILHADAS)", 12, UITheme.Amber, TextAnchor.LowerLeft,
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(44f, 128f), new Vector2(-88f, 22f));
            m_Notes = CreateInputField(panel.transform, "Notes", "Anote uma hipótese...", string.Empty,
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 68f), new Vector2(-88f, 50f));
            var notesRect = (RectTransform)m_Notes.transform;
            notesRect.offsetMin = new Vector2(44f, 68f);
            notesRect.offsetMax = new Vector2(-44f, 118f);
            m_Notes.onEndEdit.AddListener(SaveNotes);

            CreateButtonBottom(panel.transform, "FECHAR", new Vector2(0f, 16f), new Vector2(220f, 44f), () => m_Manager.CloseNotebook());
        }

        private void BuildMap()
        {
            m_Map = CreatePanel("Map", m_Canvas.transform, UITheme.Backdrop, true);
            var panel = CreateCenteredPanel("MapPanel", m_Map.transform, new Vector2(1000f, 760f));
            CreateText(panel.transform, "Title", "MAPA DE EXTENSÕES", 30, UITheme.Paper, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -44f), new Vector2(600f, 44f));
            CreateText(panel.transform, "Hint", "Só mostra o que já foi descoberto.", 13, UITheme.Dim, TextAnchor.MiddleRight,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-44f, -44f), new Vector2(420f, 30f));
            m_MapBody = CreateText(panel.transform, "Body", string.Empty, 16, UITheme.Green, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -96f), new Vector2(910f, 560f));
            m_MapBody.supportRichText = true;
            CreateButtonBottom(panel.transform, "FECHAR (M / ESC)", new Vector2(0f, 24f), new Vector2(260f, 44f), () => m_Manager.CloseMap());
        }

        private void BuildMissionComplete()
        {
            m_MissionComplete = CreatePanel("MissionComplete", m_Canvas.transform, UITheme.Backdrop, true);
            var panel = CreateCenteredPanel("CompletePanel", m_MissionComplete.transform, new Vector2(960f, 640f));
            m_MissionTitle = CreateText(panel.transform, "Title", "CAPÍTULO CONCLUÍDO", 38, UITheme.Green, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -58f), new Vector2(860f, 56f));
            m_MissionSubtitle = CreateText(panel.transform, "Subtitle", string.Empty, 16, UITheme.Amber, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -102f), new Vector2(860f, 28f));
            m_MissionSummary = CreateText(panel.transform, "Summary", string.Empty, 18, UITheme.Paper, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -146f), new Vector2(840f, 260f));
            m_MissionStats = CreateText(panel.transform, "Stats", string.Empty, 15, UITheme.Dim, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -420f), new Vector2(840f, 60f));
            m_NextChapterButton = CreateButtonBottom(panel.transform, "PRÓXIMO CAPÍTULO", new Vector2(170f, 34f), new Vector2(300f, 52f),
                () => m_Manager.ContinueToNextChapter());
            CreateButtonBottom(panel.transform, "VOLTAR AO MENU", new Vector2(-170f, 34f), new Vector2(300f, 52f), () => m_Manager.ReturnToMenu());
        }

        private void BuildSettings()
        {
            m_Settings = CreatePanel("Settings", m_Canvas.transform, UITheme.Backdrop, true);
            var panel = CreateCenteredPanel("SettingsPanel", m_Settings.transform, new Vector2(820f, 900f));
            CreateText(panel.transform, "Title", "CONFIGURAÇÕES", 32, UITheme.Paper, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(600f, 46f));

            var y = -104f;
            m_Master = AddSliderRow(panel.transform, "Volume geral", y);
            m_Call = AddSliderRow(panel.transform, "Chamadas", y - 52f);
            m_Sfx = AddSliderRow(panel.transform, "Efeitos", y - 104f);
            m_Ambience = AddSliderRow(panel.transform, "Ambiente", y - 156f);
            m_Scare = AddSliderRow(panel.transform, "Sustos", y - 208f);
            m_Sensitivity = AddSliderRow(panel.transform, "Sensibilidade", y - 260f);
            m_Fov = AddSliderRow(panel.transform, "Campo de visão", y - 312f);
            m_SubtitleScale = AddSliderRow(panel.transform, "Tamanho das legendas", y - 364f);

            m_InvertY = AddToggleRow(panel.transform, "Inverter eixo vertical", y - 420f);
            m_Subtitles = AddToggleRow(panel.transform, "Legendas de eventos e ambiente", y - 460f);
            m_ReduceFlashes = AddToggleRow(panel.transform, "Reduzir flashes", y - 500f);
            m_ReduceScare = AddToggleRow(panel.transform, "Reduzir intensidade de sustos", y - 540f);
            m_Fullscreen = AddToggleRow(panel.transform, "Tela cheia", y - 580f);

            m_QualityLabel = CreateText(panel.transform, "QualityLabel", "Qualidade: —", 15, UITheme.Paper, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, y - 630f), new Vector2(300f, 28f));
            CreateButtonInternal(panel.transform, "ALTERAR", new Vector2(360f, y - 630f), new Vector2(180f, 38f), CycleQuality,
                new Vector2(0f, 1f), new Vector2(0f, 1f));
            CreateText(panel.transform, "Note", "Legendas das chamadas são sempre exibidas enquanto as vozes forem placeholders.", 12, UITheme.Dim, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, y - 680f), new Vector2(720f, 24f));

            CreateButtonBottom(panel.transform, "FECHAR", new Vector2(0f, 26f), new Vector2(240f, 46f), () =>
            {
                m_Settings.SetActive(false);
                if (m_SettingsReturn == GameState.Paused) m_Pause.SetActive(true);
                else if (m_SettingsReturn == GameState.MainMenu) m_MainMenu.SetActive(true);
            });
        }

        private void BuildDisconnected()
        {
            m_Disconnected = CreatePanel("Disconnected", m_Canvas.transform, UITheme.Backdrop, true);
            var panel = CreateCenteredPanel("DisconnectedPanel", m_Disconnected.transform, new Vector2(820f, 360f));
            CreateText(panel.transform, "Title", "SESSÃO ENCERRADA", 32, UITheme.Red, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(700f, 46f));
            m_DisconnectedText = CreateText(panel.transform, "Body", string.Empty, 17, UITheme.Paper, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(720f, 140f));
            CreateButtonBottom(panel.transform, "VOLTAR AO MENU", new Vector2(0f, 30f), new Vector2(300f, 50f), () => m_Manager.ReturnToMenu());
        }

        private void BuildStats()
        {
            m_Stats = NewUI("Stats", m_Canvas.transform);
            var rect = m_Stats.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(360f, 120f);
            rect.anchoredPosition = new Vector2(-12f, -90f);
            var bg = m_Stats.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.6f);
            bg.raycastTarget = false;
            m_StatsText = CreateText(m_Stats.transform, "Text", string.Empty, 13, UITheme.Green, TextAnchor.UpperLeft,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            m_StatsText.rectTransform.offsetMin = new Vector2(10f, 6f);
            m_StatsText.rectTransform.offsetMax = new Vector2(-10f, -6f);
            m_Stats.SetActive(false);
        }

        // ------------------------------------------------------------- menus

        private static string ShortTitle(int index)
        {
            switch (index)
            {
                case 0: return "PRÓLOGO";
                default: return "CAPÍTULO " + index;
            }
        }

        public void ShowBriefing(int chapter)
        {
            HideAllScreens();
            SetActive(m_Briefing, true);
            m_Manager?.SetState(GameState.Briefing);
            SelectChapter(Mathf.Clamp(chapter, 0, CampaignCatalog.ChapterCount - 1));
            SyncLoadoutToggles(m_LoadoutToggles);
            RefreshDifficulty();
            SetCursorForMenu(true);
        }

        private void SelectChapter(int chapter)
        {
            var completed = m_Manager != null && m_Manager.Save != null ? m_Manager.Save.chaptersCompleted : null;
            if (!CampaignCatalog.IsUnlocked(chapter, completed))
            {
                m_BriefingTitle.text = CampaignCatalog.Get(chapter).Title;
                m_BriefingBody.text = "Bloqueado. Conclua o capítulo anterior para liberar.";
                return;
            }

            m_SelectedChapter = chapter;
            var info = CampaignCatalog.Get(chapter);
            m_BriefingTitle.text = info.Title;
            var builder = new StringBuilder();
            builder.Append(info.Theme).Append(" — ").Append(info.Novelty).Append("\n\n").Append(info.Briefing).Append("\n\n");
            builder.Append("Recomendado: ");
            for (var i = 0; i < info.Recommended.Length; i++)
            {
                if (i > 0) builder.Append(", ");
                builder.Append(Loadout.Label(info.Recommended[i]));
            }

            if (completed != null && completed.Contains(chapter)) builder.Append("\n\nJá concluído. Jogar de novo gera outra variação.");
            m_BriefingBody.text = builder.ToString();

            for (var i = 0; i < m_ChapterButtons.Count; i++)
            {
                var unlocked = CampaignCatalog.IsUnlocked(i, completed);
                var label = m_ChapterButtons[i].GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = (unlocked ? string.Empty : "[BLOQUEADO] ") + ShortTitle(i) + (completed != null && completed.Contains(i) ? "  (concluído)" : string.Empty);
                    label.color = i == chapter ? UITheme.Amber : unlocked ? UITheme.Paper : UITheme.Dim;
                }
            }
        }

        private void CycleDifficulty()
        {
            if (m_Manager == null) return;
            var next = (DifficultyLevel)(((int)m_Manager.Difficulty.Level + 1) % 3);
            m_Manager.SetDifficulty(next);
            RefreshDifficulty();
        }

        private void RefreshDifficulty()
        {
            if (m_Manager == null || m_BriefingDifficulty == null) return;
            m_BriefingDifficulty.text = "Dificuldade: " + m_Manager.Difficulty.Label + "\n" + m_Manager.Difficulty.Description;
        }

        private void OnLoadoutToggle(EquipmentKind kind, bool value, List<Toggle> toggles)
        {
            if (m_UpdatingLoadout || m_Manager == null) return;
            var loadout = Loadout.FromMask(m_Manager.Loadout.ToMask());
            if (value != loadout.Has(kind))
            {
                if (!loadout.Toggle(kind))
                {
                    ShowSubtitle("EQUIPAMENTO", "No máximo dois equipamentos além da lanterna e do sintonizador.", UITheme.Dim, 3f);
                }
            }

            m_Manager.SetLoadout(loadout);
            m_Manager.ApplyLoadoutToLocalPlayer();
            SyncLoadoutToggles(toggles);
        }

        private void SyncLoadoutToggles(List<Toggle> toggles)
        {
            if (m_Manager == null) return;
            m_UpdatingLoadout = true;
            for (var i = 0; i < toggles.Count; i++) toggles[i].isOn = m_Manager.Loadout.Has((EquipmentKind)i);
            m_UpdatingLoadout = false;
        }

        private void ShowHostPanel()
        {
            HideAllScreens();
            if (m_HostKey != null && string.IsNullOrEmpty(m_HostKey.text)) m_HostKey.text = SessionKeyGenerator.Generate();
            SetActive(m_HostPanel, true);
        }

        private void ShowJoinPanel()
        {
            HideAllScreens();
            SetActive(m_JoinPanel, true);
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

        private void UseInvite()
        {
            if (!InviteCodec.TryDecode(m_JoinInvite != null ? m_JoinInvite.text : string.Empty, out var data, out var error))
            {
                if (m_JoinError != null) m_JoinError.text = error;
                return;
            }

            m_JoinIp.text = data.Ip;
            m_JoinPort.text = data.Port.ToString();
            m_JoinKey.text = data.Key;
            if (m_JoinError != null) m_JoinError.text = string.Empty;
            StartJoin();
        }

        private void StartJoin()
        {
            var ip = m_JoinIp != null ? m_JoinIp.text.Trim() : string.Empty;
            var key = m_JoinKey != null ? m_JoinKey.text : string.Empty;
            if (string.IsNullOrEmpty(ip))
            {
                if (m_JoinError != null) m_JoinError.text = "Informe o IP do host ou cole um convite.";
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

            m_LanSearch?.StopSearch();
            if (m_JoinError != null) m_JoinError.text = string.Empty;
            m_Manager.JoinGame(ip, port, key);
        }

        private void SearchLan()
        {
            if (m_LanSearch == null)
            {
                m_LanSearch = gameObject.AddComponent<LanDiscovery>();
                m_LanSearch.SessionFound += OnLanFound;
                m_LanSearch.SearchFinished += () =>
                {
                    if (m_JoinLanStatus != null)
                    {
                        m_JoinLanStatus.text = m_LanFound.Count == 0
                            ? "Nenhuma sessão encontrada. Firewall, isolamento de Wi-Fi ou roteador podem bloquear broadcast. Use o IP manual."
                            : $"{m_LanFound.Count} sessão(ões) encontrada(s).";
                    }
                };
            }

            m_LanFound.Clear();
            foreach (var button in m_LanButtons) button.gameObject.SetActive(false);
            if (m_JoinLanStatus != null) m_JoinLanStatus.text = "Procurando por 10 s na porta UDP 47777...";
            m_LanSearch.StartSearch(10f);
        }

        private void OnLanFound(LanDiscovery.Found found)
        {
            foreach (var existing in m_LanFound)
            {
                if (existing.Address == found.Address && existing.Beacon.Port == found.Beacon.Port) return;
            }

            if (m_LanFound.Count >= m_LanButtons.Count) return;
            m_LanFound.Add(found);
            var button = m_LanButtons[m_LanFound.Count - 1];
            button.gameObject.SetActive(true);
            var label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = $"{found.Beacon.Name} — {found.Address}:{found.Beacon.Port} — {found.Beacon.PlayerCount}/4" +
                             (found.Beacon.MissionStarted ? " (em andamento)" : string.Empty);
            }
        }

        private void PickLan(int index)
        {
            if (index < 0 || index >= m_LanFound.Count) return;
            var found = m_LanFound[index];
            m_JoinIp.text = found.Address;
            m_JoinPort.text = found.Beacon.Port.ToString();
            if (m_JoinError != null) m_JoinError.text = "Endereço preenchido. Informe a chave combinada com o host.";
        }

        private void CopyInvite()
        {
            var network = m_Manager != null ? m_Manager.Network : null;
            if (network == null || !network.IsHost) return;
            GUIUtility.systemCopyBuffer = network.BuildInvite();
            ShowSubtitle("CONVITE", "Convite copiado. Compartilhe por fora do jogo.", UITheme.Green, 3f);
        }

        private void CycleLobbyChapter()
        {
            var completed = m_Manager != null && m_Manager.Save != null ? m_Manager.Save.chaptersCompleted : null;
            for (var i = 1; i <= CampaignCatalog.ChapterCount; i++)
            {
                var candidate = (m_LobbyChapterIndex + i) % CampaignCatalog.ChapterCount;
                if (CampaignCatalog.IsUnlocked(candidate, completed))
                {
                    m_LobbyChapterIndex = candidate;
                    break;
                }
            }

            RefreshLobbyChapter();
        }

        private void RefreshLobbyChapter()
        {
            if (m_LobbyChapter == null || m_Manager == null) return;
            m_LobbyChapter.text = "CAPÍTULO: " + CampaignCatalog.Get(m_LobbyChapterIndex).Title + "\nDIFICULDADE: " + m_Manager.Difficulty.Label;
        }

        private void CycleQuality()
        {
            var settings = m_Manager != null ? m_Manager.Settings : null;
            if (settings == null) return;
            settings.qualityLevel = (settings.qualityLevel + 1) % GameSettings.QualityLabels.Length;
            settings.NotifyChanged();
            RefreshSettings();
        }

        // ---------------------------------------------------------------- API

        private void HideAllScreens()
        {
            SetActive(m_MainMenu, false);
            SetActive(m_Briefing, false);
            SetActive(m_HostPanel, false);
            SetActive(m_JoinPanel, false);
            SetActive(m_Lobby, false);
            SetActive(m_HUD, false);
            SetActive(m_Pause, false);
            SetActive(m_Notebook, false);
            SetActive(m_Map, false);
            SetActive(m_MissionComplete, false);
            SetActive(m_Settings, false);
            SetActive(m_Disconnected, false);
        }

        public void ShowMainMenu()
        {
            HideAllScreens();
            SetActive(m_MainMenu, true);
            SetCursorForMenu(true);
            if (m_Manager != null && m_Manager.State == GameState.Briefing) m_Manager.SetState(GameState.MainMenu);

            // Fluxo "próximo capítulo": a cena foi recarregada e o briefing abre direto.
            if (GameManager.PendingChapter >= 0 && m_Manager != null && m_Manager.State == GameState.MainMenu)
            {
                var chapter = GameManager.PendingChapter;
                GameManager.PendingChapter = -1;
                ShowBriefing(chapter);
            }
        }

        public void ShowHUD()
        {
            HideAllScreens();
            SetActive(m_HUD, true);
            if (m_Crosshair != null) m_Crosshair.SetActive(true);
        }

        public void ShowLobby(bool isHost)
        {
            HideAllScreens();
            SetActive(m_Lobby, true);
            var network = m_Manager != null ? m_Manager.Network : null;
            if (m_LobbyInfo != null)
            {
                if (isHost && network != null)
                {
                    var addresses = string.Join("  •  ", NetworkBootstrap.LocalAddresses());
                    m_LobbyInfo.text = $"SESSÃO CRIADA — IP local: {addresses}  •  Porta UDP {network.Port}  •  Chave {network.SessionKey}\n" +
                                       "Para internet: encaminhe a porta UDP no roteador e informe o IP público. CGNAT pode impedir a conexão.\n" +
                                       "Atenção: quem se conecta vê o IP do host. Se o host sair, a sessão termina.";
                }
                else
                {
                    m_LobbyInfo.text = "CONECTANDO — aguardando o host iniciar a missão. O host escolhe capítulo e dificuldade.";
                }
            }

            if (m_LobbyInvite != null) m_LobbyInvite.text = isHost && network != null ? "CONVITE: " + network.BuildInvite() : string.Empty;
            if (m_StartMissionButton != null) m_StartMissionButton.gameObject.SetActive(isHost);
            if (m_LobbyChapterButton != null) m_LobbyChapterButton.gameObject.SetActive(isHost);
            if (m_LobbyDifficultyButton != null) m_LobbyDifficultyButton.gameObject.SetActive(isHost);
            m_LobbyChapterIndex = m_Manager != null && m_Manager.Save != null ? CampaignCatalog.NextChapter(m_Manager.Save.chaptersCompleted) : 0;
            RefreshLobbyChapter();
            SyncLoadoutToggles(m_LobbyLoadoutToggles);
            SetCursorForMenu(true);
        }

        public void ShowNetworkError(string message)
        {
            if (m_HostError != null && m_HostPanel != null && m_HostPanel.activeSelf) m_HostError.text = message;
            if (m_JoinError != null) m_JoinError.text = message;
        }

        public void ShowDisconnected(string message)
        {
            HideAllScreens();
            SetActive(m_Disconnected, true);
            if (m_DisconnectedText != null) m_DisconnectedText.text = message;
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
            ApplyNotebookCover();
            RefreshNotebook();
            SetActive(m_Pause, false);
            SetActive(m_Settings, false);
            SetActive(m_Notebook, true);
            SetCursorForMenu(true);
        }

        /// <summary>Capa do caderno: desbloqueio cosmético por final visto (GDD §15).</summary>
        private void ApplyNotebookCover()
        {
            if (m_NotebookCover == null || m_Manager == null || m_Manager.Save == null) return;
            var save = m_Manager.Save;
            var color = new Color(0.16f, 0.15f, 0.12f, 0.97f);
            if (save.unlocked.Contains("cover_3")) color = new Color(0.1f, 0.12f, 0.17f, 0.97f);
            else if (save.unlocked.Contains("cover_2")) color = new Color(0.1f, 0.16f, 0.12f, 0.97f);
            else if (save.unlocked.Contains("cover_1")) color = new Color(0.18f, 0.09f, 0.08f, 0.97f);
            m_NotebookCover.color = color;
        }

        public void ShowMap()
        {
            m_MapOpenedFrame = Time.frameCount;
            RefreshMap();
            SetActive(m_Map, true);
            SetCursorForMenu(true);
        }

        public void RefreshMap()
        {
            if (m_MapBody == null || m_Manager == null || m_Manager.World == null) return;
            var builder = new StringBuilder();
            var current = m_Manager.Sectors != null ? m_Manager.Sectors.Current : null;
            builder.Append("SETORES\n");
            foreach (var sector in m_Manager.World.Sectors)
            {
                if (!sector.Discovered) continue;
                builder.Append(sector == current ? "<color=#E3A64B>► " : "   ").Append(sector.Label);
                if (sector == current) builder.Append("  (você está aqui)</color>");
                builder.Append('\n');
            }

            builder.Append("\nEXTENSÕES CONHECIDAS\n");
            var any = false;
            foreach (var pair in m_Manager.World.Phones)
            {
                var phone = pair.Value;
                if (phone == null) continue;
                var sector = m_Manager.World.SectorAt(phone.transform.position);
                if (sector == null || !sector.Discovered) continue;
                any = true;
                builder.Append("   ").Append(phone.isOperatorPhone ? "CENTRAL (mesa)" : pair.Key >= 1000 ? "EXTENSÃO ZERO" : pair.Key.ToString("000"))
                    .Append(" — ").Append(phone.locationLabel).Append('\n');
            }

            if (!any) builder.Append("   nenhuma\n");
            builder.Append("\nTRONCO 0 — linha morta   •   LINHA 9 — contaminada");
            m_MapBody.text = builder.ToString();
        }

        public void ShowMissionComplete(MissionReport report)
        {
            HideAllScreens();
            SetActive(m_MissionComplete, true);
            if (report == null) report = new MissionReport { Summary = "A linha foi isolada." };
            var ended = report.Ending != EndingKind.None;
            if (m_MissionTitle != null)
            {
                m_MissionTitle.text = ended ? "FINAL — " + EndingRules.Title(report.Ending) : "CAPÍTULO CONCLUÍDO";
                m_MissionTitle.color = ended ? UITheme.Amber : UITheme.Green;
            }

            if (m_MissionSubtitle != null) m_MissionSubtitle.text = report.ChapterTitle ?? string.Empty;
            if (m_MissionSummary != null) m_MissionSummary.text = string.IsNullOrEmpty(report.Summary) ? "A linha foi isolada." : report.Summary;
            if (m_MissionStats != null)
            {
                var minutes = Mathf.FloorToInt(report.Seconds / 60f);
                var seconds = Mathf.FloorToInt(report.Seconds % 60f);
                m_MissionStats.text = $"TEMPO {minutes:00}:{seconds:00}   •   CONTAMINAÇÃO MÁXIMA {report.PeakContamination:0}%   •   FATOS NO CADERNO {report.EvidenceCount}";
            }

            if (m_NextChapterButton != null) m_NextChapterButton.gameObject.SetActive(report.HasNextChapter);
            SetCursorForMenu(true);
        }

        public void HideOverlays()
        {
            SetActive(m_Pause, false);
            SetActive(m_Notebook, false);
            SetActive(m_Map, false);
            SetActive(m_Settings, false);
            SetActive(m_MissionComplete, false);
            SetActive(m_Lobby, false);
            SetActive(m_MainMenu, false);
            SetActive(m_Briefing, false);
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
                var categories = (EvidenceCategory[])System.Enum.GetValues(typeof(EvidenceCategory));
                foreach (var category in categories)
                {
                    var header = false;
                    for (var i = 0; i < model.Entries.Count; i++)
                    {
                        var entry = model.Entries[i];
                        if (entry.Category != category) continue;
                        if (!header)
                        {
                            builder.Append("<color=#E3A64B>").Append(entry.CategoryLabel).Append("</color>\n");
                            header = true;
                        }

                        builder.Append("• <b>").Append(entry.Title).Append("</b>");
                        if (!string.IsNullOrEmpty(entry.Source)) builder.Append("  <i>(").Append(entry.Source).Append(")</i>");
                        builder.Append('\n').Append(entry.Body).Append("\n\n");
                    }
                }
            }

            m_NotebookBody.text = builder.ToString();
            if (m_Notes != null) m_Notes.text = PlayerPrefs.GetString(NotesKey, string.Empty);
        }

        public void UpdatePlayerList(List<NetworkPlayer> players)
        {
            if (m_LobbyPlayers == null) return;
            if (players == null || players.Count == 0)
            {
                m_LobbyPlayers.text = "Nenhum jogador conectado.";
                return;
            }

            var builder = new StringBuilder("INVESTIGADORES (" + players.Count + "/4)\n");
            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player == null) continue;
                var host = player.IsOwnedByServer ? "  [HOST]" : string.Empty;
                var local = player.IsOwner ? "  — VOCÊ" : string.Empty;
                var layers = LayerDistributor.Distribute(players.Count, i);
                var layerText = new StringBuilder();
                foreach (var layer in layers)
                {
                    if (layerText.Length > 0) layerText.Append('+');
                    layerText.Append(LayerDistributor.Label(layer));
                }

                builder.Append($"{i + 1}. Investigador {i + 1}{host}{local} — camadas: {layerText}\n");
            }

            m_LobbyPlayers.text = builder.ToString();
        }

        private bool SubtitlesEnabled => m_Manager == null || m_Manager.Settings == null || m_Manager.Settings.subtitles;

        /// <summary>Mensagem essencial (objetivo, sistema, missão): sempre exibida.</summary>
        public void ShowSubtitle(string title, string body, Color color, float duration = 4f)
        {
            if (m_Subtitle == null || string.IsNullOrEmpty(body)) return;
            m_Subtitle.text = string.IsNullOrEmpty(title) ? body : title + "\n" + body;
            m_Subtitle.color = color;
            m_SubtitleUntil = Time.unscaledTime + Mathf.Max(1f, duration);
            m_SubtitleVisible = true;
            if (m_SubtitleGroup != null) m_SubtitleGroup.alpha = 1f;
            ApplyAccessibility(m_Manager != null ? m_Manager.Settings : null);
        }

        /// <summary>
        /// Legenda de camada da chamada. Com vozes placeholder (sem gravação), é a única forma de
        /// entender a informação e por isso é sempre exibida.
        /// </summary>
        public void ShowCallSubtitle(string label, string text, Color color, float duration, bool essential)
        {
            if (!essential && !SubtitlesEnabled) return;
            ShowSubtitle(label, text, color, duration);
        }

        /// <summary>Legenda de evento/ambiente: respeita a opção de legendas.</summary>
        public void ShowEventSubtitle(string text, Color color, float duration)
        {
            if (!SubtitlesEnabled) return;
            ShowSubtitle(string.Empty, text, color, duration);
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

        public void SetChapterTitle(string title)
        {
            if (m_ChapterTitle != null) m_ChapterTitle.text = title ?? string.Empty;
        }

        public void SetLayerLabel(string layer)
        {
            if (m_Layer != null) m_Layer.text = "CAMADA: " + (layer ?? "—");
        }

        public void SetBattery(float normalized)
        {
            if (m_Battery != null) m_Battery.text = $"LANTERNA: {Mathf.RoundToInt(Mathf.Clamp01(normalized) * 100f)}%";
        }

        public void SetEquipmentLabel(string label)
        {
            if (m_Equipment != null) m_Equipment.text = "CABO: " + (label ?? "—");
        }

        public void SetLoadoutLabel(string label)
        {
            if (m_Loadout != null) m_Loadout.text = label ?? string.Empty;
        }

        public void SetReceiver(string text)
        {
            if (m_Receiver != null) m_Receiver.text = text ?? string.Empty;
        }

        public void SetRecorderLabel(string text)
        {
            if (m_Recorder != null) m_Recorder.text = text ?? string.Empty;
        }

        public void SetLineBusy(bool active, int frequency, int target, float remaining)
        {
            if (m_LineBusyPanel == null) return;
            if (m_LineBusyPanel.activeSelf != active) m_LineBusyPanel.SetActive(active);
            if (!active || m_LineBusyText == null) return;
            m_LineBusyText.text = $"LINHA OCUPADA — {remaining:0} s\nSINTONIZADOR {frequency}   •   RETORNO {target}   (Q gira; segure na frequência)";
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

            RefreshSettings();
            m_Master.onValueChanged.AddListener(v => { settings.masterVolume = v; settings.NotifyChanged(); });
            m_Call.onValueChanged.AddListener(v => { settings.callVolume = v; settings.NotifyChanged(); });
            m_Sfx.onValueChanged.AddListener(v => { settings.sfxVolume = v; settings.NotifyChanged(); });
            m_Ambience.onValueChanged.AddListener(v => { settings.ambienceVolume = v; settings.NotifyChanged(); });
            m_Scare.onValueChanged.AddListener(v => { settings.scareVolume = v; settings.NotifyChanged(); });
            m_Sensitivity.onValueChanged.AddListener(v => { settings.mouseSensitivity = Mathf.Lerp(0.3f, 4f, v); settings.NotifyChanged(); });
            m_Fov.onValueChanged.AddListener(v => { settings.fieldOfView = Mathf.Lerp(60f, 100f, v); settings.NotifyChanged(); });
            m_SubtitleScale.onValueChanged.AddListener(v => { settings.subtitleScale = Mathf.Lerp(0.7f, 1.6f, v); settings.NotifyChanged(); });
            m_Subtitles.onValueChanged.AddListener(v => { settings.subtitles = v; settings.NotifyChanged(); });
            m_ReduceFlashes.onValueChanged.AddListener(v => { settings.reduceFlashes = v; settings.NotifyChanged(); });
            m_ReduceScare.onValueChanged.AddListener(v => { settings.reduceScareIntensity = v; settings.NotifyChanged(); });
            m_InvertY.onValueChanged.AddListener(v => { settings.invertY = v; settings.NotifyChanged(); });
            m_Fullscreen.onValueChanged.AddListener(v => { settings.fullscreen = v; settings.NotifyChanged(); });
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
            m_SubtitleScale.SetValueWithoutNotify(Mathf.InverseLerp(0.7f, 1.6f, settings.subtitleScale));
            m_Subtitles.SetIsOnWithoutNotify(settings.subtitles);
            m_ReduceFlashes.SetIsOnWithoutNotify(settings.reduceFlashes);
            m_ReduceScare.SetIsOnWithoutNotify(settings.reduceScareIntensity);
            m_InvertY.SetIsOnWithoutNotify(settings.invertY);
            m_Fullscreen.SetIsOnWithoutNotify(settings.fullscreen);
            if (m_QualityLabel != null)
            {
                m_QualityLabel.text = "Qualidade: " + GameSettings.QualityLabels[Mathf.Clamp(settings.qualityLevel, 0, GameSettings.QualityLabels.Length - 1)];
            }
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

            UpdateStats();

            if (m_Manager == null) return;
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.f3Key.wasPressedThisFrame && m_Stats != null) m_Stats.SetActive(!m_Stats.activeSelf);
            if (m_Manager.State == GameState.Playing && keyboard.tabKey.wasPressedThisFrame)
            {
                m_Manager.OpenNotebook();
            }
            else if (m_Manager.State == GameState.Notebook && (keyboard.tabKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame))
            {
                m_Manager.CloseNotebook();
            }
            else if (m_Manager.State == GameState.Map && keyboard.mKey.wasPressedThisFrame && Time.frameCount > m_MapOpenedFrame)
            {
                m_Manager.CloseMap();
            }
        }

        /// <summary>Overlay F3 (PERFORMANCE_TARGETS §5): FPS médio, tempo de frame e memória.</summary>
        private void UpdateStats()
        {
            if (m_Stats == null || !m_Stats.activeSelf) return;
            m_StatsFrames++;
            m_StatsTime += Time.unscaledDeltaTime;
            if (Time.unscaledTime < m_StatsNext) return;
            m_StatsNext = Time.unscaledTime + 0.5f;
            var fps = m_StatsTime > 0f ? m_StatsFrames / m_StatsTime : 0f;
            var ms = fps > 0f ? 1000f / fps : 0f;
            m_StatsFrames = 0;
            m_StatsTime = 0f;
            var managed = System.GC.GetTotalMemory(false) / (1024f * 1024f);
            var allocated = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
            var sector = m_Manager != null && m_Manager.Sectors != null && m_Manager.Sectors.Current != null ? m_Manager.Sectors.Current.Label : "—";
            m_StatsText.text = $"FPS {fps:0}  ({ms:0.0} ms)\nMemória alocada {allocated:0} MB  •  gerenciada {managed:0} MB\n" +
                               $"GPU: {SystemInfo.graphicsDeviceName}\nVRAM declarada: {SystemInfo.graphicsMemorySize} MB  •  {SystemInfo.graphicsDeviceType}\nSetor: {sector}";
        }

        private void SetCursorForMenu(bool menu)
        {
            Cursor.lockState = menu ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = menu;
        }

        private Toggle AddToggleRowAt(Transform parent, string label, Vector2 position, float labelWidth)
        {
            return AddToggleRowAt(parent, label, position, labelWidth, new Vector2(0f, 1f), 0f);
        }

        private Toggle AddToggleRowAt(Transform parent, string label, Vector2 position, float labelWidth, Vector2 anchor, float toggleOffsetX)
        {
            var toggle = CreateToggle(parent, anchor, position + new Vector2(toggleOffsetX, 0f));
            CreateText(parent, "Label_" + label, label, 14, UITheme.Paper, TextAnchor.MiddleLeft,
                anchor, anchor, position + new Vector2(toggleOffsetX + 40f, 0f), new Vector2(labelWidth, 28f));
            return toggle;
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
