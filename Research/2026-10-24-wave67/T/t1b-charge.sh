#!/usr/bin/env bash
# NIGHT67 builder T, T1b line 3 (FORGE67b redteam.md 10): builder T's NIGHT66 after-runs of the charge rule (d25ea7d8) re-run at NIGHT67's t0
# code (this tree's built DLL; d25ea7d8 is in HEAD), the same commands as Research/2026-10-23-wave66/T/t1b-amend.sh "after" (skip v2 and
# n46flow --verbs65, seeds 1-10, a2 and a8, on the main tree's Logs/n66/r2 and Logs/n66/red65 levels and Logs/n66/slate.json, read only), each
# output into Research/2026-10-24-wave67/T/t1b-charge/<the NIGHT66 file name>, then each diffed against NIGHT66's file with the header and
# wall-time lines aside (identical = the code T measured): diff.txt. Runs the BUILT harness (never builds it; never `dotnet run`); one job at
# a time (one dotnet job in this tree).
#   bash Research/2026-10-24-wave67/T/t1b-charge.sh
cd "$(git rev-parse --show-toplevel)" || exit 1
OUT=Research/2026-10-24-wave67/T/t1b-charge; OLD=Research/2026-10-23-wave66/T
D=Temp/SimHarness/bin/Release/net10.0/SimHarness.dll
M=C:/Users/gaige/dev/SKYGRIND-unity/Logs/n66
mkdir -p "$OUT"
sha=$(git rev-parse --short HEAD); dirty=$(git status --short -uno | tr '\n' ' ')
dll="$D ($(ls -la --time-style=+%F_%T "$D" | awk '{print $6}'))"
for lv in r2 red65; do
  L="$M/$lv/n65-foundry.json"
  for r in a2 a8; do
    for what in skip flowV; do
      f="$OUT/t1b-amend-after-$lv-$what-$r.txt"
      if [ "$what" = skip ]; then args="n65verb skip --level $L --room $r --seeds 10 --bars $M/slate.json"; else args="n46flow --level n65-foundry --file $L --rooms $r --seeds 10 --first 1 --verbs65"; fi
      { echo "# command: dotnet $D $args"; echo "# sha: $sha (tracked changes: ${dirty:-none}); harness $dll; $(date '+%F %T'); $(pwd -W)"; echo; timeout 900 dotnet "$D" $args < /dev/null 2>&1; echo "# exit $?, $(date +%T)"; } > "$f"
      echo "ran $f: $(grep -h -E 'n65verb skip: M1|SUMMARY' "$f" | head -1 | cut -c1-160)"
    done
  done
done
# the diff: every line but the header (#), the wall-time line and the blank line after the header
strip() { grep -v -E '^#|wall time|^$' "$1" | sed 's/\r$//'; }
{
  echo "# t1b-charge diff: NIGHT66's after-runs ($OLD/t1b-amend-after-*.txt, harness built from d25ea7d8's tree) against NIGHT67's re-runs ($OUT/, sha $sha, $dll), header and wall-time lines aside; $(date '+%F %T')"
  same=0; differ=0
  for f in "$OUT"/t1b-amend-after-*.txt; do
    b=$(basename "$f")
    if diff <(strip "$OLD/$b") <(strip "$f") > /dev/null; then echo "IDENTICAL $b ($(strip "$f" | wc -l) lines)"; same=$((same + 1));
    else echo "DIFFERS   $b:"; diff <(strip "$OLD/$b") <(strip "$f") | head -20 | cut -c1-240; differ=$((differ + 1)); fi
    echo "   old: $(grep -h -E 'n65verb skip: M1|SUMMARY' "$OLD/$b" | head -1 | cut -c1-150)"
    echo "   new: $(grep -h -E 'n65verb skip: M1|SUMMARY' "$f" | head -1 | cut -c1-150)"
  done
  echo "t1b-charge: $same identical, $differ differ (of $((same + differ)))"
} > "$OUT/diff.txt"
tail -1 "$OUT/diff.txt"
