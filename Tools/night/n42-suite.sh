#!/usr/bin/env bash
# NIGHT42 (the lead): the whole EditMode suite IN the live Editor, in batches each under the tool's 600 s timeout (NIGHT41's M and N
# batches passed it and P-Z gave no result). Same rules as n41-suite.sh: a batch is a name prefix (run_tests takes one substring); a
# no-JSON batch is re-run once alone; never in Play mode, never beside a build or a reel. Core by letter (13 min in NIGHT41); Game by
# letter with M split into M9 / MapC / MapP / Me / Mo, N into N1 / N2 / N3 / N4, S and T split by their second letters.
#   bash Tools/night/n42-suite.sh <tag> [Core|Game] [batch ...]    e.g.  bash Tools/night/n42-suite.sh freeze   |   ... r2 Game MapC N4
# Output: Logs/n42-suite-<tag>.txt (one line per batch + every failure), and the totals at the end.
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"; only="${2:-}"; shift 2 2>/dev/null
pick=("$@")
out="Logs/n42-suite-$tag.txt"
echo "== n42-suite $tag start $(date +%T) (only=${only:-all} batches=${pick[*]:-all})" >> "$out"
P=0; T=0; F=0
run() { # $1 = filter prefix
  local r; r=$(bash Tools/unity-loop.sh tests "$1" 2>&1)
  if echo "$r" | grep -q "no JSON" || { echo "$r" | grep -q " FAIL " && ! echo "$r" | grep -q " FAIL .*: ."; }; then
    echo "   retry $1 (no JSON / empty failure message)" >> "$out"; r=$(bash Tools/unity-loop.sh tests "$1" 2>&1)
  fi
  local line; line=$(echo "$r" | grep "^tests:")
  echo "$(date +%T) $1: ${line:-$(echo "$r" | head -c 300)}" >> "$out"
  echo "$r" | grep " FAIL " >> "$out"
  local p t f; p=$(echo "$line" | awk '{print $2}'); t=$(echo "$line" | awk '{print $4}'); f=$(echo "$line" | awk '{print $6}')
  case "$p$t$f" in *[!0-9]*|"") echo "   (no totals for $1: not counted)" >> "$out"; return ;; esac
  P=$((P + p)); T=$((T + t)); F=$((F + f))
}
CORE="A B C D E F G H I J K L M N O P Q R S T U V W X Y Z"
GAME="A B C D E F G H I J K L M9 MapC MapP Me Mo N1 N2 N3 N4 O P R Sa Se Sh Sk Sl Sm Sp St Ta Tel Tex To Tr U V W Z"
for ns in Core Game; do
  [ -n "$only" ] && [ "$only" != "$ns" ] && continue
  dir="Assets/Tests/EditMode"; [ "$ns" = Game ] && dir="Assets/Tests/EditMode/Game"
  list=$CORE; [ "$ns" = Game ] && list=$GAME
  [ ${#pick[@]} -gt 0 ] && list="${pick[*]}"
  for b in $list; do
    ls "$dir"/"$b"*.cs >/dev/null 2>&1 || continue
    run "Skygrind.$ns.Tests.$b"
  done
done
echo "== n42-suite $tag end $(date +%T): $P / $T passed, $F failed" >> "$out"
tail -1 "$out"
