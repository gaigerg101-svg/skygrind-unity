#!/usr/bin/env bash
# NIGHT41 (the lead): one round's Player build in the live Editor (scenes Survival then Arena, passed explicitly: GG's modified
# EditorBuildSettings.asset is never relied on), polled to the end (wait-build.sh), then the drift a build leaves: ProjectSettings (CLAUDE.md:
# commit only what is understood) and the URP Mobile asset rewrite (memory: urp-mobile-asset-rewrite). Refuses under 10 GB free or in Play.
#   bash Tools/night/n41-build.sh <tag>        e.g. bash Tools/night/n41-build.sh n41-r1
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"
free=$(df -BG /c | tail -1 | awk '{print $4}' | tr -d G)
[ "${free:-0}" -lt 10 ] && { echo "build $tag REFUSED: C: has ${free} GB free (< 10)"; exit 1; }
st=$(bash Tools/unity-loop.sh status 2>&1)
echo "$st" | grep -q '"playMode": "stopped"' || { echo "build $tag REFUSED: the Editor is not stopped: $st"; exit 1; }
echo "build $tag start $(date +%T) at $(git log --oneline -1 | cut -c1-60) (C: ${free} GB free)"
unity command build --target StandaloneWindows64 --outputPath "Builds/$tag/SKYGRIND.exe" \
  --scenes '["Assets/Scenes/Survival.unity","Assets/Scenes/Arena.unity"]' --confirm true \
  --caller plugin --skill unity-cli --no-banner --format json --result-only 2>&1 | head -c 400; echo
bash Tools/night/wait-build.sh
ls -la "Builds/$tag/SKYGRIND.exe" 2>&1 | cut -c1-120
echo "drift after the build:"; git status --short ProjectSettings Assets/Settings "Assets/TextMesh Pro" 2>/dev/null
