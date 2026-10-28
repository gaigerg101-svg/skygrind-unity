// NIGHT33 builder R -> THE LEAD (not run by R on the repo: these rows are outside design.n33.devices33 / arenas33). What the n33 levels need before
// design.n33.kit33 is on (its levelSwap n32-* -> n33-*):
//   1. design.maps.list: a map row for each of n33-foundry, n33-iceline, n33-kiln, n33-span, n33-spire (SurvivalGame.N25 skips a level id with no map row).
//   2. design.n31.unlocks.levels.rows: a row per n33 id, the same as its n32 twin (Unlocks26 / Unlocks31.StartFor and RoomAward look a level up BY ID: with no
//      row the run falls back to NIGHT30's loadout): n33-foundry n 1 (no earns), n33-iceline n 2 earns gauntlet room i1a, n33-kiln n 3 shotgun, n33-span n 4
//      nailgun, n33-spire n 5 torch.
//   3. Design/levels/unlocks.json: the same rows for the ids its n32 twins have there (n33-kiln, n33-span, n33-spire; the level select's weapon text and
//      Campaign24's last-room unlock for level 5).
// Each row goes right after its n32 twin (the n32 ids stay: --off33 / kit33 off is NIGHT32's game), in the file's own line ends. Idempotent.
// Run from the repo root: node Tools/night/n33r-lead-rows.cjs [root]   (root defaults to "."; R dry-ran it on scratch copies), then copy
// Design/unity-config.json and Design/levels/unlocks.json to Assets/StreamingAssets/Design/ (the lead's sync).
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

/// a multi-line unlock object in design.n31.unlocks.levels.rows, inserted right after its twin's object (the twin may be the array's last element: n32-spire)
function unlockRowAfter(twin, id, keys) {
  let t = fs.readFileSync(CFG, "utf8");
  const eol = eolOf(t);
  if (new RegExp(`"n": \\d+,\\r?\\n[ ]*"id": "${id}",\\r?\\n`).test(t)) { console.log(`${CFG}: unlock row ${id} already there`); return; }
  // the unlock row's "id" follows its "n" key (design.n32.level5 also has "id": "n32-spire", with no "n" before it)
  const re = new RegExp(`"n": \\d+,\\r?\\n[ ]*"id": "${twin}",\\r?\\n`, "g");
  const m = re.exec(t);
  if (!m) throw new Error(`the ${twin} unlock row not found`);
  if (re.exec(t)) throw new Error(`the ${twin} unlock row found twice`);
  const at = t.indexOf(`"id": "${twin}"`, m.index);
  const open = t.lastIndexOf("{", at);
  let depth = 0, e = open;
  for (; e < t.length; e++) { const c = t[e]; if (c === "{") depth++; else if (c === "}" && --depth === 0) break; } // the notes hold no braces (checked by JSON.parse below)
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
const LABEL = { foundry: "LEVEL 1 THE FOUNDRY", iceline: "LEVEL 2 THE ICE LINE", kiln: "LEVEL 3 THE KILN", span: "LEVEL 4 THE SPAN", spire: "LEVEL 5 THE SPIRE" };
for (const [g, label] of Object.entries(LABEL)) {
  insertAfterLine(CFG,
    `{ "id": "n32-${g}", "label": "${label}", "file": "Design/levels/n32-${g}.json", "campaignOnly": true },`,
    `        { "id": "n33-${g}", "label": "${label}", "file": "Design/levels/n33-${g}.json", "campaignOnly": true },`,
    `"id": "n33-${g}", "label"`);
}

// 2. the unlock rows (config), each as its n32 twin
const NOTE = "(NIGHT33 R, design.n33.kit33 swaps n32-X for it): as n32-X.";
unlockRowAfter("n32-foundry", "n33-foundry", [["n", 1], ["id", "n33-foundry"], ["note", "LEVEL 1 THE FOUNDRY, overhauled rooms " + NOTE.replace(/X/g, "foundry") + " The KESTREL only."]]);
unlockRowAfter("n32-iceline", "n33-iceline", [["n", 2], ["id", "n33-iceline"], ["earns", "gauntlet"], ["room", "i1a"], ["note", "LEVEL 2 THE ICELINE, overhauled rooms " + NOTE.replace(/X/g, "iceline") + " The PULSE gauntlet in i1a."]]);
unlockRowAfter("n32-kiln", "n33-kiln", [["n", 3], ["id", "n33-kiln"], ["earns", "shotgun"], ["note", "LEVEL 3 THE KILN, overhauled rooms " + NOTE.replace(/X/g, "kiln") + " The THUNDERCLAP."]]);
unlockRowAfter("n32-span", "n33-span", [["n", 4], ["id", "n33-span"], ["earns", "nailgun"], ["note", "LEVEL 4 THE SPAN, overhauled rooms " + NOTE.replace(/X/g, "span") + " The NAILGUN (also a pickup in room s1)."]]);
unlockRowAfter("n32-spire", "n33-spire", [["n", 5], ["id", "n33-spire"], ["earns", "torch"], ["note", "LEVEL 5 THE SPIRE, overhauled rooms " + NOTE.replace(/X/g, "spire") + " Starts with all four weapons; earns the TORCH at its end."]]);

// 3. unlocks.json (n32-foundry / n32-iceline have no row there)
insertAfterLine(UNL, '{ "n": 3, "id": "n32-kiln", "earns": "shotgun",',
  '    { "n": 3, "id": "n33-kiln", "earns": "shotgun", "note": "THE KILN overhauled rooms (NIGHT33 R, design.n33.kit33): the THUNDERCLAP." },', '"id": "n33-kiln"');
insertAfterLine(UNL, '{ "n": 4, "id": "n32-span", "earns": "nailgun",',
  '    { "n": 4, "id": "n33-span", "earns": "nailgun", "note": "LEVEL 4 THE SPAN overhauled rooms (NIGHT33 R, design.n33.kit33): the NAILGUN (also a pickup in room s1)." },', '"id": "n33-span"');
insertAfterLine(UNL, '{ "n": 5, "id": "n32-spire", "earns": "torch",',
  '    { "n": 5, "id": "n33-spire", "earns": "torch", "note": "LEVEL 5 THE SPIRE overhauled rooms (NIGHT33 R, design.n33.kit33): the TORCH, earned with the last room, as n32-spire." },', '"id": "n33-spire"');
