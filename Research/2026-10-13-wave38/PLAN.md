# Wave 38 plan: every level overhauled for flight, walls you stay on, doors you can see, a HUD that tells you (2026-10-13)

Sources: GG-REQUEST.md; research/arena-web.md (arena + platforming design, URLs); research/uk-hud-feel.md (ULTRAKILL's HUD, shake,
hurt, walls, doors: decompiled numbers only); the code audit (file:line below). Every number is a starting point for config.

## What GG asked, in one line each, and the answer
| ask | answer |
|---|---|
| wall rides: never fall, stay at your height, aim up / down to drift | **ride38**: a held ride has no sink and no 8 s cap; the look pitch drifts you up or down on the wall (beyond ULTRAKILL, whose cling always slides) |
| doors invisible, you run into the dark | **doors38**: every doorway is a holographic curtain you can see from across the room: cyan shimmer = open (pass through), red-orange = locked (solid), the boss door its own sign; the next room lit and visible through it |
| levels: open arenas with roofs, flight rooms that force a verb and can't be skipped, no old random rooms, don't stop the pace | **levels38**: every level rebuilt on the n36 method + a **gate checker** that proves each gate needs its verb; every copied old room removed |
| THE STAG: liked; middle walls short and clunky; hard to read | **stag38**: tall, long middle walls you can ride for 3 s+; a read pass (where it is, what's coming) |
| THE LOOM polish; THE CROWN look better | **loom38 / crown38** polish passes on frames |
| charge readiness unclear; crosshair rotation more of a thing; grapple indicator bland | **hud38**: a three-segment bar under the crosshair per gun (ULTRAKILL's rule: one colour charging, another full, a white flash + an event on the frame it fills), the crosshair spins through each reload / cooldown, a grapple icon with charges in their own colour |
| boss entrance shake + flicker; the worm digging; damage = static TV | **juice38**: shake + a full-screen flicker on every boss entrance and big boss move (the worm's dig included); a full-screen static burst on damage, scaled by the hit |

## 1. ride38: walls you stay on (Mover.cs, inside the wallride35 branch: named here, CLAUDE.md rule 3)
- A HOLD ride (jump held) keeps your height: no sink (wallride35.sinkSpeed 0.1 -> 0) and no 8 s safety cap (a long cap, 60 s, only
  against stuck states). A timed ride (jump not held) keeps today's 1.2 s rule.
- The look pitch drifts you: pitch above +15 deg rises, below -15 deg sinks, up to ~2.5 m/s at +-45 deg; never above the wall's top
  minus the capsule's upper sample (Mover's contact band feet+0.35..feet+1.45), so you never pop off the top by drifting.
- Ends stay: release (the jump-off), crouch, "away" input, a corner beyond 100 deg, the wall ending.
- Check: harness (a 60 m wall: a 30 s ride holds the height within 0.05 m; pitch +45 deg rises 2.5 m/s and stops at the top; pitch -45
  sinks), the Player reel (frames at 0 / 5 / 10 s on a long wall), the golden vectors exact with the row off.

## 2. levels38: every level rebuilt for flight
**The method (n36's + one new tool):**
1. The **envelope table** (builder E, first): for every verb and combo, the max distance and height measured in the real sim from flat
   ground and from a wall ride (jump 6.9 m / 1.5 m; double jump; dash 6.1 m; jump-dash 21 m; slide-hop 12.8 m; the shotgun boost 10 m/s
   per mid-air shot; the shockwave 12 m/s; the grapple 40 m; the wall-ride jump-off 17.2 m/s; chains of each).
2. The **gate checker** (`n38 gates <level>`): every gate in a level is tagged with its intended verb; the checker drops the real
   Mover at the takeoff and proves: the intended verb crosses with a margin (>= 10 % of its envelope), and NO other verb or combo the
   player owns there crosses (the research's rule: a gate sits between envelopes, plus ceilings under the double-jump apex and walls
   that visibly can't be ridden to block the alternatives). A gate that any one move skips is RED.
3. A beat sheet (one job per room), job tags on every solid (n36-jobs), the walk bot, Player frames per room, as NIGHT36.
**The shape of a level** (5-7 rooms, no copied room):
- Mostly OPEN ROOFED ARENAS (24-40 m across, 10-16 m roofs, at most three floor heights, the exit visible from the entrance) with
  enemies spawned ON your route, ahead of you, so fights start while you move (Doom's push-forward; Hakita: "no points where you wait").
- One or two **FLIGHT ROOMS** per level, like the platforming GG liked before the final boss: a chain of gates that each force the
  level's verb, the goal in sight, a fall costs a little and returns you to the gate.
- At most two tight rooms per level, each with a job (a parry alley, a reveal).
- Kills refill (heal36, dash back on parry) so the pace never needs a stop.
**The levels:**
| level | weapon / verb | flight room gates (forced) | the boss |
|---|---|---|---|
| L1 FOUNDRY | KESTREL, the wall ride | keep a1-a6 (GG: "the tutorial ... clearly incentivized the movement"); add one open arena + one flight room of ride chains whose walls are farther apart than any jump | THE WICK |
| L2 ICE LINE | GAUNTLET, skating | skate-lane gates (speed you only have from a lane), ride chains over ice | THE STAG (stag38) |
| L3 KILN | THUNDERCLAP, the shotgun boost | **boost gates**: walls you visibly can't ride, ceilings too low for a double jump to clear, a ledge only a mid-air boost reaches | THE KILN WYRM (kept) + its entrance shake |
| L4 SPAN | grapple, STITCHER | grapple gates over voids too long for any jump or ride chain | THE LOOM (loom38) |
| L5 SPIRE | jump-dash, TORCH | **far dash-jump gates** with ceilings and a limited dash meter, so the dash-jump can't be spammed gap to gap | THE CROWN (crown38) |
| L6 FINALE | everything | mixed gates, one per verb, in sequence | the old Stag stays until THE RIVAL (D-1, next) |

## 3. doors38
- Every doorway draws a holographic curtain across its opening (a scrolling scanline shimmer, visible at 40 m): CYAN while open (no
  collider; you pass through it with a short shimmer on the camera), RED-ORANGE while locked (solid: today's gate field, made visible
  from afar), the boss door PALE VIOLET-WHITE with the boss's sign; the open frame lit, the next room lit behind it (Chung's cutaways).
- Fix the slab orientation bug in Arena36View (x vs z doors).

## 4. hud38 + juice38
- Under the crosshair, a three-segment bar per gun's charge / cooldown (KESTREL's charge, THUNDERCLAP's slug, the gauntlet's BLAST pips
  which are hidden today, the grapple's 2 charges): one colour while charging, cyan when full, a white flash + `hud38Ready` on the frame
  it fills (ULTRAKILL's rule, RailcannonMeter / Revolver). The crosshair's ticks rotate through each reload / cooldown (a full turn per
  cycle) and snap square when ready. The grapple gets its own hook icon with two charge pips.
- Juice: a boss entrance = a hit-stop, a shake (ULTRAKILL boss stomps 2-3, decays 3/s, the bigger replaces), a full-screen flicker
  (Crt29's strength spiked for 0.3 s) and the title card; big boss moves (the worm's dig and erupt, the Stag's crash, the Loom's fall,
  the Crown's shatter) shake and flicker; taking damage = a full-screen static / RGB-split burst for the hit's i-frame time (ULTRAKILL:
  alpha 0.5 / 0.8 and the overlay IS the i-frame timer), scaled by the damage, plus a small camera shake (ULTRAKILL: damage / 20).
  `feel.trauma.boss` exists in config and nothing reads it: wire it.

## 5. Boss passes
- stag38: lane walls 5 m tall (a 2.6 m wall holds only while your feet are under ~2.25 m: Mover's contact band), runs of 30 m+ so a ride
  lasts 3 s; ride walls on the island; a read pass (an off-screen arrow to the Stag, the lane warnings brighter).
- loom38 / crown38: one polish pass each on the n37-r7 frames (THE CROWN's look: GG "make it look a little bit better"), entrance
  shake + flicker.

## Scope (DECIDE38 D-1)
This is every level + four systems. Builders: E (envelopes + gate checker, lands first), M (ride38 + doors38), H (hud38 + juice38),
and level builders (one per level, owning its rooms and its boss pass). One night cannot finish five levels at NIGHT36's depth: the
recommended split is two nights: N38 = E, M, H + L3 (the boost level) + L5 (the dash-jump level) + stag38 with L2's walls; N39 = L2, L4,
L6 + L1's flight room + loom38 / crown38.
