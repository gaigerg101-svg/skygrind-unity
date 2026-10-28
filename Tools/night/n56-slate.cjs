// NIGHT56 (the lead; from n55-slate.cjs): edits Logs/n56/slate.json. Only the lead runs it. Entries are never removed; their `check` (doneWhen) is
// never rewritten after a builder started on it.
//   node Tools/night/n55-slate.cjs add '<json entry>'
//   node Tools/night/n55-slate.cjs set <id> <field> '<json value>'      (state, notes, passes, owner, red)
//   node Tools/night/n55-slate.cjs list
const fs = require("fs");
const file = "C:/Users/gaige/dev/SKYGRIND-unity/Logs/n56/slate.json";
const d = JSON.parse(fs.readFileSync(file, "utf8"));
const [cmd, a, b, c] = process.argv.slice(2);
const find = (id) => { const e = d.entries.find((x) => x.id === id); if (!e) throw new Error("no entry " + id); return e; };
if (cmd === "add") {
  const e = JSON.parse(a);
  if (d.entries.some((x) => x.id === e.id)) throw new Error("entry exists: " + e.id);
  for (const k of ["id", "title", "source", "owner", "row", "red", "doneWhen"]) if (!e[k]) throw new Error("missing " + k);
  d.entries.push(Object.assign({ passes: false, state: "open", notes: "" }, e));
} else if (cmd === "set") {
  const e = find(a);
  if (b === "doneWhen" || b === "id") throw new Error("doneWhen / id are never rewritten");
  e[b] = JSON.parse(c);
} else if (cmd !== "list") throw new Error("usage: add | set | list");
if (cmd !== "list") fs.writeFileSync(file, JSON.stringify(d, null, 2) + "\n");
for (const e of d.entries) console.log([e.id, e.owner, e.state, "passes " + e.passes].join(" | "));
