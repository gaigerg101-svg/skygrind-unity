#!/usr/bin/env bash
# NIGHT66 T1b: skip v2 on one real room of the LATEST assembly (NIGHT65's final room files at the t0 sha: red65's level), for its wall time
# at 10 seeds and a first look. The level is assembled in this tree, copied to the scratch path given, and the tree's seven generated files
# put back (git checkout of the files the run itself wrote). Bars: the main tree's slate (read only).
#   bash Research/2026-10-23-wave66/T/t1b-latest.sh <scratch dir> <room> [<room> ...]
cd "$(git rev-parse --show-toplevel)" || exit 1
SC="${1:?scratch dir}"; shift
mkdir -p "$SC"
S=Research/2026-10-23-wave66/T/save.sh
GEN=(Assets/StreamingAssets/Design/levels/n65-foundry.json Design/levels/n65-foundry.json Design/levels/n65/foundry.assembly.json Design/levels/n65/foundry.json Design/levels/n65/foundry.plan.json Design/levels/n65/openings65.json Reports/LEVEL-n65-foundry-validate.md)
[ -n "$(git status --short -- "${GEN[@]}")" ] && { echo "t1b-latest: the generated files are dirty before the run: refused"; exit 1; }
node Tools/night/gen-n65-foundry.cjs --slots=latest --run > "$SC/gen-latest.txt" 2>&1 || { echo "t1b-latest: the latest assembly failed ($SC/gen-latest.txt)"; git checkout -- "${GEN[@]}"; exit 1; }
cp Design/levels/n65-foundry.json "$SC/n65-foundry-latest.json"
git checkout -- "${GEN[@]}"
echo "t1b-latest: the latest level at $SC/n65-foundry-latest.json ($(sha256sum "$SC/n65-foundry-latest.json" | cut -c1-12)); the tree's generated files put back: $(git status --short -- "${GEN[@]}" | wc -l) dirty"
for r in "$@"; do
  bash $S "t1b-latest-$r" dotnet run --project Tools/SimHarness -c Release --no-build -- n65verb skip --level "$SC/n65-foundry-latest.json" --room "$r" --seeds 10 --bars C:/Users/gaige/dev/SKYGRIND-unity/Logs/n66/slate.json
done
grep -h "n65verb skip: M1\|n65verb skip: M2\|wall time" Research/2026-10-23-wave66/T/t1b-latest-*.txt | cut -c1-400
