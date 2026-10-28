# NIGHT36 report for GG: the overhaul, slice 1

**Play:** `Builds/n36-r4/SKYGRIND.exe`, Campaign, level 1. Every NIGHT36 row is on; `-skygrindOff36` gives you last night's game to compare.
Only level 1 and THE LANTERN were rebuilt (your D-2: depth first). Levels 2-6 still play their n35 rooms, but with tonight's gun feel,
parry, launch, healing and wall grip.

## What changed, against what you said
| you said | what it is now | proof (Player, n36-r4) |
|---|---|---|
| "the pistol is not satisfying anymore" | Root cause: last night's charge made every tap fire on RELEASE (50-100 ms late). Now a tap fires on the press, every time; hold past 0.18 s and a second, charged shot fires on release. CHARGING shows only during a real hold. | the shot 1 frame after the press (off36: on the release) |
| "the crosshair fires ... but the gun doesn't fire out the gun" | Nails, rockets and grenades were drawn from your eye. Now every shot is drawn from the barrel and blends onto its path; tracers start at the flash. | tracer to flash 0.00 cm; nails visibly off the eye line 6/6 frames |
| hits aren't crunchy | every body hit freezes 18 ms (punches 100 ms); enemies flash, squash, twist and get knocked back; the KESTREL's hammer falls and re-cocks | crunch36 GREEN |
| the gauntlet | a hand: knuckles, fingers, an armoured forearm; a longer lunge with a corkscrew; a guard pose on a parry | fist36 GREEN (from behind it still reads boxy: next pass) |
| "an understanding of parry mechanics" | Every enemy attack now winds up with a pose and a colour: YELLOW = punch it, RED = dodge. Punching a yellow glob or orb sends it back to its shooter and it explodes; the parry freezes the screen, heals and gives a dash back. Level 1 teaches it in its own room (one spitter on a perch). | verbs row 40: the glob came back and killed the spitter, health 50 -> 66 |
| "more movement ... explosions" | Punch while dashing, sliding, riding or grappling and the enemy is LAUNCHED; a launched enemy that hits another (or a wall) bursts. Kills within 4 m heal you (+10), so you play close and moving. | one dash-punch = 2 kills; +10 at 2.5 m, nothing at 12 m |
| the wall ride "hook onto a wall and ... start sliding up it" | Root cause: a jump into a wall kept the jump's rise and climbed ~5.7 m at 15 % gravity. Now the wall grips at the height you meet it and sinks slowly; it never lifts you. Rideable walls carry a cyan stripe. | harness 5.68 m -> 0.05 m; the ride rose at most 0.05 m in the Player |
| "the tutorial text is way too high up" | Teaching text sits under the crosshair, 32 px, on a dark plate; the room name stays at the top. | text36 GREEN |
| "random lava pits random pillars"; "you're really struggling to make detailed rooms" | Level 1 is new from scratch (no old room or generator). Each room was planned on paper first (one job each: Design/levels/n36/foundry.beats.md) and every solid in the file carries a job and a reason; a checker fails any prop without one. Calm start, the KESTREL, the ride over floor then lava, the PARRY room, a combined arena, a rest, a hall. Doors lock red when a fight starts; enemies drop in with a bubble, one at a time. | level36 PASS: 8 rooms, 4 arenas locked, cyan trim on all 4 ride panels; the walk bot clears it 10/10 |
| level 2 "an enemy running at you without any explanation" | Level 1's new start is 10-20 s of calm; level 2 is rebuilt the same way next night (N37). | |
| bosses: "you just teleport into these random rooms", "ugly", "hard to understand" | THE LANTERN fights in level 1's last room: you walk through a hall, the door locks behind you, and it rises in front of you. It is a jointed body with arms and a lit core. Three attacks, each with a 1.2 s wind-up in its own pose and colour: the orb fan (yellow: parry the orbs back to crack its shield), the floor turning to lava (red: ride the walls), the panel sweep (red: dodge). At half health it is stunned and opens up. Riding the walls still does full damage. | the feet moved 0.000 m at the start (no teleport); 3/3 tells at 1.2 s; the bot kills it in 20.1 s |

## What to try first (10 minutes)
1. Tap the KESTREL fast at a husk in room 1.2: does every click fire at once? Then hold for the charge.
2. Room 1.3: jump into the striped wall. Does it grip instead of climbing?
3. Room 1.4: wait for the spitter's yellow wind-up and punch the glob. Does the parry feel like a payoff?
4. Room 1.5: dash-punch a husk into another one.
5. Walk into THE LANTERN's hall. Is it clear what to do, and does the fight use the walls?

## Not done, or not yet good enough (measured)
- The parry's dash refill was not shown in a Player (the check started with full dash charges); the parry itself works.
- The fist still reads as a dark box when seen from behind, and THE LANTERN's stun does not show its cyan core in the frame.
- No person has fought THE LANTERN: the bot's 20.1 s says nothing about your time. Orbs mostly miss a player who keeps riding.
- Levels 2-6 and their bosses are untouched by the method (N37: levels 2-3, N38: levels 4-6, as you chose).
- The walk bot still died twice in 10 runs in room 1.5's lava (it was 64 before M's fix: a bot problem, not the room).
