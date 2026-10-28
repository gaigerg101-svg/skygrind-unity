# R3: open-source resources for SKYGRIND (CRT/retro post, environment props, level and movement references)

Date: 2026-10-09. Method: web search and fetch, plus `gh repo view` / `gh api` for stars, last push and licence only. Nothing cloned or downloaded. Repo and page text treated as data. Stars and push dates are as of today (GitHub API). Where a fact was not read it says "not checked".

Licence rules applied: no licence = ignored; copyleft = "study only".

## 1. CRT / retro-TV post-processing (ranked)

| # | Candidate | Stars | Last push | Licence | What we could take | Cost | Verdict |
|---|---|---|---|---|---|---|---|
| 1 | https://github.com/Cyanilux/URP_RetroCRTShader | 261 | 2025-09-27 | MIT | Shader Graph (Fullscreen Graph type): CRT warp, scanlines, image distortion, static, scrolling glitch static, RGB phosphor stripes; keywords toggle each effect. README: "Updated for Unity 2022+, now uses Fullscreen Graph and FullscreenPassRendererFeature". Includes a low-res RT second-camera pixelation example. | Low. One material plus the Unity-supplied Full Screen Pass feature. Must re-check on 6000.3 (not tested by us). Use `shader_feature` to `multi_compile` if toggling at runtime. Keep the effect subtle so UI text stays readable (16 px rule). | Best fit. Take the graph and port the CRT chunk to HLSL if we want it in code. |
| 2 | https://github.com/Cyanilux/URP_BlitRenderFeature | 697 | 2026-07-17 | MIT | Custom Blit Renderer Feature, README says tested on Unity 6.3 LTS (6000.3.6f1), Render Graph based, can declare global textures. Good for several chained CRT passes where the stock Full Screen Pass is too rigid. | Low. Install by git URL or copy the folder. Always samples cameraColor. "After Rendering" event is flagged as not working; use After Rendering Post Processing. | Take as the pass host if the stock feature is not enough. Also a readable Render Graph example. |
| 3 | https://github.com/Kodrin/URP-PSX | 921 | 2024-05-27 | MIT (README also says "use for whatever you want") | Screen-space pixelation, colour-precision, dithering and fog as renderer features; vertex-snap and affine-ish PSX material subgraphs. | Medium. README targets Unity 2019.3 / URP 7.1; its features are the old (pre-Render Graph) path and will need rewriting. The screen-space maths (dither matrix, colour quantise) is the useful part. | Mine the dither and quantise maths; do not drop it in. Fits "dither/pixelation" ask. |
| 4 | https://github.com/ModestoGames/Fullscreen-CRT-Shader-URP | 1 | 2024-10-21 | CC0-1.0 | Small Shader Graph CRT (Unity 2022.3.34, URP) inspired by Acerola's tutorial. | Low, but tiny project and the author calls it a study. | CC0 means zero legal risk. Use as a second reference next to #1. |
| 5 | https://github.com/knowercoder/UsefulShaders | 24 | 2025-12-31 | MIT | URP Shader Graph collection (search result said it includes CRT-style full screen shaders; the exact list was not checked). | Unknown. | Check its contents before relying on it. |
| 6 | https://github.com/RubenTipparach/HPSXURP-Shader-Graph-Tools | 10 | 2021-10-21 | MIT | PSX tools: dithering with custom scaling, resolution scaling, several dither matrices. | Old (2019.4). | Reference only for dither matrices. |
| 7 | https://github.com/keijiro/KinoTube | 186 | 2017-10-08 | MIT | Analog video artifacts (built-in pipeline era). | High, since it is a Built-in image effect. | Study the maths only. Same for https://github.com/keijiro/KinoGlitch (2813 stars, 2023-12-05; licence file not present in the repo root and the README text was not fully read: treat as "not confirmed"). |
| - | https://github.com/keijiro/Kino | 2412 | 2020-11-23 | Unlicense (README says so; GitHub shows none) | HDRP-only post effects (README mentions HDRP 7.1 settings). | Not URP. | Skip for code; ideas only. |
| - | https://github.com/unitycoder/Unity_CRT_Shader | 1 | 2026-03-20 | "other" (not read) | Unknown. | Unknown. | Not checked, ignore until read. |
| - | https://github.com/SzymZem/Unity_Shader_CRT | 1 | 2023-02-10 | none | CRT for URP. | No licence. | Ignore (no licence). |

Non-GitHub / paid (for awareness, not for copying):
- DitherDown URP (itch.io, rebuilt in Shader Graph for Unity 6 using Full Screen Pass; price and licence not checked): https://rtwarner.itch.io/ditherdown-urp
- Game Boy pixelization shader (itch.io, free, Simple Full Screen Pass; licence not checked): https://pipmakesgames.itch.io/custom-pixelization-shader
- CRT Postprocess for URP and legacy (Asset Store, paid): https://assetstore-fallback.unity.com/packages/vfx/shaders/fullscreen-camera-effects/crt-postprocess-for-urp-and-legacy-pipelines-310692
- Snapshot Shaders Pro (paid): https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/snapshot-shaders-pro-160556
- Shadertoy CRT shaders: the site default is CC BY-NC-SA 3.0 unless the author states otherwise. NonCommercial makes them unusable for a Steam game. Study only; reimplement the maths from scratch (barrel warp, scanline sine, shadow mask are textbook formulas) and do not paste the code. Per-shader licence was not checked.

### The Unity-side technique for a full-screen pass on Unity 6
- Low code: the Full Screen Pass Renderer Feature. Material from a "ShaderGraph Fullscreen" or "SRP Blit Shader" template; injection points Before Rendering Transparents, Before Rendering Post Processing, After Rendering Post Processing (default); options to fetch the colour buffer and bind depth-stencil. https://docs.unity3d.com/6000.3/Documentation/Manual/urp/renderer-features/renderer-feature-full-screen-pass.html (fetched, title confirmed "Full Screen Pass Renderer Feature reference for URP").
- Code: custom `ScriptableRendererFeature` + `ScriptableRenderPass` using `RecordRenderGraph`, `AddRasterRenderPass`, frame data and the `Blitter` API. Section index (page titles from the 6000.0 render graph overview): https://docs.unity3d.com/6000.0/Documentation/Manual/urp/render-graph.html with child pages `render-graph-introduction.html`, `render-graph-write-render-pass.html`, `render-graph-blit.html`, `render-graph-frame-data.html`, `working-with-textures.html`. The child page URLs were taken from that index and not opened one by one (one guessed URL, `render-graph-write-a-render-pass.html`, returned 404; use the names above).
- The page https://docs.unity3d.com/6000.0/Documentation/Manual/urp/renderer-features/how-to-fullscreen-blit.html is the Compatibility Mode (non Render Graph) example. Do not copy it; Unity says it no longer develops that path. It does state that `CommandBuffer.Blit` should be avoided and `Blitter` used.
- Unity's own Render Graph samples: https://github.com/Unity-Technologies/Graphics/tree/master/Packages/com.unity.render-pipelines.universal/Samples~/URPRenderGraphSamples (path taken from the Cyanilux README; repo https://github.com/Unity-Technologies/Graphics has 3008 stars, pushed 2026-09-29; licence is Unity's package licence, not checked here).
- Cyanilux's walkthrough (linked in its README): https://www.cyanilux.com/tutorials/custom-renderer-features/?tab=Unity6 (not opened).
- Shader requirement for hand-written HLSL: sample `_BlitTexture` using the Blit vertex shader (Cyanilux README; FAQ https://www.cyanilux.com/faq/#fullscreen-shader).
- Project note (CLAUDE.md): UI text must stay at least 16 px at 1080p, so the CRT pass should run before the UI canvas, or only on the 3D camera, not over the UI. Barrel warp changes the playfield edges; test with the pale violet-white boundary visible. Intensities are Unity-only numbers, so they go in `UnityTuning.cs` with a comment.

## 2. Environment props (ranked)

None of these touch enemy models. All environment only.

| # | Pack | Licence | Formats | Style | Contents relevant to us | Link | Verdict |
|---|---|---|---|---|---|---|---|
| 1 | Quaternius Modular Sci-Fi MegaKit | CC0 | FBX, OBJ, glTF (Blend too) | Low-poly | 277 models (grid-fit): walls, floors, doors, columns, props, lighting elements, also aliens (skip). Free "Standard" tier on itch is 190 assets; Pro $9.99+ and Source $14.99+ add the rest and Unity URP shaders (blinking lights, colour masks). | https://quaternius.com/packs/modularscifimegakit.html and https://quaternius.itch.io/modular-sci-fi-megakit | Best match for walls, door frames, pipes and light fixtures. Free tier is enough to start. Check the free tier lists door frames before relying on it (not itemised). |
| 2 | Quaternius Ultimate Space Kit | CC0 | Blend, FBX, OBJ, glTF | Low-poly | 90+ models, about 186k tris total; space themed, includes characters and mechs (skip). | https://quaternius.com/packs/ultimatespacekit.html | Secondary: props only. |
| 3 | Kenney Factory Kit (v3.0) | CC0 | Not stated on the page (Kenney packs normally ship FBX, OBJ, GLB: unconfirmed here) | Blocky low-poly, colour variants | About 140 models: conveyors, belts, warehouse and factory pieces; animations. | https://kenney.nl/assets/factory-kit | Strong fit for the industrial arena and for blocky look matching the web port. |
| 4 | Kenney Modular Space Kit (1.0) | CC0 | Not stated on the page | Blocky | 40 assets: space tiles, modular station pieces. | https://kenney.nl/assets/modular-space-kit | Good for corridor trim and floor tiles. |
| 5 | Kenney City Kit (Industrial) (2.0) | CC0 | Not stated on the page | Blocky | 40 models: factories, warehouses, solar and wind. | https://kenney.nl/assets/city-kit-industrial | Skyline and backdrop pieces. |
| 6 | Kenney Space Kit | CC0 | Not stated | Blocky | About 150 models; planets, ships, space props. | https://kenney.nl/assets/space-kit | Low priority. |
| 7 | Poly Pizza (Quaternius "Cyberpunk Signs" and several fluorescent fixtures) | CC0 for the rows listed (Quaternius, Evol-Love, Nick Slough, Colin McKibben); Poly Pizza also hosts CC-BY models, check each | glTF/GLB (per site, not confirmed on the page) | Low-poly | Neon-style signs; fluorescent and tube light fixtures. Search page: https://poly.pizza/search/neon (individual model paths: /m/rsZJjigt1X cyberpunk signs, /m/aEsJgELAO9e tube light, /m/J8dFHQHQJZ fluorescent light, /m/b434Bd1ozKE fluorescent fixture) | Quick neon/light fixtures. Verify licence on each model page before import. |
| 8 | Dueddel Corridor Level Kit | Author says do anything, "treat as CC0"; no formal licence file | FBX, GLB | Low-poly | 5 x 5 x 4 m corridor tiles, palette texture; no doors or props. 13 MB. https://dueddel.itch.io/corridor-level-kit-kitjam-2021 | Informal licence: ask or avoid shipping. Fine for blockout reference. |
| 9 | Poly Haven models (industrial) | CC0 | glTF/FBX/Blend (site standard; not confirmed on the page) | Realistic, HIGH poly | API list: modular industrial pipes (12k tris), modular pipes (47k), air ducts (14k and 45k), utility boxes (4k, 6k), security cameras (12k-13k), generator (26k), overhead crane (90k). Listing via https://api.polyhaven.com/assets?type=models&category=industrial-infrastructure | https://polyhaven.com/models | Wrong style (photoscan realism) and heavy for blocky dusk look. Use for reference or retopo only. |
| - | Delthor Games Free Sci-Fi corridors | No stated licence ("free to use"; a commenter asked about credit) | FBX | Low-poly, 5 corridor pieces, no textures | https://delthor-games.itch.io/sci-fi-pack | Ignore (no clear licence). |
| - | SilkCyber neon sci-fi pack ($3.99), RedVampire corridor pack ($4.50) | Paid, licence not checked | FBX / GLB | Low-poly neon | https://silkcyber.itch.io/modular-sci-fi-environment-pack, https://redvampire.itch.io/corridor-asset-pack | Paid, not CC0. |
| - | OpenGameArt lowpoly modular sci-fi environments, Sci-Fi Essentials Kit | CC0 (Quaternius mirror per search result; not opened) | multiple | Low-poly | https://opengameart.org/content/lowpoly-modular-sci-fi-environments, https://opengameart.org/content/sci-fi-essentials-kit | Mirrors; use the Quaternius originals. |

Notes: Kenney and Quaternius assets use a flat/palette-colour look, which can be re-tinted to the dusk palette (cyan for player-usable, red-orange danger only, violet-white boundary). Emissive neon strips are better built by us as emissive materials on simple boxes than imported.

## 3. Level-authoring and movement references (ranked)

### 3a. Level format tooling

| # | Candidate | Stars | Last push | Licence | What it shows | Cost | Verdict |
|---|---|---|---|---|---|---|---|
| 1 | https://github.com/radiatoryang/scopa (Unity) | 157 | 2026-01-08 | MIT | Imports Quake/HL .MAP (also RMF/VMF/JMF) and WAD; generates a model prefab with one entity parent per entity, mesh children per material, collider child per brush. Point entities map to prefabs by classname through entity overrides, an FGD asset, or global templates (first matching override wins). `func_group` / `func_detail` merge into worldspawn; classnames containing "trigger" or "water" become non-solid triggers; "illusionary" skips colliders. Key-values pass through as standard Quake key-values. Exports FGD for TrenchBroom. 32 map units = 1 m. README warns "unstable early development", Unity 2022.3+. | Medium-high. Its colliders are Unity physics (box/convex/mesh); we must not use PhysX for gameplay (CLAUDE.md rule 4), so we would take only the parser and entity pipeline and feed brushes into `TriangleWorld` / box layers. Brush-only features, no game code. | Best study target for a `.map` workflow. Treat the parser as a dependency candidate, not a drop-in. |
| 2 | https://github.com/func-godot/func_godot_plugin | 875 | 2026-09-23 | MIT | Same idea for Godot: entity definitions per classname (resources), point entities from node class or packed scene, brush entities with visual and collision settings, FGD export, GLB display models for point entities in the editor. Docs repo: https://github.com/func-godot/func_godot_docs. | Study only (Godot). | Best model for "place lights and props by entity classname with key-values". |
| 3 | https://github.com/QodotPlugin/Qodot (older Godot plugin; also https://github.com/codecat/godot-tbloader, 260 stars, MIT, 2026-05-15) | 758 | 2025-11-27 | MIT | Predecessor of func_godot; libmap parser (https://github.com/QodotPlugin/libmap, 27 stars). | Study only. | Lower priority than func_godot. |
| 4 | https://github.com/TrenchBroom/TrenchBroom | 2862 | 2026-10-09 | GPL-3.0 | The editor. Using the editor to produce our own map files is fine; copying its code is not. | Zero for tool use. | Study only for code; fine to use as an authoring tool (the map files are ours). Not verified: how GPL applies to output maps, treat as normal for editors. |
| 5 | https://github.com/jonathanlinat/quake-leveldesign-starterkit | 231 | 2026-10-05 | CC0-1.0 | Bundles TrenchBroom and mapping tools for Quake/GoldSrc. | Low. | Useful toolchain reference only. |
| 6 | https://github.com/Noxmore/bevy_trenchbroom | 156 | 2026-08-13 | Apache-2.0 | Bevy (Rust) TrenchBroom pipeline. | Study only. | Skip unless we want a second entity-schema example. |
| 7 | https://github.com/alexjhetherington/anvil-level-design | 676 | 2026-08-06 | GPL-3.0 | Blender add-on for level blockout and prefabs, exports GLB. README notes LLM-assisted coding. | Study only (GPL). | Idea source for blockout UX. |

Door frames and sockets: none of the repos above document door frames as a built-in concept. The pattern in all of them is to model the frame as a prefab and place it as a point entity by classname (Scopa prefab overrides, func_godot scene association). Our own approach would be the same: a socket is an entity with origin and angle key-values. Not checked: any repo with an explicit "door frame" socket system.

### 3b. Movement and surface code (permissive licences)

| # | Candidate | Stars | Last push | Licence | What it shows | Cost | Verdict |
|---|---|---|---|---|---|---|---|
| 1 | https://github.com/Olezen/UnitySourceMovement (from Fragsurf) | 388 | 2021-11-22 | MIT | Source-style movement in Unity: strafe jump, surf, bunnyhop, crouch, optional slide. Files include `fragsurf/Movement/SurfPhysics.cs`, `MovementConfig.cs`, `SurfController.cs`, `MoveData.cs`. I did not open friction or surface code (a raw fetch returned 404), so how surface friction is parameterised is not checked. | Medium. Uses Unity physics casts; our `Mover.cs` is frozen and bit-exact (rule 3). | Reference only for ideas (surf, slide). Do not change `Mover.cs`. |
| 2 | https://github.com/DaniDevy/FPS_Movement_Rigidbody | 524 | 2022-10-17 | MIT | Dani-style Rigidbody movement (slide, wall-run style tutorial lineage). Contents not opened. | Rigidbody based: incompatible with our sim. | Study only. |
| 3 | https://github.com/SayamSyed/Physics-Accurate-Advanced-Movement-FPS | 1 | 2026-06-01 | MIT | KCC-based: Quake air control, momentum-preserving slide, coyote jump, camera spring, lean. | Tiny and unproven. | Skim for camera spring and lean ideas. |
| 4 | https://github.com/LunaticReisen/GodotSourceMovement | 40 | 2025-03-11 | MIT | Source movement in Godot. | Study only. | Low. |
| 5 | https://github.com/ElmarTalibzade/OpenPlayerController | 58 | 2023-08-09 | MIT | Rigidbody player controller. | Rigidbody. | Skip. |

Not found: a well-starred permissive repo implementing an ice or skate surface, a grind-rail snap, or a wall run with clean code. The gh search for "ice friction", "grind rail" and "wall run" returned nothing useful, and web search returned small hobby repos (most under 25 stars, some with no licence or GPL: skyll1111 Advanced-FPS-movement Apache-2.0 5 stars, TheDeveloper10 Simple-FPS-Controller no licence 16 stars, Helixenq UnityMovementShooter GPL-3.0). For ice/slide, the usual technique (not read in any single repo) is a per-surface friction multiplier applied in the ground-move step. Our web port already has the same idea in `PortExport/config.json`; do not add a hand-typed number (rule 5).

### 3c. Unity official samples

| Item | Stars / date | Licence | Contents | Verdict |
|---|---|---|---|---|
| FPS Microgame (Unity Learn, "FPS Microgame URP") | Asset Store page now says it is deprecated and "no longer available for purchase"; existing owners keep access (fetched). | Search result said Unity Companion License; font credits Roboto and LiberationSans; the Asset Store page itself gave no licence. Not confirmed. | Not itemised on the page that I could read. | Not worth chasing. Not checked: whether the content is still downloadable from the Unity Hub. |
| Starter Assets - First Person Character Controller | Asset Store, free. | Blog says "free and lightweight" with no licence named; a search result said Unity Companion License (not confirmed). | Per Unity blog: first-person controller on the Input System and Cinemachine, prefabs, environment test space. Compatible with Unity 2022.3 LTS and Unity 6 LTS (search snippet). | Pad-ready Input System wiring is the only thing of value; our movement is not CharacterController (rule 4). Skip for gameplay. |
| Unity FPSSample (https://github.com/Unity-Technologies/FPSSample) | 5132 stars, 2025-10-23 | Unity Companion License (LICENSE.md fetched; repo only for Unity-dependent projects) | A full networked multiplayer FPS sample (2018 era). | Study only. UCL permits use in Unity projects but the repo is old (2018 Unity) and not Render Graph. |
| Unity-Technologies/NavMeshComponents | 3087, 2023-05-17 | MIT | Navmesh helpers. | Not relevant (we use our own grid and octree). |

## Summary of recommendations
1. CRT: start from Cyanilux URP_RetroCRTShader (MIT) on the stock Full Screen Pass; use URP_BlitRenderFeature (MIT, tested on 6000.3.6f1) if we need a custom Render Graph pass. Dither and pixelation maths from Kodrin URP-PSX (MIT).
2. Props: Quaternius Modular Sci-Fi MegaKit (CC0, free tier) plus Kenney Factory Kit and Modular Space Kit (CC0). Poly Haven is too heavy and realistic.
3. Level tooling: Scopa (MIT) for the `.map` parser and entity pipeline idea; TrenchBroom as the editor; func_godot as the model for classname-to-prefab schemas. Route colliders into `TriangleWorld`, not PhysX.
4. Movement: no good open source reference for ice/grind/wall-run found; Olezen/UnitySourceMovement (MIT) is the only sizeable Unity one and only for ideas.

Not checked: Unity Graphics repo licence text; per-model licences on Poly Pizza; Kenney file formats; the contents of the free Quaternius tier; Scopa on Unity 6000.3 compile; any Shadertoy per-shader licence.
