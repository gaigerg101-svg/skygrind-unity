// NIGHT33 builder R (DESIGN33 card R): a copy of gen-n32-kiln.cjs with the level id n33-kiln and the output Design/levels/n33/kiln.plan.json
// (design.n33.kit33 swaps n32-kiln for n33-kiln). R reshapes rooms here; builder K's dressing pass (Tools/night/dress33.cjs) runs on the plan after it.
// Run: bash Tools/night/n33-regen.sh kiln
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

const OUT = "Design/levels/n33/kiln.plan.json";
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

// ---------------------------------------------------------------- NIGHT33 R: the overhauls (DESIGN33 R; Tools/night/n33r-lib.cjs)
const R33 = require("./n33r-lib.cjs");
function overhauls33(L) {
  // k1 LOADING BAY (18 x 22; the press, its 2.4 m catwalk and ramp, the THUNDERCLAP pickup and the north spawn row stay): a 1.6 m MEZZANINE in the west aisle
  // with its ramp from the entry floor (enemies climb it), an UPDRAFT FAN in the east aisle up to the catwalk, a HAZARD VENT in the west aisle's north choke
  // (between the mezzanine and the north floor). The press is the landmark.
  R33.overhaul(L, "k1", (r, o) => {
    o.note("k1 LOADING BAY overhauled: a west mezzanine, a fan up to the catwalk, a vent in the west choke");
    o.drop((p) => p.kind === "crate" && Math.abs(p.at[0] - r.X(-7)) < 0.01 && Math.abs(p.at[2] - r.Z(3)) < 0.01); // stood on the mezzanine ramp's line
    r.box("deck", -9, 6, -6.2, 13, 0, 1.6, "NIGHT33 R: the west mezzanine (1.6 m; the aisle east of it stays 3.2 m wide): a view over the press's aisle and the pickup");
    r.ramp(-7.6, 2.6, 6, 2.8, 1.6, "NIGHT33 R: up to the mezzanine from the entry floor (walkers: 3.4 m for 1.6 m)");
    o.solid("crate", -7.5, 1.2, 1.2, 1.2, 1.2); // the entry floor's west cover (the crate the ramp replaced, moved south)
    o.fan(6.5, 11, 8, 12.5, 0, 3.0, [7.5, 0, 8.5], "the catwalk fan: rise in the east aisle, drift north-west onto the 2.4 m catwalk");
    o.vent(-4.8, 14.5, -3.3, 16, 0, 0.3, [-7, 1.6, 11.5], "the west choke vent: between the mezzanine and the north floor");
    o.landmark(-3, 7, 3, 15, "the press itself (k1-P1): the hero is already standing; dress its frame, not the floor");
  });
  // k4 FLUE (14 x 30; the lava flue, the 3.6 m gantry and its ramp, the dry east strip stay): a MOVING PLATFORM up the flue's west side (entry floor <-> north
  // floor, 18 m: the hot route past the gantry), a HAZARD VENT in the dry strip beside the ramp's foot, a plinth in the north-west corner (K's chimney mouth).
  R33.overhaul(L, "k4", (r, o) => {
    o.note("k4 FLUE overhauled: a platform ferry up the flue's west side, a vent in the dry strip, a landmark plinth");
    o.drop((p) => p.kind === "crate" && Math.abs(p.at[0] - r.X(-5)) < 0.01); // the north-west crate: the plinth stands there
    r.box("hull", -6.5, 27.5, -4, 30, 0, 1.4, "NIGHT33 R: the landmark plinth (north-west corner; cover; builder K's chimney mouth)");
    o.platform([-4.5, 0, 6.5], [-4.5, 0, 25.5], [3, 0.5, 3], 0, [-4.5, 0, 3], "the flue ferry: 19 m up the lava's west side, entry floor to north floor");
    // round 3 (the lead: k4 146 deaths over 10 seeds): three floor-high escape islands east of the ferry's lane (the flue was 9 m wide, its west wall 9 m from
    // dry ground): no lava point more than 6 m from dry ground now; the third is a ledge at the gantry's west foot
    // the islands cut 146 -> 107 only (the burns: the 3 m entry floor put the spawn 1 m from the lava, and the west flue was 9 m from dry ground), so the
    // FLUE NARROWS: the lava is lx -7..-1 (6 m, the ferry's lane and the wall lane) and lz 6..25 (the entry floor 6 m deep); the gantry stands on dry floor at
    // its east edge; one escape ledge at the gantry's west foot. The n30 ride proof's hazard band and the jump-out proof follow the new edges.
    const pit = r.pitsOut.find((q) => q.lava);
    pit.rect = R33.rectW(r, -7, 6, -1, 25);
    pit.note = "NIGHT33 R: the narrowed flue (6 m wide, the entry floor 6 m deep): the ferry's lane and the west wall lane";
    for (const p of r.row.proofs || []) {
      if (p.kind === "ride") p.hazardZ = [r.Z(25), r.Z(6)];
      if (p.kind === "walk" && p.jump && Math.abs(p.points[0][1] + 0.8) < 1e-6) p.points = [r.pt(-3, -0.8, 8), r.pt(-3, 0, 4.5)];
    }
    r.box("deck", -2.5, 15.5, -1, 19.5, -1.8, 0, "NIGHT33 R: the escape ledge at the gantry's west foot (floor height)");
    o.vent(4.6, 9, 6.1, 10.5, 0, 0.5, [3.25, 1, 10], "the dry strip vent beside the gantry ramp's foot (refuge: the ramp)");
    o.solid("crate", 6.3, 0.9, 1, 1.2, 1); // the entry floor's south-east corner (arenas33: 9.7 m from cover without it), off the spawn's walk line
    o.solid("crate", 5, 28.5, 1.2, 1.2, 1.2); // the north floor's east cover (off the dry strip's walk line)
    o.landmark(-6.5, 27.5, -4, 30, "a kiln chimney mouth or a crucible over the flue's north end, on the plinth");
  });
}

P.__write = P.write;
P.write = function () {
  overhauls33(this);
  for (const id of Object.keys(built)) { built[id].row.proofs = []; proofs[id](built[id]); }
  this.plan.id = "n33-kiln";
  this.plan._why = "NIGHT33 builder R (design.n33.kit33 swaps n32-kiln for this file): Tools/night/gen-n33-kiln.cjs = gen-n32-kiln.cjs. NIGHT32 builder A (design.n32.arenas32 swaps n30-kiln for this file): Tools/night/gen-n32-kiln.cjs = gen-n30-kiln.cjs with k2 POUR HALL rebuilt and k2b SPLIT LEVEL inserted (new). "
    + this.plan._why;
  return P.__write.call(this, OUT);
};

require("./gen-n30-kiln.cjs");
