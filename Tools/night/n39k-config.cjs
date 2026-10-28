// NIGHT39 builder K4 (the kiln38 row): writes ONLY design.n38.kiln38's `enabled`, `_why` and `gates_why` in Design/unity-config.json (text edits
// inside the kiln38 block, the file's layout kept; its levelFrom / levelTo are the lead's and stay), then copies the file to
// Assets/StreamingAssets/Design/unity-config.json (byte-identical). Idempotent. Run: node Tools/night/n39k-config.cjs [--off]
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const CFG = path.join(ROOT, "Design/unity-config.json");
const SA = path.join(ROOT, "Assets/StreamingAssets/Design/unity-config.json");
const ON = !process.argv.includes("--off");

const WHY = "Builder K4 (NIGHT39, GG 2026-10-13: level 3 'completely overhauled ... encourage far more mechanics, have a new arena, and keep the worm AI the same'; NIGHT38 K3's first rebuild had its gates RED 0/4): level 3 rebuilt around the THUNDERCLAP CLIMB (jump, shoot down, double, shoot down): every gate a noride cliff 9.5 m over its take-off in a flight room, open roofed arenas with the boost through the pack, rides over the pours; THE KILN WYRM's AI and paths untouched, its room the level's own (k5 wyrmArena: Design/maps/wyrm39-arena.json, read once the lead's hook Tools/night/n39k-wyrm-hook.cjs is in). Flow only: a levelFrom -> levelTo swap in Curriculum25.N38Levels while on. ";
const GATES = "Turned on by builder K4 when both bars held (NIGHT38's): builder E's checker passed every gate of Design/levels/n38/kiln.plan.json (n38 gates GREEN 4/4: G1 THE FLUE +49 %, G2a / G2b THE STACK +69 % / +55 %, G3 THE SPOUT +49 %; the best other try of each: none) AND the walk bot cleared every playable room 10/10 (n38kiln walk: level median 197.4 s). K4's own probe of the blast + wall-jump family the checker does not fly (n38kiln blastwall, 864 tries per gate): 0 land, the feet reach 11.03 m 3 m out from the face. The level: Tools/night/gen-n38-kiln.cjs, bash Tools/night/n38k-regen.sh.";

let s = fs.readFileSync(CFG, "utf8");
const nl = s.includes("\r\n") ? "\r\n" : "\n";
const at = s.indexOf('"kiln38": {');
if (at < 0) throw new Error("no design.n38.kiln38 block");
const end = s.indexOf("}", at);
let block = s.slice(at, end);
block = block.replace(/"_why": "[^"]*"/, '"_why": ' + JSON.stringify(WHY));
block = block.replace(/"enabled": (true|false)/, `"enabled": ${ON}`);
block = block.replace(/,\r?\n\s*"gates_why": "[^"]*"/, "");
if (ON) {
  const key = '"levelTo": "n38-kiln"';
  const k = block.indexOf(key);
  if (k < 0) throw new Error("kiln38 has no levelTo n38-kiln");
  const ls = block.lastIndexOf("\n", k) + 1;
  const ind = block.slice(ls, k).match(/^\s*/)[0];
  block = block.slice(0, k + key.length) + "," + nl + ind + '"gates_why": ' + JSON.stringify(GATES) + block.slice(k + key.length);
}
s = s.slice(0, at) + block + s.slice(end);
const c = JSON.parse(s); // still JSON
fs.writeFileSync(CFG, s);
fs.writeFileSync(SA, s);
const same = fs.readFileSync(CFG).equals(fs.readFileSync(SA));
console.log(`kiln38.enabled ${c.design.n38.kiln38.enabled}; levelFrom ${c.design.n38.kiln38.levelFrom} -> ${c.design.n38.kiln38.levelTo}; copies ${same ? "identical" : "DIFFER"}`);
if (!same) process.exit(1);
