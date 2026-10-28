# A climber for SKYGRIND: a proposal for GG (NIGHT20, builder N; nothing built)

CLAUDE.md rule 7 makes enemy design GG's. This page proposes a new kind. Nothing in it is in the code. Tonight's work
gives the existing kinds new verbs behind `design.routes`, with no new kind:
- the husks and spitters climb up to 5 m and leap up to 8 m;
- the swarmers crawl up to 10 m;
- every walker drops up to 12 m;
- authored ladders and leaps reach the tall perches.

## Why ask

THE CLIMB (the lead's judge, Survival, `design.routes` on) measured these rows tonight:
- **R1:** 0 of 830 player nodes unreachable (477 with the routes off).
- **R2:** the worst horde time is 15.9 s; 52 nodes are more than 6 s slower than the floor under them.
- **R3** (the STAND bot at round 10): on most of the 10 worst perches the bot still lives 2-30 x longer than on the
  ground (9.3 s). A perch still buys safety.
- **The routes are long.** Ledge chains, ladders and long leaps are single file, so the horde arrives one at a time.
  The CHASE's STAND numbers say a perch still beats the plaza.

Shipped games answer this with a kind built for height, not with more ladders:
- **Dying Light:** Virals "are really fast and they can climb, and they show up typically when you make noise" (the
  Techland interview in `Research/2026-10-06-enemy-routes.md` 2.3). Biters can't climb, so the rooftops are safe
  from them only.
- **Left 4 Dead:** the commons climb about 5 m (`ClimbUpHeight`). Smokers "select areas above the Survivor team".
- **Deep Rock Galactic:** the Glyphids walk on walls.

## The proposal: THE SCALER (a working name; GG names it)

| | |
| --- | --- |
| Role | Answers a perch, never the ground: it spawns only while you are up (the camp score's height reading) and goes straight up the nearest face. |
| Verb | Climbs any vertical face at 4 m/s with no height cap: it uses the tall-climb links (CLIMB HIGH) and authored ladders, and its own face-crawl where the grid has no link. It grabs the lip and pulls itself over in 0.4 s. |
| Threat | It grabs the player at the lip: a 0.6 s DANGER tell (it screeches, its head pops over the edge), then a pull that drags you off the perch (an impulse toward the drop: the punish is the fall, not damage). |
| Counter | Any hit knocks it off the face (the climb's knock-off, built tonight). A punch at the lip is a PARRY: it falls and stands for dropStagger where it lands. |
| Body | Lean and low, four limbs, the head forward. The swarmer's blocky style at husk size, in the DANGER role (red-orange); its tell is the climb's claws and lip bar (built tonight). |
| Numbers | Proposed: HP 30 (two KESTREL body shots), 1 per perch per 20 s, at most 2 alive, from round 6. Its HP and spawn rate go in a new `design.enemies.scaler` block (rule 5). |
| Events | For GG's sounds: `scalerSpawn`, `scalerGrab`, `scalerPull`, `scalerKnocked`, plus the climb's own `climb` phases. |
| Judge | THE CLIMB's R3 with the SCALER on: STAND's survival on the 10 worst perches <= 2 x the ground's. |

## Smaller alternatives (no new kind)

1. **THE PILE** (World War Z): husks stack at a wall's foot until the top one reaches the lip. Proven in WWZ, but the
   pile needs crowd physics that don't exist here, and it is a new visual system. Cost: large.
2. **A faster horde on the routes:** ladders at 8 m/s, a 0.2 s tell. A number change in `design.routes`; it shortens
   R2 but keeps the single file.
3. **The LANCER and the DRONE** (the web's own height answers, ported by builder E): they hit a perch from range and
   leave it reachable on foot. They are already GG's designs, and DRYDOCK's 30 m crow's nest needs them.

## What GG decides

- A new kind (the SCALER), alternative 1 or 2, or only the web's LANCER and DRONE.
- If the SCALER: its name, its look, and whether its pull is allowed (it moves the player).
