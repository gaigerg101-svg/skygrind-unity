#!/usr/bin/env bash
# NIGHT67 builder T, T1b line 1's proof: `L67=<L> ROOMS=<room> bash Tools/night/n67-round.sh dry-<L>` end to end on all three levels (one room
# each, FORGE67b redteam.md 5: g8, kb3, v6), the dry slate's planted verbs (t1b-dry-slate.json: ride, bar 1.25) through SLATE=, the tracked
# status before / after each; then a PLANTED verb65 MISMATCH (g8's room file given verb65 pierce against the slate's ride): the round must
# refuse before any run; the file restored. Waits for the charge re-run (one dotnet job in this tree). Outputs: Logs/n67/dry-<L>/ (T's tree)
# and Research/2026-10-24-wave67/T/t1b-dry-<L>.txt (each run's console).
#   bash Research/2026-10-24-wave67/T/t1b-dry.sh [iceline kiln spire mismatch]
cd "$(git rev-parse --show-toplevel)" || exit 1
E=Research/2026-10-24-wave67/T
until [ -f "$E/t1b-charge/diff.txt" ]; do sleep 5; done
items="${*:-iceline kiln spire mismatch}"
declare -A ROOM=([iceline]=g8 [kiln]=kb3 [spire]=v6)
for it in $items; do
  if [ "$it" = mismatch ]; then
    f="$E/t1b-dry-mismatch.txt"; rf=Design/levels/n67/iceline/rooms/g8.json
    { echo "# PLANTED: $rf row.verb65 = pierce (the dry slate says ride); L67=iceline ROOMS=g8 SLATE=$E/t1b-dry-slate.json bash Tools/night/n67-round.sh dry-iceline-mismatch"; echo "# sha $(git rev-parse --short HEAD); $(date '+%F %T')"
      node $E/t1b-plant-verb65.cjs "$rf" pierce
      L67=iceline ROOMS=g8 SLATE=$E/t1b-dry-slate.json timeout 1800 bash Tools/night/n67-round.sh dry-iceline-mismatch; echo "# exit $? (1 expected: refused)"
      git checkout -- "$rf"; echo "# restored $rf: git status '$(git status --short -- "$rf")' (empty = restored)"
      echo "# the round folder holds: $(ls Logs/n67/dry-iceline-mismatch | tr '\n' ' ')"; } > "$f" 2>&1
    tail -6 "$f"; continue
  fi
  f="$E/t1b-dry-$it.txt"
  { echo "# L67=$it ROOMS=${ROOM[$it]} SLATE=$E/t1b-dry-slate.json bash Tools/night/n67-round.sh dry-$it"; echo "# sha $(git rev-parse --short HEAD); dirty: $(git status --short -uno | tr '\n' ' '); $(date '+%F %T')"
    L67=$it ROOMS=${ROOM[$it]} SLATE=$E/t1b-dry-slate.json timeout 3600 bash Tools/night/n67-round.sh "dry-$it"; echo "# exit $? at $(date +%T)"; } > "$f" 2>&1
  tail -4 "$f"
done
