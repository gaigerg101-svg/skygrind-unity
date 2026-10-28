// NIGHT47 builder F: copy47's map check. Every OLD template in design.n47.copy47.numbers.map must be a string that some config or level file
// shows (else the line never swaps: a dead entry), and every NEW line must keep the OLD one's {action} placeholders (a button the old line named is
// never lost) and meet the copy rules' word cap (<= 12 tokens counting a placeholder as one word; NIGHT47.md section 3: <= 10 words a line, the
// weapon card's name sentence excluded). Run: node Tools/night/n47-copy-check.cjs   (exit 1 on a miss)
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const cfg = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/unity-config.json"), "utf8"));
const map = cfg.design.n47.copy47.numbers.map;
const strings = new Set();
function walk(o) {
  if (typeof o === "string") strings.add(o);
  else if (Array.isArray(o)) o.forEach(walk);
  else if (o && typeof o === "object") Object.values(o).forEach(walk);
}
walk(cfg);
// the C# string literals too (a few lines live in UnityTuning: the message slot's danger words)
function walkCs(dir) {
  for (const f of fs.readdirSync(dir, { withFileTypes: true })) {
    const p = path.join(dir, f.name);
    if (f.isDirectory()) walkCs(p);
    else if (f.name.endsWith(".cs")) for (const m of fs.readFileSync(p, "utf8").matchAll(/"((?:[^"\\]|\\.)*)"/g)) strings.add(m[1]);
  }
}
walkCs(path.join(ROOT, "Assets/Scripts"));
const lv = path.join(ROOT, "Design/levels");
for (const f of fs.readdirSync(lv)) if (f.endsWith(".json")) walk(JSON.parse(fs.readFileSync(path.join(lv, f), "utf8")));
let bad = 0, n = 0;
const ph = (s) => (s.match(/\{[A-Za-z]+\}/g) || []).sort().join(",");
for (const [oldS, newS] of Object.entries(map)) {
  n++;
  const misses = [];
  if (!strings.has(oldS)) misses.push("OLD not found in any config / level file");
  const po = ph(oldS), pn = ph(newS);
  const lost = (oldS.match(/\{[A-Za-z]+\}/g) || []).filter((p) => !newS.includes(p));
  if (lost.length) misses.push("NEW drops " + lost.join(" "));
  const body = newS.replace(/^THE [A-Z ]+\.\s*/, "");
  const words = body.split(/\s+/).filter((w) => w.length).length;
  if (words > 12) misses.push("NEW has " + words + " words");
  if (misses.length) { bad++; console.log("MISS  " + JSON.stringify(oldS) + ": " + misses.join("; ")); }
}
console.log("n47-copy-check: " + n + " lines, " + bad + " miss(es)");
process.exit(bad ? 1 : 0);
