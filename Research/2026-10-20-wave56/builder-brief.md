# NIGHT56 builder brief (common to builders L, G, V; the lead's; from NIGHT55's brief with the n56 names)

You build in your own git worktree, branched from `port/stage3` at the NIGHT56 shared commit (NIGHT55's branch commits are already
picked into it). The shared commit placed every `design.n56.<row>` (all `enabled: false`, each with an empty `numbers` object:
`rank56` and `tokens56` (sim, G), `rim56`, `colour56`, `yellow56` (view, V)) and the reel hooks `FeelReel.N56.cs` (a156 L, a256 G,
a356 V, as `static partial void N56<Name>(...)`: `N56Room`, `N56Play`, `N56Colour`): implement YOUR partial in a NEW file
`Assets/Scripts/Game/FeelReel.N56.<Name>.cs` (models: `FeelReel.N55.Room.cs`, `FeelReel.N55.Kill.cs`, `FeelReel.N55.Boss.cs`,
`FeelReel.N52.Rooms.cs` for a452's entry / mid frames). NIGHT56.md is untracked: read it by its absolute path
`C:\Users\gaige\dev\SKYGRIND-unity\NIGHT56.md` (sections 0, 2, 5, 6, 10).

## The loop per slate entry (one at a time; commit every slice: the usage limit has stopped every agent at once, twice)
1. **Contract:** your slate entry's `doneWhen` (Logs/n56/slate.json in the MAIN repo `C:\Users\gaige\dev\SKYGRIND-unity`, read-only to
   you) is the contract. If you think it is unmeasurable as written, say so in your notes file and propose a sharper one; build to
   the sharper one AND print the original's number.
2. **RED proof first:** write the proof (a reel phase that prints its input happened and its number, an EditMode test, a harness
   command or a Python checker with a planted case) so it fails / reads the before-number with the row off. Commit it.
3. **Build** behind your row: `NightRows.ViewOn(cfg, "<row>")` for a view row; a sim row reads its tape bit (models:
   `SurvivalSim.N55.Wick.cs`, `Core/Enemies/Wick37.N55.cs`, `SurvivalSim.N53.RideKill.cs`). With every n56 row off the game is
   NIGHT55's bit for bit. Rooms (builder L) are level data edited in place (no row; the comparison side is Builds/n55-freeze).
4. **GREEN where you can check it:** EditMode-style tests compile via `bash Tools/night/dn-check-wt.sh <your scratch dir>` (0 errors);
   sim numbers via the dotnet harnesses (`dotnet run --project Tools/SimHarness -c Release -- <cmd>`, `Tools/CoreHarness`).
   **After EVERY commit the default SimHarness build prints the fingerprint EQUAL:**
   `dotnet run --project Tools/SimHarness -c Release -- fingerprint` (no -p:Extra; a harness file that needs -p:Extra must not
   break the default build: NIGHT55's C did). If you touch anything under `Assets/Scripts/Core`: golden
   `dotnet run --project Tools/CoreHarness -c Release` must print "0 failing, 0 mismatches", and `... fingerprint --off55`,
   `... fingerprint --off56` must each print EQUAL too. Paste those lines in your report.
5. **Commit** with explicit paths plus their `.meta` (new files under Assets need a `.meta`: `fileFormatVersion: 2` + a fresh
   32-hex guid), message `NIGHT56 <letter> <entry> <row>: <what> (<number before -> after>)`, trailer
   `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
6. **Report:** the lead integrates by cherry-pick (builders cannot merge), builds the Player, runs your reel and hands the numbers
   and <= 4 frames per entry to ONE fresh batched critic per build. You never see the critic; your report is not evidence. Only the
   lead flips `passes`. Builds: r1 ~2:40, r2 ~3:50, r3 ~5:10 after t0 (00:00 MST); builders stop at 5:00 (~04:57).

## Your numbers
- Gameplay numbers in `Design/unity-config.json` under YOUR row's `numbers` (and the byte copy
  `Assets/StreamingAssets/Design/unity-config.json`: keep both identical: `cmp`). Edit the config with ONE writer script of your own
  (`Tools/night/n56-<letter>-rows.cjs`, written with the Write tool) that parses the JSON, changes only `design.n56.<your row>.numbers`
  (or the NIGHT55 row you own tonight), asserts nothing else changed, and writes both copies (models: Tools/night/n54-grade-off.cjs,
  n55-b-rows.cjs). Never hand-type a gameplay number in code (CLAUDE.md rule 5).
- Unity-only numbers (render, camera, UI layout) in a NEW partial file `Assets/Scripts/Game/UnityTuning.N56<Letter>.cs`
  (`public static partial class UnityTuning`), each with a comment.
- Level data (L): the plans `Design/levels/n52/<level>.plan.json` and the generated `Design/levels/n52-<level>.json` + its byte copy
  under `Assets/StreamingAssets/Design/levels/` (the NIGHT53-55 tools: Tools/night/n53-loop-*.cjs, e53-padspot.py, n55-r-kb2b.cjs show
  the in-place edit pattern; `python -I Tools/night/n53-identity.py`, `bash Tools/night/n53-contract-all.sh`).

## Files (ownership; every builder edits ONLY its own list, plus new files of its own)
- **L (levels):** Tools/night/n56-room.py, Tools/night/n56-l-*.cjs / *.py, Design/levels/n52/*.plan.json, Design/levels/n52-*.json and
  their StreamingAssets copies, Reports/LEVEL-n52-*-validate.md, `FeelReel.N56.Room.cs` (a156 + the room meter), NIGHT55's clear55
  (Clear55.cs, SurvivalGame.N55R.cs, UiClear55.cs, UnityTuning.N55R.cs, FeelReel.N55.Room.cs, n55-r-clear-config.cjs) and doorDark
  (n55-r-kb2b.cjs, n55-r-flat.py), Research/2026-10-20-wave56/L.md.
- **G (gameplay / enemies):** new Core files for rank56 / tokens56 (e.g. Core/Rank56.cs, Core/Tokens56.cs) and their sim partials
  (`SurvivalSim.N56.*.cs`), `Tools/SimHarness/Harness.N56.*.cs`, their tests, `FeelReel.N56.Play.cs` (a256), NIGHT55's wick55
  (Core/Enemies/Wick37.N55.cs, SurvivalSim.N55.Wick.cs, Harness.N55.Wick.cs, Wick55Tests.cs; any FIGHT MODE change in a NEW partial
  file, not FeelReel.N55.Boss.cs), kill55 (Kill55.cs, Kill55Damage.cs, Kill55Debris.cs, SurvivalGame.N55K.cs, UnityTuning.N55K.cs,
  FeelReel.N55.Kill.cs, N55KillTests.cs), the wyrm's encounter numbers (wyrm files: a new `*.N56.Wyrm.cs` partial), Tools/night/n56-g-*.cjs,
  Research/2026-10-20-wave56/G.md.
- **V (colours):** `Tools/night/n56-colour.py` (the colour meter, with a planted case), `FeelReel.N56.Colour.cs` (a356), new view files
  for rim56 / colour56 / yellow56, `UnityTuning.N56V.cs`, NIGHT55's grade55 (BossGrade55.cs, BossGrade55Measure.cs, Wick37View.N55.cs,
  UnityTuning.N55B.cs, FeelReel.N55.Boss.cs (a155), N55BossGradeTests.cs, Tools/night/n55-b-rows.cjs), decor55 (Decor55.cs,
  n55c-decor-config.cjs, n55-decor-share.py), ssao55 (Ssao55.cs, FeelReel.N55.Look.Ssao.cs, UnityTuning.N55C.cs), Tools/night/n56-v-*.cjs,
  Research/2026-10-20-wave56/V.md.
- **Shared files** (`SurvivalGame*.cs`, `SurvivalSim*.cs` that are not yours, `FeelReel.N27.cs`, `UnityTuning.cs`, `Hud*.cs`, `Ui*.cs`,
  `Campaign24.cs`): only through a NEW partial file of your own plus at most ONE call-site line per hook in an existing file, named in
  your report (the lead resolves the cherry-picks). If you need more than that, write it in your notes and the lead decides.

## Never
- Run the Unity Editor, a build, a Player, a reel, run_tests or `unity test`; push; merge; commit outside your branch; touch another
  builder's files or rows; edit `.unity` / `.prefab` YAML; add a package; audio code or sound files; enemy looks / models / new enemy
  kinds (rule 7: write a proposal page only); the wall ride's speed; level 6.
- Stage GG's four files (ProjectSettings/UnityConnectSettings.asset, ProjectSettings/EditorBuildSettings.asset,
  Assets/ProjectSettings/UIEffectProjectSettings.asset, Assets/Scenes/Arena.unity) or the TMP font assets.
- `git add -A` / `git add .`.
- Leave dotnet bin/obj or scratch copies inside the worktree when you finish (disk: C: had 32 GB at t0; delete your own scratch output).
- Look rules (CLAUDE.md rule 8): cyan player-usable, red-orange enemies / danger, yellow parryable, magenta RIVAL only, pale
  violet-white boundary; UI text >= 16 px at 1080p; no effect over the whole screen; at most 3 flashes a second.
- Any new view that copies the stone material passes vertex colours or uses `Light54.StoneOrLit` (Stone.shader ignores `_BaseColor`).
- No allocation per sim step; randomness only from `Mulberry32` in sim code; no new idea in code (ideas go to your notes file).

## Research to use
Research/2026-10-20-wave56/: levels.md (rules R1-R15, recipes THE RING / THE GYM / THE CHINA SHOP), enemies.md (M1 tokens, M2
accuracy vs speed, M6 push-forward, M10 waves on kills), colors.md (the saturation cap, value first, 3:1 contrast floor, warm enemies /
cool world, per-level keys, the dusk-sky risk), gameplay.md (the three proposals), IDEAS-POOL.md, redteam.md / redteam-a.md (what the
plan was checked against). NIGHT55's critics (Research/2026-10-20-wave55/critic-*.md) are the best map of what GG would notice.

## Report (under 300 words), last line
`N56-<letter>: round <r> · built <ids> · commits <shas>`
with each entry's state, every sha, every number before / after, the exact reel command the lead must run
(`bash Tools/night/n47-reels.sh <tag> <side>,<level>,<scenario>[,<arg>]`, e.g. `l1on,n52-foundry,a156` and
`l1off,n52-foundry,a156,-skygrindOff56`), and the frames the critic must see.
