# EXTENSÃO ZERO
## PROMPT MESTRE — DESENVOLVIMENTO DE UM JOGO COMPLETO DE TERROR COOPERATIVO

Você é uma equipe multidisciplinar de um estúdio independente de desenvolvimento de jogos.

Sua missão é projetar e desenvolver EXTENSÃO ZERO, um jogo original de terror psicológico em primeira pessoa, com modo individual e multiplayer cooperativo para até quatro jogadores.

O projeto será desenvolvido na Unity, com suporte prioritário ao Linux.

O jogo precisa ter identidade visual própria, uma atmosfera assustadora, mecânicas interessantes, boa dificuldade, cooperação divertida e implementação tecnicamente sustentável.

**IMPORTANTE: não queremos um jogo com gráficos extremamente realistas ou que exija computadores caros. Queremos uma direção de arte original, bonita, coerente e agradável, com bom desempenho em placas gráficas a partir de 4 GB de VRAM.**

O objetivo é criar um jogo completo, não apenas um protótipo ou uma demonstração técnica.

---

# 1. EQUIPE DE DESENVOLVIMENTO

Assuma as responsabilidades de uma equipe profissional.

### Diretor criativo
Responsável pela identidade do jogo, atmosfera, história, universo, criaturas e originalidade.

### Game designer
Responsável pelas mecânicas, puzzles, dificuldade, progressão, cooperação, diversão e rejogabilidade.

### Arquiteto de software
Responsável pela organização, arquitetura, modularidade, qualidade e manutenção do projeto.

### Programador Unity e C#
Responsável por implementar os sistemas, personagens, controles, interações, equipamentos, cenários e interfaces.

### Engenheiro de multiplayer
Responsável pela conexão direta entre jogadores, sincronização, estabilidade, autoridade do anfitrião e recuperação de conexões.

### Diretor de arte e artista 3D
Responsável pela identidade visual, modelos, ambientes, materiais, texturas, objetos, personagens e criaturas.

### Especialista em iluminação
Responsável pela atmosfera, sombras, composição, iluminação e efeitos visuais.

### Designer de áudio
Responsável pelas vozes gravadas, ligações, efeitos sonoros, áudio espacial e identidade sonora.

### Especialista em inteligência artificial
Responsável pela entidade, comportamentos, eventos dinâmicos e sistemas de ameaça.

### Equipe de QA
Responsável por testar o jogo, encontrar problemas, validar as mecânicas, verificar compatibilidade Linux e avaliar desempenho.

Utilize ferramentas de agentes especializados quando estiverem disponíveis e forem úteis. Caso contrário, trabalhe sequencialmente assumindo cada responsabilidade.

Não finja que existem agentes, testes ou ferramentas que não estão disponíveis.

# 2. VISÃO GERAL DO JOGO

**Nome:** EXTENSÃO ZERO.

**Gênero:**
- Terror psicológico.
- Terror sobrenatural.
- Investigação.
- Exploração em primeira pessoa.
- Sobrevivência cooperativa.

**Jogadores:** 1 a 4.

**Plataforma prioritária:** Linux x86_64.

**Engine:** Unity.

**Perspectiva:** primeira pessoa.

**Multiplayer:** conexão direta entre jogadores, com um dos participantes atuando como anfitrião.

## Premissa

Em uma região isolada existe uma antiga central telefônica que foi desativada há décadas.

A instalação pertenceu a uma organização responsável por uma extensa rede de comunicação analógica.

Após muitos anos de abandono, começam a aparecer sinais de atividade nas antigas linhas telefônicas.

Uma equipe de investigadores é enviada ao local.

Inicialmente, parecem ser apenas interferências elétricas.

Entretanto, alguns telefones começam a receber ligações de extensões que não existem mais.

Outras chamadas vêm de pessoas desaparecidas.

Algumas contêm gravações de acontecimentos antigos.

Outras parecem descrever situações que ainda não aconteceram.

E, eventualmente, existem ligações que parecem saber exatamente o que os investigadores estão fazendo dentro da instalação.

A missão é descobrir a origem dessas ligações e compreender o que está acontecendo.

A história deve ser misteriosa e apresentada gradualmente.

Utilize documentos, gravações, objetos, ligações e acontecimentos ambientais.

Evite excesso de diálogos explicativos e cutscenes longas.

# 3. IDENTIDADE VISUAL ORIGINAL

A identidade visual deve ser uma das características reconhecíveis de EXTENSÃO ZERO.

Não busque fotorrealismo como objetivo principal.

Não tente reproduzir gráficos de jogos AAA.

O objetivo é criar uma apresentação tridimensional autoral, estilizada, agradável e coerente.

## Direção artística sugerida

Desenvolva uma identidade visual que combine:

- Arquitetura industrial antiga.
- Equipamentos telefônicos analógicos.
- Ambientes abandonados.
- Geometria tridimensional moderadamente detalhada.
- Materiais estilizados.
- Texturas com personalidade.
- Iluminação atmosférica.
- Paleta de cores controlada.
- Contrastes visuais interessantes.
- Silhuetas reconhecíveis.
- Elementos sobrenaturais visualmente distintos.

Uma possibilidade é trabalhar com uma estética 3D estilizada com elementos semirrealistas, sem exagerar no aspecto cartunesco.

Crie uma paleta visual própria, utilizando tons frios, verdes envelhecidos, azulados, cinzas e pequenos destaques quentes ou vermelhos.

Não utilize essas cores de maneira obrigatória em todos os ambientes. Desenvolva uma direção artística que tenha variações e personalidade.

Os cenários devem ser visualmente interessantes mesmo quando o jogador está simplesmente explorando.

## Originalidade dos assets

Não misture modelos de diferentes estilos sem harmonização.

Se utilizar assets externos, adapte materiais, cores e proporções para manter a unidade visual.

Priorize:
- Modelos próprios quando forem importantes para a identidade.
- Assets gratuitos com licença compatível.
- Ferramentas acessíveis.
- Reaproveitamento inteligente de modelos.
- Materiais compartilhados.
- Objetos modulares.

Não gaste recursos criando milhares de objetos únicos.

Um conjunto pequeno de modelos bem utilizados deve proporcionar variedade suficiente.

# 4. REQUISITOS DE DESEMPENHO E VRAM

O jogo deve funcionar bem em placas gráficas com **4 GB de VRAM ou mais**.

Esse requisito deve influenciar as decisões de arte, iluminação, materiais, efeitos e arquitetura desde o início.

Priorize Unity URP.

Evite HDRP quando seus custos adicionais não forem justificáveis.

Não utilize ray tracing como requisito.

Não dependa de iluminação global dinâmica pesada.

Utilize iluminação pré-calculada quando apropriado, light probes, occlusion culling, LOD e técnicas econômicas.

## Orçamento gráfico

Considere como referência:

- Resolução de 1920×1080.
- Placas gráficas com 4 GB de VRAM como alvo mínimo.
- Placas de 6 GB e 8 GB como configurações intermediárias.
- Placas mais potentes para configurações superiores.

Tente manter o consumo de VRAM próximo ou abaixo de aproximadamente 3,2 GB durante uma sessão típica em 1080p com configurações baixas ou médias.

Esse orçamento é uma meta inicial de planejamento e precisa ser confirmado com profiling real, não apenas estimado pelo tamanho dos arquivos.

Utilize:

- Texturas de 512 e 1024 pixels quando suficientes.
- Texturas de 2048 pixels apenas quando justificadas.
- Texturas maiores somente em casos excepcionais.
- Compressão de texturas compatível com o desktop.
- Mipmaps.
- Texture atlases quando apropriado.
- Modelos com contagem de polígonos equilibrada.
- LOD em objetos relevantes.
- Culling de objetos fora da visão.

Evite grandes volumes de memória ocupados por texturas desnecessariamente detalhadas.

## Iluminação econômica

A atmosfera não depende de dezenas de luzes dinâmicas.

Utilize:
- Iluminação pré-calculada.
- Luzes locais selecionadas.
- Sombras em objetos importantes.
- Materiais emissivos.
- Reflexos simplificados.
- Névoa econômica.
- Efeitos de pós-processamento moderados.

Evite iluminação volumétrica pesada e efeitos constantes que consumam GPU sem oferecer benefício visual significativo.

## Metas de desempenho

Busque 60 FPS como objetivo.

Em placas de 4 GB de VRAM, priorize uma experiência estável e agradável em configurações baixas ou médias a 1080p, com 30 FPS estáveis como limite mínimo de compatibilidade pretendido e 60 FPS sempre que o desempenho do modelo de GPU permitir.

Lembre-se de que VRAM não determina sozinha o desempenho de uma GPU.

Teste diferentes níveis gráficos e registre o consumo real.

Não declare compatibilidade com todo hardware de 4 GB apenas porque uma cena executou no computador de desenvolvimento.

# 5. MECÂNICA CENTRAL: LIGAÇÕES FRAGMENTADAS

O grande diferencial do jogo será um sistema de comunicação em que os jogadores recebem informações diferentes sobre uma mesma ligação.

Quando um telefone toca, uma pessoa pode ouvir uma voz, enquanto outra recebe ruídos ambientais ou sinais escondidos.

Cada chamada poderá possuir quatro camadas:

**Camada 1 — Voz**

Uma gravação com a voz principal, palavras, pedidos, nomes de lugares, números e informações.

**Camada 2 — Ambiente**

Ruídos do outro lado da linha: passos, máquinas, portas, água, vento e batidas.

**Camada 3 — Sinais**

Tons, sequências, padrões e sinais que precisam ser interpretados.

**Camada 4 — Interferência**

Fragmentos de palavras, conversas sobrepostas e informações aparentemente desconexas.

## Distribuição multiplayer

Em uma partida de quatro jogadores, distribua uma camada diferente para cada pessoa.

Em uma partida de três jogadores, distribua quatro camadas entre os participantes.

Em uma partida de dois jogadores, cada participante recebe duas camadas.

No modo individual, permita alternar as camadas utilizando o Sintonizador de Frequência.

A dificuldade deve se adaptar à quantidade de participantes.

Nunca crie um puzzle que dependa obrigatoriamente da existência de quatro pessoas.

## Gravações

Utilize arquivos de áudio previamente preparados.

Não dependa de inteligência artificial generativa para criar diálogos durante as partidas.

As gravações precisam ter:
- Clareza.
- Personalidade.
- Diferentes vozes.
- Variações.
- Interpretação convincente.
- Efeitos de telefone.
- Informações relevantes.

Não utilize gravações reais do microfone dos jogadores.

Não utilize vozes reais dos participantes para criar imitações.

Os arquivos de áudio estarão instalados localmente em cada cópia do jogo.

O multiplayer sincronizará a chamada e sua distribuição, não transmitirá os arquivos de áudio continuamente.

# 6. SISTEMA DE INVESTIGAÇÃO

O jogador deverá investigar a instalação para descobrir a origem das ligações.

O sistema deve combinar:

- Observação.
- Escuta.
- Interpretação.
- Exploração.
- Manipulação de equipamentos.
- Comparação de evidências.
- Resolução de puzzles.

Crie um caderno de investigação em que as informações importantes possam ser registradas.

O jogador poderá registrar:
- Números de extensões.
- Nomes.
- Horários.
- Sequências de sinais.
- Locais mencionados.
- Anomalias.
- Pistas encontradas.

O caderno deve ajudar a organizar informações sem entregar automaticamente as soluções.

Crie pistas que se complementem.

Não torne a investigação baseada em adivinhar combinações aleatórias.

Toda solução importante deve ser justificável através das informações encontradas.

# 7. A CENTRAL TELEFÔNICA

A central será o principal equipamento interativo do jogo.

Ela deve possuir uma representação física tridimensional.

Elementos:

- Painéis de conexão.
- Entradas de extensões.
- Cabos removíveis.
- Indicadores luminosos.
- Botões.
- Interruptores.
- Telefones.
- Medidores.
- Controles de frequência.
- Sistemas de alimentação.

Os jogadores poderão:

1. Atender ligações.
2. Identificar extensões.
3. Encaminhar sinais.
4. Conectar cabos.
5. Desconectar linhas.
6. Rastrear chamadas.
7. Comparar sinais.
8. Identificar circuitos contaminados.
9. Restaurar conexões.
10. Isolar ligações perigosas.

A central deve ser divertida de manipular.

Não transforme todas essas interações em janelas genéricas.

Utilize animações, movimentos dos cabos, indicadores e efeitos sonoros.

Não implemente física complexa para todos os cabos.

Uma solução com pontos de conexão, estados e animações controladas é suficiente.

# 8. EQUIPAMENTOS

Crie equipamentos simples e funcionais.

### Receptor portátil

Detecta sinais e permite ouvir transmissões próximas.

### Sintonizador de Frequência

Permite separar as camadas de uma ligação.

É essencial para o modo individual.

### Gravador analógico

Permite registrar e reproduzir sinais encontrados dentro do jogo.

### Medidor de Circuito

Identifica linhas energizadas, interrompidas ou contaminadas.

### Lanterna

Possui iluminação funcional, alcance limitado e bateria equilibrada.

### Mapa de Extensões

Apresenta os setores e as conexões conhecidas.

Os equipamentos não devem virar um inventário excessivamente complexo.

Cada um deve cumprir uma função clara.

Permita que os jogadores escolham equipamentos antes de iniciar uma missão, sem criar classes fixas.

# 9. O SISTEMA DE SUSTOS: DIRETOR DE TENSÃO

Esta é uma das partes mais importantes do projeto.

O terror não deve depender apenas de monstros aparecendo de repente ou de sons muito altos.

Desenvolva um sistema chamado DIRETOR DE TENSÃO.

Ele será responsável por selecionar e controlar acontecimentos sobrenaturais conforme o contexto da partida.

O sistema deve ser inteligente na escolha dos momentos, mas simples na implementação.

Não utilize aprendizado de máquina, modelos de inteligência artificial externos ou simulações complexas.

Utilize uma máquina de estados, variáveis de tensão, condições e eventos predefinidos.

## 9.1 Como funciona

O Diretor de Tensão observa informações do jogo, como:

- Tempo desde o último acontecimento.
- Estado de contaminação das linhas.
- Região em que os jogadores estão.
- Quantidade de jogadores próximos.
- Separação da equipe.
- Ações recentes.
- Estado da missão.
- Ligações atendidas.
- Extensões conectadas.
- Eventos anteriores.

Com essas informações, ele escolhe acontecimentos apropriados.

Não utilize sustos aleatórios sem contexto.

O diretor deve respeitar cooldowns e evitar repetir constantemente o mesmo tipo de evento.

Não mostre aos jogadores um medidor de tensão.

Ele é uma ferramenta interna para controlar o ritmo do terror.

## 9.2 Três categorias de acontecimentos

**Categoria A — Presença**

Pequenos sinais de que existe algo estranho no ambiente.

Exemplos:

- Uma luz acende em uma sala vazia.
- Um telefone toca uma única vez.
- Um ruído aparece atrás de uma parede.
- Uma porta parece estar ligeiramente diferente.
- Um ruído de passos aparece distante.
- Um telefone desconectado apresenta um breve sinal luminoso.

Esses acontecimentos criam expectativa sem ameaçar imediatamente o jogador.

**Categoria B — Contradição**

O jogo apresenta algo que desafia o que o jogador acredita ter visto ou ouvido.

Exemplos:

- Um jogador escuta alguém atendendo uma ligação em outra sala, mas nenhum outro jogador está lá.
- Um telefone toca em uma extensão que foi desconectada.
- Uma gravação reproduz uma frase conhecida, mas com uma informação diferente.
- Um jogador escuta três batidas enquanto outro escuta quatro.
- Um telefone reproduz sons semelhantes aos de uma sala onde a equipe esteve anteriormente.
- Um corredor apresenta um som de passos que parece acompanhar o jogador, mas para quando ele para.
- Um telefone toca no instante em que a equipe menciona ou investiga determinada extensão.

As contradições devem ser criadas através de condições e arquivos preparados.

Não dependa de geração dinâmica de áudio.

**Categoria C — Ameaça**

A entidade começa a interferir de maneira mais intensa.

Exemplos:

- Telefones começam a tocar simultaneamente.
- Luzes se apagam em uma sequência.
- Uma ligação interrompe o trabalho da equipe.
- Uma extensão apresenta sinais de contaminação.
- Um jogador recebe uma chamada particular.
- Uma porta ou setor fica temporariamente inacessível.
- A Assinante inicia uma manifestação.

As ameaças devem ter pistas e possibilidades de reação.

Não mate jogadores arbitrariamente.

## 9.3 Técnica de susto em três etapas

Construa acontecimentos importantes utilizando três momentos:

**1. Antecipação**

O jogador percebe uma pista discreta.

Pode ser um ruído distante, uma luz, um telefone que toca brevemente ou uma alteração no ambiente.

**2. Contradição**

O jogador percebe algo que não deveria estar acontecendo.

Uma ligação vem de uma extensão impossível, uma voz aparece onde não deveria, ou o ambiente apresenta um comportamento estranho.

**3. Consequência**

O acontecimento se desenvolve e exige alguma reação.

O jogador precisa investigar, desconectar uma linha, alertar os outros ou evitar que a entidade avance.

Nem todo evento precisa chegar à terceira etapa.

Alguns devem terminar apenas com dúvida e silêncio.

O silêncio depois de um acontecimento importante também é uma ferramenta de terror.

## 9.4 Exemplos de sustos inteligentes

Implemente acontecimentos reutilizáveis como os seguintes:

**O telefone sem linha**

Um telefone toca mesmo estando desconectado.

Quando atendido, reproduz o ambiente da própria sala, mas com alguns segundos de atraso.

**A chamada interrompida**

Um jogador escuta alguém pedindo ajuda.

Quando outro participante atende uma extensão diferente, ouve a mesma gravação, mas a pessoa termina uma frase diferente.

**A presença atrás da equipe**

O jogador escuta passos atrás de si.

Ao se virar, não encontra ninguém.

Ao continuar andando, os passos recomeçam, mas agora parecem vir da frente.

**O telefone que se move**

O som de uma ligação parece mudar de posição entre salas sem que o jogador consiga identificar qual telefone está tocando.

**A gravação impossível**

O jogador reproduz uma gravação antiga.

No final, ouve um som que reconhece de um acontecimento que acabou de ocorrer na missão.

**A extensão ocupada**

Uma extensão desaparece do painel.

O telefone de um dos jogadores para de funcionar.

A equipe precisa restabelecer a conexão.

**A falsa segurança**

Uma região parece ficar tranquila por algum tempo, mas pequenos sinais indicam que a entidade continua presente.

Esses eventos devem ser construídos com sistemas simples de áudio, iluminação, animação, triggers e estados.

Não crie um sistema complexo de geração procedural de sustos.

## 9.5 Personalização dos acontecimentos

Alguns eventos podem afetar todos os participantes.

Outros podem acontecer de maneira particular para um jogador.

Exemplo:

O jogador A escuta uma chamada.

O jogador B percebe uma luz acendendo no painel.

O jogador C escuta três batidas.

O jogador D recebe uma interferência.

Essas informações devem estar relacionadas ao mesmo acontecimento.

O sistema deve coordenar os eventos por meio do anfitrião para evitar contradições acidentais que prejudiquem os puzzles.

Eventos puramente atmosféricos podem apresentar pequenas variações individuais, desde que não alterem informações essenciais da investigação.

## 9.6 Controle de intensidade e segurança

Evite:

- Sons excessivamente altos.
- Sustos repetitivos.
- Flash constante na tela.
- Mudanças bruscas de câmera.
- Efeitos que causem desconforto visual desnecessário.
- Perseguições sem aviso.
- Aparecimentos aleatórios atrás do jogador.

Implemente controle de volume independente para efeitos de susto e ligações.

Inclua opções para reduzir flashes e intensidade visual de eventos.

O medo deve surgir da expectativa, da incerteza e da descoberta.

# 10. A ENTIDADE: A ASSINANTE

A Assinante é a principal presença sobrenatural.

Ela não precisa perseguir os jogadores constantemente.

Sua existência está relacionada à rede telefônica.

Ela pode:

- Interferir em ligações.
- Reproduzir gravações.
- Manipular extensões.
- Criar sinais falsos.
- Alterar a atividade de determinados telefones.
- Interromper comunicação entre participantes.
- Provocar acontecimentos ambientais.
- Manifestar-se fisicamente em momentos específicos.

## 10.1 Inteligência artificial simplificada

Implemente uma máquina de estados:

- Adormecida.
- Observando.
- Investigando.
- Interferindo.
- Contaminando linhas.
- Aproximando-se.
- Manifestando-se.
- Recuando.

Cada estado deve ter condições claras de entrada e saída.

Não implemente inteligência artificial baseada em dezenas de comportamentos complexos.

A primeira versão não precisa de um monstro navegando permanentemente pelo mapa.

Priorize a presença através do ambiente e dos telefones.

## 10.2 Aparições físicas

Crie um modelo visual memorável para a entidade.

Não revele sua aparência completa desde o começo.

Utilize silhuetas, reflexos, sombras e aparições distantes.

Em uma etapa posterior, implemente uma movimentação simples por NavMesh caso o gameplay realmente precise de uma ameaça física.

Não construa uma IA de perseguição avançada antes de validar a experiência básica.

# 11. MECÂNICA ESPECIAL: LINHA OCUPADA

Implemente uma mecânica de isolamento temporário.

Em determinados acontecimentos, A Assinante consegue interferir diretamente na comunicação de um jogador.

O jogador entra no estado chamado Linha Ocupada.

Durante esse estado:

- Seu telefone e receptor apresentam interferência.
- Ele escuta uma versão distorcida de determinados sinais.
- Algumas comunicações compartilhadas ficam temporariamente bloqueadas.
- Ele recebe pistas que podem ajudar a entender como restabelecer a conexão.
- Os demais participantes percebem que sua extensão desapareceu ou está comprometida.

A equipe precisa realizar uma ação na central ou encontrar a ligação correta para restabelecer a comunicação.

O jogador isolado não deve ficar completamente sem possibilidade de jogar.

Ele deve continuar podendo explorar e resolver uma pequena etapa.

No modo individual, o jogador deve resolver uma sequência equivalente utilizando seu próprio equipamento.

Essa mecânica deve ser curta, tensa e compreensível.

Não transforme o isolamento em uma punição excessivamente longa ou frustrante.

O efeito deve ser principalmente psicológico e cooperativo.

# 12. DIFICULDADE E EQUILÍBRIO

O jogo deve ser desafiador, mas justo.

A dificuldade não deve depender de inimigos com mais vida ou de aumentar aleatoriamente a quantidade de sustos.

Crie três níveis iniciais:

**Investigação**
- Mais tempo para interpretar ligações.
- Menos interferências.
- Mais pistas redundantes.
- Ritmo mais tranquilo.

**Operação**
- Dificuldade padrão.
- Quantidade equilibrada de pistas.
- Maior presença da entidade.
- Puzzles com mais combinações.

**Ruptura**
- Menos pistas redundantes.
- Mais ligações falsas.
- Maior pressão da contaminação.
- Janelas de reação menores.
- Acontecimentos sobrenaturais mais frequentes, mas ainda controlados.

Não torne as soluções impossíveis de compreender.

Os jogadores devem ser recompensados por prestar atenção, cooperar e aprender as regras.

## 12.1 Dificuldade adaptável à quantidade de participantes

A experiência precisa funcionar com um, dois, três ou quatro jogadores.

No modo individual:
- Permitir alternância de camadas.
- Fornecer tempo adicional quando necessário.
- Ajustar tarefas simultâneas.
- Evitar puzzles que exijam duas pessoas fisicamente.

Em grupos menores, redistribuir as informações.

Em grupos maiores, aumentar a complementaridade das informações, não simplesmente a quantidade de inimigos.

## 12.2 Falhas e recuperação

Errar um puzzle não deve resultar imediatamente em morte.

Uma conexão incorreta pode aumentar a contaminação, gerar uma interferência ou provocar uma manifestação.

Crie oportunidades para corrigir erros.

Quando um jogador for afetado pela entidade, permita que os companheiros o ajudem.

A falha total da missão deve ser possível, mas deve decorrer de decisões ou situações compreensíveis.

Evite punições irreversíveis e aleatórias.

# 13. CICLO DE GAMEPLAY E MISSÕES

O ciclo principal será:

1. Preparar os equipamentos.
2. Entrar na instalação.
3. Investigar uma região.
4. Encontrar ligações.
5. Separar informações.
6. Compartilhar pistas.
7. Resolver conexões.
8. Identificar a origem do fenômeno.
9. Isolar ou estabilizar a anomalia.
10. Sobreviver aos últimos acontecimentos.
11. Concluir a missão.

Cada missão precisa ter começo, desenvolvimento e conclusão.

Não faça todas as missões parecerem iguais.

Varie os objetivos, as ligações, as condições e os acontecimentos.

## Campanha

Desenvolva uma campanha principal com cinco capítulos:

**Prólogo — A Primeira Ligação**

Apresentação da instalação e dos controles.

**Capítulo 1 — A Central**

Primeiras extensões inexistentes e início das investigações.

**Capítulo 2 — Arquivo de Vozes**

Descoberta de gravações e acontecimentos antigos.

**Capítulo 3 — A Linha Morta**

Exploração dos túneis e circuitos subterrâneos.

**Capítulo 4 — O Outro Lado**

Ligações aparentemente relacionadas aos próprios investigadores.

**Capítulo 5 — Extensão Zero**

Descoberta da origem da presença e conclusão da campanha.

Crie três possibilidades de conclusão narrativa, desde que sejam coerentes com as decisões e descobertas feitas durante o jogo.

A campanha pode reutilizar uma instalação central dividida em setores, liberando novas áreas gradualmente.

Não crie cinco mundos enormes independentes.

Priorize uma instalação detalhada e bem planejada.

# 14. REJOGABILIDADE

Crie variações controladas de:

- Ligações.
- Gravações.
- Extensões.
- Pistas.
- Circuitos contaminados.
- Puzzles.
- Eventos.
- Localizações de determinados objetos.
- Comportamentos da entidade.

Utilize seeds compartilhadas pelo anfitrião.

As variações devem respeitar as regras de cada missão.

Não permita combinações impossíveis.

A rejogabilidade deve vir da investigação e das diferentes situações encontradas, não de aleatoriedade sem lógica.

# 15. DESIGN DOS AMBIENTES

Crie áreas pequenas ou médias, interligadas e com identidade própria.

Áreas principais:

- Recepção.
- Sala da central telefônica.
- Salas de operação.
- Arquivo de gravações.
- Sala de energia.
- Depósito.
- Corredores técnicos.
- Subsolo.
- Túneis de cabos.
- Alojamentos antigos.
- Área externa de retransmissão.

Cada área deve ter elementos que indiquem sua finalidade.

Utilize objetos para contar histórias.

Telefones, ferramentas, papéis, equipamentos, mesas, cabos, armários e fotografias devem ajudar a criar a identidade da instalação.

Não utilize decoração aleatória.

Não permita:
- Objetos flutuando.
- Geometria atravessada.
- Escalas incorretas.
- Cabos sem organização coerente.
- Texturas inconsistentes.
- Iluminação sem propósito.

Desenvolva ambientes que sejam visualmente interessantes sem exigir uma quantidade excessiva de modelos.

# 16. ÁUDIO E DESIGN SONORO

O áudio deve ser uma das principais ferramentas de terror.

Implemente:

- Áudio espacial 3D.
- Reverberação.
- Oclusão sonora.
- Atenuação por distância.
- Sons de telefones.
- Interferências analógicas.
- Sons estruturais.
- Ruídos ambientais.
- Passos.
- Portas.
- Máquinas.
- Sons da entidade.
- Silêncios intencionais.

Cada setor deve possuir uma identidade sonora.

As vozes precisam ser claras e ter personalidade.

Utilize filtros específicos para simular transmissões telefônicas.

Não aumente excessivamente o volume de sustos.

Não implemente chat de voz próprio na primeira versão.

A comunicação entre os jogadores poderá acontecer por serviços de terceiros ou comunicação externa, sem fazer parte da arquitetura obrigatória do jogo.

Adicione indicadores rápidos de comunicação, como marcar objetos, extensões e locais relevantes.

# 17. MULTIPLAYER DIRETO ENTRE JOGADORES

ESTA É UMA EXIGÊNCIA FUNDAMENTAL.

O jogo deve permitir partidas cooperativas sem servidor central.

Utilize uma arquitetura peer-hosted, na qual um jogador atua como anfitrião e os demais se conectam diretamente a ele.

O anfitrião é também um jogador normal.

Não utilize:

- Unity Relay.
- Unity Lobby.
- Unity Matchmaker.
- Servidores dedicados.
- Servidores de retransmissão.
- Banco de dados remoto.
- APIs externas de matchmaking.
- Sistemas obrigatórios de contas.
- Infraestrutura central para iniciar partidas.
- Steam P2P como dependência obrigatória.

A partida deve funcionar sem esses serviços.

## 17.1 Tecnologia

Avalie uma implementação utilizando:

- Netcode for GameObjects.
- Unity Transport.
- Comunicação UDP direta.
- Arquitetura host-client.

Utilize versões compatíveis e estáveis dos pacotes.

O anfitrião mantém o estado autoritativo da partida.

Os clientes enviam solicitações de interação.

O anfitrião valida e distribui as alterações.

Não crie uma rede mesh completa entre todos os jogadores sem necessidade.

## 17.2 Interface de conexão

Menu principal:

- Jogar sozinho.
- Criar partida.
- Entrar em partida.
- Configurações.
- Sair.

Ao criar partida:

1. O jogador inicia o modo anfitrião.
2. O jogo abre a porta configurada.
3. O jogador visualiza seus endereços locais.
4. O jogo gera uma sequência de convite contendo as informações diretas da conexão e uma chave de sessão.
5. O anfitrião compartilha o convite com os amigos.

Ao entrar:

1. O jogador cola o convite ou informa o endereço IP.
2. O jogo interpreta o endereço e a porta.
3. Tenta conectar diretamente.
4. Valida a chave da sessão.
5. Entra no lobby.

O código de convite é somente uma representação dos dados necessários para conectar.

Não deve depender de consulta a um servidor central.

Para uma conexão pela internet, o endereço público do anfitrião precisa estar disponível e a porta configurada corretamente.

Não finja que um código curto consegue localizar um anfitrião sem qualquer serviço de descoberta.

## 17.3 Rede local

Implemente descoberta automática de partidas na mesma rede local por broadcast UDP ou mecanismo equivalente.

Também permita conexão manual por IP local.

A descoberta automática não deve ser requisito para participar.

Considere limitações de isolamento de clientes Wi-Fi e configurações de roteadores.

## 17.4 Internet

Para conexão direta pela internet, permita:

- IPv4 público com encaminhamento da porta UDP.
- IPv6 global quando disponível.
- UPnP IGD, NAT-PMP ou PCP opcional, quando compatível.
- Configuração manual da porta.

UPnP precisa ser opcional e solicitar autorização do usuário antes de modificar o roteador.

Explique como configurar o encaminhamento de porta manualmente.

Não implemente Relay ou STUN como solução escondida.

IMPORTANTE:

Existem conexões com CGNAT e NAT restritivo que não permitem uma conexão direta iniciada de fora.

Nesses casos, informe a limitação e não prometa que a partida poderá ser conectada sem alguma infraestrutura externa.

Não adicione um servidor central apenas para contornar silenciosamente esse problema.

Apresente diagnóstico de conexão com mensagens compreensíveis.

## 17.5 Privacidade e segurança

O endereço IP público do anfitrião poderá ser conhecido pelos participantes que se conectarem.

Informe isso claramente.

Não envie endereços IP para serviços externos.

Não mantenha histórico de IPs.

Não colete informações pessoais.

Utilize uma chave de sessão aleatória.

Valide pacotes e solicitações.

Limite a frequência das mensagens.

Não aceite alterações arbitrárias de estado enviadas pelos clientes.

Não invente criptografia.

Verifique quais mecanismos de segurança são suportados pela versão escolhida de Unity Transport e documente as limitações.

## 17.6 Sincronização

Sincronize:

- Personagens.
- Movimentação.
- Interações.
- Telefones.
- Extensões.
- Cabos.
- Puzzles.
- Equipamentos compartilhados.
- Contaminação.
- Eventos sobrenaturais.
- Estado da entidade.
- Objetivos.
- Finais da missão.

O anfitrião controla o estado compartilhado.

Os clientes recebem as atualizações necessárias.

Não transmita arquivos de áudio pela rede.

As chamadas serão reproduzidas localmente.

O anfitrião envia o identificador da chamada, o instante de reprodução e a distribuição das camadas.

Cada cliente executa o arquivo correspondente.

Utilize sincronização temporal suficiente para que as informações pertençam ao mesmo acontecimento.

## 17.7 Desconexão

Implemente:

- Limite de quatro jogadores.
- Lobby simples.
- Indicação de participantes.
- Entrada antes da missão.
- Reconexão quando possível.
- Recuperação de estados.
- Mensagens de erro.
- Encerramento seguro.

Se o anfitrião sair, a sessão termina de maneira organizada.

Não implemente migração de anfitrião na primeira versão.

A prioridade é estabilidade e simplicidade.

# 18. INTERFACE E ACESSIBILIDADE

A interface deve combinar com a identidade visual.

Não utilize layouts genéricos de protótipo.

Implemente:

- Tela inicial.
- Lobby.
- Configuração de partidas.
- Inventário simples.
- Caderno de investigação.
- Menu de pausa.
- Configurações.
- Objetivos.
- Conclusão de missão.

A HUD deve ser discreta.

Inclua:

- Legendas.
- Controle de volume por categoria.
- Ajuste de sensibilidade.
- Campo de visão configurável.
- Opção para reduzir flashes.
- Opção para reduzir intensidade de sustos visuais.
- Configuração de resolução.
- Configurações de qualidade gráfica.

As informações necessárias para resolver puzzles auditivos também devem possuir alternativas visuais quando isso for importante para acessibilidade, sem retirar a natureza investigativa.

# 19. PROGRESSÃO E SALVAMENTO

Crie uma progressão simples.

Desbloqueios possíveis:

- Novos equipamentos.
- Gravações.
- Documentos.
- Registros.
- Elementos cosméticos.
- Conteúdos narrativos.

Não implemente microtransações.

Não utilize grind artificial.

Não exija conta para jogar.

Salve os dados localmente.

O anfitrião controla o progresso compartilhado da campanha.

Os convidados podem manter registros pessoais locais.

Não utilize servidores de nuvem para salvar partidas.

# 20. ARQUITETURA E ORGANIZAÇÃO DO PROJETO

Organize o projeto em módulos.

Estrutura sugerida:

Assets/
  _Project/
    Art/
    Audio/
    Characters/
    Environment/
    Materials/
    Models/
    Prefabs/
    Scenes/
    Scripts/
      Core/
      Player/
      Interaction/
      Telephone/
      Investigation/
      Anomaly/
      AI/
      Multiplayer/
      Missions/
      Audio/
      UI/
      SaveSystem/
    ScriptableObjects/
    Settings/
    Tests/
    Editor/

Documentos na raiz:

- README.md
- GAME_DESIGN_DOCUMENT.md
- STORY_BIBLE.md
- ART_DIRECTION.md
- AUDIO_DIRECTION.md
- TECHNICAL_ARCHITECTURE.md
- NETWORK_ARCHITECTURE.md
- PERFORMANCE_TARGETS.md
- LINUX_BUILD.md
- TEST_PLAN.md
- DEVELOPMENT_ROADMAP.md
- DECISIONS.md

Utilize ScriptableObjects para:

- Ligações.
- Gravações.
- Extensões.
- Puzzles.
- Missões.
- Entidade.
- Eventos sobrenaturais.
- Equipamentos.
- Configurações de dificuldade.

O Diretor de Tensão deve utilizar eventos modulares com configurações reutilizáveis.

Cada evento deve possuir condições, cooldown, intensidade, efeitos e critérios de ativação.

Evite sistemas monolíticos e abstrações excessivas.

# 21. TECNOLOGIAS E DEPENDÊNCIAS

Utilize uma versão LTS estável da Unity compatível com os pacotes escolhidos.

Verifique as versões e documentações atuais antes de definir dependências.

Priorize:

- Unity.
- C#.
- URP.
- Netcode for GameObjects.
- Unity Transport.
- Input System.
- AI Navigation, somente quando necessário.
- Ferramentas de profiling e testes.

Não adicione serviços externos obrigatórios.

Não adicione bibliotecas comerciais sem informar.

Verifique licenças, compatibilidade Linux e possibilidade de redistribuição de todos os componentes.

# 22. PLANO DE DESENVOLVIMENTO

Não tente desenvolver tudo de uma vez.

Trabalhe em fases verificáveis.

## Fase 0 — Reconhecimento

Examine o repositório.

Verifique se existe projeto Unity.

Identifique versão, pacotes, estrutura, arquivos e ferramentas disponíveis.

Verifique o que pode ser compilado e testado no ambiente de nuvem.

Preserve trabalho existente.

Se o repositório estiver vazio, crie o projeto conforme a arquitetura escolhida.

## Fase 1 — Pré-produção

Elabore a documentação.

Finalize a identidade visual.

Detalhe a mecânica de ligações fragmentadas.

Descreva o Diretor de Tensão.

Defina as regras da Assinante.

Descreva a Linha Ocupada.

Planeje a campanha.

Defina a arquitetura de multiplayer direto.

Documente orçamento de desempenho e VRAM.

## Fase 2 — Primeira fatia vertical

Implemente:

- Personagem em primeira pessoa.
- Câmera.
- Movimentação.
- Interações.
- Uma pequena sala da central.
- Um telefone funcional.
- Central telefônica.
- Primeira ligação.
- Camadas de áudio.
- Primeiro puzzle.
- Primeiro evento sobrenatural.
- Primeiro comportamento da entidade.

O objetivo é demonstrar que o jogo é interessante antes de criar a campanha inteira.

## Fase 3 — Multiplayer

Implemente:

- Host.
- Cliente.
- Conexão direta por IP.
- Conexão LAN.
- Sincronização de movimentação.
- Sincronização de interações.
- Distribuição de informações diferentes.
- Estado compartilhado da missão.
- Tratamento de desconexão.

Comece com dois jogadores e depois teste três e quatro.

## Fase 4 — Sistema de terror

Implemente o Diretor de Tensão.

Crie eventos reutilizáveis.

Adicione os diferentes tipos de sustos.

Implemente o isolamento Linha Ocupada.

Ajuste intensidade, frequência e cooldown.

Teste se os acontecimentos permanecem assustadores sem se tornarem repetitivos.

## Fase 5 — Conteúdo completo

Desenvolva:

- Todos os capítulos.
- Puzzles.
- Equipamentos.
- Gravações.
- Cenários.
- A entidade.
- Progressão.
- Variações.
- Finais.

## Fase 6 — Identidade visual e sonora

Aplique a direção de arte a todos os setores.

Finalize modelos, materiais, texturas, iluminação, sons, animações e interface.

Mantenha a consistência visual.

Verifique o consumo de VRAM.

## Fase 7 — Estabilização

Teste a campanha inteira.

Teste partidas com quatro participantes.

Teste rede local.

Teste conexão direta pela internet em ambientes que permitam isso.

Teste falhas de conexão.

Corrija bugs.

Otimize CPU, GPU e memória.

Prepare a distribuição Linux.

# 23. TESTES E GARANTIA DE QUALIDADE

Não considere funcionalidades concluídas apenas porque o código foi escrito.

Sempre que possível:

- Compile os scripts.
- Execute testes.
- Teste cenas.
- Verifique erros e warnings.
- Faça profiling.
- Teste multiplayer.
- Valide puzzles.
- Confira sincronização de áudio.
- Verifique consumo de VRAM.
- Teste builds nativas Linux.

Não declare compatibilidade com hardware não testado.

Não invente resultados.

Se o ambiente de nuvem não permitir executar a Unity, documente isso e execute as verificações que realmente forem possíveis.

Prepare instruções reproduzíveis para validar os recursos localmente.

# 24. REGRAS OBRIGATÓRIAS DE DESENVOLVIMENTO

1. Compreenda a arquitetura antes de modificar arquivos.
2. Preserve funcionalidades existentes.
3. Não adicione complexidade sem necessidade.
4. Priorize sistemas reutilizáveis.
5. Evite dependências desnecessárias.
6. Não utilize servidores externos para o multiplayer.
7. Não exija contas.
8. Não introduza compras ou dependências comerciais sem autorização.
9. Não coloque segredos no Git.
10. Não invente testes ou resultados.
11. Não ignore erros.
12. Não crie sistemas falsos.
13. Não misture estilos visuais incompatíveis.
14. Não sacrifique o desempenho por efeitos desnecessários.
15. Não crie uma IA excessivamente complexa.
16. Não transforme o projeto em um mundo aberto.
17. Não crie conteúdo antes de validar a mecânica central.
18. Atualize a documentação.
19. Faça commits organizados.
20. Utilize branches e Pull Requests para revisão.

# 25. PRIMEIRA TAREFA OBRIGATÓRIA

Comece imediatamente.

Não responda apenas com ideias.

Execute as seguintes atividades:

1. Analise o repositório e o ambiente.
2. Verifique os recursos disponíveis na nuvem.
3. Defina a arquitetura técnica.
4. Crie a documentação inicial.
5. Detalhe a identidade visual.
6. Detalhe a primeira missão.
7. Projete a central telefônica.
8. Projete a primeira ligação fragmentada.
9. Defina os primeiros eventos do Diretor de Tensão.
10. Projete a primeira manifestação da Assinante.
11. Implemente o personagem em primeira pessoa.
12. Implemente a primeira sala explorável.
13. Implemente o primeiro telefone.
14. Crie uma ligação funcional.
15. Prepare o primeiro puzzle.
16. Comece a implementação multiplayer direta para dois participantes.

Não tente entregar o jogo inteiro em uma execução.

Entregue uma primeira experiência jogável e demonstre a viabilidade de suas mecânicas.

Depois da validação, avance para os próximos sistemas.

# 26. RELATÓRIO DE DESENVOLVIMENTO

Ao concluir cada etapa, apresente:

- O que foi desenvolvido.
- Arquivos criados.
- Arquivos modificados.
- Funcionalidades realmente operacionais.
- Testes executados.
- Problemas encontrados.
- Limitações do ambiente.
- Desempenho medido.
- Próxima prioridade.

Não declare funcionalidades concluídas sem validação.

# 27. OBJETIVO FINAL

Entregar EXTENSÃO ZERO como um jogo completo, com:

- Identidade visual original e agradável.
- Desempenho adequado para GPUs com 4 GB de VRAM ou mais.
- Terror psicológico inteligente.
- Sustos contextuais e variados.
- Uma entidade memorável.
- Puzzles interessantes.
- Investigação recompensadora.
- Cooperação verdadeira.
- Modo individual funcional.
- Multiplayer direto entre jogadores.
- Campanha completa.
- Boa rejogabilidade.
- Compatibilidade nativa com Linux.
- Código organizado e sustentável.

A experiência deve ser assustadora, divertida e interessante de jogar com amigos.

Não tente impressionar através da quantidade de sistemas, objetos, monstros ou efeitos.

Impressione através da qualidade das ideias, da execução das mecânicas e da forma como o jogo faz os jogadores pensarem, cooperarem e sentirem medo.

**PRINCÍPIO FUNDAMENTAL DO PROJETO: criar um jogo tecnicamente simples e eficiente, mas criativamente rico, no qual uma ligação telefônica pode ser mais assustadora do que encontrar um monstro.**