// NIGHT33 builder R (DESIGN33 card R): a copy of gen-n32-foundry.cjs with the level id n33-foundry and the output Design/levels/n33/foundry.plan.json
// (design.n33.kit33 swaps n32-foundry for n33-foundry). R reshapes rooms here; builder K's dressing pass (Tools/night/dress33.cjs) runs on the plan after it.
// Run: bash Tools/night/n33-regen.sh foundry
// NIGHT32 builder A (design.n32.arenas32; DESIGN32 card A; GG 2026-10-10: "the arenas are so boring for these stages", "add some new arenas on existing levels",
// "a lot of the rooms to be a lot smaller and more action packed like ... ULTRAKILL", "There always really needs to be a route that enemies can climb up").
// Generates Design/levels/n32/foundry.plan.json (level id n32-foundry) FROM the n30 generator, which stays untouched: this script hooks n30-lib's Level.room / write,
// runs Tools/night/gen-n30-foundry.cjs, and on the way
//   - INSERTS f9b "1.9b LAVA BANKS" (NEW; M3 "Drop Ring" / "Turntable" cut to a room): 20 x 22, lava along both long walls under cyan wall lanes (level 1's verb: ride
//     over the lava), a 1.5 m island in the middle with a ramp at each end (the enemies' climb route), a floor ring round the island (the loop);
//   - REBUILDS f10 "1.10 CRUCIBLE" (the most boring room: NIGHT31's f10 was an 18 x 26 flat floor with one 2 m platform in a hazard strip; M3 "Crucible Tiers"):
//     22 x 22, two lava pockets in the south corners under the wall lanes, two 1.5 m side decks (a ramp up from the south, a ramp down to the north floor), a 2.0 m
//     crown in the middle (its own ramp), floor lanes between the tiers; the floor ring round the crown and the ramp-deck-ramp routes are loops; the room keeps its id
//     (f10), its 12 enemies and its place before f11 / f12;
//   - draws f12's ring (sink31.draw, level-svg only) ROUND: 64 facets and arc platforms (Core/Enemies/Lantern30Arena.cs with design.n32.arenas32 on).
// Round 1 (walk 10 + 10 seeds): a moat round the island and a crucible on the entry line put lava on the bots' straight line; the autopilot has no way out of a
// lava trench (Walk29Pilot rescues only f5 / f5b / f9), so it died there 176-189 times and stalled 2 of 20 runs. Round 2 keeps every hazard on an EDGE (as DESIGN32
// asks: "a hazard edge") and a floor route from the entry to the exit.
// Every other room (ids, lessons, pickups, proofs) is the n30 generator's. Run: bash Tools/night/n32-regen.sh foundry
const lib = require("./n30-lib.cjs");
const { Level, Room, r3 } = lib;

const OUT = "Design/levels/n33/foundry.plan.json";
const built = {}; // id -> the n32 room (its proofs are set at write time: the n30 proofs block would add the OLD room's walks to a kept id)
const proofs = {}; // id -> function(room) adding the n32 proofs

// ---------------------------------------------------------------- f9b LAVA BANKS (new)
function lavaBanks(L) {
  const r = Level.prototype.__room.call(L, { id: "f9b", name: "1.9b LAVA BANKS", kind: "room", w: 20, l: 22, h: 9, cx: 2, outX: 2, par: 30, spawnLz: 2, exitFromN: 2.5 });
  // local: lava lx -10..-7 and 7..10, lz 4..19 (0.8 m down) under the wall lanes; the island lx -3..3, lz 8.5..14.5, top 1.5, a ramp at each end (lx 0, 2.5 wide);
  // the floor ring lx -7..7 round the island; south floor lz 0..4, north floor lz 19..22 wall to wall
  r.lava(-10, 4, -7, 19, 0.8, "the west bank: lava under the west wall lane (ride over it; a touch burns, the rim is 0.8 m)");
  r.lava(7, 4, 10, 19, 0.8, "the east bank: lava under the east wall lane");
  r.box("deck", -3, 8.5, 3, 14.5, 0, 1.5, "the island (1.5 m): the high ground in the middle of the floor ring");
  r.ramp(0, 5, 8.5, 2.5, 1.5, "the island's south ramp (walkers: 3.5 m for 1.5 m)");
  r.ramp(0, 17.5, 14.5, 2.5, 1.5, "the island's north ramp (walkers: 3 m for 1.5 m)");
  [[-6, 4.5], [6, 4.5], [-5, 16], [5, 17], [-4, 20.5], [4.5, 20.5]].forEach(([lx, lz]) => r.nodeAt(lx, lz)); // >= 6 m from the entry; inside the view cone only at >= 12 m
  r.highNodeAt(-1.5, 14.2, 1.5); r.highNodeAt(1.5, 14.2, 1.5);
  r.row.waves = [["husk", "husk", "swarmer", "swarmer"], ["husk", "husk", "spitter"]]; // at most two kinds a wave (the budget)
  r.row.lessons = [{ kind: "husksDead", n: 7, prompt: "RIDE THE BANKS. HOLD THE ISLAND.", promptPad: "RIDE THE BANKS. HOLD THE ISLAND." }];
  r.light("wall", [r.X(-10 + 0.05), 1.8, r.Z(11.5)], [1, 0, 0], [14, 0.25, 0.1], "cyan", 8, 1.5); // the W lane over the lava
  r.light("wall", [r.X(10 - 0.05), 1.8, r.Z(11.5)], [-1, 0, 0], [14, 0.25, 0.1], "cyan", 8, 1.5); // the E lane
  r.pool(-8.5, 11.5, 2.4, "orange", 8, 2.0, { danger: true }, -0.8); // the lava's glow (danger only; one accent per room)
  r.pool(0, 11, 2.4, "cyan", 7, 1.6, undefined, 1.5); // the island top
  r.ceil(0, 11, 4, 4, "white", 10, 0.9); r.ceil(-4, 2.5, 3, 2, "white", 8, 0.9);
  r.pool(0, 20, 3, "cyan", 7, 1.8); // within 8 m of the exit
  r.solid("crate", -5.5, 2, 1.4, 1.2, 1.4); r.solid("pillar", 8.5, 21, 1, 8.9, 1); // the pillar stands in the north-east corner, off the floor ring
  proofs.f9b = (q) => {
    q.walkSE([[-5, 0, 4], [-5, 0, 19.5]]); // the floor route west of the island
    q.walk([[0, 0, 3], [5, 0, 4], [5, 0, 19.5], [-5, 0, 19.5], [-5, 0, 4], [0, 0, 3]]); // the ring round the island (the loop)
    q.walk([[0, 0, 4], [0, 1.5, 11], [0, 0, 18]]); // over the island: up the south ramp, down the north one
    q.walk([[-8.5, -0.8, 8], [-5, 0, 8]], { jump: true, maxSeconds: 6 }); // out of the west bank onto the floor
    q.walk([[8.5, -0.8, 15], [5, 0, 15]], { jump: true, maxSeconds: 6 });
    q.proof({ kind: "ride", from: [q.x0 + 1.2, 0, q.Z(1)], hazardZ: [q.Z(19), q.Z(4)], toZ: q.Z(19.5) }); // the W lane over the bank
  };
  built.f9b = r;
}

// ---------------------------------------------------------------- f10 CRUCIBLE (rebuilt)
function crucible(L) {
  const r = Level.prototype.__room.call(L, { id: "f10", name: "1.10 CRUCIBLE", kind: "room", w: 22, l: 22, h: 10, cx: 0, outX: -2, par: 40, spawnLz: 2, exitFromN: 2.5 });
  // local: lava pockets lx -11..-8 and 8..11, lz 2..9 (the south corners, under the wall lanes); the DECKS lx +-6..+-11, lz 12..19 at 1.5 m (a ramp up from the
  // south at lx +-7, a ramp down to the north floor at lx +-8.5); the CROWN lx -3..3, lz 12..17 at 2.0 m (its ramp from lz 7.5); floor lanes lx +-3..+-6 run
  // from the entry floor to the north floor (lz 17..22): the floor ring round the crown
  r.lava(-11, 2, -8, 9, 0.8, "the west crucible pocket (south-west corner, under the W lane)");
  r.lava(8, 2, 11, 9, 0.8, "the east crucible pocket (south-east corner, under the E lane)");
  r.box("deck", -11, 12, -6, 19, 0, 1.5, "the west deck (1.5 m)");
  r.box("deck", 6, 12, 11, 19, 0, 1.5, "the east deck (1.5 m)");
  r.ramp(-7, 9, 12, 2, 1.5, "up to the west deck from the south (walkers: 3 m for 1.5 m)");
  r.ramp(7, 9, 12, 2, 1.5, "up to the east deck from the south");
  r.ramp(-8.5, 22, 19, 3, 1.5, "the west deck down to the north floor (walkers climb it back up)");
  r.ramp(8.5, 22, 19, 3, 1.5, "the east deck down to the north floor");
  r.box("deck", -3, 12, 3, 17, 0, 2.0, "the crown (2.0 m): the top tier in the middle of the floor ring");
  r.ramp(0, 7.5, 12, 3, 2.0, "up to the crown from the entry floor (walkers: 4.5 m for 2 m)");
  [[-9.5, 10.5], [10, 11.5], [-5.5, 3.5], [-4.5, 20], [4.5, 20.5], [0, 20]].forEach(([lx, lz]) => r.nodeAt(lx, lz));
  r.highNodeAt(-8.5, 15, 1.5); r.highNodeAt(8.5, 15.5, 1.5); r.highNodeAt(-1, 14, 2.0);
  r.row.waves = [["husk", "husk", "husk", "husk"], ["husk", "husk", "swarmer", "swarmer", "swarmer"], ["husk", "spitter", "spitter"]]; // NIGHT30's 12
  r.row.lessons = [{ kind: "husksDead", n: 12, prompt: "TAKE THE HIGH GROUND. LAVA BELOW.", promptPad: "TAKE THE HIGH GROUND. LAVA BELOW." }];
  r.light("wall", [r.X(-11 + 0.05), 2.0, r.Z(6)], [1, 0, 0], [10, 0.25, 0.1], "cyan", 8, 1.5); // the W lane over the pocket
  r.light("wall", [r.X(11 - 0.05), 2.0, r.Z(6)], [-1, 0, 0], [10, 0.25, 0.1], "cyan", 8, 1.5); // the E lane
  r.pool(-9.5, 5.5, 2.4, "orange", 8, 2.0, { danger: true }, -0.8); // the crucible's glow (danger only; one accent per room)
  r.pool(0, 14.5, 3, "cyan", 7, 1.6, undefined, 2.0); // the crown
  r.solid("pillar", 0, 16, 1, 3, 1, 2.0); // the hero on the crown, lit from its foot
  r.ceil(-5, 3, 3, 2, "white", 8, 1.0); r.ceil(0, 11, 4, 4, "white", 10, 0.9); r.ceil(6, 20, 3, 2, "white", 10, 0.8);
  r.pool(-2, 20.5, 3, "cyan", 7, 1.8); // within 8 m of the exit
  r.solid("crate", 5, 2, 1.4, 1.2, 1.4);
  proofs.f10 = (q) => {
    q.walkSE([[4.5, 0, 9], [4.5, 0, 19.5]]); // the floor lane east of the crown, straight to the north floor
    q.walk([[4.5, 0, 6], [4.5, 0, 19.5], [-4.5, 0, 19.5], [-4.5, 0, 6], [4.5, 0, 6]]); // the floor ring round the crown (the loop)
    q.walk([[-7, 0, 7.5], [-8.5, 1.5, 15], [-8.5, 0, 21.5], [8.5, 0, 21.5], [8.5, 1.5, 15], [7, 0, 7.5]]); // deck, north floor, deck: the high loop
    q.walk([[0, 0, 6], [0, 2.0, 14.5]]); // the crown ramp
    q.walk([[-9.5, -0.8, 5], [-6, 0, 5]], { jump: true, maxSeconds: 6 }); // out of a pocket
    q.proof({ kind: "ride", from: [q.x0 + 1.2, 0, q.Z(1)], hazardZ: [q.Z(9), q.Z(2)], toZ: q.Z(9.5) }); // the W lane over the pocket
  };
  built.f10 = r;
}

// ---------------------------------------------------------------- the hooks
const P = Level.prototype;
P.__room = P.room;
P.room = function (s) {
  if (s.id === "f10") {
    lavaBanks(this); // the new arena goes in before f10
    crucible(this);
    const dummy = new Room(this, s, 0, 0); // the n30 block's f10 calls land on a detached room (not in the level)
    dummy.row.spawn = { at: [0, 0, 0], yawDeg: 0 };
    dummy.row.exit = [0, 0, 0];
    return dummy;
  }
  return P.__room.call(this, s);
};

/// f12's ring drawn round (level-svg only): Lantern30Arena's facets with design.n32.arenas32 on (16 panels x 4 facets, apothem 15, platforms on 2 / 7 / 13)
function roundRing(L) {
  const r = L.rooms.find((q) => q.id === "f12");
  if (!r || !r.row.sink31) throw new Error("no f12 sink31 to draw");
  const PANELS = 16, FPP = 4, AP = 15, DEPTH = 2.4, INSET = 0.6, SINK = r.row.sink31.platformPanels;
  const F = PANELS * FPP, fa = 2 * Math.PI / F, pa = 2 * Math.PI / PANELS, R = AP / Math.cos(fa / 2), ccx = r.X(0), ccz = r.Z(r.l / 2);
  const onRing = (a, apo) => { const j = Math.floor(a / fa), rr = apo / Math.cos(a - (j + 0.5) * fa); return [r3(ccx + rr * Math.cos(a)), r3(ccz + rr * Math.sin(a))]; };
  const ring = [];
  for (let j = 0; j <= F; j++) ring.push([r3(ccx + R * Math.cos(j * fa)), r3(ccz + R * Math.sin(j * fa))]);
  const plats = SINK.map((k) => {
    const a0 = k * pa + INSET / AP, a1 = (k + 1) * pa - INSET / AP, br = [a0];
    for (let j = Math.floor(a0 / fa) + 1; j * fa < a1 - 1e-9; j++) br.push(j * fa);
    br.push(a1);
    const back = br.map((a) => onRing(a, AP)), front = br.map((a) => onRing(a, AP - DEPTH)).reverse();
    return { panel: k, poly: back.concat(front) };
  });
  r.row.sink31.draw = { ring, platforms: plats, facets: F };
}

// ---------------------------------------------------------------- NIGHT33 R: the overhauls (DESIGN33 R; Tools/night/n33r-lib.cjs)
const R33 = require("./n33r-lib.cjs");
function overhauls33(L) {
  // f5 WALL-RIDE ROOM made fair (the lead, round 3: the walk pilot died in its 10.5 x 14.5 m lava 291 times over 10 seeds; one run ended in DOWN): a floor-high
  // ISLAND in the middle of the lava (lx -3..1, lz 10.5..14.5): every lava point is now within 4 m of dry ground (the rims or the island), so a touch's knock-up
  // lands the player out; the wall lane stays the only dry way across (the island's gaps are 5 m and 5.5 m: a dash, not a plain jump). The lava-escape
  // proofs at lx -2 (lz 6.7 south, lz 17 north) keep their lines.
  { const r = L.rooms.find((q) => q.id === "f5"); r.box("deck", -3, 10.5, 1, 14.5, -1.8, 0, "NIGHT33 R: the refuge island in the lava (floor height): no lava point more than 4 m from dry ground"); }
  // K's kit33 LAYOUT warnings (cover reach f10 94 %, f9b 96 %): a crate in each entry floor's far corner (not overhauls: no device, no landmark)
  { const r = L.rooms.find((q) => q.id === "f9b"); r.solid("crate", 8.5, 1.2, 1.2, 1.2, 1.2); r.solid("crate", -8.5, 21, 1.2, 1.2, 1.2); }
  { const r = L.rooms.find((q) => q.id === "f10"); r.solid("crate", -6, 1.2, 1.2, 1.2, 1.2); }
  // f9 LAVA LANE (16 x 25; the 10 m lava trench lz 8..18 stays: the blast line, the west wall lane and their lessons are the room's):
  //   a MOVING PLATFORM over the trench's east half (the pistol player's third way across, 7 m with dwell), a 1.5 m GANTRY on the north floor's east side with
  //   its ramp from the trench's north lip (enemies climb it), a low wall on the south floor (cover), a plinth in the north-west corner (K's landmark).
  R33.overhaul(L, "f9", (r, o) => {
    o.note("f9 LAVA LANE overhauled: a platform ferry over the trench, a 1.5 m gantry with its ramp, a low wall, a landmark plinth");
    // the gantry stands on the SOUTH floor (the enemies' side: walkers never cross the trench, so a north tier had no climb route): east of the entry, at
    // the lava's lip, over the ferry's dock; the old 7.9 m pillar stands on it
    r.box("deck", 5, 3.5, 8, 8, 0, 1.5, "NIGHT33 R: the gantry (1.5 m) on the south floor's east side, at the lava's lip: the high ground over the ferry");
    r.ramp(6.5, 0.5, 3.5, 3, 1.5, "NIGHT33 R: up to the gantry from the entry floor, its full width (walkers: 3 m for 1.5 m)");
    r.box("hull", -8, 23, -6, 25, 0, 1.2, "NIGHT33 R: the landmark plinth (north-west corner; cover; builder K stands the hero on it)");
    // cover on the south floor, off the blast line (lx 0, the report's blast-gap run-up) and off the west spawn nodes
    o.solid("lowWall", 4.5, 4.5, 0.6, 1.2, 2.4);
    o.solid("pillar", -7.4, 1, 0.8, 1.2, 0.8);
    o.platform([3.5, 0, 9.8], [3.5, 0, 16.2], [3, 0.5, 3], 0, [3.5, 0, 6], "the lava ferry: rides 7 m over the trench's east half (dwell 1.5 s at each lip)");
    o.landmark(-8, 23, -6, 25, "a foundry ladle or crucible on the plinth, read from the entry across the lava");
  });
  // f8 PISTOL (14 x 20; the 3.2 m shelf, its ramp, the stand-in line at lx 4.5 and the east wall lane stay):
  //   an UPDRAFT FAN beside the ramp's west side (a fast way up to the shelf: rise, drift north), a HAZARD VENT at the ramp's east foot (the tell, then a burst:
  //   do not camp the ramp), a plinth on the shelf's north-west corner (K's landmark; cover up top).
  R33.overhaul(L, "f8", (r, o) => {
    o.note("f8 PISTOL overhauled: a fan up to the shelf, a vent at the ramp's foot, a landmark plinth on the shelf");
    o.fan(-6.8, 11.5, -5.2, 13.5, 0, 3.5, [-6, 0, 9.5], "the shelf fan: up the column, drift north onto the 3.2 m shelf");
    o.vent(-0.5, 9.5, 1.0, 11.0, 0, 0, [-0.2, 0, 13], "the ramp-foot vent: a tell, then a burst; step off or jump");
    r.box("hull", -7, 18.5, -5.5, 20, 3.2, 4.4, "NIGHT33 R: the landmark plinth on the shelf (cover up top; builder K stands the hero on it)");
    o.landmark(-7, 18.5, -5.5, 20, "a foundry bell or gauge tower on the shelf's plinth, seen from the door over the cover");
  });
}

P.__write = P.write;
P.write = function () {
  roundRing(this);
  overhauls33(this);
  for (const id of Object.keys(built)) { built[id].row.proofs = []; proofs[id](built[id]); }
  this.plan.id = "n33-foundry";
  this.plan._why = "NIGHT33 builder R (design.n33.kit33 swaps n32-foundry for this file): Tools/night/gen-n33-foundry.cjs = gen-n32-foundry.cjs. NIGHT32 builder A (design.n32.arenas32 swaps n30-foundry for this file): Tools/night/gen-n32-foundry.cjs = gen-n30-foundry.cjs with f9b LAVA BANKS inserted (new), f10 CRUCIBLE rebuilt and f12's ring drawn round. "
    + this.plan._why;
  return P.__write.call(this, OUT);
};

require("./gen-n30-foundry.cjs");
