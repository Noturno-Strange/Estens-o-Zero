# EXTENSÃO ZERO — DEVELOPMENT ROADMAP

## Fase 0 — Reconhecimento ✅

- Repositório inicialmente vazio (apenas `projeto.md`).
- Unity 6000.3.28f1 instalado via Unity CLI; licença Personal ativa.
- Linux Build Support (Mono) presente.
- Sem GPU real para medição; sem segundo processo para teste de rede.
- Projeto Unity criado, pacotes resolvidos, import batch concluído.

## Fase 1 — Pré-produção ✅

- [x] GDD
- [x] Story bible
- [x] Direção de arte
- [x] Direção de áudio
- [x] Arquitetura técnica
- [x] Arquitetura de rede
- [x] Metas de desempenho
- [x] Plano de testes
- [x] Build Linux
- [x] Decisões (ADRs)
- [x] Primeira missão detalhada
- [x] Central telefônica projetada
- [x] Primeira ligação fragmentada projetada
- [x] Primeiros eventos do Diretor de Tensão
- [x] Primeira manifestação da Assinante

## Fase 2 — Fatia vertical ✅ (implementada; validação parcial)

- [x] Personagem em primeira pessoa, câmera, movimentação
- [x] Interações por raycast
- [x] Sala da central, corredor, arquivo, recepção
- [x] Telefone funcional e ligação com 4 camadas
- [x] Central telefônica com 20 extensões, tronco 0 e linha 9
- [x] Cabo físico e conexão
- [x] Primeiro puzzle
- [x] Caderno de investigação
- [x] Diretor de Tensão com eventos A/B/C
- [x] Primeiro comportamento da Assinante
- [x] Primeira manifestação
- [x] Linha Ocupada
- [x] UI completa da fatia
- [x] Salvamento local

**Pendente de validação:** build Linux nativo, sessão de jogo manual, medição de desempenho.

## Fase 3 — Multiplayer 🟡 (base implementada)

- [x] Host/cliente com NGO + Unity Transport
- [x] Conexão direta por IP/porta
- [x] Descoberta LAN
- [x] Chave de sessão e aprovação
- [x] Movimento autoritativo no host
- [x] Interações via ServerRpc
- [x] Distribuição de camadas
- [x] Estado compartilhado da missão
- [x] Tratamento básico de desconexão
- [ ] Teste real com 2, 3 e 4 jogadores
- [ ] Reconexão automática
- [ ] UPnP opcional com consentimento
- [ ] Validação de latência/perda

## Fase 4 — Sistema de terror 🟡

- [x] Diretor de Tensão com tensão/cooldowns/pesos
- [x] Eventos reutilizáveis A/B/C
- [x] Linha Ocupada
- [x] Manifestação
- [ ] Ajuste fino de frequência/intensidade com playtests
- [ ] Mais eventos de contradição
- [ ] Medidor de tensão invisível validado

## Fase 5 — Conteúdo completo ⬜

- [ ] Prólogo completo
- [ ] Capítulos 1–5
- [ ] Puzzles por capítulo
- [ ] Gravações definitivas
- [ ] Equipamentos completos
- [ ] Progressão e três finais
- [ ] Variações por seed

## Fase 6 — Identidade visual e sonora ⬜

- [ ] Arte final modelada em DCC
- [ ] Kit modular final
- [ ] Iluminação pré-calculada por setor
- [ ] VO gravada
- [ ] Animações
- [ ] UI final
- [ ] Medição de VRAM

## Fase 7 — Estabilização ⬜

- [ ] Campanha completa
- [ ] 4 jogadores
- [ ] LAN e internet
- [ ] Falhas de conexão
- [ ] Otimização CPU/GPU/memória
- [ ] Build Linux de distribuição
- [ ] QA completo

## Próxima prioridade

1. Compilar e executar a fatia no editor; corrigir erros de runtime.
2. Gerar build Linux e validar o fluxo completo.
3. Testar host + cliente na mesma máquina e na LAN.
4. Medir desempenho em GPU real.
5. Só então expandir conteúdo para o Capítulo 1.

## Princípio

Não construir o Capítulo 2 antes de a mecânica central estar validada. Não adicionar sistemas por quantidade. A qualidade da primeira ligação define o jogo.
