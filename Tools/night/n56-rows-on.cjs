// NIGHT56 freeze (the lead; from n46-rows-on.cjs, any night): turns design rows ON in Design/unity-config.json and writes the byte copy
// Assets/StreamingAssets/Design/unity-config.json. Each row's enabled_why becomes "ON at the NIGHT56 freeze: <why>" (the N55 / N56
// SharedTests require an ON row's enabled_why to start with "ON"). Asserts that nothing but those rows' enabled / enabled_why changed.
//   node Tools/night/n56-rows-on.cjs <reasons.json>   where reasons.json = { "n55.kill55": "<why ON>", "n56.colour56": "...", ... }
const fs = require("fs");
const root = "C:/Users/gaige/dev/SKYGRIND-unity/";
const file = root + "Design/unity-config.json";
const copy = root + "Assets/StreamingAssets/Design/unity-config.json";
const reasons = JSON.parse(fs.readFileSync(process.argv[2], "utf8"));
let s = fs.readFileSync(file, "utf8");
if (s !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ");
const before = JSON.parse(s);
for (const [nr, why] of Object.entries(reasons)) {
  const [night, row] = nr.split(".");
  if (!before.design[night] || !before.design[night][row]) throw new Error("no design." + nr);
  if (before.design[night][row].enabled !== false) throw new Error("design." + nr + " is not off");
  const nAt = s.indexOf('"' + night + '": {');
  if (nAt < 0 || s.indexOf('"' + night + '": {', nAt + 1) >= 0) throw new Error("night block not found once: " + night);
  const at = s.indexOf('"' + row + '": {', nAt);
  if (at < 0) throw new Error("row text not found: " + nr);
  const en = s.indexOf('"enabled": false', at);
  if (en < 0) throw new Error("enabled not found: " + nr);
  s = s.slice(0, en) + '"enabled": true' + s.slice(en + '"enabled": false'.length);
  const w = s.indexOf('"enabled_why": ', en);
  const end = s.indexOf("\n", w);
  const line = s.slice(w, end);
  const comma = line.trimEnd().endsWith(",") ? "," : "";
  const cr = s[end - 1] === "\r" ? "\r" : "";
  s = s.slice(0, w) + '"enabled_why": ' + JSON.stringify("ON at the NIGHT56 freeze: " + why) + comma + cr + s.slice(end);
  console.log("ON design." + nr);
}
const after = JSON.parse(s);
for (const nr of Object.keys(reasons)) {
  const [night, row] = nr.split(".");
  if (after.design[night][row].enabled !== true) throw new Error(nr + " not on");
  before.design[night][row].enabled = true;
  before.design[night][row].enabled_why = after.design[night][row].enabled_why;
}
if (JSON.stringify(before) !== JSON.stringify(after)) throw new Error("something beyond the rows' enabled / enabled_why changed");
fs.writeFileSync(file, s);
fs.writeFileSync(copy, s);
console.log("both copies written (" + Object.keys(reasons).length + " row(s) ON)");
