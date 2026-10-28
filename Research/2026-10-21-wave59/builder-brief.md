# NIGHT59 builder brief (common to builders L, G, V; the lead's; NIGHT57's brief with the n58 names)

You build in your own git worktree, branched from `port/stage3` at the NIGHT59 shared commit. HEAD already holds NIGHT57's seven ON rows
(kiln57, wyrm55, tokens56, brief55, wick55, grade55, colour56; tag `u3-n57`). The shared commit placed `design.n58.span58` (L; a LEVEL row:
`levelFrom` n52-span, `levelTo` n59-span, off; the placeholder `Design/levels/n59-span.json` is a byte copy of n52-span with its two
"name" keys changed) and the reel hooks `FeelReel.N58.cs` (a158 L, a258 G, a358 V, as `static partial void N58<Name>(...)`: `N58Level`,
`N58Play`, `N58Colour`): implement YOUR partial in a NEW file `Assets/Scripts/Game/FeelReel.N58.<Name>.cs` (models:
`FeelReel.N57.Level.cs`, `FeelReel.N57.Play.cs`, `FeelReel.N57.Colour.cs`). There is NO new sim row: kill55, press57 (G), rim56, yellow56
(V) keep their rows and are fixed inside them. L0 edits n57-kiln's data behind kiln57 (ON): it has no n58 off switch, so it carries its own
before / after. NIGHT59.md is untracked: read it by its absolute path `C:\Users\gaige\dev\SKYGRIND-unity\NIGHT59.md` (sections 0, 1, 4, 5),
and the shipping rule in `C:\Users\gaige\dev\SKYGRIND-unity\NIGHT57.md` section 0.

## THE SHIPPING RULE (NIGHT57.md section 0)
Your entry names a MAIN number and GUARD numbers. A row ships ON when its MAIN beats the A0 baseline (the same measure, the row off) by more
than its noise (bot numbers: more than the gap between seeds 1-10 and 11-20) and no GUARD is worse than the baseline beyond its band (bot
numbers 10 % relative or outside the seed spread). So: print MAIN and every GUARD, before (row off) and after (row on), in every report. A
near miss on a target that is not a guard does not block; say it plainly. A row without an r1 critic verdict stays OFF tonight.
Baselines (MAIN repo, HEAD after D-5, every n58 row off): `Logs/n58/flow-base.txt` / `flow-base-B.txt` (n46flow on n52-foundry, iceline,
span, spire; seeds 1-10 / 11-20) and `Logs/n58/kiln-base.txt` / `kiln-base-B.txt` (n46flow `--level n57-kiln`, both seed sets: put at the
next room 14 / 15, stall 18.9 / 20.5 %, deaths 2 / 6, clear 177.0 / 169.9 s).

## The loop per slate entry (one at a time; commit every slice: the usage limit has stopped every agent at once, several times)
1. **Contract:** your slate entry (Logs/n58/slate.json in the MAIN repo, read-only to you) names MAIN, GUARDS and doneWhen. If one is
   unmeasurable, say so in your notes and build to a sharper one AND print the original's number.
2. **Cause first, then a RED proof:** print the cause (a trace, a checker with a planted case, a harness command); it reads the
   before-number with the row off. Commit it.
3. **Build** behind your row: `NightRows.ViewOn(cfg, "<row>")` for a view row; a sim row reads its tape bit. With every n58 row off and
   kill55 / press57 / rim56 / yellow56 off, the game is NIGHT57's bit for bit. Level 4 (L1) lives behind span58: off loads n52-span unchanged.
4. **GREEN where you can check it:** EditMode-style tests compile via `bash Tools/night/dn-check-wt.sh <your scratch dir>` (0 errors);
   sim numbers via `dotnet run --project Tools/SimHarness -c Release -- <cmd>` and `Tools/CoreHarness`.
   **After EVERY commit the default SimHarness build prints the fingerprint EQUAL:** `dotnet run --project Tools/SimHarness -c Release --
   fingerprint`, and `... fingerprint --off57`, `... fingerprint --off58`. If you touch `Assets/Scripts/Core`: golden `dotnet run --project
   Tools/CoreHarness -c Release` prints "0 failing, 0 mismatches". Paste those lines in your report.
5. **Commit** with explicit paths plus `.meta` (new files under Assets need one: `fileFormatVersion: 2` + a fresh 32-hex guid), message
   `NIGHT59 <letter> <entry> <row>: <what> (MAIN <before -> after>)`, trailer `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
6. **Report:** the lead integrates by cherry-pick (builders cannot merge), builds the Player, runs your reel and gives the numbers and
   <= 4 frames per entry to ONE fresh batched critic. Only the lead flips `passes`. Build r1 at ~0:55 after t0 (report what is ready by
   ~0:50); the freeze at ~1:25 has NO critic: what has no r1 verdict stays OFF. Builders stop at 1:25.

## Your numbers
- Gameplay numbers in `Design/unity-config.json` under YOUR row's `numbers` (and the byte copy `Assets/StreamingAssets/Design/unity-config.json`:
  keep both identical, `cmp`), edited by ONE writer script of your own (`Tools/night/n59-<letter>-rows.cjs`, written with the Write tool)
  that changes only your rows and asserts nothing else changed (model: Tools/night/n57-g-rows.cjs, n57-v-rows.cjs). Never hand-type a
  gameplay number in code (CLAUDE.md rule 5).
- Unity-only numbers in a NEW partial `Assets/Scripts/Game/UnityTuning.N58<Letter>.cs`, each with a comment.
- Level 3 (L0): `Tools/night/gen-n57-kiln.cjs` and its run order (its header); checkers `python -I Tools/night/n57-l-room.py`,
  `bash Tools/night/n57-l-contract.sh`, `python -I Tools/night/n53-identity.py`, walk bot via levelgen `--walk`, n46flow `--level n57-kiln`.
- Level 4 (L1): `Tools/night/gen-n59-span.cjs` from `Tools/night/gen-n52-span.cjs` (span's own generator) with NIGHT57's slots idea;
  writes `Design/levels/n58/span.plan.json`, then levelgen, then `--finish` to `Design/levels/n59-span.json` + the StreamingAssets copy;
  `Tools/night/n59-l-room.py` / `n59-l-contract.sh` (n57's hard-code 'kiln'). Check every list of known level ids (grep `n52-span` in
  Assets/Scripts, Tools, Design) and add n59-span where a level must be listed; name each in your report.

## Files (ownership; every builder edits ONLY its own list, plus new files of its own)
- **L (levels):** Tools/night/gen-n57-kiln.cjs and Design/levels/n57/* + Design/levels/n57-kiln.json and its StreamingAssets copy (L0);
  Tools/night/gen-n59-span.cjs, Tools/night/n59-l-*.cjs / *.py / *.sh, Design/levels/n58/*, Design/levels/n59-span.json and its
  StreamingAssets copy (+ .meta) (L1); docs/ROOMS.md (NEW sections "Level 3 as n57-kiln: NIGHT59 exits" and "Level 4 as n59-span" only);
  `FeelReel.N58.Level.cs` (a158); Research/2026-10-21-wave59/L.md.
- **G (hits, stillness):** kill55 (Kill55*.cs, SurvivalGame.N55K.cs, UnityTuning.N55K.cs, FeelReel.N55.Kill.cs, N55KillTests.cs), press57
  (SurvivalSim.N57.Press.cs, its Core files, Harness.N57.*.cs), new `Harness.N58.*.cs`, tokens56 (its N56 files; G3 only),
  `FeelReel.N58.Play.cs` (a258), Tools/night/n59-g-*.cjs / *.sh, Research/2026-10-21-wave59/G.md.
- **V (colour, the exits lit):** rim56 / yellow56 (their N56 view files, `n56-colour.py`, `n57-v-*.py`), a NEW view file for the exit lamp
  (`*.N58V.cs`) or lamp entries handed to L, `FeelReel.N58.Colour.cs` (a358), `UnityTuning.N58V.cs`, Tools/night/n59-v-*.cjs / *.py,
  Research/2026-10-21-wave59/V.md.
- **Shared files** (`SurvivalGame*.cs`, `SurvivalSim*.cs` not yours, `FeelReel.N27.cs`, `UnityTuning.cs`, `Hud*.cs`, `Ui*.cs`,
  `Campaign24.cs`): only through a NEW partial file of your own plus at most ONE call-site line per hook, named in your report.
- A builder that changes a checker its own row is read by gets a critic review of that diff (L-106): name the diff in your report.

## Never
- Run the Unity Editor, a build, a Player, a reel, run_tests or `unity test`; push; merge; commit outside your branch; touch another
  builder's files or rows; edit `.unity` / `.prefab` YAML; add a package; audio; enemy LOOKS / models / new enemy KINDS (rule 7: a "new
  role" for level 4 is an EXISTING kind new to that level); the wall ride's speed; level 6; THE KILN WYRM's AI.
- Stage GG's four files (ProjectSettings/UnityConnectSettings.asset, ProjectSettings/EditorBuildSettings.asset,
  Assets/ProjectSettings/UIEffectProjectSettings.asset, Assets/Scenes/Arena.unity) or the TMP font assets in Assets/Resources/Fonts.
  `git add -A` / `git add .`.
- Leave dotnet bin/obj or scratch copies in the worktree (disk: C: had 14 GB at t0).
- Look rules (CLAUDE.md rule 8): cyan player-usable, red-orange enemies / danger, yellow parryable, magenta RIVAL only, pale violet-white
  boundary; UI text >= 16 px at 1080p; no whole-screen effect; at most 3 flashes a second. A new view copying the stone material passes
  vertex colours or uses `Light54.StoneOrLit`.
- Allocation per sim step; randomness outside `Mulberry32` in sim code; a new idea in code (ideas go to your notes file).

## Research to use
Research/2026-10-21-wave57/: GAPS.md, L.md / G.md / V.md (NIGHT57's builder notes: the causes measured), gate-r3.md / gate-r3b.md (why
each row missed), research/design.md, research/github.md. docs/ROOMS.md (the A-9 card form; "Level 3 as n57-kiln").

## Report (under 300 words), last line
`N58-<letter>: built <ids> · commits <shas>`
with each entry's state, every sha, MAIN and GUARDS before / after, the exact reel command (`bash Tools/night/n47-reels.sh <tag>
<side>,<level>,<scenario>[,<arg>]`, e.g. `l0on,n57-kiln,a158` and `l1on,n59-span,a158`), and the frames the critic must see.
