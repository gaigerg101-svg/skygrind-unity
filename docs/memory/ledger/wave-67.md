# Wave 67 (NIGHT67, THE AREAS; 2026-10-24 10:36:42 to ~18:20, 8 hours: BUILD 0:00-7:15, FREEZE 7:15, CAP 8:00; tag u3-n67)

**Ask (GG 2026-10-24, verbatim, NIGHT67.md line 3):** "need to build a another detailed 8 hour build prompt following same methods, area
deisgn need t be better in alot of levels thats the mian focus". The forge FORGE67b (Research/2026-10-24-forge67b/: four scouts,
D-1..D-9, the red team's R-1..R-14) chose six rooms on three levels (level 2 g7 g8, level 3 kb3 kb3c, level 5 v2 v6), judged by a new
static area lint and skip v2, in two work blocks around the usage limit. GG set the /goal in a fresh session.

**Prompt shape:** NIGHT67.md (187 lines, untracked) + GOAL.txt on one end-state script (n67-close-check.cjs, 28 self-test cases); slate 11
(T1a, T1b, T1c, six rooms, LV, PC); builders R (g7 g8 v2), S (kb3 kb3c v6), T (tools) in NIGHT66's three reused worktrees + one review
slot; cards + a card critic, r1 chained to compute through the stop, a GATE per held room (lint, skip v2 on two seed sets, M3, deco,
guards, a level review, a spotter, a critic).

**Branch C** (C: 11.7 GB at t0): sim only. **One usage stop:** 2:29-5:04 (2:35). The disk fell to 4.8 GB at 2:19 (the pagefile, D-1);
the builders stopped at the 5 GB line (D-2), the space came back during the stop (9.4 GB).

**What shipped (game):** nothing: the three rows design.n67.iceline67 / kiln67 / spire67 OFF; every slots67.json all old.

**Tools:** T1a PASS (gen-slots.cjs for any level; the shared commit 16116dea), T1b PASS (the round tools on L67, the area lint, the parry
refusal fix, d25ea7d8 PASS), T1c FAIL, FAIL, PASS (verdict-T1c.md, -r2.md, -r3.md: the GATE pack's room-file and level-review checks).
Harness fixes after the reds: the ride pilot's gun (f3d4ad6f), **the put-in loadout** (a527ac13: earlier rooms' awards; the reds re-run,
g7's bar 1.25 -> 1.39), the verb pilots' put-in (5adbca3b), the launch pilot's hand (c305a2ab). The flow base re-made: EQUAL.

**Numbers (skip v2 M1 vs mainBar; r3 = the builders' final files on main, Logs/n67/r3-<L>/):**
- g7 parry: red 1.19, bar 1.39; r1 1.43 (deaths A 3 > 1), r2 1.56 but the GATE pack's seeds 11-20 1.24 (Logs/n67/gate/g7/), r3 1.32;
  lint, deco, parry M3, guards all PASS. R: cover cannot separate the spit, the gun and the returned glob (HuskBrain.cs:816-822).
- g8 ride 0.77 vs 1.25 (the pilot rides every face: 92 vs 55); v2 launch 1.01 vs 1.25 (stall 24.7 vs 19.0: lava in plan);
  kb3 launch 1.02 vs 1.35 (every guard held; M3 RED); kb3c ride 1.06 vs 1.25 (ride --shoot GREEN; deaths A 1 vs 0); v6 launch 0.96 vs
  1.33 (~21 launches per 10 seeds).
- The area lint: FAIL on all six shipped rooms -> PASS on five of six at r3 (v6 FAIL P9).
- Lava beside the fight (S's first cut): walk-bot deaths 19 / 13 (kb3), 15 / 12 (v6); dry pits fixed deaths, raised stall.

**SHORT (8):** the six rooms, LV (branch C, no PASS), PC (no PASS: no play build). PASS: T1a, T1b, T1c.

**Reviews:** the card critic (0 GO, 6 CHANGE), the T1a + T1b critic, three T1c critics, gg-proxy at the close (Logs/n67/gg-proxy-close.md).

**Lessons:** L-150 re-confirmed (w67: check the instrument's put-in loadout before any red); L-128 extended (commit pressure grows the
pagefile and eats the disk); L-003 re-confirmed (one stop, 2:35).

**Estimate to actual:** T1a 0:50 planned -> 0:20; T1b 1:45 -> 1:07; T1c ~5:15 -> 1:37 (then two fix rounds to 5:21); r1 chained 2:05
planned -> started 1:49 after the re-reds; main-tree rounds 233-485 s per level. Idle: the stop (2:35).

**Reaction:** none yet (GG last played 2026-10-20).
