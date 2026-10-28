# FORGE70 red team (fresh reviewer, 2026-10-25; read only; no dotnet / harness / build / Player / Editor)
Ran: `guard.cjs --selftest` (111 / 111), `n70-close-check.cjs --selftest` (30 cases PASS), `n70-next.cjs --selftest` (PASS), `n70-slate.cjs <seed>`
(7 slices, 7 todo), `bash -n` on n70-runner.sh / n70-throws.sh / room-chain.sh (OK), the guard on 26 of the night's commands (a scratch probe,
logs to the scratchpad: all allowed but `n41-build.sh n70-freeze`, disk 7.5 GB, as planned), git / grep / read-only node. C: 8 GB at 23:3x.
Not checked: the Windows sleep / power plan during a stop; run times with Z3's extra run; how T will name the committed run's lines.

## BLOCKERS

1. **The Act 2 red re-make cannot produce a red.** NIGHT70.md:74-75 runs `n65-verbs.sh skip Design/levels/$1.json $2 --seeds 20 --bars ...`
   on the SHIPPED levels, without `--plant-verb65`. Neither shipped room has a verb65 (node read of Design/levels/n57-kiln.json kb3 and
   n52-spire.json v2: `NO verb65`); NIGHT69 made these reds with `--plant-verb65 kb3:launch` (Logs/n69/red-kiln/kb3/skip.txt:1, the same for
   v2). Without it the harness prints `no verb65 in the row (and none planted)` and `M1 UNSURE`, exit 3 (Harness.N65.Verb.cs:1164-1168),
   over-writing red-<L>/<id>/skip.txt with no M2 line; `n70-red-bars.cjs --write` then reads `MISSING skip M2 VERB pooled ratio`
   (n70-red-bars.cjs:33-36), the bars are never re-made on Z3 and the a2 precondition (NIGHT70.md:78) never holds.
   Also: the command is printed over two lines but a job is `head -1` of its file (n70-runner.sh:33).
   **Fix:** a committed script `Tools/night/n70-red-z3.sh` (the loop, `--plant-verb65 $2:launch` in the command and its `# command` echo,
   then `grep -q '^n65verb skip: M2 VERB'` or exit 1); the job line is `bash Tools/night/n70-red-z3.sh`.
2. **None of FORGE70 is committed, so the builders' trees will not have the tools.** `git ls-files Tools/night | grep -c n70` = 0;
   `git status`: all 27 `Tools/night/n70-*`, `forge70-make-n70.cjs`, `Research/2026-10-25-forge70/` untracked, `.claude/hooks/guard.cjs`
   modified. Worktrees are switched to `<t0 sha>` (NIGHT70.md:55), so R / S cannot run `n70-throws.sh`, `n70-red.sh`, `n70-patch-share.cjs`
   (brief:117-121) and T's slices edit n70-gate-pack.cjs / n70-next.cjs / n70-m3-cases.sh that its branch lacks. FORGE69 committed its
   drafts before its night (0c8ab992, 7583c273). **Fix:** the forge commits these (explicit paths) before the night; section 3 step 1 adds
   `git ls-files --error-unmatch Tools/night/n70-runner.sh Tools/night/n70-throws.sh Research/2026-10-25-forge70/builder-brief.md`.
3. **Z3 inflates v2 with no bar change.** The bar is `max(1.25, redRatio + 0.20)` (n70-red-bars.cjs:4). v2's red is 0.87, so its bar stays
   1.25 unless the Z3 red exceeds 1.05. The faster of two verb-aware runs is the faster of two chaotic draws (per-seed sd 0.20-0.32,
   NIGHT70.md:36): its selection gain goes straight into v2's ratio. D-5's "a red that rises raises its bar" (DECISIONS.md:68, slate
   ROOM[4], NIGHT70.md:76) is false at the floor. **Fix (before any measurement):** bar_Z3 = max(1.25 + (red_Z3 - red_t0), red_Z3 + 0.20),
   in ROOM[4] and n70-red-bars.cjs; the M2 line also prints the plain-only ratio and how many seeds took the committed run.

## MAJORS

4. **A dead runner looks alive, and its leftovers stop the night.** `--status` never checks the pid (n70-runner.sh:18-22); runner.alive
   is written only between jobs (:29, :34), so a reaped runner and a 30-minute round print the same lines. A killed runner leaves
   runner.busy (the hold waits on it forever: NIGHT70.md:22) and `<job>.running` (that job never runs again: :31); a restart clears neither;
   the close waits for a runner.log `exit` that never comes (NIGHT70.md:91). Rule 6 says "never restart a job unasked", with nobody to ask.
   **Fix:** `--status` prints `DEAD (pid N)` when `kill -0` fails; start-up renames a stale `.running` to `.died` and removes busy; the prompt:
   a DEAD runner is restarted once, its died job re-queued as a new N, a `MISS:` logged; the close waits on the pid, not the log line.
5. **The hold can idle the whole stop, and it races.** The lead touches the hold, then waits for busy to clear (NIGHT70.md:22). A round takes
   18-27 min (Logs/n69/room-chain-1.txt: 10:40:37-11:07:16 for three rooms), so a usage stop that lands in that wait leaves the hold set, and
   the runner sleeps through the stop (n70-runner.sh:30). Race: a hold touched after :30's check but before :34 lets a job start under it.
   **Fix:** the runner ignores (and logs) a hold older than 15 min; the lead sets the hold only when busy is absent, waits about 20 s (one
   loop), checks busy again, and never waits under a hold. Name the wait: a Monitor until-loop, because the Bash tool refuses a foreground `sleep`.
6. **The stop computes ~30 of its ~160 minutes.** NIGHT70.md:72 queues only a1 ("about 30 minutes"). prompt-methods.md rule 4 (:76-78)
   asks for >= 1:45 of deterministic pools. E70 number (2) is therefore set by the prompt. **Fix:** at each 30-minute re-queue, add evidence jobs
   behind a1: each room's second-best committed candidate (a1b), seeds 31-50 on a1's rooms (the noise check), the shipped rooms' 20-seed throw charts.
7. **Z3 is last in T's order, and a2 has no Z3 deadline.** T's order is Z1 0:40, Z2 1:10, Z3 1:40. The stop can come at 1:00 (NIGHT70.md:13;
   DECIDE70.md:7-8 expects a first act of 1:00-1:15 after the forge). Act 2 then needs Z3, its critic (17-23 min, L-109), the reds in the queue,
   a2 (~30 min), the review, the packs and the critic: about 2:30 against a ~2:20 window. "a2 ... after Z1-Z3 are each PASS or SHORT"
   (NIGHT70.md:78) never says when Z3 becomes SHORT. **Fix:** order T Z1, Z3, Z2 (Z2 only prints); Z3 is SHORT at R2+0:30 if not picked,
   and a2 then runs on the t0 skip and says so.
8. **Z3 is under-specified for its critic.** (a) The plain verb-aware run must be byte-identical too: the doneWhen names only refused-in and
   walker (slate Z3 doneWhen[0]). (b) Which run's launches meet the card target and the screen: room-chain.sh:108 greps only `verb-aware:`
   seed lines, and the committed run's label is not fixed. On the committed run the targets (kb3 >= 6, v2 >= 4: brief:111) are close to
   automatic. (c) Are "exercised", STYLE and deaths read on the chosen run? (d) The doneWhen names a `--commit` flag, but the red, round and
   fresh commands pass none: each must print a line proving that the committed run ran (L-089). **Fix:** write (a)-(d) into Z3's doneWhen and the critic prompt.
   Count the target on the plain run: the room's job is to bring fresh bodies.
9. **The minimum close can deadlock the goal.** NIGHT70.md:94 says a running job is "report it, never kill it unasked". But `--tag --minimum`
   tags only when the failed checks are player / gates (n70-close-check.cjs:170-171; `running` is in PRE, :162). So there is no tag, and neither
   GOAL.txt branch can be met. **Fix:** "wait for it (Monitor, up to 30 min); a builder's job: stop the builder (TaskStop, then its PIDs: NIGHT52's TaskStop orphans)".
10. **The lead's own dotnet beside the runner.** PC at R2+1:30 runs `gen-slots.cjs --run` three times (dotnet build + levelgen:
    gen-slots.cjs:403-410) while a2 (queued at R2+1:00, about 30 min) runs. Rule 6 lists only "the flow base, rounds, reds, gates" (NIGHT70.md:21).
    **Fix:** PC's gen-slots runs are a queued job; rule 6 says "every dotnet / gen-slots --run / n70-throws in the main tree".
11. **E70 number (1) cannot be computed.** No file records when a stop began. stops.txt is written at the resume (NIGHT70.md:88), with the
    notice text and the resume time. The runner cannot see the session. **Fix:** `n70-clock.cjs` (run every turn: rule 1) appends
    `<time> turn` to Logs/n70/heartbeat.txt. A stop's start is the last heartbeat before the gap. The uncommitted minutes are that time minus the
    last commit on any branch (`git log --all --format=%ci`).

## MINORS

12. **M3 >= 10 / 20 does not discriminate, and the builder places its origin.** The shipped v2 reads 14 / 20 and kb3 r1d 14 / 20 (spike.md:24-25).
    The probe throws from line65[0] (n70-throws.sh:3-4), which the builder writes (ROOM[1]), and nothing checks that play punches there.
    **Fix:** PASS at >= max(10, the red's N + 3), and the critic checks that line65[0] is within ~3 m of the median in-play punch (Z2's launchend lines).
13. **The builders' paths are relative where the worktree has no Logs/.** brief:105, :107, :110 cite `Logs/n70/slate.json`,
    `Logs/n69/r1/review-level.md` and `Logs/n70/throws-<id>-shipped.txt`, and brief:101 says to run every command from the worktree.
    **Fix:** absolute main-tree paths, as brief:118 already does.
14. **Four agents against the forge's own rule 5.** prompt-methods.md:79-81 says at most 2 builders + 1 critic while the seat is shared.
    DECIDE70.md:7 says APEX S32's forge shares this window. NIGHT70.md:19 allows four. **Fix:** say which rule holds, and why.
15. **Worktrees R and S each carry 13 modified generated files** (`git -C <wt> status --short`: Design/levels/n67-*.json, n67/*.plan.json,
    openings67.json and their StreamingAssets copies). The blobs equal t0's, so `switch -c` carries them silently. **Fix:** log the list at
    the bearings; the brief says these are never staged or read.
16. **Queue order and cancel.** Jobs run in name order (n70-runner.sh:31), and nothing cancels a job. A stale a1c named before a2 delays it, and
    deletion is barred (rule 4). **Fix:** name jobs `<minutes since t0>-<name>`; cancel a job by writing its `.done` as `rc cancelled`.
17. **n70-next's window line is the fixed plan** (STOP_MIN 140 / RESUME 300 / STOP2 440: n70-next.cjs:22). It says "after it only files and
    docs", which is wrong after a 1:05 stop. It has no runner piece. **Fix:** read the last line of stops.txt and print the runner's liveness (finding 4).
18. **The card critic is unscheduled.** ROOM[0] asks for "one card critic pass", but Act 1's review slot holds only the Z1 + Z2 critic
    (NIGHT70.md:68-71), and the pack and n70-next check only that the card file exists (n70-next.cjs:61). **Fix:** drop it from ROOM[0], or give it a slot.
19. **Wording.** The `red-*-n68` copies are NIGHT69's reds (skip.txt sha 38a92116, 2026-10-25 04:39): name them `-n69`. The report skeleton is
    committed at 1:00 while a1 starts, against the corner "a lead write during a round" (NIGHT70.md:44, :71): say docs commits are allowed and
    only room, tool and slate files wait for the hold. "The drafts" (R2+1:40) are not listed: copy NIGHT69's list (report, log, HANDOFF,
    CORE's state line, ledger, slate).
20. **Length.** NIGHT70.md is 116 lines and 18,332 bytes (NIGHT69: 106 / 16,728): 8 rules, 9 sections, lines of about 158 characters. It is
    readable, but sections 5 and 8 pack 6-10 commands per line. Of prompt-methods' six rules, rule 4 is broken (finding 6) and rule 5 is
    contradicted (finding 14). The rest are present.

## Verdict
**Ship after the blockers** (1-3: one script, one commit, one bar formula), with findings 4, 5, 7 and 9 fixed in the same pass. Each is a few
lines in n70-runner.sh or NIGHT70.md. Without 4, 5 and 9, the two-act design can lose its stop or its close to one dead process. The design
of the night (two acts keyed to the resets, the queue, two rooms in depth, a measured target) is sound.
