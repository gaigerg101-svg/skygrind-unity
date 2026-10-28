#!/usr/bin/env bash
# NIGHT33 W (round 2 item 3): NIGHT32's boss pilot times with design.n33.pierce33 OFF (the repo's config) vs ON (a scratch root whose design config copy has
# the row on). The harness resolves PortExport/ from its working directory (Program.cs Root) and Design/ + Assets/Scripts/Game/UnityTuning.Boss30.cs beside
# it, so the ON run works from a copy of those; nothing in the repo changes.
# Usage, from the worktree root:  bash Tools/night/n33w-bosspierce.sh <scratchDir> [seeds=5]
set -e
REPO="$(cd "$(dirname "$0")/../.." && pwd)"
SP="${1:?usage: n33w-bosspierce.sh <scratchDir> [seeds]}"
SEEDS="${2:-5}"
cd "$REPO"
dotnet build Tools/SimHarness -c Release -nologo -v q 2>&1 | grep -E " error " || true
DLL="$REPO/Temp/SimHarness/bin/Release/net10.0/SimHarness.dll"
mkdir -p "$SP/Assets/Scripts/Game"
rm -rf "$SP/PortExport" "$SP/Design"
cp -r PortExport "$SP/PortExport"
cp -r Design "$SP/Design"
cp Assets/Scripts/Game/UnityTuning.Boss30.cs "$SP/Assets/Scripts/Game/"
node Tools/night/n33w-row-on.cjs "$SP/Design/unity-config.json" pierce33
for side in OFF ON; do
  if [ "$side" = OFF ]; then dir="$REPO"; else dir="$SP"; fi
  echo "===== pierce33 $side ====="
  (cd "$dir" && dotnet "$DLL" boss32 all --seeds "$SEEDS" 2>&1 | grep -E "^boss32 [a-z]+ ")
  (cd "$dir" && dotnet "$DLL" crown32 --seeds "$SEEDS" --nocamp 2>&1 | grep -E "^crown32 sweep|kit share")
done
