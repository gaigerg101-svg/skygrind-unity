# The four bosses, restarted from scratch (2026-10-12; GG: "not traditional ... a consistently moving battle")

Sources: research/boss-web.md (web + GitHub, every claim with its URL), research/uk-bosses.md (ULTRAKILL's boss AI, decompiled: numbers
and rule shapes only, file:line there), the engine audit (below), GG-REQUEST.md. THE KILN WYRM (level 3) is kept as it is (GG). Every
number below is a starting point for config (`design.n37.<boss>`), tuned on Player frames and the bot band.

## What the engine can do today (audit, file:line in the audit notes)
- Any wall with |normal.y| < 0.3 can be wall ridden; curved walls work (Lantern / Crown rings: 64 facets, a clean ride lap measured).
- World layers can be refilled every step (Devices33 does it); a rider standing ON TOP of a moving layer is carried; a wall rider is
  NOT carried (new Mover work if we want riding on moving walls: we design around it, see "engine work").
- Grapple nodes can move with a boss (THE LOOM's joints already rewrite map.grappleNodes each step; the rope follows).
- Arena changes mid-fight exist: sectors dropping into lava (Crown32), platforms sinking (Lantern31), a deck breaking (Loom31),
  perches crumbling (Wyrm31).
- Boss bodies are not solid today (the Wyrm passes through you). Collision queries scan each layer linearly: keep moving layers small.

## Shared rules for all four (the Boss37 shape: an evolution of tonight's Boss36Brain)
1. **The fight travels.** Each boss is a set piece through a space (a climb, a race, a crossing, a descent), entered by walking in
   (walkin36) and split into 3 legs; each leg changes the arena (ULTRAKILL Leviathan: the arena changes at each third). No arena is a
   flat room you can stand in.
2. **Movement is the only way to deal damage.** The weak point is only open to a player who is moving the level's way (riding, skating,
   pulled by the grapple, mid jump-dash). Standing still is never a damage position.
3. **Short fights, earned windows, no health sponges.** The boss has a STAGGER meter (Minos Prime: 6 stamina in phase 1, 12 in phase 2,
   a lockout when empty): the right move at the right time drains it; empty = a 2-3 s cyan window where the core takes full damage.
   Target: bot 15-22 s, a person ~40 s (GG w31-w32: "way too tanky" never again).
4. **Attack budget and rhythm** (ULTRAKILL MinosPrime.cs:342-353; Gabriel.cs:132-142): each attack costs 1-3 points; at the cap the
   boss must rest 2 s (the breather where you reposition); 2 attacks chain, then a rest. **A parry refunds 5 points** (MinosPrime.cs:1125)
   and drains stagger: parrying buys you time.
5. **The next attack** is a weighted pick with a pity bonus (+0.25 per unpicked turn, never the same attack twice; Gabriel.cs:74-128),
   gated by distance bands, with **habit reading** (Minotaur.cs:294-296: if you've stayed on the ground ~8 s it denies the ground; in
   the air, it denies the air): the boss pushes you to change how you move.
6. **No hiding** (GabrielBase.cs:355-363): out of reach, out of sight or below it for 3 s fills a timer and the boss repositions onto you.
7. **One colour per answer**: YELLOW = parry it, RED = dodge it, CYAN = your window (the open core). Wind-ups 1.0-1.5 s, one at a time.
8. **Fail forward.** A fall or a death restarts the current leg, not the fight (the leg's checkpoint, the boss's stagger kept), at a
   health cost; the boss bar remembers.
9. **Taught before it is tested.** The two rooms before each boss rehearse its movement (a short spiral climb before THE WICK, a lane
   change room before THE STAG, a node chain over a void before THE LOOM, a falling platform run before THE CROWN).

---

## L1 THE WICK (replaces THE LANTERN): a climb up a furnace stack
**The verbs it tests:** wall ride + the jump-off (Lucio: the leap's height follows your look pitch), dash, the parry, the KESTREL's
charged pierce.
**The idea:** the level ends at the foot of a smelting stack. The Wick, a living flame caged in iron plates, rises up the stack's
centre, and molten slag rises behind you. You climb by chaining wall rides up a spiral of panels, and you can only hurt it from the
walls. You never stand still: the slag is always coming.

**The arena** (the stack):
```
   top: THE CAP (a ring catwalk, r 9 m) ......... leg 3
   |  spiral panels, 3 per turn, 6 m rise per turn
   |  ledge ring (rest, 1.2 m wide) at 22 m ..... leg 2 starts
   |  spiral panels
   |  ledge ring at 8 m ........................... leg 1 starts
   floor: slag pool (kill plane rises)
   shaft: round, inner radius 9 m, 40 m tall; the Wick on the centre line
```
- Ride panels (cyan stripe, 8 m long, 3.5 m tall) climb in a spiral around the shaft; consecutive panels are 4-5 m apart and 2-2.5 m
  higher: you can't reach the next by riding (walls never lift you since wallgrip36), only by leaping off with your look pitched up
  (upHigh 9.1 m/s) and dashing. Gaps on alternate sides of the shaft make you cross the void (a jump across the centre past the Wick).
- **Rising slag:** a hazard plane that rises 0.6 m/s during the Wick's attacks and stops during its rests (always 4+ m below the last
  ledge you reached); touching it = 30 damage and a bounce out (not death), so falling is costly, not fatal.
- Ledge rings at 8 m and 22 m: the leg checkpoints; 2 s of rest each.

**The Wick** (a cage of 6 iron plates around a flame core, ~4 m tall, floats on the centre line at your height + 2 m):
- **Ember volley (YELLOW, 1.2 s wind-up):** 3 slow globs at you. Parry one back into the cage: it cracks a plate (drains stagger).
- **Flare sweep (RED, 1.4 s):** a ring of fire wipes the half of the shaft wall you're riding (the red bar shows which half): leap
  across to the other half.
- **Plate slam (RED, 1.0 s, only if you're on a ledge > 3 s):** the habit rule: a plate drops on the ledge.
- **Draft (no damage):** at rest the Wick drifts up past you (the slag rises 1 m): "keep climbing".
- **Damage:** the core is behind the plates. A KESTREL charged pierce fired FROM A WALL RIDE through an open gap (a cracked plate)
  hits the core for full; anything from a ledge does 10 %. Stagger empty (2 parries + 1 pierce) = the cage opens CYAN for 2.5 s:
  every shot is full.
- **Leg 3 (the cap):** the Wick bursts out of the top and circles the cap's catwalk; the catwalk's outer wall is one long ring panel
  (ride a full lap). It dives at you (RED) and stalls at the wall (the dive's stall = its last window); a launch punch from a dash
  knocks it into the shaft and the slag. The end.

**Why it's not traditional:** the fight is a 40 m climb; its pressure is the slag, not a health bar.
**Engine work:** a rising hazard plane (new, small); the spiral is static geometry; the Wick is a flying body (no solid needed).

---

## L2 THE STAG (from scratch): a chase on a frozen canal
**The verbs it tests:** skating lanes and ice, lane changes (dash sideways), the PULSE GAUNTLET's punch and LT blast, the launch, the
parry. GG asked for a chase boss before (w24: "you're chasing the boss", "both of you are almost like trying to not catch each other").

**The idea:** the level's last room opens onto a frozen canal loop. The Stag bolts. You chase it on the ice. It's faster than you on
open ice but slower in your slipstream, and it fights back by kicking ice behind it. You win by catching it and punching it, three
times, one per lap.

**The arena** (a canal loop, ~260 m, three lanes 5 m wide each, with banked turns):
```
   ======================== straight A (90 m): 3 skate lanes =========
  //  banked turn 1 (ride the bank wall: cyan panels on the outside)  \\
  ||                                                                   ||
  \\  turn 2: the ice breaks into floes in leg 2 (jump floe to floe)   //
   ======================== straight B (90 m): lane walls, a tunnel ===
```
- Skate lanes (SkateLane29) with lips; banked turns whose outer walls are ride panels (ride a turn instead of skating it: faster).
- **Slipstream:** within 12 m behind the Stag your skating speed gains +25 % (a cyan streak shows it): the catch-up mechanic.
- Leg 2: the canal on straight B cracks into floes (Crumble31-style drops by lane, telegraphed RED a lane ahead): you jump floe to floe
  while chasing. Leg 3: the Stag turns at the tunnel and charges back head-on down your lane.

**The Stag** (a long-legged ice beast with a crown of antlers, body ~3 m tall, its legs readable as targets):
- **Shard kick (RED, 1.0 s):** a spray of ice shards down one lane behind it: change lanes (a sideways dash).
- **Ice chunk (YELLOW, 1.2 s):** a big chunk lobbed back at you: punch it back into the Stag (a stagger and a stumble).
- **Lane cut (RED, 1.4 s):** it rakes its antlers across a lane wall, dropping a barrier: jump it or ride the wall past it.
- **Kick back (habit: if you sit in its slipstream > 4 s):** a two-hoof kick straight behind: the slipstream isn't free.
- **Damage:** catch it (within 4 m) and punch a hind leg: a stumble (stagger -2). Stagger empty = it falls and skids CYAN for 2.5 s;
  a launch punch into its side drives it into the canal wall (a burst + full damage). Guns do 10 %.
- **Leg 3 finale:** it charges straight at you down the lane: a YELLOW antler charge (1.5 s wind-up, the biggest parry in the game):
  parry it with the gauntlet and it crashes into the tunnel wall; its antlers break. The end.

**Why it's not traditional:** it's a race; you damage it by catching it.
**Engine work:** the Stag follows a path (a spline of nodes); the slipstream rule; floes = per-lane layer drops (existing pattern).

---

## L4 THE LOOM (from scratch): a fight with no floor
**The verbs it tests:** the grapple's pull (straight, constant, to what you hit), the whip, the BEND (slow motion on a latch), the
STITCHER (nails stick, LT detonates), wall rides on towers.

**The idea:** the span ends at a canyon. THE LOOM, a huge weaving machine, hangs on threads strung between stone towers, and it is
weaving the canyon shut. There is no floor: only towers, thread anchors (grapple nodes) and the Loom's own body. You cross the canyon
on the grapple while it travels ahead of you, and you kill it by nailing its joints and pulling away so the nails tear (ULTRAKILL 7-4:
the boss is the level).

**The arena** (a canyon 120 m long, three tower pairs, a void below):
```
   tower pair 1 ----threads----> tower pair 2 ----threads----> tower pair 3
   (anchor ledge: start)          (leg 2)                        (leg 3: the Loom's nest)
   void below (fall = back to the last anchor ledge, -20 HP)
```
- Towers: tall, ridable walls (cyan stripes) between grapple anchors: pull, ride, pull.
- Thread anchors are grapple nodes; the Loom's 6 leg joints are MOVING grapple nodes (already supported).
- **The Loom cuts threads behind you:** nodes go dark 3 s after it passes (RED pulse first): you must keep moving forward.

**THE LOOM** (a body slung between six long legs; each knee a glowing joint; the body underneath a shielded spinneret):
- **Thread lash (RED, 1.2 s):** a thread whips across the span: be off the line (pull to another node).
- **Spool shot (YELLOW, 1.2 s):** a weighted spool at you: parry it back (or whip it).
- **Web wall (RED, 1.4 s):** it weaves a sheet between two towers: the route changes; go around (ride the tower).
- **Hide rule:** if you stay on one tower > 4 s, it severs that tower's anchors.
- **Damage:** nail a knee joint (nails stick), then pull yourself AWAY with the grapple (or detonate with LT): the nails tear the
  joint (stagger). The BEND fires on the latch: nail the joints in slow motion. Every 2 joints torn = the leg pair goes limp; at
  stagger empty the spinneret opens CYAN: whip onto the body and unload.
- **Legs:** leg 1 (tower pair 1-2): learn; leg 2: it moves faster and cuts more threads; leg 3 (the nest): it turns to fight,
  threads everywhere, its last two knees: tear them and it falls into the canyon.

**Why it's not traditional:** there's no floor and the fight crosses a canyon; the grapple IS your footing.
**Engine work:** threads as moving nodes (exists); thread-cut node removal (new, small); the web wall as a layer swap.

---

## L5 THE CROWN (from scratch): a descent of a falling spire
**The verbs it tests:** the jump-dash (L5's verb: distance and height), every weapon (the TORCH earned here burns at close range),
platforming on moving platforms.

**The idea:** you reach the top of the spire and the Crown, a halo of gem-set blades, shatters the summit. The spire starts to fall
apart, and the fight is a descent: down its outside, across its falling pieces, with the Crown circling and cutting the path ahead.
Going down is fast and dangerous: the opposite of L1's climb.

**The arena** (the spire, 70 m tall; the fight goes top to bottom):
```
   summit (start, the shatter) ..... leg 1: the crown ledges (rings orbiting the spire)
   mid: the gallery (a spiral ramp on the outside, broken in places) ..... leg 2
   base: the plaza (the Crown comes down to fight) ..... leg 3
```
- **Orbiting ring platforms** (moving layers that carry you while you stand on them: supported today): fragments of the summit
  orbiting the spire at 3 heights; jump-dash between them on the way down.
- **The gallery:** a spiral ramp with gaps (jump-dash across), ride panels on the spire face, sectors that drop (Crown32's pattern)
  when the Crown's beam cuts them.
- **The plaza:** a round floor whose outer sectors drop into the void one by one.

**THE CROWN** (a ring of 8 gem-set blades around an empty centre, ~6 m across, spinning; each gem a weak point):
- **Blade ring (RED, 1.2 s):** it spins out to cut a full ring around the spire at your height: drop or jump-dash over it.
- **Gem shard (YELLOW, 1.0 s):** it flings a gem shard at you: parry it back to shatter a gem socket.
- **Beam cut (RED, 1.4 s):** a beam cuts the next platform you're heading to (shown RED first): pick another.
- **Habit rule:** if you stay airborne > 6 s, it fires low; if you stay on one platform > 3 s, the platform drops.
- **Damage:** the gems take damage only at close range: jump-dash THROUGH the ring's centre (a cyan flash = a pass) and the TORCH
  burns the gems you pass. 8 gems; each pass breaks 2 at most. Stagger empty = the ring stops spinning CYAN: anything works.
- **Leg 3 (the plaza):** the Crown lands, plaza sectors drop around you; last 2 gems; the THUNDERCLAP slug or the KESTREL pierce
  through its centre ends it.

**Why it's not traditional:** a descent through a collapsing tower; the damage comes from flying through the boss.
**Engine work:** orbiting platforms (the Devices33 mover pattern, extended to a ring path); sector drops (exist); the Crown as a
non-solid ring body with a pass test through its centre.

---

## Level 6 (not one of the four)
Today level 6 repeats THE STAG. With THE STAG rebuilt as a chase, level 6 needs its own finale: DECIDE36 D-4's THE RIVAL (a magenta
V2-style mirror that rides, grapples and parries like you), or a boss gauntlet of all five on a short course. GG decides (D-37-1).

## What GG may be missing (each one a decision or a task)
1. **Level 6's boss** (above).
2. **The rooms before each boss** must rehearse its movement (rule 9): each level gets a new or rebuilt pre-boss pair of rooms.
3. **Checkpoints inside a fight** (rule 8): a fall restarts the leg, not the fight.
4. **Readability while moving fast:** each boss has one cyan weak point and a silhouette that reads at 30 m; pad aim assist on a moving
   target (light, never on the core) is a tuning question.
5. **Motion comfort:** constant movement + a moving boss + slow motion can be a lot; keep camera roll small, a "reduce motion" option
   already exists (slowMoOn).
6. **The bot pilots:** each boss needs a pilot that can play it (climbing, chasing, grappling, descending) or the 15-22 s band can't be
   measured. This is real work per boss (the climb and the chase are the hardest to script).
7. **Switch performance:** moving layers are scanned linearly: keep each moving layer under ~50 boxes.
8. **Sound:** every attack's wind-up and the stagger break get event names for GG (no audio code).
9. **Scope:** four bosses from scratch, each with a travelling arena, is about 4 nights of builder work at tonight's depth (one boss +
   its pre-boss rooms per night), or 2 nights at two each with less polish. Recommended order: THE WICK (level 1 is already rebuilt),
   THE STAG (with level 2's rebuild), THE LOOM (with level 4), THE CROWN (with level 5).

## Engine work, all of it (each behind its row, Mover.cs untouched unless a row names it)
- A rising hazard plane (THE WICK).
- A boss path follower and a slipstream speed rule (THE STAG).
- Thread-cut node removal and moving thread nodes (THE LOOM: extends what exists).
- Orbiting platforms that carry a standing player (THE CROWN: extends Devices33).
- A stagger meter, an attack budget, the weighted pick with pity, the habit reader and the anti-hide timer in the shared Boss37 brain.
- Riding a MOVING wall is not supported and none of the four needs it (designed around it).
