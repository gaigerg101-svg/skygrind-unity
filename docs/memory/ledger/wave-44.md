# Wave 44 (NIGHT44): the finishing night, cut to one hour (2026-10-16 20:47-21:47, port/stage3, box 1 h, closed at 1:00)

Harvested by NIGHT45 C1 (2026-10-17) from NIGHT44-LOG.md, Research/2026-10-16-wave44/GG-REQUEST.md, FORGE.md, R2.md, V2.md.

**Ask:** "great start planning out next detailed prompt to finish whats not done and anything else you can think of, only 2 hours max"
(2026-10-16 ~20:05, right after NIGHT43's close report): H1 NIGHT43's SHORTs and open threads (T-59, T-65..T-71, the w40-w42 harvest),
H2 the thin landings (the FUSION's flat streaks, the TORCH's thin stream), H3 the whole night in 2 hours. At the go GG cut it: "start
now, you only have one hour max".

**Shipped (build n44-r1):** vis44 ON (V2: the FUSION's streaks curved and tapered with a hot-pink / white heart; the TORCH cone;
MessageSlot33.Drop on roomEnter for T-69); R2's reel fixes (no rows): the wick37 fight reel drives Wick37Pilot one step per sim step
(Wick44ProofInput), the ash bed fires a real shot inside the window, move43's chimney picks a 3.5-4.0 m gap. Shared commit 1adc3e8e
(design.n44.vis44 + Game/N44View.cs only).

**Numbers (source NIGHT44-LOG.md):** headless `n44proof wick --seeds 3`: the old per-frame loop 0/3 killed (0 rides) -> 3/3 killed
(49.8 / 51.1 / 49.8 s); the Player wick37 fight: panels ridden [] -> all 15, leg 3, 8 parries, 8 cracks, alive at the reel's end; kiln43's
ash-bed counter 0 -> 1 (T-68 closed); look44's FUSION pair 0.0805 and the stale card gone (T-69 closed); the suite's priority batches 153 /
154 (Lesson25TextTests: T-62).

**SHORT:** K2 THE RING (T-65) and B2 every boss's reveal (DECIDE44 D-3) cut for the hour (their sim rows needed an N44Blocks commit);
move43 RED (the chimney a7 still one press then the floor: T-66); the TORCH pair unmeasured (the reel never fired it); B after a real
ride and T-67 not done; the smoke FAILED on n44-r1 (18 / 17 steps) and on the control n43-r5 (19 steps) at the same time: the machine's
focus / pad state, not the code; Core and Game MapP-Z not run; verbs not re-run (no sim change).

**Prompt shape:** NIGHT44.md 135 lines, planned for 2 h with four builders (R2, V2, K2, B2), run in 1 h with two; a lean forge (no new
research, one read-only SPIKE.md of five premises with file:line, a fresh reviewer: 15 findings, each with a fate in FORGE.md); forge
experiment: fewer builders after NIGHT43's spend-limit stop (nine stopped for 1.6 h).

**What went wrong in the run:** the hour cut two of four cards before they started; the smoke failed for the machine (the pad / a window
taking focus: player-smoke-focus-pause), so no play-build smoke passed; the w40-w42 harvest carried again.

**Lessons added or confirmed:** L-071 (a reel pilot steps the sim per sim step, never once per rendered frame: R2's 0/3 -> 3/3);
L-072 (a Player smoke failure needs its control: the same smoke on the last good build, run at the same time); L-073 (a sim row that needs
a shared Blocks commit cannot fit a one-hour night: cut it at G0, not mid-way).

**Reaction:** none yet (GG had not played n43-r5 or n44-r1 at NIGHT45's forge, NIGHT45.md "Where we are"). GG's next message (2026-10-17,
Research/2026-10-17-wave45/GG-REQUEST.md): "see where we are, follow best methods, dont repeat same mmistakes ... if it arena related
please dont just randomly plce stuff it needs intension and personality and lets cut fluff and old stuff, dont cut anything meaningful".
