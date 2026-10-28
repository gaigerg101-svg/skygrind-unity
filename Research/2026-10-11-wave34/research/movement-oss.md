# Open-source momentum movement: repos, numbers, patterns (wave 34 research)

Web research only, 2026-10-11, about 35 tool calls. Every number below was read from the linked source this session
unless marked "not checked". Licences come from the GitHub API or the repo page.

## Answer

The best copyable sources are Celeste's `Player.cs` (MIT, code only), Olezen's UnitySourceMovement (MIT, its own traces,
no Rigidbody, the cleanest slide maths), Project Borealis' PBCharacterMovement (MIT, HL2 movement in UE C++), the Godot
dot-player-controller (MIT, deterministic, replayable, with coyote / buffer / slide / grapple modes) and the Godot
Quake-wallrun prototype (Apache-2.0, the simplest wallrun). Q3Movement (CPMPlayer.cs) has no licence and is reference
only, as are Momentum Mod (Source 1 SDK licence) and Dave's grappling tutorial (no licence). No open, licensed
Titanfall or ULTRAKILL controller with real numbers turned up; the best Titanfall-style constants are a student
recreation's (UCSD CSE125, no licence found, treat as tuning hints, not Respawn data), and ULTRAKILL's numbers come from
its wiki. The shared recipe is the same everywhere: forgiveness windows of about 0.1 s (coyote, buffer), no ground
friction on a buffered landing jump, a multiplicative slide-entry boost clamped to a band with a cooldown, wallrun
gravity that starts at zero and ramps back, fixed up-plus-away wall-jump impulses with a capped count, and corner
correction / apex hang so near-misses resolve in the player's favour. At 120 Hz, 0.1 s is 12 whole steps.

## Repos

| # | Repo | Licence | Copy? | Engine / lang | Stars / activity | Techniques |
|---|------|---------|-------|---------------|------------------|------------|
| 1 | https://github.com/NoelFB/Celeste (Source/Player/Player.cs) | MIT (README: code only, not the game or assets) | y | Monocle, C# 2D | 4.0k | coyote (JumpGraceTime), variable jump, half gravity at apex, wall jump with force time, super wall jump, dash and upward corner correction, lift boost caps, wall slide |
| 2 | https://github.com/Olezen/UnitySourceMovement | MIT | y | Unity C#, own trace collision (no Rigidbody / CharacterController) | 388, last push 2019-08 | Source bhop / air strafe with airCap, surf, autoBhop flag, crouch slide with entry multiplier, constant slide friction, downhill gain, slide delay, step offset |
| 3 | https://github.com/ProjectBorealis/PBCharacterMovement | MIT | y | Unreal 5.5, C++ | 1.5k, push 2025-06 | HL2 bhop, air strafe, AirSpeedCap, surf / rampslide (speed-scaled step height and floor angle), crouch jump, crouch slide boost, JumpBoost modes (HL2 ABH, patched) |
| 4 | https://github.com/modcommunity/dot-player-controller | MIT | y | Godot, GDScript | 14, push 2026-10-09 | deterministic motor + packed command + state snapshots for replay; coyote, jump buffer, slide (duration, cooldown, boost), launch (cooldown), grapple / ladder / water modes, max_air_wish_speed |
| 5 | https://github.com/JamesSayegh/Godot-Quake-wallrun-prototype (Scenes/Player.gd) | Apache-2.0 | y (keep NOTICE) | Godot, GDScript | 3, push 2021-06 | Q3 ground / air / strafe settings, side-ray wallrun, wallrun gravity, wall jump along normal, camera roll lerp |
| 6 | https://github.com/DaniDevy/FPS_Movement_Rigidbody | MIT | y (but Rigidbody forces; ideas only for us) | Unity C# | 524, push 2022-10 | Karlson-style counter-movement, crouch-entry slide impulse, reduced slide counter-movement, extra gravity, half air control |
| 7 | https://github.com/WiggleWizard/quake3-movement-unity3d (CPMPlayer.cs) | none (API: no licence) | n, reference only | Unity C# / JS | 303 | Q3 / CPM strafe jumping, CPM air control (k = 32), side-strafe accel, holdJumpToBhop |
| 8 | https://github.com/momentum-mod/game | Source 1 SDK Licence (NOASSERTION) | n, reference only | Source, C++ | 644, archived open snapshot | surf, bhop, rocket jump, parkour (Titanfall 2 style, from HL2 Mobility Mod; docs say "not fully working yet") |
| 9 | https://github.com/DaveGameDevelopment/Grappling-Tutorial-GitHub (Assets/Grappling.cs) | none | n, reference only | Unity C# | 44 | grapple as a computed parabolic launch to the hit point plus overshootYAxis; values are Inspector-only |
| 10 | https://cse125.ucsd.edu/2026/cse125g2/docs/TitanfallConstants_8hpp.html | none found | n, reference only | C++ (student game) | n/a | Titanfall-style wallrun (grip, gravity ramp, kickoff), wall jump, slide boost + cooldown, slide hop, lurch, coyote, double jump, grapple range |

Also seen in search listings, not opened: Fragsurf/Fragsurf-2 (licence "Other", not checked),
MirceaPrunaru/Fragsurf-Character-Controller (MIT), 0xspig/3dCharacterController (Godot, GPL-3.0, reference only),
keenanwoodall/Grapple-Effect (Unlicense, rope visual only), unsurf/cs-movement (TypeScript, Apache-2.0),
Kevin-Kwan/Lets-Surf (Unity, MIT).

## Numbers

Units: Celeste px and px/s (8 px tile); Source / Titanfall recreation in u/s (HU); PB in cm (1 HU = 1.905 cm in PB,
57.15 = 30 HU); Unity and Godot repos in m and m/s; ULTRAKILL wiki in Unity units/s.

| Technique | Value | Source |
|-----------|-------|--------|
| Coyote time | 0.1 s | Celeste JumpGraceTime; dot coyote_time |
| Coyote time | 0.15 s (ground or wall) | TF recreation k_coyoteTime |
| Jump buffer | 0.1 s | dot jump_buffer_time (Celeste's 0.08 s Input.cs buffer: not checked) |
| Variable jump hold window | 0.2 s | Celeste VarJumpTime |
| Apex hang | half gravity while abs(vy) < 40 of MaxFall 160 | Celeste HalfGravThreshold |
| Jump horizontal kick | +40 px/s | Celeste JumpHBoost |
| Corner correction | 4 px up, 4 px dash | Celeste UpwardCornerCorrection, DashCornerCorrection |
| Wall-jump reach | 3 px check (article: 2 px), super wall jump 5 px | Celeste WallJumpCheckDist; Celeste & Forgiveness |
| Wall-jump input lock | 0.16 s forced away | Celeste WallJumpForceTime |
| Wall-jump impulse | up 320 + away 350 u/s (older page text) / up 640 + away 350 (current) | TF recreation |
| Wall-jump impulse | wallNormal * 9 + vy = 8.5 m/s | Godot wallrun prototype |
| Wall jumps per airtime | 3 max | ULTRAKILL wiki |
| Wallrun gravity | grip 0 g for 0.5 s then ramp to full over 1.0 s (older) / 1.0 s + 2.0 s (current) | TF recreation |
| Wallrun gravity | 1.0 vs 20.0 normal (5 %) | Godot wallrun prototype |
| Wallrun min entry speed | 200 u/s horizontal | TF recreation |
| Wallrun speed | accel 800 u/s2 to cap 630 (older) / 500 to 800 (current) | TF recreation |
| Wallrun max time per wall | 1.75 s (older) / 5.0 s (current) | TF recreation |
| Wallrun camera roll | 7.5 deg | TF recreation |
| Wallrun camera roll | 20 deg, lerped | Godot wallrun prototype (too much for a shooter, by my reading) |
| Lurch (post-jump redirect) | max at 0.2 s, off at 0.5 s, re-arm after 0.3 s grounded, cap 400 u/s | TF recreation (search snippet + page) |
| Q3 ground | accel 14, decel 10, friction 6, speed 7, gravity 20, jump 8 | CPMPlayer.cs |
| Q3 air | accel 2, decel 2, airControl 0.3, k = 32; side-strafe accel 50 to wishspeed 1 | CPMPlayer.cs |
| Source air | airAcceleration 12, airCap 0.4 m/s, maxVelocity 50, friction 6, gravity 20, jump 6.5 | Olezen MovementConfig |
| HL2 air | AirSpeedCap 57.15 cm/s (30 HU), air accel mult 10, ground friction 4, jump 304.8 | PB PBPlayerMovement.cpp |
| Air wish cap | max_air_wish_speed 1 m/s, air_accelerate 100, stop_speed 4 | dot tunables |
| Slide entry | speed * 1.75, clamped 9..18 m/s | Olezen slideSpeedMultiplier, min/maxSlideSpeed |
| Slide friction | constant 14 m/s2 (speed -= 14 * dt) | Olezen slideFriction |
| Slide downhill | gain * 2.5 of slope component | Olezen downhillSlideSpeedMultiplier |
| Slide re-entry delay | 0.5 s | Olezen slideDelay; dot slide_cooldown |
| Slide length | 1.0 s max | dot slide_duration |
| Slide boost | x1.5 | PB CrouchSlideBoostMultiplier |
| Slide boost | max +380 u/s, cooldown 1.5 s, cancels below 200 u/s; slide-hop vy 500 | TF recreation |
| Slide (Karlson) | 400 impulse on crouch, counter-movement 0.2 vs 0.175 walking | DaniDevy PlayerMovement.cs |
| Rampslide | step height and floor angle shrink between 1.7x and 2.5x sprint speed | PB SpeedMultMin / Max |
| Speeds | walk 16.5, slide 24, dash 49.5, whiplash reel 60, slam 100 u/s | ULTRAKILL wiki |
| Dash budget | 3 stamina, dash 1, dash-jump 2, regen 0.7 / s (Standard) | ULTRAKILL wiki |
| Grapple | parabolic launch to point, apex = target height + overshoot | Dave Grappling.cs |
| Grapple | range 2200 u (from 4000), timeout 5 s | TF recreation |
| Hard speed cap | 7000 u/s horizontal | TF recreation |
| Launch ability | cooldown 8 s | dot launch_cooldown |

## Talks and write-ups

1. Maddy Thorson, "Celeste & Forgiveness": https://maddymakesgames.com/articles/celeste_and_forgiveness/
   Rules: coyote time; buffer the jump so it fires on the landing frame; halved gravity at the jump peak; jump and
   dash corner correction (wiggle around a bonked corner); wall jump from 2 px away, super wall jump from about 5 px;
   lift momentum storage; a grace window that converts one jump type to another after the fact (stamina refund).
   The player never sees these; they only feel that near-misses go their way.
2. ULTRAKILL devs on mechanics (80.lv): https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/
   Hakita: "The main focus was just on the game feel"; "Going fast is fun"; constant forward momentum with short
   breathing gaps for contrast. Interview text: https://intothebluesky.com/2020/12/19/interview-with-arsi-hakita-patala/
   (search snippet only, not opened).
3. ULTRAKILL wiki, Stamina: https://ultrakill.wiki.gg/wiki/Stamina
   A movement budget, not a cooldown per verb: 3 bars, dash 1, dash-jump 2, dash i-frames, wall jumps capped at 3 per
   airtime, a short window after a ground jump where slam is refused.
4. PC Gamer, "2026 is a low-key revolutionary year for player movement":
   https://www.pcgamer.com/games/2026-is-a-low-key-revolutionary-year-for-player-movement-in-games/
   (snippet only) Titanfall 2 blends movement states into one continuous flow so advanced play reads as choreography.
   A GDC Vault talk on Titanfall 2 or Doom Eternal movement: not found within the budget.

## Top 8 patterns to adapt, ranked by feel impact per hour

All windows below in whole 120 Hz steps (`stepsFor`); new numbers go in a Unity-side config file (CLAUDE.md rule 5),
since the web never had them.

1. **Jump buffer + coyote on every surface (12 steps each).** 0.1 s / 0.1 s (Celeste, dot), up to 0.15 s (TF rec).
   Apply coyote after leaving a wall too, and let a buffered jump fire on wall contact as a wall jump. Cheapest, largest
   "responsive" gain; fixes "I pressed jump and nothing happened".
2. **Frictionless buffered landing (bhop forgiveness).** If a jump is buffered on the landing step, jump that step and
   skip ground friction (Q3 / Source order: friction is only applied when not jumping). Q3's holdJumpToBhop and
   Olezen's autoBhop show the hold-to-hop option; keep the airCap-style wish clamp (Source 30 HU, dot 1 m/s) so speed
   comes only from strafing.
3. **Slide = clamped multiplicative boost + constant friction + cooldown.** Entry `v = clamp(v * 1.5..1.75, min, max)`
   (Olezen 9..18 m/s, PB x1.5), friction a constant deceleration (14 m/s2), downhill gain x2.5 on the slope term, boost
   re-armed only after 0.5-1.5 s (Olezen 0.5, TF rec 1.5) so spam does not stack. Keep full horizontal speed on a jump
   out of the slide (slide hop; TF rec vy 500 u/s).
4. **Wallrun with phased gravity.** Attach only above a horizontal speed floor (TF rec 200 u/s); 0 gravity for a grip
   phase (0.5 s), then ramp to full over 1.0 s; accelerate along the wall toward a cap; force kickoff after a per-wall
   limit (1.75 s) and lock re-attach to the same wall until grounded or another wall. Camera roll about 7.5 deg, eased
   (20 deg in the Godot repo reads as too much for aiming).
5. **Wall jump: fixed up + away impulse, tangent kept, count capped.** Set vy to an up value and add a fixed away
   component along the wall normal (TF rec 320 up / 350 away; Godot normal * 9 + vy 8.5), keep the along-wall speed,
   and cap wall jumps per airtime at 3 (ULTRAKILL). Widen the detection a little beyond contact (Celeste: 2-5 px past
   the wall) and lock steering away for a short force time (Celeste 0.16 s, about 19 steps).
6. **Jump lurch: one redirect window after takeoff.** For 0.2-0.5 s after a jump, a strong input change rotates part of
   the horizontal velocity toward the new wish direction, capped (TF rec 400 u/s), re-armed after 0.3 s grounded.
   Gives Titanfall's "correctable" jumps without free air control; Q3 CPM air control (k = 32, airControl 0.3, only
   when forward is held) is the copy-free alternative idea.
7. **Grapple: constant-speed pull or launch arc, velocity kept on release.** Two models seen: Dave's parabolic launch
   (solve the jump velocity that peaks at target height + overshoot) and ULTRAKILL's constant reel (60 u/s). For a
   deterministic mover, prefer a constant pull speed plus a hard rope length for swings (on each step, remove the
   outward radial component of velocity when past the length; no spring), and add, never replace, velocity on release
   so grapple chains into slide and bhop. Range and a timeout (TF rec 2200 u, 5 s).
8. **Corner correction, apex hang, speed-scaled step-up.** Nudge the player up to a few cm when the head or a ledge lip
   clips a corner (Celeste 4 px up), apply half gravity near the jump apex (Celeste: abs(vy) < 25 % of max fall), and
   at high speed shrink step height / widen slide angles like PB's rampslide (1.7x-2.5x sprint) so momentum survives
   lips and ramps instead of snagging.
