# SKYGRIND tool inventory and Asset Attribution Sheet (ASU AME 294, Portfolio Game 3)

Status: DRAFT by a coding agent (Claude Sonnet 5.5), 2026-10-28, from files in this repository. Licence names and copyright lines
are copied from the licence files and source headers named. I did not open any website, so every URL below is as the repository
records it and every "terms" line needs GG's check against the current page. `TODO(GG)` marks what only GG knows.

## 1. Generative AI models and tools, by kind of work

### 1.1 Code
| Tool | What it did | Evidence |
|---|---|---|
| Claude Code (Anthropic's CLI agent), model Claude Opus 5.5 | Wrote almost all the code, tests, tools and docs from GG's night prompts, as the lead and as builder, critic and reviewer agents | Commit trailer "Co-Authored-By: Claude Opus 5.5" on 2,622 commits; the night prompts' header "model / effort: claude-opus-5-5 / high" (`nights/NIGHT21.md`); agent files `model: opus`, `effort: high` (`.claude/agents/*.md`) |
| Claude Opus 5.5 (1M context) | The same, in long sessions | Trailer on 1,003 commits |
| Claude Sonnet 5.5 | The same, in shorter sessions; also wrote these `docs/pg3` drafts | Trailer on 462 commits |
| Unity's Claude Code plugin and `unity` CLI (skill `unity:unity-cli`) | Drove the Unity Editor: compile, run tests, Play, build, run C# | `CLAUDE.md` "Workflow"; `Tools/unity-loop.sh` |
| Skills and agents | Skills `wave-forge` (writes the next night's prompt), `wave-memory` (long-term memory), `night-context` (session brief) at user level; `skygrind-idea-lab` and `skygrind-retro` in the repo; agents `skygrind-spotter`, `skygrind-critic`, `gg-proxy`, `skygrind-reviewer`, `dead-code-auditor` | `.claude/skills/`, `.claude/agents/`, `nights/PROMPT-FORGE.md` |
| Reverse Engineer Anything skill (REA) | GG asked for it on 2026-10-11: "lean heavily on the Reverse Engineer Anything skill and start pulling insights from GitHub repos, open-source movement systems, and other games" | OWNER.md (w33 row); how often it ran: not checked |
| Unity AI packages `com.unity.ai.assistant` 2.20.0-pre.1 and `com.unity.ai.inference` 2.6.1 | Present in the project from its first commit (the Unity 6.3 template); whether GG used them: `TODO(GG)`, not checked | `Packages/manifest.json`; commit `3fee5c7c3` |
| Other chat or code models | None found in the repository. A ChatGPT window was open on the PC during the 2026-10-22 crash (auto-memory `pc-out-of-memory-crash`); whether it was used for SKYGRIND: `TODO(GG)` | |

Approval and direction: GG's messages (`docs/memory/OWNER.md`) set each night; the agents wrote the code. Every commit carries one
author name, Gaige Geraci (GG's git identity), with the agent named in the "Co-Authored-By" trailer.

### 1.2 3D models
| Tool | What it did | Evidence |
|---|---|---|
| Tripo (tripo3d.ai) | The three THE KILN WYRM parts (head, body, tail), made by GG, 2026-10-08 | `Assets/Art/WormBoss/*/tripo_convert_*.fbx`; commit `8eff2485c`; `NIGHT26-LOG.md` ("NEW from GG at 21:30: the worm gets the Tripo models"). PG3 models (KESTREL, husk, THE WICK): `TODO(GG)` |
| Plan, prompts, credits | `TODO(GG)` for every model | `PROMPT_LOG.md` section 4 |

### 1.3 Audio
| Tool | What it did | Evidence |
|---|---|---|
| ElevenLabs sound-generation API | The 28 PG3 clips: effects for the Reward, Damage and End events, weapon and movement sounds, and a looped music bed | `Tools/pg3/sounds.json`, `Tools/pg3/sfx-gen.cjs`, commit `818051f66`. No clip had been generated when this draft was written |
| Plan or tier, key owner, generation date | `TODO(GG)` | |
| Suno (PG2, the web game only) | A "Suno workflow" is described for the web game's music; not in the Unity build. Whether you used it: `TODO(GG)` | `C:\Users\gaige\dev\skygrind\MUSIC-BRIEF.md` |

### 1.4 Image and video (trailer)
| Tool | What it did | Evidence |
|---|---|---|
| Image generator, image-to-video tool | The trailer's 0 to 3 s hook, if used | `TODO(GG)`: names, plans, dates, prompts (`PROMPT_LOG.md` section 6) |
| Game footage | Captured from the game itself: the reel system writes 1920 x 1080, 60 fps JPG sequences (`Tools/night/reel.sh`), or a live capture of the Player | `Logs/feel/n74-play/*/reel.json` |
| ffmpeg 9.0.2 (WinGet `Gyan.FFmpeg`, full build) | Assembling and encoding: not an AI tool; listed because the brief asks. The exact command: `TODO(GG)` | On this machine's `PATH` |

### 1.5 Other software (not generative AI)
Unity 6000.3.7f1 (URP 17.3.0); Git; Node.js scripts (`Tools/night/*.cjs`); a .NET harness (`Tools/CoreHarness`, `Tools/SimHarness`);
Python scripts (`Tools/feel-metrics.py` and others). Unity licence tier: `TODO(GG)`.

## 2. Asset Attribution Sheet

### 2.1 Fonts (all SIL Open Font License 1.1)
Each family's `OFL.txt` sits beside its font files in `Assets/Fonts/<Family>/`. The in-game text uses TextMesh Pro SDF atlases
converted from these files by `Assets/Editor/ThemeFonts.cs` into `Assets/Resources/Fonts/`; the code comment says the font files were
"fetched once from Google Fonts' GitHub with their OFL.txt".

| Family | Faces in the project | Copyright line in its `OFL.txt` |
|---|---|---|
| Archivo | ExtraBold, ExtraCondensedBlackItalic, SemiBold | Copyright 2020 The Archivo Project Authors (https://github.com/Omnibus-Type/Archivo) |
| Barlow Semi Condensed | Bold, SemiBold | Copyright 2017 The Barlow Project Authors (https://github.com/jpt/barlow) |
| Big Shoulders Display | Black | Copyright 2019 The Big Shoulders Project Authors (https://github.com/xotypeco/big_shoulders) |
| Chakra Petch | BoldItalic, SemiBold | Copyright 2018 The Chakra Petch Project Authors (https://github.com/m4rc1e/Chakra-Petch.git) |
| Saira | SemiCondensedSemiBold | Copyright 2020 The Saira Project Authors (https://github.com/Omnibus-Type/Saira) |
| Tourney | BlackItalic | Copyright 2020 The Tourney Project Authors (https://github.com/Etcetera-Type-Co/Tourney) |
| Share Tech Mono | Regular (in `Assets/Fonts`; whether any text uses it: not checked) | Copyright (c) 2012, Carrois Type Design, Ralph du Carrois (post@carrois.com www.carrois.com), with Reserved Font Name 'Share' |
| Liberation Sans | TextMesh Pro's default font and fallback asset | `Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt`; its text was not read |

The ten SDF assets in `Assets/Resources/Fonts/` are the six families above (Archivo three faces, Barlow two, Big Shoulders one,
Chakra Petch two, Saira one, Tourney one). Keep the `OFL.txt` files in any release.

### 2.2 Art and effects (CC0, Kenney)
| Asset | Licence | Credit line | Path |
|---|---|---|---|
| Input Prompts 1.5A (Xbox, PlayStation and Switch button glyphs, packed into one atlas) | CC0 1.0 | "Created/distributed by Kenney (www.kenney.nl)"; its `License.txt` shows a creation date written 11-07-2026 | `Assets/ThirdParty/KenneyInputPrompts/` |
| Particle Pack 1.1 (8 sprites in the project: `circle_02`, `dirt_01`, `flare_01`, `smoke_04`, `spark_07`, `star_06`, `trace_06`, `twirl_02`) | CC0 1.0 | "by Kenney Vleugels (Kenney.nl)"; "Additional credit for creating filter templates: Indigo Ray, Craig Nisbet, Zoltan Erdokovy, Heliagon, ThreeDee, Killst4r and Tim2501" | `Assets/ThirdParty/KenneyParticles/` |

Both licence files say credit is "nice" but "not mandatory" (the Input Prompts file: "Support by crediting 'Kenney' or 'www.kenney.nl' (this is not a requirement)"). Used in `Game/Input/PromptGlyphs.cs`, `Game/FxSprites.cs`, `Game/HitSpark.cs` and others.

### 2.3 3D models (Tripo)
| Asset | Files | Made by | Terms |
|---|---|---|---|
| THE KILN WYRM head, body, tail | `Assets/Art/WormBoss/Head`, `Body`, `Tail` (`tripo_convert_af829661...`, `..._9d612cea...`, `..._9e704a7a...` `.fbx` with `.fbm` texture folders and `WormBoss_*.mat`); baked into `Assets/Resources/WormBossParts.asset` | Generated on Tripo, exported by GG, 2026-10-08. Prompt, plan, credits: `TODO(GG)`. Drawn only when `design.n26.wormVisual.enabled` is true (it is false) | See below |
| KESTREL, husk, THE WICK | `Assets/Art/Tripo/` (not yet in the repo) | `TODO(GG)` | See below |

Tripo's terms: models generated on Tripo's free plan are public, and the plan decides the commercial rights, so record the plan for
each model. Cite Tripo's terms page, https://www.tripo3d.ai/terms (check current wording; not opened by the agent), and say in the
README whether any shipped model came from the free plan. `TODO(GG)`: plan and date per model.

### 2.4 Audio (ElevenLabs)
All 28 clips are generated through the ElevenLabs API, not recorded or sampled by us. Output goes to
`Assets/Resources/Audio/PG3/<id>.mp3` (mp3, 44.1 kHz, 128 kbit/s). Terms: https://elevenlabs.io/terms-of-use is the page to cite
(check current wording; not opened by the agent). The rights to generated audio depend on the plan used at generation time, and some
free tiers ask for attribution and limit commercial use; verify for your plan. `TODO(GG)`: plan, date of generation, and the
`Tools/pg3/sfx-gen-log.json` that the script writes. Prompts: `PROMPT_LOG.md` section 5.

### 2.5 Code ported or adapted from other projects
"Ported" means rewritten in C# from the source named; the file header carries the credit. Where a licence text is kept in the file
it is noted.

| Source | Licence and copyright | What was taken | Where |
|---|---|---|---|
| GG's own web game SKYGRIND (PG2; `C:\Users\gaige\dev\skygrind`, tag `p17-night6`) | GG's own | The simulation, movement, collider, guns, enemies, director, economy: ported module by module, line for line with the web's comments | `Assets/Scripts/Core/` |
| three.js r186, Octree and helpers | MIT (the web game's notices list three.js as MIT) | The collision world, reproduced exactly | `Core/ThreeOctree.cs` |
| V8 13.6 (Node 24.19.0): fdlibm functions, `Math.hypot`, `Math.round` | fdlibm notice kept in the file: "Copyright (C) 1993 by Sun Microsystems, Inc. All rights reserved. Developed at SunSoft, a Sun Microsystems, Inc. business. Permission to use, copy, modify, and distribute this software is freely granted, provided that this notice is preserved." V8's own licence (BSD-3-Clause, Google) is not reproduced in the repo: `TODO(GG)` add it | JavaScript maths reproduced bit for bit so the C# sim matches the web's doubles | `Core/JsMath.cs`, `JsMath.Fdlibm.cs` |
| orangeduck/Spring-It-On | MIT, Copyright (c) 2021 Daniel Holden (licence text kept in the file) | Critically damped half-life springs | `Game/Viewmodel.Springs.cs` |
| keijiro/KvantStream | MIT, Copyright (C) 2015 Keijiro Takahashi | The head-to-tail streak rule | `Game/SpeedLines20.cs`, `Shaders/Resources/SpeedLines20.shader` |
| MikeFrom/Godot-Particles-Tunnel-Speed-Lines | CC0-1.0 | Births on a ring, quads along the wall | `Game/SpeedLines20.cs` |
| Bauxitedev/godot-motion-blur | MIT, Copyright (c) 2018 Bauxitedev | Depth-based motion blur, linear only | `Game/Render/TravelBlurFeature.cs`, `TravelBlur.shader` |
| daniel-ilett/blur-pro-urp | MIT, Copyright (c) 2026 Daniel Ilett | The Unity 6 render-graph pass recipe | `Game/Render/TravelBlurFeature.cs` |
| dip000/my-godotshaders `WindTrailWithLoops` | MIT, Copyright (c) 2023 Adrián | The wind wisps' vertex wave | `Game/MovementFx.Wind20.cs` |
| chrisnolet/QuickOutline | MIT, "Copyright (c) 2018 Chris Nolet" | The smoothed-normal technique only | `Game/Skin52.cs`, `Shaders/Enemy.shader` |
| ColinLeung-NiloCat/UnityURPToonLitShaderExample | MIT, "Copyright (c) 2020 ColinLeung-NiloCat" | The cel ramp's form | `Shaders/Enemy.shader` |
| Minoteaur / UnityHFSM (https://github.com/Inspiaaa/UnityHFSM, as `BossFsm27.cs` writes it) | MIT | The state-machine pattern; no file copied | `Core/Enemies/BossFsm27.cs` |
| CoffweDev/physics-based-wallrunning-unity | MIT | Parameters read as a cross-check; nothing copied | `docs/THIRD-PARTY.md` |

The MIT texts for the projects whose licence is not kept in a source file (KvantStream, godot-motion-blur, blur-pro-urp,
my-godotshaders, QuickOutline, ToonLit) are in those projects' own repositories; `TODO(GG)`: copy them into a `NOTICES.txt` for the
Release. The fuller table, with what each reading was: `docs/THIRD-PARTY.md`. Ideas read and not copied (for example the Pioneer
space-sim's speed lines, GPL-3.0, "idea only, copy no code") are in `Research/2026-10-06-speed-lines.md`.

### 2.6 Unity packages (`Packages/manifest.json`)
| Package | Version | Licence | Note |
|---|---|---|---|
| LitMotion (`com.annulusgames.lit-motion`, annulusgames) | v2.0.2, from GitHub | MIT ("LICENSE read at the tag", commit `8cb1ab174`) | Tweens |
| UIEffect (`com.coffee.ui-effect`, mob-sakai) | 5.9.7, from GitHub | MIT ("LICENSE.md read at the tag", commit `38e522943`) | HUD effects; `docs/memory/FEATURES.md` notes it installs in the Editor only, so a Player has no shine or edge glow |
| Universal Render Pipeline | 17.3.0 | Unity | Rendering |
| Input System | 1.18.0 | Unity | Keyboard, mouse and pad |
| uGUI (includes TextMesh Pro) | 2.0.0 | Unity | UI; TMP Essentials imported in NIGHT7 (`Assets/TextMesh Pro`) |
| Burst, Collections, Mathematics | 1.8.27, 2.6.2, 1.3.3 | Unity | Added for the enemy bench (commit `d5c5b0a0a`) |
| AI Navigation, Test Framework, Timeline, Visual Scripting | 2.0.9, 1.6.0, 1.8.10, 1.9.9 | Unity | Template and test packages |
| Unity AI Assistant, AI Inference | 2.20.0-pre.1, 2.6.1 | Unity | In the template; use not checked |
| Pipeline (`com.unity.pipeline`) | 0.8.0-exp.1 | Unity (experimental) | The server the `unity` CLI talks to |
| IDE integrations (Rider, Visual Studio), Collab Proxy, Multiplayer Center | 3.0.39, 2.0.26, 2.11.3, 1.0.1 | Unity | Editor tools |

Licence of the Unity-owned packages: `TODO(GG)` confirm the current Unity terms (the Unity Companion License covers most registry
packages; I did not check each package here).

### 2.7 Not in this build
The web game used Quaternius' Ultimate Guns Pack (CC0) for its revolver and shotgun models and downloaded Kenney's Blaster Kit (CC0)
without using it (`C:\Users\gaige\dev\skygrind\THIRD_PARTY_NOTICES.md`). The Unity project has no `.glb`, `.obj`, `.blend` or audio
file; its only model files are the three Tripo FBX files. The guns are built in code from boxes and tubes (`Game/BlockyKit.cs`).
The web game's Yuka steering library was not carried over as code; the Unity port reimplemented the steering it needed
(NIGHT7 commit `f35d0fdcd`; whether any Yuka code was copied: not checked).

## 3. Credit text (draft for the README and the end card)

Game, design and direction: Gaige Geraci. Code: written with Claude Code (Claude Opus 5.5 and Claude Sonnet 5.5, Anthropic).
3D models: generated with Tripo (tripo3d.ai). Sound and music: generated with ElevenLabs. Fonts: Archivo, Barlow Semi Condensed,
Big Shoulders Display, Chakra Petch, Saira, Tourney (SIL OFL 1.1). Button glyphs and particle sprites: Kenney (kenney.nl, CC0).
Built in Unity 6. Open-source credits: see `docs/pg3/ATTRIBUTION.md`.

## 4. Gaps for GG to close

1. Tripo and ElevenLabs plan, date and prompt for every generated asset (sections 2.3 and 2.4).
2. V8's BSD-3 licence text and the MIT texts listed under 2.5 for a `NOTICES.txt`.
3. Whether Share Tech Mono is used; whether the Unity AI packages were used.
4. The Unity licence tier and the current terms pages for Tripo, ElevenLabs and Unity (I opened none).
5. The image and video tools for the trailer's hook shot.
