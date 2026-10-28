#!/usr/bin/env bash
# NIGHT67 builder T, T1c line 2's proof: n65-play-build.sh with several levels and --update, on a FAKE source build in a scratch tree
# (BUILDS=<scratch>): no copy of a real build, nothing under Builds/ written. The fake build: SKYGRIND.exe (a stub), the config, two levels
# (n52-iceline, n57-kiln: the tracked files) . The play build takes n67-iceline + n67-kiln (added) in one call, then --update adds
# n67-spire; every source sha256 checked unchanged; the refusals: <dst> exists without --update, --update on a missing <dst>, a level the
# build does not ship without --add-level, an unknown option. Output: t1c-playbuild.txt; the scratch tree removed after (T's own).
cd "$(git rev-parse --show-toplevel)" || exit 1
E=Research/2026-10-24-wave67/T; f=$E/t1c-playbuild.txt
X="${TMPDIR:-/tmp}/t1c-play-$$"; SA=SKYGRIND_Data/StreamingAssets/Design
mkdir -p "$X/src/$SA/levels"
printf 'stub\n' > "$X/src/SKYGRIND.exe"
cp Design/unity-config.json "$X/src/$SA/unity-config.json"
cp Design/levels/n52-iceline.json Design/levels/n57-kiln.json Design/levels/n67-iceline.json "$X/src/$SA/levels/"
s() { sha256sum "$1" | cut -c1-16; }
{
  echo "# sha $(git rev-parse --short HEAD) + uncommitted n65-play-build.sh; the fake source $X/src (config + n52-iceline, n57-kiln, n67-iceline); $(date '+%F %T')"
  echo "## source before: config $(s $X/src/$SA/unity-config.json), n67-iceline $(s $X/src/$SA/levels/n67-iceline.json), files $(ls $X/src/$SA/levels | tr '\n' ' ')"
  echo; echo "## (1) two levels in one call: n67-iceline (the build ships it: replaced) + n67-kiln (added)"
  BUILDS=$X bash Tools/night/n65-play-build.sh src play Design/levels/n67-iceline.json,Design/levels/n67-kiln.json --config Design/unity-config.json --add-level; echo "# exit $?"
  echo "# play: $(ls $X/play/$SA/levels | tr '\n' ' ')"
  echo; echo "## (2) --update: n67-spire added to the same play build"
  BUILDS=$X bash Tools/night/n65-play-build.sh src play Design/levels/n67-spire.json --add-level --update; echo "# exit $?"
  echo "# play: $(ls $X/play/$SA/levels | tr '\n' ' '); n67-spire $(s $X/play/$SA/levels/n67-spire.json) = the tracked $(s Design/levels/n67-spire.json)"
  echo "## source after: config $(s $X/src/$SA/unity-config.json), n67-iceline $(s $X/src/$SA/levels/n67-iceline.json), files $(ls $X/src/$SA/levels | tr '\n' ' ')"
  echo; echo "## (3) refusals"
  BUILDS=$X bash Tools/night/n65-play-build.sh src play Design/levels/n67-spire.json --add-level; echo "# exit $? (dst exists, no --update)"
  BUILDS=$X bash Tools/night/n65-play-build.sh src nothere Design/levels/n67-spire.json --add-level --update; echo "# exit $? (--update, no dst)"
  BUILDS=$X bash Tools/night/n65-play-build.sh src play2 Design/levels/n67-spire.json; echo "# exit $? (a level the build does not ship, no --add-level)"
  BUILDS=$X bash Tools/night/n65-play-build.sh src play3 Design/levels/n67-spire.json --bogus; echo "# exit $? (an unknown option)"
  echo "# done $(date +%T)"
} > "$f" 2>&1
rm -rf "$X"
cut -c1-260 "$f"
