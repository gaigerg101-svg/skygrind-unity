# NIGHT56 builder G: notes (gameplay / enemies)

Every number here is a bot number (the walk pilot of n46flow, or n47human's human-model boss pilot) unless it says otherwise. Nothing here
was run in the Editor or a Player (builders never do).

## G4's row (the lead, 00:4x)

THE KILN WYRM's tail uses NIGHT55's existing sim row `design.n55.wyrm55` (sim, off, its `_why` is G4's job); G fills its `numbers` with
`Tools/night/n56-g-rows.cjs`. No new row (the lead's correction replaced the first message's `wyrm56`).

## G4 THE KILN WYRM's tail (NIGHT55's sim row design.n55.wyrm55): built, SHORT on deaths

Core/Enemies/Wyrm55.cs (the numbers), SurvivalSim.N56.Wyrm.cs (from the night's hooks, after wyrm46 / wyrm47: every step the row's values on
Wyrm31 / the worm's params, compare-before-restore at the next run; the pool once a fight), Wyrm55Tests (2/2 mini NUnit: encounter keys only,
the attacks' numbers as shipped), Harness.N56.Wyrm.cs (n47human's stats policy: hurt and the worm's hit counters), n56-g-wyrm.sh (40 seeds in
4 parallel chunks + the combined line). Encounter levers only: pool x0.6, contact cooldown 0.8 -> 1.6 s, P3 gate 0.33 -> 0.2, the spit
barrage's PACING x2 (P2 3.2 -> 6.4 s, P3 2.2 -> 4.4 s; its count / speed / damage / aim unchanged), the stagger's open x3 -> x4.
Why the barrage pacing: of the hurt in 9 baseline fights, body contacts 117 of 270 hits, the barrage 97, the head 50; the contact cooldown
alone moved contacts 117 -> 106 (separate touches).

| `n47human --boss wyrm --level n52-kiln --seeds 40` (bot) | killed | median | p90 | min | deaths | taken med |
|---|---|---|---|---|---|---|
| off (A0 / mine, equal) | 38/40 | 122.6 s | 256.9 s | 45.6 s | 68 | 194 |
| probe 1 (pool 0.7, contact 1.6, P3 0.2) | 38/40 | 99.4 | 246.8 | 54.2 | 62 | 166 |
| **probe 2 = shipped numbers** | **40/40** | **86.8** | **178.3** | 27.0 | **32** | 100 |
| probe 3 (barrage x3) | 39/40 | 67.0 | 195.9 | 27.0 | 33 | 84 |

Verdict (bot): 40/40, median 86.8 s (60-120), p90 178.3 s (<= 180): MET; deaths 32 (<= 10): NOT MET, SHORT. The deaths plateau at ~32
with x2 and x3 pacing: the walk-in pilot takes ~2 HP/s from contacts and the head; at 100 HP a 60-120 s fight cannot stay under 10 deaths
in 40 without changing an attack's own damage or cadence (out of wick54's method). Min 27.0 s (one seed) is under the old band's 60 s floor.

## P1-wickWindow (wick55's Player half: a155 FIGHT MODE) and Wick55Tests

**Wick55Tests' failing kit test: the TEST was wrong** (0020e42f): it compared wickSlam / wickDive with design.n37's legs (1-3), but wick47's
kit (Wick37.N47.cs StartFight47, shipping since NIGHT47, on and off) re-legs them (DIVE 0, SLAM P1 only). `n56 wickkit` runs the test's
comparison headless: 0 mismatching fields. The lead re-runs Wick55Tests in the Editor.

**FIGHT MODE, reproduced headless** (`n56 wickfight`, Harness.N56.Wick.cs: B55Fight's sim side on n52-foundry): the bare pilot gives "pilot
steps 18000, rides 2, leaps 4, shots 1, punches 33; OPEN share 55 %; core punches 0; alive at 150 s (bar 40 %)" (the Player's bfon: shots 0,
rides 0, punches 27, 40 %, alive, bar 40 %). Two causes:
1. The bot numbers wick55 was gated on (n47human) are Wick37Pilot INSIDE n47human's human model, whose trigger habit fires the KESTREL; the
   reel ran the pilot bare. Fixed: Bot/Wick56FightInput.cs adds that habit unchanged (30 / 15 steps, 20 deg), hooked in B55Fight with 2
   lines in V's FeelReel.N55.Boss.cs (the UseInput line and its restore in offs). Headless: shots 1 -> 26.
2. NOT fixed: on n52-foundry's stack the pilot never climbs (trace: feet on the floor at y 0-1.6 the whole fight, rides 2 in 150 s; on the
   survival map's stack n47human seed 1 rides 7 and kills at 62.7 s). P2 THE CLIMB floors the bar at gate[1] (40 %) until the player stands
   on the cap, so the core never opens and no core punch can land: FIGHT MODE stays RED until Wick37Pilot can climb the live level's stack
   (a pilot job, not the fight's). Not attempted further tonight (G4 first).

## G3 tokens56 (sim row design.n56.tokens56): built, bot numbers below

**Rules (Core/HuskBrain.N56.cs; HuskBrain became `partial`, 1 Rec line + 2 call-site lines in Think; SurvivalSim.N56.Tokens.cs):**
1. ON SCREEN FIRST: an enemy that wants a token while outside the SIM cone (the sim's own look, SurvivalSim.Aim(): input.Yaw / Pitch,
   half angle camera.fovDeg / 2 = 37 deg, eye to aim point; never the FOV setting) waits numbers.waitS (4 s) of thinks first: no token,
   its ring; after the wait it may attack from where it is (looking away is never a shield); the wait restarts when it commits.
2. Ranged accuracy vs speed ALREADY SHIPS (AimMath.HitChance: ai.aim.baseHit 0.7 standing -> minHit 0.25 at fullSpeed 24 m/s, rolled in
   Think for every ranged token holder). Nothing added.
3. ADVANCE: a waiter with no token, no attack, standing (< 0.5 m/s) for 1 s walks to the current room's node (RoomDef.nodes) inside the
   cone nearest it (>= 3 m), until within 1.5 m or 4 s. Flankers (swarmers) and stationary kinds keep the brain's own rules.
Campaign runs only (as tokens41). Off: brain.on56 false, every line skipped (golden 0 mismatches, fingerprints EQUAL, the off flow
equal to flow-base).

**The meter** (n46flow's TOKENS / RANGED / IDLE lines, Harness.N56.Tokens.cs, fed from the style column's hooks): attack starts =
`enemyCommit` inside the cone or not, by kind; ranged = each spitter volley / drone shot, the player's speed at the shot bucketed
(stand < 2, run 2-10, fast 10-18, dash >= 18 m/s), a hit = a `hurt` of that source within 2.5 s; idle = an enemy's steps under
0.5 m/s not in an attack (lancer and burrower left out: they stand by design).

**Numbers (bot: the walk pilot; n52 levels; Logs/n56/g3-*.txt):**

| | foundry | iceline | kiln | span | spire | pooled |
|---|---|---|---|---|---|---|
| attack starts in the cone, off s1-10 | 62.3 | 45.2 | 43.0 | 44.4 | 41.7 | **46.5 %** |
| off s11-20 | 66.8 | 46.8 | 42.4 | 40.4 | 40.3 | 47.0 % |
| ON (waitS 4) s1-10 | 86.9 | 81.6 | 91.2 | 77.2 | 89.5 | **84.7 %** |
| ON (waitS 3) s1-10 / s11-20 | 85.7 / 84.1 | 80.0 / 79.2 | 86.9 / 86.3 | 80.2 / 77.0 | 85.9 / 78.9 | 83.0 / 81.0 % |
| deaths off s1-10 / s11-20 | 15 / 20 | 2 / 3 | 8 / 12 | 1 / 1 | 0 / 0 | |
| deaths ON waitS 4 s1-10 | 14 | 1 | 8 | 1 | 0 | |
| clear (sum of room medians) off / ON w4 | 130.2 / 144.8 | 233.2 / 219.9 | 92.6 / 97.1 | 95.3 / 100.9 | 141.0 / 142.8 | |

Span stays under 80 % (77.2): its spitters lob from the far rails once their wait ends. Iceline's out-of-view starts are swarmers
(flank pounces outside +-70 deg by design: swarmer.frontConeDeg).
Ranged hit rate (row off, s1-10, pooled): stand 32/230 = 13.9 %, run 102/1044 = 9.8 %, fast 9/165 = 5.5 %, dash 0/6: fast+dash = 38 % of
standing (<= 50 %: the shipped rule already meets it). The dash bucket alone has 6 shots in 50 walks: **not a number** (the walk pilot is
rarely at >= 18 m/s when a shot leaves); fast+dash is the measurable line.
Idle in place (pooled share of enemy-alive steps), off s1-10: 8.5 / 14.9 / 20.0 / 6.5 / 10.1 %; ON (w3) 7.3 / 16.9 / 18.8 / 6.7 / 10.8 %:
<= 20 % everywhere both ways; the idlers are drones (28-40 %: a ranged token holder keeps its ring) and wardens (28-31 %); the advance fired
4-74 times per 10 walks per level. Per enemy, 3.7-20.5 % of enemies idle over 20 % of their time (off and on alike).
Verdict (bot): cone >= 80 % pooled and on 4 of 5 levels (span 77.2 %); ranged fast+dash 38 % of standing (already shipped); idle <= 20 %
pooled on every level; deaths inside the off seed band (foundry 14 vs 15-20 is one under). Ships OFF; the lead's critic decides.
Reel: none of G3's is a Player frame (a sim rule); the a256 reel is G1's.

## G2 kill55's fix (NIGHT55's view row design.n55.kill55; the lead's n56-r1 a355 line: RED on input, "ladder NO")

**Cause, measured headless** (`n56 kestrel`, Harness.N56.Kill.cs: a355's chain on the sim, 2 steps a frame; kill55 is a view row, so this
measures when kills land, not the holds): the KESTREL's LIVE interval is 58 steps = 0.483 s (weapons.revolver.fireInterval 0.4 x
design.n39.p39.intervalMult 1.2), not the 0.4 s kill55's chainGapS 0.45 was set for. r1's tap ("a press when the cooldown reads 0")
lands kills 62 steps apart (0.517 s; the Player read 0.508-0.516) and kills 1 -> 3 take 1.033 s; no gap fits 0.45 (0 of 4).
**Conflict the lead should know:** a354's spacing (0.483-0.492 s) IS the KESTREL's own rate. Any chainGapS that lets the KESTREL chain
chains a354's kills too; "the KESTREL's real interval fits" and "a354 off a chain" cannot both hold. I chose the KESTREL (the doneWhen's
chain needs it: the level-1 player owns only the KESTREL); a354 measures move54 alone with -skygrindOff55 (it is not in tonight's reels).

**Fix (two parts):**
1. `design.n55.kill55.numbers.chainGapS` 0.45 -> 0.55 (n56-g-rows.cjs patches that key and its _why only). 0.55 chains the KESTREL at
   its rate and a tap a frame or two late (0.517 s), and stays under a355's spaced pass (40 frames, ~0.62 s of play).
2. a355's chain input (FeelReel.N55.Kill.cs): a press / release every other frame (a thumb hammering the trigger): the release inside the
   interval is queued and fires on the step it ends, so kills land 58 steps apart: kills 1 -> 3 in **0.967 s** of play (headless, foundry
   and iceline), gaps inside 0.55: 4 of 4. Also: 20 frames recorded after the chain's last kill (r1's kill 6 read "held 0 frames" on both
   sides: the loop ended on its frame), and a new GREEN line "3 in 1 s" (kills 1 -> 3 <= 1 s of play, from kill55's probe).
N55KillTests' config test now pins chainGapS >= the KESTREL's live interval (58 steps) and < a355's spaced gap. The hold ladder, move54's
0.12 s cap, one hold per event: unchanged (Kill55.cs untouched).
**The r1 critic's Look line (the "pale violet square" on hit-next):** it is in the OFF frame too (k1off hit-next, same place, my read of
both crops): NIGHT54's hit flash, not kill55's mark. kill55's marks barely show: on r1's hit-before / hit-after the changed body pixels
that are dark at +0.5 s are 0.33 % ON vs 0.02 % OFF (and "changed at +0.5 s" 7.8 % ON vs 6.4 % OFF: the old check passed OFF too).
Slice 2: the marks x1.5 (scorch 0.165, crack 0.36 x 0.06, hole 0.21 m) and 4.5 cm off the shell (was 1.6 cm; a part of the model over
ShellR hiding the crack is a guess, not checked); a355's damage check now needs the MARK pixels (changed and V < 0.12 at +0.5 s) >= 1 %
and prints the changed pixels' median hue and the marks' own colours (crack #160807 H 8, hole #070303, hot #FF5A1E H 17 cooling to
#5A160A: the enemy family). Not seen by me in a Player: the lead's reel decides.
**Attempt 2 (after the r2 critic: chain counts now, kills 3-6 held 4 frames = 0.067 s; mark pixels 0.12 %, the changed pixels H 3 S 0.99
V 0.55; a354 with kill55 on jitter 0.212 vs 0.191 off, held 0.183 vs 0.150 s):** chainS 0.08 -> 0.09 (5 whole frames at 60 fps; under the
0.12 s cap; one hold per event unchanged); the marks' hot core now cools in 0.35 s (was 1.2) to a near-black ember 0x1A0604 (V 0.10, was
0x5A160A V 0.35), so at +0.5 s the mark is dark, not the glowing red r2 measured. **a354's jitter cannot come back to its OFF value with the
ladder on:** a354's kills are the KESTREL at its own live rate (0.483 s) while running, the same spacing a355's chain needs; any ladder that
lengthens a KESTREL chain lengthens a354's holds (held 0.150 -> 0.183 s) and its jitter with them. The alternatives: a354 judged with
-skygrindOff55 (move54 alone), or a ladder that skips kills while running (a new rule: not built).
**Reels for the lead:** `bash Tools/night/n56-reels.sh <tag> k2on,n52-foundry,a355` (the copy's config with kill55 on: want GREEN, "3 in 1 s
ok", "ladder ok") and `k2off,n52-foundry,a355,-skygrindOff55` (RED: every kill 0.050 s). Frames: shot-N55-K-chain.jpg (on / off), the
hit-next frames. Then the blind pair (n46-blind-pairs.cjs, both orders) on the chain frames.

## G1 rank56 (sim row design.n56.rank56): measured SHORT on its first line

**What was built.** The style rank (StyleMeter.rank, the rank SHOWN when the kill lands: the same index Economy reads for
points.rankMult) multiplies heal36's close-kill heal and RIDE KILL's refund unit. Numbers: `design.n56.rank56.numbers.mult`
= [1, 1.2, 1.4, 1.6, 1.8, 2] (DULL .. SKYGRIND; heal 10 / 12 / 14 / 16 / 18 / 20 HP; one ride-kill boost 0.5 .. 1.0 of the bar).
Files: Core/Rank56.cs, SurvivalSim.N56.cs (the night's hooks), SurvivalSim.N56.Rank.cs, N56RankTests (3/3 on the harness's mini NUnit).
Call-site lines in shared files: Heal36.OnKill gained `double scale = 1` (amount x 1 is the same double), SurvivalSim.Core36.cs's heal36
handler passes `Rank56HealScale()`, SurvivalSim.N53.RideKill.cs's refund passes `unit * Rank56RefundScale()`. Off = x1 exactly.

**The meter.** n46flow has a style column (Harness.N56.Style.cs, three call-site lines in Harness.N46.Flow.cs): per room the share of
in-fight steps at each shown rank, the top rank per seed (median / max), and a PAYOFFS line (heal36 HP per close kill and RIDE KILL
refund per ride kill, by the rank shown when each landed). The column only reads: the flow numbers with it equal the lead's
Logs/n56/flow-base.txt line for line (checked: every SUMMARY line identical).

**Before (row off, u3-n55 + the shared commit; `n46flow --level n52-foundry,n52-iceline,n52-kiln,n52-span,n52-spire --seeds 10`,
Logs/n56/g1-style-base.txt), bot:**

| level | DULL % of fight | fight s | rooms reaching SAVAGE | top rank |
|---|---|---|---|---|
| foundry | 56.0 | 1340 | 3/6 | SKYGRIND |
| iceline | 69.6 | 2576 | 0/8 | AIRBORNE |
| kiln | 31.9 | 966 | 1/3 | SAVAGE |
| span | 11.4 | 955 | 2/3 | SKYGRIND |
| spire | 10.5 | 1407 | 3/3 | SKYGRIND |
| all (time-weighted) | **42.9** | 7244 | **9/23** | |

The "~100 % DULL" of the verbs37 reels is not the combat number: in fights the walk pilot spends 43 % of the time at DULL (iceline g1
100 %, span p3 0 %: the meter carries between rooms). "A combat room reaches SAVAGE" is already true with the row off (9 of 23 rooms).

**After (row on, `--rows rank56`, Logs/n56/g1-style-on.txt), bot:** DULL foundry 56.0, iceline 71.1, kiln 30.7, span 10.7, spire 10.5;
all **43.0 %** (+0.1 points); rooms reaching SAVAGE 9/23; deaths foundry 15 -> 15, iceline 2 -> 3, kiln 8 -> 8, span 1 -> 0, spire 0 -> 0.
PAYOFFS (on): heal per close kill foundry DULL 9.3 HP (31 kills), COOL 10.0 (18), BOLD 10.8 (7), AIRBORNE 15.0 (2), SAVAGE 17.0 (1);
span SAVAGE 15.5 (21), SKYGRIND 12.3 (26) (the heal is capped at the health max, so the mean is under 10 x mult). RIDE KILL refund
per ride kill: 0.00 at every rank on every level (153 ride kills): the kill's own refill (design.stamina.rewards) has already filled the
bar when the ride kill pays, so the refund (and its rank scale) gives the walk pilot nothing.

**Verdict: SHORT on "DULL falls >= 30 points" (42.9 -> 43.0 %, bot).** The row scales PAYOFFS, not style GAIN: nothing in it moves the
meter, and the walk pilot does not play for the heal. The honest number is +0.1 points; no bent meter. What the row does do, measured:
the heal at AIRBORNE / SAVAGE is 1.6-1.8x DULL's in the PAYOFFS line, the off row reads x1 at every rank (N56RankTests). A sharper
contract that this row can meet: "(bot) heal per close kill at AIRBORNE or above >= 1.5x DULL's; with the row off x1; the a256 frame
shows the heal at the rank shown". The DULL share needs a style-GAIN rule (gameplay.md P1's movement multiplier or a hit costing a
rank), which the brief rules out tonight (GG 2026-10-20 "don't change the core mechanics"): an idea for docs/memory/IDEAS.md, not code.
StyleSim's golden records: CoreHarness golden 0 failing, 0 mismatches (StyleSim never reads the row).

**a256 (FeelReel.N56.Play.cs, written, not run by me):** the campaign, a non-boss room's spawn, the meter put at DULL, 7 husks (1 hp) on
a 3 m arc (close kills: heal36's radius is 4 m), killed at the KESTREL's own rate, the health held at 60 before each kill. Prints each
kill's rank and heal, the frame the rank left DULL with husks alive (shot-N56-G-rank.jpg), the heal at DULL vs the top rank reached and
rank56's multiplier. GREEN on: left DULL + heal ratio = numbers.mult[rank]. Reels: `bash Tools/night/n56-reels.sh <tag>
g1on,n52-foundry,a256` (the copy with design.n56.rank56 on) and `g1off,n52-foundry,a256,-skygrindOff56` (RED on "pays").
PLAY note for GG: rank56 ships OFF. On, a close kill at SKYGRIND heals 20 HP instead of 10; a ride kill's boost refund doubles but is
usually wasted (the kill already refilled the bar).
