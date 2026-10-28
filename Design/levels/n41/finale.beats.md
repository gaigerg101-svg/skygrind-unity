# LEVEL 6 THE MIRROR, n41: the beat sheet (NIGHT41 builder L6, card L6; written before any geometry)

GG, 2026-10-13 (Research/2026-10-14-wave40/GG-REQUEST.md, ask 21): "the current sixth one should be removed and rebuilt from scratch later
because it looks awful". GG, 2026-10-09: "I'd like a lot of the rooms to be a lot smaller and more action packed like think about the design
of Ultra Kill". GG, 2026-10-12: "random lava pits random pillars that serve no purpose". GG, NIGHT20: "There always really needs to be a route
that enemies can climb up". GG, 2026-10-13 ~00:10: "you should be flying through and ... killing the enemies before you enter the next room in a
stylish fashionable way". DECIDE37 D-1 (GG): level 6's finale is **THE RIVAL**, "a magenta V2-style mirror that rides, grapples and parries
like you".

**This is a plan for GG to react to (DECIDE41 D-3).** The greybox sits behind `design.n41.finale41`, OFF; level 6's slot stays locked. Nothing
of n35-finale or n38-finale survives (no room, prop or generator: "rebuilt from scratch"). THE RIVAL is a one-page proposal
(Research/2026-10-14-wave41/RIVAL.md): no code; its arena below is an empty, reserved room.

Generator: `node Tools/night/gen-n41-finale.cjs` (requires only `n36-lib.cjs`) writes `Design/levels/n41/finale.plan.json`, then levelgen
writes `Design/levels/n41-finale.json` + its `Assets/StreamingAssets` copy (cmp), replacing the placeholder (today a byte copy of n35-finale
with the name changed). Id `n41-finale`; finale41 on puts it back as the campaign's last level (Curriculum25 N41Levels). `--off41` never sees it.

## What level 6 is: the exam
PRINCIPLES 4: one new idea per level, and level 6's idea is **none new: every verb of levels 1-5, two or three at once**. Research
(Research/2026-10-11-wave35/research/teach.md rule 9): "level 6 is the exam mixing every verb". So no room teaches; every room is a *combine*
room whose layout makes a pair or a trio of old verbs the fast answer, and the last room before THE RIVAL asks for all of them.

The verbs it examines (docs/VERBS.md rows; the level that taught them):

| level | taught there (VERBS rows) | where level 6 asks for it |
|---|---|---|
| 1 FOUNDRY | KESTREL tap / zoom / charge (25-27), slide (6), dash (5), slam (7), parry (40), launch (41), heal (42), the climb (44) | 6.1, 6.2, 6.3, 6.5, 6.7 |
| 2 ICE LINE | PULSE gauntlet swing / blast (28), the wall ride + jump-off (9-11), the gauntlet on big ones | 6.1, 6.2, 6.6, 6.7 |
| 3 KILN | THUNDERCLAP slug + the mid-air boost (29-30) | 6.3, 6.4, 6.7 |
| 4 SPAN | grapple pull (17-18), the whip (20-21), the bend (22), the swap inside a bend (23) | 6.4, 6.5, 6.6, 6.7 |
| 5 SPIRE | the jump dash (8), the ring pass's line (47: a dash-jump through a hole) | 6.5, 6.6, 6.7 |

Weapons at the start: everything the earlier levels award (the KESTREL, the PULSE gauntlet, the THUNDERCLAP; the fist always; the grapple).
No `startSet`, no `awards`: an exam gives nothing. The NAILGUN is out of the campaign (loom40). The TORCH: VERBS row 33 says "earned at level 5,
owned after 5"; whether the unlock rows give it on level 6 is **not checked**, and no room needs it.

## Rules this sheet is held to
- **Small and action-packed** (GG 2026-10-09): rooms 20-30 m across (the last 30 x 32), roofs 9-12 m; the exit seen from the door; at most
  three floor planes (arenas.md rule 2: "jungle gyms, not bowls").
- **One job per room, one exam per room, a line you can draw** (the n36 method). Every solid carries `job` + `why` (n36-jobs GREEN); a pillar
  exists only if it carries a node, a ride face or cover the room's line uses (GG's "pillars that serve no purpose").
- **Lava only where an enemy can be launched into it** (n38's rule): one channel in the whole level (6.5), and its edge is where the husks walk.
- **A climb route for enemies in every room** (PRINCIPLES 4): every raised floor an enemy stands on has a ramp or stairs to it, named per room.
- **The fast line and the walker's line** (arenas.md rule 7: "the obvious route works but scores badly"): every crossing a verb shortens has a
  walk (a ramp, a bridge) that the walk bot and a lost player can take. No traversal gate (`gate` tag) in this level: the exam is combat with
  verbs, not jumping puzzles; `n38 gates` has nothing to judge.
- **Enemies wake ahead on the line** (GG's "killing the enemies before you enter the next room"): each room's wave 1 stands where the line
  points (arenas.md rule 9), and wakes on start40's rule (movement or a timer, then a tell).
- **The budget:** at most 2 enemy kinds a wave, 3 with a tank (design.n27.budget). Kinds: husk, swarmer, spitter, tank, warden, drone, charger.
- **Numbers the rooms are spaced from** (n36-lib MOVE, read from PortExport/config.json and Design/levels/n35-ride-distances.json): jump 1.5 m
  + double 0.9 x -> about 2.7 m; dash 24 m/s x 0.25 s = 6 m; slide collider 1 m; walk 8 m/s; ride 10.45 m/s; jump-off gaps 3-12 m. The
  THUNDERCLAP's mid-air boost adds about 2 m over the double jump (n38 THE STEP's sheet), so a 3.6 m deck is a boost; the jump dash lands
  6.75 m past a near edge (spire38's check). Ride panels are 5 m tall (ride40's attach asks >= ~2 m).
- **Colour:** cyan only on what the player uses (ride faces' trim, nodes, the exit pool); red-orange only on enemies and the lava; magenta
  nowhere in rooms 6.1-6.8 (THE RIVAL's only); pale violet-white the boundary. Room light warm dusk, never cyan or red-orange (D-4's rule).
- **Text:** one line per room naming the pair, at most 6 words, placeholders only ({fire}, {altFire}, {punch}, {dash}, {jump}, {grapple},
  {quickSwap}); none is a lesson (the exam reminds, it does not teach).

Coordinates: room-local, `lx` metres east of the room's centre line, `lz` metres north of its south edge (n36-lib). `@y` = height.

| # | id | room | size (w x l x h) | job | the exam (verbs that pay) | enemies (waves) | bot time |
|---|---|---|---|---|---|---|---|
| 1 | m1 | 6.1 RETURN | 22 x 24 x 9 | combine | parry + launch (L1) with the gauntlet (L2) | 2 spitters + 3 husks; drone + tank | 25-40 s |
| 2 | m2 | 6.2 THE GALLERY | 20 x 32 x 10 | combine | ride (L2) + zoom + charge's pierce (L1) | charger + 4 husks; 2 spitters + 4 husks | 30-45 s |
| 3 | m3 | 6.3 THE STEP | 22 x 26 x 10 | combine | THUNDERCLAP boost up (L3), slam down + blast (L1, L2) | 2 spitters + 6 swarmers; tank + 4 swarmers | 30-45 s |
| 4 | m4 | 6.4 THE HOOK | 24 x 28 x 12 | combine | whip + bend + swap to the slug (L4, L3) | 2 drones + 2 husks; warden + 2 drones | 30-45 s |
| 5 | m5 | 6.5 THE CHANNEL | 24 x 28 x 9 | combine | launch into the lava (L1); cross by jump dash, ride or hook (L5, L2, L4) | 6 husks; charger + 4 husks | 25-40 s |
| 6 | m6 | 6.6 THE WINDOW | 24 x 26 x 10 | combine | ride jump-off + jump dash through a hole to the wardens' backs (L2, L5) | 2 wardens + 2 spitters; 2 wardens + charger | 30-45 s |
| 7 | m7 | 6.7 EVERYTHING | 30 x 32 x 12 | test | all of it on three floors | 3 waves, every kind | 50-75 s |
| 8 | mr | 6.8 REST | 10 x 10 x 5 | rest | a breath, health | none | 5 s |
| 9 | mx | 6.9 THE MIRROR | 34 x 34 x 14 | boss | THE RIVAL (reserved: an empty arena, RIVAL.md) | none tonight | - |

The level: about 4-6 minutes of rooms for the bot, then THE RIVAL (GG's band: one to two minutes for a person). Every room is smaller than
n38-finale's arenas (36 x 40 and 40 x 40), and its three one-verb gate corridors (34-56 m long) are gone: here every room is a fight.

## 6.1 RETURN (combine: the parry and the launch, with the gauntlet)
- **Line:** spawn (0, 2) > two SPITTERS on the north perch (lx -7..7, lz 19..24, 3 m) lob YELLOW shots at the door > punch each shot back
  ({punch}, VERBS 40: freeze, it returns and bursts, health up, a dash back) > three husks walk at you down the perch's two ramps: dash-punch
  one into the others (VERBS 41: a launched husk that hits another bursts) > wave 2: a DRONE over the perch (5-7 m up) and a TANK at the
  perch's foot: return the drone's orb, swing the gauntlet at the tank (x10, VERBS 28) > up a ramp to the exit (0, 22.5) on the perch.
- **Space:** 22 m wide. Ride panels W and E (lx -10 face east, lx 10 face west, lz 4..16, 5 m) for the dodge while shots fly. No cover in
  the middle: the parry is the cover.
- **Climb route:** the perch's two ramps (lx -9 and 9, lz 13 -> 19, 3 m up): the husks come down them, the player goes up them.
- **Text:** "{punch} THE YELLOW BACK" (husksDead 1).

## 6.2 THE GALLERY (combine: the ride, the zoom and the charged pierce)
- **Line:** spawn (0, 2) > a CHARGER wakes at (0, 16) and four husks file down the far ramp > the floor is the charger's: jump onto the W ride
  panel (lx -9 face east, lz 4..28, 5 m) and ride north > zoom ({altFire}) while riding (slow39: ride + aim slows time to 0.30) and charge
  the KESTREL: the husks on the ramp stand in one line, the pierce takes them together > the charger hits the E wall's pillar (2.5 x 2.5 x 5 m
  at lx 6, lz 16, its back x3) > wave 2: two spitters on the north balcony (lx -8..8, lz 28..32, 4 m) and four husks on the ramp > ride off
  north onto the balcony (jump-off, VERBS 11) > the exit (0, 30) on the balcony.
- **Space:** 20 m wide, 32 m long: a gallery. The single pillar carries the charger's stop (cover + the stun wall); the E side has a second
  ride panel (lx 9 face west, lz 4..20) so the line works mirrored.
- **Climb route:** the balcony's ONE long ramp (lx 3..7, lz 18 -> 28, 4 m up) along the E wall: the file of husks the pierce is aimed at.
- **Text:** "RIDE, {altFire}, HOLD {fire}" (husksDead 1).

## 6.3 THE STEP (combine: the THUNDERCLAP's boost up, the slam and the blast down)
- **Line:** spawn (0, 2) > the room is split by THE STEP: a deck 3.6 m up across the north half (lx -11..11, lz 14..26), its south face
  `noride` (dull, no cyan) > two spitters on the deck's lip shoot down; six swarmers run at you on the floor > jump, double jump, fire the
  THUNDERCLAP down (the mid-air boost, VERBS 29) onto the deck > the spitters are within punch reach up there > wave 2: a TANK and four
  swarmers come up the deck's ramp at you: slam down off the deck into the swarmers (VERBS 7) and blast the pack ({altFire} with the
  gauntlet, five kills charge it), the tank to the gauntlet's swing > the exit (0, 24.5) on the deck.
- **The walker's line:** a ramp at the W wall (lx -10, lz 2 -> 14, 3.6 m up, 1:3.3) is the long way round (about 12 m more under fire): the
  walk bot's and the enemies' route. The boost saves the walk; the ramp keeps nothing locked.
- **Climb route:** the W ramp (above); the swarmers climb it to reach a player on the deck.
- **Text:** "{jump}, {jump}, {fire} DOWN: UP" (husksDead 1).

## 6.4 THE HOOK (combine: the whip, the bend, the swap to the slug)
- **Line:** spawn (0, 2) > two DRONES hang 8 m up over the middle (lx -5 / 5, lz 16) out of the gauntlet's reach > whip one ({grapple} at it,
  VERBS 20): the latch pulls it in and the bend starts (VERBS 22) > inside the bend swap to the THUNDERCLAP ({quickSwap}, 0 steps, VERBS 23)
  and slug it > husks come off the mezzanine > wave 2: a WARDEN on the floor (its shield frontal) and two drones: hook to a node (VERBS 17)
  over the warden's head, land behind it, gauntlet its back > the exit (0, 26) under the mezzanine.
- **Space:** 24 m wide, 12 m tall (the room's verticality is the drones'). Two grapple NODES on two posts (1 x 1 x 9 m at lx -6 and 6, lz 18):
  each post's job is its node (a line), nothing else. A mezzanine (lx -12..-4, lz 20..28, 3 m) on the W side.
- **Climb route:** the mezzanine's stairs (lx -4 -> 0, lz 22..26: four 0.75 m steps): the husks and the warden's second wave walk them down.
- **Text:** "WHIP IT, {quickSwap}, SLUG IT" (husksDead 1).

## 6.5 THE CHANNEL (combine: launch them into the lava; cross three ways)
- **Line:** spawn (0, 2) > THE CHANNEL: lava across the room (lz 12..18, 6 m wide; the level's ONLY lava) > six husks cross the BRIDGE
  (lx -2..2) at you > punch them while dashing or sliding (the launch, VERBS 41): a husk launched off the bridge goes into the lava > wave 2:
  a CHARGER on the far side charges across the bridge: step aside and it goes into the lava (or it stops at the bridge's posts) > cross:
  the jump dash over the lava (6 m wide < 6.75 m past the edge, VERBS 8), the ride panel along the E wall (lx 11 face west, lz 8..22, VERBS 9),
  the node over the middle (lx -6, 8 m up, lz 15, VERBS 17), or the bridge on foot > the exit (0, 26).
- **The walker's line:** the bridge. A fall into the lava costs health and a climb out at the channel's ends (0.5 m steps at lx -11 / 11:
  never a death from the ledge, the n40 canal's rule), per lava's own damage (design.lava: **not checked** which value the campaign uses).
- **Climb route:** the bridge, and two 0.5 m steps out of each channel end for anything that falls in.
- **Text:** "{punch} THEM INTO THE LAVA" (husksDead 2).

## 6.6 THE WINDOW (combine: the ride's jump-off and the jump dash through a hole, to the wardens' backs)
- **Line:** spawn (0, 2) > a 5 m WALL across the room (lx -12..12, lz 13..14) with a WINDOW in its middle (lx -2..2, 2.5..5 m up) and a
  4 m DOOR at its W end (lx -12..-8, on the floor) > two WARDENS face the door gap, shields up; two spitters on the far side's ledge >
  ride the S side panel (lx 10 face west, lz 2..11, 5 m) north, jump off with the look up (VERBS 11) and jump-dash through the window (the
  ring pass's line, VERBS 8 / 47) > you land BEHIND the wardens: gauntlet their backs > wave 2: two wardens and a charger come through the W
  door at you: back through the window, or hook the node above the window's far side (lx 0, lz 16, 7 m) > the exit (0, 24).
- **The walker's line:** the W door (4 m wide, on the floor): through the wardens' shields, the slow way.
- **Climb route:** the far ledge's ramp (lx 8..11, lz 16 -> 22, 2.5 m) for the spitters' replacements and for anything chasing the player.
- **Text:** "THROUGH THE WINDOW, THEN BEHIND" (husksDead 1).

## 6.7 EVERYTHING (test: three floors, every verb pays)
- **Line:** spawn (0, 2) > the floor (0 m), two side decks (W lx -15..-9 and E 9..15, lz 10..24, 2.5 m), and a north CATWALK (lx -15..15,
  lz 26..32, 5 m) > wave 1: four husks + two spitters on the decks (parry, launch, the pierce) > wave 2: a TANK and six swarmers (the gauntlet,
  the blast, the slam from a deck) > wave 3: two drones + a warden + a charger (the whip, the bend, the swap; the warden's back from a deck) >
  the exit (0, 30) on the catwalk: boost up from a deck (2.5 -> 5 m: one THUNDERCLAP boost), ride the N face of a deck pillar, or the ramps.
- **Space:** 30 x 32 x 12. Two ride panels in the middle (lx -4 face west and lx 4 face east, lz 8..20, 5 m, back to back: the middle's line
  from deck to deck), two nodes over the catwalk's ends (lx -12 / 12, lz 28, 9 m).
- **Climb route:** each deck's ramp (lx -12 / 12, lz 4 -> 10) and the catwalk's two stairs (lx -15..-12 and 12..15, lz 24 -> 26 from the decks).
- **Text:** none (the test).

## 6.8 REST
- A breath before THE RIVAL. No enemy; a health pickup in the middle; a cyan pool on the door.

## 6.9 THE MIRROR (boss: THE RIVAL's arena, reserved)
- 34 x 34 x 14, empty tonight: a warm light and a cyan pool at the centre. The walk ends at its door (the walk bot proves 6.1-6.8).
- **The arena RIVAL.md proposes** (built only after GG's yes): a square that is the same on every side, because the fight is a mirror: four
  ride panels in a pinwheel (10 m long, 5 m tall, 6 m in from each rim wall), a centre block at boost height (6 x 6 x 3.6 m, `noride`)
  with two nodes 9 m above its corners, no cover that stops a hook. Everything the player has, THE RIVAL also uses; nothing is the boss's.
- Not built tonight: the RIVAL's body, AI and fight (GG owns enemy design).

## Round 2: the greybox, changed while building (levelgen and the walk bot), and measured
- **Exits at floor level everywhere:** a door is a floor-level gap, so every perch, balcony and catwalk that meets the north wall is split in
  two halves with the exit lane between them (6.1, 6.2, 6.7); 6.3's exit is on the north floor behind THE STEP.
- **6.3 THE STEP:** 22 x 32 (was 26 long): the deck lz 12..20; the up-ramp by the west wall (lx -8, lz 1 -> 12); a DOWN-ramp north of the
  deck (lx 8, lz 30 -> 20) to the exit's floor. Every enemy spot is ON the deck (spots on the north floor put enemies out of sight behind the
  3.6 m face). Wave 1 is a DRONE + six swarmers (was two spitters + six: see the walk below). levelgen's route graph finds no path over THE
  STEP (not checked why); the crossing is proved by the real Mover's walk proof (completable f3, 7.6 s) and named as a `routeVia`.
- **6.2:** a second balcony ramp (the west half was unreachable by walkers). **6.6:** wave 1 is two wardens + ONE spitter (two cost 12 of
  wave 1's 10 points, design.n27.budget); the ledge's ramp foot 2 m clear of THE WALL. **6.7:** four waves (the budget's 2 kinds a wave, 3
  with a tank): husks + spitters; a tank + swarmers; a warden + drones; chargers + husks.
- **6.9 THE MIRROR:** `kind: "boss"`, `boss: "rival41"`: no sim code answers that id (SurvivalSim.BossRoomBegin returns false), so the room
  clears at once on entry (`n41fin level`: phase Cleared, 0 alive, PASS). levelgen accepts it (0 errors).
- **Measured:** n36-jobs GREEN; levelgen 0 errors (the whole line 33 s of travel on the route graph); `n41fin level` GREEN (every kind spawns).
  `n41fin walk` (the KESTREL, gauntlet and THUNDERCLAP given by hand): 6.1 and 6.2 cleared 2 / 2 (medians 24-26 s and 46 s). 6.3: 0 / 2: the
  bot walks into THE STEP's face under the enemies and dies there (the bot's nav graph, like levelgen's, finds no way over the deck). 6.4 (from
  6.4): 0 / 2 in 900 s (two husks left; the bot in the air near the posts: not diagnosed yet). Round 3 continues the walk.

## Round 3: the walk (the root cause, the fixes, the numbers)
- **Room ids m1..m7, mr, mx (were f1..f7, fr, fx).** Walk29Pilot scripts the n29 / n30 foundry rooms by room id alone (f2 / f3 GapRoom, f4
  Flue, f5 Ride, f7..f9), so the walk bot ran THOSE rooms' lanes here: in 6.3 it held x 0 at THE STEP's face, hopping, killing nothing
  (`n41fin walk --probe`). This, not the spitters, was round 2's 0 / 2. 6.3's wave 1 is two spitters + six swarmers again (as written).
- **6.3 THE HOPS:** levelgen's route graph and the bot's (Core/Nav/NavGraph.cs) put nodes on box tops only and walk at most 0.4 m
  (nav.stepHeight): a ramp carries no node. Two blocks against THE STEP's face (1.2 m and 2.4 m, 3 m wide at lx -4) are three hops up for
  the graph and for a player without the boost (the boost is one jump); the ramp stays the enemies' climb. Tried first: 0.4 m stairs (the
  graph PASS, but the real Mover does not walk up a 0.4 m step: the walk proof stopped) and one 1.8 m block (the graph PASS, the bot stood
  on it for 1400 s: its follower never flew the 1.8 m double).
- **6.7:** the roof 10 m (was 12; a drone at 11.4 m was out of the bot's reach) and the middle panels lz 5..14 (were 6..18).
- **Measured** (`n41fin walk --seeds 10`, the KESTREL, gauntlet and THUNDERCLAP given): m1-m6 10 / 10 (medians 23.3, 21.9, 26.0, 18.3, 21.8,
  35.5 s; 6.6 has 33 deaths in 10 runs: the wardens), m7 8 / 10 (median 57.3 s), mr 8 / 10: **8 of 10 runs**. The two misses: the bot stuck
  at (lx 8.8, lz 7.7) in 6.7, in the nook between the east deck's SW corner and its ramp, under wave 3's drones (144-149 unsticks). levelgen
  0 errors; n36-jobs GREEN.

## Round 4: 6.7's corner
- 6.7 HOPS (as 6.3's): a 1.25 m block at each deck's front (lx -8 / 8, lz 7.5..10) and a 3.75 m block in each deck-to-catwalk gap (lz 24..26),
  so the route graph climbs by hops (the bot's follower failed the 2.5 m double it offered: round 3's two misses at lx 8.8, lz 7.7).
- **Measured** (`n41fin walk --seeds 10`): **10 of 10 runs** through m1..mr; m1-m6 as round 3; m7 10 / 10, median 46.9 s (p90 76.8), 2 deaths.
  levelgen 0 errors, n36-jobs GREEN.

## The checks (round 2 on, when the greybox lands)
1. `node Tools/night/n36-jobs.cjs Design/levels/n41/finale.plan.json`: GREEN (every solid `job` + `why`).
2. levelgen: 0 errors (the walk proofs, the routes, the doors, the lights).
3. `n41fin walk --seeds 10` (Tools/SimHarness/Harness.N41.Finale.cs, the N41Finale partial): the walk bot through m1..mr with finale41
   forced on in the harness only (design stays OFF), per room cleared k / n, deaths, median clear times; the target 10/10.
4. `fingerprint` and `fingerprint --off41` EQUAL (finale41 is a data row; with it off nothing reads n41-finale).
5. The frames the lead takes (round 3, if the greybox is in a build): the door view of each of 6.1-6.7 (is the line readable from the door?),
   6.5's channel with a husk in the lava, 6.6's window from the ride panel.
