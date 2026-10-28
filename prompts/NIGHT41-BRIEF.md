# NIGHT41 builder brief (the lead's; read it whole, then your card in NIGHT41.md)

Untracked, like NIGHT41.md. Your card is in **C:\Users\gaige\dev\SKYGRIND-unity\NIGHT41.md** (`<cards>`; untracked, so read it by that
absolute path; read `<method>` and `<lean_rules>` too). The owner's request: Research/2026-10-14-wave41/GG-REQUEST.md; the full list with
evidence and the code facts: Research/2026-10-14-wave41/INVENTORY.md (your card names its rows). Research with URLs from last night:
Research/2026-10-14-wave40/research/ (boss-web.md, enemies-web.md, feel-web.md) and the code map research/CODE-MAP.md. Decisions:
C:\Users\gaige\dev\SKYGRIND-unity\DECIDE41.md (every row a default: GG has not answered tonight) and DECIDE40.md. Rules: CLAUDE.md,
docs/PRINCIPLES.md, docs/VERBS.md. Last night: NIGHT40-LOG.md (the close table), NIGHT40-REPORT.md.

## 1. Who you are
One of eight builders on SKYGRIND (Unity 6000.3.7f1, URP; a first-person movement shooter at dusk). You work in your own git worktree (a
branch the harness made from `port/stage3` at the NIGHT41 shared commit). The lead alone drives the Unity Editor, the builds, the
Players, the reels, the suite and git on `port/stage3`. You own only the files in your card and the builders table.

## 2. Hard rules (CLAUDE.md, restated)
- **Never** run the Unity Editor (no `unity-loop.sh`, no `unity` commands, no Play), a build, a Player, a reel, `run_tests`, or
  `unity test`. Your checks: `dotnet run --project Tools/CoreHarness -c Release` (golden), `dotnet run --project Tools/SimHarness -c
  Release -- <cmd>` (from your worktree root), and `bash Tools/night/dn-check-wt.sh <scratchDir>` from your worktree (a plain directory
  under your own scratchpad; the lead tested it with a planted error). The lead runs every Player check and sends you the frames' paths:
  name the frames you need in your report (the scenario, the level, the room, the moment).
- **One heavy job at a time** in your worktree (a dotnet build or a harness run). Keep scratch output small; delete your own scratch build
  folders when you are done with them.
- **Git:** never commit to `port/stage3` or `master`, never push. **`git merge` and `git rebase` are blocked for builders** (the permission
  classifier refuses them): the lead cherry-picks your commits; to read what is on port/stage3 (another builder's integrated work) use
  `git show port/stage3:<path>`. Commit in YOUR branch with explicit paths (+ `.meta` for every new file under Assets/: copy the 2-line form
  of a neighbouring .cs.meta with a fresh 32-hex guid), never `git add -A` / `.`. One commit per slice. Never touch GG's four modified files
  (ProjectSettings/UnityConnectSettings.asset, ProjectSettings/EditorBuildSettings.asset, Assets/ProjectSettings/UIEffectProjectSettings.asset,
  Assets/Scenes/Arena.unity). Trailer: `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- **Builders cannot message each other.** A hand-off (E6's DRONE placement for R5's level 5, a weak point P3's aim needs from B8) goes in
  your report; the lead passes it on.
- **Movement exact:** at every commit that touches `Skygrind.Core`: golden 0 mismatches, 22 replays exact. `Core/Mover.cs` is untouched
  tonight (no card names it).
- **Sim parity:** before each sim commit, `fingerprint` and `fingerprint --off41` both EQUAL. Every sim row behind `N41Blocks`
  (`Core/N41Blocks.cs`); read your row's bit from `sim.n41` (e.g. `n41.sweep41`), and in Core code take the flag as a parameter or a field
  the sim sets at a run's start.
- **`--off41` is NIGHT40's game:** never edit a design.n35-n40 row, an n37 / n38 / n40 level file or an older generator's output in place.
  Override numbers in your design.n41 row; put level edits in your NEW level id (n41-foundry, n41-kiln, n41-spire, n41-finale: copy the
  older generator to a gen-n41-*.cjs, write Design/levels/n41-*.json and its Assets/StreamingAssets copy). Each n41 id is already in
  design.maps.list with a placeholder file (a copy of its levelFrom with the name changed); your level row already swaps levelFrom ->
  levelTo (Curriculum25.N41Levels); an n41 level reads as its levelFrom to every older system (`sim.N41BaseId(id)`: NIGHT40's start sets /
  awards, kiln40's boost chain, the walk pilots Walk38Foundry / Kiln / Spire, the verbs reel's start rules).
- **Old rows on through n41 rows, never in place:** juice38 = design.n38.juice38 || design.n41.juice41On; doors38 = design.n38.doors38 ||
  design.n41.doors41On; pump40 = design.n40.pump40 || design.n41.pump41On (the lead's hooks are in Juice38.cs, Doors38View.cs,
  Viewmodel.Pump40.cs). A view row reads `N41View.RowOn(cfg, "<row>")` (off with -skygrindOff41 and every older switch).
- **Numbers:** gameplay numbers in `Design/unity-config.json` under `design.n41.<row>` with a `_why` (copy to
  `Assets/StreamingAssets/Design/unity-config.json`; the two stay byte-identical: `cmp`). Patch ONLY your own row, with a `.cjs` script
  written with the Write tool (never `node -e`, never a heredoc). Render / UI numbers in `Assets/Scripts/Game/UnityTuning.<Name>41.cs`, each
  with a comment.
- **Randomness** only from `Mulberry32` seeded per run; no `System.Math` transcendentals in Core (JsMath). Whole-step timers (`StepsFor`).
- **No audio.** Event names only, for GG's sounds (list your new event names in your report).
- **Look:** cyan = player-usable; red-orange = enemies and danger ONLY; yellow = parryable; magenta = RIVAL only; pale violet-white = the
  boundary. UI text >= 16 px at 1080p. Room light is never cyan or red-orange.
- **Nothing lost (PRINCIPLES 1):** a row that changes a verb says so in your commit and report (VERBS.md row number); tell the lead the
  verbs-reel check change your row needs.
- **No allocation per sim step** (ZeroAllocTests, AutopilotTests): cache lists at a run's or room's start.
- **Reports:** under 300 words: done / SHORT, every commit sha, every check with its measured result, the frames the lead must take (the
  scenario, the level, the room, the moment), and the reel scenario name if you added one. Last line: `N41-BUILDER <name>: round <r> ·
  done <k>/<n> · commits <sha list>`.
- **Docs** with the Write / Edit tools only. Notes in `Research/2026-10-14-wave41/<your-name>.md`.
- **Deleting:** only your own scratch files.

## 3. The shared surface (the lead's, in the NIGHT41 shared commit)
- `Core/N41Blocks.cs`: sweep41 (0), tokens41 (1), heavy41 (2), aim41 (3), foundry41 (4), kiln41 (5), spire41 (6), finale41 (7); `From(cfg)`,
  `FromBits`, `ForceOff`, `Names`. Tape format 24 records them.
- `Game/SurvivalSim.N41.cs`: `n41` (the run's blocks), `n41Override`, `N41BaseId(levelId)`; declaration-only partials (fill yours in your
  OWN file, e.g. `Game/SurvivalSim.N41.Loom.cs`): `N41LoomStartRun` / `N41LoomStep` and `N41BrawlStartRun` / `N41BrawlStep` (B8),
  `N41FoesStartRun` / `N41FoesStep` (E6), `N41AimStartRun` / `N41CrownStep` (P3). They run after the NIGHT40 hooks.
- `Core/WeakPoint41.cs` + `Enemy.weakPoint41` (Core/Combat.cs): a boss body's weak point (`Set(at)`, `Clear()`, `active`, `at`); inactive
  by default. B8 fills THE LOOM's and THE ICEBREAKER's bodies, P3 THE CROWN's; P3's aim41 reads `e.weakPoint41.active ? e.weakPoint41.at :
  e.AimPoint`.
- `Loadout.Heavy41(Enemy enemy, ref double knock, ref double knockTime, ref double hitstop)` (Core/Combat.cs, a declaration-only partial at
  the gauntlet swing's knock; E6 writes its body in a new `Core/Heavy41.cs` as `public sealed partial class Loadout`); called only while
  `loadout.heavy41` (set from sim.n41 at a run's start); a hitstop > 0 goes to SurvivalSim.Hitstop(s, "heavy41").
- `Game/N41View.cs`: `RowOn(cfg, row)`, `Off`, `BossBar41Live` (J2's UiBossBar41 sets it while it draws; the HUD33 bar and the Boss37
  stagger bar hide while it is true: the lead's hooks in UiHud33.Boss33 and Boss37StaggerBar.Show).
- Harness partials in `Tools/SimHarness/Program.cs`: `N41Proof` (T2), `N41Juice` (J2), `N41Look` (K2), `N41Bosses` (B8), `N41Enemies` (E6),
  `N41Pad` (P3), `N41Levels` (R5), `N41Finale` (L6). Implement yours in `Tools/SimHarness/Harness.N41.<Name>.cs` as
  `static partial void N41<Name>(string cmd, string[] args, ref int code)`; use a command prefix of your own (e.g. `n41loom ...`). The
  harness compiles Core/**, Game/SurvivalSim*.cs, Game/Bot/*.cs, Game/Tape/*.cs (see Tools/SimHarness/SimHarness.csproj).
- `design.n41`: every row `enabled: false`. Sim rows above; view rows juice41, juice41On, doors41On, bars41, entrance41, select41 (J2),
  pump41On (T2), look41 (K2), brawlLook41 (B8), models41 (E6). Level rows hold levelFrom / levelTo: foundry41 n38-foundry -> n41-foundry,
  kiln41 n40-kiln -> n41-kiln, spire41 n40-spire -> n41-spire, finale41 -> n41-finale (added as the last level when on; OFF tonight).
- Last night's tools still work: the gates `n38 gates <plan.json>` / `n38 limits` / `n38 envelopes` (Harness.N38.Gates.cs; the ride move
  is R2's from NIGHT40), the walks (`n38f walk foundry` and the kiln / span / spire equivalents; `n40ice walk`; `n40foes ...`), THE LOOM's
  `n40loom stay`, THE CROWN's `n40crown defects`, THE ICEBREAKER's `n40brawl ...`.

## 4. Your first hour
1. Read this brief, your card in NIGHT41.md, the INVENTORY rows it names, CODE-MAP.md's section for your area.
2. Prove your card's root cause with one harness line (or, for a view card, name the file:line that draws it), build your first checkable
   slice, check it, commit it, report. t0 = 09:06 local. The lead integrates at about 1:30, 2:30 and 3:30 after t0 (~10:35, 11:35, 12:35)
   and resumes you by message; reports arriving just before those times get into that round's build. Your card ends by the round-4 build
   (~13:25 local); round 4 is fixes from the Player only.
A SHORT with a measured reason beats a late DONE.
