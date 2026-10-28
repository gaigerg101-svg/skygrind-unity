# FORGE70 code + data scout (read-only; 2026-10-25 ~22:10; HEAD 89c7a5e8 = u3-n69, branch port/stage3)

Read: NIGHT69-REPORT.md, NIGHT69-LOG.md, DECIDE69.md, R.md / S.md (`git show n69-R:` / `n69-S:Research/2026-10-25-wave69/`), Logs/n69/r1-*,
r2-kiln, room-chain-1.txt, and the code cited. Ran git, grep, read-only node (one scratch reader of the skip files, in the session scratchpad),
`df`, `du` and one CIM read. No dotnet, harness, build, Player or Editor. Settled in FORGE69's scout and unchanged: X4 (rooms alone), X5 / Y3
(patch share), X6 / Y1 (STYLE): not repeated.

**Three facts first.** (1) **A launched husk always dies in its own burst**, so for a husk "ends in a pit" (M3) can only happen when the throw
hits NOTHING: the burst deals 60 to every enemy within 3.5 m, the flier included (Launch36.cs:4, :211-229; config design.n36.launch36
burstDamage 60, burstRadius 3.5: Design/unity-config.json:3905-3906); husk hp 40, spitter 60. (2) **M3 is ONE probe on seed 1**; tonight both
probes killed bodies and FAILed: kb3 "bursts 1 (hits 4, kills 4) ... in a pit no" (Logs/n69/r1-kiln/verbs-launch.txt:9-13), v2 "kills 3"
(r1-spire/verbs-launch.txt:9-13). The "0.3-2.5 m flights" are these two lines. (3) **skip v2 prints `falls 0` because it never counts a fall
return:** it subscribes to "stageRespawn" only (Tools/SimHarness/Harness.N65.Verb.cs:977); the quench return emits "fallReturn43"
(Assets/Scripts/Core/Campaign24.cs:1038), which n46flow counts (Harness.N46.Flow.cs:233-234). The flow's 52-78 falls are right.

## A. The launch's real flight

**The throw (all from code + config).** speed 16 m/s, lift 5 m/s, flightS 0.8 s, armS 0.05, graceS 0.15, hitPad 0.3, wallProbe 0.35,
wallMaxNormalY 0.5 (unity-config.json:3897-3911). Launch sets the body's knockback velocity 16 m/s for 0.8 s and vy = 5 (Launch36.cs:105-113;
Combat.cs:78-85; Husk.cs:363-370). Enemy gravity = 2 x jump.height 1.5 / apexTime 0.36^2 = 23.1 m/s^2 (EnemyCrowd.cs:191, MovementMath.cs:102,
PortExport/config.json jump). So: airborne 0.43 s, apex 0.54 m, **first touch-down ~6.9 m** out; then it SLIDES at full speed (Husk.cs:264-267
"standing && knocked: slide with the knockback velocity", no friction) to **~12.8 m** at 0.8 s (R's clean throws 11.1-14 m: R.md:47-48).
A charger (hp 150, charger.knockbackScale 0.3: Charger.cs:153) flies 0.3 x 16 m/s: **~3.8 m**, and survives a burst.
**What ends a flight** (Launch36.cs:137-178), checked each step in this order:
- a live body within h.r + o.r + hitPad = 0.4 + 0.4 + 0.3 = **1.1 m** of the step's swept segment, heights overlapping (:182-198) -> body burst
  at the midpoint; the flier is never above 0.54 m, so any standing body on the line catches it;
- after armS (6 steps, ~0.8 m) a face with |n.y| < 0.5 within 0.35 m ahead, probe lifted 0.25 m (:156-161, :202-209) -> wall burst (kills the
  flier: the `kills 1 wall` lines);
- age >= flightSteps (96) or grounded with knockSteps 0 (the same moment) -> "timeout"/"landed", no burst (:162-167). The floor never ends it.
**Why 0.3-2.5 m:** the pilot walks straight at its target (Verb.cs:305-312 moveZ 1, yaw at the body; the skip layer the same:
Harness.N65.VerbPilot.cs:469-475) and the throw goes along that aim, so the queue behind the target is ON the throw line and the 1.1 m test
fires at once (kb3 burst 2.5 m: hits 4; v2 0.3 m: hits 3). Not a short throw: a blocked one.
**The launch bound** (R's "below the launch bound"): bound = max(punch.damage 10 x momentum.meleeMax 3 = **30 hp**, the largest non-parry punch
dealt in the run) (VerbPilot.cs:212, :299-305; probe Verb.cs:256-264; printed "the launch bound 30.0": r1-kiln/verbs-launch.txt:7). An episode
starts only on a husk-shaped body (no stand-in, no tank) within **5.5 m flat and 50 deg** of the look, hp above the bound, not about to parry
(VerbPilot.cs:340-347). A 40 hp husk must have taken <= 10 gun damage by 5.5 m; swarmers (10 hp) never qualify (seed 1: "skipped 510 steps ...
swarmer hp 10": r1-kiln/verbs-skip.txt seed 1 line). The probe dealt 16, not 30 (verbs-launch.txt:8): the bound is the pilot's worst case.
**The skip runs' launches (my reader over the saved files):**

| file | launches A (20 seeds) | per seed | launchkill bursts | kills | wall / body | kills >= 3 | below y -0.2 |
|---|---|---|---|---|---|---|---|
| kb3 r1d a1 | 57 | 1-6 | 26 | 52 | 12 / 14 | 6 | 0 |
| kb3 r2b a2 | 83 | 1-8 | 40 | 82 | 24 / 16 | 12 | 5 |
| v2 s1b a1 | 45 | 1-5 | 23 | 57 | 10 / 13 | 12 | 2 |

So ~1 kill per launch, 2.3-4.2 launches a seed against ~23 bodies: the launch count, not the pit, caps the ratio.
**The geometry that catches a throw** (derived from the numbers above; not measured in a room):
- along the approach line (the pilot's straight line to the punch), the pit's near rim at <= ~6.9 m from the punch spot (where the arc comes
  down) or anywhere up to ~12 m (the slide); a pit that begins AND ends inside 0-6.9 m is flown over (R.md:47-49: "flown over, not landed in");
- for a husk, a pit narrower than the fall's run (2 m deep: sqrt(4 / 23.1) = 0.42 s x 16 = **6.7 m**; 1.5 m deep: 5.8 m) is hit on its far
  wall below the rim -> a wall burst IN the pit (the flier dies; the probe's "in a pit YES" reads the death point, y < -0.2: Verb.cs:350-364);
  a far wall under ~2.5 m from the rim kills it at y > -0.2 (no "in a pit"): pit length 2.5-6.5 m along the throw;
- width across the throw: >= 2 x 1.1 m plus the aim's spread (not measured);
- NO live body within 1.1 m of the line before the pit: the queue must stand off the approach line, or bodies must arrive one at a time;
- depth only matters for a body that survives (a timeout landing, or a charger): rooms43 ends it after 2 s more than pitDropM 1.5 m under the
  room's floor (SurvivalSim.N43.Rooms.cs:160-171; unity-config.json:6585-6588); a charger's pit is within ~3.8 m of the punch. Lava ends any
  body in enemyBurnSeconds 0.8 (Lava30.cs:173-177).
- What a burst needs: a body or a wall face on the line after 0.8 m. A wall 1-12 m past the punch on the approach line is a guaranteed burst
  (kills the flier and all within 3.5 m); bodies against that wall make it a multi-kill.
**A command for the landings.** None prints every launch's landing in a skip run: skip v2 keeps only bursts with kills > 0, run A only
(Verb.cs:985, printed :1283-1284 `n65verb skip launchkill: room | seed | at (x, y, z) kills N [wall]`); no launch point, no distance, no
timeout landings. What a builder can run TODAY in its own tree (level from `node Tools/night/gen-slots.cjs --level kiln --slots=kb3 --run
--out=<dir>`, R.md:3-4):
```
dotnet build Tools/SimHarness -c Release -nologo -v q
for s in $(seq 1 20); do dotnet Temp/SimHarness/bin/Release/net10.0/SimHarness.dll n65verb launch --level <dir>/n67-kiln.json --room kb3 --seed $s \
  | grep -E '^n65verb launch: |^  burst at|^  target chosen'; done > <dir>/launch20.txt     # one throw per seed: thrown, end, landed, in a pit
bash Tools/night/n65-verbs.sh skip <dir>/n67-kiln.json kb3 --seeds 20 --bars Logs/n69/slate.json | grep 'skip launchkill'   # the kill bursts
```
(`--wave n` / `--target x,y,z` aim the probe at a later wave or a spot: Verb.cs:194-218.) The probe is wave 1's nearest body from line65[0]
only. **Smallest add (~25 lines, print-only):** in V65Clear (Verb.cs:972-985) record each LaunchEvent id's position and step; after each
sim.Step diff `sim.launch36Core.flights` (public, Launch36.cs:37) and, for an id that left, print `n65verb skip launchend: room | seed |
from (x, z) to (x, y, z) thrown d m | end <lc.lastEnd> | in pit yes/no` (lastEnd is per step: two flights ending in one step share it). A new
line kind, not `launchkill` (the area lint and deco read launchkill: n69-area.cjs:143, n69-deco.cjs:114). Planted pair: a 3-seed kb3 run
before/after, every other line byte-identical.

## B. M3 and the burst (DECIDE69 Q-A)
- Today M3 = the probe's line is `: GREEN`, `launch36 [1-9]`, and "in a pit YES" or "in lava YES" on the launched body (n65-verbs.sh:18-29;
  the same test in n69-gate-pack.cjs:186-195 launchM3); the probe's GREEN is punched + launched + (thrown >= 1 m or a burst) (Verb.cs:373);
  the slate's text (Logs/n69/slate.json, M3 line). The bursts are already on the verdict line: "bursts N (hits h, kills k)" (Verb.cs:374).
- Smallest change: one more PASS branch in both readers: n65-verbs.sh:26 `elif echo "$vl" | grep -q -E "bursts [1-9][0-9]* \(hits [0-9]+,
  kills ([2-9]|[1-9][0-9]+)\)"; then m3=PASS; m3why="burst"` and in launchM3 a regex on the same text, `+k >= 2`; the slate's M3 text says
  which; plant three new cases in n69-m3-cases.sh's set (GREEN kills 4 pit no -> PASS; GREEN kills 1 wall -> FAIL; RED kills 3 -> FAIL).
  "Kills" counts the flier: kills 2 = the flier + one other. On tonight's saved lines it turns kb3 and v2 M3 to PASS (kills 4 / 3).
- Better: M3 over 20 probe seeds (section A's loop), PASS on a share (e.g. >= 10 of 20 with a pit end or a >= 2-kill burst): one seed is
  one sample of a 0.67-1.80 spread. Written before any measurement.
- Is the burst kill the launch's in skip v2? The kill's weapon is "launch36" (Launch36.cs:238), printed only as `launchkill` lines; M2 is
  clear time only, so the burst helps M2 through the clock and nothing counts it as the verb's pay.

## C. The ride's payer (DECIDE69 Q-C)
- **The rule:** Assets/Scripts/Game/MapAuthoring/LevelValidate24.cs:219-258 "enemies reach every tier: <room>": every encounter node AND
  every box of kind `ledge` with top >= 1 m in the room's zone must be reached from the floor spawn (or `reachFrom`) on Link.MaskWalker =
  walk + drop + long drop only (NavField.cs:26). highNodes are added to the room's nodes (Campaign24.cs:324), so a "high node" is checked too.
  GG's source: "There always really needs to be a route that enemies can climb up" (NIGHT20.md). (g8 passes: Logs/n69/r1-iceline/LEVEL-n67-iceline-validate.md:19.)
- **Mechanisms that could hold a ranged body:** (a) the LANCER: `k.stationary`, no movement (HuskBrain.cs:882-888), "perches on high points
  ... never walks: it blinks between perches" = the director's high spawn nodes (Enemies/Lancer.cs:3-11); the campaign can spawn one
  (SurvivalSim.Campaign24.cs:77-81) and old level07's waves do (Design/levels/level07.json); its node must still pass the walker check; where
  it blinks in a campaign room: not checked. A new kind in a room's waves = enemy design (GG's; D-10's precedent). (b) The check reads only kind
  `ledge` (:222-226): a `deck` / `pillar` top is not a tier to it: a loophole against GG's rule, not a mechanism. (c) A long walker route + a
  short ride: legal, but S measured spitters leave their perch for a sight line (pocket 65 vs floor/stair 352 sightings: S.md:31-34).
- **The pilot:** rides only line65's face (Harness.N68.Ride.cs:1-3; the episode VerbPilot.cs:10-12: face within 1.6 m, jump held, along to
  1.5 m from the end); the refused run drops the jump's hold in the air, and in a ride room refused-in = walker (one run: NIGHT69-LOG.md:46).
- **What would make the ride faster:** the clear is kill-bound (S.md:40-41; the ride saves 2.3 m: DECIDE69.md:48-49; verb-aware 44.1 vs 41.2 s:
  r1-iceline/verbs-skip.txt totals), so the ride pays only if the LAST bodies stand where the walk is long and the ride short, and stay
  there (a stationary kind or a perch with sight over the whole floor), or the clear has a non-kill end (a switch / exit only the ride
  reaches: a rule change). Neither exists in g8 today.
- **Other verbs for g8:** launch was measured: 0.98 shipped, 0.97 r1a, 1.09 r1c (Research/2026-10-24-wave67/cards/g8.md:6-7); slide and parry
  on g8: none found.

## D. The noise (exercised seeds of the a1 / a2 skip files; normal-approx median SE = 1.2533 sd / sqrt(n); bootstrap beside)

| room | per-seed sd | range | 20-seed median SE (boot) | 40-seed (boot) | sets 1-10 / 11-20 |
|---|---|---|---|---|---|
| kb3 r1d | 0.238 | 0.81-1.90 | 0.067 (0.046) | 0.047 (0.030) | 1.06 / 1.13 |
| kb3 r2b | 0.203 | 0.83-1.60 | 0.057 (0.039) | 0.040 (0.030) | 1.04 / 1.16 |
| v2 s1b | 0.253 | 0.57-1.36 | 0.071 (0.098) | 0.050 (0.074) | 1.04 / 0.95 |
| g8 r1f (17) | 0.315 | 0.46-1.52 | 0.088 (0.077) | 0.063 (0.044) | 0.84 / 1.00 |

Two independent 20-seed medians differ by ~0.10 by chance (sqrt 2 x 0.07); at 40 seeds ~0.07. The gaps (kb3 -0.25, v2 -0.26, g8 -0.39 to
1.35) are 3.5-5 SE: no re-roll closes them. **Wall time:** skip 20 seeds kb3 323 s, v2 218 s, g8 265 s (each file's totals line); fresh 10
seeds 174 / 114 / 118 s; one alone round 452 / 321 / 380 s ("done in", r1-*-run.txt:10); the a1 chain of three rooms with fresh and patch
26.6 min (room-chain-1.txt:1-15). `--seeds 40` works (sets of 10, need 70 %: Verb.cs:1231-1244) but n69-round-verbs.sh hard-codes
`--seeds 20` (:46, :58, :59). A 40-seed round ~ round + one more skip + fresh: kb3 ~16 min, v2 ~11, g8 ~13: **~10-12 rooms per 2:40 block
in one tree, serially** (two dotnet jobs never share a tree); screening skips alone (40 seeds 7-11 min): ~15-20. Parallel trees: CPU
contention not measured.

## E. The n70 tools
- forge69-make-n69.cjs copies every `n68-*.cjs|sh` to n69 with common renames (n68- -> n69-, Logs/n68, `'Logs', 'n68'`, NIGHT68; :16-20),
  special exact-once edits (:25-56: gates.sh builds both harnesses once; round.sh SLOTS; clock 195 / 240 / 420; close-check cap option + a
  4-hour planted case), refuses to overwrite (:61). A forge70 copy must start from `n69-*` (it then also picks up the n69-only tools:
  n69-next, n69-patch-share, n69-m3-*) and must NOT replay n69's specials (already in the text). Literals the common rename misses:
  `'n69'` in n69-clock.cjs:13, n69-close-check.cjs:54 and :252, n69-next.cjs:18, `'Logs', 'n69'` in n69-spot-pair.cjs:14, :19.
- **8-hour clock:** n70-clock DEF `freezeMin: 435, capMin: 480, staleMin: 720` (n68's: forge69-make-n69.cjs:47). close-check derives every
  window from capMin (start >= cap - 60 = 7:00, stop 7:30-8:00: n69-close-check.cjs:77, :91-99) and **its default self-test clock is already
  8 hours** (`capMin: o.cap || 480`, :252); the 4-hour case is the extra planted one. The guard needs no change: it reads Logs/clock.json's
  freezeMin / capMin / staleMin (guard.cjs:85-95), at FREEZE only heavy steps tagged with a closeTags tag and no test run (:248-254), no new
  agent (:302-303); disk: heavy scripts (n*-build / n*-reels / reel / n*-smoke* / n*-focus-probe .sh, unity build, batch) refused under 12 GB
  (:27, :241-244); a direct `SKYGRIND.exe` call is not covered (NIGHT69's D-11).
- **What broke or misled:** (1) `git rev-parse --short u3-n69 HEAD` -> "fatal: Needed a single revision": `--short` implies `--verify`,
  which takes exactly one name (reproduced now; `git rev-parse --short u3-n69` alone prints 89c7a5e8). Inside the close-check the same text
  comes from :115, which tries `rev-parse --short u3-n69^{commit}` BEFORE the tag exists; the try catches it but the `git` helper (:56) sets no
  stdio, so git's stderr reaches the console (reproduced with a missing tag). Fix: `stdio: ['ignore', 'pipe', 'ignore']` on that call, and
  write one-name rev-parse lines in HANDOFF. (2) n69-next.cjs: STOP_MIN 140, one stop (:19); an 8-hour night had two (NIGHT68 ~1:13 and
  ~6:24); it reads no room check (<round>/<id>/room-check.txt "ROOM kb3: FAIL R5 foothold") and no air65 (<round>/air.txt): two ok() lines.
  (3) skip v2's falls (fact 3). (4) M3 on one seed (fact 2). (5) the 20 fixed seeds (D).

## F. The state (22:00-22:10)
- C: **7.9 GB free** (`df -BM /c` 7928M, 21:59); commit free **19.6 of 100.9 GB**, physical free 24.6 GB; four Unity processes, three at
  7.3-7.4 GB private (CIM, 22:00). An n70 freeze build / reel set / smoke script needs 12 GB free (guard.cjs:27): **4.1 GB short**.
- Builds/: 258 folders; HANDOFF.md:46 puts the old builds at about 25 GB (DECIDE68 Q0). My per-folder `du -sm -l` (hard links counted in each
  folder; it did not finish in the time) had reached 45 of 258 folders, alphabetical to n27-*: largest e53-freeze 374 MB, e53-freeze2 374 MB,
  n27-1..3 288-289 MB, Dev 229 MB, n20-* / n21-* 177 MB each; the n28-n69 folders: not measured. Freeing space means removing every link of a file (n69-play and n65-play-latest are hard
  links of n66-freeze); deleting is GG's (DECIDE69 "the night never deletes anything").
- Open (GG's): DECIDE69 **Q-A** (burst vs pit end), **Q-B** (faster of two refusals; answered by measurement, costs 0-0.14, no room reaches a
  bar against any single refusal: DECIDE69.md:44-47), **Q-C** (a ride's payer), **Q-D** (try Builds/n69-play), **Q-E** (the disk = DECIDE68
  Q0); DECIDE68 **Q6** (kb3d's wave 3), **Q7** (the melee-only room); eye63's blind X / Y; Builds/n65-play-latest (no reaction).

## The five things NIGHT70 must change, with the evidence
1. **Measure every launch before any room:** add the `launchend` print and run the 20-seed probe loop on the shipped rooms first. Today no
   line gives a skip run's landings (Verb.cs:985 keeps kill bursts only) and the "0.3-2.5 m" is two single-seed probes, both blocked by the
   queue at the 1.1 m test (verbs-launch.txt:9-13; Launch36.cs:182-198); the clean throw is ~12.8 m (16 m/s x 0.8 s, sliding).
2. **Rewrite M3 (Q-A) before measuring:** a husk always dies in its own burst (60 dmg, 3.5 m, flier included: Launch36.cs:211-229), so a pit
   end only happens when the throw hits nothing; tonight's M3 FAILs killed 4 and 3. Count a >= 2-kill burst (or a pit / lava end) on >= 10 of
   20 probe seeds; two readers + three planted cases (n65-verbs.sh:26, n69-gate-pack.cjs:190-195).
3. **Build launch rooms around the throw's numbers:** fresh bodies (above the 30 hp bound, unshot) entering 5.5 m at a corner or door, and
   a wall or a 2.5-6.5 m-long pit 7-12 m past the punch spot on the approach line with no body within 1.1 m of it; 2.3-4.2 launches a seed
   for ~1 kill each is the ceiling now (the skip files; VerbPilot.cs:303-347).
4. **Stop reading differences under ~0.10 and fix the instrument's prints:** 20-seed median SE 0.06-0.09, 40 seeds 0.04-0.06; let rounds
   take `SEEDS` (n69-round-verbs.sh:46, :58-59; ~10-12 40-seed rooms per 2:40 tree); count fallReturn43 in skip v2 (Verb.cs:977 vs
   N46.Flow.cs:234); n70-next reads room-check / air65 and two stops; the 8-hour clock 435 / 480 / 720.
5. **Take g8 off the ride until GG answers Q-C:** under LevelValidate24.cs:219-258 every node and ledge is walker-reachable, spitters leave
   perches (S.md:31-34), the clear is kill-bound (ride 44.1 s vs refusal 41.2 s); the only stationary ranged body is the lancer, an enemy
   kind (GG's). Launch on g8 read 0.97-1.09 (g8.md:6-7); give the slot's time to kb3 / v2.
