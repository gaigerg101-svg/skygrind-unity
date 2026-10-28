# NIGHT8 report: the Demo Day build (`u2a-demo`)

For GG, 2026-10-02. All 17 stage 2a items in `tasks.json` are passing, none cut. Branch `port/stage2`, local tag
`u2a-demo`. Nothing is pushed or merged. Times are local, from `date`. Each item's checks are in `NIGHT8-LOG.md`.

## What you can play

- **Windows:** `Builds/Stage1/SKYGRIND.exe` (built 7:50 PM, smoke passing).
- **Browser:** the Unity Web build in `Builds/Web/`, zipped for itch as `Builds/skygrind-web-u2a.zip` (not uploaded).
  The section below has how to run it locally.
- A run now plays like this:
  - **Title:** PLAY SURVIVAL, SETTINGS, AIM ASSIST, QUIT. SETTINGS has 17 of the web's settings in four tabs, plus a LAB
    tab. Settings save across launches.
  - **Rounds:** round 1 is husks; swarmers come from round 3; **SPITTERS from round 6.** A spitter keeps its distance
    and lobs a 3-glob volley that leads you. Acid puddles stay on the floor. **Punch a glob in reach to send it back.**
  - **Things to buy:**
    - the ARMORY's THUNDERCLAP
    - **five perk machines:** VIGOR, MEND, PLATE, QUICK, GRIP
    - **kill drops:** DOUBLE POINTS, OVERCLOCK, SHOCKWAVE, STASIS, REFUND
    - **MODS drafts** in the web's draft rounds: 1 of 3, with one paid reroll
    - **THE FORGE** in THE GRINDER: 5000 forges the gun in your hand
  - **Feel:**
    - shots and kills shake the camera (the web's trauma)
    - impact sparks; speed lines
    - the freshness tier by the weapon name
    - **PERFECT PARRY:** a punch in the last 0.15 s of a wind-up heals you to full and freezes 0.15 s
    - **PEAK FADE:** no new spawns while the peak's enemies are still on you
    - markers on the last three enemies, and edge arrows for any you can't see
  - **Death:** DOWN says what killed you and the damage of the last 3 s. **R restarts in about 4 ms**, from DOWN or
    from RESULTS. RESULTS opens with RESTART focused and shows your personal best.
  - **READY:** hold E in a break, away from anything for sale, to start the round now (the web's, from round 3).

## How to play it

- **Windows:** double-click `Builds/Stage1/SKYGRIND.exe`.
- **Browser, locally:** run `node Tools/serve-web.cjs 8090` in the repo, then open `http://127.0.0.1:8090/` in Chrome.
- **Browser, on itch:** upload `Builds/skygrind-web-u2a.zip` as an HTML project. The files are gzip with the
  decompression fallback, so no server settings are needed. Not tried on itch.
- **The Web canvas is a fixed 960x600** (Unity's default template). For a full-size game, press the blue fullscreen
  button under it. On itch, set the viewport to 960 x 600 and turn on itch's fullscreen button.
- **Keys:**

  | Key | Action |
  |---|---|
  | WASD | move |
  | SPACE | jump |
  | SHIFT | dash |
  | C | slide / slam |
  | LMB | fire |
  | RMB | zoom |
  | F | punch / parry |
  | E (hold) | buy, READY |
  | 1-3 | weapons |
  | Q | last weapon |
  | ESC | pause |
  | R | restart (DOWN / RESULTS) |

- **Debug keys, which work in this build:**
  - F3: overlay
  - F8: opens every gate
  - F9: skips to the next round

  Tell peers to leave F8 / F9 alone, or a run's result means nothing.
- **The LAB:** SETTINGS, then LAB. There are five toggles, all OFF by default:

  | Toggle | What it does |
  |---|---|
  | CLOSE-KILL HEAL | a punch or gauntlet kill, or any kill within 4 m, heals 15; regen slows to 12 HP/s (from 25) |
  | FRESHNESS TEETH | a FRESH weapon pays x1.5 style; a new DULL tier (freshness under 2.5) pays nothing |
  | STYLE RAMP | the style multiplier climbs from 1.2 to 3 over 1 s of air, slide or wall ride; the ground resets it |
  | READY FROM ROUND 1 | READY works in every break |
  | SQUADS TIGHTEN | squads come x0.96 faster each round, never under 1 s apart |

  The toggles apply from the next run. A run with any of them on is stamped "LAB · <toggles> · NOT A PERSONAL BEST"
  on the results and never stored as a best. The numbers live in `Design/unity-config.json` (`design.lab`), with why
  the web has none.

## What shipped (commits on `port/stage2`)

| Item | Commit | What |
|---|---|---|
| U2-01 | `992d51f` | verify on arrival: the NIGHT7 baseline green |
| U2-02 | `02ab591` | zero-allocation sim steps (EventBus, Loadout, Director, HuskBrain, AimAssist, Husks: 0 B over 600 steps with 30 enemies), a ProfilerMarker per system, an instrumented bench |
| U2-03 | `ed5dd59` | a HUD that allocates nothing per frame; the TMP fallback asset is no longer rewritten by Play |
| U2-04 | `7b4b5ef` | SETTINGS (settings.js ported; the web's keys; PlayerPrefs) |
| U2-05 | `e3f19d2` | R restart, the death recap, damage-direction arcs, personal bests |
| U2-10 | `2bb1802` | the SPITTER, its globs and acid, enemy aim by your speed, glob reflect |
| U2-11 | `cd8c817` | perk machines: the starter five |
| U2-12 | `da208df` | kill drops and power-ups |
| U2-13 | `ec95eac` | MODS drafts (10 of 19 mods) |
| U2-06 | `317ed9b` | feel parity: trauma shake, impacts, speed lines, the freshness tier |
| U2-07 | `06ac6a8` | the Unity design config and PERFECT PARRY |
| U2-08 | `6d03d9e` | PEAK FADE |
| U2-09 | `e51bc67` | enemy markers |
| U2-14 | `98c6e5c`, `7b24926` | the Unity Web build (the WebGL boot, URL checks, link.xml, the local server) |
| U2-16 | `597dfd9` | the LAB, and READY (the port never had it) |
| U2-17 | `f16ecd5` | THE FORGE |
| U2-15 | the tagged commit | the close: these docs, HANDOFF.md, PORT-STATUS.md, tasks.json |

## The numbers

- **Tests:**
  - `tests: 237 / 237 passed, 0 failed`, `replays: 16 / 16`, `golden tests: 55 / 55` (8:01 PM).
  - That is 55 new tests since stage 1's 182. Movement is still bit-exact.
- **The Player smoke** (`SKYGRIND.exe -skygrindSmoke`, 7:53 PM): `DONE: all steps passed`, 0 exceptions. It covers:
  - round 1 cleared in 9 s; round 2
  - ARMORY and VIGOR bought
  - a death with the recap; RESULTS with the best line
  - R from RESULTS (4 ms) and from DOWN (3 ms)
  - round 6 with a spitter's volley
- **The Windows bench** (60 s, 24+ husks chasing a god-mode player, 1920x1080 windowed, vsync off):
  - frame time: median 3.35 ms, p95 5.32 ms, **worst 1% 8.37 ms**, 282 fps mean
  - conditions: Balanced on AC at every 2 s poll
  - GC: 8 collections in 16,919 frames
  - FrameTimingManager: CPU median 3.34 ms, GPU median 1.79 ms
  - stage 1 measured 9.35 / 18.47 / 40.25 ms, but on a busy machine (CPU at 100% for half the run), so this is better
    but not a like-for-like comparison
  - about 3.9 KB a frame is still allocated somewhere outside the sim steps. A Development Player profile would find it
    (next steps).
- **The Web build** (rebuilt at 8:12 PM with everything above; the U2-14 and U2-15 sections of `NIGHT8-LOG.md`):
  - On a 1920x1080 canvas, Chrome holds its 60 fps cap with 24 husks: median 17.00 ms, worst 1% 17.00 ms, 1 GC in
    30 s. Chrome rendered on this laptop's integrated Radeon 890M, not the RTX.
  - The smoke passes with 0 console errors, and PLAY by hand reaches RESULTS (on the 6:24 PM build).
  - TITLE in about 6 s from a local server; 19.7 MB zipped.

## Cuts and what is partial (and why)

No item was cut. These parts of items are partial. Each one waits for a system that isn't ported yet:
- **Perks:** 5 of 14 (the other nine need the arsenal, momentum or more guns).
- **MODS:** 10 of 19 (the rest need the arsenal, momentum or blasts).
- **THE FORGE's family statuses** (the THUNDERCLAP's STAGGER): they roll, but land nothing until the arsenal (U3). The
  KESTREL's forged headshot bonus works.
- **The PROTOCOL** (and its free forge).
- **The "closer" extras of section 6 were not done:**
  - the Development Player profile
  - the CONTROLS page
  - a NEW: SPITTER debut card
  - the dash tint

## For you to decide (details in HANDOFF.md section 5)

- **D-01, what peers play on Oct 7:**
  - My recommendation is the Unity Web build. It runs in Chrome at the browser's cap with the smoke passing.
  - Keep the exe as the fallback.
  - Nothing is uploaded.
- **D-03:** the LAB's five proposals are ready for an A/B at Demo Day.
- **D-09 (new):** the Mobile URP asset is upgraded by Unity on every build. I restored it each time; commit it once if
  you agree.
- **D-10 (new):** three proposed CLAUDE.md lines. I did not edit CLAUDE.md.
- **D-05:** the build turned `UnityConnectSettings` cloud on again. It is left unstaged.

## A 10-minute Demo Day checklist (one question per new feel point)

Play one run on the defaults first, then one with the LAB. Ask the question right after the moment, not at the end.

| Min | Do | Ask |
|---|---|---|
| 0:00 | SETTINGS: sensitivity, FOV, comfort | "Could you get the mouse and FOV where you like them in under a minute?" |
| 0:30 | Round 1: shoot husks | "Do the shots and kills feel like they land (shake, sparks, the hitstop)?" |
| 1:30 | Punch a husk as its wind-up flashes | "Did you notice the PERFECT PARRY (full heal, the freeze)? Was the timing readable?" |
| 2:30 | Buy VIGOR or QUICK at a machine | "Was it clear what the machine sells and what it did for you?" |
| 3:00 | Grab a kill drop | "Did you see the drop, and did you know what it gave you?" |
| 3:30 | The first MODS draft (in a break) | "Did any of the three cards change how you wanted to play?" |
| 4:30 | Let the last enemies hide (markers) | "Could you find the last enemies without searching?" |
| 5:00 | Fight through a peak (PEAK FADE) | "After the big wave, did the game give you a breath, or did it drag?" |
| 5:30 | Watch the weapon name's FRESH / USED / STALE | "Did the tier make you switch weapons?" |
| 6:00 | F9 to round 6: SPITTERS; punch a glob back | "Could you read the volley coming, and was the glob reflect satisfying?" |
| 7:00 | THE FORGE, if the run has THE GRINDER open and 5000 points (a 10-minute run may not: then ask about the plan) | "Did the forged gun feel stronger? Did you notice the '+' and the brighter gem?" (or: "Would you save 5000 for it?") |
| 7:30 | Die on purpose | "Did DOWN tell you what killed you?" |
| 8:00 | R | "How long did the restart feel? Would you start another run?" |
| 8:30 | LAB: CLOSE-KILL HEAL + STYLE RAMP, one short run | "Did healing from close kills push you forward? Did air time feel rewarded?" |
| 9:30 | LAB: READY FROM ROUND 1 + SQUADS TIGHTEN | "Did READY make the breaks better? Did the later rounds feel too dense?" |

## Not checked (and where it would matter)

- The LAB's FRESHNESS TEETH, STYLE RAMP and SQUADS TIGHTEN by hand. EditMode tests only; try each once.
- A forged kill and the gem's glow by eye. The damage is tested.
- The Web build on itch.io, and on a machine other than this laptop.
- Long runs (past round 10) in the Player.
