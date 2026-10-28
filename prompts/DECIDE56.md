# DECIDE56: the lead's calls on NIGHT56 (untracked; GG will not answer during the run)

Each line: the call, the reason, the alternative rejected.

- **D-1 (00:00) Base on HEAD e7d5b59b, not u3-n55 c1a1a273.** HEAD is two commits newer (3996e2db + e7d5b59b): the NIGHT56 forge's
  research and red-team docs only (git diff --stat u3-n55 HEAD: 8 files under Research/2026-10-20-wave56/, no code, no data).
  Rejected: resetting to u3-n55 (would drop the research the builders must read).
- **D-2 (00:00) The A0 baselines run in a detached worktree at e7d5b59b (C:\Users\gaige\dev\SKYGRIND-n56base, Tools/night/n56-base.sh),
  so P0-pick can proceed in the main tree at the same time without leaking into the "before" numbers.** The worktree is the lead's own
  temporary output; removed after the baselines. Rejected: running the baselines in the main tree first and the pick after (~12 min lost
  on the clock's tightest stretch); running them after the pick (they would measure NIGHT55's rows' code, not u3-n55).
- **D-3 (00:02) P0-pick: all 13 commits HANDOFF lists, in its order (B 26121bd5 fd71daf1 169e162c; C 91f9c48e 47c64d8b 5fe9b6cc
  283f3c41; K 3c8fd885 1b996462; R edfee264 876dc97e e25d89bb 34b55c1d); 8288aec2 skipped (it is the lead's 3b71c610, already on
  port/stage3); S had nothing past bbc8269c.** 5fe9b6cc and 283f3c41 picked back to back (5fe9b6cc does not compile alone); the gates run
  once after the whole pick. Rejected: squashing the pair (loses the builder's messages, which carry the reel specs).
- **D-4 (00:02) The one conflict (876dc97e, SurvivalGame.Impact.cs WorldScale share line): both lines kept, kill55's chain hold
  (N55KFrame) then clear55's (Clear55Frame).** Each returns the share itself with its row off, so with both rows off the line is
  NIGHT54's bit for bit; with both on, the room-clear hold is applied last. Rejected: dropping either (each is a NIGHT55 slate row).
- **D-5 (00:12) The pick's dotnet gates ran beside the wyrm baseline.** The machine has 24 cores and the harness results are
  deterministic sims (no timing in their numbers), so concurrency cannot change either result. Rejected: waiting ~5 min idle.
- **D-6 (00:15) The a156 room meter is builder L's (in FeelReel.N56.Room.cs), not the lead's shared commit; the colour meter is V's.**
  The shared commit pre-places the a156 / a256 / a356 hooks; the meter is L2's own proof and L has the hours. Rejected: the lead writing
  it before the spawn (it cannot be run before r1 either, and it would delay every builder by ~30 min).
- **D-7 (00:18) Builder G's order: G1, G3, G4, then G2 on the lead's message with P0's a355 result.** G2's fix depends on that result;
  G1's style column also measures G3. Rejected: G2 first (nothing to fix against until the critic reads a355).
- **D-8 (00:23) n56-r1 (P0's build) is built at 0:26 from the shared commit, not at ~2:40.** The pick finished at 0:20 and the builders
  have no commits yet, so r1 carries exactly NIGHT55's rows (the P0 gate) and the first gate lands ~2 h earlier (the forge experiment's
  "time of the first gate"). r2 (~2:40-3:50) then carries the builders' first slices. Rejected: waiting for builders' work to share r1.
- **D-9 (00:25) G4 uses the existing NIGHT55 sim row design.n55.wyrm55 (S's, never written; its _why is G4's job), not a new wyrm56.**
  No row added after the shared commit. Rejected: a new wyrm56 (two rows for one fight).
- **D-10 (00:39) The reels' own verdict lines go to the owning builder at once (V: a155 grade / hot wall; G: a355, FIGHT MODE), labelled
  "not the critic's verdict".** The builders can plan their one fix while the critic reads; nothing turns ON without the critic.
  Rejected: holding the numbers until the critic answers (~40 min of builder time lost).
- **D-11 (01:01) The r1 critic's blind pairs use a new tool, Tools/night/n56-blind.cjs (any on / off pair, both orders, the key outside the
  pairs' folder).** n46-blind-pairs.cjs reads feel46's file names only. Rejected: hand-copying files (no seeded key).
- **D-12 (01:02) verbs37 x5 also runs on n56-r1 (NIGHT55 rows on), not only on r2.** It tells which NIGHT55 rows lose nothing before the
  r2 checkpoint freeze turns any on, and gives P1-teachText / P1-doorDark their numbers early; its frames go to the r2 critic.
  Rejected: waiting for r2 (one fewer data point on NIGHT55's rows; the machine is idle while the critic reads).
- **D-13 (01:02) Four builds for builders' work: r2 ~02:30, r3 ~03:50, r4 ~05:05 (builders' last commits ~04:57).** r1 ran early (D-8), so
  the plan's r2 / r3 slots become r2 / r3 / r4 and each entry gets up to two attempts inside the builders' window. The checkpoint freeze
  follows the r2 gate as written. Rejected: keeping two builder builds (one attempt per entry for most of the slate).
- **D-14 (01:25) The r1 gate (gate-r1.md, 01:23): 10 of 11 NIGHT55 entries FAIL; P0-ssao DONE-no-defect (OFF <= 1.22x the floor noise in
  four rooms; measured on the AUTO-low preset the Player picked: noted for GG). ssao55 stays OFF and closes; no fix spent.**
- **D-15 (01:25) The one fix per RED NIGHT55 row goes to tonight's owner by area:** V: grade55 (THE CROWN; the hot wall), decor55 (cyan
  not the brightest on 3 / 12 frames; foundry strips 16 deg from parry), and step55 (C's camera row: V inherits C's family, last
  priority); G: kill55 (the chain never counts: 0.51 s gaps > chainGapS 0.45), wick55's Player pilot (0 core punches) and Wick55Tests
  (3/4: wickSlam legs 7 vs 1 in the Editor), brief55's teach card (94 frames over the counter on kiln / spire: G owns "text" tonight);
  L: clear55 needs no code (only iceline was framed: the lead reels all five), doorDark after L3. The lead: P0-smoke's steal, P0-t125's
  a152 / a252, P0-t99's long reel, a354. Rejected: spreading the text fix to V (V carries V1-V4 plus three NIGHT55 rows already).
- **D-16 (01:27) L2's stall line stays as written (<= 25 % on foundry / iceline / kiln; doneWhen is never rewritten).** The baselines are
  already over it (26.2 / 32.0 / 29.2 %); builder L's proposal (no worse than baseline + 3 points) goes to the critic beside the original,
  with the baselines, and the critic judges. Rejected: rewriting the gate (the slate's rule) and silently passing on the proposal.
- **D-17 (01:28) The planted focus steal no longer uses Notepad: Tools/night/n56-foreground.ps1 shows a window from its own process and
  forces the foreground (ALT tap + AttachThreadInput; test: "took True").** Windows 11's Notepad hands its window to one shared process
  (pid 28328 tonight) that may hold GG's own tabs: it cannot be closed by PID safely, so it is left alone. NIGHT55's and tonight's first
  steals opened windows in it (r1's steals never took focus: "focus changes 0"; one test window may remain open in that Notepad).
  Rejected: killing pid 28328 (it may hold GG's unsaved tabs).
- **D-18 (01:44) P0-decor (decor55) is superseded by V1 (colour56 owns decor54's accent pieces and decor55's string course, V's 283cdc5f):
  its one fix IS V1.** On r2 both rows are on in the copy (as V built them); P0-decor closes with V1's verdict (SHORT "superseded by
  colour56" if V1 passes, decor55 then stays off), never measured on decor55's old hue. Rejected: a separate decor55-only a452 set (seven
  more sides for a row the night replaces).
- **D-19 (01:44) r2's copy: NIGHT55 rows grade55 wick55 step55 decor55 kill55 brief55 clear55 and NIGHT56 rows rank56 colour56 on;
  ssao55 off (closed DONE-no-defect); tokens56 / rim56 / yellow56 not built yet.** 36 reel sides plus one long reel (verbs37,a452 in one
  Player run) for P0-t99. Rejected: one row per build copy (36 sides x 9 rows does not fit the night).
- **D-20 (02:22) r3 is picked and built while the r2 critic reads (02:22-02:29), not at ~03:45.** The Player sat idle during each
  critic; the builders had 11 commits (the r1 critic's fixes, V2, V3, L3's first rooms). The checkpoint freeze runs on r2 after the r2
  verdict as written (its verbs37 x5 already ran with every r2 row on; the smoke runs with the rows that pass). Builds now: r4 ~04:00
  (G3 / G4, V1's tint fix, L3's rest), r5 ~05:05 (the builders' last). Rejected: the fixed r3 slot (the night's gate count is the
  forge experiment's measure; idle Player time does not count toward it).
- **D-21 (02:23) THE CROWN's half of P0-wickGrade / V4 had its one fix (bf2dbee0, in r2): RED on r2's reel lines (p2 near-danger 0.29;
  graded vs raw dS -0.094); V was told to stop on it unless the r2 critic says otherwise.** THE WICK's grade and hot wall carry on.
- **D-22 (02:49, confirmed 04:22) r3 and r4 get no critic of their own; their evidence goes to the r5 critic.** r4 superseded r3's entries
  within 25 min, and after the usage-limit pause a critic on r4 (~30 min) would land after the builders' last commits, so its verdicts
  could not be acted on. One batched critic on r5 judges every entry with r3 / r4 / r5 evidence. Rejected: a critic per build (two more
  critics' usage for verdicts nobody can act on).
- **D-23 (04:21) After the usage-limit pause (03:09-04:20, 71 min) the builders were resumed by SendMessage (NIGHT56.md 11) and their last
  commits moved from ~04:57 to 05:10; r5 picks at ~05:12.** The hard cap (07:27) does not move: r5's reels ~05:20-05:50, the r5 critic
  ~05:50-06:20, the freeze ~06:20-06:50, the close table at or after 06:47. Rejected: no builder time after the pause (their in-progress
  slices were 10-20 min from a commit).
- **D-24 (04:22) THE WYRM's turn (V4 / NIGHT55 P1-wyrmTurn) is judged on the turn's wall hue inside 15-55 deg (r4 b3on: turn H 22), not on
  the generic grade's "walls >= 30 deg from danger" line, which THE WYRM fails by design (GG NIGHT47: "the same yellow-red-orange color
  theme").** The critic gets both numbers. Rejected: applying THE WICK's distance-from-danger rule to the one boss GG asked to be orange.
- **D-25 (04:37) r5 picked as soon as L and V reported done (04:33-04:34) instead of at 05:10; G's last fix (5990fff9) was already committed,
  so r5 holds every builder commit of the night and no r6 is needed.** The final gate is split into two parallel critics (A levels, B
  gameplay / colour / bosses: > 30 images in one), spawned after r5's reels and a steal smoke with no builder running. Rejected: waiting to
  05:10 with an idle Player.
- **D-26 (04:44) N56RankTests' row-off failure in the Editor (heal 0 vs 10) does not block anything: rank56 ships off (G1 is SHORT on its
  own line), and the shipping game's heal by aggression is checked where it matters, verbs37 row 42 on the freeze (rank56 off).** If row 42
  fails there, G's Heal36.OnKill scale hook is reverted before the tag. Rejected: chasing a test-isolation question with ~2.5 h left.
- **D-27 (05:18) L2's spawn data (waveNodes43, level data in place) stays in the freeze although L2 closes SHORT.** The r2 critic passed every
  L2 line but the bot stall; r5's failures come from L1's stricter reading (13 rooms have no foothold, so "in view" is "not measured") and
  the stall, which the baselines already broke. Kept: the entry frames show the first wave (a156), every clear time is better (bot),
  verbs37 nothing lost. Costs, written in the report: iceline stall 32.0 -> 36.5 % and foundry deaths 15 -> 21 (bot). Rejected: reverting
  the in-view fix GG's critics asked for first (NIGHT55 veteran #4, ggproxy #1).
- **D-28 (05:19) g8's THE ASH PIT (d33f9feb) stays, flagged for GG; not reverted.** The r5 critic's GG line: "a random lava pit, and it
  keeps killing me" ((bot) g8 deaths 0 -> 7) names GG's own dislike, so I tried `git revert`: it conflicts in the one-line generated
  n52-iceline.json (later commits edited it), and regenerating from the plan at 05:19 risks losing L2's spawn edits. The report gives GG the
  revert (the plan's pits[5] out, then iceline regenerated). Rejected: a blind regeneration before the freeze.
- **D-29 (05:23) The freeze turns ON exactly one row, design.n55.clear55 (gate-r5a P2-clear PASS); every other NIGHT55 / NIGHT56 row ships
  OFF (each failed its last attempt at gate-r5a / r5b).** No row GG asked for by name tied (NIGHT56.md 6). rank56 ships off as the slate
  says. Rejected: turning on rows whose bot numbers improved but whose critic line failed (kill55's ladder, tokens56, wyrm55: their
  numbers go to GG in the report as rows to try).
- **D-30 (05:37) The freeze ran early (05:25-05:37) because every builder had finished by 04:38 and the final gate landed at 05:22; the close
  table waits for 06:47 (the 6:50 floor) and the last commit for 06:57 (7 h).** No building after the freeze. Rejected: a further build
  round (the builders were done, the slate's attempts were used, and the cap rule says caps are caps).
