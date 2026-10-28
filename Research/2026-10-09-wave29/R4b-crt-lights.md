# R4b: CRT overlay pass in Unity 6 URP, and unrunnable light fixtures

Web-only, 2026-10-09. Web content treated as data. "Source" = fetched or returned by search. "Inference" = my reasoning, not a sourced fact. Not checked: any ms figure at 1440p, any ULTRAKILL internals, any shipped game's fixture policy.

## (a) CRT / pixel overlay

### Unity 6000.3 mechanism (sourced)
- Full Screen Pass Renderer Feature injects a pass with a material at fixed points; no C# needed. Steps: create a Fullscreen Shader Graph, make a Material, add the feature to the Universal Renderer, set Pass Material, set Injection Point, enable Fetch Color Buffer. https://docs.unity.com/en-us/engine/6000.5/manual/post-processing-and-full-screen-effects/urp/custom-post-processing/custom-effect-low-code (page is the 6000.5 copy; the 6000.3 URL I tried returned 404, so verify the 6000.3 wording in-Editor)
- Properties: Injection Point (Before Rendering Transparents / Before Rendering Post Processing / After Rendering Post Processing, default After), Requirements (None, Depth, Normal, Color, Motion, Everything), Fetch Color Buffer (binds the camera color as `_BlitTexture`), Bind Depth-Stencil ("impacts performance"), Pass Material, Pass (advanced). https://docs.unity.com/en-us/engine/6000.6/manual/post-processing-and-full-screen-effects/urp/custom-post-processing/renderer-feature-full-screen-pass (6000.6 copy)
- The shader reads the scene through the URP Sample Buffer node set to Blit Source. Same low-code URL as above.
- Custom Render Graph passes (only if the low-code feature is too limited, e.g. for a fixed-seed grain driven from C#): subclass `ScriptableRenderPass`, declare a PassData, implement `RecordRenderGraph` (declares inputs and outputs only), run commands in a static function set by `SetRenderFunc`, enqueue from a Renderer Feature's `AddRenderPasses`. https://docs.unity.com/en-us/engine/6000.3/manual/render-pipelines/universal-render-pipeline/customizing-urp/render-graph/write-render-pass
- Recommended: use the Full Screen Pass feature (already Render Graph based in Unity 6; inference, the docs I read do not say so in one line). Pass Material from a Fullscreen Shader Graph or a hand-written HLSL using Blit.hlsl `Vert` and `_BlitTexture`. Injection: After Rendering Post Processing, so bloom and tonemap are not warped twice and the HUD (if screen-space overlay uGUI) stays unwarped. Inference: a Screen Space Overlay canvas is not in the camera color buffer at all, so the HUD stays sharp unless it is moved to Camera space.
- Cost per frame at 1440p: no measured figure found. Source says only that on tile-based GPUs ordinary full-screen post stores and reloads tiles through main memory, using bandwidth. https://docs.unity.com/en-us/engine/6000.5/manual/post-processing-and-full-screen-effects/urp/on-tile-post-processing That is a mobile/Switch concern. Inference for a desktop GPU: one full-screen pass is one read plus one write of 2560x1440 = 3.69 Mpx; at RGBA16F about 59 MB of traffic per frame; expect well under 0.5 ms. Not measured. Measure with the project's own `-skygrindBench`, off vs on, median of 3 (lesson L-005).
- Switch note (inference, from the on-tile page's point): the pass is an extra full-frame copy; give it a quality tier that bypasses it.

### Default numbers (sourced references, my picks marked)
Reference defaults from shipped open-source shaders:

| Effect | Reference default | Source |
|---|---|---|
| Barrel curvature | 0.1 (0 flat, 0.5 heavy) | https://app.unpkg.com/three@0.183.1/files/examples/jsm/tsl/display/CRT.js |
| Scanline intensity / count | 0.3 / 240 | same |
| Vignette intensity / smoothness | 0.4 / 0.5 | same |
| Colour bleed amount | 0.002 (0-0.01 recommended) | same |
| Warp amount | 1.0 (0-5) | https://godotshaders.com/shader/vhs-and-crt-monitor-effect/ |
| Scanline opacity / width | 0.4 / 0.25 | same |
| Aberration | 0.03 (-1..1) | same |
| Noise opacity / speed | 0.4 / 5.0 | same |
| Vignette intensity / opacity | 0.4 / 0.5 | same |
| Brightness comp | 1.4 | same |

"Not too intense, a little warped" starting values (my picks, tune on screen; expressed as fractions of the reference defaults):
- Warp: displace UV by 2 % at the corners (inside the requested 1-3 %); equivalent to three.js curvature about 0.04-0.05. Sample outside [0,1] as black or clamp, and zoom the image about 1.02 so no black corners show.
- Scanlines: count = output height / 2 (720 at 1440p) so a line pair is 2 px and does not moire; intensity 0.12 (reference 0.3 is heavy for a movement shooter). Compensate brightness by x1.08 not 1.4.
- Chromatic split: 0.0015 UV at the edge, 0 at centre (scale by distance^2 from centre). About 2 px at 1440p, edge only. Reference bleed is 0.002.
- Grain: opacity 0.04-0.05; seed from a fixed table or `frame % 24` (flicker at 24 Hz), not a free-running clock, so captures and the reel stay deterministic. Do not use `UnityEngine.Random` (project rule 6 is about sim; this is render-only, so a hash of pixel coord + step index is fine).
- Vignette: intensity 0.25, smoothness 0.6 (reference 0.4/0.5 darkens too much on a dusk palette).
- Optional pixel step: quantise screen UV to blocks of 2 px at 1440p (about 720p look), default off; 3-4 px is the "retro" setting. Point sample inside the block.
- Order in shader: pixel step -> warp -> sample with split -> scanlines -> grain -> vignette (inference; matches how the sourced three.js functions compose).

### Accessibility off switch (inference; no source fetched)
- One master toggle plus a strength slider (0 = pass skipped entirely, not just zero uniforms: set `FullScreenPassRendererFeature.SetActive(false)` from a settings component).
- Split sub-toggles: warp, grain flicker, chromatic split, scanlines. Motion-sensitive and photosensitive players need grain/flicker and warp off independently. Default flicker rate 24 Hz is above the 3 Hz flash guideline range but should still be switchable.
- Store the pref key in `UnityTuning.cs` with a comment (project rule 5). Keep UI text legibility: render the HUD after the pass.

## (b) Fixtures the player cannot run through

Honest status: the searches returned almost no direct "how games do it" sources. What I found plus what is engineering inference:

Sourced:
- Level design books treat lights as designed anchors: lone light draws the player to a spot, two lights frame a gap, wash lights along a wall mark a boundary. https://book.leveldesignbook.com/process/lighting/d6-lighting
- Practical fixtures named there are torches and sconces (wall-mounted). Same URL.
- A common trick for first-person light/flashlight clipping is to keep the emitter tiny and close to the camera, while player collision keeps it out of the environment. Search result summary of itch.io forum posts, not opened: https://itch.io/post/8786617 (treat as weak; not verified)
- A generic guidance snippet recommends separate layers for visuals and collision and never inferring collision from decoration; and a source-to-light inventory (emitter ID, light ID, attachment, occlusion). https://tessl.io/registry/skills/github/MengTo/Skills/author-game-levels (an AI-skill registry page, weak source)

Inference (my reasoning for the web collider here, not sourced):
- Principle: a fixture is decoration only if its geometry is not in the `ThreeOctree` / box layers, so nothing needs to be run through. Make the placement rule do the work, so the player cannot be inside it.
  1. Ceiling strips and recessed panels: flush or inside the ceiling plane, thickness 0 to 0.05 m, above the player's head clearance plus jump apex (read from config; never type a height). Not collidable, not in the octree.
  2. Wall trims and sconces: depth <= 0.15 m proud of the wall face and a collision-skin wall behind them; the wall's own collider absorbs the player, the fixture just sits in front of it. Do not add a separate collider.
  3. Floor-standing lamps and pillars: if they must exist, give them a box layer in the same collider so they block; never leave a visible solid object without collision.
  4. Light shafts: view-only quads/cones, additive, no collider, no shadow casting, faded by camera distance so intersecting one is not noticeable. Cone beams from a ceiling slot are the usual look; keep the source inside the ceiling so the player cannot reach the origin.
  5. Rail/ladder rule: nothing within the player's slide, wallrun and mantle volumes (the sim's own `Mover` volumes) except flush wall trims.
- Add an authoring test (EditMode) that fails if any fixture's AABB intersects the playable-space volume computed from the web collider with the player capsule swept along the nav routes. Not checked against the repo.
- Colour rule for the look (project CLAUDE.md rule 8): fixtures in pale violet-white or dusk warm, never red-orange (enemy/danger) or magenta (RIVAL).

## Gaps for the lead
- No 1440p ms figure and no ULTRAKILL shader specifics were found; run the project's own bench.
- Part (b) is mostly design inference; the only real sources are the level design book (wall washes and sconces) and weak forum summaries.
