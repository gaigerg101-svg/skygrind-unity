// NIGHT35 builder A (NIGHT35.md A1, design.n35.campaign6 swaps n34-kiln for n35-kiln): LEVEL 3 THE KILN, polished: the level of THE THUNDERCLAP. It runs
// Tools/night/gen-n34-kiln.cjs UNCHANGED (which runs gen-n33 -> gen-n30) with two hooks placed BEFORE it loads:
//   - Level.room: the RIDE HALL k2r (Tools/night/n35a-lib.cjs rideRoom) right before k3 REST, after k2b SPLIT LEVEL: the wall ride under the shotgun's
//     pressure (husks and spitters on the lane and the north floor);
//   - Level.write: after NIGHT34's lines34 pass, the n35 pass below, the id n35-kiln, Design/levels/n35/kiln.plan.json.
// Then dress33 and levelgen: bash Tools/night/n35-regen.sh kiln. Checks: lines35 n35-kiln, lines34 walk n35-kiln (Harness.N35.Levels.cs).
// D-3 (decided per room by A): NIGHT34's rings in k1, k2, k2b and k4 STAY as dim locked rings (teach.md rule 8: the tease of level 4's grapple).
// teach35: l3.charge at k2's door (the THUNDERCLAP's charge; its pickup line is T's weapon line w.shotgun, shown on the first weaponEarned).
const lib = require("./n30-lib.cjs");
const { Level } = lib;
const A35 = require("./n35a-lib.cjs");

const OUT = "Design/levels/n35/kiln.plan.json";
const P = Level.prototype;

A35.insertBefore(P, "k3", (L, base) => A35.rideRoom(L, base, {
  id: "k2r", name: "3.2r RIDE HALL", par: 25,
  waves: [["husk", "husk", "spitter"], ["husk", "husk", "spitter"]],
  prompt: "RIDE OVER. SHOTGUN THEM CLOSE.",
}));

function n35(L) {
  const R = (id) => { const r = L.rooms.find((q) => q.id === id); if (!r) throw new Error("n35 kiln: no room " + id); return r; };
  for (const id of ["k1", "k2", "k2b", "k4"]) A35.dimNodes(R(id), "level 3: a tease of the grapple (level 4)");
  { const r = R("k2"); A35.teach(r, "l3.charge", -10, 0, 10, 4.5); } // the room after the pickup room k1 (where T's w.shotgun shows): the charge at k2's door
}

P.write = (function (base) {
  return function () {
    n35(this);
    this.plan.id = "n35-kiln";
    this.plan._why = "NIGHT35 builder A (design.n35.campaign6 swaps n34-kiln for this file): Tools/night/gen-n35-kiln.cjs = gen-n34-kiln.cjs + the ride hall k2r before k3 (Lucio semi-platforming: two staggered ride panels over lava, a deck, a pillar) + D-3 (the rings in k1 / k2 / k2b / k4 stay as dim locked teases) + the teach35 trigger l3.charge at k2's door. " + this.plan._why;
    return base.call(this, OUT);
  };
})(P.write);

require("./gen-n34-kiln.cjs");
