# Wave 66 (NIGHT66, THE FOUNDRY EARNED; 2026-10-23 22:26:11 to 2026-10-24 ~06:20, 8 hours: BUILD 0:00-7:15, FREEZE 7:15, CAP 8:00; tag u3-n66)

**Ask (GG 2026-10-23 ~20:50, verbatim, NIGHT66.md line 3):** "can we follow same methods for creating next detailed night prompt that
will go for 8 hours?". The forge FORGE66 (Research/2026-10-23-forge66/: four scouts, D-1..D-12, a red team's 14 findings as R-1..R-14)
chose level 1's six rooms made to NEED their verbs, measured by a skip test v2, with a sim-side GATE. GG set the /goal in the forge's own
session, so the night ran there.

**Prompt shape:** NIGHT66.md (234 lines, untracked) + GOAL.txt on one end-state script (n66-close-check.cjs: PASS / MINIMUM PASS / FAIL,
--selftest 23 cases, --pre, --tag); slate 12 (T1a, T1b, T1c, T2, six rooms, LV, PC); builders T (tools), R (a2, a3, a5), S (b1, a8, a4) in
NIGHT65's three reused worktrees (no new disk) + one review slot; verb plans, one plan critic, four reviewed rounds, early GATE after
round 3; MAIN = skip v2's M1 against mainBar = max(1.25, red65 + 0.20).

**Branch C** (C: 7.7 GB at t0; 11.9-13.5 GB later, never 18): sim only. **Two usage stops** (an individual spend limit): 0:20-2:15 and
4:35-7:15, 4:35 of the 8 hours, each stopping all agents and the lead at once.

**What shipped (game):** nothing: foundry65 OFF; slots65.json gained b2 = old (T2). The freeze build n66-freeze re-checked the unchanged
game with the night's tool code.

**Tools (the night's real output):**
- T1a PASS (Logs/n66/verdict-T1a.md): tour65's GREEN rule (unframed 0, WARN 0, wedges); `gen --run` safe on a levelgen error.
- T1b PASS on round 3 (verdict-T1b.md, -r2.md, -r3.md), skip v2:
  - three runs per seed; the ride refusal is the backstop only; pierce uses KESTREL-only; the M1 line names its bar's source;
  - planted: forced 3.84 PASS, null 0.95 FAIL;
  - a3 1.64 -> 1.13 as the refusal's bias came out.
- T1c FAIL on line 3 (verdict-T1c-r2.md): `ride --shoot`, the decoration checker v2 (planted 8 / 8, 7 / 7) and the round's `--out` were
  built; the proof failed on the lead's mid-round writes.
- T2: b2 as a seventh slot (`--check` all PASS).

**Numbers (skip v2 M1 vs mainBar; Logs/n66/r1, r2, the builders' trees):**
- a2: 0.87 (r1) -> **1.52 PASS** (r2, 10 / 10); stall V 33.1 % outside the reds' 12.4 / 23.9.
- a4: 1.22 -> **1.57 PASS** (S's round 2, own tree); one walk-bot death on seed 13.
- a3: 0.88 -> 1.13, short of 1.25.
- a5: 0.79 UNSURE -> 1.25 (deaths 1 / 4) -> 0.80 UNSURE.
- a8: 0.84 -> 1.03 UNSURE (4 / 10 exercised).
- b1: 1.33 twice against 1.53; dropped (DECIDE66 D-3).
- The decoration checker on r2: 0 pieces added or moved in all six rooms.

**SHORT (10):** T1c, T2 (no critic), all six rooms (no GATE ran), LV (branch C), PC (nothing new to play). PASS: T1a, T1b.

**Reviews:** the verb-plan critic (a2 / b1 GO, four CHANGE); the r1 and r2 level reviews (11 and about 8 BLOCKING lines); no gg-proxy.

**Lessons (LESSONS.md):**
- L-003 re-confirmed: two stops in one night.
- L-150 re-confirmed and extended: the instrument needs its own critic rounds first; a spawn is the retry point, not the first entry
  (a3's spawn move gamed the skip's start); never write tracked files or commit while a round runs, even with --out.

**Estimate to actual:** T1a 0:40 planned -> 0:12 done; T1b 1:30 -> 2:35 (then two fix rounds to 4:06); rounds every ~65 min planned ->
r1 3:27, r2 4:21 (main-tree rounds 22-23 min under load vs dry0's 9). Idle: the two stops (4:35).

**Reaction:** none yet (GG last played 2026-10-20).
