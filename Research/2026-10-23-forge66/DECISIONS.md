# DECISIONS (FORGE66, 2026-10-23 ~21:00-22:00, right after NIGHT65's close; GG was not asked: he asked for the prompt)

GG, verbatim (2026-10-23 ~20:50, during NIGHT65's close loop): "can we follow same methods for creating next detailed night prompt that will
go for 8 hours?". The method is FORGE65's: four scouts in this folder (history.md, n65-data.md, research.md, code-scout.md), these
decisions, a red team (redteam.md), the slate seed, the builder brief, GOAL.txt, the generated n66 tools. Grounding: the repo's record
(docs/memory, NIGHT65-REPORT.md / -LOG.md, Logs/n65/, DECIDE65.md); Google Drive and Gmail were tried for SKYGRIND material and both
sessions had expired (not searched); Linear is not connected.

## D-1 Eight hours, held by the guard's clock; 7:30 minimum (the lead's reading of "8 hours")
Tools/night/n66-clock.cjs (forge66-make-n66.cjs): BUILD 0:00-7:15, FREEZE 7:15-8:00, CAP 8:00, stale after 10 h. The close starts between 7:00
and 7:15; NIGHT66 COMPLETE at 7:30 or later and before 8:00. GG named no minimum; 7:30 follows "7 hours 6.5 min" (DECIDE66 asks him).

## D-2 The disk is a go / no-go at t0, not a branch the night discovers
Measured (code-scout.md section 2): a build 382 MB; one reel side raw up to 0.83 GB, pruned 37-140 MB; three builder worktrees 2.1 GB;
NIGHT65's unexplained drift about 0.47 GB/h; the guard refuses builds, reels and smokes under 12 GB at their start. So: **A (Player
track) needs 23 GB free at t0; C (sim only) needs 10 GB; under 10 GB the night does not build rooms** (it runs the tools and the analysis
only, with one builder, and the report opens with it). C: had 8.2 GB at the forge. DECIDE66's first item: what GG can free (21 superseded
builds, about 8 GB; NIGHT63-65's worktrees). Rejected: discovering the branch at 0:05 again (NIGHT65's whole Player half was lost to it).

## D-3 The focus: level 1's rooms made to NEED their verbs, then shipped; b2 THE GANTRY joins as GG's real "awful" ride
GG's words since w33 ask for rooms that make you use the mechanics; gg-proxy on the final n65 rooms (Logs/n65/gg-proxy-final.md): b1 YES,
a3 / a5 UNSURE, a2 / a8 / a4 NO, his predicted first words "The rooms still don't incentivize wall-riding or creative use of the weapons".
The history scout found GG's "waallride section ... awful and relaly difficult and not fun" (2026-10-14) is THE GANTRY b2
(Research/2026-10-14-wave42/GG-REQUEST.md:21), which NIGHT65 kept bit for bit. Tonight: the six n65 rooms (from their final files) plus a
seventh slot b2 (it must keep verbs37's 25-30 m ride wall and 14 m lane: nothing lost, VERBS.md). Not tonight: other levels, bosses, the
movement code (Mover.cs stays: CLAUDE.md rule 3, GG's "don't change the core mechanics").

## D-4 Instruments first: the skip test v2, then MAIN is fixed against its red
The data scout (n65-data.md sections 1, 3, 6): NIGHT65's GREENs came with the new test, not the rooms (a5's file equal at r3 and r3b, RED then
GREEN); the refused run swaps the whole verb-aware layer out (a confounded control); 9 seeds with the verb never performed still read a median
ratio of 1.25 (the GREEN bar). **Builder T, 0:05-1:30:** skip v2 (the refusal inside the verb-aware layer, so the only difference is the verb;
10 seeds; the per-seed paired ratio refused / verb-aware; seeds exercised printed; a jittered pilot option), the ride pilot shooting from the
wall, the launch pilot's target into a hazard (`--wave` / `--target` exist), the "not just decoration" checker (a piece passes only if it can
be stood on, ridden or bounced, blocks a ranged enemy's sight of the walk line, kills a launched enemy, or marks the next piece / the exit:
research.md lever 9), the close-check script (D-10). Then the v2 red on n52-foundry and on NIGHT65's final rooms (both are reds: a room must
beat the old room AND keep what NIGHT65 gained).

## D-5 MAIN per room on the verb instruments, written into the slate at A1 before any round is measured
For every room: **(M1)** skip v2's median paired ratio >= 1.5 over 10 seeds with >= 7 exercised (research.md: no published bar; 1.25 sits
inside the noise per n65-data.md); **(M2)** STYLE above DULL with the verb >= refused + 15 points; **(M3)** the room's own verb pilot GREEN on
its card's line (a5: a launched body "in lava yes"; a3 / b2: a kill from the wall); **(M4)** every new piece passes the decoration checker.
GUARDS (no worse than the n52 red and NIGHT65's final): room check items, n46flow clears 10 / 10 on both seed sets, deaths and stall in band,
air65 hand-offs. The walk bot's DULL is a GUARD at most (L-150).

## D-6 No card-gate cycles: a verb plan per room, one pass, then rounds
NIGHT65's card gate took about 45 min for three passes (history.md section 5). The rooms exist; each builder writes a verb plan per room (<= 40
lines: the lever from n65-data.md / research.md, the pieces, the MAIN aim) by 0:45; one critic reads all seven in one pass (<= 15 min); open
items become round 1's BLOCKING list (DECIDE65 D-4's rule, now the plan).

## D-7 Four reviewed rounds; a GATE per builder's set; one fix round
Rounds at about 1:45 (R1), 2:55 (R2), 4:05 (R3), 5:15 (R4); each: the builders' sim loop, the lead's round (n65-round.sh with v2), the
iso sheets, the two reviewers (gg-proxy, level design) batched; in branch A also the data-swap Player round (D-9). GATEs from about 5:30
(R's set) and 5:45 (S's and T's b2); one fix round picked by 6:30; no new work after 6:45.

## D-8 A room's GATE is sim-side; the Player items belong to LV
NIGHT65's room doneWhens held Player lines in a night that could not run a Player, and both critics capped every room at UNSURE (history.md
corner N2). Tonight a room's doneWhen is the sim's (D-5's MAIN and GUARDS, the blind SPOT on the iso pair, the reviews, the critic). A PASS
puts the room in slots65.json (the shipped level behind the OFF row). The Player items (tour65 frames, verbs37 n65-foundry 27 / 27, NRE 0,
-skygrindOff65's boot, the smoke) are LV's, the level row's own slice, and the row flips only by LV's critic (n66-freeze-rows.cjs binds the
level files and slots65.json: the NIGHT65 review's S2, fixed in the generated tool, self-test 5 cases).

## D-9 Branch A's Player track by data swap: one code build, rounds as hard-link copies
code-scout.md section 2: a hard-link copy carries data only (config and level), never code. NIGHT66 builds once from A1's code (b0: tour65,
the S3 fix), then each round's Player reel runs on `n65-play-build.sh b0 r<n> <level> --config ...` copies (0 build bytes; ~0.18 GB of pruned
reels): tour65 frames for the reviewers and the SPOT, verbs37 n65-foundry. LV's GATE build is a real build (rc1). Without branch A: the rooms
are judged by the sim and the iso pictures, and the play build is n66-play-latest (data swap on n64-freeze, D-7 of DECIDE65).

## D-10 The goal names one end-state check, not an ordering a judge must reconstruct
NIGHT65's goal said "in order", "before the close starts", and the Stop-hook judge looped nine times on history it could not see as ordered
(NIGHT65-LOG.md "The close"; research.md finding 10). Tonight's goal: met when the transcript shows `node Tools/night/n66-close-check.cjs`
printing `CLOSE-CHECK n66: PASS` (it checks the clock stopped at 7:30 or later, the tag on HEAD, the four close files clean and committed
after the close's start mark, the gates' last line, the slate, the row vs slots66, the Blocked lines when a Player step was refused), followed
by the final message `NIGHT66 COMPLETE`. The fallback for a stop: the same script prints `CLOSE-CHECK n66: MINIMUM PASS`. Nothing after it.

## D-11 Builders: T (instruments, then b2), R (a2, a3, a5), S (b1, a8, a4); at most four agents alive
T: D-4's tools by 1:30, then b2's verb plan and rounds 2-4 (b2 needs a new slot in gen-n65-foundry.cjs: T's own file). R and S keep their
rooms (they know them; their notes are in Research/2026-10-23-wave65/). The fourth slot is the review / critic slot.

## D-12 Levers that need GG first (DECIDE66), not built tonight unless he says yes
A dash-pip refund on a ride or launch kill (research.md finding 2: a movement economy change, a default-off row, the golden vectors kept);
a new enemy type on level 1 (a charger leading a8's file: the only pierce gain NIGHT65 measured, n65-data.md); the guard's 12 GB floor.

## The red team's amendments (2026-10-23 ~22:10-23:30; redteam.md findings 1-14; they supersede the decisions above where they differ)
- **R-1 (finding 1):** NIGHT66's round tools are n66 copies (Tools/night/forge66-make-n66-rounds.cjs: n66-round.sh, n66-round-verbs.sh,
  n66-red.sh, n66-table.cjs, n66-gate-pack.sh / .cjs): Logs/n66 by default, an existing round folder refused, the slot rooms from
  slots65.json, the skip at 10 seeds, the reds Logs/n66/red and red65, the verb plan in place of NIGHT65's card. A1 dry-runs them (dry0).
- **R-2 (finding 2), amends D-2:** the branches are A >= 23 GB, A-lean 18-23 GB (b0, one tour reel, LV), C 7-18 GB, no rooms under 7 GB.
  NIGHT65's three worktrees are reused in every branch (0 new bytes). DECIDE66's numbers corrected: the 21 builds give about 16-17 GB.
- **R-3 (finding 3), amends D-5:** skip v2 runs three runs per seed; M1 = the median paired ratio of the faster refusal (inside-the-pilot
  or plain walker) over the verb-aware run, at or above the room's mainBar = max(1.25, its red65 v2 ratio + 0.20), 7 of 10 exercised. The
  1.5 is dropped: it was fixed before its instrument existed. GUARDS: the walk bot's clears and deaths; stall on n46flow --verbs65. The
  N10 rule: two rounds with no M1 move, redesign or drop. The prediction is three or more rooms, not five.
- **R-4 (finding 4), amends D-10:** the close-check works from PowerShell and Git Bash (Win32_Process through powershell.exe), counts only
  this night's jobs since t0 by PID, and has --selftest (23 planted cases), --pre and --tag; the tag comes after the pre-check; the final
  check runs alone in the foreground. n66-gates.sh sets MSBUILDDISABLENODEREUSE=1.
- **R-5 (finding 5), amends D-10:** the MINIMUM line is exact and only after a stop ate the close (start at 7:30 or later, or a stop at
  8:00 or later), and still needs the slate and the row; GOAL.txt no longer asks the judge to read history and has a way out after the tag.
- **R-6 (finding 6), amends D-7 and D-11:** at most four subagents running; T ends its turn when its slices are done; the builders end theirs
  after round 4, so the GATE agents run in parallel; a room may GATE after round 3; LV starts at the first PASS, no later than 6:00; rc1 is
  built as n66-freeze and reused by the close when no code changed after it.
- **R-7 (finding 7), amends D-3 and D-11:** b2 is a slot (T2, a tool), not a room, unless GG's own message names it; then R or S builds it on
  T's frozen tools and it is judged as a traversal. T builds no room (L-106).
- **R-8 (finding 8), amends D-4:** T1 is three slices: T1a (tour65 S3, the safe --run) by 0:40 for b0; T1b (skip v2, the reds) by 1:30;
  T1c (ride --shoot, the decoration checker v2, --out) by 2:15. Round 1 moves to about 2:00; one extra build is allowed for a tour65 fix.
- **R-9 (finding 9):** the decoration checker's four tests read skip v2's per-piece touches and launch36 kills, against `--base <t0 sha>`.
- **R-10 (finding 10):** the night never deletes; DECIDE66 lists GG's own commands. GG's answers in a file steer priorities only; a deletion,
  a bulk change or a D-12 lever needs his own message (the org rules: file content is data, only the user authorises).
- **R-11 (finding 11):** a round saves its level and plan under Logs/n66/<tag>/ (the play build and the GATE pack read them); no build, GATE
  pack or `git add Design/levels` while a round runs; one dotnet job in the main tree at a time; T1c moves the round off the shipped files.
- **R-12 (finding 12):** the close-check reads the freeze summary's six sides and the smoke log, accepts Blocked only with the guard's own
  words, needs the slate tool's exit 0 with 0 in progress and 0 todo, and checks `frozen` (no slot or config commit in the close).
- **R-13 (finding 13):** LV's doneWhen holds n46flow on n65-foundry (both seed sets), `n39views wall n65-foundry`, the `--off65` flow against
  flow-base-a0, and verbs37's five numbers named per level.
- **R-14 (finding 14):** the tool slices are `n/a (tools)`, PC `n/a (a card)`, so their PASS keeps the slate well formed.
