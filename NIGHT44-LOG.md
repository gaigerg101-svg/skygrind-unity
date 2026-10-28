# NIGHT44 log (the finishing night, cut to one hour; NIGHT44.md; Research/2026-10-16-wave44/)

## G0 (20:47-20:50)
- 20:47 GG: "start now, you only have one hour max" (the forge planned 2 h). t0 20:48, hard cap 21:48. Cut for the hour: K2 (THE RING,
  T-65) and B2 (every boss's reveal): their sim rows need a shared N44Blocks commit (~15 min of anchors) the hour cannot carry. Kept: R2
  (reel / pilot fixes: no rows) and V2 (one view row vis44); the lead runs the suite's priority Game batches (N4, Lesson, C, MapC, U) and
  the final round; Core's whole suite skipped (13 min; NIGHT43's Core tests and golden / fingerprint pass).
- Premise: the Editor stopped, C: 46 GB free (the forge deleted NIGHT43's own numbered frames), only GG's four files dirty.
- 20:49 shared commit 1adc3e8e: design.n44.vis44 (off) + Game/N44View.cs; Editor compile 0 errors. 20:50 R2 and V2 launched (cards end
  21:25). 20:51 the suite's Game batches started.

## Build (20:49-20:59)
- 20:55 the suite's priority batches (Logs/n42-suite-n44.txt): N4 12/12, Lesson 5/6 (Lesson25TextTests' 7-word prompt: T-62, since
  NIGHT40), C 55/55, MapC 62/62, U 19/19 = 153/154; the rest of Game started (Logs/n42-suite-n44b.txt).
- 20:56 V2 picked (1a0ece7d..1ae5cdc0): vis44: the FUSION's streaks curved (55 deg) and tapered (26 -> 3 px) with a hot-pink glow and a
  white heart growing to the meet at 0.09 s; the TORCH's cone (4 puffs ringing each of 18 segments, 11 deg); T-69 MessageSlot33.Drop on
  roomEnter; the look44 reel (vis44 off vs on with feel43 on). dn-check 0 errors; no Player frame yet.
- 20:58 R2 picked (9e00f0e2..a6d14015): T-59 the wick37 fight reel drives Wick37Pilot through Wick44ProofInput (sim.UseInput, a step per
  sim step): headless `n44proof wick --seeds 3` the old per-frame loop 0/3 killed (0 rides) -> 3/3 killed (49.8 / 51.1 / 49.8 s; 15
  rides, 4 parries, 4 cracks, leg 3); T-68 the ash bed fires a real shot inside the window; T-66 move43's chimney 3.5-4.0 m from floor +
  3 m (`n44proof chim`: every gap 3.5-4.5 m climbs in a 20 m box); SHORT: B after a real ride, T-67.

## Final round (21:10-21:45; n44-r1 = port/stage3 at the picks, vis44 on in its copy)
- Gates GREEN 21:10 (golden 0 / 22 exact, fingerprint EQUAL plain and --off43; the Editor compile 0 errors after a forced import of R2's
  new file). The Game suite run 20:55-21:09 then stopped for the hour: A-M9 + MapC all pass except Lesson25TextTests (T-62); MapP-Z and
  Core not run.
- wick37 fight GREEN: all 15 panels ridden (was []), leg 3 reached, parries 8, cracks 8, pierces 3, a window; THE WICK alive at the reel's
  end (not killed on frame): T-59 narrowed again. kiln43 GREEN: ash counter 1 (T-68 closed). move43 RED: the chimney (a7, 3.65 m) still one
  press then the floor (T-66 open). look44 RED only on the TORCH pair (the reel never fired the TORCH): the FUSION pair 0.0805, the stale
  card gone with vis44 on (T-69 closed); the FUSION frame looked at (curved tapered streaks, a pink-white core, the word).
- The smoke on n44-r1 FAILED twice (18 / 17 steps: every buy, the power switch, exfil "Paused") and the control on n43-r5 (passed at
  19:54) FAILED the same way at 21:35 (19 steps): the machine's state (a window taking focus / the pad: player-smoke-focus-pause), not
  NIGHT44's code; not re-run clean.
- 21:46 vis44 ON in the repo (its frames); play build Builds/n44-r1 (vis44 on in its copy = the repo). Verbs not re-run (no sim change
  tonight: R2 and V2 touched reels and views only).

## Close table (21:47)
| card | state | measured |
|---|---|---|
| G0 | DONE | the shared commit 1adc3e8e at 0:01 |
| P0 suite | SHORT | Game A-M9, MapC, N4, Lesson, C, U: all pass but T-62; MapP-Z and Core not run (the hour) |
| R2 | DONE (T-59 partial, T-68) / SHORT (T-66, T-67) | wick37: 15/15 panels ridden, leg 3, not killed on frame; kiln43 counter 1; move43 RED |
| V2 | DONE / SHORT (TORCH unmeasured) | FUSION pair 0.0805, stale card dropped; vis44 ON |
| K2 THE RING | SHORT (cut) | the hour; T-65 open |
| B2 reveals | SHORT (cut) | the hour; DECIDE44 D-3 open |
| verbs | not run | no sim change tonight |
| the smoke | SHORT | n44-r1 failed 18 / 17 and the control n43-r5 failed 19 at the same time: the machine's focus / pad state |
