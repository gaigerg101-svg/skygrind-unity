// NIGHT52 the room loop, builder F (round 6): spire v2 from the lead's midpoint review (item 2), in Design/levels/n52/spire.plan.json:
//   the health off the bell (the 5 m block: the block + its beam ramp + the health made it the camp) onto the floor under the bell ring
//   (8.5 m up, 2.5 m south of the bell's cut face): taking it puts you on the open floor in front of the bell, in every lane's fire
// Every other room, door and piece is as before (asserted). Run: node Tools/night/n52-loop-f-v2d.cjs, then bash Tools/night/n52-loop-regen.sh spire v2
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/spire.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const R = "v2";
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const room = plan.rooms.find((r) => r.id === R);
if (!room || !room.f52c_why || room.f52d_why) throw new Error("v2 is not round 4's (n52-loop-f-v2c.cjs) state (already run?)");
const X = (lx) => lx - 10, Z = (lz) => 83 - lz;
const P = (lx, y, lz) => [X(lx), y, Z(lz)];

const hp = plan.props.find((q) => q.id === "v2-P1");
if (!same(hp.at, P(1.5, 5.9, 17.5))) throw new Error("v2's health moved upstream");
if (!room.grappleNodes.some((n) => same(n, P(0, 8.5, 13.5)))) throw new Error("the bell ring moved upstream");
hp.at = P(0, 0.9, 13.5);
hp.why = "the riskiest spot pays (A20; round 6, the lead's midpoint review: off the bell, where the block, its beam ramp and the health made the camp): on the floor under the bell ring, in front of the bell's cut face on the door's line, where wave 1 comes round both sides of the bell and the aisles' spitters see it";
room.f52d_why = "NIGHT52 F round 6 (Tools/night/n52-loop-f-v2d.cjs; the lead's midpoint review item 2): the health off the bell onto the floor under the bell ring";

for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== R && !same(plan.rooms[i], before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
if (!same(plan.doors, before.doors)) throw new Error("a door changed");
for (const k of ["boxes", "ramps", "pits", "props", "lights", "pads", "tunnels", "grapple"]) {
  const keep = (b) => b.zone !== R && b.room !== R;
  if (!same((before[k] || []).filter(keep), (plan[k] || []).filter(keep))) throw new Error(k + ": a row outside v2 changed");
}
for (const k of Object.keys(before)) if (!["rooms", "props"].includes(k) && !same(before[k], plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-f-v2d: v2 health under the bell ring");
