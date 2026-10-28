# NIGHT69 builder R: kb3 THE PRESS HALL (notes; tree .claude/worktrees/agent-a9d0ffb6d0dfdb2be, branch n69-R from d9e22d48)

All numbers: skip v2 (`bash Tools/night/n65-verbs.sh skip Design/levels/n67-kiln.json kb3 --seeds 20 --bars <main>/Logs/n69/slate.json`) on
the level assembled with ONLY kb3 new (`node Tools/night/gen-slots.cjs --level kiln --slots=kb3 --run`), R's tree, the t0 harness (no Y1 /
Y2), seeds 1-20; M3 = `n65-verbs.sh launch ... --seed 1..3` (t0 probe). Outputs under Research/2026-10-25-wave69/kb3/<variant>/, each with
its command, sha, room-file hash, level sha256 and time at the top. Made by Research/2026-10-25-wave69/kb3/r1/make-kb3-n69r1.cjs <variant>.

## The variants (all from the blank rect; forge wall, far gallery, quench vent kept: patch share 19 % PASS on each)
| variant | what changed | pooled (sets) | verb / refused / walker median s | deaths v / r / w | launch36 | M3 |
|---|---|---|---|---|---|---|
| r1 (0a0724a0) | a 4.5 m lane beside one 2 m quench, the press bank, two presses | 1.01 (1.20 / 0.96) | 43.7 / 49.4 / 49.8 | 0 / 1 / 1 | 67 | FAIL 3/3 |
| r1c (4e7c579a) | a 3 m bed between two 2 m quenches; bank ramp from the far floor | 1.08 (1.07 / 1.09) | 41.0 / 46.7 / 50.0 | 1 / 5 / 5 | 69 | FAIL 3/3 |
| **r1d (12819ed5, a1's file)** | the bank's climb moved to the apron (a stair); press 1 cut for it | 1.08 (1.06 / 1.13) | 41.5 / 46.1 / 46.2 | 0 / 2 / 2 | 57 | FAIL 3/3 |
| r1e (750c7dab, cand) | r1d + wave 4's four swarmers -> husks | 1.12 (1.12 / 1.13) | 37.4 / 45.8 / 45.7 | 0 / 2 / 2 | 65 | not run |
| r1f (cand) | r1e + line65 from the apron east of the mouth | (skip not run: line65 is not a skip input) | | | | FAIL 3/3 |
| r1g (cand) | r1f + the bed 2 m + wave 1's two spitters -> husks | 1.10 (1.13 / 0.97, 19/20) | 38.8 / 46.0 / 44.7 | 1 / 2 / 1 | 62 | FAIL 3/3 |
| r2a (980b1a18) | r1e + THE DROP PRESS (the entry's foothold, R5) | 1.02 (1.10 / 0.99) | 39.6 / 47.2 / 43.8 | 1 / 3 / 1 | 70 | FAIL 3/3 |
| r2b (cand) | r2a + the vent 2 m north (I4) and walk proof 1 from the spawn's spot | 1.09 (1.04 / 1.16) | 42.5 / 45.8 / 49.7 | 1 / 3 / 3 | 83 | FAIL 3/3 |

| r2c (4937fa4c) | r2b without fallReturn43 | 0.94 (0.93 / 1.01) | 36.9 / 38.7 / 39.0 | 0 / 0 / 1 | 74 | - |
| r3a (a7c18c4c) | r2b + a1 review's changes: east quench to z -204, wave 1 husks, the press at the red's spot, line65 to the mouth | 0.94 (0.96 / 0.94, 19/20) | 43.1 / 42.9 / 46.0 | 0 / 0 / 3 | 66 | FAIL 3/3 |

Guards (`ROOMS=kb3 bash Tools/night/n69-red.sh 2 n67-kiln <dir>`, flow A / B: deaths, stall A, falls): r2a 0 / 1, 20.7 %, 50 / 66;
r2b 1 / 2, 19.8 %, 57 / 53 (the same on the Y1 / Y2 build, kb3/fix-r2b/red/); r2c 1 / 0, 23.7 %, 0 / 0; r3a 1 / 1, 20.6 %, 88 / 77 (B
stall 23.9 %). **FIX candidate: r2b (626eb75c)**: the best pooled ratio of the variants that hold R5 and I4; its flow A deaths 1 is over
the ceiling 0 (r2a, which differs only by the vent's spot, read 0: one death in ten runs is inside the noise, not checked further).

a1 on main (Logs/n69/r1-kiln/, r1d): M2 1.08 (1.06 / 1.13), the same line as R's tree; M3 FAIL under Y2's reader (the same throw as R's
r1d seed 1); room check R5 FAIL and I4 FAIL; deaths A 0 / B 2, stall A 17.9 %; flow A falls 69. r2a / r2b: R5 PASS, IN VIEW PASS; r2b I4
PASS (`python -I Tools/night/n59-l-room.py n67-kiln room kb3`: `ROOM kb3: PASS`).

r1d also: area lint `PASS kb3 (4/4)` and deco `PASS ... 5 pass` with r1d's touches (kb3/r1d/area-touches.txt, deco-touches.txt).
No variant is within 0.10 of the bar (1.33); the differences between variants (0.04-0.07) are under the noise (0.10 on a 20-seed pool).

## The causes found
1. **The launch is a small part of every clear.** The verb-aware run launches 1-6 times a run against 23 bodies (r1d per seed: launch36
   1-6, kb3/r1d/skip20.txt). A launch needs a body within 5.5 m, in the look, above the launch bound (24.4-30 hp) and not winding up; the
   gun has usually taken a body under the bound before it comes that close, and a swarmer (10 hp) never qualifies. So the room's ratio is
   set by the few launches that burst a queue (4-6 kills at once: kb3/r1c/launch-m3.txt seeds 1 and 3). Cutting the swarmers (r1e) gave
   +0.04, inside the noise.
2. **M3: the launched body never ends in a quench, and the reason is the throw's direction, not the pit.** The probe's pilot walks at the
   nearest body, dashes inside 5.5 m and punches at 0.9 x reach (Tools/SimHarness/Harness.N65.Verb.cs:305-330); the throw goes along the
   punch's aim. On a bed the pilot meets the body inside the bed, so the aim is the bed's axis: the body bursts on the next one (r1c seeds 1,
   3: hits 5-6) or lands on the bed (r1d: (0, -211.5)). When the dash carries the pilot past the body it turns and throws it BACK toward
   the apron (r1f seed 1: a husk at (-0.2, -209) thrown 11.2 m north to the north wall). Floor-spawned spitters (wave 1) were the nearest
   target on 4 of 6 probes, and a launched spitter flew 3.7-4.9 m and came back to its start (thrown 0.0 m, flight end timeout).
   Two more facts from the probes: a CLEAN throw (no body or wall in the way) flies 11-14 m (r1g seeds 1-2: 12.5 m to (-7.6, -200.8), 11.1 m
   into the forge wall), so a quench 3-8 m wide is flown over, not landed in: the pit has to be where the throw comes DOWN; and the pilot
   walks straight at its target (moveZ 1, the yaw at the body: Harness.N65.Verb.cs:308-312), so from any pose off the bed's axis its straight
   line crosses a quench and it falls. Hence: on a straight bed fed from one end, a straight-walking pilot can only throw along the bed's
   axis (into the queue: a burst, a kill, but no pit) or, after a dash carries it past the body, back toward the apron. No geometry tried
   tonight puts the throw's landing over a pit; the 4-6-kill queue bursts are the verb's real pay here.
3. **The bodies avoid the bed when they can.** r1c's bank ramp from the far floor took them over the bank and down to the apron (r1c M3
   targets on the bank, (-7.5, 2.7, -216.2)); r1d moved the bank's climb to the apron, so the bed is the only floor way north.
4. **Falls:** two quenches beside a 3 m bed cost the skip runs deaths 1 / 5 / 5 (r1c) with the bank ramp in the far floor and 0 / 2 / 2
   with the stair on the apron (r1d); the flow guards (deaths A 0 / B 2, stall A 22.7 %) were not run in R's tree (a1 runs them).

5. **The fall return is part of the ratio.** Without it (r2c) the falls went 57 -> 0 and every run got faster (verb-aware 42.5 -> 36.9 s,
   refused 45.8 -> 38.7 s), the refusal most: the ratio fell 1.09 -> 0.94 and flow A's stall rose to 23.7 %. The 10-health return slows the
   refusal more than the verb run; it is also the corner NIGHT69 names (a pit beside the walk: falls 50-88 a set, shipped 0).
6. **a1's review changes (r3a) made every line worse** (0.94; A deaths 1, falls 88; B stall 23.9 %), so the fix candidate keeps r2b's
   geometry. The review's reading of M3 ("line65[2..3] ... so the dash direction is the bed's axis") is not how the probe aims: it takes
   the pose and first segment from line65 and then walks straight at the nearest body (Harness.N65.Verb.cs:106-119, 305-312); ending
   line65 at the mouth also FAILs P5 (its nearest spawn 12 m).

## For the next forge (R's reading, from the files above)
- A launch pit must be where a throw comes DOWN (8-14 m from the punch, or against a wall just past a narrow pit), not 1.5 m beside it.
- The ratio of this room sits at 1.0-1.1 whatever the middle does (seven layouts): the verb-aware run launches 1-6 times in 23 bodies
  because the gun takes bodies under the launch bound first. A launch room needs bodies that reach the player fresh (out of the gun's
  sight until inside 5.5 m) or that the gun cannot kill quickly (chargers), before any pit can pay.

## What could not be checked
- The guards (flow A / B / V) for any variant: not run (a1 runs them for r1d on main).
- Y1 / Y2 on R's tree: not synced; the STYLE lines above are the t0 rule's.
