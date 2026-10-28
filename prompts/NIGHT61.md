# NIGHT61.md: THE BUILD NIGHT (6 h minimum, 7 h hard cap): build what a player sees, polish, kill dead code, tell the truth

GG, 2026-10-22, verbatim: "research best ways to prompt and get better results, skills whatever ... so it can create a powerful goal and
loop prompt to build and not run into mistakes, polish, kill dead code, but need to see real results and it needs to be honest and
critical where things are not good."

Written by the forge (FORGE61.md; evidence and calls in `Research/2026-10-22-forge61/`: RETRO, RESEARCH, DECISIONS, redteam). Order of
authority: the org rules, GG's words above, CLAUDE.md, this file, HANDOFF.md, the logs. Read this whole file before anything, and again
after any compaction or resume. The /goal is `Research/2026-10-22-forge61/GOAL.txt`. **The night ends when section 9's close is done and
the last message is `NIGHT61 COMPLETE`.**

**Why tonight is different.** Nights 55-60: ~29.5 h of lead clock, ~47 % measuring, ~24 % waiting, ~25 minutes of the lead building; of
11 rows turned ON only 5 are visible in a level's first minute; the critics' top complaints since NIGHT55 (dark doors, floating lights,
a style meter stuck at DULL, a camera blind to speed, bosses that never say what they want, rooms that hide their fight) were never
touched (RETRO.md). Tonight builders build those, each is measured once at the end, and the report says what is still bad first.

## 0. Rules at a glance (all of CLAUDE.md's hard rules apply; these five broke most, the first three are now a hook)
1. **Disk** (L-104, L-113): the guard hook refuses a build or a reel set under 12 GB free; the watcher wakes you when space runs low;
   `n61-reels.sh` prunes your own frames beside every set.
2. **Docs and code only with Write / Edit** (rule 9, L-114): the hook blocks heredocs, `node -e` / `python -c` writes, `sed -i`, `perl -i`,
   redirects into docs or code, and `dotnet format` without `--verify-no-changes`.
3. **Git and the machine** (rules 1, 2, 10; L-047): explicit paths plus `.meta`, never GG's four files or the four TMP fonts; the hook
   blocks push, merge, pull, rebase, master, `reset --hard`, `checkout -- .`, `clean -f`, `stash`, `commit -a` / `--amend`, `branch -D`,
   tag deletes, `gh auth switch`, `unity test`, killing Unity.exe, deleting `.claude/worktrees` or a whole project folder, and edits in
   the web repo (`node .claude/hooks/guard.cjs --selftest`: 77 planted cases). If it blocks you, do it the allowed way; never script around it.
4. **The usage limit** (L-003, L-098): commit every step; resume from the slate and the log (section 8).
5. **Evidence** (L-014, L-107, L-108): nothing turns ON without a Player frame, a SPOT line and a critic PASS; no "done" without the
   command or frame behind it. `node Tools/night/n61-slate.cjs` refuses `passes: true` unless the verdict file starts `<id>: PASS` and
   a player-facing slice has a SPOT line that names a change.
Commits: one feature per commit series, never two features in one commit; a test-first commit stands alone so it can be seen RED
(CLAUDE.md's "one commit per feature", as every night since NIGHT55 has read it). "22 replays" = the golden harness's count today (CLAUDE.md
rule 3's 16 trajectories plus 6 added since).

## 1. Bearings (steps 1-3 every session, including after a compaction or a resume; steps 4-6 at the first session only)
1. `date` (the first one is t0: write it at the top of `NIGHT61-LOG.md`, the progress file, tracked), `pwd`, `git status --short`,
   `git log --oneline -15`, `git tag -l u3-n60` (must print). Read `NIGHT61-LOG.md`, the slate (`node Tools/night/n61-slate.cjs`; at
   the first session after step 4), docs/memory/LESSONS.md (every row is a rule), `DECIDE61.md` (untracked; create it at the first
   session: one D-id per call you make for GG, with the alternative rejected).
2. **Disk:** `df -h /c`. **With 12 GB or more free**, start the watcher as a background command (run_in_background):
   `bash Tools/night/n61-diskwatch.sh <warn> 8`, `<warn>` = the smaller of 15 and (free minus 3), so it never starts above the free space;
   when it exits, prune your own n61 frames (`bash Tools/night/n61-prune.sh <tag>`) and restart it with a new `<warn>`. **Under 12 GB**
   (it was 11.4 GB at the forge, 2026-10-22 09:10) the guard already refuses every build and reel: do not start the watcher; log it under
   Blocked and run the no-Player night (B1, B2's cause, D1 verified by the gates and the EditMode test batches with verbs37 owed, S-slices'
   code behind OFF rows with their Player evidence owed); re-check `df -h /c` at the top of every hour and resume Player work the moment 12
   GB are free. The report opens with it. Never delete anything you did not write tonight.
3. **The guard and the Editor:** `node .claude/hooks/guard.cjs --selftest` prints `77 / 77 as planted`. `bash Tools/unity-loop.sh status`
   shows `"playMode": "stopped"`. If it times out: first check whether a test run or a compile is still going (`Logs/tests-last.json`,
   the Editor log's tail); only then use the unfocused-Editor recipe (memory unity-editor-unfocused-autotick). Never kill the Editor. If
   it does not answer for 20 minutes, log it, keep building with `n61-gates.sh --no-editor`, and build Players in batch mode from
   `C:\Users\gaige\dev\SKYGRIND-unity-batch` (memory batch-build-and-editor-stall). That copy sits at NIGHT54's close (6462b106) with its
   own uncommitted changes and no remote: sync it only with `git -C C:/Users/gaige/dev/SKYGRIND-unity-batch fetch
   C:/Users/gaige/dev/SKYGRIND-unity port/stage3` then `git -C C:/Users/gaige/dev/SKYGRIND-unity-batch checkout --detach FETCH_HEAD`; if
   the checkout refuses, log it under Blocked (no batch fallback tonight: never force or stash there; the guard blocks both). A hung
   batch-mode Unity may be closed by its PID (the guard allows `-batchmode` Unity processes, never the Editor).
4. `mkdir -p Logs/n61 && cp Research/2026-10-22-forge61/slate-seed.json Logs/n61/slate.json`; `bash Tools/night/n61-gates.sh --flow-base`
   (~5 min; it also keeps `Logs/n61/flow-base-a0.txt`, the base every slice is judged against), then `bash Tools/night/n61-gates.sh`:
   all GREEN is the baseline (if not, fixing it is task one, for 45 minutes at most; then log it under Blocked and go on with what runs).
5. **One end-to-end check before new work** (if the disk allows): `bash Tools/night/n41-build.sh n61-a0`, `git diff --stat ProjectSettings`
   (only GG's two known files), `bash Tools/night/n61-reels.sh n61-a0 vk,n57-kiln,verbs37` (want 31 / 31, 0 NREs).
6. **The honest state check:** spawn `gg-proxy` on the newest frames (Logs/feel/n60-freeze/v*/verbs37, Logs/feel/n60-b7/v1on-*/a357,
   Logs/feel/n60-b8/b8on/a157, Logs/feel/n61-a0/vk if it ran) writing `Logs/n61/gg-proxy-start.md`. It may add slices (each with `red`,
   `doneWhen` and `visible` written before any number); it never removes one.

## 2. The slate and the visible rule
`Logs/n61/slate.json` (seeded; every field's source is in `Research/2026-10-22-forge61/slate-seed.json`): id, title, why, owner, row,
`red` (the before), `doneWhen` (hard thresholds written before any measurement, never loosened after), `visible` (what a player sees in
the first minute, with its frame), `passes`, `state` (todo / building / measuring / critic / pass / short / blocked), `verdict` and `spot`
(file paths). Only the lead edits it (Edit tool), prints it with `node Tools/night/n61-slate.cjs` after each change, and commits it with
`git add -f Logs/n61/slate.json`. A slice added or re-scoped tonight is committed before any of its numbers is measured (the commit's time
proves the thresholds came first). `n61-freeze-rows.cjs` refuses a row whose slice has not passed.

**The visible rule.** A player-facing slice turns ON only if GG would notice it in normal play. Take two frames from the same build and
pose that differ only by this slice's one-row switch (`-skygrindNo<Row>61`, or the row's old switch); a level-data slice with no switch
(S2) uses the same pose on the last build before its pick and the first build after it, every other new row OFF in both copies. Copy them to
`Logs/n61/spot/<6 random letters>/A.png` and `B.png` in a coin-flip order, spawn a fresh `skygrind-spotter` (it can only Read) with those
two paths and nothing else, and write its one line to `Logs/n61/spot/<id>.txt`. Log the coin flip and the mapping after the answer. One
SPOT per slice per build; never a re-run on a different pair (a new SPOT only after the slice itself changed and was rebuilt, logged). It
must name the slice's `visible` change: "NOTHING OBVIOUS" or another difference keeps the row OFF, whatever its numbers. Bug and
dead-code slices are exempt and go under their own report headings.

## 3. Order of work and owners (ranked by what a player sees, not by what is easy to measure)
After the bearings: **S1** the exit door reads as the way out (V; a redesign, not a third brightness tweak of lamp60), **S2** level 3's
last rooms show their fight from the door (L), **S3** every boss says what it wants (G), **S4** lights hang from fixtures (V), **S5** the
style meter climbs (G; a view row only: if its cause is in the sim, it ends SHORT with the cause and a proposal), **S6** the camera reacts
to speed (the next free builder), **S7** kiln's presses read as machines (L); the bugs **B1** focus60 vs the Player tours (D, first) and
**B2** Game M's hang (the lead: read the cause first; run Game M only in batch mode on the batch copy, never in the live Editor, which it
wedges); **D1** the dead-code pass (D, after B1). Then the beyond list: idle60's cut 2 (drones that climb: GG's "a route that enemies can
climb up"), NIGHT60-PROPOSALS P3 (a card taken on the pad), kill55's camera-jitter guard with kill55 firing, a person on level 3's 4.5 m
tiers. A slice whose MAIN misses twice tonight stops: SHORT with its numbers. **No sim row turns ON mid-night** (it would fail every
later plain-fingerprint gate); a sim change stays OFF with its evidence for GG.

## 4. One slice, start to finish
1. The builder gets `Research/2026-10-22-forge61/builder-brief.md` and its slice id; it measures `red` where it can, builds test-first
   behind its row, runs `bash Tools/night/n61-gates.sh --no-editor`, commits each step and reports shas plus the Player commands.
2. The lead picks as builders report (never during an Editor compile, L-067), runs the test RED on the test-first commit and GREEN after,
   `bash Tools/night/n61-gates.sh`, and re-reads the full diff. Every ~45 minutes (not more often: L-038) one Player build carries the
   picks so far: `bash Tools/night/n41-build.sh <tag>`, `git diff --stat ProjectSettings`, each new row ON in that copy only
   (`node Tools/night/n46-build-rows.cjs <tag> on <night.row>`).
3. Player evidence: the slice's reel on vs off in that build (`n61-reels.sh`), its meter with a planted case (L-093), the lead opens one
   frame on and one off (Read on the PNG); verbs37 x5 once per build. Force-add the evidence files the report will cite (`git add -f`).
4. SPOT (section 2), then one `skygrind-critic` (the GATE) with the slate entry, the frames, the meter files, the SPOT file and the
   shas (it reads patches without messages, never the builder's notes). PASS: `passes: true`, `state: pass`, `verdict` and `spot`
   set, `node Tools/night/n61-freeze-rows.cjs "<night.row>=<reason>"`, `bash Tools/night/n61-gates.sh --flow`, commit. A level-data
   slice (S2) re-makes `Logs/n61/flow-base.txt` only after its PASS, in its commit, saying so. FAIL / UNSURE: the row stays OFF;
   `state: short` with the number, or one fix and one more GATE. The GATE's read of the diff is the slice's fresh review; no other agent
   double-checks a builder. A critic or spotter not back within 40 minutes is stopped (TaskStop) and replaced once; a second failure
   leaves the row OFF, logged UNSURE "critic died".
5. Log every agent you spawn (its name and id) in `NIGHT61-LOG.md` under "Agents" the moment it starts: after a compaction SendMessage
   needs the id.

## 5. The dead-code pass (D1)
Dead means proven unreached, all four: no references (grep the symbol across Assets/, Tools/, Design/; the script's `.meta` guid in
scenes / prefabs / assets; `m_MethodName` UnityEvent targets; config strings, `-skygrind` flags, reflection; IDE0051 / IDE0052 hits are
leads only: the forge's probe missed planted dead code); not used by any ON row; not on the `--off57`..`--off61` paths (the ones the gates
fingerprint); no test or smoke run reaches it. The lead spawns `dead-code-auditor` once at A1 (it writes `Logs/n61/DEADCODE-candidates.md` in batches). Builder
D deletes one batch per commit; the lead then runs the Editor compile, golden 0 mismatches, 22 replays exact, fingerprints plain /
--off57..--off61 EQUAL, the touched test batches, and verbs37 x5 at the next build. One line per batch in `Logs/n61/DEADCODE.md` (what,
evidence, lines removed). Never: Mover.cs, the web-ported Core files kept line for line, the audio event names, GG's files, OFF rows GG
has not decided (they go on a list for him). A batch that breaks anything is reverted with `git revert` and leaves the list.

## 6. Delegation
At most four builders (L, V, G, D) and two critic slots (the spotter and the GATE share them) alive at once: the project settings cap
subagents at 6 concurrent and 1 level deep, and builder worktrees start at HEAD. `gg-proxy` and the auditor run only when fewer than six
agents are alive (A0, A1, the close prep). Spawn builders one at a time with `df -h /c` between. Builders on UI slices (S3, S5) load the
`unity:ui` skill first and use only the theme fonts in Assets/Resources/Fonts (CLAUDE.md rule 8). Every subagent writes its result to a
file and returns a short pointer. Resume a stopped builder with SendMessage (L-098), never a fresh copy.

## 7. The clock (t0 = the first `date`; print `date` and `h:mm / 7:00` at the start of every turn)
| from | to | what |
|---|---|---|
| 0:00 | 0:40 | bearings (section 1), the end-to-end check, gg-proxy's state check |
| 0:40 | 1:00 | A1: the shared commit (the `design.n61` block with `_why` in both config copies, `N61SharedTests` modelled on `N60SharedTests`, `fingerprint --off61` EQUAL), the dead-code-auditor, then builders D, V, L, G |
| 1:00 | 5:30 | continuous integration (section 4): picks as builders report, one Player build per ~45 min, SPOT and GATE as evidence lands |
| 5:30 | | **freeze**: no new pick after this except a fix to a picked slice; builders write notes and stop; builder L drafts the wave-memory harvest 61 (L-079) |
| 5:30 | 6:00 | the last GATEs and ON flips; the report and HANDOFF drafted (every section but the close lines); the dead-code ledger closed |
| 6:00 | 7:00 | **the close** (section 9): it starts no earlier than a printed `date` 6:00 after t0 and must end by 7:00 |
Work running out before 6:00 is not a reason to start the close: take the next beyond item or the next dead-code batch.

## 8. Usage limit and resume
Every step is committed; the state lives in `Logs/n61/slate.json` (force-added) and `NIGHT61-LOG.md`. After a stop or a compaction:
bearings steps 1-3, `node Tools/night/n61-slate.cjs`, resume each builder by SendMessage, log the gap with its times. A slice still in
progress at the freeze or the cap becomes SHORT with what was measured.

## 9. Honesty, the close and the report
- Every claim names its command or frame; anything not checked says "not checked"; a failed slice is reported as failed with its number;
  tests are never edited to pass (a wrong test is fixed only with the reason in the commit); critics report every problem and the lead
  filters with a logged reason. Every mistake becomes a LESSONS.md row from L-116 (`| L-NNN | rule | what happened, file, commit | scope |
  new | w61 |`).
- **The close (6:00-7:00; the boxes add up to 57 minutes).** The report, HANDOFF and the harvest are drafted by 6:00 (section 7), so the
  close fills in numbers, it does not write from scratch. Each step has a time box; a step that fails or runs over is logged under Blocked
  with its reason and the next step starts; when time is short, (e) is skipped first. (a) `bash Tools/night/n61-gates.sh`, shown (8 min);
  (b) `bash Tools/night/n41-build.sh n61-freeze` from HEAD and `git diff --stat ProjectSettings` (4); (c) `bash Tools/night/n61-reels.sh
  n61-freeze vf,n52-foundry,verbs37 vi,n52-iceline,verbs37 vk,n57-kiln,verbs37 vs,n52-span,verbs37 vp,n52-spire,verbs37` (8); (d) with no
  agent running, `bash Tools/night/n56-smoke-steal.sh n61-freeze` ("DONE: all steps passed") (7); (e) only if the clock reads 6:30 or
  earlier: `timeout 900 bash Tools/night/n41-suite.sh n61 Game ABCDEFGHIJKLNOPQRSTUVWXYZ` (never Game M in the live Editor) while
  `gg-proxy` ranks the freeze frames into `Logs/n61/gg-proxy-close.md` (15); (f) `node Tools/night/n61-slate.cjs`, shown, nothing in
  progress; finish `NIGHT61-REPORT.md`, HANDOFF.md, the harvest (ledger, THREADS <= 60 lines, FEATURES, CORE) and LESSONS (10); (g) `git
  tag -l` plus each ON row's `enabled` printed and matched to the report (L-107), commit with explicit paths, `touch
  Logs/n61/diskwatch.stop`, tag `u3-n61`, `git status --short NIGHT61-REPORT.md NIGHT61-LOG.md HANDOFF.md docs/memory/CORE.md` printing
  nothing (5); the final message `NIGHT61 COMPLETE`. At 7:00, whatever is left is logged under Blocked, then (g).
- **NIGHT61-REPORT.md, in this order:** (1) **What is still not good**: the 10 worst things in the game now, from gg-proxy's close ranking
  and the critics, each with its frame, whether or not tonight touched them; (2) what you can see tonight: each VISIBLE slice ON, its
  frame, its switch and a three-line PLAY note; (3) bugs fixed and dead code removed, with evidence and lines; (4) what failed, with its
  numbers; (5) not checked; (6) your calls (the OFF-row list from D1, anything only GG decides); (7) the close lines.
