# THE LANTERN's hall: level 1's last room (NIGHT36 builder B's spec for builder L)

NIGHT36 card B1 (walkin36 + boss36). The fight happens in this room: no Respawn into a far arena. Builder L builds the approach and the
room row; builder B's code builds the ring itself (walls, floor, roof, the entry slab) as the boss's world layer, from level load.

## Coordinates
- Arena-local: `C` = the hall's centre on the floor (y = the level's floor height there). L picks `C` in the level and writes it in the
  room row (below). The player arrives from **+z** walking toward **-z** (the n35 foundry's direction): yawDeg 0 faces -z.
- Angles: `atan2(z, x)` in degrees, counter-clockwise from +x seen from above. +z is 90 deg.

## The ring (B builds it; L places NO solid inside the disc r <= 16 m around C, except the hall's last metre)
| part | value | why |
| --- | --- | --- |
| panels | 16 flat wall panels, apothem 15 m (30 m across), 5.97 m wide each, each collided and drawn as 4 flat facets (5.6 deg seams) | the n30/n32 ring GG asked for ("it needs to be a circle"); a hold ride laps it (ring32) |
| panel 0 | centred on +z (90 deg); panel i centred at 90 + 22.5 i deg | the entry is panel 0 |
| wall height | 14 m, closed roof at 14 m | the n30 ring's height; nothing to camp on top |
| ride band | the whole panel is rideable; a **cyan trim stripe** at y 2.0-2.3 m and a cyan lane edge at 6.0 m on every panel (view) | cyan = player-usable (rule 8); a jump from the floor (about 3.1 m) lands a ride inside the band |
| entry gap | panel 0, 4.0 m wide (arena-local x -2..+2), 4.5 m tall, open from the floor | the walk-in |
| entry slab | B's layer closes the gap 0.4 s after the player enters the room (lit red-orange while locked) | ULTRAKILL: doors lock red on entry |
| floor | a 16-gon disc at y 0 (B's layer); ALL of it can go lava (see zones) | the floor attack sends you to the walls |
| centre well | a 2.5 m radius grate disc at the centre, flush with the floor (view only: solid like the floor) | the boss rises out of it |
| platforms / pillars / crates | **none** | every solid has a job (PLAN 3.3); GG: "random pillars that serve no purpose" |
| light | one soft point light at (0, 9, 0), pale violet-white | as the n27 room |

## Lava cycle zones (the RED floor attack)
- Zone `floor`: every point with r <= 14.6 m (the whole floor, the well included). A rider (Mover state WallRide) or a player above
  y 1.2 m is safe. Tell 1.2 s (the floor glows red-orange from the well outward), then live 4 s, then off.
- Zone `slab`: the 1 m strip in front of the entry slab is part of `floor` (no safe spot by the door).

## The boss
- THE LANTERN hovers over the well at arena-local (0, 4.5, 0): **the core's centre is 4.5 m up**, level with a rider's eye (a ride
  runs at 2-6 m); from the floor you look up at its shielded underside.
- Body: a jointed cage about 3 m tall and 2.4 m wide, four arms reaching 4 m out (the silhouette is about 8 m wide with arms open).
- Orbs come from the core (4.5 m up) for the fan; the panel sweep comes from one arm's tip toward a panel.
- It never leaves r <= 3 m; the slam at 50 % drops it to (0, 1.5, 0) on the well for the stun.

## The approach (L builds it)
- The previous room's exit door, then a **short hall**: 4.0 m wide, 4.5 m tall, 8 m long, straight along -z, from arena-local
  z = +23 to z = +15 (the gap). Its walls are plain (no ride walls: you walk in). Dim; the gap frames the glowing well and the far
  panel's cyan trim (what the player sees on entry: the ring empty, the well lit).
- The hall is the door between the two rooms (its rect = the hall), so Campaign24's door lock closes it behind the player on entry.

## The room row (L writes it; B reads it)
```
{ "id": "<L's id>", "name": "1.<n> THE LANTERN", "kind": "boss", "boss": "hulk",
  "rect": [Cx-15, Cz-15, Cx+15, Cz+15], "height": 14, "roof": false,
  "spawn": { "at": [Cx, Cy, Cz+12], "yawDeg": 0 },
  "exit": [Cx, Cy, Cz],
  "lantern36": { "centre": [Cx, Cy, Cz], "entryYawDeg": 90, "_why": "builder B's ring is built here (Design/levels/n36/lantern-hall.spec.md)" } }
```
- `boss` stays `"hulk"` (the plan id of level 1's boss): with boss36 off the n35 LANTERN still fights (rule 1); with boss36 on B owns
  the begin through N36BossBegin.
- The room's zone (the trigger that enters it) = the disc r <= 12.5 m: the player is 2.5 m inside the gap when the room starts.
- `spawn` = where a retry puts you (inside the ring, facing the centre). `exit` = the level's end beacon after the kill (the well).
- No waves, no adds, no pickups in the room; a health pickup may sit in the hall (the last rest).

## The reveal (B's walkin36; no cut, the camera is the player's)
1. 0.0 s: the player crosses z = +12.5: the room starts; the slab closes over 0.4 s (red-orange); event `boss36Lock`.
2. 0.2 s: the well's grate glows; a beam of light rises from it (event `boss36Reveal`).
3. 0.5-3.0 s: THE LANTERN rises out of the well from y -4 to its 4.5 m hover; arms unfold; the core lights.
4. 1.0 s: the name card "THE LANTERN" (the BossKit bar's name).
5. 3.5 s: the fight starts (no damage before it).
