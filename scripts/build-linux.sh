#!/usr/bin/env bash
# Gera o build Linux x86_64 (Mono) em Builds/Linux/.
# Uso: ./scripts/build-linux.sh [--dev]
set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/_unity.sh"

METHOD="ExtensaoZero.EditorTools.BuildScript.BuildLinux"
LOG="$ROOT/Logs/build-linux.log"
if [[ "${1:-}" == "--dev" ]]; then
  METHOD="ExtensaoZero.EditorTools.BuildScript.BuildLinuxDevelopment"
  LOG="$ROOT/Logs/build-linux-dev.log"
fi

echo "Unity: $UNITY_BIN"
echo "Log:   $LOG"
"$UNITY_BIN" -batchmode -nographics -quit \
  -projectPath "$ROOT" \
  -buildTarget Linux64 \
  -executeMethod "$METHOD" \
  -logFile "$LOG"

echo "Build concluído em $ROOT/Builds/Linux/"
