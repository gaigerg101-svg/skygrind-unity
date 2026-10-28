// NIGHT32 builder S -> THE LEAD (not run by S on the repo: these rows are outside design.n32.span32). What n32-span needs before design.n32.span32 is on:
//   1. design.maps.list: a map row for "n32-span" (SurvivalGame.N25.cs skips a level id with no map row, so the swapped level 4 would be skipped).
//   2. design.n31.unlocks.levels.rows: { n 4, id "n32-span", earns "nailgun" } (Unlocks26.StartFor / RoomAward look the level up BY ID: with no row
//      the run falls back to NIGHT30's loadout rules and level 4 would not start with the THUNDERCLAP).
//   3. Design/levels/unlocks.json: the same n 4 row for "n32-span" (the level select's weapon text; Systems29Tests read this file).
// Each is a text insertion right after the n30-span row (n30-span stays: --off32 / the row off is NIGHT31's game), in the file's own line ends.
// Idempotent. Then copy Design/unity-config.json and Design/levels/unlocks.json to Assets/StreamingAssets/Design/ (the lead's sync).
// Run from the repo root: node Tools/night/n32s-lead-rows.cjs   (S dry-ran it on scratch copies: the three rows go in, a second run changes nothing)
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

// 1. the map row
insertAfterLine("Design/unity-config.json",
  '{ "id": "n30-span", "label": "LEVEL 4 THE SPAN", "file": "Design/levels/n30-span.json", "campaignOnly": true },',
  '        { "id": "n32-span", "label": "LEVEL 4 THE SPAN", "file": "Design/levels/n32-span.json", "campaignOnly": true },',
  '"id": "n32-span", "label"');

// 2. the unlock row in design.n31.unlocks.levels.rows (a multi-line object: inserted after the n30-span object's closing "},")
{
  const file = "Design/unity-config.json";
  let t = fs.readFileSync(file, "utf8");
  const eol = eolOf(t);
  if (/"id": "n32-span",\r?\n/.test(t)) console.log(`${file}: unlock row n32-span already there`);
  else {
    const m = /"id": "n30-span",\r?\n/.exec(t);
    if (!m) throw new Error("the n30-span unlock row not found");
    const at = m.index;
    const close = t.indexOf("},", at);
    const lineStart = t.lastIndexOf("\n", at) + 1;
    const indent = t.slice(lineStart, at); // the keys' indent
    const objIndent = indent.slice(2);
    const row = ["{", `${indent}"n": 4,`, `${indent}"id": "n32-span",`, `${indent}"earns": "nailgun",`,
      `${indent}"note": "LEVEL 4 THE SPAN rebuilt (NIGHT32 S, design.n32.span32 swaps n30-span for it): the NAILGUN (also a pickup in room s1)."`, `${objIndent}},`].join(eol);
    t = t.slice(0, close + 2) + eol + objIndent + row + t.slice(close + 2);
    JSON.parse(t);
    fs.writeFileSync(file, t);
    console.log(`${file}: inserted the unlock row n32-span`);
  }
}

// 3. unlocks.json
insertAfterLine("Design/levels/unlocks.json",
  '{ "n": 4, "id": "n30-span", "earns": "nailgun",',
  '    { "n": 4, "id": "n32-span", "earns": "nailgun", "note": "LEVEL 4 THE SPAN rebuilt (NIGHT32 S, design.n32.span32): the NAILGUN (also a pickup in room s1)." },',
  '"id": "n32-span"');
