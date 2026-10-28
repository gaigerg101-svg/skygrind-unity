# NIGHT66.md: THE FOUNDRY EARNED (8 hours; 7:30 minimum; 8:00 hard stop): level 1's rooms made to need their verbs, then shipped

GG, 2026-10-23 ~20:50, verbatim: "can we follow same methods for creating next detailed night prompt that will go for 8 hours?". His room
brief (2026-10-13): "one might encourage the dash jump another 1 encourage wall riding"; "they can't be too exploitable"; "you should be
flying through and ... killing the enemies before you enter the next room in a stylish fashionable way". His proxy's prediction of his first
words on last night's rooms (Logs/n65/gg-proxy-final.md): "The rooms still don't incentivize wall-riding or creative use of the weapons"
(2026-10-09). And: "more elements that are not just decoration" (2026-10-14).

Written by the forge FORGE66 (Research/2026-10-23-forge66/: DECISIONS D-1..D-12 and the red team's amendments R-1..R-14, history.md,
n65-data.md, research.md, code-scout.md, redteam.md, GOAL.txt, slate-seed.json, builder-brief.md). Order of authority: the org rules, GG's
words, CLAUDE.md, this file, HANDOFF.md, the logs. **Read this whole file first and again after any compaction or resume.** The night ends
when section 9's close is done: `node Tools/night/n66-close-check.cjs` prints `CLOSE-CHECK n66: PASS` and the last message is
`NIGHT66 COMPLETE`, at 7:30 or later and before 8:00.

## Where we are, honestly
- **NIGHT65 rebuilt level 1's six fight rooms** (Design/levels/n65/rooms/<id>.json; n65-foundry behind design.n65.foundry65, OFF;
  slots65.json all old). On the sim, the old rooms against the final ones: room checks PASS 1 -> 5 / 6, verb pilots 6 / 6 working, the
  verb-aware skip test 0 -> 3 / 6 GREEN. **The GATE: 0 / 6**: the cards' MAIN sat on the walk bot's DULL share (it never uses a verb:
  L-150) and every room's doneWhen held Player lines in a night with no Player (history.md corners N1, N2).
- **The skip test is not yet trustworthy** (n65-data.md section 6): the GREENs arrived with the test, not the rooms; its refused run swaps
  the whole verb-aware layer out; seeds where the verb never fired still read the GREEN bar. a5's pour has never taken a body. The rooms
  that stayed RED read a2 1.12, a3 0.71, a8 0.70 (n65-data.md:62-72), and five rounds never moved them.
- **gg-proxy on the final rooms:** b1 YES; a3, a5 UNSURE; a2, a8, a4 NO ("no ride", "still the old shape").
- **b2 THE GANTRY**, GG's 2026-10-14 "awful" ride (Research/2026-10-14-wave42/GG-REQUEST.md:21), was answered by w42 ("THE GANTRY
  forgiving", riders 40.4 -> 100 %; GG's next play: "major leap in quality") and changed again in w43 and w46. It is not a live complaint:
  tonight it becomes a slot (T2), and a room only if GG's own message names it.
- **The disk:** C: 9 GB free at the forge's red team (13.2 at NIGHT65's t0). No Player ran in NIGHT65. GG's play build
  Builds/n65-play-latest is n64-freeze's code with NIGHT65's data; whether it boots n65-foundry was not run.
- **GG has not played since 2026-10-20.** If he has answered DECIDE65 / DECIDE66 or played n65-play-latest before t0, his words override
  this file's room priorities (log it as D-1 in DECIDE66).

## The target, said plainly
**Six rooms of n65-foundry (a2, a3, a5, b1, a8, a4) that a walker cannot clear as well as a player using the room's verb**, measured by skip
v2 (the verb refused inside the same pilot, against the faster of that refusal and the plain walker, over a bar measured on NIGHT65's own
rooms), each with pieces that do jobs (the decoration checker), PASSed by a sim-side GATE and set new in slots65.json. In branch A or
A-lean the level row turns ON by its own slice LV in a Player. **Done well = three or more rooms PASS on M1-M4 with the guards held, nothing
lost (verbs37 n65-foundry 27 / 27 where a Player runs), GG's play build with the passed rooms.** Not tonight: other levels, bosses,
Mover.cs, b2's room without GG's word, new enemy types or movement economy rows without GG's own message (DECIDE66).

## 0. Rules at a glance (all of CLAUDE.md's hard rules apply; the guard hook enforces many)
1. **The clock** (`node Tools/night/n66-clock.cjs`, every turn): BUILD 0:00-7:15; FREEZE 7:15-8:00 = the close (only builds, reel sets
   and the smoke tagged `n66-freeze`; no test run; no new agent); CAP 8:00. **The close starts between 7:00 and 7:15** with
   `node Tools/night/n66-close-check.cjs --start`; the final message at 7:30 or later. Never restart t0; never edit Logs/clock.json.
2. **The disk is a go / no-go at the bearings** (D-2 as amended, R-2), logged as DECIDE66 D-1 with the numbers:
   - **A, 23 GB or more:** the code build b0, a Player reel set per round on a data-swap copy, and LV.
   - **A-lean, 18-23 GB:** b0, one `tour,n65-foundry,tour65` + `v,n65-foundry,verbs37` reel set at about 1:00, and LV; no per-round reels.
   - **C, 7-18 GB:** sim only (like NIGHT65); with 12 GB or more after the GATEs, one Player boot of n66-play-latest's level 1 (a tour65
     reel). Under 12 GB the guard refuses every build, reel set and smoke.
   - **Under 7 GB: no rooms** (T alone runs the instruments; the report opens with it).
   In every branch the builders reuse NIGHT65's three worktrees (bearings step 4: no new disk). Builders stop at 5 GB free (the pagefile
   shares C:, L-128). Keep only the newest round copy; prune reel frames after every set. **The night never deletes older nights' builds
   or worktrees** (the org rules reserve a deletion to a person; DECIDE66 lists GG's commands).
3. **Docs and code only with Write / Edit** (rule 9): no heredocs, `node -e` / `python -c` writes, `sed -i` (the guard refuses it).
4. **Git, the machine and GG's calls:** explicit paths plus `.meta`; never GG's four files or the four TMP fonts; no push, merge, rebase,
   master, `reset --hard`, `stash`, `commit -a` / `--amend`, `branch -D`, moving or deleting a tag (the guard refuses it), `unity test`,
   killing Unity.exe. **The tag u3-n66 is made only by `node Tools/night/n66-close-check.cjs --tag`** (after its pre-check passes), on the
   close's last commit. GG's answers in DECIDE66 steer priorities only: a deletion, a bulk change or a D-12 lever is done only on GG's own
   message in this session (or his /goal text); otherwise it goes under `## Blocked` with the commands for him.
5. **Agents: at most four subagents running at once.** Until round 4: builders R, S, T and one review slot. T ends its turn when its
   slices are done and is resumed by SendMessage only for an instrument fix (its slot then serves reviews too). After round 4's reports the
   builders end their turns, so the GATE's spotters and critics run in parallel. One Player at a time. Read commit free in the same command
   before every build, reel set, smoke or gate run (under 15 GB: hold, log the top processes).
6. **Background, not foreground; one job per tree:** every reel set, gate run, build, smoke, round, critic, reviewer and gg-proxy in the
   background. **One dotnet job in the main tree at a time** (queue the GATE packs: each runs a skip). **No build, GATE pack or `git add
   Design/levels` while a round runs** (a round writes the shipped level files until T1c's `--out` lands, red team 11). Never pipe a long
   script into `head`. End a waiting turn with a tool call (a wait loop), never with a bare message while work runs.
7. **Every output a verdict cites is a file under Logs/n66/** the moment it prints, with the command, the sha and the time (L-149). Never
   `git add -f` a round's level copy (Logs/n66/<tag>/n65-foundry.json, 2.5 MB).
8. **A PASS binds to its sha, and the slots are the lead's:** `n65-slot-set.cjs <room> new --slate=Logs/n66/slate.json` writes
   slots65.json only if the room file is unchanged since passSha. The level row flips only by LV through `n66-freeze-rows.cjs` (it binds
   the level files and slots65.json, against the working tree).
9. **Reviewers see evidence, not the story:** frames or iso sheets, the measured files, the verb plan, the diff; no builder notes.
10. **Never repeat the corners** (history.md section 3, N1-N16): MAIN on an instrument that cannot see the verb; a Player-only doneWhen in
    a night that cannot run a Player; card-gate cycles; decoration in place of pieces with jobs; a skip test whose refusal never fires or
    stands the pilot still; a pilot that starts standing instead of from the flight; round tools never dry-run; NIGHT65's folders written
    by NIGHT66 commands; a tag before the last commit; commits after the clock stop. A reviewer's BLOCKING finding names a piece, a rule or
    a GG quote, and a one-round fix.
11. **The usage limit** (L-003): commit every step; resume agents by SendMessage (their ids in the log); the report committed by 3:30 and
    by 6:30; after any stop past 7:00, the minimum close (section 7) when the close starts at 7:30 or later, else the full close.

## 1. Bearings (steps 1-3 every session; 4-7 the first session only)
1. First session: `node Tools/night/n66-clock.cjs start`. Every session: the clock, `git status --short`, `git log --oneline -5`,
   **HANDOFF.md's "Verify on arrival" table** (each line logged), `claude --version`, the branch said aloud. t0 at the top of NIGHT66-LOG.md
   with `git rev-parse --short HEAD` (the t0 sha: the decoration checker's `--base`).
2. **The Editor:** `timeout 60 bash Tools/unity-loop.sh status` ("playMode": "stopped"), then `timeout 45 unity command set_autotick --enable
   true --interval_ms 16 --caller plugin --skill unity-cli`. Commit free and `df -BG /c | tail -1`: **the go / no-go (rule 2), DECIDE66 D-1.**
3. `mkdir -p Logs/n66 && cp Research/2026-10-23-forge66/slate-seed.json Logs/n66/slate.json`; then `bash Tools/night/n66-gates.sh
   --flow-base > Logs/n66/flow-base-run.txt 2>&1` (background, about 10 min).
4. **The worktrees (0:00-0:05), reused, no new disk:** T -> `.claude/worktrees/agent-af48e888c59fa3626` (n65-T3, clean); R ->
   `agent-a9d0ffb6d0dfdb2be` (n65-S5); S -> `agent-aeb9fc116ba05ef4d` (n65-R5). For each: `git -C <wt> status --short`; S5's and R5's hold
   exactly the seven generated files of NIGHT65's last `--run` (Assets/StreamingAssets/Design/levels/n65-foundry.json,
   Design/levels/n65-foundry.json, Design/levels/n65/foundry.assembly.json, foundry.json, foundry.plan.json, openings65.json,
   Reports/LEVEL-n65-foundry-validate.md): check `git -C <wt> diff --stat` names only those, then `git -C <wt> checkout -- <the seven>`
   (anything else dirty: that builder gets a fresh isolated worktree instead, logged). Then `git -C <wt> switch -c n66-<L> <t0 sha>`. R5's
   tree keeps NIGHT65 R's untracked probe Tools/SimHarness/Harness.R65.DashJump.cs and tmpR/ (S's tree): if S's harness build fails on the
   probe, the lead moves that one file to Research/2026-10-23-wave65/R-probe/ and logs it. Log each path, branch and sha.
5. **At 0:05 spawn the builders** in the background **without worktree isolation**, each told its absolute worktree path,
   `Research/2026-10-23-forge66/builder-brief.md`, t0, its deadlines and the t0 sha. Log each agent id.
6. **A1 (the lead, 0:10-0:40):** the slate's MAIN lines checked against D-5 as amended; the self-tests (`node Tools/night/n66-freeze-rows.cjs
   --selftest`: 5 cases; `node Tools/night/n66-close-check.cjs --selftest`: `SELFTEST PASS (23 cases)`); **the round tools' dry run** once
   the flow base is done: `bash Tools/night/n66-round.sh dry0 --shipped > Logs/n66/dry0-run.txt 2>&1` (background, about 15-20 min), then
   `bash Tools/night/n66-gate-pack.sh a2 dry0` (its verbs65-skip.txt header prints `--seeds 10`) and `git status --short Logs/n65`
   (nothing). In branch A / A-lean, **b0** once T1a lands (about 0:50: `bash Tools/night/n41-build.sh n66-b0`), then its tour reel (T1a).
7. **The verb-plan critic (0:45-1:00):** one fresh skygrind-critic reads the six verb plans in one pass; open items become round 1's list.

## 2. The slate and the evidence
Logs/n66/slate.json (FORGE65's fields plus passSha / passFiles and each room's mainBar). Only the lead edits it (`n66-slate-set.cjs`,
`n66-slate-add.cjs`) and commits it with `git add -f`. A doneWhen is written before any measurement and never loosened. **A room's GATE is
sim-side** (D-8): M1-M4 and the GUARDS, the blind SPOT on the iso pair (or the tour65 pair in branch A), the reviews, the critic. **A room's
MAIN** (D-5 as amended, R-3), fixed now:
- **M1:** skip v2 runs three runs per seed: the verb-aware pilot, the same pilot with the verb refused inside it, the plain walker. It
  prints `M1 PASS` when the median over the exercised seeds of the paired ratio (the faster refusal's clear time / the verb-aware clear
  time) is at or above the room's **mainBar**, with 7 or more of 10 seeds exercised. **mainBar = max(1.25, the room's red65 v2 ratio +
  0.20)**, written once by the lead from Logs/n66/red65 at about 2:00, before round 1 is measured.
- **M2:** STYLE above DULL on the verb-aware run, at least the faster refusal's plus 15 points.
- **M3:** the room's verb pilot GREEN on its plan's line (a5 a body "in lava yes"; a3 a kill from the wall).
- **M4:** every piece added or moved since t0 passes the decoration checker v2 (its four tests read skip v2's touches and launch36 kills).
- **GUARDS,** no worse than both reds (Logs/n66/red: the shipped slots; Logs/n66/red65: NIGHT65's final rooms): the walk bot clears 10 / 10
  on both seed sets with deaths in band; stall on flow V (n46flow --verbs65) in band; the room check's items; air65; levelgen 0 ERROR.
  **The walk bot's DULL is a guard at most.**
- **Before T1b lands**, a round's skip lines are labelled "v1, not MAIN". **Two rounds in a row with no M1 move:** the builder redesigns the
  line or the lead drops the slot (logged). **Early GATE:** a room may go to its GATE after round 3 when M1-M4 held on rounds 2 and 3.

## 3. Order of work and owners
- **T (instruments, from 0:05; red team 8 split T1):**
  - **T1a by 0:40:** tour65's GREEN rule (S3: unframed 0, warns 0, wedges in Solids) and the safe `--run`, so b0 can build at about 0:50.
  - **T1b by 1:30 (the MAIN gate):** skip v2 with its planted rooms; then the lead runs the v2 reds, `bash Tools/night/n66-round.sh red
    --shipped` and `bash Tools/night/n66-round.sh red65` (serially, background, 1:30-2:00, before the first pick), and writes the mainBars.
  - **T1c by 2:15:** `n65verb ride --shoot`; `n66-deco.cjs` v2; `gen-n65-foundry.cjs --out=<dir>` with the round's instruments reading
    Logs/n66/<tag>/.
  - **T2 by 2:45:** b2 as a slot (a tool; `--check` equal; verbs37's wall pick kept).
  - Then T maintains the instruments: a reviewer's finding against a tool goes to T by SendMessage. T builds no room (L-106: a builder
    never judges its own checker). If GG's message names b2, the lead adds the slice (`n66-slate-add.cjs`). It is judged as a traversal:
    riders %, the crossing time with the ride against the walk, `n39views wall n65-foundry` rows 9 / 11, the fall return, the lesson and
    bot43 false kept. R or S builds it on T's tools frozen at the T1b PASS sha.
- **R (a2, a3, a5) and S (b1, a8, a4):** 0:05-0:45 a verb plan per room (`Research/2026-10-23-wave66/plans/<id>.md`, <= 40 lines: the lever
  from n65-data.md section 5 and research.md section 1, the pieces with jobs, M1-M3's aim); from 1:00 their own sim loop (n65's skip until
  T1b lands, v2 after); round reports at about 1:55, 3:00, 4:05, 5:10.
- **The lead:** bearings, the go / no-go, A1, b0, the picks, `n66-round.sh` per round, the iso sheets, the reviewers, the GATEs, the
  slots, LV, the play builds, the report.
- **Leftovers, ranked (never past 6:45):** (1) the links between rooms as slots (gg-proxy's "one straight green corridor"; T, as a tool
  export like T2); (2) T-155 one instruction on screen at a time; (3) the frame-pacing bench switch (branch A).

## 4. One round, the GATE, LV
1. **The builder** edits only its room files (and its plan when a review asks); each change adds or moves a piece with a job (the
   decoration checker in its loop). The sim loop in its own tree (builder-brief.md step 6); commit the room file and the outputs (never the
   generated level); report; end the turn only after round 4 (rule 5).
2. **The lead** picks, runs `bash Tools/night/n66-round.sh r<n> > Logs/n66/r<n>-run.txt 2>&1` (background; it refuses an existing folder,
   saves the round's level and plan, writes table.md against red and table-red65.md against red65), the sheets, the reviewers (gg-proxy and
   level design, batched in the review slot). **Branch A:** a data-swap copy `bash Tools/night/n65-play-build.sh n66-b0 n66-r<n>
   Logs/n66/r<n>/n65-foundry.json --config Design/unity-config.json n65.foundry65` and one reel set `tour,n65-foundry,tour65
   v,n65-foundry,verbs37` on it (pruned). Keep only the newest round copy. Findings to the builders by SendMessage.
3. **Rounds at about 2:00, 3:05, 4:10, 5:15.**
4. **The GATE per room:** after round 3 (about 4:35) for a room whose M1-M4 held on rounds 2 and 3, else after round 4 (about 5:40):
   `bash Tools/night/n66-gate-pack.sh <id> r<n>` (one at a time, never during a round), a fresh blind skygrind-spotter on spot-A / spot-B,
   one skygrind-critic per room or per set with the ROOM list's doneWhen; a verdict file per room (`Logs/n66/verdict-<id>.md`, first line
   `<id>: PASS` / `FAIL <reason>` / `UNSURE <reason>`). PASS: passSha, passFiles, `node Tools/night/n65-slot-set.cjs <id> new
   --slate=Logs/n66/slate.json`. FAIL with a named cause: one fix round picked by 6:30, a round-2 critic.
5. **LV (branch A and A-lean):** starts at the first room PASS, no later than 6:00, on the first GATEs' PASSes only; no slot changes after
   rc1 starts. rc1 is a real build of HEAD named **n66-freeze** (`bash Tools/night/n41-build.sh n66-freeze`); its reels (`tour,n65-foundry,
   tour65`, verbs37 on six levels, `-skygrindOff65`'s boot) and the smoke (no agent running) are LV's doneWhen lines; LV's own critic
   (verdict-LV.md); PASS: the flip by `n66-freeze-rows.cjs n65.foundry65=<reason>` before 7:00, its commit adding n65-foundry to the gates'
   --flow list and re-making the flow base, then the `--off65` flow against flow-base-a0. A failure rolls back in one commit. In branch C, LV
   is SHORT with its reason. A fix-round PASS after rc1 ships in n66-play-latest only.
6. **The play builds:** the verified n66-freeze (branch A / A-lean) and `Builds/n66-play-latest` (a data swap with every room's newest
   file, labelled unreviewed); in branch C, `Builds/n66-play-passed` (the PASSed rooms) and n66-play-latest, both data swaps on n64-freeze.

## 5. Delegation
Three builders (T, R, S) and one review slot; at most four subagents running (rule 5). Builders run in NIGHT65's reused worktrees, spawned
without isolation and given their absolute paths. Resume a stopped agent by SendMessage, never a fresh copy. A builder never runs the
Editor, a build, a Player, a reel or the suite.

## 6. The clock (t0 = the clock's start)
| from | to | what |
|---|---|---|
| 0:00 | 1:00 | bearings, the go / no-go, the worktrees, the flow base; builders at 0:05; A1 and the dry run; T1a by 0:40; b0 about 0:50 (A / A-lean); verb plans by 0:45; the verb-plan critic 0:45-1:00 |
| 1:00 | 2:00 | the A-lean tour reel about 1:00; T1b by 1:30; the v2 reds 1:30-2:00 and the mainBars; R / S on their own loops |
| 2:00 | 3:05 | round 1 (picked about 2:00); T1c by 2:15; T2 by 2:45 |
| 3:05 | 4:10 | round 2; the report skeleton committed by 3:30 |
| 4:10 | 5:15 | round 3; early GATEs (about 4:35) |
| 5:15 | 6:45 | round 4; the GATEs (about 5:40); slots; LV from the first PASS (no later than 6:00); fix rounds picked by 6:30; the play builds; the report by 6:30 |
| 6:45 | 7:15 | the last verifications; **the close starts between 7:00 and 7:15** |
| 7:15 | 8:00 | FREEZE: the close; the final message at 7:30 or later; CAP 8:00 |

## 7. Usage limit, resume and the minimum close
State on disk: Logs/n66/slate.json, NIGHT66-LOG.md (every agent's id), slots65.json, the builders' branches and notes. After a stop:
bearings 1-3, the slate, resume each agent by SendMessage, log the gap. **The minimum close** (no agent, no Player; when the close can only
start at 7:30 or later): (1) `node Tools/night/n66-close-check.cjs --start`; (2) every slice in progress or todo set SHORT with what was
measured; (3) NIGHT66-REPORT.md with its sections 2 and 8 (section 8 holds the ROW line, section 9 (i)); (4) HANDOFF.md's NIGHT66 section;
(5) one commit with explicit paths; (6) `node Tools/night/n66-close-check.cjs --tag --minimum`; (7) `node Tools/night/n66-clock.cjs stop`;
(8) `node Tools/night/n66-close-check.cjs` -> `CLOSE-CHECK n66: MINIMUM PASS` or `PASS`; then the final message.

## 8. Honesty
Every claim names its command or file; anything not checked says "not checked" (in particular "the verb cannot be skipped: not checked"
unless skip v2 shows it with its refusal exercised); a failed room is reported failed with its numbers; tests are never edited to pass;
reviewers and critics report every problem and the lead filters with a logged reason. The rooms are a tested guess until GG plays them.
Mistakes become LESSONS.md rows (at its 150 cap: merge or retire).

## 9. The close (started between 7:00 and 7:15, done by 8:00) and the report
- **In series:**
  - (a) `node Tools/night/n66-close-check.cjs --start`; no GATE, slot or flip from here (the check's `frozen` line).
  - (b) Branch A / A-lean: when LV's n66-freeze exists and `git diff --stat <its sha> HEAD -- Assets Design PortExport Packages
    ProjectSettings` prints nothing but GG's files, reuse it (logged); else `bash Tools/night/n41-build.sh n66-freeze` and `git diff --stat
    ProjectSettings`.
  - (c) `bash Tools/night/n66-reels.sh n66-freeze vf,n52-foundry,verbs37 vn,n65-foundry,verbs37 vi,n52-iceline,verbs37 vk,n57-kiln,verbs37
    vs,n52-span,verbs37 vp,n52-spire,verbs37` (skip the sides LV ran on the same build, logged).
  - (d) With no agent running, `bash Tools/night/n56-smoke-steal.sh n66-freeze` (or LV's smoke on the same build).
  - If the guard refuses (b)-(d), each refusal goes under `## Blocked` in NIGHT66-LOG.md, quoting the guard's line and naming the freeze
    build, verbs37 and the smoke.
  - (e) `bash Tools/night/n66-gates.sh --flow > Logs/n66/gates-flow-close.txt 2>&1`.
  - (f) The play builds, each one's level file sha printed.
  - (g) Every slice still todo or in progress set SHORT with its reason; `node Tools/night/n66-slate.cjs` exits 0 with "0 in progress" and
    "0 todo".
  - (h) Finish NIGHT66-REPORT.md, HANDOFF.md, LESSONS rows and the harvest (ledger wave-66.md, THREADS <= 60, FEATURES, CORE <= 40).
  - (i) The report's section 8 gets the exact line the close-check prints as `ROW: n65.foundry65 enabled=<bool>; SLOTS: a2=<use> ...`.
  - (j) **One commit** of every close file with explicit paths.
  - (j2) `node Tools/night/n66-close-check.cjs --pre` must print `PRE-CHECK n66: PASS`; if not, fix the named check, commit again, re-run.
  - (j3) `node Tools/night/n66-close-check.cjs --tag` (it tags HEAD as u3-n66). Nothing is committed after it.
  - (k) At 7:30 or later, `node Tools/night/n66-clock.cjs stop`.
  - (l) `node Tools/night/n66-close-check.cjs`, **alone, in Git Bash, in the foreground, not redirected, not chained** -> `CLOSE-CHECK n66:
    PASS`.
  - (m) The final message, exactly `NIGHT66 COMPLETE`, with no background job or agent running.
- If (l) prints FAIL, fix what needs no commit (stop the night's own job by the PID the check prints, wait for it) and re-run (l). A FAIL
  that needs a commit cannot be fixed after the tag. Log it under Blocked, and the final message is exactly `NIGHT66 CLOSED: <the FAIL
  line>` (GOAL.txt's way out).
- **NIGHT66-REPORT.md, in this order:** (1) what is still not good (the last reviews and gg-proxy); (2) **GG's play card** (the play builds,
  what to try in each room, the one question: better, worse or the same as the old level 1, and which room; eye63's X / Y beside it); (3)
  tools proven; (4) what failed, with numbers; (5) not checked; (6) your calls (DECIDE66.md); (7) the next prompt's seed; (8) the close
  lines and the ROW line.

**Closing reminder:** the disk decides the branch before t0; instruments before rooms; MAIN on the verb against a measured bar, with its
refusal exercised; pieces with jobs, not decoration; a room's GATE is sim-side, the row's is the Player's; one commit, the pre-check, the
tag, the clock stop, the close-check, the final message.
