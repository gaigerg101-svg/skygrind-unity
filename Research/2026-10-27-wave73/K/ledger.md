# K ledger: THE KILN ARENA IS ALL DANGER ORANGE (NIGHT73, builder K)

Only what was measured or read; the rest is "not checked". Frames are read by Tools/night/n73-hue.py (danger band hue 0-40 / 345-360, S >= 0.35,
V >= 0.20, HUD bands cut) and by a region sampler (9 x 9 px means at fixed points, scratch script, not committed).

## 1. The cause (step 1)

The arena's surfaces are NOT what is orange. Read and measured:

1. **The stone's own colours are cool.** WyrmRoomView.cs:64 draws the ring's boxes in `colors.pillarTop` 0x877D92 (hue 270, S 0.14) and
   `colors.pillarSide` 0x524A7A (hue 255, S 0.39) (PortExport/config.json:7249-7250); the level's own k5 stone is palette72's kiln family in the
   play build (plum walls hue 272, sand floor; palette72 is one of the eight rows ON). The lead's two "ruled out" items stand.
2. **BossGrade54's wyrm palette crushes the blue channel of everything on screen.** Same pose, grade off vs on (Logs/feel/n54-r3/g3k/a254,
   shot-N54-wyrm-p1-raw.jpg vs shot-N54-wyrm-p1.jpg): back wall (36, 25, 57) hue 260 -> (5, 4, 1) hue 45; right wall (32, 19, 49) -> (5, 4, 2);
   the tan left wall (103, 68, 64) S 0.38 -> (55, 23, 3) S 0.94 hue 23; the ledge (115, 64, 58) -> (66, 18, 3) S 0.95. Blue 57 -> 1, 64 -> 3.
   WHY (BossGrade54.cs:393-396, 160-165 and design.n54.grade54.numbers): the split toning's colours are pushed away from grey by
   `common.intensity` 1.8 (BossGrade54.Push); wyrm P1 shadows (0.36, 0.34, 0.20) push to (0.25, 0.21, -0.04 -> clamped 0.0) and P3's (0.28, 0.21,
   0.13) to (0.10, 0.00, 0.00), the highlights' blue (0.30 / 0.26 / 0.22) to 0.14 / 0.03 / 0.0; URP's SoftLight with a blend of 0 on a channel
   squares it (base^2), so a shadow's blue goes to nothing and its saturation to 0.9+. The colour filter (0.95, 0.96, 0.78) pushes to
   (0.91, 0.93, 0.60) linear on top, the saturation delta +4 / +10 / +12 x 1.8 and the contrast delta +16 / +24 / +32 x 1.8 (up to +58 points)
   harden it, and the turn's wash (0.85, 0.95, 0.45) pushes to blue 0.0 and `turnSat` 35 adds saturation at each turn peak.
3. **BossGrade55's wyrm palette adds the amber on purpose** (design.n55.grade55.numbers.wyrm: "WARM shadows (amber-umber split toning ...) the
   yellow-red-orange theme kept through every phase"): split (0.62, 0.48, 0.34), ambient tint (1, 0.88, 0.72), key (1, 0.9, 0.75), fog (0.16, 0.09,
   0.05). Same arena, grade55 off vs on, p1 (Logs/feel/n56-r4/b3off vs b3on, a155): danger share 7.3 % vs 51.9 % (luma 21.8 vs 47.9).
   THE CROWN's grade55 numbers are the proven alternative (neutral split, balance 50, ev 0, contrast 0, fogMix 0, a cool ambient / key).
4. **lit72 turns that dark, saturated, blue-less pixel into a bright one in its own hue** (Lit72.cs:5-6, 30-36: colour scaled, never replaced,
   kmax 9). Measured: lit72 OFF wall (53, 7, 2) hue 6 S 0.96 V 0.21 (n72-t2/litOff2, idle) vs the play build's (82, 29, 31) hue 358 S 0.64 V 0.32
   (n72-play); floor (100, 32, 1) hue 19 -> (138, 67, 7) hue 28; ceiling (31, 4, 1) -> (111, 29, 7) hue 13. The brief's own numbers: 77.4 -> 91.8 %
   on the same frames. lit72 is the amplifier (+10-15 points), the grades are the source (the other 77 %): lit72 OFF still reads 77-87 %.
5. **Bright71** lifts the ambient colours, the fog colour and the stone albedo with the hue kept (Bright71.cs:56-86): no hue push; and Stone.shader
   lights with SampleSH (the theme's probe), not RenderSettings.ambient* (Lit72.cs:94-98), so the grades' ambient tints reach the stone only through
   the key light (the sun colour x the key tint: (0.95, 0.95, 0.70) x (1, 0.90, 0.75), blue x 0.52). The Stone fog uses Look's `_SkyFog*` globals,
   not RenderSettings.fogColor (Stone.shader:167-176): the grades' fog colours do not reach the stone.
6. **The room's light** (design.n25.wyrmAim.room.lightColor 0xcfc6ff, WyrmRoomView.cs:85) is pale violet: not a contributor. k5 has one level
   light (campaign.lights k5-L3, colour "white", n57-kiln.json); no lava in k5 (its lava is kb3c / kb3d).
7. **The red speckled border** (3 of 5 boss63 frames: stagger, tell, windup; none in flinch, idle): Calm72.cs:5-22 (builder L, NIGHT72) read
   its sources: EXPOSED's pulsing edge (UiFeelEdges.cs:74-86, colour 0xff280a = UnityTuning.ExposedEdgeHex, alpha 0.35 + 0.35 sin(7 t), 90 px
   blur + 30 px spread: 120-150 px deep) shown while `director.Exposed` (the web's camp meter >= camp.exposedAt: standing still in one place,
   Director.cs:163), at any health. The reel stands still; the flinch (2.0 s) and idle shots come before the camp meter rises, the stagger
   (14.0 s), windup (12.9 s) and tell shots after it. That fits; it is NOT measured (no frame names which overlay it is). It is a danger overlay
   drawn at full health (the reel's player has 9999 HP), not a hurt cue.

Not traced / not checked: the exact per-contributor share of the 83.1 % mean (no Player run in this tree; the shares above are pair evidence from
older builds, n54 / n56, whose arena and rows differ from the play build's); whether the room mesh (WyrmRoomView) or the level's k5 stone is the
surface in front on each wall; the wyrm glow / ring lights' share on the floor and ceiling; whether a campaign boss room runs the director's camp
meter in a real fight (the reel does).

## 2. kiln73 (step 2, built)

What it does with the row ON (view only; fingerprints EQUAL; the row is read every frame, the numbers are UnityTuning.N73K.cs):

- **K1 the wyrm's two grades go cool** (Kiln73.cs): the wyrm's Grade54Boss and Grade55Boss phases (public objects BossGrade54.PaletteOf /
  BossGrade55.PaletteOf return) take kiln73's numbers in place while the row reads ON and get their own numbers back (a deep copy taken first) when
  it reads OFF: filter near white, the split toning neutral-to-plum with no clamped channel, no saturation delta, a small contrast delta, the turn's
  wash pale (a turn only darkens, as THE CROWN's), cool ambient / key tints (the CROWN's), the fog a dark plum, `turnSat` 0, and the grade54 rim
  (`rimHdr`) recomputed against the new filter (BossGrade54.ReadBoss pre-divides it by P1's filter). Grade, turns, tells and the player's rim colour all
  keep working: the wyrm's palette is now a quiet plum / violet one (rule 8: red-orange is the wyrm and its tells, not its room).
- **K2 the EXPOSED edge is not drawn in the wyrm's arena** (Kiln73.cs): the Image the HUD calls "Exposed edge" is disabled while the arena is shown and
  the row is ON (calm72's method; Calm72.Active leaves it alone); put back when the arena or the row goes. The director, its camp meter and the sim are
  untouched. A real hurt cue (the hurt flash, the damage and low-health vignettes) is left as it is.
- **K3 the proof prints its input**: `[kiln73] ...` lines (see the report).
- The room mesh (WyrmRoomView) is NOT retinted: its colours are cool already (1.) and the level's stone is palette72's; a retint would add a writer
  with no measured job. The probe line prints the room mesh's colours so the next run can say otherwise.
- The numbers (UnityTuning.N73K.cs): filter (0.98, 0.97, 1.0) -> pushed (0.964, 0.946, 1.0); shadows P1-P3 pushed to about (0.42, 0.38, 0.62) /
  (0.40, 0.35, 0.66) / (0.39, 0.33, 0.68), hue about 250; highlights neutral; wash (0.92, 0.92, 1.0); grade55 split (0.46, 0.42, 0.58) balance 40; ambient
  (0.92, 0.91, 1.0), key (0.96, 0.95, 1.0), fog (0.10, 0.08, 0.16) at 0.25; sat 0, contrast 6 / 4, turnSat 0. Kiln73Math.PushedInDanger = 0 of 10
  (Kiln73Tests pins it).
- The probe lines (Player log, both sides): `[kiln73] wyrm arena: row ON|off, surfaces 0 retinted ...` (build), `[kiln73] wyrm arena shown: ...` (the light's and the
  grades' live state: sun, ambient equator, fog, lit72, the edge, director exposed), `[kiln73] edge probe: director exposed yes|no at <t> s, the HUD's
  exposed edge alpha .., hurt flash alpha .., damage vignette alpha ..` (each change of EXPOSED while the arena is drawn: the plain side's line names
  which overlay the border is), and ON only `[kiln73] wyrm grades: ...`, `[kiln73] exposed edge: hidden ...`.

## 2b. Correction (the lead's Editor run, Kiln73Tests 5 / 6)

`TheConfigsWyrmShadows_ClampAndSitInTheBand_KilnsDoNot` asserted that the pushed P1 shadow (0.248, 0.212, 0.0) is "hue 9, S 1: the danger band". My arithmetic was
wrong, not the band check: for a colour whose largest channel is red, hue = 60 x (g - b) / (max - min) = 60 x 0.212 / 0.248 = 51.3 degrees (I had used r - g).
Pushed by 1.8, the config's three shadows are P1 (0.248, 0.212, 0.0) hue 51.3 S 1.00 V 0.248 (amber-yellow: out of the band's 0-40, in the parry arc),
P2 (0.212, 0.086, 0.0) hue 24.3 S 1.00 V 0.212 (orange: in the band), P3 (0.104, 0.0, 0.0) hue 0 S 1.00 V 0.104 (dark red: under the band's V 0.20). The cause
stands (a clamped blue, a fully saturated shadow, blue 57 -> 1 in the n54 pair): only the "P1 is in the band" sentence was wrong. The test now pins the three
hand-computed values and that P2 is in the band. Nothing in the kiln73 maths or numbers changed.

## 3. Checks run in this tree (no Editor, no Player)

- `bash Tools/night/dn-check-wt.sh <scratch>/dn`: 0 errors on the four projects (after the last edit).
- `bash Tools/night/n72-gates.sh --no-editor`: all GREEN: config copies identical, golden 39 vector sets / 22 replays exact, 0 mismatches, fingerprints EQUAL
  (baseline and --off57 .. --off67), run at 9cfe0c0e8's tree (before the edge-probe commit, which touches no sim file).
- Kiln73Tests: written, compiled (dn-check), NOT RUN (no Editor here), so not seen RED then GREEN. The first test is the cause's pin: the config's own
  shadows clamp when pushed (it passes as soon as it compiles; it is a pin of the cause, not a failing-first test).

## 4. Not checked, and what could go wrong

- Not one pixel of the ON arena has been seen: the hue maths above is read from code and from older pair frames (n54 a254, n56 a155), which predate
  Bright71, lit72, palette72 and the n57-kiln arena. The bar (ON mean over the five boss63 shots at most 35 %, dark share at most 15 %, the wyrm still
  red-orange) is the lead's Player run.
- The sun colour (Look.Apply / DuskClock22) and the theme's ambient probe are not touched; if the ON mean stays high the next suspect is the key light's own
  hue and the probe (the `wyrm arena shown` line prints the sun, ambient equator and fog colours), then the wyrm's glow / ring lights on the floor and ceiling.
- The orange floor lines and the dark-orange floor tiles in the frames (the ring's lanes and heat panels) were not traced: lane lines are the wyrm's own tells
  (kept by design); the tiles (158, 47, 2) next to them were not identified.
- Dropping the wyrm's sulphur / umber identity is a design call (GG 2026-10-20: "each boss ... its own distinct color palette"): it becomes a quiet plum;
  its body, ring and tells stay red-orange. THE CROWN's grade55 numbers were the precedent (neutral split, cool light).
- Hiding the EXPOSED edge in the arena removes an anti-camp warning there (the director and its meter run as before; other rooms and survival keep the cue).
  GG's call whether the cue comes back thin (calm72's measured way).

## 5. K2: calm72 reworked (branch n73-K2 from 5fe5dbf1b)

Job (the lead): with the row ON, at ANY health the hurt flash, the damage vignette and the EXPOSED edge all draw, at the alphas Ui wrote, on the thin frame
(L72CalmEdgeClear 0.94: 58 px at the sides, 32 px top / bottom at 1080p); EXPOSED's pulse amplitude at most half; under 30 % the thin vignettes as before; K2 of kiln73
consistent; the log line per change names what the HUD wrote and what is drawn; bar: the border at most 60 px on boss63 n57-kiln with calm72 ON, the hurt flash drawn
(alpha > 0) on every hit.

Read: Ui writes the hurt flash's colour every frame it changes (Ui.cs:1166-1186), the damage vignette's when its rounded value changes (Ui.cs:881-887), and the EXPOSED
edge's only when its own rounded alpha changes (UiFeelEdges.cs:77-83: `ea != shownExposedEdge`), so a value read back from the Image is the HUD's only if calm72 has not
written it since. calm72 therefore keeps `exposedWrote` (the HUD's last) apart from `exposedOut` (what it drew): a colour alpha that is not `exposedOut` is a new HUD
write. The swell Ui gives a starting edge is 0.04 s (UiHudMotion18.cs:167, Hud18FlashAttack), so calm72 repeats it from its own age counter (Calm72Math.ExposedAttack).

Built (Calm72.cs, UnityTuning.N73K.cs, Kiln73.cs's comments only, Tools/night/n73-edge.py, Calm72Tests.cs):
- the three Images' sprites are swapped for thin ones (the EXPOSED edge's 9-slice type set to Simple while swapped), always enabled (the first build held the three at
  `enabled = edge` and kept an `enabled` it read from someone else's write: a hazard with kiln73's K2, so none is read any more); everything is put back with the row;
- EXPOSED: drawn alpha = scale x wrote + (1 - scale) x base x swell, scale `Calm73ExposedSwingScale` 0.45: Ui's 0 .. 0.70 pulse becomes 0.19 .. 0.51 (range 0.32 of 0.70);
- `[calm72] hit N at <t> s | health .. % | the HUD wrote hurt <peak> (peak) | hurt <peak> thin 58 x 32 px` once per hit at the flash's peak (a rise of at least
  `Calm73HitJump` 0.03 in a frame starts one; the low-HP breath moves by hundredths); and the per-change `[calm72] ON|off | health .. % | under the low-health bar yes|no |
  the HUD wrote hurt .. damage .. exposed .. | drawn hurt <alpha> thin 58 x 32 px, damage .., exposed ..` (OFF names the HUD's own sprite instead of "thin");
- kiln73's K2 already stands down while `Calm72.Active`: with calm72 ON the wyrm's arena shows the thin EXPOSED edge like every room (comments updated, no code change);
- PostFx's low-HP vignette still starts at 30 % with the row ON (unchanged).

Tools/night/n73-edge.py: the depth in px of the red border from each edge (sides: the middle half of the height; top: x 20-32 % and 68-80 %; bottom: x 27-45 %), where
"red" is red-dominant (R over G and B by 8): the first draft used n73-hue.py's danger band and the planted case showed it read a 135 px frame as 40 px (over a cool wall the
overlay enters that band only at an alpha of about 0.3). Planted cases (`--selftest`, run here): none 0 px; thin 0.94 at the first frame of a hit reads 37 px sides / 21 px
top and bottom of its 58 / 32; the same with a red HUD bar and plate planted stays 37 / 21; thick 0.86 reads 85 / 48 of its 135 / 76. All PASS. An edge whose strip is red
over 30 % of its length inside (an orange floor, the lava bars on the left) prints n/a. Run on the lead's kiln73-ON, calm72-OFF frames (Logs/feel/n73-c/kon/boss63) it reads
the thick frame: right 84 / 88 px, top 52 / 56 px on the stagger and windup shots, tell left 102 / right 74 / top 44 / bottom 60; the bottom strip is n/a (orange floor) on the
others, the left n/a or 143-196 px (scene red in the strip), flinch 0 / 0, idle right 0 / top 0 (no border). So the bar (at most 60 px) is read on RIGHT and TOP (the cleaner strips);
the frame ON should read about 37 px / 21 px at a hit's first frame.

Checks here: dn-check 0 errors on the four projects; n72-gates --no-editor all GREEN (golden 0 mismatches, 22 / 22 replays, fingerprints EQUAL incl. --off67); n73-edge --selftest PASS.
Not checked: any Player run; that the Image swap behaves as read in a built Player (the thin sprite on the 9-slice EXPOSED Image with its type set to Simple, the per-frame colour-alpha write); whether boss63
produces hits with its invulnerable player (the earlier frames' border is most likely EXPOSED, not a hit; the `[calm72] hit` line counts hits, 0 lines on a run with no hit);
the look of the thin EXPOSED frame (no frame has been seen); Calm72Tests / the new tests have not been run in the Editor.

## 6. K3: the night's review, S1-S5 (branch n73-K3 from 8a5801f55; the review is Logs/n73/review.md in the main tree)

- **S1 (kiln73's K2 hid EXPOSED with calm72 OFF): retired.** Kiln73.cs no longer has `Edge`, `HideExposed`, `EdgeHidden`, `exposedEn0` or its own find counter; kiln73 never touches
  the EXPOSED Image, the hurt flash or the damage vignette (the edge probe only READS them). Header, the arena build line (`... the EXPOSED edge is calm72's (kiln73 never touches it)`),
  the `arena shown` line and the edge probe line (`calm72 ON|off (read only: kiln73 touches none of the three)`) say so. The HUD's own EXPOSED edge is therefore the NIGHT72 game's
  with calm72 OFF in the arena too. Kiln73Tests: `TheExposedEdge_IsHiddenOnlyInTheArenaWithTheRowOn` removed with the function. S5 (K2 read `Calm72.Active` from the previous frame) is moot.
- **S2 (the HUD search's session-wide cap of 40, never reset): fixed.** The count is a per-HUD allowance (`Calm72Search`, pure: Max, Every, Due, Searched, Reset; the numbers are
  `UnityTuning.Calm73FindMax` 40 / `Calm73FindEveryS` 0.5, the first build's literals). `Calm72Math.HudLost` (pure) says the HUD is lost when an Image found once is gone now; `HudLost()`
  drops the refs, the seen flags, the kept sprites and the trackers, resets the search and logs `[calm72] the HUD was rebuilt (a scene load; N so far) ...`. Tests:
  `HudLost_IsAnImageFoundOnceThatIsGone` (truth table) and `Search_SpendsItsAllowance_ResetGivesItBack` (100 simulated scene loads: each fresh HUD gets the full 40 searches, 0.5 s apart).
- **S3 (the restore put the previous scene's sprite on the new HUD): fixed.** The originals are kept per Image instance (`hurtFrom` / `damageFrom` / `exposedFrom`: taken again when
  the Image is another one; `exposedHasOut` reset with it), and a lost HUD drops them with the refs, so Restore only ever restores what was kept from the Image it restores.
- **S4 (EXPOSED's swell age on the wrong clock, reset by Restore): fixed, small.** `exposedAge` advances every Tick (before the row is read, so also in OFF and in a hold) by
  `Math.Min(FeelClock.FrameDelta, cfg.movement.loop.maxFrameDelta)` (Ui's own `frameDt`: SurvivalGame.cs:273; the reel's fixed clock, 0 in a held frame), is reset only when the
  director stops calling the player exposed, and Restore no longer touches it. Not covered by a unit test (it is a call of two engine clocks); the existing
  `ExposedAttack_IsUisSwell` pins the swell itself.
- Checks: dn-check 0 errors on the four projects; n72-gates --no-editor all GREEN (golden 0 mismatches, 22 / 22 replays, fingerprints EQUAL incl. --off67); a reflection runner over the
  built Skygrind.Game.Tests.dll (scratch, not committed) ran Calm72Tests 8 / 8 and Kiln73Tests 4 / 5 PASS: `RimHdr_IsRecomputedAgainstTheNewFilter` needs the engine (Color.linear is an ECall:
  "ECall methods must be packaged into a system module") and passed in the lead's Editor run; nothing in RimHdr changed.
- Not checked: any Player or Editor run of K3 (the HUD rebuild path, the [calm72] HUD-rebuilt line, the arena with calm72 OFF now showing the HUD's thick EXPOSED edge again by design).
