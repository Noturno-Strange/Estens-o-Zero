using System.Collections.Generic;
using ExtensaoZero.AI;
using ExtensaoZero.Anomaly;
using ExtensaoZero.Audio;
using ExtensaoZero.Environment;
using ExtensaoZero.Investigation;
using ExtensaoZero.Interaction;
using ExtensaoZero.Missions;
using ExtensaoZero.Multiplayer;
using ExtensaoZero.Player;
using ExtensaoZero.Telephone;
using ExtensaoZero.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Phone = ExtensaoZero.Telephone.Telephone;

namespace ExtensaoZero.Core
{
    /// <summary>
    /// Orquestrador da fatia vertical. Mantém referências explícitas para evitar buscas caras em runtime.
    /// </summary>
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameSettings Settings { get; private set; }
        public SaveData Save { get; private set; }
        public UIManager UI { get; private set; }
        public AudioDirector Audio { get; private set; }
        public NotebookSystem Notebook { get; private set; }
        public WorldContext World { get; private set; }
        public CallSessionManager CallSession { get; private set; }
        public ContaminationSystem Contamination { get; private set; }
        public TensionDirector Tension { get; private set; }
        public SubscriberEntity Subscriber { get; private set; }
        public FirstCallMission Mission { get; private set; }
        public NetworkBootstrap Network { get; private set; }
        public NetworkGameState NetworkState { get; private set; }

        public FirstPersonController LocalPlayerController { get; private set; }
        public NetworkPlayer LocalNetworkPlayer { get; private set; }
        public readonly List<NetworkPlayer> Players = new List<NetworkPlayer>();

        public GameState State { get; private set; } = GameState.Boot;
        public float MissionStartTime { get; private set; }

        public bool IsMultiplayer => Network != null && Network.IsRunning;
        public bool IsHost => !IsMultiplayer || (Network != null && Network.IsHost);
        public bool IsPlaying => State == GameState.Playing;

        private float m_PlayTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Initialize()
        {
            Settings = GameSettings.Create(transform);
            Save = SaveSystem.Load();
            m_PlayTime = Save.playTime;

            Audio = AudioDirector.Create(transform);
            UI = UIManager.Create(transform, this);
            Notebook = NotebookSystem.Create(transform, this);

            World = WorldBuilder.Build(this);
            LocalPlayerController = World.PlayerController;

            CallSession = CallSessionManager.Create(transform, this);
            Contamination = ContaminationSystem.Create(transform, this);
            Tension = TensionDirector.Create(transform, this, World);
            Subscriber = SubscriberEntity.Create(transform, this, World);
            Mission = FirstCallMission.Create(transform, this, World);

            foreach (var id in Save.evidenceIds)
            {
                Notebook.RestoreById(id);
            }

            Settings.ApplyAll();
            SetState(GameState.MainMenu);
            UI.ShowMainMenu();
            Debug.Log($"[EZ] Inicialização concluída. Jogadores: {Players.Count}. Estado: {State}.");
        }

        private void Update()
        {
            if (State == GameState.Playing)
            {
                m_PlayTime += Time.deltaTime;
                if (Save != null) Save.playTime = m_PlayTime;
            }

            if (State == GameState.Playing && Input.GetKeyDown(KeyCode.Escape))
            {
                Pause();
            }
        }

        public void StartSolo()
        {
            if (Network != null && Network.IsRunning) Network.Shutdown();
            SetState(GameState.Playing);
            World.SetPlayerActive(true);
            LocalPlayerController.Teleport(World.PlayerSpawn, World.PlayerSpawnRotation);
            UI.ShowHUD();
            SetCursorLocked(true);
            MissionStartTime = Time.time;
            Mission.StartMission();
            Audio.PlayAmbient(World.AmbientZone);
        }

        public void CreateHost(string sessionKey, ushort port)
        {
            if (Network == null) Network = NetworkBootstrap.Create(transform, this);
            Network.StartHost(sessionKey, port);
            SetState(GameState.Lobby);
            UI.ShowLobby(true);
        }

        public void JoinGame(string ip, ushort port, string key)
        {
            if (Network == null) Network = NetworkBootstrap.Create(transform, this);
            Network.StartClient(ip, port, key);
            SetState(GameState.Lobby);
            UI.ShowLobby(false);
        }

        public void StartNetworkMission()
        {
            if (!IsMultiplayer || !IsHost) return;
            Network.MissionStarted = true;
            World.SetPlayerActive(false);
            SetState(GameState.Playing);
            UI.ShowHUD();
            SetCursorLocked(true);
            MissionStartTime = Time.time;
            Mission.StartMission();
            NetworkState?.NotifyMissionStarted();
        }

        public void BeginNetworkMissionAsClient()
        {
            if (State == GameState.Playing) return;
            World.SetPlayerActive(false);
            SetState(GameState.Playing);
            UI.ShowHUD();
            SetCursorLocked(true);
            MissionStartTime = Time.time;
        }

        public void OnNetworkPlayerSpawned(NetworkPlayer player)
        {
            if (player == null) return;
            if (!Players.Contains(player)) Players.Add(player);
            if (player.IsOwner)
            {
                LocalNetworkPlayer = player;
                LocalPlayerController = player.Controller;
                World.SetPlayerActive(false);
                if (World.Player != null) World.Player.SetActive(false);
                UI.ShowHUD();
                SetCursorLocked(State == GameState.Playing);
            }

            UI.UpdatePlayerList(Players);
        }

        public void OnNetworkPlayerDespawned(NetworkPlayer player)
        {
            if (player == null) return;
            Players.Remove(player);
            if (LocalNetworkPlayer == player) LocalNetworkPlayer = null;
            UI.UpdatePlayerList(Players);
        }

        public void Pause()
        {
            if (State != GameState.Playing) return;
            SetState(GameState.Paused);
            UI.ShowPause();
            SetCursorLocked(false);
        }

        public void Resume()
        {
            if (State != GameState.Paused && State != GameState.Notebook) return;
            SetState(GameState.Playing);
            UI.HideOverlays();
            SetCursorLocked(true);
        }

        public void OpenNotebook()
        {
            if (State != GameState.Playing) return;
            SetState(GameState.Notebook);
            UI.ShowNotebook();
            SetCursorLocked(false);
        }

        public void CloseNotebook()
        {
            if (State != GameState.Notebook) return;
            Resume();
        }

        public void SetState(GameState state)
        {
            State = state;
            if (UI != null) UI.OnStateChanged(state);
        }

        public void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        public void CompleteMission()
        {
            if (State == GameState.MissionComplete) return;
            Save.missionCompleted = true;
            Save.missionStep = Mission != null ? Mission.StepIndex : 0;
            SaveSystem.Save(Save);
            SetState(GameState.MissionComplete);
            SetCursorLocked(false);
            UI.ShowMissionComplete(Mission != null ? Mission.Summary : string.Empty);
            if (IsMultiplayer && IsHost) NetworkState?.NotifyMissionComplete();
        }

        public void ReturnToMenu()
        {
            if (Network != null) Network.Shutdown();
            SaveSystem.Save(Save);
            Instance = null;
            Destroy(gameObject);
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex >= 0 ? scene.buildIndex : 0);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void OnPhoneAnswered(Phone phone)
        {
            if (IsMultiplayer && !IsHost) return;
            Mission?.OnPhoneAnswered(phone);
        }

        public void OnCallFinished(Phone phone)
        {
            if (IsMultiplayer && !IsHost) return;
            Mission?.OnCallFinished(phone);
        }

        public void OnCablePicked()
        {
            if (IsMultiplayer && !IsHost) return;
            Mission?.OnCablePicked();
        }

        public void OnConnectionResult(bool correct, int a, int b)
        {
            if (IsMultiplayer && !IsHost) return;
            Mission?.OnConnectionResult(correct, a, b);
        }

        public void OnCorridorEntered()
        {
            if (IsMultiplayer && !IsHost) return;
            Mission?.OnCorridorEntered();
        }

        public void OnIsolatorPulled()
        {
            if (IsMultiplayer && !IsHost) return;
            Mission?.OnIsolatorPulled();
        }

        public void RegisterEvidence(EvidenceEntry entry)
        {
            if (entry == null) return;
            if (!Notebook.Register(entry)) return;
            if (!Save.evidenceIds.Contains(entry.Id)) Save.evidenceIds.Add(entry.Id);
            SaveSystem.Save(Save);
            UI.RefreshNotebook();
            if (IsMultiplayer && IsHost) NetworkState?.BroadcastEvidence(entry);
        }

        public void AdvanceMissionStep(int step)
        {
            if (Save != null) Save.missionStep = step;
            if (IsMultiplayer && IsHost) NetworkState?.SetMissionStep(step);
            Tension?.SetMissionStep(step);
        }

        public void AttachNetworkState(NetworkGameState state)
        {
            NetworkState = state;
        }

        public void OnRemoteMissionComplete()
        {
            if (State == GameState.MissionComplete) return;
            Save.missionCompleted = true;
            SaveSystem.Save(Save);
            SetState(GameState.MissionComplete);
            SetCursorLocked(false);
            UI?.ShowMissionComplete("A linha foi isolada pelo host. A origem da chamada permanece desconhecida.");
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            InteractionRegistry.Clear();
        }
    }
}
