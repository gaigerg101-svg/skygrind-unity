// NIGHT34 (the lead): resolve conflict hunks in a JSON file by keeping HEAD's side of each (the builders' config scripts reformat
// rows another builder owns; HEAD holds the integrated values). Prints each hunk's two sides; the result must parse as JSON.
//   node Tools/night/n34-resolve-ours.cjs <file> <expectedHunks>
const fs = require("fs");
const [file, want] = process.argv.slice(2);
let s = fs.readFileSync(file, "utf8");
const re = /<<<<<<< HEAD\r?\n([\s\S]*?)=======\r?\n([\s\S]*?)>>>>>>> [^\n]*\n/g;
const hunks = [...s.matchAll(re)];
if (hunks.length !== Number(want)) throw new Error("expected " + want + " hunks, found " + hunks.length);
for (const h of hunks) console.log("keep HEAD: " + JSON.stringify(h[1].trim()) + "  drop theirs: " + JSON.stringify(h[2].trim()));
s = s.replace(re, (m, ours) => ours);
fs.writeFileSync(file, s);
JSON.parse(fs.readFileSync(file, "utf8"));
console.log("resolved " + file);
