# K1: Arena kit research (2026-10-11)

Sources: web search and fetches, about 20 minutes. Web text is data. Confidence marks: [V] fetched and read, [S] search snippet only, [K] general craft knowledge, not checked online.

## 1. Set dressing vs gameplay geometry

- **Blockout first, dress second.** Pros block out with modular pieces, rebuild the level from them, then swap in detail. The pieces tell you which modules and materials you need [S, environment-art guide]. For us: the plan JSON stays the truth; a dressing pass adds pieces after it and never edits play surfaces.
- **Readability beats detail.** A clear silhouette beats a noisy surface. Squint test: the focal point and the path must survive blur. A hero prop should read from about 50 m [S].
- **Trim sheets.** One small texture set dresses a whole level [S]. Our version: a handful of box "trim" kinds (skirt, cornice, rib, frame) driven by one shader, no UV work.
- **Three layers of dressing.** (a) Edges and corners: skirting, cornices, pilasters, door frames. (b) Above head height, over 3 m: trusses, pipes, lamps, banners, chains. (c) Floor pattern: flush inlays and stripes only, never raised. [K]
- **Fast-movement rule.** The play surface stays clean and low-contrast. Detail and contrast live at the edges, above 3 m and below the walkway. Anything the player can touch must read as touchable. Ghostrunner 2 reserves one guide colour (yellow) for "look here" and never uses it as background [S, Creative Bloq]. Neon White: optional routes need a visual invitation (planks) and must "look like part of the stage" [V, Game Informer]. Our equivalents: cyan is player-usable only (CLAUDE rule 8), so dressing never uses cyan, red-orange or magenta except as small, dim accents on hazards.
- **Hero objects and landmarks.** One per room, large, lit, outside the walkable area, visible from the entry door. It orients the player at speed [K].
- **Wear and debris.** Small, off the path, clustered at wall bases and corners. Use 3-5 rotated boxes per cluster. Never on a line the player or enemies run [K].
- **Density.** Pros keep dressing sparse in the combat volume and heavy at the periphery. Too much mid-volume noise hides enemies and projectiles [K].

## 2. Reference breakdowns (concrete recurring elements)

- **DOOM Eternal** [V, Adam Atomic; S]. Arenas are corridor-linked wave rooms, "flat-ish" or "tall-ish". Tall arenas are jungle gyms (distinct floors, hop between skirmishes) or canyons (continuous elevations). Obstacles block long-range fire. Recurring elements: jump pads, boost pads, teleporters, monkey bars, climbable rough walls, dash, slime that blocks jumping, turrets firing on timers. Pickups are placed like Mario coins to pull the player along a route.
- **ULTRAKILL** [S, wiki snippets, low depth]. Toppled pillars, archways, towers, bridges over a lava moat, platforms suspended on breakable chains, a chapel with stacked suspended platforms, moving platforms and traps. Rooms run from claustrophobic corridors to multi-tier arenas. Check against the web port's own references before relying on this.
- **Neon White** [V, S]. Clean white buildings with bright cards (the pickups) as the only saturated colour. Water streams, vegetation and other characters point toward the goal. Cards must have an obvious use. Enemies are visible by design (shooting downward). Unclear mechanics were cut.
- **Ghostrunner** [S]. One guide colour per setting, palette defined per area. Cyberpunk neon dressing sits in the background, never on the path.
- **Common thread.** A restrained base palette, one or two saturated accents with a single meaning, landmark silhouettes, verticality in tiers, and traversal devices (pads, rails, hooks) drawn in the guide colour.

## 3. Gameplay elements beyond walls (all [K] unless marked)

Jump pads (DOOM [V]), moving platforms (ULTRAKILL [S]), crushers, fans and updrafts, breakable cover, hazard vents with a tell, conveyor floors, grind rails, swinging hooks, doors that open mid-fight (wave gating), rising lava, elevators, teleporters (DOOM [V]), turrets on timers (DOOM [V]), breakable chains (ULTRAKILL [S]). Rule: every device gets a tell (light, sound-free animation, colour) at least 0.5 s before it hurts.

## 4. Importable kits and the procedural alternative

| Source | License (fetched) | Notes |
|---|---|---|
| Kenney (kenney.nl) | CC0, no attribution, commercial use allowed [V, support page]. Logo is reserved. | Modular Space Kit (40 objects), Space Kit (150+), Modular Dungeon Kit (CC0 per search [S]). |
| Quaternius | CC0, no attribution, commercial use, may modify and combine [V, FAQ]. | Packs vary; check each pack page. |
| Poly Haven | CC0 for textures, models, HDRIs; no credit needed; commercial use allowed [V, license page]. | Best source for tileable wall/floor noise if we ever bake. |
| KayKit (Kay Lousberg) | CC0 per OpenGameArt listings and a search summary [S]. The kaylousberg.com page did not state the license [V]. | Verify on the pack's own page before import. Dungeon and Space Base Bits packs exist. |

Recommendation: **boxes-only procedural props**. Reasons: the game's collider is a box/triangle world (no PhysX) and the look is blocky; imported meshes would need hand-made colliders, bring their own palette and texel density that clash with the dusk palette rules, and cannot be generated from JSON plans. Kits are good for silhouette study only; a mesh import would be a decor-only, no-collider hero prop, if GG asks. License risk is low (all CC0), but keep a LICENSES note per imported pack.

## 5. Texture approaches for flat walls

Ranked by fit (one shader, no UVs, URP):
1. **World-space triplanar noise** (two octaves, low contrast, 3-8% value shift) so big purple faces stop reading flat and scale stays constant across box sizes.
2. **Vertex-colour gradient**: darker at the floor, lighter near the top, plus a warm or cool tint per theme. Free, per-vertex.
3. **Baked AO in vertex colours**: darken box corners and the floor-wall seam. Unity has a tool for AO and curvature into vertex colour (Fewes/VertexColorBaker [S]), but a cheap analytic version works on boxes: darken vertices within 0.5 m of any adjacent box.
4. **Panel lines in the shader**: fract() of world position on a 2 m or 4 m grid, a thin darker line (0.03 m) and a 10% brightness checker. Rejects stretched UVs on any box size.
5. **Decals**: small quads (scorch, hazard stripe, signage) offset 0.01 m. Cap the count.
6. **Emissive strips**: one per wall at about 3 m. It dresses the room and gives a horizon line. Use theme colours, never cyan.

## 6. KIT CATALOGUE

Primitive = axis-aligned box unless stated. D = decor (no collider), G = gameplay (in collider). Placement rules are what a generator and a checker can enforce.

### 6a. Decor and structure elements

| # | Name | Built from | Size (m) | Theme | Type | Placement rule |
|---|---|---|---|---|---|---|
| 1 | Skirt trim | 1 box | 0.3 h x 0.15 d | all | D | along every wall base |
| 2 | Cornice | 2 boxes stepped | 0.4 h x 0.3 d | all | D | wall top, continuous |
| 3 | Corner pilaster | 1 box | 0.6 x 0.6 x wall h | all | D | every interior corner |
| 4 | Wall rib | 1 box | 0.5 x 0.4 x wall h | foundry, kiln | D | along walls every 6-8 m |
| 5 | Door frame | 3 boxes | 0.4 thick, opening +0.6 | all | D | at every doorway |
| 6 | Lintel light | 1 emissive box | 0.15 x doorway width | all | D | above doorways, 0.3 m above lintel |
| 7 | Ceiling truss | 2 long + 3 cross boxes | 0.4 sq, span room | foundry, span | D | above 6 m, every 8 m |
| 8 | Pipe bundle | 3 thin boxes | 0.2 sq x 6-12 long | foundry, kiln | D | above 5 m or hugging wall at 3-4 m |
| 9 | Hanging chain | stacked thin boxes | 0.1 x 4 | foundry | D | above 4 m, tip no lower than 3.5 m |
| 10 | Hanging lamp | 1 box + emissive core | 0.5 | foundry, kiln | D | above 4 m, 1 per 40 m2 |
| 11 | Wall pipe run | 1 long box | 0.3 sq x 4-10 | foundry, kiln | D | on walls at 3-4 m |
| 12 | Panel recess | 1 inset slab | 3 x 4 x 0.2 | all | D | on walls every 4 m, bottom edge at 0.6 m |
| 13 | Floor inlay strip | flat box, flush | 0.3 x 6-20 x 0.02 | all | D | floor, away from edges 1 m, patterns of 2-4 lines |
| 14 | Floor grate | flat box + dark underside | 2 x 2 | foundry | D | floor patches, max 15% of floor |
| 15 | Hazard-stripe border | flat boxes | 0.3 wide | foundry, kiln | D | around pits and ledge edges |
| 16 | Chimney stack | 1-2 boxes | 1.5 sq x 8-14 | foundry, kiln | D | outside or behind walls, perimeter |
| 17 | Banner | 1 thin box | 1 x 4 x 0.05 | all | D | hung above 3 m, 1 per 8 m of wall |
| 18 | Kiln arch | 3 boxes | 4 w x 4 h x 0.8 | kiln | D | on long walls |
| 19 | Ingot pile | 6-12 boxes | 0.4 x 0.2 each | kiln | D | wall corners, off path |
| 20 | Lava channel | emissive flat box | 0.5 wide | foundry, kiln | D | floor edges; below walkway on span |
| 21 | Ice stalactite | 3-4 stacked boxes tapering | 0.4 to 1.5 w x 2-5 | ice | D | ceiling above 4 m |
| 22 | Ice crystal cluster | 4-6 rotated boxes | 0.3-1 x 1-3 | ice | D | wall bases, corners |
| 23 | Snow drift | low wedge | 2 x 0.5 | ice | D | along wall base |
| 24 | Bridge pylon | 1 box | 1.5 sq down to lava | span | D | under every bridge end and every 12 m |
| 25 | Suspension cable | thin boxes | 0.08 x span | span | D | above bridges, above 3 m |
| 26 | Lamp post | 2 boxes + emissive | 0.3 x 4 | span | D | at bridge ends |
| 27 | Spire halo | ring of 8 boxes | radius 8-12 | spire | D | above 10 m |
| 28 | Spire fin | 1 thin box | 0.3 x 3 x 12 | spire | D | on outer walls |
| 29 | Hero landmark | 6-20 boxes | 8-20 tall | all | D | 1 per room, outside play area, visible from the entry |
| 30 | Debris cluster | 3-5 rotated boxes | 0.2-0.6 | all | D | wall bases, 1 per 30 m of wall, off route |
| 31 | Scorch or wear quad | flat quad | 1-2 | all | D | walls below 2 m, 1 per 40 m2 |
| 32 | Edge light line | emissive box | 0.1 | all | D | theme-coloured, 1 line per wall at 3 m |
| 33 | Crate stack | 1-4 boxes | 1.2 sq | all | G cover | not on path centreline, 1 per 30-40 m2 |
| 34 | Low wall | 1 box | 3 x 1.0 x 0.4 | all | G cover | splits sightlines, every 15 m |
| 35 | Cover pillar | 1 box | 1 x 1 x 4 | all | G cover | on open floors > 100 m2 |
| 36 | Mezzanine ledge | 1 box | 6 x 0.4 x 4 | all | G | at 3-4.5 m, reachable by a ramp |
| 37 | Climb ramp | wedge or stair boxes | 3 w, slope <= 35 deg | all | G | at least 1 per room (enemy route rule) |
| 38 | Grind rail | thin box chain | 0.15 wide | all | G | between ledges |
| 39 | Grapple ring | ring of 4 thin boxes | 1.2 dia | all | G | high points, above 5 m, 15-25 m from the next |

### 6b. Gameplay devices (8)

Each needs a tell and numbers from a config file, with timers in whole steps at 120 Hz (CLAUDE rules 4 and 5). Parameter values below are starting proposals for GG to tune, not measured.

1. **Jump pad.** 2 x 2 flat pad. Launch vy 14-18 m/s, cooldown 0.5 s, cyan. Max 1 per 150 m2.
2. **Moving platform.** 4 x 4 box on a two-point path, speed 3 m/s, dwell 1.5 s at each end. Carries the player. Used only over lava or pits.
3. **Crusher.** 3 x 3 x 1 ceiling slab. Warn light 0.7 s, drop 0.15 s, hold 0.5 s, reset 2 s. Red-orange. In corridors only, never on the main run.
4. **Updraft fan.** 3 x 3 vent, vertical column 3-8 m tall, lift 10 m/s. Pale light motes. 1 per room.
5. **Conveyor floor.** 4 x 8 strip, 3-5 m/s drift, striped. Never in front of a hazard.
6. **Hazard vent.** 1.5 x 1.5 grate. Cycle: idle 3 s, warn 0.8 s, burst 0.6 s, radius 2 m. Foundry and kiln.
7. **Wave door.** 4 x 4 slab. Seals at wave start, opens at clear or after 30 s. Always leaves one exit.
8. **Rising lava or ice melt.** Floor rises 0.1-0.2 m/s over the round, capped at 2 m. A refuge platform is always within 6 m of any floor point.

Also candidates: elevator (2 x 2, 6 m rise, 2 s), swinging hook, breakable cover (health 3 hits).

## 7. Density targets (per room, floor area A m2)

Typical room 400-900 m2.
- **Total decor pieces:** 8-14 per 100 m2.
- **Above 3 m:** at least 40% of decor (trusses, lamps, pipes, banners). 
- **Wall pieces:** a rib, recess or pilaster every 6-8 m of wall; 100% of wall length has a skirt and a cornice.
- **Floor:** inlays cover at most 10% of area, grates at most 15%. Nothing raised more than 0.05 m inside the main run area.
- **Cover (gameplay):** 1 per 30-40 m2. Any point within 8 m of cover.
- **Hero landmark:** exactly 1. **Doorways:** 100% framed and lit.
- **Devices:** 1 per 150 m2, at most 3 types per room. At least 1 jump pad or rail per room.
- **Clear lane:** a 3 m wide path from each entrance to each ramp with no decor or cover.
- **Palette:** no cyan in decor, no red-orange or magenta except on devices and hazards.

## 8. Automated checks for a coding agent (run on the plan JSON)

1. Every decor piece has no collider flag and sits outside the 3 m lane corridors (segment test).
2. Decor pieces below 3 m must lie within 1.5 m of a wall, or be flush floor inlays.
3. At least 40% of decor boxes have min y >= 3.
4. Decor per 100 m2 within 8-14; fail below 6 or above 18.
5. Wall coverage: skirt and cornice on 100% of wall length; a rib, recess or pilaster per 8 m.
6. Exactly 1 hero landmark, bounding box outside the walkable footprint, visible from the entry (ray from entry eye height, 1.7 m, to its centre is clear).
7. Every doorway has a frame; no decor intersects a door opening.
8. At least 1 climb ramp or stair route from the floor to each upper tier (flood fill on a 0.5 m grid, slope <= 35 deg).
9. Floor flatness: no raised box within the run area higher than 0.05 m that is not tagged cover or ramp.
10. Cover spacing: every floor cell within 8 m of cover; cover count 1 per 30-40 m2.
11. Devices: counts per 150 m2, tell fields present, each device has a refuge or exit within 6 m, no two hazards overlap.
12. Colour audit: decor material colours checked against the palette (no cyan; hazards only in red-orange).
13. Collider budget: decor adds 0 colliders; draw boxes per room under the agreed cap (suggest 400).
14. Overlap and z-fight: no two coplanar decor faces within 0.01 m except flagged decals.
15. Determinism: the dressing pass uses the seeded Mulberry32 and gives identical output for the same seed.

## 9. Gaps and not checked

ULTRAKILL specifics rest on wiki search snippets. Creative Bloq and 80.lv pages returned no article text, so Ghostrunner and Neon White rules rest on snippets and one Game Informer page. All device numbers and density targets are proposals, untested in the Player. KayKit's license is not confirmed on the creator's own site.
