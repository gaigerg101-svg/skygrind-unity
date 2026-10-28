# NIGHT18.md: SKYGRIND in Unity: FEEL IN MOTION (the kill at every range, hits that land, movement you can see, nothing pops in; measured on screen, not claimed), at least 5.5 h, hard cap 6 h

> **Written 2026-10-06 ~8 AM, after the NIGHT17 close** (anchor `u3-n17`). It's built from three sources:
> `NIGHT17-RETRO.md` (what the overhaul night proved and what it didn't), `Research/2026-10-06-feel-in-motion.md` (two
> research agents; the Opus 5.5 prompting page read first-hand) and the NIGHT17 captures.
>
> GG's ask: "follow the best available methods ... analyse the results ... what did not get completed or built at all
> ... create a prompt that will show real tangible improvements". GG had not played NIGHT17's build when this was
> written. Map night 2 moved again, to `NIGHT19.md` (U3-32).

You are Claude Code working in the Unity project `C:\Users\gaige\dev\SKYGRIND-unity` (Unity 6000.3.7f1, URP). GG starts
this run and leaves, with the Editor open on this project. Nobody answers questions during the night.

## GG's switches (GG may edit these before starting; the defaults are in capitals)

| Switch | Default | The other choice |
| --- | --- | --- |
| **AUDIO** | **OFF**: CLAUDE.md rule 7 holds. Every new moment emits an event for GG's sounds. | **ON**: placeholder sounds synthesised in code (no files) on the existing events, with an SFX VOLUME row |
| **KILL MARKER** | **ON**: a HUD kill confirm at the crosshair (white, distinct from the hit marker), 300-400 ms, overshoot pop | **OFF**: world effects only |
| **CHUNKS LINGER** | **OFF**: chunks fade within 1.5 s (D-29: no gore that stays) | **ON**: chunks rest on the floor for 3 s before fading (Vlambeer's "permanence") |
| **CAMERA FX** | **100 %**: the FOV kicks and shake at full strength, with FOV KICK and CAMERA SHAKE rows (0-100 %) in SETTINGS | any percentage as the new default |

## GG's notes come first

If `GG-NOTES.md` exists in the repo root, or GG's start message carries notes from playing NIGHT17's build
(`FEEL-PLAYTEST.md`), read them before anything else. Where they disagree with this plan's priorities, they win. Log
what changed and why in `NIGHT18-LOG.md`, under RECEIVED.

## Why this night (NIGHT17-RETRO.md; read it whole)

NIGHT17 landed 185 commits with every gate green: golden exact, the fingerprint EQUAL, the suite 850 to 969. The menus
and the identity improved for real. But four fresh reviewers scored the build 25, 23, 27, 27 out of 50, and the rows GG
cares most about never moved:

| Row | Reviewers, rounds 1-4 | Why |
| --- | --- | --- |
| Kill crunch | 2, 2, 3, 2 | A gun kill's first frame looks like a hit's; at 10-20 m a kill is a ring and sparks |
| Movement feel | 2, 2, 2, 2 | The FOV widen and lines exist, but no capture ever showed their onset |
| Motion | 2, 2, 2, 2 | Stills can't show it, and elements still popped in raw |
| Hit feedback | 3, 2, 3, 3 | An 11° tilt for 95 ms on a symmetric capsule barely changes the silhouette; a revolver body hit has no push-back |

**The lesson:** NIGHT17 counted "implemented and unit-tested" as "felt".
- Builders got adjectives ("make it read") and returned code that met the adjective in the code.
- Reviewers judged motion from 1/3-scale mosaics that skipped frames.
- One reviewer per round, scoring 1-5, varied by ±2 on the same build. The lead's own scores ran 5-12 points high.

The research says the same in general terms: agents claim improvement while about half of the measured deltas are zero
or worse, and a separate, calibrated judge with access to the real artefact is what fixes it.

**Tonight's single idea: an effect is done when the screen measurably changes, and blind judges pick it over the old
build in both orders.** We build fewer, deeper things: the kill, the hit, the movement and UI motion. Each is owned end
to end against numbers.

## What "done" means tonight (the contract; agreed before any feel code)

Every number below is measured by the harness (section 1): from the game's own per-frame telemetry, and from
full-resolution frames for pixel metrics. All are at 1920 x 1080. Build 1 fills the baseline column before any feel
change lands. The targets come from the research; "(inferred)" marks the ones no source publishes, and the lead may
retune those once, at round 1, with the reason logged.

| # | Metric (scenario) | How it's measured | Target |
| --- | --- | --- | --- |
| M1 | **A kill's frame 1 vs a hit's frame 1** (kill12 vs hit12) | Pixels changed (luma Δ > 40) in a 400 x 400 window at the target, on the first rendered frame after the shot, against its own pre-shot frame. World pixels only: the HUD's element rects (from the scanner) are masked, so H's markers don't count | kill12's count ≥ 3.0 x **build 1's** hit12 count (a fixed denominator, so R's stronger hits don't count against X) (inferred) |
| M2 | **A kill reads at range** (kill6, kill12, kill20) | The union screen rect of the kill's effects (burst, chunks, ring) on frames 1-4, from telemetry; the HUD kill marker's frame and size | effects (X's half) ≥ 8 % of screen height at 20 m (inferred); the marker (H's half; n/a when KILL MARKER is OFF) on frame 1-2, ≥ 48 px, 300-400 ms |
| M3 | **Chunks thrown, not swelling** (kill12, horde5) | From telemetry: the chunk cloud's screen centroid and the share of chunks outside the body's pre-death rect, 250 ms after the freeze ends. A throw straight away from the camera barely moves on screen, so the throw needs a lateral and upward part | centroid moved ≥ 1.5 body heights; ≥ 60 % outside |
| M4 | **The freeze ladder** (hit12, kill12, headshot12, punchkill, horde5) | HELD steps in telemetry (120 Hz) | hit 0-2; body kill 6-8 (50-67 ms); headshot kill 10-13; punch kill 11-14; 5-kill blast ≥ 18 (≤ 160 ms cap) |
| M5 | **A hit lands on the body** (hit12, a revolver body hit) | The victim's screen silhouette (telemetry rect): the centre's displacement and the top edge's shift | ≥ 8 px within 50 ms, back within 200 ms; a white flash ≥ 2 frames |
| M6 | **Boost, sprint, slide in the camera** (boost, sprint, slide) | Camera FOV, height and roll per frame from telemetry, at a 60 fps capture | boost +10° ± 2 within 2 frames, back over 300-400 ms; sprint +5-6° within 250 ms; slide +6-8° and a ≥ 0.4 m drop within 100 ms, roll 3-5° |
| M7 | **Strafe and landing** (strafe, land) | Roll vs lateral speed; camera dip on landing | roll 1.5-3° at full strafe; dip 0.05-0.15 m by fall speed, back in 150-200 ms |
| M8 | **Speed lines** (sprint, boost) | Line alpha and screen region from telemetry | alpha > 0.3 only above 70 % of max speed; never in the central 60 % |
| M9 | **Nothing pops in** (every ui-* and HUD scenario) | Only elements that first appear inside the recorded window; their first visible frame against rest (rest = the median of their last 10 visible frames). Excluded: the harness's own canvas and in-place digit changes | 0 elements at ≥ 0.95 rest scale **and** ≥ 0.95 rest alpha **and** < 40 px from their rest position on their first frame (a ≥ 40 px slide counts as motion); entrances 90-300 ms |
| M10 | **Text floor** (every screen) | Rendered TMP font size at 1080p | 0 texts under 18 px |
| M11 | **Colour roles** (every screen) | The scanner lists every element whose colour is the GOLD, DANGER or RIVAL token. The lead writes `Tools/feel-roles.json` at the baseline (which element paths may use each), and the tool flags any new path | 0 unlisted users of GOLD / DANGER / RIVAL |
| M12 | **Cost** (`-skygrindBench 60`, 24 husks) | Frame p99, same power plan both runs | ≤ baseline + 1.0 ms |
| M13 | **The gates** | golden, fingerprint, `SimHarness impact`, the smoke | 0 mismatches, 22 exact; EQUAL; `impact PASS` (if M4's ladder breaks the freeze budget, the budget wins and the reason is logged); all steps (one re-run allowed, nothing else running) |

**Too much is a failure too.** Medium-to-high juice beats extreme on play time and experience (Kao, N = 3,018). Every
flash goes through `flashGate`, and the reviewers answer "too much?" each round.

## How tonight works (and why)

1. **Measure before changing.** The harness comes first, and build 1 is the baseline: NIGHT17's behaviour plus
   telemetry. Without a baseline, "better" is an opinion.
2. **One owner per experience, end to end, against its metrics.** A builder's brief is the metric, the measured value,
   the target and the scenario, never an adjective. A builder is done when the next reel shows the target met, not when
   the code compiles.
3. **Separate, calibrated, blind judges** (section 6):
   - **Who:** two fresh reviewers each round, read-only. They never see the code, the metrics or the lead's notes.
   - **What they see:** the same scenarios rendered twice, with NIGHT17's look (`FeelAB` off, or build 1) and with
     tonight's, labelled A / B at random.
   - **What they answer:** binary per-criterion questions, in both orders (position bias). "Unknown" is allowed.
   - **Anchoring:** they also score NIGHT17's 10-row rubric against anchored examples, so the trend continues.
   - **The lead's own scores** are logged as advisory and never decide an item.
4. **Motion is judged as motion.** Every rendered frame is kept at full resolution, at a fixed 60 fps capture, with
   300 ms of pre-roll before each event. Reviewers get near-native flipbook sheets of the 8 key frames of section 1
   (chosen from telemetry), not a 1/3-scale mosaic. The model reads near-native images better than
   mosaics, and PIL crops are allowed.
5. **Rounds of about 45 minutes on Player builds,** with the Editor reel (one guarded eval, about 1 minute) for checks
   between builds.
6. **Never idle.** If a builder finishes early or the lead is ahead of the clock, they take the next item from the
   backlog (U3-31.9). NIGHT17 idled 75 minutes waiting for its minimum.
7. **State lives in files.** After every integration, the log's last line is
   `NEXT: <what the lead does next, which builders are out, what each owes>`. After a stop (NIGHT17 lost 100 minutes to
   a usage limit), the start routine reads that line, `git log -5` and `tasks.json`, then carries on.
8. **Every status claim cites a tool result from this session:** a command's output, a metric row, a file and line.

## Read these files

- **First:** `CLAUDE.md`, then this file, then **`NIGHT17-RETRO.md`** and **`Research/2026-10-06-feel-in-motion.md`**.
- **`HANDOFF.md`:** sections 0, 1, 6, 7 (the dead ends: the Editor smoke's TITLE pad steps, the Editor's saved map, the
  eval-output grep).
- **`NIGHT17-LOG.md`:** the round tables and the U3-30.10 table.
- **`IDENTITY.md`.**
- **`PORT-RULES.md`:** sections 6, 11, 12.
- **`tasks.json`:** write U3-31 and U3-31.1 .. U3-31.10 before any code. `NIGHT19.md` (map night 2) uses U3-32.
- **The memory notes:** `player-smoke-focus-pause`, `play-check-eval-recipe`, `unity-editor-unfocused-autotick`,
  `night-parallel-agents`, `skygrind-targets-and-taste`.

**Run length: at least 5.5 hours, hard cap 6 hours, from your first `date`.**
- The close starts at 4:50 (the code freeze, then build 6), and its table prints at 5:30 or later. Run `date` right
  before printing the table, so the transcript proves the time.
- Running past 6:00 is a failure, and so is closing before 5:30.
- Show the time against the budget in every status line, like `3:10 / 6:00`.

## 0. Tonight's rules (on top of CLAUDE.md, which holds except where this prompt, GG's, says otherwise)

1. **Git:**
   - Branch `port/stage3` from its head (`u3-n17` plus docs).
   - Tag `u3-n18` once, last, on the close commit.
   - No push, no merge, nothing on master. The web repo is read only.
   - Restore the theme SDF font assets and the TMP fallback asset with `git checkout --` before every commit; builds
     rewrite them.
2. **A / B inside one build (`FeelAB`).**
   - Every NIGHT18 change to an effect's behaviour or number reads `FeelAB.On`. With it off, the code takes NIGHT17's
     path and values, through named numbers, never literals:
     `UnityTuning.Pick(FeelAB.On, UnityTuning.KillBurstMinScreen, UnityTuning.KillBurstMinScreenN17)`, or the two
     config tables below.
   - The freeze numbers are read by Core's `ImpactRules`, which can't see `FeelAB`. X keeps NIGHT17's freeze table as
     `design.impact.n17` (with its `_why`), and the Game side (`ImpactFrames.cs`, X's tonight) picks which table to
     build `ImpactRules` from when the switch changes.
   - The switch is on by default. `-skygrindFeelAB off` turns it off for a reel.
   - In a developer build (`SKYGRIND.exe -skygrindDev`), F6 (free: F1-F3, F8 and F9 are taken) and a LAB row toggle it
     live, with a small corner chip reading `FEEL N17` / `FEEL N18`, so GG can feel the difference in one session.
     FEEL-PLAYTEST.md says how.
   - Pure moves (the EnemyViews split) and the harness are exempt.
   - **Why:** blind A/B on identical geometry in the same build is the cleanest comparison. It also gives a baseline
     for scenarios added after build 1, and makes GG's playtest a toggle rather than two builds.
3. **Effects are pictures (the determinism wall).**
   - Everything tonight lives in the Game layer, so the composed fingerprint stays EQUAL across every commit. An
     effect that changes it is in the wrong layer.
   - The only Core file that may change is `Core/ImpactRules.cs`, if the freeze ladder (M4) needs a new rule. That
     needs golden + replays at its commit; the numbers live in `design.impact` with `_why`.
4. **Pillar 1 is the law:**
   - visuals follow the sim's timers and never gate them;
   - no eaten input;
   - no added latency.
5. **Numbers:**
   - Visual numbers go in `UnityTuning.*.cs` files with a comment.
   - Freeze numbers go in `design.impact` with `_why`, and the Player copy stays byte-identical.
   - UI colours and faces come from `UiTheme` tokens only.
6. **Accessibility:**
   - flashes through `flashGate` and FLASH INTENSITY;
   - REDUCED MOTION stills shake, FOV kicks and roll;
   - new FOV KICK and CAMERA SHAKE rows (0-100 %);
   - text ≥ 18 px.
7. **Performance:**
   - no allocation per frame in new code (PORT-RULES 6), including the telemetry (fixed ring buffers, written out only
     between scenarios). The reel's frame capture (the JPG encode) is exempt: it runs only in a reel;
   - M12 holds.
8. **The smoke and focus.**
   - A window taking focus pauses the run (memory `player-smoke-focus-pause`).
   - **In rounds,** the smoke may run while builders work, so nobody idles. Any failing step is re-run once with the
     builders asked to hold their tool calls for 5 minutes, before it counts as the build's.
   - **At the close,** the smoke runs alone: every agent reported, nothing else running, the idle time logged.
   - On a failure, compare round 1's line (`8 s ... points 1576` when clean) and look for `Paused` before calling it
     the build's. Re-run once.
   - The Editor's smoke fails its three TITLE pad steps by design (the attract); the Player's smoke is the check.
9. **The Editor plays GG's saved map.**
   - For the Editor reel, set `SurvivalGame.PendingMapId = RunSetup.DefaultMap` and reload in Play mode.
   - Never `PickMap`: it overwrites GG's preference.
10. **Caps.** A cap reached makes its item SHORT with the count.
   - Player builds: 7 (six planned and one spare, for a harness or compile fix).
   - Editor Play checks: 6.
   - Reel runs: 14 on Players (two per build: `FeelAB` off and on) and 6 in the Editor.
   - Disk: a reel is about 0.7 GB of JPGs. Full frames are kept for the baseline, the last two rounds and the final;
     older rounds keep their flipbooks only (deleting the frames is deleting your own temporary output).
   - Builders: 5, resumable by SendMessage.
   - Reviewer agents: 10 (two per round, two for the final).
   - Research agents: 0 (done).
   - Bench runs: 4 (two on the baseline, two on the final).
   - Lead verification time: 75 minutes, logged per check. Verification means suites and the headless gates
     (CoreHarness, SimHarness) only. Builds, smokes, reels, metrics and reviews are the night's method and don't count.
     Reaching the cap only stops the mid-round suites; it makes no item SHORT, and the close's gates always run.
11. **One driver for the Editor.**
    - Builders work in worktrees with dotnet only (`Tools/wt-csproj.cjs`), and never run `unity`.
    - Tests come first and are seen red.
    - Topic-named members in shared partials.
    - Hold back smoke-path changes when no build is left.
12. **No questions, no waiting.**
    - One status line per item in `NIGHT18-LOG.md`.
    - End every message with `N18-STATUS: <d> done, <s> short, <l> left (of 10) · round <r> · metrics <passed>/<total> ·
      tests <passed>/<total> · <h:mm> / 6:00`.

## 1. The harness (U3-31.2, the lead; the first thing built)

1. **`Game/FeelProbe.cs`**, static and allocation-free:
   - **Per rendered frame:** the frame index, the sim step, HELD (frozen) or not, and the camera's FOV, roll, pitch and
     height.
   - **Per tracked enemy:** its screen rect and its flash amount.
   - **Per effect group:** the union screen rect and the count, for chunks, the burst, the ring, the kill marker, the
     hit marker and the speed lines (with alpha). For chunks, also the screen centroid and the share outside the
     victim's pre-death rect (M3).
   - **Per UI element, found by a scanner:** no builder has to register anything, so the scanner catches the pop-ins
     nobody thought of.
     - Every active Graphic under the game's canvases, cached and refreshed when the hierarchy changes, using
       non-allocating `GetComponentsInChildren(list)`.
     - For each: its path name, screen rect, `lossyScale`, its effective alpha (CanvasGroup × colour), its colour, and
       for TMP its rendered size at 1080p (`fontSize × canvas.scaleFactor × 1080 / Screen.height`).
     - **Rest values:** an element's rest scale, alpha and position are the median of its last 10 visible frames
       (M9).
   - **The API** (`FeelProbe.Effect(...)`, `FeelProbe.Ui(...)`) is pre-placed in the shared commit as a no-op outside a
     reel, so builders call it from their own files from minute one.
   - **Output:** one JSONL file per scenario.
2. **FeelStrip v2:**
   - **A fixed capture rate, with one clock for everything:**
     - Unity's docs: "captureDeltaTime does not have any affect on Time.unscaledTime". SurvivalGame's loop reads
       `Time.unscaledDeltaTime` (SurvivalGame.cs:265), so `Time.captureDeltaTime` alone won't pace it.
     - Add a reel-only frame clock (`FeelClock`, the lead's): while a scenario records, `frameDt` is exactly 1/60 per
       rendered frame, however long the frame takes in wall time (JPG writes). The fixed 120 Hz sim then steps 2 per
       frame, and every view system that takes `frameDt` from the loop follows.
     - Also set `Time.captureDeltaTime = 1/60`, for Unity-driven systems.
     - Route the direct wall-time readers through the clock while it's active:
       - `FlashGate.RealNow`;
       - `UiPress.pressAge`;
       - the quit-confirm window in `UiMenus`;
       - `CombatReader.cs:149`.

       A grep for `Time.unscaled` and `realtimeSinceStartup` outside the checks finds them; FeelStrip's and ShotTour's
       own timeouts may stay on wall time.
     - **During a reel, pin the quality preset** (high) and turn **off AUTO quality**: `Quality.cs:154` reads
       `unscaledDeltaTime`, so slow JPG frames would drop the preset mid-reel. Also turn **off the attract**
       (`SurvivalGame.Kiosk.cs:259 / 291`), or it takes TITLE in ui-title. Both sides of an A/B must render under the
       same settings.
     - Prove the clock: a scenario's telemetry shows sim steps advancing 2 per rendered frame (a rare 1 or 3 from
       rounding is fine), except HELD and slow-mo frames.
   - **Every frame** goes to JPG q90 at 1920 x 1080, with 300 ms of pre-roll, and a post-roll of 1.2 s for kills and
     1.5 s for the flourish.
   - **Fixed geometry:** the target goes on open floor, found by casting the collider, at exactly 6 / 12 / 20 m.
     - It's held still with `g.husks.frozen` and spawned with `g.husks.Spawn`, as FeelStrip's existing scenarios do
       (FeelStrip.cs:14, 498, 557).
     - The same seed and the same placement are used for both `FeelAB` sides, so A and B differ only by tonight's
       change.
   - **The scenarios, in two phases** (a phase-B scenario's baseline comes from `-skygrindFeelAB off` on the build where
     it lands):
     - **Phase A, by build 1 (12):** hit12, kill12, kill20, headshot12, horde5 (all five die in one blast), boost (from
       standstill), sprint (from rest), slide (from a sprint), rankup, roundend, ui-title, ui-settings (open, then move
       the focus twice).
     - **Phase B, by build 2:** kill6, punchkill, boostside, land (a 3 m drop), strafe, swapall, pickup, hud-fight (a
       3 s fight), ui-paused, ui-results (entering), ui-mods (entering).
   - The run is `-skygrindReel <abs dir> [-skygrindReelOnly ...]` on a Player. The Editor uses the same coroutines
     through one guarded eval (see `play-check-eval-recipe`).
3. **`Tools/feel-metrics.py`** (PIL + numpy, both installed; tracked). Builder N writes it in its first 45 minutes from
   `Tools/feel-schema.md` and the sample JSONL the lead puts in the shared commit; the lead reviews it on build 1's reel.
   It:
   - Reads one reel folder and computes M1-M11. M12 comes from the bench log.
   - Writes `Logs/feel/<tag>/FEEL-METRICS.md`: metric, value, target, pass, and the baseline's value.
   - Writes per-scenario flipbooks of **8 frames chosen from telemetry**, not 8 in a row. 8 consecutive frames are
     only 133 ms, shorter than a freeze plus the throw.
     - The frames: the input, the first HELD frame, the last HELD frame, the freeze's end, the freeze's end + 120 ms,
       + 250 ms, + 400 ms, and the last frame with a live effect. Camera and UI scenarios use the input, then + 33,
       67, 100, 167, 250, 400 and 700 ms.
     - The sheets stay near native, because the model downscales larger images:
       - kills and hits: 4 x 2 native 480 x 480 crops centred on the target's telemetry rect, on a 1920 x 960 sheet;
       - camera and UI scenarios: two 2 x 2 sheets of whole frames at 960 x 540 per side.
     - **No in-frame stamp:** no ms, no step, no HELD. Tile numbers go in the margin only, so nothing reveals which
       side is which.
   - Writes **blind A/B sheets** for two reels into a neutral folder with numbered names (`s07-A.png`), labelled A / B
     at random per scenario. The key goes in a file only the lead reads, and the lead prints the decoded picks in the
     transcript. That's about 70 images for a whole reel: a reviewer can read them all.
4. **Proof the harness tells the truth:** on build 1, check three metrics by eye against their frames (a boost's FOV, a
   kill's M1 window, one UI element's first frame) and log the agreement. A metric that disagrees with its frames is
   fixed before it's trusted.

**Build 1 (about 1:05) = the baseline:** NIGHT17's behaviour plus the harness. Its reel fills the baseline column of
every phase-A metric. From round 1, each round's own `FeelAB` off reel is the "A" side, since it has the same geometry
and code; build 1 stays the reference for the report.

## 2. Who owns what (builders)

| Builder | Owns (creates or edits only these) | Answers for |
| --- | --- | --- |
| **X: the kill** | `Chunks.cs`, `KillConfirm.cs` (world rings), `ImpactFx.cs`, `PostFx.cs`, the kill's shake (a `UnityTuning.Crunch` multiplier on the trauma a kill adds; the lead adds the one hook line in the loop on request; never `config.json` or `Core/Trauma.cs`), `EnemyViews.Death.cs` and the rest of `EnemyViews.cs` (after the split; not `.Hit.cs`), `UnityTuning.Crunch.cs`, `ImpactFrames.cs` (the freeze ladder and the `FeelAB` freeze-table pick), `design.impact` in both config copies, `Core/ImpactRules.cs` (M4 only); AUDIO ON, if GG set it, after M1-M4 | M1, M2 (the effects half), M3, M4 |
| **R: the hit** | `EnemyViews.Hit.cs` (after the split: flash, flinch, tilt, push-back), new `Game/HitSpring.cs` (a view-only damped spring; Source's punch, damping 9 and spring 65, is the starting point), new `UnityTuning.HitReact.cs` | M5 |
| **M: movement** | `CameraFeel.cs` (it is also where the FOV KICK and CAMERA SHAKE percentages and REDUCED MOTION apply, to every kick including X's: the `fovKick` stack in `ImpactFrames.cs` (:42, :86, :96) routes through M's composition point), `MovementFx.cs` (sparks, dust), `UiFeel.cs` (the speed lines), `Viewmodel*.cs` (movement poses, the flourish, the glint; V is off tonight), `UiStaminaBar.cs`, `StaminaBarLook.cs`, `StaminaHud.cs`, new `UnityTuning.CameraMove.cs` | M6-M8; backlog 1-2 |
| **H: HUD motion** | `Ui.cs` (the HUD), `UiHudMotion`, `UiPointsPops`, `UiStyleMeter`, `UiCeremony`, `UiHotbar`, `UiFeelHud`, `UiPowerUps`, `UiPerks`, `UiMoveRow`, `UiMarkers` (the hit and **kill markers**), `UiTween.cs` (additive only: new eases, nothing changed), `UnityTuning.Hud17.cs` / `UnityTuning.FeelHud.cs`; **any unowned HUD-side file the metrics flag** (e.g. `UiDamageNumbers`, `UiWeapons`, `UiMomentum`, `UiPayoff`, `UiFeelEdges`), named by the lead | M9 (HUD), M10 / M11 on HUD screens, the marker half of M2 |
| **N: menus motion and words** | **first (about 45 minutes):** `Tools/feel-metrics.py` from `Tools/feel-schema.md`, the A/B sheet writer, the side-by-side GIF writer (`ab-<scenario>.gif`) and the `feel-roles.json` check, all pure Python run against the sample. **Then:** `UiMenus`, `UiKit` (transitions), `UiMeta`, `UiResults`, `UiSettings`, `GameSettings.cs` (the FOV KICK / CAMERA SHAKE rows), `UiMods`, `UiOnboarding` (ASCII prompts to drawn chips), `UiNav`, `UnityTuning.Menus17.cs`; **any unowned menu-side file the metrics flag** (e.g. `TitleView.cs`'s UI half), named by the lead | M9 (menus), M10 / M11 on menu screens |
| **The lead** | `FeelProbe.cs`, `FeelClock`, `FeelAB.cs`, `FeelStrip.cs`, `ShotTour.cs`, `PlayerChecks.cs`, `Tools/feel-schema.md`, `Tools/feel-roles.json`, the EnemyViews split (a pure move, in the shared commit), `SurvivalGame*.cs` loop lines, every doc, the Editor, git, the reviewers, the builds | the harness, the rounds, the gates |

**The shared commit (0:15-0:30, the lead):**
- `EnemyViews.cs` split as a pure move, with the kill / hit strips compared before and after:
  - `EnemyLook` (lines 42-730) becomes `partial`;
  - its hit members (`OnHit`, the `Hit` struct, flinch / tilt / knock, the shed) go to `EnemyViews.Hit.cs`, which is
    **R's**;
  - its death members (`Die`, `Corpse`, the dissolve) go to `EnemyViews.Death.cs`;
  - the rest of `EnemyViews.cs` stays, including the per-type views' `OnKill` (HuskView :780, SwarmerView :900,
    SpitterView :1100) and `Particles`. That file and `EnemyViews.Death.cs` are **X's**.
- the FeelProbe API stub;
- `Game/FeelAB.cs` (`On`, `-skygrindFeelAB off`, F6, the corner chip) and `UnityTuning.Pick`, plus its LAB row in
  `UiSettings.cs`, placed now because that file becomes N's;
- the FeelClock routing lines in files that become N's or H's (`UiMenus`' quit-confirm window, `UiPress.pressAge`),
  also placed now;
- `Tools/feel-schema.md`: the JSONL fields and a hand-written sample for N.
- the FOV KICK / CAMERA SHAKE keys in GameSettings' store, with N's rows to follow;
- empty test fixtures (`HitSpringTests`, `FeelProbeTests`).

Then launch the builders. The common brief is section 7.

## 3. The items (U3-31.1 .. U3-31.10)

**Where an item is decided:** on the final build (build 6, or 7 if the spare was used earlier), by its rows in that build's `FEEL-METRICS.md` and by the
final pair of blind reviewers (`FeelAB` off vs on, the sides swapped for the second reviewer). One judgement is one
reviewer's pick on one scenario, so 4 scenarios give 8 judgements. The rounds show progress; the final decides.

### U3-31.1 Start (lead, 0:00-0:15)
- `date`, then GG's notes (above). Then HANDOFF section 1 row by row.
- `set_autotick`; the machine (power plan, AC, CPU, free disk: at least 15 GB); RECEIVED in `NIGHT18-LOG.md`.
- Copy `Builds/Stage1/` (NIGHT17's build 6) to `Builds/n17-before/`.
- **Done when:** the rows are logged, tasks.json parses with U3-31.*, and `Builds/n17-before/` exists.

### U3-31.2 The harness and the baseline (lead, 0:15-1:20; N writes the metrics tool)
- **Done when:**
  - FeelProbe, FeelClock, FeelAB, FeelStrip v2 (phase A) and feel-metrics.py are committed (FeelProbeTests green);
  - build 1's reel wrote `Logs/feel/b1/FEEL-METRICS.md` with every phase-A row valued, or "n/a" with its reason;
  - the clock proof holds (2 steps per rendered frame);
  - the three eye checks agree, each printed as the telemetry value beside the value read from the frame;
  - the bench baseline ran (2 runs, the plan logged);
  - the builders were launched by 0:30, with a `date` output at the launch;
  - phase B and its `-skygrindFeelAB off` baselines are in by build 2.

### U3-31.3 The kill at every range (X)
- Make a kill unmistakable in its first frame and readable at 20 m:
  - a kill-only frame 1 (the silhouette white-hot plus a burst, with a screen-size floor);
  - chunks thrown along the shot and out of the body's rect;
  - the freeze ladder of M4;
  - the horde scaling.
- **Done when:**
  - M1, M2 (the effects half), M3 and M4 pass;
  - the reviewers pick tonight's side as "the more satisfying kill" in at least 6 of 8 judgements over kill12, kill20,
    headshot12 and horde5;
  - neither reviewer marks "too much".

### U3-31.4 The hit that lands (R)
- Give a body hit visible weight on a symmetric capsule. Rotation alone doesn't read on one, so use displacement plus
  squash and stretch on a view-only spring that overshoots.
- **Done when:** M5 passes, and both reviewers pick tonight's side on hit12 ("which shows the hit landing?"): 2 of 2.

### U3-31.5 Movement you can see (M)
- **Boost:** a FOV kick whole within 2 frames.
- **Sprint:** a held widen and edge lines.
- **Slide:** a drop, a roll and floor sparks in view.
- **Strafe and landing:** strafe roll from lateral speed, and a landing dip by fall speed.
- **The stamina bar:** readable while it drains.
- **Done when:** M6-M8 pass, and the reviewers pick tonight's side in at least 7 of 10 judgements over boost, sprint,
  slide, land and strafe.

### U3-31.6 HUD motion and the kill marker (H)
- No HUD element appears at full size and full alpha in one frame.
- The kill marker (KILL MARKER switch) is distinct from the hit marker.
- Pops, counter, meter and banners animate in, measured.
- **Done when:** M9 shows 0 HUD pop-ins, M10 and M11 show 0 violations on HUD screens, the marker half of M2 passes (n/a when KILL MARKER is OFF), and
  the reviewers pick tonight's side in at least
  6 of 8 judgements over rankup, roundend, pickup and hud-fight.

### U3-31.7 Menus motion and words (N)
- Every menu entrance and focus move animates (ease-out-back, 90-300 ms, along the slant).
- The ASCII prompts become drawn chips.
- The FOV KICK / CAMERA SHAKE rows.
- **Done when:** M9 shows 0 menu pop-ins, M10 and M11 show 0 violations on menu screens, the smoke's menu and pad steps
  pass, and the
  reviewers pick tonight's side in at least 7 of 10 judgements over the five ui-* scenarios.

### U3-31.8 The rounds (lead)
- Four rounds on builds 2-5 (about 1:35, 2:20, 3:05, 3:50; round 4 ends by 4:40). Each round, A is
  `-skygrindFeelAB off` on the same build (or build 1) and B is the round's build:
  - the build;
  - the smoke (builders may keep working; a failing step is re-run once with them held, rule 8);
  - the reel;
  - `feel-metrics.py` against b1 and the previous round;
  - two blind reviewers;
  - fix lists.

  A fix list names each failed metric with its value and target, and each lost blind judgement with the reviewer's
  reason.
- **Done when:** four rounds are logged, each with its metrics table, both reviewers' verdicts and 10-row scores, the
  fix lists and what changed.

### U3-31.9 The backlog (any idle builder, or the lead), in order
1. The kill flourish captured (a post-roll strip) and readable (M).
2. The QUICKDRAW glint redone on the gun's own surface (M).
3. The spawn ring in DANGER, a role fix (X).
4. The low-HP vignette pulse (X, `PostFx`).
5. HUD STYLE ATTACHED (H).
6. Per-gun recoil measured as distinct (M, telemetry).

- **Done when:** each item taken has a commit, a metric or flipbook, and a two-line review; each item left has its
  reason.

### U3-31.10 Close (lead, from 4:50; table at 5:30 or later; hard stop 6:00)
- **The code freezes at 4:50; build 6 starts at once** (allow 15 minutes: NIGHT17's final was a 582 s full rebuild).
- The suite on the frozen code (Editor, async) runs after the build, while the Player checks and the docs proceed.
- **What the transcript must show, so the /goal checker can see it:**
  - the decoded blind picks (a table per scenario: reviewer, pick, side);
  - the final FEEL-METRICS table;
  - the gate lines;
  - a `date` output before the table.
- The smoke, alone; `-skygrindGolden`; `-skygrindFingerprint`; `-skygrindReplay` on the smoke's tape; the bench (2
  runs).
- The final reels (`FeelAB` off and on), the metrics against b1 and against `FeelAB` off, and the final pair of blind
  reviewers (off vs on, both orders), who decide U3-31.3-31.7. The docs are drafted by 4:50, so the close only fills
  in numbers.
- **The docs:**
  - `NIGHT18-LOG.md`;
  - `NIGHT18-REPORT.md`, GG's one page:
    - the metrics table, baseline vs final;
    - the blind verdicts;
    - four before / after flipbooks (kill20, hit12, boost, ui-settings), each also as an animated side-by-side GIF at
      real speed (`Logs/feel/final/ab-<scenario>.gif`, written by feel-metrics.py with PIL; no ffmpeg is installed), so
      GG sees the motion;
    - the questions;
  - `FEEL-PLAYTEST.md` rewritten for `Builds/n17-before` vs `Builds/Stage1`;
  - PORT-STATUS;
  - HANDOFF (the `handoff` skill, anchor `u3-n18`);
  - tasks.json.
- **Tag `u3-n18`;** print the close table, then `NIGHT18 CLOSED`.

## 4. The clock (from the first `date`)

| When | Lead | Builders |
| --- | --- | --- |
| 0:00-0:15 | U3-31.1 | none |
| 0:15-0:30 | the shared commit (the split, the FeelProbe stub, FeelAB, the schema); launch X, R, M, H, N | none |
| 0:30-1:05 | FeelProbe, FeelClock, FeelStrip v2 phase A; build 1 (the baseline) at about 1:05 | N: feel-metrics.py first. X, R, M, H: first passes behind `FeelAB`, against NIGHT17's measured symptoms (NIGHT17-RETRO.md) |
| 1:05-1:35 | build 1's reels, metrics and eye checks (print both values); integrate the first passes; phase B; build 2 at about 1:35 | landing; N to menus |
| 1:35-4:40 | rounds 1-4 on builds 2-5 (about 1:35, 2:20, 3:05, 3:50; about 45 minutes each), the Editor reel between builds; round 4 ends by 4:40 | fix lists, then the backlog |
| 4:40-4:50 | the last fixes, checked by the Editor reel; the docs' drafts | the last fixes |
| 4:50-6:00 | U3-31.10: freeze, build 6, the gates, the final reels and reviewers, the suite, the docs (table at 5:30 or later) | none |

**The cut order if short:**
1. The backlog.
2. M7 (strafe and landing).
3. Menu motion beyond entrances.
4. The 4th round.

**Never cut:** the harness and its baseline, M1-M5, the blind reviewers, the gates, the close.

## 5. The rubric for continuity (NIGHT17's 10 rows, 1-5, anchored)

Each reviewer still scores NIGHT17's rows so the trend continues, against these anchors (NIGHT17 frames):
- **kill crunch 2** = `Logs/shots/n17-r4/n17-strip-kill.png`;
- **movement 2** = `n17-r4/n17-strip-sprint.png`;
- **motion 2** = `n17-r4/n17-strip-roundend.png` (the banner pops in);
- **menu simplicity 4** = `n17-final/n17-settings-game.png`;
- **identity 4** = `n17-final/n17-title.png`.

A 5 needs a reason that names a frame. These scores are context; the blind A/B and the metrics decide the items.

## 6. The reviewer brief (two per round, fresh, read-only)

> You are judging a game's feel from frames only. You won't see code, metrics or anyone's notes. For each scenario you
> get flipbook sheets for A and B: the same moment rendered twice, by two versions of the game. Each shows 8 key
> frames in order, tile 1 = the input, then the moments after it, near native resolution. Crop or zoom with PIL if a
> detail matters.
>
> For each scenario, answer the question asked (e.g. "In which one does the kill read as a kill at a glance?", "Which
> shows the hit landing on the body?", "Which boost feels faster?", "Which menu enters with motion rather than
> appearing?") with **A**, **B**, **Same** or **Unknown**, and give one line naming the frames that decided it. Then
> answer "Is either one too much (noisy, hard to read, tiring)?".
>
> Flag only problems that bear on those questions.
>
> Then score the 10-row rubric for B only, against the anchor images given, one line per row.
>
> Return a table, under 600 words.

The lead runs the A/B in both orders (the second reviewer sees every side swapped), decodes the picks with the key, and
logs each reviewer's pick per scenario. An item's threshold counts those picks; "Same" and "Unknown" count against it.
Both reviewers also answer "too much?"; a "yes" on an item's scenario holds that item at SHORT until it's fixed.

## 7. The builder brief (common part; each builder also gets its rows)

**A fix-list entry names the metric, the frames and the target, never an adjective.**

<example type="good">
X, M1 (kill12 vs hit12): ratio 1.4, target ≥ 3.0.
`Logs/feel/b2/flip/kill12.png` and `hit12.png`, frame 1: both show the white capsule and the cyan muzzle star. The
kill's extra pixels are the ring (about 30 px), which lands on frame 2. The burst never appears on frame 1.
Make frame 1 differ: the kill-only burst and the white-hot silhouette on the impact frame itself.
Expect M1 ≥ 3.0 and M2 ≥ 8 % at 20 m.
</example>

<example type="good">
R, M5 (hit12): centre displacement 2 px (target ≥ 8 px within 50 ms).
The tilt is there (telemetry: 11° at frame 2), but the capsule's rect barely moves.
Push the view back along the shot and squash it on the impact frame through the spring, then check M5's two numbers in
the next Editor reel.
</example>

<example type="bad">
X: kills still don't feel crunchy enough; make them punchier and more readable at range.
</example>

The bad one is what NIGHT17 sent, and it produced code that met the words but not the screen.

- **Objective:** your metrics' targets on the next Player build's reel. Code that compiles is not done.
- **Inputs:**
  - the metrics table (the current value, the target, the scenario);
  - the flipbooks and frames;
  - your files (section 2).
- **Rules:**
  - Tests first for pure rules (seen red).
  - Call `FeelProbe` from your code so the harness can measure your effect.
  - No allocation per frame.
  - Never edit another builder's file: ask the lead with the exact lines.
  - Never run `unity`.
- **Report** (under 400 words):
  - the commits;
  - what a player now sees, and in which scenario and frames;
  - which metric you expect to move, and by how much;
  - what you didn't do;
  - dotnet results.

## 8. The close table (print it, then `NIGHT18 CLOSED`)

| Item | DONE / SHORT | Evidence ("done when", from this session) |
| --- | --- | --- |
| U3-31.1 Start | | |
| U3-31.2 The harness and the baseline | | |
| U3-31.3 The kill at every range | | |
| U3-31.4 The hit that lands | | |
| U3-31.5 Movement you can see | | |
| U3-31.6 HUD motion and the kill marker | | |
| U3-31.7 Menus motion and words | | |
| U3-31.8 The rounds | | |
| U3-31.9 The backlog | | |
| U3-31.10 Close | | |

SHORT needs its measured reason: a cap reached, a decision reserved for GG, a blocker after two approaches, or the time
limit. A SHORT on U3-31.3-31.7 states its metric values and blind-judgement counts.

## 9. The /goal for GG to paste

```text
/goal Run NIGHT18.md in C:\Users\gaige\dev\SKYGRIND-unity. Run `date` first, then read CLAUDE.md, NIGHT18.md and the files it lists, and work U3-31.1 .. U3-31.10 in NIGHT18.md's order with at most five background builders split by file ownership. Never ask me questions and don't stop early; end every message with the N18-STATUS line. Never push, merge or commit to master; the web repo is read only. Run at least 5.5 hours: don't start the close before 4:50 or print its table before 5:30 from the first `date` (run `date` right before the table); hard cap 6 hours. This goal is met when your latest message prints NIGHT18.md's close table with every item U3-31.1 .. U3-31.10 marked DONE (with its "done when" evidence from this session) or SHORT (with its measured reason: a cap reached, a decision reserved for GG, a blocker after two approaches, or the time limit once a `date` output reads 5.5 hours or more after the first `date` output), followed by the line "NIGHT18 CLOSED". SHORT rows count as met.
```

## GG's standing instruction (verbatim, from NIGHT8; also Anthropic's recommended text for unattended Opus 5.5 runs)

A standing instruction from the user, the person you are working for. It is about how your turns end. A message with no
tool call in it ends your turn, and the work stops there until you are asked to continue. The user has seen you end turns
in four ways while work they asked for was still owed, and does not want any of them. One: a long summary of what was
done that closes by announcing the next step and has no tool call, so the next thing never starts. Two: an offer to carry
on with something unless the user would prefer otherwise, which stops to wait for an answer the user was not going to
give. Three: a list of decisions for the user when, by your own account, none of them blocks the rest of the work. Four:
deciding that this is a good place to report, because the turn has been long or a milestone is done. Status notes are
welcome, and so are your recommendations on open decisions, but put them in the same message as your next tool call and
carry on with whatever does not depend on the user's answer. If you notice yourself inviting the user to redirect you or
offering to wait, delete it and do the next thing. The stops the user does want are the ones where nothing can move
without them, or where the thing blocking you is deliberately protected from you. This does not override the need for
confirmation on risky or destructive actions.
