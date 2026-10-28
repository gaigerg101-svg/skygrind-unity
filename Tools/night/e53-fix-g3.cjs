// EXT53 cleanup (the lead): two levelgen errors that NIGHT53's g3 cycle (n53-loop-g3.cjs) left in iceline's report (Reports/LEVEL-n52-iceline-
// validate.md: "errors: 2"; the cycle's regen filter hid the report line, builder M found it at 11:1x):
//   1. "no grid node under: g3 node (-7.5, 0, -86.5)": the node sat 0.25 m off the NW coal car (x -10.25..-7.75, z -86.75..-85.25), inside
//      the nav probe's margin. It moves to (-7, 0, -88.5): the west lane's north end still, 1.9 m off the car's corner, 3.5 m off the hoist pad.
//   2. "g3-L2: not flush to a surface": THE HOPPER's amber lamp hung where the 6 x 8 m hopper's door face was (z -74); the hopper is 4 x 6 m
//      now and its chute is on its west face: the lamp moves onto that face over the chute (x -6.05, facing west, z -78).
// Asserts both rows' current values; nothing else changes.   node Tools/night/e53-fix-g3.cjs; bash Tools/night/n52-loop-regen.sh iceline g3
const fs = require("fs");
const path = require("path");
const PLAN = path.join(__dirname, "../../Design/levels/n52/iceline.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text), before = JSON.parse(text);
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const g3 = plan.rooms.find((r) => r.id === "g3");
const ni = g3.nodes.findIndex((n) => same(n, [-7.5, 0, -86.5]));
if (ni < 0) throw new Error("g3's node (-7.5, 0, -86.5) not found (already fixed?)");
g3.nodes[ni] = [-7, 0, -88.5];
g3.nodes_why += "; EXT53 (e53-fix-g3.cjs): the (-7.5, 0, -86.5) node off the NW coal car's margin to (-7, 0, -88.5) (levelgen: no grid node under it)";
const L = plan.lights.filter((l) => l.id === "g3-L2" && same(l.at, [-4, 4, -73.95]) && same(l.normal, [0, 0, 1]));
if (L.length !== 1) throw new Error("g3-L2 not as NIGHT53 left it");
L[0].at = [-6.05, 4, -78];
L[0].normal = [-1, 0, 0];
L[0].why += " (EXT53: on the 4 x 6 m hopper's west face, over the chute; it hung where the old hopper's door face was)";
for (const k of Object.keys(before)) {
  if (k === "rooms") { for (let i = 0; i < before.rooms.length; i++) if (before.rooms[i].id !== "g3" && !same(before.rooms[i], plan.rooms[i])) throw new Error("room changed"); }
  else if (k === "lights") { const keep = (l) => l.id !== "g3-L2"; if (!same(before.lights.filter(keep), plan.lights.filter(keep))) throw new Error("lights changed"); }
  else if (!same(before[k], plan[k])) throw new Error(k + " changed");
}
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("g3: the node and the lamp fixed");
