# NIGHT57 builder brief (common to builders L, G, V; the lead's; from NIGHT56's brief with the n57 names)

You build in your own git worktree, branched from `port/stage3` at the NIGHT57 shared commit. The shared commit placed `design.n57.press57`
(sim, G; `enabled: false`, an empty `numbers` object) and `design.n57.kiln57` (L; a LEVEL row: `levelFrom` n52-kiln, `levelTo` n57-kiln, off),
and the reel hooks `FeelReel.N57.cs` (a157 L, a257 G, a357 V, as `static partial void N57<Name>(...)`: `N57Level`, `N57Play`,
`N57Colour`): implement YOUR partial in a NEW file `Assets/Scripts/Game/FeelReel.N57.<Name>.cs` (models: `FeelReel.N56.Room.cs`,
`FeelReel.N55.Kill.cs`, `FeelReel.N55.Boss.cs`, `FeelReel.N52.Rooms.cs`). The NIGHT55-56 shelf rows (tokens56, rank56, kill55, wyrm55,
wick55 for G; colour56, rim56, yellow56, grade55, brief55 for V) keep their names: fix them inside their own row. NIGHT57.md is untracked:
read it by its absolute path `C:\Users\gaige\dev\SKYGRIND-unity\NIGHT57.md` (sections 0, 2, 5, 6, 10). Read `Research/2026-10-21-wave57/GAPS.md`.

## THE SHIPPING RULE (NIGHT57.md section 0; it replaces "every critic line passes")
Your entry names a MAIN number and GUARD numbers. A row ships ON when its MAIN beats the A0 baseline (the same measure, the row off) and no
GUARD is worse than the baseline beyond its band (bot numbers 10 % relative or outside the seed spread). So: print MAIN and every GUARD,
before (row off) and after (row on), in every report. A near miss on a target that is not a guard does not block; say it plainly.
Baselines: `Logs/n57/flow-base.txt` (n46flow, n52 levels, 10 seeds; it prints RANGED hit rate by speed and IDLE per kind) and
`Logs/n57/wyrm-base.txt` (n47human wyrm on n52-kiln, 40 seeds), in the MAIN repo.

## The loop per slate entry (one at a time; commit every slice: the usage limit has stopped every agent at once, three times)
1. **Contract:** your slate entry (Logs/n57/slate.json in the MAIN repo, read-only to you) names MAIN, GUARDS and doneWhen. If one is
   unmeasurable, say so in your notes and build to a sharper one AND print the original's number.
2. **RED proof first:** a reel phase that prints its input happened and its number, an EditMode test, a harness command or a Python
   checker with a planted case; it reads the before-number with the row off. Commit it.
3. **Build** behind your row: `NightRows.ViewOn(cfg, "<row>")` for a view row; a sim row reads its tape bit (models:
   `SurvivalSim.N55.Wick.cs`, `Core/Enemies/Wick37.N55.cs`, `SurvivalSim.N53.RideKill.cs`). With every n56 / n57 row off the game is
   NIGHT56's bit for bit. Level 3 (L) lives behind kiln57: off loads n52-kiln unchanged.
4. **GREEN where you can check it:** EditMode-style tests compile via `bash Tools/night/dn-check-wt.sh <your scratch dir>` (0 errors);
   sim numbers via `dotnet run --project Tools/SimHarness -c Release -- <cmd>` and `Tools/CoreHarness`.
   **After EVERY commit the default SimHarness build prints the fingerprint EQUAL:** `dotnet run --project Tools/SimHarness -c Release --
   fingerprint` (no -p:Extra). If you touch `Assets/Scripts/Core`: golden `dotnet run --project Tools/CoreHarness -c Release` prints
   "0 failing, 0 mismatches", and `... fingerprint --off56`, `... fingerprint --off57` each print EQUAL. Paste those lines in your report.
5. **Commit** with explicit paths plus `.meta` (new files under Assets need one: `fileFormatVersion: 2` + a fresh 32-hex guid), message
   `NIGHT57 <letter> <entry> <row>: <what> (MAIN <before -> after>)`, trailer `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
6. **Report:** the lead integrates by cherry-pick (builders cannot merge), builds the Player, runs your reel and gives the numbers and
   <= 4 frames per entry to ONE fresh batched critic per build. Only the lead flips `passes`. Builds r1 ~1:30, r2 ~2:40 after t0;
   builders stop at 3:30.

## Your numbers
- Gameplay numbers in `Design/unity-config.json` under YOUR row's `numbers` (and the byte copy `Assets/StreamingAssets/Design/unity-config.json`:
  keep both identical, `cmp`), edited by ONE writer script of your own (`Tools/night/n57-<letter>-rows.cjs`, written with the Write tool)
  that changes only your rows and asserts nothing else changed (models: Tools/night/n54-grade-off.cjs, n55-b-rows.cjs). Never hand-type a
  gameplay number in code (CLAUDE.md rule 5).
- Unity-only numbers in a NEW partial `Assets/Scripts/Game/UnityTuning.N57<Letter>.cs`, each with a comment.
- Level 3 (L): `Tools/night/gen-n57-kiln.cjs` writes `Design/levels/n57/kiln.plan.json`, then levelgen, then `--finish` to
  `Design/levels/n57-kiln.json` + `Assets/StreamingAssets/Design/levels/n57-kiln.json` (copy gen-n52-kiln.cjs's run order and header).
  Checkers: `python -I Tools/night/n56-room.py`, `python -I Tools/night/n53-identity.py`, `bash Tools/night/n53-contract-all.sh`, walk
  bot via levelgen `--walk` and n46flow `--level n57-kiln` (with kiln57's level present). Check every list of known level ids
  (grep `n52-kiln` in Assets/Scripts, Tools, Design) and add n57-kiln where a level must be listed; name each in your report.

## Files (ownership; every builder edits ONLY its own list, plus new files of its own)
- **L (level 3):** Tools/night/gen-n57-kiln.cjs, Tools/night/n57-l-*.cjs / *.py, Design/levels/n57/*, Design/levels/n57-kiln.json and its
  StreamingAssets copy (+ .meta), docs/ROOMS.md (a NEW section "Level 3 as n57-kiln" only), `FeelReel.N57.Level.cs` (a157),
  Research/2026-10-21-wave57/L.md. Shared files where n57-kiln must be listed: one line each, named in the report.
- **G (enemy pressure, kills):** press57's Core and sim partials (e.g. Core/Press57.cs, `SurvivalSim.N57.Press.cs`), `Harness.N57.*.cs`,
  their tests, `FeelReel.N57.Play.cs` (a257); tokens56 (its N56 files), kill55 (Kill55*.cs, SurvivalGame.N55K.cs, UnityTuning.N55K.cs,
  FeelReel.N55.Kill.cs, N55KillTests.cs), wyrm55 / wick55 / rank56 (their files), Tools/night/n57-g-*.cjs, Research/2026-10-21-wave57/G.md.
- **V (colour budget, near misses):** colour56 / rim56 / yellow56 (their N56 view files, `n56-colour.py`), grade55 (BossGrade55*.cs,
  Wick37View.N55.cs, UnityTuning.N55B.cs, FeelReel.N55.Boss.cs, N55BossGradeTests.cs, n55-b-rows.cjs), brief55 (its files),
  `FeelReel.N57.Colour.cs` (a357), `UnityTuning.N57V.cs`, Tools/night/n57-v-*.cjs / *.py, Research/2026-10-21-wave57/V.md.
- **Shared files** (`SurvivalGame*.cs`, `SurvivalSim*.cs` not yours, `FeelReel.N27.cs`, `UnityTuning.cs`, `Hud*.cs`, `Ui*.cs`,
  `Campaign24.cs`): only through a NEW partial file of your own plus at most ONE call-site line per hook, named in your report.

## Never
- Run the Unity Editor, a build, a Player, a reel, run_tests or `unity test`; push; merge; commit outside your branch; touch another
  builder's files or rows; edit `.unity` / `.prefab` YAML; add a package; audio; enemy LOOKS / models / new enemy KINDS (rule 7; GG said
  yes to ATTACK BEHAVIOUR on existing kinds only); the wall ride's speed; level 6; THE KILN WYRM's AI (GG: "keep the worm AI the same").
- Stage GG's four files (ProjectSettings/UnityConnectSettings.asset, ProjectSettings/EditorBuildSettings.asset,
  Assets/ProjectSettings/UIEffectProjectSettings.asset, Assets/Scenes/Arena.unity) or the TMP font assets. `git add -A` / `git add .`.
- Leave dotnet bin/obj or scratch copies in the worktree (disk: C: had 34 GB at t0).
- Look rules (CLAUDE.md rule 8): cyan player-usable, red-orange enemies / danger, yellow parryable, magenta RIVAL only, pale violet-white
  boundary; UI text >= 16 px at 1080p; no whole-screen effect; at most 3 flashes a second. A new view copying the stone material passes
  vertex colours or uses `Light54.StoneOrLit`.
- Allocation per sim step; randomness outside `Mulberry32` in sim code; a new idea in code (ideas go to your notes file).

## Research to use
Research/2026-10-21-wave57/: GAPS.md, research/design.md (lessons 1-12 with URLs), research/github.md (licences: port only MIT / Apache /
BSD / CC0 / Zlib, logged). Research/2026-10-20-wave56/: levels.md (R1-R15, THE RING / THE GYM / THE CHINA SHOP), enemies.md (M1 tokens,
M2 accuracy vs speed), colors.md (the saturation cap, value first), gate-r5a.md / gate-r5b.md (why each shelf row missed).

## Report (under 300 words), last line
`N57-<letter>: round <r> · built <ids> · commits <shas>`
with each entry's state, every sha, MAIN and GUARDS before / after, the exact reel command (`bash Tools/night/n47-reels.sh <tag>
<side>,<level>,<scenario>[,<arg>]`, e.g. `l1on,n57-kiln,a157` and `l1off,n52-kiln,a157,-skygrindOff57`), and the frames the critic must see.
