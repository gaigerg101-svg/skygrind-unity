# Wave 69 (NIGHT69, THE ROOM THAT PAYS; 2026-10-25 09:55:04 MST, 4 hours: BUILD 0:00-3:15, CAP 4:00; tag u3-n69)

**Ask (GG 2026-10-25, verbatim):** "follow those 2 same prompting methods to make this prompt to create a detailed 4 hour long prompt to build". Standing: "you make the calls".

**Prompt shape:** FORGE69 (Research/2026-10-25-forge69/: scout.md, prompt-methods.md, DECISIONS D-1..D-8 + R-1..R-17, redteam.md, slate-seed.json, builder-brief.md, room-chain.sh, GOAL.txt);
NIGHT69.md 107 lines (one usage window: agent work before ~2:20, a ten-minute minimum close after a stop); the n69 tools (Tools/night/n69-*, forge69-make-n69.cjs; n69-next.cjs the
next-action printer = the forge experiment); slate 8 (Y1-Y3, kb3 v2 g8, LV, PC); builders R, S, T + one review slot + one critic. **Branch C** (C: 5.5 GB at t0, 7 GB later: no build, no Player).

**What shipped (game):** nothing (every room behind its OFF row). Harness / tools: Y1 the STYLE rule (verb-aware >= refusal - 5), Y2 the M3 reader and probe (a pit counts only for a LAUNCHED body), Y3
the patch-share tool (Tools/night/n69-patch-share.cjs). Each: planted pair RED -> GREEN, critic PASS (Logs/n69/verdict-Y1..Y3.md).

**Numbers (a1: each room alone on main sha deda5825; M2 VERB pooled seeds 1-20 vs mainBar; fresh 21-30):** kb3 r1d launch **1.08** vs 1.33 (red 1.13; fresh 1.23), M3 FAIL, room check R5 + I4
regressed; v2 s1b launch **0.99** vs 1.25 (red 0.87; fresh 0.94), deco FAIL; g8 r1f ride **0.96** vs 1.25 (1.35 for a ride; red 0.63; fresh 1.06), I2 ride shortcut + air65 regressed. The GATE 0 / 3.
a2 (kb3 r2b alone, main f5dfe4fc): 1.09 vs 1.33 (fresh 1.10; it swaps wave 4's kinds and breaks deaths A: not shipped in the build); the fix round (R r3a 0.94 / r2b / r2c 0.94; S s3 0.88, s1d 0.94, s2c 1.07 refused, g8 pocket 0.80-0.94): none beat a1. PC: PC-1 (10:01) and PC-2 (11:52) built Builds/n69-play (kb3 r1d, g8 r1f, v2 s1b; every room NOT PASSED); booted once under `-skygrindSmoke` (DONE: all steps passed; the rebuilt levels not loaded); critic PASS round 2 (Logs/n69/verdict-PC.md). Slate: Y1 Y2 Y3 PC PASS, kb3 v2 g8 LV SHORT. Q-B check: the single-refusal ratio is at most 1.20 on every recorded file (Logs/n69/q-b-refusals.txt).

**Causes (the level review, Logs/n69/r1/review-level.md; S.md / R.md on n69-S / n69-R):** the queue the room builds stands on the throw line, so a launched body bursts before the pit (measured
flights 0.3-2.5 m, not the 12.8 m the v2 card assumed); the verb-aware run's slow seeds walk 100-260 m more than the refusals; v2's clear time is set by whether W3's TANK falls into the crypt
(26-37 s vs 47-76 s); skip v2 compares against the faster of two refusals (v2 s2c 1.20 vs either alone, 1.08 vs the faster; the lead's table over every recorded skip file: the single-refusal median is at most 1.20, so the rule is not what failed the rooms); g8's ride saves 2.3 m of walking in a 40 s kill-bound clear and no piece
keeps a body where only the ride reaches (a spitter is a walker).

**Reviews:** the level review (9 BLOCKING lines; every fix candidate measured worse or equal: r3a 0.94, s3 0.88), the critic for Y1-Y3 (PASS x3) and for PC (FAIL then PASS), gg-proxy (Logs/n69/gg-proxy-close.md: the pit never where the punch sends the body; the mechanic is not the better way; the old room with a patch).

**Lessons:** a deterministic sim reads the same alone in a builder's tree and on main (kb3 r1d 1.08 both): builders' own alone numbers predict the main round, so screen candidates there; a rebuild
that satisfies a card and fails the geometry (the pit beside the throw, a card sized for a 12.8 m flight that is 0.3-2.5 m): measure the verb's real flight on the shipped room before the card;
a guard the printer does not read (the room check's R5 / I4) still failed twice: put the room check in the builders' own pre-pick list; Y2 picked before a1 kept a1's M3 honest.

**Estimate to actual:** Y1 0:20 -> 0:12, Y3 0:45 -> 0:17, Y2 1:00 -> 0:40; a1 0:45-1:15 -> 0:45-1:12 (27 min for three rooms alone); gates 12 min; level review 17 min; critic 6 min. Idle: one usage stop ~12:05-12:50 (45 min; the night ran 2:10 before it; the forge expected ~2:20); the close started at 3:00 as planned (full close, not the minimum).

**Reaction:** none yet (GG last played 2026-10-20; Builds/n69-play is for him).
