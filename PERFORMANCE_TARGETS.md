# EXTENSÃO ZERO — PERFORMANCE TARGETS

## 1. Alvo

- Resolução de referência: 1920×1080.
- GPU mínima pretendida: 4 GB de VRAM.
- Meta: 60 FPS; mínimo aceitável em configurações baixas/médias: 30 FPS estáveis.
- VRAM alvo em 1080p, configurações baixas/médias: **≤ 3,2 GB**; meta da fatia: **≤ 1,2 GB**.
- Build Linux x86_64, Mono.

VRAM não determina sozinha o desempenho. Nenhuma compatibilidade é declarada sem medição real.

## 2. Orçamento gráfico

| Recurso | Meta |
|---|---|
| Triângulos visíveis | < 250.000 |
| Draw calls | < 350 |
| Materiais únicos | < 25 |
| Texturas únicas | < 20 |
| Tamanho de textura | 256–512 px; 1024 excepcional |
| Luzes em tempo real | ≤ 8 |
| Luzes com sombra | ≤ 2 |
| Partículas | ≤ 200 |
| Pós-processamento | bloom, vinheta, grão, color grading |
| Sombras | 1024–2048, distância curta |
| Mipmaps | ligados |
| Compressão | desktop (BC7/BC5/BC1) |

## 3. Técnicas

- URP, não HDRP.
- Sem ray tracing.
- Sem GI dinâmica pesada.
- Iluminação pré-calculada quando os setores forem fechados (fatia: luzes locais limitadas).
- Light probes para personagens/props dinâmicos.
- Occlusion culling por setor (a configurar quando houver mais de uma cena).
- Frustum culling nativo.
- Static batching do ambiente em runtime (`StaticBatchingUtility.Combine`).
- Materiais compartilhados e texturas procedurais pequenas.
- LOD apenas em objetos grandes/repetidos (capítulos futuros).
- Névoa exponencial barata.
- Sem volumetria constante.
- Sem reflexos em tempo real; cubemap simples ou nenhum.
- UI em canvas único, sem rebuild por frame.
- Áudio com pools; sem alocações por frame.

## 4. Qualidade

| Nível | Render scale | Sombras | Luzes | Pós |
|---|---|---|---|---|
| Baixa | 0,8 | 1, 1024 | 4 | mínimo |
| Média | 1,0 | 2, 2048 | 6 | padrão |
| Alta | 1,0 | 2, 2048 | 8 | completo |

Implementado em `GameSettings.ApplyQuality`: seleciona o nível Unity "Low", "Medium" ou "High" e ajusta uma **cópia em runtime** do asset URP (escala de renderização 0,8/1,0/1,0; luzes adicionais 4/6/8; distância de sombra 14/22/26 m). O asset do projeto não é alterado. O pós-processamento é o mesmo nos três níveis. A qualidade padrão é Média.

## 5. Medição

Procedimento reproduzível:

1. Build Linux com `./scripts/build-linux.sh`.
2. Executar com `-screen-width 1920 -screen-height 1080 -screen-fullscreen 0`.
3. No jogo, ativar overlay de estatísticas (F3) ou usar Unity Profiler em build de desenvolvimento.
4. Registrar: FPS médio/1% low, tempo de CPU main thread, tempo de GPU, draw calls, triângulos, memória de texturas, VRAM total.
5. Testar cenas: menu, sala da central, corredor com manifestação, quatro jogadores.
6. Repetir em Baixa/Média/Alta.

Ferramentas: Unity Profiler, Frame Debugger, `nvidia-smi`/`radeontop`, RenderDoc (Linux), `/usr/bin/time -v`.

## 6. Estado atual

- A fatia vertical original foi importada e compilada na Unity 6000.3.28f1 em uma sessão anterior.
- **A campanha completa (esta versão) não foi compilada nem executada na Unity.** Foi compilada contra referências da Unity 2021.3 + stubs (ADR-018).
- **Nenhum FPS, VRAM ou tempo de carregamento foi medido.** Nenhum número de desempenho é declarado.
- Medidas de orçamento já no código:
  - culling de luzes por setor: só o setor atual e os vizinhos ficam acesos, no máximo ~8 por região;
  - 2 luzes com sombra no mundo (central e sala de energia), além da lanterna local;
  - static batching de paredes, pisos e tetos (`StaticBatchingUtility.Combine`);
  - materiais compartilhados e uma textura de ruído de 32 px;
  - áudio procedural com cache;
  - UI em canvas único;
  - eventos e oclusão atualizados por intervalo, não por frame.
- O overlay F3 mostra FPS, tempo de frame, memória alocada, a GPU e a VRAM declarada pelo driver.

## 7. Riscos

- Sombras em luzes demais no corredor.
- Overdraw de partículas de névoa.
- Muitos AudioSources simultâneos.
- Sobreposição de canvas/legendas.
- Custo de geração procedural de texturas no primeiro frame (fazer uma vez no bootstrap).
- Custo de `LineRenderer` dos cabos (poucos cabos, seguro).

## 8. Critérios de aceite

- Build Linux inicia e chega ao menu.
- Cena principal estável por 10 min sem vazamento de memória.
- 60 FPS em GPU de referência de 6–8 GB em Médio; 30 FPS mínimos em 4 GB em Baixo.
- VRAM ≤ 3,2 GB em 1080p Médio.
- Nenhum erro/warning crítico no log do build.
- Testes EditMode verdes.
