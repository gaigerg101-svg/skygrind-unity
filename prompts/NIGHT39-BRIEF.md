# NIGHT39 builder brief (the lead's; read it whole, then your card in NIGHT39.md)

Untracked, like NIGHT39.md. Your card is in **C:\Users\gaige\dev\SKYGRIND-unity\NIGHT39.md** (`<cards>`; untracked, so read it by that
absolute path). The owner's words: Research/2026-10-14-wave39/GG-REQUEST.md (verbatim + a row map). Research with URLs:
research/feel-web.md, research/arena-boss-web.md. Decisions: DECIDE39.md. Rules: CLAUDE.md, docs/PRINCIPLES.md, docs/VERBS.md. Last
night: NIGHT38-LOG.md (the integration record and the close table: read the rows for your area).

## 1. Who you are
One of seven builders on SKYGRIND (Unity 6000.3.7f1, URP; a first-person movement shooter at dusk). You work in your own git worktree
(a branch the harness made from `port/stage3` at the NIGHT39 shared commit). The lead alone drives the Unity Editor, the builds, the
Players, the suite and git on `port/stage3`. You own only the files in your card's "Owns" line and the builders table.

## 2. Hard rules (CLAUDE.md, restated; the same as NIGHT38's)
- **Never** run the Unity Editor (no `unity-loop.sh`, no `unity` commands, no Play), a build, a Player, `run_tests`, or `unity test`. Your
  checks: `dotnet run --project Tools/CoreHarness -c Release` (golden), `dotnet run --project Tools/SimHarness -c Release -- <cmd>`, and
  `bash Tools/night/dn-check-wt.sh <scratchDir>` from your worktree (a plain directory under your own scratchpad).
- **One heavy job at a time** in your worktree (free memory was 13 GB at t0).
- **Git:** never commit to `port/stage3` or `master`, never push or merge into `port/stage3`. Commit in YOUR branch with explicit paths
  (+ `.meta` for new `.cs` files), never `git add -A` / `.`. One commit per slice. Never touch GG's four modified files
  (ProjectSettings/UnityConnectSettings.asset, ProjectSettings/EditorBuildSettings.asset, Assets/ProjectSettings/UIEffectProjectSettings.asset,
  Assets/Scenes/Arena.unity). Trailer: `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- **Before each report:** `git merge port/stage3`. Keep your own rows and HEAD's shared lines on a conflict.
- **Movement exact:** at every commit that touches `Skygrind.Core`: golden 0 mismatches, 22 replays exact. `Core/Mover.cs` only inside the
  `wallride35` branch, and only builder R1.
- **Sim parity:** before each sim commit, `fingerprint` and `fingerprint --off39` both EQUAL. Every sim row behind `N39Blocks`
  (`Core/N39Blocks.cs`: p39, ride39, boss39); it reads its own `design.n39.<row>.enabled`. slow39 is loop pacing: no bit.
- **Numbers:** gameplay numbers in `Design/unity-config.json` under `design.n39.<row>` with a `_why` (copy to
  `Assets/StreamingAssets/Design/unity-config.json`; the two stay byte-identical). Patch ONLY your own row, with a `.cjs` script written
  with the Write tool (never `node -e`, never a heredoc). Render / UI numbers in `Assets/Scripts/Game/UnityTuning.<Name>39.cs`, each with a
  comment. **Never edit design.n35.wallride35 or design.n37.* in place:** override them in your design.n39 row, so `--off39` gives NIGHT38.
- **Randomness** only from `Mulberry32` seeded per run; no `System.Math` transcendentals in Core (JsMath).
- **No audio.** Event names only, for GG's sounds.
- **Look:** cyan = player-usable; red-orange = enemies and danger ONLY; magenta = RIVAL only; pale violet-white = the boundary. UI text
  >= 16 px at 1080p.
- **Nothing lost (PRINCIPLES 1):** a row that changes a verb says so in your commit and report (VERBS.md row number).
- **Reports:** under 300 words: done / SHORT, every commit sha, every check with its measured result, what the lead must do (frames to
  take, with the exact moment). Last line: `N39-BUILDER <name>: round <r> · done <k>/<n> · commits <sha list>`.
- **Docs** with the Write / Edit tools only. Notes in `Research/2026-10-14-wave39/<your-name>.md`.
- **Deleting:** only your own scratch files.

## 3. The shared surface (the lead's, committed in the NIGHT39 shared commit)
- `Core/N39Blocks.cs`: p39 (bit 0), ride39 (bit 1), boss39 (bit 2); `From(cfg)`, `FromBits`, `ForceOff`.
- `Game/SurvivalSim.N39.cs`: `n39` (the run's blocks), `n39Override`; declaration-only partials: `N39PistolStartRun` / `N39PistolStep`
  (P1), `N39RideStartRun` (R1), `N39BossStartRun` / `N39BossStep` (B1). Fill one in your OWN file (e.g. `Game/SurvivalSim.N39.Pistol.cs`).
- Harness partials in `Tools/SimHarness/Program.cs`: `N39Pistol` (P1), `N39Ride` (R1), `N39Slow` (F1), `N39Boss` (B1), `N39Views` (V); K4 uses
  the existing `N38Kiln` (Harness.N38.Kiln.cs), E the existing `N38Gates`. Implement yours in `Tools/SimHarness/Harness.N39.<Owner>.cs` as
  `static partial void N39<Owner>(string cmd, string[] args, ref int code)`.
- `design.n39`: p39, ride39, boss39, slow39 (every row `enabled: false`). The level row for K4 is the existing `design.n38.kiln38`.
- `--off39` (harness) / `-skygrindOff39` (Player) turn every NIGHT39 block off. Tape format 22.
- The ghost weapon is fixed (n35-kiln's award is `shotgun`; d52c5042). Use weapon ids (`revolver`, `shotgun`, `gauntlet`, `nailgun`) in
  every award, never display names.
- The gate checker (E's): `n38 gates <plan.json>`, `n38 limits <level> <room>`, `n38 envelopes`. Level files: Design/levels/n38/*.

## 4. Your first hour
1. Read this brief, your card in NIGHT39.md, GG-REQUEST.md, the research section your card names, and the files your card names.
2. Build your first checkable slice; check it; commit it; merge port/stage3; report. The lead integrates at about 1:30, 2:30 and 3:30
   after t0 (t0 = 14:29 local) and resumes you by message.
A SHORT with a measured reason beats a late DONE.
