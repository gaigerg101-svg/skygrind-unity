# Wave 22: FINISH, POLISH, PROVE (planned 2026-10-07 14:29-14:55 by the wave-forge; branch port/stage3, box 4 h, hard cap 4.5 h, not yet run)
**Ask:** GG, 2026-10-07: "Prioritize building an iteration polishing accuracy all of." (4 h box, 4.5 h cap, 30 minutes to write the prompt). Raw text: the playbook
library, 2026-10-07_skygrind_night22-request_finish-polish-prove.
**Planned:** nine proof-first rows (NIGHT22-FEATURES.seed.json): arcline, stamp, deadline, grind, replay (the first cut), challenge, ghost-daily, dusk-clock, haptics;
seven accuracy items (A-1 .. A-7: a measured replay error, the pad press, mutators, a round P, DEADLINE at rounds 10 and 15, the bench alone, the allocation);
seven seeded leftovers. Planning set `Research/2026-10-07-wave22/`.
**Prompt shape:** NIGHT22.md 265 lines (guide ~350), 11 items, builders 3 + W2 by file ownership, four compile rounds (1:15, 1:50, 2:30, 3:05), freeze 3:10, table
4:00+, caps builds 8, reel invocations 24, farm batches 3, bench 3, suite 2, building share >= 65 % of agent-minutes.
**Forge notes:**
- Research was thin (three web agents, snippets only, about 40 s each): the stamp-timing, scale/alpha and grapple-cooldown cues were "not found".
- The fresh reviewer (9 minutes, 15 findings) earned its time: RED could not happen on a Player before the features exist; the Editor-gate script was run before it was
  written; M10 (18 px) and M9 (entrance 90-300 ms) disagreed with the prompt's numbers; the caps did not add up; the goal's 4:00 floor was not in its condition.
- Lesson for the forge: for every number the prompt gives, grep the meter that will judge it (`feel-metrics.py` constants) before writing it.
**Forge experiment:** proof-first rows: each feature's reel scenario is written and seen RED (same Player, switch off) before the feature is built, with named
owners for the scenario files.
- Hypothesis: at least 6 of the 9 contracts pass (NIGHT21: 1 of 7).
- Measure: the `passes` count in NIGHT22-FEATURES.json at the close.
**Shipped / Numbers / SHORT / What went wrong / Lessons / Reaction:** (the harvest after the run)
