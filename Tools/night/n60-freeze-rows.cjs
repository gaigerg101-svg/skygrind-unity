// NIGHT60 (the lead; a copy of n58-freeze-rows.cjs; NIGHT60.md section 1, NIGHT57.md section 0's rule): turn the rows that passed the shipping rule ON in the REPO's
// config (both copies, byte identical), each with an enabled_why starting "ON at NIGHT60 by the rule: <reason>"; asserts that nothing else
// changes. Off switches stay: -skygrindOffNN for each row's night.
//   node Tools/night/n60-freeze-rows.cjs <night.row>=<reason> [<night.row>=<reason> ...] [--dry]
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const A = path.join(ROOT, "Design/unity-config.json"), B = path.join(ROOT, "Assets/StreamingAssets/Design/unity-config.json");
const dry = process.argv.includes("--dry");
const pairs = process.argv.slice(2).filter((a) => !a.startsWith("--")).map((a) => { const i = a.indexOf("="); return [a.slice(0, i), a.slice(i + 1)]; });
if (!pairs.length) throw new Error("usage: n60-freeze-rows.cjs <night.row>=<reason> ...");
let raw = fs.readFileSync(A, "utf8");
if (raw !== fs.readFileSync(B, "utf8")) throw new Error("the two config copies differ");
const before = JSON.parse(raw);
const after = JSON.parse(raw);
for (const [key, reason] of pairs) {
  const [night, row] = key.split(".");
  const r = after.design[night] && after.design[night][row];
  if (!r || typeof r.enabled !== "boolean") throw new Error(key + ": no such row with a bool enabled");
  if (!reason || reason.length < 20) throw new Error(key + ": give a measured reason");
  r.enabled = true;
  r.enabled_why = "ON at NIGHT60 by the rule: " + reason;
}
// text edit per row (keeps the file's layout): the row's "enabled": false and its "enabled_why" line, found inside its own block
for (const [key, reason] of pairs) {
  const [night, row] = key.split(".");
  const nightAt = raw.indexOf('\n    "' + night + '": {');
  if (nightAt < 0) throw new Error(night + ": block not found");
  const rowAt = raw.indexOf('\n      "' + row + '": {', nightAt);
  if (rowAt < 0) throw new Error(key + ": row not found");
  const end = raw.indexOf("\n      }", rowAt);
  let block = raw.slice(rowAt, end);
  if (!/"enabled": false/.test(block)) throw new Error(key + ": not off now");
  block = block.replace('"enabled": false', '"enabled": true')
    .replace(/"enabled_why": "(?:[^"\\]|\\.)*"/, '"enabled_why": ' + JSON.stringify("ON at NIGHT60 by the rule: " + reason));
  raw = raw.slice(0, rowAt) + block + raw.slice(end);
}
if (JSON.stringify(JSON.parse(raw)) !== JSON.stringify(after)) throw new Error("the text edit and the parsed edit disagree");
const chk = JSON.parse(raw);
for (const [key] of pairs) { const [n, r] = key.split("."); chk.design[n][r].enabled = before.design[n][r].enabled; chk.design[n][r].enabled_why = before.design[n][r].enabled_why; }
if (JSON.stringify(chk) !== JSON.stringify(before)) throw new Error("something beyond the named rows changed");
if (dry) { console.log("DRY: would turn ON " + pairs.map((p) => p[0]).join(", ")); process.exit(0); }
fs.writeFileSync(A, raw); fs.writeFileSync(B, raw);
console.log("ON in both config copies: " + pairs.map((p) => p[0]).join(", "));
