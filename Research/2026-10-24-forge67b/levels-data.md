# Levels data: the measured state of every campaign room (FORGE67b data scout, 2026-10-24)

GG, verbatim (2026-10-24): "area deisgn need t be better in alot of levels". This file answers one question: what do the instruments say
about every fight room of the five shipped levels today, and which rooms are weakest? I only read and ran tools. I wrote nothing in the
tree except this file. Every number names its source key (below). "Not checked" means not measured.

**Source keys** (run 2026-10-24 ~07:50-08:40 at HEAD eaa4f5f4, unless the key says otherwise; `<dll>` = `Temp/SimHarness/bin/Release/net10.0/SimHarness.dll`, built 07:18; one sim job at a time, no Editor, no build):
- **ORD**: `dotnet <dll> order32`, which prints the campaign order (Core/Curriculum25.Order).
- **FB**: `Logs/n66/flow-base.txt`, the walk bot. That is n46flow seeds 1-10, Walk38Pilot plus the ported autopilot, written at NIGHT66 t0 (2026-10-23 22:31).
  - It is still current. The level files last changed on 2026-10-21 (levels 1, 2, 4, 5: NIGHT56) and 2026-10-22 (level 3: NIGHT60, `git log -1`).
  - The only commit to `Assets/Scripts`, `PortExport` or `Design/unity-config.json` since then is 0a9d038e (the tour65 view rule). NIGHT66's close gate also read "flow --off66 EQUAL" to this base.
- **RC**: the room check. `python -I Tools/night/n59-l-room.py <level> table` and `... room <id>` (n56-room R2 / R5 / R8 / IN VIEW plus n53-identity I1-I4).
- **CT**: the NIGHT52 room contract. `python -I Tools/night/n52-room-check.py <plan> <room> --report Reports/LEVEL-<level>-validate.md` (`--grapple` on span and spire). The contract text exempts teach rooms.
- **PS**: plan statistics. My read-only script counts each plan's rows by `zone` (boxes, ramps, pads, pits, tunnels) and by `room` (lights, props). It also reads each room row's `waves`, `grappleNodes`, `verb65` and `line65`.
- **AIR**: `node Tools/night/n65-air.cjs <plan> <room>`. The figures are air hand-offs / the longest floor-free chain / dead ends.
- **TOUR**: `dotnet <dll> n65tour --level <level>`.
- **V37**: `Logs/feel/n66-freeze/summary47.txt` (the verbs37 Player reel per level, NIGHT66 freeze).
- **N65**: today's measurement of level 1's rebuilt room files (the carry-over 973b9c16; row OFF). Section 6 gives the commands.
- **SKIP**: `dotnet <dll> n65verb skip --level Design/levels/n57-kiln.json --room kb3c --plant-verb65 kb3c:ride --seeds 5 --bar 1.25`.

The raw outputs of today's runs are in this session's scratchpad (`roomtable.txt`, `roomdetail.txt`, `contract.txt`, `air.txt`, `tour.txt`,
`n65-*.txt`, `skip-kb3c.txt`). That folder does not last. The sim is deterministic, so each command above prints the same numbers again.

## 1. The campaign

ORD: `n52-foundry, n52-iceline, n57-kiln, n52-span, n52-spire` (5 levels). No finale. `design.n38.finale38` and `n41.finale41` are OFF.

| # | level (the shipped file) | how it ships (Design/unity-config.json) | rooms: fight / link / boss | last change |
|---|---|---|---|---|
| 1 | THE FOUNDRY `Design/levels/n52-foundry.json` (plan `Design/levels/n52/foundry.plan.json`) | n52.foundry52 ON; **n65.foundry65 OFF** (the rebuild, every slot "old": `Design/levels/n65/slots65.json`) | 11: 6 (a2 a3 a4 a5 b1 a8) / 4 (a1, b2 THE GANTRY 20 x 104 m, a6, a7) / a9 THE STACK | 2026-10-21 884d7028 |
| 2 | THE ICE LINE `n52-iceline.json` (plan `n52/iceline.plan.json`) | n52.iceline52 ON | 10: 8 (g1-g8) / 1 (gr) / x9 THE BRAWL | 2026-10-21 9aedb84d |
| 3 | THE KILN `n57-kiln.json` (plan `n57/kiln.plan.json`) | n57.kiln57 ON (the NIGHT57 rebuild) | 10: 5 (kb2 kb2c kb3 kb3c kb3d) / 4 (kb1 kb1b kb2b kb4) / k5 THE RING | 2026-10-22 8419921a |
| 4 | THE SPAN `n52-span.json` (plan `n52/span.plan.json`) | n52.span52 ON; n58.span58 OFF (n58-span: its NEW list is empty, its rooms are n52's; RC reads identically) | 8: 3 (p1 p3 p5) / 4 (p2 36 x 50 m, p4 40 x 102 m, o1, o2) / o3 THE NARROWS | 2026-10-21 6ffe9046 |
| 5 | THE SPIRE `n52-spire.json` (plan `n52/spire.plan.json`) | n52.spire52 ON | 11: 3 (v2 v5 v6) / 7 (v1 v3 v4 v7 v8 v9 v10) / v11 THE SPIRE | 2026-10-21 9aedb84d |

That is 25 fight rooms in the shipped campaign. Levels 4 and 5 have 3 fight rooms each. Their other rooms are traversal links: grapple gates,
jump-dash gates, falling stones and the ring.

## 2. Per level, per room

Reading the columns:
- **planes**: the contract's standing planes (floor plus each cluster of walkable tops 1 m or higher), from CT.
- **solid**: boxes (d = deck, h = hull, w = wall), ramps, pads and solid props in the room's zone (PS).
- **dress**: colliderless props other than pickups (PS).
- **ride**: walls with `ride: true` and their longest run (PS). Every fight room's ride wall also has a `ride36` face, which the verb pilot needs. Span and spire's 5 face-less rides are in links, the boss rooms and v5's short second wall.
- **waves**: the enemy total and the kinds per wave (PS).
- **v65**: whether the room row has `verb65` and `line65`. None of the 25 shipped rooms has them. Only level 1's rebuilt room files do (section 6).

Walk bot columns (FB, 10 seeds):
- stall %: in-fight steps under 2 m/s
- air %
- gap: the longest kill gap, in s
- deaths and falls: totals over the 10 seeds
- clear: the median clear time, with the par in brackets
- DULL %: the game's style meter at DULL, with the median top rank

RC is the room check's verdict. I1 is the mirror overlap (under 60 % passes). I2 is ride length against the walk (1.3 or more passes). CT is
the contract's computed items. AIR is hand-offs / chain / dead ends.

### Level 1 THE FOUNDRY (n52-foundry, shipped)
| room (job) | m (m2), h | solid | dress / lights | planes | ride | waves | stall / air | gap | deaths / falls | clear (par) | DULL / top | RC | I1 / I2 | CT | AIR |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| a2 KESTREL (teach) | 14x20 (280), 7 | 1d, 1 ramp | 0 / 3 | 2 (0, 2.0) | none | 2 husk (+ stand-ins) | 12.4 / 73.9 | 9.8 | 0 / 0 | 14.7 (18) | 21.1 / BOLD | FAIL R5 | 0 % / n/c | 3/11 (teach) | 0 / 1 / 1 |
| a3 RIDE (practise) | 20x26 (520), 10 | 6d 5h 1w, 2 ramps, 2 pads; lava 1 | 0 / 8 | 3 (0, 2.5, 5.0) | 1 (18) | 8: 2 husk+2 swarmer / 3 swarmer+husk | 38.2 / 51.1 | 25.1 | 4 / 0 | 28.1 (22) | 21.0 / AIRBORNE | PASS | **69 % FAIL** / 1.64 | 11/11 | 45 / 9 / 0 |
| a4 PARRY (teach) | 8x19 (152), 6 | 1d, 2 ramps | 0 / 3 | 2 (0, 1.5) | none | 3: spitter / 2 spitter | 33.7 / 57.6 | 11.7 | 0 / 0 | 13.8 (16) | 43.5 / COOL | FAIL R5 | **100 % FAIL** / n/c | 3/11 (teach) | 0 / 1 / 1 |
| a5 CRUCIBLE (combine) | 20x32 (640), 10 | 4d 7h 1w, 3 ramps, 3 pads; lava 1 | 2 / 8 | 3 (0, 2.5, 5.0) | 1 (18) | 8: spitter+4 husk / 2 spitter+husk | 28.1 / 53.7 | 16.4 | **6** / 0 | 30.5 (45) | **71.5** / COOL | FAIL R5 | 57 % / 2.14 | 11/11 | 18 / 7 / 0 |
| b1 THE YARD (combine) | 20x32 (640), 12 | 4d 6h 2w, 2 ramps, 2 pads; lava 1 | 2 / 6 | 3 (0, 2.5, 4.5) | 2 (32) | 11: 3 husk / 2 spitter+2 husk / 4 husk | 28.8 / 57.3 | 11.8 | 3 / 0 | 34.2 (45) | 53.0 / BOLD | FAIL R5 | 36 % / inf | 11/11 | 9 / 5 / 0 |
| a8 PIERCE (teach) | 14x22 (308), 9 | 4d 2w | 0 / 5 | **1 (floor only)** | 2 (7) | 3 husk | **45.6** / 33.6 | 5.6 | 0 / 0 | 5.4 (24) | 19.6 / BOLD | FAIL R2, IN VIEW | 0 % / **1.14 FAIL** | 3/11 (teach) | 2 / 2 / 0 |

Level (FB): stall 30.8 %, air 55.4 %, DULL 43.7 %, deaths 13, falls 0, clear sum 126.7 s, level time median 242.1 s. RC: 1 of 6 rooms
passes (a3). TOUR: GREEN. V37: PASS 27 / 27.

### Level 2 THE ICE LINE (n52-iceline)
| room (job) | m (m2), h | solid | dress / lights | planes | ride | waves | stall / air | gap | deaths / falls | clear (par) | DULL / top | RC | I1 / I2 | CT | AIR |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| g1 TANK ALONE (teach) | 22x26 (572), 9 | 2w | 0 / 6 | **1** | 2 (13) | **2: tank / charger** | 43.7 / 23.9 | **66.3** | 0 / 0 | 18.7 (12) | **100.0** / DULL | PASS | n/c / **1.08 FAIL** | 3/10 (teach) | 0 / 1 / 2 of 2 |
| g2 THE SHIELD (teach) | 21x28 (588), 10 | 3d 3h 1w, 3 ramps, 4 pads, 1 pillar | 0 / 5 | 3 (0, 1.5, 4.0) | 1 (18) | 7: 2 warden+2 husk / charger+2 husk | 43.3 / 13.8 | 38.5 | 0 / 0 | 14.7 (30) | 60.9 / COOL | PASS | 31 % / 1.40 | 11/11 | 17 / 6 / 0 |
| g3 THE SWARM (teach) | 24x28 (672), 10 | 7d 5h 1w, 2 ramps, 2 pads | 1 / 7 | 3 (0, 2.0, 4.0) | 1 (18) | 11: 3 husk / 8 swarmer | **67.2** / **0.9** | 4.0 | 0 / 0 | 16.8 (25) | 74.7 / COOL | FAIL R5 | 21 % / **1.28 FAIL** | 11/11 | 22 / 7 / 0 |
| g4 THE REACH (twist) | 24x30 (720), 10 | 7d 6h 1w, 2 ramps, 3 pads; ice canal | 0 / 7 | 3 (0, 2.0, 4.0) | 1 (24) | **3: drone / 2 drone** | 27.4 / 46.0 | 35.8 | 0 / 0 | 39.3 (30) | **88.4** / DULL | FAIL R5 | 59 % / **1.18 FAIL** | 10/11 (types: drone only) | 56 / 10 / 0 |
| g5 THE COOLDOWN (twist) | 21x30 (630), 12 | 2d 5h 4w, 2 ramps, 3 pads | 0 / 6 | 3 (0, 2.0, 4.5) | 4 (18) | 15: 5 swarmer / 6 swarmer+charger / warden+2 drone | 41.5 / 28.4 | **62.7** | 0 / 0 | 36.8 (30) | 74.8 / BOLD | FAIL R5 | 50 % / PASS | 11/11 | 49 / 12 / 0 |
| g6 THE ARMOURY (reward) | 14x18 (252), 6 | 1 plinth | 0 / 3 | **1** | none | 3: 2 husk / husk | 42.2 / 45.7 | 4.2 | 0 / 0 | 10.2 (8) | 22.2 / BOLD | FAIL R5 | n/c / n/c | 3/11 | 0 / 0 / 0 |
| g7 THE MIX (combine) | 24x32 (768), 10 | 6d 7h 2w, 4 ramps, 2 pads; lava 3 | 1 / 6 | 3 (0, 2.0, 4.0) | 2 (18) | 16: tank+2 spitter / 6 swarmer+spitter / tank+spitter+4 swarmer | 23.7 / 51.3 | 13.8 | 1 / **143** | 58.7 (50) | 55.0 / AIRBORNE | FAIL R5 | 52 % / 1.75 | 11/11 | 95 / 14 / 0 |
| g8 THE STAMPEDE | 21x30 (630), 12 | 2d 4h 2w, 2 ramps, 1 pad, 5 pillars; lava 1 | 1 / 6 | 3 (0, 3.0, 5.5) | 2 (23) | 10: charger / 2 charger+2 husk / 3 charger+2 spitter | 26.2 / 49.1 | 16.5 | **5** / 0 | 46.0 (30) | 76.1 / COOL | FAIL R2, R5 | 39 % / 1.40 | 11/11 | 7 / 5 / 1 |

Level (FB):
- stall 34.7 % and DULL 71.6 %, the highest of the five levels
- air 37.3 %, the lowest
- kill gap up to 66.3 s, deaths 6, falls 143
- clear sum 241.1 s, level time median 292.0 s

RC: 2 of 8 rooms pass (g1, g2). TOUR: **RED**, because g3's mid36 pose is inside a solid ("the camera in a wall"). V37: PASS 29 / 29. The
loadout: the gauntlet only until g6 "the KESTREL returns" (the plan's verb36).

### Level 3 THE KILN (n57-kiln, NIGHT57's rebuild)
| room (job) | m (m2), h | solid | dress / lights | planes | ride | waves | stall / air | gap | deaths / falls | clear (par) | DULL / top | RC | I1 / I2 | CT | AIR |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| kb2 THE CHARGING FLOOR (teach) | 20x28 (560), 14 | 4d 5h 1w, 2 ramps, 2 pads | 0 / 7 | 3 (0, 2.5, 4.5) | 1 (17) | 18: 3 husk+4 swarmer / 2 spitter+3 husk / 4 swarmer+2 spitter | 18.6 / 52.6 | 8.3 | 0 / 0 | 36.4 (60) | 63.6 / COOL | PASS | 15 % / 1.44 | 11/11 | 9 / 4 / 0 |
| kb2c THE CLINKER BRIDGES (teach) | 18x30 (540), 12 | 8d 3h 1w, 2 ramps, 1 pad | 0 / 8 | 3 (0, 1.5, 4.5) | 1 (17) | 15: burrower+4 swarmer / 4 husk+2 swarmer / burrower+3 husk | 17.1 / 61.7 | 13.4 | 0 / 0 | 34.0 (60) | 16.8 / AIRBORNE | PASS | 49 % / 2.18 | 11/11 | 12 / 5 / 0 |
| kb3 THE PRESS HALL (combine) | 20x28 (560), 14 | 4d 7h 1w, 2 ramps, 2 pads | 0 / 7 | 3 (0, 2.5, 4.5) | 1 (17) | 23: 5 husk+2 spitter / charger+4 husk / charger+2 spitter / 4 husk+4 swarmer | 14.7 / 60.5 | 9.3 | 0 / 0 | 44.7 (70) | 6.6 / SAVAGE | PASS | 44 % / 2.21 | 11/11 | 6 / 4 / 1 |
| kb3c THE TAP RUN (test) | 22x24 (528), 12 | 4d 5h 1w, 3 ramps, 2 pads; lava 1 | 0 / 8 | 3 (0, 2.5, 4.5) | 1 (18) | 12: 2 drone+4 husk / 2 drone+4 swarmer | 20.3 / 65.4 | 9.8 | 1 / 0 | 28.5 (60) | 0.0 / SKYGRIND | PASS | 31 % / 1.40 | 11/11 | 14 / 6 / 0 |
| kb3d THE CRACKED FLOOR (combine) | 24x20 (480), 12 | 5d 5h 1w, 2 ramps, 1 pad; lava 1 | 0 / 9 | 3 (0, 2.5, 4.5) | 1 (17) | 12: burrower+4 husk / charger+4 swarmer / burrower+charger | 25.6 / 54.2 | 15.3 | 0 / 0 | 26.6 (60) | 0.0 / SKYGRIND | PASS | 12 % / 1.50 | 11/11 | 5 / 4 / 0 |

Level (FB):
- stall 18.8 %, air 58.9 %, DULL 18.6 %
- deaths 1, falls 0, 15 stuck burrowers ended
- clear sum 170.2 s, level time median 187.7 s

RC: **5 of 5 pass**, the only level where every room does. TOUR: GREEN. V37: PASS 31 / 31. The pars are generous (clear / par 0.44-0.64).

### Level 4 THE SPAN (n52-span)
| room (job) | m (m2), h | solid | dress / lights | planes | ride | waves | stall / air | gap | deaths / falls | clear (par) | DULL / top | RC | I1 / I2 | CT | AIR |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| p1 HOOK YARD (teach) | 24x32 (768), 12 | 3d 8h 2w, 2 ramps, 1 pad, 5 anchors | 0 / 6 | 3 (0, 2.4, 4.0) | 2 (18) | 9: 3 husk+swarmer / 2 spitter+3 swarmer | 23.5 / 51.3 | 11.3 | 0 / 10 | 26.6 (20) | 60.3 / BOLD | PASS | n/c / 1.91 (I3 PASS 3.10) | 11/11 | 24 / 8 / 1 |
| p3 KNOT YARD (practise) | 24x30 (720), 14 | 4d 3h 2w, 3 ramps, 1 pad, 5 anchors; a void | 0 / 6 | 3 (0, 2.5, 4.2) | 2 (17) | 15: 4 husk / 2 spitter+3 husk / 4 swarmer+2 husk | 8.0 / 57.6 | 10.0 | 0 / **55** | 27.8 (35) | 0.0 / SKYGRIND | FAIL R2, IN VIEW | 55 % / 3.18 | 12/12 | 2 / 3 / **3 of 4** |
| p5 SPOOL HALL (combine) | 26x28 (728), 14 | 3d 3h 4w, 4 ramps, 1 pad, 4 anchors | 0 / 6 | 3 (0, 2.5, 4.2) | 4 (17) | 17: 4 husk+2 swarmer / 2 spitter+3 husk / 4 swarmer+2 spitter | 12.7 / 62.0 | 19.0 | 0 / **42** | 34.3 (36) | 0.0 / SKYGRIND | FAIL R5 | 11 % / 1.41 | 12/12 | 15 / 7 / 1 |

Level (FB): stall 14.5 %, air 57.3 %, DULL 18.4 %, deaths 0, **falls 107**, clear sum 88.7 s, level time median 111.4 s. RC: 1 of 3 pass.
TOUR: GREEN. V37: PASS 31 / 31.

### Level 5 THE SPIRE (n52-spire)
| room (job) | m (m2), h | solid | dress / lights | planes | ride | waves | stall / air | gap | deaths / falls | clear (par) | DULL / top | RC | I1 / I2 | CT | AIR |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| v2 THE NAVE (practise) | 24x32 (768), 14 | 5d 5h 1w, 4 ramps, 2 pads, 3 anchors | 3 edge lights / 10 | 3 (0, 2.4, 5.0) | 1 (17) | 13: 3 husk+2 swarmer / 2 spitter+2 husk / heavy+3 swarmer | 19.0 / 52.7 | 30.8 | 0 / 0 | 49.9 (45) | 33.4 / BOLD | FAIL R5 | 55 % / **1.17 FAIL** | 12/12 | 33 / 8 / 0 |
| v5 THE RINGERS' STAIR (practise) | 24x26 (624), 12 | 2d 4h 2w, 3 ramps, 2 pads, 3 anchors | 0 / 7 | 3 (0, 2.5, 5.0) | 2 (18) | 8: 3 husk+spitter / 2 swarmer+2 husk | 10.7 / 71.6 | 7.8 | 0 / 0 | 21.6 (10) | 0.0 / SKYGRIND | FAIL R5 | 49 % / 1.54 | 12/12 | 22 / 7 / 0 |
| v6 THE CHOIR (combine) | 24x32 (768), 16 | 5d 4h 2w, 3 ramps, 2 pads, 4 anchors; lava 1 | 0 / 5 | 3 (0, 2.4-3.2, 6.0) | 2 (24) | 17: 4 husk+2 swarmer / heavy+husk+2 drone / heavy+4 swarmer+2 husk | 18.3 / 60.1 | **41.8** | 0 / 0 | **79.6** (60) | 0.0 / SKYGRIND | FAIL R5 | 25 % / 1.41 | 12/12 | 18 / 7 / 0 |

Level (FB): stall 17.5 %, air 59.2 %, DULL 11.0 %, deaths 0, falls 0, clear sum 151.1 s, level time median 170.4 s. RC: 0 of 3 pass.
TOUR: GREEN. V37: PASS 31 / 31. v6 is the slowest fight room in the campaign (79.6 s). v5's par of 10 s is far under its 21.6 s clear: the
par looks stale (not checked).

**Across the campaign:**
- RC passes **9 of 25** rooms: a3, g1, g2, kb2, kb2c, kb3, kb3c, kb3d and p1.
  - R5 (no foothold) fails in 14 rooms: a2 a4 a5 b1, g3-g8, p5, v2 v5 v6.
  - R2 (a dead end) fails in a8, g8 and p3.
  - IN VIEW fails in a8 and p3.
  - On n56-room's 18 rooms (ALL18), R5 now passes 7 / 18, IN VIEW 6 / 18 and R2 16 / 18. NIGHT56 L's baseline was "IN VIEW PASS 1 / 18; R5 PASS 6 / 18; R2 PASS 16 / 18" (Research/2026-10-20-wave56/L.md:61). kb2, kb2c and kb3 were rebuilt since (NIGHT57).
- Identity: I1 (the room mirrors itself) fails in a3 and a4. I2 (the ride is no shortcut) fails in a8, g1, g3, g4 and v2. I4 (pads clear) passes in all 25.
- CT "enemies reach every tier" (walkers on walk and drop links; GG NIGHT20: "There always really needs to be a route that enemies can climb up") passes in **25 / 25**. In the 1-plane rooms this is trivial.

## 3. The weakest rooms, by these numbers

I flagged each room against named thresholds. Each threshold is a reading choice of mine, not a game rule:
- BIG: 600 m2 or more
- EMPTY: 3 enemies or fewer, or under 1.0 per 100 m2
- STALL: 40 % or more
- DULL: 70 % or more
- FLAT: 1 standing plane
- GAP: a kill gap of 30 s or more
- SLOW: clear / par of 1.3 or more
- DEATHS: 4 or more
- FALLS: 40 or more
- the RC fails (R5, R2, IN VIEW) and the identity fails (I1, I2)

| flags | room | which |
|---|---|---|
| 7 | g1 TANK ALONE | EMPTY STALL DULL FLAT GAP SLOW I2 |
| 7 | g4 THE REACH | BIG EMPTY DULL GAP SLOW R5 I2 |
| 6 | a8 PIERCE | EMPTY STALL FLAT R2 IN VIEW I2 |
| 6 | g8 THE STAMPEDE | BIG DULL SLOW DEATHS R5 R2 |
| 5 | g3 THE SWARM | BIG STALL DULL R5 I2 (+ the TOUR WARN) |
| 5 | g5 THE COOLDOWN | BIG STALL DULL GAP R5 |
| 4 | a5 CRUCIBLE / g6 THE ARMOURY / p3 KNOT YARD / v2 THE NAVE / v6 THE CHOIR | a5 BIG DULL DEATHS R5; g6 EMPTY STALL FLAT R5; p3 BIG FALLS R2 IN VIEW; v2 BIG GAP R5 I2; v6 BIG GAP SLOW R5 |
| 3 | a4, g7, p5, v5 | a4 EMPTY R5 I1; g7 BIG FALLS R5; p5 BIG FALLS R5; v5 BIG SLOW R5 |
| 2 | a2, a3, b1, g2, p1 | a2 EMPTY R5; a3 DEATHS I1; b1 BIG R5; g2 STALL GAP; p1 BIG SLOW |
| 0 | kb2, kb2c, kb3, kb3c, kb3d | none (all 480-560 m2: under BIG) |

**Per level:**
- **Level 1 (shipped n52 rooms):** a5 (DULL 71.5 %, the level's highest; 6 deaths, the level's most), then a8 (stall 45.6 %, one plane, a 7 m ride that is no shortcut, a 3-husk fight of 5.4 s), then a3 (stall 38.2 %, 4 deaths, a 69 % mirror). Level 1 is the room-file rebuild's level (section 6).
- **Level 2 is the weakest level on four level-wide numbers:** stall, DULL, air and the longest kill gap. RC passes only 2 of 8. Level 1 has more deaths: 13 against level 2's 6.
  - g1: 572 m2 holding 2 enemies and 2 wall pieces (0.35 pieces per 100 m2); DULL 100 %; a 66.3 s kill gap.
  - g4: 720 m2 holding 3 drones; DULL 88.4 %; clear 39.3 s against par 30; its ride is no shortcut (1.18).
  - g3: stall 67.2 %, the campaign's worst; air 0.9 %; the nearest enemy at a median 0.9 m (FB "near"). The swarm sits on the bot.
  - g8: 5 deaths and a dead end. g5: a 62.7 s kill gap.
- **Level 3:** no room trips a flag. Its weakest by its own numbers is kb2, the first room (DULL 63.6 %). But SKIP shows kb3c's carded verb, the ride, is skippable: refusing it is faster. GG has never played n57-kiln: it shipped 2026-10-21, and CORE.md's last recorded play reaction is 2026-10-20.
- **Level 4:** p3 (55 falls, a dead end, IN VIEW fails, AIR dead ends 3 of 4), then p5 (42 falls, no foothold). Only 3 fight rooms. The level's two largest spaces are the void links p2 (1800 m2) and p4 (4080 m2), which are not measured.
- **Level 5:** v6 (79.6 s, a 41.8 s kill gap) and v2 (a 30.8 s gap, a ride that is no shortcut). R5 fails in 3 of 3.
- **Overall bottom six:** g1, g4, a8, g8, g3, g5. Four of them are level 2. a8 is a teach room in the level being rebuilt.

**Structural findings (all rooms):**
- **The rooms are one template.** 19 of the 25 meet the same computed contract: 11 / 11, or 12 / 12 with the grapple. Those are a3, a5, b1, g2, g3, g5, g7, g8, the five kiln rooms, p1, p3, p5, v2, v5 and v6. g4 reads 10 / 11.
  - That means 3 standing planes (the floor, one at 1.5-3.2 m, one at 4.0-6.0 m), 2.50-3.08 pieces per 100 m2, 1-4 ride walls with the longest 17-32 m, 1-4 pads and one health pickup in a 480-768 m2 rect (CT, PS).
  - Inside that same envelope the bot's DULL runs from 0.0 to 76.1 %, and its stall from 8.0 to 67.2 %. Meeting the contract does not tell a good room from a bad one.
- **Size:** 20 of 25 fight rooms are 500 m2 or more (PS). The small ones are a2 280, a4 152, a8 308, g6 252 and kb3d 480. GG, 2026-10-09: "I'd like a lot of the rooms to be a lot smaller and more action packed like think about the design of Ultra Kilikow".
- **Height:** in every 3-plane room the top plane is 4.0-6.0 m (CT). a2 and a4 stop at 2.0 m and 1.5 m. a8, g1 and g6 are floor only. Room heights are 6-16 m.
- **Few dressing pieces in the shipped rooms:** 0-3 colliderless props per room (pickups aside), 3-10 lights (PS). Level 1's rebuilt files carry 4-12 each (section 6). GG, 2026-10-11 (OWNER.md:95): "the game is cluttered with detail for the sake of detail".

**What these numbers can and cannot see:**
- **The walk bot never uses a room's carded verb on purpose (L-150).** Its DULL is the game's own style meter on a bot.
  - Spearman rank correlation over the 25 rooms (my arithmetic on FB and PS): DULL against air -0.72, against stall +0.60, against level order -0.46, against enemies per 100 m2 -0.42, against kills per second -0.36, against area +0.17.
  - So DULL mostly reads "the bot moved little and killed slowly". It does not read whether the space asks for movement.
- **DULL falls room by room inside levels 3, 4 and 5:** kb2 63.6 to kb3d 0.0, p1 60.3 to p3 0.0, v2 33.4 to v5 0.0. Either the style rank carries across rooms or the loadout grows (not checked which). Compare DULL only inside a level. The loadout grows across levels too (level 1: the revolver only, V37).
- **RC, CT, AIR and identity are static.** They count pieces, planes, faces and links. Whether a piece has a job in the fight is judged only by the decoration checker v2 (`Tools/night/n67-deco.cjs`), and only on n65 room files with a skip v2 run.
- **No Player, no human.** The look, readability and clutter are not checked. Links and boss rooms are not measured per room: n46flow skips links and plays bosses only with `--bosses`.

## 4. Which instruments can measure which levels today

| instrument | what it needs | L1 shipped (n52) | L1 rebuild (n65 rooms) | L2-L5 |
|---|---|---|---|---|
| n46flow (walk bot; `--rooms`, `--file`) | a level file | yes (FB) | yes (N65). Its geometry column reads the tracked `Design/levels/<id>.json` through `node Tools/night/n46-flow-geo.cjs <id>` (Harness.N46.Flow.cs:604), not `--file` | yes (FB) |
| room check + identity (n59-l-room.py) | plan, level, openings | yes | yes (`--dir`) | yes, all 19 rooms. IN VIEW reads "-" where a room has no opening (a2 a4 a8 g1 g6) |
| contract (n52-room-check.py) | plan + levelgen report | yes | yes | yes. Teach rooms are exempt by the contract's text |
| air65 (n65-air.cjs), iso65 (n65-iso.cjs) | a plan | yes | yes | yes (AIR ran on all 25). iso65's cameras derive from the rect and doors (not run here) |
| tour65 headless (n65tour) | a level | GREEN | not run | GREEN on kiln, span and spire; **RED on iceline** (g3) |
| verbs37 (Player reel) | a Player build | PASS (V37) | RED 22 / 27 with the row OFF (V37; NIGHT66-REPORT 8: a row-OFF artefact) | PASS (V37). Per level, not per room. Needs a build: with C: at 14.2 GB free at 08:05 (`Get-PSDrive C`), DECISIONS D-2's branch C (7-18 GB) runs the sim only |
| verb pilots (`n65verb launch / pierce / parry`) | a pose (line65 or the spawn) and the verb's targets | no line65 | yes (6 rooms) | no line65 anywhere |
| skip v2 / `n46flow --verbs65` | `verb65` in the row, or `--plant-verb65 r:v`; the verb in the pilot's set (launch, launch+slide, pierce, ride, parry of a **spitter's** glob, slide: Harness.N65.VerbPilot.cs:6-15); a bar | planted only | yes | planted only. **It works off level 1:** SKIP on kb3c reads `M1 FAIL ... ratio 0.81 vs bar 1.25 (--bar), exercised 5/5`, M2 FAIL 47.3 vs 49.5 %, medians 23.7 / 17.9 / 17.9 s (101 s wall) |
| decoration checker v2, GATE pack, n67-table / round / red | n65 room files, slots65.json, Logs/n67 | no | yes | **no** (they are level 1's room-file tools) |

**The gap for levels 2-5.** The verb pilots perform only 5 verbs. Of the 19 rooms on levels 2-5, **one** has a carded verb (PS `verb36`)
inside that set: kb3c (the ride). The rest are outside it:
- g1 the tank punch; g2 the warden's shield; g3 / g5 the LT blast
- g4 a drone's orb parried back (the pilot parries spitter globs only); g7 the weapon switch; g8 a charger dodged into a column
- kb2 the THUNDERCLAP boost; kb2c / kb3d the burrower; kb3 / kb3d the charger stun
- p1 / p3 / v5 the grapple pull; p5 the whip; v2 / v6 the TORCH and jump-dash

The ride appears as a secondary verb in most rooms (every contract room has a ride36 wall), and SKIP shows it is the verb the instrument can
plant everywhere. There are no reds or bars for any L2-L5 room: `mainBar = max(1.25, red65 + 0.20)` exists for level 1's six only
(NIGHT66-REPORT section 3).

## 5. What a night could change in a room

**Editable per room in the plan** (PS; the plan's own row keys):
- In the room row:
  - the geometry frame: `rect`, `height`, `roof` (the doors fix the rect, so changing one moves the neighbours' doors)
  - the fight: `par`, `spawn` (D-7a: the retry point, never moved toward the verb line), `exit`, `waves` (lists of kinds), the spawn nodes `nodes` / `highNodes` / `waveNodes43`, `standIns`
  - the teaching and proof rows: `lessons` / `lessons31`, `proofs` (walk proofs), `frames36` / `frames52`, `grappleNodes`, `job36` / `verb36`
  - n65 room files only: `verb65`, `line65`, `card65`, `frames65`, `bot43`
- Pieces keyed by `zone`:
  - `boxes`: kind floor / deck / hull / wall / slab, plus `job`, `why`, `ride` + `ride36` face, `noride`, `gate`, `ceiling`, `burrowSafe43`
  - `ramps`: from, to, width, rise, job
  - `pads`: at, target, apexAbove
  - `pits`: rect, floorY, lava
  - `tunnels`: rect, ceiling
  - `surfaces` (ice)
- Keyed by `room`: `lights` (kind, at, colour, range, intensity) and `props` (kind, `collider`, `pickup`).
- The room's opening: `Design/levels/n52/openings53.json`, overlaid per prefix by `openings<NN>.json`.
- Enemy kinds already placed in the campaign's waves (PS): husk, swarmer, spitter, charger, tank, warden, drone, burrower, heavy. New kinds are GG's (CLAUDE.md rule 7).

**How each level's rooms are built and shipped:**
- **L1:** room files plus slots.
  - `Design/levels/n65/rooms/<id>.json`, assembled by `Tools/night/gen-n65-foundry.cjs` (`--out` keeps a round off the tracked files).
  - It ships only by a slot write plus `design.n65.foundry65` ON. b2 is a slot too.
- **L3:** `Tools/night/gen-n57-kiln.cjs` (slots kb2 / kb2c / kb3 / de). n57-kiln is the shipped file, so editing its plan changes the shipped game unless a new row swaps the file.
- **L4:** `Tools/night/gen-n58-span.cjs` slots, row span58 OFF. Its NEW list is empty: THE GAP NEST in p2 was taken out "until its header checks run" (56681aee).
- **L2 and L5:** no generator. The plan goes through levelgen into the shipped `n52-<level>.json` and its StreamingAssets copy, and the rows iceline52 / spire52 are ON. **An in-place edit ships at once.**
- The safe path every night has used is a new `design.nNN.<row>.levelFrom / levelTo` row (NightRows.SwapLevels) pointing at a new file.

**Run costs measured today (alone in the tree):**
- levelgen on level 1's assembled plan: 8.6 s
- n46flow, 10 seeds, the whole n65 level: 251 s. FB's five-level run spent 34-81 s per level.
- skip v2, one room, 5 seeds: 101 s
- RC, CT and AIR: seconds per room. AIR on g7 took over 60 s.

## 6. Level 1's rebuilt rooms (the carry-over, row OFF), measured today

NIGHT67's r1 was meant to measure these. Here are the same instruments, run into the scratchpad with no tracked file written
(`git status` unchanged):
1. `node Tools/night/gen-n65-foundry.cjs --slots=latest --out=<scratch>/n65latest`
2. `dotnet <dll> levelgen <scratch>/n65latest/foundry.plan.json <scratch>/n65latest/foundry.json --walk --report <scratch>/n65latest/LEVEL-n65-foundry-validate.md`. It printed "level report: 0 errors", "plan checks: 2 problems" and "bot walk: DID NOT reach the last exit", the same three lines as `Logs/n66/r2/gen-latest.txt`.
3. `node Tools/night/gen-n65-foundry.cjs --finish --out=<scratch>/n65latest`
4. RC `--dir`, CT on the scratch plan, AIR, and `dotnet <dll> n46flow --level n65-foundry --seeds 10 --first 1 --file <scratch>/n65latest/n65-foundry.json`

| room | stall % (n52) | air % (n52) | DULL % (n52) | clear s (n52) | deaths (n52) | RC (n52) | CT (n52) | planes | AIR hand-offs / chain (n52) | dress props / lights |
|---|---|---|---|---|---|---|---|---|---|---|
| a2 | 30.5 (12.4) | 64.2 (73.9) | 36.7 (21.1) | 19.1 (14.7) | 0 (0) | PASS (FAIL R5) | 5/11 (3/11) | 2 | 17 / 5 (0 / 1) | 10 / 4 |
| a3 | 21.6 (38.2) | 56.2 (51.1) | 44.8 (21.0) | 36.4 (28.1) | 3 (4) | PASS (PASS) | 9/11 (11/11): density 1.73, 2 planes | 2 | 19 / 6 (45 / 9) | 12 / 7 |
| a4 | 43.1 (33.7) | 56.1 (57.6) | 87.2 (43.5) | 27.9 (13.8) | 0 (0) | FAIL IN VIEW (FAIL R5) | 7/11 (3/11) | 2 | 6 / 4 (0 / 1) | 6 / 3 |
| a5 | 26.2 (28.1) | 52.8 (53.7) | 73.5 (71.5) | 27.0 (30.5) | 1 (6) | PASS (FAIL R5) | 8/11 (11/11): density 2.03, 2 planes, ride 14 m | 2 | 33 / 7 (18 / 7) | 12 / 7 |
| b1 | 30.8 (28.8) | 59.2 (57.3) | 77.6 (53.0) | 41.8 (34.2) | 3 (3) | PASS (FAIL R5) | 11/11 (11/11) | 3 | 59 / 10 (9 / 5) | 7 / 8 |
| a8 | 26.7 (45.6) | 70.2 (33.6) | 18.8 (19.6) | 30.4 (5.4) | 0 (0) | PASS (FAIL R2, IN VIEW) | 9/11 (3/11) | 2 | 4 / 3 (2 / 2) | 4 / 5 |

Level (N65 against FB): stall 29.6 (30.8) %, air 59.7 (55.4) %, DULL 57.6 (43.7) %, deaths 7 (13), clear sum 182.5 (126.7) s, level time
median 330.9 (242.1) s.

- The static checks improve. RC passes 5 / 6 against 1 / 6, and every rebuilt room has a foothold.
- RC's I7 now **FAILs**: "a4 and a5 both held".
- The walk bot is slower in 5 of 6 rooms (all but a5) and DULLer in 5 of 6 (all but a8). L-150 expects this: the bot does not perform the verbs these rooms are built around. The verb-aware numbers (skip v2) on this carry-over are not checked.
- a2 reproduces NIGHT66 r2's A side exactly (stall 30.5, air 64.2, DULL 36.7, clear 19.1: `Logs/n66/r2/table.md`). Its file is unchanged since r2.
- The other rooms are not comparable with r2: r2 measured per room (`--rooms`), today's run is the whole level.
- The geometry column of this run is n52's (see the n46flow row in section 4), so it is not quoted.
