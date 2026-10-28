# ULTRAKILL player movement numbers (web research, 2026-10-26)

Units: ULTRAKILL "u" = 1 Unity unit. V1's capsule is 3.5 u tall; SKYGRIND's body is 1.8 m. "Scaled" = x0.5143 (1.8 / 3.5) on lengths and speeds, times unchanged (the same scaling the V1 mod uses for its "0.52" mode).

Confidence codes: **P** published on the official wiki. **D** read from the game's decompiled NewMovement.cs by a third party (the V1 mod repo; we cannot see the decompile). **+W** the number also appears on the wiki. **C** our arithmetic from D. **N** not found.

Provenance warning: every jump, gravity, mass and air number is D. The mod's README says values come from the decompile "not tuned by feel, unless a comment says so", and many D numbers match the wiki exactly (walk, slide, dash, slam, 0.48 air, stamina regen), which supports the author. Jump power, gravity and mass are NOT cross-checked by any second source. No player measurement of jump height exists in what we found.

## Sources

- W = https://ultrakill.wiki.gg/wiki/Movement ; WS = https://ultrakill.wiki.gg/wiki/Stamina ; WJ = https://ultrakill.wiki.gg/wiki/SSJ ; WO = https://ultrakill.wiki.gg/wiki/Options
- M = https://github.com/Rand0mNme/V1-Ultrakill-v01-/blob/main/docs/ULTRAKILL_values.md (Rand0mNme, a Risk of Rain 2 port; sources: Assembly-CSharp NewMovement.cs, globalgamemanagers, AssetRipper prefab export)
- MC = same repo, `V1Mod/Survivors/V1/SkillStates/` V1CharacterMain.cs, Dash.cs, Slide.cs, Slam.cs, V1MovementData.cs; MK = `V1Mod/Survivors/V1/V1Camera.cs`
- SB = https://forums.spacebattles.com/threads/ultrakill-feats-and-capabilities-thread.1242248/ (fan thread, speedometer readings)

## Table (raw u = ULTRAKILL as shipped; scaled = for a 1.8 m body)

| Item | Raw value | Scaled | Source | Conf |
|---|---|---|---|---|
| Physics tick | 0.008 s (125 Hz) | same | M | D |
| Gravity | -40 u/s2, one value; no fall multiplier found | -20.57 m/s2 | M (DynamicsManager) | D; fall mult N |
| Body | Rigidbody mass 100, force driven (no CharacterController), capsule r 0.5 h 3.5 | r 0.257, h 1.8 | M | D |
| Walk / run speed | 16.5 u/s (walkSpeed 750 x 0.008 x 2.75); no sprint | 8.49 m/s | W, WJ, M, SB | P +W |
| Ground accel | velocity moves 25% of the gap to target each tick: 50% in 0.019 s, 95% in 0.083 s (rate 36 /s) | same times | M, MC | D |
| Air accel | airAcceleration 6000 / mass 100 x 0.008 = 0.48 u/s per tick per axis = 60 u/s2; per-axis cap 16.5 u/s, speed above the cap is kept | 30.9 m/s2, cap 8.49 | M, MC; W (0.48 u/s while slamming) | D +W |
| Normal jump | AddForce(up x 90 x 1500 x 2.6) on mass 100 in one tick: v0 28.08 u/s, apex 9.86 u, 0.702 s to apex, 1.40 s air | 14.44 m/s, 5.07 m, 0.702 s | M, C | D / C |
| Jump fixed | "Jumping propels the player into the air at a fixed, unchanging rate" (no variable height) | | W | P |
| Slide jump | factor 2.0: v0 21.6, apex 5.83 u, 0.54 s; keeps slide speed | 11.1 m/s, 3.00 m | M, C | D / C |
| Dash jump | factor 1.5: v0 16.2, apex 3.28 u, 0.405 s; costs 2 pips (1 with an enemy step) | 8.33 m/s, 1.69 m | M, C; WS | D / C; cost P |
| Jump timers | cooldown 0.2 s; "jumping" flag 0.25 s | same | M, MC | D |
| Terminal fall | 100 u/s (reached after 2.5 s, 125 u) | 51.4 m/s | W, SB, M | P +W |
| Double jump | none. Wiki lists jumps from ground or wall only; 3 wall jumps per airtime, refilled by ground or an enemy step | | W, WJ | P |
| Wall jump | velocity zeroed, then 24 u/s away + 24 u/s up (apex 7.2 u, 0.6 s); cooldown 0.1 s; count 3 | 12.34 + 12.34 m/s | M, MC; count W | D; count P |
| Wall cling | push into a wall while falling: fall slows, then "weight" grows until 100 u/s; code ramps fall speed 8 u/s per s | | W (qualitative), MC | P / D |
| Dash | 49.5 u/s (3 x walk), horizontal only, 25 ticks = 0.2 s = 9.9 u, invulnerable all dash, resets momentum, ends at walk speed | 25.46 m/s, 5.09 m | W, WJ, M, MC | P +W |
| Stamina | 3 pips, dash 1 pip, regen 0.7 /s Standard (1.05 Lenient, 1.4 Harmless), none while sliding, parry refills all | same | WS, M | P +W |
| Slide | 24 u/s base x preSlideSpeed 1..3 (max 72, 77 with Slideways); decays to 24 on the ground; A/D add 5 u/s; ends below 10 u/s for 0.1 s | 12.34 / 37.0 m/s | W, WJ, M, MC | P +W |
| Super slide jump | adds 24.75 / 12.375 / 6.19 u/s if the jump lands 1 / 2 / 3 frames after the slide ends | | WJ | P |
| Ground slam | 100 u/s straight down, only with ground more than 3 u below; tiny air control (0.48 u/s) | 51.4 m/s | W, WJ, M | P +W |
| Slam bounce | wiki: default limit about 30 u, ends slightly above the slam start. Mod: jump factor 3 + (slamForce - 1), slamForce +5/s from 1, max 12.5; windows 0.1 s + 0.306 s. A 30 u slam gives 4.5 -> 29.5 u | 15.2 m | W, M, C | P (30 u) / D |
| Height, eye | capsule 3.5 u; camera 2.9 u above the feet; slide drops the camera 0.625 u | 1.8 m, eye 1.49 m | M, MK | D |
| FOV | 105 default, range 45-160; dash forward -1/20, backward +1/10; camera tilt on by default | | WO, MK | P |
| Coyote, jump buffer | not found (no mention anywhere) | | | N |
| Rigidbody drag, fall mult | not found | | | N |

Open conflicts: the mod's slam factor reaches 12.5 (228 u) while the wiki says the default bounce limit is about 30 u, so a clamp exists that we did not find. Wiki says "Dash: briefly immune" with no duration; 0.2 s is D only.

## Clones and recreations (which claim to match)

- Rand0mNme V1 mod (RoR2): claims the decompile, lists numbers (the D rows above). Best source. https://github.com/Rand0mNme/V1-Ultrakill-v01-
- SpitSwapula Ultramove (Garry's Mod E2): only "3 wall jumps", "3 dashes of stamina"; no constants in the README. https://github.com/SpitSwapula/Ultramove
- Ultramove (Minecraft datapack): "as closely as possible", but lists differences; block units (jump up to 6 blocks) are not ULTRAKILL units. https://modrinth.com/mod/ultramove
- xandermodsstuff-dot/ultrakill-minecraft: no movement numbers in the README. No Unity clone with published numbers was found.
- Cheat table (FearLess): field names only (walkSpeed, jumpPower, airAcceleration, wallJumpPower, slamForce, fallTime), no values. https://fearlessrevolution.com/viewtopic.php?t=15274

## Recommended profile for a 1:1 feel (scaled to the 1.8 m body; SKYGRIND now in brackets)

Time-based feel is kept exactly; lengths and speeds shrink by 0.5143. SKYGRIND's loop is 120 Hz, ULTRAKILL's 125 Hz, so write per-tick constants as rates (ground lerp t = 1 - 0.75^(dt / 0.008)).

- jump.height 5.07 m [1.5]. The biggest gap: ULTRAKILL's jump is about 2.8 body heights. Commit 168e8f9fd already applies jumpHeightMult 1.5 in Unity (2.25 m effective).
- jump.apexTime 0.702 s [0.36]; implied gravity 2h / t2 = 20.57 m/s2.
- jump.fallGravityMult 1.0 [1.35]; also apexHangMult 1.0, releaseMult 1.0 (fixed-height jump per the wiki). Terminal fall 51.4 m/s.
- jump.doubleJumps 0 [1]. Enemy step is the only mid-air jump (refills wall jumps).
- jump.coyoteTime, bufferTime: N, keep [0.1, 0.1].
- speed: ground.walkSpeed 8.49 [8], one speed, no sprint; ground response reaches 95% in 0.083 s. speed.maxHorizontal stays [40] (slide ceiling is 37.0, boosts go higher).
- air accel: 30.9 m/s2 per axis with a per-axis cap of 8.49 m/s, speed above the cap kept. SKYGRIND `air.accel` 1.5 is a different unit and is not mapped.
- dash.speed 25.46 [24]; dash.duration 0.2 [0.25]; dash.maxPips 3 [3]; dash.pipCost 1 [1]; dash.regenPerSec 0.7 [0.7]. Dash is horizontal only and fully invulnerable; stamina refills on a parry.
- dashJump.v0Mult 0.577 (= 1.5 / 2.6) [0.8]; dashJump.extraPipCost 1 [1].
- slide.floorSpeed 12.34 [11.6]; slide jump keeps slide speed (slide.jumpHorizontalMult 1.0 [1.2], our inference) with vertical v0 = 0.769 of a normal jump (2.0 / 2.6); jumpBoostMaxSpeed 37.0 [24].
- slam.speed 51.43 [48]. Slam bounce cap about 15.2 m (30 u scaled).
- wallJump.maxPerAirtime 3 [3]; outSpeed 12.34 [8]; v0Mult 0.855 (= 24 / 28.08) [0.9]; zero the velocity first.
- body: capsuleHeight 1.8 matches; eyeHeight 1.49 [1.6]; radius 0.257 [0.35]; FOV 105.

Suggested check before trusting jump numbers: count frames of a flat jump in a 60 fps ULTRAKILL clip (expect about 42 frames up, 84 total), or read the speedometer at the dash and wall jump. Not done here.
