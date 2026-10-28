# NIGHT23-LOG.md

Zero: 2026-10-07 8:09:54 PM (first `date`). Close table 1:50 or later (9:59 PM); hard cap 2:15 (10:24 PM).

## U3-37.1 Start (8:10 PM)
- Base 6ab7689, tree clean except UnityConnectSettings.asset.
- Editor gate: no Unity process and port 7800 closed after the crash; started once with Start-Process at 8:12:42 PM (PID 46944). `editor-ready.sh` polls for 5 minutes.
- CPU at start: 43 % (78 python processes from other sessions).

## U3-37.2 Reel (8:16-8:31 PM, 0:06-0:21)
- Editor ready 8:15 PM. n22-4's Player threw NullReferenceException in `Ui.BuildCode22` at startup (RESULTS not built yet): game never started; the first ON invocation was wasted (closed by PID). Fixed in 3af57a1 (`BuildCode22Results` after `results = Screen`), Editor Play 0 console errors; rebuilt Builds/n23-1 (build 1 of 4).
- Reel invocations used: 4 of 12 (1 dead, ON, RED, PAD). All on Builds/n23-1; logs `Logs/player/n23-reel-n23r1-{on,red,pad}.log`.
- ON (keyboard): arcline2 PASS (arclinePull 1, ropeAttach 1, peak 31.7 m/s, chip Ready / Cooldown 15 s / NoTarget reticle alpha 0.40); stamp PASS (entrance 233 ms, exit 183 ms, grade card vs POINTS 0 px2 over 210 frames); deadline PASS (bar from frame 1, 42 px, pulse 301 frames, pops 3, burn 1); grind (waveBreak 1x, WAVE banner 1x, telegraph 240 frames, death to Results, best wave 1, "FIRST RUN ON RECORD"); code PASS (mutators [glassCannon, speedDemon], x2 / x2 / x1.2, RESULTS "CODE 004P-PGW3-00KD"); ghostdaily PASS (none before a finish, loaded after, "BEHIND YOUR GHOST BY 0.3 s (ROUND 1)"); directed PASS (beacon step list 1 -> 3, hunt not exercised: design.adds.hunt off); haptics keyboard pass (nothing logged without a pad) PASS; dusk share 0 / 0.43 / 1.00; replay: NO VIEW (RED): flow Down, offered False, clip 0 frames (builder W's cause: the DOWN clip was taken only inside a sim step; fixed in dd7789d, not yet on a Player).
- RED (`-skygrindOff21`): every row prints its RED reason (arcline off, stamp none + overlap 30803 px2, deadline no bar, grind / replay skipped, code mutators [] x1, ghost raced before a finish, haptics / directed off, dusk share 0 / 0 / 0).
- PAD: arcline2 PASS via virtual pad Y (arclinePull 1, ropeAttach 1, 31.7 m/s); haptics PASS (shockwave 3 steps, yank 2, attach 2, kill 1, down 1; VIBRATION off: log unchanged); stamp PASS; code PASS; grind and directed as keyboard (directed shows the pad glyph).
- Metrics (`feel-metrics.py`) for on and pad running in the background.

## U3-37.3 Triage (8:35-8:45 PM)
| Row | Failing check on Builds/n23-1 | Owner | Size | Result |
| --- | --- | --- | --- | --- |
| replay | NO VIEW on the Player (DOWN clip taken only inside a sim step; the reel kills between frames) | builder W | S | fixed dd7789d; n23-2: VIEW offered, 740 ring frames, 1200 replay frames, back to Results; design.n21.replay turned ON (fingerprint EQUAL with it on) |
| deadline | M9: 13 pop-ins on the "+N" pop (first frame at full alpha); M11: the bar's DANGER fill unlisted | lead | S | 0.12 s fade-in (UiDeadline22.cs), role listed (Tools/feel-roles.json); n23-2 reel: pops 3, PASS; pop-ins 10 (all older elements: Markers 0-7 and the Points pop) |
| stamp | STAMP-T read older RESULTS "Stamp" elements (117-133 ms); T11 meter found no groups | lead | S | STAMP-T reads only "Round stamp": 5 of 5 at 267 ms PASS; T11 groups by path: RED 168 / 168 frames, GREEN 0 / 168 |
| directed | S12: the break line read "HOLD E" on the pad pass (suffix cached once at build time, an older bug) | lead | S | the suffix rebuilds when the device changes (Ui.cs BreakReadyRefresh); compiled, not re-reeled |
| dusk | LUM must fall | none | - | PASS: thirds 29.0 > 27.0 > 23.4 (RED 29.0 > 30.0 > 29.7 on n22-3) |
| all | M9 pop-ins / entrance times and M11 roles on OLDER elements (Buy prompt, HP damage ghost, Markers, Down menu, prompt wheel, Points pop, hurt / damage vignettes, round-clear text, Style decay bar, Exposed): present in NIGHT22's C1 reels too | - | L | not tonight's elements: T-12 stays open (listed, not fixed) |

## U3-37.5 DEADLINE round 15 (decision, 8:45 PM)
Measured (NIGHT22, 30 seeds, KITE): round 10 = 46 % of normal, round 15 = 61 % (cap 50 % at BOTH); retunes fuse 12 / burnFuse 4 -> 60 %, 6 / 2 -> 59 %. Chose (A) keep 18 s / 5 s and call the contract SHORT with these numbers; rejected (B) a burn that can kill (the contract says deadlineBurn never kills) as a rule change. Then, with 1 h of slack, one bounded attempt (C): a per-round decay of the kill refill, by builder S (see below).

## Accuracy (A-items) so far
- A-1 replay camera error 0.0347 m worst of 3 seeds (limit 0.05); A-2 pad Y carries SPECIAL + GRAPPLE (v3), virtual pad Y fires the yank on the Player (arclinePull 1, 31.7 m/s); A-3 identical tape hashes for one code (code2) and the code's mutators active in RunStats (Player reel); A-4 scripted round P prints roundGrade p=true once (Player stamp reel); A-5 round 10 46 % / round 15 61 %; A-6 not run (CPU not clear: other sessions' python jobs); A-7 0 B headless (not reproducible).

## Freeze, smoke, suite (9:00-10:00 PM)
- Code freeze 9:00 PM at 22b3c79 (no sim-changing default on: fingerprint EQUAL as configured, `--off22`, `--off21`, `--off19`; N22OnPath false; design.n21.replay on with fingerprint EQUAL, harness fingerprint 12 hashes). Final build `Builds/n23-3` (build 3 of 4). Final reels (7 of 12 invocations): ON all ten rows and PAD seven rows green again; metrics: M-MAGENTA 0 on every frame, M10 0, T11 0 of 168 / 48 / 1116 frames, STAMP-T 5 of 5 (250-267 ms), LUM 29.0 > 26.3 > 24.2, S12 0 on the pad pass (the directed break line fixed). M9 on tonight's elements: 0 pop-ins on the round stamp, replay, dusk, directed; remaining: DEADLINE burn flash (instant by design) and the Replay prompt label; older elements' pop-ins and M11 roles are T-12.
- Player smoke on n23-3, alone (run 1 at idle 0 s: 4 pad steps failed, the first two from a person at the machine; run 2 at idle 177 s): every non-pad step PASS (clean slate EQUAL, tape EQUAL, autopilot 30 / 30 and 60 / 60 hashes, attract, ghost); FAIL `pad only: TITLE ('PLAY' selected), A started a run: Title` and `grind22: the d-pad reached the MODE row False` both runs. Cause not found in the time (suspects: NIGHT22's TITLE changes). So the contract's smoke step is not met: GRIND row evidence = reel + harness.
- Suite: started 9:38 PM (run_tests EditMode, nothing else running). At 10:17 PM (39 minutes, budget 15) it had not completed: the Editor sat at 100 % CPU and its Pipeline timed out on every status call. NO suite totals this night (focused files green: N22Shared 6 / 6, Code22 13, Trick22 6, Replay22 3, Directed22 7, Motion22 1, DuskClock22 4; Stamp22 5, Deadline22 2, Grind22 4, Arcline22 5 at NIGHT22). The `u3-n22` tag was NOT created: the suite and the smoke did not print.

## CLOSE TABLE (printed after 1:49 / 9:59 PM; cap 2:15 / 10:24 PM)
| Row | Status | Evidence / measured reason |
| --- | --- | --- |
| U3-37.1 start | DONE | Editor started once after the crash (8:12 PM), ready 8:15; CPU 43 %; tasks.json parses; N22SharedTests 6 / 6 |
| U3-37.2 reel | DONE | n23-1 ON / RED / PAD all rows; n22-4's startup NRE found and fixed (3af57a1); ledger in NIGHT22-PROOF.md; 4 of 12 invocations before the fixes |
| U3-37.3 triage | DONE | table in this log |
| U3-37.4 fix round 1 | DONE | replay clip (dd7789d, VIEW on the Player), +N pop fade, STAMP-T / T11 / M11 meter fixes, break-line key word; one rebuild n23-2 |
| U3-37.5 DEADLINE | SHORT | decision A logged; builder S's refill-free test: round 10 40 %, round 15 59 %; contract needs both <= 50 % |
| U3-37.6 fix round 2 | DONE | the final rebuild n23-3 (3 of 4 builds); only the break-line fix; reels green |
| U3-37.7 freeze and verify | SHORT | fingerprint / replays / focused tests green; Player smoke fails the 2 TITLE pad steps (twice); suite: see below; bench not run (CPU not clear, 58-78 python processes) |
| U3-37.8 close | SHORT | docs written (report, handoff, feel-playtest, proof ledger, memory); `u3-n22` NOT tagged (suite never finished in 39 minutes; smoke 2 pad steps red) |

8 of 9 NIGHT22 contracts GREEN on a Player (arcline, stamp, grind (reel), replay, challenge, ghost-daily, dusk-clock, haptics); DEADLINE SHORT. NIGHT23 CLOSED
