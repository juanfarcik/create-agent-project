#!/usr/bin/env bash
# Builds self-contained, single-file `create-agent-project` binaries for every
# supported platform. No .NET runtime required on the target machine —
# just download the binary for your OS and run it.
#
# Native AOT is deliberately NOT used here: it compiles, but YamlDotNet's
# reflection-based (de)serializer breaks at runtime under trimming
# (validate/architecture/optimize crash — see dotnet/README.md's "Native
# AOT does not work yet" note). Self-contained + single-file gives the
# same "no separate runtime install" outcome without that breakage.
#
# Usage:
#   ./scripts/publish.sh              # build for the current OS/arch only
#   ./scripts/publish.sh --all        # build for every supported RID
#   ./scripts/publish.sh <rid>        # build for one specific RID

set -euo pipefail
cd "$(dirname "$0")/.."

PROJECT="src/AgentProjectArchitect.Cli"
OUT_ROOT="publish"
ALL_RIDS=(osx-arm64 osx-x64 linux-x64 linux-arm64 win-x64)

detect_current_rid() {
  local os arch
  case "$(uname -s)" in
    Darwin) os="osx" ;;
    Linux) os="linux" ;;
    MINGW*|MSYS*|CYGWIN*) os="win" ;;
    *) echo "error: unrecognized OS $(uname -s)" >&2; exit 1 ;;
  esac
  case "$(uname -m)" in
    arm64|aarch64) arch="arm64" ;;
    x86_64|amd64) arch="x64" ;;
    *) echo "error: unrecognized architecture $(uname -m)" >&2; exit 1 ;;
  esac
  echo "${os}-${arch}"
}

publish_one() {
  local rid="$1"
  local out="${OUT_ROOT}/${rid}"
  echo "==> Publishing for ${rid}..."
  dotnet publish "$PROJECT" \
    -c Release \
    -r "$rid" \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -o "$out"

  local bin_name="AgentProjectArchitect.Cli"
  [ "${rid#win}" != "$rid" ] && bin_name="AgentProjectArchitect.Cli.exe"

  local archive="${OUT_ROOT}/create-agent-project-${rid}"
  if [ "${rid#win}" != "$rid" ]; then
    (cd "$out" && zip -q "../../${archive}.zip" "$bin_name")
    echo "    -> ${archive}.zip"
  else
    tar -czf "${archive}.tar.gz" -C "$out" "$bin_name"
    echo "    -> ${archive}.tar.gz"
  fi
}

mkdir -p "$OUT_ROOT"

if [ "${1:-}" = "--all" ]; then
  for rid in "${ALL_RIDS[@]}"; do publish_one "$rid"; done
elif [ -n "${1:-}" ]; then
  publish_one "$1"
else
  publish_one "$(detect_current_rid)"
fi

echo "Done. Binaries in ${OUT_ROOT}/"
