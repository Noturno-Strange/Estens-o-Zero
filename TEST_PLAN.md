# EXTENSÃO ZERO — TEST PLAN

## 1. Filosofia

Nenhuma funcionalidade é considerada pronta só porque o código foi escrito. Este plano separa o que foi realmente executado do que depende de validação local.

## 2. O que foi executado nesta entrega

| Verificação | Onde | Resultado |
|---|---|---|
| Compilação do runtime (player e editor), do editor e dos testes | `tools/compile-check/run.sh` (referências Unity 2021.3 + stubs) | sem erros |
| 32 testes de lógica pura (NUnit, .NET 8) | `tools/compile-check/run.sh` | 32/32 passaram |
| Compilação na Unity 6000.3.28f1 | — | **não executada** |
| Testes EditMode completos e PlayMode na Unity | — | **não executados** |
| Build Linux, rede real, desempenho | — | **não executados** |

## 3. Testes automatizados

```bash
./scripts/run-tests.sh            # EditMode, na Unity
```
```bash
./scripts/run-tests.sh playmode   # PlayMode, na Unity; precisa de GPU/servidor gráfico
```
```bash
tools/compile-check/run.sh        # sem Unity: compilação + lógica pura
```

### EditMode

| Suíte | Cobre | Roda fora da Unity |
|---|---|---|
| `SessionCodecTests` | normalização, validação e hash da chave | sim |
| `LayerDistributorTests` | 1–4 jogadores, cobertura das 4 camadas, sem duplicação | sim |
| `SwitchboardRulesTests` | extensões válidas, 214↔0, mesma extensão, linha 9 | sim |
| `CampaignRulesTests` | liberação de capítulos, próximo capítulo, requisitos dos finais, equipamento (máx. 2), dificuldade, seed | sim |
| `NetworkProtocolTests` | convite (ida e volta, IPv6, versão, porta, chave), chave aleatória, beacon LAN, aprovação | sim |
| `LineBusyRulesTests` | duração 15–25 s, limiar, frequência de retorno, giro | sim |
| `NotebookModelTests` | registrar, deduplicar, rejeitar id vazio, anotações | não (usa `Time`) |
| `TensionSelectorTests` | faixa de tensão, etapa, sem repetição, cooldown | não (usa `ScriptableObject`) |

### PlayMode

| Suíte | Cobre |
|---|---|
| `SoloMissionPlayModeTests` | prólogo completo: telefone, camadas, cabo, 214→9 com Linha Ocupada, 214→0, segunda chamada, corredor, Assinante, isolamento, save. Salva capturas em `$TMPDIR/extensao-zero-shots` |

Ainda não existem testes automatizados para os capítulos 1–5; eles são cobertos pelo roteiro manual da §4.

## 4. Testes manuais (na Unity ou no build)

### 4.1 Inicialização e menus
- [ ] Bootstrap → menu sem exceções; "Continuar campanha" abre o briefing do próximo capítulo.
- [ ] Capítulos bloqueados mostram "[BLOQUEADO]"; concluídos mostram "(concluído)".
- [ ] Equipamento: no máximo 2; a escolha persiste entre sessões.
- [ ] Dificuldade alterna Investigação, Operação e Ruptura.
- [ ] F3 mostra FPS, memória e setor.

### 4.1b Cenário e visual
- [ ] Nenhum aviso `[EZ] Modelo ausente` ou `[EZ] Textura ausente` no Console (se aparecer, o glTFast não importou: confira o Package Manager).
- [ ] Renderer `EZ_UniversalRenderer` com a feature SSAO; URP com sombras de luzes adicionais ligadas.
- [ ] Nenhuma luz atravessa paredes (compare salas vizinhas com uma luz apagada pelo F3/Inspector).
- [ ] Texturas não esticadas; normal maps com relevo (sem aviso "Fix now" de normal map).
- [ ] Área externa: céu escuro, névoa, torre com baliza piscando, cerca sem saída.
- [ ] Props apoiados no chão/paredes (nada flutuando ou atravessado); portas abrem sem atravessar móveis.

### 4.2 Prólogo
- [ ] Spawn no pátio externo, de frente para a entrada; a porta da recepção abre; o telefone da central toca; rádio de apoio repete o briefing.
- [ ] Q alterna as 4 camadas; o DTMF toca 2-1-4 completo.
- [ ] Cabo no arquivo; G solta e o cabo pode ser recolhido de novo; com o cabo plugado, G desconecta.
- [ ] 214→9: contaminação, Linha Ocupada com frequência de retorno no HUD; girar com Q até o número e segurar libera; a extensão 201 fica vermelha e "Restaurar" também libera.
- [ ] Alavanca antes da hora volta à posição (sem travar a missão).
- [ ] 214→0 → segunda chamada → corredor → silhueta some ao ser encarada por ~1,2 s ou ao se aproximar → alavanca conclui.

### 4.3 Capítulo 1
- [ ] Três chamadas fantasmas em ordem variada (comparar duas seeds).
- [ ] Conectar a extensão certa ao tronco 0 → o medidor aponta o setor → o telefone do setor toca.
- [ ] A extensão contaminada (interferência fala da "nove") fica com LED vermelho.
- [ ] Isolar o circuito errado aumenta a contaminação; o certo trava a alavanca.
- [ ] A chamada final descreve a ordem visitada.

### 4.4 Capítulo 2
- [ ] Arquivo morto liberado; duas fitas reproduzem como chamadas.
- [ ] Com o gravador: R numa fita e R na outra mostra a comparação.
- [ ] Escala de plantão confirma a 212; 207→0 contamina; 212→0 faz a 207 tocar.
- [ ] Silhueta no arquivo morto; alavanca conclui.

### 4.5 Capítulo 3
- [ ] Spawn no corredor; porta do subsolo aberta; rampa transitável (sem travar o CharacterController).
- [ ] A 218 toca ao chegar aos túneis; sinais = números das caixas contaminadas.
- [ ] LEDs pulsam e zumbem nas contaminadas; o medidor confirma.
- [ ] Caixa limpa → contaminação e possível Linha Ocupada.
- [ ] Reflexo no vidro após o 2º isolamento; o 3º leva à chamada final.

### 4.6 Capítulo 4
- [ ] A previsão se cumpre ao abrir a porta da sala de energia (a luz apaga duas vezes).
- [ ] Desligar B → Linha Ocupada para quem desligou; religar corrige.
- [ ] A e C desligados com B ligado → chamada particular.
- [ ] A Assinante se aproxima pelo túnel só quando ninguém olha; encarar a faz recuar; o contato isola a linha e encerra a aproximação.

### 4.7 Capítulo 5 e finais
- [ ] Porta da central original destrancada; o telefone da operadora toca.
- [ ] Religar o tronco com circuitos contaminados contamina; depois de isolar os três, registra a restauração limpa.
- [ ] Ouvir as 4 camadas da gravação original (solo: Q) libera a marca.
- [ ] Os consoles mostram o requisito quando indisponíveis.
- [ ] Cada final mostra o epílogo; o save registra o final e a capa do caderno muda.

### 4.8 Interface e acessibilidade
- [ ] Legendas legíveis em 1080p; a escala funciona.
- [ ] "Legendas" desligado esconde eventos, não as camadas placeholder.
- [ ] Reduzir flashes (luzes, LEDs) e reduzir sustos (volume, versões atenuadas).
- [ ] Inverter eixo Y, FOV, sensibilidade e tela cheia aplicam na hora.
- [ ] Qualidade Baixa/Média/Alta muda a escala de renderização e as luzes.
- [ ] Caderno com rolagem e categorias; mapa (M) só com setores visitados.

### 4.9 Salvamento
- [ ] Save v1 (da fatia) migra: a missão concluída vira o prólogo concluído.
- [ ] Progresso, finais e marcas persistem; JSON válido em `~/.config/unity3d/.../extensao-zero-save.json`.

## 5. Testes de rede

Requer dois processos ou duas máquinas.

### 5.1 Host + 1 cliente na mesma máquina
- [ ] O host vê IPs locais, porta, chave e convite; "Copiar convite" funciona.
- [ ] O cliente cola o convite e entra; o lobby mostra 2 jogadores e as camadas de cada um.
- [ ] O host escolhe capítulo e dificuldade e inicia; ambos entram no mesmo mundo (mesma seed).
- [ ] Os corpos remotos são visíveis; o movimento sincroniza.
- [ ] Atender um telefone: cada jogador recebe 2 camadas; Q alterna só as suas.
- [ ] Portas, cabo, central, fitas, caixas e disjuntores aparecem iguais nos dois.
- [ ] A Linha Ocupada atinge quem errou; o outro vê a extensão vermelha e pode restaurar.
- [ ] Cliente sai: o host continua e as camadas são redistribuídas. Host sai: o cliente vê "sessão encerrada".

### 5.2 LAN
- [ ] "Procurar na rede local" lista a sessão em até 10 s.
- [ ] Conexão manual por IP local; firewall liberado (UDP 7777 e 47777).
- [ ] Três e quatro jogadores.

### 5.3 Segurança e erros
- [ ] Chave errada, sessão cheia, missão iniciada e versão incompatível são rejeitadas com motivo.
- [ ] Timeout mostra diagnóstico.
- [ ] Interação a mais de ~3,2 m é ignorada pelo host (log `[EZ] Interação ignorada`).

### 5.4 Internet
- [ ] IPv4 público + encaminhamento de porta.
- [ ] Documentar CGNAT/NAT restritivo; não prometer conexão. IPv6 no host: não suportado.

## 6. Testes de desempenho

- [ ] Build Linux inicia em < 15 s em SSD.
- [ ] 60 FPS em GPU de referência (Média, 1080p); 30 FPS mínimos em 4 GB (Baixa, 1080p).
- [ ] VRAM ≤ 3,2 GB (Média).
- [ ] Sem vazamento em 10 min; draw calls < 350; triângulos < 250 mil.
- [ ] No máximo 8 luzes ativas por setor (culling por setor).

Registrar hardware, driver, versão do Vulkan e resultados reais.

## 7. Regressão

- [ ] `tools/compile-check/run.sh` verde.
- [ ] Compilar na Unity sem erros ou warnings; EditMode + PlayMode verdes.
- [ ] Jogar o prólogo inteiro.
- [ ] README e documentação atualizados.
