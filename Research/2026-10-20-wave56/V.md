# NIGHT56 builder V: colours and readability (V1 colour56, V2 rim56, V3 yellow56, V4 grade55 round 3; the r1 fixes)

Builder V, worktree `worktree-agent-ad4ce01a176d99096` from the shared commit 6b1c00c9. No Player, Editor, build or reel was run by V: every
"after" below is what the code is built to do, not a measured number. Every "before" is measured by `Tools/night/n56-colour.py` on frames that
already exist in `Logs/feel/` (named per line). After every commit: `dotnet run --project Tools/SimHarness -c Release -- fingerprint` printed
`EQUAL to the baseline`; `bash Tools/night/dn-check-wt.sh` printed 0 errors before each code commit. No file under `Assets/Scripts/Core` changed.

## The colour meter (commit 174e2b7d): `python -I Tools/night/n56-colour.py <frame dir> ... [--kinds entry,mid] [--quiet]`
- No role-ID pass. Regions per frame come from the reel's own data: the HUD = the telemetry's `ui[]` rects (alpha > 0.05; a rect over 25 % of
  the frame is an overlay and stays in), the enemies = `enemies[]` rects (FeelProbe.Enemy), the gun = `fx.gun`, the sky = above the horizon row
  (y = h/2 + (h/2) tan(pitch) / tan(fov/2)); with an a356 sidecar, only the ray grid's open cells above the horizon (upper walls and ceilings with
  a collider are world). The 150 px screen-edge band (the hit / low-health wash, critic-ggproxy #10) is left out of the world and the sky and
  its warm share printed.
- Matching: a356 shots by their sidecar's telemetry frame index; a452 shots (no sidecar) by the camera's still segments in capture order (checked
  on n54-freeze df: 9 segments for 9 shots; n55-r1 di iceline: 17 segments for 18 shots because FeelProbe's 1800-frame ring cut the 6th room: the
  last shot has no telemetry, said on its line); verbs37 JPGs by their own line.
- Planted: `python -I Tools/night/n56-colour.py planted`: 7 / 7 as planted (one frame that passes every check; one each failing cover, sky,
  saturation, tint, yellow, rim) and the pairwise-accent check planted at a 20 deg gap.
- Thresholds that are the meter's, not a source's: KEY_TOL +-20 deg; the tint FAIL at >= 60 % of the world within 25 deg of the key at S >= 0.2
  or the world's luma IQR < 0.06; "= 0 %" sky read at the printed 2-decimal precision (< 0.005 %); yellow and sat ignore V < 0.15 / 0.30 pixels.

### Contracts I sharpened (built to the sharper one, the original printed beside it)
- **V2 (rim56)**: "the enemy rects' mean V and S above the world's p90" cannot be met by any pill: the world's p90 S is 0.92-0.98 on NIGHT54's
  a452 mid frames (the usable cyan and the hit wash), and a rect's mean includes the background round the pill. Sharper: each enemy in view
  (>= 15 % of its rect warm) passes when its BODY (the warm pixels of its rect) has mean V and S above the ENVIRONMENT's p90 (the world outside
  every role band). The original is the "original rect over world p90" number on the same line.
- **V1 sky**: above the horizon is mostly walls indoors; the a356 sidecar's ray grid makes "sky" the open cells only. On a452 frames (no grid)
  the horizon rule counts the foundry's orange-brown upper walls as "sky warm" 9-14 %: read those as wall numbers.
- **V1 coverage** is met by NIGHT54's frames already (foundry 6.0 %, kiln 5.7 %, iceline 60.9 % of the frame within 20 deg of tonight's keys at
  S >= 0.35, because the keys sit near hues the levels already wear). It does not tell ON from OFF; the on-vs-off key % / accent % on a356 and the
  blind pair do.

### RED baselines (before; n56-colour.py)
| what | frames | number |
|---|---|---|
| V1 sky warm (horizon rule) | n54-freeze df a452 foundry entry+mid | 9-14 % 6/6 FAIL (the orange-brown walls, decor54's ramp hue 22-29) |
| V1 tint | n55-r1 di a452 iceline | 11 / 12 FAIL (blue 210-230 on 40-50 % of the frame: the line is a blue tint already) |
| V1 world S vs tells | foundry / kiln / iceline a452 | world p99.5 S 1.00 on 20 / 24 (jade 150-170 on the foundry, 1.0) |
| V2 enemies (body vs env p90) | foundry + kiln + iceline a452 entry/mid | 22 of 38 (58 %); foundry 2 of 12; original rule 4 of 38 (11 %) |
| V3 yellow > 0.5 % | n55-freeze verbs37 x5 | 66 of 180 frames: foundry 24/36 (median 1.59 %: floor-edge trims), spire 30/36 (median 16.85 %, max 58.6 %: dark olive walls + the TORCH plate 2-6 %), span 8, iceline 3, kiln 1 |
| V3 a452 | iceline g7-mid | 46.5 % (the riders' firebox fills the frame) |

## V1 colour56 (commit 283cdc5f; spire walls in c094bde5): each level's KEY and ACCENT
Built: `Game/Colour56.cs` (row design.n56.colour56, view; off = NIGHT55 bit for bit). THE KEY: an unlit frieze (a copy of the neon, the
number's hue on screen whatever the level's light tint) along the room faces of every `wall` box, 3.4-4.8 m over the floor beside it
(UnityTuning.N56V.cs), above decor55's string course (2.7-3.02 m). THE ACCENT: colour56 owns the level accent (Decor55.Accent's chain:
colour56, then decor55, then yellow56, else decor54's): every decor54 accent piece and decor55's string course (built while colour56 is on even
with decor55 off). THE WALL RAMP: foundry (soot iron) and spire (slate) replace decor54's ramp (Decor54.ScriptFor's one hook line).

| level | KEY (frieze) | ACCENT (trims, rims, door frames, string course) | why (GG's eye decides: these are proposals) |
|---|---|---|---|
| foundry | #6b8c73 oxide green, 135 (round 2; r2 #39804b rendered S 0.98) | #a267e6 temper violet, 268 | colors.md's verdigris key wins over decor55's 72; the molten orange stays the only warm thing; walls to soot iron #1b1e21 / #545c66 (were orange-brown 22-29: the danger band on every wall) |
| iceline | #385194 ultramarine, 224 (round 2: V 0.58) | #86d9a2 pine / aurora green, 140 (round 2: S 0.38) | the line's own blue, deeper; the one colour the blue line does not wear; round 2: walls neutral #262729 / #7d8085 (the tint) |
| kiln | #944a5c rose clay, 345 | #5a82e0 cobalt glaze, 222 | THE KILN WYRM's level stays warm by S and value on the kiln's fired-clay hue (decor54's 324-340), 20 deg off the danger band; the wyrm and the vents keep the top saturation |
| span | #654a94 indigo-violet, 262 (round 2: V 0.58) | #8dd957 signal lime, 95 | colors.md's open-sky key; saturated, never the boundary's pale violet-white; round 2: walls neutral #262529 / #797780 (the tint) |
| spire | #688c3f absinthe, 88 | #e67793 rose, 345 (round 2: S 0.48) | the only arc >= 40 deg from the other four keys that is not a role hue is 75-95; saturated and bright, a band, not the low-chroma olive on every surface GG named (spire v6). Alternative for GG: jade 160 (10 deg off cyan's band). Walls to slate #1d1f24 / #6e7280 (the gold key turned decor54's warm grey to dark olive) |

Accents pairwise >= 45 deg (least: iceline 140 / span 95); every key and accent >= 18 deg from every role band (danger 5-35, yellow 45-70,
cyan 170-200, magenta 285-330). Risks I could not check: an accent on a bright HDR decor piece clips per channel toward a neighbour hue (the r1
critic saw decor55's 72 render 60): violet 268 could lean toward magenta, cobalt 222 toward cyan on the brightest rims; trimLum 0.6 covers the
floor trims only. Iceline's tint FAIL is the level's lighting (look49's azure rim / grade), not the frieze; it was 11 / 12 before.

**The reel (a356, FeelReel.N56.Colour.cs):** a452's entry and mid frames per room, each room its own telemetry, each shot a sidecar (the
telemetry frame, the horizon row, a 96 x 54 ray-grid sky mask). Per level 1-5:
`bash Tools/night/n56-reels.sh <tag> v1on,n52-foundry,a356` and `v1off,n52-foundry,a356,-skygrindOff56` (likewise n52-iceline, n52-kiln,
n52-span, n52-spire), rows on in the copy: `colour56` (and `rim56`, `yellow56` for their numbers on the same frames). Numbers:
`python -I Tools/night/n56-colour.py Logs/feel/<tag>/v1on/a356 --kinds entry,mid` and the off folder. Frames for the critic: one entry frame
per level on / off (the pair: "which has more colour direction"), a mid frame for V2.

### Round 2 from n56-r2 (the lead's a356 numbers; my reads of the same folders) -> ff8a2da5, 8cb6223f
- **The frieze rendered far above its number on the foundry**: authored #39804b (S 0.55) rendered S 0.98 (median RGB 0.01 / 0.40 / 0.10; the
  foundry's grade: look49's jade tint x saturation +23 x contrast 25 crushes red and blue on a green). The other friezes rendered near their
  numbers: iceline S 0.60 / V 0.28, kiln 0.54 / 0.49, span 0.50 / 0.32, spire 0.63 / 0.33; accents: foundry 0.65, iceline 0.86 (crushed),
  kiln 0.61, span 0.64, spire 0.76. Retuned (ff8a2da5): foundry key S 0.24 (#6b8c73; a power-law fit of the one sample puts it at ~0.75
  rendered: not measured), iceline accent S 0.38, spire accent 0.48, iceline / span keys V 0.58.
- **SAT FAIL on the OFF sides too: the world really has S 1.00 pixels with colour56 off**, and the meter also let two role families in. By the
  top-0.5 % pixels' hue (n56-colour.py now prints it): v1foff hue 85-90 (decor55's green-brass accent 72 rendered on the lit trims: decor55 on
  in the r2 copy), v1koff hue 141-145 (kiln greens at S 1.0, on and off: not identified; candidates the cyan strips' fringes or green pickups),
  spire hue 105-108 (the emerald-lit stone), iceline 155-167 and the kiln's 150-158 (the usable cyan's bloom and chromatic-aberration fringes),
  and pure-red lava / tells at hue 0-5 (outside the 5-35 band). The meter now leaves the danger family 350-35 and the cyan family 150-200 out of
  the world's sat check and prints colour56's own pixels separately: "key|accent S p99.5" (by hue window, which still catches the level's own
  same-hue pixels) and, new in 8cb6223f, "FRIEZE (exact, A/B)": each a356 shot has a twin one frame later with the frieze hidden, and the meter
  reads the frieze's exact pixels by the difference (coverage, rendered hue, S, V). **Sharper V1 sat contract**: colour56's own pixels (the
  frieze by A/B) at p99.5 S <= the tells' median S; the whole world's number stays printed (it is look49's grade, which colour56 does not draw:
  a world-wide saturation cut would be a whole-screen effect).
- **TINT FAIL iceline 12 / 12, span 6 / 6**: decor54's wall ramp is the key's own hue (iceline 0x161e2c / 0x5c6c84 S 0.30-0.50; span 0x161530 /
  0x46447e S 0.46-0.56) under look49's same-hue light: one hue on every surface. Iceline failed the same check before colour56 (n55-r1 di: 11 /
  12). Now (ff8a2da5) both walls go neutral (S 0.06-0.10, decor54's value ramp kept) so the frieze and the accent carry the colour on a grey
  ground. The light's own blue / violet (look49's rim and grade tint) stays: if the tint still FAILs, that is the next lever (PostFx / V49, not
  V's files tonight).
- **The key gaps the meter printed (27 deg)**: those are gaps to the role TOKENS (danger 12): kiln key 345 and spire accent 345 sit 20 deg
  outside the danger BAND (5-35) and 15 outside magenta's (285-330): never inside a role band (the slate: "never cyan, red-orange, yellow or
  magenta"). The meter prints both now. With four accents at 268 / 140 / 222 / 95, the 330-5 arc is the only one left >= 40 deg from all of
  them; a GG call if rose reads as red on the frame.

### n56-r4 (ff8a2da5 / 8cb6223f / 4d4eb6ea / 2d7c56c7 in it; my reads with n56-colour.py and a scratch A/B diff)
- The frieze by A/B difference (exact pixels): foundry 6.8 % of the frame, hue 130-140, S median 0.52 (r2: 0.98: the retune held); iceline
  5.4 %, hue 220, S 0.83; kiln 4.8 %, 340, S 0.53; span 3.5 %, 260, S 0.52; spire 6.3 %, hue 100, S 0.98 (look49's +28: 58efff9a sets 0 in
  r5). The key alone covers >= 3 % of every level's frames. (The meter's own A/B line reads 0 % on iceline / span r4 frames because the config
  now holds the swapped keys of 58efff9a: re-read on r5.)
- TINT: iceline 12 / 12 and span 6 / 6 PASS on r4 (r2 / r3: 0 / 12, 0 / 6): the neutral wall ramps did it before the key swap; foundry,
  kiln, spire 6 / 6.
- Whole-world sat: still FAIL on foundry / kiln / spire (look49's own saturation; 58efff9a's foundry / spire saturation 0 is in r5).
- THE WYRM (2d7c56c7, a155 b3on n52-kiln): THE WYRM'S TURN GREEN (the turn frame's walls H 22, whole frame 19; p1 17, p2 16); b3off RED
  (walls H 110: the acid green).
- V2 (rim56 r1 in r4): enemies over the environment's p90: foundry 2 / 14, iceline 7 / 18, kiln 9 / 15, span 16 / 16, spire 0 / 15.

## V2 rim56 (commit ac9dfb9e)
Built: `Game/Rim56.cs` (row design.n56.rim56, view): skin52's shader globals while the row is on (Enemy.shader unchanged, no model or
material change: rule 7): the shadow side x0.55 / 0.30 / 0.26 (skin52 0.20 / 0.15 / 0.30: the danger token's shadow ~#C4260A, V ~0.77 instead
of ~0.48), the lit side x1.2 (the red channel at the clip: hue ~12, S ~0.88 kept), the near rim's share 0.3 -> 0.55 at power 3.5. Row off or
skin52 off: skin52's globals (Skin52.Apply restores them). Not touched: the fog on enemies and the world's own saturation: on the foundry the
environment's p90 S is 0.84-0.97 (look49's saturated jade), above a pill's 0.85-0.9, so the S half of V2 depends on the world, not the rim.
Reel: a356's mid frames (above), `--kinds mid`.
**Why r3 did not move (6cb33d16, r5):** a356 printed the globals written, but the bodies' mean V moved +0.01 (kiln 0.559 -> 0.569, span
0.672 -> 0.668): skin52's ramp (the shadow side) runs only to 20 m and its near rim cut only under 14 m, most a356 enemies stand 8-20 m, and the
lit side was already at the red channel's clip (x1.2 moves nothing on screen). Round 2: the lit side x1.5 (G and B lifted with R past the
clip), the shadow side 0.70 / 0.38 / 0.30, and the enemy body material's own far rim (EnemyLook.body._RimColor x1.5, power 2.2 -> 1.7; restored
when off). What rim56 cannot reach: the post's EV and the fog darken every enemy (look49), and the S half of the contract fails wherever the
world itself sits at S 1.0 (foundry, spire: 58efff9a's look49 saturation 0 addresses that in r5).

## V3 yellow56 (commit c094bde5)
Built: `Game/Yellow56.cs` (row design.n56.yellow56, view). The viewmodel guns whose family colour (the web's weapons.families) sits at hue
28-75 with S > 0.5 (THUNDERCLAP edge #ffb000 41, TORCH fire #ff8c1a 30, BOUNCER blast #ffe14a 50) take S 0.32 (a pale brushed brass; hue and
value kept); muzzle flashes, sparks, pickups, the radial keep the family colour. A level decor accent in 35-80 (decor54's foundry brass 41,
spire 77) takes yellow56's level accent when neither colour56 nor decor55 sets one. Hooks: Viewmodel.FamilyOf (one line) and the THUNDERCLAP's
build line. The spire's dark olive walls (most of its "yellow" by HSV) move with colour56's slate ramp, not with yellow56.
Round 2 (4d4eb6ea; n56-r2 a356 foundry 6 / 6 over 0.5 %, 0.9-2.4 %, mostly V 0.25-0.4: the tan floors the grade turns ochre; spire v5-mid
23.0 %): the map stone's FLOOR vertices in the yellow arc (30-75) drop to S 0.18 (Yellow56Floors; hue and value kept; re-applied when a probe
vertex shows the stone was rewritten). The meter prints the bright share (V >= 0.5) beside the contract's: most of the foundry's "yellow" is
dark ochre (bright 0.07-0.98 %).
Reel: verbs37 x5 with the row on, `python -I Tools/night/n56-colour.py Logs/feel/<tag>/<side>/verbs37` per level (the "yellow per frame" line).
Not changed (GG's design, for DECIDE56): THE CROWN's yellow gems (crown47 gold / red-gold, CLook47GoldHex), the CROWN FIRE's yellow core
(Crown47YellowHex 0xffc94a), enemy visors (models43's amber 0xff9a3c), the parry tells themselves. The meter cannot tell a parryable
projectile outside an enemy rect from world yellow (no role pass): such a frame reads high.

## V4 grade55 round 3 and the r1 fixes
- **THE WICK's burn (145fd17d):** n56-r1 a155: "burns 0 -> 0 after 8 placement(s)" (wick55 on in the copy burns a ride only), patch S 0.22
  vs walls 0.66. Now: the placement suspends wick55's ride-only rule for the placement (the brush rule; restored at once) over 4 heights x 12
  angles; the burn is seen from across the shaft (0.55 R, 50 deg off the diameter; r2 looked from the spawn into the tunnel's wall); while a
  burn's heat >= hot.burnOpaqueMin 0.5 its iron quads draw OPAQUE in Stack37HotHex (hot.burnIron 1) behind the seams (an additive red-orange
  over the indigo iron summed to pink). Expected patch S ~0.88 (the token's): not measured.
- **The ride panels' cyan cue (7afdb2ea):** a155 prints, per shot, the wall cells within 0.35 m of a panel and their share in the cyan band.
  Printed, not gated (the panels fold in P1: expect them in turn / p2 only).
- **n56-r2 (the lead):** THE WICK GRADE GREEN and HOT WALL GREEN (burn S 0.935 vs walls 0.312, the most saturated class; the ride panels'
  cyan 0-8 %). THE CROWN GRADE RED (near danger p2 0.29; dH 11 / dS -0.094; tells kept 400 / 527 but the walls S 0.764 the most saturated
  other class): its one fix tonight is spent; the CROWN half closes SHORT unless the critic says otherwise.
- **THE CROWN (bf2dbee0):** r1: walls 4 deg from danger at the turn, near-danger 0.26-0.31, p1 vs raw dH 15 / dS -0.079. Now: a saturation
  delta per phase (p1 -12, p2 / p3 -10; never above 0), the split's balance swung to -15 at a turn's peak, deeper turn colours (p1 slate-blue
  [0.26, 0.32, 0.84], p2 sea-steel [0.24, 0.4, 0.8]), p1 balance 42 -> 48 for the ring's tells (r1 kept 0.80: the floor). THE WICK unchanged.
  The red "sky" planes over THE CROWN's arena (a155's sky class H 309-347, 47 % of the upper right at p1; absent before the fight) are not
  touched by the grade: the walls' and whole-frame numbers are what moves.
- **P0-decor (02ec805a):** decor55's foundry accent 72 -> oxide green 135 (the lit floor-edge strips rendered hue ~60); the moved floor trims'
  luminance x trimLum 0.6 (decor55's; colour56's own 0.6 while on) under usable cyan (r1: 3 of 12 entry frames over it). Decor54.FixAccent x
  Decor55.TrimLum (1 with both rows off).
- **THE WYRM's turn (2d7c56c7):** grade55 gains a warm WYRM palette (amber-umber shadows, turns 21-37 deg at balance 0) over grade54's
  acid-green highlights; a155 on n52-kiln forces the beat into P2. n56-r4: GREEN (turn walls H 22 vs OFF 110). Its shadows sit near the danger
  hue on purpose (GG NIGHT47); N55BossGradeTests' rule 2 (>= 60 deg from danger) is kept for THE WICK and THE CROWN and the wyrm has its own
  15-55 test.
- **Attempt 2 (99d0dc2d, the r2 critic):** THE CROWN's saturation delta back to 0 (r2: -12 dropped marginal tell cells, kept 0.76) and the shift
  from deeper steel-blue shadows (p1 ~220 S 0.76 at balance 50, p2 / p3 ~226); THE WICK's ride panels' cyan strips 0.16 -> 0.32 m with the row
  on (P1 folds the panels into the wall: no strip there by design). Not measured.
- **P0-step (step55):** not started (last priority): SHORT.
- **P0-decor:** closes SHORT (the lead): the kiln's accent at x1.4-1.6 vs off (3x wanted).

## Files touched in shared / other files (one line each)
- `Assets/Scripts/Game/Decor54.cs`: ScriptFor + `Colour56.Walls(...)`; FixAccent x `Decor55.TrimLum(cfg)`; Recolour returns
  `Colour56.Clamp(n)` (1e5f8286: a moved piece's largest channel held at 1.0 while colour56 is on, so the tonemapper's shoulder does not turn
  violet toward magenta or cobalt toward cyan).
- `Assets/Scripts/Game/Viewmodel.cs`: FamilyOf through `Yellow56.Family`; the THUNDERCLAP's build line through `Yellow56.Family`.
