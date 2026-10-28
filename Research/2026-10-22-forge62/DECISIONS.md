# DECISIONS (FORGE62, 2026-10-22, the forge's calls for NIGHT62; GG was not asked: "follow same process", FORGE61.md section 0)

## D-1 NIGHT62 lasts 6 hours: the close starts no earlier than 5:00 and must end by 6:00
GG 2026-10-22: "it will last for 6 hours". The work runs 0:00-5:00, the close 5:00-6:00, an escape at 7:00 if NIGHT62-REPORT.md is committed
(so the goal can always be met). Rejected: FORGE61's 6 h minimum / 7 h cap (GG named one number: 6); a close before 5:00 when the slate runs
dry (the leftovers list always has work; NIGHT61 waited 25 minutes for its floor with nothing left, RETRO section 3).

## D-2 Two preconditions decide the night, checked before any plan
NIGHT61 lost every Player and Editor step because it planned visible work before checking the disk and the Editor (RETRO section 1). NIGHT62's
bearings measure both and pick a branch: **A** (12 GB or more free, the Editor answers): P0 proves NIGHT61's rows in a Player; **B** (12 GB, the
Editor deaf): Player builds in batch mode from the batch copy (FORGE61's fallback), tests through the outside-Editor path; **C** (under 12 GB):
the no-Player branch, limited to work that can be proven without a Player. Rejected: one plan for every case (NIGHT61's).

## D-3 P0 is NIGHT61's debt: one build proves the five OFF rows
THREADS T-148: exitFrame61, bossRule61, text61, fixture61, gunYellow61 are written, compiled and gated, and each builder's notes hold its reels,
proof line and meter. One Player build carries all five on in its copy (`n46-build-rows.cjs <tag> on n61.<row>`), so the cheapest path to
something GG sees is a build, five reel pairs, five blind SPOTs and five critics. Rejected: new slices before the debt (CLAUDE.md order of work;
the playbook's checklist item 5).

## D-4 Two new guard rules, each a rule broken more than once
`game-m-sync` refuses a synchronous test run that reaches MapChoiceTests (or the whole suite) in the live Editor: NIGHT60's close left one
running and NIGHT61 had no Editor (T-147, L-126). `bg-chain` refuses a background `&` at the end of an `&&` chain (L-116, broken three times on
2026-10-22). Self-test 92 / 92 as planted; each refused live in the forge's transcript. Rejected: another sentence in CLAUDE.md (both rules were
already written down and broken).

## D-5 NIGHT62's tools are NIGHT61's, renamed, plus a bounded Editor gate
n62-reels / n62-prune / n62-diskwatch / n62-slate / n62-freeze-rows are n61's with the n62 names (a scripted rename, 0 names left; the freeze
tool's rows stay generic, so it can turn NIGHT61's rows on). n62-gates.sh adds --off62 and bounds the Editor step (editor_status in 60 s, the
compile in 300 s): NIGHT61's close sat 8 minutes in it. Dry run 2026-10-22 16:58-17:01: the non-Editor gates GREEN through --off62, the Editor
step "GATE FAIL: editor compile (editor_status gave no answer in 60 s: not checked)" (Research/2026-10-22-forge62/dryrun-n62-gates.txt).

## D-6 At most three builders, and each compile scratch deleted after its check
NIGHT61's three builders finished every slice handed to them; the disk fell 8.3 -> 4.1 GB with their worktrees (~165 MB each) and compile
scratch (681 MB per run) (L-119). Rejected: four builders (the disk). See research-worktrees.md for a lighter worktree (adopted only if it
is proven in the forge).

## D-7 The outside-Editor test path is a NIGHT62 slice, not a forge build
The builders' scratch harnesses showed pure logic runs outside Unity (D 48 / 48, V 168 / 168 and 85 / 85), but `Port.Root`
(Assets/Tests/EditMode/GoldenVectorTests.cs:17) reads Application.dataPath, so most tests cannot. NIGHT62's slice T1: a one-line fallback (an
environment variable read first) and a runner tool, seen RED and GREEN in the Editor when it answers. Rejected: building the runner in the
forge (the forge builds no game code; the change touches a test helper and must be seen in the Editor).

## D-8 One fresh "OFF means unchanged" review of the night's integrated diff
NIGHT61's review found 0 must-fix and 2 real should-fix bugs that no other check could see (Logs/n61/review-n61.md). NIGHT62 runs it once,
before the close prep, with the agent file .claude/agents/skygrind-reviewer.md. Rejected: a review per builder (the GATE critic reads each
slice's diff; a second reader per slice doubles the cost, FORGE61 D-12).

## D-9 The /goal names only printed things, has the end condition first, and can always be met
`date` and `h:mm / 6:00` each turn; the close's gate line or its Blocked line; the build, reels and smoke or their Blocked lines; the slate line
with 0 in progress; the tag; the clean status of the close files; `NIGHT62 COMPLETE` last; the 7:00 escape with the report committed.

## D-10 No Editor wake by window tricks
NIGHT61's one wake stole GG's foreground and did not revive the Editor (L-118). NIGHT62 asks `editor_status` with a 60 s limit; if it does not
answer, the Editor is GG's to restart and the night takes branch B or C. Rejected: the wake recipe (it needs GG idle and it failed on a wedged
Editor).

## D-11 What GG does before the run (the hand-off asks first)
Free C: to 12 GB or more (Logs/ 200,611 MB and Builds/ 70,465 MB by du on 2026-10-22; 225 git worktrees) and restart the Editor. The night never
deletes what it did not write (CLAUDE.md rule 10).

## D-13 Sparse builder worktrees stay a proposal, not a setting
research-worktrees.md: Claude Code's `worktree.sparsePaths` (git sparse-checkout for agent worktrees) exists, but a report says worktrees made
mid-session ignored it and subagents were untested; and a sparse tree would hide prefabs, shaders and scenes that view builders (S7, C2) read.
Not set in .claude/settings.json tonight. NIGHT62 keeps three builders and deletes each compile scratch after its check (D-6). Rejected:
setting it unproven (one broken builder costs more than the disk it saves); a WorktreeCreate hook (worktrees it makes are never cleaned up).

## D-12 The forge experiment
Change: the night branches on its two preconditions and spends its first hours on the previous night's built-but-unseen rows. Hypothesis: when
both preconditions hold, at least 2 of NIGHT61's 5 rows turn ON by the visible rule. Measure: the ON count in NIGHT62-REPORT.md section 2, and
the hours to the first ON.

# FORGE62b: the 2-hour resize (2026-10-22 ~20:33-21:30, after the PC ran out of memory at 17:32 and killed FORGE62 before its red team)
GG, 2026-10-22 ~20:35, verbatim: "follow similar methods from building other prompts and create a 2 hour build prompt hard cap and follow best
methods". The 6-hour draft is kept untracked as NIGHT62-6h.md (and GOAL-6h.txt); NIGHT62.md is now the 2-hour run.

## D-14 Two hours, and the cap held by a hook, not by the prompt
A budget written in a prompt is advisory (wave-forge opus-5-5 notes: "keep the hard cap in the hook"); a build, reel set or critic started late
is what pushes a short run past its end. New: Tools/night/n62-clock.cjs (start / status / stop, Logs/clock.json) and the guard rule `clock`:
FREEZE at 1:30 lets only the close's heavy steps start (a build, reel set or smoke tagged n62-freeze), no test run, no new agent; CAP at 2:00
lets no heavy step, test run or agent start; a stopped clock or one older than 6 h is ignored. The hook's matcher gains Agent|Task (settings.json).
Proven: selftest 110 / 110 (18 new clock cases); live with a planted clock at 2:05: a build refused 20:46:56 and an Agent refused 20:46:59
(Logs/guard-blocks.log), the clock file deleted after. Rejected: the /goal alone (it cannot stop a running build); a guard that also blocks
writes and commits after 2:00 (it would block the close itself).

## D-15 Scope: NIGHT61's five rows, the Editor debt, no builder
Measured minutes (RESEARCH.md section 5) fill the box: about 38 reels at 1-1.75 min each (50-60 min), five GATEs at 17-23 min, the close about
25 min. S6, the 6-hour draft's builder slice, is dropped: the speed FOV it asks for exists and works (CameraFeel.cs, camera.fovBoostDeg 8;
NIGHT60's verbs37 telemetry 74 deg standing, median 82-83 deg at 20-26 m/s, max 85.8), so its red "equal" was never measured and is wrong
(wave-forge: grep each pick's mechanism, not its name). C2, S7 and S2 take 60-75 builder minutes (NIGHT61's S4 65, S1 70) and could not be judged
before 2:00. Rejected: one builder on C2 (its code would wait on a branch; one more agent after today's memory crash); T1 (the Editor answers
tonight); P8 inside the run (it holds the Editor up to 40 min; a leftover only).

## D-16 Three GATE slots and one spotter / gg-proxy slot, four agents at most
With no builder, its place goes to a third critic so the five GATEs do not queue (two on branch A-lean, under 20 GB commit free). Rejected: six
agents (the project's cap): the PC ran out of virtual memory at 17:32 with several Claude sessions and their MCP servers open.

## D-17 Branches A / A-lean / C; FORGE62's branch B dropped for two hours
The batch-copy builds (B) cost a sync and a cold import that a 2-hour box cannot carry. The Editor answered every forge check (gates 20:44-20:46,
set_autotick 20:50, a test class 20:53). If it goes deaf at two checks 10 minutes apart, the run takes C and says so.

## D-18 Memory is a precondition, and the launch line loads no MCP servers
The bearings read commit free (20 GB for branch A). The launch line adds `--strict-mcp-config` (no --mcp-config: the night uses the unity CLI,
no MCP server): each Claude Code session here starts about 24 child processes, 6 of them node at 0.69-0.76 GB two levels down; 67 node
processes held 8.5 GB machine-wide ten minutes after the restart (2026-10-22 20:2x). Dry run 20:52:32-39: `claude -p ... --model
claude-opus-5-5 --strict-mcp-config --append-system-prompt-file nights/PROMPT-STOPS.md --dangerously-skip-permissions` answered "OK".

## D-19 gg-proxy ranks the P0 frames before the freeze, not at the close
The freeze refuses new agents; the P0 build's frames carry all five rows on, the newest frames there are. The report says which frames it
ranked. Rejected: no ranking (section 1 of the report would repeat NIGHT61's start ranking).

## D-20 P1's SPOT pair uses the one-row switch; its MAIN keeps its doneWhen's pair
P1's doneWhen measures "on vs -skygrindNoExit58"; the visible rule wants a pair that differs only by the row's own switch. A third side
(s1spot, -skygrindNoExitFrame61) serves the SPOT; the doneWhen is unchanged. Cost: one reel.

## D-21 P5's meter is written in the forge
V-C3.md step 4 says "Which tool: not checked", and n56-colour.py finds 0 frames in an a157 folder (no telemetry.jsonl there).
Tools/night/n62-gun-yellow.py counts yellow56's band (45-70 deg, S > 0.5, V >= 0.15) inside a rect of a PNG: planted 5 / 5; red 2,887 px on
NIGHT60's kb2c entry (n60-b8 b8on). The verbs37 poses keep n56-colour.py's yellowGunPct (the gun's own rect from telemetry).

## D-22 The seed for the next prompt is the report's section 7
SKYGRIND records solved problems as LESSONS.md rows (wave-memory), not docs/solutions/; compound-engineering's /ce-compound is not set up here
(no .compound-engineering/config.yaml with cross_model_review_mode off), so the 2-hour close does not start it.

## D-24 The red team's fixes (Research/2026-10-22-forge62/redteam-2h.md, 14 findings, 21:00-21:18; all fixed but the three below)
Fixed in NIGHT62.md and the tools: verbs37 x5 runs first, because every GATE's "nothing lost" line needs it (finding 1, the blocker); every
long job and agent runs in the background, and the reel side timeout is 420 s, not 1500 (2); P1, P2 and P4 run their SPOT pair first, so a
row the spotter cannot see costs no more reels (3); no fix round (4); no disk watcher, which would keep the /goal from being evaluated, and
the clock line prints C:'s free GB instead; the close prints `git tag -l u3-n62` (5); n60-v-lamp.py prints P1's OWN GAIN line with three planted
cases (the red is 0 / 5 on it against 4 / 5 on the old line) (6); n62-gun-yellow.py --pair counts yellow among the recoloured pixels and boxes
the yellow left (planted 9 / 9) (7); the critic agent reads the slate the lead names (8); the close starts by 1:25, runs the flow gate once
after the last flip, makes verbs37 on n62-freeze conditional on the clock, and leaves the memory harvest to the next forge (9); three duplicate
reels dropped (10); the memory command's quoting (11); P6 / P7 verdict files written by the lead (12); n62-prune.sh's loop waits for its own
set's "== all done", and the guard names n62-prune.sh (13); the forge commits its tools before the launch (14).
Not fixed, with why: P4 keeps its five-level MAIN and three-level flicker guard (its doneWhen is written and never loosened; SPOT-first and the
cut order bound its cost); the boss61 reel's length is still not measured (no Player in the forge); the GATE's 17-23 minutes stay the
estimate (L-109), so P4's GATE may still land after 1:25 and be cut.

## D-23 The forge experiment (FORGE62b)
Change: the hard cap is enforced by the guard's clock rule (FREEZE 1:30, CAP 2:00) instead of only by the prompt and the /goal. Hypothesis: the
close starts by 1:35 and the last commit lands by 2:00, and no heavy step starts after its phase allows it. Measure: the close's start and end
against t0 in NIGHT62-LOG.md, and every clock refusal in Logs/guard-blocks.log (report section 7). D-12's measure (rows ON) stands beside it.
