# NIGHT17.md: SKYGRIND in Unity: THE OVERHAUL NIGHT (an identity, kills with crunch, the HUD and menus rebuilt, one-bar stamina), at least 8 h, hard cap 9 h

> **Written 2026-10-05 ~9 PM, after the NIGHT16 close** (anchor `u3-n16`), from GG's message and 40 minutes of research
> (`Research/2026-10-05-overhaul.md`, four agents). GG's words, trimmed:
>
> "you should start out with only one bar ... you should be able to Sprint and it uses your [stamina] bar ... you can
> either choose the boost or hold a Sprint ... you can still boost even if the bar is not fully filled, it just won't be
> as much of a boost ... the UI just all looks very cheap ... the menus don't really follow proper game design ... way too
> wordy ... way too cluttered and has way too many options ... a quality of life overhaul completely ... how points and
> stuff are shown, the score, it just feels very stale, not energetic ... the effects and the animations are very poor ...
> I don't care about the map and the weapons [and] the enemies yet ... I feel like we're going in circles, so we need to
> find new methods ... I want it to feel like ULTRAKILL where it's just so satisfying to defeat enemies ... it's not
> crunchy ... the text and the font and the colors are just very bland, it needs identity ... minimum 8 hours, hard cap 9
> hours ... massive changes, not subtle changes ... testing only for maybe an hour, an hour and a half max ... actually
> loop and iterate ... animations, swapping weapons, satisfying hits ... get outside the box and go well beyond what I'm
> thinking of."
>
> Map night 2 moved to `NIGHT18.md` (deferred one night). No new guns, enemies, enemy models or map work tonight.

You are Claude Code working in the Unity project `C:\Users\gaige\dev\SKYGRIND-unity` (Unity 6000.3.7f1, URP). GG starts
this run and leaves; the Editor is open on this project. Nobody answers questions during the night.

## GG's switches (GG may edit these before starting; the defaults are in capitals)

| Switch | Default | The other choice |
| --- | --- | --- |
| **AUDIO** | **OFF**: CLAUDE.md rule 7 holds (no audio code, no sound files). Every new moment emits an event for GG's sounds. | **ON**: builder X adds placeholder sounds synthesised in code at load (sfxr-style, no files), pooled, on the existing events, with an SFX VOLUME row. The research says sound is the one crunch factor nothing replaces. |
| **CHUNKS** | **ON**: a killed enemy breaks into its own blocks, which tumble and fade within 1.5 s. Nothing stays, so D-29 ("no gore that stays") holds. | **OFF**: the dissolve only. |
| **MARKS** | **OFF**: nothing left on floors or walls (D-29). | **ON**: enemy-coloured scorch marks that fade over 8 s. |
| **STAMINA KEY** | **ONE KEY**: Shift boosts on the press, and holding Shift runs on into a sprint. No new key, no input lag. | **TWO KEYS**: Shift boosts; a separate SPRINT (L-Ctrl / Mouse 4 / L3) sprints. |

## Why this night (the critique; it decides every trade-off below)

Sixteen nights made a faithful, deterministic port, and every night after the port polished it by small steps. GG's
verdict is that it still looks cheap and doesn't feel good. From GG's screenshots (2026-10-05 20:06-20:31) and the
NIGHT16 captures, judged against the research checklist:

1. **Kills aren't crunchy.**
   - A kill is: a 0.04-0.09 s freeze, a cube burst, a 0.22 s dissolve, a ring and a light pop.
   - During the freeze nothing tells you it's deliberate, so a freeze with no sound reads as lag.
   - Hits that don't kill barely register: 5 cubes and a 5 cm flinch.
   - The victim never reacts like it was struck.
2. **The UI is a programmer's UI.**
   - Small condensed caps everywhere; a low-alpha grey plate at every corner; thin lines.
   - No focal point: no hierarchy, and nothing moves.
   - Developer things are shown to players: COPY REPORT, COPY SCORE LINE, SAVE CARD, COPY SEED, a version stamp, the LAB
     tab.
3. **Too many words.**
   - Sentence-long toasts ("L-SHIFT DASHES, C SLIDES (SLAMS IN THE AIR): STYLISH MOVES RAISE YOUR POINTS").
   - A 190-character key line on TITLE; key hints living permanently in the HUD ("X NEXT · D-PAD LEFT BACK", "ZL ZOOM
     ×1.5"); 92-113 character tips.
4. **Too many options.**
   - PAUSED shows 7 buttons and a whole key list.
   - TITLE shows 7 buttons and a strip of 8 mode / HEAT / map buttons.
   - SETTINGS has **50 rows in 7 tabs**.
   - RESULTS has 6 buttons, 4 lines of stats and a raw score string.
5. **The score is stale.**
   - The points total just changes.
   - The style plate is a grey "D DULL" with a thin bar.
   - The round counter is a tiny tally; nothing celebrates.
6. **No identity.**
   - Generic condensed fonts in one weight at small sizes.
   - Colours used at low alpha, so cyan and gold read as washed out.
   - The one strong idea, the wordmark's cyan on an offset, is used nowhere else.
7. **Movement balance (GG):** start with one bar; sprint and boost share it, and a boost from a partly filled bar is
   weaker.

## How tonight works (GG: "we're going in circles, find new methods")

- **Rebuild, don't nudge.** Each area gets a new version built against a target picture. "Alpha 0.12 to 0.07" is not
  tonight's kind of change.
- **Identity first, applied everywhere through one theme.** In the first 1.5 h the lead decides a style tile: the
  fonts, colour tokens, shape language, motion language and type scale. Every screen reads it from `UiTheme`, so a
  token change restyles the whole game.
- **A loop with eyes.** Four rounds. In each, the lead builds, runs the **feel reel** (fixed strips + the UI pass),
  **scores the rubric** below, and sends each builder a fix list aimed at the lowest rows.
- **A fresh critic each round:** one read-only reviewer agent sees only the round's captures and the checklist, never
  the code. People stop seeing their own work; GG's "going in circles" is partly that.
- **Look more, test less.** The verification budget is **90 minutes of lead time all night** (GG). Tests only for pure
  rules (the stamina maths, the gates). Visuals are proven by the reel. Suites run async and only count when waited on.
- **Go beyond the ask.** From round 2, the "beyond" list (U3-30.10) adds what GG didn't name but players feel.

## The rubric (scored 1-5 per round from the reel; the lead and the reviewer each score; aim: every row ≥ 4 by round 4)

| # | Row | What a 5 looks like |
| --- | --- | --- |
| 1 | Kill crunch | A body kill, a headshot kill, a punch kill and a horde each read as an event in 1 frame: the victim reacts, the freeze is unmistakably deliberate, something flies. |
| 2 | Hit feedback | A hit that doesn't kill still visibly lands on the enemy (flash, tilt) and in the hands. |
| 3 | Weapon feel | The shot kicks the gun (spring, overshoot), the camera kicks with it, a swap snaps, and every gun reads differently. |
| 4 | Movement feel | Boost, sprint and slide feel distinct, and the bar is readable at a glance. |
| 5 | Score energy | Points punch and roll, pops merge, and the round's end is a moment. |
| 6 | Style meter | The rank slams, callouts stack, decay is visible, and rank ups are felt. |
| 7 | HUD clarity | 200 ms glance test, greyscale test, 50 % size test; no key hints at rest. |
| 8 | Menu simplicity | Items per screen, words per line, steps from boot to PLAY. |
| 9 | Identity | One frame says SKYGRIND: fonts, colours, shapes. |
| 10 | Motion | Every entrance animated with ease-out-back, nothing linear, nothing pops in raw. |

Log each round's scores as a table in `NIGHT17-LOG.md` with a one-line reason per score; the trend is the night's proof.

## Read these files

- **`CLAUDE.md`** first, then this file, then **`Research/2026-10-05-overhaul.md`** (all of it: the numbers, the
  checklist, the code audit with file:line).
- **`HANDOFF.md`** (sections 0, 1, 4, 6), **`FEEL-DESIGN.md`** sections 5-6, **`Reports/NIGHT16-web-gaps.md`**,
  **`DESIGN-PILLARS.md`**, **`PORT-RULES.md`** sections 6, 11 and 12.
- **`tasks.json`**: write U3-30 and U3-30.1 .. U3-30.11 before any code. `NIGHT18.md` (map night 2) used U3-30 for
  itself; it renumbers when it runs.
- **The memory notes:** `unity-editor-unfocused-autotick`, `play-check-eval-recipe`, `night-parallel-agents`,
  `bench-power-plan-flips`, `skygrind-targets-and-taste`.

**Run length: at least 8 hours, hard cap 9 hours from your first `date`.** The close starts at 8:00, not before. GG
named an early close as a failure; so is running past 9:00.

## 0. Tonight's rules (on top of CLAUDE.md, which is in force except where this prompt, GG's, says otherwise)

1. **Git:** branch `port/stage3` from its head (`u3-n16`). Tag `u3-n17` once, last, on the close commit. No push, no
   merge, nothing on master. The web repo is read only.
2. **What this prompt changes in CLAUDE.md:**
   - **Rule 8, fonts and text:** new OFL fonts are allowed (with their licence file committed beside them), and the UI
     text floor rises **from 16 to 18 px at 1080p**, so the Steam Deck gets at least 13 px.
   - The **colour roles stay exactly as they are**: cyan is the player's, red-orange is danger, magenta is RIVAL,
     pale violet-white is the boundary, gold is points.
   - Identity comes from **value, saturation, type, shape and motion**, not new hues.
   - The style rank ladder is a **cyan value ladder** (pale, then saturated, then white-hot with glow), not the web's
     hue ladder.
   - **Rule 7 (audio)** holds unless the AUDIO switch is ON. **D-29** holds unless MARKS is ON. **D-30 (no new
     packages)** holds: tweens are code.
3. **Pillar 1 is the law:**
   - No animation lock: visuals follow the sim's timers and never gate them.
   - No eaten input.
   - No added latency: the boost fires on the press.
   - A shot during a swap fires when the sim allows, and the animation yields.
4. **Effects are pictures.** Chunks, flashes, shake, the HUD and menus all live in the Game layer. The composed
   fingerprint stays EQUAL across every effects commit; an effect that changes it is in the wrong layer. `kill` / `hit`
   handlers only queue work.
5. **The stamina rules are tonight's one gameplay change (GG asked for them).**
   - They live in Core (`Core/Stamina.cs`, pure and tested) behind `design.stamina.*` in `Design/unity-config.json`,
     with `_why` and the byte-identical Player copy. Each rule has a flag, ON by default.
   - **U3-30.4 names `Mover.cs`:** new fields and guarded hooks only, as NIGHT16's `design.move` did. The web's lines
     stay.
   - **Gates at every Core commit:** golden 0 mismatches and 22 / 22 replays, with the rules neutral in the vectors.
   - **The fingerprint is re-recorded once**, in its own commit, with the before / after lines.
   - **THE TAPE goes to format 3** (a stamina flags byte); formats 1-2 replay with the flags off. "stamina" joins
     `MoverState.Fields`, optional when absent.
   - The farm diff and the spam / verbs tables, before and after.
6. **Numbers:**
   - Gameplay numbers go in `design.stamina` (and `design.feel.*` if any gameplay timing changes), each with `_why`.
   - Every visual number goes in a `UnityTuning.*.cs` file with a comment.
   - The identity's tokens live in **one place**: `UnityTuning.Theme.cs` (fonts, colours, the type scale, the slant, the
     corner cut, the motion durations), read through `UiTheme`.
   - No `new Color(` literal in UI code after tonight: the 55 that exist move to tokens.
7. **Developer things only in a developer build.**
   - `game.devMode` is set by `-skygrindDev` or by any automation flag.
   - It guards COPY REPORT, COPY SCORE LINE, SAVE CARD, COPY SEED, the LAB tab, the version stamp and the dev overlay.
   - Every automation flag, the smoke, the strips and the tour still reach what they need.
8. **Accessibility:**
   - Every flash goes through `game.flashGate` (at most 3 a second) and FLASH INTENSITY.
   - REDUCED MOTION stills the HUD shake and the camera shake, and halves the chunks' spin.
   - Glyphs match the device.
   - Text ≥ 18 px.
   - A HUD SCALE row (0.8-1.25) and a 48-64 px safe inset.
9. **Performance:**
   - No allocation per frame in new code (PORT-RULES 6): pools only, and the tween helper allocation-free.
   - Chunks and particles stay under the quality preset's cap.
   - The worst 1 % stays under 6.9 ms: the close's Silent series, if on AC and idle.
10. **Caps:**
    - Player builds 6.
    - Editor Play checks 4.
    - Captures and strips 150.
    - Builders 5, resumable by SendMessage.
    - Reviewer agents 4 (one per round).
    - Research agents 1 (only for fonts if the downloads fail).
    - Bench runs 3 (the close's series).
    - **Verification budget: 90 minutes of lead time**, logged per check.
    - A cap reached makes its item SHORT with the count.
11. **One driver for the Editor:**
    - Builders work in worktrees, dotnet only (`Tools/wt-csproj.cjs`).
    - Agents never run `unity`.
    - Topic-named members in shared partials (PORT-RULES 11).
    - Tests first, seen red at cherry-pick.
    - Hold back any change to a path the Player smoke drives if no build is left to try it (memory
      `night-parallel-agents`).
12. **No questions, no waiting:**
    - One status line per item in `NIGHT17-LOG.md`.
    - End every message with `N17-STATUS: <d> done, <s> short, <l> left (of 11) · round <r> · tests <passed>/<total> ·
      <h:mm from the first date>`.

## 1. The identity (U3-30.2 decides it; these are the starting proposals)

- **Fonts (OFL, Google Fonts):**
  - Display: **Archivo Extra Condensed Black Italic** (Archivo's width axis at 62).
  - Body: **Archivo** (width 100, Medium / SemiBold).
  - Rank letters and the round number: **Tourney**.
  - Compare against one alternative pair (Big Shoulders Display + Saira, or Anton + Barlow) in one capture, then pick.
  - Bake the static TMP SDF atlases through `Editor/ThemeFonts.cs`.
  - Digits are tabular (monospaced).
- **The colour tokens:**
  - INK: opaque near-black violet for panels, at least 92 % alpha or no panel at all.
  - WHITE: text.
  - CYAN: the hero colour (the player, focus, fills).
  - CYAN-PALE and CYAN-HOT: the value ladder.
  - GOLD: points only.
  - DANGER (red-orange) and RIVAL (magenta), unchanged.
  - Muted text is a token too, never "text at 0.74 alpha".
- **Shape:**
  - One slant, 12 deg (matching the display italic).
  - 45 deg corner cuts of 10 px.
  - 4 px accent bars.
  - Hard text shadows: a 2 px offset in INK, or the wordmark's offset in CYAN for hero numbers.
  - No 1 px outlines and no translucent grey plates.
- **The type scale (1080p):** 18 / 24 / 32 / 42 / 56 / 75 / 100 / 133. At most 4 sizes per screen.
- **Motion:**
  - In: 120-180 ms, ease-out-back, about 10 % overshoot.
  - Out: 80-120 ms, ease-in.
  - Menus enter along the slant.
  - Every number change animates (a roll, a punch or a pop).
  - Nothing linear.

## 2. Who owns what (builders, from the shared commit)

| Builder | Owns (creates or edits only these) | Proves with |
| --- | --- | --- |
| **M (stamina)** | `Core/Stamina.cs`, `Mover.cs` (new fields and guarded hooks), `Game/SurvivalSim.Stamina.cs`, `Tape/RunTape.cs` + recorder / replay (format 3), `RunSnapshot.cs` (MoverState), `BotBrain.cs` / `Nav/Follower.cs` (dash links need a full bar), the smoke's pip assertion, `Tests/EditMode/StaminaTests.cs`, `Tools/SimHarness/Harness.Spam.cs` | golden + replays; spam / verbs before and after; the farm diff; the fingerprint re-record |
| **X (crunch)** | `EnemyViews.cs` (EnemyLook's hit / death, new chunks), new `Game/Chunks.cs`, `Decals.cs` (MARKS only), `ImpactFx.cs`, `KillConfirm.cs`, `Trauma.cs` / `CameraFeel.cs` (shake and kick tuning only), `LightPools.cs`, `PostFx.cs` (impact frame), `WeaponFx.cs` (muzzle stars), `FxGates.cs`, `UnityTuning.Crunch.cs`; the AUDIO switch's synth if ON | strips: kill, headshot, punch kill, horde, parry |
| **V (hands)** | `Viewmodel*.cs`, `BlockyKit.cs` (arm parts), `UnityTuning.Hands.cs`, `HandsTests.cs` | strips: swap-all, fire per gun, punch, boost / sprint poses |
| **H (HUD)** | `Ui.cs` (the HUD half after the split), `UiHudMotion`, `UiFeelHud`, `UiHotbar`, `UiPointsPops`, `UiFeelEdges`, `UiFeelTags`, `UiMoveRow`, `UiAds`, `UiPerks`, `UiPowerUps`, new `UiStyleMeter.cs` / `UiStaminaBar.cs`, `UnityTuning.FeelHud.cs` / `Hotbar.cs` | captures: the HUD at rest, in a fight, at a rank-up, at a round end |
| **N (menus and words)** | new `UiMenus.cs` (the menu half after the split), `UiMeta`, `UiResults`, `UiSettings` (layout), `GameSettings.cs` (which rows show, not their values), `UiControls`, `UiMods`, `UiOnboarding`, `UiNav`, `UiReport`, `UnityTuning.Impressions.cs` (the copy) | captures: TITLE, PAUSED, every SETTINGS tab, RESULTS, DOWN, MODS, the first minute's prompts |
| **Lead** | the shared commit, `UnityTuning.Theme.cs` / `UiTheme` / `UiTween`, the fonts, `FeelStrip.cs` / `ShotTour.cs` (the reel), `SurvivalGame*.cs` loop lines, every doc, the Editor, git, the reviewers | the reel, the rubric, every gate |

## 3. The items (U3-30.1 .. U3-30.11)

### U3-30.1 Start (lead, 20 min)
- `date`; HANDOFF section 1, row by row; `set_autotick`; RECEIVED in `NIGHT17-LOG.md`; the machine (power plan, AC,
  CPU); U3-30.* in tasks.json.
- Copy `Builds/Stage1/` (build 4, `u3-n16`) to `Builds/n16-before/` for GG's A/B.
- Run the **before reel** on it: the existing strips plus `-skygrindShots ... -skygrindShotsUi`, into
  `Logs/shots/n17-before/`.
- Score the rubric once on that reel: round 0, the baseline.
- **Done when:** the rows are logged, tasks.json parses, and round 0's scores are in the log with their captures.

### U3-30.2 The identity (lead, by 1:30)
- Download the candidate fonts' TTFs and their OFL licences into `Assets/Fonts/<family>/`, then bake their TMP assets.
- Build a **style tile** screen: a hidden debug screen, or a ShotTour pose. It shows the vitals block, a menu row, a
  points pop, the rank letter, the round number and a settings row, in each direction.
- Capture it, pick a direction, and write `IDENTITY.md`: the tile in words, the tokens, and do / don't examples.
- Commit the tokens to `UnityTuning.Theme.cs`.
- **Done when:** IDENTITY.md and the tokens are committed, the tile capture is reviewed in two lines, and every builder
  is told by SendMessage.

### U3-30.3 The shared commit (lead, by 0:50; then launch the builders)
- `UiTheme` (tokens: fonts, colours, the type scale, slant, cut, durations; today's values until U3-30.2).
- `UiTween`: allocation-free ease-out-back, punch, count-up / odometer and slide, driven by frame time, struct-based.
- **The Ui.cs split:** move the menu-building code into `UiMenus.cs` as a pure move. Check TITLE / PAUSED / RESULTS
  captures before and after; no behaviour change.
- `game.devMode` and its guards (rule 7).
- The `design.stamina` keys with `_why`, and the Player copy.
- **The feel reel:**
  - New named FeelStrip scenarios: `kill`, `headshot`, `punchkill`, `horde`, `boost`, `sprint`, `swapall`, `rankup`,
    `roundend`.
  - The UI pass extended: TITLE, PAUSED, each SETTINGS tab, RESULTS, DOWN, MODS, the HUD at rest and in a fight.
  - Started by one command, `-skygrindReel <dir>`.
- Empty test fixtures for M.
- **Done when:** the Editor compile is clean, the suite is green, the fingerprint is EQUAL, the reel runs on the shared
  commit's build, and the builders are launched.

### U3-30.4 One bar, a sprint, a partial boost (M; the starting numbers from the research, tunable with reasons)
- **The bar:** 1.0 at the start; VIGOR adds a second bar. Today's 3 pips are retired from the HUD.
- **The boost** (fires on the press):
  - Costs up to 0.5 bar.
  - With less than its cost it still fires, its **duration** scaled by fill / cost (the speed stays 24 m/s), with a floor
    of 0.25 of the full boost.
  - A fill ≥ 90 % of the cost counts as full.
  - The invulnerability frames scale with the duration.
- **The sprint** (the STAMINA KEY switch):
  - 10.5 m/s on the ground (under the slide's 11.6), draining 0.4 bar/s while held and moving.
  - It ends by itself when the bar is empty; no lockout.
- **The refill:** 0.8 bar/s, 0.4 s after the last spend (0.8 s if a sprint emptied it). A kill refills 0.15, a slam
  landing 0.25, a parry all of it (ULTRAKILL / DOOM: aggression pays for motion).
- **The dash-jump's extra cost:** in bar units (proposed 0.25).
- The bot's dash links need a full bar; `RestoreStamina` keeps working.
- Each rule has its own flag in `design.stamina`, ON. The spam and verbs tables are run before and after, then the farm
  diff.
- **Done when:** golden 0 mismatches with 22 exact; StaminaTests seen red then green; the spam table shows sprint below
  the slide line and a boost at 0.5 fill travelling half a boost; the fingerprint re-record is committed; the boost and
  sprint strips are reviewed.

### U3-30.5 Crunch (X)
1. **The kill freeze reads as deliberate:**
   - the victim is fully white and vibrates (2-4 cm, decaying);
   - the burst, the chunks and the shake onset all land **on the freeze's first frame**.
2. **A hit flash on every damaging hit** (white, eased over 0.1 s), plus a tilt / pop away from the shot for 60-100 ms,
   view only.
3. **CHUNKS:** the dead enemy's own blocks fly along the shot with spin, bounce off the floor (simple maths against the
   world collider: no PhysX), and fade within 1.5 s. Overkill throws them harder; a headshot kill pops the head block
   high. Pooled, under the preset cap.
4. **Damage shows before death** (DOOM Eternal's destructible demons): below 50 % HP an enemy sheds a non-structural
   block, view only.
5. **Rotational trauma shake** (trauma², Perlin noise, at most about 2 deg yaw / pitch and 3 deg roll in first person, no
   translation), tuned per weapon class.
6. **A muzzle star per gun** in its family colour, on screen for at most 1 frame plus a half-strength 2nd frame, through
   the flash gate.
7. **An impact frame** (one frame of inverted / white) on parries and 3+ multikills only, through the flash gate.
8. **MARKS** if ON.
- **Done when:** the kill / headshot / punchkill / horde / parry strips are reviewed each round, and rubric rows 1-2
  reach ≥ 4 by round 4 (or SHORT with the scores).

### U3-30.6 The hands (V)
- **Swaps that snap:** the visual in-tos sped up with overshoot and settle (the sim's swap timer unchanged); each gun's
  draw keeps its signature.
- **A QUICKDRAW glint** for the 0.5 s after a swap.
- **Recoil as per-gun spring impulses** (pitch up and back, roll, overshoot) paired with the camera kick; each gun
  distinct.
- The punch: more anticipation, a harder stop at impact.
- **Boost and sprint poses:** the gun tucks and tilts while sprinting and swings on a boost.
- Faster in-tos even if they look rougher (DOOM's lesson).
- **Done when:** the swapall / fire / punch / boost / sprint strips are reviewed each round, rubric row 3 ≥ 4, and
  HandsTests are green.

### U3-30.7 The HUD rebuilt (H)
- **The three tiers:** glance (HP, points, the stamina bar) in big numerals with no labels; secondary (rank, weapon,
  round); teaching (gone at rest).
- **Points:** an odometer with tabular digits; a punch of 1.0 -> 1.15 -> 1.0 at most every 100 ms; one white frame; the
  pops merge ("+150 ×3") and are sized by value (24 / 32 / 42).
- **The style meter, ULTRAKILL-like:**
  - the letter slams 2x -> 1x in 100 ms, the word shows for 600 ms, a decay bar sits under the letter;
  - named callouts, newest on top;
  - the cyan value ladder;
  - a rank drop on damage, shown;
  - the multiplier chip ("×1.2 AIR", NIGHT16 entry 39).
- **The stamina bar under the crosshair:** dim and hollow below the boost floor, solid above it, one flash when full,
  and the `staminaFull` event.
- **The weapon** as a card with a silhouette drawn in code (Neon White), not a name plus key hints. Hints move to
  onboarding.
- **The round:** a hero number of 133-160 px at the centre for 1.5 s at a round start, then a 400 ms tween to its corner.
- **A kill marker distinct from the hit marker.**
- HUD SCALE and the safe inset.
- **Done when:** the HUD captures are reviewed each round and rubric rows 5-7 ≥ 4 by round 4.

### U3-30.8 Menus and words (N)
- **TITLE:** one-press PLAY, big. The run setup (mode, HEAT, map) as one compact card with sensible defaults. No key
  line. At most 5 items.
- **PAUSED:** RESUME / RESTART / SETTINGS / QUIT (QUIT confirms and autosaves at a break), under a one-line run header
  (round · points · time). Keys and aim assist move into SETTINGS.
- **SETTINGS:**
  - At most 5 tabs: GAME / CONTROLS / VIDEO / ACCESSIBILITY / plus GAMEPAD when a pad is present.
  - At most 8 rows per tab before an ADVANCED fold.
  - Sliders for ranges; a one-line footer for the focused row (8 words at most); the LAB in dev only.
  - Mouse hover selects (reuse UiNav's frame), with the smoke's pad paths re-run before the build is trusted.
- **RESULTS:**
  - a hero tally: the round, the points, the grade slam; lines 200 ms apart, any input completes it;
  - 2 buttons (RETRY (R), TITLE) plus DETAILS for the rest;
  - developer buttons in dev only.
- **DOWN:** the killer drawn properly (not the pale pill), "WHAT GOT YOU" in the display face.
- **MODS:** cards with an icon, a one-line effect and rarity as a colour bar; REROLL with its cost.
- **Every player-facing string rewritten:** at most 8 words, and 3 on the HUD.
- **Onboarding:**
  - one verb per prompt, as glyph + verb ("[SHIFT] BOOST"), at the moment it's useful;
  - dismissed on the first use, shown again after 60 s unused, never pausing;
  - the sentence toasts are deleted.
- `u-small2-n16`'s three ready commits (the focus label, disabled buttons, the pad's keys) are folded in here and tried
  on a build.
- **Done when:** the UI pass captures are reviewed each round, rubric rows 8-10 ≥ 4 by round 4, and the smoke's menu and
  pad steps pass on the round's build (re-run once if the machine is in use).

### U3-30.9 The rounds (lead)
- **Round 1:** build 1 by 2:45. Round 2: by 4:30. Round 3: by 6:00. Round 4: by 7:15.
- Each round:
  - Build, then run the smoke (every build).
  - Run the reel (about 25 captures), then score the rubric.
  - The reviewer agent gets the captures and the checklist, nothing else, and returns its scores and its 5 worst
    problems.
  - Send fix lists to the builders (lowest rows first) and integrate.
- **Done when:** four rounds are logged with their scores, the reviewer's lists, and what changed.

### U3-30.10 Beyond the ask (from round 2; the lead picks from this list and adds to it)
- **The world answers the rank:** the neon trims brighten and pulse faster as the rank climbs (the cyan ladder; no S+
  hue shift, D-70).
- **A rank-up moment:** a full-width streak along the slant, the letter slam, a 4-6 px HUD shake.
- **The round's end:** the tally of the round's kills and best moment slides in for 1.5 s; the next round's number
  slams in.
- **Low HP:** a slow pulse in the vignette, with no new hue.
- **Kill streaks:** "DOUBLE" / "TRIPLE" / "MASSACRE" callouts in the style feed (named lines only; the style scoring
  stays the web's).
- **Pickups:** pulled toward the player in the last metre with a flash on collection (view only).
- **Menus' backdrop:** the world blurred or dimmed behind an INK panel along the slant, never the bright world through a
  translucent box.
- **A RESULTS card worth a screenshot:** the grade, the route and the best moment in one framed card.
- **Enemy spawns:** a short telegraph flash where the beam lands.
- Anything a round's reviewer finds that a player would feel.
- **Done when:** each taken entry has a commit, a capture and a two-line review; each left has a reason.

### U3-30.11 Close (lead, from 8:00; hard stop 9:00)
- **The final build:**
  - the smoke (re-run once if the machine is in use; the idle time logged);
  - `-skygrindGolden` 0 mismatches; `-skygrindFingerprint` EQUAL to tonight's baseline; `-skygrindReplay` EQUAL;
  - the suite; the Silent series if on AC, idle and on the Silent plan;
  - the final reel, scored (round 5).
- **The docs:**
  - tasks.json, `NIGHT17-LOG.md`;
  - `NIGHT17-REPORT.md` for GG: one page with before / after pairs (n17-before vs final) for kills, the HUD, PAUSED,
    SETTINGS and RESULTS; the rubric trend; the questions;
  - `FEEL-PLAYTEST.md` rewritten for tonight's A/B (`Builds/n16-before/` vs `Builds/Stage1/`);
  - PORT-STATUS; `IDENTITY.md`;
  - HANDOFF rewritten (the `handoff` skill, anchor `u3-n17`);
  - `NIGHT18.md` renumbered (map night 2: U3-31, `u3-n18`, N18-STATUS).
- Tag `u3-n17`; the close table; then `NIGHT17 CLOSED`.

## 4. The clock (from the first `date`)

| When | Lead | Builders |
| --- | --- | --- |
| 0:00-0:20 | U3-30.1 (the before reel, round 0) | none |
| 0:20-0:50 | U3-30.3 the shared commit; launch M, X, V, H, N | none |
| 0:50-1:30 | U3-30.2 the identity (fonts, the tile, IDENTITY.md, tokens) | first passes against today's tokens |
| 1:30-2:45 | integrate (red, then green); build 1; round 1 | landing |
| 2:45-4:30 | round 1's fixes; build 2 by 4:30; round 2 | round 1 fix lists |
| 4:30-6:00 | round 2's fixes + U3-30.10; build 3; round 3 | round 2 fix lists, beyond entries |
| 6:00-7:15 | round 3's fixes; build 4; round 4 | round 3 fix lists |
| 7:15-8:00 | the last fixes, the docs' drafts | the last fixes |
| 8:00-9:00 | U3-30.11 the close (build 5 or 6) | none |

**The cut order if short:**
1. U3-30.10's depth.
2. MARKS.
3. The settings sliders and mouse hover.
4. The weapon silhouettes.
5. The destructible blocks before death.
6. The 4th round.

**Never cut:** the identity applied to the HUD and the menus, crunch items 1-3, the stamina rules and their gates, the
4-item PAUSED, the deleted sentence toasts, the close.

## 5. The close table (print it, then `NIGHT17 CLOSED`)

| Item | DONE / SHORT | Evidence ("done when", from this session) |
| --- | --- | --- |
| U3-30.1 Start | | |
| U3-30.2 The identity | | |
| U3-30.3 The shared commit | | |
| U3-30.4 One bar, a sprint, a partial boost | | |
| U3-30.5 Crunch | | |
| U3-30.6 The hands | | |
| U3-30.7 The HUD rebuilt | | |
| U3-30.8 Menus and words | | |
| U3-30.9 The rounds | | |
| U3-30.10 Beyond the ask | | |
| U3-30.11 Close | | |

## GG's standing instruction (verbatim, from NIGHT8)

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

## Suggested `/goal` for GG

> Run NIGHT17.md in C:\Users\gaige\dev\SKYGRIND-unity. Run `date` first, then read CLAUDE.md, NIGHT17.md and the files it
> lists, and work U3-30.1 .. U3-30.11 in NIGHT17.md's order with at most five background builders split by file
> ownership. Never ask me questions and don't stop early; end every message with the N17-STATUS line. Never push, merge
> or commit to master; the web repo is read only. Run at least 8 hours: don't start the close before 8 hours from the
> first `date`; hard cap 9 hours. This goal is met when your latest message prints NIGHT17.md's close table with every
> item U3-30.1 .. U3-30.11 marked DONE (with its "done when" evidence from this session) or SHORT (with its measured
> reason: a cap reached, a decision reserved for GG, a blocker after two approaches, or the time limit once a `date`
> output reads 8 hours or more after the first `date` output), followed by the line "NIGHT17 CLOSED". SHORT rows count as
> met.
