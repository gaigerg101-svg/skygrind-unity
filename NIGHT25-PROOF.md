# NIGHT25-PROOF.md (row, scenario, RED, GREEN, build, date)

Players: Development Players built by Unity -batchmode from a project copy (`Assets/Editor/BatchBuild24.cs`). RED = the same scenario with the row off (`-skygrindOff25`, side `red` / `redcamp`) or before the fix. Logs `Logs/player/n25-reel-<tag>-*.log`, frames `Logs/feel/<tag>/*`. 2026-10-08.

| Row | Scenario | RED | GREEN | Build |
| --- | --- | --- | --- | --- |
| shockwave-off | `shockwave` | `RED: design.n25.shockwaveOff is off (NIGHT24's shockwave: 1 of 3 presses fired)` (p1 red) | `PASS: no shockwave events on 3 shotgun LT presses; zoom on (x1.25)` (p1 on) | n25-8 |
| pulse-charge | `pulse` | n25-1: `FAIL: the release did not launch once` (a scenario bug, fixed by C: banked hold launched early, then the 3 s lockout); p1 red: `RED: design.n25.pulseCharge is off` | `PULSE held on a husk: first notch after 7 landed hits (want 6-8); release at the first notch launched 1.1 m (asked 1.1 m = 0.5 J) ... PASS` | n25-8 |
| melee-fire | `meleefire` | cannot be RED in Core (the sim runs both in one step with the row off too: `8 shots + 5 punches (the melee row off; Core runs both in one step either way)`) | `8 shots alone; 8 shots + 5 punches (the melee row on)`: PASS. The cause GG felt is the PAD: PUNCH was on R3 (RS) under the aiming thumb; layout v5 puts PUNCH on RB (`PadMelee25Tests`) | n25-8 |
| bug-void | `wyrmvoid` | GG's screenshots (void sky, no floor, ROUND 1 · 0 LEFT) | `PASS: no worm outside the campaign` on a survival boot; the arena room view (WyrmRoomView) draws floor, walls, light only while the worm fights; `wyrmdies` ran the campaign path to LAYER CLEARED with the room hidden after the kill | n25-8 |
| boss-bar | `wyrmbar` | no bar (NIGHT24 build) | `bar valid True, name 'KILN WYRM', HP 1.00 -> 0.95 after a 5 % hit, phase mark 0.66, pips 2, lit 2 -> 1 at phase 2, name text 24 px` PASS | n25-8 |
| boss-dies | `wyrmdies` | | `bossDown True, pieces drawn after no, room still drawn no; campaign LAYER CLEARED` BUT the scenario FAILED its phase-retry clause: `HP 0.30 -> 0.30 (threshold 0.33), retries counted 0` (W debugging) | n25-8 |
| boss-aimed | harness `wyrm25` | composed sim, seeds 3-5, wyrmAim off: 3.0 player hits / min, kill 3:01 | wyrmAim on: 5.5 hits / min, kill 3:57 (3:42 / 4:10 / 4:00), shortest tell 0.60 s, nearest lethal pair 1.73 s; Player scenario `wyrmaim` NOT WRITTEN | |
| tutorial | `tutorial` | n25-2: `NOT WRITTEN (RED)`; n25-4: SKIPPED (gate bug) | n25-8: `TUTORIAL CLEARED`, 8 areas in 42.2 s game time (par stage sums 51 s), stage respawns 0, deaths 0, lessons in order, 8 per-stage recordings of up to 240 frames | n25-5 / n25-8 |
| lesson prompts | `lesson` | | `'SLIDE UNDER THE GAP' (4 words, 24 px, first alpha 0.00, in 217 ms, out 217 ms, gone after use True)` PASS; text follows the device and layout (placeholders) | n25-8 |
| bigpick head | `bigpick` | n24: `ROUND 10` read for the round-5 pick | `round 5: head 'BIG PICK · ROUND 5' ... round 10: default after 6.0 s` PASS | n25-8 |
| first-play-hitch | `-skygrindHitch` | n25-1 (n24 code): cold PLAY worst 346 ms | route prebuild (N20StartRun 138 -> 1 ms), HUD wake (R.Ui 173 -> 5.7 ms): cold PLAY worst 247.8 -> 74.4 ms (n25-7); two frames still > 33 ms (SimStep 24 ms, R.World 20 ms): SHORT against `no frame above 33 ms` | n25-7 |
| curriculum | harness `level25` / bot rates | | levels 2-10 as plan files with 0 validate errors, bot first-try clear rates per room in CURRICULUM.md; levels 6, 9, 10 boss rooms are SHORT (BeginBoss starts only the wyrm; W wires them) | |
| m9-popins | `m9-list.py` | tutorial reel: 11 first appearances at alpha >= 0.95 (style meter x9, kill marker x2) | fixes d1c0489 (0.1 s / 0.04 s fades), re-measure pending | |
| fingerprint | | | pending the freeze | |

## Final-code proof (build n25-12, 19:37-19:48, rows ON; Logs/player/n25-reel-p3*-*.log)
- shockwave PASS / RED (p3 red: `RED: design.n25.shockwaveOff is off ... 1 of 3 presses fired`); pulse PASS (7 hits to the notch, 1.1 m launch) / RED; meleefire PASS; lesson PASS (24 px, 217 ms in / out); bigpick PASS (`BIG PICK · ROUND 5`).
- worm: wyrmvoid PASS, wyrmbar PASS, wyrmdies PASS (`phase-3 retry: HP 0.29 -> 0.33 (threshold 0.33), retries counted 1; bossDown True; LAYER CLEARED; room hidden after`), wyrmaim PASS (aimed 13 / plain 67, 2 lunge locks, 2 midpoint re-aims, 9.3 hits / min); RED pair: `wyrmvoid`/`wyrmbar` with `-skygrindOff25` pass as designed (wyrmAim off).
- bosses (map level10): wyrm PASS (40 s), bulwark PASS (87 s), monolith PASS (33 s), ELDER WYRM PASS (38 s, bar 'ELDER WYRM', retry p4 0.21 -> 0.25), RIVAL PRIME PASS (58 s, bar 'RIVAL PRIME', retry p2 0.45 -> 0.50); each: bar valid, floor + room drawn (lower-third luminance void 0.051 vs room 0.439), magenta px 0; RED lines: `final with design.n25.bosses off: no fight`, `rival with design.n25.bosses.rival off: no duel`.
- tutorial CLEARED (p3 tut, 42.2 s); campresults FAIL on the Player (focus on RETRY LEVEL, CONTINUE -> PendingMapId ''): F fixing.
- suite 2 on the integrated tree: 1760 / 1760, 22 / 22 replays, 97 / 97 golden (468 s).
- hitch tour on a busy machine (builders compiling): cold PLAY worst 197.5 ms, p99 31 ms: not comparable to the quiet n25-7 run (cold PLAY 74.4 ms); re-measure quiet at the freeze.
- levelwalk (n25-11, 235 s game-time budget): level3, level4, level8 LEVEL CLEARED; layer1, level5, 06, 07, 10 stopped by the budget at room 2-4 (bot slow in shield / fist rooms; the harness bot clears them): SHORT as Player-verified levels.
