#!/usr/bin/env bash
# NIGHT56 (the lead): the lead's own bot / checker numbers on HEAD for a gate (the critic reads the lead's runs, not a builder's):
# n56-room.py table + planted, the identity table, n46flow on the n52 levels x10 (Harness.N46.Flow.cs, with G's style column), the WYRM
# (n47human 40 seeds on n52-kiln) with the given rows. Bot numbers, not a person's.
#   bash Tools/night/n56-lead-bot.sh <out dir> [--rows a,b]   (rows forced on for the harness runs; default none)
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
OUT="${1:?out dir}"; shift; mkdir -p "$OUT"
ROWS=(); [ "$1" = "--rows" ] && ROWS=(--rows "$2")
echo "[n56-lead-bot] start $(date +%T) at $(git rev-parse --short HEAD) rows ${ROWS[*]:-none}"
python -I Tools/night/n56-room.py table > "$OUT/room-table.txt" 2>&1
python -I Tools/night/n56-room.py planted > "$OUT/room-planted.txt" 2>&1; echo "[n56-lead-bot] planted exit $?"
python -I Tools/night/n53-identity.py table kiln:kb2,kb2c,kb3 foundry:a3,a5,b1 iceline:g2,g3,g4,g5,g7,g8 spire:v2,v5,v6 span:p1,p3,p5 > "$OUT/identity.txt" 2>&1
echo "[n56-lead-bot] identity FAIL rooms: $(grep -c 'FAIL I' "$OUT/identity.txt")"
dotnet build Tools/SimHarness -c Release -v q -nologo > "$OUT/build.txt" 2>&1 || { echo "[n56-lead-bot] build FAILED"; exit 1; }
dotnet run --project Tools/SimHarness -c Release --no-build -- n46flow --level n52-foundry,n52-iceline,n52-kiln,n52-span,n52-spire --seeds 10 "${ROWS[@]}" --out "$OUT/flow.txt" > "$OUT/flow.out" 2>&1
echo "[n56-lead-bot] flow exit $? $(date +%T)"
grep SUMMARY "$OUT/flow.out" | cut -c1-260
echo "[n56-lead-bot] done $(date +%T)"
