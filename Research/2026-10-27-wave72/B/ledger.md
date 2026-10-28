# Builder B ledger (NIGHT72; worktree n72-B, branch n72-B from t0b b80afc2fa; t0 2026-10-27 10:19:20 MST, the lead's clock)

Every number below names its file, command, sha and time. Anything not run is "not checked". No Editor, Player, reel or suite was run by this builder.

| slice | commit (n72-B) | row (design.n72, OFF) | state | what the lead runs |
|---|---|---|---|---|
| B2 bossfx72 | 9f181a35e (10:50:55) | bossfx72 | built, compiled (dn-check-wt 0 errors), meter planted PASS; no Player frame | boss63 on n52-spire, ON / OFF |
| B1 body72 | 2ca44bb4c (11:07:59) | body72 | built, compiled; Spring72Tests written; meter planted PASS; no Player frame | boss63 on n57-kiln, ON / OFF |
| B3 btext72 | 3f7f479c6 (11:18:21) | btext72 | built, compiled; Btext72Tests written; no Player frame | boss63 on n57-kiln and n52-spire, words71 / voice71 / bossText55 on both sides |
| B4 spring72 | 003d05c9f (11:25:00) | spring72 | built, compiled; no Player frame | kill12 (and any reel with enemies), ON / OFF |
| B5 finish55 + bosskill72 | 52eac37c2 (11:32:49) | finish55 (n55) | built, compiled; no Player frame | bosskill72 on n57-kiln and n52-spire, ON / OFF |

Gates on 52eac37c2 (`bash Tools/night/n72-gates.sh --no-editor`, 2026-10-27 11:34:47, in this worktree, output Research/2026-10-27-wave72/B/evidence/gates-52eac37c2.txt): config copies identical; golden 39 vector sets
(48094 records), 22 replays (22 exact), 0 failing, 0 mismatches; fingerprints seed 1 EQUAL for plain and --off57 .. --off67; "gates: all GREEN". No slice touched Skygrind.Core or the sim.

The commands and the log lines to read are in each section's "The Player step for the lead". The meters: Tools/night/n72-crownfx.py (planted case PASS 2026-10-27 10:49) and Tools/night/n72-wyrmpx.py
(planted PASS 11:07); their OFF readings of the lead's n71-polish8 frames are in evidence/. dn-check-wt (the compile of the four projects, a scratch build): 0 errors after each slice.

## B2 bossfx72 (THE CROWN visible in its fight)

### What the frames are (read, not measured)
The n71-polish8 frames (Logs/feel/n71-forge72/bp/boss63, the lead's) show a 700-900 px cyan (idle, stagger) or red-orange (tell, windup) mass under the crosshair. Read in
the code (Crown37View.Crown38.cs, Crown37View.Crown40.cs, config design.n38.crown38): it is THE CROWN's own 20 gem halos, not a separate effect. NIGHT38 sizes a halo at
gemHaloScale 2.2 x a gem of 1.6 x 0.7 = 1.12 m = 2.46 m, additive; 20 gems sit on a 3 m ring, 0.94 m apart (2 pi 3 / 20), so the 20 squares overlap into one annulus about
4.2 m out (4.2 / 6.6 m x 704 px = 450 px radius at the reel's 6.6 m flinch distance: 700-900 px across). Crown40's idle tint puts the halos (and the ring) cyan, a red tell
puts the ring and the halos in the danger colour. The reel's torch (the player's cyan lick at the aim point) adds small sparks, not the mass. Not checked in a Player:
that the halos alone make the mass (no ON frame exists yet).

### The change (view only; row design.n72.bossfx72, OFF in the config; the lead flips it)
- Crown37View.Crown72.cs (hook lines in Crown37View.Crown.cs: CrownReset, DrawCrown after Crown47Draw): with the row read ON every frame, the alive gems are 0.72 of the gap
  between neighbours (0.68 m), their crosses follow, halos 1.3 x the gem (0.88 m, inside the gap), the halo colour NIGHT38/NIGHT40's own read back x 0.7; the blade ring, the inner
  rim and the outer halo ring stay CYAN through a RED tell (gain 0.9, the lit gain at pass speed); the window, a pass flash, a yellow tell and a gem's break flash are untouched.
  Row OFF: the scales are restored the frame it reads false; colours are NIGHT38/40's every frame.
- BossTell63.cs: THE CROWN's danger column is 0.62 x radius, 0.6 x height with the row ON (the gain, level and rise are unchanged, so the boss63 cue measure reads the same).
- Numbers: Assets/Scripts/Game/UnityTuning.N72B.cs (Fx72*).
- Proof lines (Player.log, the input printed; they read one way OFF, the other ON):
  `[fx72] crown row bossfx72 on|off: gem <m> m, halo <m> m, gap <m> m` (once per state) and
  `[fx72] crown red tell <attack>: ring hue <deg> (cyan 165-200), gems hue <deg>, column radius x<s> | row bossfx72 on|off` (once per red attack, hues read back from the
  renderers' _BaseColor); BossTell63's existing `[tell63] cue ... radius <m> m, height <m> m` also changes (5.0 x 10.0 OFF, 3.1 x 6.0 ON).
  Expected, from the code, not run: OFF gem 1.12 / halo 2.46; ON gem 0.68 / halo 0.88; red-tell ring hue about 10 deg OFF, about 185 deg ON.

### The meter (Tools/night/n72-crownfx.py; planted case PASS, evidence/crownfx-off-n71-polish8.txt)
Bars fixed before any ON frame existed: the effect coverage of the 300 px disc round the crosshair (the gun's corner cut out) <= 15 % (the slate's "15 % of the frame inside 300 px"
cannot fail as written: the disc is 13.6 % of a 1080p frame; read as 15 % of the disc); THE CROWN's body = the cyan band outside the opened mass, >= 15000 px AND coverage within the
bar, in >= 4 of 5 frames; the tell and windup frames' danger-hue share <= 20 % of the frame.
OFF (the lead's n71-polish8 frames, evidence/crownfx-off-n71-polish8.txt, 2026-10-27 10:49): visible 0 of 5; coverage 24.2 / 12.3 / 25.5 / 13.9 / 16.8 % (worst 25.5, FAIL);
tell frame's danger-hue share 26.8 % (FAIL). The ON half needs the lead's frames.

### The Player step for the lead
ON and OFF in one build, one Player per side (L-046): `bash Tools/night/n72-reels.sh n72-<k> bpon,n52-spire,boss63,-skygrindRows72,bossfx72:1 bpoff,n52-spire,boss63`
Read: Logs/feel/n72-<k>/bpon/boss63-crown37.txt and the Player log Logs/player/n47-reel-n72-<k>-bpon.log for the `[fx72]` lines (ON: gem 0.68, halo 0.88, ring hue 160-200 in a red
tell; OFF: gem 1.12, halo 2.46, ring hue under 40) and `[tell63] cue crown37 ... radius 3.1 m, height 6.0 m` (ON; OFF 5.0 x 10.0). Then the meter on both sides:
`python -I Tools/night/n72-crownfx.py ON Logs/feel/n72-<k>/bpon/shot-N30-B63-crown37-{idle,flinch,tell,windup,stagger}.png` (and OFF). The pair: `node Tools/night/n72-pair.cjs bossfx72 <on tell.png> <off tell.png>`.
The boss63 reel's own GREEN line must hold with the row ON (the row only changes tints and scales: bossMotion63's limit share is untouched).

### The state bossfx72 builds on (the lead's n72-h0 H4 frames, read 11:36)
Logs/feel/n72-h0/h4on-sp/boss63 (wyrm71 + voice71 + words71 + cyan71 + bossText55 ON, no n72 row), same meter: evidence/crownfx-harvest-h4on-sp.txt. cyan71 (weak points bone while closed) already took the idle frame's
cyan mass away: idle coverage 24.2 % -> 1.0 %, cyan body 18,339 px (visible yes). The other four are unchanged: flinch coverage 13.2 %, tell 24.9 % (danger-hue share 26.8 %), windup 13.4 %, stagger 16.5 % (cyan flood: mass
83,972 px); visible 1 of 5, worst coverage 24.9 % (FAIL), tell share 26.8 % (FAIL). So the pair's OFF side for bossfx72 is the harvest-ON state, and the work left for the row is the red tells (the halo annulus, the ring in the
danger colour, the column) and the open window's flood: exactly what Crown72 / BossTell63 change. (The tell and windup frames still show the orange ring of halos with the pink ring inside it, as in n71-polish8:
Logs/feel/n72-h0/h4on-sp/boss63/shot-N30-B63-crown37-windup.png read at 11:36.)

### Not checked
Any Player frame; whether the gem halos alone make the mass (the harvest frames agree: the halos' colour follows the tell, the shape did not change); the compile in the Editor (dn-check-wt: 0 errors on the four
projects, scratch build, 2026-10-27 10:46); the EditMode suite.

## Leftover 1: the other three bosses through boss63 (read from the lead's existing frames; no new Player step)
Frames: Logs/feel/n71-polish6/{bwk, bbr, blo}/boss63 (THE WICK, THE ICEBREAKER, THE LOOM; the build before polish8, none under the n72 rows). Meter n72-crownfx.py, the two bars that apply to any boss: effect coverage of the
300 px disc round the crosshair (<= 15 %) and the tell and windup frames' danger-hue share (<= 20 %); evidence/bosses3-off-n71-polish6.txt (11:36).
- THE WICK: coverage 1.6-3.9 %, danger-hue share 0.9 % at most: both bars PASS with no change.
- THE ICEBREAKER: coverage 0.1-0.6 %, share 0.5 % at most: both PASS.
- THE LOOM: coverage 1.6-2.5 % in idle, flinch and windup; 30.6 % in the stagger frame, share 0.1 %. The 30.6 % is the open spinneret: one cyan sphere (usable cyan, the thing to hit) with the player's THUNDERCLAP
  and the words line over it; read by eye (Logs/feel/n71-polish6/blo/boss63/shot-N30-B63-loom37-stagger.png), it is a legible weak point, not a burst hiding a body. Not changed by bossfx72 (its column share list holds
  crown37 only); stated, not hidden: it fails the literal coverage bar as the weak point is large by design.
- The boss TEXT surfaces are shared: btext72 (the STAGGER word, one order at a time, the plate, 22 px) applies to all five bosses; the Loom's windup frame shows the grey STAGGER word and "Hook a lit knee R, then grapple
  away." with a pale diamond (the grapple target) crossing the line, the same defects gg-proxy #5 names on the Crown and the wyrm.
- Their dark shares and lighting: L's (L3 lit72 / BossGrade); not measured here.

## B1 body72 (THE KILN WYRM inside the pill language, D-11)

### What the code shows (read, not measured)
The wyrm is drawn in the shipped game as the sim's own oriented boxes (WyrmView.cs; the NIGHT26 models are OFF), unlit neon-opaque, in NIGHT47's near-black ash (0x1a1210) with molten seams
(wyrmLook47 ON). Every husk, swarmer and spitter wears SKYGRIND/Enemy instead (Assets/Shaders/Enemy.shader: the rim light, the form shade, the NIGHT51 ink outline, the NIGHT52 ramp); no boss does.
That is the "box" read: flat colour, no edge light, darker than the brown wall. The reel's one wyrm wind-up in 25 s is the `aggro` charge (bk/boss63 log: "attack aggro windup 12875 hit 13408 lead 533"),
whose tell is the head's punch-window yellow; BossTell63's column skips it and the roar (UnityTuning.BossTell63Skip).

### The change (view only; row design.n72.body72, OFF in the config; the lead flips it; it builds on wyrm71's motion parameters)
- WyrmView.Body72.cs (hooks: WyrmView.cs constructor / Render / the piece's colour; WyrmView.N63V.cs DrawMotion63): with the row read ON every frame, (1) PRESENCE: the 24 body pieces and the head draw with
  SKYGRIND/Enemy (EnemyLook.For(game).body, a private block with the same properties EnemyLook.Tint sets, Skin52.Bake once per renderer) in a lifted warm ash (Body72Hex 0x5c4c46, head 0x6a584f) with
  NIGHT47's molten heat on it; the punch window's yellow and the open head's weak role are untouched. (2) FOLLOW-THROUGH: each body piece trails its rigid centre by a damped spring (4 Hz at the head end, 2.2 Hz at
  the tail, z 0.35: Spring72Math, scout-animation.md 1), held to 0.6 of the piece's own guard (BossMotion63's limitShare x half width) and then, with the ripple, sag and breath, to the whole guard by
  DrawMotion63's own clamp; the springs are stepped with the row off too (D1). (3) THE TELL GLOW: through any wind-up but the roar the seams and trim plates go to the danger red-orange (gain 2.2 x
  BossTell63.Level 0.45-1) and the body pieces move toward it (head: its own yellow stays).
- Numbers: UnityTuning.N72B.cs (Body72*). Pure maths: Spring72.cs (Spring72Math), pinned by Assets/Tests/EditMode/Game/Spring72Tests.cs (written; the Python mirror of its arithmetic agrees:
  overshoot z 0.3 / 0.5 / 0.7 within 0.008 / 0.016 / 0.016 of exp(-pi z / sqrt(1 - z^2)); the following spring's steady lag 1.000 of 2 z v / w). Not run in the Editor.
- Proof lines (Player.log): `[body72] wyrm row body72 on|off: material SKYGRIND/Enemy (rim, form, ink)|neon (flat), base 0x5c4c46|0x1a1210, lag 4-2.2 Hz z 0.35|none` (at a state change),
  `[body72] wyrm tell <attack>: level <l>, trim gain <g> | row body72 on` (once per wind-up; OFF prints none), `[body72] wyrm lag: <n> pieces, largest <m> m (share <s> of the guard) | row body72 on|off`
  (every 4 s of a drawn fight; OFF prints 0.000), and the [boss63] rms lines (the parts' offsets from their rigid pose: OFF 0.033-0.053 m, limit share max 0.53 on bk/boss63, n71-polish8).

### The meter (Tools/night/n72-wyrmpx.py; planted case PASS; evidence/wyrmpx-off-n71-polish8.txt)
Bars fixed before any ON frame existed: the median luma of the worm's box against the wall bands above and below it, at least 25 (0-255); the tell glow as a DELTA (ON minus OFF, the same box and pose,
because the kiln arena already holds danger-hued stripes and floor lines inside any box): windup delta >= 600 px and idle delta <= 300 px; the segments' motion RMS (the [boss63] rms lines) above OFF.
OFF reading (the lead's n71-polish8 frames): idle head box diff 88.4 (the head is the punch window's yellow, so that bar is met by the head already: the real gap is the BODY); windup body band
(box 0,500,1015,566) median luma 55.9 against the wall 47.6: diff 8.3 (FAIL); idle body band diff 5.4 (the body is buried at idle). The self-pair (OFF against OFF) reads delta 0 and FAILs the glow bar.
Note the slate's "luma differs by >= 25 in boss63 idle": the wyrm's body is under the floor at idle; the visible idle worm is the head, so the body's bar is read on the windup frame. Stated, not loosened.

### The Player step for the lead
One build, two sides (L-046): `bash Tools/night/n72-reels.sh n72-<k> bkon,n57-kiln,boss63,-skygrindRows72,body72:1 bkoff,n57-kiln,boss63` (plus `+wyrm71:1` on both to test it on top of wyrm71's state, and the
OFF side with the same). Read Logs/feel/n72-<k>/bkon/boss63-wyrm.txt: the `[boss63] rms wyrm` lines (above OFF's 0.033-0.053 m), the verdict line's "limit share max" (must stay <= 1.00: the guard),
"reach ... outside the sim's sphere 0"; the Player log's `[body72]` lines. Then
`python -I Tools/night/n72-wyrmpx.py --pair body72 <bkon idle> <bkoff idle> <bkon windup> <bkoff windup> 0,500,1015,566` and `node Tools/night/n72-pair.cjs body72 <bkon windup> <bkoff windup>`.

### Not checked
Any Player frame (whether SKYGRIND/Enemy's rim and ink read on the boxes; whether 0x5c4c46 lands at luma ~100 under the wyrm arena's grade; the ink's corner gaps on a cube if the primitive mesh is not
readable in the Player: Skin52 then keeps NIGHT51's push); EditMode; that the lag reads in motion (the spring's 4 Hz x 0.6 guard is an hypothesis until the [boss63] rms and a pair show it).

## B3 btext72 (the boss text, right)

### What the code shows (read, not measured)
The boss text is five surfaces: the boss bar's name / LEG / tag (UiBossBar43.cs), the bar's own order (b43Hint, 18 px at a grey 55 % alpha: UnityTuning.Bar43HintPx; hud47 sized it only if the bar
existed at its build, and the lazy bar does not), hud47's fight line (UiHud47.Boss.cs, 40 px), THE CROWN's call (Crown37View.Crown40.cs) and the gem counter. The harvest rows already own three of the slate's
defects once ON: words71 (the order names the hand you hold; its counter prints the wrong-hand frames, row on or off), voice71 (the crown's call hidden while the HUD draws the window order),
bossText55. gg-proxy #5's remaining defects are not theirs: STAGGER's word under an unfilled meter (Bar43Draw: `tag = lineOn ? "STAGGER" : ""` in NORMAL mode), the bar's order at 16-18 px and grey, no plate
under any boss line, and a second order (the bar's) beside the fight line when the two do not read the same (order64 hid the bar's only when they read the same).

### The change (view only; row design.n72.btext72, OFF in the config; the lead flips it AFTER words71, voice71 and bossText55)
- Btext72.cs (pure rules and counter), UiBtext72.cs (the HUD half; one hook: the last line of UiWords71.cs's Words71Late, so UiHud33.cs, the HUD's layout file, is untouched: the first cut (3f7f479c6) put the call in UiHud33.cs and the next commit moved it), UiBossBar43.cs (the STAGGER word: `Btext72.LabelWord`), Words71.cs / Voice71.cs
  (btext72 ON counts as their row ON: one `||` in each OffReason, their -skygrindNoWords71 / -skygrindNoVoice71 switches still win). With the row read ON every frame: (1) STAGGER's word is not drawn
  beside the unfilled meter (it is drawn nowhere else: the OPEN tag says the state); (2) while the fight line draws a line the bar's order is not drawn (one order at a time); (3) the bar's order is
  >= 22 px (x TEXT SIZE) in the bone colour; (4) a plate (the HUD's shadow ink 0x0A0E14 at 0.62 alpha, 18 x 6 px padding, sized to the text's preferred width, following the text's alpha) sits
  behind the fight line and behind the bar's order.
- Numbers: UnityTuning.N72B.cs (Btext72*). Tests: Assets/Tests/EditMode/Game/Btext72Tests.cs (written: label word, hide rule, counter, totals line; not run in the Editor).
- Proof line (Player.log, every 6 s of a drawn boss bar, at the bar's going down, a new boss, a new run and the quit; printed with the row on or off):
  `[btext72] <boss> | boss lines naming a weapon not held <w> frames (the bar's order a, the fight line b) | STAGGER drawn while not staggered <s> frames | two orders at once <t> frames (would be <u> without
  the row's hide) | smallest boss text <px> px | plates drawn <p> | held <h> frames | row btext72 on <n> of <m> frames | <why>, t <s> s (FeelClock)`.
  The slate's doneWhen reads: wrong-hand 0 and STAGGER-while-not-staggered 0 with the row ON, both > 0 OFF; smallest boss text >= 22; plates > 0.

### The Player step for the lead
Needs words71 / voice71 / bossText55 flipped first (the H train) or `-skygrindRows72 words71:1+voice71:1+bossText55:1` on both sides, then ON = those plus `btext72:1`, OFF = those alone (same build):
`bash Tools/night/n72-reels.sh n72-<k> btonK,n57-kiln,boss63,-skygrindRows72,words71:1+voice71:1+bossText55:1+btext72:1 btoffK,n57-kiln,boss63,-skygrindRows72,words71:1+voice71:1+bossText55:1` and the same on `n52-spire`
(btonC / btoffC). Read Logs/player/n47-reel-n72-<k>-bt*.log for the `[btext72]` lines (the quit's totals line) and the SPOT frames (crown windup / wyrm idle). The pair for the spotter:
`node Tools/night/n72-pair.cjs btext72 <bton wyrm-idle.png> <btoff wyrm-idle.png>`; the crown windup frame is the beam case.

### Not checked
Any Player frame (where the plate lands on the 1920 x 1080 canvas, whether `preferredWidth` of the rich-text fight line is the drawn width with a pad glyph sprite in it, whether TMP's outline and the plate read together);
EditMode; the crown's call "OPEN: SHOOT IT" and the gem counter (Crown40's own canvas) get no plate (they are not the fight line or the bar's order); bossText55's own code was not read (the lead flips it first).

## B4 spring72 (secondary motion on every enemy body)

### What the code shows (read, not measured)
Every enemy kind's view (husk, swarmer, spitter, charger, drone, warden ...) calls EnemyLook.React once a frame for its body: the hit reaction's offset / squash / tilt (NIGHT17-18's spring, crunch36, hitzone47's
head snap, fusion, nav). Idle: none (the pose is rigid between hits); wind-up: the husk pulses its colour and scale on a sine (windupPulseScale), no squash-and-release; death: Chunks.Break from the last drawn
pose (the break-apart is models41's / kill55's). The swarmer has foes47's crouch / leap / land springs (a row); nothing is shared. The bosses have bossMotion63 (Boss*View), wyrm71 and body72's lag.

### The change (render only; row design.n72.spring72, OFF in the config; the lead flips it after hitzone47 and models41)
- EnemyViews.Spring72.cs (hooks: EnemyViews.cs Frame -> Spring72Frame; EnemyViews.Hit.cs React -> Spring72React after Zone47React): per live body (game.enemies.all, by id), three layers on the body's whole squash, all
  stepped on frame time and VOLUME-PRESERVING (y by 1 + s, x and z by 1 / sqrt(1 + s): the product of scales is 1.000, pivoting on the feet), capped at +-0.2, not drawn under REDUCED MOTION / EFFECTS 0:
  idle breathing (2.2 % of the height at 0.7 Hz, the phase of the id), a wind-up squash with anticipation (a 6 Hz z 0.35 spring gathers the body down 12 % while Enemy.windupSteps > 0; the release overshoots
  into a stretch), a flinch follow-through (a hit = hp fell kicks a 5 Hz z 0.3 spring: an 8 % squash then a 3 % stretch, settled in 0.33 s). The springs run with the row off (D1); only the application waits.
  Death is not changed (the break-apart reads the last drawn pose, which now carries the squash): stated, not done.
- Numbers: UnityTuning.N72B.cs (Spring72*); the spring: Spring72.cs (Spring72Math), Spring72Tests (written; Python mirror: overshoot within 0.016 of the formula, steady lag 1.000; flinch / wind-up peaks 8 % / 3 % and
  15 % (dip) / 3.5 % (release stretch), volume product 1.0000 at sy -0.2 .. 0.2 [scratchpad flinch_check.py, spring_check.py]).
- Proof line (Player.log, every 5 s of drawn bodies, row on or off): `[spring72] row spring72 on|off: <b> body-frames, applied <a>, scale rms applied <r> | the springs' own <o> (idle breath peak, wind-up squash peak
  at <kind>, flinch squash peak at <kind>) | volume product <lo>..<hi> of the applied`. OFF: applied 0, rms applied 0.0000, product "-"; ON: applied > 0, product 0.9999..1.0001, rms > 0 (the slate's
  doneWhen: the transform log's idle and windup motion RMS > OFF; the product of scales 1 +- 0.02; hitboxes and fingerprints unchanged: no sim read for writing).

### The Player step for the lead
`bash Tools/night/n72-reels.sh n72-<k> sk1on,<a survival or campaign map>,kill12,-skygrindRows72,spring72:1 sk1off,<the same map>,kill12` (kill12 is in FeelReel.cs's PhaseA; I did not read which map it expects); read the `[spring72]`
lines in Logs/player/n47-reel-n72-<k>-sk1on.log (applied > 0; flinch peak > 0 after the kills' first hits) and -sk1off.log (applied 0), then the pair: `node Tools/night/n72-pair.cjs spring72 <on frame> <off frame>` on the frame just
after a hit (the squash) and one idle frame. The fingerprints: the smoke's baseline checks (render only).

### Not checked
Any Player frame; whether 2.2 % breathing reads at range; the husk's own windup scale pulse (windupPulseScale) stacked on the spring's squash (both draw: one is a sine on the root, the other this spring); kinds whose
views do not apply the squash to the root (not read for every view); a boss63 windup pair (bosses are not in game.enemies: their windups are bossMotion63's and body72's).

## B5 kill feel: kicks71 and finish55, and the bosskill72 reel

### What the code shows (read, not measured)
- kicks71 (Kicks71.cs, Feel46View.cs): built and probe-GREEN in NIGHT71 (ledger wave-71: "probe GREEN, SPOT not aligned"; L-156: two frames of one same-run pair from different moments named HUD state, not the kick).
  The row is in FrameRows, so `-skygrindSameRun63,n71.kicks71` gives an aligned pair at one sim state. What the code does NOT do: a boss's death is a KillEvent of kind "boss" (BossKit.Down) that the kill ladder reads as
  a plain BODY kill (not melee, not a headshot): 2.2 deg and a 1.2 deg FOV punch-in, the SMALLEST tier on the biggest kill in the game.
- finish55 (design.n55.finish55, "the boss-kill finish: the freeze frame at a boss's death (a comic-panel beat with the JoJo turn)"): its config block has `numbers: {}` and no code anywhere in Assets/Scripts
  (grep: only the config name and two header comments in SurvivalGame.N55K.cs / UnityTuning.N55K.cs). NIGHT55's K never wrote it. GG's 2026-10-20 JoJo words (OWNER.md:115) ask for "its own distinct color palette
  and lighting identity ... similar to how JoJo's Bizarre Adventure uses dramatic color changes": each boss room's grade, not a kill beat.

### The change (view only; row design.n55.finish55, OFF in the config; the lead flips it)
- Finish55View.cs (self-installing, hooks the bus: bossDying, kill kind boss, bossDown; the camera work on g.OnRender as Feel46View does): with the row read ON every frame, a boss kill gets a camera kick of 6 deg
  (attack 0.03 s, recover 0.55 s; above the ladder's biggest, the punch kill's 3.6) and a 3.5 deg FOV punch-in (recover 0.5 s), both x EFFECTS INTENSITY and the CAMERA SHAKE / FOV KICK settings, none under REDUCED MOTION;
  and a comic panel: two letterbox bars (8.5 % of the height each, the HUD's shadow ink at 0.92 alpha, a 3 px bone hairline on the inner edge) slide in over 0.12 s from bossDying (or the kill), hold through the
  finisher and 0.6 s after bossDown, slide out over 0.35 s: one slide in, one out, no loop or flicker; under the HUD canvas (order 9 < 10). The beat's state runs with the row off (D1).
  NOT built: a JoJo "colour turn" (a full-screen colour flash breaks "no new full-screen flicker"; GG's JoJo words are the rooms' palettes: L / C's). Stated, not hidden.
- FeelReel.N72.B.cs: the pre-placed phase bosskill72 (partial N72BossKillBody): boss63's front (campaign run, walk-in to the rise), the fight 6 s from the rise (taps at the weak point), then the reel's action (the
  boss's health set to 2 % of its pool, its stagger forced, re-forced every 120 frames if the window closes), taps until it dies; frames shot-N30-BK72-<boss>-{pre, dying, down, after}.png; the lines
  `[bosskill72] <boss> hp ... (forced)`, `kill: bossDying at <ms> ms, kill event (kind boss) at <ms>, bossDown at <ms>`, `feel: kicks71 on|off, kill kicks <n>, fov punches <n>, last kick <deg> | finish55
  on|off: beats <n>, kick peak <deg>, fov peak <deg>, bars peak <share>` and a verdict (GREEN: finish55 on and a beat, a kick peak over half 6 deg, the bars at full; OFF: finish55 off and nothing drawn; RED else).
  Bosses: wyrm, crown, wick, loom (the health body is known); THE ICEBREAKER: SHORT said (Brawl40's own death state).
- Numbers: UnityTuning.N72B.cs (Finish55*, BossKill72*).
- kicks71 "made right": no code change to it. Its remaining gap was the SPOT's alignment (a reel / lead step), and the boss kill's tier, which finish55 now carries when it is ON; with finish55 off a boss kill
  still reads as a body kill, as NIGHT71.

### The Player steps for the lead
1. Boss kill, both rows (one Player per side, L-046): `bash Tools/night/n72-reels.sh n72-<k> bkon,n57-kiln,bosskill72,-skygrindRows72,finish55:1+kicks71:1,-skygrindSameRun63,n71.kicks71 bkoff,n57-kiln,bosskill72,-skygrindRows72,kicks71:1`
   (and `bcon,n52-spire,...` / `bcoff,n52-spire,...` for THE CROWN). Read Logs/feel/n72-<k>/bkon/bosskill72-wyrm.txt: the verdict line (GREEN with finish55 on, OFF without), `kill: bossDying at ... bossDown at ...`, `feel:` (beats 1, kick peak ~6.00 deg,
   bars peak 1.00 on; 0 / 0 off), and the four frames; Logs/player/n47-reel-n72-<k>-bkon.log for `[finish55]` lines. Pair for the spotter: `node Tools/night/n72-pair.cjs finish55 <bkon down.png> <bkoff down.png>`.
2. The kicks71 aligned pair on plain enemies (L-156): `bash Tools/night/n72-reels.sh n72-<k> kk,<a survival map>,kill12,-skygrindRows72,kicks71:1,-skygrindSameRun63,n71.kicks71`, the frame at the kill's first held frame and its <side>-off half.
   Read kicks71's probe line (Feel46Probe: hit kicks / kill kicks / merged) in the log.

## ROUND 2: the lead's t1 verdicts (n72-t1, afd66240c; Logs/n72/verdict-{btext72,bossfx72,body72,spring72}.md; read 2026-10-27 12:12)

All four are SHORT. Each is answered below with the cause found in the lead's own files, the change, the bar set before any round-2 frame, and the Player step. No Player was run.

### B3 btext72: the ON line is nine words in lower case (by a split on spaces it is ten: Swap, to, the, gauntlet, then, swing, at, its, charging, head; my first test pinned "nine" from the verdict and failed on t2; fixed 12:50, the table's 24 lines all pass a Python mirror of the rule: words <= 6, a verb in capitals, the token kept, SWAP where a weapon is named)
Cause: the ON line "Swap to the gauntlet, then swing at its charging head." is words71's config variant (design.n71.words71.numbers.lines, the lead's file). btext72 had no line of its own: it only
hid, sized and plated whatever words71 carried. Change: Btext72Lines (UnityTuning.N72B.cs; the lead's config is untouched): eight orders, each with a held line and a swap line, at most 6 words, the verb in CAPITALS, the
glyph as a token (resolved to the bound key or the pad glyph by the HUD's own resolver). Wired through words71's own variant path (UiWords71.cs: with btext72 on, `Btext72.Order(raw, held)` is consulted
before `Words71.OrderFor` for the bar's order and the rule line), so one writer sets the line and nothing flip-flops. The wyrm: "SWING gauntlet {fire} at the head" with the gauntlet in hand, "SWAP to gauntlet, then
SWING {fire}" without it; THE ICEBREAKER "GET CLOSE, SWING gauntlet {fire}"; THE WICK "PARRY {punch} yellow embers, then SHOOT"; THE LOOM "HOOK lit knee {grapple}, GRAPPLE away"; THE CROWN "JUMP {jump}, DASH {dash} through ring".
New counter: `boss lines over 6 words <n> frames ("<text>" (<w> words))` in the `[btext72]` line (a glyph or a key name counts as one word; the first long lines are named). Tests (written, not run):
Every_boss_line_in_the_table_keeps_the_rule (every line: words <= 6, a verb in capitals, the token kept, SWAP where a weapon is named), The_wyrm_order_names_the_hand_you_hold, The_word_count_and_the_caps_verb_rule,
The_counter_remembers_a_long_line_once. Python: not applicable (text). Re-pair on the kiln: ON must read the SWING / SWAP line above; the `[btext72]` line's "over 6 words" 0 on the five bosses' rule lines and orders,
"STAGGER drawn while not staggered" 0, "smallest boss text" >= 22.
Not covered: lines that are not in the table (a boss hint I have not seen; the crown's window call): the counter names them (they print under "boss lines over 6 words").

### B2 bossfx72: the ring and the gems still sit on the crosshair
Read: the reel aims at the Crown's weak point, the ring's TOP (BossAttackClock63: centre + up x ringRadius), so the ring and its top gems are at the crosshair by construction; the ring is the Crown's body,
a line, not an effect. What sat on the crosshair as a burst was the gems: t1's idle frame (bfalone-sp) shows a flat cyan blotch of 4-5 touching gems across the crosshair (gem 0.68 m + cross + halo at gain 1.4 clipped
to flat 255 cyan). Meter (n72-crownfx.py, bar set before any round-2 frame): AIM CLEARANCE = the glow share of the 120 px disc round the crosshair that is MASS (survives a 41 px opening: a gem's blotch, a halo; not a
ring segment or a blade), bar <= 5 %. t1 ON (bfalone-sp): idle 10.4 %, stagger 4.7 %, flinch / tell / windup 0 (worst 10.4, FAIL); t1 OFF (bpoff-sp): idle 55.3 % (evidence/crownfx-t1-aimmass-baseline.txt; the
first, plain-glow reading, 27.3 % idle, is evidence/crownfx-t1-bfalone-bpoff.txt). Change (Crown37View.Crown72.cs, numbers in UnityTuning.N72B.cs): gems 0.5 of the gap (0.47 m, was 0.68), halos 1.2 x gem (0.56 m)
at half the gain, the gem's own colour x 0.7 (no flat clip), and AN AIM WINDOW: a gem, its cross and its halo within 300 px (at 1080p) of the screen's centre ease down to 0.12 of their colour inside 100 px (a dim
core stays; the torch's spray rule, banned 16, did the same), computed from the gem's screen position after DrawMotion63 so it is final. The ring and the blades are not touched. New log lines: `[fx72] crown row bossfx72
on: ... aim window 100-300 px floor x0.12` and, every 2 s, `[fx72] crown aim window: <n> of <m> gems inside 300 px of the crosshair, the dimmest at x<k>`.
Honest limit: the ring (thin cyan arc) will still cross the crosshair in the reel because the reel aims at the ring's top; the gems at the crosshair read as dim cyan cores, not blotches.
The Player step: `bash Tools/night/n72-reels.sh n72-t2 bfalone-sp,n52-spire,boss63,-skygrindRows72,bossfx72:1` (and the OFF side as t1's bpoff-sp), then `python -I Tools/night/n72-crownfx.py <side> <the five crown frames>`:
AIM CLEARANCE <= 5 %, coverage <= 15 %, visible >= 4 of 5, tell frames' danger-hue share <= 20 %.

### B1 body72: the ON glow under the centre platform is yellow
WHICH ELEMENT: the worm's own pieces and their seams, not a floor beam. At f0100 (the reveal) the worm stands as a vertical stack of pieces in front of the arena's own pillar; the thin yellow bars (x 920-955)
are the pillar (in both frames). In the OFF frame the stack is near-black ash with thin amber seams: a small red-orange patch at (868, 595) is its top piece; in the ON frame the same place is a wide box-by-box
column: the pieces, lit, with the seams over them ([boss63] rms wyrm: 8 parts at s1, 17 at s2: the worm is up). CAUSE of the colour: the kiln's grade zeroes blue (the ON frame reads blue 3.4 / 3.5 / 13 in the wall,
the dark wall and the floor), so my NEUTRAL ash (0x5c4c46, R about G) rendered R = G = yellow, hue 46.9 deg median with 52.4 % of its bright pixels in the parry band 45-70 (OFF 30.9 deg, 41.1 %): the parry tell's
colour on a body. Change: Body72Hex 0x7a3626 and the head 0x8a4430 (red-weighted: about (165, 73, 3) after the shader's 1.35 and the grade: hue 25 deg, luma about 87 against the wall's 40-60), and the seams through
BossColours63.Hot (the gain first, then the danger route: clip-safe) instead of Part. Meter (n72-wyrmpx.py --hue, bars set before any round-2 frame): median hue <= 35 deg and yellow-band share <= 42 % (the plain
config's 41.1 % rounded up; a first bar of 15 % failed the OFF frame itself because its seam cores clip yellow in the plain game: changed here, before any round-2 frame) on the box 858,590,912,760: t1 ON 46.9 deg /
52.4 % FAIL, t1 OFF 30.9 / 41.1 % PASS (evidence/wyrmpx-t1-hue.txt). The Player step: `bash Tools/night/n72-reels.sh n72-t2 bbalone-ki,n57-kiln,boss63,-skygrindRows72,body72:1`, then
`python -I Tools/night/n72-wyrmpx.py --hue t2-ON <f0100 png> 858,590,912,760`. Not changed: the tell glow is the windup's `Body72Trim` (already Hot, danger).

### B4 spring72: no [spring72] line in the kill12 logs
Cause: the line printed every 5 s of body-frames; kill12 is 150 frames at 60 fps (2.5 s: Logs/player/n47-reel-n72-t1-sk1on-ki.log, "kill12: 150 frames"), so the clock never fired. Not a spring that did not run
(the log shows `[rows72] spring72=1`). Change (EnemyViews.Spring72.cs): the interval is 1 s (Spring72SayEveryS), and three lines that cannot be missed: `[spring72] first body: <kind> id <n>, row spring72 on|off, motion
allowed|off, frame <f>` on the first body of a run, `[spring72] flinch kick <k>: <kind> id <n>, hp a -> b, speed <v> | row spring72 on|off` on each of the first 6 hits, and the totals line on the quit and on a new run.
The totals line now also prints in OFF (applied 0). The motion read: the springs' own rms and the applied rms (OFF 0.0000, ON > 0), the peaks, the volume product. A still cannot show spring motion (the lead's note):
this line is the measurement; a clip needs a reel that records transforms (not built: said). The Player step: `bash Tools/night/n72-reels.sh n72-t2 sk1on-ki,n57-kiln,kill12,-skygrindRows72,spring72:1 sk1off-ki,n57-kiln,kill12`;
read `first body`, `flinch kick`, and the totals line in both logs (ON applied > 0 and volume product 0.9999..1.0001; OFF applied 0).

## ROUND 2b: leftover 8 and the H5 check, from the lead's n72-h0 telemetry (read 2026-10-27 12:36; no new Player step)

The lead's verdicts name B for these: reticle42 / pump40 ("the reticle's spin and the pump's cycle are motion; the still frame cannot show them: B owns the rework if a motion check is asked for") and hitzone47 /
models41 / kicks71 ("B's H5 check take it from here"). A still cannot judge them; the reel's telemetry.jsonl can (every frame records the camera, the reticle's spin and kind, the pump's phase, the enemy's rect).
Tool: Tools/night/n72-telemetry.py (cam, enemy, reticle, pump, spinlen, pumplen, keys; planted case PASS 12:34). Evidence: evidence/telemetry-h5-h8-n72-h0.txt (commands, time, numbers).
- kicks71 (H5, kill12 on n57-kiln, h5on-ki with hitzone47 + models41 + kicks71 against h5off-ki): the camera differs from OFF on 13 frames (0.22 s), first at frame 78, the kill; ON minus OFF pitch +1.90 deg at
  frame 78, +2.19 at 79 (the peak), +2.10 at 80, +1.94 at 81, +1.73 at 82, back to 0 by frame 91; the FOV is narrower by 1.17 deg at most over 9 frames. That is kicks71's own numbers (bodyKillDeg 2.2, bodyKillFovDeg
  1.2), measured. THE SPOT PAIR WAS MISALIGNED (L-156 again): the pair was f0100, 22 frames after the kill, 10 frames after the kick's last frame (90); n72-prune keeps only every 50th frame, so f0079 (the peak) was written and
  deleted. The aligned pair is frame 79 (peak), 78 or 80 around it. New: `bosskill72` with `-skygrindKill72 <m>` is kill12's kill with SHOTS at the kick (FeelReel.N72.B.cs Kill72Scenario: K72-kill+0, +1, +2, +5,
  +15, each with the OFF half of the same instant under -skygrindSameRun63,n71.kicks71; shots are kept by the prune).
- hitzone47 and models41 (the same pair): the enemy's screen rect is identical on all 78 frames before the kill (height rms 0.00 px both): kill12 kills on the first hit, so the hit reaction hitzone47 draws (a
  head snap, a knee buckle) never has a frame; the instrument is the wrong one for it. The right one is hit12 (a hit that does not kill): `n72-telemetry.py enemy` reads the rect's height / centre ON against OFF there.
  models41's break-apart is in the kill's chunks (a still at the kill's first frames, the K72-kill+0/+1 shots show it). Stated, not guessed.
- reticle42 and pump40 (H8, verbs37 on n52-span): the telemetry reads spin 0 and spinKind 0 on all 1800 frames of BOTH sides and pump -1 on all 1800: the verbs37 reel never records a reticle turn or a pump,
  so H8 measured nothing of either row: the verdict's "no visible change" is "not measured", not "no change". The telemetry itself works on a reel that does fire (kill12: spinKind 2 at t 0.45 s for 0.73 s, spinKind
  3 at 1.30 s for 1.20 s, identical ON and OFF because reticle42 is not on there: the tool tells an unchanged run). The instruments that fire them: `weapons42` (the KESTREL tap's turn and the cannon turn:
  its own bar "over by 0.27 s, before 0.34 s; the cannon by 0.46 s, before 1.82 s") and `weapons41` (the THUNDERCLAP volley and its pump; pump40: delay 0.2 s, duration 0.22 s). Not run.
- The Player step, if the lead wants these three decided: (1) `bash Tools/night/n72-reels.sh n72-<k> kk1on,n57-kiln,bosskill72,-skygrindKill72,12,-skygrindRows72,kicks71:1+hitzone47:1+models41:1,-skygrindSameRun63,n71.kicks71
  kk1off,n57-kiln,bosskill72,-skygrindKill72,12` (the pair frame: Logs/feel/n72-<k>/kk1on/shot-N30-K72-kill+1.png against its same-run OFF half; kill+2 is the peak), (2) `r42on,<a campaign map>,weapons42,
  -skygrindRows72,reticle42:1` against `r42off,<same>,weapons42`, then `n72-telemetry.py spinlen`; (3) `w41on,<map>,weapons41,-skygrindRows72,pump40:1` against plain, then `n72-telemetry.py pumplen`;
  (4) `h12on,<map>,hit12,-skygrindRows72,hitzone47:1` against plain, then `n72-telemetry.py enemy`.

## ROUND 3: the lead's t2 verdicts (n72-t2, ba47bb280; Logs/n72/verdict-{body72,bossfx72,spring72}-t2.md; read 2026-10-27 13:40)

### B2 bossfx72 (SHORT only for want of the aim meter on the t2 frames): the meter, run
`python -I Tools/night/n72-crownfx.py bfon2 Logs/feel/n72-t2/bfon2/boss63/shot-N30-B63-crown37-{idle,flinch,tell,windup,stagger}.png` (and bfoff2); output evidence/crownfx-t2-bfon2-bfoff2.txt (13:38). The bars were fixed before
these frames (aim burst mass <= 5 % of the 120 px disc, coverage <= 15 % of the 300 px disc, body visible in >= 4 of 5, tell frames' danger-hue share <= 20 %). ON (bfon2): AIM 0.0 % on all five (bar 5: PASS; t1 ON
worst 10.4 %), coverage worst 6.2 % (bar 15: PASS; OFF 24.2 %), visible 4 of 5 (the tell frame is the one not visible: its cyan body is 12,789 px under the 15,000 bar, the red column crossing the ring; bar 4 of 5: PASS),
tell frames' danger-hue share 7.9 % (bar 20: PASS; OFF 26.5 %). OFF (bfoff2): aim 55.3 %, coverage 24.2 %, visible 0 of 5, share 26.5 %: all four FAIL. By eye (bfon2 idle and tell): a cyan ring with its blades and small gems,
the gems under the crosshair dim; the red column is narrower and the ring reads through it. The "bursts" part of the doneWhen is met by the pre-committed bar.

### B4 spring72: PASS on its line (the lead: applied 61 ON / 0 OFF, volume product 1.0000). No change. The clip (a transform-recording reel) is still not built: said.

### B1 body72 (SHORT: ON median hue 43.9 deg, yellow 47.6 % on the t1 box)
What the t2 crop shows (Logs/n72/pairs/body72-t2/on.png against off.png, 2x, 13:40): the wide orange column is the worm's pieces (the red-weighted body works: hue about 25); two things are still yellow. (1) The bright vertical strip
at x 882-905 is the ARENA'S OWN pillar beam, bright yellow in both frames and continuous with the beam above the platform: it filled 41 % of my box in the plain frame, so the box-hue bar (yellow <= 42 %, the plain frame's reading)
left no room and could not be passed by any change that is not darker: a measuring flaw of mine, said. (2) A golden box on top of the column (hue about 49): the top piece with the Enemy shader's rim: the config's
fx.enemyRimColor (255, 154, 82) x 1.6 x CrunchRimBoost 1.5 x up to 1.6 with distance (the wyrm was 29-30 m away) is about (3.8, 1.2, 0.3) HDR, which on an orange body clips G to 255 = yellow; plus the additive seam plates of ten
stacked pieces summing R and G (each Hot plate is clip-safe alone, not stacked).
Meter: Tools/night/n72-wyrmpx.py --changed <label> <on.png> <off.png> <box>: only the pixels body72 changed (any channel > 40 apart, ON pixel bright), their count, median hue and yellow-band share, bars set BEFORE any
round-3 frame: >= 800 px, median hue <= 35 deg, yellow share <= 15 %. Readings (evidence/wyrmpx-t2-hue.txt): t2 on the box 858,590,912,760: 4,341 px, median 27.8 deg, yellow 17.5 % (FAIL on the share by 2.5 points);
t1 (the neutral ash): 4,702 px, 35.9 deg, 22.3 %; the whole column box 800,470,1060,800: 5,903 px, 29.8 deg, 23.5 %. The old --hue mode stays (t2 ON 43.9 / 47.6 %, OFF 30.9 / 41.0 %).
Change (WyrmView.cs, WyrmView.Body72.cs, UnityTuning.N72B.cs): with the row on (a) every piece's rim is the danger red (Tell36RedHex linear x Body72RimGain 2.0) through its property block `_RimColor`, not the config's orange;
(b) the seam plates are drawn OPAQUE (the neon opaque material, not the additive one) with the clip-safe Hot colour, including the floor trail: opaque plates cannot stack into yellow; the material goes back to additive the
frame the row reads false. Not run. The Player step: `bash Tools/night/n72-reels.sh n72-t3 bbon3,n57-kiln,boss63,-skygrindRows72,body72:1`, the OFF side as t2's bboff2, then
`python -I Tools/night/n72-wyrmpx.py --changed t3-changed <bbon3 f0100.png> <bboff2 f0100.png> 858,590,912,760` (bar: >= 800 px, <= 35 deg, <= 15 %); the wind-up pair as before.

### B5 bosskill72 -skygrindKill72 on the kiln: SKIPPED "no open floor at 12 m" (0.5 s wall)
The ordinary-kill mode did kill12's PlaceAt but not kill12's fallback (the player moved to other open floor nearby, UnityTuning.ReelMoveTries, which is how kill12 itself got its husk on the kiln); copied now
(FeelReel.N72.B.cs). Not run. The spire is the lead's other option; either: `kk1on,n57-kiln,bosskill72,-skygrindKill72,12,-skygrindRows72,kicks71:1+hitzone47:1+models41:1,-skygrindSameRun63,n71.kicks71`.

### Not checked
Any Player frame (whether the bars and the kick read; whether 6 deg is too much on a boss with a body-kill 2.2 beside it); that bossDying precedes the kill for every boss (the beat starts on whichever comes first);
that the reel's forced hp reaches a death on the Crown inside 60 s (its window closes; the re-force is a guess); BossKit's other boss ids; EditMode (nothing pure to pin: the bars are UI).

## ROUND 4: the lead's t3 verdicts (n72-t3, aec97e33d; Logs/n72/verdict-{body72,btext72,finish55}-t3.md; messages 14:2x)

### B1 body72: PASS candidate (the lead: changed-pixel meter 3,924 px, median hue 25.4 deg, yellow 1.5 %: my round-3 bars 800 / 35 / 15 met). No change.

### B3 btext72: SHORT by one word (the lead's count: "SWAP to gauntlet, then SWING LEFT CLICK" is seven words as displayed)
My flaw, said: the table, its test and the HUD counter (UiBtext72.Words72) all counted a glyph token as ONE word, and the counter even collapsed the resolved "LEFT CLICK" into one; on a keyboard {fire} reads LEFT CLICK (UnityTuning.
Lesson25Aliases), two words. So a table line that read "six" read seven, and the runtime counter could not see it. Change (455f648de): the swap line is "SWAP gauntlet, then SWING {fire}" (the lead's own example); the held
lines that carried "gauntlet" and {fire} dropped the weapon's name where they were seven ("SWING {fire} at the head", "SWING {fire} at glowing head", "SWING {fire} as it comes"; "GET CLOSE, SWING gauntlet {fire}" is six);
Btext72.DisplayWords counts a Lesson25Aliases token in its words; Btext72Tests holds every held and swap line to 6 displayed words; the HUD counter counts a sprite tag as one word, a key name as one, LEFT CLICK as two,
other rich-text tags as none. Tools/night/n72-btext-words.py mirrors the count on the C# table (a Python mirror, not the C# run): evidence/btext-words-t3cut.txt (14:23): planted PASS (the t3 line reads 7, the cut line 6),
all 24 lines (12 rows, held and swap) at most 6 as displayed on a keyboard (5 on a pad for the {fire} lines), a verb in capitals in every line. `bash Tools/night/dn-check-wt.sh` 0 errors. EditMode not run (a builder runs none).
Re-pair: `bash Tools/night/n72-reels.sh n72-<k> btx4,n57-kiln,boss63,-skygrindRows72,btext72:1`; read `[btext72] ... boss lines over 6 words 0 frames` in the log (the counter now counts LEFT CLICK as two).

### B5 finish55 / bosskill72: SHORT (the lead: kk3 / kk4 killed a HUSK; "kill kicks 0, last kick 0.00 deg"; finish55's boss kick never fired)
Cause, said: I gave the lead `bosskill72 ... -skygrindKill72 12` for the kill-timed kicks71 frames; that switch selects Kill72Scenario, kill12's HUSK kill, where finish55 (a boss-kill row, started by bossDying) cannot fire.
The boss kill is the same phase WITHOUT the switch (BossKill72Scenario), which no Player had run; reading it against the sim found a second defect: THE KILN WYRM puts its health back at a phase threshold when a hit lands under
it (Wyrm.Apply: p1 under phase2At -> EnterBeat resets hp to 66 %, p2 under phase3At), so the one-step "set hp to 2 %" would have come back at 66 % and the wyrm would not have died in 60 s (FeelReel.N24.Wyrm's wyrmdies steps
the phases for this reason). Change (FeelReel.N72.B.cs, UnityTuning.N72B.cs): (1) Wyrm72Descend: in p1 / p2 (p3 where phase4At exists) hp to BossKill72StageBelow (0.9) of the next threshold and Apply(0), the sim's beat, until the
last phase; then the 2 % kill share and the forced stagger as before; (2) Kill72Apply: if the taps have not killed it BossKill72ApplyAfterS (6 s) after the share is set, the boss's own public Apply for the last points (wyrm,
crown, wick, loom), said in the line (`death by the reel's Apply` / `the taps`); (3) the husk mode's line now says it is a husk kill and finish55 is not measured there. `dn-check-wt` 0 errors (14:27). Not run in a Player.
The Player step (one Player per side, L-046; the finish55 row is OFF in the repo, so ON is `-skygrindRows72`):
  the kiln: `bash Tools/night/n72-reels.sh n72-<k> bkon,n57-kiln,bosskill72,-skygrindRows72,finish55:1+kicks71:1,-skygrindSameRun63,n71.kicks71 bkoff,n57-kiln,bosskill72,-skygrindRows72,kicks71:1`
  the spire: the same with `bcon,n52-spire,...` / `bcoff,n52-spire,...`.
  Read Logs/feel/n72-<k>/bkon/bosskill72-<wyrm|crown37>.txt: `[bosskill72] wyrm stage: p1 -> beat/p2 ...` (the kiln only), `hp ... (forced)`, `kill: bossDying at ... bossDown at ...`, `feel: ... kill kicks <n>, last kick <deg> |
  finish55 on: beats 1, kick peak ~6.00 deg, fov peak ~3.50 deg, bars peak 1.00`, and the verdict line GREEN (bkoff: OFF). Frames shot-N30-BK72-<boss>-{pre,dying,down,after}.png.
Not checked: that the wyrm's staged descent reaches phase 3 inside the 60 s cap (three beats of the sim's own length); that the crown's taps kill it before the 6 s Apply; whether `Apply(0)` is refused when lastPart is "tail" (a tail shed
pending: it would repeat each frame until the phase changes, said nowhere).
