# NIGHT22-REPORT.md (one page for GG)

**Outcome:** 0 of 9 contracts passed on a Player (the forge guessed 6). DEADLINE's bar is the only row whose Player reel was GREEN with a RED pair; its A-5 chase check misses at round 15 (61 % against the 50 % cap), so the row stays SHORT. The night was cut by a computer crash around 5:40 PM; the n22-4 reels (the first run with every row wired) never produced results, and the suite, the final build, `close.sh` and the `u3-n22` tag were not done. Nothing was pushed or merged. Latest commit: `287adc9` on `port/stage3`; latest Player: `Builds/n22-4` (built, never reeled).

**Per row (all numbers from runs this session)**
- ARCLINE: headless PASS (pad Y yank, second press attaches, 30.7 m/s; row off reads RED). Chip with three looks (READY cyan, COOLDOWN arc + seconds, NO TARGET reticle at 40 % alpha, never red) is built and compiled; not seen on a Player. Look at it first.
- THE STAMP + T-11: Player entrance 233 ms; exit and the grade-card overlap (4205 px2) were red on n22-3; fixes integrated, not re-measured.
- DEADLINE: Player GREEN (bar, 42 px, pulse, +N pops, burn flash); row-off RED. A-5: round 10 46 %, round 15 61 %.
- THE GRIND: headless PASS (telegraph, records survive a restart, tape replays EQUAL); smoke step and reel never run.
- INSTANT REPLAY: camera error 0.0347 m (limit 0.05); view and a V / pad-Y prompt exist, off in config.
- Challenge code: headless PASS (pad fills a code, mutators active, same tape twice). Ghost on the DAILY: headless PASS with the row on. Both Unity UIs unseen.
- THE DUSK CLOCK: the first three Player reels were flat (the sky dome is a dark strip on this walled map); the fix dims the key light and ambient with the round. Not re-measured. Rounds 8 and 13 are THE HUNT's and lean the sky to red, so the reel uses 1 / 7 / 15.
- Haptics, directed steps: built and unit-tested; never run on a Player. TRICK CHAIN: built, off (72.0 vs 42.0 style in the harness). OVERDRIVE and the spawn beam: not integrated.

**Not verified:** every Unity-side widget except the DEADLINE bar and the stamp's entrance; the suite; the bench (CPU was 100 % from other sessions all night); fingerprint on the final build.

**DECIDE22 for GG:** D-5 (DEADLINE at round 15 is 61 %: retune the burn rule, accept it, or check at round 10 only); D-3/D-6/D-7 as filed. Next session: reel `Builds/n22-4` (`-skygrindReelOnly arcline2,stamp,deadline,grind,replay,code,ghostdaily,dusk,haptics,directed`, then `-skygrindOff21` for RED and `-skygrindReelPad`), then the suite.
