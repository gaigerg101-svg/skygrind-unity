# NIGHT21-REPORT: the moves and the moments (for GG; one page)

The night ran from 08:20 on 2026-10-07 (`date`) to about 13:30. **About 3 hours of it were lost** to the Unity Editor
sitting on "Recovering Scene Backups" (09:09 to 11:30) after the terminal crash; the builders worked in pure C# against
the headless harness meanwhile. The Editor first compiled and tested the new code at 11:43. Every number below is from a
tool run this session; the full record is `NIGHT21-LOG.md`.

## Outcome

**Five features run in the Player's sim (ARCLINE, SLIDE BITES, STAMP, DEADLINE, THE GRIND); INSTANT REPLAY and challenge codes are Core only; none is fully finished.** No feature passed its whole
contract: each contract names a reel scenario, and **no N21 reel scenario was built** (no Editor for the first 3 h; the
FeelReel phase-E partials never got written). `passes` is 0 of 7 in `NIGHT21-FEATURES.json`. Nothing has been played by a
human or looked at by a blind reviewer. The persona triage (one fresh agent, files only) judged from the code, not from play.

## Per feature: what you can do, its switch, its number

All rows are `design.n21.<row>.enabled` (both config copies) and `-skygrindOff21` gives NIGHT20's game bit for bit.

| Feature | You can | Number printed tonight | Not done |
| --- | --- | --- | --- |
| **THE ARCLINE** (`arcline`, on) | Buy it at the west-island terminal for 5500 once ISLANDS is open; one button yanks a husk, swarmer or drone to you (15 s cooldown), zips you to a big enemy, else swings on a node or surface | headless `arcline PASS`: attaches 10/10, peak 30.7 m/s after a swing (a scripted dash into the hook; a plain hook reached about 19), a yanked husk moved 8.6 m; `Arcline21Tests` 5/5 in Unity | the rope / node glows / reticle compile and are wired but **no human or reel has seen them**; no red reticle, no HUD chip; the shared Y button vs SPECIAL not re-derived; the persona list says the pad button may collide with RB (**unchecked**) |
| **SLIDE BITES** (`slideBites`, on) | Slide through enemies: they take damage, a survivor staggers, a swarmer dies; CLOSE CALL and a SLIDE BITE style bonus | `slidebite PASS` (3 of 3 damaged); 3 tests | no view, never seen |
| **THE STAMP** (`stamp`, on) | Each cleared round gets a letter (`roundGrade`); RESULTS lists them | autopilot, 32 rounds: A 12, B 11, C 3, D 3, S 3; `Stamp21Tests` 7/7 | the inked entrance, the grade card off POINTS (T-11); a round **P** was never reached by the bot |
| **The magenta square** (T-01) | The rank letter no longer draws magenta in the Player | **M-MAGENTA 0 px on every frame of both reel passes** (4726 + 4845 frames, 37 scenarios on the pad pass); NIGHT20's `hud-fight` frame 60 reads 9487 px, tonight's same frame 0 | the fix is "UIEffect installs in the Editor only", so **the Player has no shine / edge glow** (a look change) |
| **DEADLINE** (`deadline`, on) | A fifth twist: an 18 s fuse, kills refill it (slide and slam most), a burn is a heavy hit that never kills and restarts a 5 s fuse, points double | THE CHASE KITE, round 10: 30.3 s of 63.2 s normal (48 %, 10 seeds). Your 25 / 10 s starting numbers gave 60 % (over the 50 % contract), so I retuned | the HUD fuse bar; no reel |
| **THE GRIND** (`grind`, on) | TITLE's MODE row: endless waves, 4 s break, WAVE banner, the core reshapes, death ends the run, RESULTS shows the best wave | `grind --seeds 10 --seconds 300`: median wave 8 (min 5, max 9); the core's heights differ on every wave (75 fights); a forced death ends the run once; `Endless21Tests` 11/11 | the smoke's GRIND step, a reel, persistence of the best wave, the telegraph edges, the tape does not record the mode |
| **INSTANT REPLAY** (`replay`, **off**) | nothing yet | the ring's path check reads 0.0000 m **by construction** (one pose per step; every 2nd step missed at 0.064 m); `Replay21Tests` 7/7 | no view or VIEW key: it did not pass |
| **Challenge codes** (beyond 5, on) | nothing in the UI yet | `challenge PASS`: 200 round trips, corrupted codes rejected, a decoded-code run records the same tape as the raw seed; `Challenge21Tests` 10/10 | the TITLE CODE field and RESULTS line; no code reads `setup.mutators`, so mutator effects are **unproven** |

## The gates (this session)

- **Suite:** 1620 / 1626 on the first run in Unity (never run before). Two reds were Editor `/api/exec` timeouts (pass alone,
  19/19); four tests had a stale premise (a fixed 20 s burn, a husk behind a wall, a hand check pinned to `--off19`, a run in
  which the stuck-enemy watchdog no longer fires with N21 on) and were corrected, with the reasons in the commit. Final run on
  the frozen code: **1625 / 1626** (876 s), replays 22 / 22, golden 97 / 97; the one red, `AutopilotTests.PerStep_AllocatesNothing`
  (1 allocation under load), passes 8 / 8 twice alone, as HANDOFF warned.
- **Fingerprint:** recorded `FingerprintBaselineN21` once at the freeze (the blocks change the run from checkpoint 2:
  round 3, 35 kills, 4808 earned, 1 alive; NIGHT20 was round 4, 36 kills, 4775, 8 alive). The final Player prints
  **three lines, all EQUAL**: N21 on, `--off19` vs N18, `--off21` vs N20 (`FingerprintBaselineN20`, kept for good). Golden 0 mismatches.
  Replay EQUAL (1 of 1 checkpoints, 488 steps).
- **Smoke:** build 1 `199.5s DONE: all steps passed`. The final build's first smoke failed 3 pad steps (226 s); run again alone it was
  `200.1s DONE: all steps passed`. I read the first as a focus pause (the known cause) but did not prove it.
- **Bench: not run** (CPU read 100 % from other sessions' python jobs at the baseline and at the close; `bench.sh` refuses above 30 %).

## What I decided without you

- Switched on stamp, deadline and grind in config at the end (your D-4..D-6 defaults); `replay` stays off.
- DEADLINE's fuse 25 -> 18 s and burn fuse 10 -> 5 s, to meet the 50 % contract.
- Deleted nothing: I did not dismiss the Editor dialog (you did), and `Assets/_Recovery/` (recovered scenes) is still untracked.

## Open

DECIDE21 (`Research/2026-10-07-wave21/DECIDE21.md`) has the measured state per question. Parked threads untouched.
Persona triage's top asks: see `NIGHT21-LOG.md`. First fixes for NIGHT22: build the six reel scenarios, the DEADLINE fuse bar,
the ARCLINE chip and reticle states, the stamp entrance + T-11, the GRIND smoke step, then a human look.

## Late addendum (13:50)

Two Player reel scenarios were added after the close table: **arcline** ran on the Player (`arclinePull 1, ropeAttach 1`; M-MAGENTA 0 on 239 frames; S12 0 on the pad
pass) but scripts the buy and has no horde, so it is not a contract pass; **slidebite** read 0 bites because the scripted player bounces in place at the
spawn spot (a scenario placement problem; the headless probe still passes). No other number above changed.

## Second late addendum (13:58): SLIDE BITES passes

The slidebite reel's 0 bites was the spawn spot beside a jump pad; from the arena's middle the Player reel reads **`slide bites 3`**. With the 3 / 3 tests, the
`slidebite PASS` probe and the off-test, contract `slidebites` is met: **1 of 7 `passes`**, 4 DONE and 7 SHORT. The ARCLINE still isn't a pass (scripted buy, no horde).
