#!/usr/bin/env bash
# NIGHT66 T1b fix (Logs/n66/verdict-T1b.md, the critic's problems 1, 2, 5, 6, 10): the proofs, each saved by save.sh.
#   (1) the ride refused, not the jump: on rooms with no ride face (n52-foundry a2, a4) the plain walker clears in the same time with
#       `--refuse ride` as with `--refuse none` (NIGHT65's skip, `--v1`: its allowed run is the walker with nothing refused, its refused run
#       the walker with the ride refused; both clear times per seed); and a3's refused runs (skip v2 on the latest a3) ride 0 times on 10 / 10
#   (2) the bar's source on the M1 line: the same a3 run with --bars (mainBar null today) and with --bar 1.4
#   bash Research/2026-10-23-wave66/T/t1b-fix.sh <the latest level, C:/...>
cd "$(git rev-parse --show-toplevel)" || exit 1
L="${1:?the latest level}"
S=Research/2026-10-23-wave66/T/save.sh
H="dotnet run --project Tools/SimHarness -c Release --no-build -- n65verb skip"
bash $S t1b-fix-noride-a2 $H --v1 --level Design/levels/n52-foundry.json --room a2 --refuse ride --seeds 10
bash $S t1b-fix-noride-a4 $H --v1 --level Design/levels/n52-foundry.json --room a4 --refuse ride --seeds 10
bash $S t1b-fix-a3 $H --level "$L" --room a3 --seeds 10 --bars C:/Users/gaige/dev/SKYGRIND-unity/Logs/n66/slate.json
bash $S t1b-fix-a3-bar $H --level "$L" --room a3 --seeds 2 --bar 1.4
echo "== the walker, refuse none vs ride, per seed (a2, a4): its own wall rides with nothing refused, both clear times, the steps refused"
for f in Research/2026-10-23-wave66/T/t1b-fix-noride-a2.txt Research/2026-10-23-wave66/T/t1b-fix-noride-a4.txt; do
  echo "  $f"
  grep -h "^  seed" "$f" | sed -E 's/.*seed ([0-9]+): verb allowed: cleared (yes|NO) in ([0-9.-]+) s.*wall rides ([0-9]+), STYLE.*ride refused: cleared (yes|NO) in ([0-9.-]+) s.*wall rides ([0-9]+), STYLE.*presses refused ([0-9]+).*/  seed \1: none \3 s (its wall rides \4) | ride refused \6 s (wall rides \7), steps refused \8/'
done
echo "== a3's refused runs: wall rides per seed"
grep -h -E "^  seed [0-9]+ (refused-in|walker)" Research/2026-10-23-wave66/T/t1b-fix-a3.txt | grep -o -E "seed [0-9]+ (refused-in|walker)|wall rides [0-9]+|stick taken off a wall [0-9]+ steps|rides ended at once by the away rule [0-9]+ steps|steps on a wall [0-9]+" | paste - - - - -
echo "== a3's verb-aware runs: wall rides per seed"
grep -h -E "^  seed [0-9]+ verb-aware" Research/2026-10-23-wave66/T/t1b-fix-a3.txt | grep -o -E "seed [0-9]+ verb-aware|wall rides [0-9]+" | paste - -
grep -h "n65verb skip: M1" Research/2026-10-23-wave66/T/t1b-fix-a3.txt Research/2026-10-23-wave66/T/t1b-fix-a3-bar.txt
