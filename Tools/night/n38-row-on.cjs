// NIGHT38 lead (integration): turns design.n38.<row>.enabled on in Design/unity-config.json and copies the file to
// Assets/StreamingAssets/Design/ (the Player's copy). The lead turns a level row on only when its gates are GREEN on the checker AND its
// walk clears every room (NIGHT38-LOG.md). Changes only that one flag's text; every other byte stays as it is.
// Run: node Tools/night/n38-row-on.cjs <row>      e.g. node Tools/night/n38-row-on.cjs spire38
const fs = require("fs");
const row = process.argv[2];
if (!row || !/^[a-z0-9]+$/i.test(row)) throw new Error("usage: n38-row-on.cjs <row>");
const file = "C:/Users/gaige/dev/SKYGRIND-unity/Design/unity-config.json";
const copy = "C:/Users/gaige/dev/SKYGRIND-unity/Assets/StreamingAssets/Design/unity-config.json";
let s = fs.readFileSync(file, "utf8");
const re = new RegExp('("' + row + '": \\{[^}]*?"enabled": )false');
const hits = s.match(new RegExp(re.source, "g")) || [];
if (hits.length !== 1) throw new Error("expected one " + row + " enabled:false, found " + hits.length);
s = s.replace(re, "$1true");
JSON.parse(s);
fs.writeFileSync(file, s);
fs.copyFileSync(file, copy);
const d = JSON.parse(s);
const night = Object.keys(d.design).find((k) => d.design[k] && d.design[k][row] && typeof d.design[k][row].enabled === "boolean"); // NIGHT39: the row may be in design.n38 or design.n39
console.log("design." + night + "." + row + ".enabled = " + d.design[night][row].enabled + "; copy identical: " + (fs.readFileSync(copy, "utf8") === s));
