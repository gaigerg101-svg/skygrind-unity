# THIRD-PARTY (NIGHT27 entries; art and input glyphs are in `Assets/ThirdParty/`)

Rule (NIGHT27.md): copy only MIT / Apache / CC0 / Unlicense; record URL, licence, and what was taken. A reading of a repo for its pattern or parameters is not a copy.

| Source | Licence | What was taken (NIGHT27) |
| --- | --- | --- |
| Minoteaur / UnityHFSM (hierarchical state machine) | MIT | The pattern only (hierarchical states, one transition per step, `needsExitTime` for attack states), rewritten from scratch in pure C# in `Skygrind.Core` for the boss FSM base (builder B). No source file copied. Repo owner as named in NIGHT27.md: Minoteaur / UnityHFSM (URL not verified tonight). |
| CoffweDev / physics-based-wallrunning-unity | MIT | Parameters only, read as a cross-check of the wall-ride feel; Rigidbody based, never copied. |
| UnitySpline2D, unity-curves | MIT | Named in the brief for the curving course's view math; whether builder B took anything from them: not checked (see the glacier commit). |
| mirzabeig / anime-speed-lines | Unlicense | Reference only. The border effect is banned by GG; nothing taken. |
| Titanfall 2 and Karlson wall-run repositories | none | Not copied. |

## NIGHT52 (builder U, the polish lane; NIGHT52.md section 4; techniques ported by reading, no file vendored)

| Source | Licence | What was taken (NIGHT52) |
| --- | --- | --- |
| https://github.com/chrisnolet/QuickOutline (`QuickOutline/Scripts/Outline.cs`, read 2026-10-19) | MIT (`License.txt`, GitHub API spdx MIT 2026-10-19; the .cs file's own header reads "Copyright (c) 2018 Chris Nolet. All rights reserved.") | skin52: the smoothed-normal technique only (group a mesh's vertices by position, average their normals, normalise, store in UV channel 3; the outline pass pushes along it), rewritten in `Assets/Scripts/Game/Skin52.cs` (Smoothed) and the ink pass of `Assets/Shaders/Enemy.shader`. No code copied. |
| https://github.com/ColinLeung-NiloCat/UnityURPToonLitShaderExample (`SimpleURPToonLitOutlineExample_LightingEquation.hlsl`, read 2026-10-19) | MIT (`LICENSE`, "Copyright (c) 2020 ColinLeung-NiloCat") | skin52: the cel ramp's form, `smoothstep(mid - softness, mid + softness, N.L)` between a shadow colour and the lit one, written into the body pass of `Assets/Shaders/Enemy.shader` with our own key light and numbers (UnityTuning.U52.cs). No code copied. |
| game-feel skill (local, `~/.claude/skills/game-feel`) | n/a (our own) | hud52: the decaying-trauma shake (shake = trauma^2, smooth sines, decays to rest) and the eased pop; written in `Assets/Scripts/Game/UiHud52.cs` (Hud52Math). |
| TMPEffects, LitMotion, UIEffect (named in RESEARCH.md B for hud52) | MIT | Not used tonight: hud52's motion is the project's own UiTween curves; nothing taken. |
| Cyanilux URP_ShaderCodeTemplates (named for guns52) | CC0 | Not used: guns52 is geometry (chamfered parts, recessed two-tone panels, lit edge faces in `Assets/Scripts/Game/BlockyKit.Guns52.cs`, our own code), drawn by the existing SKYGRIND/Stone material; no shader added. |
