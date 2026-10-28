# NIGHT44 builder brief (the lead's; read it whole, then your card in NIGHT44.md)

Untracked, like NIGHT44.md. Your card is in **C:\Users\gaige\dev\SKYGRIND-unity\NIGHT44.md** (`<cards>`; read `<method>` and `<builders>`
too). GG's request: Research/2026-10-16-wave44/GG-REQUEST.md (H1-H3). **The forge's spike (each premise with file:line): Research/
2026-10-16-wave44/SPIKE.md.** Last night: NIGHT43-REPORT.md, NIGHT43-LOG.md, Research/2026-10-16-wave43/ (CODE-FACTS-A / B, every builder's
notes: M1 L1 W1 F1 U1 E1 K1 A1 B1 .md). Rules: CLAUDE.md, docs/PRINCIPLES.md, **docs/DESIGN-RULES.md**, docs/ROOMS.md, docs/VERBS.md.
Calls: C:\Users\gaige\dev\SKYGRIND-unity\DECIDE44.md and DECIDE43.md (every row a default until GG answers).

## 0. The clock (2 hours, hard cap)
t0 = GG's go. Builders start ~0:20. The lead integrates at **~0:35** (R2 only), **~0:50** and **~1:12**; a report before those gets into that
build. Your card ends at **~1:10**; after that, fixes from the Player only. A SHORT with a measured reason beats a late DONE. Commit every slice (the spend
limit stopped every builder for 1.6 h last night). Spend your time building; test with the harness; do not loop on tiny numbers.

## 1. Who you are
One of four builders on SKYGRIND (Unity 6000.3.7f1, URP; a first-person movement shooter at dusk). You work in your own git worktree (a
branch made from `port/stage3` at the NIGHT44 shared commit). The lead alone drives the Unity Editor, the builds, the Players, the reels,
the suite and git on `port/stage3`. You own only the files in your card and the builders table.

## 2. Hard rules (CLAUDE.md, restated)
- **Never** run the Unity Editor (no `unity-loop.sh`, no `unity` commands, no Play), a build, a Player, a reel, `run_tests`, or
  `unity test`. Your checks: `dotnet run --project Tools/CoreHarness -c Release` (golden), `dotnet run --project Tools/SimHarness -c
  Release -- <cmd>` (from your worktree root), and `bash Tools/night/dn-check-wt.sh <scratchDir>` from your worktree (a directory of YOUR
  OWN under your scratchpad: last night two builders shared one and got 688 false errors). The lead runs every Player check: name the
  frames you need (scenario, level, room, moment) in your report.
- **Git:** never commit to `port/stage3` or `master`, never push. **`git merge` / `git rebase` are blocked for builders**: the lead
  cherry-picks your commits; read port/stage3 with `git show port/stage3:<path>`. Never make a "sync" commit that re-adds the lead's files
  (it duplicated JSON keys last night): if you need a port/stage3 change, ask the lead. Commit in YOUR branch with explicit paths (+ a
  `.meta` for every new file under Assets/: `fileFormatVersion: 2` / `guid: <fresh 32-hex>`), never `git add -A` / `.`. One commit per
  slice. Never touch GG's four modified files (ProjectSettings/UnityConnectSettings.asset, ProjectSettings/EditorBuildSettings.asset,
  Assets/ProjectSettings/UIEffectProjectSettings.asset, Assets/Scenes/Arena.unity), nor HANDOFF.md. Trailer:
  `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- **Builders cannot message each other.** Hand-offs go in your report; the lead passes them on.
- **Movement exact:** at every commit that touches `Skygrind.Core`: golden 0 mismatches, 22 replays exact. `Core/Mover.cs` is not named
  tonight: nobody edits it.
- **Sim parity:** before each sim commit, `fingerprint` and `fingerprint --off44` both EQUAL. Every sim change behind `N44Blocks`
  (`Core/N44Blocks.cs`: kiln44 K2, reveal44 B2); read your bit from `sim.n44`; in Core code take the flag as a field the sim sets in your
  `N44<Area>StartRun` partial body.
- **`--off44` is NIGHT43's game:** never edit a design.n27-n43 row, an older level file or an older generator's output in place. Numbers in
  your design.n44 row; no new level ids tonight (K2's arena is a new map file).
- **Numbers:** gameplay numbers in `Design/unity-config.json` under `design.n44.<row>` with a `_why` (copy to
  `Assets/StreamingAssets/Design/unity-config.json`; the two stay byte-identical: `cmp`). Patch ONLY your own row, with a `.cjs` script
  written with the Write tool (never `node -e`, never a heredoc). Render / UI numbers in `Assets/Scripts/Game/UnityTuning.<Name>44.cs`, each
  with a comment. A view row reads `N44View.RowOn(cfg, "vis44")` (V2).
- **Helpers carry your prefix:** harness and reel helper names start with your builder id (N43Arg clashed between two builders last night).
- **Randomness** only from `Mulberry32` seeded per run; no `System.Math` transcendentals in Core (JsMath). Whole-step timers (`StepsFor`).
- **No audio.** Event names only (list new event names in your report).
- **Look:** cyan = player-usable; red-orange = enemies and danger; yellow = parryable; magenta = RIVAL only; FUSION's hot pink only in the
  FUSION burst; pale violet-white = the boundary. UI text >= 16 px at 1080p. Fonts: the theme's (UnityTuning.Theme.cs).
- **Nothing lost (PRINCIPLES 1):** a change that touches a verb names its VERBS.md row in your commit and report.
- **No allocation per sim step** (ZeroAllocTests, AutopilotTests). **No Unity API in MonoBehaviour field initializers** (L-035).
- **Feel is measured and seen (L-001):** before you build, write your PROOF in your notes: the number and the reel frames before / after.
  You never set `enabled: true` yourself.
- **Reports:** under 300 words: done / SHORT, every commit sha, every check with its measured result, the frames the lead must take, your
  reel scenario name, hand-offs. Last line: `N44-BUILDER <name>: round <r> · done <k>/<n> · commits <sha list>`.
- **Docs** with the Write / Edit tools only. Notes in `Research/2026-10-16-wave44/<your-name>.md`.

## 3. The shared surface (the lead's, in the NIGHT44 shared commit; built from NIGHT43's scripts with 43 -> 44)
- `Core/N44Blocks.cs`: kiln44 (0, K2), reveal44 (1, B2); `From(cfg)`, `FromBits`, `ForceOff`, `Names`. Tape format 27 records them.
- `Game/SurvivalSim.N44.cs`: `n44`, `n44Override`; declaration-only partials, fill yours in your OWN file: `N44KilnStartRun` / `N44KilnStep` (K2), `N44RevealStartRun` / `N44RevealStep`
  (B2). They run after the NIGHT43 hooks.
- `Game/N44View.cs`: `RowOn(cfg, row)`, `Off` (V2's vis44).
- `Game/FeelReel.N44.cs`: the dispatch: `proof44` (R2: FeelReel.N44.Proof.cs; R2 also fixes the NIGHT43 reels it owns in place), `ring44`
  (K2: FeelReel.N44.Ring.cs), `look44` (V2: FeelReel.N44.Look.cs), `reveal44` (B2: FeelReel.N44.Reveal.cs): fill `static partial void N44<Name>(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre,
  ref IEnumerator body)` in `FeelReel.N44.<Name>.cs`.
- Harness partials in `Tools/SimHarness/Program.cs`: `N44Proof` (R2), `N44Kiln` (K2), `N44Reveal` (B2): implement in
  `Tools/SimHarness/Harness.N44.<Name>.cs` as `static partial void N44<Name>(string cmd, string[] args, ref int code)`, commands prefixed
  `n44proof` / `n44kiln` / `n44reveal`.
- `design.n44`: every row `enabled: false`; kiln44 holds `arena: "Design/maps/wyrm44-arena.json"` (no new level id: kiln44 makes n43-kiln
  load THE RING's new arena). `N44Events.BossStart` (payload {boss, position}) fires where every boss starts its fight (B2 subscribes to
  it; nobody edits the bosses' files).
- Last nights' tools still work: NIGHT43's harness commands (n43move, n43level, n43rooms, n43kiln, n43burrow, n43wick), the gates
  `n38 gates <plan.json>`, the walks, the reels (move43, foundry43, weapons43, feel43, ui43, rooms43, kiln43, burrow43, wick43, wick37).
- To test with a row on: `sim.n44Override = new N44Blocks { kiln44 = true }` (as NIGHT43's harnesses set n43Override).
