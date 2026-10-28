// NIGHT34 builder A (NIGHT34.md A1, design.n34.lines34 swaps n33-kiln for n34-kiln): LEVEL 3 THE KILN with a movement line in every fight room.
// It runs Tools/night/gen-n33-kiln.cjs UNCHANGED (which runs gen-n30-kiln.cjs) with a Level.write hook placed BEFORE it loads: after NIGHT33's
// overhauls, the lines34 pass below, the id n34-kiln and the output Design/levels/n34/kiln.plan.json. Then dress33 and levelgen:
// bash Tools/night/n34-regen.sh kiln. Spacings from `lines34 measure` (a ride 9.60 m, a 3-jump lane 20.8 m, grapple reach 40 m, a step 1.3 m);
// the checker: `lines34 n34-kiln`. Every grapple node hangs 6-8 m up between a floor and the wall lane or tier it chains to.
const lib = require("./n30-lib.cjs");
const { Level } = lib;
const R33 = require("./n33r-lib.cjs");

const OUT = "Design/levels/n34/kiln.plan.json";
const P = Level.prototype;

function lines34(L) {
  const note = (r, s) => { r.row.lines34 = s; };
  // k1 LOADING BAY (18 x 22): the press is a 6 x 8 x 7.9 m block in the middle (a ride face on each side), the catwalk at 2.4 m, the mezzanine at 1.6 m:
  // a node over the mezzanine's approach and one over the fan side chaining onto the catwalk
  R33.overhaul(L, "k1", (r) => {
    r.grappleNode(-5, 6, 4);
    r.grappleNode(5.5, 6, 13);
    note(r, "k1: the press's faces and the room walls are the rides, a node onto the mezzanine, a node onto the catwalk; the east floor is the lane.");
  });
  // k2 POUR HALL (20 x 24): the west wall lane over the pour channel with a node over the lava into it; a node north of the gantry chaining onto it
  R33.overhaul(L, "k2", (r) => {
    r.grappleNode(-8.5, 7, 8);
    r.grappleNode(3, 7, 18);
    note(r, "k2: the west wall ride over the pour channel and its node, a node onto the 3 m gantry; the centre line under the gantry is the lane.");
  });
  // k2b SPLIT LEVEL (20 x 22): the east wall lane over the lava with a node into it; a node over the floor at the deck's edge (floor <-> the 2 m deck)
  R33.overhaul(L, "k2b", (r) => {
    r.grappleNode(8.5, 6.5, 11);
    r.grappleNode(1, 6.5, 12);
    note(r, "k2b: the east wall ride over the lava and its node, a node between the floor and the 2 m deck; the floor half is the lane.");
  });
  // k4 FLUE (14 x 30, lava lx -7..-1 lz 6..25): the east strip lx 5.2..6.6 becomes the lane (the ramp-side vent moves to the ramp's north foot and the
  // corner crate goes), two nodes over the lava along the ferry's line chaining to the 3.6 m gantry, the landmark plinth lowered to a 1.2 m step
  R33.overhaul(L, "k4", (r, o) => {
    const v = (r.row.devices33 || []).find((d) => d.kind === "vent");
    if (!v) throw new Error("k4: no vent to move");
    v.rect = R33.rectW(r, 2.2, 23.5, 3.7, 25);
    v.refuge = r.pt(5.5, 0, 22);
    v.note = "NIGHT34 A: the ramp-foot vent (moved off the east lane to the ramp's north foot): a tell, then a burst; do not camp the ramp";
    o.drop((p) => p.kind === "crate" && Math.abs(p.at[0] - r.X(6.3)) < 0.1 && Math.abs(p.at[2] - r.Z(0.9)) < 0.1);
    const plinth = r.boxesOut.find((b) => b.zone === "k4" && /landmark plinth/.test(b.note));
    if (!plinth) throw new Error("k4: no landmark plinth");
    plinth.max[1] = 1.2;
    plinth.note += " (NIGHT34 A: 1.4 -> 1.2 m, a step every walker climbs)";
    r.grappleNode(-4, 8, 12);
    r.grappleNode(-4, 8, 20);
    note(r, "k4: the east lane lx 5.9 (26 m), the ferry and two nodes over the lava onto the 3.6 m gantry (its ramp is the enemies' way up).");
  });
}

P.write = (function (base) {
  return function () {
    lines34(this);
    this.plan.id = "n34-kiln";
    this.plan._why = "NIGHT34 builder A (design.n34.lines34 swaps n33-kiln for this file): Tools/night/gen-n34-kiln.cjs = gen-n33-kiln.cjs + the lines34 pass (grapple nodes in every fight room, k4's east lane cleared, its plinth a 1.2 m step). " + this.plan._why;
    return base.call(this, OUT);
  };
})(P.write);

require("./gen-n33-kiln.cjs");
