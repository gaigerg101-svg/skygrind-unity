# NIGHT40 builder brief (the lead's; read it whole, then your card in NIGHT40.md)

Untracked, like NIGHT40.md. Your card is in **C:\Users\gaige\dev\SKYGRIND-unity\NIGHT40.md** (`<cards>`; untracked, so read it by that
absolute path; read `<method>` and, for a boss builder, `<boss_rules>` too). The owner's words: Research/2026-10-14-wave40/GG-REQUEST.md
(verbatim + a row map). Research with URLs: research/boss-web.md, enemies-web.md, feel-web.md; the code facts (file:line, numbers):
research/CODE-MAP.md. Decisions: C:\Users\gaige\dev\SKYGRIND-unity\DECIDE40.md (GG answered D-0 and D-2). Rules: CLAUDE.md,
docs/PRINCIPLES.md, docs/VERBS.md. Last night: NIGHT39-LOG.md (the close table).

## 1. Who you are
One of nine builders on SKYGRIND (Unity 6000.3.7f1, URP; a first-person movement shooter at dusk). You work in your own git worktree (a
branch the harness made from `port/stage3` at the NIGHT40 shared commit 4fa44abe). The lead alone drives the Unity Editor, the builds,
the Players, the reels, the suite and git on `port/stage3`. You own only the files in your card and the builders table.

## 2. Hard rules (CLAUDE.md, restated)
- **Never** run the Unity Editor (no `unity-loop.sh`, no `unity` commands, no Play), a build, a Player, a reel, `run_tests`, or
  `unity test`. Your checks: `dotnet run --project Tools/CoreHarness -c Release` (golden), `dotnet run --project Tools/SimHarness -c
  Release -- <cmd>` (from your worktree root), and `bash Tools/night/dn-check-wt.sh <scratchDir>` from your worktree (a plain directory
  under your own scratchpad). The lead runs every Player check and sends you the frames' paths: name the frames you need in your report
  (the scenario, the moment).
- **One heavy job at a time** in your worktree. **Disk is tight (C: ~18 GB free):** keep scratch output small; delete your own scratch
  build folders when you are done with them.
- **Git:** never commit to `port/stage3` or `master`, never push or merge into `port/stage3`. Commit in YOUR branch with explicit paths
  (+ `.meta` for every new file under Assets/: copy the 2-line form of a neighbouring .cs.meta with a fresh 32-hex guid), never `git add -A`
  / `.`. One commit per slice. Never touch GG's four modified files (ProjectSettings/UnityConnectSettings.asset,
  ProjectSettings/EditorBuildSettings.asset, Assets/ProjectSettings/UIEffectProjectSettings.asset, Assets/Scenes/Arena.unity). Trailer:
  `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- **Before each report:** `git merge port/stage3`. Keep your own rows and HEAD's shared lines on a conflict.
- **Movement exact:** at every commit that touches `Skygrind.Core`: golden 0 mismatches, 22 replays exact. `Core/Mover.cs` only inside the
  `wallride35` branch, and only builder R2.
- **Sim parity:** before each sim commit, `fingerprint` and `fingerprint --off40` both EQUAL. Every sim row behind `N40Blocks`
  (`Core/N40Blocks.cs`); read your row's bit from `sim.n40` (e.g. `n40.loom40`), and in Core code take the flag as a parameter.
- **`--off40` is NIGHT39's game:** never edit a design.n35-n39 row, an n37 / n38 level file or an older generator's output in place.
  Override numbers in your design.n40 row; put level edits in your NEW level id (n40-iceline, n40-kiln, n40-span, n40-spire: copy the n38
  level and its generator to a gen-n40-*.cjs, write Design/levels/n40-*.json and its StreamingAssets copy). Your level row already swaps
  levelFrom -> levelTo (Curriculum25.N40Levels) and gives the weapons by data: `startSet`, `awards` [{room, weapon}], loom40's
  `dropWeapons` (SurvivalSim.N40.cs). The n38 walk pilots (Walk38Kiln / Span / Spire) already answer for their n40 ids.
- **Numbers:** gameplay numbers in `Design/unity-config.json` under `design.n40.<row>` with a `_why` (copy to
  `Assets/StreamingAssets/Design/unity-config.json`; the two stay byte-identical: `cmp`). Patch ONLY your own row, with a `.cjs` script
  written with the Write tool (never `node -e`, never a heredoc). Render / UI numbers in `Assets/Scripts/Game/UnityTuning.<Name>40.cs`, each
  with a comment.
- **Randomness** only from `Mulberry32` seeded per run; no `System.Math` transcendentals in Core (JsMath). Whole-step timers (`StepsFor`).
- **No audio.** Event names only, for GG's sounds (list your new event names in your report).
- **Look:** cyan = player-usable; red-orange = enemies and danger ONLY; yellow = parryable; magenta = RIVAL only; pale violet-white = the
  boundary. UI text >= 16 px at 1080p.
- **Nothing lost (PRINCIPLES 1):** a row that changes a verb says so in your commit and report (VERBS.md row number); tell the lead the
  verbs-reel check change your row needs (the lead owns FeelReel.N35.Verbs.cs).
- **No allocation per sim step** (ZeroAllocTests, AutopilotTests): cache lists at a run's or room's start.
- **Reports:** under 300 words: done / SHORT, every commit sha, every check with its measured result, the frames the lead must take (the
  level, the room, the moment), and the reel scenario name if you added one. Last line: `N40-BUILDER <name>: round <r> · done <k>/<n> ·
  commits <sha list>`.
- **Docs** with the Write / Edit tools only. Notes in `Research/2026-10-14-wave40/<your-name>.md`.
- **Deleting:** only your own scratch files.

## 3. The shared surface (the lead's, committed in 4fa44abe)
- `Core/N40Blocks.cs`: start40 (0), kiln40 (1), cut40 (2), charge40 (3), ride40 (4), iceline40 (5), brawl40 (6), loom40 (7), crown40 (8),
  foes40 (9); `From(cfg)`, `FromBits`, `ForceOff`, `Names`.
- `Game/SurvivalSim.N40.cs`: `n40` (the run's blocks), `n40Override`; declaration-only partials: `N40StartStartRun` / `N40StartStep` (S1),
  `N40ChargeStartRun` (W1), `N40RideStartRun` (R2), `N40IcelineStartRun` (L2), `N40BrawlStartRun` / `N40BrawlStep` (B2),
  `N40LoomStartRun` / `N40LoomStep` (B4), `N40CrownStartRun` / `N40CrownStep` (B5), `N40FoesStartRun` / `N40FoesStep` (E3). Fill yours
  in your OWN file (e.g. `Game/SurvivalSim.N40.Loom.cs`). H1 has no sim hook (a view row).
- `Core/Campaign24.cs`: `waveReady40` (a Func<RoomDef, int, bool>; S1 sets it from its partial).
- Harness partials in `Tools/SimHarness/Program.cs`: `N40Start` (S1), `N40Weapons` (W1), `N40Ride` (R2), `N40Iceline` (L2), `N40Brawl`
  (B2), `N40Loom` (B4), `N40Crown` (B5), `N40Foes` (E3). Implement yours in `Tools/SimHarness/Harness.N40.<Name>.cs` as
  `static partial void N40<Name>(string cmd, string[] args, ref int code)`; use a command prefix of your own (e.g. `n40loom ...`). If the
  harness project lists sources explicitly, add your file the way Harness.N39.*.cs files are listed.
- `design.n40`: every row `enabled: false`. The level rows already hold levelFrom / levelTo; kiln40 `startSet: ["shotgun"]`, iceline40
  `startSet: ["gauntlet"]`, loom40 `dropWeapons: ["nailgun"]`, cut40 `cut: ["n35-finale"]`. Weapon ids: `revolver`, `shotgun`,
  `gauntlet`, `nailgun` (never display names).
- The gate checker: `n38 gates <plan.json>`, `n38 limits <level> <room>`, `n38 envelopes` (Harness.N38.Gates.cs; R2 owns its ride move
  tonight). The walk: see the n38 harness files (`n38f walk foundry`, the kiln / span / spire equivalents).

## 4. Your first hour
1. Read this brief, your card in NIGHT40.md, GG-REQUEST.md, CODE-MAP.md's section for your area, the research section your card names.
2. Prove your card's root cause with one harness line (method 3), build your first checkable slice, check it, commit it, merge
   port/stage3, report. The lead integrates at about 1:30, 2:30 and 3:30 after t0 (t0 = 23:11 local: so ~00:40, 01:40, 02:40) and resumes
   you by message. Your card ends by the round-4 build (~03:30 local); round 4 is fixes from the Player only.
A SHORT with a measured reason beats a late DONE.
