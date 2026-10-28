# DECISIONS (FORGE67, 2026-10-24 ~06:50-07:40, right after NIGHT66's close; GG asked "finish prompt")

GG, verbatim (2026-10-24, after NIGHT66's goal loop): "finish prompt". Read as: the next detailed night prompt that NIGHT66's HANDOFF
said was not written. The method is FORGE66's, kept lean because the evidence is a few hours old: no new scouts (NIGHT66-REPORT.md,
NIGHT66-LOG.md, Logs/n66/ and the six verdicts are the scouting), these decisions, generated tools, a red team (redteam.md), the slate
seed, the builder brief and GOAL.txt. Grounding: the repo only; Google Drive and Gmail were not searched (their sessions had expired at
FORGE66; this game's material lives in the repo). Order of authority: the org rules, GG's words, CLAUDE.md, NIGHT67.md, HANDOFF.md.

## D-1 Eight hours again; plan for one usage stop
NIGHT66 lost 4:35 of 8:00 to two stops of an individual spend limit whose sessions reset five hours apart ("resets 12:40am", then
"resets 5:40am"). NIGHT67 keeps 8:00 (7:30 minimum, the close from 7:00) but front-loads: every room's GATE is meant to land by 4:30, before
a likely stop near 5:00. Agents: **two builders (R, S) and one review slot**; builder T is spawned only for a tool fix a critic names.
Every slice commits its state; the log names each agent's id; a resume re-reads NIGHT67.md and sends each agent one SendMessage.

## D-2 The disk decides the branch at t0, as NIGHT66 (DECIDE66 D-1)
A >= 23 GB, A-lean 18-23 GB, C 7-18 GB (sim only), under 7 no rooms. C: was 13.3 GB at NIGHT66's close: branch C unless GG frees it
(DECIDE67 lists his commands; the night never deletes).

## D-3 The focus: a2 and a4 to their GATEs first, then a3, a5, a8
a2 (`M1 PASS 1.52 vs 1.25` on NIGHT66's r2; its stall guard out: the verb pilot's charge rule) and a4 (`M1 PASS 1.57 vs 1.42` in S's
round 2; one walk-bot death, S's carry-over pickup fixes seed 13) are each one guard from a GATE. a3, a5, a8 continue. b1 stays NIGHT65's
room (DECIDE66 D-3) unless GG's own message names it; b2 stays a slot (old) unless GG names it.

## D-4 Start from the carry-over, measured clean
NIGHT66's builders' last work is on main as carry-over (973b9c16: a3 THE SLAG RUN, a5 THE SPILL, a4's pickup, a8's filled tailrace),
unmeasured. NIGHT67's first main-tree round r1 measures it, with no pick, commit or slot write during it: that round is also T1c's line 3
proof (a clean round, tracked status equal before and after).

## D-5 The instruments carried over are judged first (slice T0)
On main but never judged: d25ea7d8 (the charge rule's reach: the verb-aware pierce trigger counts a body only when the charged shot's
ray hits it; n65-verbs.sh runs the DLL under `timeout 900`) and T1c's fix 2 (the table's rooms from the round, exact `marks`). T0: one
critic on them, with before / after on a2 and a8, then a2's and a8's red65 re-measured on the amended pilot and their mainBars recomputed
by the slate's formula (a3 / a4 / a5 keep theirs: their verbs' code did not change). Until T0 passes, the rooms' M1 lines are "not MAIN".

## D-6 A room's GATE: two held main-tree rounds, not four
NIGHT66's "four reviewed rounds" could not fit a night with a stop. NIGHT67's ROOM list: a room GATEs when M1-M4 and the GUARDS held on
two consecutive main-tree rounds on the T0-passed tool, each read by a level review; a room short of that is SHORT. The rounds' clock: r1
about 0:40, r2 1:40, r3 2:40, r4 3:40; GATEs from about 2:10.

## D-7 Three rules NIGHT66 learned (LESSONS L-150 as amended)
(a) A room's spawn is its retry point, never moved toward the verb's line (a3's NIGHT66 spawn move gamed the skip's start: put back in
r1). (b) No pick, commit or slot write while a round runs, even with --out. (c) The decoration checker saw 0 pieces in six rooms through
r2: a round should add or move a collider piece with a job (M4 then judges it).

## D-8 The close and the goal
n67-close-check.cjs reads the verbs37 side on n65-foundry only when the level row is ON (NIGHT66's close: vn RED 22 / 27 with the row OFF,
the grapple rows: forge67-make-n67.cjs; planted case 24). GOAL.txt drops the end-state description that NIGHT66's judge read as extra
conditions ("the clock stopped ... before 8:00") and says the script's last line is the verdict.

## The red team's amendments (redteam.md findings 1-17; they supersede the decisions above where they differ)
- **R-1 (1), amends D-3:** a2 is not one guard from a GATE: T's unjudged after-runs read M1 1.49 against a bar of 1.47 and stall V 29.1 %
  out of band. a4 first; a2 needs a room change. T0 re-runs T's after files on main and diffs them.
- **R-2 (2), amends D-6:** a GATE is one held main-tree round plus skip v2 on seeds 11-20 (the GATE pack's fresh skip), the room file
  unchanged since; the GATE critic writes that round's level review when none did (the sim is deterministic: two rounds of an unchanged
  room repeat). The review slot runs T0's critic first.
- **R-3 (3), amends D-1:** expect the first stop 2:30-3:30 after the usage window's start; DECIDE67 asks GG to start after a reset.
- **R-4 (4):** section 1's step 3 runs once and never clobbers (the slate, the reds, the flow base).
- **R-5 (5):** T0's planted hang runs at `N65_VERBS_TIMEOUT=60` (n65-verbs.sh); T's proofs run in T's tree; T0's line 1 re-worded to what
  the tools print; a T0-FAIL branch re-runs only the verb pilots on r1's saved level.
- **R-6 (6):** every red's verb lines re-run on the T0 tool before r1 (all six rooms' skip, a2 / a8's flow V); every bar rewritten.
- **R-7 (7):** the minimum close commits CORE.md too (a planted case: without it `--tag --minimum` refuses); section 9 names the
  after-8:00 `--tag --minimum` route for a --pre that fails only on a result.
- **R-8 (8):** GOAL.txt cut to its conditions; section 9 (m) names the `NIGHT67 CLOSED:` exit.
- **R-9 (9):** the GATE critic gets table-red65, the deco file, the levelgen report and the diff since e37083cf beside the pack; the pack's
  fresh skip has a 900 s timeout; n67-round-verbs.sh runs a3's `ride --shoot`; NIGHT66's r2 review copied into Logs/n67/r0n66/.
- **R-10 (10):** GUARDS are numeric ceilings per room in the slate (the higher of the two reds, on the T0 tool).
- **R-11 (11):** builders run skip through n65-verbs.sh (never a bare `dotnet run`), freeze a GATE candidate, commit their verb plans with
  every lever change, stop at 6:45.
- **R-12 (12):** r1's a3 lines do not count; R moves a3's spawn back first.
- **R-13 (13):** branch C's play build assembles slots65.json's set (`gen --run --out=Logs/n67/play-passed`), never the latest rooms.
- **R-14 (14):** every reviewer, spotter and critic prompt forbids dotnet in any tree.
- **R-15 (15):** DECIDE67 names the two stray Unity processes; builders stop at 6:45 for the close's headroom.
- **R-16 (16):** each builder's first commit adds its tree's untracked NIGHT66 evidence; HANDOFF lists it.
- **R-17 (17):** ROOMS overridable in n67-round.sh (NIGHT67 runs a2 a3 a4 a5 a8); the Fallback font restore before the tag; the n67
  tools' comments cite NIGHT67 files where the evidence is NIGHT66's (Logs/n66, Research/2026-10-23-wave66).
