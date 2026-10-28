#!/usr/bin/env bash
# NIGHT27: the EditMode suite WITHOUT the live Editor (it stalled): Unity batch mode on the batch copy (own Library), at <sha>.
#   bash Tools/night/n27-batch-tests.sh <sha> <tag>   -> Logs/<tag>-tests.xml, Logs/<tag>-tests.log, summary on the last lines
SHA="${1:?sha}"; TAG="${2:?tag}"; FILTER="${3:-}"
cd /c/Users/gaige/dev/SKYGRIND-unity-batch || exit 1
git checkout -f --detach "$SHA" 2>&1 | tail -1
START=$(date +%s)
"/c/Program Files/Unity/Hub/Editor/6000.3.7f1/Editor/Unity.exe" -batchmode -nographics -projectPath "C:\Users\gaige\dev\SKYGRIND-unity-batch" \
  -runTests -testPlatform EditMode ${FILTER:+-testFilter "$FILTER"} -testResults "C:/Users/gaige/dev/SKYGRIND-unity/Logs/${TAG}-tests.xml" -logFile "C:/Users/gaige/dev/SKYGRIND-unity/Logs/${TAG}-tests.log"
echo "unity exit $? after $(( $(date +%s) - START )) s"
grep -o '<test-run [^>]*' "/c/Users/gaige/dev/SKYGRIND-unity/Logs/$TAG-tests.xml" | head -1 | cut -c1-400
