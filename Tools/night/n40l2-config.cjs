// NIGHT40 builder L2: patch ONLY design.n40.iceline40 (L-049) in Design/unity-config.json, then copy the file byte for byte to
// Assets/StreamingAssets/Design/unity-config.json. The row's text is replaced in place (from its `"iceline40": {` line to its closing brace at
// the same indent), keeping the file's other bytes and the row's own line ending. Idempotent: run it again after an edit below.
// Usage: node Tools/night/n40l2-config.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const file = path.join(ROOT, "Design/unity-config.json");
const copy = path.join(ROOT, "Assets/StreamingAssets/Design/unity-config.json");
const raw = fs.readFileSync(file, "utf8");
const cfg = JSON.parse(raw);
const old = cfg.design.n40.iceline40;

const row = {
  _why: old._why,
  enabled: old.enabled,
  enabled_why: old.enabled_why,
  levelFrom: old.levelFrom,
  levelTo: old.levelTo,
  startSet: old.startSet,
  startSet_why: old.startSet_why,
  awards: [{ room: "g6", weapon: "revolver" }],
  awards_why: "GG ask 14 ('unlock them later in the level when entering certain zones'): the KESTREL is given on entering n40-iceline's 2.6 THE ARMOURY (room g6, after the gauntlet's five lessons g1-g5; Design/levels/n40/iceline.beats.md). Given once, as the unlock table's room award (SurvivalSim.N40 N40RoomAwardsStep).",
  budgetCost: { warden: 4, drone: 2, charger: 4 },
  budgetCost_why: "The wave budget's cost of the three kinds n40-iceline places (LevelValidate29.Budget knows husk 1, swarmer 0.5, spitter 2, heavy / tank 6 from design.n27.budget; it has no cost for these): a level-authoring number, no gameplay effect. WARDEN 4 (200 hp behind a shield: between a spitter and a heavy), DRONE 2 (30 hp, ranged: a spitter's), CHARGER 4 (150 hp, a heavy's charge but no tank's hp). Unity-only; the web has no wave budget.",
};

const lines = raw.split("\n");
const start = lines.findIndex((l) => /^\s*"iceline40": \{\r?$/.test(l));
if (start < 0) throw new Error("no iceline40 row");
const indent = lines[start].match(/^\s*/)[0];
const cr = lines[start].endsWith("\r") ? "\r" : "";
let end = start + 1;
while (end < lines.length && !(lines[end].replace(/\r$/, "") === indent + "}," || lines[end].replace(/\r$/, "") === indent + "}")) end++;
if (end >= lines.length) throw new Error("no end of the iceline40 row");
const comma = lines[end].replace(/\r$/, "").endsWith(",") ? "," : "";
const body = JSON.stringify(row, null, 2).split("\n").map((l, i) => (i === 0 ? indent + '"iceline40": ' + l : indent + l));
body[body.length - 1] += comma;
const out = lines.slice(0, start).concat(body.map((l) => l + cr), lines.slice(end + 1)).join("\n");
const check = JSON.parse(out);
// only this row changed
const a = JSON.parse(raw), b = JSON.parse(out);
delete a.design.n40.iceline40; delete b.design.n40.iceline40;
if (JSON.stringify(a) !== JSON.stringify(b)) throw new Error("something outside design.n40.iceline40 changed");
fs.writeFileSync(file, out);
fs.copyFileSync(file, copy);
console.log("design.n40.iceline40:", JSON.stringify(check.design.n40.iceline40.awards), "budgetCost", JSON.stringify(check.design.n40.iceline40.budgetCost), "; copied to StreamingAssets");
