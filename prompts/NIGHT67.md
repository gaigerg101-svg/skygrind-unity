# NIGHT67.md: THE AREAS (8 hours in two work blocks; 7:30 minimum; 8:00 hard stop): better area design on levels 2, 3 and 5

GG, 2026-10-24, verbatim: "need to build a another detailed 8 hour build prompt following same methods, area deisgn need t be better in
alot of levels thats the mian focus". His area words (docs/memory/OWNER.md): "every single level needs a complete arena overhaul completely
from scratch" (2026-10-09); "I'd like a lot of the rooms to be a lot smaller and more action packed like think about the design of Ultra
Kilikow" (2026-10-09); "random lava pits random pillars that serve no purpose" and "you're really struggling to make detailed rooms that
really incentivize mechanics" (2026-10-12); level 2 "Nearly all rooms ... randomly placed" (2026-10-13); level 5 "the level doesn't make
sense" (2026-10-18); "There always really needs to be a route that enemies can climb up"; "more elements that are not just decoration".
(His level-3 words, "just isn't good" and "The first room is confusing", were said of the kiln before NIGHT57 rebuilt it: n57-kiln has
never been played.)

Written by the forge FORGE67b (Research/2026-10-24-forge67b/: history.md, levels-data.md, research.md, code-scout.md, DECISIONS D-1..D-9
and the red team's R-1..R-14, redteam.md, slate-seed.json, builder-brief.md, GOAL.txt). It replaces NIGHT67's first plan (level 1's rooms:
Research/2026-10-24-forge67/NIGHT67-v1.md). Order of authority: the org rules, GG's words, CLAUDE.md, this file, HANDOFF.md, the logs.
**Read this whole file first and again after any compaction or resume.** The night ends when section 9's close is done:
`node Tools/night/n67-close-check.cjs`'s last line is `CLOSE-CHECK n67: PASS` (or `MINIMUM PASS`) and the last message is `NIGHT67 COMPLETE`,
or, after a tag, a FAIL that needs a commit: `NIGHT67 CLOSED: <the FAIL line>`.

## Where we are, honestly (the scouts and the red team)
- **Five levels ship** (n52-foundry, n52-iceline, n57-kiln, n52-span, n52-spire; 25 fight rooms). Only level 1 has had rooms work since
  w60, and none of it shipped; **no person has judged any room built after EXT53** (GG last played 2026-10-20).
- **The rooms are one template:** 19 of 25 meet the same computed contract and the walk bot's DULL still runs 0-76 % inside it; 20 of 25
  rooms are 500 m2 or more; the 23 fight rooms hold 76 tall solid blocks against 36 ride walls (GG's "random pillars", counted).
- **Level 2 is the weakest level**, and its four weakest rooms (g1-g4) cannot be measured by any verb pilot tonight: with the gauntlet
  alone the punch drops and the ride pilot counts only gun kills (redteam.md finding 2). Tonight takes level 2's g7 and g8 (after the
  KESTREL). Level 3's measured fault is kb3c (its ride skippable, 0.81); gg-proxy's worst frames are kb3. Level 5's flagged rooms: v2, v6.
- **The instruments:** the verb pilot performs ride, slide, launch, pierce and parry; no shipped room on levels 2-5 carries a verb65 row
  (a plant gives one). The pipeline is level 1's; levels 2 and 5 have no slot generator. **A slot keeps its room's rect and doors: no
  room gets smaller tonight** (a room can lose empty floor, not size).
- **The usage limit:** NIGHT66's windows lasted about 2:20 of work under this load: plan two blocks (rule 11).
- **The disk:** C: 13-14 GB at the forge: branch C (sim only) unless GG frees it.

## The target, said plainly
**Six rooms on three levels rebuilt as areas: level 2's g7 and g8 and level 5's v2 (builder R), level 3's kb3 and kb3c and level 5's v6
(builder S). Each passes the area lint and makes its verb the better line, by skip v2, on a GATE.** Done well = three or more rooms PASS
with every guard held, on at least two levels, nothing lost, and GG's play build holding them. Not tonight: level 1 (its rebuild waits
for GG's play), level 4, level 2's g1-g4 (a later gauntlet pilot), links, bosses, new enemy types or movement economy without GG's word.

## 0. Rules at a glance (all of CLAUDE.md's hard rules; the guard hook enforces many)
1. **The clock** (`node Tools/night/n67-clock.cjs`, every turn): BUILD 0:00-7:15; FREEZE 7:15-8:00 = the close; CAP 8:00. The close starts
   between 7:00 and 7:15 with `node Tools/night/n67-close-check.cjs --start`. Never restart t0; never edit Logs/clock.json.
2. **The disk at the bearings** (DECIDE67 D-1): A >= 23 GB (b0, a reel per round, LV); A-lean 18-23 GB (b0, LV); C 7-18 GB (sim only);
   under 7 GB no rooms. Builders reuse NIGHT66's worktrees and stop at 5 GB free. The night never deletes older builds or worktrees.
3. **Docs and code only with Write / Edit** (no heredocs into files, no `node -e` / `python -c` writes, no `sed -i`).
4. **Git and GG's calls:** explicit paths plus `.meta`; never GG's four files or the four TMP fonts; no push, merge, rebase, master,
   `reset --hard`, `stash`, `--amend`, `branch -D`, tag moves. **u3-n67 is made only by `n67-close-check.cjs --tag`** (or `--tag --minimum`).
   GG's answers in DECIDE67 steer priorities; a deletion, a bulk change or a lever needs GG's own message in this session.
5. **Agents: at most four running while T works (R, S, T, one review slot), three when T is between slices.** Critics and spotters are
   batched (one critic per batch of rooms; spotters short). One Player at a time. Commit free read before every build, reel set, smoke or
   gate run (under 15 GB: hold; a hold that does not clear in 20 minutes is logged and only that heavy job waits).
6. **Background, one job per tree:** every round, gate run, build, reel set, smoke, critic and reviewer in the background; **one dotnet job
   in the main tree at a time** (the flow base, the reds and the rounds in series); **no pick, commit, slot write or slate write on
   port/stage3 while a main-tree round runs** (each round reads a slate snapshot). **Every reviewer, spotter and critic prompt says: no
   dotnet, harness, build, Player or Editor in any tree; read-only node, python -I and git only.** Never pipe a long script into `head`. End
   a waiting turn with a tool call. Log times never run ahead of the clock.
7. **Every output a verdict cites is a file under Logs/n67/** the moment it prints, with its command, sha and time.
8. **A PASS binds to its sha; the slots are the lead's:** `node Tools/night/n65-slot-set.cjs <room> new
   --slots=Design/levels/n67/<level>/slots67.json --slate=Logs/n67/slate.json` (T1b makes it refuse an unknown option or a room not in that
   file); the rows flip only by LV through `n67-freeze-rows.cjs`.
9. **Reviewers see evidence, not the story:** pictures, measured files, the card, the diff; never builder notes.
10. **The corners never repeated** (history.md section 4): an instrument that cannot see the design; decoration counted as building;
    pillars or lava with no job; a moved spawn; a lead write during a round; two dotnet jobs in the main tree; a critic running dotnet;
    stale reds beside new bars; a card gate cycled; a verb the room's loadout cannot fire.
11. **The usage limit: two work blocks.** NIGHT66's windows lasted about 2:20 of work under this load. **Block 1 (0:00 to the stop, about
    2:20):** the bearings, the reds, the cards, T1a / T1b, the builders' round 1, and **r1 started as one background chain by about 2:05**
    so it computes through the stop. **Block 2 (after the reset, about 5:00 to 8:00):** r1's reviews, the GATEs, r2, the GATEs, the close.
    Plan for at most two judged rounds. Log any "resets" time the session shows. After a stop: section 1's steps 1-2, `git log -1 --
    Logs/n67/slate.json`, the slate printed, the background jobs listed (a round still running is waited for), one SendMessage per agent,
    the gap logged. The report committed before the stop (about 2:10) and by 6:30. After a stop past 7:00, the minimum close (section 7)
    when the close starts at 7:30 or later, else the full close.

## 1. Bearings (steps 1-2 every session; 3-7 the first session only)
1. `node Tools/night/n67-clock.cjs start` (first session). Every session: the clock, `git status --short`, `git log --oneline -5`,
   HANDOFF.md's "Verify on arrival" table (each line logged), `claude --version`, the branch said aloud. t0 and `git rev-parse --short
   HEAD` (the t0 sha) at the top of NIGHT67-LOG.md.
2. The Editor: `timeout 60 bash Tools/unity-loop.sh status` ("playMode": "stopped"), then `timeout 45 unity command set_autotick --enable
   true --interval_ms 16 --caller plugin --skill unity-cli`. Commit free and `df -BG /c | tail -1`: **the go / no-go, DECIDE67 D-1.**
3. (first session only, never clobbering) `mkdir -p Logs/n67`; `[ -e Logs/n67/slate.json ] || cp Research/2026-10-24-forge67b/slate-seed.json
   Logs/n67/slate.json`; only if Logs/n67/flow-base.txt is missing, `bash Tools/night/n67-gates.sh --flow-base > Logs/n67/flow-base-run.txt
   2>&1` (background; it builds the harness).
4. **The worktrees (no new disk):** R -> `.claude/worktrees/agent-a9d0ffb6d0dfdb2be` (n66-R); S -> `agent-aeb9fc116ba05ef4d` (n66-S;
   NIGHT65 R's untracked probe Harness.R65.DashJump.cs and tmpR/ stay, never committed); T -> `agent-af48e888c59fa3626` (n66-T). For each:
   `git -C <wt> status --short` (no tracked change expected), then `git -C <wt> switch -c n67-<L> <t0 sha>`.
5. **At 0:05 spawn R, S and T** in the background without worktree isolation, each told its absolute path, the brief
   (Research/2026-10-24-forge67b/builder-brief.md), t0, the t0 sha and its deadlines. Log each id.
6. **The reds on the shipped levels (after the flow base, in series):** `ROOMS="g7 g8" bash Tools/night/n67-red.sh 3 n52-iceline
   Logs/n67/red-iceline`, `ROOMS="kb3 kb3c" bash Tools/night/n67-red.sh 3 n57-kiln Logs/n67/red-kiln`, `ROOMS="v2 v6" bash
   Tools/night/n67-red.sh 3 n52-spire Logs/n67/red-spire` (the room check, the walk bot's flows, pads, the ride pilot); after the cards
   (about 0:55) each room's skip on its verb with `--plant-verb65 <id>:<verb>` into the same folders (`bash Tools/night/n65-verbs.sh skip
   Design/levels/<shipped>.json <id> --plant-verb65 <id>:<verb> --seeds 10 > Logs/n67/red-<L>/<id>/skip.txt`, its header with the command
   and sha). Then each room's `verb`, `mainBar` (max(1.25, red + 0.20)) and ceilings (`deathsAMax`, `deathsBMax`, `stallAMax`: the red's
   own) written to the slate and committed. A verb changed later re-runs that room's red.
7. **T1a's landing (about 0:50):** pick it; run `node Tools/night/n67-a1.cjs` (the three rows OFF, the placeholder levels, every
   slots67.json all old, the map list, look49, Polish53IcelineLevels); the Editor compile; `bash Tools/night/n67-gates.sh` (fingerprints
   plain and --off67 EQUAL); commit; tell R and S to sync (the brief). **The card critic (about 1:00):** one fresh skygrind-critic reads the
   six cards (archetype, verb against the probe, lever, pieces with jobs, lint rules); GO / CHANGE <one thing> per room. **T1b's landing
   (about 1:45):** pick; one critic for T1a + T1b together, which also reads d25ea7d8 and the charge-rule re-run (no pierce room before it
   PASSes). If T1a's `--check` fails on a level, that level's rooms go SHORT and the night continues on the others.

## 2. The slate and the evidence
Logs/n67/slate.json (seeded from Research/2026-10-24-forge67b/slate-seed.json). Only the lead edits it and commits it with `git add -f`,
never while a round runs. A room's GATE (the ROOM list): **M1 AREA (the lint: every rule that applies PASSes), M2 VERB (skip v2 on the
room's verb >= its mainBar on seeds 1-10 and 11-20, STYLE +15), M3 its verb pilot, M4 the decoration checker on every piece and at least
one added piece with a job, every GUARD at or under the room's ceilings; on one main-tree round on the passed tools; the room file unchanged
since; that round read by a level review; a blind spotter; a skygrind-critic PASS.** Before T1b passes, no room's M1 or M2 counts.

## 3. Order of work and owners
- **The lead:** bearings, the reds, T1a's landing, the card critic, T1b's landing, the picks, the rounds, the reviews, the GATEs, the
  slots, LV, the play builds, the report.
- **R:** g7, g8 (level 2), v2 (level 5). **S:** kb3, kb3c (level 3), v6 (level 5). The probe and the cards by 0:50; the room files from
  T1a's sync; round 1 reported by about 2:00; round 2 after the reset (by about 6:00); a GATE candidate is frozen; builders stop at 6:45.
- **T:** T1a by 0:50, T1b by 1:45, T1c by about 5:15 (slate T1a / T1b / T1c), then on call.
- **Leftovers (never past 6:00), ranked:** (1) level 1's a4 (NIGHT67 v1's carry-over: Research/2026-10-24-forge67/NIGHT67-v1.md); (2)
  level 4's p3 / p5 (needs span in levels67.json). Links stay as they are (the slot model keeps them bit for bit).

## 4. One round, the GATE, LV
1. **The builder** edits only its room files and cards, changing the space each round (a piece added, removed or moved by double or half,
   fixing a named lint rule); its own sim loop (the brief); commits; reports by ending its turn; the lead resumes it at once.
2. **The lead** picks (`git checkout <sha> -- Design/levels/n67/<level>/rooms/<id>.json Research/2026-10-24-wave67/cards/<id>.md`,
   committed), then runs the round as **one background chain over the three levels**: `for L in iceline kiln spire; do L67=$L
   ROOMS="<its rooms>" bash Tools/night/n67-round.sh r<n>-$L > Logs/n67/r<n>-$L-run.txt 2>&1; done` (nothing on port/stage3 until it
   ends); then per room the lint into `Logs/n67/r<n>-<L>/area-<id>.txt` and the decoration checker into `deco-<id>.txt`; one level review
   per round only for rounds with a GATE candidate (`Logs/n67/r<n>/review-level.md`, `## <id>` headings, BLOCKING lines); findings to R
   and S.
3. **r1 chained by about 2:05 (it computes through the stop); r2 after the reset (about 5:30).**
4. **The GATE** once a round holds a room: `L67=<L> bash Tools/night/n67-gate-pack.sh <id> r<n>-<L>` (one at a time, never during a
   round; its fresh skip is seeds 11-20 under a 900 s timeout); fresh blind spotters; **one critic per batch of held rooms**, with the ROOM
   list and, beside each pack, the round's area-<id>.txt, deco-<id>.txt, table, levelgen report and `git diff <the room file's first
   commit> <sha> -- Design/levels/n67/<level>/rooms/<id>.json`; a verdict file per room (`Logs/n67/verdict-<id>.md`, first line `<id>:
   PASS` / `FAIL <reason>` / `UNSURE <reason>`). PASS: passSha, passFiles, the slot set new (rule 8). FAIL with a named cause: one fix round
   in r2, a round-2 critic.
5. **LV (branch A and A-lean):** from the first PASS, no later than 6:00, by its slate entry (a build with the passed rows ON in a config
   copy, judged before the flip). In branch C, LV is SHORT with its reason.
6. **The play builds:** branch A / A-lean the verified n67-freeze; branch C with a PASS: each rebuilt level assembled from its
   slots67.json (`node Tools/night/gen-slots.cjs --level <L> --run --out=Logs/n67/play-passed/<L>`), then one play build (T1c's
   multi-level n65-play-build.sh) on Builds/n67-freeze when the close built it (section 9 (b)), else on Builds/n66-freeze with "level 2's
   colour grade missing: its level list is code" on the card.

## 5. The clock (t0 = the clock's start; block 2's times assume the reset near 5:00)
| from | to | what |
|---|---|---|
| 0:00 | 0:50 | bearings, the go / no-go, the worktrees, the flow base, the reds; R, S, T at 0:05; the probes and the six cards; T1a about 0:50 |
| 0:50 | 2:20 | T1a's landing and the sync; the card critic; the reds' skips and the bars; T1b about 1:45 and its critic; round 1; **r1 chained by 2:05**; the report skeleton committed |
| 2:20 | 5:00 | the likely stop (background r1 computes) |
| 5:00 | 6:45 | r1's review and GATEs; round 2 and r2 (about 5:30); its GATEs; LV (A / A-lean); the play build; gg-proxy on the final rooms; the report by 6:30; builders stop at 6:45 |
| 7:00 | 8:00 | the close (section 9); the final message at 7:30 or later; CAP 8:00 |

## 6. Delegation
Two room builders, a tools builder for its three slices, one review slot (batched critics). Resume a stopped agent by SendMessage (ids in
the log). A builder never runs the Editor, a build, a Player, a reel or the suite.

## 7. Usage limit, resume and the minimum close
After a stop: rule 11's resume. **The minimum close** (when the close can only start at 7:30 or later): (1) `--start`; (2) every slice in
progress or todo set SHORT with what was measured; (3) NIGHT67-REPORT.md with sections 2 and 8 (every ROW line); (4) HANDOFF.md and
docs/memory/CORE.md's state line; (5) one commit of NIGHT67-REPORT.md, NIGHT67-LOG.md, HANDOFF.md and docs/memory/CORE.md; (6) `node
Tools/night/n67-close-check.cjs --tag --minimum`; (7) the clock stop; (8) the close-check -> `MINIMUM PASS` or `PASS`; the final message.

## 8. Honesty
Every claim names its command or file; anything not checked says "not checked" (every rebuilt level: "not checked in a Player" in branch
C); a failed room is reported failed with its numbers; tests are never edited to pass; reviewers report every problem and the lead filters
with a logged reason. The rooms are a tested guess until GG plays them. Mistakes become LESSONS rows.

## 9. The close (started between 7:00 and 7:15, done by 8:00)
- (a) `node Tools/night/n67-close-check.cjs --start`; no GATE, slot or flip from here. Every builder stopped (6:45).
- (b) With 12 GB free or more (any branch): `bash Tools/night/n41-build.sh n67-freeze` (or LV's rc1 when nothing but docs changed after
  it), then `git diff --stat ProjectSettings`.
- (c) `bash Tools/night/n67-reels.sh n67-freeze vf,n52-foundry,verbs37 vi,n52-iceline,verbs37 vk,n57-kiln,verbs37 vs,n52-span,verbs37
  vp,n52-spire,verbs37` (a level flipped by LV plays through its row at its campaign place: the reel line names the level it loaded);
  **plus `vn,n65-foundry,verbs37` only when level 1's row is ON.**
- (d) With no agent running, `bash Tools/night/n56-smoke-steal.sh n67-freeze`. Under 12 GB the guard refuses (b)-(d): each refusal goes
  under `## Blocked` quoting the guard's line and naming the freeze build, verbs37 and the smoke.
- (e) `bash Tools/night/n67-gates.sh --flow > Logs/n67/gates-flow-close.txt 2>&1`. (f) The play builds, each one's level sha printed.
- (g) Every slice still todo or in progress set SHORT with its reason; `node Tools/night/n67-slate.cjs`: exit 0, 0 in progress, 0 todo.
- (h) NIGHT67-REPORT.md, HANDOFF.md, the harvest (ledger wave-67.md, LESSONS, THREADS, FEATURES, CORE). (i) The report's section 8 holds
  every ROW line the close-check prints (level 1's and each design.n67 row in the config).
- (j) Restore Play mode's rewrite of `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset` if it shows
  (read its diff first: L-047); one commit of every close file. (j2) `node Tools/night/n67-close-check.cjs --pre` -> `PRE-CHECK n67: PASS`
  (else fix and commit again). **When --pre fails only on player / gates for a reason the guard did not print** (a verbs37 RED that is a
  result, a hold that does not clear by 7:45): log it, wait to 8:00, then `--tag --minimum`, the clock stop, the check (MINIMUM PASS).
  (j3) Otherwise `node Tools/night/n67-close-check.cjs --tag`; nothing is committed after it. (k) At 7:30 or later `node
  Tools/night/n67-clock.cjs stop`.
- (l) With no background task or agent pending: `node Tools/night/n67-close-check.cjs`, alone, in the foreground.
- (m) The final message, exactly `NIGHT67 COMPLETE`. If the final check FAILs, fix only what needs no commit (a stray process by its PID)
  and run it again; a FAIL that needs a commit after the tag is final: the last message is exactly `NIGHT67 CLOSED: <the FAIL line>`.
- **The report, in order:** (1) what is still not good; (2) GG's play card; (3) tools proven; (4) what failed, with numbers; (5) not
  checked; (6) your calls; (7) the next prompt's seed; (8) the close lines and every ROW line.
