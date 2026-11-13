#!/usr/bin/env bash
# Verificação sem Unity: compila runtime/editor/testes contra assemblies de referência da
# Unity 2021.3 (NuGet) + stubs dos pacotes (NGO 2.14, Input System 1.20, URP 17.3) e executa
# os testes de lógica pura com NUnit. NÃO substitui a compilação na Unity 6000.3.28f1:
# APIs exclusivas do Unity 6 e o code-gen do Netcode só são validados no editor.
# Requisitos: dotnet SDK 8, curl, unzip. Uso: tools/compile-check/run.sh
set -euo pipefail
cd "$(dirname "$0")"
CACHE=.cache
mkdir -p "$CACHE"

fetch() { # id versão destino padrão
  local dir="$CACHE/$3"
  [ -d "$dir" ] && return
  curl -sSL -o "$CACHE/$3.nupkg" "https://api.nuget.org/v3-flatcontainer/$1/$2/$1.$2.nupkg"
  mkdir -p "$dir" && unzip -qo "$CACHE/$3.nupkg" "$4" -d "$dir"
  chmod -R u+rw "$dir"
}
fetch unityengine.modules 2021.3.33 mod 'lib/*'
fetch unity3d.sdk 2021.1.14.1 sdk 'lib/*'
fetch defiant_zombie.ksp.skeleton.unityengine.ui 1.11.2 ui '*.dll'
fetch nunit 3.13.3 nunit 'lib/netstandard2.0/*'

status=0
for p in "Runtime.csproj" "Runtime.csproj -p:ExtraDefines=UNITY_EDITOR" "Editor.csproj" "Tests.csproj"; do
  out=$(dotnet build $p -nologo -v q -clp:NoSummary 2>&1 || true)
  # NamedBuildTarget existe a partir da Unity 2021.2; a referência de editor é 2021.1.
  real=$(echo "$out" | grep -E ": error " | grep -v "NamedBuildTarget" | sed -E 's#^.*/Assets/_Project/##; s# \[.*##' | sort -u || true)
  if [ -n "$real" ]; then echo "=== $p: FALHOU"; echo "$real"; status=1; else echo "=== $p: OK"; fi
done

echo "=== Testes de lógica (NUnit)"
dotnet test LogicTests.csproj -nologo 2>&1 | grep -E "Passed!|Failed!|error|Failed " || status=1
exit $status
