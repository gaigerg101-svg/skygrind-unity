// NIGHT52 the room loop, builder F (round 4): spire v2 from the P-rank review (the lead's items 8-9), in Design/levels/n52/spire.plan.json:
//   (8) the crossing round the beam's north end was 1 m between the two apse posts and the beam's foot: the posts trimmed to 0.5 m deep
//       (door jambs)
//   (9) the bell (5 m) on the door's axis hid every wave-1 node (w1 in view 0/5) and was the camp (the ring and the health on it): its south
//       3 m cut (lz 13..19 -> 16..19), the health, the aisle hoist's landing and the lip props onto what is left (the bell ring stays: see below)
// Every other room, door and piece is as before (asserted). Run: node Tools/night/n52-loop-f-v2c.cjs, then bash Tools/night/n52-loop-regen.sh spire v2
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
if (!room || !room.f52b_why || room.f52c_why) throw new Error("v2 is not round 4's (n52-loop-f-v2b.cjs) state (already run?)");
const X = (lx) => lx - 10, Z = (lz) => 83 - lz;
const P = (lx, y, lz) => [X(lx), y, Z(lz)];
const find = (pred, what) => { const b = plan.boxes.find((q) => q.zone === R && pred(q)); if (!b) throw new Error("not found: " + what); return b; };

// (8) the apse posts as door jambs
for (const lx0 of [-4, 2]) {
  const p = find((b) => same(b.min, [X(lx0), 0, Z(32)]) && same(b.max, [X(lx0 + 2), 14, Z(30)]), "an apse post at lx " + lx0);
  p.max = [p.max[0], p.max[1], Z(31.5)];
  p.why += " (round 4, P-rank review: 0.5 m deep, door jambs: at 2 m the crossing round the beam's north end was 1 m wide)";
  p.note = p.why;
}
// (9) the bell's south 3 m cut
const bell = find((b) => same(b.min, [X(-3), 0, Z(19)]) && same(b.max, [X(3), 5, Z(13)]), "the bell");
bell.max = [bell.max[0], 5, Z(16)];
bell.why = bell.why.replace("THE BELL (deck 5 m,", "THE BELL (deck 5 m, 6 x 3 m: round 4, P-rank review: its south 3 m cut, so wave 1 shows past it from the door and it is less of a camp;");
bell.note = bell.why;
const hp = plan.props.find((q) => q.id === "v2-P1");
hp.at = P(1.5, 5.9, 17.5);
for (const [id, lz] of [["v2-bell-lip-s", 15.97], ["v2-bell-crown", 15.97]]) {
  const q = plan.props.find((x) => x.id === id);
  q.at = [q.at[0], q.at[1], Z(lz)];
}
const aisleHoist = plan.pads.find((p) => p.zone === R && same(p.target, P(-1.5, 5, 15)));
if (!aisleHoist) throw new Error("the aisle hoist moved upstream");
aisleHoist.target = P(-1.5, 5, 17.5);
// the bell ring stays where it hung (8.5 m up, now 2.5 m south of the bell's cut face): the floor -> the crown is a swing up and over onto what
// is left. (Moved over the bell, lz 16.5, it was the room's only ring off the east aisle ring's line, so the walk bot's span pilot read v2 as a
// grapple crossing and pressed at the bell's face after the clear: chained seeds 1 and 12 were put at the next room.)
if (!room.grappleNodes.some((n) => same(n, P(0, 8.5, 13.5)))) throw new Error("the bell ring moved upstream");
room.grapple = room.grapple.replace("the BELL RING 3.5 m over the bell's south edge (floor -> the crown)", "the BELL RING 8.5 m up, 2.5 m south of the bell's cut face (round 4) (floor -> the crown: a swing up and over onto it)");
if (!room.grapple.includes("round 4")) throw new Error("the grapple text moved upstream");
// the room's spawn 2.5 m in (lz 5.5 -> 8): with the bell's south 3 m gone the waves reach the floor in front of the door sooner and the walk
// pilot (it holds the spawn and kites back from it) backed into the entry mouth (isolated dwell 1.0 -> 3.7 s); lz 8 keeps wave 1's nearest
// nodes (lz 19.5, 5.5 m off the axis) 12.75 m out (spawn31: >= 12 m inside the view cone)
if (!same(room.spawn.at, P(0, 0, 5.5))) throw new Error("v2's spawn moved upstream");
room.spawn.at = P(0, 0, 8);
room.proofs = room.proofs.map((pr) => ({ ...pr, points: pr.points.map((q) => (same(q, P(1.5, 5, 14.5)) ? P(1.5, 5, 17.2) : q)) }));
room.f52c_why = "NIGHT52 F round 4 (Tools/night/n52-loop-f-v2c.cjs; the P-rank review): the apse posts 0.5 m deep, the bell's south 3 m cut (the health and the aisle hoist's landing on what is left; the bell ring a swing onto it)";

for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== R && !same(plan.rooms[i], before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
if (!same(plan.doors, before.doors)) throw new Error("a door changed");
for (const k of ["boxes", "ramps", "pits", "props", "lights", "pads", "tunnels", "grapple"]) {
  const keep = (b) => b.zone !== R && b.room !== R;
  if (!same((before[k] || []).filter(keep), (plan[k] || []).filter(keep))) throw new Error(k + ": a row outside v2 changed");
}
for (const k of Object.keys(before)) if (!["rooms", "boxes", "props", "pads"].includes(k) && !same(before[k], plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-f-v2c: v2 P-rank fixes (apse posts 0.5 m, the bell's south 3 m cut)");
