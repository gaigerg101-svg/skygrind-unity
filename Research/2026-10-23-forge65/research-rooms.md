# Rooms that ask you to move: research for FORGE65 (2026-10-23)

Scout: one agent, about 12 minutes of web reading. Question: how do movement shooters build small, dense combat rooms
that make the player use the verbs, and what checkable rules follow for SKYGRIND's JSON rooms (boxes, ramps, pads,
rails, lights)? GG's complaint, in his words: "random lava pits random pillars that serve no purpose"; "a lot smaller
and more action packed"; "There always really needs to be a route that enemies can climb up" (CORE.md, NIGHT20.md).

How to read the sources: **[read]** means I opened the page and the claim comes from it. **[s]** means the claim comes
only from a search-result summary, and I did not open the page (lower trust). "Derived" means my own arithmetic on
SKYGRIND's `PortExport/config.json`. "Proposal" means my design synthesis, with no source of its own.

## 0. SKYGRIND's own scale (derived from PortExport/config.json, not measured in a Player)

- Walk 8 m/s (`ground.walkSpeed`). Slide 11.6 m/s on the floor (`slide.floorSpeed`), slide-jump boost up to 24 m/s
  (`slide.jumpBoostMaxSpeed`). Dash 24 m/s x 0.25 s = 6 m (`dash`). Speed cap 40 m/s (`speed.maxHorizontal`).
- Jump 1.5 m (`jump.height`). The double jump is v0 x 0.9, so it adds about 1.5 x 0.81 = 1.2 m: about 2.7 m in all
  (apex hang ignored).
- Wall ride: at most 1.2 s at 8 m/s or more (`wallRide.maxTime`, `minSpeed`), so about 10 m at 8 m/s and about 29 m at
  24 m/s. Wall jumps: up to 3 per airtime (`wallJump.maxPerAirtime`).
- Slam: 48 m/s, shockwave radius 5 m (`slam`). Slopes boost a slide (`slide.slopeBoost` 1.6, `slopeRampLength` 8 m).
- So a 30 m room takes 3.75 s to cross on foot and about 1.25 s at 24 m/s. One wall ride at speed spans a whole small room.

## 1. Fifteen checkable rules for one combat room

1. **Small floor: about 30 x 30 m or less for the main fight.** Max Pears (The Division, Cyberpunk 2077) built his
   combat blockout as a 30 m x 30 m space (a course constraint, not an industry norm) [read]
   https://gamedeveloper.com/design/engaging-level-design-for-combat---pt-3 . DOOM's lead level designer Jerry Keehan:
   "You realise how fast you're going when you start to shrink the space." [read]
   https://blog.playstation.com/archive/2017/05/12/classic-levels-deconstructed-the-beautiful-brutality-of-dooms-lazarus-labs
   Check: the walkable bounding box is 30 x 30 m or less; crossing time at walk speed is 4 s or less (derived).
2. **At most three floor planes, each at least 1 m apart.** "Most maps tend to max-out at three different floor planes
   for any given area"; merge small height differences, because they add complexity without new dynamics [read]
   https://book.leveldesignbook.com/process/layout/flow/verticality . "Just by raising an area by 1m" changes the
   encounter (Pears, pt 3 above) [read]. Check: count distinct floor heights (3 or fewer); no two within 1 m of each other.
3. **Every plane has an up route for the player and one for the enemies.** Going up needs "reserved space for stairs,
   ramps, and ladders"; drops are easy, climbs are not (LDB verticality, above) [read]. GG: "There always really needs to
   be a route that enemies can climb up" (NIGHT20.md). Check: a path search from each enemy spawn reaches every plane
   the player can stand on.
4. **No dead ends: every plane sits on a loop.** Pears: build combat loops "making sure that they do not end up in dead
   ends" (pt 3) [read]. In DOOM 2016's Lazarus Labs, the lab's flow was poor until Hugo Martin linked the hallways'
   dead ends into a loop with portals, which "worked immediately" (PlayStation Blog, above) [read]. Check: every
   walkable node is on a cycle (graph check); no cul-de-sac deeper than one dash (6 m).
5. **A foothold on entry, then a one-way drop that commits you.** The Level Design Book lists three footholds: you
   ambush them, they ambush you, or a vista with a one-way entrance, where enemies are visible but out of range, then
   "a short drop or airlock door" [read] https://book.leveldesignbook.com/process/combat/encounter . One-way drops stop
   the player "pull[ing] enemies back through a chokepoint" (LDB verticality) [read]. Check: the entry is above the
   floor, has a view of the whole floor, and has no climb back up.
6. **Spawn enemies in front, and shift the front mid-fight.** Spawning enemies behind the player feels unfair on a
   gamepad, because a 180-degree turn is slow (LDB verticality) [read]; SKYGRIND is pad-first (CORE.md). Hide
   reinforcements "around a corner" (LDB encounter) [read]. Pears: hidden enemies join after the first one dies, and
   changing the front partway through "force[s] movement" (pt 3) [read]. Check: no spawn behind the player's view at
   the trigger; at least one later wave enters from a second side.
7. **Two enemy types for a normal room, three for a hard one.** LDB: 1 type for a tutorial or rest, 2 for a "regular"
   relationship, 3 for complex fights, 5+ is chaos players break into subsets [read] (encounter page). Hakita:
   "The main thing that drives combat scenario difficulty is the amount of enemies and their composition"; making a
   fight harder or easier "is just a matter of removing one tricky enemy" [read]
   https://intothebluesky.com/2021/10/05/follow-up-interview-with-hakita/ . Check: count distinct enemy types per wave.
8. **Low cover; every tall blocker has a job.** "Less cover is better"; Epic: "In general, low cover is better than tall
   cover"; "Too many wide tall cover objects can create confusing maze-like flow"; in arcade shooters "Dodging is often
   a better strategy than standing still behind cover" [read] https://book.leveldesignbook.com/process/combat/cover .
   Heights from Uncharted 4: high cover 1.75 m or more, low 1.0-1.25 m, "not cover" 0.5 m or less (same page) [read].
   Check: each box 1.75 m or taller is also a wall-ride surface, a climb, or a line-of-sight break on the loop.
9. **One idea per room: introduce, develop, twist, conclude.** Koichi Hayashida (Super Mario 3D Land): decide one
   core concept per level, introduce it, build skill, then twist it [s]
   https://www.nintendolife.com/news/2012/04/super_mario_3d_land_director_shares_level_design_inspirations and
   https://www.mcvuk.com/development/video-nintendos-level-design-secrets-in-four-steps . Neon White's early levels
   "were built to teach specific mechanics" [read] https://gameinformer.com/feature/2022/11/25/how-a-neon-white-level-is-made .
   LDB: to push an underused mechanic, "limit competing resources and add relevant props" (encounter page) [read].
   Check: the room names its one verb, and the pieces serving that verb outnumber all the others.
10. **Every piece has an obvious purpose the first time you see it.** Neon White: "every card has to have an obvious
    purpose in the moment that you pick it up"; a Bomb card used only for a shortcut was removed; removing ambiguity
    is "the main work of building levels" (Game Informer, above) [read]. Check: the bot or a playtester touches every
    pad, rail and ride wall on its first clear; delete or move any piece touched 0 times.
11. **Don't demand every skill at once.** Neon White: "we designed the levels to never require players to do all three
    at the same time" (platforming, shooting, card management); "The player always has the right tool for the next
    encounter" [read] https://www.gamedeveloper.com/road-to-igf-2023/-neon-white-and-designing-for-player-creativity .
    Check: a required traversal (one you must do to survive) is never placed where the hardest wave fires.
12. **Readable at a glance.** Hakita: the style makes "visuals are very easy to read at a glance"; "the geometry of a
    level is simple and clean"; the AI is "purposefully simple to make the enemies predictable" [read]
    https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/ . Neon White's
    rule: "The player always knows where to go next" (Game Developer, above) [read]. Check: the exit is lit and visible
    from the fight floor; one landmark light per room (the landmark part is a proposal: not found in a source).
13. **No waiting and no safe camp.** Hakita: make sure "there's no points where you have to just wait for something"
    (follow-up interview) [read]. DOOM's push-forward combat: "動き続けなければ死ぬ" ("if you stop moving, you die")
    [read] https://game.watch.impress.co.jp/docs/news/1113403.html ; many DOOM arenas have no health or ammo crates,
    so you resupply from enemies [s] https://www.gamedeveloper.com/design/how-doom-s-push-forward-design-cured-my-hoarder-syndrome .
    Check: no spot out of every enemy's reach for more than a few seconds; pickups sit on the movement line.
14. **Alternate hard rooms with simple, crowded ones.** Hakita: "constant forward momentum with only occasional
    downtime" (80.lv) [read]; back-to-back hard fights and "you'll probably get exhausted and quit"; he mixes in "simple
    encounters that just have higher enemy counts" (follow-up) [read]. Check: no two hard rooms in a row in a level's order.
15. **Chain the traversal into the fight; a ride crosses the room.** Turbo Overkill (Sam Prebble): "jump-pads linked to
    wall-runs, followed by dashing into another wall-run ... leap-dashing straight into an elevated combat arena";
    "wall-running can be used to reach the other side of a combat arena" [read]
    https://gamingbolt.com/turbo-overkill-interview-inspirations-level-design-weapons-and-more . Titanfall 2's tech-test
    players said maps had too few wall-run spots; Respawn said some final maps would focus on chaining wall runs [s]
    https://gamingbolt.com/titanfall-2-will-have-maps-that-will-heavily-focus-on-chaining-wall-runs . Check: at least one
    ride wall of 10 m or more joins two planes, and the room's entry flows into it without a stop.

## 2. How designers iterate rooms fast

- **Cheap blockout, playtested at once.** "99% of the time, your blockout will not survive a playtest"; keep it "cheap"
  until it is ready to become "expensive"; set the grid to the module size; "Playtest immediately", in engine with "full
  player gravity, collision, and speed"; test massing, metrics and flow [read] https://book.leveldesignbook.com/process/blockout .
  The same page quotes Dirty Bomb's target of 8-12 s from spawn to objective: "if it feels long, it's too long" [read].
- **A kit of pieces, rearranged like Lego.** DOOM Eternal's designers grayboxed and rearranged arenas, traversal puzzles
  and hazards like Lego sets until the layout worked (Shacknews' own account, not a developer quote); Hugo Martin:
  levels went through "years, in some cases, of iteration" [read]
  https://www.shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=6 .
- **Block each encounter alone, then assemble.** Neon White: "each encounter in the level is blocked out in isolation,
  then assembled"; layouts built "using primitive cubes" (Game Developer, above) [read]. One level, Smackdown, went
  through "more than 50 iterations" over about three years; the game shipped 100+ levels and scrapped about twice as
  many (Game Informer, above) [read].
- **One-mechanic prototypes.** Respawn's "action blocks" for Titanfall 2: rapid prototypes that "became the key to their
  design process" [read] https://80.lv/articles/gdc18-designing-titanfall-levels-with-action-blocks ; each explored one
  mechanic or theme [s] (talk: https://gdcvault.com/play/1025105/Designing-Unforgettable-Titanfall-Single-Player).
- **Hakita grows rooms and corrects them:** "I try to course correct things if they end up organically growing into a
  bad place"; S-rank times are his own run, "lower[ed] ... by a bit" (follow-up interview) [read].
- **Measures that exist:**
  - Death and kill heatmaps: Bungie drew Halo 3 heatmaps from thousands of matches [s]
    https://www.coolinfographics.com/blog/2009/1/12/halo-3-heatmaps.html . Microsoft's lab reshaped terrain on a level
    where players kept getting lost [s] https://blogs.msdn.microsoft.com/gduthie/2007/09/03/the-science-of-play/ .
  - ULTRAKILL's style meter is a shipped variety meter. Each hit lowers that weapon's freshness by 0.7, through the tiers
    FRESH 1.5x / USED 1.0x / STALE 0.5x / DULL 0x. A movement multiplier rises "x2.0/s up to 3x" while airborne or
    sliding, and an airborne kill gives +50 [read] https://ultrakill.wiki.gg/wiki/Style . SKYGRIND has its own style
    with diminishing returns (`style.diminish` window 4, repeat 0.6 in config.json).
  - **Not found:** a published designer threshold for "time airborne", "mechanic-use rate" or "damage taken on the
    ground", or a typical number of iterations per room. The proxies above are the closest I found. Proposal: a
    per-room meter of the share of time off the ground, uses of each verb per minute, and damage taken while grounded,
    with a bar set from a good room's baseline, not invented.

## 3. Failure modes: empty boxes, random pillars, pits with no purpose

- **Tall cover scattered about:** tall walls separate "the player from all the action going on over the wall" and make
  "maze-like flow" (LDB cover) [read]. Fix: low cover, and few tall pieces, each with a job (rule 8).
- **A piece with no purpose on the first run:** Neon White cut the Bomb card that only served a shortcut (Game Informer)
  [read]. Random pillars are the same failure. Fix: rule 10's touch count.
- **Height noise:** small height steps "add complexity without adding new dynamics" (LDB verticality) [read]. Fix: snap to
  three planes. Pits with a purpose exist: ULTRAKILL's Cyber Grind pattern "Pits" holds 64 jump pads, so each pit is
  also a launch [read] https://ultrakill.wiki.gg/wiki/The_Cyber_Grind . Fix: a pit is legal only if it holds a way out
  (a pad or ramp) and a reason to go in (an enemy, a pickup, or a ride line over it).
- **Dead ends and backtracking:** DOOM's lab hallways (rule 4); chokepoint pulling without a one-way drop (rule 5).
- **Rooms too big for the speed:** Keehan's "shrink the space" (rule 1). Turbo Overkill's later levels were vertical
  enough that one reviewer got lost, and pacing suffered as enemy counts rose [s]
  https://www.playstationcountry.com/?p=24621 .
- **Too short to explore an idea:** Severed Steel's levels are "extremely short, to the point where they don't get a
  chance to explore their own concepts" [s] https://bit-tech.net/reviews/gaming/pc/severed-steel-review/1 . A room
  needs room for the develop and twist beats (rule 9).
- **Enemies from behind on a pad:** LDB verticality (rule 6) [read].

## 4. Five room archetypes for SKYGRIND's verbs (proposals; sizes derived from section 0)

**A. THE GUTTER (slide, slide-jump).** 20 x 30 m. A sunken lane 4 m wide and 1.5 m deep runs the 30 m length, with 8 m
ramps at both ends (the length of `slopeRampLength`) so a slide down gains speed. The rims are 1.5 m up with low cover
(1 m). The far end is 4 m up (third plane), reached by the ramp out of the lane or two corner stairs. Enemies: melee
husks drop into the lane from side ramps (climb routes both ways); 2-3 ranged on the far plane. It tests slide into
slide-jump: the lane is the only fast line and the ranged sit at its end. Fun: speed is safety, and the lane drops
you among the melee crowd at speed. Basis: rules 2, 3, 9; Turbo Overkill "Speeding down slopes will increase the player's
speed drastically" (GamingBolt, above) [read].

**B. THE CANYON (wall ride and the jump-off boost).** 14 x 32 m. Two parallel walls 6 m tall run the long sides, both
ride surfaces; the floor's middle third is a hazard strip with two pads that throw you back up (no pit without a way
out, section 3). Ledges 4 m up at each end; enemies climb by corner ramps. Enemies: ranged on the far ledge, melee
crossing the floor; a second wave comes in at a side door on the far ledge, in view as you land (a front shift
that keeps rule 6). It tests a full ride (about 24 m at speed covers the strip) and a jump-off onto the far ledge. Fun: the
fastest line across is also the attack line. Basis: rules 8, 15.

**C. THE TOWER (double jump, slam).** 18 x 18 m. A central tower with planes at 0, 2.5 and 5 m (2.5 m is under the
2.7 m double-jump height), a spiral ramp for the husks, and ring cover 1 m high on the floor. Enemies: a melee crowd
on the floor that climbs the spiral; 2 ranged on the top; the top is reached first by a pad from the entry
drop. It tests slam: a top-tier slam lands in the crowd (shockwave 5 m). Fun: you trade the high ground for one big
hit, then climb back up while they follow you. Basis: rules 2, 3, 5; Pears "It is easier to shoot down, than up" (pt 3) [read].

**D. THE PIT RING (pads, air shots).** 24 x 24 m. A floor ring 4 m wide around a 12 x 12 m pit 4 m deep; four pads in
the pit throw you onto a 3 m catwalk on two sides; two ramps lead out of the pit for husks. Enemies: melee spawn in the
pit and climb; flyers or ranged on the catwalk opposite. It tests pad timing and shooting while airborne. Fun: every fall
is a launch, which is the Cyber Grind "Pits" logic (64 pads) [read, wiki.gg above].

**E. THE GALLERY LOOP (grapple and rails, level 4 on).** 26 x 26 m. A gallery 4 m up runs round the room, with a rail on
three sides; grapple anchors on four outer pillars, each also a ride wall (rule 8); the floor has low cover only.
Enemies: ranged on the far gallery, melee on the floor climbing stairs at two corners; reinforcements enter the
gallery from the side opposite the entry. It tests the grapple and the rail as a loop (no dead ends, rule 4); a parry
lane where the rail passes the ranged husks is a proposal. Fun: you never stop, and the loop is the cover.
Basis: rule 4 (Pears; DOOM's lab loop), rule 15.

## 5. "Smaller and more action packed" in numbers

- **ULTRAKILL 1-1 (HEART OF THE SUNRISE):** S rank 4:30 and 76 kills [read]
  https://ultrakill.wiki.gg/wiki/1-1:_HEART_OF_THE_SUNRISE . Derived: about 17 kills a minute, one every 3.6 s. The wiki
  lists 15 rooms, 11 of them with waves; the 15 waves hold 1-14 enemies (median 4, mean 5.7; my count from the page),
  so about 25 s per combat room including the travel between rooms (derived from 4:30 / 11).
- **The Cyber Grind (ULTRAKILL's horde arena, the closest match to SKYGRIND's rounds):** one 16 x 16 tile grid, 38
  official height patterns; each wave is a point budget starting at 10 and growing by 3 + floor(wave / 3) (wave 5 = 28,
  wave 10 = 55); Filth 1 point, Stray 2, Schism 5; commons spawn at 0.1 s intervals [read] (wiki.gg page above).
  **Not found:** the tile's size in metres.
- **Hakita:** the player's mobility is offset by "how many enemies there are in each encounter" (80.lv) [read].
- **Room length targets:** Dirty Bomb's 8-12 s spawn to objective (LDB blockout) [read]; Neon White's Smackdown, a whole
  level, is run in 9.56 s at best (Game Informer) [read]. A 30 m room (rule 1) is 3.75 s on foot in SKYGRIND (derived).
- **Not found:** room footprints in metres for ULTRAKILL, DOOM Eternal or Titanfall 2; enemies per square metre;
  official encounter lengths. Not checked: Dusk, Mullet MadJack, Steve Lee's talks (no time in the budget).
