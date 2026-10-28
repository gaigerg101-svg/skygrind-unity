# FORMAT-REQUESTS.md (builder C, levels 6-10)

R1. **Boss rooms other than the WYRM.** `Campaign24`'s `kind: "boss"` room calls `world.BeginBoss`, which starts only THE KILN WYRM (`SurvivalSim.Wyrm24`).
Levels 6 (BULWARK, `Core/Enemies/Bulwark.cs`), 9 (MONOLITH, `Core/Enemies/Monolith.cs`, `SurvivalSim.Monolith24.cs`) and 10 (the final boss) therefore ship their
boss room as a plain combat gauntlet (`kind: "room"`) with a `bossMarker {boss, status: "SHORT", why}` key at the plan's top level and a `_note` on the room (levelgen
ignores both keys). Needed to close it: a `boss` field on a `kind: "boss"` room (`"boss": "bulwark" | "monolith"`) and a `BeginBoss(kind, out spawn, out yaw)` / `BossDefeated`
per kind in `SurvivalSim.Campaign24.cs` (L's) with the start call in `SurvivalSim.cs` (the lead's).

R2. **The WARDEN is the shield kind** (`config.warden`: a front shield that absorbs frontal hits; flank, headshot-from-above or a punch breaks it), not an area-denial
kind. CURRICULUM.md's level 7 "area denial" has no enemy kind to carry it; level 7 uses wardens in a room with lane walls (they herd you) as the nearest honest version.

R3. **Fast routes for level 8 (SPEED).** The plan rows exist (`wallRoute`, `pads`, `grapple`) but each needs a hand-measured line per room; level 8's first cut has the stepped
route only (its rooms are the common template). SHORT: no wall / pad / grapple route and no "fast route beats stepped route" report row for level 8.

R4. **Par and the bot.** Pars are the headless bot's median over 10 seeds (`Tools/night/levels610-pars.py`). The bot is slow against shields and with fists only
(level 6 room 2 197 s, level 7 room 3 meleeOnly 409 s): a human is faster; the rank thresholds (S within par) will read generously there. Needs: a better shield-break
rule and a fist rule in the autopilot (AutopilotGuns), not a plan change.

K33 (NIGHT33 builder K, DONE in the format, no lead line needed). **Decor prop rows.** A `props` row may carry `decor: true` (and `dress33: true` when
Tools/night/dress33.cjs wrote it). A decor row is view only: `collider` must be false; it may stand up to `UnityTuning.Kit33DecorOutside` (6 m) outside
its room's rect (the wall ring, a chimney band) and is exempt from LevelValidate29's socket-depth and mount checks; its own rules are the harness's
`kit33 <plan|level>`. New prop kinds drawn by `Kit33Props.cs` (PropsView29 hands them over): skirt, cornice, pilaster, rib, doorFrame, lintelLight, truss,
pipes, chain, lamp, wallPipe, recess, wash, inlay, grate, hazardStripe, chimney, banner, kilnArch, ingots, lavaChannel, stalactite, crystal, drift,
pylon, span, lampPost, halo, fin, hero, debris, scorch, edgeLight, crateStack, lowWall, coverPillar. No `color` key on decor (the level's theme tones,
from its id: foundry / kiln / ice / span / spire). Cover kinds (crateStack, lowWall, coverPillar) are ordinary collider rows (no `decor`).

R5. **Maps list rows (the lead's config) for levels 6-10:** add to `design.maps.list`
`{ "id": "level06", "label": "THE BULWARK", "file": "Design/levels/level06.json", "campaignOnly": true }`, then `level07` WARDENS, `level08` SPEED, `level09` THE MONOLITH,
`level10` FINALE (same shape), and the order `layer1, level03.. level05 (L), level06 .. level10` in `design.n25.curriculum`.
