# NIGHT61-REPORT.md: the build night that could not build a Player

t0 = 10:18:34 AM (NIGHT61-LOG.md). Drafted 14:50 (4:32 / 7:00); section 7 is filled at the close. Every number below names its file or
command; "not checked" means exactly that.

**The short of it, honestly:** nothing a player can see changed tonight. Six player-facing slices and one bug fix were built behind OFF rows
and checked as far as a compiler and the sim can check them, and 370 lines of dead code went. None of it has been seen in a Player or run in
the Editor. GG asked for "real results"; tonight's results are real code with every Player and Editor proof still owed.

## Opening: the disk and the Editor (bearings steps 2 and 3)
- **No Player was possible.** C: had 8.3 GB free at 10:21 (`df -h /c`) and the guard refuses every build and reel set under 12 GB
  (`node .claude/hooks/guard.cjs --selftest`: 77 / 77 as planted). Over the night: 8.3 (10:21), 6.1 (11:25), 5.1 (12:00), 4.7 (13:00),
  4.5 (14:04), 4.6 GB (14:28). What holds the disk (`du -sm`, ~11:20): **Logs/ 200,611 MB, Builds/ 70,465 MB**, Library 5,785 MB; `git
  worktree list` shows 225 worktrees (one sampled at 165 MB). The night wrote none of that and may not delete it: freeing space is GG's call.
  The drop during the night was the builders' compile scratch (681 MB per run) and their worktrees until the delete-after-check rule at 13:01.
- **The Editor did not answer all night.** `editor_status` timed out at 10:21 and at every hourly probe after (11:04, 12:00, 13:00, 14:04;
  40 s each). One wake at 10:33 (idle 225 s): Unity took the focus, `set_autotick` still timed out at 30 s; GG's browser window was given back
  with the Alt-key unlock (checked). The Editor process is alive (Responding True, 2.8 % CPU) and its log stopped at 10:17; the last
  `run_tests` (07:44) timed out at 600 s, the same class as B2. **Likely cause, not confirmed:** NIGHT60's handoff says the Editor was left
  running Game M's MapChoiceTests at that close (`git show 1a8df4ac:HANDOFF.md`, line 25: "If it does not answer ... wait, then restart it
  yourself"). So no Editor compile, no EditMode test and no `run_tests` ran tonight. Restarting the Editor is GG's call (CLAUDE.md rule 10).

## (1) What is still not good: the ten worst things in the game now
From gg-proxy's honest state check on the newest frames that exist (Logs/n61/gg-proxy-start.md: 18 frames from NIGHT60's builds; quotes
are GG's recorded words with file:line). gg-proxy's close ranking was not run: there are no freeze frames to rank, and no visible change is
ON, so this ranking is still the state of the game. v1on-kiln/a357 was not found: not checked.
1. **Empty boxy rooms; plain dark pillars that serve no purpose** (vk/verbs37 f0300 and f1500, vi f1200, kb3-entry). GG asked 7 times.
   Tonight: nothing (S7 and S2 were not started; levels 4 and 5 wait on GG).
2. **Choppy movement, a camera blind to speed, a clunky wall ride.** A still cannot show it. GG asked 6 times. Tonight: nothing (S6 not
   started; the wall ride is Mover.cs, which needs a task that names it).
3. **Two or three layers of text at every room entry** (the instruction banner on 11 of 18 frames; kb2c-entry shows the room name, an
   instruction and the last room's ROOM CLEAR at once). Tonight: **C1 built, OFF** (design.n61.text61); not seen.
4. **Bosses never say what they want** (no boss frame in the set). Tonight: **S3 built, OFF** (design.n61.bossRule61); not seen. Found on the
   way: the live WYRM line at Design/unity-config.json:7526, "{punch} its head as it comes.", names the F punch, and builder G reads the code as
   saying the punch does not stagger the WYRM, the gauntlet swing does (SurvivalSim.Boss30.cs:286, not re-checked by the lead). Not changed.
5. **Dark views; lamps hanging in the air** (vs f1500 near black; vi f0300 a ceiling panel with no mount). Tonight: **S4 built, OFF**
   (design.n61.fixture61: 49 unmounted ceiling lights over five levels by builder V's count, housed with the row on). Whether the dark housings
   even read at 1080p is open. The dark rooms themselves: nothing.
6. **The style meter sits at D / DULL; the score box reads 500 in all 18 frames.** Tonight: **S5 SHORT**: the cause is in the sim's pacing,
   not the display (Style.cs, StyleCore.cs; the web's golden-pinned rules); the bot's real fights do climb (28-89 % of fight time above DULL in
   Logs/n61/flow-base-a0.txt). Found on the way: **burrower kills pay 0** (no burrowerKill in style.base; four burrowers on n57-kiln).
7. **Kills and hits are not satisfying.** A still cannot show it. Tonight: nothing.
8. **An enemy up close turns half the screen orange** (foundry-b1-mid.png). Tonight: nothing (C2 not started).
9. **The exit door is a small dark cyan frame 24-25 m away; the pad's beam beside it is brighter.** Tonight: **S1 built, OFF**
   (design.n61.exitFrame61); the beam competing with the door is not addressed by S1's doneWhen (gg-proxy's note).
10. **Yellow slabs on the guns, lime stripes on the floor.** Tonight: **C3 built, OFF** (design.n61.gunYellow61, the guns' yellow becomes the
    usable cyan); the lime stripes: nothing.

## (2) What you can see tonight
**Nothing.** No row was turned on. The visible rule (NIGHT61 section 2) needs a Player frame pair, a blind SPOT line and a skygrind-critic
PASS; no Player could be built, so no SPOT and no critic ran. The rows built tonight, all OFF, and how to try each once a Player can be built
(`bash Tools/night/n41-build.sh <tag>`, then `node Tools/night/n46-build-rows.cjs <tag> on n61.<row>`):
| row | what it does when on | its one-row switch | built by, notes |
|---|---|---|---|
| exitFrame61 (S1) | a cleared room's exit gets a lit cyan frame, a light in the passage and a floor line | -skygrindNoExitFrame61 (also off under -skygrindNoExit58) | V, Research/2026-10-22-wave61/V.md |
| bossRule61 (S3) | each campaign boss's fight starts with one line saying what its code rewards, with the button glyph | -skygrindNoBossRule61 | G, G.md (each rule with file:line) |
| text61 (C1) | one centre text block at a time; the last room's ROOM CLEAR goes when the next room's name shows; outside level 1 an instruction shows once a run | -skygrindNoText61 | G, G.md |
| fixture61 (S4) | each ceiling light gets a dark housing and two hanger rails to its joists | -skygrindNoFixture61 | V, V-S4.md |
| gunYellow61 (C3) | THUNDERCLAP, TORCH and BOUNCER lose their yellow plates (the usable cyan) | -skygrindNoGunYellow61 | V, V-C3.md |
Each builder's notes list its reel set, its proof line to grep in the Player log, and its meter.

## (3) Bugs fixed and dead code removed
- **B1, focus60 vs the Player tours: built, not proven.** `SurvivalGame.MayPauseOnFocusLoss` keeps every PlayerChecks mode (the smoke, the
  capture tours, the reels, golden, fingerprint, farm, replay, bench, the two NIGHT29 checks) from pausing on a focus loss, and keeps the focus
  probe's own run pausing (port/stage3 f84ee2e7 test first, 9fa1c72d change, c7f502ca notes; the lead read the diff; reel.sh always passes
  -skygrindReel, which is covered). The EditMode test was **not run** in the Editor; the change commit's message says GREEN, and that GREEN is
  builder D's scratch harness only (stub: 33 fail / 15 pass; change: 48 / 48). The focus probe (doneWhen 2): not checked.
- **B2, Game M's hang: the cause is written, not proven, and nothing is fixed** (Research/2026-10-22-wave61/B2.md). MapChoiceTests waits on
  nothing (a grep of its path finds no blocking call); its 48 map cases are the cost; the synchronous 600 s run_tests limit and the Editor's
  queue behind a running batch are the likely mechanism. The 606 s measured against ~180 s estimated is not explained. The candidate fix is
  the suite's async mode for class M; not written, not run.
- **D1, dead code: 35 batches, 397 lines deleted, 370 net** (Logs/n61/DEADCODE.md, one line per batch with its port/stage3 sha; 52 files under
  Assets; nothing under Core, no Mover.cs, no test touched). Each batch: builder D's re-grep (only the symbol's own declaration and cascade) and
  its compile (0 errors); the lead reviewed batches 1, 2 and 3 and added two follow-ups it found (1b shownRank, 2b navSkip). All together on
  port/stage3: gates GREEN 14:29:22, combined compile 0 errors. **Not run:** the Editor compile, the eleven touched test classes, verbs37, the
  smoke's TITLE step after batch 7.
- **Fixed on the way:** a stale reel name in a comment (f8f69429).
- **The fresh review of tonight's integrated diff** (Logs/n61/review-n61.md; one read-only reviewer, 15:01-15:50, DECIDE61 D-14, standing in
  for the GATE critics that could not run): **must-fix 0, should-fix 2, notes 12.** Verified by reading the code: with every n61 row OFF
  nothing changes what the game does or draws (the hooks return at once; the two installers cost two reference checks a frame); B1 follows
  PlayerChecks.Attach's order; no deleted member is still reached by name, reflection, a UnityEvent or a string switch. The two should-fix
  items were in the boss61 proof reel (it timed the rule line on the wall clock, not the HUD's fixed clock; its 720p check ignored the line's
  alpha): **fixed in 545e9fbc** (dn-check 0 errors; the reel itself not run). The twelve notes are row-ON design points and edge cases (for
  example, the focus probe beside -skygrindBench reads the rule off; the one-row switches disagree on case), for the session that turns each
  row on.
- **The integration evidence** (every pick): gates GREEN after A1 (11:09:17), B1 (12:06:33), S1 (12:40:41), S3 (12:53:38), C1 (13:21:04),
  D 1-22 (13:14:26), D 23-31 (13:41:24), D all (13:55:34), S4 (14:04:13) and the full tree (14:29:22); every combined dn-check 0 errors
  (Logs/n61/gates-*.txt, dncheck-*.txt). The fingerprints stayed EQUAL at every step, so no sim change slipped in.

## (4) What failed, with its numbers
- Every Player step: refused or impossible under 12 GB (the disk numbers above). 0 builds, 0 reels, 0 smokes, 0 SPOTs, 0 critics, 0 rows ON.
- The Editor's Pipeline: no answer from 10:21 to the close; the one wake failed (set_autotick 30 s timeout while focused).
- S5 (the style meter): SHORT, the cause in the sim. B2: SHORT, the cause unproven. S2, S6, S7 and C2: not started (no Player to judge them;
  S2's level data changes a shipped level and cannot ship without one).
- **The lead's own mistakes** (each is now a LESSONS row, L-116 to L-125): a backgrounded `&&` chain lost the output of one cherry-pick and one
  gate run (re-run); a cherry-pick range started from a copied sha re-picked B1 and stopped at a conflict (aborted, re-run with the right
  range: no bad commit landed); a gate file made during that conflict was invalid (deleted, never used); a wrong row name in a message to
  builder V (V caught it); a foreground sleep; one log time written from memory (corrected); a wait loop that tested a branch name; a
  `git worktree remove` the guard refused.
- Builders' slips they reported themselves: V wrote one commit message with a heredoc and made one extra 6 MB scratch (both settled); G's
  S3 file kept a stale reel name in a comment (fixed by the lead); D's notes had two slips (fixed in its notes).

## (5) Not checked
- The Editor compile of anything committed tonight, and every EditMode test: N61SharedTests, FocusAutomation61Tests, N61ExitFrameTests /
  RowTests, N61BossRuleTests / RowTests, the C1 tests, N61FixtureTests / RowTests, N61GunYellowTests / RowTests, and the eleven test classes the
  dead-code batches touch. The builders' scratch harnesses (outside Unity) are their own claims, not the Editor.
- Every Player step: builds, reels, verbs37 (27 / 29 / 31 / 31 / 31 on n60-freeze), the smoke with steals, the focus probe, every SPOT and critic.
- How any built row looks, reads or measures on screen; whether S4's housings read at all; the TORCH's tank bands under C3; what follows S3's
  boss line (THE WICK's own hint may repeat it).
- The WYRM finding's file:line (SurvivalSim.Boss30.cs:286) and the burrower finding were read by the builders, not re-checked by the lead.
- What the review could not check (its own list): the five rows' data on levels other than n57-kiln; whether a test that walks the design block
  rejects bossRule61's string lines under `numbers`; whether the teach slot hands resolved or template text (its N8).
- B2's mechanism. The cause of the Editor's stall (likely the MapChoiceTests run NIGHT60's close left in it; not confirmed). The cause of
  the night's disk use beyond the scratch folders and worktrees.

## (6) Your calls, and the night's (DECIDE61.md has the reasons)
**For GG to decide:**
- **The disk.** Logs/ is 200 GB and Builds/ 70 GB (du, ~11:20); 225 worktrees under `.claude/worktrees`. Until C: has 12 GB free, no night
  can build a Player. The forge's candidates (FORGE61 D-6): the worktrees and `Builds/n60-b1..b8`.
- **The Editor.** Its Pipeline has not answered since 10:21 (likely since 07:44's timed-out test run). A restart is yours.
- **Game M's suite mode** (B2): run class M with `--async_tests true --timeout 2400` and poll, or only in batch mode on a copy.
- **The 56 OFF rows from NIGHT55 or older** (the auditor's "for GG" list in Logs/n61/DEADCODE-candidates.md) and the deferred dead-code
  batches 8-10 (they remove hints NIGHT17 kept on purpose, and two tests), 37-51 (public read-outs; a reel entry) and 52-59 (Core).
- **The style meter** (S5): a measurement reel with a varied kill chain first; then, if you want one-gun chains to climb, an OFF Unity sim row
  (kill awards exempt from the repeat cut, or a lower DULL drain). **The burrower's kill value** under the same rule.
- **The live WYRM line** ("{punch} its head as it comes."): change it after a Player check.
**The night's own calls:** D-1 the no-Player night; D-2 one Editor wake with GG's window given back; D-3 the `--no-editor` path; D-4 Unity code
compile-checked by dn-check; D-5 nothing ON tonight; D-6 the agents; D-7 B2 read before any run; D-8 the D1 scope (1-7, 11-36); D-9 B1's
GREEN is the scratch harness only; D-10 S1 also off under -skygrindNoExit58; D-11 V and G alive, no respawn; D-12 no second wake, scratch
deleted after each check; D-13 the lead's one combined compile with no builder compiling.

## (7) The close lines (NIGHT61-LOG.md "Close" and "Blocked" B-3..B-8 hold each line's output)
- **Started 16:19:21, 6:00 after t0** (printed `date`).
- (a) `bash Tools/night/n61-gates.sh` (under `timeout 480`): config copies identical; golden 39 sets, 48094 records, 22 replays exact, 0
  mismatches; fingerprint plain and --off57..--off61 EQUAL; **the Editor compile step never answered and the 8-minute box ran out (exit 124):
  no "gates: all GREEN" tonight; logged under Blocked (B-3).**
- (b) `bash Tools/night/n41-build.sh n61-freeze`: **refused by the guard**, "4.1 GB free on the project drive, under 12 GB" (B-4); no build.
  `git diff --stat ProjectSettings`: GG's UnityConnectSettings.asset only.
- (c) the verbs37 reels x5 on n61-freeze: **refused by the guard**, the same reason (B-5). No verbs37 line tonight.
- (d) the smoke with planted steals: **not run**, no build to smoke (B-6). No "DONE: all steps passed" line tonight.
- (e) the Game suite and gg-proxy's close ranking: **not run**, the Editor silent all night and no freeze frames (B-7, B-8).
- (f) `node Tools/night/n61-slate.cjs`: **"slate: 13 slices: 0 PASS (passes true), 2 SHORT, 7 BLOCKED, 0 in progress, 4 todo"**.
- (g) the rows' `enabled` matched to this report: the five n61 rows false, focus60 true (since w60): nothing turned ON tonight. The tag
  **u3-n61** is set on the commit that carries these lines.
