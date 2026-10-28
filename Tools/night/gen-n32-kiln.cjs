// NIGHT32 builder A (design.n32.arenas32; DESIGN32 card A; GG 2026-10-10: "the arenas are so boring for these stages", "add some new arenas on existing levels",
// "a lot of the rooms to be a lot smaller and more action packed like ... ULTRAKILL", "There always really needs to be a route that enemies can climb up").
// Generates Design/levels/n32/kiln.plan.json (level id n32-kiln) FROM the n30 generator, which stays untouched: this script hooks n30-lib's Level.room / write,
// runs Tools/night/gen-n30-kiln.cjs, and on the way
//   - REBUILDS k2 "3.2 POUR HALL" (the most boring room: NIGHT31's k2 was 18 x 28, a flat floor, a lava pit crossed by a 2 m bridge, one shelf): 20 x 24, a GANTRY
//     floating 3 m over the floor across the room (2.6 m clear under it: the floor runs on beneath), a ramp up to it from the south-west and one from the north-east
//     (the enemies' climb route), lava along the west wall under a cyan wall lane; the floor under the gantry and the ramp-gantry-ramp route make the loop. It keeps
//     its id (k2: Walk29Pilot's k2 script walks the centre line north once the room is clear, and the centre line is floor), its 12 enemies and the spitters' height;
//   - INSERTS k2b "3.2b SPLIT LEVEL" (NEW): 20 x 22, the west half a 2.0 m deck (a ramp at each end), the east half floor with lava along the east wall under a wall
//     lane: shotgun on the floor, pistol at the ledge; the loop runs up one ramp, along the deck, down the other and back on the floor.
// Every hazard sits on an EDGE (n32-foundry round 1: lava on the bots' straight line killed the autopilot 176-189 times), and the centre line is floor.
// Every other room (ids, lessons, the THUNDERCLAP pickup in k1, proofs, k5's crumble overlay) is the n30 generator's. Run: bash Tools/night/n32-regen.sh kiln
const lib = require("./n30-lib.cjs");
const { Level, Room } = lib;

const OUT = "Design/levels/n32/kiln.plan.json";
const husks = (n) => Array(n).fill("husk");
const built = {};
const proofs = {};

// ---------------------------------------------------------------- k2 POUR HALL (rebuilt)
function pourHall(L) {
  const r = Level.prototype.__room.call(L, { id: "k2", name: "3.2 POUR HALL", kind: "room", w: 20, l: 24, h: 10, cx: 3, outX: 3, par: 35, spawnLz: 2, exitFromN: 2.5 });
  // local: lava lx -10..-7, lz 5..19 (the W lane over it); the GANTRY lx -7..10, lz 11..14, a slab y 2.6..3.0; the SW ramp lx -5 (2.5 wide) lz 5..11 up to the gantry's
  // south edge; the NE ramp lx 8.5 lz 20..14 up to its north edge; floor everywhere else (the centre line lx 0 runs from the door to the exit under the gantry)
  r.lava(-10, 5, -7, 19, 0.8, "the pour channel: lava along the west wall under the W lane (a touch burns; the rim is 0.8 m)");
  r.box("deck", -7, 11, 10, 14, 2.6, 3.0, "the GANTRY (top 3.0 m, 2.6 m clear under it): the spitters' walk, across the room over the floor");
  r.ramp(-5, 5, 11, 2.5, 3.0, "the south-west ramp up to the gantry (walkers: 6 m for 3 m)");
  r.ramp(8.5, 20, 14, 2.5, 3.0, "the north-east ramp up to the gantry from the north floor (walkers)");
  [[6, 4.5], [-6, 4], [8, 6], [-3, 21], [3, 21.5], [5.5, 18.5], [-5, 20]].forEach(([lx, lz]) => r.nodeAt(lx, lz));
  r.highNodeAt(-4, 13.5, 3.0); r.highNodeAt(4.5, 13.5, 3.0); r.highNodeAt(8, 12.5, 3.0);
  r.row.waves = [husks(6), [...husks(4), "spitter", "spitter"]]; // NIGHT30's 12
  r.row.lessons = [{ kind: "husksDead", n: 12, prompt: "SPITTERS UP TOP. PISTOL OR CLIMB.", promptPad: "SPITTERS UP TOP. PISTOL OR CLIMB." }];
  r.light("wall", [r.X(-10 + 0.05), 1.8, r.Z(12)], [1, 0, 0], [14, 0.25, 0.1], "cyan", 8, 1.6); // the W lane over the pour channel
  r.pool(-8.5, 8, 2.4, "orange", 8, 2.0, { danger: true }, -0.8); // the lava's glow (danger only; one accent per room)
  r.pool(1.5, 12.5, 2.4, "cyan", 7, 1.5, undefined, 3.0); // the gantry's top
  r.ceil(0, 4, 4, 3, "white", 10, 0.9); r.ceil(0, 18, 4, 3, "white", 10, 0.9);
  r.pool(0, 21, 3, "cyan", 7, 1.8); // within 8 m of the exit
  r.solid("crate", 6, 8, 1.6, 1.2, 1.6); r.solid("crate", -3, 17, 1.6, 1.2, 1.6); // cover on the floor (off the centre line)
  proofs.k2 = (q) => {
    q.walkSE([[0, 0, 5], [0, 0, 20]]); // the centre line, under the gantry
    q.walk([[-5, 0, 4], [-5, 3.0, 12.5], [8.5, 3.0, 12.5], [8.5, 0, 20.5], [0, 0, 20.5], [0, 0, 4], [-5, 0, 4]]); // the loop: up, across, down, back under
    q.walk([[-8.5, -0.8, 17], [-5, 0, 17]], { jump: true, maxSeconds: 6 }); // out of the pour channel
    q.proof({ kind: "ride", from: [q.x0 + 1.2, 0, q.Z(1)], hazardZ: [q.Z(19), q.Z(5)], toZ: q.Z(19.5) }); // the W lane over the channel
  };
  built.k2 = r;
}

// ---------------------------------------------------------------- k2b SPLIT LEVEL (new)
function splitLevel(L) {
  const r = Level.prototype.__room.call(L, { id: "k2b", name: "3.2b SPLIT LEVEL", kind: "room", w: 20, l: 22, h: 9, cx: 3, outX: 3, par: 30, spawnLz: 2, exitFromN: 2.5 });
  // local: the west DECK lx -10..-1, lz 6..17, top 2.0 (a ramp at lx -5.5 from the south, lz 2..6, and from the north, lz 21..17); lava lx 7..10, lz 4..18 along the
  // east wall under the E lane; floor lx -1..7 the whole length (the door, the exit and the centre line)
  r.box("deck", -10, 6, -1, 17, 0, 2.0, "the west deck (2.0 m): the ledge half of the room");
  r.ramp(-5.5, 2, 6, 3, 2.0, "the deck's south ramp (walkers: 4 m for 2 m)");
  r.ramp(-5.5, 21, 17, 3, 2.0, "the deck's north ramp (walkers)");
  r.lava(7, 4, 10, 18, 0.8, "the east channel: lava along the east wall under the E lane");
  [[6, 4.5], [3, 16], [5, 20], [0, 20], [-8.5, 19.5]].forEach(([lx, lz]) => r.nodeAt(lx, lz));
  r.highNodeAt(-7, 12, 2.0); r.highNodeAt(-3, 14, 2.0); r.highNodeAt(-8, 15, 2.0);
  r.row.waves = [["husk", "husk", "husk", "spitter"], ["husk", "husk", "heavy"]];
  r.row.lessons = [{ kind: "husksDead", n: 7, prompt: "SHOTGUN BELOW. PISTOL THE LEDGE.", promptPad: "SHOTGUN BELOW. PISTOL THE LEDGE." }];
  r.light("wall", [r.X(10 - 0.05), 1.8, r.Z(11)], [-1, 0, 0], [14, 0.25, 0.1], "cyan", 8, 1.6); // the E lane over the channel
  r.pool(8.5, 15, 2.4, "orange", 8, 2.0, { danger: true }, -0.8); // the lava's glow (danger only; one accent per room)
  r.pool(-5.5, 11.5, 2.4, "cyan", 7, 1.5, undefined, 2.0); // the deck
  r.ceil(3, 5, 3, 3, "white", 10, 0.9); r.ceil(-5, 11, 4, 4, "white", 10, 0.9);
  r.pool(3, 20, 3, "cyan", 7, 1.8); // within 8 m of the exit
  r.solid("crate", 0.5, 9, 1.6, 1.2, 1.6); r.solid("crate", 5, 15.5, 1.6, 1.2, 1.6); // cover on the floor half (off the lx 3 line)
  proofs.k2b = (q) => {
    q.walkSE([[3, 0, 5], [3, 0, 19.5]]); // the floor half, door to exit
    q.walk([[-5.5, 0, 1], [-5.5, 2.0, 11], [-5.5, 0, 21.5], [3, 0, 21], [3, 0, 1.5], [-5.5, 0, 1]]); // the loop over the deck and back on the floor
    q.walk([[8.5, -0.8, 10], [5, 0, 10]], { jump: true, maxSeconds: 6 }); // out of the east channel
    q.proof({ kind: "ride", from: [q.x1 - 1.2, 0, q.Z(1)], hazardZ: [q.Z(18), q.Z(4)], toZ: q.Z(18.5), lean: -0.25 }); // the E lane over the channel
  };
  built.k2b = r;
}

// ---------------------------------------------------------------- the hooks
const P = Level.prototype;
P.__room = P.room;
P.room = function (s) {
  if (s.id === "k2") {
    pourHall(this);
    splitLevel(this); // the new arena goes in after the rebuilt k2, before k3
    const dummy = new Room(this, s, 0, 0); // the n30 block's k2 calls land on a detached room (not in the level)
    dummy.row.spawn = { at: [0, 0, 0], yawDeg: 0 };
    dummy.row.exit = [0, 0, 0];
    return dummy;
  }
  return P.__room.call(this, s);
};

P.__write = P.write;
P.write = function () {
  for (const id of Object.keys(built)) { built[id].row.proofs = []; proofs[id](built[id]); }
  this.plan.id = "n32-kiln";
  this.plan._why = "NIGHT32 builder A (design.n32.arenas32 swaps n30-kiln for this file): Tools/night/gen-n32-kiln.cjs = gen-n30-kiln.cjs with k2 POUR HALL rebuilt and k2b SPLIT LEVEL inserted (new). "
    + this.plan._why;
  return P.__write.call(this, OUT);
};

require("./gen-n30-kiln.cjs");
