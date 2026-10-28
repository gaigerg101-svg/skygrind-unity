# NIGHT65 builder T: notes (branch n65-T from 2f3e4d94; branch C, no Player)

t0 = 2026-10-23 13:58:02 (MST). Evidence files: Research/2026-10-23-wave65/T/ (each with its command, sha and time at its top, written by
Tools/night/n65-save.cjs).

## T1 (due 1:00): the assembler, the slot tool, the bot opt-out

### What was built
- **Tools/night/gen-n65-foundry.cjs**, the assembler. It reads n52-foundry's plan (Design/levels/n52/foundry.plan.json) read only. Six slots
  (a2 a3 a4 a5 b1 a8) take **room files**, and every other room (a1 b2 a6 a7 a9), every door and every level key stays n52's bit for bit
  (asserted).
  - A room file `Design/levels/n65/rooms/<id>.json` = `_why, room, row` (the whole room row, every key) + `boxes, ramps, pads, grapple, pits,
    tunnels` (zone-keyed) + `lights, props` (room-keyed) + `teach35` (its entries) + `drop` (reasons for dropped row keys). One line per piece
    and per row key, so a builder's diff is a line per change.
  - `Design/levels/n65/rooms-n52/<id>.json` are the six n52 rooms exported exactly (never edited). `--check` re-exports them from the plan and
    compares. `rooms/<id>.json` started as the same bytes: they are the builders' starting point.
  - Slot sets: `--slots=latest` (every rooms/ file), `--slots=old` (every rooms-n52/ file = n52's room), `--slots=a2,a5` (those latest, the
    rest old), `--slots=<slots65.json>` (the shipped set; a "new" slot's file must equal its file at the PASS's sha, else refused). Default:
    Design/levels/n65/slots65.json if it exists, else old.
  - Per slot it prints a per-key **keep / replace / add / drop** table (for every slot whose file differs from n52's room; `--table` prints
    all). It **refuses**: an unlisted row key (KNOWN: the 94 keys any level's rows have carried + frames65, line65, bot43, card65; doc keys
    `*_why` / `*Why` always allowed), an unlisted file key, a dropped row key with no reason, a change to `id` / `kind` / `rect` (the doors
    fix the rect), a piece keyed to another room, and a piece of a rebuilt room without a job and a why (R8). It warns when a rebuilt row has
    no `bot43: false`. It fills `note = why` on a piece that leaves note out (the plan's convention).
  - A list the file leaves exactly as n52 had it is not touched (a8's boxes and lights are not contiguous in n52's lists); a changed list goes
    in at its room's first row (gen-n57-kiln.cjs's swap).
  - `--finish` keeps gen-n52-foundry.cjs's finish (lines 429-473): data.name "n46-foundry", a9's wick47 / frames47 / climbTop48 re-applied in
    the same order, a9 asserted bit-equal to n52-foundry.json's, the kept rooms, doors and other rooms' rows asserted equal; both copies
    written and compared. `--run` = plan + levelgen --walk + finish (the builders' loop in one command).
  - levelgen exits 1 on any "plan checks" problem, and n52's own plan has one (ground spawn 22 overlaps box 88, a wall in b2), so the tool's
    gate is the level report's ERROR count (the doneWhen's "levelgen --walk 0 ERROR").
- **Tools/night/n65-slot-set.cjs**, the only writer of Design/levels/n65/slots65.json. `<room> new` needs the slate slice `<room>` with
  passes true, a passSha and a verdict file whose first line is exactly `<room>: PASS`, and rooms/<room>.json unchanged since passSha (git diff
  passSha against the working tree). It records the passSha, the commit and the file's blob. `<room> old <reason>` rolls a slot back.
- **The bot opt-out:** a row with `bot43: false` makes the walk pilot skip the id-keyed scripts: Walk36.Room36 (before its `switch (rd.id)`),
  Walk38Foundry.SampleMove (before Walk43 / the n38 b2 chain / the finale; fire43 cleared) and Walk43Foundry.Walk43. `BotRows65.OffRooms`
  reads the level file's campaign rows. No row of any level before NIGHT65 has the key, so the old levels run as before. `Walk38Pilot.idScriptSteps65`
  counts the steps Walk43's script drove. Test: Assets/Tests/EditMode/Game/N65BotOptOutTests.cs (3 tests). Its headless twin: `n65bot`
  (Tools/SimHarness/Harness.N65.Bot.cs).
- **Tools/night/n65-save.cjs**: runs one command and saves its output with the command, the sha and the time at the top (L-149).

- **The opening (the lead's add, from S's cards):** a room file may carry a top-level `"opening": "vista" | "ambushed" | "ambusher" | "held"`
  and `"opening_why": "<the card's line>"`. The plan step writes Design/levels/n65/openings65.json (`{"foundry": {<id>: <opening>},
  "_cards": {...}}`) from every slot that is not old and whose file sets it. n59-l-room.py n65-foundry overlays that file on n52's
  openings53.json, so a room with no opening, or an old slot, keeps n52's. Any other value is refused. An opening alone does not count as a
  rebuild (no bot43 warning).
- **Tools/night/n65-flow-cmp.cjs**: one level's n46flow block against the flow base, as n65-gates.sh --flow compares (wall-time lines aside).

### How a builder uses it (R / S)
1. Edit only `Design/levels/n65/rooms/<id>.json`: the `row` (keep `id`, `kind`, `rect`; add `bot43: false`, `frames65`, `line65`,
   `card65`), the piece lists (every new piece with `job` and `why`; `note` is filled from `why`), `teach35`, `opening` / `opening_why`.
   A row key you remove needs `"drop": {"<key>": "<reason>"}`. A new row key not in KNOWN is refused: ask T / the lead.
2. `node Tools/night/gen-n65-foundry.cjs --slots=latest --run` (the plan, levelgen --walk, the finish: about 10 s), then the room checker,
   n46flow, hold52 as the brief says. Never commit the generated files (Design/levels/n65-foundry.json and its StreamingAssets copy,
   Design/levels/n65/foundry*.json, openings65.json, slots65.json, Reports/LEVEL-n65-foundry-validate.md).

### doneWhen, each with its evidence (Research/2026-10-23-wave65/T/)
| doneWhen | command | evidence | result |
|---|---|---|---|
| --check with every slot old = n52-foundry apart from names; a planted one-box change prints the difference; the finish keeps a9 and data.name | `node Tools/night/gen-n65-foundry.cjs --check` | check-T1.txt | all PASS: the plan equal apart from id / _why; the level equal apart from name / _why / generatedBy, and with n52's three names the bytes ARE n52-foundry.json (2474618 chars); the planted a2 box prints collision, navGrid, data, campaign.boxes (1 row each side); a9 asserted bit-equal in every finish |
| the room file is the whole row + pieces + lights / props + grapple + teach35; the per-key table; an unlisted key refused | `--check` (planted), `--slots=latest --dry-plan --table` | check-T1.txt, assemble-latest-T1.txt | refused: an unknown row key, an unknown file key, a drop with no reason, a piece keyed to another room, a widened rect, a new piece with no job / why, an opening "sideways"; the full keep table of all six slots |
| --slots=latest and --slots=slots65.json both work; a planted room replaces only its slot; levelgen --walk 0 ERROR | `--slots=latest --run`; `n65-slot-set.cjs a4 old ...` then `--slots=Design/levels/n65/slots65.json --dry-plan`; `--check` | run-latest-T1.txt, slots65-assemble-T1.txt, check-T1.txt | latest: levelgen 0 ERROR, both copies identical; slots65.json (all old) assembles; planted a5 rebuild: every differing campaign row is a5's, 0 ERROR |
| n65-slot-set writes slots65.json only when the room file is unchanged since passSha (a planted changed file refused) | `node Tools/night/n65-slot-set.cjs --selftest` | slot-set-selftest-T1.txt | PLANTED GREEN, 5 cases (incl. the assembler refusing a changed file under a new slot) |
| bot43: false runs the generic autopilot, with an EditMode test; the old level's rows unchanged | `n65bot --level n52-foundry --rooms a8,b2,a7,a3`; `n46flow --level n52-foundry --seeds 10` + `n65-flow-cmp.cjs` | n65bot-T1.txt, flow-n52-foundry-T1.txt, flow-cmp-n52-foundry-T1.txt | id-script steps a8 1393 / b2 1438 / a7 645 / a3 0 as filed, 0 / 0 / 0 / 0 with bot43 false, each room entered in both runs; n46flow n52-foundry EQUAL to Logs/n65/flow-base.txt line for line (23 lines). The EditMode test: compiled (dn-check 0 errors), **not run** (the Editor suite is the lead's) |
| C# checks | `bash Tools/night/dn-check-wt.sh <scratch>`; `bash Tools/night/n65-gates.sh --no-editor` | dn-check-T1.txt, gates-T1.txt | 0 errors in 4 projects; gates all GREEN (golden 0 mismatches, 22 / 22 replays, fingerprint plain and --off57..--off65 EQUAL) |
| the opening (the lead's add) | `--check`; planted a3 opening vista, then `n59-l-room.py n65-foundry room a3`; reverted | check-T1.txt, opening-planted-plan-T1.txt, opening-planted-room-a3-T1.txt | old slot writes nothing, latest writes {"a2": "vista"}; the checker read the planted a3 vista (IN VIEW vista FAIL) and, with the plant reverted, ambushed (PASS) again |

### Not checked
- N65BotOptOutTests in the Editor (compiled only). Its logic is n65bot's, run headless.
- levelgen's own bot walk prints "DID NOT reach the last exit; zone at the end a1" on n65-foundry, and on n52's plan too: the spike's walk,
  not a gate (the level report's 0 ERROR is).
- A rebuilt room through the whole chain (no builder file exists yet): only the planted cases.

## iso65 (due 1:30; replaces T2 as the second job, branch C): the sim-side room picture
- **Tools/night/n65-iso.cjs** `node Tools/night/n65-iso.cjs <plan.json | level.json> <roomId> <out.png> [--svg <out.svg>] [--plant-box
  x0,y0,z0,x1,y1,z1]`. A level file is drawn from its plan (`<px>-<level>` -> Design/levels/<px>/<level>.plan.json), because the pieces'
  kind, job and ride flags live in the plan. One 1600 x 1100 PNG holds:
  - view A, isometric from the entry door (30 deg off the room's axis, 35.3 deg down);
  - view B, from the far corner (the one farther from A's camera) toward the centre;
  - two elevations at one scale: along the long axis (seen from the side, metres from the entry wall) and across it (seen from the entry),
    with the standing planes labelled (deck / slab tops, ramp tops, the floor, pit floors), the roof line and the doors.
  - Drawn: boxes, ramps, pits (lava red-orange), tunnels, props (dressing dashed), lights, pads and their arcs, ride walls with the ride face
    bright, grapple anchors, enemy spots (wave 1 filled, stand-ins x), the player spawn, the exit, line65 (numbered) and frames65 poses.
  - **The cameras and scales come only from the slot's rect, its doors and two constants** (the frame from -4.5 to 14 m), never from the
    room's contents, so the red and the rebuild share the same views.
  - Deterministic: no clock, numbers fixed to 0.1 px, a stable draw order (separating planes between the pieces' boxes, then the centres'
    depth, then the list index). Rasterized by headless Edge with n52-svg-png.sh's flags.
- **Tools/night/n65-iso-proof.cjs** renders the red and runs the checks: `iso-proof.txt` all PASS.
  - The same input three times gives the same PNG bytes (a5 sha256 fc40fddcab42ebe4) and the same SVG.
  - A planted 2 x 3 x 2 m hull in a2 shows in both views and both sections (`iso-planted-box-a2.png`).
  - A planted line65 (5 points), frames65 (2 poses) and grapple anchor draw (`iso-planted-line-a2.png`).
  - n65-foundry assembled with --slots=latest renders all six slots, and with no room rebuilt yet 6 / 6 draw exactly the red's pieces.
  - A level file draws from its plan.
- The red: `iso-red-a2.png`, `-a3`, `-a4`, `-a5`, `-b1`, `-a8` (n52-foundry's plan). Looked at a5, a3, a4, a8 by eye: the a3 runner and the a5
  crane rail read as cyan faces, a8's 4 m pit and its steps show in both sections.
- Not checked: the draw order on every possible rebuild (painter's order with separating planes is exact for disjoint boxes; intersecting
  pieces fall back to depth order and can draw wrong); props with a yaw other than 0 / 90 / 180 / 270 are drawn axis-aligned.

## T2 tour65 (due 2:00): built, EditMode test, Player proof not checked (branch C)
- **The reel** `-skygrindReelOnly tour65` (Assets/Scripts/Game/FeelReel.N65.Tour.cs; registered beside same63 in FeelReel.N57.cs's PhaseN57,
  so a157 and verbs37 are untouched). Boot: `bash Tools/night/n65-reels.sh <tag> tour,n65-foundry,tour65` (the red: `n52-foundry`).
  `-skygrindMap` picks the level file, so the row foundry65 need not be on.
- **What it frames, per room** (every fight room, and any room whose row has frames65), entered as a157 enters a room (DOORFREE, the first
  pose, then wave 1 up and settled):
  - each frames65 pose, in the file's order (`_` keys skipped); a room without frames65 falls back to frames36.mid (pose "mid36");
  - the row's line65 resampled into 8 frames by arc length, each looking along the line.
- **Its proof lines:**
  - one per pose: `[reel] tour65: <room> pose <name> (<source>) <file> | feet (...) yaw pitch | camera (...) | alive seen | campaign <room>`;
  - one per line frame (`line65 i/8`), and one per sequence (`line65: 8 frames <first> .. <last> | waypoints, length`);
  - the done line: GREEN when every toured room was entered with its wave live and every pose framed at 1920x1080.
- **Planted pose:** `-skygrindTourPlant65=<room>:x/y/z/yaw/pitch` (one token, because n65-reels.sh splits extra args on commas). A pose
  whose feet or eye is inside a collision box prints `WARN inside a solid`.
- **Files:** the pure parts are in Assets/Scripts/Game/Tour65.cs (no UnityEngine, so SimHarness compiles it); the numbers are in
  UnityTuning.N65T.cs (8 frames, the pitch clamp and bias, the WARN test points).
- **EditMode test** N65TourTests (5 tests): frames65 order and skipped keys; the frames36.mid fallback in every fight room of n52-foundry;
  8 line frames by arc length with yaw and pitch; the plant flag in both forms; WARN inside a2's deck but not on its top. Compiled
  (dn-check 0), not run (the Editor suite is the lead's).
- **Headless twin `n65tour`** (Tools/SimHarness/Harness.N65.Tour.cs), the reel's dry run on a level file:
  - n52-foundry (`n65tour-red-n52-foundry-T2.txt`): 6 rooms toured, all on frames36.mid, none inside a solid; the planted pose under a2's
    deck prints WARN; the parsing cases PASS.
  - A planted a2 frames65 (3 poses) + line65 (5 waypoints) through the real assembler and levelgen (`tour-planted-assemble-T2.txt`, then
    `n65tour-planted-n65-foundry-T2.txt`): 3 poses + 8 line frames. WARN on the pose inside the deck, and on line frame 7, which cuts through
    the deck (a straight line between two waypoints, not an arc). Plant reverted: rooms/a2.json equals its export again.
- Checks: dn-check-wt 0 errors (`dn-check-T2.txt`); n65-gates.sh --no-editor all GREEN (`gates-T2.txt`).
- **Not checked (branch C):** every Player proof line (the frames, alive / seen, the camera position, the done line); the Editor compile of
  the three new files (the lead's pick; NIGHT41's note "pick a new file, stale Editor": a pick that adds a .cs may need ImportAsset + RequestScriptCompilation); verbs37 / a157 on rc0's level (untouched in code:
  only the phase list gained "tour65").

## Verb pilots (due 2:30; the lead's ask at 1:03): n65verb
- **Tools/SimHarness/Harness.N65.Verb.cs**, `dotnet run --project Tools/SimHarness -c Release -- n65verb <verb> --level <Design/levels/X.json>
  --room <id> [--pose x,y,z,yawDeg] [--seed n] [--off65]`. Each prints detail lines and one verdict line `n65verb <verb>: GREEN|RED | ...`.
  It is a CAMPAIGN run of the level file on the config's own rows (n46flow's), with the room's real waves; the player is put at the pose by
  DOORFREE (DOORFREE off once the room is held). In launch / pierce / parry the player is invulnerable. The pose is `--pose`, else the row's
  line65 first point (yaw along its first segment), else the room's spawn; a pose inside a collision box is refused.
  - **launch [--slide]**: walk at the nearest husk-shaped body, dash inside 5.5 m (or slide), punch inside 0.9 x punch.range. Prints launch36
    events, the flight's end and distance, bursts (hits, kills), where the body came down, whether that was in a pit or lava rect, and what
    killed it. Enemies are pooled, so the track stops at the target's death.
  - **pierce [--yaw deg]**: the KESTREL held to charge35Full, released along line65's first segment, or --yaw, else at the nearest body (tracked
    through the charge). Prints the charged shot's bodies hit (chargeShot27Fire.hits), kills, stand-ins killed.
  - **parry [--tries n]**: face the nearest spitter; when one of its globs is inside 0.9 x orb.reflectRange of the eye, punch. Earlier waves
    with no spitter are killed outright after 2 s to reach one (the count is printed). Prints parried (acid.reflects), the return's hits and
    bodies, the spitter's death and its cause, and parry36's return burst (hits: a second victim).
  - **skip [--refuse launch|pierce|parry|ride] [--seeds n] [--cap s]**: the room cleared by n46flow's pilot (Walk38Pilot + the lava rule, not
    invulnerable) twice per seed, the verb allowed and the verb refused. Refusals: launch = no punch while dashing / sliding / riding or in
    launch36's grace; pierce = fire held only on its press step (taps); parry = no punch with a glob inside 1.5 x reflectRange; ride = jump
    held only on its press step in the air. Default by room: a2 / a8 pierce, a4 parry, a5 / b1 launch, a3 ride. GREEN = with the verb
    refused, fewer clears, a median 25 % slower, or more deaths. It prints a NOTE when the pilot never used the verb even when allowed (then
    it measures the room, not the verb).
- **Tools/night/n65-verbs.sh** `<verb> <level> [rooms] [extra args]`: one verb over the six slots.
- **The red** (n52-foundry, seed 1, the pose each room's spawn: no n52 row has line65):
  | room | launch (`verb-launch-red.txt`) | pierce (`verb-pierce-red.txt`) | parry (`verb-parry-red.txt`) | skip, 5 seeds (`verb-skip-red.txt`) |
  |---|---|---|---|---|
  | a2 | RED: stand-ins only | **GREEN: 3 bodies (the 3 stand-ins)** | RED: no spitter | RED (pierce refused: median 11.1 s vs 14.9 s allowed) |
  | a3 | GREEN: 8.7 m, a wall burst | RED: 1 body | RED: no spitter | RED (ride refused: 24.0 s, 1 death vs 34.0 s, 3 deaths allowed; 23 rides allowed) |
  | a4 | GREEN: 3.6 m, a wall burst | RED: 1 body | **GREEN: parried, the spitter killed by its own glob** | RED (the pilot never parries: NOTE) |
  | a5 | **GREEN: a body burst, 2 kills** | GREEN: 2 bodies | GREEN: parried, a second victim | RED (the pilot never launches: NOTE) |
  | b1 | **GREEN: a body burst, 3 kills** | RED: 1 body | GREEN: parried, a second victim (wave 1 killed outright to reach it) | RED (never launches: NOTE) |
  | a8 | GREEN: a burst, 2 kills | **GREEN: 2 bodies** | RED: no spitter | RED (pierce refused: 4.8 s vs 6.9 s) |
  - The slide launch (`verb-launch-slide-red.txt`): a5 and b1 GREEN.
  - Planted: launch in a1, which has no enemy, RED "no target" (`verb-planted-no-target.txt`); a pose inside a2's deck refused
    (`verb-planted-pose-in-solid.txt`); a planted a2 line65 through the assembler: pose line65[0], aim along its first segment, 3 bodies pierced
    (`verb-planted-line65-pierce-a2.txt`, plant reverted).
  - **What the skip says about the red:** in every old room n46flow's pilot clears as well, or faster, with the room's verb refused. The old
    rooms do not force their verbs on this pilot. Where the pilot never uses the verb (a4 parry; a5 / b1 launch), the skip measures the room,
    not the verb. A rebuilt room passes when its skip turns GREEN against this red.
  - Checks: dn-check 0 (`dn-check-verb.txt`), gates GREEN (`gates-verb.txt`).
  - Not checked: other seeds of the probes (seed 1 only); whether a person would use the verb (the pilot is a guard, never the verdict).

## The verb-aware pilot (due 2:40; the lead's ask at 1:35): verb65, n46flow --verbs65, n65verb skip --verbs65
- **Harness only; no Assets code changed.** The pilot is Tools/SimHarness/Harness.N65.VerbPilot.cs, a wrapper round n46flow's pilot
  (F1Input). It runs only with `--verbs65` and only in a room whose row carries `verb65`; everywhere else the inner pilot's frames pass
  through untouched. n46flow's edits (Harness.N46.Flow.cs) are no-ops without the flags: the flags parsed, the level JSON passed through
  V65Planted (unchanged with no plant), the pilot through V65Wrap (itself without --verbs65), and the VERBS65 block printed only with --verbs65.
- `verb65` added to gen-n65-foundry.cjs's KNOWN list, with its reader named: launch | launch+slide | pierce | ride | parry | slide.
- **Triggers and episodes** (the file's head): launch = a husk within 5.5 m and 50 deg ahead: dash, punch in range at speed; pierce = 2 or
  more bodies within 2 m of one line from the eye through a body in clear sight: charge to full, release; ride = a ride face within 1.6 m in
  the air (2.5 m on the ground: a jump first): hold jump, steer along it to its far end; parry = an unreflected glob inside 0.9 x
  reflectRange: look at it, punch; slide = a 1.0-1.35 m tunnel within 2.5 m ahead: slide. Any other verb65 is refused and counted.
- Two lessons on the way, both measured:
  - the punch was dropped by NIGHT30's rule (no melee while the gun's action runs) because the autopilot had just fired: 6 punches,
    0 launches. Now the episode holds fire, queues no switch, and punches when `rules30.MeleeBlocked` is false.
  - the counters read the punch's and the release's effects a few steps later: they count inside a 0.25 s window.
- `--plant-verb65 a2:pierce,...` plants rows in memory (verb65 plus bot43 false: the generic path, as a rebuilt row) for n46flow and skip.
- `n65verb skip --verbs65`: the allowed run is the verb-aware pilot, the refused run the plain pilot with the verb refused. Both print clear,
  deaths, the verb's uses and STYLE (the share of fight steps above DULL, the top rank).
- **The red with verb65 planted per the cards** (a2 pierce, a3 ride, a4 parry, a5 / b1 launch, a8 pierce; bot43 false):
  - Without the flags, n46flow n52-foundry 10 seeds is EQUAL to Logs/n65/flow-base.txt (`flow-cmp-noflag-verbs.txt`).
  - `n46flow --verbs65` 10 seeds (`flow-verbs65-red.txt`): the trigger held a2 53.2 %, a3 14.6 %, a4 0.3 %, a5 7.1 %, b1 6.4 %, a8 12.9 % of
    fight steps. Performed: a2 37 charged shots, a3 25 rides, a4 10 parries, a5 12 launches (27 punches), b1 12 (39), a8 16 charged shots.
  - The level against the base: clear 116.6 s vs 126.7, deaths 7 vs 13, stall 28.2 % vs 30.8, STYLE DULL 19.7 % vs 43.7, rooms reaching
    SAVAGE 4/6 vs 1/6. Part of this is bot43 false (a8's id script off), not the verbs.
  - `n65verb skip --verbs65` 5 seeds (`verb-skip-verbs65-red.txt`):
    - **b1 GREEN**: launch, median 28.1 s vs 36.7 s refused, STYLE above DULL 52.9 vs 33.0 %;
    - a5 RED but faster (22.6 vs 27.5 s, under the 25 % bar), STYLE 58.6 vs 18.4 %;
    - a2 / a8 RED: pierce no faster (12.2 vs 11.1 s; 19.8 vs 20.2 s);
    - a3 RED: the ride costs time and deaths (32.0 s, 5 deaths vs 24.0 s, 1);
    - a4 RED: parry 10.6 vs 11.8 s, only 4 parries in 5 seeds.
  - Planted (`verb-planted-verbs65.txt`): verb65 "grapple" in a3, REFUSED (7040 steps, the inner pilot ran); verb65 "slide" in a4, which has
    no tunnel: trigger held 0 of 4020 fight steps.
  - Checks: gen-n65-foundry --check all PASS, dn-check 0, gates GREEN (`check-verbs65.txt`, `dn-check-verbs65.txt`, `gates-verbs65.txt`).
  - Not checked: the slide and launch+slide episodes in a room that has the geometry (no red room has a slide tunnel); seeds beyond 10 / 5.

## air65 (due 3:00; the measure for round 1's design finding, Logs/n65/r1/review-ld.md)
- **Tools/night/n65-air.cjs** `<plan.json | level.json> <roomId> [--json]`, and `--selftest`; **Tools/night/n65-air-rooms.sh** `<plan> [rooms]`
  runs the six slots and prints a table. Static, node only.
- **Air pieces:**
  - rides: a box with ride + ride36, held 2.5 m over its base;
  - decks: deck / slab tops at 1 m or more;
  - ledges: hull tops at 1 m or more, under the room's height less 1.8 m;
  - every top must be at least the capsule's 0.7 m across and less than 80 % covered by a box within 1.8 m above it;
  - pads: a pad on the floor (under 0.5 m) can start a path but is never a target.
- **Moves:**
  - from a deck: jump, jump + double, dash (6 m flat), dash-jump, dash-jump + double;
  - from a ride: jump-off (ride39's), jump-off + double, wall jump (wallfling42's) with and without a double, dash;
  - a pad: its own flight to the piece that holds its target.
- **How an edge is tested:** the flight is aimed straight at the target's nearest part and stepped at 120 Hz. It must clear a deck's edge by
  the capsule radius, or arrive inside a ride's band. The body points (feet + 0.3 m, feet + 1.5 m) are tested every 0.25 m against the room's
  boxes (source and target excepted).
- **Numbers (each cited in the file's head):**
  - PortExport/config.json: jump v0 8.33, g 23.15, fall x1.35, double v0 7.50, dash 24 m/s x 0.25 s, dash-jump v0 6.67 at g x0.5 for 0.5 s
    keeping 24 m/s (Mover.cs:1080; air accel never slows a body, MovementMath.Accelerate), run-up 8, capsule 0.35;
  - Design/unity-config.json: wall jump out 5 / up 8.33 (wallfling42 ON), ride 9.2 m/s and jump-off out 8 + 1 / up 7.3 (ride39 ON over
    wallride35).
- **Printed:** the pieces; every edge with its easiest move, margin and the other moves that work; the longest floor-free chain; the dead ends
  (pieces no move leaves); the same without the dash-jump (its flight reaches most of a 32 m room). And the review's own question: the most
  FLIGHT pieces (rides + pads) on one floor-free path.
- **Planted** (`air-selftest.txt`): two ride walls 4.8 m apart give a wall-jump edge both ways (the easiest move is ride39's jump-off,
  margin 2.46 m; the wall jump + double has margin 3.24 m); 9.6 m apart give no wall-jump edge (a jump-off + double still crosses). PLANTED GREEN.
- **The red** (n52's plan, `air-red.txt`) against the latest rooms (port/stage3 at 02d789cd, assembled with --slots=latest in a scratch tree,
  `air-latest-02d789cd.txt`). Flights handed on = the most rides + pads on one floor-free path:
  | room | red: pieces / chain / dead ends / flights handed on | latest: pieces / chain / dead ends / flights handed on |
  |---|---|---|
  | a2 | 1 / 1 / 1 (D1) / 0 of 0 | 3 / 3 / 0 / 0 of 0 |
  | a3 | 9 / 9 / 0 / **3 of 3** | 4 / 4 / 0 / **1 of 1** |
  | a4 | 1 / 1 / 1 (D1) / 0 of 0 | 2 / 2 / 0 / 0 of 0 |
  | a5 | 7 / 7 / 0 / **4 of 4** (P5 -> D2 -> R4 -> D3 -> P6 -> D1 -> P7) | 7 / 6 / 0 / **2 of 2** (a floor pad, then the rail) |
  | b1 | 6 / 5 / 0 / 3 of 4 | 8 / 7 / 0 / 2 of 3 (a floor pad, then the rail) |
  | a8 | 2 / 2 / 0 / 2 of 2 (R1 -> R2) | 3 / 2 / 1 (D2) / 1 of 2 |
  - **Reading:** the review's finding stands on this measure. The rebuilt rooms carry fewer flight pieces and hand fewer on: one ride or pad
    per path, the second piece being a floor pad that opens it. n52's a3 / a5 / b1 chained 3-4 flight pieces. The rebuilds' chains are deck
    -> ledge -> deck hops (wall tops, posts), not flight lines.
  - Not checked: real flights (no air control, no apex hang, straight lines in plan); whether a person would take a hand-off.

## The skip test's exercise rule (due 4:40; the S GATE critic, Logs/n65/verdict-a4.md; branch n65-T2 from fd9c565b)
- **The flaw:** in a4 every refused run showed "presses refused 0". Seed 1 made 0 parries and still won by 11.9 s, so the GREEN did not show
  the parry was needed.
- **The fix** (Tools/SimHarness/Harness.N65.Verb.cs, V65Skip):
  - every seed line prints the presses refused and the verb's uses in both runs;
  - a seed EXERCISES the refusal when its refused run refused a press or its allowed run used the verb. A seed that did neither prints
    "WARN not exercised" and does not count toward GREEN;
  - the verdict is computed on the exercised seeds only and prints "seeds exercising the refusal k/n (need 3)". It is **UNSURE when fewer
    than 60 % (3 of 5)** were exercised (exit 3), and warns "no press was refused on any seed" when the refusal never fired;
  - slides are now counted too (SlideStart in the room); n65-verbs.sh tallies UNSURE.
- **Re-run on HEAD's latest** (fd9c565b, assembled with --slots=latest plus levelgen and the finish in a scratch folder, deleted after;
  `verb-skip-verbs65-fd9c565b.txt`):
  - **a5 GREEN**: 4/5 exercised (seed 1 not), median 22.7 vs 33.2 s refused, STYLE above DULL 65.9 vs 54.6 %;
  - **b1 GREEN**: 4/5 (seed 2 not), 31.2 vs 42.3 s, STYLE 80.6 vs 39.1 %;
  - **a4 RED**: 4/5 counted through the verb-aware run's 9 parries, but 0 presses refused on any seed (the plain pilot never punches near a
    glob). The medians are 25.6 vs 25.2 s: the room clears as well without the parry. The verdict line WARNs that the refusal never fired.
- Checks: dn-check 0 (`dn-check-skipfix.txt`), gates GREEN (`gates-skipfix.txt`).

## Three GATE-critic fixes (hard stop 5:15; branch n65-T3 from 4332130b)
1. **iso65** (Logs/n65/verdict-a5-r2.md: a5's slide line was invisible to the blind spotter). A floor strip (PropsView29's cyan lane line),
   or a prop that says cyan, is now drawn in the cyan role, not beige dressing. A cyan light (a ride wall's line, a pool) is drawn as its
   own strip or panel, not a 3 px dot. New `--plant-strip x0,z0,x1,z1`. Proof: `iso-planted-strip-a2.png`, where the planted strip and
   a2's cyan exit pool both read cyan.
2. **The launch pilot**: `--wave <n>` plays the room up to its wave n (the earlier waves killed outright after 2 s; the campaign spawns
   wave n) and takes that wave's nearest husk; `--target x,y,z` takes the husk nearest that point.
   - a5 on HEAD's latest (4332130b, a scratch assembly + level, deleted after; `verb-launch-a5-wave2-slide-4332130b.txt`), run with
     `--wave 2 --slide --pose 0,0,-114,14` (line65's south-mouth point, facing along its first segment).
   - Result: wave 2 reached (7 bodies of wave 1 killed outright); the slide began at step 190 and the punch went out at step 353 in Slide;
     **launch36 0, RED: no body landed in lava** (the target was at (8.1, 0, -122.7), alive).
3. **The GATE pack**: open-findings.md reads every `Logs/n65/r*/review-*.md` (the old `/^r\d+$/` skipped r3b's).
- Checks: gates GREEN (`gates-T3.txt`), dn-check 0 (`dn-check-T3.txt`). Only harness / node code changed; no Assets code.

## The GATE evidence pack (due 3:15): n65-gate-pack.sh
- `bash Tools/night/n65-gate-pack.sh <id> <round-tag>` (Tools/night/n65-gate-pack.cjs) writes Logs/n65/gate/<id>/ and the spot key
  Logs/n65/gate/keys/<id>-spot-key.txt (outside the pack). Options for running elsewhere: --logs, --plan, --level, --out, --key, --sha,
  --blind, --no-skip.
- **The sha** is the round's assembly sha (the header of <round>/gen-latest.txt), else HEAD.
- **The round's plan** is assembled AT THAT SHA in a scratch tree: the assembler, n52's plan and the six room files as git shows them,
  run with --slots=latest. When the round's row has verb65, levelgen + --finish run inside the scratch tree as well (with the sha's
  PortExport/config.json and Design/unity-config.json beside it), and the --verbs65 skip runs on that level. The scratch tree is deleted
  after, so Design/levels and Reports are never written.
- **The pack:**
  - table.md: the round's row, which carries the red in brackets;
  - verbs.md: the round's verbs-*.txt lines naming the room (verbs.md only as a fallback) against the red's verb-*-red.txt, plus
    verbs65-skip.txt when the row has verb65;
  - air.md: n65-air on both plans;
  - spot-A.png / spot-B.png: both drawn fresh with n65-iso --no-card-marks (new: n52's rooms carry no line65 / frames65, so those marks alone
    would name the rebuild), through n65-blind.py; A / B from sha256("<id>:<sha>")'s first hex digit;
  - card.md at the sha; room-diff.txt (git diff --no-index of rooms-n52 against rooms at the sha);
  - checks.txt: room-check, the flow rows / SUMMARY / STYLE, pads and ride, the round against the red;
  - open-findings.md: the room's BLOCKING lines with their continuations from every r*/review-*.md and cardgate*.md, and every "<id>: " line;
  - README.md: each file with its command and the sha.
- **Proved on a2:**
  - r1 (sha 0d163c65; `gate-proof-a2-r1/`, 10 files): no verb65 on a2's row at that sha, so no skip run.
  - r2 (sha d602157a; `gate-proof-a2-r2/`, 11 files): verb65 pierce, so the skip ran on the scratch level: RED, median 20.0 vs 20.7 s.
  - The keys are in `gate-proof-keys/`: r1's sha256("a2:0d163c65") starts f (odd), so A = the red; r2's sha256("a2:d602157a") starts 2
    (even), so A = the round. The coin falls both ways.

### Findings
- On n52-foundry, only **a8, b2 and a7** have id scripts that run (Walk43Foundry, BaseId n43-foundry). Walk36's a3 / a5 scripts run only on
  n36 levels (`Level36` = the id starts with "n36"), and Walk38Foundry's b2 chain only on n38-foundry. Measured by n65bot: a3 ran 0 id-script
  steps as filed.
- The lead's A1 placeholder (535cd738:Design/levels/n65-foundry.json) equals this tree's generated all-old n65-foundry.json apart from
  name / _why / generatedBy (its generatedBy is still n52's).
