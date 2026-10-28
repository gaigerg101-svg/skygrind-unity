// NIGHT33 builder R (DESIGN33 card R): a copy of gen-n32-iceline.cjs with the level id n33-iceline and the output Design/levels/n33/iceline.plan.json
// (design.n33.kit33 swaps n32-iceline for n33-iceline). R reshapes rooms here; builder K's dressing pass (Tools/night/dress33.cjs) runs on the plan after it.
// Run: bash Tools/night/n33-regen.sh iceline
// NIGHT32 builder A (design.n32.arenas32; DESIGN32 card A; GG 2026-10-10: "the arenas are so boring for these stages", "add some new arenas on existing levels",
// "a lot of the rooms to be a lot smaller and more action packed like ... ULTRAKILL", "it needs to feel like speed skating ... you snap on to it" (2026-10-09),
// "There always really needs to be a route that enemies can climb up").
// Generates Design/levels/n32/iceline.plan.json (level id n32-iceline) FROM the n30 generator, which stays untouched: this script hooks n30-lib's Level.room / write,
// runs Tools/night/gen-n30-iceline.cjs, and on the way
//   - REBUILDS i6 "2.6 FIGURE EIGHT" (the most boring room: NIGHT31's i6 was 18 x 28, one ice sheet between two flat 1.2 m banks; M3 "Figure-8 Rink"): 22 x 22, an
//     ice sheet round two staggered 1.5 m blocks (a ramp onto each), so the skate line is a figure eight; lava pockets in the south-east and north-west corners
//     under the wall lanes; it keeps its id (i6), its 8 enemies (two tanks) and its lesson;
//   - INSERTS i6b "2.6b SKATE RING" (NEW; M3 "Turntable" cut to a room): 20 x 22, ice wall to wall round a 2.0 m centre deck (a ramp at each end), lava pockets in the
//     south-west and north-east corners under the lanes: skate the ring, take the deck, punch the tank.
// Every hazard sits on an EDGE (n32-foundry round 1: lava on the bots' straight line killed the autopilot 176-189 times); the line from the door to the exit is dry.
// The gauntlet's pickup31 in i1a, i2's blast gap, i4 / i5's lanes, every other id / lesson / proof are the n30 generator's. Run: bash Tools/night/n32-regen.sh iceline
const lib = require("./n30-lib.cjs");
const { Level, Room } = lib;

const OUT = "Design/levels/n33/iceline.plan.json";
const husks = (n) => Array(n).fill("husk");
const built = {};
const proofs = {};

// ---------------------------------------------------------------- i6 FIGURE EIGHT (rebuilt)
function figureEight(L) {
  const r = Level.prototype.__room.call(L, { id: "i6", name: "2.6 FIGURE EIGHT", kind: "room", w: 22, l: 22, h: 9, cx: 0, outX: -2, par: 45, spawnLz: 2, exitFromN: 2.5 });
  // local: the west block lx -7..-2, lz 6..11 (1.5 m, its ramp from the south at lx -4.5, lz 2.5..6); the east block lx 2..7, lz 11..16 (1.5 m, its ramp from the
  // north at lx 4.5, lz 19.5..16); lava lx 8..11, lz 2..8 (SE) and lx -11..-8, lz 14..20 (NW); ice lx -8..8, lz 2..20 (the figure eight round the blocks)
  r.surface("ice", -8, 2, 8, 20, { maxY: 2, note: "the figure-eight sheet round the two blocks (lane rule: one lane the sheet's width)", lane: { axis: "z", line: r.X(0), halfWidth: 8, dir: [0, -1] } });
  r.lava(8, 2, 11, 8, 0.8, "the south-east pocket: lava under the E lane (a touch burns; the rim is 0.8 m)");
  r.lava(-11, 14, -8, 20, 0.8, "the north-west pocket: lava under the W lane");
  r.box("deck", -7, 6, -2, 11, 0, 1.5, "the west block (1.5 m)");
  r.box("deck", 2, 11, 7, 16, 0, 1.5, "the east block (1.5 m)");
  r.ramp(-4.5, 2.5, 6, 2.5, 1.5, "onto the west block from the south (walkers: 3.5 m for 1.5 m)");
  r.ramp(4.5, 19.5, 16, 2.5, 1.5, "onto the east block from the north (walkers)");
  [[-9.5, 4], [10, 12.5], [-3, 15], [-6, 18.5], [3, 20.5], [6, 20]].forEach(([lx, lz]) => r.nodeAt(lx, lz));
  r.highNodeAt(-6.5, 6.5, 1.5); r.highNodeAt(4.5, 14, 1.5); r.highNodeAt(5.5, 15, 1.5);
  r.row.waves = [husks(2), [...husks(2), "tank"], [...husks(2), "tank"]]; // NIGHT30's 8
  r.row.lessons = [{ kind: "husksDead", n: 8, prompt: "TANKS FIRST. PUNCH THEM.", promptPad: "TANKS FIRST. PUNCH THEM." }];
  r.light("wall", [r.X(-11 + 0.05), 2.6, r.Z(12)], [1, 0, 0], [16, 0.25, 0.1], "cyan", 8, 1.5); // the W lane
  r.light("wall", [r.X(11 - 0.05), 2.6, r.Z(10)], [-1, 0, 0], [16, 0.25, 0.1], "cyan", 8, 1.5); // the E lane
  r.pool(9.5, 5, 2.4, "orange", 8, 2.0, { danger: true }, -0.8); // the lava's glow (danger only; one accent per room)
  r.pool(-4.5, 8.5, 2.4, "cyan", 7, 1.5, undefined, 1.5); r.pool(4.5, 13.5, 2.4, "cyan", 7, 1.5, undefined, 1.5); // the block tops
  r.ceil(0, 5, 4, 3, "white", 11, 0.8); r.ceil(0, 16, 4, 3, "white", 11, 0.8);
  r.pool(-2, 20.5, 3, "cyan", 7, 1.8); // within 8 m of the exit
  r.decal("frost", 0, 11, 6, 6, "white");
  proofs.i6 = (q) => {
    q.walkSE([[-1, 0, 6], [-0.5, 0, 16], [-2, 0, 19.5]]); // the dry line between the blocks
    q.walk([[0, 0, 4], [9, 0, 9], [9, 0, 18.5], [0, 0, 18.5], [-9.5, 0, 12], [-9.5, 0, 4.5], [0, 0, 4]]); // round both blocks (the outer loop)
    q.walk([[-4.5, 0, 2.5], [-4.5, 1.5, 9]]); q.walk([[4.5, 0, 20.5], [4.5, 1.5, 13.5]]); // onto each block
    q.walk([[9.5, -0.8, 5], [6, 0, 5]], { jump: true, maxSeconds: 6 }); q.walk([[-9.5, -0.8, 17], [-6, 0, 17]], { jump: true, maxSeconds: 6 }); // out of the pockets
  };
  built.i6 = r;
}

// ---------------------------------------------------------------- i6b SKATE RING (new)
function skateRing(L) {
  const r = Level.prototype.__room.call(L, { id: "i6b", name: "2.6b SKATE RING", kind: "room", w: 20, l: 22, h: 9, cx: -2, outX: -2, par: 35, spawnLz: 2, exitFromN: 2.5 });
  // local: the centre deck lx -3.5..3.5, lz 7.5..14.5, top 2.0 (ramps at lx 0: south lz 3.5..7.5, north lz 18.5..14.5); lava lx -10..-7, lz 3..9 (SW) and lx 7..10,
  // lz 13..19 (NE) under the lanes; ice lx -7..7, lz 1..21 (the ring round the deck)
  r.surface("ice", -7, 1, 7, 21, { maxY: 2.5, note: "the skate ring round the centre deck", lane: { axis: "z", line: r.X(0), halfWidth: 7, dir: [0, -1] } });
  r.lava(-10, 3, -7, 9, 0.8, "the south-west pocket: lava under the W lane");
  r.lava(7, 13, 10, 19, 0.8, "the north-east pocket: lava under the E lane");
  r.box("deck", -3.5, 7.5, 3.5, 14.5, 0, 2.0, "the centre deck (2.0 m): the high ground inside the ring");
  r.ramp(0, 3.5, 7.5, 2.5, 2.0, "the deck's south ramp (walkers: 4 m for 2 m)");
  r.ramp(0, 18.5, 14.5, 2.5, 2.0, "the deck's north ramp (walkers)");
  [[-6, 4.5], [6, 4.5], [-5.5, 16], [6, 14], [-4, 20.5], [4, 20.5]].forEach(([lx, lz]) => r.nodeAt(lx, lz));
  r.highNodeAt(-2, 14, 2.0); r.highNodeAt(2, 14, 2.0);
  r.row.waves = [husks(4), [...husks(2), "tank"]];
  r.row.lessons = [{ kind: "husksDead", n: 7, prompt: "SKATE THE RING. PUNCH THE TANK.", promptPad: "SKATE THE RING. PUNCH THE TANK." }];
  r.light("wall", [r.X(-10 + 0.05), 2.4, r.Z(8)], [1, 0, 0], [12, 0.25, 0.1], "cyan", 8, 1.5); // the W lane
  r.light("wall", [r.X(10 - 0.05), 2.4, r.Z(14)], [-1, 0, 0], [12, 0.25, 0.1], "cyan", 8, 1.5); // the E lane
  r.pool(8.5, 16, 2.4, "orange", 8, 2.0, { danger: true }, -0.8); // the lava's glow (danger only; one accent per room)
  r.pool(0, 11, 2.4, "cyan", 7, 1.5, undefined, 2.0); // the deck
  r.ceil(0, 11, 4, 4, "white", 10, 0.8); r.ceil(-5, 3, 3, 2, "white", 8, 0.9);
  r.pool(0, 20.5, 3, "cyan", 7, 1.8); // within 8 m of the exit
  r.decal("frost", 5, 6, 3, 3, "white");
  proofs.i6b = (q) => {
    q.walkSE([[-5, 0, 4], [-5, 0, 19.5]]); // the west side of the ring
    q.walk([[0, 0, 2.5], [5, 0, 3], [5, 0, 19.5], [-5, 0, 19.5], [-5, 0, 3], [0, 0, 2.5]]); // the whole ring (the loop)
    q.walk([[0, 0, 2.5], [0, 2.0, 11], [0, 0, 19.5]]); // over the deck
    q.walk([[-8.5, -0.8, 6], [-5, 0, 6]], { jump: true, maxSeconds: 6 }); q.walk([[8.5, -0.8, 16], [5, 0, 16]], { jump: true, maxSeconds: 6 });
  };
  built.i6b = r;
}

// ---------------------------------------------------------------- the hooks
const P = Level.prototype;
P.__room = P.room;
P.room = function (s) {
  if (s.id === "i6") {
    figureEight(this);
    skateRing(this); // the new arena goes in after the rebuilt i6, before i7
    const dummy = new Room(this, s, 0, 0); // the n30 block's i6 calls land on a detached room (not in the level)
    dummy.row.spawn = { at: [0, 0, 0], yawDeg: 0 };
    dummy.row.exit = [0, 0, 0];
    return dummy;
  }
  return P.__room.call(this, s);
};

// ---------------------------------------------------------------- NIGHT33 R: the overhauls (DESIGN33 R; Tools/night/n33r-lib.cjs)
const R33 = require("./n33r-lib.cjs");
function overhauls33(L) {
  // i2 PULSE ROOM (16 x 25; the 10 m trench, the blast line lx 0, the five-kill lesson and the spawn nodes stay): a 1.5 m ICE SHELF on the south floor's west
  // side with its ramp from the entry floor (enemies climb it), an ICE FLOE (moving platform) over the trench's west half, a HAZARD VENT on the north floor's
  // west side (the landing is not safe ground), a plinth in the north-east corner (K's landmark).
  R33.overhaul(L, "i2", (r, o) => {
    o.note("i2 PULSE ROOM overhauled: a 1.5 m ice shelf with its ramp, an ice floe over the trench, a vent on the landing, a landmark plinth");
    r.box("deck", -6, 3.5, -3, 8, 0, 1.5, "NIGHT33 R: the ice shelf (1.5 m) on the south floor's west side, at the trench's lip");
    r.ramp(-4.5, 0.5, 3.5, 3, 1.5, "NIGHT33 R: up to the ice shelf from the entry floor (walkers: 3 m for 1.5 m)");
    // the n30 proof "out of the trench onto the south floor" ran at lx -4, now the shelf's face (1.5 m): the same proof at lx -1 (open floor)
    for (const p of r.row.proofs || []) if (p.kind === "walk" && p.jump && p.points[0][0] === r.X(-4)) p.points = p.points.map((q) => [r.X(-1), q[1], q[2]]);
    o.platform([-4.5, 0, 9.8], [-4.5, 0, 16.2], [3, 0.5, 3], 0.5, [-4.5, 1.5, 6], "the ice floe: 6.4 m over the trench's west half, from below the ice shelf");
    o.vent(-3.5, 19.5, -2, 21, 0, 0.25, [-6.5, 0, 20], "the landing vent on the north floor's west side");
    r.box("hull", 6, 23, 8, 25, 0, 1.2, "NIGHT33 R: the landmark plinth (north-east corner; cover; builder K's hero)");
    o.landmark(6, 23, 8, 25, "an ice pylon or frozen bell on the plinth, seen across the trench from the entry");
  });
  // i5 TWO LANES (14 x 26; the ice lanes, rails, divider, switch blocks, the far bank and its ramp stay): an UPDRAFT FAN east of the far bank (from the lanes'
  // exit straight up onto the 2.0 m bank), a HAZARD VENT on the north floor's east side, a plinth on the bank's west end (K's landmark).
  R33.overhaul(L, "i5", (r, o) => {
    o.note("i5 TWO LANES overhauled: a fan up to the far bank, a vent on the north floor, a landmark plinth on the bank");
    o.fan(0.5, 20.5, 2, 22, 0, 2.5, [3, 0, 19], "the bank fan: rise beside the bank's east edge, drift west onto it");
    o.vent(3.5, 20.5, 5, 22, 0, 0.6, [3, 0, 18.5], "the north-east vent: the lanes' exit is not a resting place");
    r.box("hull", -7, 24, -5.5, 26, 2, 3.2, "NIGHT33 R: the landmark plinth on the far bank's west end (builder K's hero)");
    o.landmark(-7, 24, -5.5, 26, "a skate-rink lamp or an ice crystal cluster on the bank's plinth, seen down both lanes");
  });
}

P.__write = P.write;
P.write = function () {
  overhauls33(this);
  for (const id of Object.keys(built)) { built[id].row.proofs = []; proofs[id](built[id]); }
  this.plan.id = "n33-iceline";
  this.plan._why = "NIGHT33 builder R (design.n33.kit33 swaps n32-iceline for this file): Tools/night/gen-n33-iceline.cjs = gen-n32-iceline.cjs. NIGHT32 builder A (design.n32.arenas32 swaps n30-iceline for this file): Tools/night/gen-n32-iceline.cjs = gen-n30-iceline.cjs with i6 FIGURE EIGHT rebuilt and i6b SKATE RING inserted (new). "
    + this.plan._why;
  return P.__write.call(this, OUT);
};

require("./gen-n30-iceline.cjs");
