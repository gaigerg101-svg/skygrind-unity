# NIGHT17-LOG: the overhaul night (U3-30.1 .. U3-30.11)

First `date`: **Mon Oct 5 20:55:38 USMST 2026** (epoch 1791258938). The close starts no earlier than 04:55:38; the hard
cap is 05:55:38. GG's switches as written in NIGHT17.md: **AUDIO OFF, CHUNKS ON, MARKS OFF, STAMINA KEY ONE KEY**.

## Status lines (one per item)

| Item | Status | Line |
| --- | --- | --- |
| U3-30.1 Start | DONE 21:02 | RECEIVED rows logged, tasks.json parses (148 items), round 0 scored 17 / 50 with 18 captures |
| U3-30.2 The identity | DONE 21:39 | fonts + tile `fcdc730`, tokens + IDENTITY.md `804aea6`; the tile reviewed (below); builders told 21:40 |
| U3-30.3 The shared commit | DONE 21:37 | `e97d6a0` `611afab` `89429cd` `8e5b416` + the reel `c78a214`; compile clean; fingerprint EQUAL; suite 849 / 850 + the one re-run green (below); build 1's reel ran (24 captures); builders launched 21:16 |
| U3-30.4 One bar, a sprint, a partial boost | DONE 22:31 | golden 0 mismatches, 22 exact (21:58 and every gate since); StaminaTests 0 / 21 red -> 21 / 21; spam: sprint 10.5 m/s under the slide's 11.6, a 0.5-fill boost 15 steps / 3.00 m = 0.50 x a full boost; the fingerprint re-record `964899b`; the boost / sprint strips reviewed rounds 1-4; the farm diff (22:31) |
| U3-30.5 Crunch | SHORT (round 4 was the deadline; reviewers 4 / 4 spent) | strips reviewed every round; rows 1-2 at round 4: the lead 4 / 3, the reviewer 2 / 3 (not both ≥ 4); round 5 / 6 fixes after |
| U3-30.6 The hands | SHORT (same cap) | strips reviewed every round; row 3 at round 4: the lead 3, the reviewer 3; HandsTests green in the Editor suite (968 / 968) |
| U3-30.7 The HUD rebuilt | SHORT (same cap) | HUD captures reviewed every round; rows 5-7 at round 4: the lead 3 / 4 / 3, the reviewer 2 / 3 / 3 |
| U3-30.8 Menus and words | SHORT (same cap) | UI captures reviewed every round; rows 8-10 at round 4: the lead 4 / 4 / 3, the reviewer 4 / 3 / 2; the smoke's menu and pad steps passed on every round's build (b5: on the re-run) |
| U3-30.9 The rounds | DONE 02:22 | rounds 1-4 logged with the lead's and a fresh reviewer's scores, the reviewers' 5 worst, the fix lists and what changed (round 5 in the Editor and at the close) |
| U3-30.10 Beyond the ask | DONE 02:32 | 11 entries taken, each with commits, a capture (the flourish none: why) and a two-line review; 1 left with its reason (U3-30.10's table) |
| U3-30.11 Close | DONE 05:3x | build 6, the smoke's re-run all green, golden / fingerprint / replay, round 5 35 / 50, the docs, tag `u3-n17` (the close table at the end) |

## The verification budget (90 min of lead time)

| Check | When | Lead minutes | Running total |
| --- | --- | --- | --- |
| The before reel (strips + results + UI pass + title tour, `Builds/n16-before`) | 20:57-21:00 | 4 | 4 |
| Arrival gates (CoreHarness, SimHarness fingerprint / twice / residual / spam), in the background | 20:58-20:59 | 1 | 5 |
| The shared commit's suite (async) + the one failing test re-run + fingerprint | 21:12-21:21 | 2 | 7 |
| Build 1 (292 s) + the style tile + build 1's reel (80 s) | 21:27-21:37 | 6 | 13 |
| Integration 1 (H, X, V cherry-picked; Editor compiles; the suite async) | 21:40-21:44 | 2 | 15 |
| Integration 2's gates (CoreHarness, SimHarness fingerprint / twice / residual / spam; the suite async) | 21:55-21:58 | 2 | 17 |
| Build 2 + its smoke + golden + fingerprint + round 1's reel | 22:08-22:14 | 6 | 23 |
| Integrations 3-4's gates (compile, suite async, fingerprint) | 22:24-22:48 | 4 | 27 |
| Build 3 + smoke + reel | 22:48-22:56 | 6 | 33 |
| Integration 6's gates (suite, CoreHarness, fingerprint) | 23:16-23:35 | 3 | 36 |
| Build 4 + two smokes + reel | 23:36-23:45 | 7 | 43 |
| Integration 7's gates (fingerprint, CoreHarness, residual, the suite async) | 01:52-02:02 | 3 | 46 |
| Build 5 + two smokes + reel | 02:03-02:17 | 7 | 53 |
| Integration 8's gates (two suites, CoreHarness, fingerprint, residual, impact) | 02:35-02:55 | 4 | 57 |
| Round 5's Editor check (Play check 1, 7 captures) | 02:55-02:58 | 3 | 60 |
| Round 6's Editor check (Play check 2, 4 captures) + the frozen code's suite and gates | 03:11-03:22 | 4 | 64 |

(The rows from integration 2 on were filled at 03:25 from the log's times. A row's minutes are the lead's own time on the
check, not the background wait.)

## RECEIVED (U3-30.1, 20:55-21:02)

HANDOFF.md section 1, row by row:

| # | Row | Result |
| --- | --- | --- |
| 1 | HEAD, tag | `630d6a2` on `port/stage3`, `u3-n16-1-g630d6a2` (one docs commit after the tag: the research file) |
| 2 | `git status --short` | as expected: ` M ProjectSettings/UnityConnectSettings.asset`, `?? Assets/_Recovery*`, `?? NIGHT7.md` .. `NIGHT18.md`, `?? QUEUE-*.md` |
| 3 | Editor status, auto-tick | **`playMode: playing`** (GG's Play session left on after testing); stopped by the lead at 20:56 for the night's compiles (`Exited play mode`), as NIGHT16 did at 14:31. `set_autotick`: `Auto-tick already enabled (interval updated to 16ms)` |
| 4 | The suite (async) | started 20:59 (`Tests started in async mode`); result below when polled |
| 5 | CoreHarness | `golden total: 39 vector sets (48094 records), 22 replays (22 exact); 0 failing, 0 mismatches` |
| 6 | SimHarness | `fingerprint seed 1: 12 hashes ... EQUAL to the baseline`; `twice` run 1 = run 2 (`round 4, kills 36, shots 80, earned 5102`); `residual check: 7 of 7 EQUAL`; `spam: targets (a) (b) (c) HOLD` |
| 7 | The Player smoke | not run on arrival (the build is unchanged since NIGHT16's close smoke; the first smoke is build 1's) |
| 8 | Power plan, AC, CPU / GPU (20:56) | **Balanced**, AC online; CPU 79 % with GG's Play session running (Unity 21595 s CPU), GPU 61 % / 6650 MiB; re-sampled before any bench |

- `Builds/n16-before/` = a copy of `Builds/Stage1/` (build 4, `u3-n16`, stamped `065fa12-dirty`), for GG's A/B.
- tasks.json: U3-30 and U3-30.1 .. U3-30.11 added after U3-29.10 (a text insertion, the file's layout kept); it parses
  (148 items).
- **The before reel** (`Logs/shots/n17-before/`, 1920 x 1080): 10 strips (freeze = the headshot kill, punch = the punch
  kill, punch-press, parry PERFECT, slowmo, slide, land, swap, draw) + the results strip + the UI pass (ACCESS, LAB,
  PAUSED, DOWN, RESULTS) + the title tour (TITLE, GAMEPAD): **18 captures (18 of 150)**. The UI pass's first run wrote
  nowhere ("Failed to store screen shot": a relative dir resolves under `SKYGRIND_Data/`); re-run with an absolute path.
  Every later reel uses absolute paths.

## Round 0 (the baseline: the n16-before reel), scored by the lead

| # | Row | Score | Why (one line) |
| --- | --- | --- | --- |
| 1 | Kill crunch | 2 | The punch kill holds a static white blob for 80 ms (10 HELD frames): nothing vibrates, nothing flies; the headshot kill at 8 m is a 20 px white flicker that's gone by frame 3. |
| 2 | Hit feedback | 1 | No hit-without-kill on the reel; the code's hit is 5 cubes and a 5 cm flinch (the research audit). |
| 3 | Weapon feel | 2 | The per-gun draws exist (draw strip); the recoil is small; nothing differs much gun to gun at a glance. |
| 4 | Movement feel | 2 | Three thin pips inside the HP plate; no sprint; the dash state isn't readable at a glance. |
| 5 | Score energy | 2 | The points roll (518 -> 624 over 130 ms) in a ~30 px gold number in the corner; the "+159" pop is ~12 px. |
| 6 | Style meter | 1 | A grey "D / DULL" plate about 20 px tall with a hairline bar: no slam, no callouts that read. |
| 7 | HUD clarity | 2 | Key hints at rest ("KEY 1", "RMB ZOOM ×1.5", "1 KESTREL 2 PULSE"); labels on every number; HP digits ~16 px. |
| 8 | Menu simplicity | 1 | PAUSED 7 buttons + a 30-row key card; TITLE 6 buttons + an 8-button setup strip + a 190-character key line + a version stamp; RESULTS 6 buttons + 4 stat lines + the raw score string; SETTINGS 7 tabs. |
| 9 | Identity | 2 | The wordmark (cyan over a red-orange offset) is the one idea; the rest is generic condensed caps on translucent grey plates. |
| 10 | Motion | 2 | A 0.15 s fade and the rank pop exist; menus and RESULTS appear in one frame. |
| | **Total** | **17 / 50** | |

## U3-30.3 the shared commit (21:02-21:37)

- `e97d6a0` UnityTuning.Theme.cs (every identity token), UiTheme (the tokens read once: faces, INK / WHITE / MUTED, the
  cyan value ladder, GOLD, DANGER, hard-shadow materials, a cut-corner sprite), UiTween (allocation-free curves and the
  frame-time structs UiEnter / UiPunch / UiCount / UiSlam), UiShape (a slanted, corner-cut solid Graphic).
- `611afab` the Ui.cs split, a move: UiKit.cs (the lead's shared builders), UiMenus.cs (builder N's menus), Ui.cs keeps
  the HUD (builder H's), 1585 -> 1118 lines; two lines beyond the move (UiTheme.Init first; Text's floor 16 -> 18 px).
  The before / after captures of TITLE / PAUSED / RESULTS: build 1's reel (`n17-b1/n17-title.png`, `n17-paused.png`,
  `n17-results.png`) against `n17-before/` show the same screens (the buttons' face changed only with U3-30.2).
- `89429cd` `game.devMode` (`-skygrindDev` as DevKeys reads it, or an automation flag; off in the Editor and for the
  reel), `design.stamina` (bar / boost / sprint / refill / rewards, each with `enabled` and `_why`; the Player copy
  byte-identical), M's empty StaminaTests fixture. The guards themselves are builder N's (they're in N's files).
- `8e5b416` StaminaHud: the one place the views read the bar (H's bar, X's sprint cue, V's sprint pose), M switches it.
- `c78a214` the feel reel: `-skygrindReel <dir>` = FeelStrip's 11 reel scenarios (kill, headshot, punchkill, hit, horde,
  parry, boost, sprint, swapall, rankup, roundend) + ShotTour.RunReel (TITLE, each SETTINGS tab, the HUD at rest, PAUSED,
  MODS, the HUD in a fight, DOWN, RESULTS); numbers in UnityTuning.Reel.cs.
- **Gates:** Editor compile `compilationFailed: false, consoleErrors: 0` (21:09, 21:11, 21:20); SimHarness `fingerprint
  seed 1: 12 hashes ... EQUAL to the baseline` (21:14); the suite (async, 21:12-21:20) `849 / 850`: the one failure
  `AutopilotTests.PerStep_AllocatesNothing` ("over 12169 fight and 2751 break steps, Expected: 0, But was: 1"); the lead
  had copied the font TTFs into Assets during the run (an import on the main thread is the likely cause; the shared
  commit touched no sim code); re-run alone 21:21 `tests: 1 / 1 passed`.
- **Build 1** (21:27-21:34, 292 s): `Succeeded`, 0 errors, 495 warnings; `git diff --stat ProjectSettings`: only
  UnityConnectSettings (never staged). **The reel on build 1** (21:35-21:36, 80 s): 11 strips + 13 screens (the HUD in a
  fight SKIPPED: the reel picked the MODS card with AutopilotInput.TakeDraft, which refuses while the draft has the run
  paused; fixed to `g.PickMod(0)`). Captures: 18 + 1 (tile) + 24 = **43 of 150**.
- **Builders launched 21:16** (M stamina, X crunch, V hands, H HUD, N menus), each in a worktree from `8e5b416`, with a
  common brief (rules, dotnet only, tests first, the report format).
- **Slip:** one multi-line patch by `node -e` (moving the SETTINGS plate's line back into the constructor), against
  CLAUDE.md rule 9; written `.cjs` patch scripts from then on.

## U3-30.2 the identity (21:00-21:39)

- The fonts: Google Fonts' variable TTFs (OFL), static instances cut with fontTools 4.66 (Archivo wdth 62 wght 900
  italic, wdth 100 wght 600 / 800; Tourney wght 900 italic; Big Shoulders Display 900; Saira wdth 87.5 wght 600), each
  folder in `Assets/Fonts/<family>/` with its OFL.txt; baked by `ThemeFonts.MakeN17` (21:22): `NIGHT17 done: 6 / 6 font
  assets`, 1024 atlases, missing only U+25A1 / U+2713 (and Tourney's arrows).
- **The style tile** (`-skygrindTile`, build 1, `Logs/shots/n17-b1/n17-tile.png`): both directions with the same
  components. **Review (two lines):** A's italic display carries the slant on its own ("PAUSED", the gold "12450",
  "+150 ×3" read in one glance) and Tourney's "07" / "S" have a character nothing else in the game has; B (Big Shoulders
  + Saira) is upright and scoreboard-generic and fights the slant. Seen on the tile: INK at 94 % still let the floor
  through, so INK went to 96 %.
- `804aea6` the tokens (A) + IDENTITY.md + UiKit through UiTheme (Text's faces, Button as a solid INK-SOFT cut plate with
  a 4 px cyan bar and a DISPLAY 32 px label). Builders H / N / X / V / M told by SendMessage (21:40).

## Integration 1 (21:40-21:44)

| Builder | Commits (cherry-picked) | Their evidence | Lead's check |
| --- | --- | --- | --- |
| H (HUD) | `9cc4f3f` (tests first) `19c6070` `23fe3ee` `244831b` `d6d79e3` `82409f5` | Hud17Tests 17 red -> 17 green (dotnet); fingerprint EQUAL | Editor compile clean |
| X (crunch) | `6e5806d` (tests first) `2b0c3be` `8d22617` `b6e7aa0` `22a14cd` + the hit flash's fade | CrunchTests 0 / 11 -> 11 / 11 (dotnet); FxGatesTests 9 / 9; fingerprint EQUAL | Editor compile clean |
| V (hands) | `9831adf` (tests first) `269d6c0` | HandsTests 32 / 45 -> 44 / 45 (the one: the Editor-only allocation constraint, failing at the base too under dotnet) | Editor compile clean |

The .meta files of the new files committed by the lead (`1093b05`, then V's). **The suite on integration 1** (async,
21:43-21:54): `tests: 889 / 889 passed, 0 failed, 0 skipped (615.01 s)`, `replays: 22 / 22`, `golden tests: 96 / 96`;
SimHarness `fingerprint ... EQUAL to the baseline` (21:43). The new tests (Hud17Tests, CrunchTests, HandsTests' new
rules) were seen red by their builders under dotnet at their tests-first commits; the lead saw them green in the Editor
(this run), not red (the red runs were cherry-picked together with their implementations).

## Integration 2 (21:55-21:58): every builder's first pass in

| Builder | Commits | Their evidence |
| --- | --- | --- |
| H | `9bcdafe` the HUD matched to the tile (HP S75 over a white bar, points S56, the style meter laid out as the tile, "07" round hero) | Hud17Tests + FeelHudTests + SmallUi + MoveRow 44 / 44 (dotnet) |
| V | `fa79328` the viewmodel's plain muzzle quad removed (X's star is the one muzzle flash) | HandsTests 44 / 45 (the Editor-only constraint) |
| X | `4a9a58a` the crunch shake paired with V's per-gun springs (light guns 0.06 trauma; heavy guns pitch-only onset) | dotnet 0 errors |
| N | `50434d3` (tests first) `7b49b55` `6b01978` `5f05edc` `a7f0c21` `09d862d`: TITLE (PLAY, one run-setup card, PROGRESS / SETTINGS / QUIT), PAUSED (RESUME / RESTART / SETTINGS / QUIT, confirm on QUIT, a run header), SETTINGS (GAME / CONTROLS / VIDEO / ACCESSIBILITY + GAMEPAD with a pad + LAB in devMode; 8 rows then ADVANCED; sliders; a footer; HUD SCALE), RESULTS (the hero tally; RETRY (R) / TITLE / DETAILS), DOWN, WHAT GOT YOU, MODS, glyph + verb onboarding, the words cut, `91caa6b` folded in | MenusTests 0 / 9 -> 9 / 9 (dotnet); SmallUi2Tests 3 / 3; fingerprint EQUAL |
| M | `8fb3cbc` (tests first) `7df91bb` `cb70e24` (the lead's SurvivalSim lines, pre-placed in their own commit) `26d98e5` `ccc880d` (refill.rate 0.8 -> 0.5, measured) `964899b` (the fingerprint re-record, its own commit) | StaminaTests 0 / 21 -> 21 / 21; StaminaTapeTests 6 / 6 (not seen red); Core fixtures 618 / 619 (the one: ContractsTests' allocation counter under CoreCLR) |

**M's measurements (SimHarness spam, x a walk over 10 s on the plane), before (the web's pips) -> after:** dash mashed
1.40 -> 1.28; dash on each charge 1.35 -> 1.35; dash-jump chain 1.86 -> 2.17; sprint held - -> 1.09 (peak 10.5); boost
then sprint - -> 1.11; boost + sprint every 3 s - -> 1.26. The boost table: a full bar 30 steps / 6.00 m; 0.9 of the
cost 30 / 6.00; 0.75 -> 23 / 4.60; **0.5 -> 15 steps / 3.00 m = 0.50 x a full boost**; 0.25 -> 8 / 1.60; 0.2 -> no
boost. `spam --stamina-off` prints NIGHT16's table row for row. The refill retune: at 0.8 bar/s a boost came back every
~1.3 s (the web's pips: ~1.68 s) and the dash-jump chain reached 2.56 x a walk; at 0.5 the boost economy matches the
web's. verbs (DRYDOCK): identical before and after. A headless farm of 8 seeds, bar on vs off: identical (mean round
13.75); the bot never holds the key.

**The fingerprint re-record (`964899b`, its own commit):** design.stamina alone left the fingerprint EQUAL (the script
pressed the dash for one step and the bar was full at every 420-step dash); SimFingerprint now holds the key 1 s after
each press. Flags off: the old lines (600 `42c74be7ac21a5b3` ... 7200 `89091d1f7b7c2e4b`, round 4, kills 36, 5102
earned). Flags on (the new baseline): 600 `861ba038a88e3bae` ... 7200 `645ac080e7aa00cb` (round 3, 34 kills, 4200
earned; 17 boosts and 16 sprints).

**The lead's gates on integration 2 (21:58):** CoreHarness `golden total: 39 vector sets (48094 records), 22 replays (22
exact); 0 failing, 0 mismatches`; SimHarness `fingerprint seed 1: 12 hashes ... EQUAL to the baseline; run Fight round
3, kills 34, shots 80, earned 4200`; `twice` run 1 = run 2; `residual check: 7 of 7 EQUAL`; spam (the lead's run): the
plane's sprint held `1.09 ... 10.5 ... (e) ok: under the slide's 11.6`, boost-then-sprint `1.11 ... (e) ok`. Editor
compile clean. **The suite (21:58-22:07):** `tests: 925 / 925 passed, 0 failed, 0 skipped (594.35 s)`, `replays: 22 /
22`, `golden tests: 96 / 96`.

## Round 1 (build 2, 22:08-)

- **Build 2** (22:08-22:09, 28 s): `Succeeded`, 0 errors; ProjectSettings: only UnityConnectSettings. The code
  `7955220` (every builder's first pass + the reel fixes).
- **The reel** (22:09-22:10, 70 s): 12 strips (kill, headshot, punchkill, hit, horde, parry, boost, boostside, sprint,
  swapall, rankup, roundend) + 12 screens (title, 5 SETTINGS tabs: game / controls / video / access / pad, the HUD at rest,
  PAUSED, MODS, the HUD in a fight, DOWN, RESULTS) in `Logs/shots/n17-r1/`: **24 captures (67 of 150)**.

**Round 1's scores, the lead's:**

| # | Row | Score | Why (one line) |
| --- | --- | --- | --- |
| 1 | Kill crunch | 1 | A regression: the killed enemy vanishes on the kill frame and no block shows (punchkill: nothing where the husk stood through the 80 ms freeze; kill / horde / rankup the same); only the per-gun muzzle star is new. |
| 2 | Hit feedback | 3 | The hit flash reads (white at once, fading over ~8 frames at 7 m); the tilt doesn't read at that range; the hands kick. |
| 3 | Weapon feel | 3 | A muzzle star per gun in its family colour (cyan KESTREL, gold THUNDERCLAP), a visible kick; QUICKDRAW and the swaps not judged yet (the reviewer has the swap sheet). |
| 4 | Movement feel | 3 | The sprint reads apart from the boost (10.5 M/S in the plate, speed lines, the gun carried low and canted); the stamina bar under the crosshair is a ~20 px sliver, unreadable at a glance. |
| 5 | Score energy | 3 | Points as a big gold numeral with no label, merged pops ("+75 ×2"); the pops are small and the round's end wasn't judged. |
| 6 | Style meter | 3 | A big letter that slams white then settles cyan on a rank-up, the word, callouts stacked newest on top; it reads, but it isn't yet the screen's event. |
| 7 | HUD clarity | 3 | Labels and key hints gone at rest, except a 9-word sentence toast top-left ("A POWER-UP DROPPED: RUN THROUGH IT BEFORE IT FADES") and "[TAB] WHEEL" in the horde strip. |
| 8 | Menu simplicity | 4 | PAUSED 4 rows + a one-line run header; RESULTS 3 buttons; TITLE PLAY + one setup card + 3 rows. |
| 9 | Identity | 4 | One display face, one slant, cut plates and the cyan focus everywhere; the menus finally look like one game. But TITLE and RESULTS are clipped at the left edge ("KYGRIND", "N OVER", "EST D"). |
| 10 | Motion | 3 | The rank letter's slam and the hit flash's ease read in the strips; the menus' entrances aren't caught by a still. |
| | **Total** | **30 / 50** (round 0: 17) | |

- **22:12 to X (row 1, urgent):** the chunks invisible in the Player, with the evidence and the prime suspect
  (`RenderParams.worldBounds` left at its default: the instanced draw culled with the origin), a log line for the first
  three breaks, and the victim never gone before the blocks show.
- **Build 2's gates:** the smoke (22:10-22:13, idle 70 s before it) `[smoke] 180.9s DONE: all steps passed` (N's menus
  kept every path the smoke drives: the pad's menu steps included); `-skygrindGolden` `golden total: 39 vector sets
  (48094 records), 22 replays (22 exact); 0 failing, 0 mismatches`; `-skygrindFingerprint` `seed 1: 12 hashes over 7200
  steps ... EQUAL to the baseline` (step 7200 `645ac080e7aa00cb`: M's re-recorded baseline). THE FARM started 22:15 on
  build 2 (100 runs, normal, seeds 1..100; the diff against `Logs/farm-n16-b1/` below when it lands).
- **22:16 to N:** the left-edge clipping on TITLE / RESULTS / SETTINGS (everything inside the 56 px inset, at 16:9 and
  16:10), the surviving 9-word power-up toast, WHAT GOT YOU's killer plate over its heading, RESULTS' doubled NEW BEST and
  the lone grade plate over the bright world, TITLE's balance, the tally / SETTINGS entrances. **To H:** the stamina bar
  (a ~20 px sliver: make it 140-180 x 8-10 px, readable while it drains), "[TAB] WHEEL" at rest, a cut "BOU" chip, pops
  too small and on the crosshair, the rank-up as the screen's event (+ the 4-6 px shake).
- Not a defect: the reel's DOWN shows no killer because its death is synthetic (damage from a "husk" that doesn't exist);
  a real death records the killer for WHAT GOT YOU.

**Round 1's reviewer** (a fresh agent, the 24 captures + 4 before-shots and the checklist only, no code; 22:11-22:21):

| # | Row | Score | Its reason (trimmed) |
| --- | --- | --- | --- |
| 1 | Kill crunch | 2 | A freeze exists (headshot / punchkill / parry hold ~75-80 ms) but the victim is gone in frame 1 and nothing flies: the freeze holds an empty spot; the horde's survivors don't react. |
| 2 | Hit feedback | 3 | The flash eases back over ~80 ms and the gun kicks; no tilt or knockback; the muzzle star covers the target in frames 1-2. |
| 3 | Weapon feel | 3 | The revolver kicks hard and settles slowly; the shotgun pitches the camera; six silhouettes read apart; each swap shows empty hands for ~100 ms and takes 250-400 ms. |
| 4 | Movement feel | 2 | Boost and sprint look different; the stamina bar is an unreadable sliver; a strafe boost uses the forward burst. |
| 5 | Score energy | 2 | The counter flashes and rolls, pops merge, but pops are ~24 px and static, the round's bonus pops stack on each other, the round end is a white wash and "ROUND 2 IN 10" at ~14 px. |
| 6 | Style meter | 3 | The C slams; callouts stack but are small and overlap world labels; the decay bar is a near-empty thin line. |
| 7 | HUD clarity | 2 | HP is the biggest number; "LANDING" sits permanently top centre; key hints and a 9-word tip mid-fight; a cut "BOU" chip; enemies the walls' value in greyscale. |
| 8 | Menu simplicity | 3 | Item counts good everywhere (a big step from n16); settings duplicate rows, no visible focus, jargon; headers clipped. |
| 9 | Identity | 3 | Slanted cut plates, the italic display and the cyan logo are consistent and far stronger than n15 / n16, but the logo and every header are cut at the left edge. |
| 10 | Motion | 2 | Eased: the rank slam, the score flash, the hit marker; raw: enemies blink out, the fist arrives after the kill, swaps have dead frames, pops have no overshoot, flat white-outs. |
| | **Total** | **25 / 50** | |

Its 5 worst: (1) the victim vanishes and nothing flies, so the freeze is empty; (2) the punch's fist arrives after the
kill and the parry whites out the whole frame, HUD included, 60-70 % for 74 ms; (3) every screen header cut off at the
left edge; (4) the HUD's hierarchy upside down (HP louder than points; "LANDING" permanent; hints and a tip mid-fight;
the strips' "ROUND 1 IN 8947848", which is the strip tool's break-hold sentinel: fixed to a 90 s hold); (5) points and
the round end aren't moments. More: the stamina bar, swaps' empty frames, settings duplicates (AIM ASSIST, INVERT Y), the
GAME tab stuck lit, jargon, two slants; WHAT GOT YOU's tag over its heading; colour roles (the THUNDERCLAP / STITCHER's
gold plates, the shotgun's amber flash, gold mod letters, the lime power-up beam, a magenta edge in the sprint's
fringe). Its one change: every kill leaves a body for the freeze, broken into its own blocks on release, the gun or fist
at full extension on that frame.

**Round 1's fix lists (22:24-22:29), lowest rows first:** X: the chunks (already `9dd5513`: the blocks now RenderMesh on
the enemy's own material, the victim never gone before them, log lines; the muzzle star halved) + move the death burst
and the kill ring to a working path, the parry's white-out (<= 25 %, under the HUD, <= 2 frames, a cyan ring), the
round-end wash, the horde's survivors reacting, enemies brighter in value, no magenta in the sprint's fringe, a
directional strafe boost. V: the fist at full extension on the contact frame, swaps with no empty frame (120-150 ms), the
KESTREL's settle, hold poses clear of the weapon card. H: points the one hero number, pops 36-48 px merged into one
running total that flies into the counter (a 1.25 punch), "ROUND 1 CLEAR" as a moment, the zone name for 2 s only, an
INK plate behind the style column, the speed lines under the plates, a label for the round counter. N: the clipping,
the toast, WHAT GOT YOU, RESULTS, settings duplicates / the stuck tab / focus / jargon / one slant, MODS' letters, the
WHEEL prompt only in a quiet moment. The colour roles of guns and the power-up beam are the web's looks: noted for GG,
not changed (CLAUDE.md rule 7 and the guns' family colours).

**THE FARM on build 2 (U3-30.4's farm diff; 22:15-22:31, 951 s):** `farm 100 runs, normal, seeds 1..100, brain botBrain:
mean round 10.36, max 22, timeouts 12; causes: ACID 19, HUSK 39, SPITTER GLOB 6, SWARMER 24, timeout 12`. The diff against
NIGHT16's AFTER (`Logs/farm-n16-b1/`, `node Tools/farm-report.cjs diff`, `Logs/FARM-n17.md` + `farm-n17.html`,
gitignored): mean round 10.06 -> 10.36 (+0.30), median 7.5 -> 9.0, timeouts 15 -> 12; kills by gun shotgun 38 -> 40 %,
revolver 33 -> 28 %; deaths SPITTER GLOB 13 -> 6, HUSK 34 -> 39. An observation for GG, not a gate: the AUTOPILOT never
holds the boost key (no sprint), so this mostly measures the bar's boost economy (refill 0.5 bar/s, matched to the web's
pips by M) and the run-to-run chaos of a changed path.

**The lead's own U3-30.10 entries (22:17-22:27):** `601877f` the world answers the rank (RankWorld: the map's neon climbs
the cyan value ladder, up to 2.6 x as bright and pulsing 0.5 -> 2.2 Hz with the rank; REDUCED MOTION: no pulse);
`639b59e` power-ups pulled in over the last metre and a flash on collection (PowerUpView, view only); PORT-STATUS.md's
event list. `381044d` the strips' break hold (the reviewer's "ROUND 1 IN 8947848").

## Integration 3 (22:24-22:32): round 1's fixes

| Builder | Commits | Their evidence |
| --- | --- | --- |
| X | `d4ef3d0` the blocks on `RenderMesh` + the enemy's own material (instanced SKYGRIND/Particle draws nothing in this Player: the death burst and the kill ring were invisible too), log lines, the muzzle star halved; `8c7b48c` the death burst and the kill ring off instancing; `80a4e37` the impact frame 25 % / 2 frames / under the HUD (the round-end white wash was it at 55 %), a perfect parry's cyan ring, parry knockback, heavy hits push survivors back, enemies x1.35 value and rim x1.5, the sprint's chroma 0 (it clamped to full aberration: the magenta) | CrunchTests 11 / 11 (dotnet) |
| H | `312f94f` (tests first) `0119b36` (the stamina bar 160 x 9 px with its floor notch, whole-word mod chips, pops sized up per merge, the rank slam 3x with a 5 px HUD shake); `cab3ba6` (tests first) `6dc47cd` (points the one hero number S75, HP S42 over a quartered bar, one running pop per burst flying into the counter with a 1.25 punch, "ROUND N CLEAR" slam + the bonus, the zone name 2 s only, the style column on an INK plate, the speed lines under the plates, "ROUND" over the counter) | Hud17Tests 3 red -> 20 green, then 6 red -> 21 green (dotnet); 50 / 50 with FeelHud / SmallUi / MoveRow / ModChip |
| N | `d88b30e` (tests first) `e6fe8a2` `da85fe7`: every menu text on one 12 deg column inside the inset at 16:9 and 16:10 (MenuRules.Left), the tips at 3 words with glyphs, hints over 3 words dropped in play, cleared on DOWN / RESULTS, WHAT GOT YOU's plate under its heading, RESULTS' tally on one INK slab with the grade in its colour, NEW BEST once, TITLE's slab wider + a 0.32 INK sheet, entrances for the tally / SETTINGS' rows / DOWN's word, per-device settings rows (MOUSE / PAD), the first row lit on a tab, MOUSE SPEED as a plain number, the full 12 deg row slant, MODS' tag words, prompts quiet within 10 m and only in a break for WHEEL / LAST GUN | MenusTests 9 / 12 red -> 12 / 12 (dotnet) |

Editor compile clean (22:32). X's report: the parry's full-frame white is UiFeel.cs's parry flash and the strafe boost's
centred burst is UiFeel's speed lines (both H's files): relayed to H with X's exact lines. **The suite on integration 3**
(22:33-22:40): `tests: 932 / 932 passed, 0 failed, 0 skipped (395.71 s)`, `replays: 22 / 22`, `golden tests: 96 / 96`;
fingerprint EQUAL.

## Integration 4 (22:40-22:48): the beyond entries and the rest of round 1

| Builder | Commits | Their evidence |
| --- | --- | --- |
| X | `fd58c3b` FxCubes / FxLines / shells off instancing (CubeBatch: one mesh per colour, RenderMesh); `585e187` (U3-30.10) an enemy's arrival: a DANGER ground ring + a 2-frame core where the spawn beam lands; `7026432` BlobShadows off instancing (BlobShadows.cs given to X) | dotnet 0 errors; CrunchTests 11 / 11 |
| N | `42f23cf` (tests first) `5922e5c` (U3-30.10) the RESULTS card (the tally row in one slanted INK card with an 8 px cyan bar, the wordmark, THE ROUTE's minimap, the best moment, the date and the seed; inside the inset at 16:9 and 16:10); `c6e55bc` CONTROLS / CONTRACTS / WEAPONS onto the new row style | MenusTests 12 / 14 red -> 14 / 14 |
| H | `75610a6` UiFeel: the parry flash 0.08 s at <= 25 % under the plates, none on a PERFECT parry; the speed lines lean toward the travel; `fc96d6f` (U3-30.10 a) the rank-up streak along the slant; `e10950c` (tests first) `d0dd229` (U3-30.10 b) DOUBLE / TRIPLE / MASSACRE as named lines; `51a3eab` (U3-30.10 c) the round-end tally ("14 KILLS · BEST +480") | Hud17Tests 24 / 24; 53 / 53 with the HUD fixtures; fingerprint EQUAL |
| V | `2845cd2` (tests first) `d754bc7`: the fist at full extension on the contact frame (the punch event's step = the kill = the freeze's first frame) and a fist that reads as a fist; swaps drawing the target gun from the press (the old one flicked out in 55 ms, no empty frame); the KESTREL settling in 0.2 s; the GAUNTLET's hold raised off the weapon card (D-69, Unity-side); `a168208` (tests first) `c549c45` the slide pose (low, canted into the lean, a skid tremor by speed, the free hand bracing) | HandsTests 39 / 49 red -> green; 47 / 50 -> 50 / 50 |

**Gates:** the suite on integration 4 (22:41-22:47) `tests: 940 / 940 passed, 0 failed, 0 skipped (358.89 s)`, `replays:
22 / 22`, `golden tests: 96 / 96`; CoreHarness `0 failing, 0 mismatches` (22 exact); fingerprint EQUAL; after V's slide
pose `tests HandsTests`: `50 / 50 passed` (22:48). `950db40` the slide strip joins the reel.

## Round 2 (build 3, 22:48-)

- **Build 3** (22:49, 17 s): `Succeeded`, 0 errors; ProjectSettings: only UnityConnectSettings.
- **The reel** (22:49-22:50, 70 s): 13 strips + 12 screens in `Logs/shots/n17-r2/`: **25 captures (92 of 150)**. The
  Player log: `[crunch] break #1: husk, blocks 17, pooled 17, cap 96, draw RenderMesh SKYGRIND/Enemy` and `[crunch] frame
  191: drew 17 blocks ... 7.0 m from the camera, frozen yes` (the blocks draw now, on the freeze's frame).
- **The smoke** (22:51-22:54, idle 265 s before): `[smoke] 176.7s DONE: all steps passed`.
- **The reviewer** launched 22:51 (a fresh agent; the captures only).

**Round 2's scores, the lead's:**

| # | Row | Score | Why (one line) |
| --- | --- | --- | --- |
| 1 | Kill crunch | 3 | The blocks exist: a real fight's kill bursts into orange blocks (hud-fight); the punch kill holds a wall of white blocks with the fist out on frame 1. But at 7 m the blocks barely leave the body's silhouette in 200 ms, the first kill of the process hitches 122 ms (frame 1 +13 ms -> frame 2 +136 ms with one sim step), and the headshot freeze still washes grey. |
| 2 | Hit feedback | 3 | The flash eases and the hit enemy pops back; at 7 m still small. |
| 3 | Weapon feel | 3 | The fist now lands on the contact frame; per-gun kicks; swaps judged by the reviewer. |
| 4 | Movement feel | 3 | The sprint, the boost and now the slide each have a pose; the stamina bar is a real bar under the crosshair. |
| 5 | Score energy | 4 | "ROUND 1 CLEAR" slams with "9 KILLS · BEST +122" and the counter rolls in big gold; one running pop per burst; but the bonus pop sits on the tally line. |
| 6 | Style meter | 4 | The letter, the word, the decay bar and DOUBLE / named callouts on a solid INK column; the rank-up streak cuts across the crosshair (a defect). |
| 7 | HUD clarity | 4 | Points the hero, HP secondary over a quartered bar, ROUND labelled, whole-word chips, a 3-word toast; "LANDING" on each strip's zone entry. |
| 8 | Menu simplicity | 4 | TITLE / PAUSED / RESULTS / SETTINGS as round 1, the clipping gone, per-device rows. |
| 9 | Identity | 4 | TITLE whole and balanced; RESULTS a card worth posting (the wordmark, the route, the seed). |
| 10 | Motion | 3 | Slams and rolls everywhere on the HUD; frame-1 white-outs on the parry and the round end still flatten two moments. |
| | **Total** | **35 / 50** (round 1: 30) | |

- **22:53 to X:** the first-kill hitch (warm every kill-path material / variant up at load; the log lines off the kill
  frame), the blocks thrown with a spread cone and an upward kick, the frame-1 white-outs (the impact frame <= 15 %, none
  on a round clear's last kill, only the strongest overlay per frame). **To H:** the rank-up streak confined behind the
  style column (alpha <= 0.35, never over the crosshair), the round end's three overlapping lines spaced, "LANDING"'s fade.
- **The lead's own:** `1214b0f` the headshot kill's flash 0.18 / 0.1 s -> 0.08 / 0.035 s (the grey wash); `1650d62` the
  reel's kill strips 16 frames at gap 2 (past the freeze); `20fc0ca` spawn and pickup strips (U3-30.10's evidence);
  `4820656` ImpactFrames' flash canvas built at load (a first-kill hitch suspect, X's trace).
- U3-30.4's boost and sprint strips (round 2), reviewed by the lead: the sprint reads 10.5 M/S in the plate with the gun
  carried low and the bar under the crosshair draining; the strafe boost's speed lines now stream from the side it moves
  toward (24.0 M/S). `spam --stamina-off` (22:57) prints the arrival's rows exactly (walk 7.11 s, dash mashed 4.61 s /
  1.54 x, dash on each pip 1.39 x, dash-jump 2.51 x): with the flags off the Mover is NIGHT16's.
- **Integration 5 (22:56-23:00):** H `2e7c930` (the streak confined behind the column, the round end's lines in one
  slot); X `cf27b58` (the kill path warmed at load: materials and batches made up front, one sub-pixel draw of each path on
  TITLE's first frame, the [crunch] text written 30 frames later), `358a50a` (the blocks thrown in a 60 deg cone, out and
  up, 2-4 m/s up, more spin; the punch's throw up and across), `c6f616a` (the impact frame <= 15 %, never on a round
  clear's last kill or in a break, yielding to the headshot / parry flash). The suite (23:01-23:10): `tests: 942 / 942
  passed, 0 failed, 0 skipped (504.16 s)`, `replays: 22 / 22`, `golden tests: 96 / 96`.

**Round 2's reviewer** (a fresh agent, the 25 captures + 4 before-shots and the checklist only; 22:51-23:10):

| # | Row | Score | Its reason (trimmed) |
| --- | --- | --- | --- |
| 1 | Kill crunch | 2 | The freezes exist but hold a pale haze (headshot 1-8, parry 1-3, roundend 1-3); the body crumbles where it stands, nothing flies. |
| 2 | Hit feedback | 2 | The capsule flashes and fades over 90 ms; no tilt or knockback seen; "+13" ~20 px. |
| 3 | Weapon feel | 2 | Six distinct silhouettes and a real revolver flip, but the flip peaks late and holds at 217 ms; every swap blanks 1-2 frames; the 6-tab card covers four guns. |
| 4 | Movement feel | 2 | Boost and strafe boost look alike; the sprint's shafts read as sunlight; the slide is only a camera drop; the bar is a 6 px white line. |
| 5 | Score energy | 2 | The bank reads "500" through every kill strip (points land only when the pop arrives); pops white on white or gold on orange; roundend's roll + punch is the one real moment. |
| 6 | Style meter | 3 | The letter slams, callouts stack; the decay line ~3 px; DOUBLE printed twice; the plate translucent in the play space. |
| 7 | HUD clarity | 2 | ~16 text items in a fight; hints mid-fight; "LANDING", "0.0 M/S", "ROUND 01", FRESH / USED / STALE everywhere; the style plate over an enemy. |
| 8 | Menu simplicity | 3 | PAUSED 4, RESULTS 3 (n16: 7 and 6); SETTINGS 5 tabs / ~36 rows with overlapping rows; TITLE's selectors, a lock line, a row of dashes. |
| 9 | Identity | 3 | TITLE / PAUSED / RESULTS share the face, the logo, slanted plates and a diagonal wipe, a clear step up; the 3D frame carries none of it; MODS adds a third family. |
| 10 | Motion | 2 | Good: the rank slam, the round banner, the bank roll; bad: speed lines at full density on frame 1, the haze, the plate's alpha fade, linear hint fades, swap blanks, debris that barely moves. |
| | **Total** | **23 / 50** | |

Its 5 worst: (1) kills read as fog, then fizzle (the fix direction: a dark value-crush impact frame, the victim white
1-2 frames only, chunks travelling metres); (2) points don't land at the kill (the bank frozen until the pop flies in;
pops on the body, unstroked, same size); (3) the weapon card eats the viewmodels, swaps blank; (4) a flat translucent cyan
plank across the screen (the rank-up streak: fixed in `2e7c930`, after this build); (5) HUD clutter and the style plate in
the play space. Its one change: every kill one event (a 30-60 ms crush frame, the pop at 2x with a dark stroke, the bank
ticking on that frame), then the body bursting along the shot. Note: by this build the headshot's wash (`1214b0f`), the
impact frame (`c6f616a`), the throw (`358a50a`), the streak (`2e7c930`) and the first-kill hitch (`cf27b58`, `4820656`)
were already being fixed; the next reel checks them.

**Round 2's fix lists (23:11-23:14):** X: the crush frame (a value crush via PostFx, the victim's HDR blocks staying
white-hot), the victim white 1-2 frames only, throw distance, a slide cue of its own. H: the bank ticks on the kill frame,
pops with a dark stroke up-right of the hit sized 36 / 56 / 80 and merged within 500 ms, the weapon readout a <= 360 x 64
strip of slot glyphs (the name only on a swap, near the crosshair; nothing over the viewmodel), the style meter up in the
top third with no plate, the speed readout hidden at rest, the round number in the top line, freshness as a pip, the mod
chip off gold. N: prompts never in a fight (first 2-3 times only, quiet 15 m / 3 s), SETTINGS on an INK slab with aligned
labels, one control per kind, PLAY lit by default, the empty bests row hidden, MODS' cards on INK, RESULTS' and DOWN's
wording and contrast. V: swaps overlapping 30-40 ms (no blank frame), the KESTREL's flip down by ~120 ms, the slide's
brace clear of the HP card.

## Integration 6 (23:16-23:35): round 2's fixes

| Builder | Commits | Their evidence |
| --- | --- | --- |
| V | `d912103` (tests first) `1f6d937`: swaps overlap ~30-35 ms (the old gun on a second root, the new one from ~14 ms), no empty frame at 144 fps; each gun's kick on real time (the KESTREL under 15 % of its peak by 120 ms, freeze or not); the slide's brace above the HP card | HandsTests 46 / 50 red -> green |
| X | `6264eb9` (tests first) `2f97c8e` the CRUSH frame (post exposure -2 EV for 25 ms, eased back over 35 ms, real time; the blocks' HDR white up by as much) instead of any white; `1e577c3` blocks white 2 frames then their own colour with a hot rim; `5781b63` hero blocks thrown 3-5 m, 360-720 deg/s, the head at 12-14 m/s, a punch's body 4-6 m; `e7349a3` the slide's own cue (a 2.5 deg widen, low streaks, sparks ahead) | CrunchTests 11 / 13 red -> 13 / 13 |
| H | `6879f73` the bank ticks on the gain's frame (the pop's flight decoration only); `94dbb96` (tests first) `07f8662` pops with a 4-5 px INK stroke, 90 px up-right of the crosshair, 32 / 56 / 75 px by what pays, 1.8x in, merged within 500 ms, the CLEAR in its own lane; `e176d42` the weapon readout a <= 360 x 64 strip of slot silhouettes (the name on a swap only, under the crosshair), freshness as a pip; `ae24dc4` the style meter in the top third with no plate (a dark stroke), at most 3 callouts, DOUBLE once; `25b874c` the speed readout above a walk only, the round number in the top line, the mod chip off gold | Hud17Tests 5 red -> 24 / 24; 53 / 53 with the HUD fixtures; fingerprint EQUAL |
| N | `821eeb2` (tests first) `d75ba97` prompts only in quiet (no enemy within 15 m, 3 s since a shot / swap / hit, not sprinting or sliding, at most 3 shows); `1546bdb` SETTINGS on one opaque INK slab, labels and values aligned, chips for on/off, drawn chevrons, one number format, RUMBLE absorbing VIBRATION; `8fecc88` PLAY lit by default, the best row only with grades, opaque slabs; `f5bf55d` MODS opaque, no tag word; `0b583f5` RESULTS "BEST BURST +33", a 280 px map, DOWN's sheet 0.62, WHAT GOT YOU on one INK block | MenusTests 11 / 16 red -> 16 / 16; fingerprint EQUAL |

The lead's: `7062ab5` no white flash on a headshot kill (the crush owns it). **Gates:** Editor compile clean; the suite
(23:27-23:35) `tests: 946 / 946 passed, 0 failed, 0 skipped (488.56 s)`, `replays: 22 / 22`, `golden tests: 96 / 96`;
CoreHarness `0 failing, 0 mismatches` (22 exact); SimHarness fingerprint EQUAL.

## Round 3 (build 4, 23:35-)

- **Build 4** (23:36, 22 s): `Succeeded`, 0 errors; ProjectSettings: only UnityConnectSettings.
- **The reel** (23:36-23:37, 68 s; `-skygrindReelOnly`: 14 strips + 8 screens): **22 captures (114 of 150)** in
  `Logs/shots/n17-r3/`.
- **The smoke:** the first run (23:37-23:41, idle 175 s) `DONE: 3 step(s) failed`: `VIGOR bought: owned False, points 1600
  -> 1600`, `Mystery Box rolled ... paid 0`, `STITCHER (bought 1600 -> 1600)` (every hold-to-buy from 18.8 s to 49.9 s;
  the COMET and LONGWAVE buys after it passed). Core is unchanged since build 3 (`git diff --stat` on Assets/Scripts/Core:
  nothing), and the buy needs only a candidate in the cone and the hold counted over sim steps; the run overlapped a
  builder's dotnet build and the reviewer's image work. **The re-run** (23:42-23:45, idle 473 s): `[smoke] 180.9s DONE: all
  steps passed`. Logged as a load flake (the buy latch releases only when a sim step sees E up inside the smoke's 0.15 s
  gap; a starved frame can miss it), not the build's.
- **The reviewer** launched 23:38.
- The lead found in the strips: the crush frame never shows in a strip (PostFx refused it in a break; every strip holds
  the first break): fixed by X `84e7534` (only the round-clear frame and its slow-mo excluded; the punch victim's white 1
  frame + 30 ms); the pickup strip showed nothing (the strip's trigger sat at the grab radius, 1.6 m, and the pull aimed at
  the chest, under the camera): fixed `9b0afa7` (a catch point in front of the camera; the trigger 1 m outside the grab);
  the round-start hero "01" covers the crosshair and the arriving enemy for its 1.5 s (spawn strip): to H.

**Round 3's scores, the lead's:**

| # | Row | Score | Why (one line) |
| --- | --- | --- | --- |
| 1 | Kill crunch | 4 | The punch kill is an event: white blocks through the freeze, hot orange, then a burst across the whole screen; a 7 m body kill throws blocks wide and high; the crush frame not yet seen (fixed after this build). |
| 2 | Hit feedback | 3 | The flash and the push read up close; at 7 m still small. |
| 3 | Weapon feel | 3 | Per-gun kicks on real time, the fist on the contact frame; the swap overlap not yet judged by eye. |
| 4 | Movement feel | 4 | Boost (radial lines from the travel side), sprint (shafts, the carry, a draining bar), slide (its own widen, streaks, sparks) each have a cue. |
| 5 | Score energy | 4 | The bank rolls and punches on the kill frame; stroked pops sized by what pays; "ROUND 1 CLEAR" with its bonus in its own lane. |
| 6 | Style meter | 4 | Up in the top third with no plate, the letter big with a stroke, DOUBLE once, 3 callouts. |
| 7 | HUD clarity | 4 | "ROUND 1 · 3 LEFT" on top, a compact weapon strip, no speed at rest; the round-start numeral over the crosshair is the one blocker. |
| 8 | Menu simplicity | 4 | SETTINGS on one slab with 8 aligned rows, chips and chevrons, a short footer. |
| 9 | Identity | 4 | One face, one slant, solid ink, the cyan ladder everywhere. |
| 10 | Motion | 3 | Slams, rolls and the blocks' throw move well; stills can't show the menus' entrances. |
| | **Total** | **37 / 50** (round 2: 35) | |

**Round 3's reviewer** (a fresh agent, the 22 captures + 4 before-shots, the checklist only; 23:38-23:48):

| # | Row | Score | Its reason (trimmed) |
| --- | --- | --- | --- |
| 1 | Kill crunch | 3 | The headshot (a held white body, then large cubes) and the punch kill (cubes thrown at the camera) read as events; the body kill doesn't (gone at f1, one frame held); the horde holds less than one headshot. |
| 2 | Hit feedback | 3 | White ~45 ms easing back over ~100 ms, the gun kicks ~15 deg; the capsule never moves or tilts. |
| 3 | Weapon feel | 3 | The kick and the star are there; swaps snap in ~60-100 ms with distinct silhouettes; every gun dark violet, small, same corner. |
| 4 | Movement feel | 2 | The boost reads (full-screen lines, a roll); the sprint shows nothing in 262 ms; the slide a small dip; the bar a ~5 px sliver. |
| 5 | Score energy | 3 | The counter rolls and flashes; pops overprint (rankup f3-9); the score disappears gold-on-orange; the round bonus lands after the roll. |
| 6 | Style meter | 2 | The slam is white ~30 ms and at rest by ~70 ms; D fades in with no plate and the world's cyan trim runs through it; callouts repeat themselves. |
| 7 | HUD clarity | 3 | At rest it passes (4 clusters, no hints); the world's cyan trim through the crosshair and the meter; the score has no plate; "+111" fuses with the world sign. |
| 8 | Menu simplicity | 4 | A big cut from n15 / n16: TITLE 4 items + a 3-row card with PLAY lit, PAUSED 4, SETTINGS 7 rows + ADVANCED, RESULTS 3. |
| 9 | Identity | 2 | Consistent heavy italic on slanted ink with a cyan focus, but "a stock esports menu kit"; TITLE's half a flat close-up of floor; the world's untextured boxes; hues leaking. |
| 10 | Motion | 2 | Swaps eased; pops and the C slam snap in 1-3 frames; the rank fades in; callouts and ROUND 1 CLEAR pop in; the spawn a slow fade; the pickup no motion. |
| | **Total** | **27 / 50** (round 1's reviewer 25, round 2's 23) | |

Its 5 worst: (1) the body kill, the most common, is the weakest event; (2) the style meter barely exists; (3) points
unreadable at the moments that pay most; (4) the big moments (round clear, a pickup) are whispers; (5) nothing in a frame
says SKYGRIND and colours leak (the world's cyan trims and sky cubes, the lime power-up beam, the THUNDERCLAP / STITCHER's
gold, the COMET's lavender). Its one change: every kill a three-beat event (a >= 50 ms freeze growing with the count and a
white silhouette; big chunks thrown; one merged, stroked gold pop flying into a plated score; the rank letter slamming
for >= 100 ms).

**Round 3's fix lists (23:50):** X: one kill grammar (a white silhouette held at f1 through the freeze, bigger chunks
8-12 m/s, a floor ring, shotgun knockback before the burst), a bigger sprint FOV + edge lines, the slide's camera drop and
roll from f1, a bigger parry ring. H: the style meter on an opaque INK plate up top (the 2.2x slam held 100 ms, named
callouts only, an 8 px decay bar), the score on a plate, pops merging within 300 ms, world signs in the BODY face, the
round end on a full-width INK band, the pickup as a banner that becomes a chip with a ring timer, the top line hidden
under menus. N: a wordmark from the game's shape language, MODS with a glyph per mod and the number as the hero, RESULTS'
grade on a plate, SETTINGS' value face / tab gaps / footer wording, drawn key chips instead of ASCII brackets. V: the
sprint carry 20 deg lower, the guns a little bigger and their accents brighter, and (U3-30.10) the style aura on the held
gun on the cyan ladder (D-70's recommendation). The world's cyan trims, the lime beam and the guns' gold are the web's
looks (GG's: noted for the report, not changed).
**The lead's (23:48-23:55):** `963a2fb` design.impact.freeze.kill 0.04 -> 0.05 (`_why` updated, the Player copy identical;
fingerprint EQUAL; `impact PASS: the budget holds on 2 map(s)`), `6b37a42` ImpactRulesTests' sim-request expectation as the
longer of the two (24 / 25 -> 25 / 25: the test had encoded the two configs being equal), `9475a01` TITLE's backdrop
toward the horizon; H's `a8ca0c8` (the hero numeral 250 px above the crosshair).

## Integration 7 (23:57-00:11) and a usage-limit stop (00:11-01:51)

| Builder | Commits | Their evidence |
| --- | --- | --- |
| X | `5544a91` one kill grammar (the body held white at 1.1 x through the freeze; 9 bigger blocks + 3 heroes, 8-12 m/s; a floor ring; the THUNDERCLAP's shove-back before the burst; a ray along the throw bounces blocks off a wall within 6 m); `678327f` the sprint's 7 deg widen + edge-only lines; `60ff2bf` the slide's entry dip and lean, streaks and sparks from frame 1; `48181d1` the perfect parry's ring twice the body, a reflected glob cyan with a trail; `00316e1` (tests first) `1b36e88` the trail pre-warmed; `28a777a` (U3-30.10) a death signature per gun (KESTREL punch-through, THUNDERCLAP shove + 100 deg burst, STITCHER chips pinned on the nail line, COMET radial blast + a bigger ring, LONGWAVE split along the beam with a white seam) | CrunchTests 13 / 15 red -> 15 / 15 |
| V | `f55a3ad` `bea6cd2` the sprint carry 33 deg down with a bigger bob; `27b67cf` `7de5fd6` the guns 7 % bigger (COMET 5 %), brighter stripes and gem; `bce235d` `5c0b31d` (U3-30.10, D-70) the style aura on the held gun (a back-face rim on the cyan ladder, CYAN-HOT + a pulse at the top; REDUCED MOTION: no pulse); `a9cb89a` `bc7d39c` (U3-30.10) the kill flourish (on a multikill or a rank-up, after a 0.35 s pause: the KESTREL's spin + twirl, the THUNDERCLAP's rack, ...; yields to any shot / swap / punch) | HandsTests red -> 53 / 54 (the Editor-only constraint) |
| H | `8dfc0f8` the HUD hidden under PAUSED / menus; `030ac6e` `adfe054` the style meter on an opaque INK plate up top, a 2.2 x slam held 100 ms, named callouts only, an 8 px decay bar; `9d69f60` `58541f6` the score on an INK plate, pops merging within 300 ms, world signs in BODY-BOLD; `d6bd206` `2c69225` the round end on a full-width INK band, the CLEAR at 133 px, the bonus flying into the score; `88994cb` the pickup: a gold edge flash, a 75 px banner, a chip with a ring timer | Hud17Tests red -> 28 / 28; 57 / 57 with the HUD fixtures |
| N | `e9c558a` (tests first) `3818039` the wordmark on a cut slab with a cyan accent bar; `51d7c56` MODS: a 96 px glyph per tag, the number as the hero, card 1 focused; `4dc1903` RESULTS' grade on its own 150 px plate, even gaps, a drawn R chip; `b4e470e` SETTINGS: one value face, even tabs, footers that say what a row does; `9087f98` drawn key chips on DOWN / WHAT GOT YOU / prompts, the HEAT lock line only with HEAT focused | MenusTests 16 / 18 red -> 18 / 18 |

Editor compile clean (00:11). **At 00:11 the lead's session hit a usage limit and stopped until 01:51** (the builders had
all reported and were idle; nothing ran). Gates on integration 7 (01:52-02:02): SimHarness `fingerprint ... EQUAL`,
CoreHarness `0 failing, 0 mismatches` (22 exact), `residual check: 7 of 7 EQUAL`; the suite (async, `Logs/n17-suite-int7.json`)
`total 958, passed 958, failed 0, skipped 0` (659.79 s).

## Round 4 (build 5, 02:03-)

- **Build 5** (02:03-02:04, 28 s; code `3f6a9ed`): `Succeeded`, 0 errors; ProjectSettings: only UnityConnectSettings.
- **The reel** (`-skygrindReelOnly`, 14 strips + 4 screens): **18 captures (132 of 150)** in `Logs/shots/n17-r4/`.
- **The smoke, two runs, neither all green, every step green in one of them:**
  - Run 1 (02:05:53-02:09:24, idle 142 s; the reviewer agent working) `DONE: 3 step(s) failed`: `ARMORY bought: open
    False, points 1297 -> 1297`, `VIGOR bought: owned False`, `virtual pad menus: ... right moved a value False`. Its round
    1 already differed (`cleared in 12 s: 9 kills, 8 headshots, points 1297`; the deterministic value, seen on b4's re-run
    and this re-run, is `8 s ... 9 headshots, points 1576`): the smoke's aim runs per rendered frame, so the run was
    perturbed before any failing step.
  - Run 2 (02:12:56-02:17:13, idle 179 s; the reviewer finishing) `DONE: 2 step(s) failed`: `the exfil radio at the spawn
    called in round 11's break (could True): Off` and `EXFIL at round 1 ...: Paused, ''`; the buys, the pad menus and every
    other step passed, round 1 `8 s, 9 headshots, points 1576` (b4's re-run's numbers exactly).
  - The cause read: the second run's EXFIL step logs the flow as **Paused**. SurvivalGame.cs:279 pauses a playing run when
    the cursor lock drops (Esc, or another window taking the foreground). A pause stops the buy (the sim is not `Active`)
    and the exfil; both failing runs (and b4's first) overlapped an agent's tool work, and each run failed different steps
    at different times; build 5's code since build 4 touches no input, flow, buy, sim or smoke file (`git log
    d1b9668..HEAD` on Input/, SurvivalSim.Run.cs, SurvivalGame.cs, SliceSmoke*.cs, Core: nothing). Logged as the
    environment's, not the build's. **The close's smoke runs with every agent idle.**
- **The reviewer** (the fourth and last of 4; launched 02:06, reported 02:17).

**Round 4's scores, the lead's** (stricter than round 3's 37: the 10 px speed readout breaks the 18 px rule and the pops
overprint; both were in round 3's captures and the lead missed them then):

| # | Row | Score | Why (one line) |
| --- | --- | --- | --- |
| 1 | Kill crunch | 4 | The punch kill and the headshot are events (the white body held, the blocks hot orange); a body kill's first frame still looks like a hit and its blocks cluster near the body. |
| 2 | Hit feedback | 3 | The flash and the kick read; the capsule doesn't move. |
| 3 | Weapon feel | 3 | Distinct swaps and kicks; the swap's cyan glint floats off the gun like a clipping bug. |
| 4 | Movement feel | 3 | The slide drops and rolls; the sprint strip shows only the lowered carry (no widen, no lines visible); the stamina bar a sliver. |
| 5 | Score energy | 3 | The bank on its plate rolls; "+120" and "+125" overprint (rankup f3-6), gold vanishes on orange. |
| 6 | Style meter | 4 | On its plate up top, the C slams big; "+ HEADSHOT" listed twice. |
| 7 | HUD clarity | 3 | The rest HUD is clean; "10.5 M/S" is about 10 px (the rule is 18); "+111" sits on the world sign. |
| 8 | Menu simplicity | 4 | TITLE 4 + a 3-row card, PAUSED 4, RESULTS 3 numbers and 3 buttons, SETTINGS 7 rows. |
| 9 | Identity | 4 | The wordmark slab, one face and slant, ink and the cyan ladder on every screen; TITLE's right half floor. |
| 10 | Motion | 3 | The slams and the throw move; ROUND 1 CLEAR and DOUBLE POINTS pop in full size. |
| | **Total** | **34 / 50** (round 3: 37) | |

**Round 4's reviewer** (a fresh agent, the 18 captures + the before-shots, the checklist only; 02:06-02:17):

| # | Row | Score | Its reason (trimmed) |
| --- | --- | --- | --- |
| 1 | Kill crunch | 2 | Only the punch kill lands (~90 ms, chunks at the camera); a revolver kill's frame 1 is a hit's frame 1; chunks swell in place; the horde freezes less than one headshot. |
| 2 | Hit feedback | 3 | White and a 15-20 deg kick; no tilt or knockback; the white eases back over ~100 ms. |
| 3 | Weapon feel | 3 | Kicks back by ~45 ms, swaps 50-80 ms with six silhouettes; the "ready" glint floats off the gun. |
| 4 | Movement feel | 2 | 256 ms of sprint: only the gun lowers; the stamina bar a ~75 x 4 px sliver; the slide a drop and roll, the arm the floor's colour. |
| 5 | Score energy | 2 | The counter rolls but doesn't punch; pops shrink, overlap ("+1205"), vanish on orange; the round end a static banner over the kill. |
| 6 | Style meter | 3 | A real slam (white to cyan by ~150 ms), stacked callouts, a decay bar; the letter a grey ghost for 100+ ms; "+ HEADSHOT" twice; the bar ~3 px. |
| 7 | HUD clarity | 3 | No hints at rest, HP and score read; "POWER-UP · GRAB IT" on a half-empty plate, "BOUNTY HUNTER" not actionable, "+111" melts into the sign. |
| 8 | Menu simplicity | 4 | TITLE 9 -> 4, PAUSED 7 -> 4, RESULTS a wall -> 3 numbers, SETTINGS 7 rows; jargon left (HEAT, BEST D, WHAT GOT YOU), broken mod copy. |
| 9 | Identity | 3 | "A coherent system now" (slash, slanted plates, condensed italic, ink + cyan on every screen) but a generic shooter-menu look; no dusk in the menus; TITLE's right half floor. |
| 10 | Motion | 2 | The slam and the swap ease; pops jump big-white to small-gold in a frame; ROUND 1 CLEAR jumps position on frame 2; DOUBLE POINTS pops in; the style plate fades over ~230 ms. |
| | **Total** | **27 / 50** (round 1 25, round 2 23, round 3 27) | |

Its 5 worst: (1) gun kills read as hits, a headshot as a body kill; (2) the horde's freeze is shorter than one headshot's
and the THUNDERCLAP's barrel hides it; (3) pops shrink and collide, the round end isn't a moment; (4) movement has no
feel and stamina can't be read; (5) focus and copy bugs on RESULTS (two lit buttons, RETRY dark), MODS (lines cut
mid-thought, SCAVENGER in points gold), SETTINGS (the focused row weaker than the ON pills). Its one change: every kill
the loudest thing on screen, scaled by value (a kill-only frame 1, 70 / 110 ms freezes + 25 ms per extra kill, chunks
thrown along the shot at 10-16 m/s, one merged overshooting pop flying into a counter that punches).

**Round 5's fix lists (02:22; the last round: build 6 is the close's, so small and safe, nothing in the smoke path,
last commits by 03:50):** X: a kill-only frame 1 (white-hot 1.15 x for 2 frames + a radial burst), chunks thrown along
the shot (10-16 m/s, 2-3 body widths out by 200 ms), the headshot's head pop + ring, the freeze 50 ms + 25 ms per extra
kill (cap 160 ms; design.impact, X's this round), the sprint widen and edge lines made visible, the slide's sparks, the
reflected glob cyan. H: pops 0.7 -> 1.3 -> 1.0 x in 120 ms with a 3 px INK outline, a 150 ms merge that catches two
bodies, the counter's 1.2 x punch, "+ HEADSHOT ×2", the style letter opaque on the slam and the plate fitting it, an 8 px
decay bar, the round end slammed in place on an opaque band in the upper third, the power-up prompt's plate fitted, the
mod chip off the rest HUD, the speed readout at 18 px or off, DOUBLE POINTS slammed, the swap's name on a plate. N:
RESULTS one focus state (RETRY on entry), NEW BEST a stamp, MODS lines finished and SCAVENGER's own glyph, SETTINGS'
focused row a full cyan fill and HUD SCALE "100 %", TITLE's stray chip and "BEST D". V: the swap glint seated or gone,
the THUNDERCLAP's kick opening the centre, the slide's sleeve off the floor's colour. M: the stamina bar on the cyan
ladder, ~6 x 120 px, a spent flash, an empty pulse. The lead: TITLE's backdrop (the right half floor).

**The lead's round-5 fix (02:24):** `ddcb6b8` TITLE's camera looks 3 deg below its own level (the survival map's box tops
reach 29.2 m and the camera flies 2.5-6 m over them; the old look at 0.85 of that height ~66 m away pitched it ~8 deg
down, the horizon in the frame's top 14 %). Editor compile clean.

## U3-30.10: the beyond entries (each taken: commit, capture, two-line review; each left: why)

| Entry (NIGHT17.md's list + additions) | Commit(s) | Capture | Review (two lines) |
| --- | --- | --- | --- |
| The world answers the rank | `601877f` (the lead's) | `n17-r4/n17-strip-rankup.png` | The map's neon climbs the cyan ladder (up to 2.6 x, pulsing 0.5 -> 2.2 Hz with the rank; REDUCED MOTION: no pulse). In a 200 ms strip the climb is slow to see; it reads over a fight, not a frame. |
| A rank-up moment | `fc96d6f` `2e7c930` (H) | `n17-r4/n17-strip-rankup.png` | The letter slams 3 x behind a slanted cyan streak and the HUD shakes; the streak stays right of the crosshair's middle. Reviewer 4: the letter is a grey ghost for 100+ ms first (in round 5's list to H). |
| The round's end | `51a3eab` (the tally), `2c69225` (the band) (H) | `n17-r4/n17-strip-roundend.png` | ROUND 1 CLEAR at 133 px on a full-width band, "+100 ROUND" under it. The tally line is not inside the strip's 650 ms; the band is see-through and the banner jumps on frame 2 (in round 5's list). |
| Kill streaks as named lines | `e10950c` `d0dd229` (H) | `n17-r4/n17-hud-fight.png` | DOUBLE in the style feed (the web's multikill window, the style scoring the web's). "+ HEADSHOT" repeats instead of merging (in round 5's list). |
| Pickups pulled in, a flash on collection | `639b59e` `9b0afa7` (the lead's), `88994cb` (H's banner + chip) | `n17-r4/n17-strip-pickup.png` | The crystal leans in over the last metre to a catch point in front of the camera; frame 14: a gold edge flash and DOUBLE POINTS. The banner pops in full size (in round 5's list to H). |
| A RESULTS card worth a screenshot | `42f23cf` `5922e5c` (N) | `n17-r4/n17-results.png` | ROUND / POINTS / GRADE in one slanted block with the cyan slash, THE ROUTE's map, the date, seed and wordmark. Reviewer 4: two buttons lit and NEW BEST shaped like a button (fixed in round 5: `28bd276` `00a8513`). |
| Enemy spawns: a telegraph where the beam lands | `585e187` (X) | `n17-r3/n17-strip-spawn.png` | A ground ring and a core at the landing while the body forms. The ring reads pale (near white) at 15 m, not DANGER: noted for GG, not changed. |
| Menus' backdrop | `c6e55bc` (N; CONTROLS / CONTRACTS / WEAPONS on INK), TITLE's slanted INK sheet (N, round 1) | `n17-r4/n17-title.png` | Every menu on opaque INK; TITLE's sheet runs along the 12 deg slant with the living world beside it. No blur (an INK panel instead of a translucent box, as the entry allows). |
| A death signature per gun | `00316e1` `28a777a` (X) | `n17-r4/n17-strip-kill.png` (KESTREL), `n17-r4/n17-strip-horde.png` (THUNDERCLAP) | The KESTREL's clean punch-through and the THUNDERCLAP's shove-back + wide burst differ in the strips. Reviewer 4 still reads a body kill's first frame as a hit's (round 5's list to X). |
| The kill flourish in the hands | `a9cb89a` `bc7d39c` (V) | none | Plays after 0.35 s of quiet following a multikill or a rank-up; every strip ends before that, so no capture shows it. HandsTests cover the trigger and the yield to a shot / swap / punch. |
| The style aura on the gun | `bce235d` `5c0b31d` (V; D-70's recommendation) | `n17-r4/n17-hud-fight.png` | A cyan back-face rim on the held gun, brighter up the ladder. Subtle at C; the top-rank pulse is not in a capture. |
| Low HP: a slow vignette pulse | left | none | Not built: the HP numeral already blinks DANGER at low HP (`9bcdafe`, H) and the post stack's vignette is shared with the freeze's crush frame; no time left after round 4's fix lists. |

## Integration 8 (02:26-02:46): round 5's fixes (the last; build 6 is the close's)

Every builder rebased onto port/stage3 first (their picked commits dropped as duplicates) and reported by 02:36:

| Builder | Commits (on port/stage3) | Their evidence |
| --- | --- | --- |
| V | `33132d4` (tests first) `3dc7f13` the QUICKDRAW strip off (the floating cyan quad that went dark beside the KESTREL; the gem still brightens), the LONGWAVE's sweep band additive; the THUNDERCLAP kicks 7 deg down / 6 deg right (+2 cm, 3 deg roll); the slide's brace off and the sleeve in the hands' ink with a cyan edge; `531b81a` the sprint carry at -3.5 cm (at -5.8 cm + the bob the gun sank under the screen's edge) | HandsTests red 51 / 54 -> 53 / 54 under dotnet (the Editor-only alloc check) |
| M | `f5fe2df` (tests first) `e835830` the stamina bar's colour by state (CYAN-DEEP under a boost's cost, CYAN over it, CYAN-HOT while the sim refills, WHITE 80 ms when spent), a 0.9 s pulse when it can't boost; 160 x 9 px kept (the reviewer's "75 x 4" was the bar in a 1/3 tile; it did drain 0.39 -> 0.30 in the strip, CYAN-HOT on the pale sky) | StaminaBarTests red 0 / 5 -> 5 / 5; fingerprint EQUAL |
| N | `152e56c` (tests first) `28bd276` one focus state (the item left goes dark at once, the new one lights over 90 ms; RETRY lit on RESULTS' entry; hover selects only after a 2 px move); `00a8513` NEW BEST a gold stamp at -8 deg over the points; `096678a` MODS lines finished ("+25%" over "DAMAGE BEYOND 20 M"), gold only on point-paying mods, SCAVENGER's own glyph, white card text; `c8660f3` SETTINGS' focused row the one full CYAN fill, ON / OFF as text chips, HUD SCALE "100 %", values 24 px inside the row; `1f33680` TITLE's stray notch gone, "BEST GRADE D"; `fed03cf` the hint's plate sized to its words | MenusTests red 18 / 20 -> 20 / 20; fingerprint EQUAL |
| H | `bc5c191` (tests first) `2e2ac1f` pops 0.7 -> 1.3 -> 1.0 x in 120 ms, gold from frame 1 with a ~3 px INK outline, a 150 ms merge into the most recently fed pop, the counter's 1.2 x punch on each gain and arrival; `78b05e9` "+ HEADSHOT ×2", the style block enters whole and slams, the plate fits the letter (the decay bar was already 8 px at 1080p); `cbf0862` ROUND N CLEAR slams 1.6 -> 1.0 x in 140 ms from its first frame on an opaque band 110-310 px above the centre; `11a0316` the mod chip off the rest HUD; `2e31551` the speed readout off the default HUD (the 18 px rule); `9f6293d` DOUBLE POINTS slams 2 -> 1 x; `e684538` the swap's name in BODY-BOLD 24 px on an INK plate | Hud17Tests red -> 29 / 29; 58 / 58 with the HUD fixtures; fingerprint EQUAL |
| X | `69ffc84` chunks thrown at 10-16 m/s within 35 deg of the shot, 540-900 deg/s, no gravity for 80 ms, a wall bounce 0.6; `9df56a2` a gun kill's frame 1: the body 1.15 x white-hot (HDR 2.4) for 2 frames + a 14-streak radial burst ~2 x the body (FLASH INTENSITY); `94ae6b0` the headshot: the head block up at 8 m/s on the impact frame, a white ring 0.15 -> 0.9 m; `698d353` (tests first) `c321c53` the kill freeze grows with the step's kills (design.impact.freeze.killPerExtra 0.025, killCap 0.16: 5 kills 0.15 s; budget.perSecond 0.12 -> 0.2 so the capped freeze fits; Core/ImpactRules.cs KillFreezeSeconds, the freeze's length only); `a069f19` the boost's +10 deg whole from frame 1, the sprint's +5 deg, brighter 3 cm edge streaks (the r4 strip was all inside the sprint, so the widen had finished before frame 1); `3f58a96` the slide's sparks 2-4 m ahead at 14 cm (they spawned under the screen's edge), streaks at full brightness; `f9b5673` a shed block flies aside, not at the camera | ImpactCountTests red 0 / 2 -> 2 / 2, ImpactRulesTests 24 / 24, CrunchTests 15 / 15; CoreHarness 0 mismatches, 22 exact; fingerprint EQUAL |

X's note: the orange square in the parry strip is the web's 5-cube hit burst in the enemy's colour on the parry swing's
hit (the lead's `hit` handler); kept (an enemy's colour on an enemy's hit is the role). Editor compile clean after each
builder; the `.meta` files `6a4de9a` `70ff752`. The suite on V + M + N + H (02:35-02:43, `Logs/n17-suite-r5a.json`):
`tests: 966 / 966 passed, 0 failed, 0 skipped (460.33 s)`, `replays: 22 / 22`, `golden tests: 96 / 96`.

**Gates on everything (`70ff752`):**
- The suite (02:45-02:53, `Logs/n17-suite-r5b.json`): `tests: 968 / 968 passed, 0 failed, 0 skipped (474.11 s)`,
  `replays: 22 / 22`, `golden tests: 96 / 96`.
- `Logs/n17-int8-gates.txt`:
  - CoreHarness: `golden total: 39 vector sets (48094 records), 22 replays (22 exact); 0 failing, 0 mismatches`
    (X's Core change is ImpactRules' freeze length only).
  - SimHarness:
    - `fingerprint seed 1: 12 hashes ... EQUAL to the baseline`
    - `residual check: 7 of 7 EQUAL`
    - `impact`: `worst rolling minute: 1850.0 ms budgeted (budget 3600.0 ms)`, `budget HOLDS`, `impact PASS: the budget
      holds on 2 map(s)`.

## Round 5's check in the Editor (Play check 1 of 4; 02:55-02:58)

No build is left before the close's, so round 5's fixes were seen in the live Editor's Play mode instead.
- **How:** one guarded eval coroutine ran FeelStrip's reel scenarios and ShotTour's reel screens on `70ff752`
  (`scratchpad/r5e-reel.cs`), on a 2560 x 1440 Game view.
- **Output:** 7 captures in `Logs/shots/n17-r5e/` (**139 of 150**): kill, headshot, horde, sprint, rankup, roundend,
  results. The run took 57 s.

**Round 5's fixes as seen:**

| Fix | Seen in |
| --- | --- |
| RESULTS has one lit button (RETRY, cyan); TITLE and DETAILS are ink | results |
| The C enters whole: big white, then cyan on its plate (no grey ghost) | rankup, horde |
| "+ HEADSHOT ×2" in the callouts | rankup |
| ROUND 1 CLEAR at 1.6 x on frame 1, then 1.0 x on an opaque band in the upper third; "9 KILLS · BEST +125" under it from about 850 ms | roundend |
| The horde's body held white about 220 ms (`HELD`), then big blocks thrown across the screen; floor rings | horde |
| The pops are gold with an ink outline | all strips |
| Thin pale streaks at the screen's edges while sprinting; the gun carried low | sprint |

**Still wrong:**

| Problem | Seen in |
| --- | --- |
| A kill at 15-20 m: chunks are 2-4 px specks, and the radial burst and the head pop don't show (only a floor glow and a ring) | kill, headshot |
| Gold arcs over the THUNDERCLAP's body (gold is points-only) | horde f1-8 |
| "+120" and "+125" still overprint for 2 frames (`+1205`) | rankup f3-4 |
| The last kill's "+71" prints on "+100 ROUND" | roundend f1-2 |
| The THUNDERCLAP's barrel lies across the right-hand body at hold | horde |

**Round 6's short lists (03:01; last commits by 03:55; a second Editor check before build 6):**
- **X:** far kills keep a minimum screen size (the chunks, the burst, the head pop and ring); the gold arcs to white or
  hot orange.
- **H:** two live pops never overlap (merge them, or stack the newer one); the kill's pop clear of the round bonus.
- **V:** the THUNDERCLAP's hold pose down and right, out of the centre.

## Integration 9 (03:01-03:09): round 6, and its check in the Editor (Play check 2 of 4)

**H, `d865e5e`.**
- The overprint's cause: "+125" was the previous burst, more than 150 ms older, still holding at the crosshair when
  "+120" spawned on the same spot.
- Older pops now leave from wherever they are, and a new burst's pop stacks one line (84 px) above any pop still near
  the crosshair.
- ROUND N CLEAR moves to 270 px above the centre and the bonus / tally line to 180 px; the band spans 110-330 px.
- Hud17Tests + fixtures 58 / 58; fingerprint EQUAL.

**X, `df50998` and `f997c91`.**
- A far kill keeps a minimum screen size: blocks at least 2 % of the screen's height at their distance (at most 4 x),
  the burst at least 22 %, the head ring at least 8 %.
- The gold arcs were the kill-confirm rings in the killing gun's family colour (the THUNDERCLAP's `edge` 0xffb000);
  they are now white for every gun.
- CrunchTests 15 / 15; fingerprint EQUAL.

**V, `a738af9` (tests first, red) and `a62e6ab`.**
- The THUNDERCLAP's kit turns 6 deg down and 10 deg right about the rig's origin (HandsDraw.HoldTurn); the muzzle ends
  right of and below the centre box.
- Its own swap depths are 0.47 / 0.56, and the heavy class's overshoot is about 7 %.
- The overlap test's floor drops 28 -> 27 ms: the turned gun's swap-out overlaps 27.8 ms (4 frames at 144 fps).
- HandsTests 54 / 55 under dotnet (the Editor-only alloc check).

Editor compile clean after each.

**The check:** `scratchpad/r6e-reel.cs` (the same guarded coroutine), 03:11, 4 captures in `Logs/shots/n17-r6e/`
(**143 of 150**: the final reel keeps 7).

| Strip | Seen |
| --- | --- |
| rankup | "+120" stands a line above "+125" from frame 1; "+125" flies to the counter; no overprint |
| horde | The rings are white. The THUNDERCLAP rests lower-right with the dying body's centre open. The white body breaks into white then orange blocks thrown across the screen. |
| kill (15-20 m) | The kill-confirm ring is white. The blocks are a little bigger but still small. The 2-frame burst isn't in any captured frame: the Editor rendered at ~10 fps in this run, frames 33-150 ms apart, so it can't be judged. |
| roundend | Frame 1 (the 1.6 x slam) still overlaps "+100 ROUND" with the kill's "+71" for one frame; from frame 2 they're apart |

**The code is frozen at `a62e6ab`** for build 6 (the captures left are the final reel's).
- **The suite on it** (03:12-03:20, `Logs/n17-suite-frozen.json`): `tests: 969 / 969 passed, 0 failed, 0 skipped
  (416.56 s)`, `replays: 22 / 22`, `golden tests: 96 / 96`.
- **Headless** (`Logs/n17-frozen-gates.txt`):
  - CoreHarness: `golden total: 39 vector sets (48094 records), 22 replays (22 exact); 0 failing, 0 mismatches`.
  - SimHarness:
    - `fingerprint seed 1: 12 hashes ... EQUAL to the baseline`
    - `residual check: 7 of 7 EQUAL`
    - `impact PASS: the budget holds on 2 map(s)`
    - `spam: targets (a) (b) (c) (e) (f) HOLD`.

## The frozen code's smoke in the Editor (Play check 3 of 4; 03:28-03:42; no captures)

The smoke runs in the Player at the close with no build left after it. This was a dry run of its path on `a62e6ab`
(`SliceSmoke.Begin()` in Play mode).

**The first try refused:** `FAIL the smoke plays the arena: this composition plays 'drydock'`.
- The Editor's saved MAP is DRYDOCK. **So round 5's and round 6's Editor strips (`n17-r5e`, `n17-r6e`) were shot on
  DRYDOCK**, not the reel's ARENA. That's why their zones read THE GANGWAY / THE CRADLE and their kills sit at 15-20 m.
- The arena was then loaded by `SurvivalGame.PendingMapId` and a scene reload. That choice isn't saved, so GG's saved
  MAP is unchanged.

**The run** (`Logs/n17-editor-smoke.json`): `38 PASS, 3 FAIL`, `DONE: 3 step(s) failed`.
- Passed: the buys (ARMORY, VIGOR, the box, every gun), the PROTOCOL, `EXFIL at round 11`, `clean slate ... 12 of 12`,
  `tape ... EQUAL (1 of 1 checkpoints, 486 steps)`, `autopilot ... 30 of 30 hashes equal`, `attract ... the next run
  EQUAL`.
- Failed: the three TITLE pad steps (`virtual pad: TITLE selected 'none'`, `virtual pad menus: the d-pad reached
  'PROGRESS' ...`, `pad only: TITLE ('none' selected)`).

**Their cause, measured:**
- After `g.ToTitle()` in the Editor, one eval later the flow read `Playing sel=none attract=True focused=True fps=61`.
- In the Editor the game isn't in automation mode, so the attract takes TITLE after its idle time, and its lock
  (`UiKiosk.KioskTitleLock`) clears the selection.
- The Player's `-skygrindSmoke` runs as automation; build 5's Player smoke selected `'PLAY'` and passed all three steps
  on its re-run.
- Round 5's focus commit (`28bd276`) changed only the lit look and the hover threshold, not the selection.

Read as the Editor's environment. **The close's Player smoke is the check.**

## U3-30.11 the close (04:55:39-; `date` at 04:55:39 = 480 min after the first)

**Build 6, the final** (code `a62e6ab`; docs only since: `git diff --stat a62e6ab..HEAD -- Assets` is empty):
- Started 04:58:50, ended 05:08:32 (582 s: a full rebuild). `Succeeded`, 0 errors, 495 warnings.
- `git diff --stat ProjectSettings`: only UnityConnectSettings (not staged).
- The build rewrote the four theme SDF font assets; restored with `git checkout --` (no font change was meant).

**The smoke:**
- First run (05:09:36-05:13:16, idle 0 s: the build and its CLI calls had just run): `DONE: 1 step(s) failed`. The one
  failure was `virtual pad menus: ... down reached 'Row crosshairColor', right moved a value False`, the same step build
  5's first run failed under load.
- **The re-run** (05:16:30-05:19:56, idle 321 s, nothing else running): **`[smoke] 182.9s DONE: all steps passed`**.

**The Player checks** (`scratchpad/close-checks.sh`, batchmode):
- `-skygrindGolden`: `golden total: 39 vector sets (48094 records), 22 replays (22 exact); 0 failing, 0 mismatches`.
- `-skygrindFingerprint`: `seed 1: 12 hashes over 7200 steps in 1.22 s: EQUAL to the baseline`.
- `-skygrindReplay` on the smoke's tape: `EQUAL (1 of 1 checkpoints, 491 steps); seed 737373, Stopped, stamp SKYGRIND
  0.1.0 (a105aaf)`.

**The suite:** on the identical code `a62e6ab` (03:12-03:20, `Logs/n17-suite-frozen.json`): `tests: 969 / 969 passed, 0
failed, 0 skipped`, `replays: 22 / 22`, `golden tests: 96 / 96`. It was not re-run after the build: the code was
unchanged and the time went to the checks above.

**The Silent series:** not run. The plan read `Balanced` at 03:2x and again at 05:21 (on AC); NIGHT17.md runs it only
on the Silent plan, and the plan is GG's to set.

**The final reel** (05:20-05:21, `-skygrindReelOnly punchkill,kill,title,settings1,paused,fight,results`): 7 captures
in `Logs/shots/n17-final/` (**150 of 150**).

**Round 5's scores, the lead's** (the final reel; no reviewer: 4 of 4 spent):

| # | Row | Score | Why (one line) |
| --- | --- | --- | --- |
| 1 | Kill crunch | 3 | In the fight frame, big orange blocks are thrown wide with a white ring. The KESTREL kill at ~10 m still reads as a ring and sparks, not a break. |
| 2 | Hit feedback | 3 | Not in tonight's last reel; round 4's capsule still doesn't move. |
| 3 | Weapon feel | 3 | The aura rim and the carry read; no new gun capture. |
| 4 | Movement feel | 3 | Edge streaks and the bar's colour (Editor strips); no Player strip of it. |
| 5 | Score energy | 4 | Outlined gold pops; no overprint (stacked, Editor r6e); the counter on its plate. |
| 6 | Style meter | 4 | On its plate: C, COOL, DOUBLE, "+ HEADSHOT ×6", the decay bar. |
| 7 | HUD clarity | 4 | No speed readout, no mod chip; the hint's plate fits "POWER-UP · GRAB IT"; "ROUND 1 · 3 LEFT". |
| 8 | Menu simplicity | 4 | SETTINGS: one full-cyan focused row, ON / OFF chips, "100 %"; TITLE 4 + the card; RESULTS 3. |
| 9 | Identity | 4 | TITLE now shows the dusk: the horizon at mid-frame, the sun's band behind the map. |
| 10 | Motion | 3 | Stills can't show it; the slams and the throw moved in the Editor strips. |
| | **Total** | **35 / 50** (round 4: 34; round 0: 17) | |

## The close table

| Item | DONE / SHORT | Evidence ("done when", from this session) |
| --- | --- | --- |
| U3-30.1 Start | DONE | RECEIVED's rows logged 20:55-21:02; tasks.json parses; round 0 scored 17 / 50 on the n16-before reel (18 captures) |
| U3-30.2 The identity | DONE | fonts + the tile `fcdc730`; the tokens in UnityTuning.Theme.cs + IDENTITY.md `804aea6`; the tile reviewed in two lines; every builder told 21:40 |
| U3-30.3 The shared commit | DONE | `e97d6a0` `611afab` `89429cd` `8e5b416` + the reel `c78a214`; Editor compile clean; the suite 849 / 850 + the one re-run green; fingerprint EQUAL; build 1's reel ran (24 captures); builders launched 21:16 |
| U3-30.4 One bar, a sprint, a partial boost | DONE | golden `0 failing, 0 mismatches`, 22 exact (every gate; build 6's `-skygrindGolden` the same); StaminaTests 0 / 21 red -> 21 / 21; spam: sprint 10.5 m/s under the slide's 11.6, a 0.5-fill boost = 0.50 x a full one; the re-record `964899b`; the boost / sprint strips reviewed rounds 1-4 |
| U3-30.5 Crunch | SHORT (a cap: round 4 was the item's deadline, reviewers 4 / 4 spent) | strips reviewed every round; rows 1-2 at round 4: the lead 4 / 3, the reviewer 2 / 3; round 5 (the final reel): 3 / 3 |
| U3-30.6 The hands | SHORT (the same cap) | strips reviewed every round; row 3 at round 4: the lead 3, the reviewer 3; HandsTests green in the suite (969 / 969 on `a62e6ab`) |
| U3-30.7 The HUD rebuilt | SHORT (the same cap) | HUD captures reviewed every round; rows 5-7 at round 4: the lead 3 / 4 / 3, the reviewer 2 / 3 / 3 (round 5: 4 / 4 / 4) |
| U3-30.8 Menus and words | SHORT (the same cap) | UI captures reviewed every round; rows 8-10 at round 4: the lead 4 / 4 / 3, the reviewer 4 / 3 / 2; the smoke's menu and pad steps passed on every round's build (builds 5 and 6: on the re-run) |
| U3-30.9 The rounds | DONE | four rounds logged on builds 2-5 (the lead 30 / 35 / 37 / 34, the reviewers 25 / 23 / 27 / 27), each with the reviewer's 5 worst, the fix lists and what changed; two more fix rounds checked in the Editor |
| U3-30.10 Beyond the ask | DONE | 11 entries taken, each with commits, a capture (the flourish: none, with why) and a two-line review; the low-HP vignette left with its reason (U3-30.10's table) |
| U3-30.11 Close | DONE | build 6 `Succeeded`; the smoke's re-run `DONE: all steps passed`; `-skygrindGolden` 0 mismatches; `-skygrindFingerprint` EQUAL; `-skygrindReplay` EQUAL; the suite 969 / 969 on the same code; the Silent series not run (Balanced, measured); the final reel scored (round 5: 35 / 50); NIGHT17-REPORT.md, FEEL-PLAYTEST.md, PORT-STATUS.md, IDENTITY.md, HANDOFF.md (`u3-n17`), tasks.json; NIGHT18.md renumbered; worktrees removed; tag `u3-n17` |

Caps used:

| Cap | Used |
| --- | --- |
| Builds | 6 of 6 |
| Play checks | 3 of 4 |
| Captures | 150 of 150 |
| Builders | 5 of 5 |
| Reviewers | 4 of 4 |
| Research | 0 of 1 |
| Bench runs | 0 of 3 |
| Verification | about 80 of 90 lead minutes (the close's build, smokes, checks and reel: about 16) |
