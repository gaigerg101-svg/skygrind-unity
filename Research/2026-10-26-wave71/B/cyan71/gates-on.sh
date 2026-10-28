#!/usr/bin/env bash
# Scratch (builder B, NIGHT71): the gates at HEAD with the rows OFF, then the fingerprints with B's four rows ON (render-only check), then
# the configs restored. Run from the worktree root.
cd /c/Users/gaige/dev/SKYGRIND-unity/.claude/worktrees/n71-B || exit 1
echo "# sha: $(git rev-parse --short HEAD)"
echo "# start: $(date '+%F %T')"
echo "## 1. bash Tools/night/n71-gates.sh --no-editor (the rows OFF, as committed)"
bash Tools/night/n71-gates.sh --no-editor 2>&1
echo "## 2. the rows voice71, words71, wyrm71, cyan71 ON in both config copies (node flip-rows.cjs), then the fingerprints"
node /c/Users/gaige/AppData/Local/Temp/claude/n71-B/flip-rows.cjs voice71,words71,wyrm71,cyan71 Design/unity-config.json Assets/StreamingAssets/Design/unity-config.json || exit 1
cmp -s Design/unity-config.json Assets/StreamingAssets/Design/unity-config.json && echo "copies identical (rows ON)"
SIM="dotnet Temp/SimHarness/bin/Release/net10.0/SimHarness.dll"
for o in "" "--off67"; do
  echo "fingerprint (rows ON) $o: $($SIM fingerprint $o 2>&1 | grep 'fingerprint seed')"
done
echo "## 3. restore: git checkout -- the two config copies"
git checkout -- Design/unity-config.json Assets/StreamingAssets/Design/unity-config.json
git diff --stat -- Design/unity-config.json Assets/StreamingAssets/Design/unity-config.json
echo "restored: $(git status --short -- Design/unity-config.json Assets/StreamingAssets/Design/unity-config.json | wc -l) changed files (want 0)"
echo "# end: $(date '+%F %T')"
