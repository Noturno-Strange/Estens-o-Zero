#!/usr/bin/env bash
# Localiza o editor Unity da versão fixada em ProjectSettings/ProjectVersion.txt.
# Pode ser sobrescrito com a variável de ambiente UNITY=/caminho/para/Editor/Unity.

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_VERSION="$(sed -n 's/^m_EditorVersion: //p' "$ROOT/ProjectSettings/ProjectVersion.txt")"

find_unity() {
  if [[ -n "${UNITY:-}" ]]; then
    echo "$UNITY"
    return
  fi
  local candidates=(
    "$HOME/Unity/Hub/Editor/$UNITY_VERSION/Editor/Unity"
    "$HOME/.local/share/UnityHub/Editor/$UNITY_VERSION/Editor/Unity"
    "/opt/unity/Editor/$UNITY_VERSION/Editor/Unity"
  )
  for c in "${candidates[@]}"; do
    if [[ -x "$c" ]]; then
      echo "$c"
      return
    fi
  done
}

UNITY_BIN="$(find_unity)"
if [[ -z "$UNITY_BIN" || ! -x "$UNITY_BIN" ]]; then
  echo "Unity $UNITY_VERSION não encontrada. Defina UNITY=/caminho/para/Editor/Unity." >&2
  exit 1
fi

mkdir -p "$ROOT/Logs"
