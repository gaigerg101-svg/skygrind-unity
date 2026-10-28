# R1: arenas, wall riding, aim slow-mo, open source (wave 27 research)

Date 2026-10-09. Web research only. Tags: [read] = I fetched the page and read the extract; [snippet] = search summary only; [design] = my own inference, not a source claim.
Honesty note: WebFetch returns a small-model summary of each page, not raw text, so [read] means "fetched", not "verbatim". Several sources the brief asked for had NO numbers (stated per section). Nothing here touches the local repo except this file.

---------------------------------------------------------------------------
## 1. Encounter and arena design

### 1.1 ULTRAKILL: what the sources really give

Devs (Hakita) on design, [read] https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/
- "levels are linear to make sure the fast pace is kept up and the player doesn't get lost."
- Visuals "very easy to read at a glance"; geometry simple and clean so high-speed traversal does not disorient.
- Enemy AI "purposefully simple" so enemies are predictable and easy to track. Threat comes from quantity, because the player has so many movement tools.
- Constant forward momentum, only occasional downtime for contrast.
- No arena sizes, no enemy counts in the interview.

Hakita / Arsi Patala interview, [read] https://intothebluesky.com/2020/12/19/interview-with-arsi-hakita-patala/ : only enemy-fragility tradeoff ("tanky would detract from the do-or-die quick action... fragile means they can't be comboed"). No level numbers.

Concrete encounter data from the wiki, [read] https://ultrakill.wiki.gg/wiki/0-4:_A_ONE-MACHINE_ARMY (layer 0 level 0-4):
- First arena: wave 1 = 10 Filth + 5 Strays (15 enemies); wave 2 = 1 Malicious Face, triggered after clearing either all Filth or all Strays.
- Rotating tunnel: wave 1 = 6 Filth + 1 Stray; wave 2 = 4 Strays (on reaching the centre platform).
- Final arena: wave 1 = 6 Filth + 4 Strays + 2 Schisms (12); wave 2 = 14 Filth.
- Rank targets for the WHOLE level: S = 2:20, 53 kills, 8500 style; A = 3:00; B = 4:00; C = 5:00.
- The level challenge is "slide uninterrupted for 17 seconds", i.e. the level is a momentum gauntlet between arenas.
- Reading: a whole level is about 53 kills in about 2.5 min, in 3 arenas of 7 to 20 enemies in 1-2 waves, mostly fodder (Filth) plus a few ranged and one heavy.

Other arenas, [snippet] via search summary of the wiki (fandom pages returned HTTP 402 when fetched): Statue Arena = large room, several platforms, stairs; 5 Strays on high platforms + 10 Filth on the ground, a Malicious Face after either group clears. Fan Arena = glass floor over a spinning fan; wave 1 = 4 Strays, 6 Filth, 2 Schisms, then more Filth from the rafters, then the exit opens. Pattern: wave 1 is 12-15 enemies, trigger-based second wave is 1 heavy or 4-14 fodder, the exit opens on clear.

Cyber Grind (endless mode), [read] https://ultrakill.wiki.gg/wiki/Cyber_Grind
- Arena is a fixed 16x16 tile grid (tile size not given on the page); the layout is regenerated each wave; health, stamina and cooldowns restore before each wave.
- Spawn budget: initial max points 10, plus 3 + floor(wave/3) per wave (wave 5 = 28, wave 10 = 55). Common enemies fill the remaining budget, 0.1 s between spawns; uncommon from wave 12, specials from wave 16, radiance (x3 cost) from wave 25.
- Reading: a point budget per wave, not a raw count, is the ULTRAKILL way to keep enemy count balanced. SKYGRIND can copy the idea (cost per enemy type).

Not found: arena dimensions in metres for any ULTRAKILL arena. I found no dev statement of metres. Do not cite a figure for ULTRAKILL room size. [design] fall back to visual estimate and own testing.

### 1.2 Doom Eternal

Source [read] https://blog.adamatomic.com/post/613311014289768448/design-of-doom-eternal (design critique, not a dev talk):
- Structure is corridors linking big wave-based arenas. Tall arenas come in two flavours: "jungle gyms" (distinct floors, separated combat levels) and "canyons" (open, fewer elevation divisions). The critic says canyons are overused and jungle gyms break up ranged fire better.
- Resource loop: chainsaw = ammo, glory kill = health, flame belch = armor. Low ammo pools force the player to kill weak enemies as "dispensaries" while managing big threats.
- Cognitive load: 7+ enemy types per wave is flagged as a lot to track.
Also [snippet] https://www.gamedeveloper.com/design/the-dark-souls-design-of-doom-eternal (combat loop framing: you cannot finish an arena without running short on something). No per-arena counts or minutes found.
Takeaway: every encounter is a closed resource loop (kills refund what the fight spends), which stops the player from camping or fleeing.

### 1.3 Generic encounter rules, [read] https://book.leveldesignbook.com/process/combat/encounter
- Enemy type count per encounter: 1 = rest/tutorial, 2 = standard ("work together in an interesting way"), 3 = hard to manage, 4 = brawl (use two factions for readability), 5+ = slaughter (player focuses on a subset anyway).
- Telegraph window about 2 to 3 s for big attacks (God of War example).
- No arena metres or fight durations on that page.

### 1.4 Titanfall 2, Ghostrunner, Mirror's Edge, Neon White (pacing)
- Titanfall 2 SP: GDC 2018 "Designing Unforgettable Titanfall Single Player Levels with Action Blocks" (Christopher Dionne). Page [read] https://80.lv/articles/gdc18-designing-titanfall-levels-with-action-blocks had no numbers (recording link https://gdcvault.com/play/1025105/Designing-Unforgettable-Titanfall-Single-Player is the real source; not watched). [snippet] https://primagames.com/news/blocking-drunk-titanfall-2s-story-mode-came : the team wanted to keep movement freedom while stopping the player simply wall-running past everything; pacing was the control. "Action blocks" = a rapid-prototyped chunk of one core verb in a small space.
- Ghostrunner, [snippet] https://www.inverse.com/gaming/ghostrunner-interview-cyberpunk-hotline-miami : devs looked at Titanfall 2 and Mirror's Edge; always keep the next target in the player's field of view; "most important principle was to keep the player moving"; combat arenas are race-track-like with several obvious routes and lane switches (secondary summary, not a dev quote I read).
- Mirror's Edge, [snippet] https://worldofleveldesign.com/categories/level_design_tutorials/one-mistake-in-mirrors-edge.php : the route being out of view breaks flow; designers and game designers did not share a vision.
- Neon White, [snippet] https://primagames.com/tips/neon-white-tips-and-tricks-for-going-fast : stages are 20 to 30 s, built for speedrunning. Evidence that very short levels work.
- Not found: any dev-stated wall length, gap size, or arena size for these games.

---------------------------------------------------------------------------
## 2. Rules that make wall riding the best route

Honest status: I found NO published numeric wall-run geometry rules from Respawn, One More Level, DICE, Insomniac or Dani. What follows is what the sources state qualitatively plus [design] inferences from them and from the ULTRAKILL data above.

Stated by sources:
- Ghostrunner: several obvious routes, lane switching, next thing always in view. [snippet] (see 1.4)
- Titanfall 2: the wall-run must be paced, otherwise it trivialises the level. [snippet]
- Sunset Overdrive: the whole city was redesigned for momentum (no ladders, rails and wires everywhere, wall-run, zip-lines) so touching the ground is the slow option. [snippet] https://en.wikipedia.org/wiki/Sunset_Overdrive
- ULTRAKILL 0-4 challenge: "slide 17 seconds uninterrupted" implies the route is built so the fast verb is continuous and ground interruptions (doors, breakable glass) are the penalty. [read]
- Karlson: verticality in urban/industrial settings; wall-run is a core verb. [snippet] https://shapes.inc/karlson (low-quality source)

[design] geometry rules (to be tested, not sourced):
- Make the wall the shortest line: place the wall-run segment on the straight line between spawn/entry and the exit/objective, and make the floor route 30-50 % longer (detour around a pit or obstacle).
- Floor under the wall-run is a hazard or a void in the fast lane (pit, lava, laser floor, kill-plane). The player is never punished on the wall but is on the ground.
- Wall run lane is continuous: long enough to cross the room (the room's long axis) with no more than one small gap per run; gaps bridged by a jump a normal air control covers.
- Ceilings low enough (about 2.5 to 3 m above the wall-run line) near the walls to stop camping on top, high enough in the centre for jump arcs. Mark the wall surface with a bright cyan strip (project palette: cyan = player-usable).
- Lanes: 2 parallel wall lanes (left and right walls) so the player can cross over by jumping between walls; enemies spawn in the centre so wall lanes circle them.
- Put the wall-run entry within 3-4 m of the room entrance so the player sees it immediately (Ghostrunner "next thing in view").
- Give enemy ranged units sightlines to the wall lane (not cover behind it) so the lane is the dangerous-fun route, not the safe one.

---------------------------------------------------------------------------
## 3. Mild slow-motion on aim

### 3.1 Prior art (values)
- Ghostrunner Sensory Boost: hold in air to slow time for a few seconds, enemy bullets nearly stop while the player keeps near-normal control; consumes Focus meter; "Slowdown" upgrade makes it stronger. [snippet] https://godisageek.com/2020/10/ghostrunner-beginners-tips/ . No numeric timescale found.
- Superhot: time advances in proportion to player movement. [snippet] https://en.wikipedia.org/wiki/Superhot
- Katana Zero: hold LB to slow time, limited by a meter that refills. [snippet] https://shacknews.com/article/111305/katana-zero-review-the-cutting-edge
- ULTRAKILL: holding the variation-wheel button brings up the wheel and slows time; no number published. [snippet] https://ultrakill.wiki.gg/wiki/Weapons
- Doom Eternal: weapon wheel slows time but does not pause; no percentage found. [snippet] (doomwiki page returned HTTP 403, not read)
- Dishonored 2 Blink: time stops/slows while selecting the target. [snippet] (no value)
- Neon White has no slowdown (short levels instead). [snippet]
- Max Payne: NOT researched successfully (no source fetched). Do not cite values.
- Honest gap: no source gave a time scale for any of these. Typical practice [design]: aim-assist slow 0.6 to 0.8 (mild = 20-40 % slower); a "bullet time" is 0.2 to 0.3. "Mild" for SKYGRIND: target 0.75 (range 0.65-0.85), ease in 120-150 ms, ease out 80-120 ms (out faster than in so exiting never feels sticky). Test blind.

### 3.2 Unity facts, [read] https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Time-timeScale.html
- Time.timeScale changes take effect the next frame; at 0.5 time runs half speed.
- FixedUpdate frequency depends on timeScale; to keep fixed steps per frame constant you must also scale Time.fixedDeltaTime yourself.
- This is why timeScale is the wrong tool for SKYGRIND: it would also change the audio/UI/camera unscaled timers and the project rule "one fixed 120 Hz loop in Update with accumulator".

### 3.3 Deterministic way (project rule 4) [design]
Keep the 120 Hz step constant and slow the rate at which real time is fed into the accumulator. The sim sees identical steps, so golden vectors and replays stay bit-exact, because replays record steps, not seconds. Slow-mo is a presentation-time scale only.

```csharp
// SurvivalGame.Update (pseudo-C#)
float real = Mathf.Min(Time.unscaledDeltaTime, MaxFrame);       // unscaled, clamp
slowTarget = (player.WallRiding && player.AimHeld) ? SlowScale : 1f;  // e.g. 0.75
float rate = slowTarget < slow ? 1f/EaseInSec : 1f/EaseOutSec;      // smooth, in unscaled time
slow = Mathf.MoveTowards(slow, slowTarget, rate * real);
acc += real * slow;                      // the ONLY change: dt scaled
while (acc >= Step) { Simulate(Step); acc -= Step; }  // fixed 1/120, unchanged
Render(alpha: acc / Step);               // interpolation unchanged
// hitstop stays as today; apply it after (acc += 0 while hitstop).
```
Input responsiveness: sample input every render frame in unscaled time and queue it to the next step. Look/aim (camera yaw/pitch) should stay at real-time sensitivity, so the player feels the world slow, not themselves. Keep the slow factor out of the sim state (do not store it in replays); if it must affect gameplay (enemy projectile speed relative to the player), it is by definition a sim change and needs a golden-vector pass. If it is presentation only, the player moves slower in real time too, which is the intended "world slows" read; decide and document. Cooldown/timers that are in steps keep their step counts, so they stretch in real time: say so in the design. Reset slow to 1 at run start (rule 5/6: per-run state).

### 3.4 Screen effects (URP, cheap)
- URP Volume overrides, [read via snippet] https://docs.unity.com/en-us/engine/6000.3/manual/post-processing-and-full-screen-effects/urp/effect-list : Vignette and Chromatic Aberration are the cheapest overrides; Chromatic Aberration is discouraged for VR comfort. [snippet]
- Suggested stack, all driven by one 0..1 value `slow01` (lerped in unscaled time) [design]: vignette intensity 0 to 0.25 and a tint; a small chromatic aberration 0 to 0.15; saturation -10 to -20 (colour adjustments); FOV +3 to +5 degrees; a subtle radial/edge streak sprite at the lower alpha. Note the project's "never again": screen-border speed lines were retired in w20 (CORE.md). So avoid border streaks; use vignette + desaturation + FOV, which are not "forced border" effects.
- Cost: use ONE global Volume with a weight blended by slow01, never enable/disable overrides per frame (avoids a shader variant hitch). Chromatic aberration and vignette are single full-screen passes inside the existing post stack.
- Feel metric (project lesson L-001): measure that the screen changes and run a blind paired A/B.

---------------------------------------------------------------------------
## 4. Open source worth reading (GitHub)

Method: GitHub search API through WebFetch (several queries returned zero hits, so coverage is thin). Licences are the `license.spdx_id` from the API; "None" = no licence file, so NOT copyable (all rights reserved).

| Repo | Licence | Use |
|---|---|---|
| https://github.com/CoffweDev/physics-based-wallrunning-unity | MIT (copyable) | single WallRun.cs, read |
| https://github.com/Jobberson/Simple-Unity-Wave-Spawner | MIT | wave spawner, folder `Snog/`; files NOT read |
| https://github.com/luckyblackcat23/new-wave-spawner | MIT | tower defence wave spawner; not read |
| https://github.com/RaunakGameDev/SharpShooter-Prototype | MIT | first-person arena shooter, wave-based; not read |
| https://github.com/superbderrick/UnitySpawningwaves | Apache-2.0 | wave spawn test; not read |
| https://github.com/ozata/unity-ultimate-titanfall-2-wallrun | None (inspiration only; do not copy) | Titanfall 2 style wall run |
| https://github.com/Legendized/Karlson-Movement-With-Wallrunning | None (inspiration only) | Karlson movement + wall run |
| https://github.com/Vakarian15/Unity-WallRunning-and-GrapplingHook | None | FPS microgame wall run |
| https://github.com/8izips/PostProcessingURP | MIT | collection of URP post effects (read-for-ideas) |
| https://github.com/keijiro/KinoEightURP | Unlicense (public domain) | example of URP renderer-feature post pass |
| https://github.com/sugi-cho/Unity-URP-PostProcessing-With-Alpha | MIT | URP post pass with alpha |
| https://github.com/p3nGu1nZz/unity-input-system | MIT [snippet] | Unity input system character controller with wall running (not fetched) |
| https://github.com/ttesla/WallBreakingFramework | MIT [snippet] | not relevant to wall running |

Gaps: no dedicated "bullet time URP" repo came back (query returned 0). No arena-generator repo found. The ULTRAKILL-like arenas have no open source clone I could find. Keijiro's repos (Unlicense) are the usual place for cheap URP full-screen passes: worth a manual look.

### 4.1 Read and summarised: CoffweDev/physics-based-wallrunning-unity (MIT), WallRun.cs [read, condensed by the fetch tool, not verbatim]
Technique: rigidbody-based. Two side raycasts, a ground-clearance raycast, forward force, wall-normal jump, camera tilt.
```csharp
// each Update
wallLeft  = Physics.Raycast(pos, -orientation.right, wallDistance, wallLayer, out leftHit);
wallRight = Physics.Raycast(pos,  orientation.right, wallDistance, wallLayer, out rightHit);
bool offGround = !Physics.Raycast(pos, Vector3.down, minJumpHeight);   // must be above the floor
if (offGround && (wallLeft || wallRight)) {
    rb.useGravity = false;                      // reduced/none (assumed; not in the summary)
    rb.AddForce(orientation.forward * wallRunSpeed);
    tilt = Mathf.Lerp(tilt, wallLeft ? -camTilt : camTilt, camTiltTime * dt);  // camera roll
    if (jumpPressed) {
        Vector3 dir = Vector3.up * wallRunUpFactor + wallHit.normal;           // push away + up
        rb.AddForce(dir * wallRunJumpForce * 70f);
    }
} else { tilt = Mathf.Lerp(tilt, 0, camTiltTime * dt); StopWallRun(); }
```
Verdict for SKYGRIND: the SKYGRIND web collider already has its own wall-run in the ported sim, so this is only useful as a cross-check of the standard parameter set (wallDistance, minJumpHeight, camTilt, camTiltTime, jump = up + normal). Do not copy: it uses Rigidbody, which project rule 4 forbids. Note the summary said "grounded" but the logic reads as "off ground" (the summary's naming was inverted); verify before relying on it.

### 4.2 Not fetched (honest)
The brief asked me to fetch the 2-3 best. I fetched one script (above). Wave spawner repos were found and licence-checked but their code was not read, and I found no URP slow-mo repo to read. A follow-up should fetch `Snog/` of Jobberson/Simple-Unity-Wave-Spawner (MIT) before deciding whether a spawner design is worth copying; SKYGRIND's existing spawner is a sim system with Mulberry32 so most likely only the data-structure idea (wave = list of {type, count, delay}) is reusable.

---------------------------------------------------------------------------
## What this means for SKYGRIND (buildable rules)

Numbers marked "src" come from the ULTRAKILL 0-4 data; "test" means a starting guess to be tuned by measuring.

1. Level length target 120 to 150 s of play, hard cap 180 s. src: 0-4 S rank is 2:20 / 53 kills; Neon White stages are 20-30 s.
2. 3 arena beats per level maximum (src: 0-4 has 3), each 1 to 2 waves.
3. Wave size 6 to 12 enemies, never more than 15 alive at once; total per level 35 to 55 kills (src: 53 kills in 0-4; 15, 7 and 12 in wave 1).
4. Use an enemy cost budget per wave (Cyber Grind style: 10 points + 3 per wave, cost per type) so counts are balanced by data, in a config file, not typed in code (rule 5).
5. Trigger the second wave on clear of one group ("either group clears"), not on a timer, to keep tempo; wave 2 is either 1 heavy or a 10-14 fodder burst.
6. At most 2 enemy types per wave (3 with a single heavy); 4+ only for finale (level design book).
7. Room width at most 24 m on the short axis and 40 m on the long axis (test; no source), tall 8 to 14 m, so a full crossing takes 3 s at wall-run speed. Every arena has two wall lanes along the long walls.
8. Wall-run lane is the shortest line from the entrance to the exit; the floor route is 30-50 % longer or has a hazard (test; design).
9. Floor beneath the wall lane is hazardous in the fast lane and safe in the slow lane (rule: wall = fast and safe from melee; ground = slow and exposed).
10. Each arena exit opens on clear; the connecting corridor is a 17 s-style slide or wall-run gauntlet with no doors or breakable floor interrupting momentum (src: 0-4 challenge).
11. Add jungle-gym tiers (separate floors) to split ranged enemy sightlines, not open canyons (Doom Eternal critique); always keep one route enemies can climb (GG's rule in CORE.md).
12. Aim-while-wall-riding slow: scale the accumulator input, not Time.timeScale, target 0.75 (range 0.65-0.85), ease in about 130 ms, out about 100 ms; the 120 Hz step, `stepsFor` timers and replays unchanged.
13. Keep aim/look at real-time rates and sample input every render frame so control stays crisp while the world slows.
14. Screen cue from one `slow01` value: vignette 0 to 0.25, chromatic aberration 0 to 0.15, saturation -15, FOV +4 degrees, one global Volume blended by weight; no border speed lines (retired in w20).
15. Judge it blind and paired (L-001): capture the same wall-ride with and without the effect, measure the screen change (mean luminance, edge vignette delta), and review blind before keeping it.

Open gaps for the next research round: ULTRAKILL room dimensions in metres (none found), Titanfall 2 / Ghostrunner numeric wall-run rules (need the GDC Vault video or a dev talk transcript), a real time scale from Ghostrunner / Doom Eternal (could be measured by frame-counting footage), and Max Payne values.
