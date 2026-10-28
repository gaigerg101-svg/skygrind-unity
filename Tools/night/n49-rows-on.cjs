// NIGHT49 freeze (the lead; from n47-rows-on.cjs): turns rows ON in Design/unity-config.json and copies it to
// Assets/StreamingAssets/Design/ (byte-identical). A row may sit in design.n47, design.n48 or design.n49 (the night is found from the
// config, so one reasons file serves P0 and the quiet freeze). Each row's enabled_why becomes "ON at the NIGHT<nn> freeze: <why>".
// Run:  node Tools/night/n49-rows-on.cjs <reasons.json> [--dry]      reasons.json = { "<row>": "<why ON>", ... }
// --dry prints what would turn on and writes nothing (every row must be found exactly once, and off).
const fs = require("fs");
const root = "C:/Users/gaige/dev/SKYGRIND-unity/";
const file = root + "Design/unity-config.json";
const copy = root + "Assets/StreamingAssets/Design/unity-config.json";
const dry = process.argv.includes("--dry");
const reasonsPath = process.argv.slice(2).find((a) => a.endsWith(".json"));
if (!reasonsPath) throw new Error("usage: node Tools/night/n49-rows-on.cjs <reasons.json> [--dry]");
const reasons = JSON.parse(fs.readFileSync(reasonsPath, "utf8"));
let s = fs.readFileSync(file, "utf8");
if (s !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ");
const d = JSON.parse(s);
const NIGHTS = ["n47", "n48", "n49"];
for (const [row, why] of Object.entries(reasons)) {
  const where = NIGHTS.filter((nn) => d.design[nn] && d.design[nn][row]);
  if (where.length !== 1) throw new Error(row + ": found in " + where.length + " of design.n47 / n48 / n49 (want exactly 1)");
  const nn = where[0];
  if (d.design[nn][row].enabled !== false) throw new Error(row + " (design." + nn + ") is not off");
  if (typeof why !== "string" || why.length === 0) throw new Error(row + ": a reason is required");
  // the row's own block: from `"<row>": {` to its `"enabled": false` (the first after the key), then its `enabled_why` line
  const key = '"' + row + '": {';
  const nightAt = s.indexOf('"' + nn + '": {');
  const at = s.indexOf(key, nightAt);
  if (nightAt < 0 || at < 0) throw new Error("row text not found: " + row);
  const en = s.indexOf('"enabled": false', at);
  const next = s.indexOf('"enabled_why": ', at);
  if (en < 0 || next < 0) throw new Error("enabled / enabled_why not found: " + row);
  const end = s.indexOf("\n", next);
  const line = s.slice(next, end);
  const comma = line.trimEnd().endsWith(",") ? "," : "";
  const cr = s[end - 1] === "\r" ? "\r" : "";
  s = s.slice(0, en) + '"enabled": true' + s.slice(en + '"enabled": false'.length);
  const at2 = s.indexOf(key, nightAt);
  const next2 = s.indexOf('"enabled_why": ', at2);
  const end2 = s.indexOf("\n", next2);
  s = s.slice(0, next2) + '"enabled_why": ' + JSON.stringify("ON at the NIGHT" + nn.slice(1) + " freeze: " + why) + comma + cr + s.slice(end2);
  console.log("ON " + nn + "." + row);
}
const chk = JSON.parse(s);
for (const row of Object.keys(reasons)) {
  const nn = NIGHTS.find((n) => chk.design[n] && chk.design[n][row]);
  if (!nn || chk.design[nn][row].enabled !== true) throw new Error(row + " not on after the edit");
}
if (dry) {
  console.log("DRY RUN: nothing written (" + Object.keys(reasons).length + " row(s))");
} else {
  fs.writeFileSync(file, s);
  fs.copyFileSync(file, copy);
  console.log("both copies written (" + Object.keys(reasons).length + " row(s))");
}
