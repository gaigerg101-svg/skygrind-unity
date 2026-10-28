#!/usr/bin/env bash
# NIGHT56 (the lead; P0-gate55 / NIGHT55 P1-wickWindow): THE WICK's bot numbers on HEAD, wick55 off and on, the skilled pilot (n47human) and
# the casual pilot (n55wick), 40 seeds each. Bot numbers, not a person's.
#   bash Tools/night/n56-wick-bot.sh <out dir>
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
OUT="${1:-Logs/n56/wick}"; mkdir -p "$OUT"
dotnet build Tools/SimHarness -c Release -v q -nologo > "$OUT/build.txt" 2>&1 || { echo "build FAILED"; exit 1; }
run() { dotnet run --project Tools/SimHarness -c Release --no-build -- "$@"; }
run n47human --boss wick --seeds 40 > "$OUT/skilled-off.txt" 2>&1
run n47human --boss wick --seeds 40 --rows wick55 > "$OUT/skilled-on.txt" 2>&1
run n55wick --seeds 40 > "$OUT/casual-off.txt" 2>&1
run n55wick --seeds 40 --rows wick55 > "$OUT/casual-on.txt" 2>&1
for f in skilled-off skilled-on casual-off casual-on; do echo "== $f"; grep -E "^\| wick|killed|window|hot" "$OUT/$f.txt" | cut -c1-400 | head -6; done
