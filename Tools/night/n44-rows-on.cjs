// NIGHT44 freeze (the lead): turns design.n44 rows ON in Design/unity-config.json and copies it to Assets/StreamingAssets/Design/
// (byte-identical). Each row's enabled_why becomes the reason given on the command line (the frames looked at and the iteration).
// Run: node Tools/night/n44-rows-on.cjs <reasons.json>   where reasons.json = { "<row>": "<why ON>", ... }
const fs = require("fs");
const root = "C:/Users/gaige/dev/SKYGRIND-unity/";
const file = root + "Design/unity-config.json";
const copy = root + "Assets/StreamingAssets/Design/unity-config.json";
const reasons = JSON.parse(fs.readFileSync(process.argv[2], "utf8"));
let s = fs.readFileSync(file, "utf8");
if (s !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ");
const d = JSON.parse(s);
for (const [row, why] of Object.entries(reasons)) {
  const r = d.design.n44[row];
  if (!r) throw new Error("no design.n44." + row);
  // the row's own block: from `"<row>": {` to its `"enabled": false` (the first after the key)
  const key = '"' + row + '": {';
  const at = s.indexOf(key, s.indexOf('"n44": {'));
  if (at < 0) throw new Error("row text not found: " + row);
  const en = s.indexOf('"enabled": false', at);
  const next = s.indexOf('"enabled_why": ', en);
  if (en < 0 || next < 0) throw new Error("enabled / enabled_why not found: " + row);
  const end = s.indexOf('\n', next);
  const line = s.slice(next, end);
  const comma = line.trimEnd().endsWith(",") ? "," : "";
  s = s.slice(0, en) + '"enabled": true' + s.slice(en + '"enabled": false'.length);
  const next2 = s.indexOf('"enabled_why": ', at);
  const end2 = s.indexOf('\n', next2);
  const cr = s[end2 - 1] === "\r" ? "\r" : "";
  s = s.slice(0, next2) + '"enabled_why": ' + JSON.stringify("ON at the NIGHT44 freeze: " + why) + comma + cr + s.slice(end2);
  console.log("ON " + row);
}
const chk = JSON.parse(s);
for (const row of Object.keys(reasons)) if (chk.design.n44[row].enabled !== true) throw new Error(row + " not on");
fs.writeFileSync(file, s);
fs.copyFileSync(file, copy);
console.log("both copies written");
