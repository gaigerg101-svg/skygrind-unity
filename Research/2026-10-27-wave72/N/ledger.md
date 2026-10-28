# NIGHT72 builder N ledger (NAVIGATION & MOVEMENT)

Tree: .claude/worktrees/n72-N, branch n72-N, from t0b b80afc2fa. t0 = 2026-10-27 10:19:20 MST (the lead's clock). Every number below names its
file under Research/2026-10-27-wave72/N/evidence/ (command, sha and time at the top of each file). Anything not run is "not checked".

## N0 The level rows' files (started 10:32, level files done 11:10)

### What the slate said, and what was true
- Slate: re-export a3 (roof false), `--check` PASS, `--run --slots=b1,a8`, three `gen-slots` runs, n67 sha1 equal to GG's play build.
- Found 1 (spike.md S3's first trap): `Design/levels/n65/rooms-n52/a3.json` still read `"roof": true`. `gen-n65-foundry.cjs --export a3` (no --force)
  rewrote it: roof false, plus the `l1.wallrun` teach35 entry the n52 plan has had since 0e3f95ea5. rooms/a3.json (the old builder's file) kept.
- Found 2 (new): with a3 roofless the generator REFUSES. `--run --slots=b1,a8` stopped with levelgen's 2 ERROR rows
  (evidence/n0-foundry-try1.txt): `closedRoom: a3: no roof: true` and `lights: a3-L1, a3-L2, a3-L4 not flush to a surface` (the three ceiling
  lights hung from the slab, y 9.95). The standard (LevelValidate29.ClosedRoom) demands `roof: true` on every non-boss room. The shipped files
  were never produced by the generator after the hotfix.
  Fix (Tools/night/gen-n65-foundry.cjs, my slice's generator): exactly those two rows, by name, and only while the plan's a3 says roof false,
  are allowed (`unexpectedErrorRows`); any other ERROR row or any other a3 light still refuses `--run` and `--finish`. Two planted cases in
  `--check` (the allow-list passes the own rows only with a3 roofless; another light / another room's closedRoom / a mixed row / another check
  each refuse). The level is the plain levelgen product of the roofless plan: no hand edit.
  The three a3 ceiling lights now hang with nothing above them (9.95 m). Not fixed here: L's fixture61 housings or a wall mount is the job
  (lights are the C / L tracks'); the lead decides. They are the only lighting change a3's roofless state needs to be flush again.
- Found 3 (new, for the lead): 0e3f95ea5 ("1.3 RIDE ... roof removed", the lead's hotfix) did NOT reach the shipped n52-foundry.
  - It committed the plan and `Design/levels/n52/foundry.json` (the raw levelgen file), not `Design/levels/n52-foundry.json` nor
    `Assets/StreamingAssets/Design/levels/n52-foundry.json`. At t0b both still hold the slab (box min [-12,10,-87] max [12,11,-57]) and have no
    `l1.wallrun` teach entry (evidence/n0b-slabcheck.txt).
  - `SKYGRIND/Sync Port Data` (SurvivalSetup.cs:149-160) copies `Design/levels/*.json` over the StreamingAssets copies before every Player
    build, so a hand-patched StreamingAssets copy is overwritten by the stale Design/ copy.
  - The raw file's hand edit removed the slab from `data.boxes` only; `collision.boxes` (202) and `navGrid` still held it
    (regenerated: 201; navGrid 2,255,055 vs 2,353,484 chars).
  - N0b (below) regenerates the level the sanctioned way.

### N0b: n52-foundry regenerated from its roofless plan (the base the n65 `--check` compares to)
- `dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n52/foundry.plan.json Design/levels/n52/foundry.json --walk`
  (evidence/n0b-n52-levelgen.txt: the same 2 ERROR rows, plan checks 1 problem, bot walk as before), then
  `node Tools/night/gen-n52-foundry.cjs --finish` (evidence/n0b-n52-finish.txt: both copies identical, a9 bit-equal to n48's, 2,376,150 chars).
- Against HEAD's shipped n52-foundry: collision and navGrid regenerated, `data.boxes` 201 -> 200 (the slab), teach35 +l1.wallrun, a3's row roof
  true -> false; nothing else (evidence/n0b-diffs.txt).
- n46flow n52-foundry, seeds 1-10, same build, HEAD's file via `--file` (BEFORE) vs the new file (AFTER)
  (evidence/flow-n52-foundry-before.txt, flow-n52-foundry-after.txt, command in n0b-flow-run.txt):
  | | stall | longest | air | kill gap | nearest | dwell | dead | deaths | clear |
  |---|---|---|---|---|---|---|---|---|---|
  | BEFORE (with the slab) | 30.8 % | 3.2 s | 55.4 % | 2.3 / 25.1 s | 6.9 m | 14.0 s | 18.7 s | 13 | 126.7 s |
  | AFTER (roofless) | 29.4 % | 3.5 s | 56.4 % | 2.2 / 14.6 s | 7.2 m | 14.7 s | 20.0 s | 24 | 127.2 s |
  Both GREEN 10/10. The n52-foundry lines of `n72-gates.sh --flow` therefore move: the lead re-makes Logs/n72/flow-base.txt after the pick
  (`n72-gates.sh --flow-base`) and logs why. The other four levels' files are untouched (not re-run here: not checked).

### N0: the four level files
| level | command (evidence file) | result |
|---|---|---|
| n65-foundry | `node Tools/night/gen-n65-foundry.cjs --run --slots=b1,a8` (n0-foundry-run.txt) | 194 boxes + floor; a3 roof false; no slab (slabcheck); levelgen 2 ERROR rows, both the roofless a3's; both copies sha256 b94337213383ee11... |
| n67-iceline | `node Tools/night/gen-slots.cjs --level iceline --run --slots=g4 --out=Logs/n71/play/iceline4` (n0-iceline-run.txt) | sha256 71310b9b469fc009... = the play build's (Logs/n71/play/iceline4-gen.txt); sha1 348b0b1c76 = spike.md S3 |
| n67-kiln | same with `--level kiln --slots=kb2,kb3,kb3c,kb3d --out=Logs/n71/play/kiln3` (n0-kiln-run.txt) | sha256 6a857b6c07101451... = the log's; sha1 cfc4a7a260 = spike.md S3 |
| n67-spire | same with `--level spire --slots=v6 --out=Logs/n71/play/spire2` (n0-spire-run.txt) | sha256 1ce5a6ede22b1fc3... = the log's; sha1 0390bf6182 = spike.md S3 |
- The n67 levels are GG's played files byte for byte: the `--out` directory names are the play build's, because the level's `generatedBy`
  text carries the plan path; the files were then copied (cp) into `Design/levels/n67-<L>.json` and the StreamingAssets copy (sha1 equal in
  both copies). The n67 plans, assemblies and openings67.json were written by the default-mode plan step
  (`node Tools/night/gen-slots.cjs --level <L> --slots=<list>`, evidence/n0-<L>-plan.txt); each plan is `cmp`-identical to the play build's plan.
  `generatedBy` of those three level files therefore names Logs/n71/play/<dir>/<L>.plan.json (not in the repo; the plan beside it is).
- The slots files (slots65.json, slots67.json x3) are untouched: all old. The rooms are in the level files (list mode), as n71's play build.
- n65-foundry cannot be byte-equal to GG's played file (it had the roof); the rebuilt rooms b1 and a8 are the same files with the same piece
  counts as the play log (b1 boxes 12 -> 14, ramps 2 -> 5, lights 6 -> 8, props 3 -> 8; a8 boxes 6 -> 6, ramps 0 -> 2, pads 0 -> 1, props 0 -> 5).
- Differences of each level from HEAD's all-old file: evidence/n0-level-diffs-vs-head.txt.
- `gen-n65-foundry.cjs --check`: all PASS, 23 lines (evidence/n0-foundry-check3.txt; the first run, n0-foundry-check.txt, had 3 FAIL: the n52 base
  was stale, and the planted a5 case expected 0 ERROR rows).

### N0c: a roofless a3 keeps no ceiling light (the lead's decision, message at 11:16; its own commit, so it reverts alone)
- `gen-n65-foundry.cjs` `assemble()` takes a3-L1, a3-L2, a3-L4 (kind ceiling, y 9.95) out of the n65 plan while a3's row says roof false;
  `--keep-ceiling-lights` keeps them (`--check`'s equal-to-n52 proofs run that way). The allow-list is now closedRoom a3 only: a lights ERROR row
  for any a3 light is refused (planted).
- `--run --slots=b1,a8`: levelgen 1 ERROR row (closedRoom a3), allowed; n65-foundry sha256 1f19ee93f0a69ce1... (evidence/n0c-foundry-run.txt).
  The level differs from the N0 commit's by exactly the three lights (campaign.lights 65 -> 62 in the plan; evidence/n0c-diff.txt).
- `--check`: all PASS, 25 lines (evidence/n0c-foundry-check.txt); two new proofs: the three lights out and every other light n52's bit for bit; a3
  with a roof takes none out.
- n52-foundry (the shipped base, regenerated in N0b) still carried the same three lights over a roofless a3 (the `foundry65:0` side): the lead's later call took them out of the n52 plan too
  ("N0d", 12:40): `n72-n2-a3-lights.cjs` removes a3-L1, a3-L2, a3-L4 from Design/levels/n52/foundry.plan.json in place (idempotent, `lights_why`), then levelgen, `gen-n52-foundry.cjs --finish`,
  `gen-n65-foundry.cjs --export a3` and `--run --slots=b1,a8` (evidence/n2-lights-*.txt). n52-foundry differs from before only in those three lights (campaign.lights 65 -> 62) and a3's `lights_why`;
  n65-foundry only in `lights_why` (it already had them out). gen-n65's drop stays (a room file that carries them still loses them; planted in `--check`, 26 PASS). `n72-reach`: 0 of 9 escapes.

### For the lead (pick)
- Pick in this order: the N0b commit (n52-foundry), the N0 commit (n65 / n67 + the generator), the N0c commit (the a3 ceiling lights out of n65-foundry;
  it rewrites the same n65 files, so pick it after N0 or the three together). Then `n72-gates.sh --flow-base`.
- Not run: verbs37, Player reels, the smoke (the lead's). The LV rows' reels: `-skygrindRows72 foundry65:1` etc. (L-154: the shipped side with `:0`).
- The a3 ceiling lights are out (N0c); the a3 `closedRoom` row stays an allowed ERROR (levelgen's standard wants a roof on every room).

## N5 The movement parity lab and the profile decision (started 11:20)

### The lab
- `Tools/SimHarness/Harness.N72.Lab.cs`, command `n72lab` (build: `dotnet build Tools/SimHarness -c Release`; run: `dotnet Temp/SimHarness/bin/Release/net10.0/
  SimHarness.dll n72lab [--set LABEL:ground.friction=31.07,... ] [--out file]`). The real Mover at the 120 Hz step on a flat floor and one wall, three
  or more profiles side by side: web (PortExport/config.json), now (web + design.ultra71.set of the tree it runs in), and each `--set` label on top of now.
  Rows: jump apex / time to apex / air time, the double jump's apex, dash distance / speed / duration, slide speed (first step, after 1 s), slam speed,
  wall-jump out and up speeds, the stop and the ground accel (50 % and 95 %), the air accel from rest and a 90 degree air turn, the speed kept along the old axis,
  the bunny hop (speed kept per hop, time between hops: the reel's press rule), and the speed an optimal air strafe gains in one flat jump. Each row has its
  ULTRAKILL target x0.5143 (Research/2026-10-26-ultrakill/movement.md) and, per label, the count of rows that moved TOWARD the target against "now".
- Not applied (honest): the sim's design rows (flow34, stamina, design.move, the ride rows, perks) and the surfaces: the lab measures the movement config on
  the bare Mover. Row 12's reel, the ice, and every row that reads friction or accel are verbs37's to prove in a Player. Not checked: the real game's numbers.
- Targets: the ground rows use ULTRAKILL's own 8 ms ticks (3 ticks = 0.024 s to half, 11 ticks = 0.088 s to 95 %; the continuous values 0.019 / 0.083 are in the
  note). The air-strafe target (+3.5 m/s) is my arithmetic (G: the per-axis cap 8.49 x sqrt 2 = 12.0 from walk speed).
- Files: evidence/n5-lab-1.txt (A36 / F31 / F31s first run), n5-lab-2.txt (the five candidates, with "now" = the profile at t0b), n5-lab-3-after-config.txt
  (the lab run on the tree with the new profile line: "now" is the new profile).

### What it printed (n5-lab-2.txt; "now" = the profile at t0b)
| measure | target | web | now | A36 (the slate's: 36 / 36 / air 3.6) | G31 (ground 31.07 / 31.07) | G31a3 (G31 + air 3.63) |
|---|---|---|---|---|---|---|
| jump apex / time to apex / air time | 5.07 m / 0.702 s / 1.404 s | 1.58 / 0.44 / 0.87 | 5.07 / 0.70 / 1.41 | same | same | same |
| double jump apex above the floor | (ULTRAKILL has none; kept) | 2.88 m | 9.18 m | 9.18 | 9.18 | 9.18 |
| dash distance / speed / duration | 5.09 m / 25.46 / 0.200 | 5.80 / 24.0 / 0.242 | 4.88 / 25.46 / 0.192 | same | same | same |
| slide speed | 12.34 m/s | 11.6 | 11.6 | 11.6 | 11.6 | 11.6 |
| slam speed | 51.43 m/s | 48.0 | 51.43 | same | same | same |
| wall jump out / up | 12.34 / 12.34 m/s | 7.9 / 7.3 | 12.23 / 12.18 | 12.09 / 12.18 | 12.23 / 12.18 | 12.08 / 12.18 |
| stop to half / to 5 % of walk speed | 0.024 s / 0.088 s | 0.117 / 0.333 | 0.117 / 0.342 | 0.017 / 0.058 | 0.025 / 0.067 | 0.025 / 0.067 |
| ground accel to 50 % / 95 % | 0.024 s / 0.088 s | 0.067 / 0.142 | 0.067 / 0.142 | 0.017 / 0.075 | 0.025 / 0.092 | 0.025 / 0.092 |
| air accel from rest to 95 % / a 90 degree turn | 0.261 s | 0.642 / 0.642 | 0.642 / 0.642 | 0.267 / 0.267 | 0.642 / 0.642 | 0.267 / 0.267 |
| bunny hop: speed kept (worst of 3) / time between | 100 % / 1.404 s | 100 / 0.867 | 100 / 1.408 | 100 / 1.408 | 100 / 1.408 | 100 / 1.408 |
| air strafe, optimal: speed gained in one jump | +3.5 m/s (G) | +7.1 | +9.9 | +18.6 | +9.9 | +18.7 |
| rows toward the target against "now" | | | | 6 toward, 1 away (the wall-jump out speed by 0.15 m/s), air strafe away | 4 toward, 0 away | 6 toward, 2 away |

### The profile decision (the slate's rule: a change only if its numbers move toward the target; verbs37 is the lead's)
1. **Ground friction and accel: CHANGE to 31.07 each** (not the scout's 36). The Quake model with friction = accel is exactly ULTRAKILL's lerp:
   v' = v x (1 - f dt) + f dt x target; ULTRAKILL's 25 % per 8 ms tick at a 120 Hz step is f dt = 1 - 0.75^(8.333/8) = 0.2589, f = 31.07. 36 is the
   continuous rate (exp(-36 t)) and overshoots at the 120 Hz step: 0.017 s / 0.075 s against ULTRAKILL's 0.024 s / 0.088 s (-31 % / -15 %), where 31.07 reads
   0.025 s / 0.092 s (+4 % / +4 %). The walk stop 0.342 s -> 0.067 s, the 95 % accel 0.142 s -> 0.092 s. No row moves away. The jump, dash, slam and
   the hop's 100 % speed kept are unchanged (the hop's first grounded step skips friction, and the buffered jump leaves it).
   Candidate `F31s` (stopSpeed 0.6 as well) would put the 5 % rows at -5 %; not taken: a third number for 20 ms of tail.
2. **Air accel: NO CHANGE** (stays 1.5). 3.6 puts the air control on ULTRAKILL's (0.642 s -> 0.267 s to the cap) but an optimal strafe-turn then gains 18.7 m/s per
   flat jump against 9.9 now and ULTRAKILL's 3.5: two rows away (the air-strafe gain +182 % -> +435 %). The Mover caps along the wish direction, ULTRAKILL per axis:
   a faithful air model is a Mover change, not tonight's (Mover.cs). 2.5 (`G31a2`): 0.383 s, +13.2 m/s: a compromise, not taken. GG's call, in DECIDE72.
3. **Not changed, listed**: slide.floorSpeed (11.6 vs 12.34, -6 %): applying it turned verbs37 row 13 RED on 4 of 5 levels (config _why, n71-polish3); the
   dash distance 4.88 m (-4 %; the lab counts 23 steps in the Dash state at 25.46 m/s; why it is not 24 is not checked) is left as it is.
4. **Ice and mud**: `design.n26.surfaces` iceFrictionMult 0.25 and mudFrictionMult 2 scale ground friction, so with 31.07 ice grips 5.2x harder (7.8 against
   1.5 /s; today's ground is 6). `design.n30.glide` (ON) swaps the Mover's friction for its own decay (0.12 /s) on glide ice, so the gliding rooms keep
   their slide; plain ice outside the glide row would change. To keep ice's absolute friction at 1.5, iceFrictionMult would be 0.0483 (0.25 x 6 / 31.07),
   mud 0.386 (2 x 6 / 31.07 = 0.386): those are the lead's config numbers (not mine to edit); iceline's verbs37 and the glide proof are where it shows.
   Measured (13:47, evidence/n5-lab-4-surfaces.txt; the lab's walk stop from a steady walk, `Mover.groundFrictionMult` 1 / 0.25 / 2; no glide row in the lab): plain ground 1.146 m before N5b (the web's 1.069), 0.173 m
   with it; ice (x0.25) 4.766 m before, 0.872 m with it (5.0x the ground's, the web's rule "about 4x"; the web's ice 4.444 m); mud (x2) 0.437 m before, 0.023 m with it. Ice's absolute stop distance comes
   back (4.766 m) with `iceFrictionMult` 0.0483 (= 0.25 x 6 / 31.07) and mud's nearly (0.543 m against 0.437) with `mudFrictionMult` 0.386: both are config numbers of design.n26.surfaces (the lead's), not
   changed by me. (Those two rows' web / Gold columns in the file are not meaningful: the multipliers are written for friction 31.07.) With `design.n30.glide` ON the glide ice keeps its own decay whatever
   the friction is (Glide30.cs:105), so the gliding rooms are unaffected; plain ice outside the glide row is what the lab's number describes.
5. **Fingerprints**: the profile changes the Player's movement, so FingerprintBaseline.Ultra71Hashes needs the lead's two equal `-skygrindFingerprint` runs
   after the pick (NIGHT72.md section 5 step 7); I update it then. The golden vectors and the 16 replays read PortExport/config.json, untouched.

### Commits
- N5a: the lab and its evidence (no config change). N5b: the two config lines (`design.ultra71.set` + ground.friction / ground.accel, both copies byte-identical,
  `_why` extended): a candidate the lead picks at the train that runs verbs37 with it. Off: drop the commit, or remove the two keys; `-skygrindNoUltra71` turns the
  whole profile off.
- Gates with N5b: `n72-gates.sh --no-editor` GREEN (evidence/n5b-gates.txt: config copies identical, golden 0 mismatches, 22 exact replays, 12 fingerprints EQUAL;
  the harness does not apply ultra71, so the Player's fingerprint is the lead's).

### N5c: ice keeps its stop (the lead's decision, 13:50; Rule 1: nothing the player had is lost)
- The lead's call: `design.n26.surfaces` iceFrictionMult and mudFrictionMult are set so the walk stop on each surface under friction / accel 31.07 is what it was before N5b: ice 4.766 m, mud 0.437 m.
  The Ultra71 line stays 31.07. Glide30 is untouched.
- Solved by bisection on `Mover.groundFrictionMult` (the lab's own walk stop, steady walk 1 s then no input, until the speed is under 0.05 m/s): `n72lab --solve ice=4.766,mud=0.437` (evidence/n5-surfaces-solve.txt).
  Ice: exact 0.048281 gives 4.766 m. Four places, 0.0483, give 4.764 m (0.002 m short); three places, 0.048, give 4.794 m. Mud: exact 0.468508 gives 0.437 m; four places, 0.4685, give 0.437 m (printed
  precision, 0.469 gives 0.436 m). Committed: **iceFrictionMult 0.048281, mudFrictionMult 0.4685** (both config copies, byte-identical, `_why` extended with the arithmetic).
- Checked from the committed file, not the solver: `n72lab --committed` reads this tree's `design.n26.surfaces` and prints ice x0.048281: stop 4.766 m in 1.47 s, steady 8.49 m/s; mud x0.4685: stop
  0.437 m in 0.15 s, steady 8.49 m/s; plain ground 0.173 m (evidence/n5-surfaces-committed.txt).
- Two honest differences from the web, not a stop distance: (1) ice: the web's ice (x0.25 on friction 6, accel 10) stopped in 4.444 m and walked at 8.00 m/s, so ice keeps its stop from before N5b
  (4.766 m, the Ultra71 steady walk 8.49), not the web's 4.444; (2) mud: the web's mud (x2 on friction 6 against accel 10) capped the walk at 6.67 m/s and stopped in 0.406 m; under friction = accel
  nothing caps the walk, so mud no longer slows walking (8.49 m/s) whatever the multiplier is: a stop and a walk speed cannot both be matched with one multiplier. No level has a mud rect (only
  iceline's g4 canal has an ice rect among the level files; Design/maps/glacier-course.json says its whole floor is ice, and whether that map is in the build's campaign is not checked).
- Glide30 (ON) swaps the Mover's friction for its own decay, so a gliding room keeps its slide: the ice multiplier does not enter that decay (Glide30.cs:105 recomputes the Mover's friction with
  the same multiplier and removes it). NOT checked, and not changed: Glide30 keeps the INPUT's acceleration `a` (v = v0 x (1 - decay dt) + a), and N5b makes the ground accel 31.07 (was 10), so a held
  backward or sideways input on glide ice brakes and steers harder than before N5b. No lab row measures that (the glide30 harness does not apply ultra71).
- Gates: `n72-gates.sh --no-editor` GREEN (evidence/n5-surfaces-gates.txt: config copies identical, golden 0 mismatches, 22 exact replays, 12 fingerprints EQUAL, 12 --off runs EQUAL; the SimHarness
  run does not use ice). dn-check-wt (fresh dir): 0 errors on the four projects (evidence/n5-surfaces-dn-check.txt). Tests that name the multipliers (Rink28ArenaTests) build their own
  SurfaceRules, so they do not read the config; Rink28Check / Harness.N28 compare against `surf.rules.iceMult` (equality, no threshold).
- Not checked in a Player: whether the Player's -skygrindFingerprint moves with these two numbers (the SimHarness run, which does not apply ultra71, is EQUAL; the lead's Player runs decide).
  The iceline slide itself in a Player (verbs37 rows 40-42, the g4 canal): the lead's t3 sides.

## N1 verbs37 row 12 (the bunny hop) under the ULTRAKILL profile (started 11:36)

### What was measured (all on the bare Mover over each level's collision world, indicative only: no design rows)
- The slate's reading ("the window grows") is half of it. `Tools/SimHarness/Harness.N72.Lane.cs` (`n72lane`) runs the row's press logic (HopSite72.Dry, the reel's code
  copied) at Open35's pick: with the web's movement 3 hops in 1.3-1.9 s on iceline, kiln, span, spire; with the ULTRAKILL profile the old 4.0 s window gives 1-2 hops
  on every level and 3 hops need 4.4 s on kiln and spire (n1-lanes.txt). So 4.0 s cannot hold three 1.4 s hops: the window is a cause.
- The dry run is NOT the Player: it reads foundry and span RED where the Player (polish4 / polish8) passes them (foundry web 2 hops RED), so it predicts nothing
  about which level goes green. What the Player reads is the lead's. Open35's lanes (rays to 30 m, first room with 14 m): iceline g6 16 m, kiln kb2b 24 m,
  span p1 14 m, foundry a2 16 m, spire v1 24 m; three hops are about 26 m, but a deck or a low box under the 5 m arc is no obstacle, so lane length alone does not
  predict a RED either (span passes on 14 m).
- A pogo is not a hop: the dry run "passes" a few places in 0.7-2 s (hops under a low ceiling, g8 0.7 s), which three real hops cannot do (flat air 1.4 s).

### What changed (the bar is NOT changed: 3 hops, each keeping >= 99 % of the speed)
- `Assets/Scripts/Game/HopSite72.cs` (Unity-free; + .meta; also in SimHarness.csproj): `Window(mc)` = max(4.0, 0.3 + 3 x (air time + 0.5)) s: 6.0 s with the ULTRAKILL
  profile, 4.0 s with the web's (unchanged); `Candidates(...)`: other places, longest clear lane first (rays and the floor check to 60 m), at most 4; `Dry`, `RoomLane`.
- `Assets/Scripts/Game/UnityTuning.N72N.cs` (+ .meta): the row's constants and the new ones, each with a comment.
- `Assets/Scripts/Game/FeelReel.N35.Verbs.cs` row 12 only: Do35's window is `HopSite72.Window`; the first attempt is Open35's place (today's behaviour with the longer
  window); if it misses the bar, up to 4 more places are tried in turn, a retry counting only with real hops (each gap between two hops at least half the flat air time:
  a low ceiling is not a hop); the line keeps its old text and adds `; window 6.0 s at room <id> ...; attempts: room g6 (Open35): 2 hops | room g8 lane 20 m: 3 hops`.
- `dn-check-wt.sh`: 0 errors on the four projects (evidence/n1-dncheck.txt). Not checked: a Player run (the lead's).

### N1 follow-up (13:44, after the t2 verbs37: iceline 26/29 and kiln 27/30 with rows 40, 41, 42 RED, "nearest - m")
- A suspect in my own change, not confirmed (no Player): row 12's retry places are other rooms' spawns (HopSite72.Candidates), and a `Respawn` into another room starts that room's fight and moves the campaign on
  (the cue reel's room-2 banner relies on exactly that). Iceline and kiln are the two levels whose first row-12 run misses the bar, so they are the two that ran the retries before rows 13-47; rows 40-42 then
  looked for an enemy to parry, launch and heal from in a campaign that had moved. The t2 build's log line shows it: `12 bunny hop ... attempts: room g6 (Open35): 2 hops | room g8 lane 20 m: ...` (a second
  room in the attempts) would confirm it; one attempt only would rule it out.
- The fix (commit "N1b"): Open35's place runs where it always did (its PASS is final); when it misses, the row is HELD, rows 13-47 run, and the retries run after the last row (`HopAttempt` as a local
  iterator, `HopLine()`, the row's one result before `Stop()`), so no retry can touch another row. The line is the same text, printed late; the totals count the row once. dn-check-wt 0 errors in a fresh scratch dir
  (two earlier runs read hundreds of CS0246 errors in a dirty scratch dir: `rm: cannot remove .../dn/Assets/Scripts/Game/Bot: Directory not empty`; evidence/n1b-dncheck-run*.txt).
- Whether N5b (ground friction / accel 31.07) is behind rows 40-42 is the lead's OFF-side check; nothing in the lab reads an enemy.

### For the lead (N1)
- The reel: `bash Tools/night/n72-reels.sh <tag> vi,n52-iceline,verbs37 vk,n57-kiln,verbs37` (and vf, vp, vs, and the n67 levels with `-skygrindRows72 iceline67:1`...). Read
  `[reel] verbs37: 12 bunny hop`: PASS / RED, "N hops, speed kept per hop ...", the window, and the attempts. With `-skygrindNoUltra71` the row reads as before (4.0 s window).
- If a level is still RED after the attempts, the line says where each attempt stood; the next fix would be a level-specific lane, not a window.

## N2 The reach audit (started 12:00)

### The meter
- `node Tools/night/n72-reach.cjs [--level ...] [--selftest]`, evidence/n2-reach-1.txt (before) and n2-reach-2.txt (after). Reads Design/levels/<id>.json. Reach numbers from the
  parity lab (n5-lab-2.txt): jump 5.07 m, jump + double jump 9.18 m above what the feet stood on (the lab's "double jump apex above the floor" 9.176; the slate's
  "5.07 + 4.1" is the same). ESCAPE: a roofless room whose ring top T (its `height`) is at most the floor's double jump reach, or whose decks within 2 m of a ring wall
  have top + 9.18 >= T. BONK: a roofed room whose ceiling C is under base + jump + 1.8 m (head) / base + double + 1.8 m, for the room (base 0) and for each ride lane (base = the wall's min y).
- Planted: `--selftest` (a roofless room with a 6 m ring, a 10 m ring with a 2.5 m deck at its wall, the same deck under a 16 m ring, a roofed 7 m room with a ride lane): 6 PASS.
- Not checked: wall-jump chimneys (a wall jump off a different wall refills the count), climbing a ring by wall rides, grapples, jump pads, and what lies beyond a ring.

### What it found (before the fix: n2-reach-1.txt)
- ESCAPE only in level 1's a3 (1.3 RIDE), in both n52-foundry and n65-foundry: six ledges whose reach clears the 10 m ring, five touching it (the 2.5 m shelf and
  steps along the west and north walls: 11.68 m; the 5 m east platform: 14.18 m). Every other level: 0. The closed rooms have a roof slab on their ring; the other roofless
  rooms are boss arenas with tall rings (spire v11, foundry a9).
- BONK (listed, not fixed: the slate asks the fix only for escapes): the single jump (5.07 + 1.8 head) meets the ceiling at the floor in foundry a1, a4, a6 (heights 6, 6, 5),
  iceline g6, gr, kiln kb4, spire v8; the double jump (9.18 + 1.8) meets it in 6 of foundry's 10 rooms (a1 a2 a4 a5 a6 a8), 7 of iceline's, 1 of kiln's (kb4), 7 of spire's
  (v1 v3 v4 v7 v8 v9 v10), 0 of span's. Ride lanes that bonk on the double jump: foundry a5 (1), a8 (3, n65), iceline g1 2, g2 1, g3 1, g4 1 (n67), g7 2, spire v6 (n67) 1.
  With the profile the double jump is mostly dead under a 7-10 m roof: a design call for GG (raise the rooms' heights, a plan regeneration of every level), not made here.

### The fix (the escapes)
- `Tools/night/n72-n2-a3-ring.cjs`: a3's `height` 10 -> 15 in the n52 plan (in place, idempotent, asserts a3 is the roofless 1.3 RIDE; `height_why`): the highest deck's reach is 14.18 m.
  Then levelgen of the n52 plan and `gen-n52-foundry.cjs --finish` (n52-foundry, both copies), `gen-n65-foundry.cjs --export a3` and `--run --slots=b1,a8` (n65-foundry, both copies;
  n65 sha256 f35b1ed3e408fc1d...), the plan, assembly, validate reports and the rooms-n52 export with them (evidence/n2-regen-n52.txt, n2-foundry-run.txt).
- `gen-n65-foundry.cjs --check`: all PASS, 25 lines (n2-foundry-check.txt). `n72-reach.cjs` after: 0 of 9 levels with an ESCAPE (n2-reach-2.txt); the n52 and n65 files
  differ from before only in a3's ring walls (8 boxes), collision, navGrid and the row's height (n2-diffs.txt).
- n46flow n52-foundry seeds 1-10 after the ring (flow-n52-foundry-after-n2.txt): GREEN 10/10, clear 139.3 s (127.2 s after N0b), a3 30.6 -> 33.6 s; the unchanged rooms a5 and
  b1 moved too (26.4 -> 29.1 s, 35.2 -> 41.8 s): the bot's runs shift with any change, so read the level number as noise-sized (not checked: a repeat). The lead re-makes the flow base at the pick.
- What I did not see: the room's view. A roofless room still has a drawn ceiling at its `height` (RoofView27 / LightPlan33 roomCeil), now 15 m, with its strips: tf a3 frames
  (tour65 n52-foundry-a3 / n65-foundry-a3) are the check.

## N3 goal72: the room's one line at the thing it names (started 12:10; row design.n72.goal72, OFF until the lead's pair)

### What it is (view only: no sim read beyond the sim's public state, no tape bit, no audio)
- A cleared campaign room with its exit open: ONE line hangs over the exit (the verb in capitals, painted in the player's cyan; at most 6 words; UiTheme S24 = 24 px at 1080p, the
  input's own glyph through the Ui's resolver) with an arrowhead marker under it. An exit off screen: the marker on the screen's edge (never the bottom: the gun and the HP plate), the
  line beside it. Texts by where the exit is (Goal72Math.Format): up (+2.5 m): "GO UP to the exit"; down: "DROP DOWN to the exit"; level: "GO to the exit".
- STUCK hints that retire: no progress (the feet 2.5 m closer than at the last progress) for 10 s: the line says where ("The exit is UP THERE" / "BELOW you" / "TURN AROUND: the exit is
  behind" / "AHEAD") and the marker breathes; 20 s: it says how ("DOUBLE JUMP: press {jump} twice"; past a double jump (7.5 m up) "RIDE a wall: hold {jump}"; down "WALK OFF the ledge and
  drop"; level "FOLLOW the arrow to the exit") and a pointer rides a ring (170 px) round the crosshair toward the exit. Progress retires a hint (counted, never back in the room); a verb the
  player already used this run (a ride, a slide, a double jump spent: read from the mover's state) is not taught again; each stage counts once a room.
- One line at a time: only in the cleared phase (never in a level's first 5 s of campaign time, never in the fight); the DOOR OPEN cue (its distance and side words are this line's job) is held at
  alpha 0 while the line is up (UiCampaignCue24's "Door cue 24" group, written after it each frame by OnRender; off: the cue as before). A scripted player (a reel's forced input) never gets a hint
  (its clock is held), so a verbs37 run shows none; the planted run lifts it (REEL ONLY).
- Files (all new, +.meta): Goal72Math.cs (pure: the state machine and texts; in SimHarness.csproj), Goal72View.cs (self-hooked, as Feel46View: its own overlay canvas under the HUD's), Ui.Goal72.cs and
  SurvivalGame.Goal72.cs (the glyph resolver made reachable), FeelReel.N72.N.cs (stuck72), UnityTuning.N72N.cs (every number with a comment), Tools/SimHarness/Harness.N72.Goal.cs (`n72goal`).
  No shared file edited. Read every frame with NightRows.ViewOn(cfg, "goal72"); the state machine runs while the row reads off (D1).

- hud33's own door hint (the message slot's "DOOR OPEN" and the chevron + "N m" over the door, UiHud33.Door33) is what the game draws today (hud33 ON); with goal72's line up it gives way:
  ONE shared-file line in UiHud33.cs Door33 (`if (Goal72Probe.Visible) open = false;`; its own commit "N3c", so it reverts alone and the lead can re-resolve it if another builder touched
  UiHud33.cs). Row off: Goal72Probe.Visible is false, Door33 is exactly as it was. (UiCampaignCue24's cue, which hud33 replaces, is still held at alpha 0 by the view.)
- The line slides in (36 px, a cubic ease-out over 0.32 s, again when the line changes; none with REDUCED MOTION): the leftover "the room line slides in" (GG 2026-10-14 "UI needs to be
  animated"). The counter already punches on a kill (counter43, ON: the number punches and flashes white, the pip goes), so nothing was added there.

### What was run (no Player)
- `n72goal` (evidence/n3-goal-math.txt): 7 planted cases PASS: a steady approach shows the line and no hint; a stuck player gets stage 2 at 10.0 s and stage 3 at 20.0 s, each counted once; a scripted
  idle player gets stage 1 and no hint, and with the planted switch 2 at 10.0 s and 3 at 20.0 s; progress retires a hint (retired 1, stage 1, no return in 30 s); a verb used is not taught again; every
  text of every stage / direction / learned-verb combination is at most 6 words (13 lines); a new room restarts the clock and the hide radius shows nothing.
- dn-check-wt: 0 errors on the four projects (evidence/n3-dncheck.txt). Not checked: the drawing (the marker's edge placement, the line's plate width, the cue hiding, the arrow sprite), the
  Player log's `[goal72]` lines, the reel's numbers.

### For the lead: the Player steps
- The proof (L-014 / L-115, the input prints itself): `bash Tools/night/n72-reels.sh <tag> sf,n52-foundry,stuck72,-skygrindRows72,goal72:1` (the ON side) and the same without the row list (OFF).
  Read `[reel] stuck72` / the phase's line: `GREEN stuck72 | ... | scripted idle 22 s: max stage 1, hints 0 | planted: stage 2 at 10.0 s '...' (5 words, 24 px), stage 3 at 20.0 s '...' | progress: retired 1,
  stage 1`; with the row OFF it reads RED "goal72 is OFF here". Frames: `shot-N72-stage1-line.png`, `stage2-where`, `stage3-how` in the side's folder (the SPOT inputs; the same file names on both sides).
- Also in the Player log: `[goal72] stage 1 room a1 line 'GO UP to the exit' (5 words, 24 px) ...`.
- A normal verbs37 run: no `[goal72] stage 2 / 3` line (a scripted player). The row ON must not move the fingerprint (view only).

### N3b: why the stuck72 reel was RED on both sides at n72-t2, and the fix (14:20)
- The lead's reading: "the hint trigger does not run in the scripted idle". The log (Logs/player/n47-reel-n72-t2-sfon.log) says otherwise: `max stage 0` came with `room 1 was not cleared in 90 s`, `goal72 on True, visible False, stage 0,
  lines 0`. The trigger (Goal72Math.State, driven by Goal72View) never got a frame to run on: the view draws only for a CLEARED room of a HUMAN's run (`playing = c != null && flow Playing && !attractOn && !sim.metaSilent`,
  `cleared = phase Cleared`), and the reel broke both:
  1. n52-foundry's room 1 ('a1') is a taught LINK (`kind link`, 3 lesson steps, no waves): it clears when its lessons are done. The reel waited for it with the cue reel's loop (KillAll; nothing to kill) and the
     N24StartCampaign bot, which does not pass the lessons: never Cleared.
  2. N24StartCampaign attaches the AUTOPILOT and AutopilotInput sets `sim.metaSilent = true` until Detach(): even a cleared room would have drawn nothing (Goal72View's gate).
- Fix (Assets/Scripts/Game/FeelReel.N72.N.cs only; the shipped view, the state machine and the numbers are untouched): the reel waits for room 1 to be entered, marks the lessons done the way Campaign24's own lessonCap does
  (`c.lesson.index = c.lesson.defs.Count`; the campaign then clears the room itself on its next step; a fight room is killed out as before, 30 s cap), detaches the bot and returns `g.sim.UseInput(g.playerInput)` (a human's run,
  scripted by `g.forcedMove`), then runs A / B / C as before. C's progress spot: a node of the room, else a point 9 / 12 / 16 m from the exit on the line to the spawn (a link has no nodes: the t2 line said "closer node none"). The result
  line now prints `room 1 'a1' (link) cleared by the campaign ..., lessons forced done ..., human run ..., metaSilent ..., flow ..., attract ...` and the spots tried, so a second miss names its gate.
- Proof on the sim side, headless (`SimHarness n72goal --campaign n52-foundry`, evidence/n3-stuck72-campaign.txt, 5 PASS): an idle player stays in Fight on a1 for 28 s (the t2 reel's 90 s wait); the lessons marked done clear a1 in 10 steps; Goal72View's
  gate copied onto the sim's own feet / exit / phase gives, for a scripted idle with the planted switch off, stage 1 only (lines 1, hints 0 / 0), and planted, stage 2 at 10.0 s and stage 3 at 20.0 s (hints 1 / 1); AutopilotInput sets
  metaSilent False -> True and Detach() puts it back. `n72goal` (the pure cases) still all PASS (evidence/n3-stuck72-goal-pure.txt). dn-check-wt (fresh dir): 0 errors on the four projects (evidence/n3-stuck72-dn-check.txt).
- NOT checked: the reel in a Player (the canvas, the text, the three shots, `Goal72Probe` through the real view; whether flow is Playing and attract off at that point; progress spot C on the foundry's floor). The sfon / sfoff re-run is the lead's;
  on sfon the line to read is `[reel] stuck72: GREEN stuck72 | ... human run True ... scripted idle 22 s: max stage 1, hints 0, line 'GO to the exit' | planted: stage 2 at ~10 s ..., stage 3 at ~20 s ...`; sfoff stays RED by design (row off).

## N4 cam72: the ULTRAKILL camera (started 12:24; row design.n72.cam72, OFF until the lead's pair)

### What it is (view only; Cam72Math.cs pure, applied in CameraFeel.cs; numbers UnityTuning.N72N.cs Cam72*)
- STRAFE ROLL: target = the strafe input x 1 deg, x5 while dashing or sliding; toward the target at 25 deg/s x (|error| + 0.01) (100 deg/s boosted), back to 0 by a 0.5 s smooth damp. It replaces the web's
  velocity-based strafe lean (NIGHT18's Quake roll) and the slide's fixed left tilt (and its extra); the wall ride's lean, its pre-tilt and the shakes stay. The input is the step's own move frame in a campaign
  run (sim.moveFrame30), else the lateral speed over the walk speed.
- DASH / SLIDE FOV: a dash or slide forward narrows the FOV x0.95, backward widens it x1.1, sideways nothing (the direction's dot with the look); it comes and goes at 300 deg/s. With the row on the
  web's dash kick (+11.8 deg) and the slide's widen are gone (ULTRAKILL has neither); the speed ramp (within its cap), the gust, the wall ride's lift and the pinch stay.
- SLIDE EYE DROP: 0.32 m (0.625 u x0.5143) eased at (distance + 0.5) x 10 m/s, with the landing dip kept and the slide's own dip dropped. The eye is clamped under a ceiling over the slide (a ray up from
  0.9 m over the feet, 0.12 m clear): the slide capsule is 1.0 m tall (slide.colliderHeight), so a 1.28 m eye would look through the roof of a slide gap; with a 4 m roof the eye is 1.28 m, under a 1.2 m roof 1.08.
  So the web's 0.7 m drop is needed under a low gap and 0.32 m is what an open room gets.
- REDUCED MOTION: no roll, no FOV change, no drop. The row is read every frame (NightRows.ViewOn, D1); the three states step either way. Off: CameraFeel's old expressions exactly (position, FOV, roll, slide roll).
- cam72 supersedes eye63 and fov55 (not harvested: they stay OFF). The ULTRAKILL source is one mod author's port of the game's camera ('C' in scout-movement.md): the numbers are not verified by us.

### What was run (no Player)
- `n72cam` (evidence/n4-cam-math.txt): 4 planted cases PASS: strafe peak 1.000 deg at 0.12 s and back under 0.05 deg 1.18 s after the release, no undershoot; dashing / sliding left -5.000 deg within a frame
  (100 deg/s x (error + 0.01) is 500 deg/s at a 5 deg error: a dash's strafe snaps, as the port's does); the FOV at base 105: forward 99.75, back 115.50, sideways 105, two frames each way at 60 fps; the drop
  0.32 m in 3 frames and back in 3, the eye 1.28 m open, 1.28 m under a 4 m roof, 1.08 m under a 1.2 m roof.
- dn-check-wt: 0 errors on the four projects (evidence/n4-dncheck.txt). Not checked: any frame of it in a Player (the roll's look, the slide drop against a real gap, the FOV).

### For the lead: the Player steps (L-014: the lines print their input; ON and OFF read differently)
- Any verbs37 side, ON `-skygrindRows72 cam72:1`, OFF plain: the Player log prints once per process `[cam72] dash forward: fov 99.8 against base 105.0 (x0.950; ULTRAKILL x0.950), row ON` (OFF: the web's kick,
  fov above the base), `[cam72] slide: eye 1.28 m over the feet (standing 1.60, drop 0.32 ...), roll -5.0 / +5.0 deg ..., row ON` (OFF: drop about 0.7, the fixed left roll), `[cam72] strafe: roll 1.00 deg ...`
  (only a run that strafes: the walk rows do not). The frames: verbs37's slide row (the eye drop and the roll) and the dash row (the FOV) on the ON side against the OFF side.
- Fingerprints EQUAL (view only; the SimHarness gates do not run CameraFeel).

## N6 and the leftovers: state at 12:40
- N6 iceline label: no code. The shipped-map side of the iceline reel runs with `-skygrindRows72 iceline67:0` (L-154; DECIDE71 Q-3's trace: the reel's old-map gate swaps the old map out of the campaign
  order and counts the grapple as owned). A reel fix is optional and only with the row off reading 29/29: not made.
- N6 exit63: waits for the H pair (HV-exit63 hands it to N only if the pair shows a defect). What is known from NIGHT63 (Research/2026-10-22-wave63/V-EXIT.md, written before any Player run): its doneWhen "own
  gain >= 1.5x in 4 of 5 rooms" was predicted SHORT on the clear poses whatever the doorway draws (kb3, kb2c, kb3c have ramps and ledges in front of the door; kb2 and kb3d already read lit by Doors38's curtain: an
  upper bound of 1.49x / 1.31x at veil gain 1.0), every part's gain is <= 1 by design (the GUARD: bloom past the door's box lit the gun, L-142), and the shot that tests the drawing itself is the exit63 shot at
  kb3d (10 m in front of the door), not the clear poses. A rework would need a gain above 1 (an emissive door face) and the GUARD's meter to say how far the bloom reaches: not started, no evidence to size it.
- select41 (HV-select41): waits for its H pair the same way.
- The UI's motion: goal72's line slides in; the counter's kill punch exists (counter43). Nothing else added.
- FingerprintBaseline.Ultra71Hashes: waits for the lead's two equal t2 -skygrindFingerprint runs; `node Tools/night/n72-fp-update.cjs <fp-1> <fp-2>` (self-tested, evidence/n5-fp-update-selftest.txt) writes it
  and refuses a differing or short pair.

## For PLAY72.md (one line each; where to see it; the off switch)
- Movement (N5b, ground friction / accel 31.07): walking stops and starts in about 0.1 s (ULTRAKILL's lerp); any room. Off: `-skygrindNoUltra71` (the whole profile) or drop the two keys.
- Row 12 / the hop reel (N1) and the levels (N0, N2): not player-facing except level 1's 1.3 RIDE (no roof, a 15 m ring) and the n67 / n65 rooms GG played (the level rows' own off switches).
- goal72 (N3): clear a room: one line at the exit ("GO UP to the exit", the verb in cyan), a marker on it, a breathing hint after 10 s still and a how-to after 20 s; it replaces the DOOR OPEN hint. Off: `-skygrindNoGoal72`.
- cam72 (N4): strafe and the camera leans 1 degree (5 when dashing or sliding), a forward dash narrows the view, a slide drops the eye 0.32 m. Off: `-skygrindNoCam72`.
