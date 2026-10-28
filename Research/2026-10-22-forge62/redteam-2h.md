# NIGHT62 (2-hour) red team, 2026-10-22 ~21:00-21:25: 14 findings, ranked by the run time each would waste

Read: NIGHT62.md, GOAL.txt, slate-seed-2h.json, DECISIONS D-14..D-23, RESEARCH section 5, CLAUDE.md, DECIDE62.md, the guard's clock rule, the
n62 tools, n41-build / n46-build-rows / n56-smoke-steal / n60-focus-probe / reel.sh / smoke.sh / wait-build.sh / unity-loop.sh, the three
agent files, the builders' notes V.md, G.md, V-S4.md, V-C3.md. Run (read only): `node .claude/hooks/guard.cjs --selftest` (110 / 110),
`node Tools/night/n62-slate.cjs <seed>` (8 todo, exit 0), `node Tools/night/n62-clock.cjs` (no clock, exit 1), `df -h /c` (42 GB free),
the memory command as written and fixed. Things that check out: every `-skygrind...` flag the prompt names exists in Assets/Scripts; the reel
scenarios a157, a357, a154, boss61 and verbs37 exist (FeelReel.N57.cs:15, FeelReel.N54.cs:15, FeelReel.N41.cs:21, FeelReel.N27.cs:20); the
five design.n61 rows exist, OFF, in both identical config copies; all thirteen test classes exist (eleven N61* classes); the tag u3-n61 exists;
the guard lets every close step through in FREEZE (gates, the n62-freeze build / reels / smoke, git, clock stop) and refuses untagged heavy
steps, test runs and new agents, and at CAP refuses every heavy step but no git step.

1. **blocker. Every GATE needs verbs37 x5, which runs fifth; two of five rows cannot be judged before the freeze.**
   Line: NIGHT62.md:71-89 (the reel order: P5, P1, P3, P2, verbs37, P4) and :112 (a GATE starts "the moment its evidence and SPOT exist").
   Problem: every visible entry's doneWhen ends with "verbs37 x5 unchanged" (slate P5, P1, P3, P2, P4), and the critic's own rule 3 is "NOTHING
   LOST: verbs37 x5 ... on the build that carries the slice", where a number it cannot find makes the verdict UNSURE
   (.claude/agents/skygrind-critic.md:24-27). verbs37 x5 starts only after 22 reels, about 0:45-0:50. A GATE started earlier returns UNSURE,
   the row goes SHORT, and no rule allows a second GATE. Waiting is no better. Three slots of 17-23 min give a first wave around 0:50-1:13 and
   a second around 1:13-1:36, so P2 and P4 land after the 1:30 flip window and the 1:40 freeze-build cutoff (:148). P3's doneWhen line 3
   (level 1's a357 entry frames, slate:56) is only on P4's `s4on-f`, the first thing cut when behind (:108).
   Sizing (item 4): the reel phase overruns first. 38 reels at the measured 63-121 s each (n60-b7: ten a357 reels in 12:49; n60-b8: verbs37
   68-89 s, a157 with clear shots 78-121 s), plus about 9 set starts and 3 focus-probe Players, come to about 55 min. Starting at about 0:13,
   the reels end around 1:08-1:15, not 1:05.
   Fix: build, then **verbs37 x5 first**, then the rest of P5, P1, P3 (with `s4on-f,n52-foundry,a357` moved into P3's set), P2, P4. Start each
   GATE once verbs37 and its own set have landed, and say in each brief that verbs37 x5 is on n62-p0. With the cuts in 3 and 10, the first
   wave runs about 0:27-0:58, P2 about 0:50-1:13 and P4 about 1:03-1:26.
   Checked: the critic file, the slate, the reel start times in Logs/feel/n60-b7, n60-b8, n57-r1 and n57-r2 summary47.txt.

2. **major. The prompt never says to run reel sets or agents in the background.**
   Line: NIGHT62.md:71 ("each `bash Tools/night/n62-reels.sh n62-p0 <sides>`"), :112 and :103 (GATEs and gg-proxy), :93 (the only
   background test batch).
   Problem: the Bash tool stops a foreground command after 120 s by default, or 600 s at most. P4's ten a357 reels take about 12-15 min, and
   P2's six boss61 reels (never timed) and the close's five verbs37 reels (6-7.5 min) are near that limit. A killed set leaves its Player and
   its prune loop running (memory: taskstop-orphans), and the next set then starts a second Player (rule 5: one Player at a time). A GATE
   started as a foreground Agent call blocks the lead for 17-23 min, so no set starts, no SPOT runs and nothing is measured. Also, reel.sh's
   per-side timeout is 1500 s (n62-reels.sh:21), so one hung side holds a set for 25 min. And P6's probe steals the foreground; started
   while a background set is still running, it would steal it from that reel's Player.
   Fix: add one rule to section 0: "every reel set, the smoke, `n62-gates.sh --flow`, the test batch, every GATE and gg-proxy start with
   run_in_background; wait on the completion notice; the spotter may run in the foreground; start the focus probe only after the last set's
   completion notice". Optionally lower the per-side timeout in n62-reels.sh to about 400 s.
   Checked: the Bash tool's limits; n62-reels.sh; reel.sh:6-18; n60-b7's a357 timings; RESEARCH.md:16 (background subagents notify on
   completion).

3. **major. P4 is the weakest part: about 40 % of the reel time for the row least likely to show anything GG can see (item 7).**
   Line: NIGHT62.md:87-89, :64 and :103; slate P4.
   Problem:
   - It costs 16 of 38 reels: ten a357 (about 13 min measured) and six a154 (never timed).
   - Its builder doubts it will read on screen: "They may not show against the black ceiling at 1080p" (V-S4.md:118-120).
   - The MAIN has no frame meter. Its red, 49, is a light-plan count (V-S4.md:38-53, "the frame count is the lead's"), so the lead would count
     lamps by eye on about 25 entry frames per side.
   - Its SPOT rooms are not chosen (V-S4.md:159-160), and "SPOT on two a357 entry pairs" (slate:95) contradicts "One SPOT per slice per
     build" (:64).
   - Its evidence lands around 1:05-1:10, while gg-proxy holds the only spotter slot from 1:00 to 1:25 (:103, D-16). The GATE then starts
     at 1:15-1:25 and its verdict lands at 1:35-1:48, past the 1:40 cutoff.
   Fix: SPOT first. Run one a357 pair on n52-iceline (the floating panel gg-proxy named, vi/verbs37 f0300) and SPOT it. Run the other eight
   a357 reels, and a154 on foundry only (two reels), only if the SPOT names the lamps or fixtures. Otherwise P4 is SHORT "NOTHING OBVIOUS"
   after two reels, saving about 18 min. Use the same SPOT-first order for P2 (b61k and b61koff before the other four bosses) and P1 (s1on and
   s1spot first). Move gg-proxy to about 0:30-0:50: its frames, verbs37 and a157 with all five rows on, exist once verbs37 and c3on-k land.
   Checked: V-S4.md, the slate, D-16, the n60-b7 timings.

4. **major. The fix round contradicts the clock table and cannot finish before the freeze.**
   Line: NIGHT62.md:99-102 ("only if its build can start by 1:00") against :131 ("1:00-1:30 ... one fix round").
   Problem: with the current order the first verdicts land at 0:50 or later (1:10 or later if they wait for verbs37). A fix still needs:
   - a test-first change and its tests;
   - the gates (1 min 27 s), the diff read, and the n62-b2 build (33-76 s);
   - the row's reels, plus verbs37 x5 on n62-b2, which the critic requires on "the build that carries the slice" (5 reels nobody counted);
   - a new SPOT and a GATE of 17-23 min.
   The earliest PASS is about 1:35-1:45, with no agent allowed to start after 1:30 if anything slips. Meanwhile it holds the Editor and the
   one Player that P4 and P6 need.
   Fix: drop the fix round tonight. A FAIL row goes SHORT with the lever the critic names (P1's are in UnityTuning.N61V.cs: ExitFrame61Glow,
   ExitFrame61LightIntensity, V.md:123). If it stays, gate it on "its GATE can start by 1:10", count the 5 extra reels, and fix the table.
   Checked: NIGHT62.md sections 3, 4 and 6; skygrind-critic.md:27; RESEARCH.md:72-75.

5. **major. The 8-hour disk watcher switches off the /goal's evaluation for the whole run, and the close never prints `git tag -l u3-n62`.**
   Line: NIGHT62.md:42 (`n62-diskwatch.sh <warn> 8`) and :154-157 (close step g); GOAL.txt.
   Problem: RESEARCH.md:8-10 (marked Verified) says "a turn that ends with a subagent or a background shell still running is not evaluated".
   The watcher runs from about 0:05 to the close, so the evaluator never runs and never pushes the lead on. A turn that ends early with
   nothing else pending sits idle until a notification arrives or the 30-minute check-in (RESEARCH.md:15). In step (g) the watcher exits up to
   60 s after `touch` (n62-diskwatch.sh:19-21, `sleep 60`), which is after `NIGHT62 COMPLETE`, so it starts a new turn after the "final"
   message. Step (g) also prints `git tag -l` before `git tag u3-n62`, and nothing ever prints the `git tag -l u3-n62` line GOAL.txt names
   (`git tag` prints nothing on success). The disk does not need a watcher: C: has 42 GB free, the guard refuses heavy steps under 12 GB
   (guard.cjs:242), and n62-reels.sh skips sides under 10 GB (:19).
   Fix: drop the watcher and print `df -BG /c` with the clock line (or after each set). If it stays, run it with hours = 2 and, in (g), touch
   the stop file first and wait for its exit notice. Then run `git tag u3-n62 && git tag -l u3-n62`, the clock stop, and the final message.
   Checked: n62-diskwatch.sh, RESEARCH.md section 1, GOAL.txt, `df -h /c` at about 21:05.

6. **major. P1's meter prints the wrong MAIN.**
   Line: NIGHT62.md:77-78 ("MAIN and GUARD: `python -I Tools/night/n60-v-lamp.py ...`").
   Problem: the meter's per-room PASS / FAIL and its summary line, "LAMP: MAIN (door / room median >= 1.5x) n / 5" (n60-v-lamp.py:86-89),
   are lamp60's old MAIN. The slate's MAIN is the own gain, door luma on / off >= 1.5x in at least 4 of 5 rooms (slate:34), and the meter
   prints that only inside parentheses. The red already scores 4 / 5 on the old MAIN (Logs/n60/b8a2-lamp.txt:6), and the planted cases
   (n60-v-lamp.py:93-140) test only the old MAIN. A lead or critic who trusts the summary line gets P1 wrong (L-100).
   Fix (forge, before launch): add a line "OWN GAIN (door luma on / off >= 1.5x) n / 5" and one planted own-gain case. Or write in the
   prompt and brief: "count the per-room 'own gain ... x' values; ignore the LAMP summary line and the PASS labels".
   Checked: n60-v-lamp.py:80-89, Logs/n60/b8a2-lamp.txt, V.md:18-28.

7. **major. P5's "= 0" counts pixels that are not the gun, so a cyan gun can still FAIL.**
   Line: slate:16 ("yellow-hue pixels on the viewmodel ... = 0 in the four poses"); NIGHT62.md:74-75.
   Problem:
   - n62-gun-yellow.py counts the whole bottom-right rectangle, 0.55-1.0 of the frame (:6, :63): world, kiln glow, enemy tells and HUD.
   - n56-colour.py's yellowGunPct is yellow inside the gun's rectangle (n56-colour.py:220-222), which also holds world pixels.
   - Non-gun yellow is already measured: 15 px in kb3's corner (RESEARCH.md:81).
   - The TORCH's tank bands may be a fixed colour (V-C3.md:82-83).
   - Three of the four poses are JPGs.
   "Never loosened after" (:58) then forbids fixing the threshold mid-run.
   Fix (now, before any P0 measurement): define the count as yellow px among the pixels that differ between the on and off frames of the
   same pose (the recoloured gun), which must be 0, with the rest of the rectangle within a stated number of px of off. Add a `--diff` mode and
   a planted case to n62-gun-yellow.py.
   Checked: n62-gun-yellow.py, n56-colour.py:200-222, V-C3.md, RESEARCH.md section 5.

8. **major. The critic agent reads NIGHT61's slate, whose ids differ from tonight's.**
   Line: .claude/agents/skygrind-critic.md:14 ("The slice's entry in `Logs/n61/slate.json`"); NIGHT62.md:112-114.
   Problem: Logs/n61/slate.json holds S1, S3, C1, C3 and S4 with NIGHT61's wording; tonight's ids are P1-P5. Both n62-slate.cjs (:26-27)
   and n62-freeze-rows.cjs (:20-23) refuse a PASS unless the verdict's first line is exactly "<tonight's id>: PASS". A critic that writes
   "S1: PASS" costs a SendMessage round or a whole new GATE (17-23 min).
   Fix: before launch, change the agent file to read "the slate the lead names (tonight Logs/n62/slate.json)". Put "your first line exactly
   `P1: PASS` / `P1: FAIL <reason>` / `P1: UNSURE <reason>`" in each GATE brief (:112).
   Checked: skygrind-critic.md, the ids in Logs/n61/slate.json, the two tools.

9. **major. The close does not fit 1:30-2:00, and its gates run before its last flips.**
   Line: NIGHT62.md:146-157 (the 25-minute boxes) and :131.
   Problem:
   - Step (b) may wait until 1:40. After it: (c) 6-7.5 min (verbs37 at 68-89 s each on n60-b8), (d) about 4 min (227.9 s plus launch),
     (f) 8 min for the report, HANDOFF, the full harvest (ledger, THREADS <= 60, FEATURES, CORE) and LESSONS, then (g). The close ends about
     2:08-2:12. NIGHT61's harvest alone was its own commit (adddcf2a).
   - The drafting slot, 1:00-1:30, is already full: verdicts, flips (about 15 min each, RESEARCH.md:75), `--flow` (5 min of flow, then the
     gates and the Editor compile, about 7 min), gg-proxy and the fix round.
   - Step (a) runs before (b)'s wait, so a row flipped at 1:35-1:40 ships without its `--flow` gate.
   - A `--flow` started around 1:25 overlaps (a): two `dotnet run --project Tools/SimHarness` at once, and the build-output locks fail a gate.
   Fix:
   - In the close, run `bash Tools/night/n62-gates.sh --flow` once as (a), after (b)'s wait.
   - Take the harvest out of the 2-hour run; the GOAL does not name it, and the next forge can harvest as adddcf2a did. (f) keeps the report,
     HANDOFF and the LESSONS rows.
   - Draft the report skeleton around 0:45, while the P2 and P4 reels run.
   Checked: the boxes, the measured minutes, GOAL.txt, n62-gates.sh:15-35.

10. **minor. Three reels are exact duplicates.**
    Line: NIGHT62.md:80 (`t61on,n57-kiln,a157`) against :72 (`c3on-k,n57-kiln,a157`); :72-73 (`c3on-s ... verbs37`, `c3on-p ... verbs37`)
    against :85 (`vs`, `vp`).
    Problem: each pair is the same build, level, scenario and arguments, so the three duplicates cost about 3.5-4.5 min.
    Fix: drop them. c3on-k is P3's on side, and vs and vp are P5's span and spire on sides. That leaves 35 reels.
    Checked: the side specs; n62-reels.sh passes only these arguments.

11. **minor. The memory check fails as written, and it decides branch A or A-lean.**
    Line: NIGHT62.md:46.
    Problem: in Git Bash, bash expands `$o` inside the double quotes, so PowerShell prints "'=Get-CimInstance' is not recognized" (ran it:
    exit 1). The result decides between three GATE slots and two.
    Fix: `powershell -NoProfile -Command '$o=Get-CimInstance Win32_OperatingSystem; "{0:N1} GB commit free" -f ($o.FreeVirtualMemory/1MB)'`.
    Ran it at about 21:10: "46.1 GB commit free of 97.6".
    Checked: both forms run in Git Bash.

12. **minor. Nothing says who writes the verdicts for P6 and P7, and P6's last proof comes after agents are refused.**
    Line: NIGHT62.md:66 ("Bug, test and dead-code entries are exempt"); slate:114 (P6's smoke line); n62-slate.cjs:24-27.
    Problem: the slate tool refuses `passes: true` for any slice without a "<id>: PASS" verdict file, but no critic is assigned to P6 or P7.
    P6's third doneWhen is the close's smoke at about 1:45, after the freeze refuses new agents. Careful engineers would split three ways:
    the lead writes the verdict, a critic must, or the slice stays SHORT.
    Fix: add "P6 / P7: no critic; the lead writes Logs/n62/verdict-P6.md (or -P7.md) whose first line is `P6: PASS` only when every doneWhen
    line has its printed line; P6 is judged after (d)".
    Checked: the slate tool and the slate.

13. **minor. The prune loop stops after the first set in a tag, and the guard names the wrong prune tool.**
    Line: NIGHT62.md:23 ("prunes beside every set"); n62-prune.sh:22; guard.cjs:242.
    Problem: the loop exits at once when summary47.txt already holds "== all done", and P5's first set writes it. The tag must be the build
    folder (n62-reels.sh:21), so every later n62-p0 set is pruned only at its end. NIGHT60 shows the effect: n60-b8's second set took C: from
    13 GB to 10 GB. Tonight that is about 7-10 GB held during the a357 set against 42 GB free, so nothing stops, but the prompt's claim is
    false. The guard's disk refusal also tells the lead to run Tools/night/n61-prune.sh, which refuses n62-* tags (n61-prune.sh:9).
    Fix: in n62-prune.sh, count the "== all done" lines when the loop starts and break only on a new one. Change guard.cjs:242's text to
    n62-prune.sh.
    Checked: n62-prune.sh, n62-reels.sh, n61-prune.sh, Logs/feel/n60-b8 and n57-r2 summary47.txt (the free-GB column).

14. **minor. The forge's guard, settings and tools are uncommitted.**
    Line: NIGHT62.md:38-39 (the bearings' `git status --short`); section 8 (g) and (7).
    Problem: the clock rule (guard.cjs, +99 lines), the settings.json Agent|Task matcher and the eight Tools/night/n62-* files are modified
    or untracked. The prompt never says who commits them, so tag u3-n62 would lack the tools the report must cite "exactly as run". A lead
    committing "explicit paths" may or may not include them.
    Fix: the forge commits them before launch: guard.cjs, settings.json, Tools/night/n62-*.{sh,cjs,py} and
    Research/2026-10-22-forge62/redteam-2h.md. Otherwise the prompt names that commit as the run's first.
    Checked: `git status --short`, `git diff --stat .claude/`.
