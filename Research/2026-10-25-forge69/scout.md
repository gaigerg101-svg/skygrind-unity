# FORGE69 code + data scout (read-only; 2026-10-25 ~09:20; HEAD d82f6b09, branch port/stage3)

Read: NIGHT68-REPORT.md (all), T/open.md, R.md and S.md (in their worktrees: .claude/worktrees/agent-a9d0ffb6d0dfdb2be/Research/2026-10-24-wave68/R.md,
.claude/worktrees/agent-aeb9fc116ba05ef4d/Research/2026-10-24-wave68/S.md; not on main), Logs/n68/gg-proxy-close.md, r2/review-level.md,
verdict-I2-r2.md, verdict-I4-r3.md, verdict-g7leak.md, and the code below. Ran only git, grep, read-only node and one read-only scratch script
(patch share, section A X5). No dotnet, harness, build, Player or Editor. "R.md" / "S.md" below mean those worktree files.

**Two facts first.** (1) **C: has 6.2 GB free now** (`df -BM /c`: 6357M, 09:16); NIGHT68's rule 2 says "under 7 GB no rooms" and PC at 8 GB or more
(NIGHT68.md:77-78). Commit free 18.2 of 99.6 GB (Win32_OperatingSystem, 09:17); the three Unity processes hold 7.2 GB private each.
(2) The forge's n69 tools already exist, untracked, made 09:15 by Tools/night/forge69-make-n69.cjs (renamed copies of n68-*; clock freeze 3:15,
cap 4:00: forge69-make-n69.cjs:3-6, n69-clock.cjs:13). Every n68 line cited below has the same line in its n69 copy unless said.

## A. Instrument fixes

**X1. The launch M3 reader passes an unlaunched body.** Path: the probe sets `inPit` / `inLava` from the TARGET's position on any step of a 3 s
track, whether or not it flew (Tools/SimHarness/Harness.N65.Verb.cs:326-339; `flew` is computed at :327 and never read); the verdict line prints
it beside a RED (:347-348). Two readers then grep "in a pit YES" on any launch line, RED included: Tools/night/n65-verbs.sh:19-22 (no n69 copy;
shared) and n68-gate-pack.cjs:188-192 (= n69-gate-pack.cjs:188-192). Evidence: Logs/n68/r2-kiln/verbs-launch.txt:21 "RED | room kb3 | launch36 0
... in a pit YES ... target alive" then :23 "M3: kb3 PASS" (verdict-I2-r2.md:20-24). The launch event carries the thrown body's id
(Assets/Scripts/Core/Launch36.cs:133 `events.Emit(LaunchEvent, Boxes.Int(h.id))`); the probe only counts it (Verb.cs:275).
- Smallest fix: (a) Verb.cs:275 keep the first launched id (`o is int id`); (b) :326-339 track THAT body (by id among sim.enemies.all; stop at its
  death, as the target's track does at :278-280) and set inPit / inLava only from it, only when launchEv >= 1; print "launched body <kind> #<id>
  (the target: yes / no)" (this also answers open.md item 10: the punch threw another body on shipped kb3 seed 3, verdict-I2-r2.md:44-49);
  with launchEv 0 print "in a pit n/a (nothing launched)"; (c) both readers: M3 PASS only on a `: GREEN` line with `launch36 [1-9]` and a YES.
- Planted pair: RED = the saved r2 line through today's readers ("kb3 PASS", verbs-launch.txt:21/:23); GREEN = the same line through the fixed
  readers -> "M3: kb3 FAIL (nothing launched)". Harness pair: `bash Tools/night/n65-verbs.sh launch Logs/n68/r2-kiln/n67-kiln.json kb3` on the
  current DLL first (r2 ran before the I2 fix, verdict-I4-r3.md:91-94, so its line must be re-made, L-150), then on the fix; control: v2
  (`... launch Logs/n68/r2-spire/n67-spire.json v2`, r2: GREEN, launch36 1, in a pit YES, Logs/n68/r2-spire/verbs-launch.txt:11) must stay PASS.
- Note: "in a pit" means any step below -0.2 m inside a pit rect (Verb.cs:338), not "ended": see X2. Minutes: 30 code + 10 pair. No red re-made
  (M3 is not in a red).

**X2. A launched body that dies in a pit or lava is not a launch kill.** skip v2 counts launch kills only from Launch36BurstEvent with kills > 0
(Verb.cs:958; R.md:100-101 cited :870, open.md item 2 :953: the line moved). T's proposed fix (open.md:10-12, "count a KillEvent") would miss
both: lava emits `lavaEnemyDeath` with the id, no KillEvent (Assets/Scripts/Core/Lava30.cs:173-177, payload Lava30Event.id :39-41); a dry pit
ends a body with "alive false, no kill event" (Assets/Scripts/Game/SurvivalSim.N43.Rooms.cs:164-171), and only when it stays under its room's
floor minus pitDropM 1.5 m for endS 2 s (Design/unity-config.json:6585, :6587). So **a 1.5 m pit never ends a body** (kb3's tanks and v2's
crypt are floorY -1.5: review-level.md:176-178; v2.json pits floorY -1.5), a 2 m pit does (kb3c, kb3d). Lava kills in enemyBurnSeconds 0.8 (:2307).
- Smallest fix in V65ClearRun: (a) :952 keep `launched[id] = stp` beside `res.launches++`; (b) subscribe "lavaEnemyDeath" and count a launched id
  within flightS 0.8 (:3902) + 3 s as `lava`; (c) after `sim.Step()` (:1016) check the launched bodies still in that window: alive last step,
  dead now, and no "kill" event for the id this step -> `pit` (rooms43's end or the kill plane); (d) print them as a NEW line kind
  `n65verb skip launchend: room <id> | seed N | at (x, y, z) <lava|pit>` and a per-seed count. A new kind, not `launchkill`, because the area lint
  and the deco checker read `launchkill` lines as "a launch kill within 1 m" of a piece (n68-area.cjs:143, n68-deco.cjs:114): adding pit / lava
  ends there would change M1 P4 and M4 inputs.
- Planted pair: the same DLL before / after on `--level Logs/n68/r2-kiln/n67-kiln.json --room kb3d --seed 19 --seeds 2` (kb3d's 2 m moat; r2's
  seeds 19-20 had kills below the floor, review-level.md:136-137): RED prints no launchend row; GREEN prints >= 1, and every seed line and the M2
  line are byte-identical apart from the new rows (proof it is print-only). If seeds 19-20 throw nothing into the moat, plant one with the probe's
  `--target` on line65's last point (-3, 0, -270), which lies in the west moat cell (kb3d.json pits [-7.5,-275,-1,-267], floorY -2). 30-40 min.

**X3. The parry refusal predicts the volley instead of refusing every swing near a tell.** Today: any glove press is refused while any live
spitter within dMax of the eye is in its last buffer + 1 tell steps; dMax = reach + globSpeed x (remaining buffer + window), up to 10.7 m
(Harness.N68.Glob.cs:71-82; verdict-g7leak.md:37-41). The critic's model: no leak anywhere, but 182 / 327 refused cells uncatchable even with the
0.5 m margin (verdict-g7leak.md:47-59); seeds 2 and 11 went 29.9 -> 39.4 s and 32.7 -> 48.0 s refused-in (:66-73); the median held only by chance
(:77-81). The game's lob is fully readable: ThrowVolley (Assets/Scripts/Core/Spitter.cs:346-384): from = position + height x 0.85; time =
clamp(flat d / globSpeed, globTimeMin, globTimeMax); aim = tp + lead (velocity x time unless hunted) + 0.4 m up; a miss pushes the aim missBy
sideways on a random side (rng: unknowable, test both); three globs at -spread / 0 / +spread; velocity = Lob(from, aim, time) (:344). The volley
fires when the tell ends (:159-170, ThrowVolley at :165); the spitter stands still in its tell (:161-162).
- Smallest fix: move the per-glob test loop (:41-66) into a function of (pos, vel, first step); in :71-82 replace the dMax sphere by the coming
  volley: for each spitter in tell with stateSteps <= buffer + 1, build 3 aims (hit, miss left, miss right) x 3 spread globs = 9 lobs starting at
  step stateSteps, from the player's feet frozen at the press (the critic's moving-player cases found no leak from freezing, :47-48), and run the
  same tests over the buffer + window. Keep the 0.5 m margin. Apply the same function to the punch's refusal (Verb.cs:771, :782: the 8 m
  GlobInReach rule, open.md item 6), since the punch uses the same TryReflect and parry42 buffer as the swing (Assets/Scripts/Core/Combat.cs:1363,
  :1374): that is kb2's refusal path (kb2 ends with the shotgun in hand, "gauntlet presses refused 0": verdict-I4-r3.md:30).
- Planted pair (the files exist): RED = Logs/n68/g7leak/r2-seeds1-20-green.txt seeds 2 and 11 (refused-in 39.4 / 48.0 s); GREEN = the fix on the
  same command (its header line in that file): seeds 2 and 11 back near r2's 29.9 / 32.7 s, AND seeds 8 and 12 still 0 globs sent back
  (r2-seed8-green.txt / r2-seed12-green.txt), AND seeds 1-20 "the refused run used the verb on 0". A pure `--glob-cases` (the critic's grid: still /
  9 m/s toward / 4 m above) would print the over-refused count falling from 182 / 327. Code 45-60 min, runs ~10 min (g7 20 seeds: wall time
  150 s, Logs/n68/r3-iceline/verbs-skip.txt), then the parry reds re-made (g7, kb2: ~3-5 min each, L-150). Only worth it if a parry room is on
  the slate: g7 waits on GG's Q7 (DECIDE68.md:45-46) and takes the gun (PRINCIPLES rule 1; gg-proxy-close.md:52-61); kb2's rebuilds all lost to
  the shipped room (S.md:55).

**X4. A room measured alone.** gen-slots already assembles one slot latest and the rest old: `--slots=g7,g8` = "those latest"
(Tools/night/gen-slots.cjs:20-21, slotSet :172-173); R ran exactly that (`--slots=kb3`: R's kb3/r3/skip20-r1d-own-level-gen.txt:2). The round
hard-codes every slot latest: n68-round.sh:64 / n69-round.sh:64 (`--run --slots=latest`). Smallest change: `--slots=${SLOTS:-latest}` there,
SLOTS printed in the round's first echo (:52) and refused unless every SLOTS id is in ROOMS. Then per room, one at a time (two dotnet runs never
share a tree, n68-round.sh:14):
`L67=kiln ROOMS=kb3 SLOTS=kb3 bash Tools/night/n69-round.sh r1-kb3` (likewise `L67=spire ROOMS=v2 SLOTS=v2 ... r1-v2`, `L67=iceline ROOMS=g8 SLOTS=g8 ... r1-g8`).
- Time: a one-room round took 399 s (r3-kiln, kb3c), 287 s (r2-spire, v2), 424 s (r3-iceline, g8 + g7) (Logs/n68/r3-kiln-run.txt,
  r2-spire-run.txt, r3-iceline-run.txt "done in"); the fresh 21-30 pre-run adds ~half a skip (kb3c's 20-seed skip: 261 s). So 3 rooms ~25-30 min.
- Every downstream tool reads the round's own folder: area lint `--dir` takes <dir>/<L>.plan.json and <dir>/<id>.json (n68-area.cjs:382-383,
  called with --dir $out at n68-round.sh:86); deco judges the round's copy room-files/<id>.json (:88); the red set uses LEVEL_DIR=$out (:74); the
  verbs use LEVEL_FILE=$out/<id>.json (:81); the GATE pack reads <round>/<T67.id>.json and <L>.plan.json (n69-gate-pack.cjs:88-91) and ties a
  --fresh pre-run to that level's sha. So yes, they read the right folder; nothing else changes. The red (Design/levels/n57-kiln.json etc.,
  Logs/n68/red-kiln/kb3/skip.txt:1) is "every slot old", so an alone round is the like-for-like comparison (gen-slots --check asserts all-old
  equals the base level apart from names: gen-slots.cjs:29-34; not run by me).
- **What alone does not fix: the noise.** R's pair (same file, same DLL): per seed 9 ratios up, 10 down, verb-aware clear times identical on 1 of
  20 seeds (my diff of skip20-r1d-own-level.txt vs skip20-r1d-on-r2-level.txt). The neighbours re-roll every seed; they do not push one way.
  Per-seed sd 0.26-0.34, so the 20-seed median's error is ~0.07-0.09 (my calculation from those files). Alone makes the round comparable with the
  red; only more seeds shrink the error (40 seeds ~0.05-0.06, ~2x skip time: ~9 min per room). Both R runs predate the I2 fix (672e239d lacks
  98eff2e7: `git merge-base --is-ancestor`), so 1.22 is not a final-harness number.

**X5. Patch share.** The shipped room is Design/levels/n67/<L>/rooms-base/<id>.json ("n57-kiln's room exactly", gen-slots --export,
gen-slots.cjs:37; R's gen log line "slot kb2: old .../rooms-base/kb2.json: n57-kiln's room exactly"); the rebuild is .../rooms/<id>.json. No tool
counts kept pieces (the reviewer used python: review-level.md:3-5; deco's base is the room file's FIRST commit, n68-deco.cjs:25-28, a different
question). My read-only scratch count (geometry equal: boxes min/max, ramps from/to/width/rise, pads at/target, pits rect/floorY/lava; job / why
text ignored) reproduces the reviewer's box counts exactly:

| room | shipped pieces kept | boxes | rect / spawn / exit | waves |
|---|---|---|---|---|
| kb3c | 7 / 16 (44 %) | 5 / 10 | all same | changed |
| kb2 | 3 / 14 (21 %) | 3 / 10 | all same | same |
| kb3d | 11 / 15 (73 %) | 9 / 11 | all same | changed |
| kb3 | 9 / 16 (56 %) | 6 / 12 | all same | same |
| g8 | 1 / 13 (8 %) | 1 / 8 | all same | changed |
| g7 | 0 / 24 (0 %) | 0 / 15 | all same | changed |
| v2 | 0 / 17 (0 %) | 0 / 11 | all same | changed |

- Tool: `node Tools/night/n69-patch-share.cjs --level <L> <id>` printing the table row and `PATCH` over a forge-set share. Planted pair: a copy of
  rooms-base/<id>.json as the room (100 %, PATCH) and g7 (0 %). 25-30 min. Limits: gen-slots locks only id, kind and rect (gen-slots.cjs:118,
  :189); spawn is not locked by the tool but the slate's GUARD forbids moving it toward the verb's line (Logs/n68/slate.json:11); doors are
  fixed by the links (finishLevel's guards, gen-slots.cjs:348-357). So "from scratch" here means 0 % kept inside the fixed rect and doors.

**X6. STYLE >= refusal - 5.** The rule is computed in one place and printed in four: Tools/SimHarness/Harness.N68.Skip.cs:10-12 (V68StyleNeed);
the planted cases :28-55 (`--style-cases`, all 8 written for the old branches); Harness.N65.Verb.cs:1223-1224 (used), :1230 (the M2 VERB line's
text "min(refusal + 15, max(refusal, 95))"), :1203-1207 (comment), :1081 (doc comment, still "+15"), :1200-1202 (the STYLE+15 line, printed "not
the verdict": drop or keep under NIGHT67's name). The slate's ROOM text: Logs/n68/slate.json:8, written by Research/2026-10-24-forge68/
make-slate.cjs:16 (the n69 slate's maker must carry the new text). No Tools/night script computes it (grep: only the harness). Planted pair:
new cases (verb 69.0 vs refusal 74.0 PASS; 68.9 vs 74.0 FAIL; NaN UNSURE; NIGHT68's rule beside each) RED on today's DLL, GREEN after. 15 min.
No red changes (bars read the pooled ratio only: verdict-I4-r3.md:45). Effect on recorded numbers (refusal = need - 15 where need < 95): kb3c r2
67.7 vs new need 57.4, kb2 52.6 vs 39.2, kb3d 71.8 vs 62.6, kb3 83.2 vs 72.5 (final-harness alone 81.9 vs 74.4), g8 39.0 vs 20.9, v2 79.1 vs 70.8
(review-level.md:48, :85, :120, :159, :197, :269): **every room would pass STYLE; M2 would rest on the ratio alone.** It is GG's open Q8
(DECIDE68.md:47-48, no answer in the file); NIGHT68 kept the rule "never loosened after a measurement" (NIGHT68-REPORT.md:26-28).

## B. Room choices (one builder pair, 4 hours: work to 3:00, close 3:00-4:00, forge69-make-n69.cjs:4-5)

Bar = max(1.25, red + 0.20) on the final reds (NIGHT68-REPORT.md:65; verdict-I4-r3.md:45-55). **A ride room also needs >= bar + 0.10 to PASS:**
[bar, bar + 0.10) is UNSURE (Verb.cs:1225-1226), so 1.35 for kb3c and g8. "+ over red" = the best valid measurement minus the red.

| room | final state (file) | red -> bar | best measured (+ over red) | gap | the one thing most likely to move M2 | rebuild ~2 h? |
|---|---|---|---|---|---|---|
| kb3 launch | r1d, 56 % kept; tanks 1.5 m dry; presses cut | 1.13 -> 1.33 | alone 1.22 (+0.09), on r2 1.12 (R's kb3/r3, pre-I2-fix) | -0.11 | throw into a pit that ENDS a body (>1.5 m deep) on line65; bodies outside their attack window (open.md 16) | yes |
| v2 launch | r1f, 0 % kept (from blank) | 0.87 -> 1.25 | r2 1.09 (+0.22, pre-I2-fix) | -0.16 | crypt deeper than 1.5 m on the throw line; give back ride face + pads (air65 0/0 vs 3/3) | done once; yes |
| g8 ride | r1f, 8 % kept | 0.63 -> 1.25 (1.35) | r3 0.96 (+0.33), fresh 1.06 | -0.39 | R's untested pocket: W4 spitters behind a parapet facing the near bank, 12 m by ride vs ~28 m round (R.md:55-57) | rebuilt; yes |
| kb3c ride | r3a, 44 % kept | 0.97 -> 1.25 (1.35) | r2 1.09 (+0.12); r3 0.99 | -0.36 | a target only the ride reaches (S.md:36-44); spread 0.67-1.80 (S.md:66) | yes, but noise |
| kb2 parry | r1h, 21 % kept | 1.18 -> 1.38 | shipped 1.18; rebuilds 0.94-1.15 | -0.44 | none known: every rebuild lost to the shipped room (S.md:55) | no |
| kb3d launch | r1h, 73 % kept | 0.89 -> 1.25 | r2 0.91 (+0.02) | -0.34 | wave 3: 740 / 1060 hp unlaunchable (S.md:68-71): GG's Q6 | no (GG) |
| g7 parry melee | r1, 0 % kept, gun jammed | 1.12 -> 1.32 | r3 1.35 on its line, fresh 1.10 | +0.03 / -0.15 | X3 + its own walk baseline; takes the gun (rule 1), GG's Q7 | no (GG) |

Sources: NIGHT68-REPORT.md:68-76; review-level.md:48, :174-175, :213-217, :275, :280-281; verdict-I4-r3.md:91-94 (kb3, v2, kb2, kb3d never ran
on the final harness); patch shares from X5. **No room got more than +0.33 over its red; the 1.25 floor, not the +0.20, binds every room whose
red is under 1.05** (g8, v2, kb3d, kb3c need +0.28 to +0.62).

**Rank (best 3):**
1. **kb3 (launch).** The only room where the verb clearly pays: verb-aware 44.5-46.9 s vs refusals 55.8-58.0 s, 79-89 launches (R's two kb3/r3 totals
   lines; review-level.md:174-175); the smallest gap (-0.11 alone). Its throw never ends a body today (1.5 m tanks = pitDropM), so the burst is
   the only kill; a 2 m pit on line65 adds a kill the refusal cannot get, which X2 then counts. Rebuild from blank to answer gg-proxy #1 (56 %
   kept) and give back the four presses' job (the charger's stun, review-level.md:183-185; gg-proxy #4). Highest bar (1.33).
2. **v2 (launch).** Already from the blank rect (0 % kept), floor bar 1.25, measured +0.22. Same pit-depth point; plus the shipped ride face and
   two pads back (rule 1; air65 guard FAIL) and the bare near half (24 x 16.5 m, review-level.md:287). Its stall A guard (21.5 vs 19.0) is the
   other blocker (:273).
3. **g8 (ride).** Already a rebuild (8 % kept) with one untested idea aimed at the measured cause (the clear is kill-bound: every body comes to the
   near bank, R.md:54-56, :96-97). Weakest of the three: effective bar 1.35, the red is held down by the ride fallback pilot (open.md item 4), and
   exercised seeds depend on the walker's route (open.md item 5).
- First job for kb3 and v2: one alone round on the final harness (X4; ~6 min each): neither has an M2 number on it (verdict-I4-r3.md:91-94).
- Feasibility: R drafted three rooms (g8, v2 from blank, kb3) by make scripts in 0:00-0:35 (R.md:24-27); a round is 5-7 min; levelgen's learned
  limits (2 kinds a wave, spawn31 12 m, pit recovery 2 m not 2.5 m, a 1 m deck is a walkers' tier) are in S.md:57-59 and R.md:66, :74-75.

## C. The play build (PC)

Best by M2 per level from the current files: kiln **kb3** (r2 1.12; kb3c r3 0.99, kb2 0.94, kb3d 0.91), spire **v2** (1.09), iceline **g7** by
M2 (r3 1.35) but it jams the gun (rule 1), else **g8** (0.96) (NIGHT68-REPORT.md:68-76). Commands (NIGHT68.md:197-201's recipe, n69 paths):
```
node Tools/night/gen-slots.cjs --level kiln --slots=kb3 --run --out=Logs/n69/play/kiln
node Tools/night/gen-slots.cjs --level iceline --slots=g8 --run --out=Logs/n69/play/iceline
node Tools/night/gen-slots.cjs --level spire --slots=v2 --run --out=Logs/n69/play/spire
bash Tools/night/n65-play-build.sh n66-freeze n69-play Logs/n69/play/kiln/n67-kiln.json,Logs/n69/play/iceline/n67-iceline.json,Logs/n69/play/spire/n67-spire.json --config Design/unity-config.json --add-level n67.kiln67 n67.iceline67 n67.spire67
```
- Later rooms: `--update` rewrites only the given levels in Builds/n69-play (n65-play-build.sh:10-12, :24).
- Source: Builds/n66-freeze (385 MB, ships no n67 level: --add-level needed). Its code equals HEAD's except one look line (`git diff --stat
  72abcfe1 HEAD -- Assets/Scripts`: UnityTuning.N53.cs:29 adds n67-iceline to the iceline grade), so n67-iceline plays without that grade. The
  level swap is data-driven (NightRows.SwapLevels / LevelRows, Assets/Scripts/Core/NightRows.cs:269-277, :319-335); HEAD's config lists n67-kiln,
  n67-iceline, n67-spire in design.maps.list and holds the three rows (OFF; n46-build-rows.cjs turns them on in the copy only).
- Disk: hard links ~0; written ~11 MB (config 1.1 MB + levels 3.4 + 2.6 + 3.4 MB) plus three --out folders ~7 MB each (r3-kiln's assembly files
  ~7 MB): under 40 MB. The 8 GB line is NIGHT68's rule, not the job's size; C: is at 6.2 GB now.
- Earliest safe: at t0, before any chain: it needs no Player; `--run` builds the SimHarness once (gen-slots.cjs:40), so it must not share the tree
  with another dotnet job (n68-round.sh:14); assemblies took 12-37 s (r2/r3 *-run.txt "assembled"). Then `--update` when a room improves.
- Boot once: `Builds/n69-play/SKYGRIND.exe -skygrindSmoke -logFile Logs/n69/play-boot.txt` (CLAUDE.md Player checks), with no agent running (the
  focus pause), closed by PID; `-skygrindMap n67-kiln` boots that map (automation arg: SurvivalGame.Maps.cs:59-62). Whether the smoke's steps
  pass on a campaign map: not checked. The guard's heavy rule matches the smoke / reel SCRIPTS and unity builds, not the exe (.claude/hooks/
  guard.cjs:239-242), and refuses them under 12 GB; NIGHT65's lead chose not to run an exe around it (NIGHT65-LOG.md:178). The forge must say which.

## D. The closing gates

- What happened: started 9:03 in the background (NIGHT68-LOG.md:233); flow --off67 EQUAL, config, golden, the plain fingerprint, then stopped at
  the --off57 fingerprint "because the system is running low on memory" (:244-248; Logs/n68/gates-flow-close.txt ends "GATE FAIL: fingerprint
  --off57" with an empty line, written 08:00:56 local = ~9:10). Not the command's failure.
- Memory: per-process use of the gates is not measured anywhere. Known: commit free 9.9 GB at 4:24 and 10.6 GB after the stop (NIGHT68-LOG.md:132,
  :245), the three Unity processes ~7.1 GB each (:132-133), NIGHT68's rule 5 holds gates under 15 GB commit free (NIGHT68.md:86): they were
  started under that line. NIGHT66 read 5.4 GB commit free mid-gate vs 14.7 GB after (NIGHT66-LOG.md:98), a hint that the chain dips several GB
  (not isolated). The gates call `dotnet run --project` 14 times (n68-gates.sh:27, :32, :35 x12): each re-evaluates the build (MSBuild) before
  running; MSBUILDDISABLENODEREUSE=1 (:14) stops worker reuse, so each call starts fresh MSBuild nodes.
- Time: n46flow on 5 levels 387 s at the close vs 295 s for the base (flow-off67.txt / flow-base.txt wall-time lines); the mid-night full run
  (no Editor) finished inside ~5-6 min (log :52 at 0:42, "gates: all GREEN 23:37:49" :57). With the Editor step add up to 60 + 300 s (:39-42).
- How to get "gates: all GREEN" in the close: (1) cut the 14 MSBuild evaluations to 2: `dotnet build` Tools/SimHarness and Tools/CoreHarness
  once, then run the DLLs directly, as n65-verbs.sh:7, :14 already does (`dotnet Temp/SimHarness/bin/Release/net10.0/SimHarness.dll ...`); a
  ~6-line change to n69-gates.sh:19, :27, :32, :35. (2) Run them in the FOREGROUND, alone, right after the night's last commit to Tools/,
  Assets/ or Design/ (a code freeze, e.g. 2:45 before the 3:00 close start), reading commit free first (rule 5) and writing
  Logs/n69/gates-flow-close.txt; close-check reads only that file's last `gates:` line (n68-close-check.cjs:135-138), so an earlier run counts,
  but put `git rev-parse HEAD` at its top and re-run if any code lands after it. (3) With no Assets/ change all night, `--no-editor` is honest
  (the Editor step only checks a compile) and it still prints "gates: all GREEN" (n68-gates.sh:38-49); say so in the log.
