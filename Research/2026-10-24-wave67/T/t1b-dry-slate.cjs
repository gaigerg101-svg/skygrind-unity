// NIGHT67 builder T, T1b's dry rounds: a copy of the main tree's Logs/n67/slate.json with planted verbs and bars on the dry rooms (the main
// slate's room verbs are null until the cards' critic), written to Research/2026-10-24-wave67/T/t1b-dry-slate.json. The dry round reads it
// through SLATE=<file> (n67-round.sh copies it into the round folder as slate-at-start.json). Not the night's slate: never copied to Logs/n67.
//   node Research/2026-10-24-wave67/T/t1b-dry-slate.cjs [room:verb:bar ...]   default g8:ride:1.25 kb3:ride:1.25 v6:ride:1.25
const fs = require("fs");
const path = require("path");
const src = "C:/Users/gaige/dev/SKYGRIND-unity/Logs/n67/slate.json";
const out = path.join(__dirname, "t1b-dry-slate.json");
const s = JSON.parse(fs.readFileSync(src, "utf8"));
const plants = process.argv.slice(2).length ? process.argv.slice(2) : ["g8:ride:1.25", "kb3:ride:1.25", "v6:ride:1.25"];
for (const p of plants) {
  const [id, verb, bar] = p.split(":");
  const x = s.slices.find((q) => q.id === id);
  if (!x) throw new Error("no slice " + id + " in " + src);
  x.verb = verb; x.mainBar = Number(bar);
}
s._dry = "NIGHT67 T1b dry slate (builder T): " + src + " with planted " + plants.join(", ") + "; not the night's slate";
fs.writeFileSync(out, JSON.stringify(s, null, 1) + "\n");
console.log("t1b-dry-slate: " + out + " (" + plants.join(", ") + ")");
