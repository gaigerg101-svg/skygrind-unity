// NIGHT53 builder K (the lead's review findings on kb3, DECIDE53 D-13): kiln kb3 3.3 CASTING FLOOR, the third cut, in place in
// Design/levels/n52/kiln.plan.json (kb3 as n53-k-kb3b.cjs leaves it).
//   1. GG persona: "the east pair of 3.5 m stubs stand in open floor, neither cover nor a step: cut both" (GG: "dont just randomly plce
//      stuff ... cut fluff"). n53-k-kb3b.cjs cut the north one; the south one (x 4.5..6, z -206.5..-205) goes now.
//   2. P-rank: "the east 4.2 m plateau is the camp spot (the biggest high floor, one ramp, a pad escape, the ladle in sight): cut its south
//      4 m to floor". The free-standing ladle keeps its north 4 m (x 4.5..8.5, z -216..-212): a 4 x 4 m landing on its ramp, both pads
//      landing on it; its hanger on the landing's north-east corner.
//   3. The density re-earned with a verb (2.60 -> 2.44 without it): THE CRADLE STEP, a 2.1 m step against the landing's south face, and
//      the ingot stack (0.9 m) against the step's: the climb onto the ladle by jumps (0.9, 2.1, 4.2: a jump, a jump, a double jump) on the
//      side the ramp is not, so the landing has a second way up for you (the walkers keep the ramp and drop off the step).
// Asserts kb3 is n53-k-kb3b.cjs's; every other room, door and row bit for bit. After gen-n52-kiln.cjs: n53-k-kb3.cjs, n53-k-kb3b.cjs, then
// this script; then levelgen + node Tools/night/gen-n52-kiln.cjs --finish.
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/kiln.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const Z = "kb3";
const TAG = "NIGHT53 K (n53-k-kb3c.cjs)";
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const room = plan.rooms.find((r) => r.id === Z);
if (!room || !room.n53b_why || room.n53c_why) throw new Error("kb3 is not n53-k-kb3b.cjs's (or already run)");
const one = (arr, pred, what) => {
  const hits = arr.filter(pred);
  if (hits.length !== 1) throw new Error(`kb3: ${hits.length} x ${what} (not n53-k-kb3b.cjs's?)`);
  return hits[0];
};
const box = (min, max, what) => one(plan.boxes, (b) => b.zone === Z && same(b.min, min) && same(b.max, max), what);
const note = (b, why) => {
  b.why = `${b.why} (${TAG}: ${why})`;
  if (b.note !== undefined) b.note = b.why;
};

// ---- 1. the south-east stub
plan.boxes.splice(plan.boxes.indexOf(box([4.5, 0, -206.5], [6, 3.5, -205], "the south-east post")), 1);

// ---- 2. the ladle's south 4 m
const ladle = box([4.5, 0, -216], [8.5, 4.2, -208], "the ladle");
ladle.min = [4.5, 0, -216];
ladle.max = [8.5, 4.2, -212];
note(ladle, "its south 4 m cut to the floor (the review, D-13: the biggest high floor with one ramp was the camp): a 4 x 4 m landing on its ramp, both pads land on it, the cradle step and the ingot stack the climb on its south side");
const hanger = box([7.5, 4.2, -213.5], [8.5, 14, -210.5], "the hanger");
hanger.min = [7.5, 4.2, -216];
hanger.max = [8.5, 14, -214.5];
note(hanger, "on the landing's north-east corner (the landing is 4 m deep): the spitter's cover from the gantry and the lane behind");
const ingot = box([6, 0, -208], [8, 0.9, -206.5], "the ingot stack");
ingot.min = [5.5, 0, -210];
ingot.max = [7.5, 0.9, -208.5];
note(ingot, "against THE CRADLE STEP's south face: the first jump of the climb onto the ladle (0.9, 2.1, 4.2)");

// ---- 3. the cradle step
const step = { zone: Z, kind: "deck", min: [5, 0, -212], max: [8, 2.1, -210], job: "line",
  why: "THE CRADLE STEP (NIGHT53 K, the review's cut: the density re-earned with a verb): a 2.1 m step against the ladle landing's south face: jump from the ingot stack (0.9 m), double jump onto the landing (4.2 m): the climb onto the ladle on the side its ramp is not; the walkers drop onto it off the landing (no ramp: not their way up)" };
step.note = step.why;
plan.boxes.splice(plan.boxes.indexOf(ladle) + 1, 0, step);

// ---- the pads land on the landing
const spring = one(plan.pads, (q) => q.zone === Z && same(q.at, [9.5, 0, -201.5]), "the tapping spring");
if (!same(spring.target, [6.5, 4.2, -209.5])) throw new Error("the tapping spring's target is not n53-k-kb3b.cjs's");
spring.target = [6, 4.2, -213.5];
const vent = one(plan.pads, (q) => q.zone === Z && same(q.at, [-7, 0, -203.5]), "the blast vent");
if (!same(vent.target, [6, 4.2, -212])) throw new Error("the blast vent's target is not n53-k-kb3b.cjs's");
vent.target = [5.5, 4.2, -214];
const pool = one(plan.lights, (l) => l.id === "kb3-L5", "the ladle's pool");
pool.at = [6, 4.225, -214];

// ---- the room row
const swapNode = (arr, a, b) => { const i = arr.findIndex((n) => same(n, a)); if (i < 0) throw new Error("node " + JSON.stringify(a) + " is not n53-k-kb3b.cjs's"); arr[i] = b; };
swapNode(room.highNodes, [6.5, 4.2, -212], [6.5, 4.2, -213]);
if (!same(room.proofs[1].points, [[6.5, 4.2, -212], [6.5, 4.2, -215.8], [6.5, 0, -223.8]])) throw new Error("kb3's ladle proof is not n53-k-kb3b.cjs's");
room.proofs[1].points = [[6, 4.2, -213], [6.5, 4.2, -215.8], [6.5, 0, -223.8]];
if (!same(room.frames52.high, [7.5, 4.2, -214.8, 90, -0.04])) throw new Error("kb3's high frame is not n53-k-kb3b.cjs's");
room.frames52.high = [5.5, 4.2, -213, 90, -0.04];
room.frames36.exit = [6, 4.2, -213.5, 0, -0.3];
room.n53c_why = "NIGHT53 K (Tools/night/n53-k-kb3c.cjs; the review, DECIDE53 D-13): the south-east stub cut; the ladle's south 4 m cut (a 4 x 4 m landing); THE CRADLE STEP (2.1 m) and the ingot stack the climb onto it";

// ---- the guards
for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== Z && !same(plan.rooms[i], before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
if (!same(plan.doors, before.doors)) throw new Error("a door changed");
for (const k of ["boxes", "ramps", "pads", "grapple", "pits", "tunnels", "lights", "props"]) {
  const keep = (o) => o.zone !== Z && o.room !== Z;
  if (!same((before[k] || []).filter(keep), (plan[k] || []).filter(keep))) throw new Error(k + ": a row outside kb3 changed");
}
for (const k of Object.keys(before)) if (!["rooms", "boxes", "pads", "lights"].includes(k) && !same(before[k], plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n53-k-kb3c: kb3 the SE stub cut, the ladle a 4 x 4 m landing, the cradle step (2.1 m) + the ingot stack the climb");
