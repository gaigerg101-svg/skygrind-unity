#!/usr/bin/env bash
# NIGHT68 (the lead's; FORGE66's copy of n68-round.sh, its red team's findings 1 and 11): one sim round of every slot room on the main tree,
# after the lead's pick of the builders' room files. In order, each output under Logs/n68/<tag>/ (refused when that folder exists) with its
# command at its top (L-149):
#   1. gen-latest.txt: `node Tools/night/gen-n65-foundry.cjs --run --slots=latest --out=Logs/n68/<tag>` (every slot's newest room file: the
#      round's level; with --shipped, slots65.json's set: the red). NIGHT68 T1c: the level (n65-foundry.json), plan (foundry.plan.json),
#      assembly, openings65.json and levelgen report land in Logs/n68/<tag>/ ONLY (never `git add -f` them); the tracked level files are not
#      written while the round runs (tracked-status.txt: their git status before and after)
#   2. <id>/...: `LEVEL_DIR=Logs/n68/<tag> bash Tools/night/n68-red.sh 6 n65-foundry Logs/n68/<tag>` (room check --dir, n46flow --file seeds
#      1-10 / 11-20, hold52 --pads --file, the plan PNG, the verb-aware flow V, the ride pilot in a3 and b2) on that level
#   3. <id>/iso.png: builder T's sim-side picture (Tools/night/n65-iso.cjs on the round's plan, Logs/n68/<tag>/foundry.plan.json) when it exists
#   4. verbs-*.txt: `LEVEL_FILE=Logs/n68/<tag>/n65-foundry.json bash Tools/night/n68-round-verbs.sh <tag>` (the pilots, skip v2); air.txt
#   5. table.md and table-red65.md: `node Tools/night/n68-table.cjs Logs/n68/<tag> [Logs/n68/red65]` (each number against each red)
# Run it alone in this tree (two dotnet runs never share a tree). No shipped level to put back any more (T1c): a build or a commit during a
# round no longer ships the round's level.
#   bash Tools/night/n68-round.sh <tag> [--shipped]      e.g. r1; the reds: `red --shipped`, then `red65` (both before the first pick)
# NIGHT68 T1b (THE AREAS; slate T1b line 1; FORGE67b redteam.md 5 / 6 / 8 / 14): L67=<iceline|kiln|spire> runs the round on that level of
# Tools/night/levels67.json: gen-slots.cjs --level <L> --run (--slots=latest, or with --shipped the level's slots67.json) --out=Logs/n68/<tag>
# (the level <id>.json, <L>.plan.json, <L>.assembly.json, openings67.json and LEVEL-<id>-validate.md land there only); ROOMS default to the
# slate's rooms of that level; the slate is COPIED to Logs/n68/<tag>/slate-at-start.json at the start and every instrument reads that copy
# (--bars, each room's verb: a slate write during a round changes nothing); each room's verb65 against the slate's verb is printed
# (verbs65.txt) and a MISMATCH stops the round before any run; a base room (no verb65) gets the slate's verb planted (skip v2, flow V); the
# ride pilot in every room whose verb is ride; the table against Logs/n68/red-<L> (NIGHT68.md bearings step 6). SLATE=<file> overrides the
# slate copied (a dry round's planted verbs). L67 unset (or foundry): level 1 as before, plus the slate snapshot.
#   L67=<L> ROOMS="<rooms>" bash Tools/night/n68-round.sh <tag> [--shipped]
cd "$(git rev-parse --show-toplevel)" || exit 1
tag="${1:?tag}"; mode="${2:-}"; out="Logs/n68/$tag"; T0=$SECONDS
[ -e "$out" ] && { echo "n68-round $tag: $out exists: a round's evidence is never overwritten (FORGE66 red team 1); use a new tag"; exit 1; }
[ "$L67" = foundry ] && L67=""
if [ -n "$L67" ]; then node Tools/night/gen-slots.cjs --level "$L67" --info=id > /dev/null || { echo "n68-round $tag: L67=$L67 is not a level of Tools/night/levels67.json"; exit 1; }; fi
mkdir -p "$out"
MAIN="$(cd "$(git rev-parse --git-common-dir)/.." && pwd -W)"
SRC_SLATE="${SLATE:-$MAIN/Logs/n68/slate.json}"
[ -f "$SRC_SLATE" ] || { echo "n68-round $tag: no slate $SRC_SLATE"; exit 1; }
cp "$SRC_SLATE" "$out/slate-at-start.json"; export SLATE="$out/slate-at-start.json"   # FORGE67b red team 8: the round reads this copy only
if [ -n "$L67" ]; then
  LID=$(node Tools/night/gen-slots.cjs --level "$L67" --info=id); LSLOTS=$(node Tools/night/gen-slots.cjs --level "$L67" --info=slotsFile)
  ROOMS="${ROOMS:-$(node Tools/night/n68-verbs-of.cjs "$SLATE" x --rooms-of="$L67")}"
  [ -n "$ROOMS" ] || { echo "n68-round $tag: no ROOMS and the slate has no room of level $L67"; exit 1; }
  export L67
else LID=n65-foundry; fi
ROOMS="${ROOMS:-$(node -p "Object.keys(require('./Design/levels/n65/slots65.json').slots).join(' ')")}"; export ROOMS   # the slot rooms, or the caller's (FORGE67 red team 17: NIGHT68 runs ROOMS="a2 a3 a4 a5 a8")
sha=$(git rev-parse --short HEAD)
# NIGHT68 T1c fix (Logs/n68/verdict-T1c.md problems 1 and 4): what the round RAN: each room file (the latest slots' files) copied into
# <out>/room-files/<id>.json at the round's start, with its sha256 in <out>/room-files.sha256 (the round's sha, the time); the GATE pack
# compares the room file then against this record and refuses on a difference; the round's deco judges the copy, not the working tree
RDIR=Design/levels/n65/rooms   # NIGHT68 T1c round 2 (verdict-T1c-r2.md A10): an L67 level's folder, or the round stops (never level 1's by accident)
if [ -n "$L67" ]; then RDIR=$(node Tools/night/gen-slots.cjs --level "$L67" --info=roomsDir) || { echo "n68-round $tag: cannot read the rooms folder of L67=$L67 (gen-slots --info=roomsDir)"; exit 1; }; fi
mkdir -p "$out/room-files"
{ echo "# the room files this round ran (n68-round.sh $tag, sha $sha, $(date '+%F %T')): sha256 <file> (the raw bytes; the GATE pack compares line endings aside)"; for r in $ROOMS; do if [ -f "$RDIR/$r.json" ]; then cp "$RDIR/$r.json" "$out/room-files/$r.json" && echo "$(sha256sum "$RDIR/$r.json" | cut -c1-64) $RDIR/$r.json"; else echo "MISSING $RDIR/$r.json"; fi; done; } > "$out/room-files.sha256"
grep -q "^MISSING" "$out/room-files.sha256" && { echo "n68-round $tag: a room file is missing ($out/room-files.sha256):"; grep "^MISSING" "$out/room-files.sha256"; exit 1; }
echo "n68-round $tag: level ${L67:-foundry} ($LID), rooms $ROOMS, the slate snapshot $SLATE (from $SRC_SLATE, sha256 $(sha256sum "$SLATE" | cut -c1-16))"
gen() { # <file> <args...>
  local f="$1"; shift
  if [ -n "$L67" ]; then { echo "# command: node Tools/night/gen-slots.cjs --level $L67 $*"; echo "# sha: $sha; time: $(date '+%F %T')"; node Tools/night/gen-slots.cjs --level "$L67" "$@" 2>&1; echo "exit $?"; } > "$f"
  else { echo "# command: node Tools/night/gen-n65-foundry.cjs $*"; echo "# sha: $sha; time: $(date '+%F %T')"; node Tools/night/gen-n65-foundry.cjs "$@" 2>&1; echo "exit $?"; } > "$f"; fi
}
# NIGHT68 T1c (red team 11, slate T1c line 3): the round's level, plan, assembly, openings and levelgen report are written by gen --out into
# $out only; every instrument below reads them there (n68-red.sh LEVEL_DIR, n65-iso.cjs and n65-air-rooms.sh the round's plan
# $out/foundry.plan.json, n68-round-verbs.sh LEVEL_FILE $out/n65-foundry.json), so
# the tracked level files are never written while a round runs (their status is saved before and after: tracked-status.txt)
TRACKED="Design/levels Assets/StreamingAssets/Design/levels Reports"
{ echo "# git status --short $TRACKED, before the round ($(date '+%F %T')):"; git status --short $TRACKED; } > "$out/tracked-status.txt"
if [ "$mode" = "--shipped" ]; then gen "$out/gen-latest.txt" --run --out="$out"; else gen "$out/gen-latest.txt" --run --slots=latest --out="$out"; fi
grep -q "^exit 0$" "$out/gen-latest.txt" || { echo "n68-round $tag: the latest assembly failed (tail of $out/gen-latest.txt):"; tail -5 "$out/gen-latest.txt"; exit 1; }
echo "n68-round $tag: ${mode:-latest} assembled into $out ($((SECONDS - T0)) s): $(grep -iE 'walk|ERROR' "$out/gen-latest.txt" | tail -1)"
PLANF="$out/${L67:-foundry}.plan.json"   # the round's own plan (gen --out): <L>.plan.json
cp "$out/LEVEL-$LID-validate.md" "$out/levelgen-report.md"   # the round's own levelgen rows, under the name the table and the GATE pack read
# NIGHT68 T1b (redteam.md 6): each room's verb65 against the snapshot's verb, printed; a MISMATCH stops the round before any run
if [ -n "$L67" ]; then
  { echo "# command: node Tools/night/n68-verbs-of.cjs $SLATE $out/$LID.json $ROOMS"; echo "# sha: $sha; time: $(date '+%F %T')"; node Tools/night/n68-verbs-of.cjs "$SLATE" "$out/$LID.json" $ROOMS; } > "$out/verbs65.txt" 2>&1 || { echo "n68-round $tag: REFUSED: a room's verb65 differs from the slate's verb ($out/verbs65.txt): fix the room file or the slate (a verb change re-runs its red first)"; grep MISMATCH "$out/verbs65.txt"; exit 1; }
  sed -n '3,$p' "$out/verbs65.txt" | sed "s/^/n68-round $tag: /"
fi
LEVEL_DIR="$out" bash Tools/night/n68-red.sh 6 "$LID" "$out" > "$out/red-run.txt" 2>&1   # never `| head`: a closed pipe hangs the red's last echoes (r1, 1:40)
grep -m1 "^n68-red:" "$out/red-run.txt"
if [ -f Tools/night/n65-iso.cjs ]; then
  # NIGHT68 T1c fix (Logs/n68/verdict-T1c.md): the round's OWN plan (a level file is drawn from the plan its name names, the tracked one)
  for r in $ROOMS; do node Tools/night/n65-iso.cjs "$PLANF" "$r" "$out/$r/iso.png" > "$out/$r/iso.txt" 2>&1 || echo "  iso $r: FAILED ($out/$r/iso.txt)"; done
  echo "n68-round $tag: iso pictures $(ls "$out"/*/iso.png 2>/dev/null | wc -l) / $(echo $ROOMS | wc -w)"
else echo "n68-round $tag: no Tools/night/n65-iso.cjs yet (plan PNGs only)"; fi
LEVEL_FILE="$out/$LID.json" bash Tools/night/n68-round-verbs.sh "$tag"   # each room's verb pilot and skip v2, on the round's level (SLATE: the snapshot)
[ -f Tools/night/n65-air-rooms.sh ] && { echo "# command: bash Tools/night/n65-air-rooms.sh $PLANF $(echo $ROOMS | tr ' ' ',') (the round's plan); sha $sha"; bash Tools/night/n65-air-rooms.sh "$PLANF" "$(echo $ROOMS | tr ' ' ',')" 2>&1; } > "$out/air.txt"   # air65: flights handed on (NIGHT68 T1b: the round's rooms)
node Tools/night/n68-table.cjs "$out" > "$out/table.md"   # NIGHT68 T1b: against Logs/n68/red-<L> for an L67 level, Logs/n68/red for level 1
[ -z "$L67" ] && [ -d Logs/n68/red65 ] && node Tools/night/n68-table.cjs "$out" Logs/n68/red65 > "$out/table-red65.md"
# NIGHT68 T1b: M1 AREA per room (the ROOM list's command, NIGHT68.md section 4 step 2): area-<id>.txt, its command and sha at its top
if [ -n "$L67" ]; then for r in $ROOMS; do { echo "# command: node Tools/night/n68-area.cjs --level $L67 --dir $out --touches $out/verbs-skip.txt $r"; echo "# sha: $sha; time: $(date '+%F %T')"; node Tools/night/n68-area.cjs --level "$L67" --dir "$out" --touches "$out/verbs-skip.txt" "$r" 2>&1; } > "$out/area-$r.txt"; echo "n68-round $tag: $(tail -1 "$out/area-$r.txt")"; done; fi
# NIGHT68 T1c: M4 per room (the decoration checker; its base the room file's first commit, the round's skip touches): deco-<id>.txt
if [ -n "$L67" ]; then for r in $ROOMS; do { echo "# command: L67=$L67 node Tools/night/n68-deco.cjs $r --touches $out/verbs-skip.txt --file $out/room-files/$r.json (the round's own copy of the room file)"; echo "# sha: $sha; time: $(date '+%F %T')"; node Tools/night/n68-deco.cjs "$r" --touches "$out/verbs-skip.txt" --file "$out/room-files/$r.json" 2>&1; } > "$out/deco-$r.txt"; echo "n68-round $tag: $(grep -m1 '^n68-deco: \(PASS\|FAIL\)' "$out/deco-$r.txt" || tail -1 "$out/deco-$r.txt")"; done; fi
{ echo "# git status --short $TRACKED, after the round ($(date '+%F %T')):"; git status --short $TRACKED; } >> "$out/tracked-status.txt"
echo "n68-round $tag: done in $((SECONDS - T0)) s; table $out/table.md; the round's level is $out/$LID.json (never the tracked files); tracked status before / after ($out/tracked-status.txt):"
sed 's/^/  /' "$out/tracked-status.txt"
