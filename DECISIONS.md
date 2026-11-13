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

**Decisão:** conexão direta; descoberta LAN por broadcast; convite colável `EZ1:` com IP/porta/chave e chave aleatória (implementados). CGNAT/NAT restritivo é documentado como limitação.

**Consequências:** alguns jogadores não conseguirão conectar sem port forwarding. Isso é honesto e explícito.

## ADR-006 — Input System com fallback de UI legacy

**Contexto:** o prompt prioriza Input System; uGUI precisa de módulo de input.

**Decisão:** usar a API `UnityEngine.InputSystem` para gameplay; manter `activeInputHandler = Both` e `StandaloneInputModule` para a UI. Isso evita dependência de um asset de ações e mantém a UI simples.

**Consequências:** ambos os sistemas habilitados; migração futura para `InputSystemUIInputModule` e ações configuráveis é possível.

## ADR-007 — Movimento autoritativo no host

**Contexto:** o host deve manter o estado autoritativo.

**Decisão original:** o cliente envia intenção de movimento; o host aplica no CharacterController e replica via NetworkTransform.

**Estado:** substituída parcialmente pela ADR-016 (o código nunca implementou movimento por input). As interações continuam como ServerRpcs validadas por distância e cooldown.

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

## ADR-014 — Campanha como missões em código na mesma instalação

**Contexto:** cinco capítulos, sem editor de níveis nem YAML frágil, com variação por seed e multiplayer.

**Decisão:** uma classe `MissionBase` por capítulo (prólogo + 1–5), com passos explícitos, ganchos tipados e eventos de mundo em texto. Todos os setores são construídos no boot; portas liberam setores por capítulo. `SetupWorld(seed)` roda igual em todos os peers; a lógica só roda no host.

**Consequências:** fácil de ler, testar e replicar; o conteúdo fica no código (como `CallCatalog`). Um editor visual de missões fica para depois, se for necessário.

## ADR-015 — Execução replicada de interações

**Contexto:** portas, cabo, central, fitas e caixas precisam ficar idênticas em todos os peers sem sincronizar cada objeto.

**Decisão:** o dono pede a interação (ServerRpc com ID determinístico); o host valida e manda todos os peers executarem a mesma interação com a cópia do jogador que agiu. Mensagens pessoais só aparecem para quem agiu. A lógica de missão continua só no host.

**Consequências:** pouco tráfego e estado consistente, desde que o mundo seja construído na mesma ordem. A latência de uma ida e volta ao host é perceptível na interação, aceitável para um jogo cooperativo de investigação.

## ADR-016 — Movimento: posição enviada pelo dono, validada pelo host

**Contexto:** movimento autoritativo por input exige predição e reconciliação para não parecer travado, o que é complexo e não foi implementado.

**Decisão:** o dono move localmente e envia a posição a 15 Hz; o host rejeita posições fora dos limites da instalação e replica; os outros interpolam. As interações dependem da posição validada no host.

**Consequências:** é simples e responsivo, mas um cliente modificado poderia se teletransportar dentro dos limites. Isso é aceitável em sessões cooperativas entre conhecidos e está documentado.

## ADR-017 — Kit base + dois opcionais

**Contexto:** o GDD lista seis equipamentos, permite escolher dois e mantém a lanterna sempre equipada. Sem o sintonizador não há como ouvir as camadas.

**Decisão:** lanterna e sintonizador formam o kit base. Receptor, gravador, medidor e mapa são opcionais (até dois). Nenhum puzzle depende de um opcional.

**Consequências:** a escolha muda o ritmo e a forma de investigar, não a possibilidade de concluir.

## ADR-018 — Verificação sem Unity no ambiente de nuvem

**Contexto:** o ambiente de nuvem não tem a Unity 6 nem licença; a expansão da campanha precisava de alguma verificação de compilação.

**Decisão:** `tools/compile-check` compila o código contra assemblies de referência da Unity 2021.3 (NuGet) e stubs com as assinaturas públicas do NGO 2.14, do Input System 1.20 e do URP 17.3, e executa os testes de lógica pura com NUnit.

**Consequências:** pega erros de C# e de API comum. **Não** valida APIs exclusivas do Unity 6, o code-gen de RPC do Netcode, a serialização nem o comportamento em cena. Isso continua exigindo a Unity.

## ADR-019 — A "voz que responde" sem microfone

**Contexto:** a Story Bible fala de uma voz que responde a perguntas feitas em voz alta; a ADR-011 proíbe capturar áudio de microfone.

**Decisão:** a voz "responde" às ações da equipe: descreve a ordem dos lugares visitados (Cap. 1) e cita frases do caderno (Cap. 4). Nenhum áudio de jogador é capturado.

**Consequências:** o efeito de "ela ouve vocês" se mantém sem risco de privacidade.
