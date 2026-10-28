# NIGHT22-LOG.md

Zero: 2026-10-07 3:03:56 PM (first `date`). Freeze 6:14 PM. Close table at 7:04 PM or later. Hard cap 7:34 PM.

## U3-36.1 Start (3:04 PM)
- Editor gate: `Tools/night/editor-ready.sh` printed `EDITOR READY (poll 1)`; window "SKYGRIND-unity - Survival - Windows, Mac, Linux - Unity 6.3 LTS (6000.3.7f1) <DX12>" (no dialog). `dismiss-dialog.ps1` not needed.
- CPU reading at start: 100 % (other sessions' jobs, as NIGHT21 saw). The bench waits for under 30 % (A-6).
- `set_autotick` already on (16 ms); `editor_focus` done.
- GG's answers: none read (DECIDE22 not answered; no word from GG).
- `NIGHT22-FEATURES.json` copied from the seed (9 rows, all `passes: false`).

## U3-36.2 The shared commit (3:10 - )
- Written: `Core/N22Blocks.cs`, `Game/SurvivalSim.N22.cs`, `Game/SurvivalGame.N22.cs`, `Game/FeelReel.N22.cs` (phase F, `SafeSpot`, `Preconditions`), `Tools/SimHarness/Harness.N22.cs`, `Assets/Tests/EditMode/Game/N22SharedTests.cs`; rows in both config copies; tape format 7; `FingerprintBaseline.N22OnPath = false`; hooks in Ui.cs (BuildN22Hud / RenderN22Hud), Init / Render, Run.cs.
- Premise changed in two NIGHT21 checks (not weakened): `N21SharedTests.TheTape_Format6...` and `Harness.N21.cs` asserted the current format is 6; it is 7 now, and the format 6 / 5 tapes are derived from a format 7 one (`AsFormat6`).
- Headless gates: n22shared PASS; n21shared PASS; fingerprint EQUAL as configured / `--off22` (N21 baseline) / `--off21` (N20) / `--off19` (N18).

## CLOSE TABLE (7:52 PM, after the crash; the 4:30 cap, 7:34 PM, had passed. Every row is from a run this session; "not checked" means no run)
| Row | Status | Evidence / measured reason |
| --- | --- | --- |
| U3-36.1 | DONE | Editor gate poll 1 ready 3:04 PM; CPU 100 % logged; contracts copied; tasks.json parses |
| U3-36.2 | DONE | shared commit c21435c: n22shared PASS, fingerprint EQUAL as configured / --off22 / --off21 / --off19, golden 58/58, Tape 13/13, N20/N21/N22 shared tests green; meters T11 / LUM / STAMP-T with self-tests (5db9d55) |
| U3-36.3 ARCLINE | SHORT | headless `arcline2` PASS (pad Y yank, ropeAttach, peak 30.7 m/s; --off RED); Arcline22 tests 5/5; Unity chip compiled and built (n22-3/n22-4) but its Player reel never ran: the n22-4 reel run died on the crash |
| U3-36.4 STAMP + T-11 | SHORT | Player n22-3: entrance 233 ms PASS; exit 3533 ms and grade card overlap 4205 px2 RED; fixes integrated (3aa00bf, a5255fe), not re-measured |
| U3-36.5 DEADLINE | SHORT | bar GREEN on the Player (n22-3: bar from frame 1, 42 px, pulse 301 frames from 5.0 s, +N pops 3, burn flash 1; RED row-off: no bar). A-5 chase: round 10 46 % PASS, round 15 61 % FAIL (retunes 12/4 and 6/2 gave 60 % / 59 %): contract SHORT |
| U3-36.6 GRIND | SHORT | harness grind2 PASS (telegraph, records restart, GRIND tape format 7 replays EQUAL); smoke step and reel written, never run |
| U3-36.7 REPLAY | SHORT | A-1 camera error 0.0347 m over 3 seeds (limit 0.05) PASS; view + VIEW prompt wired and compiled, never seen on a Player; design.n21.replay stays off |
| U3-36.8 challenge / ghost / dusk / haptics | SHORT | challenge: code2 PASS headless (pad fill 163 presses, mutators active, same tape, RESULTS line; --off21 RED); ghost: ghost2 PASS only with the row flipped locally; DUSK CLOCK: LUM flat on n22-1..n22-3 (29.0 > 29.7 > 29.1 vs RED 29.0 > 30.0 > 29.7), fix (key light and ambient dim) built into n22-4, never measured; haptics: Core 13/13 pure tests, no pad reel |
| U3-36.9 accuracy | SHORT | A-1 0.0347 m; A-2 pad Y carries SPECIAL + GRAPPLE (v3), harness PASS; A-3 same-tape hashes equal (code2); A-4 scripted round P prints roundGrade p=true once (stamp2 PASS); A-5 46 % / 61 %; A-6 bench not run (CPU 100 % from other sessions); A-7 0 B over 21600 steps, not reproducible headless |
| U3-36.10 leftovers | SHORT | 1 directed steps (render-only, harness PASS, never on a Player); 2 T-12: Hud18EnterTime 0.15 -> 0.20 + announcer plate fix, not re-measured; 3 T-10: no glyph labels found, no change; 4 TRICK CHAIN built, off (trick2: 72.0 vs 42.0 style, row off 42.0); 5 OVERDRIVE and 6 spawn beam: the builders were cut by the crash, not integrated; 7 shine: S's sheen band (D-7), unseen on a Player |
| U3-36.11 close | SHORT | the cap (4:30) passed in the crash: no suite run (0 of 2), no final build, no close.sh, no u3-n22 tag; builds used 4 of 8, reel invocations 11 of 24 |

Switches at the stop (both config copies): design.n22 ghostDaily / duskClock / haptics / directed ON, trickChain / overdrive OFF; N22OnPath false (fingerprint EQUAL to the N21 baseline). Building share was not logged. The NIGHT22-REPORT.md has the one-page view. NIGHT22 CLOSED

## Emergency stop (5:40 PM, GG: "computer is about to die")
- State at the stop: HEAD has the shared commit, meters, DUSK CLOCK (light + ambient + sky), all builders' slices integrated and wired (stamp, deadline, ARCLINE chip, haptics, directed, GRIND, replay view, challenge code, ghost-daily, TRICK CHAIN off); design.n22 ghostDaily / duskClock / haptics / directed on, trickChain / overdrive off. Builds used 4 of 8 (Builds/n22-4 is the latest). Reels for n22-4 (on / red / pad) were still running; their output is in Logs/ (untracked), not verified.
- Measured so far: DEADLINE reel GREEN on the Player (n22-3: bar from frame 1, 42 px, pulse 301 frames, +N pops 3, burns 1) and RED with the row off; STAMP entrance 233 ms but exit and T-11 were red on n22-3 (fixes integrated, not re-measured); DUSK LUM flat on n22-3 (fixed by dimming the key light and ambient, not re-measured); A-1 replay camera error 0.0347 m (3 seeds); A-5 DEADLINE round 10 46 %, round 15 61 % (SHORT); A-7 not reproducible headless (0 B over 21600 steps); A-6 bench not run (CPU 100 % from other sessions).
- Not done: the suite (never run this night), the final build, the close table, docs (REPORT, HANDOFF, FEEL-PLAYTEST), tag u3-n22, NIGHT22-PROOF ledger entries. Builders M (overdrive) and W (spawn beam) may have uncommitted/unintegrated commits in .claude/worktrees.
