// NIGHT72 builder N (slice N5): re-records FingerprintBaseline.Ultra71Hashes (Assets/Scripts/Game/FingerprintBaseline.cs) from the Player's -skygrindFingerprint lines, but only when TWO runs agree
// (NIGHT72.md section 5 step 7: the lead runs Builds/<tag>/SKYGRIND.exe -skygrindFingerprint -logFile ...-fp-1.log and -fp-2.log; equal hashes come to N).
//   node Tools/night/n72-fp-update.cjs <fp-1 file> <fp-2 file>      the files hold "[fingerprint] seed 1 step N hash H" lines (a Player log, or those twelve lines pasted)
//   node Tools/night/n72-fp-update.cjs --check <file>               parse one file and print the twelve checkpoints (no write)
// Refuses (nothing written) unless: each file has exactly the twelve seed-1 checkpoints, steps 600 .. 7200 in 600s, and the two files' checkpoints are identical. Writes only the array's twelve lines.
"use strict";
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const TARGET = path.join(ROOT, "Assets/Scripts/Game/FingerprintBaseline.cs");
process.on("uncaughtException", (e) => { console.error("n72-fp-update: REFUSED: " + e.message); process.exit(1); }); // one line, nothing written

function parse(file) {
  const out = [];
  for (const line of fs.readFileSync(file, "utf8").split(/\r?\n/)) {
    const m = /\bseed 1 step (\d+) hash ([0-9a-f]{16})\b/.exec(line);
    if (m) out.push({ step: Number(m[1]), hash: m[2] });
  }
  return out;
}
function check(name, rows) {
  if (rows.length !== 12) throw new Error(name + ": " + rows.length + " seed-1 checkpoints, expected 12");
  rows.forEach((r, i) => { if (r.step !== 600 * (i + 1)) throw new Error(name + ": checkpoint " + i + " is step " + r.step + ", expected " + 600 * (i + 1)); });
}

const args = process.argv.slice(2);
if (args[0] === "--check") {
  const rows = parse(args[1]);
  check(args[1], rows);
  for (const r of rows) console.log("step " + r.step + " hash " + r.hash);
  process.exit(0);
}
if (args.length !== 2) { console.error("usage: n72-fp-update.cjs <fp-1> <fp-2> | --check <file>"); process.exit(2); }
const a = parse(args[0]), b = parse(args[1]);
check(args[0], a);
check(args[1], b);
for (let i = 0; i < 12; i++) if (a[i].hash !== b[i].hash) throw new Error("the two runs differ at step " + a[i].step + ": " + a[i].hash + " vs " + b[i].hash + " (not recorded)");
const text = fs.readFileSync(TARGET, "utf8");
const eol = text.includes("\r\n") ? "\r\n" : "\n";
const re = /(public static readonly string\[\] Ultra71Hashes =\s*\{)([\s\S]*?)(\};)/;
const m = re.exec(text);
if (!m) throw new Error("Ultra71Hashes not found in FingerprintBaseline.cs");
const body = eol + a.map((r) => '            "step ' + r.step + " hash " + r.hash + '",').join(eol) + eol + "        ";
const next = text.replace(re, (_, h, _b, t) => h + body + t);
if (next === text) { console.log("Ultra71Hashes already equal to these two runs: nothing to do"); process.exit(0); }
fs.writeFileSync(TARGET, next);
console.log("Ultra71Hashes re-recorded from two equal runs (" + path.basename(args[0]) + ", " + path.basename(args[1]) + "):");
for (const r of a) console.log("  step " + r.step + " hash " + r.hash);
