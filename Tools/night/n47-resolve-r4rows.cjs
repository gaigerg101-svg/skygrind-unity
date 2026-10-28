// NIGHT47 (the lead): resolves the one config conflict of F's R4 pick (74d7996e) against B's R4 rows: both sides only ADD rows at the
// end of design.n47 (B: stampede47, trail47; F: finale47, hitzone47), so the resolution keeps both: B's block, closes trail47, then F's.
// Run: node Tools/night/n47-resolve-r4rows.cjs   (writes both config copies, byte-identical; checks the JSON parses and all four rows exist)
const fs = require("fs");
const root = "C:/Users/gaige/dev/SKYGRIND-unity/";
const file = root + "Design/unity-config.json";
let s = fs.readFileSync(file, "utf8");
const crlf = s.includes("\r\n");
if (crlf) s = s.replace(/\r\n/g, "\n");
const re = /<<<<<<< HEAD\n([\s\S]*?)\n=======\n([\s\S]*?)\n>>>>>>> [^\n]*\n/;
const m = s.match(re);
if (!m || (s.match(/<<<<<<< /g) || []).length !== 1) throw new Error("expected exactly one conflict");
const ours = m[1], theirs = m[2];
if (!ours.includes('"stampede47"') || !ours.includes('"trail47"') || !theirs.includes('"finale47"') || !theirs.includes('"hitzone47"')) throw new Error("unexpected hunk");
s = s.replace(re, () => ours + "\n      },\n" + theirs + "\n");
const d = JSON.parse(s);
for (const r of ["stampede47", "trail47", "finale47", "hitzone47"]) if (!d.design.n47[r]) throw new Error("missing " + r);
if (crlf) s = s.replace(/\n/g, "\r\n");
fs.writeFileSync(file, s);
fs.writeFileSync(root + "Assets/StreamingAssets/Design/unity-config.json", s);
console.log("resolved: design.n47 keeps stampede47, trail47, finale47, hitzone47; rows " + Object.keys(d.design.n47).length);
