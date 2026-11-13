# EXTENSÃO ZERO

Jogo de terror psicológico cooperativo em primeira pessoa para 1 a 4 jogadores.
Este repositório contém a **primeira fatia vertical jogável** do projeto e a documentação de pré-produção.

> **Estado:** fatia vertical implementada em Unity 6000.3.28f1 (URP), com modo individual completo para a primeira missão, base de multiplayer direto para 2 jogadores, Diretor de Tensão, primeira manifestação da Assinante e puzzle da central telefônica.
> O jogo completo (5 capítulos) **não** está implementado nesta entrega. O escopo e as limitações estão descritos em [DEVELOPMENT_ROADMAP.md](DEVELOPMENT_ROADMAP.md) e no relatório final.

---

## O que já funciona

- Personagem em primeira pessoa: caminhar, olhar, correr, lanterna com bateria, interação por raycast.
- Sala da central telefônica, corredor, arquivo de vozes e recepção construídos em tempo de execução com geometria modular estilizada.
- Telefone funcional com quatro camadas de áudio: Voz, Ambiente, Sinais e Interferência.
- Sintonizador de Frequência (modo individual) para alternar camadas.
- Primeira ligação fragmentada completa, com pistas que se complementam.
- Puzzle físico da central: encontrar o cabo, conectar a extensão 214 ao tronco 0, isolar a linha.
- Caderno de investigação que registra evidências sem entregar a solução.
- Diretor de Tensão com eventos de Presença, Contradição e Ameaça, cooldowns e variaedade.
- Assinante com máquina de estados e primeira manifestação física no corredor.
- Estado "Linha Ocupada" curto e recuperável ao conectar a linha errada.
- Multiplayer peer-hosted: host, cliente, conexão direta por IP/porta, descoberta LAN por broadcast UDP, chave de sessão e distribuição das camadas da ligação.
- Interface completa da fatia: menu, lobby, HUD, caderno, pausa, configurações, conclusão de missão.
- Salvamento local de progresso, evidências e configurações (JSON, sem nuvem).

## Requisitos

- Linux x86_64.
- Unity **6000.3.28f1** com Linux Build Support (Mono).
- GPU com 4 GB de VRAM ou mais para o alvo mínimo; a fatia usa orçamento bem menor.
- Nenhum serviço externo, conta ou servidor é necessário para jogar.

## Como abrir

```bash
# na raiz do repositório
~/Unity/Hub/Editor/6000.3.28f1/Editor/Unity -projectPath "$(pwd)"
```

O primeiro import executa o setup automático (`Assets/_Project/Editor/ProjectSetup.cs`), que cria a cena `Bootstrap`, os assets URP, o perfil de pós-processamento e o prefab de rede.

Também é possível abrir pelo Unity Hub e selecionar a versão 6000.3.28f1.

## Como jogar no editor

1. Abra `Assets/_Project/Scenes/Bootstrap.unity`.
2. Pressione **Play**.
3. No menu, escolha **Jogar sozinho**.

## Controles

| Ação | Tecla |
|---|---|
| Mover | W A S D |
| Olhar | Mouse |
| Correr | Shift |
| Interagir / atender / conectar | E |
| Alternar camada da ligação | Q |
| Lanterna | F |
| Caderno | Tab |
| Pausa | Esc |
| Soltar cabo | G |

## Como gerar o build Linux

```bash
./scripts/build-linux.sh
```

O build sai em `Build/Linux/ExtensaoZero.x86_64`. Instruções detalhadas em [LINUX_BUILD.md](LINUX_BUILD.md).

## Testes

```bash
./scripts/run-tests.sh
```

Os testes EditMode cobrem distribuição de camadas, codificação de convite, sessão, caderno e regras do Diretor de Tensão. Veja [TEST_PLAN.md](TEST_PLAN.md).

## Estrutura

```
Assets/_Project/
  Editor/            setup do projeto, build, validação
  Scripts/
    Core/            bootstrap, estado de jogo, configurações, salvamento
    Player/          controlador FPS, equipamentos, lanterna
    Interaction/     interface de interação, portas, objetos examináveis
    Telephone/       ligações, camadas, telefone, central, cabos, sintonizador
    Investigation/   caderno, evidências, pistas
    Anomaly/         Diretor de Tensão, contaminação, eventos
    AI/              Assinante (máquina de estados)
    Multiplayer/     rede direta, descoberta LAN, sessão, sync
    Missions/        primeira missão e objetivos
    Audio/           áudio procedural, mixagem, oclusão, filtros
    UI/              HUD, menu, lobby, caderno, configurações
  Scenes/            Bootstrap (cena única da fatia)
  Settings/          URP, volume, qualidade
  Tests/EditMode/    testes automatizados
```

## Documentação

- [GAME_DESIGN_DOCUMENT.md](GAME_DESIGN_DOCUMENT.md)
- [STORY_BIBLE.md](STORY_BIBLE.md)
- [ART_DIRECTION.md](ART_DIRECTION.md)
- [AUDIO_DIRECTION.md](AUDIO_DIRECTION.md)
- [TECHNICAL_ARCHITECTURE.md](TECHNICAL_ARCHITECTURE.md)
- [NETWORK_ARCHITECTURE.md](NETWORK_ARCHITECTURE.md)
- [PERFORMANCE_TARGETS.md](PERFORMANCE_TARGETS.md)
- [LINUX_BUILD.md](LINUX_BUILD.md)
- [TEST_PLAN.md](TEST_PLAN.md)
- [DEVELOPMENT_ROADMAP.md](DEVELOPMENT_ROADMAP.md)
- [DECISIONS.md](DECISIONS.md)

## Limitações conhecidas desta entrega

- As vozes são **placeholders procedurais** (sons de voz sintetizados, não inteligíveis). As informações chegam por legendas e pelo caderno. As gravações definitivas precisam ser produzidas e colocadas nos campos `AudioClip` das definições de ligação. Ver [AUDIO_DIRECTION.md](AUDIO_DIRECTION.md).
- A arte usa geometria modular e texturas geradas em código, coerente com a direção visual, mas ainda não é a arte final.
- O multiplayer foi implementado e compila, mas os testes de rede real dependem de duas máquinas/processos e não foram executados neste ambiente. Ver relatório e [TEST_PLAN.md](TEST_PLAN.md).
- Não há migração de host, reconexão automática, UPnP automático nem chat de voz.
- O consumo de VRAM/FPS não foi medido em GPU real neste ambiente de nuvem. O orçamento é uma meta de projeto; nenhuma compatibilidade de hardware é declarada sem medição.
