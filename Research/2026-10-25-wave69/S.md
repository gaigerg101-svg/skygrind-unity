# NIGHT69 builder S: v2 and g8 (notes; t0 d9e22d48, branch n69-S)

Every number below is measured ALONE in S's tree (only the room latest in its level; seeds 1-20; bars from Logs/n69/slate.json) and saved
under Research/2026-10-25-wave69/<id>/. The position traces come from a LOCAL, never committed print in Tools/SimHarness/Harness.N65.Verb.cs
(env S69TRACE=1: every 2 s of fight the pilot and the room's live bodies, plus each launch and burst; it changes no sim step: s1b read 0.99
with and without it). The trace files and the small readers (v2/diag/*.cjs, g8/diag/*.cjs) are committed; the print is not.

## a1 files (picked by the lead at 10:38: main deda5825)
- v2 = s1b (89b162ed): pooled 0.99 (1.04 / 0.95), 20/20. g8 = NIGHT68 r1f (26a99ff3): 0.96 (0.84 / 1.00), 17/20 (my pocket s1 read 0.80).

## v2: why the verb-aware run had a long tail (s1 / s1b: seeds at 49-80 s, 400-500 m walked)
1. **The TANK set the clear time, not the verb.** W3's heavy is a husk with 600 hp that launch36 never targets (tank29). Per seed and run
   (v2/diag/tank-table-s1b.txt): when the TANK followed the pilot into THE CRYPT it was ended there at hp 400-600 and the run cleared in
   26-37 s; when it was shot down on the floor the run took 47-76 s (seed 3 verb-aware: the TANK alone from 42 s to 66 s, hp 591 -> 1).
   The TANK was not in the crypt when last seen in 7 / 20 verb-aware runs, 2 / 20 refused-in runs and 6 / 20 walker runs. Nothing in the room
   decides it: the pilot (any run) drops into the 2 m crypt and the bodies path after it (the nav graph drops 5 m and jumps 2.2 m up).
2. **skip v2 takes the faster of two refusals** (refused-in and walker), so a coin-flip like (1) reads below 1 even when the verb helps.
   v2/diag/refusal-min.txt: s2c's median ratio is 1.20 against the refused-in run alone, 1.20 against the walker alone, 1.08 against the faster
   of the two; s1 1.15 / 1.28 / 1.03. In a noisy launch room the 1.25 bar asks about 1.37-1.40 against a single refusal. (Not a ride room's
   problem: in g8 the two refusals are the same run.)
3. **The rings over the screen are what keeps the fight short.** Both runs grapple THE DOOR RING over the screen at about 6 s. s2a (the
   three rings moved into the near half, to keep the pilot at the door) read **0.86**: launch36 29 (s1b 45), every run slower (46-49 s).
- **s2c = s1b + W3's TANK -> a husk: pooled 1.07 (1.21 / 1.04), 20/20, launch36 52; median clears verb-aware 28.7, refused-in 33.2,
  walker 35.6 s** (the verb is the faster state by 14 % on the medians). Committed as the v2 CANDIDATE (4e6c92f7): it changes the shipped
  kinds (the TANK is level 5's), so it is a design question for the lead / GG, not a free fix. s2d (s2c + both swarmers -> husks) 1.00.
- After s2c the last minutes are mop-ups (v2/diag/last-body-s2c.txt): one body left 2-16 s in every run, often in the crypt's 2 s hold.
- What I think moves it next (not built): make the launch the only fast answer to the last bodies, e.g. the far half's floor lower than the
  door (bodies come UP to the door one at a time = a queue the punch meets un-shot), or the TANK kept but on a route that crosses the crypt
  in every run (its spawn beyond the crypt from the door); and the min-of-two noise is the instrument's (T / the lead).

## g8: why the pilots seldom climb into THE POCKET (s1: 0.80; s1a 0.88)
1. **The pocket does not hold its spitters.** Spitter sightings over all seeds (g8/diag/pocket-count-s1.txt): in THE POCKET 65 (verb-aware)
   / 82 (refused), on the stage outside it 60 / 60, on the coal stair and the far bank's floor 352 / 263, in the pit 25 / 51. The spitters
   walk down the stair to find a sight line, so the kill comes to the pilot and the climb is never needed.
2. **The 2 m pit traps the pilot.** The pilot is in the pit (y < -1) in 51 % (verb-aware) and 57 % (refused) of fight snapshots; the last
   bodies and the pilot are often both in it (g8/diag/last-body-s1.txt). r1f's 1.5 m pit: 16 % / 17 % (g8/diag/pocket-count-r1f.txt).
   **s1c** (s1 with the pit back at 1.5 m, derive-g8-s1c.cjs): pooled **0.94** (0.94 / 0.94), 16/20, deaths 6 / 7 / 7, the pilot in the pit
   22 % / 30 %, spitters still mostly on the floor / stair (393 / 286 sightings against 67 / 80 in the pocket): no better than r1f (0.96).
   g8 keeps r1f (the a1 file); none of s1a 0.88, s1 0.80, s1c 0.94 is a candidate.
3. **The ride is a detour in a kill-bound room** (NIGHT68 R finding 6, confirmed): the refused run and the walker are the same run in a ride
   room, and its median clear is 7 s faster than the verb-aware run on s1 (32.1 vs 39.5 s) and 3 s on r1f (41.1 vs 44.2 s).
- What would make the climb the short way: a target that cannot leave its perch (no walkers' route down: but levelgen's 'enemies reach
  every tier' refuses a perch walkers cannot reach, and a spitter is a walker); or a room whose clear is not kill-bound (a switch, an exit
  only the ride reaches: a rule change, GG's); or ranged bodies that keep their stand (an enemy rule, GG's). With the rules as they are, I
  found no piece that keeps a body where only the ride reaches it.

## Fix round on a1's level review (Logs/n69/r1/review-level.md; main's Y1 / Y2 / Y3 tools synced, c2d7b56e)
- The lead: s2c is NOT accepted (the TANK is level 5's; GG owns enemy design): v2 keeps its wave kinds.
- **v2 s3** (derive-v2-s3.cjs: the review's BLOCKING 1 smallest change: THE CRYPT x -15.5..-9.5, z 58..64.5 behind the door across the
  throw, 1 m off the screen; the W1 / W2 melee nodes at x -8.5..-3, z 60.5..63; the floor route east of the pit; the crypt ring over it):
  pooled **0.88** (0.79 / 1.04), 18 / 20, STYLE 75.2 vs 67.2 (PASS); medians verb-aware 50.8, refused-in 41.5, walker 54.3 s; **launch36 28**
  (s1b 45); 1.02 against the refused-in run alone. Worse than s1b on every count: the queue no longer meets the pilot in the door (the
  bodies file along a 1 m strip and the pilot walks after them: walked 381 m median against 300). air65 3 of 3, patch 0 % (v2/s3/).
  BLOCKING 2 (the pads' touches) not built: one candidate fitted the time.
  s3 fresh 21-30: median 0.82, 10 / 10 (v2/s3/fresh.txt).
- **v2 s1d** (derive-v2-s1d.cjs: BLOCKING 2 only, on s1b: THE FONT at (-11, 72.5) on the spawn-to-door line, the pulpit pad's landing on
  the loft's north edge): THE FONT touched 20 / 20 seeds (deco PASS for it), the pulpit pad 4 / 20 (still FAIL: deco 12 pass, 1 FAIL);
  air65 3 of 3; but pooled **0.94** (0.99 / 0.91), 14 / 20, launch36 21, every run slower (the FONT throws every pilot back to the tribune
  on its way to the door: verb-aware 51.9, refused-in 44.9 s). Not a candidate (v2/s1d/).
- **Result: no v2 fix candidate beats the a1 file s1b (0.99) with the shipped kinds.** v2.json on n69-S is s1b again.
- **g8 keeps r1f** (the a1 file, 0.96): the review's three changes (THE TURNTABLE WALL along the near rim over the dazed chargers, the
  east crossing narrowed, a pad off the stage) were not built: no time to measure them before 11:50, and my three pocket variants (0.80-0.94)
  never beat r1f. The diag above (the pocket leaks, the ride a detour in a kill-bound clear) is the reason I expect no piece to reach 1.35.

## Open / not checked
- The guards (walk bot deaths A / B, stall A) on s2c and on g8 s1; M3 on s2c; fresh seeds 21-30; deco and lint on s2c (the pieces are
  s1b's, the waves changed).
