# Arena, boss length and wall-run marking: web research (wave 39)

Date: 2026-10-13. Budget about 6 minutes; web only. Each claim carries its URL. "Fetched" means the page itself was read;
"snippet" means only the search engine's summary of the page was read (weaker; verify before relying on it).
Several primary sources returned HTTP 403 / 404 (adamatomic Doom Eternal essay, GameSpot Doom Eternal platforming
article, Mapcore's Effect and Cause breakdown, gdconf Doom Q&A): not checked.

## 1. Levels and arenas that force and reward a self-boost (shotgun / rocket jump)

### ULTRAKILL (the closest reference)
- Shotgun boost is a costed verb, not free: the pump-charge shotgun's 3-pump overload explodes; aiming opposite the
  intended direction and firing launches the player, "at the cost of 50 self-damage"; it works "in any direction and in
  midair, but requires slightly more preparation". The explosion does 10 damage to enemies. Core Eject "core boosting"
  (slide + shoot a core) pushes forward for 35 self-damage. Plain pellets push the player forward "by an almost
  imperceptible degree". (fetched) https://ultrakill.wiki.gg/wiki/Shotgun
- Rocket movement has built-in diminishing returns: rocket riding slows the rocket and lets the player steer it; "the
  fourth rocket ridden before touching ground starts at half power, and the ninth onward has none". Ground rocket jumps
  cost no health (shockwaves don't hurt the player); gasoline rocket jumps cost 35. (fetched)
  https://ultrakill.wiki.gg/wiki/Rocket
  - Design reading (ours, not the source's): a per-airtime decay is the device that stops one boost verb chaining
    across a whole level, which is one way to prevent a "single trivial skip".
- Movement speeds span walking 16.5 u/s to rocket riding 65 u/s (snippet). https://ultrakill.wiki.gg/wiki/SSJ
- No ULTRAKILL source found that describes how levels are built around the shotgun jump: **not found**.

### Doom Eternal (arena traversal)
- Arenas are wave-based, joined by corridors; "flat-ish or tall-ish", tall ones in two flavours, "jungle gyms and
  canyons"; jungle gyms let you "jump from skirmish to skirmish and use mobility options to 'interrupt' combat", and
  their obstacles make long-range enemy attacks harder to land (snippet; page 403 on fetch).
  https://blog.adamatomic.com/post/613311014289768448/design-of-doom-eternal
- Arena furniture: monkey bars, boost pads, teleporters "accentuate the game's remarkably fluid traversal" (snippet).
  https://shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=6
- Enemies as traversal anchors: the meat hook on enemies such as the Pain Elemental gives vertical movement (snippet).
  Same Shacknews / GameSpot results; GameSpot fetch 403:
  https://www.gamespot.com/articles/why-doom-eternal-incorporates-puzzle-platforming/1100-6473287/
- Doom (2016) arenas: "medium to large scale arenas that are asymmetrical in nature with a lot of verticality",
  "combat chess" (pick which demon first and how to get there); push-forward combat removes regen / reload / cover so
  the player takes what they need from enemies; glory kills give small health (snippet of the GDC 2018 talk by Kurt
  Loudy and Jake Campbell). https://gdcvault.com/play/1024940/Embracing-Push-Forward-Combat-in ,
  https://www.gamedeveloper.com/design/video-the-combat-design-of-i-doom-i-

### Quake / TF2 jump maps
- Quake DM6: rocket jumping bypasses the stairs of the Small and Large Staircase rooms (snippet), i.e. the jump is a
  shortcut beside a walkable route, not the only route. https://wiki.quakeworld.nu/DM6
- TF2: jump-map culture with "jump academy" maps of many small courses used to teach; rj_training separates beginner
  and advanced jumps (snippet). https://steamcommunity.com/app/440/discussions/0/4286935937415123826 (source of the
  snippet not confirmed). Valve's mapper reference has player-movement numbers:
  https://developer.valvesoftware.com/wiki/Team_Fortress_2/Mapper%27s_Reference (not fetched).
- Momentum Mod's rocket-jump guide is a stub ("TODO" where mechanics and map structure would be). (fetched)
  https://docs.momentum-mod.org/guide/gamemodes/rocket_jump/
- Concrete published rules for jump-map shapes (shaft widths, ledge heights above jump height, ceiling use): **not
  found** in this budget.

### Titanfall 2 / Dusk
- Titanfall 2 campaign gauntlet: to clear obstacles the player must wall run, and a holographic ghost shows exactly how
  to clear each obstacle; a leaderboard rewards faster completion (snippet).
  https://en.wikipedia.org/wiki/Effect_and_Cause , https://Www.wikipedia.org/wiki/Titanfall_2
- Respawn built SP levels from "action blocks" (rapid prototypes of one gameplay idea) because early work used
  "comfortable techniques and led to familiar results" (fetched; GDC 2018, Christopher Dionne).
  https://gdcvault.com/play/1025105/Designing-Unforgettable-Titanfall-Single-Player
- Effect and Cause was "the most labor and time-intensive level", revised many times to not confuse players "while
  still trying to avoid guiding them too much" (snippet). https://en.wikipedia.org/wiki/Effect_and_Cause
- Dusk: **not found** (not searched in budget).

## 2. Boss fight length

- Published designer numbers for ULTRAKILL, Doom Eternal, Hades or Hollow Knight boss length: **not found**.
- Cuphead: bosses "don't take longer than 5-10 minutes" (journalism, not the studio; snippet).
  https://twinfinite.net/guides/cuphead-how-long-it-is/
- Forum / indie rule of thumb: a boss "shouldn't be more than 5 minutes overall, maybe 10 for something complex"; one
  indie dev planned 3-5 minutes (snippet; low authority). https://itch.io/post/11973481
- Doom Eternal criticism (reviews, not id): bosses called "a bullet sponge that goes on for far too long", "wars of
  attrition than tests of skill" (snippet). https://tvobsessive.com/2020/04/03/doom-eternal-25-days-later-review/
- Hugo Martin on the Marauder: "the first enemy in the series that demands you fight on its own terms": attack only in
  its opening before the shield returns; "all of these things are skillshot opportunities" (snippet).
  https://www.pcgamer.com/uk/doom-eternals-director-says-the-marauder-is-good-actually/
  - This is the "armor window" pattern: length comes from waiting for a readable opening, not from health.
- Hades Megaera: starts with Lunge and Whip Whirl; Volley unlocks at 75 % HP and Flame at 50 % (snippet; wiki-sourced).
  https://breezewiki.discard.no/hades/wiki/Furies
  - Pattern: HP thresholds add attacks rather than add HP.
- Hakita (ULTRAKILL) prefers "picking individual moves for each moment where he can weigh risk and reward" over combo
  dials; God Hand cited for feel (snippet). https://intothebluesky.com/2020/12/19/interview-with-arsi-hakita-patala/
- General advice (low-authority blog): list the skills to test and invent attacks that force them; don't just raise
  health and damage per phase; add new patterns, hazards or changed weak points (snippet).
  https://www.wayline.io/blog/crafting-legendary-boss-battles

## 3. Marking runnable vs non-runnable surfaces

- Doom Eternal: only climbable walls are climbable; they are "white walls with little creases" (snippet).
  https://www.thegamer.com/doom-eternal-how-to-climb-walls/
- Mirror's Edge: environments whitewashed; Runner Vision paints usable objects red (doors, railings, ledges, pipes,
  boxes, ropes, buttons) (snippet). https://www.gamedeveloper.com/design/visual-cues-in-level-design ,
  https://www.worldofleveldesign.com/categories/game_environments_design/mirrors-edge-color.php . Level designer
  Elisabetta Silli: red "means danger or stop" but "Red is perfect for Faith, I wouldn't change it"; for a sequel she
  wanted "the path of the player... clear from the beginning" (fetched).
  https://www.engadget.com/2010-08-17-overheard-gdc-mirrors-edges-odd-go-here-color.html
- Mirror's Edge Catalyst: "Follow the Red" remains; Runner's Vision has Off / Classic / Full View settings (fetched).
  https://www.ea.com/amp/news/runners-vision-in-mirrors-edge-catalyst
- Ghostrunner: you can run "along almost any flat vertical surface"; yellow pipes, yellow mats and arrows guide the
  route (snippet; walkthrough site). https://primagames.com/?p=314223 ,
  https://gamerwalkthroughs.com/ghostrunner/level-1-an-awakening/
- Dying Light: "Basically anything that looked climbable will be climbable" (fetched; the article gives no visual
  marking method). https://www.gamedeveloper.com/design/game-design-deep-dive-i-dying-light-i-s-natural-movement-system
  Dying Light 2: "readable edges, yellow grips" telegraph routes (snippet).
  https://80.lv/articles/designing-dying-light-2-s-environments-and-levels/
- Titanfall 2: walls are "possibilities" not boundaries (snippet, review). https://scientificgamer.com/thoughts-titanfall-2/
  A Titanfall 2 non-runnable-material marking: **not found**.
- Neon White: **not found** (search returned nothing relevant).
- Common pattern across sources: mark the USABLE surface (red, yellow, white + creases), and keep everything else
  neutral; no source found that marks the NON-usable surface instead.
