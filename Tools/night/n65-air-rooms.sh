#!/usr/bin/env bash
# NIGHT65 builder T: air65 (Tools/night/n65-air.cjs) over a plan's six fight-room slots (or the rooms given), then a one-line table.
#   bash Tools/night/n65-air-rooms.sh <plan.json | level.json> [a2,a3,...]
cd "$(git rev-parse --show-toplevel)" || exit 1
plan="${1:?plan}"; rooms="${2:-a2,a3,a4,a5,b1,a8}"
sum=()
for r in ${rooms//,/ }; do
  out=$(node Tools/night/n65-air.cjs "$plan" "$r" 2>&1)
  echo "$out"
  echo
  sum+=("$(echo "$out" | grep "^n65-air $r:")")
done
echo "== air65 table ($plan)"
printf '%s\n' "${sum[@]}"
