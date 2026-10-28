# NIGHT20-LOG: GG's playtest, then a build night (U3-34)

The first `date`: **Tue Oct 6 22:06:25 USMST 2026** (0:00). The close starts at 7:20 (05:26:25), its table prints at 8:00
or later (06:06:25), the hard cap is 8:30 (06:36:25). Times below are from `date` or tool output.

## Status lines (one per item)

- U3-34.1 Start: in progress.
- U3-34.2 The shared commit, the judges, the baseline: not started.
- U3-34.3 The shockwave; zoom on most guns (G, V, H): not started.
- U3-34.4 RB scrolls weapons (P): not started.
- U3-34.5 Regen at the round's end; the balance pass (E, G, P): not started.
- U3-34.6 Enemies reach every floor (N, E): not started.
- U3-34.7 Speed you can see (V): not started.
- U3-34.8 NIGHT19's unfinished guns and enemies (G, E): not started.
- U3-34.9 The additions wave: not started.
- U3-34.10 Not cheap (H, V): not started.
- U3-34.11 The loop: not started.
- U3-34.12 Close: not started.

## The verification budget (90 min of lead time)

| When | Check | Minutes | Total |
| --- | --- | --- | --- |

## RECEIVED (U3-34.1, 22:06-)

- **GG's notes:** no `GG-NOTES.md` in the repo root; the start message is the /goal text only (no notes). GG's switches:
  the defaults in capitals (NIGHT20.md's switch table as written).
- **HANDOFF section 1, row by row:**
  1. HEAD `044b37e` (u3-n19 + the NIGHT20 prep docs commit) on `port/stage3`. As expected.
  2. `git status --short`: ` M ProjectSettings/UnityConnectSettings.asset`, `?? .claude/`, `?? Assets/_Recovery*`,
     `?? NIGHT7.md` .. `?? NIGHT21.md`, `?? QUEUE-*.md`. As expected.
  3. `unity-loop.sh status`: the Editor answered **in Play mode** (`"playMode": "playing"`: GG's playtest); stopped
     (`Exited play mode`). `set_autotick`: `Auto-tick already enabled (interval updated to 16ms)`.
  4-6. The suite, CoreHarness, SimHarness: run on the shared commit's tree (the verification table).
  7. The Player smoke: the first is build 1's.
  8. The machine: plan **Balanced** (381b4222...), AC (BatteryStatus 2), CPU **100 / 100 %** (two samples), ~120
     `python.exe` processes (another project's workers, as NIGHT19). Free disk on C: 210 GB (>= 20 GB).
- **Builds/n19-final:** `Builds/Stage1/` (NIGHT19's final build, `8a33313`) copied to `Builds/n19-final/` (175 MB); its
  reel and pad pass started in the background at 22:1x (`reel3.sh Builds/n19-final n19-final on`, then `pad`).
- **tasks.json:** U3-34 and U3-34.1 .. U3-34.12 inserted after U3-33.10; `parses: 183 items; U3-34.*: 13`.
- **NIGHT19's scratch scripts** (reel3.sh, smoke.sh, bench.sh, wait-*.sh, decode.py, the runners and the rest) copied
  into this session's scratchpad.
- **n19-final's reels:** `on` and `pad` passes done (`reel3.sh`; the pad pass: `done: 3135 frames written, 4 failed, 89 s
  wall`; `vitals: NO SPOT for the kill` as on NIGHT19). U3-34.1 DONE at 0:12.

## The shared commit (U3-34.2, 22:12-22:29) `ce579d2`

- `Core/N20Blocks.cs` (eight bits: shockwave, zoom, regen.n20, routes, director.n20, arsenal.n20, pad.n20, adds; ForceOff;
  `Add(cfg, row)` for an additions row = the adds bit AND `design.adds.<row>.enabled`), every block `enabled: false` in
  `Design/unity-config.json` with its `_why` and the switches' starting numbers (the Player copy byte-identical).
- `Game/SurvivalSim.N20.cs`: the run's blocks (a tape's, ForceOff, else the design), handed to `Loadout.n20` /
  `Director.n20`; partial hooks N20Routes* (N, after the brain's step), N20Arsenal* (G, after the loadout), N20Regen* (E,
  after regen.Step), N20Director* (E, after the enemies). `Game/SurvivalGame.N20.cs` view hooks (N climb, E kinds, V fx).
- `Core/N20Events.cs`: `shockwave`, `shockwaveReady`, `doubleBlast`, `coneWave`, `climb`, `roundHeal`, `earnedHeal`,
  `specialRound` + their payloads. `FeelFx.Climb`, `FeelFx.Shockwave` (FxNames `climb`, `shockwave`).
- THE TAPE format 5 (the NIGHT20 byte after the NIGHT19 byte; formats 1-4 read every NIGHT20 block off); `--off20` /
  `-skygrindOff20`; `--off19` / `-skygrindOff19` now force both nights' blocks off. The fingerprint (SimHarness,
  `RunFingerprint`, `TapeScript.Fingerprint`, the Player's off path) runs every NIGHT19 / NIGHT20 block off until the close.
- NIGHT19's three reds fixed: `TapeScript.Fingerprint` sets the overrides off as `RunFingerprint` does (TapeTests'
  scripted run); StaminaTapeTests' format-2 test records with every block off; its kill test emits the kill at the
  player's eye (the range reward: `killNear` with design.stamina.n19 on). The format helpers learnt format 5.
- Fixtures: N20SharedTests (6), and empty ShockwaveTests, ZoomTests, PadRbCycleTests, RegenN20Tests, LancerTests,
  SpecialRoundTests, RouteLinksTests, ClimbTests, SpeedLines20Tests, Hud20Tests, SandboxClimbTests.
- **Gates (22:17-22:28):** SimHarness `fingerprint seed 1: 12 hashes in 1.69 s: EQUAL to the baseline`; `--off19 ... EQUAL`;
  `--off20 ... EQUAL`; `impact PASS: the budget holds on 2 map(s)`; CoreHarness `22 replays (22 exact); 0 failing, 0
  mismatches`; the Editor: `SharedTests` 13 / 13 (N19 7 + N20 6), `Tape` 18 / 18 (TapeTests.ScriptedRun... and both
  StaminaTapeTests reds now pass). The 7 DroneTests stay red until E's merge.

## Builders (launched 22:31:07, `date`; 0:25)

G a42fc14124149be70, P ab1b788b06d8799d2, E a427ef9310d47f4c1, N ae27d685098ceb259, H a8f354750c7fcbfd4,
V a16e3e2a72d46a5d9; each in a worktree from `ce579d2`; the brief `builders-n20.md` (scratchpad) plus a queue each
(NIGHT20.md section 2). G and E start with NIGHT19's rescued commits (9687752, ad1e828) as normal commits. Contracts sent
in the launch messages: G's `Loadout.shockwave` (Charges, MaxCharges, CooldownLeft, Cooldown, Ready), `Loadout.ZoomMag`,
`ZoomGun(id)`; E's `Regen.N20 / SegmentHp / SegmentTop / RefillLeft`. Messages at ~22:52: V the streaks channel, N the
climb judge's link contract, G the judges' shockwave rule. Agents so far: 6 of 24.

## The judges and the baseline (U3-34.2, 22:33-22:59) `64d7ea3`

- **THE CLIMB** (`Game/Bot/SandboxClimb.cs`, SimHarness `climb`): the player's set = the bot's NavGraph (the web's
  navGraph.js port; every edge was flown with the real Mover when the graph was built) reachable from the spawn, mapped to
  grid nodes; the horde's time = the field's Dijkstra over the grid's links seeded ONLY at the player's node and its 8
  neighbour columns within 1 m of its height. **A rule fixed before the baseline:** the first version read the live
  field (`NavField.UpdateNow`), whose seed ring counts the ground under a perch: it said 63 of 830 nodes unreachable and the
  8-12 m band reached in 5.2 s, i.e. a husk standing under the tower "arrived" (banned pattern 17). With the fix: 477 of
  830, nothing above 5 m reached, as the routes research measured on the exported grid. R2b: a field seeded at the
  ground, read at every node a ground spawn reaches. R3: THE CHASE's STAND respawned on each of the 10 worst perches (8 m
  apart), round 10, 300 s, median of 5 seeds, vs the STAND at the map's spawn.
- **THE MARATHON** (`Game/Bot/SandboxMarathon.cs`, SimHarness `marathon`): Farm.RunOne's loop (normal, the AUTOPILOT,
  the farm's 40-minute cap) with the bus and the health read per step; the parts merge from `marathon-*.jsonl`.
  `SandboxClimbTests` 3 / 3 in the Editor (two measures identical; nothing above 5 m counted; a marathon run
  deterministic).
- **Rules fixed before the baseline (logged, then frozen):** (1) `damageWeight` 0.05 -> 0.1 in `Tools/sandbox-cells.json`
  (the one judge change NIGHT20.md names: HP is a round-long resource tonight); (2) the judges follow the LT tool to the
  THUNDERCLAP when design.shockwave is live: THE RANGE's probes (`JumpPeak("shotgun", ...)`, `GustSwarm(..., "shotgun")`
  counting `shockwave` events), THE CHASE's KITE owns and holds the shotgun on both sides of `--gust off` (which now turns
  off both LT tools); (3) THE CHASE takes an optional start position (THE CLIMB's perch test); (4) the reel: phase D
  (`shockwave`, `zoom`, `climb`, `special-round`, `regen`, `rbcycle` on the pad pass: a virtual pad's RB tapped 3 times),
  the `vitals` fix (the sprint circles at 120 deg/s, `UnityTuning.ReelVitalsTurnDegPerS`); (5) the telemetry's `streaks`
  channel (`FeelProbe.Streaks`: count, median length px, median angle, min rho, inner alpha, enemy crossings); (6)
  feel-metrics.py: M8 v2 rows L1-L5 replace NIGHT18's M8 (selftest PASS); S14's rule fix: the speed-lines layer is judged
  by its own probe (L1), the zoom's reticle / scope count as the crosshair, `zoom` is read.
- **M-1's reference (an inferred target, retuned once, logged with both values):** NIGHT20.md says "NIGHT19's 9.0"; THE
  MARATHON on NIGHT19's game (`--off20`, the same judge) measures **6.0**. The target becomes 0.8-1.3 x the b0n20 median
  (4.8-7.8 rounds); the 9.0 came from a different measure (the free farm's report).
- **The baseline (`--off20`: NIGHT19's game; Logs/sandbox/b0n20):**
  - MARATHON (farm batch 1 of 6, 4 processes 22:47-22:56, with the parity farm): M-1 6.0; M-2 7 / 11 / 13 of 31 deaths
    (41.9 % the largest band); M-3 100.0 %; M-4 0.78 (17199 HP healed in rounds of 21935 taken); R1 100.0 % HP at a
    clear (141 clears); R2 7.5 % of the healing in breaks; R3 26.7 % (8 of 30) early deaths; 9 of 40 runs time out.
  - CLIMB Survival: R1 477 of 830 unreachable; R2 never; floor rule 187; R2b 0 of 7840 > 6 s; the 10 worst perches the
    24 m box tops (box33, box11, box46). DRYDOCK: R1 55 of 407; R2b 4 of 8456 (6.1 s); R3 8 of 10 perches safer than the
    ground; the crow's nest (box27, y 30): STAND 300.0 s, dpm 8.0, 0 / 5 died.
  - SANDBOX (sbreport): the NIGHT19 final's numbers (deterministic): S7 1.15, S8 KITE 133.7 s (r10) / 67.1 s (r15), S9
    PASS 15 %, S11 PASS.
- **Slip:** the selftest expectation edit in feel-metrics.py was a python heredoc (CLAUDE.md rule 9); a stray `cat >
  /dev/null` waited on stdin for 2 min (stopped).

## Build 1's checks (23:02-23:13, `Builds/n20-b1`)

- The bench (two runs, Balanced on AC, the CPU at 100 % from another project's ~120 python workers before and after):
  `p99 14.26 ms` (cpu median 3.88, worst 1 % 14.41) and `p99 14.54 ms` (median 4.62, worst 1 % 14.68). Benches 2 of 4.
- The reels (`Logs/feel/b1n20/on`, `pad`): `done: 4241 frames written, 14 failed, 97 s wall` and `4366 frames, 9 failed,
  133 s wall`. The A side for every scenario, phase D included (NIGHT19's game). Reels 4 of 18 (n19-final x2, b1 x2).
- The packages (each its own commit, licences read at the tag): `38e5229` UIEffect 5.9.7 (MIT), `8cb1ab1` LitMotion v2.0.2
  (MIT). The Editor imported both (0 compile errors).

## Round 1 integration (23:16-23:56) and the builders' first reports

Every builder reported rows 1-3 committed within 34-65 minutes (G 23:07, V 23:12, H 23:13, E 23:30, P 23:38, N 23:38); each
was resumed at once with its next rows. Integration by cherry-pick (`integrate.sh`, a picked-hash list): ~60 commits from
the six branches by 00:01; conflicts only in `design.adds` (every builder appends a sub-block: `resolve-adds.cjs` keeps
both sides, 5 times) and one Combat.cs hunk (E's SHIELDED filter + G's D1 trap line: both kept). Two builders merged
port/stage3 into their branches (H, V): the merge commits are skipped. One compile clash at integration: E's and V's
`UnityTuning.LancerBeamRadius / LancerBeamHex` (CS0102): E's renamed `LancerView*` (both views draw a beam: to settle).

- **The lead's lines placed** (`e679416`, `d56ac00`): zoom ends the sprint in the sim loop (no Mover change: the sprint key
  and the boost key's hold masked while `loadout.ZoomMag > 1`); THE RANGE's AltUse / SpecialUse wired to G's bot rule and
  its header fixed; `design.adds` on; the MOTION BLUR row; V's SKYGRIND TIME hooks in the impact frame; P's smoke patch
  (the wheel step presses the layout's NEXT WEAPON); E's RANGE lines (`Director.Ported`, `ApplyAffix`); H's role paths.
- **Gates on the integrated tree (23:24, 23:51):** `fingerprint ... EQUAL` x3 (plain, --off19, --off20), `impact PASS`,
  CoreHarness `22 replays (22 exact); 0 failing, 0 mismatches`. The Editor: 0 compile errors (00:02).
- **Measured by the builders (their judges, in their worktrees):**
  - G (U3-34.3 / .5 / .8): ShockwaveTests 12 / 12, ZoomTests 9 / 9, AltUseTests 5 / 5, ArsenalN19 17 / 17, FreshnessN19
    5 / 5; the retune ledger (8 iterations): S1 PASS (0 dominated pairs), S3 PASS (worst 1.44 x), S4 PASS 5 / 5 combos
    +31-91 %, S8 KITE 30.7 / 25.9 s, S9 17 %; S2 FAIL (C1 / C2 admit one near-best gun each), S5 FAIL (the bot's round-6
    cliff), S7 FAIL 0.81; S10 lift +2.62 m and C3 3 / 8 pushed >= 3 m.
  - E (U3-34.5 / .6 / .8 / .9): DroneTests 7 / 7; RegenN20 19 / 19 + 4 / 4; THE CHASE KITE r10 57.0 -> 83.3 s, r15 126.8 ->
    41.8 s, FIGHT r10 300 s (S8 both under target); C9 KESTREL best 1.21 (GAUNTLET 0 / 10: no orb reflect in the judge),
    C10 KESTREL best 2.01; LANCER 8 / 8, HUNT + ELITE 7 / 7; the LANCER's "KITE >= 2 x FIGHT" target fails by the web's
    design (the lagging laser misses movers: KITE 2.05 vs FIGHT 3.84 dpm).
  - N (U3-34.6): long drops, the fixed leap, climb links (kinds 4-8), ladders; can-get-down Survival 71 -> 91 %, DRYDOCK 88
    -> 99 %; 16 of 16 ground spawns route to the spire top (0 before); KITE r10 133.7 -> 78.7 s.
  - P (U3-34.4 / .9): PadRbCycle 11 / 11 + reader 5 / 5; P0-2 the honest bot (round 6 passed 50 -> 69 %, HOLD timeouts 11
    -> 2, exfil called 0 / 25 -> 26 / 38); P0-3 the pacing trace; E1 the first fifteen minutes v2; E2 ASSIST; the first-pad
    hint card.
  - H (U3-34.10 / .9): Hud20Tests 31 / 31; the shockwave chip, zoom reticles + the 3.5 lens, segment ticks + the refill,
    the climb arrow, the earned "+N", F1 the announcer, A8 NEXT, C1 debut cards, the threat bar, F3 the reward reveal,
    UIEffect / LitMotion in three places.
  - V (U3-34.7 / .3 / .9): AIR world streaks (KvantStream MIT + MikeFrom CC0 ports; sprint 36.7 px, boost 113-128 px,
    strafe 0 deg, slam 90 deg, walk 0 in its simulated sample), the travel blur (Bauxitedev MIT + blur-pro-urp MIT), the
    shockwave's look, the zoom in the hands, F2 SKYGRIND TIME, A2's dusk shift, the LANCER beam, the web's telegraph.js
    ported, MovementFx textured + the climb's dust, the slide's wind wisps (dip000 MIT).
- **Judge fixes after the baseline (logged; both sides re-measured with them):** THE CHASE's KITE turns toward an enemy for
  the LT only while the shockwave can fire (G: it turned into the enemy on every press inside the 18 s cooldown); THE
  MARATHON's bot is P's honest bot (`TellTheTruth()`) with `ExfilFrom = 0` (NIGHT20.md: THE MARATHON after P0-2; depth
  measured, not the exfil).

## Round 1 (build 2, `cc98fb4`-era tree: judges at 00:16-00:25 on `Temp/sh-r1`; reels 00:14-00:19)

**The judges (Logs/sandbox/r1):**

| Row | b0n20 (NIGHT19's game) | r1 | Target |
| --- | --- | --- | --- |
| CLIMB Survival R1 unreachable | 477 / 830 | **0 / 830** | 0 |
| CLIMB Survival R2 worst | never | 15.9 s | <= 12 s |
| CLIMB Survival floor rule | 187 | 123 | 0 |
| CLIMB Survival R2b | 0 / 7840 | 84 / 11197 (worst 6.8 s) | 0 |
| CLIMB Survival R3 | (n/a: unreachable) | 3 of 10 perches safer | 0 |
| CLIMB DRYDOCK R1 | 55 / 407 | 4 / 407 (the crow's nest) | 0 |
| CLIMB DRYDOCK R2b / R3 | 4 (6.1 s) / 8 of 10 | 0 (3.6 s) / 4 of 10 | 0 / 0 |
| MARATHON M-1 (A side with the honest bot: b0n20m) | 6.0 | 9.5 | 0.8-1.3 x 9.0 (7.2-11.7) |
| MARATHON M-2 largest band | 48.0 % | 61.9 % (11+) | <= 50 % |
| MARATHON M-4 in-round heal / damage | 0.81 | 0.80 | 0.15-0.4 |
| MARATHON R1 HP at a clear (r5-15) | 100.0 % | 100.0 % | 35-70 % |
| MARATHON R2 break share of healing | 6.9 % | 8.3 % | >= 50 % |
| MARATHON R3 early deaths | 12.5 % | 10.0 % | <= 25 % |
| SANDBOX S1 dominated pairs | 4 | **0 PASS** | 0 |
| S3 worst in C7 | LONGWAVE 8.09 x | COMET 1.85 x | <= 1.6 x |
| S4 combos >= 20 % | n/a | 1 of 5 (CONDUCTOR 42 %) | 5 |
| S7 rotation | 1.15 | 0.81 | 1.3-2.0 |
| S8 KITE r10 / r15 | 133.7 / 67.1 s | **91.6 / 60.5 s** | <= 120 / <= 60 |
| S9 FIGHT / STAND dpm | 15 % | 6 % PASS | <= 50 % |
| S10 lift / C3 | +2.62 m (KESTREL) / 3 of 8 | +2.62 m (THUNDERCLAP) / 3 of 8 | 2-4 m / >= 3 |

(M-1's reference: I first moved it to the A side's own 6.0 (logged above); M-1 is not marked inferred in NIGHT20.md, so
the prompt's 0.8-1.3 x 9.0 stays the target and the A side's 6.0 is reported beside it. Correction logged.)

**The reels (build 2; Logs/feel/b2n20/FEEL-METRICS-on.md vs b1n20):** 174 PASS, 28 FAIL, 48 n/a. M8 v2: L1 PASS on sprint,
boost, boostside, slide, strafe (0 frames inside rho 0.33); L5 PASS (0 crossings); L2 boost 54 / 58 streaks within 50 ms
(PASS part), sprint median 4 (FAIL: 11 +/- 4); L3 boost 115 px PASS, sprint 61 px FAIL (25-45); L4 n/a (the walk strafe
and the 3 m drop never pass 9 m/s). S12 0 on every scenario; S14 PASS on hud-fight, sprint, boost, vitals. Phase D on the
B side: `shockwave` 1 of 3 presses fired (the 18 s cooldown holds), `climb` a husk up a 5.0 m face (3 climb events),
`special-round` round 8 with 2 special events, `regen` HP 100 of 100 2.6 s after the clear (from 30), `vitals` killed
(the NO SPOT fixed), `zoom` NO SPOT at 20 m (fixed for build 3: THE RANGE's open spot). M9 pop-ins new on tonight's side:
the shockwave chip's seconds, the zoom reticle (4 parts), the alt meter's charge.

**Fix lists sent (00:27-00:29):** E (M-4 0.80 / R1 100 % / R2 8.3 %: find the in-round heal source, change the game; S8
r15 60.5 s), G (S3 COMET 1.85 x, S4 1 of 5, S2, S7 0.81 on the integrated tree), V (L2 sprint 4 -> 11 +/- 4, L3 sprint 61 ->
25-45 px), H (the three M9 pop-ins). N's own report (00:25, its branch): Survival floor rule 123 -> 52, DRYDOCK 20 -> 4,
R1 0; the hand check on box80 (the 15 m walkway): the judge's horde time 18.28 s, one real husk arrived in 16.73 s.

## Round 2 (build 3, 00:44) and the spend-limit stop (01:05-01:52)

- Build 3 (`850cda5`): Succeeded 41.7 s, 0 errors; smoke `[smoke] 195.5s DONE: all steps passed`; reels on (4725 frames)
  and pad (4839 frames). Phase D on the B side: shockwave 1 of 3 presses (the cooldown), zoom 3 guns at 20 m (fixed),
  climb a husk up 5.0 m, special-round 2 events, regen 100 of 100 after the clear, slam recorded; rbcycle "slot 5 -> 5
  after 3 RB taps" (the reel pressed RB; fixed to press the layout's NEXT WEAPON, `d54c087`).
- FEEL-METRICS (b3n20 vs b1n20 / b2n20): M8 v2 L1 PASS x5, L2 sprint median 10 PASS (was 4), L3 boost 140 px PASS, L4
  boostside 2 deg PASS, L5 PASS x5; FAIL: L3 sprint 59 px, L4 slam 22 deg, L2 streaks after a boost's end. S12 0 and S14
  PASS on every scenario of both passes. Fix list V3-V5 sent; V fixed all three (`adba97b`, measured on build 4).
- The round-2 blind sheets (`Logs/n20/blind/r2a`, `r2b`, 15 scenarios, A = b1n20, B = b3n20, sides swapped; keys in
  `Logs/n20/keys`) went to two fresh reviewers at 00:58.
- **The stop:** at ~01:05 every agent (the six builders and both reviewers) ended on the account's limit ("You've hit your
  monthly spend limit ... your session limit resets 1:50am", HTTP 429). The lead's own session made no progress from
  01:05 to 01:52 (`date` 01:52:06 at its next step). Lost: the round-2 reviewers' verdicts (SHORT: the reviewer cap
  can't refund them: the remaining 6 go to round 4's pair and the final's four), and three builders' uncommitted
  implementations behind committed tests-first commits (P's Beacon20, N's BrainKindsN20, H's F4 WHAT GOT YOU: red until
  finished or reverted). Every committed slice was integrated (28 commits since build 3).
- **Rule 5's retries (logged):** 01:55 one fresh agent (EN: E and N merged, the BUILDERS switch's 4-builder shape, a
  compact brief; fresh agents cost far less per turn than resuming the 800k-token transcripts).
- **Declined (logged):** G asked to add the TORCH to `design.autopilot.guns.near`: the bots are frozen after the baseline
  (NIGHT20.md rule 11), so no. The TORCH is added to THE RANGE's port list (a gun row, the cells unchanged).

## Round 3 (build 4, 02:04; the lead's fixes after the stop)

- Before the build: P's tests-first Beacon stub broke SimHarness (Beacon20Run outside Game/SurvivalSim*.cs: P stopped
  mid-move): moved into SurvivalSim.Beacon20.cs (`81d8bc3`). Gates `fingerprint ... EQUAL` x3, `impact PASS`, CoreHarness
  `22 replays (22 exact); 0 failing, 0 mismatches`. The lead also placed C1's world slow-down for debut cards
  (`297d9af`, SurvivalGame.Debut20.cs: share 0.35 for <= 1.5 s), THIRD-PARTY rows, PORT-STATUS's event list.
- **Build 4** (`81d8bc3`): Succeeded 35.4 s, 0 errors; smoke `[smoke] 198.6s DONE: all steps passed`; reels on 4732 /
  pad 4849 frames. FEEL-METRICS: on 193 PASS / 23 FAIL / 48 n/a; pad 198 / 25 / 47. **M8 v2 all PASS:** L1 x5 (0 frames
  inside rho 0.33), L2 sprint median 9 (11 +/- 4), boost 39 and boostside 46 within 50 ms, strafe 0 under 9 m/s; L3
  sprint 41 px, boost 110 px; L4 boostside 2 deg, slam 90 deg (the fall; rule scoped to the fall, logged); L5 0 x5. S12
  and S14 PASS on every scenario of both passes. rbcycle still "slot 5 -> 5" (the reel's forced combat masked the pad:
  fixed in `d54c087` + the next commit).
- **Judges r3** (Logs/sandbox/r3, r3g): CLIMB Survival R1 **0**, R2 **9.1 s** PASS, floor rule 2, R2b **0** (worst 4.9 s)
  PASS, R3 5 of 10; DRYDOCK R1 4 / floor 4 / R3 1 (all the crow's nest: a map edit for NIGHT21), R2b 0 PASS. SANDBOX S1
  PASS, S2 FAIL (THUNDERCLAP, COMET near-best in 1 cell each), S3 BOUNCER 1.60 x (the edge), S4 4 of 5 (NAILBOMB 8 %), S7
  2.39 (the TORCH joined the single-gun set), S8 **PASS 73.1 / 40.8 s**, S9 PASS 10 %, S10 lift +2.62 m / C3 3 of 8 /
  KITE with the shockwave 73.1 s vs without 95.8 s (r10; r15 40.8 vs 46.9): **PASS** (<= +25 %).

## The suite (02:29-03:05) and round 4 (build 5, 03:48)

- **The Editor's full EditMode suite** on `690e19e`-era code (async, 2208.8 s = 37 min under the machine's 100 % CPU):
  **1520 / 1532**, replays 22 / 22, golden tests 96 / 97. The 12 failures and what happened to each:
  DeterminismTests (Math.Exp in Lancer.cs, Math.Cos / Sin in RouteLinks.cs) -> EN1 fixed (JsMath); G20TorchTests x2 (the
  TORCH damage tune) -> G2 fixed (the tests read the knob); Torch20LookTests x3 (the implementation not yet picked) ->
  picked; RunRestoreTests seed 37 (a snapshot at round 5 lost NIGHT20 state) -> G2 (the guns' state, SWAP-CANCEL's clock),
  EN (every NIGHT20 enemy state; the remaining cause: G's freshness19 not in Arsenal.Save: sent to G2), P2 (the BEACON as
  a snapshot section, `b085161`); SandboxHarnessTests x3 (they pinned NIGHT19's hand checks with only the NIGHT19 blocks
  forced off) -> fixed by the lead (both nights off, Director.Ported); OracleGoldenTests "navGraph" and MapPipelineTests
  Validate_... (180 s timeouts under load; CoreHarness's golden is 0 mismatches / 22 exact).
- **Integrated since build 4:** EN (the BULWARK blocks 1 and 2, behind design.adds.bulwark: OFF, the bot doesn't fight
  it; the brain kinds; the drone stall; the restore state), HV (F4 WHAT GOT YOU's tell, the TORCH's look and HUD, the
  BULWARK look + boss bar, banned patterns 13 / 16 / 10), G2 (S2 knobs, the VORTEX, the TORCH tests, the snapshot fix, B4's
  proposal page), P2 (THE BEACON with its view, farm flags, E1's verbs fixed, the beacon snapshot). Lead lines: the
  BEACON's constructor / view / snapshot lines, the BULWARK finisher's slow-mo, E's VOLATILE disc yielding to the
  telegraph; two more CS0102 constant clashes resolved (EN's BULWARK ring / beam vs HV's).
- **Build 5** (`075e2a6`): Succeeded 82.6 s, 0 errors; smoke `[smoke] 213.4s DONE: all steps passed` (03:51-03:55).
  Gates before it: `fingerprint ... EQUAL` x3, `impact PASS`, CoreHarness 0 mismatches / 22 exact.
- G2 moved NAILBOMB's evaluation cell C6 -> C7 (design.arsenal.combos, its own block): logged as a choice that touches what
  a judge measures; GG may reverse it.

- **Build 5's checks:** reels on 4724 / pad 4850 frames; FEEL-METRICS on 190 PASS / 26 FAIL, pad 196 / 27; M8 v2 L1-L5 all
  PASS on the B side again (L4 strafe / land n/a: under the 9 m/s gate); S12 / S14 0 FAIL on both passes; rbcycle
  **"slot 5 -> 2 after 3 taps of RightShoulder"** (RB scrolls, each tap one gun, 6 guns wrapping). Rehearsal of the close's
  Player gates on build 5: `[golden] ... 0 mismatches` per set; `[fingerprint] seed 1: 12 hashes ...: EQUAL to the
  baseline` and `off path ... vs FingerprintBaselineN18: EQUAL`; the replay found the smoke's tape pruned by the later
  reels (the close runs it right after the smoke).
- **MARATHON r4** (farm batch 3 of 6; STRICT regen, the honest bot without exfil, 40 runs): M-1 **10.0** (1.11 x 9.0:
  PASS), M-2 45.9 % (PASS), M-3 100 % (PASS), M-4 0.59 (FAIL, 0.15-0.4), R1 100 % (FAIL, 35-70 %), R2 26.8 % (FAIL, >= 50 %),
  R3 25.0 % (PASS, <= 25 %). The in-round healing that's left is the web's BLOOD PRICE mod (EN measured it alone at 0.37
  of damage taken) and the +1 / +10 earned heals: a decision for GG (NIGHT20-REPORT section 4).
- **Round-4 reviewers** (two fresh, blind, sides swapped; A = NIGHT19's game: build 1's reel with phase D and the speed
  scenarios from build 4 run with `-skygrindOff20`; B = build 5): launched 04:08.
- **The close plan (the suite takes 37 min under this machine's load):** the last code pick at ~04:40; build 6 from it is
  the frozen code; the full suite and the final judges / farm run on it before 05:26; at 05:26 the final build (the same
  code), then the smoke alone, golden / fingerprint / replay, the bench x2, the reels, the final reviewers.

## Round 4's verdicts and round 5 (the freeze, 04:30-05:26)

- **Round-4 judges** (`Logs/sandbox/r4`, farm batch 3 of 6, done 04:37): SANDBOX S1 PASS (0 dominated), S2 FAIL (shotgun
  best in 0 cells), S3 PASS (rail 1.51 x), S4 FAIL (SWAP-CANCEL 19 %, the other four >= 26 %), S5 PASS (median of medians
  9.0), S6 FAIL (shotgun 60 % of free play), S7 PASS 1.92, S8 PASS 81.2 / 44.2 s, S9 PASS 18 %, S10 +2.62 m (C3 3 / 8),
  S11 FIGHT 0.0 s vs KITE 68.7 s. CLIMB (Survival) R1 0 / 830, R2 8.7 s, floor rule 0, R2b 0 (worst 4.9 s), R3 1 / 10;
  DRYDOCK R1 4 / 407, floor rule 4 (the crow's nest: a map edit, NIGHT21), R2b 0, R3 1 / 10. The CLIMB rows match builder
  EN's `en2-final` numbers: the climb judge read the config after EN's routes pick at 04:30 (`9e8d2d0`).
- **Round-4 reviewer A, decoded** (`Logs/n20/keys/key-r4a.json`; N20 = tonight's game, N19 = `-skygrindOff20`):

  | Scenario | Pick | Side |
  | --- | --- | --- |
  | s01 sprint | B | N20 |
  | s02 boost | A | N20 |
  | s03 boostside | B | N20 |
  | s04 slide | A | N20 |
  | s05 shockwave | A | N20 |
  | s06 zoom | B | N20 |
  | s07 kill12 | Same | Same |
  | s08 swapall | Same | Same |
  | s09 hud-fight | B | N20 |
  | s10 vitals | B | N19 |
  | s11 rankup | B | N19 |
  | s12 ui-title | B | N19 |
  | s13 ui-weapons | A (marginal) | N20 |
  | s14 ui-results | Same | Same |
  | s15 climb | B | N20 |
  | s16 special-round | Unknown (leans A) | Unknown (leans N20) |

  9 N20, 3 N19, 3 Same, 1 Unknown. The speed lines (s01-s03) and the shockwave / zoom / climb went to tonight's game; the
  vitals, rank-up and title went to NIGHT19's. Flags: a magenta block on a rank letter, border lines on one side, the
  results' grade card covering the POINTS label. Shockwave: "A yes, B no" (A = N20).
- **Round-4 reviewer B, decoded** (`Logs/n20/keys/key-r4b.json`, sides swapped from A; returned 05:26):

  | Scenario | Pick | Side |
  | --- | --- | --- |
  | s01 sprint | A | N20 |
  | s02 boost | B | N20 |
  | s03 boostside | A | N20 |
  | s04 slide | B | N20 |
  | s05 shockwave | B | N20 |
  | s06 zoom | A | N20 |
  | s07 kill12 | Same | Same |
  | s08 swapall | Same | Same |
  | s09 hud-fight | B | N19 |
  | s10 vitals | A (marginal) | N19 |
  | s11 rankup | A | N19 |
  | s12 ui-title | A (marginal) | N19 |
  | s13 ui-weapons | B | N20 |
  | s14 ui-results | Same | Same |
  | s15 climb | A | N20 |
  | s16 special-round | B | N20 |

  9 N20, 4 N19, 3 Same. Shockwave: "A no; B yes" (B = N20): **both round-4 reviewers answer yes on tonight's side only**
  (U3-34.3's question). The speed scenarios s01-s04 went to tonight's game in 8 of 8 round-4 judgements; B called
  NIGHT19's border lines "too much" on all four. **Both reviewers saw a magenta square where tonight's rank letter
  belongs** (s09 f8, s11 f1-7 on the N20 side): Unity's missing-material colour on the style grade, a real defect, found
  at the freeze; NIGHT21's first fix (the code froze at 04:58; not fixed tonight). B also ticked the THUNDERCLAP's gold /
  orange stripe (s05 f1, both sides), "RMB" / "C SLIDE" / "E START ROUND" words where glyphs belong, flat cyan diamond
  shards on the slide (both sides), and the results' grade card popping in (both sides). B's rubric for tonight's side:
  kill crunch 4, hit 4, weapon 3, movement 3, score 3, style meter 2 (the magenta letter), HUD 4, menus 3, identity 3,
  motion 3.
- **The last picks (04:30):** EN `4527533` (THE CLIMB's R3 5 -> 1 of 10, floor rule 2 -> 0: design.routes perchTokens 3,
  ladderSpeed 8, swarmerClimbSpeed 8), G2 `ab94a5a` (the TORCH's damage knob 2 -> 2.5), P2 `3f52caf` (the Steam Deck page).
  EN's owed lines (the BULWARK slow-mo call, G2's fresh19 save, HV's ITelegraph) were already in. Every builder is done.
- **Gates on the integrated tree (04:32):** `fingerprint seed 1: 12 hashes in 1.31 s: EQUAL to the baseline`, `--off19
  ... EQUAL`, `--off20 ... EQUAL`, `impact PASS: the budget holds on 2 map(s)`, CoreHarness `39 vector sets (48094
  records), 22 replays (22 exact); 0 failing, 0 mismatches`. Editor compile: 0 errors. **Build 6** (`98e5459`): Succeeded
  55.3 s, 0 errors.
- **FingerprintBaselineN20 (`560a4e0`, its own commit, the reason in FingerprintBaseline.cs):** NIGHT20.md rule 3 asks
  the close to record the run with every block as configured (NIGHT19's close never did). SimHarness `fingerprint --print`
  in two processes and `twice` in one printed the same twelve (step 7200: round 4, 36 kills, 4775 earned, 8 alive; the
  off path: round 3, 34 kills, 4200 earned, 2 alive). After it: `fingerprint ... EQUAL to the baseline`, `--off19 ...
  EQUAL`, `impact PASS`; `--off20` now differs by design (NIGHT19's blocks on, NIGHT20's off: never recorded). Tests:
  N20SharedTests 6 / 6, N19SharedTests 7 / 7, FingerprintTests 6 / 6, TapeTests 13 / 13, ResidualTests 2 / 2,
  RunRestoreTests 5 / 5. **The code froze here** (04:58).
- **Build 7** (`560a4e0`, round 5's rehearsal of the close's Player checks): Succeeded 67.0 s, 0 errors; `[smoke] 206.0s
  DONE: all steps passed`; `[replay] EQUAL (1 of 1 checkpoints, 490 steps); seed 737373, Stopped, stamp SKYGRIND 0.1.0
  (560a4e0)` (the tape copied aside straight after the smoke); `[golden] ... 0 mismatches` per set; `[fingerprint] seed 1:
  12 hashes over 7200 steps in 1.13 s: EQUAL to the baseline; at the end: run Fight round 4, kills 36, shots 80, earned
  4775, alive 8` and `[fingerprint] seed 1 off path (every NIGHT19 block off) vs FingerprintBaselineN18: EQUAL`.
- **The additions wave (U3-34.9), every row taken** (each behind `design.adds.<row>`, on in tonight's config unless
  noted; its event names in PORT-STATUS; "Player" = what a Player build showed; the rest is EditMode / SimHarness):

  | Row | Commit(s) | Number | Player | Review (two lines) |
  | --- | --- | --- | --- | --- |
  | C2 LANCER (lancer.js) | E's port; `352b32d` (its off-screen tell) | THE RANGE: LANCER 8 / 8 (round 1); Hud20Tests 34 / 34 | the smoke passes with it on; farm deaths name it (4 of 40, r4) | Keep. Its "KITE >= 2 x FIGHT" target fails by the web's own numbers (logged round 1). |
  | A2 THE HUNT + A4 THE ELITE | E's rule, `73216a8` (V's ring / aura), `2b393a5` (H's threat bar) | THE RANGE: HUNT + ELITE 7 / 7; Hud20Tests 28 / 28 | the reel's `special-round`: reviewer B picked tonight's ("a red-orange beam marks the quarry") | Keep. A's overlap of the banner on "08" was the NIGHT19 side. |
  | C4 WARDEN | `70a2b4b` | BrainKindsN20Tests 2 / 2 | farm deaths name it (1 of 40, r4) | Keep; not judged blind. |
  | C3 affixes (VOLATILE, BLINKER, REGENERATING, SWIFT) | E's rule, `70a2b4b`, `30a8cb4` (CODEX) | AffixFx20 3 / 3; Hud20Tests 36 / 36 | not checked on a Player | Keep; VOLATILE's disc now yields to telegraph.js's circle (`7d2d3e2`). |
  | A5 twists (FOG, SWARMAGEDDON, LETHAL, GOLDEN) | `b0dacfe` / `30a8cb4` | Hud20Tests 36 / 36 (the announcer's twist line) | not checked on a Player | Keep; no blind check. |
  | A1 BULWARK | EN blocks 1-2, `7a1c761` (HV's look) | BossLook20Tests 5 / 5 | **row OFF**: the bot never shot it (0 shots in 520 s, EN) | Iterate (NIGHT21: the bot's boss plan, EN's report). |
  | F1 announcer + A8 NEXT | H's rows, `30a8cb4` | Hud20Tests 36 / 36 | not checked on a Player | Keep. |
  | C1 debut cards | H's card, `297d9af` (the world slows: share 0.35, <= 1.5 s) | Hud20 tests green | not checked on a Player | Keep. |
  | F3 reward reveal | H's reveal, `3f6faff` (key chips) | Hud20Tests 40 / 40, PromptText 10 / 10 | not checked on a Player | Keep. |
  | E3 CODEX | `9760cb1`, `30a8cb4` | Hud20Tests 33 / 33, Menus 20 / 20 | the reel's `ui-weapons`: both round-4 reviewers picked tonight's (GUNS / CODEX tabs) | Keep; B saw the tabs pop in at full alpha. |
  | F2 SKYGRIND TIME | `d582478` / `f4d1c6c` | SkygrindTime20Tests green (seen red 5 / 6 first) | not checked on a Player | Keep; rare by rule (1 in 60 s). |
  | B2 / D1 combos + SENT | G's / G2's combos | S4: 4 of 5 combos >= 20 % (SWAP-CANCEL 19 %) | not checked on a Player | Iterate SWAP-CANCEL. |
  | B3 FORGE tiers | G's | ShockwaveTests' FORGE charge green | not checked on a Player | Keep. |
  | B5 TORCH + VORTEX | G2 `60696a9` (TORCH 2 -> 2.5), HV `f34f5d6` / `268bbf8` | TORCH pfarm 5.5 vs median 5.25 (S5); r4 S5 PASS | not checked on a Player | Keep; the VORTEX's burst still lacks light (HV). |
  | A3 THE BEACON | `3495ec9`, `8415797`, `6f99c6d` | P2's farm: reward 85 %, 0 downs on lit rounds | the smoke passes with it on | Keep. |
  | E1 first fifteen v2 | `ff08c07`, `3045b2f` | first unlock 20 / 20 inside 5 min (31.9-45.7 s, median 37.6 s) | the smoke's new-profile steps pass | Keep. |
  | E2 ASSIST | `0ff0a6d` | +3 rounds target **not met** (the bot can't shoot drones: P2) | not checked on a Player | Iterate (the bot, then re-measure). |
  | the first-pad hint card | `6ef1a96` | P's tests green | the smoke's pad steps pass | Keep. |
  | CONTROLS glyphs + L3 HOLD / TOGGLE | P's rows | PromptText 10 / 10 | S12 0 on the pad pass (build 5) | Keep; B still saw "C SLIDE", "RMB", "E START ROUND" words in frames. |
  | E4 WEEKLY | `696aa5f` | its tests green | the smoke passes | Keep. |
  | F4 WHAT GOT YOU's tell | `9588393` / `dfe76b7` | Hud20Tests 37 / 37 | not checked on a Player | Keep. |
  | the threat bar (A1 / A4 HUD) | `bd9e926` / `2b393a5` | Hud20Tests 28 / 28 | not checked on a Player | Keep. |

  22 rows landed with a commit and a number; the rows measured on a Player build beyond the smoke are THE HUNT, the
  CODEX and CONTROLS (reels). Cut: none. Proposals only (rule 7 / GG's names): B4 the wonder weapon, C5 the climber,
  Steam Deck row 1 (`Reports/`).
- **The close's order** (NIGHT20.md U3-34.12; the suite after the benches, as the caps say): the final build (build 8,
  the same `560a4e0`), the smoke alone, golden, fingerprint, the replay, the bench x2; then the suite async, the final
  reels (normal, pad), the final judges and farm (batch 4 of 6: `final-judges.sh`), the final reviewers (two pairs, sides
  swapped).

## The close (U3-34.12, from 05:26:30 = 7:20)

- **Round-4 reviewer B returned at 05:26** (decoded above). The code had frozen at 04:58 (`560a4e0`); the magenta rank
  letter is NIGHT21's first fix.
- **The final build** (build 8 of 9, `Builds/n20-final`, started 05:26:30 from `560a4e0`'s code; HEAD `64528af` +
  FEEL-PLAYTEST.md unstaged): Succeeded 19.0 s, 0 errors; `git diff --stat ProjectSettings`: only UnityConnectSettings
  (never staged).
- **The smoke, alone** (05:27:50-05:31:59): `[smoke] 209.1s DONE: all steps passed`. Its tape copied aside at once.
- **-skygrindGolden** (05:32-05:33): `[golden] golden PASS ...` on all 39 sets, 0 mismatches each.
- **-skygrindFingerprint** (05:33): `[fingerprint] seed 1: 12 hashes over 7200 steps in 1.36 s: EQUAL to the baseline;
  at the end: run Fight round 4, kills 36, shots 80, earned 4775, alive 8, owned 2, flow Playing` and `[fingerprint] seed
  1 off path (every NIGHT19 block off) vs FingerprintBaselineN18: EQUAL`.
- **-skygrindReplay** on the smoke's tape (05:33): `[replay] EQUAL (1 of 1 checkpoints, 491 steps); seed 737373, Stopped,
  stamp SKYGRIND 0.1.0 (64528af-dirty)` (dirty = FEEL-PLAYTEST.md, a doc).
- **The bench x2** (bench runs 3 and 4 of 4; plan Balanced before and after both): final-1 `p99 17.36 ms, p99.9 69.87 ms,
  max 258.62 ms` (11410 frames); final-2 `p99 114.51 ms, p99.9 224.69 ms, max 472.42 ms` (4487 frames). Baseline (build
  1): p99 14.26 / 14.54 ms. **Worthless as a measure:** at 05:38 the CPU read 100 % with another session's jobs running
  (a dozen python processes at 77k-169k CPU-seconds each and a llama-server at 5 GB): not this session's, so not
  stopped. The cap is used up: S16 / L6 are SHORT (a cap reached, the runs contaminated).
- **Then, together (05:39):** the suite async in the Editor, the final judges + farm (batch 4 of 6, `Logs/sandbox/final20`),
  the final reels (normal, pad) and the four final sheets.
- **The suite on the frozen code** (05:39-06:01, async): **`tests: 1569 / 1569 passed, 0 failed, 0 skipped (1342.93 s)`;
  replays 22 / 22; golden tests 97 / 97** (`Logs/n20-suite-final.json`). The round-3 reds (navGraph / MapPipeline 180 s
  timeouts, DeterminismTests, RunRestore seed 37, the TORCH / SandboxHarness tests) all pass.
- **The final judges** (batch 4 of 6, from the snapshot `Temp/sh-final20` of `560a4e0`'s harness): the batch ran 3-5 x
  slower than round 4's (the suite, the reels and another session's jobs beside it: marathon seed 1 98 s vs 37 s), so at
  05:46 the parity farm (S5) and the free farm (S6) were stopped (the time limit; their latest values are round 4's on the
  pre-pick code: S5 PASS, median of medians 9.0; S6 FAIL, shotgun 60 %), and THE CHASE ran in their place
  (`final-judges2.sh`). Done 05:55.
  - CLIMB (Survival): R1 **0 of 830**, R2 **8.7 s**, floor rule **0**, R2b **0 of 11197 (worst 4.9 s)**, R3 **1 of 10**.
    DRYDOCK: R1 4 of 407, R2 never, floor rule 4, R2b 0 of 9674 (worst 3.6 s), R3 1 of 10.
  - SANDBOX: S1 PASS (0 dominated), S2 FAIL (shotgun best in 0 cells), S3 PASS (rail 1.51 x), S4 FAIL (POP 26 %, LAUNCH +
    SNIPE 26 %, CONDUCTOR 58 %, NAILBOMB 21 %, SWAP-CANCEL 19 %), S5 n/a at the close (2 rail seeds before the stop), S6
    n/a (stopped), S7 PASS 1.92, S8 PASS (KITE 81.2 s at round 10, 44.2 s at 15), S9 PASS 18 %, S10 +2.62 m / C3 3 of 8 /
    KITE gust on-off n/a, S11 FIGHT 0.0 s vs KITE 68.7 s.
  - MARATHON (40 runs, `marathon: 40 runs in the report`): M-1 **10.0** (1.11 x 9.0: PASS), M-2 **44.1 %** (PASS), M-3
    **100 %** (PASS), M-4 **0.61** (FAIL), R1 **100 %** (FAIL), R2 **26.7 %** (FAIL), R3 **17.6 %** (PASS).
- **The final reels** (reels 13-14 of 18): on `done: 4725 frames written, 9 failed, 181 s wall`; pad `done: 4846 frames
  written, 8 failed, 326 s wall`, **rbcycle `slot 5 -> 2 after 3 taps of RightShoulder`**. FEEL-METRICS on **193 PASS, 25
  FAIL, 46 n/a**; pad **197 PASS, 26 FAIL, 47 n/a**. **M8 v2: L1 0 frames inside rho 0.33 (sprint, boost, boostside,
  slide, strafe); L2 sprint median 9, boost max 39 / boostside 46 within 50 ms, 0 under 9 m/s; L3 41 px sprinting, 110
  px boosting; L4 boostside 2 deg off horizontal, slam 0 deg off vertical; L5 0 frames over a live enemy: all PASS** on
  both passes. **S12 0 FAIL; S14 PASS on hud-fight, sprint, boost, vitals, zoom** on both passes. The FAILs are M9 pop-ins
  / entrance times (zoom, ui-weapons, special-round, slide, roundend, regen, gust, vitals, ui-results).
- **Before / after GIFs** for the report: `Logs/n20/gifs/{sprint,shockwave,zoom,climb}.gif` (A20 | final20 side by side).
- **The final reviewers** (four fresh agents; two pairs, sides swapped within each pair: FA / FB on keys `key-fa` /
  `key-fb` (inverted), FC / FD on `key-fc` / `key-fd` (inverted); A20 = NIGHT19's game vs final20; launched 05:54, back
  06:01-06:03), decoded with `decode20.py` (N20 = tonight, N19 = NIGHT19's game):

  | Question | Scenario | FA | FB | FC | FD |
  | --- | --- | --- | --- | --- | --- |
  | speed | s01 sprint | A: N20 | B: N20 | A: N19 | A: N20 |
  | speed | s02 boost | A: N20 | B: N20 | A: N19 | A: N20 |
  | speed | s03 boostside | A: N20 | B: N20 | B: N20 | A: N20 |
  | speed | s04 slide | B: N20 | A: N20 | B: N19 | B: N20 |
  | weapon | s05 shockwave | A: N20 | B: N20 | B: N20 | A: N20 |
  | weapon | s06 zoom | B: N20 | A: N20 | A: N20 | B: N20 |
  | weapon | s07 kill12 | Same | Same | Same | Same |
  | weapon | s08 swapall | Same | Same | Same | Same |
  | HUD | s09 hud-fight | B: N19 | A: N19 | B: N19 | A: N19 |
  | HUD | s10 vitals | B: N19 | B: N20 | B: N19 | A: N19 |
  | HUD | s11 rankup | A: N19 | B: N19 | B: N19 | A: N19 |
  | menu | s12 ui-title | A: N19 | B: N19 | Same | B: N20 |
  | menu | s13 ui-weapons | A: N20 | B: N20 | B: N20 | Same |
  | menu | s14 ui-results | Same | Same | Same | Same |
  | enemy | s15 climb | B: N20 | B: N19 | B: N20 | B: N19 |
  | enemy | s16 special-round | A: N20 | B: N20 | A: N20 | B: N20 |

  Totals: FA 9 N20 / 4 N19 / 3 Same; FB 9 / 4 / 3; FC 6 / 6 / 4; FD 8 / 4 / 4. **Shockwave ("a burst of air that moves
  the player?"): yes on tonight's side, no on NIGHT19's, from all four** (FA "yes, but weakly"; FC "yes for the burst;
  whether it moves the player is unclear"). **T6 (the speed question):** FA, FB, FD picked tonight on all four speed
  scenarios, FC on 1 of 4 (FC read NIGHT19's radial border as "fast at a glance"): **3 of 4 reviewers, 13 of 16
  judgements.** **S15** (speed + weapon + HUD + menu, s01-s14, 56 judgements): tonight 26 (46 %), NIGHT19 16, Same 14
  (62 % of the decided ones): **under 70 %**. The HUD question went to NIGHT19 in 11 of 12: **all four reviewers ticked
  the magenta square** (pattern 11) where tonight's rank letter belongs (s09 f8, s11), the defect round 4 found.
  Too much on tonight's side: the shockwave's ring over the centre ~100-170 ms (borderline, all four); NIGHT19's border
  lines were "too much" for every reviewer. Other ticks on tonight's side: flat cyan slide shards (13), "E START ROUND" /
  "C SLIDE" words (10), the grade card over POINTS (8), the THUNDERCLAP's gold stock (11), the PULSE GAUNTLET as a box
  (14), capsule enemies (14).

## The close table (U3-34.12; `date` before it: see the transcript)

| Item | DONE / SHORT | Evidence ("done when", from this session) |
| --- | --- | --- |
| U3-34.1 Start | DONE | RECEIVED logged 22:06-22:18; tasks.json `parses: 183 items; U3-34.*: 13`; `Builds/n19-final` (`8a33313`) copied; its reels ran (pad `done: 3135 frames written`). |
| U3-34.2 The shared commit, the new judges and the baseline | DONE | `ce579d2` + gates (fingerprint EQUAL, `--off19` EQUAL, `impact PASS`, golden 0 / 22 exact); `damageWeight` 0.05 -> 0.1 logged; THE CLIMB / THE MARATHON committed, SandboxClimbTests green, the hand check (box80: judge 18.28 s, a real husk 16.73 s); `Logs/sandbox/b0n20` S1-S11 / CLIMB / MARATHON valued; M8 v2 + reel phase D; bench baseline p99 14.26 / 14.54 ms; builders launched 22:31:07; the suite 1569 / 1569 at the close (DroneTests included). |
| U3-34.3 Shockwave on the THUNDERCLAP; zoom on most guns | SHORT | ShockwaveTests 12 / 12, ZoomTests 9 / 9 (53.3 / 62.2 / 24.3 deg); C3 3 of 8 pushed >= 3 m; KITE shockwave on 73.1 s vs off 95.8 s (<= +25 %); "a burst of air": both round-4 reviewers and 4 of 4 final say yes on tonight's side. Short: the farm's shockwaves per round (1.5-3) not measured (the time limit). |
| U3-34.4 The pad: RB scrolls weapons | DONE | PadRbCycle 11 / 11 + reader 5 / 5 (in the suite's 1569 / 1569); the final pad reel `rbcycle: slot 5 -> 2 after 3 taps of RightShoulder`; S12 0 FAIL on the pad pass; the final smoke `209.1s DONE: all steps passed` (its pad steps). |
| U3-34.5 Regen at the round's end + balance | SHORT | RegenN20Tests 25 / 25 + RegenN20SimTests 4 / 4; MARATHON (40): M-1 10.0 PASS, M-2 44.1 % PASS, M-3 100 % PASS, **M-4 0.61, R1 100 %, R2 26.7 % FAIL**, R3 17.6 % PASS; S8 81.2 / 44.2 s PASS; S9 18 % PASS; S1 PASS, S2 FAIL, S3 1.51 PASS. Short: the in-round heal left is BLOOD PRICE (the web's mod) and MEND: a decision reserved for GG (NIGHT20-REPORT section 4). |
| U3-34.6 Enemies reach every floor | SHORT | CLIMB Survival R1 0 / 830, R2 8.7 s, floor rule 0, R2b 0 (worst 4.9 s), **R3 1 / 10**; DRYDOCK **R1 4 / 407**, floor rule 4, R2b 0, R3 1 / 10; RouteLinks / Climb tests in the 1569 / 1569; golden 0 / 22 exact; the climb tell seen by 2 of 4 final reviewers (tell timing not measured). Short: R3's box54 after EN's five knob passes (a blocker after two approaches); the crow's nest needs a map edit (GG's question 3). |
| U3-34.7 Speed you can see | SHORT | Final reels, both passes: L1 0 frames inside rho 0.33; L2 sprint 9, boost 39 / 46 in 50 ms, 0 at a walk; L3 41 / 110 px; L4 2 deg / 0 deg; L5 0 over enemies; S14 PASS sprint / boost; SpeedLines20Tests in the 1569 / 1569; **T6: 3 of 4 final reviewers (13 of 16 judgements) pick tonight**. Short: L6 (bench p99 + 0.3 ms) not measurable: the bench cap (4 / 4) is spent and both final runs ran beside another session's jobs (17.36 / 114.51 ms). |
| U3-34.8 NIGHT19's unfinished guns and enemies | SHORT | S1 PASS (0 dominated), **S2 FAIL**, S3 PASS (1.51 x), **S4 FAIL** (4 of 5 >= 20 %; SWAP-CANCEL 19 %), S5 PASS at round 4 (9.0; not re-run at the close), **S6 FAIL** at round 4 (shotgun 60 %), S7 PASS 1.92; DroneTests 7 / 7, ArsenalN19, FreshnessN19 in the 1569 / 1569; C9 / C10 valued (KESTREL best 1.21 / 2.01). Short: S2 / S4 / S6 unmet at the time limit. |
| U3-34.9 The additions wave | SHORT | 22 rows landed, each with a commit and a number (the table above), each behind `design.adds.<row>`, event names in PORT-STATUS; on a Player: THE HUNT (`special-round`: 4 of 4 final reviewers pick tonight), the CODEX (`ui-weapons`: 3 of 4), CONTROLS (S12 0), the first fifteen (the smoke). Short: most rows' numbers are EditMode / SimHarness, not a Player build; the BULWARK row off; ASSIST's +3 unmet (the time limit). |
| U3-34.10 Not cheap | SHORT | S14 0 in the central 40 % on hud-fight, sprint, boost, vitals, zoom (both passes); THIRD-PARTY rows for all 8 imports. **S15 26 of 56 (46 %) < 70 %**; pattern 11 (the magenta rank square) ticked on tonight's side by all four; M9 pop-in / entrance FAILs on new screens; S16 not measurable (the bench cap, contaminated runs). Short: the time limit (the defect found at the freeze) and the bench cap. |
| U3-34.11 The loop | SHORT | Rounds 1-4 on builds 2-5 and round 5 (build 7, the close's rehearsal) logged with their judges, reels, fix lists and keep / iterate / cut; round 4's two reviewers decoded; beyond rows taken with commits (D1 SENT, B3, B5, E2, E3, the pad card, CONTROLS glyphs: the table above). Short: round 2's reviewers were lost to the account's spend-limit stop (01:05-01:52). |
| U3-34.12 Close | SHORT | Freeze 04:58 (`560a4e0`, FingerprintBaselineN20 recorded once with its reason); the final build at 05:26:30 (Succeeded, 0 errors); smoke alone `209.1s DONE`; golden 39 sets 0 mismatches; fingerprint EQUAL + off path EQUAL; replay EQUAL (491 steps); bench x2 run; the suite 1569 / 1569 (replays 22 / 22, golden 97 / 97); the final reels (on, pad); the final judges (range, chase, climb x2) and THE MARATHON; four final reviewers decoded; the docs; tag `u3-n20`. Short: THE PARITY FARM and the free farm were stopped at 05:46 (the time limit: the batch ran 3-5 x slow beside the suite and another session's jobs). |

## Builds

| # | When | Code | Result | Smoke | Notes |
| --- | --- | --- | --- | --- | --- |
| 1 | 22:59 | `64d7ea3` | Succeeded, 49.7 s, 0 errors | `[smoke] 185.3s DONE: all steps passed` (23:02-23:05, beside the builders) | the baseline build (NIGHT19's game + the shared commit + the judges), copied to `Builds/n20-b1` |
| 2 | 00:06 | `cc98fb4`-era | Succeeded, 0 errors | `[smoke] 197.1s DONE: all steps passed` | round 1 (`Builds/n20-b2`) |
| 3 | 00:44 | `850cda5` | Succeeded, 41.7 s, 0 errors | `[smoke] 195.5s DONE: all steps passed` | round 2 (`Builds/n20-b3`) |
| 4 | 02:04 | `81d8bc3` | Succeeded, 35.4 s, 0 errors | `[smoke] 198.6s DONE: all steps passed` | round 3 (`Builds/n20-b4`) |
| 5 | 03:48 | `075e2a6` | Succeeded, 82.6 s, 0 errors | `[smoke] 213.4s DONE: all steps passed` | round 4 (`Builds/n20-b5`) |
| 6 | 04:39 | `98e5459` | Succeeded, 55.3 s, 0 errors | not run (the re-record came next) | the last picks (`Builds/n20-b6`) |
| 7 | 05:00 | `560a4e0` | Succeeded, 67.0 s, 0 errors | `[smoke] 206.0s DONE: all steps passed` | round 5: the frozen code with FingerprintBaselineN20; the close's Player checks rehearsed (`Builds/n20-b7`) |
| 8 | 05:26 | `560a4e0` (HEAD `64528af`) | Succeeded, 19.0 s, 0 errors | `[smoke] 209.1s DONE: all steps passed` (05:27-05:31, alone) | **the final build** (`Builds/n20-final`) |
