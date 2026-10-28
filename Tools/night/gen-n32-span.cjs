// NIGHT32 (builder S, DESIGN32 card S, row design.n32.span32): generates Design/levels/n32/span.plan.json, LEVEL 4 THE SPAN rebuilt FROM SCRATCH as id n32-span
// (the n30 files stay as they are). Run, from the repo root: node Tools/night/gen-n32-span.cjs, then
//   dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n32/span.plan.json Design/levels/n32-span.json   (0 ERROR rows)
// GG (2026-10-10): "I also want the 4th level to be completely overhauled again"; "the arenas are so boring"; "the grapple is way too broken ... you can just spam
// the grapple"; (2026-10-09) "a lot of the rooms to be a lot smaller and more action packed like ... ULTRAKILL"; "There always really needs to be a route that enemies
// can climb up". NIGHT30's level 4 was 8 boxes in a straight 235 m line with every door on x = 0. This one: 9 rooms that TURN (doors on the side walls, every door
// off the rooms' centre lines), every room at most 24 m across except the Loom's deck, two or more heights per room, lava edges (no void anywhere), a walk loop in
// every room (a slow line and a fast ring line), an enemy climb route (ramp / stairs) to every raised top.
// THE LIMITED GRAPPLE (DESIGN32 G, D-1 / D-3: 2 charges, 4 s each, >= 0.8 s between pulls, a non-boss kill refunds one, landing does not refill, a TAP pulls only to
// rings / boss joints / big enemies, range 40 m): every ring route is at most TWO pulls before ground, a wall ride or a kill; the fast line is pull, wall ride, pull.
// Each room's ring routes are rows `ringRoute32` (read by the harness `span32 rings`, Tools/SimHarness/Harness.N32.Span.cs: the real pull, the 2-charge rule
// simulated around it until G's grapple32 lands).
// Each room's `par` is the median clear time of `span32 walk --seeds 10` (walk29's pilot, the design config as it is; LEVEL-FORMAT.md's rule), measured
// on this layout (NIGHT32 S slice 2); s8's 120 is NIGHT30's (the boss is builder B's).
// Built on NIGHT30's helpers (Tools/night/n30-lib.cjs: Room's boxes / ramps / lava / lights / props / proofs) with this file's own placement (rooms anywhere,
// doors on any wall), because Level.room() only stacks rooms north on one line. Web coordinates, metres: x east, y up, z SOUTH (north is -z).
// Room-local: lx = metres east of the room's centre, lz = metres NORTH of its south edge (n30-lib's convention).
const { Level, Room, WALL, r3 } = require("./n30-lib.cjs");

const L = new Level("n32-span", "LEVEL 4 THE SPAN",
  "LEVEL 4, THE SPAN, rebuilt from scratch (NIGHT32, builder S, DESIGN32 card S; GG 2026-10-10: 'the 4th level ... completely overhauled again', 'the arenas are so boring'). "
  + "A gantry yard over a lava gorge that turns back on itself: s1 TOLLGATE (the NAILGUN, a mezzanine, a lava trench with one bridge), s2 FIRST HOOK (one ring over lava to a terrace; the stairs are the slow way), "
  + "s3 ROPE AND RIDE (two rings with the north wall's ride between, the travel turns east), s3b CRUCIBLE (new: a three-tier fight), s4 HOOK FIGHT (islands in a lava cross, rings over the channel, a moving fight), "
  + "s5 REST, s6 CHAIN (pull, wall ride, pull to the spitters' deck; the south ledge is the walk), s7 ESCALATION (the pylon in its moat with a ring of four, two balconies), s8 THE LOOM (the boss deck, kept). "
  + "Every ring route is at most two pulls before ground, a wall ride or a kill (DESIGN32 G: 2 charges, 4 s). Web coordinates, metres: x east, y up, z SOUTH.",
  { rank: { time: [240, 360, 540, 780], kills: [100, 85, 65, 40], style: [2500, 1500, 700, 250], _why: "Unity-only (builder S, NIGHT32): S/A/B/C cuts for level 4 up to the Loom, NIGHT30's numbers kept (time in seconds, kills = percent of the waves' enemies, style = points; NOT calibrated on this layout)." } });
L.plan.n32 = true;

const rect = (r, lx0, lz0, lx1, lz1) => [r.X(lx0), r.Z(lz1), r.X(lx1), r.Z(lz0)];
const n = (k, c) => Array(c).fill(k);
const rooms = {};

// a room with its world placement: x0 = west edge, z1 = SOUTH edge (the larger z); w along x, l along z
function place(s, x0, z1) {
  const sp = Object.assign({}, s, { cx: x0 + s.w / 2 });
  const r = new Room(L, sp, z1, 0);
  L.rooms.push(r);
  rooms[s.id] = r;
  return r;
}
// spawn and exit in room-local coordinates; yaw 0 faces north, +90 west, -90 east, 180 south
function spawnExit(r, slx, slz, yawDeg, elx, elz) {
  r.row.spawn = { at: r.pt(slx, 0, slz), yawDeg };
  r.row.exit = r.pt(elx, 0, elz);
}
// a door from room a to room b on a's side ('N' | 'E' | 'W' | 'S'); [w0, w1] = the WORLD span along the wall (x for N / S, z for E / W)
function door(a, b, side, w0, w1) {
  const dh = Math.min(4.5, a.h, b.h);
  let rc;
  if (side === "N") { if (Math.abs(a.z0 - WALL * 2 - b.z1) > 1e-6) throw new Error(`${a.id}->${b.id}: not 4 m north`); rc = [w0, b.z1, w1, a.z0]; }
  else if (side === "S") { if (Math.abs(b.z0 - WALL * 2 - a.z1) > 1e-6) throw new Error(`${a.id}->${b.id}: not 4 m south`); rc = [w0, a.z1, w1, b.z0]; }
  else if (side === "E") { if (Math.abs(a.x1 + WALL * 2 - b.x0) > 1e-6) throw new Error(`${a.id}->${b.id}: not 4 m east`); rc = [a.x1, w0, b.x0, w1]; }
  else { if (Math.abs(b.x1 + WALL * 2 - a.x0) > 1e-6) throw new Error(`${a.id}->${b.id}: not 4 m west`); rc = [b.x1, w0, a.x0, w1]; }
  const along = side === "N" || side === "S" ? [[a.x0, a.x1], [b.x0, b.x1]] : [[a.z0, a.z1], [b.z0, b.z1]];
  for (const [p0, p1] of along) if (w0 < p0 + 0.5 || w1 > p1 - 0.5) throw new Error(`${a.id}->${b.id}: door ${w0}..${w1} not inside ${p0}..${p1}`);
  L.doors.push({ id: `d${L.doors.length + 1}`, between: [a.id, b.id], rect: rc.map(r3), height: dh, checkpoint: true });
}
// a ring route for the harness (span32 rings): from a standing point, pull the listed rings in order (the room's ring indices), land in the goal rect
function ringRoute(r, name, from, yawDeg, pulls, goal, note) {
  (r.row.ringRoute32 = r.row.ringRoute32 || []).push({ name, from: r.pt(from[0], from[1], from[2]), yawDeg, pulls, goal: rect(r, goal[0], goal[1], goal[2], goal[3]), goalMinY: goal[4], note });
}

// ================================================================ s1 TOLLGATE (20 x 18): the NAILGUN on its plinth, a mezzanine, a lava trench with one bridge
const s1 = place({ id: "s1", name: "4.1 TOLLGATE", kind: "room", w: 20, l: 18, h: 8, par: 11 }, -10, 0);
{
  const r = s1;
  spawnExit(r, -5, 2, 0, 5, 15.5);
  r.box("deck", -10, 6, -5, 18, 0, 2.4, "the west mezzanine (2.4 m): height over the swarmers' approach, the walk line round the trench");
  r.ramp(-7.5, 0.5, 6, 3, 2.4, "up to the mezzanine (walkers: 5.5 m for 2.4 m)");
  r.box("deck", 4, 3, 10, 9, 0, 1.2, "the east platform (1.2 m)");
  r.ramp(6, 0, 3, 3, 1.2, "up to the east platform (walkers)"); // NIGHT32 S: ramps, not n30-lib's stairs (the report: the 0.4 m risers stop the Mover and the walkers)
  r.lava(-5, 10, 0, 12.5, 0.8, "the trench, west of the bridge (lava edge: the mezzanine's foot)");
  r.lava(2.5, 10, 10, 12.5, 0.8, "the trench, east of the bridge");
  r.solid("plinth", -2.5, 7, 1.2, 0.6, 1.2);
  r.pickup("weaponPickup", -2.5, 7, 1.1, { weapon: "nailgun", note: "the NAILGUN on its plinth, 27 deg off the entry's facing (the room's `award` is the fallback)" });
  r.row.pickup = { weapon: "nailgun", at: r.pt(-2.5, 1.1, 7) };
  r.row.award = "nailgun";
  [[7, 15.5], [-2, 16.5], [3.5, 16]].forEach(([lx, lz]) => r.nodeAt(lx, lz));
  r.highNodeAt(8, 6, 1.2); r.highNodeAt(-7.5, 14, 2.4);
  r.row.waves = [n("swarmer", 4), n("husk", 3)];
  r.row.lessons = [{ kind: "husksDead", n: 4, prompt: "NAILGUN: HOLD TO STREAM.", promptPad: "NAILGUN: HOLD TO STREAM." }];
  r.row.lessons31 = [{ kind: "husksDead", n: 2, prompt: "NAILS STICK. {altFire} DETONATES THEM.", promptPad: "NAILS STICK. {altFire} DETONATES THEM." },
    { kind: "husksDead", n: 4, prompt: "A CHARGED SHOT SETS NAILS OFF.", promptPad: "A CHARGED SHOT SETS NAILS OFF." }];
  r.solid("crate", 8.5, 14, 1.4, 1.2, 1.4); r.solid("crate", -2, 14, 1.4, 1.2, 1.4);
  r.ceil(-2.5, 7, 2, 2, "amber", 8, 1.6); // the plinth's accent: the gun is the hero
  r.trim("W", 3, 3, 0.4, "cyan"); r.trim("E", 13, 3, 0.4, "cyan");
  r.pool(5, 16, 3, "cyan", 7, 1.8);
}

// ================================================================ s2 FIRST HOOK (14 x 26): ONE ring over the lava to a 3.5 m terrace; the east stairs are the slow way
const s2 = place({ id: "s2", name: "4.2 FIRST HOOK", kind: "link", w: 14, l: 26, h: 11, par: 3 }, 0, s1.z0 - 2 * WALL);
door(s1, s2, "N", 3, 7);
{
  const r = s2;
  spawnExit(r, -2, 2, 0, 4.5, 23.5);
  r.lava(-7, 6, 3, 12, 0.8, "the lava at the terrace's foot (a miss burns and knocks you up: climb back out to the south)");
  r.box("deck", -7, 12, 7, 16, 0, 3.5, "the terrace (3.5 m): no jump reaches it, the ring above the lava does; the east ramp climbs it slowly");
  r.ramp(5, 3, 12, 3, 3.5, "the slow way up (the east ramp, walkers: 9 m for 3.5 m)");
  r.ramp(-4, 24, 16, 4, 3.5, "down from the terrace to the exit floor (8 m for 3.5 m)");
  r.grappleNode(-2, 8.5, 10);
  r.row.grapple = "ONE ring at y 8.5 over the lava's north edge (lz 10): a TAP from the entry floor (about 10.6 m) pulls you up and over onto the terrace; the east stairs are the walk (8 m longer, no charge)";
  r.row.lessons = [{ kind: "reached", rect: rect(r, -7, 12, 7, 26), minY: -0.5, prompt: "{grapple} GRAPPLE THE RING.", promptPad: "{grapple} GRAPPLE THE RING." }];
  r.row.lessons31 = [{ kind: "reached", rect: rect(r, -7, 12, 7, 26), minY: -0.5, prompt: "TAP {grapple}: PULL. HOLD: SWING.", promptPad: "TAP {grapple}: PULL. HOLD: SWING." }];
  r.row.gapLz31 = 12; // pull31 / G's chain: past the lava's north edge
  ringRoute(r, "one ring", [-2, 0, 3], 0, [0], [-7, 12, 7, 26, -0.3], "tap the ring from the entry floor: over the lava onto the terrace or past it (span32 rings: the arrival carry lands past the terrace)");
  r.light("wall", [r.X(-2), 2.4, r.Z(12 - 0.05)], [0, 0, 1], [6, 0.25, 0.1], "cyan", 8, 1.6); // a cyan strip on the terrace's face, under the ring
  r.ceil(-2, 10, 2, 2, "white", 10, 1.0);
  r.trim("W", 4, 3, 0.4, "white", 5, 0.9); r.trim("E", 3, 3, 0.4, "cyan", 5, 0.9);
  r.pool(4.5, 23.5, 3, "cyan", 7, 1.8);
}

// ================================================================ s3 ROPE AND RIDE (24 x 16, entered from the WEST): two rings with the north wall's ride between; the south strip is the walk
const s3 = place({ id: "s3", name: "4.3 ROPE AND RIDE", kind: "link", w: 24, l: 16, h: 12, par: 11 }, s2.x1 + 2 * WALL, s2.Z(21) + 0.5);
door(s2, s3, "E", s2.Z(25), s2.Z(21));
{
  const r = s3;
  const dz = r.z1 - (s2.Z(23)); // the door's middle, room-local lz
  spawnExit(r, -10.5, dz, -90, 9, 13.5);
  r.lava(-7, 3, 5, 16, 0.8, "the lava pool against the north wall (the ride lane is the north wall above it)");
  // the rails (1.5 m): from the lava (0.8 m down) a rail top is 2.3 m up, over the bots' graph's jumpUp 2.2, so its path cannot run through the pool
  // (span32 watch s3: the bot's path dropped into the pool, the lava guard held it at the edge, the unstick hopped it in: 80-116 deaths in 10 runs)
  r.box("hull", -7, 2.6, 5.4, 3, 0, 1.5, "the rail along the pool's south edge (1.5 m): the walk strip's edge");
  r.box("hull", 5, 3, 5.4, 16, 0, 1.5, "the rail along the pool's east edge (1.5 m): no way out of the lava east; the rings' carry clears it");
  r.box("hull", -1, 3, 0, 12, -0.8, 12, "the SCREEN: a floor-to-roof slab in the pool (lz 3-12): the exit is out of sight from the entry floor across the lava (span32 watch s3: the bot steered straight at the exit through the pool and burned), the rings' lane along the north wall passes north of it");
  r.box("deck", -12, 10, -8, 16, 0, 2.0, "the west balcony (2.0 m) over the pool's corner: a second height and the start of the high line");
  r.ramp(-10, 4.5, 10, 3, 2.0, "up to the west balcony from the entry (walkers)");
  // NIGHT32 S round 2 (the lead: "s3 is the plainest room ... one more height or a moving threat"): a third height, the TOWER (2.5 m) in the south-east
  // corner with its ramp from the north (the drop off its west edge back to the strip is the loop), a lookout over the pool and the ring line.
  // Tried as a fight (a spitter on the tower, swarmers over the pool, kind room): span32 deaths 0 -> 30 in 10 runs, 29 of them in the pool: s3 stays a link
  r.box("deck", 8, 0, 12, 5, 0, 2.5, "the tower (2.5 m): the spitter's perch over the strip's end; drop off its west edge (the loop)");
  r.ramp(10, 11, 5, 3, 2.5, "up to the tower from the north (walkers: 6 m for 2.5 m)");
  const S3R = (process.env.S3R || "-3,7,14.5,5,7,14.5").split(",").map(Number); // span32 rings tries (ring 1 x y lz, ring 2 x y lz)
  r.grappleNode(S3R[0], S3R[1], S3R[2]); r.grappleNode(S3R[3], S3R[4], S3R[5]);
  r.row.grapple = "two rings at y 7 over the pool (lz 13, 3 m off the north wall): TAP ring 1 from the entry, ride the north wall east, TAP ring 2 over the pool's end onto the east floor; the south strip (lz 0-3) is the walk";
  r.row.lessons = [{ kind: "reached", rect: rect(r, 7.5, 1, 11.5, 15), minY: -0.2, prompt: "PULL. RIDE THE WALL. PULL.", promptPad: "PULL. RIDE THE WALL. PULL." }];
  r.row.lessons31 = [{ kind: "reached", rect: rect(r, 7.5, 1, 11.5, 15), minY: -0.2, prompt: "TAP {grapple}. RIDE. TAP AGAIN.", promptPad: "TAP {grapple}. RIDE. TAP AGAIN." }];
  ringRoute(r, "ring, ride, ring", [-10.5, 0, dz], -90, [0, 1], [6, 0, 12, 16, -0.3], "two pulls (the 2 charges), the ride between; lands on the east side");
  ringRoute(r, "from the balcony", [-10, 2.0, 13], -90, [1], [6, 0, 12, 16, -0.3], "one pull from the west balcony straight to ring 2");
  r.light("wall", [r.X(-0.5), 4.0, r.Z(16 - 0.05)], [0, 0, 1], [11, 0.25, 0.1], "cyan", 8, 1.6); // the ride lane on the north wall
  r.ceil(S3R[0], S3R[2], 2, 2, "white", 11, 1.0); r.ceil(S3R[3], S3R[5], 2, 2, "white", 11, 1.0);
  r.endTrim("S", -0.5, 4, 0.4, "white", 5, 0.9);
  r.pool(9, 13.5, 3, "cyan", 7, 1.8);
}

// ================================================================ s3b CRUCIBLE (22 x 20, new): a three-tier fight (0 / 1.5 / 3 m) with a floor-to-roof column and a lava gutter
const s3b = place({ id: "s3b", name: "4.3b CRUCIBLE", kind: "room", w: 22, l: 20, h: 10, par: 15 }, s3.X(9) - 13, s3.z0 - 2 * WALL);
door(s3, s3b, "N", s3.X(7), s3.X(11));
{
  const r = s3b;
  const dx = s3.X(9) - r.cx;
  spawnExit(r, dx, 2, 0, -9.5, 16);
  r.box("deck", 3, 6, 11, 13, 0, 1.5, "tier 1 (1.5 m)");
  r.box("deck", 3, 13, 11, 20, 0, 3.0, "tier 2 (3.0 m): the spitter's perch");
  r.ramp(7.5, 1, 6, 3, 1.5, "up to tier 1 (walkers)");
  r.ramp(4.5, 7, 13, 3, 3.0, "tier 1 up to tier 2 (walkers; its lower half is inside tier 1, it surfaces at lz 10)");
  r.box("hull", -3, 8, -1, 10, 0, 10, "the column: floor to roof, cover and a wall to kick off");
  r.lava(-8, 3, -6, 9, 0.8, "the lava slot west of the column (2 m wide, off the wall: a wall rider never drops into it; walk30 burned in a wall-side gutter: 26 deaths in 10 runs)");
  r.lava(-4, 14, 1, 17, 0.8, "the hot plate in front of the exit (dash it or go round)");
  [[-6, 16.5], [-2, 19], [2, 15]].forEach(([lx, lz]) => r.nodeAt(lx, lz));
  r.highNodeAt(8.5, 17, 3.0); r.highNodeAt(10, 12.5, 1.5);
  r.row.waves = [["husk", "husk", "husk", "husk", "swarmer", "swarmer"], ["husk", "husk", "husk", "spitter"]];
  r.row.lessons = [{ kind: "husksDead", n: 10, prompt: "TAKE THE HIGH GROUND.", promptPad: "TAKE THE HIGH GROUND." }];
  r.light("wall", [r.X(-2), 6.0, r.Z(8 - 0.05)], [0, 0, 1], [2, 0.3, 0.1], "amber", 9, 1.6); // the column's accent
  r.trim("E", 4, 3, 0.4, "cyan"); r.trim("W", 6, 3, 1.4, "cyan");
  r.ceil(8.5, 16.5, 2, 2, "white", 10, 1.0); r.ceil(-6, 6, 2, 2, "white", 10, 1.0);
  r.pool(-9.5, 17.5, 2.4, "cyan", 7, 1.8);
}

// ================================================================ s4 HOOK FIGHT (24 x 24, entered from the EAST): islands in a lava cross; rings over the channel; a moving fight
const s4 = place({ id: "s4", name: "4.4 HOOK FIGHT", kind: "room", w: 24, l: 24, h: 12, par: 34 }, s3b.x0 - 2 * WALL - 24, s3b.Z(14) + 12);
door(s3b, s4, "W", s3b.Z(18), s3b.Z(14));
{
  const r = s4;
  const dz = r.z1 - s3b.Z(16);
  spawnExit(r, 10.5, dz, 90, 4, 21.5);
  r.lava(-3, 0, -1, 2, 0.8, "the north-south channel (2 m), south of the bridge");
  r.lava(-3, 5, -1, 24, 0.8, "the north-south channel (2 m), north of the bridge");
  r.lava(-1, 11, 4.5, 13, 0.8, "the east-west channel (2 m), west of its bridge");
  r.lava(6.5, 11, 8, 13, 0.8, "the east-west channel (2 m), east of its bridge (its east end is dry: the walk round it)");
  r.box("deck", 4.5, 11, 6.5, 13, -0.8, 0, "round 2: the bridge over the east-west channel (floor level; span32 deaths: 45 lava entries in 10 runs crossing the channels)");
  r.box("deck", -3, 2, -1, 5, -0.8, 0, "the bridge over the north-south channel (floor level)");
  r.box("deck", -12, 12, -3.5, 24, 0, 2.5, "the west gantry (2.5 m): the high side of the fight");
  r.ramp(-8, 6, 12, 4, 2.5, "up to the west gantry (walkers)");
  r.box("hull", -2.5, 11, -1.5, 13, -0.8, 12, "the mast at the channels' crossing (floor to roof)");
  r.ramp(1, 14, 20.5, 3, 2.5, "round 2: up from the north island to the north bridge (walkers)");
  r.box("deck", -3.5, 20.5, 2.5, 22, -0.8, 2.5, "round 2: the north bridge (2.5 m) over the north-south channel onto the gantry: a second way up, the gantry loop");
  r.solid("crate", 2.5, 7, 1.4, 1.1, 1.4); r.solid("crate", 6.5, 18, 1.4, 1.1, 1.4);
  r.grappleNode(-7, 9, 12); r.grappleNode(4, 9, 12);
  r.row.grapple = "two rings at y 9 over the east-west channel (lz 12): the gantry ring and the island ring; a fight on the move between the islands and the gantry, a kill refunds a charge";
  ringRoute(r, "island to gantry", [6, 0, 6], 90, [0], [-12, 12, -3.5, 24, 2.0], "one pull from the south island onto the west gantry");
  ringRoute(r, "gantry and back", [-8, 2.5, 20], -90, [1], [-0.5, 13.5, 12, 24, -0.3], "one pull from the gantry over the channel to the north island");
  [[2, 3.5], [6, 23], [-8, 3]].forEach(([lx, lz]) => r.nodeAt(lx, lz));
  r.highNodeAt(-8, 18, 2.5); r.highNodeAt(-10, 22, 2.5);
  r.row.waves = [["husk", "husk", "husk", "husk", "swarmer", "swarmer", "swarmer"], ["husk", "husk", "husk", "spitter", "spitter"], n("swarmer", 4)];
  r.row.lessons = [{ kind: "husksDead", n: 16, prompt: "NAILS FOR FLYERS.", promptPad: "NAILS FOR FLYERS." }];
  r.light("wall", [r.X(-12 + 0.05), 5.0, r.Z(18)], [1, 0, 0], [3, 0.3, 0.1], "amber", 8, 1.6); // the gantry's accent
  r.trim("E", 5, 3, 0.4, "cyan"); r.trim("E", 21, 3, 0.4, "cyan");
  r.ceil(-7, 12, 2, 2, "white", 11, 1.0); r.ceil(4, 12, 2, 2, "white", 11, 1.0);
  r.pool(4, 21.5, 3, "cyan", 7, 1.8);
}

// ================================================================ s5 REST (10 x 10): health, the Loom's legs glimpsed through a slot
const s5 = place({ id: "s5", name: "4.5 REST", kind: "link", w: 10, l: 10, h: 5, par: 1 }, s4.X(4) - 6, s4.z0 - 2 * WALL);
door(s4, s5, "N", s4.X(2), s4.X(6));
{
  const r = s5;
  const dx = s4.X(4) - r.cx;
  spawnExit(r, dx, 1.5, 0, -3.5, 7);
  r.box("deck", 1, 6, 5, 10, 0, 0.5, "the dais (0.5 m, one step)");
  r.pickup("healthPickup", 3, 8, 1.4, { note: "a health pickup on the dais before the chain" });
  r.decal("slot", 0, 4, 3, 0.4, "white", 0, { note: "the slot in the floor: the Loom's legs glimpsed below (view)" });
  r.trim("W", 3, 3, 0.4, "amber", 5, 1.0); r.trim("E", 3, 3, 0.4, "white", 5, 1.0);
  r.ceil(1, 5, 2, 2, "white", 6, 0.9); r.pool(-3, 7.5, 2.4, "cyan", 5, 1.2);
}

// ================================================================ s6 CHAIN (24 x 22, entered from the EAST): pull, wall ride, pull to the spitters' deck; the south ledge is the walk
const s6 = place({ id: "s6", name: "4.6 CHAIN", kind: "room", w: 24, l: 22, h: 12, par: 11 }, s5.x0 - 2 * WALL - 24, s4.z0 - 2 * WALL);
door(s5, s6, "W", s5.Z(9), s5.Z(5));
{
  const r = s6;
  const dz = r.z1 - s5.Z(7);
  spawnExit(r, 10.5, dz, 90, -10, 20.5);
  // round 2 (the lead: s6's deaths and its pocket): the south ledge 2.5 -> 4 m; the east strip's dead end now runs on into a 2.5 m NORTH LEDGE under
  // the ride lane to the exit floor (the loop: south ledge -> deck -> exit floor <- north ledge <- east strip <- entry); the lava is the middle, with
  // a dry edge on all four sides. Tried: two floor islands in the lava with the pocket turned to lava (span32 deaths 16 -> 23, lava entries 66 -> 97)
  r.lava(-7.5, 4, 8, 19.5, 0.8, "the lava floor, between the 4 m south ledge, the east strip, the 2.5 m north ledge and the deck");
  r.box("deck", -12, 7, -8, 13, 0, 2.5, "the deck (2.5 m): the spitters' perch");
  r.ramp(-10, 1, 7, 3, 2.5, "the slow way up: a ramp from the south ledge to the deck (walkers; its foot 1 m off the south wall, so the ledge's cells reach it)");
  r.ramp(-10, 19, 13, 4, 2.5, "down from the deck to the exit floor");
  const S6R = (process.env.S6R || "4,7,20,-6.5,7,20").split(",").map(Number); // span32 rings tries (ring 1 x y lz, ring 2 x y lz)
  r.grappleNode(S6R[0], S6R[1], S6R[2]); r.grappleNode(S6R[3], S6R[4], S6R[5]);
  r.row.grapple = "two rings at y 7 by the north wall (lz 18.5): TAP ring 1 from the entry, ride the north wall west, TAP ring 2 onto the deck / the exit floor; the 4 m south ledge and the ramps are the walk";
  ringRoute(r, "pull, ride, pull", [10.5, 0, dz], 90, [0, 1], [-12, 6, -8, 22, -0.3], "two pulls with the north wall's ride between; lands on the deck or the exit floor");
  r.highNodeAt(-10, 9, 2.5); r.highNodeAt(-11, 11.5, 2.5);
  r.nodeAt(-6, 2); r.nodeAt(-10, 21);
  r.row.waves = [["spitter", "spitter"], n("swarmer", 4)];
  r.row.lessons = [{ kind: "husksDead", n: 2, prompt: "HOOK. RIDE. HOOK.", promptPad: "HOOK. RIDE. HOOK." }];
  r.row.lessons31 = [{ kind: "husksDead", n: 2, prompt: "TAP {grapple}. RIDE. TAP AGAIN.", promptPad: "TAP {grapple}. RIDE. TAP AGAIN." }];
  r.light("wall", [r.X(0), 4.0, r.Z(22 - 0.05)], [0, 0, 1], [14, 0.25, 0.1], "cyan", 8, 1.6); // the north wall's ride lane
  r.ceil(S6R[0], S6R[2], 2, 2, "white", 11, 1.0); r.ceil(S6R[3], S6R[5], 2, 2, "white", 11, 1.0); r.ceil(-10, 9, 2, 2, "white", 10, 1.0);
  r.trim("E", 16, 3, 0.4, "cyan"); r.endTrim("S", 0, 6, 0.4, "cyan", 6, 1.0);
  r.pool(-10, 20.5, 2.4, "cyan", 7, 1.8);
}

// ================================================================ s7 ESCALATION (24 x 24): the pylon in its moat with a ring of four; two balconies (2.5 / 4.0 m)
const s7 = place({ id: "s7", name: "4.7 ESCALATION", kind: "room", w: 24, l: 24, h: 12, par: 38 }, s6.X(-9) - 10, s6.z0 - 2 * WALL);
door(s6, s7, "N", s6.X(-11.5), s6.X(-7.5));
{
  const r = s7;
  const dx = s6.X(-9.5) - r.cx;
  spawnExit(r, dx, 2, 0, -2, 21.5);
  r.box("hull", -2, 10, 2, 14, 0, 12, "the PYLON: floor to roof; the ring of rings runs round it");
  r.lava(-4, 8, 4, 10, 0.8, "the moat, south"); r.lava(-4, 14, 4, 16, 0.8, "the moat, north");
  r.lava(-4, 10, -2, 14, 0.8, "the moat, west"); r.lava(2, 10, 4, 14, 0.8, "the moat, east");
  r.box("deck", -12, 6, -8, 18, 0, 2.5, "the west balcony (2.5 m)");
  r.ramp(-10, 1, 6, 3, 2.5, "up to the west balcony (walkers)");
  r.box("deck", 8, 13, 12, 24, 0, 4.0, "the east balcony (4.0 m)");
  r.ramp(10, 5, 13, 3, 4.0, "up to the east balcony (walkers: 8 m for 4 m)");
  r.grappleNode(-6, 9, 6.5); r.grappleNode(6, 9, 6.5); r.grappleNode(6, 9, 17.5); r.grappleNode(-6, 9, 17.5);
  r.row.grapple = "four rings at y 9 at the moat's corners: a ring you pull round the pylon while you fight (a kill refunds a charge: the chain lives on kills)";
  ringRoute(r, "balcony to balcony", [-10, 2.5, 16], -90, [2], [8, 13, 12, 24, 3.5], "one pull from the west balcony to the east balcony's ring");
  ringRoute(r, "round the pylon", [-6, 0, 3], 0, [3, 2], [-12, 18, 12, 24, -0.3], "two pulls round the pylon's north side; the third would need a kill");
  [[6, 4], [-6, 22], [5, 21], [0, 20]].forEach(([lx, lz]) => r.nodeAt(lx, lz));
  r.highNodeAt(-10, 15, 2.5); r.highNodeAt(10, 20, 4.0);
  r.row.waves = [n("husk", 5), ["husk", "husk", "husk", "husk", "swarmer", "swarmer", "swarmer", "swarmer"], ["spitter", "spitter", "husk", "husk", "husk"]];
  r.row.lessons = [{ kind: "husksDead", n: 18, prompt: "RING THE PYLON. KEEP MOVING.", promptPad: "RING THE PYLON. KEEP MOVING." }];
  r.solid("crate", -9.5, 21, 1.4, 1.1, 1.4); r.solid("crate", 2.5, 5.5, 1.4, 1.1, 1.4);
  r.light("wall", [r.X(0), 7.5, r.Z(10 - 0.05)], [0, 0, 1], [3, 0.3, 0.1], "amber", 9, 1.8); // the pylon's accent
  r.trim("W", 12, 6, 2.0 + 1.8, "cyan"); r.trim("E", 18, 6, 4.0 + 1.8, "cyan");
  r.ceil(-6, 6.5, 2, 2, "white", 11, 1.0); r.ceil(6, 17.5, 2, 2, "white", 11, 1.0);
  r.pool(-2, 22, 3, "cyan", 7, 1.8);
}

// ================================================================ s8 THE LOOM (the boss deck, KEPT: NIGHT30 / 31's room as it was, moved; loomLegs and deckBreak31 for builder B)
const s8 = place({ id: "s8", name: "4.8 THE LOOM", kind: "boss", w: 34, l: 34, h: 22, par: 120 }, s7.X(-2) - 17, s7.z0 - 2 * WALL);
door(s7, s8, "N", s7.X(-4), s7.X(0));
{
  const r = s8;
  spawnExit(r, 0, 4, 0, 0, 34 - 2.5);
  r.row.boss = "loom";
  r.lava(-17, 0, 17, 8, 0.8, "the lava ring, south (the entry strip crosses it)");
  r.lava(-17, 26, 17, 34, 0.8, "the lava ring, north");
  r.lava(-17, 8, -9, 26, 0.8, "the lava ring, west"); r.lava(9, 8, 17, 26, 0.8, "the lava ring, east");
  r.box("deck", -2, 0, 2, 8, -0.8, 0, "the entry strip over the south lava (floor level)");
  r.box("deck", -2, 26, 2, 34, -0.8, 0, "the exit strip over the north lava (floor level)");
  r.row.loomLegs = [r.pt(-9, 0, 8), r.pt(9, 0, 8), r.pt(9, 0, 26), r.pt(-9, 0, 26)];
  r.row.loomLegs_why = "the four points (world x, y, z) where THE LOOM's legs grip the deck's edge: SW, SE, NE, NW corners of the 18 x 18 deck (builder B: the legs, their two joints each and the body under the deck). NIGHT32 S: the room is NIGHT30's s8 moved, not changed";
  r.grappleNode(-13, 12, 4); r.grappleNode(13, 12, 4); r.grappleNode(13, 12, 30); r.grappleNode(-13, 12, 30);
  r.row.grapple = "four static rings at y 12 over the lava ring by each leg (the leg joints themselves are builder B's moving targets)";
  r.row.deckBreak31 = { rect: [r.X(-5), r.Z(22), r.X(5), r.Z(12)], lavaY: -0.8, when: "half health", tell: "red-orange cracks across the middle",
    _why: "NIGHT31 DESIGN31 C4 / D: the deck's breakable middle (its extent and the lava under it); builder B adds the collider swap and the tell's seconds (design.n31.loom31). NIGHT32 S: kept as it was, relative to the deck." };
  r.trim("W", 17, 8, 2.0, "white", 12, 1.2); r.trim("E", 17, 8, 2.0, "white", 12, 1.2);
  r.ceil(0, 17, 6, 6, "white", 18, 1.2); r.pool(0, 31.5, 3, "cyan", 8, 1.4);
}

// ---------------------------------------------------------------- the layout check: no room's rect comes within two wall thicknesses of another's (walls never cut a room)
for (let i = 0; i < L.rooms.length; i++)
  for (let j = i + 1; j < L.rooms.length; j++) {
    const a = L.rooms[i], b = L.rooms[j];
    const gx = Math.max(a.x0 - b.x1, b.x0 - a.x1), gz = Math.max(a.z0 - b.z1, b.z0 - a.z1);
    if (Math.max(gx, gz) < 2 * WALL - 1e-6) throw new Error(`rooms ${a.id} and ${b.id} overlap their walls (gap x ${gx}, z ${gz})`);
  }

// ---------------------------------------------------------------- completability proofs (the real Mover walks them in the level report)
{
  const R = (id) => rooms[id];
  { const r = R("s1"); r.walkSE([[-3, 0, 4], [1.25, 0, 9], [1.25, 0, 13.5]]); r.walk([[-7.5, 0, 0.8], [-7.5, 2.4, 15]]); r.walk([[6, 0, 0.5], [6, 1.2, 6]]); }
  { const r = R("s2"); r.walk([[5, 0, 2], [5, 0, 2.5], [5, 3.5, 12.5], [0, 3.5, 14], [-4, 3.5, 15.5], [-4, 0, 24.5], [4.5, 0, 23.5]]); r.walk([[0, -0.8, 9], [0, 0, 4]], { jump: true, maxSeconds: 10 }); }
  { const r = R("s3"); r.walkSE([[-6, 0, 1.5], [6.7, 0, 1.5], [6.7, 0, 12]]); r.walk([[-10, 0, 3.5], [-10, 2.0, 13]]); r.walk([[10, 0, 12.5], [10, 2.5, 2.5]]); }
  { const r = R("s3b"); r.walkSE([[2, 0, 8], [0.5, 0, 12.5], [-5.5, 0, 13], [-6, 0, 18]]); r.walk([[7.5, 0, 0.8], [7.5, 1.5, 8], [4.5, 1.5, 9.5], [4.5, 3.0, 15]]); }
  { const r = R("s4"); r.walkSE([[10, 0, 17]]); r.walk([[9, 0, 6], [1, 0, 3.5], [-6, 0, 3.5], [-8, 0, 5.5], [-8, 2.5, 14], [-8, 2.5, 20]]);
    r.walk([[1, 0, 13.5], [1, 2.5, 20.5], [-2, 2.5, 21.2], [-6, 2.5, 21.2]]); r.walk([[5, 0, 9], [5, 0, 15]]); } // round 2: the north bridge up to the gantry; the east-west bridge
  R("s5").walkSE();
  { const r = R("s6"); r.walkSE([[9, 0, 1.2], [-8, 0, 1.2], [-10, 0, 1.2], [-10, 2.5, 8], [-10, 2.5, 12.5], [-10, 0, 19.5]]); r.walk([[0, -0.8, 6], [0, 0, 1.2]], { jump: true, maxSeconds: 10 }); }
  { const r = R("s7"); r.walkSE([[-6, 0, 4], [-6, 0, 18], [-2, 0, 20]]); r.walk([[-10, 0, 1.2], [-10, 2.5, 12]]); r.walk([[10, 0, 4.5], [10, 4.0, 18]]); }
  R("s8").walkSE([[0, 0, 12], [0, 0, 28]]);
}

L.write("Design/levels/n32/span.plan.json");
