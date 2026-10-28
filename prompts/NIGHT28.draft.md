# NIGHT27.md: SKYGRIND in Unity: LEVELS 4-10 TO THE SAME STANDARD (a 2.5 h build, hard cap 3 h: one new thing per level, platforming and wall-ride sections, a boss or mini-boss, authored geometry, narrower rooms, ranks from bot medians, a weapon unlock per level or two)

<library_header>
id: skygrind-n27 | version: 1 (DRAFT by builder L, 2026-10-08 ~22:30; the lead fills the TO FILL markers from NIGHT26's close table) | model / effort: claude-opus-5-5 / high
raw request: GG's next words, if any, in `Research/2026-10-09-wave27/GG-REQUEST.md` (the lead copies them there at 0:00); until then NIGHT26's open items and the DECIDE26 answers rule
base: branch port/stage3 at the commit tagged `u3-n26` (TO FILL: the close table, rows DONE / SHORT; suite totals; 22 / 22 replays; golden 39 sets)
outcome: (after the run)
</library_header>

<role_and_goal>
You are the lead engineer on SKYGRIND, a first-person movement shooter at dusk (Unity 6000.3.7f1, URP; a deterministic 120 Hz C# sim with its own collider and an input tape), in `C:\Users\gaige\dev\SKYGRIND-unity`, branch `port/stage3`. GG owns the game; you build for GG's taste, for Steam.

NIGHT26 rebuilt the first three levels: LEVEL 1 THE FOUNDRY (the tutorial, a platforming run, a wall-ride hall, a husks floor, THE SLAG HULK), LEVEL 2 THE ICE LINE (the glacier chase on ice, pistol only), LEVEL 3 THE KILN (layer 1 cut to 32 m rooms, then the worm with proximity aggro), a level select with ranks (time, kills, style, aggregate, P), CAMPAIGN as the default mode, the PULSE blast on the left trigger, weapon identity (an LT on every weapon, vulnerabilities, the rocket and BOUNCER nerfs) and a weapon unlock per level. Levels 4-10 are still NIGHT25's plan files: they teach one thing each, but their rooms are 52-60 m wide, they have no platforming or wall-ride sections, their pars are bot medians from the old graph bot, and the select shows them LOCKED. GG's words about level 1 apply to every level: "way too wide", "very bare bones", "I want it to really encourage better uses of mechanics like the wall jump and the dashing like platforming sections".

**GG's rules, kept from NIGHT26: build in about 2 hours; STOP OVER-TESTING: build, then check each thing once.** Level count in the end: 8 to 10 (the select has ten slots). Levels 11+ are not this night.

**Run length: a 2.5 h box from your first `date`: code freeze at 1:50, one suite and one smoke 1:50-2:20, docs and the close table 2:20-2:30, hard stop 3:00.** The close table prints at 2:20 or later, never after 3:00.
**NIGHT26's lessons rule this night (read `NIGHT26-LOG.md`):** (a) never wait on a stalled Editor: `Tools/night/dn-check.sh` compiles Unity-side code with dotnet and a Player builds from the batch-mode project copy; (b) a row passes only on a Player line with its RED; the lead reads the first Player line of every builder scenario the hour it lands; (c) a scenario must read the SAME input and the same objects a player has: NIGHT26's tutorial pilot held the click while the player now holds the left trigger (stages 1.5 and 1.8 stalled), the first `levelselect` seeded a profile the screen did not read, the first `pulselt` released before the charge; (d) bound every long scenario by GAME time and a wall guard, never by a frame count (the NIGHT25 levelwalk bug returned in the tutorial reel at ~120 s wall: unrecorded frames run at display rate); (e) a real gamepad plugged into the PC breaks virtual-pad steps; (f) a usage limit can cut the lead: commit every slice and keep the close table's skeleton written early.
**Testing budget (GG: stop over-testing):** per row ONE scenario seen RED once and GREEN once; one batch Player build per integration round (at most 6 builds); ONE suite at the freeze; NO persona triage, NO bench, NO farm, NO reviewers unless time remains; headless bots only for the ranks and clear rates (10 seeds, not 30).
</role_and_goal>

<ggs_words>
GG, 2026-10-08, voice-typed (spelling is his transcription's); decides ties. TO FILL: any new words after his playtest of NIGHT26 go here verbatim, first.
> "for that first map the arena is just way too wide ... it's still very bare bones and basic but I want it to really encourage better uses of mechanics like the wall jump and the dashing like platforming sections"
> "you slowly unlock weapons as you go on and maybe two levels in between the next weapon"
> "every single weapon has a positive a weakness and they work in certain scenarios ... you really don't feel incentivized at all to switch weapons"
> "I want it to be like ULTRAKILL where it kind of shows all the levels what rank you got what time ... copy that and go beyond this"
> "all this should really be built in 2 hours max ... stop with the over testing just build"
Earlier words that still bind (CORE.md): movement / crowd-control verbs need long cooldowns; enemies must reach every floor; no mystery box and no BIG PICK in the campaign; text 18 px or more; art and polish come later; GG wants BUILDING, not cuts.
</ggs_words>

<facts_already_measured>
Read at NIGHT26's close (`NIGHT26-LOG.md`, `NIGHT26-REPORT.md`); re-check each in the first 15 minutes (TO FILL where the close table moved a number).
- **Levels 1-3 (builder L, harness, seed 1 unless noted):** `levelgen` 0 errors for `tutorial`, `level2`, `layer1`; the tutorial pilot clears 14 stages (`tutorial --n26 --seeds 3`: 3 / 3, hulk 34.8 s, 4 stage respawns in total); the layer1 bot clears rooms 1-5 (17.5 / 64.9 / 54.1 / 6.6 / 8.0 s against pars 16 / 71 / 65 / 7 / 8); the flue's wall line 3.2 s against the stepped route 8.6 s.
- **Where things are:** plans `Design/levels/{tutorial,level2,layer1,level3,level4,level5,level06..level10}.plan.json` (+ generated `.json`; order in `design.n25.curriculum.order`; labels in `design.maps.list`: levels 4-10 are labelled LEVEL 4 SPITTERS, 5 VERTICAL, 6 CHARGERS AND LANCERS, 7 THE BULWARK, 8 WARDENS, 9 SPEED, 10 THE MONOLITH, 11 FINALE: the finale is slot 11 and the select lists ten); `Core/Rank26.cs` (ranks), `Core/LevelSelect26.cs`, `Game/UiLevelSelect26.cs`, `Game/CampaignUnlocks26.cs`, `Core/Unlocks26.cs` + `Design/levels/unlocks.json` (level3 zapper, level4 rail, level5 bouncer, level07 rocket; level06 and later rows missing), the plan format `Design/levels/LEVEL-FORMAT.md` (+ `surfaces`, `hallRoute`, `dashGap` keys from builder M), the platforming kits `Design/levels/kits/platforming.kits.json` with `Tools/night/gen-platforming-kits.py --place KIT OX OZ PREFIX` (shaft, dash, hop, hall), `TutorialPilot25` + `HulkPilot26` (scripted stages the graph bot cannot play).
- **Bosses on file:** HULK (level 1), GLACIER (level 2), WORM (level 3), BULWARK (level 7 slot), MONOLITH (slot 10), ELDER WYRM and RIVAL PRIME (the finale). The old levels' boss rooms are NIGHT25's, unchanged; their `design.n25.bosses` row stays.
- **Rules of the plan format learned tonight:** a pit's end ledge must be 5 m or more (a 3 m ledge put the exit in the pit); a wall slab must not reach the pit floor (it cut the pit cells off from the stair); a gap of 7 m with a 4 m island fails (the dash overshoots: islands 9 m); the validator's graph finds no route over an 8 m gap but passes 7 m; a dash-only gap is 6.5 m at most (M).
- **Open from NIGHT26 (TO FILL: the close table):** the glacier scenario was RED on the Player (no lane attack seen in 20 s; harness 170 s PASS); the worm's glow / aura pixels (the effects cost +8 % against the 3 % cut rule: V cuts the aura and embers); the first-play hitch (cold PLAY worst 74 ms on a quiet box at NIGHT25); `FingerprintBaseline.N26OnPath` not recorded (pulseLT and weaponIdentity change the composed run: record it at the freeze); every `rank` block's STYLE cuts are guesses (no run measured style); the WALL-RIDE HALL has no floor-versus-wall time in L's plan (M's hall kit has: 6.18 s against 13.47 s); a human's times on levels 1-3.
</facts_already_measured>

<scope>
- **In (levels 4-10):** (1) each level rebuilt to level 1's standard: ONE new thing that appears alone first; every room at most 32 m wide; authored geometry (no kit-only rooms); at least one PLATFORMING or WALL-RIDE section (M's kits or new ones); a boss or mini-boss at the end (the existing five bosses plus two small new ones: TO FILL which levels); (2) ranks: each plan's `rank` block from the headless bot's medians (time) and a measured style total, no guesses; (3) the weapon unlock per level or two in `unlocks.json` (zapper at 4, rail at 5, bouncer at 6, rocket at 8; the rows for the rest) and each level's LIMIT flags that fit the weapons owned; (4) enemy-versus-weapon vulnerabilities per level: each new room makes one owned weapon the right tool (data from `design.n26.vulnerability`); (5) surfaces: MUD and BOOST strips in later levels (ice is level 2's); (6) the NIGHT26 open items above.
- **Out:** audio; art and lighting (only if everything is DONE: GG said later); new Mover verbs; levels 11+; the web repo; the highway OUTRUNNER (NIGHT24's design stays parked). New ideas go to `docs/memory/IDEAS.md`.
</scope>

<design_brief>
**Per level (the standard, from NIGHT26's level 1):** the plan is the source (a generator script is a scratch tool, not kept); `levelgen` 0 errors, every pit with its stair and a `respawnY` stage respawn, no soft-lock, enemies reach every tier, a par that is the bot's median rounded up, a `rank` block (time cuts = the median x 1.0 / 1.5 / 2.5 / 4, kills 100 / 85 / 65 / 40, style from a measured run), the first room easier than the last room of the level before. A platforming stage is one verb. The scripted pilot (`TutorialPilot25`'s pattern) is written for each new movement stage the graph bot cannot play, and the harness prints a floor route against a wall route for every wall section (M's `hallRoute` row).
**The levels (the ONE new thing; TO FILL the geometry once the builders read the plans):** 4 SPITTERS AND COVER (the ranged enemy; cover lanes; unlock ZAPPER, stuns chains: a room where the zapper clears a crowd the pistol cannot); 5 VERTICAL (the grapple and ARCLINE; drones; a vertical wall-ride shaft; unlock RAIL, breaks shields); 6 CHARGERS AND LANCERS (crowd control: the slide kick and the pulse blast; a boost-strip lane; unlock BOUNCER); 7 THE BULWARK (shield break; an ice or mud section; a bulwark boss with 2 husk adds); 8 WARDENS (area denial; unlock ROCKET with its self-damage as the cost: the room asks for splash); 9 SPEED (a boost-strip run and a mud trap; a timed gate); 10 THE MONOLITH (the finale boss and the FINALE slot: RIVAL PRIME and the ELDER WYRM stay behind it; the select shows ten slots, so the finale needs a slot number decision: TO FILL with D-3).
**Level select:** levels 4-10 become playable one by one as their block lands (`design.n26.levelSelect.playable` 3 -> 10 at the freeze); `sequential` stays off unless GG says otherwise.
**Ranks, honestly:** replace the guessed STYLE cuts of levels 1-3 with the headless bot's style total over 10 seeds (S = 1.0 x the median, A 0.6, B 0.3, C 0.1), and print a human-skill factor (TO FILL: the bot is 1.4-2 x faster than GG on boss rooms).
</design_brief>

<lean_rules>
- **Building share:** at least 75 % of agent-minutes go to committed code, plans and fixes (GG: build).
- **Proof, small:** per row one scenario (`reel`) or harness line, RED once and GREEN once, printed with its preconditions; a long scenario is bounded by game time and a wall guard; the lead reads the first Player line the hour it lands.
- **Golden at every Core commit (CLAUDE.md rule 3):** fingerprint + golden + trajectory before each Core commit, said in the message; `Mover.cs` is never edited.
- **Config:** web numbers from `PortExport/config.json`; Unity-only numbers in `design.n27.<row>` or the plan's own `rank` block (both config copies byte-identical via `Tools/night/n24-sync.sh`; each with a `_why`); nothing typed in code; Mulberry32 only; no `System.Math` Sin / Cos / Atan2 in Core (JsMath).
- **Switches:** `Core/N27Blocks.cs` if a sim-changing block is added (TO FILL: probably none: levels are data); a level that ships behind `design.n26.levelN` style rows is off in `--off26`; with every block off the game is NIGHT26's on the sim path.
- **Caps (a cap reached = SHORT with the count):** Player builds 6; reel invocations 20; suite runs 2; builders 4 alive, 5 in total; no reviewers, no bench, no farm.
- **Heavy jobs one at a time:** a build, a reel, a suite, the smoke. An edit under `Assets/` while a suite runs recompiles the Editor and kills the run.
- **Commits:** every finished slice, explicit paths plus `.meta` (files outside `Assets/` have none); never `ProjectSettings/UnityConnectSettings.asset`; restore the TMP fallback font assets and `Mobile_RPAsset.asset` after Play or a build; never master, never push, never merge. Night prompts stay untracked; logs are tracked.
- **Time boxes:** `date` at every phase boundary into `NIGHT27-LOG.md`. A phase 15 minutes over its box is cut.
</lean_rules>

<context>
**Read first:** `CLAUDE.md` (imports `docs/memory/CORE.md`), this file, `NIGHT26-LOG.md`, `NIGHT26-REPORT.md`, `Design/levels/CURRICULUM.md` (the NIGHT26 section: levels 1-3 as built), `docs/LEVEL-DESIGN-NOTES.md` (the NIGHT26 notes: what the validator found), `Design/levels/LEVEL-FORMAT.md`, `Design/levels/kits/platforming.kits.json`, `Design/levels/unlocks.json`, `Design/weapons/WEAPON-IDENTITY.md`, `HANDOFF.md`.
**Key files:** `Game/Bot/TutorialPilot25.cs` + `HulkPilot26.cs` (the scripted-pilot pattern: lip arrays, per-room state reset), `Tools/SimHarness/Harness.N25.Level.cs` (`tutorial --n26`), `Harness.N24.Campaign.cs` (`campaign <plan> --seeds n`), `Assets/Editor/Maps/LevelPlan24.cs`, `Game/MapAuthoring/LevelValidate24.cs`, `Core/Campaign24.cs` (rooms, doors, LIMIT flags, `boss` key, `unlocks` hook), `Core/Rank26.cs`, `Game/FeelReel.N26.Level.cs`, `Tools/night/{dn-check.sh,integrate.sh,n24-sync.sh,reel.sh}`.
**GG's answers come first:** act on any filled Answer in `DECIDE27.md` or any new word from GG, and log how. A blank Answer means the default stands.
**The rules that bite:** CLAUDE.md 1 (git), 3 (golden exact), 4 (the C# collider), 5 (numbers), 6 (Mulberry32), 7 (no audio; boss events keep the web's names), 8 (cyan player, red-orange danger, magenta RIVAL only, 18 px text), 9 (docs by Write / Edit), 10 (close every Player by PID; never kill the Editor).
</context>

<autonomy>
- Never ask GG. Decide, log the choice with the rejected alternative, move on. What only GG decides is in `DECIDE27.md` with its default.
- If one approach fails twice, switch to the next-best and log both. A row that still fails after two fix rounds is SHORT with its numbers.
- **The Editor gate (first 10 minutes):** `date`; `bash Tools/night/editor-ready.sh`; a "Recovering Scene Backups" dialog: `powershell -File Tools/night/dismiss-dialog.ps1` after 5 minutes; no Unity process: start it once. Unreachable for 15 minutes: the batch path and `dn-check.sh`; Editor-only rows SHORT with the date.
- **After a crash or usage limit:** `git status`, `git log`, the last lines of `NIGHT27-LOG.md`; redo only what is half-done; commit before any risky step.
- The stops you do not want: a summary announcing the next step with no tool call; an offer to continue; a list of decisions that block nothing. (The kickoff appends `PROMPT-STOPS.md`.)
</autonomy>

<builders>
Four builders in git worktrees by file ownership (`isolation: "worktree"`); only the lead touches the Editor, builds, reels, the integration and the shared files. A worktree builder cannot run compound shell: scripts are written with the Write tool and run as one plain command; each builder merges `port/stage3` before starting and before reporting, and dn-checks its own worktree before every Unity-side commit.

| Builder | Owns | Queue (in order) |
| --- | --- | --- |
| **L1** (levels 4-6) | `Design/levels/level3..level5.plan.json` (+ `.json`, pars, `rank`), `TutorialPilot`-style pilots for their movement stages, `unlocks.json` rows 4-6 | (1) level 4 SPITTERS rebuilt (32 m rooms, a cover lane, a dash-gap link, zapper room); (2) level 5 VERTICAL (a wall-ride shaft with the hall kit, grapple nodes); (3) level 6 (a boost-strip lane once M's surfaces are in the plan); each `levelgen` 0 errors and a bot pass |
| **L2** (levels 7-10) | `Design/levels/level06..level10.plan.json`, their boss rooms' entry halls, `unlocks.json` rows 7-10 | (1) level 7 BULWARK (an ice or mud approach, the shield room, the boss); (2) level 8 WARDENS (lanes, rocket room); (3) level 9 SPEED; (4) level 10 and the finale slot |
| **W** (weapons and vulnerabilities) | `design.n26.vulnerability` rows, `Weapon26`, `Design/weapons/WEAPON-IDENTITY.md`, the loadout wheel | (1) the weapon-by-enemy table per level (which room asks for which weapon); (2) the unlock rows and LIMIT flags per level; (3) pulse and weapon prompts for the new levels |
| **M** (surfaces and bosses of the middle levels) | `Design/levels/kits/*`, `surfaces` in plans, mini-bosses (TO FILL: which two) | (1) mud and boost strips in levels 6 and 9; (2) the floor-versus-wall route rows for every wall section; (3) two mini-bosses if time |

**The lead alone** touches: `SurvivalSim.cs`, `SurvivalSim.Run.cs`, `SurvivalGame*.cs`, `Ui.N27.cs`, `FeelReel.N27.cs` (the phase list), `Tools/SimHarness/Program.cs`, `Harness.N27.cs`, `RunTape.cs`, `FingerprintBaseline*.cs`, the config merge, and every build, reel, integration. A builder who needs a line there names the one call in the report; the lead applies it the same hour.

## Contracts at a glance (`NIGHT27-FEATURES.json`, written by the lead at U3-41.2)
level-4 | level-5 | level-6 | level-7 | level-8 | level-9 | level-10 (each L1 / L2: levelgen 0 errors, a bot pass on 10 seeds with the first-try band 60-90 % for fights, a Player `levelwalk` line, ranks from medians) | unlocks (W: the order data and the wheel) | surfaces (M) | ranks-honest (L: measured style cuts for levels 1-10) | open-items (lead: glacier Player kill, worm pixels, hitch, N26 fingerprint) | fingerprint (lead)
</builders>

<tasks>
## U3-41.1 Start (0:00-0:10)
`date`; `export NIGHT=n27`; the Editor gate; `git status`; `GG-REQUEST.md` and `DECIDE27.md` from the table below; `NIGHT27-LOG.md`; tasks.json items U3-41 and U3-41.1 .. U3-41.6.
## U3-41.2 The shared commit and the builders (0:10-0:35)
Scenario ids (`level4` ... `level10`), the `design.n27` rows if any, `NIGHT27-FEATURES.json`; gates: fingerprint EQUAL, golden exact, dn-check 0. Launch the builders with a `date`.
## U3-41.3 Build rounds (0:35-1:50): integrate at 1:00, 1:25, 1:50
Each round: `integrate.sh`, `n24-sync.sh`, `dn-check.sh` 0, one batch Player build, ONE reel invocation for the new levels (`levelwalk` bounded by game time), the RED line once, then back to the builders with exact lines. The lead reads the first Player line of every row and three frames per new room.
## U3-41.4 Freeze (1:50) and verification (1:50-2:20)
Record `FingerprintBaselineN26` once in its own commit (pulseLT and weaponIdentity are on), `--off26` EQUAL to the old baseline; ONE suite on the frozen code (alone); the smoke once (real pad disabled around its virtual-pad steps); the final proof lines into `NIGHT27-PROOF.md`.
## U3-41.5 Docs and the close (2:20-2:30)
`NIGHT27-LOG.md` close table, `NIGHT27-REPORT.md` (one page for GG: outcome first: can he play levels 1-10 in order, what each boss is, the unlocks, the ranks), `FEEL-PLAYTEST.md`, `HANDOFF.md`, `PORT-STATUS.md`, `docs/LEVEL-DESIGN-NOTES.md`, `Design/levels/CURRICULUM.md` (levels 4-10 truthful), `docs/memory` (`wave-memory harvest 27`), and the NIGHT28 prompt. Tag `u3-n27`; `date`; print the table; "NIGHT27 CLOSED".
</tasks>

<definition_of_done>
| Item | DONE when (from this session's output) |
| --- | --- |
| U3-41.1 | the Editor gate logged, tasks.json parses |
| U3-41.2 | the shared commit with its gates printed; builders launched with a `date` |
| each level row | `levelgen` 0 errors, a bot clear on 10 seeds, its `levelwalk` / scenario line seen RED then GREEN on a Player or harness line from tonight, its rank block from medians; or SHORT with the measured reason |
| U3-41.3 | three integration rounds logged |
| U3-41.4 | the freeze checks, N26OnPath recorded, one suite, the smoke |
| U3-41.5 | the docs, `NIGHT28.md`, `u3-n27` tagged, the close table, "NIGHT27 CLOSED" |
A row is SHORT with a measured reason: a cap, a GG decision, a blocker after two approaches, the spend limit with its `date`, or the time limit.
</definition_of_done>

<final_report>
`NIGHT27-REPORT.md`, one page for GG, outcome first: can he start the game, pick any of levels 1-10 on the select, and play them in order; what each new level teaches and which weapon it unlocks; which levels have platforming or wall-ride sections; the ranks he sees and where they came from. Then per row one line (what GG can do, its switch, the contract's number, what felt worse), what is not verified, the DECIDE27 questions that matter. Every number comes from a run this session.
</final_report>

<closing_reminder>
- **Build, then check each thing once.** GG: stop over-testing; 2 hours.
- **A scenario reads what a player reads:** the same input (LT, not the click), the same profile, game time and a wall guard, never a frame count.
- **One new thing per level; narrower rooms; a platforming section; a boss; ranks from medians.**
- Commit every slice. Never push, merge or commit to master; never stage `UnityConnectSettings.asset`.
- Every message ends with `N27-STATUS: <d> done, <s> short, <l> left (of 5 phases) · round <r> · builders <alive>/4 · rows <n>/12 · <h:mm> / 3:00`.
</closing_reminder>

## DECIDE27 defaults (the lead copies this table to `DECIDE27.md`; a blank Answer means the default stands)
| id | question | default |
| --- | --- | --- |
| D-1 | Level order and numbering | tutorial (1), level2 (2), layer1 (3), level3..level09 (4-10); `level10` (the finale) is the 11th: shown after a slot count of 11, or folded into level 10 as its last boss (TO FILL) |
| D-2 | Weapon unlock order | THUNDERCLAP 1, NAILGUN 2, TORCH 3 (the RATTLER is not ported), ZAPPER 4, RAIL 5, BOUNCER 6, ROCKET 8 (rows for 7, 9, 10: TO FILL) |
| D-3 | The select's slot count | ten (an eleventh only if GG wants the finale as its own slot) |
| D-4 | Select progression | `sequential` off (all unlocked levels replayable at any time); on if GG wants ULTRAKILL's gate |
| D-5 | Ranks | per-level cuts from the bot's medians (time x 1.0 / 1.5 / 2.5 / 4), measured style, kills 100 / 85 / 65 / 40; aggregate S5 A4 B3 C2 D1 minus restarts, P = S x 3 with no restart |
| D-6 | Surfaces beyond ice | mud x2.0 and boost 6 m/s (the NIGHT26 D-6 numbers), only in plans' `surfaces` lists |
| D-7 | Survival modes | keep BIG PICK and the mystery box; the campaign has neither |
