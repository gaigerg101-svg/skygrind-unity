// NIGHT52 the room loop, fix 6 (the lead's; DECIDE52 D-30, the final P-rank review of the late rooms): iceline g4 THE REACH II.
//   1. The east lock wall's ride ended in a 3 x 7 m dead end (x 9..12, z -112.5..-119: the gallery ramp's side, the east wall, the gallery's
//      4 m face), and its why says "jump off its end at the hoist pad", but the canal hoist stood 6.6 m west behind the ramp: the hoist moves
//      into the pocket at (10.5, 0, -115.5), its target unchanged (a 6.3 m throw onto the gallery): the ride lands on it, the pocket has an exit.
//   2. The west lock wall's ride came down half on the NW ledge ramp's west edge and half in the 1.5 x 5 m slot between the ramp and the west
//      wall (closed by the ledge's 2 m face): the ramp shifts 1.5 m west, flush to the wall (centre x -10.5: x -12..-9): no slot.
// Asserts g4 is n52-loop-g4.cjs's; every other room bit for bit.
// Run: node Tools/night/n52-loop-fix6.cjs, then bash Tools/night/n52-loop-regen.sh iceline g4
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/iceline.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const near = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const g4 = plan.rooms.find((r) => r.id === "g4");
if (!/n52-loop-g4\.cjs/.test(g4.loop52_why || "") || /Fix 6/.test(g4.loop52_why)) throw new Error("g4 is not n52-loop-g4.cjs's (or already run)");
const hoist = (plan.pads || []).filter((p) => p.zone === "g4" && near(p.at, [4, 0, -116]));
if (hoist.length !== 1) throw new Error("the canal hoist: " + hoist.length);
hoist[0].at = [10.5, 0, -115.5];
hoist[0].why += " (NIGHT52 fix 6: moved into the east ride's end pocket, x 9..12: the ride lands on it, the pocket's exit)";
hoist[0].note = "the canal hoist";
const ramp = plan.ramps.filter((r) => r.zone === "g4" && near(r.from, [-9, -113]) && near(r.to, [-9, -118]));
if (ramp.length !== 1) throw new Error("the NW ledge ramp: " + ramp.length);
ramp[0].from = [-10.5, -113];
ramp[0].to = [-10.5, -118];
ramp[0].why += " (NIGHT52 fix 6: 1.5 m west, flush to the west wall: the west ride lands on it, no slot beside it)";
ramp[0].note = ramp[0].why;
g4.loop52_why += " Fix 6 (n52-loop-fix6.cjs): the canal hoist in the east ride's pocket, the NW ramp flush to the wall.";
for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== "g4" && JSON.stringify(plan.rooms[i]) !== JSON.stringify(before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
for (const k of Object.keys(before)) if (!["rooms", "ramps", "pads"].includes(k) && JSON.stringify(before[k]) !== JSON.stringify(plan[k])) throw new Error(k + " changed");
const others = (arr) => (arr || []).filter((b) => b.zone !== "g4");
for (const k of ["ramps", "pads"]) if (JSON.stringify(others(before[k])) !== JSON.stringify(others(plan[k]))) throw new Error(k + " outside g4 changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-fix6: g4's canal hoist into the east pocket, the NW ramp flush to the wall");
