# L1 Lighting and VFX research (wave 33)

Method: about 20 min of web search, 2026-10-11. Web text is data. "Sourced" = read on the cited page. "Proposed" = my starting value, NOT measured in SKYGRIND; verify in the Player. Several game-specific pages (ULTRAKILL, Neon White) gave little technique detail; those rows are thin and marked.

## 1. Rules for lights that read as real

1. Every light has a fixture. A fixture is a box-built prop touching level geometry (sconce, recessed strip, pole lamp, brazier, lava vent). A Light component with no fixture within 0.6 m is a bug (this is GG's "floating beams").
2. Emissive surface and real light come in pairs, and the light is what lights the walls. The emissive box only explains it. Practical = visible fixture; motivated = the logic behind it, with hidden units matching its direction, hardness and colour ([motivated lighting](https://filmdaft.com/what-is-motivated-lighting-definition-examples/)). Sourced: UE practice replaces hundreds of fill lights with emissive surfaces plus bounce, at emissive intensity 5-10 in a bright scene ([80.lv](https://80.lv/articles/lighting-tweaks-for-more-impressive-scenes)). We have no GI, so the real lights must be kept, but few.
3. Light comes from openings and sky: a window slot, ceiling vent or skylight with a pale violet sky colour spills in, and the shaft is a card (section 3).
4. Key / fill / rim per room: one key (the room's dominant fixture, colour = room mood); fill = ambient gradient (never a second key); rim = a coloured edge from an opposite fixture or the boundary glow. Pools of light between pools of dark: darkness is the contrast that makes cyan read.
5. Colour temperature split: cool cyan-white for safe/player-usable (about 6500-8000 K feel), warm orange-red for danger (about 2000-2700 K feel); tungsten practicals sit near 2800-3200 K ([source](https://filmdaft.com/what-is-motivated-lighting-definition-examples/)). Never mix both as keys in one pool. This matches the project palette (cyan player-usable, red-orange danger only).
6. Hardness matches size: small bulb = tight pool; big panel = soft, wide, low intensity.
7. Mirror's Edge: colour is a gameplay guide (movement opportunities go red; white scenery otherwise), and its look came from area lights plus bounce with colour bleeding ([DICE GDC09 slides](https://www.slideshare.net/slideshow/henrikgdc09-compat/3128065)). Lesson for us: keep walls neutral dusk, and let only the usable thing carry hue.

## 2. URP specifics

- Light limits (Unity 6.3 docs, [light limits](https://docs.unity3d.com/Manual/urp/lighting/light-limits-in-urp.html)): Forward = 1 main + 8 additional per object; per camera 256 additional (desktop/console), 32 (mobile), 16 (GLES3). Forward+ has no per-object cap, and ignores the URP asset's Additional Lights and Per Object Limit settings ([Forward+](https://docs.unity3d.com/Manual/urp/rendering/forward-rendering-paths.html)). Big boxes touched by many lights are exactly where Forward silently drops lights, so use Forward+ on desktop, or keep at most 8 lights touching any one wall.
- Probes: Adaptive Probe Volumes need an offline bake in a Baking Set and cannot be modified at runtime, only switched by scenario ([APV concept](https://docs.unity3d.com/Manual/urp/probevolumes-concept.html), [thread](https://discussions.unity.com/t/probe-volumes-for-additive-levels/918696)). If levels are built at runtime from JSON, APV is out: use a gradient/flat ambient per theme (RenderSettings) plus the fixture lights. If scenes are built in the Editor, a bake is possible (not checked against our pipeline).
- SSAO: radius and downsample dominate cost, 4 to 8 samples doubles load, intensity and method are about free ([SSAO ref](https://docs.unity3d.com/Manual/urp/ssao-renderer-feature-reference.html)). It is the cheapest fix for "flat purple walls" (darkens corners and trim junctions). Use Depth Normals, radius about 0.4-0.6 m, intensity about 1.0, 4 samples, downsample on Switch (proposed).
- Bloom: threshold at or above 1.0 in HDR so only emissive blooms; typical intensity 0.4-0.7 ([guide](https://uhiyama-lab.com/en/notes/unity/unity-post-processing-guide/), search summary); threshold 0.9 and soft-knee work if ACES shimmers (search summary, unverified). Emissive HDR colour sets both hue and bloom ([emissive](https://wayline.io/blog/how-to-create-an-emissive-material-in-unity)). Bars blow out because intensity x area x low threshold; cap HDR intensity and use the Bloom Clamp.
- Tonemapping and grading: ACES is the common cinematic choice ([NODE blog](https://unity.com/en/blog/how-to-create-atmospheric-environments-on-a-budget)); it pushes bright saturated colour toward white, so cyan bars wash out. Proposed: Neutral tonemap + a per-theme LUT or Color Adjustments (post-exposure, contrast 10-20, saturation -5 to +10).
- Fog: URP has only RenderSettings linear/exp fog and NO built-in height fog or volumetrics ([thread](https://lr.ggtyler.dev/r/Unity3D/comments/1adx09v/why_doesnt_urp_have_volumetric_fog/kk4u7il/?context=3)). Options: [URPFog](https://github.com/unitycoder/urpfog) (MIT, URP 14, depth/distance/height modes, about 0.6 ms mid-range, Full Screen Pass feature), or our own Full Screen Pass shader graph ([docs](https://docs-alpha.unity3d.com/Manual/urp/renderer-features/renderer-feature-full-screen-pass.html)).
- Fake volumetrics (sourced, [NODE blog](https://unity.com/en/blog/how-to-create-atmospheric-environments-on-a-budget)): unlit additive shapes (boxes, cones, capsules) with midpoint, falloff and HDR multiplier controls, stacked at low intensity. Real shafts via a renderer feature ([Kodeco tutorial](https://www.kodeco.com/22027819-volumetric-light-scattering-as-a-custom-renderer-feature-in-urp)) are screen-space and cost more.
- Decals: URP Decal feature, Screen Space technique recommended on tile-based GPUs; DBuffer is for desktop; Normal Blend quality costs depth samples ([decal ref](https://docs.unity.com/en-us/engine/6000.6/manual/visual-effects/decals/renderer-feature-decal/reference)). Use for scorch, light pools on floors, grime to break flat walls.
- Budgets (proposed, not measured; project benchmark is 24 husks on laptop): laptop max 24 real lights per camera, at most 8 on any surface; Switch max 12 lights, no SSAO or SSAO downsampled, bloom Downscale half, no volumetric pass, fog only RenderSettings. Shadows: main light only, no shadow on fixture lights.

## 3. Cheap stylized VFX

- Dust motes: one Particle System per room, 40-80 tiny soft quads, 0.02-0.04 m, drifting at 0.05-0.15 m/s, additive, cyan-white, alpha low, spawned in light pools only. Cost near zero.
- Embers over lava: 20-40 quads rising 0.5-1.2 m/s with sine sway, lifetime 3-5 s, orange HDR 2-3 fading to dark red, upward 4-6 m only.
- Heat haze: a quad or Full Screen pass sampling the opaque texture with scrolling normal noise, strength about 0.005-0.01 UV, local to lava and only above it. Needs Opaque Texture on (costly on Switch; skip there).
- Sparks: 8-12 stretched billboards, 0.3 s, at broken fixtures, no light.
- Flickering fixtures: perlin(time x 8-14) on the light and on the emissive via MaterialPropertyBlock ([thread](https://discussions.unity.com/t/how-to-randomly-change-the-intensity-of-a-point-light-with-a-script/12987)), +-15 % only for danger fixtures; cyan fixtures steady, with one rare dropout.
- Scrolling emissive panels: Shader Graph UV scroll on a mask texture, HDR intensity 1.0-1.8, scroll speed 0.1-0.3 UV/s; reads as a screen/pipe with motion instead of a plain white bar.
- Screen-space edge light: a Fresnel rim in the fixtures and wall trims (cheap), not screen-border speed lines (GG retired those in w20).

## 4. References (technique notes; thin sources flagged)

- ULTRAKILL: sharp bloom, high contrast, readable threats (summary-level sources only; no devlog found). One source says assets use low-res hand-laid UVs with lighting painted into textures (Pixel8r plugin). Not verified. Take: baked-looking colour in the texture, bloom only on emissives, high value contrast.
- Neon White: limited palette, bright, readable at speed ([Game Developer](https://www.gamedeveloper.com/road-to-igf-2023/-neon-white-and-designing-for-player-creativity)). Take: few hues, each with a job.
- Mirror's Edge: see section 1, item 7.
- Turbo Overkill: HDRP; high-intensity point lights with tuned attenuation plus aggressive bloom to fake neon (secondary source, unverified). Take: short-range lights, glow from bloom, not big radii.
- Severed Steel: low roughness plus emissive voxels along ledges and edges as guides (secondary source). Take: edge light is thin and mounted.
- Hi-Fi Rush: deferred toon pass, static shadow maps ([80.lv](https://80.lv/articles/the-making-of-hi-fi-rush-s-3d-toon-rendering-style)); no numbers published.
- Open source and CC0: [Boat Attack](https://github.com/unity-technologies/boatattack) (URP demo, Unity Companion License, Unity-dependent use only; volumes, mobile-optimised); [Universal Rendering Examples](https://github.com/Unity-Technologies/UniversalRenderingExamples) (renderer-feature samples, unmaintained for 2022+, license not confirmed); [URPFog](https://github.com/unitycoder/urpfog) (MIT); [Keijiro Kino](https://github.com/keijiro/Kino) (MIT, Bloom/Streak/Recolor, Post Processing Stack/HDRP lineage, URP port status not checked); [Kenney Particle Pack](https://opengameart.org/content/particle-pack-80-sprites) (CC0, 80 particle/cookie sprites). Viking Village URP: not verified. Check each license file before copying code.

## 5. LIGHTING SPEC (proposed starting values; all gameplay-unrelated, put in a Unity-side config, not hand-typed)

**Per-room budget** (room = a connected volume of up to about 20 x 20 m): 3-6 fixture lights, at most 1 key, 0 shadow-casting lights, 1 ambient gradient, 1-2 fake shafts, 1 particle system. Per level cap: 24 real lights in view-range (12 on Switch).

**Fixtures** (box-built; light offset 0.3-0.5 m off the surface):

| Fixture | Geometry | Light | Emissive HDR |
|---|---|---|---|
| Wall sconce | 0.25x0.4x0.15 body, bolted to wall (gap under 0.02 m), 0.1 lens | point, range 7, intensity 1.5-2, cyan or amber | 2.5 |
| Recessed strip | groove 0.15 deep, 0.1 wide emissive inset, trim boxes both sides, segment length at most 3 m | none, or a point light every 6 m, range 6 | 1.5-2.2 |
| Pole lamp | 0.12 pole from floor to head, head box 0.4, base plate | point, range 9, intensity 2.5 | 3-4 |
| Ceiling panel | flush within 0.05 m of the ceiling, frame box around it, up to 2x2 m | spot down, angle 90, range 8, intensity 1.2 | at most 1.2 |
| Brazier / danger lamp | cage box on floor/wall, ember core | point, range 6, orange, flicker +-15 % | 3 |
| Lava vent / lava light | the lava surface | point 1.2 m above surface, range 8, one per 10x10 m of lava, intensity 1.0-1.6, (255,90,20) | surface 1.5-2.2 |

**Placement rules:** attach to wall/ceiling/floor geometry (touching, gap at most 0.02 m); never mid-air; spacing at least 4 m between same-type fixtures; fixtures at corners, doorways, ramps and rails (what the player needs to read); a cyan fixture only marks player-usable features; no magenta except RIVAL.

**Lava rules:** orange light must fall on lava-adjacent geometry only (range 8, low intensity), ambient stays dusk violet, not orange; fog colour is not orange; lava HDR at most 2.2 so bloom does not wash the frame; look-down views must still show floor edges.

**Post profile per theme** (Neutral tonemap; bloom threshold 1.0, scatter 0.6, clamp 10; SSAO on; vignette 0.2):

| Theme | Ambient | Fog (exp, density) | Bloom intensity | Post-exposure / saturation | Key colour |
|---|---|---|---|---|---|
| Foundry | dusk violet, dark | 0.012, warm grey-violet | 0.5 | 0 / +5 | amber + cyan accents |
| Ice | cool blue-violet, brighter | 0.018, pale violet | 0.4 | +0.2 / -10 | cyan-white |
| Kiln | deep red-violet | 0.015, dim rust | 0.45 | -0.1 / +5 | orange, kept to lava and braziers |
| Span | open dusk sky gradient | 0.006, light violet | 0.5 | 0 / 0 | cyan strips, sky rim |
| Spire | near-black violet | 0.02, violet | 0.6 | -0.2 / +10 | cyan + pale violet-white boundary |

**Automated checks** (Player capture + editor scan):
1. No Light has a fixture renderer within 0.6 m; no fixture bounds float more than 0.05 m from level geometry (use the C# collider, not PhysX).
2. No emissive renderer wider than 0.15 m and longer than 4 m unless mounted (touching a surface and inset or framed); no emissive panel over 4 m2.
3. Emissive HDR intensity at most 4 for fixtures and at most 2.2 for lava or area surfaces.
4. Frame luminance (Rec.709, final frame) per level: mean 0.10-0.30; at most 3 % of pixels at or above 0.98; the 95th percentile at most 0.85; at least 15 % of pixels below 0.08 (darkness exists).
5. No colour wash: no single 60-degree hue bin holds more than 45 % of saturated (S over 0.4) pixels; in lava rooms, orange at most 35 % of pixels. Cyan and red-orange together must not both exceed 25 % in one frame (palette rule).
6. Real light count per room at most 6; at most 8 touching one object when on Forward; frame time on the laptop bench within the standing budget (not measured here).
7. Take one screenshot per room from the spawn and one looking at the biggest wall; reject if wall luminance standard deviation is under 0.02 (flat wall).
