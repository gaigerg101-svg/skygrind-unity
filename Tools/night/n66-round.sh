#!/usr/bin/env bash
# NIGHT66 (the lead's; FORGE66's copy of n66-round.sh, its red team's findings 1 and 11): one sim round of every slot room on the main tree,
# after the lead's pick of the builders' room files. In order, each output under Logs/n66/<tag>/ (refused when that folder exists) with its
# command at its top (L-149):
#   1. gen-latest.txt: `node Tools/night/gen-n65-foundry.cjs --run --slots=latest --out=Logs/n66/<tag>` (every slot's newest room file: the
#      round's level; with --shipped, slots65.json's set: the red). NIGHT66 T1c: the level (n65-foundry.json), plan (foundry.plan.json),
#      assembly, openings65.json and levelgen report land in Logs/n66/<tag>/ ONLY (never `git add -f` them); the tracked level files are not
#      written while the round runs (tracked-status.txt: their git status before and after)
#   2. <id>/...: `LEVEL_DIR=Logs/n66/<tag> bash Tools/night/n66-red.sh 6 n65-foundry Logs/n66/<tag>` (room check --dir, n46flow --file seeds
#      1-10 / 11-20, hold52 --pads --file, the plan PNG, the verb-aware flow V, the ride pilot in a3 and b2) on that level
#   3. <id>/iso.png: builder T's sim-side picture (Tools/night/n65-iso.cjs on the round's plan, Logs/n66/<tag>/foundry.plan.json) when it exists
#   4. verbs-*.txt: `LEVEL_FILE=Logs/n66/<tag>/n65-foundry.json bash Tools/night/n66-round-verbs.sh <tag>` (the pilots, skip v2); air.txt
#   5. table.md and table-red65.md: `node Tools/night/n66-table.cjs Logs/n66/<tag> [Logs/n66/red65]` (each number against each red)
# Run it alone in this tree (two dotnet runs never share a tree). No shipped level to put back any more (T1c): a build or a commit during a
# round no longer ships the round's level.
#   bash Tools/night/n66-round.sh <tag> [--shipped]      e.g. r1; the reds: `red --shipped`, then `red65` (both before the first pick)
cd "$(git rev-parse --show-toplevel)" || exit 1
tag="${1:?tag}"; mode="${2:-}"; out="Logs/n66/$tag"; T0=$SECONDS
[ -e "$out" ] && { echo "n66-round $tag: $out exists: a round's evidence is never overwritten (FORGE66 red team 1); use a new tag"; exit 1; }
mkdir -p "$out"
ROOMS="$(node -p "Object.keys(require('./Design/levels/n65/slots65.json').slots).join(' ')")"; export ROOMS   # the slot rooms (b2 too once T2 adds its slot)
sha=$(git rev-parse --short HEAD)
gen() { # <file> <args...>
  local f="$1"; shift
  { echo "# command: node Tools/night/gen-n65-foundry.cjs $*"; echo "# sha: $sha; time: $(date '+%F %T')"; node Tools/night/gen-n65-foundry.cjs "$@" 2>&1; echo "exit $?"; } > "$f"
}
# NIGHT66 T1c (red team 11, slate T1c line 3): the round's level, plan, assembly, openings and levelgen report are written by gen --out into
# $out only; every instrument below reads them there (n66-red.sh LEVEL_DIR, n65-iso.cjs and n65-air-rooms.sh the round's plan
# $out/foundry.plan.json, n66-round-verbs.sh LEVEL_FILE $out/n65-foundry.json), so
# the tracked level files are never written while a round runs (their status is saved before and after: tracked-status.txt)
TRACKED="Design/levels Assets/StreamingAssets/Design/levels Reports"
{ echo "# git status --short $TRACKED, before the round ($(date '+%F %T')):"; git status --short $TRACKED; } > "$out/tracked-status.txt"
if [ "$mode" = "--shipped" ]; then gen "$out/gen-latest.txt" --run --out="$out"; else gen "$out/gen-latest.txt" --run --slots=latest --out="$out"; fi
grep -q "^exit 0$" "$out/gen-latest.txt" || { echo "n66-round $tag: the latest assembly failed (tail of $out/gen-latest.txt):"; tail -5 "$out/gen-latest.txt"; exit 1; }
echo "n66-round $tag: ${mode:-latest} assembled into $out ($((SECONDS - T0)) s): $(grep -iE 'walk|ERROR' "$out/gen-latest.txt" | tail -1)"
cp "$out/LEVEL-n65-foundry-validate.md" "$out/levelgen-report.md"   # the round's own levelgen rows, under the name the table and the GATE pack read
LEVEL_DIR="$out" bash Tools/night/n66-red.sh 6 n65-foundry "$out" > "$out/red-run.txt" 2>&1   # never `| head`: a closed pipe hangs the red's last echoes (r1, 1:40)
grep -m1 "^n66-red:" "$out/red-run.txt"
if [ -f Tools/night/n65-iso.cjs ]; then
  # NIGHT66 T1c fix (Logs/n66/verdict-T1c.md): the round's OWN plan (a level file is drawn from the plan its name names, the tracked one)
  for r in $ROOMS; do node Tools/night/n65-iso.cjs "$out/foundry.plan.json" "$r" "$out/$r/iso.png" > "$out/$r/iso.txt" 2>&1 || echo "  iso $r: FAILED ($out/$r/iso.txt)"; done
  echo "n66-round $tag: iso pictures $(ls "$out"/*/iso.png 2>/dev/null | wc -l) / $(echo $ROOMS | wc -w)"
else echo "n66-round $tag: no Tools/night/n65-iso.cjs yet (plan PNGs only)"; fi
LEVEL_FILE="$out/n65-foundry.json" bash Tools/night/n66-round-verbs.sh "$tag"   # each room's verb pilot and skip v2, on the round's level
[ -f Tools/night/n65-air-rooms.sh ] && { echo "# command: bash Tools/night/n65-air-rooms.sh $out/foundry.plan.json (the round's plan); sha $sha"; bash Tools/night/n65-air-rooms.sh "$out/foundry.plan.json" 2>&1; } > "$out/air.txt"   # air65: flights handed on
node Tools/night/n66-table.cjs "$out" > "$out/table.md"
[ -d Logs/n66/red65 ] && node Tools/night/n66-table.cjs "$out" Logs/n66/red65 > "$out/table-red65.md"
{ echo "# git status --short $TRACKED, after the round ($(date '+%F %T')):"; git status --short $TRACKED; } >> "$out/tracked-status.txt"
echo "n66-round $tag: done in $((SECONDS - T0)) s; table $out/table.md; the round's level is $out/n65-foundry.json (never the tracked files); tracked status before / after ($out/tracked-status.txt):"
sed 's/^/  /' "$out/tracked-status.txt"
