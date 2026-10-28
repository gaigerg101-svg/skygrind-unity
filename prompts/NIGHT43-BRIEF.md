# NIGHT43 builder brief (the lead's; read it whole, then your card in NIGHT43.md)

Untracked, like NIGHT43.md. Your card is in **C:\Users\gaige\dev\SKYGRIND-unity\NIGHT43.md** (`<cards>`; untracked, so read it by that
absolute path; read `<method>` and `<builders>` too). The owner's thirty asks, verbatim: Research/2026-10-16-wave43/GG-REQUEST.md (G1-G30).
**Today's numbers (read before designing): Research/2026-10-16-wave43/CODE-FACTS-A.md (movement, level 1, doors) and CODE-FACTS-B.md
(weapons, bosses, enemies, UI, level 3).** Research with URLs: Research/2026-10-16-wave43/research/ (movement-web, combat-web,
encounters-arenas-web, ui-art-web, bosses-web); older: Research/2026-10-14-wave42/research/ (hitfeel-web, arenas-web). The design rules:
docs/PRINCIPLES.md now, docs/DESIGN-RULES.md when the lead lands it (`git show port/stage3:docs/DESIGN-RULES.md`). Rules: CLAUDE.md,
docs/VERBS.md. Last night: NIGHT42-REPORT.md, NIGHT42-LOG.md, DECIDE42.md; tonight's calls: C:\Users\gaige\dev\SKYGRIND-unity\DECIDE43.md.

## 0. The clock
t0 = 16:00 local. Builders start ~16:50. The lead integrates at **~18:00**, **~19:00** and **~20:00** and resumes you by message; a report
that lands before those times gets into that round's build. Your card ends at **~20:00**; after that, fixes from the Player only.
Keep the card's "keep first" order; **a SHORT with a measured reason beats a late DONE.** Commit every slice. GG asked for real building:
spend most of your time building, test with the harness, do not loop on tiny numbers.

## 1. Who you are
One of nine builders on SKYGRIND (Unity 6000.3.7f1, URP; a first-person movement shooter at dusk). You work in your own git worktree (a
branch made from `port/stage3` at the NIGHT43 shared commit a9da30f6). The lead alone drives the Unity Editor, the builds, the Players, the
reels, the suite and git on `port/stage3`. You own only the files in your card and the builders table.

## 2. Hard rules (CLAUDE.md, restated)
- **Never** run the Unity Editor (no `unity-loop.sh`, no `unity` commands, no Play), a build, a Player, a reel, `run_tests`, or
  `unity test`. Your checks: `dotnet run --project Tools/CoreHarness -c Release` (golden), `dotnet run --project Tools/SimHarness -c
  Release -- <cmd>` (from your worktree root), and `bash Tools/night/dn-check-wt.sh <scratchDir>` from your worktree (a plain directory
  under your own scratchpad) for the Unity-side code (Game views, tests). The lead runs every Player check: name the frames you need
  (scenario, level, room, moment) in your report.
- **One heavy job at a time** in your worktree. Keep scratch output small; delete your own scratch build folders when done.
- **Git:** never commit to `port/stage3` or `master`, never push. **`git merge` / `git rebase` are blocked for builders**: the lead
  cherry-picks your commits; to read what is on port/stage3 use `git show port/stage3:<path>`. Commit in YOUR branch with explicit paths
  (+ a `.meta` for every new file under Assets/: the 2-line form `fileFormatVersion: 2` / `guid: <fresh 32-hex>`), never `git add -A` / `.`.
  One commit per slice. Never touch GG's four modified files (ProjectSettings/UnityConnectSettings.asset, ProjectSettings/EditorBuildSettings.asset,
  Assets/ProjectSettings/UIEffectProjectSettings.asset, Assets/Scenes/Arena.unity), nor HANDOFF.md. Trailer:
  `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- **Builders cannot message each other.** Hand-offs go in your report; the lead passes them on.
- **Movement exact:** at every commit that touches `Skygrind.Core`: golden 0 mismatches, 22 replays exact. `Core/Mover.cs` is named
  tonight for **M1 only**, behind chimney43 / ride43 / boost43.
- **Sim parity:** before each sim commit, `fingerprint` and `fingerprint --off43` both EQUAL. Every sim row behind `N43Blocks`
  (`Core/N43Blocks.cs`); read your bit from `sim.n43` (e.g. `n43.parry43`); in Core code take the flag as a field the sim sets at a run's
  start (in your `N43<Area>StartRun` partial body).
- **`--off43` is NIGHT42's game:** never edit a design.n27-n42 row, an older level file or an older generator's output in place. Override
  numbers in your design.n43 row; level edits only in your n43 level.
- **Numbers:** gameplay numbers in `Design/unity-config.json` under `design.n43.<row>` with a `_why` (copy to
  `Assets/StreamingAssets/Design/unity-config.json`; the two stay byte-identical: `cmp`). Patch ONLY your own row, with a `.cjs` script
  written with the Write tool (never `node -e`, never a heredoc). Render / UI numbers in `Assets/Scripts/Game/UnityTuning.<Name>43.cs`, each
  with a comment. A view row reads `N43View.RowOn(cfg, "<row>")`.
- **Randomness** only from `Mulberry32` seeded per run; no `System.Math` transcendentals in Core (JsMath). Whole-step timers (`StepsFor`).
- **No audio.** Event names only (list your new event names in your report).
- **Look:** cyan = player-usable; red-orange = enemies and danger; yellow = parryable; magenta = RIVAL only; pale violet-white = the
  boundary; FUSION's hot pink (W1 / F1) only in the FUSION burst. UI text >= 16 px at 1080p. Fonts: the theme's (UnityTuning.Theme.cs).
- **Nothing lost (PRINCIPLES 1):** a row that changes a verb names its VERBS.md row in your commit and report.
- **No allocation per sim step** (ZeroAllocTests, AutopilotTests). **No Unity API in MonoBehaviour field initializers** (L-035).
- **A new Mover field** goes into RunSnapshot.MoverState's Fields or Skipped (Core RunSnapshotTests) (M1).
- **Feel is measured and seen (L-001):** before you build, write your row's PROOF in your notes: the number and the reel frames before /
  after. You never set `enabled: true` yourself. Write your reel scenario (section 3) so the lead can take the frames.
- **Reports:** under 300 words: done / SHORT, every commit sha, every check with its measured result, the frames the lead must take, your
  reel scenario name, hand-offs. Last line: `N43-BUILDER <name>: round <r> · done <k>/<n> · commits <sha list>`.
- **Docs** with the Write / Edit tools only. Notes in `Research/2026-10-16-wave43/<your-name>.md`.

## 3. The shared surface (the lead's, in the NIGHT43 shared commit a9da30f6)
- `Core/N43Blocks.cs`: chimney43 (0), ride43 (1), boost43 (2), foundry43 (3), gauntlet43 (4), parry43 (5), fusion43 (6), rooms43 (7),
  iceline43 (8), kiln43 (9), burrow43 (10), wick43 (11); `From(cfg)`, `FromBits`, `ForceOff`, `Names`. Tape format 26 records them.
  Also: `N43Events` (Fusion "fusion43", BurrowTell / BurrowEmerge / BurrowCounter, BossLeg, BossOpen, RoomClear) and the payloads
  `Fusion43Event`, `Burrow43Event`, `Boss43Event`, `Boss43State`.
- `Game/SurvivalSim.N43.cs`: `n43`, `n43Override`, `N43BaseId(levelId)` (N42BaseId asks it first: n43-foundry -> n42-foundry; n43-iceline
  -> n40-iceline; n43-kiln -> n41-kiln); declaration-only partials, fill yours in your OWN file (e.g. `Game/SurvivalSim.N43.Move.cs`):
  `N43MoveStartRun` / `N43MoveStep` (M1), `N43WeaponsStartRun` / `N43WeaponsStep` (W1), `N43RoomsStartRun` / `N43RoomsStep` +
  `N43RoomLeftCount(ref int n)` (E1), `N43BurrowStartRun` / `N43BurrowStep` (A1), `N43WickStartRun` / `N43WickStep` +
  `N43BossStateFill(ref Boss43State s)` (B1), `N43KilnStartRun` / `N43KilnStep` (K1). Read-side: `sim.N43RoomLeft` (int, -1 none) and
  `sim.N43BossState` (U1). They run after the NIGHT42 hooks.
- `Game/N43View.cs`: `RowOn(cfg, row)`, `Off`.
- `Game/FeelReel.N43.cs`: the proof reels' dispatch; fill your `static partial void N43<Name>(SurvivalGame g, string dir, string[] res,
  List<Action> offs, int pre, ref IEnumerator body)` in `FeelReel.N43.<Name>.cs` (scenarios move43 M1, foundry43 L1, weapons43 W1, feel43 F1,
  ui43 U1, rooms43 E1, kiln43 K1, burrow43 A1, wick43 B1). Copy the shape of NIGHT42's FeelReel.N42.*.cs (their campaign start, DoorFree41,
  frames).
- Harness partials in `Tools/SimHarness/Program.cs`: `N43Move` (M1), `N43Level` (L1), `N43Weapons` (W1), `N43Rooms` (E1), `N43Kiln` (K1),
  `N43Burrow` (A1), `N43Wick` (B1). Implement yours in `Tools/SimHarness/Harness.N43.<Name>.cs` as `static partial void N43<Name>(string
  cmd, string[] args, ref int code)` with a command prefix of your own (e.g. `n43move ...`). F1 and U1 (views) have none: dn-check +
  EditMode tests of your pure helpers.
- `design.n43`: every row `enabled: false`. Level rows: foundry43 (n42-foundry -> n43-foundry), iceline43 (n40-iceline -> n43-iceline),
  kiln43 (n41-kiln -> n43-kiln); placeholders in design.maps.list (copies with the outer name changed; n43-kiln's data.name stays
  "n40-kiln" like n41-kiln's). gauntlet43 already holds `swingSeconds: 1.25`.
- Last nights' tools still work: the gates `n38 gates <plan.json>` (Harness.N38.Gates.cs), the walks (`n38f walk foundry` and the kiln /
  span / spire equivalents), `n40brawl ...`, `n41proof slidewall`, NIGHT42's commands (n42move, n42level, ...), the reels.
- To test your row on: harness runs can force a row on (look at how NIGHT42's Harness.N42.*.cs set `sim.n42Override`); do the same with
  `sim.n43Override` (e.g. `new N43Blocks { chimney43 = true, ride43 = true }`).
