# R2: Genre rules for forcing a mechanic without locked doors

Web research, 2026-10-09. All page content treated as data. Quotes are under 15 words. Where a page would not load or a number was not found, it says "not found". Items marked INFERENCE are my reading, not a source's claim.

Gaps up front: GDC Vault talks (DOOM "Push Forward", Titanfall "action blocks") are paywalled, so only their abstracts were read. PC Gamer, Epic Store, Mapcore, Valve wiki sv_friction and func_friction pages returned 403. No source gave DOOM Eternal arena dimensions in metres or ULTRAKILL arena sizes. No source gave a numeric ice friction for Mario, Trackmania or Source ice.

## Ten rules for forcing a mechanic with a device

1. **Make the device the only free answer, and give the tool in the same room.** Neon White's rule set: "The player always has the right tool for the next encounter." Its levels also never ask for platforming, shooting and card management at once. Evidence: https://www.gamedeveloper.com/road-to-igf-2023/-neon-white-and-designing-for-player-creativity

2. **Build each encounter alone, then chain.** "Each encounter in the level is blocked out in isolation, then assembled." Prototype one arena per mechanic and combine only after each reads on its own. Same source as rule 1.

3. **Teach in the Mario four-step: safe, consequence, twist, optional finale.** "Introduced in a very safe way", then "Consequences are introduced", then "a twist". Put the first ice or gap with no hazard under it, then add the hazard, then add the twist. Evidence: https://byrdseed.com/what-we-can-learn-from-super-marios-levels

4. **A mechanic gets one sequence, then is retired.** Mark Brown's Titanfall 2 analysis, as summarised in search results, says tension mechanics appear in one sequence and are then dropped. Effect and Cause ships with its time-shift only in that level, and the two maps needed "perfect vertical alignment". Evidence: https://en.wikipedia.org/wiki/Effect_and_Cause (the GMTK video itself was not read, only a summary of it).

5. **Enemies are the lock; their weakness is the key.** DOOM Eternal's Arachnotron turret dies to one scoped shot; Cacodemon takes a grenade in the mouth; Mancubus is safe only at range. "Sometimes by giving people more choices, you're actually giving them less" means a hard-gated weakness drives the weapon swap, and the tool must be fast to reach. Evidence: https://blog.adamatomic.com/post/613311014289768448/design-of-doom-eternal and the Epic summary linked from search (not fetchable).

6. **Make the wrong move cost, not forbid it (accuracy tied to speed).** DOOM 2016 AI: "Demons deliberately miss when you're moving top speed." Standing still gets you hit, so the arena pushes motion without a rule. Evidence: https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-

7. **Choke enemy simultaneity with tokens, so one mechanic can be read.** The same DOOM 2016 talk gives each attack type a limited token count per difficulty. A floor hazard room should cap simultaneous attackers the same way. Same URL.

8. **Use arena type to choose the problem.** Adam Atomic: canyon arenas are "continuous", jungle gyms have "distinct floors" so mobility can "interrupt" combat. A low ceiling or a gap suits a jungle gym; a lane suits a canyon. Evidence: https://blog.adamatomic.com/post/613311014289768448/design-of-doom-eternal

9. **Foothold first, committed entry second.** Level Design Book: give a "foothold" to survey the room, or use a one-way entrance (drop, airlock) "forcing committed engagement". A one-way surface replaces the locked door. Also keep tells at 2 to 3 seconds. Evidence: https://book.leveldesignbook.com/process/combat/encounter

10. **Signpost the route and the shortcut as part of the stage.** Neon White: optional paths must "look like part of the stage", and a plank is placed "as a final nudge". Red doors and green ivy mark the way at speed. Evidence: https://gameinformer.com/feature/2022/11/25/how-a-neon-white-level-is-made and the IGF interview above.

Supporting notes (not numbered). ULTRAKILL keeps levels "linear" and geometry "simple and clean" with simple enemy AI, and offsets strong movement with encounter density (https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/). Neon White hides medal times, ghost and shortcut hints until the player has proved the level, which is a teaching gate with no door (IGF interview above). Rail and lane devices: Sonic Stadium notes rails "lead across gaps and bottomless pits", and that Sonic Colors turned rails into automation; INFERENCE: a snap lane works when the player keeps a decision (switch, jump off), else it is a cutscene (search summary only; the page itself returned 404).

## Ice that feels icy

What the sources support:
- **Quake III (code, read in full):** on a SURF_SLICK floor, ground friction is skipped entirely, and ground acceleration drops from `pm_accelerate = 10` to `pm_airaccelerate = 1`, a 10x cut. Base `pm_friction = 6`, `pm_stopspeed = 100`. So ice = no braking plus air-like steering. https://raw.githubusercontent.com/id-Software/Quake-III-Arena/master/code/game/bg_pmove.c
- **Source SDK 2013 (code):** both friction and acceleration scale by `player->m_surfaceFriction`: `friction = sv_friction * m_surfaceFriction` and `accelspeed = accel*frametime*wishspeed*m_surfaceFriction`. `sv_friction` is 4 in CS and CS:S and 5.2 in CS:GO. Ice's numeric surfaceFriction value: not found. https://raw.githubusercontent.com/ValveSoftware/source-sdk-2013/master/src/game/shared/gamemovement.cpp and https://developer.valvesoftware.com/wiki/Sv_friction (via search result)
- **Game feel rule of thumb:** friction about 25 to 30 percent of acceleration is "balanced"; 60 to 100 ms to full speed feels snappy, 500 ms "floaty". Ice is the inverse: slow rise, almost no drop. https://www.gamedeveloper.com/design/game-feel-tips-ii-speed-gravity-friction
- **Keep it controllable:** apply "a small explicit deceleration toward zero when there is no input", scale acceleration down on ice, and clamp top speed (search summary of forum guidance; low authority).
- **Trackmania:** ice has "no grip whatsoever and braking has little effect"; a variant "no grip ice" removes slide-outs; slope matters (a 7 degree slope adds forward momentum). Search summary only. https://en.wikipedia.org/wiki/Trackmania_(2020_video_game) was listed, but not read.
- **Mario:** ice "stalls his momentum and can lead to imprecise jumps" (SM64); tiptoeing buys precision at a speed cost; Yoshi slides down ice slopes even standing still. No numbers on the wiki. https://www.mariowiki.com/Ice
- **Consequence is what makes ice read as ice:** poor traction "makes it more likely to slide off the edge" into pits or hazards (search summary of Mario ice levels). INFERENCE: put a hazard at the lip of the sheet so momentum has a cost.

Cues: camera, FOV, particles and sound cues for ice were searched and no usable source was found. Treat these as untested design suggestions, not evidence: a distinct floor material and gloss, skid particles at heading change, a footstep or slide sound change, a brief FOV or camera sway tied to carried speed.

Teaching ice before a boss: follow rule 3. Safe flat sheet with no hazard, then a sheet with a lip hazard, then a twist (slope or moving enemy), then the boss on the same surface. Super Mario Galaxy's Freezeflame has four of six missions on ice and an ice-side boss (https://mariowiki.com/Freezeflame_Galaxy), but no source states its teaching order.

## Boss windows

- **DOOM Eternal Marauder (best match).** Hugo Martin calls it a "black belt" demon. It "falters for a specific amount of time" so you get one shot with most weapons but two if you swap weapon. It forces the arena clear to go fast: "I need to fight him and clear the field." Vulnerability is caused by the player's timed action (counter shot when eyes flash green). Frustration came from players who "didn't understand what to do". Evidence: https://www.pcgamer.com/au/doom-eternals-director-says-the-marauder-is-good-actually (read through search summary, page 403) and https://techraptor.net/content/quakecon-2019-doom-eternal-interview-marty-stratton-hugo-martin (403).
- **Reward the better move rather than force it:** the swap yields two shots instead of one (same Marauder source). INFERENCE: a boss window long enough for the basic move, longer for the mechanic you want taught.
- **Tells and rhythm:** 2 to 3 second anticipation windows; mix single attacks with combos; make players relearn chains (Level Design Book encounter page).
- **Early bosses as loose tutorials,** e.g. God of War's first Valkyries (same page).
- **Titanfall 2 bosses:** per search summary, bosses that used the level gimmick (Viper, Slone) were remembered; Kane and Richter "were wholly removed from the level's gimmick" and were not. Rule: tie the boss to the device the level taught. Low authority (forum-type summary, not fetched).
- **Pursuit and flee:** Resident Evil's Nemesis works through "paranoia" and persistent threat, and the remake lost it by moving to "obvious set pieces". Player choice of fight or flee exists in contained spaces like the courtyard. Evidence: https://www.dreadcentral.com/editorials/493049/monster-mania-the-hunt-never-ends-with-resident-evils-nemesis/. INFERENCE for SKYGRIND: a chase boss in a small closed arena needs a pursuer that is slower than a full-speed player but cuts off lanes, and a catch that costs rather than kills.
- **Floor-is-lava style attacks and phase telegraphs in FPS movement games:** no source found beyond the above. Not checked: Hades, Dark Souls (excluded as not clearly relevant).

## Room sizes and counts

- **Quake 1 (Level Design Book metrics):** small room 256 u wide, medium 512, large 1024+; hallway 128+; ceilings low 64, regular 128, tall 256+; player hull 32x32x56, max run 320 u/s, running jump 244 u, standing jump 43 u high, rocket jump about 320 long and 128 high. A rule: put monsters in spaces at least double their size. https://book.leveldesignbook.com/process/blockout/metrics/quake
- **Doom 1993:** small room 256 to 512, medium 512 to 1024, large 1024 to 1536; narrow hallway 128; typical Episode 1 map about 4000 u across; ranged attacks to 2048 u, melee within 64 u; about 32 map units to 1 metre is a rough conversion, so a medium room is roughly 16 to 32 m. https://book.leveldesignbook.com/process/blockout/metrics/doom
- **Enemy types per encounter (Level Design Book):** 1 type is a tutorial or rest, 2 regular, 3 complex, 4 is 2v2 factions, 5+ chaotic and the player focuses on a subset anyway. "Most of the time, use only a few enemy types." DOOM Eternal exceeds this: "7+ enemy types at a time" (game-wisdom summary of GDC 2018 talk, https://game-wisdom.com/?p=27137, only seen in search). About 3 waves per encounter (search summary).
- **DOOM Eternal and ULTRAKILL arena dimensions and per-arena enemy counts: not found.** Titanfall 2 campaign encounter dimensions: not found. Quake or Reflex Arena map sizes beyond the Quake 1 metrics above: not checked.

## What would close the gaps (suggested, not done)
Watch the Titanfall "action blocks" and DOOM "Push Forward" talks directly; measure SKYGRIND-relevant numbers from DOOM Eternal and ULTRAKILL arenas in the game itself; read the Source `surfaceproperties.txt` ice entry from a local install; test camera, FOV and particle cues for ice blind and paired, as the project's L-001 requires.
