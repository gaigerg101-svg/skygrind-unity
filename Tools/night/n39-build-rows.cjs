// NIGHT39 lead (C1 row 3, frames): turns design rows on or off in a BUILD's runtime config copy only
// (Builds/<tag>/SKYGRIND_Data/StreamingAssets/Design/unity-config.json), so a frames reel can show view rows that are OFF in the repo
// without a new build. The repo's Design/unity-config.json and its StreamingAssets copy are never touched.
// Run: node Tools/night/n39-build-rows.cjs <tag> on|off <night.row> [<night.row> ...]   e.g. ... n38-r1 on n38.doors38 n38.hud38
const fs = require("fs");
const [tag, mode, ...rows] = process.argv.slice(2);
if (!tag || !/^(on|off)$/.test(mode || "") || rows.length === 0) throw new Error("usage: n39-build-rows.cjs <tag> on|off <night.row> ...");
const file = "C:/Users/gaige/dev/SKYGRIND-unity/Builds/" + tag + "/SKYGRIND_Data/StreamingAssets/Design/unity-config.json";
let s = fs.readFileSync(file, "utf8");
for (const nr of rows) {
  const [night, row] = nr.split(".");
  if (!/^n\d\d$/.test(night) || !/^[a-z0-9]+$/i.test(row || "")) throw new Error("bad row " + nr);
  const from = mode === "on" ? "false" : "true", to = mode === "on" ? "true" : "false";
  const re = new RegExp('("' + row + '": \\{[^}]*?"enabled": )' + from);
  const hits = s.match(new RegExp(re.source, "g")) || [];
  if (hits.length !== 1) throw new Error(nr + ": expected one enabled:" + from + ", found " + hits.length);
  s = s.replace(re, "$1" + to);
  const d = JSON.parse(s);
  console.log(tag + ": design." + nr + ".enabled = " + d.design[night][row].enabled);
}
fs.writeFileSync(file, s);
