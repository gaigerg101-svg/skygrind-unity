#!/usr/bin/env bash
# NIGHT66 T1c: the decoration checker's planted case (slate T1c line 2): b1's room file plus crate A (a 1 m crate in a free corner) and
# crate B (a 0.3 m step on the walk line ahead of the spawn), assembled into a scratch level (gen --room-file, --out: nothing tracked is
# written), skip v2 run on it (10 seeds: the touch table), then n66-deco.cjs against the t0 sha: A must FAIL, B must PASS.
#   bash Research/2026-10-23-wave66/T/t1c-deco-plant.sh <scratch dir, C:/...> [t0 sha]
cd "$(git rev-parse --show-toplevel)" || exit 1
SC="${1:?scratch dir}"; T0="${2:-e37083cf}"
mkdir -p "$SC"
echo "== 1. the planted room file"
node Tools/night/n66-deco.cjs --plant b1 "$SC/b1-planted.json" || exit 1
echo "== 2. the scratch level (gen --room-file --out)"
node Tools/night/gen-n65-foundry.cjs --slots=latest --room-file="b1:$SC/b1-planted.json" --run --out="$SC/deco-plant" 2>&1 | grep -E "room-file|shipped|levelgen --walk|round's level|REFUSED"
echo "  tracked level files: [$(git status --short Design/levels Assets/StreamingAssets/Design/levels Reports | tr '\n' ';')]"
echo "== 3. skip v2 on it (b1, 10 seeds)"
dotnet run --project Tools/SimHarness -c Release --no-build -- n65verb skip --level "$SC/deco-plant/n65-foundry.json" --room b1 --seeds 10 > "$SC/deco-plant-skip.txt" 2>&1
grep -E "n65verb skip: M1|PLANTED|box\((-?[0-9.]+,){2}-?[0-9.]+\.\.(-?[0-9.]+,){2}-?[0-9.]+\) \| seeds" "$SC/deco-plant-skip.txt" | grep -E "M1|crate|seeds" | head -3
grep -E "^n65verb skip touch: room b1" "$SC/deco-plant-skip.txt" > "$SC/deco-plant-touch.txt"; grep -E "^n65verb skip launchkill: room b1" "$SC/deco-plant-skip.txt" >> "$SC/deco-plant-touch.txt"
cp "$SC/deco-plant-touch.txt" Research/2026-10-23-wave66/T/t1c-deco-plant-touch.txt
echo "  the touch table and launch kills: Research/2026-10-23-wave66/T/t1c-deco-plant-touch.txt ($(wc -l < "$SC/deco-plant-touch.txt") lines)"
echo "== 4. n66-deco.cjs --base $T0 b1 --file <planted> --touches <the table>"
node Tools/night/n66-deco.cjs --base "$T0" b1 --file "$SC/b1-planted.json" --touches Research/2026-10-23-wave66/T/t1c-deco-plant-touch.txt
echo "  exit $? (1: a piece FAILs, as planted: crate A)"
