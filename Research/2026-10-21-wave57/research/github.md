# Open-source scout for wave 57 (2026-10-21)

**Method.** I found candidates with web search, `gh search repos` / `gh search code` and the earlier OSS scouts
(`Research/2026-10-07-wave21/research/tech-oss.md`, `2026-10-09-wave29/R3*-open-source*.md`,
`2026-10-11-wave34/research/movement-oss.md`). I didn't repeat repos those scouts already covered unless something
had changed. Stars, licence and "last push" come from `gh api repos/<r>` on 2026-10-21. "Last push" is GitHub's
`pushed_at`, which is close to the last commit date but is not exactly that. Where GitHub reported NOASSERTION, I read
the licence file and give the real licence.
**Licence flag:** **(!)** marks anything that is not MIT, Apache-2.0, BSD, Zlib, CC0 or Unlicense. GPL and LGPL code
is for study only, and none of it gets copied. "None" means the repo has no licence file, so all rights are reserved
(study only). UCL is the Unity Companion License, which allows use inside Unity projects only.
**Fit (1-5)** is scored against SKYGRIND's constraints: a pure C# 120 Hz sim with no PhysX, Unity 6 URP, pad-first.
GG owns sound and enemy design. Movement is locked (CLAUDE.md rule 3). 5 means build or port it this wave. 1 means
reference only.
**Already in the project** (Packages/manifest.json): LitMotion 2.0.2, UIEffect 5.9.7 and Input System 1.18.0. Kenney
particles and input prompts are in `Assets/ThirdParty`. `Tools/feel-metrics.py` (M1-M10, A/B, flipbook) and the
headless `Tools/SimHarness` (bot, climb, autopilot) already exist, so several picks below extend those tools rather
than adding new ones.

## 1. Movement-shooter feel: controllers, juice kits, hit-stop and shake, impacts and gibs

Movement code is frozen by rule 3, so the controllers below matter only for their **camera-side** feel (view punch,
landing dip, bob). Free equivalents of Feel are thin on the ground in Unity. The best free kit I found is for Godot.

| Project | Stars | Licence | Last push | What SKYGRIND could take | Fit |
|---|---|---|---|---|---|
| [Kelpekk/Juicee](https://github.com/Kelpekk/Juicee) (Godot 4, "inspired by FEEL") | 157 | MIT | 2026-10-14 | Its stacking rules, as a spec for our feedback layer: two shakes at once both show and return the camera home; two hit-stops can't leave time stuck at zero; two rumbles add instead of cancelling. It also has a catalogue of effects (squash, trail, damage numbers) | 4 |
| [grapefrukt/juicy-breakout](https://github.com/grapefrukt/juicy-breakout) ("Juice it or lose it") | 688 | Zlib | 2017-03-24 | Every juice effect sits behind its own toggle. That's the pattern for our blind paired A/B (L-001): one flag per effect, so a reel can switch each one off | 3 |
| [annulusgames/LitMotion](https://github.com/annulusgames/LitMotion) | 2344 | MIT | 2026-09-18 | **Installed.** A zero-allocation tween library for HUD and camera punch curves on the render side, never in the sim | 3 |
| [KyryloKuzyk/PrimeTween](https://github.com/KyryloKuzyk/PrimeTween) | 2007 | custom **(!)** | 2026-10-21 | Its `ShakeCamera` / `PunchScale` API design is worth reading. The licence lets you ship binaries but limits redistributing the source; LitMotion already covers the same ground | 2 |
| [brunomikoski/Animation-Sequencer](https://github.com/brunomikoski/Animation-Sequencer) | 1239 | MIT (needs DOTween, custom **(!)**) | 2026-03-04 | A Feel-like inspector timeline of steps. Read it for the data model only | 2 |
| [andersonaddo/EZ-Camera-Shake-Unity](https://github.com/andersonaddo/EZ-Camera-Shake-Unity) | 423 | MIT | 2020-05-23 (archived) | Several Perlin shake instances at once, each with fade-in and fade-out and separate position and rotation influence. A small piece to port into the camera rig | 3 |
| Squirrel Eiserloh, "Juicing Your Cameras With Math", GDC 2016 ([vault](https://www.gdcvault.com/play/1023557/Math-for-Game-Programmers-Juicing)) | talk | n/a | 2016 | Trauma-squared shake, Perlin rather than random noise, rotation over translation, a shake clock driven by the sim step. The reference technique | 4 |
| [Cinemachine Impulse](https://github.com/Unity-Technologies/com.unity.cinemachine) | 700 | UCL **(!)** | 2026-09-29 | The impulse source / listener model with distance falloff, for shakes from world events (boss slam). Technique only, since our camera is custom | 2 |
| [ProjectBorealis/PBCharacterMovement](https://github.com/ProjectBorealis/PBCharacterMovement) (UE C++) | 1503 | MIT | 2025-06-08 | HL2-style view punch and landing-dip camera code as a feel reference. The movement itself is locked | 2 |
| [majikayogames/SimpleFPSController](https://github.com/majikayogames/SimpleFPSController) (Godot) | 191 | CC0 | 2026-02-07 | Quake/CS:S controller. Its stair-step camera smoothing and head bob are on the camera side and can be lifted | 2 |
| [Olezen/UnitySourceMovement](https://github.com/Olezen/UnitySourceMovement), [IsaiahKelly/quake3-movement-for-unity](https://github.com/IsaiahKelly/quake3-movement-for-unity) | 387 / 105 | MIT / Unlicense | 2021 | Scouted in w29 and w34. Nothing new, and movement is frozen | 1 |
| [momentum-mod/game](https://github.com/momentum-mod/game) | 648 | Source SDK licence **(!)** | 2025-05-19 | Scouted in w34. Reference only | 1 |
| [naelstrof/SkinnedMeshDecals](https://github.com/naelstrof/SkinnedMeshDecals) | 199 | MIT | 2026-09-28 | Decals painted onto skinned or animated enemies: scorch and hit marks that stay on a pill body until it dies | 3 |
| URP Decal Projector (built in) + [Fewes/MeshDecal](https://github.com/Fewes/MeshDecal) | n/a / 250 | UCL / MIT | 2020-12-05 | World impact decals. A pool of N projectors, spawned from the sim's ray-hit events | 3 |
| [dgreenheck/OpenFracture](https://github.com/dgreenheck/OpenFracture) | 1142 | MIT | 2024-07-17 | Pre-fracture each pill model offline (editor time) into gib chunks, then fly the chunks with our own ballistic code. No PhysX, render only | 3 |
| [DavidArayan/ezy-slice](https://github.com/DavidArayan/ezy-slice), [ElasticSea/unity-fracture](https://github.com/ElasticSea/unity-fracture) | 2280 / 1330 | MIT | 2023 / 2021 | Slice along the shot plane for a "cut in two" kill read. Runtime cost not checked | 2 |
| [revanaii/MMFeedbacks](https://github.com/revanaii/MMFeedbacks) | 6 | none **(!)** | 2019-10-23 | The free 2019 MMFeedbacks dump. No licence, so study only | 1 |

## 2. Enemy AI for horde and arena shooters

No open-source DOOM token code exists. The pattern is small and well documented, and it belongs in `Skygrind.Core`.
DotRecast and RVO2-CS are the two that fit best: both are pure C#, free of engine dependencies, and permissively
licensed.

| Project | Stars | Licence | Last push | What SKYGRIND could take | Fit |
|---|---|---|---|---|---|
| DOOM (2016) attack tokens: "Embracing Push Forward Combat in DOOM", GDC 2018 ([vault](https://gdcvault.com/play/1024940/Embracing-Push-Forward-Combat-in)); [Game Developer write-up](https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-) | talk | n/a | 2018 | A shared token pool: take a token to attack, return it after. On-screen enemies can steal from off-screen ones. Pools per category (melee, ranged, heavy) and per difficulty. This fixes "everyone shoots at once" | 5 |
| [twestpha/FirstPersonEngine](https://github.com/twestpha/FirstPersonEngine) `AttackTokenComponent.cs` | 27 | GPL-3.0 **(!)** | 2024-01-28 | A Unity FPS token pool that recharges over time and gives one token per enemy until it expires. Study only, never copy | 2 |
| Mike Booth, "The AI Systems of Left 4 Dead", 2009 ([PDF](https://steamcdn-a.akamaihd.net/apps/valve/2009/ai_systems_of_l4d_mike_booth.pdf)) | paper | n/a | 2009 | An intensity meter: damage and near misses push it up, it decays, and it cycles build-up, peak, relax. Spawn pacing inside a round, run as a sim system in `simulate()` order | 5 |
| [charlie2099/Director-Ai-For-Survival-and-Shooter-Games](https://github.com/charlie2099/Director-Ai-For-Survival-and-Shooter-Games) | 28 | none **(!)** | 2024-09-27 | A Unity rule-based L4D-style director with designer-defined rules. Read its rule schema, but no licence, so study only | 3 |
| Risk of Rain 2 directors ([wiki](https://riskofrain2.wiki.gg/wiki/Directors)) | wiki | n/a | n/a | A credit budget per second spent on weighted "spawn cards", with weights per map. A data-driven round composer for our JSON level plans | 4 |
| [ikpil/DotRecast](https://github.com/ikpil/DotRecast) | 955 | Zlib | 2026-10-03 | A pure C# port of Recast/Detour with no dependencies. Build navmeshes headless from `TriangleWorld` triangles. Off-mesh links stand for pads and climb routes, and `Detour.Crowd` does avoidance. It also gives the room checker a test that every floor has an enemy route (GG: "There always really needs to be a route that enemies can climb up") | 4 |
| [snape/RVO2-CS](https://github.com/snape/RVO2-CS) (ORCA) | 859 | Apache-2.0 | 2026-10-14 | Pure C# local avoidance, so a horde surrounds the player without clumping. Run it single-threaded in a fixed agent order to keep it deterministic. Bit-exactness not checked | 4 |
| [aillieo/RVO2-Unity](https://github.com/aillieo/RVO2-Unity) | 158 | Apache-2.0 | 2026-07-26 | A Unity wrapper and demo for ORCA, useful for tuning the radius and time horizon | 2 |
| [sturdyspoon/unity-movement-ai](https://github.com/sturdyspoon/unity-movement-ai) | 2133 | MIT | 2024-05-20 | Steering: separation, cohesion, offset pursuit (surround slots), wall avoidance, wander. Strip the `Rigidbody` use and port the maths | 3 |
| [meshula/OpenSteer](https://github.com/meshula/OpenSteer), [GDQuest/godot-steering-ai-framework](https://github.com/GDQuest/godot-steering-ai-framework) | 375 / 1583 | MIT / MIT | 2023 / 2024 | Reynolds' original steering code (C++) and a clean modern rewrite (GDScript). References for blending priorities | 2 |
| Elijah Emerson, "Crowd Pathfinding and Steering Using Flow Field Tiles", Game AI Pro ch. 23 ([free PDF](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter23_Crowd_Pathfinding_and_Steering_Using_Flow_Field_Tiles.pdf)) | chapter | n/a | 2013 | One BFS flow field per player cell on `CoreGrid`, read by every enemy. Navmesh-free, cheap for hordes, and it handles climb cells | 4 |
| [RevolNoom/godot_flight_navigation_3d](https://github.com/RevolNoom/godot_flight_navigation_3d) | 63 | MIT | 2026-09-27 | A sparse voxel octree with greedy A* for flyers and wall-crawlers. A port reference if flow fields can't handle full 3D | 2 |
| [PhilS94/Unity-Procedural-IK-Wall-Walking-Spider](https://github.com/PhilS94/Unity-Procedural-IK-Wall-Walking-Spider) | 280 | none **(!)** | 2024-03-16 | Wall-walking with surface-normal alignment (a ray fan), for climbing enemies' orientation. Study only | 2 |
| [meniku/NPBehave](https://github.com/meniku/NPBehave), [ashblue/fluid-behavior-tree](https://github.com/ashblue/fluid-behavior-tree) | 1275 / 1184 | MIT | 2025 / 2026 | Event-driven and code-built behaviour trees. Only worth it if enemy logic outgrows its state machines | 2 |
| [apoch/curvature](https://github.com/apoch/curvature) + Game AI Pro 3 ch. 13 ([PDF](http://www.gameaipro.com/GameAIPro3/GameAIPro3_Chapter13_Choosing_Effective_Utility-Based_Considerations.pdf)) | 290 | BSD-3 | 2018 | Infinite-axis utility scoring for target or cover choice. The curves would live in a config file (rule 5) | 2 |
| [crashkonijn/GOAP](https://github.com/crashkonijn/GOAP) | 1773 | Apache-2.0 | 2026-10-21 | A multi-threaded GOAP planner. Too heavy for arena fodder | 1 |

## 3. Level and arena tooling: generators, checkers, playtest bots, telemetry

The best find of the scout is **Go-Explore for reachability**. Its "return to a saved cell" step is free in a
deterministic sim that can snapshot and restore, which ours can.

| Project | Stars | Licence | Last push | What SKYGRIND could take | Fit |
|---|---|---|---|---|---|
| [uber-research/go-explore](https://github.com/uber-research/go-explore) + Lu, Georgescu & Verwey, "Go-Explore Complex 3D Game Environments for Automated Reachability Testing", 2022 ([arXiv 2209.00570](https://arxiv.org/abs/2209.00570); no code released) | 587 | Apache-2.0 | 2022-12-08 | An archive of cells (voxel, speed bucket, grounded or wallride). Restore a cell, explore with random or verb macros, keep the new cells. The paper reports more navmesh coverage than curiosity RL and a 1.5 km² world covered in 10 h on one machine. For `SimHarness`: unreachable pads, stuck spots, skip routes, the real floor area of each room | 5 |
| Gordillo et al. (EA SEED), "Improving Playtesting Coverage via Curiosity Driven RL Agents", CoG 2021 ([arXiv 2103.13798](https://arxiv.org/abs/2103.13798)) | paper | n/a | 2021 | A novelty reward over visited cells, plus coverage heatmaps as the output. A good figure format for our room reports. No code | 3 |
| [Unity-Technologies/ml-agents](https://github.com/Unity-Technologies/ml-agents) | 19779 | Apache-2.0 | 2026-10-20 | Curiosity (RND/ICM) trainers. Needs a Python trainer, and our sim is plain C#, so wrapping it is possible but heavy. Go-Explore first | 2 |
| [edbeeching/godot_rl_agents](https://github.com/edbeeching/godot_rl_agents) | 1614 | MIT | 2026-07-10 | Godot only. A reference for a light environment protocol | 1 |
| [iv4xr-project/aplib](https://github.com/iv4xr-project/aplib) + [labrecruits](https://github.com/iv4xr-project/labrecruits) | 11 / 6 | LGPL-3.0 **(!)** / none **(!)** | 2025-03-03 | Goal-structured test agents ("reach button X, then door Y opens") in Java. Read the goal-tree idea for scripted room checks | 2 |
| [obsidian-level-maker/Obsidian](https://github.com/obsidian-level-maker/Obsidian) (OBLIGE successor) | 196 | GPL-3.0 **(!)** | 2026-02-13 | Twenty years of rule-based Doom arena generation: monster, health and ammo budgets per room, key/lock "quests", prefab fitting. Read its Lua rules for the round and room budget. Study only | 3 |
| de Donato, Lanzi & Loiacono, MAP-Elites for FPS maps, 2026 ([arXiv 2605.30570](https://arxiv.org/abs/2605.30570)) | paper | n/a | 2026 | Quality-diversity search over map encodings. Our room checker and walk bots would be the fitness function, with room-plan variants laid out on a grid (size × verticality). The Project Arena code wasn't found | 3 |
| [BorisTheBrave/DeBroglie](https://github.com/BorisTheBrave/DeBroglie) | 527 | MIT | 2025-12-21 | Constraint WFC in pure C# (path and count constraints). Scouted in w29. For dressing a room plan rather than laying out arenas | 2 |
| [OndrejNepozitek/Edgar-Unity](https://github.com/OndrejNepozitek/Edgar-Unity), [mxgmn/WaveFunctionCollapse](https://github.com/mxgmn/WaveFunctionCollapse) | 912 / 25422 | MIT | 2026 | Graph-based room chaining and the original WFC. GG ordered hand-designed arenas, so reference only | 1 |
| Heatmaps: [Unity Heatmaps](https://bitbucket.org/Unity-Technologies/heatmaps/wiki/) (Bitbucket), [karl-/unity-heatmap](https://github.com/karl-/unity-heatmap) | n/a / 24 | n/a / MIT | 2013 | Both are stale: Unity's needs the Analytics raw export (Pro). Better to write sim events to JSONL from `SimHarness` and the Player, then render top-down heatmaps with numpy and PIL in `feel-metrics.py` style | 2 |
| Wave and encounter designers | none | n/a | n/a | I found no open-source project worth adopting. Use the RoR2 credit director (section 2) as the model | n/a |

## 4. Visual readability: toon, rim and outline, palettes, retro, VFX, colour-blind tools

| Project | Stars | Licence | Last push | What SKYGRIND could take | Fit |
|---|---|---|---|---|---|
| [ColinLeung-NiloCat/UnityURPToonLitShaderExample](https://github.com/ColinLeung-NiloCat/UnityURPToonLitShaderExample) | 7825 | MIT | 2026-07-02 | A hand-written URP toon-lit shader. Its outline pass keeps the same width at any camera distance or FOV (`NiloOutlineUtil.hlsl`), and the cel lighting sits in one small file. It matches our hand-written `Enemy.shader` style, so lift the outline. It has no real rim term (one mention), so the red-orange danger rim (rule 8) is ours to add | 4 |
| [chrisnolet/QuickOutline](https://github.com/chrisnolet/QuickOutline) | 700 | MIT | 2024-05-23 | Stencil outlines with smoothed normals and five modes (`OutlineAll`, `OutlineVisible`, `OutlineHidden`, `OutlineAndSilhouette`, `SilhouetteOnly`). An enemy silhouette behind cover is one option to A/B | 3 |
| [IronWarrior/UnityOutlineShader](https://github.com/IronWarrior/UnityOutlineShader) | 940 | Unlicense | 2021-01-03 | A depth and normal edge-detect tutorial (built-in RP; port it to a URP full-screen pass). Room-edge lines that read at low resolution | 3 |
| [Cyanilux/URP_ShaderGraphCustomLighting](https://github.com/Cyanilux/URP_ShaderGraphCustomLighting) | 784 | MIT | 2025-07-09 | URP main and extra light functions in HLSL. Useful snippets for custom-lit shaders | 2 |
| [Unity Toon Shader](https://github.com/Unity-Technologies/com.unity.toonshader), [lilxyzw/lilToon](https://github.com/lilxyzw/lilToon) | 1609 / 1591 | UCL **(!)** / MIT | 2026-10 | Full-featured toon shaders, overkill next to our pill models | 1 |
| [Kodrin/URP-PSX](https://github.com/Kodrin/URP-PSX), [dsoft20/psx_retroshader](https://github.com/dsoft20/psx_retroshader) | 920 / 1235 | MIT | 2024 / 2022 | Scouted in w29. Vertex snap, affine textures, fog bands, colour-depth reduction. We already have dither and low resolution, so only the fog bands and colour depth are new | 2 |
| [MenacingMecha/godot-psx-style-demo](https://github.com/MenacingMecha/godot-psx-style-demo) | 740 | MIT | 2023-09-21 | The cleanest reference for a PS1 dither, fog and colour-depth stack | 2 |
| URP Color Lookup volume (built in), with a LUT baked from the palette | n/a | UCL | n/a | Bake the dusk palette into a 32³ LUT, so each palette change is one asset diff that can be measured on the frame (L-097) | 3 |
| [adobe/leonardo](https://github.com/adobe/leonardo) | 2159 | Apache-2.0 | 2026-07-08 | Generate the colour ramps from target contrast ratios against the dusk background: cyan for the player, red-orange for danger, magenta for RIVAL. Numbers rather than taste (L-097) | 4 |
| [DaltonLens/DaltonLens-Python](https://github.com/DaltonLens/DaltonLens-Python) | 44 | MIT | 2021-12-17 | Machado, Brettel and Viénot colour-vision-deficiency simulation as a Python library. Run it on Player screenshots, then measure whether cyan and red-orange still separate (protan and deutan) | 4 |
| [mariosubspace/colorblind-unity](https://github.com/mariosubspace/colorblind-unity) | 9 | MIT | 2017-10-21 | An in-game CVD-simulation post effect for a debug toggle | 2 |
| [VisualEffectGraph-Samples](https://github.com/Unity-Technologies/VisualEffectGraph-Samples) | 2136 | UCL **(!)** | 2024-11-19 | Impact, spark and muzzle examples. We use a particle shader, not VFX Graph, so only the timing and shape ideas carry over | 2 |
| [keijiro/Kino](https://github.com/keijiro/Kino), [keijiro/KinoGlitch](https://github.com/keijiro/KinoGlitch) | 2412 / 2817 | Unlicense (package.json; no LICENSE file) | 2020 / 2023 | Streak, overlay and glitch post effects for a damage or parry flash | 2 |

## 5. UI and HUD for fast shooters

There is no open-source style meter worth porting. The only ULTRAKILL-adjacent code is tiny mods. The stack below
(two already installed) covers the animation the meter needs.

| Project | Stars | Licence | Last push | What SKYGRIND could take | Fit |
|---|---|---|---|---|---|
| [mob-sakai/ParticleEffectForUGUI](https://github.com/mob-sakai/ParticleEffectForUGUI) | 6025 | MIT | 2026-10-21 | Particles inside uGUI with no extra camera or render texture: rank-up bursts and kill pops on the HUD canvas | 4 |
| [LeiQiaoZhi/Easy-Text-Effects-for-Unity](https://github.com/LeiQiaoZhi/Easy-Text-Effects-for-Unity) | 1071 | MIT | 2026-03-12 | Per-vertex TMP effects driven by curves, with presets as ScriptableObjects and edit-mode preview. For style words and rank letters in our TMP SDF fonts. The README says it is looking for maintainers | 4 |
| [mob-sakai/UIEffect](https://github.com/mob-sakai/UIEffect) | 7692 | MIT | 2026-08-27 | **Installed (5.9.7).** Shiny, flash, dissolve and transition effects for the rank-letter change. Check what's already wired before adding anything | 3 |
| [annulusgames/LitMotion](https://github.com/annulusgames/LitMotion) | 2344 | MIT | 2026-09-18 | **Installed.** Punch-scale and number-roll tweens for the meter | 3 |
| [mob-sakai/SoftMaskForUGUI](https://github.com/mob-sakai/SoftMaskForUGUI) | 2838 | MIT | 2026-08-27 | Soft-edged fill masks for the meter's drain bar | 2 |
| [Unity-UI-Extensions](https://github.com/Unity-UI-Extensions/com.unity.uiextensions) | 1739 | BSD-3 | 2026-10-13 | `UILineRenderer` and radial layouts, for hit-marker ticks and a radial cooldown | 2 |
| [end-4/ultrakill-more-revamp](https://github.com/end-4/ultrakill-more-revamp), [GAMINGNOOBdev/UltraRankSounds](https://github.com/GAMINGNOOBdev/UltraRankSounds) | 1 / 2 | MIT | 2026-05 | Mods that hook ULTRAKILL's rank change. They show which events the meter fires, nothing more | 1 |

## 6. Evaluating the game without a human: pixel metrics, VLM critics, input replay

| Project | Stars | Licence | Last push | What SKYGRIND could take | Fit |
|---|---|---|---|---|---|
| [NVlabs/flip](https://github.com/NVlabs/flip) | 655 | BSD-3 | 2026-10-13 | A perceptual difference map and score between two frames (`pip install flip-evaluator`, Python 3.10+). Add it as a metric in `feel-metrics.py ab`: "does B's frame measurably differ from A's where the eye looks?" (L-001, L-093) | 4 |
| [Breakthrough/PySceneDetect](https://github.com/Breakthrough/PySceneDetect) | 5277 | BSD-3 | 2026-10-21 | A content score per frame over a reel. Impact spikes on a timeline; runs of identical frames reveal hit-stop and how long it lasts | 4 |
| [richzhang/PerceptualSimilarity](https://github.com/richzhang/PerceptualSimilarity) (LPIPS) | 4288 | BSD-2 | 2024-07-02 | A learned perceptual distance, for whether two looks are really different (palette and room identity, T-127) | 3 |
| [princeton-vl/RAFT](https://github.com/princeton-vl/RAFT) | 4117 | BSD-3 | 2025-08-24 | Optical-flow magnitude as a "speed read" metric on movement reels. Needs a GPU, cost not checked | 2 |
| [Netflix/vmaf](https://github.com/Netflix/vmaf) | 5565 | BSD-2-Clause-Patent | 2026-10-19 | Video quality scoring. Only for checking the compression of trailers and reels | 1 |
| [VideoGameQA-Bench](https://asgaardlab.github.io/videogameqa-bench/) ([arXiv 2505.15952](https://arxiv.org/abs/2505.15952); [data](https://huggingface.co/datasets/taesiri/VideoGameQA-Bench)) | 7 likes | site CC0; data CC-BY-4.0 **(!)** | 2025-12-30 | Task formats for our critic agents: visual unit test, UI unit test, visual regression, clipping detection, video needle-in-a-haystack. Copy the prompt and answer formats | 4 |
| [GlitchBench](https://huggingface.co/datasets/glitchbench/GlitchBench) ([code](https://github.com/GlitchBench/Benchmark), CVPR 2024) | 13 | data MIT; code none **(!)** | 2024-07-28 | Calibrate a VLM critic before trusting it: run it on these known glitches and record the hit rate | 3 |
| [lmgame-org/GamingAgent](https://github.com/lmgame-org/GamingAgent) | 994 | MIT | 2025-11-16 | A VLM/LLM agent harness and benchmark (ICLR 2026). Its harness pattern (perception, memory, reasoning modules) suits a screenshot-driven critic playthrough | 3 |
| [BAAI-Agents/Cradle](https://github.com/BAAI-Agents/Cradle) | 2594 | MIT | 2024-11-07 | General computer control over screenshots and keyboard/mouse. Too slow for a 120 Hz shooter, but fine for menus and flow | 2 |
| [AirtestProject/Airtest](https://github.com/AirtestProject/Airtest) | 9613 | Apache-2.0 | 2026-03-23 | Image-template assertions on the Player window on Windows ("rank letter visible", "no error dialog") | 3 |
| [AirtestProject/Poco](https://github.com/AirtestProject/Poco), [gwaredd/unium](https://github.com/gwaredd/unium) | 1960 / 237 | Apache-2.0 / MIT | 2024 / 2022 | Query the scene and UI of a running Unity Player. Mostly covered already by `unity-cli` and the Player check flags | 2 |
| [alttester/AltTester-Unity-SDK](https://github.com/alttester/AltTester-Unity-SDK) | 122 | GPL-3.0 **(!)** | 2026-09-23 | UI-driven Unity test automation. The GPL SDK would ship inside the build, so avoid it | 1 |
| Input System `InputEventTrace` (installed 1.18.0; UCL) | n/a | UCL | n/a | Record a real pad session (GG's playtest) to a file and replay it through our input layer into the deterministic sim. That turns a human run into a regression reel | 3 |
| [Eurekaleo/awesome-ai-for-games](https://github.com/Eurekaleo/awesome-ai-for-games) | 399 | MIT | 2026-10-21 | A current, searchable index of game-AI papers (testing, playtesting, generation). Where to look next | 2 |

## Top 10 to study or port (ranked)

1. **Go-Explore reachability in `SimHarness`.** Sources: [uber-research/go-explore](https://github.com/uber-research/go-explore)
   (Apache-2.0) and [arXiv 2209.00570](https://arxiv.org/abs/2209.00570). Port the algorithm: an archive keyed on
   (voxel, speed bucket, contact state), restore by sim snapshot, explore with verb macros (dash, slide, wallride,
   grapple, pad). The output per room is coverage, unreachable pads, stuck cells and skip routes. It fits the
   deterministic sim and the existing walk bots and room checker exactly.
2. **DOOM attack tokens as a Core system.** Source: [GDC 2018 talk](https://gdcvault.com/play/1024940/Embracing-Push-Forward-Combat-in).
   A token pool per category, a steal rule and difficulty scaling, timers in whole steps, numbers in a Unity-side config
   file (rule 5). It fixes the readability of "everyone fires at once". This is enemy behaviour, so it needs GG's go-ahead.
3. **An intensity director.** Sources: [Booth 2009 L4D PDF](https://steamcdn-a.akamaihd.net/apps/valve/2009/ai_systems_of_l4d_mike_booth.pdf)
   and the [RoR2 credit director](https://riskofrain2.wiki.gg/wiki/Directors). The build-up, peak, relax meter
   paces spawns inside a round, and spawn cards are bought with credits. [charlie2099's repo](https://github.com/charlie2099/Director-Ai-For-Survival-and-Shooter-Games)
   has no licence, so read its rule schema only.
4. **[ikpil/DotRecast](https://github.com/ikpil/DotRecast)** (Zlib, pure C#). A headless navmesh from `TriangleWorld`,
   with off-mesh links for pads and climbs. Its first use is a checker: "every floor has an enemy route". Detour.Crowd
   is a possible later step.
5. **[snape/RVO2-CS](https://github.com/snape/RVO2-CS)** (Apache-2.0, pure C#). ORCA surround and anti-clumping, run
   single-threaded in a fixed order. Pair it with the offset-pursuit slots from [unity-movement-ai](https://github.com/sturdyspoon/unity-movement-ai) (MIT).
   Run the bit-exactness check before any Core commit.
6. **[NVlabs/flip](https://github.com/NVlabs/flip) + [PySceneDetect](https://github.com/Breakthrough/PySceneDetect)**
   (both BSD-3) inside `Tools/feel-metrics.py`. A perceptual A/B difference, a per-frame impact spike and hit-stop
   detection, so feel is judged on measured pixels (L-001, L-093).
7. **[VideoGameQA-Bench](https://asgaardlab.github.io/videogameqa-bench/) formats + [GlitchBench](https://huggingface.co/datasets/glitchbench/GlitchBench)
   calibration** (site CC0, data CC-BY-4.0; data MIT). Structure the critic agents' screenshot checks as visual unit and
   regression tests, and record the critic's hit rate on known glitches first.
8. **[ParticleEffectForUGUI](https://github.com/mob-sakai/ParticleEffectForUGUI) + [Easy-Text-Effects](https://github.com/LeiQiaoZhi/Easy-Text-Effects-for-Unity)**
   (both MIT), with the installed LitMotion and UIEffect. Style meter and rank-letter animation (bursts, per-letter
   shake) with no new camera.
9. **[UnityURPToonLitShaderExample](https://github.com/ColinLeung-NiloCat/UnityURPToonLitShaderExample)** (MIT, 7.8k
   stars). Lift its outline pass, which keeps the same width at any distance or FOV, into `Enemy.shader` and add our
   own danger rim, so enemies read against the dusk at low resolution. Pair it with [QuickOutline](https://github.com/chrisnolet/QuickOutline)'s
   `SilhouetteOnly` mode for enemies behind cover. Check that the shader reads the property being set (L-096).
10. **[adobe/leonardo](https://github.com/adobe/leonardo) (Apache-2.0) + [DaltonLens-Python](https://github.com/DaltonLens/DaltonLens-Python)
    (MIT).** Generate the palette from contrast targets, then simulate colour-vision deficiency on Player screenshots to
    show that the cyan and red-orange split survives.

Honourable mentions: [Kelpekk/Juicee](https://github.com/Kelpekk/Juicee) (MIT; its effect-stacking rules as a spec)
and the [flow-field tiles chapter](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter23_Crowd_Pathfinding_and_Steering_Using_Flow_Field_Tiles.pdf)
(navmesh-free climbing hordes on `CoreGrid`).

**Gaps.** No open-source project offers any of these: a Feel equivalent for Unity, a style meter, a wave or encounter
designer, the DOOM token code, or code for the EA SEED and Microsoft playtesting papers. **Not checked:** runtime cost
of the slicing and fracture libraries, RVO2-CS bit-exactness, and the FLIP and RAFT cost on 1080p reels.
