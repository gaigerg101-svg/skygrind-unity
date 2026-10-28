// NIGHT52 (the lead's; DECIDE52 D-20): look52's gradeLevels narrowed to iceline. U's round 9 (r12 frames interpolated): iceline reaches the
// look meter's 4/4 at a wall lift of 1.08-1.12 (now 1.12); span reaches it at no lift value, so span keeps today's grade (no list entry).
// Both config copies; asserts the round-6 list once.   node Tools/night/n52-look52-grade2.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const files = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"].map((p) => path.join(ROOT, p));
const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const old = '"gradeLevels": ["n47-iceline", "n47-span", "n52-iceline", "n52-span"],';
if (raw.split(old).length !== 2) throw new Error("the round-6 gradeLevels line was not found once (already run?)");
const out = raw.replace(old, '"gradeLevels": ["n47-iceline", "n52-iceline"],');
const d = JSON.parse(raw), b = JSON.parse(out);
if (JSON.stringify(b.design.n52.look52.numbers.gradeLevels) !== JSON.stringify(["n47-iceline", "n52-iceline"])) throw new Error("did not apply");
b.design.n52.look52.numbers.gradeLevels = d.design.n52.look52.numbers.gradeLevels;
if (JSON.stringify(b) !== JSON.stringify(d)) throw new Error("something else changed");
for (const f of files) fs.writeFileSync(f, out);
console.log("look52.gradeLevels = [n47-iceline, n52-iceline] (span: today's grade)");
