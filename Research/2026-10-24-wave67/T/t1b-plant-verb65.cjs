// NIGHT67 builder T, T1b's planted MISMATCH: sets (or with --undo removes) "verb65": "<verb>" in a room file's row, in T's tree only, so the
// next round's level carries a verb65 that differs from the slate's verb. The chain restores the file with `git checkout --` right after.
//   node Research/2026-10-24-wave67/T/t1b-plant-verb65.cjs <room file> <verb>
const fs = require("fs");
const [file, verb] = process.argv.slice(2);
const f = JSON.parse(fs.readFileSync(file, "utf8"));
f.row.verb65 = verb;
fs.writeFileSync(file, JSON.stringify(f, null, 1) + "\n");
console.log("t1b-plant-verb65: " + file + " row.verb65 = " + verb + " (PLANTED; restore with git checkout -- " + file + ")");
