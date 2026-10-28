// NIGHT35 builder A (NIGHT35.md A1, design.n35.campaign6 swaps n34-iceline for n35-iceline): LEVEL 2 THE ICE LINE, polished: the level of the Lucio
// wall ride + wall jump (PRINCIPLES 4: one new idea per level; T's teach35 content: L2 the wall ride + wall jump). It runs Tools/night/gen-n34-iceline.cjs
// UNCHANGED (which runs gen-n33 -> gen-n30) with two hooks placed BEFORE it loads:
//   - Level.room: the RIDE HALL i1r (Tools/night/n35a-lib.cjs rideRoom) right before i2 PULSE ROOM, after the two tank rooms: the level's idea taught early,
//     in a room built for it (the teach35 triggers wallride at its door, walljump between the panels);
//   - Level.write: after NIGHT34's lines34 pass, the n35 pass below, the id n35-iceline, Design/levels/n35/iceline.plan.json.
// Then dress33 and levelgen: bash Tools/night/n35-regen.sh iceline. Checks: lines35 n35-iceline, lines34 walk n35-iceline (Harness.N35.Levels.cs).
// D-3 (decided per room by A): NIGHT34's rings in i1a, i1b, i6 and i6b STAY as dim locked rings: the tease of a later verb (teach.md rule 8: tease grapple
// points in levels 2-3 that cannot be reached yet; the grapple is owned from level 4, design.n35.grapple35.fromLevel; M / V dim them while not owned).
const lib = require("./n30-lib.cjs");
const { Level } = lib;
const A35 = require("./n35a-lib.cjs");

const OUT = "Design/levels/n35/iceline.plan.json";
const P = Level.prototype;

A35.insertBefore(P, "i2", (L, base) => {
  const r = A35.rideRoom(L, base, {
    id: "i1r", name: "2.1r RIDE HALL", par: 25,
    waves: [["husk", "husk"], ["swarmer", "swarmer", "husk"]],
    prompt: "RIDE THE PANELS. JUMP BETWEEN.",
  });
  A35.teach(r, "l2.wallride", -9, 0, 9, 5.5);          // the door: the panels and the lava in view
  A35.teach(r, "l2.walljump", -9, 0, 3, 15, -1);     // the door to between the panels: queued after l2.wallride (T's queue, 5 s each), so it shows while riding A
  return r;
});

function n35(L) {
  const R = (id) => { const r = L.rooms.find((q) => q.id === id); if (!r) throw new Error("n35 iceline: no room " + id); return r; };
  for (const id of ["i1a", "i1b", "i6", "i6b"]) A35.dimNodes(R(id), "level 2: a tease of the grapple (level 4)");
}

P.write = (function (base) {
  return function () {
    n35(this);
    this.plan.id = "n35-iceline";
    this.plan._why = "NIGHT35 builder A (design.n35.campaign6 swaps n34-iceline for this file): Tools/night/gen-n35-iceline.cjs = gen-n34-iceline.cjs + the ride hall i1r before i2 (Lucio semi-platforming: two staggered ride panels over lava, a deck, a pillar; the teach35 triggers wallride and walljump) + D-3 (the rings in i1a / i1b / i6 / i6b stay as dim locked teases). " + this.plan._why;
    return base.call(this, OUT);
  };
})(P.write);

require("./gen-n34-iceline.cjs");
