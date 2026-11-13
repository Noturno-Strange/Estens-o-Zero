# EXTENSÃO ZERO — DECISÕES DE ARQUITETURA (ADRs)

## ADR-001 — Cena mínima + construção procedural do mundo

**Contexto:** o repositório começou vazio e não havia cena/arte importada. Criar YAML de cena e prefabs manualmente é frágil e difícil de revisar.

**Decisão:** a cena `Bootstrap` é mínima; o mundo da fatia é construído em runtime por `WorldBuilder` com geometria modular, materiais e texturas procedurais. O setup do editor cria a cena, os assets URP e o prefab de rede.

**Consequências:** setup reproduzível, sem assets binários grandes, fácil de testar em batch. A arte final substituirá os builders por prefabs autorais sem mudar os sistemas. O custo de geração ocorre uma vez no início.

## ADR-002 — Áudio procedural na fatia, arquitetura pronta para gravações

**Contexto:** não é possível gravar vozes humanas neste ambiente, e inventar gravações seria violar as regras do projeto.

**Decisão:** implementar o sistema completo de camadas com `AudioClip`; usar clips procedurais (DTMF, campainha, ruído, batidas, voz não inteligível) e legendas na fatia. Marcar `isPlaceholder = true`.

**Consequências:** a mecânica é jogável e testável; a substituição por VO real é só preencher os campos. Nenhuma voz de jogador é imitada.

## ADR-003 — URP em vez de HDRP

**Contexto:** meta de 4 GB de VRAM e 60 FPS.

**Decisão:** URP 17.3, sem ray tracing, sem GI dinâmica pesada, luzes locais limitadas, pós-processamento contido.

**Consequências:** menos recursos gráficos de ponta, mas orçamento compatível com o alvo e identidade estilizada.

## ADR-004 — Netcode for GameObjects + Unity Transport

**Contexto:** exigência de multiplayer direto sem serviços.

**Decisão:** NGO 2.14 + Unity Transport 2.6, arquitetura host-cliente, UDP direto, aprovação por chave de sessão.

**Consequências:** integração nativa com Unity, prefabs e RPCs. Sem relay/lobby. Testes reais de rede ainda pendentes.

## ADR-005 — Sem Relay/STUN/matchmaking

**Contexto:** o projeto proíbe infraestrutura central e não quer esconder limitações de NAT.

**Decisão:** conexão direta; descoberta LAN por broadcast; convite com IP/porta/chave. CGNAT/NAT restritivo é documentado como limitação.

**Consequências:** alguns jogadores não conseguirão conectar sem port forwarding. Isso é honesto e explícito.

## ADR-006 — Input System com fallback de UI legacy

**Contexto:** o prompt prioriza Input System; uGUI precisa de módulo de input.

**Decisão:** usar a API `UnityEngine.InputSystem` para gameplay; manter `activeInputHandler = Both` e `StandaloneInputModule` para a UI. Isso evita dependência de um asset de ações e mantém a UI simples.

**Consequências:** ambos os sistemas habilitados; migração futura para `InputSystemUIInputModule` e ações configuráveis é possível.

## ADR-007 — Movimento autoritativo no host

**Contexto:** o host deve manter o estado autoritativo.

**Decisão:** o cliente envia intenção de movimento; o host aplica no CharacterController e replica via NetworkTransform. Interações são ServerRpcs validadas por distância e cooldown.

**Consequências:** mais latência perceptível para o cliente; comportamento consistente e sem trapaça trivial de posição. Pode evoluir para predição local.

## ADR-008 — Sem migração de host na primeira versão

**Contexto:** prioridade é estabilidade e simplicidade.

**Decisão:** se o host sair, a sessão termina organizadamente.

**Consequências:** partidas longas dependem do host; documentado no lobby.

## ADR-009 — Salvamento local em JSON

**Contexto:** sem contas, sem nuvem, sem banco remoto.

**Decisão:** `JsonUtility` em `Application.persistentDataPath`, com escrita atômica (arquivo temporário + replace).

**Consequências:** simples e auditável. Sem sincronização entre máquinas; convidados mantêm registros locais.

## ADR-010 — Uma cena/setor na fatia

**Contexto:** evitar cinco mundos enormes e carregamentos complexos.

**Decisão:** uma instalação central dividida em setores; a fatia usa uma cena com quatro áreas.

**Consequências:** carregamento rápido e iteração simples. Streaming/occlusion por setor entra quando houver mais conteúdo.

## ADR-011 — Sem chat de voz próprio

**Contexto:** reduzir escopo, privacidade e complexidade de rede.

**Decisão:** comunicação por serviço externo do jogador (Discord etc.) e marcações no mundo. Nenhum áudio de microfone é capturado ou transmitido.

**Consequências:** menos infraestrutura e menos risco de privacidade; a cooperação depende de comunicação externa.

## ADR-012 — Conteúdo de demonstração em memória

**Contexto:** criar assets `.asset` manualmente é frágil; o setup pode gerar assets, mas a fatia precisa funcionar sem etapa manual.

**Decisão:** `CallCatalog` e `TensionCatalog` criam ScriptableObjects em memória no bootstrap. Os tipos e campos aceitam assets reais.

**Consequências:** zero dependência de YAML/GUID; fácil de testar. A produção deve migrar para assets versionados no editor.

## ADR-013 — Segurança de rede documentada, não inventada

**Contexto:** o prompt proíbe inventar criptografia.

**Decisão:** usar validação de chave de sessão e validação de estado; não afirmar criptografia. As opções de segurança do Unity Transport serão avaliadas e habilitadas em etapa própria, com documentação das limitações.

**Consequências:** a chave protege contra entrada casual, não contra interceptação. O lobby avisa que o IP do host é visível.
