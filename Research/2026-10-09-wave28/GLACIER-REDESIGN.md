# GLACIER-REDESIGN: a real curved rink, one ice rule, a boss you catch in a loop

Date 2026-10-09. Research + design only; no code edited. Tags: [read] = I opened the page or the file and read it.
[snippet] = a search-result summary I did not open. [design] = my own synthesis. [not checked] = I did not verify it.
Web text was treated as data. The web search tool returned thin results for most of the games asked about; where it gave
nothing I say so instead of guessing.

GG said (2026-10-09): rebuild the second boss; the arena cannot curve, the ice is on only part of the floor, the fight is
boring and repeats other levels; "maybe we just have to change our boss to be something more simple, unless we really dig
deep and research well and plan out the design."

---------------------------------------------------------------------------------------------------------------------

## 0. The finding that shapes everything: what the Mover can and cannot do (all [read] from the code)

The curved-bowl idea from other games does not port 1:1, because `Mover.cs` is frozen and a few of its rules decide what a
"curved arena" can mean here. Each fact below is from `Assets/Scripts/Core/Mover.cs`, `MovementMath.cs`, `Surface26.cs`,
`TriangleWorld.cs`, `PortExport/config.json`.

1. **Floor vs wall by normal.** A contact is FLOOR if normal.y >= 0.7 (a slope up to 45.6 deg) and a WALL only if
   |normal.y| < 0.3 (steeper than 72.5 deg). Between the two is a dead band neither rule claims. So a banked turn must be a
   floor-grade bank (I use 28 deg) and a separate near-vertical rim; a smooth 0-to-90 deg quarter-pipe would pass through
   the dead band, and what the player does there is not checked.
2. **The wall ride cannot follow a curve.** The ride fixes `rideNormal` at entry and projects velocity onto that plane each
   step (`StepWallRide`); it ends on timeout (`wallRide.maxTime` 1.2 s), when a wall contact is more than 30 deg off the
   entry wall ("corner"), when the probe (0.12 m) loses the wall, or when input points away. Entry needs along-wall speed
   > 4 m/s; the ride holds at least 8 m/s. So a ride is a straight line along a straight wall, about 1.2 s long
   (roughly 19-24 m at 16-20 m/s). "Wall-ride around a half-pipe" is not a verb the sim has. Wall lanes belong on STRAIGHTS.
3. **Ground carving by gravity does not exist.** In the Ground state velocity is horizontal Quake-style: friction, then
   `Accelerate` toward the wish direction up to `walkSpeed` 8 m/s (accel 10). Slope gravity applies ONLY in the Slide
   state (`SlopeSlideAccel = g * slopeBoost * normal.y * (normal . slideDir)`, g = 23.15 m/s^2 from jump height 1.5 / apex
   0.36 s, slopeBoost 1.6). The slide direction is locked and deflects off walls; it is not steered. So the "ride a bowl and
   pump speed" verb of skate games exists here in one form only: SLIDE DOWN A BANK. On a 28 deg bank straight down the fall
   line that is 23.15 x 1.6 x 0.883 x 0.469 = about 15 m/s^2 [design arithmetic; measure it].
4. **Speeds you can count on.** Walk 8 m/s; slide floor 11.6; dash 24 m/s for 0.25 s (about 6 m), 3 pips, 0.7 pip/s regen;
   `speed.maxHorizontal` 40; a boost strip +6 m/s once. A player cannot run faster than 8 m/s without slide, dash, a bank,
   a strip or a pulse launch. The boss's base speed therefore has to sit between 8 and 11.6 m/s.
5. **Ice is `groundFrictionMult` (0.25 now) and nothing else.** `Surfaces26` sets it each step from RECTANGLES in the map's
   `surfaces` list, and only while the player is grounded. The Slide state ignores it (`slide.friction * slideFrictionMult`).
   Ground `accel` (10) is unchanged, so steering on ice is still crisp: at 0.25 the stop takes about 4x the metres but a
   turn is not slidier. `glacier-course.json` has NO `surfaces` key [read: top-level keys are `_why name tileLen half height
   tile start boosts playerSpawn`]; where the level-2 plan paints its ice I did not check. This is the likely source of
   "ice only partly applied": zones are rects, not "the floor". [design]
6. **The collision world takes triangles.** `TriangleWorld` has `AddTriangle`, `AddBox`, `AddWedge` (the web's 20 deg ramp);
   capsule-vs-triangle is the web Octree's exact port. A faceted bank is therefore legal collision data with no Mover
   change. Whether the map JSON can express arbitrary triangles or an arc/bank primitive: [not checked]; a builder may need
   a small primitive in Core that compiles an arc into triangles (a Core change, gated by the golden vectors and replays,
   but not a Mover change).

What this means: the old plan "a straight tile track with a camera-bend" (R2 Option A) is replaced by a REAL closed oval of
static triangles. No recycling, no visual-only curve, no rotated colliders. The curve is real because it is geometry.

---------------------------------------------------------------------------------------------------------------------

## 1. Research: how shipped games make a momentum arena / boss fun

I could verify little beyond the structural ideas. What I could read or see:

- **Furi** [read, itch.io analysis]: each boss alternates two phases that demand opposite skills, bullet-hell positioning
  (dash to set up the next move) and close-quarters reaction (telegraph, then parry or delayed dash); the move from one to
  the other is EARNED (survive until a health block drops, then you get a short window to close in). Lesson used: phases
  that change the movement verb, with a window the player earns.
- **ULTRAKILL** [snippet, player reviews]: a bossfight is fun when "you NEED every movement option you have" and the boss
  is designed like a character-action boss rather than a standing FPS target. No arena numbers found. Lesson used: gate
  damage on movement, never on standing and shooting (this was already the card in R2 section C).
- **Jet Set Radio** [snippet]: grind speed depends on the speed you had when you jumped on the rail; jumping while grinding
  adds speed. Lesson used: entry speed is carried into the lane (our slide-off-a-bank into a straight).
- **Trackmania / banked turns** [snippet]: high banking allows high speed; board-track items are wide turns with smooth
  transitions into the straights. Lesson used: bank the turns, ease into the straight (a flat lead-out).
- **Half-pipes / SSX** [snippet]: the ratio of transition height to radius sets the lip angle and the ramp's personality;
  a common skatepark quarter-pipe is about 3 ft rise on an 8 ft radius [snippet]. Lesson used: keep the bank at a
  floor-grade angle here (28 deg) because of Mover fact 1.
- **Rocket League** [read, rlgym.org constants page]: side wall +-4096 uu, back wall +-5120, ceiling 2044, corner ramp height
  256 uu (1 uu = 1 cm; so 2.56 m ramps and a 20.4 m ceiling). The arena is flat floor + curved corner ramps + walls. Lesson
  used: a curved corner with a low ramp (256 uu) keeps a flat play floor; use it as a sanity check for scale only.
- **Ice physics in games** [snippet]: slow to start, slow to stop, slowest to turn around; ice frustrates when it takes
  momentum away at a wall (player review) or feels uncontrollable. Lesson used: the SAME low friction everywhere (GG's
  "inconsistent" complaint is exactly this), glancing off curved boards must keep tangential speed (it does in the Octree
  capsule resolve: normal component removed, tangent kept; [design], to be measured), and nothing the player does should
  zero their speed.
- **Titanfall 2 Viper / Ash, Doom Eternal, Hades, Sonic, Mirror's Edge, Tony Hawk's level design**: the search returned
  marketing-level text or nothing on the design. I am not claiming anything about them. (Titanfall 2's Viper fight is on a
  flying ship [snippet]; no mechanics detail found.)
- **Not found anywhere:** an open-source ice-boss arena layout, a published ULTRAKILL room size in metres, or a licensed
  level file for a bowl / rink. So "copy an arena online" has no file to copy; the nearest thing is real-world rink standards
  (facts, below).

Real-world dimensions I can use as scale references [snippet, Wikipedia and sports pages]:
- NHL rink 200 x 85 ft = 61.0 x 25.9 m, corner radius 28 ft = 8.5 m.
- Short-track speed skating: 111.12 m oval, curve radius about 8 m.
- Hockey/skating dimensions are facts; I do not see a licence problem using them (not legal advice).

---------------------------------------------------------------------------------------------------------------------

## 2. Open-source and permissive sources (what we may copy)

| Source | URL | Licence | What it gives | Usable as collision data? |
|---|---|---|---|---|
| CurveDesigner (cmacmillan) | https://github.com/cmacmillan/CurveDesigner | MIT [read: the repo page] | editor tool for tubes, ramps, curves and half-pipes meshes in Unity | Only as an offline authoring aid; it makes Unity meshes. We would still export triangles. NOT needed: a bank is a strip of arc facets, ~100 lines of our own code. Do not take a dependency. |
| InstantPipes (leth4) | https://github.com/leth4/InstantPipes | MIT [snippet] | procedural pipe/tube editor | Not relevant (tubes). |
| Catlike Coding "Swirly Pipe" | https://catlikecoding.com/unity/tutorials/swirly-pipe/ | not checked | biarc (two circular arcs) pipe math | Read-only reference for arc sampling; licence [not checked], so no copying. |
| Unity ProBuilder | package docs (`ShapeGenerator.GeneratePipe`, curved stairs) | Unity package terms [not checked] | in-editor shapes | Editor-only, not `Skygrind.Core`; skip. |
| UnitySpline2D, unity-curves | https://github.com/sinbad/UnitySpline2D , https://github.com/zapdot/unity-curves | MIT [read in R2] | arc-length tables | Only if the rink path needs a distance-to-point lookup; a stadium oval has closed-form arc length (below), so not needed. |
| UnityHFSM | https://github.com/Inspiaaa/UnityHFSM | MIT [read in R2] | hierarchical state machine pattern | Already planned for boss FSMs (R2). |
| Kenney assets | https://kenney.nl/support | CC0, no attribution required [read] | art kits (3D / 2D) | Art only, not collision geometry; use for later dressing. |
| Quaternius, OpenGameArt, Poly Haven | not opened this session | CC0 / mixed [not checked; do not assume] | models, textures | Art only. |
| Rocket League constants (rlgym.org) | https://rlgym.org/Cheatsheets/game_values | site licence not stated [read] | scale numbers | Reference only; do not copy files. |

Honest conclusion: the geometry we need (an oval with banked ends) is simple closed-form math. No source gives data we can
drop into `TriangleWorld`; the cheapest, safest path is to generate it ourselves. Copy nothing; record real-world scale
references in the commit message.

Stadium-oval math we will use [design]: straight length L, end-cap radius R (centre line radius r_c). Centre-line length
= 2L + 2*pi*r_c. A point at arc distance s: straights are linear, caps are arcs (angle = (s - L) / r_c). A bank facet strip is
a sequence of quads with step 7.5 deg (24 per 180 deg cap), each split into two triangles wound outward with `AddOutward`.

---------------------------------------------------------------------------------------------------------------------

## 3. The recommended design: THE GLACIER, a lap chase on one sheet of ice

**One line:** you chase a skating ice-giant around a real banked oval rink. The whole rink, banks and island included, is
one ice surface. You cannot hurt it by shooting it down or by standing still: you crack its glaze (pistol), catch it
(slide off the banks, cut through the island, dash) and land the PULSE. Three phases change what you have to DO: slide the
bank, wall-ride a straight over thawing floor, then slalom columns and launch across a chasm.

### 3.1 The rink (STARTING VALUES, all `test`)

```
                  <-- 50 m -->
        ______________________________
       /   rim wall (vertical, 5 m)    \          plan view, stadium oval
      |   ____________________________  |         outer: 50 x 26 m, end-cap outer radius 13 m
      |  /   bank 28 deg, 4.5 m run   \ |         island: 24 m straight + radius 3 m ends (30 x 6 m) with 2 doors
      | |  ______ flat lane 5.5 m ____ | |         flat lane 5.5 m wide on each side of the island
      | | |       island (2.6 m)     | | |         bank 4.5 m run x 2.4 m rise (28 deg) between lane and rim
      | |  \______ flat lane 5.5 m __/ | |         rim wall 5 m tall above the bank top
      |  \____________________________/ |
       \_______________________________/
```
- Straights 24 m, end caps radius 13 m (outer), centre-line radius about 5.75 m from the island's cap, lap about 84 m
  (48 m of straight + 2 pi x 5.75). Width 26 m: close to the NHL rink (25.9 m) and inside the "short axis <= 24 m for fight
  rooms" spirit of NIGHT27 only for the flat part (20 m incl. banks/island); this is a boss arena, same as the Lantern's
  30-34 m ring. Height 8 m at the rim (bank top 2.4 + wall 5 + 0.6 lip). Roof per roof27 with a skylight slot.
- **Bank**: 28 deg (normal.y = 0.883, well inside the 0.7 floor limit, so it is GROUND for walking and sliding), 4.5 m run,
  2.4 m rise. Faceted in 7.5 deg steps around the end caps (24 facets per cap, about 1.8 m a facet at the lane edge).
- **Rim**: vertical (normal.y = 0, wall-ridable). On the 24 m straights it is the wall-ride lane (a ride lasts 1.2 s; at
  16-20 m/s that is 19-24 m: the straight is the right length). On the caps it is a plain wall (you glance off it; no ride
  design depends on it).
- **Island**: 2.6 m tall solid (blocks sight and the boss's line, so you cannot shoot across), TWO 4 m wide doors at the
  middle of each side. A door is the "cut": it turns the track into a figure with a shortcut. A cyan marker ring on the boss
  stays visible through the island so it is never a speck (R2 rule 12).
- **Facet safety**: every adjacent facet pair differs by <= 7.5 deg, far under the ride's 30 deg corner rule, so a glance
  along the cap does not trigger "corner" instantly. [design; to be measured by a bot glancing at the cap at 16 m/s]
- Triangles: roughly 24 cap facets x (bank 2 + wall 2) x 2 caps + straights + island + floor ~ 450 triangles. Brute-force
  candidate gathering in `TriangleWorld` is fine at this size [design; measure the step time].

### 3.2 The ONE ice rule (answers "the entire floor, inconsistent")

- ONE surface zone: a rect covering the whole rink bounding box (x/z beyond the rim, y from -1e9 to 1e9), kind `ice`, in the
  map's `surfaces` list. No other zone in the arena, no boost strips on the lane (the boost lives in the bank: gravity).
  Because `Surfaces26.FrictionMult` returns the first zone containing the player, banks, island top and lane all read the
  same multiplier. [read: `Surfaces26.Contains` is a rect + y range test.]
- **Ice friction multiplier: 0.25 (the existing `design.n26.surfaces.iceFrictionMult`, unchanged); try 0.15 if it still does
  not read as ice.** At 0.25 the ground friction is 6 x 0.25 = 1.5; stop speed 2.5. At 0.15 it is 0.9.
- **The steering gap** (Mover fact 5): friction alone does not make steering slidy because ground `accel` is 10. A second
  multiplier `groundAccelMult` (start 0.4) would be the second half of "skating". That is a `Mover.cs` change, which
  `CLAUDE.md` rule 3 and NIGHT27 forbid except by a named task with GG's say. I list it as a DECISION for GG, not as part
  of the base design. The base design works without it, using the banks and slide for the skating feel (the slide ignores
  the ice multiplier anyway, which keeps the slide the same on every square metre).
- **A test makes the rule hard** (build it in the first hour): for a 1 m grid across the arena, the multiplier at every
  FLOOR-grade point is exactly the ice value; one slide-and-stop at 6 sample points (lane, bank, island, door) stops in the
  same distance +-5 %.

### 3.3 The boss: THE GLACIER (original blocky skater, one blade, the NIGHT26 glow / trail / aura effects kept)

State machine (pure C# in `Skygrind.Core`, UnityHFSM pattern, timers in whole steps at 120 Hz, rolls from Mulberry32,
telegraph >= 0.5 s). Reuse what exists: `Glacier27` (gap-based elastic speed, the window and miss-cost rules, three-hit
phases) and its `Glacier27Body`; replace the straight track with a loop-aware arc coordinate s in [0, 84).

**Movement rule.** The boss skates the centre line (it may use the bank on caps to look alive; sim position stays on the
centre line, view-only wobble). It FLEES along the shorter arc from the player (not a timer). Base speed 9.5 m/s;
`speed = base + k x (gap - targetGap)` clamped to +-15 % of the slide floor 11.6 m/s (+-1.7 m/s), so clean sliding closes the
gap (the player gains about 2 m/s) and mistakes open it. Gap measured along the arc, signed, both directions.

**The glaze (the reason for the pistol).** The boss wears an ICE GLAZE (cyan-lit plates, since cyan = player-usable). While
glazed, a PULSE hit clangs off (feedback only, costs 8 m of gap: the boss accelerates away for 1.5 s; never health). The
glaze is cracked by gun fire at range: 8 pistol hits, or 1 charged pistol shot (RT hold, pierces 3 per NIGHT27), or the
shotgun's charged blast within 10 m. Once cracked it stays open 5 s, then re-glazes; a PULSE hit during the open time lands
and takes one third of the boss (phase advances). The loadout is therefore pistol + shotgun + gauntlet, with the gauntlet the
only thing that does damage. (NIGHT27 / GG said "only the pulse" for level 2: this widens it by one role; decision D-2.)

**The catch.** Pulse swing range 2.4 m, cone 35 deg [read: `weapons.gauntlet`]; N27's 2.5 s heavy swing and 5-kill LT blast
(0.4 s self-boost, 15 s+ cooldown) are from the NIGHT27 brief, not measured. Window rule R2: when the gap is under 12 m
and the glaze is open, show PULSE-READY and give a 1.5 s window; a launch or a dash-plus-swing that lands counts; a miss
costs 20 m of gap, never health. The charge for the LT launch comes from KILLS of FROST HUSKS that the RINK spawns between
phases (4-6, from floor cracks): the arena's own wildlife, not boss-dropped sentries, so it is not the Lantern's P3 trick.

**The reversal window (the cut).** If the player enters an island door and comes out ahead of the boss, the boss has to
reverse. It brakes (a visible 0.7 s skid, telegraph) and turns: the glaze plates stay lit and the 5 s open time is not
required for THAT window: a pulse hit during the skid counts even when glazed, once per phase. The player caused it with
a movement verb (the cut). No timer opens it. [design]

### 3.4 Phases, attacks and what each one forces

| Phase | Time (s) | Boss attack (telegraph, then effect) | Forced movement | Weapon beat |
|---|---|---|---|---|
| Intro | 0-12 | none. Rink reveal; 5 frost husks crack out of the lane; the pips fill | slide + swing | gauntlet kills fill 5 pips |
| P1 LAPS | 12-52 | SHARD TRAIL: 3 m ice shards seeded on the racing line behind the boss; floor decal 0.8 s (96 steps), shard lasts 6 s | take the BANK: slide down it to carry speed past the shards and to close the gap (about +5 m/s over 12) | pistol cracks glaze at range, slide-chase, pulse |
| P2 THAW | 52-92 | MELT STRIP: a 16 m stretch of a straight's lane turns to meltwater: decal 1.5 s, live 8 s, 12 HP/s, 20 s spacing (the NIGHT27 hazard rows) | WALL-RIDE the rim straight over the strip (a 16 m strip is about 1 s at 16 m/s, inside the 1.2 s ride); the boss is on the rim side's far lane, so you ride toward it | shotgun / charged shot to crack, ride-launch the pulse |
| P3 BREAKUP | 92-132 | ICE COLUMNS (2 x 2 m, 4 m tall) rise in a slalom, decal 1.0 s; ROLLER (1.6 m boulder along a straight, decal 0.6 s, jump or climb the bank); LOW BAR across a lane (slide under, decal 0.7 s); the island splits into a 10 m CHASM the boss leaps | dash-slalom, jump the roller, slide the bar, LT LAUNCH over the chasm (the dash is 6 m, a launch about 10 m: the only crossing) | glaze cracks faster (5 hits); the final window is a launch onto the leaping boss |
| Kill | 132-138 | hitstop + slow-mo (the existing impact freeze); the kill shot is the verb | | |

Total fight about 135 s (inside 90-150); a failed catch costs time and gap, never HP. Each phase teaches before it tests: P1
uses only the slide and bank; P2 adds the wall-ride on a straight with a safe first strip (no damage on the first
appearance, 30 s into P2 the damage turns on: the teach-then-test order of R2); P3 adds the launch only at the final
window (never a new mechanic as the first thing in the hardest phase).

**Why every attack asks a different movement:** shards = take a line (bank); melt = leave the floor (rim ride); columns =
slalom (dash); roller = hop; bar = slide; chasm = launch. In the old fight the lane attacks were three lane dodges on one
track.

### 3.5 How it differs from the Lantern and from other levels

| | THE LANTERN (level 1) | THE GLACIER (level 2, this design) | Level 3 Kiln + worm | Foundry / tutorial |
|---|---|---|---|---|
| Boss moves? | hovers in the centre, shield turns | skates a loop and flees, reverses when cut | coils through an arena | Hulk stays |
| Arena | round courtyard of straight wall segments | stadium oval, banked ends, an island with doors | boxy rooms 32 x 58 | halls |
| Verb that opens the window | wall-ride toward a lit face | cut an island door; crack the glaze, then catch and launch | not this design's concern | |
| Wall ride | the core gate | an ESCAPE over thawing floor, on straights only | | tutorial lesson |
| Weapon beat | any gun while the face is lit | gun cracks, gauntlet hits | | |
| Surface | normal | one ice sheet, the whole arena | | |

### 3.6 Starting numbers in one place (all `test`; every gameplay number goes in a Unity-side config, e.g.
`design.n28.glacier.*`, with a `_why` saying the web has no such number)

| Item | Start |
|---|---|
| Rink outer | 50 x 26 m, caps R 13 m, straights 24 m, island 30 x 6 m, 2 doors 4 m wide, lap about 84 m |
| Bank | 28 deg, 4.5 m run, 2.4 m rise, facets 7.5 deg |
| Rim wall | 5 m vertical, lit cyan strip on the straights, pale violet-white boundary line |
| Ice | iceFrictionMult 0.25 (try 0.15); ONE rect over the whole arena |
| Boss speed | base 9.5 m/s, clamp +-1.7 m/s, targetGap 28 m (start gap 30 m) |
| Window | open when gap < 12 m and glaze open; 1.5 s; miss cost 20 m of gap; clang cost 8 m |
| Glaze | 8 pistol hits or 1 charged shot; open 5 s; reglaze after |
| Telegraphs | shards 0.8 s (96 steps), melt 1.5 s (180), columns 1.0 s (120), roller 0.6 s (72), bar 0.7 s (84) |
| Melt strip | 16 m, live 8 s, 12 HP/s, spacing 20 s |
| Fight length | about 135 s (intro 12, P1 40, P2 40, P3 40, kill 6) |
| HUD | gap tape in metres (signed, both ways) + cyan marker ring; no screen-border speed lines (CORE.md) |

### 3.7 Build order (each step has a measured gate)

1. Rink geometry (arc-to-triangle generator in Core) + the one ice rect + the uniform-friction test above. Gate: 0 golden
   mismatches, 16 replays bit-exact (Core touched), friction grid test passes.
2. Bank physics probe: a bot slides down the bank at 28 deg from rest and from 12 m/s; measure the gain (expect about +5
   to +11 m/s, a hypothesis). Glance test: a bot skates a cap at 16 m/s; speed kept and no `corner` end every facet.
3. Boss on the loop with the gap-based speed + the island-door reversal; a pilot bot measures catch time (target: a competent
   bot catches in 20-45 s; if under 10 s or never, retune k and base speed).
4. Glaze and window; then P1 shards; P2 thaw; P3 columns, roller, bar, chasm.
5. One Player reel scenario per phase (L-014), judged blind and paired (L-001); a reel of "the cut" reversal.

---------------------------------------------------------------------------------------------------------------------

## 4. Risks

1. **Dead-band contact** between the 28 deg bank and the vertical rim (a 62 deg inside corner): untested; if the capsule
   sticks or snaps oddly, add a 0.4 m flat lip. [not checked]
2. **Glancing along the curved cap** may end `wallRide`/lose speed more than expected, or the capsule may jitter across
   7.5 deg facets. Only a bot run can tell. [design]
3. **Bank slide is the only speed engine** (Mover fact 3): if the measured gain is small the chase becomes a walk. Fallback:
   add short boost strips on the straights (existing `Surfaces26` BOOST, +6 m/s) and lower the boss to 9 m/s.
4. **Steering is not slidy** without a `groundAccelMult` (Mover is frozen). The rink may read as "slippery stopping" only.
   Decision for GG, D-1.
5. **Loop chase logic** (boss flees the shorter arc, reverses when cut) is new AI; a bug could make it oscillate at the
   antipode. Needs a hysteresis (flee direction changes only when the other arc is shorter by more than 8 m).
6. **Island sightlines** make the boss a speck when it is on the far straight; the marker ring and gap tape carry it.
7. **Map format**: arbitrary triangles / arcs may not be expressible in map JSON [not checked]; adding a primitive touches
   Core and its exactness gates.
8. **Pistol-for-crack** widens GG's "only the pulse"; if GG rejects it, glaze becomes a timer (bad: boss opens itself),
   so the fallback is the simpler design below.
9. **Scope**: arena + boss + 5 attacks is a large build; the simpler design is the fallback in section 6.
10. **Time-of-flight telegraph accuracy** at skating speeds: decal timing in whole steps, but players moving 15 m/s cover
    12 m in the 0.8 s tell; the shard is placed ahead of the boss (behind it on the player's side), not on the player.

---------------------------------------------------------------------------------------------------------------------

## 5. Three rejected alternatives (one line each)

1. **A true bowl / half-pipe boss ridden for speed:** rejected because the wall ride is a straight line that ends at 1.2 s and
   ground carving has no slope gravity (Mover facts 2-3), so the verb that makes a half-pipe fun does not exist here.
2. **Keep the straight 40 m tile chase and rotate the tiles for a real curve (R2 Option B):** rejected because rotated
   colliders touch Core's exactness gates and a closed static oval gets a real curve with none of that.
3. **An orbiting hover-boss on a round rink (the Lantern with ice):** rejected because it repeats THE LANTERN's shape and
   verb, which is exactly GG's "repeating similar elements".

(Also noted, not pursued: a grind-rail boss, because the Mover has no rail verb.)

---------------------------------------------------------------------------------------------------------------------

## 6. The SIMPLER boss (GG offered), in order of how little it costs

**S1, the minimum that still answers GG's complaints: THE ZAMBONI LAP.** The same oval rink with the one ice rect and the
banked ends, but no island doors, no glaze, no pistol role and no phases 2-3 mechanics. The boss is a big blocky ice-machine
driving fixed laps at 9 m/s; one attack (SHARD TRAIL, 0.8 s tell); you catch its glowing rear with the PULSE swing (gap rule
as above, miss costs gap); 3 hits kill; fight about 75 s. It uses the rink, the one rule and the bank slide, drops the
reversal AI, the thaw and the column phases. Build cost: the arena work (step 1-2) + the existing `Glacier27` rules on a loop.
It fixes: no real curve, partial ice, boring straight lane dodges. It does not fix "unique gameplay" beyond the bank slide.

**S2, an even smaller cut: THE BERG.** A stationary ice-giant on a plinth at one end of the rink; the ice floor and bank are
the puzzle: you slide down the bank and launch off the lip (a 20 deg wedge ramp from the web) to reach its glowing core with
a pulse swing; it fires one shard volley per phase. No chase AI. Fight about 60-75 s.

**Not recommended:** a re-skin of the straight track with ice painted on everything. That fixes "partial ice" and nothing
else.

---------------------------------------------------------------------------------------------------------------------

## 7. Decisions for GG

- **D-1** One new `Mover` multiplier, `groundAccelMult` (start 0.4 on ice), so steering on ice is slidy? (Default: no; the
  base design works without it.)
- **D-2** Pistol and shotgun allowed on level 2 to crack the glaze (the gauntlet does all damage)? (Default: yes; otherwise use
  the Zamboni Lap, S1.)
- **D-3** Full design (about 135 s, three phases) or S1 first (about 75 s) and the rest later?

## 8. Sources

Read: GG request and `NIGHT27.md` brief; `R2-bosses-chase-ai.md`; `Mover.cs`, `MovementMath.cs`, `Surface26.cs`,
`SurvivalSim.Surface26.cs`, `TriangleWorld.cs`, `PortExport/config.json`, `Design/unity-config.json`, `glacier-course.json` keys;
https://itch.io/blog/772886/game-analysis-of-core-systems-furi ; https://github.com/cmacmillan/CurveDesigner ;
https://rlgym.org/Cheatsheets/game_values ; https://kenney.nl/support .
Search snippets only: ULTRAKILL boss review pages (intothebluesky.com, steamcommunity.com threads); Jet Set Radio grinding
(jetsetradio.fandom.com, wikipedia); SSX and half-pipe design (gameinformer.com, giantbomb.com); Trackmania banked turns
(item.exchange); skatepark transition radius (surfertoday.com, strayrocket.com); NHL rink and short-track dimensions
(Wikipedia "Ice hockey rink", "Speed skating rink"); ice physics in games (itch.io devlog, forums); InstantPipes, Catlike
Coding swirly pipe.
Could not verify: Titanfall 2 / Doom Eternal / Hades / Sonic / Mirror's Edge boss or arena design specifics; any licensed bowl
or rink level data; Quaternius / OpenGameArt / Poly Haven licence text (not opened); where level 2's plan paints its ice.
