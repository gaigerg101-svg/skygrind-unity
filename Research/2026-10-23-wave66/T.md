# NIGHT66 builder T (instruments): notes

Tree `.claude/worktrees/agent-af48e888c59fa3626`, branch `n66-T`, from the t0 sha e37083cf. Branch C (sim only): no Editor, no Player, no
build. Every output named here is a file under `Research/2026-10-23-wave66/T/` with its command, sha and time at its top (written by
`T/save.sh`, a 15-line helper that runs a command from the tree root and saves its output).

## T1a (code 1b1f98ad): tour65's GREEN rule (S3) and the safe `--run`

**What changed**
- `Assets/Scripts/Game/Tour65.cs`: `Tour65.Green(rooms, unframed, warns, wrongRoom, noWave, screen, ended)` = rooms > 0, unframed 0, WARN 0,
  wrong room 0, no wave 0, 1920x1080, not ended. The old term `framed == poses` is dropped: both counters counted the same poses, so it was
  always true. `Tour65.Solids` reads `collision.wedges` too (a 7-number entry per wedge; `InsideSolid` tests a point under the ramp's sloped
  top in the ramp's own frame, base y 0, as ArenaMap.WedgeCorners builds it). `Tour65.Toured` (which rooms the tour frames) and
  `Tour65.Dry` (the level-file half of the tour: rooms, poses, unframed, WARNs, by the reel's own rules) are shared by the reel and the
  harness.
- `Assets/Scripts/Game/FeelReel.N65.Tour.cs`: the reel's `green` is `Tour65.Green(...)`; its room filter is `Tour65.Toured`.
- `Assets/Tests/EditMode/Game/N65TourTests.cs`: two new tests (no old test edited): an unframed fight room reads RED, then GREEN once it has
  a pose; a pose inside a slide wedge is a WARN (RED), then GREEN standing on the ramp; plus `Tour65.Green`'s single-term REDs.
- `Tools/SimHarness/Harness.N65.Tour.cs` (`n65tour`, the headless twin): its verdict is now `Tour65.Dry(...).Green`, it prints the planted
  cases (RED once, GREEN once fixed, the old rule's reading beside each), and `--level` takes a file path too.
- `Tools/night/gen-n65-foundry.cjs`: `--run` refuses on a levelgen ERROR (or a failed levelgen) **before** `finish()`, so the shipped files
  are never written from a level with errors; the sha256 of both shipped copies printed before and after. `--plant-error` (the proof only)
  lifts a2's spawn 40 m off the nav grid in the assembled plan.

**doneWhen 1 (tour65's GREEN):**
- `n65tour --level n65-foundry` -> `T/t1a-tour-n65-foundry.txt`: the planted cases `unframed p2 -> RED (the old rule: GREEN); fixed -> GREEN`
  and `wedge: solids 2 (1 box, 1 wedge), WARN 1 -> RED (the old rule, boxes only: GREEN); fixed (feet on the ramp at y 2.2) -> GREEN`,
  `cases (T1a): PASS`; the shipped all-old n65-foundry: 6 rooms, 6 poses (all frames36.mid), WARN 0, unframed none: GREEN.
- `n65tour --level n52-foundry` -> `T/t1a-tour-n52-foundry.txt`: the same cases PASS; 6 rooms, WARN 0: GREEN (rc0's level).
- The EditMode tests compile (dn-check below); they are run by the lead in the Editor: **not run by me**.

**doneWhen 2 (the safe --run):** `node Tools/night/gen-n65-foundry.cjs --slots=latest --run --plant-error` -> `T/t1a-gen-plant-error.txt`:
levelgen's report `spots on the nav grid | ERROR | no grid node under: a2 spawn` and `a1 exit to a2 spawn has NO route` (2 ERROR), `REFUSED,
the shipped files NOT written`, both copies sha256 `5f3eba53c34b84b6a3a4548281865c6cc5691539745636b9ed88215ab296cd85` before and after
(`byte-identical`), exit 1. The unplanted control
`--slots=latest --run` -> `T/t1a-gen-latest-run.txt`: 0 ERROR, the shipped copies 5f3eba53 -> 62559c9d (written, as before). After both runs
I restored my tree's seven generated files with `git checkout --` (their diff was the runs' own output: the plan, foundry.json, the
assembly, openings65.json, the report, the two shipped copies).

**A finding for the lead (not a T1a item):** the new rule reads the LATEST room files (NIGHT65's final rooms at the t0 sha) **RED** with 4
WARNs (`T/t1a-tour-latest.txt`): a2 line65 7/8 at (2.6, 0, -48.4) feet + eye; a4 line65 6/8 at (-2.5, 2.1, -106.7) feet; a5 line65 4/8 at
(-2.0, 0, -127.3) eye; a8 frames65 `exit` at (0, 0, -360.5) feet. The old rule printed these WARNs and still read GREEN. A Player tour65 of
a round's level will read RED until R / S move those points (their room files' line65 / frames65).

**Checks:** `bash Tools/night/dn-check-wt.sh <scratch>` -> `T/t1a-dn-check.txt`: Game 0, Game.Tests 0, Core.Tests 0, Editor 0 errors
(the scratch folder deleted after). `bash Tools/night/n66-gates.sh --no-editor` -> `T/t1a-gates.txt`: config identical, golden 0 failing /
0 mismatches, fingerprint plain and --off57..--off66 EQUAL, `gates: all GREEN 22:37:37`.

**Not checked:** the Editor compile and the EditMode run (the lead's pick); tour65 in a Player (branch C); whether b0 is built (branch C:
no).

## T1b (code 115008a0, 59ae6fed, 7b2139b2): skip v2

**What it runs** (`n65verb skip`, Harness.N65.Verb.cs `V66SkipV2`; NIGHT65's skip is `n65verb skip --v1`). Per seed (10 by default), three
runs of the room from its spawn under the same walk pilot (Walk38Pilot + n46flow's lava rule + the ported autopilot, not invulnerable):
1. **verb-aware**: the verb-aware layer (V65VerbPilot), the verb allowed.
2. **refused-in**: the SAME layer in its refused mode (V65VerbPilot.refuse, Harness.N65.VerbPilot.cs): the same triggers start the same
   episodes, each episode declines the verb (one press refused, counted) and releases what only serves it, so the run plays on:
   - launch: the look stays on the episode's husk (the gun's aim); the inner pilot's move and fire run: no dash, no slide, no punch, and
     the fire is no longer held off (the allowed episode sets `c.fire = default`, VerbPilot.cs:330 at t0);
   - pierce: the look stays on the line's body for at most 1.0 s or its death; the inner pilot's move and fire run: no hold to charge, no
     switch to the KESTREL, no forced release (redteam.md finding 3's "stands there and does not fire" is this hold, now released);
   - ride, parry, slide: nothing of the episode is played (every input of those episodes serves the verb only): counted, then the 1 s rest.
   V65Filter round it drops the inner pilot's own verb presses (v1's refusal).
3. **walker**: the plain pilot through V65Filter (v1's refused run).

**Per seed it prints** each run's clear (a run that does not clear in the 120 s cap counts the cap), deaths, falls, the verb's uses, STYLE
above DULL and top rank, **metres walked and shots fired** in the room's fight (the lead's ask: the refused run moved and fired; a WARN when it
walked under 5 m or fired none), the bodies hit per charged shot and, in a ride room, the first ride's time and start (builder R's asks);
the refused-in run's presses refused (episodes declined + the inner pilot's presses dropped); the paired ratio min(refused-in, walker) /
verb-aware and which refusal was faster; exercised or not (**the verb-aware run used the verb AND the refused-in run refused a press**, both
named when not); the pieces the verb-aware run touched (steps) and launch36's kills with where.

**The verdict** `n65verb skip: M1 PASS|FAIL|UNSURE | room <id>, ratio <x> vs bar <b>, exercised <n>/<seeds>`: the median of the exercised
seeds' paired ratios (the mean of the middle two for an even count) against the bar; UNSURE under ceil(0.7 x seeds) = 7 of 10 exercised.
The bar: `--bar b`, else `--bars <slate.json>` (the room's slice's `mainBar`; null or no slice: 1.25, said in the header), else 1.25; a missing
`--bars` file is refused (exit 2). Then `n65verb skip: M2 PASS|FAIL` (STYLE above DULL pooled over the exercised seeds, the verb-aware run
against each seed's faster refusal, +15 needed), `n65verb skip: totals` (medians, clears, deaths, uses, presses, walked, shots, the wall
time), `n65verb skip touch: room <id> | <piece key> | seeds <k>/<n> | steps | <job: label>` for every piece of the room (boxes, ramps, pads;
the key is the geometry, for n66-deco.cjs) and `n65verb skip launchkill:` lines. Exit 0 PASS, 1 FAIL, 3 UNSURE, 2 refused.

**The planted rooms** (`T/t1b-planted.sh`; n52-foundry, 10 seeds, the default bar 1.25; `--plant-forced` / `--plant-null` act through the
loadout's bonus hook in the harness only):
| planted | file | M1 line | the runs (verb-aware / refused-in / walker) |
|---|---|---|---|
| **forced**: b1, verb65 launch, the room's first launchable husk takes damage only from launch36 | `t1b-plant-forced.txt` (59ae6fed) | `M1 PASS \| room b1, ratio 3.84 vs bar 1.25, exercised 10/10` | cleared 10 / 0 / 0; median 31.2 / 120 (cap) / 120 s; launch36 20 / 0 / 0; walked (median) 224 / 687 / 641 m, shots 28 / 107 / 111: the refusals played on and fired for the whole cap; M2 PASS 58.1 vs 15.2 % |
| **null** (verb-irrelevant): the same room, launch36's damage x0 | `t1b-plant-null.txt` (59ae6fed) | `M1 FAIL \| room b1, ratio 0.95 vs bar 1.25, exercised 9/10` | cleared 10 / 10 / 10; median 31.5 / 31.5 / 30.3 s; launch36 22 / 0 / 0; walked 223 / 164 / 168 m, shots 28 / 32 / 28 |
| **no fire**: a2 (two husks, no spitter), verb65 parry | `t1b-plant-nofire.txt` (7b2139b2) | `M1 UNSURE \| room a2, ratio - vs bar 1.25, exercised 0/10 \| too few seeds exercised (0 < 7)` | globs parried 0 / 0 / 0, presses refused 0 / 0; the three runs identical (13.9 s, 91.8 m, 8.5 shots): every seed "NOT exercised" with its reason |
| reference, natural: a2, verb65 pierce (the old room NIGHT65's red read 0.85 on) | `t1b-plant-natural-a2-pierce.txt` (59ae6fed) | `M1 FAIL \| room a2, ratio 0.65 vs bar 1.25, exercised 10/10` | median 12.5 / 9.0 / 10.6 s; charged shots 39 / 0 / 0; walked 94 / 74 / 86 m, shots 8 / 18 / 22: the refused-in run tapped and cleared faster than the charging pilot (the reverse confound is gone: it does not stand to charge) |

**A fault the plants found and fixed (7b2139b2):** the first no-fire run (`t1b-plant-nofire-59ae6fed.txt`, kept) printed "parries 5 / 5 / 5"
in a room with no spitter: Parry36.Event also fires on the autopilot's melee parries, in every run. It still read UNSURE 0/10 (the refused-in
run refused nothing), but a parry room's "uses" would have counted melee parries. Skip v2's parry uses are now reflected globs
(SpitterAcid.reflects in the room's fight); parry36 events are printed beside them.

**In the round and GATE tools (7b2139b2):**
- `Tools/night/n66-round-verbs.sh`: the skip line is `skip <the slot rooms> --seeds 10 --bars Logs/n66/slate.json` (v2; no `--verbs65`).
- `Tools/night/n66-gate-pack.cjs`: the fresh skip is `n65verb skip --level <the round's level> --room <id> --seeds 10 --bars
  <main tree>/Logs/n66/slate.json` (the pack throws if the slate is missing); verbs65-skip.txt keeps v2's header, `runs:`, seed, M1 / M2 /
  totals, touch and launchkill lines; its line picker (`pick`) reads v2's formats (checked on `t1b-plant-forced.txt` with a scratch copy of
  its regexes: b1 86 of 86 lines picked, a2 0).
- `Tools/night/n65-verbs.sh` (the runner both call): counts `M1 PASS` as GREEN, `M1 UNSURE` as UNSURE, anything else RED.
- `--bars` is read against the command's working directory; the round runs from the main tree's root, so `Logs/n66/slate.json` is the
  lead's slate. Every room's mainBar is null until the lead writes it: until then each line says "the default 1.25 (... mainBar is null)".
- Checked: `bash Tools/night/n65-verbs.sh skip Design/levels/n52-foundry.json b1,a2 --seeds 2 --plant-verb65 b1:launch --bars <main
  slate>` -> `T/t1b-verbs-sh-tally.txt`: b1 `M1 PASS`, a2 `M1 UNSURE ... no verb65`, tally `GREEN 1, RED 0, UNSURE 1`. The whole
  n66-round-verbs.sh and n66-gate-pack.sh were **not run** by me (they need a round folder and the lead's tree; A1's dry run covers them).

**Wall time, one room at 10 seeds (the lead's ask):** 81-97 s on NIGHT65's final rooms (b1 97 s, a4 81 s, a3 82 s below), 66-84 s on old
rooms, 127 s on the forced plant (its refusals run to the 120 s cap). Six rooms: about 8-10 min per round, one dotnet job.

**A first look on NIGHT65's final rooms** (the latest assembly at the t0 sha, which is red65's level; `T/t1b-latest.sh` assembles it in this
tree, copies it to a scratch path and puts the seven generated files back; bars from the main slate, all null, so 1.25):
| room | file | M1 | M2 | median verb-aware / refused-in / walker | notes |
|---|---|---|---|---|---|
| b1 | `t1b-latest-b1.txt` | PASS 1.33, 10/10 | PASS 67.5 vs 33.4 % | 26.5 / 39.8 / 44.1 s | launch36 16 / 0 / 0; refused-in walked 219 m, 35 shots |
| a4 | `t1b-latest-a4.txt` | FAIL 1.22, 9/10 | PASS 32.3 vs 16.8 % | 21.9 / 33.0 / 33.0 s | globs parried 12 / 0 / 0; refused-in and walker identical (a parry episode plays nothing when refused) |
| a3 | `t1b-latest-a3.txt` | FAIL 0.72, 10/10 | FAIL 30.5 vs 28.5 % | 34.5 / 27.3 / 27.3 s | wall rides 69 / 1 / 1; seed 1's first ride 5.5 s at (3.7, 1.4, -67.8); refused-in = walker (ride) |
These are not the lead's red65 (that is `n66-round.sh red65` on the main tree); NIGHT65's v1 read b1 1.74, a4 1.40, a3 0.71 on paired
geo-means of 5 seeds (n65-data.md section 1), so v2 is lower on the two rooms the v1 confound helped.

**What v2 cannot tell apart:** for ride, parry and slide a refused episode plays nothing, so refused-in and walker are the same run
(identical clear, metres and shots in a3 and a4 above); only launch and pierce rooms get a distinct refused-in run (the aim kept, the gun
running). The walk pilot's own use of another verb is printed, not refused: in a launch room the walker charges the KESTREL
(`t1b-plant-null.txt` seed 1 walker `charged 6`, `t1b-latest-b1.txt` seed 1 walker `charged 4`: the autopilot's held fire).

**Checks:** SimHarness builds 0 errors; `bash Tools/night/n66-gates.sh --no-editor` -> `T/t1b-gates.txt`: `gates: all GREEN 00:59:40`
(fingerprints plain and --off57..--off66 EQUAL; V65VerbPilot's refused mode is off unless skip v2 sets it, so n46flow --verbs65 runs as
at t0). No Assets C# changed in T1b (no dn-check needed).

**Not checked:** the reds (`n66-round.sh red --shipped`, `red65`) and the mainBars (the lead's); n66-round-verbs.sh / n66-gate-pack.cjs
end to end; a planted forced case for pierce or parry (the forced / null plants take launch rooms only: the harness has no public flag
for a charged shot's damage).

## T1b fix (code 631e1e26, 944c3bac, 9d31727a, and fe586cd3's `--slate` in the pack; after the critic's FAIL, Logs/n66/verdict-T1b.md)

631e1e26's first ride rule (the stick's whole push into any wall taken out, no speed or clearance gate) was replaced by 944c3bac before
any proof was kept: its run changed the walker on hundreds of steps a seed (moved to my scratch, not committed).

**1. The ride refused, never the jump (BLOCKING 1).** V65Filter's old ride rule dropped every held jump in the air in both refused runs.
The new `RefuseRide` (Harness.N65.Verb.cs) follows ride40's own gates (Mover.cs Ride40Intent; config ride40 inputDot 0.1, minSpeed 4,
minClearance 1; ride43 holdNoStick false, dashIntoDot 0.1). It acts only in the air (or on a jump press), at minSpeed - 0.5 or more, with
the floor at least minClearance - 0.1 under the feet and a wall within capsuleRadius + 0.75 m (8 rays at 0.5 and 1.4 m). There the stick's
unit push into that wall, when at inputDot or more, is cut to half of inputDot (the rest of the stick kept), and a dash press aimed into it
is dropped. The backstop: a ride that starts anyway is ended at once by the Mover's own away rule (the stick away from the wall: StepWallRide,
wallRide.awayDot). **The jump buttons are never touched.** Each refused run prints the steps steered, the dashes dropped, the rides ended and
its steps on a wall.

Proofs (`T/t1b-fix.sh`, outputs `T/t1b-fix-proofs.txt`, `t1b-fix-noride-a2.txt`, `t1b-fix-noride-a4.txt`, `t1b-fix-a3.txt`,
`t1b-fix-a3-summary.txt`):
- **The critic's equality test cannot hold as posed:** on the old a2 and a4 (no ride36 face) the PLAIN walker rides ordinary walls 1-6 times a
  seed with nothing refused (a2: 1, 3, 4, 6, 5, 4, 2, 4, 4, 4; a4: 3, 2, 0, 2, 4, 3, 3, 3, 5, 4). Those are the verb, and refusing them
  changes the clear time (a2 seed 1: 5.2 s -> 12.9 s, with 0 rides refused-side). The only seed where it never rode, **a4 seed 3, reads 10.5
  s both ways** (1 step steered): with no ride to refuse, the run is unchanged.
- **a3's refused runs:** 8 / 10 seeds 0 wall rides; seeds 7 and 9 one ride each, **1 step on the wall** (ended by the away rule) in both
  refused runs. So the ride does start on 2 seeds and lasts one step: not the "0 on 10 / 10" asked for, said plainly. The verb-aware runs ride
  3-12 times a seed (69 in all).
- a3 at 10 seeds on the latest assembly: `M1 FAIL | room a3, ratio 0.83 vs bar 1.25 (mainBar)` (it read 0.72 with the old filter): the
  refusals clear in 24.6 s against the riding pilot's 34.5 s. The refused runs no longer lose their jumps, and a3 still FAILs.
  - The wall time here, 204 s, was measured while the lead's r1 ran on the same machine.

**2. The bar's source on the M1 line (BLOCKING 2).** The M1 line now ends its bar with `(mainBar)`, `(mainBar null)`, `(no slice for the room
in the slate)`, `(--bar)` or `(default: no --bars)`: `t1b-fix-a3-null.txt` (a planted slate, a3 null) `M1 FAIL | room a3, ratio 1.03 vs bar
1.25 (mainBar null), exercised 2/2`; `t1b-fix-a3-bar.txt` `vs bar 1.40 (--bar)`; `t1b-fix-a3.txt` `vs bar 1.25 (mainBar)`.
`n66-gate-pack.cjs` refuses a room whose mainBar is null or missing: `t1b-fix-pack-null.txt` (`--slate` = a copy with a2 null, made by
`T/t1b-slate-null.cjs`): `Error: a2's mainBar in ...slate-null-a2.json is null: the GATE's M1 needs the room's own bar`, exit 1. The pack and
n66-round-verbs.sh now read the MAIN tree's slate from any worktree (git's common dir; the critic's problem 10).

**3. A pierce use is a charged shot through >= 2 bodies** (the critic's problem 5): skip v2's pierce uses (and so "exercised") count
`chargedHits >= 2`, printed as "pierces (charged shots through >= 2 bodies)". And the pierce refusal (the critic's problem 6) drops the held
fire only while the KESTREL is in hand. Neither was re-run on a pierce room after the change: **not checked** on a2 / a8.

**Checks:** SimHarness 0 errors; `bash Tools/night/n66-gates.sh --no-editor` -> `T/t1b-fix-gates.txt`: `gates: all GREEN 01:46:12`.

**Still open from the critic (not fixed):** problem 3's direction bias (the verb-aware ride episode rides to the face's farther end, not
toward the exit); problem 4 in part (the refused runs' "presses refused" is now verb-specific for ride and pierce, the exercised rule's second
clause still reads any of them); problem 7 (the parry refusal drops a punch at a husk while a glob is near); problem 8 (the refused-in launch /
pierce aim); problem 9 (no forced plant for ride, pierce or parry).

## T1c (code 348d5377, 415b388d, b4c0f5c8, 052c891a)

**First, T1a's critic's finding 1 (348d5377):** `gen-n65-foundry.cjs --plant-error` is refused without `--run` and `--out=<dir>` outside
Design / Assets / Reports, so a planted plan or level never lands in a tracked file; `--finish` refuses a level whose levelgen report is
missing, lists an ERROR row, or is older than the level. `T/t1c-gen-out.txt`: the three refusals; the planted error into a scratch `--out`
(2 ERROR, refused, the tracked copies `5f3eba53...` byte-identical); a standalone `--finish` of that planted level `REFUSED ... lists 2 ERROR
row(s)`; the latest into a scratch `--out` (0 ERROR); `git status --short Design/levels Assets/StreamingAssets/Design/levels Reports` empty
after every step.

**doneWhen 1, `n65verb ride --shoot` (415b388d):** the ride pilot on the room's own ride36 face with its wave live (rider put 1.2 m up beside
the face's near end at 11 m/s, jump held, the stick along and into it, n53ridekill's entry). `--shoot` steers the look OFF THE WALL at a target
(the planted husk, else the nearest live body in clear sight on the open side), re-aims the stick so the ride holds (the a253 reel's
re-aim), fires a one-step press every 12 steps while the line is clear; a kill counts from the wall when the last shot before it was fired in
WallRide within 0.6 s. `--plant-target auto` holds a husk (its own 40 hp) 10 m along the face, 3 m out. `T/t1c-ride.txt`:
- latest a3: no --shoot `ride yes (1.8 s, 16.2 m) | look off the wall at most 0.0 deg | kills from the wall 0`; `--shoot`: `ride yes (4.7 s)
  | look off the wall at most 180.0 deg | shots from the wall 8 | kills from the wall 4`; `--shoot --plant-target auto`: `kills from the wall
  4 | the planted target #5 killed from the wall`.
- old b1 (its crane rails): `--shoot` kills from the wall 3; with the plant 4, `the planted target #4 killed from the wall`.
- control (`T/t1c-ride-control.txt`): b1 with the plant and no --shoot: `kills from the wall 0 | the planted target #4 ALIVE`.
- "at most 180 deg" means it turned to shoot a body behind it along the face; the pool's reuse of a dead body's object (and id) is handled
  (the plant is its first id).

**doneWhen 2, `Tools/night/n66-deco.cjs` (b4c0f5c8):** `--base <sha> <room> [--touches <skip v2 output>]... [--file <room file>]`. The pieces
added or moved since <sha> (boxes, ramps and pads with zone = the room, keyed by geometry exactly as skip v2 keys them; cyan lights / props
as markers), each with the four tests: (a) touched by skip v2's verb-aware run on >= 3 of 10 seeds; (b) blocks a ranged spawn's sight of a
walk-line sample (the spitter waves' waveNodes43 spots, else nodes / highNodes, eye 1.2 m, to the spawn -> exit line every 1 m at 1.6 m);
(c) a launch36 kill burst within 1 m of it; (d) a cyan marker, at most 2 a room, within 6 m of the exit or a box / ramp / pad. A piece
passing none FAILs. Non-cyan lights and colliderless props are dressing: listed, not judged, with a WARN when a change adds dressing and no
piece. `--plant <room> <out>` writes the planted room file.
- The planted case (`T/t1c-deco-plant.txt`, `T/t1c-deco-plant-touch.txt`): b1's room file + crate A (1 m, a free corner) + crate B (a
  0.3 m step 2 m ahead of the spawn), assembled with `gen --room-file=b1:<planted> --run --out=<scratch>` (the tracked files untouched),
  skip v2 at 10 seeds on it, then the checker against e37083cf: `FAIL added box(-9.3,0,-179.3..-8.3,1,-178.3) | PLANTED crate A | a touched
  no (2/10 seeds (need 3)); b sight no (blocks 0 of 124); c launch no; d marker no` and `PASS added box(-0.8,0,-151.3..0.8,0.3,-149.7) |
  PLANTED crate B | a touched YES (10/10 seeds)`; exit 1.
- On real pieces: against b1's first room-file commit 73b88cbd (NIGHT65's whole rebuild), with t1b-latest-b1.txt's touches: 24 pieces, 19
  PASS, 5 FAIL: P6 THE GANTRY POST south / middle / north (touched 2, 1, 1 of 10; no sight blocked) and the 3rd and 4th cyan markers
  (b1-exitLintel, b1-exit: over the 2 a room). Not saved as a file (run on the command line): **re-run before citing it**.
- Its limits: (b) is a static segment test against the piece alone (a sight already cut by a wall still counts); the walk line is spawn ->
  exit straight; "touched" is the capsule's box within 0.15 m (leaning on a wall counts); a pickup prop is dressing.

**doneWhen 3, the round's level outside the shipped files (052c891a):** `n66-round.sh` assembles with `gen --run --out=Logs/n66/<tag>`; the
red (`LEVEL_DIR`: `n59-l-room.py --dir`, `n46flow --file`, `hold52 --file`, the plan PNG and `n53ridekill ride --level` on the folder's
files), the iso pictures, the verb pilots and skip v2 (`n66-round-verbs.sh` with `LEVEL_FILE`) and air65 all read the round folder; the
round writes `tracked-status.txt` (the tracked level files' git status before and after) and no longer puts a shipped level back.
- `n46flow --file` and `n59-l-room.py --dir` against their tracked-path runs on the same bytes (`T/t1c-paths-eq.txt`): room b1 and a3
  `--dir EQUAL`; n46flow b1 3 seeds `--file EQUAL`.
- `n66-red.sh` on one room with `LEVEL_DIR` = a scratch `--out` folder of the latest assembly (`T/t1c-red-dir.txt`): tracked status `[]`
  before and after; the room check read the folder's b1 (`ROOM b1: PASS`, where the tracked all-old b1 reads `FAIL R5 foothold`); flow A / B
  / V `GREEN 10/10`; pads PASS. The plan PNG was not made there because n52-svg-png.sh puts `$PWD` in front of its output path (my
  absolute scratch path); a round's relative `Logs/n66/<tag>` path works as before (r1 has plan.png).
- **Not run: a whole `n66-round.sh` end to end** (about 20 min; your next round or a dry run is its proof: `tracked-status.txt` must read
  empty before and after).

## T1b fix 2 (code 8ba4c56e; after the round-2 critic's FAIL, Logs/n66/verdict-T1b-r2.md)

1. **The ride refusal is the backstop only.** The stick cut and the dash drop are gone: a refused run's input is the walker's own until the
   Mover is in WallRide; on that step the stick points away from the wall, past wallRide.awayDot, so StepWallRide's away rule ends the ride.
   Each run prints the rides started and ended, the steps the stick was turned, and its steps on a wall.
2. **The proof** (`T/t1b-fix2.sh`, `T/t1b-fix2-proofs.txt` and the four `t1b-fix2-*.txt` it saved):
   - a planted room with no rideable wall by construction, by the game's own rule: `--set design.n40.ride40.minWallHeight=99` (skip's new
     `--set`, n46flow's kind: this process's config only). Every ride entry (the attach, the draw, the dash attach) goes through
     Mover.Ride40Intent, which refuses a face with less wall than minWallHeight over the feet; climb walls (the one exception) are only in a9.
     The walker with `--refuse none` against `--refuse ride` (`--v1`): **old a2 10 / 10 seeds bit-identical, old a4 10 / 10** (e.g. a2 seed 1
     10.3 / 10.3 s, a4 seed 1 23.3 / 23.3 s), wall rides 0 / 0, presses refused 0, on every seed. (The plant changed the room: a2 seed 1 was
     5.2 s with the walker's own rides.)
   - a3 on the latest assembly, skip v2: the refused runs start 2-7 rides a seed (42 in 10 seeds) and **every one is on the wall exactly one
     step** (`rides started and ended 7 | steps on a wall 7`, ... on 10 / 10 seeds, both refused runs). `M1 FAIL | room a3, ratio 0.73 vs bar
     1.25 (mainBar), exercised 10/10`; medians 34.5 / 23.6 / 23.6 s.
   - What a one-step ride still gives the refused run (the round-2 critic's problem 4): EndRide("away") opens the wall coyote and ride35's
     late jump-off, and the start refreshes the double jump. That favours the refused run (a lower M1): not measured.
3. **Pierce:** a use is a KESTREL charged shot (ChargeShot27Event.gun == "revolver") through >= 2 bodies; a charged shotgun shot is counted
   in `charged` only. `T/t1b-fix2-a2-pierce.txt` (the latest a2): pierces 0, 1, 1, 1, 1, 1, 1, 1, 2, 1 a seed (bodies per KESTREL charged
   shot printed, e.g. seed 9 `2/1/2/1`); `M1 FAIL | room a2, ratio 0.96 vs bar 1.25 (mainBar), exercised 9/10`.
4. Also: the GATE pack's verbs65-skip.txt header names the slate it read (the round-2 critic's problem 11).

**Still open (the round-2 critic's list):** the ride episode's farther-end direction (problem 3); exercised's second clause for ride now
counts the backstop's refusals only, so it reads "a ride started and was ended" (problem 7, in part); problems 8 (parry punch rule, launch /
pierce aim lock) and 9 (no forced ride / pierce / parry plant); the skip header printed twice (problem 12: not found in my runs, which print
it once; not checked further).

## T2 (code 9b68b214): b2 THE GANTRY as the seventh slot

**What changed:** `gen-n65-foundry.cjs` SLOTS = a2 a3 a4 a5 b1 b2 a8 (the plan's order), KEPT = a1 a6 a7 a9; `--export b2` wrote
`Design/levels/n65/rooms-n52/b2.json` (n52's b2 exactly: 13 boxes, 2 pits, 12 lights; kind link, its whole row: the lesson, flight38,
fallReturn43, bot43 untouched) and `Design/levels/n65/rooms/b2.json` (its copy, so `--slots=latest` assembles; no one edits it without GG's
word). `n65-slot-set.cjs` knows b2 (a slots65.json written before T2 has no b2 entry: gen reads a missing slot as old, and the setter prints
it so). `n39views wall` takes a level path.

**doneWhen 1:**
- `node Tools/night/gen-n65-foundry.cjs --check` -> `T/t2-check.txt`: `PASS Design/levels/n65/rooms-n52/b2.json is n52's b2 exactly`;
  `PASS every slot old: the assembled plan equals Design/levels/n52/foundry.plan.json apart from id / _why`; `PASS every slot old: levelgen
  --walk (0 ERROR) + the finish: the level equals Design/levels/n52-foundry.json apart from its names`; `... the bytes ARE
  Design/levels/n52-foundry.json (2474618 chars)`; every planted case PASS; `gen-n65-foundry --check: all PASS`.
- slots65.json is the lead's file, so I ran the setter dry only (`T/t2-proofs.txt`): the self-test `PLANTED GREEN (5 cases)`, and `b2 old
  <reason> --dry` would write `"b2": {"use": "old", "why": ...}`, `slots: a2 old, a3 old, a4 old, a5 old, b1 old, b2 old, a8 old`. **The
  lead's command:** `node Tools/night/n65-slot-set.cjs b2 old "NIGHT66 T2: b2 THE GANTRY joins the slots as n52's room; rebuilt only on GG's
  own word" --slate=Logs/n66/slate.json`. Once b2 is in slots65.json the round tools run it too (they read the rooms from slots65.json):
  room check, flows, pads and the ride pilot on b2, and skip v2 prints `M1 UNSURE ... no verb65` for it.
- The all-old assembly with b2's slot (`gen --slots=old --run --out=<scratch>`) against the tracked n65-foundry.json (assembled before T2,
  b2 then a kept room): `t2-samebytes: EQUAL apart from name / _why / generatedBy (2472006 / 2472006 chars of the rest)`.

**doneWhen 2: SHORT on its premise.** `n39views wall <the all-old assembly with b2's slot> n65-foundry n52-foundry` (`T/t2-proofs.txt`):
rows 9, 10 and 11 PASS on all three, `n39views wall: 3/3 levels with rows 9 and 11 PASS on the picked wall`, and the pick is the same on all
three. But the picked room printed is **a7, not b2**: `NIGHT38 pick: 30 m in b2 at (8.7, 0.2, -226.6)`, `NIGHT39 pick: 18 m in a7 at (1.1,
0.2, -326.2) along (0, 1) (CHANGED)` (THE FLUE's east chimney wall). The reel's pick (VerbsWall39.Pick with NIGHT39's rule) is a7's 18 m
wall on n52-foundry itself, so "verbs37's ride rows still pick b2's 25-30 m wall" was not true before T2 either; b2's 30 m wall is
NIGHT38's plain pick. The "14 m lane" is not printed by this tool: not checked. T2 changed no pick (the three blocks are identical).

**Checks:** SimHarness 0 errors; `bash Tools/night/n66-gates.sh --no-editor` -> `T/t2-gates.txt`: `gates: all GREEN 02:20:05`.

**Not checked:** the slot setter's real write (the lead's); a round with b2 in slots65.json; the links between rooms as slots (leftover 1).

## T1c fix (code 79636744; after the critic's FAIL, Logs/n66/verdict-T1c.md)

**1. The iso step draws the round's own plan.** `n66-round.sh` passes `$out/foundry.plan.json` to n65-iso.cjs (its comment says so), and
`n65-iso.cjs` reads a plan beside a level file (`<dir>/foundry.plan.json`) before the tracked one the level's name names. `T/t1c-fix-iso.txt`
(r1's files in the main tree, read only, pictures to my scratch): r1's plan `4 boxes, 1 ramps, 1 pads`; r1's level (now the plan beside it)
`4 boxes, 1 ramps, 1 pads` (the same png sha256 380ec1bd...); the tracked level `1 boxes, 1 ramps, 0 pads` (4e4451a8...).

**2. Test (d), and (b).**
- (d) a cyan marker, one of the room's first 2, passes only within 6 m (floor plan) of the room's exit (row.exit), or within 6 m of the piece
  its new `marks` field names (a piece id, its why head or its key; "exit" too) when that piece does a job by (a), (b) or (c). No `marks`:
  only the exit counts.
- (b) a sight counts only when no OTHER solid of the room cuts it and this piece does (the critic's "already cut by a wall").
- `node Tools/night/n66-deco.cjs --selftest b1` -> `T/t1c-fix-deco-selftest.txt` (b1's file, its cyan markers stripped, the planted pieces
  added, t1c-deco-plant-touch.txt's table): **8 / 8 PASS**: B+ a box on a clear spitter sight `b sight YES (cuts 3 of the 66 sights clear
  without it)`; B- a box inside the west rail `cuts 0`; C+ a block under a recorded launch kill `c launch YES`; A- a corner crate, every test
  no, FAIL; D- a cyan light in the far corner, 6 m up, no marks: `the exit 31.0 m off ... no marks (only the exit counts)` FAIL; D+ a cyan
  pool 1 m from the exit PASS; D+m a light over the west rail with `marks` naming it (touched 9/10: a job) PASS; D-m a light over the corner
  crate with `marks` naming it (no job) FAIL.
- The same on a4 (`--selftest a4 Research/2026-10-23-wave66/T/t1b-latest-a4.txt` -> `T/t1c-fix-deco-selftest-a4.txt`): 7 / 7 PASS, C+ n/a
  (no launch kill in a parry room's table).
- `marks` reaches the level: `T/t1c-fix-marks-gen.txt`: b1's rail light given `marks: "P15 THE LADLE"`, assembled with gen --room-file
  --out: levelgen 0 ERROR and `"marks":"P15 THE LADLE"` in the level's lights; tracked status `[]`. Whether the game's light loader ignores
  the key: not checked in Unity (the harness reads the level).
- NIGHT65's b1 under the new rules (`T/t1c-fix-deco-b1-n65.txt`, against 73b88cbd, the latest b1 touches): 24 pieces, 17 PASS, 7 FAIL: the
  three P6 GANTRY POSTs (as before) and **all four cyan lights**: b1-rail and b1-ladle (no marks, the exit 16.8 / 13.3 m off) and the two exit
  lights (the room's 3rd and 4th cyan markers: over the cap of 2). A builder fixes them with `marks` on the rail / ladle lights and at most
  two cyan markers.

**Not checked:** a whole `n66-round.sh` run (the lead's round is its proof: its iso.png from the round's plan, tracked-status.txt unchanged).
