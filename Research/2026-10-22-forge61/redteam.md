# NIGHT61 red-team review (a fresh reviewer, 2026-10-22)

Read in full: `NIGHT61.md`, `CLAUDE.md`. Opened only to check a NIGHT61 claim: `slate-seed.json`, `builder-brief.md`,
`.claude/settings.json`, `.claude/hooks/guard.cjs`, `.claude/agents/gg-proxy.md` and `skygrind-critic.md`, `Tools/night/` n61-slate.cjs,
n61-reels.sh, n61-prune.sh, n61-diskwatch.sh, n61-gates.sh, n60-freeze-rows.cjs, n61-freeze-rows.cjs, n46-build-rows.cjs, n41-build.sh,
n41-suite.sh, n56-smoke-steal.sh; a grep of `research-deadcode.md`; `Golden.cs:356`; `NightRows.cs:12,158`.

Ran (read-only):
- `node .claude/hooks/guard.cjs --selftest` printed `guard selftest: 72 / 72 as planted`.
- `node Tools/night/n61-slate.cjs Research/2026-10-22-forge61/slate-seed.json` printed `10 slices: 0 PASS ... 10 todo` and exited 0.
- `df -h /c` showed **11G available (100 %)**.
- `git tag -l u3-n60` prints the tag.
- `git check-ignore` says `Logs/` is ignored (`.gitignore:6`).
- `.claude/worktrees` holds **202** worktrees; the one measured is 191 MB.
- `Logs/guard-blocks.log` has 11 live blocks from today, so the hook is wired in the main tree.

Not checked: the Editor's state now (no Unity calls allowed); the /goal text (GOAL.txt is forge research, not read); whether the
hook fires inside builder worktrees; the Game suite's run time; plugin-provided agents.

## Findings

### A. Disk and the start state

1. **Section 0 rule 1 / 1.2**: "the guard hook refuses a build or a reel set under 12 GB free"; "Under 20 GB free: prune only tonight's
   own frames; if still under 12 GB ... do the non-Player slices".
   - Problem: C: has 11 GB free now (measured). At t0 tonight owns no frames, and `n61-prune.sh` refuses any tag that isn't `n61-*`.
     Nothing in NIGHT61 lets the night free older output: CLAUDE.md rule 10 and the guard forbid it, and the 202 old worktrees are GG's
     call (about 38 GB if all are like the one measured at 191 MB).
   - Overnight: no n61-a0 build, reel, SPOT, ON flip, freeze build, freeze reels or smoke. All seven visible slices die before A1, and
     report section (2) is empty.
   - Fix: before t0, GG frees at least 40 GB (the worktrees), or NIGHT61 names old output the night may prune (for example
     `Builds/n60-b2..b8`, `Logs/feel/n5*`) with a DECIDE line. Add a first bearings check: "under 25 GB at t0: log BLOCKED-DISK first".

2. **1.2**: "Start the watcher ... `bash Tools/night/n61-diskwatch.sh 15 8`. If it exits, read why, prune, restart it."
   - Problem: whenever free space is under 15 GB and nothing is left to prune (it is 11 GB now), the watcher exits on its first loop
     (`free < warn`, exit 3).
   - Overnight: exit, wake, prune nothing, restart, exit again within a second, all night, burning turns.
   - Fix: "restart with warn = free now minus 3 GB; if free is already under 15 GB, log Blocked: disk once and don't restart".

3. **1.2**: "do the non-Player slices (B1, B2, D1's auditing)".
   - Problem: B1's doneWhen needs `n60-focus-probe.sh`, a Player run that the guard's disk rule (`n\d+-focus-probe`) blocks under 12 GB.
     Every D1 batch needs "verbs37 x5 unchanged", also a Player.
   - Overnight: under the disk block only B2 and the auditor's list can finish. B1 and D1 end SHORT, or get marked done without
     their Player line.
   - Fix: "B1 up to its test RED/GREEN, B2, and the auditor's list; B1's probe and D1's deletions wait for disk".

4. **Section 6**: "Spawn builders one at a time with `df -h /c` between."
   - Problem: no threshold and no action. Each worktree costs about 0.19 GB at checkout, plus the dotnet bin/obj of CoreHarness and
     SimHarness from every `n61-gates.sh --no-editor` run.
   - Overnight: four builders and their harness builds take the drive from about 15 GB to under 12 GB during I1, and the guard
     refuses the window's build.
   - Fix: "spawn no builder under 20 GB free; log it".

5. **2 / 4.3**: "copy its before and after frames (same pose)"; "the lead opens one frame on and one off". The tools involved are
   `n61-reels.sh` and `n61-prune.sh`.
   - Problem: the prune loop thins every finished side during the set (every 20 s) and all sides at the end. It keeps only the PNG
     shots and every 50th jpg (`f*00` / `f*50`).
   - Overnight: a SPOT pair or meter frame at any other index (S5's "a mid-chain pair", S6's "a same-pose ride pair") is deleted
     before the lead copies it. The critic's "re-run at least one meter yourself" finds no frames.
   - Fix: SPOT and meter frames are PNG shots named in the slate, or the reel line passes a keep-list to the prune.

### B. The Editor, resume and the ending

6. **1.3**: "`bash Tools/unity-loop.sh status` shows "playMode": "stopped" (if it times out it is unfocused ...; never kill it)."
   - Problem: a timeout can also mean the Editor is still running a test batch. n41-suite.sh says "when the CLI's 600 s timeout fires,
     the Editor keeps running that batch", and NIGHT60's close ended on a MapChoiceTests TIMEOUT at 606 s. NIGHT61 has no branch for an
     Editor that stays unusable.
   - Overnight: the lead loops the focus recipe. Every full gate run fails at "editor compile", `n41-build.sh` refuses ("the Editor is
     not stopped"), and no build happens all night, the freeze build included.
   - Fix: define "wedged" as three status timeouts after the focus recipe (about 10 min). Then log Blocked: Editor, run `--no-editor`
     gates, keep the builders going, skip the Player steps, and run the close's non-Editor steps.

7. **1.4**: "`bash Tools/night/n61-gates.sh`: all GREEN is the baseline (if not, fixing it is task one)".
   - Problem: task one has no time box, and nothing separates a code failure from an environment one (stale `consoleErrors`, about
     which CLAUDE.md warns "the console tail keeps old lines", or a wedged Editor).
   - Overnight: hours spent "fixing" a baseline that isn't code.
   - Fix: "task one gets 30 min; then log Blocked with the failing gate line and continue with --no-editor".

8. **Section 3**: "the bugs ... **B2** Game M's hang (the lead)".
   - Problem: running Game M (MapChoiceTests) in the live Editor is the likeliest way to lose the Editor for the night. It has hung
     every night since NIGHT41 (slate B2), and the CLI timeout leaves the Editor still running it. B2 has no time box and no slot.
   - Overnight: the lead tries M at 2:30, the Editor stays busy, and I2 and I3 get no compile and no build.
   - Fix: B2 first reads the test and writes the cause with file:line. It runs M at most once, after I3's ON flips, and never before
     a build the night still needs.

9. **Section 9**: "the Game suite `bash Tools/night/n41-suite.sh n61 Game ABCDEFGHIJKLNOPQRSTUVWXYZ` (... a TIMEOUT is logged, never
   waited out)".
   - Problem: the tool does wait it out. Before every later batch, `drain()` polls `editor_status` up to 120 times, each poll a 45 s
     timeout plus a 30 s sleep. That is up to about 150 min, more than the "60 min" its own comment says.
   - Overnight: one TIMEOUT at 6:20 pushes the close past 7:00, and the tag and `NIGHT61 COMPLETE` never come.
   - Fix: `timeout 1500 bash Tools/night/n41-suite.sh ...` and log "suite cut at 25 min", or give the tool a no-drain flag.

10. **Section 9**: "**The close (6:00-7:00), in order:** ...".
    - Problem: the order has no failure path. A wedged Editor (no gates, no build), disk under 12 GB (the guard refuses the build and
      the reels), a smoke FAIL or a suite TIMEOUT stalls every later step: report, HANDOFF, harvest, tag, `NIGHT61 COMPLETE`.
    - Overnight: the run sits at close step 2 or 3, with no report and no tag.
    - Fix: "a close step that can't run is logged NOT RUN with its reason and the close continues; at 6:45 jump to slate, report,
      HANDOFF, commit, tag, NIGHT61 COMPLETE".

11. **4.4 and slate S5**: "fingerprints plain and --off61 EQUAL (or, for a sim row turned ON, a new baseline recorded once at the
    freeze with its reason: L-010)".
    - Problem: `n61-gates.sh` checks the plain fingerprint first and "exits 1 at the first failure". Once a sim row is ON, every later
      gate run stops there and never reaches `--off57..--off61` or the Editor compile. Recording the new baseline isn't a close step.
    - Overnight: every pick after S5 is integrated with no compile gate, and the close's first command (`n61-gates.sh`) is RED.
    - Fix: record the new plain baseline in the same commit as the sim row's ON flip, with its reason, or give the gates an
      expected-change flag that names the row.

12. **7 / 8**: "It starts no earlier than a printed `date` 6:00 after t0 and must finish by 7:00".
    - Problem: after a usage-limit stop (L-003; memory records one that stalled the lead 3.5 h) both clauses can be impossible.
      NIGHT61 doesn't say whether t0 moves or what gets dropped.
    - Overnight: resumed at 6:40, the lead either starts I3 or starts a 60-90 min close, and overruns the hard cap.
    - Fix: "the clock is wall time from t0. Resumed after 5:30: freeze at once. After 6:00: the close. After 7:00: the short close
      (slate, report with the gap, HANDOFF, commit, tag, COMPLETE)".

13. **Section 8**: "Any slice "in progress" at the cap becomes SHORT with what was measured."
    - Problem: "the cap" is undefined (the usage cap, or the 7 h cap). And `short` / `blocked` need no command line behind them.
    - Overnight: a slice in progress at a 2:00 usage stop is marked SHORT on resume with hours left, an easy way out of the work.
    - Fix: "in progress at the freeze (5:30) becomes SHORT". Every SHORT or BLOCKED cites the command line that shows it.

14. **Section 6**: "at most two critics alive at once"; the critic file's "Budget: 20 minutes".
    - Problem: nothing enforces the budget, and nothing says what to do with a critic that dies or hangs without a verdict.
    - Overnight: one hung critic holds a slot. Its slice stays in `state: critic`, and the close's "nothing left in progress" can't be
      met honestly.
    - Fix: "no verdict file 30 min after the spawn: stop it (TaskStop, then check its children by PID), mark it UNSURE, the row stays
      OFF; one re-spawn at most".

15. **Section 8**: "resume each builder by SendMessage".
    - Problem: NIGHT61 never says to log each subagent's id or name, worktree path, branch and slice at spawn.
    - Overnight: after a compaction the ids are gone. The lead can't SendMessage, so it spawns fresh copies (which section 6 forbids)
      or loses uncommitted builder work.
    - Fix: section 6 adds: "log every spawn in NIGHT61-LOG.md: agent id, role, worktree path, branch, slice".

16. **Section 1**: "Bearings (every session, including after a compaction or a resume)". Only step 4 is marked "First session only".
    - Problem: steps 5 (build n61-a0 plus a reel) and 6 (gg-proxy, 15 min) read as every session. Step 4's
      `cp ... slate-seed.json Logs/n61/slate.json` has no `-n`.
    - Overnight: each compaction costs about 25 min. A lead unsure whether this is "the first session" copies the seed over the slate
      and wipes every pass and state. A second `--flow-base` overwrites the baseline after rows have changed.
    - Fix: mark steps 5 and 6 first-session-only; use `cp -n`; run `--flow-base` only when `Logs/n61/flow-base.txt` is missing.

17. **NIGHT61 as a whole**: there is no stop condition.
    - Problem: n61-slate.cjs calls its last line "the totals the /goal reads", but NIGHT61 states no /goal and no time escape (memory
      says a /goal must stay satisfiable: "or 12 h"). Not checked: GOAL.txt.
    - Overnight: if the goal waits for `NIGHT61 COMPLETE` and the close can't tag (finding 10), the Stop hook keeps the run looping
      past 7:00.
    - Fix: write the goal line into NIGHT61, with "or 7 h 15 min after t0".

18. **Section 9**: "tag `u3-n61`; the final message `NIGHT61 COMPLETE`".
    - Problem: the guard blocks `git tag -f` / `-d`. If the tag was made before a usage stop and work continued, it can't be moved.
    - Overnight: the tag points at a pre-stop commit, and the L-107 check ("each ON row's enabled ... matched to the report")
      compares the report against the wrong tree.
    - Fix: "the tag is the very last git action; if `git tag -l u3-n61` already prints, log it and tag `u3-n61b`".

### C. Gaming (a number passes without a better game)

19. **Slate S2 doneWhen**: "kb3c / kb3d deaths and put-ats no worse than the Logs/n61/flow-base.txt lines". n61-gates.sh's header
    adds: "A level-data slice changes its own level's lines: re-make the base in that slice's commit".
    - Problem: S2 is the level-data slice, so the base it is judged against gets rebuilt from S2's own output.
    - Overnight: "no worse" is true by construction, and a level that kills more players ships.
    - Fix: in the bearings, copy the base to `Logs/n61/flow-base-t0.txt` (force-added). S2's doneWhen and its critic read that file.

20. **`n61-gates.sh --flow`** (section 4.4 "run `n61-gates.sh --flow`").
    - Problem: the n46flow run's output is discarded (`> /dev/null 2>&1`), its exit code isn't checked, and the old
      `Logs/n61/flow-off61.txt` isn't deleted first. `--flow-base` likewise exits 0 whatever happened.
    - Overnight: after S2's data change, n46flow throws on the level and writes nothing. The diff then compares the base with the
      previous run's file and prints "EQUAL to flow-base", stale but GREEN.
    - Fix: `rm -f` the output first, check the exit code, and require `grep -c SUMMARY` = 5 in both modes.

21. **Section 0 rule 5**: "`node Tools/night/n61-slate.cjs` refuses `passes: true` without a verdict file."
    - Problem: it refuses nothing. The lead edits the JSON with Edit, and the tool only exits 1 when someone runs it. Its test is
      `fs.existsSync(s.verdict)`, so any existing path passes (the slate itself, a builder's note), and it never reads PASS. The ON
      switch (`n60-freeze-rows.cjs`) never looks at the slate.
    - Overnight: a row goes ON with `verdict: "Logs/n61/slate.json"` and the slate prints clean.
    - Fix: the slate tool checks the verdict's first line is `<id>: PASS`, and the freeze-rows tool refuses any row whose slate entry
      isn't `state: pass` with such a verdict.

22. **Section 2**: "spawn a fresh `skygrind-critic` in mode SPOT with only those two paths".
    - Problem: skygrind-critic.md gives the critic Read, Grep, Glob, Bash and Write. It can read `Logs/n61/slate.json`, NIGHT61-LOG.md
      and `git log`, or hash A and B against `Logs/feel/<tag>/<side>/` (sides named `v1on` / `v1off`). The blind holds only if the
      critic chooses not to look.
    - Overnight: a SPOT critic that "checks context" names exactly the slate's `visible` change.
    - Fix: a separate SPOT agent with `tools: Read, Write` only, given neutral copies of the frames.

23. **Section 2**: "in a coin-flip order".
    - Problem: the flip isn't recorded before the answer, and a model's coin flip isn't random.
    - Overnight: B is always the "after", the order can be learned, and the blind can't be audited.
    - Fix: `echo $((RANDOM % 2))`, with the A/B mapping written to `Logs/n61/spot-map.txt` before the spawn.

24. **2 / 4.4 and the slate's SPOT lines**.
    - Problem: nothing limits SPOT runs per slice or fixes the pair in advance. Section 2 wants `visible` "with the frame", but S3 to
      S7 name no frame, and S5 and S6 say only "a mid-chain pair" and "a same-pose ride pair".
    - Overnight: the lead re-spawns SPOT, or picks a closer pose, until one answer names the change.
    - Fix: the seed names each SPOT pair (reel, side, shot) before any measurement. One SPOT per slice per build; the first answer
      counts; every SPOT file goes to the GATE critic.

25. **Section 2**: "'NOTHING OBVIOUS' or another difference means the slice stays OFF", together with 4.2's one build holding several
    slices.
    - Problem: when A and B differ by more than the slice's own row, SPOT names the neighbour's change. That happens when the before
      frame comes from n61-a0, or another slice's row is on in only one side. S1's kb3-clear frame and S7's kb3-entry frame share
      kiln's kb3.
    - Overnight: S1 fails on S7's presses, or passes on them.
    - Fix: A and B come from one build and differ only by the slice's own switch (`-skygrindNo<Row>61`, which builder-brief.md step 4
      already requires).

26. **4.4**: "never the builder's notes". Yet the critic reads "the diff (`git show <sha>`)" (skygrind-critic.md).
    - Problem: `git show` prints the builder's commit message, claims included. The builder's notes
      (`Research/2026-10-22-wave61/<letter>.md`, committed per the brief) land in the tree on cherry-pick, one Grep away.
    - Overnight: the critic reads "gain 1.8x in 5/5 rooms" in the commit message and anchors on it.
    - Fix: give the critic `git show --format= <sha>` (the diff only), and cherry-pick the notes commit after the GATE.

27. **Slate S4 red**: "the count of room light sources drawn with no visible mount (counted by the slice's meter before any change)".
    - Problem: the builder writes the meter that defines both red and "mounted". A scene-data count would also break L-093 ("a view
      proof measures pixels in the frame").
    - Overnight: red is inflated (every emissive counts) and the after is deflated (a 2 cm stub counts as a mount), so "<= 10 % of
      red" passes.
    - Fix: the lead or a critic counts the unmounted lights on the named a357 frames before the builder starts, and counts the after
      the same way.

28. **Slate S5**: "the shown rank climbs >= 2 ranks above DULL and the rank-up animates".
    - Problem: no GUARD keeps the meter meaningful.
    - Overnight: every kill jumps two ranks, or decay is switched off. The number passes and the meter means nothing.
    - Fix: add GUARDs: one kill stays at most 1 rank above DULL; 10 s idle decays to DULL; the top rank needs a chain of at least N.

29. **Slate S3**: "lines bad 0 of 5"; "SPOT on THE KILN WYRM's fight-start pair names the text".
    - Problem: "bad" is undefined, and any new text makes SPOT say "text".
    - Overnight: five vague lines ("Survive.") pass the visible rule.
    - Fix: "bad" means the line contradicts the rule the critic quotes from the boss code, or shows the wrong glyph. SPOT passes only
      if it names what the line tells you.

30. **Slate S7**: "a ram head, a piston and a hazard band (red-orange only on what hurts: CLAUDE.md rule 8)".
    - Problem: it's judged by eye, and the seed doesn't say whether kiln's presses hurt. If they don't, a red-orange band breaks rule
      8; if they do, the band is required.
    - Overnight: harmless pillars get a red-orange band, or a real hazard gets a non-hazard colour, and the critic guesses.
    - Fix: state from the level data whether the presses deal damage and what colour the band takes; the critic counts the three
      parts on each press.

31. **Section 2**: "Bug and dead-code slices are exempt". The lead owns B1 and B2.
    - Problem: NIGHT61 doesn't say whether bugs and D1 batches get a GATE critic, so the lead writes, integrates and judges its own
      fixes.
    - Overnight: the lead sets B1 to `pass` with a verdict file the lead wrote.
    - Fix: bugs and each D1 batch get a GATE critic reading the diff and the RED/GREEN lines. Otherwise they're reported as FIXED (with
      their commands) and never marked `pass`.

32. **Section 5 / slate D1**: "lines removed" per batch; "not needed by the `--off59` / `--off60` / `--off61` paths".
    - Problem: "lines removed" can be padded with comments, blank lines or whole old SimHarness commands (the `--off19` to `--off30`
      paths). And the criteria protect only `--off59..61`, while the same section's check runs "fingerprints plain / --off57..--off61".
    - Overnight: D deletes code only `--off57` / `--off58` use, the gate breaks, and a `git revert` wastes the batch. Or the report
      boasts thousands of comment lines.
    - Fix: count code lines only, protect `--off57..--off61`, and put old nights' harness commands on GG's list.

33. **1.6**: "It [gg-proxy] may add slices (append to the slate with `red`, `doneWhen`, `visible` written before any number)".
    - Problem: gg-proxy.md says "never edit any other file", and section 2 says "Only the lead edits it". So the lead writes the
      thresholds for slices it will build and judge. Nothing records that they came first: the slate isn't committed after the copy.
    - Overnight: a doneWhen is written after a quick look at the numbers, and nobody can tell.
    - Fix: gg-proxy proposes at most 2 entries in its own file. The lead pastes them verbatim and commits the slate (`git add -f`)
      before any measurement. n61-slate.cjs diffs every doneWhen against the first committed slate and prints LOOSENED.

34. **Slate S1**: "on vs -skygrindNoExit58".
    - Problem: the off side turns off the whole Exit58 feature, not just lamp60. So "the lamp's own gain" mixes in everything Exit58
      draws, and the GUARD's door mask is the meter's own.
    - Overnight: the ratio passes by brightening any Exit58 element inside the mask, not by making the door read as the exit.
    - Fix: the off side uses lamp60's own switch (or build-rows off), so A and B differ by the slice alone.

### D. Ambiguity and wrong or missing names

35. **4.4**: "turn the row ON with `node Tools/night/n60-freeze-rows.cjs "<night.row>=<reason>"`".
    - Problem: that script writes `"ON at NIGHT60 by the rule: ..."`. The forge's `Tools/night/n61-freeze-rows.cjs` (the same code
      with a NIGHT61 label, dated 08:59 today) exists, but NIGHT61 never names it.
    - Overnight: every row turned ON tonight, lamp60 included, reads as NIGHT60's. Section 9's L-107 match ("each ON row's enabled
      printed and matched to the report") then finds a mismatch or credits the wrong night.
    - Fix: name `n61-freeze-rows.cjs`.

36. **Section 5**: "The lead spawns the `dead-code-auditor` at A1".
    - Problem: no such agent exists. `.claude/agents/` holds only gg-proxy.md and skygrind-critic.md, and `~/.claude/agents/` is
      empty (plugin agents not checked).
    - Overnight: the spawn fails or the lead improvises a brief, and "proven dead" means whatever that improvised prompt says.
    - Fix: add `.claude/agents/dead-code-auditor.md`, or say "a general-purpose subagent with section 5 and research-deadcode.md as
      its brief".

37. **Section 5**: "IDE0051 / IDE0052 where `research-deadcode.md` says they can run".
    - Problem: research-deadcode.md reports "0 hits in 4 builds, including on a planted file with three dead members". The analyzers
      don't work here.
    - Overnight: time goes into running them, or "IDE0051: 0 hits" gets cited as evidence.
    - Fix: "IDE0051/52 report nothing here (research-deadcode.md); proof is by reference search only".

38. **Section 5**: "no references (grep the symbol across Assets/, Tools/, the scenes / prefabs / assets by the script's `.meta` guid,
    config strings, `-skygrind` flags, reflection ...)".
    - Problem: the list misses Unity's invisible callers, which have zero C# references by design: private
      `Awake` / `Update` / `OnValidate` / `OnGUI`, `[RuntimeInitializeOnLoadMethod]`, `[MenuItem]` / `[InitializeOnLoad]`, UnityEvent
      targets (`m_MethodName` in .unity / .prefab YAML), and `SendMessage("Name")`.
    - Overnight: D deletes one, it compiles, and behaviour silently disappears unless verbs37 or the smoke happens to cover it.
    - Fix: exclude Unity message methods and attribute-registered members, and grep the YAML for `m_MethodName` and string names.

39. **Section 3**: "A slice whose own MAIN misses twice stops ...: no third attempt at a SUBTLE change". Slate S1 reads "(lamp60 v3)".
    - Problem: NIGHT60 closed lamp60 "OFF after 2 attempts (blocked)" (commit 8e26d84a) as SUBTLE, so S1 is the third attempt.
    - Overnight: one lead stops S1 at once and V idles; another builds a third gain tweak.
    - Fix: say S1 is a new design rather than a gain change and that the two-miss count is per night, or drop S1.

40. **Section 3**: "own MAIN".
    - Problem: MAIN isn't defined in NIGHT61. Only the GATE file calls the doneWhen lines MAIN, and the seed tags only its GUARD lines.
    - Overnight: a SPOT failure or a verbs37 miss counts toward the two misses, or doesn't, as the lead chooses.
    - Fix: tag one doneWhen line per slice `MAIN:`.

41. **4.2**: "builds one Player for every slice picked in that window (L-038)".
    - Problem: this reads either as one Player per slice or as one Player for all of them.
    - Overnight: either 3-4 builds plus reels per window (time), or one build whose on and off sides differ by several rows (finding
      25).
    - Fix: "one build per window with every picked row ON; each slice's off side uses its own `-skygrindNo<Row>61`".

42. **Slate S2 row**: "level data behind kiln57 (ON) ...; ships by the rule like NIGHT60's B13-B15".
    - Problem: S2 has no switch, so section 4's "row ON in the build's copy", "reel on vs off in the same build" and the ON flip don't
      apply.
    - Overnight: S2's SPOT or GATE fails, but its level data is already in HEAD, with no instruction to undo it.
    - Fix: S2's before frame is n61-a0 at the same pose. A FAIL is undone with `git revert` of its commits, and the t0 flow base
      (finding 19) is restored.

43. **Section 2**: "`visible` (what a player sees in the first minute, with the frame)". "Why tonight is different" adds: "only 5 are
    visible in a level's first minute".
    - Problem: S3 (a boss at the level's end), S5 (a 5-kill chain) and S6 (top ride speed) aren't first-minute events.
    - Overnight: they fail the night's own visibility measure, or the term gets bent slice by slice.
    - Fix: "in the first minute of the moment it belongs to (a boss fight, a chain, a ride)".

44. **Section 3**: "then the beyond list: idle60's cut 2 ..., NIGHT60-PROPOSALS P3 ..., kill55's camera-jitter guard ..., a person on
    level 3's 4.5 m tiers".
    - Problem: these have no slate entries, so no red, doneWhen, visible or owner, yet section 7 says to take them when work runs out.
      "A person on level 3's 4.5 m tiers" is undefined (an NPC? a persona review? a bot walk?).
    - Overnight: beyond work is built and judged against thresholds written after the fact.
    - Fix: seed each as a slate entry, or require the lead to write and commit one before starting it.

45. **4.3**: "its meter with a planted case (L-093)".
    - Problem: no slice names its planted case.
    - Overnight: the lead improvises one or skips it, and the meter is never shown able to see the change.
    - Fix: add a `planted` field to each slice in the seed.

46. **4.4**: "A code slice of M size".
    - Problem: "M size" is undefined.
    - Overnight: a 400-line slice skips the diff review because it was sized S.
    - Fix: for example, "more than 150 changed lines or more than 3 files".

47. **Section 0 rule 3**: "never GG's four files or the four TMP fonts".
    - Problem: neither NIGHT61 nor CLAUDE.md lists the four files; CLAUDE.md names only UnityConnectSettings. Only guard.cjs lists
      them: UnityConnectSettings, EditorBuildSettings, UIEffectProjectSettings, Arena.unity. The guard sees git commands and
      Write / Edit, but not an Editor eval that saves a scene or an asset.
    - Overnight: a view slice's editor script saves Arena.unity over GG's uncommitted edit, and no hook fires.
    - Fix: list all eight paths in section 0 and add "no editor eval saves Arena.unity or a font asset".

48. **Section 0**: "these five broke most, and four are now hooks".
    - Problem: only rules 1-3 are hooks. Rule 4 says "not a hook", and rule 5 rests on n61-slate.cjs, a checker run by hand.
    - Overnight: the lead trusts an evidence gate that doesn't exist (finding 21).
    - Fix: "three are hooks".

49. **Section 6**: "At most four builders (L, V, G, D) plus at most two critics alive at once; the project settings cap subagents at 6
    concurrent".
    - Problem: the dead-code-auditor, spawned at A1 and writing "in batches", makes a seventh.
    - Overnight: in I1, with four builders and the auditor alive, the second critic's spawn is refused or queued, and the window
      waits.
    - Fix: the auditor finishes its whole list before builder D starts, or the windows run one critic while the auditor is alive.

50. **builder-brief.md (Builder D)** says "the lead runs the Editor compile, the touched test batches and verbs37 before the next
    batch". NIGHT61 section 5 says "verbs37 x5 at the next window".
    - Problem: the two cadences contradict.
    - Overnight: D does one batch per window (three a night), or stacks unverified batches. A stack that breaks verbs37 then needs
      extra builds to find the bad batch.
    - Fix: "at most 2 batches per window; a verbs37 miss reverts that window's batches".

51. **Slate S1 and the GATE file**: "verbs37 x5: 27 / 29 / 31 / 31 / 31, RED rows none".
    - Problem: 27 of 31 and 29 of 31 square with "RED rows none" only if the missing rows are n/a, and NIGHT61 doesn't say they are.
    - Overnight: an unchanged 27 gets called RED, or a new RED that trades places with an old n/a row keeps the count and passes.
    - Fix: "the same rows pass as at n60-freeze (name them); 0 new RED".

52. **Section 9**: "(1) What is still not good: the 10 worst things ..., each with its frame".
    - Problem: gg-proxy is an AI judge of stills (L-105: such a judge "FAILED calibration"), and its own rule says feel complaints
      have no frame ("a frame cannot show this").
    - Overnight: feel items are dropped for want of a frame, or given a decorative one.
    - Fix: allow "frame: none (feel)", and label the ranking as counts of GG's own quotes.

53. **Slate S6**: "the world FOV rises smoothly".
    - Problem: "smoothly" isn't measured.
    - Overnight: a step change at the sprint threshold passes.
    - Fix: "at most 0.5 deg per 120 Hz step in the reel's FOV print".

54. **Section 7, A1**: "the `design.n61` block with `_why` in both config copies".
    - Problem: it doesn't say A1 pre-places the five new rows (bossRule61, fixture61, style61, fov61, press61). builder-brief.md step 4
      has each builder add its own row to both copies with its own `n61-<letter>-rows.cjs`.
    - Overnight: three builders edit the same new JSON block, and every window has cherry-pick conflicts in two files (L-049 / L-065:
      "the shared commit pre-places hooks and placeholders").
    - Fix: A1 pre-places all five rows, OFF, with `_why`; builders flip only their own fields.

55. **Section 7, A1**: "`fingerprint --off61` EQUAL".
    - Problem: `--offNN` turns off "every row of night NN and later" (NightRows.cs:12, :158). With no n61 row yet, `--off61` equals
      plain, so the check proves nothing.
    - Overnight: a row that bypasses NightRows passes A1 and every gate until it's turned ON.
    - Fix: N61SharedTests plants a dummy n61 row ON and asserts that `--off61` turns it off.

56. **Section 0 rule 3**: "The hook blocks all of these".
    - Problem: this is proven for the main tree (`Logs/guard-blocks.log` has live blocks today) but not for builders. `.claude/` is
      untracked (`?? .claude/`), worktrees start at HEAD, and the hook runs `${CLAUDE_PROJECT_DIR}/.claude/hooks/guard.cjs`. Not
      checked: what CLAUDE_PROJECT_DIR is inside a builder's worktree.
    - Overnight: if it points at the worktree, the script isn't there and builders' calls run unguarded. If the harness honours
      `onFailure: block`, every builder call is refused instead.
    - Fix: the first builder runs one probe that is harmless if it goes through (`echo probe > Logs/n61/guard-probe.md`) and reports
      the block line.

### E. CLAUDE.md

57. **Header**: "Order of authority: the org rules, GG's words above, CLAUDE.md, this file, HANDOFF.md, the logs."
    - Problem: CLAUDE.md orders it "the org rules, then GG's message / the night's prompt, then this file [CLAUDE.md]".
    - Overnight: on any conflict (finding 58, for example) each file says the other loses, and the lead picks whichever suits.
    - Fix: "CLAUDE.md's hard rules win over this file; this file wins on the night's process".

58. **Section 5 / slate**: "22 replays exact", against CLAUDE.md rule 3: "all 16 trajectory replays are bit-exact".
    - Problem: `PortExport/trajectories` holds 22 entries, and the gate's golden total counts every replay (Golden.cs:356). NIGHT61 is
      right and CLAUDE.md is out of date.
    - Overnight: a builder reading CLAUDE.md reports "16 / 16", or questions the 22.
    - Fix: one line in section 5 ("22; CLAUDE.md's 16 is out of date"), and GG updates CLAUDE.md.

59. **4.2 and 9 (the builds and the tag)**, against CLAUDE.md rule 1: "After any Player build, run `git diff --stat ProjectSettings`"
    and "Before a commit or a tag, restore ... LiberationSans SDF - Fallback.asset".
    - Problem: neither appears in a window or in the close. `n41-build.sh` only prints `git status --short`, and memory adds the URP
      Mobile_RPAsset rewrite.
    - Overnight: five builds leave Play-mode and build drift beside GG's real edits, exactly what L-047 trips over the next night.
    - Fix: add both to 4.2 after each build, and to section 9 before the tag.

60. **Slate S4**: "each light gets a cable, arm or bracket to the nearest ceiling or wall".
    - Problem: finding that ceiling takes a ray, and CLAUDE.md rule 4 says "every ray use[s] the C# web collider". Campaign levels may
      have no Unity colliders.
    - Overnight: Physics.Raycast hits nothing and cables hang into the void, or rule 4 is broken.
    - Fix: "cast against TriangleWorld / the level's box layers".

61. **4.4, 5 and 9 (where the evidence lives)**.
    - Problem: `Logs/` is ignored (`.gitignore:6`). The verdicts, SPOT lines, gg-proxy start and close files, DEADCODE.md,
      DEADCODE-candidates.md and flow-base.txt all live there. Only the slate is force-added, and "`verdict: <file>`" never says where
      the file goes.
    - Overnight: after the night the slate's verdict links point at uncommitted files, and the report's frames are pruned away.
    - Fix: verdicts and SPOT lines go to `Research/2026-10-22-wave61/` (tracked), or the close force-adds `Logs/n61/*.md` and every PNG
      the report cites.

62. **Section 0 rule 4**: "commit every step", plus the brief's "the test, the change, the row script". CLAUDE.md rule 1 says "One
    commit per feature".
    - Problem: the two contradict in the literal sense.
    - Overnight: a careful builder squashes the test-first commit, and the lead can't run it RED on its own commit (4.2).
    - Fix: "one feature = its test commit, its change commit and its row commit; never squash them".

63. **Slate S3 and S5 (UI work)**, against CLAUDE.md: "Load ... `unity:ui` before UI work"; rule 8: "Fonts: the converted theme fonts in
    `Assets/Resources/Fonts/`".
    - Problem: neither NIGHT61 nor the brief mentions either rule for the boss line or the rank animation.
    - Overnight: the boss line ships in TMP's default LiberationSans, which breaks rule 8.
    - Fix: S3 and S5's doneWhen name the font asset, and the brief says to load `unity:ui`.

### F. Time

64. **Section 7**: "1:45 / 3:15 / 4:45 | +45 min | integration windows".
    - Problem: a window of about 3 slices, using the brief's own numbers, takes roughly 95 min, not 45:

      | step | time |
      |---|---|
      | RED/GREEN compiles and test runs | ~15 min |
      | gates | ~5 min |
      | build | 2 min |
      | on/off reels | ~4 min |
      | verbs37 x5 | 6 min |
      | three SPOTs | ~10 min |
      | three GATEs (15-25 min each, two alive at once) | ~40 min |
      | ON flips plus `--flow`, ~5 min each | ~15 min |

    - Overnight: I1 runs into I2, and I3's critics run past the freeze.
    - Fix: at most 2 slices per window, 75-min windows at 1:45 / 3:30 / 5:00, and critics allowed to finish during the next building
      stretch.

65. **Section 7**: "5:30 | 6:00 | the last critics and ON flips", together with section 9's "the smoke with no agent running".
    - Problem: I3's GATE critics, spawned around 5:20-5:30 at 15-25 min each and two at a time, end around 5:50-6:20.
    - Overnight: the close's smoke waits for them, or runs beside them and breaks its own rule, and everything after it slides.
    - Fix: no GATE spawn after 5:15; anything later is UNSURE and stays OFF.

66. **Section 9, a close in 60 min**.
    - Problem: itemized, the close takes about 75-95 min:

      | step | time |
      |---|---|
      | gates, with the Editor compile | ~6 min |
      | build | 2-4 min |
      | five verbs37 reels | ~6 min |
      | smoke | 5 min |
      | Game suite (1006 tests; Core's 1095 took 13 min) | not checked |
      | gg-proxy (can overlap the suite, not the smoke) | 15 min budget |
      | report | ~10 min |
      | HANDOFF | ~8 min |
      | harvest and LESSONS consolidation (NIGHT60's took its own commit) | ~15-20 min |
      | L-107 match, commit, tag | ~5 min |

      The forge's ~45 min holds only with a ~10 min suite and a pre-written harvest.
    - Overnight: the 7:00 cap breaks, or the last steps (harvest, tag) are dropped ad hoc.
    - Fix: draft the report, HANDOFF and harvest in 5:30-6:00, start gg-proxy during the suite, and set a cut order at 6:45 (finding
      10).

67. **3 / 4 / 5 (the lead's load)**: the lead owns B1 and B2, every integration, every compile and test run, every Player, reel, SPOT
    and GATE, and each D1 batch's compile, golden, fingerprints and test batches.
    - Problem: the retro's finding ("~25 minutes of the lead building") comes back by design. The lead is the night's bottleneck.
    - Overnight: windows overrun (finding 64), and B1 and B2 slide to the end.
    - Fix: give B1's code and test to a builder, with the lead running only the probe; run D1's checks once per window.

68. **Section 3**: "Owners: ... G (S3, S5, S6)".
    - Problem: G holds the three hardest slices, one at a time, over three windows: five bosses with glyphs at two resolutions, a
      cause hunt in Core that may become a sim row, and the camera. V and L hold two slices each.
    - Overnight: S6 never reaches a window.
    - Fix: give S6 to V after S1, or rank it out now.

69. **4.1**: builders run "`n61-gates.sh --no-editor`" in their worktrees whenever they like.
    - Problem: each run builds and runs CoreHarness plus six SimHarness fingerprints. n41-suite.sh's rule is "one heavy job at a time",
      and memory records 3-5x slowdowns and flaky timing beside other jobs.
    - Overnight: the lead's reels, verbs37 and smoke run beside four harness builds, and the Player evidence comes out slow or flaky.
    - Fix: a `Logs/n61/heavy.lock` that the lead holds during Players and the suite; builders wait for it before running gates.

70. **Section 7**: "0:00 | 0:35 | bearings ..., the end-to-end check, gg-proxy's state check"; 1.5: "(want 31 / 31, 0 NREs ...)".
    - Problem: with nothing failing, the bearings take about 35-40 min:
      - `--flow-base`: ~5 min
      - gates: ~6 min
      - build: 2-4 min
      - a reel
      - gg-proxy: 15 min
      - reading 115 LESSONS rows, the slate and the log, the selftest, and starting the watcher

      Nothing says what a miss of 31 / 31 means.
    - Overnight: A1 starts around 0:45, and a 30 / 31 sends the lead debugging before anyone builds.
    - Fix: start gg-proxy on the n60 frames while n61-a0 builds, and add "a miss is logged as the new red and the night goes on (a
      30 min cap if it's an NRE)".

71. **1.1**: "docs/memory/LESSONS.md (all rows: they are rules)", every session.
    - Problem: 115 rows re-read after every compaction, on top of NIGHT61 and a growing log.
    - Overnight: each re-read brings the next compaction sooner on a 7 h night.
    - Fix: all rows at t0; after a compaction, only the rows the log cites.

72. **1.5 / 4.3**: `n61-reels.sh` starts `n61-prune.sh --loop` in the background and stops it only when the set ends.
    - Problem: if the set is stopped early (TaskStop, a usage stop), the prune loop waits forever for "all done" (memory: TaskStop
      orphans children).
    - Overnight: an orphaned loop keeps deleting frames under that tag. Re-running with the same tag sets two pruners racing the
      lead's frame copies.
    - Fix: after any stopped reel set, kill its prune loop by PID, or have the loop exit when its parent PID is gone.

## The five most serious
1. **Disk.** C: has 11 GB free now, under the guard's 12 GB line. Tonight has nothing to prune, nothing lets the night free older output, and the watcher exits and restarts forever (findings 1-3). Unless GG frees space first, every visible slice is dead at t0.
2. **No way out of a stuck Editor or a stuck close.** A status timeout is read as "unfocused", B2 can wedge the Editor, and n41-suite's drain waits up to ~150 min after a TIMEOUT. The close has no NOT RUN path, no time escape and no /goal text (findings 6-10, 12, 17, 18).
3. **Numbers can pass without a better game.** S2 is judged against a flow base its own commit rebuilds, and a failed flow run reads a stale EQUAL. The slate accepts any existing file as a verdict. The SPOT critic has Bash and Grep, can be re-run on a chosen pair, and gets an unrecorded flip; the GATE critic reads the builders' commit messages (findings 19-26).
4. **Wrong or missing tools.** `n60-freeze-rows.cjs` stamps every row "ON at NIGHT60" while `n61-freeze-rows.cjs` exists unnamed. The `dead-code-auditor` agent doesn't exist, the IDE0051 pointer leads to an analyzer that found nothing on planted dead code, and Unity's reflection-called methods aren't protected from deletion (findings 35-38).
5. **The clock doesn't fit.** A window needs ~95 min, not 45, and the close ~75-95 min, not 60. The first sim row turned ON (S5) makes every later gate run stop at the plain fingerprint, skipping the Editor compile gate until the close (findings 11, 64-67).

## Status against the revised NIGHT61.md (rewritten 09:30 during this review)

The findings above quote the version I read first. At 09:28-09:30 the forge rewrote NIGHT61.md (it now cites "redteam"), the seed, the
builder brief, n61-gates.sh, n61-slate.cjs and the agents (it added dead-code-auditor.md and skygrind-spotter.md). I re-read NIGHT61.md
once and spot-checked n61-gates.sh, n61-slate.cjs, skygrind-spotter.md and the batch copy. I did not re-read the new seed or the new
brief.

- **Fixed in the new text or tools (checked):** 6, 8, 9, 10, 11, 16, 19, 20 (`rm -f` of the old output, n61-gates.sh:24), 22 (the
  spotter has `tools: Read`), 24 (one SPOT per slice per build), 25, 26, 35, 36, 37, 39, 41, 46, 48, 58, 62, 63, 68.
- **Partly fixed:**
  - 1: now an explicit no-Player night; no space is freed.
  - 2: still loops; see N1.
  - 3: B1's doneWhen probe is a Player run (seed not re-read).
  - 12: no rule for a resume after 5:30 or 6:00, but 7:00 now jumps to (g).
  - 13: "the cap" is still undefined.
  - 17: a stop condition is stated; GOAL.txt not checked.
  - 21: n61-slate.cjs now checks `<id>: PASS` and the SPOT line, but n61-freeze-rows.cjs (unchanged, 08:59) still turns a row ON
    without reading the slate.
  - 23: the mapping is logged after the answer, and the flip is still not random.
  - 31: B1 has moved to D; B2 is still judged by the lead.
  - 38: `m_MethodName` was added; Unity message methods, `[RuntimeInitializeOnLoadMethod]` / `[MenuItem]` and `SendMessage` strings
    aren't in NIGHT61 (the auditor file may cover them; not checked).
  - 49: the auditor starts only when under six agents are alive, but nothing makes it finish before two critic slots are needed.
  - 59: the ProjectSettings diff was added; the TMP Fallback restore before a commit or tag is still missing.
  - 61: evidence is now force-added, but the verdict's path is still unnamed.
  - 64 and 67: integration is now continuous; the lead's per-build load is unchanged.
  - 70: no rule yet for a 31 / 31 miss.
- **Still open (text unchanged):** 4, 5, 7, 14, 15, 18, 32, 33, 40, 42 (section 2 now requires "this slice's one-row switch", which S2
  doesn't have), 43, 44, 45, 47, 52, 54, 55, 56, 57, 65, 69, 71, 72.
- **Not re-checked (the seed or brief changed):** 27, 28, 29, 30, 34, 50, 51, 53, 60.

### New problems in the revision

N1. **1.2**: "`<warn>` = 15 if 18 GB or more are free, else 3 GB under today's free space and never under 12".
    - Problem: at the forge's own 11.4 GB reading, warn = max(8.4, 12) = 12, which is above the free space.
    - Overnight: the watcher exits at once, "restart it with a new `<warn>`" gives 12 again, and finding 2's loop is back at exactly the
      measured level.
    - Fix: under 12 GB don't start it (the night is no-Player anyway), or set warn = free minus 1 with no floor.

N2. **Section 9 close**: the time boxes are (a) 10 + (b) 5 + (c) 10 + (d) 8 + (e) 20 + (f) 15 + (g) 5.
    - Problem: they sum to 73 min inside a 60 min close.
    - Overnight: (f) (the slate, NIGHT61-REPORT.md, HANDOFF, the harvest) starts around 6:53. "At 7:00, whatever is left is logged
      under Blocked, then (g)" then cuts the night's main output first.
    - Fix: write (f) during 5:30-6:00 and only fill in the close's numbers in (f), or make the boxes sum to 55 or less.

N3. **1.3 / section 3**: "build Players in batch mode from `C:\Users\gaige\dev\SKYGRIND-unity-batch`"; "run Game M only in batch mode on
    the batch copy".
    - Problem: the copy exists, but its HEAD is `6462b106 NIGHT54 close`, seven nights old. NIGHT61 gives no sync step, and the guard
      blocks `git pull` and `merge`.
    - Overnight: a fallback build or B2's batch run builds and tests NIGHT54's code and reports it as tonight's.
    - Fix: name the sync (`git -C <batch> fetch <repo> port/stage3`, then `checkout --detach FETCH_HEAD`), print its `log -1`, and
      require it to equal HEAD before any batch run.

N4. **Section 3 B2 (a batch-mode Game M run)**.
    - Problem: a hung batch Unity is also `Unity.exe`, and the guard's kill-editor rule blocks `taskkill //PID` on any Unity.exe image.
    - Overnight: a hung M run can't be stopped. It holds the batch copy's project lock, so the fallback batch build is refused, and it
      keeps CPU and RAM all night.
    - Fix: launch it as `timeout <s> <Unity.exe> -batchmode ...` in one command, or let the guard allow a PID whose command line holds
      `-batchmode` and the batch copy's path.

N5. **1.3 batch builds and B2's batch run**.
    - Problem: neither matches the guard's disk rule, which knows only the `n\d+-build|reels|smoke|focus-probe` scripts and
      `unity command build`. Both write Library / Builds into the batch copy on the same nearly full C: drive.
    - Overnight: they fill the last gigabytes with no refusal.
    - Fix: run them through a script the guard's heavy rule covers (for example `Tools/night/n61-batch-build.sh`).

N6. **1.2**: "Under 12 GB at t0 ... run the no-Player night".
    - Problem: there is no way back to Player work if space returns later.
    - Overnight: GG frees space at 1:00, and the night still never builds, reels or turns anything ON.
    - Fix: "re-check `df` at each build slot; above 15 GB, resume the Player steps".

### The five most serious against the current version
1. **Disk.** The revision makes it an explicit no-Player night, but the watcher formula still loops at the measured 11.4 GB (N1), nothing frees space, and there is no way back (N6).
2. **The batch-copy fallback.** It sits at NIGHT54's HEAD with no sync step (pull and merge are blocked), bypasses the disk guard, and a hung batch M run can't be killed because the guard blocks any Unity.exe (N3-N5).
3. **The close's boxes.** They sum to 73 min in 60, and the 7:00 cut falls on the report, HANDOFF and harvest (N2).
4. **Open gaming.** S2 has no one-row switch for the new SPOT rule (42), the lead still writes gg-proxy's slices with no commit before measurement (33), the ON tool doesn't read the slate (21), and D1's protected paths stop at --off59 while the gate checks --off57 (32).
5. **Open ending.** No critic timeout (14), no log of agent ids for a SendMessage resume (15), and "fixing it is task one" still has no time box (7).
