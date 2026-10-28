# NIGHT47 builder W: the cards (THE WICK, the climb, its arena; G1-G9). For the card gate; the lead assembles docs/BOSSES47.md / docs/ROOMS.md

Every measured number cites Research/2026-10-18-wave47/W.md (the harness and its line); every number marked "ours" is a design target to be
tuned by the meters; research lines cite W.md's research list or NIGHT47.md section 10. Rows: `wick47` (sim), `climb47` (sim, Mover),
`wickTop47` (sim), `wickLook47` (view), `foundry47` (level: n46-foundry -> n47-foundry by Tools/night/gen-n47-foundry.cjs). Off =
NIGHT46's fight bit for bit.

## 1. Boss card: THE WICK (level 1, 1.11 THE STACK)

**Fantasy (one line):** the foundry's last flame, caged in iron, tries to escape up its own chimney, and you chase it to the top and put it out.

**Silhouette and palette:** a ~4 m cage of 6 iron plates round a white-yellow flame core (today's model kept: GG likes this fight). Plates
dark iron with red-orange heat seams (enemy / danger); the core white-yellow; a cracked plate shows a yellow seam (yellow = parry / punch it,
the game's language); in P3 the plates hang wider and glow brighter (the Gabriel rule: the phase is told by colour, a line and harder versions
of the same attacks: NIGHT47.md section 10, Verified by the lead; W.md: Reported for the second fight). Cyan appears only on what the player
uses: the ride panels' edges, the glass floor's rim.

**What GG likes about THE WICK today (read from the code, Core/Enemies/Wick37.cs, Wick37.N43.cs):** (1) the EMBER parry: a yellow glob punched
back into the cage cracks a plate, and a KESTREL shot from a ride through that gap lands in full and drains the stagger (the parry -> pierce
combo; GG w43: "I like that the current attacks force the player to parry"; DESIGN-RULES B-5); (2) the chase up THE STACK (the fight
travels: the boss floats 2 m above you as you climb); (3) the leg beats with a gift crack at each new floor (wick43); (4) the finisher: the
DIVE stalls at the wall and a dash-punch knocks it into the slag. All four stay. What changes: the climb's ratchet (W.md: -0.56 m per gap),
the 10 % chip (chipScale 0.1) and the hidden bar locks, the one-room-one-rule structure (now three phases), the attack count (4 -> 7).

**The arena's job:** one shaft, three states, each told by the arena changing at the phase's start (never mid-wave): P1 the FLOOR (the shaft's
floor, the panels folded into the wall), P2 the CLIMB (the panels swing out of the wall in a spiral: the climb IS the phase), P3 the CAP (the
glass floor slides shut over the shaft: a full disc to fight on, like P1's floor but wider, with the cap's ride wall round it).

**The length target:** the human-model pilot's median 120 s (band 90-150 s, p90 <= 180 s, min >= 60 s, 40 seeds): P1 ~40 s, P2 ~40 s (the
sloppy climb pilot's climb 25-45 s), P3 ~40 s. From windows and phases, never HP: the pool stays 600 or goes down; chip outside a window rises
from 10 % (design.n37.boss37.chipScale 0.1, W.md) to 35 % of full (NIGHT47.md section 3: >= 30 %), so G4's "10 s of steady fire with the
starting gun moves the bar >= 5 %" holds. The leg locks ([0.6, 0.3, 0], the bar silently stops) are replaced by PHASE GATES: when the bar
reaches the gate the next phase starts at once (P1 -> P2 at 65 %; in P2 the bar floors at 40 % until the player stands on the cap, and the bar
says CLIMB) (B-1: the bar tells the truth).

### P1, THE FLOOR (100 % -> 65 %; teaches: 3 attacks, generous tells; the verb it stars: the PARRY)

| attack | tell (what, how long) | counter (verb) | punish window | hit effect | new? |
|---|---|---|---|---|---|
| EMBER volley | YELLOW: the core flares, 3 globs swell on the cage, 1.2 s (today's windup) | PARRY one back (the punch; VERBS row parry) | the cracked plate stays open 4 s (crackSeconds): any shot through it lands full (the ride rule is dropped on the floor: a grounded shot through a crack is full in P1 / P3) | a hit: 14 damage, a yellow splash, small knock | kept |
| FLARE sweep | RED: half the floor (the half you stand in) lights red with a rising heat shimmer, 1.4 s | cross to the other half: dash or slide (VERBS 5 / 6) | its recover 0.4 s + the brain's rest: the WICK hangs low, chip at 35 % | 20 damage, a burn flash, push toward the centre | kept (floor version: the band on the floor instead of the wall) |
| SLAG GEYSER (new, G6) | RED: 3 floor vents glow and hiss, a ring of embers circles each, 1.0 s; the vents are fixed in the floor (learnable) and the WICK picks the 1-2 nearest the player | step off the lit ring / jump (VERBS 3, 4) | after the eruption the WICK sinks to vent height for 1.5 s: the core is level with the player (headshot height), shots land at 100 % for that 1.5 s | 18 damage + a launch up 3 m (no death); a husk in a geyser dies (hazard both ways, A-5) | NEW |

Rule: a grounded fight in a small round room (r 9 m, GG's "smaller and more action packed"). Combined attacks: none in P1 (one tell at a time).

**Transition P1 -> P2 (1.2 s beat, at 65 %):** the WICK roars (a white-yellow flash on the cage only, never the screen), the shockwave pushes
the player 3 m toward the wall (soft), the WICK bursts up the shaft to 6 m; the panels swing out of the wall one after another in the spiral
(0.1 s apart, each lighting its cyan edge as it locks), the slag rises out of the floor grate to 0.3 m under the floor's top (the floor
becomes a hazard: you must climb). The line (G5, a hud47 fight line: <= 6 words, 44 px): **"IT'S CLIMBING. GET UP THERE."** The brain's LEGCHANGE (no attacks,
no damage) covers the beat, then the gift crack (wick43) cracks the plate facing the player.

### P2, THE CLIMB (65 % -> 40 %; the rule change: the floor is slag, the panels are the arena; the verb it stars: the RIDE / the climb)

| attack | tell | counter | punish window | hit effect | new? |
|---|---|---|---|---|---|
| EMBER volley | YELLOW, 1.2 s, the globs home at most 70 deg/s (today) | parry one mid-ride (the punch works on a ride) | the crack 4 s: a KESTREL shot from a ride through it lands full + drains stagger 1 (today's pierce) | 14 damage | kept |
| FLARE (wall half) | RED: the half of the shaft wall you ride on burns, 1.4 s | leap across to a panel on the other half, or drop to the catch ledge below | its recover + rest | 20 damage, a burn flash; never knocks you off a panel (forgiveness: a hit costs health, not height) | kept |
| EMBER CURTAIN (new, G6) | YELLOW: at a catch ledge only (the brain's camp clock, today's SLAM slot), 5 globs fall in a slow column round the ledge, 1.0 s of glow on the ledge above | parry one upward: it cracks the plate it returns into (the same parry, a new angle) or step onto the next panel | the crack 4 s | 10 damage each | NEW (replaces the plate SLAM on ledges in P2: a punch-it answer instead of a run-away answer) |

Rule: the WICK floats 4 m above the player (was 2) so it is in view while the look is up on a ride (and up is where the climb goes); the slag
never rises above 4 m under the last catch ledge the player stood on (today's belowLedge 4) and only while the WICK attacks: it is pressure,
never a timer. The bar floors at 40 % (shown: "CLIMB" on the bar) until the player stands on the cap. Damage in P2 comes from ride shots
through cracks (full) and chip (35 %).

**Transition P2 -> P3 (1.2 s beat, on the cap):** the player's feet touch the cap; the WICK shoots up out of the shaft and hangs over the
centre; the glass floor (wickTop47) slides shut over the shaft from the rim inward in 0.6 s, its cyan edge lit, then a pulse runs across it
(it reads as a floor before you stand on it); the slag below flares. The line: **"NO WAY DOWN. FINISH IT."** The bar loses its CLIMB floor
and turns to the P3 colour (the plates' glow).

### P3, THE CAP (40 % -> 0 %; P1's kit remixed: faster, combined, telegraphs clearer, never shorter than 0.5 s; the verb it stars: the PUNCH)

| attack | tell | counter | punish window | hit effect | new? |
|---|---|---|---|---|---|
| EMBER double volley | YELLOW, 1.0 s, a second volley 0.6 s after (wick43's leg 2 rule) | parry | the crack 4 s, full from the floor or the cap's ride lap | 14 each | kept, harder |
| FLARE + EMBER (combined) | RED half of the disc, 1.2 s; the embers come from the other half's side so the safe half is where you parry | cross, then parry | as P1 | 20 / 14 | kept, combined |
| SLAG GEYSER (cap version) | RED: the 4 rim vents of the cap catwalk (the glass covers the floor's vents), 0.8 s | step off / jump | the WICK sinks 1.2 s | 18 + launch up | new (P1's, harder) |
| PLATE RING (new, G6) | RED: the cage's 6 plates lift and spread to 3 m radius, spinning faster, a red ring at chest height on the floor's edge lights, 1.0 s | jump over the ring as it sweeps out (VERBS 3) or slide under its gap (the ring has one 60 deg gap, shown yellow) | while the plates are out (1.5 s) the core is bare: every hit lands 100 % | 22 damage + a knock 6 m | NEW |
| CRACK-OPEN CHARGE (new, G7) | RED: the WICK backs to the rim opposite you, its plates clamp shut and glow red, a red lane lights on the glass from it to you, 1.2 s (wind-up inside the 1-1.5 s band) | sidestep the lane: dash, slide, or jump to the cap's ride wall (VERBS 5 / 6 / 9) | it hits the far wall (or misses you and stops at the rim) and CRACKS OPEN: the plates burst apart, the YELLOW core pulses for 3.0 s (>= 2.5 s, G7) | the charge: 25 damage + a knock 10 m; the punch: below | NEW (the DIVE + STALL evolved: same place in the kit, same finisher) |

**The crack-open's weak point (G7):** the core turns YELLOW (the punch-it colour) and pulses 2 Hz for 3.0 s; world-space indicators: a yellow
ring round the core that shrinks with the time left (a readable clock), 3 yellow chevrons on the glass pointing at it from the player's side,
the plates flung out (open); the first time only, the line **"PUNCH THE CORE"** (a hud47 fight line: 44 px, upper third, 2.0 s + 0.3 s a word). A MELEE hit
(fist or gauntlet; the launch rule's reach 3.5 m + cage radius) takes **22 % of the bar** (G7: 20-25 %), and launches the player **15 m back**
(G7: 12-18 m) up and away at ~30 deg (so he lands on the glass, never off the cap: the cap's ride wall stands at 13 m and the launch aims at
the far side's open glass), with hitstop 0.22 s (DESIGN-RULES C-2's "hit on an OPEN boss" tier, 24-34 steps at 120 Hz: 0.20-0.28 s) then
0.4 s of slow motion at 0.35x and a hard camera hit (trauma +0.6, the heavy tier 3-5 deg). A gun hit in the window lands 100 % (never the 22 %
chunk: the punch is the answer, GG: "hit it with melee"). The finisher kept: when the punch leaves the bar at or under 15 % (finishAt), the
WICK is knocked off the cap into the shaft's slag through the glass (the glass shatters under it only), the end.

**The finale beat (G9 idea 1, below):** the last hit: 0.6 s slow motion, the glass cracks under it, the slag below flares through the cracks.

Length: P3 ~40 s = one crack-open punch (22 %) + ~18 % from parries / bare-core hits; a sloppy player sees 2 charges (the pilot must land
>= 70 % of openings, G7).

### The fight's words (G5: F's hud47 text system draws them (the lead's message 23:2x): <= 6 words a line, 44 px at 1080p, one at a time, held 2.0 s + 0.3 s a word, upper third; "PUNCH THE CORE" is one of these fight lines, not a separate size; W writes the lines)
| moment | line |
|---|---|
| the walk-in reveal (B-3, <= 6-8 s) | THE WICK |
| the reveal hint (replaces "HOLD {jump}: RIDE. LET GO: LEAP UP" in P1, the ride is P2's) | PUNCH THE YELLOW EMBERS BACK |
| the first crack (once) | SHOOT THROUGH THE CRACK |
| P1 -> P2 | IT'S CLIMBING. GET UP THERE. |
| P2's first panel (once) | HOLD {jump} TO RIDE. LOOK UP. |
| a fall to a catch ledge (once) | CAUGHT. KEEP CLIMBING. |
| P2 -> P3 | NO WAY DOWN. FINISH IT. |
| the first crack-open | PUNCH THE CORE |
| the finisher | (none: the flash and the fall say it) |

## 2. climb47 card (the Mover, behind the row; G1, G2; DESIGN-RULES M-1, M-2, M-3, M-5; VERBS rows 9, 10, 11, 14, 44 get "changed by climb47")

The cause (W.md, measured): a jump-off from a ride gains 2.16 m with the look up at the release (1.15 m level), the ride's speed adds nothing,
the panels rise 2.80 m, so each attach lands 0.56 m lower on its panel than the last until the capsule misses the face; only a hand that keeps
the look up during the whole ride (ride38's drift) and releases late keeps its height. Fix the system first (M-1), then size the gaps
(section 3). Every rule acts only on a CLIMB WALL: a face the sim marks as one (THE STACK's panels and the cap's ride wall in n47-foundry; the
sim passes the Mover a `climbWall(normal, position)` predicate per run, null with the row off). Off: every branch skipped, NIGHT46 bit for bit
(the golden vectors and the 22 replays exact; the new fields go into RunSnapshot.MoverState).

| number (design.n47.climb47.numbers) | value (ours, tuned by the sloppy pilot) | why |
|---|---|---|
| `setUp` | 10.0 m/s | A jump-off or wall jump off a climb wall SETS the vertical speed to max(today's up speed, setUp), never adds to a falling one (Celeste's WallJump assigns Speed.Y: W.md, Verified). Today's +45 deg jump-off gains 2.16 m (its up speed: not read yet, Ride35.Boost); 10 m/s is a first guess for ~2.9 m (measured in R1 by `n47wick reach --rows climb47`), so the reach beats the 2.8 m rise even with a LEVEL look: the pitch still steers (ride38's rule kept) but is no longer a hidden requirement (M-2). |
| `catchBelow` / `catchLine` | 1.2 m / 0.3 m | THE CATCH (the ratchet's fix, Celeste's corner correction idea: W.md): an attach on a climb wall whose feet are under the panel's bottom + catchLine by at most catchBelow is lifted to bottom + catchLine over `catchEase` 0.12 s (eased, not a snap). Each attach then starts at the same height on its panel, so every jump gains the same (G2: +-5 %), whatever the last one did. |
| `wallCoyote` | 0.15 s | After leaving a climb wall (a ride ended by the wall's end, not by a jump), a jump press still jumps off it (NIGHT47.md section 3; Celeste's 0.1 s coyote is for ledges, JumpGraceTime 0.1: W.md, Verified; walls get more because the panel's end comes at speed). flow34's wall coyote today: its value not checked yet (R1 reads design.n34.flow34). |
| `jumpBuffer` | 0.15 s | A jump press up to 0.15 s before touching a climb wall is kept and fires on the attach (M-3's 0.15 s; the sloppy pilot's +-120 ms jitter fits inside). |
| `catchReach` / `catchConeDeg` | 0.9 m / 15 deg | The near-miss draw onto a climb wall reaches 0.9 m (ride40's attachReach 0.5) and accepts a stick / look aimed up to 15 deg off the face's direction (NIGHT47.md section 3: "a jump aimed 15 degrees off still attaches"); Celeste accepts a wall jump from 3 px (a quarter tile+) off the wall (W.md, Verified). |
| `ledgeRefill` | true | Standing on a climb ledge (a catch ledge) refills the wall jumps and the double jump at once (a landing already does; a catch ledge's touch counts even without the land's settle). |
| `retrySpeed` | 10.4 m/s | A ride started from a catch ledge after a fall starts at the ride's full climb speed (G2: "a retry starts at full climb speed"; 10.4 m/s = the stack's measured ride speed, Harness.N37.Wick.cs). |

## 3. Arena card + flow plan: 1.11 THE STACK, n47-foundry (DESIGN-RULES A-9 card; arena-flow-web.md Part D's 11 points)

| field | P1 THE FLOOR / P2 THE CLIMB / P3 THE CAP |
|---|---|
| Title | THE STACK (the foundry's chimney) |
| Place fantasy | the foundry's furnace chimney; the slag of the whole works drains under its grate; THE WICK is the flame that kept it lit |
| Spatial law | SHAFT (one round space read in three states: floor, wall, cap) |
| Verb it celebrates | P1 the parry, P2 the ride climb, P3 the punch |
| The 30 s problem | P1: the WICK hangs low over a 9 m floor; its embers come yellow from above: punch one back, then shoot the crack before it closes. P2: it rises with you; ride a panel, look up, leap, and shoot through a crack from the ride. P3: it charges across the glass; sidestep, then punch the opened core. |
| Molecule | P1: one disc (r 9 m) with 3 fixed vents at 120 deg (1.5 m from the wall) and the entry gap; P2: 4 sections of panels on the spiral, each ending on a CATCH RING (a full 1.4 m ring round the wall, so any miss lands one section down at most: G2), one shortcut: the chain cap speed (20 m/s) skips a panel per lap; P3: the glass disc (r 9) + the catwalk (9-13 m) + the cap's ride lap (13 m, 9 m tall) |
| Landmark | from the entry gap: the WICK's core glow at the top of the cage, and above it the cap's ring of light 24 m up (the goal is visible from the start) |
| Hero prop | the floor grate over the slag: P1 the vents breathe through it, P2 the slag rises through it, P3 it is under the glass |
| Hazard both ways | P1 geysers launch the player 3 m (18 damage) and kill a husk; P2 the slag (20 damage + a bounce to the last catch ring); P3 the open shaft is covered (no fall: G8) |
| State change | P1 -> P2: the panels swing out (cyan edges light one by one), the slag floods the floor; P2 -> P3: the glass slides shut (cyan rim, a pulse across) |
| Light / colour story | P1 dark floor, the WICK's core the main light, the vents red; P2 the cyan panel edges are the only cool light, a path up; P3 open sky at dusk, the glass lit from the slag below in red-orange |
| Every-solid rule | each panel is a hold of the climb (its rise and chord below); each catch ring is the fall's floor; the 3 vents are the P1 geyser's places; the cap wall is P3's dodge (ride the lap to escape the charge's lane) |
| Tell-a-friend line | "you fight it on the floor, then it flies up the chimney and you chase it up the walls, and at the top the floor seals and you punch its heart out" |
| Cut list | today's ledge ARCS (replaced by full catch rings); the plate SLAM on ledges in P2 (replaced by the EMBER CURTAIN); the hot shaft wall stays (a ride on the bare wall is a burn; the panels are the route) |

**The climb's numbers (P2; gaps as ratios of the measured reach; the reach is re-measured with climb47 ON in R1 and the gaps re-sized):**
Reach r = the +45 deg jump-off's gain over the release, look up at the release only (today 2.16 m; with climb47 the target is ~2.9 m). Bands
(NIGHT47.md section 3): bottom <= 55 % of r, middle <= 70 %, top <= 80 %, no gap in 0.8-1.1. With today's r 2.16 m: bottom rise 1.15 m
(0.53), middle 1.45 m (0.67), top 1.65 m (0.76). The final rises are set from climb47's measured r (with r 2.9: bottom 1.55, middle 2.0,
top 2.3; never above the top band). Horizontal: the chord between a panel's end and the next start stays 5.66 m at 4 per turn until the
jump-off's horizontal reach is measured (W.md: not checked); the bottom third gets 5 per turn (a 4.4 m chord) if that reach says so.
Sections: 4 (bottom 5 panels, two middle sections of 5, top 4), each ending on a catch ring; the cap at ~24-26 m (was 38.4 m: a climb of
25-45 s for the sloppy pilot at ~1.5 s a panel). Each segment's start shows the next hold in view (the next panel's cyan edge inside a 90 deg
cone from the ride's look direction: checked by a ray test in the harness). M-5: flight time between panels <= 0.5 s at the ride speed
(10.4 m/s along a 5.66 m chord: 0.54 s today: the bottom third's 5-per-turn chord fixes it).

**Part D's 11 points (arena-flow-web.md; numbers ours, checked by n46flow --level n47-foundry --rooms a9 --bosses and the pilots):**
1 entry view: the WICK + the cap's light in view from the gap (a ray test); 2 the loop: P1 a ring round the 9 m disc (no dead ends: the vents
are walkable when cold), P3 the catwalk lap + the ride lap; 3 cut-through: P3's ride wall lap; 4 sightlines: the WICK is never hidden by a
panel for > 1 s (the follow height 4 m above in P2); 5 cover: none in P1 (a boss floor; the parry is the defence); 6 heights: P1 one tier,
P2 4 catch rings, P3 two (glass / ride lap); 7 spawn tells: no adds in THE STACK (a boss room); 8 no behind-spawns: n/a; 9 the hazard at the
rim, never the centre (P1 vents 1.5 m from the wall); 10 the way back closed (whether the gap seals at the reveal today: not checked; R1 reads SurvivalSim.Wick37.cs); 11 a fall's cost: one
section (a catch ring) and 20 health in P2, none possible in P3.

## 4. wickTop47 card (the glass floor; G8)

A disc at the cap's height covering the shaft (r <= 9 m), added to the stack's triangle layer when the player first stands on the cap (the
P2 -> P3 trigger), solid to the player and to every enemy (the same layer the stack's triangles live in, so the Mover and every ray see it).
Look (wickLook47): a glass pane, translucent, cyan edge strip at its rim (player-usable: V-2), a faint hex grid so it reads as a surface, the
slag's red-orange glow seen through it. Done when: frames; a 40-seed P3 run with 0 falls into the shaft (the harness counts feet under the
cap's top - 1 m inside r 9 m in P3).

## 5. The sloppy climb pilot (harness only, `n47climb sloppy` in Tools/SimHarness/Harness.N47.Wick.cs; G1's meter)

On THE STACK of the level under test (n46-foundry's today, n47-foundry's new), no boss (the climb alone) and then with the boss (P2):
- presses: every jump press (the release, a double jump, a wall jump) at the pilot's ideal step +- U(-120, +120) ms (Mulberry32, seed per run);
- look: yaw and pitch error U(-12, +12) deg, drawn once per ride (a human's aim holds); the look WHILE riding is level in half of the rides
  (a human who does not know the drift) and up 30-45 deg in the other half; the release pitch U(25, 45) deg;
- releases: the release point U(40, 90) % of the panel; one release in four late by 150 ms;
- the double jump at the apex +- the press jitter, in 3 rides of 4.
Reports over 40 seeds: per segment (gap) the first-try clear share (bottom third >= 95 %, top third >= 80 %: G1), the falls and how far
(never more than one section: G2), the climb time (25-45 s: G1), the height per jump on the chain (+-5 %: G2), and per gap the narrowest input
window that still clears (the range of release steps / press steps that succeed: >= 150 ms, G1). Runs with `--rows climb47` (GREEN) and
without (RED).

## 6. PROOF plan (L-001 / L-014: RED once, GREEN once; the meters first)

| row | RED (before / row off) | GREEN (row on) | frames the lead takes (reel a147, n47-foundry, 1280x720 and 1920x1080) |
|---|---|---|---|
| G2 climb47 | the stack chain: attach -0.56 m a gap, fell at gap 3 / 5 (W.md, measured) | the same chain +-5 % a gap, 5 / 5; a chain of five jump-offs: the same gain each | a mid-ride frame per segment (the next hold in view) |
| G1 the climb | the sloppy pilot on n46-foundry with climb47 off (R1 measures it) | >= 95 % bottom, >= 80 % top first try, 25-45 s, no window < 150 ms | P2 at the bottom, middle, top |
| G3 phases | n46-foundry's fight (no phases, the bar locks) | P1 / P2 / P3 entered in order in 40 / 40 seeds (the human pilot) | P1 from the entry, the P1 -> P2 beat (panels swinging out), P2 mid-climb, the top, P3 |
| G4 length | the human pilot's WICK baseline (the lead's, ~23:45); 10 s steady fire with the KESTREL off-window today: chip 10 % (computed, then measured) | median 90-150 s, p90 <= 180 s, min >= 60 s; 10 s of fire >= 5 % of the bar | the bar after 10 s of fire |
| G5 text | today's lines (n46-r3 frames) | every line <= 6 words, 44 px (hud47) on the frames | each line's frame at both sizes |
| G6 attacks | 4 attacks, 1 new per leg | SLAG GEYSER, EMBER CURTAIN, PLATE RING: each fired, tell timed (harness log), dodged by its counter in the pilot | each new attack's tell start and its last 0.2 s |
| G7 crack-open | none today | the pilot lands the punch in >= 70 % of P3 openings; 22 % of the bar; launch 12-18 m (harness distance) | the crack, the hit, the launch, the landing |
| G8 glass floor | none (a fall into the shaft on the cap is possible today: the cap is a ring) | 0 falls into the shaft in P3 over 40 seeds | the glass appearing, standing on it |

Checks at every code commit: golden 0 / 22 exact (CoreHarness), `fingerprint`, `--off46`, `--off47` EQUAL, dn-check-wt 0.

## 7. Push past the notes (G9; R4, each behind its own row, researched before it is built)
1. **The finale beat:** the last hit's slow motion and the glass cracking under the falling WICK (the lead's list; ULTRAKILL / DMC finisher
   language, research in R4).
2. **The phase checkpoint:** a death in P2 or P3 restarts at the phase's start with the bar where the phase began (retry46 extended to
   phases), never from P1 (A-4: failure costs time, not a restart).
3. **The ember ride-catch:** a parried ember punched while riding a panel bounces back with the ride's speed added (a faster, bigger crack:
   the ride and the parry combine, M-6), shown by a cyan streak on the returning glob.
