# DECISIONS (FORGE68, 2026-10-24 ~22:00-22:30; GG: "follow same methods to build prompt for overnight, 8 hours" and "same methods but
# also again resreach best methods to get best results to claude prompt for a long pormpt and opus 5.5, =go beyond what i am saying")

The method is FORGE67b's: scouts in this folder (history.md, levels-data.md, research.md, code-scout.md, plus prompt-methods.md for GG's
second ask), these decisions, a red team (redteam.md), the slate seed (make-slate.cjs -> slate-seed.json), the builder brief, GOAL.txt,
the n68 tools (Tools/night/forge68-make-n68.cjs: renamed copies of the n67 tools that keep NIGHT67's data names; close-check self-test
28 cases PASS, the area lint's 25 planted cases GREEN, both run 2026-10-24 ~22:05). Grounding: the repo; Google Drive and Gmail searched
for SKYGRIND (no results); Linear holds nothing for this game (the prompt-methods scout).

## D-1 Same shape as NIGHT67: 8 hours, two work blocks, R / S / T + one review slot, branch by the disk
NIGHT67's two-block plan predicted the stop within 10 minutes (history.md section 1); the tools came in at 0.3-0.6x. C: about 13 GB at
the forge: branch C unless GG frees space (DECIDE68).

## D-2 The instruments first (I1-I4), because NIGHT67's failures were mostly instruments (levels-data.md, code-scout.md)
I3 lava by height (Harness.N46.Flow.cs:444-450, 572), I2 the launch target (VerbPilot.cs:197; Combat.cs:1483-1511), I4 pooled seeds 1-20,
the refusal count and a STYLE rule that can be met (Harness.N65.Verb.cs:1059: +15 unreachable at 80.1 / 88.6 %), I1 the ride line
(VerbPilot.cs:168, 224-244; Verb.cs:853-856). Smallest first so three land before the ride. Each with a planted pair and a critic.

## D-3 The M2 rule, written before any measurement
Pooled median over seeds 1-20 >= max(1.25, red + 0.20), 14 / 20 exercised, the refusal fired on every exercised seed; STYLE no lower than
the refusal and +15 only when the refusal is under 80 %; the GATE's fresh seeds 21-30 >= 1.25 (history.md: per-seed 0.67-4.30, g7 1.56 /
1.24 on the two sets of ten).

## D-4 The rooms (levels-data.md section 3): S kb3c kb2 kb3d (level 3), R g8 (level 2), v2 (level 5, blank rect), kb3 (level 3)
g7 waits for GG's rule (DECIDE68 Q1); v6 is a leftover (S's NIGHT67 call SHORT). Rejected: g1-g5 (no gauntlet pilot: code-scout (e),
2-2.5 h; a leftover for T).

## D-5 "How a verb wins" (research.md): the four conditions on every card
It pays / the other answer is weaker / the route without it is longer / the verb is the safe or fast state. Tonight's rooms use the
route and, within GG's rules, a pay at the verb's end; the ride speed boost and safe states need Mover.cs or rules: DECIDE68.

## D-6 The level review is scheduled (history.md: NIGHT67 never wrote one, so no room could have passed)
A fresh read-only reviewer starts the moment a round ends; a round without one holds no GATE.

## D-7 Queue the stop: one chain from the harness build through the reds, the bars, r1 and the 21-30 pre-runs (started by 2:10)

## D-8 PC always: a labelled hard-link play build of the best room per level, PASSed or NOT PASSED, from 6:00, in a slots copy
GG has played nothing after EXT53; two nights gave him no build. The tracked slots and rows never change for PC.

## D-9 The tools: n68 copies; NIGHT67's data names kept
No new rows tonight: design.n67.*67, Design/levels/n67/, slots67.json, levels67.json, --off67 and the cards in
Research/2026-10-24-wave67/cards/ carry on. Evidence goes to Logs/n68 and Research/2026-10-24-wave68/.

## D-10 The forge experiment: Logs/n68/STATE.md
forge experiment: a lead state file rewritten at every landing and every 30 minutes (agent ids, background jobs, the next three steps),
read first after any compaction, idle compaction or stop; hypothesis: resumes lose less time and no agent id is lost (Claude Code 2.1.299
compacts an idle 200K+ session after about 54 minutes, and SendMessage needs ids after a compaction: prompt-methods.md); measure: in
NIGHT68-REPORT.md, the count of STATE.md rewrites and the minutes from each compaction or resume to the next useful tool call (NIGHT67:
not measured; the resume after the stop is in NIGHT67-LOG.md for comparison).
## D-11 The calls (GG 2026-10-24, after the planning set: "you make the calls"; DECIDE68.md holds each reason)
Q0 no deletion (the branch read at t0); Q1 g7 YES as a melee-only room by the game's own `limit` field (a seventh slate room, R, behind a
probe); Q2 the ride boost NO (Mover.cs, rule 3); Q3 the pit rule NO; Q4 the play card YES; Q5 E1 YES (built: .claude/hooks/night-resume.cjs
and a SessionStart compact|resume entry in .claude/settings.json), E2 NO (a Stop hook could block the /goal's exact final message), E3 NO.
**D-10's experiment becomes STATE.md plus the hook** (the measure unchanged; NIGHT67 is the baseline with neither).

## The red team's amendments (redteam.md 1-13, applied to NIGHT68.md, slate-seed.json and the brief)
R-1 the verbs set after the card critic, before any red; R-2 / R-3 the reds start at a landing, in one chain started at I4's landing,
the ride rooms' reds after r1-go; R-4 block 2 timed so r2's GATEs and one batched critic land before --start (r2 by 5:35, critic by
6:35); R-5 / R-6 the fresh set is seeds 21-30 in the tool (I4: `--seed 21`, `--fresh`); R-7 the red's skip command written out,
red-bars reads the pooled line (I4); R-8 the flow base re-made after I3, LV against the current base; R-9 PC through `gen-slots
--slots=<ids>` and n65-play-build's options; R-10 STYLE >= min(refusal + 15, max(refusal, 95)), UNSURE defined; R-11 the worktrees' ` M`
files carry over unstaged; R-12 ROOMS per level dropped, reds named "the shipped room", rc1 named n68-rc1, T's files listed, blank-rect
v2 is design; R-13 a glossary, bracketed reasons, a closing stop section (the compaction hook stays GG's call: DECIDE68 Q5).

Rejected for tonight (each changes settings, which is GG's call: DECIDE68 Q5): a SessionStart `compact` hook that re-injects the core
rules (prompt-methods.md E1), a Stop hook that refuses a DONE without a file (E2), per-role effort (E3).
