# Small combat rooms where the movement verbs are the fastest and safest route (wave 56 research)

Date: 2026-10-20. Budget: about 12 minutes of web search and reading.
Question: how do ULTRAKILL, DOOM (2016) / DOOM Eternal and Titanfall 2 (plus GDC talks and developers' writing on combat design)
build small arenas that make the player move? Which rules are concrete enough to check?

**Labels.** "Verified" means I read the page (through WebFetch) and the quote is on it. "Reported" means only a search
summary or a secondary source said it; I did not read the primary page (several returned 403).
"SKYGRIND number" marks a threshold I propose for our checks. The source does not give it. Treat it as a starting band
to tune, not a published fact.

**The honest gap.** None of the sources I could read give arena dimensions in metres, sightline lengths, enemy spawn
distances or a "next wave when N remain" number. id's GDC 2018 talk ("Embracing Push Forward Combat in DOOM", Loudy and
Campbell) and Respawn's GDC 2018 talk ("Designing Unforgettable Titanfall Single Player Levels with Action Blocks",
Dionne) are video only; I did not watch them. Published guidance is mostly qualitative. The numbers below come from
Naughty Dog's cover metrics, fan-measured CoD Zombies caps and the Level Design Book's enemy-type table. Everything else
is a SKYGRIND number.

---

## Rules

### R1. Shrink the room so the speed shows
- **Rule:** tight rooms make speed felt. A small room is a feature of a fast game, not a compromise.
- **Source words:** Jerry Keehan (lead level designer, DOOM 2016): "You realise how fast you're going when you start to
  shrink the space."
- **Numbers:** none from the source.
- **Source:** https://blog.playstation.com/archive/2017/05/12/classic-levels-deconstructed-the-beautiful-brutality-of-dooms-lazarus-labs (Verified)
- **Check (level file):** the room's walkable bounding box divided by base run speed (from `GameConfig`).
  SKYGRIND number: a floor diagonal crossed in 2-4 s of plain running, and in under 1.5 s with dash + slide.
  A room that takes longer than 4 s to cross on foot is a corridor or a field, not a combat room.

### R2. No dead ends: every route closes into a loop
- **Rule:** if a room's flow feels bad, join its dead ends into a loop.
- **Source:** in DOOM 2016's medical lab, combat flow through the main room and its two hallways felt poor. Hugo Martin's
  fix was to connect the hallways' dead ends with portals, which made a loop.
- **Source URL:** https://blog.playstation.com/archive/2017/05/12/classic-levels-deconstructed-the-beautiful-brutality-of-dooms-lazarus-labs (Verified)
- **Check (level file):** build the room's walk graph (floor cells plus verb edges: wall-ride, pad, grapple, drop).
  Count the nodes of degree 1, which are dead ends; the target is 0. Every node of the combat floor should lie on a cycle,
  meaning the graph is 2-edge-connected.
- **Check (Player / bot):** the bot completes one full lap of the room without reversing direction.

### R3. Break sightlines often; cover is a move, not a place
- **Rule:** a fast shooter uses cover briefly and then runs again. Rooms need frequent line-of-sight breaks ("ins and
  outs") and little hide-behind cover.
- **Source words:**
  - Martin on DOOM 2016's crate room: its "line of sight breaks, the ins and outs"; the crates are cover and escape routes
    (Verified, the PlayStation Blog piece above).
  - The Level Design Book on Doom / Quake / Unreal: cover is used briefly, dodging often beats standing still, cover is
    "a maneuver" not a tactic, and "less cover is better" in most cases.
- **Source URL:** https://book.leveldesignbook.com/process/combat/cover (Verified)
- **Check (level file):** sample the loop every 1 m. From each sample, find the nearest position that hides the player
  from the room's ranged-enemy perches. SKYGRIND number: a sightline break within 1 s of travel (dash counts) from every
  loop sample.
- **Second check:** no floor area of 4 m² or more sees fewer than 25 % of the enemy spawn points. Larger blind areas are
  camping spots, which GG punishes; see the boss notes in CORE.md.

### R4. No god spot
- **Rule:** split the arena so that no single position sees everything.
- **Source words:** Andrew Yoder: partitioning means "there is no longer a position that can see everything."
- **Source URL:** https://andrewyoderdesign.blog/2019/08/04/the-door-problem-of-combat-design/ (Verified)
- **Check (level file):** cast rays from a floor grid to every enemy spawn point and perch. SKYGRIND number: the
  best-placed grid point sees at most 70 % of them, so the player has to move to keep every enemy in view.

### R5. A foothold at the entry; enemies held at a distance on islands
- **Rule:** put a strong cover position just inside the entry to draw the player into the room. Keep enemies "at a
  distance on islands of territory" so the player has to push forward instead of fighting from the doorway.
- **Source words:**
  - "The easiest solution is to add a foothold of cover."
  - "Convex corners make strong cover."
  - Retreating to the door should only make sense if the foothold is "too exposed."
- **Source URL:** https://andrewyoderdesign.blog/2019/08/04/the-door-problem-of-combat-design/ (Verified)
- **Check (level file):** a full-height block (taller than the eye height in `GameConfig`) stands 4-8 m inside the entry
  (SKYGRIND number). No enemy spawn point is within 8 m of the entry door, and none has line of sight into the entry
  corridor.
- **Check (Player frame):** at the entry frame, at least 3 enemies or their spawn tells are visible beyond the foothold,
  and none is closer than 8 m.

### R6. Trigger the fight past midfield; put bait in the middle; make the entry one-way
- **Rule:**
  - An "enemy ambush" room starts empty. The fight triggers when the player crosses "some sort of midfield threshold".
  - Place items "in the middle as 'bait'".
  - A one-way entrance uses "a short vertical drop or airlock door closing behind them."
- **Source URL:** https://book.leveldesignbook.com/process/combat/encounter (Verified)
- **Check (level file):** the spawn trigger volume sits past 50 % of the entry-to-exit distance. At least one pickup lies
  within the middle third of the room. The entry is a drop, or a door that locks behind the player; the player cannot walk
  back out without a verb.

### R7. Few enemy types per wave
- **Rule:** the number of enemy kinds sets the fight type, and most fights use only a few kinds at once.
- **Source table:**

  | Kinds of enemy at once | Fight type |
  |---|---|
  | 1 | tutorial / rest |
  | 2 | regular |
  | 3 | complex |
  | 4 | brawl |
  | 5 or more | slaughter |

  Source words: "Most of the time, use only a few enemy types at once."
- **Source URL:** https://book.leveldesignbook.com/process/combat/encounter (Verified)
- **Check (level file / spawn tables):** count the distinct enemy kinds per wave. SKYGRIND number: most waves have 2-3
  kinds; 4 or more only in the level's climax room; 1 kind only in a teaching room or a rest beat.

### R8. Simple, clean geometry you read at a glance; every piece has a job
- **Rule:** high speed works because "the geometry of a level is simple and clean, so as soon as you look at a room it's
  easy to understand it with no details or decorations distracting or obfuscating it."
- **Source:** Hakita (ULTRAKILL)
- **Source URL:** https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/ (Verified)
- **SKYGRIND reading:** this is GG's w35 complaint ("random lava pits random pillars that serve no purpose") turned into
  a rule. Every solid inside the combat volume carries exactly one job tag: `foothold`, `los-break`, `loop`,
  `ride-wall`, `pad`, `perch`, `hazard`, `enemy-climb` or `bait`.
- **Check (level file):** 0 untagged solids in the combat volume. Decoration goes outside the play volume or below 0.5 m.
  0.5 m is the "not cover" height from Uncharted 4's metrics (R15).

### R9. More enemies, simple AI: the answer to "movement outclasses enemies"
- **Rule:** keep enemy AI simple and predictable so fights stay readable. Make the threat with enemy count, not with
  smarter single enemies.
- **Source words:** Hakita:
  - "The AI is kept purposefully simple to make the enemies predictable and easy to keep track of."
  - The player "has a lot of movement tools to easily outmaneuver anything, but that becomes less of an issue with how
    many enemies there are in each encounter."
- **Source URL:** https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/ (Verified)
- **Check (sim telemetry):** log alive enemies per 120 Hz step. SKYGRIND number: during a room's fight, the alive count
  stays at or above 4 for at least 70 % of the fight time, and it never falls to 0 between waves.

### R10. Overlap waves: the next wave by count or by the player's act, under an alive cap
- **Rule:** the next wave should not wait for the room to be empty.
  - ULTRAKILL 4-3's big arena spawns a wave whenever the player lights a torch, so a player who wants to "show off" can
    fight several waves at once. (Reported: a Steam developer post seen only in a search summary.)
  - CoD Zombies caps zombies alive on the map at 24 and feeds reserves in as zombies die. (Reported: fan guides and forum
    posts. Per-player scaling, such as 4 players = 42, is disputed.)
- **Source URLs:**
  - https://steamcommunity.com/app/1229490/discussions/0/3115898713376064893 (Reported)
  - https://expertbeacon.com/how-many-zombies-are-in-each-round-black-ops-3/ (Reported)
  - https://www.callofdutyzombies.com/topic/145097-2-players-6-more-zombies-30-or-not (Reported)
- **Check (spawn tables / telemetry):**
  - Every wave after the first has a trigger of the form "alive ≤ N" with N ≥ 2 (SKYGRIND number: N = 2-3 for a small
    room), or a player-act trigger.
  - The alive cap is a config value (from `GameConfig` or the Unity-side config, never typed in code).
  - In telemetry, the gap between the last kill of wave k and the first spawn of wave k+1 is at most 1.5 s.

### R11. Lanes, not Swiss cheese
- **Rule:** organise verticality into a few readable lanes, not routes "from any point to any point".
- **Source words:** Steve Fukada (Titanfall 2 game director): "a 'window pane' effect, where we think in terms of lanes."
  The 3D "Swiss cheese" verticality was simplified so that maps did not become a mess of lines.
- **Source URL:** https://shacknews.com/article/96986/titanfall-2-has-gone-gold (Verified for the quotes; the lanes
  explanation is Reported from search summaries.)
- **Check (level file):** the room has 2-3 named lanes, for example floor, wall-ride and high perch. Each lane is a
  continuous route from the entry side to the far side.
- **Check (Player frame):** at the entry frame, the start of every lane is visible.

### R12. Show the mechanics; tell what to do, not how; never force one technique
- **Rule (Dan Taylor, "Ten Principles of Good Level Design"):**
  - good level design "showcases the game's mechanics through the medium of the level"
  - "players should never be forced to use a singular technique to solve an objective"
  - "the player must never be in any doubt as to what their objective is"
- **Source URLs:**
  - https://www.gamedeveloper.com/design/ten-principles-of-good-level-design-part-1- (Verified)
  - https://www.gamedeveloper.com/design/ten-principles-of-good-level-design-part-2- (Verified)
- **Check (room contract):**
  - Each room names its showcase verb or verbs.
  - verbs37 on that room shows the showcase verb in use, so the room is not solved on foot.
  - At least 2 lanes (R11) reach every objective, so no single technique is forced.

### R13. Teach each traversal piece alone, then combine them
- **Rule:** in DOOM Eternal, each traversal mechanic (dash, monkey bars, climb walls) is introduced on its own, and
  designers combine them later. Graybox layouts are "Lego sets, moved and reconfigured until LDs arrive at an arrangement
  that works."
- **Source words:** Martin: "Levels should challenge me, and be just as engaging to solve, as combat."
- **Source URL:** https://shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=6 (Verified)
- **Check (level file):** for each verb, find the first room in the level that requires it. That room requires no other
  verb the player has not yet met in this level. A room that combines verbs comes after each of its verbs had its own room.

### R14. Push forward: resources come from enemies and sit on the loop, never behind cover
- **Rule:** DOOM 2016 dropped regenerating health, reloads and cover. The player "take[s] what they need from
  adversaries" (GDC 2018, Loudy and Campbell).
- **Source URL:** https://www.gamedeveloper.com/design/video-the-combat-design-of-i-doom-i- (Verified, as a summary of the
  talk. The talk itself is video only and I did not watch it.)
- **SKYGRIND fit:** no passive in-round regen is already on CORE.md's never-again list.
- **Check (level file):**
  - Static health and ammo pickups lie on the loop (R2) at least 1 lane-change away from the foothold (R5). SKYGRIND
    number: on the far half of the loop.
  - No pickup sits in a blind area (R3).
  - Enemy drops supply more than half of the room's healing (telemetry: healing by source).

### R15. Cover heights, if the room uses cover at all
- **Rule:** Uncharted 4's metrics (Emilia Schatz, Naughty Dog):

  | Kind | Height |
  |---|---|
  | high cover | ≥ 1.75 m |
  | low cover | 1.0-1.25 m |
  | not cover | ≤ 0.5 m |

  Epic's Gears map theory says "in general, low cover is better than tall cover" (Reported through the Level Design Book;
  the Epic page returned 403).
- **Source URL:** https://book.leveldesignbook.com/process/combat/cover (Verified, a secondary source for the numbers)
- **Check (level file):**
  - Classify every solid by height. `los-break` and `foothold` pieces are high.
  - Low pieces must be vaultable or slidable, so that they are part of the route, not a stop.
  - Nothing between 0.5 m and 1.0 m. That height band blocks movement without protecting the player. Our heights are scaled
    from the eye height in `GameConfig`, not typed in by hand.

---

## Room recipes

### Recipe A: THE RING (a loop room around a sightline block)
Use it for a level's first or second combat room; it showcases dash and slide.
- **Size:** crossed in about 3 s on foot (R1).
- **Pieces and their jobs:**
  - **Foothold block:** full height, 4-8 m inside the drop entry. It hides the room's far half, so the player has to
    step out (R5, R6).
  - **Central core block:** the `los-break` the loop runs around. No spot sees more than 70 % of the spawns (R4).
  - **The loop:** a closed ring around the core, with no degree-1 nodes (R2).
  - **Low slide slot:** a gap under a beam, about half the player's height, on one side of the ring. Sliding through it
    is the shortcut (R15: a route piece, not a stop).
  - **Ride wall on the outer wall:** on the opposite arc. The fast lane, faster than running the floor (R11).
  - **Health:** on the far arc, off the foothold side (R14).
  - **Bait pickup:** at the midfield; the spawn trigger sits past it (R6).
- **Waves:** 2 kinds of enemy (R7). The next wave comes at alive ≤ 2 (R10).
- **Proof:** the bot laps the room; verbs37 shows slide and dash in use.

### Recipe B: THE GYM (three lanes, three tiers)
Use it for the middle of a level; it showcases wall-ride, wall-jump and pads, and the grapple from level 4.
- **Size:** about 4 s to cross on foot. The tiers are the floor, a mid ledge and a high perch.
- **Pieces and their jobs:**
  - **Floor lane:** where the melee enemies are. It is the most dangerous lane to stand in.
  - **Ride-wall lane:** two facing ride walls far enough apart that a wall-jump chain climbs them. It is the safe way from
    the floor to the mid tier (R11).
  - **Pad:** on the loop's far end, up to the high perch (R13: the pad gets its own earlier room first).
  - **High perch:** where the ranged enemies stand. They pull the player up because shooting them from the floor exposes
    the player to the melee enemies (R9).
  - **Enemy climb route:** a ramp or ledge chain the enemies can climb. This is GG's "There always really needs to be a
    route that enemies can climb up."
  - **Grapple anchor:** above the perch (level 4 and later). It is the third lane's shortcut, not a required step (R12).
  - **Health:** on the mid tier, reachable only by the ride-wall lane (R14).
- **Waves:** 3 kinds of enemy, ranged ones on the perch (R7). Waves overlap by count (R10).
- **Proof:** in telemetry, more than half the time on a tier above the floor; verbs37 shows wall-ride and pad.

### Recipe C: THE CHINA SHOP (a long hall with a charger)
DOOM 2016's Pinky room, "a bull in a china shop": a long room where timing and movement decide the fight. Use it for a
level's climax room or as a short palate cleanser. It showcases sideways dash and slide.
- **Size:** long and narrow. Its length is about 3x its width; the width is 2-3 dash lengths (from `GameConfig`).
- **Entry:** one-way, by a drop (R6).
- **Pieces and their jobs:**
  - **Wall pillars:** pillars along both long walls are the `los-break` pieces that the charger's path cannot cut (R3).
  - **No centre cover:** the middle stays open so the dodge, not a crate, is the answer (R3, R15).
  - **Ride walls:** on both long walls, the escape lane above the charger's line.
  - **Pads:** a pad at each end turns the hall into a loop. Down one side on a wall-ride, back on the floor (R2).
  - **Pickups:** health and ammo sit at the ends, so the player commits to a run (R14).
- **Waves:** 1-2 chargers plus 1 support kind, 2-3 kinds at most (R7). The next charger spawns at alive ≤ 2 (R10).
- **Proof:** in telemetry, dash or slide fires within 0.5 s of each charge tell; deaths by charge in the bot band.

---

## Sources read
- PlayStation Blog, "Classic Levels Deconstructed: Lazarus Labs" (Martin, Stratton, Keehan), Verified:
  https://blog.playstation.com/archive/2017/05/12/classic-levels-deconstructed-the-beautiful-brutality-of-dooms-lazarus-labs
- 80.lv, Hakita and Oshry Q&A, Verified:
  https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/
- Level Design Book, Combat / Encounter / Cover, Verified:
  - https://book.leveldesignbook.com/process/combat
  - https://book.leveldesignbook.com/process/combat/encounter
  - https://book.leveldesignbook.com/process/combat/cover
- Andrew Yoder, "The Door Problem of Combat Design", Verified:
  https://andrewyoderdesign.blog/2019/08/04/the-door-problem-of-combat-design/
- Dan Taylor, "Ten Principles of Good Level Design", parts 1-2, Verified:
  - https://www.gamedeveloper.com/design/ten-principles-of-good-level-design-part-1-
  - https://www.gamedeveloper.com/design/ten-principles-of-good-level-design-part-2-
- Shacknews:
  - "Hell Razer: The Making of DOOM Eternal", page 6, Verified:
    https://shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=6
  - "Titanfall 2 has gone gold" (Fukada), Verified: https://shacknews.com/article/96986/titanfall-2-has-gone-gold
- Game Developer, a write-up of "Embracing Push Forward Combat in DOOM" (GDC 2018), Verified as a summary:
  https://www.gamedeveloper.com/design/video-the-combat-design-of-i-doom-i-
- 80.lv, "GDC18: Designing Titanfall Levels with Action Blocks", Verified; no level rules in it:
  https://80.lv/articles/gdc18-designing-titanfall-levels-with-action-blocks

## Not read (403 or video only)
- Adam Atomic, "Design of Doom Eternal": https://blog.adamatomic.com/post/613311014289768448/design-of-doom-eternal
  - It returned 403 and is Reported through a search summary.
  - It sorts tall arenas into "jungle gyms" (many floors, obstacles that block long-range attacks) and "canyons" (open).
- GDC Vault talks, video only and not watched:
  - "Embracing Push Forward Combat in DOOM": https://gdcvault.com/play/1024940/Embracing-Push-Forward-Combat-in
  - "Designing Unforgettable Titanfall Single Player Levels with Action Blocks":
    https://gdcvault.com/play/1025105/Designing-Unforgettable-Titanfall-Single-Player
- Epic, Gears multiplayer map theory (403): https://docs.unrealengine.com/udk/Three/GearsMultiplayerMapTheory.html
