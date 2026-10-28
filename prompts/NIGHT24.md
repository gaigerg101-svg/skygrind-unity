# NIGHT24.md: SKYGRIND in Unity: THE LINEAR TURN (a 6.5 h night, hard cap 7 h: remove, fix, build the campaign's spine and a worm boss)

<library_header>
id: skygrind-n24 | version: 1 | date: 2026-10-08 | model / effort: claude-opus-5-5 / high
raw request: GG, 2026-10-08, verbatim in Research/2026-10-08-wave24/GG-REQUEST.md (read it first; its last table says how each garbled phrase is read)
planning set: Research/2026-10-08-wave24/ (RESEARCH.md, IDEAS-POOL.md, DESIGN24-bosses.md, DECIDE24.md, NIGHT24-FEATURES.seed.json, R1-R4 = the four web passes) | forge: wave-forge
base: branch port/stage3, commit c431658 (NIGHT23's close); NIGHT23 left no `u3-n22` tag (suite unfinished, smoke pad steps red)
outcome: (after the run)
</library_header>

<role_and_goal>
You are the lead engineer on SKYGRIND, a first-person movement shooter at dusk (Unity 6000.3.7f1, URP; a deterministic 120 Hz C# sim with its own collider and an input
tape that replays runs bit for bit), in `C:\Users\gaige\dev\SKYGRIND-unity`, branch `port/stage3`. GG owns the game; you build for GG's taste, for Steam.

**GG turned the game.** It was a zombies-style round survival in one arena. GG now wants a **linear boomer shooter in the ULTRAKILL mould that focuses on boss fights**: a
pistol first, a shotgun earned, a few arenas that LIMIT your weapons or movement, closed rooms with better lighting, some platforming and wall play, and bosses (a crazy fast
worm; later a highway chase and a V2-style difficulty spike). SURVIVAL and THE GRIND stay as modes; the DAILY, the WEEKLY and the INSTANT REPLAY go. GG also asked for a charged
fist, a cooldown arc under the reticle, a sparing and BIG pick screen, Y to switch weapons, a slower shotgun, and an answer to the lag when switching screens.
Art style and polish come later (GG's words). This night builds the SPINE: the pipeline that turns a plan file into a playable layer, and the first boss.

**Run length: a 6.5 h box from your first `date` (code freeze at 5:00, verification 5:00-6:15, docs and the close table 6:15-6:45). Hard stop at 7:00.** The close table prints at 6:15 or later, never after 7:00. There is no early stop.
Nights 22 and 23 taught three things that rule this one: (1) a row passes only on a Player line from tonight, with its RED (the same scenario with the row off); (2) the first
Player must START: run Editor Play and read 0 console errors, then read the Player log's first exception, before trusting any scenario (NIGHT22's last Player threw an NRE at
startup and nothing ran); (3) the machine is shared (60-80 python jobs, a reel died on `fork: Resource temporarily unavailable`): one heavy job at a time.

Order: start, the shared commit, builders, the baseline suite and the hitch tour, four compile rounds (C1 2:15, C2 3:20, C3 4:20, C4 4:50), the freeze at 5:00, the verification, the close.
**If time runs short, cut in this order** (each a logged SHORT with its numbers): the lighting kit (art and polish are "later", GG's words: keep only room accent colours and the guide light), the second reviewer and the bench, the second LIMIT flag, the reticle's dash pips, the wyrm's tail-shed, then the traversal links' second kit. Never cut: the removals, the campaign spine, the worm, the fist, the pad, the shotgun, the BIG PICK, the proof.
</role_and_goal>

<ggs_words>
The ten sentences that decide ties (full text in GG-REQUEST.md; L-008: map loose words before building, defaults are in DECIDE24.md):
- "the first weapon ... charge it up by hitting enemies over time ... flick air kind of movement boost ... balanced unless fully charged ... only how many enemies you hit or kill."
- "Remove the replay feature." "Remove the daily and weekly day mode."
- "the pick a mod thing is a good idea but ... sparing ... every five or 10 rounds ... a big buck ... a free mystery box spin or 1000 points ... something simple."
- "Don't disagree please just do this ... the Y ... should also be able to switch weapons." "the shotgun is not balanced it just fires a little too fast."
- "under your reticle there should be a little bar or like a curved bar ... how much of your cool down ... it should not be in the way."
- "let's keep these game modes ... but I'd rather have it be a linear ... boomer shooter ... focus on the boss fights similar to ULTRAKILL ... start out with a pistol ... earn a shotgun ... limited weapons ... lose movement."
- "a crazy worm boss ... slithers really easy though and is really fast" (twice). "an endless highway ... you're chasing the boss ... you can't just move right to left 'cause that's boring."
- "feel like V2 where it is a difficulty spike." "better lighting ... closed rooms type movement ... a little bit of platforming or wall riding."
- "we need to study that 3D design, 3D arena building ... three bosses, I've already made two of them and you can make one more ... split this up into different prompts."
- "art style and polish will come later ... think of also just other ways to iterate" and "think big picture, think smarter."
</ggs_words>

<facts_already_measured>
Read 2026-10-08 08:00-08:40. Re-check each in the first 15 minutes; do not trust blindly.
- **NIGHT23's close** (`NIGHT23-LOG.md`, `NIGHT23-REPORT.md`): 8 of 9 NIGHT22 contracts GREEN on a Player (arcline, stamp, grind (reel), replay, challenge, ghost-daily, dusk, haptics); DEADLINE
  SHORT (round 15 = 59-61 %); the full suite never finished in 39 minutes (the Editor sat busy; it may have lost focus: keep it focused); the Player smoke fails `pad only` and
  `grind22` on TITLE (A on PLAY starts no run) twice, every other step passes: suspect NIGHT22's TITLE changes (the CODE item, the ghost gate) or the smoke's pad assumptions; builds `Builds/n23-3`.
- **What the code already gives (grepped / read):** `Zones.Open(id, free)` + `GateQuads()` = a door (a zone gate opens free on clear; `SurvivalSim.cs` blocks the nav behind it);
  THE GRIND (`Core/Endless21.cs` 264 lines, `Game/SurvivalSim.Grind21.cs` 222) = the template for a mode that replaces the round source (`grindOn`, banner text, records, the tape's `mode` string);
  `IRoundSpawner` (`StartWave`, `Step`, `Done`, `Clear`) in `SurvivalRun.cs`; maps are JSON (`Design/maps/<id>.json`: `collision` boxes / barriers / wedges, `spawns`, `pads`, `grappleNodes`, `zoneParts`, `navGrid`,
  `spots`) made by a pure plan converter (`DrydockPlan` in `Assets/Editor/Maps/Build_drydock.cs`; its plan file `Design/maps/drydock.plan.json` ALREADY has `zones` (rects with ids), `doorways` (between two zones, a width, a face), `boxes`, `ramps`, `pads`, `loops`, `shortcuts`, `grapple` and `spots`: so a LEVEL PLAN is that format with zones as rooms and doorways as doors, plus a new `rooms` block for encounters, limits and awards: extend the converter, never fork it; headless: `dotnet run --project Tools/SimHarness -c Release -- mapgen <plan> <out>`) and checked by
  `MapValidate`; bosses: `Core/Enemies/Bulwark.cs` (847 lines, built, `design.adds.bulwark` off), `BossKit.cs`, `BossMath.cs`; the web (frozen, read only) has MONOLITH (`boss.js` 695), HIVE (`hive.js` 419), ORBITER (`orbiter.js` 687) and a fifth;
  enemies: husk, swarmer, spitter, drone, charger, lancer, warden; weapons in `PortExport/config.json` (`weapons.shotgun.fireInterval` 0.9 s, `pumpDelay` 0.45, 8 pellets x 8; revolver 0.4 s x 25; the gauntlet is `kind: melee`).
- **The pad (v3, `Core/Gamepad.cs`):** Y = SPECIAL + ARCLINE, RB = scroll weapons (the v2 default had Y = NEXT WEAPON; `SwapRbY`); a wheel opens on a hold (`design.pad.n20.wheelHold`).
- **PICK A MOD** (`Core/Mods.cs`, config `mods`): 3 offers, 1 reroll at 1500, `draftRounds` [2, 4, 5, 7, 9, 10, 12, ...], small percentage cards (screenshot: LANCE +25 %, SKYDANCER +20 %, VARIETY +15 points).
  The round-2 draft is on the composed FINGERPRINT's path: changing it forces ONE re-record at the freeze (rule: its own commit).
- **GG's screenshots:** gameplay at 104-172 FPS (so the lag is a switch hitch); the PAUSED menu and the Stats overlay in the Editor's Game view at 0.61x scale; the Stats overlay covered the left PICK card in one shot.
- **Research (RESEARCH.md S1-S7, URLs in R1-R4):** ULTRAKILL gives the shotgun by a boss drop and one new weapon per layer; shotgun intervals: Quake 0.5, Dusk 0.9, ULTRAKILL 1.25 / 0.9 (pump charge); rewards every 5 rounds as world objects, a box spin first; the reticle arc is
  about 140 x 5 px, 26-48 px under the crosshair; a hit-fed 3-tier fist; the likely switch-hitch causes (SetActive on canvases, shader first use, TMP glyphs, sync loads); the worm spec and the chase spec in DESIGN24-bosses.md.
- **The machine:** other sessions run 58-78 python processes; CPU read 43-100 %. The Editor must be started once after a crash (`Start-Process` on Unity.exe). Unity's `run_tests` and `test_status` calls time out at 30 s while the Editor is busy: poll `Tools/night/wait-tests.sh`.
</facts_already_measured>

<scope>
- **In:** the 13 contracts of `NIGHT24-FEATURES.seed.json`; the P0 debts of NIGHT23 (the smoke pad steps, the suite, the `u3-n22` tag); the seeded leftovers in order; NIGHT25's designs written down (done in DESIGN24-bosses.md: update it with what you learn).
- **Out:** audio (rule 7); art style and polish (GG: later); new Mover verbs (wall-run, rail grind: rule 3, movement stays exact: use the existing wall jump / kick / slide / grapple / pads); the highway chase, the V2 rival, the MONOLITH / HIVE / ORBITER ports (NIGHT25: designed, not built; a 400 m highway stub only if every contract is DONE); the web repo; balance beyond the shotgun and the fist. A new idea goes to `docs/memory/IDEAS.md`, never into code.
</scope>

<lean_rules>
- **Building share:** at least 65 % of agent-minutes go to committed code, scenarios, level plans and fixes; measuring and reviewing the rest. Log the split in the close.
- **Proof-first, in a size that fits:** each contract row has its scenario (`reel`) or harness command written and seen RED (the same scenario with the row off, or before the feature) before it is called built; a scenario prints its PRECONDITIONS (`FeelReel.Preconditions` and `SafeSpot`: spawn spot, distance to the nearest pad and buy station, what is owned). A row passes only on a Player or harness line from tonight.
- **The startup gate (NIGHT22's lesson):** after every integration that touches UI or views: Editor Play, `bash Tools/unity-loop.sh errors` shows `consoleErrors` 0, THEN the first reel's Player log has no exception. Builders cannot compile Unity-side code: they write it carefully, the lead compiles and returns errors (it compiled first try in NIGHT22 when they copied neighbours).
- **Golden at every Core commit (CLAUDE.md rule 3):** a builder runs the harness `fingerprint` and the SimHarness golden / trajectory commands before each Core commit and says so in the message; the lead runs the Editor tests `Golden` and `Trajectory` at every integration. `Mover.cs` and `Core/Movement` are never edited.
- **Config files:** after the shared commit each builder edits ONLY its own `design.n24.<row>` block in `Design/unity-config.json`; the LEAD copies it to `Assets/StreamingAssets/Design/unity-config.json` at every integration (`cmp` must print identical). Nobody else writes the Player copy.
- **Testing:** one focused test per rule, seen red once; the full suite twice (the baseline at 0:50 and the close); re-run only failing files; a red that passes alone is re-run twice before it is called a flake. Tests verify the work: never edit one to pass it; a changed premise is said in the commit (the removals change premises: list them).
- **Wired, not just defined:** every config key is read by code, every event is in `PORT-STATUS.md`, every switch has an off-test (`--off24` / `-skygrindOff24` = NIGHT22's game on the sim path), both config copies byte-identical, tape format 8 round-trips the new run state.
- **Numbers:** web numbers from `PortExport/config.json`; Unity-only numbers in `design.n24.<row>` (both copies, each with `_why`) or `UnityTuning.*.cs`; nothing typed in code. Randomness: Mulberry32 only. Looks: shipped materials and shaders only; cyan = player-usable, red-orange = enemies and danger only, magenta = RIVAL only; text 18 px or more (M10); read frames from the Player reel, never the Editor's.
- **Caps (a cap reached = SHORT with the count):** Player builds 10; reel invocations 40 (one invocation may run several rows: `-skygrindReelOnly a,b,c`; keyboard and pad count separately); suite runs 2; builders 4 alive and 5 in total (a resumed builder is the same builder; SendMessage resumes keep their context); veto reviewers 2 (at C4, 25 minutes each); no persona triage; farm batches 2 (alone); bench only if the CPU is under 30 % before each of 3 runs. Read-only research agents and the 2 reviewers do not count toward the builder caps; a reviewer may read files while a suite runs (it never touches the Editor). `export NIGHT=n24` at 0:00.
- **Heavy jobs one at a time, always:** a build, a reel, the metrics, the smoke, the suite, a farm. Check `tasklist` for stray `SKYGRIND.exe` first and close them by PID. An edit under `Assets/` while the suite runs recompiles the Editor and kills the run: the lead edits only outside `Assets/` (docs, `Tools/`, the log) during a suite. Keep the Editor focused (`editor_focus`; `set_autotick`).
- **Commits:** every finished slice, explicit paths plus `.meta` files; builders at least every 30 minutes; never `ProjectSettings/UnityConnectSettings.asset`; restore the TMP fallback font assets, `Assets/Resources/Fonts/*.asset` and `Mobile_RPAsset.asset` after Play or a build (`git checkout --`); after every Player build `git diff --stat ProjectSettings Assets/Settings` and commit only what you understand (CLAUDE.md rule 1); never master, never push, never merge (CLAUDE.md rule 1). Night prompts stay untracked; logs are tracked.
- **Time boxes:** `date` at every phase boundary into `NIGHT24-LOG.md` (Write / Edit only). A phase 25 minutes over its box is cut; the rest goes to the leftovers.
</lean_rules>

<context>
**Read first:** `CLAUDE.md` (imports `docs/memory/CORE.md`), this file, `Research/2026-10-08-wave24/GG-REQUEST.md`, `DECIDE24.md`, `RESEARCH.md`, `DESIGN24-bosses.md`, the NIGHT23 block of `HANDOFF.md`, `NIGHT23-LOG.md`.
**GG's answers come first:** act on any filled Answer in DECIDE24.md or any word from GG before U3-38.4, and log how. A blank Answer means the default stands.
**Key files:** `Core/MetaEvents.cs` (`RunSetup`), `Core/SurvivalRun.cs`, `Core/Zones.cs`, `Core/Endless21.cs` + `Game/SurvivalSim.Grind21.cs` (the mode template), `Core/Mods.cs`, `Core/Gamepad.cs`, `Core/WeaponMath.cs`, `Core/Enemies/Bulwark.cs` + `BossKit.cs`, `Game/UiMeta.cs` (the MODE row), `Game/UiMenus.cs`, `Game/Ui.cs` + `Ui.N22.cs`, `Game/FeelReel.N22*.cs` (scenario pattern), `Assets/Editor/Maps/Build_drydock.cs` (the plan converter), `Tools/SimHarness/Harness.N22*.cs` (harness pattern), `Tools/feel-metrics.py`, `Tools/night/*.sh`.
**Git:** stay on `port/stage3`; tag `u3-n22` on the baseline commit only if its suite and smoke are green; tag `u3-n24` once on the close commit.
**The rules that bite:** CLAUDE.md 1 (git), 3 (golden exact; `Mover.cs` untouched: the fist's flick is an impulse through the existing API), 4 (the C# collider, no PhysX), 5 (numbers), 6 (Mulberry32), 7 (no audio; boss events keep the web's names), 8 (roles, 18 px), 9 (docs by Write / Edit), 10 (close every Player by its PID; never kill the Editor).
</context>

<autonomy>
- Never ask GG. Decide, log the choice with the rejected alternative, move on. What only GG decides is in DECIDE24.md with its default.
- If one approach fails twice, switch to the next-best and log both. A row that still fails after two fix rounds is SHORT with its numbers.
- **The Editor gate (L-025), first 10 minutes:** `date`, `bash Tools/night/editor-ready.sh` (polls 5 minutes), a "Recovering Scene Backups" dialog: `powershell -File Tools/night/dismiss-dialog.ps1` after 5 minutes (it acts only on that exact title). No Unity process and port 7800 closed: start it once with `Start-Process` on `C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.exe` with `-projectPath C:\Users\gaige\dev\SKYGRIND-unity`. Unreachable for 15 minutes: work headless (SimHarness) and mark Player rows SHORT with the date.
- **After a crash or interruption:** `git status`, `git log`, the last lines of `NIGHT24-LOG.md` and `Logs/n24-*.txt`; redo only what is half-done; commit before any risky step.
- On a usage or spend limit: commit, log the gap with `date`, keep working alone, retry agents at most every 30 minutes (L-003).
- The stops you want: the close table, or the hard cap. Stops you do not want: a summary announcing the next step with no tool call; an offer to continue; a list of decisions that block nothing. (The kickoff appends `PROMPT-STOPS.md`.)
</autonomy>

<builders>
Four builders in git worktrees by file ownership (`isolation: "worktree"`); **only the lead touches the Editor, builds, reels, the integration and the shared files.** Builders write Core, harness AND Unity-side files (views, scenarios) in their worktree and cannot compile Unity-side code (say what is uncompiled in the report); the lead integrates by `Tools/night/integrate.sh` (update its ids; record hand-resolved picks in `Logs/night-picked.txt`), compiles once per round, returns errors by SendMessage. Each brief holds: its contracts from `NIGHT24-FEATURES.json` verbatim, its files, "scenario first, seen red; commit every 30 minutes; the off-test first", the relevant RESEARCH.md sections, the startup-gate rule, and "a report under 300 words: commits, what a player now does, the contract's printed line, what is not compiled".

| Builder | Owns | Queue (in order) |
| --- | --- | --- |
| **R** (removals and pad; **an exclusive edit window for the removals**) | the removals touch about 35 files (`grep -l 'Daily\|Weekly20\|GhostDaily22\|Replay2[12]'`): R may edit ANY file, lead-owned ones included, but ONLY to delete DAILY / WEEKLY / ghost-daily / replay code, references, config rows, smoke steps (`SliceSmoke.cs` has a DAILY step) and test premises, until the lead announces C0 (target 1:20: R's removal commits integrated and the others merge `port/stage3`); L, F and B do not touch `UiMeta.cs`, `UiMenus.cs`, `UiResults.cs`, `Profile.cs`, `MetaEvents.cs`, `RunLog.cs`, `SliceSmoke.cs` or any `SurvivalSim*` / `SurvivalGame*` file before C0; then `Core/Daily.cs`, `Weekly20.cs`, `GhostDaily22.cs`, `Game/Route/GhostDaily22.cs`, `Replay21Ring.cs`, `Game/Replay22*.cs`, `Ui.N22.Replay.cs`, `Core/Gamepad.cs` layout v4, `Core/WeaponMath.cs` + the shotgun config rows, `FeelReel.N24.Remove.cs`, `.PadY.cs`, `.Shotgun.cs`, `Harness.N24.Shotgun.cs`, their tests | remove-replay, remove-daily-weekly (update every test premise, list them), pad-y-weapons, shotgun-rate (measure BEFORE, then change), then leftover 1. Its scenario ids: `remove`, `title`, `padweapons`, `shotgun` |
| **F** (fist, arc, rewards) | `Core/Fist24*.cs`, `Game/UiReticleArc24.cs`, `Core/Mods.cs` + `Game/UiDraft*.cs` + `Core/BigPick24.cs`, `FeelReel.N24.Fist.cs`, `.Reticle.cs`, `.BigPick.cs`, `Harness.N24.Fist.cs`, their tests | charge-fist (the harness first: idle 60 s = +0; the kill-run number), reticle-arc (the ARCLINE chip moves in), big-pick, then leftover 2 |
| **L** (levels and the campaign) | `Core/Campaign24*.cs`, `Core/Level24*.cs`, `Game/SurvivalSim.Campaign24.cs`, `Design/levels/*`, `Assets/Editor/Levels/*`, `Harness.N24.Level.cs`, `FeelReel.N24.Room.cs`, `.Limit.cs`, `.Layer.cs`, `.Lit.cs`, `Game/Lighting24*.cs`, the MODE row in `Game/UiMeta.cs`, their tests | **first 30 minutes: write `Design/levels/LEVEL-FORMAT.md` and spike the pipeline (a 2-room plan to collision JSON to a bot walking it headless); tell the lead the result by SendMessage**, then campaign-core (**first deliverable: `Zones` has `Open` but NO re-lock: add `Zones.Lock(id)` or a start-locked door set, in Core, with Save / Load and tests**), weapons-limits, layer-1, lighting-kit (minimal); it also writes `docs/LEVEL-DESIGN-NOTES.md` at the close (what the rooms taught: sizes, bot par times, clear rates, balance numbers: GG asked to "study the game, how we balance it, what's shown") |
| **B** (the worm) | **the body pieces are not solid to the player** (`BoxLayer` is axis-aligned; no collider changes): shots test a ray against each piece's oriented box in Wyrm code, the player takes contact damage and a knockback from a piece's box test; `Core/Enemies/Wyrm*.cs`, `Game/WyrmView.cs`, `Design/bosses/wyrm.json`, `Design/maps/wyrm-arena.*`, `FeelReel.N24.Wyrm.cs`, `Harness.N24.Wyrm.cs`, their tests | wyrm (standalone first: its own arena map from boxes, a headless bot fight; L integrates it as layer 1's boss room: THE WORM IS LAYER 1'S BOSS; the BULWARK is layer 2's, NIGHT25), then tail-shed phase if time |

**The lead alone** touches: `SurvivalSim.cs`, `SurvivalSim.Run.cs`, `SurvivalGame*.cs`, `Ui.N24.cs`, `FeelReel.N24.cs` (the phase list and any shared helper), `Tools/SimHarness/Program.cs`, `Harness.N24.cs`, `RunTape.cs`, `FingerprintBaseline*.cs`, the config merge, `Tools/feel-metrics.py` and `feel-roles.json`, and every build, reel, probe, gate and the integration. A builder who needs a line there leaves a `partial void` hook and names the one call in the report.

## Contracts at a glance (`NIGHT24-FEATURES.json`, copied from the seed at U3-38.1; the seed has the full check text)
remove-replay (R) | remove-daily-weekly (R) | pad-y-weapons (R) | shotgun-rate (R) | big-pick (F) | reticle-arc (F) | charge-fist (F) | switch-lag (lead) | campaign-core (L) | weapons-limits (L) | layer-1 (L) | lighting-kit (L) | wyrm (B)
</builders>

<tasks>
## U3-38.1 Start (0:00-0:15)
`date` (the night's zero); `export NIGHT=n24`; the Editor gate; `git status`; `tasklist | grep -i skygrind` (close strays by PID); CPU and python-process count logged; `editor_focus`, `set_autotick`. Copy `Research/2026-10-08-wave24/NIGHT24-FEATURES.seed.json` to `NIGHT24-FEATURES.json` and add U3-38 and U3-38.1 .. U3-38.9 to `tasks.json` (single-line items after the last U3-37 line, CRLF kept). Read the context files; act on GG's answers. Write `NIGHT24-LOG.md` and `NIGHT24-PROOF.md` (row, scenario, RED line, GREEN line, date).

## U3-38.2 The shared commit (0:15-0:50)
1. **Switches:** `Core/N24Blocks.cs` (bits: padY, shotgunTune, bigPick, chargeFist, reticleArc, campaign, wyrm, lighting, prewarm; `ForceOff`, `-skygrindOff24` / `--off24`; `--off22` and older imply it), `SurvivalSim.N24.cs` with partial hooks, `design.n24.<row>` rows in both config copies (each `enabled: false`, each `_why`), `FingerprintBaseline.N24OnPath` false until the freeze. Copy the NIGHT22 pattern (`N22Blocks.cs`, `SurvivalSim.N22.cs`, `N22SharedTests.cs`, `Harness.N22.cs`).
2. **Tape format 8:** stores the N24 byte AND the campaign state (level id, room index, checkpoint, limit flags: a `RunSetup.levelId` and a small block); formats 1-7 still read; a test and an `n24shared` harness line (copy `n22shared`).
3. **Scenario scaffolding:** `FeelReel.N24.cs` phase G with the 13 ids (`remove`, `title`, `padweapons`, `shotgun`, `bigpick`, `reticle`, `fist`, `room`, `limit`, `layer1`, `lit`, `wyrm`, `hitch`); each scenario body is a `static partial void` hook like `FeelReel.N22.cs` (a missing body prints `NOT WRITTEN (RED)`); `Harness.N24.cs` with the dispatch lines already placed for `levelgen`, `campaign`, `wyrm`, `fist`, `shotgun`, `hitch` (builders implement them in their own files; `Program.cs` is the lead's), `Ui.N24.cs` hooks (`BuildN24Hud` / `RenderN24Hud`), `SurvivalGame.N24.cs` view hooks, `partial void` run hooks for the campaign (`N24CampaignStartRun / Step`).
4. **Gates:** fingerprint EQUAL (as configured: nothing on yet), `--off24` EQUAL, `--off22` / `--off21` / `--off19` EQUAL to their baselines, golden 0 mismatches, `n24shared PASS`, the Editor compile, Editor Play 0 console errors. Commit. Launch R, F, L, B with a `date` and the briefs; L's first message is the format spike.

## U3-38.3 The baseline suite and the NIGHT23 debts (0:50-1:40, builders working headless; its fallback: if the suite is RED or the smoke cannot be fixed by 1:40, record it, branch your gates against the NIGHT23 totals (suite 1625+ / 1626, golden 97 / 97 per NIGHT21) and go on: `u3-n22` SHORT, never block the night on it)
Alone, the Editor focused, no edits under `Assets/`: `bash Tools/unity-loop.sh tests` async via `Tools/night/wait-tests.sh Logs/n24-suite0.json` (budget 25 minutes; if it has not completed, record the last status and the Editor's state, and go on: SHORT with the minutes). Then the smoke on the baseline Player alone (`BDIR=Builds/n24-0 bash Tools/night/smoke.sh n24-0`, 200-250 s, no one at the machine): if `pad only` and `grind22` fail again, **bisect the cause** (read `SliceSmoke.Night15Pad.cs` and the TITLE nav: the CODE `MenuItem`, `UiNav`, the ghost gate; try the smoke with the CODE item hidden) and fix it in a commit with the before and after lines. Green suite and smoke: tag `u3-n22` on the baseline commit. The Player build of the baseline is build 1.

## U3-38.4 R, F, B, L slices land (C0 1:20: the removals only; compile rounds C1 2:15, C2 3:20, C3 4:20, C4 4:50: the last integration; the final build comes after the freeze)
Each round: integrate, compile once in the background (`Tools/unity-loop.sh compile`, read `errors`: trust `compilationFailed` / `consoleErrors`, old lines linger), the focused tests of the rows, Editor Play 0 errors, a build, ONE reel invocation per side (ON keyboard; RED with `-skygrindOff24`; PAD with `-skygrindReelPad` for the pad rows), then `feel-metrics.py` one process at a time in the background (M-MAGENTA, M10, M9 on NEW elements, M11, S12 on the pad pass, T11, LUM). Record every RED and GREEN line in `NIGHT24-PROOF.md` with the build and the date. Return failures to owners by SendMessage with the exact line. The wyrm joins layer 1's boss room at C3 (4:20); a fight that is not integrated by C4 is SHORT with its standalone proof.

## U3-38.5 The contract rows (the builders' queues above; their contracts verbatim in NIGHT24-FEATURES.json)
Rules of thumb the reviewers will check: the fist never charges with time; the arc is under the reticle and small; a BIG PICK is one press and big; the campaign's rooms are closed rooms (a locked door, an encounter, a checkpoint) not a wave counter; limits restore at the door; the worm teaches sliding and jumping over passes; every number is in config.

## U3-38.6 switch-lag (the lead, 1:40-2:15, between the smoke and C1: it is the one Editor job with no integration in it)
Build the hitch logger (a ring buffer of unscaled frame deltas; log a frame over 3x the median and 8 ms with the last event tag; `ProfilerRecorder` main-thread and GC alloc) and the `hitch` tour (TITLE, PAUSED, SETTINGS, MODE switches, 3 weapon switches, a BIG PICK, RESULTS), cold then warm, in a Development Player, BEFORE any fix: print worst frame, p99, count above 33 ms. Fix the dominant measured cause first (RESEARCH.md S6 ranks them: `Canvas.enabled` instead of `SetActive` on large screens, a hidden boot prewarm pass with `Canvas.ForceUpdateCanvases`, TMP `TryAddCharacters`, shader prewarm, preloads out of click paths), re-measure after each. Measure in BOTH places GG sees it: the Editor Game view at 1x (Play Maximized, the Stats overlay closed; GG's screenshots were at 0.61x with the overlay open) and a Development Player; if only the Editor lags, say so (Editor noise) but still fix the cheap causes. Log CPU and the python-process count with every number: on a loaded box a pass is noisy, so a pass needs the thresholds on a quiet run, else SHORT with the numbers. Targets from the research (ours, not Unity's): no frame above 33 ms, worst cold frame under 20 ms.

## U3-38.7 The freeze (5:00) and the verification (5:00-6:15, one heavy job at a time; two reviewers read files in parallel and never touch the Editor)
In this order: (1) `FingerprintBaseline.N24OnPath` is true if and only if a sim-changing default is on (bigPick and chargeFist change the composed run: expect true); record `FingerprintBaselineN24` once, in its own commit, with the reason; `--off24` EQUAL to the old baseline, `--off22` to its, `--off21` to N20, `--off19` to N18; (2) the final build; (3) alone: `bash Tools/night/close.sh Builds/<final>`; (4) the suite on the frozen code, nothing beside it; (5) the smoke alone; (6) the rows' final scenarios (ON, RED, PAD) and the bench x3 only if the CPU is under 30 %; the reviewers (`Tools/night/reviewer-veto.md`, sides swapped, 25 minutes each) read the frames and logs from step (6) and the code while the suite of step (4) runs. While the suite runs the lead writes the report skeleton outside `Assets/`.

## U3-38.8 The leftovers, in order (a free builder takes the next; each gets a contract line before it starts; none after 5:00)
1. The other LIMIT flags (NO SLIDE, NO JUMP, MELEE ONLY) in the room format. 2. Room par times and per-room rank in the layer's stamp. 3. T-12: M9 pop-ins on the older screens (the Buy prompt, the Down menu, Markers, vignettes, the prompt wheel: NIGHT23 listed them). 4. The `directed` cue for the campaign (reuse NIGHT22's `UiDirected22`). 5. A second traversal link kit (a slide tunnel; a wall-kick shaft). 6. The WYRM's tail-shed sub-boss at 66 % and 33 %. 7. A 400 m highway stub with the gap meter and a running boss box, behind `design.n24.highway` (only if every contract row is DONE). 8. Port MONOLITH through BossKit (`skygrind/src/enemies/boss.js`, read only) as layer 2's boss. After 8, take the top row of `docs/memory/IDEAS.md` that is not parked.

## U3-38.9 The docs and the close (6:15-6:45; skeletons written earlier while the suite ran)
`NIGHT24-LOG.md` (the close table and the building-share split), `NIGHT24-REPORT.md` (one page for GG), `FEEL-PLAYTEST.md` (10 minutes, pad first: CAMPAIGN layer 1, the fist, the arc, the BIG PICK, the worm; per row what to look at), `PORT-STATUS.md` (events and the removals), `DECIDE24.md` state table, `HANDOFF.md` (a NIGHT24 block), `tasks.json`, `NIGHT24-FEATURES.json`, `NIGHT24-PROOF.md`, `DESIGN24-bosses.md` (what the worm taught), `docs/memory` (`wave-memory harvest 24`), and the **NIGHT25 prompt draft** (`NIGHT25.md`, untracked: the highway chase, the V2 rival, the MONOLITH / HIVE / ORBITER ports, level 2, from DESIGN24-bosses.md and what tonight measured). Tag `u3-n24`; `date`; print the table; "NIGHT24 CLOSED".
</tasks>

<definition_of_done>
| Item | DONE when (from this session's output) |
| --- | --- |
| U3-38.1 | the Editor gate logged, contracts copied, tasks.json parses, CPU and process count logged |
| U3-38.2 | the shared commit with its gates printed, tape format 8 tested, builders launched with a `date` |
| U3-38.3 | the baseline suite totals (or SHORT with its minutes), the smoke result, the pad-step cause named and fixed or SHORT, `u3-n22` tagged or SHORT |
| each contract row | its scenario seen RED, then GREEN on a Player or harness line from tonight, `passes: true` with the evidence line, the off-test green, golden exact, events in PORT-STATUS; or SHORT with the measured reason |
| U3-38.6 | the hitch numbers before and after (or SHORT with the dominant marker named) |
| U3-38.7 | five compile rounds logged, the freeze checks printed, the suite totals, the smoke, the close checks |
| U3-38.9 | the docs, `NIGHT25.md`, `u3-n24` tagged, the close table, "NIGHT24 CLOSED" |
A row is SHORT with a measured reason: a cap, a GG decision, a blocker after two approaches, the spend limit with its `date`, or the time limit.
</definition_of_done>

<final_report>
`NIGHT24-REPORT.md`, one page for GG, outcome first: how many of the 13 contracts pass on a Player, whether LAYER 1 plays door to door, whether the worm dies to a bot and how long it took. Then per row: what GG can do (one line), its switch, the contract's number, what felt worse. Then the hitch numbers before and after, what is not verified, and the DECIDE24 questions that still matter. Every number comes from a run this session.
</final_report>

<closing_reminder>
- **Start, then prove:** Editor Play with 0 console errors and the first Player log line before any scenario is trusted.
- **One heavy job at a time**, the Editor focused, no `Assets/` edits during a suite.
- **The campaign is rooms with doors, not waves with a counter:** reuse Zones and the GRIND's mode template; no new Mover verbs.
- Commit every slice. Never push, merge or commit to master; never stage `UnityConnectSettings.asset`.
- Every message ends with `N24-STATUS: <d> done, <s> short, <l> left (of 9 phases) · compile round <r> · builders <alive>/4 · passes <n>/13 · <h:mm> / 7:00`.
</closing_reminder>

## The kickoff

Launch `claude --model claude-opus-5-5 --append-system-prompt-file PROMPT-STOPS.md`, run `/effort high`, then paste:

```text
/goal Met only when a `date` reads 6:15 or later after the first `date` AND NIGHT24.md's close table is printed with every row U3-38.1 .. U3-38.9 DONE (its done-when evidence from this session) or SHORT (a measured reason: a cap, a GG decision, a blocker after two approaches, the spend limit with its date, or the time limit once a `date` reads 7:00), followed by "NIGHT24 CLOSED"; SHORT rows count as met; at 7:00 the run stops whatever the table says. Run NIGHT24.md in C:\Users\gaige\dev\SKYGRIND-unity: `date` first, then the Editor gate, read CLAUDE.md, NIGHT24.md, Research/2026-10-08-wave24/GG-REQUEST.md, DECIDE24.md and NIGHT23-LOG.md; commit the shared commit, launch builders R, F, L, B (at most 4 alive, 5 in total, in worktrees by file ownership), run the baseline suite alone, then integrate in compile rounds; proof-first: each row's scenario is seen RED before it counts. Freeze at 5:00. One heavy job at a time; Editor Play with 0 console errors before any Player is trusted; commit every slice; never push, merge or commit to master; the web repo is read only; never ask me questions. After a crash: git status, git log and the last log lines first. End every message with the N24-STATUS line.
```
