B14: PASS

Gate B14, NIGHT60. One slim critic, 2026-10-22 02:10-02:22, read-only. The brief is gate-b14-brief.md; the rule is NIGHT57.md section 0, clauses 1-5. I used 4 of the 6 images and ran 1 re-run.

## The rule, clause by clause

- (1) MAIN: met on the MAIN's direct measure. The put-at "-> 0" target was missed by one, by a cause that was already in the baseline.
  - NO PATH, seeds 11-20: 1591 -> 0. The count is the `kb3c: detour ... NO PATH` lines in Logs/n60/L/b14/base-B.txt and c2-B.txt.
  - Noise, by the rule: the baseline gap between seeds 1-10 and seeds 11-20 is |1012 - 1591| = 579. The drop of 1591 is 2.7 times that gap, so it beats the noise.
  - The other sets get better, not worse: seeds 1-10 go 1012 -> 0 and seeds 21-30 go 2171 -> 0 (base-A/C vs c2-A/C).
  - Put-ats (the SUMMARY line "put at the next room ... N" of each file): 1 / 2 / 5 -> 0 / 1 / 0.
    - On seeds 11-20 alone, 2 -> 1 is a change of 1. That equals the baseline gap (1 vs 2) and is not more than it, so the put-at half alone does not clear the noise bar on the named seeds.
    - Pooled over seeds 1-30: 8 -> 1.
  - Put-ats by cause (the trace line printed just before each PUTAT60 line):
    - The pocket, 6 of the 8 base put-ats. Five were NO PATH strandings (seeds 3, 12, 15, 23, 27). Seed 30 was stuck at (4.5, 0, -237.0), path 2/20.
    - The d6b jamb, the other 2 (seeds 21 and 22, base-C lines 827 and 915). In both, the bot was thrown to y 5.5-5.6 at (-7.5, -248) and then stood at (-7.5, 0, -253.0) until the 15 s rule.
    - After the cuts, the pocket cause is 0 on all 30 seeds (2 -> 0 on seeds 11-20). The jamb cause is 1: seed 14, thrown at (-7.1, 5.0, -248.5) and then standing at the same (-7.5, 0, -253.0) (c2-B lines 21-32).
    - So the put-at that is left is the B15 mechanism, which was already in the baseline. The cuts moved it to another seed; they did not create it (2 -> 1 over 30 seeds). One note: seed 14 itself had no put-at in base-B.
- (2) GUARDS: all hold.
  - kb3c deaths: 1 / 6 / 1 -> 0 / 0 / 0 (the kb3c room rows of base-* and c2-*).
  - n57-kiln stall: 20.3 / 22.9 / 22.7 -> 20.9 / 21.5 / 20.4 % (the SUMMARY lines). All are <= 23.65. Set A rose 0.6 points, which is 3 % relative, inside the 10 % band.
  - Walks: GREEN 10/10 on all six files.
  - kb3c contract: 14/14 PASS. Dwell 1.10 -> 0.00 (Logs/n60/L/contract/n57-kiln-kb3c-c2/table.md). Its stall A is 33.4, against 32.4 on cut 1 (contract-c1.txt). That is +1.0 point, inside the band. I did not find the pre-B14 contract stall.
  - Room table: PASS (room-c2.txt). kb3c is in view 6.0 of 6, and IN VIEW is 5 / 5 rooms.
  - levelgen: errors 0 of 34 checks (Reports/LEVEL-n57-kiln-validate.md, updated in bdb2ec0b).
  - a157 in the Player: GREEN, 5/5 rooms framed. kb3c is seen 3 / 6 at the entry and 1 / 6 at the spawn (Logs/feel/n60-b4/summary47.txt lines 51-52). That equals n60-b1 (Logs/n60/b1-summary47.txt lines 9-10: 3 / 6 and 1 / 6), so the ">= 3 / 6" guard holds.
- (3) Nothing lost.
  - verbs37 on n57-kiln: PASS 31/31, RED rows [] (summary47.txt line 40, block vk14, -skygrindNoRim56).
  - NRE count 0 and exceptions 0 on all 12 NRE lines in that summary.
  - B14's two commits touch only level data, gen-n57-kiln.cjs, docs and logs (`git show --stat 612530f0 bdb2ec0b`). There is no Core or sim code in them, so golden, replays and fingerprints cannot move. I did not re-run the gates or the other levels' verbs37.
- (4) Measured at HEAD on tonight's build, and the comparison is fair.
  - My re-run, at e65d4275 with the level file clean, matches c2-B line for line (89 lines). The one difference is c2-B's "PUTAT60 ... seed 14" line, which the main tree's harness does not print: it is builder L's worktree-only print, which B6 allows. The summary's put-at count of 1 matches.
  - The rows before kb3c are equal in base-B, c1-B and c2-B (seed 14's kb2, kb2c and kb3 rows), so the cut changes only kb3c and what comes after it.
  - The Player is the right build with the row in it. Builds/n60-b4's StreamingAssets n57-kiln.json gives git blob 9b2e250a1a5b, which is bdb2ec0b's blob. The pre-B14 blob at 4fdcf8e6 is 9db9b6d8a339.

## My re-run (seeds 11-20, 02:11:43-02:13:44, tree at e65d4275)
`dotnet run --project Tools/SimHarness -c Release -- n46flow --level n57-kiln --seeds 10 --first 11 --per-seed --trace --no-geo`
- `10/10 walks through kb2..kb3d; level time median 193.9 s; ... out of lava 322 steps, held at an edge 538 steps, chases round it 0, detours 1; put at the next room (...) 1; stuck46 burrowers ended 24`
- `kb3c       room |   24.1      1.3  59.1 |  0.9 / 9.4   |  6.2 /  1.7 / 11.9  |   1.5   1.0 |      0     0 |   20.5 (10/10) | -`
- `n46flow n57-kiln SUMMARY: GREEN 10/10; stall 21.5 %, longest stall 3.9 s, air 53.3 %, ... deaths 1, falls 0, clear 171.3 s`
- NO PATH lines: 0. The seed-14 trace is equal to c2-B's: a 24-cell detour, thrown at (-7.1, 5.0, -248.5), then standing at (-7.5, 0, -253.0). Wall time 102 s.
- Every line equals Logs/n60/L/b14/c2-B.txt except its worktree-only PUTAT60 print.

## The missed "-> 0" put-at target: my view
It should not block.
- Clause 4 says: "A near miss on a target that is not a guard is noted, not blocking." The put-at target is a missed target that is not a guard: it missed by one.
- The MAIN's direct measure of the defect B6 found (NO PATH) clears the noise 2.7 times on seeds 11-20 and is 0 on all 30 seeds.
- The put-ats the defect caused go 6 -> 0. The one left is the d6b jamb launch, which was already in the baseline on seeds 21 and 22. That is now B15.
- The NIGHT59 L0 precedent (a "<= 5 per set" guard met at exactly 5) is a guard met at its limit, not a miss, so it does not decide this case. Clause 4 does.
- Caveat for the lead: if a two-number MAIN must have both halves beat the noise, then the put-at half (2 -> 1 against a gap of 1) does not, and the verdict would be UNSURE, not FAIL.
- Keep B15 (MAIN: kb3c put-ats 0 on seeds 1-30). The PLAY note should say that THE BLOW PIPE pad 2.5 m in front of d6b can still throw a player walking out.

## Images (4 of 6)
- n60-b1 kb3c entry (before): HUD 12 LEFT / THE TAP RUN / 500. The near dark block has a cyan top edge. Five enemies are in view (a drone, a parry-ringed husk, a glowing red one at the block's end, one on the far ledge). There is a violet-white vertical light in the back.
- n60-b4 kb3c entry (after): the same view, enemies and geometry. Pixel differences of 16 or more cover 0.25 % of the frame, mostly one cell (x 1200-1440, y 540-720, 6.1 %) around the small cyan mark and the enemy glow at the right ledge. That looks like animation or particles, not geometry. Nothing I can see changed.
- n60-b1 kb3c spawn (before), at (-6.5, -234) yaw 0: a corridor toward the cyan exit door, with four red x-rayed enemies through the right-hand block and a drone on its top.
- n60-b4 kb3c spawn (after): the same. Pixel differences of 16 or more cover 0.19 % of the frame, in the door's animated glow cells (3.3-3.8 %). Nothing I can see changed. Neither pose shows the opened pocket or the shorter lava end, as the brief says.
