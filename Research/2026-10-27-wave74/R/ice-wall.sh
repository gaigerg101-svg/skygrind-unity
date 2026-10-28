#!/usr/bin/env bash
# NIGHT74 R: the Player's verbs37 wall pick (S's wallprobe, zones opened) on the iceline with the lead's ON rooms (S's g4, g1, g6 as on port/stage3)
# plus the rooms R is adding, one reading per list. S's room files are copied from the scratchpad over the tree's g1 / g4 / g6 for the generation only
# and put back with `git checkout --` (those three files are never modified or committed here).
#   bash Research/2026-10-27-wave74/R/ice-wall.sh <outFile> <list> [<list> ...]       lists like g4,g1,g6   g4,g1,g6,g3
cd "$(git rev-parse --show-toplevel)" || exit 1
OUT="$1"; shift
SC="C:/Users/gaige/AppData/Local/Temp/claude/C--Users-gaige-dev-SKYGRIND-unity/07ef7707-bc3a-4541-8901-f98169a14875/scratchpad/ice"
R=Design/levels/n67/iceline/rooms
for g in g1 g4 g6; do cp "$SC/s-$g.json" "$R/$g.json"; done
P=Temp/wallprobe/bin/Release/net10.0/wallprobe.dll
{
  echo "# command: node Tools/night/gen-slots.cjs --level iceline --slots=<list> --run --out=<dir>; dotnet $P <level> open [cands]   (S's wallprobe, zones OPENED: the Player's verbs37 pick, rows 9 / 11)"
  echo "# sha: $(git rev-parse --short HEAD) (+ R's g3 / g5 room files as in the tree, S's g4 / g1 / g6 from port/stage3); time: $(date '+%F %T'); row 9 PASS needs a ride over 1.2 s"
  for list in "$@"; do
    tag="w-${list//,/}"
    node Tools/night/gen-slots.cjs --level iceline --slots="$list" --run --out="$SC/$tag" > "$SC/$tag.gen.txt" 2>&1 || { echo "## $list: gen failed"; tail -3 "$SC/$tag.gen.txt" | cut -c1-250; continue; }
    echo "## $list  ($(grep -c . /dev/null) levelgen: $(grep 'levelgen --walk' "$SC/$tag.gen.txt" | tail -1 | cut -c1-80))"
    dotnet "$P" "$SC/$tag/n67-iceline.json" open < /dev/null 2>&1 | sed -n 2,3p | cut -c1-260
    echo "-- rooms with a run of 12 m or more (open):"
    dotnet "$P" "$SC/$tag/n67-iceline.json" open cands < /dev/null 2>&1 | grep -E "^room (g[0-9]+)" | cut -c1-200
  done
} > "$OUT"
for g in g1 g4 g6; do git checkout -- "$R/$g.json"; done
cat "$OUT"
