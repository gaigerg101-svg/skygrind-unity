// EXT53 cleanup (the lead): span's level report gained one error with NIGHT53's p1 (builder K moved the start 5 m into the yard, n53-k-p1.cjs):
// "spawn31: p1 node (5, 0, 109.5) is in the first view cone at 8.6 m (in the cone: min 12)": a husk would spawn 8.6 m in front of you.
// The node moves to (4.5, 0, 104): 13.3 m from the start, beside the hoist well's railing (2.2 m off), the wave's east side kept.
// Asserts the node's current value; nothing else changes.   node Tools/night/e53-fix-p1.cjs; bash Tools/night/n52-loop-regen.sh span p1
const fs = require("fs");
const path = require("path");
const PLAN = path.join(__dirname, "../../Design/levels/n52/span.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text), before = JSON.parse(text);
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const p1 = plan.rooms.find((r) => r.id === "p1");
const i = p1.nodes.findIndex((n) => same(n, [5, 0, 109.5]));
if (i < 0) throw new Error("p1's node (5, 0, 109.5) not found (already fixed?)");
p1.nodes[i] = [4.5, 0, 104];
p1.nodes_why = (p1.nodes_why || "") + "; EXT53 (e53-fix-p1.cjs): (5, 0, 109.5) -> (4.5, 0, 104): 13.3 m from the new start (spawn31's 12 m in the first view)";
for (let k = 0; k < before.rooms.length; k++) if (before.rooms[k].id !== "p1" && !same(before.rooms[k], plan.rooms[k])) throw new Error("room changed");
for (const k of Object.keys(before)) if (k !== "rooms" && !same(before[k], plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("p1: the near node moved");
