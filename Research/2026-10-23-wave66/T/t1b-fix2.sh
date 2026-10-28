#!/usr/bin/env bash
# NIGHT66 T1b fix 2 (Logs/n66/verdict-T1b-r2.md): the proofs, each saved by save.sh as t1b-fix2-*.txt.
#   (1) a planted room with no rideable wall BY CONSTRUCTION, by the game's own rule: `--set design.n40.ride40.minWallHeight=99` (every ride
#       entry, attach, draw and dash attach, goes through Mover.Ride40Intent, which refuses a face with under minWallHeight of wall over the
#       feet; climb walls only in a9): the walker with `--refuse none` and with `--refuse ride` (NIGHT65's skip, --v1: both per seed) on the
#       old a2 and a4: bit-identical clear times on 10 / 10 seeds, 0 wall rides, 0 presses refused
#   (2) a3's refused runs on the latest assembly (skip v2): every started ride ended after one step (rides started = steps on a wall)
#   (3) a pierce room (the latest a2): the uses are KESTREL charged shots through >= 2 bodies
#   bash Research/2026-10-23-wave66/T/t1b-fix2.sh <the latest level, C:/...>
cd "$(git rev-parse --show-toplevel)" || exit 1
L="${1:?the latest level}"
S=Research/2026-10-23-wave66/T/save.sh
H="dotnet run --project Tools/SimHarness -c Release --no-build -- n65verb skip"
SET="--set design.n40.ride40.minWallHeight=99"
bash $S t1b-fix2-rideless-a2 $H --v1 --level Design/levels/n52-foundry.json --room a2 --refuse ride --seeds 10 $SET
bash $S t1b-fix2-rideless-a4 $H --v1 --level Design/levels/n52-foundry.json --room a4 --refuse ride --seeds 10 $SET
bash $S t1b-fix2-a3 $H --level "$L" --room a3 --seeds 10 --bars C:/Users/gaige/dev/SKYGRIND-unity/Logs/n66/slate.json
bash $S t1b-fix2-a2-pierce $H --level "$L" --room a2 --seeds 10 --bars C:/Users/gaige/dev/SKYGRIND-unity/Logs/n66/slate.json
echo "== (1) the planted rideless rooms: per seed the walker with nothing refused against the ride refused"
for f in Research/2026-10-23-wave66/T/t1b-fix2-rideless-a2.txt Research/2026-10-23-wave66/T/t1b-fix2-rideless-a4.txt; do
  echo "  $f"
  grep -h "^  seed" "$f" | sed -E 's/.*seed ([0-9]+): verb allowed: cleared (yes|NO) in ([0-9.-]+) s.*wall rides ([0-9]+), STYLE.*ride refused: cleared (yes|NO) in ([0-9.-]+) s.*wall rides ([0-9]+), STYLE.*presses refused ([0-9]+).*/  seed \1: none \3 s (wall rides \4) | ride refused \6 s (wall rides \7), presses refused \8/'
  echo "  identical seeds: $(grep -h '^  seed' "$f" | sed -E 's/.*verb allowed: cleared (yes|NO) in ([0-9.-]+) s.*ride refused: cleared (yes|NO) in ([0-9.-]+) s.*/\2 \4/' | awk '$1==$2' | wc -l) / $(grep -c '^  seed' "$f")"
done
echo "== (2) a3's refused runs: rides started and ended, steps on a wall, per seed"
grep -h -E "^  seed [0-9]+ (refused-in|walker)" Research/2026-10-23-wave66/T/t1b-fix2-a3.txt | grep -o -E "seed [0-9]+ (refused-in|walker)|wall rides [0-9]+|rides started and ended by the away rule [0-9]+|steps on a wall [0-9]+" | paste - - - -
grep -h "n65verb skip: M1\|n65verb skip: totals" Research/2026-10-23-wave66/T/t1b-fix2-a3.txt | cut -c1-400
echo "== (3) a2 (pierce): uses per seed (KESTREL charged shots through >= 2 bodies) and the bodies per KESTREL charged shot"
grep -h -E "^  seed [0-9]+ verb-aware" Research/2026-10-23-wave66/T/t1b-fix2-a2-pierce.txt | grep -o -E "seed [0-9]+ verb-aware|pierces \(charged shots through >= 2 bodies\) [0-9]+|charged [0-9]+(, bodies hit per KESTREL charged shot [0-9/]+)?" | paste - - -
grep -h "n65verb skip: M1\|n65verb skip: totals" Research/2026-10-23-wave66/T/t1b-fix2-a2-pierce.txt | cut -c1-400
