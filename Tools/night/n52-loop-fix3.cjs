// NIGHT52 the room loop, fix 3 (the lead's; DECIDE52 D-15): g7's door dwell measured in isolation (n52-room-flow.sh) was 0.65 -> 1.7 s (the
// mean of seeds 1-10 and 11-20): the boiler shields the door landing from both far stands, so the walk pilot holds at the door. The P-rank
// reviewer's finding 1 ("the door third is a keep behind a lava moat: move two nodes south of the lava") is applied: two quay nodes, 2 m short
// of the grate beside the mooring posts (11.5 m from the door, in view), so waves 1-2 can land on the door's side of the fire. Asserts fix 2.
// Run: node Tools/night/n52-loop-fix3.cjs, then bash Tools/night/n52-loop-regen.sh iceline g7
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/iceline.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const g7 = plan.rooms.find((r) => r.id === "g7");
if (!/Fix 2/.test(g7.loop52_why || "")) throw new Error("g7 is not fix 2's");
if (g7.nodes.length === 8 && /Fix 3/.test(g7.loop52_why)) { g7.nodes.length = 6; g7.loop52_why = g7.loop52_why.replace(" Fix 3 (n52-loop-fix3.cjs): two quay nodes.", ""); g7.nodes_why = g7.nodes_why.replace(/; NIGHT52 fix 3: .*$/, ""); } // the first try's nodes (9.9 m from the spawn: spawn31's 12 m in the view cone)
if (g7.nodes.length !== 6) throw new Error("g7 nodes: " + g7.nodes.length);
// 12.1 m from the spawn and ~55 deg off its yaw (spawn31: a node in the first view cone must be >= 12 m away)
g7.nodes.push([-10, 0, -192.8], [10, 0, -192.8]);
g7.nodes_why += "; NIGHT52 fix 3: two quay nodes beside the mooring posts (the P-rank review: the door third must be attackable from its own side, not a keep behind the fire)";
g7.loop52_why += " Fix 3 (n52-loop-fix3.cjs): two quay nodes.";
for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== "g7" && JSON.stringify(plan.rooms[i]) !== JSON.stringify(before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
for (const k of Object.keys(before)) if (k !== "rooms" && JSON.stringify(before[k]) !== JSON.stringify(plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-fix3: g7 two quay nodes");
