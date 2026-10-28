# NIGHT46 F1: the flow table on the candidate game (port/stage3 59a44d7c, 2026-10-18 00:17-00:26)

The meter: `n46flow` (Research/2026-10-17-wave46/F1.md: one pilot for every room, caps left out of a room's numbers, bosses not measured).
Re-run on any checkout, one command (about 4 min, nothing heavy beside it):
`bash Tools/night/n46-flow-table.sh <outDir> [rows]` (rows default `retry46,overlap46,teach46,stuck46,walkin46,wyrm46`). It builds that
checkout's harness, runs 20 meters in parallel (baseline = n45-foundry / n45-iceline / n45-kiln / n40-span / n41-spire with `--off46`;
candidate = n46-foundry ... n46-spire with the rows forced on; seeds 1-10 and 11-20 each) and prints the table below (`<outDir>/table.md`).
The band is that run's own baseline seeds 1-10 against 11-20 (it reproduces F1.md's band exactly: the sim is deterministic).

Where it ran: a detached scratch worktree of port/stage3 at 59a44d7c (read-only; removed after). In it: A2's n46-spire, A3's n46-foundry /
n46-iceline, B1's n46-kiln + wyrm46 / walkin46, F1's rows up to reveal46. **Not in it:** A1's n46-span (the placeholder: a copy of n40-span)
and F1's g7 fix (b738899a: overlap46's entryM / growthMax, not yet picked). The level files load as themselves; no sim code reads a level
row's `enabled` (checked by grep), so "the level rows on" is the n46 files loaded. Bosses (k5 included, where wyrm46 / walkin46 act) are not
measured by this pilot: their numbers are their builders'.

## 1. The candidate as port/stage3 has it (59a44d7c)
Each cell: baseline -> candidate, the mean of seeds 1-10 and 11-20 [the candidate's 1-10 / 11-20]; walks = both sets (of 20). ✓ = better
past the band, ✗ = worse past it.

| level | walks | stall % | longest stall s | dwell s | dead s | deaths | clear s |
|---|---|---|---|---|---|---|---|
| L1 n46-foundry | 16 -> 20/20 ✓ | 31.1 -> 29.9 [30 / 29.7] | 5.3 -> 3.1 [3 / 3.1] | 15.2 -> 12.3 [11.6 / 12.9] | 53.3 -> 30.2 ✓ [31.6 / 28.8] | 26.5 -> 25.5 [22 / 29] | 210.6 -> 138.1 ✓ [130.7 / 145.5] |
| L2 n46-iceline | 20 -> 20/20 | 35.8 -> 38.8 [38.7 / 38.9] | 7.1 -> 7.5 [7.8 / 7.1] | 14.8 -> 10.5 ✓ [9.9 / 11] | 36.1 -> 30.3 ✓ [29.3 / 31.2] | 2 -> 4.5 [4 / 5] | 196.5 -> 181.6 [179 / 184.2] |
| L3 n46-kiln | 7 -> 19/20 ✓ | 35.4 -> 31.7 [32.4 / 30.9] | 8.8 -> 4.7 [4.7 / 4.7] | 14.2 -> 6.2 ✓ [7.2 / 5.1] | 26.1 -> 12.1 ✓ [12.6 / 11.6] | 6 -> 6 [8 / 4] | 218.5 -> 123 ✓ [121.9 / 124.1] |
| L4 n46-span (placeholder) | 20 -> 20/20 | 18.7 -> 19.6 [21.3 / 17.9] | 3.7 -> 3.7 [3.7 / 3.7] | 7.9 -> 6.5 [5.9 / 7] | 15.6 -> 10.6 ✓ [10.6 / 10.6] | 2 -> 1.5 [2 / 1] | 127 -> 112 [110.6 / 113.3] |
| L5 n46-spire | 20 -> 20/20 | 12.9 -> 14.1 [13.7 / 14.5] | 4.3 -> 4.3 [4.3 / 4.3] | 8.5 -> 7.6 [8.6 / 6.5] | 11.4 -> 10.4 [10.4 / 10.4] | 0.5 -> 0 [0 / 0] | 191 -> 159.5 ✓ [155.2 / 163.7] |
| band | - | 6.8 | 4.8 | 4.1 | 3.3 | 4 | 20.2 |

No ✗. Iceline's deaths 2 -> 4.5 are inside the band, all in g7 (4 / 5): overlap46 without the fix (F1.md round 2).

## 2. The same candidate + F1's g7 fix (b738899a applied in a second scratch worktree, not committed there)
| level | walks | stall % | longest stall s | dwell s | dead s | deaths | clear s |
|---|---|---|---|---|---|---|---|
| L1 n46-foundry | 16 -> 20/20 ✓ | 31.1 -> 29.9 | 5.3 -> 3.1 | 15.2 -> 12.3 | 53.3 -> 30.2 ✓ | 26.5 -> 25.5 | 210.6 -> 138.1 ✓ |
| L2 n46-iceline | 20 -> 20/20 | 35.8 -> 37.3 [37.5 / 37.1] | 7.1 -> 7.4 [7 / 7.8] | 14.8 -> 14.9 [15.8 / 13.9] | 36.1 -> 32.4 ✓ [33.1 / 31.6] | 2 -> 1.5 [1 / 2] | 196.5 -> 183.5 [181.3 / 185.6] |
| L3 n46-kiln | 7 -> 19/20 ✓ | 35.4 -> 31.7 | 8.8 -> 4.7 | 14.2 -> 6.2 ✓ | 26.1 -> 12.1 ✓ | 6 -> 6 | 218.5 -> 123 ✓ |
| L4 n46-span (placeholder) | 20 -> 20/20 | 18.7 -> 19.6 | 3.7 -> 3.7 | 7.9 -> 6.5 | 15.6 -> 10.6 ✓ | 2 -> 1.5 | 127 -> 112 |
| L5 n46-spire | 20 -> 20/20 | 12.9 -> 14.1 | 4.3 -> 4.3 | 8.5 -> 7.6 | 11.4 -> 10.4 | 0.5 -> 0 | 191 -> 159.5 ✓ |
Only iceline changes (the fix touches overlap46's holds; levels 1, 3, 4, 5 byte-equal): g7's deaths 4 / 5 -> **1 / 2**, g7 clear 52.6 /
44.9 -> 51.5 / 52.9 s; the price is iceline's dwell (10.5 -> 14.9: g7's wave 2 now waits for the clear).

## Per room, what moved (candidate with the fix, seeds 1-10; baseline in F1.md)
- L1: a8 dead 9.8 -> 0.7 s and clear 13.8 -> 5.5 s (A3's perch on the first ride wall + teach46); b1 clear 104.7 (cap 2) -> 48.4 s, dead 13.2
  -> 4.2 s (retry46 + overlap46); a2 dead 11.3 -> 8.2 s (teach46); a5 deaths 7 -> 6 (seeds 11-20: 12).
- L3: walks 7 -> 19 / 20: B1's kb3 (no burrower across the pour) + stuck46; the one stop left is kb2 seed 19, a drone at y 7.7 above
  (-4.4, -109.5) for 240 s (2 such in the baseline).
- L2: g3 THE TIPPLE (A3's rebuild): loop y (A3's ring), clear 19.9 -> 18.5-19.5 s; g7 as above.
- L5: v2 / v6 (A2's rebuild) + overlap46: clear 191 -> 159.5 s, dead 11.4 -> 10.4 s.

## Not checked
- A1's n46-span (not in port/stage3): L4's row is the placeholder (n40-span) + the rows. Re-run the script once A1 is picked.
- The bosses (k5's wyrm46 / walkin46: B1's numbers). Any frame or Player.
- Raw output: Research/2026-10-17-wave46/F1-candidate-runs.txt (the 20 candidate runs of both tables); the baseline runs reproduce
  F1-baseline.txt's numbers.
