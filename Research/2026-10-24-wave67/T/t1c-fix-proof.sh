#!/usr/bin/env bash
# NIGHT67 builder T, the T1c fix's planted cases (Logs/n67/verdict-T1c.md problems 1-6): (1) a dry round (iceline g8, verb65 ride PLANTED in
# T's tree, = the dry slate's) that records its room files; (2) the pack with the planted file restored (the working tree differs from the
# round's record): REFUSED, both hashes; (3) the pack with the file as the round ran it (re-planted, the same bytes): UNCHANGED, the fresh
# skip on the round's slate snapshot with the build recorded, deco on the round's file, MISSING card / red (none in T's tree) -> exit 3 and
# the README's first line; (4) the fallback: the pack on an older round with no record (dry-pack2-iceline, --no-skip); (5) the launch M3
# tally: n65-verbs.sh launch on n57-kiln kb3 (the pilot's GREEN, in a pit no, in lava no -> RED). Output: t1c-fix-proof.txt
cd "$(git rev-parse --show-toplevel)" || exit 1
E=Research/2026-10-24-wave67/T; f=$E/t1c-fix-proof.txt; rf=Design/levels/n67/iceline/rooms/g8.json; TAG=dry-rec-iceline
trap 'git checkout -- "$rf" 2>/dev/null' EXIT
pk() { L67=iceline timeout 1800 node Tools/night/n67-gate-pack.cjs "$@"; echo "# exit $?"; }
{
  echo "# sha $(git rev-parse --short HEAD) (+ uncommitted: $(git status --short -uno Tools | tr '\n' ' ')); $(date '+%F %T')"
  echo "## (1) PLANTED $rf verb65 ride; L67=iceline ROOMS=g8 SLATE=$E/t1b-dry-slate.json bash Tools/night/n67-round.sh $TAG"
  node $E/t1b-plant-verb65.cjs "$rf" ride
  L67=iceline ROOMS=g8 SLATE=$E/t1b-dry-slate.json timeout 3600 bash Tools/night/n67-round.sh "$TAG" | grep -E "^n67-round"; echo "# exit ${PIPESTATUS[0]}"
  echo "# the record: $(cat Logs/n67/$TAG/room-files.sha256 | tr '\n' ' ')"
  git checkout -- "$rf"; echo "# restored $rf (the working tree now differs from the round's record)"
  echo; echo "## (2) PLANTED change: the pack with the room file restored (not the file the round ran)"
  pk g8 "$TAG" --slate $E/t1b-dry-slate.json --out Logs/n67/gate/g8-rec-changed --key Logs/n67/gate/keys/g8-rec-changed.txt
  echo "# pack folder left: $(ls -d Logs/n67/gate/g8-rec-changed 2>/dev/null || echo none)"
  echo; echo "## (3) the room file as the round ran it (re-planted, the same bytes): the pack runs (no --slate: the round's snapshot gives the bar)"
  node $E/t1b-plant-verb65.cjs "$rf" ride > /dev/null
  pk g8 "$TAG" --out Logs/n67/gate/g8-rec --key Logs/n67/gate/keys/g8-rec.txt
  git checkout -- "$rf"
  echo "# README first line: $(head -1 Logs/n67/gate/g8-rec/README.md)"
  echo "# room-unchanged.txt: $(tail -1 Logs/n67/gate/g8-rec/room-unchanged.txt)"
  echo "# verbs65-skip.txt head:"; head -6 Logs/n67/gate/g8-rec/verbs65-skip.txt | sed 's/^/    /' | cut -c1-260
  echo "# deco.txt head: $(head -1 Logs/n67/gate/g8-rec/deco.txt | cut -c1-220)"; grep -m1 "^n67-deco: \(PASS\|FAIL\)" Logs/n67/gate/g8-rec/deco.txt
  echo "# open-findings.md: $(tail -1 Logs/n67/gate/g8-rec/open-findings.md)"
  echo "# levelgen-report.md first line: $(head -1 Logs/n67/gate/g8-rec/levelgen-report.md | cut -c1-260)"
  echo; echo "## (4) FALLBACK: the pack on dry-pack2-iceline (a round with no room-files record), --no-skip"
  pk g8 dry-pack2-iceline --slate $E/t1b-dry-slate.json --no-skip --out Logs/n67/gate/g8-fallback --key Logs/n67/gate/keys/g8-fallback.txt
  echo "# room-unchanged.txt: $(tail -1 Logs/n67/gate/g8-fallback/room-unchanged.txt 2>/dev/null | cut -c1-300)"
  echo; echo "## (5) the launch M3 tally: bash Tools/night/n65-verbs.sh launch Design/levels/n57-kiln.json kb3"
  bash Tools/night/n65-verbs.sh launch Design/levels/n57-kiln.json kb3 2>&1 | grep -E "^n65verb launch: (GREEN|RED)|^n65-verbs " | cut -c1-260
  echo "# done $(date +%T)"
} > "$f" 2>&1
cut -c1-300 "$f"
