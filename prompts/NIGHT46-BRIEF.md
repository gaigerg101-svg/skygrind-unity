# NIGHT46 builder brief (the lead's; read it whole, then your card in NIGHT46.md)

Untracked, like NIGHT46.md. Your card: **C:\Users\gaige\dev\SKYGRIND-unity\NIGHT46.md** (`<cards>`; read "Where we are", the mistakes
table, `<method>` and `<builders>` too). GG's words are at its top; GG will not test tonight and every call is made in
**C:\Users\gaige\dev\SKYGRIND-unity\DECIDE46.md** (untracked: read it by absolute path). Read before your card, whole:
- Research/2026-10-17-wave46/FLOW-AUDIT.md (where the player stops, ranked, with file:line and JSON paths)
- Research/2026-10-17-wave46/arena-flow-web.md (Part D: the 11-point arena checklist your flow plan answers)
- Research/2026-10-17-wave46/feel-flow-web.md (F32: the flow metric set; V1: all of it)
- Research/2026-10-17-wave46/FORGE.md (what tonight's shared commit placed for you: rows, hooks, reel phases)
Rules: CLAUDE.md, docs/PRINCIPLES.md, docs/DESIGN-RULES.md (A-1..A-9, R, B), docs/ROOMS.md, docs/VERBS.md, docs/NIGHT-ROWS.md.

## 0. The clock (5 h 45 min, hard cap)
t0 = GG's go. Builders start ~0:20. **Arena builders (A1, A2, A3, B1): your first commit is your cards + flow plans, by ~0:50; geometry
only after the lead's PASS (~0:55).** F1's baseline table reaches you ~1:15: use its numbers in your targets. The lead integrates at
**~1:50, ~3:05 and ~4:15**; a report before each gets into that build. After ~4:15: fixes from the Player only. A SHORT with a measured
reason beats a late DONE. Commit every slice (the spend limit has stopped every builder four times). Build most of the time; test with the
harness; do not loop on tiny numbers.

## 1. Who you are
One of six builders on SKYGRIND (Unity 6000.3.7f1, URP; a first-person movement shooter at dusk; pad-first, for Steam and Switch). You
work in your own git worktree (a branch from `port/stage3` at the NIGHT46 shared commit). The lead alone drives the Unity Editor, the
builds, the Players, the reels, the suite and git on `port/stage3`. You own only the files in your card and the builders table.

## 2. Hard rules (CLAUDE.md, restated)
- **Never** run the Unity Editor (no `unity-loop.sh`, no `unity` commands, no Play), a build, a Player, a reel, `run_tests`, or
  `unity test`. Your checks: `dotnet run --project Tools/CoreHarness -c Release` (golden), `dotnet run --project Tools/SimHarness -c
  Release -- <cmd>` (from your worktree root), and `bash Tools/night/dn-check-wt.sh <scratchDir>` from your worktree (a directory of YOUR
  OWN under your scratchpad). The lead runs every Player check: name the frames you need (reel phase, level, room, moment) in your report.
- **Git:** never commit to `port/stage3` or `master`, never push. `git merge` / `git rebase` are blocked for builders: the lead
  cherry-picks your commits; read port/stage3 with `git show port/stage3:<path>`. Never a "sync" commit re-adding the lead's files. Commit
  in YOUR branch with explicit paths (+ a `.meta` for every new file under Assets/: `fileFormatVersion: 2` / `guid: <fresh 32-hex>`),
  never `git add -A` / `.`. One commit per slice. Never touch GG's four modified files (ProjectSettings/UnityConnectSettings.asset,
  ProjectSettings/EditorBuildSettings.asset, Assets/ProjectSettings/UIEffectProjectSettings.asset, Assets/Scenes/Arena.unity), nor HANDOFF.md.
  Trailer: `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- **Builders cannot message each other.** Hand-offs go in your report; the lead passes them on.
- **Movement exact:** at every commit that touches `Skygrind.Core`: golden 0 mismatches, 22 replays exact. Nobody edits `Core/Mover.cs`
  (DECIDE46 N-5): flow is made with geometry, waves, doors, timing and the camera.
- **Sim parity:** before each commit that touches code, `fingerprint`, `fingerprint --off45` and `fingerprint --off46` all EQUAL (tonight's
  rows are off in the composed fingerprint until the freeze).
- **`--off46` is NIGHT45's game:** never edit a design.n27-n45 row, an older level file or an older generator's output in place. Level
  edits only in your n46 level; config only in your own `design.n46.<row>`.
- **Rows as data (docs/NIGHT-ROWS.md):** your sim row is `design.n46.<row>` with `"sim": true` (pre-placed); read it with `rows.On(idXxx46)`
  in your own `SurvivalSim.N46.<Name>.cs` partial through the hooks the shared commit placed; force it in a harness or reel with
  `sim.rowsOverride = sim.rowsRegistry.Defaults().Set("<row>", true)`. No N46Blocks, no tape format change, no edit to an older shared test.
- **Numbers:** in your `design.n46.<row>` with a `_why` (copy to `Assets/StreamingAssets/Design/unity-config.json`; the two byte-identical:
  `cmp`), patched by a `.cjs` script written with the Write tool (never `node -e`, never a heredoc), touching ONLY your row. Render numbers in
  `Assets/Scripts/Game/UnityTuning.<Name>46.cs`, each with a comment.
- **Helpers carry your builder id** (harness / reel helper names). **Level id checks** ask `N45Levels.BaseId(cfg, id)`.
- **Randomness** only from `Mulberry32`; no `System.Math` transcendentals in Core (JsMath). Whole-step timers (`StepsFor`).
- **No audio.** Event names only. **Look:** cyan = player-usable; red-orange = enemies and danger; yellow = parryable; magenta = RIVAL;
  pale violet-white = the boundary. UI text >= 16 px at 1080p. No effect over the whole screen (GG w30).
- **Nothing lost (PRINCIPLES 1):** a change that touches a verb names its VERBS.md row in your commit and report.
- **No allocation per sim step. No Unity API in MonoBehaviour field initializers (L-035).**
- **Feel is seen (L-001):** write your PROOF first (the meter numbers and the frames before / after). You never set `enabled: true` yourself.
- **Reports:** under 300 words: done / SHORT, every commit sha, every check with its measured result, your `n46flow` numbers before / after,
  the frames the lead must take, your reel phase, hand-offs. Last line: `N46-BUILDER <name>: round <r> · done <k>/<n> · commits <sha list>`.
- **Docs** with the Write / Edit tools only. Notes in `Research/2026-10-17-wave46/<your-name>.md`.

## 3. Arena builders: the card, the flow plan, then the geometry
1. **The card** (DESIGN-RULES A-9, every field one line) under your room's heading in docs/ROOMS.md's section "NIGHT46 identity cards".
2. **The flow plan** right under it: arena-flow-web.md Part D's 11 points, one line each, with numbers:
   floor m2 (<= ~900 or the card's reason) · tiers and their heights · the ground loop (node list) round the landmark · the air loop (ride
   walls / grapple perches / rails as a node list) that never touches the floor · the one one-way drop that breaks the circle · each node's
   two outs · the entry lip (what it shows) and the drop in · the first foothold's distance (5-10 m) · jump-height cover per quadrant ·
   the hazard's job (what it guards / which loop it cuts; never the strafe loop) · wave 1's nodes in view from the lip · where waves 2-3
   enter relative to the player's heading (ahead, 12-16 m) · the captain's node · the pickup's exposed spot · the meter target you promise
   (e.g. "doorway dwell under 2 s; stall share under the baseline's by 5 points").
3. Commit both (the card gate reads your commit), then wait for the lead's PASS line (or fix the one line it names). Build only after it.
4. **The geometry** in your generator, each solid's `why` naming its card field and plan line (A-1). A piece no line asks for is cut.
   Change the arena between waves only (a wave from a new side, a crumble), never mid-wave (Titanfall 2's lesson).
5. **Proof:** `n46flow --level <your id>` (F1's meter: the same pilot and seeds as the baseline) before / after; the walk 10 / 10; levelgen
   0 errors; your reel phase's frames named for the lead (each room: from the lip, mid-fight, after a state change).
Systems you may place by data: lava (Lava30, both ways), pours, campaign.lights (fixtures), props with `why` / `job`, ride panels, noride,
plates, crumbling perches, spawn33 tells, afterLesson43 / fallReturn43 / waveNodes43, burrowers, grapple nodes. Something that needs new
sim code goes into your card as "next night" unless your card owns a sim row.

## 4. The shared surface (the lead's: rows-switch-on.cjs + Tools/night/n46-shared.cjs, dry-run by the forge; FORGE.md lists every file)
- `design.n46`: level rows span46 (A1), spire46 (A2), foundry46 / iceline46 (A3), kiln46 (B1) (levelFrom -> levelTo n46-*; placeholders are
  copies of the levelFrom files with the outer name changed); sim rows retry46 / overlap46 / teach46 / reveal46 (F1), wyrm46 / walkin46
  (B1); view rows feel46 (V1), flow46 (F1), shift46 (a leftover). All off.
- Hooks: `SurvivalSim.N46.cs` (F1's `N46Flow*`, B1's `N46Wyrm*` partials, the ids resolved once a run) and Campaign24's hooks: Core
  delegates `retryWave46`, `waveRelease46`, `teachHold46` (null = the old behaviour) that F1 sets in `N46Campaign()`, and the flag
  `walkIn46` read by `Boss36WalkIn` (B1) (FORGE.md has the exact signatures); `Game/FeelReel.N46.cs` (partials N46Span / N46Spire / N46Smooth / N46Wyrm / N46Feel, phases a146-a546). Nobody edits
  FeelReel.N27.cs, Campaign24.cs or SurvivalSim.Rows.cs tonight: ask the lead if a hook is missing.
- Harness: `Tools/SimHarness/Harness.N46.<YourName>.cs` hooked as FORGE.md says, with a `--level <id>` option; older commands are wired
  to their own files: copy what you need (prefixed helpers), never edit theirs. F1's `n46flow` is everyone's meter (from ~1:15).
- Last nights' tools: `n43level gates | cross | walk`, `n43rooms clear | idle`, `n43kiln`, `n43wick wyrm`, `n45ring`, `n45yard`, `n45mix`,
  `n38 gates`, the walks; the reels foundry43, rooms43, kiln43, a145 / a245 / a345.
