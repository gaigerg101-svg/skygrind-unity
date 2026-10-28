#!/usr/bin/env bash
# NIGHT65 (the lead's; branch C): each rebuilt room's own verb pilot and the skip test (builder T's n65verb, Tools/night/n65-verbs.sh) on the
# level in the tree, into Logs/n65/<tag>/: verbs-pierce.txt (a2, a8), verbs-launch.txt (a5, b1), verbs-launch-slide.txt (a5, D-3),
# verbs-parry.txt (a4), verbs-skip.txt (all six), and verbs.md (every verdict line). a3's ride pilot is n65-red.sh's ride.txt. The red's
# twins are Research/2026-10-23-wave65/T/verb-*-red.txt (n52-foundry). Called by n65-round.sh while the latest level is in place; alone:
#   bash Tools/night/n65-round-verbs.sh <tag> [--latest]      --latest: assemble the latest level first and put the shipped one back after
cd "$(git rev-parse --show-toplevel)" || exit 1
tag="${1:?tag}"; out="Logs/n65/$tag"; mkdir -p "$out"; T0=$SECONDS
L=Design/levels/n65-foundry.json; sha=$(git rev-parse --short HEAD)
if [ "$2" = "--latest" ]; then node Tools/night/gen-n65-foundry.cjs --run --slots=latest > "$out/gen-latest-verbs.txt" 2>&1 || { echo "n65-round-verbs: the latest assembly failed"; exit 1; }; fi
run() { # <file> <verb> <rooms> [args]
  local f="$1"; shift
  { echo "# command: bash Tools/night/n65-verbs.sh $1 $L $2 ${*:3}"; echo "# sha: $sha; the level's $(grep -o '"slots": *"[^"]*"' Design/levels/n65/foundry.assembly.json | head -1); time: $(date '+%F %T')"; bash Tools/night/n65-verbs.sh "$1" "$L" "$2" "${@:3}" 2>&1; } > "$f"
}
run "$out/verbs-pierce.txt" pierce a2,a8
run "$out/verbs-launch.txt" launch a5,b1
run "$out/verbs-launch-slide.txt" launch a5 --slide
run "$out/verbs-parry.txt" parry a4
run "$out/verbs-skip.txt" skip a2,a3,a5,b1,a8,a4 --verbs65 --seeds 5   # the verb-aware pilot (T's verb65) against the verb refused; plain skip never performs the verb (r3, 3:12)
grep -h -E "^n65verb [a-z]+: |^n65-verbs " "$out"/verbs-*.txt > "$out/verbs.md"
echo "n65-round-verbs $tag: $(grep -c '^n65verb' "$out/verbs.md") verdicts in $((SECONDS - T0)) s ($out/verbs.md)"
if [ "$2" = "--latest" ]; then node Tools/night/gen-n65-foundry.cjs --run > "$out/gen-shipped-verbs.txt" 2>&1 || { echo "n65-round-verbs: the SHIPPED assembly failed: the tree holds the latest level"; exit 1; }; echo "n65-round-verbs: the shipped level put back"; fi
