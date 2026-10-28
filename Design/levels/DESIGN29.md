# DESIGN29: the NIGHT29 plan (levels 1-3 from scratch, two bosses, CRT, the bug list)

Written 2026-10-09 15:10 from `Research/2026-10-09-wave29/RESEARCH.md` and its files, NIGHT29.md and GG's words. A card is a promise the validator, the bot and a Player frame must keep; a card that fails after two fix rounds is cut and logged.

## A. The room standard (numbers)
- A fight room: 14-20 m wide, 20-30 m long, 6-12 m tall, CLOSED (collider roof + view roof, no sky gap). A teach room 8-16 m wide, 10-24 m long. A rest room 6-10 m long, 5 m wide, no enemies, a prompt or a pickup, lit differently.
- Every fight room: one surface faster than the floor (a wall lane 2.5 m, a rail, an ice lane) and one thing a standing player loses to (a hazard strip, a ranged enemy with a lane, a shield that opens to a verb).
- No two consecutive rooms share a footprint; rooms are L, ring, shaft, pit-and-bridge, crossing, not only boxes.
- Kills per fight room 12-30 by `design.n27.budget` (husk 1, swarmer 0.5, spitter 2, heavy 6); 2-3 waves; the tutorial's rooms have 1-8.
- Lights are SOCKETS in the plan (`lights` rows: kind wall / ceiling / pool, position, colour, range, flush to a surface). Props are AUTHORED (`props` rows). No random scatter anywhere. A door is a FRAME sized from the wall opening (gap 0).
- Light: dark walls, cyan / green neon trim wall-mounted and ceiling-recessed, ONE accent light per room, a pool at the goal; red-orange = danger only; magenta = rival only; pale violet-white = boundary only. Edges dark, fight bright.
- Each level plays 3-6 minutes for the pilot. Fall = a cost of 25 % health and a respawn at the room's start (no level loss); a death restarts the room (existing campaign rule).

## B. Level 1: THE FOUNDRY (plan id `n29/foundry`; pistol, fist, PULSE gauntlet only)
Card: `id | footprint w x l x h | verb | forcing device | enemies | weapon and why | fail | text (<= 8 words) | light`.
| id | footprint | verb | forcing device | enemies | weapon | fail | text | light |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| f1 INTAKE | 8 x 14 x 5 | move, look | a narrow lit corridor ending in a door that opens when the player reaches the pool | 0 | none | none | "MOVE. LOOK. WALK TO THE LIGHT." | one cyan pool at the door, trim low on both walls |
| f2 STEP | 10 x 18 x 6 | jump | a 3.5 m gap with a kill-floor strip 4 m down (a ledge 1.5 m below) | 0 | none | fall: -25 %, back at f2's start | "JUMP THE GAP" | cyan trim, amber strip on the pit floor |
| f3 DASH | 10 x 20 x 6 | dash | a 5.5 m gap (dash-only, validator max 6.5) with a visible far pad | 0 | none | as f2 | "JUMP, THEN [L] TO DASH" (pad shows the key) | green trim |
| f4 SHAFT | 12 x 12 x 14 | wall-jump | a vertical shaft: the exit is a 12 m climb between two facing walls; floor is the pit | 0 | none | as f2 | "JUMP WALL TO WALL" | cyan trim rising, a pool at the top |
| f5 WALL-RIDE ROOM | 14 x 24 x 10 | wall-ride, the slow-mo aim | HAZARD FLOOR: after 3 safe seconds the floor damages CONTINUOUSLY (8 per second, no off phase longer than 1 s; a 24 m floor crossing takes about 3.5 s at run speed so the floor route costs about 28 health and the lane route nothing); the only route to the exit ledge is one 2.5 m wall lane along the long wall; an example husk stand-in on a ledge across the room (static, then strafing) dies only to shots fired from the wall in the slow-mo | 1 static + 1 strafing target (husk stand-ins) | pistol | fall / hazard hit: -25 %, respawn at start; clear on `wallRiddenAndFired` | "RIDE THE WALL. AIM: TIME SLOWS." | cyan lane strip, floor turns amber on the cycle |
| f6 REST | 5 x 8 x 4 | none | empty lit corridor, a health pickup | 0 | none | none | none | warm white pool |
| f7 SLIDE | 8 x 18 x 4 | slide | a 1.2 m tunnel in the middle (validator tunnel row) | 0, then 2 swarmers beyond that cannot be shot through it | none | stopped by the tunnel | "SLIDE UNDER" | green trim |
| f8 PISTOL | 14 x 20 x 7 | shoot, charge | an example husk first, then 3; a ranged spitter on a far shelf out of melee reach | 1 + 3 husk, 1 spitter | pistol; charge shot for the spitter | death restarts the room | "SHOOT. HOLD RT TO CHARGE." | amber accent at the shelf |
| f9 PULSE | 16 x 22 x 8 | the gauntlet and its LT blast | five kills fill the pips, a 7 m gap the blast carries you over to a ledge with the exit | 8 husks (2 waves of 4) | gauntlet (punch kills fill the glove) | as f8 | "FIVE KILLS. THEN LT LAUNCHES." | magenta-free; cyan glove glow |
| f10 FIGHT | 18 x 26 x 9 | everything, with a ring route | 12 husk + 4 swarmer + 2 spitter (cost 18) in 3 waves; two wall lanes and a low platform; a hazard strip along the middle | per the mix | pistol and fist | death restarts the room | "USE EVERYTHING." | two cyan strips, red-orange only on the hazard |
| f11 REST | 5 x 10 x 4 | none | a lit hallway and a checkpoint; the Lantern's glow shows through the far door (foreshadow) | 0 | none | none | none | orange glow leaks under the door |
| f12 THE LANTERN | 20 x 20 x 14 | wall-ride, aim in the slow-mo | see boss card C1 | boss + orbs | pistol (light damage), gauntlet in the window | death restarts the boss room | boss prompts | dusk, orange core |
Par for each room is set from the bot's median over 10 seeds (written by `levelgen` / `campaign`).

## C. Boss cards
**C1 THE LANTERN (plan id `hulk`, class Lantern27 untouched; the new behaviour is `Lantern29`, a wrapper on the Filter and the cycle)**
Diagnosis (NIGHT29-LOG 0:20): `Lantern27.Filter` returns 0 unless the shot hits the face that looks at the ridden wall. New rule (`design.n29.boss1`): any gun anywhere does `lightDamage` (0.20) of a normal hit (the bar always moves); full damage only while the core is exposed (the floor is live) AND the shot is fired while WALL RIDING (a platform does not count: review fix); a player on the floor or a platform gets light damage only. The attack: FLOOR IS LAVA, a cycle of `cycleSeconds` 11: 0.9 s tell (the arena glow shifts red-orange, the core opens; no sound), 7 s live floor (the hazardFloor27 strip over the whole floor, 8 damage per second so the 7 s phase costs at most 56 of 100 HP), 3.1 s rest. While live the world slows to 0.55 with a 0.2 s ease (D-7). Between cycles orbs at the wall rider at the old speed (the slow-mo applies only while the floor is live, so the rider is pressured, not rested). Nothing kills in under 0.5 s. Proof: harness `boss27 lantern` shows a wall-riding lag bot kills inside 45-70 s (D-5) and a floor-standing bot takes at least 3x longer (lightDamage 0.20 gives 5x on the damage alone; the review caught the 2.5x / 5x mismatch).
**C2 THE GLACIER (plan id `glacier`, new class `Glacier29`; Glacier28 stays as the off path)**
Diagnosis: Glacier28.Filter lets only a PULSE hit land inside a window (and never through the glaze); the pistol cracks glaze and does nothing. New rule (`design.n29.boss2`): any other gun does `otherGunFraction` 0.12 of the gauntlet hit at any time BUT cannot take the bar below `otherGunFloor` 0.25 (the last quarter needs the exposed window, so a camper never finishes it); the catch-up window (within 10 m on the same lane, side by side) cracks the glaze and opens an EXPOSED window of 2.5 s for full damage. The arena (card G1) is five parallel skate lanes with banks and a central island. The boss skates the lanes and flees; the player skates to close the gap; both try not to be caught: the Glacier lays frost trails (a slow field, -35 % speed) and fires ice shard volleys (a dash sideways dodges); a charge along a lane wants a jump or a bank. V2's patience gauge: 12 s at a gap above 25 m makes the volleys 1.5x faster until the gap closes below 15 m. No attack from behind without a 0.8 s tell and a visible arc; nothing kills in under 0.5 s. The HUD shows the gap as a bar. Proof: harness `boss27 glacier`: a catching-up lag bot kills in 80-120 s and a staying-away bot has the bar above 70 % at 3 minutes.
**C3 THE WORM (level 3, existing class; its arena rebuilt)**: the HP cut from item 7 only; the arena is a closed ring (card K4).

## D. Level 2: THE ICE LINE (plan id `n29/iceline`; pistol, fist, gauntlet; no award)
| id | footprint | verb | forcing device | enemies | weapon and why | text |
| --- | --- | --- | --- | --- | --- | --- |
| i1 TANK CORRIDOR | 8 x 24 x 6 | the gauntlet | a 24 m corridor, a coloured TANK (a recoloured husk box, hp 600) blocks it behind 6 light husks; the pistol does x0.15 to it, the gauntlet one hit | 6 husk + 1 tank | gauntlet for the tank, pistol for the husks | "THE TANK DIES TO ONE PUNCH" |
| i2 PULSE ROOM | 16 x 22 x 8 | the LT blast | a 7 m gap, the glove's pips filled by a wave of 5 husks | 5 husk | gauntlet | "FIVE KILLS. LT LAUNCHES." |
| i3 REST | 5 x 8 x 4 | none | a lit hall; the first ice sheen on the floor | 0 | none | none |
| i4 FIRST LANE | 8 x 26 x 6 | the skate lane | one ice lane, a hazard strip both sides, the lane is the only way; the lesson is the slide into a lip | 0 | none | "HOLD FORWARD. DASH SIDE TO SWITCH." |
| i5 TWO LANES | 14 x 26 x 7 | lane switch + dash | two lanes, a dash sideways between them, a husk wave on the far bank | 6 husk | pistol | "DASH SIDE TO SWITCH LANES" |
| i6 ICE FIGHT | 18 x 28 x 9 | all | mixed ice floor, two tanks and 10 husks in 3 waves, a bank on each side | 2 tank + 10 husk | gauntlet + pistol | "TANKS FIRST. PUNCH THEM." |
| i7 REST | 5 x 10 x 4 | none | a lit hallway, the Glacier's frost shows ahead | 0 | none | none |
| g1 THE RINK | 28 x 52 x 12 | skating, catching up | card C2; walls dark dusk-blue (not pale), no white roof; five lanes + an island with cover | boss | gauntlet (full), pistol (12 %) | gap bar |

## E. Level 3: THE KILN (plan id `n29/kiln`; THUNDERCLAP awarded in k1)
| id | footprint | verb | forcing device | enemies | weapon and why | text |
| --- | --- | --- | --- | --- | --- | --- |
| k1 LOADING BAY | 18 x 22 x 8 | the shotgun | THUNDERCLAP pickup in view on entry; a ring route round a central press; a cluster of 8 husks that the pellets clear and the pistol cannot in time | 8 husk + 4 swarmer | shotgun (splash) | "GRAB THE THUNDERCLAP" |
| k2 CASTING FLOOR | 18 x 28 x 9 | pit and bridge | a pit with a 2 m bridge (hazard below), a far shelf with 2 spitters out of the shotgun's reach | 10 husk + 2 spitter | pistol / charge for the shelf, shotgun on the bridge | "SHELF NEEDS THE PISTOL" |
| k3 REST | 5 x 8 x 4 | none | a lit hall, heat shimmer | 0 | none | none |
| k4 FLUE | 12 x 12 x 16 | the wall lanes up | a vertical shaft with two wall lanes; enemies on gantries every 5 m; floor = hazard | 8 husk + 1 heavy | shotgun on the gantry, wall-ride up | "RIDE UP. SHOTGUN CLOSE." |
| k5 THE RING | 26 x 26 x 12 round | the worm | the worm's closed ring arena | worm | all | worm prompts |

## F. The unlock table (D-2)
Level 1 none, level 2 none, THUNDERCLAP at k1 (a pickup in view, never from a LIMIT room), NAILGUN 4, TORCH 5, ZAPPER 6, RAIL 7, BOUNCER 8, ROCKET 9 (levels 4-10 untouched this night; `unlocks.json` rows kept, only level 1-2 awards removed). A tank row: `design.n26.vulnerability.tank`: gauntlet x10, pistol x0.15, others x0.4, with a `_why`.

## G. Item cards for the systems
- **Lockout (item 8):** `design.n29.lockout`: a melee press is ignored while a gun's cycle (its fire interval, the shotgun's pump) runs; the trigger is ignored while a melee swing runs; the gauntlet is the melee (no separate melee); a queued weapon switch waits. The second input is DROPPED. Cycle read from each weapon's own interval.
- **Shotgun (item 10):** `design.n29.shotgun.fireIntervalMult` x1.3 after one harness read; `_why`.
- **DOORFREE (item 14):** hold RB 3.0 s (F8 3.0 s): doors open, waves still spawn on entry, a boss room can be entered at once, "DOORS OPEN" corner tag, run UNRANKED and writes no record.
- **CRT (item 4):** E in R4b: warp 2 %, scanlines 0.12 at 720, chroma 0.0015, grain 0.045 at 24 Hz, vignette 0.25, pixel step off; one setting 0-100 % default 35; an accessibility off switch; applied before the UI; `-skygrindOff29` off.
- **Skate lane (item 13a):** `Core/SkateLane29.cs`, applied after the Mover each step on ice: when grounded on an ice lane above `snapSpeed` the lateral position eases to the lane line (the wall ride's snap look), lateral velocity is zeroed, a ground dash sideways moves the line to the adjacent lane over `switchSeconds`, speed bleeds slowly; off the lane the existing x0.25 friction stays. Mover untouched.

## H. Builders (file ownership) and their first three tasks
- **R rooms and levels:** `Design/levels/n29/*`, `LevelPlan29` / `LevelValidate29` (rows `lights`, `props`, `doorFit`, `closedRoom`), pilots for new stages. First three: (1) the room standard + `lights` / `props` / `doorFit` / closed-room rows in the validator; (2) `n29/foundry` f1-f12 plan; (3) `n29/iceline` i1-i7.
- **B bosses:** `Core/Enemies/{Lantern29,Glacier29}.cs`, `Core/SkateLane29.cs`, `design.n29.boss1/boss2/bossHp`. First three: (1) Lantern light damage + floor cycle; (2) Glacier other-gun fraction + the lane rule; (3) the Glacier's attacks and patience gauge + HP cuts.
- **V view:** `Game/Crt29*.cs`, the Renderer Feature, `UnityTuning.Crt29.cs`, `LightKit29`, door frames, roof closure, settings rows, the rink and foundry looks. First three: (1) the CRT pass and its setting; (2) light sockets and door frames in the views; (3) the HUD gap bar and the DOORFREE tag.
- **S systems:** `Core/Guns/Lockout29.cs`, the shotgun row, `unlocks.json`, the tank row, DOORFREE, `Lesson25.wallRiddenAndFired`. First three: (1) the lockout; (2) the shotgun and the tank; (3) DOORFREE.
- **Lead alone:** the shared commit (`N29Blocks`, tape format 12, the rows off), `SurvivalSim*`, `SurvivalGame*`, the harness `Program.cs`, builds, reels, integration, the level-select fix (done: `N29Fixes`).

## I. Reviewer's questions (answered here before the review)
- Which card does not force its mechanic? f1 / f6 / f11 / i3 / i7 / k3 are rest rooms on purpose; f7 forces slide only if the swarmers stand beyond the tunnel; i1 forces the gauntlet only because the pistol's x0.15 makes the tank take 7x longer.
- Which numbers are guesses? the lantern cycle (11 s, 7 s live, 12 damage per second), the Glacier's frost trail (-35 %), the patience gauge (12 s, 25 m), the tank's 600 HP, the CRT defaults (R4b picks), all par times (set from the bot).
- What would let a player stand still and win? The Lantern: standing on the floor does light damage only (2.5x longer, the cycle hurts); the Glacier: staying away enrages it and its bar stays above 70 %.
