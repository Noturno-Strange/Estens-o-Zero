# EXTENSÃO ZERO

Jogo de terror psicológico cooperativo em primeira pessoa para 1 a 4 jogadores, em Unity 6000.3.28f1 (URP), com build nativo para Linux.

> **Estado:** campanha completa implementada em código: prólogo, capítulos 1 a 5 e três finais. Inclui modo individual, multiplayer direto para até 4 jogadores, Diretor de Tensão, a Assinante, equipamentos, dificuldade, variação por seed e progressão local.
>
> **Validação:** o código **ainda não foi aberto na Unity 6** depois desta expansão. Ele foi compilado contra assemblies de referência da Unity 2021.3 e stubs dos pacotes (veja [Verificação sem Unity](#verificação-sem-unity)). Os 32 testes de lógica pura foram executados com NUnit e passaram. Faltam o teste PlayMode, o build Linux, partidas reais em rede e a medição de desempenho, que precisam ser feitos na sua máquina. Veja [TEST_PLAN.md](TEST_PLAN.md).

---

## O que o jogo tem

- **Campanha** em uma única instalação dividida em setores (ADR-010):

  | Capítulo | Setores | Mecânica nova |
  |---|---|---|
  | Prólogo — A Primeira Ligação | recepção, central, arquivo de vozes, corredor | camadas, central, cabo, Linha Ocupada |
  | 1 — A Central | os mesmos | rastreio de extensões fantasmas pelo tronco 0, isolamento de circuito |
  | 2 — Arquivo de Vozes | + arquivo morto | fitas de 1979 e 1987, comparação com o gravador |
  | 3 — A Linha Morta | + rampa e túneis de cabos | seis caixas de passagem, medidor, contaminação física, reflexo |
  | 4 — O Outro Lado | + sala de energia | previsões que se cumprem, disjuntores, chamada particular, aproximação da Assinante |
  | 5 — Extensão Zero | + central original | registros do incêndio, gravação original em 4 camadas, forma completa da Assinante, **três finais** |

- **Ligações fragmentadas:** Voz, Ambiente, Sinais e Interferência, distribuídas entre 1 e 4 jogadores. Legendas por camada. Aceita gravações reais por camada (`AudioClip`).
- **Central telefônica:** 20 extensões, tronco 0 e linha 9. Cabo físico (solta com G), restauração de extensões comprometidas e alavanca de isolamento.
- **Equipamentos:** lanterna e sintonizador sempre presentes, mais até dois opcionais: receptor portátil, gravador analógico, medidor de circuito ou mapa de extensões.
- **Diretor de Tensão:** 13 eventos (presença, contradição e ameaça), cooldowns, silêncio de 8–20 s após ameaças e versões atenuadas para acessibilidade. Inclui eventos divergentes entre jogadores, como "três batidas para um, quatro para outro".
- **A Assinante:** estados ligados à contaminação e quatro tipos de manifestação: silhueta, reflexo, aproximação e forma completa. Ela nunca mata; no pior caso, isola a linha de um jogador.
- **Linha Ocupada:** dura de 15 a 25 s. Sai-se dela sintonizando a frequência de retorno (Q) ou quando a equipe restaura a extensão na central.
- **Dificuldade:** Investigação, Operação e Ruptura. A **seed** do host varia a ordem das chamadas, a posição do cabo e quais circuitos estão contaminados.
- **Multiplayer direto:** host e cliente com NGO + Unity Transport. Conexão por IP/porta ou convite colável `EZ1:`, chave aleatória, descoberta LAN e aprovação por versão, chave, vagas e estado da missão. Interações validadas pelo host.
- **Interface completa:** menu, briefing com capítulos, equipamento e dificuldade, lobby, HUD, caderno por categoria, mapa, pausa, configurações, tela final com relatório e F3 com estatísticas.
- **Cenário (projeto.md §15):** área externa de retransmissão com torre e baliza, recepção, alojamentos antigos, central, salas de operação, arquivo de vozes, corredor técnico, depósito, arquivo morto, subsolo e túneis, sala de energia e central original. Texturas PBR e 26 modelos CC0 da Poly Haven ([CREDITS.md](CREDITS.md)), luminárias com sombra e SSAO.
- **Salvamento local** em JSON: progresso, evidências, marcas de campanha, finais vistos e capas do caderno. Sem nuvem e sem conta.

## Requisitos

- Linux x86_64.
- Unity **6000.3.28f1** com Linux Build Support (Mono).
- GPU com 4 GB de VRAM é o alvo mínimo, ainda não medido (veja [PERFORMANCE_TARGETS.md](PERFORMANCE_TARGETS.md)).
- Nenhum serviço externo, conta ou servidor.

## Como abrir

```bash
~/Unity/Hub/Editor/6000.3.28f1/Editor/Unity -projectPath "$(pwd)"
```

Se a cena `Bootstrap`, os assets URP ou os prefabs de rede estiverem faltando, rode **Extensao Zero → Setup Project**.

## Como jogar no editor

1. Abra `Assets/_Project/Scenes/Bootstrap.unity` e pressione **Play**.
2. **Continuar campanha** ou **Capítulos** abre o briefing, onde você escolhe capítulo, equipamento e dificuldade.
3. Para jogar em grupo, use **Criar sessão** ou **Entrar em sessão**.

## Controles

| Ação | Tecla |
|---|---|
| Mover / olhar / correr | W A S D / mouse / Shift |
| Interagir, atender, conectar | E |
| Sintonizador: alternar camada ou girar a frequência na Linha Ocupada | Q |
| Lanterna | F |
| Soltar o cabo (ou desconectar, se estiver plugado) | G |
| Gravador: gravar a camada atual ou reproduzir | R |
| Mapa de extensões | M |
| Caderno | Tab |
| Pausa | Esc |
| Estatísticas de desempenho | F3 |

## Build Linux

```bash
./scripts/build-linux.sh
```

Para um build de desenvolvimento com profiler, use `--dev`. A saída vai para `Builds/Linux/`. Detalhes em [LINUX_BUILD.md](LINUX_BUILD.md).

## Testes

Na Unity:

```bash
./scripts/run-tests.sh            # EditMode
```
```bash
./scripts/run-tests.sh playmode   # fluxo do prólogo; precisa de GPU/servidor gráfico
```

### Verificação sem Unity

```bash
tools/compile-check/run.sh
```

Compila o runtime, o editor e os testes contra referências da Unity 2021.3 (NuGet) e stubs dos pacotes, depois executa os testes de lógica pura com NUnit. Precisa de `dotnet` 8, `curl` e `unzip`. Ela pega erros de C# e de API comum, mas **não** substitui a Unity: APIs exclusivas do Unity 6 e o code-gen do Netcode só são validados no editor.

## Estrutura

```
Assets/_Project/
  Editor/            setup do projeto, build
  Scripts/
    Core/            bootstrap, GameManager, configurações, salvamento
    Campaign/        capítulos, dificuldade, equipamentos, finais, seed (lógica pura)
    Missions/        MissionBase + prólogo e capítulos 1–5
    Player/          controlador FPS, equipamentos, sintonizador, gravador, lanterna
    Interaction/     portas, documentos, fitas, caixas, disjuntores, consoles, zonas
    Telephone/       chamadas, sessão de chamada, telefone, central, cabo
    Investigation/   caderno e catálogo de evidências
    Anomaly/         Diretor de Tensão, eventos, contaminação, Linha Ocupada
    AI/              Assinante
    Multiplayer/     bootstrap de rede, jogador, estado, convite, beacon LAN, aprovação
    Environment/     construção do mundo, setores, comandos de mundo, iluminação
    Audio/           áudio procedural, diretor de áudio, oclusão
    UI/              interface completa
  Tests/EditMode/    testes de lógica
  Tests/PlayMode/    teste de fumaça do prólogo
scripts/             build Linux e testes na Unity
tools/compile-check/ verificação sem Unity (compilação + testes de lógica)
```

## Documentação

[GAME_DESIGN_DOCUMENT.md](GAME_DESIGN_DOCUMENT.md) · [STORY_BIBLE.md](STORY_BIBLE.md) · [ART_DIRECTION.md](ART_DIRECTION.md) · [AUDIO_DIRECTION.md](AUDIO_DIRECTION.md) · [TECHNICAL_ARCHITECTURE.md](TECHNICAL_ARCHITECTURE.md) · [NETWORK_ARCHITECTURE.md](NETWORK_ARCHITECTURE.md) · [PERFORMANCE_TARGETS.md](PERFORMANCE_TARGETS.md) · [LINUX_BUILD.md](LINUX_BUILD.md) · [TEST_PLAN.md](TEST_PLAN.md) · [DEVELOPMENT_ROADMAP.md](DEVELOPMENT_ROADMAP.md) · [DECISIONS.md](DECISIONS.md)

## Limitações conhecidas

- **Sem validação na Unity desta versão:** podem existir erros que só a Unity 6 mostra, como APIs novas ou o code-gen de RPC do Netcode. O primeiro passo é abrir o projeto, rodar os testes e jogar o prólogo.
- **Vozes placeholder:** a voz é sintetizada e não inteligível. A informação chega por legendas e pelo caderno. As gravações reais entram nos campos `AudioClip` de cada camada (veja [AUDIO_DIRECTION.md](AUDIO_DIRECTION.md)).
- **Arte:** texturas e modelos CC0 adaptados à paleta, mais o kit modular em código. Ainda não há modelos próprios (personagens, Assinante, central) nem iluminação pré-calculada. No primeiro import, a Unity gera os `.meta` dos assets de arte; faça commit deles.
- **Multiplayer não testado entre processos reais.** O cliente envia a própria posição e o host valida os limites; não é movimento autoritativo por input (ADR-016). Não há migração de host, reconexão automática, UPnP, IPv6 no host nem chat de voz.
- **Assinante:** os efeitos são locais. Em grupo, cada jogador pode vê-la em um lugar ligeiramente diferente, o que é intencional, mas ainda não foi validado em playtest.
- **Desempenho não medido.** Nenhuma compatibilidade de hardware é declarada.
