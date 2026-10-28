# Wave 46 (NIGHT46): the flow night (2026-10-17 22:56 - 2026-10-18, port/stage3, box 5 h 45 min; GG did not test)

Written by the NIGHT46 lead at the close from NIGHT46-LOG.md, NIGHT46-REPORT.md and Research/2026-10-17-wave46/ (FLOW-AUDIT, arena-flow-web,
feel-flow-web, FORGE, F1, A1-A3, B1, V1).

**Ask:** "what should night prompt for 5-6 hours focus on, think hard, do good resrecah ... you dicide answers for me, i wont be able to
test so i trust you, again follow best methods and study real game deisgn and areana layouts, need to feel and flow better" (2026-10-17,
after NIGHT45). Every open call decided on DECIDE46.md (DECIDE43-45, docs/RETIRED.md's "GG decides" rows, N-1..N-11).

**Shipped (play build n46-r3 = 71905b12's config; verbs37 26 / 20 / 25 / 30 / 30 with the rows on; the smoke "DONE: all steps passed",
0 NRE):** rows as data switched on (tape format 27; the shared commit is config + hooks). ON: foundry46 / iceline46 (levels 1-2 smoothed
in data, g3 THE TIPPLE), kiln46 + walkin46 + wyrm46 (THE KILN WYRM walked into through d8 at 8.00 -> 8.00 m/s; bot median 38.3 s, max
85.1 s, 40 / 40 killed; cliff run-ups), span46 (level 4: p1 THE HOOK YARD, p3 THE TIE-OFF, p5 THE SPOOL HALL, lip returns on every void),
spire46 (level 5: v2 THE FALLEN BELL, v6 THE ROPE CHOIR, four belfry gates -> three), retry46 (restart at the wave you died in), overlap46
(the next wave enters while 1-2 bodies stand; general holds entryM 12 / growthMax 2), teach46 (holds <= 8 s), stuck46 (a stuck burrower
ends), reveal46 (boss reveal 2 -> 1 s), feel46 (G1 the off-screen wind-up warning at the edge, G5 the hurt static in the edge band; the
kicks G2-G4 off). The flow meter `n46flow` + Tools/night/n46-flow-table.sh (F1).

**Numbers (the flow table at 71905b12, seeds 1-20, baseline = NIGHT45's levels):** L1 walks 16 -> 20 / 20, dead 53.3 -> 30.2 s, clear
210.6 -> 137.9 s; L2 dead 36.1 -> 32.4 s; L3 walks 7 -> 19 / 20, dwell 14.2 -> 6.2 s, dead 26.1 -> 12.1 s, clear 218.5 -> 123 s; L4 stall
18.7 -> 14.5 %, dead 15.6 -> 6.2 s, clear 127 -> 96.3 s; L5 clear 191 -> 163.1 s; nothing worse past the band. The suite Core 1127 / 1129
(2 = a test leak, fixed; L+N re-run 59 / 59) + Game 1046 / 1046. Blind pairs: reviewer ON 4 / 6 (2 "same", 0 OFF), lead 3 / 3.

**SHORT:** shift46 (the state-change light) built, blind pairs 2 of 5 agreed, OFF; the kicks (G2-G4: "same" in blind pairs); g3's stall / dwell targets (the pilot melees the swarm); v2's bell reads as a stepped
stack, not clearly a bell (boxes only); the worm's fight length in a human's hands (bot only); verbs37's grapple rows on a build copy whose
rows silently stayed off (a tool fault, caught and re-run); T-73 the pad (a "GAMEPAD CONNECTED" toast mid-reel; the smoke passed).

**Prompt shape:** NIGHT46.md ~150 lines, six builders, 5 h 45 min; the forge dry-ran the switch-on + n46-shared.cjs twice (GREEN first
try at G0, 32 files); a card gate before geometry (4 cards PASS in 10 min); builds at 00:10 / 00:42 / 01:15 (all batch-mode from the
project copy, 27-43 s each); the freeze at 01:45, about 2 h 50 min into the box.

**What went wrong in the run:** the live Editor stalled unfocused at 23:34 (35 min; G0's MapChoice / N3 / N4 batches lost); a test leaked
static flags into a later batch (Lesson25Tests -> N46SharedTests); n39-build-rows.cjs threw on feel46's nested object and wrote nothing,
and only its last line was printed (r3's first verbs ran rows-off); the lead closed a Player that was its own running reel (L-054); low
memory from another session's jest run stopped a waiting shell; the a446 / a346 / a146 reels needed a round each to frame the right room.

**Lessons added or confirmed:** L-081..L-085 (LESSONS.md); L-054 re-confirmed (identify a Player before closing it); L-001 (feel judged
blind, the row split by part); the batch-copy build (memory batch-build-and-editor-stall) carried all three builds.

**Reaction:** not yet (GG did not test).

## Closed threads (moved from THREADS.md, consolidate 2026-10-20)
- Closed at NIGHT46: T-65 THE RING (walked in, a446 GREEN on r2 / r3); T-71 (DECIDE46: the gantry keeps its meter); T-72 (the bot band met
  walked in: median 38.3 s); T-76 (a446 clears the stale card before its frames); T-54 (GG played later waves).
- Settled at NIGHT46: DECIDE43 D-1..D-9 (defaults applied) are decided on DECIDE46.md (kept as built).
