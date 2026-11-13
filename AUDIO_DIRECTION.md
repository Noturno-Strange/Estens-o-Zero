# EXTENSÃO ZERO — AUDIO DIRECTION

## 1. Princípio

O áudio é a principal ferramenta de terror. A informação chega pelo que se ouve — e pelo que não se ouve. O jogo não depende de sustos altos; depende de camadas, distância, oclusão, repetição e silêncio.

## 2. Identidade sonora

- **Sala da central:** zumbido elétrico de 60 Hz, relés distantes, madeira estalando, telefones com campainha eletromecânica.
- **Corredor:** reverberação longa, passos que voltam, tubulação, vento em frestas.
- **Arquivo:** ar parado, papel, gavetas, insetos, fluorescência falhando.
- **Recepção:** relógio, rádio de apoio, vidro, chuva distante.
- **Subsolo/túneis (capítulos futuros):** água, cabos, transformadores, eco.
- **A Assinante:** ruído de linha, respiração filtrada, fios se movendo, uma frequência que nunca resolve.

## 3. Camadas de ligação

| Camada | Conteúdo | Tratamento |
|---|---|---|
| Voz | fala principal | passa-banda 300–3400 Hz, compressão, leve saturação, ruído de linha |
| Ambiente | ruídos do outro lado | estéreo estreito, reverb curto, volume baixo |
| Sinais | DTMF, tons, pulsos | limpo, sem reverb, mono, destaque |
| Interferência | fragmentos e sobreposição | ruído, dropout, pitch instável, volume controlado |

Cada camada tem legenda própria. A legenda identifica a camada e o falante; nunca revela a solução.

## 4. Mixagem

Categorias independentes:

- Master
- Chamadas (voz, camadas, ruído de linha)
- Efeitos (portas, passos, objetos)
- Ambiente (loops, zumbidos)
- Sustos (eventos do Diretor de Tensão)

Padrões: master 0 dB, chamadas −3 dB, efeitos −6 dB, ambiente −12 dB, sustos −8 dB com limitador. Nenhum pico acima de −1 dBFS. Sustos nunca aumentam mais de 6 dB em relação à cena.

## 5. Espacialização

- Todas as fontes do mundo usam `spatialBlend = 1`, rolloff logarítmico, distância mínima 1 m, máxima 25 m.
- Oclusão simplificada: raycast do ouvinte até a fonte a cada 0,2 s; sem linha de visão, low-pass em 1,2 kHz; com linha de visão, 22 kHz.
- Reverb zones por sala (central, corredor, arquivo). Sem reverb global pesado.
- Vozes de ligação: quando atendidas, vão para o fone do jogador (2D parcial) com filtro telefônico; o telefone do mundo continua com uma versão abafada.
- Não transmitir áudio pela rede. O host envia o ID da chamada, o instante de início e a distribuição das camadas; cada cliente reproduz o arquivo local.

## 6. Implementação na fatia vertical

O ambiente de desenvolvimento não permite gravar vozes humanas. Para não simular resultados inexistentes:

- A arquitetura de chamadas já aceita `AudioClip` gravados.
- Na fatia, as camadas usam **áudio procedural gerado em tempo de execução**: campainha, DTMF, ruído, batidas, passos, zumbido e uma textura de voz sintetizada **não inteligível**.
- As informações essenciais são entregues por **legendas sincronizadas** e pelo caderno. Isso também atende acessibilidade.
- O campo `isPlaceholder` da definição de chamada é verdadeiro na fatia; a UI de depuração pode exibi-lo.
- Nenhuma voz de jogador é gravada, imitada ou transmitida.

### Gravações a produzir

Cada chamada tem um id em `CallCatalog` e quatro campos `AudioClip` (`voiceClip`, `ambienceClip`, `signalsClip`, `interferenceClip`). Uma camada com clip toca a gravação; sem clip, toca o placeholder procedural.

| ID | Capítulo | Voz | Estado |
|---|---|---|---|
| `first_call`, `second_call` | Prólogo | desconhecida (Ana, sem identificação) | a gravar |
| `ch1_intro` | 1 | voz de arquivo (RENALTA) | a gravar |
| `ch1_phantom_205/211/216`, `ch1_source_*`, `ch1_final` | 1 | desconhecida | a gravar (`ch1_source_*` e `ch1_final` variam pela seed: gravar as variantes de interferência e a fala final em partes) |
| `ch2_tape_1979`, `ch2_tape_1987` | 2 | Celina Prado | a gravar (mesma fala, números diferentes) |
| `ch2_reply` | 2 | voz jovem (Ana) | a gravar |
| `ch3_tunnel` (gerada pela seed), `ch3_after` | 3 | desconhecida | a gravar (números e locais em partes) |
| `ch4_prediction`, `ch4_breakers`, `ch4_personal` | 4 | desconhecida / Ana | a gravar |
| `ch5_original` | 5 | Celina e Ana Prado | a gravar |
| `ch5_operator` | 5 | Ana Prado | a gravar |
| rádio de apoio | todos | Helena Rocha, Nara Prado | a gravar (hoje: legenda) |

Formato de entrega: WAV 48 kHz/24 bits, mono para vozes, sem processamento destrutivo; os filtros são aplicados em tempo de execução. Cada linha deve ter versão limpa e versão "telefone" para referência.

### Implementado nesta versão

- Ambiência por setor (central, recepção, arquivo, corredor, túneis, energia, central original) em loop contínuo.
- Reverb zone por setor: Room, Hallway, Livingroom, SewerPipe, Hangar e Cave.
- Fone do jogador com passa-banda 300–3400 Hz.
- Fontes do mundo com rolloff logarítmico de 1 a 25 m; oclusão em 1,2 kHz.
- Sustos limitados a no máximo 2× o volume da cena (≈ +6 dB); "reduzir intensidade" corta pela metade.
- DTMF completo na camada de sinais.

## 7. Eventos do Diretor de Tensão

| Evento | Som | Categoria |
|---|---|---|
| Presença atrás da parede | três batidas abafadas | A |
| Telefone solitário | um toque curto | A |
| Passos distantes | passos com reverb, param com o jogador | A/B |
| Telefone sem linha | campainha + ruído de linha | B |
| Batidas divergentes | 3 batidas para um jogador, 4 para outro | B |
| Passos espelhados | passos acompanham os seus e dão um a mais | B |
| LED sem cabo | relé + LED no painel | A |
| Chamada particular | um toque só para um jogador | C |
| Sequência de luzes | relés e cliques | C |
| Sussurro | interferência com formantes | B |
| Manifestação | frequência grave + fios | C |

Cada evento tem versão reduzida quando “Reduzir intensidade de sustos” está ativo.

## 8. Silêncio

Após um evento importante, o diretor entra em silêncio deliberado por 8–20 s: apenas ambiente, sem música, sem sustos. O silêncio é parte da composição.

## 9. Acessibilidade

- Legendas para todas as camadas, com identificação de falante e camada.
- Indicador visual de direção de som relevante (opcional).
- Controle de volume por categoria.
- Redução de flashes/intensidade.
- Nenhum som essencial apenas em frequência inaudível.
- Alternativa visual para DTMF: representação gráfica dos tons no caderno.

## 10. Ferramentas e licenças

- Áudio procedural: código próprio, sem dependências.
- Gravações futuras: estúdio próprio ou licença compatível; registrar autoria e direitos.
- Nenhuma biblioteca comercial de áudio sem autorização.
- Nenhum serviço externo de geração de voz em tempo de jogo.
