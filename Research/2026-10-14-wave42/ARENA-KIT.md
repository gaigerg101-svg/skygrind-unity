# ARENA-KIT: arena elements that are not decoration (NIGHT42 K1, 2026-10-14)

GG's ask: "way better arena design for the gameplay. more elements that are not just decoration. research ultrakill, doom and
other boomer shooters and actually look at real models and images". Starting point: `research/arenas-web.md` (the forge's research).

Method. Images were downloaded with curl into a scratch folder, opened with the Read tool (it shows the picture) and described
from what was on screen. Page text was read with WebFetch (a small model returns the quotes; they were asked for verbatim). Scratch
images were deleted after. Sizes in metres were not measurable from any image: no enemy or player is in the ULTRAKILL shots, so
scale is "not checked" unless stated.

Access notes:
- `https://ultrakill.wiki.gg/wiki/Special:FilePath/<file>` returned HTTP 403 for every file. The page-listed paths work:
  `https://ultrakill.wiki.gg/images/thumb/<file>/800px-<file>` (HTTP 200, a Referer header was sent).
- Blocked, HTTP 403: doomwiki.org (Super_Gore_Nest, Monkey_bar), doom.fandom.com (Super_Gore_Nest, Monkey_bars, Jump_pad).
- No DOOM Eternal swing-bar, monkey-bar or jump-pad image was found at a reachable URL. The DOOM images below are the Wikipedia
  gameplay screenshot and the Steam store screenshots; none shows a bar or a pad.

## ULTRAKILL images (all from ultrakill.wiki.gg, path `/images/thumb/<file>/800px-<file>`)

### 1. `2-1_Bridge_Arena_1.jpg` (page https://ultrakill.wiki.gg/wiki/2-1)
Shows: a raised steel platform held up on thin pylons over a void; a city's lit windows are far below. The platform edges are pink
truss girders, no walls. A stair climbs from the foreground deck to a second level, and the middle of the upper level is a sunken
square with a translucent purple floor (glass or a lit panel; which, not checked). In the foreground sits a dark square block with
a diamond emblem on top (what it is: not checked). Cranes and a second platform stand in the background. The play space ends at
open edges, not walls.
Wiki text (read): "Various bounce pads are present under the bridge to allow the player to save themself from falling."
Lesson: the edge is the hazard and a pad under it is the safety net; a fight on a ledge over a drop gives every knockback a payoff
and every miss a way back.

### 2. `2-1_Overhead.jpg` (same page)
Shows: from high above, one long narrow blue-lit bridge leads straight to a compact cluster of platforms around a tall dark tower;
the rest of the frame is void and the city far below. The bridge is the only approach visible.
Lesson: one readable approach into a compact arena; the arena is small, the void around it is large.

### 3. `2-1_First_Tower_Low.jpg` (same page)
Shows: looking up inside a tall dark shaft; stacked ledges and pillar tops at several heights, each lit by a single small lamp; a
large white lamp hangs mid-shaft. A green chevron shape is at the top-left (what it is: not checked).
Lesson: a lamp per ledge marks where you can stand in a vertical space; the climb route reads from below.

### 4. `4-2_red_skull_arena.png` (page https://ultrakill.wiki.gg/wiki/4-2)
Shows: a tight block of sandstone stairs and walkways with green trim, several short flights at different heights, a balustrade
balcony, one tall pillar. In the middle is a square of green translucent tiles in a grid (about 3 x 3 panes as seen) set lower
than the walkways around it. Burning sand surrounds the whole structure.
Wiki text (read): "An outdoor arena, consisting of a main set of tight-knit stairways and platforms leading down to the Red Skull
Key covered by a glass case, with a pit covered by breakable glass panes."
Lesson: the hazard sits under the fight's centre, covered by something that breaks; the floor you stand on is a timer.

### 5. `4-2_ruin_arena_3.png` (same page)
Shows: a stepped stack of sandstone platforms rising out of the sand: a low wide base, a balustrade, a ramp, then a roofed upper
block on columns. Three or four floor heights in one compact pile; sand on every side.
Lesson: floor planes stacked in one small footprint, linked by ramps and stairs, with the hazard as the floor around them.

### 6. `4-2_insurectionist_boss_room.png` (same page)
Shows: four low platforms with sloped green-tiled roofs (hipped, walkable-looking) in the sand, split by open sand lanes; a small
blue pedestal on the left platform, a red one on the right; an exit building at the far end; a high wall on the left.
Wiki text (read): "An open air arena featuring four sandstone platforms, a Terminal, the Blue and Red Skull Pedestals, and the
exit elevator."
Lesson: islands with a hazard between them force a hop every time you move; the boss arena is just four safe spots.

### 7. `4-2_start.png` (same page)
Shows: the player on a tiled slab; ahead, several separate low slabs and a pillar stand out of shimmering sand, with a wall on the
left; the path is the gap pattern between slabs.
Wiki text (read): "Touching the sand deals 10 DP to the player."
Lesson: teach the hazard in a calm space first (the level opens on it before any arena uses it).

### 8. `0-1_grinder_hallway.png` (page https://ultrakill.wiki.gg/wiki/0-1)
Shows: a narrow corridor whose middle section is a glass walkway; below the glass the floor glows red; pink steam vents from the
side walls; a door with two green lights at the end.
Wiki text (read): "A corridor housing a glass walkway situated over a hazardous pit of grinders."
Lesson: show the grinder before the fight that uses it; the player walks over it and sees it lit red.

### 9. `0-1_turbine_chamber_arena.png` (same page)
Shows: looking down a tall chamber; translucent orange glass walkways in a zigzag with truss railings, panels at several heights,
a ladder at the bottom; the turbine itself is not clearly visible in this frame.
Wiki text (read): "A tall chamber with a large turbine fan at its bottom, with a glass walkway suspended above."
Lesson: glass walkways let the player see the hazard under the fight at all times.

### 10. `0-1_projectile_arena_overhead.png` (same page)
Shows: a long room with a raised central walkway running its length, two statues at its far end, a door beyond; on each side, a
lower trench holding blocky stacked shapes (cover-sized; their purpose: not checked).
Wiki text (read): "A large room with a small walkway encircling its middle segment."
Lesson: a raised spine with lower pits on both sides gives two floor planes and a reason to drop and climb.

### 11. `0-1_glass_floor.png` (same page)
Shows: a short boxed room; the floor looked like wood-brown panels in this frame, the glass was not distinguishable (not checked
from the image). Wiki text (read): "A small room with a glass floor housing a door underneath. The player is instructed to use the
Piercer Revolver's alternate fire to break through the glass."
Lesson: one room per verb: the breakable floor is taught alone before the arenas use it.

## DOOM images

### 12. Wikipedia `File:Doom_Eternal_gameplay_screenshot.jpg`
URL: https://upload.wikimedia.org/wikipedia/en/e/e5/Doom_Eternal_gameplay_screenshot.jpg (file page says source Eurogamer, purpose
"To illustrate the game's platforming gameplay").
Shows: floating chunks of wreckage and rock over a red void; a circular yellow marker sits on one chunk; a large glowing
yellow-green mass fills the upper middle. No bar or pad visible.
Wikipedia text (read): "In combat arenas, he can make use of swing bars, portals, and jump pads to reach distant areas."
Lesson: traversal targets carry a bright marker that reads at distance.

### 13. Wikipedia `File:Screenshot_for_Doom_2016.jpg`
URL: https://upload.wikimedia.org/wikipedia/en/b/b7/Screenshot_for_Doom_2016.jpg
Shows: a rock-walled outdoor clearing, a red demon close in front, a small green pickup at the right edge, a flat dirt floor.
Lesson: a flat canyon floor; the pickup sits at the edge of the space, away from where the player stands.

### 14. Steam store screenshots, DOOM Eternal (app 782330), 9 images
URLs: `https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/782330/ss_<hash>.600x338.jpg` (hashes from
`store.steampowered.com/api/appdetails?appids=782330&filters=screenshots`). Seen: the helmet close-up, close combat in corridors
(Cacodemon, Mancubus chainsaw, Baron, flamethrower on zombies), one vista of a ruined city with lava falls. None shows a bar, a
pad or an arena layout. The TechRaptor Super Gore Nest guide header (`de_3.jpg`) is key art, not a level.

## ULTRAKILL Style: hazard bonuses (https://ultrakill.wiki.gg/wiki/Style, quoted as read)

| Bonus | Points | Text |
|---|---|---|
| Fall | 80 | "Make any enemy fall down a bottomless pit" (corpse: 20) |
| Splattered | 100 | "Kill an enemy through fall damage" |
| Terminal Velocity | 350 | "Make an enemy fall until its falling duration exceeds a time limit, without accelerating" |
| Crushed | 80 | "Crush any enemy with any environmental crushing hazard" (corpse: 20) |
| Minced | 80 | "Mince any enemy with any environmental grinding hazard" (corpse: 20) |
| Shredded | 80 | "Kill any enemy with any environmental shredding hazard" (corpse: 20) |
| Heavy Light | 80 | "Crush any enemy with a chandelier" |
| Roadkill | 80 | "Drop any enemy onto the tracks in 7-1, or run over any enemy with a moving tram in 2-4 or 7-2" (corpse: 20) |
| Boiled | 80 | "Drop any enemy into the River Phlegethon in 7-2" (corpse: 20) |
| Long Way Down | 80 | "Drop any enemy into the ashen wasteland in 7-4 or into the City of the Dead Sun in 8-1" (corpse: 20) |
| For Thee | 80 | "Crush any enemy with the falling clock tower in 7-2" (corpse: 20) |
| Zapped | 80 | "Zap an enemy with the zap zone in The Cyber Grind" (corpse: 20) |

The pattern: every environmental kill of a live enemy is 80, a corpse 20, fall damage 100; the one long-skill bonus is 350.

## What SKYGRIND takes

1. **THE GRINDER (element 1).** A pit / grinder hazard enemies can be knocked into; the enemy dies, style "GROUND" for 80 (the
   ULTRAKILL live-enemy value: Fall, Minced, Crushed are all 80), event `arena42Ground`. From images 4, 8 and 9: put it under or
   beside the fight, visible through or past the floor, glowing red-orange (danger colour, CLAUDE.md rule 8), and show it once on
   the approach before the fight uses it (image 8). The value 80 is a new gameplay number: it goes in the Unity-side config
   (design.n42.arena42), not in code.
2. **Breakable cover (element 2).** `cover42` boxes with hp that shatter. From images 4 and 11: what you stand behind or on is a
   timer; it breaks under fire, so standing still ends. Keep boxes small (one player wide, below head height) so they buy a
   breath, not a camp.
3. **Launch pad over a fight (element 3).** The existing aimed jump pads: `pads` [{x,y,z,target:{x,y,z},apexAbove}]. From images 1
   and 2 (2-1's pads "save themself from falling"): one pad at the hazard's edge that throws the player across the fight to the
   far floor plane; the miss into the pit area has a way back. No new pad code; placement only.
4. **Swing bar / grapple point: CUT tonight.** docs/PRINCIPLES.md rule 4: "the grapple at level 4 or 5, never earlier", and
   NIGHT42.md's cut order cuts K1's fourth element first. DOOM Eternal's bars were read in text only (Wikipedia, the ludo.guide
   walkthrough in arenas-web.md); no image of one was reachable.

### Placement rules for level 1
1. At most three floor planes (book.leveldesignbook.com, verticality; image 5 shows three or four in one pile, which is the upper
   limit, not the target).
2. Enemies keep a climb route to every plane the player can stand on (CORE.md: "There always really needs to be a route that
   enemies can climb up"); the grinder must not cut the only route.
3. Pickups as routing: place health and ammo on the far side of the grinder or at the pad's landing, so taking them crosses the
   fight (adamatomic post in arenas-web.md; image 13's pickup sits at the edge).
4. The hazard edge is where the fights happen: spawns and the arena's centre within one knockback of the grinder, never in a
   corner the player won't visit (images 4 and 6: the hazard is between or under the safe spots).
5. Cover small and breakable, two to four boxes per arena, none of them on the grinder's edge (that edge is for enemies).
6. Teach before use: the grinder is seen (walkway or window over it) before the first fight that uses it (images 8 and 11).
