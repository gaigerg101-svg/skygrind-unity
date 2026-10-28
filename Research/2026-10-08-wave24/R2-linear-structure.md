# R2: Linear boomer-shooter campaign structure (web research, 2026-10-08)

Method: web search + page fetch only. No local code read. Each claim has a URL. "not found" = searched, no usable source.
"Inference" = my reasoning, not a sourced fact. Several wikis (doomwiki.org, Dusk fandom, Doom fandom) returned 402/403 to
fetches; for those I only had search-result snippets, flagged "(snippet)".

## 1. How ULTRAKILL structures levels

### 1.1 Overall shape
- Prelude (5 levels) + 3 acts; each act = 3 layers; layers 1-2 of an act have 4 levels, layer 3 has 2; every layer's last
  level is a "climax" with a boss. 29 main levels shipped, 35 planned. https://ultrakill.wiki.gg/wiki/Levels (search summary of the Levels page)
- Per level: ~one challenge condition, one boss/mini-boss slot, a few secrets (5 Soul Orbs per normal level), S-rank par
  numbers. Examples below are from https://ultrakill.wiki.gg/wiki/Levels
  - 0-1 Into the Fire: "Get 5 kills with a single glass panel"
  - 0-3 Double Down: "Kill only 1 enemy"
  - 0-4: "Slide uninterrupted for 17 seconds"
  - 0-5 Cerberus: "Don't inflict fatal damage to any enemy"
  - 1-1: "Complete the level in under 10 seconds"
  - 1-2 and 1-4: "Do not pick up any skulls"
  - 2-1: "Don't open any normal doors"; 2-2: "under 60 seconds"; 2-3: "Don't touch any water"; 2-4: "Parry a punch"
  - Challenges are optional side-goals, not blocking conditions. (Inference from list; the wiki lists them as "Challenge".)
- Secrets: Soul Orbs unlock cosmetics; P-rank on all levels of an act unlocks that act's Prime Sanctum with a secret boss.
  https://ultrakill.wiki.gg/wiki/Levels
- Hakita on level design: simple clean geometry readable at a glance; linear to keep momentum; calm/chaos contrast; flow
  controlled by which enemies are where, not complex AI. https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/
- ULTRAKILL is developed mainly by one person (Hakita) in Unity. https://en.wikipedia.org/wiki/Ultrakill (via search summary)

### 1.2 Arena counts, concrete levels
- 0-1 Into the Fire: forced tutorial rooms (movement: punch, slide, dash, wall jump, ground slam; blood healing, Soul Orb),
  then ~11 arenas/rooms in a largely linear chain (first arena with weapon pickup, hallways, glass floor, grinder walkway,
  turbine, projectile arena, combo hallway, ground slam chamber, boss arena). 83 enemies (66 Filth, 16 Strays, 1 boss).
  S-rank par: 2:30, 71 kills, 7000 style. https://ultrakill.wiki.gg/wiki/0-1:_Into_the_Fire
- 0-3 Double Down: ~9 named areas; shotgun dropped by the first Swordsmachine boss; shotgun taught "through prompted
  instructions" across several encounters; menacing room series "designed for shotgun combat". Par 3:30, 49 kills.
  https://ultrakill.wiki.gg/wiki/0-3:_Double_Down
- 1-4 Clair de Lune: not a straight arena. Explore a manor (pathway, hall, bedroom, dining hall) to find 3 skull keys, open a
  courtyard door, then V2 enters through a window into a water-floor arena with four breakable pillars and a skylight.
  S par: 2:15 (the level is mostly exploration plus one fight). Drops Knuckleblaster. https://ultrakill.wiki.gg/wiki/1-4:_Clair_de_Lune
- So a layer's rhythm = 3 combat/traversal levels of ~8-12 arenas each, then a short boss-gated level. Typical S-par 2-4 minutes
  per level (sample above), i.e. levels are short; replay is the depth. (Inference from the three par times above.)

### 1.3 Style / rank system
- Ranks D-C-B-A-S + P. Final rank = sum of time, kills, style ranks minus 1 point per restart; P = S in all three, no restart.
  Taking damage drops the style meter by one rank and costs damage x 5 points; respawn at a checkpoint resets the style meter
  and reduces score. https://ultrakill.wiki.gg/wiki/Ranking
- Style meter rewards variety: repeating one weapon lowers the multiplier; swapping to fresh weapons gives bonuses
  (search summary of guides): https://www.ludo.guide/guide/ultrakill/style-meter-ranking
- Design use: rank doubles as the "mastery layer" so the main path can be forgiving; challenge/limited modes are the opt-in
  edge. (Inference.)

### 1.4 Weapon / movement introduction timeline (Layer 0 to 2)
- Source for weapon acquisition: https://ultrakill.wiki.gg/wiki/Weapons
  | Item | Where | Notes |
  |---|---|---|
  | Revolver | 0-1 | start of game, first arena has the weapon pickup (https://ultrakill.wiki.gg/wiki/0-1:_Into_the_Fire) |
  | Movement kit (punch, slide, dash, wall jump, slam) | 0-1 tutorial rooms | taught before the first real arena |
  | Shotgun (Core Eject) | 0-3 (or 0-2 if secret encounter beaten) | dropped by a boss (Swordsmachine) in 0-3 |
  | Nailgun | 1-1 or 1-2 | layer 1 |
  | Railcannon | 2-2 | layer 2 |
  | Rocket launcher | 5-3 | far later |
  | Variants | bought at Terminal Shop with points: Pump Charge 12,500 P, Sawed-On 25,000 P | https://ultrakill.wiki.gg/wiki/Pump_Charge_Shotgun |
- Pattern: gun #2 arrives at level 3 of the first chapter, gun #3 at the start of the next layer, gun #4 one layer later, i.e.
  roughly one new weapon per layer in Act 1. Weapons are given by (a) a boss drop or (b) a pickup in an early arena.
  (Inference from the table.)
- Not found: an official ULTRAKILL designer statement on why the weapons come in this order.

### 1.5 V2 (1-4) and why it spikes
- First meeting of V2 is the boss of 1-4 (end of layer 1); V2 40 HP, uses Piercer Revolver charge (30 dmg), Core Eject shotgun
  (25 pellets / 35 grenade), 3-shot revolver bursts (10 each); four movement modes shown by wing colour; it enrages if the player
  keeps distance. https://ultrakill.wiki.gg/wiki/V2
- Why it works (my analysis, based on the wiki facts above):
  - It is a mirror: same tools as the player, so it tests whether the player learned movement and weapon-switching, not new rules.
  - The colour-coded wing modes make its behaviour readable ("telegraph"), so deaths feel attributable.
  - It comes after 4 levels of exploration and after the shotgun and nailgun are owned; the previous boss fights (Cerberus,
    Swordsmachine, Malicious Face) were scripted-pattern bosses, V2 is the first reactive one.
  - Anti-kiting enrage punishes the exact "stay far away" habit that trivialised earlier bosses.
- Community view that V2 "was way too hard" (speed complaints): https://ultrakill.fandom.com/wiki/V2_(Second_Encounter) (via search summary)
- Not found: a developer postmortem explaining the V2 spike.

### 1.6 Doom Eternal / Dusk / Ion Fury / Quake
- Doom Eternal: three-resource loop. Glory kill -> health, chainsaw -> ammo, flamethrower -> armor; "you never finish an arena
  battle without running short" so retrieval pulls you into the fight. Arenas are multi-layered with monkey bars and canisters.
  https://www.pressreader.com/australia/edge/20200521/283034056761200 (search summary of an Edge review) and
  https://blog.adamatomic.com/post/613311014289768448/design-of-doom-eternal (fetch blocked; snippet only)
- Doom Eternal arena types: "jungle gyms" (distinct floors) vs "canyons" (continuous); traversal puzzles, hazards, turrets are
  modular level components between combat arenas. Cooldowns and ammo limits keep tactics varied.
  https://blog.adamatomic.com/post/613311014289768448/design-of-doom-eternal and
  https://www.gamespot.com/articles/why-doom-eternal-incorporates-puzzle-platforming/1100-6473287/ (search titles/summaries only)
- Doom Eternal: arena count per level, enemy count per arena: not found.
- Dusk: Szymanski modelled his design on John Romero ("what would Romero do?"): non-linearity, abstract geometry; Unity because "it is
  what I know how to use". https://www.thegamer.com/dusk-postmortem-interview/ (fetch 404; search summary) and
  https://www.quakeworld.nu/blog/394/dusk-an-interview-with-david (fetched; engine only). Dusk level length/encounter counts: not found.
- Ion Fury: six zones, 5+5+5+5+3+1 acts; starts with Loverboy pistol (and a melee baton); the Disperser (shotgun/grenade),
  Penetrator (dual-wield SMG), Ion Bow, Chaingun, Clusterpuck arrive across the campaign.
  (search summary) https://gamingpastime.com/ion-fury-review/ ; exact unlock order and level length: not found.
- Quake: level length/encounter counts: not found.

## 2. Weapon unlock pacing and temporary removal

### 2.1 Unlock order evidence
- ULTRAKILL: revolver (level 1) -> shotgun (level 3) -> nailgun (layer 1) -> railcannon (layer 2) -> rockets (much later).
  https://ultrakill.wiki.gg/wiki/Weapons
- Teach method seen in ULTRAKILL: the weapon is dropped by a boss or placed in a first arena, and the following rooms are built
  around it ("Menacing Room: areas designed for shotgun combat", prompts teach alt-fire). https://ultrakill.wiki.gg/wiki/0-3:_Double_Down
- Ion Fury starts with pistol + melee and grants shotgun-like Disperser later. https://gamingpastime.com/ion-fury-review/
- Titanfall 2 "Effect and Cause": careful on-boarding of one mechanic before "utter science fiction weirdness"; it is "tightly
  contained". https://www.gamedeveloper.com/design/a-close-analysis-of-i-titanfall-2-s-i-level-effect-and-cause (summary)
- Respawn built levels from "action blocks" (rapid prototype set pieces): https://80.lv/articles/gdc18-designing-titanfall-levels-with-action-blocks
  (full workflow: not found, only in the GDC video).
- Suggested curve for the designer's game (Inference): pistol L1 arena 1; shotgun at L1 boss or L2 arena 1; third weapon
  (nail/rail-class) at L3 or later; each new weapon gets (1) a safe pickup room with a target dummy or 2 weak enemies, (2) one arena
  where it is clearly the best tool, (3) a later arena that forces a switch away from it.

### 2.2 Taking away weapons / abilities
- ULTRAKILL "limited" and challenge conditions are opt-in sub-goals shown on the level (e.g. "Kill only 1 enemy", "Slide
  uninterrupted for 17 seconds", "Don't open any normal doors"). https://ultrakill.wiki.gg/wiki/Levels
  It does not strip weapons on the main path in the pages I read. Official "limited weapons" mode in ULTRAKILL: not found.
- Doom Eternal "no-BFG" levels / Slayer Gates restrictions: not found (my searches returned nothing on this).
- Titanfall 2 stripped sections: the summary I got describes "limited equipment choices" in Effect and Cause (source above) but
  gave no specifics: partially found only.
- Devil Daggers: the entire game is one fixed kit (daggers: shotgun-like burst on tap, stream on hold) in a minimal arena.
  https://en.wikipedia.org/wiki/Devil_Daggers (search summary)
- Hades / Returnal restrictions: not found beyond Returnal's stated "insurmountable at first" difficulty intent
  (https://www.thegamer.com/returnal-director-difficulty-accessibility-challenge/ search summary).
- How to avoid "punishing" (Inference, no source): show the restriction on the room entry and HUD; give a short ability recovery
  after the room; make restrictions short (one arena, not one level); make a strong replacement tool available; keep checkpoints
  within the restricted arena; reward mastery (style bonus) rather than gating progress; make it opt-in for P-rank-type goals.
  Neon White's principle "if you're ever confused on what to do, the game doesn't feel good" supports clear communication:
  https://www.gameinformer.com/feature/2022/11/25/how-a-neon-white-level-is-made

## 3. 3D arena and level craft (what was actually found)
- Found: general metrics. Hallway width: Quake/Half-Life 64 units min, Unreal 150 cm, Unity 2.0 m; min hall = 2x player width;
  tighter spaces make speed feel faster; fast arcade games favour wide, smooth turns. Prodeus: several arenas connected,
  progression gated by keys and locked doors. https://book.leveldesignbook.com/process/layout/flow ,
  https://book.leveldesignbook.com/process/blockout/metrics (via search summaries of the proxy mirror)
- Doom classic metrics: rooms 256-1536 units, halls 64-128 units, pistol 2.5 rounds/s (5-15 dmg). https://book.leveldesignbook.com/process/blockout/metrics/doom
- Found: windows and vantage points give anticipation and cohesion (same Level Design Book source).
- Found: Doom Eternal jungle-gym vs canyon arena types (section 1.6 URLs).
- Found: ULTRAKILL readability: clean geometry, calm-chaos contrast (80.lv URL in 1.1).
- NOT found (no usable sources): arena dimensions for 15-30 m/s movement; cover ratios; wall-run/slide lane widths; lighting
  rules for guiding light / colour-coded danger; kill-box door timing; checkpoint spacing numbers; a GDC talk on these.
- My inference for sizing (unsourced): at 20 m/s a player crosses a 30 m arena in 1.5 s; arenas for 15-30 m/s want at least
  40-60 m across with vertical tiers (jungle-gym style), so enemies and projectiles have time to read; corridors 6-8 m wide for
  slide/strafe room; place hard cover sparingly because the speed itself is the cover.
- Colour code: matches the project's CLAUDE.md palette rules (cyan player, red-orange danger); no external source needed.

## 4. Building levels efficiently in Unity
- ProBuilder: in-editor greyboxing of structures, collision geometry, trigger zones and nav meshes; ProGrids gives snapping on
  all axes for modular or tile-based work. https://80.lv/articles/probuilder-advanced-level-design-in-unity/ ;
  https://docs.unity3d.com/Packages/com.unity.progrids@3.0/manual/index (via search summaries). Note ProGrids is a preview package
  and its snapping now partly lives in the editor itself (not verified in this research).
- Greybox first: Neon White used Unity and gray-box iterations (50+ iterations on one 9.56 s level over 3 years): the lesson is to
  decide flow in greybox before art. https://www.gameinformer.com/feature/2022/11/25/how-a-neon-white-level-is-made
- ULTRAKILL: Unity, mostly one developer; whether he used ProBuilder, a custom tool, or hand-modelled meshes: not found.
- Dusk: Unity; level editor choice (Trenchbroom/ProBuilder): not found.
- Data-driven room-list formats (JSON / ScriptableObjects), module snapping kits, navmesh vs custom grid for 10+ levels:
  no documented indie sources found. Inference for this project (which already has a C# web-collider rather than PhysX):
  - describe each level as an ordered list of "room" records (id, bounds, spawn script, door lock rule, weapon whitelist, ability
    mask, checkpoint flag); build geometry in ProBuilder/greybox, keep encounter data in JSON beside it
  - reuse the existing survival wave spawner as the encounter engine inside each room
  - avoid navmesh for speed shooters unless enemies need it; use the existing CoreGrid-style pathing
  - a "room tag" component per arena gives both the lock-in volume and the data key.

## 5. Shotgun balance table

Fire interval = seconds between shots (not including reload for double-barrels unless stated). DPS = max damage per shot / interval
(all pellets hit). Pellet/damage units are each game's own, so DPS is comparable only inside a row's game.

| Game / weapon | Pellets x dmg | Dmg/shot (max) | Interval (s) | Max DPS | Source |
|---|---|---|---|---|---|
| ULTRAKILL Core Eject / Sawed-On | 12 x 0.25 | 3.0 | 1.25 | 2.4 | https://ultrakill.wiki.gg/wiki/Core_Eject_Shotgun |
| ULTRAKILL Pump Charge (primary) | 10 x 0.25 | 2.5 | 0.9 | 2.8 | https://ultrakill.wiki.gg/wiki/Pump_Charge_Shotgun |
| ULTRAKILL Pump Charge, 1 pump | 16 x 0.25 | 4.0 | not found | not found | same |
| Quake 1 Shotgun | 6 x 4 | 24 | 0.5 | 48 | https://quakewiki.org/wiki/Shotgun_(Q2) (search summary; Quake 1 SG) |
| Quake 1 Super Shotgun | 14 x 4 (2 shells) | 56 | 0.7 | 80 | https://quakewiki.org/wiki/Super_Shotgun |
| Dusk Shotgun (single) | 5 x 30 | 150 | ~0.9 | ~167 | Dusk wiki snippet: https://dusk.fandom.com/wiki/Weapons?oldid=2801 |
| Dusk Shotgun (dual wield) | 5 x 30 each | 150 | ~0.64 | ~234 | same |
| Dusk Super Shotgun | 20 x 30 | 600 | slowest weapon; ~1.45 reload cited | ~414 if 1.45 is the cycle (unverified) | https://dusk.gamepedia.com/Super_Shotgun?oldid=2803 |
| Doom (2016) Super Shotgun | 16 x 60 | 960 | low fire rate; number not found | not found | https://doom.fandom.com/wiki/Super_shotgun/Doom_(2016) (snippet) |
| Doom (2016) Combat Shotgun | ~22-26 per pellet (snippet) | not found | "fast rate of fire"; number not found | not found | https://www.doomwiki.org/wiki/Combat_shotgun (snippet) |
| Doom Eternal Combat / Super Shotgun | not found | not found | not found | not found | doomwiki.org returned 403 |

- Pistol comparators found: Classic Doom pistol 2.5 rounds/s (0.4 s), 5-15 damage. https://book.leveldesignbook.com/process/blockout/metrics/doom
  ULTRAKILL revolver and Dusk pistol numbers: not found.
- Takeaways (sourced numbers, my reading):
  - Single-barrel shotgun intervals across these games sit between 0.5 s (Quake) and 1.25 s (ULTRAKILL base); ULTRAKILL's
    designer-intended "heavy" shotgun is 1.25 s and the faster Pump Charge variant is 0.9 s. A 1.25 s base is slower than the
    other three data points and survives because ULTRAKILL gives alt-fire/cores and a fast revolver as filler.
  - Dusk's cadence of ~0.9 s for 150 dmg/shot is about 10x its Super Shotgun's per-shot damage 600 over a much longer cycle:
    double barrel = fewer, bigger shots. (Computed from the table.)
  - If the designer's shotgun "fires a little too fast", a first fix is to push the interval to 0.9-1.25 s and raise per-shot
    damage to keep DPS roughly constant; compare to the pistol DPS so the shotgun is clearly better only up close.
  - DPS of the shotgun relative to the starter gun is not publicly given in any source found; set it by playtest.

## Recommendations for a first 6-7 hour build of a 3-level linear campaign slice

All items below are my inference from the sources above unless a URL is attached.

1. **Scope (hours 0-1): lock the format.** 3 levels x (1 tutorial/warm-up room + 3-4 combat arenas + traversal links + 1 boss
   arena). ULTRAKILL's 0-1 has ~11 rooms with S-par 2:30, so aim for 5-8 rooms and 3-5 minutes per level, not 11.
   https://ultrakill.wiki.gg/wiki/0-1:_Into_the_Fire
2. **Data format (hour 1-2): one JSON per level** with ordered rooms; each room has bounds, encounter script (reuse existing wave
   spawner), door lock-until-clear flag, weapon whitelist, ability mask, checkpoint flag, par time. Geometry stays greybox
   (ProBuilder) with the room id as a tag. Keep Survival and Endless "grind" as additional level files using the same loader.
3. **Weapon schedule (3 levels):** L1 pistol only; shotgun dropped at the end of L1's mini-boss (ULTRAKILL gives its shotgun via a
   boss drop at level 3 of the prelude: https://ultrakill.wiki.gg/wiki/0-3:_Double_Down); L2 arena 1 is a shotgun-only teaching
   room; L3 introduces the third weapon in its first arena. One new weapon per level.
4. **Shotgun tuning now:** set interval to a 0.9-1.25 s range and re-scale pellet damage; check it against the pistol DPS in a
   headless bench. Reference rows: Quake 0.5 s, Dusk ~0.9 s, ULTRAKILL 0.9 / 1.25 s (table above).
5. **Restrictions (2 only, both short, opt-in feeling):** one L2 arena that bans a movement ability (e.g. no dash) for one room
   with the HUD flag shown on entry, and one L3 arena with a "pistol + shotgun only" loadout. Checkpoint inside each. No
   source for exact feel; test with one blind pass each.
6. **Boss spike (L3 end): a mirror boss like V2** (https://ultrakill.wiki.gg/wiki/V2): uses the player's own moves, readable
   colour-coded movement states, anti-kiting enrage, appears after the player owns all slice weapons.
7. **Rank system (cheap, ~1 hour):** time + kills + style, deaths cost points, as in ULTRAKILL (https://ultrakill.wiki.gg/wiki/Ranking).
   Gives replay value without more content.
8. **Arena sizing (unsourced, tune by feel):** ~40-60 m across with 2-3 vertical tiers for 15-30 m/s, corridors 6-8 m. First
   pass with lit guide colour per CLAUDE.md palette. Verify with a movement playtest, not a doc.
9. **Cut list if time runs short:** secrets (Soul Orb-like), challenge conditions, and the third weapon's arena; keep rooms,
   door locks, checkpoints, boss.
10. **Open questions the research could not answer (not found):** arena size numbers for this speed range; light/colour rules from a
    named talk; kill-box timing; indie room-list formats; official limited-weapon designs in ULTRAKILL, Doom Eternal, Titanfall 2,
    Hades, Returnal; Doom Eternal shotgun intervals. A direct GDC Vault / Level Design Book pass would fill these.
