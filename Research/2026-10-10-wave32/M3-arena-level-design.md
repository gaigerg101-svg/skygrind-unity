# M3: Arena and level design patterns for SKYGRIND (wave 32)

Method: web search and page fetches on 2026-10-10. Web text is treated as data. Numbers marked (inferred) are mine, not sourced. Not found: ULTRAKILL room sizes in metres (the wiki gives shapes, not dimensions). Not read: the GDC videos themselves (only summaries).

## 1. What the sources say

**ULTRAKILL**
- Boss arenas are not one shape. 4-4's V2 arena is "a large octagonal room" with corner torches and a throne on a raised platform (https://ultrakill.wiki.gg/wiki/4-4). 1-4's V2 fight is "a large room" with four breakable pillars and a skylight V2 jumps through (https://ultrakill.wiki.gg/wiki/1-4:_Clair_de_Lune).
- Gabriel (3-2) fights on "a single, circular platform bordered by teeth" over "an endless black pit"; the win condition is knocking him into the pit, and the player air-stalls around the heart and walls (https://ultrakill.wiki.gg/wiki/3-2:_In_the_Flesh). The void edge is the hazard.
- The 5-2 Ferryman's phase 2 moves to the ship roof, "a much smaller arena than before", enclosed by a barrier (https://ultrakill.wiki.gg/wiki/5-2). Boss arenas shrink per phase.
- 5-2 alternates open piers with "small wooden shacks that force cramped enemy encounters"; water hurts the player 20 DP but kills enemies instantly. 4-4 uses a fatal pit and sand pools.
- Cyber Grind: 16x16 cell grid in a void, one height unit is half a cube, stairs and jump pads are cell prefabs, a zap grid kills below (https://ultrakill.wiki.gg/wiki/Cyber_Grind). Its 38 patterns (Skatepark, Labyrinth) show that a plain grid of heights, ramps and pads is enough to carry a whole arena game. This matches SKYGRIND's box-plan pipeline.
- Rooms lock by key/door gates and barriers. The Mannequin guide notes enemies "climb walls", so climb routes are part of the encounter (https://www.ludo.guide/guide/ultrakill/arena-management).

**DOOM**
- id called the arenas "almost like skate parks" with verticality, many routes, ramps, jump pads and swing bars (https://www.gamewatcher.com/2015-07-07-doom-s-arenas-are-almost-like-skate-parks-according-to-exec-producer). Encounters give the player "one more thing than they can handle" (via the staffs.ac.uk level doc search result, secondary).
- Eternal splits tall arenas into "jungle gyms" (distinct floors; mobility interrupts the fight) and "canyons" (open, continuous). The essay says Eternal over-favours canyons and loses the variety jungle gyms gave (https://blog.adamatomic.com/post/613311014289768448/design-of-doom-eternal).
- Push-forward: resources come from kills (glory kill = health, chainsaw = ammo), so the player never has to hide (https://game-wisdom.com/?p=27137). Pickups and kill-rewards are routing tools: they pull the player into danger.

**Titanfall 2, Neon White, Ghostrunner**
- Titanfall 2: "action blocks" are rapid playable blockouts, "not perfect, but playable", reviewed by show-and-tell (https://80.lv/articles/gdc18-designing-titanfall-levels-with-action-blocks). Into the Abyss moves the floor and flips floors into walls; it ends in an arena fight inside the finished prefab village (https://store.epicgames.com/en-US/news/five-levels-that-show-why-titanfall-2-is-one-of-the-best-shooters-ever-made).
- Neon White: enemies are navigation and resources; "the shortcut path cannot be hidden"; never require platforming, shooting and card management at once (https://www.gamedeveloper.com/road-to-igf-2023/-neon-white-and-designing-for-player-creativity).
- Ghostrunner: combat arenas have "several relatively obvious routes" with "a few opportunities to switch lanes", like a race track (https://shacknews.com/article/121101/ghostrunner-review-elegance-precision-and-power). Mullet MadJack, Severed Steel, Turbo Overkill: only store-page and review text found; no design talks (not checked further).

## 2. Circular boss arenas

Why rings work (design reasoning, mostly inferred; no single source):
- Every point on the rim is the same distance from the boss, so no corner is safer. Ranged camping fails because there are no corners and no long sightline.
- A circle has no wall to back into, so strafing becomes a loop. Loops (paths that never dead-end) are the standard strafing structure.
- Pillar ring: pillars at ~60% radius break line of sight for projectiles and give wall-ride and grapple anchors. Concentric rings with gaps force dash or grapple crossings. Rising lava rings and drop-away segments shrink the safe radius per phase (the Ferryman phase shrink; Maggot Larry's outer floor ring breaks away, per a search snippet).
- Anti-camp: a floor that shrinks or drops forces movement, which is what GG's "bosses are campable" complaint needs.

**Box approximation of a circle** (my maths, inferred as a method; check with the web collider, which is triangle-based per CLAUDE.md, so yaw-rotated boxes should be allowed):
- Inscribed n-gon of radius R: the wall sags up to R(1 - cos(180/n)) inside the true circle; each segment is 2R sin(180/n) long, and the facing kink between neighbours is 360/n degrees.
- R = 14 m: n=16: 0.27 m sag, 5.46 m chord, 22.5 deg kink. n=24: 0.12 m, 3.65 m, 15 deg. n=32: 0.07 m, 2.74 m, 11.25 deg. n=48: 0.03 m, 1.83 m, 7.5 deg. n=64: 0.017 m, 1.37 m, 5.6 deg.
- Recommendation (inferred): walls n=48 for a boss ring (sag 3 cm, kink under 8 degrees reads as smooth from first person). n=32 is acceptable if the wall carries a visual trim. Put the segment midpoints on radius R and set boxes at R + half the sag to split the error. Make floors a visual disc with a box floor under it. The old octagon read as "not a circle" because 45 degree kinks are visible: 8 sides is wrong, 16 is still faceted.
- If boxes must stay axis-aligned, a stair-step ring needs cell size of 0.5 m or less to read as round (inferred), which costs far more boxes than 48 rotated ones.
- Grapple rings and pillars on the ring: place on the polygon vertices so their positions are exact.

## 3. Level 5 finale structure

Common pattern (sources are general: final levels "incorporate elements of all previous levels" with "reprised mechanics in more challenging configurations", escalating without throwing everything at once; boss rush is a frequent end beat): remix, escalate, gauntlet, then boss.
Proposed for SKYGRIND (inferred): 6 to 7 rooms, each 12 to 24 m, one verb each, in this order: wall-ride room (foundry), skate room (ice), slide/vents room (kiln), grapple span (rings), then a chained gauntlet with two verbs per room, a no-fight breather with ring-grapple shortcut, then the boss ring. Each early room returns with its hazard turned up (lava now rises, ice now has gaps). Neon White's rule: never require all verbs at once until the gauntlet.

## 4. Ten arena ideas

Legend: `#` wall, `.` floor, `~` lava/pit, `=` ice, `o` pillar/post, `R` grapple ring, `/` ramp or climb route, `B` boss, `S` spawn, `^` jump pad.

**1. The Turntable** (boss ring, 28 m diameter, 48 wall segments). Verbs: dash, wall-ride, grapple, skate-optional.
```
  ..####====####..
 .##...........##.
 #..o....R....o..#
 #...............#
 #o.............o#
 #.......B.......#
 #o.............o#
 #...............#
 #..o....R....o..#
 .##...........##.
  ..####====####..
```
Eight pillars at 60% radius, rim wall for wall-ride, two rings above. Climb: spiral ramp round the rim wall. Not boring: no corner to hide in, the boss ring rotates its fire arc, and each phase drops outer segments.

**2. Drop Ring** (36 m, three concentric rings over lava). Verbs: dash, grapple. Outer ring drops in 8 sectors per phase, inner island keeps the boss.
```
 ~~~~~~~~~~~~~~~~
 ~..............~
 ~.~~~~~~~~~~~~.~
 ~.~..........~.~
 ~.~..oBo.....~.~
 ~.~..........~.~
 ~.~~~~~~~~~~~~.~
 ~..............~
 ~~~~~~~~~~~~~~~~
```
Bridges are 2 m wide, sectors drop on a telegraph. Climb: ramps on bridges. Not boring: the safe radius shrinks, so ranged camping fails.

**3. The Half-Pipe** (ice, 30 x 14 m, banks 5 m high). Verbs: skate, wall-ride, slide.
```
 ####################
 #/================/#
 #/================/#
 #/======oo========/#
 #/================/#
 ####################
```
Enemies climb the end ramps and ride the banks. Speed-skating feel: carve down a bank and snap onto the next. Not boring: speed comes from the geometry.

**4. Gantry Cross** (30 x 30 m, four islands over lava, central tower). Verbs: grapple, dash.
```
 ~~~~R~~~~~~~~R~~
 ~.....~~~~.....~
 ~..S..~~~~..S..~
 ~.....~##~.....~
 ~~~R~~~##~~R~~~~
 ~~~~~~~~~~~~~~~~
```
Pairs of rings link islands; the tower is a ring hub. Climb: stair ramp on each island, one lava-edge walkway. Not boring: crossing is the fight.

**5. The Chimney** (10 x 10 x 24 m shaft). Verbs: wall-ride, dash, grapple. Lava rises 1 m per kill wave.
```
 ##########
 #.../====#
 #.==/....#
 #====/...#
 #../.====#
 #.====/..#
 ##########
```
Top-down view of spiral ledges (each 3 m wide, 4 m rise). Climb: ledge ramps. Not boring: height is the clock.

**6. Figure-8 Rink** (ice, 28 x 16 m). Verbs: skate, slide.
```
 ####################
 #=======##=======..#
 #=#####=##=#####=..#
 #=#####====#####=..#
 #=#####=##=#####=..#
 #=======##=======..#
 ####################
```
Two loops round two blocks, one crossover. Enemies run the loop the other way. Not boring: you snap on and carve; head-on crossings.

**7. Vent Crawl** (kiln, 24 x 14 m). Verbs: slide, dash. Ceiling beams 1.2 m clear, vents blast heat.
```
 ####################
 #S..||..||..||...###
 #...||..||..||..B..#
 #...||..||..||...###
 ####################
```
`||` are low beams to slide under. Climb: a pipe ramp each side. Not boring: forces a slide-dash-slide rhythm.

**8. The Span Run** (bridge, 44 x 8 m, rings overhead). Verbs: grapple, dash, wall-ride on railings.
```
 ~~~~~~~~~~~~~~~~~~~~~~~~
 ~..R....R.....R....R..~~
 ~S......^......^....B..~
 ~..R....R.....R....R..~~
 ~~~~~~~~~~~~~~~~~~~~~~~~
```
Enemies charge down the lane; gaps force a ring grapple. Pads add rhythm. Not boring: a race track with lane switches (Ghostrunner).

**9. Crucible Tiers** (22 x 22 m, three stepped terraces, 3 m each). Verbs: dash, grapple, wall-ride. A jungle gym per DOOM Eternal.
```
 ####################
 #..^..========..^..#
 #.##..========..##.#
 #.##../=====/..##..#
 #...../.....\.....##
 ####################
```
Pads between tiers; ramps on both sides. Enemies climb ramps and flush the camper. Not boring: each tier is its own fight, mobility interrupts it.

**10. RIVAL Octagon** (magenta duel, 20 m octagon, four corner posts). Verbs: dash, wall-ride, slide. An echo of V2's octagon (4-4).
```
   ..######..
  .##......##.
 .#....o.....#.
 #...........#
 #.o.........#
 #.....B.....#
 #.........o.#
 #...........#
 .#.....o....#
  .##......##.
   ..######..
```
Corners are 135 degree wall-ride bays. The rival also dashes and wall-rides, so camping loses. Climb: corner bays have ramps.

## 5. Principles to carry into the plans (ranked)

1. One verb per room until the gauntlet (Neon White).
2. Loops, not dead ends (Ghostrunner lanes, DOOM jungle gyms).
3. Hazard-defined edges (void for Gabriel, water for the Ferryman, lava for us).
4. Round boss rings: n=48, boxes at R plus half sag.
5. Shrink or drop the floor per phase, so camping loses.
6. Enemy climb route is part of the plan, not an afterthought.
