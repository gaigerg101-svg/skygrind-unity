# NIGHT55 research (b): GitHub methods for five technical items

The GitHub researcher, 2026-10-20 (read-only; this file is its only output). Method: the `skygrind-idea-lab` skill's research
step (our own code first, then outside sources, every claim with its URL, unverified things marked). Searches used generic technical
terms only. Stars are GitHub's API counts read today. Every licence below was read from the repo's LICENSE file (raw URL fetched)
unless the row says otherwise. "Copy: yes" = MIT / BSD / Apache / Unlicense / CC0: the builder may copy code and must add a
THIRD-PARTY.md row (ready rows at the end). "Copy: no (idea only)" = GPL, no licence file, or a proprietary licence: read the idea,
write our own code, copy nothing. No new package is proposed anywhere here.

Slate entries this serves (Logs/n55/slate.json): P0-step and P0-ssao (builder C), P0-wickGrade (B), P1-kill and P2-finish (K).
Their `doneWhen` lines are quoted where a method has to meet them.

---

## 1. Camera step-down / step-up smoothing (P0-step, builder C, row design.n55.step55)

**Ours now (read):** `Game/Move54.cs` `Move54Step` + `Game/SurvivalGame.Move54.cs`. After each 120 Hz sim step, `StepEnded` opens a
window of `gateFrames` (3) rendered frames when both ends are grounded and the feet moved `stepMin` (0.1 m) to `resetJump` (1 m).
Inside the window, the drawn height's change beyond `keepRate` (4 m/s) x frame time goes into an offset; a critically damped spring
(`omega` 40 rad/s, exact solution, frame time) returns it to 0; `maxOffset` 0.4 m. Numbers in `Design/unity-config.json`
`design.n54.move54.numbers.step`. The only test (`Tests/EditMode/Game/N54MoveTests.cs SnapDownIsSpreadAndConverges`) is one 0.3 m
snap at 60 fps with the snap landing whole in one frame. a354 never crossed a step ("not checked"), so the effect is unmeasured.

| repo | URL | stars | licence (LICENSE file) | copy |
|---|---|---|---|---|
| majikayogames/SimpleFPSController (Godot, but the cleanest stair camera I found) | https://github.com/majikayogames/SimpleFPSController | 191 | CC0-1.0 | yes |
| Jan-Ott/CharacterMovementFundamentals (Unity) | https://github.com/Jan-Ott/CharacterMovementFundamentals | 162 | MIT, (c) 2025 Jan Ott | yes |
| nicholas-maltbie/OpenKCC (Unity) | https://github.com/nicholas-maltbie/OpenKCC | 799 | MIT, (c) 2023 Nicholas Maltbie | yes |
| reference only: Source SDK 2013 `SmoothViewOnStairs` | https://github.com/ValveSoftware/source-sdk-2013 (`src/game/shared/baseplayer_shared.cpp`) | not read | file header "Copyright Valve Corporation, All rights reserved"; repo licence not opened | no (idea only) |
| reference only: ioquake3 `CG_StepOffset` | https://github.com/ioquake/ioq3 (`code/cgame/cg_event.c`, `cg_view.c`, `cg_local.h`) | not read | GPL-2.0-or-later (file header) | no (idea only, never GPL) |

**How they do it (read in the files):**
- **SimpleFPSController** (`FPSController/FPSController.gd`): `_save_camera_pos_for_smoothing()` stores the smooth camera's GLOBAL
  position just before a snap up (before the teleport onto the step) or a snap down (only when the down test-move succeeds); every
  physics tick `_slide_camera_smooth_back_to_origin(delta)` puts the camera back at the saved global y, clamps its local y to
  +-0.7 (CROUCH_TRANSLATE, "incase teleported"), then `move_toward(local_y, 0, max(velocity.length() * delta, walk_speed/2 * delta))`
  (walk_speed 7: a 3.5 m/s floor); smoothing stops at exactly 0. The return rate is the player's own speed: the faster you run, the
  shorter the lag in time, the same lag in distance.
- **Source SDK** (`SmoothViewOnStairs`): only while on a ground entity with MOVETYPE_NONE (static ground), the convar on and the view
  offset unchanged (no crouch in progress): `m_flOldPlayerZ += frametime * 150 * dir`, clamped to within `stepSize` 18 units of the
  real z, and `eyeOrigin.z += m_flOldPlayerZ - currentZ`; otherwise the old z resets to the real one. 150 u/s ~ 3.8 m/s and 18 u ~
  0.46 m if 1 unit = 1 inch (the conversion is mine, not the file's). Both directions.
- **ioquake3**: a step-up event adds 4/8/12/16 units to `cg.stepChange` on top of what is left of the last one
  (`oldStep = stepChange * (STEP_TIME - delta) / STEP_TIME`), capped at `MAX_STEP_CHANGE` 32; the view is lowered by
  `stepChange * (STEP_TIME - timeDelta) / STEP_TIME`, `STEP_TIME` 200 ms: a linear decay that accumulates on a staircase. Whether
  step-downs raise the event was not checked (bg_slidemove.c not read).
- **CMF** (`Source/Scripts/Smoothing/SmoothPosition.cs`): the camera is not parented; each Update / LateUpdate it moves toward the
  target by `Lerp(current, target, dt * lerpSpeed)` or `SmoothDamp(..., smoothDampTime)`, with optional extrapolation
  (`target += target - (current - offset)`) to cancel the lag. All three axes.
- **OpenKCC** (`openkcc.cinemachine/Player/SmoothAvatarPos.cs`): `previousPos = Lerp(transform.position, previousPos, 0.05)` in
  FixedUpdate, reset on teleport. All axes, per physics tick (frame-rate dependent).
- The lesson across them: smooth the vertical only (CMF / OpenKCC smooth all axes, which lags a 20 m/s run), detect the snap from the
  movement code (not from the drawn change), accumulate repeated steps under a cap (Q3, Source's clamp), skip non-static ground and
  crouch changes (Source), and tie the return to speed (SimpleFPSController).

**The method for us (view only; Mover, the sim and every ray untouched):**
```
// per SIM STEP (in Move54StepEnded, after the step), view-only state, no allocation:
if snapDetected (both ends grounded, stepMin <= |dy| <= resetJump):          // as today
    off -= dy                       // hold the whole snap at once (Q3: accumulate on top of what is left)
    off = clamp(off, -maxOffset, +maxOffset)
off = springStep(off, vel, omegaEff, 1/120)                                   // the exact critically damped step, in SIM time
omegaEff = max(omega, speedH / returnM)                                       // optional: SimpleFPSController's speed tie
camY_prev = camY_cur; camY_cur = feetY_cur + off                              // a smoothed height per step
// per RENDERED FRAME: draw camY = lerp(camY_prev, camY_cur, alpha)             // interpolated like the feet
```
Why: today's window counts FRAMES (3) and subtracts the DRAWN change, so at 144-240 fps the snap's interpolated spread and the gate
interact per refresh rate (not measured at those rates). Holding the snap in sim time and interpolating the smoothed height by alpha
makes the result the same at 60, 144 and 240 fps by construction, and a freeze (no steps) simply holds the offset. Keep the keepRate
idea only as the stepMin gate (ramps never open it). Every number (omega, returnM, maxOffset) in the row's `numbers`.

**Plug-in:** a new `Move55Step` class (or a second mode inside `Move54Step` behind step55) in `Game/Move55Step.cs`; the two hooks
already exist (`Move54StepEnded` per step, `Move54Drawn` per frame), so one call-site line each in `SurvivalGame.Move54.cs` at most.

**Measure (P0-step's doneWhen: "the camera's largest per-frame vertical drop during the step with the row on <= 50 % of the row
off's; no added lag on flat ground (jitter <= 0.191)"):**
1. An EditMode test first (RED with the row off), driving the class with the render's own math: feet y per 120 Hz step, frames at
   60 / 144 / 240 fps, drawn = prev + (cur - prev) x alpha. Cases: one 0.35 m snap-down (collision.groundSnap) at 8, 14 and 24 m/s;
   a staircase (0.2 m every 0.3 m at 12 m/s); a 0.4 m step-up; a 45 degree ramp at 10 m/s (must hold nothing); a jump on the step's
   next step (nothing held). Print per case: max per-frame camera drop / the feet's, the offset at 50 and 100 ms, the ramp's added lag.
2. The reel (a255): pick the line with `Move54Line`'s scan (a354 already finds a step between flat samples); print the step's
   height and the frame index it was crossed, so "not crossed" can never pass.

**Risks:** a slide's eye-height ease (CameraFeel) lands on top: skip the hold while the eye height is easing (Source's "view offset
unchanged" rule). A respawn / teleport must reset (resetJump, as today).

---

## 2. SSAO noise without TAA (P0-ssao, builder C, row design.n55.ssao55)

**Ours now (read):** `Assets/Settings/PC_Renderer.asset`, the ScreenSpaceAmbientOcclusion feature: `AOMethod: 0` (Blue Noise),
`Downsample: 0`, `AfterOpaque: 0`, `Source: 1` (DepthNormals), `Intensity: 0.4`, `DirectLightingStrength: 0.25`, `Radius: 0.3`,
`Samples: 1` (Medium, 8), `BlurQuality: 0` (High = bilateral), `Falloff: 100`. `Shaders/Stone.shader` samples it
(`_SCREEN_SPACE_OCCLUSION`, `GetScreenSpaceAmbientOcclusion`). No game code touches the feature. No TAA.

**The cause (read in our URP 17 package, Library/PackageCache/com.unity.render-pipelines.universal@a6e30d1f59f5):**
- `Runtime/Passes/ScreenSpaceAmbientOcclusionPass.cs` lines 257-276: with Blue Noise, EVERY FRAME it moves to the next blue-noise
  texture (`m_BlueNoiseTextureIndex + 1`) and offsets it by `Random.value, Random.value`. Unity's reference page says the same:
  Blue Noise "uses a selection of blue noise textures to generate dynamic SSAO"; Interleaved Gradient Noise "generate[s] static
  SSAO" (https://docs.unity3d.com/Manual/urp/ssao-renderer-feature-reference.html). Without TAA nothing averages the per-frame
  pattern, so the AO crawls on a still camera. That is our noise.
- `ShaderLibrary/SSAO.hlsl` `PickSamplePoint`: the IGN path calls `InterleavedGradientNoise(positionSS, sampleIndex)`, the second
  argument being the sample index, not a frame count: the pattern is fixed per pixel, frame to frame.
- Line 106: Blue Noise multiplies the radius by 1.5 internally (`radiusMultiplier`); IGN by 1. Switching keeps the look only with
  Radius 0.3 x 1.5 = 0.45.
- **Trap:** `Assets/Settings/PC_RPAsset.asset` has `m_PrefilterSSAOInterleaved: 1` (the IGN variant is STRIPPED from builds today) and
  `m_PrefilterSSAOBlueNoise: 0`. `Editor/ShaderBuildPreprocessor.cs` lines 820-830 and 1124-1138 recompute these from the ACTIVE SSAO
  features only (an inactive feature is skipped). The shader's `multi_compile_local_fragment _INTERLEAVED_GRADIENT _BLUE_NOISE` has
  no empty option, so if IGN is stripped the Player can only draw Blue Noise, silently (my reading of Unity's variant fallback: not
  tested in a Player).

| repo | URL | stars | licence (LICENSE file) | copy |
|---|---|---|---|---|
| GameTechDev/XeGTAO | https://github.com/GameTechDev/XeGTAO | 900 | MIT, (c) 2016-2021 Intel (README: Intel discontinued it) | yes |
| keijiro/MiniEngineAO | https://github.com/keijiro/MiniEngineAO | 515 | MIT, (c) 2015 Microsoft | yes |
| reference only: bladesero/GTAO_URP | https://github.com/bladesero/GTAO_URP | 53 | none (API: license null) | no (idea only) |

**How they do it:** XeGTAO's README: noise from a 64x64 Hilbert-curve tile driving the R2 sequence; the temporal offset
`288*(frameIndex%64)` is added only when TAA is there ("we indirectly leverage it by enabling temporal noise"); without TAA the
5x5 depth-aware spatial denoiser alone is "a compromise that allows for the effect to still be used by codebases that do not employ
TAA". MiniEngineAO's README: Volumetric Obscurance + multi-scale AO in compute, "smooth results, good temporal characteristics" (a
deterministic multi-resolution AO, no per-frame random). Both say the same thing: without TAA, never animate the noise.

**The method for us:**
1. Switch the method to IGN at Radius 0.45; keep bilateral blur (High), full resolution, 8 samples. Expected: on a still camera the
   AO is identical frame to frame (static noise + deterministic blur). This is a settings change, no shader code.
2. The row's switch (off must stay NIGHT54's Blue Noise, and BOTH variants must survive the build): add a second SSAO feature to
   PC_Renderer (IGN, 0.45, same other fields), BOTH active in the asset, so the build keeps both variants; at load the row calls
   `ScriptableRendererFeature.SetActive(false)` on the one not wanted (row off: the IGN one; on: the Blue Noise one). Do it once, before
   the first camera render, and put it back on quit: in the Editor `SetActive` writes the asset (drift in git, as the TMP fallback
   does). Asset edits by an editor eval (`SerializedObject` on the renderer data), never hand-edited YAML while the Editor is live.
   After the build: `git diff Assets/Settings/PC_RPAsset.asset` must show `m_PrefilterSSAOInterleaved: 0` and `m_PrefilterSSAOBlueNoise: 0`.
3. Only if IGN shows a fixed screen-door pattern in motion: Samples High (12) first; a custom temporal pass (camera-only reprojection
   of a history texture, depth-rejected, as XeGTAO assumes TAA does) is a multi-hour render-graph job: not tonight.
Simpler fallback if the two-feature trick fails: make IGN the asset default (one feature), and let `--off55` lose Blue Noise; that
breaks "--off55 = NIGHT54's game" for the look only, so it needs a DECIDE55 line.

**Measure (doneWhen: "the AO's frame-to-frame changed pixels (|dL| > 2 %) with the row on <= 50 % of off's; mean luminance within
+-0.02 of off's"):** two consecutive captures with the camera and the world still (God, husks frozen, as a254 sets up), the 64x36 grid
or full frame, count |L1 - L2| > 0.02; print off and on, two rooms; also print the mean L of each. Expect off > 0 everywhere AO
reaches, on ~ the non-AO floor (motes, flicker). Frame cost: Unity's page rates the method's cost "Insignificant".

**Risks:** the Mobile_RPAsset (rewritten by every build: memory) has its own prefilter flags: not checked which quality levels use
which asset; check both after the build.

---

## 3. Colour grading by region / per boss, shadows only, hue on chosen pixels (P0-wickGrade, builder B, row design.n55.grade55)

**Ours now (read):** `Game/BossGrade54.cs`: a second global URP Volume built in code (priority `numbers.common.priority`), weight
blended while a boss bar is live, per boss / phase: ColorAdjustments' colour filter (a whole-frame multiply), SplitToning
shadows / highlights / balance, saturation / contrast / EV deltas; fog, ambient and key light tinted as layers. `FeelReel.N54.Grade.cs`
measures a 64x36 grid of the WHOLE frame with HSV hue, circular mean weighted by saturation. D-16: THE WICK p1 H 26 and THE CROWN
p1 H 29 sat within 20 degrees of the danger hue (~12), both shipped ungraded. doneWhen: "the WALL pixels' mean hue >= 30 deg from
the danger hue at p1, the turn and p2 ... the danger-tell pixels keep saturation >= 0.6 and are the most saturated class ... cyan
>= 4.5:1".

**How URP's grade treats shadows (read, `Shaders/PostProcessing/LutBuilderHdr.shader` lines 102-130):** split toning runs in gamma
space: `luma = saturate(luma + balance)`, shadows colour = `lerp(0.5, _SplitShadows, 1 - luma)`, applied by SoftLight. Shadows /
Midtones / Highlights is a per-channel MULTIPLY by luminance band: `shadowsFactor = 1 - smoothstep(shStart, shEnd, luma)`,
`highlightsFactor = smoothstep(hiStart, hiEnd, luma)`, defaults 0 / 0.3 / 0.55 / 1 (`Runtime/Overrides/ShadowsMidtonesHighlights.cs`).
So an SMH shadows tint touches only dark pixels; a bright emissive danger tell (high luma) gets the neutral highlights factor.
The colour filter multiplies everything, which is how a warm filter dragged the whole frame's hue onto the danger hue.

| repo | URL | stars | licence (LICENSE file) | copy |
|---|---|---|---|---|
| AmplifyCreations/AmplifyColor | https://github.com/AmplifyCreations/AmplifyColor | 181 | MIT, (c) 2023 Amplify Creations | yes |
| Philipp-Seifried/Unity-PostProV2-User-LUT-And-Z-Grading | https://github.com/Philipp-Seifried/Unity-PostProV2-User-LUT-And-Z-Grading | 30 | MIT, (c) 2019 Philipp Seifried | yes |
| Oklab (Bjorn Ottosson; code on the page, not a repo) | https://bottosson.github.io/posts/oklab/ | n/a | "public domain ... also available under an MIT licence" (the page) | yes |
| reference only: keijiro/Kino (HDRP) Recolor / Overlay | https://github.com/keijiro/Kino | 2412 | README says Unlicense; no LICENSE file found in the repo or its package folder | no until a LICENSE file is read |

**How they do it:**
- **Amplify Color, the render mask** (`Plugins/AmplifyColorRenderMask.cs`, `EditorResources/Shaders/MaskBlend.shader`): a hidden
  second camera copies the main one, clears to black, and for each chosen layer renders it with a flat colour replacement shader into
  a screen-sized mask texture; the grade pass returns `lerp(color, graded(color), mask.r)`. Its volumes (`AmplifyColorVolumeBase.cs`)
  carry a LUT, `EnterBlendTime` (1 s), `Exposure` and `Priority`: a timed blend on entry (the blend code itself is in files not read).
  `RenderWithShader` does not exist in URP: the URP equivalent is a ScriptableRenderPass that draws the chosen renderers with an
  override material into an R8 target.
- **Z grading** (README): the image is graded "based on each pixel's depth": a foreground grade, a background grade, blended from
  "Background Blend Start" over "Background Blend Range" world units.
- **Oklab**: linear sRGB -> LMS -> cube root -> Lab; chroma = sqrt(a^2 + b^2), hue = atan2(b, a). A perceptual hue: HSV hue on dark,
  near-grey dusk walls is mostly noise and HSV's hue spacing is uneven.

**The method for us:**
1. **Shadows only:** per boss phase, colour filter white (1, 1, 1: no whole-frame multiply), SMH shadows = the cool tint (hue 220-260,
   e.g. a vector like (0.85, 0.92, 1.25): numbers in the row), midtones and highlights neutral (1, 1, 1, 0), `shadowsEnd` set to the
   walls' luminance ceiling measured on the frame (print the wall class's L), `highlightsStart` below the tells' luminance; SplitToning
   shadows the same cool, highlights neutral grey, balance > 0 to confine it. Fog colour (the far walls' largest mass) toward the cool.
   Add a `ShadowsMidtonesHighlights` override to the grade54 profile in code, as BossGrade54 adds its two.
2. **Regions:** keep the code-driven weight (no Volume colliders: they are PhysX, and our grade is already a weight); a room grade is
   `weight = smoothstep(0, blendM, distance from the camera to the room's box)` with our own box math, `blendM` in the row.
3. **Walls only, if the band trick cannot reach 30 degrees:** Amplify's mask in URP: one pass drawing the map stone renderers
   ("Stone" under "Survival map") with an unlit white override into an R8 mask, one full-screen pass lerping graded vs ungraded by it
   (the TravelBlurFeature recipe, blur-pro-urp MIT, is our template). Costs a second draw of the map: not tonight unless 1 fails.
4. **Hue on chosen pixels (the proof):** for each grid sample, a ray from `Camera.ViewportPointToRay` into our C# world
   (`GameWorld.RayIntersect(origin, dir, out RayHit hit)`, `hit.normal`; rule 4: no Physics.Raycast) and the sim's
   `enemies.Raycast(origin, dir, max)`: class = enemy (enemy hit nearer), wall (|n.y| < 0.35), floor (n.y > 0.7), ceiling (n.y < -0.7),
   none (sky / void). The "danger tell" class by colour: Oklab hue within 20 degrees of colors.enemy and chroma over a floor. Per class
   print: count, Oklab mean hue (circular, chroma-weighted, samples under a chroma floor dropped), mean chroma, mean L; then the wall
   hue's distance from danger, the tells' saturation vs every other class, cyan's luminance ratio. Boss bodies with their own
   `Raycast` (Core/Enemies/*) and view-only particles are not in the world ray: drop samples far brighter than their class median.

**Plug-in:** `Game/BossGrade55.cs` (or a grade55 branch read by BossGrade54 at the same layer) and `Game/FeelReel.N55.<Name>.cs`
(a155). The measurement helper is pure and testable (the classifier takes rays + colours).

**Risks:** SMH and split toning both act in the LUT, so a cool shadow can still read grey on low-chroma stone (Stone.shader takes
vertex colour, not `_BaseColor`: NIGHT54's find); the walls' own colour (decor54's wall tones) may need the cool tint first.

---

## 4. Procedural animation for enemies / bosses without Animator

**Ours now (read):** pill-language models drawn by `EnemyViews*.cs` (RenderMesh, MaterialPropertyBlocks); `EnemyViews.Gait48.cs`
picks one of `Mesh43.Legs48`'s fixed poses by the distance the body was drawn to move (diagonal pairs); `HitSpring.cs`
(Source view-punch spring, exact solution) for hit reactions; `Viewmodel.Springs.cs` (Spring-It-On, MIT, already in THIRD-PARTY);
THE LOOM's knees and feet come from the sim (`Loom37View.cs`); THE WYRM's chain follows the sim's trail (`WyrmVisual26.cs`).
No slate entry names this tonight; it serves P2 / beyond and any boss view.

| repo | URL | stars | licence (LICENSE file) | copy |
|---|---|---|---|---|
| Sopiro/Unity-Procedural-Animation | https://github.com/Sopiro/Unity-Procedural-Animation | 408 | MIT, (c) 2021 sopiro | yes |
| ditzel/SimpleIK (FastIK) | https://github.com/ditzel/SimpleIK | 434 | MIT, (c) 2019 D. E. | yes |
| vrm-c/UniVRM (VRM spring bone) | https://github.com/vrm-c/UniVRM | 3410 | MIT, (c) 2020 VRM Consortium | yes |
| reference only: PhilS94/Unity-Procedural-IK-Wall-Walking-Spider | https://github.com/PhilS94/Unity-Procedural-IK-Wall-Walking-Spider | 280 | none (API: license null) | no (idea only) |

**How they do it (read):**
- **Sopiro** (`Assets/Scripts/Leg.cs`, `LegController.cs`): each foot casts down from a body-relative point (7 m max); a step starts
  when the foot is more than `tipMoveDist` 0.55 from that hit and its group may move; the foot travels over `tipAnimationTime` 0.15 s
  along an ease curve toward the hit plus an overshoot `tipPassOver` 0.275 along the step, lifted by a height curve x
  `tipMaxHeight` 0.2, and it speeds up if the target keeps moving (`tipAcceleration`). Legs alternate by index parity; the turn passes
  when the tracked leg lands; a leg lagging more than `maxTipWait` 0.7 forces its group. Body = mean of the feet + up x 1.3, eased
  (Lerp 0.1 / Slerp 0.2 per fixed step: frame-rate dependent as written); up = the sum of the feet's normals.
- **SimpleIK** (`FastIKFabric.cs`): FABRIK, `ChainLength` 2, `Iterations` 10, `Delta` 0.001; bone lengths from the rest pose; a pole
  target bends the middle joints by projecting onto the plane between neighbours; `SnapBackStrength` blends toward the rest pose first;
  runs in LateUpdate.
- **VRM spring bone** (algorithm as UniVRM implements it; pseudocode read in the VRM spec repo, whose own licence is not set; the
  UniVRM source file path was not opened): `inertia = (cur - prev) * (1 - drag)`, `stiff = dt * restDir * stiffness`,
  `ext = dt * gravityDir * gravity`, `next = cur + inertia + stiff + ext`, then the length constraint `next = root + normalize(next -
  root) * len`, then push out of sphere colliders and re-apply the length.

**The method for us (view only, frame time, no sim writes, no PhysX, no new enemy design: GG's rule 7, animate parts that exist):**
1. **Planted feet** for a walker with real legs (a boss with legs; a swarmer if Gait48's poses are replaced): per foot keep
   `planted`, `from`, `to`, `t`; home = body point + facing offsets; find the ground once per STEP START with `GameWorld.RayIntersect`
   down (not per frame); start a step when |foot - home| > stepDist and the other group is planted; foot = lerp(from, to + overshoot,
   ease(t)) + up x height x sin(pi t) over stepS; groups alternate; a lagging foot forces its group. stepDist and stepS scale with the
   drawn speed so a fast body takes longer, quicker steps.
2. **Legs from feet:** two segments = the analytic two-bone (law of cosines, knee toward the body's outward pole) drawn as two capsule
   matrices; FABRIK (SimpleIK) only for chains longer than 2 (tentacles, a tail).
3. **Secondary motion:** a verlet chain (the spring-bone step above) for cables, antennae, flames on a boss; and a body lean from the
   drawn acceleration on our existing exact spring (HitSpring / Spring-It-On's half-life damper), so it is the same at any frame rate.
4. Per-body state in reused arrays (no allocation after first use, as Gait48 does); off = today's poses.

**Risks:** THE LOOM's feet are sim positions (the grapple nodes): animate between them only, never move them. Copying Sopiro's
Lerp-per-fixed-step would make the look frame-rate dependent: use exp(-dt / tau) or the exact springs.

---

## 5. Hit VFX / kill feedback without audio (P1-kill and P2-finish, builder K, rows design.n55.kill55 / finish55)

**Ours now (read):** a lot already exists, so nothing below re-proposes it: `Core/ImpactRules.cs` (freeze tiers, cooldowns, a budget
with `maxFreeze` (Smash's 0.5 s cap), slow-mos incl. the round's last kill), `Game/ImpactFrames.cs`, `Game/PostFx.cs`'s impact
frame (an inverted palette was rejected on NIGHT17: it maps cyan toward red), `Game/Chunks.cs` (kill blocks, pooled ChunkRing,
`ChunkLife` 1.45 s, "nothing stays (D-29)"), `EnemyViews.Hit.cs` (flash, HitSpring kick, HitSparks), `KillFeel42.cs`, `KillPop53.cs`,
`Decals.cs`, `EnemyViews.Scorch52.cs`. Not found in the code: a screen-space displacement burst, a plane split of the dying body, a
per-hit damage mark on the living body. P1-kill wants ">= 8 debris pieces per campaign kill alive >= 1.5 s" and "a hit husk's body
pixels change >= 5 % on the next frame"; P2-finish "a freeze >= 0.8 s real time ... at most 1.5 s".

| repo | URL | stars | licence (LICENSE file) | copy |
|---|---|---|---|---|
| NoelFB/Celeste (Player.cs public) | https://github.com/NoelFB/Celeste | 3999 | MIT, (c) 2018 Noel Berry | yes (the Monocle / Level code behind the calls is not in the repo) |
| mixandjam/Batman-Arkham-Combat | https://github.com/mixandjam/Batman-Arkham-Combat | 382 | MIT, (c) 2021 Mix and Jam (code; art not named) | yes (code) |
| DavidArayan/ezy-slice (and mixandjam/MGR-BladeMode, 272, MIT, which uses it) | https://github.com/DavidArayan/ezy-slice | 2280 | MIT, (c) 2016 David Arayan | yes |
| majikayogames/SimpleFPSController (decal pool) | https://github.com/majikayogames/SimpleFPSController | 191 | CC0-1.0 | yes |

**How they do it (read):**
- **Celeste** (`Source/Player/Player.cs`): on a dash start, `if (Engine.TimeRate > 0.25f) Celeste.Freeze(.05f)` (no freeze while time
  is already slowed), a displacement burst at the player `AddBurst(Center, .4f, 8, 64, .5f, Ease.QuadOut, Ease.QuadOut)` (0.4 s, radius
  8 -> 64 px on a 320x180 game, alpha 0.5; parameter names inferred from the values), `SlashFx.Burst` along the dash, rumble Strong /
  Medium; squash (0.6, 1.4) on a jump; a white hair flash 0.12 s when a dash is spent.
- **Batman-Arkham-Combat** (`Assets/Scripts/CombatScript.cs`): the last enemy's last hit (`AliveEnemyCount() == 1 && health <= 1`)
  gets `Time.timeScale = .5f` for 2 s of REAL time plus a focus camera on the target; the camera impulse amplitude is set by distance
  `Mathf.Max(3, 1 * distance)`; a normal hit gets only a particle at the fist.
- **EzySlice** (README): cuts a convex mesh by a plane, triangulates the cross-section (monotone chain), interpolates UVs / normals
  barycentrically; returns null on failure; it builds new meshes (allocations: not stated, assume yes).
- **SimpleFPSController** (`weapon_manager/bullet_decal_pool.gd`): a static pool of up to 1000 decals; when full, `pop_front()` the
  oldest, push it to the back and re-parent it; aligned to the hit normal; no fade (a decal ends only when recycled).

**The method for us:**
1. **Debris that stays (P1-kill):** after a chunk lands and its speed < epsilon (the floor height is already found once by the world
   ray), move it to a RESTING ring (cap N in UnityTuning, oldest recycled first, the decal-pool rule) drawn by one instanced call per
   colour, no per-frame update; it fades only when recycled or at the round's end. D-29 ("nothing stays") is an old call: the lead needs a
   DECIDE55 line reversing it.
2. **The living body shows damage (P1-kill):** Enemy.shader already has a world-space cell noise and a clip for the dissolve
   (`DissolveNoise`, `DissolveClip`); add a `_Damage` (0..1, from HP lost, per body by MaterialPropertyBlock) that darkens / cracks the
   cells under the threshold with a hot edge band, never discarding: a hit changes the body's pixels on the next frame. Or up to 4
   object-space hit points (`float4 _Hits[4]`, xyz + age) drawn as hot scorch spots fading over ~1 s. Hot colours stay in the
   enemy red-orange family (rule 8).
3. **The finish beat (P2-finish):** a separate finish freeze outside ImpactRules' budget (parries are already exempt; maxFreeze 0.5 s
   would cut 0.8 s), as Batman's 2 s real-time hold for the last hit; the world frozen, the camera live; and Celeste's gate the other
   way round: no other freeze or slow-mo may start inside it.
4. **A displacement ring (new crunch, optional):** at a kill / parry point, a camera-facing ring quad that offsets the scene colour
   radially over 0.3-0.4 s (QuadOut radius, alpha falling), as Celeste's burst. It needs `_CameraOpaqueTexture`, which PerfSwitches
   turns off for the main camera today (Look.cs line 142: "no project shader samples" it); turning it on costs one copy per frame (not
   measured). The alternative is a distortion target drawn by the rings and one full-screen offset pass (the TravelBlurFeature recipe).
   Local, not over the whole screen.
5. **A split kill (optional, rail / sword / ride kill):** instead of EzySlice's new meshes, draw the dying body twice with a plane clip
   (`clip(dot(posWS - p0, n) * side)`, the same place DissolveClip already clips) and push the halves apart along +-n; a Cull Front pass
   shades the hollow inside hot (a fake cap). No allocation, no new mesh.

**Risks:** P1-kill's ladder (">= 0.08 s ... inside the 0.155 s cap") goes through ImpactRules (Skygrind.Core): golden / fingerprints
must stay EQUAL with the row off. Flashes stay under 3 a second. The resting debris adds draw cost in a 24-husk fight: cap it.

---

## THIRD-PARTY.md rows (paste only for the sources code is actually copied from)

| Name | Version | URL | SPDX | Path | Used for | Checked |
| --- | --- | --- | --- | --- | --- | --- |
| SimpleFPSController (majikayogames) | main | https://github.com/majikayogames/SimpleFPSController | CC0-1.0 | <the file it went into> | the stair camera return rule / the decal-pool recycling | 2026-10-20 (N55 GitHub researcher: LICENSE read) |
| Character Movement Fundamentals (Jan Ott) | main | https://github.com/Jan-Ott/CharacterMovementFundamentals | MIT | <file> | camera position smoothing | 2026-10-20 (LICENSE read) |
| OpenKCC (Nicholas Maltbie) | main | https://github.com/nicholas-maltbie/OpenKCC | MIT | <file> | avatar smoothing | 2026-10-20 (LICENSE.txt read) |
| XeGTAO (Intel) | master | https://github.com/GameTechDev/XeGTAO | MIT | <file> | AO denoise | 2026-10-20 (LICENSE read) |
| MiniEngineAO (keijiro, Microsoft) | master | https://github.com/keijiro/MiniEngineAO | MIT | <file> | AO | 2026-10-20 (LICENSE read) |
| Amplify Color (Amplify Creations) | master | https://github.com/AmplifyCreations/AmplifyColor | MIT | <file> | the render-mask grade | 2026-10-20 (LICENSE read) |
| Unity-PostProV2-User-LUT-And-Z-Grading (Philipp Seifried) | master | https://github.com/Philipp-Seifried/Unity-PostProV2-User-LUT-And-Z-Grading | MIT | <file> | depth-blended grade | 2026-10-20 (LICENSE.md read) |
| Oklab (Bjorn Ottosson) | 2020 post | https://bottosson.github.io/posts/oklab/ | public domain (MIT alternative) | <file> | the hue measurement | 2026-10-20 (the page's licence sentence read) |
| Unity-Procedural-Animation (sopiro) | master | https://github.com/Sopiro/Unity-Procedural-Animation | MIT | <file> | planted feet | 2026-10-20 (LICENSE read) |
| SimpleIK / FastIK (D. E.) | master | https://github.com/ditzel/SimpleIK | MIT | <file> | FABRIK chains | 2026-10-20 (LICENSE read) |
| UniVRM (VRM Consortium) | master | https://github.com/vrm-c/UniVRM | MIT | <file> | the spring-bone step | 2026-10-20 (LICENSE.txt read) |
| Celeste (Noel Berry) | master | https://github.com/NoelFB/Celeste | MIT | <file> | the freeze gate / burst numbers | 2026-10-20 (LICENSE read) |
| Batman-Arkham-Combat (Mix and Jam) | main | https://github.com/mixandjam/Batman-Arkham-Combat | MIT | <file> | the last-hit hold | 2026-10-20 (LICENSE read) |
| EzySlice (David Arayan) | master | https://github.com/DavidArayan/ezy-slice | MIT | <file> | plane slicing | 2026-10-20 (LICENSE read) |

Never copy from: ioquake3 (GPL), Source SDK 2013 (Valve's licence), bladesero/GTAO_URP and PhilS94's spider (no licence file),
Unity's URP / Graphics source (read for facts above, not copied), keijiro/Kino until a LICENSE file is found.

## Not checked
- Whether a missing `_INTERLEAVED_GRADIENT` variant falls back to Blue Noise in a Player (reasoned from the multi_compile line, not run).
- Which quality levels use PC_RPAsset vs Mobile_RPAsset.
- Whether ioquake3 raises step events on step-downs (bg_slidemove.c not read); UniVRM's own spring-bone file (path not found;
  the spec's pseudocode was read instead); Amplify Color's volume blend code (AmplifyColorEffect.cs not read).
- The cost of turning the opaque texture on for the main camera; the cost of a second SSAO feature in the asset while one is inactive.
- Nothing here was run in the Editor or a Player (this role may not); every number above is from a file or a page, cited.
