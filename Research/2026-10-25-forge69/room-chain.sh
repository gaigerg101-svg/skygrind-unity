#!/usr/bin/env bash
# NIGHT69's per-room chain (FORGE69; the lead copies it to Logs/n69/room-chain.sh at the bearings). For each room given, in series (one
# dotnet job at a time in the main tree): a round with ONLY that room new in its level (NIGHT69.md section 6), its fresh set on seeds
# 21-30 (with --room-file, for the GATE pack's --fresh), and its patch share when the tool exists. It does NOT run the GATE pack: the pack
# needs the round's level review (a `## <id>` section), which a fresh reviewer writes after the round ends (red team 3); the lead runs the
# pack then:  L67=<L> bash Tools/night/n69-gate-pack.sh <id> r<n>-<L> --fresh Logs/n69/r<n>-<L>/fresh-<id>.txt > Logs/n69/gate-<id>-pack.txt
# Every room is alone on its level (kb3 kiln, v2 spire, g8 iceline), so the round's folder is r<n>-<level>, as the level review, n69-next.cjs and
# the GATE pack expect. A failed round cannot re-run under the same N (n69-round.sh refuses an existing folder): a re-run is a NEW N.
#   bash Logs/n69/room-chain.sh <n> <id> [<id> ...]  > Logs/n69/room-chain-<n>.txt 2>&1     (background; the lead may not pick, commit or write
#   the slate on port/stage3 while it runs; Logs/n69/room-chain-<n>.done holds its exit code, written last: no marker = still running or killed)
cd "$(git rev-parse --show-toplevel)" || exit 1
export MSBUILDDISABLENODEREUSE=1
N="$1"; shift
[ -n "$N" ] && [ "$#" -ge 1 ] || { echo "usage: room-chain.sh <n> <id> [<id> ...]"; exit 2; }
say() { echo "$(date '+%T') $*"; }
slate_of() { node -p "require('./Logs/n69/slate.json').slices.find(s=>s.id==='$1').$2"; }
rc=0
say "room-chain $N start, sha $(git rev-parse --short HEAD), rooms: $*"
dotnet build Tools/SimHarness -c Release -nologo -v q > "Logs/n69/room-chain-$N-build.txt" 2>&1 || { say "harness build FAILED"; echo 1 > "Logs/n69/room-chain-$N.done"; exit 1; }
say "harness build ok"
for id in "$@"; do
  L=$(slate_of "$id" level)
  D="Logs/n69/r$N-$L"
  L67=$L ROOMS=$id SLOTS=$id bash Tools/night/n69-round.sh "r$N-$L" > "Logs/n69/r$N-$L-run.txt" 2>&1 || { rc=1; say "round $id FAILED (see Logs/n69/r$N-$L-run.txt)"; continue; }
  say "round $id ($L, alone): $(grep -E 'done in' "Logs/n69/r$N-$L-run.txt" | tail -1 | cut -c1-70)"
  line=$(grep -E "^n65verb skip: M2 VERB" "$D/verbs-skip.txt" 2>/dev/null | grep "room $id," | head -1)
  say "M2 $id: ${line:15:230}"
  f="$D/fresh-$id.txt"; rf="$D/room-files/$id.json"
  { echo "# command: bash Tools/night/n65-verbs.sh skip $D/n67-$L.json $id --seed 21 --seeds 10 --bars $D/slate-at-start.json --room-file $rf"; echo "# sha: $(git rev-parse --short HEAD); time: $(date '+%F %T')"; bash Tools/night/n65-verbs.sh skip "$D/n67-$L.json" "$id" --seed 21 --seeds 10 --bars "$D/slate-at-start.json" --room-file "$rf" 2>&1; echo "exit $?"; } > "$f"
  say "fresh $id: $(grep -E '^n65verb skip: M2 FRESH' "$f" | head -1 | cut -c15-200)"
  if [ -f Tools/night/n69-patch-share.cjs ]; then
    node Tools/night/n69-patch-share.cjs --level "$L" "$id" > "$D/patch-$id.txt" 2>&1; say "patch share $id: $(tail -1 "$D/patch-$id.txt" | cut -c1-120)"
  else say "patch share $id: not run (Tools/night/n69-patch-share.cjs is not on main yet)"; fi
done
say "room-chain $N end (rc $rc)"
echo "$rc" > "Logs/n69/room-chain-$N.done"
exit "$rc"
