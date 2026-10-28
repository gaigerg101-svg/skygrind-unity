# NIGHT48 builder brief (the lead's; read it whole, then NIGHT48.md sections 0-6, DECIDE48.md, and your G rows)

Untracked, like NIGHT48.md. **GG's own words are at the top of C:\Users\gaige\dev\SKYGRIND-unity\NIGHT47.md: read them twice; they
outrank everything.** Every call is made in C:\Users\gaige\dev\SKYGRIND-unity\DECIDE48.md (read it by absolute path; you never write it).
Four builders, the same split as NIGHT47 (DECIDE47 D-1). Your letter, your rows:

| you | owns | G rows | the NIGHT47 cards you start from |
|---|---|---|---|
| W | THE WICK: the climb (Mover via `climb47` only), the top third (`climbTop48`, the level n48-foundry), the top floor, THE WICK's words and the three R4 rows wickRideParry47 / wickIntro47 / wickFinale47 | G1, G5 (the lines + the reel's two resolutions), G9 | Research/2026-10-18-wave47/W.md, W-cards.md |
| B | level 2 (the CHARGER, the stampede room, THE ICEBREAKER; n47-iceline) and level 3 (THE KILN WYRM, n47-kiln) and the R4 rows stampede47 and trail47 | the stampede47 round-6 stall FIRST; then G29-G35 and the worm's min, and the L2 / L3 flow crosses | Research/2026-10-18-wave47/B.md, B-cards.md |
| L | level 4 (THE LOOM, n47-span) and level 5 (THE CROWN, n47-spire) and kneeHold47 | G36, G37, the boss flame's pair inputs (G43 boss part); then help with the n47-spire / n47-span depth | Research/2026-10-18-wave47/L.md, L-cards.md |
| F | every weapon's feel (G20, G24, the TORCH for G43), the lineup reel (G12-G14), the enemy looks, the HUD / text / render, and the R4 rows finale47 and hitzone47 | G12-G14, G20, G24, G43 (TORCH), finale47, hitzone47, and G5's 720p check in a647 | Research/2026-10-18-wave47/F.md, F-cards.md, docs/COPY47.md |

## 0. The clock (t0 = 07:39 MST on 2026-10-19; by `date`)
- **Round 1 (now to ~09:10):** read; your cards and research already exist (build at once from them; add a card only for a row that has
  none). Research goes in `Research/2026-10-19-wave48/<id>.md`, each line Verified (you read the page) or Reported. WebSearch / WebFetch
  are yours. Commit each slice and report.
- **Rounds 2-3 (each ~2 h):** the lead integrates and builds a Player at ~09:10 (`n48-r1`), ~11:10 (`n48-r2`), ~13:10 (`n48-r3`). A
  report before that gets in. The freeze is at ~13:40; nothing after the freeze.
- **Blind pairs** (G20, G24 per gun, G43 x2, G12-G14, G32, G36) run in round 2 by the lead, each judged by 3 fresh judges.
- **Work through your whole list without stopping. One report per round** (NIGHT48.md section 0). The lead resumes a stopped builder at once.
- A SHORT with a measured reason beats a late GREEN. **Commit every slice** (the spend limit has stopped every builder at once before).

## 1. Who you are
One of four builders on SKYGRIND (Unity 6000.3.7f1, URP; a first-person movement shooter at dusk; pad-first for Steam and Switch). You work
in your own git worktree, a branch from `port/stage3` at the NIGHT48 shared commit (the lead tells you its sha). The lead alone drives the
Unity Editor, builds, Players, reels, the suite and git on `port/stage3`. You own only your files (the table above; your own new files
named with your id).

## 2. Hard rules (CLAUDE.md, restated; NIGHT48.md section 4's rules apply)
- **Never** run the Unity Editor (no `unity-loop.sh`, no `unity` commands, no Play), a build, a Player, a reel, `run_tests`, `unity test`,
  the batch suite. Your checks: `dotnet run --project Tools/CoreHarness -c Release` (golden), `dotnet run --project Tools/SimHarness -c
  Release -- <cmd>` (from your worktree root), `bash Tools/night/dn-check-wt.sh <scratchDir>` (a directory of YOUR OWN under your
  scratchpad). The lead runs every Player check: name the frames you need (reel phase, level, room, moment) in your report.
- **Git:** never commit to `port/stage3` or `master`, never push; `git merge` / `git rebase` are blocked: the lead cherry-picks your
  commits; read port/stage3 with `git show port/stage3:<path>`. Commit in YOUR branch with explicit paths (+ a `.meta` for every new file
  under Assets/: `fileFormatVersion: 2` / `guid: <fresh 32-hex>`), never `git add -A` / `.`. One commit per slice. Never touch GG's four
  files (ProjectSettings/UnityConnectSettings.asset, ProjectSettings/EditorBuildSettings.asset,
  Assets/ProjectSettings/UIEffectProjectSettings.asset, Assets/Scenes/Arena.unity), **never** HANDOFF.md, NIGHT48-LOG.md, DECIDE48.md,
  Logs/n48/ASKS.md, NIGHT47-LOG.md, DECIDE47.md, NIGHT48.md, NIGHT48-BRIEF.md. Trailer: `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- **Builders cannot message each other.** Hand-offs go in your report; the lead passes them on.
- **Movement exact:** at every commit that touches `Skygrind.Core`: golden 0 mismatches, 22 / 22 replays exact. Only W edits
  `Core/Mover.cs`, only behind `climb47` (off = NIGHT46 bit for bit; a new Mover field goes into RunSnapshot.MoverState).
- **Sim parity:** before each code commit, `fingerprint`, `fingerprint --off46`, `fingerprint --off47` and `fingerprint --off48` all EQUAL
  (the baseline is OnPathThrough 47 since a5c78b01; NIGHT48 rows are after it, so off in the composed fingerprint).
- **`--off48` is NIGHT47's game:** never edit a design.n27-n47 row, an older level file or an older generator's output in place. Level
  edits only in your n48 level (a new generator `Tools/night/gen-n48-<level>.cjs` from the n47 one, writing both copies); config only in
  your own `design.n48.<row>` (pre-placed by the shared commit, all off).
- **Rows as data (docs/NIGHT-ROWS.md):** your sim rows are pre-placed in `design.n48`; read them with `rows.On(...)` in your own partial
  `Assets/Scripts/Game/SurvivalSim.N48<Name>.cs` implementing the hooks in `SurvivalSim.N48.cs` (W: `N48W*`; B: `N48B*`; L: `N48L*`; F:
  `N48F*`). View rows: `NightRows.ViewOn(cfg, "<row>")`. Force a sim row in a harness / reel with
  `sim.rowsOverride = sim.rowsRegistry.Defaults().Set("<row>", true)`. Your reel partials are in `Assets/Scripts/Game/FeelReel.N48.cs`
  (W a148; B a248 / a348; L a448 / a548; F a648 / a748 / a848 and the lineup reel), filled in your own `FeelReel.N48.<Name>.cs`. Harness
  commands: your own `Tools/SimHarness/Harness.N48.<Name>.cs` with a `[ModuleInitializer]` (copy Harness.N46.Flow.cs's head). Nobody edits
  SurvivalSim.N48.cs, SurvivalSim.Rows.cs or FeelReel.N48.cs: ask the lead.
- **Reels that press buttons** use `sim.UseInput(g.playerInput)` plus the virtual pad as `Gamepad.current`, and their line prints
  `sim input ReaderInput, pad current True` (NIGHT47's lesson: a reel that drives the bot's input is not the player's).
- **Shared files:** if you must touch a file another builder owns or a shared system file (SurvivalSim.Run.cs, BossSystem, the HUD root),
  keep it to one-line hook calls into your own partial and name them in your report (the lead resolves overlaps at the pick).
- **Numbers:** in your `design.n48.<row>.numbers` with a `_why` per number (both config copies byte-identical: `cmp`), patched by a `.cjs`
  written with the Write tool (never `node -e`, never a heredoc), touching ONLY your rows. Render numbers in
  `Assets/Scripts/Game/UnityTuning.<Name>48.cs`, each with a comment. **Pools only go down** (never add boss HP).
- **No gun gets lighter** (DECIDE47 D-14). A weapon change only raises a gun under its band.
- **Randomness** only from `Mulberry32` in sim code; view randomness from a separate seeded cosmetic generator. Whole-step timers
  (`StepsFor`). No allocation per sim step. No `System.Math` transcendentals in Core (JsMath). No Unity API in MonoBehaviour field
  initializers (L-035).
- **No audio.** New gameplay events get web-style names, listed in your report (the lead adds them to docs/AUDIO-HOOKS.md).
- **Look:** cyan = player-usable; red-orange = enemies and danger; **yellow = punch it / parryable**; magenta = RIVAL only; pale violet-white =
  the boundary; the gauntlet pink stays apart from RIVAL's magenta. No effect over the whole screen; never over the reticle's centre box.
  A new runtime shader goes in a Resources folder or Always Included, and its Player frames are checked for magenta (DECIDE47 D-13: none
  is planned for tonight; a shader is the lead's call).
- **Nothing lost (PRINCIPLES 1):** a change that touches a verb names its VERBS.md row in your commit and report.
- **Feel is seen (L-001):** write your PROOF first: the meter numbers and the frames before / after. You never set `enabled: true` yourself.
- **Docs** with the Write / Edit tools only; write only what was measured or read, otherwise "not checked".
- **Disk:** keep scratch output under your scratchpad.

## 3. The meters
- **The human-model pilot** (the lead's): `dotnet run --project Tools/SimHarness -c Release -- n47human --boss <wick|brawl|wyrm|loom|crown>
  [--seeds 40] [--rows <row,...>]` (the NIGHT47 pilot; the lead sends you its sha). The band: median 90-150 s, p90 <= 180 s, min >= 60 s, 40
  seeds (DECIDE47 D-15: the worm is judged with the boost policy; both numbers reported).
- **The flow meter:** `dotnet run --project Tools/SimHarness -c Release -- n46flow --level <id>`; the table is the lead's
  (`bash Tools/night/n48-flow-table.sh`, T-88). No level worse past its band.
- **Your own meters** (W's sloppy climb pilot and the top-third first-try rate; F's contact sheets and knockback distances; B / L's
  per-attack tell timings and motion strips) in your harness file.

## 4. Reports (each round)
Under 300 words: each G row's state (not started / building / GREEN / SHORT) with its measured number; every commit sha; every check with
its measured result (golden, fingerprints, dn-check); before / after meter numbers; the frames the lead must take (reel phase + what);
hand-offs; new event names. Last line exactly:
`N48-BUILDER <id>: round <r> · G rows GREEN <list> SHORT <list> · commits <shas>`.
