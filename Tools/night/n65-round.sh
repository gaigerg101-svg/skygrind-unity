#!/usr/bin/env bash
# NIGHT65 (the lead's; branch C, DECIDE65 D-2: no Player): one sim round of every rebuilt room on the main tree, after the lead's pick of the
# builders' room files. In order, each output under Logs/n65/<tag>/ with its command at its top (L-149):
#   1. gen-latest.txt: `node Tools/night/gen-n65-foundry.cjs --run --slots=latest` (every slot's newest room file: the round's level)
#   2. <id>/...: `bash Tools/night/n65-red.sh 6 n65-foundry Logs/n65/<tag>` (room check, n46flow seeds 1-10 / 11-20, hold52 --pads, the plan
#      PNG, a3's ride pilot) on that level
#   3. <id>/iso.png: builder T's sim-side picture (Tools/night/n65-iso.cjs) when it exists
#   4. table.md: `node Tools/night/n65-table.cjs Logs/n65/<tag>` (each number against the red)
#   5. gen-shipped.txt: the shipped assembly put back (`--run` with slots65.json, or every slot old before any PASS), so the committed level
#      is always the shipped one; it prints the level files' git status after
# Run it alone in this tree (two dotnet runs never share a tree).   bash Tools/night/n65-round.sh <tag>      e.g. r1
cd "$(git rev-parse --show-toplevel)" || exit 1
tag="${1:?tag}"; out="Logs/n65/$tag"; mkdir -p "$out"; T0=$SECONDS
sha=$(git rev-parse --short HEAD)
gen() { # <file> <args...>
  local f="$1"; shift
  { echo "# command: node Tools/night/gen-n65-foundry.cjs $*"; echo "# sha: $sha; time: $(date '+%F %T')"; node Tools/night/gen-n65-foundry.cjs "$@" 2>&1; echo "exit $?"; } > "$f"
}
gen "$out/gen-latest.txt" --run --slots=latest
grep -q "^exit 0$" "$out/gen-latest.txt" || { echo "n65-round $tag: the latest assembly failed (tail of $out/gen-latest.txt):"; tail -5 "$out/gen-latest.txt"; exit 1; }
echo "n65-round $tag: latest assembled ($((SECONDS - T0)) s): $(grep -iE 'walk|ERROR' "$out/gen-latest.txt" | tail -1)"
cp Reports/LEVEL-n65-foundry-validate.md "$out/levelgen-report.md"   # the round's own levelgen rows (the slide tunnel's, ...): the shipped run overwrites the report after (r3b's critic)
bash Tools/night/n65-red.sh 6 n65-foundry "$out" > "$out/red-run.txt" 2>&1   # never `| head`: a closed pipe hangs the red's last echoes (r1, 1:40)
grep -m1 "^n65-red:" "$out/red-run.txt"
if [ -f Tools/night/n65-iso.cjs ]; then
  for r in a2 a3 a5 b1 a8 a4; do node Tools/night/n65-iso.cjs Design/levels/n65-foundry.json "$r" "$out/$r/iso.png" > "$out/$r/iso.txt" 2>&1 || echo "  iso $r: FAILED ($out/$r/iso.txt)"; done
  echo "n65-round $tag: iso pictures $(ls "$out"/*/iso.png 2>/dev/null | wc -l) / 6"
else echo "n65-round $tag: no Tools/night/n65-iso.cjs yet (plan PNGs only)"; fi
[ -f Tools/night/n65-verbs.sh ] && bash Tools/night/n65-round-verbs.sh "$tag"   # each room's verb pilot and the skip test, on the latest level
[ -f Tools/night/n65-air-rooms.sh ] && { echo "# command: bash Tools/night/n65-air-rooms.sh Design/levels/n65/foundry.plan.json (the latest plan); sha $sha"; bash Tools/night/n65-air-rooms.sh Design/levels/n65/foundry.plan.json 2>&1; } > "$out/air.txt"   # air65: flights handed on
node Tools/night/n65-table.cjs "$out" > "$out/table.md"
gen "$out/gen-shipped.txt" --run
grep -q "^exit 0$" "$out/gen-shipped.txt" || { echo "n65-round $tag: the SHIPPED assembly failed: the tree holds the latest level ($out/gen-shipped.txt)"; exit 1; }
echo "n65-round $tag: done in $((SECONDS - T0)) s; table $out/table.md; the shipped level put back; level files' status:"
git status --short Design/levels/n65-foundry.json Assets/StreamingAssets/Design/levels/n65-foundry.json Design/levels/n65/ | sed 's/^/  /'
