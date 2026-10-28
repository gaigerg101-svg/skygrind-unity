# Enemies: web research for wave 40 (deflectable projectiles, roster roles, telegraphs, silhouettes, spawn pacing)

Web research only, 2026-10-13. Every bullet ends with its source. "Not found" means it was searched for and no
source turned up; nothing here is invented. "Derived" means arithmetic on sourced numbers, done here.
ULTRAKILL speeds are in Unity units per second (u/s); the wiki does not give a metre conversion, so compare them only
as ratios to V1's own speeds.

## 1. Deflectable / parryable projectiles

### ULTRAKILL
- Almost any projectile can be parried (Stray hell orbs, Mindflayer Hell Seekers, Hideous Mass mortars). The one named
  exception is the Mirror Reaper's Acid Missiles, which have to be shot down instead. https://ultrakill.wiki.gg/wiki/Parry
- Colour code on the enemy: a **yellow** flash means the attack can be parried, a **blue** flash means it can't. Melee
  attacks show a 4-point spark, ranged attacks an 8-point spark. https://ultrakill.wiki.gg/wiki/Parry
- On a parry the game briefly freezes (hitstop) and flashes the screen white. It also refills stamina and heals to full
  (up to the Hard Damage limit). https://ultrakill.wiki.gg/wiki/Parry
- Parry window: **no frame or ms number was found**. What was found: patch 1.03b added "3 mercy frames between a
  projectile hitting the player and the damage happening", and changed the projectile parry "from a raytrace into a
  hitbox", making angled punches easier. https://hakita.itch.io/ultrakill-prelude/devlog/98473/103b-patch-notes
- One punch can parry up to two projectiles: one inside the player (during its mercy frames) and one in front.
  https://ultrakill.wiki.gg/wiki/Parry (via search summary of the wiki)
- Feedbacker (the parry arm): ~0.35 s punch animation, plus 0-0.8 s "punch exhaustion"; exhaustion regenerates at
  1.25 per second. https://ultrakill.wiki.gg/wiki/Arms
- V1's speeds, for scale: walk 16.5 u/s, slide 24 u/s, dash 49.5 u/s. https://ultrakill.wiki.gg/wiki/Movement
- **Stray** (the basic parry-teacher): throws one hell orb at **65 u/s** (about 3.9x walk speed, derived) for 25 damage.
  First throw comes 1-2 s after it re-spots the player, then one every 1-2.5 s. It keeps its distance and moves at
  10 u/s (0.61x walk, derived). You can parry the throw itself (Feedbacker or shotgun parry) but not the charge-up. Its
  health is 1.5. https://ultrakill.wiki.gg/wiki/Stray
- **Schism**: fires 6 linked yellow hell orbs at 65 u/s, 35 damage each, after a 1-2 s charge, then every 1-2.5 s. The
  orbs can be parried during the barrage. Health 5. A charged point-blank shotgun blast kills it in one shot. The wiki
  says it "cannot aim with any degree of accuracy". https://ultrakill.wiki.gg/wiki/Schism
- **Soldier**: 4 projectiles in a 5° cone at 65 u/s, first volley 1-2 s after spotting. Moves at 15 u/s. While
  grounded it blocks normal explosions (75 % damage cut), but not in the air. Both its shot and its kick can be parried.
  https://ultrakill.wiki.gg/wiki/Soldier
- **Drone**: before firing, its light "turns red for a moment". Its projectile speed is 27.86 u/s on Harmless and
  83.57 u/s on Violent/Brutal. Parrying the drone itself while it charges kills it outright. Drones dash to dodge and
  come in swarms. https://ultrakill.wiki.gg/wiki/Drone
- **Cerberus**: its orb can technically be parried but is "often difficult due to its high speed and lack of flash
  indicator". This is a published example of what happens when a parryable projectile has no telegraph.
  https://ultrakill.wiki.gg/wiki/Cerberus
- **Mindflayer**: 5 homing Hell Seekers at 32.5 u/s (Harmless), 48.75 (Lenient) and 87.75 (Violent/Brutal). Its cyan
  beam can't be parried. It teleports away after taking more than 6 damage. https://ultrakill.wiki.gg/wiki/Mindflayer
- **Malicious Face**: fires 6 hell orbs, 25 damage each. Its charged beam can be parried only at the moment the charge
  finishes, "denoted by a sound and a yellow flash". Shooting its eyes as a weak point was scrapped as "too demanding
  and complicated for such a simple enemy". https://ultrakill.wiki.gg/wiki/Malicious_Face
- **Streetcleaner**: slaps rockets and cores back unless they hit it from behind. Its back fuel tank is a weak point
  that kills it instantly, and it has a 3 s dodge cooldown. This makes it a front-deflector you beat by flanking.
  https://ultrakill.wiki.gg/wiki/Streetcleaner

### Others
- **Doom: The Dark Ages**: only **green** attacks can be parried. Red and blue can only be blocked. A parried
  projectile homes back to whoever fired it, and a parried single melee strike staggers the enemy. One tap is enough;
  you don't need to hold. https://www.shacknews.com/article/144239/how-to-parry-doom-the-dark-ages
- Doom: The Dark Ages ships "Parry Window" and "Enemy Projectile Speed" sliders (plus Game Speed and Daze Duration),
  but the post gives no numbers. https://slayersclub.bethesda.net/en-AU/news/doom-the-dark-ages-gameplay-modifiers
- Hugo Martin said the Dark Ages Hell-surge attacks were red during development before green became the parry colour.
  Search summary only, not checked against the page. https://doomwiki.org/wiki/Shield_saw (403 on fetch)
- **Sekiro**: the deflect window is **12 frames (0.2 s)** before the hit. Each recent press shrinks it, down to 4
  frames or even 0 when spammed. The penalty clears after 30 frames or on a successful deflect.
  https://sekiroshadowsdietwice.wiki.fextralife.com/Deflection
- Sekiro uses a **red kanji** ("danger") for perilous attacks. Thrusts can be deflected but not guarded; sweeps and
  grabs can be neither. https://sekiroshadowsdietwice.wiki.fextralife.com/Perilous+Attacks
- **Ghostrunner**: you deflect projectiles by blocking just before the hit. Guides call the window "surprisingly wide"
  but give no number. https://afkgaming.com/gaming/general/ghostrunner-2-how-to-parry-oncoming-attacks-and-projectiles
- **Doom Eternal**: has no projectile parry. The closest thing is the Marauder: its eyes flash **green** before the axe
  lunge, and hitting it with the Super Shotgun or Ballista in that moment staggers it. Earlier shots are blocked by its
  shield. https://www.gamerevolution.com/?p=639744 ; https://pcgamer.com/how-to-kill-doom-eternal-marauder-tips-fight
- Severed Steel: not found (it has no deflect verb in any source checked).

### What these sources agree on (pattern, not a sourced number)
- One reserved colour means "parryable", used for nothing else: ULTRAKILL yellow, Dark Ages green, Marauder green
  eyes.
- A separate "can't parry" colour: ULTRAKILL blue, Dark Ages red/blue, Sekiro's red kanji.
- The basic parry-teacher fires one slow orb about 4x walk speed (Stray), with 1-2 s before the first throw.

## 2. A roster that makes weapon switching matter

- Hugo Martin calls the demons "chess pieces" used to move the player around, for example off ledges.
  https://www.shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=4
- Martin: "For enemies that dodge projectiles, the ballista is a really good option for them. But on enemies that move
  fast, I can use the mod for the rocket launcher that allows for remote detonation." Also: "If one weapon is
  unbalanced, that takes away from everything else." And: "You know you've got an encounter dialed in right when the
  player is running by certain enemies to kill another one."
  https://www.shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=4
- Martin: "Combat is another type of puzzle. Players must determine which weapons and modifications to use."
  https://shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=8
- Doom Eternal weak points, one tool each:
  - Arachnotron: destroy the turret.
  - Cacodemon: a grenade in the mouth, then a glory kill.
  - Revenant: shoot off the shoulder launchers.
  - Mancubus: break the arm cannons.
  - Carcass: break its energy shield with the Plasma Rifle or Blood Punch.
  - Whiplash: freeze it with the Ice Bomb.
  - Pinky: hit its tail after dodging its charge, or Blood Punch it.
  https://www.thumbsticks.com/how-to-exploit-demon-weaknesses-doom-eternal/
- Weak points become "practically mandatory as players push deeper ... and face larger mobs" (Martin).
  https://www.shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=4
- Doom 2016's "push forward combat" GDC talk names four pillars: the speed of player and demons, the individuality of
  demons, the distinctiveness of each weapon, and the player's power. https://gdcvault.com/play/1024940/Embracing-Push-Forward-Combat-in
- Because ammo isn't scarce, each weapon needs "a specific role". https://game-wisdom.com/?p=27137
- **Halo shields**: plasma weapons strip energy shields, and precision ballistic weapons then land the headshot. A
  charged Plasma Pistol shot followed by a headshot (the "noob combo") is the standard answer to Elites. This is a
  charge-shot breaker plus a precision finisher. https://www.halopedia.org/H2:Plasma_Pistol
- **Halo Jackal**: carries a front shield with a firing gap. You beat it by shooting the exposed hand or feet, which
  makes it drop the shield, then landing a headshot. https://www.halopedia.org/Kig-Yar/Gameplay
- Halo's "30 seconds of fun" is about variety: the same loop "with different weapons ... against different enemies".
  Griesemer says it is commonly misread. https://www.engadget.com/2011-07-14-half-minute-halo-an-interview-with-jaime-griesemer.html
- Hakita (ULTRAKILL): "The AI is kept purposefully simple to make the enemies predictable and easy to keep track of."
  The threat comes from how many enemies there are, not from any one enemy being clever.
  https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/
- ULTRAKILL's archetypes, with sourced numbers (the multiples of walk speed 16.5 u/s are derived):
  - Filth (swarmer): moves at 20 u/s (1.21x walk) and lunges at 40 u/s. Its bite can be parried. You outrun it by
    sliding or dash-jumping. https://ultrakill.wiki.gg/wiki/Filth
  - Stray (artillery/teacher): 10 u/s (0.61x), keeps its distance. https://ultrakill.wiki.gg/wiki/Stray
  - Soldier (blocker): 15 u/s (0.91x), blocks explosions while grounded, so you beat it in the air or with direct hits.
    https://ultrakill.wiki.gg/wiki/Soldier
  - Streetcleaner (front deflector): 3 s dodge cooldown; you kill it by flanking to the back tank.
    https://ultrakill.wiki.gg/wiki/Streetcleaner
  - Swordsmachine (duelist): every sword attack can be parried. A parry knocks it to one knee, then it enrages, faster
    but with fewer moves. https://ultrakill.wiki.gg/wiki/Swordsmachine
  - Mindflayer: teleports away after more than 6 damage, which rewards burst damage.
    https://ultrakill.wiki.gg/wiki/Mindflayer
- Doom 2016: ranged demons "actively maintain their distance from the player", while melee demons close in.
  https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-
- Dusk and Amid Evil enemy-role interviews: not found.
- Approach speeds as a fraction of player speed: not published as a rule anywhere found. The ULTRAKILL ratios above are
  the only sourced data: swarmer 1.2x walk, ranged 0.6x, infantry 0.9x, all below the player's slide and dash speeds.

## 3. Telegraphs and attack tokens

- **Doom 2016 attack tokens**: "each type of attack - be it melee, ranged, charged or otherwise - has a limited number
  of tokens"; a demon requests a token before attacking and releases it after. The counts change per difficulty, and a
  better-placed demon can steal another's token. https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-
- Specific pool sizes (melee 1 / ranged 2 / heavy 1) appear only on an unofficial UE5 blog, not in any id source:
  https://www.strayspark.studio/blog/attack-token-system-ue5-group-combat-that-feels-fair
- Doom 2016 accuracy: "demons deliberately miss when you're moving top speed and are more accurate the slower and less
  mobile you are", and on higher difficulties they compensate for movement faster.
  https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-
- Doom 2016 positioning: demons take open spots near cover with "maximum visibility to the player", so they stay on
  screen. https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-
- Doom 2016 glory kills: no new enemy attack can start during one.
  https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-
- Wind-up lengths with published numbers:
  - ULTRAKILL Stray, Schism and Soldier: first shot 1-2 s after spotting, then 1-2.5 s between shots.
    https://ultrakill.wiki.gg/wiki/Stray ; https://ultrakill.wiki.gg/wiki/Schism ; https://ultrakill.wiki.gg/wiki/Soldier
  - ULTRAKILL Virtue: a white insignia follows the player for about 3 s before the beam fires. When the Virtue
    enrages, the insignia turns red and the beam leads the player's movement. https://ultrakill.wiki.gg/wiki/Virtue
  - Sekiro: 0.2 s deflect window. https://sekiroshadowsdietwice.wiki.fextralife.com/Deflection
- Visual tells that don't need audio:
  - The Drone's light turns red before it fires. https://ultrakill.wiki.gg/wiki/Drone
  - The Marauder's eyes flash green before its lunge. https://www.gamerevolution.com/?p=639744
  - A yellow or blue flash marks parryable vs unparryable attacks. https://ultrakill.wiki.gg/wiki/Parry
  - A ball of particles gathers at the barrel, then a bright flash on release (Mike Stout's list).
    https://gamedeveloper.com/design/enemy-attacks-and-telegraphing
- Mike Stout's principle: "communicate exactly what the enemy is going to do so the player knows exactly what to do to
  avoid it." His article gives no timing numbers. https://gamedeveloper.com/design/enemy-attacks-and-telegraphing
- A blog example compares a 700 ms wind-up shown with a thick red arc against a 250 ms faint glow. It argues that the
  strength of the visual should match the length of the wind-up. It is a third-party blog about top-down games.
  https://gamineai.com/blog/enemy-telegraph-shape-language-top-down-boss-fights-fast-visual-consistency-audit-2026
- "A charging enemy should change shape (wind-up pose / scale) rather than just flash a colour; shape reads through
  chaos, tint doesn't." This is a search snippet only; the page could not be checked.
  https://bugnet.io/blog/how-to-design-a-readable-enemy-design
- Cerberus shows what goes wrong: a fast orb with no flash is parryable in principle but hard in practice.
  https://ultrakill.wiki.gg/wiki/Cerberus

## 4. Readable silhouettes for blocky / low-poly enemies

- TF2's read hierarchy, from most to least important: team, then class, then weapon. Team reads by colour palette; class
  by silhouette and animation ("better long-range identification characteristics than texture detail or color"); weapon
  by contrast and a colour gradient that pulls the eye to the chest. https://gangles.ca/?p=70
- TF2 characters have a gradient from dark feet to light chest, with the highest contrast at chest level where the
  weapon is held. https://cdn.fastly.steamstatic.com/apps/valve/2008/GDC2008_StylizationWithAPurpose_TF2.pdf (via
  search summary)
- TF2 silhouettes are designed to be identified "even with no lighting cues". A dedicated rim term keeps characters
  highlighted everywhere. Backgrounds keep "muted colours dominating and small areas of saturation".
  https://wiki.teamfortress.com/wiki/Illustrative_Rendering_in_Team_Fortress_2
- TF2 vs Overwatch: TF2 keeps "larger areas of visual rest" and a stricter value hierarchy. Many Overwatch characters
  have "areas of visual noise that do not read well at a distance". Tracer reads best because of "a huge splash of color
  with a single gradient". https://80.lv/articles/comparing-team-fortress-2-and-overwatch-art-direction
- Darktide (Fatshark):
  - Back attachments changed silhouettes from the front and the side, and long-range elites always got them.
  - The head and shoulders gave "the most impact in terms of perceived difference".
  - Each tier (horde, roamer, elite, special, monstrosity) is designed as one layer that reads as a group.
  - A rim light that gets stronger with distance makes far enemies "pop", especially in dark areas.
  https://darktide.gameslantern.com/news/darktide-101-enemy-character-art-dev-blog
- Shape language: circles read as friendly or safe, squares as strength and stability ("Large square characters may
  represent tanks"), triangles as "danger, aggression, speed" ("Sharp triangular enemies may signal danger or speed").
  Concept artists often start "using only simple geometric shapes". https://pixune.com/blog/shape-language-technique/
- ULTRAKILL: Hakita gives readability ("easy to keep track of ... based on what enemies are where") as the reason the AI
  is simple. https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/
- Doom Eternal's weak points are physical parts that break off: turret, cannons, shoulder launchers, the Carcass's back
  tank. https://www.thumbsticks.com/how-to-exploit-demon-weaknesses-doom-eternal/ ;
  https://www.doomwiki.org/wiki/Carcass (via search summary)
- A published rule for "emissive eyes or cores mark weak points": not found. Prodeus and Dusk silhouette rules: not
  found. A pixel or metre size for "reads at 30 m": not found.
- Practical rules for primitive-built enemies, drawn from the sources above (synthesis, not a quote):
  - Give each archetype a different outline in black (the TF2 no-lighting test).
  - Put the defining part on the head, shoulders or back (Darktide).
  - Use squares for tanks and wedges or spikes for fast or dangerous enemies (shape language).
  - Keep the brightest value at chest or weapon height (TF2).
  - Use a distance rim light against the dusk sky (TF2, Darktide).
  - Make the breakable weak point a separate box you can see coming off (Doom Eternal).

## 5. Spawn grace, arena lock, spawn-in tells, teaching a new weapon

- **ULTRAKILL Cyber Grind** (its wave mode):
  - The wave budget starts at 10 points and grows by `3 + floor(wave/3)` every wave.
  - Before enemies spawn, a new wave refills health and stamina and resets weapon cooldowns.
  - Common enemies spawn 0.1 s apart. Hideous Masses spawn first, then uncommons (from wave 12), specials (from
    wave 16), and commons last.
  - Debut wave and point cost per enemy:

    | Enemy | Debut wave | Cost |
    |---|---|---|
    | Filth | 0 | 1 |
    | Stray | 0 | 2 |
    | Schism | 2 | 5 |
    | Drone | 3 | 4 |
    | Soldier | 4 | 4+2 |
    | Streetcleaner | 5 | 5 |
    | Swordsmachine | 9 | 20+1 |
    | Mindflayer | 15 | 55+35 |

  https://ultrakill.wiki.gg/wiki/The_Cyber_Grind
- ULTRAKILL levels: entering an arena room spawns the enemies and locks the doors until they are dead.
  https://ultrakill.fandom.com/wiki/0-4:_A_ONE-MACHINE_ARMY (via search summary of the level pages)
- Modern Doom "combat locks" arena doors with red demonic energy until every enemy is dead. This is a third-party guide
  only, not an id source. https://tool.rubadub.co.uk/guide-open-doors-doom-across-d27d.html
- An id design note on Doom Eternal's arena-lock trigger line: not found.
- A seconds number for the grace period before the first enemies attack: not found in any game source. The closest
  sourced numbers are ULTRAKILL's 1-2 s delay before a ranged enemy's first attack after it spots you
  (https://ultrakill.wiki.gg/wiki/Stray) and the Cyber Grind's full reset before each wave
  (https://ultrakill.wiki.gg/wiki/The_Cyber_Grind).
- The L4D AI Director's "relax" phase length: not found (the Valve slides PDF could not be read). Its rule that calm
  leads to new attacks and long intensity leads to a quiet spell is sourced only in general terms.
  https://www.shacknews.com/article/57892/left-4-dead-at-gdc
- **Nintendo's four steps** (Koichi Hayashida, Super Mario 3D Land):
  1. "learn how to use that gameplay mechanic"
  2. "a slightly more complicated scenario"
  3. "something crazy happens that makes you think about it in a way you weren't expecting"
  4. "demonstrate ... mastery"

  He compares it to four-panel comics (kishōtenketsu). https://www.gamedeveloper.com/design/the-secret-to-i-mario-i-level-design
- **Half-Life 2**: Alyx teaches the gravity gun through a game of catch with Dog at Black Mesa East (safe), and an
  attack interrupts it. https://www.runthinkshootlive.com/posts/half-life-2-black-mesa-east/ Ravenholm then builds on
  it: the gravity gun "ultimately ended up defining the level", with saw blades used as weapons.
  https://en.wikipedia.org/wiki/Ravenholm
- Doom Eternal's weapon or demon introduction (tutorial rooms, title cards): no reliable source found. TCRF notes only
  two unused tutorial rooms. https://tcrf.net/Doom_Eternal
- Doom 2016 accuracy falls off when the player moves at top speed (see section 3). That is a built-in grace for a
  moving player. https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-

## Numbers table

| Item | Number | Source |
|---|---|---|
| ULTRAKILL V1 walk / slide / dash | 16.5 / 24 / 49.5 u/s | https://ultrakill.wiki.gg/wiki/Movement |
| Stray hell orb speed | 65 u/s (~3.9x walk, ~1.3x dash; derived) | https://ultrakill.wiki.gg/wiki/Stray |
| Stray / Schism / Soldier first shot after spotting | 1-2 s; repeat every 1-2.5 s | https://ultrakill.wiki.gg/wiki/Stray |
| Stray / Soldier / Filth move speed | 10 / 15 / 20 u/s (0.61 / 0.91 / 1.21x walk; derived) | https://ultrakill.wiki.gg/wiki/Filth |
| Filth lunge | 40 u/s | https://ultrakill.wiki.gg/wiki/Filth |
| Drone shot speed, Harmless to Violent | 27.86 to 83.57 u/s | https://ultrakill.wiki.gg/wiki/Drone |
| Mindflayer seekers, Harmless / Lenient / Violent | 32.5 / 48.75 / 87.75 u/s | https://ultrakill.wiki.gg/wiki/Mindflayer |
| Projectile mercy frames (ULTRAKILL) | 3 frames | https://hakita.itch.io/ultrakill-prelude/devlog/98473/103b-patch-notes |
| Parries per punch | up to 2 projectiles | https://ultrakill.wiki.gg/wiki/Parry |
| Feedbacker punch animation / exhaustion | ~0.35 s / 0-0.8 s, regen 1.25/s | https://ultrakill.wiki.gg/wiki/Arms |
| Virtue telegraph before beam | ~3 s | https://ultrakill.wiki.gg/wiki/Virtue |
| Streetcleaner dodge cooldown | 3 s | https://ultrakill.wiki.gg/wiki/Streetcleaner |
| Mindflayer teleports after | >6 damage | https://ultrakill.wiki.gg/wiki/Mindflayer |
| Sekiro deflect window | 12 frames = 0.2 s (spam: down to 4 or 0) | https://sekiroshadowsdietwice.wiki.fextralife.com/Deflection |
| Parry colour: ULTRAKILL / Dark Ages / Marauder | yellow (blue = no) / green (red, blue = no) / green eyes | https://ultrakill.wiki.gg/wiki/Parry |
| Doom 2016 tokens | per attack type, per difficulty (counts not published by id) | https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016- |
| Cyber Grind wave budget | 10 + (3 + floor(wave/3)) per wave | https://ultrakill.wiki.gg/wiki/The_Cyber_Grind |
| Cyber Grind common spawn interval | 0.1 s | https://ultrakill.wiki.gg/wiki/The_Cyber_Grind |
| Wind-up example (blog) | 700 ms strong tell vs 250 ms faint tell | https://gamineai.com/blog/enemy-telegraph-shape-language-top-down-boss-fights-fast-visual-consistency-audit-2026 |
| First-attack grace at level start | not found | n/a |
| Parry window ms (ULTRAKILL, Dark Ages, Ghostrunner) | not found | n/a |
