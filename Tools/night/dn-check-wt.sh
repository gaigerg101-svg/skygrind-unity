#!/usr/bin/env bash
# NIGHT29: dn-check for ANY worktree (no Editor). Usage, from inside your worktree:
#   bash Tools/night/dn-check-wt.sh <scratchDir>       (scratchDir: a plain directory under your own scratchpad, created if missing)
# Copies this worktree's Assets/{Scripts,Tests,Editor} into the scratch dir, rewrites the csproj files and `dotnet build`s each; prints the unique errors.
REPO="$(cd "$(dirname "$0")/../.." && pwd)"
SP="${1:?usage: dn-check-wt.sh <scratchDir>}"
mkdir -p "$SP/Assets"
cd "$REPO" || exit 1
for d in Scripts Tests Editor; do rm -rf "$SP/Assets/$d"; cp -r "Assets/$d" "$SP/Assets/$d"; done
node Tools/wt-csproj.cjs "$(cygpath -w "$SP")" >/dev/null
cd "$SP" || exit 1
total=0
for p in Skygrind.Game.csproj Skygrind.Game.Tests.csproj Skygrind.Core.Tests.csproj Assembly-CSharp-Editor.csproj; do
  out=$(dotnet build "$p" -nologo -v q 2>&1 | grep -E "error CS" | sed -e 's| \[.*||' -e "s|.*$(basename "$SP")[/\\\\]||" | sort -u) # NIGHT30 (S found it): strip the trailing [project path] FIRST, else the path strip ate "error CS" and every run printed 0
  n=$(echo "$out" | grep -c "error CS")
  echo "== $p: $n error(s)"
  [ "$n" -gt 0 ] && echo "$out" | head -${MAXERR:-25}
  total=$((total + n))
done
echo "dn-check total errors: $total"
