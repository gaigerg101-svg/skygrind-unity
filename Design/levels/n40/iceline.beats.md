# LEVEL 2 THE ICE LINE, n40: the beat sheet (NIGHT40 builder L2, card L2; written before any geometry)

GG, 2026-10-13 ~22:00 (Research/2026-10-14-wave40/GG-REQUEST.md, asks 13 and 14): "Nearly all rooms in the second level need a complete
overhaul to better support the new gauntlet mechanic ... the level needs more tutorial-style buildup that teaches the gauntlet's strengths,
limitations, and optimal use cases; players should not have access to the pistol at the start of level two ... The rooms should be rebuilt
to emphasize how the gauntlet excels against large enemies, how its charge attack can be saved for groups of smaller enemies, and how
players must manage its cooldown strategically."

Generator: `node Tools/night/gen-n40-iceline.cjs` (n36-lib; no older generator required, no old level file edited) writes
`Design/levels/n40/iceline.plan.json`; `bash Tools/night/n40l2-regen.sh` runs the jobs checker, levelgen, and writes
`Design/levels/n40-iceline.json` + its StreamingAssets copy (cmp). Id `n40-iceline`; the campaign plays it behind `design.n40.iceline40`
(levelFrom `n37-iceline` -> levelTo `n40-iceline`, Curriculum25.N40Levels). `--off40` never sees this file.

## Rules this sheet is held to
- One job per room, one lesson per room, a line you can draw (the n36 method). Every solid carries `job` + `why` (n36-jobs GREEN).
- The gauntlet's curriculum in the card's order: teach -> practice -> twist -> test. Small rooms (20-30 m; GG 2026-10-09 "a lot smaller
  and more action packed"), each with a MOVEMENT LINE (5 m ride panels, a bridge, a lane) and a CLIMB ROUTE for enemies to any raised spot
  (PRINCIPLES 4; GG NIGHT20 "There always really needs to be a route that enemies can climb up").
- Weapons by data (design.n40.iceline40): `startSet: ["gauntlet"]` (no KESTREL at the start, GG ask 14); `awards: [{room: "g6", weapon:
  "revolver"}]`: the KESTREL returns on entering 2.6 THE ARMOURY. The STITCHER and the THUNDERCLAP are not level 2's.
- The gauntlet's facts the rooms are built on (design.n27.pulse27, read): a heavy swing every 2.5 s (192 damage); 5 kills (any weapon)
  fill 5 pips; a full glove enables the {altFire} BLAST (6 m along the aim, 1.5 m radius, x2 damage) with a 15 s cooldown; the tank takes
  the swing x10 (design.n26.vulnerability.tank: one punch kills a 600 hp tank), the pistol x0.15. The WARDEN's shield turns a frontal hit,
  a punch stuns it 1 s (Warden.cs). A punch reflects a DRONE orb (Drone.cs). A CHARGER that hits a wall is stunned 1.5 s, its back x3.
- Enemies placed by the campaign ids `warden`, `drone`, `charger` (E3 retunes them behind foes40; until then they are the ported survival
  kinds: Husks.Spawn takes all three, caps charger 6 / drone 20 / warden 6, PortExport/config.json).
- Ride panels are 5 m tall (R2's ride40 attach asks a wall >= ~2 m: every panel here qualifies; no riser or deck face is meant as one).
- Text: at most 6 words, placeholders only (Lesson25TextTests).

Coordinates: room-local, `lx` metres east of the room's centre line, `lz` metres north of its south edge (n36-lib). `@y` = height.

| # | id | room | size (w x l x h) | job | the gauntlet lesson | enemies (waves) | time |
|---|---|---|---|---|---|---|---|
| 1 | g1 | 2.1 TANK ALONE | 22 x 26 x 9 | teach | the punch kills what a gun barely scratches | tank | 10-15 s |
| 2 | g2 | 2.2 THE SHIELD | 24 x 28 x 9 | teach | punch the shield (1 s stun) or get round it | warden; warden + 2 husks | 25-35 s |
| 3 | g3 | 2.3 THE SWARM | 26 x 28 x 9 | teach | five kills charge the glove; the blast is for the pack | 3 husks; 8 swarmers | 20-30 s |
| 4 | g4 | 2.4 THE REACH | 24 x 30 x 10 | twist | the glove cannot reach a flyer: punch its orb back | drone; 2 drones | 25-35 s |
| 5 | g5 | 2.5 THE COOLDOWN | 26 x 30 x 9 | twist | a blast spent early is not back for the second pack (15 s) | 5 swarmers; 6 swarmers + 2 husks | 25-35 s |
| 6 | g6 | 2.6 THE ARMOURY | 14 x 18 x 6 | reward | the KESTREL returns (awards); two husks to try it | 2 husks | 5-10 s |
| 7 | g7 | 2.7 THE MIX | 30 x 32 x 10 | combine | switch: the pistol far, the punch close | 3 waves, every kind so far | 40-60 s |
| 8 | g8 | 2.8 THE REHEARSAL | 24 x 30 x 9 | rehearse | dodge the charge into a pillar, punch its back (the boss's haymaker -> stuck rhythm) | charger; charger + 2 husks | 25-35 s |
| 9 | gr | 2.9 REST | 10 x 10 x 5 | rest | a breath, health | none | 5 s |
| 10 | x9 | THE BRAWL | 38 x 38 x 14 | boss | B2's brawler (THE ICEBREAKER, working name) | B2's | B2's |

**B2's arena:** room id `x9`, name "THE BRAWL", `kind: "boss"`, `boss: "brawl40"`, the row's `brawl40.centre` the arena floor's centre (world).
Round 2, B2's contract (Research/2026-10-14-wave40/B2.md): the sim builds a 36 x 36 floor with 1 m rim walls just outside +-18 m and the
entry gap x -2..2 on its +z wall, so nothing of this room may be solid inside the 38 x 38 m footprint: the room is 38 x 38 (its own walls
start outside it), the centre 19 m in from the door (the door at centre.z + 19 meets the gap's wall at +18..+19; both 4 m wide), the spawn
at centre.z + 16, yaw 0. This file reserves the empty room (a light, a cyan pool at the centre); B2's arena is everything else.

## 2.1 TANK ALONE (teach: the punch kills what guns barely scratch)
- **Line:** spawn (0, 2) > the TANK switches on alone in the middle (0, 14), slow, walking at you > punch it ({punch}) when it is close >
  up the ramp (lx 0, lz 16 -> 21) onto the north deck (1.5 m) > the exit (0, 24.5).
- **Space:** 22 m wide; ride panels W (face east, lx -8) and E (face west, lx 8), lz 5..18, 5 m tall: room to circle a slow body, and the
  line round it if you misjudge the swing.
- **Enemies:** one tank at (0, 14). Nothing else: the lesson cannot be missed.
- **On entry:** the tank's spot under an amber light; the deck and its ramp behind it; the exit's cyan pool on the deck.
- **Text:** "{fire}: ONE SWING FELLS A TANK" (husksDead 1). Round 3 (the lead's n40-r3 frame): it read "BIG ONES FALL TO F", and F (the punch) does
  nothing with the gauntlet in hand: its swing is {fire}. Every line below now names the button that does the room's job.

## 2.2 THE SHIELD (teach: the WARDEN; punch the shield or flank)
- **Line:** spawn (0, 2) > the WARDEN switches on at (0, 16) facing you, its shield up > punch the shield (a 1 s stun) and punch again, or
  ride a side panel (lx -9 / 9, lz 6..22) past it to its back > wave 2 (a warden + two husks) comes off the north deck's ramps > the exit
  (0, 26) on the deck (1.5 m).
- **Space:** 24 m wide; two cover pillars (2 x 2 x 4 m) at (-4, 10) and (4, 10): step round one to reach its flank.
- **Enemies:** wave 1: one warden (0, 16). Wave 2: a warden at (0, 20) and two husks on the deck (raised nodes at lz 25.5, down its ramps
  at lx -6 / 6: the climb route).
- **Text:** "{fire} THE SHIELD, OR FLANK IT" (husksDead 1; with foes40 the gauntlet's swing opens the shield, Warden.cs meleeShield40).

## 2.3 THE SWARM (teach: five kills charge the glove; save the blast for the pack)
- **Line:** spawn (0, 2) > three husks walk at you from the middle (lz 12-14): punch them (with the earlier rooms' kills, the glove is full)
  > the swarmers' PEN at the north (lz 20..28) opens through a 6 m GAP between two 3 m walls (lx -13..-3 and 3..13, lz 18..20): eight
  swarmers pour through it in a line > the {altFire} BLAST down the gap takes the pack > the exit (0, 26) through the pen.
- **Space:** 26 m wide; ride panels on the room's sides (lx -11 / 11, lz 4..16) for the dodge round the husks.
- **Enemies:** wave 1: 3 husks at (-3, 12), (0, 14), (3, 12). Wave 2: 8 swarmers at the pen's nodes (lz 22-26).
- **Text:** "FIVE KILLS: {altFire} BLASTS THE PACK" (husksDead 3).

## 2.4 THE REACH (twist: the glove cannot reach a DRONE across the canal; the parry sends its orb back)
- **Line:** spawn (0, 2) > the CANAL across the room (lz 12..18, a dry channel 1 m deep, its floor ICE: the level's name) > the DRONE
  hovers over the far side (lz 22-26, 4-8 m up, keeping 12-18 m): its YELLOW orb comes at you > swing ({fire}) as it arrives: it goes
  back and kills its drone (foes40, E3) > cross the
  canal on the BRIDGE (lx -2..2) > the exit (0, 28).
- **Falls:** a jump clears the 1 m lip anywhere; a 0.5 m step at each end (lx -10.5 and 10.5) for walkers: a fall costs a jump, never health.
- **Enemies:** wave 1: one drone at (0, 24). Wave 2: two drones at (-6, 25) and (6, 25).
- **Text:** "{fire} AS THE YELLOW ORB ARRIVES" (husksDead 1).

## 2.5 THE COOLDOWN (twist: a blast spent early is not back in time)
- **Line:** spawn (0, 2) > a tight clump of five swarmers at (0, 14): the bait > wave 2 comes AT ONCE off the two side decks (lx -13..-8
  and 8..13, lz 20..30, 1.5 m; ramps down at lz 15..20): six swarmers and two husks > a glove spent on the five is 15 s from ready: punch
  the first five, blast the second pack > the exit (0, 28).
- **Space:** 26 m wide; two ride panels (lx -9 face east, lx 9 face west, lz 4..13): the line from the door to each deck's ramp. (Round 3:
  moved from the middle, lx -+5, whose 5 m walls made a corridor that hid the decks from the door in the n40-r2 frame.)
- **Text:** "SAVE {altFire} FOR THE SECOND PACK" (husksDead 5).

## 2.6 THE ARMOURY (reward: the KESTREL returns)
- **Line:** spawn (0, 1.5) > on entry the KESTREL is given (design.n40.iceline40.awards [{room: "g6", weapon: "revolver"}]); its plinth
  and pickup at (0, 5) under a cyan light > two husks at (-3, 14.5) and (3, 14.5), 13 m off: shoot them ({fire}) > the exit (0, 16.5).
  (Changed while building: two stand-ins and a targetsHit lesson failed levelgen, "the room has no enemies"; the room is 14 x 18.)
- **Text:** "KESTREL BACK: {fire} THE FAR ONES" (husksDead 2; the n40-r2 frame shows the KESTREL in hand on entry).

## 2.7 THE MIX (combine: switching wins)
- **Line:** spawn (0, 2) > wave 1: a tank in the middle (0, 14) and two spitters on the NW perch (lx -15..-9, lz 24..32, 2 m): punch the
  tank, pistol the spitters > wave 2: a warden and two drones (the shield up close, the flyers far) > wave 3: a tank and six swarmers (the
  punch for the big one, the blast for the pack) > the exit (0, 30). (Changed while building: the budget allows at most 2 kinds a wave, 3
  with a tank, design.n27.budget.)
- **Space:** 30 m wide; perches NW and NE (2 m) with ramps (lz 18 -> 24 at lx -12 / 12: the climb route); ride panels by the side walls
  (lx -13 face east, lx 13 face west, lz 4..17): ride north to a perch's ramp. (Round 3: moved from the middle, lx -+5, whose walls hid the
  perches and their spitters from the door in the n40-r2 frame; the middle is open.)
- **Text:** "{quickSwap}: KESTREL FAR, GLOVE CLOSE" (husksDead 3).

## 2.8 THE REHEARSAL (rehearse: the CHARGER, the boss's rhythm)
- **Line:** spawn (0, 2) > the CHARGER switches on at (0, 22): its 0.8 s tell, then a 22 m/s charge > step aside so it hits a PILLAR
  (four 2.5 x 2.5 x 5 m pillars at (+-5, 10) and (+-5, 18)) > it is stunned 1.5 s, its back x3: punch it > wave 2: a charger and two
  husks > the exit (0, 28). The haymaker -> stuck -> punish rhythm of THE BRAWL, in miniature.
- **Text:** "{dash} ASIDE, THEN {fire} ITS BACK" (husksDead 1).

## Round 3, measured
- `n40ice prompts`: all 8 lines PASS (each placeholder bound on the v5 pad and the keyboard, at most 6 words, at most 45 characters on the
  keyboard: 27-39).
- `n40ice walk --seeds 10` (iceline40 + foes40): 10/10 to x9's door; medians g1 3.3, g2 34.3, g3 153.9, g4 13.8, g5 207.5, g6 3.5, g7 37.7,
  g8 20.8, gr 0.6 s; 0 deaths.

## 2.9 REST
- A breath before THE BRAWL. No enemy; a health pickup in the middle; a cyan pool on the door.

## THE BRAWL (x9; B2's)
- The empty 38 x 38 room (above). B2: the boss, its arena (rim ride walls, pillars), its pilots.

## Round 2, measured (`n40ice walk --seeds 10`, iceline40 + foes40 on, brawl40 off: the walk ends at x9's door)
- 10/10 runs clear g1..gr. Median clear (p90): g1 3.3 s (3.3), g2 121.2 (244.6; 3 deaths in 10), g3 171.0 (270.6), g4 12.8 (15.1), g5 171.6
  (321.9), g6 3.2 (7.3), g7 56.9 (86.9), g8 17.3 (21.3), gr 0.6. The slow rooms are the bot's gauntlet against shields and swarms (a 2.5 s
  swing); a person's times are not measured.
- **g4 needs foes40:** with foes40 off a gauntlet-only player cannot parry (E3, 6760c918), and the walk dies 51 times in 3 seeds without
  clearing g4 (0/3). iceline40 must ship with foes40 on.
- The bot's lesson scripts (Walk40Iceline.cs): the parry (look at an orb inside 7 m, swing inside 1.6 m), the blast (3 enemies inside 5 m in
  g3 / g5 / g7: g5's median 340.5 s without it, 171.6 s with it), the canal (out north).
- Changed while building: THE CANAL is 1 m deep (was 2 m: the bot wall-rode its sides and never climbed out, 5/5 seeds); one 0.5 m step at
  each end.

## The checks
1. `node Tools/night/n36-jobs.cjs Design/levels/n40/iceline.plan.json`: GREEN.
2. `levelgen`: 0 errors (the walk proofs, the routes, the doors, the lights).
3. No traversal gate in rooms 1-8 (no `gate` tag: every crossing is a walk, a bridge or a ramp), so `n38 gates` has nothing to judge here.
4. `n40ice walk --seeds 10` (Tools/SimHarness/Harness.N40.Iceline.cs): the walk bot (Walk29Pilot + Walk40Iceline) through g1..gr, iceline40
   on (the gauntlet alone at the start), per room cleared k / n; the owned weapons printed at the start and after g6.
