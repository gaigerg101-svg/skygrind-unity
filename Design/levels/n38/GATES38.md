# NIGHT38 level-format notes: gates, ceilings, noride walls, flight rooms (builder E)

These rows extend `Design/levels/LEVEL-FORMAT.md` and the n36 plan rows (`Tools/night/n36-lib.cjs`). The checker is
`Tools/SimHarness/Harness.N38.Gates.cs`; the numbers it spaces from are `Design/levels/envelopes38.json`.

```
dotnet run --project Tools/SimHarness -c Release -- n38 envelopes [--write] [--ride38]   the envelope table (--write: envelopes38.json)
dotnet run --project Tools/SimHarness -c Release -- n38 gates <plan.json> [--verbose] [--only <gateId>] [--ride38]
dotnet run --project Tools/SimHarness -c Release -- n38 gates --selftest                  planted gates: clean PASS, cheesable FAIL, thin FAIL
bash Tools/night/n38-gates.sh [<plan.json> ...]                                          every n38 / n37 plan that has a gate
```

Everything is measured on the real sim: a Survival run's SurvivalSim with the config's movement rows (flow34, wallride35,
wallgrip36, stamina; ride38 with `--ride38`), the real Loadout's THUNDERCLAP shot. The Mover's World is swapped for a box world
built from the plan. No distance is typed by hand.

## `gate` (on a box or a collider prop: the LANDING solid)

```json
{ "zone": "k2", "kind": "deck", "min": [...], "max": [...], "job": "line", "why": "...",
  "gate": { "verb": "boost", "from": [x, y, z], "to": [x, y, z], "runUp": 6, "id": "k2-g1" } }
```

- Put the tag on the solid the verb must land on (a deck, a ledge). Its top (`max.y`) is the height the checker judges.
- `from`: the takeoff point (feet) on the lip of the takeoff surface. `to`: a point (feet) on the landing solid's top. The line
  from `from` to `to` must cross the landing solid's footprint.
- `runUp` (m, default 6): clear, flat run behind `from` on the takeoff surface. The Mover starts there and sprints at the lip.
- `verb`: one of `jump double dash dashjump slide boost ride walljump grapple shockwave`. `dashjump` (alias `jumpdash`) is the
  Mover's dash jump: the dash first, then the jump inside the dash (JumpKind.Dash, the DashJump event). A jump followed by an air
  dash is not a dash jump. Other aliases: `slidehop` = `slide`, `wallride` / `ridechain` = `ride`, `shotgun` = `boost`.
- **A try's verbs are what the Mover did, not what the script pressed.** `ride` counts when a wall ride started, and a plain jump
  that brushes a ride wall rides it, even a tap ride. `dashjump` counts when a DashJump happened. `walljump` counts when a wall
  jump happened without a ride.
- **Lines:** each gate is flown on its own line and on the same line shifted 2 m and 4 m to each side. A shifted line is skipped
  when its run-up start is not standing ground in a room or the line misses the landing solid. A wall-hugging gate line cannot hide
  a ride-free line down the middle. `--lines 0` flies only the gate's own line.
- **PASS (D-6)** needs two things:
  1. Some try that uses the verb lands on the solid (grounded at its top, past its near edge) with a margin of at least 10 %.
     The margin is measured on the same try with the landing solid taken out of the world: how far past the near edge it still
     is at the top's height, divided by the gap. For a rise over 0.5 m it is also the apex over the top divided by the rise. The
     smaller of the two counts.
  2. No try without the verb lands there. The tries are every single verb and combo the level owns, at several timings and
     pitches:
     - jump; double at 0.2-0.6 s (also tapped: no hold, so no ride can start); ground dash; jump then air dash at 0.02-0.45 s,
       with a double after it or before it; slide-hop with +double and +dash.
     - The dash jump (dash 0.2-3.4 m before the lip, the jump 1-25 steps into the dash; C2's grid), and the dash jump + double
       (held and tapped).
     - Boost x1 / x2 / x3 at pitch -89 / -65 / -45 / -25 / -10 at 0.05 / 0.3 s; boost combined with double or dash.
     - The held ride: both sides, released 1-12 m before the edge at pitch -45 / 0 / +45, then a re-press for the next wall or
       the double jump.
     - The ride CHAIN: each ride let go where its wall ends (a probe 0.5 / 1.5 / 3 m ahead finds no wall), the jump pressed
       again 0.25 s later to catch the next wall on the other side or the same side, at pitch -20 / 0 / +30, to the landing.
     - Tap wall jumps.
     - Every try with a dash again with a walked run-up: the sprint drains the stamina bar, and a sprinted run-up leaves too
       little for the dash jump's extra cost.

     The grapple is tested by line of sight and range (below).
- The checker prints the gap, the rise, the verb's margin and its best try, the best other try's distance from crossing (a WARN
  within 5 %), and every cheat grouped by family. Exit 1 when a gate is RED. **Ship only GREEN gates.**

## `owns38` (top level of the plan)

`"owns38": ["jump", "double", "dash", "slide", "boost", "ride", "walljump"]` lists the verbs the player has in this level. List the
weapons the level gives or that carry in from earlier levels. Add `grapple` from the level where design.n35.grapple35.fromLevel puts
it (level 4 today). If `owns38` is missing, the checker uses every verb except the grapple and says so.
`pulse` (the PULSE gauntlet's LT blast, design.n27.pulse27: a 16 m/s self-boost along the aim over 0.4 s, one per full glove) is
owned when `owns38` lists `pulse` or `gauntlet`. On every plan that is not the foundry, the checker adds it itself, since the
gauntlet is given in level 2's j1 and carries on, and the header says "pulse added". The tries: the blast from the jump at pitch
0-70, + double, after a dash jump, after a double, and from the ground at the lip. Each runs with a full glove and no cooldown.

## Room limits and a room's `owns38`

A gate is checked with the verbs of the room its landing solid stands in. That is the room's own `owns38` if it has one, else the
plan's, minus what the room's LIMIT takes away:
- PISTOL ONLY: no THUNDERCLAP shot and no PULSE blast.
- MELEE ONLY: no THUNDERCLAP shot.
- NO DASH: no dash and no dash jump.
- NO SLIDE: no slide.
- NO JUMP: no jump, double, ride or wall jump.

This is measured, not assumed. `n38 limits [level] [room]` runs a Campaign24 run of the level and enters the room through the
campaign's own room entry (Campaign24.Enter). On n38-spire's v3 (PISTOL ONLY):
- Before the room, both verbs fire: one blast, one THUNDERCLAP shot.
- In the room, the owned list is "revolver" only. Neither gun can be put in hand (every slot was tried), and neither fires (0
  blasts, 0 shots).

## THUNDERCLAP shots in the tries

Each shot is a fresh trigger pull: the button is pressed on every other step, because a trigger held down does not fire the
THUNDERCLAP again. Shots and blasts count only from the takeoff on: a press left over from the try before can fire during the
run-up. Slice 3's chained-boost numbers had both faults; the table below is re-measured. K3's ledge sequence is a try: jump,
shoot straight down at 0.05 s, the double jump at the apex, shoot again (pitch -89 / -80 / -70, x2 and x3).

## Ice and the `skate` gate (NIGHT39 E2)

The checker now builds the plan's `surfaces` (ice, mud, boost rects) into the test world, and the real sim runs them: Surfaces26
for the friction and Glide30 (design.n30.glide) for the glide decay and for a dash on ice that adds speed. Every gate is flown on
the level's real ice. When a gate's takeoff stands on ice, every ground try is also flown with the SKATE run-up ("pumped"). That
run-up does not sprint. It fires a full boost whenever the bar holds one, and stops 12 m before the lip (30 m before it when the try
dashes itself, so the bar can refill).

- `verb: "skate"` (aliases `ice`, `glide`): a try uses the skate when a dash begun on the ice added speed (Glide30.dashAdds). The
  gate is flown twice: once on the level as it is, and once as the DRY control, which is the same geometry with the ice taken out. A
  crossing in the dry control is a crossing without the skate, so it counts as a cheat (D-6). The checker prints the skate's sizing:
  the best skate reach, the best reach without the skate, and the gap window where a skate-only gate could exist (or NONE).
- `--gate <solid>:<verb>:<fx,fy,fz>:<tx,ty,tz>[:<runUp>[:<id>]]` tags a solid the plan does not tag, where `<solid>` is `box<i>`,
  `prop<i>` or an id. `--room-limit <room>:<flag>` is a what-if LIMIT that is not in the plan.
- **Measured on n38-iceline's G1 (2026-10-13, round 1):**
  `n38 gates Design/levels/n38/iceline.plan.json --gate box10:skate:14,0,-134:14,0,-164:53:j3-g1 --only j3-g1`.
  - Result: RED. The skate crosses with a +85 % margin, but 706 tries cross without it: the dash jump and the PULSE blast, off dry
    ground.
  - With a what-if `pistolOnly` on j3 (no blast) it is still RED: the dry dash jump + double reaches 37.47 m and the best skate
    37.45 m.
  - Why: the pumped lane carries about 17 m/s to the lip, which is under the dash's own 24 m/s. A dash jump at the lip is therefore
    the same jump on ice or off it. On flat ice no skate-only gap exists at any length with today's glide numbers.

## The grapple, flown (NIGHT39 E2)

A `grapple` gate is now judged by flying the grapple. NIGHT38's rule (a hook point in sight from the lip counts as crossed, margin
+100 %) is gone.

How it is flown:
- The rig has its own Arcline21, the game's own pick: an enemy, then a node in the cone, then the surface under the crosshair. It
  sees the test world and the plan's nodes: the top-level `grapple` list and each room's `grappleNodes`.
- The sim's own rules stay in force: the tap latch, the pull (44 m/s), its arrival (keep 4 m/s, pop 3), 2 charges and the 0.8 s gap
  (design.n31 / n35 / n32).
- Each try jumps at the lip and fires one or two hooks in order, 0.05 s or 0.3 s in, then nothing, the double jump or a dash.
- The hook points are the nodes along the gap, the gate solid (top +1 m, top centre, near face), what stands past it (the far wall)
  and the roof.

How it is judged:
- The margin is the farthest hook's range slack, (40 - distance) / 40.
- A crossing whose last bite was not the gate solid also needs its carry: the same flight with the solid out of the world.
- The line-of-sight hook is still printed. On non-grapple gates it is still the cheat test.

What the flight showed:
- A pull to a node lets go on arrival.
- The aim prefers a node in its 15 deg cone over the surface behind it. So "node 1, then the deck" becomes "node 1, then node 2",
  and both charges are spent.
- x6 crosses only as "node 2, then the end wall behind the deck".

Measured 2026-10-13, round 1 (`n38 gates Design/levels/n38/n38-finale.plan.json --only x6-g1`):
- x6, the 44.5 m void: the grapple crosses with a +15 % margin (2 hooks, the farthest 26.86 m; 102 of 990 flown tries land). The
  gate is still RED: 35 tries cross without the grapple (the THUNDERCLAP boost x3 and the dash jump + PULSE blast).
- With a what-if `--room-limit x6:pistolOnly` (F's room-limit fix, C1 row 5): PASS. The grapple keeps +15 %, and the best other try
  is the dash jump + double at -16 %.
- n38-span, checked on the same night: all three grapple gates are RED on this checker AND on the one before it (port/stage3
  dd013f7b). The flown grapple crosses each at +31 %, but `boost x3 p-70 @0.05 + double@apex` (NIGHT38 round 4's ledge sequence)
  crosses 30.5 m with its 10 m rise in 15 / 25 / 15 tries. span38 has been ON with RED gates since that round.

## The blast plus a wall jump (NIGHT39 E2, round 2; found by K4's `n38kiln blastwall`)

Every plan that owns the PULSE gauntlet and the wall jump now also tries the blast with presses at the wall. The Mover allows a wall
jump off a noride wall. The family comes in two forms:
- Reactive: 1 or 2 presses, one on each new wall contact after the blast, then the double at the apex or not.
- Timed: K4's grid, thinned: a press 0.2-1.2 s after the blast, then a second press 0.3 s later or none.

The plain blast tries now also aim at pitch 80 and 89. On kiln G1 the blast alone tops out at 8.46 m and never lands. With one wall
jump off the cliff face it lands on the 9.5 m ledge, with the feet up to 11.31 m over the take-off. The selftest's blastwall case is
kiln's G1 in small, and the checker must catch it as a blast + wall-jump cheat.

Measured 2026-10-13, round 2, n38-kiln (K4, merged 6548c678): **RED 0 / 4.** The boost margins are fine (G1 +49 %, G2a +69 %,
G2b +55 %, G3 +49 %), but this family crosses every gate: 24 / 29 / 29 / 10 tries. Examples: `pulse p89 @0.15 + face x1` lands
1.83 m in; `pulse p89 @0.3 + face x1 + apex double` lands 5.59 m in.

A LIMIT cannot take the blast and keep the boost (pistolOnly removes both). The fix is geometry: a cliff over the combo's reach, or a
face that cannot be wall-jumped. The Mover treats a face as a wall only while |normal.y| < 0.3, so a face leaning more than about
17.5 deg is never one. Neither is measured yet.

## `ceiling` (a solid that caps the apex)

`"ceiling": true` on a box over a gap marks it as an apex cap: a slab whose underside sits under the apex of the verb it must
stop. The checker builds it like any other solid; the tag is for the reader and the frames. The apexes come from the envelope.
Do not hand-type them; read `envelopes38.json`.

## `noride` (a wall the Mover must not ride)

`"noride": true` on a wall box. Look: a dull, ridged material with no cyan trim (cyan = player-usable). Builder M owns the Mover
flag (through the lead).

- The checker hands the plan's noride boxes to the real Mover (`Mover.SetNoRide38`, builder M's flag) as the sim does from a
  level's campaign boxes. No ride starts on a noride wall and none turns onto one. A wall jump off one is still the Mover's to
  allow.
- The geometric alternative: the Mover treats a face as a wall only while |normal.y| < `surface.wallMaxAbsNormalY` (0.3 in
  PortExport/config.json; read in Mover.cs at its wall tests). So a face that leans more than about 17.5 deg from vertical is
  never ridden. This comes from config and is not checked in the harness.

## `flight38` (a room type)

`"flight38": true` on a room row (or `"kind": "flight"`) marks a FLIGHT ROOM: a chain of gates that each force the level's verb,
with the goal in sight. A fall costs a little and returns the player to the gate (a recovery stair or a respawn floor, as in
LEVEL-FORMAT's pits). The checker prints each flight room's gate count. A flight room with no gate is RED.

## The envelope (measured 2026-10-13, port/stage3 4bd19270 rows: wallride35 on, wallgrip36 on, ride38 off; envelopes38.json)

From standing ground at a lip (sprint run-up, 12 m). Reach is in metres past the lip at the takeoff height; apex is the rise over
the takeoff.

| verb / combo | reach | reach 2 m lower | apex |
|---|---|---|---|
| jump | 8.02 | 9.68 | 1.58 |
| jump + double | 14.75 | 16.15 | 2.87 |
| ground dash off the lip | 6.49 | 9.73 | 0.00 |
| jump, then air dash (sprinted) | 14.02 | 15.68 | 1.58 |
| **dash jump** (dash, then jump inside it; walked run-up) | **27.24** | 30.84 | 1.92 |
| **dash jump + double** | **37.44** | 40.44 | 3.41 |
| dash jump with a sprinted run-up (the bar too low: no DashJump) | 9.79 | 11.54 | 1.29 |
| jump + air dash + double | 18.83 | 20.14 | 2.82 |
| jump + PULSE blast (pitch 45; highest at 70) | 29.65 | 31.81 | 7.75 |
| jump + double + PULSE blast | 33.71 | 36.92 | 6.12 |
| dash jump + PULSE blast | 57.26 | 60.77 | 5.89 |
| PULSE blast from the ground at the lip (no lift measured; not checked why) | 1.18 | 12.05 | 0.00 |
| slide-hop | 10.65 | 12.86 | 1.58 |
| slide-hop + double | 17.15 | 19.00 | 2.87 |
| jump + boost x1 | 22.40 | 24.30 | 6.76 |
| jump + boost x2 (fire interval 0.9 s) | 37.64 | 39.90 | 9.61 |
| jump + boost x3 | 46.58 | 48.51 | 10.54 |
| K3's sequence: jump, shot down at 0.05 s, double at the apex, shot (pitch -89) | 29.64 | 30.36 | 13.36 |
| jump + boost + double (+ boosts chained) | 60.35 | 62.99 | 16.26 |

From a held wall ride, released after 0.5 s. Out is metres from the wall at the release height; along is metres along it.

| release pitch | out / along / apex | + double | + one boost |
|---|---|---|---|
| -45 | 3.13 / 4.40 / 0.37 | 9.85 / 13.83 / 1.58 | 10.65 / 15.03 / 1.94 |
| 0 | 4.89 / 6.86 / 0.95 | 10.38 / 14.58 / 2.23 | 13.39 / 18.90 / 3.28 |
| +45 | 6.72 / 9.43 / 1.80 | 10.84 / 15.22 / 2.85 | 16.13 / 22.77 / 4.90 |

- Facing walls chain (a held ride, released, re-pressed 0.25 s later) at plane gaps of 3-11 m. They miss at 12-20 m.
- Tap wall jumps climb 5.56 m in 4 s between walls 3 m apart, and 3.08 m between walls 5 m apart.
- Grapple (read from config, not flown): range 40 m, 2 charges, a 4 s recharge, a 0.8 s minimum gap, owned from campaign level
  4. Under grapple35 the hook pulls to any surface it hits. So the checker counts a grapple crossing when a point of the landing
  solid within 1 m of its top is in line of sight within range from the takeoff eye. A grapple node within range, within 6 m of
  the landing and no more than 1 m under its top also counts.
- Shockwave: not live (design.n25.shockwaveOff on: Loadout.ShockwaveOn false). No level owns it.

## What the numbers say about gates (measured windows; each gate still goes through the checker)

- **The dash jump is the longest ground verb.** It reaches 27.24 m, or 37.44 m with the double jump, so it outreaches every
  other ground try without the boost (18.83 m). Slice 1's table missed it, because its only "jump-dash" was a jump then an air
  dash. Any gate the dash jump must NOT clear needs a gap over 37.44 m on every line the player can take, or a ceiling.
  - C2's spire gates: 17.1-18.1 m gaps under a 4 m ceiling. The dash jump lands with +37-39 %. The best other try is
    jump + air dash + double at -3 % (v3) and -9 % (v4, v5, v7). These are full-bar numbers; C2's meter is not modelled.
  - A ride gate shorter than about 41 m (37.44 x 1.1) is cheesable by the dash jump + double on any ride-free line.
- **Boost gates have a rise window.** One boost lifts the feet 6.76 m; nothing without the boost goes over 2.87 m on the ground
  tries. A ledge risen more than 2.87 m and less than 6.15 m (6.76 / 1.1, so the boost keeps 10 %) separates them, as long as no ride wall stands in reach:
  make the walls near the gate `noride`. Chained boosts carry far (60 m with a double), so a long boost gate needs a ceiling to
  stop the chain if it must be one shot.
- **Ride gates:** facing walls 12 m or more apart do not chain. A gap along the route longer than the best ground reach (37.44 m: the
  dash jump + double), on every line, forces the ride chain on a level without the THUNDERCLAP. The ride chain try lets go at
  each wall's end and catches the next wall (iceline38 j3-g2 crosses 28.5 m with +66 %).
- **The grapple skips any gate whose landing it can see within 40 m.** On levels 4-6, hide the landing's top from the takeoff eye
  (a lip, a ceiling) or the grapple counts as a cheat.
