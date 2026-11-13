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

1. Jogador escolhe **Criar partida**.
2. `NetworkBootstrap` inicia o host na porta configurada (padrão UDP 7777).
3. O jogo exibe os endereços locais (LAN) e, se disponível, o endereço público informado/configurado.
4. Gera uma chave de sessão aleatória (6 caracteres alfanuméricos) e um convite.
5. O anfitrião compartilha o convite por fora do jogo.

### Entrar

1. Jogador cola o convite ou informa IP e porta.
2. `SessionCodec` interpreta o convite.
3. Cliente conecta via Unity Transport.
4. O payload de aprovação contém a chave de sessão.
5. Host valida a chave, o limite de jogadores e se a missão ainda não começou.
6. Entra no lobby.

### Formato do convite

```
EZ1:<base64url(JSON)>
JSON = {"v":1,"ip":"203.0.113.10","port":7777,"key":"A1B2C3"}
```

O convite é apenas uma representação dos dados de conexão. **Não existe descoberta automática por código curto.** Para internet, o endereço público e a porta precisam estar acessíveis.

## 4. Descoberta LAN

- Host envia beacon UDP a cada 2 s na porta 47777, em broadcast.
- Beacon: `EZLAN1|name|port|playerCount|missionStarted|fingerprint`.
- Cliente escuta em thread separada e lista sessões por 10 s.
- A descoberta não é requisito: conexão manual por IP local sempre disponível.
- Limitações: isolamento de clientes Wi-Fi, firewalls e roteadores podem bloquear broadcast. O lobby informa quando nenhuma sessão é encontrada.

## 5. Internet e NAT

Permitido:

- IPv4 público com encaminhamento manual da porta UDP.
- IPv6 global quando disponível.
- UPnP IGD/NAT-PMP/PCP **opcional** e com consentimento (não implementado na fatia).
- Porta configurável manualmente.

Limitações reais:

- CGNAT e NAT restritivo podem impedir conexão direta iniciada de fora.
- Não há Relay/STUN escondido. O jogo informa a limitação e orienta o encaminhamento de porta.
- Não prometemos conexão universal.

Diagnóstico exibido ao jogador: endereço local, porta, se o host está ouvindo, tempo de tentativa, motivo da falha (timeout, chave inválida, sessão cheia, missão iniciada).

## 6. Aprovação e segurança

- `ConnectionApproval = true`.
- Payload: chave de sessão + versão do protocolo.
- Chave aleatória de 6 caracteres, gerada com `RandomNumberGenerator`.
- Limite de 4 jogadores.
- Entrada apenas antes do início da missão.
- Host valida distância para interações (≤ 3,2 m) e cooldown de 0,2 s.
- Clientes não podem alterar diretamente variáveis de estado; apenas solicitar.
- Limite de frequência de mensagens no host.
- Nenhum IP é enviado a serviços externos; nenhum histórico de IP é mantido.
- A chave de sessão **não é criptografia**. Unity Transport oferece opções de segurança/DTLS; nesta fatia não estão habilitadas e isso é documentado, não inventado.
- O IP público do host é visível para quem se conecta; o lobby avisa isso claramente.

## 7. Sincronização

| Estado | Mecanismo | Autoridade |
|---|---|---|
| Posição/rotação dos jogadores | input do cliente → ServerRpc → CharacterController no host → NetworkTransform | host |
| Interações | ServerRpc com ID do interactable | host |
| Telefones (tocando/atendido) | NetworkVariable + ClientRpc | host |
| Extensões/cabos | NetworkVariable (bitmask) | host |
| Contaminação | NetworkVariable float | host |
| Chamadas | ClientRpc com callId + tempo de rede + respondido por | host |
| Distribuição de camadas | determinística por cliente/índice | host define a chamada |
| Eventos de tensão | ClientRpc com tipo/posição/intensidade | host |
| Estado da Assinante | NetworkVariable (enum) | host |
| Objetivos/fim | NetworkVariable (etapa) + ClientRpc | host |

Áudio **não** é transmitido. Cada cliente executa o clip local correspondente à sua camada.

## 8. Distribuição de camadas

`LayerDistributor.Distribute(playerCount, playerIndex)`:

| Jogadores | Índice 0 | Índice 1 | Índice 2 | Índice 3 |
|---|---|---|---|---|
| 1 | Voz+Ambiente+Sinais+Interferência | — | — | — |
| 2 | Voz+Ambiente | Sinais+Interferência | — | — |
| 3 | Voz+Ambiente | Sinais | Interferência | — |
| 4 | Voz | Ambiente | Sinais | Interferência |

O índice é a posição do cliente na lista ordenada de `ClientId`s (host primeiro). A mesma lista é visível em todos os peers. O jogador pode alternar entre as camadas atribuídas com Q.

## 9. Desconexão

- Se um cliente sair, o host remove o jogador, mantém a sessão e redistribui as camadas da chamada ativa (ou encerra a chamada).
- Se o host sair, a sessão termina de forma organizada; não há migração de host na primeira versão.
- Reconexão automática não implementada na fatia; o lobby permite nova conexão manual antes da missão.
- Mensagens claras para timeout, chave inválida, sessão cheia, versão incompatível e missão em andamento.

## 10. Testes de rede

Não foram executados dois processos reais neste ambiente. O código compila e a lógica pura é testada. O plano manual está em [TEST_PLAN.md](TEST_PLAN.md):

- host + 1 cliente na mesma máquina (portas diferentes);
- host + 1 cliente em duas máquinas na LAN;
- host + 2 e 3 clientes;
- chave errada, sessão cheia, entrada após início;
- desconexão de cliente e host;
- distribuição de camadas e sincronização de chamada;
- latência simulada (tc/netem) e perda de pacotes.

## 11. Privacidade

- Sem contas, telemetria ou analytics.
- Sem envio de IP para terceiros.
- Sem armazenamento de histórico de IP.
- Convite e chave ficam apenas na memória e no clipboard local.
- Configurações e progresso salvos localmente.
