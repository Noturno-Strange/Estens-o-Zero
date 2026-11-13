# EXTENSÃO ZERO — TECHNICAL ARCHITECTURE

## 1. Stack

| Componente | Versão | Observação |
|---|---|---|
| Unity | 6000.3.28f1 (Unity 6.3 LTS) | Linux x86_64 |
| URP | 17.3.0 | pipeline prioritário |
| C# | .NET Standard 2.1 / C# 9 | scripts |
| Netcode for GameObjects | 2.14.0 | host-cliente |
| Unity Transport | 2.6.0 | UDP direto |
| Input System | 1.20.1 | teclado/mouse (gameplay) |
| uGUI | 2.0.0 | interface (fonte built-in) |
| Test Framework | 1.6.0 | EditMode/PlayMode |
| glTFast | 6.20.0 | importação dos modelos CC0 (glTF) |
| Linux Build Support | Mono | build nativo |

Nenhum serviço externo obrigatório. Nenhuma conta. Nenhum relay, lobby ou matchmaker.

## 2. Princípios

- Host autoritativo para estado compartilhado; interações aprovadas são executadas em todos os peers na mesma ordem (ADR-015).
- Lógica de regras em classes puras, testáveis sem cena (`Campaign/*`, `SwitchboardRules`, `TensionSelector`, `LineBusyRules`, `InviteCodec`, `LanBeacon`, `ConnectionPayload`, `LayerDistributor`).
- Conteúdo configurável em tipos `ScriptableObject` (`CallDefinition`, `TensionEventDefinition`), criados em memória por catálogos.
- Missões como máquinas de estado pequenas, uma classe por capítulo (ADR-014).
- Sem abstrações especulativas, sem ECS e sem física complexa de cabos.
- O jogo roda no editor e em build Linux sem etapas manuais ocultas.

## 3. Estrutura de pastas

```
Assets/_Project/
  Resources/          NetworkPlayer.prefab, NetworkGameState.prefab, EZ_Default.mat
  Scenes/             Bootstrap.unity (cena única)
  Settings/           URP asset, renderer, volume profile
  Scripts/
    Core/             GameBootstrap, GameManager, GameSettings, GameState, SaveSystem
    Campaign/         CampaignCatalog, ChapterInfo, DifficultyProfile, Loadout, EndingRules, SeedVariation
    Missions/         MissionBase, MissionFactory, ConnectionVerdict, FirstCallMission (prólogo),
                      CentralMission, VoiceArchiveMission, DeadLineMission, OtherSideMission, ExtensionZeroMission
    Player/           FirstPersonController, PlayerInteractor, PlayerEquipment, Tuner, Recorder, Flashlight
    Interaction/      InteractableBase, DoorInteractable, ExamineInteractable, CablePickup, TapeReel,
                      JunctionBox, BreakerSwitch, MissionSwitch, EndingConsole, IsolatorLever,
                      BatteryStation, ZoneTrigger, CorridorTrigger, ISignalSource/ICircuitReadable
    Telephone/        CallDefinition, CallCatalog, CallSessionManager, Telephone, Switchboard(+Rules), SwitchboardSocket, CableVisual
    Investigation/    Evidence, NotebookModel, NotebookSystem + EvidenceCatalog
    Anomaly/          TensionDirector, TensionSelector, TensionCatalog, TensionEventDefinition,
                      TensionEventRunner, ContaminationSystem, LineBusyRules
    AI/               SubscriberEntity, ManifestationMode
    Multiplayer/      NetworkBootstrap, NetworkPlayer, NetworkGameState, LanDiscovery, LanBeacon,
                      InviteCodec, SessionCodec, SessionKeyGenerator, ConnectionPayload, LayerDistributor
    Environment/      WorldBuilder, WorldContext, WorldCommands, SectorSystem, SectorInfo,
                      LightingRig, MaterialLibrary, PropFactory
    Audio/            ProceduralAudio, AudioDirector, OcclusionFilter
    UI/               UIManager, UITheme, MissionReport
  Tests/EditMode/     lógica pura
  Tests/PlayMode/     fluxo do prólogo
  Editor/             ProjectSetup, BuildScript
tools/compile-check/  verificação sem Unity (referências + stubs + NUnit)
```

## 4. Bootstrap

A cena `Bootstrap` é vazia. `GameBootstrap` (`RuntimeInitializeOnLoadMethod(AfterSceneLoad)`) cria o `GameManager`, que monta, nesta ordem:

1. `GameSettings` e `SaveSystem` (migração do save v1 → v2).
2. `AudioDirector`, `UIManager`, `NotebookSystem`.
3. `WorldBuilder`: a instalação inteira, com 9 setores, portas, telefones, central, caixas, disjuntores, consoles, jogador e luzes.
4. `SectorSystem`, `CallSessionManager`, `ContaminationSystem`, `TensionDirector`, `SubscriberEntity`.
5. A missão do prólogo (substituída pela do capítulo escolhido).
6. Menu principal.

A ordem de construção do mundo define os IDs de interação, que são iguais em todos os peers. **Não reordene o `WorldBuilder` sem considerar a rede.**

## 5. Fluxo de estado

```
MainMenu → Briefing → Playing ⇄ Paused / Notebook / Map → MissionComplete → (próximo capítulo) MainMenu
MainMenu → Lobby → Playing …                                     ↘ Disconnected
```

`GameManager.BeginChapter(chapter, seed, difficulty, runLogic)` roda em todos os peers:

1. Reseta o mundo (`WorldCommands.ResetForChapter`): portas do capítulo, itens, telefones, central, caixas, disjuntores e consoles.
2. Configura o Diretor de Tensão (seed + dificuldade) e a Assinante (capítulo).
3. `Mission.SetupWorld(seed)` aplica as variações determinísticas e registra chamadas geradas pela seed.
4. Posiciona o jogador. Só com `runLogic` (host/solo) roda `Mission.StartMission()`.

"Próximo capítulo" recarrega a cena e abre o briefing do capítulo seguinte (`GameManager.PendingChapter`).

## 6. Sistemas

### Missões
- `MissionBase`: passo, objetivo, resumo, final e utilidades (`Ring`, `Say`, `Hint`, `Command`, `Complete`).
- Ganchos: telefone atendido/encerrado, cabo, `EvaluateConnection` → `ConnectionVerdict` (Solution, Progress, NoAnswer, Contaminated, Neutral), isolador, evidência e `HandleEvent(string)` para eventos de mundo (`zone:*`, `tape:*`, `junction:*`, `breaker:*`, `circuit:*`, `door.open:*`, `heard:*`, `ending:*`, `subscriber.*`, `radio`).

### Mundo
- `ArtLibrary` carrega as superfícies PBR e os modelos CC0 de `Resources/Art`, com substituto procedural; `ModularMesh` gera caixas com UV em escala de mundo.
- `WorldBuilder` constrói a geometria modular e registra tudo em `WorldContext` (dicionários por id).
- `WorldCommands` aplica comandos de texto determinísticos (`door.unlock:x`, `pickup.show:label:anchor`, `junction.contaminate:i`, `lights.off:sector`, `subscriber.manifest:anchor:mode`…). O host aplica e replica.
- `SectorSystem` faz o culling de luzes por setor (atual + vizinhos), troca a ambiência e marca setores descobertos para o mapa. Atualiza a cada 0,4 s.

### Jogador e equipamentos
- `PlayerInteractor`: raycast de 2,6 m; pede a interação ao `GameManager` (solo executa; em rede envia ao host). Mostra a leitura do medidor no prompt.
- `PlayerEquipment`: lanterna, `Tuner` (camada e frequência de retorno), `Recorder` (uma gravação e comparação por `CallDefinition.group`), receptor (HUD), mapa e cabo (soltar ou desconectar com G). Só o dono lê teclado.

### Telefone
- `CallSessionManager`: camadas por `LayerDistributor`, duração ajustada por dificuldade e número de jogadores, sequência DTMF completa, legendas e registro das camadas ouvidas (a união da equipe é feita no host).
- `Telephone`: Idle, Ringing e Answered; lembrete a cada 25 s se ninguém atende; é fonte de sinal do receptor.
- `Switchboard`: um cabo por vez; a missão julga a conexão; extensões comprometidas (Linha Ocupada) podem ser restauradas.

### Anomalia e Assinante
- `TensionDirector`: só no host. Combina tensão, contaminação, separação da equipe e fase da missão; cooldown global × dificuldade e silêncio após ameaça. Os efeitos rodam localmente em cada peer.
- `ContaminationSystem`: valor do host replicado; estado local da Linha Ocupada; recuperação por frequência ou pela equipe.
- `SubscriberEntity`: estados de fundo dirigidos pela contaminação. Manifestações: silhueta, reflexo, aproximação e forma completa. Some ao ser observada (1,2 s) ou ao se chegar perto (< 5 m); o contato só gera Linha Ocupada.

### Áudio
- `ProceduralAudio`: campainha, DTMF, estática, zumbido, passos, batidas, sussurro, voz placeholder e ambiências por setor em loop contínuo.
- `AudioDirector`: categorias de volume, filtro telefônico no fone (300–3400 Hz) e limite de sustos.
- `OcclusionFilter`: linecast a cada 0,2 s; passa-baixa em 1,2 kHz sem linha de visão.
- Reverb zones por setor.

### UI
- Canvas único gerado em runtime. Legendas: as das camadas sem gravação são sempre exibidas; as de eventos respeitam a opção de legendas. F3 mostra FPS, tempo de frame e memória.

### Save
- `SaveData` v2: capítulo, capítulos concluídos, evidências, marcas de campanha (flags dos finais), circuitos isolados, finais vistos, desbloqueios, dificuldade e equipamento.
- JSON local em `Application.persistentDataPath/extensao-zero-save.json`, com escrita atômica.

## 7. ScriptableObjects

| Tipo | Uso | Estado |
|---|---|---|
| `CallDefinition` | chamadas, camadas, grupo de comparação, `AudioClip` por camada | usado (catálogo em memória) |
| `TensionEventDefinition` | eventos do diretor | usado (catálogo em memória) |
| Missões | classes C# por capítulo | decisão ADR-014, sem `MissionDefinition` |
| Dificuldade, equipamentos, finais | classes puras em `Campaign/` | sem asset |

Os tipos aceitam assets reais sem mudança de código. Por exemplo, um `CallDefinition` com clips gravados pode ser registrado com `CallCatalog.Register`.

## 8. Rede

Veja [NETWORK_ARCHITECTURE.md](NETWORK_ARCHITECTURE.md).

## 9. Convenções

- Namespaces `ExtensaoZero.*`; um tipo público por arquivo, com exceções pequenas e documentadas (`SwitchboardRules`, `EvidenceCatalog`, `CableItem`).
- Sem `FindObjectOfType` em `Update`; referências explícitas via `GameManager`/`WorldContext`.
- Comentários em português para regras de design; código em inglês; logs com prefixo `[EZ]`.

## 10. Testes

- EditMode, lógica pura: `LayerDistributor`, `SessionCodec`, `SwitchboardRules`, `TensionSelector`, `NotebookModel`, `CampaignRules` (catálogo, finais, equipamento, dificuldade, seed), `NetworkProtocol` (convite, chave, beacon, aprovação) e `LineBusyRules`.
- PlayMode: fluxo completo do prólogo em modo individual.
- Fora da Unity: `tools/compile-check/run.sh` compila tudo e executa os testes de lógica pura (32 testes).

## 11. Limitações

- Uma cena; sem streaming de setores (o culling é só de luzes).
- Sem NavMesh: a aproximação da Assinante é em linha reta, lenta e sem perseguição.
- Sem IL2CPP; build Mono Linux.
- Sem migração de host, reconexão automática, UPnP ou IPv6 no host.
- Sem vozes gravadas e sem arte final modelada.
