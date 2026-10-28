// NIGHT55 builder K: writes design.n55.<row>.numbers for K's rows (kill55, finish55, speed55, brief55) in both config copies
// (Design/unity-config.json and Assets/StreamingAssets/Design/unity-config.json, which must be byte-identical before and after).
// The numbers come from Tools/night/n55k-numbers.json ({ "<row>": { ...numbers... } }). Only the named rows' `numbers` block is replaced
// (found as the row's own `"numbers": ...` line one indent inside its block, through its closing brace); the parsed result is checked to
// differ from the old one only there.
//   node Tools/night/n55k-numbers.cjs kill55 [finish55 ...]
const fs = require("fs");
const rows = process.argv.slice(2);
const K = ["kill55", "finish55", "speed55", "brief55"];
if (!rows.length || rows.some((r) => !K.includes(r))) throw new Error("usage: n55k-numbers.cjs <" + K.join("|") + "> ...");
const files = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];
const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const data = JSON.parse(fs.readFileSync("Tools/night/n55k-numbers.json", "utf8"));
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
let lines = raw.split(eol);
const before = JSON.parse(raw);
const n55 = lines.findIndex((l) => l.trim() === '"n55": {');
if (n55 < 0) throw new Error("no n55 block");
for (const row of rows) {
  if (!data[row]) throw new Error("no numbers for " + row + " in n55k-numbers.json");
  const open = lines.findIndex((l, k) => k > n55 && l.trim() === '"' + row + '": {');
  if (open < 0) throw new Error("no " + row + " block");
  const ind = lines[open].match(/^ */)[0].length;
  const end = lines.findIndex((l, k) => k > open && l.match(/^ */)[0].length === ind && l.trim().startsWith("}"));
  const num = lines.findIndex((l, k) => k > open && k < end && l.match(/^ */)[0].length === ind + 2 && l.trim().startsWith('"numbers":'));
  if (num < 0) throw new Error(row + ": its numbers key not found");
  // the block's last line: the same line when it is `{}`, else the closing brace at ind + 2
  let last = num;
  if (!/^"numbers": \{\},?$/.test(lines[num].trim())) {
    last = lines.findIndex((l, k) => k > num && k < end && l.match(/^ */)[0].length === ind + 2 && l.trim().startsWith("}"));
    if (last < 0) throw new Error(row + ": the numbers block's end not found");
  }
  const comma = lines[last].trim().endsWith(",") ? "," : "";
  const pad = " ".repeat(ind + 2);
  const body = JSON.stringify(data[row], null, 2).split("\n").map((l, i) => (i === 0 ? pad + '"numbers": ' + l : pad + l));
  body[body.length - 1] += comma;
  lines = lines.slice(0, num).concat(body, lines.slice(last + 1));
}
const out = lines.join(eol);
const after = JSON.parse(out);
for (const row of rows) {
  if (JSON.stringify(after.design.n55[row].numbers) !== JSON.stringify(data[row])) throw new Error(row + ": the numbers did not land");
  after.design.n55[row].numbers = before.design.n55[row].numbers;
}
const norm = (o) => JSON.stringify(o, (k, v) => (v && typeof v === "object" && !Array.isArray(v) ? Object.fromEntries(Object.entries(v).sort()) : v));
if (norm(after) !== norm(before)) throw new Error("something else changed");
for (const f of files) fs.writeFileSync(f, out);
console.log("design.n55 numbers written for " + rows.join(", ") + "; both copies");
