# Wave 54 (NIGHT54, THE LOOK PASS; 2026-10-20 14:09 - 16:5x MST; tag u3-n54)

**Ask (GG ~14:05, verbatim in NIGHT54.md and OWNER.md):** the visuals across the game; the lighting and flicker bugs fixed; the movement
cleaner and less choppy without changing the mechanics; THE WICK relatively easy with its half-arena attack removed and the rest unchanged;
each boss its own palette and lighting identity with JoJo-style colour turns; the guns' boss-fight colours by colour theory; the UI untouched.
A 15-minute plan into a 2.5 h build window, hard cap 2.75 h (D-1: go 14:09, cap 16:54).

**Prompt shape:** pasted request, no kickoff file; the lead + 3 read-only scouts (boss 1, flicker, choppiness) in the first 15 minutes;
the shared commit at 0:15 (design.n54: wick54 sim row, light54 / grade54 / move54 view rows; a fifth builder D added decor54 at 0:41);
5 builders in worktrees (W, L, G, M, D), picked by cherry-pick in 4 rounds; a Player build is ~1 min, so 5 builds (r1, r2, r3, freeze,
freeze2) and ~45 proof reels fit. Decisions: DECIDE54 D-1..D-17.

**Shipped (Builds/n54-freeze2, 90b62818; all five rows ON, -skygrindOff54 = NIGHT53's game):**
- wick54: FLARE out (every leg, the cap flare too); HP 330, chip 0.6, burst 1, rests 3 / 3.5 / 1.5 s, P2 gate 0.55, 1 core punch.
  n47human 40 seeds: median 95.0 -> 45.5 s, deaths 51 -> 2, taken 189 -> 72, P2 / P3 40/40 (Logs/n54/wick-base.txt, wick-after.txt).
- light54: the shared muzzle / impact light split; fixture slots with hysteresis and cross-fades; the landmark eased; tell strobes
  (7-14 Hz) to swells; Juice38's flicker one flash; z-fight lifts 3 cm; THE STACK's white albedo (Stone.shader reads vertex colour, not
  _BaseColor) + 7 boss views. a154 THE STACK flicker 55 -> 2.
- grade54: a second Volume per boss + 0.55 s turns + cyan-family gun rims; ICEBREAKER / WYRM / LOOM / RIVAL on; THE WICK and THE CROWN
  OFF (D-16: the graded mean hue 1-14 deg from the danger red).
- move54: kill freezes shorter (gun kill 0.085 -> 0.05 s), slow-mo ease-in, step-pop spring, interpolated enemy yaw, the pad's assist pull
  spread, AUTO resolution half steps, VSync default on for new profiles. a354 held 0.250 -> 0.150 s, jitter 0.246 -> 0.191.
- decor54: per-level decor accents; cyan off pure decor (rims ~128 -> 0, trims, lenses); subtle on screen.
- verbs37 27 / 21 / 31 / 31 / 31 (nothing lost); golden 0 mismatches; fingerprints plain / --off53 / --off54 EQUAL.

**SHORT:** G (two bosses ungraded); Z (the smoke not clean: run 1 a pad step with a real gamepad, run 2 paused by focus; the full
batch suite 2277 / 2277 passed after the tag). The step-down smoothing unmeasured.

**Reaction:** (none yet)
