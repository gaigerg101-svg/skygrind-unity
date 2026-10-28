# FORGE65 code scout: rooms, levels, the judges and the gates (2026-10-23, read-only)

Everything below was read from the tree at fa4082a5 (port/stage3) or measured with read-only commands. "Not determined" means I could not
settle it in the time.

## 1. The level format and the code that loads it

- **Two files per level.**
  - A plan, `Design/levels/<px>/<level>.plan.json`, is written by a generator, `Tools/night/gen-<px>-<level>.cjs`.
  - The game loads the generated map JSON, `Design/levels/<px>-<level>.json`, plus a byte copy in `Assets/StreamingAssets/Design/levels/`.
  - Pipeline: gen script -> plan -> `dotnet run --project Tools/SimHarness -c Release -- levelgen <plan> <out> --walk` -> `gen --finish`.
    `--finish` writes both copies (gen-n57-kiln.cjs:14-17, 28-45). levelgen also writes `Reports/LEVEL-<id>-validate.md`
    (Design/levels/LEVEL-FORMAT.md:8-9).
  - levelgen is `LevelPlan24.Build` (Assets/Editor/Maps/LevelPlan24.cs:216) -> `DrydockPlan.Build`. It is compiled into SimHarness
    (Tools/SimHarness/SimHarness.csproj:40-42), so no Editor is needed.
- **Plan top level** (n57/kiln.plan.json; metres, x east, y up, z south, a rect is `[x0, z0, x1, z1]`, LEVEL-FORMAT.md:23): `rooms, doors, boxes, ramps, pads, grapple, pits, tunnels, lights, props, rank`, plus the flags
  `n29 / n30 / n36` (roof, lights) and `floorSize, killPlaneY, wallHeight`.
  - boxes: `{zone, kind: deck|slab|hull|wall, min, max, job, why}`.
  - ramps: `{zone, from, to, width, rise}`.
  - pads: `{zone, at, target, apexAbove}`.
  - grapple: `{zone, at}`.
  - pits: `{zone, rect, floorY, lava?}`.
  - tunnels: `{zone, rect, ceiling}`.
  - Sources: LEVEL-FORMAT.md:35-88.
- **Room row** (LEVEL-FORMAT.md:46-58):
  - Base keys: `id, name, kind: room|link|boss, rect, par, spawn{at, yawDeg}, exit, nodes, highNodes, waves, limit, award, wallRoute`.
  - Later nights added keys to the same row: `height, roof, job36, verb36, card57, arena36, waveNodes43, roles43, proofs, frames36,`
    `frames52 (frames52.entry is what a157 frames), lessons, owns38`. Doors: `{id, between, rect, height, checkpoint}`.
  - The generated JSON keeps all of it under `campaign`: `rooms, doors, rank, pits, tunnels, wallHeight, lights, props, boxes`, next to
    `collision, navGrid, zoneParts, pads, grappleNodes, lava`.
- **Runtime:**
  - `SurvivalMap.FromExport` (Assets/Scripts/Core/SurvivalMap.cs:112) parses the map.
  - `Campaign24` (Core) runs the rooms: zones lock, waves spawn only at the room's nodes, a clear opens the doors (LEVEL-FORMAT.md:97-103).
  - `SurvivalBuilder.Build` (Game/SurvivalBuilder.cs:24) builds the geometry; `LightKit29` / `PropsView29` / `Kit33Props` build props and
    lights; `BotNav.Build` (Game/Bot/BotNav.cs:124) builds the nav.
  - The file comes from `design.maps.list` (Game/SurvivalGame.Maps.cs:34-49), resolved under StreamingAssets in a Player.
  - **A map id that is not in maps.list falls back silently to the survival arena**, with only a warning (SurvivalGame.Maps.cs:66-70).
- **What the campaign plays now:** `n52-foundry, n52-iceline, n57-kiln, n52-span, n52-spire`.
  - This is the gate list (Tools/night/n64-gates.sh:15, n65-gates.sh:13). Order of levels 1-5 is from the unity-config chain.
  - **How the order is chosen:** `Curriculum25.Order` (Core/Curriculum25.cs:12-42) starts from `design.n30.levels.order`. Each night's
    level rows then swap ids in turn: N32 to N43 have their own functions; from N46 on it is the generic `NightRows.SwapLevels`
    (Core/NightRows.cs:269-278): `design.nNN.<row>.{levelFrom, levelTo, enabled}`.
  - The rows in force today: `design.n57.kiln57` ON (n52-kiln -> n57-kiln; Design/unity-config.json:9761) and `design.n58.span58` OFF
    (n52-span -> n58-span; :9771). `n40.cut40` drops level 6 (:5617).
  - `NightRows.BaseId` (NightRows.cs:283) follows the chain back, so older systems see n57-kiln as n45-kiln. n46flow prints
    "BaseId n45-kiln".
  - `-skygrindMap <id>` boots any id listed in maps.list, but **only** with `reel.sh` + `-skygrindReelOnly <scenario>` does it run as
    CAMPAIGN (L-062).

## 2. Rooms as data today (plans; numbers from Tools/night/n46-flow geometry in Logs/n64/flow-base.txt:56-62)

| room | footprint x height | pieces (boxes deck/hull/wall, ramps, pads) | enemies (waves) | spawn nodes |
|---|---|---|---|---|
| kiln kb2 THE CHARGING FLOOR | 20 x 28 m, h 14; floor 523 m2, 3 tiers | 4/5/1, 2 ramps, 2 pads, 1 health | 18 in 7+5+6: husk 6, swarmer 8, spitter 4 | 6 floor + 4 high |
| kiln kb3 THE PRESS HALL | 20 x 28 m, h 14; 518 m2, 4 tiers | 4/7/1, 2, 2 | 23 in 7+5+3+8: husk 13, spitter 4, charger 2, swarmer 4 | 6 + 2 |
| kiln kb3d THE CRACKED FLOOR | 24 x 20 m, h 12; 430 m2, 4 tiers | 5/5/1, 2, 1, 1 pit | 12 in 5+5+2: burrower, charger, husk, swarmer | 7 + 2 |
| foundry a5 CRUCIBLE | 20 x 32 m, h 10 | 4/7/1, 3, 3, 1 pit | 8 in 5+3: husk 5, spitter 3 | 6 + 5 |
| foundry b1 THE YARD | 20 x 32 m, h 12 | 4/6/2, 2, 2, 1 pit | 11 in 3+4+4 | 10 + 3 |

- Kiln's five fight rooms are 20-24 m by 20-30 m, 12-14 m tall, with about 11-16 pieces each. Their geometry rows all print `loop n`
  (no central walk loop), with sightlines of 30-34 m.
- The pilot's bot numbers for those five rooms: stall 15-26 %, clear 27-45 s, near 4.7-6.4 m.
- The room contract (below) keeps rooms at 20-32 m on purpose.
- **Climb routes:** the validator's row 4 ("enemies reach every tier ... walk and drop links only: a ramp, a stair") must PASS. It does
  for all five kiln rooms (Reports/LEVEL-n57-kiln-validate.md:12-16). Enemies have no climbing move, so a route up means a ramp or a stair.
- **Decor vocabulary beyond boxes exists, but the ON levels barely use it:**
  - `Kit33Props.Kinds` has about 45 view-only kinds with no collider (Game/Kit33Props.cs:46), for example:
    lamp, lampPost, pipes, truss, banner, cornice, pilaster, kilnArch, edgeLight, grate, debris, scorch, chain, lowWall, coverPillar,
    crateStack, hazardStripe, lavaChannel, hero.
  - `PropsView29` covers crate, pillar, console, press, floorStrip, cable, padMarker, frost and the pickups (PropsView29.cs:1-11).
  - A prop that blocks needs its own `boxes` row (`collider: true`).
  - The rows `n33.kit33`, `n29.props` and `n54.decor54` are ON. `n61.fixture61` (light mounts) is OFF.
  - **Measured prop counts:** n33 kiln (NIGHT33) 332, all kit dressing; n47 kiln 3 and n57 kiln (ON) 6, all health pickups; n52 span 4,
    spire 8, foundry 8 (4 health, 2 hero, 2 lavaChannel), iceline 18. The dressing was lost when the rooms were rebuilt; the kit to put
    it back is still in the code.
  - "Pieces with jobs" means the per-box `job` / `why` strings, for example "molecule: the small boost", "30 s problem: THE CHARGING
    DECK", "hero prop: the deck's tipping lip". n52-room-check.py item 2 and n56-room R8 check that every piece has them.

## 3. The judges: what they read, how long they take

| tool | command | reads | time |
|---|---|---|---|
| validator (levelgen) | `levelgen <plan> <out> --walk` | plan | not measured |
| **n46flow** (walk bot) | `dotnet run --project Tools/SimHarness -c Release -- n46flow --level <id> --seeds 10 [--rooms kb2] --out f` | `Design/levels/<id>.json` directly (Harness.N46.Flow.cs:18) | measured, see below |
| hold52 (camping probe) | `... hold52 --level <id> --room <r> --perches` / `--pads` | level JSON | part of the contract |
| **room contract** | `bash Tools/night/n59-l-contract.sh <px>-<level> [rooms]` | plan, flow, hold52, report | 123 s for one room |
| room checker + identity | `python -I Tools/night/n59-l-room.py <px>-<level> table` | plan + level JSON, no sim | 0.4 s |
| plan picture | `node Tools/night/level-svg.cjs <plan> <out.svg> --rooms kb2`; `bash Tools/night/n52-svg-png.sh <plan> <room> <out.png>` | plan | seconds |
| verbs37 (Player) | `-skygrindReelOnly verbs37` | the Player | 48-51 s per level |
| a157 (Player) | `<side>,<level>,a157[,-skygrindSpawnShots60/-skygrindClearShots60]` | the Player | 36 s per level |

- **Validator rows** (Reports/LEVEL-<id>-validate.md, LevelValidate24.cs): spots on the nav, routes, door nav, enemies reach every
  tier, pit recovery, wall routes, tunnels, doorFit, closedRoom. Exit 1 on an ERROR.
- **n46flow per room:** stall %, maxStall, air %, kill gap, near, dwell, dead, deaths / falls, clear k/n, plus geometry (floor m2,
  tiers, loop, pockets, sightline, wave 1 in view) and STYLE / TOKENS / IDLE lines.
  - Wall time for 10 seeds: n52-foundry 175 s, iceline 138, n57-kiln 105, span 70, spire 79 (Logs/n64/flow-base.txt).
  - `--rooms` measures one room.
  - It is deterministic: the noise band is seeds 1-10 against 11-20, not a re-run.
- **The room contract** (L-086) runs four n46flow runs (base and candidate x seed sets A and B), hold52 perches and pads, then
  `n52-room-check.py`, and prints C n/14. One room took 123 s with 6 jobs at JOBS=12 (Logs/n57/L/contract/table.md). The older
  versions are n52-contract-all.sh (n52 only) and n57-l-contract.sh (kiln only).
- **Room checker + identity:** R2 dead ends, R5 foothold, R8 every job, IN VIEW (opening held / vista / ambushed), I1 no mirror,
  I4 pads off walk lines, I5 one shape, I7 openings. It is n56-room.py plus n53-identity.py, pointed at any level. Measured: two kiln
  rooms in 0.4 s (`n57-l-room.py table kiln:kb2,kb3`).
- **verbs37 (Player):** docs/VERBS.md rows 9-41 on one level (FeelReel.N35.Verbs.cs:1-11). 4 min for all five
  (Logs/feel/n64-freeze/summary47.txt:1-211). Its staging picks the longest lane of at least 14 m after the first award room
  (FeelReel.N35.Verbs.cs:140-150), and its ride rows use a 25-30 m wall.
- **a157 (Player):** frames every fight room that has `frames52.entry`, with wave 1 live (FeelReel.N57.Level.cs:1-16). Spire's
  3 rooms took 36 s including boot (Logs/feel/n64-b1/summary47.txt:1-12).
- **Read level data directly, with no Editor and no Player:** levelgen, n46flow, hold52, the contract, n59-l-room / n56-room /
  n53-identity, level-svg. A new level can be judged sim-side in roughly 2-4 min per room.

## 4. What a level change does to the gates

- **Golden vectors and trajectory replays: no dependence.** CoreHarness reads `PortExport/{config, vectors, trajectories, maps}`
  (Tools/CoreHarness/Program.cs:5-6). There are 22 trajectories; the logs say "22/22 replays exact", although CLAUDE.md says 16.
  PortExport/maps holds only spire, survival and training.
- **Fingerprint: no dependence.** `fingerprint` runs `SurvivalSim.Headless(Config(), Map())` on `PortExport/maps/survival.json`
  (Tools/SimHarness/Program.cs:20, 107-139). A level row has no sim bit ("data, no bit", NightRows.cs:265).
  - So the fingerprint is not a guard of any campaign level at all.
- **The --flow gate: yes, it depends on level data.** `n6x-gates.sh --flow` runs n46flow on the five ON level ids with `--offNN`, and
  must equal `Logs/n6x/flow-base.txt` line for line (n64-gates.sh:10-11, 23-28).
  - An in-place edit of n57-kiln.json changes those lines ("re-make the base in its commit, say so").
  - A new file id behind a row does not: `--offNN` turns the row off, and the gate keeps loading the old id. The ids are hard-coded on
    line 13 of n65-gates.sh.
- **Before:** NIGHT52 rebuilt 18 rooms in new n52-* files behind rows (fingerprints plain / --off50 / --off52 EQUAL, golden 22/22;
  ledger/wave-52.md). NIGHT57's shared commit 2c8e3f6c put a byte-copy placeholder n57-kiln.json behind `kiln57` (off) and took the flow
  base with every n57 row off; every round passed `flow --off57 EQUAL` (NIGHT57-LOG.md:26-27, 34-35, 41, 71). NIGHT59 (L0) and NIGHT60
  (B13-B15) edited n57-kiln in place with no row and re-made the flow base (NIGHT60-LOG.md:78-80, 112, 156-157): no OFF side.

## 5. Shipping a rebuilt level behind a row (the NIGHT57 recipe)

**NIGHT57's shared commit (n57-shared.cjs, 22 files, 2c8e3f6c):**
- `design.n57.kiln57 {levelFrom: n52-kiln, levelTo: n57-kiln, enabled: false}` in **both** unity-config copies. The gate checks they
  are byte-identical.
- The maps.list row (Design/unity-config.json:394), added after the red team found it missing.
- A placeholder level file, a byte copy with its names changed, in Design/levels and StreamingAssets, with a `.meta`.
- `SurvivalSim.N57.cs` with `N57LevelFrom`, so kiln47's worm beats follow n57 -> n52 -> n47 (SurvivalSim.N47.Kiln.cs:50).
- Reel hooks; `N57SharedTests` (the row is off, the order is unchanged); n57-gates.sh; the A0 flow base.

**The builder then wrote:**
- the A-9 identity cards (docs/ROOMS.md "Level 3 as n57-kiln");
- `gen-n57-kiln.cjs` (560 lines): it reads n52's plan, keeps the start, the links and the boss room bit for bit, fills **slots** (each
  holds the old room until its new room passes), and has `--check` (with every slot old, the level equals n52-kiln apart from the names);
- `Design/levels/n57/{slots57, openings57}.json` and the checkers pointed at n57.

**ON at the freeze:** gate PASS (clear 92 -> 168 s, stall 25.5 -> 19.0 %, deaths 8 -> 2, verbs37 31/31). The old level stays in the
tree, and `-skygrindOff57` / `--off57` gives NIGHT56's game.

**For an n65 level the builder also has to carry over (a trap: PRINCIPLES rule 1, nothing lost):**
- The exit lamp, exit frames and exit63 are keyed to the literal `"n57-kiln"` (Exit58.N58V.cs:22, 112; ExitFrame61.N61V.cs:32;
  Exit63.N63V.cs:33; tests N60LampTests, N61ExitFrameTests, N63ExitTests). A new kiln id loses all three until those constants accept it.
- kiln47's beats need an `N65LevelFrom` hop or a generic BaseId; they key on room ids kb1 / kb2 / kb3 (SurvivalSim.N47.Kiln.cs:59-76).
- Shift46View keys on room `k5`.
- Two look rows key on map ids: `Look52TierLevels` and `Polish53IcelineLevels` (UnityTuning.U52.cs:150, UnityTuning.N53.cs:29).
- `data.name` must stay `n40-kiln` (gen-n57-kiln.cjs:37).

## 6. The fastest honest loop for one room

(a) **Sim-side, about 1-4 min, no Editor:**
```
node Tools/night/gen-n65-<level>.cjs && dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n65/<level>.plan.json Design/levels/n65/<level>.json --walk && node Tools/night/gen-n65-<level>.cjs --finish
python -I Tools/night/n59-l-room.py n65-<level> room <room>                         # R2/R5/R8/IN VIEW/I1-I7, under 1 s
dotnet run --project Tools/SimHarness -c Release -- n46flow --level n65-<level> --rooms <room> --seeds 10 --out Logs/n65/<room>.txt
bash Tools/night/n59-l-contract.sh n65-<level> <room>                               # C n/14, about 2 min
bash Tools/night/n52-svg-png.sh Design/levels/n65/<level>.plan.json <room> Logs/n65/<room>.png
```

(b) **Player frame:**
- The new level must be in maps.list and in the build's StreamingAssets.
- Build with `bash Tools/night/n41-build.sh <tag>` (live Editor). An incremental build took 38 s (NIGHT63-LOG.md:54).
- Then run `bash Tools/night/n65-reels.sh <tag> r1,n65-<level>,a157,-skygrindSpawnShots60`: about 36 s for the level, plus
  verbs37 at about 50 s.
- The row does not need to be ON for a reel: `-skygrindMap` picks the id. The level's systems (beats, exits) do key on the id.
- **Not tried, and possible:** a Player reads levels and config from `Builds/<tag>/SKYGRIND_Data/StreamingAssets/` at run time
  (n46-build-rows.cjs already edits that config copy without a rebuild; the b15 critic matched a build's n57-kiln.json blob to HEAD,
  Research/2026-10-21-wave60/gate-b15.md:43-44). Copying a new level JSON and its maps.list row into a build copy would give a room frame
  in under a minute, with no build, for data-only changes.

## 7. What can stall a room-rebuild night

1. **The contract bans small rooms.** n52-room-check.py:55 requires a long side of 20-32 m and a height of 10-16 m; :112 a ride wall at
   least 17 m long and 3 m tall; :92 exactly 3 standing planes; :60 at least 2.5 pieces per 100 m2. GG's "a lot smaller, ULTRAKILL"
   rooms will FAIL item 1 unless the forge re-bands the contract on purpose, and says so. verbs37 needs 25-30 m walls and a lane of at
   least 14 m somewhere in the level.
2. **The Editor never loads a campaign level** (L-044, L-062): level and view code is unproven until a Player reel boots the level. A
   bad prop row once broke a whole level view (L-043: 470888 NREs). Read the Player log for NREs (n65-reels.sh prints the count).
3. **Disk:** C: has **14 GB free now** (`df`). n65-reels refuses a set under 12 GB and skips a side under 10 GB without failing the
   batch; n41-build refuses under 10 GB. A reel set writes about 10 GB of frames (L-104): prune after every batch. Builds/ holds about
   20 builds at about 380 MB each (HANDOFF.md:44). Memory dips from other sessions (L-128, L-146; HANDOFF.md:41-42).
4. **The bot is the only judge sim-side:**
   - n46flow is one walk pilot with a lava rule. It moves the bot on when a link is not crossed, which happened 14 / 15 times on
     n57-kiln at first (ledger wave-57 SHORT; T-139).
   - The contract's dwell item cost builder L five cuts on kb3 (Research/2026-10-21-wave57/L.md:81-88).
   - L-087: the numbers miss pads on walk lines, dead-end rides and sameness. Keep a persona review.
5. **Level 4's precedent:** span58 (n58-span, THE GAP NEST) is still OFF. Its ride-wall and height items failed the contract
   (THREADS.md:50, T-144).
6. **Tracked side effects:** levelgen rewrites the tracked `Reports/LEVEL-<id>-validate.md` (NIGHT60-LOG.md:80); every level JSON is
   2.5-5 MB, written twice (Design + StreamingAssets); an unlisted map id silently boots the arena (SurvivalGame.Maps.cs:69).
7. **Tests pinned to ids:** 26 references to "n57-kiln" in Assets/Tests. MapChoiceTests hangs in the live Editor (T-147; never run it there).
