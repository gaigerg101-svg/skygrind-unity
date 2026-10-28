// EXT53 cleanup (the lead; the EXT53 review at 11:56 flagged it, the plan confirmed it): builder M's g2 (e53-m-g2.cjs) moved the room's east
// wall 3 m in (rect x -12..9) and rebuilt THE EAST GANTRY "4 x 5 m against the east wall", but the deck kept x 5..12: 3 m of it inside and
// behind the new wall. The deck ends at the wall now (x 5..9: the 4 x 5 m its why names). Asserts the deck's current box; nothing else changes.
//   node Tools/night/e53-fix-g2.cjs; bash Tools/night/n52-loop-regen.sh iceline g2
const fs = require("fs");
const path = require("path");
const PLAN = path.join(__dirname, "../../Design/levels/n52/iceline.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text), before = JSON.parse(text);
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const g2 = plan.rooms.find((r) => r.id === "g2");
if (!same(g2.rect, [-12, -58, 9, -30])) throw new Error("g2's rect " + JSON.stringify(g2.rect));
const d = plan.boxes.filter((b) => b.zone === "g2" && b.kind === "deck" && same(b.min, [5, 0, -58]) && same(b.max, [12, 4, -53]));
if (d.length !== 1) throw new Error("the east gantry: " + d.length);
d[0].max = [9, 4, -53];
d[0].why += " (EXT53 e53-fix-g2.cjs: the deck ends at the moved east wall, x 5..9; it ran 3 m into it)";
if (d[0].note !== undefined) d[0].note = d[0].why;
const keep = (b) => b !== d[0];
if (!same(before.boxes.filter((b, i) => keep(plan.boxes[i])), plan.boxes.filter(keep))) throw new Error("other boxes changed");
for (const k of Object.keys(before)) if (k !== "boxes" && !same(before[k], plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("g2: the east gantry ends at the wall");
