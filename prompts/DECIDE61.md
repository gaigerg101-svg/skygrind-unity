# DECIDE61.md: the lead's calls on NIGHT61 (untracked; one D-id per call, the alternative rejected; GG was not asked)

## D-1 (10:21) No-Player night: C: has 8.3 GB free, under the 12 GB floor
Call: no Player build, reel set or Player evidence tonight. The disk watcher is not started. `df -h /c` is re-run at the top of every
hour; the night resumes Player work the moment 12 GB are free.
Why: `df -h /c` = 8.3 GB avail; the guard refuses builds and reel sets under 12 GB (NIGHT61 section 1 step 2; FORGE61 D-6, D-18).
Rejected: deleting anything the lead did not write tonight (the night's rule; the forge's candidates, `.claude/worktrees` and
`Builds/n60-b1`..`n60-b8`, are GG's call under D-6); the batch-mode fallback (the disk check refuses batch builds and test runs too).

## D-2 (10:33) One wake of the Editor, with GG's foreground window given back
Call: ran the unfocused-Editor recipe once, at 225 s of idle time (`Tools/night/editor-wake-idle.ps1` logic, plus a restore of the
previous foreground window), then `unity command set_autotick`. Result: Unity got the focus; set_autotick timed out at 30 s; GG's
browser window was restored at 10:33:44 by the Alt-key unlock and verified with GetForegroundWindow.
Why: NIGHT61 section 1 step 3 orders the recipe after the run/compile check (`Logs/tests-last.json` shows a timed-out run from 07:44;
the Editor is idle at 2.8 % CPU, so no run is in flight).
Rejected: killing the Editor (CLAUDE.md rule 10; the guard blocks it); a second wake now (it steals GG's foreground again with no new
reason). Next try: the top of each hour, and only when idle >= 60 s.

## D-3 (10:41) The 20-minute rule: the `--no-editor` path
Call: the Editor answered nothing from 10:21 to 10:41 (editor_status, set_autotick, a 45 s status retry at 10:39). The night runs
`bash Tools/night/n61-gates.sh --no-editor` for every gate that does not need the Editor, and logs every Unity-side check it could not run
as "not checked" under Blocked. The flow baseline (`--flow-base`) runs now, in the background.
Rejected: waiting for the Editor until the close (it would leave every slice with no check at all).

## D-4 (10:44) Unity code is compile-checked without the Editor
Call: builders' Game, Tests and Editor code is compiled with `bash Tools/night/dn-check-wt.sh <scratch dir>` (a plain directory under the
session scratchpad), which regenerates the csproj files with `Tools/wt-csproj.cjs` and runs `dotnet build` on each. A slice's code counts as
compile-checked when that build reports 0 errors. Its EditMode tests are "not run" until the Editor answers.
Why: `n61-gates.sh --no-editor` does not compile the Game assembly (the compile gate is the Editor path only), so an unchecked Game commit
would break the next Editor compile. Rejected: committing Game code with no compile at all; writing no Game code tonight (the plan's no-Player
branch allows code behind OFF rows).

## D-5 (10:44) Visible slices: red from existing frames, code only behind OFF rows, nothing turns ON tonight
Call: S1..S7 measure `red` read-only from frames that already exist (Logs/feel/n60-*). A builder may commit OFF-row code for a slice when
dn-check-wt.sh is GREEN. No row turns ON: the visible rule needs a Player frame and a SPOT line, and no Player can be built under 12 GB.
Rejected: turning a row ON on a compile check or a sim number alone (the rule, NIGHT61 section 2); inventing frames by any other route.

## D-6 (10:44) Agents tonight
Call: gg-proxy (the honest state check, NIGHT61 section 1 step 6) and dead-code-auditor (A1) start now, both read-only except their
own output files. Builders follow one at a time, with `df -h /c` between spawns: D (B1, then D1), then V (S1), then G (S3), each only while
fewer than four builders are alive.
Why: neither the state check nor the dead-code list needs the Editor or the disk. Rejected: waiting for the A1 shared commit before the
auditor starts (the auditor reads, it does not depend on the design.n61 block).

## D-7 (10:50, to confirm) B2 cause, read before any run
Call: B2 is read from the code first (NIGHT61 section 3). MapChoiceTests has one case per listed map (48 maps); each case composes a headless
sim (about 35 s of composition in CoreCLR per the test's own comment, about 3.9x that under Mono). A synchronous run_tests over the Game class
therefore runs past the Pipeline's 600 s client limit, and the Editor's Pipeline cannot answer while its test run holds the main thread.
Rejected for now: weakening or removing the map cases (tests are never edited to pass). The fix candidates are the suite's async mode
(`run_tests --async_tests true --timeout 2400` plus `test_status` polling, per the unfocused-autotick note) and running Game M only in batch
mode on the batch copy, which the disk refuses tonight. Recorded as a hypothesis until the Editor answers and a run confirms it.

## D-8 (12:03) D1 scope tonight: batches 1-7 and 11-36 only
Call: the dead-code auditor's list has 59 batches (about 760 lines; Logs/n61/DEADCODE-candidates.md, finished 12:0x). Tonight builder D
takes batches 1-7 (private Game members) and 11-36 (unused UnityTuning constants), compile-checked per batch, with the lead's grep and the
auditor's "for GG" list as the gate. Deferred to the next session, listed for GG: batches 8-10 (each removes a hint NIGHT17 kept on purpose,
and 8 and 9 remove tests: a test is never deleted to make a build pass); 37-51 (public read-outs; the ShotTour reel entry, which a reel script
may call); 52-59 (Core: the web-ported files stay line for line, and each batch needs golden, replay and fingerprint gates). Rejected: the
whole list tonight (about 60 compile cycles and the disk: 5 GB free at 12:00). Constants of an OFF row GG has not decided on are not deleted.

## D-9 (12:05) B1 integrated without the Editor; its message says GREEN, and that is the scratch harness only
Call: the three builder commits were cherry-picked in order onto port/stage3 (f84ee2e7 test first, 9fa1c72d the change, c7f502ca the notes);
not amended (the guard refuses `commit --amend`). The change commit's message says "FocusAutomation61Tests GREEN". The only GREEN behind that
is the builder's scratch harness (outside Unity, against the built Skygrind.Game.dll): the stub gave 33 fail / 15 pass, the change 48 / 0. The
EditMode run in the Editor has not happened. Recorded so the report says it, not the commit message. The lead reviewed the full code diff
(PlayerChecks.cs and SurvivalGame.N60L.cs: 29 lines in, 5 out) and checked the reel launcher: reel.sh passes -skygrindReel to every reel, so
every reel run is in FocusAutomation61Modes.

## D-10 (12:13) Builder V's exit path also goes off under -skygrindNoExit58
Call: the new path (row design.n61.exitFrame61) is off under -skygrindOff61, under its own switch -skygrindNoExitFrame61, and under
-skygrindNoExit58. Builder V asked for the check: the slate's S1 compares "on vs -skygrindNoExit58 in the same build", and a switch that
kept the new frame lit under NoExit58 would make that comparison measure nothing. Rejected: keeping the new frame lit under NoExit58 (the
OFF side would then be a lit door). lamp60's path and its numbers are untouched. V states it in the row's _why.

## D-11 (12:12) Builders V and G are alive; no respawn
Call: both answered the lead's status request by SendMessage (V: read-only analysis of the red and the eight lit doors' floor-line runs;
G: S3's reading done, its test-first commit next). Their transcripts had stayed at 0 bytes since 11:25 and 11:27, which read as silence and
was not: the status replies came from their tool rounds. The rule that a builder is stopped and respawned applies only to a builder that
does not answer.

## D-12 (13:01) No second Editor wake; builders delete their compile scratch after each check
Call: at the 13:00 check the Pipeline still did not answer (40 s probe). The lead does not repeat the focus-stealing wake: the first wake (10:33,
Unity held the focus for about 30 s) did not revive it, and the Editor is idle, not busy. The remaining route is GG's restart of the Editor
(CLAUDE.md rule 10: the night never kills it). The disk was 4.7 GB at 13:00 (the floor for new scratch is 4 GB), so each builder deletes its
own scratch folder after each compile check (rm -rf; dn-check-wt recreates it). Rejected: a second wake (same expected result, the cost is
GG's focus); waiting for the Editor to the close (it would leave every EditMode test unrun for the whole night).

## D-13 (13:55) The lead's combined compile may run once, when no builder is compiling, with the disk at 4.6 GB
Call: the combined dn-check needs a 681 MB scratch. At 4.6 GB free the peak is about 3.9 GB, just under the 4 GB floor set for new scratch
(D-12). The floor protects against builders' scratch folders filling the disk; once no builder is compiling, one lead compile is a
temporary peak that is deleted at once, so the lead runs it when V's S4 is the last pick and the disk reads at least 4.5 GB before it
starts. Rejected: skipping the combined compile (it is the only compile of the integrated tree); waiting for a bigger disk (no sign of one).

## D-14 (15:01) A fresh read-only review of tonight's integrated diff, in place of the GATE critics that could not run
Call: with the builders done and 48 minutes to the freeze, the lead spawns one fresh reviewer (general-purpose, read-only, findings to
Logs/n61/review-n61.md) on `git diff bda4f11a..HEAD`, first asking whether every hook added to existing files leaves the game unchanged with
the n61 rows OFF. Why: no skygrind-critic ran tonight (it needs Player evidence), so no builder's diff had an independent read; the lead's own
review was partial. A must-fix it finds is fixed before the close (a fix to a picked slice is allowed after the freeze). Rejected: builder D on
dead-code batches 37-50 (low value; its compile scratch would take C: under the 4 GB floor); a beyond-list item (each needs a Player).
