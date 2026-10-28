# Levels data for FORGE68: NIGHT67's six rooms at r3, the other rooms of levels 2-5, and six rooms for NIGHT68

LEVELS-DATA scout, 2026-10-24, HEAD 6b298590. Read-only: I read files and the harness source, and ran one read-only python
script (RF below). I ran no dotnet, harness, build, Player or Editor. Every number names its file. "Not checked" means not measured.
Grounding: the repo only (NIGHT67-REPORT.md, Logs/n67, the cards, the builders' notes, FORGE67b's levels-data.md); no Drive, mail or
ticket source was searched, because this scout's scope was local read-only files.

**Source keys**
- SK-i / SK-k / SK-s: `Logs/n67/r3-iceline|kiln|spire/verbs-skip.txt` (skip v2, seeds 1-10, line numbers given).
- In `Logs/n67/r3-<L>/`: TB `table.md`, VM `verbs.md` (M3), AR `area-<id>.txt` (lint), DE `deco-<id>.txt`, RC3 `<id>/room-check.txt`,
  AIR3 `air.txt`.
- RB `Logs/n67/red-bars.txt`; GP `Logs/n67/gate/g7/verbs65-skip.txt`; BL `Logs/n67/area-baseline.txt` (the lint on all 25 shipped rooms);
  GGP `Logs/n67/gg-proxy-close.md`; CARD `Research/2026-10-24-wave67/cards/<id>.md`.
- R.md `.claude/worktrees/agent-a9d0ffb6d0dfdb2be/Research/2026-10-24-wave67/R.md`; S.md
  `.claude/worktrees/agent-aeb9fc116ba05ef4d/Research/2026-10-24-wave67/S.md`.
- LD `Research/2026-10-24-forge67b/levels-data.md` (its FB walk-bot, RC room-check and CT keys; shipped rooms).
- RF: my read-only python over `Design/levels/{n52-iceline,n57-kiln,n52-span,n52-spire}.json` and `Design/unity-config.json`
  `design.n40.*.startSet / awards`: each fight room's loadout on entry, wave kinds, ride36 faces, tunnels and pits. Not kept (scratchpad);
  deterministic.
- Code: `Tools/SimHarness/Harness.N65.Verb.cs`, `Harness.N65.VerbPilot.cs`, `Harness.N46.Flow.cs`; numbers from `PortExport/config.json`.

## 0. Five facts the forge needs before it ranks anything

1. **The GATE's M2 has two clauses, and the second cannot be met on levels 3 and 5.** M2 asks for the ratio against mainBar and STYLE
   +15 (NIGHT67.md:102-103); skip v2 prints them as its own "M1" and "M2" lines. The STYLE test is 15 absolute points: `sa >= sr + 15`
   (Harness.N65.Verb.cs:1059).
   - The faster refusal's STYLE at r3: g7 19.6, g8 28.7, kb3c 69.7, v2 70.0, kb3 80.1, v6 88.6 % (SK-i:58, :120; SK-k:153, :58; SK-s:58, :142).
   - So v6 would need 103.6 % (impossible) and kb3 95.1 %. This is a rule question, not a room question.
2. **The verb-aware launch trigger accepts swarmers, and the punch kills a swarmer before it can be thrown.**
   - The trigger takes any `e is Husk` within 5.5 m and 50 deg (VerbPilot.cs:197); a Swarmer is a Husk (S.md:38). punch.damage 10 and
     swarmer.hp 10 (config).
   - Swarmers among the husk-shaped bodies in the shipped waves (RF): v2 5 of 10, v6 6 of 13, kb3 4 of 17.
   - Separately, the M3 launch pilot already picks a 40 hp husk, and its punch still killed it in kb3: "target dead (punch)" (VM kiln).
3. **The ride M3 cannot pass in g1-g5.** `ride --shoot` needs a gun kill from the wall. Level 2 starts with the gauntlet alone
   (`iceline40.startSet ["gauntlet"]`); the KESTREL comes on entering g6 (`iceline40.awards`; RF; R.md:106-107).
   - g8's M3 is RED for another reason: the one live body when the wall is ridden is W1's charger (VM iceline, "alive 1"; CARD).
     charger.hp 150, revolver damage 25: 5 shots deal at most 125.
4. **Each pilot fix moves the reds, so every bar must be re-made before a round.** The put-in loadout fix moved g7's bar 1.25 -> 1.39
   (NIGHT67-REPORT section 1; RB). mainBar = max(1.25, red + 0.20), and a fixed ride pilot changes g8's red (0.84) and kb3c's (0.91).
   Corner: "stale reds beside new bars".
5. **The instruments cannot reach some places at all.** Levels 2-5 have no tunnels (RF: 0 in every fight room), so the slide pilot has
   nothing to fire on. The span is not in `Tools/night/levels67.json` (keys: foundry, iceline, kiln, spire), so p1, p3 and p5 need
   T1a-style tooling first (NIGHT67.md:113-114).

## 1. NIGHT67's six rooms at r3 (the final files, main f8ea361c)

The bar is from RB. Guards are deaths A / B and stall A, each against the red's ceiling (RB, TB). M1 is the lint (AR). M4 is the decoration
checker (DE). M3 is the verb pilot (VM).

| room (verb) | ratio vs bar (exercised) | STYLE aware vs refused (need) | M1 | M3 | M4 | guards vs ceilings | gap to PASS |
|---|---|---|---|---|---|---|---|
| g7 (parry) | **1.32 vs 1.39** (10/10) SK-i:57; seeds 11-20 not run on r3 | 70.6 vs 19.6 PASS | PASS 4/4 | GREEN | PASS 4/4 | 1 / 3 (1 / 3), 19.0 (29.3) | +0.07 on 1-10; 11-20 |
| g8 (ride) | 0.77 vs 1.25 (10/10) SK-i:119 | 29.9 vs 28.7 (43.7) | PASS 6/6 | RED | PASS 5/5 | 0 / 0 (1 / 3), 21.4 (30.1) | +0.48, STYLE, M3 |
| v2 (launch) | 1.01 vs 1.25 (7/10) SK-s:57 | 78.9 vs 70.0 (85.0) | PASS 4/4 | RED | PASS 9/9 | 0 / 0 (0 / 0), **24.7 (19.0)** | +0.24, STYLE, M3, stall |
| kb3 (launch) | 1.02 vs 1.35 (10/10) SK-k:57 | 83.4 vs 80.1 (95.1) | PASS 4/4 | RED | PASS 7/7 | 0 / 0 (0 / 2), 21.8 (22.7) | +0.33, STYLE, M3 |
| kb3c (ride) | 1.06 vs 1.25 (10/10) SK-k:152 | 68.9 vs 69.7 (84.7) | PASS 6/6 | GREEN | **FAIL 8/9** | **1** / 0 (0 / 2), 30.2 (33.0) | +0.19, STYLE, deco, a death |
| v6 (launch) | 0.96 vs 1.33 (9/10) SK-s:141 | 88.4 vs 88.6 (103.6) | **FAIL 4/5** (P9) | pilot GREEN, slate FAIL | PASS 1/1 | 0 / 0 (0 / 0), **19.6 (18.9)** | +0.37, STYLE impossible, P9, M3, stall |

After those items, every room still needs the rest of the GATE: a level review, a blind spotter and a critic. No room reached them.

### g7 THE COAL CHUTE (parry, level 2)
- **Changed from shipped (29 -> 10 items, DE):** built two coal bins (hulls to the 10 m roof, a staggered 4 m chicane), THE HOPPER (a
  3.5 m deck on the chute's axis; every wave spawns on it) and its stair; cut 3 lava pits, both sluice ride walls, the gantry, the keeper's
  tower, the bunkers and the posts; W2 now 3 spitters + 4 swarmers; a second health pickup (CARD).
- **What it lost:** air65 1 piece, 0 hand-offs, dead ends 1 / 1 (AIR3 iceline:11), against the shipped room's 95 hand-offs (LD). GGP item 5:
  "bare", "3 boxes (0 ride), 1 ramps".
- **The other seed set:** 1.24 on r2's file in the GATE pack (GP:59); r9 read 1.32 / 1.30 in R's tree (R.md:90). Every variant r6-r9 pools
  at 1.30-1.32, and the per-seed spread (0.67-4.30) is wider than any change's effect (R.md:92).
- **Remaining fault:** one line serves the spitter's sight, the KESTREL's aim and the returned glob (HuskBrain.cs:816-822,
  Spitter.cs:455-458; R.md:97-100). Cover that blocks the gun also stops the spit, so the refusing player just shoots the spitters.
- **The one move: a rule change.** GG's call, put to him in DECIDE67:60-61: a parry-only wave, or a spitter only a returned glob can hurt.
  None of the three pilot fixes touches parry, and geometry has been tried (r6-r9).

### g8 THE TURNTABLE (ride, level 2)
- **Changed from shipped (24 -> 12 items, DE):** built THE ENGINE (a deck 6 x 10 m, 3.5 m high), its two step ramps and two 0.9 m coal
  carts; moved the inspection pit across the west lane; cut the east lane ride wall, the five pillars, the sand towers, the hoppers, the
  signal box and its ramp, the cross-hoist pad and the ash pit; waves kept (CARD).
- **The ride is slower and no shortcut:** medians 47.4 s verb-aware against 33.2 s refused, with 92 wall rides against 55 caught
  (SK-i:121). I2 FAILs at 1.10 (walk 25.3 m, ride 23.0 m; RC3); the shipped room read 1.40 (LD).
- **M3:** 5 shots from the wall, 0 kills (VM; the lone 150 hp charger, section 0 fact 3). n53ridekill on another start is GREEN (g8/ride.txt).
- **Remaining fault:** the verb-aware pilot rides any face within 2.5 m to its far end (VerbPilot.cs:234); the extra rides start on the
  room's own east wall and on the engine (R.md:80-82).
- **The one move: pilot fix (a).** It removes the measured 14 s cost, but the ride saves only 2.3 m of 25.3, so expect a ratio near 1.0
  at most (not checked). A PASS also needs a room change (the ride the only short way, I2 1.3 or more), a W1 the wall can kill, and STYLE
  +13.8 more.

### v2 THE CRYPT (launch, level 5)
- **Changed from shipped (31 -> 35 items, DE):** built a 10 x 8 m lava crypt with the bell moved into it as a 5 m island, six 0.9 m chancel
  rails and the narthex stair; trimmed the wing to 5.5 m; cut the two 14 m apse posts and the bell hoist pad. Everything else is as
  shipped: the same ramps, decks and both 14 m entry cheeks (GGP item 3, "shipped room + patch").
- **The launch barely fires:** launch36 11 over 10 seeds; seeds 3, 8 and 10 never launched (SK-s:7-52). M3: the punch landed at 1.6 m and
  the target stayed alive, launch36 0 (VM); why is not established. Stall A 24.7 % (B 31.0) against a ceiling of 19.0 (TB).
- **Remaining fault:** the walk bot's lava rule holds any step whose point 1.5 m ahead is lava *in plan*, at any height
  (Harness.N46.Flow.cs:569-572). The bell's crown and the beam stand over the crypt: 1860 hold steps at r1c, 0 shipped (R.md:43, 75-77).
  Every skip run uses that rule (the SK-s header). And about one launch per seed cannot carry 20 % of a 43 s clear.
- **The one move: a room change.** Bring the husks onto the throw line with the crypt behind them, in a room rebuilt from scratch (GG's
  2026-10-09 order). Pilot fix (c) is the precondition: it clears the stall guard and the hold noise but does not move the ratio by itself
  (not checked). Fix (b), if it reaches the verb-aware trigger, stops episodes wasted on the 5 swarmers.

### kb3 THE PRESS LINE (launch, level 3)
- **Changed from shipped (24 -> 24 items, DE):** built THE ANVIL (1.7 m, the R5 foothold), the 14 m press gate posts at the chute's mouth,
  the flush press bed, a 0.9 m east bed and a dry west tank 0.5 m deep; moved the hammer vent pad to the entry floor and the husk nodes
  beyond the gate; cut the door liners 14 -> 1.7 m. Kept the gallery, both ramps and the west deck (GGP item 3).
- **More launches did not help:** launch36 went 32 -> 45 with the chute and the ratio did not move (S.md:224); r1's lava had raised it to
  1.33 only by killing the refused runs (S.md:217). M3 RED: the punch killed the 40 hp husk it should have thrown (VM).
- **Remaining fault:** the throws land on the bed and the posts, never in a hazard. The tank on the throw line was never built: it needs
  the gallery ramp re-routed and 2 m strips for the nav grid (S.md:153-154).
- **The one move: a room change** (the hazard on the throw's line, S.md:237-241). Even then M2 needs STYLE at 95.1 %: a rule question
  (section 0 fact 1).

### kb3c THE TAP RING (ride, level 3)
- **Changed from shipped (24 -> 25 items, DE):** raised the run wall's base to the floor; built THE SPOUT TOWER (3.5 m; the last wave's 2
  drones and 4 swarmers on it), grew the furnace 6 x 6 -> 6 x 9 m, added THE SKIM WALL (a 4.5 m ride wall) and the tower, furnace and slag
  ramps; cut the east door liner. Kept the 4 m lava tap run, the two 12 m hulls and the decks (GGP item 3).
- **Static items:** all hold but the decoration checker, whose tower ramp was touched on 2 of 10 seeds (DE). M3 GREEN, a revolver kill
  from the run wall (VM). I2 1.83 and 1.52 (RC3).
- **The refused run's 84 "rides" are not a leak:** the backstop catches each ride that starts and ends it after one step on the wall
  (Harness.N65.Verb.cs:657-676). That corrects GGP's reading that "the run with the ride refused still rode 84 times".
- **Remaining fault:** the clear is kill-bound and the pilot rides any face (S.md:60-61); a climb at the spawn's side helps the refused
  runs (r2d 0.92, S.md:230-231); the floor lava kills about once in 10 seeds: death A 1 against a ceiling of 0 (TB; S.md:246). Fix (c)
  does not touch floor lava.
- **The one move: pilot fix (a).** Then S's room change: the tower ramp 7 -> 14 m, so the ride is the only short way up (S.md:243). STYLE
  needs +15.8 more: the risk.

### v6 THE CHOIR (launch, level 5)
- **Changed from shipped (22 -> 23 items, DE):** built the island's north ramp and a dry bell gutter 0.5 m deep; made the organ pipes a
  ride wall (for P17). The card's 105 m2 casting pit is not in the file (GGP item 3). The west lane pier, which the card cut, came back at
  r1c (S.md:73).
- **The launch fires about 21 times per 10 seeds whatever the room or the waves** (S.md:225-227; SK-s:143). Of 13 husk-shaped bodies, 6
  are swarmers (RF). The lint's P9 FAILs on the organ loft (AR).
- **The one move: a rule change.** STYLE +15 is impossible at a refusal of 88.6 %. Otherwise the room needs a new verb with its own red;
  S suggests pierce (S.md:247-248) and recommended SHORT (S.md:182).

## 2. The rooms NIGHT67 did not touch (levels 2-5)

Weakness numbers are from LD (FB walk bot, 10 seeds; RC; flags) and BL (the lint on the shipped room). The loadout and "fireable" columns
are from RF and the pilot's verb set: launch, pierce, ride, parry of a spitter's glob, slide (VerbPilot.cs:6-15). Launch needs a punch, so a
gun in hand, never the gauntlet (VM kiln: "the gauntlet in hand drops the punch"); pierce needs the KESTREL (revolver); parry needs
spitters. No room here has a skip run or a red (searched Logs/n65-n67 and Research: none), so every bar is still to be made.

| room (job) | carded verb (verb36) | loadout on entry | pilot verbs it can fire | weakness numbers | NIGHT68? |
|---|---|---|---|---|---|
| g1 (teach) | the tank punch | gauntlet | ride (2 faces); ride M3 impossible | 572 m2, 2 bodies, DULL 100, gap 66.3 s, stall 43.7, I2 1.08; lint 1/3 (OPEN 16 m) | no: needs a gauntlet pilot |
| g2 (teach) | the warden's shield | gauntlet | ride (1, I2 1.40) | stall 43.3, gap 38.5, DULL 60.9; RC PASS; lint 2/3 (P4 0/5) | no (same) |
| g3 (teach) | the LT blast | gauntlet | ride (1, I2 1.28) | **stall 67.2** (worst), air 0.9, DULL 74.7; TOUR RED (mid36 pose in a solid); lint 2/3 | no: gauntlet pilot, TOUR fix |
| g4 (twist) | a drone's orb parried | gauntlet | ride (1, I2 1.18); parry is for spitter globs only | 720 m2, 3 drones, DULL 88.4, gap 35.8, clear 39.3 vs par 30; lint 2/5 | no: the parry pilot needs orbs |
| g5 (twist) | the blast's cooldown | gauntlet | ride (4 faces, I2 PASS) | gap 62.7, stall 41.5, DULL 74.8; lint 2/5 (P1, P4 2/8, P17) | no: ride M3 needs a gun |
| g6 (reward) | the KESTREL returns | gauntlet + revolver on entry | pierce, launch (3 husks) | 252 m2, 3 bodies, 1 plane, clear 10.2 s; lint 0/1 (OPEN 12.5) | no: 3 bodies |
| kb2 (teach) | the THUNDERCLAP boost | **shotgun only** (KESTREL at kb2b) | parry (4 spitters), launch (14, 8 swarmers), ride (1, I2 1.44) | DULL 63.6 (level 3's highest), clear 36.4 vs par 60; RC PASS; lint 2/5 (P1 2/10, P4 2/5, P9) | **yes** (section 3) |
| kb2c (teach) | the burrower | shotgun, revolver | launch (13, 6 swarmers), pierce, ride (1, I2 2.18); no pit, no spitter | DULL 16.8; RC PASS; lint 2/3 (P4 0/4) | maybe (pierce, 7 husks) |
| kb3d (combine) | burrower + the charger's stun | shotgun, revolver, gauntlet | launch (8, 4 swarmers) **into its lava pit**, pierce, ride (1, I2 1.50) | 480 m2 (level 3's smallest), DULL 0.0, clear 26.6 vs par 60, deaths 0 with its lava; RC PASS; lint 2/3 (**P4 0/6**) | **yes** (section 3) |
| p1 (teach) | the grapple pull | no startSet row; not printed | parry (2), launch (7), pierce, ride (2) | falls 10, clear 26.6 vs par 20; RC PASS; lint 2/5 | no: tooling, grapple |
| p3 (practise) | the swing over the void | as p1 | launch into the void (13), parry (2), pierce, ride (2) | **falls 55**, R2 and IN VIEW FAIL, AIR dead ends 3/4; lint 2/3 | later: span tooling |
| p5 (combine) | the whip | as p1 | parry (4 on the spool island), launch (13), pierce, ride (4) | falls 42, R5; lint 2/5 | later: span tooling |
| v5 (practise) | the grapple pull up the ropes | revolver, gauntlet, shotgun (spire put-in, VM spire) | launch (7, 2 swarmers), parry (1), pierce, ride (1 of 2, I2 1.54) | 8 bodies, clear 21.6 vs par 10 (stale par, LD), DULL 0.0; R5; lint 1/4 (P4, P9, P17) | weak: 8 bodies |

Read across the table:
- **No verb in g1-g5 can be measured.** Their verbs are the gauntlet's, and their only fireable pilot verb, the ride, cannot pass M3 with
  the gauntlet alone. The report's own seed agrees: g1-g4 "still wait for a gauntlet pilot" (NIGHT67-REPORT:124).
- **kb2 is the room R said was missing for parry** (R.md:101-103: "a spitter the gun hits only from its own deck (range ...)"). Its only
  gun is the THUNDERCLAP: 8 pellets x 8 damage, range 60 (config). Spitters fight at 6-24 m with 60 hp (config), and the KESTREL (25
  damage, range 200) is not owned until kb2b (`kiln40.awards`). Not checked: how many pellets land at 12-24 m, and the refusal's STYLE on
  level 3 (kb3c's 69.7 and kb3's 80.1 suggest it is high).
- **kb3d already holds lava that has never killed the walk bot** (FB deaths 0, LD): the one level-3 launch room where "in lava yes" needs no
  new hazard. Its six tall solids have no job (BL: P4 0/6).

## 3. Six rooms for NIGHT68, ranked

This ranking assumes the three pilot fixes land first: (a) the ride pilot rides only line65's face; (b) the launch pilot targets bodies
that survive the punch; (c) the walk bot reads lava heights. It also assumes every red is re-made after them (section 0 fact 4).

1. **kb3c (ride, level 3).** Fix (a) is aimed at its fault, and every static item already holds: M1 6/6, M3 GREEN, room check PASS, I2
   1.83. It has the smallest ratio gap of the rooms that need no rule change (1.06 vs 1.25). Still to do: one deco piece (the tower ramp,
   2 of 10 seeds), the floor-lava death (A 1 against a ceiling of 0, which fix (c) does not cover), and S's tower ramp 7 -> 14 m. Risk:
   STYLE needs 84.7 % against 68.9 %. Against GG's order: it is a patched shipped room (GGP item 3).
2. **g8 (ride, level 2).** Fix (a) removes a measured 14 s cost (47.4 vs 33.2 s). Level 2 is GG's weakest level on four level-wide numbers
   (LD), and it is the level where the STYLE clause can be met (refusal 28.7 %). Still needs a room change that makes the ride the short
   way (I2 1.10 now) and a W1 the wall can kill, for M3.
3. **kb2 (parry, level 3; new).** The one room where parry is not undercut by the KESTREL. The parry pilot exists. Level 3 is the one GG
   said "just isn't good" (CORE.md). Cost: a red first, and a re-card: its verb36, the THUNDERCLAP boost, stays (PRINCIPLES rule 1). Not
   checked: the shotgun's reach at 12-24 m, and its STYLE.
4. **g7 (parry, level 2), only if GG answers DECIDE67:60-61 with an enemy rule.** The closest room on the numbers: every item but the ratio
   holds (1.32 vs 1.39). Without his answer, keep it SHORT: geometry has been tried (r6-r9). Also to fix: its cut air play (1 air piece, AIR3).
5. **v2 (launch, level 5).** Fix (c) clears its one failing guard (stall 24.7 vs 19.0). Its STYLE gap is the smallest of the launch rooms
   (+6.1), and fix (b) matters here (5 of its 10 husk-shaped bodies are swarmers). It needs a from-scratch rebuild with the fight on the
   throw line: the current file is the shipped room with a patch (GGP item 3).
6. **kb3d (launch into its existing lava, level 3; new).** The smallest level-3 room (GG, 2026-10-09: "a lot smaller"), and its lava is
   already safe for the walk bot. P4 0/6 says its pieces need jobs. Cost: a red first; STYLE not checked. Alternate: **kb3** takes this
   slot if the forge changes M2's STYLE clause: every guard and static item there holds, and it needs only the hazard on the throw line,
   plus M3 after fix (b).

**Not ranked, with the reason:** v6 (STYLE +15 impossible; S recommended SHORT); kb3 (STYLE needs 95.1 % under the current rule); g1-g5 (no
gauntlet pilot, and a ride M3 needs a gun); g6 (3 bodies); p1, p3, p5 (the span is not in levels67.json); v5 (8 bodies, a 21.6 s clear);
kb2c (no hazard, no spitter: a pierce room at most).

**The cheapest single change for the night is a rule decision, not a room: M2's STYLE clause.** It blocks v6 outright, makes kb3 need
95.1 %, and is the main risk in kb3c and v2 (section 0 fact 1).
