# NIGHT47 builder brief (the lead's; read it whole, then NIGHT47.md sections 0-7 and 10, and your G rows)

Untracked, like NIGHT47.md. **GG's own words are at the top of C:\Users\gaige\dev\SKYGRIND-unity\NIGHT47.md: read them twice; they outrank
everything.** Every call is made in C:\Users\gaige\dev\SKYGRIND-unity\DECIDE47.md (read it by absolute path). Tonight there are FOUR
builders (DECIDE47 D-1), not eight: where NIGHT47.md names builder I / K / C / X / U / E, read your letter from this table.

| you | owns | G rows | NIGHT47.md's old letters you absorb |
|---|---|---|---|
| W | THE WICK, the climb (Mover via `climb47` only), the top floor, the WICK's arena (n47-foundry) and its words | G1-G9 | W |
| B | level 2 (the CHARGER, the stampede room, THE ICEBREAKER; n47-iceline) and level 3 (THE KILN WYRM, n47-kiln) | G25-G35 | I, K |
| L | level 4 (THE LOOM, its new arena; n47-span) and level 5 (THE CROWN, the level's sense, the boss's flame; n47-spire) | G36-G43 (boss parts) | L, C |
| F | every weapon's feel, the LT blast, the punch aura (player and enemy side), the zoom, the grapple line, the TORCH, the HUD, the text system, every string, enemy looks (visors, shield, small robots), the door bug, the lava light | G10-G24, G43 (TORCH) | X, U, E |

## 0. The clock (t0 = 22:46 on 2026-10-18; by `date`)
- **Round 0 (now to ~23:55):** read; research (your notes in `Research/2026-10-18-wave47/<id>.md`, each line marked Verified when you read
  the page, Reported otherwise; WebSearch / WebFetch are yours); measure your baselines; write your CARDS (section 5 of NIGHT47.md) in
  `Research/2026-10-18-wave47/<id>-cards.md` (the lead assembles docs/BOSSES47.md / docs/ROOMS.md / docs/COPY47.md from them: never edit
  those shared docs yourself tonight). Commit the notes + cards, report, stop. The lead answers PASS or names the line to fix.
- **Rounds 1-3 (each ~80 min):** build. The lead integrates and builds a Player at ~01:35, ~02:55, ~04:15; a report before that gets in.
- **Round 4 (~04:15-05:25):** second attempts on SHORT rows, then the beyond round (G9 / G44 / G45: up to 2 researched ideas of your own,
  each behind its own row, each with frames).
- A SHORT with a measured reason beats a late GREEN. **Commit every slice** (the spend limit has stopped every builder at once before).

## 1. Who you are
One of four builders on SKYGRIND (Unity 6000.3.7f1, URP; a first-person movement shooter at dusk; pad-first for Steam and Switch). You work
in your own git worktree (a branch from `port/stage3` at the NIGHT47 shared commit). The lead alone drives the Unity Editor, builds,
Players, reels, the suite and git on `port/stage3`. You own only your files (the table above; your own new files named with your id).

## 2. Hard rules (CLAUDE.md, restated; NIGHT47.md section 1's changes apply)
- **Never** run the Unity Editor (no `unity-loop.sh`, no `unity` commands, no Play), a build, a Player, a reel, `run_tests`, `unity test`.
  Your checks: `dotnet run --project Tools/CoreHarness -c Release` (golden), `dotnet run --project Tools/SimHarness -c Release -- <cmd>`
  (from your worktree root), `bash Tools/night/dn-check-wt.sh <scratchDir>` (a directory of YOUR OWN under your scratchpad). The lead runs
  every Player check: name the frames you need (reel phase, level, room, moment) in your report.
- **Git:** never commit to `port/stage3` or `master`, never push; `git merge` / `git rebase` are blocked: the lead cherry-picks your commits;
  read port/stage3 with `git show port/stage3:<path>`. Never a "sync" commit re-adding the lead's files. Commit in YOUR branch with explicit
  paths (+ a `.meta` for every new file under Assets/: `fileFormatVersion: 2` / `guid: <fresh 32-hex>`), never `git add -A` / `.`. One
  commit per slice. Never touch GG's four files (ProjectSettings/UnityConnectSettings.asset, ProjectSettings/EditorBuildSettings.asset,
  Assets/ProjectSettings/UIEffectProjectSettings.asset, Assets/Scenes/Arena.unity), HANDOFF.md, NIGHT47-LOG.md, DECIDE47.md.
  Trailer: `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- **Builders cannot message each other.** Hand-offs go in your report; the lead passes them on.
- **Movement exact:** at every commit that touches `Skygrind.Core`: golden 0 mismatches, 22 / 22 replays exact. Only W edits
  `Core/Mover.cs`, only behind `climb47` (off = NIGHT46 bit for bit; copy how ride40 / wallgrip36 kept the vectors exact; a new Mover field
  goes into RunSnapshot.MoverState).
- **Sim parity:** before each code commit, `fingerprint`, `fingerprint --off46` and `fingerprint --off47` all EQUAL.
- **`--off47` is NIGHT46's game:** never edit a design.n27-n46 row, an older level file or an older generator's output in place. Level edits
  only in your n47 level (write a new generator `Tools/night/gen-n47-<level>.cjs` from the n46 one, writing both copies); config only in your
  own `design.n47.<row>` (pre-placed, all off).
- **Rows as data (docs/NIGHT-ROWS.md):** your sim rows are pre-placed in `design.n47`; read them with `rows.On(idXxx47)` in your own partial
  `Assets/Scripts/Game/SurvivalSim.N47.<Name>.cs` implementing the hooks in `SurvivalSim.N47.cs` (W: `N47WStartRun / N47WStep /
  N47WCampaign`; B: `N47B*`; L: `N47L*`; F: `N47F*`; the ids `idWick47 ... idOverheal47` are resolved there). View rows:
  `NightRows.ViewOn(cfg, "<row>")`. Force a sim row in a harness / reel with `sim.rowsOverride = sim.rowsRegistry.Defaults().Set("<row>", true)`.
  Your reel partials are in `Assets/Scripts/Game/FeelReel.N47.cs` (W a147; B a247 / a347; L a447 / a547; F a647 / a747 / a847), filled in
  your own `FeelReel.N47.<Name>.cs`. Harness commands: your own `Tools/SimHarness/Harness.N47.<Name>.cs` with a `[ModuleInitializer]`
  (copy Harness.N46.Flow.cs's head). Nobody edits SurvivalSim.N47.cs, SurvivalSim.Rows.cs, FeelReel.N27.cs or FeelReel.N47.cs: ask the lead.
- **Shared files:** if you must touch a file another builder owns or a shared system file (SurvivalSim.Run.cs, BossSystem, the HUD root),
  keep it to one-line hook calls into your own partial and name them in your report (the lead resolves overlaps at the pick).
- **Numbers:** in your `design.n47.<row>.numbers` with a `_why` per number (both config copies byte-identical: `cmp`), patched by a `.cjs`
  written with the Write tool (never `node -e`, never a heredoc), touching ONLY your rows. Render numbers in
  `Assets/Scripts/Game/UnityTuning.<Name>47.cs`, each with a comment. **Pools only go down** (never add boss HP).
- **Randomness** only from `Mulberry32` in sim code; view randomness from a separate seeded cosmetic generator. Whole-step timers
  (`StepsFor`). No allocation per sim step. No `System.Math` transcendentals in Core (JsMath). No Unity API in MonoBehaviour field
  initializers (L-035).
- **No audio.** New gameplay events get web-style names, listed in your report (the lead adds them to docs/AUDIO-HOOKS.md).
- **Look:** cyan = player-usable; red-orange = enemies and danger; **yellow = punch it / parryable**; magenta = RIVAL only; pale violet-white =
  the boundary; the gauntlet pink stays apart from RIVAL's magenta. No effect over the whole screen; never over the reticle's centre box.
- **Nothing lost (PRINCIPLES 1):** a change that touches a verb names its VERBS.md row in your commit and report.
- **Feel is seen (L-001):** write your PROOF first: the meter numbers and the frames before / after. You never set `enabled: true` yourself.
- **Docs** with the Write / Edit tools only; write only what was measured or read, otherwise "not checked".
- **Disk:** keep scratch output under your scratchpad.

## 3. The meters
- **The human-model pilot** (the lead's, DECIDE47 D-5): `dotnet run --project Tools/SimHarness -c Release -- n47human --boss <wick|brawl|wyrm|loom|crown> [--seeds 40] [--rows <row,...>]`,
  landing on port/stage3 around 23:45; the lead sends you the commit sha to cherry-pick into your branch (`git cherry-pick <sha>` is allowed
  for the lead's shared commits only). It reports kill time (median / p90 / min), damage taken, deaths, window hits, verbs used per minute.
  The band: median 90-150 s, p90 <= 180 s, min >= 60 s, 40 seeds. If you need a boss-specific behaviour from it (which verb your boss card
  stars), add it in YOUR harness file as a policy the pilot calls (the pilot exposes a hook; see its header) and say so in the report.
- **The flow meter:** `dotnet run --project Tools/SimHarness -c Release -- n46flow --level <id>` and `bash Tools/night/n46-flow-table.sh <dir>`;
  no level worse past its band (Research/2026-10-17-wave46/F1.md).
- **Your own meters** (W's sloppy climb pilot; F's contact sheets and knockback distance; B / L's per-attack tell timings) in your harness file.

## 4. Reports (each round)
Under 300 words: each G row's state (not started / building / GREEN / SHORT) with its measured number; every commit sha; every check with
its measured result (golden, fingerprints, dn-check); before / after meter numbers; the frames the lead must take (reel phase + what);
hand-offs; new event names. Last line exactly:
`N47-BUILDER <id>: round <r> · G rows GREEN <list> SHORT <list> · commits <shas>`.
