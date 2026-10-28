# NIGHT23-REPORT.md (one page for GG)

**Outcome:** 8 of 9 contracts now pass on a Player (NIGHT22: 0; the forge guessed 6 for NIGHT22). Each row has a RED line (the same scenario with the row off) and a GREEN line from this night
in `NIGHT22-PROOF.md`. The ninth, DEADLINE, misses its chase check at round 15 (59-61 % of normal against a 50 % cap). The first reel found that NIGHT22's last Player threw an NRE at startup and
could not run at all (a UI build-order bug, fixed in `3af57a1`). Final Player `Builds/n23-3` (code `22b3c79`); 3 of 4 builds, 7 of 12 reel invocations. Nothing pushed or merged.

**Per row (what you can do, its switch, the number)**
- **THE ARCLINE chip** (`design.n21.arcline`): buy it, press Y at a husk: READY / COOLDOWN (seconds) / NO TARGET (reticle 40 %, never red). Pad Y fires the yank (arclinePull 1, ropeAttach 1, 31.7 m/s). M10 0, S12 0, M-MAGENTA 0.
- **THE STAMP** (`design.n21.stamp`): the inked grade slams in over 233 ms (STAMP-T 5 of 5, 250-267 ms) and leaves in 183 ms; RESULTS grade card clear of POINTS (T11 0 of 168 frames; RED 168 of 168); a scripted round P prints `roundGrade p=true` once.
- **DEADLINE bar** (`design.n21.deadline`): the number is the bar (42 px), pulse in the last 5 s, a cyan +N pop (now fades in over 0.12 s), burn flash. **Chase check: round 10 46 %, round 15 61 % (a refill-free, 6 s fuse reached 59 %): SHORT.** Needs your call (D-5).
- **THE GRIND** (`design.n21.grind`): WAVE banners, no ROUND wording, pillar telegraph, best wave kept across restarts. Reel GREEN; the Player smoke step cannot run (see below).
- **INSTANT REPLAY** (`design.n21.replay`, **now ON**): DOWN offers VIEW (V / pad Y); 740 ring frames, 1200 replay frames, back to RESULTS; camera error 0.0347 m (limit 0.05). Fixed this night: the DOWN clip was only taken inside a sim step.
- **Challenge CODE** (`design.n21.challenge`): TITLE CODE item (pad-fillable), RESULTS line, the code's mutators are active (x2 / x2 / x1.2), same tape twice.
- **DAILY ghost** (`design.n22.ghostDaily`): none before your first DAILY finish; then AHEAD / BEHIND YOUR GHOST by n.n s.
- **THE DUSK CLOCK** (`design.n22.duskClock`): the sky and the light sink with the round; LUM 29.0 > 26.3 > 24.2 (RED: flat). Look at round 15: is it too dark? (`lightScale` 0.55, `ambientScale` 0.6.)
- **Haptics** (`design.n22.haptics`, pad): distinct patterns for shockwave, yank / attach, kill, DOWN; VIBRATION off silences all.
- **Directed steps** (`design.n22.directed`): THE BEACON's step list (the hunt's needs `design.adds.hunt`, off: not seen).
- TRICK CHAIN is built and off (72.0 vs 42.0 style in the harness); OVERDRIVE and the GRIND spawn beam are parked.

**What felt worse / is wrong:** (1) the dusk reel first read flat (the sky is a dark strip on this walled map), so the clock now also dims the key light and ambient: unseen by a person. (2) **The Player smoke fails 2 pad steps on TITLE every time**
(`pad only`, `grind22`: A on PLAY did not start a run); NIGHT21's smoke passed. Possibly a NIGHT22 TITLE change (the CODE item, the ghost gate): bisect first next session. (3) M9 pop-ins and M11 roles remain on OLDER elements (T-12), plus tonight's DEADLINE burn flash and the Replay prompt label.

**Not verified:** the bench (CPU never clear); the hunt's directed list; any of it by a person; the full suite did not finish in 39 minutes (the Editor stayed busy), so there are no suite totals and the `u3-n22` tag was NOT made; only focused test files ran (all green).

**For GG (DECIDE22):** D-5 DEADLINE round 15 (accept SHORT, check at round 10 only, or allow a burn to kill at high rounds); keep INSTANT REPLAY on (it is on now)? D-7 the rank letter's sheen (a pale band now). Next session: bisect the smoke pad steps, then T-12, then the bench when the CPU clears.
