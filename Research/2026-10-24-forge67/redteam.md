# FORGE67 red team (2026-10-24 ~07:00-07:45, read-only; tree b3223328 + the forge's untracked NIGHT67.md / DECIDE67.md). Most damaging first.

Each finding: the risk, the evidence (file:line), the damage, the fix (the text to add or change).

**Read in full:** NIGHT67.md, DECIDE67.md, Research/2026-10-24-forge67/ (DECISIONS.md, slate-seed.json, builder-brief.md, GOAL.txt),
forge67-make-n67.cjs and the n67 tools it wrote (close-check, round, round-verbs, red, gate-pack .sh / .cjs, table, gates, clock, reels,
slate, slate-set), n65-verbs.sh. Also read: NIGHT66-REPORT.md, NIGHT66-LOG.md, Logs/n66/verdict-T1b-r3.md and verdict-T1c-r2.md (the
others by heading), Logs/n66/r2/review-level.md (headings and BLOCKING lines), LESSONS L-003 / L-150, HANDOFF.md, CLAUDE.md, the forge66 red
team's format, Research/2026-10-23-wave66/S.md (rounds 2-3), .claude/agents/skygrind-critic.md, the guard's heavy / disk / clock rules
(.claude/hooks/guard.cjs:237-252), the harness's skip v2 options (Harness.N65.Verb.cs:824-866) and d25ea7d8's diff.

**The three worktrees, read only:** status, branches, and builder T's untracked files under
`.claude/worktrees/agent-af48e888c59fa3626/Research/2026-10-23-wave66/T/`.

**Ran:**
- git reads.
- `df -BG /c`: 14 GB free at 07:40.
- The commit headroom (Win32_OperatingSystem) and the Unity processes (Get-Process).
- A seed-line diff of Logs/n66/r2/verbs-skip.txt against T's re-run.
- One scratch script outside the repo (scratchpad/rt67/mincore.cjs): a planted minimum close in a temp git repo (deleted after), run
  through the repo's n67-close-check.cjs with --root.

No dotnet, build, Editor or harness. Grounding: the repo only. Google Drive, Gmail and Linear were not searched: this game's material
lives in the repo, and FORGE66 found those sessions expired.

---

1. **a2 is not "one guard from a GATE": builder T's own after-runs, on disk since 03:13, show the charge rule leaves its stall out of
   band and raises its bar to within 0.02 of its ratio.**
   - **Risk:** the night's headline is a2 and a4 through their GATEs by about 2:40 (NIGHT67.md:15-16 and :25; DECISIONS D-3, D-5). The
     premise is that d25ea7d8's charge rule fixes a2's stall guard ("fixed after the close"). T ran that before / after at NIGHT66's end.
     The files are untracked in T's tree and unjudged, and the plan does not use them.
   - **Evidence** (T's folder above, all on the amended pilot):
     - a2 stall V on r2's level: 33.1 % -> **29.1 %** (t1b-amend-after-r2-flowV-a2.txt:10).
     - NIGHT65's a2 (red65) on the same pilot: 23.9 -> **24.1 %** (after-red65-flowV-a2.txt:10). The shipped red is 12.4 %
       (Logs/n66/red/a2/flow-V.txt). So a2 is still worse than both reds.
     - a2's red65 skip on the amended pilot: `M1 PASS | room a2, ratio 1.27` (after-red65-skip-a2.txt:56). The slate's formula
       max(1.25, red65 + 0.20) then gives **mainBar 1.47**. a2 reads `ratio 1.49` (after-r2-skip-a2.txt:56): a 0.02 margin.
     - a8 reads M1 UNSURE 0.97, 5 / 10 exercised (after-r2-skip-a8.txt). Its red65 stall V moves **37.5 -> 48.6 %**
       (after-red65-flowV-a8.txt:10).
     - These ran on a scratch DLL built from T's dirty tree (the headers say "tree 9-17 dirty paths"). Main's code is the same: `git diff
       2fa9c5a6 HEAD -- Tools/SimHarness` is empty. The same numbers are expected on main (not checked).
   - **Damage:**
     - r1 (the carry-over) cannot hold a2: its stall guard fails. R must change a2, so the change reaches r2 at the earliest, and a2's
       second held round is r3 (2:40, done about 3:05).
     - The plan's a2 GATE "from about 2:10" (NIGHT67.md:101) cannot happen. The slate's a2 title "(M1 PASS 1.52 ... the stall guard to
       fix)" sends R to fix a guard the tool already re-measured.
   - **Fix:**
     - NIGHT67.md "Where we are" a2 line, replaced with: "a2: on the amended pilot (T's unjudged after-runs, T tree
       Research/2026-10-23-wave66/T/t1b-amend-after-*.txt): M1 1.49 against a bar that becomes 1.47 (red65 1.27); stall V 29.1 % against
       the reds' 12.4 / 24.1: still a GUARD FAIL; a2 needs a room change (r2 and r3 at the earliest)."
     - Target line, replaced with: "a4 first (r1 + r2, GATE about 2:30-3:00), then a2 (r2 + r3 at the earliest)."
     - T0's first step: re-run T's eight after files on main's DLL and diff the seed / SUMMARY / M1 lines against T's. Identical means
       d25ea7d8 is the code T measured. Commit T's files as the "after" evidence (finding 16).

2. **The clock cannot carry two GATEs by 2:40. There is one review slot, two reviewers are booked at once at ~1:05, and the "two held
   rounds" rule only repeats an unchanged room's numbers.**
   - **Risk:** everything that reads evidence goes through one review slot (NIGHT67.md:39). By 2:40 the slot must run, in series: the
     r1 level review, T0's critic, the r2 level review, and each GATE's spotter and critic. At ~20 min each (the critic's own budget,
     skygrind-critic.md:36), that is ~140 min in the 95 min between r1's end (~1:05) and 2:40.
   - **Evidence:**
     - Section 1 step 6 starts T0's critic at ~1:10. Step 7 starts the r1 review "right after r1" (~1:05). With R and S that is four
       agents against rule 5's three.
     - A round takes 1324-1354 s beside builders (NIGHT66-REPORT.md:62), now with seven slots (b1 and b2 are measured every round:
       n67-round.sh:21 always overwrites ROOMS).
     - The sim is deterministic. T's re-run of NIGHT66 r2's a2 skip, in another tree, gives the same 51 seed and verdict lines (my diff:
       IDENTICAL). So an unchanged room on an unchanged tool is expected to read the same in its second round (the other rooms change
       around it; not checked): the hour between r1 and r2 buys no evidence for a4.
     - A room that does change between its rounds (builder-brief.md:25-26, rule 4) is a new design measured once.
   - **Damage:** a4's verdict lands about 3:00-3:40 and a2's about 4:00-4:30, both inside the likely stop (finding 3). The night repeats
     NIGHT66: no GATE judged.
   - **Fix:**
     - ROOM list line 2 (slate-seed.json:5), replaced with: "a GATE needs M1-M4 and every GUARD held on one main-tree round on the
       T0-passed tool, with skip v2 also on seeds 11-20 (`--seed 11 --seeds 10`, M1 >= the bar on both sets, >= 7 / 10 exercised on
       each); the room file unchanged since that round". This is not a loosening: 20 distinct seeds instead of the same 10 twice.
     - The second seed set runs in the GATE pack, which already runs a fresh skip (n67-gate-pack.cjs:143); add `--seed 11` (skip v2
       already reads it: Harness.N65.Verb.cs:835).
     - The GATE critic also writes that room's level review for the round (one agent, not two).
     - Order the slot: T0's critic first (it gates M1), then the r1 review.
     - If the forge keeps two rounds, say that the second round of an unchanged file is a repeat, and drop rule 4 for a room between
       its two rounds (finding 11).

3. **The usage-limit model is wrong: NIGHT66 shows a 5-hour window gone in 2:20 of team work, so the stop is not "likely near 5:00".**
   - **Risk:** NIGHT67.md:51 and builder-brief.md:5-6 plan for one stop near 5:00 and put every GATE "by 4:30" (DECISIONS D-1).
   - **Evidence:**
     - NIGHT66-LOG.md:50-54 and :260: t0 22:26; stopped 22:46 ("resets 12:40am"); resumed 00:41; stopped again 03:01 ("resets 5:40am").
       The 00:40 window lasted 2:20 with three builders, one or two critics and the lead.
     - The stop's clock time depends on where t0 falls in the window. NIGHT66 ran in FORGE66's own session (NIGHT66-LOG.md:4-5) and
       stopped at 0:20.
     - If the windows run back to back, the current one opened ~05:40 and closes ~10:40 (inferred, not checked). NIGHT66's close and
       FORGE67 have already used it.
   - **Damage:** a night started this morning can stop inside T0 (0:20-1:30), the way NIGHT66 did. One started right after a reset
     stops around 2:30-3:30, which is exactly the GATE window.
   - **Fix:**
     - Rule 11, replaced with: "A window lasted 2:20 under NIGHT66's load (L-003). Expect the first stop 2:30-3:30 after the window's
       start, or earlier if t0 falls mid-window. If this session shows a limit message, log its 'resets' time at the bearings."
     - Order the work so each step ends in a committed file before the next: T0's reds, r1, a4's GATE pack.
     - Add to DECIDE67's first block: "start NIGHT67 right after a reset, not in the window FORGE67 used."

4. **Section 1's step 3 runs "every session", so a resume after a stop overwrites the night's slate with the seed.**
   - **Risk:** NIGHT67.md:55 says "steps 1-3 every session", and rule 11 (:51-52) repeats "section 1's steps 1-3" after a stop. Step 3
     (:61-63) is `cp Research/2026-10-24-forge67/slate-seed.json Logs/n67/slate.json && cp -r Logs/n66/red Logs/n66/red65 Logs/n67/`,
     then the flow base in the background.
   - **Evidence:**
     - A usage stop halts the model, not its background shells, so a round started before the stop keeps running. This is how
       background tasks work; NIGHT66's log does not record it.
     - The flow base is a `dotnet run` build (n67-gates.sh:19). A second dotnet job beside a round failed with MSB3021 on NIGHT66
       (NIGHT66-LOG.md:197-199).
   - **Damage:** after the first stop, the copy loses everything the lead wrote since the last commit: T0's state, the rewritten
     mainBars, PASS fields, SHORT reasons. A GATE then reads M1 against the seed's 1.25. The red folders rebuilt by T0 (finding 6) are
     overwritten with the stale copies, and a still-running round gets a second dotnet job beside it.
   - **Fix:**
     - Change :55 to "(steps 1-2 every session; 3-7 the first session only)".
     - Make step 3 refuse to clobber: `[ -e Logs/n67/slate.json ] || cp ...; [ -d Logs/n67/red ] || cp -r Logs/n66/red Logs/n67/;
       [ -d Logs/n67/red65 ] || cp -r Logs/n66/red65 Logs/n67/`, and start the flow base only when Logs/n67/flow-base.txt is missing.
     - Rule 11's resume becomes: "steps 1-2, `git log -1 -- Logs/n67/slate.json`, the slate printed, the background jobs listed (a
       round still running is waited for)".

5. **T0 cannot be finished in 0:05-1:30 as written, and it has no plan for a FAIL.**
   - **Risk:** T0's doneWhen (slate-seed.json:19-24) and section 1 step 6 (NIGHT67.md:70-76) need new harness code from T, a planted
     hang, and lines no tool prints.
   - **Evidence:**
     - (a) **The planted hang.** n65-verbs.sh:12 hard-codes `timeout 900`, so the hang costs 15 min of the main tree's only dotnet
       slot. No `--plant-hang` exists: T must write one.
     - (b) **The planted charge** ("two bodies 1.5 m off the line read no charge") also needs new code.
     - (c) **The pick.** T has 35 min including the brief's ~10 min of reading (builder-brief.md:14). Its code must be picked before r1
       (no pick during a round), or r1 ran a different harness from the one T0 judges.
     - (d) **Line 1 asks for prints that do not exist.** It wants n46flow --verbs65's "charges per seed and bodies per charge". n46flow
       prints only totals ("trigger held ... charged shots 35", after-r2-flowV-a2.txt:20); only skip v2 prints bodies per KESTREL charged
       shot (Harness.N65.Verb.cs:878).
     - (e) **Line 1's "charges only when the shot's ray hits >= 2 bodies" reads false on its own output.** The after files list 1-body
       and 0-body charges on every seed (e.g. "charged 4, bodies hit per KESTREL charged shot 2/1/1/1"). The trigger tests the ray at the
       trigger, bodies move during the 2 s charge, and the inner autopilot charges too ("the autopilot's own charges, one body or none",
       Harness.N65.Verb.cs:872-873).
     - (f) **No FAIL branch.** If T0 fails with a code change, r1 and r2 are void. The first countable round is then r3, and with the
       two-round GATE no room GATEs before ~4:45.
   - **Damage:** a literal critic finds line 1 NOT MET on (d) and (e), and T0 FAILs on wording. The plan's whole sequence slips past
     the stop.
   - **Fix:**
     - (i) **The timeout:** n65-verbs.sh:12 becomes `timeout "${N65_VERBS_TIMEOUT:-900}"`. The planted hang runs at 60 s, e.g.
       `N65_VERBS_TIMEOUT=60 bash Tools/night/n65-verbs.sh skip <level> a8 --seeds 200`. That needs no new harness code: the run outlasts
       the timeout and its seed lines are kept.
     - (ii) **Where the proofs run:** T runs its proofs in its own tree (one dotnet job per tree). The lead runs only r1 and the reds in
       main.
     - (iii) **Line 1, re-worded before any measurement:** "skip v2 before / after on Logs/n66/r2/n65-foundry.json for a2 and a8 (the
       seed lines: charged shots and bodies per KESTREL charged shot); n46flow --verbs65 the stall %, 'trigger held' and 'charged shots'
       lines; the trigger rule by reading VerbPilot.cs:206-221; planted: two bodies 1.5 m off the ray, no trigger."
     - (iv) **A T0-FAIL branch,** added to step 6: "a FAIL that changes only Verb.cs / VerbPilot.cs re-runs the skip and flow V on r1's
       saved level (`LEVEL_FILE=Logs/n67/r1/n65-foundry.json bash Tools/night/n67-round-verbs.sh r1b`, flow V by the DLL into
       Logs/n67/r1b/<id>/), and r1's other guard files stand (flow A / B, the room check, levelgen and air65 never run the verb
       pilot)".

6. **The reds copied from Logs/n66 hold three tool generations, and the table, the GATE pack and the stall guard read the oldest.**
   - **Risk:** step 3 copies Logs/n66/red65 whole. Every GUARD and the pack compare against Logs/n67/red / red65 (slate-seed.json:10;
     n67-gate-pack.cjs:115-118 and :202; n67-round.sh:47). T0 re-measures only a2 / a8's skip, into a folder nothing reads
     (Logs/n67/red65-amend: NIGHT67.md:74).
   - **Evidence:**
     - Logs/n66/red65/verbs-skip.txt has the header `sha: 109d9dcd` (T1b before both fixes): a2 0.94, a3 0.72, a8 0.60. Fix 2's numbers
       (0.96 / 0.73 / 0.67) live only in Logs/n66/red65-fix2, and the amended a2 / a8 (1.27 / 0.93) only in T's tree (finding 1).
     - red65's flow-V for a2 / a8 (23.9 / 37.5 %) predates d25ea7d8. Amended, they read 24.1 / 48.6 %.
     - Logs/n66/red has no b2 folder, so b2's red reads "?".
   - **Damage:**
     - A critic sees a bar of 1.47 beside a red65 line of 0.94 that cannot produce it, and rules UNSURE.
     - a8's stall guard is judged against 37.5 % when its own tool's red is 48.6 %: a false GUARD FAIL.
     - a3's pack shows the ride-bug red.
     - L-150 ("re-measure the reds and bars on the PASSed tool") is not met.
   - **Fix:** step 6 rebuilds the reds' verb lines on the t0 tool before r1, so r1's M1 lines print the final bars:
     - Rename the copy `Logs/n67/red65/verbs-skip.txt` -> `verbs-skip-n66-109d9dcd.txt`.
     - Run `{ echo "# command: bash Tools/night/n65-verbs.sh skip Logs/n67/red65/n65-foundry.json a2,a3,a4,a5,b1,a8 --seeds 10 --bars
       Logs/n67/slate.json"; echo "# sha: $(git rev-parse --short HEAD); $(date '+%F %T')"; bash Tools/night/n65-verbs.sh skip
       Logs/n67/red65/n65-foundry.json a2,a3,a4,a5,b1,a8 --seeds 10 --bars Logs/n67/slate.json; } > Logs/n67/red65/verbs-skip.txt 2>&1`
       (~10 min).
     - Rename red65's a2 / a8 flow-V.txt the same way and re-run them with the DLL: `dotnet
       Temp/SimHarness/bin/Release/net10.0/SimHarness.dll n46flow --level n65-foundry --file Logs/n67/red65/n65-foundry.json --rooms <id>
       --seeds 10 --first 1 --verbs65`, with the same header.
     - Every bar is rewritten from the new file.

7. **The minimum close in section 7 cannot be tagged, so no state of the /goal can be reached.**
   - **Risk:**
     - `--tag --minimum` allows only the player and gates lines to fail (n67-close-check.cjs:157).
     - The files line needs all four close files, docs/memory/CORE.md included, committed after `--start` (:92, :96).
     - Section 7 (NIGHT67.md:124-127) commits the report and HANDOFF only. CORE.md is touched only in the full close's harvest (:144).
   - **Evidence:** my planted case (scratch repo; the close started at 7:35; CORE.md last committed at 5:00) printed `FAIL files ... last
     committed before the close start: docs/memory/CORE.md` and `PRE-CHECK n67: FAIL files, player, gates`, with no tag. The same plant
     with CORE.md re-committed printed `PRE-CHECK n67: MINIMUM (player, gates missing after a stop)` and tagged. The selftest's minimum
     cases always commit all four files (n67-close-check.cjs:230-232), so they never see this.
   - **Damage:**
     - Without the tag, the final check prints FAIL and the /goal's FAIL exit (which needs the tag) is closed too: the judge loops.
     - The full close has the same trap one level down. A RED result or a memory hold (finding 15) fails `--pre` with no guard line to
       quote. The only way out is the after-8:00 `--tag --minimum` NIGHT66 used (NIGHT66-LOG.md:290-294), and NIGHT67.md never names it.
   - **Fix:**
     - Section 7 steps (3)-(5) become: "(3) NIGHT67-REPORT.md with sections 2 and 8; (4) HANDOFF.md and docs/memory/CORE.md's state line;
       (5) one commit of NIGHT67-REPORT.md, NIGHT67-LOG.md, HANDOFF.md, docs/memory/CORE.md (the close-check's files line needs each one
       committed after --start)."
     - Add to section 9 (j2): "when --pre fails only on player / gates for a reason the guard did not print (a verbs37 RED that is a
       result, a commit-free hold), log it, wait to 8:00, then `--tag --minimum`, the clock stop, the check (MINIMUM PASS)".
     - Add a selftest case "minimum close without CORE.md -> FAIL files".

8. **GOAL.txt: a correct close meets it, but its working-rules paragraph invites the drift NIGHT66's judge showed, and NIGHT67.md never
   names the FAIL exit.**
   - **Risk:**
     - The conditions are right: a full PASS or MINIMUM PASS, then exactly `NIGHT67 COMPLETE`.
     - But about two thirds of GOAL.txt is "working rules" ("T0 ... judged before any room's M1 counts", "every output a verdict cites
       is saved under Logs/n67/", "the clock stop at 7:30 or later"): the same shape NIGHT66's judge looped on (DECISIONS D-8).
     - "while the tag u3-n67 exists" is not something a transcript judge can see, except through the check's own tag line.
     - NIGHT67.md:9-10 and section 9 (m) know only `NIGHT67 COMPLETE`. A lead whose final check FAILs after the tag will keep fixing,
       commit, and fail `tag` forever ("a tag is never moved").
   - **Fix:**
     - GOAL.txt becomes: "/goal Met when the latest output of `node Tools/night/n67-close-check.cjs` (no arguments) in this conversation
       ends in the line `CLOSE-CHECK n67: PASS` or `CLOSE-CHECK n67: MINIMUM PASS` and the last assistant text after it is exactly
       `NIGHT67 COMPLETE` (tool calls earlier in that turn are fine); or, when that output ends in a line starting `CLOSE-CHECK n67:
       FAIL` and its `tag` line reads `u3-n67 -> <sha>`, the last assistant text is exactly `NIGHT67 CLOSED: ` followed by that FAIL
       line. The script's last line is the whole verdict: do not re-judge its other lines, the night's work or the order of steps. Work by
       NIGHT67.md (read it in full first and after any compaction or resume); its rules are working rules, not conditions of done."
     - Add to section 9 (m): "If the final check FAILs, fix only what needs no commit (a stray process by its PID, the clock) and run it
       again; a FAIL that needs a commit after the tag is final: the last message is `NIGHT67 CLOSED: <the FAIL line>`."
     - Before (l): "no background task or agent pending (a late task notification after the final message ends the turn on other
       text)".

9. **The GATE pack is one round's evidence and misses half of the ROOM list.**
   - **Risk:** a GATE critic sees "evidence, not the story" (rule 9), but the pack (n67-gate-pack.cjs:9-23) does not cover the GATE rule.
   - **Evidence:**
     - It takes one `<round-tag>` (:33): no second round.
     - It copies `table.md` (:79) but not `table-red65.md`.
     - It has no M4 file: `deco-<id>.txt` is written to the round folder (NIGHT67.md:98) and not packed.
     - It has no levelgen report and no spawn check against e37083cf (room-diff.txt compares to n52's export, :191-196).
     - a3's M3, "`n65verb ride --shoot`" (slate-seed.json:8), is run by no n67 tool. n67-round-verbs.sh:22-27 runs pierce, launch,
       parry and skip; a3's ride.txt is n53ridekill (n67-red.sh:38).
     - open-findings.md reads only Logs/n67/r*/review-*.md (:218). NIGHT66's open BLOCKING lines (Logs/n66/r2/review-level.md:87,
       :142-143, :208-209, :252-254, :273-274) are invisible to it, and so is a review saved under any other name.
     - Its fresh skip is `dotnet run` under spawnSync with no timeout (:143): the a8 hang can hold the lead's job forever.
   - **Damage:** the critic must FAIL or UNSURE the rooms it cannot see M4, levelgen or the second round for. a3 cannot GATE at all, and
     an a8 pack can hang the main tree.
   - **Fix:**
     - Name each round's review `Logs/n67/r<n>/review-level.md`, in NIGHT66's format (`## <id>` headings, BLOCKING lines).
     - Copy NIGHT66's r2 review to `Logs/n67/r0n66/review-level.md` (the pack's regex reads it).
     - Hand the critic, beside the pack: the round's table-red65.md, deco-<id>.txt, levelgen-report.md, and `git diff e37083cf
       <passSha> -- Design/levels/n65/rooms/<id>.json`.
     - Add `bash Tools/night/n65-verbs.sh ride <the round's level> a3 --shoot` to n67-round-verbs.sh.
     - Give the pack's spawnSync `timeout: 900000` (or run the DLL through n65-verbs.sh).

10. **"No worse than both reds" and "within the reds' band" are two different GUARD rules in one line, and they decide a4 and a3.**
    - **Risk:** slate-seed.json:10 says "GUARDS, no worse than both reds" and then "stall ... within the reds' band".
    - **Evidence:**
      - a4's round-2 stall V is 30.7 % against the red's 31.2 and red65's 25.5 (S.md:133). That is inside the band but worse than
        red65.
      - a3's reds' deaths A / B are 6 / 2 (shipped) and 0 / 0 (red65) (Logs/n66/red/a3, red65/a3 flow-A / B).
      - a8's stall red is 42.3 (shipped) and 48.6 or 37.5 (red65, amended or not).
    - **Damage:** the same numbers PASS or FAIL depending on the critic's reading: a4's GATE is a coin toss.
    - **Fix:** write each room's ceilings into the slate before t0, as numbers beside its mainBar (`stallVMax`, `deathsAMax`,
      `deathsBMax`), taken from the re-made reds (finding 6) by one stated rule. ROOM line 10 becomes: "GUARDS: clears 10 / 10; deaths A /
      B and stall V at or under the room's slate ceilings (the higher of the two reds, measured on the T0 tool)."

11. **The builder brief's commands repeat the bug d25ea7d8 fixed, and two of its rules contradict the GATE.**
    - **Evidence and risk:**
      - **The skip command.** builder-brief.md:27-30 runs skip v2 as `dotnet run --project Tools/SimHarness ...` "under `timeout 900`".
        d25ea7d8 exists because `dotnet run`'s child outlives a killed host (n65-verbs.sh:10-11; S killed four hung PIDs,
        NIGHT66-LOG.md:250-251). An idle orphan `...\Temp\SimHarness\...` started since t0 fails the close-check's `running` line
        (n67-close-check.cjs:194-202).
      - **Rule 4 against the GATE.** Rule 4 ("every round changes the room ... add or move a collider piece", :25-26) contradicts a GATE
        candidate's hold between its rounds. NIGHT67.md rule 10 (:49) makes it a corner never to repeat. R's slice says "change the room
        only if it is still out of band" (:42-43).
      - **The verb plans.** Rule 2 says "never commit ... the plan" (:22-23), while ROOM line 1 wants the verb plan updated in the same
        commit as a lever change. Main's plans already miss the carry-over levers: `grep -ci spill plans/a5.md` = 0 and `grep -ci "slag
        run" plans/a3.md` = 0. The lead's pick takes only the room file (NIGHT67.md:96), and the pack reads the plan at the sha
        (n67-gate-pack.cjs:184).
      - **No stop time.** The brief gives builders none.
    - **Damage:**
      - Orphans fail the close.
      - A builder either edits a holding room or breaks rule 4.
      - a3 and a5 fail ROOM line 1 by their plans.
      - Builders still running at 7:00 fail `running`.
    - **Fix:**
      - Rule 5's command becomes `bash Tools/night/n65-verbs.sh skip Design/levels/n65-foundry.json <id> --seeds 10 --bars
        C:/Users/gaige/dev/SKYGRIND-unity/Logs/n67/slate.json`, and `ROOMS=<id> bash Tools/night/n67-red.sh 2 n65-foundry
        Research/2026-10-24-wave67/<id>/r<n>` for the guards (the round's own flags; 2 jobs, since three trees share one CPU).
      - Rule 4 gains: "except a room the lead has named a GATE candidate: it is frozen until its verdict".
      - Rule 2 becomes: "never commit the generated level files (Design/levels/n65-foundry.json, Design/levels/n65/foundry.plan.json,
        foundry.assembly.json, openings65.json) or slots65.json; commit your verb plan with every lever change".
      - The pick becomes `git checkout <sha> -- Design/levels/n65/rooms/<id>.json Research/2026-10-23-wave66/plans/<id>.md`.
      - R's first round updates plans/a3.md and a5.md.
      - Add: "6:45: commit, report, and end your turn; run no job after it".

12. **a3's carry-over is built on the gamed spawn.**
    - **Risk:** main's a3 still has NIGHT66's moved spawn, `"spawn": { "at": [2.5, 1.45, -61.8] }` (Design/levels/n65/rooms/a3.json:14).
      THE SLAG CRUST's why (:73) depends on it: "the walk pilot falling off its spawn on the ramp's top into the slag". DECISIONS D-7 (a)
      says "put back in r1", but r1 measures the carry-over with no pick.
    - **Damage:** r1's a3 numbers measure the start L-150 forbids. Moving the spawn back to [0, 0, -59.5] removes THE SLAG RUN's
      reason, and its deaths (4 / 8 without the crust) are unmeasured from the door.
    - **Fix:** say in step 6 that "r1's a3 lines are not countable (spawn moved)". R's a3 round 1 moves the spawn back first and
      re-measures flow A / B deaths before keeping THE SLAG RUN / CRUST.

13. **Branch C's play build would ship every latest room, passed or not.**
    - **Risk:** NIGHT67.md:106-107 passes "the GATE round's level" to n65-play-build.sh. That level is the round's `--slots=latest`
      assembly (n67-round.sh:33): every slot's newest file, a3 / a5 / a8 / b1 included. The PC slice promises "every PASSed room against
      the old level" (slate-seed.json:52-55).
    - **Damage:** GG plays unjudged rooms labelled as passed.
    - **Fix:** after the slot sets, `node Tools/night/gen-n65-foundry.cjs --run --out=Logs/n67/play-passed` (slots65.json's set: passed
      rooms new, the rest old), then `bash Tools/night/n65-play-build.sh n66-freeze n67-play-passed Logs/n67/play-passed/n65-foundry.json
      --config Design/unity-config.json --add-level n65.foundry65`.

14. **Reviewers and critics are not told to keep dotnet out of the main tree.**
    - **Risk:** skygrind-critic.md:35-36 forbids the Editor, builds, Players, reels and the suite, but not the SimHarness, and asks the
      critic to "re-run at least one meter yourself". NIGHT66's lead had to tell T1b's critic not to run dotnet in main
      (NIGHT66-LOG.md:90). NIGHT67.md never says it.
    - **Damage:** a critic's `dotnet run` during a round is NIGHT66's 4:01 mistake (a second dotnet job, MSB3021), and it breaks T0's
      clean-round line.
    - **Fix:** rule 6 gains: "every reviewer, spotter and critic prompt says: no dotnet, harness, build, Player or Editor in any tree;
      read-only node, python -I and git only".

15. **Memory: two stray Unity processes hold 13.6 GB, and rule 5's 15 GB hold can block the close with no line the close-check accepts.**
    - **Evidence:**
      - Commit free was 17.4 of 97.6 GB at 07:40.
      - Unity PIDs 49588 and 51936 have run since 2026-10-22 23:24 with 6.8 GB each; NIGHT66 saw them too (NIGHT66-LOG.md:98-100). 21008
        is the Editor.
      - Rule 5 (:40) holds every build, reel set, smoke and gate run under 15 GB. NIGHT66 read 5.4 GB mid-gate.
    - **Damage:** a hold at the close fails player / gates without the guard's words (finding 7's trap).
    - **Fix:**
      - Add to DECIDE67's first block: "two Unity processes (PIDs 49588, 51936, started 10/22 23:24, 6.8 GB each) are not the Editor;
        closing them is yours".
      - Section 9: "stop the builders by 6:45 so the close has the headroom; a hold that does not clear by 7:45 takes the after-8:00
        minimum route (finding 7)".

16. **NIGHT66's last evidence lives only as untracked files in the worktrees, and HANDOFF calls the trees "clean".**
    - **Evidence:** HANDOFF.md:29-31 says "All three trees are clean". `git status` shows untracked, never-committed evidence:
      - R: Research/2026-10-23-wave66/r4/ (r4a, r4b: a3 / a5 round 4).
      - S: a4/r3/ (trace-seed13.txt, A-nopad, V1, V2) and a8/r3/.
      - T: the 16 t1b-amend-* files and t1b-amend.sh.
      - Main's S.md and the carry-over whys cite them, and none is tracked at HEAD.
    - **Damage:** a critic cannot find the evidence for a4's pickup or THE SLAG CRUST, and T0's best before / after (finding 1) is one
      `git clean` away from gone.
    - **Fix:** step 4, after `switch -c`: each builder's first commit on n67-<L> adds its folder with explicit paths (R: r4/; S: a4/r3/,
      a8/r3/; T: T/t1b-amend*). The lead picks T's into main for T0. HANDOFF's line becomes "clean of tracked changes; untracked NIGHT66
      evidence listed".

17. **Smaller items.**
    - **T0 line 3:** "no commit ... (git log times)": say "on port/stage3". Builders' commits during r1 are in the same repository, and
      `git log --all` shows them.
    - **Output headers:** step 6's red65-amend output (:74) and the charge rule's "after" have no command / sha header (rule 7). NIGHT67.md
      gives no command or output path for the after at all: use T's t1b-amend.sh pattern into Logs/n67/t0-charge/.
    - **The rename:** forge67-make-n67.cjs renamed NIGHT66 citations in comments, so the n67 tools cite files that do not exist
      ("NIGHT67 T1c fix 2 (Logs/n67/verdict-T1c-r2.md)", n67-table.cjs:13; n67-deco.cjs:10). Tell T0's critic the evidence is under
      Logs/n66.
    - **ROOMS:** n67-round.sh:21 cannot be told ROOMS, so b1 (dropped) and b2 (a slot) cost every round a skip and four flows.
    - **The Fallback font:** CLAUDE.md rule 1's LiberationSans Fallback restore before the tag is not in section 9 (j). Read its diff
      first (L-047).
    - **Worktree removal:** DECIDE67's `git worktree remove --force` would lose any untracked work in an "older" worktree. `git worktree
      list` has 235 entries, many of them other sessions' scratch trees. Ask GG to read `git -C <path> status --short` first.

## Checked and fine (so the night does not spend time on them)
- **The worktree switch:** `git -C <wt> switch -c n67-<L> <t0>` works in all three trees. None has a tracked change, no n67-* branch
  exists, and none of the untracked paths is tracked at HEAD, so the switch keeps them. Main's a2 / a3 / a5 equal n66-R a64c5c3d, and a4 /
  a8 / b1 equal n66-S 29322705. Nothing on the builders' branches is lost.
- **The harness build:** n65-verbs.sh:7 builds the harness before running the DLL. Each round's gen `--run` (`dotnet run`,
  gen-n65-foundry.cjs:372) rebuilds it before n67-red.sh reads the DLL, so a round never mixes harness builds. The main tree's DLL
  (07:18:51) is newer than d25ea7d8.
- **The close-check:** it reads vn only with the row ON. A full correct close prints PASS, and the minimum close with CORE.md committed
  tags.
- **The guard:** it treats n41-build, n67-reels and n56-smoke as heavy (12 GB floor; n67-freeze is the close tag). n65-play-build.sh is
  not heavy.
- **The disk:** C: 14 GB, so branch C; the close's build is allowed at 12 GB or more.
- **The sim is deterministic:** an exact re-run reproduces the seed lines, so each red and round is reproducible evidence.
