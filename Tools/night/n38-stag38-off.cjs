// NIGHT38 lead (integration, 2026-10-13 ~11:50): turns design.n38.stag38.enabled off in Design/unity-config.json and copies the file to
// Assets/StreamingAssets/Design/ (the Player's copy). Builder S2 left stag38 on in its branch; the lead holds rows off until the frames
// are looked at (NIGHT38-LOG.md). Changes only that one flag's text; every other byte stays as it is.
// Run once: node Tools/night/n38-stag38-off.cjs
const fs = require("fs");
const file = "C:/Users/gaige/dev/SKYGRIND-unity/Design/unity-config.json";
const copy = "C:/Users/gaige/dev/SKYGRIND-unity/Assets/StreamingAssets/Design/unity-config.json";
let s = fs.readFileSync(file, "utf8");
const re = /("stag38": \{[^}]*?"enabled": )true/;
const hits = s.match(new RegExp(re.source, "g")) || [];
if (hits.length !== 1) throw new Error("expected one stag38 enabled:true, found " + hits.length);
s = s.replace(re, "$1false");
JSON.parse(s);
fs.writeFileSync(file, s);
fs.copyFileSync(file, copy);
const d = JSON.parse(s);
console.log("design.n38.stag38.enabled = " + d.design.n38.stag38.enabled + "; copy identical: " + (fs.readFileSync(copy, "utf8") === s));
