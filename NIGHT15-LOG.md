# NIGHT15-LOG: A MAP WITH A PERSONALITY (map night 1)

Times are from `date` on 2026-10-05 (USMST). First `date`: **Mon Oct 5 11:14:05 USMST 2026**; the close starts by
21:44 (10.5 h); the hard stop is 23:14 (12 h).

## RECEIVED (U3-28.1)

HANDOFF.md section 1, row by row (11:14-11:25):

| # | Check | Output | Verdict |
|---|---|---|---|
| 1 | `git rev-parse --short HEAD`; `git log --oneline u3-n14..HEAD` | `cf04161` on `port/stage3`; after `u3-n14`: `587df44` (the post-close review) and `cf04161` (MAP-RESEARCH.md, HANDOFF for NIGHT15) | as expected (both docs commits) |
| 2 | `git status --short` | ` M ProjectSettings/UnityConnectSettings.asset`, `?? Assets/_Recovery.meta`, `?? Assets/_Recovery/`, `?? NIGHT7.md` .. `?? NIGHT15.md`, `?? QUEUE-first-fifteen.md`, `?? QUEUE-power-guns.md` | as expected (the QUEUE files are GG's untracked prompts) |
| 3 | Unity window title; `bash Tools/unity-loop.sh status` | pid 5908 (since 8:02 AM) "SKYGRIND-unity - Survival - ... Unity 6.3 LTS (6000.3.7f1) <DX12>"; `"status": "ready"`, `"playMode": "stopped"` | ready |
| 4 | `bash Tools/unity-loop.sh compile`, then `tests` | `compile: completed, DLLs rewritten (11:15:43)`, `"compilationFailed": false`, `"consoleErrors": 0`; tests: see below | |
| 5 | `dotnet run --project Tools/CoreHarness -c Release` | `golden total: 39 vector sets (48094 records), 22 replays (22 exact); 0 failing, 0 mismatches` | pass |
| 6 | `SimHarness -- fingerprint`, `-- twice` | `fingerprint seed 1: 12 hashes in 1.28 s: EQUAL to the baseline`; `twice: run 1 vs baseline EQUAL; run 2 vs run 1 EQUAL (12 of 12)` | pass |
| 7 | build 2's smoke | see below | |
| 8 | the machine (11:15) | AC on (BatteryStatus 2, 100 %), power plan **Turbo**; commit **73.3 / 131.1 GB** (the limit is 131.1 GB now, not 145.6); free RAM 20.6 of 63.1 GB; **CPU load 90 %**: `llama-server` (pid 47544, 8.2 GB) busy, `ollama`, four `claude` sessions, Chrome | GG's LLM jobs run by day: no bench before they stop |

- tasks.json: U3-28 and U3-28.1 .. U3-28.9 written after U3-27.13 (a text insert in the file's layout; `asOf` refreshed);
  `node` parses it: 125 items.
- Commit-charge rule tonight (rule 9): 85 % of 131.1 GB = 111 GB.
- Row 4's suite: `run_tests` timed out client-side at 600 s (11:16:45-11:26:50: `Pipeline command 'run_tests' timed out
  after 600000ms`); the Editor.log shows the run finished (`IPostBuildCleanup`, Survival.unity reloaded), but from 11:27
  every Pipeline call times out at 30 s while the Editor process uses 0.0 CPU s over 5 s and its window is not hung
  (`IsHungAppWindow` False): the unfocused-Editor stall (memory note). Someone is using the machine (idle 0-92 s;
  foreground chrome / claude), so the wake script does not take the foreground (it waits for 240 s idle).
  `editor_focus` itself times out (it needs the same main thread). 11:50: 23 minutes: logged, a terminal notification
  sent; Editor-free work continues (rule 9). Row 7 (build 2's smoke) waits for the suite (one heavy check at a time).

## Items

- **11:30 the shared commit `ef20edb`** (gates under dotnet; the Editor stalled): CoreHarness `golden total: 39 vector
  sets (48094 records), 22 replays (22 exact); 0 failing, 0 mismatches`; SimHarness `fingerprint seed 1: 12 hashes in
  0.56 s: EQUAL to the baseline`; `dotnet build` Skygrind.Game / Core.Tests / Game.Tests `Build succeeded`. Builders N
  (nav grid), P (pipeline), K (pad) launched 11:33 in worktrees from it.
  - Decision: the pad wheel opens by holding the NEXT WEAPON button (Y) `design.pad.wheelHold` s or a button bound to the
    new `PadAction.Wheel`; rejected: RB (the web's default layout has RB = PUNCH / PARRY: a hold-to-open would delay
    every punch; changing the web's layout would break the ported GamepadTests).
  - Decision: the aim-assist defaults are applied on the Game side (GameSettings), and the migration's once-key is a
    `_migrated` id (`assist15`), not `_version` 2; rejected: `SettingsStore` (Core) defaults and a `_version` bump: the
    ported web tests pin the web's defaults and `"_version":1` (SettingsTests :102, :172).
  - The new Assets files' `.meta` are not in `ef20edb` (the Editor could not import them); they follow in their own
    commit once it does.
- **11:48 U3-28.3 builder N reported; 11:49 integrated** (`61f54cc` tests, `7cfd2b7` the port): `dotnet run --project
  Tools/CoreHarness -c Release -- navgrid`: `navgrid survival: 11228 nodes, 80482 links (walk 79103, jump 141, drop 360,
  gap 878): EQUAL (ExportFor 389 ms)`, `zoneParts survival: 114 parts: EQUAL`, `navgrid spire: 11178 nodes ... EQUAL`,
  `navgrid training: 4048 nodes ... EQUAL`, `navgrid spire-duel (core towers): ... EQUAL`, `navgrid total: 0 failing`;
  red first (N): `navgrid survival: FAIL NotImplementedException ...`, `navgrid total: 4 failing`. `golden total: 39
  vector sets (48094 records), 22 replays (22 exact); 0 failing, 0 mismatches` after the cherry-pick. Owed: the NUnit
  tests (NavGridBuilderTests) in the Editor.
- **11:40-11:55 U3-28.2 DONE (`514921b`).** `MAP-DESIGN.md`: section 1 the distances with their config keys (the
  player's measured from the web's own trajectories by `scratchpad/traj-measure.cjs`: dash-jump 21.2 m / 1.85 m up,
  wall-ride climbing to 7.6 m, wall-jump 12.2 m, slide-jump 8.9 m, run + double jump 10.5 m / 2.19 m, ARCLINE swing
  59.2 m), section 2 the research (11:40-11:53, 13 min of web time; UNVERIFIED marked; no published map sizes found),
  section 3 three concepts (DRYDOCK, TETHER, THE DIAL), section 3.4 the recommendation (DRYDOCK; D-59 stays GG's),
  section 4 the plan, section 5 the pitfalls, section 6 Proposals. The plan's numbers: `Design/maps/drydock.plan.json`
  (also the greybox's input); `node Tools/map-plan-svg.cjs ...` -> `Logs/shots/n15-plan.svg`, rendered by headless Chrome
  to `Logs/shots/n15-plan.png` (1396 x 1076, 11:53; capture 1 of 25). Review: one second: "a hub ring around the hull
  and the mast with three neighbours that plainly differ (ribs W, ramps E, the start deck S)"; still wrong: "the north
  row is three equal boxes, the hub is mirror-symmetric, labels overlap at the mast". One finding written into section 5:
  the slipway-lip shortcut is 18 m against a 21.2 m dash-jump (0.85: the ambiguous band).
- **11:58-12:02 U3-28.7 code (`d68e0a5` tests first, `b4d88b3`).** Seen red then green under dotnet (the Editor
  stalled): `dotnet run --project Tools/SimHarness -c Release -- aim15` with the stubs: `aim15 FAIL a fresh profile:
  aimAssist snap, padAimAssist snap, padVibration False`, `aim15 FAIL ... the mouse at OFF: the fingerprint differs at
  checkpoint 1`, `aim15: 3 failed`; with the code: `aim15: all passed` (`the mouse at OFF: the fingerprint EQUAL to the
  baseline, the hook handed back True, the setting off`). The NUnit tests (AimAssistDefaultsTests, FingerprintTests'
  new case) and the GAMEPAD-tab capture wait for the Editor. SimHarness gained `aim15` and `mapfp <map.json>`.
- **12:03 U3-28.8 builder K reported; 12:03 integrated** (`be63e31` tests first, `1551f77` the wheel, `92d22d0` Nintendo
  glyphs + rumble strength, `988103b` the pad-only smoke step, `dd0265b` PLATFORM.md) + `c72f014` (lead: the hotbar's
  Nintendo lines). K's evidence (a CoreCLR runner over the built test DLL): `6 of 7 PadWheelTests fail` at the tests
  commit, `PadWheelTests pass 7 of 7, and RadialInputTests 5 of 5` at the end. After the cherry-pick: `dotnet build`
  Skygrind.Game / Game.Tests `Build succeeded`; SimHarness `fingerprint ... EQUAL to the baseline`. K's decisions logged
  in its commits (a tap of Y fires NEXT WEAPON on the release; the stick back in its dead zone keeps the pick).
- **12:04-12:08 U3-28.5 code (`0863c3c`) and the smoke's steps 23-24 (`b1e7c27`).** The map choice, records keyed
  'survival' / 'survival@<id>', the ghosts per map, TITLE's MAP row, the sync of Design/maps. Owed: a test that loads
  both maps (after the greybox exists), the fingerprint in the Editor / Player, the capture of the MAP row.
  - Decision: a map pick reloads the scene (the composition is built for one map in Awake); rejected: rebuilding the
    composition in place (every view and the nav hold the map).
  - Decision: automation (`-skygrind*`) always boots the arena unless `-skygrindMap <id>`; rejected: the saved pick
    (a Player check would test whatever GG last played).
  - Slip (rule 9): one whitespace fix in `UnityTuning.Maps.cs` through `node -e` (a blank line removed); later patches
    use the Edit tool or a written script.
- **12:08-12:20 Editor-free work while the Editor stalls:** `22836fd` a Unity map's own wall guns (`spots.wallBuys`) and
  gate prices (`data.zones[i].cost`; the arena's fingerprint EQUAL); `0987c81` SimHarness `newmap` (on the arena:
  `newmap PASS ... round 8 / 8, cleared 7 ... 60 of 60 hashes equal`); `e6e153f` the SimHarness hook for builder G;
  `f3541ef` the Player's capture tour (`-skygrindShots <dir> [-skygrindMap <id>]`, `-skygrindPadShot <png>`), so the
  night's captures don't need the Editor's Play mode; `5a52b54` DRYDOCK in `design.maps.list`; PLATFORM.md reviewed
  (P5 marked DONE after `c72f014`).
- **The Editor stall, investigated (12:12-12:22):** no modal window (the Editor has one visible top-level window, not
  hung); the Editor.log grows only by the Hub IPC's write timeouts (2 lines per 20 s; the Hub isn't running; these
  began at boot, 8:04 AM, and the Editor answered all morning, so they aren't the cause); 364 WM_NULL posts and 183
  rounds of WM_ACTIVATEAPP / WM_NCACTIVATE / RedrawWindow didn't wake it (`editor_status`, `set_autotick` still time
  out); the Pipeline cancels a request that times out (`Dispatcher.cs:89-95`), so nothing queues for later. The first
  wake (12:19:45, at 61 s idle) got `SetForegroundWindow ... (False)`: Windows' foreground lock. The second watcher
  (`scratchpad/editor-wake-idle2.ps1`, from 12:22) waits for 45 s of idle, takes the foreground through
  `AttachThreadInput`, sends the Pipeline's `set_autotick --enable true --interval_ms 16` (`AutoTickCommand.cs`: "keeps
  the Unity Editor ticking at full rate even when its window is unfocused", persisted for the session) and hands the
  foreground back. Build 1 (rule 8: by 13:44) waits for it.
- **12:23 the Editor answers again.** The second watcher at 12:23:00 (idle 71 s): `Editor to the foreground: True`,
  `set_autotick: "Auto-tick already enabled (interval updated to 16ms)"`, `editor_status: { "status": "ready" ...}`,
  `the foreground back to the previous window: True`. From then on the Editor answers while unfocused (12:23:46,
  foreground chrome). Lesson (for the memory note): Windows refuses SetForegroundWindow to a background process
  (`False`); AttachThreadInput to the foreground thread works; one foreground moment revives the Editor for the session.
  `AssetDatabase.Refresh()` by eval imported the files changed since 11:27 (no auto-refresh while unfocused); `aa0bc1d`
  their `.meta`.
- **12:26-12:32 the arrival suite, async** (`run_tests --async_tests true`, polled with `test_status`, which needs no main
  thread): `"total": 702, "passed": 702, "failed": 0, "skipped": 0` in 339.5 s, `replays: 22 / 22`, `golden tests: 96 /
  96`; tonight's: NavGridBuilderTests 9 / 9, AimAssistDefaultsTests 5 / 5, PadWheelTests 7 / 7,
  FingerprintTests.TheMouseAssistOff_TheFingerprintIsStillTheBaseline 1 / 1, RadialInputTests 5 / 5, GamepadTests 21 / 21,
  SettingsTests 10 / 10. **U3-28.3 DONE** (tasks.json passing).
- **12:28 U3-28.4 builder P reported; 12:31 integrated** (`89ca87b` tests first, `e8695ba` the pipeline; `b424c23` the
  metas and MapChoiceTests). P's evidence (dotnet): `survival Build: nav built, 38/38 sim keys same, whole without
  diagnostics SAME`, the round-tripped file's `fingerprint seed 1: 12 hashes ... EQUAL to the baseline`,
  `Reports/MAP-survival-validate.md` `findings: 5` (113 checks; the 5 are perks / relays on perches RIVAL's graph has no
  move for: explained in the report). In the Editor (12:34-12:36): MapPipelineTests `13 / 16 passed`: the acceptance
  test `RoundTrip_Survival_NavBuilt_SimKeysByteIdentical_AndTheFingerprintIsTheBaseline` passed (5.0 s); 3 failed
  (`Import_Survival_MakesOneMarkerPerEntry`: 28 zone boxes for 29; training's `core.note` written twice): sent back to P
  with a second request (a VERB result for designed verb gates, so DRYDOCK's validator can be clean). MapChoiceTests
  `1 / 3` (red until DRYDOCK's file exists, as written).
- **12:33 builder G launched** (the DRYDOCK greybox) from `e8695ba`: the map built two ways from the one plan (a
  SimHarness `mapgen` under dotnet; `Build_drydock.cs` in the Editor), proven by `newmap` and `mapvalidate`.
- **12:37:41 build 1 queued** (Survival + Arena, StandaloneWindows64, `Builds/Stage1/SKYGRIND.exe`).
- **Build 1 (rule 8: 1 h 24 min after the first `date`):** `"result": "Succeeded"`, `"totalErrors": 0`,
  `"totalWarnings": 9`, 12:37:44-12:38:31 (46 s); `git diff --stat ProjectSettings`: only `UnityConnectSettings.asset`
  (not staged); `Mobile_RPAsset.asset` not rewritten. Builds tonight: 1 of 5.
- **Build 1's smoke** (12:39:03-12:42:20, windowed 1280 x 720, `-skygrindPadShot`): `[smoke] 180.9s DONE: 3 step(s)
  failed`: step 18 `clean slate: ... 12 of 12 checkpoints equal, the first run differs from the baseline at checkpoint 6`
  and step 21 `attract: ... the next run differs at checkpoint 6` (the NIGHT14 residual, accepted tonight: rule 4), and
  step 24 `new map: design.maps.list has no Unity-authored map with a file` (DRYDOCK's file comes with builder G). Every
  other step PASS, 0 `Exception` lines; the new step 23 (U3-28.8): `pad only: TITLE ('PLAY' selected), A started a run`,
  `Y held 0.26 s opened the wheel True (on the HUD True); the right stick pointed at slot 1 (want 1, the HUD 1); the view
  held still True ...; the release took it: slot 0 -> 1, wheel shut True`, `a tap of Y is NEXT WEAPON: slot 1 -> 0`,
  `START paused True; the d-pad reached 'SETTINGS'; A opened SETTINGS True; B closed it back to PAUSED True`, `START
  resumed the run ... the device used last Pad (the smoke sent no mouse or keyboard input)`.
- **Capture 2, `Logs/shots/n15-pad-wheel.png`** (12:42, the wheel opened by the virtual pad, 2 s linger). Review: one
  second: "a translucent wheel mid-screen with two labelled guns, the picked one (2 · PULSE GAUNTLET) cyan, a dot where the
  stick points; the hotbar says Y NEXT · D-PAD LEFT BACK"; still wrong: "the world's THE GRINDER 1250 gate label shows
  through the disc and clashes with the centre text; K's pad prompt (RIGHT STICK · RELEASE) isn't visible; nothing on the
  hotbar says the wheel is a hold of Y". **U3-28.8 DONE** (tasks.json passing).
- **Build 1's other Player checks (12:42:41-12:44:02):** `[golden] golden total: 39 vector sets (48094 records), 22
  replays (22 exact); 0 failing, 0 mismatches` (28.0 s); `[fingerprint] seed 1: 12 hashes over 7200 steps in 0.49 s:
  EQUAL to the baseline`; `-skygrindReplay` on the smoke's tape: `[replay] EQUAL (1 of 1 checkpoints, 485 steps)`; the
  capture tour (`-skygrindShots`): `n15-title.png`, `n15-gamepad-tab.png` (captures 3, 4).
- **The migration, live:** build 1's first boot migrated this machine's saved settings once: `[settings] the NIGHT15
  defaults: aimAssist off, padAimAssist assist, padVibration True` (in the smoke's log only; the later Players didn't);
  `skygrind-settings.json`: `"_migrated":["sens14","snap1","window1","assist15"]`.
- **Capture 4, `n15-gamepad-tab.png`** reviewed: one second: "the GAMEPAD tab: SNAP MODE (PAD) LIGHT, VIBRATION ON (the
  new defaults), the new RUMBLE 1.0 row"; still wrong: "the row is still called SNAP MODE (PAD) though its values now read
  plainly (AIM ASSIST (PAD) would match); no backing panel, so the arena's geometry crowds the rows". **U3-28.7 DONE**.
- **12:40 P's follow-up `f0ddfc2`** (the VERB class, the three test fixes: test bugs, e.g. survival has 25 zone boxes +
  3 holes = 28). The Editor 12:43-12:46: `MapPipelineTests: 19 / 19 passed`. **U3-28.4 DONE** (the round trip and its
  fingerprint in `RoundTrip_Survival_NavBuilt_SimKeysByteIdentical_AndTheFingerprintIsTheBaseline`; survival's 5 findings
  explained in `Reports/MAP-survival-validate.md`).
- **12:48 TITLE's MAP group moved onto the run setup's line** (`82e1eb0`): capture 3 showed its planned spot (176 px above
  PLAY) on the subtitle. `c17be8e`: the aim rows renamed AIM ASSIST / AIM ASSIST (PAD) (capture 4's review).
- **12:56 U3-28.6 builder G reported; 12:57 integrated** (`a5d2441`), the follow-up (`26ac6ff`) at 12:59. G's gates
  (dotnet): `mapgen drydock: 1707756 chars -> Design/maps/drydock.json and Assets/StreamingAssets/Design/maps/drydock.json
  (byte-identical: yes)`, `model round trip (FromJson -> Export, the Editor's markers' path): SAME`, `plan checks: 0
  problems`, `in a headless composition: 4 of 4 wall guns hang (shotgun, rocket, nailgun, rail), 5 of 5 perk machines
  stand (mend, flip, vigor, slipstream, quick), THE FORGE stands`; `mapvalidate`: `**findings: 0** (53 checks, 1 VERB)`
  (VERB: relay keel, wallRide); `newmap PASS autopilot on drydock: seed 4242 normal (botBrain) twice for 300 s: round 6 /
  6, cleared 5, 99 kills, cause timeout, spent 6350 of 8645; 60 of 60 hashes equal`. In the lead's tree (12:57, the
  first version): `newmap PASS ... round 9 / 9, cleared 8 ... 60 of 60 hashes equal (3.8 s)`, the arena's fingerprint
  EQUAL.
  - The lead's design calls on G's findings (logged in the plan's notes): the wall guns the port fires (THUNDERCLAP,
    COMET, STITCHER, LONGWAVE; RATTLER / BOUNCER / DISCUS aren't ported) and MEND for STEADY; the exfil radio faces the
    gangway (yaw pi); the slipway deck meets the wall (the 2 m pocket gone); LAUNCH RAIL raised to 3.0 m on a 2.0 m
    kicker. Rejected: keeping the design's guns (no wall buy would stand in three zones tonight).
  - Open (for "As built"): RIVAL's graph still reaches LAUNCH RAIL by a dash-jump plus a double jump (~3.1 m by
    formula); kept as a second expert route.
- **12:58-12:59 MapChoiceTests 3 / 3** in the Editor (`4537c42`: the headless run needs `noLab` / `metaSilent`; red at
  12:35 before DRYDOCK's file).
- **13:00:35 the Editor's half:** `Skygrind.EditorTools.BuildDrydock.Build()` (eval; its 5 s reply limit ran out, the
  build completed): `[map] build DRYDOCK: Design/maps/drydock.json and its Player copy (1707756 chars; the file before:
  SAME); markers 133 in Assets/Scenes/Maps/drydock-authoring.unity (saved), their export SAME as the plan's text; plan
  checks: 0 problems; validate: 0 findings, 1 VERB -> Reports/MAP-drydock-validate.md; 0 notes, 1 import warnings (5081
  ms)` (the warning: `spots: key 'wallBuys' is not carried (the export drops it)`: P's model keeps no wall-buy field; G's
  builder adds them after). The files it rewrote differed only in line endings (git's CRLF checkout): restored.
  `a6cf9a4` the authoring scene. Worktrees: all three builders' removed (their uncommitted files were byte-equal copies
  of the integrated code).
- **13:01:37 build 2:** `Succeeded`, 0 errors, 9 warnings, 16 s; `drydock.json` in the Player's StreamingAssets.
  Builds tonight: 2 of 5.
- **Build 2's smoke (13:02-13:05): `[smoke] 187.3s DONE: all steps passed`**, 0 `Exception` lines. Step 24: `PASS
  autopilot on drydock: seed 4242 normal (botBrain) twice for 300 s: round 6 / 6, cleared 5, 99 kills, cause timeout; 60
  of 60 hashes equal (6.2 s)`. Steps 18 and 21 passed this time: `clean slate: ... the first run EQUAL to the baseline`,
  `attract: ... the next run EQUAL to the baseline (12 of 12)` (not claimed fixed: see build 3).
- **The tour on DRYDOCK (build 2, 13:06):** `[shots] tour on 'drydock' (2 map(s) listed)`, the Player's log `Survival
  loaded: 4 zones, 664 spawn nodes, 10732 nav nodes`; 7 captures (`Logs/shots/n15-b2/`; captures 5-11). Reviews:
  - `n15-title.png`: one second: "the setup line reads SURVIVAL | DAILY · HEAT 0-3 · MAP ARENA | DRYDOCK, DRYDOCK picked
    in cyan; the new map's mast and hull behind the menu"; still wrong: "the MAP group has no sub-line (its neighbours
    do); the row reaches further left than the info line". **U3-28.5 DONE.**
  - `n15-drydock-above.png`: "the built heart reads exactly like the plan: the rib canyon, the hull with its arches in
    the ring, the deck and two ramps, the gangway"; still wrong: "the locked gates don't show from above".
  - `n15-drydock-ribs.png`: "a wall-ride canyon: two tall walls 7 m apart, cyan rims on top"; still wrong: "nothing marks
    a rideable rib; the alley ends in the closed north wall".
  - `n15-drydock-hub.png` (at the gangway door), `n15-drydock-slipway.png` (on the deck), `n15-drydock-landmark.png`
    (beside a rib): failed as views: the locked gate's "THE CRADLE 750" sign filled the door view; the deck hid the ramp;
    a slab hid the mast entirely (a finding: inside the rib alleys the mast is hidden). `b6a1211` opened the zones for the
    tour and moved the three poses.
- **13:07-13:10 `SimHarness verbs`** (`cb25832`; `Logs/n15-verbs.txt`): the lane A slide `peak 28.81 m/s`; `launch rail: 3
  of 6 take-off points land on the 3.0 m rail`; `ribs wall-ride: wall-ride 1.20 s, max y 7.68` (first run 0.00 s: the
  run from the cross alley clipped the slab's end, 8 -> 4.6 m/s, `verbtrace`); the four pads `off by 0.07 m` or less;
  `deck lip dash-jump: 3 of 4 timings land on the hull deck`.
- **13:13-13:19 the suite (async): `tests: 724 / 724 passed, 0 failed, 0 skipped`, `replays: 22 / 22`, `golden tests: 96 /
  96`** (the code at `cb25832`; DLLs 13:11:10).

## The close (U3-28.9)

- **13:19:48 build 3, the final Release build** (the code at `cb25832`): `Succeeded`, 0 errors, 8 warnings, 23 s; `git
  diff --stat ProjectSettings`: only `UnityConnectSettings.asset` (not staged); `Mobile_RPAsset.asset` not rewritten; the
  TMP fallback asset not rewritten (no Play mode tonight). Builds: 3 of 5.
- **Build 3's smoke (13:20:37-13:23:47): `[smoke] 189.0s DONE: 2 step(s) failed`:** step 18 `clean slate: ... the first
  run differs from the baseline at checkpoint 6` and step 21 `attract: ... the next run differs at checkpoint 6` (the
  NIGHT14 residual, accepted tonight: rule 4); every other step PASS, step 23 `pad only: ... (the smoke sent no mouse or
  keyboard input)`, step 24 `autopilot on drydock: ... round 6 / 6, cleared 5 ... 60 of 60 hashes equal (5.5 s)`. The
  residual tonight: failed on builds 1 and 3, passed on build 2 (intermittent).
- **Build 3's checks (13:24-13:26):** `[golden] golden total: 39 vector sets (48094 records), 22 replays (22 exact); 0
  failing, 0 mismatches` (36.2 s); `[fingerprint] seed 1: 12 hashes over 7200 steps in 0.79 s: EQUAL to the baseline`;
  `[replay] EQUAL (1 of 1 checkpoints, 491 steps); seed 737373, Stopped, stamp SKYGRIND 0.1.0 (cb25832)` on build 3's
  smoke tape; the tour: 7 captures in `Logs/shots/n15-b3/` (captures 12-18 of 25). Reviews of the three new poses:
  - `n15-drydock-hub.png` (inside THE RING): "the hull's 24 m rib arches with the mast rising behind them; a wide lane";
    still wrong: "hull, ribs and walls share one purple stone; the mast's top leaves the frame".
  - `n15-drydock-slipway.png` (lane A's lip): "the 40 m ramp drops away toward the kicker and LAUNCH RAIL's pylon";
    still wrong: "the slope barely reads (the deck's material, no stripe); the south wall closes the view".
  - `n15-drydock-landmark.png` (66 m from the mast): "the mast stands clear above the arches"; still wrong: "a plain 2 m
    post, no crow's nest or beacon to read; the HUD's round counter overlaps it". **U3-28.6 DONE** (MAP-DESIGN.md
    section 7, As built: `939c000`).
- **The UI-hurt Silent series: not run (SHORT, rule 10):** 13:26 `powercfg`: **Turbo** (not Silent; the plan is GG's
  setting), AC on (100 %), the user idle 3084 s, but **CPU load 88 %** with two `llama-server` processes and `ollama`
  busy: "no heavy process beside them" fails.
- **13:27-13:30 the docs:** `939c000` MAP-DESIGN.md As built; `3046d5d` this log and tasks.json; `c7356b9`
  NIGHT15-REPORT.md and PORT-STATUS.md's Maps row (header `u3-n15`); `5f1e96d` HANDOFF.md rewritten (the `handoff` skill;
  anchor `u3-n15`; D-59 .. D-64); NIGHT16.md written (untracked: map night 2); memory: `unity-editor-unfocused-autotick`
  (new), `night-parallel-agents` (the refused cherry-pick, seen twice tonight). Captures: 18 of 25. Builds: 3 of 5. Play
  checks: 0 of 3 (the Player's tour replaced them). Builders: 4 of 4. Research: 13 of 30 min.

## Close table (13:30)

| Item | Status | Evidence |
|---|---|---|
| U3-28.1 Start | DONE | RECEIVED 11:14-11:30 (HANDOFF section 1 row by row; the machine: AC, Turbo, commit 73.3 / 131.1 GB, CPU 90 %); tasks.json U3-28.* written, `node` parses it (125 items) |
| U3-28.2 MAP-DESIGN.md | DONE | `514921b`: the distances with their config keys (measured from the web's trajectories), 13 min of research (UNVERIFIED marked), three concepts, DRYDOCK recommended (D-59 GG's), the plan `Logs/shots/n15-plan.png` reviewed (11:54), the pitfalls checked, Proposals |
| U3-28.3 The nav grid in C# | DONE | red: `navgrid total: 4 failing` (stubs); green: `navgrid survival: 11228 nodes, 80482 links ...: EQUAL`, spire 11178 / training 4048 / the duel grid EQUAL, `zoneParts survival: 114 parts: EQUAL`; NavGridBuilderTests 9 / 9 in the Editor; CoreHarness `golden total: ... 0 failing, 0 mismatches` |
| U3-28.4 The map pipeline | DONE | MapPipelineTests `19 / 19` in the Editor, incl. `RoundTrip_Survival_NavBuilt_SimKeysByteIdentical_AndTheFingerprintIsTheBaseline` (import survival.json, export: the sim keys byte-identical; its fingerprint the baseline); `Reports/MAP-survival-validate.md` `findings: 5`, each explained (perches RIVAL's graph has no move for) |
| U3-28.5 Choosing a map | DONE | MapChoiceTests red 12:35 (`1 / 3`), green 12:59 (`3 / 3`: both maps load and play); the suite `724 / 724 passed`; build 3's fingerprint `EQUAL to the baseline` (automation boots the arena); `Logs/shots/n15-b2/n15-title.png` (MAP ARENA | DRYDOCK) reviewed |
| U3-28.6 The greybox | DONE | `Reports/MAP-drydock-validate.md` `**findings: 0** (53 checks, 1 VERB)`; the Player smoke's step 24 (builds 2 and 3) `PASS autopilot on drydock: ... round 6 / 6, cleared 5, 99 kills ... 60 of 60 hashes equal`; five views reviewed (above, the hub, the ribs, the slipway, the landmark from 66 m; `Logs/shots/n15-b2/`, `n15-b3/`); MAP-DESIGN.md As built (`939c000`); the moves measured (`SimHarness verbs`) |
| U3-28.7 Aim assist calmed | DONE | red `aim15: 3 failed` (stubs), green `aim15: all passed`; AimAssistDefaultsTests 5 / 5 and the fingerprint case 1 / 1 in the Editor; build 3's fingerprint `EQUAL`; the live migration once (`[settings] the NIGHT15 defaults: aimAssist off, padAimAssist assist, padVibration True`); `n15-gamepad-tab.png` reviewed |
| U3-28.8 The pad's gaps | DONE | PadWheelTests 7 / 7 in the Editor (red: 6 of 7 failing, CoreCLR); the Player smoke's step 23 PASS on builds 1-3 (`... the smoke sent no mouse or keyboard input`); `n15-pad-wheel.png` (the wheel opened by the virtual pad) reviewed; PLATFORM.md |
| U3-28.9 Close | DONE | build 3 (the final Release build) `Succeeded`; its smoke `DONE: 2 step(s) failed` = steps 18 / 21 only (the accepted residual); `-skygrindGolden` `0 failing, 0 mismatches`; `-skygrindFingerprint` `EQUAL to the baseline`; `-skygrindReplay` `EQUAL (1 of 1 checkpoints, 491 steps)`; the UI-hurt Silent series not run by its own rule (measured 13:26: Turbo, CPU 88 % with llama-server x2 and ollama); tasks.json, this log, NIGHT15-REPORT.md, HANDOFF.md, NIGHT16.md, PORT-STATUS.md; tag `u3-n15` on the close commit |

NIGHT15 CLOSED (13:30).
