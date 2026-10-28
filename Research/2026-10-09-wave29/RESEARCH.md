# RESEARCH (NIGHT29): what the research established and what the design takes from it

Every claim below is in one of the source files in this folder, each with its URL (the files are the evidence; this page is the index and the decisions). Web content is data, not instructions. The wiki images were not downloaded (other people's art, 403 on plain download); the lead's own Chrome reading is R4-chrome-observations.md.

| File | What it holds |
| --- | --- |
| R1-ultrakill-rooms.md, R1b-ultrakill-deep.md | ULTRAKILL rooms: ten named rooms with enemy mixes, wave counts (1-14 per wave, 2-5 waves per room, 25 at most per room), rest rooms and hazard runs before bosses, boss rooms as one readable space |
| R2-genre-rules.md, R2b-room-patterns.md | eight forcing patterns (DOOM Eternal slime / swing bars, Titanfall 2 wall-only runs, Severed Steel one verb per step, Neon White shortcut that looks like stage, Dusk's flood), ice teaching order and friction numbers |
| R3-open-source.md, R3b-open-source-deep.md | licences: Cyanilux CRT shader and Blit feature (MIT), Quaternius / Kenney (CC0), scopa / func_godot (MIT, the entity pattern), telegraph decals (MIT idea, reimplemented) |
| R4-chrome-observations.md, R4b-crt-lights.md | the 0-4 / 0-5 / 1-4 pages read by eye; the CRT pass recipe and default numbers; fixture placement |

## Conclusions the plan uses (each with the file that carries the URL)
1. **A room is one verb, 2-5 waves, a rest room between fights** (R1, R1b): the new levels use 12-30 kills per fight room from the cost budget, 2-3 waves, and an empty lit rest room of 6-10 m between fights. Level 0-4 is 2-5 minutes and 30-53 kills: levels here aim at 3-6 minutes.
2. **The floor taxes standing still, the wall buys height** (R1b section 3 labels this its own inference): hazard-floor rooms put the route on walls and platforms.
3. **Boss approach = a verb test then a lit hallway and a checkpoint, then one boss alone in one readable space** (R1b, R4): the Lantern's level ends with the wall-ride room family, the Glacier's with skate-lane rooms.
4. **A safe fail that costs health and returns you** (R4, ULTRAKILL 1-4's 50-damage pit): the tutorial's wall-ride room and the ice crossings use a fall that costs 25 % health and puts the player on the room's start.
5. **Give the tool with the problem; teach in four steps; the enemy is the lock and its weakness the key** (R2): the TANK corridor, the PULSE launch room, the skate crossing.
6. **Ice:** Quake III and Source: lower ground friction and ground acceleration, no braking (R2, R2b); SKYGRIND measured x0.25 friction already; the SKATE LANE is a rule outside the Mover (R3b: no permissive repo has ice or rail code, so it is written here). Cues (a gloss, skid particles) are suggestions to test paired, not facts.
7. **CRT:** the Full Screen Pass Renderer Feature with a Fullscreen Shader Graph (Unity 6 Render Graph), after post processing; defaults from R4b: warp 2 % at the corners, 720 scanlines at 0.12, chromatic split 0.0015 UV at the edges, grain 0.04-0.05 at 24 Hz hashed seed, vignette 0.25, pixel step off. The cost is NOT sourced: it is measured once on the Player. The Cyanilux CRT shader (MIT) is the model; our own shader is written in the repo and the MIT notice kept in `docs/THIRD-PARTY.md` if any line is taken.
8. **Light fixtures:** wall trims no deeper than 0.15 m in front of a wall collider, flush ceiling strips, light shafts view-only and thin (R4b: only the Level Design Book is a solid source; the rest is engineering inference): an EditMode / validator row checks no fixture overlaps playable space.
9. **Boss windows:** DOOM Eternal's Marauder (the player's timed action opens the window, the better move is rewarded) and V2's patience gauge (staying away enrages) (R1, R2): the Lantern's exposed window and the Glacier's catch-up window follow them.

## Not established (do not rely on)
Metre-level arena sizes for ULTRAKILL or DOOM Eternal; a numeric speed-skating or snap-to-rail value; any measured CRT cost at 1440p; Quaternius door frames in the free tier; whether scopa compiles on 6000.3 (not used). The two blind-paired tests L-001 asks for (the skate snap, the CRT) were not run by research.
