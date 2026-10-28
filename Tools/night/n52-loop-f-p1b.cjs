// NIGHT52 the room loop, builder F (round 9, a freeze fix, D-30; the final P-rank review): span p1's WELL PAD sat mid-lane in the east lane's
// mouth on walk proof 1's line (jumpPad.radius 1 m: almost every lap round the drum threw you onto the engine deck): moved against the east
// hoist house's north face, (4.75, 0, 113) -> (9.5, 0, 119), its target unchanged. Edits Design/levels/n52/span.plan.json in place (p1 as
// n52-loop-f-p1.cjs left it; that script now writes the new spot too). Run: node Tools/night/n52-loop-f-p1b.cjs, then bash Tools/night/n52-loop-regen.sh span p1
const fs = require("fs");
const path = require("path");
const PLAN = path.join(__dirname, "../..", "Design/levels/n52/span.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const pad = plan.pads.find((p) => p.zone === "p1" && same(p.at, [4.75, 0, 113]));
if (!pad || !same(pad.target, [9.5, 2, 97.5])) throw new Error("p1's well pad is not round 7's (already moved?)");
pad.at = [9.5, 0, 119];
pad.why = pad.why.replace("at the east lane's south end beside the well's lip", "against the east hoist house's north face (round 9, D-30: off the east lane's walk line, where every lap round the drum stepped on it)");
pad.note = pad.why;
for (const k of Object.keys(before)) if (k !== "pads" && !same(before[k], plan[k])) throw new Error(k + " changed");
if (!same(before.pads.filter((p) => p !== before.pads[plan.pads.indexOf(pad)]), plan.pads.filter((p) => p !== pad))) throw new Error("another pad changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-f-p1b: p1's well pad -> (9.5, 0, 119)");
