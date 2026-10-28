# DESIGN31: bosses you cannot camp (the lead, NIGHT31 design box, 2026-10-10 10:28)

Sources: NIGHT31.md, Research/2026-10-10-wave31/ (R1 bosses, R2 grapple, R3 weapons, SPIKE), the code at a17cfc84. Each
number is tagged **measured** (a run), **web** (PortExport/config.json), **n30** (a Unity row already in
Design/unity-config.json) or **guess** (a first pick to measure tonight). Builders refine a card in their first task; the
lead reads the change before it is built.

## A. Weapon cards (item 2, item 3, D-1 as changed by the lead)

The order: **level 1 KESTREL only; level 2 + PULSE gauntlet (awarded in i1, the tank corridor); level 3 + THUNDERCLAP
(as today); level 4 + NAILGUN (as today).** The fist (R3 click melee) is always there; it is not a slot. Rejected: the
forge's L2 THUNDERCLAP / L3 gauntlet, because level 2's i1 (the tank dies to one gauntlet hit) and i2 (the PULSE room) are
built around the gauntlet and would need a rebuild, and the Wyrm's head punch would come one level after the gauntlet.

| weapon | arrives | RT tap | RT hold (charge) | LT (always does something) | proves it | cooldowns |
| --- | --- | --- | --- | --- | --- | --- |
| KESTREL (revolver) | level 1 start | the web's shot | 0.4 s hold + 0.5 s charge (guess, R3: ULTRAKILL piercer about 0.5 s) -> a PIERCING shot, 4 x damage through 3 bodies (n30 numbers), the reticle ring fills, a flash at full; release early = cancel, nothing spent | zoom 1.5 x (web) with the walk slow; while zoomed the charged shot's ring shows on the reticle | the Lantern's core shield (only a charged shot from the wall breaks it); a line of husks in f8 | charge cooldown 4 s (guess; R3: piercer 2.5 s, n27 had 10 s) |
| PULSE gauntlet | level 2, room i1 | punch (0.35 s swing) | the heavy swing (as today, 2.5 s) | five punch-kill pips full: the BLAST (as today, 15 s); pips not full: a PARRY, 0.25 s window (guess) that sends a projectile back at its shooter for 2 x its damage, 1.0 s cooldown (guess; a parry moves no enemy, so it is not crowd control); a miss shows "PARRY" on the slot ring | the Stag's open back; the Wyrm's head; Lantern orbs in boss rush | heavy 2.5 s (n30), blast 15 s (n30) |
| THUNDERCLAP (shotgun) | level 3 room 1 | the web's volley | 0.4 s hold + 0.7 s (guess) -> a SLUG: one pellet line, 3 x damage, forward knock 2 x (n30) | zoom 1.25 x (n30; no shockwave, GG NIGHT25) | the Wyrm while staggered (full gun damage only then); armoured husks | charge cooldown 6 s (guess, n27 had 14 s) |
| NAILGUN (STITCHER) | level 4 room s1 | the stream | none (a stream weapon: hold is fire) | detonate every stuck nail (as today) | the Loom's joints; flyers | as today |

**The cross-weapon combo (item 2c, cut first):** a charged KESTREL shot that passes through a body carrying stuck nails
detonates them at 1.5 x (guess). One line in the level-4 weapon room teaches it.

**HUD:** the charge uses HUD30's CHARGING ring on the weapon's slot plus a reticle ring; the charge cooldown is COOLING with
seconds; the gauntlet slot's LT label reads BLAST when the pips are full and PARRY otherwise (16 px or more).

## B. The grapple card (item 1, D-3)

- **Tap (press and release within 0.2 s, guess) = PULL-IN.** The hook takes the marked target (the existing cone 15 deg,
  1.5 m probe, 60 m range: n21 rows) and the Mover's ZIP mode flies you straight at it, rope taut, colliding as the swing
  does (SPIKE.md). Speed: `design.n31.grapplePull.speed` applied to the Mover's `config.arcline.zip.reelSpeed` the same way
  `Arcline21Rules.Apply` already writes the zip numbers (per run; golden runs never hook, so they cannot see it). First pick
  44 m/s (guess: web zip 34, R2 cites ULTRAKILL's whiplash 60 u/s; 44 keeps the 8 m dash-jump the faster verb over short
  gaps). Arrival: the web's 2.6 m or 1.2 s (web), then the velocity is kept and an upward pop of 5 m/s (guess, R2: 4-6) is
  added from outside (`Mover.velocity` after the zip release event).
- **Hold (still held 0.2 s after the press) = the NIGHT30 swing** (unchanged: the rope keeps its 0.4 s cooldown; the enemy
  yank keeps its 15 s).
- **Target mark:** a cyan diamond at the target the hook would take this frame (node or surface), drawn only while the
  grapple is owned and ready; red-orange never (palette rule 8).
- **Cancels:** a jump or a dash ends a pull with the speed kept (the Mover's own jump release already does `ReleaseRope
  ("jump")`; the dash path is builder G's first read: a cancel that needs a Mover edit is SHORT).
- **Feel (view):** FOV +8 deg over 0.12 s during a pull (guess), the rope drawn retracting.
- **Measured RED first:** the Player speed toward the node 0.3 s after a hook today.

## C. Boss cards (items 4-6). The six BOSS QUESTIONS: 1 still 3 s loses to; 2 the verb; 3 two damage states / two weapons; 4 the close reward; 5 half-health change; 6 tells and where taught.

### C1. THE LANTERN (level 1, KESTREL only; TARGETED; `design.n31.lantern31`)
Keeps NIGHT30's 16-panel ring, lava floor, lines. Adds:
1. Still: a floor camper burns in the rising lava (n30); a platform camper: the platforms SINK into the lava at half health
   and before that the orbs target a player who has stood on a platform for 3 s (guess).
2. Verb: the wall ride (the core's shield is breakable only from a wall panel).
3. Damage states: (i) the SHIELD: only a CHARGED KESTREL shot from the wall breaks it (plain shots spark off; fallback if
   the charge is not on by round 2: a zoomed shot hits the shield's seam); (ii) the CORE: open for 4 s (guess), any shot.
4. Close reward: none at range; a core hit from within 12 m (guess) drops a health shard on the nearest platform.
5. Half: the three platforms sink (lava), new vents.
6. Tells: the shield's crack line glows cyan when a charged shot can break it; taught in f8 by a husk line behind a glass
   pane that only a charged shot pierces (L's weapon room for level 1).

### C2. THE STAG (level 2, KESTREL + gauntlet; FULL; replaces the Glacier; `design.n31.stag31`)
A blocky ice ram (boxes, the web's style) on the glide-ice arena (the Glacier's 44 x 70 m rink plan reused as the floor;
its banks become low ice ramps, every spot ice or in shard range).
1. Still: standing still 2 s (guess) or farther than 22 m (guess) triggers a CHARGE at the player (a 0.9 s tell: head
   down, hooves scrape, a red-orange line on the ice); a camper is hit (30 % of max health, guess) and knocked.
2. Verb: GLIDE: the dodge is a sideways dash on ice (glide adds speed); a walking player is too slow to clear the charge
   width (3 m, guess).
3. Damage states: (i) ANTLER CRYSTALS (two, cyan) at range: a CHARGED KESTREL shot shatters one and the Stag stumbles
   (a 1.5 s stun, guess) where it stands; (ii) the BACK: after a missed charge it skids, stops 3 m short of any wall (no
   corner exploit: the reviewer), and turns its OPEN BACK away from the player it charged for 2.5 s (guess): a GAUNTLET
   punch there does full damage (guns 15 %). A stunned Stag's back is open too.
4. Close reward: a back punch refills one gauntlet pip.
5. Half: the high ice ramps melt to open water (lava rules: 25 % and a knock, n30), cracks split the floor into lanes;
   charges come in pairs.
6. Tells: the charge line (red-orange), the open back glows cyan; taught in i5 / i6 by the tank's own charge (if the tank
   has none, a line: "DODGE SIDEWAYS. PUNCH ITS BACK.").
Camp test extra camper: the wall corner.

### C3. THE KILN WYRM (level 3, + THUNDERCLAP; FULL; `design.n31.wyrm31`)
Keeps the head-punch stagger (n30). Adds:
1. Still: a burrow (cut first) rises under a player who stood still 3 s; a far plinker meets a leading spit volley every
   6 s (guess) when farther than 25 m (guess).
2. Verb: the punch timing and the dash out of the charge line.
3. Damage states: (i) guns do 20 % (guess) unless the head is STAGGERED; (ii) STAGGERED (after a head punch): 2.5 s
   (guess) in which the THUNDERCLAP does full damage and its slug double.
4. Close reward: the stagger itself; a slug kill-shot on a staggered head drops health.
5. Half: the k-room platforms crumble (a 1 s tell), feints start.
6. Feints: one charge in three stops short and sweeps (a sideways tail sweep, jump it), so a held punch misses; the head
   glows cyan only on a real charge. Taught: the cyan head line from NIGHT30.

### C4. THE LOOM (level 4, + NAILGUN, grapple; TARGETED; `design.n31.loom31`)
Keeps NIGHT30's joints-as-nodes, CUT, sweep. Adds: the joints are PULL-IN targets (item 1): a tap zips you to the joint and
you hang there on a short rope (the swing), nails at full damage; standing on the deck 3 s (guess) calls a flyer drop on
you (the flyers attack: cut list in NIGHT30, built only if time); half health: the deck's middle breaks (lava under it),
nodes only on its legs. The ride phase is cut (NIGHT32).

## D. Arenas (item 7)
- f12 (Lantern ring): platform sink rows; the glass-pane lesson in f8.
- g1 (Stag): the rink plan reused; banks to low ramps; a hand SVG at `Design/levels/n30/sketches/stag31.svg`.
- k5 (Wyrm): crumble rows on two platforms.
- s8 (Loom): the deck's breakable middle.
- Weapon rooms: f1 (KESTREL: charge shot target behind glass), i1 (gauntlet: award + the tank), k1 (THUNDERCLAP: the slug
  on an armoured husk), s1 (NAILGUN: detonate + the combo line).
Every arena: levelgen 0 errors, walk30 10/10, two Player frames.

## R. The reviewer's fixes (10:50; these OVERRIDE the sections above)
1. **Stag corner (C2):** the charge does NOT stop short of walls: a charge that reaches a wall slams it, the Stag is stunned
   2 s with its back open (a dodge next to a wall is the best punish), and a 4 m stomp hits anyone within 4 m of where it
   stops (wall or not). A corner camper is hit by the charge or the stomp.
2. **Camp PASS (E):** a camper passes (the boss wins) only when the camper DIES or loses at least 50 % of max health within
   30 s; the damage-rate check (under a third of the pilot's) is the second condition, not an alternative.
3. **The grapple tap (B):** the Mover releases the rope the moment the button is up (Mover.cs:542-545, "letGo"), the hook
   flies only 0.06-0.1 s, and zip is fixed at the fire from the target (Mover.cs:537). So: a Core input LATCH (like
   `HoldToggle.cs`, outside Mover.cs) keeps `grapple.held` true until the rope releases, and when the button comes up within
   the tap window while the rope is out, `rope.zip` is set true (the anchor stays: zipAnchor null for a node / surface). A
   hold past the window is the swing, released on button-up as today. The grapple's toggle mode (HoldToggle) gets the same
   tap / hold rule, written by G. The dash cancel already exists (Mover.cs:579-583); the jump cancels only in the air.
4. **Loom (C4):** a zip that arrives at a joint switches to a swing on the shortest rope (minLen) with the latch held (the
   hang); a joint shake throws off anyone hanging longer than 3 s; the deck-camper flyer drop is IN (not cut). Nails do full
   damage on a joint only within 6 m of it or while roped to it; 20 % from the deck.
5. **Wyrm (C3):** during the stagger only the THUNDERCLAP does full damage (the slug double); other guns stay at 20 %. The
   leading spit volley fires at anyone who has not moved 3 s, at any range (not only past 25 m), so cutting the burrow
   leaves a stillness punish.
6. **LT gaps (A):** the NAILGUN's LT zooms 1.25 x when no nail is stuck and detonates when one is (the slot label says ZOOM or
   DETONATE); the gauntlet's LT shows COOLING with seconds during the parry's 1.0 s, and falls back to PARRY while the BLAST
   cools.
7. **Crowd control (15 s rule):** only an ANTLER CRYSTAL stuns the Stag and a crystal regrows after 15 s; the slug's forward
   knock does not act on bosses (husks only, the slug's charge cooldown 6 s stays because a knock on a husk is a gun's own
   hit, as the THUNDERCLAP's volley already knocks); the pull-in's 0.4 s cooldown (a movement verb) goes on DECIDE31 as D-11.
8. **Fire and melee (w29 lockout):** a melee press while a charge is building cancels the charge, nothing spent; with the
   gauntlet in hand its LT is the only parry (`AltUse.ParryWanted`'s punch parry is skipped for the gauntlet).
9. **Measure first (item 1):** the Player speed toward the node 0.3 s after a hook; the time to cover 30 m at 34 / 44 / 60
   m/s (harness); the length of real pad trigger taps (the tap window).

## E. The camp test (item 4)
`camp31`: four campers per boss (centre, edge / corner, highest reachable platform, farthest reachable point; the Stag
adds the wall corner); each stands, aims at the boss's centre and fires its best owned weapon (the charge shot when it
has it). PASS: each camper dies, or its damage rate is under one third of the pilot's. RED first on NIGHT30's bosses.
