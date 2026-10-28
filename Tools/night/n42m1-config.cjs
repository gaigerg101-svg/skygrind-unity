// NIGHT42 builder M1: patch ONE of M1's own rows in design.n42 (wallfling42 or boost42) in both config copies, keeping them byte-identical.
//   node Tools/night/n42m1-config.cjs <row> <spec.json>
// spec.json: an object of keys to set in the row (with their _why); every other key of the row is kept; `enabled` is never touched here.
// The row is re-serialized as the shared commit wrote it (JSON.stringify, 2 spaces, the row's own indent, the file's line ends).
const fs = require("fs");
const path = require("path");
const root = path.join(__dirname, "../..");
const file = path.join(root, "Design/unity-config.json");
const copy = path.join(root, "Assets/StreamingAssets/Design/unity-config.json");
const row = process.argv[2];
const spec = JSON.parse(fs.readFileSync(process.argv[3], "utf8"));
if (row !== "wallfling42" && row !== "boost42") throw new Error("M1 owns wallfling42 and boost42 only");
if ("enabled" in spec) throw new Error("never set enabled (the lead turns a row on)");
let raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ before the patch");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const n42At = raw.indexOf('"n42": {');
if (n42At < 0) throw new Error("no design.n42");
const re = new RegExp('(\\r?\\n)([ ]*)"' + row + '": \\{', "g");
re.lastIndex = n42At;
const m = re.exec(raw);
if (!m) throw new Error("no row " + row);
const indent = m[2];
const open = m.index + m[0].length - 1; // the row's "{"
let depth = 0, inStr = false, end = -1;
for (let i = open; i < raw.length; i++) {
  const ch = raw[i];
  if (inStr) {
    if (ch === "\\") i++;
    else if (ch === '"') inStr = false;
    continue;
  }
  if (ch === '"') inStr = true;
  else if (ch === "{") depth++;
  else if (ch === "}") {
    depth--;
    if (depth === 0) { end = i; break; }
  }
}
if (end < 0) throw new Error("unclosed row");
const obj = JSON.parse(raw.slice(open, end + 1));
for (const k of Object.keys(spec)) obj[k] = spec[k];
const text = JSON.stringify(obj, null, 2).split("\n").map((l, i) => (i === 0 ? l : indent + l)).join(eol);
const out = raw.slice(0, open) + text + raw.slice(end + 1);
JSON.parse(out); // still JSON
fs.writeFileSync(file, out);
fs.writeFileSync(copy, out);
console.log(`n42m1-config: design.n42.${row}: ${Object.keys(spec).filter(k => !k.endsWith("_why")).map(k => k + "=" + JSON.stringify(spec[k])).join(" ")}`);
