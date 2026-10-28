# Tech and OSS for cheap notable features (wave 21)

Web research only, 2026-10-07, about 12 minutes. Nothing here was built, run or measured.

Marks:
- **[F]** the page itself was fetched and read.
- **[S]** the claim comes from a search-result summary of that page; the page was not opened. Treat the licence as
  unverified until someone opens the repo's LICENSE file.
- **[I]** my inference for SKYGRIND, not a sourced claim.

A rule that holds for every row ([I]): SKYGRIND's gameplay runs in `Skygrind.Core` on its own collider at 120 Hz, with
no PhysX. So any Unity repo below that moves a Rigidbody, a CharacterController or a SpringJoint is a **reference for
the maths only**, never a drop-in. Visual-only code (fragments, cameras, capture) can be used as is, outside the sim.

## Sources

Replay and determinism
- Doom demo (LMP) format: https://doomwiki.org/wiki/Demo [S]
- Doom 1.91 demo format: https://www.chocolate-doom.org/wiki/index.php/Doom_1.91_demo_format [S]
- Unofficial LMP format: https://www.gamers.org/docs/FAQ/lmp.faq.html [S]
- Quake DEM format (server messages): https://www.gamers.org/dEngine/quake/Qdem/dem-1.0.2-2.html [S]
- Quake III demo specs: http://www.elho.net/games/q3/q3dspecs.htm [S]
- Trackmania replay revalidation service: https://github.com/BigBang1112/revalidate [S]
- TMInterface (Trackmania input tooling): https://donadigo.com/tminterface/ [S]
- Neon White ghost parser (.phant format): https://github.com/PandorasFox/neon-white-ghost-parser [S]
- Overwatch replay tech talk (GDC 2017): https://gdcvault.com/browse/gdc-17/play/1024053 [S]
- Write-up of that talk: https://www.gamedeveloper.com/design/video-how-blizzard-designed-and-built-i-overwatch-i-s-replay-tech [S]
- Overwatch gameplay architecture and netcode (GDC 2017): https://gdcvault.com/browse/gdc-17/play/1024001 [S]
- Gaffer on Games, Deterministic Lockstep: https://gafferongames.com/post/deterministic_lockstep/ [S]
- Gaffer on Games, Snapshot Interpolation: https://gafferongames.com/post/snapshot_interpolation/ [S]
- Moments, GIF replay recorder for Unity: https://github.com/Chman/Moments [S]

Movement
- Quake III movement for Unity (Unlicense fork): https://github.com/mirogon/quake3-movement-for-unity [S]
- Original fork: https://github.com/IsaiahKelly/quake3-movement-for-unity [S]
- VelocityMovement (bhop, MIT): https://github.com/TheAsuro/VelocityMovement [S]
- UnitySourceMovement (Source movement, MIT): https://github.com/Olezen/UnitySourceMovement [F]
- Fragsurf repositories: https://github.com/orgs/Fragsurf/repositories [S]
- Fragsurf character controller (MIT): https://github.com/MirceaPrunaru/Fragsurf-Character-Controller [S]
- Grappling hook component (MIT): https://github.com/ZacharyBuffone/UnityGrapplingHookComponent [S]
- Grappling hook game (MIT): https://github.com/jeremiahkellick/grappling-hook-game [S]
- UnityBezierSolution (MIT): https://github.com/yasirkula/UnityBezierSolution [S]
- Unity-Spline-Movement (CC0-1.0): https://github.com/sKm-games/Unity-Spline-Movement [S]
- Titanfall controls interview: https://www.gamedeveloper.com/design/designer-interview-getting-i-titanfall-i-s-controls-just-right [S]
- Titanfall 2 movement analysis: https://medium.com/@jackpbenjamin18/titanfall-2-dimensions-of-mobility-the-creation-of-the-movement-shooter-ee03c028eecd [S]

Destruction
- OpenFracture (MIT): https://github.com/dgreenheck/OpenFracture [S]
- OpenFracture release thread: https://discussions.unity.com/t/released-openfracture-fracture-and-slice-meshes-during-runtime-open-source/867250 [S]
- EzySlice (MIT): https://github.com/DavidArayan/ezy-slice [S]
- VoxelEngine (Jobs and Burst voxel destruction): https://github.com/Kugelhaufen/VoxelEngine [S]
- Voxelfield (destructible-terrain shooter): https://github.com/qhdwight/voxelfield [S]
- Teardown structural rule: https://en.wikipedia.org/wiki/Teardown_(video_game) [S]

Photo mode and cameras
- Photo Session thread: https://discussions.unity.com/threads/free-photo-session-ingame-precision-screenshot-utility-open-source-github.1076252/ [F]
- URP autofocus DOF script: https://github.com/DjoleDzele/URP-Autofocus-DOF-Script [S]
- Cinemachine licence (Unity Companion License): https://docs.unity3d.com/Packages/com.unity.cinemachine@2.9/license/LICENSE.html [S]

Gamepad, gyro, haptics
- GamepadMotionHelpers (MIT): https://github.com/JibbSmart/GamepadMotionHelpers [S]
- JoyShockLibrary (MIT): https://github.com/JibbSmart/JoyShockLibrary [S]
- GyroWiki, flick stick: https://gyrowiki.jibbsmart.com/blog:good-gyro-controls-part-2:the-flick-stick [S]
- GyroWiki, the gyro is a mouse: http://gyrowiki.jibbsmart.com/blog:good-gyro-controls-part-1:the-gyro-is-a-mouse [S]
- GyroWiki, player space gyro: http://gyrowiki.jibbsmart.com/blog:player-space-gyro-and-alternatives-explained [S]
- Flick Stick: https://en.wikipedia.org/wiki/Flick_Stick [S]
- UniSense, DualSense for Unity Input System (MIT, archived June 2025): https://github.com/nullkal/UniSense [S]
- Unity Input System DualSenseGamepadHID: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.8/api/UnityEngine.InputSystem.DualShock.DualSenseGamepadHID.html [S]
- Steamworks.NET (MIT): https://github.com/rlabrecque/Steamworks.NET [S]
- ISteamInput API: https://partner.steamgames.com/doc/api/isteaminput [S]
- Steam Input adds PS5 controller support: https://steamcommunity.com/groups/steamworks/announcements/detail/2924487488167580236 [S]
- SDL3 SDL_SetGamepadSensorEnabled: https://wiki.libsdl.org/SDL3/SDL_SetGamepadSensorEnabled [S]
- Godot PR using SDL3 joypad features (triggers, sensors): https://github.com/godotengine/godot/pull/107967 [S]

Procedural variation
- DeBroglie, WFC with constraints (MIT): https://github.com/BorisTheBrave/DeBroglie [S]
- DeBroglie licence file: https://github.com/BorisTheBrave/DeBroglie/blob/master/LICENSE.txt [S]
- Edgar for Unity (graph-based room layouts, 2D tilemaps): https://github.com/OndrejNepozitek/Edgar-Unity [S]
- Edgar-DotNet (the engine-free core): https://github.com/OndrejNepozitek/Edgar-DotNet [S]
- How Spelunky generation works: https://shanemartin2797blog.wordpress.com/2015/11/20/how-spelunky-random-generation-works/ [S]
- Slay the Spire Daily Climb: https://slaythespire-archive.fandom.com/wiki/Daily_Climb [S]
- Daily Climb launch write-up: https://intothespine.com/2018/03/05/slay-the-spire-daily-climb/ [S]

## 1. Deterministic replay: kill cams, last 10 s, ghosts, challenge codes

How the reference games do it:
- **Doom**: a demo is the tic commands only, 4 bytes per tic at 35 tics/s, about 140 bytes/s. Seeded randomness keeps
  playback identical. https://doomwiki.org/wiki/Demo [S]
- **Quake**: a DEM records the server-to-client packets, not inputs. Playback needs no determinism, but the file is
  bigger. https://www.gamers.org/dEngine/quake/Qdem/dem-1.0.2-2.html [S]
- **Trackmania**: deterministic physics; the replay stores the inputs, and a server re-simulates them to validate a
  record. https://github.com/BigBang1112/revalidate [S]
- **Neon White**: ghosts store timestamped frames (position, camera angle, action states), not inputs. So they can't be
  re-run or validated. https://github.com/PandorasFox/neon-white-ghost-parser [S]
- **Overwatch**: the kill cam was built alongside the netcode, and the client was refactored so a kill cam can be
  interrupted. https://gdcvault.com/browse/gdc-17/play/1024053 [S]

| repo / article | licence | what it gives | Unity effort | risk |
|---|---|---|---|---|
| Doom LMP format (doomwiki, chocolate-doom wiki) | doc | Proof that per-step inputs plus a seeded RNG are a complete replay; the size budget | [I] none for code. SKYGRIND's tape is already this. Add a header: build hash, config hash, seed, modifiers. | [I] A tape is only valid for the exact build and config. Version every code and reject mismatches. |
| Trackmania revalidate | not checked | Server-side re-simulation of input replays to validate records | [I] SimHarness can already replay headless. A `validate` verb that re-runs a tape and compares the final fingerprint is small. | [I] Any non-determinism (float paths, iteration order) shows up as false "invalid". |
| Neon White .phant parser | not checked | The opposite design: ghosts as position samples | [I] Cheapest ghost: sample the player's pose every N steps while the best round plays, and draw a translucent rig on the samples. No second sim. | [I] Can't validate. Can't show the enemies. Fine for a "best round" ghost. |
| Overwatch replay talk (GDC 2017) | talk | Kill cam, highlights, replays for bug repro; the client has to be split so a kill cam is interruptible | [I] Kill cam / last 10 s: keep a ring of full sim snapshots (one every 1 to 2 s), restore the newest one before T-10 s into a second `Sim`, and step it with the tape while the view renders it. The RunRestore tests suggest state already round-trips (not checked). | [I] Cost is one more sim at up to 120 steps per frame for fast-forward. Views must read the replay sim, not the live one. Hitstop and render interpolation must come from the replay sim too. |
| Gaffer on Games, deterministic lockstep | article | Why input-only sync works and where it fails (floating point across machines and compilers) | [I] Read before shipping shareable tapes across machines. | [I] Same exe on the same CPU family is likely fine; a cross-platform (Switch) tape is an open question. |
| Moments (Chman) | Zlib [S] | Keeps the last N seconds of frames in a ring buffer and dumps a GIF on demand | [I] Drop-in on the camera for "save clip". A better fit is to re-render the clip from the tape at any resolution, using the feel-metrics frame capture. | [S] Tested on Unity 4.6 / 5. Old; may need URP fixes. [I] GIF encoding is CPU-heavy; run it off the main thread. |

Challenge codes ([I], using sources above): a code = base32 of {format version, build hash, seed, modifier ids}. That is
enough for "play this seed". Attach the tape (Doom-sized, about 4 to 10 bytes per step before compression) only when the
code needs to prove a score. Slay the Spire's Daily Climb (one seed and 3 modifiers for everyone each day) is the design
template: https://slaythespire-archive.fandom.com/wiki/Daily_Climb [S]

## 2. Movement tech

| repo / article | licence | what it gives | Unity effort | risk |
|---|---|---|---|---|
| Olezen/UnitySourceMovement (from Fragsurf) | MIT [F] | Source-style strafe jumping, surf, bhop, crouch-jump, optional slide | [I] Port the accelerate / air-accelerate / friction functions into Core as new mover states, with numbers in a Unity-side config. Its collision layer is not used. | [I] The licence is clean. Rule 3: Mover.cs changes only when a task names it, and every golden vector and the 16 replays must stay bit-exact, so put new states behind a flag that is off on the replay path. |
| MirceaPrunaru/Fragsurf-Character-Controller | MIT [S] | Same family: CS bhop / surf in Unity | [I] Reference only | [I] Duplicates Olezen. |
| mirogon/quake3-movement-for-unity | Unlicense [S] | Quake III vanilla and CPMA strafe jumping (the Q3PlayerController) | [I] CPMA air control is about 30 lines of maths to port | [I] Unlicense isn't on the list but is public-domain-like. Check that the fork's LICENSE matches the original's. |
| TheAsuro/VelocityMovement | MIT [S] | Minimal bhop | [I] Reference | [I] Small, probably unmaintained (not checked). |
| ZacharyBuffone/UnityGrapplingHookComponent and others | MIT [S] | Force-based grapple | [I] Don't use the code; it's Rigidbody forces. A Core grapple is one ray against TriangleWorld for the anchor, then a rope constraint each step: when the distance exceeds the rope length, remove the outward radial velocity (a pendulum). | [I] Grapple changes every map's routes and the bots' pathing. Enemies must still reach every floor (GG's note). |
| yasirkula/UnityBezierSolution | MIT [S] | Bezier splines with constant-speed travel | [I] Rail grind: author rails as polylines in the map data, so the grind stays in Core. Use the constant-speed parameterisation maths only. | [I] Rails are new map data, and the maps come from the web export; a Unity-side map layer is needed. |
| Unity-Spline-Movement | CC0-1.0 [S] | Move objects along splines | [I] Same as above | [S] Made with Unity 2019. |
| Titanfall controls interview / movement analysis | articles | Wall-run design: speed grows with time on the wall, the view tilts away from the wall, levels teach it early | [I] Wall-run in Core: look for a near-vertical TriangleWorld normal beside the player while airborne; clamp the vertical speed and add speed along the wall; time-limit it in whole steps. | [I] The most mechanically risky item: wall detection on arbitrary triangle soup, and its interaction with the existing air dash. |

Air-dash chains and slide-hop: no permissive Unity repo found in the time box (not checked further). [I] Both are small
state machines in Core with step-counted windows (`stepsFor`).

## 3. Destruction that stays deterministic

The pattern ([I]): **gameplay destruction is discrete and in Core; the visual debris is not.** A breakable cover piece is
a box in a box layer with HP. When it reaches zero at step N, the sim removes it from CoreGrid / the box layer at step N
(deterministic, and on the tape). The view then spawns OpenFracture / EzySlice fragments with PhysX, purely cosmetic, so
they never touch the sim.

| repo | licence | what it gives | Unity effort | risk |
|---|---|---|---|---|
| dgreenheck/OpenFracture | MIT [S] | Runtime and pre-baked fracture / slicing, convex and non-convex meshes, meshes with holes | [I] Pre-fracture each cover type in the editor; on break, swap in the fragments with Rigidbodies on a debris-only layer. | [I] PhysX debris differs run to run. Fine while it's cosmetic: a replay's debris won't match, so kill cams show different rubble. |
| DavidArayan/ezy-slice | MIT [S] | Plane slicing of convex meshes, with UV / normal interpolation | [I] Cheap "cut in half" feedback for a heavy weapon hitting cover | [I] Convex only [S]. Allocation on the slice frame. |
| Kugelhaufen/VoxelEngine | not checked | Voxel destruction on Jobs and Burst | [I] Too heavy for the sim. A Core voxel grid would need its own ray / collide path. | [I] Burst float maths is not bit-exact with the C# web collider. Keep it out of Core. |
| qhdwight/voxelfield | not checked | A whole shooter with destructible terrain, server-authoritative | [I] Read-only reference | [I] Not deterministic by design (server-authoritative). |
| Teardown | proprietary | Rule: a chunk stays fully supported until it is cut off from the ground | [I] A cheap support rule for a few stacked boxes | [I] Design call for GG. |

## 4. Photo mode and replay camera in URP

| repo | licence | what it gives | Unity effort | risk |
|---|---|---|---|---|
| Photo Session (Unity thread) | not specified [F]; became an Asset Store product in July 2022 [F] | Pause, WASD free camera, auto DOF for URP / HDRP / built-in, up to 16K capture, 360 capture | [I] Use as a feature reference, not as code | [F] No licence stated on the thread; the GitHub URL isn't given there. |
| DjoleDzele/URP-Autofocus-DOF-Script | not checked | Raycast autofocus that drives URP's Depth of Field | [I] About 30 lines: ray from the camera, set the Volume DOF focus distance. Use a Core ray, not Physics.Raycast. | [S] Tested on Unity 2020.1; URP volume APIs have moved since. |
| Cinemachine | Unity Companion License [S], not on the permissive list | Dolly / orbit cameras for replays | [I] Optional; a hand-written orbit camera on the replay sim is enough | [I] Adds a package; keep it out of Core. |

[I] SKYGRIND already has the hard part: the sim can stop (hitstop) and render is interpolated. Photo mode = freeze the
sim, detach a free camera (UnityTuning numbers for speed and FOV range), hide the HUD, expose URP Volume DOF / exposure /
the dusk grade, and save with ScreenCapture. A replay camera is the same camera on the kill-cam sim from section 1.

## 5. Steam Deck and gamepad: gyro, flick stick, triggers, haptics

| repo / article | licence | what it gives | Unity effort | risk |
|---|---|---|---|---|
| JibbSmart/GamepadMotionHelpers | MIT [S] | Header-only C++ (math.h only): sensor fusion, gyro calibration, gravity; converts to deg/s and g | [I] Port to C# (it's small) or use it as a native plugin. Feed it the raw gyro from whatever reads the pad. | [I] It needs raw gyro and accel at the pad's rate; Unity's Input System doesn't give DualSense gyro natively (UniSense does, see below). |
| GyroWiki: gyro is a mouse, flick stick, player space | articles | The design and maths: gyro as mouse-like deltas, real-world calibration, player-space gyro as the default, flick stick (point the stick and the view snaps to that direction, then rotates with the stick) | [I] Flick stick is pure input maths on the right stick: no new hardware, works on every pad, Deck included. Write the look deltas into the tape like mouse deltas, so replays stay exact. | [I] Needs a settings row and a tutorial line; flick stick replaces normal right-stick look while it's on. |
| JibbSmart/JoyShockLibrary | MIT [S] | DS4, DualSense, Joy-Con, Switch Pro on PC, with gyro (accumulated gyro, gyro space options) | [I] Native DLL plus P/Invoke | [S] C++, Windows build. [I] One more native DLL in the Player; the last release date was not checked. |
| nullkal/UniSense | MIT [S] | DualSense on the Unity Input System: adaptive triggers, haptics, gyro, accel | [I] Package import; set a trigger resistance per weapon (THUNDERCLAP heavy, others light) | [S] Archived June 2025 and described as incomplete. Treat it as a fork-and-own. |
| Unity Input System DualSenseGamepadHID | Unity Companion License (not checked for this package) | SetMotorSpeeds (low / high), SetLightBarColor | [I] Rumble on hits / kills / hitstop today, with no new dependency | [I] Rumble only; no adaptive triggers or gyro here. |
| Steamworks.NET + ISteamInput | MIT [S] (wrapper); Steamworks SDK terms for Valve's SDK | Steam Input: gyro as an input source, TriggerVibration, SetDualSenseTriggerEffect | [I] The Steam Deck's gyro reaches games through Steam Input (unverified that the Input System can't see it). Wire it when Steam integration starts. | [I] Steam-only; a Switch build needs its own path. Valve SDK terms apply beside the MIT wrapper. |
| SDL3 gamepad API | Zlib (SDL's licence; not opened this session) | SDL_SetGamepadSensorEnabled for gyro / accel; DualSense adaptive triggers and Xbox trigger rumble per the Godot PR | [I] Needs a C# binding (none checked for Unity) and a native SDL3 DLL | [I] A second input stack beside Unity's; device ownership fights are likely. |

[I] Recommended order: flick stick (no dependency), then rumble via the Input System, then gyro via Steam Input for the
Deck, then adaptive triggers.

## 6. Procedural variation for round-based maps

| repo / article | licence | what it gives | Unity effort | risk |
|---|---|---|---|---|
| Slay the Spire Daily Climb | design | One seed for everyone per day, a preset loadout, exactly 3 modifiers that are trade-offs, not only punishments | [I] A modifier table in a Unity-side config; Mulberry32 picks 3 from the day's seed; each modifier is a hook in Core's systems (spawn mix, gravity, dash count, enemy speed). | [I] Every modifier is a new gameplay number (rule 5: config file, named in the commit). Each one needs a fingerprint test. |
| Spelunky room templates (explainer) | article | A grid of rooms, a guaranteed solution path, templates per exit type, variation inside a room that keeps the exits | [I] For arenas: pick, per round, which prefab "rooms" / cover clusters fill fixed sockets in the map, seeded per run | [I] Maps come from the web export; sockets are new map data. |
| BorisTheBrave/DeBroglie | MIT [S] | WFC with backtracking and non-local constraints (path connectivity), on 2D, hex and 3D voxel grids | [I] Pure C#: could run in Core with a fixed seed if its picker uses an injected RNG (not checked) | [I] Must not call System.Random (rule 6). Verify its RNG path before using it in the sim; or run it offline to bake variants. |
| Edgar for Unity / Edgar-DotNet | not checked (Unity: free and PRO tiers [S]) | Graph-based layouts from hand-made room templates | [I] Edgar-Unity targets 2D tilemaps [S]; Edgar-DotNet is the engine-free core | [I] A poor fit for a 3D arena shooter; an offline layout tool at most. |

[I] The cheapest variation that is clearly visible: seeded per-round events ("low gravity round", "only BULWARKs",
"double hitstop", "the boundary shrinks") drawn from a table, shown on the round banner. They reuse existing systems, so
there is no new art.

## Ranked top 8 (cheap and notable)

Ranking is [I]: my judgement of value against cost for this codebase, from the sources above. Nothing was prototyped.

1. **Instant replay / kill cam of the last 10 s**, from the tape plus a snapshot ring buffer. Technique: Doom demos
   https://doomwiki.org/wiki/Demo and the Overwatch replay talk https://gdcvault.com/browse/gdc-17/play/1024053. No
   dependency. Risk: a second sim instance, and views must bind to it.
2. **Daily seed with 3 modifiers + shareable challenge codes**, from the Slay the Spire Daily Climb
   https://slaythespire-archive.fandom.com/wiki/Daily_Climb. Pure Core with Mulberry32. Risk: tapes are tied to the
   build, so codes carry a build hash.
3. **Flick stick + gyro aim**, from GyroWiki https://gyrowiki.jibbsmart.com/blog:good-gyro-controls-part-2:the-flick-stick
   and GamepadMotionHelpers (MIT) https://github.com/JibbSmart/GamepadMotionHelpers. Flick stick costs no dependency.
   Risk: gyro input on the Deck needs Steam Input (Steamworks.NET, MIT) https://github.com/rlabrecque/Steamworks.NET.
4. **Ghost of your best round** as position samples (the Neon White model)
   https://github.com/PandorasFox/neon-white-ghost-parser, with tape-validated scores later (the Trackmania model)
   https://github.com/BigBang1112/revalidate. Risk: position ghosts can't be validated.
5. **Breakable cover**: discrete box removal in Core plus cosmetic OpenFracture debris (MIT)
   https://github.com/dgreenheck/OpenFracture. Risk: PhysX debris differs between a run and its replay.
6. **Photo mode**: freeze the sim, free camera, URP DOF. Reference: Photo Session
   https://discussions.unity.com/threads/free-photo-session-ingame-precision-screenshot-utility-open-source-github.1076252/
   (no licence stated, so own code). Risk: low.
7. **Rumble + DualSense adaptive triggers per weapon**: Input System SetMotorSpeeds
   https://docs.unity3d.com/Packages/com.unity.inputsystem@1.8/api/UnityEngine.InputSystem.DualShock.DualSenseGamepadHID.html,
   then UniSense (MIT, archived) https://github.com/nullkal/UniSense. Risk: UniSense is unmaintained.
8. **One new movement verb (rail grind or wall-run) in Core**, with the maths from UnitySourceMovement (MIT)
   https://github.com/Olezen/UnitySourceMovement and UnityBezierSolution (MIT) https://github.com/yasirkula/UnityBezierSolution.
   Ranked last despite being the most notable: rule 3 (bit-exact replays), new map data, and enemy reachability make it
   the costliest.

Also worth a look: "save the last 10 s as a GIF" via Moments (Zlib) https://github.com/Chman/Moments. Cheap, but old
(Unity 4.6 / 5 era); re-rendering from the tape is the better long-term path.
