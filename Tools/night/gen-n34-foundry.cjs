// NIGHT34 builder A (NIGHT34.md A1, design.n34.lines34 swaps n33-foundry for n34-foundry): LEVEL 1 THE FOUNDRY with a movement line in every fight
// room. It runs Tools/night/gen-n33-foundry.cjs UNCHANGED (which runs gen-n30-foundry.cjs) with two hooks placed BEFORE it loads:
//   - Level.room: f8 PISTOL gets 22 m of length (20 before): the room for a 3-jump lane;
//   - Level.write: after NIGHT33's overhauls (gen-n33's own write hook), the overhauls34 pass below, the id n34-foundry and the output
//     Design/levels/n34/foundry.plan.json. Builder K's dressing pass (dress33.cjs) and levelgen run after it: bash Tools/night/n34-regen.sh foundry.
// Spacings from `dotnet run --project Tools/SimHarness -c Release -- lines34 measure` (never guessed): a ride from a run 9.60 m (1.2 s at 8 m/s), a
// running jump 6.93 m (a 3-jump lane 20.8 m), the grapple's reach 40 m, a step 1.3 m. The checker: `lines34 n34-foundry`.
// Grapple nodes (the cyan ring, GrappleNodes30): each one hangs 6-7 m up between a floor and the wall or tier it chains to, never over a door.
// The lava rooms (f5 WALL-RIDE, f5b LAVA RUN, f9 LAVA LANE) keep their lava: their line is the wall ride and the grapple across the hazard; no dry
// lane is built over the lava (the lead, round 2): their lane stays SHORT, reported with its length.
const lib = require("./n30-lib.cjs");
const { Level } = lib;
const R33 = require("./n33r-lib.cjs");

const OUT = "Design/levels/n34/foundry.plan.json";
const P = Level.prototype;
const LONGER = { f8: 22 }; // room id -> length (m)

const roomBase = P.room;
P.room = function (s) {
  if (LONGER[s.id]) s = Object.assign({}, s, { l: LONGER[s.id] });
  return roomBase.call(this, s);
};

function lines34(L) {
  const note = (r, s) => { r.row.lines34 = r.row.lines34 ? r.row.lines34 + " " + s : s; };
  // f5 WALL-RIDE ROOM (14 x 24, lava lx -7..3.5, lz 5.5..20): the west wall ride over the lava (24 m wall, a ride 9.6 m) + two nodes over the lava's
  // middle: south floor -> node 1 -> the island -> node 2 -> the north floor (a second way across that is not the floor)
  R33.overhaul(L, "f5", (r) => {
    r.grappleNode(-2, 6.5, 8);
    r.grappleNode(-2, 6.5, 17);
    note(r, "f5: the line is the west wall ride over the lava and two nodes over it (south floor > node > island > node > north floor). Lane SHORT by design (the lava is the lesson).");
  });
  // f5b LAVA RUN (14 x 36, lava lz 5.5..21 wall to wall, the exit deck lz 21..27 at 2 m): two nodes over the run, the second chaining onto the deck
  R33.overhaul(L, "f5b", (r) => {
    r.grappleNode(0, 7, 10);
    r.grappleNode(2, 7, 17.5);
    note(r, "f5b: the line is the wall lanes and two nodes over the lava run onto the 2 m exit deck. Lane SHORT by design.");
  });
  // f7 SLIDE (8 x 18, h 4: the slide tunnel's 1.2 m ceiling fills its middle; Walk29Pilot's f7 script slides it): NIGHT33's shape (round 2: a 22 m f7
  // with a deck stalled the walk pilot 0 / 10) plus a node under the roof over the north floor (into the side walls' 18 m rides). Round 3: the node is
  // back now that Walk29Pilot keeps its own script in a scripted room (Span30Rooms). Lane and floors SHORT by design (a 4 m slide corridor).
  R33.overhaul(L, "f7", (r) => {
    r.grappleNode(0.5, 3.4, 16.5);
    note(r, "f7: the slide tunnel and the 18 m side-wall rides, a node under the roof over the north floor. Lane and floors SHORT by design (a 4 m slide corridor).");
  });
  // f8 PISTOL (now 14 x 22): the east strip lx 3.8..5.2 is the lane (lz 0..22), the east wall a 22 m ride wall, a node over the floor chaining onto
  // the 3.2 m shelf, a node by the east wall over the entry floor (into the ride); the crate that stood in the lane moves west of it
  R33.overhaul(L, "f8", (r, o) => {
    o.drop((p) => p.kind === "crate" && Math.abs(p.at[0] - r.X(3.6)) < 0.1 && Math.abs(p.at[2] - r.Z(9)) < 0.1);
    o.solid("crate", 2.4, 9, 1.4, 1.2, 1.4);
    r.grappleNode(2.5, 6, 12.5);
    r.grappleNode(5.5, 5.5, 5);
    note(r, "f8: lane lx 4.5 (22 m) along the east ride wall, a node into the ride, a node onto the shelf (the ramp is the enemies' way up).");
  });
  // f9 LAVA LANE (16 x 25, lava lz 8..18 wall to wall): a node over the trench's west half (the ferry runs the east half), a node over the gantry.
  // Round 3: back now that Walk29Pilot keeps its f9 ride (round 2 with the span pilot in charge: 9 / 10). Lane SHORT by design (the trench is the lesson).
  R33.overhaul(L, "f9", (r) => {
    r.grappleNode(-3, 6.5, 13);
    r.grappleNode(5, 6.5, 5.5);
    note(r, "f9: the line is the west wall lane, the ferry and a node over the trench; a node over the gantry. Lane SHORT by design (the trench is the lesson).");
  });
  // f9b LAVA BANKS (20 x 22): the bank lanes over the lava, a node over each bank chaining into its wall ride, one over the island
  R33.overhaul(L, "f9b", (r) => {
    r.grappleNode(-8.5, 7, 11.5);
    r.grappleNode(8.5, 7, 11.5);
    r.grappleNode(0, 7, 11.5);
    note(r, "f9b: wall rides over both banks, a node over each bank and one over the island; the floor ring is the lane.");
  });
  // f10 CRUCIBLE (22 x 22): a node over each crucible pocket chaining into its wall lane, one over the crown's ramp onto the crown
  R33.overhaul(L, "f10", (r) => {
    r.grappleNode(-8.5, 7, 6);
    r.grappleNode(8.5, 7, 6);
    r.grappleNode(0, 7.5, 9.5);
    note(r, "f10: wall lanes over the pockets, a node into each, a node onto the crown; decks and crown joined by ramps (the enemies' climb).");
  });
}

P.write = (function (base) {
  return function () {
    lines34(this);
    this.plan.id = "n34-foundry";
    this.plan._why = "NIGHT34 builder A (design.n34.lines34 swaps n33-foundry for this file): Tools/night/gen-n34-foundry.cjs = gen-n33-foundry.cjs + the lines34 pass (grapple nodes in every fight room, f8 22 m long with a clear lane). " + this.plan._why;
    return base.call(this, OUT);
  };
})(P.write);

require("./gen-n33-foundry.cjs");
