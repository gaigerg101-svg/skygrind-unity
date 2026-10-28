// NIGHT66 T1c fix: a copy of a room file whose first cyan light carries `marks` (test d's named target), for the check that the assembler and
// levelgen accept the key (gen-n65-foundry.cjs --room-file ... --out).   node Research/2026-10-23-wave66/T/t1c-marks-file.cjs <room> <target> <out>
const fs = require("fs");
const path = require("path");
const [room, target, out] = process.argv.slice(2);
const f = JSON.parse(fs.readFileSync(path.join(__dirname, "../../../Design/levels/n65/rooms", room + ".json"), "utf8"));
const l = (f.lights || []).find((x) => String(x.color || "").toLowerCase() === "cyan");
if (!l) { console.log("no cyan light in " + room); process.exit(1); }
l.marks = target;
fs.writeFileSync(out, JSON.stringify(f, null, 1) + "\n");
console.log("t1c-marks-file: " + room + "'s light " + l.id + " marks '" + target + "' -> " + out);
