#!/usr/bin/env bash
# NIGHT74 builder S: the verbs37 reel's wall pick (rows 9-11: Bot/VerbsWall39.Pick, re-performed headless by `n39views wall`) on one or more level files.
# The Player's verbs37 reads RED when a new room's wall becomes the pick and its ride is short (NIGHT71: kb2c r4 1.10 s; v2 / v5 0.00 s).
#   bash Research/2026-10-27-wave74/S/run-wall.sh <outFile> <label>=<level.json> [...]
cd "$(git rev-parse --show-toplevel)" || exit 1
OUT="$1"; shift
sha=$(git rev-parse --short HEAD)
{
  echo "# command: dotnet Temp/SimHarness/bin/Release/net10.0/SimHarness.dll n39views wall <level>   (the reel's rows 9-11 wall pick, re-performed headless; row 9 PASS needs a ride over 1.2 s)"
  echo "# sha: $sha; time: $(date '+%F %T')"
  for a in "$@"; do
    echo "## ${a%%=*}  (${a#*=})"
    dotnet Temp/SimHarness/bin/Release/net10.0/SimHarness.dll n39views wall "${a#*=}" 2>&1 | sed -n '1,12p'
  done
} > "$OUT"
