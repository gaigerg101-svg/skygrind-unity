// NIGHT30 (the lead): turn design.n30 rows on / off in Design/unity-config.json by text (keeps the file's layout), then run n24-sync.sh.
//   node Tools/night/n30-rows.cjs on rules teach hud30      |   node Tools/night/n30-rows.cjs off look30
// Also takes "n29.<row>" for a NIGHT29 row.
const fs = require("fs");
const file = "C:/Users/gaige/dev/SKYGRIND-unity/Design/unity-config.json";
const [mode, ...rows] = process.argv.slice(2);
if (mode !== "on" && mode !== "off") throw new Error("usage: on|off <row>...");
let s = fs.readFileSync(file, "utf8");
const d = JSON.parse(s);
for (const r0 of rows) {
  const [blk, row] = r0.includes(".") ? r0.split(".") : ["n30", r0];
  const start = s.indexOf('"' + blk + '": {');
  if (start < 0) throw new Error("no block " + blk);
  const at = s.indexOf('\n      "' + row + '": {', start);
  if (at < 0) throw new Error("no row " + blk + "." + row);
  const en = s.indexOf('"enabled": ', at);
  const end = s.indexOf("\n      }", at);
  if (en < 0 || en > end) throw new Error("no enabled in " + blk + "." + row);
  const valEnd = s.indexOf(en + 11 < s.length && s[en + 11] === "t" ? "true" : "false", en) + (s[en + 11] === "t" ? 4 : 5);
  s = s.slice(0, en) + '"enabled": ' + (mode === "on" ? "true" : "false") + s.slice(valEnd);
  console.log(blk + "." + row + " -> " + mode);
}
JSON.parse(s);
fs.writeFileSync(file, s);
