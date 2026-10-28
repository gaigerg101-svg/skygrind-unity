// NIGHT35 builder A -> THE LEAD (rows outside design.n35.campaign6, so A does not run it on the repo: L-049). What the n35 levels need before
// design.n35.campaign6 is on (its levelSwap n34-foundry -> n35-foundry, n34-iceline -> n35-iceline, n34-kiln -> n35-kiln, and its finale n35-finale),
// as Tools/night/n34a-lead-rows.cjs did for NIGHT34:
//   1. design.maps.list: a map row for n35-foundry, n35-iceline, n35-kiln and n35-finale (SurvivalGame.N25 skips a level id with no map row;
//      -skygrindMap needs it too: the levels35 reel);
//   2. design.n31.unlocks.levels.rows: a row per n35 id (Unlocks31.StartFor / RoomAward look a level up BY ID: with no row the run keeps the whole
//      loadout): n35-foundry n 1 (no earns: the KESTREL only), n35-iceline n 2 earns gauntlet room i1a, n35-kiln n 3 earns shotgun, and the finale
//      n35-finale n 6 (no earns: it starts with the earns of n 1-5, the KESTREL, gauntlet, THUNDERCLAP, NAILGUN and TORCH; PRINCIPLES 4: the exam);
//   3. Design/levels/unlocks.json: n35-kiln as its n34 twin (A's own file: A's commit already has this row; the script skips it when present).
// Each row goes right after its n34 twin (the n34 ids stay: --off35 / campaign6 off is NIGHT34's game); the finale's rows go after n33-spire's. In the
// file's own line ends. Idempotent (a second run changes nothing).
// Run from the repo root: node Tools/night/n35a-lead-rows.cjs [root]   (root defaults to "."; A dry-ran it twice on a scratch copy), then copy
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
    `{ "id": "n34-${g}", "label": "${label}", "file": "Design/levels/n34-${g}.json", "campaignOnly": true },`,
    `        { "id": "n35-${g}", "label": "${label}", "file": "Design/levels/n35-${g}.json", "campaignOnly": true },`,
    `"id": "n35-${g}", "label"`);
}
insertAfterLine(CFG,
  `{ "id": "n33-spire", "label": "LEVEL 5 THE SPIRE", "file": "Design/levels/n33-spire.json", "campaignOnly": true },`,
  `        { "id": "n35-finale", "label": "LEVEL 6 THE LAST LINE", "file": "Design/levels/n35-finale.json", "campaignOnly": true },`,
  `"id": "n35-finale", "label"`);

// 2. the unlock rows (config), each as its n34 twin; the finale after level 5's row
const NOTE = "(NIGHT35 A, design.n35.campaign6 swaps n34-X for it): as n34-X plus a Lucio wall-ride room and the teach35 triggers.";
unlockRowAfter("n34-foundry", "n35-foundry", [["n", 1], ["id", "n35-foundry"], ["note", "LEVEL 1 THE FOUNDRY " + NOTE.replace(/X/g, "foundry") + " The KESTREL only."]]);
unlockRowAfter("n34-iceline", "n35-iceline", [["n", 2], ["id", "n35-iceline"], ["earns", "gauntlet"], ["room", "i1a"], ["note", "LEVEL 2 THE ICE LINE " + NOTE.replace(/X/g, "iceline") + " The PULSE gauntlet in i1a."]]);
unlockRowAfter("n34-kiln", "n35-kiln", [["n", 3], ["id", "n35-kiln"], ["earns", "shotgun"], ["note", "LEVEL 3 THE KILN " + NOTE.replace(/X/g, "kiln") + " The THUNDERCLAP."]]);
unlockRowAfter("n33-spire", "n35-finale", [["n", 6], ["id", "n35-finale"], ["note", "NIGHT35 A: LEVEL 6 THE LAST LINE (design.n35.campaign6's finaleId, DECIDE35 D-1): starts with the earns of n 1-5 (KESTREL, PULSE gauntlet, THUNDERCLAP, NAILGUN, TORCH); earns nothing (the exam)."]]);

// 3. unlocks.json (A's own file; A's commit already carries it)
insertAfterLine(UNL, '{ "n": 3, "id": "n34-kiln", "earns": "shotgun",',
  '    { "n": 3, "id": "n35-kiln", "earns": "shotgun", "note": "THE KILN with a Lucio wall-ride room (NIGHT35 A, design.n35.campaign6): the THUNDERCLAP." },', '"id": "n35-kiln"');
