# EXTENSÃO ZERO — TECHNICAL ARCHITECTURE

## 1. Stack

| Componente | Versão | Observação |
|---|---|---|
| Unity | 6000.3.28f1 (Unity 6.3 LTS) | Linux x86_64 |
| URP | 17.3.0 | pipeline prioritário |
| C# | .NET Standard 2.1 | scripts |
| Netcode for GameObjects | 2.14.0 | host-cliente |
| Unity Transport | 2.6.0 | UDP direto |
| Input System | 1.20.1 | teclado/mouse |
| uGUI / TextMeshPro | 2.0.0 / 5.0.0 | interface |
| Test Framework | 1.6.0 | EditMode |
| Linux Build Support | Mono | build nativo |

Nenhum serviço externo obrigatório. Nenhuma conta. Nenhum relay/lobby/matchmaker.

## 2. Princípios

- Host autoritativo para estado compartilhado.
- ScriptableObjects para conteúdo configurável.
- Sistemas pequenos, com responsabilidade única.
- Eventos modulares no Diretor de Tensão.
- Sem abstrações especulativas; sem ECS; sem física complexa de cabos.
- O jogo deve rodar no editor e em build Linux sem etapas manuais ocultas.

## 3. Estrutura de pastas

```
Assets/_Project/
  Art/                materiais e texturas geradas/importadas
  Audio/              clips gravados (vazio na fatia; placeholders procedurais)
  Characters/         modelos de personagem
  Environment/        kit modular
  Materials/          materiais compartilhados
  Models/             modelos
  Prefabs/            prefabs de props/rede
  Resources/          NetworkPlayer.prefab (carregado em runtime)
  Scenes/             Bootstrap.unity
  Scripts/
    Core/             GameBootstrap, GameManager, GameSettings, SaveSystem
    Player/           FirstPersonController, PlayerEquipment, Flashlight
    Interaction/      IInteractable, InteractableBase, portas, examinar
    Telephone/        CallDefinition, Telephone, Switchboard, Cable, Tuner
    Investigation/    NotebookSystem, Evidence
    Anomaly/          TensionDirector, TensionEventDefinition, Contamination
    AI/               SubscriberEntity
    Missions/         FirstCallMission
    Audio/            ProceduralAudio, AudioDirector, OcclusionFilter
    Multiplayer/      NetworkBootstrap, NetworkPlayer, NetworkGameState, LanDiscovery, SessionCodec
    UI/               UIManager, UITheme, NotebookUI
  Settings/           URP assets, volume profile
  Tests/EditMode/     testes
  Editor/             ProjectSetup, BuildScript, validation
```

## 4. Bootstrap

A cena `Bootstrap` é vazia. `GameBootstrap` usa `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]` e cria:

1. `GameManager` (DontDestroyOnLoad).
2. `GameSettings` + `SaveSystem`.
3. `AudioDirector`.
4. `UIManager`.
5. `WorldBuilder` (geometria, luzes, props, player, central).
6. `FirstCallMission`, `TensionDirector`, `SubscriberEntity`.
7. Menu principal.

Vantagens: cena mínima, sem YAML frágil, setup reproduzível, fácil de testar em batch. A geometria é modular e determinística; a mesma seed produz o mesmo mundo em todos os peers.

## 5. Fluxo de estado

```
MainMenu → Lobby → Preparing → Playing → Paused → Playing → MissionComplete
                                   ↘ Disconnected / Error
```

`GameManager` mantém o estado e as referências. Sistemas não procuram objetos por nome em tempo de execução, exceto o bootstrap.

## 6. Sistemas

### Player
- `FirstPersonController`: CharacterController, olhar, movimento, gravidade, corrida, head bob sutil.
- `PlayerInteractor`: raycast de 2,6 m, prompt, tecla E.
- `PlayerEquipment`: lanterna, sintonizador, cabo, gravador; sem inventário complexo.
- `Flashlight`: Spot Light, bateria, recarga, flicker.

### Telephone
- `CallDefinition : ScriptableObject`: ID, camadas, legendas, duração, placeholder.
- `Telephone`: estados Idle/Ringing/Answered/Disconnected, campainha, atender/desligar.
- `Switchboard`: 20 extensões + tronco 0 + linha 9, conexões, validação, contaminação.
- `CableItem`: cabo carregável; `CableVisual` desenha Bézier com LineRenderer.
- `Tuner`: alterna camadas; no multiplayer, alterna apenas as camadas atribuídas.

### Investigation
- `NotebookSystem`: evidências descobertas, deduplicação, persistência.
- `NotebookUI`: leitura por categoria; não valida hipóteses.

### Anomaly / AI
- `TensionDirector`: tensão, cooldowns, seleção ponderada, eventos.
- `TensionEventDefinition : ScriptableObject`: categoria, tensão mínima/máxima, cooldown, peso, intensidade, tipo.
- `TensionEventRunner`: executa o tipo escolhido (luzes, telefone, passos, sussurro).
- `ContaminationSystem`: 0–100, aumenta com erro, reduz com isolamento.
- `SubscriberEntity`: máquina de estados, manifestação, olhar do jogador.

### Audio
- `ProceduralAudio`: clips gerados (DTMF, campainha, ruído, passos, zumbido, voz placeholder).
- `AudioDirector`: pools de AudioSource, categorias de volume, reverb zones.
- `OcclusionFilter`: raycast periódico + low-pass.

### UI
- Construída em runtime com uGUI e fonte built-in, sem assets de layout.
- `UIManager` centraliza telas; `UITheme` define cores e helpers.
- HUD discreta; caderno em overlay; legendas por camada.

### Save
- `SaveSystem`: JSON local em `Application.persistentDataPath/extensao-zero-save.json`.
- Salva progresso, evidências, configurações e cosméticos.
- Sem nuvem, sem conta, sem histórico de IP.

## 7. ScriptableObjects

| Tipo | Uso |
|---|---|
| `CallDefinition` | chamadas e camadas |
| `TensionEventDefinition` | eventos do diretor |
| `MissionDefinition` | capítulos/missões (estrutura preparada) |
| `EquipmentDefinition` | equipamentos (estrutura preparada) |
| `DifficultyDefinition` | três níveis |
| `EntityProfile` | parâmetros da Assinante |

Na fatia, catálogos de demonstração são criados em memória (`CallCatalog`, `TensionCatalog`) para evitar assets binários/YAML frágeis. A estrutura aceita assets reais sem mudança de código.

## 8. Rede

Detalhes em [NETWORK_ARCHITECTURE.md](NETWORK_ARCHITECTURE.md). Resumo:

- `NetworkBootstrap` cria `NetworkManager` + `UnityTransport` em runtime.
- Prefab `NetworkPlayer` é gerado pelo setup do editor e carregado de `Resources`.
- Host valida chave de sessão na aprovação de conexão.
- Movimento autoritativo no host: cliente envia input, host aplica e replica.
- Interações do cliente viram ServerRpc; host valida distância/estado.
- Estado da missão, chamadas e contaminação replicados por NetworkVariables/ClientRpc.
- Descoberta LAN por UDP broadcast; conexão manual por IP/porta.

## 9. Convenções

- Namespaces `ExtensaoZero.*`.
- Um tipo público por arquivo.
- Sem `FindObjectOfType` em Update.
- Sem alocações por frame em caminhos críticos.
- `[SerializeField]` privado; propriedades públicas somente quando necessário.
- Comentários em português para regras de design; código em inglês.
- Logs com prefixo `[EZ]`.

## 10. Testes

- EditMode para lógica pura: `LayerDistributor`, `SessionCodec`, `NotebookSystem`, `TensionDirector` (cooldowns/seleção), `Switchboard` (validação de conexão).
- PlayMode/manual para cena, áudio, rede e performance.
- Nenhuma funcionalidade é declarada pronta sem compilação e teste executado.

## 11. Limitações da fatia

- Sem streaming de setores; uma cena.
- Sem NavMesh/perseguição.
- Sem IL2CPP; build Mono Linux.
- Sem migração de host/reconexão automática.
- Sem UPnP automático.
- Sem vozes gravadas.
- Sem arte final modelada em DCC; geometria/textura procedurais.
