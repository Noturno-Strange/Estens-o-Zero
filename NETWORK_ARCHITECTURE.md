# EXTENSÃO ZERO — NETWORK ARCHITECTURE

## 1. Requisito

Partidas cooperativas para 1–4 jogadores **sem servidor central**. Arquitetura peer-hosted: um jogador é anfitrião e também jogador; os demais conectam diretamente a ele por UDP.

Proibido como dependência obrigatória: Relay, Lobby, Matchmaker, servidores dedicados, retransmissão, banco remoto, APIs externas, contas, Steam P2P.

## 2. Stack

- Netcode for GameObjects 2.14.0.
- Unity Transport 2.6.0.
- Host-cliente, não mesh.
- Host autoritativo para estado compartilhado.
- Cliente envia solicitações; host valida e replica.

## 3. Fluxo de conexão

### Criar partida

1. O jogador escolhe **Criar sessão**. A chave de sessão é gerada com `RandomNumberGenerator` (6 caracteres, sem 0/O/1/I) e pode ser editada.
2. `NetworkBootstrap` inicia o host na porta configurada (padrão UDP 7777).
3. O lobby mostra os IPs locais (IPv4), a porta, a chave, o convite `EZ1:` (botão **Copiar convite**) e os avisos: o IP do host é visível, CGNAT pode impedir conexão e a sessão termina se o host sair.
4. O host escolhe capítulo e dificuldade e inicia a missão.

### Entrar

1. O jogador cola o convite, informa IP/porta/chave ou escolhe uma sessão encontrada na LAN (a chave continua sendo pedida).
2. `InviteCodec` valida o prefixo, a versão, o IP, a porta e a chave.
3. O cliente conecta via Unity Transport (10 tentativas de 1 s; diagnóstico após 12 s).
4. O payload de aprovação é `"EZ"` + versão do protocolo + hash FNV-1a da chave.
5. O host valida versão, chave, vagas (máximo 4) e se a missão ainda não começou, e devolve um motivo legível em caso de recusa.
6. O jogador entra no lobby e escolhe o próprio equipamento.

### Formato do convite (implementado)

```
EZ1:<base64url(JSON)>
JSON = {"v":1,"ip":"203.0.113.10","port":7777,"key":"A1B2C3"}
```

O convite é só uma representação dos dados de conexão. **Não existe descoberta automática por código curto.** Para jogar pela internet, o endereço público e a porta precisam estar acessíveis.

## 4. Descoberta LAN

- O host envia o beacon UDP a cada 2 s na porta 47777, em broadcast (`LanDiscovery.StartBeacon`).
- Beacon: `EZLAN1|name|port|playerCount|missionStarted|fingerprint`. O fingerprint é um identificador aleatório da sessão; **a chave nunca é anunciada**.
- O cliente escuta por 10 s em uma thread separada; os resultados são entregues na thread principal e listados no painel "Entrar".
- A descoberta não é requisito: a conexão manual por IP local está sempre disponível.
- Limitações: isolamento de clientes Wi-Fi, firewalls e roteadores podem bloquear broadcast. O painel informa quando nenhuma sessão é encontrada.

## 5. Internet e NAT

Permitido:

- IPv4 público com encaminhamento manual da porta UDP.
- IPv6 global: o convite aceita endereço IPv6, mas o host desta versão escuta só em IPv4 (`0.0.0.0`). **IPv6 não é suportado ainda.**
- UPnP IGD/NAT-PMP/PCP **opcional** e com consentimento (não implementado na fatia).
- Porta configurável manualmente.

Limitações reais:

- CGNAT e NAT restritivo podem impedir conexão direta iniciada de fora.
- Não há Relay/STUN escondido. O jogo informa a limitação e orienta o encaminhamento de porta.
- Não prometemos conexão universal.

Diagnóstico exibido ao jogador: endereço local, porta, se o host está ouvindo, tempo de tentativa, motivo da falha (timeout, chave inválida, sessão cheia, missão iniciada).

## 6. Aprovação e segurança

- `ConnectionApproval = true`; o host também passa pela aprovação com o próprio payload.
- Payload: `EZ` + versão do protocolo (2) + hash da chave. A chave em texto claro nunca trafega.
- Rejeições com motivo: payload inválido, versão incompatível, chave incorreta, sessão cheia (4) e missão iniciada.
- O host valida cada interação: distância ≤ 3,2 m (+ tolerância de altura dos olhos), cooldown de 0,2 s por jogador, limite de 30 mensagens/s por jogador e `CanInteract`.
- Clientes não alteram variáveis de estado; só pedem. Evidências reportadas por clientes só são aceitas se existirem no catálogo.
- Nenhum IP é enviado a serviços externos; nenhum histórico de IP é mantido.
- A chave de sessão **não é criptografia**. As opções de segurança/DTLS do Unity Transport não estão habilitadas, e isso está documentado, não inventado.
- O IP público do host é visível para quem se conecta; o lobby avisa.

## 7. Sincronização

| Estado | Mecanismo | Autoridade |
|---|---|---|
| Posição/rotação dos jogadores | dono envia posição (15 Hz) → host valida limites → NetworkVariable → interpolação | host valida (ADR-016) |
| Interações (portas, telefones, cabo, central, fitas, caixas, disjuntores, consoles) | ServerRpc com ID → validação → ClientRpc "executar" em todos os peers, na mesma ordem | host |
| Soltar cabo | ServerRpc → ClientRpc | host |
| Telefones tocando | ClientRpc (extensão + id da chamada) | host |
| Chamadas | atender é uma interação replicada; cada peer toca as suas camadas localmente | host |
| Contaminação | NetworkVariable float (2 Hz) | host |
| Comandos de mundo (portas, luzes, itens, manifestação) | ClientRpc com string de comando | host |
| Objetivo, legendas de missão, evidências | ClientRpc | host |
| Eventos de tensão | ClientRpc com id do evento; efeito local, relativo a cada jogador | host |
| Linha Ocupada | ClientRpc (alvo, duração, frequência, extensão); a recuperação volta por ServerRpc | host |
| Camadas ouvidas | ServerRpc (máscara); o host une as da equipe (requisito do final Restaurar) | host |
| Zonas (gatilhos) | o dono detecta e reporta por ServerRpc | host |
| Início e conclusão | ClientRpc (capítulo, seed, dificuldade / resumo, final) | host |

O áudio **não** é transmitido. Cada cliente toca localmente o clip da sua camada.

## 8. Distribuição de camadas

`LayerDistributor.Distribute(playerCount, playerIndex)`:

| Jogadores | Índice 0 | Índice 1 | Índice 2 | Índice 3 |
|---|---|---|---|---|
| 1 | Voz+Ambiente+Sinais+Interferência | — | — | — |
| 2 | Voz+Ambiente | Sinais+Interferência | — | — |
| 3 | Voz+Ambiente | Sinais | Interferência | — |
| 4 | Voz | Ambiente | Sinais | Interferência |

O índice é a posição do cliente na lista de jogadores ordenada por `ClientId` (o host é o 0). A mesma lista existe em todos os peers. O jogador alterna entre as camadas atribuídas com Q. Se alguém entra ou sai durante uma chamada, as camadas são recalculadas.

## 9. Desconexão

- Se um cliente sair, o host remove o jogador, mantém a sessão e redistribui as camadas da chamada ativa.
- Se o host sair, os clientes veem "sessão encerrada" com o motivo e voltam ao menu. Não há migração de host.
- Reconexão automática não está implementada; antes de a missão começar é possível conectar de novo manualmente.
- Mensagens para timeout (com diagnóstico de IP, porta, firewall e CGNAT), chave inválida, sessão cheia, versão incompatível e missão em andamento.

## 10. Testes de rede

**Nenhum teste com dois processos reais foi executado.** Os testes automatizados cobrem o convite, a chave, o beacon e a aprovação (`NetworkProtocolTests`). O código de rede compila contra stubs com as assinaturas do NGO 2.14; o code-gen de RPC do Netcode só é validado na Unity. O plano manual está em [TEST_PLAN.md](TEST_PLAN.md).

## 11. Privacidade

- Sem contas, telemetria ou analytics.
- Sem envio de IP para terceiros.
- Sem armazenamento de histórico de IP.
- Convite e chave ficam apenas na memória e no clipboard local.
- Configurações e progresso salvos localmente.
