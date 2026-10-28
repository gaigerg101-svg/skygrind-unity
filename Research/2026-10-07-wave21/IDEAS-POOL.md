# Wave 21 idea pool (wave-forge S2: diverge before judging; 2026-10-07 07:50)

Lenses:
1. GG's fantasy
2. the genre's best
3. another genre's trick
4. the map is the level
5. the moment a player shows a friend
6. tech that just became possible
7. a constraint card
8. the debt that hides a feature
9. **the role-play panel**: five personas each pitch, then attack, the slate

What the game looks like now (final reel, 2026-10-07):
- A dusk arena of navy blocks with cyan edges, red-orange enemies, a gold score.
- The rank badge shows a magenta square (the missing-material defect).
- A climbing husk fills the screen as a flat orange capsule.
- The HUD shows the corners only; the title is a clean left column.

GG's words this card set answers (verbatim, docs/memory/OWNER.md):
- "I WANT BUILDING new fetaures that match and ideas, innovate"
- "notable changes ... not changes as in cutting things but additions innovations role played iterate"
- NIGHT16: the UI cheap, kills not crunchy, identity bland; he wants massive rebuilds
- Targets: Steam and hopefully Switch

| id | name | pitch (the player's words) | lens | GG hook | builds on | proof (on a Player build) | cost h | risk / premise |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| C-01 | INSTANT REPLAY | "That was sick, hold on, watch it again." Back/View at round end or on DOWN replays the last 10 s from a chase cam | 5, 6 | identity, crunch | the tape (deterministic), TapeReplay, the views | the replay's checkpoint hash EQUAL to the live run's; reel scenario `replay` | 3 | views assume a live sim: spike it |
| C-02 | KILL CAM | "Oh, THAT's what got me." DOWN shows the killer's last 2 s from its side | 5, 8 | WHAT GOT YOU (F4) | C-01's machinery, DeathRecap | reel `killcam` frames show the killer; S12 0 | 1.5 | same as C-01 |
| C-03 | GHOST OF YOUR BEST | "I'm racing my own ghost on today's DAILY." A cyan ghost replays your best daily run beside you | 3, 5 | identity | the run log, the daily seed | the ghost path matches the best tape within 0.1 m; reel `ghost` | 3 | colour role: the ghost is player-usable cyan |
| C-04 | CHALLENGE CODES | "Beat my code: K7Q-DUSK, round 12, 40k." A seed + mutators + target as a short code on TITLE and RESULTS | 5 | Steam sharing | the seed, mutators, RESULTS | the code round-trips: same seed, same mutators, the same tape hash | 1.5 | none |
| C-05 | THE HOOK | "I hook a ledge mid-air and fling over the horde," or yank a husk to me | 2, 1 | speed you can see | Mover.Impulse (as the shockwave), the shot rays | golden / replays exact with the switch off; THE CLIMB: perches the player can reach rise; reel `hook` | 4 | Mover-adjacent: impulse only, behind a switch, a named task |
| C-06 | WALL-RUN | "I run along the arena wall over their heads" | 2 | speed | Mover | the fingerprint changes; a big Core change | 6 | too costly for one wave |
| C-07 | RAIL GRIND | "I grind the cyan edge of the walkway at speed" | 4, 2 | identity (the cyan edges) | the map's edge lines, Mover.Impulse | grind speed and exit boost measured | 5 | map authoring + Mover |
| C-08 | ROUND GRADES (P-RANK) | "S-rank round! The grade stamps in after every clear" | 2 | crunch, UI not cheap | the style meter, the magenta fix | the stamp's entrance measured (M9 0 pop-ins); grade per round in the run log | 2 | stacks with the debt fix |
| C-09 | THE MAIN QUEST | "Power the 3 beacons, build the DUSK LANCE, then hold the last exfil" | 2, 4 | massive rebuilds, Zombies | THE BEACON, exfil, the FORGE | the bot completes the quest in >= 3 of 10 seeds; reel `quest` | 4-5 | map-night overlap (NIGHT21 queue's quest) |
| C-10 | PERK AUGMENTS | "My SPEED perk's major augment lets me slide-cancel reloads" | 2 | depth | perks, PROGRESS | AugmentTests; the run log shows picks | 3 | numbers: a design block |
| C-11 | DUSK CHARMS | "I took a SECOND WIND charm into the run": 3 one-shot buffs picked before a run | 2 | replay value | progression, the shop | charms used per run in the farm | 3 | balance (MARATHON) |
| C-12 | FIELD UPGRADE | "My shroud makes them lose me for 6 s" on a bumper charge | 2 | build variety | SPECIAL (Y), the shockwave's charge clock | a charge clock test; farm uses per round | 3 | the pad map is full |
| C-13 | BREAKABLE COVER | "I blew the crate apart and they poured through" | 6, 2 | crunch | the kill's block break look | view-only debris first; sim cover = a Core change | 3 | determinism if it blocks rays |
| C-14 | THE DUSK CLOCK | "By round 10 the sun is gone and the neon comes on" | 4, 1 | identity bland | the sky, PostFx, the palette roles | sky luminance per round on the reel (a curve); colour roles held (S11-style check) | 3 | colour roles; readability at night |
| C-15 | SUPPLY POD | "A pod screams down; I race the horde to it" | 2 | crunch, set pieces | the drop table, the director | pods per run in the farm; reel `pod` | 3 | none big |
| C-16 | PERSONA PLAYTESTERS | five bots with personas (rusher, camper, kiter, newbie, completionist) play every build and say what they never used | 9, 6 | iterate | the autopilot, the sandbox bots | a persona table per round in the log | 2 | tooling, not player-facing: the wave's loop, not a headline |
| C-17 | TRICK CHAIN | "SLIDE > AIR KILL > SLAM > HEADSHOT x4 = TRICK 12" (Tony Hawk) | 3 | speed, style | the style meter, movement events | chain events in the run log; reel `trick` | 3 | overlaps the style meter: make it its feeder |
| C-18 | OVERDRIVE | "Meter full, both sticks: 8 s of infinite boost and ricochet rounds" (a fighting game's super) | 3, 1 | crunch, wow | the style meter, SKYGRIND TIME | OverdriveTests; uses per run 1-3 in the farm | 3 | MARATHON balance: a block |
| C-19 | CONTRACT DOORS | "Every 5 rounds I pick a door: double points + FOG, or a FORGE tier + elites" (Hades) | 3 | depth | NEXT line, twists, THE BEACON | picks per run; MARATHON M-1 holds | 3 | UI between rounds |
| C-20 | WONDER WEAPON (W-1) | the VORTEX anchor gun from Reports/N20-wonder-weapon.md | 2 | Zombies | the proposal | per GG's name | 4 | GG names it first (DECIDE) |
| C-21 | PHOTO MODE | "Paused, flew the camera round, framed the kill" | 5 | identity, Steam | the camera stack | reel `photo` | 2 | low value alone |
| C-22 | GYRO + FLICK STICK | "On the Deck I aim with the motion sensors" | 6 | Steam Deck, Switch | Input System; GamepadMotionHelpers (MIT) | gyro tests; a pad-pass reel | 3 | Steam Input often owns gyro |
| C-23 | HAPTICS LANGUAGE | "Every kill thumps; the shockwave rumbles left to right" | 6 | crunch | the event bus | rumble events per scenario logged | 1.5 | not audio: allowed |
| C-24 | LAST STAND | "Downed, pistol only, 3 kills in 10 s and I'm back" (Borderlands' Second Wind) | 7 | losing is fun | DOWN, WHAT GOT YOU | LastStandTests; revives per 40 runs | 2 | MARATHON M-1 shift |
| C-25 | MOVEMENT GYM | "3 minutes in the Arena and I know boost, slide, slam, hook" | 7 | first fifteen | the Arena scene, ghosts (C-03) | first-run gym completion by the newbie bot | 3 | tutorial scope |
| C-26 | THE BOSS FIGHTS | the BULWARK on at rounds 10 / 20 with the slow-mo finisher; the bot taught to fight it (EN's plan) | 8 | set pieces | the BULWARK (web port), the boss bar | `SimHarness boss`: kill 60-150 s, win >= 80 % | 4 | the bot's boss plan |
| C-27 | RANK STAMP (the debt) | the magenta square becomes an inked stamp letter that slams in per rank | 8 | UI not cheap | the style badge | 0 magenta pixels on every reel frame; the stamp's M9 entrance PASS | 1 | P0 |
| C-28 | NIGHT MUTATORS FROM THE SKY | "BLOOD MOON round: everything red, double gibs, double points" | 4 | identity | twists, the dusk clock | twist per run; reel | 2 | colour roles (red = danger) |

## The role-play panel (lens 9; the same personas attack the slate in S7)
- **The Zombies veteran:** "Where's the quest? I want a reason to stay to round 30." Picks C-09, C-10, C-24, C-26.
- **The ULTRAKILL speedrunner:** "Give me a hook and a P-rank, and let me race myself." Picks C-05, C-08, C-17, C-03.
- **The Steam Deck couch player:** "Gyro, rumble, and a HUD I can read at arm's length." Picks C-22, C-23, C-27.
- **The streamer:** "The replay IS the content." Picks C-01, C-02, C-04, C-21.
- **GG:** "Crunchy kills, an identity, UI that isn't cheap, notable additions." Picks C-14, C-18, C-01, C-27.
