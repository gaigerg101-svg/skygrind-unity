# Scout: what makes ULTRAKILL movement and camera feel the way they do (2026-10-27, web only)

Tags: P = official wiki quoting patch notes / developer. C = community measurement or a mod author's port of the decompiled game
(unverified by us). G = guess, inference, or my own arithmetic on P/C numbers.
Short names: WIKI = https://ultrakill.wiki.gg ; V1MOD = https://github.com/Rand0mNme/V1-Ultrakill-v01-/blob/9378297761dcb989bcac2f2aaf6d4358b1c87227
Not opened (403/402/429): speedrun.com, fandom, most Steam guides, PCGamingWiki; their claims are search-snippet only.

## Ranking: what would change a copy that already has the numbers

1. The live game moved after the numbers were probably read (P). Patch 17 (2026-02-25): "Increased the strength of air strafing at high
   speeds" (undocumented line). Patch 17c (2026-04-12): "Air strafing is stronger at all speeds", "Added Wind state tech that allows for
   greater air control", "Made SSJ framerate independent and prioritized over dash jumps", "Fixed jump timing issues with slam and slam
   storage". Patch 17: "Timing of slam jump and slam storage jump is significantly stricter".
   https://ultrakill.wiki.gg/wiki/Development/Changelogs
   Wind State = a timed air-control boost (higher turn speed, tighter radius) for 0.5 s after a slide wall jump, a dash/dive enemy step or a
   dash extension, 0.75 s after a non-damaging rocket jump (P). https://ultrakill.wiki.gg/wiki/Movement (section Wind State)
   Our "0.48 u/s per tick per axis, cap 16.5" is from the V1 mod doc, whose game version is not stated (G: it may predate patch 17). Not checked.
2. Camera numbers exist, from one source (C). V1MOD V1Camera.cs says it ports ULTRAKILL's CameraController:
   - FOV 105. Dash/slide forward: FOV x(1 - 1/20) = 99.75. Dash/slide backward: FOV x(1 + 1/10) = 115.5. Returns at 300 deg/s (MoveTowards).
   - Strafe roll: target = -strafe input (so +-1 deg), x5 while dashing/sliding (+-5 deg). Rate 25 deg/s x (|error|+0.01) normal, 100 deg/s x
     (|error|+0.01) boosted; plus SmoothDampAngle back to 0 with 0.5 s smooth time. Applied as roll about forward.
   - Slide camera drop 0.625 u, eased: MoveTowards at (distance + 0.5) x 10 per s. Walk bob: dips 0.1 u below rest and returns, 0.5 u/s,
     vertical rate scaled by min(speed,15)/15, only grounded and moving. Zoom = FOV/2.
   - No landing dip, no shake, no recoil, no speed-based FOV in that port. Eye 2.9 u above the feet (capsule 0.5 x 3.5); the file's header
     says "1.4 m above centre", which would be 3.15; the code uses 2.9. Treat 2.9 as the value.
   https://github.com/Rand0mNme/V1-Ultrakill-v01-/blob/9378297761dcb989bcac2f2aaf6d4358b1c87227/V1Mod/Survivors/V1/V1Camera.cs
   Scaled x0.5143 (lengths only; angles and FOV are not scaled): eye 1.49 m, slide drop 0.32 m, bob dip 0.051 m, capsule r 0.257 / h 1.80 m. (G)
3. Walking has almost no inertia; speed is kept only by states (G, my arithmetic on C numbers). Ground velocity moves 25% of the gap to the
   target per 0.008 s tick (V1MOD docs/ULTRAKILL_values.md, C). That is exp(-36 t): half-life 19 ms, 95% gone in 83 ms. A landed hop at 30 u/s
   is back to walk speed in about a tenth of a second. So momentum comes from slide (24 u/s x preSlideSpeed 1-3, decays to 24 by friction;
   cap 72, 77 with Slideways, P), slide jump, dash jump and slam storage, not from bunny hopping.
   https://github.com/Rand0mNme/V1-Ultrakill-v01-/blob/9378297761dcb989bcac2f2aaf6d4358b1c87227/docs/ULTRAKILL_values.md
   https://ultrakill.wiki.gg/wiki/Movement (section Sliding)
4. Air control has an over-speed rule your summary omits (C). V1MOD V1CharacterMain.cs: gain per step = airAcceleration x dt / mass
   (6000 x 0.008 / 100 = 0.48 per tick = 60 u/s^2), projected on input and the aim-relative forward and right axes, clamped so each axis
   does not exceed 16.5. When already faster than the cap, turning into a new direction is scaled by 0.4, then limited by a cubic falloff.
   No input adds nothing (momentum carries). Fall speed clamp 100. Reaching 16.5 from rest in air takes 0.275 s (G).
   https://raw.githubusercontent.com/Rand0mNme/V1-Ultrakill-v01-/9378297761dcb989bcac2f2aaf6d4358b1c87227/V1Mod/Survivors/V1/SkillStates/V1CharacterMain.cs
   The wiki's "0.48 u/s of horizontal velocity" while slamming is the same number; it is per tick, not per second (G).
   https://ultrakill.wiki.gg/wiki/Movement (section Slamming)
5. Slam-bounce numbers conflict (G, my arithmetic). V1MOD: jump k = 3 + (slamForce - 1), 12.5 when slamForce >= 5.5, slamForce grows 5/s,
   slam speed 100 u/s, v = 10.8 k. A 30 u slam gives k 4.5, v 48.6, apex 29.5 u: matches the wiki "ends slightly above the slam's start,
   default limit about 30 units (6 Strays)". A 0 u slam gives k 3, apex 13.1 u, 1.33x a normal jump: matches "slightly above a normal
   jump". A 90 u slam gives k 7.5, apex 82 u: matches "slightly lower than the start". But k 12.5 gives v 135, apex 228 u, which
   contradicts "long slams default to a maximum bounce cap about 30 units". Resolve before copying the cap.
   https://ultrakill.wiki.gg/wiki/Movement (section Slam Bounce) ; V1MOD docs/ULTRAKILL_values.md
6. Dash end and dash cancels (C/P). Dash = 25 ticks (0.2 s), horizontal only, 3 x walk = 49.5 u/s; at a normal end speed returns to walk
   speed in that direction and vertical speed is 0 (V1MOD Dash.cs, C). Dash resets momentum (P), can cancel a slam (P), costs 1 stamina of 3,
   dash jump 2 (1 with enemy step) (P). A slide started during a dash keeps i-frames up to 1 s ("dash storage", consistent 1 s since patch 13,
   P). Slide cancel and restart is instant (P). https://ultrakill.wiki.gg/wiki/Movement (Dashing, Sliding) ; changelog patch 13.
   https://github.com/Rand0mNme/V1-Ultrakill-v01-/blob/9378297761dcb989bcac2f2aaf6d4358b1c87227/V1Mod/Survivors/V1/SkillStates/Dash.cs
7. No evidence of coyote time or jump buffer in the base game (see Q3); it uses short strict windows instead (SSJ 3 frames, slam delay
   after a grounded jump, 0.1 s power-slam hold), so generous forgiveness would be a departure (G).

## 1. Independent checks of jump height and air time
- No independent measurement found (no frame-count video, speedrun note, or community figure for apex or air time). Searches of the wiki
  (api search "jump height"), the speedrun forum, Steam and GitHub returned none. Nothing contradicts 9.86 u / 0.70 s; nothing confirms it.
- Internal checks only (G): 90 x 1500 x 2.6 x 0.008 / 100 = 28.08 u/s; 28.08^2 / 80 = 9.857 u; 28.08 / 40 = 0.702 s up, 1.404 s flat total.
  The slam-bounce cross-check above is the only outside agreement (wiki prose vs the same formula).
- Caveats that could move the number: the derivation assumes one fixed step of AddForce, no Rigidbody drag, no extra gravity (drag and extra
  gravity: not checked). Patch 5 (2020-12-19) "Standard jump height increased very slightly"; patch 12 fixed a bug where jump, slide, unslide
  near the ground multiplied jump height; patch 17b (2026-03-03) "Fixed the player being able to jump twice in quick succession to perform a
  mid-air jump". All P: https://ultrakill.wiki.gg/wiki/Development/Changelogs . Footage older than Dec 2020 will read lower.
- The jump is "a fixed, unchanging rate" (no hold-for-height) (P). Jump cooldown 0.2 s, jumping flag 0.25 s, wall jump cooldown 0.1 s (C).
  https://ultrakill.wiki.gg/wiki/Movement (section Jumping) ; V1MOD CharacterMain/MovementData
- Derived (G): slide jump apex 5.83 u, dash jump 3.28 u. SSJ boosts +24.75, +12.375, +6.1875 u/s (frames 1-3; window marked "[?]"),
  SSJ speed capped at 100 u/s since patch 15 (P). https://ultrakill.wiki.gg/wiki/SSJ

## 2. Camera feel
- Official options (P): FOV default 105, range 45-160. Camera Tilt toggle, on by default: "very slightly tilted in the direction in
  which they are currently moving". Screenshake slider 0-200% (default 100%) scales camera displacement from heavy attacks, weapons and
  events. "Reduce HUD Motion" stops HUD and held weapons shifting with the player's velocity (so weapon sway follows velocity).
  https://ultrakill.wiki.gg/wiki/Options
- Patch notes (P) https://ultrakill.wiki.gg/wiki/Development/Changelogs : v1.01 added the tilt and shake options; patch 10 "Walking slower
  will now also make view bob, gun bob and footsteps slower" (so view bob exists and scales with walk speed); patch 15c "Fixed temporary FOV
  changes getting stuck while sliding sideways" and "Camera can be rotated during a hitstop"; patch 11b "Camera no longer clips through the
  world when shaking"; patch 5c "screenshake offsetting the camera" (shake is a positional offset). Hitstop exists (UltraSmooth mod removes
  "all hitstops and slowdowns", C): https://github.com/Glebux/UltraSmooth
- Numbers: only the V1MOD set in "Ranking 2" (C). Landing dip, jump kick, view-punch: none found in any source; the wiki Options page has no
  head-bob option. Not checked: the real CameraController (no legal decompile to read; a repo "teker821/ULTRAKILL" appears in GitHub
  search as "Literally just ULTRAKILL"; I did not open it and it should not be used as a source).
- Clone guesses, contrast only (G): heavent slide roll 4 deg, dash roll 2.5, FOV 88 -> 96 by speed, slide drop 0.65:
  https://raw.githubusercontent.com/miltivik/heavent/HEAD/scripts/player/player_camera.gd

## 3. What players say makes it feel good
- Slide jump keeps momentum, especially chained after a dash jump (P): https://ultrakill.wiki.gg/wiki/Movement ; Steam thread "Why do you need
  a slide?": "Slide jumps will also preserve all momentum you previously had, especially after a dash-jump" (G, forum)
  https://steamcommunity.com/app/1229490/discussions/0/4032476115581015101/
- Speedrun forum (snippet only, C): slide-hop speed "around 70", cap about 3x slide speed (matches the wiki's 72); "since the new patch,
  you can conserve momentum by doing more slide jumps when you hit the ground in a small window". https://www.speedrun.com/ultrakill/forums/w3doc
- Slam storage (P): slide against a wall, slam, wall-jump off quickly (or slam onto a hookpoint while whiplashing); jumping right after
  landing gives a very high jump; can chain into a slide jump or dive. No numbers on the page. The V1 doc has a 0.1 s super-jump window then
  0.306 s extra (C). https://ultrakill.wiki.gg/wiki/Movement ; docs/ULTRAKILL_values.md. Prelude 1.04b: "you can jump immediately after a
  ground slam to use the momentum for a very high jump" (P, changelog).
- Slam bounce: chainable, limit about 30 u by default; wall jumps limited to 3 per airtime (reset by ground or enemy step); wall cling weight
  grows until fall speed reaches 100 u/s, a new cling overrides current fall speed (P). https://ultrakill.wiki.gg/wiki/Movement
- Dash extension (dash off a ledge, slide as you drop) keeps dash speed in the air and starts wind state 0.5 s (P, same Movement page).
- Slide: 24 u/s at once; slideways +5 up to 29; airborne slides keep state and speed (patch 10); slight strafe (patch 7); climbstepping over
  small steps (patch 5, no height given) (P).
- Coyote time and jump buffer: the wiki Movement, SSJ and Stamina pages never mention either (P by absence). The V1 port implements neither
  (C: V1CharacterMain.cs says grounding is instantaneous). The UltraStrafe mod ADDS a JumpBuffer (G: suggests the base lacks one):
  https://github.com/10-days-till-xmas/UltraStrafe . A fandom Movement Guide snippet said falling off a ledge gives a jump window it calls
  coyote time; I could not open the page and the wiki does not say it (G): https://ultrakill.fandom.com/wiki/Movement_Guide
- Horizontal speed on landing (C/G): no landing logic in the port; ground lerp resumes once grounded and the 0.25 s jumping flag expires, so
  speed decays at once unless a slide starts. UltraStrafe adds "frictionless frames" after air moves, so the base applies friction on landing:
  https://raw.githubusercontent.com/10-days-till-xmas/UltraStrafe/HEAD/UltraStrafe/QuakeMovement.cs

## 4. Open-source recreations (none claims frame-exact; none Unity with real constants)
- Rand0mNme V1 mod (RoR2 mod, C#): the only one with real ULTRAKILL constants, read from the game's DLL; licence not seen; source for the
  numbers above. https://github.com/Rand0mNme/V1-Ultrakill-v01-
- tyler-knoop/ULTRAKILL-Movement-Prototype: Unity, archived 2023, 3 commits, no licence, no accuracy claim; values live in the Inspector (not
  in the repo text). https://github.com/tyler-knoop/ULTRAKILL-Movement-Prototype
- MasterCheap9447/VORTEX: Godot, MIT, "inspired by"; walk 24, jump 24, gravity 24 up / 36 down, air cap 1.9, dash 50 for 0.1 s, slide max 32,
  slide roll 1 deg, FOV 90 + speed, wall jumps unlimited, slam not written, no coyote or buffer. Not ULTRAKILL numbers (G).
  https://raw.githubusercontent.com/MasterCheap9447/VORTEX/HEAD/assets/scripts/PLAYER/player.gd
- miltivik/heavent: Godot 4, MIT, "inspired by"; jump 10, gravity 30 up / 55 down, dash 24 for 0.15 s, slide 14, slam -60, wall jump
  (6, 9), 3 wall jumps, coyote 6 frames, buffer 6 frames, air accel 18 (Quake projection). Adds coyote and buffer the game lacks (G).
  https://raw.githubusercontent.com/miltivik/heavent/HEAD/scripts/player/player_controller.gd
- Enkrypt512/Cool-Movement-Game: Godot, GPL-3.0, not ULTRAKILL-derived (double jump). Skip. https://github.com/Enkrypt512/Cool-Movement-Game
- Mods that patch the real game (show what it lacks): MovementPlus (no licence) https://github.com/wafflethings/MovementPlus ; UltraStrafe
  (no licence); Source-movement-ultrakill-bhop- (MIT) https://github.com/lazurut/Source-movement-ultrakill-bhop-
- Ports with no constants: ultrakill-minecraft (MIT) https://github.com/xandermodsstuff-dot/ultrakill-minecraft ; Ultramove for Garry's Mod
  (no licence) https://github.com/SpitSwapula/Ultramove ; Ultramove datapack (CC-BY-4.0) https://modrinth.com/mod/ultramove

## 5. Developer talks and interviews (thin on movement)
- Hakita (80.lv): "The main focus was just on the game feel."; "Going fast is fun, so that wasn't a worry."; "constant forward momentum with only
  occasional downtime"; "the player has a lot of movement tools to easily outmaneuver anything" so enemies must threaten by encounter size;
  linear levels keep the pace. https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/ (P)
- 2020 interview: dash buffering / infinite i-frames in a slide "seemed at first like it would break the game" but close-quarters levels
  limit long slides; bugs that help the player are kept if fun. https://intothebluesky.com/2020/12/19/interview-with-arsi-hakita-patala/ (P)
- 2021 follow-up: level 4-2 platforms were placed "to make it easy to slidehop across most of the first half"; wants "constant forward
  momentum"; prefers committed moves over cancel-anything. https://intothebluesky.com/2021/10/05/follow-up-interview-with-hakita/ (P)
- No talk or documentary on camera, FOV or movement constants found; Hakita names no Quake intent. Film Stories: "What if DMC was an FPS?"
  https://filmstories.co.uk/gaming/ultrakill-improvisational-and-impulsive/

## 6. Differences from a Quake / Source model that matter for a copy
- Ground: lerp-to-target (25% per 8 ms tick, about 36/s), not friction plus acceleration. Quake and Source use friction 4-6/s with a
  stop-speed floor (from memory, not fetched; G). So no speed is kept by hopping; slide, dash and slam states carry it (see Ranking 3).
- Air: per-axis cap 16.5 with 60 u/s^2, over-speed turn penalty 0.4 + cubic falloff (C); no speed gain from strafe-jumping in the base
  design. Quake-style airstrafing exists only as a mod: https://github.com/10-days-till-xmas/UltraStrafe (README: equations from Xonotic
  DarkPlaces; its tweak raises accel by log2(speed - 8.5) - 2, clamped 1-10; no licence). Patch 17 strengthened real air strafing (P).
- States set velocity, they do not add impulses: dash, slide, slam (0,-100,0); wall jump zeroes velocity first (C).
- Rigidbody-style units: forces divided by mass 100, fixed step 0.008 s (125 Hz). Keep the sim step near this or scale per-tick numbers (C).
  Your 120 Hz loop differs by 4%; per-tick values (0.48, 25%) must be converted per second.
- Frame windows (SSJ 3 frames, slam delay) were retuned twice in 2026 (P); at 120 Hz count steps, not frames.

## Not checked
Rigidbody drag and extra gravity in NewMovement; which game version the V1 doc was read from; the real strafe-tilt and FOV code; slide
collider height; step-up height; whether holding jump auto-repeats; any frame-count video of jump arc; speedrun.com, fandom and Steam pages.
