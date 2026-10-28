#!/usr/bin/env bash
# NIGHT68 (the lead's; FORGE66's copy of n68-round-verbs.sh, its red team's finding 1: Logs/n68, the slot rooms; NIGHT68 I4: skip v2 on seeds
# 1-20 pooled, was 10): each room's own
# verb pilot and the skip test (n65verb, Tools/night/n65-verbs.sh; T1b's skip v2 takes over the skip line) on the level in the tree, into
# Logs/n68/<tag>/: verbs-pierce.txt (a2, a8), verbs-launch.txt (a5, b1), verbs-launch-slide.txt (a5, D-3),
# verbs-parry.txt (a4), verbs-skip.txt (every slot room), and verbs.md (every verdict line). a3's and b2's ride pilot is n68-red.sh's ride.txt. The reds'
# twins are Logs/n68/red/verbs-*.txt and Logs/n68/red65/verbs-*.txt. Called by n68-round.sh while the latest level is in place; alone:
#   bash Tools/night/n68-round-verbs.sh <tag> [--latest]      --latest: assemble the latest level into Logs/n68/<tag>/ first (gen --out)
#   LEVEL_FILE=<file> bash Tools/night/n68-round-verbs.sh <tag>  the pilots on that level file (n68-round.sh passes the round's own)
cd "$(git rev-parse --show-toplevel)" || exit 1
tag="${1:?tag}"; out="Logs/n68/$tag"; mkdir -p "$out"; T0=$SECONDS
[ -e "$out/verbs.md" ] && { echo "n68-round-verbs $tag: $out/verbs.md exists: never overwritten (FORGE66 red team 1)"; exit 1; }
if [ -n "$L67" ] && [ "$L67" != foundry ]; then
  # NIGHT68 T1b: a levels67 level: its new id's level file and assembly (gen-slots.cjs), ROOMS default the slate's rooms of that level
  LID=$(node Tools/night/gen-slots.cjs --level "$L67" --info=id) || { echo "n68-round-verbs: L67=$L67 is not a level of Tools/night/levels67.json"; exit 1; }
  ROOMS="${ROOMS:-$(node Tools/night/n68-verbs-of.cjs "${SLATE:-$(cd "$(git rev-parse --git-common-dir)/.." && pwd -W)/Logs/n68/slate.json}" x --rooms-of="$L67")}"
  L="${LEVEL_FILE:-Design/levels/$LID.json}"; sha=$(git rev-parse --short HEAD)
  ASM="$(dirname "$L")/$L67.assembly.json"; [ -f "$ASM" ] || ASM=Design/levels/n67/$L67.assembly.json
  if [ "$2" = "--latest" ] && [ -z "$LEVEL_FILE" ]; then node Tools/night/gen-slots.cjs --level "$L67" --run --slots=latest --out="$out" > "$out/gen-latest-verbs.txt" 2>&1 || { echo "n68-round-verbs: the latest assembly failed"; exit 1; }; L="$out/$LID.json"; ASM="$out/$L67.assembly.json"; fi
else
ROOMS="${ROOMS:-$(node -p "Object.keys(require('./Design/levels/n65/slots65.json').slots).join(' ')")}"
# NIGHT68 T1c: LEVEL_FILE (n68-round.sh: the round's Logs/n68/<tag>/n65-foundry.json, from gen --out) is the level every pilot reads; the
# assembly note beside it. --latest assembles the latest level into <out> (gen --out): the tracked level files are never written.
L="${LEVEL_FILE:-Design/levels/n65-foundry.json}"; sha=$(git rev-parse --short HEAD)
ASM="$(dirname "$L")/foundry.assembly.json"; [ -f "$ASM" ] || ASM=Design/levels/n65/foundry.assembly.json
if [ "$2" = "--latest" ] && [ -z "$LEVEL_FILE" ]; then node Tools/night/gen-n65-foundry.cjs --run --slots=latest --out="$out" > "$out/gen-latest-verbs.txt" 2>&1 || { echo "n68-round-verbs: the latest assembly failed"; exit 1; }; L="$out/n65-foundry.json"; ASM="$out/foundry.assembly.json"; fi
fi
run() { # <file> <verb> <rooms> [args]
  local f="$1"; shift
  { echo "# command: bash Tools/night/n65-verbs.sh $1 $L $2 ${*:3}"; echo "# sha: $sha; the level's $(grep -o '"slots": *"[^"]*"' "$ASM" | head -1) ($ASM); time: $(date '+%F %T')"; bash Tools/night/n65-verbs.sh "$1" "$L" "$2" "${@:3}" 2>&1; } > "$f"
}
MAIN="$(cd "$(git rev-parse --git-common-dir)/.." && pwd -W)"   # NIGHT68 T1b fix (verdict-T1b.md problem 10): the MAIN tree's slate, from any worktree
# NIGHT68 T1b (THE AREAS; slate T1b line 1, redteam.md 6 / 8): SLATE=<file> (n68-round.sh passes the round's snapshot
# Logs/n68/<tag>/slate-at-start.json, so a slate write during a round changes nothing; default the main tree's Logs/n68/slate.json) gives
# --bars and, with L67=<iceline|kiln|spire>, each room's verb: the room's own pilot by its verb (ride: `ride --shoot`; launch; launch+slide:
# `launch --slide`; pierce; parry; slide has no pilot: said, not run), skip v2 with `--plant-verb65 <id>:<verb>` for a room whose row has no
# verb65 (a base room), and a verb65 / slate MISMATCH refused (verbs65.txt). Level 1 (L67 unset): its fixed pilots as before.
SLATE="${SLATE:-$MAIN/Logs/n68/slate.json}"
[ "$L67" = foundry ] && L67=""
if [ -z "$L67" ]; then
  run "$out/verbs-pierce.txt" pierce a2,a8
  run "$out/verbs-launch.txt" launch a5,b1
  run "$out/verbs-launch-slide.txt" launch a5 --slide
  run "$out/verbs-parry.txt" parry a4
  case " $ROOMS " in *" a3 "*) run "$out/verbs-ride.txt" ride a3 --shoot ;; esac   # FORGE67 red team 9: a3's M3, a kill from the wall
  run "$out/verbs-skip.txt" skip "$(echo $ROOMS | tr ' ' ',')" --seeds 20 --bars "$SLATE"   # NIGHT68 I4: skip v2 pooled over seeds 1-20 (the M2 VERB line against each room's mainBar; NIGHT68 T1b ran 10)
else
  { echo "# command: node Tools/night/n68-verbs-of.cjs $SLATE $L $ROOMS"; echo "# sha: $sha; time: $(date '+%F %T')"; node Tools/night/n68-verbs-of.cjs "$SLATE" "$L" $ROOMS; } > "$out/verbs65.txt" 2>&1 || { echo "n68-round-verbs $tag: REFUSED: a room's verb65 differs from the slate's verb ($out/verbs65.txt):"; grep MISMATCH "$out/verbs65.txt"; exit 1; }
  vr() { node Tools/night/n68-verbs-of.cjs "$SLATE" "$L" $ROOMS --verb="$1"; }
  R=$(vr ride); [ -n "$R" ] && run "$out/verbs-ride.txt" ride "$R" --shoot
  R=$(vr launch); [ -n "$R" ] && run "$out/verbs-launch.txt" launch "$R"
  R=$(vr launch+slide); [ -n "$R" ] && run "$out/verbs-launch-slide.txt" launch "$R" --slide
  R=$(vr pierce); [ -n "$R" ] && run "$out/verbs-pierce.txt" pierce "$R"
  R=$(vr parry); [ -n "$R" ] && run "$out/verbs-parry.txt" parry "$R"
  R=$(vr slide); [ -n "$R" ] && echo "n65-verbs slide: $R: no slide pilot (n65verb has none: M3 not checked)" > "$out/verbs-slide.txt"
  P=$(node Tools/night/n68-verbs-of.cjs "$SLATE" "$L" $ROOMS --plants)
  # NIGHT68 I4: skip v2 pooled over seeds 1-20 (the ROOM list's M2 VERB: the pooled median, each set's beside it, exercised N / 20)
  if [ -n "$P" ]; then run "$out/verbs-skip.txt" skip "$(echo $ROOMS | tr ' ' ',')" --seeds 20 --bars "$SLATE" --plant-verb65 "$P"
  else run "$out/verbs-skip.txt" skip "$(echo $ROOMS | tr ' ' ',')" --seeds 20 --bars "$SLATE"; fi
fi
#   # skip v2: per seed the verb-aware pilot, the same pilot with the verb refused inside it, the plain walker (Harness.N65.Verb.cs V66SkipV2)
grep -h -E "^n65verb [a-z]+( --[a-z-]+)*: |^n65-verbs " "$out"/verbs-*.txt > "$out/verbs.md"   # NIGHT68 T1b: `n65verb ride --shoot: RED |` is the ride pilot's verdict
echo "n68-round-verbs $tag: $(grep -c '^n65verb' "$out/verbs.md") verdicts in $((SECONDS - T0)) s ($out/verbs.md)"
if [ "$2" = "--latest" ]; then echo "n68-round-verbs: the latest level stayed in $out (gen --out): nothing to put back"; fi
