# NIGHT12 report for GG: the look, the feel and the meta core

One night (7:06 PM - after midnight, 2026-10-03 / 04), a building night: seven background builders in their own git
worktrees, the lead wiring and gating. Tag `u3-n12`. Details and every printed line: `NIGHT12-LOG.md`.

## What you get when you press Play

- **The look** (`c21a6cc`): the map now takes SSAO and point lights (it couldn't before), post.js's grade, bloom and
  vignette from config, chroma pulses (hurt, dash, rank up, speed), the low-HP vignette, the web's height fog, light
  pools at the machines / box / relays / radio / power switch, a muzzle flash light and kill pops, motes, blob shadows.
- **Enemies** (`1ea530e`): one SKYGRIND/Enemy shader: a rim that grows with distance, a white hit flash, a dissolve on
  death (no gibs: D-29), an x-ray through walls (SETTINGS > HUD, or X in play); a flinch on hits; the kill confirm ring
  in your gun's colour; particles drawn instanced.
- **The gun's life** (`fad5a8d`): breath, bob, sway, strafe / slide / wall tilts, air lift, dash pull, land dip, the
  KESTREL's cylinder, the THUNDERCLAP's pump (and its missing parts), a tracer per pellet, shells, the impact flare,
  land puffs, slam rings, wall-ride sparks, the dash edge tint.
- **Ceremonies and type** (`01b21e6`): Chakra Petch + Barlow Semi Condensed everywhere; the round number burns in (tally
  strokes for 1-5); RESULTS counts up card by card and slams the grade; the HP ghost and the points roll; a living
  title flying over the map. Damage numbers exist, off by default (SETTINGS > HUD).
- **The meta** (`6fda847`, `77edbfb`, `171db51`): a JSON profile; **CONTRACTS** (39: the web's 24 + 15 new; the first,
  FIRST FIFTEEN, unlocks the DAILY in round 2); the **PAYOFF strip** on DOWN / RESULTS; **HEAT 1-3** on the run setup
  (GLASS CANNON, + SPEED DEMON, + NO REGEN; an exfil opens the next rung); today's **DAILY** (a twist mutator and a
  first mod; the first finished run counts; a streak); **MASTERY** per gun (3 tiers, a finish each) on WEAPONS;
  a first-run guide (one lesson per round, a light on the next thing; SETTINGS > GAME turns it off); tips; RETRY SAME
  SEED and COPY SCORE LINE on RESULTS.

## Numbers

| Check | Result |
|---|---|
| EditMode | `tests: 501 / 501 passed, 0 failed`, `replays: 22 / 22`, `golden tests: 91 / 91` (u3-n11: 437 / 22 / 90) |
| Harness / Player golden | 34 vector sets, 22 replays exact, 0 mismatches (both) |
| Player smoke | `[smoke] 107.1s DONE: all steps passed`, 0 exceptions (new: a contract + its unlock; a DAILY on HEAT 1) |
| Bench (60 s, 24 husks) | **Balanced** (u3-n11's plan), machine quiet: three back-to-back runs at 4:28-4:32 AM, worst 1% **6.84 / 7.00 / 4.64 ms, median 6.84 ms** (median frame 2.46-2.55 ms); a single run at 4:18 gave 6.90 ms. **Silent** (your bench plan; I set it for three runs at 4:36-4:40 AM and put Balanced back): worst 1% **7.13 / 7.25 / 7.19 ms, median 7.19 ms, over the bar** (11:36 / 11:39 PM single runs: 7.62 / 7.38 ms). u3-n11: 5.55 ms on Balanced. A Balanced run at 01:03 (10.15 ms) is void: your machine's 1:00 AM jobs (patric2's nightly batch, a local llama-server at ~9 cores) were running until about 4:16 AM |

## Captures (`Logs/shots/`)

`n12-map-before.png` (the u3-n11 Player, the spawn view) and `n12-round-after.png` (same spot tonight, round 2's burn-in,
a husk dissolving, a far husk) · `n12-title.png` · `n12-contracts.png` · `n12-kill.png` (a THUNDERCLAP kill: ring,
dissolve, light pool, the lesson card) · `n12-wallride.png` · `n12-results.png` (mid-tally with the PAYOFF strip) ·
`n12-slide.png` (the Slide state, but facing a wall) · `n12-round.png` (round 1's clear, an early try).

## What is short, and why

- **The bench misses 6.9 ms on Silent** (median of three 7.19 ms), so U3-25.2, U3-25.12 and U3-25 stay
  `in_progress`. On Balanced the median of three runs is 6.84 ms, with one run at 7.00 ms (identical runs swing by up
  to 2.4 ms, so one run doesn't decide it). Which
  plan the bar means is your call (D-34: the memory note says you bench on Silent; the 6.9 ms bar came from u3-n11's
  Balanced 5.55 ms). The GPU has room (1.4 ms); the spikes are CPU-side. Moving the costliest effect to a quality tier
  needs a second build, over tonight's cap of one. Next: profile a Development Player and tier what it names (SSAO
  itself ran already at u3-n11; the new per-frame light, Volume and property-block updates are the suspects).
- **Captures: 15 of a 12 cap.** A retried command re-took three shots; the lesson is in memory. The mid-slide shot faces
  a wall; a husk at exactly 30 m and the x-ray through a wall weren't captured.
- **11 web contracts can't progress yet** (bosses, ARCLINE, affixes, Thumper): they sit at the end of the list.
- **LOW GRAVITY** needs a Mover change no item named: listed, not playable.

## 10-minute checklist (please)

1. TITLE: does the fly-over and the type feel like a shipped game? Too dark, too busy?
2. PLAY round 1-2: the gun's sway / bob / slide tilt: too much, too little?
3. Kill a few husks: is the rim readable at range, the dissolve and the kill ring satisfying?
4. Clear round 1: does the round burn-in land like CoD's?
5. Die: RESULTS' tally pace (any key skips) and the PAYOFF strip: does it make you want another run?
6. CONTRACTS: 39 enough? Did FIRST FIFTEEN unlock the DAILY fast enough?
7. Run setup: try HEAT 1 (after an exfil) and today's DAILY: clear rules?
8. WEAPONS: do the finishes read? Is 25 kills for tier 1 right?
9. A fresh profile's first run: one lesson per round: helpful or noisy?
10. SETTINGS > HUD: DAMAGE NUMBERS and ENEMY X-RAY: keep defaults (off / on)?

## Questions (one each)

The look: brighter or darker? · The gun's life: dial up or down? · Kill confirm + dissolve: keep? · Round ceremony:
right size? · RESULTS: faster tally? · CONTRACTS: the first unlock at 15 kills right? · HEAT: should its points bonus
also be spendable (D-35)? · DAILY: one counted run per day OK? · MASTERY: rewards right (D-36)? · First run: keep the
guide on by default? · The bench: OK to bench on Balanced, and to tier SSAO if needed (D-34)?
