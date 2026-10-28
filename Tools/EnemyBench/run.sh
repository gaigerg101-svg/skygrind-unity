#!/usr/bin/env bash
# One pass of the enemy bench in the built Player, set up like the web bench (skygrind port-export/bench): every
# variant x N in a fresh Player process, 1920 x 1080 windowed, vsync and frame cap off (set by the bench), 10 s warm-up,
# 30 s measured. The Windows power plan is polled every 2 s during each run and stored in its record ("powerPlans"),
# so a run that was not on the expected plan the whole time can be told apart. Run one pass at a time, nothing else.
# usage: Tools/EnemyBench/run.sh <Player exe> <pass tag> ["variants"] ["counts"]
set -u
EXE="$1"
TAG="$2"
VARIANTS="${3:-closer-web closer-unity plain}"
COUNTS="${4:-24 40 100 200}"
HERE="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$HERE/../.." && pwd)"
OUT="${BENCH_OUT:-$HERE/results}" # BENCH_OUT, BENCH_EXTRA: another results folder and extra Player arguments
RAW="$ROOT/Builds/bench-raw" # every frame and step time (large, not committed)
LOGS="$ROOT/Builds/bench-logs" # the Player logs (the GPU line is in each)
mkdir -p "$OUT" "$RAW" "$LOGS"
plan() { echo "$(powercfg -getactivescheme | tr -d '\r' | sed -n 's/.*(\(.*\)).*/\1/p')"; } # "-" switch: MSYS rewrites "/x" as a path
for v in $VARIANTS; do
  for n in $COUNTS; do
    name="$TAG-$v-$n"
    rm -f "$OUT/$name.json"
    plans="$LOGS/$name.plans"
    plan > "$plans"
    ( while true; do sleep 2; plan >> "$plans"; done ) &
    poller=$!
    "$EXE" -bench -benchVariants "$v" -benchCounts "$n" -benchWarm 10 -benchMeasure 30 -benchTag "$TAG" ${BENCH_EXTRA:-} \
      -benchOut "$(cygpath -w "$OUT/$name.json")" -benchRaw "$(cygpath -w "$RAW/$name.json")" \
      -screen-width 1920 -screen-height 1080 -screen-fullscreen 0 -logFile "$(cygpath -w "$LOGS/$name.log")" > /dev/null
    code=$?
    kill $poller 2> /dev/null
    wait $poller 2> /dev/null
    plan >> "$plans"
    seen="$(sort "$plans" | uniq -c | awk '{print $2 " x" $1}' | paste -sd, -)"
    [ -f "$OUT/$name.json" ] && node -e 'const fs=require("fs");const [f,p]=process.argv.slice(1);const a=JSON.parse(fs.readFileSync(f,"utf8"));for(const r of a)r.powerPlans=p;fs.writeFileSync(f,JSON.stringify(a,null,1)+"\n");' "$OUT/$name.json" "$seen"
    echo "$name: exit $code, $(grep -o '"fps": [0-9.]*' "$OUT/$name.json" 2> /dev/null | head -1), plans $seen, $(grep -o 'GPU: [^(]*' "$LOGS/$name.log" | head -1)"
  done
done
