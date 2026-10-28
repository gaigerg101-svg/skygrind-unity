# NIGHT64 builder D: C4 (THE EXIT THAT READS DANGER), then B3

## C4: branch n64-C4, cut from port/stage3 d86ba168 (the lead's A1)

### Commits, in order (cherry-pick in this order, RED first)
1. `6eda5e74` **RED.** N64GoalTests (8 tests) and a placeholder Goal64Math that answers nothing: 7 of 8 are expected to fail in the
   Editor (the goal colour, the table read while colours63 is off, the split, the switches, the live read, the proof line, the
   Route47View source pin); the one that passes pins "no table, no route: unchanged". The door meter `Tools/night/n64-c4-door.py`.
   The commit message holds the contract (the proof command and the output expected with the row off and on).
2. `3b1f5665` **GREEN.** Goal64Math.N64D.cs filled; Route47View.cs splits the column's colour from the shafts'.
3. `b3222621` **The meter: the TV fringe printed apart** (before any GATE frame; see Risks 1).
4. This notes commit.

### The red (measured first, read only, on the lead's red set on rc0)
`python -I Tools/night/n64-c4-door.py red Logs/feel/n64-red/cr/a157` (Builds/n64-red = n63-freeze; a157 n52-spire -skygrindClearShots60):

| shot | door | box px | DANGER px in the box |
|---|---|---|---|
| shot-N29-L57-n52-spire-v2-clear.png | d2 | 12,992 | 184 |
| shot-N29-L57-n52-spire-v5-clear.png | d5 | 20,300 | 178 |
| shot-N29-L57-n52-spire-v6-clear.png | d6 | 13,452 | 101 |
| **all** | | | **463** |

DANGER = S >= 0.55, V >= 0.50 (n63-c1-meter.py's vivid floor), hue 345-35 (colours63's danger band). On v2 the line runs from the frame's
top (y 0) down to the reticle; inside the box (x 904-1016, y 463-579) it is the short stretch above the "29 m" marker. Read on the pixels:
row y 470 is (222, 26, 11) x 954-956, (246, 139, 12) x 957-958, (213, 205, 1) x 959-960: the red-gold line, split red / green by the CRT.

### What changed
- **Goal64Math.N64D.cs** (new): the row `goal64`, `-skygrindNoGoal64`, night 64; `Off` (-skygrindOff64 or older, or its own switch, any
  case); `Live` (ViewOn and the switches, D1's reel override first through NightRows.Live); `GoalColour` (the colour table's route
  `goal47` -> its role, `Colours63Math.ToRole` at the red-gold's own luminance; false and unchanged with no table or no route); `Column`
  (the split); `LiveLine` (the proof line).
- **Route47View.cs**: line 58's one colour stays the window shafts' (`Colours63.Route("goal47", ...)`, untouched: the red-gold while
  colours63 is off; GG's call, DECIDE64). The column (one `_BaseColor` per draw feeds both Column47.shader passes, the through-wall one
  too) takes `Goal64Column`: the table read once per config with `Colours63Math.From(cfg)` (never `Colours63.Route`), the row read every
  frame.
- The colour is the table's: #FF8A3A -> (0.633, 0.592, 0.803), hue 252, S 0.26, V 0.80 (the boundary role, from barrier.boundaryColor
  #C9BCFF). No number typed, no new tuning. View only: no sim read, no tape bit.

### Checks
- dn-check: 0 errors on 6eda5e74 (RED) and on 3b1f5665 (GREEN); the scratch deleted.
- `bash Tools/night/n64-gates.sh --no-editor` on 3b1f5665: **gates: all GREEN 11:09:35** (config copies identical; golden 0 failing,
  0 mismatches, 22 / 22 replays exact; fingerprints plain and --off57 .. --off64 EQUAL).
- `python -I Tools/night/n64-c4-door.py planted`: **C4 DOOR METER PLANTED GREEN** (11 cases).
- Not checked: N64GoalTests in the Editor (RED and GREEN are the lead's run: expected 7 fail / 1 pass on 6eda5e74, 8 / 8 on 3b1f5665).
  I did not run them outside Unity: they touch Look and Colours63, which need the engine.

### Player steps (the lead's)
- **Build rows:** `node Tools/night/n46-build-rows.cjs <tag> on n64.goal64` (plus the build's other rows).
- **Reel sides** (lowercase): `bash Tools/night/n64-reels.sh <tag> c4,n52-spire,a157,-skygrindClearShots60,-skygrindSameRun63,n64.goal64`
  (the same-run pair: ON frames in `Logs/feel/<tag>/c4-on/a157/`, OFF in `c4-off/a157/`, the reel's own frame and the sidecars in
  `c4/a157/`). verbs37 x1: `vp,n52-spire,verbs37`.
- **SPOT pose:** `shot-N29-L57-n52-spire-v2-clear.png` (door d2), the c4-on / c4-off pair. Why a player notices it there: the line is the
  brightest thing on the frame, a vertical stroke from the top of the screen down through the door to the reticle; with the row on it turns
  pale violet-white, and the short red-gold bar over the door stays.
- **The meter with its planted case:** `python -I Tools/night/n64-c4-door.py planted` (GREEN), then
  `python -I Tools/night/n64-c4-door.py pair Logs/feel/<tag>/c4-on/a157 Logs/feel/<tag>/c4-off/a157`. It reads the door box from the
  sidecar beside the reel's own frame (`c4/a157`). Expected: each box with the line, `colDangerOff` > 0, `colDangerCoreOn` 0, `colCoreOn`
  and `colPaleOn` > 0; the summary prints "C4 STRICT ..." and "C4 CORE ...".
- **The proof line to grep:** `grep "\[goal64\] column live" Logs/player/n47-reel-<tag>-c4.log`. Expected: one "live on: role boundary |
  column #A197CD (from #FF8A3A) | shafts #FF8A3A | column built yes" line at the map's first frame (#A197CD is (0.633, 0.592, 0.803)
  before the column's gain; computed by hand, not run), then "live off" and "live on" once per same-run shot (the held off frame).

### Risks and what I could not check
1. **The TV fringe (likely STRICT FAIL).** Crt29's flat split (`UnityTuning.Look35FlatChromaPx` = one virtual px across the whole screen)
   gives every bright line a red-only left edge and a blue-only right edge. On NIGHT63's colours63 spire pair (c1vp vs c1xp, n63-b4, two
   runs, JPEG: a preview, not evidence) the pale column's left edge inside the door reads (128, 72, 39): hue 16, S 0.70, V 0.50, a DANGER
   px. The meter therefore prints STRICT (every changed DANGER px) and CORE (the DANGER px not within 4 px of the line's pale core). On the
   preview: the column's DANGER on 1,069 px, 1,015 of them fringe. If the GATE's STRICT is not 0 but CORE is, the fringe is the screen's
   split of a pale line (every white HUD line has it), not the column's colour: the critic decides; I did not change the meter's floors.
2. **"Its pixels in the boundary band > 0".** Over the cyan door, the pale violet column adds up to a pale cyan-white (the preview's core
   hue 141-191; BOUNDARY 28 px vs PALE 4,535). The meter counts PALE as "still drawn" and prints BOUNDARY on its own; if the critic reads
   the doneWhen as the boundary band itself, the count may be small. Above the door (the dark ceiling, outside the box) it should read
   violet-white: not measured.
3. The column's pulse runs on Time.time (unchanged, NIGHT47's): in the held frame it moves the shafts' brightness by under 1 % (0.4 Hz),
   under the meters' 24-level diff. Not measured.
4. With colours63 also on, the column and the shafts both take the boundary (NIGHT63 C1's behaviour, unchanged): goal64 only matters
   while colours63 is off.
5. Process (G's note): G's dn-check and mine both used `scratchpad/dn` at about 11:08-11:10. My C4 GREEN dn-check ran there in that
   window (it printed 0 errors). C4's Editor compile on port/stage3 is 0 errors (the lead, 11:14:54), so the code compiles. From B3 on I
   use `scratchpad/dn-d64`.

## B3: branch n64-B3, cut from port/stage3 a19ce296 (C4 and U1 picked)

### Commits, in order (RED first)
1. `d5f558a2` **The meter.** `n63-b3-roles.py --hue <x0,y0,x1,y1> <on> <off>`, and rule (c)'s floor fixed. B3 ROLES PLANTED GREEN, 16 cases.
2. `e976a847` **RED.** N64WeakAimTests (6 tests) against a placeholder WeakAim64Math, with no table in config yet. Expected in the Editor:
   4 fail (the table, the aim, the input gate, the sites) and 2 pass (RimBoss null with no fight; grade54's numbers). It also adds
   `BossGrade54.RimBoss` (RimAt's own gate as an accessor). No NIGHT63 pinned assertion changed.
3. `d812e2e3` **GREEN, round 1.** The weak aim in config, WeakAim64Math filled, the two weak paths through it, and the punch window's
   head glow routed.
4. This notes commit.

### The red, reproduced (the slate's 158 +/- 2)
`python -I Tools/night/n63-b3-roles.py --hue 795,376,1121,684 Logs/feel/n63-b11/b3k-on/boss63/shot-N30-B63-wyrm-stagger.png
Logs/feel/n63-b11/b3k-off/boss63/shot-N30-B63-wyrm-stagger.png`
-> "HUE median 158.5 (p10 154.6, p90 162.0) over 88453 changed vivid px (of 97659 changed in the rect), median RGB [8, 238, 157], V 0.93 |
OFF median 88.0 RGB [173, 222, 130] | the slate's band 175-200: OUT (+16.5 deg to 175)". The rect is NIGHT63's GATE blob (x 795-1120,
y 376-683), x1 / y1 exclusive.

### Rule (c), fixed
The floor for "a weak point in two colours" is now (a)'s own, max(300 px, 2 % of the whole change). It was 5 % of the non-danger px.
- **Planted:** the crown's pale-pink streak edges (rose 300 + rival 300 px scattered along a 60,000 px amber -> red change) now PASS. The
  old rule failed it.
- **On b11's real pairs:** the crown's tell and windup now read (c) [], so b3p is BOSS PASS (it was FAIL on (c) alone). The wyrm's stagger
  still FAILs (a) and (c) [player, green]: its two colours are real (36,491 + 27,240 of 187,052 px).

### Why the head read 158.5, and the aim
- **What I read in the code and on the frames:**
  - The project grades in LDR: PC_RPAsset `m_ColorGradingMode: 0`, and the boss fight's tonemap is Neutral (PostFx.cs:422, look33). So
    URP tonemaps and clips each channel at 1 BEFORE the LUT applies THE WYRM's colour filter.
  - The filter is Push([0.95, 0.96, 0.78], white, intensity 1.8) = (0.91, 0.93, 0.60), linear about (0.81, 0.85, 0.33).
  - The proof line on b11: `wyrm.open weak #FFD23A x2.2 -> #0854FF x6.8`, `wyrm.mark ... -> #0854FF x3.6`. RimAt's input already had hue
    222. Its green (2.24 x 6.8 in the colour) clips at 1 after the tonemap, and the filter then leaves green (0.85) over blue (0.33).
  - The screen's blue in this fight tops out near 157: the red's own blue is 157.
- **The aim (code-scout option 1).** The weak point's own colour, in the row's table: `weakAim.wyrm = { aim [0, 0.3, 1], scale 0.8 }`, drawn
  as aim x (the part's brightest channel x scale). The lit head becomes #004DFF x1.76, its shell #004DFF x (0.8 x its glow). Green stays
  under the clip; blue reaches the ceiling.
- **The model (scratch, not in the repo).** URP's LDR path: post exposure, Neutral tonemap and clip, LogC contrast, the filter, split
  toning in gamma with SoftLight, then saturation, phase 1's numbers. On the red's three measured heads it reads:

  | head | measured | model |
  |---|---|---|
  | b11 ON | hue 158.5 | 151 |
  | b7 ON | 94 | 102 |
  | OFF | 88 | 86 |

  Its blue reads 25-50 levels low. For this aim it reads hue 185-189 over the shell's pulse, (0, 110-125, 129-136). A first pick of
  aim 0.22 / scale 0.9 would have read 202.
- **The sensitivity (model):** +0.2 scale is about -12 deg (greener); -0.2 is about +12 deg. Green / blue +0.04 is about -6 deg.

### Checks
- dn-check: 0 errors on e976a847 (RED) and on d812e2e3 (GREEN), in `scratchpad/dn-d64`; deleted after.
- `bash Tools/night/n64-gates.sh --no-editor` on d812e2e3: **gates: all GREEN 11:27:28** (config copies identical; golden 0 failing,
  0 mismatches, 22 / 22 replays exact; fingerprints plain and --off57 .. --off64 EQUAL).
- `python -I Tools/night/n63-b3-roles.py --selftest`: B3 ROLES PLANTED GREEN.
- Not checked: N64WeakAimTests and N63BossColoursTests in the Editor (the lead's run); any Player frame of this aim.

### Player steps (the lead's; b2)
- **Build rows:** `node Tools/night/n46-build-rows.cjs <tag> on n63.bossColours63` (plus the build's other rows).
- **Reel sides** (lowercase, NIGHT63's b11 set): `bash Tools/night/n64-reels.sh <tag>` with these sides, each `boss63` plus
  `-skygrindSameRun63,n63.bossColours63`:

  | side | map |
  |---|---|
  | b3k | n57-kiln |
  | b3p | n52-spire |
  | b3f | n52-foundry |
  | b3i | n52-iceline |
  | b3s | n52-span |

  The doneWhen needs all five. verbs37 x1: `vk,n57-kiln,verbs37`.
- **SPOT pose:** `boss63/shot-N30-B63-wyrm-stagger.png`, b3k-on vs b3k-off (THE KILN WYRM staggered, the HUD's "OPEN"). The head box is the
  biggest thing at the reticle (187,052 px changed on b11), so a player notices it there. It should go from lime-yellow (row off) to a
  darker cyan (row on), not spring green.
- **The meter:**
  - First `python -I Tools/night/n63-b3-roles.py --selftest`.
  - Then `python -I Tools/night/n63-b3-roles.py --hue 795,376,1121,684 Logs/feel/<tag>/b3k-on/boss63/shot-N30-B63-wyrm-stagger.png
    Logs/feel/<tag>/b3k-off/boss63/shot-N30-B63-wyrm-stagger.png`. Expected: the median in 175-200 ("IN").
  - And `--boss Logs/feel/<tag>/<side>-on/boss63 Logs/feel/<tag>/<side>-off/boss63 <side>` for each of the five sides. Expected: b3k's
    stagger (c) without a green family; b3f / b3s PASS; b3p PASS; b3i unchanged.
- **Proof lines to grep** in `Logs/player/n47-reel-<tag>-b3k.log`:
  - `"[bossColours63] weak aims"`: "wyrm aim #004CFF scale 0.80; every other boss: RimAt".
  - `"wyrm-stagger.png (frame"`: the frame line names `wyrm.open weak #FFD23A x2.2 -> #004CFF x1.8` and `wyrm.mark ... -> #004CFF x..`
    (worked out by hand: aim x 2.2 x 0.8 = x1.76).
  - Why #004CFF: the log rounds 0.3 x 255 = 76.5 to 4C. The config's `_why` and the commit say #004DFF for the same [0, 0.3, 1].

### Risks and what I could not check
1. **A model, not a measurement.** If b2's hue is out of 175-200, the fix round moves `scale` (and, if needed, the aim's green) by the
   measured gap with `node Tools/night/n64-d-b3-rows.cjs --replace`: config only, both copies.
2. **A darker cyan.** The grade's blue cut caps the screen's blue near 157, so a cyan head can be at most about V 0.6: like the Loom's
   (V 0.61, accepted by NIGHT63's round 2), not the wick's 0.94. Its shell around the head may read a dim blue under the vivid floor.
   Only a grade change could make it brighter (GG's call, code-scout option 3).
3. **Phase 1 only.** Phases 2 and 3 cut blue further (0.74, 0.70 in the filter): the stagger shot is in phase 1. Not measured.
4. **Not shown on the stagger shot.** The punch window's head glow (Boss30View.cs:482) is routed but is not on that shot: it shows only
   while the wyrm is punchable. Not measured.

## B3 round 2 (the one fix round): branch n64-B3b, cut from port/stage3 f4d7332f

**Why.** B3's GATE on b2 FAILED (Logs/n64/verdict-B3.md). `--hue` read 192.8, IN, but only on the vivid px. Every saturated changed px of
the head read 217.3: the bright cube read ~190, and the dim translucent box (wyrm.mark) read ~222, navy, over most of the head. The SPOT
said "blue in A", and `--boss` b3k failed (c) with [player, blue]. One aim per boss could not move the box without moving the cube.

### Commits, in order (RED first)
1. `fda7461a` **The meter.**
   - `--hue-all <rect> <on> <off>`: every changed px at S, V >= 0.30, the critic's floor.
   - Every hue line now prints its share over 200 and under 175.
   - 2 planted cases (a cyan cube in a navy box; the same cube in a cyan box). B3 ROLES PLANTED GREEN, 18 cases.
2. `c083cb4f` **RED.** 2 new tests (the box's own aim; For). 2 of tonight's own pins restated, `Weak(Route(role, c))` ->
   `Weak(part, Route(role, c))`, reason in the commit. 3 of 8 expected to fail in the Editor.
3. `a9b57ec4` **GREEN.** dn-check 0 errors (scratchpad/dn-d64, deleted after); `n64-gates.sh --no-editor`: gates all GREEN 11:54:35.
   - `WeakAim64Math`: `Aim.For(part)` / `SetPart` / `PartNames`, and `From` reads `weakAim.<boss>.parts`.
   - `BossColours63`: `Weak(part, ...)`.
   - Config by `node Tools/night/n64-d-b3-rows.cjs --replace`:
     - The cube (wyrm.open, and every weak part with no entry): `wyrm.scale` 0.8 -> 0.6.
     - The box: `wyrm.parts["wyrm.mark"] = {aim [0, 0.42, 1], scale 1.0}`.

### b2 measured with the new meter (`--hue-all`, Logs/feel/n64-b2/b3k-on|off/boss63/shot-N30-B63-wyrm-stagger.png)
| region | rect | median | over 200 | RGB | V |
|---|---|---|---|---|---|
| the head | 795,376,1121,684 | 218.0 | 62.9 % | (48, 83, 131) | 0.51 |
| the cube | 870,480,1061,641 | 192.5 | 12.3 % | (14, 138, 165) | 0.65 |
| the box's top strip | 815,380,1116,431 | 220.3 | 100 % | (49, 67, 115) | 0.45 |

### The aim (a model, calibrated on b2)
- On b2's drawn colours the model reads the cube at 188.3 (measured 192.5) and the box at 207.5 (measured 220.3). So it is about 4 deg
  green on the cube and about 13 deg green on the dim box.
- **The box:** green / blue 0.42 at x1.0 of the shell (x1.18 at glow 1.2; round 1 drew x0.94). The model reads 178, so about 191-193 by
  b2's offset, and brighter.
- **The cube:** x0.6 of the lit head (x1.32; round 1 drew x1.76), so the brighter box over it does not turn it green. The model reads cube
  + box at 179.5, so about 183 by b2's offset.
- **Sensitivity (model):** the box's green / blue +0.04 is about -6 deg; the cube's scale +0.2 is about -12 deg.

### Contract and Player steps (same as round 1's; the expected numbers)
- **b3k stagger pair, row on:**
  - `--hue-all` on the cube rect: 175-200.
  - `--hue-all` on the box strip: 175-200.
  - `--hue-all` on the head rect: 175-200.
  - `--hue` on the head: 175-200.
  - `--boss` b3k: the stagger's (c) families [player] only.
- **The other four sides:** unchanged (no aim).
- **Proof line:** "[bossColours63] weak aims: wyrm aim #004CFF scale 0.60 (wyrm.mark aim #006BFF scale 1.00); every other boss: RimAt". The
  hex is worked out by hand: 0.42 x 255 = 107.1 -> 6B.
- **Not checked:** the Editor tests and any Player frame. The box's offset (+13 deg) comes from one measured frame.
