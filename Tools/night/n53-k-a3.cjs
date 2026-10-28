// NIGHT53 builder K (row K, queue item k3; NIGHT53.md section 4; DECIDE52 D-28 / D-30): foundry a3 1.3 RIDE (THE CASTING PIT), its second
// pass, edited IN PLACE in Design/levels/n52/foundry.plan.json (a3 as gen-n52-foundry.cjs's rebuildA3 writes it).
//   1. THE RIDE IS THE WAY ON (D-28: "a3's ride is no shortcut: walking round is ~1 m longer"; I2 1.08): THE POURING PLATFORM, the far
//      floor north of the pit raised to 2.5 m (x -7..2, z -85..-80), level with the mould shelf: the runner lands you on it flat (the
//      ride's end, as a5's skim step and kb2's lip take a ride at 2.5 m); on foot its only ways up are the shelf ramp (then along the
//      shelf) and the crane stair (then off the ladle perch and its step). NIGHT53.md asked +1.5 m: at 1.5 m the lanes' ends stay inside
//      a jump (the nav's 2.2 m, the player's 1.5 m), so walking round stays the short way (DECIDE53, K).
//   2. THE EXIT: d3's 4.5 m opening cannot sit behind a 2.5 m floor: THE POUR GATE, a 4 x 2 m trench at floor height in front of the door
//      (x -2..2, z -85..-83): drop off the platform into it to leave. THE TAPPING BRIDGE (2.5 m, x 2..4 beside the gate) joins the ladle
//      step to the platform, so the crane stair is the second walk on; the east lane ends in a 3 x 5 m pocket under the platform's face.
//   3. D-30 (a 1.5 m slot at the ring's NE turn between the pit and the ladle step): the step's south 1.5 m cut (lz 24..26 now); the NE
//      turn is a 3 m floor with the trench beside it.
//   4. The shelf's back ramp (the far floor up to the shelf) is cut: the platform is the shelf's height. The proofs follow the new walks.
//   5. I4 (the pads clear of the walk lines): the hoist pad stays (the new walk lines clear it by 2.6 m+); the return pad stays (the shelf's
//      walk now goes onto the platform, not past the pad).
// Asserts a3 is u3-n52's (gen-n52-foundry.cjs round 5 + D-30); every other room, door and row bit for bit.
// Re-running gen-n52-foundry.cjs rebuilds a3 from rebuildA3 and wipes this: run this script again after it.
// Run: node Tools/night/n53-k-a3.cjs
//      dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n52/foundry.plan.json Design/levels/n52/foundry.json --walk
//      node Tools/night/gen-n52-foundry.cjs --finish
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/foundry.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const R = "a3";
const TAG = "NIGHT53 K (n53-k-a3.cjs)";
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const Z = (lz) => -59 - lz;
if (plan.id !== "n52-foundry") throw new Error("unexpected plan " + plan.id);
const room = plan.rooms.find((r) => r.id === R);
if (!room || !same(room.rect, [-10, -85, 10, -59]) || !room.card52_why || room.n53_why) throw new Error("a3 is not u3-n52's (already run?)");
const one = (arr, pred, what) => {
  const hits = arr.filter(pred);
  if (hits.length !== 1) throw new Error(`a3: ${hits.length} x ${what} (not u3-n52's?)`);
  return hits[0];
};
const box = (min, max, what) => one(plan.boxes, (b) => b.zone === R && same(b.min, min) && same(b.max, max), what);
const note = (b, why) => {
  b.why = `${b.why} (${TAG}: ${why})`;
  if (b.note !== undefined) b.note = b.why;
};
const piece = (o) => Object.assign({ zone: R }, o, { note: o.why });

// ---- 3. the ladle step's south 1.5 m cut
const step = box([4, 0, Z(26)], [6.5, 2.5, Z(22.5)], "the ladle step");
step.max = [6.5, 2.5, Z(24)];
note(step, "its south 1.5 m cut (lz 24..26; D-30: a 1.5 m slot at the ring's NE turn between it and the pit): the turn is 3 m; still the two-jump way up from the floor (a double jump to 2.5 m, another to the perch) and the platform's east end");

// ---- 4. the back ramp cut (the platform is the shelf's height)
const backRamp = one(plan.ramps, (r) => r.zone === R && same(r.from, [-6, Z(21.5)]) && same(r.to, [-6, Z(26)]), "the shelf's back ramp");
plan.ramps.splice(plan.ramps.indexOf(backRamp), 1);
const shelf = box([-10, 0, Z(26)], [-7, 2.5, Z(8)], "the mould shelf");
note(shelf, "its back ramp cut: its north end runs straight onto THE POURING PLATFORM (the same 2.5 m): the shelf ramp, the shelf, the platform is the west walk onto the far side");
const stair = one(plan.ramps, (r) => r.zone === R && same(r.from, [8.75, Z(10.5)]) && same(r.to, [8.75, Z(22.5)]), "the crane stair");
note(stair, "with the shelf ramp the only walks onto the far side now (the perch, then its step onto THE POURING PLATFORM): walking round the pit is the long way, the runner the short one");

// ---- 1 + 2. the platform and the trench
const platformW = piece({ kind: "deck", min: [-7, 0, Z(26)], max: [-2, 2.5, Z(21)], job: "line",
  why: "THE POURING PLATFORM, west (NIGHT53 K; D-28: the ride was no shortcut): the far floor raised to 2.5 m, level with the mould shelf: the runner's ride lands you on it flat (ride the wall, step off at its end), while walking round the pit ends at its 2.5 m face (the lanes' ends: out of a jump); its walks are the shelf ramp along the shelf and the crane stair over the perch; the husks drop off it into the lanes at you" });
const platformM = piece({ kind: "deck", min: [-2, 0, Z(24)], max: [2, 2.5, Z(21)], job: "line",
  why: "THE POURING PLATFORM, middle (NIGHT53 K): the runner's landing over the pit's north edge (the health is here), in front of THE POUR GATE (the trench to the exit): drop into it to leave; a husk on it drops into the gate and comes round by the east lane" });
const bridge = piece({ kind: "deck", min: [2, 0, Z(26)], max: [4, 2.5, Z(24)], job: "line",
  why: "THE TAPPING BRIDGE (NIGHT53 K): a 2 m deck at 2.5 m from the ladle step west over the pour gate's east side onto THE POURING PLATFORM: the crane stair's walk (the perch, its step, the bridge) onto the far side, the second way on foot; the husks come back over it" });
const runner = box([-2.6, -0.8, Z(22)], [-2, 10, Z(4)], "the runner");
const runnerAt = plan.boxes.indexOf(runner);
plan.boxes.splice(runnerAt + 1, 0, platformW, platformM, bridge);
note(runner, "its ride now lands on THE POURING PLATFORM (2.5 m) over the pit's north edge: the flat way onto the far side, walking round the long one");

// ---- the room row: the exit in the trench, the health on the landing, the nodes, the proofs
if (!same(room.exit, [0, 0, Z(24.5)])) throw new Error("a3's exit is not u3-n52's");
room.exit = [0, 0, Z(25)];
const health = one(plan.props, (q) => q.room === R && q.kind === "healthPickup", "the health");
if (!same(health.at, [-1, 0.9, Z(21.8)])) throw new Error("a3's health is not u3-n52's");
health.at = [-1, 3.4, Z(22)];
health.why += ` (${TAG}: on THE POURING PLATFORM at the runner's landing, 2.5 m up: the ride's reward)`;
const pool = one(plan.lights, (l) => l.id === "a3-L5", "the exit pool");
if (!same(pool.at, [0, 0.025, Z(24.5)])) throw new Error("a3-L5 is not u3-n52's");
pool.at = [0, 0.025, Z(25)];
pool.size = [2.5, 1.6, 0.05];
const lift = (v) => [v[0], 2.5, v[2]];
const F1 = [1.5, 0, Z(23.5)], F2 = [-3.8, 0, Z(24)], H4 = [5.25, 2.5, Z(24)], F5 = [3, 0, Z(24.8)];
const F1n = lift(F1), F2n = lift(F2), H4n = [5.25, 2.5, Z(25)];
const swapNode = (arr, a, b) => { const i = arr.findIndex((n) => same(n, a)); if (i < 0) throw new Error("node " + JSON.stringify(a) + " is not u3-n52's"); arr[i] = b; };
for (const [a, b] of [[F1, F1n], [F2, F2n]]) {
  const i = room.nodes.findIndex((n) => same(n, a));
  if (i < 0) throw new Error("node " + JSON.stringify(a) + " is not u3-n52's");
  room.nodes.splice(i, 1);
  room.highNodes.push(b);
}
swapNode(room.highNodes, H4, H4n);
const F5n = [3.5, 0, Z(22.5)]; // u3-n52's (3, 0, -83.8) is under THE TAPPING BRIDGE now: the pocket at the east lane's end
swapNode(room.nodes, F5, F5n);
swapNode(room.waveNodes43[0], F5, F5n);
swapNode(room.waveNodes43[0], F1, F1n);
swapNode(room.waveNodes43[0], H4, H4n);
swapNode(room.waveNodes43[1], F2, F2n);
room.waveNodes43_why += " NIGHT53 K: the far floor's nodes on THE POURING PLATFORM (2.5 m): its husks drop into the lanes and the gate and come round at you; the step's node on the cut step";
if (room.proofs.length !== 5) throw new Error("a3's proofs are not u3-n52's");
room.proofs = [
  // the east walk onto the far side: the east lane, the crane stair, the perch, its step, the platform, the gate to the exit
  { kind: "walk", points: [[0, 0, -59.5], [0, 0, Z(5)], [6.25, 0, Z(11)], [8.75, 0, Z(10.6)], [8.75, 5, Z(22.4)], [8.5, 5, Z(24.5)], [5.25, 2.5, Z(25)], [3, 2.5, Z(25)], [1, 2.5, Z(23)], [0, 0, Z(25)]] },
  // the west lane to its end under the platform's face (a dead end: the platform is out of a jump)
  { kind: "walk", points: [[0, 0, Z(5)], [-1, 0, Z(3)], [-4.5, 0, Z(3)], [-6, 0, Z(11)], [-6, 0, Z(20.3)]] },
  // the shelf ramp, the shelf, the platform (the west walk onto the far side)
  { kind: "walk", points: [[-4.5, 0, Z(3)], [-6, 0, Z(1)], [-8.5, 0, Z(1)], [-8.5, 0, Z(2.2)], [-8.5, 2.5, Z(8.5)], [-8, 2.5, Z(14)], [-8, 2.5, Z(20)], [-4, 2.5, Z(22.5)]] },
  // the crane stair onto the perch (u3-n52's proof 4)
  { kind: "walk", points: [[6.25, 0, Z(9.5)], [8.75, 0, Z(10.6)], [8.75, 5, Z(22.4)], [8.5, 5, Z(24)]] },
  // the east lane to its end (the old NE turn, 3 m now: the pocket under the platform's face and the bridge)
  { kind: "walk", points: [[6.25, 0, Z(11)], [6.25, 0, Z(20.5)], [4.5, 0, Z(22.5)], [3, 0, Z(23.3)]] },
];
// ---- the retry spawn 4 m further in (try 2; T-119's cause, as kb3's NIGHT52 round 5 spawn): with the far side up on the platform the
// walk pilot fights from the entry floor, and backing off the swarmers it ended in the 4 m mouth between the cheeks (hold52 --dwelltrace
// seeds 1-10: the pilot at z -59.4..-62, a swarmer at 0.1-3 m); THE ROOM CONTRACT's dwell 1.30 -> 2.30 s (limit 1.8). Tried and dropped:
// (-6.5, 0, -61) beside the shelf ramp's foot (2.8-3.0 s: home was 4.9 m from the door). The spawn at (0, 0, -68), 9 m in, 3 m short of
// the pit, the runner 2 m to its left: a step back ends 5 m short of the door's band. The lanes' two nodes 3 m north (z -79), so the
// view cone keeps every node at 12 m or more (spawn31)
if (!same(room.spawn, { at: [0, 0, Z(5)], yawDeg: 0 })) throw new Error("a3's spawn is not u3-n52's");
room.spawn = { at: [0, 0, Z(9)], yawDeg: 0 };
const F3 = [6.25, 0, Z(17)], F4 = [-6, 0, Z(17)], F3n = [6.25, 0, Z(20)], F4n = [-6, 0, Z(20)];
swapNode(room.nodes, F3, F3n);
swapNode(room.nodes, F4, F4n);
swapNode(room.waveNodes43[0], F3, F3n);
swapNode(room.waveNodes43[1], F4, F4n);
room.spawn_why = "NIGHT53 K (n53-k-a3.cjs, try 2): the retry point 9 m in (was 5), 3 m short of the pit with the runner beside it: a death never restarts the fight in the mouth between the cheeks (the walk pilot's dwell, T-119's cause); the lanes' nodes 3 m north with it (spawn31)";
room.verb36 = room.verb36.replace("the ride (hold jump on a wall)", "the ride (hold jump on a wall; NIGHT53 K: the flat way onto the far side's 2.5 m pouring platform, walking round is the long way)");
room.n53_why = "NIGHT53 K (Tools/night/n53-k-a3.cjs; DECIDE52 D-28 / D-30, the second pass): THE POURING PLATFORM (the far floor at 2.5 m: the runner's ride is the flat way on, the shelf ramp and the crane stair the walks), THE POUR GATE (a floor-height trench in front of d3), THE TAPPING BRIDGE (the step onto the platform), the ladle step's south 1.5 m cut, the shelf's back ramp cut";

// ---- the guards
for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== R && !same(plan.rooms[i], before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
if (!same(plan.doors, before.doors)) throw new Error("a door changed");
for (const k of ["boxes", "ramps", "pads", "grapple", "pits", "tunnels", "lights", "props"]) {
  const keep = (o) => o.zone !== R && o.room !== R;
  if (!same((before[k] || []).filter(keep), (plan[k] || []).filter(keep))) throw new Error(k + ": a row outside a3 changed");
}
for (const k of ["pads", "pits", "tunnels", "grapple", "teach35"]) if (!same(before[k], plan[k])) throw new Error(k + " changed (a3's stay)");
for (const k of Object.keys(before)) if (!["rooms", "boxes", "ramps", "lights", "props"].includes(k) && !same(before[k], plan[k])) throw new Error(k + " changed");
for (const k of ["boxes", "ramps"]) for (const o of plan[k]) if (o.zone === R && !(String(o.job || "").trim() && String(o.why || "").trim())) throw new Error(k + ": an a3 row without job / why");
const eol = text.includes("\r\n") ? "\r\n" : "\n";
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1).replace(/\n/g, eol) + eol);
console.log("n53-k-a3: a3 second pass: the pouring platform (2.5 m) + the pour gate, the ladle step's south 1.5 m cut, the shelf's back ramp cut");
