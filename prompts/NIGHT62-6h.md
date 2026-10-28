# NIGHT62.md: THE PROOF NIGHT (6 hours): put NIGHT61's built rows in front of a Player, ship what passes, then build; tell the truth

GG, 2026-10-22, verbatim: "ok follow same process in how we built this last prompt into building next prompt but it will last for 6 hours".
The ask it carries forward (FORGE61, verbatim): "need to see real results and it needs to be honest and critical where things are not good."

Written by the forge (FORGE62; evidence and calls in `Research/2026-10-22-forge62/`: RETRO, RESEARCH, DECISIONS, redteam, GOAL.txt,
slate-seed.json, builder-brief.md). Order of authority: the org rules, GG's words above, CLAUDE.md, this file, HANDOFF.md, the logs. Read this
whole file before anything, and again after any compaction or resume. The /goal is `Research/2026-10-22-forge62/GOAL.txt`. **The night ends
when section 9's close is done and the last message is `NIGHT62 COMPLETE`.**

**Why tonight is different.** NIGHT61 built five visible slices, a bug fix and 370 lines of dead-code removal, and GG saw none of it: C: was
under 12 GB, so no Player could be built, and the Editor never answered (NIGHT60's close left MapChoiceTests running in it). All of it sits
behind OFF rows, compiled and gated (THREADS T-148). Tonight's first hours turn that code into frames and verdicts; new work comes after.

## 0. Rules at a glance (all of CLAUDE.md's hard rules apply; the guard hook enforces the first three)
1. **Disk** (L-104, L-113, L-119): the guard refuses a build or a reel set under 12 GB; a builder deletes its compile scratch (about 681 MB)
   after every check; `n62-reels.sh` prunes beside every set.
2. **Docs and code only with Write / Edit** (rule 9, L-114): the guard blocks heredocs, `node -e` / `python -c` writes, `sed -i`, `perl -i`,
   redirects into docs or code.
3. **Git and the machine** (rules 1, 2, 10; L-047): explicit paths plus `.meta`, never GG's four files or the four TMP fonts. The guard blocks
   push, merge, pull, rebase, master, `reset --hard`, `checkout -- .`, `clean -f`, `stash`, `commit -a` / `--amend`, `branch -D`, tag deletes,
   `gh auth switch`, `unity test`, killing Unity.exe, deleting `.claude/worktrees` or a project folder, edits in the web repo, **a synchronous
   test run that reaches MapChoiceTests** (game-m-sync, T-147) and **a background `&` that ends an `&&` chain** (bg-chain, L-116)
   (`node .claude/hooks/guard.cjs --selftest`: 92 / 92 as planted). If it blocks you, do it the allowed way; never script around it.
4. **The usage limit** (L-003, L-098): commit every step; resume from the slate and the log (section 8).
5. **Evidence** (L-014, L-107, L-108): nothing turns ON without a Player frame, a SPOT line and a critic PASS; no "done" without the command or
   frame behind it. `node Tools/night/n62-slate.cjs` refuses `passes: true` without a "<id>: PASS" verdict and, for a visible slice, a SPOT line.
Commits: one feature per commit series; a test-first commit stands alone so it can be seen RED. "22 replays" = the golden harness's count.

## 1. Bearings and the two preconditions (steps 1-3 every session, including after a compaction or a resume; 4-7 the first session only)
1. `date` (the first is t0: write it at the top of `NIGHT62-LOG.md`, the progress file, tracked), `pwd`, `git status --short`,
   `git log --oneline -15`, `git tag -l u3-n61` (must print). Read `NIGHT62-LOG.md`, the slate (`node Tools/night/n62-slate.cjs`, after step 5
   at the first session), docs/memory/LESSONS.md (every row is a rule), `DECIDE62.md` (untracked; create it at the first session: one D-id per
   call you make for GG, with the alternative rejected).
2. **Disk:** `df -h /c`. With 12 GB or more free, start the watcher as a background command: `bash Tools/night/n62-diskwatch.sh <warn> 8`,
   `<warn>` = the smaller of 15 and (free minus 3); when it exits, prune your own frames (`bash Tools/night/n62-prune.sh <tag>`) and restart it.
3. **The Editor:** `timeout 60 bash Tools/unity-loop.sh status` must print `"playMode": "stopped"`. No wake by window tricks (L-118): if it
   does not answer, log it under Blocked and ask again at the top of every hour. Never kill the Editor.
4. **Pick the branch by the two preconditions, and log it** (FORGE62 D-2):
   - **A:** 12 GB free and the Editor answers: the full night (section 3).
   - **B:** 12 GB free, the Editor deaf: Player builds in batch mode from `C:\Users\gaige\dev\SKYGRIND-unity-batch`, synced only by
     `git -C C:/Users/gaige/dev/SKYGRIND-unity-batch fetch C:/Users/gaige/dev/SKYGRIND-unity port/stage3` then
     `git -C C:/Users/gaige/dev/SKYGRIND-unity-batch checkout --detach FETCH_HEAD` (if the checkout refuses, log it and take C); EditMode checks
     through T1's runner once it lands, else "not checked". T1's runner is an early warning, never the Editor's run: a test it prints as
     needs-Unity stays "not checked" (RESEARCH.md section 3).
   - **C:** under 12 GB: no Player. Only work that can be proven without one: T1, R1, D2, P8 if the Editor answers, and the leftovers that need
     no Player. **No new visible slice is built** (NIGHT61's five already wait). The report opens with it.
   Re-check `df -h /c` and the Editor at the top of every hour; move up a branch the moment both allow.
5. `mkdir -p Logs/n62 && cp Research/2026-10-22-forge62/slate-seed.json Logs/n62/slate.json`; `bash Tools/night/n62-gates.sh --flow-base`
   (~5 min; it keeps `Logs/n62/flow-base-a0.txt`, the base every slice is judged against), then `bash Tools/night/n62-gates.sh` (branch A) or
   `--no-editor` (B, C): GREEN is the baseline (if not, fixing it is task one for 45 minutes at most; then Blocked and go on).
6. **A1 (the lead, ~20 min):** the `design.n62` block with a `_why` in both config copies (byte-identical), `N62SharedTests` modelled on
   `N61SharedTests`, `fingerprint --off62` EQUAL, commit.
7. The honest state check is NIGHT61's (`Logs/n61/gg-proxy-start.md`): no newer frames exist yet. gg-proxy ranks the new frames at the close.

## 2. The slate and the visible rule
`Logs/n62/slate.json` (seeded; every field's source is in `Research/2026-10-22-forge62/slate-seed.json`): id, title, why, owner, row, `red`,
`doneWhen` (hard thresholds written before any measurement, never loosened after), `visible`, `passes`, `state` (todo / building / measuring /
critic / pass / short / blocked), `verdict`, `spot`. Only the lead edits it (Edit tool), prints it with `node Tools/night/n62-slate.cjs` after
each change, and commits it with `git add -f Logs/n62/slate.json`. A slice added or re-scoped tonight is committed before any of its numbers.

**The visible rule.** A player-facing slice turns ON only if GG would notice it in normal play. Take two frames from the same build and pose
that differ only by this slice's one-row switch (`-skygrindNo<Row>61` for P1-P5, `-skygrindNo<Row>62` for tonight's rows). Copy them to
`Logs/n62/spot/<6 random letters>/A.png` and `B.png` in a coin-flip order, spawn a fresh `skygrind-spotter` with those two paths and nothing
else, and write its one line to `Logs/n62/spot/<id>.txt`; log the coin flip and the mapping after the answer. One SPOT per slice per build; a
new SPOT only after the slice itself changed and was rebuilt. It must name the slice's `visible` change: "NOTHING OBVIOUS" or another
difference keeps the row OFF, whatever its numbers. Bug, test and dead-code slices are exempt and go under their own report headings.

## 3. Order of work and owners (branch A; B and C at the end)
1. **P0, 0:40-2:30: NIGHT61's five rows in one build (P1-P5, the lead).** `bash Tools/night/n41-build.sh n62-p0`, `git diff --stat
   ProjectSettings` (only GG's files), then `node Tools/night/n46-build-rows.cjs n62-p0 on n61.exitFrame61 n61.bossRule61 n61.text61
   n61.fixture61 n61.gunYellow61`. For each row, its reel pair from its builder's notes (Research/2026-10-22-wave61/V.md, G.md, V-S4.md,
   V-C3.md) with the OFF side by that row's own switch in the same build, its meter with a planted case (L-093), its proof line grepped; then
   verbs37 x5 once: `bash Tools/night/n62-reels.sh n62-p0 vf,n52-foundry,verbs37 vi,n52-iceline,verbs37 vk,n57-kiln,verbs37
   vs,n52-span,verbs37 vp,n52-spire,verbs37`. Then SPOT and GATE per row (section 4), ON by `node Tools/night/n62-freeze-rows.cjs
   "n61.<row>=<reason>"`, `bash Tools/night/n62-gates.sh --flow`, commit. A row whose review notes (Logs/n61/review-n61.md) would change what
   its SPOT sees waits for R1's fix and a rebuild.
2. **P6-P8 alongside, in the Editor (the lead):** B1's test by name; D1's eleven test classes by name, one class per call; MapChoiceTests alone in
   async mode (`unity command run_tests --mode EditMode --filter Skygrind.Game.Tests.MapChoiceTests. --async_tests true --timeout 2400 --caller
   plugin --skill unity-cli`, polled with `test_status`), then the other Game M classes by name.
3. **Builders from 0:45**, one at a time with `df -h /c` between (builder-brief.md): **D** T1 then D2; **V** S6 then S7; **G** R1 then C2.
4. **The second build at ~3:30, `n62-b2`:** tonight's new rows on in its copy; their reels, SPOTs and GATEs.
5. **Leftovers, ranked** (about twice the building time; take the next one whenever the list above is DONE or SHORT): S2 (level 3's last rooms
   show their wave: NIGHT61's slate entry, level data, judged against flow-base-a0); THREADS T-146's open items (rim56 reads its row twice a
   frame; an Editor run whose cursor never locked; n60-v-lamp.py ignores "cleared"); the style meter's measurement reel with a varied chain (S5's
   proposal; a sim change stays GG's call); NIGHT60-PROPOSALS P3 (a card taken on the pad).
**Branch B:** the same order, with the builds from the batch copy and the EditMode checks by T1's runner. **Branch C:** T1 (D), R1 (G), D2
(D), T-146's items; P8 if the Editor answers; no new visible slice. A slice whose MAIN misses twice tonight stops: SHORT with its numbers. No sim
row turns ON mid-night (it would fail every later plain-fingerprint gate).

## 4. One slice, start to finish
1. The builder gets `Research/2026-10-22-forge62/builder-brief.md` and its slice id; it measures `red` where it can, builds test-first behind
   its row, compiles (dn-check-wt.sh), deletes the scratch, runs `bash Tools/night/n62-gates.sh --no-editor`, commits each step and reports its
   own shas plus the Player steps.
2. The lead picks as builders report (never during an Editor compile, L-067) by `git cherry-pick <builder's last picked sha>..<tip>` with the
   builder branch's own shas (L-117); a config conflict on a row is resolved by HEAD's copies plus the builder's own row script, dry run first
   (L-049). Then the test RED on the test-first commit and GREEN after (in the Editor; in branch B by T1's runner, saying so), `bash
   Tools/night/n62-gates.sh`, and the full diff read. One combined `dn-check-wt.sh` per pick set when no builder is compiling.
3. Player evidence: the slice's reel on vs off in the build that carries it, its meter with a planted case, one frame on and one off opened by
   the lead (Read on the PNG); verbs37 x5 once per build. Force-add the evidence files the report cites (`git add -f`).
4. SPOT (section 2), then one `skygrind-critic` (the GATE) with the slate entry, the frames, the meter files, the SPOT file and the shas (it reads
   patches without messages). PASS: `passes: true`, `state: pass`, `verdict` and `spot` set, `node Tools/night/n62-freeze-rows.cjs
   "<night.row>=<reason>"`, `bash Tools/night/n62-gates.sh --flow`, commit. FAIL / UNSURE: the row stays OFF; `state: short` with the number, or
   one fix and one more GATE. A critic or spotter not back within 40 minutes is asked by SendMessage first (an empty transcript is not a stuck
   agent, L-120), then stopped and replaced once; a second failure leaves the row OFF, logged UNSURE.
5. Log every agent you spawn (its name and id) in `NIGHT62-LOG.md` under "Agents" the moment it starts.

## 5. The dead-code pass (D2)
NIGHT61.md section 5's rule, unchanged: dead means proven unreached, all four (no references incl. scene / prefab GUIDs, `m_MethodName`, config
strings, `-skygrind` flags, reflection; not used by an ON row; not on the `--off57`..`--off62` paths; no test or smoke run reaches it), re-proven
on HEAD. Tonight's batches: 37-50 of `Logs/n61/DEADCODE-candidates.md`. Builder D deletes one batch per commit; one line per batch in
`Logs/n62/DEADCODE.md`. Never Mover.cs, the web-ported Core files, the audio event names, GG's files, or OFF rows GG has not decided on.

## 6. Delegation
At most three builders (D, V, G) and two critic slots (the spotter and the GATE share them), plus one `skygrind-reviewer` on the night's diff at
the freeze (FORGE62 D-8) and `gg-proxy` at the close, never more than six agents alive (the project caps subagents at 6 concurrent, 1 deep).
Builders on UI slices load the `unity:ui` skill first and use only the theme fonts. Every subagent writes its result to a file and returns a
short pointer. Resume a stopped builder with SendMessage (L-098), never a fresh copy.

## 7. The clock (t0 = the first `date`; print `date` and `h:mm / 6:00` at the start of every turn)
| from | to | what |
|---|---|---|
| 0:00 | 0:40 | bearings, the branch (section 1), the baseline gates, A1 |
| 0:40 | 2:30 | P0: the build, NIGHT61's five rows' reels, SPOTs, GATEs and ON flips; P6-P8 in the Editor; builders from 0:45 |
| 2:30 | 4:15 | builders' picks as they report; the second build at ~3:30 with tonight's rows; their SPOTs and GATEs |
| 4:15 | 4:30 | **freeze**: no new pick except a fix to a picked slice; builders write notes and stop; `skygrind-reviewer` on the night's diff |
| 4:30 | 5:00 | the last GATEs and ON flips; the report, HANDOFF and the harvest drafted (every section but the close lines) |
| 5:00 | 6:00 | **the close** (section 9): it starts no earlier than a printed `date` 5:00 after t0 and must end by 6:00 |
Work running out before 5:00 is not a reason to start the close: take the next leftover.

## 8. Usage limit and resume
Every step is committed; the state lives in `Logs/n62/slate.json` (force-added) and `NIGHT62-LOG.md`. After a stop or a compaction: bearings
steps 1-3, `node Tools/night/n62-slate.cjs`, resume each builder by SendMessage, log the gap with its times. A slice still in progress at the
freeze or the cap becomes SHORT (or BLOCKED, with the precondition that stopped it) with what was measured.

## 9. Honesty, the close and the report
- Every claim names its command or frame; anything not checked says "not checked"; a failed slice is reported as failed with its number;
  tests are never edited to pass (a wrong test is fixed only with the reason in the commit); critics report every problem and the lead filters
  with a logged reason. Every mistake becomes a LESSONS.md row from L-127 (`| L-NNN | rule | what happened, file, commit | scope | new | w62 |`).
- **The close (5:00-6:00; the boxes add up to 54 minutes).** The report, HANDOFF and the harvest are drafted by 5:00, so the close fills in
  numbers. Each step has a time box; a step that fails or runs over is logged under Blocked with its reason and the next step starts; when time
  is short, (e) is skipped first. (a) `bash Tools/night/n62-gates.sh`, shown (8 min; its Editor step fails in about a minute when the Editor
  is deaf); (b) `bash Tools/night/n41-build.sh n62-freeze` from HEAD and `git diff --stat ProjectSettings` (6); (c) `bash
  Tools/night/n62-reels.sh n62-freeze vf,n52-foundry,verbs37 vi,n52-iceline,verbs37 vk,n57-kiln,verbs37 vs,n52-span,verbs37
  vp,n52-spire,verbs37` (8); (d) with no agent running, `bash Tools/night/n56-smoke-steal.sh n62-freeze` ("DONE: all steps passed") (7); (e)
  only if the clock reads 5:25 or earlier: `gg-proxy` ranks the freeze frames (Logs/feel/n62-freeze/v*/verbs37) into
  `Logs/n62/gg-proxy-close.md` (10); (f) `node Tools/night/n62-slate.cjs`, shown, nothing in progress; finish `NIGHT62-REPORT.md`, HANDOFF.md,
  the harvest (ledger, THREADS <= 60 lines, FEATURES, CORE) and LESSONS (10); (g) `git tag -l` plus each ON row's `enabled` printed and matched
  to the report (L-107), commit with explicit paths, `touch Logs/n62/diskwatch.stop`, tag `u3-n62`, `git status --short NIGHT62-REPORT.md
  NIGHT62-LOG.md HANDOFF.md docs/memory/CORE.md` printing nothing (5); the final message `NIGHT62 COMPLETE`. In branch C, (b)-(d) are refused
  by the guard: log each under Blocked with the guard's line, and do not run the smoke without a build (its planted steals would only take
  GG's window). At 6:00, whatever is left is logged under Blocked, then (g).
- **NIGHT62-REPORT.md, in this order:** (1) **What is still not good**: the 10 worst things in the game now, from gg-proxy's close ranking (or
  its start ranking when no new frame exists) and the critics, each with its frame; (2) what you can see tonight: each VISIBLE slice ON, its
  frame, its switch and a three-line PLAY note; (3) bugs fixed, tests proven and dead code removed, with evidence and lines; (4) what failed,
  with its numbers; (5) not checked; (6) your calls (the OFF rows GG decides, the disk, the Editor); (7) the close lines.

**Closing reminder (the three rules nights like this one dropped):** check the two preconditions before planning visible work, and say which
branch the night is on; nothing turns ON without a frame, a SPOT and a critic, and a GREEN without the Editor says whose harness gave it; put a
background job in its own statement, cherry-pick from the builder's own shas, and copy row names from the slate.
