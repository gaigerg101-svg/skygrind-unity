# DECISIONS (FORGE64, 2026-10-23 ~08:30-09:30, run right after NIGHT63's late close; GG was not asked: he asked for the prompt)

GG, verbatim (2026-10-23 ~08:30): "is prompt fully finsihed or do we need to handoff and continue, create handoff and use that, resrecah,
follow best methods and create next prompt that will go for 3 hours". Sources: HANDOFF.md, NIGHT63-REPORT.md (section 7, the seed),
Logs/n63/gg-proxy.md (this morning's ranking of the shipped frames), DECIDE63.md, and four forge files in this folder: two web scouts
(research-colour-readability.md, research-run-methods.md), a code scout (code-scout.md) and the red team (redteam.md).

## D-1 Three hours, held by the guard's clock; the close never starts later than T-45
Tools/night/n64-clock.cjs (n63's, renamed by Tools/night/forge64-make-n64.cjs): BUILD 0:00-2:15, the close 2:15-3:00, CAP 3:00, stale after
10 h (D-9). The close starts at 2:15 at the latest (the run-methods scout: a minimum close no later than T-45, research-run-methods.md rule 3) and no
earlier than 2:00. Rejected: NIGHT63's "never before 7:00" shape (its whole close window was lost to the usage limit, 05:23-07:41).

## D-2 The lead: GG's NIGHT63 ask, finished, not the rooms
Tonight finishes the half of GG's 2026-10-22 ask that NIGHT63 did not ship ("effect colores everything doenst feel alligned"; the bosses):
the red line down every spire exit (C4), a boss order printed twice (U1), the wyrm's weak point lime (B3, NIGHT63 SHORT, a tuning loop),
plus a kit for GG's own blind A / B of eye63 (L1). The target is C4, U1 and L1; B3 if its loop lands (D-10). All are view only, each pinned
to file:line by the code scout, each in the colour scout's "fixable in 3 hours" ranking (research-colour-readability.md section 4). The
forge's fourth slice, U2 (a lesson under a "DOOR" banner), was cut by the red team: "DOOR" is a room title text61 already waits for. Rejected: the rooms (gg-proxy #1, 7 asks)
in 3 hours: a level night rebuilds geometry, its flow base and its bot walks (w52, w57 took full nights); DECIDE64 asks GG for the order of the
next long nights (rooms, movement). Rejected: boss readability by redesign (the Crown's 6 m ring, the Loom's cyan knee): enemy design, GG's
(CLAUDE.md rule 7); the code scout found the Crown and the Loom read as designed (code-scout.md section 3).

## D-3 Two builders and one review slot: at most three subagents, four agents with the lead
D (C4 then B3: both colour routing), G (U1, then the leftovers: the fight line's room name, the harvest draft); the spotter, the critics and
gg-proxy share one slot in turn. Rejected: three builders (the run-methods scout: coding parallelises poorly, two builders on disjoint files pay off in 3 hours,
research-run-methods.md rule 7; NIGHT63's usage-limit stop, L-003).

## D-4 A verified release candidate at all times
rc0 is Builds/n63-freeze (tag u3-n63, verified 07:55-08:17: verbs37 x5 27 / 29 / 31 / 31 / 31, the smoke "DONE: all steps passed", flow
"gates: all GREEN 08:17:31"); tonight's A1 adds only OFF rows, which its gates prove. Each GATE build gets verbs37 x1 on the level a slice
touches plus its own proof; the full set runs on the RC builds: rc1 (a fresh build from HEAD right after b1's flips, its copy the repo's
config: verbs37 x5, the smoke only while no agent is alive) and the freeze. If rc1's x5 or smoke fails, the rows flipped since the last RC go
back OFF in one commit and the earlier RC stays the play build. Logs/n64/rc.json holds the last verified sha. Rejected: NIGHT63's freeze-only
verification (research-run-methods.md rules 1-2).

## D-5 Boss readability is measured, not fixed, and the reel's pose is named as part of it
The code scout: the Crown's and the Loom's "unreadable" shots are their own designs; the wyrm's idle / windup / tell are the reel standing 29 m
away and aiming through a wall (code-scout.md section 3). Tonight changes neither the bosses' shapes nor the reel's pose (a reel change would
move every boss row's evidence mid-night); the report names which frames are the reel's. Rejected: a boss63 pose change tonight (leftover 3).

## D-6 A PASS binds to the sha its critic judged
The slate's passSha and passFiles; Tools/night/n64-freeze-rows.cjs refuses a flip when `git diff --name-only passSha HEAD -- passFiles` is
not empty (NIGHT63's T63c shipped a post-PASS change unreviewed and lost iceline's gauntlet lesson; GitHub's dismiss-stale-approvals,
research-run-methods.md rule 10). Rejected: trusting the lead to remember (it did not, at 5:05 last night).

## D-7 Reviewers see evidence, not the story, and at most 20 images
A critic's brief holds the doneWhen, the frames (contact sheets of four via Tools/night/n63-sheet.py where more than 20), the meter outputs,
the diff and the sha; no builder notes and no lead reading (research-run-methods.md rules 11-12; NIGHT63's B3 round-2 brief carried the
builder's reading). gg-proxy opens at most 20 images.

## D-8 The forge experiment: an always-verified release candidate
Change: D-4. Hypothesis: from rc1 on (about 1:50) a verified RC newer than rc0 exists at every moment, so a stop at any point after it still
leaves GG a verified play build with tonight's flips; and the close (in series) takes 35 minutes or less. Measure: Logs/n64/rc.json's
timeline and the close's start and end lines, in NIGHT64-REPORT.md section 7. (The close re-runs its heavy steps unless nothing was picked
after rc1: the red team, finding 8.)

## D-9 NIGHT64's tools are NIGHT63's, renamed, with three changes
Tools/night/forge64-make-n64.cjs wrote n64-clock / freeze-rows / gates / prune / reels / slate / spot-pair / slate-set (each syntax-checked):
the clock's 3-hour bands; the gates' fingerprint loop gains --off64; the pruner accepts capitalised side names (NIGHT63: Logs/feel/n63-b4 grew
to 11 GB). Then by hand, after the red team: the clock goes stale after 10 hours, not 5 (a long stop still reads CAP); slate-set keeps sha
fields as strings; freeze-rows gained D-6's check in the red team's form (the slice's own files with the config copies left out, plus the
row's own config block at passSha against HEAD) and a `--selftest` with the red team's planted case ("PLANTED GREEN (4 cases)"). Not run in
the forge: n64-gates.sh end to end (n63-gates.sh ran GREEN at 08:17:31 on the same code).

## D-10 The red team's 17 findings (redteam.md, 09:10-09:35), all applied
(1) the sha check would refuse the night's own second flip (every flip rewrites the config): passFiles are the slice's own picked files, the
config copies left out, the row's own block compared instead, a build's PASSes flipped in one call, a planted self-test. (2) the n64 tools
were untracked: committed with the forge, and HANDOFF's verify table checks one. (3) four slices do not fit: U2 cut, B3 moved into b1 as a
tuning loop, A1 timed at 20-25 minutes, the table re-timed. (4) builders cannot measure a Player red: the lead's red set on rc0 (a157 on
n52-spire, never run there before, with a two-run fallback), U1's red is its own counter's row-off half. (5) U2's premise was wrong: cut, and
re-scoped as a leftover for a later night. (6) B3's doneWhen narrowed to the wyrm's head hue by a named meter mode with planted cases; the
other four bosses checked against NIGHT63's round-2 verdict; the 2 % bar noted as met. (7) verbs37 x1 per GATE build and x5 on rc1, written
into every doneWhen; a rollback rule for a failed rc1. (8) rc1 defined (a fresh build from HEAD after the flips); the escape stated
honestly; D-8 restated. (9) the goal's CAP clause needs the minimum close done; the clock's stale window 10 hours. (10) the close in series,
and the goal's order matches it. (11) A1 lists goal64, order64 and fightLine64 in SameRun63Math's FrameRows. (12) no two dotnet runs at once
(the verify table runs no gates; the flow base runs alone). (13) C4's visible states that the red-gold bar over each door stays; C4 reads the
table with Colours63Math.From. (14) rc1's smoke only while no agent is alive. (15) the brief's builder letters, the agent counts, G's stop
time, the DECIDE64 reference and the flow base's minutes made consistent. (16) sha fields stay strings. (17) L1's blind holds for GG only:
the lead keeps the chop's direction out of the report and the card.
