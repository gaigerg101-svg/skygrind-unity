#!/usr/bin/env bash
# NIGHT26 builder B: dn-check pointed at builder B's worktree (a plain scratch directory, no git).
REPO=/c/Users/gaige/dev/SKYGRIND-unity/.claude/worktrees/agent-a7ff88573c0363007
SP=/c/Users/gaige/AppData/Local/Temp/claude/C--Users-gaige-dev-SKYGRIND-unity/ada2581b-e1bb-4a37-9115-078a5b594903/scratchpad/chkb
mkdir -p "$SP/Assets"
cd "$REPO" || exit 1
for d in Scripts Tests Editor; do rm -rf "$SP/Assets/$d"; cp -r "Assets/$d" "$SP/Assets/$d"; done
node Tools/wt-csproj.cjs "$(cygpath -w "$SP")" >/dev/null
cd "$SP" || exit 1
total=0
for p in Skygrind.Game.csproj Skygrind.Game.Tests.csproj Skygrind.Core.Tests.csproj Assembly-CSharp-Editor.csproj; do
  out=$(dotnet build "$p" -nologo -v q 2>&1 | grep -E "error CS" | sed -e 's|.*chkb.||' -e 's| \[.*||' | sort -u)
  n=$(echo "$out" | grep -c "error CS")
  echo "== $p: $n error(s)"
  [ "$n" -gt 0 ] && echo "$out" | head -${MAXERR:-25}
  total=$((total + n))
done
echo "dn-check total errors: $total"
