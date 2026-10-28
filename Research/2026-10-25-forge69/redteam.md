# FORGE69 red team (fresh reviewer, 2026-10-25; read only; no dotnet / harness / build / Player / Editor)
Ran: `n69-slate.cjs <seed>` (8 slices, exit 0), both `--selftest`s (PASS), `bash -n room-chain.sh` (OK). The seed's bars and ceilings
equal Logs/n68/slate.json (1.33 / 1.25 / 1.25; reds 1.13 / 0.87 / 0.63). ROOM[2] has the ALONE round, ROOM[4] the new STYLE rule.
Not checked: run times of gen-slots --run and the play build, or how the pack re-runs on an existing folder.

## Findings, most severe first

1. **Y2 can never be picked.** NIGHT69.md:82-84 and builder-brief.md:49-51: Y2 (Harness.N65.Verb.cs, a SimHarness change) lands at 1:00;
   round a1 runs in the background from 0:50 to about 1:22 (3 rooms x ~10 min + a build), rule 6 (NIGHT69.md:22) forbids any pick while it
   runs, and the code freeze at 1:15 forbids any SimHarness commit after it. So Y2 never lands, "one kb3 red re-run (Y2's)" has nothing to
   re-run, and a1's launch M3 for kb3 / v2 is read by the old reader that printed a RED probe as `M3: kb3 PASS` (seed Y2 `why`).
   Fix: order T as Y1 0:20, **Y2 0:45**, Y3 1:00 (Y3 is node only, outside the SimHarness freeze); pick Y2 before a1 starts; a1 waits for it.

2. **No `Logs/n69/red-<L>` is ever made, and the GATE pack and table read only that.** n69-gate-pack.cjs:58, :66, :346 and n69-table.cjs:17
   read `Logs/n69/red-<L>`. The reds are in `Logs/n68/red-{kiln,iceline,spire}`, and no step in NIGHT69.md section 4 copies them.
   The pack lists "red side" MISSING and exits 3 (gate-pack.cjs:7-8), so no room can hold a GATE. This is the same kind of miss NIGHT68 had
   ("reds beside a newer harness"), and n69-next.cjs does not print it. Fix: bearings step 2 adds
   `cp -r Logs/n68/red-kiln Logs/n69/red-kiln` (the same for iceline and spire) and logs it; add a `red` piece to n69-next.cjs.

3. **The GATE pack runs before the level review exists, and no step runs it again.** room-chain.sh:25-26 packs at the round's end.
   The pack needs a review of THIS round with `## <id>` (gate-pack.cjs:393-394), so it always exits 3. Fix: in section 6 (a1 and a2 rows),
   after the review lands: `L67=<L> bash Tools/night/n69-gate-pack.sh <id> r<n>-<L> --fresh Logs/n69/r<n>-<L>/fresh-<id>.txt`. Add a
   `pack COMPLETE` piece to the printer.

4. **There is one review slot for about 10 reviews, and a2's GATE cannot finish before 2:20.** The queue for that slot: critics for Y1, Y3
   and Y2, the card critic, a1's level review, the spotters, the GATE critic, a2's level review, the batched critic and gg-proxy. A critic
   takes 17-23 min (CORE L-109). a2 starts at 1:50 and needs 3 x ~10 min, so it ends about 2:22: its review and critic fall after the
   stop, and rule 2 (NIGHT69.md:14) is broken. Fix: say plainly that only an a1 room can PASS tonight. a2 takes the ONE best open room
   (1:50-2:00, review 2:00-2:12). T's slot becomes a second review slot after Y3 (1:00).

5. **The gates collide with a1, and their timing is undercounted.** The gates start "1:15, foreground, alone" while a1 runs to about 1:22.
   `--flow` adds an n46flow run on 5 levels (~5 min, the same as the base: NIGHT69.md:62) to the ~5-6 min gates, so ~11 min. With Y2's
   red re-run (n69-red.sh plus a 20-seed skip, ~7-10 min) the main tree is busy until about 1:45, and no pick can happen meanwhile. That
   leaves the builders' 1:50 fixes almost no time to be picked. Fix: "the gates start at `room-chain-1.done`"; the red re-run follows them;
   the fix deadline becomes 1:55.

6. **The minimum close tells the lead to quote a refusal that never printed.** Section 7 (4) (NIGHT69.md:94-95) writes
   "SKYGRIND guard blocked this (disk)" for a freeze build, verbs37 and a smoke that nobody tried. That breaks CLAUDE.md rule 9 and
   NIGHT69.md rule 7, and the quote is not needed: `--tag --minimum` allows player and gates to be missing (close-check.cjs:168-171,
   :181-182). Fix: try each once (at CAP the guard refuses in under a second) and paste the line it prints, or write "not tried (CAP)".

7. **Section 7 asks for the ROW lines before any step prints them.** Step (4) needs "every ROW line the close-check prints", but `--start`
   exits before printing them (close-check.cjs:74-82). The first step that prints them is (6), and its tag needs them already in place.
   Fix: add step (3b) `node Tools/night/n69-close-check.cjs --pre`, then copy each `ROW: ...` segment of its `row` line exactly (they are
   joined by ` | ` and suffixed "(NOT in the report section 8)").

8. **The PC-1 command passes the wrong arguments.** n65-play-build.sh:14 and :19-21: the level files must be ONE comma-joined argument;
   extra positional arguments become rows for n46-build-rows.cjs. NIGHT69.md:67 writes `<the three level files>`. Fix: write it literally:
   `Logs/n69/play/kiln/n67-kiln.json,Logs/n69/play/iceline/n67-iceline.json,Logs/n69/play/spire/n67-spire.json`. The LIDs are
   n67-kiln / n67-iceline / n67-spire (levels67.json). Also, `Logs/` is gitignored (.gitignore:6), so "commit Logs/n69/play-pc1.txt"
   needs `git add -f`.

9. **Nobody produces the patch share (M4), M3 or the spot for the GATE.** grep finds 0 hits for patch-share in Tools/night/n69-*. Neither
   room-chain.sh nor n69-round.sh runs it, and n69-gate-pack.cjs does not pack it. The printer's pieces (n69-next.cjs:52-75) leave out
   patch share, M3, guards and spot. So the forge experiment's MISS measure is blind to these. Fix: room-chain writes
   `node Tools/night/n69-patch-share.cjs --level $L $id > $D/patch-$id.txt` when the tool exists; the printer adds `patch`, `M3` (from
   `$D/verbs-launch.txt` / `verbs-ride.txt`) and `spot`.

10. **The round tag differs in the ROOM text.** ROOM[2] (make-slate.cjs:17) gives the ALONE round as `n69-round.sh a<n>-<id>`.
    room-chain.sh:17-18 uses `r<n>-<L>`, and n69-next.cjs:41 accepts only `^(r\d|alone)…-<L>`. A critic who reads the ROOM list literally
    can fail a correct pack. Fix: in make-slate.cjs change the text to `r<n>-<L>` (via room-chain.sh) and regenerate with `--force`.

11. **The gates file never gets its HEAD line.** NIGHT69.md:84 asks for `git rev-parse HEAD` as the first line, but the command redirects
    only the script, and section 8 (c) depends on that line. Fix:
    `{ git rev-parse HEAD; bash Tools/night/n69-gates.sh --no-editor --flow; } > Logs/n69/gates-flow-close.txt 2>&1`.

12. **Y1 may land too late for a1.** Y1 lands at 0:20. Its pick, RED / GREEN pair and five-level walk (~5 min, which must wait for PC-1's
    gen-slots, rule 6) and its critic (17-23 min) end about 0:50-0:55. If a1 starts first, M2 is judged on the old STYLE rule
    (91-94 %, unreachable), so room-chain makes no pack. Fix: a1 starts only after Y1 is picked, and the log says so.

13. **Section 8 (b) names no commands.** There is no Tools/night/n69-build.sh; the guard's heavy match is
    `n\d+-build|n\d+-reels|n\d+-smoke*.sh` (guard.cjs:239). If GG frees the disk to 12 GB or more (DECIDE69 invites it), a build that is
    "tried once" really runs inside the 3:00-4:00 close, with no budget. Fix: name the three commands, and run them only if 45 min or more
    remain; otherwise write "not run (time)".

14. **room-chain.sh details.** Its done marker always writes 0 (line 30), but its header (line 7) promises the exit code. It sets no
    `MSBUILDDISABLENODEREUSE=1` before `dotnet build` (line 14), unlike n69-gates.sh:13. A failed round cannot re-run under the same N
    (n69-round.sh:29 refuses an existing folder). Fix: export the variable, `echo $rc`, and document "re-run = a new N".

15. **Deadlines differ between the brief and the prompt.** The builder brief says "one fix round by 1:45" and "last commit and report by
    2:00" (builder-brief.md:8, :33). The prompt says "files committed by 1:50" (NIGHT69.md:85). Fix: one time (1:50) in both.
    Also, the cards' CHANGE lines arrive at 0:35, only 10 min before the first files are due at 0:45.

16. **The prompt breaks prompt-methods.md rule 5 (line 90).** The rule wants the end condition, schedule and at most 8 rules in the first
    25 lines. The schedule is at line 78, rule 8 ends at line 26, and the average line is 132 characters (15,245 bytes / 115 lines), about
    twice a 150-line budget. Rule 3 (prompt-methods.md:85: resume at CAP - 0:30 means minimum close) is not carried over. Nothing covers a
    stop that returns between 2:20 and 3:00, or at 3:00-3:30. Fix: move a five-line schedule summary into section 0, and add "a return
    after 3:30 means section 7; a return before 3:30 means section 8 from the files".

17. **No "nothing running" rule after a stop.** The minimum close needs `running` to PASS (close-check.cjs:182). Any harness dotnet started
    since t0, a builder's worktree included (close-check.cjs:208), fails it. Section 7 gives no step for a job still running or hung when
    the session returns. Fix: step (1b): list the processes (rule 6's command line plus marker check) and report them; never kill them
    unasked. MINIMUM then fails honestly.

## Top 5
1. Y2 lands at 1:00, but a1 (0:50-1:22, no picks) and the 1:15 SimHarness freeze block it: move Y2 to 0:45, before a1.
2. Nothing copies Logs/n68/red-<L> to Logs/n69/red-<L>, which the GATE pack and table read: every pack exits 3 (MISSING red).
3. The GATE pack runs at the round's end, before the level review it requires; nobody re-runs it after the review.
4. One review slot holds about 10 reviews, and a2 (1:50-2:22) cannot be reviewed or critiqued before the 2:20 stop: only a1 can PASS.
5. Section 7 quotes a guard refusal that was never printed, and needs ROW lines before any step prints them (add `--pre`).
