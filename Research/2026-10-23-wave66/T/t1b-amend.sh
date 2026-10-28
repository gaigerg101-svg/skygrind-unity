#!/usr/bin/env bash
# NIGHT66 T1b amendment (the charge rule's reach): before / after on the r2 level and red65's level (the main tree's, read only): skip v2 and
# n46flow --verbs65 seeds 1-10 for a2 and a8, each saved by save.sh as t1b-amend-<tag>-<level>-<what>-<room>.txt. Runs the BUILT harness
# (no build: the "before" tag must run on the build before the change).
#   bash Research/2026-10-23-wave66/T/t1b-amend.sh <tag: before | after>
cd "$(git rev-parse --show-toplevel)" || exit 1
TAG="${1:?before|after}"
S=Research/2026-10-23-wave66/T/save.sh
D="${HARNESS_DLL:-Temp/SimHarness/bin/Release/net10.0/SimHarness.dll}"   # "after" ran from a separate build (-o), so the "before" series kept its DLL
echo "t1b-amend $TAG: the harness $D ($(ls -la --time-style=+%F_%T "$D" | awk '{print $6}'))"
M=C:/Users/gaige/dev/SKYGRIND-unity/Logs/n66
for lv in r2 red65; do
  L="$M/$lv/n65-foundry.json"
  for r in a2 a8; do
    bash $S "t1b-amend-$TAG-$lv-skip-$r" dotnet "$D" n65verb skip --level "$L" --room $r --seeds 10 --bars "$M/slate.json"
    bash $S "t1b-amend-$TAG-$lv-flowV-$r" dotnet "$D" n46flow --level n65-foundry --file "$L" --rooms $r --seeds 10 --first 1 --verbs65
  done
done
for f in Research/2026-10-23-wave66/T/t1b-amend-$TAG-*.txt; do
  echo "== $(basename "$f"): $(grep -h -E 'n65verb skip: M1|SUMMARY' "$f" | head -1 | cut -c1-200)"
  grep -h -E "^ +(a2|a8) +verb65|trigger held" "$f" | head -2 | cut -c1-220
done
