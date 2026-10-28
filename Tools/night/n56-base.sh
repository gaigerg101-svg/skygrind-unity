#!/usr/bin/env bash
# NIGHT56 A0 baselines at u3-n55 (+ the forge's docs), run in a detached worktree so the pick in the main tree cannot leak in.
# Usage: bash Tools/night/n56-base.sh <worktree dir> <out dir>
set -u
WT="${1:-/c/Users/gaige/dev/SKYGRIND-n56base}"
OUT="${2:-/c/Users/gaige/dev/SKYGRIND-unity/Logs/n56}"
mkdir -p "$OUT"
cd "$WT" || exit 1
echo "[n56-base] start $(date) at $(git rev-parse --short HEAD)"
dotnet build Tools/SimHarness -c Release -v q -nologo > "$OUT/base-build.txt" 2>&1 || { echo "[n56-base] build FAILED"; tail -20 "$OUT/base-build.txt"; exit 1; }
echo "[n56-base] flow start $(date)"
dotnet run --project Tools/SimHarness -c Release --no-build -- n46flow --level n52-foundry,n52-iceline,n52-kiln,n52-span,n52-spire --seeds 10 --out "$OUT/flow-base.txt" > "$OUT/flow-base.out" 2>&1
echo "[n56-base] flow exit $? $(date)"
dotnet run --project Tools/SimHarness -c Release --no-build -- n47human --boss wyrm --level n52-kiln --seeds 40 > "$OUT/wyrm-base.txt" 2>&1
echo "[n56-base] wyrm exit $? $(date)"
dotnet run --project Tools/SimHarness -c Release --no-build -- fingerprint > "$OUT/fingerprint-base.txt" 2>&1
echo "[n56-base] fingerprint exit $? $(date)"
tail -3 "$OUT/fingerprint-base.txt"
echo "[n56-base] done $(date)"
