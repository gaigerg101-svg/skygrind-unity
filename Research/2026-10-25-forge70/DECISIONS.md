# DECISIONS (FORGE70, 2026-10-25 ~21:44-23:30; GG: "can we use the same 2 methods to prompt another 8 hour overnight prompt")

The two methods: (1) wave-forge: recall (NIGHT69-REPORT.md section 7, docs/memory, L-151), an idea pool (IDEAS-POOL.md, 22 cards), three scouts in parallel (scout.md: code + data;
design-research.md: knockback-into-hazard and ride rooms, with URLs; prompt-methods.md: the long-prompt / usage-window re-research), two measured spikes (spike.md), these decisions,
a red team (redteam.md), the slate seed (make-slate.cjs), the builder brief, room-chain.sh, GOAL.txt and the n70 tools (Tools/night/forge70-make-n70.cjs, n70-throws.sh,
n70-runner.sh); (2) the prompt-methods re-research of this morning's note (Research/2026-10-25-forge70/prompt-methods.md).

## D-1 An 8-hour night is two usage windows on a SHARED seat: two acts keyed to the resets, not one schedule against CAP (prompt-methods.md 0, 2)
Measured: NIGHT68 (8 h) stopped ~1:13-4:00 and ~6:24-9:01; NIGHT69 stopped at 2:10 (the window was not fresh: it opened at 07:50 with NIGHT68's close, and FORGE69 ran in it); every short
window on this seat was shared by two or three heavy sessions (APEX S30 / S31, investing S9 / S10 paused at the same resets). So NIGHT70.md is Act 1 (t0 to the first stop), the stop
(only the queue runner computes), Act 2 (the resume to the second stop or 7:00), the close. Everything Act 1 owes is committed or queued by 1:00 (the stop can come that early on a shared
seat) and again every 30 minutes. Rejected: a fixed 2:20 stop (it was a seat figure, not ours); one long schedule (NIGHT68's failure mode).

## D-2 The queue runner (Tools/night/n70-runner.sh, spiked 22:16, spike/runner-spike.txt): the one dotnet queue in the main tree, through the stops
Jobs are files (Logs/n70/queue/NNN-name.job); a hold file lets the lead pick between jobs; it stops at 7:00 or a stop file. The stop's ~2:40 then computes a1 (rounds, fresh sets,
throw charts) and the reds. Rejected: a round worktree (more machinery; the main-tree rule holds with the hold file); builders running main rounds.

## D-3 M3 on 20 probe seeds, the burst counted (DECIDE69 Q-A: the forge's call, written before any measurement)
A launched husk always dies in its own burst (60 dmg, 3.5 m, the flier included: Launch36.cs:211-229), so "in a pit" needs a throw that hits nothing; one seed was one sample (spike 2:
v2 s1b's crypt caught 15 of 20 probes where a1's seed 1 read FAIL). M3 = `M3 candidates N / 20` from n70-throws.sh (a pit / lava end, or a burst killing >= 2 incl. the flier, on GREEN
probes), PASS at N >= 10. Shipped kb3 reads 7 / 20 (FAIL), shipped v2 14 / 20: M3 is the room's "the verb works here" check, not its gain test (M2 is).

## D-4 The blocker is how often the verb happens, so the rooms' job is fresh bodies at punch range (spike.md; scout.md A; design-research.md R7)
2-4 launches a run against about 23 bodies (kb3 red 4, r1d 2.5; v2 2); the pilot launches only an unshot husk above the 30 hp bound within 5.5 m and 50 deg. A card states its launch
target (the red's median + 2 or more) and how bodies arrive close and fresh (short sight lines, a corner or door 6-10 m away), plus where each throw ends (a wall 1-12 m out on the
approach line, or a pit 0.5-6.9 m out and 2.5-6.5 m long, no body within 1.1 m of the line). A candidate goes to a main round only when its builder's own-tree numbers clear the screen
(launches >= the target, M2 within 0.10 of the bar): the sim is deterministic per room set and seed (L-151; R's alone 1.08 = a1's 1.08).

## D-5 THE COMMITTED PILOT (Z3): the verb-aware side may play the verb on purpose, symmetric with the refusal's "faster of two"
The plain verb-aware pilot shoots every husk and launches what the gun left over. A second verb-aware run holds fire on launchable bodies within 12 m; the M2 line uses the faster of the
two per seed, as the refusal uses the faster of refused-in and walker. The reds and bars are re-made on it before any GATE round (a red that rises raises its bar), and a critic checks
the refused runs are byte-identical. This is the one instrument change; it is written before any measurement. Rejected: changing the bars (never after a measurement); a stationary
enemy kind (GG's).

## D-6 Two rooms in depth (kb3 R, v2 S); g8 parked (DECIDE69 Q-C); no third room
Five nights of breadth gave the GATE 0 / 22. The ride room has no honest payer under the walker rule (LevelValidate24.cs:219-258; the only stationary ranged body is the lancer, a new kind
in the room = enemy design). T builds the three tools, then is on call for the rooms' fixes. Rejected: v6 / kb3d / kb3c tonight; the `ledge`-kind loophole (it would break GG's
"a route that enemies can climb up").

## D-7 The GATE round is a2 (Act 2, on the final tools); a1 (queued in Act 1, computed in the stop) is evidence and the review's input
Only a round whose slate-at-start shows Z1-Z3 settled holds a GATE. Act 2 needs about 1:45 for review, fixes, a2, packs and one batched critic: the prompt says what is cut if Act 2 starts
late (a2 is still run; the critic only if there is time; otherwise SHORT with numbers).

## D-8 The play build at the end is NEW (Builds/n70-play), never written over n69-play; one direct smoke on it
## D-9 Guard fix: a shell keyword before a command (`for ...; do node -e`) hid it from every rule (NIGHT69's lead wrote a file that way); selftest 111 / 111
## D-10 The forge experiment E70: acts keyed to the resets + the queue runner (prompt-methods.md 4)
Hypothesis: the stops cost no committed work and the stop's hours compute. Measure (the report's section 8): uncommitted minutes at each stop (git times vs the stop times in
Logs/n70/stops.txt), queue-runner minutes computed during the stops (runner.log), close minutes after CAP; NIGHT68: PC and the full close lost to the second stop.
