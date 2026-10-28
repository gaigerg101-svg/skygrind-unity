#!/usr/bin/env bash
# NIGHT67 builder T, T1c step 3's proof (slate T1c line 1): the GATE pack on L67. (1) a dry round whose g8 room file carries verb65 ride
# (PLANTED in T's tree, = the dry slate's verb; restored after the round), (2) `L67=iceline n67-gate-pack.cjs g8 <that round>` with the dry
# slate: the pack holds every file the ROOM list's GATE names (its fresh skip v2 on seeds 11-20), (3) the PLANTED mismatch: the same pack
# against a slate whose g8 verb is pierce: refused before anything runs. One dotnet job in this tree. Output: t1c-pack.txt
cd "$(git rev-parse --show-toplevel)" || exit 1
E=Research/2026-10-24-wave67/T; f=$E/t1c-pack.txt; rf=Design/levels/n67/iceline/rooms/g8.json
TAG="${1:-dry-pack2-iceline}"   # a new round tag (a round never overwrites its folder; dry-pack-iceline was stopped part way, 12:00)
trap 'git checkout -- "$rf" 2>/dev/null' EXIT   # the planted room file is restored however the chain ends
{
  echo "# sha $(git rev-parse --short HEAD) (+ uncommitted: $(git status --short -uno Tools | tr '\n' ' ')); $(date '+%F %T')"
  echo "## (1) PLANTED $rf row.verb65 ride; L67=iceline ROOMS=g8 SLATE=$E/t1b-dry-slate.json bash Tools/night/n67-round.sh $TAG"
  node $E/t1b-plant-verb65.cjs "$rf" ride
  L67=iceline ROOMS=g8 SLATE=$E/t1b-dry-slate.json timeout 3600 bash Tools/night/n67-round.sh "$TAG" | grep -E "^n67-round|verb65"; echo "# exit ${PIPESTATUS[0]}"
  git checkout -- "$rf"; echo "# restored $rf: '$(git status --short -- "$rf")' (empty = restored)"
  echo; echo "## (2) L67=iceline node Tools/night/n67-gate-pack.cjs g8 $TAG --slate $E/t1b-dry-slate.json --out Logs/n67/gate/g8-dry --key Logs/n67/gate/keys/g8-dry-spot-key.txt"
  L67=iceline timeout 1800 node Tools/night/n67-gate-pack.cjs g8 "$TAG" --slate $E/t1b-dry-slate.json --out Logs/n67/gate/g8-dry --key Logs/n67/gate/keys/g8-dry-spot-key.txt; echo "# exit $?"
  echo "# the pack: $(ls Logs/n67/gate/g8-dry | tr '\n' ' ')"
  grep -h -E "^n65verb skip: (M1|M2)" Logs/n67/gate/g8-dry/verbs65-skip.txt
  echo; echo "## (3) PLANTED mismatch: the same pack against a slate with g8 verb pierce"
  node $E/t1b-dry-slate.cjs g8:pierce:1.25 kb3:ride:1.25 v6:ride:1.25 > /dev/null; mv $E/t1b-dry-slate.json $E/t1c-pack-slate-pierce.json; node $E/t1b-dry-slate.cjs > /dev/null
  L67=iceline node Tools/night/n67-gate-pack.cjs g8 "$TAG" --slate $E/t1c-pack-slate-pierce.json --out Logs/n67/gate/g8-dry-mismatch --key Logs/n67/gate/keys/g8-dry-mismatch.txt; echo "# exit $? (1 expected: refused); pack folder: $(ls -d Logs/n67/gate/g8-dry-mismatch 2>/dev/null || echo none)"
  echo "# done $(date +%T)"
} > "$f" 2>&1
tail -12 "$f" | cut -c1-260
