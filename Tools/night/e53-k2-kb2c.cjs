// EXT53 builder K2 (DECIDE53 D-19): kiln kb2c 3.2c THE ASH BED, its second pass, edited IN PLACE in Design/levels/n52/kiln.plan.json.
// What it answers (n53-identity at u3-n53 / port/stage3 da7c9a1e): I1 72 % (a mirror across the door's axis: the slag stack and its ramp
// on the axis, the loft against the east ledge + ramp), I2 1.07 (the cooling wall's 25 m ride ran beside the east lane: you could walk the
// same 25 m), I4 FAIL (the ash vent 0.08 m from walk line 0: the 4 m west lane had no spot 2 m off its line), and kiln's I5 ring (the stack
// in the middle with a walkable band round it). The burrower's lesson stays: the open ash in front of the door, four burrow-safe plates.
//   1. THE SLAG RUN (new, lava): the slag stack pours its run east off its face into an 8.5 m channel to the cooling wall (x 5..9.4,
//      z -182..-173.5: the stack's whole east face and 1.5 m past it, to the lip of the ride's mount plate): the east lane is cut, so the cooling wall's ride is the east side's way north over the run (a wall ride); walkers go
//      round by the west lane. Both ways: it burns you and knocks you up; a blast or a punch puts a swarmer or a stoker in.
//   2. THE SLAG STACK 2 m east (x -1..5), its flue on the run's head (x 3.5..5), its walkers' ramp on its north face at the west end
//      (x -1..2): the stack stands off the door's axis against the run (the mirror and the ring broken), and the west lane is 6 m wide
//      (x -7..-1): the walkers' long way round, the burrower's hunting lane.
//   3. THE ASH VENT stays at [-5, 0, -177.5] (its throw onto the loft unchanged); walk line 0 now runs the wide lane's east side
//      (x -2.5, 2.5 m off the vent): I4 clear.
//   4. The east plate (it stood in the cut lane) moves to the run's north lip at the cooling wall's foot: THE QUENCH STEP, burrow-safe
//      metal where a rider who leaves the wall early lands clear of the slag, a stand to wait out the tell before the ledge ramp.
//   The proofs follow (proof 0 no longer loops: the run cuts the east lane; proof 4 walks the south-east pocket to the run's lip, the
//   ride's mount side); wave 1's east nodes move off the run; the stack's high node onto its new top.
// Asserts kb2c is u3-n53's (gen-n52-kiln.cjs + NIGHT52's fixes) and leaves every other room, door and row bit for bit.
// Re-running gen-n52-kiln.cjs rebuilds kb2c from its function and wipes this: run this script again after it (and after n53-k-kb3*.cjs).
// Run: node Tools/night/e53-k2-kb2c.cjs
//      dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n52/kiln.plan.json Design/levels/n52/kiln.json --walk
//      node Tools/night/gen-n52-kiln.cjs --finish
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/kiln.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const Z = "kb2c";
const TAG = "EXT53 K2 (e53-k2-kb2c.cjs)";
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
if (plan.id !== "n52-kiln") throw new Error("unexpected plan " + plan.id);
const room = plan.rooms.find((r) => r.id === Z);
if (!room || !same(room.rect, [-10, -193.5, 10, -163.5]) || room.e53_why) throw new Error("kb2c is not u3-n53's (already run?)");
const box = (min, max) => {
  const hits = plan.boxes.filter((b) => b.zone === Z && same(b.min, min) && same(b.max, max));
  if (hits.length !== 1) throw new Error(`kb2c: ${hits.length} boxes ${JSON.stringify(min)}..${JSON.stringify(max)} (not u3-n53's?)`);
  return hits[0];
};
const ramp = (from, to) => {
  const hits = plan.ramps.filter((b) => b.zone === Z && same(b.from, from) && same(b.to, to));
  if (hits.length !== 1) throw new Error(`kb2c: ${hits.length} ramps ${JSON.stringify(from)}..${JSON.stringify(to)}`);
  return hits[0];
};
const note = (b, why) => {
  b.why = `${b.why} (${TAG}: ${why})`;
  if (b.note !== undefined) b.note = b.why;
};

// ---- 2. the slag stack 2 m east, its flue on the run's head, its ramp at the stack's west end
const stack = box([-3, 0, -182], [3, 2.5, -175]);
stack.min = [-1, 0, -182];
stack.max = [5, 2.5, -175];
note(stack, "2 m east (x -1..5), off the door's axis: the slag run pours off its east face into the cooling wall's channel, so the stack is the run's source, not a mass to circle (I1 / I5); its west face is the 6 m west lane's east side, the walkers' long way round");
const flue = box([1.5, 2.5, -182], [3, 12, -180.5]);
flue.min = [3.5, 2.5, -182];
flue.max = [5, 12, -180.5];
note(flue, "2 m east with the stack: on the run's head, the chimney the slag pours under; still the pivot on the stack top");
const sramp = ramp([0, -187], [0, -182]);
sramp.from = [0.5, -187];
sramp.to = [0.5, -182];
note(sramp, "0.5 m east, at the moved stack's west end (x -1..2): the swarmers' climb stays in the west lane's sight and the north lane keeps a 3 m way east of it to the ledge ramp");

// ---- 4. the east plate (in the cut lane) -> THE QUENCH STEP on the run's north lip at the cooling wall's foot
const plateE = box([4, 0, -181.5], [7.5, 0.4, -178]);
plateE.min = [7.6, 0, -184.4];
plateE.max = [9.4, 0.4, -182.4];
plateE.why = "THE QUENCH STEP (a METAL PLATE, 0.4 m, on the slag run's north lip at the cooling wall's foot; it stood in the east lane the run now cuts): burrow-safe ground (it cannot surface through metal: A1's rule) where a rider who leaves the wall early lands clear of the slag, and a stand to wait out the tell before the ledge ramp (" + TAG + ")";
plateE.note = plateE.why;
const L3 = plan.lights.find((l) => l.id === "kb2c-L3");
if (!L3 || !same(L3.at, [5.75, 0.425, -179.75])) throw new Error("kb2c-L3 is not u3-n53's");
L3.at = [8.5, 0.425, -183.4];
L3.size = [1.4, 1.4, 0.05];

// ---- 1. THE SLAG RUN
const RUN = [5, -182, 9.4, -173.5];
if (plan.pits.some((q) => q.zone === Z)) throw new Error("kb2c already has a pit");
const run = {
  zone: Z, rect: RUN, floorY: -0.8, lava: true, job: "hazard",
  why: "THE SLAG RUN (" + TAG + "; I2: the cooling wall's 25 m ride ran beside a walkable east lane of the same length): the slag stack's run pours off its east face into an 8.5 m channel of slag (its whole east face and 1.5 m past it, to the lip of the ride's mount plate) to the cooling wall, 0.8 m down: the east side's way north is the cooling wall (a wall ride over the run) or the west lane the long way round; both ways (A-5): it burns you and knocks you up out to the nearest lip, and a blast or a punch off either lip puts a swarmer or a stoker in",
  note: "the slag run",
};
const after = plan.pits.findIndex((q) => q.zone === "kb3");
plan.pits.splice(after < 0 ? plan.pits.length : after, 0, run);
if (room.rims36) throw new Error("kb2c already has rims36");
room.rims36 = {
  rects: [[5, -182.5, 9.4, -182], [5, -173.5, 9.4, -173]],
  why: "EXT53 K2: the slag run's two lips: the walkers' lines past it (the NE strip to the ledge ramp, the SE pocket at the ride's mount); a blast or a punch over the lip puts them in",
};

// ---- the room row: nodes, proofs, notes
const swap = (arr, from, to, what) => {
  const i = arr.findIndex((n) => same(n, from));
  if (i < 0) throw new Error("kb2c: " + what + " " + JSON.stringify(from) + " is not u3-n53's");
  arr[i] = to;
};
// wave 1 (the burrower on the ash): the east lane's two nodes were in / beside the run now
for (const arr of [room.nodes, room.waveNodes43[0]]) {
  swap(arr, [8, 0, -178.5], [7.5, 0, -166.5], "wave-1 node"); // the south-east ash by the ride's mount: 7.8 m from the spawn, outside its first view (spawn31)
  swap(arr, [8, 0, -183], [6, 0, -183.2], "wave-1 node");
}
// wave 2 (the stokers on the tiers): the stack top's node onto the moved stack
for (const arr of [room.highNodes, room.waveNodes43[1]]) swap(arr, [-1, 2.5, -181], [1.5, 2.5, -181.3], "stack-top node"); // at the ramp's head, 12.9 m from the spawn (spawn31: >= 12 in the first view)
if (!same(room.proofs[0].points, [[0, 0, -165.5], [-5.5, 0, -174], [-4, 0, -183], [-3.5, 0, -186], [-1, 0, -188.8], [0.5, 0, -189], [4, 0, -188], [4, 0, -183], [8.4, 0, -182.5], [8.4, 0, -174], [3.8, 0, -174.5], [3.8, 0, -172], [0, 0, -166]])) throw new Error("kb2c's walk proof 0 is not u3-n53's");
// walk proof 0: the walkers' long way round (the run cuts the east lane): the door, the west lane's east side (2.5 m off the vent), the
// north lane past the stack ramp's foot, the 3 m way east of it, the NE strip to the ledge ramp's foot
room.proofs[0].points = [[0, 0, -165.5], [-2.5, 0, -172], [-2.5, 0, -183], [-2.6, 0, -186.3], [-1.6, 0, -188.5], [2.6, 0, -188.5], [3.5, 0, -185.5], [6.2, 0, -183.4]];
if (!same(room.proofs[1].points, [[0, 0, -189], [0, 2.5, -181], [0, 2.5, -177]])) throw new Error("kb2c's walk proof 1 is not u3-n53's");
room.proofs[1].points = [[0.5, 0, -189], [0.5, 2.5, -181], [0.5, 2.5, -177]];
if (!same(room.proofs[3].points, [[7.2, 0, -183.5], [7.2, 2.5, -190.5], [8.5, 2.5, -192]])) throw new Error("kb2c's walk proof 3 is not u3-n53's");
room.proofs[3].points = [[6.5, 0, -183.8], [7.2, 2.5, -190.5], [8.5, 2.5, -192]];
// walk proof 4: the south-east pocket from the door to the run's south lip (the ride's mount side is walkable)
room.proofs.push({ kind: "walk", points: [[0, 0, -166], [4, 0, -170], [4.5, 0, -173.2]] });
room.verb36 = room.verb36.replace("the cooling wall's ride)", "the cooling wall's ride over the slag run: the east side's way north)");
if (!room.verb36.includes("over the slag run")) throw new Error("kb2c's verb36 is not u3-n53's");
room.e53_why = "EXT53 K2 (Tools/night/e53-k2-kb2c.cjs; DECIDE53 D-19): THE SLAG RUN cuts the east lane (the cooling wall's ride is the east side's way north), the slag stack 2 m east as its source (the mirror and the ring broken), walk line 0 on the 6 m west lane's east side (the vent clear), the east plate to the run's north lip (THE QUENCH STEP)";

// ---- the guards: every other room, door and row bit for bit
for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== Z && !same(plan.rooms[i], before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
if (!same(plan.doors, before.doors)) throw new Error("a door changed");
for (const k of ["boxes", "ramps", "pads", "grapple", "pits", "tunnels", "lights", "props"]) {
  const keep = (o) => o.zone !== Z && o.room !== Z;
  if (!same((before[k] || []).filter(keep), (plan[k] || []).filter(keep))) throw new Error(k + ": a row outside kb2c changed");
}
if (!same(before.pads, plan.pads)) throw new Error("pads changed (the vent stays)");
for (const k of Object.keys(before)) if (!["rooms", "boxes", "ramps", "pits", "lights"].includes(k) && !same(before[k], plan[k])) throw new Error(k + " changed");
for (const k of ["boxes", "ramps", "pits"]) for (const o of plan[k]) if (o.zone === Z && !(String(o.job || "").trim() && String(o.why || "").trim())) throw new Error(k + ": a kb2c row without job / why");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("e53-k2-kb2c: kb2c THE ASH BED second pass: THE SLAG RUN " + JSON.stringify(RUN) + ", the stack 2 m east, walk line 0 on the west lane's east side, THE QUENCH STEP");
