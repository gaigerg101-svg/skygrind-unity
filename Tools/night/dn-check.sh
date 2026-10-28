#!/usr/bin/env bash
# NIGHT24: compile the Unity-side code WITHOUT the Editor: copy the working tree's Assets/{Scripts,Tests,Editor} into a scratch
# worktree, rewrite the Unity-generated csproj files (Tools/wt-csproj.cjs) and `dotnet build` each. Prints the unique errors.
#   bash Tools/night/dn-check.sh [scratchDir]      (default: the session scratchpad's chk, created from HEAD when missing)
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
SP="${1:-/c/Users/gaige/AppData/Local/Temp/claude/C--Users-gaige-dev-SKYGRIND-unity/ada2581b-e1bb-4a37-9115-078a5b594903/scratchpad/chk}"
if [ ! -d "$SP/.git" ] && [ ! -f "$SP/.git" ]; then git worktree add --detach "$SP" HEAD >/dev/null 2>&1; fi
for d in Scripts Tests Editor; do rm -rf "$SP/Assets/$d"; cp -r "Assets/$d" "$SP/Assets/$d"; done
node Tools/wt-csproj.cjs "$(cygpath -w "$SP")" >/dev/null
cd "$SP" || exit 1
total=0
for p in Skygrind.Game.csproj Skygrind.Game.Tests.csproj Skygrind.Core.Tests.csproj Assembly-CSharp-Editor.csproj; do
  out=$(dotnet build "$p" -nologo -v q 2>&1 | grep -E "error CS" | sed 's#.*/scratchpad/chk/##; s#\\#/#g; s# \[.*##' | sort -u)
  n=$(echo "$out" | grep -c "error CS")
  echo "== $p: $n error(s)"
  [ "$n" -gt 0 ] && echo "$out" | head -${MAXERR:-25}
  total=$((total + n))
done
echo "dn-check total errors: $total"
