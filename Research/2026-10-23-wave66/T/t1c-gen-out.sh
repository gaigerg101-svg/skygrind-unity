#!/usr/bin/env bash
# NIGHT66 T1c: gen-n65-foundry.cjs --out and the plant fix (T1a's critic finding 1), each step's output and the tracked files' status after.
#   bash Research/2026-10-23-wave66/T/t1c-gen-out.sh <scratch dir, Windows form C:/...>
cd "$(git rev-parse --show-toplevel)" || exit 1
SC="${1:?scratch dir (C:/...)}"
st() { echo "  git status --short Design/levels Assets/StreamingAssets/Design/levels Reports: [$(git status --short Design/levels Assets/StreamingAssets/Design/levels Reports | tr '\n' ';')]"; }
echo "== 1. the refusals (no file written)"
node Tools/night/gen-n65-foundry.cjs --slots=latest --plant-error 2>&1 | tail -1
node Tools/night/gen-n65-foundry.cjs --slots=latest --run --plant-error 2>&1 | tail -1
node Tools/night/gen-n65-foundry.cjs --slots=latest --run --plant-error --out=Design/levels/n65/x 2>&1 | tail -1
st
echo "== 2. the planted error into a scratch --out: refused before the finish; the tracked copies byte-identical"
node Tools/night/gen-n65-foundry.cjs --slots=latest --run --plant-error --out="$SC/plant" 2>&1 | grep -E "PLANTED|shipped|ERROR|levelgen n65|REFUSED"
echo "  exit ${PIPESTATUS[0]}"; st
echo "  in $SC/plant: $(ls "$SC/plant" | tr '\n' ' ')"
echo "== 3. a standalone --finish of that planted level refuses (its report lists ERROR rows)"
node Tools/night/gen-n65-foundry.cjs --finish --out="$SC/plant" 2>&1 | tail -1
echo "  exit ${PIPESTATUS[0]}; the level n65-foundry.json in $SC/plant: $([ -f "$SC/plant/n65-foundry.json" ] && echo present || echo 'absent (never finished)')"; st
echo "== 4. the latest assembly into a scratch --out: 0 ERROR, the round's level there, no tracked file touched"
node Tools/night/gen-n65-foundry.cjs --slots=latest --run --out="$SC/latest" 2>&1 | grep -E "shipped|ERROR|round's level|levelgen n65|REFUSED|finish:"
echo "  exit ${PIPESTATUS[0]}"; st
echo "  in $SC/latest: $(ls "$SC/latest" | tr '\n' ' ')"
