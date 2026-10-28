#!/usr/bin/env bash
# NIGHT14 U3-27.6: a farm batch in the built Player (NIGHT14.md rule 12: at most 4 farm processes at once, each one
# waited for and closed by its PID, the wall time logged per batch; never beside a bench). One process per difficulty
# (or per seed range), each `SKYGRIND.exe -batchmode -nographics -skygrindFarm <runs> <difficulty> <seedStart> <out>`.
# usage: bash Tools/farm-batch.sh <outDir> <runs> <seedStart> <difficulty> [<difficulty> ...]   (at most 4 difficulties)
#   e.g. bash Tools/farm-batch.sh Reports/farm-n14 100 1 easy normal hard
# Prints each process's [farm] summary line and the batch's wall time; the JSONL files land in <outDir>.
set -u
cd "$(dirname "$0")/.." || exit 1
OUT="${1:?outDir}"
RUNS="${2:?runs}"
SEED="${3:?seedStart}"
shift 3
EXE="${FARM_EXE:-Builds/Stage1/SKYGRIND.exe}"
[ "$#" -ge 1 ] && [ "$#" -le 4 ] || { echo "1 to 4 difficulties (at most 4 farm processes)"; exit 2; }
mkdir -p "$OUT"
start=$(date +%s)
echo "farm batch $(date +%H:%M:%S): $RUNS runs x [$*] from seed $SEED, $EXE"
pids=()
for d in "$@"; do
  jsonl="$OUT/farm-$d-$SEED.jsonl"
  log="$OUT/farm-$d-$SEED.log"
  rm -f "$jsonl"
  "$EXE" -batchmode -nographics -skygrindFarm "$RUNS" "$d" "$SEED" "$(cygpath -w "$PWD/$jsonl")" -logFile "$(cygpath -w "$PWD/$log")" > /dev/null &
  pids+=($!)
  echo "  started $d: pid $!"
done
for i in "${!pids[@]}"; do
  wait "${pids[$i]}"
  echo "  pid ${pids[$i]} exited $?"
done
# Every farm process closed by PID (a Player that outlived its wait is killed by its own PID only).
for p in "${pids[@]}"; do kill -0 "$p" 2>/dev/null && kill "$p" 2>/dev/null && echo "  killed leftover pid $p"; done
end=$(date +%s)
for d in "$@"; do
  echo "  $d: $(grep -c . "$OUT/farm-$d-$SEED.jsonl" 2>/dev/null || echo 0) lines; $(grep -a "\[farm\]" "$OUT/farm-$d-$SEED.log" | tail -1)"
done
echo "farm batch wall time: $((end - start)) s (ended $(date +%H:%M:%S))"
