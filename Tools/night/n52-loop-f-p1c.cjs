// NIGHT52 the room loop, builder F (round 10, a freeze fix): verbs37 row 9 (wall ride) on n52-span picked an 11 m wall in p1 (VerbsWall39:
// the longest straight wall with a floored, clear lane): the chain wall runs on to 17 m (lz 3..20) and the loading terrace starts 4 m later
// (lz 20, its ramp lz 10..20), so the chain wall's floor-side lane is 17 m. Edits Design/levels/n52/span.plan.json in place (p1 as round 9
// left it). Run: node Tools/night/n52-loop-f-p1c.cjs, then bash Tools/night/n52-loop-regen.sh span p1
const fs = require("fs");
const path = require("path");
const PLAN = path.join(__dirname, "../..", "Design/levels/n52/span.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const Z = (lz) => 123.5 - lz;
const chain = plan.boxes.find((b) => b.zone === "p1" && same(b.min, [-12, 0, Z(16)]) && same(b.max, [-11.4, 12, Z(3)]));
const terrace = plan.boxes.find((b) => b.zone === "p1" && same(b.min, [-12, 0, Z(32)]) && same(b.max, [-6, 4, Z(16)]));
const ramp = plan.ramps.find((r) => r.zone === "p1" && same(r.from, [-7.5, Z(6)]) && same(r.to, [-7.5, Z(16)]));
const strip = plan.lights.find((l) => l.id === "p1-RW");
if (!chain || !terrace || !ramp || !strip) throw new Error("p1 is not round 9's");
chain.min = [-12, 0, Z(20)];
chain.ride36 = { face: "E", planeX: -11.4, lz: [3, 20] };
chain.why = chain.why.replace("to the terrace's south face (13 m)", "to the terrace's south face (17 m: round 10, verbs37 row 9's wall: its floor-side lane is the room's 17 m straight ride)");
chain.note = chain.why;
terrace.max = [-6, 4, Z(20)];
terrace.why = terrace.why.replace("16 m along the west wall", "12 m along the west wall (round 10: 4 m shorter, the chain wall runs on)");
terrace.note = terrace.why;
ramp.from = [-7.5, Z(10)];
ramp.to = [-7.5, Z(20)];
strip.at = [-11.35, 1.2, Z(11.5)];
strip.size = [17, 0.2, 0.1];
const land = plan.lights.find((l) => l.id === "p1-land");
land.at = [-9, 4.025, Z(22.5)]; // the landing pool wholly on the shorter terrace
const room = plan.rooms.find((r) => r.id === "p1");
const pr = room.proofs[2];
if (!same(pr.points[1], [-7.5, 0, Z(5.5)])) throw new Error("p1's ramp proof moved");
pr.points = [[-4, 0, Z(4)], [-7.5, 0, Z(9.5)], [-7.5, 4, Z(20.5)], [-9, 4, Z(25)]];
room.f52c_why = "NIGHT52 F round 10 (Tools/night/n52-loop-f-p1c.cjs): the chain wall 17 m (verbs37 row 9), the terrace from lz 20";
for (const k of Object.keys(before)) if (!["boxes", "ramps", "lights", "rooms"].includes(k) && !same(before[k], plan[k])) throw new Error(k + " changed");
for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== "p1" && !same(plan.rooms[i], before.rooms[i])) throw new Error("room changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-f-p1c: p1 chain wall 17 m, terrace from lz 20");
