// NIGHT32 (builder F): the level-5 registration lines in Design/unity-config.json (text edits, so the file's hand formatting stays), then the Player copy:
//   1. design.n32.level5: id "n32-spire" (Curriculum25.N32Levels inserts it after n30-span while the row is on); the row stays enabled false (the lead turns it on)
//   2. design.maps.list: the LEVEL 5 THE SPIRE row (campaignOnly: never on the MAP row; a slot only while the id is in the campaign order)
//   3. design.n31.unlocks.levels.rows: { n 5, id n32-spire, earns torch } (all four weapons from the start: StartFor(5) = the KESTREL + every earns with n < 5)
// Idempotent (each edit is skipped when its text is already there). Usage, from the repo root: node Tools/night/n32f-config-level5.cjs
const fs = require("fs");
const FILE = "Design/unity-config.json";
let s = fs.readFileSync(FILE, "utf8");
const nl = s.includes("\r\n") ? "\r\n" : "\n";
let changed = 0;
function replaceOnce(from, to, label) {
  if (s.includes(to)) { console.log("already:", label); return; }
  const i = s.indexOf(from);
  if (i < 0 || s.indexOf(from, i + 1) >= 0) { console.error("anchor not found exactly once:", label); process.exit(1); }
  s = s.slice(0, i) + to + s.slice(i + from.length);
  changed++;
  console.log("edited:", label);
}

// 1. the level-5 row's id and its _why
replaceOnce(
  `"_why": "Builder F (DESIGN32 F): level 5, the finale: every verb and all four weapons, remixed rooms, a gauntlet, then the boss arena (a true circle). Builder F adds its numbers with their _why.",`,
  `"_why": "Builder F (DESIGN32 F): level 5, the finale: every verb and all four weapons, remixed rooms, a gauntlet, then the boss arena (a true circle). LEVEL 5 THE SPIRE (Design/levels/n32-spire.json from Tools/night/gen-n32-spire.cjs): v1 threshold, v2 FOUNDRY FLOOR (the lava wall lane), v3 ICE GLIDE, v4 landing, v5 KILN VENTS (aimed pads), v6 RING SPAN (rings for the 2-charge grapple), v7 landing, v8 THE GAUNTLET, v9 THE CROWN (boss id crown, builder C's arena; with design.n32.boss5 off the room clears on entry). While on: the level is inserted after n30-span, the level select has 5 playable slots (UnityTuning.LevelSel32Level5Playable). The weapons come from design.n31.unlocks (row n 5: all four from the start; the level earns the TORCH at its end through Design/levels/unlocks.json). No sim number of its own: the level is data.",`,
  "level5 _why");
replaceOnce(
  `"id_why": "Curriculum25.N32Levels: while this row is on, this level id is inserted right after \`after\` (level 4) in the campaign order; builder F names it.",${nl}        "id": "",`,
  `"id_why": "Curriculum25.N32Levels: while this row is on, this level id is inserted right after \`after\` (level 4) in the campaign order; builder F names it.",${nl}        "id": "n32-spire",`,
  "level5 id");

// 2. the map row
replaceOnce(
  `        { "id": "n30-kiln", "label": "LEVEL 3 THE KILN", "file": "Design/levels/n30-kiln.json", "campaignOnly": true },${nl}`,
  `        { "id": "n30-kiln", "label": "LEVEL 3 THE KILN", "file": "Design/levels/n30-kiln.json", "campaignOnly": true },${nl}        { "id": "n32-spire", "label": "LEVEL 5 THE SPIRE", "file": "Design/levels/n32-spire.json", "campaignOnly": true },${nl}`,
  "maps.list row");

// 3. the unlock row n 5 (after the last n 4 row)
replaceOnce(
  `              "n": 4,${nl}              "id": "level3",${nl}              "earns": "nailgun",${nl}              "note": "level 4 under its old id."${nl}            }${nl}`,
  `              "n": 4,${nl}              "id": "level3",${nl}              "earns": "nailgun",${nl}              "note": "level 4 under its old id."${nl}            },${nl}            {${nl}              "n": 5,${nl}              "id": "n32-spire",${nl}              "earns": "torch",${nl}              "note": "NIGHT32 F: LEVEL 5 THE SPIRE (design.n32.level5): starts with all four weapons (KESTREL, PULSE gauntlet, THUNDERCLAP, NAILGUN: the earns of n 1-4); earns the TORCH at its end (no room: Campaign24's last-room unlock through Design/levels/unlocks.json, the old table's level 5 weapon; the TORCH fires with design.adds.b5 on)."${nl}            }${nl}`,
  "n31 unlock row n 5");

JSON.parse(s); // still valid JSON
if (changed) fs.writeFileSync(FILE, s);
fs.mkdirSync("Assets/StreamingAssets/Design", { recursive: true });
fs.copyFileSync(FILE, "Assets/StreamingAssets/Design/unity-config.json");
console.log(changed, "edit(s); Player copy written");
