// NIGHT52 (the lead's; DECIDE52 D-22): the polish lane's fallbacks the checks chose on Builds/n52-r15 (Logs/player/n47-reel-n52-r15-*.log):
//   design.n52.skin52.numbers.scorchGlow = 0: the scorch core still reads darker than bare floor at 0.5 s (rise -0.036 kiln / -0.122 spire on,
//     against >= +0.08; NIGHT51's -0.178 / -0.191): NIGHT51's mark ships, a252 judges what ships (U's key, round 9).
//   design.n52.look52.numbers.gradeLevels = []: iceline's lifted grade reaches 3/4 on the look meter (saturated 10-30 no), not 4/4: today's grade.
// Both config copies; asserts the current values once.   node Tools/night/n52-fallbacks.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const files = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"].map((p) => path.join(ROOT, p));
const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const d = JSON.parse(raw);
if (d.design.n52.skin52.numbers.scorchGlow !== undefined) throw new Error("scorchGlow exists (already run?)");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
let out = raw;
const g = '"gradeLevels": ["n47-iceline", "n52-iceline"],';
if (out.split(g).length !== 2) throw new Error("gradeLevels line not found once");
out = out.replace(g, '"gradeLevels": [],');
const sk = out.indexOf('"skin52": {');
const num = out.indexOf('"numbers": {}', sk);
const next = out.indexOf('"guns52": {', sk);
if (sk < 0 || num < 0 || num > next) throw new Error("skin52's empty numbers block not found");
const ind = "          ";
out = out.slice(0, num) + '"numbers": {' + eol + ind + '"scorchGlow": 0,' + eol + ind +
  '"scorchGlow_why": "NIGHT52 D-22: the core read darker than bare floor at 0.5 s on n52-r15 (rise -0.036 / -0.122 vs >= +0.08): NIGHT51\'s mark ships (U\'s fallback key)"' +
  eol + "        }" + out.slice(num + '"numbers": {}'.length);
const b = JSON.parse(out);
if (b.design.n52.skin52.numbers.scorchGlow !== 0 || b.design.n52.look52.numbers.gradeLevels.length !== 0) throw new Error("did not apply");
const x = JSON.parse(JSON.stringify(b)); delete x.design.n52.skin52.numbers.scorchGlow; delete x.design.n52.skin52.numbers.scorchGlow_why; x.design.n52.look52.numbers.gradeLevels = d.design.n52.look52.numbers.gradeLevels;
if (JSON.stringify(x) !== JSON.stringify(d)) throw new Error("something else changed");
for (const f of files) fs.writeFileSync(f, out);
console.log("skin52.scorchGlow = 0, look52.gradeLevels = [] (both copies)");
