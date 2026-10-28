# NIGHT55 builder brief (common to builders B, C, K, R, S; the lead's)

You build in your own git worktree, branched from `port/stage3` at the NIGHT55 shared commit. The shared commit placed every
`design.n55.<row>` (all `enabled: false`, each with an empty `numbers` object) and the reel hooks `FeelReel.N55.cs`
(a155 B, a255 C, a355 K, a455 R, a555 S, as `static partial void N55<Name>(...)`): implement YOUR partial in a NEW file
`Assets/Scripts/Game/FeelReel.N55.<Name>.cs` (model: `FeelReel.N54.Grade.cs`, `FeelReel.N54.Move.cs`, `FeelReel.N54.Light.cs`).

## The loop per slate entry (one at a time; commit every slice: the spend limit has stopped every agent at once before)
1. **Contract:** your slate entry's `doneWhen` (Logs/n55/slate.json in the MAIN repo, read-only to you) is the contract. If you
   think it is unmeasurable as written, say so in your notes file and propose a sharper one; build to the sharper one.
2. **RED proof first:** write the proof (a reel phase that prints its input happened and its number, an EditMode test, or a harness
   command) so it fails / reads the before-number with the row off. Commit it.
3. **Build** behind your row: `NightRows.ViewOn(cfg, "<row>")` for a view row; a sim row reads its tape bit (model:
   `SurvivalSim.N54.Wick.cs`, `Core/Enemies/Wick37.N54.cs`). With every n55 row off the game is NIGHT54's bit for bit.
4. **GREEN where you can check it:** EditMode-style tests compile via `bash Tools/night/dn-check-wt.sh <your scratch dir>` (0 errors);
   sim numbers via the dotnet harnesses (`dotnet run --project Tools/SimHarness -c Release -- <cmd>`, `Tools/CoreHarness`). If you
   touch anything under `Assets/Scripts/Core`: golden `dotnet run --project Tools/CoreHarness -c Release` must print
   "0 failing, 0 mismatches", and `dotnet run --project Tools/SimHarness -c Release -- fingerprint`, `... fingerprint --off54`,
   `... fingerprint --off55` must each print EQUAL. Paste those lines in your report.
5. **Commit** with explicit paths plus their `.meta` (new files under Assets need a `.meta`: `fileFormatVersion: 2` + a fresh
   32-hex guid), message `NIGHT55 <letter> <row>: <what> (<number before -> after>)`, trailer
   `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
6. **Report:** the lead integrates by cherry-pick, builds the Player, runs your reel and hands before / after frames to a FRESH
   critic. You never see the critic; your report is not evidence. Only the lead flips `passes`.

## Your numbers
- Gameplay numbers in `Design/unity-config.json` under YOUR row's `numbers` (and the byte copy
  `Assets/StreamingAssets/Design/unity-config.json`: keep both identical: `cmp`). Edit the config with a `.cjs` script you write
  with the Write tool that parses the JSON, changes only `design.n55.<your row>.numbers`, asserts nothing else changed, and writes
  both copies (model: Tools/night/n54-grade-off.cjs). Never hand-type a gameplay number in code (CLAUDE.md rule 5).
- Unity-only numbers (render, camera, UI layout) in a NEW partial file `Assets/Scripts/Game/UnityTuning.N55<Letter>.cs`
  (`public static partial class UnityTuning`), each with a comment.

## Never
- Run the Unity Editor, a build, a Player, a reel, run_tests or `unity test`; push; merge; commit outside your branch; touch
  another builder's files or rows; edit `.unity` / `.prefab` YAML; add a package; audio code or sound files; a new enemy design.
- Stage GG's four files (ProjectSettings/UnityConnectSettings.asset, ProjectSettings/EditorBuildSettings.asset,
  Assets/ProjectSettings/UIEffectProjectSettings.asset, Assets/Scenes/Arena.unity) or the TMP font assets.
- `git add -A` / `git add .`.
- Leave dotnet bin/obj or scratch copies inside the worktree when you finish (disk: C: had 22 GB; delete your own scratch output).
- Look rules (CLAUDE.md rule 8): cyan player-usable, red-orange enemies / danger, yellow parryable, magenta RIVAL only, pale
  violet-white boundary; UI text >= 16 px at 1080p; no effect over the whole screen; at most 3 flashes a second.
- Any new view that copies the stone material passes vertex colours or uses `Light54.StoneOrLit` (Stone.shader ignores `_BaseColor`).
- No allocation per sim step; randomness only from `Mulberry32` in sim code.

## Shared files (ownership)
- `SurvivalGame*.cs`, `FeelReel.N27.cs`, `UnityTuning.cs`, `Hud*.cs`, `Ui*.cs`: only through a NEW partial file of your own
  (`SurvivalGame.N55<Letter>.cs`) plus at most ONE call-site line per hook in an existing file, named in your report
  (the lead resolves the cherry-picks). If you need more than that, write it in your notes and the lead decides.
- Research to use: Research/2026-10-20-wave55/web.md, github.md (if present when you start; they are being written), the critic
  files critic-*.md, Logs/n55/OPEN.md.

## Report (under 300 words), last line
`N55-<letter>: round <r> · passes <ids> · SHORT <ids> · commits <shas>`
with each entry's state, every sha, every number before / after, the exact reel command the lead must run
(`bash Tools/night/n47-reels.sh <tag> <side>,<level>,<scenario>[,<arg>]` e.g. `b1on,n52-foundry,a155` and
`b1off,n52-foundry,a155,-skygrindOff55`), and the frames the critic must see.
