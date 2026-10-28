// NIGHT34 builder A -> THE LEAD (not run by A on the repo: these rows are outside design.n34.lines34 / calm34). What the n34 levels need before
// design.n34.lines34 is on (its levelSwap n33-foundry -> n34-foundry, n33-kiln -> n34-kiln), as Tools/night/n33r-lead-rows.cjs did for NIGHT33:
//   1. design.maps.list: a map row for n34-foundry, n34-iceline (round 3) and n34-kiln (SurvivalGame.N25 skips a level id with no map row; -skygrindMap needs it too: the lines34 reel);
//   2. design.n31.unlocks.levels.rows: a row per n34 id, the same as its n33 twin (Unlocks26 / Unlocks31.StartFor and RoomAward look a level up BY ID: with
//      no row the run falls back to NIGHT30's loadout): n34-foundry n 1 (no earns), n34-iceline n 2 earns gauntlet room i1a, n34-kiln n 3 earns shotgun;
//   3. Design/levels/unlocks.json: n34-kiln as its n33 twin (n33-foundry and n33-iceline have no row there).
// Round 3 added n34-iceline: re-running it on a tree that already has the foundry / kiln rows adds only the iceline rows.
// Each row goes right after its n33 twin (the n33 ids stay: --off34 / lines34 off is NIGHT33's game), in the file's own line ends. Idempotent (a second run
// changes nothing). Only foundry, iceline and kiln: the other three levels are not rebuilt tonight (lines34's levelSwap keeps their n33 files).
// Run from the repo root: node Tools/night/n34a-lead-rows.cjs [root]   (root defaults to "."; A dry-ran it twice on scratch copies), then copy
// Design/unity-config.json and Design/levels/unlocks.json to Assets/StreamingAssets/Design/ (cp + cmp: the lead's sync).
const fs = require("fs");
const path = require("path");
const ROOT = process.argv[2] || ".";
const CFG = path.join(ROOT, "Design/unity-config.json");
const UNL = path.join(ROOT, "Design/levels/unlocks.json");

function eolOf(t) { return t.includes("\r\n") ? "\r\n" : "\n"; }

function insertAfterLine(file, anchor, line, marker) {
  let t = fs.readFileSync(file, "utf8");
  if (t.includes(marker)) { console.log(`${file}: already has ${marker}`); return; }
  const at = t.indexOf(anchor);
  if (at < 0) throw new Error(`${file}: anchor not found: ${anchor}`);
  if (t.indexOf(anchor, at + 1) >= 0) throw new Error(`${file}: anchor found twice: ${anchor}`);
  const eol = eolOf(t);
  const end = t.indexOf("\n", at);
  t = t.slice(0, end + 1) + line + eol + t.slice(end + 1);
  JSON.parse(t);
  fs.writeFileSync(file, t);
  console.log(`${file}: inserted ${marker}`);
}

/// a multi-line unlock object in design.n31.unlocks.levels.rows, inserted right after its twin's object
function unlockRowAfter(twin, id, keys) {
  let t = fs.readFileSync(CFG, "utf8");
  const eol = eolOf(t);
  if (new RegExp(`"n": \\d+,\\r?\\n[ ]*"id": "${id}",\\r?\\n`).test(t)) { console.log(`${CFG}: unlock row ${id} already there`); return; }
  const re = new RegExp(`"n": \\d+,\\r?\\n[ ]*"id": "${twin}",\\r?\\n`, "g");
  const m = re.exec(t);
  if (!m) throw new Error(`the ${twin} unlock row not found`);
  if (re.exec(t)) throw new Error(`the ${twin} unlock row found twice`);
  const at = t.indexOf(`"id": "${twin}"`, m.index);
  const open = t.lastIndexOf("{", at);
  let depth = 0, e = open;
  for (; e < t.length; e++) { const c = t[e]; if (c === "{") depth++; else if (c === "}" && --depth === 0) break; }
  const lineStart = t.lastIndexOf("\n", at) + 1;
  const indent = t.slice(lineStart, at);
  const objIndent = indent.slice(2);
  const body = keys.map(([k, v], i) => `${indent}"${k}": ${JSON.stringify(v)}${i < keys.length - 1 ? "," : ""}`);
  const obj = ["{", ...body, `${objIndent}}`].join(eol);
  if (t[e + 1] === ",") t = t.slice(0, e + 2) + eol + objIndent + obj + "," + t.slice(e + 2);
  else t = t.slice(0, e + 1) + "," + eol + objIndent + obj + t.slice(e + 1);
  JSON.parse(t);
  fs.writeFileSync(CFG, t);
  console.log(`${CFG}: inserted the unlock row ${id}`);
}

// 1. the map rows
const LABEL = { foundry: "LEVEL 1 THE FOUNDRY", iceline: "LEVEL 2 THE ICE LINE", kiln: "LEVEL 3 THE KILN" };
for (const [g, label] of Object.entries(LABEL)) {
  insertAfterLine(CFG,
    `{ "id": "n33-${g}", "label": "${label}", "file": "Design/levels/n33-${g}.json", "campaignOnly": true },`,
    `        { "id": "n34-${g}", "label": "${label}", "file": "Design/levels/n34-${g}.json", "campaignOnly": true },`,
    `"id": "n34-${g}", "label"`);
}

// 2. the unlock rows (config), each as its n33 twin
const NOTE = "(NIGHT34 A, design.n34.lines34 swaps n33-X for it): as n33-X.";
unlockRowAfter("n33-foundry", "n34-foundry", [["n", 1], ["id", "n34-foundry"], ["note", "LEVEL 1 THE FOUNDRY, movement lines " + NOTE.replace(/X/g, "foundry") + " The KESTREL only."]]);
unlockRowAfter("n33-iceline", "n34-iceline", [["n", 2], ["id", "n34-iceline"], ["earns", "gauntlet"], ["room", "i1a"], ["note", "LEVEL 2 THE ICELINE, movement lines " + NOTE.replace(/X/g, "iceline") + " The PULSE gauntlet in i1a."]]);
unlockRowAfter("n33-kiln", "n34-kiln", [["n", 3], ["id", "n34-kiln"], ["earns", "shotgun"], ["note", "LEVEL 3 THE KILN, movement lines " + NOTE.replace(/X/g, "kiln") + " The THUNDERCLAP."]]);

// 3. unlocks.json
insertAfterLine(UNL, '{ "n": 3, "id": "n33-kiln", "earns": "shotgun",',
  '    { "n": 3, "id": "n34-kiln", "earns": "shotgun", "note": "THE KILN movement lines (NIGHT34 A, design.n34.lines34): the THUNDERCLAP." },', '"id": "n34-kiln"');
