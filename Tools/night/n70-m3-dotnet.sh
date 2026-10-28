#!/usr/bin/env bash
# NIGHT70 builder T, slice Y2: a stand-in `dotnet` for Tools/night/n70-m3-cases.sh (copied there as <scratch>/bin/dotnet, first on PATH), so an
# M3 reader (Tools/night/n65-verbs.sh, this tree's or a commit's) reads a SAVED launch run instead of a new one: `dotnet build ...` does nothing
# (exit 0); `dotnet <dll> n65verb launch ... --room <id> ...` prints that room's saved block from the file $N69_M3_REPLAY (the pilot's own lines:
# from the block before its header to its `n65verb: exit N`; the reader's own `n65-verbs ...` lines and `#` lines left out) and exits as it did.
[ "$1" = build ] && exit 0
room=""; prev=""
for a in "$@"; do [ "$prev" = --room ] && room="$a"; prev="$a"; done
[ -n "$N69_M3_REPLAY" ] && [ -f "$N69_M3_REPLAY" ] || { echo "n70-m3-dotnet: no N69_M3_REPLAY file"; exit 3; }
awk -v room="$room" '
  /^#/ || /^n65-verbs / { next }
  { buf = buf $0 "\n" }
  /^n65verb [a-z]+: level / { if ($0 ~ ("room " room "[ ,\"]")) mine = 1 }
  /^n65verb: exit/ { if (mine) { printf "%s", buf; found = 1; code = $3 + 0 } buf = ""; mine = 0 }
  END { if (!found) { print "n70-m3-dotnet: no saved block for room " room; exit 3 } exit code }
' "$N69_M3_REPLAY"
