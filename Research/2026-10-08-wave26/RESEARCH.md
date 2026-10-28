# RESEARCH.md (wave 26): wall riding, ULTRAKILL weapons and ranks, chase / skating bosses, Terraria worms

Done 2026-10-08, five web searches (30 minutes box). Search results were summaries: where a claim comes only from a snippet it says so. "Us" marks our inference.

## S1. Wall riding that feels fluid
Source: https://www.theouterhaven.net/2020/03/a-conversation-with-the-ghostrunner-development-team and search snippets on Titanfall 2 (a read of the Ghostrunner page was not done: snippet only).
- Titanfall 2 builds its levels around wall running: maps have verticality and momentum; the game **rotates the view to follow a curved wall**; coyote time lets a jump fire just after leaving a wall or ledge.
- Ghostrunner is fluid by **restraint**: the transitions between run, wall run, slide and dash are smooth, but a bad landing or a dash that stops just short of a wall kills the momentum almost at once; it is not punishing, it is moderated.
- **Us (Mover.cs is frozen: golden vectors, rule 3):** the engine already has wall ride, wall jump and wall kick (the style meter shows `WALLRIDE`). Fluidity we can add WITHOUT touching the Mover: (1) level geometry made of long, continuous wall runs with gaps sized so a wall jump or dash bridges them (the existing flue was 3.2 s vs 8.6 s: a measured reward); (2) camera: the wall-ride roll / pre-tilt exists (`feel.Wire`): tune it and add a short FOV lift while riding; (3) a visible guide: a light strip along rideable walls (cyan = player-usable) so the player sees where to ride; (4) a reward: style points and a speed bonus per ride, par times that assume the wall route.

## S2. ULTRAKILL: every weapon has a strength, a weakness and an alt fire; switching is the game
Source: https://ultrakill.wiki.gg/wiki/Weapons (search result snippet).
- Enemies have varied weaknesses and strengths so the player must switch weapons often; weapons come in variants; each has a primary and an alternate fire (unique, often stronger, on a cooldown), and alternates combine across weapons.
- Nailgun: sustained midrange, resource-limited, used in bursts in swap combos; the saw variant pierces and ricochets (crowd control); shotgun: close-range damage, heals from blood, can parry; coin ricochet is the answer to certain enemies; the wheel slows time while selecting.
- **Us:** the answer to "I don't feel incentivised to switch" is not weapon numbers alone: it is ENEMY-WEAPON PAIRING: each enemy kind gets a vulnerability (a shield that only the rail or a punch breaks, a swarm that melts to splash but not to single shots, a charger that must be hit in its windup, a flyer that needs a hitscan) and each weapon a limit (ammo / heat / charge / cooldown) so that the one best gun for the whole game does not exist. GG's list: rocket and grenade (BOUNCER) clear rooms too fast: give both a high self-cost (slow fire, splash that hurts the player, a limited count) and no crowd-clear ceiling; every weapon gets an LT that zooms or does something else (GG: "almost all weapons").

## S3. ULTRAKILL ranks: time, kills, style, aggregate, level select shows the best
Source: https://ultrakill.wiki.gg/wiki/Ranking (search result snippet).
- Each level ends with ranks for TIME, KILLS and STYLE (D to S, thresholds per level); an aggregate rank sums points (S 5, A 4, B 3 ...) minus one per checkpoint restart; P rank = S in all three without restarting; the best aggregate per level is recorded and shown on the level select.
- **Us:** our Core already computes per-room S/A/B/C by time over par and deaths (`Campaign24.StampText`, ranks) and the results screen shows rows; missing: kills and style as ranks, an aggregate with restarts, a persisted best per level, and the LEVEL SELECT screen (all levels, locked / unlocked, best rank, best time, the weapon unlocked at that level).

## S4. Chase and skating bosses (Sunset Overdrive; Sonic; SSX)
Source: https://www.trueachievements.com/game/Sunset-Overdrive/walkthrough/2 and a TV Tropes grind-boots page (snippets).
- Sunset Overdrive's island boss is "on rails": coils around a tower you grind on, you leap between spring drums and rails while avoiding fire, and the style meter punishes stopping; switching between grinding above and below dodges mortar attacks.
- **Us (GG: the second level's boss is a chase like JoJo part 5's Ghiaccio highway: you skate, you chase, you dodge telegraphed attacks, close in, hit it):** one long straight course in 40 m tiles that recycle (DESIGN24-bosses.md section 2: the gap meter), ICE surfaces and boost strips (low friction, high speed: the SURFACE feature below), rail / wall lines that skip hazards, the boss ahead throwing telegraphed lane attacks; damage only when within a gap of 25 m or closer; the pistol only (GG).

## S5. Terraria worm bosses
Source: https://terraria.wiki.gg/wiki/The_Destroyer and /wiki/Worm_AI (snippets).
- The Destroyer: segments share ONE health pool and cannot die; the head deals the most contact damage (70), the body less (55), the tail least (40); every segment fires lasers; the Eater of Worlds loops around the player and burrows, attacking with contact damage with each passing segment; its head has less defence than a body segment.
- **Us (GG: hit the body = less damage, hit the head = more; if the boss is close it should go after you):** our worm already uses one shared pool with body x1, head x2, core x3 (DESIGN24-bosses.md): change to body x0.5 (config), head x2; add PROXIMITY AGGRO: when the player is within a radius of any body piece or the head, the head breaks its path cycle and dives at the player (a telegraphed 0.5 s re-aim, cooldown), then returns to the cycle: that is Terraria's relentless chase without a new attack.

## What changed in NIGHT26.md because of this
Wall-riding fluidity through geometry, guide strips, camera and rewards (no Mover change); weapon-versus-enemy pairing with a cost on every weapon; ULTRAKILL-style three ranks + aggregate + level select; a chase boss for level 2 on ice; the worm's body / head ratio and proximity aggro.
