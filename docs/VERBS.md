# SKYGRIND's verbs (PRINCIPLES 1: nothing the player had is lost)

Every verb and weapon ability the player has, its input, the row that owns it, the campaign level it is available from, and how the
`verbs35` reel proves it in a Player (one Player per map: L1 n34-foundry / its n35 swap, L3 kiln, L4 span; NIGHT35.md method 1).
A verb a DECIDE35 default changes on purpose is marked **changed by D-x** with the new behaviour, and its line checks the new behaviour.
Written by the lead at NIGHT35's start (2026-10-11 20:30) from docs/memory/FEATURES.md, PORT-STATUS.md's events, Core/Mover.cs
(InputFrame, MoverEventType), Core/Combat.cs (CombatInput) and design.n31.unlocks; "not checked" where the source was not read.
Every night that adds or changes a verb updates this table and the reel together.

Inputs: pad (Xbox names; design.pad.n19 / n20 layout v3) / keyboard + mouse. "LT" is the gun's alt (zoom on most guns, LtAlways31).

## Movement (Core/Mover.cs; MoverEventType names the event the reel counts)

| # | verb | input pad / kb | row | from level | verbs35 check (PASS when) |
|---|---|---|---|---|---|
| 1 | walk / strafe | left stick / WASD | web | 1 | the feet move > 2 m in 1 s of forward input |
| 2 | sprint | L3 toggle / Shift | design.stamina.n19 | 1 | horizontal speed above walk speed while the toggle is on |
| 3 | jump | A / Space | web | 1 | a `Jump` event and the feet rise > 0.5 m |
| 4 | double jump | A in the air / Space | web; **changed by spire38 (NIGHT38, n38-spire's flight rooms only)**: once the first dash since a gate's pad has ended, the double jump is gone until a pad or the ground outside (design.n38.spire38.meter.oneDash) | 1 | a `DoubleJump` event; **spire38 check** (harness `spire38 gates`): in a gate room no jump + dash + double line lands past the gap (control, meter off: it lands 3.4-4.4 m past) |
| 5 | dash | B or RB-less dash bind / Shift-dash (design.move) | web; **changed by spire38 (NIGHT38, n38-spire's flight rooms only)**: a gate's pad holds the bar at one jump-dash (0.75 + 0.05), the bar does not refill there, and the first dash since the pad spends it (design.n38.spire38.meter) | 1 | a `Dash` event and > 4 m covered in 0.3 s; **spire38 check** (harness `spire38 meter`): the pad's bar 0.80, 0.00 after the jump-dash and 2 s on the landing, 0.80 on the next pad; the control (off) refills to 1.00 |
| 6 | slide | B hold / Ctrl | web | 1 | `SlideStart` then `SlideEnd`, speed kept above walk |
| 7 | slam (ground pound) | crouch in the air / Ctrl in air | web | 1 | a `Slam` event |
| 8 | jump dash (dash-jump) | dash then jump / same | web; **changed by spire38 (NIGHT38, n38-spire's flight rooms only)**: one per gate pad (the gate meter); BELFRY I teaches it on its gate | 5 (taught) | a `DashJump` or `BoostJump` event; **spire38 check** (harness `spire38 gates`): from each gate's pad the dash-then-jump lands 6.75 m past the near edge (margin >= 37.5 %) |
| 9 | wall ride | jump held along a wall / Space held | design.n26.wallRide; **changed by D-6**: `wallride35` (Lucio: ride while jump is held, no 1.2 s cap); **changed by ride38 (NIGHT38, D-3)**: a held ride keeps its height (no sink, a 60 s stuck guard) and the look pitch drifts it up / down past +-15 deg (up to ~2.5 m/s); the timed tap ride keeps 1.2 s | 2 (taught) | `WallRideStart`; ride > 1.2 s on a long wall with wallride35 on (the old ride ends at 1.2 s); **ride38 check**: a 30 s held ride on a 60 m wall holds the feet within 0.05 m of the attach height; pitch +45 deg rises, -45 deg sinks, and the rise stops under the wall top; **changed by F-5** `wallgrip36`: a jump into the wall rises at most 1.5 m above the attach point |
| 10 | wall jump (up to 3 per airtime) | A off a wall / Space | web + wallJump.maxPerAirtime | 2 | a `WallJump` event |
| 11 | wall-ride jump-off boost | release jump on a wall | **new** wallride35; **changed by ride38 (NIGHT38)**: the jump-off reads the look pitch (-45 / 0 / +45) after a held ride, as well as after a timed one | 2 | the jump-off speed > the ride speed (wallride35 on); **ride38 check**: after a 10 s held ride, the jump-off at pitch +45 deg rises above the ride's height |
| 12 | bunny hop (buffered landing jump) | A on landing | flow34 | 1 | 3 hops keep speed (flow34's line) |
| 13 | slide bounce (slide into a wall jump) | slide, A at a wall | flow34 slideBounce34 | 1 | the wall jump keeps the slide's speed |
| 14 | zip carry / wall coyote / wall buffer / lurch | (passive / one air redirect) | flow34 | 1 | flow34's own line (not re-performed: rides on 12-13) |
| 15 | jump pad | walk onto a pad | web | 1 (where placed) | a `JumpPad` event (not on every map: SKIP where none). NIGHT52: every n52 campaign level has pads (the rebuilt rooms' upward lines), so verbs37 performs row 15 on all five (PASS on n52-mid); the reel reads its baseline before Reset35 (58236d53) |
| 16 | glide rail / ice glide | ride the rail / ice | design.n30.glide | 2 (iceline) | not performed by verbs35 (map feature; the walk bot crosses it) |

## Grapple, whip and the bend

| # | verb | input pad / kb | row | from level | verbs35 check |
|---|---|---|---|---|---|
| 17 | grapple pull to a node | LB tap / E (design.n21.arcline, GrappleTap31) | n21 arcline, n31 grapplePull, n32 grapple32; **changed by D-2**: owned from level 4 only (`grapple35`); **changed by spire38 (NIGHT38, n38-spire's flight rooms only)**: the grapple is off while the feet are in a gate room (design.n38.spire38.meter.noHook), back at its edge; **spire38 check** (harness `spire38 meter`): ropeEnabled false in BELFRY I, true in THE NAVE | 4 (**was 1** with NIGHT34's nodes) | L4: a `RopeAttach` and the feet within 2 m of the node; L1: the grapple is NOT owned (PASS = unowned, the D-2 change) |
| 18 | grapple pull to a bare wall | LB tap at a wall | **changed by grapple35**: pulls straight to the hit point (NIGHT32 tapNodesOnly refused it); **changed by spire38**: off in n38-spire's gate rooms, as row 17 | 4 | L4: the feet end within 2 m of the wall hit point |
| 19 | grapple hold-swing (pendulum) | LB held > 0.2 s | NIGHT30 span; **changed by D-9**: off under grapple35 (`swing` sub-switch) | 4 | L4: a held LB pulls (no swing): the path stays within 1 m of the straight line |
| 20 | the whip (latch an enemy) | LB at an enemy | design.n33.whip33 | 4 (with the grapple, D-2) | L4: a whip latch event on an enemy |
| 21 | the whip readies the punch | whip then punch | design.n34.combo34 (whipReady) | 4 | rides on 20 + 24 (not separately performed) |
| 22 | the BEND (short on a latch, full on the meter) | any latch | design.n34.bend34 (split) | 4 (**was 1**, D-2) | L4: a `bend34Start` event after a latch |
| 23 | instant swap inside a bend | RB / wheel inside a bend | bend34 | 4 | L4: a swap inside a bend takes 0 steps |

## Weapons and their abilities (Core/Combat.cs; weapons by design.n31.unlocks)

| # | verb | input pad / kb | row | from level | verbs35 check |
|---|---|---|---|---|---|
| 24 | punch (the fist melee) | R3 / F (punch) | web | 1 | a punch event |
| 25 | KESTREL fire | RT tap / LMB | web; **changed by F-1** `tap36`: the tap fires on the PRESS step (charge35 fired it on release) | 1 | `fired` count +1 on a tap; with tap36 on, on the press step; **changed by p39** (NIGHT39 P1, the KESTREL only): a tap fires on its RELEASE step (a press released inside design.n39.p39.tapS 0.15 s), aimed from the press; a press while the charge cools still fires on the press step |
| 26 | KESTREL zoom | LT / RMB | design.zoom (n20), view33 | 1 | the zoom state on while held (FOV below the base) |
| 27 | KESTREL charge (the piercing shot) | RT hold, release | n31 charge + n33 pierce33; **changed by D-8**: `charge35` (hold charges with no shots first, release fires) | 1 | `chargeShot27Start` / `charge35Start` then a charged shot (pierce beam) with 0 normal shots during the hold (charge35 on); **changed by F-1** `tap36`: the press fires one normal shot, the hold then charges, the release fires the charged shot (1 normal + 1 charged); **changed by p39** (NIGHT39 P1, the KESTREL only): a hold past tapS fires NO tap first, the release fires the charged shot alone (0 normal + 1 charged) |
| 28 | PULSE gauntlet punch / LT blast | RT / LT on the gauntlet | design.n25 pulse, PulseCharge27, reach34 | 2 (room i1a) | not on the L1 / L3 / L4 maps before it is owned; L3 / L4: a gauntlet hit event |
| 29 | THUNDERCLAP fire | RT / LMB | web shotgun; **changed by spire38 (NIGHT38, n38-spire's gate rooms only)**: the room LIMIT `pistolOnly` (Campaign24's, banner "ONE JUMP-DASH PER PAD") takes the THUNDERCLAP (and its mid-air boost) and the TORCH for the room; the owned list comes back at the door (Campaign24's limit rule); **spire38 check**: the room rows carry `limit: ["pistolOnly"]` (Design/levels/n38-spire.json) and Campaign24's restore is its own tested rule; the Player check is the lead's | 3 (earned at level 3; owned from 4 by the unlock rows: check) | L4 (owned): `fired` +1 |
| 30 | THUNDERCLAP charge (the slug) | RT hold, release | n31 charge (slug); **changed by D-8** (`charge35`) | as 29 | the slug fires after a hold, 0 normal shots during the hold |
| 31 | THUNDERCLAP LT (zoom; the shockwave was turned off in NIGHT25, shockwaveOff25: not a verb the player had since) | LT on the THUNDERCLAP | design.n25 shockwaveOff25 | as 29 | SKIP with the reason while shockwaveOff25 holds (T 20:58); the LT zoom rides on row 26 |
| 32 | NAILGUN fire / LT | RT / LT | web nailgun | 4 (earned at level 4) | not performed (owned from 5): SKIP on the three maps, listed |
| 33 | TORCH fire / LT | RT / LT | FORGE b3 / b5 | 5 (earned at level 5) | not performed (owned after 5): SKIP, listed |
| 34 | weapon swap (next / slot) | RB / 1-6, wheel | web + n30 rules | 2 (two guns) | a swap event and the hand changes |
| 35 | weapon wheel | RB hold | design.pad.n20 | 2 | not performed by verbs35 (pad-only UI; PadRbCycle tests) |
| 36 | ARSENAL / QUICKDRAW (names on kills across guns) | kill, swap, kill | combo34 sub-switches | 2 | not performed (style names; NIGHT34's rea34 line) |
| 37 | interact / buy (wall buys, shop) | X / E | web, shop33 | 1 | not performed (SURVIVAL wall buys; the shop has its own reel) |

## Teaching and reading (new tonight, not verbs but checked with them)

| # | item | row | check |
|---|---|---|---|
| 38 | teaching text on level 1 (fire, zoom, charge, slide, dash) | teach35 | the teach35 reel shows each text in its trigger |
| 39 | a weapon pickup shows its model and name | pickup35 | a frame (W's pickup35 reel) |

## NIGHT36: the core loop's new verbs (DECIDE36 D-1, movement is the weapon; checked by verbs36, builder M, from builder P's events)

| # | verb | input pad / kb | row | from level | verbs36 check |
|---|---|---|---|---|---|
| 40 | parry (a punch into a YELLOW attack or projectile: freeze, the projectile returns and explodes, heal, a dash back) | R3 / F into a yellow tell | design.n36.tells36 + parry36 (P) | 1 (room 1.4 PARRY, n36-foundry) | a `parry36` event, a returned projectile's burst, health up and a dash charge back |
| 41 | launch (a punch while dashing / sliding / riding / pulled launches the enemy; a launched enemy that hits another or a wall bursts) | punch while moving | design.n36.launch36 (P) | 1 | a `launch36Burst` event and two kills from one punch (a husk into a husk) |
| 42 | heal by aggression (a kill within the radius spills health) | kill up close | design.n36.heal36 (P, D-5) | 1 | a `heal36` event after a close kill; none after a far kill |
| 43 | the boss walk-in (THE LANTERN fights in level 1's last room: no cut) | walk through the hall | design.n36.walkin36 + boss36 (B) | 1 | the boss starts with no Respawn (the feet stay in the hall's zone) |

## NIGHT37: the boss verbs (Research/2026-10-12-wave37/BOSSES.md; each boss's own reel performs it, verbs37 checks nothing older is lost)

| # | verb | input pad / kb | row | from level | check |
|---|---|---|---|---|---|
| 44 | the climb (chain wall rides up a shaft by leaping off with the look pitched up; damage from the walls only) | jump held on a wall, release with the look up, dash | design.n37.wick37 (W) | 1 (THE WICK) | wick37: the pilot reaches the cap; damage dealt while grounded and still ~0 |
| 45 | the slipstream (skate within 12 m behind THE STAG: +25 % speed) and catching it to punch a leg | skate behind it, punch | design.n37.stag37 (S) | 2 (THE STAG) | stag37: the speed gain behind it; a hind-leg punch staggers it |
| 46 | the thread tear (nail a moving knee, pull away with the grapple to tear it) | fire nails, grapple away (or LT) | design.n37.loom37 (O) | 4 (THE LOOM) | loom37: a knee torn by a pull; the crossing with no floor |
| 47 | the ring pass (jump-dash through THE CROWN's ring to burn its gems) | jump dash through the ring | design.n37.crown37 (C) | 5 (THE CROWN) | crown37: gems broken by passes; the descent |

## NIGHT40: rows changed on purpose (marked before any row lands; NIGHT40.md method 6; GG 2026-10-13 ~22:00, Research/2026-10-14-wave40/GG-REQUEST.md)

| row | verb | changed by | how (with the row on; `-skygrindOff40` gives NIGHT39's) | the check |
|---|---|---|---|---|
| 9 | wall ride | ride40 (R2) | every ride holds its height until the player leaves, the wall ends or the feet land; the look pitch steers up / down; the attach asks for intent (clearance, speed, input, wall height) | verbs37 row 9 PASS on levels 1, 4, 5; R2's attach count 0 on short surfaces |
| 11 | wall-ride jump-off boost | ride40 (R2) | the jump-off from an endless ride (pitch rule kept) | verbs37 row 11 PASS |
| 25 / 27 | KESTREL fire / charge | charge40 (W1) | the charge takes 0.9 s (was 0.5); the tap and the hold rule as p39 | rows 25 / 27 PASS with the new time |
| 28 | PULSE gauntlet | iceline40 (L2) | owned from level 2's START (n40-iceline: the gauntlet alone); level 3 (kiln40) gives it in a later zone | the owned weapons printed per level / zone |
| 29 / 30 | THUNDERCLAP fire / charge (the slug) | charge40 (W1), kiln40 (S1) | on the ground a hold charges with no shot first (p39's rule); in the air a press fires at once (the boost); the charge takes 1.2 s (was 0.7); level 3 starts with the THUNDERCLAP alone | rows 29 / 30 PASS; the boost's air latency 0 ms; level 3's gates GREEN |
| 32 | NAILGUN (the STITCHER) | loom40 (B4) | out of every campaign level (survival keeps it; DECIDE40 D-7) | SKIP on levels 4 and 5 with the reason "loom40" |
| 45 | the slipstream (THE STAG) | iceline40 + brawl40 (B2) | THE STAG leaves level 2 (DECIDE40 D-8: GG asked for a new boss 2) | retired with the row on; stag37's reel still runs with it off |
| 46 | the thread tear (THE LOOM) | loom40 (B4) | the knee tears by the grapple alone (no nails) | loom37 / the loom40 reel: a knee torn by a pull with no nail |
| 47 | the ring pass (THE CROWN) | crown40 (B5) | kept, now taught (the entry line, the ring lit at 12 m/s, the counter) | crown37 / the crown40 reel: gems broken by passes |
| new | the brawler's counter (punch through the haymaker's stuck window), the DRONE's orb returned, the WARDEN's shield punched, the CHARGER dodged then hit in the back | brawl40 (B2), foes40 (E3) | | each builder's reel scenario |

## NIGHT41: rows changed on purpose (marked before any row lands; NIGHT41.md method 5; GG 2026-10-14 ~08:40, Research/2026-10-14-wave41/GG-REQUEST.md)

| row | verb | changed by | how (with the row on; `-skygrindOff41` gives NIGHT40's) | the check |
|---|---|---|---|---|
| 9 / 11 | wall ride / jump-off boost | foundry41 (R5) | level 1's THE GANTRY b2-g1 wall gets a no-ride band (DECIDE41 D-2): the chain of walls is needed again there; every other wall as NIGHT40 | verbs rows 9 / 11 PASS on level 1 (n41-foundry); the gates b2-g1 back to "the chain" |
| 13 | slide bounce | (the reel only, T2) | the verbs reel holds the slide-bounce press for two steps (L-039; RED on level 3 since NIGHT39 was the reel's 0.02 s press) | row 13 PASS on level 3 |
| 17 / 19 / 22 / 23 | grapple pull / hold / the BEND / the bend swap | spire41 (R5) | level 5 (n41-spire) gets grapple nodes where they serve the descent (one per descent room at least) | RED on level 5 in NIGHT39 / NIGHT40 (no node) -> PASS on level 5 in the Player |
| 40 | parry | tokens41 (E6) | at most two enemies wind up at once in the campaign (DECIDE41 D-5): fewer simultaneous yellow tells; the parry itself unchanged | row 40 PASS on levels 1-5 with the cap on |
| new | the gauntlet's big-enemy hit | heavy41 (E6) | a gauntlet hit on a tank / WARDEN / CHARGER / THE ICEBREAKER: a longer hitstop and a bigger knock-back | E6's harness line (knock m, hitstop s per kind) and a frame |
| 46 | the thread tear (THE LOOM) | sweep41 (B8) | kept; THE LOOM's new floor sweep is answered by height (a ride wall, a ledge, a hook) | the loom pilots 10/10 with the sweep on; the verbs on n40-span 30/30 |
| new | aim assist on a boss's weak point (pad) | aim41 (P3) | the pad's assist pulls to the weak point (CYAN core, lit knee, passable ring, stuck fist), centre mass otherwise; the mouse unchanged (D-6) | hits per second on each weak point, before / after |

## NIGHT42: rows changed on purpose (marked before any row lands; NIGHT42.md method 6; GG 2026-10-14 ~18:35, Research/2026-10-14-wave42/GG-REQUEST.md)

| row | verb | changed by | how (with the row on; `-skygrindOff42` gives NIGHT41's) | the check |
|---|---|---|---|---|
| 9 / 11 | wall ride / jump-off boost | foundry42 (L1), wallfling42 (M1) | level 1's THE GANTRY forgiving (n42-foundry: shorter gaps or more walls, a recovery ledge); the jump-off's back speed capped | verbs rows 9 / 11 PASS on n42-foundry; the gates; the walk 10/10 |
| 10 / 13 | wall jump / slide bounce | wallfling42 (M1) | a wall jump only when moving into the wall or riding it (G2: an air jump press near a wall no longer flings back), a smaller back push with more lift, none in a landing's first steps | verbs rows 10 / 13 PASS on levels 1-5; M1's back-speed harness line |
| 25 / 27 | KESTREL fire / charge | charge42 (W1) | a held fire after a charged shot starts the next charge when ready (no taps while held, G10); the next attack waits for the reticle's turn (G8) | verbs rows 25 / 27 PASS; weapons42 |
| 28 | PULSE gauntlet | gauntlet42 (W1) | the swing 2.5 -> 1.8 s (G3, DECIDE42 D-2) | press-to-next-swing in telemetry; row 28 PASS |
| 29 / 30 | THUNDERCLAP fire / slug | boost42 (M1), charge42 (W1) | the air boost easier (more lift, lift by pitch, G5); the held charge as 27 | lift per pitch; verbs rows 29 / 30 PASS |
| 40 | parry | parry42 (W1) | a DRONE orb parries more forgivingly (reach, mercy window, buffer; orbs only, DECIDE42 D-3) | the share of punches inside the reach that parry; row 40 PASS |
| new | arena elements (a knock-into hazard, breakable cover, a launch pad, a grapple point) | arena42 (K1) | new verbs where placed; nothing old removed | K1's harness lines and the arena42 reel |

## NIGHT43: rows changed on purpose (marked before any row lands; NIGHT43.md method 6; GG 2026-10-16, Research/2026-10-16-wave43/GG-REQUEST.md)

| row | verb | changed by | how (with the row on; `-skygrindOff43` gives NIGHT42's) | the check |
|---|---|---|---|---|
| 9 / 11 / 14 | wall ride / jump-off / wall coyote | ride43 (M1) | a wider entry probe, a softer attach (no hard rise stop), a longer lost-wall grace, the same-wall-only lockout, a dash into a wall attaches (G3) | verbs rows 9 / 11 PASS on levels 1-3; M1's attach share |
| 10 / 13 | wall jump / slide bounce | chimney43 (M1) | a wall jump off a different wall refills the count (DECIDE43 D-1); the same wall still spends; a landing refills (G13) | row 10 PASS; a chimney climb of 5+ presses |
| 6 / 7 | slide / slam (B) | ride43 (M1) | within 0.3 s of leaving a wall, B drops and keeps the speed (no slam); a slam later or looking down (DECIDE43 D-2) | rows 6 / 7 PASS; the m/s kept by B after a ride |
| 29 / 30 | THUNDERCLAP fire / boost | boost43 (M1) | jump + shot in either order within ~0.15 s boosts in full; a jump release never halves a boost (G26) | row 29 PASS; the apex by offset |
| 28 | PULSE gauntlet | gauntlet43 (W1), fusion43 (W1) | the swing 1.8 -> 1.25 s (G17); a gauntlet hit or punch at high speed is a FUSION (G19) | press-to-next-swing; row 28 PASS; a FUSION event |
| 40 | parry | parry43 (W1) | a DRONE orb is caught farther ahead (a catch volume, the swing's buffer, the window in time to impact; G18) | the share of swings that reflect; row 40 PASS |
| new | the BURROWER's counter | burrow43 (A1) | a hit inside its emergence window staggers it (a new verb where placed; nothing old removed) | A1's harness line and the burrow43 reel |

NIGHT47 (GG's playtest night; rows in design.n47; `-skygrindOff47` gives NIGHT46's):

| row | verb | changed by | how (with the row on) | the check |
|---|---|---|---|---|
| 9 / 10 / 11 / 14 / 44 | wall ride / wall jump / jump-off / wall coyote / the climb | climb47 (W) | on CLIMB WALLS only (THE STACK's panels and the cap's ride wall): a jump-off or wall jump SETS the up speed (never adds to a falling one), a wall coyote and a jump buffer of 0.15 s, a 0.9 m / 15 deg catch, the wall jumps refilled on a catch ledge, a retry at full ride speed; every other wall unchanged | verbs37 26 / 20 / 25 / 30 / 30 with climb47 on (n47-r0e, n47-r3); golden 0 / 22 exact with it off; W's 5-jump chain equal 5 / 5 |
| 28 | PULSE LT blast | pulse47 (F) | the blast throws what it hits through launch36 (a husk ~14 m, was 1.4 m), 112 damage (was 64), a ring round the player; the 5 pips and the 15 s cooldown kept | row 28 PASS; F's n47feel blast |
| 29 | THUNDERCLAP zoom (LT) | zoom47 (F, view) | the zoom eases the world FOV to ~80 % and shoulders the gun; the boost unchanged | row 29 PASS |
| 17 / 34 / 35 | grapple / swap / wheel (pad column) | none (a doc fix) | the live pad layout v4: RB = grapple + special, Y = next weapon / wheel, LB = dash, R3 = punch; the keyboard grapple is G (F's round 0 read) | not re-performed |
| new | overheal | overheal47 (F) | a close kill's heal spill at full health fills an overheal to 150, held 2 s, decaying 2 / s to 100 (decay, never regen); damage takes it first | F's n47feel overheal; a647 frames |
| new (9 / 5) | ride kill (wall ride + any kill) | rideKill53 (EXT53 N; OFF in config until its proof) | a player's kill on a wall ride, or within 0.4 s (48 steps) after one, pays one boost back (design.stamina.boost.cost 0.5 of the bar) on top of the kill's own refill and shows RIDE KILL over the reticle; the ride, the dash and the kill refill are unchanged (nothing taken) | harness `n53ridekill ride` GREEN on n52-iceline g4 / n52-foundry a3 (on), RED off; the a253 reel (the lead's Player proof); golden 0 / 22 exact and the fingerprints EQUAL with it in config |

## The reel's output
`[reel] verbs35: <#> <verb> PASS|RED|SKIP <measured>` per row, `[reel] verbs35: total <pass>/<performed>`. SKIP only where the table
says "not performed" or the verb is not owned on that map by the unlock rows; a RED at the freeze gets 15 minutes of fixing, then goes on
the close table as RED with its reason (NIGHT35.md method 1).
