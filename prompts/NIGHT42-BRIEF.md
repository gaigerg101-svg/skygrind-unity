# NIGHT42 builder brief (the lead's; read it whole, then your card in NIGHT42.md)

Untracked, like NIGHT42.md. Your card is in **C:\Users\gaige\dev\SKYGRIND-unity\NIGHT42.md** (`<cards>`; untracked, so read it by that
absolute path; read `<method>`, `<builders>` and `<lean_rules>` too). The owner's fifteen asks, verbatim: Research/2026-10-14-wave42/GG-REQUEST.md
(G1-G15). **Today's numbers for every ask (read before designing): Research/2026-10-14-wave42/CODE-FACTS.md** (in your worktree after the
shared commit; else `git show port/stage3:Research/2026-10-14-wave42/CODE-FACTS.md`). Research with URLs: Research/2026-10-14-wave42/research/
hitfeel-web.md and arenas-web.md; last nights': Research/2026-10-14-wave40/research/ (boss-web.md, feel-web.md). Decisions:
C:\Users\gaige\dev\SKYGRIND-unity\DECIDE42.md (every row a default: GG has not answered). Rules: CLAUDE.md, docs/PRINCIPLES.md, docs/VERBS.md.
Last night: NIGHT41-LOG.md (the close table), NIGHT41-REPORT.md.

## 0. The clock (the night lost an hour to a dropped connection)
t0 = 19:00 local. Builders start ~20:50. The lead integrates at **~22:00** and **~23:00** and resumes you by message; a report that lands
before those times gets into that round's build. Your card ends at the round-4 build (**~23:20**); round 4 is fixes from the Player only.
Hard cap 01:00. Keep the card's "keep first" order; **a SHORT with a measured reason beats a late DONE.** Commit every slice.

## 1. Who you are
One of eight builders on SKYGRIND (Unity 6000.3.7f1, URP; a first-person movement shooter at dusk). You work in your own git worktree (a
branch made from `port/stage3` at the NIGHT42 shared commit). The lead alone drives the Unity Editor, the builds, the Players, the reels,
the suite and git on `port/stage3`. You own only the files in your card and the builders table.

## 2. Hard rules (CLAUDE.md, restated)
- **Never** run the Unity Editor (no `unity-loop.sh`, no `unity` commands, no Play), a build, a Player, a reel, `run_tests`, or
  `unity test`. Your checks: `dotnet run --project Tools/CoreHarness -c Release` (golden), `dotnet run --project Tools/SimHarness -c
  Release -- <cmd>` (from your worktree root), and `bash Tools/night/dn-check-wt.sh <scratchDir>` from your worktree (a plain directory
  under your own scratchpad) for the Unity-side code (Game views, tests). The lead runs every Player check: name the frames you need
  (scenario, level, room, moment) in your report.
- **One heavy job at a time** in your worktree. Keep scratch output small; delete your own scratch build folders when done.
- **Git:** never commit to `port/stage3` or `master`, never push. **`git merge` / `git rebase` are blocked for builders**: the lead
  cherry-picks your commits; to read what is on port/stage3 use `git show port/stage3:<path>`. Commit in YOUR branch with explicit paths
  (+ a `.meta` for every new file under Assets/: copy the 2-line form of a neighbouring .cs.meta with a fresh 32-hex guid), never
  `git add -A` / `.`. One commit per slice. Never touch GG's four modified files (ProjectSettings/UnityConnectSettings.asset,
  ProjectSettings/EditorBuildSettings.asset, Assets/ProjectSettings/UIEffectProjectSettings.asset, Assets/Scenes/Arena.unity), nor HANDOFF.md.
  Trailer: `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- **Builders cannot message each other.** Hand-offs go in your report; the lead passes them on.
- **Movement exact:** at every commit that touches `Skygrind.Core`: golden 0 mismatches, 22 replays exact. `Core/Mover.cs` is named
  tonight for **M1 only**, inside the wall-jump / ride jump-off branch, behind wallfling42.
- **Sim parity:** before each sim commit, `fingerprint` and `fingerprint --off42` both EQUAL. Every sim row behind `N42Blocks`
  (`Core/N42Blocks.cs`); read your bit from `sim.n42` (e.g. `n42.gauntlet42`); in Core code take the flag as a field the sim sets at a
  run's start (in your `N42<Area>StartRun` partial body).
- **`--off42` is NIGHT41's game:** never edit a design.n27-n41 row, an older level file or an older generator's output in place.
  Override numbers in your design.n42 row; level edits only in n42-foundry (L1).
- **Numbers:** gameplay numbers in `Design/unity-config.json` under `design.n42.<row>` with a `_why` (copy to
  `Assets/StreamingAssets/Design/unity-config.json`; the two stay byte-identical: `cmp`). Patch ONLY your own row, with a `.cjs` script
  written with the Write tool (never `node -e`, never a heredoc). Render / UI numbers in `Assets/Scripts/Game/UnityTuning.<Name>42.cs`, each
  with a comment. A view row reads `N42View.RowOn(cfg, "<row>")`.
- **Randomness** only from `Mulberry32` seeded per run; no `System.Math` transcendentals in Core (JsMath). Whole-step timers (`StepsFor`).
- **No audio.** Event names only (list your new event names in your report).
- **Look:** cyan = player-usable; red-orange = enemies and danger ONLY; yellow = parryable; magenta = RIVAL only; pale violet-white = the
  boundary. UI text >= 16 px at 1080p.
- **Nothing lost (PRINCIPLES 1):** a row that changes a verb names its VERBS.md row in your commit and report.
- **No allocation per sim step** (ZeroAllocTests, AutopilotTests). **No Unity API in MonoBehaviour field initializers** (L-035).
- **Feel is measured and seen (L-001):** before you build, write your row's PROOF in your notes: the number (harness / telemetry) and the
  reel scenario + frame moment that shows it before / after. A row turns ON only after the lead looked at that pair; you never set
  `enabled: true` yourself. Write your reel scenario (section 3) so the lead can take the frames.
- **Reports:** under 300 words: done / SHORT, every commit sha, every check with its measured result, the frames the lead must take, and
  your reel scenario name. Last line: `N42-BUILDER <name>: round <r> · done <k>/<n> · commits <sha list>`.
- **Docs** with the Write / Edit tools only. Notes in `Research/2026-10-14-wave42/<your-name>.md`.

## 3. The shared surface (the lead's, in the NIGHT42 shared commit)
- `Core/N42Blocks.cs`: wallfling42 (0), boost42 (1), foundry42 (2), gauntlet42 (3), parry42 (4), charge42 (5), impact42 (6), brawl42 (7),
  arena42 (8); `From(cfg)`, `FromBits`, `ForceOff`, `Names`. Tape format 25 records them.
- `Game/SurvivalSim.N42.cs`: `n42`, `n42Override`, `N42BaseId(levelId)` (N41BaseId asks it first: n42-foundry -> n41-foundry ->
  n38-foundry); declaration-only partials, fill yours in your OWN file (e.g. `Game/SurvivalSim.N42.Move.cs`): `N42MoveStartRun` /
  `N42MoveStep` (M1), `N42WeaponsStartRun` / `N42WeaponsStep` (W1), `N42ImpactStartRun` / `N42ImpactStep` (F1), `N42BrawlStartRun` /
  `N42BrawlStep` (B1), `N42ArenaStartRun` / `N42ArenaStep` (K1). They run after the NIGHT41 hooks.
- `Game/N42View.cs`: `RowOn(cfg, row)`, `Off`.
- `Game/FeelReel.N42.cs`: the proof reels' dispatch; fill your `static partial void N42<Name>(SurvivalGame g, string dir, string[] res,
  List<Action> offs, int pre, ref IEnumerator body)` in `FeelReel.N42.<Name>.cs` (scenarios wall42 M1, weapons42 W1, hits42 F1, ui42 U1,
  arena42 K1, first42 L1, brawl42 B1). Copy the shape of NIGHT41's FeelReel.N41.Weapons.cs (its campaign start, DoorFree41, frames).
- Harness partials in `Tools/SimHarness/Program.cs`: `N42Move` (M1), `N42Level` (L1), `N42Weapons` (W1), `N42Impact` (F1), `N42Ui` (U1),
  `N42Brawl` (B1), `N42Arena` (K1). Implement yours in `Tools/SimHarness/Harness.N42.<Name>.cs` as `static partial void N42<Name>(string
  cmd, string[] args, ref int code)` with a command prefix of your own (e.g. `n42move ...`).
- `design.n42`: every row `enabled: false`. Sim rows above; view rows reticle42 (W1), meter42 / style42 / health42 (U1), teach42 (L1),
  kills42 (F1). foundry42: levelFrom n41-foundry -> levelTo n42-foundry; n42-foundry is in design.maps.list with a placeholder (a copy of
  n41-foundry). gauntlet42 already holds `swingSeconds: 1.8` (DECIDE42 D-2).
- Last nights' tools still work: the gates `n38 gates <plan.json>` (Harness.N38.Gates.cs), the walks (`n38f walk foundry` and the kiln /
  span / spire equivalents), `n40brawl ...`, `n41proof slidewall`, the reels' scenarios weapons41 / entrance41 / brawl40casual / level36.
