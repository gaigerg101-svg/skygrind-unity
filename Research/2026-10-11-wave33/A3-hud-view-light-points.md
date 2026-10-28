# A3: HUD, viewmodel, lighting, campaign points (read-only audit, u3-n32, 2026-10-11)

Paths are under `Assets/Scripts/Game/` unless written out. Px are at 1080p. "Computed" means worked out from config numbers, not measured in a frame.

## 1. HUD

**Canvas.** `Ui.cs:204-211`: one Screen Space Overlay canvas named "UI", sort order 10, ScaleWithScreenSize at 1920 x `ui.theme.scaleBaseHeight` (1080), match = 1 (scales with height). It has four layers: frame / values / sight / menus (`Ui.cs:215-218`), plus metaLayer, which holds the lesson line. HUD SCALE rewrites the reference resolution (`UiSettings.cs:723`). Other canvases: Hud29 (sort 11, `Hud29.cs:99-102`), GrapplePips32 (sort 12, `GrapplePips32.cs:49-52`), GlacierView28 (40), and the ImpactFrames / MovementFx / PostFx frame canvases.

**Fonts.** The theme fonts are already converted: `Assets/Resources/Fonts/*.asset`. CLAUDE.md rule 8 ("TMP default until converted") is out of date. `UiTheme.cs:42-46` loads Display = Archivo ExtraCondensed Black Italic, Body = Archivo SemiBold, BodyBold = Archivo ExtraBold and Rank = Tourney Black Italic (`UnityTuning.Theme.cs:18-28`). The older `Text()` helper loads ChakraPetch and Barlow (`Ui.cs:153-154`, `UnityTuning.Ui.cs:17-19`). The type scale is 18/24/32/42/56/75/100/133 (`UnityTuning.Theme.cs:48`). The text floor is 18 px (`:49`; `UiKit.cs:91`). The safe inset is 56 (`:63`).

| Element (as seen) | Built in | Layout numbers |
|---|---|---|
| Bottom-left health box | `Ui.cs:294-332` (HP panel, S42 numeral, slanted bar, slits); stamina pips `UiStaminaBar.cs`; segment ticks `UiVitals20.cs`; speed row `UiMoveRow.cs` | `UnityTuning.Hud17.cs:10-16`: 380 x 104 (+10 `MoveRowPlateGrow`, `UnityTuning.FeelHud.cs:106`); bar 320 x 18 |
| Big yellow points | `Ui.cs:336-340` (S75 gold); plate, odometer and gold bar in `UiHudMotion.cs:135-143` (BuildPoints17) | `Hud17PointsBox` 92, bar 4, gap 2 (`UnityTuning.Hud17.cs:23-29`) |
| Bottom-right weapon card | Plate and block `UiHotbar.cs:124-196`; hud30 replaces the silhouette row with the 5-slot widget `HudWeapons30.cs:172-216`, laid out by `Hud30.cs:111-136` (FitWeaponBlock30) | `UnityTuning.Hud30.cs:9-21`: ring 56 px, **pitch 90 px** (sized so "SWITCHING" fits, which leaves about 34 px between rings), word box 86, raise 8. Card bottom = 56+92+4+4+16 = **172 px** from the bottom edge; height ≈ 145 px; 5 slots ≈ 470 px wide (computed) |
| ZOOM / PUNCH / PARRY words | `HudWeapons30.cs:322-333` reads `Loadout.LtLabel31` (`Core/Guns/LtAlways31.cs:117`) | `FitWord32` (`UiHud32.cs:65-71`) lets a word overflow |
| Card follows gun sway | `UiHotbar.cs:405-423` + `HudAttach18` (`UiHudMotion18.cs:216`), written at `Viewmodel.cs:762` | Only with FeelAB on |
| Top-right "D DULL" | `UiStyleMeter.cs:94-151`; the old rank box is built hidden (`Ui.cs:371-391`); x1.2 chip docked by `Hud30.cs:87-95` | `UnityTuning.Hud17.cs:55-64`: top 130, letter 120 px, block 456 wide |
| HUD scanlines / flicker | `HudFx30.cs` (look30) | |
| Boss bar | `UiThreat20.cs:65` (bottom centre) | `Hud20ThreatBottom` |

**View rows that switch the HUD:** `hud30` (`Hud30.cs:46`), `hud32` (`UiHud32.cs:28-39`), `look30` (HudFx30), FeelAB (motion, HudAttach), `ceremony.hudMotion` / `ceremony.round` (`Ui.cs:158-161`), `design.n24.reticleArc`, `n31.lowHealthEdge` (`UiEdge31.cs`), `n32.grapplePips`, and `design.zoom` (UiZoom20 instead of UiAds). They are switched by `N30Fixes/N31Fixes/N32Fixes.ViewRowOn` (`N32Fixes.cs:16`); `-skygrindOff30/31/32` turns them off.

### Top-centre messages and why they overlap

| Message | Source | Rect (from top) |
|---|---|---|
| Top line (ROUND / objective) | `Ui.cs:343`; filled at `:931-963` (empty in a campaign) | 56-88 |
| ROOM n · NO DASH (limit) | `UiCampaignCue24.cs:46-76` | 90-134 (`UnityTuning.N24F.cs:33`) |
| **Main banner**: room title "1.3 DASH" (zoneEnter), "DOOR" (door zones), **"<zone> OPEN"** (gateOpen), boss "... DOWN" | `Ui.cs:348-354`, `:489-502`, Banner queue `:618-643` | **170-240**, auto-size 24-42 px |
| **DOOR OPEN / "7 m · AHEAD"** | `UiCampaignCue24.cs:38-43`, `:86-131` | **150-214** (`Cue24Y` -150, 2 lines x 32) |
| Directed cue (survival) | `UiDirected22.cs:42` | 150-270 |
| Boss gap bar | `Hud29.cs` | top 150 |
| Lesson / boss line | `UiLesson25.cs:37-55`: **bottom** centre, 230 px up (`UnityTuning.Lesson25.cs:7-9`) | not top |

**Why they overlap.** A campaign room is a zone, and so is each door (`LEVEL-FORMAT.md:42`; zone names in `n32-foundry.json` are "1.2 STEP" ... and "DOOR"). When a room clears, `Zones.Open` fires gateOpen for the exit door and for the next room, so two banners queue ("DOOR OPEN", then "1.3 DASH OPEN"), each `zones.banner.time` = 2.4 s (`Ui.cs:502`). The cue waits for banners for at most `Cue25MaxWait` = 2.0 s (`CueWait25.cs:9-14`, `UiCampaignCue24.cs:96-101`). After that it shows anyway, and once it is up it never yields (`alreadyShowing`). The banner rect (170-240) sits on the cue's second line (182-214). The fix belongs to whoever owns the top centre: one message slot, or a stacked layout.

### Reticle pieces

| Piece | File |
|---|---|
| 4 crosshair arms (10 x 2) | `Ui.cs:235-246` |
| Head diamond (headshots only) | `Ui.cs:248-252`, `UiFeelHud.cs:436` |
| **Circle**: momentum ring r 20, w 2 | `UiMomentum.cs` (config `hud.momentumArc`) |
| **Arc**: cooldown smile 140 x 5, 28-42 px under the centre | `UiReticleArc24.cs`, `UnityTuning.N24F.cs:11-24` |
| **Two charge pips** (diamonds, 48 px right, 32 px down) | `GrapplePips32.cs`, `UnityTuning.Grapple32.cs:7-15` |
| **Diamond**: grapple target mark, a 3-D world mesh | `Grapple31View.cs:54-81, 152`, `UnityTuning.Grapple31.cs:9-16` |
| ADS ring / scope | `UiAds.cs`, `UiZoom20.cs` |
| Dash pips, slide tick; words under the crosshair | `UiFeelHud.cs`, `UiFeelTags.cs` |

## 2. Viewmodel

| Item | Where | Value |
|---|---|---|
| Camera | `Viewmodel.cs:206-226`: an **overlay** camera, child of the main camera, in its URP stack, layer 30 (`:50`), no shadows | FOV `viewmodel.fovDeg` **54, fixed**; near 0.02 / far 10 |
| Main FOV | `CameraFeel.cs:328`; zoom returned by `Viewmodel.cs:922` and applied at `SurvivalGame.Render.cs:40` | `camera.fovDeg` 74 |
| Gun builders | `Viewmodel.cs:329-442` (Kestrel, Thunderclap, Gauntlet, Stitcher, Comet, Longwave) on `BlockyKit.cs:205-568`; `Viewmodel.Ports.cs:203/219` (Zapper, Bouncer); `Viewmodel.Ports20.cs:160/178`; arm `Viewmodel.Arm.cs`, `BlockyKit.cs:593` | |
| Hold pose | `Viewmodel.cs:474-523` (GunOf) | `viewmodel.holdX/Y/Z` = 0.135 / -0.147 / -0.36, yaw 5, pitch 1; per-gun `viewmodel.kitOffsets.<w>`; `HoldTurn` / `HoldNudge` / `Presence` 1.07 (`Viewmodel.Draw.cs:448-468`, `UnityTuning.Hands.cs:245-251`) |
| ADS pose | same; `ads.default` = 0.02 / -0.11 / -0.30, roll **-4**; revolver roll -5, rail roll -2; nailgun pose in `UnityTuning.Fx20.cs` (`Viewmodel.cs:496-506`) | ease `ads.classEase` 0.13 / 0.18 / 0.25 s |
| **LT path** | LT held → `Core/Guns/Zoom20.cs:94-104` sets `adsActive` (gun zoom > 1) → `Viewmodel.cs:892-908`: `adsAmount` eased, `Mix(hold, ads, Smooth)` on x / y / z / yaw / pitch, **roll added**, sway damped by `ads.vmSwayMult` 0.25 | Computed: the gun goes from about 400 px right of centre to about 70 px right, yaw 5 → 0, rolled 4-5 deg. That is the "forced angle" |
| Sway / bob / poses | `Viewmodel.cs:724-797` (breath, bob, look sway, strafe roll, slide, wall tilt, air lift, dash pull, land dip, wall pull-back); `Viewmodel.Feel.cs:879` FeelPose | |
| Recoil / kick | `Viewmodel.cs:811-859` (spring bank by class), `Viewmodel.Feel.cs`, `Viewmodel.Springs.cs`, alt-fires `Viewmodel.Alt19.cs`, draws `Viewmodel.Draw.cs`, camera punch `:919-921` | |

The hold and ADS numbers live in the web's `PortExport/config.json`. Changing them for Unity means a Unity-side override (precedent: NailgunAds in `UnityTuning.Fx20.cs`; rule 5).

## 3. Lighting

**Global look.** `Look.cs:60-134` sets the sky, linear fog plus the stone height fog (`:73-87`), Trilight ambient (`:91-94`), one soft-shadow directional key light (`:97-107`) and a rim global. Post-processing is a runtime VolumeProfile (`Look.cs:141-190`: Bloom, ACES, ShadowsMidtonesHighlights grade, ColorAdjustments, Vignette, Chroma, Grain), driven each frame by `PostFx.cs`. SlowAim27 adds its own Volume. `Render/Crt29Pass.cs` does look30's quantise and dither. **URP:** `Assets/Settings/PC_RPAsset.asset` is the default and the "PC" quality level (current = 1): HDR on, MSAA off, shadow distance 60, 2048 shadow map. `PC_Renderer` is Forward+ (`m_RenderingMode: 2`) with Travel Blur and SSAO. **Light probes, reflection probes, lightmaps:** no script references found; the scenes were not checked.

**Campaign build chain:** `CampaignLighting24.Create` → Build (`CampaignLighting24.cs:39-77`) → WallRideStrips26, RoofView27, LightKit29 (→ DoorFrames29, PropsView29), SkateFx27.

| Builder | What it draws | Where the position comes from |
|---|---|---|
| `CampaignLighting24.cs:55-65, 92-101` | 4 neon trim cubes per room, 0.5 x 0.12, brightness 2.5 | The room rect inset 0.6 m, at y 0. **Pits, lava, tunnels and boxes are ignored.** In the 5 n32 plans the trims cross a pit or lava trench **31 times**, so glowing bars hover 0.6-1 m above the lava |
| `CampaignLighting24.cs:70-118` | Room light (range 38) and pulsing guide light | Room centre at 7 m; the room's exit |
| `RoofView27.cs:73-165` | Violet ceiling plus **cyan and pale violet-white strips** | **One level-wide height** = tallest box + `ceilingAbove` 0.5 (`:73-76`), on each room's perimeter inset 0.4 m. The config `_why` says "each room closes at its own top", but the per-room version was reverted (aeb29b18). Every n32 room has `roof: true`, so the roof slabs should hide these; not checked in a frame |
| `LightKit29.cs:132-189` | Wall / ceiling fixture slabs (OrientedBox), neon x `roof27.stripBrightness` 2.2; **"white" = boundary violet-white** | The plan socket `at` + normal (`LevelPlan29.cs:6`, validated flush by `MapAuthoring/LevelValidate29.cs:5`). There are 9-20 white **ceiling panels** per n32 level, hung ≤0.05 m under the roof slab. My check: 0 sockets off-surface in 4 of 5 plans; kiln `k1-L11` (amber, [0, 5.5, -6.95]) is not on a room wall or a plan box face (it may sit on a box levelgen adds: not checked) |
| **`LightKit29.cs:210-271` (look30 beams)** | Additive crossed-quad **light shafts** | Each pool socket gets a vertical shaft **6 m tall rising from the floor** with no fixture at its top (`Look30BeamPoolHeight`). Each amber / orange socket gets a 5 m shaft tipped down. About 11-21 per n32 level (counted from the plans). **This is the best match for "light beams placed and floating"** (`UnityTuning.Look30.cs:44-51`) |
| `PropsView29.cs:117-149` | Pickup glow columns (cyan / green) | The prop `at` |
| `DoorFrames29.cs`, `WallRideStrips26.cs` | Door jamb neon, wall-ride strips | Door rect; wall boxes |

**Lights per room (code caps):** room + guide 2; LightKit29 3 (`UnityTuning.Lights29.cs:17`); RoofView27 2 (`maxLights` 4 - 2); LightPools ≤3 + muzzle flash at High (`LightPools.cs` header); 1 directional. Every point light has shadows off. Boss arenas are separate geometry at far origins (e.g. `design.n32.boss5.room.origin` [3200,0,0]; `SurvivalSim.Crown32.cs:53`), so plan fixtures never light a boss fight.

## 4. Points in the campaign

| Question | Answer (file:line) |
|---|---|
| Earned | `Core/Economy.cs:162-179` (hit / parry / kill x rank multiplier) |
| Start | `Core/SurvivalRun.cs:275` `economy.Start()`: every level starts at `points.start` 500. **`Economy.Start(double? bank)` already takes a carried bank** (`Economy.cs:189-196`) |
| Shown | The HUD's gold S75 numeral only (`Ui.cs:338`). Campaign RESULTS has no points row (`UiCampaignResults25.cs` header) |
| Spent | **Nowhere.** n32 level JSON has no wallBuys / perk / box spots; `Hud30.cs:10-12` says "the campaign has nothing to buy". A campaign run saves no records (`SurvivalGame.Campaign25.cs:4`) |
| Save | `Core/Profile.cs` (levelBest `:47`, read `:188`, write `:353`); `LevelBest26` `Core/Rank26.cs:152`; `SaveProfile` `SurvivalGame.Meta.cs:16`; pref `skygrind.campaign.next` (`SurvivalGame.N25.cs:47`) |
| **Shop hook** | `ContinueCampaign()` (`SurvivalGame.Campaign25.cs:25-42`) runs between RESULTS and the scene reload: open a shop screen there, or on `UiLevelSelect26.cs`. Store the bank and upgrades in Profile and pass the bank to `Economy.Start` |

Upgrades that change the sim (HP, damage) change the fingerprint and the campaign tape (format 8, `Campaign24TapeTests`). They must be run setup recorded in the tape and reset per run, and their numbers go in a Unity-side config file (rules 5 and 6).

## 5. Level format and new props

The pipeline is `Tools/night/gen-n32-*.cjs` → `Design/levels/n32/<id>.plan.json` → `levelgen` (`LEVEL-FORMAT.md:8`; `Assets/Editor/Maps/LevelPlan24.cs`, `LevelPlan29.cs`) → `Design/levels/<id>.json` + `Reports/LEVEL-<id>-validate.md`. The Player copy goes to `Assets/StreamingAssets/Design/`.

| Element | Keys |
|---|---|
| rooms | id, name, kind room / link / boss, rect, par, spawn, exit, nodes, waves, limit, award, wallRoute, hazard, `height`, `roof` (`LEVEL-FORMAT.md:40-58`, `LevelPlan29.cs:3-7`) |
| doors | id, between, rect, checkpoint, `height` (lintel) |
| boxes | `{zone, kind deck / slab / hull, min, max}`; ramps, pads, grapple |
| pits (`lava`), tunnels | `LEVEL-FORMAT.md:70-88` |
| lights | wall / ceiling / pool sockets (`LevelValidate29.cs:35`) |
| props | crate, pillar, console, press, floorStrip, cable, padMarker, frost, health / weaponPickup (`PropsView29.cs:104-165`). **heatVent, checkpoint, plinth and slot (used by the n32 plans) fall to the crate default (`:157`)** |

**Adding pipes, railings, catwalks, crates, vents, fixtures:**
- View: a new `case` in `PropsView29.cs:104`.
- Collider: `collider: true` on the prop row, which `LevelPlan29.cs:81-91` turns into a **hull AABB** (axis-aligned even with yaw).
- Walkable catwalk: a `deck` box in `boxes`.
- Placement: the validator's rule (floor / box top / roof / wall, `LevelValidate29.cs:9`) and the "enemies reach every tier" row.
- Railings as hull boxes also block nav.

## 6. Risks

| Risk | Detail |
|---|---|
| Tests | `Assets/Tests/EditMode/Game/`: Hud19Tests (Hud19Layout rects), HudWeapons30Tests (Slot30 words), Hud30Tests (layout check), HudOverlap25Tests, CueWait25Tests, Lesson25TextTests, FeelHudTests (hotbar labels never cut), Hud17 / Hud20 / HudMotionTests (envelopes), ZoomLookTests (**nailgun ADS stays near the web default**), HandsTests (57), ViewmodelN18Tests, Lights29Tests, Props29Tests, Look30Tests, ProfileTests, Rank26Tests, Campaign24TapeTests. These are pure rules; none pins a HUD position |
| Reels | They read the statics `Ui.Lesson25TextNow` (21 references), `Ui.Weapon30Ids` (10), `Hud30.On`, `Hud30Layout.Report` (`FeelReel.N30.View.cs`), `GrapplePips32.*`, `Ui.Cue24TextNow`. Removing them breaks compiles |
| Smoke | `SliceSmoke*.cs` reads only `UiView.RadialShown / RadialHighlight / SettingsOpen / GhostSplitVisible / RouteDrawn`; no HUD widget |
| Shared objects | `weaponBlock` / `weaponPlate` are used by UiCooldown20, UiWeaponSwitch25, HudFx30 and HudWeapons30. Build order `Ui.cs:398-430` (startup NRE history: a Player reel after any Ui change) |
| L-035 | A regex scan of `Game/*.cs` field initialisers found no Unity API calls (rough check). GrapplePips32 shows the safe pattern (objects made on first use) |
| Proof | Put new views behind a `design.n33.*` row so `-skygrindOff33`-style RED / GREEN halves work |
