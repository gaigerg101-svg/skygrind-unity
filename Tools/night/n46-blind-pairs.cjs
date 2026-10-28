// NIGHT46 (the lead; DECIDE46 N-4): blind pairs. Copies each feel46 off / on frame pair into <outDir> as pair-<k>-A.png / pair-<k>-B.png
// with the side picked by a seeded coin, and writes the key to <keyFile> (outside <outDir>), so a judge sees only A / B.
// Run: node Tools/night/n46-blind-pairs.cjs <frameDir> <outDir> <keyFile> <seed> <moment> [<moment> ...]
const fs = require("fs");
const path = require("path");
const [dir, out, keyFile, seedS, ...moments] = process.argv.slice(2);
if (!dir || !out || !keyFile || !moments.length) throw new Error("usage: n46-blind-pairs.cjs <frameDir> <outDir> <keyFile> <seed> <moment>...");
let s = (parseInt(seedS, 10) >>> 0) || 1;
function rnd() { s = (s + 0x6d2b79f5) >>> 0; let t = s; t = Math.imul(t ^ (t >>> 15), t | 1); t ^= t + Math.imul(t ^ (t >>> 7), t | 61); return ((t ^ (t >>> 14)) >>> 0) / 4294967296; }
fs.mkdirSync(out, { recursive: true });
const key = [];
moments.forEach((m, i) => {
  const off = path.join(dir, "shot-N46-V46-off-" + m + ".png"), on = path.join(dir, "shot-N46-V46-on-" + m + ".png");
  const onIsA = rnd() < 0.5;
  fs.copyFileSync(onIsA ? on : off, path.join(out, "pair-" + (i + 1) + "-A.png"));
  fs.copyFileSync(onIsA ? off : on, path.join(out, "pair-" + (i + 1) + "-B.png"));
  key.push({ pair: i + 1, moment: m, on: onIsA ? "A" : "B" });
});
fs.writeFileSync(keyFile, JSON.stringify(key, null, 1));
console.log("wrote " + moments.length + " pairs to " + out + "; key in " + keyFile);
