# NIGHT13 report for GG: run better, play anywhere, and the architecture under it

Tag `u3-n13` on `port/stage3`. The Player is `Builds/Stage1/SKYGRIND.exe` (build 5, code `aaba74c`; TITLE's stamp reads
`SKYGRIND 0.1.0 (aaba74c-dirty)` because two docs were uncommitted when it was built).
Times and numbers are from tonight's `NIGHT13-LOG.md`. Nobody played it: tests, gates, a smoke and 8 captures.

## What you get when you press Play

- **A gamepad plays the game.** Plug in a pad: TITLE selects PLAY, A starts, the sticks move and look (summed with the
  mouse), RT fires, START pauses, B goes back. Menus navigate on the d-pad with a cyan selection frame; WASD no longer
  moves menu focus. The prompts and the GAMEPAD page use the pad's names (neutral, Xbox, or PlayStation in words).
  Rumble is off by default (SETTINGS > GAMEPAD > VIBRATION). The web's pad maths, remap and menus are ported with their
  web tests.
- **CONTROLS** (TITLE and PAUSED): click a key, press the new one; a key in use swaps ("W WAS FORWARD: SWAPPED"); Ctrl
  can't be bound. With a pad connected, a GAMEPAD page remaps buttons. Bindings are saved.
- **SETTINGS** shows more of the web's rows: SLIDE and ARCLINE SWING (hold / toggle), the GAMEPAD tab, UI SCALE,
  RENDER SCALE, plus WINDOW MODE and RESOLUTION. Settings now live in `skygrind-settings.json` beside your profile.
- **The hotbar and the radial:** bottom right, your guns (up to 12) with keys 1-0, FRESH bars and family colours; the
  wheel steps with a short cooldown; hold TAB for the radial, point, release.
- **SAVE & QUIT:** at a round break after round 1 (never downed, never in an exfil), PAUSED shows SAVE & QUIT; TITLE then
  shows RESUME, which puts you back at that break (paused). One slot, used once.
- **First impressions:** a padlock image instead of the empty-box glyph; no orange striped wall ahead of spawn (the gate
  field fades with distance, stripes only in the doorway); white / gold round tally; a labelled POINTS plate; a tip the
  first time a gun goes STALE; CONTRACTS shows rewards and hides the 11 dormant rows behind one line; the first-run lesson
  is one line at the break; TITLE has one line per mode and no AIM ASSIST button (it's in SETTINGS); PAUSED has RESTART
  and a YOUR KEYS card; zone price signs no longer show over RESULTS.
- **Basics:** the version on TITLE and as the log's first line; a local run log (`runs/`, last 20; SETTINGS > RUN LOG);
  COPY REPORT in PAUSED (version, settings, the run log's path, the log's last 50 lines); F3 / F8 / F9 only in the Editor
  or with `-skygrindDev`.

## Numbers

- **Performance (your bar, 6.9 ms worst 1 % on Silent):** final build (5), three 60 s runs on an idle machine: **4.08 /
  3.98 / 4.45 ms, median 4.08 ms** (build 4 before the review fixes: 3.91 / 4.64 / 3.98, median 3.98). Playing (strafe, shoot): 3.87 ms. Last night's 7.19 ms came partly from the bench
  itself: its power-plan poller (PowerShell + WMI every 2 s) ran during every run; with it moved outside the run, the
  u3-n12 composition (build 1) reads 4.81 ms. Build 2 with every perf fix off reads 7.10 ms (tonight's new UI and
  wiring); with them on, 5.42 ms.
- **Where a slow frame goes** (worst 1 % vs a median frame): ~7.5 vs 2.2 ms on the main thread outside our scripts
  (engine work: a Release Player does not split it further), 0.6 vs 0.3 ms in our scripts, one sim step 0.3 ms, 0.5 ms waiting on Present; the GPU
  idles (0.2-0.8 ms); GC near 3 % of slow frames.
- **Pacing (D-44):** capped at 144 the CPU's busiest frames take 2.7 ms; at 240, 2.4 ms. Uncapped stays the default.
- **Reviews:** two read-only reviewers found real bugs, and these are fixed. Rebinding didn't move RESTART / LOCK ON /
  X-RAY or the keys shown on screen. A DAILY could be saved and resumed on another day. A jump on the pad could take a
  MODS card unseen, so the draft now selects nothing until you move (D-52). The rest are listed in HANDOFF section 6.
- **Tests:** 594 / 594, 22 / 22 replays, 91 / 91 golden; `-skygrindGolden` 0 mismatches in the Player; the smoke
  `DONE: all steps passed` with tonight's 5 new steps; the composed fingerprint equal to its baseline in the Player and
  headless.
- **Architecture:** `SurvivalGame.cs` 1,521 -> 320 lines (289 after the split, then the input wiring); the whole sim is `SurvivalSim` (no Unity), run headless in a test
  that must print the same 12 hashes as the u3-n12 game did. Seven builders worked in parallel worktrees.

## Captures (`Logs/shots/n13-*.png`)

1 TITLE · 2 CONTRACTS · 3 CONTROLS mid-rebind · 4 the GAMEPAD page · 5 the spawn view (no striped wall; the hotbar) · 6 the
radial · 7 PAUSED (RESTART, SAVE & QUIT, YOUR KEYS) · 8 TITLE with RESUME.

## What is short, and why

- **IL2CPP trial:** not possible here: this Unity install has no Windows IL2CPP module and the PC has no Visual Studio
  C++ tools. Installing them is your call (D-40 stays Mono; the bar is met on Mono anyway).
- **Windows HighQoS arm:** not run (the bench has no hook for it).
- **The gun bob under REDUCED MOTION:** kept, as the web does (its REDUCED MOTION covers roll, shake, fringing, speed
  lines).
- **Save-and-quit proof:** three seeds restore exactly (round 2's break, round 6's into a HOLD, round 5's); the break
  before an exfil can't be reached by the test script (rounds 6 and 10 are HOLD rounds).
- **Found, not fixed (HANDOFF):** a second run of the same seed in one session plays differently from the first (state
  carries across runs), so RETRY SAME SEED isn't yet "same seed, same run" within a session.

## 10-minute checklist (please)

1. Pad: plug in, start from TITLE without the keyboard; play a round; pause with START, resume with B.
2. Pad: does the stick feel right (dead zones, speed)? Try SETTINGS > GAMEPAD's LOOK SPEED and RESPONSE CURVE.
3. Keyboard: CONTROLS, rebind JUMP, check it works in game, RESET KEYS.
4. Hold TAB: pick a gun from the radial; scroll the wheel; press 1 / 2 / Q.
5. Play to round 2's break, PAUSE, SAVE & QUIT, RESUME: same round, points, guns?
6. Look at the hotbar and the POINTS plate: readable at a glance?
7. CONTRACTS: does the reward column and the order read well?
8. PAUSED > COPY REPORT, paste it somewhere: useful?

## Questions (one each)

- TITLE: with a saved run, should RESUME take the focus instead of PLAY?
- CONTROLS: keep the web's rows for features the port lacks (PHOTO MODE F2), or hide them?
- The GAMEPAD page: neutral labels by default, or Xbox letters when unsure?
- The hotbar: the 5-letter short names (KESTR, PULSE) work, or do you want icons first?
- SAVE & QUIT: come back paused (as now) or straight into play?
- SETTINGS: its shown list now names 42 keys (with the LAB tab): too many for a first look?
- IL2CPP: install the module and VS C++ tools so a later night can try it (D-40)?
- The new copy (mode lines, the lesson lines, "REWARD: THE BADGE", the STALE tip): keep or rewrite?
