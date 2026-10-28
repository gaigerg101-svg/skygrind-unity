# Wave 61 (NIGHT61, the build night that could not build a Player; 2026-10-22, t0 10:18:34 AM, close from 4:18 PM; tag u3-n61)

**Ask (GG 2026-10-22, FORGE61.md lines 3-5, OWNER.md:122):** "research best ways to prompt and get better results, skills whatever ... so it can
create a powerful goal and loop prompt to build and not run into mistakes, polish, kill dead code, but need to see real results and it needs to
be honest and critical where things are not good." FORGE61 wrote NIGHT61.md, the guard hook, the n61 tools and the slate seed.

**Prompt shape:** NIGHT61.md (165 lines, untracked) + the /goal (Research/2026-10-22-forge61/GOAL.txt): 6 h minimum, 7 h hard cap, the close
6:00-7:00, an 8:00 escape. Slate of 10 (S1-S7 visible, B1 / B2 bugs, D1 dead code), +3 from gg-proxy (C1-C3). The visible rule: a row turns ON
only with a Player frame pair, a blind SPOT (skygrind-spotter) and a skygrind-critic PASS. At most four builders and two critic slots. The guard
hook blocks the rules broken most (heredocs, git writes, GG's files, builds under 12 GB). Forge experiment: "build what a player sees, measured
once at the end" replacing nights of measuring; hypothesis: more visible rows per night. **Not tested tonight:** no Player could be built.

**Shipped (ON):** nothing. **Built, OFF, compile-checked and gated, never seen** (T-148): exitFrame61 (S1, V), bossRule61 (S3, G), text61
(C1, G), fixture61 (S4, V), gunYellow61 (C3, V); B1 (D: focus60 never pauses an automation run; test not run in the Editor); D1 (D: 35 batches,
397 lines deleted, 27 rewritten, 370 net, Logs/n61/DEADCODE.md). A1: design.n61 + N61SharedTests (7334a3f5). The integrated tree, HEAD 9c82dbcd
at 14:29: gates GREEN, combined dn-check 0 errors (Logs/n61/gates-final-C3.txt, dncheck-final-C3.txt).

**Numbers (before -> after, source):**
- C: free 8.3 GB (10:21) -> 6.1 (11:25) -> 5.1 (12:00) -> 4.5-4.6 GB (14:04-14:28) (NIGHT61-LOG.md); Logs/ 200,611 MB, Builds/ 70,465 MB (du).
- Gates GREEN after A1 11:09:17, B1 12:06:33, S1 12:40:41, S3 12:53:38, D 1-22 13:14:26, C1 13:21:04, D 23-31 13:41:24, D all 13:55:34, S4
  14:04:13, all 14:29:22; every combined dn-check 0 errors; the fingerprints EQUAL at every step (no sim change slipped in).
- S4's meter (builder V, the light plan, not a frame): 49 unmounted ceiling lights over five levels -> 0 with the row on.
- C1's red (builder G's count, n60-b8 a157 kiln entries): two or more centre text blocks 4 / 5; the last room's ROOM CLEAR 2 / 5.
- S5's red (builder G): every n60-freeze verbs37 frame that shows a rank shows DULL (1969 / 1969); the bot's real fights above DULL 28-89 %.

**SHORT / blocked (reasons):** every Player step (the disk under 12 GB: 0 builds, 0 reels, 0 smokes, 0 SPOTs, 0 critics); every Editor step (the
Pipeline silent 10:21 to the close: 0 EditMode tests). S5 SHORT (the cause is the sim's pacing, golden-pinned; GG's call, T-150). B2 SHORT (the
cause as a hypothesis, Research/2026-10-22-wave61/B2.md; no fix, no run, T-147). S2, S6, S7, C2 not started (S2's data changes a shipped level and
cannot ship without a Player).

**Estimate to actual (from NIGHT61-LOG's times):** D: B1 ~55 min, D1's 35 batches ~1 h 50; V: S1 ~70 min, S4 ~65 min, C3 ~21 min; G: S3
~75 min, C1 ~30 min, S5 (SHORT) ~20 min; gg-proxy 5.4 min; the auditor ~78 min. No usage-limit stop. The lead: ~10 integrations (cherry-pick,
the config row re-run by the builder's own script twice, gates, compile), ~5 min each plus the gate runs.

**What went wrong:** the disk and the Editor (T-149); the lead's mistakes (L-116..L-125): a backgrounded `&&` chain lost two outputs, a
cherry-pick range from a copied sha re-picked B1 (aborted, no bad commit), an invalid gate file (deleted), a wrong row name in a message, a
foreground sleep, a time from memory, a wait testing a branch name, a guard-blocked `git worktree remove`. Builders' own slips (reported by them):
one heredoc commit message (V), a stale comment (G, fixed f8f69429), two notes slips (D, fixed).

**Agents:** gg-proxy (the start ranking, Logs/n61/gg-proxy-start.md), dead-code-auditor (59 batches, Logs/n61/DEADCODE-candidates.md), builders
D, V, G in isolated worktrees (L not spawned; V and G resumed by SendMessage for second and third slices, L-098).

**Lessons added or confirmed:** added L-116..L-125; confirmed L-049 (each builder's row script patched only its row; the config conflicts were
resolved by re-running the builder's own script), L-098 (builders resumed by SendMessage), L-108 (every visible fix tonight is a hypothesis until a
Player frame shows it).

**Closed threads:** none met. Merged for the 60-line cap: T-140 into T-148, T-141 into T-100, T-121 into T-104, T-136 into T-123. New: T-148
(the built-but-unseen work), T-149 (the disk and the Editor), T-150 (GG's calls from NIGHT61). Updated: T-146 (B1 built), T-147 (B2's reading).

**Reaction:** no play. The next requests: "ok follow same process in how we built this last prompt into building next prompt but it will last
for 6 hours" (2026-10-22 ~16:2x, FORGE62), then "follow similar methods from building other prompts and create a 2 hour build prompt hard cap
and follow best methods" (~20:35, after the PC ran out of memory at 17:32; FORGE62b: NIGHT62 became a 2-hour run).
