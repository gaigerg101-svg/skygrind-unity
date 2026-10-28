// NIGHT53 the freeze (the lead's; NIGHT53.md section 7: "rows ON by their checks"): turns the named design.n53 rows ON with their reasons, in
// both config copies (from n52-freeze.cjs; NIGHT53 has a view row only, so no shared-test patch: N52Chain already follows later swaps).
//   node Tools/night/n53-freeze.cjs [--dry] <row>="<reason>" ...      e.g. polish53="U: a153 GREEN on / RED off"
// Refuses an unknown row or a row already ON; asserts every anchor once before writing.
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const dry = process.argv.includes("--dry");
const pairs = process.argv.slice(2).filter((a) => a !== "--dry").map((a) => { const i = a.indexOf("="); return [a.slice(0, i), a.slice(i + 1)]; });
if (!pairs.length || pairs.some(([r, why]) => !r || !why)) throw new Error('usage: n53-freeze.cjs [--dry] <row>="<reason>" ...');
const writes = [];
const cfgFiles = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];
const raw = fs.readFileSync(path.join(ROOT, cfgFiles[0]), "utf8");
if (raw !== fs.readFileSync(path.join(ROOT, cfgFiles[1]), "utf8")) throw new Error("the two config copies differ");
const d = JSON.parse(raw);
let out = raw;
for (const [row, why] of pairs) {
  const r = d.design.n53[row];
  if (!r) throw new Error("no design.n53." + row);
  if (r.enabled) throw new Error(row + " is already ON");
  // the row's own block: its "enabled": false and its enabled_why, found after its key, once
  const key = '"' + row + '": {';
  const at = out.indexOf(key, out.indexOf('"n53": {'));
  if (at < 0 || out.indexOf(key, at + 1) >= 0 && out.indexOf(key, at + 1) < out.length) {
    if (at < 0) throw new Error(row + ": its block was not found");
  }
  const en = out.indexOf('"enabled": false', at), ew = out.indexOf('"enabled_why": "', at);
  const own = out.indexOf('"_why": "', at), next = out.indexOf('"_why": "', own + 1); // the row's own _why, then the next row's
  if (en < 0 || ew < 0 || (next > 0 && (en > next || ew > next))) throw new Error(row + ": enabled / enabled_why not inside its block");
  const ewEnd = out.indexOf('"', ew + '"enabled_why": "'.length);
  const newWhy = ("ON at the NIGHT53 freeze: " + why).replace(/\\/g, "\\\\").replace(/"/g, '\\"');
  out = out.slice(0, ew) + '"enabled_why": "' + newWhy + out.slice(ewEnd);
  const en2 = out.indexOf('"enabled": false', at);
  out = out.slice(0, en2) + '"enabled": true' + out.slice(en2 + '"enabled": false'.length);
}
const back = JSON.parse(out);
for (const [row] of pairs) if (back.design.n53[row].enabled !== true || !back.design.n53[row].enabled_why.startsWith("ON")) throw new Error(row + " did not flip");
const a = JSON.parse(JSON.stringify(back)), b = JSON.parse(JSON.stringify(d));
for (const [row] of pairs) { a.design.n53[row].enabled = b.design.n53[row].enabled = null; a.design.n53[row].enabled_why = b.design.n53[row].enabled_why = null; }
if (JSON.stringify(a) !== JSON.stringify(b)) throw new Error("something else changed in the config");
for (const f of cfgFiles) writes.push([f, out]);

if (dry) { for (const [p] of writes) console.log("would write " + p); console.log("DRY RUN: rows " + pairs.map(([r]) => r).join(", ") + "; every anchor found once"); process.exit(0); }
for (const [p, s] of writes) fs.writeFileSync(path.join(ROOT, p), s);
console.log("NIGHT53 freeze: ON " + pairs.map(([r]) => r).join(", ") + "; both config copies; N47 / N48 shared tests follow later swaps (N52Chain)");
