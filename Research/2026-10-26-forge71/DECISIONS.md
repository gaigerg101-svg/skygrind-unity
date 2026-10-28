# DECISIONS (FORGE71, 2026-10-26 ~09:00-11:30 MST; GG: "i want amjroity building but wnat some time spent to kill dead code ...")

The two methods, as on FORGE70: (1) wave-forge: recall (NIGHT70-REPORT.md, ledger/wave-70.md, THREADS), a NIGHT70 post-mortem
(postmortem-n70.md), an idea pool (IDEAS-POOL.md, 23 cards), three web-only research agents (RESEARCH.md R1-R3), two read-only code
scouts (scout.md), the dead-code audit run ahead of the night (DEADCODE-candidates.md), spikes (spike.md), these decisions, a red team
(redteam.md); (2) the prompt-methods re-research for Opus 5.5 (RESEARCH.md R1). Each decision names its rejected alternative.

## D-1 GG's call: bots guard, GG judges (REQUEST.md call 2)
The room GATE (M2 >= bar, the critic, the spotter) stops deciding whether a room ships. A room goes into GG's play build when its GUARDS
hold: its level boots in a Player and the reel walks it with no exception; the walk bot completes it (seeds 1-10) with deaths and stall
inside the red's band; the area lint and room check are no worse than the shipped room's; nothing the player had is lost (pads, ride
faces, footholds: VERBS.md); no wave or enemy-kind change. The M2 ratio, the launches and the throw chart are printed on the room's card
for GG as information. Rejected: keeping the GATE with new bars (six nights, 0 / 22, R3); dropping the bots altogether (they catch
softlocks, lava on the walk and lost verbs, which GG should never have to find).

## D-2 The deliverable is GG's play build, and the close-check checks it
Builds/n71-play (hard links of Builds/n71-freeze plus the build's own config: the night's rows ON, the room rows ON with the night's slots
new) and PLAY71.md (10-15 minutes: where each room is, what to try, keep / drop boxes). Updated at about 3:30 (PB1, so a dead close still
leaves GG something) and at the close (PB2). n71-close-check.cjs gains the `play` check (forge71-make-n71.cjs; self-test 33 cases PASS).
Main stays as it is: every new row is OFF in the repo's config until GG's keep (DECIDE71), and NIGHT72's first 30 minutes flip the kept
rows ON and delete the dropped ones. No exception (red team 13: "one instruction at a time" is GG's taste, not a CLAUDE.md rule, and the
one colour row is GG's open DECIDE64 call). Rejected: rows ON in main by a bot rule (the visible-rule spotter
loop cost NIGHT61-64 nights; GG judges now); a play build only at the close (NIGHT68 lost its close to a usage stop).

## D-3 The slate: four tracks, ranked, about 2x the building time (PLAYBOOK §3.17; R1.1)
Scores in IDEAS-POOL.md. Picked: C-01 (PB0, the rooms that exist), C-02/C-03/C-19 (action blocks, level 3 first: GG "level 3 just isn't
good"), C-05 (one instruction), C-06 (boss colour), C-08 (frame pacing), C-09 (input through the freeze), C-14/C-15 (cleanup), C-17 / C-18
(card items). Leftovers ranked: C-07, C-10, C-11, C-04, C-12, C-13, C-21, C-22 (moonshot, 2:00 cap). Not tonight: C-16 (Tools/night
scripts: list only), C-20 (slow39 already shipped w39), C-23 (needs GG's play first).

## D-4 Never idle: a builder whose item ends takes the next; no step waits on an event
NIGHT70 idled 5:23 after its only plan failed at 1:27 (postmortem 1). Rules: an item with two build rounds and no progress is SHORT with
its numbers and the builder takes the next item at once (R1.2); the lead keeps every agent slot busy while the slate has a todo item; the
only waits are a running job the lead needs (wait with Monitor or a background notification, never a sleep loop) and the close's clock
floor. A usage stop is not planned around (NIGHT70's act triggers measured nothing): the rule is commit every slice, state on disk, resume.

## D-5 Cleanup: the auditor ran in the forge; one row or batch per commit; 90 minutes; the never-delete list (R2)
The dead-code-auditor's list (DEADCODE-candidates.md) is the night's input, so builder D deletes from 0:10. Procedure (RESEARCH R2):
keep the OFF path, remove the ON branch, its row-only helpers, the row key in both config copies and tests that test only the ON branch;
compile, re-grep, the gates (golden, fingerprint plain and the off chain EQUAL, the flow) and the touched test classes per commit; on red,
`git checkout --` the paths, log "blocked", next. No new candidate after 1:20; a critic reads the whole cleanup diff against the
never-delete list. Rejected: deleting rows named by threads (GG's call excluded them); deleting Tools/night scripts (gates call some).

## D-6 Three builders and one rotating slot; at most four agents at once
R (rooms), B (bosses and colour, then combat feel), F (camera and movement feel, then combat feel); slot 4: D (cleanup, 0:05-1:35), then
critics and scouts. APEX S33 may run on the same seat tonight (GG sent it the same request at ~08:28): a usage stop is likely (NIGHT68's
shape), so every slice is committed as it lands. Rejected: five builders (L-003 / L-098: the limit stops all agents at once).

## D-7 Movement stays exact; feel changes are render-side or behind a sim row with golden checks
Camera smoothing, frame pacing, kicks and colour are render-side. An input buffer through the freeze (C-09) is a sim row in Skygrind.Core:
golden 0 mismatches and the 16 replays exact with the row OFF, Mover.cs untouched, and the gates' fingerprints EQUAL.

## D-8 The forge experiment E71: the owner judges, the backlog is deep
Hypothesis: the work share rises from NIGHT70's 20 % (1:37 of 8:00) to 75 % or more, and GG gets a play build. Measure (report section 8,
from git and the log): (1) work minutes = the span with at least one builder running, over 8:00; (2) the longest gap with no agent running
and no commit (target under 20 minutes); (3) items in Builds/n71-play (rooms + rows) and cleanup commits; (4) later: GG's play reaction.

## D-9 The red team's fixes (redteam.md, 15 findings; all that cost 10 minutes or more fixed)
1. The lead's Player time is the limit: three fixed proof slots (n71-p1 ~2:15, n71-p2 ~3:45 = PB1, n71-p3 ~5:30), one reel set, one spotter
   and one critic per slot; a row not picked by 5:15 is SHORT; the rows aim cut from 5 to 4.
2. The shared commit `Tools/night/n71-rows.cjs` (design.n71 with 11 placeholder rows OFF in both copies, nine view rows in FrameRows;
   tested on a scratch copy: byte-identical copies, refuses a second run); builders branch from it; picks by cherry-pick under the hold.
3. The code freeze at 6:00: the close's freeze build, verbs37, smoke and gates by ~6:50; only PB2 after 7:00's `--start`.
4. D1 is three rows (flow46, drone49, teach42): a decided OFF verdict is not an open question; shift46 goes to DECIDE71 (its flag feeds
   two reel scenarios); D reads untracked DECIDE pages by absolute path.
5. R's own slate file for rooms no slate holds, and the red made the same day from `--slots=old`.
6. One verdict file per slice (verdict-D1 / D2, verdict-R<n>); no repo flips; n71-freeze-rows is not used.
7. Every build gets its own tag (n71-p0 .. p3, n71-freeze); play builds n71-play-pb0, -pb1, n71-play; `--update` only on the same code.
8. Every build, reel set and smoke runs in the background; leftover Players and prune loops are closed by PID.
9. Reds re-made the same day (the walk bot changed at NIGHT68).
10. air65 (`n65-air.cjs`) is a guard, and walk falls are a label.
11. A quiet window 3:50-4:10 for F1's bench and PB1's smoke.
12. The brief's compile line is `bash Tools/night/dn-check-wt.sh <scratch>/dn`.
13. No repo flips (see D-2).
14. `-skygrindOff65` for the side-by-side (an off switch turns off its night and every later one).
15. press71 dropped from the slate to leftover L10 (cap 0:45, after F4); cyan71's card shows both looks.
