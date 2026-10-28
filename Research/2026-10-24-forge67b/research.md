# Combat areas that ask for the verb: research for FORGE67b (2026-10-24)

Scout: one agent, web reading plus the repo. GG, 2026-10-24, verbatim: "area deisgn need t be better in alot of levels". His earlier
words: rooms "a lot smaller and more action packed like think about the design of Ultra Kilikow"; "random lava pits random pillars that
serve no purpose"; "you're really struggling to make detailed rooms that really incentivize mechanics"; "There always really needs to be a
route that enemies can climb up"; "more elements that are not just decoration"; "you should be flying through and ... killing the enemies
before you enter the next room in a stylish fashionable way" (CORE.md, OWNER.md, NIGHT67.md).
Builds on, does not repeat: Research/2026-10-23-forge65/research-rooms.md (15 room rules, five archetypes) and
Research/2026-10-23-forge66/research.md (14 verb levers, hand-offs, the "not just decoration" test). This file works at the level of the
AREA: the shape, the solids, the routes, where enemies stand and come from, and the space between rooms, each as a check.

Legend: **[read]** I opened the page (or extracted the PDF's text) and the claim comes from it. **[s]** a search-result summary only
(lower trust). **measured** my read-only script over the repo's plan files today (method in section 0). **derived** my arithmetic on
PortExport/config.json (not checked against Mover.cs). **proposal** my synthesis, no source. **repo** read in this repo.
Grounding: the repo. A Google Drive search for SKYGRIND was attempted (the session had expired); Gmail, Calendar and Linear were not
searched (a game project, not INVERSA work; Linear exposes only its sign-in here). No retrieved page asked for any action.

**The short version.** The shipped levels' fight rooms hold 76 tall blocks (55 of them with the job "cover") against 36 ride walls and 8
pieces of low cover, in a game modelled on one whose producer says "There is no crouching or hiding behind cover". Levels 3-5 hold more
link area than fight area, and 20 of 21 links have no enemy at all. Both are fixable with rows the runtime already reads. The best movement
shooters make the verb win with height, not width; they put ranged enemies up high and melee on the floor in separate spawn sets; they
start waves on the player's progress; they fight in the corridors too; and they change things by double or half.

## 0. What the levels are now (measured)

Method: a node script (read-only, in the session scratchpad) over Design/levels/n52/{foundry,iceline,kiln,span,spire}.plan.json (levels
1-5's plans; level 1 ships n52-foundry's rooms, NIGHT66-REPORT.md section 2) and Design/levels/n65/rooms/*.json (NIGHT65-66's rebuild,
OFF). Fight rooms are `kind: "room"`. A tall hull is `kind: "hull"` with its top at 1.75 m or more (the LDB's high cover); low cover has
its top at 1.3 m or less. Planes are distinct deck / slab tops and ramp rises, at least 1 m apart, with ramps assumed to start at y 0 (not
checked).

| level plan | fight rooms | sizes (m) | tall hulls | low cover | ride walls | pads | fight area m2 | link area m2 | links with a wave |
|---|---|---|---|---|---|---|---|---|---|
| n52 foundry | 6 | 8x19 to 20x32 | 18 | 0 | 6 | 7 | 2540 | 2936 | 1 of 4 (b2, 3 husks) |
| n52 iceline | 8 | 14x18 to 24x32 | 25 | 5 | 13 | 15 | 4832 | 100 | 0 of 1 |
| n52 kiln | 3 | 20x30 to 24x28 | 9 | 0 | 4 | 5 | 1888 | 2780 | 0 of 5 |
| n52 span | 3 | 24x30 to 26x28 | 11 | 3 | 8 | 3 | 2216 | 8560 | 0 of 4 |
| n52 spire | 3 | 24x26 to 24x32 | 13 | 0 | 5 | 6 | 2160 | 3860 | 0 of 7 |
| n65 rooms (level 1, OFF) | 6 | the same rects | 22 | 7 | 8 | 7 | - | - | - |

- **Pillars:** the 23 shipped fight rooms hold 76 tall hulls; their `job` fields read cover 55, line 12, lane 7, circuit 2. Every one has a
  `why`, and 21 of the 76 have 1.8 m or more of headroom above them (possible ledges; whether any route reaches them was not checked). b1's
  rebuild alone has 10.
- **Empty links:** links have 177 m (foundry), 177.5 m (kiln), 256 m (span) and 240 m (spire) of summed long axis, and one wave between
  them. Span's links cover 8560 m2 against 2216 m2 of fight rooms. This is DOOM 2016's "corridor, arena, corridor, arena" (Stratton [s]).
- **Rectangles:** every room is a rectangle by format (Design/levels/LEVEL-FORMAT.md: `rect`, a 2 m wall ring outside it); shape comes
  only from what is placed inside.
- **Height:** 15 of the 17 fight rooms in levels 2-5 have a plane at 4 m or more. Level 1's rebuild does not: n65 a2, a3, a4 and a5 top
  out at 2.5-3 m, about the double jump's reach of roughly 2.7 m (forge65, derived), and only b1 reaches 4 m. n52's a3 and a5 had 5 m
  planes. Whether a verb reaches any plane faster than the stairs do was not measured.
- **Density:** 0.3 to 2.9 enemies per 100 m2 per room (all of a room's waves over its rect area).
- **Two runtime features that no shipped room uses (repo):**
  - (a) **Link waves:** a link with waves spawns them and clears only at alive 0 with its exit reached (Campaign24.cs:897-906); b2 is
    the only one.
  - (b) **Trigger waves:** `triggers33` (design.n33.spawns33, enabled) holds a wave until the player's feet enter a rect given in
    fractions of the room's floor. It uses the trigger's own far-side spots, has an `early` flag that brings the wave in on top of the
    living, and shows a 0.6 s red-orange spawn tell (SurvivalSim.Spawns33.cs:1-12). The config keys triggers by room id and holds only
    f9, f10, k2, i6b, s7 and v8, none of today's rooms. Campaign24.cs:973's comment says arena36 rooms bypass the trigger lines, and n65's
    rooms are arena36. Whether a trigger can fire in them was not checked.
- **Enemy reach (PortExport/config.json):**
  - husk: speed 6 m/s, leap at least 1.5 m up and at most 10 m out
  - swarmer: 10 m/s, 10 HP
  - spitter: range 6-24 m
  - drone: range 12-18 m, hovers at 4-8 m
  - charger: range up to 26 m, charges 30 m at 22 m/s
- **Player reach (derived):** walk 8 m/s, slide 11.6 m/s, dash 6 m, the ride at most 1.2 s (10-29 m), a pad apex of 8 m, the slam's
  5 m shockwave, the punch's 2 m reach. A dash-jump flies about 25 m flat, so it crosses any room (forge66; DECIDE65 D-5).

## 1. Eighteen area principles (rule, source, test, how a room file says it)

Room-file fields named here that do not exist yet are proposals, kept under one `area67` block in the room row. The tools named are:

- a lint (proposal): a node script over the room file and the level JSON, run in seconds with no dotnet
- n67-deco.cjs: the decoration checker (tests a touched, b sight, c launch, d marker)
- n65-air.cjs: the air hand-off graph
- the levelgen report, whose row 4 checks that enemies reach every tier
- the verb pilots: `n65verb` and skip v2
- `n46flow`
- tour65 / iso65 pictures

### A. Space and scale
1. **Size the fight to the verb; win with height, not width.** Hugo Martin (id): "Too big, you actually feel slow. Too small, you're
   frustrated"; "You'd be banging against the walls and just feel like a cheetah at the zoo" [read]
   https://www.gamedeveloper.com/design/q-a-evolving-the-combat-design-of-id-software-s-i-doom-eternal-i- . On ULTRAKILL's early access:
   "V1 can usually clear from one end of a room to another in a single jump", and "opportunities to wall jump, nor just sliding in
   general, are encouraged enough" [read] https://intothebluesky.com/2020/09/19/ultrakill-early-access-review/ . SKYGRIND's dash-jump
   crosses any room (section 0), so width cannot force a verb. Height can: the double jump tops out near 2.7 m.
   **Test:** (a) a plane 3.5 m or higher holds the last wave's ranged enemies; (b) reaching it by the stairs takes at least 1.5 x as long
   as by the verb route (nav path / 8 m/s against n65-air.cjs's edges or the verb pilot's time); (c) the floor's long axis is 32 m or less.
   Baseline: n65 a2-a5 fail (a), b1 passes. **Room file:** `area67.high = {y, verbRoute: [piece ids], stairs: [ramp ids]}`.
2. **A big space is several small arenas, and the fight moves between them.** ULTRAKILL runner Yellow Swerve: "the bigger arenas usually
   end up feeling like several smaller arenas that share the same space"; "I really prefer some of the smaller ones" [read]
   https://persephonescastle.com/2021/04/vintage-to-retro-yellow-swerve.html . Hakita on 4-2: "a level that seems very open but without
   actually sacrificing the constant forward momentum of a linear level" [read]
   https://intothebluesky.com/2021/10/05/follow-up-interview-with-hakita/ .
   **Test:** in rooms of 600 m2 or more (16 of the 23 shipped fight rooms), each wave's spawn centroid sits 10 m or more from the previous
   wave's, and the verb pilot's mean position moves 8 m or more between waves (proposal numbers).
   **Room file:** `waveNodes43` (exists) plus `area67.pockets`.
3. **One spatial law per room, different from its neighbours.** Dan Taylor: "a smart designer won't design a level, he/she will design a
   series of modular, mechanic-driven encounters". His example is Medal of Honor's "Battle Moments", "sections of intense combat
   gameplay, ranging from 30 seconds to 5 minutes", prototyped and then combined [read]
   https://www.gamedeveloper.com/design/ten-principles-of-good-level-design-part-2- (2013-10-06). Hakita: "Certain combinations of
   enemies will force the player to approach a situation differently from how they normally would" [read, follow-up interview]. A-9's
   "Spatial law" (repo). **Test:** each room declares one archetype from section 2. Its signature (planes, pits, pads, ride walls, aspect
   ratio, high-plane share) differs from both neighbours' in at least two fields, and no level uses one archetype more than twice
   (proposal). **Room file:** `area67.archetype`.

### B. Solids and sight
4. **Every tall solid is a surface, a sight-break or gone.** GG: "random pillars that serve no purpose". On DOOM (2016), narrating Marty
   Stratton: "There is no crouching or hiding behind cover"; "The only way to break the line of sight with a demon is to move, move, move"
   [read] https://bethesda.net/en-AU/news/inside-doom's-brutally-fast-single-player-campaign . DOOM's AI uses its cover system "not to
   find cover points to hide behind, but rather to find open positions near cover that provide maximum visibility to the player"
   (Tommy Thompson on the GDC talks) [read] https://80.lv/articles/cyber-demons-the-ai-of-doom .
   **Test:** count the tall hulls that are not a ledge in n65-air.cjs's graph and pass none of n67-deco.cjs's (a) touched on 3 / 10 seeds,
   (b) blocks a ranged sight to the walk line, (c) a launch kill on it. The count must be 0. Run the checker on every piece, with --base at
   the commit before the room file existed, not only on pieces moved since a sha. Baseline: 76 tall hulls, 55 of them "cover".
   **Room file:** a tall hull's `job` is one of ride / ledge / sight / launch; "cover" is retired for pieces 1.75 m or taller (proposal).
5. **Walls set the engagement distance: keep the walk lane inside the ranged band and the verb line out of it.**
   - "The level designer helps determine the engagement distance by placing walls". Chrome's unlimited sight led to "every confrontation
     turning into a sniper battle at extreme distance" [read] https://mostlytigerproof.com/2008/04/25/engagement-distance/ (2008-04-25).
   - The LDB: "Varying the quantity and length of sightlines; different areas should afford different visibility"; "pay special
     attention to corners" [read] https://book.leveldesignbook.com/process/combat/balance .
   - TF2's ranges (TF2Maps via the LDB) are close 256 units or less, medium 1024 or less (where projectiles become easier to dodge), and
     rockets and snipers 2048 [read] https://book.leveldesignbook.com/process/blockout/metrics . At about an inch a unit that is 6.5, 26
     and 52 m (derived), so the spitter's 6-24 m is TF2's medium band.
   - **Test** (static, the checker's sight rays): at least half the walk line's samples are in a ranged spawn's sight at 6-24 m, and the
     verb line's share is at least 0.3 lower, or the verb line passes within 6 m of the enemy (inside its minimum range, in punch reach).
   - **Room file:** `line65` and the walk `proofs` (both exist); the lint compares the two.
6. **No camping spot.** The LDB defines a camping spot as "A position that is easily defended with high tactical value and no obvious
   weaknesses"; it asks "Are all possible enemy sightlines within a single defender's field of view?" and "It should be impossible to
   cover all chokepoints from a single point" [read, balance page]. DOOM: "If you stand still, you will die" [read, Bethesda 2016].
   Liapis, Yannakakis and Togelius score a tile's safety from its distances to reference tiles, with a constant Cs = 0.35 marking it safe
   or contested [read, the PDF's text] "Towards a Generic Method of Evaluating Game Levels" (AIIDE 2013)
   https://ojs.aaai.org/index.php/AIIDE/article/download/12680/12528/16197 . **Test:** a nav cell outside the entry foothold is a camping
   cell if the bearings at which the spawns' nav paths arrive span 90 degrees or less and the first arrival takes more than 3 s (18 m at a
   husk's 6 m/s). The target is 0 such cells (proposal); add forge66 lever 6's EXPOSED-time share for the walk bot.
   **Room file:** none needed (the lint reads the level JSON's nav grid; its form there was not checked).

### C. Routes
7. **Two or three lanes, each a different verb.** Steve Fukuda (Titanfall 2): "Defined paths become the norm: the left, the middle, the
   right"; a "'window pane' effect, where we think in terms of lanes"; away from "drawing lines across the map from any point to any
   point" [read] https://www.gamedeveloper.com/design/-i-titanfall-i-dev-speaks-to-the-value-of-slower-simpler-multiplayer (2016-08-24).
   The LDB: "most maps use only 1-3 lanes"; "Lane asymmetry is important, each lane should feel different"; without a hierarchy a map
   "will function like a maze" [read] https://book.leveldesignbook.com/process/layout/flow/circulation .
   **Test:** count the entry-to-exit routes that share no piece outside the door aprons (nav edges plus air edges): 2 or 3. Each has a
   different main move (floor, ride chain, pad / launch, high gallery), and the floor lane is the slowest by skip v2's bar.
   **Room file:** `area67.lanes = [{id, verb, pieces}]`.
8. **One racetrack loop, sized so the speed carries round it.** id called DOOM (2016)'s arenas "skate parks", "a multi-tiered, open arena
   space" [read, Bethesda 2016]; Martin: "The race car is better, the track has to get better" [read, Game Developer Q&A]. John Haggerty
   (Respawn) asked "Why would you wall-run when you can just run around?" The article's answer: "This is where wall-running's speed boost
   came in" [read] https://www.gamedeveloper.com/design/designer-interview-getting-i-titanfall-i-s-controls-just-right (2017-03-09).
   **Test:** a closed cycle of 35-60 m through at least two planes; at 11.6 m/s that is a 3-5 s lap, and a 6 m/s husk loses about half a
   lap per lap (derived). On it the verb pilot never drops under 8 m/s for more than 0.5 s, every corner over 90 degrees has a ride wall
   or a ramp, and at least one wave spawns beside it. **Room file:** `area67.loop = {pieces, lengthM}`.
9. **Every plane has two climb routes for enemies, from two sides.** GG: "There always really needs to be a route that enemies can climb
   up" (NIGHT20.md). All 38 official Cyber Grind patterns have stair tiles (at least 24, median 68.5 of 256), while 12 of the 38 have no
   jump pad [read, the pattern tables; my counts] https://ultrakill.wiki.gg/wiki/The_Cyber_Grind . DOOM: "imps can jump long or short
   distances" [read, Thompson].
   **Test:** at least two walk routes from the floor to each plane, with their tops 8 m or more apart, and a husk gets from the floor
   spawn onto the plane in 4 s or less (proposal). Baseline: 0-5 ramps per fight room; n52 a8 has none. The levelgen report counts walk
   and drop links only, so husk leaps are not counted. **Room file:** ramps with `job: "climb"` (exists), counted per plane.

### D. Enemies in the space
10. **The routes collide on the verb line.** Güttler and Johansson's "collision points" are "products of the teams mission based routes
    intercepting each other"; where the teams never meet, "the game will be reduced to a race against time" [read, the PDF's text] "Spatial
    Principles of Level-Design in Multi-Player First-Person Shooters" (ITU Copenhagen, 2003)
    https://svn.sable.mcgill.ca/sable/courses/COMP763/oldpapers/guttler-03-spatial.pdf . id's "combat chess: where your job is to figure
    out which demon to smash first and how best to get there" [read, Thompson].
    **Test:** in at least one wave out of two, the nav paths from the wave's spawn spots to the player at the verb's start (the verb
    pilot's log) cross the verb line within 5 m (slam) or 2 m (punch). The walk lane's crossings are fewer (proposal).
    **Room file:** `area67.collide = [{wave, at, verb}]`.
11. **Ranged up high, melee on the floor, in separate spawn sets.**
    - The Cyber Grind: "Prefabs determine the spawn points for enemies", with separate keys for melee (`n`) and projectile (`p`) spawn
      points. A pattern holds 22-110 projectile points (median 45) and 24-167 melee points (median 56.5); the projectile share runs
      0.17-0.72, median 0.45 [read; my arithmetic].
    - Neon White's Smackdown: "The enemy atop the staircase now shoots down at you, so you can see its location" [read]
      https://gameinformer.com/feature/2022/11/25/how-a-neon-white-level-is-made .
    - Hullett and Whitehead's "Sniper Location" and "Gallery", whose variables are height, space, cover and accessibility [s: the slides'
      variable lists read, the definitions not] https://slidetodoc.com/design-patterns-in-fps-levels-kenneth-hullett-jim/ .
    - DOOM's ranged demons "actively maintain their distance" [read, Thompson].
    - **Test:** every spitter and drone spawns on a `highNodes` spot 2.5 m or more up, 12-24 m from the walk lane; every melee enemy spawns
      on the floor or on a climbable plane; ranged enemies are 0.2-0.6 of the room's total.
    - **Room file:** `nodes` / `highNodes` / `waveNodes43` (all exist).
12. **Gather the bodies into the verb's shape.** Neon White: "The enemies are placed so the player has to dash through both"; "The gap is
    much longer to make it clear you need two dashes to get both enemies" [read, Game Informer]. Hullett and Whitehead's "Choke Point"
    (its width and length) [s]. Martin: "Glory Kills are contextual to the walls, too"; "So if a guy's close to a wall, do a Glory Kill"
    [read, Bethesda 2016]. **Test** (the verb pilots' outputs; proposal bars):
    - pierce: 2 or more bodies on at least half the charged shots
    - slam: 3 or more bodies within 5 m on at least one slam per wave
    - launch: a hazard or a drop of 3 m or more within throw on at least half the launch hits
    - parry: the returned glob's path passes within 3 m of 2 or more enemies

    For geometry, a pierce room has a chute 2-4 m wide and 10-16 m long on the husks' approach, where a husk's 0.4 m radius queues
    bodies. **Room file:** `area67.gather = {verb, pieces}`.
13. **Waves come on the player's progress, from a second side, in view.** ULTRAKILL 1-1, the River Hallway: "Entering spawns 3 Drones.
    Crossing the bridge spawns 4 Schisms and 2 Drones". In the Castle Hall, 14 Filth come in after the second set of pillars, then 4
    Strays and 4 Drones after the third [read] https://ultrakill.wiki.gg/wiki/1-1:_HEART_OF_THE_SUNRISE . Hakita: 4-2's idea was "making
    the player get to the solarium building within a certain time window" [read, follow-up]. On DOOM Eternal: "I wish some of the tells
    around enemy spawns ... were a little more pronounced" [read]
    https://www.gamedeveloper.com/design/the-dark-souls-design-of-doom-eternal (2020-08-12).
    **Test:**
    - at least one wave per room waits on a trigger rect on the verb's route (its landing, the far half), not only on the last kill
    - the last wave's spawn centroid is 10 m or more from wave 1's
    - on the verb pilot's run, every spawn is 12 m or more away and inside a ±50 degree view cone at its tell (proposal)
    - the first tell comes 3 s or less after entry

    **Room file:** triggers33 rows (exist, in config, keyed by room id), or a room-file `triggers67` that gen-n65-foundry.cjs copies there
    (proposal); the arena36 bypass is checked first.

### E. Flow between and within rooms
14. **Every link is a fight at speed (incidental combat).** Martin: "we call those spaces incidental combat spaces. Basically, non-arena
    combat spaces"; "We wanted to make every inch of the level enjoyable. Not just the big fights" [read]
    https://gamesbeat.com/doom-eternal-interview-aiming-to-surpass-expectations/ ; "it was just hallway-arena combat" [read, Q&A]. Hakita:
    "The levels are linear to make sure the fast pace is kept up" [read] https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/ ;
    "sometimes even low counts of simple enemies just to give a breather" [read, follow-up]. Titanfall 2's Pilot's Gauntlet sets targets
    you pass at speed, and a missed one adds time [s] https://www.guinnessworldrecords.com/world-records/514353-fastest-completion-of-titanfall-2-gauntlet .
    **Test:** every link 20 m or longer holds 2-6 fodder within 15 m of its fast line. The verb pilot kills at least 75 % of them without
    dropping under 8 m/s for more than 0.5 s, and the link takes at most 1.2 x its empty time. Baseline: 1 of 21 links.
    **Room file:** a link's `waves` and `nodes` (exist). Caution: such a link clears only at alive 0 (Campaign24.cs:901), so a fly-past
    leaves its exit shut. Pick fodder the pass kills, or add a clear-on-exit rule for links (a code change, a default-off row).
15. **The arena changes once, and the change opens a line.** The Cyber Grind: "Each wave changes the layout of the arena" [read]. A-9's
    "State change" (repo). Against it: Luis R. Perez's DOOM Eternal map "DOOM Hunter Base" began as "A multi-stage with transforming
    platforms" and "Later on the space got turned into a static multi platform arena" for "a more traditional DOOM combat flow" [read]
    https://www.lrpgameart.com/level-design . So: at most one change per room, and it adds a route.
    **Test:** in a hard room one collider, hazard or zone state differs between wave 1 and the last wave; the verb pilot touches the new
    piece on 3 / 10 seeds in the last wave; the walk route gets no shorter.
    **Room file:** not supported today for pieces inside a room (Zones.Lock / Open exist for doors; their use inside a room was not
    checked).
16. **Refills sit on the verb's line.** Stratton: "If you find yourself low on ammo, look for an enemy to kill with a Chainsaw"; Martin:
    "Think of it as a utility knife to get more ammo whenever you need it" [read, Bethesda 2016]. "Fodder enemies who will always be in
    arenas that can be killed easily to regain supplies" [read, Game Developer 2020]. DOOM Eternal walkthroughs describe a monkey bar
    swing into a dash refill [s]. **Test:** every pickup lies within 1.5 m of the verb pilot's path and 6 m or more from the walk bot's;
    per clear, the verb pilot takes at least twice the walk bot's pickups (proposal). **Room file:** the pickup rows (a4's carry-over has
    one; its key was not checked).

### F. Readability and method
17. **One shape language; a landmark seen from every plane.** David Shaver (Naughty Dog, GDC 2018, as summarised): "all of your
    affordances should work consistently throughout the game or you will break players' trust"; "climbing edges in Uncharted are always
    horizontal and flat"; a landmark is "a big object that can be seen from far away and from different vantage points" [read]
    https://80.lv/articles/blockmesh-tips-from-david-shaver/ (2020-08-07). The LDB: "Offer multiple sightlines to the same landmark from
    different vantage points" [read, balance page]. Hakita: "it's easy to control the flow of combat and keep track of what's going on just
    based on what enemies are where" [read, 80.lv]. V-3 and A-6 (repo).
    **Test:** every ride wall carries the cyan strip and sits in one size band (4 m tall or more, 8 m long or more: proposal; a ride is
    about 10 m at 8 m/s); no non-ride wall in that band shares its look; the landmark has a clear ray from the entry and from at least two
    other planes. **Room file:** `ride: true` (exists) plus `area67.landmark` (a piece id).
18. **Change by double or half.** The LDB: "On the first pass, double or halve the value. Small differences cannot be felt" [read, balance
    page]. Neon White's gap was made "much longer"; Russell Honor: "I think when those two planks got put down, that's when it went from
    kind of a bad level to just about perfect" [read, Game Informer]. Repo: through NIGHT66's r2 no collider moved in any room (the
    checker printed "0 piece(s) added or moved" for all six, NIGHT66-REPORT.md section 1).
    **Test:** a round's room change counts only if a piece is added or removed, or one geometry number (a plane height, a gap, a lane
    width, a pad count, a chute length) goes x2 or x0.5. n67-deco.cjs's diff lists the change. **Room file:** the piece's `why` names the
    number before and after.

## 2. Arena archetypes (sizes derived from SKYGRIND's numbers; check signatures a lint can see)

forge65 section 4's GUTTER, CANYON, TOWER, PIT RING and GALLERY LOOP are worked examples. This table gives each family's signature, so
that principle 3 can be checked.

| archetype | source | size (derived) | verbs it favours | signature | SKYGRIND now |
|---|---|---|---|---|---|
| RACETRACK (loop) | DOOM's "skate parks" [read]; a circle-strafe loop (Edge review [s]) | about 16x22 m, a 40-55 m loop round a central block 3.5 m or taller that is itself a surface | slide, ride at the corners, pierce down the chasing queue | a nav cycle of 35-60 m with 2 or more ride walls on it (principle 8) | none measured |
| PIT (bowl) | Cyber Grind pattern 09 "Pits", 64 jump pads [read] | about 20x20 m, a pit 8-10 m wide and 3-4 m deep (lava 0.6-1.0 m down, LEVEL-FORMAT.md) | launch into it, slam off the rim (5 m), pads out | pit area 15-30 % of the room; every pit cell within 4 m of a pad or stair; melee spawns at the pit | a5's pits (THE POUR has never taken a body) |
| TOWER | Cyber Grind "Funnel Towers", "Sniper Towers" [read]; Sniper Location [s] | about 18x18 m, a 6x6 m tower topped at 4.5-5.5 m, a spiral or two ramps | ride up a face, pad, slam down | one plane 4 m or higher, covering 15 % of the area or less, holding the ranged; 2 climbs; verb route 0.67 x the stair's time or less | b1 nearest (4 m) |
| GALLERY | Hullett and Whitehead "Gallery" [s]; Neon White's stair shooter [read] | 8-12 x 20-30 m, a deck 3-4 m up along one side | parry (globs come down the lane), ride the deck's face, pierce along the lane | at least half of the lane in ranged sight; the deck reached by a far stair and by the verb mid-way | a4 (8x19, deck 3 m) |
| SPLIT LEVEL | Hullett and Whitehead (height difference, openness) [s] | about 20x24 m, two floors 3-4 m apart, the upper 40-60 % of the area with 30-70 % of its rim open | slam through a hole, launch off the edge, dash-jump across | two big planes; the open-rim share; 2 or more drop holes | none |
| CHUTE (choke) | Hullett and Whitehead "Choke Point" [s]; Neon White's "dash through both" [read] | a funnel 2-4 m wide and 10-16 m long on the husks' approach | pierce (a queue), slam at its mouth, launch back down it | 70 % or more of the melee spawns' nav paths pass through it; the verb's spot looks down its axis | a2 / a8 lack one (a8: "The plain walker beats the pierce, 15.4 s against 18.6") |
| GAUNTLET (the run) | Titanfall 2's Pilot's Gauntlet [s]; ULTRAKILL 4-2 "placed the platforms in a way that'd make it easy to slidehop across" [read] | 10-14 m wide, 40-100 m long, ride walls both sides, the next piece in view | ride, dash-jump, pierce at speed | a link 30 m or longer with targets; time 1.2 x its empty run or less (principle 14) | a1, a7, b2 and the long links of kiln, span, spire |
| HUB (revisit) | ULTRAKILL 1-1's First Room on its return: 10 Filth and 3 Strays [read]; Dan Taylor's two-way spaces with a change on the second pass (the fetch's paraphrase, not a quote) | about 20x20 m with three doors | whatever verb is new by the second visit | one room in two segments of the level order, with a state change | none: A-3's one-way rule needs GG's word |

## 3. How studios iterate area design fast, mapped to an autonomous night

- **Metrics before pieces.** The LDB tells teams to codify shared building metrics, and prototypes them in engine before building [read,
  metrics page]. Night: an `area67` lint over the room files and nav grid, seconds per level with no dotnet, run before any sim round.
  Principles 1, 4, 5, 7, 9, 11 and 17 are static.
- **A kit rearranged like Lego, encounters blocked alone.** id's graybox "can be treated like Lego sets", and its designers ask "what
  [monsters] we have available ... do I have double-jump yet?" [read]
  https://www.shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=6 . Neon White blocks encounters in isolation and
  then assembles them (forge65 [read]). Night: one generator per archetype (section 2) with parameter ranges; builders choose and tune
  parameters rather than placing free-hand boxes; one archetype per room (principle 3).
- **Instrument the playtest; observers stay silent.**
  - Valve's Cabal recorded "the player's position, health, weapons, time" and "graphed them together to find any areas where there were
    problems"; "more than 200 play-test sessions"; "100 or so 'action items'" each; observers "were not allowed to say anything"; "any
    personal opinion you had given really didn't mean anything" [read]
    https://www.gamedeveloper.com/design/the-cabal-valve-s-design-process-for-creating-i-half-life-i- (1999-12-10).
  - Night: the deterministic sim is the 200 testers. Per room, a heat table from the verb pilot and the walk bot: grounded time, damage
    taken by location, kills by verb. Critics see numbers and pictures, never builder notes (NIGHT67 rule 9 already).
- **Watch someone play it, early.** Shaver: "The best thing to do is to get something playable and watch other people play it" [read].
  Hakita sets S ranks from "my standard playthrough of the finished level" [read, follow-up]. Night: par from the verb pilot; the walk bot
  is the uninterested player.
- **Search over variants, judged by simulated fights.**
  - Cardamone, Yannakakis, Togelius and Lanzi evolved FPS maps "to optimize a fitness function that is based on the players' average
    fighting time" [read, abstract] https://www.um.edu.mt/library/oar/handle/123456789/22932 (EvoApplications 2011).
  - Liapis et al. score area control, exploration and balance on a passable-tile graph, and suggest a directed graph where "a cliff which
    an avatar can jump off but not climb up again" [read, PDF text].
  - Night: per room, generate 3 variants of one parameter at x0.5 / x1 / x2 (principle 18). Run each through one skip v2 batch and keep
    the best by MAIN with the guards held. The sim is deterministic, so a pair of runs is cheap.
- **One-mechanic prototypes:** Respawn's action blocks (forge65 [read]). Night: a room's archetype and verb are its whole brief.
- **Course-correct, and cut what nobody used:** Hakita (forge65 [read]); Neon White's cut Bomb card (forge65 [read]); Esposito on an
  enemy behind bars: "It felt so important but it was actually completely unimportant to the concept of the level" [read, Game Informer].
  Night: any piece touched 0 / 10 that also blocks no sight is deleted in the next round (principle 4).
- **Iteration counts that do not fit a night:** Smackdown had "more than 50 different iterations" over about three years [read]. So a
  night spends each room's rounds on its two or three worst principle checks, not on everything.

## 4. The five changes most likely to make SKYGRIND's areas better in one night (ranked)

1. **Fodder in every link: incidental combat.**
   - Why first: links are the biggest fight-free share of four levels (span's 8560 m2 against 2216; 20 of 21 links without a wave). It is
     GG's own sentence about flying through and killing before the next room. It needs no new code, because a link already spawns and
     clears its waves (Campaign24.cs:897-906; b2 does it). It is checked by principle 14 on the verb pilot.
   - Risk: the alive-0 clear rule (pick fodder a pass kills, or a default-off clear-on-exit row). Every changed level needs its Player
     boot (L-043 / L-044), which a sim-only branch C cannot give. Say "not checked in a Player" for those levels.
2. **The pillar purge, levels 1-5.** Each of the 76 tall hulls (55 "cover") becomes a ride wall with the cyan strip, a ledge on a route,
   or a sight-break the decoration checker proves; otherwise it is removed.
   - Why: GG's "random pillars"; DOOM's "no crouching or hiding behind cover"; the change is static, quick and counted (principle 4); and
     every pillar turned into a ride wall adds a line.
   - Risk: removing sight-breaks makes the spitters deadlier, so re-run the walk-bot deaths guard; a ride wall beside a door changes flow
     (principle 7's lane check).
3. **A real high plane in every fight room, with the ranged on it and the verb as the fast way up.**
   - Why: width cannot make the verb win, because a dash-jump crosses any room (D-5). Height can, above the double jump's reach of about
     2.7 m. Level 1's rebuild lost the 5 m planes n52's a3 and a5 had. It is the Cyber Grind / Neon White pattern (ranged up high, melee
     below; principles 1, 9, 11).
   - Risk: a spitter on a 4 m deck at 24 m may dominate the floor. Check the walk-bot deaths and principle 6's camping cells.
4. **Trigger waves on the verb's landing, from a second side.**
   - Why: forward pressure without a timer (ULTRAKILL 1-1); the runtime exists (triggers33, its tell); principle 13.
   - Risk: arena36 bypasses trigger lines (Campaign24.cs:973's comment), so prove a trigger fires in an n65 room with one harness run
     first. Triggers live in config keyed by room id, not in the room files the GATE machinery reads, so a copy step is needed.
5. **Gather shapes per verb: a chute for the pierce rooms (a2, a8), a pit or ledge behind the crowd for the launch rooms (a5, b1), a return
   lane for the parry room (a4).**
   - Why: the rooms where the skip test fails are the ones where bodies are not in the verb's reach. a8's walker beats the pierce, a2's
     stall guard fails, and a5's POUR "has never taken a body" (NIGHT66-REPORT.md). Neon White's "placed so the player has to dash through
     both" is the fix; it is measured on the verb pilots' outputs (principle 12).
   - Risk: a chute's mouth can become a camping spot (principle 6).

Runner-up: two or three lanes with one racetrack loop per room (principles 7-8), held back because it needs a route-graph tool that does
not exist yet. Method rules for every change: the static lint before any sim round, double or half per round (principle 18), and one
archetype per room (principle 3).

## 5. Not found or not checked
- **Not found:** arena footprints in metres for ULTRAKILL, DOOM Eternal or Titanfall 2; the Cyber Grind tile's size in metres (only "One
  unit of height is equivalent to 1/2 of the width of a cube"); any published spacing for Titanfall wall-run walls.
- **Not read:**
  - Adam Saltsman's DOOM Eternal arena taxonomy ("jungle gyms" and "canyons") returned 403 [s].
  - The GDC 2018 DOOM talk and Shaver's talk are videos and were not watched.
  - Ketul Majmudar's "Making Combat Spaces for Spiderman PS4" was not found.
  - Hullett and Whitehead's full paper refused the connection; only the slides' variable lists were read.
  - Jason Blundell on CoD Zombies "training" (a variable "you won't be able to train anymore") returned 403 [s].
  - "Skate parks" as Stratton's 2015 word is [s].
- **Repo, not checked:**
  - the nav grid's form in the level JSON
  - whether triggers33 fires in arena36 rooms
  - ramp base heights (assumed 0)
  - whether husk leaps count as climb routes in the sim
  - the pickup rows' key
  - whether any of the 21 tall-hull tops with headroom is reachable
  - every derived flight number against Mover.cs
- **Proposals:** every threshold marked proposal is mine, to be replaced by a good room's measured baseline once one room passes.
