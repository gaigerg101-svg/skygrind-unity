B13: PASS

Gate B13, NIGHT60. One slim critic, read-only: 02:39-02:56, then after the usage-limit pause 04:02-04:12 (2026-10-22). The brief is
gate-b13-brief.md. The rule is NIGHT57.md section 0, clauses 1-4 (clause 5 covers only an AI critic's MAIN, so it does not apply here).
I used 4 of the 6 images, ran the brief's 2 re-runs, and added 1 control re-run. I went over the 15 min budget. Part of the reason: the
brief's levelgen command needed a scratch working directory so that it would not rewrite a tracked file (see "For the lead").

## The rule, one line per clause
- (1) MAIN: met, beyond noise. The Player's kb2c spawn frame reads "at (-2.0, -168.0) yaw -10 | alive 5 | seen 5 | nearestAlive 9.6"
  (Logs/feel/n60-b5/summary47.txt line 6, block a13). Both builds before B13 read "yaw 45 | alive 5 | seen 0 | nearestAlive 8.3"
  (Logs/n60/b1-summary47.txt line 6, and Logs/feel/n60-b4/summary47.txt line 48). So seen goes 0 -> 5 against the stated minimum of 2,
  and the two "before" builds agree, so there is no frame noise to beat. spawn31: PASS with 0 errors (Reports/LEVEL-n57-kiln-validate.md
  line 42, and my re-run). n46-flow-geo, kb2c "w1 in view": 1/4 -> 4/4 (Logs/n60/L/b13/geo-base.txt vs geo-c1.txt; my re-run matches).
  Note: the flow-geo part was not in the backlog's MAIN as written at 23:42 (5e64a858). It is a supporting number, and it also passes.
- (2) GUARDS: all hold. Before = Logs/n60/L/b14/c2-{A,B,C}.txt, after = Logs/n60/L/b13/c1-{A,B,C}.txt.
  - Put-ats: 0 / 1 / 0 -> 0 / 1 / 1 (the limit is <= 5). Both put-ats after the change are in kb3c at (-7.5, 0, -253.0), d6b's west
    jamb, which is B15's spot (c1-B seed 17, c1-C seed 27).
  - Deaths pooled: 1 -> 3 (the limit is <= 9.9). After: c1-A seed 5 kb3d, c1-A seed 9 kb3c, c1-B seed 18 kb3d. Before: c2-B seed 20
    kb3. None are in kb2c.
  - Stall: 20.9 / 21.5 / 20.4 -> 18.9 / 21.6 / 18.6 % (the limit is <= 23.65; set B rose 0.1 point).
  - Walks: GREEN 10/10 in all six files.
  - Contracts (Logs/n60/L/contract/n57-kiln-b13/table.md, kb2c-check.txt, kb3-check.txt): kb2c 14/14 PASS (dwell 0.70 -> 0.65,
    grace 0.5); kb3 14/14 PASS (dwell 1.10 -> 1.80, grace 0.8).
  - Room table: PASS, IN VIEW 5 / 5 (Logs/n60/L/b13/room-c1.txt). For kb2c: vista 5.0 of 5 from the foothold, and the wave-1 entry
    view 5.0 of 5.
- (3) Nothing lost.
  - verbs37 on n57-kiln: PASS 31/31, RED rows [] (Logs/feel/n60-b5/summary47.txt lines 54-55, block vk13, -skygrindNoRim56).
  - NRE count 0 and exceptions 0 in all 12 blocks of that summary.
  - B13 touches no code. `git show --stat 33ef339f` lists only the four n57-kiln level files, gen-n57-kiln.cjs, docs/ROOMS.md, L.md
    and logs. So golden, replays and fingerprints cannot move.
  - Not checked: the lead's "gates GREEN 02:26:49". n60-gates.sh prints to stdout only, so there is no log file to read.
  - Not checked: the --flow gate. It has not been re-run since B14 and B13 (Logs/n60/flow-off60.txt is from 01:15:39). Its n57-kiln
    line would now differ by design, because the level data changed (this is not a code row).
  - Not re-run: verbs37 on the other four levels. Their data is untouched.
- (4) The right build, a fair comparison, and the frames agree with the numbers.
  - Build: Builds/n60-b5's StreamingAssets n57-kiln.json hashes to git blob 007fa756. That is the blob at 33ef339f and at HEAD
    (d3e5d526). n60-b4's level, from before B13, is 9b2e250a (e5d0514a).
  - The a157 block ran with -skygrindSpawnShots60 -skygrindNoRim56 (the a13 header).
  - n60-b4 and n60-b5 use the same probe (B3's). n60-b1, built before B3, also reads seen 0. The level files in the tree equal HEAD.
  - Eye line: from the respawn, the fight is under the crosshair, down the middle channel.
  - Nothing is at arm's length. The nearest solid in view is THE NEAR BRIDGE's north end, 3.5 m ahead-left (its box is x -9..-2,
    z -174.5..-171.5, and the spawn is at (-2, -168)). Its corner is 10 deg left of the crosshair, and the frame shows it at x ~830 px,
    where the geometry predicts. It fills the left ~43 % of the lower half of the frame.

## Re-runs (the tree at 33ef339f; HEAD later moved to d3e5d526, which changes no level, harness or code file: `git diff 33ef339f HEAD`)
- levelgen. I ran it from a scratch working directory holding copies of PortExport/ and Design/unity-config.json, so the report landed in
  the scratchpad and not in the repo. Command: `dotnet run --project Tools/SimHarness -c Release --no-launch-profile -- levelgen
  Design/levels/n57/kiln.plan.json <scratch>/out/n57-kiln.json --walk`. Output: "plan checks: 0 problems", "level report: 0 errors",
  spawn31 "PASS | 42 encounter nodes in 5 rooms ... (nearest 9.96 m)".
  - The report equals the repo's Reports/LEVEL-n57-kiln-validate.md except line 3, which holds the output path.
  - The map equals Design/levels/n57/kiln.json, apart from the working copy's CRLF ending.
- spawn31 by hand for kb2c. The rule (LevelValidate29.Spawn31): yaw 0 faces -z and + turns west; the half cone is 58.26 deg; a node in
  the cone must be at least 12 m away. Wave 1's nodes are 12.75 / 12.79 / 13.12 / 13.34 m from (-2, -168), all within 8 deg of the
  yaw -10 facing, so they are in the cone and all >= 12 m. The old nodes were 10.79-12.37 m away.
- Control: the same levelgen on the pre-B13 plan (`git show e5d0514a:Design/levels/n57/kiln.plan.json`) gives the same report, with 0
  errors. Both plans print "bot walk: ; DID NOT reach the last exit; zone at the end kb1" (exit 1). That NIGHT24 spike walk never leaves
  kb1 on this level, before or after B13, so it is not B13's doing (n46flow's walks are 10/10).
- `node Tools/night/n46-flow-geo.cjs n57-kiln` prints "kb2c ... w1 in view 4/4". All six lines equal Logs/n60/L/b13/geo-c1.txt
  (with CR stripped).
- `git status --short` after the re-runs: no tracked file changed. The only differences from my start snapshot come from the lead's
  commit d3e5d526 (NIGHT60-LOG.md and the brief are now committed).

## Images (4 of 6)
- n60-b5 kb2c spawn (after, yaw -10): the view runs down the middle channel. Wave 1 (the burrower's red glow and the swarmers) is just
  under and right of the crosshair. The near bridge's end is on the left, the health cross is ahead-left, the cyan ride column with its
  ring is on the right, and a raised plate is on the right. The player can see where the fight is.
- n60-b1 kb2c spawn (before, yaw 45): the west wall's panels and the near bridge's dark face with its cyan top edge fill the frame. No
  enemy is visible, and the health cross is at the right edge. This is the defect as B1 described it.
- n60-b5 kb2c entry (after): the same view as before, seen 5 / 5. The enemy group sits a little further back (nearest 11.2 -> 12.7 m in
  the summaries).
- n60-b1 kb2c entry (before), compared with after: pixels that differ by 16 or more (of 255) cover 2.72 % of the frame.
  - Most of them are in the cell x 720-960, y 540-720 (55.5 % of that cell), which is the enemy group.
  - 12-13 % differ in three cells to its left (x 240-720, y 360-720): lighting on the wall and the near bridge. I did not check the
    cause. I see no geometry change.

## The caveats
- First contact with wave 1: the numbers show it comes no later.
  - Brief's numbers, matched in the kb2c rows of c2-* and c1-*: clear 42.5 / 37.5 / 39.9 -> 34.0 / 39.9 / 33.1 s; stall 22.4 / 18.5 /
    19.3 -> 17.1 / 19.4 / 17.0 %.
  - kb2c "dead" time (no enemy alive and the door shut): 3.0 -> 1.0 s on all three sets.
  - Nearest enemy: median 5.3 / 5.4 / 4.8 -> 4.9 / 4.6 / 4.9 m; p10 1.5 / 1.7 / 0.7 -> 1.2 / 1.1 / 1.0 m.
  - Kill gap median: 1.9 / 1.4 / 1.4 -> 1.3 / 1.5 / 1.6 s.
  - In the Player, the nearest enemy at frame time is 1.3-1.5 m further away: spawn 8.3 -> 9.6 m, entry 11.2 -> 12.7 m.
- Deaths 1 -> 3: inside the written limit and outside kb2c (kb3c 1, kb3d 2), in rooms B13 did not touch. The bot's later rooms change
  whenever its kb2c run changes. Against tonight's B14 level it is still 2 more deaths over 30 seeds, so watch it with B15.

## For the lead
- If you run the brief's levelgen command from the repo root, it rewrites the tracked Reports/LEVEL-n57-kiln-validate.md.
  Harness.N24.Level.cs line 42 writes the report under the parent of the working directory's PortExport/, and the report's line 3 names
  the output path, so the file changes. Run it from a scratch directory, as I did.
- PLAY note: after a death in THE CLINKER BRIDGES you respawn facing the fight down the middle channel. The near bridge's end is 3.5 m
  ahead-left. Off switch: kiln57 (off = n52-kiln); B13 alone is `git revert 33ef339f`.
