# NIGHT15.md: SKYGRIND in Unity: A MAP WITH A PERSONALITY (map night 1: the design, the pipeline, a playable greybox; aim assist and the pad), at most 12 h

> **Written 2026-10-05 ~11:15 AM from GG's request** ("a map overhaul ... built to maximize movement and replayability
> ... feel like CoD zombies and ULTRAKILL with personality, not a bland map"; "auto aim is too strong"; "is there
> controller support"; Steam, hopefully Switch). Anchor `u3-n14`. The new map takes about three nights: tonight the design,
> the tools that make a Unity-authored map possible, and a playable greybox of its heart; night 2 the remaining zones,
> buyables, objectives and quest; night 3 the look (lighting, landmarks, set dressing) and the first-fifteen-minutes pass.
> Queued after it: `QUEUE-first-fifteen.md` (the bot fixes, the look tour, teaching, a playtest kit) and
> `QUEUE-power-guns.md`.

You are Claude Code working in the Unity project `C:\Users\gaige\dev\SKYGRIND-unity` (Unity 6000.3.7f1, URP). GG starts
this run and leaves; the Editor is open on this project. Nobody answers questions during the night.

## Why this night (read this first; it decides every trade-off below)

**The map is the game's weakest part.** SURVIVAL plays on the web's arena: a 96 m square, symmetric, 97 boxes, 8 ramps,
8 jump pads, 17 grapple nodes and a 12 x 12 pillar core, cut into six gated zones that differ in name more than in how
they play. GG finds it bland, and the research agrees on what great wave maps do instead (`MAP-RESEARCH.md`, read it
fully):
- a place and a tone first, with a landmark you can call out
- loops within a ring for training, beside tight spaces
- one idea per space, so each zone asks for different movement and rewards different guns
- a quest that teaches the geography
- variation per run on fixed geometry, so players can master it
- a track sized for the car: SKYGRIND's dash, slide, wall-ride, pads and ARCLINE need room

**Nothing in the project can make a new map yet.** Every map comes from the web exporter (`PortExport/maps/*.json`, made
by the web's `maps.mjs`). The bot's nav grid (`navGrid`) is built only by the web's `createNavGrid`, with no C# port
(`NavField.cs:35` reads it), and the zone parts only by the web's `resolveZones`. The load path is hardcoded to
`maps/survival.json` (`SurvivalGame.Init.cs:24`), and `RunSetup` has no map field. So tonight builds the pipeline before
the map, and proves it on the old map first.

**Aim assist is too strong (GG).** Both the mouse (`settings.aimAssist`) and the pad (`settings.padAimAssist`) default to
`"snap"`: a 12° cone, 240°/s pull and a lock (`AimAssist.cs`, `config.json aim.lock`). Shooters give the mouse no
assist and the pad slowdown plus light adhesion (`MAP-RESEARCH.md` section 5). **Controller support exists but is
untested on a real pad:**
- Input System pads, Xbox and PlayStation glyphs, a radial deadzone, an 8-row GAMEPAD tab, rumble
- the radial weapon wheel is mouse-only (`WeaponSelect.cs:118`)
- no Nintendo glyph set
- no Steam Input
Steam and Switch need these right (`MAP-RESEARCH.md` sections 6-8).

Tonight's job: a design GG can react to, a Unity map pipeline proven on the old map, the new map's heart playable in
greybox, the aim assist calmed, the pad gaps closed. The map is new design (GG asked for it), so it lives in Unity-side
files; the sim's rules stay the web's.

## Read these files

- **`CLAUDE.md`** first, then this file, then **`MAP-RESEARCH.md`** (all of it).
- **`HANDOFF.md`** sections 0, 1, 4, 5 (D-57 .. D-59) and 6; **`PORT-RULES.md`** sections 6, 11 and 12; **`tasks.json`**
  (write U3-28 and U3-28.1 .. U3-28.9 below before any code).
- The memory notes: `play-check-eval-recipe`, `night-parallel-agents`, `unity-editor-play-mode-recompile`.

**Run length: up to 12 hours from your first `date`; the close starts by hour 10:30.**

## 0. Tonight's rules (on top of CLAUDE.md, which is in force)

1. **Branch `port/stage3`** from `u3-n14`. Tag `u3-n15` once, last, on the close commit. The web repo stays read only.
2. **The sim stays the web's.** The new map is data: boxes, barriers, wedges, pads, grapple nodes and the spots, in the
   same schema `SurvivalMap.FromExport` reads (`SurvivalMap.cs:103-219`), collided by the same C# web collider
   (CLAUDE.md rule 4). No new movement rules or physics.
   - Tonight builds with the primitives the collider and NavMap already handle (boxes, wedges, pads, the core grid).
     Arbitrary triangle meshes (`ThreeOctree.AddTriangle`) wait until NavMap handles them.
   - Mover.cs is untouched, and every golden and replay test stays at 0 mismatches.
3. **New map data is Unity-side design** (CLAUDE.md rule 5):
   - It goes in `Design/maps/<id>.json` with a `_why` header, plus its byte-identical Player copy, never in
     `PortExport/`.
   - Its per-map spots (the mystery box, the power switch, the exfil radio and sites) move into the map file. The
     survival map keeps today's `design.box / power / exfil` values.
4. **The old map keeps its game:**
   - `survival.json` and its fingerprint baseline are unchanged. The fingerprint pins `mapId: "survival"` and the web's
     aim-assist default (rule 6).
   - The smoke plays the old map, plus one new step for the new map.
   - The smoke's two known failures (steps 18 and 21, the NIGHT14 residual, HANDOFF section 6) are accepted tonight.
     Any other failure is a regression and is fixed before the next item.
5. **GG decides the map's identity (D-59).** Write three concepts, recommend one, and build the recommended one's
   greybox. Every number in the map is design you may set tonight. Enemy design, gameplay numbers in `config.json` and
   round order stay GG's: propose them in `MAP-DESIGN.md` section "Proposals".
6. **Aim assist, GG's call (2026-10-05, "too strong"):**
   - Unity-side defaults in a new `design.aimAssist` block (`_why` quoting GG): mouse `"off"`, pad `"assist"` (the web's
     mildest mode: a 6° cone, friction 0.55), strength unchanged.
   - A saved setting still equal to the old default `"snap"` migrates once to the new default (a settings-version bump).
     A player's own choice is kept.
   - `RunFingerprint` sets the web's `"snap"` for its run, so the baseline stays.
7. **Verify by looking, inside the item:** a change on screen is DONE only with a capture taken after it and a two-line
   written review (what a first-time player sees in one second; what is still wrong). Captures go to
   `Logs/shots/n15-*.png`.
8. **A Player build early:** build 1 within 2.5 hours of the first `date`, with the smoke run on it. Every build runs the
   smoke.
9. **One driver for the Editor:** you alone drive the Editor, the Players and git.
   - Builders (at most 4) work in worktrees, compile with `dotnet build` (`Tools/wt-csproj.cjs`) and never call `unity`.
   - Pause their builds before an Editor compile (keep the commit charge under 85 % of the limit).
   - If the Editor stops answering: `editor_focus`, wait 2 minutes, one `status`, then poll at most once a minute.
     After 20 minutes: log it, send a terminal notification, and move to work that needs no Editor. Restarting it follows
     GG's answer to D-57; with no answer, CLAUDE.md rule 10 stands.
10. **The machine:** benches only on AC with no heavy process beside them, before 1:00 AM; never beside the farm.
11. **Builders' briefs** carry CLAUDE.md's hard rules, this section, their files and their acceptance lines verbatim.
12. **No questions, no waiting.** Pick the safe default, log it with the rejected alternative, continue; one status line per
    item in `NIGHT15-LOG.md` with times from `date`.
13. **End every message with one status line** for the `/goal` checker: `N15-STATUS: <d> done, <s> short, <l> left (of
    9) · tests <passed>/<total> · <time from date>`.

## 1. The items (U3-28.1 .. U3-28.9)

Each item says why, what to do, and what proves it done. A "done when" line is a result you can quote (a test line, a
capture you reviewed, a validator report, a number), never "implemented".

### U3-28.1 Start (lead, 15 min)
- `date` first; HANDOFF section 1 row by row; `RECEIVED` in `NIGHT15-LOG.md` with the outputs; AC, power plan, CPU / GPU,
  commit charge; U3-28.* written into `tasks.json`.
- **Done when:** the arrival rows are logged and tasks.json parses.

### U3-28.2 MAP-DESIGN.md: three concepts, one recommended, designed to the metre (lead, 75 min + 30 min web research)
- **Why:** the map's personality is decided on paper, against the research and SKYGRIND's real movement numbers, before
  a single box is placed.
- **Do:**
  1. Read the movement numbers from `config.json` and derive the distances the map must fit, citing each key:
     - run and dash distance; slide speed and length
     - jump height and gap reach; pad apex; wall-ride length
     - ARCLINE range and swing radius; enemy speeds
  2. Research 30 minutes more where MAP-RESEARCH has gaps:
     - the top-down layouts of Kino der Toten, Der Riese, Mob of the Dead and Origins (sizes, loops, door order)
     - ULTRAKILL's arenas (4-2, 7-1)
     - DOOM Eternal's monkey-bar and pad placement
     - Cite every source; mark UNVERIFIED.
  3. Write three concepts, each with:
     - a place, a tone and a one-line pitch
     - the landmark visible from everywhere
     - the zone graph (6-8 zones, the hub, the loops, the gates and their cost curve)
     - per zone, its verb identity: favoured verbs, restricted verbs (marked by a visible material or shape), its wall
       guns, its threat style
     - the training spots and the choke points
     - the progression: power, THE FORGE's access, the PROTOCOL relays as an any-order quest where each relay is reached
       by its zone's signature verb
     - a visible expert shortcut per zone
     - the per-run variation, seeded from the run seed: box spots, HOLD spots, the GOLD RUSH route, which zone the
       PROTOCOL wakes first
     - how the old arena returns (an ENDLESS mode whose core re-shapes every wave, Cyber Grind style; or a mid-run "dark
       mirror")
  4. Recommend one concept (D-59; GG's later choice wins). Draw its plan as an SVG generated from the same numbers:
     top-down, metric grid, zones coloured, gates, loops, landmark, spots. Render it, look at it, and put the PNG in
     `Logs/shots/n15-plan.png`.
  5. Pitfalls checked against MAP-RESEARCH section 3: TranZit's size, Zetsubou's setup cost, Buried's single training
     path, restriction everywhere, overload in one room.
- **Done when:** `MAP-DESIGN.md` has the three concepts, the recommendation, the derived distances with their config
  keys, the plan PNG looked at (with a two-line review), and a "Proposals" section for GG's numbers.

### U3-28.3 The nav grid in C# (builder N in a worktree; lead 20 min to integrate)
- **Why:** without it no new map can have bots, spawns on walkable ground, or a nav graph.
- **Do:** port `createNavGrid` (the web's `src/ai/navGrid.js`, as the exporter calls it in
  `PortExport/scripts/maps.mjs`) and `resolveZones` (`src/survival/zoneMath.js`) to `Core/Nav/NavGridBuilder.cs` and
  `Core/ZoneMath.cs`, line for line with the web's comments (CLAUDE.md: Core, `noEngineReferences`, JsMath where numbers
  must match).
- **The oracle is already in the repo:** the exported `navGrid` of all three maps (survival 11,228 nodes, spire 11,178,
  training 4,048) and survival's `zoneParts` (114 parts).
- **Done when:**
  - Tests seen red then green: the C# builder's grid on each exported map equals the exported grid bit for bit (counts,
    every node and link), and `resolveZones` equals `zoneParts`.
  - CoreHarness `golden total ... 0 mismatches`.

### U3-28.4 The map pipeline: author in a scene, export, validate (builder P in a worktree; lead 40 min)
- **Why:** GG must be able to open a scene, move a platform, and play it; the agent must be able to build a map from a
  script that can be re-run.
- **Do:**
  1. Marker components (`Game/MapAuthoring/*`, Editor gizmos), each with its fields:
     - geometry: `MapBox` (min, max, kind), `MapWedge`, `MapBarrier`, `MapPad` (target, apex), `MapGrapple`
     - zones: `MapZone` (id, name, cost, parts, buy spots with faces)
     - spots: `MapPerkSpot`, `MapWallBuy`, `MapBoxSpot`, `MapHoldSpot`, `MapRelay`, `MapPower`, `MapExfil`,
       `MapForge`, `MapTrap`
     - spawns: `MapSpawn` (ground / high), `MapPlayerSpawn`; and the core spec
  2. `SKYGRIND/Map/Export` writes `Design/maps/<id>.json`: the schema plus `navGrid` and `zoneParts` from U3-28.3.
  3. `SKYGRIND/Map/Import` builds a scene of markers from any map JSON.
  4. `SKYGRIND/Map/Validate` writes `Reports/MAP-<id>-validate.md`:
     - every zone reachable from the start zone on the nav graph
     - every spot reachable, with its A* time from the player spawn at run speed
     - pads land on walkable ground
     - spawn nodes per zone above a design minimum
     - no walkable island without a way out
     - the map's bounds against the kill plane
- **Done when:** the round trip passes. Import `survival.json`, export it, and the sim-relevant keys are byte-identical.
  The fingerprint run on the round-tripped map is `EQUAL to the baseline`, and its validation report is clean (or every
  finding is explained).

### U3-28.5 Choosing a map (lead, 40 min)
- **Do:**
  - Add `RunSetup.mapId` (default `"survival"`). SurvivalGame loads `maps/<id>.json` from PortExport or `Design/maps`,
    plus each map's own spots.
  - TITLE's run setup gets a MAP row: the new map's working name and "ARENA" for the old one. Records and best runs are
    keyed by mode and map.
  - The fingerprint, the farm default, the bench and the old smoke steps pin `"survival"`.
  - The BotNav cache keys on the map (it already hashes the map JSON).
- **Done when:** the suite passes; a test loads both maps; the fingerprint is EQUAL; a capture of TITLE's MAP row is
  reviewed.

### U3-28.6 The greybox: the new map's heart (builder G in a worktree from U3-28.4's markers; lead 60 min)
- **Why:** a design is only real once you move through it.
- **Do:** an Editor build script (`Editor/Maps/Build_<id>.cs`, re-runnable) places the markers for the recommended
  concept:
  - the hub, the start zone and two more zones, each with its verb identity in geometry
  - the landmark's silhouette
  - one wall buy and a perk spot per zone, a box spot, a gate between each pair of zones
  - spawns, one HOLD spot, the player spawn
  Then: export, validate, play.
- **Done when:**
  - The validator report is clean.
  - The AUTOPILOT plays three rounds on the new map in the Player without a stall: a smoke step, its `autopilot ...
    hashes equal` line.
  - Five captures are reviewed: the plan from above, the hub, each zone's signature move, the landmark from far away.
  - `MAP-DESIGN.md` gets an "As built" section: what reads well, what is wrong, what night 2 changes.

### U3-28.7 Aim assist calmed (lead, 30 min)
- **Do:** rule 6: the `design.aimAssist` defaults, the one-time migration, the fingerprint pin. Rename the SETTINGS rows'
  values to what a player understands, from the web's mode names: OFF / LIGHT (assist) / STRONG (snap) / LOCK.
- **Done when:**
  - Tests seen red: a fresh profile has the mouse off and the pad on assist; a saved `"snap"` migrates once; a saved
    `"lock"` is kept.
  - The fingerprint is EQUAL.
  - A capture of the GAMEPAD tab is reviewed.

### U3-28.8 The pad's gaps (builder K in a worktree; lead 30 min)
- **Do:**
  - The radial weapon wheel on the pad: hold RB or a bound `PadAction`, the right stick picks, release takes.
  - A Nintendo glyph set: A / B and X / Y swapped positions, chosen by device name; Neutral stays the fallback.
  - The rumble default on, with its intensity row.
  - A "pad only" smoke step: TITLE to a run, the wheel, PAUSED, SETTINGS and back with the virtual pad, never the
    mouse.
  - `PLATFORM.md`: what Steam (Steam Input or not; the Deck's 1280x800 text sizes; Auto-Cloud on persistentDataPath) and
    Switch 2 (IL2CPP / AOT, the CPU budget, Mobile vs PC quality for the Switch platform,
    `QualitySettings.asset:119-131`) need. A checklist with file:line and the decisions for GG; no SDK work.
- **Done when:** the wheel test and the pad-only smoke step pass; a capture of the wheel opened by the pad is reviewed;
  PLATFORM.md exists.

### U3-28.9 Close (lead, 60 min; section 4)

## 2. Builders and file ownership

- **The shared commit first (lead, 20 min):**
  - the marker component stubs and the `Design/maps/` folder
  - `RunSetup.mapId`
  - the `design.aimAssist` block
  - the `PadAction` for the wheel
  - empty test classes per builder
- **N (the nav grid):** `Core/Nav/NavGridBuilder.cs`, `Core/ZoneMath.cs`, their tests.
- **P (the pipeline):** `Game/MapAuthoring/*`, `Editor/Maps/Export|Import|Validate*.cs`, their tests. P takes U3-28.3's
  builder through its interface (a stub until N lands).
- **G (the greybox):** `Editor/Maps/Build_<id>.cs`, `Design/maps/<id>.json`, after the shared commit and the design's
  plan.
- **K (the pad):** `WeaponSelect.cs`'s pad path, `PadReader.cs`'s glyph set, `Rumble` defaults, their tests.
- **Lead:**
  - the design doc
  - SurvivalGame* (map loading), the run setup UI, AimAssist settings, SliceSmoke*, PlayerChecks
  - integration, every build, Play check and capture
- Topic-named members in shared partial classes (PORT-RULES 11). Integration by cherry-pick, each builder's tests seen
  red first.

## 3. Caps and order

- **Caps:**
  - Play checks at most 3
  - captures at most 25
  - Player builds at most 5
  - bench runs at most 6 (only the UI-hurt series on the final build, on AC and idle)
  - builders at most 4, reviewers at most 1
  - research at most 30 minutes of web time
  - A cap reached makes its item SHORT with the count.
- **Lead minutes:** 15 + 20 + 105 + 20 + 40 + 40 + 60 + 30 + 30 + 60 = 420 (about 7 h of 12): the rest is buffer.
- **An item 25 % over its box** ships what works behind its flag, is marked `in_progress` with what remains, and the night
  moves on.

| Order | Item | Lead box |
|---|---|---|
| 1 | U3-28.1 start | 15 |
| 2 | the shared commit; builders N, P, K launched | 20 |
| 3 | U3-28.2 MAP-DESIGN.md (while N, P, K build) | 105 |
| 4 | U3-28.7 aim assist (then build 1 and its smoke by hour 2:30) | 30 |
| 5 | U3-28.3 integrate N | 20 |
| 6 | U3-28.4 integrate P; the round trip | 40 |
| 7 | U3-28.5 choosing a map | 40 |
| 8 | U3-28.6 builder G launched on the design; integrate; play it | 60 |
| 9 | U3-28.8 integrate K; PLATFORM.md | 30 |
| 10 | U3-28.9 close (starts by hour 10:30) | 60 |

- **Cut order** (only if the night runs short):
  1. U3-28.8's Nintendo glyphs and PLATFORM.md's Switch half
  2. the greybox's third zone
  3. the Import menu (keep Export and Validate)
- **Never cut:** U3-28.1, .2, .3, .4, .7, .9.

## 4. Close (U3-28.9)

1. **The final Release build.**
   - `git diff --stat` and the restores (TMP fallback, `Mobile_RPAsset.asset`).
   - The smoke passes with tonight's steps, except the two known failures.
   - `-skygrindGolden` 0 mismatches; `-skygrindFingerprint` EQUAL; `-skygrindReplay` on the smoke's tape EQUAL.
   - The UI-hurt Silent series, if the machine is on AC and idle (else SHORT with the measured state).
2. **`tasks.json`:** U3-28.* with status and evidence (a commit hash, an output line, a time). Never delete an item.
3. **Docs:**
   - `NIGHT15-LOG.md` (tracked).
   - **`NIGHT15-REPORT.md` for GG** (one page):
     - the three concepts in three lines each, and the recommended one's plan
     - what the greybox feels like, with three captures
     - the aim-assist change
     - the pad's state
     - questions: D-59 (the map's identity), the zone verbs, the old arena's future
   - **`HANDOFF.md`** rewritten with the `handoff` skill (anchor `u3-n15`).
   - **`NIGHT16.md`** (map night 2): the remaining zones, every buyable, the objectives and the quest, the per-run
     variation, the validator clean, a farm on the new map after the bot fixes from `QUEUE-first-fifteen.md` U3-28.3.
     The night after it: the look.
   - PORT-STATUS.md (a "Maps" row: the pipeline, the new map's state).
4. **Memory:** a lesson seen twice becomes a note; CLAUDE.md changes go to HANDOFF's pending decisions.
5. **Tag `u3-n15` once** on the close commit. `git status --short` shows only `UnityConnectSettings.asset` (if
   modified), `Assets/_Recovery*` (GG's to decide) and the untracked NIGHT*.md / QUEUE-*.md.
6. **Print the close table, then the line `NIGHT15 CLOSED`:**

| Item | Status | Evidence |
|---|---|---|
| U3-28.1 .. U3-28.9, one row each | DONE or SHORT | DONE: the item's "done when" evidence from tonight. SHORT: the measured reason (a cap, a decision reserved for GG, a blocker after two approaches, or the time limit once a `date` reads 10.5 hours or more after the first) |

## 5. Mistakes not to repeat (NIGHT7-14)

1. **Patch scripts through the shell.** Write `.cjs` / `.cs` files with the Write tool, then run them; no heredocs or
   `node -e`.
2. **A start-something eval** without a guard that outlives it, or with unqualified names or `using` lines.
3. **A smoke check that waits a number of frames.** A Player runs ~400 fps: wait in real time.
4. **A claim that wasn't checked:** grep or run before writing it. NIGHT14 told GG "round 6 is a wall" from bot data that
   was the bot's own limits.
5. **Two agents' generic names in one partial class** (PORT-RULES 11).
6. **A bench on a busy machine or on battery, or one run deciding anything.**
7. **Builds rewrite files:** run `git diff --stat` after each build, restore `Mobile_RPAsset.asset`; Play mode rewrites
   the TMP fallback asset.
8. **No Player build until the end.** NIGHT14's first build came after 10 hours and found two shipped bugs.
9. **CLI poll loops against a busy Editor, and forcing the import workers** (NIGHT14's hang).
10. **A builder's brief without the night's rules.**

## GG's standing instruction (verbatim, from NIGHT8)

A standing instruction from the user, the person you are working for. It is about how your turns end. A message with no
tool call in it ends your turn, and the work stops there until you are asked to continue. The user has seen you end turns
in four ways while work they asked for was still owed, and does not want any of them. One: a long summary of what was
done that closes by announcing the next step and has no tool call, so the next thing never starts. Two: an offer to carry
on with something unless the user would prefer otherwise, which stops to wait for an answer the user was not going to
give. Three: a list of decisions for the user when, by your own account, none of them blocks the rest of the work. Four:
deciding that this is a good place to report, because the turn has been long or a milestone is done. Status notes are
welcome, and so are your recommendations on open decisions, but put them in the same message as your next tool call and
carry on with whatever does not depend on the user's answer. If you notice yourself inviting the user to redirect you or
offering to wait, delete it and do the next thing. The stops the user does want are the ones where nothing can move
without them, or where the thing blocking you is deliberately protected from you. This does not override the need for
confirmation on risky or destructive actions.

## Closing reminder

- **Design on paper first, to the metre:** the movement numbers size the map, and the plan is looked at before it's
  built.
- **The pipeline is proven on the old map** (the round trip, the fingerprint EQUAL) before the new one depends on it.
- **The sim stays the web's;** the map is data; GG owns its identity (D-59), the numbers and the enemies. Write only
  what was measured; caps are caps; SHORT with a measured reason is a finished row.

## Suggested `/goal` for GG

> Run NIGHT15.md in C:\Users\gaige\dev\SKYGRIND-unity. Run `date` first, then read CLAUDE.md, NIGHT15.md and the files
> it lists, and work U3-28.1 .. U3-28.9 in NIGHT15.md section 3's order with at most four background builders split by
> file ownership. Never ask me questions and don't stop early; end every message with the N15-STATUS line. Never push,
> merge or commit to master; the web repo is read only. This goal is met when your latest message prints NIGHT15.md's
> close table with every item U3-28.1 .. U3-28.9 marked DONE (with its "done when" evidence from this session) or SHORT
> (with its measured reason: a cap reached, a decision reserved for GG, a blocker after two approaches, or the time limit
> once a `date` output reads 10.5 hours or more after the first `date` output), followed by the line "NIGHT15 CLOSED".
> SHORT rows count as met.
