// NIGHT34 builder A (NIGHT34.md A1 leftover 6, design.n34.lines34 swaps n33-iceline for n34-iceline): LEVEL 2 THE ICE LINE with a movement line in its
// fight rooms. It runs Tools/night/gen-n33-iceline.cjs UNCHANGED (which runs gen-n30-iceline.cjs) with Level.room / Level.write hooks placed BEFORE it
// loads: i1a gets 22 m of length (20 before), then after NIGHT33's overhauls the lines34 pass below, the id n34-iceline and the output
// Design/levels/n34/iceline.plan.json. Then dress33 and levelgen: bash Tools/night/n34-regen.sh iceline. Spacings from `lines34 measure` (a ride 9.60 m,
// a 3-jump lane 20.8 m, grapple reach 40 m); the checker: `lines34 n34-iceline`.
// Rooms with a Walk29Pilot script of their own (i2's blast line, i4, i5) get no node: Span30Pilot drives every room with grapple nodes in place of the
// room's script (foundry round 2: f7 0 / 10, f9 9 / 10). They keep NIGHT33's shape.
const lib = require("./n30-lib.cjs");
const { Level } = lib;
const R33 = require("./n33r-lib.cjs");

const OUT = "Design/levels/n34/iceline.plan.json";
const P = Level.prototype;
const LONGER = { i1a: 22 };

const roomBase = P.room;
P.room = function (s) {
  if (LONGER[s.id]) s = Object.assign({}, s, { l: LONGER[s.id] });
  return roomBase.call(this, s);
};

function lines34(L) {
  const note = (r, s) => { r.row.lines34 = s; };
  // i1a TANK ALONE (now 12 x 22): a 1.5 m deck in the north-EAST corner with its ramp (the enemies' way up; the room's proof walk runs up the west side at
  // lx -4.5), the west strip lx -5 the lane, the east crate moved off the ramp (to lx 1.5, lz 9), a node over the floor between the deck and the west wall
  R33.overhaul(L, "i1a", (r, o) => {
    o.drop((p) => p.kind === "crate" && Math.abs(p.at[0] - r.X(3)) < 0.1 && Math.abs(p.at[2] - r.Z(11)) < 0.1);
    o.solid("crate", 1.5, 9, 2.2, 1.2, 1.2);
    r.box("deck", 2.5, 15, 6, 21, 0, 1.5, "NIGHT34 A: the north-east deck (1.5 m): height over the tank");
    r.ramp(4.25, 10.5, 15, 2.5, 1.5, "NIGHT34 A: up to the north-east deck (walkers: 4.5 m for 1.5 m)");
    r.grappleNode(-2, 4.5, 14);
    note(r, "i1a: the west lane lx -5 (22 m), a 1.5 m deck with its ramp, a node between the deck and the west ride wall.");
  });
  R33.overhaul(L, "i1b", (r) => {
    r.grappleNode(-5, 5.5, 10);
    r.grappleNode(4.5, 5.5, 16);
    note(r, "i1b: nodes either side of the 2 m central deck (floor <-> deck <-> the side ride walls).");
  });
  // i6 / i6b: round 2's nodes put Span30Pilot's swing in charge (i6 0 / 10 seeds, 242 deaths; i6b 2 / 10, 154). Round 3: back now that the span pilot
  // drives only THE SPAN / THE SPIRE's rooms (Walk29Pilot.Span30Rooms).
  // i6 / i6b: round 2's nodes put Span30Pilot's swing in charge (i6 0 / 10, i6b 2 / 10). Round 3: back, with the span pilot off in these two rooms
  // (Walk29Pilot.Span30Rooms); i1a / i1b keep the span pilot (Walk29Pilot.SwingAlso34: with the autopilot there seed 10 stalled in i6b).
  R33.overhaul(L, "i6", (r) => {
    r.grappleNode(-9.5, 7, 17);
    r.grappleNode(9.5, 7, 5);
    r.grappleNode(0, 7, 11);
    note(r, "i6: a node over each lava pocket into its wall ride, one over the frost between the blocks.");
  });
  R33.overhaul(L, "i6b", (r) => {
    r.grappleNode(-8.5, 7, 6);
    r.grappleNode(8.5, 7, 16);
    r.grappleNode(0, 7, 11);
    note(r, "i6b: a node over each lava pocket into its wall ride, one over the 2 m centre deck.");
  });
}

P.write = (function (base) {
  return function () {
    lines34(this);
    this.plan.id = "n34-iceline";
    this.plan._why = "NIGHT34 builder A (design.n34.lines34 swaps n33-iceline for this file): Tools/night/gen-n34-iceline.cjs = gen-n33-iceline.cjs + the lines34 pass (grapple nodes in i1a / i1b / i6 / i6b, i1a 22 m long with a north-east deck and a clear west lane). " + this.plan._why;
    return base.call(this, OUT);
  };
})(P.write);

require("./gen-n33-iceline.cjs");
