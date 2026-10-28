# NIGHT57 builder G: notes (enemy pressure, kills)

Every number here is a bot number (n46flow's walk pilot: Walk38Pilot + the ported autopilot, "normal"; the n52 levels) unless it says
otherwise. Nothing here was run in the Editor or a Player (builders never do). Baselines: `Logs/n57/flow-base.txt` (the lead's A0, seeds 1-10,
every n56 / n57 row off) and mine beside it: `Logs/n57/g-base-s11.txt` (seeds 11-20, the seed band), `Logs/n57/g-base20.txt` (seeds 1-20).
Compare tool: `node Tools/night/n57-g-cmp.cjs <base> <base-s11|-> <on> ...`; runner: `bash Tools/night/n57-g-flow.sh <name> [args]` (`SEEDS=20`).

## G1 tokens56 (NIGHT56's sim row design.n56.tokens56): measured at HEAD, numbers unchanged

MAIN = attack starts inside the sim cone (bot). GUARDS = deaths per level in band; clear times <= +10 % vs flow-base.

| bot, n46flow, n52 levels | foundry | iceline | kiln | span | spire | pooled cone |
|---|---|---|---|---|---|---|
| cone, off s1-10 (flow-base) | 62.7 | 45.2 | 40.9 | 39.7 | 35.8 | **44.7 %** (slate: 44.6) |
| cone, ON s1-10 (`g1-tokens-on.txt`) | 85.4 | 84.0 | 90.0 | 78.6 | 86.7 | **85.0 %** |
| cone, off s1-20 / ON s1-20 | 61.1 / 85.4 | 44.4 / 83.7 | 43.0 / 91.4 | 38.2 / 79.2 | 36.1 / 85.5 | 44.4 / **85.0 %** |
| deaths off s1-10 / s11-20 (band) | 21 / 11 (+-10) | 9 / 4 (+-5) | 8 / 11 (+-3) | 0 / 1 (+-1) | 0 / 0 (0) | |
| deaths ON s1-10 | 13 | 7 | 8 | 0 | 0 | all in band |
| deaths off / ON s1-20 | 32 / 27 | 13 / 8 | 19 / 21 | 1 / 2 | 0 / 0 | all inside the s1-10 band |
| clear off s1-10 / s11-20 (s) | 122.6 / 116.0 | 220.3 / 239.2 | 92.2 / 87.0 | 86.8 / 92.3 | 135.1 / 157.8 | seed spread 5.4 / 8.6 / 5.6 / 6.3 / 16.8 % |
| clear ON s1-10 (s) | 134.1 (+9.4 %) | 245.2 (**+11.3 %**) | 92.5 (+0.3) | 88.7 (+2.2) | 149.0 (**+10.3 %**) | |
| clear off / ON s1-20 (s) | 116.5 / 124.7 (+7.0 %) | 233.3 / 237.3 (+1.7 %) | 91.7 / 94.7 (+3.3 %) | 89.0 / 90.5 (+1.7 %) | 140.2 / 141.8 (+1.1 %) | all <= +10 % |

- MAIN: 44.7 -> 85.0 % pooled (seeds 1-10 and 1-20 alike); the off band (s1-10 vs s11-20) is 44.7 vs 44.2 %: far beyond the noise.
- GUARD deaths: in band on every level (fewer on foundry / iceline).
- GUARD clear: at 10 seeds iceline +11.3 % and spire +10.3 % are over +10 %, but both are inside the baseline's own seed spread (the off run's
  s1-10 vs s11-20 gap: 8.6 % and 16.8 %); at 20 seeds every level is <= +7.0 %. The 10-seed clear is a sum of per-room medians and swings with
  the seed set (iceline g5 alone 32.8 -> 52.2 s at s1-10); I report both and read the 20-seed line as the measure.
- Probes (`--set`, never written): waitS 3 (`g1-p-w3.txt`): iceline +20.8 %, kiln deaths 15 (out of band): worse; waitS 2 (`g1-p-w2.txt`):
  cone 77.0 % pooled, kiln stall +8.2: worse; idleS 0.5 (`g1-p-i05.txt`, `g1-i05-20.txt`): no better at 20 seeds (iceline over-20 % idle 20.8 %).
  So tokens56 keeps NIGHT56's numbers; nothing was changed in code or config for G1.
- Per-enemy idle (a target, not a guard): share of enemies idle > 20 % of their time, off / ON s1-20: foundry 9.9 / 8.6, iceline 19.6 / 20.0,
  kiln 15.1 / 18.5, span 7.4 / 6.9, spire 8.9 / 8.8 %. Kiln's rise is the drones (idle 26.3 -> 31.4 % of their steps at s1-10): a ranged token
  holder keeps its ring by design (Advance56 skips holders). Not fixed: a near miss on a target, noted.
- RANGED (already shipping in the brain, AimMath.HitChance): with the row on, foundry run 18.4 % vs stand 33.3 % of spitter / drone shots hit.
- PLAY note (if ON): an enemy behind you waits up to 4 s before it may attack, so most attacks start where you can see them; an enemy left
  idle walks to a spot in view. Off switch: `design.n56.tokens56.enabled` false, or -skygrindOff56.
- No reel (a sim rule; NIGHT56's brief): the evidence is the bot lines above.

### G1's target, per-enemy idle (after r1; the lead's 11:2x ask: only if tokens56's MAIN / GUARDS hold)
- **Measured cause (`--g57` drone split, kiln, seeds 1-10):** with tokens56 on, of the drones' idle steps 28845 were WAITING (no token) and
  5995 holding it (off: 12855 / 15589); only 844 were on their 24 m wait ring. A waiting drone flies at its ring spot, which lies outside a
  kiln room's walls, and stalls against the wall out of view; ADVANCE (Advance56) only picks a loop node IN VIEW, so it never moved.
- **The rule (HuskBrain.N56.cs, behind a new number design.n56.tokens56.numbers.nearestIfNoneInView = 1; absent or 0 = NIGHT56's rule):** an
  idle waiter with no node in view walks to the NEAREST node (>= minM). Bit-exact check: tokens56 on with the key absent equals
  `g1-tokens-on.txt` line for line (`g1-tokens-on-recheck.txt`); golden 0 / 0, 22/22; fingerprints EQUAL.

| bot, n46flow, tokens56 on, seeds 1-20 | foundry | iceline | kiln | span | spire |
|---|---|---|---|---|---|
| enemies idle > 20 % of their time, off / as r1 / + nearest | 9.9 / 8.6 / **8.7** | 19.6 / 20.0 / **19.3** | 15.1 / 18.5 / **15.6** | 7.4 / 6.9 / **7.5** | 8.9 / 8.8 / **8.9** |
| MAIN cone (pooled 44.4 / 85.0 / **84.9 %**) | 61.1 / 85.4 / 85.7 | 44.4 / 83.7 / 84.0 | 43.0 / 91.4 / 90.1 | 38.2 / 79.2 / 79.2 | 36.1 / 85.5 / 86.0 |
| deaths off / as r1 / + nearest | 32 / 27 / 28 | 13 / 8 / 12 | 19 / 21 / 19 | 1 / 2 / 2 | 0 / 0 / 0 |
| clear vs off | +7.0 / **+7.5 %** | +1.7 / **+7.8 %** | +3.3 / **+1.5 %** | +1.7 / **+1.7 %** | +1.1 / **+4.7 %** |

- At seeds 1-10 (`g1-near-on.txt`): idle over 20 % 8.3 / 18.4 / 15.8 / 9.8 / 7.3; cone 84.7 % pooled; deaths 13 / 6 / 8 / 0 / 0; clear +3.3 /
  +9.4 / -0.2 / +2.2 / +11.8 % (spire inside its 16.8 % seed spread; +4.7 % at 20 seeds).
- So: the MAIN and the GUARDS hold, every level is now under 20 % idle (kiln -2.9 points, iceline -0.7). The key ships at 1 in the config;
  tokens56 as gated at r1 is the same row with the key at 0.

## G2 press57 (new sim row design.n57.press57): built, OFF; MAIN missed (bot)

**What it is (Core/Press57.cs, SurvivalSim.N57.Press.cs; one call-site line in Core/Spitter.cs ThrowVolley).** ULTRAKILL's Virtue on the
SPITTER's existing volley: at its commit the spot under its target's feet is marked with the telegraph module's red-orange danger circle
(Telegraph20, "avoid this area, it hits when full"; numbers.markR 3 m = spitter.puddleRadius), filling over the tell (tells36's 1 s) + the
globs' flight to the spot (0.5-1.3 s): 1.5-2.3 s in all (E-4's >= 0.8 s). At the throw the volley flies to that spot: no lead, no deliberate
miss. (b) aim worse with speed already ships (AimMath.HitChance) and a marked volley never leads: nothing added. Campaign runs only.
The view needs no new code: the sim calls `TelegraphView.Circle` (Game's Telegraph20, already wired at the views' init); null in the harness.
Shared lines: Core/Spitter.cs ThrowVolley's miss `if` (one line: `!Press57Aim(e, ref at, ref time) &&` first; off: false at its first test).
SurvivalSim.N57.Press.cs implements Night57StartRun / Night57Step (empty partials in SurvivalSim.Rows.cs until now): a later NIGHT57 sim row
adds its call there.
Tests: N57PressTests 3/3 on the harness's mini NUnit (`n57g selftest`, Harness.N57.cs; the -p:Extra list in its header); dn-check-wt 0 errors.

**The bot's answer (Harness.N57.Press.cs; one line in Harness.N46.Flow.cs F1Input).** Without one the walk pilot never sees the floor. Fixed
BEFORE the first measure, never tuned after it: 0.3 s after a circle appears under its feet (a human's reaction to a visual cue), it steps
out of it, away from the centre, keeping its own look and trigger, until 1 m past the edge; the lava rule after it still wins.

**Numbers (bot, n46flow, n52 levels, seeds 1-10; `g2-press-on.txt`, `g2-press-tok.txt` with tokens56 too, row-off side `g2-base-meter.txt`
with `--g57`):**

| | foundry | iceline | kiln | span | spire |
|---|---|---|---|---|---|
| stall % off (flow-base) | 27.7 | 36.5 | 25.5 | 13.7 | 17.0 |
| stall % press57 | 28.3 (+0.6) | 36.5 (0.0) | **29.0 (+3.5)** | 14.0 | 15.6 |
| stall % press57 + tokens56 | 29.7 (+2.0) | 34.9 (-1.6) | 26.6 (+1.1) | 13.7 | 18.0 |
| deaths off (band) / press57 / + tokens56 | 21 (+-10) / 14 / 11 | 9 (+-5) / 1 / 5 | 8 (+-3) / **15** / 3 | 0 / 0 / 0 | 0 / 0 / 0 |
| clear off / press57 / + tokens56 (s) | 122.6 / 116.9 / 130.1 | 220.3 / 220.2 / 251.7 | 92.2 / 100.3 / 91.0 | 86.8 / 84.3 / 99.9 | 135.1 / 151.6 / 142.2 |
| spitter + acid HP per min MOVING, off / press57 | 80.3 / **25.9** | 27.1 / **16.7** | 15.4 / 18.4 | 28.6 / 22.0 | 14.5 / **4.4** |
| spitter + acid HP per min STILL, off / press57 | 63.6 / 33.6 | 28.0 / 16.3 | 42.5 / 48.4 | 12.9 / 39.8 | 10.7 / 9.4 |
| spots marked / volleys at a mark / circles answered | 231 / 215 / 194 | 220 / 208 / 205 | 122 / 114 / 109 | 197 / 187 / 145 | 83 / 81 / 63 |

- **MAIN (stall down >= 5 points on foundry / iceline / kiln): MISSED** on every one (+0.6 / 0.0 / +3.5; with tokens56 +2.0 / -1.6 / +1.1).
- GUARD "a moving player's hits taken not up": held, down on 4 of 5 levels (kiln 15.4 -> 18.4 HP per min moving: up, inside one seed set).
- GUARD deaths: kiln 8 -> 15 is out of its band (+-3) with press57 alone; with tokens56 too, 3 (in band). Clear: spire +12.2 % alone.
- **Why the stall does not move (measured, `g2-diag-base.txt`, `g2-diag-kiln.txt`):** of the walk pilot's still steps in fights (row off),
  the pilot's stick is PUSHING on 29589 of 40676 (73 %) on foundry, 42015 of 106315 (40 %) on iceline, 9046 of 28790 (31 %) on kiln: those
  stalls are the bot blocked (a wall, a crowd, a turn), not a choice to stand, and no enemy attack changes them. While it answers a circle
  the bot is still half the time (kiln: 3131 of 6683 answering steps under 2 m/s); a trace (`--trace`, seed 1) shows it pinned at a circle's
  centre with the stick held and speed 0 for 2 s. The answers do move it (194 circles, 96 s on foundry), but the walk pilot returns to the
  same stand right after: the stall share is the pilot's structure, which a pressure rule on one ranged kind cannot move by 5 points.
- What press57 does do (bot): spitters stop hurting a moving player (foundry 80.3 -> 25.9 HP per min moving) and still punish a still one;
  a spitter's ground circle shows where to leave on every volley. It needs a player's eye (a257's frames) and GG's play, not this meter.
- Shipped OFF by the rule (MAIN missed). PLAY note (if ever ON): a spitter's acid lands where you stood when its sac began to swell; a red
  circle on the floor shows the spot for 1.5-2.3 s; keep moving and it never hits you. Off switch: `design.n57.press57.enabled` false, or
  -skygrindOff57.
- **Reel a257** (FeelReel.N57.Play.cs, written, not run by me): `bash Tools/night/n47-reels.sh <tag> g2on,n52-foundry,a257` (the build copy
  with design.n57.press57 on: GREEN) and `g2off,n52-foundry,a257,-skygrindOff57` (RED: "marks laid 0"). Frames: shot-N57-G-tell.jpg (the
  circle part filled under the player, the spitter ahead), shot-N57-G-strike.jpg (the landing on a still player), shot-N57-G-moved.jpg (the
  landing after a strafe: the player out of the circle). Prints the circle life per mark (E-4 >= 0.8 s), the glob / acid hurts still vs moving.
- The lead (10:37): press57 stays OFF tonight by the rule; picked into r2 as an OFF row, the tell reeled for GG's PLAY note only.

## G3 kill55 (NIGHT55's view row design.n55.kill55): two fixes, not measured by me (a Player row)

MAIN (the lead, 09:5x: P0-calib FAILED, so the measured fallback of NIGHT57.md section 0 rule 5): hit marks >= 5 % of the body the NEXT frame
AND the chain hold ladder on a355. GUARD: a354 jitter <= 0.191 with kill55 on (NIGHT56 r5: 0.230; off 0.191).

**1. The jitter: the ladder does not extend a RUNNING chain** (Kill55.cs Kill55Hold.StepEnded's new `running` argument; SurvivalGame.N55K.cs
passes the player's horizontal speed >= numbers.runSpeed 4 m/s; config by Tools/night/n57-g-rows.cjs, an ADD of one key). Why: a354's jitter
is std / mean of the camera's per-frame distance over the expected on moving frames, and a frozen frame counts q = 0; with h the share of
held moving frames, jitter ~ sqrt(h / (1 - h)): 0.191 is h ~ 3.5 %, 0.230 is h ~ 5.0 %. a354's kills are the KESTREL at its own 0.483 s rate
while running, the same spacing a355's chain needs (NIGHT56 G.md), so every ladder hold in a354 is an extra held frame. With the gate a
running chain asks nothing beyond the impact rules' own freeze (move54's 0.05 s): a354's holds are NIGHT54's, so its jitter should read as
the off side's (expected, not measured: a Player reel). The chain still counts (shards per chain step unchanged); a355's chain stands (0 m/s)
and keeps 0.05 / 0.065 / 0.09 / 0.09 ... The record says "running: NIGHT54's"; a355 prints `running N` beside extended / held / skipped.
Test: N55KillTests.Running_TheChainCounts_NoExtraHold (compiles: dn-check-wt 0 errors; the mini NUnit cannot run this class, it uses
Within: the Editor's suite runs it).

**2. The marks: their colours on their own materials** (Kill55Damage.cs). n56-r5's a355 crops (k5on hit-before / hit-after, the victim husk;
my read): after the hit a jagged patch with strips sits at the crosshair, the hole's shape, but in a dark RED close to the body's own hue,
never the near-black #070303 / #160807 the property blocks set; NIGHT56 measured 0.04 % dark mark pixels. The cause is not proven (I cannot
run a Player); the fix removes the dependency: each mark colour is a copy of EnemyLook's body material made once at the first draw (dark,
hole, and Kill55MarkHotSteps 6 cooling steps hot -> ember), its own _BaseColor / _Flash 0 / _RimOn 0 / _Dissolve 1, and its ink pass
(UniversalForward, NIGHT51's inverted hull) off. No size change (medium juice, research lesson 3): the hole (0.21 m) covers ~12 % of a husk's
body 5 m ahead if it shows in full; the crack's scorch and strips ~10 %.

**3. a355's damage line measures the MAIN:** the next frame's mark pixels (body pixels warm before the hit that changed and are dark, V < 0.12,
on the hit's next frame, where the hit flash whitens the rest of the body) >= Kill55ReelMarkNextShare 5 %; GREEN on it, the +0.5 s numbers
printed as NIGHT56's (not required).

**Reels for the lead** (the build copy with design.n55.kill55 on):
- `bash Tools/night/n47-reels.sh <tag> k3on,n52-foundry,a355` (want GREEN: "ladder ok", "MARKS NEXT FRAME >= 5 %", `running 0`) and
  `k3off,n52-foundry,a355,-skygrindOff55` (RED: no marks, ladder NO).
- `bash Tools/night/n47-reels.sh <tag> m3on,n52-foundry,a354` with kill55 on (want jitter <= 0.191; `running N` kills skipped by the gate in
  kill55's records if printed) beside `m3off,n52-foundry,a354,-skygrindOff55` (the off jitter on the same build: the band).
- Frames for the critic: shot-N55-K-hit-next.jpg (the white flashed husk with a dark hole / crack at the hit), shot-N55-K-hit-after.jpg
  (+0.5 s: the dark mark on the red body), shot-N55-K-chain.jpg (the chain's 3rd kill held).
- PLAY note (if ON): kills in a quick chain while you stand or strafe slowly hold a little longer each (0.05 -> 0.065 -> 0.09 s); running
  chains keep the smooth camera; a hit husk shows a burnt crack, a hole under half health. Off: design.n55.kill55.enabled false / -skygrindOff55.

## G4 wick55 (NIGHT55's sim row design.n55.wick55): re-measured at HEAD, numbers unchanged; deaths inside wick54's own seed band

MAIN = damage dealt in the windows (n47human's "dealt in windows", 6 % baseline). GUARD = deaths in wick54's band. The NIGHT56 gate compared
seeds 1-40 only (deaths 2 -> 7). The same off fight on seeds 41-80 has 9 deaths: wick54's own band is 2-9.

| bot, n47human --boss wick (Wick37Pilot skilled, 100 hp), 40 seeds | off s1-40 | off s41-80 | ON s1-40 | ON s41-80 |
|---|---|---|---|---|
| dealt in windows (median) | 6 % | 6 % | **57 %** | **57 %** |
| deaths (total) | 2 | 9 | 7 | 4 |
| kill time median / p90 (s) | 45.5 / 53.7 | 47.6 / 60.6 | 57.5 / 62.7 | 54.6 / 63.0 |
| damage taken (median HP) | 72 | 85 | 60 | 56 |

- Pooled over 80 seeds: deaths 11 off vs 11 ON; windows 6 -> 57 %; damage taken down. The casual pilot (`n55wick`, s1-40): deaths 15 -> 8,
  windows 4 -> 57 %, kill median 52.6 -> 60.9 s (`g4-wick-casual-off.txt` / `-on.txt`).
- By the rule wick55 passes its MAIN and its guard (deaths inside the off run's own seed spread; pooled equal). The kill median stays inside
  the bot band of DECIDE39 D-1 (30-60 s) on both seed sets.
- Not fixed: T-59 (the Player's a455 FIGHT MODE pilot cannot climb n52-foundry's stack, so no core punch lands in the Player reel): a pilot
  job, not the fight's (NIGHT56 G.md). The slate's "or" made the deaths line enough.
- PLAY note (if ON): THE WICK's damage mostly lands when you parry its yellow tell and punish the 4 s window (a hit there lands x2, a hit
  outside x0.3); brushing the hot wall in the air pushes you off without a burn. Off: design.n55.wick55.enabled false / -skygrindOff55.
- Logs: Logs/n57/g4-wick-off.txt, g4-wick-off-s41.txt, g4-wick-on.txt, g4-wick-on-s41.txt, g4-wick-casual-off.txt, g4-wick-casual-on.txt.

## G4 rank56 (NIGHT56's sim row design.n56.rank56): T-137 explained and fixed; the MAIN cannot move (retire it)

**T-137, the cause (read, not run in the Editor):** N36SharedTests.cs declares `[SetUpFixture] N36TestBaseline` in the namespace
Skygrind.Game.Tests, and its [OneTimeSetUp] sets `N36Blocks.ForceOff = true` for EVERY test in that namespace (N27-N37's fixtures do the
same for their nights). N56RankTests lives in that namespace, so in the Editor heal36 was off and the close kill healed 0: "Expected: 10.0d
But was: 0.0d". RowOn "passed" only because 0 x mult = 0. The harness's mini NUnit runs no SetUpFixture, which is why NIGHT56 saw 3/3 there.
**Fix:** N56RankTests.Run starts its run with `N36Blocks.ForceOff = false` (saved and restored around StartRun: the run keeps the config's
blocks) and asserts heal36 is on. The same trap holds for any test in Skygrind.Game.Tests that needs a NIGHT27-37 block on (noted for the
lead, not swept).
**Reproduced headless:** `n57g selftest N56RankTests --fixture36` (the flag sets N36Blocks.ForceOff as the fixture does) on NIGHT56's test:
"FAIL RowOff_TheHealAndTheRefund_AreTheSameAtEveryRank: expected 10 but was 0 (heal36's amount)", 2 of 3 (the Editor's line); on the fixed
test: 3 of 3, with N57PressTests 3/3 and N56TokensTests 2/2 under the same flag.

**The MAIN (bot, n46flow, `--rows rank56`, `g4-rank-on.txt` vs flow-base):** DULL share foundry 50.5 -> 50.6, iceline 67.8 -> 67.4, kiln 25.9
-> 26.0, span 13.2 -> 13.2, spire 12.0 -> 12.0 %: unmoved, as NIGHT56 measured. The RIDE KILL refund still pays 0.00 at every rank (the kill's
own refill has filled the bar). rank56 scales PAYOFFS, not style GAIN: no number it owns can move the DULL share. **Recommendation: retire**
(the row stays in the config, off; its test now honest). A style-gain rule (the research's air / slide multiplier) is an idea for
docs/memory/IDEAS.md, not tonight's code.
The lead (11:2x): wick55 ON pending the r2 critic's evidence check; rank56 RETIRED.

## The T-137 sweep (the lead's ask, 11:2x; read-only, no test edited)

**The trap, all of it:** every [SetUpFixture] that turns blocks off lives in `Assets/Tests/EditMode/Game/` and in the namespace
Skygrind.Game.Tests (N27, N29, N30, N31, N32, N33, N34, N35, N36, N37 SharedTests; grep of "SetUpFixture"). Each one's [OneTimeSetUp] sets its own
night's `NnnBlocks.ForceOff = true` for EVERY test of the namespace; N37's also sets N38-N43 blocks' ForceOff and
`NightRows.ForceOffFrom = NightRows.FirstNight` (46). So in the Editor every Skygrind.Game.Tests test runs with every NIGHT27-43 block off,
every NIGHT46+ row's DEFAULT off (`rowsRegistry.Defaults()`, `NightRows.ViewOn`), unless the test resets the switch or hands an explicit
override (`rowsOverride = Defaults().Set(row, true)`, `n36Override`, ...; `Resolve` returns an explicit override untouched). The mini NUnit
runs no fixture, so a headless pass proves nothing about this. This is also what T-126 called "an earlier test leaves 46" (N53RideKillTests,
N54MoveTests): the N37 fixture sets it for the whole namespace.

**The sweep (173 files in the namespace; greps of the files that build or start a SurvivalSim, then of every NIGHT27-37 block's feature
names: heal36 / parry36 / tells36 / launch36 / crunch36 / tap36 / wallgrip36 / walkin36 / boss36, wick37 / stag37 / loom37 / crown37 /
boss37, charge35 / wallride35 / grapple35 / teach35, bend34 / flow34 / air34 / combo34 / reach34, the N33 / N32 / N31 / N30 / N29 / N27 block
fields, `.n27.` - `.n37.` config paths, `ViewOn(` / `.On(cfg)` / `.Live(`):**
- Tests whose SUBJECT needs a NIGHT27-37 block ON and did not reset it: **N56RankTests only** (heal36; fixed tonight, ab3f973b).
- Sim-running tests written after NIGHT37 (N53RideKillTests, N56TokensTests, N56RankTests, N57PressTests): N53RideKillTests resets
  ForceOffFrom and hands rideKill53 by override (its refund reads no NIGHT27-43 block: not checked further than its source); N56TokensTests
  and N57PressTests hand their row by override and assert nothing a NIGHT27-43 block gates (N57PressTests passes its tell length in; under the
  fixture the spitter's tell is the web's 0.6 s, which no assertion reads). Under `--fixture36` both pass headless (3/3, 2/2).
- N38-N43SharedTests reference NIGHT36 / 37 features and set the older switches to false themselves (try / finally): safe.
- The other matches (Hud30Tests, Hud33LayoutTests, HudWeapons30Tests, Level5Tests, Look30Tests, Look35Tests, MessageSlot33Tests,
  Rail34LayoutTests, Text36LayoutTests, NightRowsTapeTests, N52UTests) read config numbers or build synthetic configs; no block switch is
  read: safe.
- `ViewOn` / `.Live(` in a Game test: N52KillWhiteTests and N53PolishTests (both reset ForceOffFrom in try / finally), Hud17Tests (an
  unrelated StyleChip.Live): safe.
- Not covered by a grep: a test whose subject depends on a gated feature without naming it (the way N56RankTests named only Heal36's event).
  The pre-NIGHT38 sim tests (FingerprintTests, RunRestoreTests, AutopilotTruthTests, FarmTests, ...) were written under these fixtures and
  pin the forced-off game on purpose (not re-read one by one).
- One-line guard for a future test that needs a block on: start its run with the switch saved, set false, restored (N56RankTests.Run's
  pattern), and assert the block is on, so a vacuous pass cannot hide.
