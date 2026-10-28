// NIGHT46 builder F1: adds design.n46.overlap46.numbers.entryM (+ its _why) after fromWave_why, as text into Design/unity-config.json, then
// the StreamingAssets copy as a byte copy. Refuses if entryM exists or fromWave_why is missing. The row stays enabled: false.
//   node Tools/night/n46-f1-overlap-entry.cjs [--dry]
const fs = require("fs");
const path = require("path");
const ROOT = path.resolve(__dirname, "..", "..");
const A = path.join(ROOT, "Design", "unity-config.json");
const B = path.join(ROOT, "Assets", "StreamingAssets", "Design", "unity-config.json");
const dry = process.argv.includes("--dry");
// --growth: the second key (growthMax, after entryM_why), added once entryM is in
if (process.argv.includes("--growth")) {
  const GW = "The lead's round-2 fix, the general half (NIGHT46): a wave more than 2 x the size of the wave before it is the room's next beat and is never released early: it waits for the full clear. n46flow --trace on n45-iceline (overlap46 on, entryM in): g7's 8 deaths in 10 walks came with wave 2's 7 bodies released on top of wave 1's last of 3 (swarmers + the spitters on the gantries), not with wave 3. In levels 1-5 it holds g7 wave 2 (3 -> 7) and g3 wave 2 (3 -> 8); every other wave keeps its overlap (g5 5 -> 8, p1 3 -> 6, p3 4 / 5 / 6, p5 6 / 5 / 6, kb2 6 / 5 / 3; FLOW-AUDIT.md's wave counts). A 1-body wave before it never overlaps anyway (shareLeft).";
  let t = fs.readFileSync(A, "utf8");
  const e2 = t.includes("\r\n") ? "\r\n" : "\n";
  const c2 = JSON.parse(t).design.n46.overlap46.numbers;
  if (c2.entryM === undefined || c2.growthMax !== undefined) throw new Error("needs entryM and no growthMax");
  const o = t.indexOf('"overlap46": {', t.indexOf('"n46": {'));
  const ew = t.indexOf('"entryM_why": ', o);
  const le = t.indexOf(e2, ew);
  const ind = t.slice(t.lastIndexOf("\n", ew) + 1, ew);
  t = t.slice(0, le) + "," + e2 + ind + '"growthMax": 2,' + e2 + ind + '"growthMax_why": ' + JSON.stringify(GW) + t.slice(le);
  const b2 = JSON.parse(t).design.n46.overlap46;
  if (b2.numbers.growthMax !== 2 || b2.numbers.growthMax_why !== GW || b2.enabled !== false) throw new Error("growthMax did not round-trip");
  if (dry) { console.log("n46-f1-overlap-entry --growth: planned (dry run)"); process.exit(0); }
  fs.writeFileSync(A, t);
  fs.writeFileSync(B, t);
  console.log("n46-f1-overlap-entry: design.n46.overlap46.numbers.growthMax <- 2 (both config copies)");
  process.exit(0);
}
const WHY = "The lead's round-2 fix (NIGHT46): a wave whose OWN nodes (waveNodes43) put one within 12 m of the room's spawn (the entry the player came in by) is never released early: it waits for the room's full clear. The rule finds exactly one wave in levels 1-5: g7 THE THAW LOCK's wave 3 (nodes 11.2-14.8 m from the entry, its card's state change: the lock's keepers come in behind you); overlap46 stacked it on wave 2 and g7's deaths went 0 -> 8 in 10 walks (n46flow, Research/2026-10-17-wave46/F1.md). Rooms whose waves use the room's shared nodes are not judged (the spawner already keeps every body minSpawnDist 12 m from the player). 12 m = design.n24.campaign.minSpawnDist.";
let text = fs.readFileSync(A, "utf8");
const eol = text.includes("\r\n") ? "\r\n" : "\n";
const cfg = JSON.parse(text);
const nums = cfg.design.n46.overlap46.numbers;
if (nums.entryM !== undefined) throw new Error("overlap46.numbers.entryM already exists (already run?)");
const at = text.indexOf('"overlap46": {', text.indexOf('"n46": {'));
const fw = text.indexOf('"fromWave_why": ', at);
if (at < 0 || fw < 0) throw new Error("overlap46.numbers.fromWave_why not found");
const lineEnd = text.indexOf(eol, fw);
const indent = text.slice(text.lastIndexOf("\n", fw) + 1, fw);
const add = "," + eol + indent + '"entryM": 12,' + eol + indent + '"entryM_why": ' + JSON.stringify(WHY);
text = text.slice(0, lineEnd) + add + text.slice(lineEnd);
const back = JSON.parse(text).design.n46.overlap46;
if (back.numbers.entryM !== 12 || back.numbers.entryM_why !== WHY || back.enabled !== false) throw new Error("did not round-trip");
if (dry) { console.log("n46-f1-overlap-entry: planned (dry run)"); process.exit(0); }
fs.writeFileSync(A, text);
fs.writeFileSync(B, text);
console.log("n46-f1-overlap-entry: design.n46.overlap46.numbers.entryM <- 12 (both config copies)");
