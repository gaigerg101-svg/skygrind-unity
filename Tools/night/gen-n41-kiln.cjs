// NIGHT41 builder E6 (NIGHT41.md E6, kiln41; INVENTORY C5; GG w40: "make weapon-switching meaningful"): n41-kiln = n40-kiln + one CHARGER room on the
// kiln's open floor. design.n41.kiln41 swaps n40-kiln for this file (Curriculum25.N41Levels); n40-kiln and its generator (Tools/night/gen-n40-kiln.cjs)
// are untouched, so --off41 still loads n40-kiln.
// The change is a room's FIGHT only, no solid, gate, node or light: 3.2 POUR HALL (kb2, the level's open roofed arena: "boost THROUGH the pack onto the
// crucible") keeps its first two waves; its third wave (NIGHT39 K4's heavy + three swarmers) becomes the CHARGER with two swarmers. The CHARGER cannot
// jump (ai.jumps.charger) and charges in a line across open floor: the room's floor is its lane, and its answers are the level's own verbs: the
// boost up onto the crucible (out of its line), the dash aside in its tell, then the THUNDERCLAP (the level's gun) into its stunned back (E3's
// counter, NIGHT40 TTK x2.11). So this generator does not re-run K4's builder: it reads the GENERATED n40-kiln (Design/levels/n40-kiln.json: the
// geometry, nav grid and campaign K4 / S1 built and the gates proved 4/4) and writes it back under the new id with that one wave changed. The
// geometry is byte-for-byte n40-kiln's, so the checker's gates (n38 gates Design/levels/n40/kiln.plan.json) are n40-kiln's; the walk is re-run.
// Out: Design/levels/n41-kiln.json and Assets/StreamingAssets/Design/levels/n41-kiln.json (byte-identical). Run from the repo root:
//   node Tools/night/gen-n41-kiln.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const SRC = path.join(ROOT, "Design/levels/n40-kiln.json");
const OUT = [path.join(ROOT, "Design/levels/n41-kiln.json"), path.join(ROOT, "Assets/StreamingAssets/Design/levels/n41-kiln.json")];

const ROOM = "kb2"; // 3.2 POUR HALL, the open roofed arena
const WAVE = 2; // its third wave
const FROM = ["heavy", "swarmer", "swarmer", "swarmer"]; // K4's wave, checked before it is replaced
const TO = ["charger", "swarmer", "swarmer"]; // the CHARGER room: one CHARGER (the lesson) and two swarmers (the flank pressure the dash answers)

const text = fs.readFileSync(SRC, "utf8");
const level = JSON.parse(text);
if (level.name !== "n40-kiln") throw new Error(`${SRC}: name ${level.name}, expected n40-kiln`);
const room = level.campaign.rooms.find(r => r.id === ROOM);
if (!room || !Array.isArray(room.waves) || JSON.stringify(room.waves[WAVE]) !== JSON.stringify(FROM))
  throw new Error(`n40-kiln ${ROOM} wave ${WAVE + 1} is not ${JSON.stringify(FROM)}: re-read the source before changing it`);
room.waves[WAVE] = TO;
room.charger41_why = "NIGHT41 E6 kiln41 (Tools/night/gen-n41-kiln.cjs): the third wave is the CHARGER room on the kiln's open floor: it charges in a line and cannot jump; "
  + "boost onto the crucible or dash aside in its tell, then the THUNDERCLAP into its stunned back.";
level.name = "n41-kiln";
level._why = "NIGHT41 E6 kiln41: n40-kiln with 3.2 POUR HALL's third wave a CHARGER room (Tools/night/gen-n41-kiln.cjs; geometry unchanged). " + level._why;
level.generatedBy = "Tools/night/gen-n41-kiln.cjs (from the generated n40-kiln.json; its generator: " + (level.generatedBy || "-") + ")";
const eol = text.includes("\r\n") ? "\r\n" : "\n";
const out = JSON.stringify(level) + (text.endsWith(eol) ? eol : "");
for (const f of OUT) fs.writeFileSync(f, out);
console.log(`n41-kiln: ${ROOM} wave ${WAVE + 1} ${JSON.stringify(FROM)} -> ${JSON.stringify(TO)}; wrote ${OUT.length} files (${out.length} bytes)`);
