# NIGHT52 REPORT (for GG; THE ARENA NIGHT, 2026-10-19 22:29 - 2026-10-20 06:2x MST)

GG asked (2026-10-19 ~22:00): plan an 8 h night, "be honest and critical on where we still need to improve and polish", compare open-source
projects, roleplay, all related to building and iterating. The night rebuilt the combat rooms to a numbered contract and ran a polish lane.

**Play build: `Builds/n52-freeze/SKYGRIND.exe`** (commit bee5387f; later commits are docs). `-skygrindOff52` gives NIGHT51's game (every n52 row off).

## The honest read (what was wrong, measured before building)
1. The combat rooms were big and nearly empty: 0.33-1.81 pieces of geometry per 100 m2 (kiln's kb3: 40 x 46 m with 6 pieces), not one jump
   pad in any level, most rooms one or two standing planes. A room with nothing in it asks for no verb: you stand on the floor and shoot.
2. Fifty nights of process, little play; tonight's measure is rooms rebuilt that pass a contract, and the share of minutes spent building.
3. The HUD looked generic (a green health bar outside the palette), enemies read as glowing jelly up close, the guns were flat blocks.

## What was built
### The rooms: 18 rebuilt to THE ROOM CONTRACT (17 at 14/14 or 15/15; iceline g4 13/14, its one miss the teach room's single enemy type, exempt by D-24)
The contract (computed by Tools/night/n52-room-check.py, all of them in one table by Tools/night/n52-contract-all.sh): 20-32 m on the long side;
>= 2.5 pieces per 100 m2, each with a job and a why; exactly 3 standing planes that walkers reach; a ride wall >= 17 m; an upward line (a
pad); health at the riskiest spot; spawns on two heights, none at the door; the walk pilot clears 10/10 on two seed sets with the door
dwell no worse; the hold probe (20 s on each tier's best spot, waves live) finds no 0-hit spot. Teach rooms keep their lesson's one enemy type.

| level | room | before | after | pieces / 100 m2 |
|---|---|---|---|---|
| kiln | kb2 3.2 POUR HALL | 36 x 44 m, 7 pieces | 24 x 28 m, 17 pieces | 0.44 -> 2.53 |
| kiln | kb2c 3.2c THE ASH BED | 28 x 30 m, 5 pieces | 20 x 30 m, 15 pieces | 0.60 -> 2.50 |
| kiln | kb3 3.3 CASTING FLOOR | 40 x 46 m, 6 pieces | 22 x 28 m, 19 pieces | 0.33 -> 3.08 |
| foundry | a3 1.3 RIDE | 16 x 26 m, 2 pieces | 20 x 26 m, 14 pieces | 0.48 -> 2.69 |
| foundry | a5 1.5 CRUCIBLE | 20 x 24 m, 5 pieces | 20 x 32 m, 18 pieces | 1.04 -> 2.81 |
| foundry | b1 1.6 THE YARD | 36 x 40 m, 6 pieces | 20 x 32 m, 19 pieces | 0.42 -> 2.97 |
| iceline | g2 2.2 THE SHIELD | 24 x 28 m, 8 pieces | 24 x 28 m, 17 pieces | 1.19 -> 2.53 |
| iceline | g3 2.3 THE SWARM | 26 x 28 m, 8 pieces | 24 x 28 m, 17 pieces | 1.10 -> 2.53 |
| iceline | g4 2.4 THE REACH | 24 x 30 m, 6 pieces | 24 x 30 m, 19 pieces | 0.83 -> 2.64 |
| iceline | g5 2.5 THE COOLDOWN | 26 x 30 m, 6 pieces | 21 x 30 m, 16 pieces | 0.77 -> 2.54 |
| iceline | g7 2.7 THE MIX | 30 x 32 m, 5 pieces | 24 x 32 m, 21 pieces | 0.52 -> 2.73 |
| iceline | g8 2.8 THE STAMPEDE | 24 x 30 m, 13 pieces | 22 x 30 m, 17 pieces | 1.81 -> 2.58 |
| spire | v2 5.2 THE NAVE | 30 x 38 m, 19 pieces | 24 x 32 m, 21 pieces | 1.67 -> 2.73 |
| spire | v5 5.5 THE RINGERS' STAIR | 12 x 35 m, 4 pieces | 24 x 26 m, 16 pieces | 0.95 -> 2.56 |
| spire | v6 5.6 THE CHOIR | 28 x 42 m, 12 pieces | 24 x 32 m, 20 pieces | 1.02 -> 2.60 |
| span | p1 4.1 HOOK YARD | 32 x 36 m, 16 pieces | 24 x 32 m, 20 pieces | 1.39 -> 2.60 |
| span | p3 4.3 KNOT YARD | 40 x 40 m, 25 pieces | 24 x 30 m, 19 pieces | 1.56 -> 2.64 |
| span | p5 4.5 SPOOL HALL | 36 x 36 m, 22 pieces | 26 x 28 m, 19 pieces | 1.70 -> 2.61 |

The before / after sheet (plans and each room's entry frame, old level vs n52, one build): Logs/n52/sheet/sheet.png, sheet-2.png,
sheet-3.png. Every room's contract line: Logs/n52/contract-table-freeze.md (+ Logs/n52/contract-fix6.txt for g4, K's print for g2, F's for p1).

Every rebuilt room has an identity card in docs/ROOMS.md (its place, its verb, its 30 s problem, the job of every solid). The campaign got
its first jump pads (0 before; now kiln 5, foundry 7, iceline 15, spire 6, span 4) and the verbs37 pad row now runs on every level. Every room had at least one pair of persona
reviews (an ULTRAKILL P-rank runner and "you" from OWNER.md); the checks decided which findings were real (DECIDE52 D-14, D-16, D-21,
D-23, D-28, D-30): e.g. span p5's island with the health behind a void moat was a camp (a bridge now), spire v5's door opened onto a wall
(thinned to 1 m), iceline g7's bunker ramps faced the wall (turned), kiln kb2's lesson "SHOOT DOWN. FLY" landed nowhere (an 8 m ledge).

### The polish lane
| row | what you will see | off switch |
|---|---|---|
| hud52 | the health and weapon blocks as hard-edged leaning ink panels; health bone, red-orange only below 30 %; a hit shake, a heal pop, a swap kick; the room's lesson card shrinks to a chip after 6 s or the first kill; the counter, the score and the room title on the same plates | -skygrindOff52 |
| skin52 | enemies close up: the jelly rim cut, a two-tone body; ink51's outline gaps closed | -skygrindOff52 |
| guns52 | the KESTREL and THUNDERCLAP machined (chamfers, recessed two-tone panels, lit edges; same silhouettes); the muzzle light aims at 40 ms of real time (measured 1/2 at the freeze: a long first-shot frame stretches it, T-122) | -skygrindOff52 |
| look52 | the health pickup is a chunky bone plus that reads at 15 m (no label); the grapple node is the thin cyan ring, the rope pale; foundry's ledge faces lifted against the walls | -skygrindOff52 |

## Checked (measured)
- **verbs37 on n52-freeze** (every verb the level owns, performed in a Player): foundry 27/27, iceline 21/21, kiln 30/30, span 31/31,
  spire 31/31; RED rows none; NIGHT51 had 26 / 20 / 25 / 30 / 30: nothing lost. 33 Player reels, NRE 0 in every one: 30 on the freeze build
  as re-made at 55acccb5, 3 span reels (verbs37, a452, the entry frames) on the final bee5387f build (only span changed between them).
- **a452** (each rebuilt room's entry / fight / highest-tier frames): GREEN on all five levels (17 rooms, 51 frames; iceline g2 has no frames52 yet: not framed).
- **The polish proofs** (each on / `-skygrindOff52`): a152 GREEN / RED; a252 GREEN 8/8 on spire, 7/8 on kiln (the teach line ended before
  8 s there) / RED; a352 the machined guns 2/2, the muzzle light's real-time item 1/2 (T-122) / RED; tier52 on foundry +0.129 vs +0.049.
- **The flow table** (Logs/n52/flow-freeze/table.md; the walk pilot through each whole level, NIGHT51's level vs n52's, seeds 1-20):
  walks 20/20 on all five. Kiln better on stall, dwell, dead time and clear time (132.7 -> 95.6 s); foundry fewer deaths (24 -> 11.5) and
  faster (142.5 -> 113.5 s) but its door dwell is worse (12.1 -> 16.8 s); iceline less dwell and dead time but longer (220 -> 246 s);
  spire faster (163 -> 144 s) with more stall (14 -> 17 %); span more stall (12 -> 16 %). Four crosses past the band (T-124).
- **Golden / fingerprints** at the freeze commit: 22 / 22 replays exact, 0 mismatches; fingerprints plain / --off50 / --off52 EQUAL.
  SharedTests 112 / 112 in the Editor.
- the full batch suite at 907dc105: 2238 / 2238 passed (Logs/n52-close-tests.xml; the first run at bee5387f was 2237 / 2238: a test leak, T-126, pinned test-only); the smoke on n52-freeze: DONE, all steps passed (227.7 s, 0 NRE) with the Bluetooth "Pro Controller" present (NIGHT49-51's smokes failed four pad steps with it present).
- The building share (Tools/night/n52-share.py over the log's blocks): 68 % (373 logged minutes, 254 building; target >= 70 %: missed).

## Not verified or not done, said plainly
- The final persona reviews of the five rooms built late (a3, kb3, g3, g4, p1) say none would satisfy you as built (a3 the closest): g4 is a
  mirror down the door's axis, g3's hopper is a giant box, kb3's ride runs beside a walkable bank, p1's ramp makes the grapple optional in
  the room that teaches it, and four rooms share one shape (a ring round a central mass). Their first-minute bugs were fixed at the freeze
  (pads on walk lines and in front of a respawn, a ride ending in a dead end); the design findings are NIGHT53's first job (D-28, D-30).
- iceline g2 THE SHIELD was rebuilt by the loop after the last persona review: it shipped with its contract print (14/14) but unreviewed.
- Every contract number is the walk pilot's or the hold probe's, not a person's (T-118). No human has ridden these walls or taken these
  pads; the persona reviews read plans and frames, not play.
- Not rebuilt: iceline g1 TANK ALONE (its open charge lane is the lesson), g6 THE ARMOURY (a reward room); foundry a2 / a4 / a8 (a8's walk
  is scripted to its pit: a bot change first, D-26); the links (corridors) between rooms.
- The flow table has four crosses (above): the whole-level walk is worse on foundry's door dwell, iceline's clear time, spire's and span's
  stall share, though each room passes its own isolated contract (T-124).
- The scorch's hot core reads darker than bare floor (NIGHT51's mark ships, T-120); iceline's lifted grade reached 3/4 on the look meter
  (today's grade ships; span already met 4/4 with no lift: T-116); spire's ledge-face tiers measured RED (foundry only, T-121); the kill's wash peak on frames 3-4 has a source
  not yet found (the chunks' tail is capped; T-111); guns52's muzzle-light timing on a long first-shot frame (T-122); in a152 the
  THUNDERCLAP never fired on the reel's taps (T-125, the proof moved to a252's KESTREL).
- The teach lines that wait for their verb: proven on spire (a252), but on kiln the line ended before 8 s (not checked why).

## Next (my recommendation)
1. Play `Builds/n52-freeze`: a few rooms per level, riding the walls and taking the pads. Tell me which rooms feel like places and which
   feel like templates; the reviews already think g4, g3 and p1 lean template.
2. NIGHT53 (NIGHT53.md): the rooms' second pass on your words (or the reviews' if you have none), a human route reel per room, then the
   remaining rooms (foundry a8 with its scripted walk, iceline g1 / g6, foundry a2 / a4) and the links.
3. Then the polish leftovers in order: the first-shot hitch (T-122), the kill wash's f3-f4 source (T-111), the scorch core, iceline / span's grade.
