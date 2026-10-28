// NIGHT30 (builder L): generates Design/levels/n30/span.plan.json (LEVEL 4, THE SPAN, DESIGN30.md section D with section I's fixes, rooms s1-s8).
// Run: bash Tools/night/n30-regen.sh span   (node Tools/night/gen-n30-span.cjs, then levelgen Design/levels/n30/span.plan.json Design/levels/n30/span.json)
// The verb is THE ARCLINE grapple (builder S's spike GO: with design.n30.span on, a campaign level whose JSON carries `grappleNodes` owns the ARCLINE from its
// first step; the swing keeps its 0.4 s, the yank its 15 s). The weapon is the NAILGUN, a pickup in s1 (`award` the fallback; unlocks.json row n30-span).
// LAVA under every gap (section I: no void, no respawn pit anywhere in level 4). Grapple crossings are NOT proved by the validator (LevelValidate29 runs
// the Mover, not the rope); each is written as a `grapple` note on its room and checked on the Player by the span30 reel.
const { Level, r3 } = require("./n30-lib.cjs");

const L = new Level("n30-span", "LEVEL 4 THE SPAN",
  "LEVEL 4, THE SPAN (NIGHT30, builder L, DESIGN30.md section D + I; GG: 'every level should revolve around a mechanic and a weapon with a fun, unique boss at the end that's inspired by that level's design'). "
  + "A broken bridge of gantries over a lava gorge: THE ARCLINE grapple is the level's verb (grapple nodes as room rows), the NAILGUN its weapon (s1). s1 the tollgate (the NAILGUN, swarmers), s2 the first hook (one node over a 4 m wall with lava at its foot), "
  + "s3 the gap (two nodes over a 12 m lava gap: a jump + dash reaches 8 m), s4 the hook fight (three gantries at three heights joined by nodes), s5 rest (the Loom's legs through a slot), s6 the chain (node -> wall lane -> node -> deck over lava), "
  + "s7 the escalation (a ring of four nodes round the pylon), s8 THE LOOM (the boss room: a deck in a lava ring, four leg anchors, builder B's boss). Web coordinates, metres: x east, y up, z SOUTH (the player walks north, -z).",
  { rank: { time: [240, 360, 540, 780], kills: [100, 85, 65, 40], style: [2500, 1500, 700, 250], _why: "Unity-only (builder L): S/A/B/C cuts for level 4 up to the Loom: time in seconds (an estimate: no pilot can grapple yet), kills = percent of the waves' enemies, style = points (NOT calibrated)." } });

const rect = (r, lx0, lz0, lx1, lz1) => [r.X(lx0), r.Z(lz1), r.X(lx1), r.Z(lz0)];
const n = (k, c) => Array(c).fill(k);

// ---------------------------------------------------------------- s1 TOLLGATE (the NAILGUN on a lit plinth; four swarmers fly in after the pickup)
{
  const r = L.room({ id: "s1", name: "4.1 TOLLGATE", kind: "room", w: 12, l: 18, h: 7, cx: 0, outX: 0, par: 30, spawnLz: 2, exitFromN: 2.5 });
  r.box("deck", -6, 12, -1, 18, 0, 2.4, "the toll deck (2.4 m): height over the swarmers' approach");
  r.ramp(-3.5, 6, 12, 3, 2.4, "up to the toll deck (walkers: 6 m for 2.4 m)");
  // NIGHT31 L round 2 (the lead's s1 frame: the plinth at (4.6, 6) sat 49 deg off the entry's facing, under the weapon HUD panel): moved to (2.5, 7), 20 deg, in view
  r.solid("plinth", 2.5, 7, 1.2, 0.6, 1.2);
  r.pickup("weaponPickup", 2.5, 7, 1.1, { weapon: "nailgun", note: "the NAILGUN on its plinth, in view from the door (the room's `award` is the fallback)" });
  r.row.pickup = { weapon: "nailgun", at: r.pt(2.5, 1.1, 7) };
  r.row.award = "nailgun";
  [[3, 14], [4.5, 15.5], [2, 16], [4, 13.5]].forEach(([lx, lz]) => r.nodeAt(lx, lz)); // NIGHT31 L (spawn31): (4, 12.5) -> (4, 13.5): 11.2 -> 12.2 m
  r.highNodeAt(-3.5, 15, 2.4);
  r.row.waves = [n("swarmer", 4)];
  r.row.lessons = [{ kind: "husksDead", n: 4, prompt: "NAILGUN: HOLD TO STREAM.", promptPad: "NAILGUN: HOLD TO STREAM." }];
  // NIGHT31 L (DESIGN31 D: level 4's weapon room teaches the detonate and the cross-weapon combo, item 2c): lessons31 replaces `lessons` when design.n31.ltAlways
  // is on (the lead / W); the combo line only matters if 2c ships (cut first: then drop the second line)
  r.row.lessons31 = [{ kind: "husksDead", n: 2, prompt: "NAILS STICK. {altFire} DETONATES THEM.", promptPad: "NAILS STICK. {altFire} DETONATES THEM." },
    { kind: "husksDead", n: 4, prompt: "A CHARGED SHOT SETS NAILS OFF.", promptPad: "A CHARGED SHOT SETS NAILS OFF." }];
  r.solid("crate", 4.5, 9.5, 1.4, 1.2, 1.4); r.solid("crate", -4.5, 3, 1.4, 1.2, 1.4);
  r.ceil(2.5, 7, 2, 2, "amber", 8, 1.6); // the plinth's one accent: the gun is the hero
  r.trim("W", 4, 3, 0.4, "cyan"); r.trim("E", 4, 3, 0.4, "cyan"); r.trim("E", 14, 3, 0.4, "cyan");
  r.pool(0, 15.5, 3, "cyan", 7, 1.8);
}

// ---------------------------------------------------------------- s2 FIRST HOOK (one node above a 4 m wall with lava at its foot: only the grapple crosses)
{
  const r = L.room({ id: "s2", name: "4.2 FIRST HOOK", kind: "link", w: 10, l: 22, h: 10, cx: 0, outX: 0, par: 12, spawnLz: 2, exitFromN: 2.5 });
  r.lava(-5, 8, 5, 11, 0.8, "the lava at the wall's foot (a miss burns and knocks you up; climb back out to the south)");
  r.box("hull", -5, 11, 5, 12, 0, 4.0, "the 4 m wall: no jump clears it, the node above it does");
  r.grappleNode(0, 8.5, 10.5);
  r.row.routeVia = [[[r.X(0), 0, r.Z(7.5)], [r.X(0), 0, r.Z(14)]]]; // the grapple crossing (the route graph has no rope links)
  r.row.grapple = "the node at y 8.5 over the wall's south face: hook it from the entry floor (about 8 m), swing, let go over the wall (NOT proved by the validator: the span30 reel hooks it)";
  r.row.lessons = [{ kind: "reached", rect: rect(r, -5, 12, 5, 22), minY: -0.5, prompt: "{grapple} GRAPPLE THE RING.", promptPad: "{grapple} GRAPPLE THE RING." }];
  // NIGHT31 L round 3 (the pull), round 4: the final wording (the lead, 12:35): the first hook teaches both halves of the button
  r.row.lessons31 = [{ kind: "reached", rect: rect(r, -5, 12, 5, 22), minY: -0.5, prompt: "TAP {grapple}: PULL. HOLD: SWING.", promptPad: "TAP {grapple}: PULL. HOLD: SWING." }];
  r.row.gapLz31 = 12; // pull31: past the wall's north face
  r.light("wall", [r.X(0), 3.0, r.Z(11 - 0.05)], [0, 0, 1], [6, 0.25, 0.1], "cyan", 8, 1.6); // a cyan strip on the wall's face, under the ring
  r.ceil(0, 10.5, 2, 2, "white", 10, 1.0);
  r.trim("W", 4, 3, 0.4, "white", 5, 0.9); r.trim("E", 4, 3, 0.4, "white", 5, 0.9);
  r.pool(0, 19.5, 3, "cyan", 7, 1.8);
}

// ---------------------------------------------------------------- s3 THE GAP (two nodes in a line over a 12 m lava gap)
{
  const r = L.room({ id: "s3", name: "4.3 THE GAP", kind: "link", w: 12, l: 33, h: 12, cx: 0, outX: 0, par: 12, spawnLz: 2, exitFromN: 2.5 });
  r.lava(-6, 7, 6, 22, 1.0, "the 15 m lava gap (section I: lava under the whole gap, no void): a running jump + dash falls into it (the validator's jumpDash row), the two nodes carry you over (15 m, not the card's 12: margin for the dash)");
  r.grappleNode(0, 9.5, 11); r.grappleNode(0, 9.5, 18);
  // NIGHT31 L round 3 (G's carry cap, arriveKeepMax 12, landed): pull31: node 1 alone now drops you in the lava at lz 20.6 (the chain is needed), but a tap at
  // node 2 straight from the entry still landed at lz 30.8 (one tap). A SCREEN WALL stands in the lava across the gap (lz 13-14, up to y WALLTOP): from the entry
  // floor the line to node 2 meets it (the hook bites the wall), from node 1's arrival (about lz 9, y 7.8) the line to node 2 passes over it, and so does the flight
  const WALLTOP = parseFloat(process.env.S3WALL || "7.5");
  // 1.5 m gaps at both side walls keep the trench one pit (the pit-recovery row: a burnt player wades back south round the wall); every line from the entry
  // floor to node 2 (x 0) still crosses the wall's plane within |x| < 2
  r.box("hull", -4.5, 13, 4.5, 14, -1.0, WALLTOP, `the screen wall in the lava (top ${WALLTOP} m, 1.5 m gaps at the side walls): node 2 is hidden from the entry floor, so the line is node 1 -> node 2`);
  r.row.routeVia = [[[r.X(0), 0, r.Z(6.5)], [r.X(0), 0, r.Z(22.5)]]]; // the grapple crossing
  r.row.grapple = "two nodes at y 9.5 over lz 11 and 18: hook the first from the lip, let go at the top, hook the second (NOT proved by the validator)";
  r.row.lessons = [{ kind: "reached", rect: rect(r, -6, 22.5, 6, 33), minY: -0.5, prompt: "SWING. LET GO AT THE TOP.", promptPad: "SWING. LET GO AT THE TOP." }];
  r.row.lessons31 = [{ kind: "reached", rect: rect(r, -6, 22.5, 6, 33), minY: -0.5, prompt: "TAP {grapple} RING TO RING.", promptPad: "TAP {grapple} RING TO RING." }]; // NIGHT31 L round 4: final (the lead, 12:35)
  r.row.gapLz31 = 22; // pull31: past the lava's north edge
  r.solid("crate", -4.5, 3, 1.4, 1.2, 1.4); r.solid("crate", 4.5, 28, 1.4, 1.2, 1.4);
  r.ceil(0, 11, 2, 2, "white", 11, 1.0); r.ceil(0, 18, 2, 2, "white", 11, 1.0);
  r.trim("W", 4, 3, 0.4, "cyan"); r.trim("E", 4, 3, 0.4, "cyan"); r.trim("W", 27, 3, 0.4, "cyan"); r.trim("E", 27, 3, 0.4, "cyan");
  r.pool(0, 30.5, 3, "cyan", 7, 1.8);
}

// ---------------------------------------------------------------- s4 HOOK FIGHT (three gantries at three heights joined by nodes; swarmers on the high one)
{
  const r = L.room({ id: "s4", name: "4.4 HOOK FIGHT", kind: "room", w: 18, l: 26, h: 12, cx: 0, outX: 0, par: 35, spawnLz: 2, exitFromN: 2.5 });
  r.box("deck", -9, 5, -4, 11.5, 0, 1.2, "gantry 1 (low, 1.2 m)");
  r.ramp(-6.5, 1.5, 5, 3, 1.2, "up to gantry 1 (walkers)");
  r.box("deck", 3, 10, 9, 17, 0, 3.0, "gantry 2 (mid, 3.0 m)");
  r.ramp(6, 25, 17, 3, 3.0, "up to gantry 2 from the north (walkers: 8 m for 3 m)");
  r.box("deck", -9, 11.5, -3, 17.5, 0, 4.0, "gantry 3 (high, 4.0 m): the swarmers' perch, reached by the nodes (and by walkers up its ramp)");
  r.ramp(-6, 25.5, 17.5, 3, 4.0, "up to gantry 3 from the north (walkers: 8 m for 4 m)");
  r.grappleNode(-1, 9, 8.5); r.grappleNode(0, 9.5, 14.5);
  r.row.grapple = "node 1 (y 9) between gantry 1 and gantry 2, node 2 (y 9.5) over the gap between gantry 2 and gantry 3 (NOT proved by the validator)";
  [[-1, 14], [2, 21], [-1, 24], [3, 24.5]].forEach(([lx, lz]) => r.nodeAt(lx, lz));
  r.highNodeAt(6, 13.5, 3.0); r.highNodeAt(-6, 14, 4.0); r.highNodeAt(-5, 15.5, 4.0);
  r.row.waves = [n("husk", 4), n("swarmer", 6)];
  r.row.lessons = [{ kind: "husksDead", n: 10, prompt: "NAILS FOR FLYERS.", promptPad: "NAILS FOR FLYERS." }];
  r.solid("crate", 1.5, 6, 1.4, 1.1, 1.4); r.solid("crate", 0.8, 16.5, 1.4, 1.1, 1.4); // round 5: the second crate left a 0.8 m slot by gantry 3's face (the walk30 stall at lz 15); now 3.1 m clear
  r.light("wall", [r.X(-9 + 0.05), 5.5, r.Z(14.5)], [1, 0, 0], [3, 0.3, 0.1], "amber", 8, 1.6); // the high gantry's accent
  r.trim("E", 13.5, 4, 3.6, "cyan"); r.trim("W", 8.5, 4, 1.8, "cyan");
  r.ceil(-1, 9, 2, 2, "white", 11, 1.0); r.ceil(0, 19, 2, 2, "white", 11, 1.0);
  r.pool(0, 23.5, 3, "cyan", 7, 1.8);
}

// ---------------------------------------------------------------- s5 REST (a lit hall; the Loom's legs through a slot)
{
  const r = L.room({ id: "s5", name: "4.5 REST", kind: "link", w: 6, l: 10, h: 4, cx: 0, outX: 0, par: 4, spawnLz: 1.5, exitFromN: 1.5 });
  r.pickup("healthPickup", 0, 4, 0.9, { note: "a health pickup before the chain" });
  r.decal("slot", 0, 7, 3, 0.4, "white", 0, { note: "the slot in the floor: the Loom's legs glimpsed below (view)" });
  r.trim("W", 4, 3, 0.4, "amber", 5, 1.0); r.trim("E", 4, 3, 0.4, "white", 5, 1.0);
  r.ceil(0, 4, 2, 2, "white", 6, 0.9); r.pool(0, 8.5, 2.4, "cyan", 5, 1.2);
}

// ---------------------------------------------------------------- s6 CHAIN (node -> wall lane -> node -> deck, lava below; two spitters on the deck)
{
  const r = L.room({ id: "s6", name: "4.6 CHAIN", kind: "room", w: 14, l: 36, h: 12, cx: 0, outX: 0, par: 30, spawnLz: 2, exitFromN: 2.5 });
  // NIGHT31 L round 3 (the pull, design.n31.grapplePull: a tap flies you straight at a node at 44 m/s and KEEPS the speed: pull31 measured one tap at node 2 from
  // the door landing at lz 35.7, past the deck and the spitters): a BAFFLE hangs from the roof across the room (lz 12-13, y 4.5 up), so from the entry floor the
  // deck node is out of sight (the hook needs a clear line) and a straight pull slams into it; the fast line is node 1 by the WEST wall -> the W wall lane under
  // the baffle (y < 4.5) -> node 2 in sight -> the deck. The SAFE slow route: a dry EAST strip (x 3.5..7) along the lava to the catch steps (walk 18 m, hop up)
  r.lava(-7, 4, 3.5, 22, 0.8, "the lava floor, from the west wall to the dry east strip");
  r.box("hull", -7, 12, 7, 13, 4.5, 12, "the BAFFLE: a 1 m slab from the roof down to y 4.5 (the wall lane and the east strip pass under it; the deck node is hidden from the entry)");
  r.box("deck", -7, 22, 7, 28, 0, 3.0, "the deck (3.0 m): the spitters' perch");
  r.box("deck", 4, 19, 7, 20, -0.8, 0.3, "catch step 1"); r.box("deck", 4, 20, 7, 21, -0.8, 1.3, "catch step 2"); r.box("deck", 4, 21, 7, 22, -0.8, 2.3, "catch step 3");
  r.ramp(-5.5, 34.5, 28, 3, 3.0, "up to the deck from the exit floor (walkers)");
  // node 1 LOW by the west wall, just past the baffle (seen from the entry under it): the pull runs nearly level, so the kept speed puts you on the W wall lane.
  // pull31 (one tap from the entry, forward held): today's carry (42 m/s at arrival) rides the lane 0.52 s and flies on to the exit floor (lz 35.7); with the
  // carry capped at 12 m/s (pull31 --keep 12, a proposal for G) it rides 0.46 s and drops at lz 21.7, short of the deck: node 2 finishes the chain.
  // Tried: (-5, 6, 7) and (-5.5, 3, 8) slam into the baffle (lz 11.8, lava); (-5.5, 2.5, 10) rides 0.6 s uncapped but falls at lz 12.1 capped
  const N1 = (process.env.S6N1 || "-6,3.5,14").split(",").map(Number);
  r.grappleNode(N1[0], N1[1], N1[2]); r.grappleNode(-2, 9.5, 19);
  r.row.grapple = "round 3 (the pull): node 1 (y 6, lz 7, 2 m off the west wall) pulls you onto the W wall lane (lz 9-16, under the baffle), node 2 (y 9.5, lz 19, hidden from the entry by the baffle) carries you onto the deck; the east strip is the walk (NOT proved by the validator; pull31 measures the taps)";
  r.row.gapLz31 = 22; // pull31: one tap past lz 22 (the deck's foot) from the entry = trivial
  r.row.reachFrom = r.pt(0, 0, 33);
  r.highNodeAt(-3, 25, 3.0); r.highNodeAt(3, 25.5, 3.0);
  r.nodeAt(4, 31);
  r.row.waves = [["spitter", "spitter"]];
  r.row.lessons = [{ kind: "husksDead", n: 2, prompt: "HOOK. RIDE. HOOK.", promptPad: "HOOK. RIDE. HOOK." }];
  r.row.lessons31 = [{ kind: "husksDead", n: 2, prompt: "TAP {grapple}. RIDE. TAP AGAIN.", promptPad: "TAP {grapple}. RIDE. TAP AGAIN." }]; // NIGHT31 L round 4: final (the lead, 12:35)
  r.light("wall", [r.X(-7 + 0.05), 3.0, r.Z(12.5)], [1, 0, 0], [7, 0.25, 0.1], "cyan", 8, 1.6); // the W wall lane
  r.ceil(0, 7, 2, 2, "white", 11, 1.0); r.ceil(-2, 19, 2, 2, "white", 11, 1.0); r.ceil(-5.5, 25, 2, 2, "white", 10, 1.0); // iteration 2: off the room title's screen line from the spawn (the r2 frames of f5b / i6 / f9; elevation 18-30 deg, azimuth under 13 deg)
  r.trim("E", 1.5, 3, 0.4, "cyan"); r.trim("E", 31, 3, 0.4, "cyan");
  r.pool(1, 33.5, 3, "cyan", 7, 1.8);
}

// ---------------------------------------------------------------- s7 ESCALATION (a ring of four nodes round the pylon)
{
  const r = L.room({ id: "s7", name: "4.7 ESCALATION", kind: "room", w: 20, l: 28, h: 12, cx: 0, outX: 0, par: 50, spawnLz: 2, exitFromN: 2.5 });
  r.box("hull", -2, 12, 2, 16, 0, 12, "the PYLON: the hero, floor to roof; the ring of nodes runs round it");
  // round 3: the moat is 2 m wide (it was 4 m and the fight kept spilling into it: walk30 97 room retries over 10 seeds)
  r.lava(-4, 10, 4, 12, 0.8, "the lava moat, south of the pylon"); r.lava(-4, 16, 4, 18, 0.8, "the lava moat, north of the pylon");
  r.lava(-4, 12, -2, 16, 0.8, "the lava moat, west of the pylon"); r.lava(2, 12, 4, 16, 0.8, "the lava moat, east of the pylon");
  const S7 = (process.env.S7S || "6,9.5,8").split(",").map(Number); // the SOUTH pair's |x|, y, lz (round 3 tries; see below)
  r.grappleNode(-S7[0], S7[1], S7[2]); r.grappleNode(S7[0], S7[1], S7[2]); r.grappleNode(6, 9.5, 20); r.grappleNode(-6, 9.5, 20);
  r.row.grapple = "four nodes at y 9.5 at the moat's corners: a ring you can swing round the pylon while you fight (NOT proved by the validator)";
  [[-8, 5], [8, 5], [-8, 23], [8, 23], [0, 22], [-5, 20.5], [-8.5, 14], [8.5, 14]].forEach(([lx, lz]) => r.nodeAt(lx, lz)); // NIGHT31 L (spawn31): (0, 5.5), 3.5 m ahead of the entry -> (-5, 20.5), north of the moat
  r.row.waves = [n("husk", 5), n("husk", 5), n("swarmer", 6), ["spitter", "spitter"]];
  r.row.lessons = [{ kind: "husksDead", n: 18, prompt: "USE THE NODES.", promptPad: "USE THE NODES." }];
  r.solid("crate", -8, 10, 1.4, 1.1, 1.4); r.solid("crate", 8, 18, 1.4, 1.1, 1.4);
  r.light("wall", [r.X(0), 7.5, r.Z(12 - 0.05)], [0, 0, 1], [3, 0.3, 0.1], "amber", 9, 1.8); // the pylon's accent
  r.trim("W", 14, 6, 1.8, "cyan"); r.trim("E", 14, 6, 1.8, "cyan");
  r.ceil(-6, 8, 2, 2, "white", 11, 1.0); r.ceil(6, 20, 2, 2, "white", 11, 1.0);
  r.pool(0, 25.5, 3, "cyan", 7, 1.8);
}

// ---------------------------------------------------------------- s8 THE LOOM (the boss room: a deck in a lava ring; four leg anchors for builder B)
{
  const r = L.room({ id: "s8", name: "4.8 THE LOOM", kind: "boss", w: 34, l: 34, h: 22, cx: 0, outX: 0, par: 120, spawnLz: 4, exitFromN: 2.5 });
  r.row.boss = "loom";
  // the deck: 18 x 18 in the middle at y 0; the lava ring round it (8 m wide); the entry and exit strips join it at the south and north
  r.lava(-17, 0, 17, 8, 0.8, "the lava ring, south (the entry strip crosses it)");
  r.lava(-17, 26, 17, 34, 0.8, "the lava ring, north");
  r.lava(-17, 8, -9, 26, 0.8, "the lava ring, west"); r.lava(9, 8, 17, 26, 0.8, "the lava ring, east");
  r.box("deck", -2, 0, 2, 8, -0.8, 0, "the entry strip over the south lava (floor level)");
  r.box("deck", -2, 26, 2, 34, -0.8, 0, "the exit strip over the north lava (floor level)");
  // the four leg anchors: the deck's corners, where the Loom's legs grip the edge (builder B builds the legs and their joints)
  r.row.loomLegs = [r.pt(-9, 0, 8), r.pt(9, 0, 8), r.pt(9, 0, 26), r.pt(-9, 0, 26)];
  r.row.loomLegs_why = "the four points (world x, y, z) where THE LOOM's legs grip the deck's edge: SW, SE, NE, NW corners of the 18 x 18 deck (builder B: the legs, their two joints each and the body under the deck)";
  r.grappleNode(-13, 12, 4); r.grappleNode(13, 12, 4); r.grappleNode(13, 12, 30); r.grappleNode(-13, 12, 30);
  r.row.grapple = "four static nodes at y 12 over the lava ring by each leg (the leg joints themselves are builder B's moving nodes)";
  // NIGHT31 L (item 7, DESIGN31 C4 / D: builder B's loom31 reads it): at half health the deck's MIDDLE breaks: a 10 x 10 m hole (lx -5..5, lz 12..22) over lava
  // at -0.8 (the ring's lava surface), leaving a 4 m walkway round it and the entry / exit strips; the nodes are then on the legs only (DESIGN31 C4). World rect
  // [x0, z0, x1, z1] as the `lava` rows. Additive: NIGHT30's Loom and a run with loom31 off never read it
  r.row.deckBreak31 = { rect: [r.X(-5), r.Z(22), r.X(5), r.Z(12)], lavaY: -0.8, when: "half health", tell: "red-orange cracks across the middle",
    _why: "NIGHT31 DESIGN31 C4 / D: the deck's breakable middle (its extent and the lava under it); builder B adds the collider swap and the tell's seconds (design.n31.loom31)." };
  r.trim("W", 17, 8, 2.0, "white", 12, 1.2); r.trim("E", 17, 8, 2.0, "white", 12, 1.2);
  r.ceil(0, 17, 6, 6, "white", 18, 1.2); r.pool(0, 31.5, 3, "cyan", 8, 1.4);
}

// ---------------------------------------------------------------- completability proofs (the real Mover; the grapple crossings are NOT proved: see each room's `grapple`)
{
  const R = (id) => L.rooms.find((r) => r.id === id);
  R("s1").walkSE([[0, 0, 6], [0, 0, 13]]);
  { const r = R("s1"); r.walk([[-3.5, 0, 5.5], [-3.5, 2.4, 13]]); }
  { const r = R("s2"); r.walk([[0, 0, 2], [0, 0, 22], ], { expect: "blocked", maxSeconds: 6 }); r.walk([[0, -0.8, 9], [0, 0, 6]], { jump: true, maxSeconds: 10 }); r.walk([[0, 0, 13], [0, 0, 19.5]]); }
  { const r = R("s3"); r.proof({ kind: "jumpDash", start: r.pt(0, 0, 1), lipZ: r.Z(7), gap: 15, toY: 0, expectFail: true }); r.walk([[0, -1.0, 8.2], [0, 0, 5]], { jump: true, maxSeconds: 10 }); r.walk([[0, 0, 23], [0, 0, 30.5]]); }
  { const r = R("s4"); r.walkSE([[0, 0, 8], [0, 0, 23.5]]); r.walk([[6, 0, 25.5], [6, 3.0, 13.5]]); r.walk([[-6.5, 0, 1], [-6.5, 1.2, 8]]); r.walk([[-6, 0, 25.5], [-6, 4.0, 15]]); }
  R("s5").walkSE();
  { const r = R("s6"); r.walk([[0, -0.8, 5.2], [0, 0, 1.5]], { jump: true, maxSeconds: 10 }); r.walk([[2, -0.8, 17], [5.5, 0.3, 19.5], [5.5, 3.0, 24]], { jump: true, maxSeconds: 12 }); r.walk([[0, 3.0, 25], [0, 0, 33.5]]);
    r.walk([[5.2, 0, 2], [5.5, 0, 18.4], [5.5, 0.3, 19.5], [5.5, 3.0, 24]], { jump: true, maxSeconds: 14 }); } // NIGHT31 L round 3: the safe route, the dry east strip under the baffle and up the catch steps
  R("s7").walkSE([[-8.5, 0, 6], [-8.5, 0, 22]]);
  R("s8").walkSE([[0, 0, 12], [0, 0, 28]]);
}

L.write("Design/levels/n30/span.plan.json");
