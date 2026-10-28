# Z2 THE THROW CHART IN SKIP V2 (print only): builder T's notes (NIGHT70; branch n70-T, from 140b0241d)

## What it is (Tools/SimHarness/Harness.N65.Verb.cs, V65ClearRun + V70ChartStep / V70Launched / V70ChartEnd)
- **The falls:** every skip v2 run now counts `fallReturn43` (the quench / pit return, Campaign24.cs:1038) as a fall in the room, beside
  `stageRespawn`, as n46flow does (Harness.N46.Flow.cs:233-234). Every seed line's `falls N` (verb-aware, refused-in, walker, committed) reads it.
  Nothing in the run reads res.falls, so no decision changes.
- **The throw chart:** on the PLAIN verb-aware run only, each launch made while the player is in the room (charted at its LaunchEvent, as
  launch36's count) is followed: its start = the new flight's own `from` (Launch36.Flight.from); its end = the step its body leaves launch36's
  flights: `lastEnd` (body / wall / timeout / landed / dead; `(shared step)` when two flights end in one step), `lastThrown` (flat metres,
  start to the flight's last check), the kills of that step's burst nearest the body (body / wall ends), then the body is followed up to 3 s
  from its launch (as the probe follows it): in a pit / in lava = inside one of the room's pit rects below y -0.2 (V65Pits' rule). Printed after
  the launchkill lines as a NEW line kind:
  `n65verb skip launchend: room <id> | seed N | from (x, z) to (x, y, z) thrown D m | end <end> | kills K | in pit yes/no | in lava yes/no`.
  Ends beyond the slate's four: `dead` (the body died mid-flight from something else: another flight's burst or the gun), `relaunched` (thrown
  again while still in flight), `running at the run's end`.
- **room-chain.sh** (Research/2026-10-25-forge70/room-chain.sh) prints `launch ends <id>: median N charted launches per run ...; ends ...; in
  pit P, in lava L; falls (verb-aware seeds) F` from the round's verbs-skip.txt.
- The first version diffed launch36's flights after each step only and missed a throw blocked on its first step (it starts and ends inside one
  sim.Step: kb3 seed 3 charted 4 of 6 launches); the chart now starts each throw at its LaunchEvent: every seed's chart count equals its
  `launch36 N` on all nine seeds below.

## The proof (this folder; BEFORE = the 140b0241d build, AFTER = the Z2 build, the same command; z2-compare.cjs)
- Shipped kb3 (n57-kiln, --plant-verb65 kb3:launch), seeds 1-3: Z2-compare-kb3.txt PASS: 54 lines identical, 0 falls-only, 0 other; 14 launchend lines.
- Shipped v2 (n52-spire, --plant-verb65 v2:launch), seeds 1-3: Z2-compare-v2.txt PASS: 53 identical; 6 launchend lines.
- The fall case, NIGHT69's r1d kb3 (Logs/n69/r1-kiln/n67-kiln.json, the main tree's, copied to the scratchpad), seeds 1-3: Z2-compare-kb3r1d.txt
  PASS: 31 lines identical, the 12 seed lines differ ONLY by their falls (0 -> 6, 9, 4, 5 / 3, 2, 5, 6 / 5, 10, 3, 1: the quench returns
  NIGHT69 read as 0), 0 other differences; 9 launchend lines (one wall burst in the quench: `thrown 9.2 m | end wall | kills 1 | in pit yes`).
- room-chain's line on the three AFTER files: Z2-room-chain-line.txt. The five-level walk on the Z2 build: Z2-walk.txt.
- Not checked: 20 seeds; a lava room; a `(shared step)` or `relaunched` end (none in these nine runs); the critic.
