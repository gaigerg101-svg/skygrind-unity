# DECIDE21: what only GG decides (NIGHT21; fill the Answer column, the next night acts on it first)

The run builds every NIGHT21 feature behind its own switch (`design.n21.<row>.enabled`; `-skygrindOff21` = NIGHT20's
game). After playing, GG keeps or drops each one here. Nothing on this page is decided in code.

| # | Question | The run's default | Answer (GG) |
| --- | --- | --- | --- |
| D-1 | Keep THE ARCLINE in Survival (the web's 5500 gadget: swing, zip to big enemies, yank small ones)? | on | |
| D-2 | The ARCLINE's pad button (v3: Y = SPECIAL + ARCLINE, as NIGHT19 bound it), and the yank's cooldown (crowd control: your long-cooldown rule) | the web's rule for the shared button; yank 15 s, swing 0.4 s (the web's) | |
| D-3 | Keep SLIDE BITES (the slide hits what it passes through)? | on | |
| D-4 | Keep THE STAMP (round grades with a round P on the run P's rule, the inked rank letter, the RESULTS card off POINTS)? | on | |
| D-5 | Keep DEADLINE (a kill-or-die timer twist round)? How often? | on, as one more twist | |
| D-6 | Keep THE GRIND (the web's endless mode, on SURVIVAL's core, reshaping every wave) on TITLE's MODE row? | on | |
| D-7 | THE GHOST (NIGHT14's) unlocked on the DAILY after a first clear, with AHEAD / BEHIND on RESULTS (beyond 1, if built): keep? | on if built | |
| D-8 | The moonshot INSTANT REPLAY, if it landed: keep? | off unless it passed its contract | |
| D-9 | Parked threads (aged 2+ nights): S2 / S4 / S6 (gun niches, SWAP-CANCEL, the shotgun's 60 % share): a balance night, or leave? | parked | |
| D-10 | Parked: BLOOD PRICE / MEND in-round healing (THE MARATHON M-4 0.61) | parked (NIGHT20-REPORT section 4) | |
| D-11 | Parked: the DRYDOCK crow's nest (a plate for a ladder, or a reward perch) | parked | |
| D-12 | Parked: teach the bot the BULWARK and turn its row on | parked | |
| D-13 | Install a memory plugin (obra's episodic-memory)? | not installed | |

## What the run did with each default (filled at the close, 2026-10-07; the Answer column stays GG's)

| # | State at the close (all measured this session; "not seen" = no Player reel scenario or human play) |
| --- | --- |
| D-1 | THE ARCLINE is **on** (`design.n21.arcline`): headless `arcline PASS` (attaches 10/10, peak 30.7 m/s after a dash into the hook, a yanked husk moved 8.6 m), `Arcline21Tests` 5/5. Its look (rope, node glows, reticle) compiles and is wired but is **not seen**; the red reticle and the HUD chip are not built. |
| D-2 | Yank cooldown 15 s, swing 0.4 s as built. The shared Y button follows `Core/Gamepad.cs` as NIGHT19 bound it; the web's SPECIAL-vs-ARCLINE rule was **not** re-derived tonight (not checked). |
| D-3 | SLIDE BITES **on**: `slidebite PASS` (a slide through 3 enemies damaged 3), 3 tests. Not seen. |
| D-4 | THE STAMP **on**: round grades exist (`roundGrade`; autopilot distribution A 12, B 11, C 3, D 3, S 3 over 32 rounds) and RESULTS lists them. The **magenta rank-letter square is fixed in the Player**: M-MAGENTA 0 px on every frame of both reel passes (NIGHT20's frame read 9487 px). The inked stamp entrance and the grade card off POINTS (T-11) are **not built**. |
| D-5 | DEADLINE **on**, weight 1 of 5 twists. THE CHASE KITE at round 10 survives 30.3 s of 63.2 s (48 %, 10 seeds) with fuse 18 s / burn fuse 5 s (tuned from 25 / 10, which gave 60 %). The HUD fuse bar is **not built**. |
| D-6 | THE GRIND **on** TITLE's MODE row: headless median wave 8 (10 seeds, 300 s); the core's heights hash differs on every wave. Compiles; **not seen**; no smoke step, no reel. |
| D-7 | THE GHOST on the DAILY: **not built** (beyond row 1 not taken). |
| D-8 | INSTANT REPLAY: **off**. Core ring + recorder only (the path check reads 0.0000 m by construction, one pose per step); no view, so it did not pass its contract. |
| D-9 .. D-12 | Parked as written; nothing built. |
| D-13 | Not installed. |
| extra | Challenge codes (beyond row 5): Core + harness only (200 round trips, corrupted codes rejected, the decoded-code tape equals the raw-seed tape). No TITLE / RESULTS UI, and mutator effects are not proven (no reader of `setup.mutators` was found). |
