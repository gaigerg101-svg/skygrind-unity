# Arena design that changes play: web research (2026-10-14)

Method: WebSearch + WebFetch only. Fetch summaries were made by a small model, so quotes are as returned, not checked against the page.
Blocked (HTTP 402/403, not read): Fandom ULTRAKILL 1-1 and 7-1, doomwiki Super Gore Nest and its walkthrough, GameSpot, GameStar, Prima Games.
Not found: any GDC talk on DOOM Eternal arena layout, any video timestamp I could verify, full-URL screenshot links.

## 1. DOOM 2016 and DOOM Eternal

- Structure: "corridors linking big wave-based arenas"; tall arenas come as "jungle gyms" (distinct floors, jump from skirmish to skirmish) and "canyons" (open, continuous combat, less division between elevations).
  https://blog.adamatomic.com/post/613311014289768448/design-of-doom-eternal
- Jungle-gym obstacles "make it slightly harder for long range enemy attacks to land, which reduces the overall ambient damage-soak". Eternal's first 3-4 hours are mostly canyons, which the author calls "slightly same-y". Same URL.
- Pickups as routing: ammo, health and upgrades are placed to pull players to new places, "using coins in Super Mario to lure players out" (the author credits Vanquish). Same URL.
- Resources are also tied to kills, not only pickups: glory kill for health, chainsaw for ammo, flamethrower for armor. This forces close-range choices.
  https://www.gamedeveloper.com/design/the-dark-souls-design-of-doom-eternal
- DOOM 2016 "push forward combat": no regen health, no reload, no cover; "take what they need from enemies". Talk: Kurt Loudy and Jake Campbell, GDC 2018.
  https://gdcvault.com/play/1024940/Embracing-Push-Forward-Combat-in
  (Free on the GDC YouTube channel per https://www.gamedeveloper.com/design/video-the-combat-design-of-i-doom-i-; I did not get the video URL or timestamps.)
- A reviewer says power-up placement "breadcrumb[s] the players through levels very naturally". Jumping up ledges is "very forgiving". https://www.gamedeveloper.com/design/design-notes-doom-2016-
- Eternal traversal between arenas: double dash, monkey bars, climbable rough walls, slime that stops jumping, interval turrets.
  Hugo Martin: "skill-based traversal puzzles ... something that you master over time"; early wall-climb "had to get it at just the right angle, and sometimes it would reject me" (the fix was forgiveness).
  Stratton on the Meat Hook working only on demons: "makes it clearer in the player's mind how they can use it."
  https://shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=6
- Pacing: arenas are "like a guitar solo ... We can't have killer arena then another killer arena"; DOOM 2016 was criticised for "too many arenas and hallways".
  https://gameinformer.com/preview/2019/06/13/id-talks-mod-support-integrating-more-traversal-and-the-controversial-neon-ui
- Eternal fights run about 5 minutes, with 7+ enemy types at once, each with its own best answer. The Game Maker's Toolkit episode "The 4 P's of DOOM's Amazing Combat" (2020-04-08, 13 min) breaks one encounter down. Page: https://thetvdb.com/series/game-makers-toolkit/episodes/7677488 (the YouTube link was not fetched).

### Super Gore Nest (Eternal mission 5), from a walkthrough
- Opening arena: Cacodemon, Whiplash, Revenant, Mancubus, Prowler, Carcass and fodder, then a Pinky.
- A swing-bar traversal pocket: swing, jump to the second bar, air dash to a ledge holding a secret.
- An upper combat zone with Dread Knights, Gargoyles and Soldiers, reached by jumping across the building.
- A central pillar that rises and falls, where you swing the top bar at its highest point and then double jump or dash up.
- Buff Totems that must be killed first because they empower demons.
- Portal rooms holding ammo, health and perk points.
  https://www.ludo.guide/guide/doom-eternal/the-campaign-destroy-the-super-gore-nest
  Other pages (blocked or unread): https://doomwiki.org/wiki/Super_Gore_Nest and https://doomwiki.org/wiki/Super_Gore_Nest/Walkthrough. No image URLs verified.

## 2. ULTRAKILL

- Hakita: levels are linear "to make sure the fast pace is kept up and the player doesn't get lost or bored". Visuals "easy to read at a glance"; geometry "simple and clean". AI is "purposefully simple" so enemies are predictable. Challenge comes from "many enemies in each encounter". https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/
- Hakita: the close-quarters arenas stop long single slides, so the dash-buffer exploit stays balanced. Style meter aims to move players off "path of least resistance". https://intothebluesky.com/2020/12/19/interview-with-arsi-hakita-patala/
- 0-1 (a tutorial of one room per verb, then arenas that use them): punch, slide, dash, blood heal, wall jump, ground slam. Arenas include a glass floor (teaches charged shots), a turbine chamber with glass walkways over pits, a projectile arena (teaches deflection), and a layered walkway (teaches Hard Damage). 66 Filth, 16 Strays, one boss. https://ultrakill.wiki.gg/wiki/0-1
- 2-1 Bridgeburner: teaches dash jump and is the first level with bounce pads. Pads "save players from falling". Three towers joined by bridge arenas, a collapsing crane walkway, and wall-jump towers. 25 Filth, 11 Strays, 15 Schisms, 11 Drones, 5 Streetcleaners. Files: `2-1_First_Tower_Low.jpg`, `2-1_First_Tower_High.jpg`, `2-1_Bridge_Arena_1.jpg`, `2-1_Second_Tower_Low.jpg`. https://ultrakill.wiki.gg/wiki/2-1
- 4-2 God Damn the Sun: burning sand does 10 damage per touch, so platforms form the safe path; a gazebo teaches the sanded-enemy buff on one Cerberus; a red-skull arena has stairs over breakable glass above a pit; a boss arena has four platforms. Files: `4-2_start.png`, `4-2_sanded_enemies_tutorial.png`, `4-2_ruin_arena_3.png`, `4-2_red_skull_arena.png`, `4-2_insurectionist_boss_room.png`. https://ultrakill.wiki.gg/wiki/4-2
  (Image path on wiki.gg not verified; open the page and use the filenames.)
- Style bonuses reward arena hazards: Fall (enemy into a bottomless pit, 80), Splattered (fall damage, 100), Terminal Velocity (350), Crushed (80), Heavy Light (chandelier, 80), Minced and Shredded (80), Boiled (7-2 river, 80), Roadkill (tram, 80), plus level crushers. Corpse versions are worth 20. https://ultrakill.wiki.gg/wiki/Style
- Checkpoints restore full health and stamina and set enemies to their state when the checkpoint was touched. Destructibles and keys stay changed. So a failed platforming section after a checkpoint means redoing the cleared fight. https://ultrakill.wiki.gg/wiki/Checkpoint
- 7-1 (Garden of Forking Paths): only the level name was confirmed from a search listing; the page itself was blocked.

## 3. Wall-running and other platforming in shooters

- Titanfall 2 Effect and Cause: the level swaps between two aligned maps (ruined and working) while you keep wall-running and shooting. Laser grids block doors in one time period and debris blocks halls in the other. "Each new idea is explored just long enough for you to get the hang of it, then dropped." https://kotaku.com/let-s-talk-about-titanfall-2-s-best-mission-1788777731
- Keating's iteration: early builds warped players out of bounds when the two maps were not perfectly stacked; he resisted over-guiding testers; enemies stopped moving across time periods "to make it more entertaining". The article says wall-running is not the level's main mechanic. https://en.wikipedia.org/wiki/Effect_and_Cause
- Titanfall 2 Into the Abyss was built from over 100 "action blocks": small prototypes of one gameplay idea each, made over months. GDC 2018, Christopher "Soupy" Dionne.
  https://gdcvault.com/play/1025105/Designing-Unforgettable-Titanfall-Single-Player and https://80.lv/articles/gdc18-designing-titanfall-levels-with-action-blocks
  I could not read the talk, so the spacing numbers are not available.
- A reviewer says Titanfall 2 works because the campaign "blend[s] these movement mechanics ... [with] very careful thought in how you structure the level geometry", and it is under five hours with a new idea always coming. https://scientificgamer.com/thoughts-titanfall-2/
- Pilot's Gauntlet (mission 1) is a timed course teaching wall run, double jump and slide against a leaderboard. https://i1.trueachievements.com/game/Titanfall-2/walkthrough/2
- Ghostrunner, what frustrates: reviewers say it "punishes you for slight deviations in positioning" and you "climb surfaces you meant to wall run". Retrying a long stretch before a hard spot is a complaint. Missing a wall ends momentum "almost immediately". https://www.pcgamer.com/ghostrunner-review/ and https://punishedbacklog.com/ghostrunner-review/ (these quotes came from search summaries, not fetched pages)
- Ghostrunner, what works: one-hit death plus "generous" checkpoints; combat spaces read like race tracks, with "several relatively obvious routes and a few opportunities to switch lanes". Same sources, plus https://www.destructoid.com/reviews/review-ghostrunner/
- Community wall-run complaints (itch.io threads, weak sources): hard to judge distance when FOV changes, unclear contact range, long restarts. Players enjoy wall-running when gaining speed and escaping or shooting from it works. Example thread: https://itch.io/post/2040765

## 4. General level-design references

- Verticality: cap at about three floor planes ("a fourth plane doesn't add new dynamics"); height helps readability; one-way drops stop retreat and force a fight; gamepad play wants flatter, discrete floor planes; Q3DM17 shows jump-pad and teleporter trajectories. https://book.leveldesignbook.com/process/layout/flow/verticality
- Balance: 3-4 chokepoints, one per lane, in PvP; "A large busy view with many deep sightlines will overwhelm the player." https://book.leveldesignbook.com/process/combat/balance
- Encounters: 1 enemy type is rest or tutorial, 2 make a relationship, 3+ is hard to read, 5+ is chaos. Without incentives "the player will just fight unimaginatively from the doorway ... Your arena design will go unused." Let players survey the arena first. https://book.leveldesignbook.com/process/combat/encounter

## Twelve arena elements that are not decoration

1. Bounce / launch pad on the path between fights: it keeps a miss from being a full reset, and teaches the dash jump alongside it. Source: ULTRAKILL 2-1, https://ultrakill.wiki.gg/wiki/2-1; DOOM launchpads, https://blog.adamatomic.com/post/613311014289768448/design-of-doom-eternal
2. Swing bars / monkey bars: let you cross a gap or climb to a ledge with a pole-to-pole jump, and make the player move between fights. Source: Super Gore Nest walkthrough, https://www.ludo.guide/guide/doom-eternal/the-campaign-destroy-the-super-gore-nest
3. Floor planes ("jungle gym"): each plane is a separate skirmish; the obstacles in between cut long-range damage. Cap them at about three. Source: adamatomic post (URL in 1); https://book.leveldesignbook.com/process/layout/flow/verticality
4. Open "canyon" with a pit or sand floor: the floor is the hazard, so platforms become the safe path and enemy knockback matters. Source: ULTRAKILL 4-2, https://ultrakill.wiki.gg/wiki/4-2
5. Pits, grinders and crushers enemies can be dropped into: they pay style, so the player uses the arena as a weapon. Source: https://ultrakill.wiki.gg/wiki/Style
6. Breakable glass or collapsing floors: they change the route mid-fight and punish standing still. Source: ULTRAKILL 0-1 and 4-2 (URLs in section 2).
7. Resource placement as a route: pickups pull players to new spots. Source: adamatomic post. Resources tied to kill type also set the target order. Source: https://www.gamedeveloper.com/design/the-dark-souls-design-of-doom-eternal
8. A priority enemy that makes you move (Buff Totem): kill it first or the others are stronger. Source: Super Gore Nest guide (URL in 2). Inference: the same shape works for any buffer enemy.
9. One-way drops and entrances: they stop retreat and force the fight. Source: https://book.leveldesignbook.com/process/layout/flow/verticality
10. Moderate chokepoints and sightlines: too many deep sightlines overwhelm; a doorway with no incentive to leave is where the player hides. Source: https://book.leveldesignbook.com/process/combat/balance and https://book.leveldesignbook.com/process/combat/encounter
11. A verb-teaching room before the arena that uses it (one verb per room, then arenas that test it): ULTRAKILL 0-1 does this with glass floor, deflection arena and layered walkway. Source: https://ultrakill.wiki.gg/wiki/0-1
12. Forgiving traversal with a cheap retry: wall-grab leniency and short recovery. DOOM's early wall climb "would reject me"; Ghostrunner reviews complain about harsh positioning; ULTRAKILL's checkpoint redo-the-fight cost. Sources: https://shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=6 ; https://www.pcgamer.com/ghostrunner-review/ ; https://ultrakill.wiki.gg/wiki/Checkpoint

Gap: explosive barrels, arena size in metres and exact enemy counts per arena (outside the wiki totals) had no sourced material. Treat them as not checked.
