// NIGHT52 the room loop, builder F (round 6): spire v5 from the lead's midpoint review (item 1), in Design/levels/n52/spire.plan.json:
//   the newel (a 4 x 5 m floor-to-roof block on the door's line) thinned to a 1 m north-south wall standing on the rope gallery's west edge
//   (lx 2..3, lz 9..14): the door opens north (its 4 m column lx -2..2 is clear to the far wall); the wall's west face (the floor's) and east
//   face (the gallery's) stay cyan ride faces and a grapple surface
//   the health off the ringing chamber (the 5 m deck: a camp with health on it) onto the floor at the foot of the ringers' stair
// Every other room, door and piece is as before (asserted). Run: node Tools/night/n52-loop-f-v5d.cjs, then bash Tools/night/n52-loop-regen.sh spire v5
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/spire.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const R = "v5";
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const room = plan.rooms.find((r) => r.id === R);
if (!room || !room.f52c_why || room.f52d_why) throw new Error("v5 is not round 4's (n52-loop-f-v5c.cjs) state (already run?)");
const X = (lx) => lx - 10, Z = (lz) => -39 - lz;
const P = (lx, y, lz) => [X(lx), y, Z(lz)];

const newel = plan.boxes.find((b) => b.zone === R && same(b.min, [X(-2), 0, Z(14)]) && same(b.max, [X(2), 12, Z(9)]));
if (!newel) throw new Error("the newel moved upstream");
newel.min = [X(2), 0, Z(14)];
newel.max = [X(3), 12, Z(9)];
newel.why = "THE NEWEL (card: Spatial law; contract item 4; round 4, GG review: 'circling the newel is a ride'; round 6, the lead's midpoint review: thinned from a 4 x 5 m block on the door's line to a 1 m wall on the rope gallery's west edge, so the door opens north down the hall), floor to roof, 5 m long: its west face from the floor and its east face from the gallery are cyan ride faces (a wall jump off its ends onto the gallery or the floor) and a grapple surface; it cuts the chamber's line of sight onto the gallery's west half";
newel.note = newel.why;
for (const [id, lx, y] of [["v5-NW", 1.95, 1.2], ["v5-NE", 3.05, 3.7]]) {
  const l = plan.lights.find((q) => q.id === id);
  if (!l) throw new Error("light " + id + " missing");
  l.at = P(lx, y, 11.5);
}
const hp = plan.props.find((q) => q.id === "v5-P1");
if (!same(hp.at, P(-7, 5.9, 21.5))) throw new Error("v5's health moved upstream");
hp.at = P(-7.65, 0.9, 7.8);
hp.why = "the riskiest spot pays (A20; round 6, the lead's midpoint review: off the ringing chamber, where it made the 5 m deck a camp): on the floor at the ringers' stair's foot, under the chamber's spitter and in the file coming down the stair";
room.frames52._why = room.frames52._why.replace("in the mouth looking at the newel", "in the mouth looking down the hall past the thinned newel");
room.f52d_why = "NIGHT52 F round 6 (Tools/night/n52-loop-f-v5d.cjs; the lead's midpoint review item 1): the newel a 1 m wall on the gallery's west edge (the door opens north), the health on the floor at the stair's foot";

for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== R && !same(plan.rooms[i], before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
if (!same(plan.doors, before.doors)) throw new Error("a door changed");
for (const k of ["boxes", "ramps", "pits", "props", "lights", "pads", "tunnels", "grapple"]) {
  const keep = (b) => b.zone !== R && b.room !== R;
  if (!same((before[k] || []).filter(keep), (plan[k] || []).filter(keep))) throw new Error(k + ": a row outside v5 changed");
}
for (const k of Object.keys(before)) if (!["rooms", "boxes", "props", "lights"].includes(k) && !same(before[k], plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-f-v5d: v5 newel thinned onto the gallery's edge, the health at the stair's foot");
