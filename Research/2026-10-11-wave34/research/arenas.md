# Combat arenas that make players move (research, 2026-10-11)

Web research only, about 30 tool calls. Every claim carries the URL it came from. "Search summary" means the claim came
from a search engine's digest of the page and the page itself was not opened; treat those as weaker. "Not found" means
the budget ran out before a source was found.

## Answer

The games that make players move creatively do it with three levers, not with detail. (1) Geometry that reads at a
glance: ULTRAKILL's developers say the retro look exists so "the geometry of a level is simple and clean, so as soon as you
look at a room it's easy to understand it", and Neon White chose "a clean, low detail look with lots of negative space"
plus a tiny colour vocabulary (red doors are the main path, green ivy says "right way"). (2) Arenas built as vertical
playgrounds with distinct floors joined by movement props (Doom's "jungle gym": floors linked by monkey bars, jump pads,
climb walls, portals), not as open bowls or cover mazes; resources and threats sit on different floors so the player must
cross. (3) A score or resource system that pays for movement: ULTRAKILL's style meter ramps a multiplier to 3x while
airborne or sliding and makes repeated weapons "DULL" (0x), and Doom pays health, ammo and armour only for getting close.
Concrete published metrics for wall-run walls, gaps and pad spacing were not found; the only hard numbers found are
generic FPS metrics and ULTRAKILL's style tables. So SKYGRIND should derive its own spacing from its own movement numbers.

## 1. ULTRAKILL

- Readability is the reason for the art style: "The player can move at a high speed with no issues since the geometry of a
  level is simple and clean, so as soon as you look at a room it's easy to understand it"; the retro look avoids "details
  or decorations distracting or obfuscating" geometry.
  https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/
- Levels are linear "to make sure the fast pace is kept up and the player doesn't get lost or bored, since there's always
  more action right around the corner"; "constant forward momentum with only occasional downtime". Same source.
- Enemy AI is "kept purposefully simple to make the enemies predictable and easy to keep track of"; difficulty comes from
  "how many enemies there are in each encounter". Same source.
- Hakita built the game around game feel first (search summary): https://intothebluesky.com/2020/12/19/interview-with-arsi-hakita-patala/
- Verticality praised by PC Gamer (search summary of Wikipedia): https://www.wikipedia.com/wiki/Ultrakill
- Arena sizes, shapes or room dimensions from Hakita / New Blood: not found.

## 2. Doom 2016 and Doom Eternal

- Arena archetypes (analysis by Adam Saltsman): "Jungle gyms" have distinct "floors", solid planes making separate levels
  of combat, that let you "jump from skirmish to skirmish" and "interrupt" combat with mobility; "canyons" are open and
  chaotic with "less concrete divisions between elevations". Jungle gyms have more obstacles, which "make it slightly
  harder for long range enemy attacks to land". He argues a game needs both, and that Eternal's early canyon-heavy arenas
  lost variety versus Doom 2016. https://blog.adamatomic.com/post/613311014289768448/design-of-doom-eternal
- Hugo Martin compares Eternal's combat to a fast-paced game of chess (search summary): https://en.wikipedia.org/wiki/Doom_Eternal
- Arenas contain monkey bars, boost pads and teleporters that "accentuate the game's remarkably fluid traversal"
  (search summary): https://cogconnected.com/feature/12-things-love-doom-eternal/
- Level designers' building blocks include arenas, climbable walls, monkey bars, hazards such as slime that stops jumping,
  and turrets firing at intervals (search summary): https://www.gamespot.com/articles/why-doom-eternal-incorporates-puzzle-platforming/1100-6473287/
- Doom 2016's push-forward loop: "Glory Kills restore health, Chainsaw Kills restore ammo, there's always an incentive to
  get close to enemies"; "power up placement breadcrumb the players through levels very naturally" (Steven Lumpkin).
  https://www.gamedeveloper.com/design/design-notes-doom-2016-
- Eternal ties each resource to a verb (flame belch for armour, chainsaw for ammo, glory kill for health) (search summary):
  https://en.wikipedia.org/wiki/Doom_Eternal
- Hugo Martin: "The combat between arenas is awesome; it's as good as the arenas". https://shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=6
- An id GDC talk with arena rules (pickup placement, "why no cover"): not found. Doom's "no cover" stance as a stated rule:
  not found.

## 3. Titanfall 2, Neon White, Ghostrunner, Quake, Splitgate

- Titanfall 2 campaign levels were prototyped with "action blocks" (GDC 2018, Christopher Dionne); the summary page
  gives no metrics. https://80.lv/articles/gdc18-designing-titanfall-levels-with-action-blocks
- Some Titanfall 2 MP maps are "very heavily focused in chaining wall runs together" (search summary):
  https://gamingbolt.com/titanfall-2-will-have-maps-that-will-heavily-focus-on-chaining-wall-runs
- Neon White (Ben Esposito): "Player creativity requires proficiency and familiarity with a level"; levels are "fairly
  easy to complete, but they're intentionally misleading. If you follow the obvious path, you won't get a great score";
  never require platforming, shooting and card choice "all three at the same time".
  https://www.gamedeveloper.com/road-to-igf-2023/-neon-white-and-designing-for-player-creativity
- Neon White's Smackdown: 50+ iterations over three years; "If you're ever confused on what to do, the game doesn't feel
  good"; an enemy on top of stairs was made to shoot down so its position marked the path; gaps between challenges were
  lengthened so it was clear whether one action or several was needed; planks added to point at an optional route.
  https://gameinformer.com/feature/2022/11/25/how-a-neon-white-level-is-made
- Ghostrunner: the next thing to do is always kept in the player's field of view; Titanfall 2 and Mirror's Edge were the
  references for fast first-person movement that is not nauseating (search summary):
  https://www.unrealengine.com/en-US/developer-interviews/ghostrunner-is-one-of-the-best-looking-coolest-cyberpunk-games-on-the-market
- Splitgate: early maps were built like Portal puzzles and playtesters found them too complicated for a fast FPS; the
  team made layouts traditional and put portal walls only in strategic spots to reward advanced players (search summary):
  https://www.unrealengine.com/developer-interviews/splitgate-arena-warfare-infuses-elements-of-halo-and-portal-to-create-a-new-breed-of-fps
- Quake 3 / Quake Champions loops and item-timer pulls: only an unreliable page was found; reliable source not found
  (mapcore and Valve's Level Flow page returned 403).

## 4. Concrete metrics

- Generic FPS metrics (The Level Design Book): Unity bounding box 1.0 x 1.8 m, eye 1.5 to 1.7 m, minimum hallway 2.0 m
  ("at least double the player width"), doors 1.25 x 2.5 m; Source: hallway 64 in, doors 56 x 112 in. TF2 combat ranges:
  close 256 units or less, medium 1024, long 2048; max safe fall 256 units.
  https://book.leveldesignbook.com/process/blockout/metrics
- Titanfall pilot speeds (datamined, search summary): run 180.6 u/s, sprint 269.85 u/s, chained wall runs approach about
  twice sprint speed. https://gameranx.com/updates/id/20713/article/titanfall-raw-weapon-stats-decrypted-titan-health-and-speed-revealed/
- Wall-run wall lengths and gaps, ceiling heights, jump-pad / grapple spacing, room sizes, route counts for any of these
  games: not found.

## 5. Visual readability

- ULTRAKILL: simple clean geometry so a room is understood at a glance; no decoration that obscures geometry (80.lv above).
- Neon White: "Doors are always red and always on the main path, green ivy means you're going in the right direction";
  "a clean, low detail look with lots of negative space worked really well"; "We wanted the gameplay to run the show"
  (Game Developer above).
- Mirror's Edge Catalyst Runner's Vision: highlights the next objects in red, plus a red trail; three settings (Off,
  Classic, Full View); when off-track it plots a route "designed to keep your momentum up"; "Players are far more creative
  than developers". https://www.ea.com/amp/news/runners-vision-in-mirrors-edge-catalyst
- Mirror's Edge (GDC, Elisabetta Silli): "Red is an odd choice because it means danger or stop", yet she kept it; for a
  sequel she would improve navigation so players see what is coming instead of relying on red flashes.
  https://www.engadget.com/2010-08-17-overheard-gdc-mirrors-edges-odd-go-here-color.html
- Doom jungle gyms: obstacles between floors also break long sight lines (Saltsman above).

## 6. Rewarding movement

ULTRAKILL style meter (wiki.gg, community-documented values): https://ultrakill.wiki.gg/wiki/Style
- Ranks and points: Destructive 200, Chaotic 300, Brutal 400, Anarchic 500, Supreme 700, SSadistic 850, SSShitstorm 1000,
  ULTRAKILL 1500. Decay per second rises with rank: 15, 18.75, 22.5, 30, 45, 60, 90, 120.
- Freshness per weapon variant: FRESH 1.5x, USED 1.0x, STALE 0.5x, DULL 0x. Each attack takes 0.7 from that variant and
  gives 0.5 to the others; unheld weapons recover 0.1 per second.
- Movement multiplier: rises 2.0x per second up to 3x while sliding or airborne, falls back to 1x in about 0.2 s on landing.
- Movement bonuses: Ground Slam 60, Air Slam 160, Airshot 50, Juggle 20.
- Doom: health, ammo and armour come only from close-range kill verbs (Lumpkin; Wikipedia above).
- Neon White: the score (time) rewards finding the non-obvious route (Game Developer above).
- Damage bonuses while airborne, wall-run or slide-specific damage bonuses in these games: not found.

## 10 arena rules for a small movement arena

1. Read in one look: plain, flat, low-detail geometry with negative space; cut every prop that is not a surface you use.
   Source: ULTRAKILL devs (80.lv), Neon White (Game Developer).
2. Build jungle gyms, not bowls: two or three distinct floors in a small room, so a fight jumps between skirmishes.
   Source: Saltsman, Design of Doom Eternal.
3. Join floors with movement props (pads, bars, climb walls, grapple points), and give enemies their own climbable ramp
   or wall to every floor (GG's rule). Source: Doom Eternal building blocks (Wikipedia, GameSpot summary).
4. Put rewards on a different floor from the threat, and pay rewards only for close or aerial play, so standing still
   starves. Source: Lumpkin on Doom 2016; Doom Eternal resource verbs (Wikipedia).
5. Pay for air time: a style or score multiplier that climbs while airborne or sliding and drops fast on landing, plus
   decay that rises with rank. Source: ULTRAKILL style meter (wiki.gg).
6. Punish repetition: per-weapon freshness that goes stale with use and recovers when unused. Source: ULTRAKILL (wiki.gg).
7. Make the obvious route work but score badly; hide the faster line in plain sight. Source: Esposito (Game Developer).
8. One colour, one meaning, kept for the whole game (Neon White's red doors and green ivy; Mirror's Edge red). For
   SKYGRIND: cyan only on player-usable surfaces, red-orange only on enemies and danger. Source: Game Developer; EA.
9. Use enemies as route markers: place them where their position and fire point along the line you want.
   Source: Smackdown interview (Game Informer).
10. Keep layouts traditional and add special movement surfaces only in a few strategic spots; space challenges so the
    player can tell one action from three. Source: Splitgate (Unreal Engine interview, search summary); Smackdown (Game
    Informer). Derive every gap from SKYGRIND's own jump, dash and wall-run distances, since no published game metrics
    were found (Level Design Book for the base player and door sizes).
