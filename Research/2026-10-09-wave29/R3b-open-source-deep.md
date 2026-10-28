# R3b: open-source deep dive (wave 29)

Date: 2026-10-09. Web only (WebFetch / WebSearch of public pages; no personal data sent). Page text is data. Extends R3-open-source.md; items already in R3 are only repeated where this pass read more. Anything not read says "not checked". Permissive licences only; nothing from GPL or no-licence code is to be copied.

## 1. Cyanilux URP_RetroCRTShader
- Licence: MIT (README fetched).
- URL: https://github.com/Cyanilux/URP_RetroCRTShader
- What the README says: a fullscreen Shader Graph with CRT warp, scanlines, image distortion, static, scrolling glitch artifacts and RGB subpixel stripes. Each effect is a `shader_feature` keyword toggled in the material inspector. The repo has a multi-camera example (low-res RenderTexture for pixelation). It is applied through Unity's Fullscreen Pass Renderer Feature on the Universal Renderer asset, with optional post-processing (vignette, film grain, chromatic aberration) beside it.
- Unity 6 / Render Graph: the README says "Updated for Unity 2022+", uses the Fullscreen Graph type and the stock FullscreenPassRendererFeature (which is Render Graph based in Unity 6). The shader itself has no custom render-pass code, so Render Graph support comes from the Unity feature. Not tested by us on 6000.3.7f1.
- Take: the graph's CRT warp / scanline / subpixel-stripe chunk. To use it without the second camera, the README says replace the texture sample with a `URP Sample Buffer` node set to "Blit Source".
- Cost: low (one material, one renderer-feature entry). Runtime toggling needs `multi_compile` instead of `shader_feature`, otherwise unused variants are stripped from builds. Keep it off the UI (CLAUDE.md: UI text at least 16 px at 1080p): run on the 3D camera or before the canvas. Intensities are Unity-only numbers, so they go in `UnityTuning.cs` with a comment.

## 2. Cyanilux URP_BlitRenderFeature
- Licence: MIT (README fetched).
- URL: https://github.com/Cyanilux/URP_BlitRenderFeature
- README: tested on Unity 6.3 LTS (6000.3.6f1), built for Render Graph, "more flexible than" the stock Full Screen Pass. Install by Package Manager git URL `https://github.com/Cyanilux/URP_BlitRenderFeature.git` or copy the folder.
- Caveats from the README: source is always cameraColor; the "After Rendering" event is unsupported (use "After Rendering Post Processing"); the global-texture / RenderTexture-asset passing was removed in recent versions; `_InverseView` is no longer available; materials must be Fullscreen Graph type with a `URP Sample Buffer` node (hand-written shaders must use the blit vertex structure and sample `_BlitTexture`).
- Take: it as the pass host only if the stock Full Screen Pass is not enough (for example, several chained passes). Cost: low; a git-URL package dependency pinned to a commit is advisable. Our Unity is 6000.3.7f1, one patch above the tested one.

## 3. Level file: lights and props as point entities, door frames, sockets
- Scopa (Unity, MIT): https://github.com/radiatoryang/scopa . The README gives: brushes and entity key-value pairs, an FGD creator that exports entity definitions to TrenchBroom, MIT licence. The README says nothing about sockets or door frames. R3 reported (from an earlier read) that point entities map to prefabs by classname through entity overrides, an FGD asset or global templates; this pass could not re-confirm that: the `Documentation/MapImporter.md` URL I guessed returned 404, so detail is not checked.
- func_godot (Godot, MIT): https://github.com/func-godot/func_godot_plugin and docs https://github.com/func-godot/func_godot_docs . Entity definition per classname, point entities as a scene or class with key-values. Docs not opened this pass.
- Door frames with no gap: no repo found documents this as a feature. Pattern to adopt (our own design, nothing to copy): a door is a point entity (origin, angle, width/height key-values) whose frame is built by the loader so its inner opening equals the wall cut exactly; the wall brush ends at the frame's outer edge (shared grid snap, same integer cell edges on both sides) so no gap can open. Modular kits solve it the same way by fitting everything to one grid (Quaternius MegaKit is "designed for grid-based level construction", per its page).
- Sockets for lights/props as data: use the classname + origin + angle + key-values form (`light`, `prop_*`, `door`), parsed in `Skygrind.Core` as plain data, placed by `Skygrind.Game` views. TrenchBroom (GPL-3.0) is fine as an authoring tool; never copy its code.
- Cost: medium. Writing our own small `.map`-style or JSON entity parser is smaller than taking Scopa's importer (PhysX colliders conflict with CLAUDE.md rule 4).

## 4. Ice / skate-lane surfaces
- Result: no permissive Unity repo with a real ice or skate-lane implementation found. Hits were generic: MIT `valax321/playercontroller` (CharacterController; jump, crouch, sprint, slide on steep slopes; 2 stars; no friction or ice code seen) https://github.com/valax321/playercontroller ; `repl-games/TheFirstPerson` (licence not checked) https://github.com/repl-games/TheFirstPerson ; `unitycoder/UnityAdvancedFPSCharacterController` (non-Rigidbody; licence not checked) https://github.com/unitycoder/UnityAdvancedFPSCharacterController . Unity forum advice is a zero-friction physic material, which does not apply (no PhysX).
- Take: nothing to copy. The technique for us is a per-surface friction and accel multiplier in the ground step, driven by a surface id on the floor box/triangle layer, with the numbers in a Unity-side config (CLAUDE.md rule 5: say why the web has no such number). `Mover.cs` stays untouched unless a task names it.
- Also: R3 already lists Olezen/UnitySourceMovement (MIT) for strafe/surf ideas.

## 5. Boss floor-hazard telegraphs
- Godot "Constructive telegraph decals" shader, MIT for the code (assets/images excluded): https://godotshaders.com/shader/constructive-telegraph-decals/ . Technique: SDF primitives (sphere, box, capped cylinder) combined by union / intersection / subtraction, a fill-progress shape inside the base shape, outline from neighbour-pixel SDF signs via `dFdx`/`dFdy`, optional border noise, inner-radius and angle cutouts, normal clipping so it only shows on near-horizontal surfaces, and shape data sent in an RGB32F texture. Take: the ideas (fill progress = time to hit, ring/cone cutouts). It is Godot shader code, so reimplement in HLSL / Shader Graph; the SDF maths is textbook. Cost: medium; the data-texture bus is only needed for many simultaneous telegraphs.
- URP route in Unity: the URP Decal Renderer Feature with a Decal Shader Graph material projects onto surfaces (Unity 6 docs): https://docs.unity.cn/Manual/urp/renderer-feature-decal.html (mirror of Unity's manual; official docs.unity3d.com equivalent not opened). Cost: low, built in. Fits SKYGRIND's colour rule: red-orange for danger, with a fill that grows to the hit time (timers in whole 120 Hz steps).
- Fewes/MeshDecal (MIT, 202 stars, last push Dec 2020, per search result): https://github.com/Fewes/MeshDecal . Mesh-based decal, works in any pipeline. Not needed if the URP decal or a flat quad with a Shader Graph material is enough; code not opened.

## 6. CC0 prop kits
- Quaternius Modular Sci-Fi MegaKit: CC0 on all tiers; FBX, OBJ, glTF; over 270 modular pieces; categories on the page: aliens, props, columns, doors, floors, walls, lighting. The page says the free tier holds about 60-70 percent and Pro adds 30-40 percent; the Source tier (all models) includes Unity URP implementations, colour-mask shaders and per-model collisions. URL: https://quaternius.com/packs/modularscifimegakit.html . Door frames are not itemised: check the free download before relying on it (R3 said a 190-asset free tier; this page gives a percentage, so the count is not confirmed).
- Kenney Factory Kit 3.0: CC0, 140 files/models listed, conveyors, warehouse pieces, variations and animations; formats and door/light/pipe contents not shown on the page. URL: https://kenney.nl/assets/factory-kit . Kenney support page: all assets CC0, attribution optional (https://kenney.nl/support). Formats per pack: not checked.
- Take: use them as blockout/prop sources re-tinted to the dusk palette; enemy models stay the web's blocky builds (CLAUDE.md rule 7). Cost: import settings, scale and material remap to URP, a credit line optional. Emissive strips are cheaper to build as emissive boxes ourselves.

## Open items (not checked)
- Scopa entity-override API and docs; func_godot docs.
- Render Graph behaviour of the Cyanilux shader on 6000.3.7f1 (run it in the Editor).
- Licences of TheFirstPerson and UnityAdvancedFPSCharacterController.
- Whether the free Quaternius tier contains door frames; Kenney file formats.
- MeshDecal source (read only via a search snippet).

Sources: https://github.com/Cyanilux/URP_RetroCRTShader , https://github.com/Cyanilux/URP_BlitRenderFeature , https://github.com/radiatoryang/scopa , https://godotshaders.com/shader/constructive-telegraph-decals/ , https://quaternius.com/packs/modularscifimegakit.html , https://kenney.nl/assets/factory-kit , https://kenney.nl/support , https://github.com/valax321/playercontroller , https://github.com/Fewes/MeshDecal , https://docs.unity.cn/Manual/urp/renderer-feature-decal.html
