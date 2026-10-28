# Research 2026-10-06: speed lines (rebuilding the sprint / boost speed cue)

One read-only research agent, 2026-10-06. GG's brief: the sprint / dash speed lines look forced ("that weird border that
only appears ... it needs to be much more dynamic and intelligent in terms of how it shows up ... copy open source
projects, go actually go into the GitHub").

Markers:
- **[lic]**: the licence was read from the repo's LICENSE file.
- **[api]**: `license.spdx_id` from `https://api.github.com/repos/<owner>/<repo>`.
- **[read]**: the page was fetched and read.
- **[snippet]**: only a search engine's summary was seen. The page wasn't read (often HTTP 403), so the item is
  **unverified**.
- **(inferred)**: no source; my reasoning.

D-72 (THIRD-PARTY.md) allows CC0, MIT, BSD-3, OFL and Unlicense, with each licence read at its LICENSE file before the
import and vendored beside the code. GPL, no licence and NOASSERTION are rejected below.

---

## 0. Today's look (read from the code at `d4673c5`, branch `port/stage3`)

Five things draw "speed" today. Three of them are screen-locked edges.

1. **The run band: HUD layer "Speed lines"** (`Assets/Scripts/Game/UiFeel.cs` `BuildFeel`, `StreakSprite18`;
   `CameraFeel.cs` `CameraMove.StreakAlpha` / `LinesTarget` / `LinesStep`; numbers in `UnityTuning.CameraMove.cs`).
   - What it is: a full-screen uGUI Image, the first child of the HUD's frame layer.
   - The sprite: a baked 512² texture of `LinesStreaksN18` = 150 radial streaks. Each streak starts at 0.74-0.80 of the
     image's half-size box (`LinesRunInner`, `LinesRunSpread`), and the sprite is masked off the HUD's bottom corners.
   - Its alpha follows the speed's share of `slide.floorSpeed` (11.6 m/s):
     - 0 under 70 % (`LinesGate`)
     - a smoothstep up to `LinesMaxAlpha` 0.45 at 100 %
     - a flat `LinesSprintAlpha` 0.4 whenever the stamina says "sprinting"
     - never over `LinesQuietAlpha` 0.28 at or under the gate
   - It fades in over 0.03 s and out over 0.6 s.
   - Its only motion: the image slides at most 5 % of half the canvas toward the sideways travel (`LinesShiftN18`). It
     has no jitter (`LinesJitterN18` 0), and **the streaks never move along their own length**.
2. **The boost layer: HUD layer "Speed lines boost"** (`UiFeel.cs` `BoostStreakSprite18`, `CameraMove.BoostStreakAlpha`).
   - The sprite: a baked 1024×548 texture of `LinesBoostStreaks` = 50 thin radial lines (1.5-3 px). They run from 0.63-0.67
     of the half-size box to the screen's edge.
   - Its alpha: 0 at or under the top speed, rising to `LinesBoostAlpha` 0.8 at 1.8× the top (`LinesBoostTarget`).
   - It is whole on the boost's first frame (`LinesBoostFadeIn` 0.01 s).
3. **The dash edge tint** (`MovementFx.cs` `BuildTint` / `RenderTint` / `DashTint18`; config `feel.dashTintOpacity` 0.6,
   `dashTintWidth` 60, `dashTintOffset` 8). It is the web's CSS `box-shadow`: four blurred edge fringes on their own
   canvas, cyan / cyan-pale in N18, at 0.55 on the sides and 0.25 on the top and bottom, × the dash kick. It is literally
   a border that appears on a boost.
4. **The sprint's world lines** (`MovementFx.cs` `SpawnSprintLines`; `UnityTuning.Crunch.cs`).
   - Rate and shape: 40 lines a second (`SprintLineRate`), each 2 m long, 0.3 s life and 0.03 m wide.
   - Placement: only in the screen's outer band, 0.85-1.0 of the view ellipse (`SprintLineEdgeMin` / `Max`), 2.5-6 m
     ahead, laid along the run.
   - They are on/off on the "sprinting" flag, not on speed. Being edge-only, they also read as a frame.
5. **Others:**
   - the wall-ride streaks and the slide's low lines and sparks (world space, surface-specific)
   - the gust's wake (a band at 0.45-1.05 of the view)
   - chromatic aberration by speed (`post.chromaSpeed`, from 14 to 32 m/s, plus `chromaDash`)
   - the FOV:
     - the base `camera.fovDeg` is 74
     - the web's speed ramp is `fovBoostDeg` 8 from 8 to 24 m/s
     - NIGHT18 adds a sprint widen of +5.95 (`SprintFovDegN18`), a boost kick of +11.8 (`BoostFovKickDegN18`, back over
       0.36 s) and a slide widen of +7

The speeds involved (config):
- walk 8 m/s (`ground.walkSpeed`)
- sprint 10.5 m/s (`Design/unity-config.json` `design.stamina.sprint.speed`)
- slide floor 11.6 m/s
- the boost (`dash.speed`) 24 m/s for 0.25 s
- the dash-jump chain averages 2.17× walk (≈ 17 m/s, the stamina block's `_why`)

**Why it reads as "a border that only appears"** (inferred, with NIGHT18's reviewer quotes from `NIGHT18-REPORT.md` and
`UnityTuning.CameraMove.cs`):
- Layers 1-3 are **pictures fixed to the screen**. Nothing travels along the streaks, so the eye reads a vignette turning
  on rather than air rushing past. A NIGHT18 round-3 reviewer said "[N18] a short, dense band at the edges ... reads as a
  frame, not as travel".
- **They are triggered by state, not by motion.** "Sprinting" gives a flat 0.4 band whether you run at a wall or across
  a plain, forward or sideways. "Dash" gives the tint.
- **They are centred on the screen, not on where you are going.** A strafe boost still bursts from the crosshair: the
  shift is at most 5 % of half the canvas.
- **They ignore the world.** A wall 1 m away and open sky get the same lines, and lines draw "over" geometry the player
  is flying past.
- **Density is fixed**; only the opacity changes. Real optic flow gets denser, longer and faster with speed and with
  nearness.
- The reviewers also kept preferring NIGHT17's long lines that reach the centre on a boost (NIGHT18-REPORT question 3), so
  the 60 % clear box is itself in question.

---

## 1. Open-source candidates (GitHub first)

### 1.1 Summary table

| # | Repo / source | Licence | Engine / pipeline | Technique | Use for SKYGRIND |
|---|---|---|---|---|---|
| 1 | [MirzaBeig/Anime-Speed-Lines](https://github.com/MirzaBeig/Anime-Speed-Lines) | **Unlicense** [lic][api] | Unity 2020.3.35f1, URP 10.9.0 (manifest read); built-in + URP | screen-space polar simplex noise, animated outward, radial mask | optional stylised boost accent (shader math only) |
| 2 | [daniel-ilett/blur-pro-urp](https://github.com/daniel-ilett/blur-pro-urp) | **MIT** [lic][api] | 2022.3 project, URP 14.0.7; code has a Unity 6 `RecordRenderGraph` path; README: "supports Unity versions from 2022.3 until 6.3" | Gaussian radial blur along `uv - 0.5` as a URP renderer feature with a Volume component | the Unity 6 render-graph pass scaffolding for a full-screen speed pass |
| 3 | [Bauxitedev/godot-motion-blur](https://github.com/Bauxitedev/godot-motion-blur) | **MIT** [lic][api] | Godot 3.3 (GLSL) | camera motion blur from the camera's linear + angular velocity, reprojecting each pixel's world position from depth | **the "travel blur"**: linear velocity only, depth-aware, centred on the direction of travel |
| 4 | [MikeFrom/Godot-Particles-Tunnel-Speed-Lines](https://github.com/MikeFrom/Godot-Particles-Tunnel-Speed-Lines) | **CC0-1.0** [lic][api] | Godot 3.4 (a 4.5 release per README) | world-space particles spawned on a ring of radius R far ahead, flying at the camera, quads turned tangent to the tunnel wall | the world-streak spawn layout |
| 5 | [keijiro/KvantStream](https://github.com/keijiro/KvantStream) | MIT text **in README only**; no LICENSE file (checked master / main), API `license: null` | Unity 5.1+, built-in, GPU particles in float textures | each particle is a line from its previous to its current position; alpha 1 at the head, 0 at the tail | the streak shape (length = travel over an exposure time); re-implement, don't vendor |
| 6 | [dip000/my-godotshaders](https://github.com/dip000/my-godotshaders), `WindTrailWithLoops/` | **MIT** [lic][api] (the godotshaders.com page says the shader is CC0) | Godot 4 | Breath-of-the-Wild-style wind trail: a tube trail mesh deformed by sine waves and loops, with alpha fade in / out | ambient wind decoration; the wiggle for streak variety (low priority) |
| 7 | [Oval17/threejs-scenes-vault](https://github.com/Oval17/threejs-scenes-vault), `code/scenes/037_warp_speed.html` | **MIT** [lic][api] | three.js | N line segments in a cylinder (r 0.5-20, z -100..0) moving at the camera at 50-130 u/s, recycled on passing | the simplest warp / tunnel reference |
| 8 | [keijiro/KinoMotion](https://github.com/keijiro/KinoMotion) | **MIT** [lic][api] | Unity 5.x built-in (last push 2017) | reconstruction-filter motion blur (motion vectors, tile max velocity) plus frame blending | reference only: URP's own Motion Blur supersedes it |
| 9 | [Cyanilux/URP_BlitRenderFeature](https://github.com/Cyanilux/URP_BlitRenderFeature) (branch `6.3`) | **MIT** [lic][api] | "Tested with Unity 6.3 LTS (6000.3.6f1)" [read] | a flexible full-screen blit renderer feature (render graph) | alternative pass scaffolding (SKYGRIND is on 6000.3.7f1) |
| 10 | godotshaders.com: [Anime Style Speedlines (w/ or w/o distortion)](https://godotshaders.com/shader/anime-style-speedlines-w-or-w-o-distortion/) by ProfesorShader | CC0 (page statement [read]; not a GitHub repo) | Godot | polar-coordinate noise, planar or radial; `radial_min` / `radial_max` mask; can distort the image instead of drawing | variant of #1 with a distortion mode |
| 11 | [pioneerspacesim/pioneer](https://github.com/pioneerspacesim/pioneer) `src/SpeedLines.cpp`, `data/configs/Speedlines.ini` | **GPL-3.0** (README badge [read]; API `license: null`): **idea only, copy no code** | C++ / OpenGL | a 3D lattice of points (`depth`³ = 9³, `spacing` 750, `bounds` 2000) around the ship, moved by `-velocity × dt` and wrapped toroidally; drawn as lines along the velocity; alpha = 1 - distance / bounds; velocity capped (`max_vel` 100) | the "wrap-around motes" alternative to spawning |

Built into Unity (no import, no licence question):
- **Particle System, Stretched Billboard.** "Camera Scale: Stretches particles according to Camera movement"; "Velocity
  Scale: Stretches particles proportionally to their speed"; "Length Scale: ... along the direction of their velocity"
  ([Unity Manual: Renderer module](https://docs.unity3d.com/Manual/PartSysRendererModule.html) [read]). This is the
  textbook world-streak trick, but SKYGRIND draws its FX without Shuriken (FxLines / FxCubes, instanced quads), so the
  same rule is better written into a small pool (section 3).
- **URP Motion Blur** (Mode: Camera Only / Camera and Objects; Quality; Intensity 0-1; Clamp 0-0.2, "the maximum length
  that the velocity resulting from Camera rotation can have") ([Unity 6 manual](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/Post-Processing-Motion-Blur.html)
  [read]). **It blurs camera rotation too, so mouse look smears the aim.** That is why the travel blur below uses linear
  velocity only.
- **URP Full Screen Pass Renderer Feature.** Assigning a material is enough for a full-screen pass (referenced by
  Cyanilux's README [read]).

### 1.2 Details per candidate (when, where, how strongly the lines show, and the files)

**1. MirzaBeig/Anime-Speed-Lines** (Unlicense [lic], 315 stars, pushed 2025-01-18 [api])
- Files:
  - `Assets/Mirza Beig/Anime Speed Lines/Shaders/Anime Speed Lines.shader` (Amplify-made CG)
  - `.../Scripts/CustomRenderPassFeature.cs`, `.../Scripts/CustomRenderPass.cs` (URP)
  - `.../Scripts/CustomPostProcessing.cs` (built-in)
  - `.../Materials/Anime Speed Lines.mat`
- **How the lines are made** (frag, read):
  - the centred UV in polar form: radius `length(uv - 0.5) × _SpeedLinesRadialScale × 2` and angle
    `atan2 × _SpeedLinesTiling / 2π`
  - scrolled along the radius by `-_SpeedLinesAnimation × _Time.y`, so the lines **flow outward**
  - 2D simplex noise, then `pow(noise, _SpeedLinesPower)`, remapped above `_SpeedLinesRemap`: a higher remap gives
    fewer, thinner lines
- **Where:** a radial mask, `pow(1 - saturate((length(uv×2-1) - _MaskScale) / (lerp(0,_MaskScale,_MaskHardness) - 0.001 - _MaskScale)), _MaskPower)`,
  clears the centre. The defaults are tiling 200, radial scale 0.1, power 1, remap 0.8, animation 3, mask scale 1,
  hardness 0, mask power 5.
- **How strongly:** `_Colour.a` only. There is no speed logic: the user drives the alpha.
- **Port notes:**
  - The render pass uses `RenderTargetHandle` / `cameraColorTarget` (URP 10). It must be rewritten for URP 17's render
    graph (use #2 or #9, or the Full Screen Pass feature).
  - The shader is CG (`UnityCG.cginc`). Port the frag body to HLSL with `Blit.hlsl` (`_BlitTexture`).
  - Recentre it on the focus of expansion (the velocity's screen projection) instead of 0.5. This fixes "a strafe boost
    bursts from the centre".

**2. daniel-ilett/blur-pro-urp** (MIT [lic], pushed 2026-04-22 [api]; the README says the asset was made free on
2026-04-20 and is being discontinued)
- Files:
  - `Assets/Blur Shaders Pro/URP/Scripts/RadialBlur.cs`: a `ScriptableRendererFeature`. Under
    `#if UNITY_6000_0_OR_NEWER` its `RecordRenderGraph` adds a copy pass and a main pass, each with `SetRenderFunc`.
  - `Assets/Blur Shaders Pro/URP/Scripts/Settings/RadialBlurSettings.cs`: a `VolumeComponent, IPostProcessComponent`
    with `strength` (1-500, the kernel size) and `stepSize` (1-20, ÷1000).
  - `Assets/Blur Shaders Pro/URP/Resources/Shaders/RadialBlur.shader`
- **The technique** (read): a Gaussian-weighted sum of `_KernelSize` taps at `uv + (uv - 0.5) × x × _StepSize`. The
  centre is naturally sharp because the offset is ~0 there.
- **When / how strongly:** a Volume value only (no speed logic).
- **Use:** the Unity 6 pass pattern, the VolumeComponent gating (`IsActive`) and the Blit shader boilerplate, if the travel
  blur needs its own C# feature.

**3. Bauxitedev/godot-motion-blur** (MIT [lic], 192 stars [api])
- Files: `motion_blur/moblur_shader.shader` (the effect) and `motion_blur/motion_blur.gd` (feeds the camera's velocities
  each frame).
- **The technique** (read):
  - The pixel's world position comes from the depth texture and the inverse projection and view matrices.
  - `total_velocity = linear_velocity + cross(angular_velocity, pixel_world - cam_pos)`
  - The previous world position is `pos - total_velocity`. It is projected back to NDC, and
    `pixel_diff_ndc = prev - now`.
  - The image is averaged over `iteration_count` taps along `pixel_diff_ndc × (i/n - 0.5) × intensity`.
  - Pixels nearer than `startRadius` are skipped (the gun / near geometry).
  - The script takes the linear velocity from the position delta per frame and the angular velocity from the quaternion
    delta.
- **Why it suits SKYGRIND:**
  - Pass **only the linear velocity** (angular = 0) and every pixel streaks away from the true focus of expansion. A strafe
    streaks sideways and a fall streaks upward.
  - The streak length is proportional to nearness: a wall flashing past streaks hard, the sky doesn't. This is the "smart
    radial blur" the brief asks for.
  - Turning the camera never blurs, so the aim stays crisp.
- Port: GLSL → HLSL. Use `SampleSceneDepth`, `ComputeWorldSpacePosition` and `UNITY_MATRIX_VP` in URP; feed the
  Mover's velocity (`WebSpace.ToUnity(game.mover.velocity)`) rather than a frame delta.

**4. MikeFrom/Godot-Particles-Tunnel-Speed-Lines** (CC0-1.0 [lic])
- File: `godot220222_tunnel_speed_lines/tunnel_speed_lines.tscn`. It holds two inline shaders.
- **The particles shader** (`RESTART` block, read):
  - each particle starts at `x = tunnel_length` on a random angle `a`, at `(sin a, cos a) × tunnel_radius`
  - `VELOCITY.x = -tunnel_length / LIFETIME` (so the lifetime sets the speed)
  - the wall's tangent goes in `CUSTOM.yz`
- **The draw shader:** it builds the quad's basis from that tangent, so each streak lies flat along the tunnel wall.
- **Where:** never on the axis: every particle is at radius R, so far ones project near the centre (small) and near
  ones at the edges (long).
- **When / how strongly:** a fixed emitter (no speed logic).
- **Use:** the ring layout for a boost burst, and the tangent orientation, which keeps streaks reading as "wall of air"
  rather than spokes.

**5. keijiro/KvantStream** (MIT in README; see the table)
- Files: `Assets/Kvant/Stream/Shaders/Line.shader`, `Assets/Kvant/Stream/Shaders/Kernel.shader`,
  `Assets/Kvant/Stream/Stream.cs`.
- **The technique** (read, `Line.shader`): each line's two vertices read the particle's current (`_ParticleTex1`) and
  previous (`_ParticleTex2`) positions, with `p = lerp(prev, now, (1 - sw) × _Tail)`. Alpha is × `sw` (1 at the head, 0
  at the tail).
- **When / how strongly:** an emitter's throttle (no camera logic).
- **Use:** the streak length should be travel over an exposure time, recomputed every frame. That makes it grow with
  speed and collapse to a dot when you stop. This is the core of "dynamic".

**6. dip000/my-godotshaders `WindTrailWithLoops/`** (MIT [lic])
- Files: `wind_trail_mesh.gdshader`, `wind_trail_particles.gdshader`, `demo.tscn`.
- **The technique:** vertex displacement on a unit tube trail mesh:
  - waves `sin(VERTEX.y × wave_frequency + t) × wave_amplitude`
  - a twist
  - a loop `(sin, cos)(VERTEX.y × TAU × loop_count + t) × loop_radius`, blended in and out on a timer
  - alpha fades in and out over `fade_time`
  - instance uniforms for variety
- It is ambient world wind (Breath of the Wild), not speed feedback. Useful later for wind over the arena's edges.

**7. Oval17/threejs-scenes-vault `037_warp_speed.html`** (MIT [lic]): the read lines are 44-76.

**8. keijiro/KinoMotion** (MIT [lic])
- Files: `Assets/Kino/Motion/Script/ReconstructionFilter.cs`, `.../Shader/Reconstruction.shader`,
  `.../Shader/Reconstruction.cginc`, `.../Shader/Prefilter.cginc`, `.../Script/FrameBlendingFilter.cs`.
- Built-in pipeline, Unity 5 era. Not a port target.

**9. Cyanilux/URP_BlitRenderFeature** (MIT [lic])
- File: `Blit.cs` (branch `6.3`).
- The README [read]: sample `_BlitTexture`; use "After Rendering Post Processing", not "After Rendering"; for graphs use
  the Fullscreen Graph type.

**11. Pioneer `SpeedLines.cpp`** (GPL-3.0)
- Read for the idea only.
- `Update` subtracts `vel × time × mult` (capped at `MAX_VEL`) from each lattice point and wraps it into
  `[-BOUNDS, BOUNDS]`. `Render` draws each point as a line along the velocity, with alpha `1 - |p| / BOUNDS`.
- **Copy no code (GPL).** The lattice-and-wrap idea is a common "space dust" technique and can be written from this
  description.

### 1.3 Rejected (no licence, or not usable)

All of these have API `license: null` and no LICENSE file was found:
- [Shakti-crypto/RadialBlur](https://github.com/Shakti-crypto/RadialBlur): a URP radial blur feature.
- [keijiro/URP-MoblurTest](https://github.com/keijiro/URP-MoblurTest): Unity 6 URP motion blur test. No LICENSE or README
  licence found.
- [keijiro/KvantWarp](https://github.com/keijiro/KvantWarp): hyperspace streaks. No LICENSE or README licence found.
- [liuyima/UnitySpeedLines](https://github.com/liuyima/UnitySpeedLines)
- [veyselbalck/WindParticles](https://github.com/veyselbalck/WindParticles)
- [TheFrost/threejs-speedup-effect](https://github.com/TheFrost/threejs-speedup-effect): FOV plus post speed lines.
- [SmurfyHehe/neon-overdrive](https://github.com/SmurfyHehe/neon-overdrive): a PR describes "faint ... streaks racing
  outward in the outer part of the frame, from 150 km/h ramping to full at ~245 km/h" [snippet].
- [analog-alex/racers-go](https://github.com/analog-alex/racers-go): wind lines.
- [guccineonicon/strafer-unity](https://github.com/guccineonicon/strafer-unity): "speed blur" [snippet].
- [cbdileo/warp_speed](https://github.com/cbdileo/warp_speed)
- [Starbimkrieg/himbla](https://github.com/Starbimkrieg/himbla): three.js radial lines by velocity [snippet].

Other rejections:
- [Unity-Technologies/VisualEffectGraph-Samples](https://github.com/Unity-Technologies/VisualEffectGraph-Samples):
  `NOASSERTION` [api]. Rejected by the brief's rule.
- [AlygonXD/Anime-Speed-Lines](https://github.com/AlygonXD/Anime-Speed-Lines): Unlicense, but a copy of #1 with 0 stars.
  Use the original.

---

## 2. How admired movement shooters show speed

| Game | World-space streaks / wind particles | Screen overlay lines / edge FX | FOV | Shake / tilt | Chromatic aberration | Motion blur |
|---|---|---|---|---|---|---|
| **ULTRAKILL** | "Wind state is indicated with white streaking particles that surround and follow the player's direction of turn"; "blue movement particles around the player during the slide" (dash-storage invulnerability) ([wiki: Movement](https://ultrakill.wiki.gg/wiki/Movement) [read]) | No speed-lines setting in Options ([wiki: Options](https://ultrakill.wiki.gg/wiki/Options) [read]). A "dash faint" shows "speed lines" [snippet, not found on the page when read]: **unverified** | setting 45-160, default 105 [read] | Screenshake 0-200 %; Camera Tilt "slightly tilted in the direction in which they are currently moving", toggle [read] | not in Options [read] | not in Options [read] |
| **DOOM Eternal** | not found | White speed lines at the screen edge on a dash: a mod "Remove Dash Edge Screen Effects" exists ([Nexus 920](https://www.nexusmods.com/doometernal/mods/920), page 403 [snippet], **unverified** in detail). DOOM: The Dark Ages: players call the sprint lines "annoying speedlines"; the only fix is UI > "Screen Effects", which also removes the low-health edge ([Steam 1](https://steamcommunity.com/app/3017860/discussions/0/560246502201514654/), [Steam 2](https://steamcommunity.com/app/3017860/discussions/0/591771273907951711/) [read]) | slider (not checked) | not checked | setting (not checked) | "a 2-pass effect that reads from the color texture and a customized velocity buffer ... 4 taps ... vertical ... 4 along the horizontal ... the player gun is excluded to prevent smearing" ([Simon Coenen, DOOM Eternal graphics study](https://simoncoenen.com/blog/programming/graphics/DoomEternalStudy) [read]) |
| **Titanfall 2** | not found | not found | slider 70-110 vertical ([WSGF](https://www.wsgf.org/dr/titanfall-2/en), [hopefulhomies](https://hopefulhomies.com/2016/12/09/field-of-view-in-titanfall-2-a-new-view-on-console-gaming/) [snippet]) | Titanfall (1) tilts the view away from the wall on a wall-run and on the jump toward it ([Gamedeveloper interview](https://www.gamedeveloper.com/design/designer-interview-getting-i-titanfall-i-s-controls-just-right) [read]) | not checked | **Conflicting:** a review praises "not incorporating any kind of motion blur" ([KBMOD](https://kbmod.com/2016/10/review-titanfall-2/) [read]); a Steam thread asks how to turn DoF and motion blur off ([Steam](https://steamcommunity.com/app/1237970/discussions/0/3180106719140587848/) [read], unclear). **Unverified.** |
| **Apex Legends** | not found | not found | "FOV Ability Scaling" widens the FOV during movement abilities, and most guides say turn it off ([prosettings](https://prosettings.net/guides/apex-legends-options/), [setup.gg](https://www.setup.gg/game/apex-legends/best-fov/) [snippet]) | "Sprint View Shake" setting, guides say Minimal [snippet] | not checked | not checked |
| **Mirror's Edge** | not found | radial blur at speed: `TdEngine.ini` `TdMotionBlur=False` "Disable Radial Blur" ([Steam](https://steamcommunity.com/app/17410/discussions/0/2149847423927209933) [read]). "Motion blur appears at the edge of the screen" ([PC Gamer](https://www.pcgamer.com/mirrors-edge-is-still-the-undisputed-queen-of-parkour/) [snippet], **unverified**). Runner Vision tints the path red ([EA](https://www.ea.com/news/runners-vision-in-mirrors-edge-catalyst) [snippet]) | not checked | not checked | not checked | radial, see left |
| **Ghostrunner** | not found | not found | guides suggest 80-100 [read] | not checked | not found | setting: epic / high / medium / off ([GameRevolution](https://www.gamerevolution.com/guides/663792-ghostrunner-motion-sickness-fixes-pc-ps4-xbox-one-ninteno-switch) [read]) |
| **Neon White** | "having the 'you're going fast' particles appear on-screen" after sliding on water ([NME review](https://www.nme.com/reviews/game-reviews/neon-white-review-3248147) [read]) | not checked | FOV setting (search summary [snippet]) | not checked | not checked | not checked. The dev line "The game shouldn't make you feel fast. It should teach you to go fast" is attributed to Angel Matrix by a search summary; the source page (TechRadar) didn't load, so it is **unverified** |
| **Warframe** | not found | "Enable HUD Motion": the HUD moves with the frame's motion (bullet jump, melee) ([wiki: Settings](https://wiki.warframe.com/w/Settings) [read]) | vertical FOV slider [read] | "Enable Screen Shake" (not recoil) [read] | not checked | "camera Motion Blur", toggle [read] |
| *Marvel's Spider-Man* (extra) | not found | not found | "Two parameters are dynamically increased in real-time according to the character's speed: FoV and follow distance" (GDC "Concrete Jungle Gym", Doug Sheahan, via [jbsiraudin](https://jbsiraudin.github.io/blog/spiderman-dolly) [read]) | | | |

**What this says** (inferred from the rows):
- **No admired movement shooter found here sells speed with a persistent full-screen line frame.** Where edge lines exist
  (DOOM Eternal's dash, The Dark Ages' sprint), players ask how to remove them, and The Dark Ages' fix is an all-or-nothing
  "Screen Effects" toggle.
- The working cues are:
  - **FOV tied to speed**: Spider-Man, Apex's ability scaling, Titanfall's high FOV
  - **world particles near the player that follow the motion**: ULTRAKILL's wind-state streaks, Neon White's "going fast"
    particles
  - **view tilt**: Titanfall's wall-run, ULTRAKILL's camera tilt
  - **velocity-buffer motion blur that excludes the gun**: DOOM Eternal
- Every blur, shake and FOV scaling is a setting that competitive players turn off. So the new cue must work at 0 blur.

**Perception research:**
- Pretto, Bresciani, Rainer and Bülthoff (eLife 2012, "Foggy perception slows us down"; summary
  [read](https://www.cyberneum.de/93470/news_publication_6605179_transferred)): speed is overestimated in fog because
  "visibility is poorer in the central than in the peripheral area of the visual field". Drivers slowed from 85 to
  70 km/h in severe fog.
- A search summary of Pretto et al. 2009 says occluding peripheral vision makes people underestimate speed and occluding
  central vision makes them overestimate it [snippet].
- So the speed signal lives in **peripheral optic flow**. Keeping the centre clear is right, and the periphery should
  carry *moving* flow, not a static frame.

**A practitioner's list:** Elliot Couvignou, "Adding the Feeling of Speed" ([elliotdev.gg](https://elliotdev.gg/adding-the-feeling-of-speed/) [read]):
- FOV is "one of the simplest and most effective"
- UI speed lines are "used the most" but cost screen space; his alternative is blur from the screen edges with the centre
  kept clear
- 3D particles "emit toward the player from a fixed distance ahead, oriented along the velocity vector"
- camera shake by speed was kept minimal in his shooter so it wouldn't hurt aim

---

## 3. Recommendation: one design, "AIR" (world-space air streaks + a translation-only travel blur)

All numbers are starting values (inferred) for `Assets/Scripts/Game/UnityTuning.SpeedLines.cs` (Unity-only render
numbers, each with a comment, per CLAUDE.md rule 5). No gameplay number changes and the Mover is untouched.

### 3.1 What goes (behind FeelAB, so a blind A/B can compare)
- The HUD's "Speed lines" run band and "Speed lines boost" layer (`UiFeel.cs`): no longer drawn with the switch on.
- The dash edge tint (`MovementFx.RenderTint`): alpha 0 with the switch on. It is the most literal "border".
- The sprint's edge-band world lines (`MovementFx.SpawnSprintLines`): replaced by AIR, which subsumes them.
- **Kept:**
  - the wall-ride streaks and the slide lines and sparks (surface-specific, already world-space)
  - the gust's air
  - the FOV numbers (+5.95 sprint, +11.8 boost back over 0.36 s, +7 slide, and the web's 8° ramp over 8-24 m/s)
  - the speed share of chromatic aberration

### 3.2 Layer 1: AIR streaks (world space; the main cue)

A dedicated pool of thin, additive, camera-facing tapered quads. Reuse `FxLines`' quad and material path, but each
streak keeps a **world-fixed head point** and recomputes its tail every frame. These are air motes the player flies past,
so the flow, parallax, direction and occlusion all come from the real motion.

| Rule | Value | Why |
|---|---|---|
| Speed signal | `v_eff = sqrt(vx² + vz² + (0.6·vy)²)` from `game.mover.velocity` | horizontal speed counts fully; ordinary jump falls (≈13.5 m/s × 0.6 = 8.1) stay under the gate; slams and big falls show |
| Gate / full | `v0 = 9.0` m/s (walk 8 never shows), `v1 = 24` m/s (`dash.speed`) | thresholds from config speeds, not state flags |
| Intensity | `k = clamp((v_eff - 9) / 15, 0, 1)^0.6`: sprint 10.5 → **0.25**, slide 11.6 → **0.35**, dash-jump chain ~17 → **0.69**, boost 24 → **1.0** | the sprint gets a thin flow, the boost the full flow |
| k smoothing | attack instant; release half-life 0.08 s | no flicker on landings |
| Spawn rate | `120 × k` per s (sprint ≈ 30/s, boost 120/s) | **density scales with speed**; at 0.38 s life: ≈ 11 alive while sprinting, ≈ 45 boosting |
| Pool | 160 streaks, oldest reused; at most 64 drawn | bounded cost |
| Where (spawn) | a screen point S in the ring `ρ ∈ [0.40, 1.10]` around the **crosshair**, angle uniform; depth `d ∈ [3, 14]` m; the head point P is unprojected at (S, d). Reject (redraw, at most 4 tries) if the camera→P segment hits the world (`GameWorld.RayIntersect`) | streaks start in air in view; anything behind a wall is never born |
| Shape (every frame) | head P (world-fixed); tail `P + v_now × T`, with exposure `T = lerp(0.040, 0.065, k)` s, world length clamped to 0.15-2.5 m (sprint ≈ 0.47 m, boost ≈ 1.4 m) | the KvantStream rule: **length = travel over an exposure**, recomputed each frame, so streaks point away from the true focus of expansion (strafe → sideways, fall → upward) and shrink to dots when you stop: the boost's end collapses them physically instead of fading a frame |
| Width | 1.5 px at 1080p at its current distance (≈ 0.0021 × d m at the 74° vertical FOV), tapering toward the tail (FxLines' quad tapers to `UnityTuning.FxLineTaper` 0.5; a thinner 0.25 tip is a new number) | thin at any depth |
| Alpha | `α = α_peak × fadeLife × fadeDist × clear × ads × effectsIntensity`, 0 with REDUCED MOTION. α_peak 0.30 (flow) / 0.55 (burst). fadeLife = `smoothstep(0, 0.15, t) × (1 - smoothstep(0.55, 1, t))` over its life. fadeDist = `smoothstep(1.5, 3, dist) × (1 - smoothstep(11, 15, dist))` | no pops; nothing huge an inch from the eye |
| Life | 0.38 s ± 25 % (Mulberry32) | 0.3-0.5 s reads as flow |
| **Central clear zone** | `clear = smoothstep(0.33, 0.55, ρ_head)`, with ρ the elliptical screen radius about the crosshair (0 centre, 1 at the edge midpoints) | nothing drawn inside a ~33 % radius ellipse, full from 55 %. A smaller clear zone than NIGHT18's 0.6 box (its reviewers preferred lines reaching in, NIGHT18-REPORT Q3); GG decides |
| Colour | `colors.usable` toward white by 0.5 (cyan-pale; flow) and 0.3 (burst), additive, HDR ≤ 1 (under the bloom threshold) | rule 8: cyan is the player's; never red-orange, magenta or the boundary's pale violet-white |
| Boost burst (Mover `Dash` event) | `32 × boostShare` streaks at once (boostShare = the partial boost's 0.6-1, as `BoostFovPartialMin`), ring `ρ ∈ [0.45, 1.05]`, `d ∈ [3, 10]` m, T 0.07 s, α_peak 0.55, life 0.25 s; the rate is ×2 for the boost's 0.25 s | a brief burst on the onset vs a sustained thin flow; nothing state-locked after it |
| Other bursts | `BoostJump` 20 streaks; the gust launch (`GustEvent`, upward) 18 streaks | acceleration events get punctuation |
| Enemies stay readable | at spawn, reject (redraw) if the projected head or tail is within 40 px (× Screen.height / 1080) of a live enemy's screen bounds; streaks are depth-tested (an enemy in front hides them); ADS: `ads = 1 - 0.7 × k_ads` (k_ads = the ADS amount UiAds already reads, 0..1) | lines never cross a target |
| Time base | world time (frozen in a hitstop, as MovementFx); cosmetic Mulberry32 seeded per run with its own salt (never a sim stream; rule 6) | consistent with MovementFx |
| Probe | report `FeelFx.SpeedLines` (the drawn streaks' bounding rect, max α) and `SpeedLinesClear` (the clear ellipse's box) each frame | M8 and the reel keep working |

Alternative for the spawn rule (inferred; idea from Pioneer, written fresh):
- ~200 motes on a lattice in a ±12 m box around the camera, wrapped toroidally as the camera moves.
- Each mote gets a threshold `u_i` (Mulberry32) and draws when `k > u_i` (alpha `smoothstep(u_i, u_i + 0.15, k)`). This
  makes density by speed with no spawn churn.
- Same shape, clear-zone and fade rules.
- Simpler to make perfectly continuous; worse at bursts and enemy rejection.

### 3.3 Layer 2: the travel blur (screen space, subtle, boost-weighted)

Bauxitedev's reprojection with **linear velocity only**, as one full-screen pass on the main camera:
- **Placement:**
  - add it with URP's Full Screen Pass Renderer Feature, or a feature modelled on daniel-ilett's `RadialBlur.cs`
    render-graph path, or Cyanilux's `Blit.cs`
  - on `PC_RPAsset`'s renderer, which already has `m_RequireDepthTexture: 1`
  - after post-processing
- **What stays untouched:** the viewmodel's overlay camera draws after it, so the gun is untouched (DOOM Eternal excludes
  its gun too). The HUD is a canvas, also untouched.
- **Per pixel:**
  - read the world position from depth
  - `prev = pos + v × Tb`, with shutter `Tb = 0.010` s
  - project prev back to the screen: `offset = proj(prev) - proj(pos)`
  - multiply by `mask = smoothstep(0.25, 0.60, ρ)`, about the crosshair
  - clamp to 1.8 % of the screen width (≈ 35 px at 1920)
  - average 6 taps over `offset × (i/5 - 0.5)`
- **Strength:** `s_b = smoothstep(0, 1, (v_eff - 14) / (24 - 14))` × effects intensity × a MOTION BLUR setting (on by
  default; off and 0 with REDUCED MOTION). So the sprint and the slide get **no** blur, the dash-jump chain about 0.2,
  and the boost 1.
- **Sanity check** (inferred): at 24 m/s a wall point 3 m aside and 3 m ahead moves about 2.3° in 0.01 s, ≈ 33 px at
  1080p (inside the clamp). The sky moves ~0 px.
- Cost: one 6-tap full-screen pass. **Not measured.** The gate is the bench p99 at +0.3 ms or less.
- Why not URP's Motion Blur: it includes camera rotation (its Clamp is defined on rotation velocity). Every mouse flick
  would smear the aim.

### 3.4 Optional accent, only if the blind A/B says the boost lacks punch
MirzaBeig's polar-noise lines as a 0.2 s boost-onset overlay:
- centred on the focus of expansion (the velocity's screen projection, clamped to the screen), not on 0.5
- the scroll speed ∝ speed
- `_SpeedLinesRemap` from 0.9 (sparse) to 0.75 (denser) by `boostShare`
- the mask's inner radius at the same 0.33 ellipse
- α ≤ 0.35, with the same gates

It stays off unless the A/B asks for it, because it is exactly the kind of overlay GG called forced.

### 3.5 What to port, file by file

| Piece | Source (licence) | Files | What to take |
|---|---|---|---|
| AIR streak layout (burst ring, tangent-flat quads) | MikeFrom/Godot-Particles-Tunnel-Speed-Lines (**CC0-1.0** [lic]) | `godot220222_tunnel_speed_lines/tunnel_speed_lines.tscn` (the `shader_type particles` RESTART block; the spatial shader's tangent basis) | the ring spawn at radius and the tangent orientation, rewritten in C# for the pool |
| AIR streak shape (head-to-tail by velocity, alpha 1 → 0) | keijiro/KvantStream (MIT text in README only; no LICENSE file) | `Assets/Kvant/Stream/Shaders/Line.shader` (vert: `lerp(p2, p1, (1 - sw) × _Tail)`, `color.a *= sw`) | the rule only (re-implement on FxLines' quad: ~10 lines). If code is vendored, GG accepts a README-only MIT first (D-72 says "read at its LICENSE file") |
| Travel blur math | Bauxitedev/godot-motion-blur (**MIT** [lic]) | `motion_blur/moblur_shader.shader` (the whole fragment), `motion_blur/motion_blur.gd` (velocity feed) | depth → world → offset by velocity → reproject → N taps; angular term dropped; the `startRadius` idea |
| Travel blur pass (Unity 6 render graph) | daniel-ilett/blur-pro-urp (**MIT** [lic]) | `Assets/Blur Shaders Pro/URP/Scripts/RadialBlur.cs` (`#if UNITY_6000_0_OR_NEWER` `RecordRenderGraph`, copy + main pass), `.../Scripts/Settings/RadialBlurSettings.cs` (VolumeComponent / `IsActive`), `.../Resources/Shaders/RadialBlur.shader` (`Blit.hlsl` `Vert`, `_BlitTexture`) | the pass and settings pattern; or Cyanilux `Blit.cs` (**MIT** [lic], tested on 6000.3.6f1); or URP's own Full Screen Pass feature (no import) |
| Optional accent | MirzaBeig/Anime-Speed-Lines (**Unlicense** [lic]) | `Assets/Mirza Beig/Anime Speed Lines/Shaders/Anime Speed Lines.shader` (frag: polar coords, simplex noise, remap, mask) | the frag body, recentred on the focus of expansion |

Any vendored code gets its licence beside it and a THIRD-PARTY.md row (D-72). No packages are added.

### 3.6 How to measure it (per "feel done = measured")

These are metrics for the reel and FeelProbe (inferred targets):
- **S1, centre clear:** streak pixels with ρ < 0.33: **0 frames**. Under 0.30 alpha out to ρ 0.55.
- **S2, density by speed:** alive streaks at a walk (8 m/s): **0**; while sprinting: **11 ± 4**; within 50 ms of a boost
  onset: **≥ 35**; back to the sprint's count within 0.3 s of the boost's end.
- **S3, length by speed** (median screen length at 1080p): sprint **25-45 px**, boost **80-140 px**.
- **S4, direction:** during a pure strafe, the streaks' median screen angle is within 20° of horizontal. During a slam,
  they are within 20° of vertical.
- **S5, clarity:** frames with a streak crossing a live enemy's screen rect: **0**.
- **S6, cost:** bench p99 +0.3 ms at most (24 husks, power plan polled before and after).
- **S7, blind A/B vs NIGHT18** (FeelAB): sprint, slide, boost, dash-jump chain, strafe boost, slam, ADS while sprinting.
- M8 is redefined from NIGHT18's "nothing in the central 60 % box" to S1.

### 3.7 Questions for GG
1. The clear zone: a 33 % radius ellipse (lines reach further in, as the reviewers preferred) or NIGHT18's 60 % box?
2. The travel blur: on by default with a MOTION BLUR setting, or off by default? Competitive players turn blur off in
   Apex, Ghostrunner and DOOM.
3. Retire the dash edge tint completely, or keep a much fainter one?
