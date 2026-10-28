# NIGHT11 log (Unity stage 3, night 3: the run's shape)

Times from `date`. Brief: `NIGHT11.md`. Branch `port/stage3` from `u3-n10` (plus the research commit `234286f`). First
`date`: Sat Oct 3 2026, 2:07:44 PM.

**Run length (GG's goal message, 2:07 PM): "Hard stop after 8 hours of work."** Hard stop: 10:07 PM. NIGHT11 section
6's budgets add up to 11.75 h; they are scaled to the 8 h (x0.65) and the close starts by 9:00 PM whatever is left.

| Item | NIGHT11 budget | Tonight (8 h) |
|---|---|---|
| U3-24.1 verify | 15 min | 10 min |
| U3-24.2 the allocation profile | 45 min | 30 min |
| U3-24.3 special rounds | 75 min | 50 min |
| U3-24.4 THE PROTOCOL | 90 min | 60 min |
| U3-24.5 traps | 90 min | 60 min |
| U3-24.6 the power switch | 45 min | 30 min |
| U3-24.7 exfil | 90 min | 60 min |
| U3-24.8 the grade | 45 min | 30 min |
| U3-24.9 ULTRA HEALTH (LAB) | 60 min | 40 min |
| U3-24.10 close | 60 min | 50 min |

Decisions for tonight (GG's goal message, 2:07 PM; HANDOFF section 5 has no other answers):
- **D-18 stays off:** ULTRA HEALTH ships as a LAB toggle, off (U3-24.9 as written, not a design flag ON).
- **The power switch, the swarm drop and exfil flags ON** (as NIGHT11 section 0.4 proposed).
- **D-23, D-24:** no answer; the NIGHT10 defaults stay (ULTRAKILL flags ON, the box's numbers as shipped).
- **Caps (the goal message):** at most 5 Play checks, 4 captures, 2 Player builds (the Development profile and the
  close).

Org-rule note: the material for this work is this repo and the web reference repo (read tonight). As on NIGHT9 and
NIGHT10, no Drive / Gmail / Calendar / Linear content is relevant to a game port, and none was read.

## U3-24.1 verify on arrival (2:07-2:12 PM)

RECEIVED (HANDOFF section 1, every row):
1. HEAD `234286f` (the research commit "docs: NIGHT11 researched ..."), on `port/stage3`; `git tag --points-at HEAD`
   prints nothing; `u3-n10` is `9d8888e`, and `git log u3-n10..HEAD` is that one commit, which HANDOFF's update note
   announces. As expected.
2. `git status --short`: ` M ProjectSettings/UnityConnectSettings.asset`, `?? NIGHT10.md`, `?? NIGHT11.md`,
   `?? NIGHT7.md`, `?? NIGHT8.md`, `?? NIGHT9.md`. As expected.
3. `unity-loop.sh status` (2:08 PM): `"status": "ready"`, `"playMode": "stopped"`. As expected (not playing this time).
4. `unity-loop.sh tests` (14:09:00-14:09:41): `tests: 368 / 368 passed, 0 failed, 0 skipped`, `replays: 22 / 22`,
   `golden tests: 86 / 86`. As expected.
5. `dotnet run --project Tools/CoreHarness -c Release` (14:09:57-14:10:06): `golden total: 30 vector sets (30529
   records), 22 replays (22 exact); 0 failing, 0 mismatches`. As expected.
6. `Builds/Stage1/SKYGRIND.exe -skygrindSmoke -logFile Logs/player/smoke-arrival-n11.log` (14:10:21-14:11:08): the
   Player quit by itself after about 34 s; `[smoke] 32.1s DONE: all steps passed`; 0 lines matching "exception". As
   expected.
7. `Builds/Stage1/SKYGRIND.exe -skygrindGolden -logFile Logs/player/golden-arrival-n11.log` (14:11:27-14:11:43):
   `[golden] golden total: 30 vector sets (30529 records), 22 replays (22 exact); 0 failing, 0 mismatches`,
   `[golden] DONE: 0 mismatches in 4.9 s`. As expected.
8. Power plan `Balanced` (381b4222-...), on AC (Win32_Battery BatteryStatus 2, 100%). Recorded.
9. Idle 31 s at 2:11 PM: GG had just started the run. Daytime: Play checks get batched, `editor_focus` before each.

GG's answers (HANDOFF section 5): D-18 off (the goal message); D-23 and D-24 unanswered (defaults kept).

Status U3-24.1: passing.

## How the night is run (2:20 PM)

- **Shared types first** (`6311033`, 2:22 PM): SurvivalMap parses `holdSpots`, `relaySpots`, `secretRelay`,
  `trapSpots`; SurvivalRun gains survival.js's `hooks.holdRound` and `hooks.step`; RunStats gains the results' fields;
  `Core/RunShape.cs` holds the web's `banner {text}` payload. No behaviour change (harness 0 mismatches; 368 / 22 / 86).
  The worktree agents branch from it, so they compile against one contract.
- **Four background agents, disjoint files** (memory note `night-parallel-agents`), none driving the Editor or git on
  `port/stage3`: the p20 recorder (web repo, `port-export-p20/` only: roundType, gradeOf / scoreLine vectors, an
  objectives session, a traps session only if it runs without a DOM); Core A in a worktree (roundType, Director
  AddSquads / fullPace, objectives.js, protocol.js, their tests, the ring and relay views); Core B in a worktree
  (traps.js, runMoments.js, their tests, the trap views); Core C in a worktree (the power switch, the swarm drop, exfil,
  ULTRA HEALTH in the LAB, the design config with collider-cast spots, their tests and views).
- **I** run U3-24.2, write the golden checkers, wire SurvivalGame, the HUD and the smoke steps, and run every gate.

## U3-24.2 the allocation profile (2:30-3:12 PM)

- **The Development Player** (the night's first of two builds): `PlayerChecks` gains `-skygrindProfile <frames>
  <file.raw>` (the Profiler's binary log for that many frames from the bench's middle). Build 14:35:25-14:37:02
  (`build --target StandaloneWindows64 --outputPath Builds/Dev/SKYGRIND.exe --scenes [Survival, Arena] --options
  [Development]`): `Succeeded`, 0 errors, 519 warnings. `git diff --stat`: `Mobile_RPAsset.asset` rewritten (D-09),
  restored.
- **The bench on it** (`bash Tools/survival-bench.sh 60 Builds/Dev/SKYGRIND.exe Logs/player/bench-dev.log`, 14:38:31-
  14:40:09, `power seen: 8 Balanced AC=True`): `median 2.50 ms, p95 3.84 ms, worst 1% 5.08 ms, mean fps 381`;
  `collections 18 in 22797 frames; GC Used Memory rises 120152.0 KB (valid); GC Allocated In Frame 86023.2 KB total,
  3864 B / frame`.
- **The profiler window** (30 s bench, 300 frames logged, 14:41:03-14:41:52, `Logs/player/profile.raw` 40 MB), read in
  the Editor (`ProfilerDriver.LoadProfile` + `HierarchyFrameDataView`, GC.Alloc samples by their nearest `SKYGRIND.*`
  marker): `301 frames, 106 SimStep samples; GC.Alloc total 1092.3 KB, 3716 B / frame`. By marker: outside every
  SKYGRIND marker 3660 B / frame, of which `SurvivalGame.Update()` itself 964.5 KB in 15600 allocs (the render half has
  no markers) and IMGUI `GUIUtility.BeginGUI` 107.8 KB in 1200 allocs; `SKYGRIND.Loadout` 16.4 KB in 424 allocs (56 B
  / frame, 4 allocs per sim step); `SKYGRIND.Assist` 0.1 KB once. Every other sim marker: 0.
- **Render markers added** (`SKYGRIND.R.*` around each render system in Update, and `R.Input`), then **Play check 1**
  (14:47-14:49, the bench's setup in the Editor's Play mode, 320 frames recorded): `GC.Alloc 1179.7 KB, 3775 B /
  frame`: `SKYGRIND.R.Props: 801.3 KB in 11520 allocs, 2564 B / frame, 36.0 allocs / frame`, `SKYGRIND.R.Ui: 206.7 KB
  in 4810 allocs, 661 B / frame, 15.0 allocs / frame`, `(outside the game's markers): 120.1 KB in 1351 allocs, 384 B /
  frame`, `SKYGRIND.Loadout: 30.1 KB in 780 allocs, 96 B / frame`, `SKYGRIND.R.Input: 21.3 KB in 320 allocs, 68 B /
  frame, 1.0 allocs / frame`, `SKYGRIND.Husks: 0.2 KB in 4 allocs`.
- **The allocators and the fixes:**
  1. **WallBuys.Render (R.Props, 2564 B / frame, 36 allocs):** four interpolated config keys and a new sign string for
     every ported ARMORY gun every frame. NIGHT10 took the wall from 1 ported gun to 4 (STITCHER, COMET, LONGWAVE), so
     this is the NIGHT10 rise. Fixed: both sign texts and the spin / bob numbers made once per gun, the text set only
     when the owned state changes. (A Game view: no EditMode line can hold it; the close's bench re-measures it.)
  2. **Loadout.StepAds (SKYGRIND.Loadout, 4 allocs a step with the KESTREL in hand):** `cfg.Has("ads.weapons.
     revolver")` every step, and GameConfig.Has splits the path (an array and 3 strings). Fixed with the loadout's own
     per-weapon cache (`ZoomWeapon`). **New ZeroAllocTests line** `LoadoutStep_EachGunInHand_Idle_AllocatesNothing`
     (idle Loadout.Step with each ported gun in hand on the 30-enemy crowd): red first, `revolver 1200, shotgun 0,
     nailgun 0, rocket 0, rail 0, gauntlet 0`, green after the fix.
  3. **IMGUI (384 B / frame, 4 allocs):** SurvivalGame's `OnGUI` (F3's overlay) made IMGUI run every frame even with
     the overlay off. Fixed: the overlay is its own `DebugOverlay` component, enabled only while F3 shows it.
  4. **Not fixed, to HANDOFF risks with these numbers:** `SKYGRIND.R.Ui` 661 B / frame (15 allocs; Ui's render, NIGHT8-9
     code; no interpolation found in the partials on a read), `SKYGRIND.R.Input` 68 B / frame (1 alloc).
- **A test that flaked after a domain reload:** the first filtered ZeroAllocTests run after a recompile reported 6
  samples in `ProjectilesStep_InFlight_AllocatesNothing` (it passed on two reruns and in every full run): its 2-step
  warm-up left the rocket blast's first JIT compile inside the counted window. Its warm-up is now a whole first volley;
  a fresh-domain run after that: `tests: 5 / 5 passed`.
- 15:10:04-15:11:01 `tests: 369 / 369 passed, 0 failed`, `replays: 22 / 22`, `golden tests: 86 / 86`; harness 0
  mismatches.

Status U3-24.2: passing (the re-measure is the close's Release bench).

## The p20 oracle (the recorder agent, 2:32-3:24 PM)

- `skygrind/port-export-p20/` (untracked; p19 plus `scripts/runShape.mjs`): `all.mjs` writes `G  vectors/roundType.json:
  503 records`, `G  vectors/runMoments.json: 444 records`, `G  vectors/objectives.json: 4 records`, `G  vectors/traps.json:
  1 records` (NIGHT11); the 57 files p19 also writes are byte-identical (`cmp: 57 files compared, 0 differ`).
  `verify.mjs`: `vectors/roundType.json (NIGHT11): 503 calls reproduced exactly`, `vectors/runMoments.json (NIGHT11): 444
  calls reproduced exactly (gradeOf 294, scoreLine 150)`, `sessions vectors/objectives.json ...: 4 of 4 records (340 ops)
  reproduced exactly`, `sessions vectors/traps.json ...: 1 of 1 records (100 ops) reproduced exactly`, `total: 0
  mismatches` (a one-value tamper of three files gave `total: 6 mismatches`). The traps session ran in node with no DOM
  shim (blockyKit's screenPanel makes no canvas without `document`), so traps got vectors too.
- The recordings fixed facts the ports had to match: objectives.js converts seconds with Math.round (not stepsFor), its
  clock counts every step even inactive, `inside` is never reset, the ring stays until endRound; traps.js uses stepsFor,
  `available()` asks isOpen first, a boss gets no kill check, the fan's pending kills are never cleared. I passed them to
  the two porting agents mid-task.
- `Golden/GoldenRunShape.cs` (mine) replays the four files: every call each module makes (zoneAt, isOpen, setBoost,
  grant, damage, launch, arcs, bursts, the events, in order), hud / holding / debugState / the step count, the traps'
  items / stats / enemies. Two findings on the way: (1) my checker first read an absent key as 0 (`bossFrom` deleted in a
  variant): JS reads `undefined`; NaN now stands for it; (2) then the vectors caught a NIGHT7-era port gap:
  directorMath.js `isBossRound` reads `bossFrom ?? bossEvery` and `isSwarmRound` `swarmFrom ?? swarmEvery`,
  `swarmOffset ?? 0`; the port had dropped the fallbacks (the real config has every key, so no game difference). Ported.
  The port-only `assistPay` (NIGHT9) on StyleBonusEvent is left out of the traps comparison at its default 1.
- 15:49 harness `golden PASS roundType: 503 records, 0 mismatches`, `golden PASS objectives: 4 records, 0 mismatches`
  (commit `d1f7ff7`); 15:57 `golden PASS runMoments: 444 records`, `golden PASS traps: 1 records, 0 mismatches`, `golden
  total: 34 vector sets (31481 records), 22 replays (22 exact); 0 failing, 0 mismatches` (commit `29d495f`).

## U3-24.3 special rounds (2:20-4:23 PM)

- **roundType + objectives.js** (Core agent A, its branch's `87d6c3f` and `00a69df` cherry-picked as `9b41520` and
  `c25a3e4`): `DirectorMath.RoundType`, `Core/Objectives.cs` line for line (minus the ring mesh; deps as the calls the
  web makes, for the session checker), ObjectivesTests 9 (night6.test.js :208-235 ported: the schedule, hold spots
  clear of every box; a HOLD round can't clear until held with a real SurvivalRun; GOLD RUSH's chain; no open spot;
  inactive / disabled; the calls in the web's order; Step allocates nothing). In Unity at 15:36: `tests: 8 / 8 passed`
  (the first commit's 8).
- **Wiring** (`24b5b55`): main.js survivalHooks: `objectives.Begin` on the director's `roundPlan` (after StartWave,
  before `roundBegin`, as hooks.onRoundBegin runs before survival emits roundBegin), `run.holdRound`, `run.stepHook`,
  `Load` at a run's start. The HUD line's objective part (topText) and the `banner` event shown (`acecdaa`). Port
  difference (HUD): hud.js shows the last banner only, so on the web a round's own banners (HOLD ROUND, THE PROTOCOL
  WAKES) were overwritten by main.js's ROUND N in the same frame; the port queues them behind it.
- **The swarm round's banner and drop** (Core agent C, `422e0bc`; design.zombies.swarmDrop ON, new design): the banner
  "SWARM ROUND · THE LAST KILL DROPS A POWER-UP", `swarmRound` / `swarmDrop` (port-only), one power-up from the run's
  stream at the last kill's position on the swarm round's clear. SwarmDropTests 5 (on, off, a normal round, no player
  kill, the run's stream).
- Smoke: `PASS round 6: the objective hold, ...`, `PASS the HOLD ring held at (36, 0): 3600 / 3600 steps; objectiveDone
  300` (Play check 4 and the Player).

Status U3-24.3: passing.

## U3-24.4 THE PROTOCOL (Core agent A, cherry-picked 3:58 PM as `4728ba2`)

- `Core/Protocol.cs` (protocol.js line for line: the relays as Buyables items, awake at round 6, the next relay's
  guide, the SURGE through `Director.AddSquads`, its roundClear forges every owned gun free (`Forge.Give(id, 0)`) and
  grants 2500, the secret relay, the HUD fields, the results' Summary; finishes not ported: onComplete / onSecret return
  false, so the banners take their no-finish text). `Director.AddSquads` (director.js addSquads) with the port's rule
  for kinds with no port: each becomes a husk (the SURGE's 12 enemies arrive as husks and spitters; logged in
  Director.cs's header). `Director.fullPace` (exfil). Buyables gains buyables.js's `owned` and `remove`. Views:
  `ObjectiveRingView`, `RelayViews` (`Game/RunShapeViews.cs`). ProtocolTests 9 (night6.test.js :93-118's relays on
  their showpieces; the quest with a real Director; the surge queued in a break / live in a fight; the secret relay;
  `powered`; AddSquads; no allocation; fullPace).
- Smoke: `PASS the four relays lit (4 / 4): events [relayLit skybridge, relayLit chimney, relayLit ring, relayLit
  pinnacle, protocolSurge], the surge live, queued squads 9`, `PASS THE PROTOCOL complete in 30 s: every owned gun forged
  True ([revolver, gauntlet]), points 160 -> 8510`.

Status U3-24.4: passing.

## U3-24.5 traps (Core agent B, cherry-picked 3:55 PM as `71373c8`)

- `Core/Traps.cs` (traps.js line for line: the four, their switches at map.trapSpots, timing in stepsFor, regulars take
  traps.kill, elites spec.elite, bosses spec.boss (none ported), rivals skipped, TRAPPED through the existing
  StyleBonusEvent (the style meter pays it: StyleMeter.cs :65), the pylon's stable nearest-first sort on a reused list;
  `powered` for design.power). `Game/TrapViews.cs` (the bodies as buildBody, the switch cabinets with a TMP label set on
  change, the pylon's zaps as a LineRenderer zig-zag: fx/arcs.js not ported, logged). UnityTuning: TrapLabelFontSize,
  TrapZapPool (`db4c418`). TrapsTests 9 (each trap's kill and cooldown, an elite, the fan's pending kill, the pylon,
  the switch for sale only in an open zone and while powered, no allocation). The p20 session: 0 mismatches.
- Smoke: `PASS SHOCK PLATE bought (850 -> 160 incl. the kill's points), the husk on it killed in 0.2 s: events [powerOn,
  trapKill husk]`.

Status U3-24.5: passing.

## U3-24.6 the power switch (Core agent C, `de3c622`; design.power ON)

- `Core/Power.cs`: a free hold-E Buyables item, `powerOn` (port-only) and the banner "POWER ON · THE TRAPS AND THE
  RELAYS WAKE"; the game points the traps' and the relays' `powered` hooks at it. The spot (agent C's collider-cast
  probe through the harness, in design.power's `_why`): (-42, 9, 4.5) on HEIGHTS' west skybridge, two gates from LANDING
  (SLAB GARDENS is one); flat over 1 m, a 1 m capsule touches 0 triangles and 0 boxes, 16 / 16 up-rays open, nav node
  6115 reachable only with pad 2's launch. **Not against a wall:** no HEIGHTS spot was both wall-backed and reachable
  (436 wall-backed spots sit on ledges with no nav route; 2.5 m steps over jumpUp 2.2 m). PowerTests 4.
- Smoke: `PASS the power switch thrown at (-42, 9, 4.5) in heights: on True, free (points 500 -> 500); the plate for
  sale before False, after True`; capture `Logs/shots/n11-power-on.png` (the banner over the cabinet).

Status U3-24.6: passing.

## U3-24.7 exfil (Core agent C, `77340b5`, `c99397d`; design.exfil ON)

- `Core/Exfil.cs`: the radio (free, hold E) at the spawn (-4, 0, 42.9) in the breaks before rounds 11, 16, 21 ...
  (BO6), `exfilCall`; the round is the exfil: a site by the round (`sites[((R - 11) / 5) % 3]`, or the next open one:
  SLAB GARDENS (43.5, 0, -12), THE GRINDER (-10.5, 0, -8), a HEIGHTS spire (-40, 24, -40) that no nav route reaches),
  REACH within 4 m in 90 s, then HORDE: 35 kills (+10 per later exfil: 35, 45, 55) in 85 s with the director at full
  pace and the queue topped up from the round's plan on the exfil's own stream; `exfilStart`, `exfilReach`, `exfilDone`,
  `exfilFail` (port-only). The game ends the run on its outcome: EXFIL straight to RESULTS (a win), EXFIL FAILED through
  DOWN; `Records.Run.exfil` (written only when true, so an old record is the web's bytes). ExfilTests 12.
- Smoke: `PASS the exfil radio at the spawn called in round 11's break (could True): Called`, `PASS EXFIL at round 11
  (site 0 slabs, 35 / 35 kills, 0 squads topped up): Results, 'EXFIL', grade C, the PROTOCOL 4 / 4 complete True; line
  'SKYGRIND survival: round 11, 23057 pts, grade C, best SAVAGE, 0:53, 2 forged, PROTOCOL'`; capture
  `Logs/shots/n11-results-exfil.png`.

Status U3-24.7: passing.

## U3-24.8 the run's grade (Core agent B, `fd94a26`)

- `Core/RunMoments.cs`: `GradeOf`, `ScoreLine` (JS template semantics; `JsNumber`, V8's String(number), checked by the
  agent on 76,935 doubles against node) and the moments tracker (the multi-kill window on sim time: the web reads
  performance.now(), a wall clock; logged). RunMomentsTests 9 (runModes.test.js :6-20 ported, the tracker, no
  allocation). The p20 vectors: 444 calls, 0 mismatches. RESULTS: the grade (results.gradeColors), BEST MOMENTS, the
  PROTOCOL row, the score line (no mutators or seed code in the port yet), the EXFIL head.

Status U3-24.8: passing.

## U3-24.9 ULTRA HEALTH in the LAB (Core agent C, `68ab3c6`; off, D-18 stays GG's)

- `Core/UltraHealth.cs` + `Health.hard` / `Cap` (heal and regen fill to max - hard; 0 is the web) + the LAB toggle
  `lab.ultraHealth` (joins Any / Names / Read; the LAB row shown: UiSettings) + `design.lab.ultraHealth` (hardShare 0.35,
  the 1 s + damage / 20 s timer, drain 15 / s, AIRBORNE 40 (timer x2), SAVAGE 60 (x3), SKYGRIND wipes it: ULTRAKILL's
  40 / 50 / 60 / wipe mapped onto SKYGRIND's six ranks, 50 dropped; the blood heal per hit within 4 m: punch / gauntlet
  20, revolver 10 (30 head), shotgun 10, nailgun 1, rail 30, rocket 0; the parry to the cap). The HP bar's grey cap
  (Ui). UltraHealthTests 7 (off, on, the timer and drain by rank, the blood heal, the parry, the LAB stamp, no
  allocation); LabTests' toggle list gained the key.

Status U3-24.9: passing.

## Integration, Play checks and slips (3:28-4:46 PM)

- Cherry-picks in order (`9b41520` .. `c99397d`), then the wiring (`26f7b1e`): BuildRunShape in main.js's order
  (objectives before the PROTOCOL), the views on worldDt, the HUD line's PROTOCOL / exfil parts, RESULTS, the record's
  exfil flag, ULTRA HEALTH after regen, the traps after the power-ups then the moments, `director.fullPace` from the
  exfil, smoke steps 8-10. Fixes on the way: two view files both defined `RunShapeKit` (renamed DesignKit); the design
  views' sizes moved from a file-local class into UnityTuning (CLAUDE.md rule 5).
- 16:21-16:23 `tests: 437 / 437 passed, 0 failed`, `replays: 22 / 22`, `golden tests: 90 / 90`.
- **Play check 2** (16:24-16:28, captures on): every step passed except the HOLD (`338 / 3600 steps` in 150 s) and so
  THE PROTOCOL. Cause (mine): the step called `SurvivalGame.Respawn` every frame to keep the player in the ring, and
  Respawn zeroes the loop's accumulator, so at the Editor's frame rate the sim almost never stepped. Fixed: respawn only
  when moved out. Captures `n11-power-on.png`, `n11-hold-ring.png`, `n11-results-exfil.png`.
- **Play check 3** (16:32-16:35): three steps failed that had passed (round 2's start, RESULTS at round 1, the trap
  kill). Editor.log showed a second smoke running beside the first: `unity-loop.sh eval-file` retries once when the
  answer reports a timeout (`Main thread operation timed out after 5000ms`), and the first call had already run
  `SliceSmoke.Begin()`. The second smoke's StartRun reset the first's run and its aim bot shot the trap's husk. The first
  smoke's HOLD (`3600 / 3600 steps`) and PROTOCOL steps passed. Fixed: `Begin()` refuses a second smoke. **Cap slip:**
  its capture `n11-relay-lit.png` fired twice (once per smoke; one file on disk): 4 capture files, 5 shutter events.
- Also from the captures: RESULTS' grade letter was body-size (the web's is 64 px): now h1; the buy prompt had no FREE /
  OWNED (hud.js buyLabel): ported.
- **Play check 4** (16:41-16:44, no captures): `[smoke] 87.0s DONE: all steps passed`. `d995b6d`; 16:44-16:45 `tests:
  437 / 437 passed, 0 failed`, `replays: 22 / 22`, `golden tests: 90 / 90`; harness 34 sets 0 mismatches.
- Caps used: Play checks 4 of 5; captures 4 files of 4 (5 shutter events, above); Player builds 2 of 2.

## U3-24.10 close (4:47 PM -)

- **The Windows Player** (16:48:25-16:49:37, the night's second build): `build --target StandaloneWindows64 --outputPath
  Builds/Stage1/SKYGRIND.exe --scenes [Survival, Arena] --confirm true`: `Succeeded`, 0 errors, 520 warnings;
  `Builds/Stage1` 155,769,543 B; its StreamingAssets hold 34 vector files (roundType, runMoments, objectives, traps
  included). `git diff --stat`: only `UnityConnectSettings.asset` (never staged); Mobile_RPAsset.asset not rewritten
  this time.
- **INV-smoke** (16:50:21-16:51:59): `Builds/Stage1/SKYGRIND.exe -skygrindSmoke -logFile Logs/player/smoke.log`: `[smoke]
  32.6s PASS the power switch thrown ...`, `[smoke] 33.3s PASS SHOCK PLATE bought (850 -> 160 incl. the kill's points),
  the husk on it killed in 0.2 s: events [powerOn, trapKill husk]`, `[smoke] 68.8s PASS the HOLD ring held at (36, 0):
  3600 / 3600 steps; objectiveDone 300`, `[smoke] 68.8s PASS THE PROTOCOL complete in 30 s: every owned gun forged True
  ([revolver, gauntlet]), points 160 -> 8510; round 7 Break`, `[smoke] 86.3s PASS EXFIL at round 11 (site 0 slabs, 35 /
  35 kills, 0 squads topped up): Results, 'EXFIL', grade C, ...`, `[smoke] 86.3s DONE: all steps passed`; 0 lines
  matching "exception". (Its personal-best line reads 'FIRST RUN ON RECORD', as at arrival: the Player stores no best.)
- **-skygrindGolden** (16:52:39-16:52:54): `[golden] golden total: 34 vector sets (31481 records), 22 replays (22
  exact); 0 failing, 0 mismatches`, `[golden] DONE: 0 mismatches in 4.7 s`.
- **The bench** (`bash Tools/survival-bench.sh 60`, 16:53:19-16:54:31, `power seen: 12 Balanced AC=True`): `median
  2.47 ms, p95 4.11 ms, worst 1% 5.55 ms, mean fps 384`; `collections 5 in 22999 frames; GC Used Memory rises 54476.0
  KB`. NIGHT10's close bench (also Balanced): +103,916 KB with 16 collections, so the U3-24.2 fixes about halved the
  managed growth (frame counts differ: 22,969 vs NIGHT10's run at mean 309 fps).
- **Worktrees:** the three agents' worktrees removed (`git worktree remove`); their branches stay
  (`worktree-agent-abbaf5cf06d2cb013`, `-acf0e00fa2a48ce0f`, `-aed29e5f9833ebeb7`, beside NIGHT10's
  `-ac8f3bcfc6870aa0f`): GG's call to delete.
- **`node port-export-p20/scripts/verify.mjs`** (17:01:11-17:01:14, from the web repo root): `total: 0 mismatches`; the
  web repo's `git status --short` unchanged before and after (only `?? port-export-p20/` is ours).
- **INV-tests / INV-replays / INV-golden** (17:08:35-17:09:19): `tests: 437 / 437 passed, 0 failed, 0 skipped`,
  `replays: 22 / 22`, `golden tests: 90 / 90`; harness `golden total: 34 vector sets (31481 records), 22 replays (22
  exact); 0 failing, 0 mismatches`.
- **Docs:** HANDOFF.md rewritten (anchor `u3-n11`; D-25..D-28 added, D-10 extended with (j) idempotent evals and (k) no
  Respawn per frame), PORT-STATUS.md (the oracle, Survival, UI, other modes, Debug, the events: the web's and the
  port-only ones), PORT-PLAN.md section 2 (traps ticked, objectives / PROTOCOL / run moments part, the guns and the
  hotbar moved to N12, the NIGHT11 design row), ZOMBIES-ULTRAKILL-RESEARCH.md section 3 (power, special rounds, secrets,
  exfil, the grade, hard damage in the LAB), PORT-RULES.md (section 6: no config lookups on the step or frame path; the
  change table), NIGHT11-REPORT.md for GG, NIGHT12.md (untracked) with its facts kept as NIGHT12-RESEARCH.md (the
  research agent's read of the code, sources tagged), tasks.json (U3-24 and U3-24.1..10 passing with evidence; U3-14
  retitled for NIGHT12 and split into U3-14.1..10; U3-15's note: traps done).
- **Memory:** `play-check-eval-recipe` (idempotent evals, no Respawn per frame, old console lines) and
  `night-parallel-agents` (a shared-types commit first, relayed recorder findings, delegate constructors for checkers,
  helper-name clashes) extended; their MEMORY.md lines updated. Lessons seen twice this night: config lookups per step
  (PORT-RULES); the rest are first sightings, kept as memory notes.
- **The TMP fallback asset:** not modified by tonight's Play runs (`git status` shows no change).

Status U3-24.10: passing. U3-24: passing. Close commit tagged `u3-n11`; `git status --short` checked after the tag.

## Summary (5:10 PM, about 3 h of the 8-hour cap)

Every NIGHT11 item passed, none cut. The run's shape is in: objective rounds, THE PROTOCOL, the traps and the grade
ported against the web's tests and p20 golden vectors; the power switch, the swarm drop and exfil behind design flags
(ON); ULTRA HEALTH in the LAB (off); the allocation debt halved. Caps: Play checks 4 / 5, captures 4 files / 4 (one
fired twice in the double-smoke check), Player builds 2 / 2.
