#!/usr/bin/env bash
# NIGHT69 builder T, slice Y2 (Logs/n69/slate.json Y2; scout.md X1): the launch M3 readers' planted cases. Both readers, Tools/night/n65-verbs.sh
# (its "n65-verbs launch M3: <id> PASS|FAIL" line) and the GATE pack's launchM3 (Tools/night/n69-gate-pack.cjs), read SAVED launch runs: the
# NIGHT68 r2 lines (kb3's RED "launch36 0 ... in a pit YES", kb3d's GREEN with no pit, v2's GREEN control), the Y2 probe's own runs, and the
# planted blocks Research/2026-10-25-wave69/T/Y2-m3-planted.txt. n65-verbs.sh runs unmodified with a stand-in `dotnet` first on PATH
# (Tools/night/n69-m3-dotnet.sh: replays the saved block, builds nothing); the pack's function is run by Tools/night/n69-m3-pack-case.cjs.
# The rule (the slate's Y2): M3 PASSes only on a `: GREEN` line with `launch36 [1-9]` and "in a pit YES" or "in lava YES".
#   bash Tools/night/n69-m3-cases.sh [--at <sha>]     this tree's readers, or the readers of commit <sha> (the t0 sha d9e22d48: the RED)
# Exit 0 when every case reads as the rule expects in both readers. Writes only a scratch folder (deleted at exit).
cd "$(git rev-parse --show-toplevel)" || exit 1
at=""; [ "$1" = --at ] && at="$2"
tmp=$(mktemp -d "${TMPDIR:-/tmp}/n69-m3.XXXXXX") || exit 1
trap 'rm -rf "$tmp"' EXIT
mkdir -p "$tmp/bin" && cp Tools/night/n69-m3-dotnet.sh "$tmp/bin/dotnet" && chmod +x "$tmp/bin/dotnet"
if [ -n "$at" ]; then
  git show "$at:Tools/night/n65-verbs.sh" > "$tmp/n65-verbs.sh" && git show "$at:Tools/night/n69-gate-pack.cjs" > "$tmp/n69-gate-pack.cjs" || { echo "n69-m3-cases: no readers at $at"; exit 2; }
  reader="$tmp/n65-verbs.sh"; pack="$tmp/n69-gate-pack.cjs"; label="the readers at $at"
else
  reader=Tools/night/n65-verbs.sh; pack=Tools/night/n69-gate-pack.cjs; label="this tree's readers ($(git rev-parse --short HEAD) + the working copy)"
fi
P=Research/2026-10-25-wave69/T
cases=(
  "Logs/n68/r2-kiln/verbs-launch.txt|kb3|FAIL|NIGHT68 r2's kb3: RED, launch36 0, in a pit YES (the target, never thrown; verbs-launch.txt:21)"
  "Logs/n68/r2-kiln/verbs-launch.txt|kb3d|FAIL|NIGHT68 r2's kb3d: GREEN, launch36 1, in a pit no, in lava no"
  "Logs/n68/r2-spire/verbs-launch.txt|v2|PASS|NIGHT68 r2's v2 (the control): GREEN, launch36 1, in a pit YES (verbs-launch.txt:11)"
  "$P/Y2-probe-kb3-GREEN.txt|kb3|FAIL|the fixed probe on r2's kb3: launch36 0, in a pit n/a (nothing launched)"
  "$P/Y2-probe-v2-GREEN.txt|v2|PASS|the fixed probe on r2's v2: launch36 1, launched body husk #2 (the target: yes), in a pit YES"
  "$P/Y2-m3-planted.txt|P1|FAIL|planted: GREEN, launch36 1, in a pit no, in lava no"
  "$P/Y2-m3-planted.txt|P2|FAIL|planted: RED, launch36 1, in a pit YES"
  "$P/Y2-m3-planted.txt|P3|PASS|planted: GREEN, launch36 2, in lava YES"
  "$P/Y2-m3-planted.txt|P4|FAIL|planted: the fixed probe's nothing launched (n/a)"
  "$P/Y2-m3-planted.txt|P5|FAIL|planted: GREEN printing launch36 0 with in a pit YES"
)
echo "n69-m3-cases: the launch M3 readers on saved runs ($label)"
ok=0; n=0
for c in "${cases[@]}"; do
  IFS='|' read -r f room want why <<< "$c"
  line=$(N69_M3_REPLAY="$f" PATH="$tmp/bin:$PATH" N65_VERBS_TIMEOUT=60 bash "$reader" launch planted "$room" 2>&1 | grep -E "^n65-verbs launch M3: $room " | head -1)
  got=$(echo "$line" | grep -o -E "M3: $room (PASS|FAIL)" | awk '{print $3}'); [ -z "$got" ] && got="none"
  pline=$(node Tools/night/n69-m3-pack-case.cjs "$pack" "$f" "$room" 2>&1); pgot=$(echo "$pline" | awk '{print $3}')
  n=$((n + 2)); [ "$got" = "$want" ] && ok=$((ok + 1)); [ "$pgot" = "$want" ] && ok=$((ok + 1))
  verdict=$([ "$got" = "$want" ] && [ "$pgot" = "$want" ] && echo OK || echo WRONG)
  echo "  case $room ($why; $f): n65-verbs.sh $got, the GATE pack $pgot (expected $want: $verdict)"
  echo "      n65-verbs.sh: ${line:-no M3 line}"
  echo "      the pack:     $pline"
done
echo "n69-m3-cases: $ok / $n reader readings as the rule expects$([ $ok = $n ] && echo ' (GREEN)' || echo ' (RED)')"
[ $ok = $n ]
