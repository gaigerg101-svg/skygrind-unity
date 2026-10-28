# Builder L (LIGHT & SCREEN) ledger, NIGHT72

Worktree `.claude/worktrees/n72-L`, branch `n72-L` from t0b b80afc2fa. t0 = 2026-10-27 10:19:20 MST (the lead's clock). Every number below
names its command and file; anything not run in a Player is "not checked in a Player". No Editor, no build, no Player was run by L.

## Status at 12:15 (t0 + 1:56), by slice (shas on branch n72-L; the lead's picks are named in the lead's STATE.md)
| slice | state | what is proven, where | what the Player must still show |
|---|---|---|---|
| L1 flicker meter | built, offline-proven | self-test PASS (calm / grain / slow fade GREEN, planted 32x32 and a z-fight strip RED), C# encoder -> Python decoder identical | still72 in a Player; the planted run RED, the plain run on the same poses |
| L3 lit72 | built, OFF, sized on existing frames | 12 forge frames: dark 9-48 % -> 1.7-8.9 %, hue shift <= 6.8 deg, blown unchanged; the other three bosses (polish4 frames): THE WICK 2-3 %, THE LOOM 15-21 %, the iceline boss 10-23 % (SHORT: near-black arenas) | the boss63 and tour65 pairs, the SPOT; the shader imports clean; `-skygrindLit72Probe=floor:0.25` on THE LOOM and the iceline boss |
| L4 calm72 | built, OFF | the three red-border sources named with file:line; n72-calm.py self-test PASS | the boss63 pair and the planted 100 / 45 / 25 % health frames |
| L6 fixture61 | dims widened (RimW 0.22 ...); offline meter: unmounted 0 on all nine levels, on my base and on port/stage3 (foundry 9 lights after N0b) | `l6-fixture61-meter.txt`, `l6-fixture61-stage3.txt` | the SPOT on the tour65 pair (the dark housing against a brown ceiling) |
| L2 fix72 | built, OFF, NOT RANKED | the sources named from the code (film grain -> the quantiser, CRT grain in linear light, bloom HQ), a diagnostic switch per source, the rank script (self-test PASS) | the ranking run and the 50 % bar |
| L5 grow plan | done offline, drawn geometry only | pairs left after the shipped growth 26 / 10 / 16 / 10 / 20 -> 0 on all nine levels; node and C# agree | `[grow72]` lines in a Player log; a still cannot show shimmer |
| leftover wash71 | override built (`-skygrindWash71PopK`), the config flip is the lead's | | the a153 reel on popK 1 / 0.8 / 0.6 |
| leftover banding (NIGHT72.md section 7, item 7) | not pursued | the final image is quantised to 32 levels per channel by the PS2 look (Crt29.shader, Look35Levels), so HDR precision, LUT size and dithering upstream of it cannot show as bands: the bands on a gradient are the look, and the flicker of their edges is what fix72 addresses | |

## L1 the flicker meter (still72 reel phase, Tools/night/n72-flicker.py, a planted case)

Built (this commit): `Assets/Scripts/Game/Still72.cs` (the pure stack file: integer luma, delta, gzip), `Assets/Scripts/Game/FeelReel.N72.L.cs` (the
`still72` phase), `Assets/Scripts/Game/UnityTuning.N72L.cs` (its numbers), `Assets/Tests/EditMode/Game/Still72Tests.cs` (5 tests, for the lead to
run: `bash Tools/unity-loop.sh tests Still72`), `Tools/night/n72-flicker.py`.

What it does:
- still72 tours a campaign level as tour65 does (the same rooms and card poses, Tour65.Poses), and at each pose holds the camera 30 settle frames, then
  keeps 60 rendered frames on the fixed 60 fps FeelClock with the enemies frozen (husks.frozen), writing the luma of each into ONE stack file per pose
  `still72-T72-<level>-<room>-<pose>.s72` plus the first frame as `shot-N72-T72-<level>-<room>-<pose>.png`. No JPG strip is recorded.
- n72-flicker.py prints per pose: the flip share (pixels whose luma reverses direction 3+ times, each swing MORE than 6 levels), the p99.9 and p99.99
  of |dY|, the max and mean step, the three biggest blobs with boxes, a heatmap PNG (`--out`), and GREEN / RED against the bar.
- THE BAR, written before any measurement and never loosened after one: RED at or over **0.01 % of the frame** flipping (207 px at 1920x1080, about a
  14 x 14 patch). The planted block (32 x 32 = 1024 px = 0.0494 %) is 4.9 times the bar. A pose with grain over the swing will read RED; fix72's target
  is the slate's >= 50 % fall of the share, not GREEN everywhere.
- The planted case: `-skygrindFlickerPlant72` adds a reel-only overlay block (white / black swapped every rendered frame at 160,540, 32 px). It proves
  the chain capture -> stack -> meter sees a flicker; it is not a render-path test (an overlay canvas is drawn after the post stack).

Evidence (all run in this worktree, 2026-10-27 10:47-10:48 MST, base b80afc2fa):
- `evidence/l1-selftest.txt`: `python -I Tools/night/n72-flicker.py --selftest`: PASS. calm GREEN (0 px); +-3 level grain GREEN (0 px); a slow fade GREEN
  (0 px); the planted 32x32 block RED (1024 px, 0.0494 %, 1024 / 1024 inside the plant rect); a 300x4 px z-fight strip RED (1200 px); the stack file
  round trip.
- `evidence/l1-cs-to-py.txt` (+ `evidence/s72check/`): the C# encoder (Still72.cs, linked into a scratch console) wrote a 640x360 stack with a 16 px
  block swapped every frame; Still72.Read read it back identical; n72-flicker.py read the same file: 256 of 256 px in the rect flip, RED. So the row
  order (top-down) and the delta coding agree across C# and Python.
- `bash Tools/night/dn-check-wt.sh .../L/dn`: 0 errors on the four projects (Game, Game.Tests, Core.Tests, Editor), 2026-10-27 10:46.

NOT checked (needs a Player; the lead runs it):
- the still72 phase itself in a Player (the Texture2D format a screen capture returns here, the wall time per pose, the file sizes, the planted
  block showing in the capture).
- the EditMode tests Still72Tests (written, compiled, not run).

(Follow-up in the L3 commit: the still72 reel's own GREEN no longer needs every room's wave to have come up; a room with no wave is counted in the line.)

Reel commands for the lead (one Player per level, L-062; `s1p` is the planted control on the same level):
```
bash Tools/night/n72-reels.sh <tag> s1,n52-spire,still72 s1p,n52-spire,still72,-skygrindFlickerPlant72
python -I Tools/night/n72-flicker.py Logs/feel/<tag>/s1 --out Logs/n72/flicker/<tag>/s1 --json Logs/n72/flicker/<tag>/s1.json
python -I Tools/night/n72-flicker.py Logs/feel/<tag>/s1p --out Logs/n72/flicker/<tag>/s1p
```
Read in the Player log (Logs/player/n47-reel-<tag>-s1.log): `[reel] still72: input: 60 frames per pose, settle 30, enemies frozen, plant off|ON ...`, then
one `[reel] still72: <room> pose <name> ... frames 60/60 ...` per pose and `done: GREEN still72`. In the planted run the meter's line for each pose
must end `plant rect 1024 of 1024 px flip`, and the same rect in the plain run must read 0 (or a few, if the scene itself flickers there).
Each stack is 2 MB (calm) to tens of MB (noisy); add `--delete-raw` to remove a stack once it is measured.

## L3 lit72: the boss arenas and level 5 lifted inside each boss's own hue (row design.n72.lit72, OFF in the repo)

Why a post lift and not more ambient: the n71-forge72 frames already carry Bright71's ambient and albedo floors (Bright71 is ON by default), and the
wyrm's walls are R 50 / G 11 / B 6 of 255 (`grid.py` on wyrm-idle: the top three rows' cells read luma 22-43, 63-95 % dark): in a red room luma
(0.299 R) reaches 30 only at R ~ 100, a 2x gain no ambient floor gives without lighting the whole room. A display-space shadow lift is the one change
whose result can be sized on the existing frames, so I sized it (Tools/night/n72-lit-sim.py) and built exactly that.

Built: `Assets/Scripts/Game/Lit72.cs` (Lit72Math: gain, target, fade; Lit72: the live state, ticked in Crt29Driver.Update, `-skygrindLit72Tune=A,l0,l1,kmax`
to retune without a rebuild), `Assets/Shaders/Resources/Crt29.shader` (the `_Lit72` block, after the scanlines, before the quantise and the grain),
`Assets/Scripts/Game/Render/Crt29Pass.cs` (sets `_Lit72`; a live lift keeps the pass on with the CRT at 0, as juice38 does), `UnityTuning.N72L.cs` (A 0.12,
l0 0.12, l1 0.42, kmax 9 (4.5 in the first commit; see "Update" below), fades 0.8 s, spire theme 4), `Assets/Tests/EditMode/Game/Lit72Tests.cs` (6 tests, for the lead: `bash Tools/unity-loop.sh tests Lit72`).
The row is read with `NightRows.ViewOn(g.cfg, "lit72")` every frame; the weight (boss room, a spire level's rooms, or a live boss grade, faded 0.8 s) is
computed every frame whether or not the row is on. Hue: the colour is scaled by one gain per pixel, so each arena keeps its palette.

Evidence `evidence/lit72-sim.txt` (+ `evidence/lit72-sim/*.pair.jpg`, before | after), the 12 existing frames of build n71-polish8, 2026-10-27 11:02:
| frame set | dark < 30 before | after (HUD and gun boxes left unlifted) | worst hue shift | blown > 235 |
|---|---|---|---|---|
| THE KILN WYRM (4 frames) | 9.0-47.9 % | 1.7-7.5 % | 6.6 deg | 0.00-0.15 %, unchanged |
| THE CROWN (5 frames) | 23.1-43.9 % | 4.5-8.9 % | 6.8 deg | 0.26-0.64 %, unchanged |
| spire tour poses v2 v5 v6 | 23.5-30.8 % | 3.5-6.2 % | 2.9 deg | 0.03-0.25 %, unchanged |
Bars from the slate: dark <= 15 % on every wyrm and crown frame (worst model 8.9 %), <= 12 % on the spire's poses (worst 6.2 %), hue within 20 deg (worst
6.8), blown <= 1 % (worst 0.64 %). Unmasked (the whole frame lifted, as the pass really does outside the HUD and the gun): worst dark 3.2 %.
(The first commit's table, kmax 4.5: 1.7-8.5 / 4.6-10.7 / 3.7-6.5 %, worst 10.7 %; kmax 9 changes these by under 2 points.)

UPDATE (the other three bosses, the NIGHT72 leftover 1; `evidence/lit72-sim-other-bosses.txt`, frames Logs/feel/n71-polish4, Bright71 ON, build n71-polish4):
| boss (map) | dark < 30 before | after the lift (kmax 9) | worst hue shift | blown > 235 |
|---|---|---|---|---|
| THE WICK (n52-foundry) | 4.0-6.4 % | 2.0-2.8 % | 25.9 deg (idle; 17 on stagger) | 0.27-0.77 %, unchanged |
| the iceline boss, brawl40 (n52-iceline) | 32.5-52.1 % | 9.9-23.4 % | 37.2 deg (windup) | 0.45-3.22 %, unchanged (3.22 % already on flinch, before the lift) |
| THE LOOM (n52-span) | 79.2-83.6 % | 15.1-20.9 % | 26.5 deg (stagger) | 0.20-0.31 %, unchanged |
(At kmax 4.5 THE LOOM stayed 29-41 % dark and the iceline boss 14-33 %: that is why the default moved to 9.) THE WICK is not dark. THE LOOM's and the iceline boss's arenas are
mostly near-black blue ((3, 3, 11) at luma under 8 on 20-31 % of the frame): a display lift cannot take a pixel at luma 1 over 30 (k 9 x 1 = 9), so they stay 10-23 % dark with the
lift and over the 15 % bar on idle and windup; they need LIGHT in the arena (the ambient probe: `-skygrindLit72Probe=<k>`, or a per-arena shadow tint added to the lift: a new design),
so I leave them SHORT for B / the lead with these numbers rather than push kmax past 9 (it would amplify the near-black noise ever more).
The physical lever is built and OFF: `-skygrindLit72Probe=floor[:m]` (Lit72.FloorProbe) builds the ambient probe the stone lights with from Bright71's own floors (room 0.42 / 0.32 / 0.22, boss 0.62 / 0.56 / 0.40 by lit72's
weight, hue of the theme's trilight kept, PostFx.Probe33's construction) and blends it in by weight x m. What it would do, by hand: THE LOOM's walls are (3, 3, 11) because probe x albedo is about 0.001 linear
(the theme's equator ambient is a few thousandths); the boss-floor equator is 0.28 linear, about 70x, so m 1 may overshoot (walls near 80 / 255 on THE LOOM, and a brighter wyrm and crown too, whose walls the direct light
already carries): try m 0.25 first (`-skygrindLit72Probe=floor:0.25`) on n52-span and n52-iceline boss63 sides, then 0.5; keep the highest m whose frames hold the dark share <= 15 % and blown <= 1 %. Not measured. The hue shift is over 20 deg on THE WICK's idle
(25.9), the iceline boss's windup (37.2) and THE LOOM's stagger (26.5): the lift raises the dark background (blue / green) over the thin emissive hues that carried the OFF frame's mean hue.
Those frames are older (polish4) than the forge frames (polish8); the arenas may have moved: the lead's boss63 pair on the three maps settles it.
THE LIMITS OF THE MODEL, so the numbers above are a prediction and not a result: it lifts the finished frames (after scanlines, vignette, the quantise and
the grain), while the shader lifts before them, so the vignette makes the real lift up to a quarter of A shy at the corners; the HUD and gun boxes it
leaves out are bigger than the real overlays (conservative the other way); the Player's own pair is the proof. Not checked in a Player: the shader compiles
(the Editor's import of Crt29.shader must show no error: a broken Resources shader turns the whole CRT pass off), the row's pair, the SPOT, verbs37.

For the lead (builds need Crt29.shader imported by the Editor first; read the console for a shader error):
```
bash Tools/night/n72-reels.sh <tag> bk1,n57-kiln,boss63,-skygrindRows72,lit72:1 bk0,n57-kiln,boss63 bp1,n52-spire,boss63,-skygrindRows72,lit72:1 bp0,n52-spire,boss63 tp1,n52-spire,tour65,-skygrindRows72,lit72:1 tp0,n52-spire,tour65
python -I Research/2026-10-27-forge72/sheet.py <out.jpg> Logs/feel/<tag>/bk1/boss63/shot-N30-B63-*.png    (mean / dark<30 / blown>235 per frame, as the forge measured)
python -I Tools/night/n72-lit-sim.py Logs/feel/<tag>/bk0/boss63/shot-N30-B63-*.png --A 0.12   (what the model says of the OFF frames: compare to the ON side's measured numbers)
```
Read in the Player log: `[rows72] lit72=1`, and (with a retune) `[lit72] tune A ...`. A same-run pair is not possible for this row (it is not in
SameRun63Math.FrameRows; adding it is the lead's file), so the pair is two sides of one build and the frames' content differs by the boss's timing
(luma and dark-share numbers are what the bars read). If a side looks washed out: retake it with `-skygrindLit72Tune=0.10,0.12,0.40,4` (the model's milder set: worst
dark 11.3 % on the same 12 frames, mean luma 8-18 above the OFF frames' instead of 10-22). For the other three bosses add sides on n52-foundry, n52-iceline and n52-span:
`bw1,n52-foundry,boss63,-skygrindRows72,lit72:1` / `bb1,n52-iceline,boss63,...` / `bl1,n52-span,boss63,...` and the plain sides.

## L4 calm72: no red border at full health; the low-health edge thin and only under 30 % (row design.n72.calm72, OFF in the repo)

THE SOURCES (read in the code at HEAD b80afc2fa; the slate asked for "the noise border's source named, file:line"). The border in the wyrm's stagger and
windup frames is not one thing. Three HUD Images draw a red frame, none of them tied to low health:
1. **EXPOSED's pulsing edge** `exposedEdge` ("Exposed edge"), `UiFeelEdges.cs:50-86` (built :37-42, written :73-86): while the director calls the player EXPOSED
   (standing still) it draws at alpha 0.35 + 0.35 sin(7 t) at ANY health; its sprite is an inset shadow of blur 90 / spread 30 (UnityTuning.ExposedEdge*), about
   120-150 px deep. The boss63 reel stands still, which is why the wyrm's frames show it at "9999". This matches GG 2026-10-06 "that weird border that only appears".
2. **The hurt flash** `vignette` ("Hurt vignette"), `Ui.cs:1166-1168`: alpha hud.hurtVignetteOpacity 0.6 falling over hud.hurtVignetteTime 0.35 s after EVERY hit,
   at any health; and `Ui.cs:1169-1184` while frac <= health.lowHpFraction its breath (base 0.18 + 0.16 x the beat). Its sprite is UiEdge31's frame (clear 0.86).
3. **The damage vignette** `damageVignette` ("Damage vignette"), `Ui.cs:881-887`: alpha (1 - frac) x regen.vignetteMax 0.5, eased: 0.28 at 45 % health, 0.35 at 31 %
   (GG's two screenshots), 0 at full health. Sprite: UiEdge31's frame (clear 0.88).
The speckle is the CRT grain (Crt29.shader) and Juice38's damage static (Juice38.cs:312-318, riding the same pass) showing on bright red; the frames show grain
all over the screen (crop of wyrm-stagger x 0-240, y 300-420: the wall and the band alike), so a border of bright red is where it reads. The URP vignette
(PostFx.cs, low-HP block) starts under post.lowHpAt 0.45 and is black, not red.

Built: `Assets/Scripts/Game/Calm72.cs` (Calm72Math; Calm72: finds the three Images by name, and with the row ON, from Crt29Driver.LateUpdate after Ui has written
this frame: at or above 30 % health all three are `enabled = false`; under it the two vignettes swap to a thin frame (clear 0.94: 58 px deep at 1920, 32 px at
1080; UiEdge31's alphas) and the EXPOSED edge stays off; with the row OFF they are put back at once), `PostFx.cs` (the `hurt` line: the low-HP vignette starts
under 30 % when the row is ON), `UnityTuning.N72L.cs` (L72CalmEdgeBelow 0.30, L72CalmEdgeClear 0.94, L72CalmProbeMax), `FeelReel.N72.L.cs`
(`-skygrindStill72Health=<fraction>`: still72 holds the player at that share of 100 health at each pose, so the edge draws as in play),
`Tools/night/n72-calm.py` (the meter: the depth in px, from each edge, of the DIFFERENCE between a frame and a reference frame of the same pose; its self-test:
a planted band of 0 / 60 / 200 px measures 0 / 56 / 188, `evidence/l4-calm-selftest.txt`: PASS; it reads about 6 % shy, so a 60 px band can read 56),
`Assets/Tests/EditMode/Game/Calm72Tests.cs` (2 tests: `bash Tools/unity-loop.sh tests Calm72`). I edited no HUD file: Ui.cs, UiFeelEdges.cs and UiEdge31.cs are the
lead's to touch, so the Images are held from outside (by name; a Find every 0.5 s until all three are found). If the lead prefers a hook in Ui.cs, the
three `enabled` lines and the sprite swap move there unchanged.
What it does NOT change: the hit's static (Juice38: GG asked for it in NIGHT38), the shake, the URP vignette's colour (black), the sim (the exposed state, the
hits and the health are untouched: fingerprints EQUAL by construction; the smoke's Player fingerprint is the lead's check).
LOST WITH THE ROW ON (a call for GG, DECIDE72): EXPOSED's red edge cue is gone at every health (its pulse is the flicker GG named); under 30 % the vignettes
are the only red edge. The EXPOSED mechanic itself is unchanged.
Not checked in a Player: all of it (the Images' names and enabled states in the real HUD, the thin frame's look, the PostFx line).

For the lead (planted health, on the spire; the OFF side plain, the ON side with the row; extra args split on commas):
```
bash Tools/night/n72-reels.sh <tag> c100,n52-spire,still72,-skygrindStill72Health=1 c45,n52-spire,still72,-skygrindStill72Health=0.45 c25,n52-spire,still72,-skygrindStill72Health=0.25 n100,n52-spire,still72,-skygrindStill72Health=1,-skygrindRows72,calm72:1 n45,n52-spire,still72,-skygrindStill72Health=0.45,-skygrindRows72,calm72:1 n25,n52-spire,still72,-skygrindStill72Health=0.25,-skygrindRows72,calm72:1
python -I Tools/night/n72-calm.py --ref Logs/feel/<tag>/n100/shot-N72-T72-<level>-<room>-<pose>.png <the same pose's PNG from c100 c45 c25 n45 n25>
```
The reference is the ON frame at 100 % (its probe line says drawn 0 / 0 / 0). Expect: c45 and c25 read a border (the old 135 px frame), n45 depth 0, n25 at most 60.
Read in the Player log: `[calm72] ...` lines (both sides print them: OFF `off | health 45 % | the HUD wrote hurt 0.18 damage 0.28 exposed 0.00 | drawn hurt 0.18 damage 0.28 ...`;
ON `ON | health 45 % | edge allowed no | ... | drawn hurt 0 damage 0 exposed 0`). For the boss pair (the wyrm's stagger and windup): the same boss63 reel as lit72's,
`-skygrindRows72,calm72:1` on the ON side; the border there comes from EXPOSED and the hit flash at full health.

## L2 fix72: the flicker sources, switchable one at a time for the ranking, and the fixes behind the row (row design.n72.fix72, OFF in the repo)

STATUS: BUILT, NOT RANKED. The slate's L2 needs the Player (the meter's numbers on the five levels); I had none. What I did instead is read the pipeline for what a
STILL camera can show, build a switch per candidate so ONE build ranks them, and put the three fixes I can defend from the code behind the row. Every figure below
marked "by hand" is arithmetic from the shader and URP's FilmGrain, not a measurement; the ranking file `flicker-rank.md` does not exist yet and is the lead's run.

The hypotheses (Fix72.cs's header has them with the lines):
1. **URP film grain into the 32-level quantiser** (PostFx.cs, quality.presets.*.grain 0.035 medium / 0.045 high, settings.filmGrain). FilmGrain draws a new random
   offset every frame, before the CRT pass; Crt29.shader then quantises to Look35Levels 32 (one step 8.2 levels). By hand: the grain is +-1.5 levels (URP: input x
   grain x intensity x a luminance response), so about 3 / 8.2 = 37 % of a flat gradient sits within a step edge and swaps by a WHOLE STEP (8.2, over the meter's
   6) frame to frame. fix72: the film grain is off while Crt29.QuantStrength > 0 (the quantiser has its own fixed Bayer dither).
2. **The CRT grain added in linear light** (Crt29.shader, design.n29.crt.grainHz 24, Look35Grain 0.012). By hand: +-0.006 linear, halved in the dark by
   GrainScale30, through the sRGB curve: +-5 levels at linear 0.01 (swing about 10), up to +-10 at linear 0.003, +-1.5 on a bright pixel. So it is a flicker of
   the SHADOWS, at 24 Hz: exactly the dark rooms GG names, and the boss arenas that lit72 lifts (a lifted shadow sits higher on the curve, so lit72 cuts it a
   little; it does not remove it). fix72: the same grain added in display (sRGB) space, +-1.5 levels everywhere (`_Fix72.x` in the shader; `fix72` the row).
3. **Bloom without High Quality Filtering** (PostFx.cs; none set): URP's manual says HQ "reduces flicker"; thin bright strips (the neon rims, the ride strips) are
   what flicker through the downscale chain. fix72: `bloom.highQualityFiltering` on. (This matters in motion more than on a still.)
4. **SSAO's blue noise moves every frame** (Ssao55.cs's finding): the harvest row ssao55 freezes it (the lead flips it as built). Here: the diagnostic only.
5. The quantiser, the scanlines and the chroma split are static on a still camera: they appear in the list so the ranking can say they are not it.
The Stone shader is not on it (spike.md S2: Forward+-correct).

What a still camera does NOT show (said so the lead does not trust a GREEN too far): the crawl of SSAO, bloom on thin strips and scanline moire under a MOVING
camera. still72 holds the view; a moving-camera meter would need motion compensation and is not built.

Built:
- `Assets/Scripts/Game/Fix72.cs`: the diagnostic `-skygrindFlickerOff72=<source>[+<source>...]` (sources: grain, crtgrain, bloom, ssao, quant, scan, chroma), read once, a
  reel's switch only (fix72 does not read it); the still72 phase prints it in its input line.
- `PostFx.cs` (each frame, after the low-HP block): the film grain's `active` = the preset wants it AND not the diagnostic AND not (fix72 AND the quantise live); the
  bloom's `active` follows the diagnostic; HQ filtering follows the row. The preset's own values are untouched (only the switches move), so the first ON frame is the frame it
  would have drawn (D1). `Render/Crt29Pass.cs` + `Resources/Crt29.shader`: `_Fix72.x` (the grain in display space) and the diagnostic's zeroing of scan / chroma / CRT grain / quant.
  `Ssao55.cs`: `Now` honours the diagnostic (the AO off).
- `Tools/night/n72-flicker-rank.py` (self-test PASS: `evidence/l2-rank-selftest.txt`): turns the sides' `n72-flicker.py --json` files into `flicker-rank.md`.
- `Assets/Tests/EditMode/Game/Fix72Tests.cs` (3 tests: `bash Tools/unity-loop.sh tests Fix72`).
- `Lit72.cs` gained an experiment for the lead's retake: `-skygrindLit72Probe=<k>`. FINDING while reading the lighting: Stone.shader lights with `SampleSH` (the AMBIENT PROBE),
  PostFx.Probe33 writes that probe once per theme from the theme's trilight, and PostFx.cs's own note says nothing recomputes it at runtime, so **Bright71's lifts of
  RenderSettings.ambientSky / Equator / GroundColor never reach the stone** in a Player: the boss arenas' ambient is the theme's 0.03-0.12, which is why they stayed dark with
  Bright71 on (what it did lift: the albedo floor, +0.35 EV, the sky and the fog). I did not change Bright71 (it is not measured either way). `-skygrindLit72Probe=2` scales
  the probe by 1 + (k - 1) x lit72's weight (default UnityTuning.L72LitProbeK 1 = untouched): the physical lift, relief kept, not sizeable from the existing frames.
  If the lead has a spare side: boss63 with `-skygrindRows72,lit72:1,-skygrindLit72Probe=1.6` against lit72 alone.

Not checked in a Player: all of it. Not run: the EditMode tests.

For the lead, the RANKING (one build; the spire has 3 poses, run a second level for the worst case: iceline has 8 and the dark rooms are the spire's and the bosses'):
```
bash Tools/night/n72-reels.sh <tag> p0,n52-spire,still72 pg,n52-spire,still72,-skygrindFlickerOff72=grain pc,n52-spire,still72,-skygrindFlickerOff72=crtgrain pb,n52-spire,still72,-skygrindFlickerOff72=bloom ps,n52-spire,still72,-skygrindFlickerOff72=ssao pq,n52-spire,still72,-skygrindFlickerOff72=quant px,n52-spire,still72,-skygrindFlickerOff72=scan+chroma pf,n52-spire,still72,-skygrindRows72,fix72:1 pl,n52-spire,still72,-skygrindRows72,lit72:1 pa,n52-spire,still72,-skygrindRows72,fix72:1+lit72:1+calm72:1
for s in p0 pg pc pb ps pq px pf pl pa; do python -I Tools/night/n72-flicker.py Logs/feel/<tag>/$s --json Logs/n72/flicker/<tag>/$s.json --out Logs/n72/flicker/<tag>/$s --delete-raw; done
python -I Tools/night/n72-flicker-rank.py --base plain=Logs/n72/flicker/<tag>/p0.json grain=.../pg.json crtgrain=.../pc.json bloom=.../pb.json ssao=.../ps.json quant=.../pq.json scan+chroma=.../px.json fix72=.../pf.json lit72=.../pl.json all=.../pa.json --out Research/2026-10-27-wave72/L/flicker-rank.md --header "<the command, the build sha, the time>"
```
(Rows joined with `+` in -skygrindRows72; the reel script splits extra args on commas.) The slate's bar for fix72: each level's worst pose falls by >= 50 % against the plain
side. If it does not, the rank table says which source is left; the diagnostic list shows what it is not. A plain side that reads GREEN everywhere (under 0.01 %) means the
flicker GG sees is a MOVING-camera one, and L2 is SHORT for want of a meter.

## L5 textures overlapping: the coplanar-face finder and the grow plan (no row: drawn geometry; off switch -skygrindNoGrow72)

GG 2026-10-26: "textures overlapping that bug out". Bright71.Grow (NIGHT71, 407cf1874) staggered the drawn stone boxes by index mod 3 (2 / 4 / 6 mm), which separates two
coplanar boxes only when their indices fall in different classes; nothing had counted what was left.

Built (commits 80794e79c the finder, and the plan's commit named in my report):
- `Tools/night/n72-zscan.cjs`: finds every same-facing coplanar overlapping face pair among a level's DRAWN stone boxes (data.boxes in draw order, as SurvivalBuilder
  draws them), counted raw, after the shipped mod-3 growth, and after the plan, per level; `--selftest` (`evidence/l5-zscan-selftest.txt`, PASS: a planted pair is found, a
  0.01 m gap is not, a 2 mm raw gap that a one-class difference would cancel is found where it cancels and kept open by the plan, touching boxes are not a pair).
  A pair is flagged when its gap after growth is under 1 mm and the rectangles overlap by at least 0.01 m2.
- `Assets/Scripts/Game/GrowPlan72.cs`: the same algorithm in C# (a class 0..5 per box, chosen greedily in index order so every coplanar overlapping pair ends at least 4 mm apart, a box with
  no conflict stays at 2 mm); `Bright71.GrowClass`; `SurvivalBuilder.cs` (both box loops) draws by the plan; one `[grow72] <map>: <n> drawn boxes, coplanar overlapping pairs raw <a>,
  after the NIGHT71 growth <b>, after the plan <c>` line per map built. Off: `-skygrindNoGrow72` (index mod 3 returns) or `-skygrindNoBright71` (no growth). Numbers:
  UnityTuning.N72L.cs (L72GrowClasses 6, L72GrowSepGood 4 mm, L72GrowMinArea 0.01 m2; the step is Bright71GrowStep). The DRAWN box grows, never the collider: no level file changes, so
  no flow base moves (the lead asked: this commit touches no `Design/levels` file).
- `Assets/Tests/EditMode/Game/GrowPlan72Tests.cs` (6 tests, for the lead: `bash Tools/unity-loop.sh tests GrowPlan72`), including "on every campaign level file the plan leaves 0".

The numbers (the nine campaign level files; node tool and C# agree on every level's classes and counts, `evidence/l5-cross-check.txt`: GrowPlan72.cs compiled with Skygrind.Core
and run on the files against the node tool's --json):
| level | drawn boxes | pairs raw | left after the shipped mod 3 (what GG sees) | after the plan |
|---|---|---|---|---|
| n52-foundry (this tree's base) | 201 | 75 | 26 | 0 |
| n52-iceline | 192 | 33 | 10 | 0 |
| n57-kiln | 179 | 55 | 16 | 0 |
| n58-span | 180 | 28 | 10 (1 088 m2) | 0 |
| n52-spire | 199 | 65 | 20 | 0 |
| n65-foundry / n67-iceline / n67-kiln / n67-spire (the repo's copies) | 201 / 192 / 179 / 199 | 75 / 33 / 55 / 65 | 26 / 10 / 16 / 20 | 0 |
The raw counts equal the 2026-10-27 scan Logs/n72-fix/zfight-scan.cjs printed (foundry 75, kiln 55, spire 65, iceline 33, span 28). Only two classes are ever used (0 and 2: 2 mm and 6 mm), so the
grow stays small. On port/stage3 e27aa160f (N0b's roofless foundry and the n67 rebuilds in; `evidence/l5-zscan-stage3.txt`): foundry 200 boxes raw 75 / mod 3 27 / plan 0, n65-foundry 194 raw 79 / 24 /
0, n67-iceline 183 raw 35 / 5 / 0, n67-kiln 181 raw 56 / 16 / 0, n67-spire 204 raw 65 / 23 / 0, the rest as above: TOTAL left after the plan 0. The plan is computed at run time per map, so the rooms the lead
regenerates need no new file.
LISTED AS NOT COVERED (the slate: "or each one left is listed with why"): none is left on the stone boxes. NOT scanned: wedges (the 20-degree ramps' slopes against box faces), the neon rims and strips, the ceilings and
walls of RoofView27, Fixtures33's frames and joists, decor lifts (20-80 mm by their own rule), and every other view's own geometry: those can still overlap and this finder cannot see them. Not checked in
a Player: whether the shimmer GG saw is gone on the frame (a still cannot show it: the meter for it is still72 with a moving camera, not built; the `[grow72]` line in the Player log is the proof of input).

## Leftover: wash71 (n71.wash71, f3-f4 +0.255 against the bar +0.25) 

State read from the files (docs/memory/ledger/wave-71.md line 22; Research/2026-10-26-wave71/F/L4/card.md): the a153 reel's T-111 line with wash71 ON read f3-f4 +0.255, the bar +0.25
(Wash49Rise, never moved), shipped +0.425. The row's two numbers are in the config (design.n71.wash71.numbers, the lead's): popFrameClock 1 and popK 1; the builder's card already named
"a second value for the slot: popK 0.6". The warm part of the rise is the kill's pop light (effects.js pop, fx.popIntensity 60, k^2 over 0.12 s: 0.34 of its peak at f3, 50 ms), the rest is
white blocks the killWash cuts already trim, so the one number that moves it is popK.
What I built: `-skygrindWash71PopK=<k>` (0..1) replaces popK for a run (`Wash71.PopKOverride`; Wash71Tests: `Wash71PopKTests`, 1 test) so ONE build tries 0.8 and 0.6 without a config edit; the bar does
not move. Not measured: any rise value for popK under 1 (no Player). My expectation, labelled as one: the pop is about 0.2 of the 0.255 on the foundry box, so popK 0.8 should take 0.255 to about
0.23 and popK 0.6 to about 0.21; the kill still pops on its first frames at 0.6 of the peak.
For the lead (the config flip is yours: design.n71.wash71.numbers.popK in both copies once a side is GREEN):
```
bash Tools/night/n72-reels.sh <tag> w10,n52-foundry,a153,-skygrindRows72,wash71:1 w08,n52-foundry,a153,-skygrindRows72,wash71:1,-skygrindWash71PopK=0.8 w06,n52-foundry,a153,-skygrindRows72,wash71:1,-skygrindWash71PopK=0.6
```
(and the same on n52-iceline). Read the a153 line `T-111 ... ON peak within the bar True` and the number on each side; pick the highest popK that is GREEN on foundry AND iceline.

## L6 fixture61 made right: lights held by a housing and a beam (row design.n61.fixture61, the harvest row; OFF in the repo until the lead's H pair)

What NIGHT62 found (Logs/n62/slate.json P4, SHORT 2026-10-22 22:08): the proof line was right ("[fixture61] mounts on n52-iceline: ceiling lights 10, housed 10,
held by a beam 10, by a wall 0, unmounted 0") and the blind SPOT said NOTHING OBVIOUS: the dark housings sat in a BLACK ceiling (the n62 frame
`Logs/n62/spot/zfkjgn/A.png`: a white panel in black, beams invisible). Builder V's own note (Research/2026-10-22-wave61/V-S4.md) said the same ("whether the
mounts read at all: not checked"). Since then Bright71 and the ceiling colours came ON: the current iceline frames (n71-forge72 `frames/iceline.jpg`) show a brown
ceiling grid under navy beams, so a dark housing now has a background to read against; I could not render it to check.

What I did, and measured offline:
- `evidence/l6-fixture61-meter.txt` (+ `evidence/fix61-meter/`, a scratch console compiling Fixture61Math, LightPlan33 and their numbers from the worktree): the
  proof line's numbers for all NINE campaign levels, with every ceiling light counted (the Player skips panels over a grapple node, so these are the superset):
  n52-foundry 10 / 10 / 10 beam / 0 unmounted; n52-iceline 10 / 10 / 10 / 0; n57-kiln 11 / 11 / 11 / 0; n58-span 8 / 8 / 8 / 0; n52-spire 10 / 10 / 10 / 0; and
  the rebuilt n65-foundry 10, n67-iceline 10, n67-kiln 11, n67-spire 10, all unmounted 0 (TOTAL unmounted 0; with no mounts the same levels read red 8-11 each).
  It reproduces NIGHT62's 10 / 10 / 11 / 8 / 10 on the five old levels exactly, before and after my change. The slate's first doneWhen line (unmounted 0 on the five
  levels) holds offline; the Player's own line is the lead's to read.
- The housing is a third wider and deeper, the hangers thicker (`UnityTuning.N61V4.cs`, numbers only): RimW 0.16 -> 0.22 (a 0.30 m dark border in all, ~9 px at 25 m;
  was ~7), Drop 0.22 -> 0.26, HangerW 0.12 -> 0.16, HangerH 0.10 -> 0.12. All parts stay within Rods40WallClear 0.3 m under the ceiling (housing 0.26, hanger band
  0.10-0.22); N61FixtureTests read these numbers from UnityTuning (none pinned as literals), so the lead's `bash Tools/unity-loop.sh tests N61Fixture` should stay green.
- No new code, no new material, no new row: the row is still design.n61.fixture61 and its off switch -skygrindNoFixture61 (the Player's proof line reads
  `[fixture61] mounts on <level>: ceiling lights <n>, housed <n>, held by a beam <n>, by a wall <n>, unmounted <n>` once per map built, nothing when off).

NOT done, said plainly: whether the pair is visible at 1080p (the second doneWhen line, the SPOT) is not checked; it needs the Player. NOT covered by this row:
wall lamps and rings (gg-proxy #7: the spire's cyan and green rings flat on walls, kiln kb2c and foundry a2 box lamps on bare walls): Fixture61Math mounts CEILING lights
only; a back plate and bracket for wall fixtures would be a new design (Fixtures33's sconces / rings), the next slice if the ceiling pair reads. If the SPOT is
NOTHING OBVIOUS again on a brown or tan ceiling the lever is the housing's value (Fixture61Shade 0.8 in UnityTuning.N61V4.cs), or a lighter bezel: a new piece of geometry.
For the lead: with the harvest flip (as built) the pair is `tour65` n52-iceline g4 / g6 and n52-spire (`-skygrindRows72,fixture61:1` vs plain: fixture61 is a design.n61 row,
Rows72.cs finds it by the digits of its name); read `[fixture61] mounts on` in the ON side's log.
