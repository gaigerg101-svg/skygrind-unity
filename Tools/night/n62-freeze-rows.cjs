// NIGHT62 (the forge FORGE62, 2026-10-22; a copy of n60-freeze-rows.cjs with the NIGHT62 label; NIGHT57.md section 0's rule plus NIGHT62's
// visible rule): turn the rows that passed ON in the REPO's config (both copies, byte identical), each with an enabled_why starting
// "ON at NIGHT62 by the rule: <reason>"; asserts that nothing else changes. Off switches stay: -skygrindOffNN for each row's night.
// It refuses a row unless the slate (Logs/n62/slate.json, or --slate=<path>) holds a slice whose `row` names design.<night.row>, with
// passes true, state pass and a verdict file whose first line is "<id>: PASS" (FORGE62 red team: the tool used to flip any row).
//   node Tools/night/n62-freeze-rows.cjs <night.row>=<reason> [<night.row>=<reason> ...] [--dry] [--slate=<path>]
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const A = path.join(ROOT, "Design/unity-config.json"), B = path.join(ROOT, "Assets/StreamingAssets/Design/unity-config.json");
const LABEL = "ON at NIGHT62 by the rule: ";
const dry = process.argv.includes("--dry");
const slateArg = process.argv.find((a) => a.startsWith("--slate="));
const SLATE = slateArg ? slateArg.slice("--slate=".length) : path.join(ROOT, "Logs/n62/slate.json");
const pairs = process.argv.slice(2).filter((a) => !a.startsWith("--")).map((a) => { const i = a.indexOf("="); return [a.slice(0, i), a.slice(i + 1)]; });
if (!pairs.length) throw new Error("usage: n62-freeze-rows.cjs <night.row>=<reason> ...");
let slices;
try { const s = JSON.parse(fs.readFileSync(SLATE, "utf8")); slices = Array.isArray(s) ? s : s.slices; } catch (e) { throw new Error("cannot read the slate " + SLATE + ": " + e.message); }
for (const [key] of pairs) {
  const hit = (slices || []).find((s) => String(s.row).includes("design." + key) && s.passes === true && s.state === "pass");
  if (!hit) throw new Error(key + ": no slate slice with row design." + key + ", passes true and state pass in " + SLATE + ": a row turns ON only after its critic PASS");
  const first = hit.verdict && fs.existsSync(hit.verdict) ? fs.readFileSync(hit.verdict, "utf8").split(/\r?\n/)[0].trim() : "";
  if (first !== hit.id + ": PASS") throw new Error(key + ": " + hit.id + "'s verdict file does not start '" + hit.id + ": PASS'");
}
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
  r.enabled_why = LABEL + reason;
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
    .replace(/"enabled_why": "(?:[^"\\]|\\.)*"/, '"enabled_why": ' + JSON.stringify(LABEL + reason));
  raw = raw.slice(0, rowAt) + block + raw.slice(end);
}
if (JSON.stringify(JSON.parse(raw)) !== JSON.stringify(after)) throw new Error("the text edit and the parsed edit disagree");
const chk = JSON.parse(raw);
for (const [key] of pairs) { const [n, r] = key.split("."); chk.design[n][r].enabled = before.design[n][r].enabled; chk.design[n][r].enabled_why = before.design[n][r].enabled_why; }
if (JSON.stringify(chk) !== JSON.stringify(before)) throw new Error("something beyond the named rows changed");
if (dry) { console.log("DRY: would turn ON " + pairs.map((p) => p[0]).join(", ")); process.exit(0); }
fs.writeFileSync(A, raw); fs.writeFileSync(B, raw);
console.log("ON in both config copies: " + pairs.map((p) => p[0]).join(", "));
