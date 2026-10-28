# NIGHT57 builder V: the colour budget and the near misses (V1 colour56, V2 brief55, V3 grade55, V4 rim56 / yellow56)

Builder V, worktree `worktree-agent-a9a40dade5705ca70` from the shared commit 2c8e3f6c. No Player, Editor, build or reel was run by V: every
"after" below is either SIMULATED on frames that already exist (said so on its line) or what the code is built to do, "not measured". Every
"before" is measured on NIGHT56's frames in the main repo's `Logs/feel/n56-r5/` (named per line). After every commit:
`dotnet run --project Tools/SimHarness -c Release -- fingerprint` printed `EQUAL to the baseline`; `bash Tools/night/dn-check-wt.sh` printed 0
errors before each code commit. No file under `Assets/Scripts/Core` changed.

The lead (09:5x): P0-calib FAILED, so colour56's MAIN is its measured fallback (NIGHT57.md section 0 rule 5): per level the key or accent at
S >= 0.35 on >= 3 % of the frame, the accents >= 40 deg apart (n56-colour.py). GUARDS unchanged.

## V1 colour56: the world capped first (3d3c5565; guards tool 56b2b241)

**What failed at NIGHT56 (gate-r5b B5):** the world out-saturated the tells on 28 / 36 colour56 ON frames (n56-r5 a356). My reading of the same
frames with n56-colour.py: the offending pixels are NOT only colour56's. The no-colour56 sides fail too (v5foff 6 / 6, v5voff 6 / 6): the jade-lit
greens of the foundry (hue 130-150 at S 1.00), the emerald-lit spire stone (95-106), the kiln's S 1.0 greens at 143-154, the azure iceline walls
(210-230 at 0.85-0.9 over ~45 % of the frame) and the chromatic / bloom fringes of the cyan strips. A per-material fix cannot reach the grade, the
lights and the fringes; so the cap is a pass on the frame.

**Built:** THE WORLD SATURATION CAP rides colour56 (Game/Render/Cap57Pass.cs, Assets/Shaders/Resources/Cap57.shader; numbers.cap). Valve's Left 4
Dead rule (research/design.md lesson 7: "a saturation threshold. Anything below that moves towards gray"). After the CRT, on the main camera
only (the viewmodel and the HUD draw after it), every pixel whose hue is OUTSIDE the role arcs (350-70 danger + parry yellow, 150-200 usable
cyan, 285-335 RIVAL / FUSION; a 6 deg smoothstep inside each edge) and whose sRGB HSV saturation is over the level's cap moves to the cap
toward the grey of its own Rec. 709 luminance. Hue kept and luminance kept: it is never a tint and the greyscale frame does not change (E-3's
check cannot get worse from it). The level's key and accent are colour56's own non-role hues, so they sit under the cap: "the world first,
then the keys under it". Caps: foundry / kiln / span / spire 0.70, iceline 0.65. Row off, or `-skygrindOff57` (NIGHT56's colour56): the pass is
never enqueued (the frame is NIGHT56's bit for bit).

First I wrote the cap keeping V (the max channel); the planted blue showed it lifts a saturated blue's luminance ~4x (iceline's azure walls
+30 %), which could only hurt the greyscale guard. The shipped maths keeps luminance instead (k solves S(L + k (c - L)) = cap).

**Numbers (SIMULATED on n56-r5 a356 ON frames, `python -I Tools/night/n57-v-cap.py simulate ... --cap foundry=0.7,iceline=0.65,kiln=0.7,span=0.7,spire=0.7`):**
| level | world out-saturates the tells (sat FAIL) | key|accent cover median (MAIN fallback) | the cap moved, median of the frame |
|---|---|---|---|
| foundry | 5 / 6 -> 0 / 6 | 9.1 % -> 9.1 % (min 2.6 %) | 2.4 % (max 28 %) |
| iceline (0.65) | 7 / 12 -> 2 / 12 | 9.3 % -> 9.3 % | 16.8 % |
| kiln | 6 / 6 -> 1 / 6 (kb2c entry: tells median S 0.50) | 13.0 % -> 13.0 % | 3.0 % |
| span | 4 / 6 -> 0 / 6 | 16.4 % -> 16.4 % | 2.3 % |
| spire | 6 / 6 -> 0 / 6 | 22.1 % -> 22.1 % | 21.4 % (its lime-olive walls toward grey) |
| total | 28 / 36 -> 3 / 36 | every level >= 3 % | |
Other caps tried (same frames): 0.75 everywhere 28 -> 6; 0.65 -> 3 (moved more); 0.55 -> 1 (iceline moved a median 27 %, spire 37 %).
**MAIN note:** the fallback MAIN (key or accent at S >= 0.35 on >= 3 %) is already met by NIGHT56's ON frames and by the no-colour56 frames too
(V.md 2026-10-20: the keys sit near hues the levels already wear; v5foff cover 6 / 6), so "beats the baseline" must be read as the per-level
cover median ON vs the `-skygrindOff56` side on the same build, not as a pass count. The cap does not move it (S >= 0.35 is under every cap).

**Guards, before (n56-r5, colour56 on without the cap; `python -I Tools/night/n57-v-cvd.py ...`):**
- greyscale (enemies + hazards median luminance >= the world's p75): foundry 3 / 6, iceline 9 / 12, kiln 2 / 6, span 3 / 6, spire 1 / 6 = 18 / 36
  (no-colour56 sides: foundry 2 / 6, spire 1 / 6). Half the frames fail E-3 before tonight; the cap keeps luminance (simulated spire v5: world
  p75 0.287 / 0.411 on both sides).
- deuteranopia (Machado 2009 severity 1.0; cyan vs red-orange class medians in CIELAB): foundry b1 entry 71.5, iceline g7 mid 62.8, kiln kb2 mid
  56.4, span p1 entry 71.3, spire v5 mid 70.6 Delta-E (>= 20): 5 / 5 apart. The cap never touches either class's hues.

**The reel and the checks (the lead):** `a357` (FeelReel.N57.Colour.cs) is a356's run plus a `-nocap` twin per shot (the cap hidden one frame):
- `bash Tools/night/n47-reels.sh <tag> v1on,n52-<level>,a357` (colour56 on in the copy) and `v1off,n52-<level>,a357,-skygrindOff57` (NIGHT56's
  colour56) and, for the MAIN's baseline, `v1none,n52-<level>,a357,-skygrindOff56` (no colour56), levels foundry / iceline / kiln / span / spire.
- `python -I Tools/night/n56-colour.py Logs/feel/<tag>/v1on/a357 --kinds entry,mid`: per shot a `CAP A/B` field (moved % of the frame, S
  median before / after, hue shift, V shift; sat without / with the cap), the per-level cover (MAIN), and the last lines `CAP A/B (NIGHT57 V1
  GUARD ...)` and `V1 GUARD count`.
- `python -I Tools/night/n57-v-cvd.py grey Logs/feel/<tag>/v1on/a357` (and the off folders), `... cvd <4 shots>`.
- Frames for the critic: per level one entry shot and its `-nocap` twin (the eye line: "is either frame a whole-frame tint?"), spire v5 mid
  (where the cap moves most).
**Risks, not checked:** the shader is not compiled by V (no Editor): a compile error leaves the pass off with one warning
("Cap57: shader SKYGRIND/Cap57 not found") or draws nothing; the a357 line says "passes recorded ... 0" then. The CRT's grain after the
quantise is under the cap's pass, so the PS2 colour steps on capped pixels are smoothed (a few % of the frame on most levels).

### r1 (the lead's numbers, Builds/n57-r1, Logs/n57/r1-colour-counts.txt; a357 x 5 levels x v1on / v1off (-skygrindOff57) / v1none (-skygrindOff56))
- Cap57 compiled and ran: a357 GREEN on all 5 levels.
- MAIN (fallback): key|accent >= 3 % on 36 / 36 (v1none 22 / 36); accents' least gap 45 deg. PASS.
- GUARD world out-saturates the tells: v1on 7 / 36 (v1off 32 / 36, v1none 36 / 36). PASS (the simulation said 3 / 36).
- GUARD deuteranopia 4 / 4. PASS.
- GUARD greyscale FAIL: v1none 25 / 35 -> v1on 18 / 36 (v1off 19 / 36). My per-level read of the same folders: foundry 3 -> 2, iceline 10 -> 9,
  kiln 4 -> 3, span 6 -> 2, spire 2 -> 2.
**Why (my read of the r1 A/B, scratch script; world p75 luma on the shot / its -nofrieze twin / the v1none shot of the same room):** the frieze
renders at luma 0.23-0.42 on 2-19 % of the frame over a world p75 of 0.17-0.29 (foundry a3 entry 0.369 / 0.287 / 0.284; spire v6 mid 0.300 /
0.166 / 0.176). On span the frieze is not it (p1 entry 0.344 / 0.342 / 0.297): NIGHT56's neutral walls kept decor54's VALUE, and a neutral at a
violet's value is ~1.65x its luminance (0x797780 luma 0.471 against decor54's 0x46447e 0.285). My earlier claim "the greyscale frame is
unchanged" held for the cap only; colour56's other parts (the frieze, the accents, the walls) were never luminance-checked.
**r2 (8d249940), all off with -skygrindOff57:** the frieze's key scaled to a Rec. 709 luma <= 0.22 (numbers.friezeLumaMax; foundry x0.43,
iceline x0.57, kiln x0.62, span x0.70, spire x0.40), the accents' largest channel 1.0 -> 0.6, iceline / span walls at decor54's luminance
(wallBaseHex57 / wallUpperHex57). Estimate on r1 frames (the frieze's A/B pixels scaled by the factor, the walls and accents not simulated):
greyscale 18 -> 20 / 36 (kiln +1, spire +1). The span walls are the larger part of the rest (not simulated). Expected r2: near 22-24 / 36, not
measured; the per-room "none" frames differ in their tells too (foundry: no frame flips between on and none by colour56's lift).

## V2 brief55: the 2.4 px overlap (e8758255)
The overlap is ONE frame (n56-r5 verbs37 kiln and spire, f702): the hint card at y 56 while the counter's plate opens at 53.9-58.4 px; on f703
the hint is at y 153.9. Cause: Ui's render lays the meta column (RenderMeta, Ui.cs:868) before the HUD opens the counter (RenderHud33 ->
Counter43, Ui.cs:896) in the same frame. Fix: UiBrief55.Brief55CounterShows counts the counter's own open test (the room's left count >= 0 while
Playing) when its root is not active yet, so the column moves on the open frame (the root's pivot is top-left: its top edge holds under any
open scale). Gated by Brief55.OpenFix57 (no `-skygrindOff57`). No teach line changed (MAIN: every line still passes, kiln 6 / 6 ok, spire 2 / 2
ok on r5). After: 0 frames expected, not measured. Check: `node Tools/night/n55k-teach.cjs Logs/feel/<tag>/vk/verbs37/telemetry.jsonl kiln
Logs/feel/<tag>/vp/verbs37/telemetry.jsonl spire` (brief55 on in the copy) and the off side with `-skygrindOff57` (expect 1 frame each).

**r1 (my read of the lead's n57-r1 verbs37, 6c2054ef):** brief55 on (vk / vp): a card over the counter in 0 frames on kiln and spire;
-skygrindOff57 (vkoff / vpoff): 1 frame, 2.4 px each; -skygrindOff55: 99 frames, 42.1 px. The r1 reels ran without a pad ("pad": false), and
the checker counted the keyboard's "LEFT CLICK" as two words and no glyph (5 / 6 kiln lines BAD on both sides alike); fixed in the checker
(a key name is one glyph). Re-read: GREEN teachText, every line ok (6 / 6, 2 / 2). By the rule: MAIN held, GUARD 1 -> 0.

## V3 grade55 (d80c0a86)
**THE CROWN, light-only.** NIGHT56's two attempts toned the shadows (SplitToning steel-blue, balance 44-50), darkened (EV), raised contrast and
lerped the fog to a cool grey: kept 400 / 527 (0.76), then 362 / 533 (0.68) of the ring's tell cells. The ring is the neon material (URP Unlit:
no light reaches it), so only the fog and the post can change its pixels. Now crown37 p1-p3: split and turn a neutral 0.5 grey (SoftLight's
identity), EV 0, contrast 0, saturation 0, fogMix 0; the ambient and the key light keep their cool tint (they reach the lit stone, never an unlit
tell). Expected: tells kept = raw (1.0); not measured. Two risks, not checked: red-lit stone near the fire that counted as "tell" by colour may
lose a few cells to the cooler ambient; a p1 shot taken inside a turn (common's turnEv -0.4 / turnContrast 30, 0.55 s) darkens the ring that
frame. The walls' "differs" line (dH >= 25 against raw) will likely fail now (the walls move only by the light): not a guard of V3.
**THE WICK:** no number or path changed (GRADE / HOT WALL GREEN at n56-r5 kept by construction). The ride cue: P1 folds the panels into the wall
and out of the world (Wick37Stack: "P1: the panels and the steps folded into the wall"; Wick37View.N47: the trim hidden), so the r5 critic's
0 % on the p1 / burn / cold shots is the P1 shaft wall, where no ride panel exists (V-3: a wall that cannot be ridden never has it). a155 now
records each shot's panels-out state and prints `RIDE CUE GREEN / RED`: the pooled cyan share of the panel cells on the panels-out shots (turn,
p2) >= proof.panelCyanMin 0.10. Read on NIGHT56's numbers: b1on r5 turn 13 %, p2 14 % (GREEN); b1off r2 8 % (RED).
**Observation, not built:** the P1 shaft wall itself may be ridden (wick55's burn needs a ride); if it is, V-3 asks it to carry the cyan strip in P1.
Not checked; THE WICK's look is GG's.
Reels (the lead): `bash Tools/night/n47-reels.sh <tag> b2on,n52-spire,a155` (THE CROWN; grade55 on in the copy; the TELLS field: graded n vs raw n)
and `b1on,n52-foundry,a155` (THE WICK: GRADE / HOT WALL / RIDE CUE). The off side `-skygrindOff55` (ungraded).
Frames for the critic: shot-N55-crown37-p1.jpg and -p1-raw.jpg (the ring in both), shot-N55-wick-p2.jpg (the panels' strips).
Note: grade55's numbers are config, not gated by `-skygrindOff57`: with grade55 ON in a copy and `-skygrindOff57`, THE CROWN takes tonight's
palette (grade55 is OFF in the shipped config, so NIGHT56's shipped game is unchanged).

## V4 rim56 and yellow56, one attempt each (9d51acfb)
**rim56:** with colour56's cap the environment's p90 S drops to ~0.70 (simulated on n56-r5 foundry), under the bodies' 0.76-0.95: the S half of
the rule passes; the V half does not (bodies V 0.57-0.62 vs the environment's p90 0.67-0.69). The body is unlit, then fogged, then posted
(Enemy.shader: no clamp before MixFog), so an HDR gain reaches the frame: skin52's lit side 1.5 -> 2.2 and the shadow side x1.47 (the ratio
and the hue kept), only without `-skygrindOff57`. Simulated with the cap alone (n57-v-cap.py, n56-r5 a356, enemies in view 77): body over the
environment's p90 41 -> 61 (53 -> 79 %; foundry 3 -> 6 / 14, iceline 13 -> 15 / 17, kiln 10 / 15, span 15 / 16, spire 0 -> 15 / 15); the
slate's original rect rule 12 -> 21 (27 %). The gain on top is not measured (a rough ACES estimate puts a foundry body near V 0.69).
MAIN (>= 90 %): likely short on foundry and kiln. Guard (red-orange stays the enemies'): the gain is per channel on the danger token; as R
passes the tonemapper's shoulder the hue drifts toward orange (estimated 11 -> ~15 deg, inside 5-35); not measured. Risk: bloom up close.
**yellow56:** colour56's role arcs keep 350-70 whole, so the cap never touched yellow. yellow56's own numbers (dimYellow) ride the same pass:
a pixel at hue 40-76 with a value under 0.75 (whole under 0.5) moves toward S 0.45 (under the meter's 0.5), luminance and hue kept; the parry
tells (V >= ~0.85) never move. Simulated on n56-r5 a356 with the cap: spire frames over 0.5 % 6 -> 5 of 6 (median 8.08 -> 2.54 %), foundry
5 -> 2, kiln 1 -> 0. What stays: THE GUN. The viewmodel draws after the pass (the overlay camera), and on the spire its dim yellow is 0.13-4.27 %
of the frame (the world alone after the caps: 0.04-0.27 % on 5 / 6, v5 mid 0.72 % beside two parry orbs). So the MAIN (<= 0.5 % per frame,
the gun counted) stays short on spire unless the viewmodel is reached: the next lever is the viewmodel's own light or a pass on the overlay
camera (not built: no Player to check that an overlay-camera pass keeps the gun intact).
**The gun (1850d877):** the dim yellow now also runs on the viewmodel's overlay camera (a second pass instance, the dim yellow alone, after the
gun draws; both rules idempotent on the capped world; if URP hands that camera the backbuffer the pass returns). Simulated on n56-r5 verbs37
with the gun under it: spire 34 / 42 -> 2 / 42 frames over 0.5 % (median 8.54 -> 0.03 %), foundry 12 / 36 -> 1 / 36. Not measured; the eye
check: the gun drawn intact. a357's line prints the passes on the viewmodel's camera.
Reels: a357 with rim56 / yellow56 on in the copy (its line prints the lit gain and the dim-yellow line); verbs37 x5 for yellow per frame:
`python -I Tools/night/n56-colour.py Logs/feel/<tag>/vp/verbs37` (the "yellow per frame" line; the gun's share on each line).

## Files touched outside V's own (one line each)
- `Assets/Scripts/Game/Render/TravelBlurFeature.cs`: `Cap57.Enqueue(renderer, cd.camera);` (one line, above the base-camera return since 1850d877; the pass event AfterRenderingPostProcessing + 1 keeps it after the CRT).
- `Assets/Scripts/Game/FeelReel.N56.Colour.cs` (colour56's reel, V's): one line in C56Shot, the -nocap twin.
- `Tools/night/n55k-teach.cjs` (brief55's checker): keyboard key names as one glyph.
