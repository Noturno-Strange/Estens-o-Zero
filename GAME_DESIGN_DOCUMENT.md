# EXTENSÃO ZERO — GAME DESIGN DOCUMENT

## 1. Pilares

1. **Uma ligação pode ser mais assustadora do que um monstro.** O terror nasce da informação incompleta, da escuta e da expectativa.
2. **Informação é o recurso cooperativo.** Cada jogador ouve uma camada diferente da mesma chamada. Ninguém tem a resposta sozinho.
3. **A central é um instrumento, não um menu.** Conectar, isolar e rastrear linhas deve ser físico, tátil e legível.
4. **O Diretor de Tensão controla o ritmo, não a aleatoriedade.** Eventos têm contexto, cooldown e consequência.
5. **Simples de implementar, rico de interpretar.** Máquinas de estado, ScriptableObjects, áudio preparado e geometria modular.

## 2. Visão geral

- Gênero: terror psicológico/sobrenatural, investigação, exploração em primeira pessoa, sobrevivência cooperativa.
- Jogadores: 1 a 4.
- Plataforma prioritária: Linux x86_64. Engine: Unity 6 LTS (6000.3.28f1), URP.
- Câmera: primeira pessoa.
- Multiplayer: peer-hosted, conexão direta, sem servidor central.
- Sessão alvo: 35–60 minutos por capítulo; fatia vertical: 10–15 minutos.

## 3. Ciclo principal

1. Preparar equipamentos no lobby.
2. Entrar no setor.
3. Investigar e ouvir.
4. Atender ligações e separar camadas.
5. Registrar pistas no caderno.
6. Compartilhar informações (voz externa ou marcações no mundo).
7. Resolver conexões na central.
8. Identificar a origem do fenômeno.
9. Isolar ou estabilizar a anomalia.
10. Sobreviver à consequência.
11. Concluir a missão e atualizar o caderno/progresso.

## 4. Mecânica central: Ligações Fragmentadas

Cada chamada tem quatro camadas:

| Camada | Conteúdo | Função |
|---|---|---|
| 1 — Voz | Fala principal, nomes, números, pedidos | Informação narrativa e objetiva |
| 2 — Ambiente | Passos, máquinas, portas, água, vento, batidas | Contexto espacial e pistas sensoriais |
| 3 — Sinais | Tons DTMF, sequências, pulsos, padrões | Códigos, extensões, frequências |
| 4 — Interferência | Fragmentos, conversas sobrepostas, ruído | Pistas contraditórias e avisos |

### Distribuição por número de jogadores

| Jogadores | Distribuição |
|---|---|
| 4 | uma camada por jogador |
| 3 | jogador 1 recebe 2 camadas; jogadores 2 e 3 recebem 1 cada |
| 2 | duas camadas por jogador |
| 1 | todas as camadas, alternáveis com o Sintonizador |

A distribuição é determinística a partir da quantidade de jogadores e do índice do jogador na sessão. O host envia apenas o identificador da chamada e o instante de início; o áudio é local.

### Regras de design

- Nenhum puzzle exige quatro pessoas.
- Toda solução é justificável por pelo menos duas fontes de informação.
- O modo individual recebe tempo extra e pistas redundantes.
- A dificuldade ajusta quantidade de pistas, interferência e janelas de reação, nunca a vida de inimigos.

## 5. Central telefônica

A central é o equipamento interativo principal. Representação física:

- Painel principal com 20 extensões (200–219) em duas fileiras.
- Tronco 0 (linha morta) e linha 9 (linha contaminada).
- Soquetes de conexão, cabos removíveis, indicadores luminosos, medidores analógicos.
- Alavanca de isolamento geral.
- Telefone de operação sobre a mesa.
- Armário de cabos no arquivo.

Ações:

1. Atender e desligar.
2. Identificar extensões.
3. Conectar/desconectar cabos.
4. Encaminhar sinais.
5. Rastrear chamadas pelo medidor.
6. Comparar camadas.
7. Isolar circuito contaminado.
8. Restaurar conexão.

O cabo é uma animação controlada por pontos (LineRenderer com curva de Bézier), não física complexa.

## 6. Equipamentos

| Equipamento | Função | Limite |
|---|---|---|
| Lanterna | Iluminação funcional | Bateria de ~4 min; recarrega em bases |
| Sintonizador | Separa camadas da chamada | Individual; 2 camadas por vez em grupo de 2 |
| Receptor portátil | Detecta sinais próximos | Alcance curto; indica direção aproximada |
| Gravador analógico | Registra e reproduz o último sinal | 1 gravação por vez |
| Medidor de circuito | Mostra linha energizada/interrompida/contaminada | Requer contato com a central |
| Mapa de extensões | Setores e conexões conhecidas | Só mostra o que foi descoberto |

Não há classes fixas. Lanterna e sintonizador formam o kit base (o sintonizador é indispensável para as camadas). O jogador escolhe até dois equipamentos opcionais (receptor, gravador, medidor, mapa) no briefing ou no lobby. Nenhum puzzle exige um opcional: cada um encurta o caminho, mas há sempre outra fonte de informação (ADR-017).

| Tecla | Uso |
|---|---|
| Q | sintonizador: alterna entre as camadas atribuídas; na Linha Ocupada, gira a frequência |
| R | gravador: grava a camada atual; sem chamada, reproduz; outra versão do mesmo grupo gera comparação |
| M | mapa de extensões (só setores visitados e telefones descobertos) |
| — | medidor: aparece no prompt ao olhar para soquetes, caixas e disjuntores |
| — | receptor: indica no HUD direção e intensidade do sinal ativo mais próximo (≤ 16 m) |

## 7. Investigação e caderno

O caderno registra automaticamente **fatos descobertos**, não soluções:

- números de extensão e troncos;
- nomes e horários;
- sequências de sinais (DTMF);
- locais mencionados;
- anomalias observadas;
- transcrições parciais de camadas.

O jogador pode anotar uma hipótese em uma página de “Observações” (texto livre local, não sincronizado). O caderno não valida a hipótese.

## 8. Diretor de Tensão

### Entradas

Tempo desde o último evento, contaminação, setor, número de jogadores próximos, separação da equipe, ações recentes, etapa da missão, chamadas atendidas, extensões conectadas e histórico de eventos.

### Saída

Seleção de um evento elegível, com categoria, peso, cooldown, intensidade e efeitos. O jogador nunca vê um medidor de tensão.

### Categorias

- **A — Presença:** luz acende, telefone toca uma vez, ruído atrás da parede, passos distantes, LED de telefone desconectado.
- **B — Contradição:** telefone toca em extensão desconectada, três batidas para um jogador e quatro para outro, gravação com frase alterada, passos que param quando o jogador para.
- **C — Ameaça:** telefones tocam juntos, sequência de luzes apagando, extensão contaminada, chamada particular, manifestação da Assinante.

### Regras

- Cooldown global de 12 s; cooldown por evento de 45–180 s.
- Nunca repetir o mesmo evento duas vezes seguidas.
- Eventos de ameaça exigem tensão ≥ 0,55 e etapa de missão compatível.
- Todo evento de ameaça tem pista e reação possível.
- Silêncio depois de um evento importante é intencional.

### Técnica de três etapas

1. **Antecipação:** pista discreta (luz, ruído, toque curto).
2. **Contradição:** algo impossível (extensão inexistente, voz onde não deveria).
3. **Consequência:** ação necessária (investigar, desconectar, isolar) ou apenas dúvida.

## 9. A Assinante

Entidade ligada à rede, não um monstro de perseguição permanente.

Estados: Adormecida → Observando → Investigando → Interferindo → Contaminando → Aproximando-se → Manifestando-se → Recuando.

- A presença é sentida por telefones, luzes, gravações e silêncios.
- A manifestação física é rara, distante e nunca mata arbitrariamente.
- A aparência completa é revelada apenas no Capítulo 5.
- A primeira manifestação da fatia é uma silhueta no fim do corredor que desaparece quando observada.

## 10. Linha Ocupada

Quando a contaminação passa de um limiar, a Assinante isola um jogador:

- o telefone/receptor dele fica com estática;
- camadas compartilhadas ficam bloqueadas para ele;
- os demais veem a extensão comprometida;
- o jogador isolado recebe uma pista (frequência de retorno) e continua podendo explorar;
- a equipe restaura a conexão na central ou o jogador sintoniza a frequência correta.

Duração alvo: 15–25 s. Nunca é punição permanente.

## 11. Dificuldade

| Nível | Tempo | Pistas | Interferência | Entidade |
|---|---|---|---|---|
| Investigação | alto | redundantes | baixa | rara |
| Operação | padrão | equilibradas | média | presente |
| Ruptura | curto | escassas | alta | frequente e controlada |

A dificuldade também adapta a quantidade de jogadores: em solo, mais tempo e pistas; em grupo, mais complementaridade.

## 12. Missão da fatia vertical — “A Primeira Ligação”

**Objetivo:** descobrir por que a extensão 214 recebe chamadas de uma linha que não existe.

1. **Atenda o telefone.** O telefone da central toca ao iniciar.
2. **Ouça as camadas.** Voz: “a extensão 214 está fora do painel; reconecte ao tronco zero antes que o sinal caia”. Ambiente: três batidas e uma porta. Sinais: DTMF 2-1-4. Interferência: “não use a linha nove”.
3. **Encontre o cabo.** O cabo de extensão está no arquivo de vozes, em uma gaveta etiquetada.
4. **Conecte 214 ao tronco 0.** A central aceita conexões físicas. A ligação 214→9 é errada e contamina a linha.
5. **Atenda a segunda chamada.** A mesma voz agradece e avisa que “ela já sabe que vocês estão aqui”.
6. **Investigue o corredor.** As luzes apagam em sequência; a silhueta aparece no fim.
7. **Isole a linha.** Puxe a alavanca de isolamento na central para concluir a missão.

**Pistas redundantes:** etiqueta do cabo, placa “TRONCO 0 — LINHA MORTA”, anotação “não conecte o 9”, sequência DTMF na camada de sinais, batidas na camada ambiente.

**Falha recuperável:** conectar 214→9 gera contaminação, evento de ameaça e Linha Ocupada; o jogador pode desconectar e corrigir.

## 13. Campanha

| Capítulo | Tema | Novidade |
|---|---|---|
| Prólogo — A Primeira Ligação | Tutorial e primeira chamada | Controles, central, camadas |
| 1 — A Central | Extensões inexistentes | Rastreamento, contaminação |
| 2 — Arquivo de Vozes | Gravações antigas | Gravador, comparação de versões |
| 3 — A Linha Morta | Túneis e circuitos | Medidor, isolamento, Linha Ocupada |
| 4 — O Outro Lado | Chamadas sobre os investigadores | Informação pessoal, contradições |
| 5 — Extensão Zero | Origem da Assinante | Manifestação, três finais |

Reutiliza uma instalação central dividida em setores; novas áreas são liberadas gradualmente. Não há cinco mundos independentes.

## 14. Rejogabilidade

Variações controladas por seed do host:

- ordem e conteúdo de ligações secundárias;
- localização de cabos e gravações;
- circuitos contaminados;
- eventos do Diretor de Tensão;
- pistas redundantes;
- comportamento da Assinante.

Regras fixas: a solução principal sempre tem pistas suficientes; nenhuma combinação impossível; a seed é compartilhada para que todos vejam o mesmo mundo.

## 15. Progressão

Desbloqueios narrativos e cosméticos: novas gravações, documentos, equipamentos, variações de capa do caderno e marcadores. Sem microtransações, sem grind, sem conta. Salvamento local; o host controla o progresso da campanha e os convidados mantêm registros pessoais.

## 16. Acessibilidade

- Legendas por camada e por evento.
- Controle de volume separado: master, chamadas, efeitos, ambiente, sustos.
- Redução de flashes e de intensidade visual.
- Sensibilidade, FOV, inversão de eixo.
- Alternativa visual para informações auditivas essenciais (espectro de sinais, transcrição).
- Sem mudanças bruscas de câmera ou perseguições sem aviso.

## 17. Campanha implementada

Todos os capítulos usam a mesma instalação. Portas liberam setores por capítulo; `SetupWorld` prepara o mundo de forma determinística pela seed em todos os peers, e a lógica da missão roda no host.

### Prólogo — A Primeira Ligação
Igual à §12. Passos 0–7. Falha recuperável: 214→9.

### Capítulo 1 — A Central
1. A central toca: três extensões fora de serviço registram tráfego.
2. Para cada extensão fantasma (ordem pela seed): a central toca; a camada de sinais traz os três dígitos; a equipe conecta a extensão ao tronco 0 para rastrear.
3. O medidor do painel aponta a origem; o telefone físico do setor toca (recepção 205, arquivo 211, corredor 216). Atender registra a origem.
4. Uma das três (seed) menciona a linha nove na interferência; o LED dela fica vermelho.
5. Isolar: conectar a extensão contaminada e puxar a alavanca. Isolar o circuito errado aumenta a contaminação.
6. A última chamada descreve, na ordem certa, os lugares por onde a equipe passou.

### Capítulo 2 — Arquivo de Vozes
1. O arquivo morto é liberado. Duas fitas: 1979 (207, tronco livre) e 1987 (212, tronco aberto).
2. Ouvir as duas registra a diferença; o gravador mostra a comparação lado a lado.
3. A escala de plantão de 1987 confirma a 212 (segunda fonte). Conectar 207→0 contamina: é a versão alterada.
4. 212→0 faz a 207 tocar no arquivo morto: uma voz jovem chama "mãe".
5. A Assinante aparece no arquivo morto; isolar na central conclui. A ordem de corte de 1998 mostra que o tronco zero nunca foi cortado.

### Capítulo 3 — A Linha Morta
1. Descer pela rampa até os túneis de cabos.
2. A extensão 218 lista as caixas: sinais com os três números, ambiente com dois locais aproximados e interferência com uma caixa falsa.
3. Isolar as três caixas contaminadas. Pistas: LED pulsando, zumbido, medidor e receptor. Isolar uma caixa limpa derruba a linha de quem errou.
4. A contaminação cresce com o tempo enquanto houver caixas ativas. No segundo isolamento, a Assinante aparece no vidro do túnel oeste.

### Capítulo 4 — O Outro Lado
1. A 218 prevê: "alguém vai abrir a porta da sala de energia; a luz vermelha vai apagar duas vezes". Acontece.
2. A 209 pede para não desligar o disjuntor B; a interferência pede o contrário; o manual na parede confirma a voz (A e C desligados, B ligado).
3. Desligar B isola quem desligou (Linha Ocupada) e apaga a emergência; religar corrige.
4. Uma chamada particular cita frases do caderno da equipe (texto, nunca vozes de jogadores).
5. A Assinante vem pelo túnel e se aproxima quando ninguém olha. Encará-la a faz recuar; o contato só isola a linha.

### Capítulo 5 — Extensão Zero
1. A central original é destrancada. Lá estão os registros do incêndio, o crachá de Ana, a gravação original em quatro camadas e cinco circuitos antigos (três contaminados, pela seed).
2. Religar o tronco com circuitos contaminados ativos contamina; com todos isolados, registra a restauração limpa.
3. O telefone da operadora toca; atender revela a forma completa e arma as três conclusões.
4. Cada console mostra seu requisito (Story Bible §7). Todos podem ser cumpridos dentro do capítulo, exceto o abandono, que é registrado quando uma Linha Ocupada termina sem que ninguém restaure a linha.

### Progressão
Concluir um capítulo libera o próximo. Finais vistos liberam capas do caderno. Rejogar um capítulo gera outra seed.
