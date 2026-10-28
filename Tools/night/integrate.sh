#!/usr/bin/env bash
# NIGHT20: cherry-pick every builder commit not yet applied (git cherry), builder by builder in the given order; stop at
# the first conflict (the caller resolves it, `git cherry-pick --continue`, and re-runs).
#   bash integrate.sh E N G P H V
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
PICKED=/c/Users/gaige/dev/SKYGRIND-unity/Logs/night47-picked.txt # this night's picks (Logs is not tracked)
# Tonight's builders: fill in each agent id (its worktree branch is worktree-agent-<id>).
declare -A ID=( [W]=ada5ee07ccf909efe [B]=adf7f493062e44e94 [L]=a6ac66b9c7ef4e315 [F]=a1f29ccecca5dc002 [H47]=a8855f05ff2fd33a2 ) # NIGHT47 (22:53)
for b in "$@"; do
  br="worktree-agent-${ID[$b]}"
  list=$(git cherry port/stage3 "$br" | grep '^+' | cut -c3-)
  n=$(echo "$list" | grep -c . )
  echo "== $b ($br): $n to pick"
  for c in $list; do
    if grep -q "^$c" "$PICKED" 2>/dev/null; then continue; fi # picked before (a hand-resolved pick changes the patch id)
    if [ "$(git rev-list --parents -n 1 "$c" | wc -w)" -gt 2 ]; then echo "  skip merge $c"; echo "$c $b merge" >> "$PICKED"; continue; fi # a builder's merge of port/stage3: nothing new
    if ! git cherry-pick "$c" >/dev/null 2>&1; then
      echo "CONFLICT at $b $c: $(git log -1 --format=%s $c | cut -c1-120)"
      git status --short | grep -E '^(UU|AA|DU|UD|AU|UA) '
      exit 2
    fi
    echo "$c $b" >> "$PICKED"
    echo "  picked $(git log -1 --format='%h %s' | cut -c1-110)"
  done
done
echo "integrate: done $(date +%H:%M:%S)"
