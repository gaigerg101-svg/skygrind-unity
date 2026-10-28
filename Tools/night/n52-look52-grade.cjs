// NIGHT52 (the lead's; DECIDE52 D-20): design.n52.look52.numbers.gradeLevels, the level ids look49's grade (with U's wall lift) covers only
// while look52 is on: iceline and span (leftover 3, T-116), both their n47 ids and the n52 copies. look49's own list
// (design.n49.look49.numbers.levels) is untouched, so -skygrindOff52 gives today's grade exactly. Both config copies.
//   node Tools/night/n52-look52-grade.cjs   (refuses to run twice)
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const files = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"].map((p) => path.join(ROOT, p));
const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const d = JSON.parse(raw);
if (!d.design.n52.look52 || d.design.n52.look52.numbers.gradeLevels) throw new Error("look52 missing or gradeLevels exists (already run?)");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const at = raw.indexOf('"look52": {');
const numAt = raw.indexOf('"numbers": {}', at);
if (at < 0 || numAt < 0 || raw.indexOf('"look52": {', at + 1) >= 0) throw new Error("look52's numbers block not found once");
const ind = "          ";
const block = '"numbers": {' + eol + ind + '"gradeLevels": ["n47-iceline", "n47-span", "n52-iceline", "n52-span"],' + eol + ind +
  '"gradeLevels_why": "NIGHT52 U round 6-7 (leftover 3, T-116; DECIDE52 D-20): look49\'s grade with the wall lift (UnityTuning.Look52WallLift) on these levels only while look52 is on; look49\'s own list is untouched (off = today\'s grade)"' + eol + "        }";
const out = raw.slice(0, numAt) + block + raw.slice(numAt + '"numbers": {}'.length);
const back = JSON.parse(out);
if (JSON.stringify(back.design.n52.look52.numbers.gradeLevels) !== JSON.stringify(["n47-iceline", "n47-span", "n52-iceline", "n52-span"])) throw new Error("did not round-trip");
const a = JSON.parse(JSON.stringify(back)); a.design.n52.look52.numbers = {};
if (JSON.stringify(a) !== JSON.stringify(d)) throw new Error("something else changed");
for (const f of files) fs.writeFileSync(f, out);
console.log("design.n52.look52.numbers.gradeLevels set (both copies)");
