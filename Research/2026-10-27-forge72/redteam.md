# FORGE72 red team (2026-10-27, read only): NIGHT72.md and its planning files against the tools and the code

Method: read NIGHT72.md, DECISIONS, harvest, spike, builder-brief, slate-seed.json, GOAL.txt, gg-proxy and CLAUDE.md in full, then the
tools and code they name. Ran only read-only commands: `node Tools/night/n72-slate.cjs Research/2026-10-27-forge72/slate-seed.json`
(54 slices, exit 0), `n72-close-check.cjs --selftest` (37 of 37), `n72-freeze-rows.cjs --selftest` (5 of 5), `n72-rows.cjs --dry`
(11 rows, anchor found once), git log / grep / cmp, one statfs. No Player, Editor, build, dotnet or test was run, so every "likely" below
is an inference from the code path and says so. Line numbers are the file's own (grep -n).

Ranked by how much run time each would waste. 14 findings.

---------------------------------------------------------------------------------------------------------------------------------

## 1. The close can end with no tag, so the /goal cannot be met (Stops; the whole close, 6:00-7:30, and every turn after the cap)
- `Tools/night/n72-close-check.cjs:131-132` needs `verbs37: PASS: total N/N` on every side vf vn vi vk vs vp (vn when foundry65 is ON).
  `:191-194`: `--tag` exits 1 on any failed line unless `--minimum` AND the close started at 7:00 or later (or the clock is past 7:30).
  NIGHT72.md:293-298 (g, h) has no branch for "--tag refused", and the final message `NIGHT72 CLOSED` needs a tag.
- The known REDs the plan walks into:
  - NIGHT72.md:262 runs `vf,n52-foundry,verbs37` with foundry65 ON (D-4, slate LV-foundry65). Once the row swaps the level, the old id is
    not in the campaign order, so `SurvivalSim.Move35.cs:70-76` gives campaign place 0, `FeelReel.N35.Verbs.cs:632-636` counts the grapple as
    owned, and rows 17 / 19 / 22 / 23 go RED. Same path as `Logs/feel/n71-p2-on/summary47.txt` vs2 (n52-iceline 25/29; vs3 with
    `-skygrindOff65` 29/29). Never measured for foundry: n71 closed with all four level rows OFF in the repo, so this is "likely", not measured.
    NIGHT72.md:149-151 states the problem for the guards in one breath and keeps the n52 labels in the list in the next.
  - Row 12 (bunny hop) is RED on n52-iceline and n57-kiln at HEAD (`Logs/feel/n71-polish8/summary47.txt` vi 28/29; n71-polish4 vk 29/30). The
    n67 maps under the ULTRAKILL profile were never run (n71-play predates it). N1 (25 min) must land and be picked first.
  - A flaky row: n71-p2-on vk n67-kiln RED [9] 29/30 (3 rides, "corner"), PASS 31/31 in n71-play and p1-on.
- Fix: (a) NIGHT72.md:262 and every n52 guard whose row is ON: `vf,n52-foundry,verbs37,-skygrindRows72,foundry65:0` (the vs3 recipe).
  (b) Run the exact close set (vf vn vi vk vs vp) at T1 and T3, not first at 6:20. (c) Add to 9(g): if `--tag` refuses, `git revert` the last
  flips, rebuild n72-freeze, rerun the reels once; if still RED at 6:55, write the RED rows under `## Blocked`, re-run `--start` at 7:00,
  recommit the close files, `--tag --minimum`. That is the only path the script allows before 7:30; today it is undocumented.

## 2. The plan ignores the measured usage window (Stops / Sizing; up to 2.5 h with the ON count near zero)
- Memory `night-usage-window.md`: NIGHT66-68 stopped at 2:20 / 2:25 / 2:24 of agent work and returned after 2:37-2:47 or past the cap.
  NIGHT72.md:95 (four builders 0:20-6:00) and :143 (T1-T4 at fixed clock times) assume continuous work. NIGHT72.md:31 and every HV doneWhen[2]
  (slate-seed.json:67 and the like) flip a row only after "the next train's guards", so with a stop near 2:20 (inside T1) maybe 0-10 of
  the 22 harvest rows and none of the 4 level rows are ON; T3 and T4 can be lost. The target (b)/(a) of section 2 is the part that needs no agent.
- Fix: end the H train with ONE combined guard run (all rows that passed their pair, `:1` together, plus verbs37 vf vn vi vk vs vp and the
  smoke) and flip them from that single run by ~1:30; flip LV rows the hour N0 lands. Start a train when two or more builder commits wait,
  not at a clock time. Commit PLAY72 / DECIDE72 / report skeletons before 2:20. Add to section 8: after a stop the train in progress is the
  next one; missed trains are not replayed.

## 3. Train capacity, an unpairable builder tail, N's 400 minutes, L's order (Sizing; about 220 builder-minutes plus 25 lead-minutes per train)
- One train: picks 5 + compile 3 + touched-class tests ~8 (`unity-loop.sh tests` takes one substring per call) + build 3 + reel set
  (9 guard sides ~10 min, pair sides ~70 s each) 25-35 + smoke 5 + sheets / key files / spotter 10-15 + flips ~3 each (verdict Write,
  slate-set with passSha and passFiles, freeze-rows, commit, cached check) + STATE + four SendMessages = 90-100 min, not 70 (NIGHT72.md:143).
  T1 carries up to 22 harvest flips; slate-seed gives each HV slice 6 min (132 total).
- T4 starts 5:05 and so ends ~6:40, after the 6:00 freeze (9a). A pick after T4's pick has no pair train (D-1 (a)); the freeze set (9b) runs
  guards only. So builder work from ~5:05 to 6:00 (4 x 55 min) cannot become an ON row; section 7's leftovers have the same limit.
- Slate estimates: N = 50+25+60+80+45+60+40+40 = 400 min against 340 available (0:20-6:00); N6 and N7 are never reached. L, C, B are 320 / 280 /
  285. L's slate order L1 (60) then L2 (70) puts L3 lit72, ranked first by value (NIGHT72.md:223), third (~2:30); L2 needs L1's numbers, which
  need a lead train (1:35 at the earliest); `builder-brief.md:40` makes "two build rounds" SHORT, i.e. 140 min.
- Fix: three pair trains plus the freeze, started on events; last pair pick 4:50; after that builders do meters, audits, docs, leftovers that
  need no ON row. L order: L1, L3, L4, L6, then L2 (after L1's numbers), L5. N order: N0, N1, N5, N2, N3, N4, N6, N7 (see 10). HV est 10-12.

## 4. N0's last doneWhen cannot be done as written, and D-10 forbids the only tool that does it (Steps; N0's 50 min plus the 4 LV slices)
- slate-seed.json:1089 "slots65.json / slots67.json read new for exactly those rooms". The only writer is `Tools/night/n65-slot-set.cjs`
  (header lines 2-8): a slot goes new only after a room-GATE slice with `passes true`, a passSha, a verdict whose first line is
  "<room>: PASS", and the room file committed and unchanged since. NIGHT72.md:75 (D-10) puts "the room GATE as a verdict" out of scope, and
  Logs/n72/slate.json has no b1 / a8 / g4 / kb2 / kb3 / kb3c / kb3d / v6 slices. `gen-n65-foundry.cjs --slots=<ids>` (list mode, what n71's
  play build used) needs none of it, but `--slots=<slots65.json>` mode refuses an unbound "new" slot.
- Related: DECISIONS D-4 says flip the four level rows "together"; the slate has four separate LV slices with separate doneWhens. One RED level
  (kiln was RED once) would hold all four or none, by reading.
- Fix: replace the line with "slots65 / slots67 stay all old (list-mode generation, as n71's play build); the level files carry the rooms; the
  close's ROW line prints SLOTS old", and say each LV row flips on its own result.

## 5. Pre-launch state: no FORGE72 commit, the tools and the brief's inputs are untracked (Steps; 10-20 min per builder, certain)
- `git log` HEAD is 646ccc818 (no FORGE72 commit; NIGHT72.md:111 expects one). `git status`: `?? Tools/night/n72-*`, `?? Research/2026-10-27-forge72/`,
  ` M .claude/hooks/night-resume.cjs`. Worktrees branch from t0b, so they hold none of it: `builder-brief.md:36` runs
  `bash Tools/night/n72-gates.sh --no-editor` (absent), `:47-50` reads `Research/2026-10-27-forge72/...` (absent), and NIGHT72.md / `Logs/n72/slate.json`
  (untracked / gitignored) cannot be read by `git show port/stage3:<path>`, which `:25` prescribes for the main tree.
- Fix: before launch commit, by explicit paths, `Tools/night/n72-*` and `forge72-make-n72.cjs`, `Research/2026-10-27-forge72/**`, the hook. Keep NIGHT72.md
  untracked (CLAUDE.md rule 1). In the brief: "NIGHT72.md and Logs/n72/slate.json are read by absolute path, read only".

## 6. The close's flow gate will fail on the night's own geometry fixes (Hidden cost / Stops; found at ~6:20, 8-10 min per retry)
- `n72-gates.sh:28-34` compares n46flow on the five n52 / n57 levels (`--off67`) with `flow-base.txt`, made at 0:25 (NIGHT72.md:120). N2
  (slate-seed.json:1131, "fixed in the plan, regenerated" on the five shipped levels), L5's fixes, C5's wall changes and C3 all touch level data. The
  script's own comment says a level-data slice must re-make the base in its commit; nothing in sections 4-9 says who or when. `--off67` also leaves
  foundry65 (night 65) ON after LV-foundry65 flips; n46flow's lines for n52-foundry with the swap on were never compared (not checked).
- Fix: in the pick step, after any pick that touches `Design/levels/n52-*` / `n57-*`, run `n72-gates.sh --flow-base` (5 min) and commit the new base;
  run `--flow` once right after LV-foundry65 flips; or restrict N2 / L5 / C5 geometry to the n65 / n67 files and mark shipped-level escapes SHORT.

## 7. Harvest mechanics (Sizing / Ambiguity; 60-90 min)
- Every HV doneWhen[0] (e.g. slate-seed.json:65) reads "row ON vs -skygrindRows72 colours63:0". The repo config has every harvest row OFF at h0, so
  a default side is OFF; the ON side needs `:1` (NIGHT72.md:163 says so). Followed literally every pair is OFF against OFF and the spotter answers
  NOTHING OBVIOUS for all 22 rows.
- Only 7 of 22 are in FrameRows (`SameRun63Math.N63D.cs:28`: colours63, goal64, voice71, words71, wyrm71, cyan71, kicks71). The other 15 need two
  sides at different instants (L-156). The n40-n45 rows (reticle42, pump40, select41, models41) do not go through `NightRows.ViewOn`
  (nn >= FirstNight 46), so only Rows72's config write reaches them (harvest.md's last paragraph leaves the grep to the lead).
- 13 of 22 have an owner slice that rewrites the same code (colours63 C1; goal64 / yellow56 / decor55 C4; fixture61 L6; exit63 N6; wyrm71 B1;
  voice71 / words71 / bossText55 B3; hitzone47 / models41 B4; kicks71 / finish55 B5; eye63 / fov55 / step55 N7). `n72-freeze-rows.cjs:66-73` refuses a flip
  when passFiles changed since passSha, so a row paired at h0 and reworked before T1's flip needs a second pair. Builders start those reworks at
  0:20, before the H pairs (0:25-1:35) say whether the row has a defect at all.
- Fix: HV doneWhen: "ON side `:1`, OFF side `:0`". The H train flips every passing row first; an owner starts a rework only for a row with a defect.
  Builders open with non-harvest slices (L1 L3 L4; C2 C3; B2; N0 N1).

## 8. Shared hooks are not pre-placed (Steps; a cherry-pick conflict per train, times four builders)
- New reel phases (still72 and `-skygrindFlickerPlant72` L1, `-skygrindStuck72` N3, B's boss-kill reel for HV-finish55) must be registered in
  `FeelReel.N27.cs:25` (IsPhaseN37 chain) and `:218` (dispatch) and `FeelReel.N57.cs:15` (one-line `PhaseN57` array). L and B own none of those
  files (`builder-brief.md:14-24`; N owns only "FeelReel.N37* and new reel phases you add", :21), and `:24` says never edit a file outside your list. L1, the
  first L slice and L2's input, cannot ship its phase without the lead's edit. The shared commit (NIGHT72.md:123-134) pre-places only design.n72
  and FrameRows.
- `n72-rows.cjs:37` writes one line per row; four builders expand adjacent lines in two config copies, so every pick conflicts (NIGHT72.md:99-100
  accepts hand re-application).
- Fix: add to the shared commit `FeelReel.N72.cs` (a `PhaseN72` array, one hook in N27 :218) with partials `FeelReel.N72.L.cs` / `.B.cs` / `.N.cs`; make
  `n72-rows.cjs` emit each row as its own multi-line block with a spacer line.

## 9. Caps are unmetered and the plan already spends them; "all caps spent" ends in an early freeze and two idle hours (Hidden cost / Stops)
- NIGHT72.md:194: builds 9, reel sides 160, smokes 8, spotter runs 40. No tool counts them (`n72-reels.sh`, `n41-build.sh` do not). Plan, my count: H ~30
  sides (:164); guards 5-9 per train x 4 = 20-36; freeze 6 + play 3; pairs for ~19 rows x 2-3 = 40-57; L2's matrix (5-6 sources x 5 levels, plus
  ON / OFF) ~40; lab / telemetry / kill pairs: about 170 before one retake (rule 7 "bolder, retake"). Spotter: 22 + 11 + retakes > 40. Smokes: 4 trains +
  freeze + play + the LV doneWhens "the smoke DONE with the row ON" (4) = 10 unless the rows flip in one train.
- If the caps bind near T3, every remaining slice goes SHORT, NIGHT72.md:236 prints `LIST EXHAUSTED` and says "start the code freeze at once", but
  `close-check.cjs:78` refuses `--start` before 6:30 and `:92-93` wants the clock stopped at 7:00-7:30: two or more forced idle hours against
  "Never idle" (:34, :80).
- Unlisted costs: pair preparation by hand (mkdir, two cp, a random A / B, key.txt, sheet.py) ~1.5 min x ~35 pairs ~ 50 min; L1's meter writes 60
  rendered frames per pose and `n72-prune.sh:18` only deletes `f[0-9]*.jpg`; the disk read 10.7 GB free at 09:39 today (guard floor 12 GB,
  `guard.cjs:27`) and 38.6 GB a minute later (spike.md S6: 52 GB at 08:55), so a transient dip refuses a build and the prompt files it under Blocked.
- One more disk number from the finished `du`: `.claude/worktrees` already holds 17 GB (55 registered worktrees, old agent-* trees left from earlier
  nights, which `guard.cjs` forbids removing: GG's call); the four n72 worktrees add to it, and C: read 35 GB free at that moment.
- Fix: `Tools/night/n72-caps.sh` (count `^== ` lines in `Logs/feel/n72-*/summary47.txt`, `Builds/n72-*`, `Logs/n72/spot/*`) printed by the clock
  line; raise sides to ~220 if the disk allows or cut L2 to 3 poses x 5 sources on one build; make LIST EXHAUSTED mean "take the next owner's
  leftover, then the close drafts", with the freeze staying at 6:00; add `n72-pair.cjs <row> <on.png> <off.png>`; the meter keeps numbers and
  heatmaps only; on a disk refusal wait 60 s and recheck once before logging Blocked.

## 10. Movement order and the instrument (Contradiction / Sizing)
- Rule 6, NIGHT72.md:103: "A bar is never loosened after a measurement." N1 (slate-seed.json:1109) changes the bunny-hop row, measured RED, to count
  hops "over the profile's air time (3 hops) or by hops kept": the second clause is a loosened bar, and an instrument change after the RED (L-150 /
  L-152 in CORE.md).
- N5 (profile: ground friction / accel ~36, air accel ~3.6; D-7) is ordered after N1 (hop window), N2 (reach audit with the 4.1 m double jump) and N4,
  all of which depend on those numbers; air accel 1.5 to 3.6 changes hop counts, reach and every verbs37 row after they were fit. After any profile
  change `FingerprintBaseline.Ultra71Hashes` must be re-recorded (N5 doneWhen, D-7) or the smoke fails its two baseline comparisons (the HEAD~4
  commit 2d59f04a8 shows smoke 32/34 for this reason): one more build, two Player runs and a commit, in no train step and no build count.
- Fix: N5 first (lab, then a profile decision or "no change" by T1), then N1 (widen the window to the profile's air time, keep 3 hops, drop "or by
  hops kept"), then N2. Last profile commit by T3; list the baseline re-record as a numbered step in section 5.

## 11. The close window (Hidden cost / Sizing)
- NIGHT72.md:274 plays `tf,n52-foundry,tour65` with foundry65 ON: the Player loads the shipped level, not GG's n65-foundry (the sibling lines use `<map>`),
  so gg-proxy's "GG's preview frames" would show a level GG does not play. Use the loaded level.
- The report asks for "flicker share halved on the five levels" and boss frames at or under 15 % "in n72-play" (NIGHT72.md:63-64, :316-322), but (d) runs
  only tf tour65 and bp / bk boss63: no still72 on five levels, no foundry / iceline / span boss. Add 5 still72 and 3 boss63 sides to (d), or word
  the aims "measured at T3".
- `guard.cjs:301-303` blocks every Agent at or after 6:45; (d) ends ~6:40 at best (6:30, cp, a 4-minute smoke, 3 sides), so gg-proxy
  (NIGHT72.md:285) has a 5-minute margin. 6:30-7:00 must also hold PLAY72, DECIDE72, the 8-part report with E72's numbers (no tool computes "the longest
  gap with no agent running"), HANDOFF, wave-memory harvest, the NIGHT73 seed, night-context close, the commit and the tag. (h) says stop the clock at 7:00
  or later: if all is done at 6:50 that is a wait, against "never a sleep loop" (:82); use Monitor until 7:00.
- Fix: draft PLAY72 / DECIDE72 / report / HANDOFF / seed during 5:10-6:00 and commit them (close-check `files` needs only that the last commit is after
  6:30); start gg-proxy at 6:35 on the frames that exist; add a one-line E72 script to `n72-clock.cjs`.

## 12. Overlapping rows and a label swap (Contradiction)
- cam72 (slate N4: slide eye drop 0.32 m, dash FOV x0.95 / x1.1) against eye63 (eased slide eye over the web's 0.70 m `slide.cameraDrop`) and fov55
  ("world FOV follows horizontal speed"; off-why.txt): three rows drive the same eye height and FOV, each pair is ON against OFF alone, and the stack
  is first seen at the freeze build. D-7 also says "FOV stays 74".
- wyrm71 against body72 ("wyrm71's numbers fold in", NIGHT72.md:209); voice71 / words71 / bossText55 against btext72 (:211); hitzone47 / models41
  against spring72 (:212): the HV slice flips the old row, the builder slice rebuilds the same surface, and no precedence is stated.
- `n72-rows.cjs:29` labels spring72 "B3" and `:31` btext72 "B4"; NIGHT72.md:211-212 and the slate say btext72 is B3 and spring72 is B4 (the label is
  copied into the config's `_why`).
- Fix: one row per surface: cam72 supersedes eye63 and fov55 (keep step55), body72 supersedes wyrm71, btext72 supersedes voice71 / words71 / bossText55;
  fix the two labels before running `n72-rows.cjs`.

## 13. Ambiguities two careful engineers would split on
- `DECISIONS.md:23` (D-3) joins rows with `,`; NIGHT72.md:127 with `+`. `n72-reels.sh:16` splits a spec on commas, so the `,` form breaks inside a reel spec.
- `-skygrindNo<Row>72` for a harvest row: `-skygrindNoColours6372`? The test names only palette72; the PLAY72 "off switch" column needs one rule (use
  `-skygrindRows72 <row>:0` for any row older than 72).
- NIGHT72.md:126 says apply Rows72 "in SurvivalGame.Init"; `GameConfig.Load` and `MovementTuning71.Apply` are in `Awake` (`SurvivalGame.Init.cs:25-26`);
  `Init(config, map)` (:75) runs after `Bright71.Create` has read the config, so the literal reading puts the call too late.
- NIGHT72.md:149-150 keeps `vi / vk / vp` on n52 labels and adds the n67 maps under the same side names: the second run overwrites
  `Logs/feel/<tag>/vi` and `Logs/player/n47-reel-<tag>-vi.log` (`n72-reels.sh:24`).
- "fingerprints EQUAL for view rows" (NIGHT72.md:31, D-1 (c)): no train step runs a fingerprint (steps 1-8); `n72-gates.sh` runs only for builders and the
  close, and the SimHarness fingerprint cannot see a view row. Either add the Player `-skygrindFingerprint` to the train or drop the clause.
- C5 (slate-seed.json:946) "a window to the next room" against its doneWhen "no collider change" (:953); N6's iceline line is an either / or.
- LV doneWhen "the smoke DONE with the row ON": step 6 (NIGHT72.md:153) passes no `-skygrindRows72`, so the smoke runs the repo's config.

## 14. The weakest new feature: body72, with the enemy half of spring72 (the change GG is most likely to reverse)
- GG, 2026-10-26 (OWNER.md:118): "the boss fight is fine". B1 redraws that boss: tapered segments, a head with a jaw and eyes, a tell glow
  (slate-seed.json B1; NIGHT72.md:209). GG, 2026-10-13 (OWNER.md:109): "keep the worm AI the same"; 2026-10-16 (OWNER.md:111): keep "the pill-shaped enemy
  concept". CLAUDE.md rule 7: GG owns enemy design; models are the NIGHT43 pill language; `builder-brief.md:27` lists "enemy design (GG's)" under Never,
  and the same brief gives B1 a boss model redesign and B4 spring motion on every enemy.
- The bar rewards size, not taste: "the wyrm's own pixels >= 3x OFF" and a spotter that "names the worm". Nothing asks whether it still reads as the
  pill language he approved. The ON flip then changes the one boss he called fine, with the off switch only on a command line.
- Second most likely: palette72 ("one accent per level", "no level's top hue over 60 %") against GG's "it all needs to be unified" (OWNER.md:116) and
  the dusk palette (CLAUDE.md rule 8). Least likely: N5's friction change, because he asked for ULTRAKILL's movement "almost one to one".
- Fix: ship body72 as a restyle inside the pill language (same silhouette family, secondary motion and a tell glow only), or leave it OFF and list it in
  DECIDE72 with a pair; a restyle he can compare in PLAY72 is cheaper to keep than to revert.

---------------------------------------------------------------------------------------------------------------------------------

## Checked and sound (no action)
- The 54-slice seed passes its own tool (exit 0, 0 PASS / 54 todo); the close-check and freeze-rows self-tests pass; `n72-rows.cjs --dry` finds its anchor once.
- `n72-gates.sh` gate strings exist (`golden total ... 0 mismatches`, `fingerprint seed ... EQUAL`); `n46flow` accepts `--off67` through `NightRows.OffFrom`.
- Reel scenarios named in the prompt exist (tour65, boss63, a157, kill12, headshot12, verbs37, taps71, `-skygrindClearShots60`, `-skygrindStand71`,
  `-skygrindSameRun63`); new ones (still72, `-skygrindFlickerPlant72`, `-skygrindStuck72`) are the builders' to write.
- The guard's clock rule matches `n41-build.sh`, `n72-reels.sh`, `n56-smoke-steal.sh` with tags n72-freeze / n72-play; the smoke at HEAD reads DONE
  (`Logs/player/n71-polish8-smoke.log`); the steal log name matches close-check's `n56-smoke-steal-n72-freeze.log`.
- Rows72's design (config write plus ReelOverride) survives the same-run reel: `FeelReel.N63Same.cs` removes the override after a hold and the config value
  (also set by Rows72) takes over. The `GameConfig.Get` cache note (GameConfig.cs:64-73) is right.

## Not checked
- Any Player, Editor, build, dotnet or test run. Finding 1's vf result under foundry65 ON, finding 6's n52-foundry flow lines and finding 10's
  n67 bunny-hop result are inferences from the code and the n71 logs, not measurements.
- Whether `n65-slot-set.cjs` could be satisfied with an `--slate` file of room slices (finding 4's second option) beyond reading its header.
- scout-*.md, REQUEST.md, RESEARCH.md and IDEAS-POOL.md (not in the review list).

## The forge's fixes (2026-10-27, after the review; every finding fixed in the files)
1. NIGHT72.md section 5 step 5 defines the close's sides: vf runs with `foundry65:0` once that row is ON, and the n67 maps are used when
   their rows are ON. They run from the first train after H. Section 9 (b2) adds the RED-at-close branch: bisect, n72-off-row.cjs (new,
   tested in tools-test.txt), a re-run, then the script's minimum path. N1 is ordered after N5 and checked on the n67 maps too.
2. D-12: the H train ends with ONE combined guard run and flips by about 1:30. Trains start on events. Skeletons of PLAY72, DECIDE72 and
   the report are committed before 2:20 (section 8), and missed trains are not replayed.
3. A train is about 90 minutes (section 5). There are three pair trains plus the freeze, and the last pair pick is at 4:50. After that,
   builders do work that needs no new pair (section 6). Slate orders are now L: L1 L3 L4 L6 L2 L5; N: N0 N5 N1 N2 N3 N4 N6 (N7 dropped,
   N 360 min); C and B open with new work. HV estimates are 8 min, batched with the pair tool.
4. N0 keeps the slots files all old (list mode, as n71's play build), and the ROW lines say so. Each LV row flips on its own result (D-4).
5. The FORGE72 commit holds the tools, the planning set and the hook. The brief reads NIGHT72.md and Logs/n72/slate.json by absolute path.
6. Section 5 step 3: a pick that touches n52 / n57 level data re-makes the flow base, with the reason logged.
7. The HV doneWhen pairs `:1` against the plain config. FrameRows covers only seven rows (harvest.md). Owners rework a harvest row only
   after its H pair shows a defect. eye63 and fov55 are superseded by cam72 and not harvested.
8. The shared commit pre-places FeelReel.N72.cs (partials still72 / stuck72 / bosskill72 in PhaseN57). n72-rows.cjs writes one block per
   row. Builders keep numbers in UnityTuning.N72<letter>.cs, so the config is the lead's alone; N's design.ultra71.set line is the one
   exception.
9. Added n72-caps.cjs (counted at every clock check; caps are now 10 builds / 220 sides / 10 smokes / 50 spotter runs) and n72-pair.cjs (the
   pair, the blind copies, the sheet). LIST EXHAUSTED no longer moves the freeze. On a disk refusal the lead waits 60 s and checks once more.
10. N1 keeps the bar (3 hops, speed kept) and widens only the window. N5 decides the profile by the first train after H, and the last
    profile commit is at the third train. Section 5 step 7 re-records the baseline.
11. Section 9 (d) runs the play reels on the levels GG plays. The report's meters are the third train's, named as such. gg-proxy starts at
    6:35. The drafts are written 5:10-6:00 (a0). Monitor waits until 7:00 if the close is done early.
12. Rule 7 sets the precedence (harvest first, new rows on top; cam72 supersedes eye63 / fov55). The labels B3 / B4 are fixed in
    n72-rows.cjs.
13. D-3 joins rows with `+`. `No<Row>72` is for the night's rows only. Rows72 runs in Awake after the load. Side names are unique per map
    in a set. The fingerprint is the smoke's baseline check. C5 has no window. N6's iceline line is decided (`iceline67:0` on the shipped
    map). The LV smoke runs with `:1`.
14. D-11: body72 is a restyle inside the pill language (presence, springs along the pill segments, a tell glow), with no jaw and no eyes.
    spring72 moves models and does not reshape them. DECIDE72 names body72 first.
