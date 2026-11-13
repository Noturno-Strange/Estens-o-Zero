# EXTENSÃO ZERO — LINUX BUILD

## 1. Requisitos

- Linux x86_64 (testado no Ubuntu 26.04).
- Unity 6000.3.28f1 com **Linux Build Support (Mono)**.
- Nenhuma dependência de runtime além das bibliotecas padrão do sistema.
- GPU com Vulkan ou OpenGL 4.5; URP usa Vulkan por padrão no Linux.

Verificar módulos:

```bash
~/.local/bin/unity install-modules -e 6000.3.28f1 -l
```

## 2. Build automatizado

```bash
./scripts/build-linux.sh
```

O script procura a Unity da versão fixada (ou usa a variável `UNITY=/caminho/para/Editor/Unity`) e a chama em batch mode:

```bash
"$UNITY" -batchmode -nographics -quit \
  -projectPath "$ROOT" \
  -buildTarget Linux64 \
  -executeMethod ExtensaoZero.EditorTools.BuildScript.BuildLinux \
  -logFile "$ROOT/Logs/build-linux.log"
```

Saída:

```
Builds/Linux/ExtensaoZero.x86_64
Builds/Linux/ExtensaoZero_Data/
```

## 3. Build pelo editor

1. Abra o projeto.
2. Menu **Extensao Zero → Build Linux** (ou **Build Linux (Development)**).
3. A saída vai para `Builds/Linux/`.

## 4. Executar

```bash
cd Builds/Linux
./ExtensaoZero.x86_64
```

Logs:

```bash
./ExtensaoZero.x86_64 -logFile /tmp/extensao-zero.log
```

Opções úteis:

```bash
-screen-width 1920 -screen-height 1080 -screen-fullscreen 0
-force-vulkan
-force-glcore
```

## 5. Execução em modo headless (testes)

```bash
./ExtensaoZero.x86_64 -batchmode -nographics -logFile /tmp/ez.log -quit
```

A cena Bootstrap cria o mundo; para testes automatizados de lógica, use o Unity Test Framework.

## 6. Empacotamento

Distribuição sugerida:

```
ExtensaoZero-linux-x86_64/
  ExtensaoZero.x86_64
  ExtensaoZero_Data/
  UnityPlayer.so
  README.txt
  LICENSES/
```

Compactar com `tar -czf` ou gerar AppImage (ferramenta externa opcional). Não incluir `Library/`, `Logs/` ou arquivos de desenvolvimento.

## 7. Porta e firewall

O multiplayer usa UDP. Porta padrão do jogo: **7777** (configurável). A descoberta LAN usa **UDP 47777** (broadcast).

- Liberar no firewall local:
  ```bash
  sudo ufw allow 7777/udp
  sudo ufw allow 47777/udp
  ```
- Roteador: encaminhar UDP 7777 para o IP local do host.
- IPv6: liberar a porta no firewall; não há NAT.
- Teste de alcance: `nc -u -v <ip> 7777` (não prova que o jogo está ouvindo, mas testa a rota).
- CGNAT/NAT restritivo: a conexão direta pode ser impossível; o jogo informa e não usa relay.

## 8. Solução de problemas

| Sintoma | Causa provável | Ação |
|---|---|---|
| Tela preta no editor | cena Bootstrap não aberta | abrir `Assets/_Project/Scenes/Bootstrap.unity` |
| Sem áudio | dispositivo padrão do sistema | verificar `pactl info`; testar outro dispositivo |
| Build falha em `BuildScript` | módulo Linux ausente | instalar Linux Build Support |
| Erro de licença | licença não ativada | abrir Unity Hub e ativar Unity Personal |
| Shader rosa | URP não configurado | rodar `Extensao Zero → Setup Project` |
| Cliente não conecta | porta/firewall/NAT | verificar IP, porta, `ufw`, encaminhamento |
| FPS baixo | qualidade/sombras | reduzir para Baixa, desligar sombras |

## 9. Reprodutibilidade

- Versão da Unity fixada em `ProjectSettings/ProjectVersion.txt`.
- Versões dos pacotes fixadas em `Packages/manifest.json` e `Packages/packages-lock.json`.
- Setup automático no primeiro import.
- Scripts de build e teste na pasta `scripts/`.
- Verificação sem Unity (compilação + testes de lógica): `tools/compile-check/run.sh`.
- Nenhum segredo no repositório.
