# NIGHT16-LOG: THE FEEL NIGHT

Times are from `date` on 2026-10-05 (USMST). First `date`: **Mon Oct 5 14:18:33 USMST 2026**; the close starts at 19:18
(5:00); the hard stop is 20:18 (6:00).

## RECEIVED (U3-29.1)

HANDOFF.md section 1, row by row (14:18-14:24):

| # | Check | Output | Verdict |
|---|---|---|---|
| 1 | `git rev-parse --short HEAD`; `git describe --tags` | `a691dd8`; `u3-n15-1-ga691dd8` (one docs commit after `u3-n15`: HANDOFF section 4) | as expected |
| 2 | `git status --short` | ` M ProjectSettings/UnityConnectSettings.asset`, `?? Assets/_Recovery.meta`, `?? Assets/_Recovery/`, `?? NIGHT7.md` .. `?? NIGHT16.md`, `?? QUEUE-first-fifteen.md`, `?? QUEUE-map-night-2.md`, `?? QUEUE-power-guns.md` | as expected |
| 3 | `bash Tools/unity-loop.sh status`; `set_autotick --enable true --interval_ms 16` | pid 5908 (since 8:02 AM); `"status": "playing"`, `"playMode": "playing"` (GG's Play session left on; idle 47 s); `Auto-tick already enabled (interval updated to 16ms)` | Play mode stopped by the lead at 14:31 (`Exited play mode`) before the shared commit's compile: GG had left the run to the night |
| 4 | the suite | after the shared commit (below) | |
| 5 | `dotnet run --project Tools/CoreHarness -c Release` | `golden total: 39 vector sets (48094 records), 22 replays (22 exact); 0 failing, 0 mismatches` | pass |
| 6 | `SimHarness -- fingerprint` | `fingerprint seed 1: 12 hashes in 1.70 s: EQUAL to the baseline; run Fight round 4, kills 36, shots 80, earned 4310, alive 0, owned 2, flow Playing` | pass |
| 7 | build 3's smoke, as `Builds/n15-before/SKYGRIND.exe -skygrindSmoke` (14:20:17-14:25:38) | `[smoke] 321.2s DONE: 4 step(s) failed`: `save and resume` (the NIGHT14 residual, step 18), `virtual pad` (step 21, the residual), `virtual pad menus`, `pad only` ("A started a run: Title"); 74 PASS lines; 0 exception lines | it ran (U3-29.1's bar). Two more pad steps failed than at NIGHT15's close (3 of 3 then failed 18 / 21 at most); the run took 321 s against 189 s (the Editor in Play mode and two dotnet runs beside it). Recorded as the A/B build's baseline, not tonight's job |
| 8 | the machine (14:19) | AC on (BatteryStatus 2, 100 %), power plan **Turbo**; commit 77.5 / 134.3 GB; CPU 23 %; Unity 5908 the top process | quiet now; benches only at the close if still idle |

- `Builds/n15-before/` = a copy of `Builds/Stage1/` (165 MB, NIGHT15 build 3, stamp `cb25832`), 14:20.
- tasks.json: U3-29 and U3-29.1 .. U3-29.10 written after U3-28.9 (a text insert; `asOf` refreshed); `node` parses it:
  136 items.

## Items

- **14:24-14:34 the shared commit** `695531c` + `390cdf3` (part 2: the ImpactRender hook): design.move and design.impact
  (with `_why`s) in Design/unity-config.json and its byte-identical Player copy (inserted as text: the file is
  hand-formatted); `Core/FeelKeys.cs` (events `weaponReady`, `impactFreeze`, `slowMo`, `slideCharged`, `weaponDraw`;
  payloads; settings keys); stubs MoveBalance / ImpactRules / ImpactFrames / FeelStrip (throwing) and the UiFeelHud
  partial; the loop's time scale (`SurvivalGame.Impact.cs`: `ImpactBeginFrame` / `ImpactStepEnded` / `WorldScale`,
  u3-n15's loop exactly until builder I fills it; the render half's world dt x `WorldScale`); `Mover.slideCharge` (a
  neutral field for the HUD); SurvivalSim's `MoveBalanceInit` / `MoveBalanceStartRun` hooks; HIT FREEZE / SLOW-MO
  (ACCESS) and the LAB's SLIDE CHARGE / SLIDE DECAY / HOP FALLOFF (on by default; not LAB toggles, so they never stamp
  a run); Ui's BuildFeelHud / RenderFeelHud; `-skygrindStrip <dir> [scenario]`; SimHarness `spam` / `impact`; empty
  fixtures. Gates: SimHarness `fingerprint seed 1: 12 hashes in 0.91 s: EQUAL to the baseline`; the Editor
  `compile: completed, DLLs rewritten (14:32:28)`, `"compilationFailed": false`, `"consoleErrors": 0`.
  - Decision: the new freezes live in the Game loop beside `sim.hitstopLeft` (the longest wins: `hitstopLeft` in the
    render half is max(sim's, the impact layer's)); the perfect parry's 0.30 s is the impact layer's request, so
    `design.parry.perfectHitstop` (0.15, the sim's own request, in the snapshot) is unchanged and the fingerprint can't
    move. Rejected: raising design.parry.perfectHitstop (a Core number in the snapshot: an effects change in the wrong
    layer, rule 3).
  - Decision: the movement rules' LAB rows are settings outside `design.lab.toggles` (so `Lab.Any` ignores them: a run
    with the rules on, the default, is not stamped LAB); automation (`noLab`) reads design.move's defaults.
  - Slip (logged): the Write tool overwrote the tracked `UnityTuning.Feel.cs` (NIGHT12's) with the new rows; seen at
    once by `git log`, restored with `git checkout --`, the rows moved to the new `UnityTuning.FeelRows.cs`. Every new
    path since is checked with `git ls-files` first.
- **14:34-14:38 builders launched** in worktrees from `390cdf3`: M (movement), I (impact), V (hands), U (UI and the
  strip tool); the read-only research agent (30 min cap). The suite started async at 14:33:17.
- **14:39 the shared commit's suite:** `total 724, passed 723, failed 1` (377.6 s), replays 22 / 22, golden 96 / 96. The
  one failure: `RunSnapshotTests.MoverState_ListsEveryMoverField` ("a new Mover field needs a decision": the shared
  commit's `Mover.slideCharge`). Fixed in `fb8c9d5` (MoverState saves it: it's run state; the fingerprint hashes its own
  mover list, so EQUAL): `tests RunSnapshotTests`: `8 / 8 passed`; CoreHarness `0 failing, 0 mismatches`, 22 exact;
  SimHarness fingerprint EQUAL. M told to add every new Mover field of its own there. **U3-29.3 DONE.**
- **14:38-14:44 the audit's tour** on `Builds/n15-before/` (`-skygrindShots ... -skygrindMap drydock`, 1920 x 1080):
  7 captures in `Logs/shots/n16-before/` (7 of 40). Looked at: TITLE, the GAMEPAD tab, the hub. The three worst UI
  defects (FEEL-DESIGN.md section 1, rows 9-11), sent to U at 14:40: the hotbar cuts names mid-word ("KESTR": a
  deliberate 5-char cut, UiHotbar.cs:170-177, still reads as a bug); the style plate greets a new run with "D / DULL" and
  an empty bar; the two first-run toasts use two type treatments.
- **14:41** I told to own the web's FOV kick stack too (player.js :166-212: the multikill widen, the melee lunge, the
  rank-up widen; CameraFeel.cs:10 "not ported"), so the camera's FOV kicks have one owner and a punch doesn't narrow twice.
- **14:42-14:49 FEEL-DESIGN.md** sections 1, 3-5 (`0115c35`), section 2 the research (`07aa374`): the agent's report
  (14:38-14:49, every fact tagged). What it changed: the look stays live in every freeze (ULTRAKILL Patch 15c, on purpose);
  the flash white only, never red (XAG 118); a 0.5 s hard cap on any freeze (Smash's); the slide charge measured both ways
  (Apex / Titanfall 2 time the boost 2.0 s from the last slide ENTRY; ours 1.0 s after the slide ends): sent to M, I, V.
- **14:44 the farm's BEFORE half** (U3-29.4): `FARM_EXE=Builds/n15-before/SKYGRIND.exe bash Tools/farm-batch.sh
  Logs/farm-n16-before 100 1 normal` started (headless, seeded).
- **Polish taken while the builders work** (U3-29.8, rank order skipping the builders' files):
  - `d3a1b29` #4 + #5 (one commit: both are MovementFx + UnityTuning.FeelPolish.cs): a landing's dust grows with the fall
    speed (more cubes, a wider spread, a faint ring at the feet; none under a slam's own ring: the mover emits Slam then
    Land, Mover.cs:1046-1057), and a slide faster than a walk throws heel dust, more the faster. Editor compile clean.
    Evidence: build 1's slide strip (pending).
  - `8d45924` #8 (half): the menus' focus frame slides 0.08 s between controls (visual only; none across screens or with
    REDUCED MOTION). The press scale needs Ui.cs's Button helper (U's tonight): later.
  - #2 (the struck enemy held white through the freeze): **exists by construction**: the husk flash counts down on the
    world dt, 0 in a freeze (EnemyViews.cs:545 with Render.cs:49), and a dying enemy's white dissolves on world time
    (EnemyViews.cs:414). Evidence: the freeze strips.
  - #3 (the pip refill blink, the web's `hud.pipFlashTime`, hud.js :479-486, not ported) and #7 (the slide charge's
    return): given to U's crosshair ticks (14:52): a cyan burst at the feet is out of a first-person view.
  - #11 (wall-ride sparks) and #13 (shells on the pump): **exist** (MovementFx NIGHT12 port-only sparks; WeaponFx.cs:168,
    355-358 ejects a shell weapons.shotgun.pumpDelay after the shot). #10 (a dash smear frame): **left**: the dash already
    has three cues (the FOV kick, the edge tint, PostFx's chroma pulse); a fourth is noise.
- **14:51 FEEL-PLAYTEST.md** drafted (U3-29.9's kit; corrected after the builders land).
- **14:53** `8c8fd91` polish #9 (the vignette half of the round-clear beat: PostFx deepens the vignette 0.12 through the
  last kill's slow-mo); `c1edf12` PORT-STATUS's feel events.
- **14:54-15:04 every builder landed and was integrated** (each tests-first commit cherry-picked alone, compiled, seen red
  in the Editor, then the rest, then green in the Editor):
  - **U** (18 min; `3490a12` tests first .. `63d1d51`): red `FeelHudTests` NotImplementedException; green
    `tests: 12 / 12 passed` (14:57). The strip tool, the crosshair HUD (+ the web's pip flash, the refill / slideCharged
    pulses), the headshot diamond, the transitions, the rank plate's zero state, the hotbar's whole words, the hold-Y
    hint, the wheel's backdrop, the MAP sub-line, the left column's gap. Its one hand-off: the hint card's type
    (UiOnboarding.cs:61-63, not U's file): the lead's `46b5474` (tBody, no letter-spacing).
  - **I** (21 min; `045fa43` tests first, `1977084`, `df29c4a`): red `ImpactRulesTests` "ImpactRules: builder I";
    green `15 / 15` and `ImpactLatch 1 / 1` (14:58-15:00). design.impact.budget.maxFreeze 0.5 added (the research's cap).
    sim.hitstopLeft: drained to 0 after every step and handed to the rules (pacing only; a Game run's snapshot saves 0;
    every sim request now emits its `hitstop` event). The ledger: section 3.2 of FEEL-DESIGN.md (`impact PASS`).
  - **M** (22 min; `0b02f03` the instrument, `346f285` tests first, `b8bca8e`, `1e94ee5` the tuning): red
    `MoveBalanceTests` "MoveBalance.Params.From"; CoreHarness at the tests-first Core commit and after the implementation
    `golden total: 39 vector sets (48094 records), 22 replays (22 exact); 0 failing, 0 mismatches`. Retuned:
    rechargeTime 1 -> 2, hop step 0.1 -> 0.15. THE TAPE format 2 (the move flags byte; a format-1 tape replays with the
    flags off). The spam table: FEEL-DESIGN.md section 3.1 (targets a, b, c met; d unchanged; e no change).
  - **The fingerprint re-recorded** in its own commit `0ac694e`: before `first difference at checkpoint 5`; after
    `fingerprint seed 1: 12 hashes in 0.53 s: EQUAL to the baseline; run Fight round 4, kills 36, shots 80, earned 5102,
    alive 14, owned 2, flow Playing`.
  - **V** (25 min; `1004854` tests first .. `0e12028`): red `HandsTests` "HandsSwapTimeline.Of"; green
    `tests: 19 / 19 passed` (15:03, the Editor-only allocation test included). The trace: `Logs/n16-vm-trace.csv`
    (168 KB, written by the suite's `WritesTheViewmodelTrace`).
- **15:00 the farm's BEFORE half** (`Builds/n15-before`): `farm 100 runs, normal, seeds 1..100, brain botBrain: mean round
  9.52, max 26, timeouts 13; causes: ACID 24, HUSK 32, SPITTER GLOB 10, SWARMER 21, timeout 13; 650.8 s wall`.
- Slip (logged): FEEL-DESIGN.md section 1's three evidence cells replaced by a `node -e` one-liner (CLAUDE.md rule 9: docs
  by the Write / Edit tools only); the text is right; Edit from here on.
- **15:04-15:12 the suite after the integration:** `total 784, passed 782, failed 2` (333 s), replays 22 / 22, golden
  96 / 96. Both failures were preconditions the slide rules moved, measured before touching them (a SimHarness probe,
  `BreakProbe.cs` in the scratchpad): `RunRestoreTests.Restore_AtALaterBreak` (seed 37: round 5's break now starts at
  script step 9362 and READY ends it after survival.readyHold's 72 steps; flags off: 957 steps): the snapshot taken 30
  steps in, not 240; `TapeTests.OldFormat...Refused` (format 1 is read since tape format 2): the refused version is 0.
  `e54fa29`: `RunRestoreTests 5 / 5`, `TapeTests 7 / 7`. **The suite: 784 / 784.**
- **15:13 build 1** (Release; `e54fa29` + docs): `Succeeded`, 0 errors, 34.3 s, 171.7 MB; `git diff --stat
  ProjectSettings`: only UnityConnectSettings.asset (not staged); the Player's design config byte-identical. Its checks
  run (below).
- **15:14 V resumed** for the polish queue's #6 (the quick-swap flick) and #14 (the idle flourish), its files.
- **Polish judged:** #12 (a hold-progress ring on BUY): **exists** as the web's prompt bar filled by
  `buyables.Progress` (Ui.cs:1244): a ring would restyle the web's; left. #15 (a perfect parry's one-frame inverted
  impact frame): **left**: inverting the dusk palette maps cyan (yours) toward red (danger only, CLAUDE.md rule 8) for that
  frame, it is a full-screen luminance flash on top of the parry's white one (WCAG's count), and one frame is 7 ms at 144
  fps; the research found no frame counts for it (FEEL-DESIGN.md 2.5).
- `c83955a` polish #8 (the press half): a menu button dips to 0.96 on press and eases back in 0.12 s (UiPress).

### Build 1 (15:13; checks 15:14-15:21)

| Check | Output | Verdict |
| --- | --- | --- |
| smoke | `[smoke] 188.3s DONE: 2 step(s) failed`: step 18 `clean slate: seed 1 twice on the played composition: 12 of 12 checkpoints equal, the first run differs from the baseline at checkpoint 6`, step 21 `attract: ... the next run differs at checkpoint 6` | the NIGHT14 residual word for word (NIGHT15's builds 1 and 3 printed the same; its build 2 passed): recorded, not tonight's job. Every other step PASS, among them step 24 (the AUTOPILOT on DRYDOCK) |
| -skygrindGolden | `golden total: 39 vector sets (48094 records), 22 replays (22 exact); 0 failing, 0 mismatches` | pass |
| -skygrindFingerprint | `seed 1: 12 hashes over 7200 steps in 0.38 s: EQUAL to the baseline; at the end: run Fight round 4, kills 36, shots 80, earned 5102, alive 14` | pass (tonight's baseline, in the Player) |
| -skygrindReplay (the smoke's tape) | `EQUAL (1 of 1 checkpoints, 485 steps); seed 737373, Stopped, stamp SKYGRIND 0.1.0 (e54fa29-dirty)` | pass (tape format 2) |
| -skygrindStrip | 6 sheets in `Logs/shots/n16-b1-strips/` (freeze, punch, parry PERFECT, slowmo, slide, swap) | reviewed below |
| the tour | 7 captures in `Logs/shots/n16-b1/` | reviewed below |

Captures and strips so far: 20 of 40 (7 + 6 + 7).

**The strips, two lines each** (what a first-time player feels in the first second / what is still wrong):
- **freeze** (a headshot kill, KESTREL): `STEP 69 HELD` from +18.9 to +69.8 ms, steps resume at +79 ms (the 0.08 s tier), the frame
  brightens on frame 1 and the struck head stays white through the hold. *Feels:* the shot lands with a clear stop. *Wrong:*
  the strip's framing hides the target behind the raised KESTREL (the tool's aim, not the game).
- **punch** (a punch kill): `STEP 210 HELD` all 10 frames (+0.7 .. +78 ms of the 0.09 s), the husk white. *Feels:* a hard
  stop. *Wrong:* **the left fist never shows**: the punch hits on its press step, the world (and the viewmodel's arm, on
  WorldDt) freezes at punchAge 0. Sent to V (15:20) as the priority fix: the arm's out-travel on frame time inside a freeze.
- **parry** (PERFECT): `STEP 401 HELD` for 92 ms (of 0.30 s) under the web's white parry flash, the husk's wind-up glow
  frozen. *Feels:* the big moment is big. *Wrong:* the same missing fist and knuckle flash (V's fix).
- **slowmo** (the round's last kill): steps every ~30 ms (1044 -> 1046 in 75 ms: 0.3 x of 120 Hz), the round tally comes in,
  the vignette deepens from frame 5. *Feels:* a beat. *Wrong:* the vignette's edge read as a hard dark arc: `cb14e82` 0.12 ->
  0.07.
- **slide**: a step a frame, the camera drops into the slide; under the crosshair the 3 pip ticks and the longer slide tick
  (dimmer once spent; a 2x crop of frames 4 and 10). *Feels:* the slide reads, and the spent charge is where the eye is.
  *Wrong:* the ticks are faint at 1080p; the heel dust is behind the camera (by design: first person).
- **swap**: **unusable**: it gave every catalog gun (8 unported, no rig), the god-mode run filled the frame with husks from
  DISCUS on, and 10 frames (+0 .. +90 ms) show only the holster (the gun changes at 125 ms). Sent to U (15:21): kit guns only,
  no spawns, 36 frames at gap 2, a `draw` scenario, the fist's framing for punch / parry, a `land` scenario.

**The tour, before / after:**
- TITLE (`n15-title.png`): the MAP group now has its sub-line ("A SHIP'S RIBS, RAMPS AND A MAST"). *Feels:* the new choice
  explains itself. *Wrong:* nothing new.
- The hub (`n15-drydock-hub.png`): the hotbar reads KESTREL / PULSE (whole words); the two first-run toasts share one type
  and a 14 px gap; the rank letter, name and bar are hidden at a run's start. *Wrong:* the rank's skewed box stayed, an
  empty dark plate mid-right: `cb14e82` fades it with the rest.

### After build 1 (15:20-15:29)

- `SimHarness spam Design/maps/drydock.json` in the main tree (rules on): `spam: targets (a) (b) (c) HOLD`; the skilled lines
  `dash_jump: first airtime 21.20 m`, `slide_jump: ... 8.93 m`, `wall_ride: ... 7.61 m up`, `slipway lane A slide: peak
  28.81 m/s`, `launch rail: 3 of 6`; `verbs Design/maps/drydock.json`: `launch rail: 3 of 6`, `ribs wall-ride: 1.20 s, max
  y 7.68`, pads within 0.07 m, `deck lip dash-jump: 3 of 4`.
- **V's follow-ups** (10 min, branch `v-polish-n16` from port/stage3: its old branch's cherry-picks made `--ff-only`
  refuse): `81d2d10` tests first (#6 / #14) and `e250ac3` test first (the fist): red in the Editor (`HandsTests`: the quick
  double swap popped `21.97` deg of roll; HandsIdle / HandsArm.AdvanceOut NotImplementedException); `c1e301e` **the fist is
  out in the freeze** (its out-travel on max(worldDt, frameDt): full by frame 9 at 144 fps inside a freeze, held; the hold
  and return on world time; the knuckles light when the fist arrives; the gauntlet's jab the same), `715d9e9` polish #6 (the
  quick-swap flick: each turn of a swap starts from the pose drawn last frame; a swap inside 0.4 s of a draw flicks aside at
  40 % depth), `e31aae9` polish #14 (the idle flourish after 8 s without input in a break; the kiosk's idle clock and the
  viewmodel's own); green `tests: 26 / 26 passed` (15:27).
- **U's follow-ups** (5 min): `0a46b9e` tests first (red `12 / 15`, NotImplementedException), `1217138` the strip tool (kit
  guns only, the break held and husks frozen, swap 36 frames at gap 2, the `draw`, `punch-press` and `land` scenarios; the
  melee camera turned 12 deg LEFT of the husk, not right as asked: the husk then sits right of centre, out of the left
  fist's path: accepted); green `tests: 15 / 15 passed` (15:28).
- **15:28 the reviewer launched** (read-only, 45 min; the brief "where would this feel bad?").
- **15:29-15:36 the suite:** `tests: 794 / 794 passed, 0 failed, 0 skipped`, `replays: 22 / 22`, `golden tests: 96 / 96`.
  `9f187fd` ZeroAllocTests gains ImpactRules (600 frames of the loop's calls and the bus's moments: `1 / 1 passed`).
- **15:20-15:33 the farm's AFTER half** (build 1, the slide rules on): `farm 100 runs, normal, seeds 1..100, brain botBrain:
  mean round 10.06, max 22, timeouts 15; causes: ACID 15, HUSK 34, SPITTER GLOB 13, SWARMER 23, timeout 15; 753.9 s wall`.
  **The diff** (`node Tools/farm-report.cjs diff`, `Logs/FARM-n16.md`): mean round 9.52 -> 10.06 (+0.54), median 6.0 -> 7.5,
  timeouts 13 -> 15; the survival curve +4 to +7 points from round 7 to 11; kills by gun shotgun 42 -> 38 %, revolver
  29 -> 33 %; deaths to ACID 24 -> 15. An observation for GG, not a gate: the AUTOPILOT (BotBrain) presses slide rarely, so
  the slide rules mostly move a run by changing its path (a run is chaotic in its inputs), not by slowing the bot.
- **15:37 build 2** (`9f187fd`): `Succeeded`, 0 errors, 20.4 s; ProjectSettings: only UnityConnectSettings.asset. Its
  checks run with the smoke's pad-wheel capture (`-skygrindPadShot`) and the new strip scenarios.

### The reviewer (15:28-15:39) and its findings

Read only, the brief "where would this feel bad?", two forks of its own on the hands and the UI. **Clean (read end to
end):** no eaten input (every press latched each frame and consumed by the next step, frozen or not; the look live), the
loop's timing (frozen time skipped, not caught up; a slow-mo only fills the accumulator slower), determinism (the rules off:
bit-exact; tape format 2), allocation (none per step or frame in the new code), the strip tool's cleanup. **Findings and
what was done** (each fix tests first where it is logic, red then green in the Editor):

| # | Finding (the reviewer's file:line) | Fix | Commit, evidence |
| --- | --- | --- | --- |
| 1 | UiFeelHud.cs:159 `GetComponent<CanvasGroup>() ?? AddComponent`: Unity's fake null in the Editor: AddComponent never ran, the next line threw while the UI was built: **Editor Play broken** (Players get a real null, so every Player check passed) | TryGetComponent | `3868feb` (the lead); Editor compile clean |
| 2 | **An uncharged slide roots the player** (Mover StartSlide + EntryFloor = min(floor, entry)): from a standstill 0 m/s with WASD dead; from a slow walk locked at that speed | an uncharged slide's entry and base never below the walk floor (walkSpeed x slideSpeedMult) | M `3462e89` tests first (red: 0.0 m/s, 3.0 m/s, `missing slideCharge`), `78372e4`; MoveBalanceTests 18 / 18; CoreHarness 0 mismatches, 22 exact; the fingerprint EQUAL (no new baseline) |
| 3 | The MODS draft swallows the round-clear slow-mo, which plays after the pick at the break's start (reads as lag) | the draft opens after the beat | `3868feb` (the lead) |
| 4 | A 3-hitch chop on automatic headshots and in a horde (the per-tier cooldown + the rolling budget) | a shared gap `design.impact.cooldown.any` 0.5 s between any two freeze starts (0.25 s still let 3 fit in a second); the non-kill headshot / heavy tiers on a shot's first hit only | I `197ec15` tests first (red 15 / 22), `7cc37e6`, `bb00cd8` (the ledger prints the rhythm); ImpactRulesTests 22 / 22; ledger: arena 978 ms / min, at most 2 starts in any second, shortest gap 0.507 s; DRYDOCK 990 ms / min, 0.514 s; budget HOLDS. A follow-up sent (16:00): a strictly higher tier may pass the gap (a body kill must not swallow the headshot kill after it) |
| 5 | The gun's recoil held still through its own shot's freeze (`recoil.Update(animDt)` on WorldDt) | the newest kick rises on max(world, frame) time to its peak, holds and returns on world time | V `97f7a0c` tests first, `42617e0`; HandsTests 30 / 30 |
| 6 | The automation played the player's saved HIT FREEZE / SLOW-MO / REDUCED MOTION | `noLab && !attractOn`: design.impact.settings' defaults | I `7cc37e6` |
| 7 | Flashes: the draw glows ignored FLASH INTENSITY; two separate 3-a-second gates; the HUD's parry flash only noted | one game-owned `FlashGate` (`3e318c1`, the lead) used by the impact flash, the HUD's parry flash, the knuckles, the glint and the draw glows; the glows x flashIntensity | I `7cc37e6`, V `42617e0` |
| 8 | Parry chains: no cooldown, exempt from the budget (~46 % of real time frozen possible) | `cooldown.parry` / `perfectParry` 1.25 s; a repeat inside it gets the web's `punch.parryHitstop` 0.06 s, budgeted | I `7cc37e6` (a test: a 20 s perfect-parry chain stays under 30 % frozen) |
| 9 | A NIGHT15 save can't resume in this build, and is deleted (two new MoverState fields, SaveVersion 1, the reader strict) | `slideCharge` and `moveBalance` optional when absent (MoverState.OptionalSinceNight16) | M `78372e4` (a NIGHT15-style snapshot loads; a missing other field still throws) |
| 10 | Back to TITLE mid-freeze left the title's world frozen or slowed | `ImpactToTitle()` on the first line of ToTitle | I `7cc37e6` |
| 11 | The press dip never played (a disabled component gets no UGUI events); the LAB note sat on the description; SLIDE CHARGE's row said 1 s; RESULTS / MODS rose with REDUCED MOTION; REDUCED MOTION didn't damp the KESTREL's spin or the THUNDERCLAP's roll | UiPress stays enabled (raycast rect widened by the dip); one auto-sized note line; the row's text from the config; no rise with REDUCED MOTION; the spin / roll scaled | `3868feb` (the lead), V `42617e0` |

### Build 2's checks (15:38-15:48)

| Check | Output |
| --- | --- |
| smoke | `[smoke] 184.6s DONE: 2 step(s) failed`: steps 18 and 21, the residual word for word (`... differs at checkpoint 6`); `PASS tape: 485 steps taped (4328 B), replayed in this process: EQUAL` |
| -skygrindGolden | `golden total: 39 vector sets (48094 records), 22 replays (22 exact); 0 failing, 0 mismatches` |
| -skygrindFingerprint | `seed 1: 12 hashes over 7200 steps in 0.37 s: EQUAL to the baseline` |
| -skygrindReplay | `EQUAL (1 of 1 checkpoints, 485 steps); seed 737373, Stopped, stamp SKYGRIND 0.1.0 (9f187fd)` |
| strips | 9 sheets in `Logs/shots/n16-b2-strips/` + the pad wheel `Logs/shots/n16-b2/n16-pad-wheel.png`: captures 30 of 40 |

- **punch** (a punch kill): **the fist now shows**: in from the lower left at frame 3 (+17.7 ms), full by frame 6 (+43.5 ms),
  held through `STEP 211 HELD`, the husk right of centre. *Feels:* the punch has a hand and the stop holds it. *Wrong:* the
  forearm is long (corner to near the centre's lower half), out of the centre box.
- **parry** (PERFECT): the fist out from frame 4 and held through `STEP 555 HELD` under the white flash. *Feels:* the parry
  lands. *Wrong:* the knuckles' own white can't be told apart under the full-screen parry flash at this size.
- **draw** (THUNDERCLAP, KESTREL; 40 frames each; a 2x crop of the settle): the outgoing gun tips to the hip, the new one
  rises from ~+176-192 ms, the THUNDERCLAP's pump shifts through the settle, the KESTREL flicks and settles. *Feels:* each
  gun arrives as itself. *Wrong:* the strip shows ~6 frames with no gun at all between the two (the sim's lowered half).
- **pad wheel**: the dim disc behind the wheel; a zone sign (THE GRINDER 1250) still read through it at 0.72: `9a9669c` 0.86.
- **land** (a 22.4 m/s landing): the camera came down beside a SHOCK PLATE switch and the ring isn't visible (0.3 effective
  alpha on the pink floor): `9a9669c` the ring's alpha 0.5 -> 0.8; to be seen in build 3.

### After the review (15:50-16:05)

- I's follow-up (the lead's refinement of finding 4): `5e9b35a` tests first (red `22 / 24`), `0525c14` a strictly higher
  tier may start inside the 0.5 s gap, once per gap (green `24 / 24`; the ledger: arena 1031.7 ms / min, at most 2 starts in
  any second, the shortest gap 0.437 s; DRYDOCK 1074.5 ms / min, 0.500 s; `impact PASS`); the fingerprint EQUAL.
- `9a9669c` the pad wheel's backdrop 0.86, the landing ring 0.8; `3b212d2` polish #18 the crosshair ticks 4 px, a 0.3 track;
  `3e318c1` polish #17 the slide charge's first-time hint (and the shared FlashGate).
- **15:58-16:04 the suite:** `total 813, passed 813, failed 0`, replays 22 / 22, golden 96 / 96.
- `10efdb5` the tour's UI pass (`-skygrindShotsUi`: SETTINGS' ACCESS and LAB tabs, PAUSED, DOWN, RESULTS).

### Build 3 (16:05; `10efdb5`; checks 16:06-16:13)

| Check | Output |
| --- | --- |
| build | `Succeeded`, 0 errors, 24.5 s; ProjectSettings: only UnityConnectSettings.asset |
| smoke (first run, 16:06) | `[smoke] 251.6s DONE: 5 step(s) failed`: `round 6: 0 spitter(s) alive at once, 0 volley(s) in 45 s` (new), `virtual pad`, `virtual pad menus`, `pad only` (as the n15 build's smoke at 14:20), `attract ... differs at checkpoint 6` (the residual); step 18 `clean slate ... EQUAL to the baseline` passed. The machine was in use (`idle s: 53` at 16:12) |
| smoke (re-run, 16:13) | `[smoke] 183.7s DONE: 1 step(s) failed`: only step 21 `attract ... the next run differs at checkpoint 6` (the residual); `round 6: 1 spitter(s) alive at once, 1 volley(s) in 5 s` PASS, the pad steps PASS, 88 PASS lines. **Verdict:** the first run's four extra failures were the environment (the Player losing the focus with someone at the machine; the pad steps need it), not the build |
| -skygrindGolden | `golden total: 39 vector sets (48094 records), 22 replays (22 exact); 0 failing, 0 mismatches` |
| -skygrindFingerprint | `seed 1: 12 hashes over 7200 steps in 0.47 s: EQUAL to the baseline` |
| -skygrindReplay | `EQUAL (1 of 1 checkpoints, 486 steps); seed 737373, Stopped, stamp SKYGRIND 0.1.0 (10efdb5)` |
| strips | freeze, land, punch-press (`Logs/shots/n16-b3-strips/`); the UI pass (`Logs/shots/n16-b3/`, 5): captures 38 of 40 |

- **freeze** (a headshot kill): the KESTREL kicks up by frame 2 (+18.8 ms) and **holds at the top of its kick through the
  HELD frames** (the review's finding 5 fixed), the struck husk white; steps resume at +79.6 ms. A 2x crop of frames 1-4 at
  the crosshair: the kill marker's four red-orange arms with **the hollow head diamond at the centre**, then its burst.
  *Feels:* the shot lands in the gun and on the target at once. *Wrong:* nothing new seen.
- **punch-press** (24 frames from the press): the fist out by frame 2 (+34 ms), held through the HELD frames (to ~+92 ms),
  then back down-left over frames 14-24 as the steps resume. *Feels:* a jab with a follow-through. *Wrong:* the forearm is
  long; GG's call.
- **land**: the scenario lands beside a SHOCK PLATE switch with the feet below the frame: the ring is still not seen in a
  capture (a follow-up asked of U: aim the land camera at the landing point).
- **The UI pass:** ACCESS tab: HIT FREEZE FULL and SLOW-MO ON read with the web's row grammar. LAB tab: the note one line
  under the nine rows, clear of the description (finding 11 fixed). PAUSED: **a zone sign (THE GRINDER 1250) read through
  the pause menu between its buttons**: `8b709f1` polish #19 hides the signs behind PAUSED (NIGHT13 hid them behind
  RESULTS, DOWN and TITLE). DOWN: WHAT GOT YOU with the killer named. RESULTS: reads; its BEST MOMENTS line lists a
  LONGEST SWING the run can't have yet (the web's line: noted, not changed).

### After build 3 (16:14-16:45)

- `92b96c6` the held-slide crawl written up as a decision for GG (FEEL-PLAYTEST.md question 1, the report).
- `3f54cf7` (builder U) the strip's `results` scenario (run only when named, so the full run's capture count stays) and
  the land camera at 58 deg down. Both run on the final build: the last two captures of the 40.
- `58d3b7c` PORT-STATUS (u3-n16); `8f940cf` FEEL-DESIGN.md section 5's state table.
- **A sweep of the web's feel not yet ported** (player.js, hud.js, viewmodel.js against the port; read only) gave six queue
  entries, 23-28 in FEEL-DESIGN.md section 5:
  - `2cad6fc` #23 the rank-up camera kick (player.js :207-210, `style.kickDeg` 0.8 over 0.04 / 0.22 s). CameraFeelTests
    2 / 2, written with the code (not seen red first). Not seen in a capture or in play (0.8 deg is the web's own size).
  - #24 the barrier-touch pinch: **left** (the web's `barrierTouch` comes from barriers.js, which isn't ported).
  - `3a431b3` #25 the crosshair's spread (hud.js :436 / :511 / :885-890): the arms open `hud.crosshairDashSpread` 2 px on
    a dash and `hud.shotgunSpread` 6 px after a THUNDERCLAP shot, back over `hud.spreadRecoverTime` 0.2 s. Measured in
    Play check 1 (below).
  - `2ec7b82` #26 the rank-up pop (hud.js rankUp :464-477 on `rankChange` up :489): the letter 1.4 -> 0.92 -> 1, the
    name slid in 30 px -> -4 -> 0 over `style.popTime` 0.26 s, eased by CSS ease-out as WAAPI does (the hud.js keyframe
    literals in UnityTuning.FeelPolish.cs). Not ported: the name's fade and skew, the screen-edge glow (#29). Measured
    in Play check 1 (below).
  - #27 / #28 (builder V, worktree from `2cad6fc`): the momentum hit's hold spring (viewmodel.js :654-656 / :665-670 /
    :740 / :875-878 / :956) and the forge flourish on the gun in hand (:658-663). Tests first `3f07413`
    (cherry-picked 16:36): Editor `HandsTests 28 / 33 passed, 5 failed` (the three new tests and both alloc tests:
    `NotImplementedException : U3-29.8 HandsImpactHold / HandsForgeFlourish`); the ports `538ea24`: `33 / 33 passed`.
    Not seen in a strip (the cap's last two captures are the final build's results and land strips). A forge of another gun shows nothing now
    (the port draws only the gun in hand's spring bank).
- **The NIGHT14 residual, found and fixed** (builder M, worktree from `3f54cf7`; the smoke's steps 18 / 21, intermittent
  since NIGHT14): two values StartRun's clean slate didn't reset.
  - The aim assist's scan clock: `AimAssist.Step` scans when `steps % aim.lock.scanEvery (2) == 0`, and `steps` ran on
    across runs, so after an odd number of played steps every scan of the next run came one step late (the lock, the look
    turn): `first difference at checkpoint 6`, the smoke's signature. Frame-driven step counts made it intermittent.
    `AimAssist.NewRun` (port-only; the web's `reset` unchanged) from `SurvivalSim.CleanSlate`.
  - The director's per-step accumulators: a hurt or a nearby kill after the director's last Step of a run stayed in
    `damageTaken` / `nearbyKills` for the next run's first fighting step. `Director.NewRun` zeroes both.
  - Tests first `f0a256f` (cherry-picked 16:36): Editor `ResidualTests 0 / 2`: `after a 2401-step attract: first
    difference at checkpoint 6`, `after a death: first difference at checkpoint 1`; SimHarness `residual check: 2 of 7
    EQUAL` (M's run). The fix `51bc000`: `ResidualTests 2 / 2`; at 16:40 on the main tree: CoreHarness `golden total: 39
    vector sets (48094 records), 22 replays (22 exact); 0 failing, 0 mismatches`; SimHarness `fingerprint seed 1: 12
    hashes in 0.59 s: EQUAL to the baseline`, `twice: run 1 vs baseline EQUAL; run 2 vs run 1 EQUAL (12 of 12)`,
    `residual check: 7 of 7 EQUAL`. The Player smoke's steps 18 / 21 on a build: the final build's smoke.
  - Left as they are (M's report; none changes a hashed system over the 7200 steps): clocks read only as differences
    (loadout, powerUps, objectives, `sim.simStep`); fields the next round or lock overwrites before reading them (the
    director's `round`, `squadsSpawned`, `cooldown`, `behindDone`; the assist's last-lock fields; nav's goal and field
    cache; mods' `offerRound` and stat lists; the mover's collision scratch; contracts' last position); telemetry counters.
    `AimAssist.scanId` only ever rises, on purpose (pooled candidates' rejected stamps can't match by accident).
- **16:39-16:47 the suite** (after `51bc000`): `tests: 820 / 820 passed, 0 failed, 0 skipped`, replays 22 / 22, golden
  tests 96 / 96.
- #29 the rank-up screen-edge glow: **left** (FEEL-DESIGN.md section 5: it glows in the rank's colour, and S / SS are
  red-orange / magenta: rule 8).
- `70c102e` #30 **the "+60" points pops** (hud.js :524-535 / :918-932): the web's CoD-style points pop at the crosshair
  was never ported (no `points.pop*` reader in the port before tonight). A pool of `points.popPool` labels in the sight
  layer, the rank's colour, the round's bonus big and gold.

### Play check 1 of 2 (16:50-16:52; Editor, idle 156 s, on AC; no captures)

One eval coroutine (`pc1-n16.cs`, scratchpad): a god-mode run, the style meter set D then B, then two `points` events,
then a THUNDERCLAP `fire`; values read from the live UI by reflection each frame.

| Entry | Output |
| --- | --- |
| #26 rank pop | `rank 2 'B'; scale max 1.318 min 0.929 end 1.000; name dx max 24.6 min -3.5 end 0.0`; frames `18ms s1.318 x24.6; ... 132ms s0.929 x-3.5; ... 255ms s1.000 x0.0` (popTime 0.26 s) |
| #30 points pops | `31 ms: '+66' #19D3E6 a1.00 y36 x-1 s1.19 22px; '+100 ROUND' #FFE6A8 a1.00 y72 x19 s1.19 30px`; `608 ms: ... a0.72 y69`; `754 ms: ... a0.17 y77`; `950 ms: 0 live` |
| #25 crosshair spread | `rest -13.0; after the shot: 14ms -18.6; ... 101ms -16.0; ... 200ms -13.0` (6 px, recovering over 0.2 s) |

*Feels:* not judged by eye (no capture left before the final build's two). The numbers follow the web's keyframes and
timers.

### More of the sweep (16:52-17:10)

- `894a47a` #31 **the damage vignette** (hud.js .hud-damage: dark red edges eased toward (1 - hp / max) x
  `regen.vignetteMax` at `regen.vignetteEase`; never ported: no reader of `regen.vignette*` in the port before).
- `eb1963b` #32-#34 (one commit: they share `UiFeelEdges.cs`): **EXPOSED's pulsing edge** (an inset box shadow as a
  9-sliced Gaussian sprite), **the buy prompt's poor / denied states**, **the regen sheen and the REGEN tag**.
- 16:59 the Editor's main thread timed out a refresh eval (unfocused); it compiled on its own at 17:00:17
  (`compilationFailed: false`, `consoleErrors: 0`; the DLL holds `RenderFeelEdges`). `set_autotick` re-sent 17:07.
- 17:05-17:11 someone at the machine: idle samples `0, 0, 0, 0, 0, 0` then `1.4, 0, 1.3, 3.3`. Play check 2 ran without
  `editor_focus` (auto-tick on) so as not to take the focus.
- Builders resumed: U (`afa0603`) for #35 the web's move row (the speed readout and the wall-jump ticks); V (`a1bc30f`)
  for #36 the web's ADS HUD (the ring, the LONGWAVE's scope, the rangefinder, the crosshair fading back).

### Play check 2 of 2 (17:12-17:14; Editor, not focused; no captures)

`pc2-n16.cs` (scratchpad): a run, HP set to 40 %, then the director's camp held over `exposedAt` for 1.5 s, then the
player at the THUNDERCLAP's wall buy with 0 points and the buyables' denied clock set.

| Entry | Output |
| --- | --- |
| #31 damage vignette | `at 40 % HP (target 0.30): 101ms a0.13; 307ms a0.23; 610ms a0.24; 1023ms a0.19` (regen began at 1.0 s: `hp 66 %`); `full HP after 1.2 s: damage a0.00` |
| #34 regen | `Regenerating True after 1.0 s`; sheen `35ms u0.158; 175ms u-0.052; 336ms u-0.293; 471ms u-0.496; 621ms u0.479; 781ms u0.239` (0.8 s period); `tag active True 'REGEN' #8FF3FF sheen #BFF8FF`; full HP: `regen False, sheen on False, tag False` |
| #32 EXPOSED edge | `Exposed 128 of 128 frames; alpha min 0.00 max 0.70 (0.35 +- 0.35, 7 rad/s); edge #FF280A sliced Sliced`; released: `Exposed False, alpha 0.00` |
| #33 buy prompt | `THUNDERCLAP (wanted THUNDERCLAP, price 750); points 0; prompt ... #FF9A82; bar back #FF4A1C a1.00`; denied `14ms 0; 28ms 2; 41ms 2; 58ms 4; 69ms 4; 85ms 2; 98ms 2; 109ms 0; ... 391ms 2; 405ms 0; 422ms 0 ...` |

The damage vignette never reached its 0.30 target: regen began 1 s in and raised the HP (expected: the web's regen
delay and rate). Play checks: 2 of 2 used.

### The builders' last two (17:15-17:19)

- **#35 the move row** (builder U, `u-moverow-n16` from `1ed1ec8`): the web's speed readout (`game.mover.Speed`, the
  horizontal speed as mover.js's `speed`; white under `hud.speedMid`, cyan from it, `colors.enemy` above `hud.speedHigh`;
  SetText only on a tenths change) and the wall-jump ticks (`wallJump.maxPerAirtime + perks.grip.wallJumps`, on while
  `i < mover.wallJumpsLeft`, shown only airborne) under the HP plate's stamina segments; the plate grows 10 px down (its
  bottom 12 px from the screen edge, was 22). Tests first `cad255e`: Editor `MoveRowTests 0 / 3` (NotImplementedException);
  `8dde864`: `3 / 3`.
- **#36 the ADS HUD** (builder V, `v-ads-n16` from `1ed1ec8`): the ring for a ring gun (opacity k, scale 1.6 - 0.6 k), the
  LONGWAVE's scope (the radial gradient to `ads.scopeVignette`, two 62 vh lines), the rangefinder (the camera's ray against
  `game.world.RayIntersect` and `game.enemies.Raycast` within `weapons.rail.range`, read-only, shown scoped at k > 0.5),
  the crosshair arms at 1 - 0.6 k; k = the new read-only `Viewmodel.AdsAmount`. Tests first `418e8f3`: Editor
  `UiAdsTests 0 / 6`; `66dbdea`: `6 / 6`. The cherry-pick conflicted with #35 on the hook line beside
  `RenderPointsPops` (both kept, by `resolve-ui-hooks.cjs` in the scratchpad).
- Neither is seen on screen: Play checks 2 of 2 and captures 38 of 40 are spent (the last two are the final build's
  results and land strips).
- A layout note for GG (read from the code, not seen): V's range label (its top 24 px under the centre, LONGWAVE scoped
  only) and U's movement ticks (30 px under the centre, shown while a dash or the slide charge is spent) can overlap
  while scoped. The web had no ticks there.

### The last of the hud.js sweep (17:20-17:35)

- #37 **the crosshair tag** (hud.js .hud-tag :83-84 / tag() :578-582 / :669-670; main.js :2239-2241): PRIMED, the
  ARSENAL's combo (COLLAPSE, OVERLOAD) or CROSS in the gun family's colour, SWAP, under the crosshair for 0.7 s. The
  ARSENAL's `primed` / `crossHit` / `swapKill` fired in Core with no UI until now (the style meter scored them unseen).
- #38 **the charge edge** (hud.js .hud-charge-edge :170 / :836-837; player.js :347-349): the LONGWAVE's charge tints the
  screen's edge in its family's colour (pierce, #eaf8ff) at charge x `hud.chargeEdgeOpacity` 0.55. The web's THUMPER
  wind-up half isn't ported (no THUMPER).
- #39 the style multiplier chip ("x1.2 AIR" by the rank; hud.js .hud-mult :185 / :607-612): **left**. Its place in the
  port's style block is under the rank bar, where the award feed's box (`Ui.cs` ~:367) also reaches; without a capture
  (spent) its overlap can't be checked.
- `0140eec` #37 / #38: Editor compile clean at 17:29:33 (`compilationFailed: false`, `consoleErrors: 0`).
- **17:19-17:30 the suite** (the code at `66dbdea`: #35 and #36 in): `tests: 829 / 829 passed, 0 failed, 0 skipped`,
  replays 22 / 22, golden tests 96 / 96.
- **Headless after the residual fix** (17:27): `impact PASS: the budget holds on 2 map(s)` (DRYDOCK: 1074.5 ms frozen per
  minute, 1.79 %; worst rolling second 120.0 ms of 120.0), `spam: targets (a) (b) (c) HOLD`.
- PORT-STATUS.md's UI row: tonight's hud.js ports, the Unity-only feel UI, and what's left with its reason.
- #40 HUD STYLE: **left** (FEEL-DESIGN.md section 5): the web's default ATTACHED makes the bottom-right block ride the
  gun's sway; the port loads `SettingsView.hudStyle` and uses it nowhere, and (U's sweep, 17:47) the row isn't in
  UiSettings' `Shown`, so the port is always CLASSIC. Moving the points and weapon block as one means reparenting live
  UI in two layers, which nothing could run tonight. HANDOFF section 6.
- 17:35 U and V resumed, read-only: the web's UI (U) and feel outside the HUD (V) still missing from the port, ranked,
  for the next session's queue.

### The sweeps (17:35-17:48) and their quick rows (17:48-18:05)

- U's sweep (the web's UI: 43 rows) and V's (feel outside the HUD: 32 rows), read only, are condensed in FEEL-DESIGN.md
  section 6. U corrected entry 40: the HUD STYLE row isn't even shown in the port (always CLASSIC). V found: Core
  allocates per combat event (PORT-RULES 6), the rank colours vs rule 8 for the web's player-side effects, the older
  flashes outside the gate (HANDOFF D-70, D-71).
- Nine small rows taken, test-proven where there's logic (FEEL-DESIGN.md section 5, entries 41-49): V `v-fixes-n16`
  (#41 particle sizes, #42 STEADY's kick, #43 REDUCED MOTION on the flinch and the shells, #44 no mid-air sparks on a
  THUNDERCLAP full miss, #45 the spawn beams' own pulses); U `u-small-n16` (#46 the award feed, #47 the rank bar's pulse,
  #48 R from PAUSED, #49 RETRY (R)).
- Integrated tests first (17:58-18:02): `0f43369` + `216a804` cherry-picked, the Editor compile clean, red: FxGatesTests
  `0 / 3`, HandsTests `33 / 34` (`SteadyScalesTheKickAndThePunch`: NotImplementedException), SmallUiTests `0 / 6`; then
  `6c334ac` .. `ae1cb73`: FxGatesTests `3 / 3`, HandsTests `34 / 34`, SmallUiTests `6 / 6`, FeelHudTests `15 / 15`,
  MoveRowTests `3 / 3`. SimHarness after them: `fingerprint ... EQUAL to the baseline`, `twice ... EQUAL (12 of 12)`,
  `residual check: 7 of 7 EQUAL` (no Core change). None seen on screen.

- **18:04-18:12 the suite** (the code at `ae1cb73`: #41-#49 in): `tests: 839 / 839 passed, 0 failed, 0 skipped`, replays
  22 / 22, golden tests 96 / 96.
- **The second batch** (V `v-fixes2-n16`, U `u-small2-n16`, 18:05-18:11): integrated 18:12-18:14 tests first: `0a94016`
  + `89d58db` red (FxGatesTests `3 / 7`, SmallUi2Tests `0 / 3`), then V's `a89b82a` #50 the particle cap, `d8f8f01` #51
  the impact ring's burst, `e6386ea` #52 the melee family sparks, `d51cc70` #53 the death burst height, and U's `dc8935b`
  #54 the fades (with the pure rules for #55-#57): FxGatesTests `7 / 7`, SmallUi2Tests `3 / 3`, SmallUiTests `6 / 6`,
  FeelHudTests `15 / 15`, HandsTests `34 / 34`.
  - **Not integrated: U's `91caa6b` (#55 / #57) and `99db5e1` (#56).** They change UiNav's focus and PAUSED's card, the
    paths the Player smoke's virtual-pad steps drive ("virtual pad menus", "pad only"); the final build is the only one
    left (builds 3 of 4) and both Play checks are spent, so nothing could try them before GG's A/B build. Ready on
    `u-small2-n16` for the next session.
- The last batch (18:15, a 20-minute box): V #58 OVERLOAD's burst (a stand-in for the web's lightning) and #59 the muzzle
  light at the muzzle; U #60 the mod chips and #61 RESULTS' target line.
- **18:15-18:23 the suite** (the code at `dc8935b`): `tests: 846 / 846 passed, 0 failed, 0 skipped`, replays 22 / 22,
  golden tests 96 / 96.
- **The last batch integrated** (18:23-18:25) tests first: `8403d8f` + `233e2e2` red (FxGatesTests `7 / 9`, ModChipTests
  `0 / 2`), then `dfc354d` #59, `e89647d` #58, `213bdb1` #60: FxGatesTests `9 / 9`, ModChipTests `2 / 2`, HandsTests
  `34 / 34`. #61 left (U: RESULTS is UiResults' tally by default, and the "k MORE" has no source there). **The code is
  frozen at `213bdb1`** for the final build.
- **18:25-18:33 the suite on the frozen code** (`213bdb1`): `tests: 850 / 850 passed, 0 failed, 0 skipped`, replays
  22 / 22, golden tests 96 / 96.
- **18:36 headless on the frozen code** (HEAD `9b774f6`, code as `213bdb1`): CoreHarness `golden total: 39 vector sets
  (48094 records), 22 replays (22 exact); 0 failing, 0 mismatches`; SimHarness `newmap PASS autopilot on drydock: seed
  4242 normal (botBrain) twice for 300 s: round 8 / 8, cleared 7, 154 kills, cause timeout, spent 9750 of 17733; 60 of
  60 hashes equal`.
- 18:26-18:32 a self-review of the riskiest integrated diffs (the reviewer's cap is spent), no change needed: the
  particles' scale (`size x scale x (1 - age / life)` = particles.js :135's `(life / maxLife) x p.scale` on a `size`
  cube), the cap's level strings (static, so no per-frame re-cap), the feed's per-award line cache and its one
  `#RRGGBBAA` tag, the fades' CanvasGroup (UiKiosk never writes its alpha).

### U3-29.8: the two-line reviews of the sweep's entries (23-61)

Entries 1-22 are reviewed in the sections above (the builds' strips and checks). "Seen" means a Play check's numbers;
nothing of 23-40 was seen by eye.

| # | Commit | Review: what it gives a player / what could be wrong |
| --- | --- | --- |
| 23 rank-up kick | `2cad6fc` | A small nod marks a rank-up without moving the aim. / Not seen; 0.8 deg may be too subtle to notice at all. |
| 24 barrier pinch | left | Nothing (barriers.js isn't ported). / The pinch returns with barriers. |
| 25 crosshair spread | `3a431b3` | The crosshair tells you a shotgun shot or a dash spreads. / Seen: 6 px and back in 0.2 s; at 2 px the dash's spread may not read. |
| 26 rank-up pop | `2ec7b82` | A rank-up lands as an event, not a silent letter swap. / Seen; no fade or skew, so the name's slide-in is plainer than the web's. |
| 27 hold spring | `538ea24` | A momentum hit drives the gun in through the freeze: weight. / Tests only; a strip of a momentum punch would show it. |
| 28 forge flourish | `538ea24` | Forging the gun in hand answers with a kick and a gem flash. / Tests only; a forge of another gun shows nothing. |
| 29 rank-up glow | left | Nothing. / An S rank-up would have glowed danger red-orange (rule 8): GG's call. |
| 30 points pops | `70c102e` | Every kill pays visibly at the crosshair (the CoD loop). / Seen; many fast kills stack pops in a 40 px band, and pops plus tags may crowd the centre. |
| 31 damage vignette | `894a47a` | You feel hurt from the edges, and you see regen giving it back. / Seen; it stacks with the port's own low-HP floor and the post's darkening. |
| 32 EXPOSED edge | `eb1963b` | Camping is now a felt warning, not a word. / Seen; a 0-0.70 pulse at 7 rad/s, maybe strong next to the danger vignettes. |
| 33 buy poor / denied | `eb1963b` | "Can't afford" reads at a glance; a refused hold answers. / Seen; the hold bar's danger back at full alpha may be loud. |
| 34 regen sheen + tag | `eb1963b` | Regen is visible as it happens. / Seen; the web's low-HP sheen colour is left (the port's low fill isn't danger). |
| 35 move row | `8dde864` | Speed is a number; airborne, the wall-jumps left are shown. / Not seen; the HP plate grows 10 px toward the screen edge. |
| 36 ADS HUD | `66dbdea` | Zooming has a reticle; the LONGWAVE gets a real scope and range. / Not seen; the range label can overlap the movement ticks. |
| 37 crosshair tag | `0140eec` | The ARSENAL's combos finally say their names. / Not seen; it shares the centre with the pops and ticks. |
| 38 charge edge | `0140eec` | The LONGWAVE's charge is felt at the screen's edge. / Not seen; pale #eaf8ff at up to 0.55 might read as a flash. |
| 39 style chip | left | Nothing. / Needs a capture to place. |
| 40 HUD STYLE | left | Nothing; the port stays CLASSIC (no row). / The next session's first UI item. |
| 41 particle sizes | `6c334ac` | Blasts read bigger, hit sparks finer: the web's proportions. / Not seen; every hit's sparks change size, the biggest visual change of the batch. |
| 42 STEADY | `5a03771` | Ready for when STEADY is buyable. / Nothing visible today. |
| 43 reduced motion | `9b2ab75` | REDUCED MOTION now also stills the enemy flinch and the shells. / The white hit flash and the stagger stay, as intended. |
| 44 full miss | `675fd36` | No phantom sparks or light in the sky. / A wall exactly at the shotgun's range is treated as a miss (a tolerance in UnityTuning). |
| 45 spawn beams | `04d6589` | Several squads' beams pulse on their own clocks. / One draw per beam (they were separate objects already). |
| 46 feed | `2b2e4c5` | Awards read as the web's "+ NAME n" in the rank's colour; zero-point tricks show. / More lines in the feed; not seen. |
| 47 rank pulse | `3d3c192` | The bar breathes faster as the rank climbs. / Off with REDUCED MOTION; not seen. |
| 48 R from PAUSED | `57d4033` | A quick restart from the pause screen. / An accidental R while paused restarts the run (the web's behaviour). |
| 49 RETRY (R) | `ae1cb73` | The button names its key. / On a pad it reads RETRY (the pad has no restart action). |
| 50 particle cap | `a89b82a` | LOW holds 160 cubes in a big fight (the Deck's frame time). / The oldest cubes vanish first in a heavy fight on LOW. |
| 51 impact ring | `d8f8f01` | Every shock wave throws its 18 cubes. / Not seen; the web's additive material is still the port's alpha one. |
| 52 melee sparks | `e6386ea` | A GAUNTLET hit sprays cyan where it lands. / A punch's sparks are warm yellow (the web's `colors.spark`). |
| 53 death height | `d51cc70` | Small enemies burst at their middle. / 7.5-10 cm; barely visible. |
| 54 fades | `dc8935b` | Restarts and quits get the soft 0.15 s fade-in, not a hard cut. / The "out" half is skipped so nothing waits; not seen. |
| 58 OVERLOAD | `e89647d` | The combo finally has a picture: volt bursts on every enemy it hits. / A stand-in: the web draws lightning. |
| 59 muzzle light | `dfc354d` | The flash's glow comes from the gun, lower right. / Not seen; a gun with no rig keeps the old point. |
| 60 mod chips | `213bdb1` | Owned mods show by the perks, as the web's. / The fire tag's chip is plain text colour (rule 8). |

## The close (U3-29.10, from 19:18)

### Build 4, the final Release build (19:18; the code `213bdb1`, HEAD `065fa12`)

| Check | Output |
| --- | --- |
| build | `"result": "Succeeded"`, `"totalErrors": 0`, `"totalWarnings": 9`, `"buildTimeMs": 41752`; the Player's `Skygrind.Game.dll` / `Skygrind.Core.dll` written 19:18:47; `git diff --stat ProjectSettings`: only `UnityConnectSettings.asset` (never staged) |
| idle before the smoke | `19:19:50 idle samples (2 s apart): 1.1, 0.2` (someone at the machine) |
| smoke (19:20-19:27) | `[smoke] 408.3s DONE: 11 step(s) failed`: the power switch (`on False`), then what needs it (SHOCK PLATE `events []`, the objective hold, the four relays, THE PROTOCOL, the exfil radio, EXFIL `Paused`), `virtual pad`, `virtual pad menus`, `pad only` (TITLE `'CONTROLS' selected`), `attract: ... 20 s behind TITLE False (0 steps ...)`; no exception in the log. Step 18 `clean slate ... the first run EQUAL to the baseline` PASS; step 21's "next run" part `EQUAL to the baseline (12 of 12)` |
| smoke re-run (19:30-19:33; idle `5.8, 0`) | `[smoke] 206.4s DONE: 2 step(s) failed`: only `virtual pad` (`TITLE selected 'PLAY', A played True, the stick moved 0.4 m ... the trigger killed False`) and `pad only` (`Y held 3.01 s opened the wheel False ... the view held still False`), the pad steps that need the focus. The power switch `on True`, SHOCK PLATE `850 -> 160 ... events [powerOn, trapKill husk]`, `EXFIL at round 11 ... the PROTOCOL 4 / 4 complete True`, **`clean slate: ... 12 of 12 checkpoints equal, the first run EQUAL to the baseline`** and **`attract: started True, 20 s behind TITLE True (2365 steps, 7 kills, metaSilent True); stopped: released True; the next run EQUAL to the baseline (12 of 12)`**: 86 PASS lines. **Verdict:** the first run's 11 were the environment (frame-driven steps starved with someone at the machine, as build 3's first run); **the NIGHT14 residual's steps 18 / 21 pass in a Player** for the first time since NIGHT14 |
| -skygrindGolden | `golden total: 39 vector sets (48094 records), 22 replays (22 exact); 0 failing, 0 mismatches` |
| -skygrindFingerprint | `seed 1: 12 hashes over 7200 steps in 0.59 s: EQUAL to the baseline` |
| -skygrindReplay | `EQUAL (1 of 1 checkpoints, 486 steps); seed 737373, Stopped, stamp SKYGRIND 0.1.0 (065fa12-dirty)` |
| strips (40 of 40) | `n16-strip-results.png` (14 frames, gap 1, trigger flow RESULTS), `n16-strip-land.png` (12 frames, gap 1, fall speed 22.4 m/s), `Logs/shots/n16-b4-strips/` |
| the Silent series | **not run (SHORT):** 19:35 `powercfg`: **Balanced** (not Silent; the plan is GG's setting), AC on (100 %); someone at the machine (idle `5.8, 0` at 19:29); the close's suite running in the Editor |

- **results** (14 frames, +0.3 .. +127.4 ms, all HELD after the first): RUN OVER and the PAYOFF strip (THIS RUN, BEST ROUND
  1 · 24 POINTS SHORT, CLOSEST, NEXT UNLOCK) are up by +15 ms, the strip's text fading in over ~45 ms. *Feels:* RESULTS
  arrives at once, no hard cut. *Wrong:* the world stays undimmed behind small text (low contrast); the tally's cards
  aren't up by +127 ms (staged later).
- **land** (12 frames, +2.5 .. +123.1 ms, steps 152-166): the camera 58 deg down beside a SHOCK PLATE switch; **#35's move
  row on screen** (a 4x crop: "0.0 M/S" under the stamina segments, inside the HP plate: the horizontal speed after a
  vertical landing); a large pale disc on the floor unchanged over the 12 frames (the plate's pad, by its constancy).
  *Wrong:* the landing ring is still not clearly seen (the scenario lands on the plate's pad again): FEEL-DESIGN entry 4
  stays "not seen".

### The rest of the close (19:35-19:45)

- **19:34-19:42 the close's suite** (the code `213bdb1`): `tests: 850 / 850 passed, 0 failed, 0 skipped`, replays 22 / 22,
  golden tests 96 / 96.
- The four builder worktrees removed after `git status` showed no leftover file in any (`git worktree remove --force`);
  their branches stay.
- Docs: tasks.json (U3-29.7, U3-29.8, U3-29.10, U3-29 passing), this log, NIGHT16-REPORT.md, FEEL-DESIGN.md (entries
  4 / 35 updated from the strips), PORT-STATUS.md (u3-n16), FEEL-PLAYTEST.md, `Reports/NIGHT16-web-gaps.md`, HANDOFF.md
  (anchor `u3-n16`), NIGHT17.md (untracked: map night 2, with NIGHT16's carry-overs). Caps used: Play checks 2 / 2,
  captures and strips 40 / 40, Player builds 4 / 4, builders 4 / 4 (resumed by SendMessage), research agent 1 / 1,
  reviewer 1 / 1, bench runs 0 / 3 (the series not run).

## The close table

| Item | DONE / SHORT | Evidence ("done when", from this session) |
| --- | --- | --- |
| U3-29.1 Start | DONE | first `date` `Mon Oct 5 14:18:33 USMST 2026`; HANDOFF section 1 row by row in RECEIVED (HEAD `a691dd8`, CoreHarness `0 failing, 0 mismatches`, 22 exact; the suite run after the shared commit); U3-29.* in tasks.json (parses) |
| U3-29.2 The feel audit | DONE | FEEL-DESIGN.md sections 1-5 (`0115c35`, `07aa374`, `5ed6990`): 16 findings ranked by the first minute, each with a number, a capture (`Logs/shots/n16-before/`, 7) or a code line; the research tagged; targets with instruments; what we won't do; the ranked queue |
| U3-29.3 The shared commit | DONE | `695531c` + `390cdf3` + `fb8c9d5`: design.move / design.impact with `_why` (the Player copy byte-identical), FeelKeys, stubs, the loop's time scale; fingerprint EQUAL; the shared commit's suite `723 / 724` -> `RunSnapshotTests 8 / 8` after `fb8c9d5` |
| U3-29.4 The slide becomes a skill | DONE | `b8bca8e` (MoveBalanceTests red then green); `SimHarness spam`: `targets (a) (b) (c) HOLD` (15:20, again 17:27); the skilled lines unchanged (dash-jump 21.20 m, slipway 28.81 m/s); the farm diff (100 runs, `Logs/FARM-n16.md`); the fingerprint re-recorded once (`0ac694e`); golden `0 mismatches`, 22 exact |
| U3-29.5 Impact frames | DONE | `1977084` (ImpactRules tests red then green); `impact PASS: the budget holds on 2 map(s)` (DRYDOCK 1074.5 ms frozen per minute, 1.79 %; worst rolling second 120.0 of 120.0 ms); the freeze / punch / parry / slowmo strips with HELD frames (builds 1-3) |
| U3-29.6 The hands | DONE | five per-gun draws, the fist, the glint, the movement layers (`96e2805` .. `42617e0`, `538ea24`, `5a03771`; HandsTests `34 / 34`); the punch, parry and draw strips (`Logs/shots/n16-b2-strips/`) |
| U3-29.7 The UI and the strip tool | DONE | the strip tool (stamps legible, frames consecutive: builds 1-4's sheets); the movement HUD (FeelHudTests 15 / 15), the headshot diamond, the open defects, the transitions; the before / after tour (`n16-before` vs `n16-b2` / `n16-b3`) reviewed in two lines; build 4's results and land strips (40 of 40) |
| U3-29.8 The polish queue | DONE | to 5:00: 61 entries in FEEL-DESIGN.md section 5, each in this log: 44 taken (commit, evidence, two-line review; Play checks 1 / 2 measured 7 of them live; the builders' rows red then green), 4 already existed, 8 left with a reason, 2 left for GG, 3 ready but not integrated (the smoke's pad paths, no build left); the residual fixed on the way (`51bc000`) |
| U3-29.9 Builds, reviewer, GG's kit | DONE | builds 1-3 logged (smoke, golden, fingerprint, replay, strips); the reviewer's 11 findings fixed or answered; FEEL-PLAYTEST.md (A = `Builds/n15-before`, B = `Builds/Stage1`; the steps, the switches, section 6's web cues, 7 questions) |
| U3-29.10 Close | DONE | build 4 `Succeeded`, 0 errors; its smoke re-run `DONE: 2 step(s) failed` = the two pad steps only (the focus: someone at the machine, idle `5.8, 0`), **steps 18 / 21 PASS** (the residual: `clean slate ... EQUAL to the baseline`, `attract ... the next run EQUAL to the baseline (12 of 12)`); `-skygrindGolden` `0 failing, 0 mismatches` (22 exact); `-skygrindFingerprint` `EQUAL to the baseline`; `-skygrindReplay` `EQUAL (1 of 1 checkpoints, 486 steps)`; the suite `850 / 850`, replays 22 / 22, golden 96 / 96; the Silent series not run (its condition unmet: the plan Balanced, the machine in use); tasks.json, this log, NIGHT16-REPORT, PORT-STATUS, HANDOFF (`u3-n16`), NIGHT17.md; tag `u3-n16` on the close commit |

NIGHT16 CLOSED
