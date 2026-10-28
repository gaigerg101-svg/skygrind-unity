// NIGHT55 (the lead): adds rows to design.n55 after the shared commit (the auditor's OPEN.md came after it), off, with an empty
// numbers object, in both config copies, and lists them in N55SharedTests' Views / Sims arrays.
//   node Tools/night/n55-addrows.cjs <id>:<sim|view>:<owner>:<why> ...
const fs = require("fs");
const root = "C:/Users/gaige/dev/SKYGRIND-unity/";
const specs = process.argv.slice(2).map((a) => { const [id, kind, owner, ...why] = a.split(":"); return { id, sim: kind === "sim", owner, why: why.join(":") }; });
if (!specs.length) throw new Error("usage: n55-addrows.cjs <id>:<sim|view>:<owner>:<why> ...");
const file = root + "Design/unity-config.json", copy = root + "Assets/StreamingAssets/Design/unity-config.json";
const raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const d = JSON.parse(raw);
const before = JSON.parse(raw);
const off = "Off until its slate entry (Logs/n55/slate.json) passes the critic gate and the freeze gates pass.";
for (const s of specs) {
  if (d.design.n55[s.id]) throw new Error("design.n55." + s.id + " exists");
  const row = {};
  if (s.sim) row.sim = true;
  Object.assign(row, { _why: s.why, enabled: false, enabled_why: off, numbers: {} });
  d.design.n55[s.id] = row;
}
// re-serialise design.n55 only: replace its block in the raw text
const start = raw.indexOf("\"n55\": {");
if (start < 0 || raw.indexOf("\"n55\": {", start + 1) >= 0) throw new Error("design.n55 anchor not found once");
const tail = eol + "  }" + eol + "}";
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
const block = JSON.stringify(d.design.n55, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
const out = raw.slice(0, start) + "\"n55\": " + block + tail;
const back = JSON.parse(out);
for (const s of specs) delete back.design.n55[s.id];
if (JSON.stringify(back) !== JSON.stringify(before)) throw new Error("something beyond the new rows changed");
fs.writeFileSync(file, out); fs.writeFileSync(copy, out);
// the tests' arrays
const tf = root + "Assets/Tests/EditMode/N55SharedTests.cs";
let t = fs.readFileSync(tf, "utf8");
for (const s of specs) {
  const name = s.sim ? "Sims" : "Views";
  const re = new RegExp("static readonly string\\[\\] " + name + " = \\{ ([^}]*) \\};");
  const m = t.match(re);
  if (!m) throw new Error(name + " array not found");
  t = t.replace(re, () => "static readonly string[] " + name + " = { " + m[1].trim() + ", \"" + s.id + "\" };");
}
fs.writeFileSync(tf, t);
for (const s of specs) console.log("added design.n55." + s.id + " (" + (s.sim ? "sim" : "view") + ", " + s.owner + ")");
