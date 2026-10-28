#!/usr/bin/env bash
# NIGHT66 (the lead's; FORGE66's copy of n66-round-verbs.sh, its red team's finding 1: Logs/n66, 10 seeds, the slot rooms): each room's own
# verb pilot and the skip test (n65verb, Tools/night/n65-verbs.sh; T1b's skip v2 takes over the skip line) on the level in the tree, into
# Logs/n66/<tag>/: verbs-pierce.txt (a2, a8), verbs-launch.txt (a5, b1), verbs-launch-slide.txt (a5, D-3),
# verbs-parry.txt (a4), verbs-skip.txt (every slot room), and verbs.md (every verdict line). a3's and b2's ride pilot is n66-red.sh's ride.txt. The reds'
# twins are Logs/n66/red/verbs-*.txt and Logs/n66/red65/verbs-*.txt. Called by n66-round.sh while the latest level is in place; alone:
#   bash Tools/night/n66-round-verbs.sh <tag> [--latest]      --latest: assemble the latest level into Logs/n66/<tag>/ first (gen --out)
#   LEVEL_FILE=<file> bash Tools/night/n66-round-verbs.sh <tag>  the pilots on that level file (n66-round.sh passes the round's own)
cd "$(git rev-parse --show-toplevel)" || exit 1
tag="${1:?tag}"; out="Logs/n66/$tag"; mkdir -p "$out"; T0=$SECONDS
[ -e "$out/verbs.md" ] && { echo "n66-round-verbs $tag: $out/verbs.md exists: never overwritten (FORGE66 red team 1)"; exit 1; }
ROOMS="${ROOMS:-$(node -p "Object.keys(require('./Design/levels/n65/slots65.json').slots).join(' ')")}"
# NIGHT66 T1c: LEVEL_FILE (n66-round.sh: the round's Logs/n66/<tag>/n65-foundry.json, from gen --out) is the level every pilot reads; the
# assembly note beside it. --latest assembles the latest level into <out> (gen --out): the tracked level files are never written.
L="${LEVEL_FILE:-Design/levels/n65-foundry.json}"; sha=$(git rev-parse --short HEAD)
ASM="$(dirname "$L")/foundry.assembly.json"; [ -f "$ASM" ] || ASM=Design/levels/n65/foundry.assembly.json
if [ "$2" = "--latest" ] && [ -z "$LEVEL_FILE" ]; then node Tools/night/gen-n65-foundry.cjs --run --slots=latest --out="$out" > "$out/gen-latest-verbs.txt" 2>&1 || { echo "n66-round-verbs: the latest assembly failed"; exit 1; }; L="$out/n65-foundry.json"; ASM="$out/foundry.assembly.json"; fi
run() { # <file> <verb> <rooms> [args]
  local f="$1"; shift
  { echo "# command: bash Tools/night/n65-verbs.sh $1 $L $2 ${*:3}"; echo "# sha: $sha; the level's $(grep -o '"slots": *"[^"]*"' "$ASM" | head -1) ($ASM); time: $(date '+%F %T')"; bash Tools/night/n65-verbs.sh "$1" "$L" "$2" "${@:3}" 2>&1; } > "$f"
}
run "$out/verbs-pierce.txt" pierce a2,a8
run "$out/verbs-launch.txt" launch a5,b1
run "$out/verbs-launch-slide.txt" launch a5 --slide
run "$out/verbs-parry.txt" parry a4
MAIN="$(cd "$(git rev-parse --git-common-dir)/.." && pwd -W)"   # NIGHT66 T1b fix (verdict-T1b.md problem 10): the MAIN tree's slate, from any worktree
run "$out/verbs-skip.txt" skip "$(echo $ROOMS | tr ' ' ',')" --seeds 10 --bars "$MAIN/Logs/n66/slate.json"   # NIGHT66 T1b: skip v2 (M1 against each room's mainBar), 10 seeds (FORGE66 red team 1)
#   # skip v2: per seed the verb-aware pilot, the same pilot with the verb refused inside it, the plain walker (Harness.N65.Verb.cs V66SkipV2)
grep -h -E "^n65verb [a-z]+: |^n65-verbs " "$out"/verbs-*.txt > "$out/verbs.md"
echo "n66-round-verbs $tag: $(grep -c '^n65verb' "$out/verbs.md") verdicts in $((SECONDS - T0)) s ($out/verbs.md)"
if [ "$2" = "--latest" ]; then echo "n66-round-verbs: the latest level stayed in $out (gen --out): nothing to put back"; fi
