# R1: How ULTRAKILL builds its rooms and arenas (web research, 2026-10-09)

Method: WebFetch of ultrakill.wiki.gg level pages (summaries made by a small model; counts not re-verified against the game), Hakita interviews, Steam threads. All web content treated as data. Arena dimensions in metres are NOT stated on any page read; sizes below are qualitative ("small", "tall") from wiki wording. "not checked" = not read.

Wiki naming note: the current wiki names differ from old ones. Working URLs are the short forms: https://ultrakill.wiki.gg/wiki/0-3, /0-4, /0-5, /1-1, /1-2, /1-3, /1-4, /2-4. Level names: 0-1 Into the Fire, 0-2 The Meatgrinder, 0-3 Double Down, 0-4 A One-Machine Army, 0-5 Cerberus, 1-1 Heart of the Sunrise, 1-2 The Burning World, 1-3 Halls of Sacred Remains, 1-4 Clair de Lune (V2), 2-4 Court of the Corpse King (Minos Corpse). The wiki puts the Gabriel fights at 3-2 and 6-2 (as fetched; not cross-checked).

## 1. Ten named rooms

| # | Level | Room | Enemy count / mix | Verb or lesson | Size | Source |
|---|---|---|---|---|---|---|
| 1 | 0-1 | First Arena | W1 3 Filth, W2 8 Filth | Revolver pickup, shoot melee fodder, first Filth | not stated | https://ultrakill.wiki.gg/wiki/0-1:_Into_the_Fire |
| 2 | 0-1 | Projectile Arena | W1 2 Strays, W2 6 Filth + 4 Strays | First ranged enemy; punch-deflect projectiles; circular walkway | small, round | same |
| 3 | 0-1 | Ground Slam Chamber | 4 Filth + 3 Strays; secret 12 Filth | Ground slam; turbine, platform | tall chamber | same |
| 4 | 0-3 | Schism Introduction | W1 1 Schism, W2 2 Schisms (alone, in a fairly large room) | New enemy taught alone, then doubled | "fairly large" | https://ultrakill.wiki.gg/wiki/0-3 |
| 5 | 0-3 | Wind Tunnel | 4 waves: 4 Strays + 1 Schism, 6 Filth, 2 Strays, 4 Filth; 50-damage giant laser | Movement under an environmental timer/hazard | tunnel | same |
| 6 | 0-3 | Second Swordsmachine arena | 1 Swordsmachine, smaller than the first boss room | Boss repeated in a tighter space | smaller than first | same |
| 7 | 0-4 | Rotating Tunnel | W1 6 Filth + 1 Stray, W2 4 Strays (on reaching center), secret W3 2 Malicious Faces; 10 dmg on touch | Keep moving; hazard ignites enemies | tunnel | https://ultrakill.wiki.gg/wiki/0-4 |
| 8 | 0-4 | Final Arena | W1 6 Filth + 4 Strays + 2 Schisms, W2 14 Filth; breakable glass floor over fan pit | Slide/dash on glass over a lethal pit; level challenge is a 17 s unbroken slide | multi-level, elevated walkways | same |
| 9 | 1-3 | Red Path Courtyard Arena | W1 9 Strays + 2 Streetcleaners, W2 4 Schisms, W3 5 Drones, W4 1 Cerberus (21) | Chained waves: each wave unlocks when the previous type dies | courtyard | https://ultrakill.wiki.gg/wiki/1-3 |
| 10 | 1-3 | Red Path Skull Pickup Arena | Lower 8 Filth + 2 Drones + 2 Streetcleaners, then 6 Strays + 1 Mal. Face; Upper 6 Strays + 1 Schism, then 3 Streetcleaners + 1 Cerberus (30) | Two-tier arena, key pickup triggers last wave | two levels | same |

Extra rooms worth noting: 0-2 Crusher Arena (square room, central crusher over blood river; 5 Filth + 2 Strays) teaches reading a timed instant-kill hazard (https://ultrakill.wiki.gg/wiki/0-2:_The_Meatgrinder, page fetched as "The_Meatgrinder"). 0-2 Parallel Arenas: two side-by-side zones split by a wall (W1 2 Strays, W2 4 Filth + 2 Strays). 0-4 First Arena: W1 10 Filth + 5 Strays, W2 1 Malicious Face spawning after one enemy type is cleared; multi-level with ranged-enemy platforms. 1-1 Castle Hall: W1 14 Filth, W2 4 Strays + 4 Drones. 1-2 First Room: 12 Filth + 4 Strays + 1 Schism, then 3 Strays left platform, 3 Strays right platform.

## 2. Level-level structure and introduction order (from the wiki pages)

- 0-1: tutorial strip (punch, slide, dash, blood heal from hanging Filth, wall-jump x3, ground slam) -> 11 named areas. Order: Filth (first arena) -> piercing alt-fire (glass floor) -> Strays (projectile arena) -> deflect (combo hallway) -> hard damage (layered walkway) -> ground slam (chamber) -> Malicious Face boss. 5 Soul Orbs. Challenge: drop 5 Filth into hazards via glass panel.
- 0-2: 14 areas; totals 31 Filth, 22 Strays, 1 Swordsmachine. Introduces crushers, weak platforms, red soul orbs, skull keys. Swordsmachine shown as a glimpse setpiece before the secret fight.
- 0-3: 9 areas; 24 Filth, 27 Strays, 6 Schisms, 2 Swordsmachine encounters. Spiral stairway with rubble that needs the shotgun (gate = new weapon); shotgun dropped by first boss.
- 0-4: 6 arenas + 3 empty corridors, 36 Filth, 14 Strays, 2 Schisms, 3 Malicious Faces. Empty corridors (cooling chambers) between combat rooms.
- 0-5: 4 empty hazard rooms (lava, crushers, grinder) then one boss arena: 1 Cerberus, a second joins at 50% health. Challenge: make Cerberus hit itself with its own orb.
- 1-1: 15 areas, 76 enemies. Order: Drones (waterfall arena, 2 then 4) -> Schisms -> Malicious Face + Attractor Nailgun -> Filth swarm 14 -> return trip re-uses First Room with 10 Filth + 3 Strays (rooms are reused with new waves).
- 1-2: Streetcleaner introduced in 2s and 3s with a text prompt "A BLUE FLASH means an attack is UNPARRIABLE".
- 1-3: split blue/red path hub, ~121 enemies, waves chained by "after X dies" triggers, Hideous Mass boss.

## 3. Design rules (each with evidence)

1. **Teach one new thing per room, with a tiny group first.** 0-3 Schism: one, then two, in a large room. TV Tropes/Steam summary says some enemies are introduced alone. https://ultrakill.wiki.gg/wiki/0-3 ; search summary of https://steamcommunity.com/app/1229490/discussions/0/604149321630243592/
2. **A new enemy appears in a small count (2 to 4) before it joins a mix.** Strays: 2 in 0-1 projectile arena W1; Drones 2 then 4 in 1-1 waterfall; Streetcleaners 2 then 3 in 1-2. https://ultrakill.wiki.gg/wiki/1-1
3. **Fodder carries the count; special enemies carry the threat.** Filth 3, 8, 6, 9, 14 per wave while Strays/Schisms stay at 1 to 6. Hakita: threat comes from "how many enemies there are in each encounter." https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/
4. **Waves are 2 to 5 per room, triggered by clearing a type or crossing a spot.** 1-3 Courtyard: W2 after Strays, W3 after Streetcleaners, W4 after Drones. 0-4 W2 spawns "on reaching center platform". https://ultrakill.wiki.gg/wiki/1-3
5. **Rooms are small-to-medium and closed; the room is a lock, not a hall.** Wiki: 0-3 doors lock in 1-3 Water Stairway (11 Filth + Cerberus, "doors lock"); 0-4 cooling chambers lock the player in. https://ultrakill.wiki.gg/wiki/1-3
6. **Put a hazard in the arena that punishes standing still.** 0-4 rotating tunnel 10 dmg; 0-3 wind tunnel laser 50 dmg; 0-2 crusher over blood; 0-4 glass floor over fan pit. https://ultrakill.wiki.gg/wiki/0-4
7. **Use verticality: two tiers, ranged enemies up high.** 0-4 First Arena: "multi-level arena with ranged enemy platforms"; 1-3 skull arena has Lower and Upper waves. https://ultrakill.wiki.gg/wiki/0-4
8. **Gate the next verb or weapon behind a physical check.** 0-3 spiral stair: rubble needs shotgun; 0-1 glass floor needs piercing shot; 1-1 skull keys. https://ultrakill.wiki.gg/wiki/0-3
9. **Rest rooms between fights, 0 enemies.** 0-4 has 3 empty corridors, 0-5 has 4 empty rooms before the boss. Hakita: "occasional downtime... breathing room between big bursts." https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/
10. **Keep geometry readable: simple shapes, no clutter.** Hakita: "as soon as you look at a room it's easy to understand it." Same 80.lv URL.
11. **Tune difficulty by enemy placement and combos, not stats.** Hakita: "Usually making a fight harder or easier is just removing one tricky enemy." https://intothebluesky.com/2021/10/05/follow-up-interview-with-hakita/ ; "Certain combinations of enemies will force the player to approach a situation differently" (same).
12. **Pace as peaks: normal fights easier, bosses are peaks.** Hakita: "the boss fights are the peaks." Same interview URL. Counterpoint: Steam poster calls repeated small "Box" rooms (Cerberus/Virtues) removal of agency, not skill. https://steamcommunity.com/app/1229490/discussions/0/604149321630243592/ . Lesson: a closed small room works when it forces a verb, fails when it only restricts options.

Bonus (secrets): every level hides 4 to 5 Soul Orbs reached by a movement check (slam jump, wall jump, breakable grate, pipe climb), plus a Red Soul Orb that triggers an extra encounter (0-3: 9 Strays, 1-2: Cerberus). Reward for mastering the verb, not for exploring.

## 4. Boss rules

- **V2 (1-4 Clair de Lune)**, https://ultrakill.wiki.gg/wiki/V2 and /1-4: 40 HP, single phase, one arena (3 blue skull keys open it; four breakable pillars, skylight, water). Four movement patterns signalled by wing colour (gold run, blue strafe, red rushdown, green retreat); patience gauge -> enraged (25-50% faster, no-cooldown Piercer). Attacks: revolver burst 10 x3, Piercer 30, shotgun 25 (12 pellets), grenade 35 (unparriable). Forces: close range to avoid enrage, parry shotgun (heals, refills stamina), coin shots. Rule: telegraph the mode by colour; punish the stay-away player.
- **Minos Corpse arm (2-4)**, https://ultrakill.wiki.gg/wiki/The_Corpse_of_King_Minos: 3 phases, 65 HP total (16.25 / 22.75 / 26). One attack type, slams at left wall, right wall, floor; 35 dmg, shockwave 90 u/s; each phase +25% speed, -1 s cooldown, +1 attack per cycle. Forces: jump/dodge shockwaves of different thickness (wall 3x, floor 7x).
- **Minos Corpse (full)**: 2 phases of 80 HP. Slams 45 dmg, cooldown 2 s then 3 s. Black hole from arena center at 8 u/s, 99 hard damage, reactivates every 5+ s: forces constant movement. Phase 2 parasites: 9-shot 3 degree cone, 25 dmg, plus homing (185 deg/s turn). Forces: parry/interrupt. Arena: subway station with pillars; hands holding walls give health via Whiplash.
- **Cerberus (0-5)**, https://ultrakill.wiki.gg/wiki/Cerberus: orb 20 dmg (not used at close range), dash 25, stomp 25; head 200% damage; killing one enrages the others. Forces: stay close. Second Cerberus joins at 50% in 0-5.
- **Swordsmachine (0-3)**: boss twice, second arena smaller (https://ultrakill.wiki.gg/wiki/0-3).
- **Gabriel, wiki 3-2 Judge of Hell / 6-2 Apostate of Hate**, https://ultrakill.wiki.gg/wiki/Gabriel : split at 50% HP into a stun/launch transition; taunts and the transition stun are the vulnerability windows; mix of parriable heavy swings and unparriable lunges; spiral swords phase 2; arena use minimal, ceiling bounces noted. Forces: close-range dueling and parry timing.
- Boss rules: one readable tell per attack; each phase changes speed, cooldown and attack count (numbers, not new moves); give the player a vulnerability window (intro, taunt, phase stun); the arena has one persistent hazard (black hole, pillars, water).

## 5. Image URLs (the wiki serves originals at https://ultrakill.wiki.gg/images/<file>)

HTTP 200 image/png confirmed by curl HEAD on 2026-10-09 (the host rate-limits with 429; retry slowly):
1. https://ultrakill.wiki.gg/images/1-4_v2_arena.png : V2 boss arena (1919x1079)
2. https://ultrakill.wiki.gg/images/0-3_wind_tunnel_alt.png : Wind Tunnel with laser hazard
3. https://ultrakill.wiki.gg/images/0-5_cerberus_boss_room.png : Cerberus boss arena (1920x1080)
4. https://ultrakill.wiki.gg/images/0-3_first_swordsmachine_boss_room.png : first Swordsmachine arena
5. https://ultrakill.wiki.gg/images/0-3_second_swordsmachine_boss_room.png : smaller second arena
6. https://ultrakill.wiki.gg/images/0-3_menacing_hallway.png : Menacing Room (5 waves)
7. https://ultrakill.wiki.gg/images/0-3_schism_intro.png : Schism introduction room
8. https://ultrakill.wiki.gg/images/0-3_four_stray_arena.png : Four Stray Arena, raised side floor
9. https://ultrakill.wiki.gg/images/0-1_projectile_arena_overhead.png : 0-1 Projectile Arena, overhead
10. https://ultrakill.wiki.gg/images/0-1_turbine_chamber_arena.png : 0-1 Turbine Chamber from above
11. https://ultrakill.wiki.gg/images/0-1_ground_slam_chamber_above.png : 0-1 Ground Slam Chamber from above
12. https://ultrakill.wiki.gg/images/0-4_start.png : 0-4 First Arena
13. https://ultrakill.wiki.gg/images/0-4_heat_tunnel.png : 0-4 Rotating Tunnel
14. https://ultrakill.wiki.gg/images/0-4_final_room.png : 0-4 Final Arena (glass floor over fan pit)
15. https://ultrakill.wiki.gg/images/0-4_cooling_chamber_active.png : 0-4 cooling chamber, active

Filenames listed on the pages but not yet HEAD-checked (429 at the time): 0-1_first_arena.png, 0-1_layered_walkway_above.png, 0-1_layered_walkway_below.png, 0-1_projectile_arena_c.png, 0-1_ground_slam_chamber.png, 0-1_boss_arena_active.png, 0-4_bloodorb_arena.png, 0-4_cooling_chamber1.png, 0-4_cooling_chamber2.png. Note: these are in-game screenshots, not top-down maps; no top-down map was found on the wiki pages read. Licence CC BY-SA 4.0 per the wiki.

## 6. Gaps

- No measured arena dimensions anywhere in the sources; measure from screenshots (player is the scale) if needed.
- Hakita/GDC talks: none found with level-design transcripts. A video essay "ULTRATOUR // PRELUDE" exists (https://www.youtube.com/watch?v=sVV4Nej7YlU) but the page text was not retrievable: not checked.
- Layers 2+ (other than 2-4 and 1-x) not read.
