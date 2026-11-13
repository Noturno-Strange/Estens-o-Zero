# EXTENSÃO ZERO — ART DIRECTION

## 1. Intenção

Criar uma identidade 3D autoral, estilizada e coerente, com desempenho adequado a GPUs de 4 GB de VRAM. Não buscar fotorrealismo. O objetivo é uma instalação industrial antiga que pareça **habitada por uma memória**, com geometria legível, materiais com personalidade e iluminação atmosférica.

Referências de sensação: estações de rádio abandonadas, painéis telefônicos analógicos, arquivos mortos, corredores técnicos, luz de tungstênio suja, névoa fria.

## 2. Estilo

- 3D estilizado semirrealista, proporções levemente alongadas.
- Silhuetas fortes e legíveis; detalhes concentrados onde o jogador interage.
- Geometria modular com quinas e painéis, não superfícies lisas genéricas.
- Texturas pintadas/procedurais com grão, manchas e desgaste controlado.
- Nada de cel-shading cartunesco; nada de PBR fotorrealista.
- Escala humana rigorosa: portas 2,10 m, pé-direito 3,6–4,2 m, mesas 0,75 m.

## 3. Paleta

| Uso | Cor | Hex |
|---|---|---|
| Sombra profunda | verde-petróleo quase preto | `#08110F` |
| Parede base | verde institucional envelhecido | `#1E3A34` |
| Parede clara | verde-cinza descascado | `#3F5F55` |
| Metal | aço escuro | `#2A3230` |
| Papel/arquivo | creme amarelado | `#D6C7A1` |
| Luz quente | tungstênio | `#FFC98A` |
| Destaque | vermelho de emergência | `#C63C32` |
| Indicador | âmbar | `#E3A64B` |
| Sinal | verde-fósforo | `#7FB89A` |
| Névoa | azul-esverdeado | `#162824` |

A paleta não é obrigatória em todos os setores. O arquivo usa creme/âmbar; os túneis usam azul frio e ferrugem; a sala de energia usa vermelho e preto.

## 4. Materiais e texturas

Texturas geradas proceduralmente nesta fatia (256–512 px, repetição, mipmaps):

| Material | Textura | Uso |
|---|---|---|
| Concreto | ruído + manchas escuras | piso, pilares |
| Parede institucional | duas cores + descascado | paredes |
| Piso vinílico | ladrilhos + sujeira | sala da central |
| Madeira | veios + riscos | mesas, gavetas |
| Metal pintado | arranhões + rebites | painéis, armários |
| Ferrugem | manchas laranja/marrom | tubulações, portas |
| Papel | fibras + manchas | documentos, cartazes |
| Borracha | ruído escuro | cabos, telefones |
| Emissivo âmbar | cor chapada + halo | lâmpadas, indicadores |
| Emissivo vermelho | cor chapada | emergência, contaminação |

Regras:

- Texturas de 512 px para superfícies grandes; 256 px para props; 1024 px apenas se necessário.
- Compressão desktop (BC7/BC5) quando aplicável; mipmaps ligados.
- Materiais compartilhados; variação por cor/UV, não por material novo.
- Nada de texturas 4K.
- Normal maps apenas em painéis e pisos; gerados a partir do height quando possível.

## 5. Kit modular

Um conjunto pequeno de peças cobre a instalação:

- Parede 2×3,6 m, parede com porta, parede com janela, canto, rodapé.
- Piso 2×2 m, forro 2×2 m, pilar, viga.
- Painel de central 1×1, 2×1, 3×2.
- Porta, porta dupla, portão de correr.
- Mesa, cadeira, armário, estante, gaveta, caixa.
- Telefone de mesa, telefone de parede, telefone público.
- Luminária pendente, luminária industrial, luminária de emergência.
- Tubo, eletrocalha, caixa de passagem, quadro elétrico.
- Papéis, pastas, carimbos, relógio, cartazes, extintor.

Nada de decoração aleatória. Cada objeto deve indicar a função do espaço.

## 6. Iluminação

- Base fria e escura; poças quentes de tungstênio.
- Luz principal: pendentes de 2700–3200 K, intensidade 1,2–2,0, alcance 6–9 m.
- Sombras apenas nas 2–3 luzes mais importantes; demais sem sombra.
- Luz de emergência vermelha em corredores e saída.
- Emissivos pequenos (LEDs, medidores) para leitura visual.
- Névoa exponencial, densidade 0,02–0,04, cor `#162824`.
- Pós-processamento: bloom contido, vinheta, grão fino, dessaturação leve, ACES.
- Nenhum ray tracing, nenhuma GI dinâmica pesada, nenhuma volumetria constante.
- Iluminação pré-calculada será usada quando os setores forem fechados; nesta fatia, luzes em tempo real limitadas.

## 7. A Assinante

- Silhueta alta (2,3 m), estreita, membros longos, cabeça pequena demais.
- Corpo em preto esverdeado, quase absorvendo luz; superfície com fios finos.
- Rosto não revelado: emaranhado de cabos e um LED vermelho.
- Aproximação: aparece em vulto, contraluz, reflexo, fumaça; nunca em close no início.
- Animação: balanço mínimo, respiração errada, parada absoluta quando observada.
- Primeira manifestação: fim do corredor, 12–18 m, silhueta imóvel; desaparece ao ser olhada por 1,2 s ou ao se aproximar.

## 8. Interface

- Estética de painel analógico: placas escuras, linhas finas, tipografia condensada.
- Cor de destaque: âmbar para objetivos; vermelho apenas para perigo/contaminação.
- HUD discreta: objetivo no topo esquerdo, status no topo direito, prompt no centro inferior, legendas na base.
- Caderno: papel creme, abas, carimbos, manuscrito simulado (texto em itálico).
- Menu: fundo 3D desfocado, painel lateral, título com numeração de extensão.
- Sem ícones genéricos de protótipo; usar formas e rótulos.

## 9. Orçamento visual da fatia

| Item | Meta |
|---|---|
| Triângulos visíveis | < 250 mil |
| Draw calls | < 350 |
| Materiais únicos | < 25 |
| Texturas únicas | < 20 (256–512 px) |
| Luzes em tempo real | ≤ 8, 2 com sombra |
| VRAM estimada | < 1,2 GB em 1080p |
| Pós-processamento | bloom, vinheta, grão, color grading |

Medições reais devem ser feitas no Profiler/RenderDoc em GPU Linux; nenhuma compatibilidade é declarada sem medição.

## 10. Regras de qualidade

- Nenhum objeto flutuando.
- Nenhuma geometria atravessada.
- Nenhuma escala inconsistente.
- Cabos sempre com origem, caminho e destino coerentes.
- Texturas com densidade de texel parecida entre props próximos.
- Iluminação com propósito narrativo/funcional.
- Reaproveitar modelos e materiais antes de criar novos.
- Adaptar qualquer asset externo à paleta e proporção.
