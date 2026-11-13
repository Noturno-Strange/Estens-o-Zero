# EXTENSÃO ZERO — DEVELOPMENT ROADMAP

Legenda: ✅ implementado e validado · 🟦 implementado, compilado fora da Unity, **não validado na Unity** · 🟡 parcial · ⬜ não feito

## Fase 0 — Reconhecimento ✅

- Projeto Unity 6000.3.28f1, pacotes e import batch (sessão anterior).
- Sessão de nuvem atual: sem Unity e sem GPU; verificação por `tools/compile-check` (ADR-018).

## Fase 1 — Pré-produção ✅

GDD, story bible, direção de arte e áudio, arquiteturas técnica e de rede, metas de desempenho, plano de testes, build Linux e ADRs.

## Fase 2 — Fatia vertical 🟦

- [x] Personagem, interação, central, cabo, telefone, ligação com 4 camadas, caderno, Diretor de Tensão, Assinante, Linha Ocupada, UI, salvamento.
- [x] Correções desta sessão: tecla Q (era T), G solta o cabo, alavanca que travava a missão, conteúdo das chamadas alinhado à story bible, DTMF completo, olhar de 1,2 s, opções "reduzir flashes/sustos" aplicadas, silêncio após ameaça, saída da Linha Ocupada por frequência, ambiência sem emenda, oclusão em 1,2 kHz, recepção, portas com dobradiça e orçamento de luzes.
- [ ] Rodar o PlayMode e jogar o prólogo na Unity.

## Fase 3 — Multiplayer 🟦

- [x] Host/cliente, IP/porta, convite `EZ1:`, chave aleatória, descoberta LAN (beacon + busca).
- [x] Aprovação: versão, chave, vagas, missão iniciada, com motivo exibido.
- [x] Interações por ServerRpc, validadas e executadas em todos os peers.
- [x] Telefones, chamadas, comandos de mundo, eventos de tensão, Linha Ocupada por jogador, evidências e conclusão replicados.
- [x] Ordenação por ClientId, redistribuição de camadas, corpo remoto visível, diagnóstico de timeout e de host encerrado.
- [ ] Teste real com 2, 3 e 4 jogadores.
- [ ] Reconexão automática, UPnP opcional, IPv6 no host.
- [ ] Validação de latência e perda (tc/netem).

## Fase 4 — Sistema de terror 🟦

- [x] 13 eventos (presença, contradição e ameaça), silêncio deliberado, dificuldade, separação da equipe.
- [x] Assinante: estados de fundo, silhueta, reflexo, aproximação e forma completa.
- [ ] Ajuste fino de frequência e intensidade com playtests.

## Fase 5 — Conteúdo completo 🟦

- [x] Prólogo e capítulos 1–5 com puzzles próprios e pistas redundantes.
- [x] Equipamentos: receptor, gravador, medidor e mapa, com escolha de até dois.
- [x] Progressão, três finais com requisitos e capas do caderno.
- [x] Variações por seed: ordem das chamadas, posição do cabo, circuitos contaminados e eventos.
- [ ] Gravações definitivas (VO).

## Fase 6 — Identidade visual e sonora 🟡

- [x] Setores com paletas distintas (túneis frios, energia vermelha), reverb por setor, ACES.
- [ ] Arte final modelada em DCC, kit modular final, animações.
- [ ] Iluminação pré-calculada por setor.
- [ ] VO gravada.
- [ ] Medição de VRAM.

## Fase 7 — Estabilização ⬜

- [ ] Campanha completa jogada na Unity, sozinho e em grupo.
- [ ] LAN e internet, falhas de conexão.
- [ ] Otimização de CPU, GPU e memória; build de distribuição; QA completo.

## Próxima prioridade (na sua máquina)

1. Abrir o projeto na Unity 6000.3.28f1 e corrigir o que o compilador e o code-gen do Netcode acusarem.
2. Rodar `./scripts/run-tests.sh all`.
3. Jogar o prólogo e cada capítulo pelo menu **Capítulos**, seguindo o [TEST_PLAN.md](TEST_PLAN.md).
4. Gerar o build Linux e testar host + cliente na mesma máquina e na LAN.
5. Medir desempenho (F3, Profiler) em GPU real.
6. Playtests para ajustar ritmo e sustos; depois, VO e arte final.

## Princípio

A qualidade da primeira ligação define o jogo. Antes de investir em arte e VO, valide na Unity que a mecânica central funciona e é assustadora.
