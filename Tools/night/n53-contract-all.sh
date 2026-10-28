#!/usr/bin/env bash
# NIGHT53 (the lead): THE ROOM CONTRACT (Tools/night/n52-contract-all.sh, unchanged: its table and its per-room outputs in Logs/n52/contract)
# plus THE IDENTITY CHECKS (Tools/night/n53-identity.py table: I1-I4 per room, I5 / I7 per level) on the same rooms, in one print.
#   bash Tools/night/n53-contract-all.sh <level:room,room ...>      (env JOBS, SEEDS, TABLE_ONLY as n52-contract-all.sh)
#   ALL18=1 bash Tools/night/n53-contract-all.sh                     the 18 rooms NIGHT52 rebuilt
# Exit 0 only when both tables PASS. Output also in Logs/n53/contract-<stamp>.md.
cd "$(git rev-parse --show-toplevel)" || exit 1
LIST=("$@")
[ -n "$ALL18" ] && LIST=(kiln:kb2,kb2c,kb3 foundry:a3,a5,b1 iceline:g2,g3,g4,g5,g7,g8 spire:v2,v5,v6 span:p1,p3,p5)
[ ${#LIST[@]} -gt 0 ] || { echo "usage: n53-contract-all.sh <level:room,...> (or ALL18=1)"; exit 2; }
mkdir -p Logs/n53
out="Logs/n53/contract-$(date +%H%M%S).md"
{
  echo "# n53-contract-all $(date '+%F %T') on $(git rev-parse --short HEAD): ${LIST[*]}"
  echo ""
  echo "## THE ROOM CONTRACT (n52-contract-all.sh${TABLE_ONLY:+, TABLE_ONLY: the outputs already in Logs/n52/contract})"
  bash Tools/night/n52-contract-all.sh "${LIST[@]}"; c=$?
  echo ""
  echo "## THE IDENTITY CHECKS (n53-identity.py)"
  python -I Tools/night/n53-identity.py table "${LIST[@]}"; i=$?
  echo ""
  echo "n53-contract-all: contract exit $c, identity exit $i"
  exit $(( c != 0 || i != 0 ))
} 2>&1 | tee "$out"
rc=${PIPESTATUS[0]}
echo "(saved $out)"
exit "$rc"
