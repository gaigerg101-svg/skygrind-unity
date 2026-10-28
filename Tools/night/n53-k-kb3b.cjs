// NIGHT53 builder K (row K; the lead's ask at 08:15: "when you re-cut a room, break its symmetry about the door axis"): kiln kb3 3.3
// CASTING FLOOR, the second pass's second cut, in place in Design/levels/n52/kiln.plan.json (kb3 as n53-k-kb3.cjs leaves it).
// I1 (n53-identity.py: the solid footprint reflected across the door's axis) read 73 %: the west gantry and its two ramps run the whole
// west wall, the ladle and its north ramp hugged the east wall in the same z band, the mould's west wall had the east lip as its twin.
//   1. THE LADLE STANDS FREE: the ladle (4.2 m) moves off the east wall onto the east bank's middle (x 4.5..8.5, was 6..11): a 2.5 m lane
//      runs behind it along the wall, so its spitters can be flanked (you come round the stand, not only up it); its ramp rises from the
//      north crossing in front of it (x 5..8), its hanger on its east edge, the ingot stack (the small boost) against its south face.
//   2. Cut: the mould's 1 m east lip (the west wall's twin; the bank sees across the mould without it), the ladle's pour lip (the ladle's
//      own west edge now stands over the east lane), the north-east post (the ramp's foot stands there; one pillar fewer).
// Asserts kb3 is n53-k-kb3.cjs's; every other room, door and row bit for bit. After gen-n52-kiln.cjs: n53-k-kb3.cjs, then this script.
// Run: node Tools/night/n53-k-kb3b.cjs, then levelgen + node Tools/night/gen-n52-kiln.cjs --finish (as n53-k-kb3.cjs)
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/kiln.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const Z = "kb3";
const TAG = "NIGHT53 K (n53-k-kb3b.cjs)";
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const room = plan.rooms.find((r) => r.id === Z);
if (!room || !room.n53_why || room.n53b_why) throw new Error("kb3 is not n53-k-kb3.cjs's (or already run)");
const one = (arr, pred, what) => {
  const hits = arr.filter(pred);
  if (hits.length !== 1) throw new Error(`kb3: ${hits.length} x ${what} (not n53-k-kb3.cjs's?)`);
  return hits[0];
};
const box = (min, max, what) => one(plan.boxes, (b) => b.zone === Z && same(b.min, min) && same(b.max, max), what);
const note = (b, why) => {
  b.why = `${b.why} (${TAG}: ${why})`;
  if (b.note !== undefined) b.note = b.why;
};
const cut = (b) => plan.boxes.splice(plan.boxes.indexOf(b), 1);

// ---- 2. the cuts
cut(box([1.4, -0.8, -220.5], [2, 1, -203.5], "the east lip"));
cut(box([4.5, 3.2, -214], [6, 4.2, -211], "the pour lip"));
cut(box([4.5, 0, -220.5], [6, 3.5, -219], "the north-east post"));

// ---- 1. the ladle off the wall
const ladle = box([6, 0, -216], [11, 4.2, -208], "the ladle");
ladle.min = [4.5, 0, -216];
ladle.max = [8.5, 4.2, -208];
note(ladle, "it stands free on the east bank (x 4.5..8.5, was against the east wall): a 2.5 m lane runs behind it along the wall, so its spitters are flanked round the stand as well as climbed; its west edge stands over the east lane (its pour lip cut); its ramp rises from the north crossing in front of it; I1: the room's mirror across the door axis 73 % -> under 60");
const hanger = box([10, 4.2, -213.5], [11, 14, -210.5], "the hanger");
hanger.min = [7.5, 4.2, -213.5];
hanger.max = [8.5, 14, -210.5];
note(hanger, "on the free-standing ladle's east edge: the spitter on the ladle's west side has its cover between you and the lane behind");
const ingot = box([4.5, 0, -210.5], [6, 0.9, -208.5], "the ingot stack");
ingot.min = [6, 0, -208];
ingot.max = [8, 0.9, -206.5];
note(ingot, "against the ladle's south face now (the ladle moved over its old spot): the small boost from the apron side");
const lr = one(plan.ramps, (r) => r.zone === Z && same(r.from, [9.5, -223.5]) && same(r.to, [9.5, -216]), "the ladle's north ramp");
lr.from = [6.5, -223.5];
lr.to = [6.5, -216];
lr.why += ` (${TAG}: 3 m west with the ladle: x 5..8, the lane behind the ladle runs past its east side to the north crossing)`;
lr.note = lr.why;
const spring = one(plan.pads, (q) => q.zone === Z && same(q.at, [9.5, 0, -201.5]), "the tapping spring");
if (!same(spring.target, [8.5, 4.2, -209.8])) throw new Error("the tapping spring's target is not n53-k-kb3.cjs's");
spring.target = [6.5, 4.2, -209.5];
const vent = one(plan.pads, (q) => q.zone === Z && same(q.at, [-7, 0, -203.5]), "the blast vent");
if (!same(vent.target, [8.5, 4.2, -212])) throw new Error("the blast vent's target is not n53-k-kb3.cjs's");
vent.target = [6, 4.2, -212];
const pool = one(plan.lights, (l) => l.id === "kb3-L5", "the ladle's pool");
if (!same(pool.at, [8.5, 4.225, -212])) throw new Error("kb3-L5 is not n53-k-kb3.cjs's");
pool.at = [6.5, 4.225, -212];

// ---- the room row
const swapNode = (arr, a, b) => { const i = arr.findIndex((n) => same(n, a)); if (i < 0) throw new Error("node " + JSON.stringify(a) + " is not n53-k-kb3.cjs's"); arr[i] = b; };
swapNode(room.nodes, [6.5, 0, -219], [9.5, 0, -219]); // was on the moved ramp: the lane behind the ladle
swapNode(room.highNodes, [8.5, 4.2, -212], [6.5, 4.2, -212]);
swapNode(room.highNodes, [5, 4.2, -213.5], [5.5, 4.2, -214.5]); // the cut pour lip's node: the ladle's west edge
if (!same(room.proofs[1].points, [[8.5, 4.2, -212], [9.5, 4.2, -216.5], [9.5, 0, -224.4]])) throw new Error("kb3's ladle proof is not n53-k-kb3.cjs's");
room.proofs[1].points = [[6.5, 4.2, -212], [6.5, 4.2, -215.8], [6.5, 0, -223.8]];
swapNode(room.nodes, [4, 0, -215], [3.2, 0, -215]); // the east lane is 2.5 m now (the ladle at x 4.5): the node on its middle
room.proofs.push({ kind: "walk", points: [[2.8, 0, -207], [3, 0, -204], [9.5, 0, -204], [9.75, 0, -212], [9.75, 0, -218], [3, 0, -222.5]] }); // the lane behind the ladle (round the south post and the ingot stack)
if (!same(room.frames52.high, [9, 4.2, -214.5, 90, -0.04])) throw new Error("kb3's high frame is not n53-k-kb3.cjs's");
room.frames52.high = [7.5, 4.2, -214.8, 90, -0.04];
if (!same(room.frames36.exit, [8.5, 4.2, -214, 0, -0.3])) throw new Error("kb3's frames36 exit is not n53-k-kb3.cjs's");
room.frames36.exit = [6.5, 4.2, -214.5, 0, -0.3];
room.n53b_why = "NIGHT53 K (Tools/night/n53-k-kb3b.cjs; I1, the lead's 08:15 ask): the ladle off the east wall (a lane behind it), its ramp, hanger and ingot stack with it; the east lip, the pour lip and the north-east post cut";

// ---- the guards
for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== Z && !same(plan.rooms[i], before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
if (!same(plan.doors, before.doors)) throw new Error("a door changed");
for (const k of ["boxes", "ramps", "pads", "grapple", "pits", "tunnels", "lights", "props"]) {
  const keep = (o) => o.zone !== Z && o.room !== Z;
  if (!same((before[k] || []).filter(keep), (plan[k] || []).filter(keep))) throw new Error(k + ": a row outside kb3 changed");
}
for (const k of Object.keys(before)) if (!["rooms", "boxes", "ramps", "pads", "lights"].includes(k) && !same(before[k], plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n53-k-kb3b: kb3 the ladle free-standing (x 4.5..8.5), its ramp / hanger / ingot stack with it; the east lip, the pour lip, the NE post cut");
