# Design/maps: Unity-authored SURVIVAL maps (NIGHT15)

A map here is new design (GG, 2026-10-05: "a map overhaul"), so it lives on the Unity side (CLAUDE.md rule 5), never in
`PortExport/` (that folder is the web's export and the tests' oracle).

- **One file per map:** `Design/maps/<id>.json`, in the schema `SurvivalMap.FromExport` reads (the web exporter's:
  `name`, `floorSize`, `killPlaneY`, `collision`, `core`, `spawns`, `pads`, `grappleNodes`, `zoneParts`, `navGrid`,
  `data`), plus:
  - `_why`: the map's design reason (its concept in MAP-DESIGN.md);
  - `spots`: the map's own spots for the Unity-only systems (`box`, `power`, `exfil`), which replace `design.box` /
    `design.power` / `design.exfil`'s spots for this map. The web's `survival.json` has none and keeps the design
    block's.
- **Made by** `SKYGRIND/Map/Export` from a scene of markers (`Assets/Scripts/Game/MapAuthoring`), or by a re-runnable
  build script (`Assets/Editor/Maps/Build_<id>.cs`). `navGrid` and `zoneParts` come from the C# ports of the web's
  `createNavGrid` and `resolveZones` (`Core/Nav/NavGridBuilder.cs`, `Core/ZoneMath.cs`).
- **Checked by** `SKYGRIND/Map/Validate` (`Reports/MAP-<id>-validate.md`).
- **The Player's copy:** `Assets/StreamingAssets/Design/maps/<id>.json`, byte-identical (`SKYGRIND/Sync Port Data`
  copies it); the run setup's MAP row lists the maps in `design.maps.list` (Design/unity-config.json).
