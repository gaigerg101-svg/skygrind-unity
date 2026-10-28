// NIGHT33 (the lead): set design.n33.<row>.enabled for the given rows, in Design/unity-config.json and its StreamingAssets copy (in place,
// keeping the file's layout: only the `"enabled": x` line inside each row's object changes).
// Run: node Tools/night/n33-rows.cjs on hud33 look33 ...   |   node Tools/night/n33-rows.cjs off pierce33
const fs = require("fs");
const root = "C:/Users/gaige/dev/SKYGRIND-unity/";
const [mode, ...rows] = process.argv.slice(2);
if (mode !== "on" && mode !== "off") throw new Error("usage: n33-rows.cjs on|off <row...>");
const files = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];
for (const f of files) {
  let s = fs.readFileSync(root + f, "utf8");
  const n33 = s.indexOf('"n33": {');
  if (n33 < 0) throw new Error(f + ": no design.n33");
  for (const row of rows) {
    const at = s.indexOf('"' + row + '": {', n33);
    if (at < 0) throw new Error(f + ": no design.n33." + row);
    const re = /"enabled": (true|false)/g;
    re.lastIndex = at;
    const m = re.exec(s);
    if (!m) throw new Error(f + ": " + row + " has no enabled");
    s = s.slice(0, m.index) + '"enabled": ' + (mode === "on") + s.slice(m.index + m[0].length);
  }
  const d = JSON.parse(s);
  for (const row of rows) if (d.design.n33[row].enabled !== (mode === "on")) throw new Error(f + ": " + row + " did not change (an enabled inside a nested object came first?)");
  fs.writeFileSync(root + f, s);
  console.log(f + ": " + mode + " " + rows.join(", "));
}
