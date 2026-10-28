# Wave 49 code facts (read-only map, 2026-10-19 ~11:30, port/stage3 at u3-n48 a8fe95cd). Re-check them in the run.

Paths under Assets/Scripts/Game/ unless given. Rows = design switches in Design/unity-config.json read by NightRows.ViewOn(cfg, "<row>").

## 1. The charge bar under the reticle (G1)
- It was arc40: one curved bar under the reticle (UiHud38.Arc40.cs:1-14, built UiHud38.cs:177). meter42 drew on the same arc:
  REST / CHARGING / FULL / COOLDOWN plus inner cooldown lines (UiHud38.Meter42.cs:1-16, chosen UiHud38.cs:282-283). The older hud33
  piece: two charge arcs closing on the centre dot (UiHud33.cs:710-721). The BEND tick: a cyan slash beside the grapple ticks from
  sim.Bend34Meter (UiRail34.cs:356-387).
- hud47 removed it: UiHud38.cs:339 `if (hud47Built) show = false;` (arc40, meter42, the hud38 cells); UiHud33.cs:720 empties the dot arcs
  when hud47Built; UiHud47.cs:173-191 moves the grapple row and the BEND slash to the weapon panel. hud47's row: unity-config.json
  ~7905 (enabled true), read UiHud47.cs:139-145.
- The reticle now: UiHud33.cs:690-753 (four ticks, the kill turn, tick colours; cyan at full charge, :726).
- Charge shown elsewhere today: the gun (Viewmodel.Charge35.cs:1-9: muzzle tip, barrel glow, white flash), the rail label
  (UiRail34.Charge35.cs:4), hud47's PULSE pips / BLAST cooldown / STITCHER heat (UiHud47.cs:441-456, from UiHud33.cs:621).
- Minimum to bring it back behind a row: gate UiHud38.cs:339 (and UiHud33.cs:720) on the new row instead of hud47Built; the data is
  computed every frame at UiHud38.cs:243-284 and passed to Hud40Math.Read / Hud42Math.Read.
- Sim data: KESTREL / THUNDERCLAP charge `lo.charge27` (ChargeShots27), `c35.holding/gun` (Core/Guns/Charge35.cs:45),
  `lo.Charge35Level` (:181), `cs.Level(gi)` (Core/Guns/ChargeShots27.cs:118), `cs.cooldown[gi]`, `ChargeShots27.Index(gun)`
  (UiHud38.cs:251-263); the slug is the same level with gun "shotgun" (ChargeShots27Params.slug, :30); PULSE blast `sim.pulse27`
  (Core/PulseCharge27.cs: pips, cooldownSteps :60, BlastCooldownSteps :112, CooldownShare :120); BEND `sim.Bend34Meter`,
  `Bend34TierNow`, `Bend34Inside` (SurvivalSim.Bend34.cs:51-57).

## 2. Post-processing
- The main Volume is built in code: Look.cs:141-190 (Bloom :149, Tonemapping ACES :155, ShadowsMidtonesHighlights :163,
  ColorAdjustments exposure + saturation :172, Vignette :178, ChromaticAberration + FilmGrain at 0 rest :184-187). PostFx.cs drives it
  per frame; look33 retunes bloom and switches tonemapping to Neutral (PostFx.cs:89-97, :388); lights35 changes the bloom threshold (:98).
- More Volumes: SlowAim27.cs:80-112 (two: ColorAdjustments, WhiteBalance, ChromaticAberration, Vignette);
  SurvivalGame.SkygrindTime20.cs:74-78 (ColorAdjustments); Bend34View.cs:67 adds ColorCurves to the main profile.
- No LUT / ColorLookup. PC_RPAsset.asset:80-81 ColorGradingMode 0 (LDR), LUT 32. Volume update Every Frame, PostFx switches it to Via
  Scripting (PostFx.cs:12-17).
- Cameras: main camera HDR, post on, SMAA (Look.cs:119-131); an overlay viewmodel camera in its stack (Viewmodel.cs:213-227).
- A per-level grade EXISTS by theme: PostFx.ApplyTheme33 (PostFx.cs:395-408) sets bloom, exposure, saturation from
  UnityTuning.Look33.cs:54-56.

## 3. Palette and theme
- UnityTuning.Theme.cs holds mixes and alphas (ink, muted, cyan deep / pale / hot, :32-44), not hues. The hues are the config `colors`
  block (Assets/StreamingAssets/PortExport/config.json ~7240: skyZenith, pillarTop, usable, lightRim ...): the web's colours.
- Levels carry no colour / fog / ambient / sky fields. The theme index comes from the level name (foundry / iceline / kiln / span /
  spire), LightPlan33.cs:120-129. Per-theme in code: fog density + colour UnityTuning.Look33.cs:58-59; ambient sky / equator / ground
  :60-62; key light colour UnityTuning.Look41.cs:12; applied RoofView27.cs:209-221 and PostFx.cs:442-458.
- The one colour field a level sets: campaign.lights[].color. In use: cyan 1604, white 1219, amber 311, green 137, orange 136. Mapped
  at LightKit29.cs:97-107: orange -> colors.enemy, ANYTHING ELSE -> colors.usable (cyan). This is why cyan is everywhere.
- Lava colours UnityTuning.Lava30.cs:9-11; sky Look.cs:62-69 from colors.sky*.

## 4. Lighting
- Look.cs: linear fog (:73-77); the stone's height fog via _SkyFog* globals (:82-87); trilight ambient (:90-94); one directional key
  with soft shadows (:97-107); the rim is a shader global, not a light (:110-113).
- Real lights: 27 AddComponent<Light>() sites; caps LightKitRealLights 3 (UnityTuning.Lights29.cs:17), Lava47MaxLit 12
  (UnityTuning.F47.cs:285), per-object 4 (PC_RPAsset.asset:48). Forward+ on PC (PC_Renderer.asset:65); the active quality is PC.
- Stone.shader: albedo = vertex colour (pillarTop / pillarSide) x grid (:1-2, :84, :132); lighting albedo * (SH*AO + direct) (:160);
  NO emissive term. Emissive fixtures are boxes (LightPlan33.cs:44-56).

## 5. VFX
- Files: Fx47View.cs (the LT blast), Fx47View.Weapons.cs (sparks, kill flare, cone, boost, swap), GunFx.Hits43.cs, GunFx.Torch43.cs,
  GunFx.Zapper.cs, GunFx.Bouncer.cs, GunFx.Spray20.cs, Tell36View.cs (tells, parry), HitSpark.cs, ImpactFx.cs, KillConfirm.cs,
  KillFeel42.cs, ImpactFrames.cs, WeaponFx.cs.
- Shared: FxSprites (FxSprites.cs:18) sprite batch, but each effect makes its own instances; Fx43Pool (GunFx.Pool43.cs:28-51) shared
  by Hits43 and Torch43 only; LightPools.Flash and FlashGate (ImpactFrames.cs:226).
- NO shared impact kit and NO colour / timing table: each effect sets its own in its own UnityTuning.*.cs (F47 Fx47*, Impact, Kills42,
  Tell36).
