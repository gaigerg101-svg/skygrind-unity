# NIGHT47 builder L: research notes and baselines (round 0, 2026-10-18 22:53-23:55)

Builder L owns level 4 (THE LOOM, its new arena, n47-span) and level 5 (THE CROWN, the level's sense, the boss's flame, n47-spire): G36-G43
(boss parts). Each research line says Verified (the page was fetched and read, by L or by L's research subagent tonight) or Reported (a
search snippet or a secondary source only). Numbers marked "measured" come from the harness tonight; "read" from the code / config.

## 1. Web research (tonight)

### Grapple, swing and slow motion in boss fights
- Sekiro (Miyazaki, Game Informer 2019-01-11): "The traversal options allow much more dynamic movement within the boss arena"; bosses were
  designed around them. https://gameinformer.com/2019/01/11/sekiro-shadows-die-twices-new-combat-options-means-bosses-are-a-little-different
  (Verified)
- Same interview: not every boss should allow every approach, or it gets "monotonous". Same URL (Reported: the fetch did not show the line)
- Doom Eternal (Hugo Martin, PCGamesN): "The fact that you can't meathook onto every single surface in the game is what will make the meathook
  feel smart"; the hook works on enemies only. https://www.pcgamesn.com/doom-eternal-intelligent-design (Verified)
- Doom Eternal (id, Stevivor): "if you build a faster race car, you've got to build a longer race track"; monkey bars + hook let you
  "choreograph your own fight scene". https://stevivor.com/features/interviews/id-software-on-the-slayer-2-0-in-doom-eternal (Reported)
- Max Payne (Scott Miller's note, Time Extension 2023): "We need to make the slomo stuff part of gameplay and not just visual. Max needs to be
  able to activate it!" https://www.timeextension.com/news/2023/03/max-payne-producer-reveals-how-bullet-time-mechanic-was-born (Verified)
- Bayonetta's Witch Time: a few seconds of slow motion earned only by a last-moment dodge; a near miss becomes a free punish.
  https://kotaku.com/nothing-beats-a-great-gaming-dodge-1827288664 (Reported)
- Ghostrunner's Hel: parry until a bar fills, then one strike; she jumps platforms and fires, dodged with mid-air slow motion.
  https://progameguides.com/ghostrunner/ghostrunner-how-to-defeat-hel/ (Reported)
- Shadow of the Colossus (Ueda, glitterberri interview): "The design of the Colossi is closely related to how you go about defeating them";
  when a colossus changed, its arena had to change too. https://glitterberri.com/extended-developer-interview/the-graphic-designers-tell-it-straight/ (Verified)
- Spider-Man 2 (GDC 2024 session page): lean into core combat, avoid one-off lock-and-key mechanics and turning off player abilities.
  https://schedule.gdconf.com/session/refining-the-rogues-gallery-building-boss-fights-in-marvels-spider-man-2/900253 (Reported: 403)

### Phases and telegraphs
- Mike Birkhead (Game Developer): reaction sweet spot ~1-1.5 s; past ~2 s players "start counting"; no two attacks share a motion or sound;
  vulnerability windows fit a full combo plus the travel to the boss; "show what matters, and only what matters"; "prove to the player how
  awesome they are". https://www.gamedeveloper.com/design/opinion-boss-design---tips-from-a-combat-designer (Verified; also NIGHT47.md's digest)
- ULTRAKILL's Gabriel: phase 2 recolours (silver-gold to red-gold) and strengthens the same attacks.
  https://ultrakill.fandom.com/wiki/Gabriel,_Judge_of_Hell (Reported tonight: 402; the lead's digest has it Verified)
- ULTRAKILL: parryable attacks show a yellow glint. https://steamcommunity.com/app/1229490/discussions/2/3734079155590153286 (Reported)
- Sekiro's Guardian Ape: phase 2 is effectively a new creature with new tells. https://www.thegamer.com/most-unique-fights-in-sekiro-recycled/ (Reported)

### Fire VFX (the boss flame, G43 boss part)
- Company of Heroes flamethrower (simonschreibt): the stream is moving mesh geometry, not a pile of particles; sparks stretch with speed, collide
  with the ground and start burns + smoke where they land; red toward the tip. https://simonschreibt.de/tag/flamethrower/ (Verified)
- Fire FX breakdown (Charles Boury): five layers: a ground decal, a shader fire mesh, embers, smoke, a light; three greyscale textures combined
  and posterized with a time offset; ~180 particles at its largest. https://charlesboury.fr/articles/fire-fx-breakdown.html (Verified)
- Riot "Clarity in League": attention matches importance; a projectile shows its direction even still (a blurred tail); a ground marker must
  never suggest more room to dodge than the attack gives. https://www.leagueoflegends.com/en-us/news/dev/clarity-in-league/ (Verified)
- Doom Eternal's flame belch: a short burst that sets enemies burning; burning enemies drop armor (NIGHT47.md's digest, Verified by the lead;
  not re-read by L tonight).

### A readable route (G42)
- Disney's "wienie" (above-the-garage): a lure toward a destination; tall pointed landmarks pull the eye up; it fails when the view is blocked.
  https://www.above-the-garage.com/wienies/ (Verified)
- Naughty Dog's David Shaver (80.lv): use a distant landmark in blockout; breadcrumbs "can be used as a crutch early", add them late.
  https://80.lv/articles/dealing-with-navigation-players-behavior/ (Verified)
- Breath of the Wild's triangle rule (80.lv): a triangle obstacle offers over-or-around and hides then reveals; rectangles hide completely.
  https://80.lv/articles/the-design-secrets-of-breath-of-the-wild/ (Verified)
- Scott Rogers GDC 2009: weenies, "picture spots", altitude changes for drama. https://boingboing.net/2009/03/27/gamedesign-lessons-f.html/amp (Reported: 403)

### Spider / weaver animation (G37)
- Procedural spider (80.lv, Godot 4.5): each leg raycasts a future foot spot from velocity; past a threshold the foot steps "in a nice arc";
  diagonal pairs alternate; body height = the feet's average + a sine hover. https://80.lv/articles/ik-driven-procedural-spider-locomotion-in-godot-4-5 (Verified)
- Sumo Digital's Spyder: purely reactive IK is not enough; legs adjust arcs before a surface change.
  https://www.unrealengine.com/en-US/tech-blog/sumo-digital-shares-how-it-created-spyder-s-sophisticated-animation-systems (Reported: 403)
- Rain World (Unity blog): creatures built in code; a body of two spherical chunks at a fixed distance, limbs drawn on top.
  https://unity.com/en/blog/exploring-procedural-design-rain-world (Verified)
- David Rosen, GDC 2014 "An Indie Approach to Procedural Animation": fluid motion from very few keyframes.
  https://www.gamedeveloper.com/design/video-an-indie-approach-to-procedural-animation (Reported)
- NIGHT47.md section 3's animation rules (anticipation, overshoot, follow-through, a 60-100 ms flinch spring, closed-form springs from NIGHT5).

### What L takes from it
1. The BEND as an earned reward (Witch Time, Max Payne's "part of gameplay"): THE LOOM's damage happens INSIDE a BEND, and its right moves
   refill the meter, so the slow motion is the punish, not decoration.
2. The hook is smart because it is selective (Doom's meathook): in THE LOOM's arena the lit knees and the few rings are the only holds; no
   floor-to-floor walk.
3. Arena and boss as one design (Ueda): THE LOOM's arena changes with each phase because the boss changes (it cuts its own frame).
4. Tells 1-1.5 s, distinct motion each (Birkhead); a phase is the same attacks, stronger, plus a recolour (Gabriel).
5. The flame as layered mesh + decal + embers + smoke + light, the floor marker exactly the hit width (Riot), the burn as a state.
6. Level 5's route: one tall lit wienie on the level's axis, seen through the rooms' openings from the start and from each door.

## 2. Baselines (measured tonight, before any change)

### THE LOOM today (read: Core/Enemies/Loom37*.cs, design.n37.loom37 / n39.boss39.loom / n40.loom40 / n41.sweep41)
- The arena: o3 4.10 THE CANYON, 44 x 140 m (x -39..5, z -390.5..-250.5), no floor (void to y -24), three tower pairs (5 x 14 m, tops y 8,
  ride faces), 13 anchor nodes (y 8-15), start / end ledges; no-ride walls 5 of 14 in leg 1, 10 in legs 2-3 (a 1 s blink); written by
  gen-n37-span-boss.cjs + gen-n40-span.cjs; gen-n46-span.cjs copies o1-o3 unchanged (read).
- The rule: three legs = three stations along the canyon (the fight travels); hook a lit knee of the leg's pair (a moving grapple node), pull
  away 7 m (tearDistM) to tear it (Stagger 3); the meter per leg 6 / 7.5 / 7.5 (two tears; legs 2-3 also one parried spool, 1.5); at 0 the
  spinneret opens CYAN for 2.2 s (kit.windowSeconds); outside it every hit chips at 0.02 (kit.chipScale); the window's close moves to the next
  leg whatever the HP (legs are places); in leg 3 it falls. Pool 750 (hp) x the fight scale (read).
- Attacks (read; wind-ups measured as boss37Tell seconds): THREAD LASH red 1.2 s (active 0.35, 30 dmg, a 2.5 m line), SPOOL SHOT yellow
  1.2 s (punch it back; 25 dmg), FLOOR SWEEP red 2.0 s (sweep41; 15 dmg; answer: height / a hook / a ride), WEB WALL red 1.4 s (legs 2-3,
  the camp habit only). Three in practice, four on paper; no attack is new between legs (only the sweep's height and the no-ride walls).
- The existing pilot, `n47loom base` (Tools/SimHarness/Harness.N47.Loom.cs, n46-span, the config's rows, Loom37Pilot, 10 seeds, measured):
  - skilled: killed 10 / 10, median 48.7 s, p90 51.3, min 44.5; taken median 45 hp, deaths 0, falls 14 in 10; windows median 3; damage dealt
    in the window median 616, chip median 13 (2 % of the dealt: "you can barely damage it" outside the window).
  - casual (L-058): killed 9 / 10, median 55.5 s, p90 60.9, min 48.8; taken median 160 hp, 1 death, falls 53.
  - tells per fight (skilled): lash 3.3, spool 4.9, sweep 1.1, web 0 (never picked: nobody camps); casual: lash 3.1, spool 7.1, sweep 1.9.
  - the BEND's uses (bend34Start per leg, skilled): median 5 / 4.5 / 6 (min 4 / 4 / 6); latches (RopeAttach) 6 / 5 / 6. Every latch is a
    bend, but all are SHORT (1 s at 0.25x) after the first: the meter is full once a run and refills only by kills (0.25 each), and THE
    LOOM's fight has no kills (read: Core/Bend34.cs, design.n34.bend34). So today the BEND is present but incidental: 1 s of slow motion
    nobody needs.
- Not measured: the human-model pilot (n47human, the lead's, due ~23:45). G40's ">= 3 BEND uses per phase" and the 90-150 s band are its.

### THE CROWN today (read: Core/Enemies/Crown37*.cs, design.n37.crown37 / n38 / n39.boss39.crown / n40.crown40)
- The arena: v11 5.11 THE SPIRE, x -44..24, z -323..-255, open sky; the core column 8 x 8 m, y -60..0 (ride panels -50..-29); leg 1 the
  summit (y 0) + 12 orbiting 5 x 5 m platforms on three rings (r 13 m, tops -8 / -15 / -22, 3.5 m/s); leg 2 the gallery (8 slabs spiralling
  -30 -> -51); leg 3 the plaza (y -60, 8 sectors that drop) (read, via the plan and Orbit37).
- The rule: a ring of gem blades (r 3 m); pass THROUGH it airborne (a jump-dash; passRadius 2.4) to break a gem (1, or 2 with the TORCH burning);
  guns do nothing to the gems outside the CYAN window (the view says BLOCKED); legs change at 13 and 6 gems left of 20 (pool 2000); leg 3 ends
  on a KESTREL charge or a THUNDERCLAP slug through the opening (read).
- Attacks (read; wind-ups measured): BLADE RING red 1.2 s, GEM SHARD yellow 1.0 s, BEAM CUT red 1.4 s (legs 1-2: the piece you head for drops),
  LOW FIRE red 1.0 s (the air habit: a red shard under your path, forced after 6 s airborne), PLAZA DROP red 1.2 s (leg 3). Five attacks.
- **There is no boss flame today.** "lowFire" is a red shard, not a flame (read: Crown37.cs:13, Crown37View.Crown.cs:5). The flamethrower
  GG saw is the TORCH (builder F's). G43's boss part therefore needs a NEW flame attack (card below).
- The existing pilot, `n47crown base` (Tools/SimHarness/Harness.N47.Crown.cs, n46-spire, Crown37Pilot, 10 seeds, measured):
  - skilled: killed 10 / 10, median 36.3 s, p90 43.0, min 34.3; passes 9; taken 35 hp; falls 21 in 10 fights; deaths 0; leg 2 at 13.5 s,
    leg 3 at 23.2 s.
  - casual: killed 10 / 10, median 58.4 s, p90 74.0, min 45.4; passes 14; taken 113 hp; **falls 103 in 10 fights (10 a fight)**; deaths 7.
  - Read: the fight is mostly falling. A casual player falls ten times in a minute; each fall restarts the leg at its checkpoint (-5 hp).
- Not measured: the human-model pilot.

### Level 5 today (read: Design/levels/n46/spire.plan.json, gen-n46-spire.cjs, docs/ROOMS.md)
- v1 THE LANDING (the TORCH) -> v2 THE NAVE / FALLEN BELL (fight) -> v3 BELFRY I (17 m jump-dash gate, the lesson) -> v4 THE STEP UP (18 m,
  +1 m) -> v5 THE RINGERS' STAIR (short fight) -> v6 THE ROPE CHOIR (fight) -> v7 THE LONG BELL (20 m gate) -> v8 THE VESTRY (rest) -> v9
  FALLING STONES (35 m pit, stones drop 2 s after landing; its lesson prompt is EMPTY) -> v10 THE RING (a spinning ring over a 10 m gap) ->
  v11 THE SPIRE (boss). All doors on x -12..-8, y 0; the level runs straight down -z; ~370 m to the boss door.
- **No landmark is visible from the start or from any room**: v1-v10 are roofed (9-16 m), no skybox or far object; the only landmarks (the bell,
  the organ) are inside their rooms; the boss (the spire's core at (-10, 0, -289)) is 402 m from the spawn and never seen before its door (read).
- Measured (`n47spire view`, Tools/SimHarness/Harness.N47.Spire.cs): a light column at the core's axis (-10, -289), y 10-300, seen from the
  eye at v1's spawn and v2..v10's entry spawns: **0 / 10** rooms (every point in view blocked by the roofs); the control v11 (open sky) sees it.
- The name says "spire" and the level never climbs: every door is at y 0 (levelgen doors carry no height), then the boss descends 60 m.
  What a player sees: a church, then gates, then a church, then gates, then a pit, then a ring, then a hole in the ground. That is "the level
  doesn't make sense" (GG): no premise ties the rooms, no goal is seen, the spire is never shown.
- Flow (NIGHT46 F1, n46flow, n46-spire): the level's playable rooms 175.9 / 170.0 s, stall 15.5 / 15.0 % (inside F1's band) (read from ROOMS.md).
