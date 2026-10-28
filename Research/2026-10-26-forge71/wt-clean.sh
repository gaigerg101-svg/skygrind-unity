#!/usr/bin/env bash
# GG 2026-10-26 (FORGE71): remove clean worktrees whose last commit is older than NIGHT70 (before 2026-10-25), no --force.
# Keeps every branch; skips a detached worktree whose HEAD no branch or tag contains; skips the main tree and n70-*.
# Usage: wt-clean.sh scan.txt manifest.tsv [--do]   (without --do: dry run, prints what it would remove)
scan="$1"; man="$2"; doit="$3"
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
printf 'date\tbranch\thead\tpath\tresult\n' > "$man"
n=0; skip=0
while IFS=$'\t' read -r d mod unt br wt; do
  [ "$d" = "MISSING" ] && continue
  [[ "$d" < "2026-10-25" ]] || { skip=$((skip+1)); continue; }
  [ "$mod" = "0" ] && [ "$unt" = "0" ] || { skip=$((skip+1)); continue; }
  case "$br" in n70-*) skip=$((skip+1)); continue;; esac
  head=$(git -C "$wt" rev-parse HEAD 2>/dev/null)
  if [ "$br" = "(detached)" ]; then
    refs=$(git for-each-ref --contains "$head" --count=1 --format='%(refname)' refs/heads refs/tags 2>/dev/null)
    [ -n "$refs" ] || { printf '%s\t%s\t%s\t%s\tKEPT unreachable\n' "$d" "$br" "$head" "$wt" >> "$man"; skip=$((skip+1)); continue; }
  fi
  if [ "$doit" = "--do" ]; then
    if out=$(git worktree remove "$wt" 2>&1); then r="removed"; else r="refused: ${out//$'\n'/ }"; fi
  else r="would remove"; fi
  printf '%s\t%s\t%s\t%s\t%s\n' "$d" "$br" "$head" "$wt" "$r" >> "$man"
  n=$((n+1))
done < "$scan"
echo "candidates: $n, skipped: $skip"
