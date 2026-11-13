# EXTENSÃO ZERO — TEST PLAN

## 1. Filosofia

Nenhuma funcionalidade é considerada pronta apenas porque o código foi escrito. Este plano separa o que foi realmente executado do que depende de validação local.

## 2. Testes automatizados (EditMode)

Executar:

```bash
./scripts/run-tests.sh
```

| Suíte | Cobre |
|---|---|
| `SessionCodecTests` | codificar/decodificar convite, versão inválida, chave, IPv6, porta |
| `LayerDistributorTests` | distribuição para 1–4 jogadores, cobertura das 4 camadas, sem duplicação indevida |
| `NotebookSystemTests` | registrar, deduplicar, ordenar, persistir evidências |
| `TensionDirectorTests` | cooldowns, tensão mínima, variedade, respeito a “reduzir intensidade” |
| `SwitchboardLogicTests` | validação 214→0, rejeição 214→9, desconexão, contaminação |
| `MissionFlowTests` | avanço de etapas, condição de conclusão, reinício |

Critérios: todos verdes, sem exceções, sem warnings de compilação.

## 3. Testes de cena (PlayMode/manual)

### 3.1 Inicialização

- [ ] Abrir `Bootstrap.unity` e pressionar Play.
- [ ] Menu aparece com o cenário ao fundo.
- [ ] “Jogar sozinho” inicia a missão sem erro.
- [ ] Nenhuma exceção no Console.

### 3.2 Movimento e interação

- [ ] WASD move; mouse olha; Shift corre.
- [ ] Colisão com paredes/props funciona.
- [ ] Prompt de interação aparece a ≤ 2,6 m e some ao se afastar.
- [ ] E interage; G solta cabo; F liga/desliga lanterna; Q alterna camada; Tab abre caderno; Esc pausa.
- [ ] Lanterna consome bateria e recarrega na base.

### 3.3 Primeira ligação

- [ ] Telefone toca ao iniciar.
- [ ] Atender reproduz áudio e legendas.
- [ ] Q alterna as quatro camadas no modo solo.
- [ ] Cada camada tem legenda e conteúdo distinto.
- [ ] Caderno registra extensão 214, tronco 0, DTMF 2-1-4 e aviso da linha 9.
- [ ] Desligar encerra a chamada.

### 3.4 Puzzle da central

- [ ] Cabo pode ser pego no arquivo.
- [ ] Cabo aparece na mão/HUD.
- [ ] Conectar 214 ao tronco 0 avança a missão.
- [ ] Conectar 214 à linha 9 gera contaminação, evento de ameaça e Linha Ocupada.
- [ ] É possível desconectar e corrigir.
- [ ] Cabo visual acompanha os soquetes sem física instável.
- [ ] Alavanca de isolamento conclui a missão após a manifestação.

### 3.5 Diretor de Tensão

- [ ] Eventos de Presença ocorrem em intervalos ≥ 12 s.
- [ ] Eventos de Contradição exigem etapa/tensão compatíveis.
- [ ] Nenhum evento se repete duas vezes seguidas.
- [ ] “Reduzir flashes” diminui flicker.
- [ ] “Reduzir intensidade” reduz volume/duração de sustos.
- [ ] Silêncio após evento importante.

### 3.6 Assinante

- [ ] Silhueta aparece no corredor após a segunda chamada.
- [ ] Desaparece ao ser olhada por ~1,2 s.
- [ ] Não mata, não persegue, não bloqueia a missão.
- [ ] Estado muda de Observando → Interferindo → Manifestando → Recuando.

### 3.7 Interface e acessibilidade

- [ ] HUD discreta, sem sobreposição.
- [ ] Legendas legíveis em 1080p.
- [ ] Caderno abre/fecha e rola.
- [ ] Configurações salvam entre sessões.
- [ ] Sensibilidade, FOV e volumes aplicam imediatamente.
- [ ] Menu de pausa retoma e abandona.
- [ ] Conclusão mostra tempo, contaminação e evidências.

### 3.8 Salvamento

- [ ] Progresso/evidências persistem após reiniciar.
- [ ] Nenhum dado é enviado para a rede.
- [ ] Arquivo JSON válido em `~/.config/unity3d/.../extensao-zero-save.json`.

## 4. Testes de rede

Requer dois processos ou duas máquinas.

### 4.1 Host + 1 cliente na mesma máquina

```bash
# terminal 1
./ExtensaoZero.x86_64 -screen-width 1280 -screen-height 720

# terminal 2 (outra cópia do build ou editor)
./ExtensaoZero.x86_64 -screen-width 1280 -screen-height 720
```

- [ ] Host cria partida; vê IP local, porta e convite.
- [ ] Cliente cola convite e entra.
- [ ] Lobby mostra 2 jogadores.
- [ ] Host inicia; ambos veem o mundo.
- [ ] Movimento remoto sincroniza.
- [ ] Host atende a ligação; cada jogador recebe 2 camadas.
- [ ] Q alterna apenas as camadas atribuídas.
- [ ] Interação com cabo/central é validada pelo host.
- [ ] Contaminação e etapa sincronizam.
- [ ] Cliente sai; host continua; mensagem clara.
- [ ] Host sai; sessão encerra organizadamente.

### 4.2 LAN

- [ ] Descoberta automática lista a sessão em até 10 s.
- [ ] Conexão manual por IP local funciona.
- [ ] Firewall liberado.
- [ ] Três e quatro jogadores.

### 4.3 Segurança/erros

- [ ] Chave errada é rejeitada com mensagem.
- [ ] Sessão cheia é rejeitada.
- [ ] Entrada após início é rejeitada.
- [ ] Versão incompatível é rejeitada.
- [ ] Timeout exibe diagnóstico.
- [ ] Interações fora de distância são ignoradas pelo host.
- [ ] Spam de interação é limitado.

### 4.4 Internet

- [ ] IPv4 público + port forwarding.
- [ ] IPv6 global.
- [ ] Documentar CGNAT/NAT restritivo; não prometer conexão.

## 5. Testes de desempenho

- [ ] Build Linux inicia em < 15 s em SSD.
- [ ] 60 FPS em GPU de referência (Médio, 1080p).
- [ ] 30 FPS mínimos em 4 GB (Baixo, 1080p).
- [ ] VRAM ≤ 3,2 GB (Médio); ≤ 1,2 GB na fatia.
- [ ] Sem vazamento em 10 min.
- [ ] Draw calls < 350, triângulos < 250 mil.
- [ ] Log sem erros críticos.

Registrar hardware, driver, versão do Vulkan e resultados reais.

## 6. Regressão

- [ ] Compilar sem erros/warnings.
- [ ] Rodar todos os testes EditMode.
- [ ] Abrir a cena e jogar o fluxo completo.
- [ ] Verificar README/documentação atualizados.
- [ ] Commit organizado.

## 7. O que NÃO foi executado nesta entrega

- Build Linux nativo (a ser executado pelo script; ver relatório).
- Teste com duas máquinas reais.
- Medição de FPS/VRAM em GPU real.
- Teste de áudio em dispositivo físico.
- Sessão completa de 4 jogadores.

Esses itens permanecem pendentes e não são declarados como aprovados.
