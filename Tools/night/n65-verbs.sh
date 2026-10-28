#!/usr/bin/env bash
# NIGHT65 builder T: one verb pilot (Tools/SimHarness/Harness.N65.Verb.cs: launch | pierce | parry | skip) over a level's six fight-room slots
# (or the rooms given), each run's detail and verdict lines in order; the SimHarness is built once first (dotnet run --no-build after).
#   bash Tools/night/n65-verbs.sh <verb> <Design/levels/X.json> [a2,a3,...] [extra n65verb args ...]
cd "$(git rev-parse --show-toplevel)" || exit 1
verb="${1:?verb}"; level="${2:?level}"; rooms="${3:-a2,a3,a4,a5,b1,a8}"; shift 3 2>/dev/null
dotnet build Tools/SimHarness -c Release -nologo -v q > /dev/null 2>&1 || { echo "n65-verbs: SimHarness build failed"; exit 1; }
green=0; red=0; unsure=0
for r in ${rooms//,/ }; do
  # NIGHT66 T1b amendment (builder S: skip v2 sat idle after its whole output on a8, never exiting): the built DLL run directly (one process,
  # so the timeout ends the run itself; `dotnet run`'s child outlives a killed host) under `timeout 900`, its printed lines kept on a timeout
  # FORGE67 red team 5: the timeout from N65_VERBS_TIMEOUT (default 900 s), so a planted hang can be proven at 60 s
  TMO="${N65_VERBS_TIMEOUT:-900}"
  out=$(timeout "$TMO" dotnet Temp/SimHarness/bin/Release/net10.0/SimHarness.dll n65verb "$verb" --level "$level" --room "$r" "$@" 2>&1 | grep -v -E "^  note:|warning (CS|MSB)"; rc=${PIPESTATUS[0]}; [ "$rc" = 124 ] && echo "n65-verbs: TIMEOUT: n65verb $verb $r ran $TMO s and was ended (its output above kept)")
  echo "$out"
  # NIGHT67 T1c fix (Logs/n67/verdict-T1c.md problem 6): the slate's launch M3 is "in a pit yes" or "in lava yes" on the launched body, not
  # the pilot's GREEN: printed per room, and a launch room counts GREEN here only when that M3 passes
  if [ "$verb" = launch ]; then
    vl=$(echo "$out" | grep -E "^n65verb launch( --[a-z-]+)*: (GREEN|RED)" | tail -1)
    # NIGHT69 Y2 (slate Y2; scout.md X1): the probe's own pit / lava reading is printed as "this probe said" (one seed, one sample)
    # NIGHT70 Z1 (slate Z1; DECIDE70 D-3; scout.md B; NIGHT69 a1 read kb3 and v2 FAIL on seed 1 while their 20-seed charts hold 14 / 20 and
    # 20 / 20): M3 is the room's THROW CHART, Tools/night/n70-throws.sh's throws-<room>.txt beside the level file (the round's folder; or
    # $N70_THROWS_DIR), re-counted by Tools/night/n70-throws-sum.cjs: a candidate is a GREEN probe with launch36 1+ AND a pit / lava end or a
    # burst killing >= 2 incl. the flier (a RED probe's burst never counts); PASS at >= 10 of 20; no chart: FAIL "no throw chart"
    chart="${N70_THROWS_DIR:-$(dirname "$level")}/throws-$r.txt"
    mv=$(node Tools/night/n70-throws-sum.cjs --m3 "$chart" --room "$r" 2>&1)
    case "$mv" in "M3 PASS ("*) m3=PASS ;; *) m3=FAIL ;; esac
    m3why="${mv#M3 ???? (}"; m3why="${m3why%%) | n70-throws: *}"; m3why="${m3why%)}"
    echo "n65-verbs launch M3: $r $m3 ($m3why) (the slate's launch M3, DECIDE70 D-3: the room's 20-seed throw chart $chart, PASS at >= 10 / 20 candidates = a GREEN probe, launch36 1 or more, and a pit or lava end or a burst killing >= 2 incl. the flier; the chart: $(echo "$mv" | grep -o -E 'M3 candidates \(D-3\) [0-9]+ / [0-9]+' || echo 'none'); this probe said: $(echo "$vl" | grep -o -E 'launch36 [0-9]+' | head -1), $(echo "$vl" | grep -o -E 'bursts [0-9]+ \(hits [0-9]+, kills [0-9]+\)' || echo 'no bursts printed'), $(echo "$vl" | grep -o -E 'in a pit (YES|no|n/a[^,]*), in lava (YES|no|n/a[^,]*)' || echo 'no verdict'))"
    [ "$m3" = FAIL ] && out=$(echo "$out" | sed -E 's/^(n65verb launch( --[a-z-]+)*): GREEN/\1: GREEN-but-M3-FAIL/')
  fi
  # NIGHT66 T1b: skip v2's verdict is `n65verb skip: M1 PASS|FAIL|UNSURE` (counted as GREEN / RED / UNSURE here)
  # NIGHT67 T1b: the ride pilot's verdict reads `n65verb ride --shoot: GREEN|RED | ...` (a flag between the verb and the colon): counted too
  # NIGHT68 I4 fix (Logs/n68/verdict-I4.md P1): skip v2's verdict is its M2 VERB line (seeds 1+) or M2 FRESH line (seeds 21+); M1 only when
  # the output has neither (an older skip)
  m2re="^n65verb skip: M2 (VERB|FRESH) "
  if echo "$out" | grep -q -E "$m2re"; then pass="M2 (VERB|FRESH) PASS"; uns="M2 (VERB|FRESH) UNSURE"; else pass="M1 PASS"; uns="M1 UNSURE"; fi
  if echo "$out" | grep -q -E "^n65verb $verb( --[a-z-]+)*: (GREEN|$pass)( |$)"; then green=$((green + 1)); elif echo "$out" | grep -q -E "^n65verb $verb( --[a-z-]+)*: (UNSURE|$uns)"; then unsure=$((unsure + 1)); else red=$((red + 1)); fi
done
echo "n65-verbs $verb on $level ($rooms): GREEN $green, RED $red, UNSURE $unsure$([ "$verb" = skip ] && echo " (skip v2: GREEN = M2 VERB PASS, or M2 FRESH PASS on seeds 21+; RED = FAIL or a refusal; NIGHT68 I4 fix)")$([ "$verb" = launch ] && echo " (launch: GREEN = the pilot's GREEN AND the slate's M3, the room's 20-seed throw chart >= 10 / 20: NIGHT70 Z1)")"
