// NIGHT54 (the lead; DECIDE54 D-16): turns one boss's grade off inside grade54 (design.n54.grade54.numbers.<boss>.on = 0) in both config
// copies, with an on_why on the same line. The a254 danger-hue check (the graded mean hue >= 20 deg from the danger hue at p1 / the turn /
// p2) failed on n54-freeze2 for the named bosses; the row stays ON for the rest. Finds the boss block's own `"on": 1,` line (the first one
// after the block's opening at one indent deeper), asserts it, and checks the parsed result differs only by those keys.
//   node Tools/night/n54-grade-off.cjs <boss> "<why>" [<boss> "<why>" ...]
const fs = require("fs");
const args = process.argv.slice(2);
if (!args.length || args.length % 2) throw new Error('usage: n54-grade-off.cjs <boss> "<why>" ...');
const files = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];
const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const lines = raw.split(eol);
const before = JSON.parse(raw);
const g = lines.findIndex((l) => l.trim() === '"grade54": {');
if (g < 0) throw new Error("no grade54");
for (let i = 0; i < args.length; i += 2) {
  const boss = args[i], why = args[i + 1];
  if (!before.design.n54.grade54.numbers[boss] || before.design.n54.grade54.numbers[boss].on !== 1) throw new Error(boss + ".on is not 1");
  const open = lines.findIndex((l, k) => k > g && l.trim() === '"' + boss + '": {');
  if (open < 0) throw new Error("no " + boss + " block");
  const ind = lines[open].match(/^ */)[0].length;
  const end = lines.findIndex((l, k) => k > open && l.match(/^ */)[0].length === ind && l.trim().startsWith("}"));
  const on = lines.findIndex((l, k) => k > open && k < end && l.match(/^ */)[0].length === ind + 2 && /^"on": 1,?$/.test(l.trim()));
  if (on < 0) throw new Error(boss + ": its own on key not found");
  lines[on] = lines[on].replace('"on": 1', '"on": 0');
  const w = on + 1; // G's own on_why follows the key: the reason is prefixed to it
  const m = lines[w] && lines[w].match(/^( *)"on_why": (".*")(,?)$/);
  if (!m) throw new Error(boss + ": no on_why line after its on key");
  lines[w] = m[1] + '"on_why": ' + JSON.stringify("NIGHT54 D-16: OFF at the freeze: " + why + ". " + JSON.parse(m[2])) + m[3];
}
const out = lines.join(eol);
const after = JSON.parse(out);
for (let i = 0; i < args.length; i += 2) {
  const b = after.design.n54.grade54.numbers[args[i]];
  if (b.on !== 0) throw new Error(args[i] + " did not flip");
  b.on = 1; b.on_why = before.design.n54.grade54.numbers[args[i]].on_why;
}
const norm = (o) => JSON.stringify(o, (k, v) => (v && typeof v === "object" && !Array.isArray(v) ? Object.fromEntries(Object.entries(v).sort()) : v));
if (norm(after) !== norm(before)) throw new Error("something else changed");
for (const f of files) fs.writeFileSync(f, out);
console.log("grade54 off for " + args.filter((_, i) => i % 2 === 0).join(", ") + "; both copies");
