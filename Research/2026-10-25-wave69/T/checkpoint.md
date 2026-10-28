# NIGHT69 builder T checkpoint (branch n69-T, worktree .claude/worktrees/agent-af48e888c59fa3626; t0 sha d9e22d48)

## Y1 (the STYLE rule) - written 10:04
- Code: Tools/SimHarness/Harness.N68.Skip.cs (V68StyleNeed = refusal - 5; V68StyleNeedN68 = NIGHT68's rule, printed only; V69StyleVerdict:
  NaN -> UNSURE; `--style-cases` 9 cases), Tools/SimHarness/Harness.N65.Verb.cs (the M2 VERB line uses V69StyleVerdict, prints NIGHT68's need
  beside; doc comment and comments).
- Evidence (this folder): Y1-style-cases-RED.txt (t0 rule + the new cases: 2 / 9, exit 1), Y1-style-cases-GREEN.txt (9 / 9, exit 0),
  Y1-skip-kb3-3seeds-RED/GREEN.txt + -diff.txt (only the M2 VERB line and the exit code differ), Y1-red-bars.txt (the same bars),
  Y1-flow-run.txt / Y1-flow-off67.txt / Y1-flow-compare.txt (the five-level walk: EQUAL to main's Logs/n69/flow-base.txt).
- Commit: 33998e31 (accepted by the lead 10:08, picked to main).

## Y3 (the patch-share tool) - written 10:08
- Tools/night/n69-patch-share.cjs (node only): `--level <L> <id>` prints the row and `n69-patch-share: PASS|PATCH <id> (<share> % kept)`
  (exit 0 / 1; 2 on an error); `--cases` the planted cases (12 / 12 GREEN: the rooms-base copy 100 % PATCH, one box moved 15 / 16, a pit's
  lava flipped 0 / 1, g7 / v2 at t0 0 % PASS, the seven NIGHT68 rooms equal to the scout's table).
- Evidence: Y3-patch-share-RED.txt (no tool at t0: MODULE_NOT_FOUND), Y3-patch-share-GREEN.txt, Y3-flow-note.txt (node only: the DLL is
  Y1's, its walk EQUAL).
- Commit: 27e0bfc4.

## Y2 (the M3 reader and probe) - written 10:20
- Code: Tools/SimHarness/Harness.N65.Verb.cs (the launch probe: the first LaunchEvent id kept, THAT body tracked 3 s, in a pit / in lava only
  from it and only with launchEv >= 1; prints "launched body <kind> #<id> (the target: yes / no)" or "launched body none" and "in a pit n/a
  (nothing launched)"); Tools/night/n65-verbs.sh and Tools/night/n69-gate-pack.cjs (launchM3): M3 PASS only on a `: GREEN` line with
  `| launch36 [1-9]` and a YES, the reason printed on a FAIL. Planted-case tools: Tools/night/n69-m3-cases.sh (both readers on saved runs;
  `--at <sha>` for a commit's readers), n69-m3-dotnet.sh (a stand-in dotnet replaying a saved block), n69-m3-pack-case.cjs (the pack's
  launchM3 text run on a saved line).
- Evidence: Y2-readers-RED.txt (t0's readers 14 / 20: r2 kb3 PASS, planted P2 / P5 PASS: WRONG), Y2-readers-GREEN.txt (20 / 20),
  Y2-probe-kb3-RED/GREEN.txt and Y2-probe-v2-RED/GREEN.txt (the probe live on r2's levels: kb3 "launched body none ... n/a" -> M3 FAIL
  (nothing launched), v2 control GREEN launch36 1, launched body husk #2 (the target: yes), in a pit YES -> M3 PASS; every other line
  identical), Y2-m3-planted.txt (the planted blocks), Y2-chain.txt (the walk, the kb3 red, its skip: 10:18-10:33),
  Y2-flow-off67.txt / Y2-flow-compare.txt (the five-level walk EQUAL to Logs/n69/flow-base.txt), Y2-red-run.txt + Y2-red-kiln/kb3/ +
  Y2-red-compare.txt (the kb3 red re-made on the n69 harness: flow A / B / V and room-check EQUAL to NIGHT68's, pads apart from the path, the
  skip's pooled ratio 1.13 with sets 1.15 / 1.03, exercised 20 / 20, the same; only its M2 VERB line's STYLE text differs (Y1): the reds stand).
- Commit: 775e41df (10:35). Pick paths: Tools/SimHarness/Harness.N65.Verb.cs, Tools/night/n65-verbs.sh, Tools/night/n69-gate-pack.cjs,
  Tools/night/n69-m3-cases.sh, Tools/night/n69-m3-dotnet.sh, Tools/night/n69-m3-pack-case.cjs (main had no change to any of them since t0
  apart from Y1's Harness.N65.Verb.cs, which this file already carries).

## Next
- On call for fix rounds until 11:55 (the lead resumes by SendMessage). No commit to Tools/SimHarness after the lead picks Y2 (~11:20).

## Next
- Y3: Tools/night/n69-patch-share.cjs (node only), by 10:40.
- Y2: the M3 probe (Harness.N65.Verb.cs:275, :326-339) and both readers (n65-verbs.sh:19-22, n69-gate-pack.cjs:188-192), by 10:55.
