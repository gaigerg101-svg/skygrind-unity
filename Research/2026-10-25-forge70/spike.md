# FORGE70 spike (S5): how much of a launch room's clear is the launch? (2026-10-25 ~21:58; read-only node over saved skip files)

Script: the forge's scratch `launch-share.cjs` (node, read-only; copied below as its rule). Per verb-aware seed of each skip file: `launch36 N` from the seed line, and the
`launch36 kills:` entries of the seed's touches line (each `kills K` with or without `wall`).

| file (20 seeds each) | launch36 per run (median) | launch kills per run (median) | wall-burst share of kill events | kill events / launches |
|---|---|---|---|---|
| Logs/n69/red-kiln/kb3/skip.txt (the shipped kb3) | 4 | 5 | 35 % | 46 / 80 |
| Logs/n69/r1-kiln/verbs-skip.txt (NIGHT69 kb3 r1d) | 2.5 | 2 | 46 % | 26 / 57 |
| Logs/n69/red-spire/v2/skip.txt (the shipped v2) | 2 | 2.5 | 56 % | 34 / 43 |
| Logs/n69/r1-spire/verbs-skip.txt (NIGHT69 v2 s1b) | 2 | 4 | 43 % | 23 / 45 |
| Logs/n69/red-kiln/kb3d/skip.txt (the shipped kb3d) | 3 | 3 | 13 % | 16 / 63 |

**Reading.** kb3 holds about 23 bodies (R.md on n69-R: "the verb-aware run launches 1-6 times against 23 bodies"); a median clear is about 40 s (Logs/n69/r1-kiln/table.md:
clear A 50.7 s, the skip's verb-aware medians 38.9-41.5 s, review-level.md line 64). So the launch kills about 2-5 of 23 bodies (10-20 %). For the verb-aware clear to be
1.33x faster (40 s vs 53 s), each of 2-4 launches must save 3-6 s of the gun's work: only a burst of 3+ bodies or an ended heavy (a 150 hp charger, a 600 hp tank) can.
**NIGHT69's rebuild made it worse:** launches per run 4 -> 2.5 and kills 5 -> 2 (kb3), because the bodies met the pilot on the far floor (review-level.md: 31 of 45 launch
kills there). **So the room's job is to bring bodies to the punch spot bunched and early, and to put the heavy where a throw ends it**; a pit nobody is thrown into is decor.

## Spike 2 (22:12-22:16): the launch probe on 20 seeds, not 1 (`bash Tools/night/n70-throws.sh <level> <room> <out> 20`, the forge's new tool; files in spike/)
| room (level file) | GREEN | thrown median (min-max) | flight ends | in a pit | bursts killing >= 2 | M3 candidates (D-3: pit / lava end or a >= 2-kill burst) |
|---|---|---|---|---|---|---|
| kb3 shipped (Design/levels/n57-kiln.json) | 18 / 20 | 1.2 m (0-12.8) | wall 8, body 6, timeout 4, none 2 | 0 | 7 | 7 / 20 |
| kb3 NIGHT69 r1d (Logs/n69/r1-kiln/n67-kiln.json) | 19 / 20 | 1.0 m (0-12.7) | body 12, wall 4, timeout 3, none 1 | 2 | 13 | 14 / 20 |
| v2 shipped (Design/levels/n52-spire.json) | 20 / 20 | 2.3 m (0.1-4.5) | wall 10, body 10 | 0 | 14 | 14 / 20 |
| v2 NIGHT69 s1b (Logs/n69/r1-spire/n67-spire.json) | 20 / 20 | **12.8 m** (0.3-12.8) | timeout 15, body 5 | **15** | 5 | **20 / 20** |
**Reading.** NIGHT69's one-seed M3 lied both ways: v2 s1b's crypt catches the probe's throw on 15 of 20 seeds (a1's seed 1 was the blocked one), and kb3 r1d's throws
burst on bodies (12 of 20) more than on the shipped room's walls. **Yet v2 s1b's M2 is 0.99: the pit works per throw, but the verb happens about twice a run** (the table
above). So the throw geometry is not the night's blocker; the number of fresh bodies that reach punch range is (the code scout: the pilot launches only an unshot husk-shaped
body above 30 hp within 5.5 m and 50 degrees: Harness.N65.VerbPilot.cs:340-347). The rooms must make bodies arrive close and fresh (short sight lines, corners, doors:
design-research.md R7) far more often, with the pit / wall already on the line.

**What this decides (DECISIONS D-3 / D-4):** a room card states its target launches per run and kills per launch from this table's red row, and the builder measures them
(the THROW CHART, slate Z1) before the ratio; a launch room whose verb-aware run does not launch more often than its red cannot pass and is not sent to a main round.
