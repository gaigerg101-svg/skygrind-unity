// NIGHT32 builder A, for the LEAD (not applied by builder A): the rows n32-iceline needs outside design.n32.arenas32, as n32-lead-level-rows.cjs did for
// n32-foundry / n32-kiln: a design.maps.list row (SurvivalGame.N25 skips a level id with no map row) right after n30-iceline's, and the unlock row in
// design.n31.unlocks.levels.rows (Unlocks26.StartFor looks a level up BY ID) right after n30-iceline's: n 2, the PULSE gauntlet earned in room i1a (the n30 twin's
// earns / room; i1a and its pickup31 are unchanged in n32-iceline). Design/levels/unlocks.json has no n30-iceline row (level 2's earn is the config's), so none is added.
// The levelSwap entry n30-iceline -> n32-iceline is builder A's (design.n32.arenas32, committed with the level). Idempotent.
// Run from the repo root: node Tools/night/n32a-iceline-rows.cjs   then bash Tools/night/n24-sync.sh
const fs = require("fs");
const file = "Design/unity-config.json";
let t = fs.readFileSync(file, "utf8");
const eol = t.includes("\r\n") ? "\r\n" : "\n";

// the map row
const mapAnchor = '{ "id": "n30-iceline", "label": "LEVEL 2 THE ICE LINE", "file": "Design/levels/n30-iceline.json", "campaignOnly": true },';
if (t.includes('"id": "n32-iceline", "label"')) console.log("map row n32-iceline already there");
else {
  const at = t.indexOf(mapAnchor);
  if (at < 0) throw new Error("the n30-iceline map row not found");
  const end = t.indexOf("\n", at);
  t = t.slice(0, end + 1) + '        { "id": "n32-iceline", "label": "LEVEL 2 THE ICE LINE", "file": "Design/levels/n32-iceline.json", "campaignOnly": true },' + eol + t.slice(end + 1);
  console.log("inserted the map row n32-iceline");
}

// the unlock row (after the n30-iceline row's closing brace)
if (/"id": "n32-iceline",\r?\n/.test(t)) console.log("unlock row n32-iceline already there");
else {
  const m = /"id": "n30-iceline",\r?\n/.exec(t);
  if (!m) throw new Error("the n30-iceline unlock row not found");
  const lineStart = t.lastIndexOf("\n", m.index) + 1;
  const indent = t.slice(lineStart, m.index), objIndent = indent.slice(2);
  const close = t.indexOf("},", m.index);
  const row = ["{", `${indent}"n": 2,`, `${indent}"id": "n32-iceline",`, `${indent}"earns": "gauntlet",`, `${indent}"room": "i1a",`,
    `${indent}"note": "LEVEL 2 THE ICELINE rebuilt rooms (NIGHT32 A, design.n32.arenas32 swaps n30-iceline for it): the PULSE gauntlet in i1a, as n30-iceline."`, `${objIndent}},`].join(eol);
  t = t.slice(0, close + 2) + eol + objIndent + row + t.slice(close + 2);
  console.log("inserted the unlock row n32-iceline");
}
JSON.parse(t);
fs.writeFileSync(file, t);
