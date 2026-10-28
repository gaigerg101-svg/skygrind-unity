# Level plans (NIGHT24, builder L): the CAMPAIGN's one source file per layer

A level is ONE plan file, `Design/levels/<id>.plan.json`. A re-runnable, headless command turns it into the map JSON the game
loads (`Design/levels/<id>.json`, the schema `SurvivalMap.FromExport` reads, plus a `campaign` block), on the C# collider like every
map, and writes the level report. Nothing is hand-edited downstream.

```
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/<id>.plan.json Design/levels/<id>.json [--walk] [--seed n]
   -> Design/levels/<id>.json  and  Reports/LEVEL-<id>-validate.md  (exit 1 on a plan problem or a report ERROR)
dotnet run --project Tools/SimHarness -c Release -- campaign Design/levels/<id>.plan.json [--seed n] [--rooms k]
   -> a bot plays the level door to door in the real sim (times against par; the boss as builder B's bot plays it)
campaign <plan> --seeds 10 --rooms 3      per-room clear rate (first try, no death) and clear-time medians
campaign --tape                           a campaign tape (format 8) replays EQUAL (rooms 1-3+)
campaign --shaft [<plan>] [--room kb]     the wall-jump line measured with the real Mover
campaign --selftest                       Campaign24Tests outside Unity
```

The conversion is `Assets/Editor/Maps/LevelPlan24.cs` (pure, no UnityEngine; it lives in the Editor assembly because it uses `DrydockPlan`): it
builds a DRYDOCK-style plan and calls `DrydockPlan.Build` (`Assets/Editor/Maps/Build_drydock.cs`), so boxes, ramps, pads, grapple nodes,
the nav grid, the zone parts and the plan checks (`DrydockPlan.Check`: every spot stands on a surface, every capsule is clear) are the code
the DRYDOCK map already uses. The report is `Assets/Scripts/Game/MapAuthoring/LevelValidate24.cs`.

Web coordinates, metres: x east, y up, **z south** (north is -z; a yaw of 0 faces north, +yaw turns left/west). A rect is `[x0, z0, x1, z1]`.

## Top level

| key | meaning |
|---|---|
| `id`, `name` | the level; `id` is the file stem |
| `floorSize` | the map's nominal size (spawn nodes are sampled inside `floorSize / 2 - 2`); the level must fit. The JSON's size follows the WALKABLE area (nav grid), not `floorSize`: one floor slab per room, not one over the bounding box (layer 1: 2.6 MB at 256) |
| `killPlaneY` | falling below it is death (default -30) |
| `wallHeight` | the perimeter walls' height (default 14); a barrier stands on every wall up to y 40 |
| `rooms` | the areas, in play order (below) |
| `doors` | the doorways between areas (below) |
| `boxes`, `ramps`, `pads`, `grapple` | DRYDOCK's rows exactly (zone-tagged boxes `{zone, kind, min, max}`, ramps `{zone, from, to, width, rise}`, pads `{zone, at, target, apexAbove}`, grapple nodes `{zone, at}`); kinds `deck` / `slab` are walkable or rideable tops, `hull` is solid cover |
| `pits` | PLATFORMING stretches (below) |
| `tunnels` | SLIDE TUNNEL kits (below) |
| `layer` / `rank` / `weapons` / `links` | the layer's own rows (copied into `campaign`) |

## Rooms (`rooms[]`)

An area is a **zone** at runtime (`Zones`: a door is a zone of its own; a locked zone is a solid force field in the collider).
Its walls are made for you: a ring 2 m thick outside `rect`, with a gap wherever a door rect cuts it. Zone boxes start at y 0: anything below
the floor (a pit) is in no zone.

| key | meaning |
|---|---|
| `id`, `name` | the zone id (`r1`, ...) and its card name |
| `kind` | `room` (a combat room), `link` (a traversal hall or shaft: no encounter; cleared when the player reaches `exit` within 4 m), `boss` (the room's fight is THE KILN WYRM in its own room: entering it takes the player there; the room clears when the worm dies) |
| `rect` | `[x0, z0, x1, z1]`, the floor plan (ground tier); tiers are `boxes` / `ramps` with `zone` = this id |
| `par` | the room's par time in seconds: the MEDIAN of the headless bot's clear times over 10 seeds (x 1.0); the bot must clear within 4x it |
| `spawn` | `{at: [x, y, z], yawDeg}` where the player stands on entering and after a death (the first room's is the run's start) |
| `exit` | `[x, y, z]` the point the player walks to to leave (the bot's goal; a link clears there). It must stand on a grid node (not inside a ramp's footprint) |
| `nodes`, `highNodes` | room-local spawn nodes `[x, y, z]` (ground / on a tier): enemies spawn ONLY here, never in another room; each must be a standing point that enemy walkers reach from the floor |
| `waves` | the encounter script: a list of waves, each a list of enemy kinds (`husk`, `swarmer`, `spitter`, ...). The next wave starts `design.n24.campaign.waveGap` s after the one before is dead; the room clears when the last is dead |
| `limit` | optional LIMIT flags for this room only: `"pistolOnly"`, `"noDash"` (shown as a banner, restored at the door, paid with a style bonus) |
| `award` | optional weapon id given at the room's end (`weaponEarned`) |
| `wallRoute` | optional: the room has a wall-jump line; the report times it against the stepped route (the faster must be the wall line) |

## Doors (`doors[]`)

A door is a thin zone in the wall gap between two areas: locked it is solid, open it is free.

| key | meaning |
|---|---|
| `id`, `between` | `d1`, `["r1", "r2"]` |
| `rect` | `[x0, z0, x1, z1]` covering the gap from one area's rect edge to the next; it must lie inside BOTH areas' extent along the wall (a door sticking out past a room cuts a hole in the wall) |
| `checkpoint` | true: entering the next area sets the checkpoint (every door in layer 1) |

## Pits (`pits[]`): the platforming stretch

`{zone, rect, floorY}`: the room's floor is cut by `rect`; the pieces around it are solid down to `floorY - 1`, and the pit has its own floor slab with
its top at `floorY` (-3.5 in layer 1). Put hop platforms (`deck` boxes from below the pit floor up to 0), pads that cross them, and a **recovery
stair** (decks stepping up from the pit floor to the hall floor, each step <= 1.3 m) in the plan's `boxes`. The report proves every pit-floor cell can
get back to the room's spawn (jumpers' links); a pit without a way out is an ERROR (a soft-lock).

**LAVA (NIGHT30, builder L; the sim is `design.n30.lava`, builder S; the surface view is builder V's):** a pit row with `"lava": true` is a lava trench:
`{zone, rect, floorY, lava: true}` where `floorY` is the lava SURFACE, 0.6-1.0 m below the floor (a player knocked up by a touch clears the rim). Its room
carries NO `respawnY` (lava burns and knocks the player up, it never teleports; only a VOID, a pit without `lava`, respawns). levelgen
(`LevelPlan29.AddLava`) writes a top-level array into the level JSON, one entry per lava pit: `"lava": [{ "rect": [x0, z0, x1, z1], "y": surfaceY,
"room": "f2" }]` (the rect in world x / z, min then max, as the pit's); the `campaign.pits` rows keep their `lava: true` too. No lava pit: no `lava` key
(older maps are byte-identical). The report's `lava` row checks the depth band, no `respawnY` in the room, the array entry, and the collider at the surface.

## Tunnels (`tunnels[]`): the slide tunnel kit

`{zone, rect, ceiling}`: a ceiling slab over `rect` from `ceiling` up 2 m; the tunnel's side walls are ordinary boxes in the plan. With
`ceiling` 1.2 m: the player must slide (slide collider 1.0 m), a swarmer (0.75 m) walks through, a husk (1.8 m) goes round. The report runs the
real Mover through it sliding (passes) and running (stopped).

## The report (`Reports/LEVEL-<id>-validate.md`), every row PASS or ERROR

1. spots on the nav grid (spawns, exits, encounter nodes) 2. the player's route through every area (RIVAL's graph; the time of each leg)
3. doors close the nav (shut: nothing behind is reachable from the room's exit; open: the area behind is) 4. **enemies reach every tier** (every
encounter node and every raised ledge top is reached from the room's floor spawn by walkers: walk and drop links only: a ramp, a stair) 5. pit
recovery 6. routes (wall-jump line vs the stepped route) 7. slide tunnels.

## Runtime (campaign-core)

Start: every zone locked but room 1. Entering a room (the player's feet in its zone): the door behind locks (`Zones.Lock`), the room's waves
spawn at its nodes from the run's seed. Cleared: the exit door and the next area open free (`Zones.Open(id, free: true)`). Death: the room
restarts within `design.n24.campaign.retryDelay` s of game time, the same seed. The run state (level, room, checkpoint, limit flags) is the
tape's `Campaign24State`; a campaign tape replays EQUAL. TITLE's MODE row lists CAMPAIGN first; the level is `campaignOnly` in
`design.maps.list` (never on the MAP row; `-skygrindMap <id>` boots it).
