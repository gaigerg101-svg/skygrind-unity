# NIGHT46 G0 forge: rows switch-on + n46-shared.cjs, dry-run (2026-10-17)

Worktree `agent-a25b29ecb00ba55f4`, branched from port/stage3 = u3-n45 (041c2cf0). No Unity Editor, no `unity` command, no build,
no Player. Times are wall clock on this laptop (not benchmarks).

**Where the files are:** this agent's isolation guard refused writes into the main checkout, so neither this file nor the script is
in `C:\Users\gaige\dev\SKYGRIND-unity` yet. Copy them from the worktree:
- `.claude/worktrees/agent-a25b29ecb00ba55f4/Tools/night/n46-shared.cjs` -> `Tools/night/n46-shared.cjs` (sha1 bc8f8354...)
- `.claude/worktrees/agent-a25b29ecb00ba55f4/Research/2026-10-17-wave46/FORGE.md` -> `Research/2026-10-17-wave46/FORGE.md`

`Tools/night/rows-switch-on.cjs` needed no fix: no anchor had moved. It is byte-identical in main and the worktree, and
`node Tools/night/rows-switch-on.cjs --dry` run against the main checkout (it writes nothing) printed `30 edit(s) planned ..., 19 file(s)`
with no MISS.

## 1. The switch-on (committed in the worktree branch: 25608beb "NIGHT46 rows switch-on (forge dry run)")

| Command | Result | Time |
| --- | --- | --- |
| `node Tools/night/rows-switch-on.cjs --dry` | `30 edit(s) planned (dry run, nothing written), 0 already in, 19 file(s)`, no MISS | 0.14 s |
| `node Tools/night/rows-switch-on.cjs` | `30 edit(s) applied, 0 already in, 19 file(s)` | < 1 s |
| same, second run | `0 edit(s) applied, 30 already in, 19 file(s)` | < 1 s |
| `bash Tools/night/dn-check-wt.sh <scratch>` | 4 projects, `dn-check total errors: 0` | 81 s |
| `dotnet run --project Tools/CoreHarness -c Release` | `39 vector sets (48094 records), 22 replays (22 exact); 0 failing, 0 mismatches` | 18 s |
| SimHarness `fingerprint` / `--off45` / `--off46` | `12 hashes ... EQUAL to the baseline` x3 | 17 s (build) + 3 s + 3 s |
| SimHarness `n45rows --expect-wired` | `wiring: WIRED GREEN (... tape format 27)`, probe / tape / stream / refused GREEN, `n45rows: GREEN (0 failing)` | 7 s |
| SimHarness `tape27` | `replay: EQUAL (6 of 6 checkpoints, 3600 steps)` | 4 s |
| `-p:Extra=../../Assets/Tests/EditMode/NightRowsTests.cs -- n45rows selftest` | `NightRowsTests: 11 of 11 pass` | 14 s |
| `-p:Extra=../../Assets/Tests/EditMode/Game/NightRowsTapeTests.cs -- n45rows selftest` | `NightRowsTapeTests: 3 of 3 pass` | 15 s |

## 2. n46-shared.cjs

`node Tools/night/n46-shared.cjs [rootDir]`. It checks that the switch-on is in (or refuses), checks every anchor exactly once and every
new path absent BEFORE writing anything, then writes 32 files and prints each one. A second run refuses
(`Error: design.n46 already exists (already run?)`, exit 1, nothing written).

**Files it writes (32):**
- config (2): `Design/unity-config.json`, `Assets/StreamingAssets/Design/unity-config.json` (a byte copy)
- placeholder levels (15): `Design/levels/n46-{foundry,iceline,kiln,span,spire}.json`, the same five under
  `Assets/StreamingAssets/Design/levels/`, and their five `.meta` files (DefaultImporter, the committed n45 metas' format)
- new code (6): `Assets/Scripts/Game/SurvivalSim.N46.cs`, `Assets/Scripts/Game/FeelReel.N46.cs`,
  `Assets/Tests/EditMode/N46SharedTests.cs`, each with its `.meta`
- one-line patches (9): `Assets/Tests/EditMode/NightRowsTests.cs`, `Assets/Scripts/Game/SurvivalSim.Loom37.cs`,
  `Assets/Scripts/Game/Bot/Walk38Span.cs`, `Assets/Scripts/Game/FeelReel.N45.Yard.cs`, `Assets/Scripts/Game/FeelReel.N45.Mix.cs`,
  `Assets/Scripts/Core/Campaign24.cs`, `Assets/Scripts/Game/SurvivalSim.Campaign24.cs`, `Assets/Scripts/Game/SurvivalSim.N36.cs`,
  `Assets/Scripts/Game/FeelReel.N27.cs`

This is more than the 3-file target in docs/NIGHT-ROWS.md section 2. The extra files are the placeholders, the hook sites the lead
asked for (so builders never edit Campaign24.cs or FeelReel.N27.cs) and the four fixes below.

**design.n46 (every row `enabled: false`, each with a `_why`, `enabled_why` "Off until ..."):**
- `_why`: "GG 2026-10-17: 'need to feel and flow better ... study real game deisgn and areana layouts' (NIGHT46.md)"
- level rows (`levelFrom` -> `levelTo`; the owner is named in the `_why`): foundry46 n45-foundry -> n46-foundry (A3), iceline46
  n45-iceline -> n46-iceline (A3), kiln46 n45-kiln -> n46-kiln (B1), span46 n40-span -> n46-span (A1; HOOK YARD, THE TIE-OFF,
  SPOOL HALL), spire46 n41-spire -> n46-spire (A2; THE NAVE, THE CHOIR). Five `design.maps.list` rows after n45-kiln's, each with
  its levelFrom row's label.
- sim rows (`"sim": true`, `"numbers": {}` for the builder): retry46, overlap46, teach46, reveal46 (F1); wyrm46, walkin46 (B1).
- view rows: shift46, flow46, feel46 (read with `NightRows.ViewOn(cfg, "<row>")`).

**Level-row ownership (for the cards):** kiln46 (n46-kiln) belongs to B1; foundry46 and iceline46 to A3; span46 to A1; spire46 to A2.

**The chain (the brief's question):** `NightRows.BaseId` already resolves chained rows. n46-foundry -> n45-foundry (the n46 row), and
then `N45Levels.BaseId` (which asks NightRows first, after the switch-on) -> n43-foundry. After that N43BaseId, N42BaseId and N41BaseId
carry on as before. n46-span -> n40-span; n46-spire -> n41-spire -> (N41BaseId, spire41 on) n40-spire. Nothing in NightRows needed a
fix. N46SharedTests.EveryN46Level_ReadsAsItsBase pins it on the real config.

**What the forge found and fixed in the script (one line each):**
1. `NightRowsTests.TheRealConfig_HasNoSimRows_AndNoProblems` pinned `NightRows.From(c).Count == 0` ("NIGHT45 adds no sim rows"). With
   the n46 sim rows it goes RED. I measured this by building the unpatched HEAD copy: `FAIL ... expected 0 but was 1`, 10 of 11. The
   patch changes the pin to "every row is a well-formed NIGHT46+ row and its default round-trips through the tape record", so no later
   night has to edit it. With the patch: 11 / 11.
2. Two direct level-id checks never ask BaseId, so they would not have seen n46-span:
   - `SurvivalSim.Loom37.cs` Loom37StartRun (`LevelDef.From(map.json).id` against loom40's levelTo n40-span: THE LOOM's rooms o1-o3)
   - `Bot/Walk38Span.cs` Level38Span (`== "n40-span"`: the level 4 walk pilot)

   Both now go through `N45Levels.BaseId`. With the rows off an n46 id is never loaded, so nothing changes.
3. Two NIGHT45 reels check the level id directly: `FeelReel.N45.Yard.cs` (a145, `lv.id == "n45-foundry"`) and `FeelReel.N45.Mix.cs`
   (a245, `id == "n45-iceline"`). Both now ask `NightRows.BaseId`.

   Not swept: `Bot/Walk40Iceline.cs` (`== "n40-iceline"`) and the harness files `Harness.N40.Loom.cs` / `Harness.N45.*.cs` (`--level`
   defaults). None of these is an n46 level's base.

**Hooks: the shape of each, and the sites read**
- `SurvivalSim.N46.cs`:
  - `Night46StartRun` resolves the ids once a run: idRetry46, idOverlap46, idTeach46, idReveal46 (F1) and idWyrm46, idWalkin46 (B1).
    It resets `walkIn46 = false`, then calls `N46FlowStartRun()` (F1) and `N46WyrmStartRun()` (B1).
  - `Night46Step(dt)` calls `N46FlowStep(dt)` and then `N46WyrmStep(dt)`.
  - `N46Campaign()` calls `N46FlowCampaign()` and `N46WyrmCampaign()`.
- Campaign24 is in Core, so a sim partial cannot be called from inside it. The hooks follow NIGHT33's pattern instead: null delegate
  fields, where null means the old rule exactly. Builders set them in `N46FlowCampaign` / `N46WyrmCampaign`. Those run once a run, from
  one new line in `SurvivalSim.Campaign24.cs` N24CampaignStep, right after the campaign is made. (The campaign does not exist yet at
  StartRun: it is made lazily on the first live step.)
  - **retry46**, at Retry()'s `wave = 0;` (Campaign24.cs:1136 at u3-n45; the death path itself is :693-705, unchanged):
    `wave = retryWave46 != null ? retryWave46(r, wave) : 0;`. Signature `Func<RoomDef, int, int>`: the room and the waves spawned at
    the death, returning the wave to restart at.
  - **overlap46**, at the wave release (:855-857): `early = ready && alive > 0 && (waveEarly33(...) || waveRelease46(r, wave, alive))`.
    Signature `Func<RoomDef, int, int, bool>`. An early wave skips gapLeft, as waveEarly33's does. SpawnWave (:910-928) is unchanged.
  - **teach46**, at the three rooms43 holds: the stand-in hold (:851), the afterLesson wait (:853) and the lesson cap (:889). Each one
    reads `Hold46(which, steps43)`, which returns `teachHold46 != null ? teachHold46(which, steps43) : steps43`. Signature
    `Func<int, int, int>`, with `Hold46StandIn = 0`, `Hold46AfterLesson = 1`, `Hold46LessonCap = 2`.
  - **walkin46**, at the boss enter (:672-676). That site already asks `world.BossWalkIn` = `SurvivalSim.Boss36WalkIn`, so the patch
    is `|| walkIn46` there (SurvivalSim.N36.cs:34). B1 sets `walkIn46` while `rows.On(idWalkin46)`. The retry's boss Respawn
    (Campaign24.cs:1129) is NOT hooked: a retry in a walked-in boss room still respawns at the boss spawn, as walkin36's does.
  - **reveal46** has no hook. The reveal / phase / die windows live in each boss module (Core/Enemies/Lantern36, Crown37, Stag37,
    Wick37, Brawl40 ...), not in Campaign24, and one call site cannot cover them. F1 needs the lead's ruling on where it may hook.
- Reels: `FeelReel.N46.cs` holds `PhaseN46 = { a146, a246, a346, a446, a546 }` -> `N46Span` (A1), `N46Spire` (A2), `N46Smooth` (A3),
  `N46Wyrm` (B1), `N46Feel` (V1). It is routed by two edits to `FeelReel.N27.cs` (IsPhaseN37 and the dispatch `else if`), as NIGHT45 did.
  Reel-side forcing of a sim row goes through `sim.rowsOverride`, set before the run starts:
  `sim.rowsOverride = sim.rowsRegistry.Defaults().Set("wyrm46", true);` (`Set` throws on an unknown name). Not checked in a reel.
- Harness: `Tools/SimHarness/SimHarness.csproj` compiles `*.cs` of its folder (`EnableDefaultCompileItems` false, an explicit
  `<Compile Include="*.cs" />`), so a `Harness.N46.<Name>.cs` is picked up with no Program.cs line. The `[ModuleInitializer]` pattern
  (Harness.N45.Rows.cs, Ring, Mix) still works: this forge's scratch runner used it (`n46forge`, compiled in via `-p:Extra`) and ran.
  Note the harness compiles `Game/SurvivalSim*.cs` and `Game/Bot/*.cs` but no FeelReel file.

**The dry runs** (each from a clean tree after the switch-on commit; between runs I reset only the 32 paths the script wrote:
`git checkout --` on the 11 modified ones, `rm` on the 21 new ones):

| Check | Run A | Run B |
| --- | --- | --- |
| `node Tools/night/n46-shared.cjs <root>` | `32 file(s)`, 0.32 s | `32 file(s)`, 0.30 s; 24 / 24 non-meta files byte-identical to run A (sha1) |
| second run | refused (design.n46 exists) | refused |
| `cmp` the two unity-config.json | equal | equal |
| duplicate keys (scratch `dupkeys.py`, json object_pairs_hook; a planted `{"x":1,"x":2}` was caught first) | 0 / 0 | 0 / 0 |
| dn-check-wt | 0 errors (39 s) | 0 errors |
| CoreHarness | 0 mismatches, 22 / 22 | 0 mismatches, 22 / 22 |
| fingerprint plain / --off45 / --off46 | EQUAL x3 | EQUAL x3 |
| `n45rows --expect-wired` | registry `6 sim row(s): n46.overlap46, n46.retry46, n46.reveal46, n46.teach46, n46.walkin46, n46.wyrm46`; checker 0 problems; WIRED GREEN; `n45rows: GREEN` | same |
| tape27 | EQUAL 6 / 6 | EQUAL 6 / 6 |
| NightRowsTests selftest (patched) | 11 / 11 | 11 / 11 |
| NightRowsTapeTests selftest | 3 / 3 | not re-run |
| N46SharedTests (headless, scratch runner `n46forge selftest`) | 4 / 4 | 4 / 4 |
| N46SharedTests with span46 planted ON (`enabled: true`, enabled_why still "Off until") | `FAIL TheRows_Exist_OffByDefault`, 3 / 4 (the plant was undone by the reset) | - |
| `campaign --selftest` (Campaign24Tests) | 12 / 12 (22 s) | 12 / 12 |
| `campaign --selftest25` (Lesson25Tests) | 10 / 10 | not re-run |
| `campaign --tape` | format 27, replay EQUAL 32 / 32, PASS (7.5 s) | PASS |
| `n39views maptime` (every listed map loads, composes, plays 600 steps, `map.name == id`) | 68 maps, 0 NO (34 s); n46-foundry / iceline / kiln / span / spire `yes` | 68 maps, 0 NO (33 s) |
| LevelValidate24 (levelgen's level report) on each placeholder and its source, scratch runner `n46forge validate` (68 s) | n46-foundry 0, n46-iceline 0, n46-kiln 0, n46-span 19, n46-spire 1: each equal to its source's (n45-* 0, n40-span 19, n41-spire 1; the same pit-recovery / route / lesson rows) | not re-run |

levelgen itself was not run on the placeholders: it takes a level plan, and a placeholder is a copy of a finished level with no plan.
LevelValidate24 is the report levelgen writes after a plan, so it was run directly on the files. The 19 errors on n40-span and the 1 on
n41-spire are already there in the committed levels; the placeholders did not add them.

The scratch tools (`dupkeys.py`, `N46ForgeRun.cs`) are in this agent's scratchpad, not in the repo.

## The G0 sequence for the lead (in the main checkout, Editor stopped for the compile step as usual)

1. Copy `Tools/night/n46-shared.cjs` (and this file) in from the worktree (see the top).
2. `node Tools/night/rows-switch-on.cjs --dry` -> `rows-switch-on: 30 edit(s) planned (dry run, nothing written), 0 already in, 19 file(s)` (0.14 s).
3. `node Tools/night/rows-switch-on.cjs` -> `30 edit(s) applied, 0 already in, 19 file(s)`.
4. `bash Tools/night/dn-check.sh` -> 0 errors (dn-check-wt took 40-84 s here).
5. `dotnet run --project Tools/CoreHarness -c Release` -> `0 failing, 0 mismatches`, `22 replays (22 exact)` (16-18 s).
6. SimHarness `-- fingerprint`, `-- fingerprint --off45`, `-- fingerprint --off46` -> `EQUAL to the baseline` each (about 17 s for the first, which builds, then 3 s).
7. `-- n45rows --expect-wired` -> `n45rows: GREEN (0 failing)` (7 s); `-- tape27` -> `replay: EQUAL (6 of 6 checkpoints, 3600 steps)` (4 s).
8. `-p:Extra=../../Assets/Tests/EditMode/NightRowsTests.cs -- n45rows selftest` -> 11 / 11; the same with `.../Game/NightRowsTapeTests.cs` -> 3 / 3 (14 s each).
9. The Editor compile, then the EditMode batches N3 / N4 / NightRows / Lesson25 (not checked here).
10. Commit the 19 files: "NIGHT46 rows switch-on".
11. `node Tools/night/n46-shared.cjs` -> 32 `wrote ...` lines, then `NIGHT46 shared commit done: 32 file(s).` (0.3 s).
12. Gates: dn-check 0; golden 0 / 22; fingerprint EQUAL plain / --off45 / --off46; `n45rows --expect-wired` lists the six n46 sim rows,
    0 problems, GREEN; `cmp Design/unity-config.json Assets/StreamingAssets/Design/unity-config.json` (silent); NightRowsTests 11 / 11;
    `campaign --selftest` 12 / 12; `campaign --tape` PASS; `n39views maptime` 68 maps, 0 NO (about 35 s).
13. The Editor compile (the new .cs files need their import first: L-067 / pick-new-file-stale-editor), then EditMode: N46SharedTests
    (4), NightRowsTests, Lesson25, Campaign24Tests, MapChoiceTests (5 new map cases, about 1 s each on CoreCLR, more on Mono) (not
    checked here).
14. Commit the 32 files plus `Tools/night/n46-shared.cjs`, explicit paths with their .meta: "NIGHT46 shared commit".

## Not checked

- The Unity Editor compile and every EditMode run, including N46SharedTests, which ran only headless on the harness's mini NUnit.
- A Player boot of any n46 level, or with -skygrindOff46 / -skygrindOff47.
- A walk or a fight with any n46 row ON. In particular: THE LOOM's o1-o3 and the Walk38Span pilot on n46-span after the BaseId patch;
  the reels a145 / a245 on n46 levels; the five new reel phases.
- The Campaign24 hooks with a delegate set (no builder code exists yet). With them null, the code paths equal NIGHT45's by reading.
  Measured: Campaign24Tests 12 / 12, Lesson25Tests 10 / 10, campaign --tape PASS, fingerprint EQUAL.
- The main checkout's git status (the guard blocked `git -C`). The 9 files the script patches, Design/unity-config.json and
  N45Levels.cs were compared byte for byte between main and the worktree: all equal, except SurvivalSim.N36.cs (main's has LF endings, the script normalises them; its anchor is
  there once) and N45Levels.cs (the worktree carries the switch-on).
- reveal46's hook sites (see above: the lead's ruling).
