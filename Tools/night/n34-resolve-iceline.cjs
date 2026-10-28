// NIGHT34 (the lead): resolve the f5fdfc34 cherry-pick conflict in Design/unity-config.json: keep HEAD's `"enabled": true` (lines34 is
// ON since round 2) and take A's levelSwap (adds n33-iceline -> n34-iceline). Writes the Design file only; StreamingAssets is re-copied
// by n24-sync.sh afterwards. Asserts exactly one conflict hunk.
const fs = require("fs");
const file = "C:/Users/gaige/dev/SKYGRIND-unity/Design/unity-config.json";
let s = fs.readFileSync(file, "utf8");
const re = /<<<<<<< HEAD\r?\n([\s\S]*?)=======\r?\n([\s\S]*?)>>>>>>> [^\n]*\n/g;
const hunks = [...s.matchAll(re)];
if (hunks.length !== 1) throw new Error("expected 1 conflict hunk, found " + hunks.length);
const eol = s.includes("\r\n") ? "\r\n" : "\n";
const theirs = hunks[0][2].split(/\r?\n/).find((l) => l.includes('"levelSwap"'));
if (!theirs || !theirs.includes("n34-iceline")) throw new Error("A's levelSwap line not found");
const merged = '        "enabled": true,' + eol + theirs + eol;
s = s.replace(re, merged);
fs.writeFileSync(file, s);
JSON.parse(fs.readFileSync(file, "utf8"));
console.log("resolved: lines34 enabled true, " + theirs.trim());
