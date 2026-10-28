#!/usr/bin/env bash
# NIGHT70's per-room chain (FORGE70; the lead copies it to Logs/n70/room-chain.sh at the bearings; FORGE69's with three changes: n70 names,
# the 20-seed throw chart (Tools/night/n70-throws.sh: M3 under DECIDE70 D-3) and the launch count from the skip file). It is queued as a
# job of Tools/night/n70-runner.sh (one dotnet job at a time in the main tree), never run beside one. For each room given, in series: a round
# with ONLY that room new in its level (SLOTS=<id>), its fresh set on seeds 21-30 (--room-file, the GATE pack's --fresh), its patch share,
# its throw chart, and a summary line. It does NOT run the GATE pack (that needs the round's level review). The round's folder is
# r<n>-<level>; a failed round cannot re-run under the same N (n70-round.sh refuses an existing folder): a re-run is a NEW N.
#   job line: bash Logs/n70/room-chain.sh <n> <id> [<id> ...]      (Logs/n70/room-chain-<n>.done holds its exit code, written last)
cd "$(git rev-parse --show-toplevel)" || exit 1
export MSBUILDDISABLENODEREUSE=1
N="$1"; shift
[ -n "$N" ] && [ "$#" -ge 1 ] || { echo "usage: room-chain.sh <n> <id> [<id> ...]"; exit 2; }
say() { echo "$(date '+%T') $*" | tee -a "Logs/n70/room-chain-$N.txt"; }
slate_of() { node -p "require('./Logs/n70/slate.json').slices.find(s=>s.id==='$1').$2"; }
rc=0
say "room-chain $N start, sha $(git rev-parse --short HEAD), rooms: $*"
dotnet build Tools/SimHarness -c Release -nologo -v q > "Logs/n70/room-chain-$N-build.txt" 2>&1 || { say "harness build FAILED"; echo 1 > "Logs/n70/room-chain-$N.done"; exit 1; }
say "harness build ok"
for id in "$@"; do
  L=$(slate_of "$id" level)
  D="Logs/n70/r$N-$L"
  L67=$L ROOMS=$id SLOTS=$id bash Tools/night/n70-round.sh "r$N-$L" > "Logs/n70/r$N-$L-run.txt" 2>&1 || { rc=1; say "round $id FAILED (see Logs/n70/r$N-$L-run.txt)"; continue; }
  say "round $id ($L, alone): $(grep -E 'done in' "Logs/n70/r$N-$L-run.txt" | tail -1 | cut -c1-70)"
  line=$(grep -E "^n65verb skip: M2 VERB" "$D/verbs-skip.txt" 2>/dev/null | grep "room $id," | head -1)
  say "M2 $id: ${line:15:230}"
  # the launch count (spike.md: the verb happens 2-4 times a run; the card's target): launch36 per verb-aware seed, median
  say "launches $id: $(grep -oE '^  seed [0-9]+ verb-aware: .*\| launch36 [0-9]+' "$D/verbs-skip.txt" | grep -oE 'launch36 [0-9]+$' | awk '{print $2}' | sort -n | awk '{a[NR]=$1} END{if(NR) print "median " (NR%2?a[(NR+1)/2]:(a[NR/2]+a[NR/2+1])/2) " per verb-aware run over " NR " seeds"; else print "none"}')"
  # NIGHT70 Z3: the committed run's launches beside (a launch room's skip v2 runs it; the card's target stays the plain verb-aware run's, above)
  say "launches (committed, Z3) $id: $(grep -oE '^  seed [0-9]+ committed: .*\| launch36 [0-9]+' "$D/verbs-skip.txt" | grep -oE 'launch36 [0-9]+$' | awk '{print $2}' | sort -n | awk '{a[NR]=$1} END{if(NR) print "median " (NR%2?a[(NR+1)/2]:(a[NR/2]+a[NR/2+1])/2) " per committed run over " NR " seeds"; else print "none (no committed run in the skip)"}'); verb side committed on: $(grep -oE 'committed on [0-9]+ of [0-9]+ seeds' "$D/verbs-skip.txt" | head -1)"
  # NIGHT70 Z2: the throw chart of the skip's plain verb-aware run (`n65verb skip launchend:` lines): launches per run (median over the seeds)
  # and how the launches ended, summed over the seeds, with the falls (fallReturn43 counted) of the verb-aware seed lines
  say "launch ends $id: $(grep -E "^n65verb skip launchend: room $id \|" "$D/verbs-skip.txt" | node -e "const L=require('fs').readFileSync(0,'utf8').split(/\r?\n/).filter(Boolean);const per={},k={};let pit=0,lava=0;for(const l of L){const s=(/\| seed (\d+) \|/.exec(l)||[])[1];per[s]=(per[s]||0)+1;const e=(/\| end ([a-z]+)/.exec(l)||[])[1]||'-';k[e]=(k[e]||0)+1;if(/in pit yes/.test(l))pit++;if(/in lava yes/.test(l))lava++;}const v=Object.values(per).sort((a,b)=>a-b);const med=v.length?(v.length%2?v[(v.length-1)/2]:(v[v.length/2-1]+v[v.length/2])/2):0;console.log(L.length?('median '+med+' charted launches per run over the '+v.length+' seeds that launched; ends '+Object.entries(k).map(([a,b])=>a+' '+b).join(', ')+'; in pit '+pit+', in lava '+lava):'no launchend lines (a skip before Z2, or no launch)')"); falls (verb-aware seeds): $(grep -oE '^  seed [0-9]+ verb-aware: [^|]*\| deaths [0-9]+, falls [0-9]+' "$D/verbs-skip.txt" | grep -oE 'falls [0-9]+$' | awk '{s+=$2} END{print s+0}')"
  f="$D/fresh-$id.txt"; rf="$D/room-files/$id.json"
  { echo "# command: bash Tools/night/n65-verbs.sh skip $D/n67-$L.json $id --seed 21 --seeds 10 --bars $D/slate-at-start.json --room-file $rf"; echo "# sha: $(git rev-parse --short HEAD); time: $(date '+%F %T')"; bash Tools/night/n65-verbs.sh skip "$D/n67-$L.json" "$id" --seed 21 --seeds 10 --bars "$D/slate-at-start.json" --room-file "$rf" 2>&1; echo "exit $?"; } > "$f"
  say "fresh $id: $(grep -E '^n65verb skip: M2 FRESH' "$f" | head -1 | cut -c15-200)"
  node Tools/night/n70-patch-share.cjs --level "$L" "$id" > "$D/patch-$id.txt" 2>&1; say "patch share $id: $(tail -1 "$D/patch-$id.txt" | cut -c1-120)"
  if [ "$(slate_of "$id" verb)" = launch ]; then
    # NIGHT70 Z1: the round (n70-round-verbs.sh) makes the chart before its launch pilot (M3 reads it); made here only when it is missing
    grep -q '^n70-throws: ' "$D/throws-$id.txt" 2>/dev/null || bash Tools/night/n70-throws.sh "$D/n67-$L.json" "$id" "$D/throws-$id.txt" 20 > /dev/null 2>&1; say "throws $id: $(tail -1 "$D/throws-$id.txt" | cut -c1-200)"
    say "M3 $id: $(node Tools/night/n70-throws-sum.cjs --m3 "$D/throws-$id.txt" --room "$id" | cut -c1-120)"
  fi
done
say "room-chain $N end (rc $rc)"
echo "$rc" > "Logs/n70/room-chain-$N.done"
exit "$rc"
