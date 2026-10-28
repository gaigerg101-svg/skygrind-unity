// NIGHT38 lead (integration): turns design.n38.<row>.enabled off in Design/unity-config.json and copies the file to
// Assets/StreamingAssets/Design/ (the Player's copy). The lead holds a level row off at integration until its gate proof is complete
// (NIGHT38-LOG.md); a builder's branch may leave the row on. Changes only that one flag's text; every other byte stays as it is.
// Run: node Tools/night/n38-row-off.cjs <row>      e.g. node Tools/night/n38-row-off.cjs kiln38
const fs = require("fs");
const row = process.argv[2];
if (!row || !/^[a-z0-9]+$/i.test(row)) throw new Error("usage: n38-row-off.cjs <row>");
const file = "C:/Users/gaige/dev/SKYGRIND-unity/Design/unity-config.json";
const copy = "C:/Users/gaige/dev/SKYGRIND-unity/Assets/StreamingAssets/Design/unity-config.json";
let s = fs.readFileSync(file, "utf8");
const re = new RegExp('("' + row + '": \\{[^}]*?"enabled": )true');
const hits = s.match(new RegExp(re.source, "g")) || [];
if (hits.length !== 1) throw new Error("expected one " + row + " enabled:true, found " + hits.length);
s = s.replace(re, "$1false");
JSON.parse(s);
fs.writeFileSync(file, s);
fs.copyFileSync(file, copy);
const d = JSON.parse(s);
console.log("design.n38." + row + ".enabled = " + d.design.n38[row].enabled + "; copy identical: " + (fs.readFileSync(copy, "utf8") === s));
