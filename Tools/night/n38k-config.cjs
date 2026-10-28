// NIGHT38 builder K3 (kiln38): writes ONLY builder K3's rows in Design/unity-config.json (text edits, the file's layout kept), then copies the file to
// Assets/StreamingAssets/Design/unity-config.json (byte-identical):
//   1. design.n38.kiln38.enabled -> true (only inside the kiln38 block; its levelFrom / levelTo are the lead's), and its gates_why line;
//   2. design.maps.list: an n38-kiln line right after n35-kiln's (campaignOnly, as every level file);
//   3. design.n31.unlocks.levels.rows: an n38-kiln row right after n35-kiln's (n 3, earns the THUNDERCLAP: as n35-kiln).
// Idempotent: a second run changes nothing. Run: node Tools/night/n38k-config.cjs [--off] (--off sets the row back to false)
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const CFG = path.join(ROOT, "Design/unity-config.json");
const SA = path.join(ROOT, "Assets/StreamingAssets/Design/unity-config.json");
const ON = !process.argv.includes("--off");

let s = fs.readFileSync(CFG, "utf8");
const nl = s.includes("\r\n") ? "\r\n" : "\n";

// 1. the row
{
  const at = s.indexOf('"kiln38": {');
  if (at < 0) throw new Error("no design.n38.kiln38 block");
  const end = s.indexOf("}", at);
  let block = s.slice(at, end);
  const was = block;
  block = block.replace(/"enabled": (true|false)/, `"enabled": ${ON}`);
  block = block.replace(/,\r?\n\s*"gates_why": "[^"]*"/, ""); // rewritten each run (the latest measured margins)
  {
    const key = '"levelTo": "n38-kiln"';
    const k = block.indexOf(key);
    if (k < 0) throw new Error("kiln38 has no levelTo n38-kiln");
    const ind = indentOf(block, k);
    const why ="Turned on by builder K3 when builder E's checker passed every gate of Design/levels/n38/kiln.plan.json (n38 gates: G1 THE STEP +82 % (4.5 m: the teach gate), G2 THE CHIMNEY +19 %, G3 THE RUNNER +15 %, G4 THE LONG RUNNER +14 %; no try without the boost lands on any; the wall-jump cheese the checker drops, measured on the real Mover by n38kiln corner: 3.50 m at most). The level: Tools/night/gen-n38-kiln.cjs, bash Tools/night/n38k-regen.sh.";
    block = block.slice(0, k + key.length) + "," + nl + ind + '"gates_why": ' + JSON.stringify(why) + block.slice(k + key.length);
  }
  if (block !== was) s = s.slice(0, at) + block + s.slice(end);
}

function indentOf(text, at) {
  const ls = text.lastIndexOf("\n", at) + 1;
  return text.slice(ls, at).match(/^\s*/)[0];
}

// 2. maps.list
if (!s.includes('"id": "n38-kiln", "label"')) {
  const key = '{ "id": "n35-kiln", "label": "LEVEL 3 THE KILN", "file": "Design/levels/n35-kiln.json", "campaignOnly": true },';
  const at = s.indexOf(key);
  if (at < 0) throw new Error("no n35-kiln maps.list line");
  const ind = indentOf(s, at);
  const line = '{ "id": "n38-kiln", "label": "LEVEL 3 THE KILN", "file": "Design/levels/n38-kiln.json", "campaignOnly": true },';
  s = s.slice(0, at + key.length) + nl + ind + line + s.slice(at + key.length);
}

// 3. unlocks rows (round 2: "room": "kb1": the THUNDERCLAP is given on entry to 3.1 THE CHARGE, as level 2's gauntlet in i1a (Unlocks31.RoomAward),
// so the gun is in hand before the first boost gate whatever the player does with the pickup)
if (/"id": "n38-kiln",\s*"earns": "shotgun",\s*"note"/.test(s)) s = s.replace(/("id": "n38-kiln",(\s*)"earns": "shotgun",)/, '$1$2"room": "kb1",');
if (!/"id": "n38-kiln",\s*"earns"/.test(s)) {
  const re = /(\{\s*"n": 3,\s*"id": "n35-kiln",\s*"earns": "shotgun",\s*"note": "[^"]*"\s*\},)/;
  const m = re.exec(s);
  if (!m) throw new Error("no n35-kiln unlocks row");
  const at = m.index;
  const ind = indentOf(s, at);
  const row = ["{", '  "n": 3,', '  "id": "n38-kiln",', '  "earns": "shotgun",', '  "room": "kb1",',
    '  "note": "LEVEL 3 THE KILN rebuilt whole for the boost (NIGHT38 K3, design.n38.kiln38 swaps n35-kiln for it): boost gates, open roofed arenas, THE KILN WYRM unchanged. The THUNDERCLAP (also a pickup and the first room\'s award)."',
    "},"].map((l, i) => (i === 0 ? "" : ind) + l).join(nl);
  const after = at + m[0].length;
  s = s.slice(0, after) + nl + ind + row + s.slice(after);
}

JSON.parse(s); // still JSON
fs.writeFileSync(CFG, s);
fs.writeFileSync(SA, s);
const same = fs.readFileSync(CFG).equals(fs.readFileSync(SA));
const c = JSON.parse(s);
console.log(`kiln38.enabled ${c.design.n38.kiln38.enabled}; maps.list n38-kiln ${c.design.maps.list.some((q) => q.id === "n38-kiln")}; copies ${same ? "identical" : "DIFFER"}`);
