# NIGHT45 builder brief (the lead's; read it whole, then your card in NIGHT45.md)

Untracked, like NIGHT45.md. Your card: **C:\Users\gaige\dev\SKYGRIND-unity\NIGHT45.md** (`<cards>`; read "Where we are", the mistakes
table, `<method>` and `<builders>` too). GG's request: Research/2026-10-17-wave45/GG-REQUEST.md (J1-J6). **The audit (every fight room,
the fluff, the repeated mistakes): Research/2026-10-17-wave45/AUDIT.md. The research: Research/2026-10-17-wave45/research/arena-identity.md.**
Rules: CLAUDE.md, docs/PRINCIPLES.md, **docs/DESIGN-RULES.md (A-9: the arena identity card before any piece)**, docs/ROOMS.md,
docs/VERBS.md. Calls: C:\Users\gaige\dev\SKYGRIND-unity\DECIDE45.md (+ DECIDE43 / DECIDE44). Last nights: NIGHT43-REPORT.md,
NIGHT44-LOG.md, Research/2026-10-16-wave43/ (each builder's notes, e.g. L1.md, K1.md, A1.md, E1.md for the generators you start from).

## 0. The clock (2 hours, hard cap)
t0 = GG's go. Builders start ~0:10. The lead integrates at **~0:45** and **~1:15**; a report before those gets into that build. Your card
ends at **~1:15**; after that, fixes from the Player only. A SHORT with a measured reason beats a late DONE. Commit every slice (the spend
limit stopped every builder twice in four nights). Build most of the time; test with the harness; do not loop on tiny numbers.

## 1. Who you are
One of five builders on SKYGRIND (Unity 6000.3.7f1, URP; a first-person movement shooter at dusk). You work in your own git worktree (a
branch made from `port/stage3` at the NIGHT45 shared commit). The lead alone drives the Unity Editor, the builds, the Players, the reels,
the suite and git on `port/stage3`. You own only the files in your card and the builders table.

## 2. Hard rules (CLAUDE.md, restated)
- **Never** run the Unity Editor (no `unity-loop.sh`, no `unity` commands, no Play), a build, a Player, a reel, `run_tests`, or
  `unity test`. Your checks: `dotnet run --project Tools/CoreHarness -c Release` (golden), `dotnet run --project Tools/SimHarness -c
  Release -- <cmd>` (from your worktree root), and `bash Tools/night/dn-check-wt.sh <scratchDir>` from your worktree (a directory of YOUR
  OWN under your scratchpad). The lead runs every Player check: name the frames you need (scenario, level, room, moment) in your report.
- **Git:** never commit to `port/stage3` or `master`, never push. `git merge` / `git rebase` are blocked for builders: the lead
  cherry-picks your commits; read port/stage3 with `git show port/stage3:<path>`. Never make a "sync" commit that re-adds the lead's
  files (it duplicated JSON keys on NIGHT43): ask the lead. Commit in YOUR branch with explicit paths (+ a `.meta` for every new file
  under Assets/: `fileFormatVersion: 2` / `guid: <fresh 32-hex>`), never `git add -A` / `.`. One commit per slice. Never touch GG's four
  modified files (ProjectSettings/UnityConnectSettings.asset, ProjectSettings/EditorBuildSettings.asset,
  Assets/ProjectSettings/UIEffectProjectSettings.asset, Assets/Scenes/Arena.unity), nor HANDOFF.md. Trailer:
  `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- **Builders cannot message each other.** Hand-offs go in your report; the lead passes them on.
- **Movement exact:** at every commit that touches `Skygrind.Core`: golden 0 mismatches, 22 replays exact. Nobody edits `Core/Mover.cs`.
- **Sim parity:** before each commit that touches code, `fingerprint` and `fingerprint --off45` both EQUAL. Tonight adds no sim rows;
  S1's registry is the one sim-side change and must leave every existing tape bit for bit.
- **`--off45` is NIGHT44's game:** never edit a design.n27-n44 row, an older level file or an older generator's output in place (C1's
  one logged exception: n40-span's 7-word prompt text). Level edits only in your n45 level.
- **Numbers:** in `Design/unity-config.json` under `design.n45.<row>` with a `_why` (copy to `Assets/StreamingAssets/Design/unity-config.json`;
  the two stay byte-identical: `cmp`). Patch ONLY your own row, with a `.cjs` script written with the Write tool (never `node -e`, never
  a heredoc). Render numbers in `Assets/Scripts/Game/UnityTuning.<Name>45.cs`, each with a comment.
- **Helpers carry your builder id** (harness / reel helper names; N43Arg clashed between two builders).
- **Randomness** only from `Mulberry32`; no `System.Math` transcendentals in Core (JsMath). Whole-step timers (`StepsFor`).
- **No audio.** Event names only. **Look:** cyan = player-usable; red-orange = enemies and danger; yellow = parryable; magenta = RIVAL;
  pale violet-white = the boundary. UI text >= 16 px at 1080p. Fonts: the theme's.
- **Nothing lost (PRINCIPLES 1):** a change that touches a verb names its VERBS.md row in your commit and report.
- **No allocation per sim step. No Unity API in MonoBehaviour field initializers (L-035).**
- **Feel is seen (L-001):** write your PROOF first (the numbers and the frames before / after). You never set `enabled: true` yourself.
- **Reports:** under 300 words: done / SHORT, every commit sha, every check with its measured result, the frames the lead must take, your
  reel scenario, hand-offs. Last line: `N45-BUILDER <name>: round <r> · done <k>/<n> · commits <sha list>`.
- **Docs** with the Write / Edit tools only. Notes in `Research/2026-10-17-wave45/<your-name>.md`.

## 3. The shared surface (the lead's, in the NIGHT45 shared commit: Tools/night/n45-levels.cjs, dry-run by the forge)
- **A level-only night:** no N45Blocks, no tape change. `Core/N45Levels.cs`: Rows (foundry45, iceline45, kiln45), `ForceOff`, `Swap`,
  `BaseId` (an n45 id maps to its n43 level unconditionally, then the older chain continues: n45-kiln -> n43-kiln -> n41-kiln). The
  shared commit also routes the five places that compared the level id directly (level 3's boost lessons, the level 1 walk pilot, the
  kiln43 and rooms43 reels) through `N45Levels.BaseId`, so an n45 level keeps them. **Anything you add that checks a level id must ask
  `N45Levels.BaseId(cfg, id)` too.** Reel hooks are pre-placed: fill your partial `N45Yard` (A1), `N45Mix` (A2) or `N45Ring` (A3) in your
  own FeelReel.N45.<Name>.cs (phases a145 / a245 / a345); nobody else edits FeelReel.N27.cs.
- `design.n45`: foundry45 / iceline45 / kiln45 (levelFrom n43-* -> levelTo n45-*; the placeholders in design.maps.list are copies of the
  n43 files with the outer name changed) and the view row look45 (`N45View.RowOn(cfg, "look45")`, Game/N45View.cs). All off.
- Your level: write Tools/night/gen-n45-<level>.cjs from the n43 generator (copy its plan, rebuild only your room), producing
  Design/levels/n45-<level>.json + the StreamingAssets copy (byte-identical) and its plan; every other room bit for bit.
- Systems you may place by data (no new sim code tonight): lava (Lava30, both ways), pours, campaign.lights (fixtures), props with `why` /
  `job`, ride panels, noride, plates, crumbling perches (THE RING), spawn33 tells, afterLesson43 / fallReturn43 / waveNodes43, burrowers.
  A state change that needs new sim code goes into your card as "next night".
- Harness: `Tools/SimHarness/Harness.N45.<YourName>.cs` hooked with a module initializer, as NIGHT44's R2 did (Harness.N44.Proof.cs), with
  a `--level <id>` option: NIGHT43's commands are hard-wired to their n43 files (`n43level walk` reads foundry43's levelTo,
  Harness.N43.Level.cs:268; `n43kiln wyrm` reads n43-kiln.json, Harness.N43.Kiln.cs:43; the iceline walk pilot matches only n40-iceline,
  Walk40Iceline.cs:31): copy what you need into your own harness (prefixed helpers), never edit theirs.
- Last nights' tools: `n43level gates | cross | walk` (L1), `n43rooms` (E1), `n43kiln` / `n43wick wyrm` (K1 / B1), `n38 gates`, the
  walks, the reels foundry43, rooms43, kiln43, burrow43, wick37, look44.
