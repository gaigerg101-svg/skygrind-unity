# R2: Original bosses, the ice chase, boss philosophy, open-source boss / track code

Date 2026-10-09. Web research only; nothing in the Unity project was edited besides this file.
Tags: [read] = I fetched the page and read the returned text. [snippet] = search-result summary only, not opened.
[design] = my own synthesis for SKYGRIND, not a claim about a source. Web content was treated as data.
Honest limit: the search tool returned thin results for several asked-for games (Sunset Overdrive, Sonic Frontiers /
Colors chase bosses, Burnout, Ghostrunner chase, Doom Eternal). Where it gave nothing I say "not found" rather than
guess. Section B's chase techniques are therefore mostly [design], built on the few facts I could read.

---------------------------------------------------------------------------------------------------------------------

## 0. Sources used (all URLs)

Boss philosophy
- S1 https://www.gamedeveloper.com/design/boss-battle-design-and-structure  [read]  Mike Stout's 8-part boss structure
- S2 https://www.thegamer.com/neon-white-celeste-boss-fights-cohesive-dark-souls/  [read]  Neon White / Celeste bosses keep the core mechanic
- S3 https://itch.io/blog/1024105/designing-the-perfect-boss-battle-a-game-developers-holy-grail.amp  [snippet]  boss = final exam
- S4 https://www.wayline.io/blog/crafting-legendary-boss-battles  [snippet]  readability / rhythm / fairness / escalation / reward
- S5 https://nex-3.com/blog/hades-ii-has-a-legibility-problem/  [read]  telegraph legibility critique (Hades II)
- S6 https://primagames.com/tips/mega-man-11-boss-order-and-boss-weakness  [snippet]  Mega Man weakness web
- S7 https://www.pcinvasion.com/?p=349990 and https://noping.com/blog/hi-fi-rush-rhythm-action-korsica-chai-combos-guia-2026  [snippet]  Hi-Fi Rush on-beat attacks
- S8 https://ultrakill.wiki.gg/wiki/Cerberus  [read]  Cerberus attack list
- S9 https://ultrakill.fandom.com/wiki/Gabriel,_Apostate_of_Hate  [snippet]  Gabriel combo, only the final thrust parryable
- S10 https://randomaccessmayhem.wiki.gg/wiki/Colossal_Pounding_Unit  [snippet]  boss damaged by ramming pillars (prior art for concept 3)
- S11 https://www.engadget.com/2017-01-31-exploring-what-made-the-doom-and-titanfall-2-campaigns-tick.html  [snippet]  Titanfall 2 campaign
- S12 https://gdcvault.com/play/1021780/AI-in-the-Awesomepocalypse-Creating  [snippet]  Sunset Overdrive AI built for a mobile player (title only)

Chase / catch-up
- S13 https://lawofgamedesign.com/2014/08/25/theory-rubber-bands/  [read]  rubber-band theory
- S14 https://giantbomb.com/wiki/Concepts/Running_Towards_the_Screen  [snippet]  "Bandicooting" chase sequences
- S15 https://en.wikipedia.org/wiki/Crash_Bandicoot_(video_game)  [snippet]  Boulders level
- S16 https://jojowiki.com/Ghiaccio  [snippet]  Ghiaccio, skates, air vent at neck is the weak point
- S17 https://jojowiki.com/GW_Episode_19  [snippet]  highway chase episode
- S18 https://lastminutecontinue.com/review/hands-on-ghostrunner/  [snippet]  Ghostrunner "close the gap" encounters

Code / repos
- R1 https://github.com/Inspiaaa/UnityHFSM  [read]  MIT. Hierarchical FSM
- R2 https://github.com/jhocking/StateMachineAI  [read]  MIT. Enemy state machine demo (read-only value: low)
- R3 https://github.com/sinbad/UnitySpline2D  [read]  MIT. Archived 2024-03-08. Distance-parametrised spline
- R4 https://github.com/zapdot/unity-curves  [read]  MIT (dual with Siccity Bezier3D). Distance-based sampling
- R5 https://matiaslavik.codeberg.page/unity-beier-roads  [read]  blog + Codeberg source, licence not stated on page
- R6 https://discussions.unity.com/t/released-runner-track-procedural-scrolling-track-system/1700422  [read]  commercial asset (read-only)
- R7 https://docs.unity3d.com/Packages/com.unity.splines@2.8/manual/index.html  [read]  Unity Splines, Unity Companion Licence (not MIT)
- R8 https://assetstore.unity.com/packages/tools/game-toolkits/forever-endless-runner-engine-140926  [snippet]  Forever, free, closed (read-only)
- Raw files fetched: https://raw.githubusercontent.com/Inspiaaa/UnityHFSM/master/src/StateMachine/StateMachine.cs ,
  .../src/Transitions/TransitionAfter.cs , https://raw.githubusercontent.com/sinbad/UnitySpline2D/master/Spline2D.cs
  (all [read]; the first WebFetch of a guessed path 404'd, the tree listing gave the right path). Note WebFetch returns a
  model-written summary, not the verbatim file, so the pseudo-C# below is a summary of the code, not a copy.

---------------------------------------------------------------------------------------------------------------------

## C. General boss philosophy (read first; it shapes A and B)

What the sources actually say
- A boss is the "final exam": the level teaches, the boss tests the same thing. [snippet S3] Zelda gives you the item in the
  dungeon, teaches it, and the boss tests that item. [snippet S3]
- Neon White and Celeste keep the boss on the core mechanic (speed-platforming; dodging plus platforming) and intensify it,
  instead of adding a new skill. Neon White bosses are rare and keep the stopwatch and rank. [read S2] The same article
  argues Dark Souls-style bosses demand new tactics in a plain arena, a break from exploration flow. [read S2]
- Mike Stout's 8 beats: build-up, intro/reveal, business as usual (basic attacks only), escalation (new attacks),
  midpoint (transformation / false defeat), "it's ON" (full arsenal), kill sequence, victory. Isolate and drill every
  skill in a safe place before the boss; never introduce an untested mechanic in a high-intensity phase. Boss must
  telegraph, must not be stun-locked, needs exposed weak points. [read S1]
- Mega Man: each boss is weak to one other boss's weapon, which makes "what weapon" the puzzle. [snippet S6]
- Hi-Fi Rush: attacks land on the beat, so a failed parry is always legible; first boss spawns wind balls a quarter note apart
  and you parry them in the same rhythm. [snippet S7]
- ULTRAKILL Gabriel: combo of four hits then a long thrust, and only the finishing thrust is parryable, so the fight asks
  one specific verb at one specific moment. [snippet S9]
- Legibility: attacks that look alike should act alike; windup length should match attack speed; do not flag an attack
  unblockable only after the player fails. [read S5]
- Not found: Titanfall 2 Viper / Ash design analysis beyond marketing-level text. [snippet S11] The one useful claim is that
  the Ash fight is a tight room and it was criticised for abandoning the movement design. I could not verify the Viper
  detail; treat as folklore.

Reusable template [design, built from S1, S2, S5, S9]
```
BOSS CARD
  Teaches ONE verb V (dash | wall-ride | slide | pulse-launch) and rewards ONE weapon W.
  Intro        3-5 s    boss silhouette + name; player already in control; no cutscene lock. One clean show of V.
  Phase 1      30-40 s  ONE attack, tell >= 0.8 s, repeats 3x. Vulnerable window opens only if V is performed. Safe to fail.
  Phase 2      30-40 s  + a second attack (tell >= 0.6 s) that punishes standing still. Window shrinks 20 %.
  Phase 3      20-40 s  both attacks interleaved, arena changes once (a pillar falls, floor drops). V now chained with a 2nd verb.
  Kill         3 s      hitstop + slow-mo; hero shot of the verb.
  Tell times   telegraph >= 0.5 s (project rule), light-cue + ground decal + fixed colour (red-orange = danger).
  Window rule  "vulnerable" is a STATE the player causes with V, never a timer the boss runs by itself.
  Reset        a failed window costs time, not health: boss re-arms, no stun-lock.
  Colour       cyan = what the player can use (the exposed weak point glows cyan); red-orange = attack telegraphs.
```
Why: the only way the boss differs from "shoot the thing" is that the damage gate is a movement state. Gate on V, give W
the damage multiplier in that window, keep every telegraph legible.

---------------------------------------------------------------------------------------------------------------------

## A. LEVEL 1 BOSS: five original concepts

Prior art found (so we know what to avoid copying)
- ULTRAKILL Cerberus: dash charge (25 dmg + knockback), stomp shockwave, energy-orb throw; head takes 200 %. It will not
  throw while you stand close. [read S8] GG's complaint, a charger that hits a wall and gets stunned, is this family.
- Boss that damages itself on arena pillars: Random Access Mayhem's CPU takes 50 + 25 per recent pillar bounce. [snippet S10]
  So "make it hit its own pillars" exists in a small game; concept 3 must differ from "charger vs wall" (see below).
- Boss weak point behind an exposed part: JoJo Ghiaccio has an air vent at the neck. [snippet S16]
- Not found: a boss whose weak point needs a wall ride in an FPS (I could not verify one). Treat as open design space. [snippet]

Constraints used for ranking: enemy AI = a state machine in pure C# in Skygrind.Core; telegraphs >= 0.5 s; 60-120 s fights;
no PhysX; boss and rays use the web collider; deterministic Mulberry32; blocky model; easy first boss.

### Concept 1: THE LANTERN (wall-ride gate). Rank 1 [design]
A hovering box-lantern (the keeper of a ring-shaped courtyard) keeps a shield-plate rotated to face the player. The shield
is down only on the face toward a wall the player is currently riding. Boss shoots slow, visible orange orbs around the ring.
- Verb forced: WALL RIDE (and wall jump to re-enter). Weapon: rifle or burst gun for the exposed seconds; pulse gauntlet
  closes the finale.
- Phases: P1 one wall (cyan strip) plus one orb pattern; P2 two walls alternate (ride, hop to the other); P3 walls
  retract in sequence so you chain rides around the ring.
- Fight 75-100 s. Build effort: low-medium (arena = ring of wall segments; boss = orbit + shield flag).

### Concept 2: THE TURNSTILE (rotating arena, slide gate). Rank 2 [design]
Boss sits at the centre of a slowly rotating disc; floor slats open in gaps; a sweeping beam crosses the disc.
Beam is slid under (low) or dashed over (a marked gap); weak point only exposed while the beam is swept past the player.
- Verb: SLIDE and DASH. Weapon: shotgun on the exposed core, or pulse launch to cross a gap.
- Phases: beam low only; beam high + low alternating; two beams plus the disc speeds up.
- Fight 80-110 s. Risk: rotating geometry vs the web collider (see rule 14); do the rotation as the boss's own visuals
  plus gap boxes moved on the sim step, not as a rotating collider.

### Concept 3: THE KILN-WARDEN (makes its own fire hit it; pulse gate). Rank 3 [design]
A heavy turret-crawler fires a long fire jet along a lane. The jet burns cracks into pillars; a pillar that has been
burned for 3 s becomes "hot". The player PULSE-launches a hot pillar into the boss (the gauntlet's charged hit sends the
pillar chunk or shock into it). Differs from Cerberus: the boss never stuns itself; the player is the cause, and it needs
a launch, not a lure.
- Verb: PULSE LAUNCH (charged melee). Weapon: gauntlet only as damage; gun for clean-up.
- Phases: one pillar; two pillars and boss moves; pillars spawn adds you must clear.
- Fight 90-120 s. Build effort: medium (pillar state, projectile from the launch; relies on the gauntlet working).

### Concept 4: THE UNSEALER (dash-gated shield). Rank 4 [design]
A shield-ball boss is immune until the player dashes through the ring of its own charge-up arc (it pulses a cyan ring on a
fixed beat; a dash that crosses the ring breaks one of three shield nodes). Boss attacks are lane shots on a fixed beat.
- Verb: DASH. Weapon: any hitscan on the broken node. Hi-Fi Rush-like rhythm [snippet S7].
- Fight 60-80 s. Build effort: lowest; easy to read; but weakest "movement test" since a dash is a basic verb.

### Concept 5: THE CLIMBER (weak point only on its back; wall-ride behind it). Rank 5 [design]
A tall walker patrols a canyon; the weak point is on its back and it turns to face you. You must out-run its turn by wall
riding along the canyon wall to get behind. Prior art feel: Shadow of the Colossus-style climbing, not read. [no source]
- Verb: WALL RIDE + wall jump. Weapon: shotgun at close range.
- Fight 100-120 s. Build effort: high (boss turning, tall rig, canyon geometry); good for a later level, not level 1.

Ranking by buildability (our tools): 1 Lantern, 2 Turnstile (if rotating is faked), 4 Unsealer (cheapest, weakest),
3 Kiln-Warden, 5 Climber. Recommendation: build Lantern for level 1; it needs no new physics, uses the existing wall ride,
and its shield flag is a single pure-C# bool tied to the player's ride state.

---------------------------------------------------------------------------------------------------------------------

## B. LEVEL 2 BOSS: THE GLACIER (ice chase)

### B1. What I could verify about chase bosses
- Ghiaccio: ice shield on a car, skates after it; weak point is an air vent at his neck. [snippet S16, S17] In the JoJo game
  the player must keep him from the car for two minutes. [snippet]
- Crash Bandicoot "Boulders": the first chase level, camera faces the player so you run toward the screen and cannot see
  obstacles until the last second. "Bandicooting": obstacles emerge from the bottom of the screen, pursuer stops you
  slowing. [snippet S14, S15] Note this is the PURSUED-by-danger chase; ours is the reverse (we chase the boss), so the
  camera issue is the same but the rule changes: in ours the camera sees AHEAD, which fixes the problem.
- Rubber-banding: boosts the trailing player or hampers the leader, can make good play meaningless; works when it adds
  decisions and stays skill-dependent (Street Fighter 4 ultra = good; Mario Kart blue shell = bad). [read S13]
- Ghostrunner: each encounter is a puzzle of how to move to close the gap. [snippet S18]
- Sunset Overdrive: AI designed around a mobile player. [snippet S12; title only, not opened]
- Not found: Sonic Frontiers/Colors chase boss specifics, Burnout, Hi-Fi Rush chase, Doom Eternal. I will not describe them.

### B2. Design for THE GLACIER [design, grounded in the above]
Core loop: boss is AHEAD on an endless curving ice course; it can only be hurt by a PULSE-launch (melee dash-hit) landed
after you close the gap. All the weight is on keeping the player closing the gap without a speck feeling hopeless.

How to keep a speck-in-the-distance from feeling bad
1. Boss speed is a function of the PLAYER's speed, not of time (a bounded elastic band): target gap G*(t) between 40 m
   and 120 m; boss speed = base + k*(G - G_target), clamped to +-15 % of player top speed. This is the good kind of band
   [read S13]: it never makes the player lose by playing well; clean skating closes it, mistakes open it.
2. Big HUD: a chevron tape at the screen top-centre with gap in metres (e.g. "62 m") and a closing-speed arrow; boss is
   marked by a cyan ring on the world even when out of view. (cyan = player-usable, so the boss's CATCH-UP target is cyan;
   its attacks stay red-orange.)
3. Boost lines: 3-4 chevron boost pads in a clean line per 40 m tile give a clearly-better line; missing them is a
   readable cost.
4. Sense of speed without banned border speed lines: use FOV push (+8 deg at top speed) and trail streaks on the ICE
   rather than screen-border lines (GG retired border lines in w20).
5. Camera: first-person, so the horizon and course stay visible; add a small boss "dust trail" so the eye can track the
   speck. Do not hide the boss behind the camera.
6. Catch-up window: when gap < 12 m the player gets a PULSE-ready flash and a 1.5 s hit window; the PULSE connects only if
   charge >= x and aim within a cone. Miss = boss accelerates away 20 m (a cost, not damage).

Course
- Infinite, curving, built from 40 m tiles. Tiles recycle behind the player. Curvature chosen from a seeded Mulberry32
  in the sim. Details in section D.
- Hazard floor: ice cracks that DAMAGE (or "thin ice" that breaks). Forced wall rides where the floor is hazardous: a
  hazard stretch (12-20 m) with side walls; the player must wall ride past it. Teach this in the first 30 s on a safe pass.
- Boost pads and a "thin-ice" shortcut tile (risk/reward) per 3 tiles.

Boss attacks that read as lane attacks [design; telegraph >= 0.5 s]
- LANE SPIKES: the course is 3 lanes (left / centre / right); the boss throws a frozen wall across 1-2 lanes with a
  0.8 s glowing lane decal on the ice, then it appears. Player skates to the open lane or slides under a low one.
- CEILING ICICLES: a row of icicles falls on one lane; 0.7 s marker on the floor then drop; wall-ride to dodge.
- ROLLING SPHERES: an ice boulder rolls down the course toward the player on one lane; shows a streak 0.6 s before. Dash
  sideways, or jump.
- Tiny surprise rule: no attack without a decal. Keep three attack types, no more for a first version (Mike Stout's
  "never introduce untested mechanics in a high-intensity phase" [read S1]).

Phases [design]
- P1 (0-40 s): lane spikes only, gap 60 m, wide course. Teach the pulse-launch connect.
- P2 (40-80 s): add ceiling icicles and the first hazard floor stretch forcing a wall ride; the boss skates faster.
- P3 (80-120 s): add rolling spheres and curves tighter; boss "jumps" a gap tile so you have to pulse across a break.
- Fight 100-120 s with three required pulse hits. Dies on the third.

Ice physics note: "low friction" must come from the existing Mover config (which exists in web config), not new numbers.
If the web has no ice-friction number, rule 5 of CLAUDE.md applies: Unity-side config file and say why. [design]

---------------------------------------------------------------------------------------------------------------------

## D. OPEN SOURCE worth reading or copying

Licence rule: copy only MIT / Apache / CC0; others are read-only.

| Repo | URL | Licence | File(s) | Use |
|---|---|---|---|---|
| UnityHFSM | https://github.com/Inspiaaa/UnityHFSM | MIT [read] | src/StateMachine/StateMachine.cs, src/Transitions/TransitionAfter.cs, src/States/ParallelStates.cs | model for our pure-C# boss FSM (we port the IDEA, no UnityEngine) |
| UnitySpline2D | https://github.com/sinbad/UnitySpline2D | MIT [read], archived | Spline2D.cs | distance-parametrised spline, Hermite + arc-length table |
| unity-curves (zapdot / JCMG) | https://github.com/zapdot/unity-curves | MIT [read] | Bezier3DSpline.cs / usage.md | constant-speed distance sampling on a Bezier |
| StateMachineAI | https://github.com/jhocking/StateMachineAI | MIT [read] | Assets/ scripts | patrol/chase/search enemy demo; weak for boss AI, skim only |
| Unity Splines | https://docs.unity3d.com/Packages/com.unity.splines@2.8/manual/index.html | Unity Companion Licence [read] | SplineContainer, Evaluate, GetLength | read-only for us; also lives in UnityEngine so cannot go in Skygrind.Core |
| Runner Track / Forever | R6 / R8 | commercial / closed | n/a | read-only; ideas: lane-based, grid placement, pooling, deterministic seeds [read R6] |

Honest note: I did not find an open-source repo that is specifically "boss AI with telegraphed attacks" with a licence I
could confirm; the search returned generic FSM libraries. UnityHFSM is the best match for the state machine itself.

### D1. UnityHFSM key functions (summary of StateMachine.cs, [read])
```csharp
class StateMachine<TStateId> : StateBase<TStateId> {      // a state machine is a state => nesting
  void Init()            { if (isRootFsm) OnEnter(); }
  void OnEnter()         { activeState = startState; activeState.OnEnter();
                           foreach (t in globalTransitions + triggerTransitions) t.OnEnter(); }
  void OnLogic()         { if (!TryTransitions(global)) TryTransitions(activeState.transitions);
                           activeState.OnLogic(); }          // at most ONE transition per tick (unless ghost state)
  bool TryTransition(t)  { if (!t.ShouldTransition()) return false;
                           RequestStateChange(t.to);  return true; }
  void RequestStateChange(name, forceInstantly=false) {
        if (activeState.needsExitTime && !forceInstantly) pending = name;   // wait for StateCanExit()
        else ChangeState(name); }
  void StateCanExit()    { if (pending) ChangeState(pending.to); }
  void ChangeState(name) { activeState.OnExit(); activeState = states[name]; activeState.OnEnter();
                           activeState.transitions.ForEach(OnEnter); }
  void Trigger(trigger)  { TryTrigger(trigger) || activeState.Trigger(trigger); }   // event propagates down the hierarchy
}
class TransitionAfter { float delay; ITimer timer;       // timer resets on OnEnter
  bool ShouldTransition() { if (timer.Elapsed < delay) return false; return condition==null || condition(this); } }
```
What to lift for SKYGRIND: (1) needsExitTime, so a boss never leaves a telegraph or an attack mid-animation; (2) one
transition per tick, so a boss cannot skip a state in one step; (3) timer transitions with an optional condition (a
phase ends after X steps OR when the player does V); (4) hierarchical nesting (Phase > Attack > Tell/Strike/Recover).
In our sim the timer must be counted in whole 120 Hz steps (`stepsFor`), not a float clock, and nothing may call
UnityEngine. [design]

### D2. Spline / track approach: UnitySpline2D (summary of Spline2D.cs, [read])
```csharp
// sample the curve at uniform t, store cumulative distance
void recalcLength() { for (t = tinc; t <= 1; t += tinc) { pos = Interpolate(t);
        distanceSoFar += Distance(lastPos, pos); distanceToTList.Add(new DistanceToT(distanceSoFar, t)); lastPos = pos; } }
// invert: distance -> t, by linear interpolation between neighbouring table rows
float DistanceToLinearT(float d) { /* find rows i,i+1 with dist[i] <= d < dist[i+1]; return lerp(t[i], t[i+1], ratio) */ }
// cubic Hermite
Vec Interpolate(i, t){ h1=2t^3-3t^2+1; h2=-2t^3+3t^2; h3=t^3-2t^2+t; h4=t^3-t^2;
                       return h1*p[i] + h2*p[i+1] + h3*tan[i] + h4*tan[i+1]; }
// Catmull-Rom style tangent: tan[i] = curvature * (p[i+1] - p[i-1]); endpoints use one-sided difference.
```
This is a (distance -> t) lookup table. Pure math; easy to port to Skygrind.Core (no Unity types needed).

### D3. A track approach for a custom tile / collider system with 40 m recycling [design, informed by D2 and R5]
Two options. Recommended one first.

Option A (recommended): SIMULATE STRAIGHT, RENDER CURVED.
- The sim runs on a straight "track space" (s along the course, lateral x, height y). All colliders (tile boxes, gap boxes,
  hazard boxes) stay axis-aligned in track space, which the web collider already handles bit-exactly; no rotated colliders.
- Curvature is purely visual: a heading function h(s) = integral of seeded curvature k(s); the renderer places each tile's
  mesh at the centre-line position P(s) = sum of unit headings, and rotates the camera's visual yaw by delta_h so the
  player sees a curve. Input steering relative to track-space.
- Pros: determinism; web collider unchanged; tiles recycle trivially (tile index = floor(s / 40)); boss on the same
  straight axis. Cons: visual-only curve means first-person look direction must be corrected (a "track-relative yaw");
  needs a playtest to make sure it does not feel like a sliding floor. Risk: medium.

Option B: real curve with rotated tiles.
- Tile i has a pose (position, yaw) chained from tile i-1 with a seeded curvature k_i (rad/m); centre-line sampled every
  1 m by integrating heading. Tile colliders are boxes in tile-local space and must be transformed to world each step.
- Requires oriented-box support in the web collider (ThreeOctree / TriangleWorld) or a yaw-rotated set of triangles per
  tile. That touches Skygrind.Core and the exactness gates (CLAUDE.md rule 3). Risk: high. Only if A fails.

Tile recycling (both options)
```csharp
// tiles of 40 m; ring buffer of N=6 (240 m visible)
void StepTrack(float s_player) {
  int head = (int)(s_player / 40f);
  while (tileBase < head - 1) { tileBase++; Recycle(tileSlot[tileBase % N], tileBase + N - 1); }   // free the one behind
}
void Recycle(slot, int idx) { var rng = new Mulberry32(seed ^ idx);        // deterministic per tile index, not a shared stream
        slot.kind  = Pick(rng, safe|boost|hazard|cracks|shortcut);
        slot.k     = Curv(rng);                                            // clamp so heading change per tile <= 22 deg
        slot.layout = BuildBoxes(slot.kind, rng); }                        // all numbers from config, not typed
```
Key point: seed each tile from (runSeed, tileIndex) so the course is reproducible and independent of how fast the player
moved. Cap curvature so consecutive tiles never overlap. Reset the seed at run start (CLAUDE.md rule 6). [design]

Constant-speed sampling of the boss along the centre-line uses the distance->t table from D2 (or, in Option A, simply s).
For the rendered bend, a cubic Hermite through tile endpoints gives C1-continuous curves between tiles. [design]

---------------------------------------------------------------------------------------------------------------------

## What this means for SKYGRIND

Numbered buildable rules.

1. Every boss has a card: one verb V, one weapon W, one sentence "what it wants to show you". If a boss can be beaten by
   standing and firing, it fails the card. [design from S1, S2]
2. The vulnerable window is a state the PLAYER causes by performing V (riding a wall, dashing a ring, pulse-launching). The
   boss never opens it on its own timer. This is the single rule that makes it un-Cerberus-like. [design; S8 contrast]
3. Teach V in a safe form in the level before the boss (a wall-ride strip with no damage, a ring to dash through) so the boss
   is an exam, not a lecture. [read S1; snippet S3]
4. Three phases, one new attack per phase, each phase drills the previous one; never a new mechanic in the last, hardest
   phase. [read S1]
5. Telegraph >= 0.5 s; use 0.8 s for the first attack, 0.6 s later. Each attack has a unique decal shape and the same decal
   always means the same effect. Fixed colours: red-orange = danger, cyan = the weak point or the thing you can use,
   magenta only for RIVAL. [read S5; CLAUDE.md]
6. A failed window costs time, not health. No stun-lock and no boss that "stands still to be shot". [read S1 on stun-lock]
7. Boss FSM in `Skygrind.Core`: hierarchical states (Boss > Phase > Attack > Tell/Strike/Recover), `needsExitTime` for
   attack states, one transition per step, timers in whole steps via `stepsFor`, all rolls from Mulberry32. Design after
   UnityHFSM (MIT, read) but no UnityEngine. [read R1; design]
8. Keep the boss as a shape of data: a config table (tell steps, strike steps, recover steps, damage, window steps) read
   through `GameConfig`; new numbers with no web equivalent go in a Unity-side config file named in the commit. [CLAUDE.md]
9. The shield or vulnerability is a single flag derived from player state (is wall-riding on face F; has dashed through
   ring R; hit by pulse launch). Unit-test it in EditMode without a scene. [design]
10. Level 1 = THE LANTERN (wall-ride gate): ring courtyard, orbiting shield plate, orange orbs in readable arcs, 3 phases,
    75-100 s. Cheapest to build with existing verbs; no new physics.
11. Level 2 = THE GLACIER: gap-based elastic band, not time-based. Boss speed = base + k*(gap - target), clamped to +-15 % of
    player top speed; clean skating closes the gap, mistakes open it. [read S13 for the good-band logic; design]
12. Never leave a "speck": HUD gap tape in metres plus a cyan marker ring, FOV push (+8 deg) at top speed, ice streak trails
    (not screen-border lines), boost-pad lines on the best route. [design; border lines banned by GG, CORE.md]
13. Chase attacks are LANE attacks: 3 lanes, a 0.7-0.8 s floor decal, then the wall/icicle/boulder arrives. At most three
    attack kinds in total for the first version. [design; read S5]
14. Make the course by Option A (simulate in straight track space, curve visually) so the web collider and the golden /
    replay gates stay untouched. Real rotated tiles (Option B) only if A fails a playtest, and then only with a
    named task for `Skygrind.Core`. [design; CLAUDE.md rule 3]
15. 40 m tiles in a ring of 6, each seeded from (runSeed, tileIndex); recycle the tile behind the player; cap curvature so
    heading change per tile <= ~22 deg; reset per run. [design]
16. Hazard floor plus side walls = forced wall ride: first a 30 s safe version, then the damaging version, then the chase.
    [design; read S1 on teach-then-test]
17. Catch-up hit: when gap < 12 m show a PULSE-ready flash and give a 1.5 s hit window; a miss costs 20 m of gap, not health.
    Three required hits across the fight; the third kills. [design]
18. Reel scenario per boss (L-014): one Player reel for the verb moment (the window opening) and one for the telegraph set,
    each judged blind and paired (L-001). [CORE.md]

### Ranked level-1 concepts (recap)

| Rank | Name | Verb forced | Phases | Fight |
|---|---|---|---|---|
| 1 | THE LANTERN | wall ride (and wall jump) | 3: one wall / two walls alternate / walls retract in sequence | 75-100 s |
| 2 | THE TURNSTILE | slide + dash | 3: low beam / high+low / two beams, disc faster | 80-110 s |
| 3 | THE KILN-WARDEN | pulse launch | 3: one pillar / two pillars + boss moves / adds | 90-120 s |
| 4 | THE UNSEALER | dash through a ring on a beat | 2-3: ring on beat / two rings / lanes + ring | 60-80 s |
| 5 | THE CLIMBER | wall ride behind a turning boss | 3 | 100-120 s |

Descriptions are in section A. One-line pick: build THE LANTERN for level 1; reserve THE KILN-WARDEN for a later level
that already teaches the pulse gauntlet.

Open questions for GG (not answered by research): does the web config contain an ice-friction number or must it be
Unity-side; is Option A's visual-only curve acceptable in first person; should the Glacier's boss ever turn to face the
player (it is a runner, so I assumed no).
