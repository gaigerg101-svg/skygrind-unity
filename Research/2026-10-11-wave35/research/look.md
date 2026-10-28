# Look research: PS2 / PS1 overlay, lighting, TV-glitch UI (wave 35)

Researched 2026-10-11, web only (about 33 tool calls). Licences were read from each repo page; anything not read says "not checked".
Project facts read locally: `Assets/Settings/PC_Renderer.asset` has `m_RenderingMode: 2` (Forward+), `PC_RPAsset.asset` has
`m_AdditionalLightsRenderingMode: 1` (per pixel) and a 2048 main shadow map; `Packages/manifest.json` pins UIEffect `5.9.7`.

## Answer

The best copyable base is **VNTG** (Unlicense code, CC0 assets, Unity 6+, Render Graph, Volume-driven): it already has
vertex snapping, affine texture warping, full-screen pixelation, colour depth reduction, 9 dither patterns, palette import,
fog, and a CRT pass with scanlines, noise, chromatic aberration, smear and VHS artifacts. **RetroPSX-URP** (MIT, Unity 6000.0+,
URP 17.0.3+, a RenderGraph feature) is the second source and also ships reduced-resolution volumetric fog. For lighting,
keep Forward+ (already on) and add **CristianQiu's Unity-URP-Volumetric-Light** (MIT, Render Graph, Unity 2022.3 to 6000.6)
for shafts, but the bigger win is fewer, stronger lights that sit in visible emissive fixtures, with bloom only on those
fixtures. For the UI, put the HUD where the CRT pass reaches it, then add UIEffect's Rgb Shift / Pixelation with
UIEffectTweener (already installed, MIT), LitMotion Punch / Shake (MIT) for jiggle, TMPEffects (MIT) for per-letter shake,
and KinoGlitch's analog-glitch maths (MIT) for the TV roll. ULTRAKILL exposes the same knobs as player settings
(downscaling, vertex warping, texture warping, colour compression), and SKYGRIND should too.

## 1. PSX / PS2 shaders and post-processing for URP

| Project | Licence | Unity 6 / Render Graph | What it gives us |
|---|---|---|---|
| [Colby-O2/VNTG](https://github.com/Colby-O2/VNTG) ([itch page](https://colby-o.itch.io/vntg-shaders)) | The Unlicense (code), CC0 (assets): copyable | Unity 6+, Render Graph supported; separate packages for 6.2-and-lower and 6.3+ | PSX material (Unlit, Lit, Texel Lit, Vertex Lit; Forward, Forward+, Deferred, Deferred+), vertex snapping, affine warping, pixelation, colour depth, 9 dither patterns, palette import (.ase .gpl .hex .txt .pal), fog, CRT (interlace, pixel decay, scanlines, noise, chromatic aberration, smear, VHS, shadow mask / aperture grille, curvature). All set by Global Volume. 21 stars, 81 commits. |
| [Natteens/retropsx-urp](https://github.com/Natteens/retropsx-urp) | MIT: copyable | Unity 6000.0+, URP 17.0.3+, RenderGraph renderer feature, per-camera RG resources | Raster modes (native, internal height, fixed, resolution scale), integer screen-space vertex snapping, affine interpolation, vertex lighting, point sampling, RGB precision, ordered dithering, distance fog (colour / modulation / stepped), reduced-res volumetric fog for directional, point and spot lights, optional CRT, a public material helper library for custom shaders. Young: 5 stars, 33 commits. |
| [AlexeyNazariev/Retro-Pixel-Dithering-URP](https://github.com/AlexeyNazariev/Retro-Pixel-Dithering-URP) | MIT: copyable | Unity 6 RenderGraph feature (`RecordRenderGraph` + `AddRasterRenderPass` + Blitter) | 4x4 Bayer dither, resolution-locked pixelation, colour quantise. A small clean RG example. 0 stars, 4 commits. |
| [AlexeyNazariev/PS1-Graphics-Kit-URP](https://github.com/AlexeyNazariev/PS1-Graphics-Kit-URP) | MIT: copyable | Unity 2021.3+; Render Graph not mentioned | Clip-space vertex jitter, virtual-height (240p) pixelation, billboards. No dither or colour depth. |
| [Kodrin/URP-PSX](https://github.com/Kodrin/URP-PSX) | MIT: copyable | Built for Unity 2019.3 / URP 7.1.8; no Render Graph | The classic: Shader Graph lit / unlit with vertex snapping, texture pixelation, colour precision, dither, pixelation, screen fog. Subgraphs are a reference; its renderer features predate Render Graph and would need a rewrite. 872 stars. |
| [RubenTipparach/HPSXURP-Shader-Graph-Tools](https://github.com/RubenTipparach/HPSXURP-Shader-Graph-Tools) | not checked | Unity 2019.4 | Reference only. |
| [jmickle66666666/PSX-Dither-Shader](https://github.com/jmickle66666666/PSX-Dither-Shader) | not checked | not checked | Reference only. |
| [Retro Shaders Pro / PSX Shaders Pro (Daniel Ilett)](https://danielilett.gumroad.com/l/retro-shaders-pro) | paid, commercial: reference only | URP | Colour depth limiter, screen or texture-space dither, CRT and VHS, barrel distortion, rolling scanline ([devlog](https://danielilett.itch.io/psx-shaders-pro-for-unity-urp/devlog/881852/version-13-brings-dithering-vhs-artifacts-and-more)). |

**How to build our own pass if we do not take a package:** Unity 6.3's documented low-code path is a Fullscreen Shader Graph
(URP Sample Buffer, Blit Source) plus the built-in Full Screen Pass Renderer Feature, injection point "After Rendering Post
Processing", "Fetch Color Buffer" on ([Unity 6.3 manual](https://docs.unity3d.com/Manual/urp/post-processing/post-processing-custom-effect-low-code.html)).
That gives colour compression, dither, scanlines and chromatic split with no C#. Vertex snapping and affine warping are
per-material (vertex stage), so they need either VNTG / RetroPSX shaders or their helper include in our flat-shaded shaders.
Low internal resolution with point upscale is a separate low-res target (RetroPSX "raster modes", VNTG pixelation).

**ULTRAKILL's look.** Its Graphics menu has a PSX section with Downscaling, Vertex Warping, Texture Warping and Colour
Compression, each in steps (light, medium, ...). The developer told players to turn off everything in that section except
Colour Compression to get "the PC settings", and players report heavy texture warping making floors swim
([Steam thread](https://steamcommunity.com/app/1229490/discussions/0/3004425113435273400),
[Steam thread 2](https://steamcommunity.com/app/1229490/discussions/1/3044983510167265625),
[ludo.guide](https://www.ludo.guide/guide/ultrakill/best-graphics-settings)). Its weapon palettes are 4x5 px PNGs, one
column per variant, top row emissive ([USGC Color Override](https://thunderstore.io/c/ultrakill/p/tetriscat66/USGCColorOverride/)).
No primary source on its shader internals was found: not checked. Takeaway: ship the effects as player-facing steps with a
strong default, and keep colour compression on even in the "clean" preset because it is the part that "really shows".

## 2. Lighting that looks good in a low-poly URP game

URP facts (Unity docs):
- Forward+ has no per-object light limit; the per-camera limit is 256 on desktop, Main Light included; with Forward+, URP
  ignores the asset's Main Light, Additional Lights and per-object limit settings ([Forward+ limits](https://docs-alpha.unity3d.com/Manual/urp/rendering/forward-plus-rendering-path-limitations.html)).
  Plain Forward is 1 main + 8 additional per object, 256 per camera desktop, 32 mobile ([light limits, 6.3](https://docs.unity3d.com/Manual/urp/lighting/light-limits-in-urp.html)).
  We are already on Forward+ with per-pixel additional lights (read above), so the light count is not what looks cheap.
- Adaptive Probe Volumes place probe bricks (4x4x4 probes each) by geometry density; each pixel samples the 8 nearest
  probes; only Mixed or Baked lights are baked into them; quality is above Light Probe Groups
  ([APV concept](https://docs.unity3d.com/Manual/urp/probevolumes-concept.html), [Unity 6 GI blog](https://unity.com/blog/engine-platform/new-ways-of-applying-global-illumination-in-unity-6)).
  Whether our code-built campaign levels can be baked is not checked.
- URP 17 "Alpha Processing" renders post output with alpha, for HUD-style uses ([URP 17 what's new](https://docs-alpha.unity3d.com/Manual/urp/whats-new/urp-whats-new.html)).

Volumetric light / fog (open source):

| Project | Licence | Unity 6 / Render Graph | Notes |
|---|---|---|---|
| [CristianQiu/Unity-URP-Volumetric-Light](https://github.com/CristianQiu/Unity-URP-Volumetric-Light) ([OpenUPM](https://openupm.com/packages/com.cqf.urpvolumetricfog/)) | MIT: copyable | Render Graph in Unity 6+, plus compatibility mode; 2022.3 to 6000.6 | Main, spot and point lights with shadows and cookies, APV support, Forward / Forward+ / Deferred / Deferred+, DX11 / DX12 / Vulkan. Limits: transparents not blended, fog only to a set distance, no WebGL. Forward+ recommended for many lights. Best pick. |
| [sinnwrig/URP-Fog-Volumes](https://github.com/sinnwrig/URP-Fog-Volumes) | MIT: copyable | Tested 2022.3; Render Graph not mentioned | Shaped fog volumes (cube, capsule, sphere, cylinder), half / quarter res, up to 32 lights per volume, temporal reprojection; no baked lighting or probes. |
| RetroPSX-URP (above) | MIT | Render Graph | Reduced-res volumetric fog built in; matches the retro pixel grid. |

Why lights read "cheesy" and "floating", and what to do (craft sources):
- One or two major sources per space, a bright source falling off into dark; contrast is what makes it "pop"
  ([polycount](https://polycount.com/discussion/comment/1834330), [scifi-meshes](https://forums.scifi-meshes.com/discussion/comment/595789/)).
- Keep emissive surfaces few: many coloured emissives make the eye dart; treat ceiling emissives as the key light and
  play down floor ones ([scifi-meshes](https://forums.scifi-meshes.com/discussion/comment/597701)).
- Low poly as a style means strong stylised lighting, visible facets and a strict palette ([bugnet style guide](https://bugnet.io/blog/low-poly-3d-a-style-guide-for-small-teams)).
- So: every light gets a visible fixture (emissive mesh at the light's position, HDR intensity so only fixtures cross the
  bloom threshold); one warm key (dusk orange sun) vs cool fill (violet ambient), cyan only on player items, red-orange
  only on enemy/danger lights (CLAUDE.md rule 8); shadows on the 1-2 key lights, none on fill; falloff ranges short so
  pools of light separate; volumetric shafts only from the key light and a few fixtures.
- "Weird little triangle displays": cause not checked. Candidates to test in a Player: a point light close to large
  flat-shaded triangles (each facet gets a different shade, a mosaic), shadow acne or low resolution on additional-light
  shadows, or probe interpolation on dynamic objects. Under a PSX pass with dither and low resolution, facet mosaics read
  as intended texture rather than noise.

## 3. UI juice: jitter, wobble, chromatic split, scanline, glitch

- **UIEffect 5 (in the project, [mob-sakai/UIEffect](https://github.com/mob-sakai/UIEffect), MIT):** Tone (Grayscale, Sepia,
  Nega, Retro, Posterize), Colour (Multiply, Additive, Subtractive, Replace, Multiply Luminance, Multiply Additive, Hsv
  Modifier, Contrast), Sampling (Blur Fast / Medium / Detail, **Pixelation**, **Rgb Shift**, Edge Luminescence, Edge Alpha),
  Transitions (Fade, Cutoff, Dissolve, Shiny, Mask, Melt, Burn, Blaze, Pattern), Shadow / Outline (Shadow, Shadow3, Outline,
  Outline8, Mirror), Gradation, Edge (Plain, Shiny), Detail filter, **UIEffectTweener** (curves; Once, Loop, PingPongOnce,
  PingPongLoop; unscaled time), UIEffectReplica; TextMeshPro and URP supported. No jitter, wobble, glitch or scanline
  is documented: it does chromatic split and crunch, not motion.
- **Motion:** [LitMotion](https://github.com/annulusgames/LitMotion) (MIT, Unity 2021.3+): zero-allocation struct tweens,
  `LMotion.Punch` and Shake, TMP per-character colour and position binding. [Coily](https://github.com/zenanon/Coily) (MIT):
  damped springs (a C# port of Ryan Juckett's springs) with scale / position / rotation / colour reactors; good for HUD
  elements that overshoot and settle.
- **Per-letter text:** [TMPEffects](https://github.com/Luca3317/TMPEffects) (MIT, Unity 2021.3+; Unity 6 not stated): tag-based
  basic and show / hide animations, Timeline support. [Easy Text Effects](https://github.com/LeiQiaoZhi/Easy-Text-Effects-for-Unity)
  (MIT): per-vertex ScriptableObject effects. [MeshEffectForTextMeshPro](https://awesome.ecosyste.ms/projects/github.com%2Fmob-sakai%2Fmesheffectfortextmeshpro): licence not checked.
- **TV glitch:** [keijiro/KinoGlitch](https://github.com/keijiro/KinoGlitch) (MIT): analog glitch (scan line jitter, vertical
  jump, horizontal shake, colour drift) and digital block damage; pipeline not stated, so port the maths.
  [saimarei/URPGlitch](https://github.com/saimarei/URPGlitch) (MIT, tested 6000.0.0f1 / 6000.0.23f1, Volume overrides) is a URP
  port, now unmaintained; a search snippet says it needs Render Graph off (not verified), so treat it as a reference.
  [Tsukina-7mochi/glitch-shader](https://github.com/Tsukina-7mochi/glitch-shader): per-object glitch, scanlines and
  chromatic split without post; licence not checked.
- **Getting the CRT onto the UI:** a Screen Space Overlay canvas is not touched by camera post (not checked in this
  project). Either render the HUD canvas in Screen Space Camera so a fullscreen pass after post includes it, or draw the HUD
  into a RenderTexture shown through a RawImage with a CRT / glitch material. The second keeps world and HUD settings apart.

## A recommended stack for our game, in priority order

1. **VNTG as the PSX base** (Unlicense / CC0; Unity 6.3 package; Render Graph; [repo](https://github.com/Colby-O2/VNTG)).
   Turn on low internal resolution with point upscale, colour compression with ordered dither, and fog first; these are
   full-screen and touch no gameplay code. Wrap its Volume values in a SKYGRIND "PSX" settings block (Downscale, Vertex
   warp, Texture warp, Colour compression, CRT) in the ULTRAKILL shape ([Steam](https://steamcommunity.com/app/1229490/discussions/0/3004425113435273400)),
   with every default in `UnityTuning.cs` (CLAUDE.md rule 5).
2. **Per-material vertex snap + affine warp** from VNTG's material or RetroPSX-URP's helper library (MIT,
   [repo](https://github.com/Natteens/retropsx-urp)) added to our flat-shaded shaders. Render-only, so movement exactness is
   untouched; keep the gun viewmodel's snap weaker (ULTRAKILL players note it looks off on weapons, [ludo.guide](https://www.ludo.guide/guide/ultrakill/best-graphics-settings)).
3. **Relight with fewer, stronger, fixtured lights:** one warm key plus cool ambient per room, each light inside an
   emissive fixture, bloom threshold above 1 so only fixtures glow, shadows on key lights only
   ([polycount](https://polycount.com/discussion/comment/1834330), [scifi-meshes](https://forums.scifi-meshes.com/discussion/comment/597701)).
   Keep Forward+ ([limits](https://docs-alpha.unity3d.com/Manual/urp/rendering/forward-plus-rendering-path-limitations.html)).
4. **Volumetric shafts from the key light** with CristianQiu's package (MIT, Render Graph, [repo](https://github.com/CristianQiu/Unity-URP-Volumetric-Light)),
   or RetroPSX's reduced-res fog if it reads better through the pixel grid. Check the cost in the Player bench first.
5. **CRT over world and HUD:** VNTG's CRT pass (scanlines, chromatic aberration, noise, VHS), with the HUD moved to Screen
   Space Camera or a RenderTexture so it gets the same treatment ([Unity fullscreen pass](https://docs.unity3d.com/Manual/urp/post-processing/post-processing-custom-effect-low-code.html)).
6. **UI juice:** UIEffect Rgb Shift + Pixelation pulses via UIEffectTweener on hits and damage ([UIEffect](https://github.com/mob-sakai/UIEffect)),
   LitMotion Punch / Shake or Coily springs for jiggle and bounce ([LitMotion](https://github.com/annulusgames/LitMotion),
   [Coily](https://github.com/zenanon/Coily)), TMPEffects for per-letter shake on big numbers ([TMPEffects](https://github.com/Luca3317/TMPEffects)).
   Drive any UI randomness from a render-side seed, not the sim's `Mulberry32` (rule 6 covers sim code only).
7. **TV-roll glitch on events** (round start, damage, boss phase): port KinoGlitch's analog glitch to a Render Graph
   fullscreen pass, MIT ([KinoGlitch](https://github.com/keijiro/KinoGlitch)), keyed to the web's event names.
8. **Later, if levels are static enough:** bake indirect light with Adaptive Probe Volumes ([APV](https://docs.unity3d.com/Manual/urp/probevolumes-concept.html)).
   Not checked whether our code-built levels allow baking.

Reference only (not copyable): Daniel Ilett's Retro Shaders Pro / PSX Shaders Pro, VHS Pro, the Fab "retro shader" pack.
