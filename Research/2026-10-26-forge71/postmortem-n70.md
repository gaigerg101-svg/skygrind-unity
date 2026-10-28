# NIGHT70 post-mortem against current methods (FORGE71, 2026-10-26)

Sources: `git log u3-n69..u3-n70` and the builder branches n70-R / n70-S / n70-T (commit times), NIGHT70.md, NIGHT70-REPORT.md,
NIGHT70-LOG.md, Logs/n70/runner.log, docs/memory/ledger/wave-70.md, RESEARCH.md (R1-R3). Every number comes from those files.

## 1. What happened, by the clock (t0 22:43:52)
| span | wall | what | evidence |
|---|---|---|---|
| 0:00-0:04 | 22:43-22:48 | bearings, report skeleton | 842fed73e |
| 0:04-1:27 | 22:48-00:11 | three builders: R 12 commits (kb3, 22 variants), S 16 (v2), T 11 (Z1 Z2 Z3 tools); picks 909b9d45, a7dcad55, 468a9856, d9336634, 68671700d | branch logs |
| 1:27-1:37 | 00:11-00:20 | the lead's drafts | 8ef612ab9 |
| **1:37-7:00** | **00:20-05:43** | **nothing: no agent, no commit, no job (the runner idled from 00:13)** | runner.log; no commit in git |
| 7:00-7:14 | 05:43-05:58 | the close: gates 8 min; the freeze build, verbs37 and the smoke refused (6.4 GB free) | gates-flow-close.txt, ## Blocked |
| 7:14-7:30 | to 06:13 | waiting for the close-check's clock floor (7:30) | n70-close-check.cjs |

- **Work share: 1:37 of 8:00 (20 %). Idle: 5:23 in one block, plus 16 min waiting for the clock floor.** Building the game: 0 minutes
  that reached a player (no file under Assets/, Design/, PortExport/ changed). E70's experiment (acts keyed to usage stops) measured
  nothing: no usage stop came.

## 2. Why: the five causes, each set against the methods (RESEARCH.md)
1. **One plan, no backlog.** NIGHT70.md had one aim ("one room PASSes its GATE on a2") and a not-tonight list that forbade the other work
   (other rooms, levels, bosses, the parry). When the screen failed at 1:27, the prompt had no next item. R1.1 and R1.3 say to keep a
   ranked backlog deeper than the night, and to treat "the main plan failed" as a named non-stop. PLAYBOOK §3.17 says the same (S19 idled
   2:37), and the forge did not apply it.
2. **The plan was built around a usage stop that did not come.** Act 2 started "at the resume R2". With no stop there was no R2, so
   there was no trigger for Act 2's work. The lead read that as "wait for 7:00". A prompt keyed to an external event needs an
   "if it does not happen" line.
3. **A metric gate that never passes became the night's purpose** (R3, Goodhart and specification gaming). The GATE ran 0 / 22 over
   NIGHT65-69, and NIGHT70 added three more instruments (Z1-Z3) to the same gate. Those were T's whole night. Six nights produced
   no game change. Nobody ever checked the instrument against GG's own judgement (R3 rec. 6). Six forges each asked "how do we pass the
   GATE", and none asked whether the GATE should decide.
4. **Disk blocked every Player step, and the thread aged ten waves.** T-149 (w61) "GG: the worktrees (about 232)". C: had 5-7 GB, the
   guard refused the build, so no play build reached GG on NIGHT70. 183 clean worktrees freed 63 GB in the forge today (GG's call,
   worktrees-removed.tsv). That cost minutes; the thread had stayed open since w61.
5. **The prompt was too dense to follow.** 120 lines, many over 300 characters, eight rules, and act triggers. The rule "every turn,
   append the clock line to stops.txt" was not followed all night, and STATE.md misread the 7:00 mark once. Research (IFScale,
   lost-in-the-middle; PLAYBOOK §3.1) says long rule-heavy prompts lose their middle. Opus 5 / 5.5 also over-verify when told to verify
   (R1). The night's tools ran to 31-48 KB each (close-check, gate-pack).

## 3. What went right (keep it)
- The picks, critics and gates: three tool slices PASSed with planted pairs and critics; the gates GREEN; the close-check's one-line
  verdict let /goal judge the end without re-judging the night (the stop hook only caught extra text in the final message).
- Honesty: every SHORT carries its numbers, and the report states what was not checked.
- The queue runner: four jobs, no collision with the lead's picks.
- Builders in fixed worktrees with explicit paths, and no stray stage of GG's files.

## 4. What NIGHT71 changes (DECISIONS.md has each one with its rejected alternative)
1. **GG's call: bots guard, GG judges** (REQUEST.md). The room GATE is retired as the verdict. Guards: the level boots in a Player, the
   walk completes, no softlock, the lint, movement exact, deaths and stall in band. The M2 ratio is printed on each room's card for
   information only.
2. **A ranked slate of about 2x the building time, in four tracks** (rooms, bosses and colour, combat and camera feel, cleanup). A
   builder whose item ends (PASS or SHORT) takes the next one. **No step waits for a clock**, except the close's own floor.
3. **The deliverable is GG's play build** (Builds/n71-play plus PLAY71.md), updated mid-night and at the close. The close-check gains a
   `play` check, so the /goal cannot pass without it.
4. **Disk freed before t0** (done: 70 GB). The guard's 12 GB rule now allows a build, the reels and the smoke.
5. **A shorter, plainer prompt:** lines under ~170 characters, six rules, no act triggers, no per-turn bookkeeping rule.
6. **Kill criteria with a state and a date** (R3): any item with two rounds and no progress is SHORT, and the builder moves on (R1.2).

## 5. Lessons for memory
- L-153 (new): a night's plan needs a next item for when its main bet fails early, and a line for when the event it is keyed to never
  comes (w70: idle 5:23 after the screen failed at 1:27; the acts waited for a usage stop that never came).
- L-154 (new): after N failed nights on one metric gate, ask whether the gate should decide, not how to pass it. Check the instrument
  against the owner's own judgement before it gates (w65-w70: 0 / 25 rooms, no game change in six nights; GG: "Bots guard, I judge").
