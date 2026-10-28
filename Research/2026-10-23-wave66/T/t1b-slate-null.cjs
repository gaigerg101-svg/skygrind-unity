// NIGHT66 T1b fix: the planted slate for n66-gate-pack.cjs's null-mainBar refusal: a copy of the main tree's slate (read only) with one
// room's mainBar set to null, written to the path given (a scratch folder).
//   node Research/2026-10-23-wave66/T/t1b-slate-null.cjs <room> <out.json>
const fs = require("fs");
const [room, out] = process.argv.slice(2);
if (!room || !out) { console.error("usage: t1b-slate-null.cjs <room> <out.json>"); process.exit(2); }
const s = JSON.parse(fs.readFileSync("C:/Users/gaige/dev/SKYGRIND-unity/Logs/n66/slate.json", "utf8"));
const sl = s.slices.find((x) => x.id === room);
if (!sl) { console.error("no slice " + room); process.exit(1); }
console.log("planted: " + room + ".mainBar " + JSON.stringify(sl.mainBar) + " -> null, in " + out);
sl.mainBar = null;
fs.writeFileSync(out, JSON.stringify(s, null, 1) + "\n");
