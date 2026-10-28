// NIGHT32 (the lead): the rows a swapped level needs outside its builder's design.n32 row (A2 / A / S): a design.maps.list row (SurvivalGame.N25
// skips a level id with no map row) and the unlock rows (Unlocks26.StartFor looks a level up BY ID: with no row the run falls back to NIGHT30's
// loadout). For n32-foundry (level 1) and n32-kiln (level 3), builder A's levels; builder S's n32-span rows come from Tools/night/n32s-lead-rows.cjs.
// Each row goes right after its n30 twin (the n30 ids stay: --off32 / the row off is NIGHT31's game). Idempotent.
// Run from the repo root: node Tools/night/n32-lead-level-rows.cjs   then bash Tools/night/n24-sync.sh and copy Design/levels/unlocks.json too.
const fs = require("fs");
function eolOf(t) { return t.includes("\r\n") ? "\r\n" : "\n"; }

function insertAfterLine(file, anchor, line, marker) {
  let t = fs.readFileSync(file, "utf8");
  if (t.includes(marker)) { console.log(`${file}: already has ${marker}`); return; }
  const at = t.indexOf(anchor);
  if (at < 0) throw new Error(`${file}: anchor not found: ${anchor}`);
  const eol = eolOf(t);
  const end = t.indexOf("\n", at);
  t = t.slice(0, end + 1) + line + eol + t.slice(end + 1);
  JSON.parse(t);
  fs.writeFileSync(file, t);
  console.log(`${file}: inserted ${marker}`);
}

function unlockRowAfter(twin, id, n, earns, note) {
  const file = "Design/unity-config.json";
  let t = fs.readFileSync(file, "utf8");
  const eol = eolOf(t);
  if (new RegExp(`"id": "${id}",\\r?\\n`).test(t)) { console.log(`${file}: unlock row ${id} already there`); return; }
  const m = new RegExp(`"id": "${twin}",\\r?\\n`).exec(t);
  if (!m) throw new Error(`the ${twin} unlock row not found`);
  const at = m.index;
  const close = t.indexOf("},", at);
  const lineStart = t.lastIndexOf("\n", at) + 1;
  const indent = t.slice(lineStart, at);
  const objIndent = indent.slice(2);
  const keys = [`${indent}"n": ${n},`, `${indent}"id": "${id}",`];
  if (earns) keys.push(`${indent}"earns": "${earns}",`);
  keys.push(`${indent}"note": "${note}"`);
  const row = ["{", ...keys, `${objIndent}},`].join(eol);
  t = t.slice(0, close + 2) + eol + objIndent + row + t.slice(close + 2);
  JSON.parse(t);
  fs.writeFileSync(file, t);
  console.log(`${file}: inserted the unlock row ${id}`);
}

// the map rows
insertAfterLine("Design/unity-config.json",
  '{ "id": "n30-foundry", "label": "LEVEL 1 THE FOUNDRY", "file": "Design/levels/n30-foundry.json", "campaignOnly": true },',
  '        { "id": "n32-foundry", "label": "LEVEL 1 THE FOUNDRY", "file": "Design/levels/n32-foundry.json", "campaignOnly": true },',
  '"id": "n32-foundry", "label"');
insertAfterLine("Design/unity-config.json",
  '{ "id": "n30-kiln", "label": "LEVEL 3 THE KILN", "file": "Design/levels/n30-kiln.json", "campaignOnly": true },',
  '        { "id": "n32-kiln", "label": "LEVEL 3 THE KILN", "file": "Design/levels/n32-kiln.json", "campaignOnly": true },',
  '"id": "n32-kiln", "label"');

// the unlock rows (config): level 1 earns nothing (its n30 twin has no earns), level 3 the THUNDERCLAP
unlockRowAfter("n30-foundry", "n32-foundry", 1, null, "LEVEL 1 THE FOUNDRY rebuilt rooms (NIGHT32 A, design.n32.arenas32 swaps n30-foundry for it): the KESTREL only, as n30-foundry.");
unlockRowAfter("n30-kiln", "n32-kiln", 3, "shotgun", "LEVEL 3 THE KILN rebuilt rooms (NIGHT32 A, design.n32.arenas32 swaps n30-kiln for it): the THUNDERCLAP as n30-kiln.");

// unlocks.json (the level select's weapon text; n30-foundry has no row there)
insertAfterLine("Design/levels/unlocks.json",
  '{ "n": 3, "id": "n30-kiln", "earns": "shotgun",',
  '    { "n": 3, "id": "n32-kiln", "earns": "shotgun", "note": "THE KILN rebuilt rooms (NIGHT32 A, design.n32.arenas32): the THUNDERCLAP." },',
  '"id": "n32-kiln"');
