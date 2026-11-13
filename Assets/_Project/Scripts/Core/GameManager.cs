using System.Collections.Generic;
using ExtensaoZero.AI;
using ExtensaoZero.Anomaly;
using ExtensaoZero.Audio;
using ExtensaoZero.Campaign;
using ExtensaoZero.Environment;
using ExtensaoZero.Investigation;
using ExtensaoZero.Interaction;
using ExtensaoZero.Missions;
using ExtensaoZero.Multiplayer;
using ExtensaoZero.Player;
using ExtensaoZero.Telephone;
using ExtensaoZero.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Phone = ExtensaoZero.Telephone.Telephone;

namespace ExtensaoZero.Core
{
    /// <summary>
    /// Orquestrador do jogo. Mantém referências explícitas para evitar buscas caras em runtime.
    /// Regra de rede: a lógica de missão roda só no host; o mundo de cada capítulo é preparado
    /// de forma determinística em todos os peers; interações são pedidas ao host e executadas
    /// em todos os peers na mesma ordem.
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
        public MissionBase Mission { get; private set; }
        public NetworkBootstrap Network { get; private set; }
        public NetworkGameState NetworkState { get; private set; }
        public SectorSystem Sectors { get; private set; }

        public FirstPersonController LocalPlayerController { get; private set; }
        public NetworkPlayer LocalNetworkPlayer { get; private set; }
        public readonly List<NetworkPlayer> Players = new List<NetworkPlayer>();

        public GameState State { get; private set; } = GameState.Boot;
        public float MissionStartTime { get; private set; }
        public int Chapter { get; private set; }
        public int Seed { get; private set; } = 1337;
        public DifficultyProfile Difficulty { get; private set; } = DifficultyProfile.Get(DifficultyLevel.Operation);
        public Loadout Loadout { get; private set; } = new Loadout();
        public float PeakContamination { get; private set; }

        /// <summary>Cliente que executou a interação em andamento (host). 0 em solo.</summary>
        public ulong CurrentActorClientId { get; private set; }

        /// <summary>Host: define quem executou a ação reportada (para Linha Ocupada e eventos).</summary>
        public void SetActor(ulong clientId)
        {
            CurrentActorClientId = clientId;
        }

        public bool IsMultiplayer => Network != null && Network.IsRunning;
        public bool IsHost => !IsMultiplayer || (Network != null && Network.IsHost);
        public bool IsPlaying => State == GameState.Playing;
        public int PlayerCount => IsMultiplayer ? Mathf.Max(1, Players.Count) : 1;

        private float m_PlayTime;
        private readonly Dictionary<ulong, float> m_BusyDeadlines = new Dictionary<ulong, float>();
        private int m_LineBusyOccurrence;

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
            Difficulty = DifficultyProfile.Get((DifficultyLevel)Mathf.Clamp(Save.difficulty, 0, 2));
            Loadout = Loadout.FromMask(Save.loadoutMask);

            Audio = AudioDirector.Create(transform);
            UI = UIManager.Create(transform, this);
            Notebook = NotebookSystem.Create(transform, this);

            World = WorldBuilder.Build(this);
            LocalPlayerController = World.PlayerController;
            Sectors = SectorSystem.Create(transform, this, World);

            CallSession = CallSessionManager.Create(transform, this);
            Contamination = ContaminationSystem.Create(transform, this);
            Tension = TensionDirector.Create(transform, this, World);
            Subscriber = SubscriberEntity.Create(transform, this, World);
            Mission = MissionFactory.Create(0, transform, this, World);

            foreach (var id in Save.evidenceIds)
            {
                Notebook.RestoreById(id);
            }

            Settings.ApplyAll();
            SetState(GameState.MainMenu);
            UI.ShowMainMenu();
            Debug.Log($"[EZ] Inicialização concluída. Capítulos concluídos: {Save.chaptersCompleted.Count}. Estado: {State}.");
        }

        private void Update()
        {
            if (State == GameState.Playing)
            {
                m_PlayTime += Time.deltaTime;
                if (Save != null) Save.playTime = m_PlayTime;
                if (Contamination != null) PeakContamination = Mathf.Max(PeakContamination, Contamination.Value);
            }

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                if (State == GameState.Playing) Pause();
                else if (State == GameState.Paused) Resume();
                else if (State == GameState.Map) CloseMap();
            }

            if (IsMultiplayer && IsHost && m_BusyDeadlines.Count > 0) CheckAbandonment();
        }

        // ------------------------------------------------------------ capítulos

        public void SetDifficulty(DifficultyLevel level)
        {
            Difficulty = DifficultyProfile.Get(level);
            if (Save != null)
            {
                Save.difficulty = (int)level;
                SaveSystem.Save(Save);
            }
        }

        public void SetLoadout(Loadout loadout)
        {
            Loadout = loadout ?? new Loadout();
            if (Save != null)
            {
                Save.loadoutMask = Loadout.ToMask();
                SaveSystem.Save(Save);
            }
        }

        /// <summary>Atalho do modo individual: inicia o prólogo (compatível com a fatia).</summary>
        public void StartSolo()
        {
            StartChapter(0);
        }

        /// <summary>Inicia um capítulo no modo individual.</summary>
        public void StartChapter(int chapter)
        {
            if (Network != null && Network.IsRunning) Network.Shutdown();
            BeginChapter(chapter, SeedVariation.NewSeed(), Difficulty.Level, true);
        }

        /// <summary>
        /// Prepara o capítulo em qualquer peer. <paramref name="runLogic"/> é verdadeiro apenas
        /// no host (ou em solo): somente ele executa a máquina de estados da missão.
        /// </summary>
        public void BeginChapter(int chapter, int seed, DifficultyLevel difficulty, bool runLogic)
        {
            Chapter = Mathf.Clamp(chapter, 0, CampaignCatalog.ChapterCount - 1);
            Seed = seed;
            Difficulty = DifficultyProfile.Get(difficulty);
            PeakContamination = 0f;
            m_BusyDeadlines.Clear();
            m_LineBusyOccurrence = 0;

            if (Mission != null && Mission.ChapterIndex != Chapter)
            {
                Destroy(Mission.gameObject);
                Mission = null;
            }

            if (Mission == null) Mission = MissionFactory.Create(Chapter, transform, this, World);

            Contamination.Set(0f);
            Tension.Configure(Seed, Difficulty);
            Subscriber.Configure(Chapter);
            Mission.SetupWorld(Seed);

            var spawn = World.GetSpawn(Chapter);
            if (!IsMultiplayer)
            {
                World.SetPlayerActive(true);
                LocalPlayerController = World.PlayerController;
                LocalPlayerController.Teleport(spawn.position, spawn.rotation);
            }
            else
            {
                World.SetPlayerActive(false);
                if (LocalPlayerController != null) LocalPlayerController.Teleport(SpreadSpawn(spawn.position), spawn.rotation);
            }

            ApplyLoadoutToLocalPlayer();
            SetState(GameState.Playing);
            UI.ShowHUD();
            UI.SetChapterTitle(CampaignCatalog.Get(Chapter).Title);
            SetCursorLocked(true);
            MissionStartTime = Time.time;
            Sectors.Refresh(true);

            if (runLogic) Mission.StartMission();
            Debug.Log($"[EZ] Capítulo {Chapter} iniciado. Seed {Seed}. Dificuldade {Difficulty.Label}. Lógica local: {runLogic}.");
        }

        private Vector3 SpreadSpawn(Vector3 basePosition)
        {
            var index = LocalPlayerIndex;
            return basePosition + new Vector3((index % 2) * 0.9f - 0.45f, 0f, (index / 2) * -0.9f);
        }

        public void ApplyLoadoutToLocalPlayer()
        {
            if (LocalPlayerController == null) return;
            var equipment = LocalPlayerController.GetComponent<PlayerEquipment>();
            if (equipment != null) equipment.ApplyLoadout(Loadout);
        }

        // ------------------------------------------------------------ rede

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

        public void StartNetworkMission(int chapter)
        {
            if (!IsMultiplayer || !IsHost || NetworkState == null) return;
            Network.MissionStarted = true;
            NetworkState.StartMissionForAll(chapter, SeedVariation.NewSeed(), (int)Difficulty.Level);
        }

        /// <summary>Chamado em todos os peers (ClientRpc) quando o host inicia a missão.</summary>
        public void OnNetworkMissionStarted(int chapter, int seed, int difficulty)
        {
            if (State == GameState.Playing) return;
            BeginChapter(chapter, seed, (DifficultyLevel)Mathf.Clamp(difficulty, 0, 2), IsHost);
        }

        public void OnNetworkPlayerSpawned(NetworkPlayer player)
        {
            if (player == null) return;
            if (!Players.Contains(player)) Players.Add(player);
            Players.Sort((a, b) => a.OwnerClientId.CompareTo(b.OwnerClientId));
            if (player.IsOwner)
            {
                LocalNetworkPlayer = player;
                LocalPlayerController = player.Controller;
                World.SetPlayerActive(false);
                ApplyLoadoutToLocalPlayer();
                SetCursorLocked(State == GameState.Playing);
            }

            UI.UpdatePlayerList(Players);
            CallSession?.RefreshAssignment();
        }

        public void OnNetworkPlayerDespawned(NetworkPlayer player)
        {
            if (player == null) return;
            Players.Remove(player);
            m_BusyDeadlines.Remove(player.OwnerClientId);
            if (LocalNetworkPlayer == player) LocalNetworkPlayer = null;
            UI.UpdatePlayerList(Players);
            CallSession?.RefreshAssignment();
        }

        public int LocalPlayerIndex => IndexOfClient(LocalNetworkPlayer != null ? LocalNetworkPlayer.OwnerClientId : 0UL);

        public int IndexOfClient(ulong clientId)
        {
            if (!IsMultiplayer) return 0;
            for (var i = 0; i < Players.Count; i++)
            {
                if (Players[i] != null && Players[i].OwnerClientId == clientId) return i;
            }

            return 0;
        }

        public NetworkPlayer FindPlayer(ulong clientId)
        {
            for (var i = 0; i < Players.Count; i++)
            {
                if (Players[i] != null && Players[i].OwnerClientId == clientId) return Players[i];
            }

            return null;
        }

        /// <summary>Extensão pessoal de cada investigador: 201 + índice (usada pela Linha Ocupada).</summary>
        public int ExtensionOfClient(ulong clientId) => 201 + IndexOfClient(clientId);

        public void AttachNetworkState(NetworkGameState state)
        {
            NetworkState = state;
        }

        public void OnDisconnectedFromHost(string reason)
        {
            SetState(GameState.Disconnected);
            SetCursorLocked(false);
            UI.ShowDisconnected(string.IsNullOrEmpty(reason) ? "A conexão com o host foi encerrada." : reason);
        }

        // ------------------------------------------------------------ interações

        /// <summary>
        /// Entrada única das interações do jogador local. Em solo executa direto; em rede pede ao
        /// host, que valida distância/cooldown e manda todos os peers executarem.
        /// </summary>
        public void RequestInteraction(IInteractable target, PlayerInteractor interactor)
        {
            if (target == null || interactor == null) return;
            if (!IsMultiplayer || LocalNetworkPlayer == null)
            {
                CurrentActorClientId = 0;
                target.Interact(interactor);
                return;
            }

            LocalNetworkPlayer.RequestInteract(target.InteractionId);
        }

        /// <summary>Executado em todos os peers, na ordem definida pelo host.</summary>
        public void ExecuteReplicatedInteraction(int interactionId, ulong actorClientId)
        {
            var target = InteractionRegistry.Get(interactionId);
            var actor = FindPlayer(actorClientId);
            var interactor = actor != null ? actor.GetComponent<PlayerInteractor>() : null;
            if (target == null || interactor == null) return;
            CurrentActorClientId = actorClientId;
            target.Interact(interactor);
        }

        public void RequestDropCable(PlayerEquipment equipment)
        {
            if (equipment == null) return;
            if (!IsMultiplayer || LocalNetworkPlayer == null)
            {
                equipment.DropCable();
                return;
            }

            LocalNetworkPlayer.RequestDropCable();
        }

        // ------------------------------------------------------------ estado

        public void Pause()
        {
            if (State != GameState.Playing) return;
            SetState(GameState.Paused);
            UI.ShowPause();
            SetCursorLocked(false);
        }

        public void Resume()
        {
            if (State != GameState.Paused && State != GameState.Notebook && State != GameState.Map) return;
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

        public void OpenMap()
        {
            if (State != GameState.Playing) return;
            SetState(GameState.Map);
            UI.ShowMap();
            SetCursorLocked(false);
        }

        public void CloseMap()
        {
            if (State != GameState.Map) return;
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

        // ------------------------------------------------------------ conclusão

        /// <summary>Conclusão no host (ou solo). Replica para os clientes.</summary>
        public void CompleteMission()
        {
            if (State == GameState.MissionComplete) return;
            var summary = Mission != null ? Mission.Summary : string.Empty;
            var ending = Mission != null ? Mission.Ending : EndingKind.None;
            ApplyCompletion(summary, ending);
            if (IsMultiplayer && IsHost) NetworkState?.NotifyMissionComplete(summary, (int)ending);
        }

        public void OnRemoteMissionComplete(string summary, int ending)
        {
            if (State == GameState.MissionComplete) return;
            ApplyCompletion(string.IsNullOrEmpty(summary) ? "A missão foi concluída pelo host." : summary, (EndingKind)ending);
        }

        private void ApplyCompletion(string summary, EndingKind ending)
        {
            Save.missionCompleted = true;
            Save.missionStep = Mission != null ? Mission.StepIndex : 0;
            if (!Save.chaptersCompleted.Contains(Chapter)) Save.chaptersCompleted.Add(Chapter);
            Save.chapter = CampaignCatalog.NextChapter(Save.chaptersCompleted);
            Save.Unlock(CampaignCatalog.Get(Chapter).Id);
            if (ending != EndingKind.None)
            {
                if (!Save.endingsSeen.Contains((int)ending)) Save.endingsSeen.Add((int)ending);
                Save.Unlock("ending_" + (int)ending);
                Save.Unlock("cover_" + (int)ending);
            }

            SaveSystem.Save(Save);
            CallSession?.StopCall();
            Contamination?.EndLineBusy(false);
            SetState(GameState.MissionComplete);
            SetCursorLocked(false);

            var report = new MissionReport
            {
                ChapterTitle = CampaignCatalog.Get(Chapter).Title,
                Summary = summary,
                Seconds = Time.time - MissionStartTime,
                PeakContamination = PeakContamination,
                EvidenceCount = Notebook != null ? Notebook.Count : 0,
                Ending = ending,
                HasNextChapter = Chapter + 1 < CampaignCatalog.ChapterCount && !IsMultiplayer
            };
            UI.ShowMissionComplete(report);
        }

        public void ContinueToNextChapter()
        {
            Save.chapter = Mathf.Min(Chapter + 1, CampaignCatalog.ChapterCount - 1);
            SaveSystem.Save(Save);
            PendingChapter = Save.chapter;
            ReturnToMenu();
        }

        /// <summary>Capítulo a abrir no briefing após recarregar a cena (fluxo "próximo capítulo").</summary>
        public static int PendingChapter = -1;

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

        // ------------------------------------------------------------ eventos de missão (host)

        public void OnPhoneAnswered(Phone phone)
        {
            if (!IsHost) return;
            Mission?.OnPhoneAnswered(phone);
        }

        public void OnCallFinished(Phone phone)
        {
            if (!IsHost) return;
            Mission?.OnCallFinished(phone);
        }

        public void OnCablePicked(string label = "CABO 214")
        {
            if (!IsHost) return;
            Mission?.OnCablePicked(label);
        }

        public ConnectionVerdict EvaluateConnection(int a, int b)
        {
            if (!IsHost) return ConnectionVerdict.Neutral;
            return Mission != null ? Mission.EvaluateConnection(a, b) : MissionBase.DefaultVerdict(a, b);
        }

        public void OnConnectionResult(ConnectionVerdict verdict, int a, int b)
        {
            if (!IsHost) return;
            Mission?.OnConnectionResult(verdict, a, b);
        }

        public void OnCorridorEntered()
        {
            if (!IsHost) return;
            Mission?.OnCorridorEntered();
        }

        public void OnIsolatorPulled()
        {
            if (!IsHost) return;
            Mission?.OnIsolatorPulled();
        }

        /// <summary>Evento genérico do mundo ("breaker:2", "tape:call_1979", "zone:tunnels"...).</summary>
        public void RaiseEvent(string id)
        {
            if (!IsHost || string.IsNullOrEmpty(id)) return;
            if (id == "radio")
            {
                // Rádio de apoio da recepção: repete o briefing do capítulo para a equipe.
                Announce("RÁDIO — HELENA", CampaignCatalog.Get(Chapter).Briefing.Replace("Helena: ", string.Empty), UITheme.Paper, 9f);
                Command("radio.reset");
            }

            Mission?.HandleEvent(id);
        }

        // ------------------------------------------------------------ comunicação do host

        /// <summary>Mensagem de missão: aparece para todos os jogadores.</summary>
        public void Announce(string title, string body, Color color, float duration = 5f)
        {
            UI?.ShowSubtitle(title, body, color, duration);
            if (IsMultiplayer && IsHost) NetworkState?.BroadcastSubtitle(title, body, color, duration);
        }

        public void SetObjective(string objective)
        {
            UI?.SetObjective(objective);
            if (IsMultiplayer && IsHost) NetworkState?.BroadcastObjective(objective);
        }

        /// <summary>Comando de mundo determinístico (portas, luzes, objetos). Aplicado em todos os peers.</summary>
        public void Command(string command)
        {
            WorldCommands.Apply(this, command);
            if (IsMultiplayer && IsHost) NetworkState?.BroadcastCommand(command);
        }

        public void RingPhone(int extension, string callId)
        {
            var phone = World.GetPhone(extension);
            var call = CallCatalog.Get(callId);
            if (phone == null || call == null) return;
            phone.BeginRing(call);
            if (IsMultiplayer && IsHost) NetworkState?.BroadcastRing(extension, callId);
        }

        public void RegisterEvidence(EvidenceEntry entry)
        {
            RegisterEvidence(entry, false);
        }

        public void RegisterEvidence(EvidenceEntry entry, bool fromHost)
        {
            if (entry == null) return;
            if (!Notebook.Register(entry)) return;
            if (IsMultiplayer && !IsHost && !fromHost) LocalNetworkPlayer?.ReportEvidence(entry);
            if (!Save.evidenceIds.Contains(entry.Id)) Save.evidenceIds.Add(entry.Id);
            SaveSystem.Save(Save);
            UI.RefreshNotebook();
            if (IsMultiplayer && IsHost) NetworkState?.BroadcastEvidence(entry);
            if (IsHost) Mission?.OnEvidence(entry.Id);
        }

        public void AdvanceMissionStep(int step)
        {
            if (Save != null) Save.missionStep = step;
            if (IsMultiplayer && IsHost) NetworkState?.SetMissionStep(step);
            Tension?.SetMissionStep(step);
        }

        private readonly Dictionary<string, int> m_TeamHeard = new Dictionary<string, int>();

        /// <summary>Camadas ouvidas por este peer; o host une as de toda a equipe.</summary>
        public void ReportLayersHeard(string callId, int mask)
        {
            if (string.IsNullOrEmpty(callId)) return;
            if (IsMultiplayer && !IsHost)
            {
                LocalNetworkPlayer?.ReportLayersHeard(callId, mask);
                return;
            }

            OnLayersHeardOnHost(callId, mask);
        }

        public void OnLayersHeardOnHost(string callId, int mask)
        {
            m_TeamHeard.TryGetValue(callId, out var before);
            var after = before | mask;
            if (after == before) return;
            m_TeamHeard[callId] = after;
            RaiseEvent($"heard:{callId}:{after}");
        }

        public void AddCampaignFlag(string flag)
        {
            if (Save == null || !Save.AddFlag(flag)) return;
            SaveSystem.Save(Save);
            Debug.Log($"[EZ] Marca de campanha: {flag}");
        }

        public void AddIsolatedCircuit()
        {
            if (Save == null) return;
            Save.isolatedCircuits++;
            SaveSystem.Save(Save);
        }

        // ------------------------------------------------------------ Linha Ocupada

        /// <summary>
        /// Chamado pelo host quando a contaminação cruza o limiar. Em solo, isola o jogador local;
        /// em rede, isola o jogador que causou o erro (ou o host).
        /// </summary>
        public void TriggerLineBusy()
        {
            if (!IsHost) return;
            var duration = LineBusyRules.ClampDuration(Difficulty.LineBusySeconds);
            var frequency = LineBusyRules.ReturnFrequency(Seed, m_LineBusyOccurrence++, 0);
            if (!IsMultiplayer)
            {
                World.Switchboard?.SetCompromised(ExtensionOfClient(0), true);
                Contamination.BeginLineBusy(duration, frequency);
                return;
            }

            var target = CurrentActorClientId;
            if (FindPlayer(target) == null && Players.Count > 0) target = Players[0].OwnerClientId;
            m_BusyDeadlines[target] = Time.time + duration + 0.5f;
            NetworkState?.BroadcastLineBusy(target, duration, frequency, ExtensionOfClient(target));
        }

        /// <summary>Recebido em todos os peers: o alvo entra em Linha Ocupada; os demais veem a extensão comprometida.</summary>
        public void OnLineBusyBroadcast(ulong target, float duration, int frequency, int extension)
        {
            World.Switchboard?.SetCompromised(extension, true);
            if (LocalNetworkPlayer != null && LocalNetworkPlayer.OwnerClientId == target)
            {
                Contamination.BeginLineBusy(duration, frequency);
            }
            else
            {
                UI.ShowSubtitle("CENTRAL", $"A extensão {extension} está comprometida. Restaurem a conexão no painel.", UITheme.Red, 5f);
            }
        }

        /// <summary>Host: um jogador saiu da Linha Ocupada (por frequência ou restauração da equipe).</summary>
        public void OnLineBusyRecovered(ulong clientId)
        {
            if (!IsHost) return;
            if (IsMultiplayer)
            {
                if (!m_BusyDeadlines.Remove(clientId)) return;
                NetworkState?.BroadcastLineBusyEnded(clientId, ExtensionOfClient(clientId));
            }

            Contamination.Reduce(12f);
            RaiseEvent("linebusy.recovered");
        }

        public void OnLineBusyEndedBroadcast(ulong target, int extension)
        {
            World.Switchboard?.SetCompromised(extension, false);
            if (LocalNetworkPlayer != null && LocalNetworkPlayer.OwnerClientId == target) Contamination.EndLineBusy(true);
        }

        /// <summary>Restauração pela equipe: alguém interage com a extensão comprometida na central.</summary>
        public void RestoreCompromisedExtension(int extension)
        {
            if (!IsMultiplayer)
            {
                World.Switchboard?.SetCompromised(extension, false);
                if (Contamination.IsLineBusy)
                {
                    Contamination.EndLineBusy(true);
                    OnLineBusyRecovered(0);
                }

                return;
            }

            if (!IsHost) return;
            foreach (var pair in new List<KeyValuePair<ulong, float>>(m_BusyDeadlines))
            {
                if (ExtensionOfClient(pair.Key) == extension) OnLineBusyRecovered(pair.Key);
            }
        }

        private void CheckAbandonment()
        {
            List<ulong> expired = null;
            foreach (var pair in m_BusyDeadlines)
            {
                if (Time.time < pair.Value) continue;
                expired ??= new List<ulong>();
                expired.Add(pair.Key);
            }

            if (expired == null) return;
            foreach (var clientId in expired)
            {
                m_BusyDeadlines.Remove(clientId);
                // Ninguém restaurou a linha a tempo: conta como abandono (requisito do final "Atender").
                AddCampaignFlag(EndingRules.FlagAbandoned);
                NetworkState?.BroadcastLineBusyEnded(clientId, ExtensionOfClient(clientId));
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            InteractionRegistry.Clear();
        }
    }
}
