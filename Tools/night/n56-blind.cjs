// NIGHT56 (the lead; from n46-blind-pairs.cjs, which reads feel46's file names only): blind pairs for ANY on / off frame pair, shown in
// BOTH orders (NIGHT56.md section 6). For each pair k: <outDir>/pair-<k>-1A / -1B (order 1: a seeded coin picks which side is A) and
// pair-<k>-2A / -2B (order 2: the same two frames swapped). The key (which file is ON) goes to <keyFile>, outside <outDir>.
//   node Tools/night/n56-blind.cjs <outDir> <keyFile> <seed> <label>=<onPath>,<offPath> [<label>=<onPath>,<offPath> ...]
const fs = require("fs");
const path = require("path");
const [out, keyFile, seedS, ...specs] = process.argv.slice(2);
if (!out || !keyFile || !specs.length) throw new Error("usage: n56-blind.cjs <outDir> <keyFile> <seed> <label>=<on>,<off> ...");
let s = (parseInt(seedS, 10) >>> 0) || 1;
function rnd() { s = (s + 0x6d2b79f5) >>> 0; let t = s; t = Math.imul(t ^ (t >>> 15), t | 1); t ^= t + Math.imul(t ^ (t >>> 7), t | 61); return ((t ^ (t >>> 14)) >>> 0) / 4294967296; }
if (path.resolve(path.dirname(keyFile)) === path.resolve(out)) throw new Error("the key must live outside the pairs' folder");
fs.mkdirSync(out, { recursive: true });
const key = [];
specs.forEach((sp, i) => {
  const eq = sp.indexOf("=");
  const label = sp.slice(0, eq), [on, off] = sp.slice(eq + 1).split(",");
  for (const f of [on, off]) if (!fs.existsSync(f)) throw new Error("missing " + f);
  const ext = path.extname(on);
  const onIsA = rnd() < 0.5, k = i + 1;
  fs.copyFileSync(onIsA ? on : off, path.join(out, "pair-" + k + "-1A" + ext));
  fs.copyFileSync(onIsA ? off : on, path.join(out, "pair-" + k + "-1B" + ext));
  fs.copyFileSync(onIsA ? off : on, path.join(out, "pair-" + k + "-2A" + ext));
  fs.copyFileSync(onIsA ? on : off, path.join(out, "pair-" + k + "-2B" + ext));
  key.push({ pair: k, label, on1: onIsA ? "A" : "B", on2: onIsA ? "B" : "A", onPath: on, offPath: off });
});
fs.writeFileSync(keyFile, JSON.stringify(key, null, 1));
console.log("wrote " + specs.length + " pairs (both orders) to " + out + "; key in " + keyFile);
