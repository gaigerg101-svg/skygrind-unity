# Z3 THE COMMITTED PILOT: builder T's notes (NIGHT70; branch n70-T)

## What it is (Tools/SimHarness/Harness.N65.VerbPilot.cs, Harness.N65.Verb.cs)
- skip v2 in a LAUNCH room (refuse = launch) runs a FOURTH run per seed, the committed run, AFTER the three it ran before (verb-aware, refused-in,
  walker run first and in the same order, so their lines are the plain build's). `--no-commit` gives the plain build's skip v2 (no fourth run);
  `--commit` is the default (a room of another verb prints that it has no committed run).
- The committed run = the plain verb-aware pilot with `commit = true`. While no launch episode runs and a launchable body is inside 12 m flat:
  - the fire is held (c.fire up). A pistol / shotgun charge already building is kept held (a charge35 release always fires: a full charge or a
    tap), and a switch to the other gun (pistol <-> shotgun, owned) is asked, which drops a building charge or a queued tap with no shot
    (ChargeShots27.cs `the gun in hand changed under the hold: dropped, nothing spent`);
  - the pilot closes on the nearest such body (look on it, stick forward); the launch episode's 50 deg test then reads the look it was given.
  - "launchable" (round 4, the DEFAULT, the slate's literal rule as the lead ruled: no rule loosened after a measurement) = husk-shaped (not a
    stand-in or a tank), hp above the launch bound (max(30, the largest non-parry punch)), chargers included. The round-2 / round-3 cap (hp at or
    under launch36's burst damage, 60: a body its launch ends) is the OPTION `--commit-burst-cap`, off by default, named in the runs line, the
    committed seed lines and the M2 line. Why the cap existed: round 1 held fire on chargers too (hp 42-150, knockbackScale 0.3, a 3.8 m flight
    they survive): the committed kb3 runs took 57-79 s against the plain run's 20-40 s, with 33-46 launches a run (Z3-skip-kb3-GREEN.txt).
- The M2 line: per seed the verb side = the faster of verb-aware and committed (ties: verb-aware); ratio, exercised, STYLE (pooled) and the
  M1 / M2 lines read the chosen run; the M2 VERB / FRESH line adds: committed on K of N seeds (which), the verb-aware run alone's pooled ratio
  and exercised count, deaths on the chosen runs (and each run's), launch36 per run median verb-aware / committed, gun shots in a held step.
- The card's launch target stays the PLAIN run's: the `  seed N verb-aware:` lines (room-chain.sh's launches grep) are byte-identical; the
  committed run prints a new line kind `  seed N committed:` (room-chain prints its median launches beside, as "launches (committed, Z3)").
- The touch table and the launchkill lines stay the plain verb-aware run's (the area lint and the deco checker read them): unchanged.
- n70-red-bars.cjs: a red whose skip ran the committed run gets bar = max(1.25 + (redZ3 - redPlain), redZ3 + 0.20), redPlain read from the
  same file's "the verb-aware run alone: ratio" (red team B3); the change printed per room.

## What else it changes (exactly)
- skip v2 wall time in a launch room: +1 run per seed (about +33 %; the committed runs are longer when they lose).
- M1 / STYLE+15 / M2 VERB / M2 FRESH / totals lines and the `=> ratio` lines carry the chosen run; the plain run's own ratio is printed beside.
- The selection gain: the faster of two chaotic runs is biased up; the Z3 bar formula above charges it to the bar (v2's 3 seeds: 0.80 -> 0.92,
  bar 1.25 -> 1.37 on the planted dry run).
- Nothing in n46flow (its verb pilot never sets commit), the flow meter, the probe, the refused runs, the walker, any room, wave, kind, Mover.

## Evidence (this folder)
- RED (t0 build, a scratch copy): Z3-skip-kb3-RED.txt, Z3-skip-v2-RED.txt (`n65verb skip --seeds 3 --plant-verb65 <id>:launch`, shipped rooms).
- Round 1 / 2 / 3 (Z3 builds): Z3-skip-*-GREEN.txt / GREEN2 / GREEN3; compares Z3-compare-*-GREEN1 / GREEN2 / GREEN3 / FINAL.txt
  (z3-compare.cjs). Round 1 and 2 FAILed on gun shots in a held step per seed (round 1: kb3 0 / 3 / 0, v2 0 / 1 / 2; round 2: kb3 0 / 1 / 0,
  v2 0 / 1 / 2; a charged shot counts 2: its "fire" and its "chargeShot27Fire"); round 3: 0 on all six runs.
- --no-commit on the round-3 build: Z3-skip-*-NOCOMMIT3.txt: every line identical to RED but the # headers, the exit clock and the wall time.
- The five-level walk on the round-3 build: Z3-walk.txt. The red-bars dry run: Z3-redbars-dry.txt.
- Not checked: 20 seeds (the reds re-made on Z3 are the lead's queue job), the fresh seeds, a ride / parry / pierce room (no committed run there:
  a code read only), the critic.
