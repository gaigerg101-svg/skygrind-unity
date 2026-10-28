# DECIDE60 (NIGHT60: the OVERNIGHT RUN; the forge wrote D-1..D-7 on 2026-10-21 ~22:40-23:0x; the run adds D-8 on)

## D-1 The base is GG's "Overnight Run" template, mapped onto this project's files
GG pasted a generic template and asked for it made "related to this project" (NIGHT60.md, top). Its sections 0-8 and "Project-specific"
are kept in order and wording; its files map to the ones this project already keeps, so nothing is kept twice:
`.overnight/LESSONS.md` -> docs/memory/LESSONS.md (table rows, L-113 on); `.overnight/PROGRESS.md` -> NIGHT60-LOG.md (tracked);
`.overnight/BACKLOG.md` -> NIGHT60-BACKLOG.md (tracked, seeded from NIGHT59's close); `.overnight/PROPOSALS.md` -> NIGHT60-PROPOSALS.md;
`.overnight/REPORT.md` -> NIGHT60-REPORT.md; the template's "Assumptions" -> DECIDE60.md D-ids plus one log line.
Rejected: a parallel `.overnight/` folder (two lesson files would drift; the project's memory skill already harvests LESSONS.md).

## D-2 The branch is port/stage3, not `overnight/*`
Every night since the port lives on port/stage3 with a tag per night (u3-n59); CLAUDE.md forbids merging without GG. A night on its own
branch could not reach the next night's base without a merge. Rejected: `overnight/n60` (GG would have to merge each morning).

## D-3 "Revert" means `git revert` of your own commits, never `git reset --hard`
The template says to `git reset --hard` to the last overnight commit. In this tree GG's own uncommitted edits (the four files and four TMP
fonts) live beside the night's work, and L-047 records a `git checkout --` that nearly cleared them. So: `git revert <own sha>`, or a
restore of your own files by explicit path after reading their diff. Rejected: the template's command as written.

## D-4 Workers: the lead plus at most two builders, at most two reviewers at once
The template is one worker doing one task at a time. This project's measured nights build faster with builders in worktrees (NIGHT59:
three builders' slices picked in one pass), but the usage limit stops every agent at once (L-003) and an 8 h night must survive it. Each
worker still does one task at a time, verified and committed. Rejected: one worker only (an 8 h night of Player checks would idle while
the lead waits on reels); three builders (NIGHT59's cap for a 2 h night; tonight is long, so fewer and steadier).

## D-5 The check command is n60-gates.sh, made at the first session
n58-gates.sh is the measured gate (GREEN 2026-10-21 22:07:45: config copies, golden, fingerprints plain / --off57 / --off58, the Editor
compile). NIGHT60 adds `--off59` and `--off60` (both parse and print EQUAL, checked 22:34) so a new n60 row is proven off-safe. The Player
smoke runs after any change the player can see, and at the end. Rejected: the template's lint / test / build trio (this project has no
npm; its truth is golden + fingerprints + the Player).

## D-6 The backlog: NIGHT59's open items, sized, MAIN / GUARDS fixed before any number
B1-B11 (NIGHT60-BACKLOG.md). Level 4's split is two M items (B4 the split to the contract's band, B5 the Player and the row), not one L:
builder L built a whole new room in 29-50 min on NIGHT58 / NIGHT59, and the template forbids building L items. kill55 (B2) and yellow56
(B3) go first among the rows because their causes are now measured (L-111; gate-x.md). Rejected: a fresh feature slate (the template
says verified improvements first; the audit pass finds new S / M work once the backlog is empty).

## D-7 The end: 8 h or no S / M item left; hard cap 8 h 30 min; `OVERNIGHT COMPLETE`
As the template, with the project's close inside it (n60-freeze, verbs37 x5, the smoke, the harvest, tag u3-n60). The /goal's end
condition can always be met: the report plus the tag plus the final line, or the clock at 8 h 30 min (memory: a /goal must stay
satisfiable). Rejected: a close-table floor like NIGHT59's 1:55 (the template's own floor is "8 h or nothing left").
Amended by D-9: the clock counts from this conversation's first `date` (the /goal judge reads the transcript, not NIGHT60-LOG.md); the
8 h 30 min escape needs NIGHT60-REPORT.md written with what is done, and a last escape at 10 h; no new M task after 6 h 30, no new S after
7 h 30 (the close takes ~45 min).

## D-8 span58 stays OFF all night: level 4's shape is GG's open call
NIGHT59-REPORT.md "Your calls" 2 asks GG to split THE GAP NEST into a room + an approach, or keep the long gap and bend the room rules; he
has not answered. The template's conservative option covers building and measuring the split (B4, B5), not shipping it for him: B5 stops
at the Player evidence and the one-row critic's verdict with the row OFF. Rejected: span58 ON by the rule (it would decide GG's question).

## D-9 The forge's red team (a fresh reviewer, 22:4x-22:5x): 10 findings, all applied
(1) Builders cannot run n58-gates.sh in a worktree (a fixed `cd` to the main tree, and the live Editor): n60-gates.sh resolves its own
root; builders run only `--no-editor`; the lead verifies after each pick. (2) The /goal's clock escape could end the night with no report,
HANDOFF or tag, and its clock pointed at a file: see D-7's amendment. (3) The Game suite can wedge the Editor: it runs only in the close,
after the freeze checks, M alone; it never counts toward "no S or M left". (4) An unfocused Editor stalls the first session: the base
checks run before the first gates, with the autotick recipe. (5) n56-reels.sh's prune loop refuses n60 tags: n60-reels.sh. (6) B4's
contract call: `OUT=Logs/n60/... n59-l-contract.sh n58-span p2` (the script honours OUT, checked 23:01) plus its planted run. (7) B3 / B6 /
B7 / B8 / B10 made measurable (relative orbs, a done-line, the meter named, the lamp behind a row, tokens56's change behind a new row).
(8) D-8; committed M code keeps its diff review even when its row's MAIN misses. (9) n60-gates.sh fails on console errors and owns a
`--flow` baseline under Logs/n60/. (10) Deletion: only tonight's own output; the template's "or that a backlog item says to remove" dropped.

## D-10 t0 is the run's first `date` (23:05:36), not the session's (the run, 23:05)
GG set the NIGHT60 /goal at ~23:05 in the same session that forged it; that session's first `date` was 13:59:29 (NIGHT58), which would
put the 8 h close at 21:59, already past, and turn an 8 h overnight into a few minutes. NIGHT60.md section 0 step 4 defines t0 as the
run's first `date`, written at the top of NIGHT60-LOG.md: 23:05:36. Every clock tonight counts from it (close at 07:05:36, the escape at
07:35:36). Rejected: the session's first `date` (it contradicts GG's "about 8 hours" and NIGHT60.md's own t0).

## D-11 Who takes what next (00:05, 00:37)
Builder V finished B2 at 00:01 and takes B3's next steps (the lead had found the spitter / swarmer probe gap; V knows the yellow56 code);
the lead verifies B2 in the Player (only the lead runs Players). Builder L finished B4 at 00:33 and takes B14 before B10: B10's change goes
behind a NEW design.n60 sim row, which needs tonight's first n60 block and its plumbing as a shared commit (L-049 / L-077) before a builder
can patch only its own row; B14 is level data in a shipped level with a measured cause. Rejected: L on B7 / B8 (colour and light, V's
area, next after B3); L on B10 now (it would make the shared n60 block inside a builder's branch).

## D-12 B9 re-sized S -> M (00:43)
Read: SurvivalGame.N56Lead.cs's rule pauses only outside reels (FeelReel.Running) and the attract; the smoke's SmokeGuard55 undoes the pause;
Pause() logs nothing; no other flag starts a run (-skygrindBench is an automation flag). A Player check outside the smoke needs a new
automation probe (start a run, print the flow on a planted focus steal, quit), so it is an M item. Rejected: calling the EditMode pin
(N56FocusPauseTests) a Player proof (T-138 exists because it is not one).

## D-14 rim56 stays ON after the diff review's terminator finding (06:22)
The fresh diff review (Research/2026-10-21-wave60/review-n60-code.md, should-fix 1) computed that rim56 at the lit gain 1.1, with the shadow
side still x Rim57Gain, takes the red shadow / lit ratio from ~0.47 to ~0.94: skin52's cel terminator nearly gone (computed, not measured).
Measured on the frames instead (a scratch meter, the planted cel / flat bodies GREEN; Logs/n60/b7-terminator.txt): the body V spread
(p10 / p90) over 142 a357 bodies, rim56 off 0.50 -> on 0.52 (median), 17 bodies flatter by > 0.10 and 7 sharper (same-index pairing: some
pairs are not the same body). At fighting range the fog and the post set the body's spread, as Rim56.cs's own NIGHT57 note says. Kept ON;
the report tells GG it may read a little flatter up close. Rejected: reverting the ON commit on a computed number the frames do not show.

## D-15 The review's other should-fix items wait for NIGHT61 (06:30)
Not fixed tonight, each with its next step in NIGHT60-REPORT.md: rim56 reads its row twice a frame (~0.5 KB of garbage a frame, estimated;
the reviewer's one-line fix); focus60 does not exclude the Player tours that start live runs (-skygrindShots / Strip / Tile / Hitch,
DoorFree29Check, LevelSelect29Check: with the row ON a focus loss can pause them; workaround -skygrindNoFocus60); an Editor Play run whose
cursor never locked can now pause on a focus loss; n60-v-lamp.py ignores the clear shot's "cleared" field; the lamp's line string is rebuilt
every frame while lamp60 is on (OFF). Why not tonight: each is a code change past the 06:35 S cutoff with a test, the gates and a Player
check to redo before the freeze; none harms a player in the shipped config (the tours are dev runs). Rejected: a rushed exclusion list
(N29Fixes.IsAutomationArg and RunLogMath.IsAutomation disagree on what a tour is: picking one needs its own test).

## D-13 B10's cut 1 stays on its branch, not in the repo (04:57)
B10 (builder L, branch n60-b10) found the real cause of iceline's idle enemies (campaign drones frozen mid-climb: they never get the NIGHT20
climb rules) and a fix behind a new OFF sim row idle60; but its MAIN missed (15.6 / 18.4 % vs <= 10 %) and a guard failed (deaths 6 -> 9 on
seeds 1-10). The conservative, reversible option: leave both commits on n60-b10 for GG and a later night (the cause is logged; the row
would only be OFF code in the repo). Rejected: picking it as OFF code (an unshipped sim path with a failed guard); reverting the work.
