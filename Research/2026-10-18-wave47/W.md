# NIGHT47 builder W: research notes and round 0 measurements (THE WICK, the climb; G1-G9)

Written 2026-10-18 22:53-23:30 by builder W. Each research line is marked **Verified** (I read the page tonight) or **Reported** (the
lead's digest or a secondary source I did not open). Every number under "Measured" was printed by the harness named beside it, on the
worktree at 871caf33 (the NIGHT47 shared commit), every row as configured (the ON game, no `--off`), nothing forced on.

## Research

- **Verified** (github.com/NoelFB/Celeste, Source/Player/Player.cs, read tonight): Celeste's `WallJump(dir)` ASSIGNS the vertical speed
  (`Speed.Y = JumpSpeed;` then adds a lift boost); it never adds to a falling speed. `SuperWallJump` assigns `-160`. `JumpGraceTime = 0.1`
  (coyote), `WallJumpCheckDist = 3` px (a wall jump from 3 px away), `WallJumpForceTime = .16` (the stick is held off the wall for 0.16 s
  after a wall jump), `VarJumpTime = .2`, `HalfGravThreshold = 40` (half gravity near the peak while jump is held). The jump buffer's
  duration lives outside that file: not checked.
- **Verified** (maddymakesgames.com/articles/celeste_and_forgiveness, read tonight): the article's own numbers are only "wall jump 2 pixels
  from a wall" (a quarter tile) and the super wall jump "I think it's 5 pixels"; coyote time, jump buffering, half-gravity peak, jump and
  dash corner correction, semi-solid popping, lift momentum storage and stamina refunds are described without numbers. (The source above
  says 3 px, the article 2 px: I use the idea, "accept a wall jump from a short distance off the wall", not the pixel count.)
- **Reported** (ULTRAKILL wiki via a search result tonight, page not opened): Gabriel's second fight changes the rule at the transition
  (50 % less damage taken during it), adds light-projection swords so attacks continue while the real swords are thrown, and colour marks
  the phase. The lead's digest (NIGHT47.md section 10) has the first fight's page Verified: phase 2 at half health with a colour change, a
  line and improved versions of the same attacks.
- **Reported** (NIGHT47.md section 10, the lead's Verified reading): tells 1-1.5 s, at most ~2 s for heavy attacks, never overlapping;
  punish windows longer than one combo plus the travel to the boss; weak points obvious, only exposed in their window, inviting; Doom
  Eternal's Marauder: one colour on the wind-up of the attack you punish; Titanfall 2's ~25 % slack on a safe jump (NIGHT5).

## Measured: the reach table (harness `n47wick reach`, Tools/SimHarness/Harness.N47.Wick.cs)

A 40 m tall test wall (face x = 3000, normal -x) and a floor slab put in the survival map's world as their own layer; the real Mover in
the real sim; a press = one step's `pressed`; "gain" = max feet over the feet where the move started.

| move | from | max height gain (m) | events |
|---|---|---|---|
| jump (held to the apex) | rest | **1.58** | Jump |
| double jump (best press, 0.45 s = the apex; swept over 2-90 steps) | rest | **2.88** | Jump, DoubleJump at 1.58 |
| wall jump (second press at the apex, the stick into the wall) | rest, at the wall | **3.17** | Jump, WallJump at 1.58 (a WallJump on 89 / 89 press steps) |
| jump, wall jump at its apex, double jump at that apex | rest, at the wall | **4.46** | Jump, WallJump 1.58, DoubleJump 3.16 |
| ride jump-off, look 0 deg at the release | a ride at 10.4 m/s (and at 20 m/s: the same) | **1.15** | WallRideStart, WallJump |
| ride jump-off, look 0 deg, + double jump at the apex | a ride at 10.4 / 20 m/s | **1.50** | + DoubleJump |
| ride jump-off, look +45 deg at the release only (level while riding) | a ride at 10.4 / 20 m/s | **2.16** | WallRideStart, WallJump |
| ride jump-off, +45 deg at the release only, + double jump | a ride at 10.4 / 20 m/s | **2.51** | + DoubleJump |
| ride jump-off, look +45 deg the whole 1 s ride and at the release | a ride at 10.4 / 20 m/s, attached at 2.00 m | **2.16** over the release, **4.65** over the attach | the ride rose 2.49 m in 1 s (ride38 / ride40's pitch drift) |
| same + double jump | | **2.51** over the release, **5.00** over the attach | |
| jump held along the wall from standing (stick 20 deg into it) | rest | 2.71 | Jump, WallRideStart at 1.54, WallJump at 1.56 (a ride attached and the release jumped off) |

Read: the ride's speed (10.4 or 20 m/s) changes NONE of the jump-off heights; only the look pitch does (1.15 at 0 deg, 2.16 at +45 deg),
and the double jump after a jump-off adds only 0.35 m (it fires near the jump-off's apex while still rising little). A ride needs a run-up
(ride40's intent: >= 1 m over the ground, >= 4 m/s, the stick toward the wall).

## Measured: the chains (harness `n47wick chain`, `n47wick stackchain`)

**One wall, the stick into it, a press at each apex (5 presses after the jump):** Jump, WallJump, DoubleJump, WallJump, DoubleJump, WallJump:
gains per press +1.58 / +1.58 / +1.29 / +1.58 / +1.29 / +1.58 (feet 8.92 m after 6 presses). The wall jumps never lose height (each SETS
the up speed: MovementMath.WallJumpVelocity assigns `upSpeed`), but a wall jump pushes off the wall, so every second press is a double jump
away from it, and the count ends at 3 (wallJump.maxPerAirtime): no 4th wall jump on one wall.
**The chimney (two walls 4 m apart, the look turned after each wall jump):** Jump then WallJump x5: +1.58 / +1.58 / +2.23 / +2.24 / +2.24 /
+2.24 (chimney43 refills on a new wall, its chain's `chainUp`): it RISES the same each jump (+-0.5 %), so G2's "the same height per jump"
already holds on a chimney.

**THE STACK, today's panels (section 2, panels 3 -> 8; Sim37W's stack; ride at 10.4 m/s; steer at the next panel's 40 % point):**

| hand | gap 1 | gap 2 | gap 3 | gap 4 | gap 5 | made |
|---|---|---|---|---|---|---|
| look LEVEL while riding, +45 at the release, release at 50 % (attach height over each panel's bottom) | +0.45 | -0.58 | FELL | | | 2 / 5 |
| look level while riding, +45 at the release, release at 80 % | +0.45 | -0.12 | -0.68 | -1.25 | FELL | 4 / 5 |
| look +45 the whole ride, release at 50 % | +0.45 | -0.08 | -0.31 | -0.45 | -1.02 | 5 / 5 (sinking) |
| look +45 the whole ride, release at 80 % | +0.45 | +0.88 | +1.46 | +1.48 | +1.48 | 5 / 5 (stable) |

**GG's "slipping further down each time" is reproduced and its cause measured:** a jump-off with the look up only at the release gains
2.16 m (the reach table), the panels rise 2.80 m each, so each attach lands **0.56-0.57 m lower on its panel** than the last (80 % hand:
+0.45, -0.12, -0.68, -1.25, fell) until the capsule misses the face. Only a hand that keeps the look up during the ride (ride38's drift,
2.49 m/s) AND rides to the panel's far end keeps its height. The climb is a ratchet against the player unless he does one hidden thing for
the whole ride (M-2: no silent limits).

## Measured: the sloppy climb pilot on today's STACK (harness `n47wick sloppy`, the RED baseline for G1)

Wick37Pilot's climb from the stack's mouth to the cap, no boss, invulnerable, 90 s cap (W-cards.md section 5's errors; the press jitter is
delay-only, 0-240 ms, since the pilot cannot press before its own decision):

| hand | seeds | the cap | climb time median / p90 | retries | the deepest fall |
|---|---|---|---|---|---|
| CONTROL (Wick37Pilot's own skilled hand: the look up on a low ride) | 10 | 10 / 10 | 20.5 / 20.5 s | 0 | 0 panels |
| SLOPPY | 40 | **9 / 40** | 67.4 / 89.6 s (of those 9) | 1221 | **15 panels** (822 falls of more than 3 panels: today's ledge ARCS catch few falls) |

FIRST try per gap (ok / seeds that reached it), SLOPPY, 40 seeds: 0->1 24/40, 1->2 30/40, 2->3 38/40, 3->4 27/40, 4->5 29/40, 5->6 33/40,
6->7 26/39, 7->8 28/38, 8->9 29/35, 9->10 22/32, 10->11 16/28, 11->12 15/21, 12->13 10/14, 13->14 10/11. The bottom third's first try is
60-95 % (G1 asks >= 95 %), the top third's 71-91 % on the few seeds that got there (G1: >= 80 %). Every try: 0->1 219/292 (75 %).
Input windows per gap (G1: none under 150 ms): not measured yet (R1).

## Measured: today's climb segments as ratios of the reach (harness `wick37 stack`, `wick37 gaps`)

Today's stack (design.n37.wick37.stack): 15 panels, rise 2.80 m (every gap), the ledges at 7.0 / 22.7 m, the cap at 38.4 m; ledge gaps
+1.40 m over the panel before; panel end to next start 5.66 m (chord), the ledge gaps 16.0 m. The move every segment teaches is the ride
jump-off at +45 deg.

| segment | vertical gap (m) | / reach of a +45 jump-off, look up at the release only (2.16) | / + double jump (2.51) | / look up the whole ride (4.65 over the attach) | NIGHT47 band (bottom <= 55 %, middle <= 70 %, top <= 80 %) |
|---|---|---|---|---|---|
| panel to panel (all thirds) | 2.80 | **1.30** | 1.12 | 0.60 | bottom FAIL, middle FAIL, top FAIL (even the best hand: 0.60 > 0.55 at the bottom) |
| panel to ledge | 1.40 | 0.65 | 0.56 | 0.30 | bottom FAIL (0.65), middle PASS, top PASS |

`wick37 gaps` (NIGHT37's idealized leap: ride from the low line with the look up the whole ride, every release point 20-100 %): every gap
made at 72 % of the release points with margin +1.84 m (up 10.4), and the LEVEL control (pitch 0) also makes every gap (margin +0.89 m) in
that idealized ride: the harness prints FAIL on its own rule "none by the level control" (12 failing) since NIGHT40's rides changed (not
investigated further: it is the skilled hand; the stack chain above is the human one).
Horizontal reach of a jump-off: **not checked** (R1 adds it to `n47wick reach`; the 5.66 m chord is made in every chain above).

## Measured: why "you can barely damage it" (read from config, not a run)

design.n37.boss37.chipScale = **0.1**: every hit outside a window (a cracked plate pierced from a ride, a parried ember, the CYAN window)
lands 10 % of its damage; NIGHT47.md section 3 asks >= 30 %. And the bar is LOCKED per leg (design.n37.wick37.boss.legLock [0.6, 0.3, 0]):
fire on it in leg 1 and the bar stops at 60 % until the player stands on ledge 2. The pool is 600.
The human-model pilot's baseline (the lead's message, run by the lead's helper, not re-run by W; Research/2026-10-18-wave47/H47.md,
cherry-picked): `n47human --boss wick`, 40 seeds, rows as shipped: 40 / 40 killed, median 72.1 s, p90 81.9 s, min 51.5 s, hip hit share
40 %; with `--no-melee` never killed in 300 s (GG's "barely damage it even while firing consistently"). Caveat from the lead: it fights THE
WICK on the survival map, not n46-foundry's room, with the reaction delay applied only on the ground; W points it at n47-foundry's
three-phase fight (the policy hook `H47PolicyWick` in Harness.N47.Wick.cs, `ctx.tell` set for the new attacks) in round 1.

## Round 1 (2026-10-18 23:21 - 2026-10-19 00:30): measured with the rows forced on (harness only; no Player)

All runs: worktree commits 8dc299d9 .. 776f83f9. "rows on" = `--rows wick47,climb47,wickTop47` (climb alone: `--rows wick47,climb47`).
The human pilot fights on the survival map's stack at (2600, 0, 0), which is the same stack n47-foundry builds (the stack is config, not
level data); the room's door and walk-in are not part of these numbers.

**climb47 on today's stack (rise 2.8 m everywhere):** the stack chains (n47wick stackchain) all 5 / 5 with the same height per jump: look level,
release 80 %: release +1.00 m (the catch line) every gap, attach -0.15 .. -0.18 m (RED: +0.45, -0.12, -0.68, -1.25, fell). The sloppy pilot
10 seeds: the cap 8 / 10, median 28.6 s (RED 2 / 10). Tried and measured worse (kept OFF by their numbers): setUp 12 (attaches crept up
0.3 m a gap and flew over a panel: sloppy 0 / 10); THE CATCH from above 1.0 with the line at 0.5 (the 50 % chains fell at gap 2).

**The new stack (wick47: rise 2.2 / 2.5 / 2.8 m per section, 12 panels, ledges after panels 3 and 7, the cap at 27.3 m):** first tried
1.15 / 1.5 / 1.7 (the ratio bands against the 2.16 m reach): the control hand never rode gap 1 -> 2 (0 / 220 tries): a jump-off rising 2.16 m
from a 1.15 m step arrives with the feet over the next panel's top - 2 m (ride40 asks the face 2 m over the feet): too low a step is as
unrideable as too high a one with 3.5 m panels. 2.2 / 2.5 / 2.8 (40 seeds, n47wick sloppy): the CONTROL 40 / 40 in 23.2 s; the SLOPPY 11 / 40
in 90 s (median 30.0 s of those), FIRST try bottom third 0->1 19/40, 1->2 26/40, 2->3 15/38; middle 3->4 38/39, 4->5 19/29, 5->6 19/23,
6->7 17/20; top 7->8 20/20, 8->9 14/16, 9->10 13/15, 10->11 6/6 (top 87-100 %: G1's 80 % met; bottom 39-65 %: G1's 95 % NOT met). The net:
no fall deeper than one section once a ledge was stood on. Ablation (10 seeds each, one error at a time, W47ABL): the casual hand alone
already misses 1 -> 2 on its first try 10 / 10 (an early release, 35 % of the panel); the press delay alone breaks the floor mount (0 -> 1:
6 / 10; a press that lands when the panel is already touched is a wall jump OFF it: the trace at t 1.5 s). Also tried and worse: the guided
set-up (12.5 m/s cap): control 20 / 20 in 15.5 s but sloppy 8 / 20; THE CATCH from above 2.5 + ride40's face-height rule waived on climb
panels: control 0 / 20 (reverted).

**The fight (n47human, 40 seeds, rows on):**

| numbers | median | p90 | min | max | phases (median start s) | band |
|---|---|---|---|---|---|---|
| rows off (the lead's baseline) | 72.1 | 81.9 | 51.5 | - | - | OUT |
| gate [0.65, 0.40], chip 0.35, one punch | 76.0 | 92.6 | 49.2 | 124.5 | P2 28.1, P3 56.2 | OUT |
| gate [0.60, 0.40, 0.15], chip 0.30 | 79.9 | 100.1 | 60.0 | 109.6 | P2 33.9, P3 61.8 | OUT |
| gate [0.55, 0.40, 0.15], chip 0.30, 2 punches to finish | **92.5** | **110.9** | **71.2** | 166.6 | P2 38.2, P3 66.3 | **IN** |
| same, the SLOPPY climb in P2 (W47SLOPPY=1), the boss attacking | **113.0** | **169.4** | **74.6** | 205.0 | P2 38.2, P3 86.5 (P2 median 49 s, max 135 s) | **IN** |

The last run: 40 / 40 killed, 40 / 40 reached P3 (the coordinator's caution 1: the sloppy hand finishes P2 with the boss attacking), core
punches 80 of 82 crack-opens (G7: >= 70 %), launches 10.1-16.5 m (median 13.9 m) with launchDist 15 (now 16: not re-measured), 0 landings off
the cap, **0 falls into the shaft in P3 over 40 seeds** (G8). Re-run at the final numbers (launchDist 16, commit 776f83f9; the skilled
climb): median 92.5 s, p90 110.9, min 71.2 (IN); 80 / 80 openings punched; launches 8.8-15.9 m, median 14.5, **76 of 80 inside 12-18 m**;
0 landings off the cap; 0 falls into the shaft in P3.

## Round 2 (2026-10-19 00:33-01:10; the lead's a147 on n47-r0b: P3's open core seen far under the glass, the punch false)

- **P3 placement:** at the P2 -> P3 change THE WICK now jumps to its cap circle (capY + 3 m over the glass, 7 m off the axis) at once (it rose
  from under the glass for ~3 s: the reel forced the open core there). Not re-run in a Player.
- **The fight lines (G5):** 7 lines through BossLine40 (design.n47.wick47.numbers.lines; once each a fight, 2.0 s + 0.3 s a word).
- **The sim's hit-stop on the core punch:** Hitstop(0.22 s, "wick47CorePunch"): fired 80 of 80 punches (n47human, 40 seeds).
- **EMBER CURTAIN:** habit "ground" (a climber on a ledge or the floor in P2): 125 curtains in 40 fights (0 with the camp habit).
- **G1 geometry tried (n47wick sloppy, 20 seeds each), all worse than round 1's [2.2, 2.5, 2.8], so reverted:** ride40's face rule eased
  to 0.9 m on climb panels: control 0 / 20; bottom rises 1.0 m with 4.4 m tall, 10 m long panels and ledges after 2 / 6: control 0 / 20,
  sloppy 3 / 20 (first tries 0-25 % at the bottom); 10 m bottom panels alone: control 0 / 20 (it stalls at the ledge gap), sloppy 10 / 20,
  bottom first tries 20-60 %. The bottom third stays SHORT; the per-section height / length fields stay in Wick37Stack (empty = off).
- **n47human, 40 seeds, final round 2 numbers:** median 97.9 s, p90 119.0, min 71.4 (IN); 80 / 81 openings punched; launches 72 / 80 in
  12-18 m (median 14.5, min 3.2: a launch into the cap wall); 0 off the cap, 0 falls into the shaft. With the SLOPPY climb in P2: median
  125.1, p90 185.1, min 68.1 (OUT: p90 over 180 by 5 s; P2 median 56.7 s, 260 curtains; with the curtain on "any" at weight 0.6: 119.5 /
  194.7; round 1 without a firing curtain: 113.0 / 169.4).

## Round 3 (01:12-02:30; a147 on n47-r0f GREEN: punch True 22.0 %, launch 14.9 m on the cap)

- **Text (G5):** under wick47 the wick43 bar hints are gone (null): only wick47's BossLine40 lines, one at a time.
- **G7 indicators:** the ring clock is a flat thick yellow annulus on the glass under the core (outer 3.7 -> 1.7 m, band 20 %), the
  chevrons flat yellow V's 1.4 m on the glass (gains 2.4 / 2.6). Not re-framed.
- **G1, the bottom as STEPS (the lead's call):** 5 plain step ledges (1.4 m apart = 0.49 of the double jump's 2.88 m, 50 deg apart, 48 deg
  wide, 2.6 m deep), the last is ledge 1 (7.0 m); 8 ride panels after it (2.5 / 2.8 m), ledge 2 at 16.2, the cap at 26.3. The first panel
  75 deg past ledge 1 (a short hop). The steps and the panels fold into the wall in P1 and come out at P2. No slag in P2 (with it at ledge 1
  - 4 m the steps flooded: 37-39 deaths in capped seeds). A ledge reached in P2 lands 4 % of the bar. The step / mount pilot (harness
  W47StepFrame, also in the human policy's P2): run to the next step's centre, jump inside 5 m, double jump at the apex; from a ledge, run
  along it to the next section's first panel and jump at it (Wick37Pilot's own ledge start stalled).
  - n47wick sloppy, 40 seeds, no boss: the cap **39 / 40**, climb median **30.7 s** (G1: 25-45 s), p90 59.0. FIRST try per step:
    40/40, 37/39, 38/39, 25/37, 31/38 (**171 / 193 = 89 %**: G1's 95 % not met; steps 3 and 4 the misses). Ride gaps after: 0->1 28/40,
    1->2 24/40, 2->3 26/40, 3->4 37/40, 4->5 31/40, 5->6 28/40, 6->7 22/34 (top third 65-82 %).
  - Variants measured: depth 3.2 (no overhang rule): steps 39/40, 33/36, 38/38, 26/35, 38/38 (93 %) but 4 of 40 human-pilot fights stuck
    under a step's overhang on the floor (capped at 300 s); the overhang rule everywhere: steps 40/40 .. 14/37 (worse).
  - Control (the pilot's own hand): 40 / 40 in 17.7 s, every first try 100 %.
- **n47human, 40 seeds, rows on (final round 3):** median **91.0 s**, p90 **124.7**, min **68.5**: IN; 80 / 80 openings punched, 80 hit-stops;
  launches 72 / 80 in 12-18 m; 0 off the cap; 0 falls into the shaft. With the SLOPPY climb in P2 (the run before the floor-only overhang
  rule): 39 / 40 killed (one capped), median 119.7, p90 181.7 (OUT by 1.7 s), curtains 201.

## Round 4 (02:33-03:00)

- **G1 steps 3 / 4 (traced, n47wick sloppy --trace, `STEP MISS` lines):** every miss took off from step 2's far end (about 300 deg) and
  flew a straight chord past step 3's centre and over its INNER edge, landing 5-6 m from the axis (on the floor) or on step 4: a run built
  over steps 0-2 plus a late press carries a jump 7-11 m. Tried: steps 3-4 62 deg wide (2 deg gaps): worse (21/40, 19/40: their centres moved
  away). Kept: steps 3-4 62 deg wide AND 4.2 m deep (stepDepths [2.6, 2.6, 2.6, 4.2, 4.2]; their undersides 5.2 / 6.6 m up, no floor pocket);
  3.6 m measured 94.3 %. n47wick sloppy 40 seeds: FIRST try per step 40/40, 35/37, 37/39, 35/36, 40/40 = **187 / 192 = 97.4 %** (G1 >= 95 %:
  met); the cap 39 / 40, climb median 29.0 s, p90 44.1 s (25-45 s: met). The top third's first tries 31/40, 30/40, 19/33 (58-78 %: under 80 %);
  rise 2.6 at the top measured no better (29/40, 28/39, 20/35), kept 2.8.
- **The band** (gate P1 45 %: at 55 % the faster climb took the skilled pilot to 84.2 s, at 50 % 89.1): n47human 40 seeds: median **95.0 s**,
  p90 **105.4**, min **71.0** IN; the SLOPPY climb in P2 with the boss: **101.2 / 119.4 / 82.9** IN, 40 / 40 killed, no cap. 80 / 81 openings
  punched; launches 74 / 80 in 12-18 m; 0 off the cap; 0 falls into the shaft.
- **G9 (3 rows, enabled false; Tools/night/w47-rows.cjs):** `wickRideParry47` (sim): 19 ride-parries in 40 fights, each a second crack (19);
  `wickIntro47` (sim): the name card "THE WICK. THE FURNACE WAKES." 40 / 40 fights, then the first hint; the band with both on: 95.0 /
  105.4 / 71.0 (unchanged). `wickFinale47` (view): the finisher's slow motion (Slow39.OnParry), 10 yellow-white glass cracks growing from
  under THE WICK over 1.6 s, the slag light x4 decaying: not measurable headless (frames W47-g9-finale / -finale-2). Research: ULTRAKILL boosts a
  parried projectile and freezes on a parry (ultrakill.wiki.gg Parry, via a search result: Reported); DESIGN-RULES B-3 / M-6 (read).
- G7's slow motion on every core punch: the view calls Slow39.OnParry under wickLook47 (the parry window's slow); not measurable headless.
- **G1's windows** (n47wick windows: per ride gap, the release points of the panel, 10-100 % in 5 % steps, that ride the next panel; the
  look level on the ride, 45 deg at the release; window = points x 5 % x the panel / 10.4 m/s): rows on, every ride gap 615-731 ms (gaps
  0-2 19/19, 4-5 and 5-6 18/19, 6-7 16/19): none under 150 ms. The steps are a jump from a floor (no timed window: any press on the step
  jumps; not measured as a window). The top third's misses: every traced miss a 150 ms late release past the panel's end (n47wick sloppy
  --trace RIDE MISS); tried coyote 0.25 s, catchBelow 2.6, top panels 9.5 m, a top rise of 2.6: none better (58-78 % first tries stay).
- **The lines (F's docs/COPY47.md):** sentence case, the glyph shown ("{punch} the yellow embers back.", "{fire} through the crack.");
  the core line dropped (F's hud47 posts "{punch} the core!"); the name card "The Wick. The furnace wakes.". Nets in P2: 432 (about 11 a fight: every miss costs one section).
**G4's 10 s of fire** (n47wick chip: KESTREL clicks at 8 Hz from the floor, every shot aimed): rows off the bar moves 4.4 %, rows on 17.5 %
(18 chips, 3 full hits in windows): >= 5 % (at the human's 45 % hit share: about 7.9 %, computed, not run).
