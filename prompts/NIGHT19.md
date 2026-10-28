# NIGHT19.md: SKYGRIND in Unity: EVERY GUN HAS A JOB (the pad plays like a shooter, the KESTREL's wind gust, guns that are sidegrades, threats that catch you, a HUD that isn't cheap; measured by three judges and blind reviewers), at least 5 h, hard cap 5.5 h

> **Written 2026-10-06 ~4 PM, after the NIGHT18 close** (anchor `u3-n18`, tag on `937cda3`). Sources:
> - GG's message of 2026-10-06 (quoted below);
> - `Research/2026-10-06-weapon-sandbox.md` (ULTRAKILL / DOOM Eternal / Hades numbers, pad conventions, glyph packs);
> - `Research/2026-10-06-assets-and-prompting.md` (permissive assets with licences, and the Opus 5.5 prompting page's
>   long-run techniques);
> - a code map of the pad, prompts, menus, melee, alt-fires, stamina, the HUD, the guns, the enemies and the harnesses.
>   Its facts are copied into this file with `path:line`; re-check any you rely on.
>
> Map night 2 moved again, to `NIGHT20.md` (U3-32, unchanged).

You are Claude Code working in the Unity project `C:\Users\gaige\dev\SKYGRIND-unity` (Unity 6000.3.7f1, URP). GG starts
this run and leaves, with the Editor open on this project. Nobody answers questions during the night.

## What GG asked for (2026-10-06, his words, lightly trimmed)

> If you're using controller you should be able to sprint using the left stick when you press it down ... for the first
> weapon if you use the left trigger ... it'll be like kind of more of a wind gust, it's more of a movement option ...
> I don't want ... you move too fast, the movement's too advanced, to where nothing can catch you. We need a little bit
> of balance. Then again games like ULTRAKILL ... when you know how to use the movement it becomes easy, so I guess
> that's not too much of a problem ... maybe it'll use a little bit less stamina ... I like the idea of starting out with
> less and then you have to get more ... if you're using the right stick and you click down on it, it should do that
> kind of melee attack that you usually see [in Zombies]; it's not as strong but it is something. The stamina bar should
> not be just in the middle of the screen, it's really ugly ... don't copy ULTRAKILL's UI but it should feel similar.
> If you're using controller the buttons need to switch ... to the correct button triggers. You should be able to ...
> switch between all the different options using the right and left bumper. ... In ULTRAKILL each weapon has its own
> purpose ... this game feels a little bit more like Zombies, where you just pick up the strongest weapon and stick to
> that. No: I want the game overhauled so that all the weapons are balanced, it's just which one you prefer, and combos
> ... the enemy AI or maybe even the map ... should really encourage that creativity. Balance the weapons, maybe even add
> more weapons, rebalance them and the progression ... Don't be afraid to copy and pull assets, because a lot of the UI
> and effects and animations still look a little cheesy.

## GG's switches (GG may edit these before starting; the defaults are in capitals)

| Switch | Default | The other choice |
| --- | --- | --- |
| **PAD LAYOUT** | **SHOOTER**: L3 sprint, R3 punch / parry (the quick melee), LB boost (as now), RB special + grapple, d-pad up lock-on; everything else as NIGHT18 (section 3) | **NIGHT18** (L3 grapple, R3 lock-on, RB punch), with L3 sprint and R3 melee as options on the CONTROLS screen |
| **L3 SPRINT** | **TOGGLE** (CoD): click to sprint; it ends when the stick returns to the centre, the bar empties, or on another click | **HOLD**: sprint while L3 is held |
| **KESTREL LT** | **GUST**: the KESTREL's alt-fire (LT, right mouse) is the wind gust and the zoom goes | **ZOOM**: the zoom stays on LT and the gust moves to the KESTREL's SPECIAL |
| **SPRINT DRAIN** | **0.3 bar/s** (from 0.4: a full bar sprints 3.3 s instead of 2.5 s) | 0.2 (the research's) or 0.4 (unchanged) |
| **FRESHNESS** | **POINTS**: ULTRAKILL's table, on points and style only, never damage; DULL pays x0.25 rather than ULTRAKILL's x0 | **OFF** |
| **FORGE** | **SIDEGRADE**: the forge gives the gun its alt-fire variant plus a flat x1.4 to every gun alike, instead of x1.8 plus a family bonus | **DAMAGE**: as now (x1.8 plus the family bonus) |
| **NEW GUNS** | **PORT 2**: ZAPPER and BOUNCER from the web (`src/combat/guns.js`), as the combo partners; VORTEX and TORCH are "beyond" | **NONE** |
| **ENEMIES** | **PORT**: the web's CHARGER, DRONE and the SHIELDED affix, rebuilt as they are (CLAUDE.md rule 7: the web's blocky models and behaviours, the web's rounds); LANCER, VOLATILE and BLINKER are "beyond" | **NONE**: only the three kinds of today, retuned |
| **ASSETS** | **PULL**: permissive art and code (CC0, MIT, BSD, OFL, Unlicense), vendored with their licences and a `THIRD-PARTY.md` row each. This replaces D-30 ("no packages", 2026-10-03) on GG's word; log it as D-72 | **ART ONLY**: CC0 art and OFL fonts, no packages |
| **AUDIO** | **OFF**: rule 7 holds; every new moment emits a named event for GG's sounds | |

## GG's notes come first

If `GG-NOTES.md` exists in the repo root, or GG's start message carries notes (from NIGHT18's build, `FEEL-PLAYTEST.md`,
or NIGHT18-REPORT section 5's six questions), read them before anything else. Where they disagree with this plan, they
win. Log what changed and why in `NIGHT19-LOG.md`, under RECEIVED. Unanswered questions keep NIGHT18's defaults.

## Why this night

NIGHT18 made the game measure its own feel and won blind picks on kills, hits, movement and menus (rubric 34 / 35).
What GG plays, though, is a CoD Zombies loop with a fast body:
- **One gun wins.** The farm's kill share (NIGHT17, `Logs/FARM-n17.md`): THUNDERCLAP 38-40 %, KESTREL 28-33 %, STITCHER
  27-29 %, everything else about 2 %. Only 6 of the 18 weapons in the config fire at all (`Combat.cs:719`,
  `Loadout.Fires`). The FORGE sells x1.8 damage plus a family bonus, so whichever gun you forge stays the answer.
- **LT is dead on half the guns.** The KESTREL zooms; LT does nothing on the THUNDERCLAP, the LONGWAVE or the GAUNTLET.
  The STITCHER's LT detonates its nails and the COMET's steers its rocket (`Combat.cs:580-660`).
- **Nothing catches you, so nothing asks you to choose.** Only three kinds are ported: HUSK 6 m/s, SWARMER 10 m/s,
  SPITTER 4.5 m/s. The sprint is 10.5 m/s and the boost 24 m/s. DRONE, CHARGER, LANCER and WARDEN spawn as husks
  (`Director.cs:429`). The research's reading: SKYGRIND's walk : slide : dash ratios are exactly ULTRAKILL's
  (1 : 1.45 : 3). **The movement isn't too fast; the threats are too slow and too alike.**
- **The pad shows a keyboard.** "HOLD E" on every buy prompt (`Ui.cs:891`), the break line's E (`Ui.cs:381`), the
  hotbar's hardcoded "LT/L2/ZL" and "Y NEXT" (`UnityTuning.Hotbar.cs:56-59`, `UiHotbar.cs:301`, `:439`), the kiosk rows,
  and TAB on a pad's WHEEL prompt (`UiOnboarding.cs`: action "radial" vs the pad's "weaponWheel"). The glyphs are words
  in TMP, not drawn buttons (`UnityTuning.Input.cs:13-27`).
- **The stamina bar sits under the crosshair** (`UiStaminaBar.cs:49`, a child of the `sight` layer).

**Tonight's single idea:** every gun is the best answer to something and the wrong answer to something else. Guns act
on each other's objects (combos). The enemies make you pick: they favour a gun, never require one. Three headless
judges prove it with numbers before any reviewer sees a frame:
- **THE RANGE:** time to clear, per gun and per situation;
- **THE CHASE:** can a player who only runs survive;
- **THE PARITY FARM:** one gun per run, does each gun go as far.

## What "done" means tonight (the contract; agreed before any gameplay code)

Two families of metrics.
- **S1-S11 come from the headless judges** (section 1). They are deterministic and run in seconds to minutes with
  dotnet, so builders can run them in their own worktrees.
- **S12-S16 come from the Player:** the reel, the scanner, tests and the blind reviewers.

Build 1's tree fills the baseline column with every NIGHT19 block off (NIGHT18's sandbox). "(inferred)" marks a target
no source publishes. The lead may retune each inferred target once, at round 1, with the reason and both values logged.

| # | Metric | How it's measured | Target |
| --- | --- | --- | --- |
| S1 | **No gun is dominated** | THE RANGE: for every pair of reachable guns, compare the cell scores | no gun scores better than or within 5 % of another in every cell |
| S2 | **Every gun has a niche** | THE RANGE | each reachable gun is best, or within 10 % of best, in at least 2 cells; no gun is best in more than 35 % of cells |
| S3 | **No gun falls off** | THE RANGE, cell C7 (round-20 HP, each gun at its forge tier) | each gun clears within 1.6 x the best gun's time (inferred) |
| S4 | **Combos pay** | THE RANGE's pair mode (section 1) | at least 5 named combos; every reachable gun is in at least 1; each combo scores at least 20 % better than the best single gun in its cell |
| S5 | **One gun goes as far as another** | THE PARITY FARM: `--gun <id>`, normal, 40 seeds per gun | every gun's median round within ±20 % of the median across guns (inferred) |
| S6 | **Free play uses many guns** | the farm (normal, 100 seeds, the bot choosing by THE RANGE's cell table) | top gun's kill share ≤ 30 %; at least 5 guns at ≥ 5 % (the research: ≤ 25 % and 6) |
| S7 | **Rotation pays, a favourite still works** | THE RANGE's points mode: points per minute, rotating vs one gun, in C6 | ratio 1.3-2.0 (inferred); damage identical with freshness on and off (a test) |
| S8 | **Kiting isn't free** | THE CHASE, KITE bot | survives ≤ 120 s at the round-10 roster and ≤ 60 s at round 15, median of 10 seeds (inferred) |
| S9 | **Movement still matters** | THE CHASE, FIGHT vs STAND | at round 10 the moving autopilot takes ≤ 50 % of the damage per minute of one that never moves |
| S10 | **The gust is a tool, not an escape hatch** | THE CHASE + `SimHarness spam` + THE RANGE | adds ≤ 25 % to KITE's survival; a gust-jump gains 2-4 m of height, below the COMET's rocket jump; in C3 it pushes at least 3 of 8 SWARMERs back ≥ 3 m |
| S11 | **Sprint costs less, earned back by fighting** | tests + THE CHASE | a full bar sprints 3.0-3.6 s (drain 0.3); a kill within 6 m refills ≥ 0.25, one beyond 15 m ≤ 0.1; FIGHT spends less time on an empty bar than KITE |
| S12 | **The pad shows the pad** | the reel's pad pass (section 1) + the UI scanner | 0 visible keyboard / mouse tokens while the pad is the last device, on every screen and prompt scenario; 0 pad tokens on the keyboard pass; glyphs are drawn sprites for Xbox, PlayStation and Switch |
| S13 | **The bindings** | EditMode tests + the Player smoke | L3 sprint (TOGGLE / HOLD), R3 punch and parry, LT gust; a saved v1 layout migrates; LB / RB switch tabs on SETTINGS, CONTROLS, RESULTS and the title's run setup; the smoke's pad steps pass |
| S14 | **The centre is clear** | the scanner on hud-fight, sprint and boost | 0 persistent HUD elements inside the central 40 % x 40 %, except the crosshair, the hit and kill markers and transient pops; the stamina pips respond within 2 frames of a boost |
| S15 | **Not cheap** | two blind reviewers per round, `Builds/n18-before` vs tonight (section 6) | tonight's side picked in ≥ 70 % of judgements over the HUD, menu and weapon scenarios; no "too much"; 0 of section 6's banned patterns ticked on tonight's side by both reviewers |
| S16 | **Cost** | `-skygrindBench 60` (24 husks), same power plan | p99 ≤ baseline + 1.0 ms |
| S17 | **The gates** | golden, the replays, the fingerprint, `SimHarness impact`, the smoke, the suite | 0 mismatches, 22 exact; the fingerprint EQUAL to NIGHT18's with every NIGHT19 block off (rule 3); `impact PASS`; all steps (one re-run allowed, nothing else running); the suite green |

## How tonight works (and why)

1. **Measure before changing.** The judges come first. Their baseline is NIGHT18's sandbox, with every NIGHT19 block
   off. Without it, "balanced" is an opinion.
2. **A builder's brief is a metric, never an adjective.** "S2: the STITCHER is best in 0 cells (C3 0.82 x the
   THUNDERCLAP); target 2" is a brief. "Make the nailgun better" is not. A builder is done when the judge shows the
   target met, not when the code compiles.
3. **Favour, never require.** An enemy trait makes one gun better against it; every gun can still kill it. id cut
   "such specific solutions" from DOOM: The Dark Ages because players hated being forced (the research, section 2).
   S1 enforces it: a gun that is useless against one kind still has its other cells.
4. **Sidegrades, not upgrades.** ULTRAKILL's variants keep the primary's damage and change the alt-fire; Hades'
   aspects change the verb. The FORGE and the new guns sell what a gun does, not a bigger number (FORGE switch).
5. **Port before inventing.** The web already has the 8 POWER guns (`src/combat/guns.js`, 636 lines), six more enemy
   kinds (`src/enemies/`: charger, drone, lancer, bulwark, hive, orbiter) and the affixes (`affixes.js`: SHIELDED,
   VOLATILE, BLINKER and more). Core already knows parts of them: `GunMath.ThumperWave`, `Impact.cs:5-8`, and
   `weapons.specials.*` in config. **Search the web and Core before you write a mechanic.** A port is line for line,
   with the web's comments (CLAUDE.md, Architecture).
6. **Separate, blind judges for the look** (section 6); the lead's own opinion of a frame is advisory.
7. **The loop: never idle, and go beyond.** When an item meets its done-when on a build before 4:10, its owner takes
   the item's **beyond** list, then the backlog (U3-33.9). A met item is never "polished" without a new number to move.
8. **State lives in files.** After every integration the log's last line is
   `NEXT: <what the lead does next, which builders are out, what each owes>`.
9. **Your context will be compacted during this run; that is expected.** Don't stop or rush because of it; commit
   finished work before long steps, so nothing significant is ever uncommitted. **After a compaction, or any fresh
   start**, before new work:
   - read `NIGHT19-LOG.md`'s first `date` line and its last `NEXT:` line;
   - read `tasks.json`'s U3-33 rows and `git log --oneline -8`;
   - run `ListAgents` (who is out, who owes what);
   - run `date`, against the first;
   - run `bash Tools/unity-loop.sh status`.

   Then carry on from `NEXT:`.
10. **Don't game the judges.** THE RANGE's cells, its aim model and its scores, THE CHASE's bots and rosters, the
    farm's settings and every metric rule are frozen once the baseline is logged.
    - A builder never edits `Tools/SimHarness/Harness.Sandbox.cs`, `Tools/sandbox-cells.json` or a target. A builder
      who thinks one is wrong sends the lead the evidence.
    - A change that only the harness can see is a failure, even when the number passes. Examples: special-casing a
      cell's setup, or a bot rule that fires only in THE RANGE.
    - Tests verify behaviour; they don't define it. If a test is wrong, say so; never work around it.
11. **No placeholders.** A ported gun or enemy without its web model, its view, its tells and its events is SHORT, not
    DONE. So is a glyph set with missing buttons, or a combo the player can't see happen.
12. **Every status claim cites a tool result from this session:** a command's output, a judge's row, a file and line.

## Read these files

- **First:** `CLAUDE.md`, then this file, then `Research/2026-10-06-weapon-sandbox.md` (all of it) and
  `Research/2026-10-06-assets-and-prompting.md` (the vendoring steps, the ranked pulls, the checklist).
- **`HANDOFF.md`:** sections 0, 1, 5, 6, 7.
- **`NIGHT18-LOG.md`:** the close table and the round tables. **`NIGHT18-REPORT.md`:** all of it.
- **`Research/2026-10-04-controller.md`:** sections 7-8 (menus on a pad, the checklist's glyph rows).
- **`ZOMBIES-ULTRAKILL-RESEARCH.md`:** sections 3-4.
- **`IDENTITY.md`**, and **`PORT-RULES.md`** sections 6, 11, 12.
- **The web, read only:** `src/combat/guns.js`; `src/enemies/charger.js`, `drone.js`, `affixes.js`; `src/ai/aim.js`.
- **`tasks.json`:** write U3-33 and U3-33.1 .. U3-33.10 before any code. `NIGHT20.md` (map night 2) uses U3-32.
- **The memory notes:** `player-smoke-focus-pause`, `play-check-eval-recipe`, `unity-editor-unfocused-autotick`,
  `night-parallel-agents`, `skygrind-targets-and-taste`, `feel-done-means-measured`.

**Run length: at least 5 hours, hard cap 5.5 hours, from your first `date`.**
- The close starts at 4:20 (the code freeze, then the final build), and its table prints at 5:00 or later. Run `date`
  right before printing the table, so the transcript proves the time.
- Running past 5:30 is a failure, and so is closing before 5:00.
- Show the time against the budget in every status line, like `3:10 / 5:30`.

## 0. Tonight's rules (on top of CLAUDE.md, which holds except where this prompt, GG's, says otherwise)

1. **Git:**
   - Branch `port/stage3` from its head (`u3-n18` plus docs). Tag `u3-n19` once, last, on the close commit.
   - No push, no merge, nothing on master. The web repo is read only.
   - Before every commit, restore with `git checkout --` the theme SDF font assets (`Assets/Resources/Fonts/*SDF.asset`),
     the TMP fallback asset and `Mobile_RPAsset.asset`; builds and Play mode rewrite them.
   - A package's `Packages/manifest.json` and `packages-lock.json` change is its own commit.
2. **Every NIGHT19 gameplay change sits behind a config block that can be switched off.**
   - The new gameplay numbers live in `Design/unity-config.json`, each block with `enabled` and a `_why`:
     `design.arsenal`, `design.gust`, `design.forge`, `design.freshness`, `design.stamina.n19`, `design.enemies.n19`,
     `design.pad`.
   - The Player copy stays byte-identical. The shared commit adds the test that checks it; none exists today
     (`Editor/SurvivalSetup.cs:116-120` copies the file, but nothing compares the two).
   - **Why:** with every block off, the game is NIGHT18's bit for bit. That is the A side for every judge, and the
     fingerprint proves it (rule 3).
   - The web's numbers stay in `PortExport/config.json`, which the golden vectors read. A retune is an override in a
     design block, read where the web's number is read (e.g. the Loadout constructor, `Combat.cs:376-431`). Each
     override is named in its commit with why the web had no such number.
3. **The determinism wall:**
   - **Golden and replays:** 0 mismatches, 22 exact at every commit that touches Core.
   - **The fingerprint:** EQUAL to NIGHT18's baseline with every NIGHT19 block off, at every Core commit.
     `SimHarness fingerprint --off19` turns them off. Keep NIGHT18's baseline as `FingerprintBaselineN18` for it.
   - **The close re-records the on-baseline once,** with the reason logged (NIGHT16 and NIGHT17 did the same).
   - **`Mover.cs` changes only in the shared commit's sprint hook** (rule 4). Nobody else edits it. A gust that needs
     the Mover asks the lead.
4. **The sprint bit (the lead, the shared commit).** L3 sprint can't live in the input layer alone: `Mover.StaminaStep`
   arms the sprint only on a boost press (`s.armed`, `Mover.cs:1062-1065`). The hook:
   - a `sprint` button in the input frame;
   - `want = rules.sprint && ((s.armed && held && !pressed) || input.sprint.held) && s.bar > 0`, read only when
     `design.stamina.n19.ownKey` is on;
   - THE TAPE format 4 records the bit, and format 3 replays with it false;
   - the PadAction masks, ActionNames, labels and LabelOrder updated (`Gamepad.cs:227-232`; the masks assume the first
     10 actions are frame buttons).

   The commit must show golden 0 / 22, the 16 trajectories bit-exact, the fingerprint EQUAL with the block off, and
   `tape-old` replaying a format-3 tape EQUAL.
5. **Numbers:**
   - Gameplay numbers go in design blocks (rule 2).
   - Visual numbers go in `UnityTuning.*.cs` with a comment.
   - Colours and faces come from `UiTheme` tokens only. Palette roles: cyan = the player's, red-orange = enemies and
     danger, magenta = the RIVAL, gold = points, pale violet-white = the boundary.
   - A new enemy's tells use DANGER; the gust's air uses the player's cyan.
6. **Randomness:** gameplay randomness comes from the run's `Mulberry32`. THE RANGE's aim error has its own seeded
   Mulberry32 stream, never the run's. VFX randomness comes from a render-side stream.
7. **Accessibility:** flashes through `flashGate`; REDUCED MOTION stills the gust's kick; text ≥ 18 px; every glyph
   has a text fallback for screen readers (the word sets stay, behind the sprites).
8. **Performance:** no allocation per frame in new code (PORT-RULES 6), drones' orbs pooled as the web pools them; S16
   holds.
9. **Assets (ASSETS switch):**
   - **Licences:** read each licence file before an import (the research's vendoring steps). Allowed: CC0, MIT,
     BSD-3, OFL, Unlicense. Not allowed:
     - the MMFeedbacks public repo (non-commercial);
     - repositories with no licence file;
     - anything with audio (rule 7);
     - anything that imitates another game's branding or UI one to one.
   - **Where it goes:** art in `Assets/ThirdParty/<Name>/` with its licence beside it; a row per item in a new root
     `THIRD-PARTY.md` (name, version, URL, SPDX id, path, use, date checked); a CREDITS row (or `licenses.txt` beside the
     exe) for the notices.
   - **Packages are the lead's:** imported in the Editor, pinned to a version, each in its own commit, before a builder
     uses one (builders' dotnet builds read the Editor's project files). If a registry fails twice, vendor the tagged
     source with its LICENSE, or mark the pull SHORT.
   - **Re-tint** every imported texture to the palette roles.
10. **The smoke and focus.**
    - A window taking focus pauses the run (memory `player-smoke-focus-pause`).
    - **In rounds,** the smoke may run while builders work. A failing step is re-run once with builders asked to hold
      their tool calls for 5 minutes.
    - **At the close,** it runs alone.
    - The Editor's smoke fails its three TITLE pad steps by design; the Player's smoke is the check.
    - Reels skip the focus pause (`FeelReel.Running`), so reels may run beside builders.
11. **The Editor plays GG's saved map:** set `SurvivalGame.PendingMapId = RunSetup.DefaultMap` and reload in Play mode.
    Never `PickMap`.
12. **Caps.** A cap reached makes its item SHORT with the count.
    - Player builds: 7 (six planned and one spare).
    - Editor Play checks: 6.
    - Reels: 16 on Players (n18-before once; each build's normal pass and pad pass) and 6 in the Editor.
    - Farm batches: 6, each at most 4 processes, never beside the bench or the close's smoke, each waited for and
      closed by PID.
    - Judges (`range`, `chase`, `spam`): no cap; they are headless and builders run them too.
    - Builders: 5, resumable by SendMessage.
    - Reviewer agents: 10 (two per round, two for the final).
    - Research agents: 0 (done).
    - Bench runs: 4 (two on the baseline, two on the final).
    - Lead verification time: 75 minutes, logged per check (suites and the headless gates only).
    - Disk: a reel is about 0.7 GB. Keep full frames for the baseline, the last two rounds and the final.
13. **One driver for the Editor.** Builders:
    - work in worktrees with dotnet only (`Tools/wt-csproj.cjs`), and never run `unity`;
    - write tests first and see them red;
    - use topic-named members in shared partials;
    - hold back smoke-path changes when no build is left.
14. **Waiting:** wait on a build, a smoke or a farm with a background command or `Monitor` (an event per output line),
    not a sleep loop.
15. **No questions, no waiting.**
    - One status line per item in `NIGHT19-LOG.md`.
    - End every message with `N19-STATUS: <d> done, <s> short, <l> left (of 10) · round <r> · judges <passed>/<total> ·
      tests <passed>/<total> · <h:mm> / 5:30`.

## 1. The judges (U3-33.2, the lead; the first thing built)

All three live in `Tools/SimHarness/Harness.Sandbox.cs` and run on `SurvivalSim` headless:
`dotnet run --project Tools/SimHarness -c Release -- <cmd>`. Each writes Markdown and JSON to `Logs/sandbox/<tag>/`.
Each is deterministic: two runs give identical files, and a test checks it.

### THE RANGE (`range [--gun id] [--cell id] [--pair a,b] [--points] [--off19]`)

- **The rig:** a single-gun loadout firing at real enemies, as `ArmoryTests.Rig(weapon)` does (`ArmoryTests.cs:33-50`).
  - A flat test floor in the C# collider.
  - Enemies spawned through the director's own spawn call, with the cell's round HP.
  - The player scripted per cell: stand, strafe ±4 m, back-pedal, or boost every 2 s.
- **The aim model, identical for every gun:**
  - Aim at the body centre; aim at the head when the gun has a head bonus and the target has a head.
  - A seeded error of σ 0.6° standing and 1.5° moving.
  - Projectiles lead the target's current velocity.
  - Fire as fast as the gun allows.
  - Use the gun's LT / alt by the catalog's `altUse` rule. W writes that rule, the same rule the bot reads (no
    harness-only rule: rule 10 of "How tonight works").
- **The punch:** always available, and used as a parry when an enemy within 2 m winds up, for every gun.
- **The score per cell:** time to clear in seconds (capped at 30) + 0.05 x the damage taken in HP (inferred: 20 HP =
  1 s). The median of 10 seeds.
- **The cells** (`Tools/sandbox-cells.json`, tracked, frozen at the baseline):

  | Cell | Situation |
  | --- | --- |
  | C1 husk-rush | 1 HUSK from 20 m, round 5 |
  | C2 husk-close | 3 HUSKs at 4 m, round 5 |
  | C3 swarm | 8 SWARMERs from 15 m, round 8 |
  | C4 spitter-far | 2 SPITTERs at 22 m, round 8 |
  | C5 line | 5 HUSKs in a 2 m corridor at 10-18 m, round 8 |
  | C6 mixed-12 | 6 HUSKs, 6 SWARMERs, 1 SPITTER, round 12, the player strafing |
  | C7 mixed-20 | C6 at round 20, each gun at its forge tier |
  | C8 charger | 1 CHARGER, round 8 (after E's port; n/a before) |
  | C9 drones | 2 DRONEs at the web's hover band, round 10 (after E) |
  | C10 shielded | 3 SHIELDED HUSKs facing the player, round 10 (after E) |
  | C11 on-the-move | C6, the player boosting every 2 s |

- **Combos (`--pair a,b`):** each combo in W's catalog names its setup gun, its payoff gun, its cell and its script
  (the swap timing). The pair's score is compared with the best single gun in that cell (S4). A combo the harness can
  run but the player can't see happen fails rule 11.
- **Points mode (`--points`):** points per minute in C6, rotating every 3 s vs one gun only (S7).
- **The reachable guns:** the KESTREL, THUNDERCLAP, STITCHER, COMET and LONGWAVE, plus each port as it lands. The
  GAUNTLET is the melee slot, scored but outside S1-S2.

### THE CHASE (`chase [--bot kite|stand|fight] [--round 10|15] [--gust on|off] [--off19]`)

- **The map:** the Survival map; the director's own waves for the round (its spawn rules and `peakFade` as
  configured); 10 seeds; a 300 s limit.
- **The bots:**
  - **KITE:** never fires or punches. It circles the open floor, sprints, boosts when an enemy is within 6 m, slides,
    and uses the gust when it's on.
  - **STAND:** the autopilot that fires and never moves.
  - **FIGHT:** the autopilot as it is.
- **The measures:** survival time, damage per minute, time to the first hit, hits by enemy kind and time on an empty
  bar. These feed S8-S11.

### THE PARITY FARM (`farm --gun <id>`, also `-skygrindFarmGun <id>` on the Player)

- The bot starts with that gun and the GAUNTLET and never buys another gun. It still buys perks, the forge and gates.
- The farm can't force a loadout today: `RunOne` always starts from `survivalStart` (`Game/Bot/Farm.cs`).
- 40 seeds per gun, normal. Use SimHarness's headless farm unless the Player's batchmode is faster (log both times
  once).
- The bot chooses guns by THE RANGE's cell table: the best owned gun for the nearest enemies' kind and range. Today it
  takes the first owned gun from a fixed near / far list (`AutopilotGuns.Choose`). With the table, S6 measures the
  niches, not one hand-written preference. It uses each gun's alt by `altUse`, and punches to parry.

### The reel's additions (FeelReel, phase B, the lead)

- **`gust`:** the KESTREL's gust from a standstill, aimed level and aimed down.
- **`ui-weapons`:** the WEAPONS menu, entering, with the focus moved twice.
- **`vitals`:** a sprint that empties the bar, then a kill that refills it.
- **The pad pass:** `-skygrindReelPad` sets `PlayerDevices.Last` to a pad for the whole reel. Real pads stay disabled
  during a reel (NIGHT18's fix), so this is an override, not a device.
  - It adds the prompt scenarios: a wall buy's prompt, the box, the forge, DOWN, RESULTS, an onboarding card, the
    hotbar, a tip and the kiosk.
  - The UI scanner logs every visible TMP text. `Tools/feel-metrics.py` gets an S12 rule: a token list (E, F, Q, R, TAB,
    SHIFT, CTRL, SPACE, ESC, LMB, RMB, MOUSE, and the keyboard key-chip text) must not appear on the pad pass, and the
    pad words must not appear on the keyboard pass.
  - The S14 rule (the central box) is added too.

**The baseline (about 1:05):**
- the judges on the lead's tree with every block off: `Logs/sandbox/b0/SANDBOX.md` (S1-S11);
- `Builds/n18-before`'s reel (started at U3-33.1): the A side for S15;
- build 1's reel and pad pass: the S12 / S14 baselines.

**Proof the judges tell the truth:** recompute three numbers by hand from the per-shot log and print both:
- one KESTREL cell (25 damage, head x2, 0.4 s: two body shots kill a 40 HP husk, so about 0.4 s plus travel);
- one THUNDERCLAP cell;
- one KITE survival time, against its hit list.

A judge that disagrees with its own log is fixed before it's trusted.

## 2. Who owns what (builders)

| Builder | Owns (creates or edits only these) | Answers for |
| --- | --- | --- |
| **P: the pad, glyphs and menus** | `Core/Gamepad.cs` (DefaultLayout v2 per PAD LAYOUT, `padLayout.v2` with a version field and the v1 migration below, the Sprint and Melee bindings); `Game/Input/*` (PadReader, ReaderInput's L3 latch into the frame's `sprint`, `Glyphs.cs`, `InputPrefs.cs`, `IPlayerInput.cs`); new `Game/Input/PromptText.cs` (the one API every prompt calls); `UnityTuning.Input.cs`, `UnityTuning.Hotbar.cs`'s pad texts; the glyph sprites (Kenney Input Prompts, CC0, in `Assets/ThirdParty/KenneyInputPrompts/` as a TMP sprite asset, laid out by position for Xbox, PlayStation and Switch); `UiNav.cs` and `GamepadMath.MenuNav` (LB / RB raise tab-previous / tab-next); `UiSettings.cs`, `UiControls.cs`, `UiResults.cs`, `UiMeta.cs` (the title's run setup), `UiKiosk.cs`, `UiOnboarding.cs`, `UiRadial.cs`; the tests `GamepadTests`, `ControlsRebindTests`, `PadWheelTests` and new `PadLayoutV2Tests`, `PromptTextTests` | S12, S13 |
| **W: the weapon sandbox** | Core: `Combat.cs`, `CombatMath.cs`, `GunMath.cs`, `Arsenal*.cs`, `ArsenalMath.cs`, `Projectiles.cs`, `Impact.cs`, the forge, `MysteryBox.cs` (the pool and prices), new `Core/Guns/Zapper.cs` and `Bouncer.cs` (ported line for line from `src/combat/guns.js`), the KESTREL's gust (`Mover.Impulse`, `Mover.cs:305`, which is ignored during a dash: that's the no-stacking rule, keep it); every reachable gun's LT; the combos; the freshness table; `Game/Bot/AutopilotGuns.cs` (the cell-table choice, `altUse`, the parry punch); `design.arsenal`, `design.gust`, `design.forge`, `design.freshness` in both config copies; the gun tests | S1-S7, S10 (with E) |
| **E: threats and stamina** | Core: `Director.cs`, the enemy files (husk, swarmer, spitter), new ports `Core/Enemies/Charger.cs`, `Drone.cs` (with its pooled orbs) and `Affixes.cs` (SHIELDED first), from `src/enemies/*.js` and the `src/ai/*` pieces they need, line for line; `Stamina.cs` and `SurvivalSim.Stamina.cs` (`design.stamina.n19`: the drain, the refill, the rewards by range); `design.enemies.n19` (the web's rounds for the new kinds) in both config copies; the new kinds' views as new partials `Game/EnemyViews.Charger.cs`, `.Drone.cs`, `.Affix.cs` (the web's blocky models rebuilt as they are, their tells in DANGER, their `FeelProbe.Enemy` calls); never `Mover.cs` | S8, S9, S11; the C8-C10 cells |
| **H: the HUD and the menus' look** | `Ui.cs` (the HUD: the vitals cluster), `UiStaminaBar.cs`, `StaminaBarLook.cs`, `StaminaHud.cs`, `UiHotbar.cs` (the weapon card: a role tag, the freshness tier, the alt's meter), `UiStyleMeter.cs` (the combo callouts), the WEAPONS menu's view (a role card per gun: "good against", "combos with"), `UiFeelHud.cs`, `UiMenus.cs`, `UiKit` / `UiTheme` additions (cut-corner and skewed plates), new `UnityTuning.Hud19.cs`; UI assets after the lead imports them (UIEffect, LitMotion, ParticleEffectForUGUI) and a monospace numeral face (Share Tech Mono, OFL) in `Assets/Fonts/` with its OFL.txt | S14, S15 (the HUD and menus) |
| **V: the hands and effects** | `Viewmodel*.cs` (the gust's pose and kick, the R3 jab, every gun's alt-fire animation, the ported guns' viewmodels from `src/assets/weaponModels.js` as they are; springs ported from Spring-It-On, MIT, with its notice); `CameraFeel.cs` (the gust's FOV kick, through its composition point); `MovementFx.cs` (the gust's wake); `ImpactFx.cs`, `HitSpark.cs`, `Chunks.cs` (textured, from the Kenney Particle Pack, CC0, in `Assets/ThirdParty/KenneyParticles/`, re-tinted); the ported guns' projectile and beam views (new `Game/GunFx.Zapper.cs`, `GunFx.Bouncer.cs`); new `UnityTuning.Viewmodel19.cs` | S15 (the weapon scenarios), the gust's look |
| **The lead** | the shared commit; `Tools/SimHarness/Harness.Sandbox.cs`, `Tools/sandbox-cells.json`; FeelReel's phase B and the pad pass; feel-metrics.py's S12 / S14 rules; `PlayerChecks.cs`; `SurvivalGame*.cs` loop lines; `Mover.cs` (the sprint hook only); the packages; every doc; the Editor, git, the reviewers, the builds | the judges, the rounds, the gates |

**The v1 migration (P):**
- A saved `skygrind.padLayout.v1` equal to the v1 default becomes the v2 default.
- A customised v1 keeps its buttons. It gains Sprint on L3 and Melee on R3 only where those buttons still hold the
  default Grapple or LockOn; otherwise those two stay unbound, and CONTROLS shows UNBOUND.
- The result is written as v2; v1 is kept for a rollback.
- Grapple must stay bound (it's in the required set, `Gamepad.cs:227,293`), so it shares RB with SPECIAL, as X carries
  Interact + Reel. The rope is off in Survival anyway (`SurvivalSim.cs:161`).

**The shared commit (0:15-0:40, the lead), placed now so no builder edits another's file:**
- the sprint bit and the Mover hook (rule 4), with its gates;
- `PromptText.For(action, hold)` as a stub that returns today's text, called from every prompt site in other builders'
  files: `Ui.cs:891` and `:381`, `UiHotbar.cs:301`, `:439`, `:555-559`, `UiFeelHud.cs`, `UiMenus.cs:440,487` and
  `SurvivalGame.Meta.cs:58`;
- the design blocks, each with `enabled: false`, and the Player-copy byte test;
- the hooks the builders share:
  - `Enemy.Airborne`, `Enemy.Shielded` and `Enemy.Conductive` properties (E sets them, W's combos read them);
  - an `EnemyViews` registry line per new kind;
  - a `FeelProbe.Effect` id for the gust (FeelFx gets a `Gust` member);
- `FingerprintBaselineN18` and `--off19`;
- empty test fixtures: `SandboxHarnessTests`, `PadLayoutV2Tests`, `PromptTextTests`, `GustTests`, `ChargerTests`,
  `DroneTests`, `FreshnessN19Tests`, `StaminaN19Tests`.

Then launch the builders; the common brief is section 7. The judges follow at 0:40-1:05. Until they land, builders
start from the research's proposals and the tests.

## 3. The items (U3-33.1 .. U3-33.10)

**Where an item is decided:** on the final build, by its rows in that build's `SANDBOX.md` and `FEEL-METRICS.md`, and
by the final pair of blind reviewers (`Builds/n18-before` vs tonight, the sides swapped for the second). One judgement
is one reviewer's pick on one scenario. The rounds show progress; the final decides.

### U3-33.1 Start (lead, 0:00-0:15)

- `date`; GG's notes; HANDOFF section 1 row by row; `set_autotick`; the machine (power plan, AC, CPU, free disk ≥ 15 GB);
  RECEIVED in `NIGHT19-LOG.md`.
- Copy `Builds/Stage1/` (NIGHT18's build 7) to `Builds/n18-before/`. Start its reel in the background
  (`-skygrindReel <abs>\Logs\feel\n18-before`) and close it by PID when it ends.
- Log D-72 (ASSETS, replacing D-30) in the log and in HANDOFF section 5.
- **Done when:** the rows are logged, tasks.json parses with U3-33.*, `Builds/n18-before/` exists and its reel has
  started.

### U3-33.2 The judges and the baseline (lead, 0:15-1:15)

- **Done when:**
  - the shared commit is in, with its gates printed;
  - THE RANGE, THE CHASE and the parity farm are committed with `SandboxHarnessTests` green (two runs identical, the
    hand-checked cell);
  - `Logs/sandbox/b0/SANDBOX.md` has every S1-S11 row valued, or "n/a" with its reason;
  - the three hand checks agree, printed side by side;
  - FeelReel's phase B and the pad pass are in, and build 1's reel wrote the S12 / S14 baselines;
  - the bench baseline ran (2 runs, the plan logged);
  - the builders were launched by 0:40, with a `date` output at the launch.

### U3-33.3 The pad plays like a shooter (P)

- **Sprint:** L3 per L3 SPRINT, through the frame's sprint bit; L-SHIFT and LB keep today's boost-then-hold.
- **Melee:** R3 is the punch and parry: 10 damage, 2 m, 0.35 s, with the parry window and the perfect parry
  (`Combat.cs:1039-1116`). It stays a weak quick melee, as GG said ("not as strong but it is something").
- **The layout:** PAD LAYOUT's default with the v1 migration.
- **Menus:** LB / RB switch tabs on SETTINGS (`SwitchTab`, `UiSettings.cs:381-402`), CONTROLS, RESULTS (SUMMARY /
  DETAILS) and the title's run setup (SURVIVAL / DAILY), with the tab's slide-in motion. The Night13 smoke's d-pad tab
  step must still pass (`SliceSmoke.Night13.cs:210-214`).
- **Done when:** S13 passes: the tests named, and the Player smoke's pad steps on the final build.
- **Beyond:** a HOLD / TOGGLE row for L3 in CONTROLS; a hint card the first time a pad is used (L3 sprint, R3 melee,
  LT gust); the Steam Deck layout check (`Research/2026-10-04-controller.md` checklist rows 1-3).

### U3-33.4 The pad shows the pad (P)

- **What:** every prompt goes through `PromptText`. It reads the live binding (remaps included) and draws the glyph
  sprite for the last device: Xbox, PlayStation or Switch, laid out by position, with the word set as the fallback.
  This also covers:
  - the hotbar's hardcoded texts and the break line;
  - the WHEEL prompt bug (action "radial" vs the pad's "weaponWheel");
  - the kiosk rows;
  - a HOLD prompt drawn as a fill ring on the glyph.
- **Done when:**
  - S12 passes on the final build's pad pass and keyboard pass;
  - `PromptTextTests` show every bound action with a sprite in all three sets;
  - the pad pass's flipbooks show drawn buttons on the buy prompt, DOWN and RESULTS.
- **Beyond:** the CONTROLS screen draws glyphs, not words; the d-pad and stick glyphs animate on a HOLD.

### U3-33.5 The KESTREL's wind gust, and an LT for every gun (W, with V's look)

- **The gust** (KESTREL LT, design.gust; starting numbers from the unported `weapons.specials.shotgun.shock`: a 30°
  cone, 8 m, push 12, stagger 1):
  - It pushes the player opposite the aim through `Mover.Impulse`.
  - It pushes and staggers light enemies in a cone.
  - A charge refills on landing or after a cooldown, so there's one gust per airtime, plus one on the ground.
  - No effect during a dash, by Mover's own rule.
  - A small stamina cost, if THE CHASE needs one.
  - With KESTREL LT = GUST, the zoom and everything that reads `adsActive` go from the KESTREL: the ricochet, the
    x1.2 headshots, the walk x0.85, AimAssist (`SurvivalSim.cs:166`), the viewmodel and the ZOOM prompt. Each reader is
    checked, and the precision role moves to the KESTREL's primary (head x2) and its POP combo.
- **Every reachable gun's LT does a distinct job.** The research's niches are the starting point:
  - THUNDERCLAP: a point-blank parry;
  - LONGWAVE: its overcharge, moved to LT;
  - STITCHER: the detonation it has;
  - COMET: steering;
  - the ports: their web alts.
- **Done when:**
  - `GustTests` are green;
  - S10 passes;
  - each reachable gun's alt beats its primary in at least one RANGE cell, or is a listed movement or utility tool;
  - both reviewers answer yes to "does the gust read as a burst of air that moves you?" on the `gust` flipbook (2 of
    2);
  - the gust emits a named event (`gust`) for GG's sounds.
- **Beyond:** a gust-cancel (gust out of a slide keeps the slide's speed, if S8 holds); the gust reflects SPITTER globs
  like a parry.

### U3-33.6 Every gun has a job (W)

- **Niches:** one per gun, by range band and job (the research's "What SKYGRIND could take" 4-10). Retune through
  `design.arsenal` only: the THUNDERCLAP point blank with a steep falloff, the STITCHER as the battery, the COMET for
  crowds and lift, the LONGWAVE for lines and armour, the KESTREL as the trigger.
- **Ports:** ZAPPER and BOUNCER from the web, with V's viewmodels and effects, and their wall-buy prices and box
  entries.
- **Combos** (each visible to the player as a named callout through H's style meter, each with an event):
  - POP: the KESTREL hits a live COMET rocket or BOUNCER grenade;
  - CONDUCTOR: STITCHER nails, then a ZAPPER or LONGWAVE hit;
  - LAUNCH + SNIPE: the COMET launches, then any hit on an airborne enemy does x2;
  - SWAP-CANCEL: today's primed swap, made the DOOM rhythm;
  - one more of W's own.
- **FORGE** per the FORGE switch; **freshness** per the FRESHNESS switch (the research's table: per-shot cost
  1.4 x the gun's interval, +0.5 to every other gun per shot, +0.1/s holstered, FRESH as the floor with one gun owned).
- **Progression:**
  - wall-buy prices by role, not power;
  - the box pool includes the ports;
  - enemy HP growth (`Director.cs:172`) stays the web's unless S3 fails at round 20; a fix goes through the forge
    first.
- **Done when:** S1-S7 pass on the final tree, the ports fire with the web's numbers and models, and the combos are on
  screen in the reel (`hud-fight` or a new `combo` scenario).
- **Beyond:**
  - VORTEX and TORCH ported (GATHER + BURST, IGNITE);
  - S6 at the research's ≤ 25 % / 6 guns;
  - a "combos with" line in the WEAPONS menu per gun;
  - the KESTREL's POP made a showpiece (a freeze step, H's callout).

### U3-33.7 Threats that catch you, stamina you earn (E)

- **Ports, as they are:**
  - the CHARGER: a 0.8 s telegraph with a painted charge line, a 22 m/s charge, stunned on a wall with its back weak
    point x3;
  - the DRONE: hovers 4-8 m up, keeps 12-18 m away, parryable orbs, pooled;
  - the SHIELDED affix: a front plate; a headshot or a melee hit breaks it.

  Each spawns at the web's rounds, with its view, its tells and its events.
- **Stamina** (SPRINT DRAIN):
  - the drain at the switch's value;
  - the refill and the rewards by range, the research's 23-24 (a kill within 6 m +0.25, beyond 15 m +0.05; a slam
    +0.25 only when it hits; a parry +1);
  - one bar at the start, VIGOR +1 bar: "start with less, earn more" stays.
- **Pursuers, if S8 still fails after the ports:** the research's patterns, as overrides on the ported kinds, never as
  new kinds:
  - a marker that trails then predicts;
  - bursts that lead more with each shot;
  - homing faster than the sprint but slower than the boost.

  Each one is beaten by a dash, a parry or a jump, not by running.
- **Done when:**
  - S8, S9 and S11 pass;
  - C8-C10 each have a best gun that is not the THUNDERCLAP, and are clearable by every gun (S1 holds);
  - the normal farm's median round is ≥ 0.8 x NIGHT18's (harder, not a wall);
  - the ported kinds' tests are green.
- **Beyond:** the LANCER (perches; its laser follows with lag, so movement escapes it); the VOLATILE and BLINKER
  affixes; a BULWARK only if the map has its pen, otherwise SHORT with that reason.

### U3-33.8 Not cheap: the vitals cluster, the HUD, menus and effects (H, V)

- **The stamina leaves the centre.** It becomes chunky, segmented pips in the bottom-left vitals plate (`Ui.cs:297-325`),
  next to HP: one pip per bar, a slight skew, hard edges, one dark plate, a drain flash and a refill tick. The swap-name
  plate and the prompts placed relative to the old bar move with it. ULTRAKILL's Standard HUD is the feel to match
  (information in one block, the centre kept empty), not the art to copy.
- **The banned patterns** (section 6), applied to every HUD and menu screen tonight. The list grows after each round
  from the reviewers' words.
- **The assets (ASSETS switch), in this order:**
  1. UIEffect (shine sweeps, dissolve, edge glow on plates, rank letters and focus);
  2. LitMotion (zero-allocation tweens and per-character TMP reveals);
  3. ParticleEffectForUGUI (real bursts for rank-up and points);
  4. the Kenney Particle Pack (V);
  5. Unity UI Extensions' cut corners;
  6. the monospace numerals.
- **The look of the new moves** (V): the gust's wake and kick; the R3 jab's pose; each alt-fire's animation; the
  ports' viewmodels as the web's models; textured sparks.
- **NIGHT18's leftovers:**
  - horde5's spark confetti and roundend's pops over the banner (both reviewers' "too much");
  - RESULTS' grade card over the points;
  - M9's two TITLE pop-ins;
  - the CONTROLS / CONTRACTS / WEAPONS bodies that appear at once.

  Leftovers in P's files (`UiControls.cs`, `UiMeta.cs`, `UiResults.cs`) are P's; the rest are H's.
- **Done when:** S14 passes, S15 passes on the final pair, M9 / M10 / M11 stay at 0 on the scenarios where NIGHT18
  passed them, every import has its THIRD-PARTY.md row and licence file, and S16 holds.
- **Beyond:** the RESULTS and title passes with UIEffect and per-character reveals; a weapon-role card animation in
  WEAPONS; the banned list's second pass.

### U3-33.9 The loop: rounds, and beyond (lead)

- **Four rounds** on builds 2-5 (about 1:45, 2:25, 3:05, 3:45; about 40 minutes each; round 4 ends by 4:10), plus a
  fifth on the spare build if the clock allows. Each round runs:
  1. the build;
  2. the smoke (rule 10);
  3. the reel and the pad pass;
  4. `feel-metrics.py` against `n18-before` and the previous round;
  5. the judges on the build's tree (`SANDBOX.md`);
  6. the parity farm (rounds 2 and 4, within the farm cap);
  7. two blind reviewers;
  8. the fix lists.
- **A fix list** names each failed metric with its value and target, and each lost judgement with the reviewer's
  reason.
- **Going beyond:** an owner whose item meets its done-when on a build takes that item's beyond list, then the backlog
  below, in order:
  1. NIGHT18's open "too much" items, if still open;
  2. the CONTROLS screen's glyph pass;
  3. VORTEX / TORCH;
  4. the LANCER;
  5. a per-gun kill flourish for each port;
  6. the WEAPONS menu's combo dictionary.
- **Done when:** four rounds are logged, each with its tables, both reviewers' verdicts and rubric scores, the fix
  lists and what changed; every beyond or backlog item taken has a commit, a number and a two-line review.

### U3-33.10 Close (lead, from 4:20; table at 5:00 or later; hard stop 5:30)

- **The code freezes at 4:20, and the final build starts at once** (allow 15 minutes).
- **Gates:** the suite on the frozen code, async in the Editor, while the Player checks and the docs proceed.
- **What the transcript must show, so the /goal checker can see it:**
  - the decoded blind picks (a table per scenario: reviewer, pick, side);
  - the final `SANDBOX.md` and `FEEL-METRICS.md` tables;
  - the gate lines;
  - a `date` output before the table.
- **On the final build:**
  - the smoke, alone;
  - `-skygrindGolden`;
  - `-skygrindFingerprint`, with the off-path check against `FingerprintBaselineN18`;
  - `-skygrindReplay` on the smoke's tape;
  - the bench (2 runs);
  - the final reels (normal and pad);
  - the final judges and the final parity farm;
  - the final pair of blind reviewers.
- **Re-record** the fingerprint's on-baseline once, with the reason.
- **The docs** are drafted by 4:20, so the close only fills in numbers:
  - `NIGHT19-LOG.md`.
  - `NIGHT19-REPORT.md`, GG's one page:
    - the gun x cell table, before and after;
    - the kill shares;
    - THE CHASE's survival times;
    - the blind verdicts;
    - before / after GIFs for `hud-fight`, `gust`, `vitals` and `ui-weapons`;
    - the questions.
  - `FEEL-PLAYTEST.md` rewritten as a 10-minute pad-first walk (L3, R3, the gust, LB / RB, each gun's job, each combo).
  - `THIRD-PARTY.md`; PORT-STATUS; HANDOFF (the `handoff` skill, anchor `u3-n19`); tasks.json.
- **Tag `u3-n19`;** print the close table, then `NIGHT19 CLOSED`.

## 4. The clock (from the first `date`)

| When | Lead | Builders |
| --- | --- | --- |
| 0:00-0:15 | U3-33.1; n18-before's reel in the background | none |
| 0:15-0:40 | the shared commit (sprint hook and gates, PromptText stubs, blocks off, hooks, fixtures); launch P, W, E, H, V by 0:40 | none |
| 0:40-1:15 | THE RANGE, THE CHASE, the parity farm; the baseline; the hand checks; the packages; FeelReel phase B and the pad pass; build 1 at about 1:05 | first passes against the research's proposals and the tests: P the layout and PromptText; W the niches and the gust; E the CHARGER port and the stamina block; H the vitals cluster; V the gust's look and the textured sparks |
| 1:15-4:10 | rounds 1-4 on builds 2-5, the judges after every integration, the Editor reel between builds | fix lists, then beyond, then the backlog |
| 4:10-4:20 | the last fixes, checked by the judges and the Editor reel; the docs' drafts | the last fixes |
| 4:20-5:30 | U3-33.10 (table at 5:00 or later) | none |

**The cut order if short:**
1. The backlog.
2. Beyond lists.
3. The DRONE (keep the CHARGER and SHIELDED).
4. The BOUNCER (keep the ZAPPER).
5. The fourth round.

**Never cut:** the judges and their baseline, S1-S2, S8, S12-S14, the blind reviewers, the gates, the close.

## 5. The rubric for continuity (NIGHT17's 10 rows, 1-5, anchored)

Each reviewer still scores the 10 rows for B, against NIGHT18's anchors, so the trend continues
(`Logs/feel/final/flip-on/` for kills, the hit and movement; NIGHT18's settings and title frames for the menu and
identity rows). A 5 needs a reason that names a frame. These scores are context; the judges, the metrics and the blind
picks decide the items.

## 6. The reviewer brief (two per round, fresh, read-only), and the banned patterns

> You are judging a game's screens and moves from frames only. You won't see code, metrics or anyone's notes. For each
> scenario you get flipbook sheets for A and B: the same moment rendered by two versions of the game, 8 key frames in
> order, near native resolution. Crop or zoom with PIL if a detail matters.
>
> For each scenario, answer the question with **A**, **B**, **Same** or **Unknown**, and give one line naming the
> frames that decided it:
> - "Which HUD looks more finished, less cheap?"
> - "Which keeps the middle of the screen clear while still showing the stamina?"
> - "Which menu looks more finished?"
> - "Which weapon moment reads better?"
>
> Then answer "Is either one too much (noisy, hard to read, tiring)?".
>
> For `gust` (B only), answer yes or no: "Does this read as a burst of air that moves the player?"
>
> Then tick each banned pattern below that you can see in A and in B, naming a frame for each tick.
>
> Then score the 10-row rubric for B only, against the anchor images given, one line per row.
>
> Return a table, under 700 words.

**The banned patterns** (from GG's and NIGHT18's reviewers' words, and the Opus 5.5 prompting page's advice to name the
patterns you don't want, not ask for "less generic"; extend the list after each round):
1. Flat TMP text on a flat rectangle, with no edge, cut, texture or depth.
2. Default rounded Unity sprites and pill buttons.
3. Soft drop shadows and outer glows on everything; gradient fills as decoration.
4. One ease for every motion, or a rubbery overshoot on everything.
5. Elements that pop in at full size and alpha (M9).
6. Confetti: many small sparks after the main hit (NIGHT18: horde5).
7. A bloom or blur that hides the thing it celebrates (NIGHT18 round 1: the kill).
8. Effects over text: pops over a banner, a grade card over the points (NIGHT18).
9. Persistent HUD in the middle of the screen (GG: the stamina bar).
10. Button names written as words ("LT", "RB", "HOLD E") where a drawn glyph belongs, and keyboard letters on a pad.
11. Colours outside their roles.
12. Centred stacks of same-weight text with no hierarchy.
13. Untextured quads as particles.
14. Grey boxes where a gun or an enemy model belongs.

The lead runs each round's A/B in both orders, decodes the picks with the key, and logs each reviewer's pick per
scenario. "Same" and "Unknown" count against an item. A "too much" on an item's scenario holds that item at SHORT until
it's fixed.

**The scenarios:**
- S15's HUD: hud-fight, vitals (or sprint for A), pickup, rankup.
- Menus: ui-title, ui-settings, ui-weapons (new: A is n18-before's WEAPONS screen when present, otherwise B is judged
  alone against the banned list), ui-results.
- Weapons: kill12, punchkill, swapall, gust (B only).

## 7. The builder brief (common part; each builder also gets its rows)

**A fix-list entry names the metric, the evidence and the target, never an adjective.**

<example type="good">
W, S2 (THE RANGE b2): the STITCHER is best in 0 cells. It's 1.6 x the THUNDERCLAP in C3 (swarm), 1.4 x the KESTREL
in C4. Its nails don't stay in (no CONDUCTOR), and at 9 damage it's a worse THUNDERCLAP at every range.
Make it the battery: nails stay in, a volt hit arcs by nail count (CONDUCTOR). Then re-run `range --gun nailgun` and
`range --pair nailgun,zapper`.
Expect C3 within 10 % of best and CONDUCTOR ≥ 20 % over the best single gun in C6.
</example>

<example type="good">
E, S8 (THE CHASE b2): KITE survives 300 s (the limit) at round 10. SWARMERs at 10 m/s never close on the 10.5 m/s
sprint; hits come only from SPITTER globs (6 of 6). The CHARGER isn't in the round-10 roster yet (the web adds it at
round <n>: check `director.js`). Port the CHARGER's spawn rule, then re-run `chase --bot kite --round 10`.
Expect ≤ 120 s.
</example>

<example type="bad">
W: the nailgun still feels weak; make it more fun and give it a purpose.
</example>

<example type="bad">
H: make the HUD look less cheap and more like ULTRAKILL.
</example>

The bad ones carry no number, no evidence and no way to know when they're done. NIGHT17 sent briefs like them and got
code that met the words but not the screen.

- **Objective:** your metrics' targets on the next build's judges or reel. Code that compiles is not done.
- **Before writing a mechanic:** search Core and the web (`src/combat/`, `src/enemies/`, `src/ai/`) for it; port
  rather than invent.
- **Rules:**
  - tests first for pure rules (seen red);
  - every new behaviour behind its design block;
  - call `FeelProbe` from your effects;
  - no allocation per frame;
  - never edit another builder's file (ask the lead with the exact lines);
  - never edit a judge or a target;
  - never run `unity`;
  - run the judges yourself in your worktree:
    `dotnet run --project Tools/SimHarness -c Release -- range --gun <id>`.
- **Report** (under 400 words; details go in commits and files):
  - the commits;
  - what a player now sees or does differently;
  - which metric moved, by how much (the judge's line);
  - what you didn't do;
  - the dotnet results.

## 8. The close table (print it, then `NIGHT19 CLOSED`)

| Item | DONE / SHORT | Evidence ("done when", from this session) |
| --- | --- | --- |
| U3-33.1 Start | | |
| U3-33.2 The judges and the baseline | | |
| U3-33.3 The pad plays like a shooter | | |
| U3-33.4 The pad shows the pad | | |
| U3-33.5 The gust, and an LT for every gun | | |
| U3-33.6 Every gun has a job | | |
| U3-33.7 Threats that catch you, stamina you earn | | |
| U3-33.8 Not cheap | | |
| U3-33.9 The loop | | |
| U3-33.10 Close | | |

SHORT needs its measured reason: a cap reached, a decision reserved for GG, a blocker after two approaches, or the time
limit. A SHORT on U3-33.3-33.8 states its metric values and blind-judgement counts.

## 9. The /goal for GG to paste

```text
/goal Run NIGHT19.md in C:\Users\gaige\dev\SKYGRIND-unity. Run `date` first, then read CLAUDE.md, NIGHT19.md and the files it lists, and work U3-33.1 .. U3-33.10 in NIGHT19.md's order with at most five background builders split by file ownership, looping rounds (build, judges, reel, blind reviewers, fix lists) and taking each item's beyond list once it is met, until the close. Never ask me questions and don't stop early; end every message with the N19-STATUS line. Never push, merge or commit to master; the web repo is read only. Run at least 5 hours: don't start the close before 4:20 or print its table before 5:00 from the first `date` (run `date` right before the table); hard cap 5.5 hours. This goal is met when your latest message prints NIGHT19.md's close table with every item U3-33.1 .. U3-33.10 marked DONE (with its "done when" evidence from this session) or SHORT (with its measured reason: a cap reached, a decision reserved for GG, a blocker after two approaches, or the time limit once a `date` output reads 5 hours or more after the first `date` output), followed by the line "NIGHT19 CLOSED". SHORT rows count as met.
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
