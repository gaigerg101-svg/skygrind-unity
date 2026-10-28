# NIGHT53 REPORT (for GG; THE SECOND PASS, 2026-10-20 07:39 - 1x:xx MST)

You asked (07:05) for another 3.5 h of polish and building, iterated, on proven data, as a loop under a goal. The night went to the rooms
NIGHT52 rebuilt: their numbers passed, but the last reviews said they read as templates. Tonight added computed checks for that and
re-cut the rooms that failed them. A polish lane shipped three looks (the kill pop GREEN on both maps tested; the scorch core GREEN on iceline only;
iceline's grade applied, its 4/4 look meter not run); two items stay at NIGHT52's look.

**Play build: `Builds/n53-freeze/SKYGRIND.exe`** (commit d2ec09ad; later commits are logs and docs). The rooms are edited in place in the
n52 levels, so the comparison side is `Builds/n52-freeze` (tag u3-n52). `-skygrindOff53` turns off tonight's polish (the kill pop, the
scorch core, iceline's grade); it does not undo the room edits.

## What changed for you in play
| room | what you will notice |
|---|---|
| kiln kb3 CASTING FLOOR | the west bank is cut by a 4 m lava inlet, so the west wall ride is that side's way north; the floor-to-roof posts are gone; the ladle stands free on the east bank with a lane behind it, its south 4 m cut to a landing |
| span p1 HOOK YARD | the terrace's south ramp and the mid-lane hook pad are cut: in the grapple's teach room you now need the hook to get up; a jib ring over the well panel; you start 5 m into the yard |
| foundry a3 RIDE | the far side is a 2.5 m pouring platform: the runner wall over the pit is now the flat way across (walking round means the shelf ramp or the crane stair) |
| iceline g4 THE REACH | no longer a mirror: one 24 m lock wall rides from the door over the canal up to the gallery; the west wall is gone (cross the west canal on a stepping stone); the first drone comes from the west, off the door's line; the canal floor is lit from below |
| iceline g3 THE SWARM | the hopper is smaller (4 x 6 m); its chute opens west into the lane and the swarm spawns there, so it packs where the blast lesson wants it |
| iceline g7 THE THAW LOCK | the return hoist pad is moved off the walk pilot's line (it sat 1.62 m from it; I4 asks 2 m) |
| HUD (polish53) | a kill punches the kill marker (x1.32) with a burst of four bone shards (LitMotion + UIEffect, no text) |
| scorch / grade (polish53) | a kill mark's core glows hot and cools; iceline gets the dusk grade with its walls lifted x1.14 |

## The identity checks (new tonight; Tools/night/n53-identity.py; each FAILs its planted case first)
I1 no mirror across the door's axis (< 60 % overlap); I2 a ride is a shortcut (walk >= 1.3 x the ride, on the bot's nav grid, never on
lava); I3 a teach room's verb is required (grapple: every ring's tier walk >= 1.5 x the pull); I4 pads 2 m clear of walk lines and spawn
lines; I5 at most two ring-round-a-mass rooms a level; I7 no two consecutive rooms open the same way.

The 18 NIGHT52 rooms, u3-n52 -> tonight (Logs/n53/identity-u3n52.md, Logs/n53/identity-freeze.md):
| room | u3-n52 | tonight |
|---|---|---|
| kiln kb3 | I1 73 % FAIL | all computed items PASS (I1 25 %, I2 1.59, I4) |
| span p1 | I3 1.22 FAIL, I4 FAIL | all computed items PASS (I2 1.91, I3 3.10, I4) |
| foundry a3 | I1 69 %, I4 FAIL | I1 69 % FAIL (I2 1.64, I4 PASS) |
| iceline g4 | I1 77 %, I2 1.18 FAIL | I1 59 % PASS; I2 1.18 FAIL |
| iceline g3 | I2 1.28 FAIL | I2 1.28 FAIL (I1 21 %) |
| iceline g7 | I1, I2, I4 FAIL | I1 99 %, I2 1.26 FAIL; I4 PASS |
| the other 12 | unchanged | unchanged (I1 FAIL: g2 100 %, b1 99 %, v2 98 %, p3 95 %, g8 90 %, a5 87 %, g5 76 %, kb2c 72 %) |
| per level | I5 FAIL kiln (3 rings), iceline (3); I7 FAIL foundry | I5 PASS on all five; I7 FAIL foundry (a5 and b1 both open on ground the enemies hold) |
Room FAIL items 33 -> 27. Every changed room passes THE ROOM CONTRACT (14/14; g4 13/14 with its exempt teach-room item, as at u3-n52).

## Measured this session
- verbs37 on n53-freeze: foundry 27/27, iceline 21/21, kiln 31/31 (NIGHT52 30), span 31/31, spire 31/31; RED none, NRE 0: nothing lost.
- a452 (entry / mid / high frames): GREEN on kiln and iceline (freeze build), foundry and span (n53-r2, rooms unchanged since).
- a153 (polish53) on the freeze build: the kill pop GREEN on / RED off on foundry and iceline; the scorch core GREEN on iceline (+0.086 over
  the floor), still 0.013 under the floor on foundry; iceline's grade applied (its own frame's meter 2/4; the 4/4 look meter is a349's, not run).
- The flow table (the walk pilot through whole levels; Logs/n53/flow-freeze/table.md): iceline's clear time 246 -> 221 s (NIGHT52's cross
  gone), foundry's door dwell 16.8 -> 13.9 s (still a cross against NIGHT51's 12.1), kiln better on stall, dwell, dead time and clear time (deaths 8.5 -> 9.5), span's stall 15.6 -> 14.9 %
  (still a cross); new: iceline's stall 34.2 -> 36.0 % against NIGHT51 (+1.8 points, band 0.7; NIGHT52 read 34.0, inside the band). Spire not re-run (unchanged).
- Golden 22/22 exact and fingerprints plain / --off52 / --off53 EQUAL at each build's gates (07:44, 08:32, 09:10, 09:29).
- The full batch suite at 77c4d995: 2241 / 2244. Two V50Tests failed because tonight's iceline grade (ON at the freeze) reads iceline as
  graded: pinned test-only (8cc9542e, Editor 6/6). AutopilotTests.PerStep_AllocatesNothing found 1 allocation: the flake seen since NIGHT14,
  and it failed again in an Editor re-run: not fixed tonight. The smoke on n53-freeze: DONE, all steps passed (230.5 s, 0 NRE).
- T-126 (a test leak NIGHT52 pinned) fixed where it starts (Lesson25Tests restores every flag it sets), proven RED then GREEN in the Editor.

## Not done or not passing, said plainly
- The two reviewers (an ULTRAKILL P-rank runner and "you" from OWNER.md) still say none of g2 / g3 / g4 / kb3 would satisfy you (09:00,
  before kb3's last two cuts). Their shared points: the door half of g4 and g2 is empty floor ("smaller and more action packed"), and
  g2 is a mirror in every big piece (I1 100 %). Not taken tonight (each a re-cut with its doors): DECIDE53 D-13 / D-14.
- kb3's last two cuts, p1, a3 and g7's pad were not reviewed by the personas after they changed. p1's new start: builder K counted 1 of 3
  first-wave enemies in the first look; the a452 entry frame (n53-r2) counted 2 alive, 2 in view (a different moment of the wave: not resolved).
- I2 still fails in 8 rooms: most ride walls run beside a walkable lane of the same length. I4 still fails in 9 (spire's pads sit on 2-3 m
  walkways; g8's only clear spot broke its pad's throw).
- The first-shot hitch (T-122): the KESTREL's first shot is smooth with a boot prewarm, the THUNDERCLAP's is not (15-25 ms): shipped OFF.
  The kill wash (T-111): lower but still over target: shipped OFF. Both at NIGHT52's look.
- Nothing here was played by a human. Every number is the walk pilot's, the hold probe's or a reel's.
- The building share (minutes logged as building): 53 % (target 75 %, NIGHT52 68 %); 4 loop cycles kept (target 4).

## Next (my recommendation)
1. Play `Builds/n53-freeze`: kiln (CASTING FLOOR), span (HOOK YARD), foundry (RIDE), iceline (THE REACH, THE SWARM). Tell me if the ride
   walls now feel like the way, and whether the kill pop reads as a kill.
2. NIGHT54: the mirrors left (g2, b1, g7, v2, p3, g8, a5, g5, kb2c, a3: I1 69-100 %), shorter door halves for g2 / g4, I2's rides beside lanes, the
   THUNDERCLAP's first shot.

---

# EXT53: the extension you asked for at 10:44 (10:47 - 12:52 MST)

You asked to keep going 2-3 more hours with subagents, new things beyond the prompt, bug fixes and creativity. Three builders started at
10:47 (M rooms, N RIDE KILL, B bugs), a fourth (K2 rooms) at 11:26 and a fifth (A5, foundry a5) after the freeze at 12:26; the lead
integrated and ran every proof. The last pre-freeze builder commit reached port/stage3 at 11:59.

**Play build: `Builds/e53-freeze2/SKYGRIND.exe`** (commit 0687ddbc: the EXT53 freeze c1ff2591 + builder A5's foundry a5; tag u3-n53x). `-skygrindOff53` turns off tonight's
polish and RIDE KILL; the rooms are edited in place (compare with Builds/n53-freeze or Builds/n52-freeze).

## New: RIDE KILL (rideKill53, ON)
Your words (OWNER.md, w29, 2026-10-09): "the rooms still don't incentivize wall-riding". A kill while you ride a wall (or just after leaving it) gives one boost back,
and "RIDE KILL +1 BOOST" slams in above the reticle in cyan (just "RIDE KILL" when your boost bar was already full). It stacks with the old
"+WALLKILL" style points: those are style, this is movement. Proof on e53-freeze: a253 GREEN with the row on ("refund yes (boosts 0.31 ->
1.31) ... callout yes ... word px 3566 vs 127 before"), RED off; I looked at the frame and the word is there. Golden 22/22 exact and
fingerprints EQUAL with the row on and off. A kill on foot does not count.

## Rooms (the template problem, continued)
| room | change | identity |
|---|---|---|
| iceline g2 THE SHIELD | the west is the flank, the east is the face: the east ride panel, deck, ramp, spring and pillar cut, the east wall 3 m in, a quench tank, a run-up step into the one ride, a yard spring looping over the warden | I1 100 -> 31 %; every computed item PASS |
| foundry b1 THE SPILL | the ladle tipped west, one crane; the first file ambushes from behind the ladle (opening now "ambushed": foundry's I7 passes); the west crane rail runs the whole west wall (32 m) | I1 99 -> 36 %; every computed item PASS |
| iceline g8 THE ROUNDHOUSE | the west wall 1 m in, the gallery ramp moved, the NW column cut, the cross hoist off the walk line | I1 90 -> 39 %, I4 PASS; I2 1.10 still FAIL |
| spire v2 THE NAVE | the bell 2 m west under its hole, the east narthex wing cut, the east aisle ends at the arcade pier | I1 98 -> 55 %; I2 1.17 still FAIL |
| kiln kb2c THE ASH BED | THE SLAG RUN, a lava channel, cuts the east lane: the 25 m ride is the east side's way north (38 m on foot) | I1 72 -> 55 %, I2 1.07 -> 1.53, I4 PASS: every computed item PASS |
| span p3 KNOT YARD | the void runs to the east wall; the big east stand is now a small winch balcony you pull to | I1 95 -> 55 %: every computed item PASS |
| foundry a5 THE POURING FLOOR (builder A5, after the freeze) | the skim ramp beside the crane rail cut (the rail is the fast way to the ladle ledge), the ledge's ramp turned east, THE CHARGING BRIDGE (a spitter perch off the gantry), the pads off the walk lines | I1 87 -> 57 %, I2 1.08 -> 2.14, I4 PASS: every computed item PASS |
| spire v2 / v5 / v6 pads | moved off the walking lines (a search tool found spots I had called impossible) | I4 PASS on all three |
All 18 rooms: identity FAIL items **33 (start of NIGHT53) -> 27 (NIGHT53 close) -> 14 (EXT53 freeze) -> 11 (with a5)**; rooms
passing every computed check **0 -> 2 -> 8 -> 9** (Logs/n53/identity-e53-freeze.md; Logs/n53/identity-e53-final.md); one-shape (I5) and openings (I7) pass on all five levels. Every changed room passes THE ROOM CONTRACT.

## Bugs fixed
- **AutopilotTests.PerStep_AllocatesNothing**, the test that has flaked since NIGHT14: builder B read the cause from Unity's IL (not
  reproduced): the test made a new GC.Alloc recorder wrapper twice a step, whose finalizer released the shared native recorder mid-measure;
  the game's path allocates nothing. Fixed in the test (and ZeroAllocTests); passed in three Editor runs and the full suite: not yet a
  flake proof (a flake is rare).
- **The THUNDERCLAP's first-shot hitch** (T-122): its first shot built fx47's sprite sheets (~240 materials and meshes) inside the frame.
  Now built at the run's first frame: the THUNDERCLAP's first shot 5.5-6.0 ms on the freeze build (15.6-17.5 ms off; 5.3-9.0 vs 34.2 on
  n53-r3). ON.
- **a152's THUNDERCLAP "never fires"** (T-125), diagnosed, not fixed: the game fires the taps (a headless probe replays them); the cause
  in the Player reel is NOT found; a152 now prints the candidates (its next run names which; a152 was not run tonight). Kiln's short teach line: the queue rule working as written (measured), not a bug.
- **Level errors left by tonight's room edits**: two in iceline (a spawn node off the nav grid, a floating lamp) and one in span (a spawn
  8.6 m in front of the new start): fixed; the levels report the errors they had at NIGHT52. Also fixed: g2's east gantry ran 3 m into its
  moved wall (the review's find, not a report error).
- Not fixed: **the kill wash** (T-111): its f3-f4 source is the fx49 kill ring; cutting it inside the wash box brings the peak to
  +0.275-0.315 (from +0.32-0.43), still above the +0.25 target: shipped OFF.

## Measured (EXT53)
- The full batch suite at the freeze commit c1ff2591: **2258 / 2258 passed** (and 2257 / 2257 at 762ea622 earlier). The a5 commit after it
  is level data only: verbs37 foundry 27/27, a452 foundry GREEN, a253 GREEN and the smoke DONE on e53-freeze2 (the suite not re-run).
- verbs37 on e53-freeze with RIDE KILL on: foundry 27/27, iceline 21/21, kiln 31/31, span 31/31, spire 31/31: nothing lost (foundry's
  wall-ride row went RED once on n53-r3 after b1's re-cut: a step in a new 25 m wall face dropped riders; the rail now runs the whole wall).
  a452 GREEN on all five levels; 16 freeze reels, 0 NRE (these ran with killWash forced on in the build's copy for its proof; the shipped
  config has it off: a view item). The smoke on e53-freeze, with the shipped config: DONE, all steps passed (227.8 s).
- Golden 22/22 exact and fingerprints plain / --off52 / --off53 EQUAL at the freeze, with RIDE KILL on (its fingerprint run has no ride
  kill: that gate cannot see the row; the sim tests and a253 are its proof).

## Not done, said plainly
- The EXT53 reviewers' pattern: the rooms break their mirrors by cutting one side; few cuts add a piece with a job. Their geometry asks
  are NIGHT54's (DECIDE53 D-22). I2 (a ride beside a walkable lane) still fails in g3, g4, g5, g7, g8, v2; I1 in a3, g5, g7; I4 in kb2, p5.
- No human has played RIDE KILL or the re-cut rooms; the numbers are the walk pilot's, the probes' and the reels'.

