// NIGHT52 (the lead's only): sets one row's passes / state / evidence_paths / note in Tools/night/n52-rows.json, after the lead printed its
// check. Never touches `check`, `title`, `owner`, `switch` or `fallback`.
//   node Tools/night/n52-flip.cjs <id> <true|false> "<state>" "<evidence path; ...>" ["<note>"]
const fs = require("fs");
const path = require("path");
const file = path.join(__dirname, "n52-rows.json");
const [id, passes, state, evidence, note] = process.argv.slice(2);
if (!id || !/^(true|false)$/.test(passes || "") || !state) throw new Error("usage: n52-flip.cjs <id> <true|false> <state> <evidence> [note]");
const d = JSON.parse(fs.readFileSync(file, "utf8"));
const row = d.rows.find((r) => r.id === id);
if (!row) throw new Error("no row " + id);
const keep = JSON.stringify([row.check, row.title, row.owner, row.switch, row.fallback]);
row.passes = passes === "true";
row.state = state;
row.evidence_paths = (evidence || "").split(";").map((s) => s.trim()).filter(Boolean);
if (note !== undefined) row.note = note;
if (JSON.stringify([row.check, row.title, row.owner, row.switch, row.fallback]) !== keep) throw new Error("a fixed field changed");
fs.writeFileSync(file, JSON.stringify(d, null, 2) + "\n");
console.log(id + ": passes " + row.passes + " | " + state);
