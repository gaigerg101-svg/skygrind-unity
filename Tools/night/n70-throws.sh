#!/usr/bin/env bash
# FORGE70 (the forge's spike tool, L-151: "measure the verb's real effect on the shipped room before a card sizes a piece"): the launch probe
# (`n65verb launch`, Tools/SimHarness/Harness.N65.Verb.cs) on SEEDS seeds of one room, one throw per seed from line65[0] at wave 1's nearest
# body, and a summary: GREEN (punched and launched), the flight end, thrown metres, bursts and their kills, in a pit / in lava, and the
# M3 candidate count (DECIDE70 D-3: a pit or lava end, OR a burst that killed >= 2 incl. the flier). Read-only on the tree; one dotnet job.
#   bash Tools/night/n70-throws.sh <level.json | level id> <room> <out.txt> [seeds, default 20]
#   e.g. bash Tools/night/n70-throws.sh n57-kiln kb3 Logs/n70/throws-kb3-red.txt
cd "$(git rev-parse --show-toplevel)" || exit 1
level="${1:?level}"; room="${2:?room}"; out="${3:?out}"; n="${4:-20}"
case "$level" in *.json) ;; *) level="Design/levels/$level.json" ;; esac   # a level id is the shipped file (the probe needs a path)
[ -f "$level" ] || { echo "n70-throws: no level file $level"; exit 1; }
export MSBUILDDISABLENODEREUSE=1
dotnet build Tools/SimHarness -c Release -nologo -v q > /dev/null 2>&1 || { echo "n70-throws: SimHarness build failed"; exit 1; }
mkdir -p "$(dirname "$out")"
{
  echo "# command: bash Tools/night/n70-throws.sh $level $room $out $n"
  echo "# sha: $(git rev-parse --short HEAD); time: $(date '+%F %T')"
  for s in $(seq 1 "$n"); do
    echo "## seed $s"
    timeout 120 dotnet Temp/SimHarness/bin/Release/net10.0/SimHarness.dll n65verb launch --level "$level" --room "$room" --seed "$s" 2>&1 \
      | grep -E '^n65verb launch|^  burst at|^  target |^  punch:' | cut -c1-600
  done
} > "$out"
# the summary (each verdict line: "n65verb launch: GREEN|RED | room R | launch36 N, flight end E thrown T m (max M m), bursts B (hits H, kills K) | ... in a pit YES|no, in lava YES|no")
# NIGHT70 Z1: the summary's one copy is Tools/night/n70-throws-sum.cjs (the same rule and the same line; the M3 readers re-count with it)
node Tools/night/n70-throws-sum.cjs --append "$out"
