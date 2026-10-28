# A2: how levels are made (wave 32 audit, read only, HEAD c4c86280)

Everything here was read from the tree, with read-only node / grep / git log only. "Not checked" means not measured.

## 1. The campaign today

**Order.** `Core/Curriculum25.cs:16` reads `design.n30.levels.order` (`Design/unity-config.json:2211`) while
`design.n30.levels.enabled` is on (it is).

| slot | id | boss room -> boss | weapon (design.n31.unlocks) |
|---|---|---|---|
| 1 | `n30-foundry` | f12 `"hulk"` -> **THE LANTERN** (ring arena) | KESTREL only |
| 2 | `n30-iceline` | g1 `"glacier"` -> **THE STAG** (n31.stag31, Rink28Arena) | PULSE gauntlet (room i1a) |
| 3 | `n30-kiln` | k5 `"wyrm"` -> **THE KILN WYRM** (`Design/maps/wyrm-arena.json`, 60 m box square) | THUNDERCLAP |
| 4 | `n30-span` | s8 `"loom"` -> **THE LOOM** (in-level deck) | NAILGUN (pickup s1) |
| 5 | `level3` | none: the old 4-room plan, label "LEVEL 4 SPITTERS", not playable | - |

"Make level 5" means a new id (for example `n32-<name>`) inserted after `n30-span`.

**Flow.** RESULTS calls `Curriculum25.Next` (`:38`; `Game/SurvivalGame.Campaign25.cs:21`, `SurvivalGame.N25.cs:32` skips ids with no
map row). An `n32-` id passes the filter at `Curriculum25.cs:22-24`.

**Level select.**
- `design.n26.levelSelect.playable` is 3 (`unity-config.json:1582`). It is raised to `UnityTuning.LevelSel30SpanPlayable = 4`
  (`Game/UnityTuning.Hud30.cs:39`) by `Game/UiLevelSelect26.cs:67`.
- Slot names come from the `design.maps.list` label (`:337-353`).
- `sequential` is false, so no level locks behind another.
- The slot's weapon text comes from `Design/levels/unlocks.json`.

**Weapons.**
- `design.n31.unlocks.levels.rows` (about `unity-config.json:2590-2630`) is read by `Core/Unlocks26.cs:79 StartFor`: level n starts with
  `start.weapons` plus the `earns` of every row with a smaller n.
- **An id with no row returns null** and falls back to NIGHT30's rules.
- `unlocks.json:7` holds the span row. The old table gives level 5 the TORCH.

**Lessons.** Room `lessons` / `lessons31`. `Assets/Tests/EditMode/Game/Lesson25TextTests.cs:82-117` scans every top-level
`Design/levels/*.json`:
- at most 6 words per line (a `{placeholder}` counts as 1);
- only bound placeholders;
- no literal key or button names.

The NIGHT31 freeze failed on two 7-word lines (8c9b552c).

**Lighting and props are data**, not keyed by id. `CampaignLighting24.cs:43` reads `LevelDef.From(map.json)`; `PropsView29` reads
`campaign.props`. d67dc294 was a `pickup31` size boxed as float (470888 NREs): write plain numbers.

**Spawn rule.** `LevelValidate29.cs:84-135 Spawn31` applies only to `"n30": true` plans:
- every encounter node at least 6 m from the room's entry spawn;
- inside the entry's camera-wide view cone, at least `design.n24.campaign.minSpawnDist` (12 m).

## 2. The generator pipeline

```
Tools/night/gen-n30-<name>.cjs (require ./n30-lib.cjs)          -> Design/levels/n30/<name>.plan.json
dotnet run --project Tools/SimHarness -c Release -- levelgen <plan> <out>
   (Harness.N24.Level.cs -> Assets/Editor/Maps/LevelPlan24.cs + LevelPlan29.cs -> DrydockPlan.Build)
                                                                -> Design/levels/n30/<name>.json + Reports/LEVEL-<id>-validate.md
cp to Design/levels/n30-<name>.json (what the game reads); bash Tools/night/n24-sync.sh -> Assets/StreamingAssets/Design/... (+ cmp)
```
`Tools/night/n30-regen.sh [foundry|iceline|kiln|span]` runs the first three steps. The output paths are hard-coded
(`gen-n30-span.cjs:206`) and levelgen always writes `Reports/`, so I did not run them. A new StreamingAssets JSON needs its `.meta`
committed.

**Geometry model** (`Tools/night/n30-lib.cjs`, which is `n29-lib.cjs` plus `lava` and `grappleNode`). A level is **a straight line of
axis-aligned rectangular rooms running north (-z)**:
- `Level.room()` (`n29-lib.cjs:142-161`) puts each room 4 m north of the last one.
- It cuts the door exactly in that gap, centred on the previous room's `outX`.
- Local coordinates: `lx` is metres east of `cx`, `lz` is metres north of the room's south edge.

Primitives:
- `box(kind, lx0, lz0, lx1, lz1, y0, y1)`: an AABB. `deck` / `slab` can be walked on, `hull` is solid.
- `ramp(lx, lzLow, lzHigh, width, rise)`: along z only. `stairs`: 0.5 m steps.
- `lava(rect, depth)`: a pit with `lava: true`, its surface 0.6-1.0 m down. `pit`: a void that respawns.
- `grappleNode(lx, y, lz)`: a ring, gathered by `LevelPlan29.AddLava` into the top-level `grappleNodes`.
- `surface("ice", rect)`.
- `nodeAt` / `highNodeAt`: enemy spawns.
- `pickup`, `solid`, `decal`.
- Lights: `trim`, `endTrim`, `ceil`, `pool`.
- Proofs: `walk`, `walkSE`, `fall`, `jumpDash` (with `expectFail`). The real Mover walks them in the report.

There is no rail primitive and nothing curved or rotated. **Curves exist only as boss triangle soups** (`Lantern30Arena`,
`Rink28Arena`) in their own world layer.

**Rooms.** `kind` is `room` (waves), `link` (clears at `exit`) or `boss` (`row.boss`; entering it calls
`SurvivalSim.Boss25.cs:123 BossRoomBegin`). Doors are zones; entering a room locks the door behind it.

**Validators.**
- `LevelValidate24`: nav spots, the route, doors close the nav, the climb rule (every node and ledge top reached by walkers via ramp
  or stair), pit recovery, wall routes.
- `LevelValidate29`: lights flush and in palette, at least one light per room, a pool within 8 m of the exit, props, doorFit,
  closedRoom, lava, spawn31.
- `LevelValidate27` (the size budget) applies only to `n27.arenas` plans, not the n30 ones.

Grapple crossings are unproved. They carry `routeVia` plus a `grapple` note; `pull31` / `walk31` check them.

**Worked example** (between s4 and s5 in `gen-n30-span.cjs`; rooms are placed in call order):
```js
{ const r = L.room({ id: "s4b", name: "4.4b CROSSWIND", kind: "room", w: 16, l: 20, h: 10, cx: 0, outX: 0, par: 25, spawnLz: 2, exitFromN: 2.5 });
  r.lava(-8, 7, 8, 12, 0.8, "lava band"); r.grappleNode(0, 8.5, 9.5);
  r.box("deck", -8, 12, -2, 20, 0, 2.0, "NW deck"); r.ramp(-5, 14, 20, 3, 2.0, "walkers' ramp");   // climb rule
  [[4, 15], [-5, 17]].forEach(([lx, lz]) => r.nodeAt(lx, lz)); r.highNodeAt(-5, 16, 2.0);          // >= 6 m from spawn
  r.row.waves = [["husk", "husk", "husk", "swarmer", "swarmer"]];
  r.row.lessons = [{ kind: "husksDead", n: 5, prompt: "HOOK OVER THE LAVA.", promptPad: "HOOK OVER THE LAVA." }];
  r.ceil(0, 10, 2, 2, "white", 10, 1.0); r.pool(0, 17.5, 3, "cyan", 7, 1.8); }
// proofs block: { const r = R("s4b"); r.walkSE([[4, 0, 5], [4, 0, 17]]); r.walk([[-5, 0, 13], [-5, 2.0, 18]]); }
```
Then: `bash Tools/night/n30-regen.sh span` (0 ERROR), `spawn31 span`, `walk31 span`, `n24-sync.sh`, and a look in Editor Play.

## 3. Boss arenas

| level | source | shape |
|---|---|---|
| 1 Lantern | `Core/Enemies/Lantern30Arena.cs` with `design.n30.lantern30.arena` (`unity-config.json:2287`), at world [2400, 0, 0] (`design.n27.boss1.room.origin`) | **16 flat panels**, square floor and roof |
| 2 Stag | `Rink28Arena`, `design.n31.stag31.rink` | faceted stadium |
| 3 Wyrm | `wyrm-arena.json` boxes | square |
| 4 Loom | the level's s8 (an 18 x 18 deck in an 8 m lava ring) | square |

**Why arena 1 is not a circle.**
- It is a regular **16-gon**: `Lantern30Arena.Build()` (`:152-158`) lays one flat quad per panel. `panels: 16` is at
  `unity-config.json:2289`. Each panel is 5.97 m wide with a 22.5 degree seam (apothem 15, height 14).
- The reason, from the header (`:1-2`) and the config `_why`: the wall ride ends at a normal change over `cornerAngleDeg` 30
  (`PortExport/config.json:97`, `Mover.cs:1192,1501`).
- `seam30` measured 61 % of the speed kept at a 16-panel seam and 57 % at 12 panels (NIGHT30-LOG.md:33).
- The floor and roof are squares (`:150-151`, `big = rc + 4`); the walls hide them.
- The collider and the view use the same triangles, so they agree.

**Collider path.** `SurvivalSim.Boss27.cs:68 Lantern27Rebuild` -> `SurvivalSim.Boss30.cs:214 Lantern30RingFill` ->
`Lantern30Arena.FillLayer` (`:177`) -> `BoxLayer.SetTriangles` (`Core/GameWorld.cs:32`).
- `SetTriangles` makes one AABB entry per `ThreeOctree.Triangle`, on world layer `"lanternRoom"` (`Boss27.cs:115`).
- The layer is refilled on the platform sink (`SurvivalSim.Boss31.cs:174-182`).
- The harness path is `FillWorld` -> `TriangleWorld`.
- The player spawn is (0, 0, 12) from `Design/maps/lantern-arena.json`.

**Code that assumes a panel:**
- `Lantern30.cs:264-266`: the SWEEP (`sweepAheadPanels` 2, `:2255`).
- `Lantern30.cs:289`: sweep reach. `:312`: orb cull at `WallDistance < 0`.
- Platforms: one per panel, width `PanelWidth - 2 x inset` (`Lantern30Arena.cs:160-173`); `platformPanels [2, 7, 13]` (`:2301`);
  `OnPlatform` (`:108`).
- Vents: `ventPanels [0, 5, 10]` (`:2307`).
- `Game/Bot/LanternPilot30.cs:42-53`.
- Views: `Boss30View.cs:130-138` (lane strips), `:374-378` (sweep quad); `LavaView30.cs:385-393` (the lava disc as a 16-gon fan),
  `:425-432`; `Boss31View.cs:208-212`.
- `gen-n30-foundry.cjs:274-286` copies `PANELS = 16` into `f12.sink31.draw`, which only the SVG uses.
- Already radius-based: `Lantern27.cs:329-334` (`ringLimit30`).
- Square leftovers that do no harm: `Lantern27` `arenaHalf` 16 (telegraphs, cull); the hidden courtyard frame (`LanternView.cs:159`).
- No EditMode test pins the ring.

**Fix.** Setting `panels: 64` alone breaks the gameplay:
- platforms 0.27 m wide;
- a 1.47 m sweep;
- vent and platform indices pointing the wrong way;
- a 2-panel sweep lead of 2.8 m.

Instead, keep **16 logical sectors** (PanelAt, the sweep, the vents, the platforms, the pilot) and add **facets**:
1. A new Unity-side row, e.g. `design.n32.ring32.facetsPerPanel: 4` (64 facets), with a `_why` (rule 5).
2. `Build()`: facet quads per sector on radius `apothem / cos(pi / facets)`. Make a disc floor and roof, or keep the square collider
   floor and only draw a disc. Make the platforms arc slabs, and update `WallDistance`, `OnPlatform` and every `PanelWidth` user.
3. Views loop over facets; smooth radial normals on the wall mesh.
4. **Measure before shipping:** extend `seam30` (`Tools/SimHarness/Harness.N30.Boss.cs:50`, `new[] {16, 12}`) with 32, 48 and 64 over a
   full lap. Four times the seams could bleed more speed per lap: not checked. Rule 3 forbids touching `Mover.cs`.
5. The layer grows from about 40 to about 300 linear `BoxLayer` entries; bench it.

## 4. Level 4 today (`gen-n30-span.cjs`)

| room | kind | w x l x h m | content |
|---|---|---|---|
| s1 TOLLGATE | room | 12 x 18 x 7 | NAILGUN plinth + award, 4 swarmers |
| s2 FIRST HOOK | link | 10 x 22 x 10 | 4 m wall over lava, 1 node |
| s3 THE GAP | link | 12 x 33 x 12 | 15 m lava, 2 nodes, screen wall |
| s4 HOOK FIGHT | room | 18 x 26 x 12 | gantries at 1.2 / 3 / 4 m, 10 enemies |
| s5 REST | link | 6 x 10 x 4 | health |
| s6 CHAIN | room | 14 x 36 x 12 | lava, baffle, deck, 2 spitters |
| s7 ESCALATION | room | 20 x 28 x 12 | pylon in a moat, 4 nodes, 18 enemies |
| s8 THE LOOM | boss | 34 x 34 x 22 | deck in a lava ring |

**Boring, as data:**
- Every room has `cx` 0 / `outX` 0, so all 7 doors sit on one line.
- About 235 m, straight north.
- 3 of 8 rooms have no enemies; 34 enemies in all.
- Flat y 0 floors with 1-3 AABB decks.
- s3 and s6 are corridors (about 2.7 : 1).
- No ice; nothing above 4 m outside s8.

**Must keep:**
- The grapple teaching order: s2 tap-pull, s3 ring to ring, s6 pull -> wall ride (`lessons31`, `gapLz31`).
- The NAILGUN in s1: pickup, `row.pickup`, `award`, both unlock rows, `FeelReel.N31.Levels.cs:28`.
- s8: `boss: "loom"`, `loomLegs` (`SurvivalSim.Boss30.cs:92-102`), `deckBreak31` (`SurvivalSim.Boss31.cs:73-82`).
- Lava only, no void.
- Room ids used by pilots and reels (`Chain31Pilot`, walk30 / walk31, `n31-reels.sh:22-29`).

## 5. Risks

1. **Level lists hard-coded in tools and reels:**
   - `Harness.N31.Levels.cs:131`, `Harness.N31.Weapons.cs:36`, `Harness.N30.Systems.cs:805`;
   - `FeelReel.N31.Levels.cs:3-28`, `n31-reels.sh:19-29`.

   `LevelSelect29Check.cs:57` presses `presses % playable` (still 20 presses), but a level that throws at load fails it.
2. **Tests that pin data:**
   - `Lesson25TextTests` (6 words);
   - `Lesson25Tests.cs:106` (`Next("n30-kiln") == "n30-span"`, safe if level 5 follows span);
   - `Systems29Tests.cs:149-159` (`unlocks.json`);
   - `Campaign24TapeTests` (the bytes of `layer1.json`: never regenerate it).

   There is no level hash. The gates are the sim fingerprint (`SimFingerprint.cs`, seed 1, 7200 steps) EQUAL plus `--offNN` EQUAL, so
   every new row ships off in the shared commit.
3. **How regeneration went:**
   - NIGHT30 made new ids (`n30-*`) behind a switch.
   - NIGHT31 edited the generators in place and gated each round on: levelgen 0, spawn31 0, walk30 / walk31 over 10 seeds, dn-check,
     fingerprint EQUAL, a Player reel.

   The freeze still caught a 7-word line and a prop boxing bug. Run `Lesson25TextTests` and one Player level load after every regen.

## Checklist: add level 5 (`n32-<name>`)
- [ ] `gen-n32-<name>.cjs` on `n30-lib.cjs` (plan `"n30": true`), plus a regen script; levelgen 0 ERROR -> `Design/levels/n32-<name>.json`.
- [ ] `design.maps.list` (~:337): `{ "id": "n32-<name>", "label": "LEVEL 5 ...", "file": "Design/levels/n32-<name>.json", "campaignOnly": true }`.
- [ ] `unity-config.json:2211`: insert after `"n30-span"`, behind an off-able n32 row.
- [ ] Playable 4 -> 5: `UnityTuning.Hud30.cs:39` / `UiLevelSelect26.cs:67`.
- [ ] `design.n31.unlocks.levels.rows`: `{ "n": 5, "id": "n32-<name>", "earns": "torch" }`; `unlocks.json` row `n: 5`.
- [ ] Boss room id handled by `BossRoomBegin` / `BossRoomSystem` (`SurvivalSim.Boss25.cs:108,123`). A new boss is GG's design (rule 7).
- [ ] Lessons of 6 words or fewer; plain-number props; lights and an exit pool in every room.
- [ ] `n24-sync.sh`, then commit the JSON and its `.meta`.
- [ ] Tool and reel lists (risk 1).
- [ ] Gates: `Lesson25TextTests`, fingerprint + off EQUAL, Editor Play 0 errors, LevelSelect29 20 / 20, DoorFree29.

## Checklist: make boss arena 1 a circle
- [ ] `seam30` at 32 / 48 / 64 facets over a full lap; choose by measurement.
- [ ] The new row `facetsPerPanel` (keep `panels` 16 as sectors).
- [ ] `Lantern30Arena.Build`: facet walls, disc floor and roof, arc platforms; `WallDistance` / `OnPlatform` / `PanelWidth` users.
- [ ] Views: `Boss30View.cs:130-138,374-378`, `LavaView30.cs:385-393,425-432`, `Boss31View.cs:208-212`; smooth normals.
- [ ] Recheck `Lantern30.cs:264-312` and `LanternPilot30.cs:42-53`; update the `gen-n30-foundry.cjs:274-286` draw.
- [ ] Gates: `camp31 lantern`, `boss30 lantern`, Player lantern reels, a frame with no visible facets, a bench.
