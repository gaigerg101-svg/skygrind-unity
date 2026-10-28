#!/usr/bin/env bash
# NIGHT27: a Development Player from the batch copy without the live Editor.
#   bash Tools/night/n27-batch-build.sh <git sha> <tag>      -> Builds/<tag>/SKYGRIND.exe, log Logs/<tag>-build.log
# Detaches the batch worktree (C:\Users\gaige\dev\SKYGRIND-unity-batch) at <sha>, runs Unity in batch mode, prints the result line.
SHA="${1:?sha}"; TAG="${2:?tag}"
B=/c/Users/gaige/dev/SKYGRIND-unity-batch
cd "$B" || exit 1
git checkout -f --detach "$SHA" 2>&1 | tail -1
# the copy needs the untracked-by-git but required data folders the main checkout syncs (StreamingAssets is tracked; PortExport too)
OUT="C:\\Users\\gaige\\dev\\SKYGRIND-unity\\Builds\\$TAG\\SKYGRIND.exe"
LOG="C:\\Users\\gaige\\dev\\SKYGRIND-unity\\Logs\\$TAG-build.log"
mkdir -p "/c/Users/gaige/dev/SKYGRIND-unity/Builds/$TAG"
START=$(date +%s)
"/c/Program Files/Unity/Hub/Editor/6000.3.7f1/Editor/Unity.exe" -batchmode -nographics -projectPath "C:\\Users\\gaige\\dev\\SKYGRIND-unity-batch" \
  -executeMethod Skygrind.EditorTools.BatchBuild24.Build -buildOut "$OUT" -dev -quit -logFile "$LOG"
echo "unity exit $? after $(( $(date +%s) - START )) s"
grep -E "Build Finished|Build succeeded|Succeeded|BatchBuild|error CS" "/c/Users/gaige/dev/SKYGRIND-unity/Logs/$TAG-build.log" | tail -6
ls -la "/c/Users/gaige/dev/SKYGRIND-unity/Builds/$TAG/SKYGRIND.exe" 2>&1 | tail -1
