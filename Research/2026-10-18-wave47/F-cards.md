# NIGHT47 builder F: the cards (round 0, for the lead's gate)

F owns G10-G24 and the TORCH (G43). Rows (pre-placed, off): sim `pulse47`, `overheal47`; view `fx47`, `grapple47`, `zoom47`, `hud47`,
`copy47`, `visor47`, `foes47`, `door47`, `lava47`. Reel phases a647 (N47Hud), a747 (N47Fx), a847 (N47Look). Research lines R1-R12 are in
`F.md` (Verified / Reported); "B" lines are tonight's baselines. Every number below is ours unless a research line is cited; render numbers go
in `UnityTuning.F47.cs` (one comment each), sim numbers in `design.n47.<row>.numbers` with a `_why`. Off = NIGHT46 bit for bit for every row.

## 1. The FX spec (G20-G24, G43 TORCH; rows `fx47`, `pulse47`, `grapple47`, `zoom47`)

**The weight ladder (all weapons; R7's uhiyama tiers, DESIGN-RULES C-1 / C-2 / C-6, NIGHT47.md section 3's camera targets).** Each tier must
be visibly bigger than the one below on a contact sheet (L-001); NIGHT46's kicks were judged "the same", so tonight's targets are the
measured camera punch peak (deg) and settle time (s) on the sheet:
| tier | shots in it | camera punch peak | settle | trauma add | hitstop (sim steps at 120 Hz; the sim's own, unchanged) | view hold | FOV punch |
|---|---|---|---|---|---|---|---|
| light | KESTREL tap, TORCH tick, fist jab | 0.5-1.0 deg | <= 0.35 s | +0.10 | 0-1 (C-2) | 0 | 0 |
| medium | KESTREL charged, PULSE swing, fist launch | 1.5-2.5 deg | <= 0.5 s | +0.25 | 2-6 | 0-30 ms | +0.8 deg |
| heavy | THUNDERCLAP shot (today 4.2 deg: already in the band, the code's heavy class), slug, the boost, the LT blast | 3-5 deg | <= 0.7 s | +0.5 | 8-20 (the blast: 9-11 = 75-90 ms) | 50-90 ms | +2-3 deg |
Rules for every layer: added in the order flash -> particles -> shake -> hitstop, one at a time, each toggled in the reel to see what it adds
(R7); the muzzle flash lives 1-2 render frames; an impact = sparks + a short light (the LightPools flash slot, never a new Light) + a small
mark; a kill = a world-space flare in the player's family (cyan; the gauntlet's pink); everything scales with EFFECTS INTENSITY, is off under
reduced motion, never covers the reticle's centre box (10 % of the screen), never fills the screen (GG w30), and flashes stay under FlashGate's
3 a second (C-6). The camera kick is rotation only, trauma squared, decay ~1.5 / s (NIGHT5), the kick against the aim and the gun returning
on a spring with a snap (R8). View randomness from a seeded cosmetic generator (never UnityEngine.Random in the sim; the view uses its own
Mulberry32 seeded per run).

### 1a. Per weapon: today (B6, code inventory) and after (fx47)
Today's facts (file:line in B6's inventory): the muzzle star on every `fire` but the gauntlet and the fist (`WeaponFx.cs:455-464`, 2 render
frames); a muzzle light 6 x LightScale, 7 m, 2 frames (`LightPools.cs:179-184`); CrunchShake trauma light 0.06 / medium 0.22 / heavy 0.42
(`UnityTuning.Crunch.cs:171-189`) plus the web trauma 0.06 / 0.15 / 0.3; feel46's kicks are **off** (`feel46.numbers.kicks` false); the
sim's hitstop is a world freeze (ImpactRules: n42 impact42). Weapon ids: KESTREL `revolver`, THUNDERCLAP `shotgun`, PULSE `gauntlet`, TORCH
`torch`, fist `punch`.

| weapon / action | today (key numbers) | the gap GG names | after (fx47; tier) |
|---|---|---|---|
| KESTREL tap (VERBS 25) | star ~0.19 m cyan; tracer 0.014 m x 0.06 s; 5 spark cubes + a Feel43 white star 0.55 m / 0.05 s; camera punch 1.8 deg (6 Hz, cap 3.5); hitstop 0.024 s on a body | small and thin: a 1.4 cm tracer for 60 ms | light tier at the top: punch 0.9 deg, settle 0.3 s; the tracer 0.03 m core + 0.09 m glow for 2 frames then a 0.12 s fading smoke line; impact: 8 sparks + a 0.06 s light (pool slot) + a 0.25 m scorch mark (decal, 8 s, max 32); a kill flare (cyan, 0.5 m, 0.12 s) |
| KESTREL charged (VERBS 27) | the LONGWAVE beam 0.28 s, radius 0.09 -> 0.02, x2.5 punch, +0.35 trauma, +3 sim steps | a beam, but no recoil body on the gun | medium-heavy: punch 2.5 deg, settle 0.5 s; the gun's barrel glow (G19's charge read) dumps to white in the shot frame; a 0.4 m ring at the muzzle; the beam keeps; pierced bodies each get a 0.08 s flash |
| THUNDERCLAP shot (VERBS 29) | star ~0.29 m; 8 tracer lines 0.014 m; per-pellet star 0.45 m (max 3 a frame); punch 4.2 deg (4.8 Hz, cap 6); vm kick back 0.17 m, pitch -7, yaw -6; hitstop 0.036 / 0.06 s; pump + shell | the boost "feels great" (keep); the shot is loud already | heavy tier: keep the numbers that read; add a 2-frame muzzle **cone** (a 0.6 m flare cone, not only a star), pellet tracers 0.022 m, wall hits: 3 dust puffs + 1 decal per pellet cluster; the pump's 0.06 m travel x1.4 (a visible rack) |
| THUNDERCLAP slug (VERBS 30) | Feel43 slug: white core 0.34 m + cyan sheath 0.9 m (0.2 s), a 2.6 m flare + 2.8 m ring, 26 sparks, 22 debris, light 5.5 / 5 m | already the heaviest read | heavy: keep; add the camera punch to 4.5 deg (from the medium spring) and a 70 ms view hold matching the sim's hitstop |
| THUNDERCLAP boost (air shot down, VERBS 29 / boost43) | no boost visual: only the speed FOV and AIR streaks | the boost has no look of its own | a ground-facing blast puff (a 1.6 m ring on the surface under the shot, dust), the player's FOV +3 deg out 80 ms, a 2.5 deg upward camera kick; the sim's boost unchanged (latency 0) |
| PULSE swing (VERBS 28) | no `fire` event, so no muzzle, no fire trauma, no recoil; per hit: KillFeel42 cyan rings 1.8-3.8 m, kick 1.6-3.4 deg, 18-32 sparks, freeze 0.16-0.24 s | the hits are **cyan**, the gauntlet is pink: the colour lies | medium-heavy: the pink aura of 1c on the swing; hit rings in the pink three tones (not cyan); kick unchanged |
| PULSE LT blast (VERBS 28) | **no dedicated visual at all** (only HUD code reacts: `UiWeapons.N27.cs`, `HudWeapons30.cs`); each body gets an ordinary gauntlet hit | "a weak gust of wind with no weight" | 1b below |
| TORCH (VERBS 33) | per 0.1 s tick: star, muzzle light, light shake 0.06, punch 0.34 deg; the Torch43 jet: 18 x 2 flare billboards, **white-hot -> gold -> orange -> deep red** (`GunFx.Torch43.cs:150-216`), a cyan NIGHT20 spray under it | a **warm orange** flame on a player weapon breaks rule 8 (red-orange = enemies / danger), plus two flames that disagree | 1f below |
| fist (VERBS 24) | wind-up 0.4 x 0.06 s, overshoot 0.04 m; hit sparks gold 0xffd27a (no family colour); freeze 0.06 or 0.1 s (which wins: not checked) | no colour of its own | light-medium: the cyan-blue aura of 1c; sparks in its rim tone; punch 1.2 deg |
| swap (VERBS 34) | old gun out 0.055 s, overlap 0.045 s, ease-out-back rise; QUICKDRAW cyan strip; no camera effect | weightless | the new gun lands with a 0.6 deg camera dip + a 0.015 m settle bounce (a spring, k by the gun's class); a faint cyan edge sweep on the slot |
| FUSION (DECIDE43 D-3, Feel43) | pink #ff7ac8 + core #ffe8f4, cyan / red-orange motes, a pink ring, the word, kick 3.2 deg + 4 roll, trauma +0.6 | GG likes it ("I like the punch effect") | kept as it is; its pink is the gauntlet family's source tone (1c) |
Per-weapon PROOF: a747 `sheet-<weapon>` contact sheets (frames -2, 0, +1, +2, +3, +4, +6, +8, +12, +18, +30 at 1080p) off and on; the
meter is the camera's pitch per frame from the sheet's log line (peak deg, the frame it settles under 10 % of the peak); the tier targets of
the ladder above; then the blind pair per weapon (3 judges: "which shot feels heavier?"; a tie keeps the new one ON: NIGHT47.md G24).

### 1b. The LT blast (G20; sim row `pulse47` + its look in `fx47`)
- **Today (B1, measured):** bodies within 1.5 m (+ radius) of the aim line out to 6 m, once each: a horizontal shove of **7 m/s for 0.2 s =
  1.40 m**, damage 64 (`Core/PulseCharge27.cs:228-246`); no lift, no launch, no burst, no sim hitstop; the player's 16 m/s self-boost peaked at
  5.9 m/s in the harness (why: not checked). The blast has **no dedicated visual** (B6: only HUD code reacts; each body gets an ordinary cyan
  gauntlet hit), and unlike the click swing it skips heavy41's x2 and MeleeKnock40 (`Combat.cs:1322-1324`). The verb: VERBS 28.
- **pulse47 (sim; numbers in `design.n47.pulse47.numbers`, a why each):** the blast **launches** every body it hits through launch36's own
  path (`Loadout.launch36.Launch`: the flight, the burst on a wall or another body, `launch36Burst`), at `launchSpeed` 18 m/s (launch36's punch
  16: the ultimate throws harder), `lift` 6 m/s (a visible arc, off the floor ~0.6 s), `flightS` 0.8 s (expected throw ~14 m on open floor, a
  wall within reach bursts it: launch36's `burstDamage` 60 / `burstRadius` 3.5 kept) -> **knockback >= 10 m on a husk, 7x today's** (the
  target in the harness: >= 2.8 m, 2x, G20's done-when); a second **ring around the player** (R9's two circles: Blood Punch's inner damage +
  outer shockwave): bodies within `ringRadius` 3.0 m in any direction get the launch at `ringDamageShare` 0.5 of the blast's damage (no more
  "only what is on the line"); `damageMult` 3.5 (gauntlet 32 x 3.5 = 112 per body; today 64); `range` 7 m and `radius` 2.2 m (today 6 / 1.5);
  a sim hitstop of `hitstopS` 0.083 s (10 steps: C-2's gauntlet band 12-20 starts higher; 10 keeps the boost's feel) through the sim's
  existing hitstop path (`SurvivalSim.Hitstop`, as heavy41); bodies that ignore knockback (bosses, bulwark, brawl, burrower, glacier) take
  the damage and a `stagger` 0.6 s instead (`Enemy.Stun`). Kept: 5 kill pips, the 15 s cooldown (CORE: a crowd-control verb needs 15 s+),
  the boost. Where: one hook in `StepPulse27` (a `blast47` params object my partial sets at StartRun; null = NIGHT46's loop bit for bit).
  Event names: `pulse47Blast {hits, launched, ring}` (the old `pulse27Blast` still fires: GG's sound keeps working).
- **The look (fx47), in order (NIGHT47.md section 3):** a 100 ms gather (inward particles to the glove, 24 pink motes from a 1.2 m shell, the
  glove's emissive 1 -> 3x) that never delays the effect (the sim fires on the press; the gather plays over the 0.083 s hitstop and the first
  frames, so the input delay is 0); a 1-2 frame white-pink core flash at the glove (#ffe8f4, 0.35 of the screen height at most, never over the
  centre box: offset toward the glove); a **shockwave ring** from 0 to the blast's 7 m in 0.2 s (an additive torus on the floor plane + a
  vertical ring along the aim, pink #ff6fb0 -> rim #d8487e) with a screen-space **distortion band** (the ring's screen projection samples the
  opaque texture with a 6-10 px radial offset: R10; Opaque Texture is on: `Assets/Settings/PC_RPAsset.asset:23` m_RequireOpaqueTexture 1); debris (12 chunks from
  the floor under the line) and dust (a ground puff along the line); flung bodies **tumble** (view: their model spins at 540 deg/s about the
  launch's cross axis while `launched`); a view hold matching the sim's 83 ms; trauma +0.5; FOV punch +2.5 deg out 60 ms, back 300 ms; embers
  0.5 s. **Two look variants** for the blind pair: A "ring" (the torus + distortion lead) and B "lance" (a pink cone of streaks along the aim
  lead, the torus small).
- **PROOF:** `n47feel blast` RED 1.40 m (off) -> GREEN >= 2.8 m (on; card target ~10 m), damage 64 -> 112, plus `fingerprint` / `--off46` /
  `--off47` EQUAL, golden 0 / 22 exact (the hook is in Core). Frames: a747 `blast` contact sheet (-2 .. +30) off / A / B; the blind pair
  (3 judges, one question: "which LT blast hits harder?").

### 1c. The punch aura (G21; fx47 player side, foes47 enemy side: 5d)
- **Today (B6):** the pink effect GG likes is the **FUSION** (a gauntlet swing or a punch landed at >= 14 m/s, a dash, or just after a wall
  jump: `Core/Fusion43.cs:1-14`): pink #ff7ac8 + a near-white core #ffe8f4 x2.4 HDR, cyan motes from the player's side, red-orange embers from
  the enemy's, a pink ring and light (`Fusion43View.cs:131-203`, `UnityTuning.Feel43.cs:11-13`). Every other gauntlet hit is **cyan**
  (KillFeel42 rings, melee sparks); the fist's hit sparks are gold 0xffd27a; no punch aura exists (`Viewmodel.Feel.cs:669-683` HandsAura is
  the style ladder, cyan).
- **After:** every punch is drawn as **three tones of one hue plus a soft halo** (a light core, a mid body, a darker rim; NIGHT47.md section
  3), on the fist's path from the wind-up to 0.15 s after the hit, and the hit adds a ring in the same three tones. Families (OKLab L / C / h
  measured tonight, R1):
  - **fist melee = cyan-blue** (VERBS 24): core #d8f6ff (0.95 / 0.03 / 218), body #46c8ff (0.79 / 0.14 / 231), rim #1e5cff (0.55 / 0.25 / 264),
    halo #8fe3ff (0.87 / 0.09 / 221). Inside the cyan family (colors.usable #19d3e6 is h 207): player-usable.
  - **gauntlet = pink** (VERBS 28, GG: "stays pink"; DECIDE43 D-3's FUSION pink #ff7ac8 is GG's liked tone): core #ffe8f4 (0.95 / 0.03 / 345),
    body #ff6fb0 (0.73 / 0.19 / 355), rim #d8487e (0.62 / 0.19 / 1), halo #ff9ccf (0.81 / 0.13 / 348). **Kept apart from RIVAL's magenta**
    #ff2bd6 (0.69 / 0.28 / 338): every gauntlet tone is h >= 344 (6+ deg warmer) and C <= 0.20 (RIVAL 0.28), written as constants
    `GauntletHueMinDeg = 344`, `GauntletChromaMax = 0.20` in `UnityTuning.F47.cs` and checked by a test.
  - **enemies = red-orange** (unparryable) / **yellow** (parryable): section 5d.
- Motion: the core is a stretched additive sphere along the fist's motion (length = speed x 0.05 s), the body and rim are two larger shells
  (1.4x / 1.8x) at falling alpha, the halo a camera-facing soft disc 2.5x; all grow over the wind-up (ease-in), peak at contact, fade in 0.15 s
  (ease-out). The hit ring: 0 -> 1.2 m in 0.12 s, three concentric bands in the three tones. A launch (launch36) leaves a 0.3 s trail in the
  rim tone along the body's flight.
- **PROOF:** a747 `punch`: fist, gauntlet swing, gauntlet blast, husk, warden: the start / hit / fade frames, off and on; a pixel meter: the
  three tones present (hue histogram of the aura's pixels inside its family band, 3 peaks).

### 1d. The THUNDERCLAP's LT zoom (G22; view row `zoom47`)
- **Today (B6):** with `n31.ltAlways` on the THUNDERCLAP zooms x1.25 (`Core/Guns/LtAlways31.cs:107-114`): base 74 deg -> **~62 deg** (0.84x),
  a **linear** 0.12 s ease (`ZoomEase33`); the pose (`ZoomPose33.Of`, `Viewmodel.View33.cs:22-37`) **keeps the hip pose** (x 0.135, y -0.147,
  z -0.36, yaw 5 deg, pitch 1 deg) and only **lowers the gun 0.05 m**; the shotgun's kit offset stays; sway x0.25, punch x0.6. So "it doesn't
  feel like a proper zoom-in, and the angle is awkward": the view narrows a little while the gun drops at its hip angle, its barrel pointing
  5 deg off the reticle line, and nothing comes to the eye.
- **After:** keep the existing zoom path (w33: world FOV only; the gun never slides to the centre). World FOV eased to **0.80x the base in
  150 ms** (ease-out cubic), back in 120 ms; the gun rises to a **shouldered pose**: from the hip pose it moves up 0.045 m and in 0.02 m, the
  barrel yawed in so its line passes through the reticle at 25 m (the pose's yaw solved from the muzzle offset: `atan(muzzleX / 25)`), **no
  roll** (roll 0, today's hip roll eased out), a 70 ms anticipation dip of 0.008 m before the rise; the reticle's spread ring tightens to the
  zoomed spread (the existing spread number: shown, not changed); the boost on LT in the air is the sim's and unchanged (VERBS 29 / 31: the
  air latency stays 0 ms: `latency34`, a view row cannot touch the sim).
- **PROOF:** a747 `zoom`: hip / mid-zoom (75 ms) / full (150 ms), off and on; meters: the camera FOV at +150 ms (want 0.80x +-0.02), the
  barrel line's screen distance from the reticle at full zoom (want < 12 px at 1080p), roll 0.

### 1e. The grapple line (G23; view row `grapple47`)
- **The bind (checked tonight in code):** the live pad layout is **v4** (`PadReader.cs:96-125`: design.pad.n19 on, design.pad.n20 on,
  design.n24.padY on, design.n25.meleeFire off) = `GamepadMath.DefaultLayoutV4` (`Core/Gamepad.cs:438`, v2 with RB / Y swapped twice): **RB =
  SPECIAL + ARCLINE (the grapple)**, Y = NEXT WEAPON (tap) / the wheel (hold), LB = DASH, R3 = PUNCH. **GG's "right bumper" is right.**
  docs/VERBS.md row 17 ("LB tap / E") and row 34 ("weapon swap: RB") are stale for the pad column (hand-off to the lead: the shared doc).
  The keyboard key: `input.bindings.grapple` = **KeyG** (`PortExport/config.json`). Every prompt uses the `{grapple}` placeholder filled with
  the live glyph or key (`Game/Input/PromptText.cs`; e.g. "PRESS {grapple} to GRAPPLE. It pulls you to the hit.", the level-4 teach lines,
  `SurvivalSim.N40.Loom.cs:123, 130`), so prompts and the bind agree by construction; the settings label is "ARCLINE"
  (`UnityTuning.Menus17.cs:284`): the copy pass renames it GRAPPLE (one name for one verb). A player's saved remap overrides all of this.
- **Today's line (B6):** one `LineRenderer`, view-aligned, 0.07 m wide, cyan #19d3e6, an additive untextured neon copy
  (`Arcline21View.cs:40-51`), a 14-segment sagging curve, a 0.17 m spinning cube hook (`Arcline21View.Rope33.cs:55-103`). **The "dotted line"
  is the pull beads:** 5 short LineRenderers 0.55 m long, 2.2 m apart, 0.154 m wide, cyan x2.4, running anchor -> camera at 34 m/s while
  pulling (`Grapple31View.cs:92-106, 182-208`, `UnityTuning.Grapple31.cs:24-28`): a dashed line over the rope. The aim diamond is already not
  drawn (`n34.nomark34`). The BEND is a grade + FOV thump (`Bend34View.cs`), slow motion x0.25.
- **After (one ribbon):** an HDR white-cyan core (#e8fdff x 3.0, bloom threshold 1.0 picks it up), width 0.018 m at the hand tapering to
  0.008 m at the hook; a soft additive glow shell 3.5x the core's width in cyan #19d3e6 x 1.2 at 0.45 alpha with a soft falloff texture
  (one generated 1 x 64 gradient, no dots); camera-facing (View alignment), one continuous strip (no dash texture), the existing curve /
  sag points kept; **the 5 pull beads go** (their information, "you are being pulled", moves into the ribbon's travelling pulse). **Fire:** the line flies out with a decaying wave (amplitude 0.12 m, 2.5 waves along the line, straight in 0.15 s);
  **pull:** a bright pulse (the core x2 over 0.6 m) runs hook -> hand every 0.25 s; **the BEND (VERBS 22):** the line brightens to x1.6 and its
  wave and pulse slow with the bend's time scale; **release:** it snaps back to the hand in 0.08 s with a last flash at the hook. **The aim
  preview:** a thin faint solid line to a valid target (0.004 m, 0.25 alpha), no dots, no target diamond (w33's "never again").
- **PROOF:** a747 `grapple`: aim / fire (+2 frames) / pull / release / BEND, off and on; meter: the line's pixel run along its length has no
  gaps (a dotted line has > 5 gaps; the ribbon 0).

### 1f. The TORCH flame (G43, TORCH part; fx47)
- **Today (B6):** all billboards: a NIGHT20 cyan spray (6 streaks a tick, `GunFx.Spray20.cs`) under the Torch43 jet (18 segments x 2 flare
  billboards, white-hot -> gold -> orange -> deep red 0xd8401c, a dark smoke tail past 62 %: `GunFx.Torch43.cs:150-216`), per tick 7 lick
  puffs, 5 embers, an orange light 4.0 x (1 +- 0.2, random) at 8 m; each tick is a `fire` (a star, a 0.34 deg punch). The flame is
  **orange**, the colour of enemies and danger, on a player weapon (rule 8; GG: "doesn't match the game's style"), with a cyan spray under
  it that disagrees, and no weight (0.34 deg ticks, no start kick, no release).
- **After (NIGHT47.md section 3, C-3):** a cone of soft flipbook flames (a generated 4 x 4 flame sheet, 16 frames at 30 fps) with a
  **white-hot core at the nozzle**, coloured by family: the player's cyan-blue plasma, **white core -> blue #46c8ff -> violet #7a3dff tips**
  (the violet tips are the overheal purple's hue band, h <= 295, never RIVAL's); heat haze at the cone's edge (the same opaque-texture
  distortion as the blast, 3 px); embers (40 / s, 0.6-1.0 s life); a flickering light on nearby surfaces (one LightPools slot, the flicker a
  slow low curve: 6 Hz +-12 %, seeded); scorch marks where the cone meets a surface (one decal per 0.2 s, max 24); a **pressure kick** on start
  (camera punch 1.2 deg, the gun back 0.03 m) and a continuous low recoil while held (0.25 deg at 10 Hz); a **lick** on release (the cone
  stretches 1.3x and fades in 0.15 s); a 0.2 s ramp-in (C-3). **Burning reads on enemies** (Doom Eternal's flame belch: R7): a burning enemy
  shows blue flame tongues on its body and its visor flickers; `EnemyViews.Burn43.cs` (B6) is the hook.
- **PROOF:** a747 `torch`: the contact sheet off / on, and a 1 s hold strip; the cone's pixel coverage of the reticle's centre box (want 0).

## 2. The HUD spec (G5 system, G16-G19; row `hud47`, sim row `overheal47`)

Baseline B4 (code inventory, file:line; the canvas is ScaleWithScreenSize 1920 x 1080 match height: `Ui.cs:204-211`, so **1 fontSize unit =
1.000 px at 1080p, 0.667 px at 720p**; TMP fontSize is the em, the cap height per font not measured). Every hud33 label draws a hard shadow copy
2 px offset, #0A0E14 at 80 %, **no TMP outline** (`UiStyle33.cs:161-178`). HUD SCALE in OPTIONS scales only the hidden old layers
(`UiHudMotion.cs:212-213`): **it does nothing to today's HUD**; UI SCALE is pinned at 100 % (`UiSettings.cs:712-728`); no text-size option.

### 2a. Text sizes and the text system (G5's system; XAG 101 = R6)
| element | today (1080p / 720p px) | after (1080p) | why |
|---|---|---|---|
| fight / boss lines (BossLine40 via the teach card line 1, Message slot banners) | teach line 1: 40 shrinking to 28 / 26.7-18.7; Message slot 24 / 16 | **44** (never shrinks below 36; the line is cut to <= 6 words instead) | NIGHT47.md section 3: boss and fight lines >= 36 px at 1080p; 36 at 720p is 24 px, above XAG console 26 only at 1080p, so the floor is 36 |
| teach cards (line 1 / line 2) | 40 -> 28 / 24 -> 18 | **40 / 30** (floors 34 / 30) | teach >= 30 px |
| the enemy counter number | 36 | **48** (+ the word "LEFT" at **30**, not 16) | counter >= 32 px |
| Message slot (ROOM banners, DOOR OPEN, countdown) | 24 | **36** | a banner is a fight line |
| boss bar name / hint / LEG-OPEN-STAGGER | 32 / 18 / 16 | **36 / 26 / 26** | XAG console 26 |
| weapon name / LT label / award feed / rank name / run lines / round line | 24 / 16 / 16 / 18 / 16 / 20 | **28 / 22 / 22 / 22 / 22 / 26** | nothing the player reads in a fight under 22 (XAG PC 18, console 26; pad-first: 22 is the floor for corner labels, 26 for anything read mid-fight) |
| health number | 56 | 56 (kept; the bar carries the state) | already large |
- **Outline:** every hud47 label gets a TMP outline 0.22 (about 2-3 px at these sizes) in #0A0E14 plus the existing soft shadow; no filled
  boxes (w33). The teach card's INK plate at 0.82 alpha stays (it is a card, not a HUD box) but its text gains the outline too.
- **Timing (the text system, every fight / boss line):** one message at a time (a queue, newest-wins only for the same source), held
  **2.0 s + 0.3 s per word**, a 0.12 s scale-in from 1.08 and a 0.25 s fade-out; upper third (y = -170 from the top, below the boss bar), never
  in the reticle's centre box (10 % of the screen); <= 6 words per fight line (W writes the WICK's words to this; F enforces the cap with a
  log line naming any longer line).
- **TEXT SIZE in OPTIONS** (XAG: scaling to 200 %): 100 / 125 / 150 %, a multiplier on every hud47 size, saved under a new prefs key in
  `UnityTuning.F47.cs` (the HUD SCALE row is relabelled nothing; it is left as it is: nothing lost).
- Fonts unchanged (the theme's: Archivo ExtraCondensed Black Italic numbers / Archivo ExtraBold labels; CLAUDE.md rule 8).

### 2b. One continuous health bar (G16)
- **Today:** 10 blade cells 15 x 24 px, pitch 22, 12 deg lean, bone #E6ECEF, red-orange at <= 25 % with a 1.2 Hz breath, a red-orange
  after-image (0.15 s wait, then drains), a cyan heal glow, 3 notches (`UiHud33.Hud43.cs:87-270`, `UnityTuning.Ui43.cs:35-50`). DESIGN-RULES
  U-6 says "segmented": **GG reverses it tonight** (his words: "a single continuous bar instead of a series of tiny segments"); U-6 gets a
  "changed by hud47" line.
- **After:** one continuous fill 260 x 18 px (the 213 px frame widened 22 %), the same 12 deg lean and bracket (the HUD's language kept), no
  cells, no notches (the 25 / 50 / 75 % notches become 2 px tick marks on the frame only, so a player can still read quarters: nothing lost).
  **Colour by health share, interpolated in OKLab (R1)** through six stops (hex, OKLab L / C / h measured tonight with Ottosson's matrices):
  100 % #35e05a (0.80 / 0.22 / 146), 80 % #86ec5c (0.85 / 0.20 / 138), 60 % #c4f03c (0.89 / 0.20 / 123), 40 % #ffd83a (0.89 / 0.17 / 94),
  20 % #ff8a2a (0.75 / 0.17 / 54), 0 % #ff3326 (0.65 / 0.24 / 29). A soft glow in the bar's own colour (an additive copy 1.6x the height at
  0.35 alpha). The ghost trail: on damage, a lighter segment (the bar's colour lerped 50 % to white) holds 0.4 s then drains at 60 % of max
  per second. **Low state told by more than hue** (red-green colour blindness): below 25 % the bar and the number pulse at 1.2 Hz (today's
  breath kept) and the number brightens to x1.4; below 10 % the frame (not the screen) flashes on each heartbeat.
- The health number stays (56 px) and is the one hero number (U-3).

### 2c. Overheal blue to purple (G17; sim row `overheal47`, DECIDE47 D-4)
- **Today:** no overheal: Heal and Regen cap at `max - hard` (`Core/Health.cs:62, 90, 101`); the HUD clamps the fill to 0..1
  (`UiHud33.Hud43.cs:187`) and the number to max. `health.max` = 100 (`PortExport/config.json:5342`).
- **The sim row (overheal47; numbers in `design.n47.overheal47.numbers` with a why each):** `cap` 150 (overheal up to +50: one more husk
  hit-and-a-half of room, not a second health bar), `fromKill` 10 (a close kill's heal36 spill that lands while hp is at max goes into the
  overheal instead of being lost: heal36's own radius 4 m and amount 10 kept, so the source is a verb the player already has, VERBS 42);
  health pickups are unchanged (NIGHT29: a pickup is refused at full health and stays for later, `Core/Campaign24.cs:386`), `holdSeconds` 2.0 then `decayPerSecond` 2.0 (overheal
  drains to 100, never below: decay, not regen, the w19 rule holds), damage takes overheal first. Where: a field `over` in my partial
  `SurvivalSim.N47.Feel.cs`; the one shared hook is a **one-line call in `Health.Damage`** (absorb from an `overAbsorb` delegate, null with the
  row off: NIGHT46 bit for bit). The heal36 spill emits nothing at full hp (`Core/Heal36.cs:48-49`: `got <= 0` returns before the event), so
  my partial listens to `kill` itself, applies heal36's own test (the kill within `design.n36.heal36.radius` of the eye) and puts the part of
  `amount` that did not fit under 100 into the overheal (no edit in Heal36.cs; the handler's order against Heal36's: read in round 1). Event names: `overheal47Gain {amount, over}`, `overheal47Spent {amount, over}`.
- **The look:** a second layer over the full bar, from 100 to the cap: #3fa0ff (OKLab h 251) at +0 -> #4a7dff (h 265) at +25 -> #7a3dff
  (h 289) at the cap, interpolated in OKLab. **The purple stays out of RIVAL's magenta:** RIVAL #ff2bd6 is OKLab h 338, C 0.28; the cap purple
  is h 289 (49 deg away), and the overheal hue is clamped to h <= 295 in code (written as two constants in `UnityTuning.F47.cs`:
  `OverhealHueMaxDeg = 295`, `RivalHueDeg = 338`). (The boundary's pale violet-white #c9bcff is h 293 but C 0.09 / L 0.83: told apart by
  saturation and value.) The number shows "130" in the overheal colour.
- **PROOF:** harness `n47feel overheal` (a full-hp player, scripted close kills, then a 6 s idle and a hit): RED off = hp never > 100;
  GREEN on = 100 -> 110 -> 120 ... 150 cap, holds 2 s, drains 2 / s to 100, a 20-damage hit at 130 leaves 110 (overheal first). Frames
  (a647): 110 / 130 / 150.

### 2d. The enemy counter (G18)
- **Today:** a red-orange 4 x 36 blade, the count at 36 px, "LEFT" at 16 px muted, **one 5 x 9 px pip per enemy (max 12)** under it (the
  "tiny bars" GG means), a 1.25x punch + white flash on a kill (`UiHud33.Counter43.cs:43-151`, `UnityTuning.Ui43.cs:67-83`). Survival's round
  line sits at the same anchor (72, -54) (overlap not checked).
- **After:** "6 LEFT" in one line: the number 48 px, the word 30 px, top-left inside the safe area (x 72, y -54), the outline of 2a; **the pips
  go**; one thin line 220 x 3 px under it that drains per kill (eased 0.2 s), the 1.25x punch + 0.05 s white flash kept; at 1 the line reads
  **"LAST ONE"** in red-orange and pulses at 1 Hz; at 0 **"CLEAR"** in cyan, then fades after 1.5 s. Frames (a647): 12 / 3 / 1 / 0.

### 2e. The reticle alone (G19): where the reticle bar's information goes
The arc under the reticle (hud38 -> arc40 -> meter42, all ON: a 104 deg arc, radius 60, 10 px: `UnityTuning.Hud40.cs:13-19`) shows, per B4
(`UiHud38.cs:248-284`, `UiHud38.Meter42.cs`, `UiHud38.Arc40.cs`), and each item's new home (PRINCIPLES 1: nothing lost):
| the arc showed | sim field | moves to (hud47 + fx47) |
|---|---|---|
| KESTREL / THUNDERCLAP charge hold (fill + white tick) | `lo.Charge35Level` / `ChargeShots27.Level` | **the gun:** a glow ramp on the viewmodel's barrel shroud (0 -> 1 emissive cyan along the barrel, the tip leading); **the panel:** the held slot's charge bar (the rail's hand bar fills: already exists with charge35) |
| FULL (cyan flash, 3 Hz breath) | level >= 1 | the gun's whole emissive flashes white 0.1 s then breathes cyan at 3 Hz; the LT label reads "CHARGED" (exists) |
| charge-shot cooldown refill + cyan ping | `cs.cooldown[gi]` | the slot's cooldown line (exists, `UiRail34`) + the barrel glow dark until ready, then one cyan ping along the barrel |
| THUNDERCLAP pump (>= 0.6 s) | `Hud30Reads.CooldownSteps` | the viewmodel's pump animation (exists: `Viewmodel.Pump40.cs`) + the slot line |
| PULSE kill pips 0-5 | `pulse27.pips` | **the glove:** five pip lights on the gauntlet's knuckles (dark -> pink as they fill); **the panel:** five 6 px pips beside the PULSE slot |
| BLAST ready (5 / 5, cooldown done) | `pulse27.Ready` | the glove's knuckles all pink + a slow pink breath of the whole glove; the LT label "BLAST READY" |
| BLAST cooldown (15 s) | `pulse27.cooldownSteps` | a pink line on the PULSE slot draining; the knuckle pips dim until it ends |
| PULSE swing cooldown | gun cooldown | the slot's cooldown line (exists) |
| STITCHER heat / overheat | `lo.Heat`, `lo.Overheated` | the nailgun's barrel glow (cyan -> red-orange as heat rises; flashing while locked) + the slot line |
| any other cooldown >= 0.6 s | gun cooldown | the slot line (exists) |
| the reticle tick spin | cooldowns | kept (the reticle itself is not the bar) |
| grapple charges + recharge (y = -76 row) | `grappleCharges32` | kept as a small row **beside the weapon panel** (two cyan hook pips), off the reticle |
| `hud38Ready` event | | kept (same name) |
- Under hud47 the arc and its under-stroke are not drawn; the dot's two small charge arcs (hud33, radius 9) are also the "charged attack
  indicator" GG names, so they go too (their information is the barrel glow). The reticle (dot + ticks) stays alone.
- **PROOF (G19):** frames (a647) of the reticle alone (row on) vs the arc (row off), and frames of the same information on the gun and the
  panel for the KESTREL charge, the THUNDERCLAP charge, the PULSE pips and the BLAST cooldown; a pixel meter: the count of non-reticle HUD
  pixels inside a 160 px box round the screen centre (RED > 0, GREEN 0 except the reticle's own).

## 3. The copy inventory (G15; view row `copy47`; the lead assembles docs/COPY47.md from this section)

**The rules (NIGHT47.md section 3; XAG 101 = R6):** second person, verb first, plain words, <= 10 words a line, the button glyph shown
(pad first: the `{action}` placeholders already fill in the live glyph or key: `Game/Input/PromptText.cs`), a reason when it helps, no
internal names, no jargon, and read aloud it sounds like a person talking. Sentence case for lines (XAG: lines of text in sentence case; a
one or two word label stays in caps); weapon and boss NAMES stay in caps (they are names). How it ships: `Copy47.Get(key, old)` at each
string site returns the new line with `copy47` on and the old one off (no string is deleted: off = NIGHT46's text exactly); config-held
lines get a `text47` sibling read only with the row on (the old `text` untouched).

### 3a. The two GG named (two variants each; the lead picks)
| where | before (today, verbatim) | after A | after B |
|---|---|---|---|
| **the gauntlet pickup** (`design.n35.teach35.lines[14]`, id `w.gauntlet`; the card's head is the weapon name) | `PRESS {fire} to PUNCH. {altFire} to PARRY.` (wrong as well as stiff: with ltAlways, LT is the BLAST once five kills charge it and the parry otherwise: VERBS 28 / 40) | head **THE PULSE GAUNTLET** / rule **Punch with {fire}. {altFire} parries, and after five kills it blasts.** | head **THE GAUNTLET IS YOURS** / rule **Hit hard with {fire}. Five kills arm the {altFire} blast.** |
| **the game start** (level 1's first room 1.1 INTAKE, `campaign.rooms[0].lessons[0].prompt` in n46-foundry; also `design.n42.teach42.skip[0]`) | `MOVE. LOOK. {jump} JUMP.` | head **GET MOVING** / rule **Move and look around. {jump} to jump.** | head **THE FOUNDRY** / rule **Stay fast. {jump} to jump, again in the air.** |

### 3b. Every other player-facing string (before -> after)
**Teach cards (`design.n35.teach35.lines`, read from config tonight; live with teach35 / teach43's card):**
| id (room) | before | after |
|---|---|---|
| l1.dash (f3) | PRESS {dash} to DASH. Works in the air. | {dash} to dash. It works in the air too. |
| l1.slide (f7) | HOLD {slide} to SLIDE. You keep your speed. | Hold {slide} to slide. You keep all your speed. |
| l1.fire (f8) | PRESS {fire} to FIRE the KESTREL. | Shoot the KESTREL with {fire}. |
| l1.charge (f8) | HOLD {fire} to CHARGE. Release to fire. (off: The shot pierces.) | Hold {fire} to charge, let go to fire. It pierces. |
| l1.zoom (f9) | HOLD {altFire} to ZOOM. Aim on a wall slows time. | Hold {altFire} to zoom. On a wall, time slows. |
| l2.wallride (i1a) | HOLD {jump} on a WALL to WALL RIDE. Let go to leap. | Hold {jump} on a wall to ride it. Let go to leap. |
| l2.walljump (i2) | PRESS {jump} on a WALL to WALL JUMP. Chain them. | {jump} off a wall to wall jump. Chain them up. |
| l3.charge (k1) | HOLD {fire} to CHARGE a SLUG. Release to fire. (off: It cracks armour.) | Hold {fire} for a slug. It cracks armour. |
| l4.grapple (s2) | PRESS {grapple} to GRAPPLE. It pulls you to the hit. (off: TAP / HOLD to SWING.) | {grapple} to grapple. It pulls you where you aim. |
| l4.bend (s3) | LATCH with {grapple} to BEND time. SWAP guns in it. | Hook an enemy with {grapple}: time slows. Swap guns then. |
| l5.jumpdash (v1) | PRESS {dash} then {jump} to JUMP DASH. | {dash}, then {jump}: a jump dash. Go far. |
| l5.whip (v2) | AIM {grapple} at an enemy to WHIP. Then PUNCH. | {grapple} an enemy to yank it close. Then punch it. |
| w.shotgun | THE THUNDERCLAP. PRESS {fire} to BLAST. | THE THUNDERCLAP. {fire} to fire. Shoot down to fly. |
| w.nailgun | THE STITCHER. HOLD {fire} to STREAM nails. | THE STITCHER. Hold {fire} to pour nails. |
| w.gauntlet | (3a) | (3a) |
| w.torch | THE TORCH. HOLD {fire} to BURN. | THE TORCH. Hold {fire} to burn. Burning foes lose armour. (only if the burn rule of 1f lands; else "Hold {fire} to burn them.") |

**Level 1 and 2 room lessons (`campaign.rooms[].lessons[].prompt`, n46-foundry / n46-iceline, read tonight):**
| room | before | after |
|---|---|---|
| 1.1 INTAKE (slide) | HOLD {slide}: SLIDE UNDER | Hold {slide} to slide under. |
| 1.3 RIDE / 1.10 PIERCE | HOLD {jump} ON A WALL: RIDE | Hold {jump} on a wall to ride it. |
| 1.4 PARRY | PUNCH {punch} THE YELLOW GLOB BACK | Yellow means punch it. {punch} sends it back. |
| 1.7 THE GANTRY | HOLD {jump} ON THE WALL / {jump} WALL TO WALL. {dash} ACROSS | Hold {jump} on the wall. / Jump wall to wall, {dash} across. |
| 1.9 THE FLUE | LOOK UP. {jump} WALL TO WALL | Look up. {jump} from wall to wall. |
| 1.10 PIERCE | HOLD {altFire}: SLOW. {fire}: PIERCE | Hold {altFire} to slow time. {fire} pierces. |
| 2.1 TANK ALONE | {fire}: ONE SWING FELLS A TANK | One gauntlet swing drops a tank. {fire}. |
| 2.2 THE SHIELD | {fire} THE SHIELD, OR FLANK IT | Punch the shield with {fire}, or get behind it. |
| 2.3 THE SWARM | FIVE KILLS: {altFire} BLASTS THE PACK | Five kills charge it. {altFire} blasts the pack. |
| 2.4 THE REACH | {fire} AS THE YELLOW ORB ARRIVES | Punch the yellow orb back with {fire}. |
| boss line (brawl40) `design.n40.brawl40.line.entry` / `n41.entrance41.lines.brawl40` | GET IN CLOSE: {fire} THE GAUNTLET HITS FULL | Get in close. The gauntlet hits it full. |
| `design.n30.span.loom.lineStart` | HANG ON A LEG. NAIL THE JOINTS. | Grab a leg. Nail its joints. |
| first fifteen (`design.adds.firstFifteen.moments`) | {punch} PARRY GLOBS / {interact} MYSTERY BOX / HOLD THE RING / PICK A MOD / TAKE ONE / {interact} CALL EXFIL | Punch the globs back with {punch}. / {interact} to open the mystery box. / Stay inside the ring. / Pick a mod. / Take one. / {interact} to call the exfil. |
| boss intro name `design.n25.bosses.final.introName` | ELDER KILN WYRM | kept (a name) |

**What the full inventory found (a read-only sweep of code + both configs + the n46 level files, file:line / JSON path each; ~250
strings; the survival-only and menu strings are listed there and get the same rules in round 2, campaign-first):**
- The gauntlet is **earned silently** at level 2 (`design.n31.unlocks`): no pickup prop, no banner, no "picked up" line; teach35's
  `w.gauntlet` card is the only announcement (shown once per profile). So G15's "gauntlet pickup text" = that card (3a), and copy47 adds the
  missing beat: a pickup banner in the message slot the moment it is earned ("THE PULSE GAUNTLET" + the 3a rule).
- **The first words of the game** are level 1 room 1.1's lesson "MOVE. LOOK. {jump} JUMP.", which the teach43 card splits into the head
  "MOVE." and the sub "LOOK. SPACE JUMP." (teach42, which would skip it, is off): 3a rewrites it. The title's tagline "A MOVEMENT SHOOTER AT
  DUSK" (`PortExport/config.json:6549`) stays.
- **7 of the 16 teach35 lines never fire** with the n46 levels: their fallback room ids (f7, i1a, i2, k1, s2, s3) do not exist; only
  n46-foundry has `campaign.teach35` rects (l1.slide, l2.wallride, l2.walljump, l3.charge, l4.grapple, l4.bend are dead; PRINCIPLES 2 says each
  verb is taught where it is first needed). Hand-off to the lead / B / L: their n47 levels should carry `campaign.teach35` rects for these ids
  (a level file is its builder's); the rewritten words above are ready for them.
- **Contradictions to fix in the words:** THE KILN WYRM's entrance line says "SHOOT ITS HEAD" (`design.n41.entrance41.lines` wyrm) while its
  punch line and bar hint say "PUNCH" the head; the level-2 boss room is named "THE BRAWL" but the boss is THE ICEBREAKER; one lesson says
  "GLOVE", everything else "GAUNTLET"; a limit room shows "ROOM 2 · PISTOL ONLY" (the room index) while the room is named "4.2"; the settings
  call the grapple "ARCLINE"; "RMB" is shown on keyboard (`UnityTuning.Hud33.cs:54`); four hints are hard-coded Xbox + keyboard
  (`UiLevelSelect26.cs:171`, `UiShop33.cs:151`, `Code22Field.cs:55`).
- **Room titles** carry internal numbers ("1.1 INTAKE", "3.2b THE STACK", and the results screen's "1  1.1 INTAKE"): with copy47 the title
  banner shows the name only ("INTAKE"); the number stays on the results list as "ROOM 1".

**Boss fight lines (live; B / L / W may change their bosses' attacks tonight: each line below is rewritten for today's fight and re-checked
against their cards at the pick; THE WICK's words are W's, listed so the system has them):**
| boss / key | before | after (<= 6 words a fight line) |
|---|---|---|
| WICK entrance41.wick | RIDE A WALL, HOLD {fire}: SHOOT THROUGH A CRACK | (W's) |
| WICK wick43.hints (reveal / legChange / stall / window / crack / locked / ember / cap / idle) | HOLD {jump}: RIDE. LET GO: LEAP UP · LEG CLEARED: A PLATE CRACKS · STUCK: {dash} INTO IT, {punch} · STAGGERED: EVERY HIT COUNTS · RIDE, HOLD {fire}: SHOOT THE CRACK · LOCKED: CLIMB TO THE NEXT LEDGE · {punch} THE EMBER: CRACK A PLATE · DODGE THE DIVE, {punch} THE STALL · CRACK A PLATE: RETURN AN EMBER | (W's) |
| ICEBREAKER brawl40.line.entry (+ entrance41) | GET IN CLOSE: {fire} THE GAUNTLET HITS FULL | Get close. The gauntlet hits hard. |
| ICEBREAKER line.far | TOO FAR: SHOTS BARELY SCRATCH IT | Too far. Your shots barely scratch. |
| ICEBREAKER line.stuck (+ the bar's "FISTS STUCK: HIT IT NOW") | ITS FIST IS STUCK: HIT IT NOW | Its fist is stuck. Hit it! (one line, both sites) |
| ICEBREAKER line.block | YELLOW BLOCK: {punch} PUNCH IT BACK | Yellow block. {punch} it back. |
| ICEBREAKER bar hints | {punch} THE BLOCK BACK · {dash} ASIDE, THEN HIT IT · {jump} OVER THE SWEEP · {dash} OUT OF THE CIRCLE · PARRY THE BLOCK. PUNISH STUCK FISTS | {punch} the block back. · {dash} aside, then hit it. · {jump} over the sweep. · {dash} out of the circle. · Parry blocks. Punish stuck fists. |
| KILN WYRM entrance41.wyrm | SHOOT ITS HEAD. THE OPEN CORE HITS x3 | Punch its head. Then shoot the core. (agrees with its punch line) |
| KILN WYRM n30.wyrm30.linePunch | PUNCH THE GLOWING HEAD. | Punch the glowing head. |
| KILN WYRM bar hints | STAGGERED: HOLD {fire}, SLUG IT · ITS CORE IS OPEN: SHOOT IT · {punch} ITS HEAD AS IT COMES · PUNCH THE HEAD TO STAGGER IT | Stunned! Hold {fire} for a slug. · Core open. Shoot it! · {punch} its head as it rises. · Punch the head to stun it. |
| LOOM loom40 entry (+ entrance41, + the o2 lesson: the same line three times) | HOOK A LIT KNEE {grapple}. THEN {grapple} PULL AWAY. | {grapple} a lit knee, then pull away. (shown once, not three times) |
| LOOM hooked | HOOKED. NOW {grapple} PULL AWAY TO A THREAD. | Hooked. Pull away to tear it. |
| LOOM open | OPEN: SHOOT THE CORE. EVERY GUN COUNTS. | It's open. Shoot the core! |
| CROWN crown40 entry (+ entryPad, entrance41) | FLY THROUGH THE RING: {dash} DASH + {jump} JUMP | {dash} + {jump} through the ring. |
| CROWN ringPrompt (+ the v10 lesson) | {dash} + {jump}: THROUGH THE RING | (as above: one line) |
| all bosses: bosses.window | STAGGERED: EVERY HIT COUNTS | Stunned! Every hit counts. |
| bossbar43 labels | LEG 1 / 3 · LOCKED · OPEN {s} · STAGGER | LEG 1 OF 3 · SHIELDED · OPEN {s} · STUNNED (labels: caps kept) |

**Banners, HUD labels, death, results (campaign; live):**
| where | before | after |
|---|---|---|
| room title on entry (`data.zones[].name`) | 1.1 INTAKE (etc.) | INTAKE (the name only) |
| `UnityTuning.Hud33.cs:105` | DOOR OPEN | DOOR OPEN (kept: two words, clear) |
| limit banner (`UiCampaignCue24.cs:70`) | ROOM {n} · PISTOL ONLY (MELEE ONLY / NO DASH / NO SLIDE / NO JUMP) | KESTREL ONLY in here. (Fists only in here. / No dashing in here. / No sliding in here. / No jumping in here.) |
| `UnityTuning.Hud33.cs:104` | EXPOSED | Standing still! Move. |
| `UnityTuning.Spawns33.cs:36` | MOVE | Keep moving! |
| LT label (`LtAlways31.cs:122-127`, `UnityTuning.Hud33.cs:54`) | RMB ZOOM / RMB BLAST / RMB PARRY / RMB COOLING (pad LT) | {altFire} ZOOM / {altFire} BLAST / {altFire} PARRY / BLAST RECHARGING (the live glyph or key, never "RMB") |
| counter (`UnityTuning.Ui43.cs:82-83`) | {n} LEFT · CLEAR | {n} LEFT · LAST ONE · CLEAR (2d) |
| death (`UiDownTell20.cs:62`, `ui.recap.names`) | KILLED BY + HUSK / BOSS / SOMETHING / an internal id uppercased | Killed by a husk. / Killed by THE ICEBREAKER (the boss's own name, not "BOSS"); no internal ids |
| `UiMenus.cs:844` pause line in campaign | ROUND {r} · {pts} · {m}:{ss} | {LEVEL NAME} · {m}:{ss} (no "ROUND 1" in campaign) |
| results (`UiCampaignResults25.cs:321-331`) | 1  1.1 INTAKE | ROOM 1  INTAKE |
| level select hint (`UiLevelSelect26.cs:171`) | ENTER / A  START      ESC / B  BACK | {confirm} START   {back} BACK (glyph-aware) |
| shop hint (`UiShop33.cs:151`) | A  BUY / EQUIP      B  TO CONTINUE      MOVE  D-PAD | {confirm} buy or equip · {back} continue (glyph-aware) |
| settings (`UnityTuning.Menus17.cs:284`) | ARCLINE | GRAPPLE |
| lesson n46-iceline g7 | {quickSwap}: KESTREL FAR, GLOVE CLOSE | {quickSwap}: KESTREL far, GAUNTLET close. |
| kiln43 chain (`design.n43.kiln43`, 7 lines: STEP 1 x3, STEP 2, STEP 3, THE BOOST..., THE FLUE...) | STEP 1 · PRESS {jump} TO JUMP / STEP 1 · IN THE AIR, LOOK STRAIGHT DOWN / STEP 1 · PRESS {fire}: THE BLAST LIFTS YOU / STEP 2 · {jump} AND {fire} TOGETHER: HIGHER / STEP 3 · AT THE TOP, PRESS {fire} AGAIN / THE BOOST: JUMP, LOOK DOWN, FIRE, FIRE AGAIN / THE FLUE · {fire}, {jump} AT THE TOP, {fire} AGAIN | {jump} to jump. / In the air, look straight down. / {fire}: the blast lifts you. / {jump} and {fire} together go higher. / At the top, {fire} again. / Jump, look down, fire, fire again. / Same again: {fire}, {jump}, {fire}. |
| kiln43 ash / ash2 | IT HUNTS THE FLOOR: JUMP, BOOST, OR A PLATE / HIT IT AS IT SURFACES: IT REELS | Something hunts the floor. Get up high. / Hit it as it surfaces. |
| kiln43 slug | HOLD {fire} ON THE GROUND: CHARGE A SLUG | On the ground, hold {fire} for a slug. |
| n46-kiln lessons | THREE STEPS, EACH ONE HIGHER / UP ALL THREE, THEN DROP DOWN / JUMP. SHOOT DOWN. JUMP. SHOOT DOWN. / JUMP IN. SHOOT {fire} DOWN. FLY. / CLIMB. / CLIMB IT ON THE RUN. | Three steps, each one higher. / Up all three, then drop. / Jump, shoot down, again. / Jump in, {fire} down, fly. / Climb. / Climb it at a run. |
| n46-span lessons | {grapple} PULL TO THE RING. / LOOK UP. {grapple} PULL. / KILLS GIVE PULLS. / TWO CHARGES. TWO PULLS. / {grapple} AT AN ENEMY: WHIP. / {grapple} PULL. RIDE. PULL. / {grapple} HOOK KNEE. {grapple} PULL AWAY. | {grapple} to pull to the ring. / Look up. {grapple} to pull. / Kills give you more pulls. / Two charges: two pulls. / {grapple} an enemy to yank it. / Pull, ride, pull. / (the LOOM line above, once) |
| n46-spire lessons | THE TORCH: HOLD {fire} CLOSE / {dash} DASH, THEN {jump}: FLY / {dash} + {jump}: THROUGH THE RING | (the TORCH card in 3b covers it: one line, not two) / {dash}, then {jump}: fly. / (the CROWN ring line) |
| iceline g5 / g6 / g8 | SAVE {altFire} FOR THE SECOND PACK / KESTREL BACK: {fire} THE FAR ONES · {dash} INTO IT. {fire} THE GAUNTLET. / {dash} ASIDE: IT HITS THE WALL · {dash} ASIDE, THEN {fire} ITS BACK | Save {altFire} for the second pack. / KESTREL back: shoot the far ones. · {dash} in and hit it with the gauntlet. / {dash} aside and it hits the wall. · {dash} aside, then hit its back. |
- **The rule check (PROOF for G15):** a test in `N47FeelTests` runs every `text47` / `Copy47` line through the rules (<= 10 words, <= 6 for a
  fight line, a glyph placeholder where an input is named, none of the jargon list: RMB, EXFIL, BEND without a teach, ARCLINE, GLOB, LATCH,
  internal ids), and the a647 `copy` reel shows each live one in its trigger.

## 4. Files, shared hooks, events, hand-offs, build order

- **F's own new files:** `Game/SurvivalSim.N47.Feel.cs` (N47FStartRun / N47FStep / N47FCampaign: pulse47's params, overheal47's pool),
  `Game/UnityTuning.F47.cs` (every render number above, one comment each), `Game/Fx47View.cs` (+ `.Blast`, `.Aura`, `.Torch`, `.Sheet`
  partials), `Game/Grapple47View.cs`, `Game/Zoom47.cs`, `Game/UiHud47.cs` (+ `.Health`, `.Counter`, `.Text` partials), `Game/Copy47.cs` (the
  string table, `copy47` on), `Game/Foes47View.cs` (+ `.Shield`, `.Visor`, `.Small`), `Game/Lava47.cs`, `Game/FeelReel.N47.Hud.cs` /
  `.Fx.cs` / `.Look.cs`, `Tools/SimHarness/Harness.N47.Feel.cs` (`n47feel blast | overheal`), `Tools/night/n47-f-rows.cjs` (my numbers only),
  `Assets/Tests/EditMode/N47FeelTests.cs` (the hue rules, OKLab round trip, the copy rule check, the blast and overheal numbers read).
- **One-line hooks in shared / older files (named for the pick):** `Core/PulseCharge27.cs` StepPulse27 (pulse47's `blast47` branch;
  null = today's loop), `Core/Health.cs` Damage (overheal absorb delegate; null = today's), `Game/Doors38View.cs` (door47: done tonight,
  2ed201bd), `Game/LightKit29.cs` + `Game/Look43View.cs` (lava47 skips their lava slot / roaming light), `Game/UiHud38.cs` (hud47 skips the
  arc), `Game/UiHud33.Hud43.cs` + `UiHud33.Counter43.cs` (hud47 hides the cells / pips; the new layers draw in UiHud47), `Game/Viewmodel.View33.cs`
  ZoomPose33 (zoom47's pose), `Game/Grapple31View.cs` (grapple47 hides the beads) + `Arcline21View.cs` (the ribbon material), `Game/GunFx.Torch43.cs`
  (fx47 swaps the palette and adds the layers), `Game/EnemyViews.N20.cs` (the warden's shield under foes47), `Game/Mesh43.cs` (the Visor47
  builder: new function, nothing changed), `Game/EnemyViews.cs` swarmer build (foes47 branch), `Game/UiTeach43.cs` / BossLine40 (the text
  system's sizes and timing under hud47; W's words go through it), string sites (copy47 reads `Copy47.Get(key, old)`: the old string is the
  default, so off = today's text).
- **Events (new names, for docs/AUDIO-HOOKS.md):** `pulse47Blast {hits, launched, ring}`, `overheal47Gain {amount, over}`, `overheal47Spent
  {amount, over}`, `hud47LastOne`, `hud47Clear`. View-only counters (no event): `Doors38View.StaleVeilFrames`.
- **Hand-offs:** (1) to the lead: docs/VERBS.md rows 17 / 34 / 35's pad column is stale (live v4: RB = grapple + special, Y = next weapon /
  wheel, LB = dash, R3 = punch; keyboard grapple G); DESIGN-RULES U-6 ("segmented health bar") is reversed by GG tonight: a "changed by
  hud47" line. (2) to W: THE WICK's fight lines go through the hud47 text system: <= 6 words, one at a time, 2.0 s + 0.3 s a word, 44 px;
  the "PUNCH THE CORE" line is a fight line. (3) to B / L: enemy punch auras (5d) are mine; their bosses' own visors and looks are theirs;
  the boss flame (L's CROWN) should use the red-orange family of 1f's structure (white core -> orange -> deep red), the TORCH the cyan-blue
  one, so the two flames never share a palette.
- **Order (each a commit):** R1: hud47 (bar, overheal look, counter, text sizes, the arc gone + the gun / panel reads), overheal47 (sim +
  harness), pulse47 (sim + harness), fx47 blast look, copy47 table; R2: grapple47, zoom47, fx47 per-weapon + aura, the TORCH; R3: foes47
  (shield, small robots, enemy auras), visor47, lava47; R4: second attempts on any SHORT + blind pairs. door47's first attempt is already
  committed (round 0).

## 5. Foe cards (G12 shield, G13 visors, G14 small robots, G21 enemy side; rows `visor47`, `foes47`)

All of it is view only (the hit shapes stay the sim's: DESIGN-RULES E-2 / E-5; the NIGHT43 pill language, DECIDE43 D-4: a dark pill body x0.35,
the class read in the top third from the visor + one back part, an amber emissive visor 0xFF9A3C that ignores scene light, red-orange only for
danger, yellow only where parryable; RocketBrush via R7: triangles danger, squares heavy, panels technology, one shape logic per faction).
**One faction rule for tonight: "pill + visor band + one triangle"**: every enemy is a pill, every visor is the same angular band family
(scaled per class), every enemy carries exactly one triangular danger part (fins, a ram, a spike). Baseline B5 (code inventory, file:line):

### 5a. The WARDEN's shield (G12)
- **Today (B5):** a free renderer parented to the root (`EnemyViews.N20.cs:129`), 1.9 x 1.955 x 0.12 m at (0, 1.196, 0.85) (`:133`, `:168`): it
  floats 0.13 m in front of the body, held by nothing; 1.9 m wide against a 1.5 m drawn body; the helmet + visor (2.07-2.62 m) stick out above
  it; the arm (0.35 x 1.3 x 0.35 at (0.75, 1.38, 0.2)) turns about its own centre, not a shoulder; **shield down** goes to (0.7, 0.805, 0.3)
  tilted -51.6 deg and **cuts through the body and the arm** (`:168-171`). Up / down snaps in one frame. Facing reads the sim yaw each frame
  (no lag: the lag GG may see is the snap). The sim (`Core/Warden.cs:117-133`) blocks a +-60 deg arc from the front (punch-open cone +-72.5),
  hits land on the capsule r 0.6; the drawn shield covers only +-48 deg.
- **After (foes47):** the shield is held: parented to a forearm pivot at the shoulder (0.62, 1.55, 0.05), the forearm runs forward to a grip
  at (0.35, 1.25, 0.62); the shield's centre at (0, 1.30, 0.70) **centred on the chest** (GG: "positioned awkwardly"), 1.45 m wide x 1.75 m
  tall (top at 2.18 m, the visor 2.25-2.45 m shows over it as a helmet peeking over a riot shield), curved as an arc of the sim's own +-60 deg
  (a cylinder section of radius 0.70 m about the body's axis, so the drawn cover equals the sim's arc), 0.10 m thick, a dark frame + a
  vertical amber slit window (the class mark: "plate"), a triangular boss at the centre (the faction's triangle). **Down** (stunned / opened):
  the forearm swings the shield out and down on a spring (k 260, damping 0.75: a 0.18 s settle) to the side at (0.95, 0.75, 0.25), its inner
  edge clear of the body by >= 0.08 m (checked per frame in the reel: the shield's bounds vs the body's capsule, 0 overlaps). Raise / lower =
  0.22 s eased swing (anticipation 0.05 s the other way), never a snap; a block hit kicks it back 4 deg on a 60-100 ms flinch spring
  (section 3 of NIGHT47.md's animation rules).
- Frames (a847): front / side / 45 deg / in a fight (block, punch-open, down), before (row off) and after.

### 5b. Visors (G13): one band family, angular, ~1.45x today's height
- **Today (B5):** six visor builds share `Mesh43.Patch()` (`Game/Mesh43.cs:235`) and the VISOR role colouring (`Enemy40View.cs:170-176`):
  husk slit 0.063 m tall x 0.58 wide; tank double slit 0.050 / 0.079; charger chevron 0.054 (0.036 at the tips); warden plate 0.29; swarmer
  3 lenses 0.057-0.074; spitter round lens 0.25 x 0.22; drone 3 lenses 0.25; lancer a cube 0.14 (no models43); burrower a mouth.
- **The spec (NIGHT47.md section 3, original, not a copy of any character):** a new `Mesh43.Visor47(height, wrap, sweep, dip)` builder: one
  band that wraps the head (+-1.25 rad, a bit past today's +-1.15), its height **1.45x today's** (husk 0.063 -> 0.092 m), sweeping back and up
  to **points at the temples** (the band's top and bottom edges converge over the last 0.25 rad into a tip 0.06 m behind and 0.025 m above
  the band's end), a **V dip at the bridge** (the top edge drops 0.30 of the band's height at the centre line, the bottom edge 0.15), **two
  faceted planes meeting at a horizontal crease** (the upper plane tilted 12 deg down, the lower 18 deg up: flat-shaded, so the crease catches
  a highlight line), a raised thickness of 0.035 m off the head (today 0.07 patch thickness but flush), and the emissive split in two: the
  **slit** (the band's middle 45 %, 0xFF9A3C x1.7, today's colour path) and a **bright top edge** (a 0.006 m lip, x3.0, toward white 25 %).
  The band's frame is the dark TRIM (x1.8 of the body). It reads as glasses, not a slot.
- Per class, the same band, a different proportion (the class read stays: E-2): husk / tank the base band (tank: two stacked); swarmer a
  narrow band with three slits cut in it (its "three lenses" kept as cuts); charger the band with the V dip deepened to 0.6 (the chevron brow);
  warden the band 1.3x taller (the plate); spitter / drone keep their round lens (ranged / flyer = round) inside a band frame with temple
  points; lancer gets the base band (it is the last box visor). Bosses: not touched (their builders own them).
- Proof: a grayscale silhouette sheet (the a847 phase: every enemy kind on a neutral plate, at 10 / 20 / 30 m, front and 3/4, row off then on);
  the meter is the visor's projected height in px at 30 m at 1080p (husk today ~0.063 m -> ~2.3 px at 30 m with the 60 deg base FOV; after
  ~3.4 px) plus the lead's look. GREEN when every kind on the sheet shows the band and the temple points at 20 m.

### 5c. The small robots (G14): the swarmer (and the drone)
- **Which one GG means (B5):** the only small walker is the **swarmer** (collider 0.3 / 0.75 m, drawn ~0.64 m, 0.42 of a husk), and it is the
  one players meet in numbers: iceline 29, span 14, spire 13, kiln 2 (none in foundry). The flyer **drone** (0.6 / 1.2, a saucer) is the other
  small one (iceline 5, kiln 8, spire 2). The LANTERN's sentry cube is boss-only. Today's swarmer is still NIGHT33's three-box layout with
  models43 meshes swapped in (`EnemyViews.cs:547, 584`): a body 0.46 x 0.32 x 0.8, a head poking 0.42 m forward, legs as a dome **wider than
  the body** (0.62: bottom-heavy), two fins, and colours x1 / x0.8 / x0.62 of an already dark x0.35 base (head 0.28, legs 0.22 of the base:
  it reads as a black blob with three dots). That is the "strange" look.
- **After (foes47): the swarmer as a small pill on legs, the faction's shape logic at 0.42 scale:** a pill body (0.36 r, 0.62 tall) tilted
  forward 25 deg (it leans into its pounce), the visor band across its front third (the narrow three-slit band of 5b), **four short pointed
  legs** (triangles: danger) splayed wide under it, a single dorsal triangle fin (its back part), the body at x0.35 like every enemy (no
  extra darkening of head / legs), the visor and the fin's edge carrying the emissive. Motion (view only): legs on a 2-phase gait cycle
  tied to its speed (so it scuttles instead of sliding), the pounce crouch springs (anticipation 0.1 s down, overshoot on landing).
  **The drone**: keep the saucer + ring of lenses (the flyer read) and add the faction's triangle (a downward spike, today's emitter, made the
  yellow-free red-orange danger point when it fires) and the band frame round its lenses.
- Proof: before / after silhouettes (a847, as 5b) at 10 / 20 / 30 m, plus a 2 s motion strip of the gait.

### 5d. Enemy punch auras (G21, enemy side)
- **Today (B5):** the husk's melee is a YELLOW tell (parryable: `SurvivalSim.Core36.cs:174`), a single additive halo sphere 0xFFD23C
  (`Tell36View.cs:197-206`); the swarmer's pounce a red halo 0xFF3A14; the warden's swing has no halo (`Core36.cs:196`); the brawler's fist
  glows 0xff3a14 (`Brawl40View.cs:271-297`).
- **After (fx47 enemy side, in foes47):** the same three-tone aura as the player's (section 1c), in the red-orange family for an unparryable
  blow and in the yellow family for a parryable one (yellow stays "punch it": the parry language outranks "enemies = red-orange"): red-orange
  core #ffe2b0 (OKLab h 81, L 0.93), body #ff7a30 (h 46), rim #c23a12 (h 36), halo #ffb070; yellow core #fff6c8, body #ffd23c (today's tell),
  rim #c89a10. The aura grows on the enemy's striking limb (a stretched sphere along the swing, not a whole-body sphere) over the wind-up and
  bursts into a ring in the same tones on the hit (or on the parry). The warden's swing gains its (red-orange) aura.

## 6. The render diagnosis plan (G10 door, G11 lava; rows `door47`, `lava47`)

### 6a. G10: the door turns pink (hypotheses a-d tested in code tonight; B2)
- **(a) A shader found by name and stripped: refuted in code.** Every runtime `Shader.Find` target (13 names) is Always Included (Enemy,
  Particle, BlobShadow, Motes: `ProjectSettings/GraphicsSettings.asset:29-40`) or in `Assets/Shaders/Resources/` (Ao27, Ceiling27, Crt29, Lava30,
  RinkIce28, RinkWall28, SkateFloor27, SpeedLines20, TravelBlur, UiBlur43), or is a scene material's (Stone, Field, Sky, Acid, URP Lit / Unlit
  via `Survival.unity:151-155`); `strictShaderVariantMatching: 0`. One soft spot: `PromptGlyphs.cs:71` falls back to `TextMeshPro/Sprite`
  (not Always Included; glyphs only, not the door).
- **(d) A material swap on the door: FOUND, the cause.** `Doors38View.cs:147-148`: every `Attach()` does `Destroy(mat); mat =
  FxDraw.Additive(...)`. The curtains are rebuilt with the new material, but the **camera shimmer veil** (a full-view quad 0.35 m in front of
  the camera, drawn for 0.28 s when you pass through an open door: `:433-451`) is created once, re-parented to null and `DontDestroyOnLoad`,
  and never given the new material. `Attach()` runs again whenever `game` goes null: every scene load (campaign continue
  `SurvivalGame.Campaign25.cs:43`, map change `Maps.cs:84/131/147`, level select `UiLevelSelect26.cs:323`). From then on the veil renders with a
  destroyed material = Unity's magenta error shader, **over the whole view, flickering at 90 Hz** (its scanline flicker), for 0.28 s, each time
  you run through an open door. That is "randomly turns pink or bugs out when running through" (random = only after the first level change of
  a session; never in a fresh boot's first level, which is why one-level reels never showed it).
- **(b) The full-screen passes:** the CRT pass's boss-entrance split (0.004 uv red / blue, tear 0.8: `UnityTuning.Juice38.cs:18-23`) over the
  violet / red boss curtain reads pinkish (a NIGHT39 / NIGHT43 comment already says so: `Doors38View.cs:342-351`); TravelBlur / Crt29 divisions
  are guarded (no NaN source found). A contributor, not the cause.
- **(c) NaN / Inf in HDR:** no division in the door / blur / CRT code can hit zero; no camera has Stop NaN; bloom's clamp is only set with
  look33 (`PostFx.cs:90-95`). Low likelihood; a guard is cheap.
- **The fix (door47):** the veil takes the live material on every frame it is shown (`if (veil.sharedMaterial != mat) veil.sharedMaterial =
  mat;`), and a counter `Doors38View.StaleVeilFrames` counts frames the veil was due to draw stale (row off). Second (in the row, R1): the
  veil becomes a band at the screen's edges (GG w30: no effect over the whole screen; the reticle's centre box stays clear) instead of a
  full-view quad. Third (safety): door additive colours clamped to <= 4.0 HDR per channel. **Built tonight in round 0 as the cheap certain
  slice** (the material line + the counter; dn-check 0; fingerprints EQUAL).
- **PROOF (G10's done-when, the lead runs it):** a reel in a Player (a847 sub-scenario `door`): boot the campaign at level 1, load level 2
  (a scene load: the re-attach), then 200 scripted passes through open doors at sprint, slide and dash speed across levels 1-5 (each level
  loaded in turn, so 4 re-attaches); per frame a pixel check of the frame's centre band for magenta (R > 0.8, B > 0.8, G < 0.3) and for
  NaN black / white blocks (8x8 blocks of exactly 0 or 1). **RED:** `door47` off -> StaleVeilFrames > 0 and magenta frames > 0 after the
  first level change. **GREEN:** `door47` on -> 0 and 0, plus the lead's look at 10 sampled frames.

### 6b-pre. Why not the per-object limit
The renderer is Forward+ (`PC_Renderer.asset:65`, `m_RenderingMode: 2`), so the per-object cap of R3 does not apply on PC; it would on the
Mobile renderer (Forward, 4 per object: `Mobile_Renderer.asset:48`, Android / WebGL only). Measured light counts: below.

### 6b. G11: the lava light (B3)
- **Today (code inventory, verified at LightKit29.cs:370-411 and Look43View.cs:470-487):** the lava surface (Lava30 shader) is emissive only and
  lights nothing; the light on lava comes from **two roaming real lights**: (1) LightKit29's last slot, which every **0.25 s** jumps to the lava
  fixture nearest the camera in the current room and sets position, colour, range and intensity **in one step, no fade** (foundry b2 alone has
  14 lava light positions ~5.3 m apart at 6 m range: the lit patch hops as you walk), and in a room without lava borrows any fixture (Spot <->
  Point and colour change: pops again at the next lava room); (2) Look43View's lava light, which **every frame** teleports to the pool under
  your feet with its range rescaled to the pool (foundry b2's two pits are ~49 m apart, range 27.75 m: the room's orange light jumps when you
  cross the landing) and cuts to 0 the moment the room has no lava. Plus four unrelated rhythms on one source: the surface breathes 0.8 Hz
  +-15 %, the underglow and Look43 light 0.35 Hz +-25 %, the slot is steady, braziers flicker Perlin 11 Hz +-15 %. Room changes reset every
  room light at once. Lava per level: foundry 9 fields 1972 m2 (21 planned lava lights by LightPlan33's one per 100 m2), iceline 3 / 168 m2 (3),
  kiln 3 / 336 m2 (4), span 0, spire 0. Real lights in a lava room on PC High: about 13-19 (Forward+, 256 cap: R3-R5): **the budget is not
  the cause** (it could be on a Forward / mobile renderer: 8 per object, Mobile_Renderer is Forward).
- **The rule (lava47), one sentence:** *a lava field's light is fixed, made once, faded by distance, and breathes with its surface.* Numbers:
  one real point light per lava light planned by LightPlan33 (one per 100 m2 of pit, >= 1), **created once at level build**, placed at the
  plan's positions 1.2 m over the surface, range 6 m, one intensity per theme (foundry 1.0, kiln 1.2, iceline 0.8, x LightScale), colour the
  lava hex; not filtered by the current room (lava seen through a door keeps its light) but **faded by camera distance** smoothly from 30 m to
  45 m (never a cut), and capped by a per-frame count of the nearest 12 (beyond: faded, never popped); every lava light's intensity follows the
  **one** surface breath (0.35 Hz, +-10 %: the surface, the underglow and the lights in phase; the 0.8 Hz +-15 % surface breath retired under the
  row); LightKit29's lava slot and Look43's roaming lava light are **off** under the row (the slot goes back to an ordinary fixture, kept by it
  for the whole room, never switching Spot / Point mid-room); room-light changes cross-fade over 0.4 s instead of a cut. On a Forward renderer
  (mobile) the lava lights are marked `LightRenderMode.ForceVertex` beyond the 4 nearest.
- **PROOF (G11's done-when):** a reel (a847 sub-scenario `lava`) walks a fixed path past every lava field on levels 1-3 (span / spire have none:
  logged as 0 fields) and logs per frame the visible real light count and the luminance of 3 fixed probe points per field (a camera-space
  read of a 9x9 block at a fixed world point on the floor 3 m from the pit edge). **RED** (row off): count jumps and >15 % luminance jumps
  between frames at the 0.25 s re-pick and the landing crossing (expected: many). **GREEN** (row on): no frame-to-frame jump > 15 % that is not a
  scripted flare (kills / impacts are logged and excluded), the light count changes by at most 1 per frame, 0 lights created after the build.

## 7. The PROOF plan (every row RED once and GREEN once: L-014; frames named for the lead)

| G | row | meter (harness / Player log) | RED (row off) | GREEN (row on) | frames (reel phase: what) |
|---|---|---|---|---|---|
| G10 | door47 | `Doors38View.StaleVeilFrames` + the magenta / NaN block pixel check over 200 door passes, levels 1-5 | > 0 after the first level change | 0 / 0 | a847 `door`: a pass at sprint / slide / dash after a level change, off and on |
| G11 | lava47 | per-frame visible light count + 3 probe luminances per lava field (levels 1-3) | jumps > 15 % at re-picks / crossings | none > 15 % unscripted; count step <= 1 | a847 `lava`: foundry b2 walk, 0.25 s apart, off / on |
| G12 | foes47 | shield bounds vs body capsule overlap per frame; shield arc vs the sim's +-60 deg | overlaps when down; 48 deg vs 60 | 0 overlaps; drawn arc 60 | a847 `shield`: front / side / 45 / block / punch-open / down |
| G13 | visor47 | visor projected px at 30 m (1080p) per kind | husk ~2.3 px | ~3.4 px, temple points visible at 20 m | a847 `visor`: grayscale sheet 10 / 20 / 30 m, every kind |
| G14 | foes47 | silhouette sheet + gait strip | 3-box blob | pill + legs + fin | a847 `small`: swarmer + drone, 10 / 20 / 30 m, 2 s gait |
| G15 | copy47 | every string in section 3 shown; the copy rule check (<= 10 words, verb first, glyph shown) as a reel log line per string | old strings | new strings, 0 rule misses | a647 `copy`: gauntlet pickup (both variants), game start (both), each teach card |
| G16 | hud47 | bar colour sampled at 6 shares vs the OKLab stops (delta E < 0.02) | 10 cells | one fill, the 6 stops | a647 `health`: 100 / 80 / 60 / 40 / 20 / 5 % |
| G17 | overheal47 + hud47 | `n47feel overheal` (sim) + the bar's second layer | hp never > 100 | 110 / 130 / 150 shown, hue <= 295 | a647 `overheal`: 110 / 130 / 150 |
| G18 | hud47 | counter text + px height | 36 px + pips | 48 px "6 LEFT", line, LAST ONE, CLEAR | a647 `counter`: 12 / 3 / 1 / 0 |
| G19 | hud47 + fx47 | non-reticle HUD pixels in a 160 px centre box | > 0 (the arc) | 0 | a647 `reticle` + a747 `gunread`: KESTREL charge, THUNDERCLAP charge, PULSE pips, BLAST cooldown on the gun and the panel |
| G20 | pulse47 + fx47 | `n47feel blast` knockback on a husk (B1: **1.40 m**) | 1.40 m, 64 dmg | >= 2.8 m (card aims ~10 m) | a747 `blast`: contact sheet -2..+30 frames, 2 look variants for the blind pair |
| G21 | fx47 + foes47 | aura tone hues sampled (3 per punch) | one pink tone | 3 tones per family, hue spread inside the family | a747 `punch`: fist / gauntlet / husk / warden: start, hit, fade |
| G22 | zoom47 | world FOV at 150 ms after LT (want 80 % of base); the gun's screen-space barrel line vs the reticle; VERBS 29 air boost latency 0 ms (the harness `latency34` line, unchanged: a view row cannot change the sim) | today's pose | FOV 0.80x in 150 ms, barrel on the reticle line, no roll | a747 `zoom`: hip / mid / full |
| G23 | grapple47 | the line's draw (one ribbon, no dots); width core / shell | dotted | ribbon, 3-4x shell | a747 `grapple`: aim / fire / pull / release / BEND |
| G24 | fx47 | contact sheets per weapon (fire -2, 0, +1, +2, +3, +4, +6, +8, +12, +18, +30) + camera punch peak deg and settle s per tier (section 1's targets) | today's sheets | the weight targets met; blind pair per weapon (3 judges; a tie keeps the new) | a747 `sheet-<weapon>`: KESTREL tap / charge, THUNDERCLAP shot / slug / boost, PULSE punch / blast, TORCH, fist, swap |
| G43 | fx47 | TORCH contact sheet + flame cone px coverage vs the reticle box | today's | the flame spec | a747 `torch` |
- The sim rows (pulse47, overheal47) are proved in the harness first (`n47feel blast`, `n47feel overheal`), with `fingerprint`, `--off46`,
  `--off47` EQUAL and golden 0 / 22 exact at each commit; the view rows are proved only on Player frames (the lead's), with dn-check 0 at
  each commit. Every reel sub-scenario runs its off pass then its on pass in one Player (a view row is forced per pass through a static
  `Force` the reel sets, as Feel46View / Shift46View do), so one Player gives RED and GREEN.
