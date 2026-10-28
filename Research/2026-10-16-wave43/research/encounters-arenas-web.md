# Encounters, enemy introduction, burrowing enemies, arena rules (wave 43 research, for G4 G9 G11 G23 G25 G27 G28)

Web research, 2026-10-16. Adds to wave 36 `design-web.md` (Kishotenketsu, Doom chess pieces, Marauder intro, ULTRAKILL wave
sizes), wave 42 `arenas-web.md` (Doom canyons / jungle gyms, ULTRAKILL 0-1 / 2-1 / 4-2, Level Design Book counts and
verticality), wave 40 `enemies-web.md` (deflectable projectiles, Halo shields, attack tokens). Those are not repeated.
"[fetched]" = page read; "[snippet]" = search summary only; "[guide]" = fan guide (weak).

## A. Encounter structure and enemy introduction (G27)

1. **Halo: nested loops, never repeated.** Griesemer: "you have a 3-second loop inside of a 30-second loop inside of a
   3-minute loop that is always different"; vary it "against different combinations of enemies, sometimes against enemies
   that are fighting each other"; the game "waits to see what you will do, how hard you will push, and then it pushes back
   at just the right resistance". [fetched] https://www.engadget.com/2011-07-14-half-minute-halo-an-interview-with-jaime-griesemer.html
   Butcher and Griesemer's paper splits a ~3-minute design scope (race personality, strategic purpose) from a 30-second
   AI scope; each race has a role (grunts flee, elites seek cover when hurt, jackals carry shields). [snippet]
   https://www.jmeiners.com/shamans/papers/ai/the_illusion_of_intelligence.pdf
   - for SKYGRIND: write every arena as three loops: the 3 s verb (shoot / dash / parry), the 30 s problem (which enemy
     first, from where), the 3 min arc (wave 1 teaches, wave 2 combines, last wave twists). No two arenas share the 30 s problem.
2. **Uncharted 2: a spreadsheet of what has been taught.** Naughty Dog tracks, per level, "the mechanics used, the weapons
   given to the player, and the AI classes introduced" and what the player has been trained on, to check the ramp;
   enemies must be able to use the player's routes so a camper is pressured. [fetched]
   https://www.gamedeveloper.com/design/designing-combat-encounters-in-i-uncharted-2-i-
   - for SKYGRIND: a tracked `docs/` table (level x room: verbs required, verbs taught, enemy roles present, new item)
     is the "stop forgetting" tool G12 asks for; every new arena row is checked against it before it is built.
3. **ULTRAKILL: composition over count.** Hakita: "Certain combinations of enemies will force the player to approach a
   situation differently"; bosses are "the peaks with important bosses like V2 and Gabriel being the peaks-among-peaks".
   [fetched] https://intothebluesky.com/2021/10/05/follow-up-interview-with-hakita/
   - for SKYGRIND: escalate by adding a ROLE, not hit points (G27 says the same). One new role per room at most.
4. **Darktide: readability by layer, not by individual.** Enemies are grouped into horde / roamer / elite / special /
   monstrosity and each LAYER reads as one character; "we can't make everyone stand out on a per enemy instance level";
   long-range elites always carry a back attachment; per-enemy unique palettes "made none of the enemies feel special".
   [fetched] https://darktide.gameslantern.com/news/darktide-101-enemy-character-art-dev-blog
   - for SKYGRIND: name 4-5 roles (fodder, flyer, charger, shield / elite, mini-boss) and give each ONE silhouette rule
     (visor shape, back part) that all its variants keep. Encounter rules then speak in roles.
5. **Place each enemy where its ability is hardest to handle.** "Pushers go near ledges, projectile users go on high
   ground, and enemies that force jumps get hazards below the landing zones." [fetched] https://critpoints.net/category/level-design/
   - for SKYGRIND: each role gets a placement rule (flyers over the lava, chargers on the long straight, worms under the
     open floor), so geometry and roster are designed together (G9).
6. **Formal introduce -> master rule (derived from 1-5 and wave 36's Kishotenketsu / ULTRAKILL first meetings):**
   - INTRODUCE: the new role alone, 1-3 of it, in a space where its threat is visible before it can hurt (it enters
     through a door / spawn tell, never already floating; G4). The verb that beats it was taught in the previous room.
   - REINFORCE: the new role plus 1 known fodder role, same room shape, more of it.
   - COMBINE: the new role with one known role whose answer conflicts (flyer + charger: look up vs dodge sideways).
   - ESCALATE: the arena twists (floor turns to lava, walls close, a hazard switches on) with the same roster.
   - MASTER: the mini-boss or boss uses the role's tell as one of its attacks (G23's mini-Icebreaker pattern: show the
     boss's language 2-3 times before the boss).
   Budget: at most 3 roles at once before level 4; 5+ only in finales (Level Design Book counts, wave 42).
7. **Tutorial pacing complaint to avoid.** Doom Eternal's repeated tutorial pop-ups and featureless tutorial rooms: "All it
   does is slow down a game that should be going very fast". [fetched]
   https://www.siliconsasquatch.com/blog/2020/11/21/apparently-conscious-changes-to-doom-eternal-that-cost-time-and-money-many-of-which-were-a-mistake-that-appear-in-the-first-eighty-five-minutes-of-play
   - for SKYGRIND: teaching rooms are real rooms in the level's art, under 30 s each, and each line shows once per save.

## B. Burrowing / worm enemies (G28)

8. **Risk of Rain 2 Magma Worm:** 2400 HP (+720 / level), 20 m/s, immune to knockback and lava; emerges and submerges
   in a pendulum around a point; on emerging "the screen flashes bright orange"; in its 14 s leaping stance it throws
   fireballs (7 m radius) each time it crosses the terrain surface; counter: fight in the open ("cramped spaces make it
   hard to ... see where it will land"), mark it to track it underground. [fetched] https://riskofrain2.wiki.gg/wiki/Magma_Worm
   - for SKYGRIND: the surface crossing is the event: emit the attack (sparks / shards) and the "hit me now" window on it.
9. **Dune: Awakening's sandworm reacts to the player's own vibration** (walking, sprinting, gunfire, vehicles); a meter
   turns yellow -> red as it nears; sand ripples, dust and a low rumble precede it; rock is safe ground; it cannot be
   killed. [guides, snippet only] https://awakening.wiki/Sandworm , https://www.dtgre.com/2025/06/dune-awakening-survive-giant-worms-dodge-shai-hulud.html
   - for SKYGRIND: the worm hunts GROUND CONTACT: being airborne (shotgun jump, wall ride) is the safe state, metal
     plates / platforms are worm-proof, so G28's "react vertically" is built into its targeting.
10. **Binding of Isaac Needle / BitJump burrower:** a lump in the floor precedes the head, and the next emerge point is
    not the last one, so the tell must be re-shown per attack; a dirt trail shows where it travels; "you can try to
    attack it when it hops out, or avoid it entirely". [guide/devlog, snippet only]
    https://www.bamboogaming.net/the-binding-of-isaac/enemies/needle , https://sofaspartan.itch.io/bitjump/devlog/1038059/new-burrowing-enemy
11. **Shadow of the Colossus, Dirge (sand worm colossus):** its eyes surface while it chases; shoot an eye to blind it; a
    blinded Dirge crashes into a wall and is stunned with its body out of the sand; climb to two weak points; stone
    pillars are safe. [guides, snippet only] https://strategywiki.org/wiki/Shadow_of_the_Colossus/Dirge_-_Colossus_10
    - for SKYGRIND: a worm boss loop that fits our kit: tell -> emerge -> a timed hit (shotgun / gauntlet) on the
      exposed head "blinds" or stuns it -> it crashes and lies exposed for a damage window. Pillars / plates = safe islands.
12. **Worm rules (derived):** a ground ripple + glow trail that follows the player's last ground position, 0.7-0.9 s of
    tell at the emerge point (a widening ring + rising pitch event for GG), emerge launches upward (dodged by leaving the
    ground or dashing out of the ring), a 0.4-0.6 s exposed apex where a timed hit triggers the special reaction
    (flip, stun, shell crack), then it dives. Never more than one worm tell on screen before level 4.

## C. Arena rules (G9, G11, G25)

13. **Loops, not lanes or mazes.** One hallway gives one line of sight; loops let players fight for position; good maps
    "control line of sight"; popular CS maps use ~3 big loops with smaller ones between; too many paths make "guess maps"
    where enemies come from any angle. [fetched] https://critpoints.net/category/level-design/ ; one-on-one mappers:
    replace a dead end with a small loop, but do not make the whole map one loop. [snippet]
    https://www.mapcore.org/forums/thread/19395-how-to-make-1v1-maps/
    - for SKYGRIND: each arena: one main loop the player can run without stopping (for slides / rides), 1-2 vertical
      cut-throughs (walls / pads), no dead ends; enemies spawn at loop nodes the player can see.
14. **Looks open, plays linear.** Hakita on 4-2: "a level that seems very open but without actually sacrificing the
    constant forward momentum of a linear level". [fetched] https://intothebluesky.com/2021/10/05/follow-up-interview-with-hakita/
    - for SKYGRIND (G11): rooms chain one way: exits are drops, one-way pads or doors that seal behind; the completed room
      stays visible below / behind but unreachable, which shows progress without backtracking.
15. **Neon White: "Removing ambiguity ... was the main work of building levels"; a first run must "feel really clear".**
    [snippet] https://www.gameinformer.com/feature/2022/11/16/laying-the-smackdown ; Esposito: the limited palette makes
    the game "legible at really high speeds" [snippet] https://gonintendo.com/comments/3622
    - for SKYGRIND (G25): every ramp / wall / platform carries a one-line reason in the level's design row (which verb,
      which enemy, which sightline); a piece with no reason is cut.
16. **Uncharted: widen the corridor horizontally and vertically, and let enemies use the same routes.** [fetched, item 2]
    - for SKYGRIND: G1's "too difficult in the wrong way": a ride section is a corridor; add a second, slower route at
      the floor (over lava with stepping plates) so a miss costs time and a little health, not a full restart.

## Not found
Any shipped shooter's formal written enemy-introduction rule; a GDC talk with Doom Eternal arena numbers; worm-enemy
timings (tell length) from a primary source.
