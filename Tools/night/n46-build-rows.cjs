// NIGHT46 (the lead; from n39-build-rows.cjs): turns design rows on or off in a BUILD's runtime config copy only
// (Builds/<tag>/SKYGRIND_Data/StreamingAssets/Design/unity-config.json). n39's regex stops at the first "}" so it cannot reach "enabled"
// after a nested object (feel46's "numbers"; measured 01:28: it threw on n46.feel46 and wrote NOTHING, so build r3's copy kept every row
// off). This one edits the parsed JSON and refuses an unknown row; the repo's configs are never touched.
// Run: node Tools/night/n46-build-rows.cjs <tag> on|off <night.row> [<night.row> ...]
const fs = require("fs");
const [tag, mode, ...rows] = process.argv.slice(2);
if (!tag || !/^(on|off)$/.test(mode || "") || rows.length === 0) throw new Error("usage: n46-build-rows.cjs <tag> on|off <night.row> ...");
const file = "C:/Users/gaige/dev/SKYGRIND-unity/Builds/" + tag + "/SKYGRIND_Data/StreamingAssets/Design/unity-config.json";
const d = JSON.parse(fs.readFileSync(file, "utf8"));
for (const nr of rows) {
  const [night, row] = nr.split(".");
  const r = d.design[night] && d.design[night][row];
  if (!r || typeof r.enabled !== "boolean") throw new Error("no boolean design." + nr + ".enabled");
  r.enabled = mode === "on";
}
fs.writeFileSync(file, JSON.stringify(d, null, 2));
const back = JSON.parse(fs.readFileSync(file, "utf8"));
for (const nr of rows) { const [n, r] = nr.split("."); console.log(tag + ": design." + nr + ".enabled = " + back.design[n][r].enabled); }
