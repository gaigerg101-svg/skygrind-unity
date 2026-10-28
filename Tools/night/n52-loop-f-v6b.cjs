// NIGHT52 the room loop, builder F (round 6): spire v6 from the lead's midpoint review (item 5: "six rings: keep the ones on the circuit
// route, cut the rest; keep >= 2.5 density and the grapple anchors item"), in Design/levels/n52/spire.plan.json:
//   kept (on the loop round the island): the east lane's door-end ring (the pull onto the east ride's line), the lava strip's ring (the
//   rope over the bell-metal beside the ride), the island -> loft gap ring (the north floor), the high stalls' ring (from the west lane)
//   cut (off the loop): the ring over the island's middle (the island has its ramp, its pad and the gap ring) and the ring over the organ
//   loft's east end (the loft has its ramp and two pads onto it)
//   the density kept >= 2.5 by one piece on the loop: THE WEST LANE PIER, floor to roof in the west lane's middle (a corner to circle on the
//   loop, it cuts the high stalls' line down the lane)
// Every other room, door and piece is as before (asserted). Run: node Tools/night/n52-loop-f-v6b.cjs, then bash Tools/night/n52-loop-regen.sh spire v6
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/spire.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const R = "v6";
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const room = plan.rooms.find((r) => r.id === R);
if (!room || room.grappleNodes.length !== 6 || room.f52r6_why) throw new Error("v6 is not round 4's state (six rings; already run?)");

const cut = [[-6.5, 5.4, -86], [-15, 9, -98]];
for (const c of cut) {
  const i = room.grappleNodes.findIndex((n) => same(n, c));
  if (i < 0) throw new Error("ring moved upstream: " + c);
  room.grappleNodes.splice(i, 1);
}
room.grapple = "FOUR rings (the card's BELL ROPES' iron grips; round 6, the lead's midpoint review: the ones on the loop round the island kept, the ring over the island's middle and the one over the organ loft cut): one 3 m over the high stalls (the west lane -> the stalls), one 8 m over the east lane's door end (the pull onto the east ride's line from the door), (NIGHT52 F, D-16) one over the island -> loft gap (the rope the short way up from the north floor) and one over the lava strip (the rope over the bell-metal beside the ride)";
const PIER = { min: [-14.5, 0, -86.5], max: [-13.5, 16, -85.5] };
for (const n of [...(room.nodes || []), ...(room.highNodes || [])])
  if (n[0] > PIER.min[0] - 1 && n[0] < PIER.max[0] + 1 && n[2] > PIER.min[2] - 1 && n[2] < PIER.max[2] + 1) throw new Error("a spawn node by the pier: " + n);
const why = "THE WEST LANE PIER (round 6, the lead's midpoint review: in place of the two rings cut, so the room keeps the contract's density), floor to roof, 1 x 1 m in the middle of the west lane between the high stalls and the island: a corner to circle on the loop round the island (husks come round it at punch range) and it cuts the stalls' line of sight down the lane; 2.5 m to the stalls' face, 3 m to the island";
const ai = plan.boxes.findIndex((b) => b.zone === R && b.min[0] === -10.5 && b.max[0] === -9.5 && b.max[1] === 16);
if (ai < 0) throw new Error("the west bell post moved upstream");
plan.boxes.splice(ai, 0, { zone: R, kind: "hull", min: PIER.min, max: PIER.max, job: "cover", why, note: why });
room.f52r6_why = "NIGHT52 F round 6 (Tools/night/n52-loop-f-v6b.cjs; the lead's midpoint review item 5): the rings on the loop kept (4), the two off it cut, the west lane pier added (density)";

for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== R && !same(plan.rooms[i], before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
if (!same(plan.doors, before.doors)) throw new Error("a door changed");
for (const k of ["boxes", "ramps", "pits", "props", "lights", "pads", "tunnels", "grapple"]) {
  const keep = (b) => b.zone !== R && b.room !== R;
  if (!same((before[k] || []).filter(keep), (plan[k] || []).filter(keep))) throw new Error(k + ": a row outside v6 changed");
}
for (const k of Object.keys(before)) if (!["rooms", "boxes"].includes(k) && !same(before[k], plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-f-v6b: v6 rings 6 -> 4 (the loop's kept), the west lane pier");
