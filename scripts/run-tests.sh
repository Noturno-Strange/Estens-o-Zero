#!/usr/bin/env bash
# Executa os testes do Unity Test Framework em batch mode.
# Uso: ./scripts/run-tests.sh [editmode|playmode|all]   (padrão: editmode)
# Os testes PlayMode carregam a cena Bootstrap e precisam de GPU/servidor gráfico.
set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/_unity.sh"

MODE="${1:-editmode}"
case "$MODE" in
  editmode) PLATFORMS=(EditMode) ;;
  playmode) PLATFORMS=(PlayMode) ;;
  all)      PLATFORMS=(EditMode PlayMode) ;;
  *) echo "Modo inválido: $MODE (use editmode, playmode ou all)" >&2; exit 2 ;;
esac

STATUS=0
for platform in "${PLATFORMS[@]}"; do
  results="$ROOT/Logs/test-results-$platform.xml"
  log="$ROOT/Logs/tests-$platform.log"
  extra=()
  [[ "$platform" == "EditMode" ]] && extra=(-nographics)
  echo "== $platform =="
  # -runTests encerra o editor sozinho; não combinar com -quit.
  if "$UNITY_BIN" -batchmode "${extra[@]}" \
      -projectPath "$ROOT" \
      -runTests -testPlatform "$platform" \
      -testResults "$results" \
      -logFile "$log"; then
    echo "$platform: OK ($results)"
  else
    echo "$platform: FALHOU — veja $results e $log" >&2
    STATUS=1
  fi
done
exit $STATUS
