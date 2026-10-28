// NIGHT37 (the lead): turn design.n37 rows on or off in Design/unity-config.json by a text edit of each row's own `"enabled": <bool>` line
// (the first one after the row's key inside design.n37), so nothing else in the file moves. Copy to StreamingAssets with n24-sync.sh.
//   node Tools/night/n37-rows.cjs on wick37 boss37 ...      node Tools/night/n37-rows.cjs off crown37      node Tools/night/n37-rows.cjs split on|off
const fs = require("fs");
const file = "C:/Users/gaige/dev/SKYGRIND-unity/Design/unity-config.json";
const [mode, ...rows] = process.argv.slice(2);
let s = fs.readFileSync(file, "utf8");
const at = s.indexOf('"n37": {');
if (at < 0) throw new Error("design.n37 not found");
function setKey(row, key, val) {
  const r = s.indexOf('"' + row + '": {', at);
  if (r < 0) throw new Error("row " + row + " not found in design.n37");
  const re = new RegExp('"' + key + '": (true|false)', "g");
  re.lastIndex = r;
  const m = re.exec(s);
  if (!m) throw new Error(row + "." + key + " not found");
  const close = s.indexOf("\n      }", r); // the row's closing brace (6-space indent inside design.n37)
  if (close >= 0 && m.index > close) throw new Error(row + "." + key + " is not inside the row");
  s = s.slice(0, m.index) + '"' + key + '": ' + val + s.slice(m.index + m[0].length);
  console.log(row + "." + key + " = " + val);
}
if (mode === "split") setKey("bend34", "split", rows[0] === "on");
else if (mode === "on" || mode === "off") for (const row of rows) setKey(row, "enabled", mode === "on");
else throw new Error("usage: on|off <rows...> | split on|off");
fs.writeFileSync(file, s);
JSON.parse(fs.readFileSync(file, "utf8"));
