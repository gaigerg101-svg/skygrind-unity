#!/usr/bin/env bash
# NIGHT70 builder T, slice Z1 (Logs/n70/slate.json Z1; DECIDE70 D-3; was slice Y2's): the launch M3 readers' planted cases. Three readers,
# Tools/night/n65-verbs.sh (its "n65-verbs launch M3: <id> PASS|FAIL" line), the GATE pack's launchM3 (Tools/night/n70-gate-pack.cjs) and
# Tools/night/n70-next.cjs's M3 piece, read SAVED launch runs and the room's THROW CHART (throws-<id>.txt, Tools/night/n70-throws.sh).
# The rule (D-3): M3 = the room's 20-seed chart, a candidate = a GREEN probe with launch36 1+ AND a pit / lava end or a burst killing >= 2
# incl. the flier (a RED probe's burst never counts); PASS at >= 10 / 20; no chart: FAIL "no throw chart". n65-verbs.sh runs unmodified with
# a stand-in `dotnet` first on PATH (Tools/night/n70-m3-dotnet.sh: replays the saved block, builds nothing) and N70_THROWS_DIR at the case's
# chart; the pack's function is run by Tools/night/n70-m3-pack-case.cjs; n70-next.cjs on a planted root (a slate with the room as a launch
# room, a round folder r1-kiln with its verbs-skip line, the n65-verbs.sh M3 line as verbs.md, and the chart).
# NIGHT69 Y2's probe rules (a RED line "in a pit YES", a GREEN launch36 0 "in a pit YES", lava counts) are probe lines of the planted chart P7.
#   bash Tools/night/n70-m3-cases.sh [--at <sha>]     this tree's readers, or the readers of commit <sha> (the t0 sha 08455efa: the RED)
# Exit 0 when every case reads as the rule expects in all three readers. Writes only a scratch folder (deleted at exit).
cd "$(git rev-parse --show-toplevel)" || exit 1
at=""; [ "$1" = --at ] && at="$2"
tmp=$(mktemp -d "${TMPDIR:-/tmp}/n70-m3.XXXXXX") || exit 1
trap 'rm -rf "$tmp"' EXIT
mkdir -p "$tmp/bin" && cp Tools/night/n70-m3-dotnet.sh "$tmp/bin/dotnet" && chmod +x "$tmp/bin/dotnet"
if [ -n "$at" ]; then
  git show "$at:Tools/night/n65-verbs.sh" > "$tmp/n65-verbs.sh" && git show "$at:Tools/night/n70-gate-pack.cjs" > "$tmp/n70-gate-pack.cjs" && git show "$at:Tools/night/n70-next.cjs" > "$tmp/n70-next.cjs" || { echo "n70-m3-cases: no readers at $at"; exit 2; }
  reader="$tmp/n65-verbs.sh"; pack="$tmp/n70-gate-pack.cjs"; next="$tmp/n70-next.cjs"; label="the readers at $at"
else
  reader=Tools/night/n65-verbs.sh; pack=Tools/night/n70-gate-pack.cjs; next=Tools/night/n70-next.cjs; label="this tree's readers ($(git rev-parse --short HEAD) + the working copy)"
fi
P=Research/2026-10-25-wave70/T; S=Research/2026-10-25-forge70/spike
# replay file | chart file (or none) | room | expected | why
cases=(
  "Logs/n69/r1-kiln/verbs-launch.txt|$S/throws-kb3-n69r1d.txt|kb3|PASS|NIGHT69 a1's kb3 r1d: its one probe (seed 1) burst on the queue, no pit: a1 read FAIL; its 20-seed chart holds 14 / 20"
  "Logs/n69/r1-spire/verbs-launch.txt|$S/throws-v2-n69s1b.txt|v2|PASS|NIGHT69 a1's v2 s1b: its one probe blocked at 0.3 m (kills 3, no pit): a1 read FAIL; its chart holds 20 / 20 (15 in the crypt)"
  "$P/Z1-m3-planted.txt|$S/throws-kb3-shipped.txt|kb3|FAIL|the shipped kb3's chart 7 / 20 under a planted probe that fell into the quench (one seed, one sample)"
  "Logs/n68/r2-spire/verbs-launch.txt|none|v2|FAIL|NIGHT68 r2's v2: GREEN, in a pit YES, and no throw chart"
  "$P/Z1-m3-planted.txt|$P/Z1-chart-P6.txt|P6|FAIL|planted: 9 GREEN candidates + 11 RED probes whose bursts killed 3 (a RED probe's burst never counts: 9 / 20)"
  "$P/Z1-m3-planted.txt|$P/Z1-chart-P7.txt|P7|PASS|planted: exactly 10 candidates (pit, lava, kills 2) beside Y2's non-candidates (RED in a pit, GREEN launch36 0 in a pit, kills 1)"
  "$P/Z1-m3-planted.txt|$P/Z1-chart-P8.txt|P8|FAIL|planted: P6's probes under a summary line that claims 20 / 20"
  "$P/Z1-m3-planted.txt|$P/Z1-chart-P9.txt|P9|FAIL|planted: 3 / 3 GREEN pit probes, a chart over 3 seeds (D-3 reads 20)"
  "Logs/n69/r1-kiln/verbs-launch.txt|$S/throws-v2-n69s1b.txt|kb3|FAIL|another room's chart (v2 s1b's 20 / 20) saved as kb3's"
)
echo "n70-m3-cases: the launch M3 readers on saved runs and throw charts ($label)"
ok=0; n=0; i=0
for c in "${cases[@]}"; do
  IFS='|' read -r f chart room want why <<< "$c"
  i=$((i + 1)); cd_="$tmp/c$i"; mkdir -p "$cd_"; ch="none"
  [ "$chart" != none ] && { cp "$chart" "$cd_/throws-$room.txt"; ch="$cd_/throws-$room.txt"; }
  line=$(N69_M3_REPLAY="$f" N70_THROWS_DIR="$cd_" PATH="$tmp/bin:$PATH" N65_VERBS_TIMEOUT=60 bash "$reader" launch planted "$room" 2>&1 | grep -E "^n65-verbs launch M3: $room " | head -1)
  got=$(echo "$line" | grep -o -E "M3: $room (PASS|FAIL)" | awk '{print $3}'); [ -z "$got" ] && got="none"
  pline=$(node Tools/night/n70-m3-pack-case.cjs "$pack" "$f" "$room" "$ch" 2>&1); pgot=$(echo "$pline" | awk '{print $3}')
  # n70-next.cjs on a planted root: the room a launch room of level kiln with one round r1-kiln
  R="$tmp/r$i"; mkdir -p "$R/Logs/n70/r1-kiln"
  echo "{\"slices\":[{\"id\":\"$room\",\"level\":\"kiln\",\"verb\":\"launch\",\"mainBar\":1.25,\"state\":\"measuring\"}]}" > "$R/Logs/n70/slate.json"
  echo "n65verb skip: M2 VERB PASS | room $room, verb launch (planted)" > "$R/Logs/n70/r1-kiln/verbs-skip.txt"
  echo "$line" > "$R/Logs/n70/r1-kiln/verbs.md"
  [ "$chart" != none ] && cp "$chart" "$R/Logs/n70/r1-kiln/throws-$room.txt"
  nline=$(node "$next" --root "$R" 2>&1 | grep -E "^$room +launch" | head -1); ngot=$(echo "$nline" | grep -o -E "(ok|--) M3" | awk '{print $1}')
  case "$ngot" in ok) ngot=PASS ;; --) ngot=FAIL ;; *) ngot=none ;; esac
  n=$((n + 3)); for g in "$got" "$pgot" "$ngot"; do [ "$g" = "$want" ] && ok=$((ok + 1)); done
  verdict=$([ "$got" = "$want" ] && [ "$pgot" = "$want" ] && [ "$ngot" = "$want" ] && echo OK || echo WRONG)
  echo "  case $i $room ($why; $f, chart $chart): n65-verbs.sh $got, the GATE pack $pgot, n70-next $ngot (expected $want: $verdict)"
  echo "      n65-verbs.sh: ${line:-no M3 line}"
  echo "      the pack:     $pline"
  echo "      n70-next:     $(echo "$nline" | grep -o -E '(ok|--) M3' || echo 'no M3 piece')"
done
echo "n70-m3-cases: $ok / $n reader readings as the rule expects$([ $ok = $n ] && echo ' (GREEN)' || echo ' (RED)')"
[ $ok = $n ]
