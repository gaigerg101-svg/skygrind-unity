# NIGHT25.md: SKYGRIND in Unity: THE BOSS NIGHT (a 6.5 h night, hard cap 7 h: layer 2, the MONOLITH, the highway chase and the rival duel) [SKELETON: written during NIGHT24; the lead fills the see NIGHT24-LOG.md / NIGHT24-PROOF.md markers at the close]

<library_header>
id: skygrind-n25 | version: 0 (skeleton) | date: 2026-10-08 | model / effort: claude-opus-5-5 / high
raw request: GG, 2026-10-08, verbatim in Research/2026-10-08-wave24/GG-REQUEST.md ("split this up into different prompts"; this is the second prompt)
planning set: Research/2026-10-08-wave24/ (DESIGN24-bosses.md sections 2-4, MONOLITH-PORT-PLAN.md, R1-bosses.md, R2-linear-structure.md, DECIDE24.md, RED-TEAM.md) | forge: wave-forge
base: branch port/stage3 at NIGHT24's close commit (tag u3-n24)
outcome: (after the run)
</library_header>

<role_and_goal>
You are the lead engineer on SKYGRIND, a first-person movement shooter at dusk (Unity 6000.3.7f1, URP; a deterministic 120 Hz C# sim with its own collider and an input tape that replays runs bit for bit), in `C:\Users\gaige\dev\SKYGRIND-unity`, branch `port/stage3`. GG owns the game; you build for GG's taste, for Steam.

NIGHT24 turned the game linear: CAMPAIGN mode, rooms with doors (`Core/Campaign24.cs`, `Design/levels/LEVEL-FORMAT.md`, `Design/levels/layer1.plan.json`), LIMIT rooms, room ranks, the KILN WYRM as layer 1's boss (`Core/Enemies/Wyrm.cs`). **This night builds the rest of the bosses GG asked for:** layer 2 with the MONOLITH (ported through `BossKit`) as its boss, THE OUTRUNNER highway chase, and the V2-style rival duel, plus the pipeline and polish debts NIGHT24 left. GG: "three bosses, I've already made two of them and you can make one more"; "the old game had plenty of bosses"; "an endless highway ... you can't just move right to left"; "feel like V2 where it is a difficulty spike". Art style and polish stay later (GG's words).

**Run length: a 6.5 h box from your first `date` (code freeze 5:00, verification 5:00-6:15, docs and the close table 6:15-6:45). Hard stop 7:00.** NIGHT24's three rules hold: (1) a row passes only on a Player line from tonight, with its RED; (2) the first Player must START (Editor Play 0 console errors, then the Player log's first exception, before any scenario); (3) one heavy job at a time on this shared machine.

Order: start, the shared commit, builders, the baseline suite, compile rounds (C1 2:15, C2 3:20, C3 4:20, C4 4:50), the freeze at 5:00, the verification, the close.
**If time runs short, cut in this order** (each a logged SHORT with its numbers): HIVE / ORBITER ports (never in the contracts), the Outrunner's second and third phases (keep the straight road with the gap meter), the pop-in sweep's last elements, the level-pipeline extras, the MONOLITH's floor sink and adds (keep phase 1 and the midpoint on a flat arena, as the port plan allows). Never cut: layer 2's rooms and door-to-door play, the MONOLITH's first slice, the rival duel room, the proof.
</role_and_goal>

<ggs_words>
The sentences that decide ties (full text in GG-REQUEST.md; L-008: map loose words before building):
- "an endless highway ... you're chasing the boss ... Sonic ... you can't just move right to left 'cause that's boring." (DESIGN24-bosses.md section 2; open question D-10: does the boss pursue the car or does the player chase? The gap meter supports both; the default is the player chases.)
- "I wanted to feel like V2 where it is a difficulty spike." (section 3)
- "three bosses, I've already made two of them and you can make one more ... split this up into different prompts."
- "linear ... boomer shooter ... focus on the boss fights similar to ULTRAKILL ... start out with a pistol ... earn a shotgun ... limited weapons ... lose movement."
- NIGHT24's playtest words, see NIGHT24-LOG.md / NIGHT24-PROOF.md at the close (GG's reaction to the worm, the rooms, the pad's Y, the shotgun).
</ggs_words>

<facts_already_measured>
Read at the start; re-check each in the first 15 minutes. Numbers marked see NIGHT24-LOG.md / NIGHT24-PROOF.md are measured by NIGHT24 and written into this file at its close.
- **NIGHT24's close:** contracts passed on a Player: 12 of 13 (switch-lag SHORT, bigpick with a head-text defect); LAYER 1 door to door on a Player (158.6 s game time, the worm dead); suite 1720 / 1720; smoke all steps passed (n24-9) (the `pad only` / `grind22` TITLE failure: cause and fix see NIGHT24-LOG.md / NIGHT24-PROOF.md; suspects read: the CODE item on TITLE, the attract/kiosk lock `UiKiosk.KioskTitleLock`).
- **The worm:** a bot kills it in see NIGHT24-LOG.md / NIGHT24-PROOF.md s median over see NIGHT24-LOG.md / NIGHT24-PROOF.md seeds (`SimHarness wyrm`); clear rate per room see NIGHT24-LOG.md / NIGHT24-PROOF.md; deaths per room see NIGHT24-LOG.md / NIGHT24-PROOF.md; room par times vs median clear see NIGHT24-LOG.md / NIGHT24-PROOF.md (`SimHarness ranks` prints layer 1's pars: 45 / 45 / 50 / 20 / 25 / 180 s).
- **The hitch (screen switch):** before 346 ms, after 328 ms (prewarm off again), dominant marker the first PLAY (the lead's hitch tour, `FeelReel.N24.Hitch`).
- **The pad and the shotgun:** layout v4 (Y = next weapon, hold = wheel, RB = SPECIAL + ARCLINE) on see NIGHT24-LOG.md / NIGHT24-PROOF.md; the THUNDERCLAP measured 0.408 s shot-to-shot before (NIGHT20's retune x0.45 of the web's 0.9 s), 0.525 s after (`SimHarness shotgun`); GG's word on it see NIGHT24-LOG.md / NIGHT24-PROOF.md.
- **Reel results:** `remove`, `title`, `padweapons`, `shotgun`, `limit`, `layer1`, `wyrm` Player lines see NIGHT24-LOG.md / NIGHT24-PROOF.md (the ledger is `NIGHT24-PROOF.md`).
- **What the code already gives (grep before you cite):** `Core/Enemies/BossKit.cs` + `BossMath.cs` (the web's boss framework: intro card, beats at 66 % / 33 %, rewards), `Core/Enemies/Bulwark.cs` (847 lines, built, `design.adds.bulwark` off: the ported boss template), `Core/Enemies/Wyrm.cs` (the newest conventions: `StartFight`, `Step`, `Filter` + `Apply`), `Game/FeelReel.N24.Wyrm.cs`, `Tools/SimHarness/Harness.N24.Wyrm.cs`, `Core/Campaign24.cs` (`Limit24`, `RankOf`, `StampText`), `Game/SurvivalSim.Campaign24.cs` (`N24LimitFrame`), `Assets/Editor/Maps/Build_drydock.cs` (`DrydockPlan`, the plan converter) and `Assets/Editor/BatchBuild24.cs`, `PortExport/maps/spire-duel-navgrid.json` + `Core/Nav/NavGraph.cs` (the rival's duel arena), `Tools/night/dn-check.sh` (headless compile gate), `Tools/night/n24-sync.sh`.
- **The web's bosses (frozen, read only):** MONOLITH `boss.js` 695 lines, HIVE `hive.js` 419, ORBITER `orbiter.js` 687. The MONOLITH port plan is `Research/2026-10-08-wave24/MONOLITH-PORT-PLAN.md` (estimate 18-20 builder-hours for all of it, 9-10 for phase 1 plus the midpoint on a flat arena).
- **The machine:** see NIGHT24-LOG.md / NIGHT24-PROOF.md (CPU, python process count, Editor state at the first `date`).
</facts_already_measured>

<scope>
- **In:** the contracts below; the BULWARK decision (U3-39.6); the T-12 pop-in sweep (`docs/memory/THREADS.md` T-12); the leftovers NIGHT24 did not reach: see NIGHT24-LOG.md / NIGHT24-PROOF.md (the lead lists them at the close; candidates: the RESULTS / stamp UI for room ranks and the closing stamp text, the optional NO SLIDE / NO JUMP / MELEE ONLY extra room in `layer1.plan.json`, the lighting kit's remainder, the `playerInput.events = events` wiring for `weaponSwitch`).
- **Out:** audio (CLAUDE.md rule 7); art style and polish (GG: later); new Mover verbs (rule 3: use the existing wall jump / kick / slide / grapple / pads); the web repo; HIVE and ORBITER unless every contract is DONE; balance beyond the bosses' first pass. A new idea goes to `docs/memory/IDEAS.md`, never into code.
</scope>

<lean_rules>
- **Building share:** at least 65 % of agent-minutes go to committed code, scenarios, level plans and fixes; measuring and reviewing the rest. Log the split in the close.
- **Proof-first, in a size that fits:** each contract row has its scenario (`reel`) or harness command written and seen RED (the same scenario with the row off, or before the feature) before it is called built; a scenario prints its PRECONDITIONS (`FeelReel.Preconditions` and `SafeSpot`). A row passes only on a Player or harness line from tonight.
- **The startup gate (NIGHT22's lesson):** after every integration that touches UI or views: Editor Play, `bash Tools/unity-loop.sh errors` shows `consoleErrors` 0, THEN the first reel's Player log has no exception. Builders cannot compile Unity-side code in the Editor, but `bash Tools/night/dn-check.sh <scratch dir>` compiles it headless: every Unity-side commit is dn-check 0 first (the script copies the main checkout; a builder in a worktree runs a copy of it pointed at the worktree).
- **Golden at every Core commit (CLAUDE.md rule 3):** a builder runs the harness `fingerprint` and the SimHarness golden / trajectory commands before each Core commit and says so in the message; the lead runs the Editor tests `Golden` and `Trajectory` at every integration. `Mover.cs` and `Core/Movement` are never edited.
- **Config files:** each builder edits ONLY its own `design.n25.<row>` block in `Design/unity-config.json`; the LEAD copies it to `Assets/StreamingAssets/Design/unity-config.json` at every integration (`cmp` must print identical). Nobody else writes the Player copy.
- **Testing:** one focused test per rule, seen red once; the full suite twice (the baseline and the close); re-run only failing files; a red that passes alone is re-run twice before it is called a flake. Tests verify the work: never edit one to pass it; a changed premise is said in the commit.
- **Wired, not just defined:** every config key is read by code, every event is in `PORT-STATUS.md`, every switch has an off-test (`--off25` / `-skygrindOff25` = NIGHT24's game on the sim path), both config copies byte-identical, the tape round-trips the new run state.
- **Numbers:** web numbers from `PortExport/config.json`; Unity-only numbers in `design.n25.<row>` (both copies, each with `_why`) or `UnityTuning.*.cs`; nothing typed in code. Randomness: Mulberry32 only (the MONOLITH port's attack stream replaces the web's `Math.random`: log it). Looks: shipped materials and shaders only; cyan = player-usable, red-orange = enemies and danger only, magenta = RIVAL only; text 18 px or more (M10); read frames from the Player reel, never the Editor's.
- **Caps (a cap reached = SHORT with the count):** Player builds 10; reel invocations 40; suite runs 2; builders 4 alive and 5 in total; veto reviewers 2 (at C4, 25 minutes each); no persona triage; farm batches 2 (alone); bench only if the CPU is under 30 % before each of 3 runs. `export NIGHT=n25` at 0:00.
- **Heavy jobs one at a time, always:** a build, a reel, the metrics, the smoke, the suite, a farm. Check `tasklist` for stray `SKYGRIND.exe` first and close them by PID. An edit under `Assets/` while the suite runs recompiles the Editor and kills the run: the lead edits only outside `Assets/` during a suite. Keep the Editor focused (`editor_focus`; `set_autotick`).
- **Commits:** every finished slice, explicit paths plus `.meta` files; builders at least every 30 minutes; never `ProjectSettings/UnityConnectSettings.asset`; restore the TMP fallback font assets, `Assets/Resources/Fonts/*.asset` and `Mobile_RPAsset.asset` after Play or a build (`git checkout --`); after every Player build `git diff --stat ProjectSettings Assets/Settings`; never master, never push, never merge (CLAUDE.md rule 1). Night prompts stay untracked; logs are tracked.
- **Time boxes:** `date` at every phase boundary into `NIGHT25-LOG.md` (Write / Edit only). A phase 25 minutes over its box is cut; the rest goes to the leftovers.
</lean_rules>

<context>
**Read first:** `CLAUDE.md` (imports `docs/memory/CORE.md`), this file, `Research/2026-10-08-wave24/DESIGN24-bosses.md` (sections 2-4), `MONOLITH-PORT-PLAN.md`, `DECIDE24.md` (act on any filled Answer, especially D-10), the NIGHT24 block of `HANDOFF.md`, `NIGHT24-LOG.md`, `NIGHT24-REPORT.md`, `Design/levels/LEVEL-FORMAT.md`.
**Git:** stay on `port/stage3`; tag `u3-n25` once on the close commit.
**The rules that bite:** CLAUDE.md 1 (git), 3 (golden exact; `Mover.cs` untouched), 4 (the C# collider, no PhysX), 5 (numbers), 6 (Mulberry32), 7 (no audio; boss events keep the web's names: bossRise, bossPhase, bossTell, bossHit, bossSlam, bossVolley, bossSurge, bossKneel, bossSink, bossDown), 8 (roles, 18 px), 9 (docs by Write / Edit), 10 (close every Player by its PID; never kill the Editor).
</context>

<autonomy>
<builders>
Four builders in git worktrees by file ownership; **only the lead touches the Editor, builds, reels, the integration and the shared files** (`SurvivalSim.cs`, `SurvivalSim.Run.cs`, `SurvivalGame*.cs`, `Ui.N25.cs`, `FeelReel.N25.cs`, `Tools/SimHarness/Program.cs`, `Harness.N25.cs`, `RunTape.cs`, `FingerprintBaseline*.cs`, the config merge, `Tools/feel-metrics.py`, `Tools/feel-roles.json`). A builder who needs a line there leaves a `partial void` hook and names the one call in the report. Builders run `dn-check` on a worktree copy and the SimHarness gates, and report what the Editor has not compiled.

| Builder | Owns | Queue (in order) |
| --- | --- | --- |
| **B** (the MONOLITH) | `Core/Enemies/Monolith*.cs`, `Game/EnemyViews.Monolith.cs`, `Game/SurvivalSim.Monolith25.cs` (the `SurvivalSim.Boss20.cs` extension), `Design/bosses/monolith.json`, `Design/maps/monolith-arena.*`, `FeelReel.N25.Monolith.cs`, `Harness.N25.Monolith.cs`, their tests | MONOLITH-PORT-PLAN.md phases 1-5: Core sim and tests (copy `Wyrm.cs`, `Bulwark.cs`, `BulwarkTests.cs`), harness `monolith` (a bot kills it, a tape replays EQUAL, tells >= 0.5 s), a flat arena first, view, reel; then the floor sink and adds if time |
| **O** (the OUTRUNNER) | `Core/Chase25*.cs`, `Game/ChaseView25.cs`, `Design/levels/highway.plan.json`, `Harness.N25.Chase.cs`, `FeelReel.N25.Chase.cs`, their tests | DESIGN24-bosses.md section 2: the gap meter (Core, pure), tile recycling (the world does not scroll), the boss box that runs (the 400 m stub), then hazards that need a move (jump / slide / airborne / dash), then the six shield plates and the finisher |
| **D** (the rival duel) | `Core/Rival25*.cs`, `Design/levels/duel.plan.json`, `Harness.N25.Rival.cs`, `FeelReel.N25.Rival.cs`, their tests | DESIGN24-bosses.md section 3: one room, no adds, the rival dashes / slides / shoots / uses cover and learns nothing (magenta is RIVAL only); the duel nav graph from `PortExport/maps/spire-duel-navgrid.json` through `Core/Nav/NavGraph.cs`; a checkpoint at the door; a bot beats it in a measured band (not 0 %, not 100 %) |
| **L** (layer 2 and the pipeline) | `Design/levels/layer2.plan.json`, `Core/Campaign24*.cs` extensions, `Assets/Editor/Levels/*`, `Harness.N25.Level.cs`, `FeelReel.N25.Layer2.cs`, the MODE row in `Game/UiMeta.cs`, their tests | layer 2's level plan (rooms, LIMIT rooms with the flags NIGHT24 added, the duel room, the MONOLITH's room as the boss), the plan validator and a one-command plan-to-playable build, the RESULTS / stamp UI for ranks if NIGHT24 left it, then the pipeline improvements below |

## Contracts at a glance (`NIGHT25-FEATURES.json`, written from this section at U3-39.1)
monolith (B) | outrunner-stub (O) | rival-duel (D) | layer-2 (L) | bulwark-decision (lead) | level-pipeline (L) | pop-ins (lead + a free builder) | leftovers see NIGHT24-LOG.md / NIGHT24-PROOF.md
</builders>
</autonomy>

<tasks>
## U3-39.1 Start (0:00-0:15)
`date`, `export NIGHT=n25`, the Editor gate (`bash Tools/night/editor-ready.sh`; the "Recovering Scene Backups" dialog: `powershell -File Tools/night/dismiss-dialog.ps1` after 5 minutes), read the context, copy the contracts to `NIGHT25-FEATURES.json`, log CPU and process counts, act on GG's answers in DECIDE24.md.

## U3-39.2 The shared commit (0:15-0:50)
`Core/N25Blocks.cs` (bits: monolith, outrunner, rival, layer2, bulwark, ...; `ForceOff`, `-skygrindOff25`; `--off24` and older imply it), tape format 9 carrying the new value, `design.n25.<row>` rows (each `enabled: false`, each `_why`), the `partial void` hooks, `FeelReel.N25.cs` with its scenario ids, `Harness.N25.cs` dispatch with a `NOT WRITTEN (RED)` line per command, `FingerprintBaseline.N25OnPath` false. Gates printed: fingerprint, golden, trajectory, `n25shared`. Launch the builders with a `date`.

## U3-39.3 The baseline suite and NIGHT24's debts (0:50-1:40, builders working headless)
The full suite once (poll `Tools/night/wait-tests.sh`), the Player smoke, the TITLE `pad only` / `grind22` result, the hitch tour numbers, `u3-n24` tagged or SHORT.

## U3-39.4 Slices land (compile rounds C1 2:15, C2 3:20, C3 4:20, C4 4:50)
`Tools/night/integrate.sh` (update its ids), `dn-check` 0, Editor Play 0 console errors, the first Player log clean. Hand-resolved picks go to `Logs/night-picked.txt`.

## U3-39.5 The contract rows (the builders' queues above)
- **monolith (B):** Core sim, tests, harness `monolith` (a bot kills it in a measured time on a flat arena, tells >= 0.5 s, a tape replays EQUAL, fingerprint EQUAL with the row off), the view in the web's look (stone 0x6b625c, core 0xff7a20), a reel with its intro, slam ring, kneel and finisher; through `BossKit` (`kit.bosses["monolith"]`, `kit.Start`), names MONOLITH / THE GRINDER WAKES already in `bossKit.names`. Port notes log the changed attack stream.
- **outrunner-stub (O):** 400 m straight road in 40 m tiles recycled behind the player, the gap meter (starts 60 m, hurt at 25 m or less, escape at 80 m = soft fail to a checkpoint, one-way rubber band shown in the HUD), a boss box that runs at base speed, the player's boosted movement 1.4x or more; a bot closes the gap in a measured time. World-side speed cues only (no screen-border lines, retired in w20).
- **rival-duel (D):** as in the table; magenta for the rival only; a checkpoint at the door; the bot win rate measured over 20 seeds.
- **layer-2 (L):** `layer2.plan.json` converts, validates and plays door to door with a bot; at least one LIMIT room per extra flag from `Limit24` (noSlide, noJump, meleeOnly); the duel room before the MONOLITH room; par times set from the bot's medians; ranks printed by `SimHarness ranks`.
- **level-pipeline (L):** plan validator errors name the room and the field; a `SimHarness levelgen` run builds both layers in one command; `LEVEL-FORMAT.md` updated; see NIGHT24-LOG.md / NIGHT24-PROOF.md (the lead adds what NIGHT24's levelgen run showed was slow or fragile).

## U3-39.6 The BULWARK decision (the lead, 0:50-1:40)
`Core/Enemies/Bulwark.cs` is built and off (`design.adds.bulwark`). Decide with the measured numbers: wire it as a mid-layer boss (layer 1's room 5 or a layer 2 room), keep it as a SURVIVAL round boss behind a switch, or leave it off and record why. Log the choice and its Player line.

## U3-39.7 The pop-in sweep (T-12)
M9 pop-ins on older elements (NIGHT23-LOG): the Buy prompt, HP damage ghost, Markers, Down menu, prompt wheel, Points pop, hurt and damage vignettes, round-clear text, Style decay bar, Exposed. Most already have NIGHT18 entrances behind `FeelAB` (`Prompt18`, `Exposed18`, `MarkerEnter18`, `FlashAttack18` in `Game/Ui.cs`, `UiMarkers.cs`): first take the per-path list from a reel (`Tools/night/m9-list.py`), then give each flagged element a short fade-in (0.12 s or less) the way `UiDeadline22.cs` did (`Deadline22PopIn`), one commit per element; list roles in `Tools/feel-roles.json` if M11 needs it.

## U3-39.8 The leftovers, in order (a free builder takes the next; each gets a contract line first)
see NIGHT24-LOG.md / NIGHT24-PROOF.md at the close. Seeded: the RESULTS / stamp UI for room ranks; the `weaponSwitch` wiring check; the lighting kit's remainder; HIVE (`hive.js`, egg sacs then the queen's leaps) and ORBITER (`orbiter.js`, ring station, pods, bullet curtain, mines, lightning sweep) as later layers' bosses through `BossKit`, one boss per layer.

## U3-39.9 The freeze (5:00), the verification (5:00-6:15), the docs and the close (6:15-6:45)
Five compile rounds logged, the freeze checks, the suite totals, the smoke, two veto reviewers at C4, `NIGHT25-REPORT.md`, `HANDOFF.md`, the `NIGHT26.md` skeleton, `u3-n25` tagged, the close table, "NIGHT25 CLOSED".
</tasks>

<definition_of_done>
| Item | DONE when (from this session's output) |
| --- | --- |
| U3-39.1 | the Editor gate logged, contracts copied, CPU and process count logged |
| U3-39.2 | the shared commit with its gates printed, tape format 9 tested, builders launched with a `date` |
| U3-39.3 | the baseline suite totals (or SHORT with its minutes), the smoke result, `u3-n24` tagged or SHORT |
| each contract row | its scenario seen RED, then GREEN on a Player or harness line from tonight, `passes: true` with the evidence line, the off-test green, golden exact, events in PORT-STATUS; or SHORT with the measured reason |
| U3-39.6 | the BULWARK choice logged with its measured numbers |
| U3-39.7 | the pop-in list before and after (counts from the reel), one commit per element |
| U3-39.9 | five compile rounds logged, the freeze checks, the suite totals, the docs, `NIGHT26.md`, `u3-n25` tagged, the close table, "NIGHT25 CLOSED" |
A row is SHORT with a measured reason: a cap, a GG decision, a blocker after two approaches, the spend limit with its `date`, or the time limit.
</definition_of_done>

<final_report>
`NIGHT25-REPORT.md`, one page for GG, outcome first: how many contracts pass on a Player, whether LAYER 2 plays door to door, whether the MONOLITH dies to a bot and how long it took, whether the chase stub runs, the rival duel's bot win rate. Then per row: what GG can do (one line), its switch, the contract's number, what felt worse. Then the pop-in counts before and after, what is not verified, and the DECIDE questions that still matter (D-10: who chases whom). Every number comes from a run this session.
</final_report>

<closing_reminder>
- **Start, then prove:** Editor Play with 0 console errors and the first Player log line before any scenario is trusted.
- **One heavy job at a time**, the Editor focused, no `Assets/` edits during a suite.
- **One boss per layer:** the worm is layer 1's, the MONOLITH layer 2's; the rival duel is a room, the chase a stub until the pieces beneath it prove out.
- Commit every slice. Never push, merge or commit to master; never stage `UnityConnectSettings.asset`.
- Every message ends with `N25-STATUS: <d> done, <s> short, <l> left (of 9 phases) · compile round <r> · builders <alive>/4 · passes <n>/<total> · <h:mm> / 7:00`.
</closing_reminder>

## The kickoff

Launch `claude --model claude-opus-5-5 --append-system-prompt-file PROMPT-STOPS.md`, run `/effort high`, then paste:

```text
/goal Met only when a `date` reads 6:15 or later after the first `date` AND NIGHT25.md's close table is printed with every row U3-39.1 .. U3-39.9 DONE (its done-when evidence from this session) or SHORT (a measured reason: a cap, a GG decision, a blocker after two approaches, the spend limit with its date, or the time limit once a `date` reads 7:00), followed by "NIGHT25 CLOSED"; SHORT rows count as met; at 7:00 the run stops whatever the table says. Run NIGHT25.md in C:\Users\gaige\dev\SKYGRIND-unity
```
