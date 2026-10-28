# NIGHT14 report for GG: THE GAME PLAYS ITSELF

**Anchor:** `u3-n14` on `port/stage3`. Details, every gate's line and the agents' decisions: `NIGHT14-LOG.md`.

**The design bet (not player demand):** the web game already had a bot (RIVAL's brain). Ported exactly and pointed at
SURVIVAL, it plays the real game deterministically, and that gives five tools without changing how the game plays: a
balance farm, a living TITLE, replayable run tapes, a death replay, and a run map with a ghost to race. You decide
whether any of them stays.

## What changed

- **The web's bot, ported line for line** (botBrain.js, astar.js, reach.js, follower.js, navGraph.js) with a new oracle
  (5 golden sets recorded from the web code: 0 mismatches). navGraph.js builds on the Survival map unchanged (848 nodes).
- **THE AUTOPILOT:** that brain drives your own body in SURVIVAL (it picks the nearest enemy in sight, shops in breaks
  from a priority list, takes the first MODS card, holds HOLD rings). No aim assist, real HP, and it records nothing.
- **THE FARM:** `SKYGRIND.exe -skygrindFarm <runs> <difficulty> <seedStart>` plays seeded autopilot runs headless, one
  line per run; `node Tools/farm-report.cjs` turns them into `Reports/FARM-n14.md` and `Reports/farm-n14.html`.
- **The kiosk and a living TITLE:** `-skygrindKiosk` (the web's `?demo=1`: big control hints, 60 s idle back to TITLE,
  no QUIT). The attract loop: after 30 s on TITLE the autopilot plays behind it with a slow orbit camera; any input stops it.
- **THE TAPE:** every run's inputs are recorded (~159 KB a minute; the last 20 beside the run logs);
  `-skygrindReplay <tape>` replays one bit for bit and says EQUAL or the first step that differs.
- **WHAT GOT YOU:** on DOWN, the last 4 seconds replay from an orbit camera, the killer outlined and named, the last half
  second slowed; any key skips. A RECAP row in SETTINGS turns it off.
- **THE ROUTE and the SHARE CARD:** RESULTS shows your run as a map (speed in cyan, kills, the DOWN mark, round ticks);
  SAVE CARD writes a PNG to Pictures\SKYGRIND; COPY SEED copies the seed code.
- **The GHOST and SPLITS:** on a seed you've finished, your best run races you as a faint cyan runner, and each round's
  start shows "ROUND 5 · -12.4 s" against it.
- **Fixed:** RETRY SAME SEED is now the same run within one session (the enemy pools, the shot ids and the director's
  streams carried over from the last run).

## The farm's top observations (for you; nothing was changed)

300 AUTOPILOT runs in the Release Player (100 per bot difficulty, seeds 1-100, real HP): `Reports/farm-n14.html` (open
it in a browser) and `Reports/FARM-n14.md`. "Difficulty" is the bot's skill (the web's easy / normal / hard bot), not the
game's.

> **Correction (2026-10-05, after the close): items 1, 4 and 5 are mostly the bot's limits, not the game's.** An
> analysis of the JSONL and the bot's code found:
> - The bot counts itself inside a HOLD ring only within half the ring's radius (`AutopilotInput.cs:73`), stops short of
>   the round-6 ring, and freezes inside it (can't dodge acid).
> - It never fires the rocket or the rail it buys (`design.autopilot.guns`), never calls an exfil, and keeps trying to
>   reach a perk machine it can't get to in time.
>
> NIGHT15 fixes the bot first and re-runs the farm. The design signals that survive are in `NIGHT15.md`'s proposals
> item. Read the list below as the bot's numbers.

1. **Round 6 is a wall for the bot.** 42-44 % of runs in every difficulty end in round 6, and 37 of the 41 runs that hit
   the time cap alive were stuck there: the first HOLD round, with the ring never held by the game's test. 173 of the
   259 downs came in a HOLD round.
2. **The bot's skill shows:** mean round 6.7 (easy), 9.5 (normal), 12.7 (hard); accuracy 20 %, 35 %, 47 %. No run reached
   an exfil.
3. **What downs it:** HUSK 110, ACID 71 (the most on hard), SWARMER 40, SPITTER GLOB 38.
4. **The shotgun carries:** 43 % of all kills (bought in 72 % of runs, around round 3.6), the nailgun 29 %, the revolver
   26 %; the rail is rare (9 % of runs, round 16 on average).
5. **Points pile up:** a run ends with 24,818 of 32,492 earned points unspent on average, and 55 of 300 runs bought no
   gun and no perk. The bot's shopping list is short; the economy may also be loose.

## The numbers

- **Tests (Unity, 8:31 AM):** `678 / 679 passed, 0 failed, 1 skipped`, replays 22 / 22, golden tests 96 / 96; then the
  skipped one (the autopilot tape) turned on: TapeTests 7 / 7.
- **Golden in the Player:** `39 vector sets (48094 records), 22 replays (22 exact); 0 mismatches` (5 new bot sets).
- **Fingerprint in the Player:** `EQUAL to the baseline` (12 hashes, 7200 steps in 1.16 s). **Tape replay:** `EQUAL`.
- **The autopilot is deterministic:** the same seed twice gives 30 of 30 equal hashes in the Player smoke, and the farm's
  lines from the Player (Mono) equal the headless harness's (CoreCLR) for every shared seed (44 of 44 checked mid-farm).
- **The tape:** about 159-163 KB a minute of play.
- **The bot's map:** the Survival nav graph is 848 nodes; it builds in about 3-4 s once and is cached.

## What is short, and why

- **The clean slate on a long session (U3-27.2):** RETRY SAME SEED gives the same run in the Player (two runs equal, 12
  of 12). The smoke found and fixed two more carry-overs (SAVE & QUIT failed after the first fix; the pad's aim assist
  leaked into a test run). One remains: after the smoke's many steps, a seed-1 run differs from a fresh game's after
  ~30 s. It is in the game layer, not the sim (a test proves the sim is clean). Not found by the time limit.
- **Captures:** none taken tonight (TITLE over the attract, the kiosk hints, WHAT GOT YOU x2, RESULTS with THE ROUTE, the
  ghost). The Editor was hung from 12:50 AM to the 7:55 AM reboot; after it the time went to the Player checks and the
  two smoke fixes. The share card PNG was looked at (`Logs/shots/n14-share-card-b2.png`).
- **Benches:** the WHAT GOT YOU cost delta and the final Silent series: not run (you were at the machine; plan Balanced).
- **Why so much is short:** the Unity Editor hung at 12:50 AM ("Waiting for Unity's code to finish executing") and
  stayed hung until the PC rebooted at 7:55 AM. Everything in between was checked headless (dotnet), not in Unity.

## A 10-minute checklist

1. Leave TITLE alone for 30 s: the attract starts. Move the mouse: it stops at once, then PLAY.
2. Die once (a husk): watch WHAT GOT YOU. Is the killer obvious? Is 5 seconds too long?
3. On RESULTS: look at THE ROUTE; press SAVE CARD and open Pictures\SKYGRIND.
4. RETRY SAME SEED: the faint cyan runner is your last run; a split shows when round 1 starts.
5. Copy a tape path (COPY REPORT) and run `SKYGRIND.exe -skygrindReplay <path> -batchmode -nographics -logFile r.log`.
6. Open `Reports/farm-n14.html` in a browser.

## One question per new screen

- **The attract:** keep it on a normal TITLE after 30 s (D-54), or only in the kiosk as the web had it?
- **WHAT GOT YOU:** worth the ~5.5 s before DOWN, or shorter / off by default?
- **THE ROUTE:** is the map readable at a glance, and does the card say what you'd share?
- **The ghost:** cyan at 30 % (D-53): right colour, right strength?
- **The farm:** should a farm diff gate every gameplay night (D-55), or stay a report?
