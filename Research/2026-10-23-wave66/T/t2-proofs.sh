#!/usr/bin/env bash
# NIGHT66 T2: b2 as the seventh slot. (1) n65-slot-set.cjs: the self-test, then `b2 old <reason> --dry` (what the lead's write puts in
# slots65.json; the file is the lead's, so the dry run only); (2) the all-old assembly with b2's slot (gen --slots=old --run --out=<scratch>)
# against the tracked n65-foundry.json (bytes apart from the names) and verbs37's wall pick on it: `n39views wall <that level>` rows 9 / 11
# with the picked room printed, beside n52-foundry's and the tracked n65-foundry's picks.
#   bash Research/2026-10-23-wave66/T/t2-proofs.sh <scratch dir, C:/...>
cd "$(git rev-parse --show-toplevel)" || exit 1
SC="${1:?scratch dir}"; mkdir -p "$SC"
echo "== 1. n65-slot-set.cjs"
node Tools/night/n65-slot-set.cjs --selftest 2>&1 | tail -3
node Tools/night/n65-slot-set.cjs b2 old "NIGHT66 T2: b2 THE GANTRY joins the slots as n52's room; rebuilt only on GG's own word (FORGE66 redteam.md finding 7)" --slate=Logs/n66/slate.json --dry 2>&1 | grep -E "b2|slots:|would write" | head -6
echo "== 2. the all-old assembly with the b2 slot (scratch --out)"
node Tools/night/gen-n65-foundry.cjs --slots=old --run --out="$SC/allold" 2>&1 | grep -E "slot b2|levelgen --walk|shipped after|REFUSED"
node Tools/night/t2-samebytes.cjs "$SC/allold/n65-foundry.json" Design/levels/n65-foundry.json 2>/dev/null || node Research/2026-10-23-wave66/T/t2-samebytes.cjs "$SC/allold/n65-foundry.json" Design/levels/n65-foundry.json
D=Temp/SimHarness/bin/Release/net10.0/SimHarness.dll
echo "== 3. n39views wall: the all-old assembly with the b2 slot, the tracked n65-foundry, n52-foundry"
dotnet "$D" n39views wall "$SC/allold/n65-foundry.json" n65-foundry n52-foundry 2>&1 | grep -v -E "^  note:"
