// NIGHT52 the room loop, fix 2 (the lead's; DECIDE52 D-15): the pockets the isolated per-room walk found (Tools/night/n52-room-flow.sh:
// n46flow --rooms <id>). The walk pilot was capped (240 s) in g7 on 3 of 20 seeds, stuck at (11.4, 0, -187.2): the 1 x 3.5 m slot between the east
// coal bunker (x 7..11), the room's east wall (x 12), the door wall and the east sluice wall's end (z -187.5); the west side has its mirror. In
// g8 on 1 of 20 seeds at (10.7, 0, -243.4): the 0.6 m slot between the signal box ramp (x 7.4..10.4) and the east wall past the lane wall's end
// (z -243 to the signal box at -246); the west lane has its mirror. Fixes: the bunkers run to the side walls (x 7..12 / -12..-7, flush with
// the sluice walls' ends); both lane walls run on to the decks (z -223 -> -246). Asserts fix 1's state; every other room bit for bit.
// Run: node Tools/night/n52-loop-fix2.cjs, then bash Tools/night/n52-loop-regen.sh iceline g7 g8
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/iceline.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const near = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const one = (pred, what) => {
  const hits = plan.boxes.filter(pred);
  if (hits.length !== 1) throw new Error(what + ": found " + hits.length + " (already run?)");
  return hits[0];
};
const wB = one((b) => b.zone === "g7" && near(b.min, [-11, 0, -188]), "g7 west bunker");
wB.min = [-12, 0, -188];
const eB = one((b) => b.zone === "g7" && near(b.max, [11, 2, -184]), "g7 east bunker");
eB.max = [12, 2, -184];
for (const b of [wB, eB]) { b.why += " (NIGHT52 fix 2: run to the side wall, flush with the sluice wall's end: no pocket behind it)"; b.note = b.why; }
const wL = one((b) => b.zone === "g8" && near(b.min, [-11, 0, -243]), "g8 west lane wall");
wL.min = [-11, 0, -246];
wL.ride36 = { face: "E", planeX: -10.4, lz: [3, 26] };
const eL = one((b) => b.zone === "g8" && near(b.max, [11, 5, -223]) && b.ride, "g8 east lane wall");
eL.min = [10.4, 0, -246];
eL.ride36 = { face: "W", planeX: 10.4, lz: [3, 26] };
for (const b of [wL, eL]) { b.why = b.why.replace("20 m x 5 m", "23 m x 5 m, run on to the deck (NIGHT52 fix 2: no slot beside the ramp)"); b.note = b.why; }
for (const z of ["g7", "g8"]) plan.rooms.find((r) => r.id === z).loop52_why += " Fix 2 (n52-loop-fix2.cjs): the pockets the isolated walk found closed.";
for (let i = 0; i < plan.rooms.length; i++) if (!["g7", "g8"].includes(plan.rooms[i].id) && JSON.stringify(plan.rooms[i]) !== JSON.stringify(before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
for (const k of ["ramps", "pits", "props", "lights", "pads"]) if (JSON.stringify(before[k]) !== JSON.stringify(plan[k])) throw new Error(k + " changed");
const others = (arr) => arr.filter((b) => !["g7", "g8"].includes(b.zone));
if (JSON.stringify(others(before.boxes)) !== JSON.stringify(others(plan.boxes))) throw new Error("a box outside g7 / g8 changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-fix2: g7 bunkers to the walls, g8 lane walls to the decks");
