# NIGHT67.md: THE FOUNDRY, FINISHED (8 hours; 7:30 minimum; 8:00 hard stop): level 1's nearest rooms through their GATEs

GG, 2026-10-24, verbatim: "finish prompt" (after NIGHT66's close). His room brief (2026-10-13): "one might encourage the dash jump another
1 encourage wall riding"; "they can't be too exploitable"; "you should be flying through and ... killing the enemies before you enter the
next room in a stylish fashionable way". And: "more elements that are not just decoration" (2026-10-14).

Written by the forge FORGE67 (Research/2026-10-24-forge67/: DECISIONS D-1..D-8 and the red team's R-1..R-17, redteam.md, slate-seed.json,
builder-brief.md, GOAL.txt). Order of authority: the org rules, GG's words, CLAUDE.md, this file, HANDOFF.md, the logs. **Read this whole
file first and again after any compaction or resume.** The night ends when section 9's close is done: `node Tools/night/n67-close-check.cjs`'s
last line is `CLOSE-CHECK n67: PASS` (or `MINIMUM PASS`) and the last message is `NIGHT67 COMPLETE`, or, after a tag, a FAIL that needs a
commit: `NIGHT67 CLOSED: <the FAIL line>`.

## Where we are, honestly (NIGHT66-REPORT.md; FORGE67's red team)
- **The instrument works.** Skip v2 (the verb refused inside the same pilot, against the faster of that refusal and the plain walker;
  mainBar = max(1.25, red65 + 0.20)) PASSed on its third critic round (Logs/n66/verdict-T1b-r3.md). The sim is deterministic: an exact
  re-run reproduces every seed line.
- **a4 is one guard from a GATE:** `M1 PASS 1.57 vs 1.42` in builder S's tree, one walk-bot death on seed 13; S's carry-over pickup kept
  seed 13 alive (the full set not re-run).
- **a2 is not:** on the amended pilot (d25ea7d8; builder T's unjudged after-runs, T's tree Research/2026-10-23-wave66/T/t1b-amend-after-*),
  M1 1.49 against a bar that becomes 1.47 (red65 1.27); stall V 29.1 % against the reds' 12.4 / 24.1: still a GUARD FAIL. a2 needs a room
  change.
- **Short:** a3 1.13 (its spawn move gamed the skip's start: the carry-over sits on the moved spawn), a5 1.25 with deaths 1 / 4 then 0.80
  UNSURE (THE POUR never took a body), a8 UNSURE (0.97 on the amended pilot). b1 stays NIGHT65's room (DECIDE66 D-3); b2 a slot.
- **On main as carry-over, unmeasured** (973b9c16): a3 THE SLAG RUN, a5 THE SPILL (DECIDE66 D-2), a4's pickup, a8's filled tailrace.
- **NIGHT66 lost 4:35 of 8:00 to two usage stops;** a usage window lasted 2:20 under NIGHT66's load. The shipped game is NIGHT64's
  (foundry65 OFF, every slot old). C: 14 GB at FORGE67: branch C unless GG frees it.

## The target, said plainly
**a4 PASSes its GATE on r1 by about 1:45; a2 after a room change, on r2 or r3; then a3, a5, a8 as far as they go; in branch A / A-lean the
level row ON by LV.** Done well = two or more rooms PASS with every guard held, nothing lost, and GG's play build holding them. Not tonight:
other levels, bosses, Mover.cs, b1 / b2 rooms, new enemy types or movement economy without GG's own message.

## 0. Rules at a glance (all of CLAUDE.md's hard rules; the guard hook enforces many)
1. **The clock** (`node Tools/night/n67-clock.cjs`, every turn): BUILD 0:00-7:15; FREEZE 7:15-8:00 = the close; CAP 8:00. The close starts
   between 7:00 and 7:15 with `node Tools/night/n67-close-check.cjs --start`. Never restart t0; never edit Logs/clock.json.
2. **The disk at the bearings** (DECIDE67 D-1): A >= 23 GB (b0, a reel per round, LV); A-lean 18-23 GB (b0, one tour reel, LV); C 7-18 GB
   (sim only); under 7 GB no rooms. Builders reuse NIGHT66's worktrees; they stop at 5 GB free. The night never deletes older builds or
   worktrees (DECIDE67 lists GG's commands).
3. **Docs and code only with Write / Edit** (no heredocs into files, no `node -e` / `python -c` writes, no `sed -i`).
4. **Git and GG's calls:** explicit paths plus `.meta`; never GG's four files or the four TMP fonts; no push, merge, rebase, master,
   `reset --hard`, `stash`, `--amend`, `branch -D`, tag moves. **u3-n67 is made only by `n67-close-check.cjs --tag`** (or `--tag --minimum`).
   GG's answers in DECIDE67 steer priorities; a deletion, a bulk change or a lever needs GG's own message in this session.
5. **Agents: at most three running:** builders R and S and one review slot; builder T only for T0's proofs (0:05-0:40, then it ends) or a
   tool fix a critic names. One Player at a time. Commit free read before every build, reel set, smoke or gate run (under 15 GB: hold).
6. **Background, one job per tree:** every round, gate run, build, reel set, smoke, critic and reviewer in the background; one dotnet job in
   the main tree at a time; **no pick, commit or slot write on port/stage3 while a main-tree round runs.** **Every reviewer, spotter and
   critic prompt says: no dotnet, harness, build, Player or Editor in any tree; read-only node, python -I and git only.** Never pipe a long
   script into `head`. End a waiting turn with a tool call. Log times never run ahead of the clock.
7. **Every output a verdict cites is a file under Logs/n67/** the moment it prints, with its command, sha and time. Never `git add -f` a
   round's level copy.
8. **A PASS binds to its sha; the slots are the lead's** (`n65-slot-set.cjs <room> new --slate=Logs/n67/slate.json`); the row flips only
   by LV through `n67-freeze-rows.cjs`.
9. **Reviewers see evidence, not the story:** pictures, measured files, the plan, the diff; never builder notes.
10. **NIGHT66's corners:** a moved spawn under the instrument's start; rounds that change only waves; a lead write during a round; a second
    dotnet job in the main tree; a critic running dotnet; stale reds beside new bars.
11. **The usage limit** (L-003): a window lasted 2:20 under NIGHT66's load; **expect the first stop 2:30-3:30 after the window's start, or
    earlier if t0 falls mid-window**; log any "resets" time this session shows. Commit every step; the log holds every agent's id. After a
    stop: section 1's steps 1-2, `git log -1 -- Logs/n67/slate.json`, the slate printed, the background jobs listed (a round still running
    is waited for), one SendMessage per agent, the gap logged. The report committed by 3:30 and by 6:30. After a stop past 7:00, the
    minimum close (section 7) when the close starts at 7:30 or later, else the full close.

## 1. Bearings (steps 1-2 every session; 3-7 the first session only)
1. `node Tools/night/n67-clock.cjs start` (first session). Every session: the clock, `git status --short`, `git log --oneline -5`,
   HANDOFF.md's "Verify on arrival" table (each line logged), `claude --version`, the branch said aloud. t0 and `git rev-parse --short
   HEAD` (the t0 sha) at the top of NIGHT67-LOG.md.
2. The Editor: `timeout 60 bash Tools/unity-loop.sh status` ("playMode": "stopped"), then `timeout 45 unity command set_autotick --enable
   true --interval_ms 16 --caller plugin --skill unity-cli`. Commit free and `df -BG /c | tail -1`: **the go / no-go, DECIDE67 D-1.**
3. (first session only, never clobbering) `mkdir -p Logs/n67 Logs/n67/r0n66`; `[ -e Logs/n67/slate.json ] || cp
   Research/2026-10-24-forge67/slate-seed.json Logs/n67/slate.json`; `[ -d Logs/n67/red ] || cp -r Logs/n66/red Logs/n67/`; `[ -d
   Logs/n67/red65 ] || cp -r Logs/n66/red65 Logs/n67/`; `cp Logs/n66/r2/review-level.md Logs/n67/r0n66/review-level.md` (NIGHT66's open
   BLOCKING lines, read by the GATE pack); then, only if Logs/n67/flow-base.txt is missing, `bash Tools/night/n67-gates.sh --flow-base >
   Logs/n67/flow-base-run.txt 2>&1` (background; it builds the harness).
4. **The worktrees (no new disk):** R -> `.claude/worktrees/agent-a9d0ffb6d0dfdb2be` (n66-R a64c5c3d); S -> `agent-aeb9fc116ba05ef4d`
   (n66-S 29322705; NIGHT65 R's untracked probe Harness.R65.DashJump.cs and tmpR/ stay, never committed); T -> `agent-af48e888c59fa3626`
   (n66-T). For each: `git -C <wt> status --short` (no tracked change expected; the untracked NIGHT66 evidence is listed in HANDOFF), then
   `git -C <wt> switch -c n67-<L> <t0 sha>`. Each builder's first commit adds its tree's NIGHT66 evidence (the brief).
5. **At 0:05 spawn R, S and T** in the background without worktree isolation, each told its absolute path, the brief
   (Research/2026-10-24-forge67/builder-brief.md), t0, the t0 sha and its deadlines (T: T0's proofs only). Log each id.
6. **T0 (0:05-1:20; slate T0), one dotnet job at a time in the main tree:**
   - T, in its own tree: the planted two-bodies-off-the-ray case and the planted hang (`N65_VERBS_TIMEOUT=60 bash Tools/night/n65-verbs.sh
     skip <level> a8 --seeds 200`); commits its NIGHT66 after-runs; ends its turn. The lead picks T's evidence into main.
   - The lead, after the flow base: T's eight after-runs re-run on main's DLL into Logs/n67/t0-charge/ (each with its command, sha and
     time) and diffed against T's (identical = d25ea7d8 is the code T measured).
   - **The reds on the T0 tool, before r1:** rename the copies (`Logs/n67/red65/verbs-skip.txt` -> `verbs-skip-n66-109d9dcd.txt`;
     red65's a2 / a8 flow-V.txt the same way), then `{ echo "# command: bash Tools/night/n65-verbs.sh skip Logs/n67/red65/n65-foundry.json
     a2,a3,a4,a5,b1,a8 --seeds 10 --bars Logs/n67/slate.json"; echo "# sha: $(git rev-parse --short HEAD); $(date '+%F %T')"; bash
     Tools/night/n65-verbs.sh skip Logs/n67/red65/n65-foundry.json a2,a3,a4,a5,b1,a8 --seeds 10 --bars Logs/n67/slate.json; } >
     Logs/n67/red65/verbs-skip.txt 2>&1` (~10 min), and red65's flow V for a2 and a8 by the DLL (`dotnet
     Temp/SimHarness/bin/Release/net10.0/SimHarness.dll n46flow --level n65-foundry --file Logs/n67/red65/n65-foundry.json --rooms <id>
     --seeds 10 --first 1 --verbs65`, the same header). **Every mainBar and every room's ceilings (deathsAMax, deathsBMax, stallVMax: the
     higher of the two reds) written to the slate** (`n67-slate-set.cjs`), committed.
   - **r1 at about 0:50:** `ROOMS="a2 a3 a4 a5 a8" bash Tools/night/n67-round.sh r1 > Logs/n67/r1-run.txt 2>&1` (the carry-over, the
     clean-round proof; **r1's a3 lines do not count**: the spawn is still the moved one).
   - **T0's critic first** in the review slot (about 1:10), then the r1 level review (`Logs/n67/r1/review-level.md`). Until T0 PASSes,
     the rooms' skip lines are "not MAIN".
   - **If T0 FAILs:** a fix that changes only Harness.N65.Verb.cs / VerbPilot.cs re-runs the skip and flow V on r1's saved level
     (`LEVEL_FILE=Logs/n67/r1/n65-foundry.json ROOMS="a2 a4 a5 a8" bash Tools/night/n67-round-verbs.sh r1b`, flow V by the DLL into
     Logs/n67/r1b/<id>/); r1's other guard files stand (flow A / B, the room check, levelgen and air65 never run the verb pilot).
7. Each round's level review is named `Logs/n67/r<n>/review-level.md` (`## <id>` headings, BLOCKING lines); its BLOCKING lines go to R / S.

## 2. The slate and the evidence
Logs/n67/slate.json (seeded from Research/2026-10-24-forge67/slate-seed.json). Only the lead edits it and commits it with `git add -f`. A
room's GATE (the ROOM list): **M1-M4 and every GUARD held on one main-tree round on the T0-passed tool, skip v2 also on seeds 11-20 (the
GATE pack's fresh skip), the room file unchanged since that round, that round read by a level review (the GATE critic writes it when none
did); a fresh blind spotter on the pack's pair; a skygrind-critic PASS.** M1 against the room's mainBar (a ride room within 0.10 of its bar
reads UNSURE); M2 +15; M3 the room's verb pilot (a3: verbs-ride.txt); M4 the decoration checker v2 on every piece changed since e37083cf.
GUARDS against the room's ceilings in the slate.

## 3. Order of work and owners
- **The lead:** bearings, T0, the picks (each room file with its verb plan), `n67-round.sh` per round, the reviews, the GATEs, the slots,
  LV, the play builds, the report.
- **R:** a2 (a room change for its stall), a3 (the spawn back first), a5 (THE SPILL). **S:** a4 first (frozen as a GATE candidate once r1
  holds it), then a8. Rounds reported about 1:30, 2:30, 3:30; builders stop at 6:45.
- **T:** T0's proofs (0:05-0:40), then only on a critic's tool finding.

## 4. One round, the GATE, LV
1. **The builder** edits only its room files and plans, adding or moving a collider piece with a job each round (a GATE candidate is
   frozen); its own sim loop (the brief); commits; reports by ending its turn; the lead resumes it at once.
2. **The lead** picks (`git checkout <sha> -- Design/levels/n65/rooms/<id>.json Research/2026-10-23-wave66/plans/<id>.md`, committed),
   then runs the round: `ROOMS="a2 a3 a4 a5 a8" bash Tools/night/n67-round.sh r<n> > Logs/n67/r<n>-run.txt 2>&1` (background; nothing on
   port/stage3 until it ends), then `node Tools/night/n67-deco.cjs --base e37083cf <id> --touches Logs/n67/r<n>/verbs-skip.txt >
   Logs/n67/r<n>/deco-<id>.txt` per room, the level review, findings to the builders.
3. **Rounds at about 0:50 (r1, the carry-over), 1:50, 2:50, 3:50.**
4. **The GATE per room** once a round holds it (a4 after r1, about 1:30): `bash Tools/night/n67-gate-pack.sh <id> r<n>` (one at a time,
   never during a round; its fresh skip is seeds 11-20 under a 900 s timeout), a fresh blind spotter on spot-A / spot-B, one critic with
   the ROOM list and, beside the pack, the round's table-red65.md, deco-<id>.txt, levelgen-report.md and `git diff e37083cf <sha> --
   Design/levels/n65/rooms/<id>.json`; verdict file `Logs/n67/verdict-<id>.md` (first line `<id>: PASS` / `FAIL <reason>` / `UNSURE
   <reason>`). PASS: passSha, passFiles, `node Tools/night/n65-slot-set.cjs <id> new --slate=Logs/n67/slate.json`. FAIL with a named cause:
   one fix round; a round-2 critic. **Every GATE as early as its round allows** (the likely stop: rule 11).
5. **LV (branch A and A-lean):** from the first PASS, no later than 5:30 (slate LV). In branch C, LV is SHORT with its reason.
6. **The play builds:** branch A / A-lean the verified n67-freeze; branch C with a PASS: `node Tools/night/gen-n65-foundry.cjs --run
   --out=Logs/n67/play-passed` (slots65.json's set: the PASSed rooms new, every other old), then `bash Tools/night/n65-play-build.sh
   n66-freeze n67-play-passed Logs/n67/play-passed/n65-foundry.json --config Design/unity-config.json --add-level n65.foundry65` (its boot
   named run or not run).

## 5. The clock (t0 = the clock's start)
| from | to | what |
|---|---|---|
| 0:00 | 0:50 | bearings, the go / no-go, the worktrees, the flow base; R, S, T at 0:05; T0's proofs; the charge rule's re-run; the reds and the bars |
| 0:50 | 1:50 | r1 (the carry-over, clean); T0's critic about 1:10; the r1 review; **a4's GATE about 1:30**; builders' round 1 |
| 1:50 | 3:50 | r2 (1:50), r3 (2:50); a2's GATE when a round holds it; the report skeleton by 3:30 |
| 3:50 | 6:45 | r4 (3:50); the other GATEs; LV (A / A-lean); fix rounds by 6:00; the play builds; gg-proxy on the final rooms; the report by 6:30; builders stop at 6:45 |
| 7:00 | 8:00 | the close (section 9); the final message at 7:30 or later; CAP 8:00 |

## 6. Delegation
Two builders and one review slot; T only for T0's proofs and tool fixes. Resume a stopped agent by SendMessage (ids in the log). A
builder never runs the Editor, a build, a Player, a reel or the suite.

## 7. Usage limit, resume and the minimum close
After a stop: rule 11's resume. **The minimum close** (when the close can only start at 7:30 or later): (1) `--start`; (2) every slice in
progress or todo set SHORT with what was measured; (3) NIGHT67-REPORT.md with sections 2 and 8 (the ROW line); (4) HANDOFF.md and
docs/memory/CORE.md's state line; (5) one commit of NIGHT67-REPORT.md, NIGHT67-LOG.md, HANDOFF.md and docs/memory/CORE.md (the
close-check's files line needs each one committed after --start); (6) `node Tools/night/n67-close-check.cjs --tag --minimum`; (7) the
clock stop; (8) the close-check -> `MINIMUM PASS` or `PASS`; the final message.

## 8. Honesty
Every claim names its command or file; anything not checked says "not checked"; a failed room is reported failed with its numbers; tests
are never edited to pass; reviewers report every problem and the lead filters with a logged reason. Mistakes become LESSONS rows.

## 9. The close (started between 7:00 and 7:15, done by 8:00)
- (a) `node Tools/night/n67-close-check.cjs --start`; no GATE, slot or flip from here. Every builder stopped (6:45).
- (b) With 12 GB free or more (any branch): `bash Tools/night/n41-build.sh n67-freeze` (or LV's rc1 when nothing but docs changed after
  it), then `git diff --stat ProjectSettings`.
- (c) `bash Tools/night/n67-reels.sh n67-freeze vf,n52-foundry,verbs37 vi,n52-iceline,verbs37 vk,n57-kiln,verbs37 vs,n52-span,verbs37
  vp,n52-spire,verbs37`, **plus `vn,n65-foundry,verbs37` only when the level row is ON** (with it OFF, n65-foundry has no campaign place
  and its grapple rows read RED: NIGHT66's close; the close-check reads vn only with the row ON).
- (d) With no agent running, `bash Tools/night/n56-smoke-steal.sh n67-freeze`. Under 12 GB the guard refuses (b)-(d): each refusal goes
  under `## Blocked` quoting the guard's line and naming the freeze build, verbs37 and the smoke.
- (e) `bash Tools/night/n67-gates.sh --flow > Logs/n67/gates-flow-close.txt 2>&1`. (f) The play builds, each one's level sha printed.
- (g) Every slice still todo or in progress set SHORT with its reason; `node Tools/night/n67-slate.cjs`: exit 0, 0 in progress, 0 todo.
- (h) NIGHT67-REPORT.md, HANDOFF.md, the harvest (ledger wave-67.md, LESSONS, THREADS, FEATURES, CORE). (i) The report's section 8 holds
  the exact ROW line the close-check prints.
- (j) Restore Play mode's rewrite of `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset` if it shows
  (read its diff first: L-047); one commit of every close file. (j2) `node Tools/night/n67-close-check.cjs --pre` -> `PRE-CHECK n67:
  PASS` (else fix and commit again). **When --pre fails only on player / gates for a reason the guard did not print** (a verbs37 RED that
  is a result, a commit-free hold that does not clear by 7:45): log it, wait to 8:00, then `--tag --minimum`, the clock stop, the check
  (MINIMUM PASS). (j3) Otherwise `node Tools/night/n67-close-check.cjs --tag`; nothing is committed after it. (k) At 7:30 or later `node
  Tools/night/n67-clock.cjs stop`.
- (l) With no background task or agent pending: `node Tools/night/n67-close-check.cjs`, alone, in the foreground.
- (m) The final message, exactly `NIGHT67 COMPLETE`. If the final check FAILs, fix only what needs no commit (a stray process by its PID)
  and run it again; a FAIL that needs a commit after the tag is final: the last message is exactly `NIGHT67 CLOSED: <the FAIL line>`.
- **The report, in order:** (1) what is still not good; (2) GG's play card; (3) tools proven; (4) what failed, with numbers; (5) not
  checked; (6) your calls; (7) the next prompt's seed; (8) the close lines and the ROW line.
