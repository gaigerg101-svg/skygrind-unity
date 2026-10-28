// NIGHT42 builder L1: patch ONLY design.n42.teach42 (its numbers) in Design/unity-config.json and copy the file to Assets/StreamingAssets (byte-identical).
// Edits the row's text in place (the file's own formatting kept): inserts the numbers after the row's "enabled_why" line.
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const SRC = path.join(ROOT, "Design/unity-config.json");
const DST = path.join(ROOT, "Assets/StreamingAssets/Design/unity-config.json");
let txt = fs.readFileSync(SRC, "utf8");
const at = txt.indexOf('"teach42": {');
if (at < 0) throw new Error("design.n42.teach42 missing");
const end = txt.indexOf("}", at);
const row = txt.slice(at, end);
if (row.includes('"windowS"')) { console.log("teach42 already has its numbers"); process.exit(0); }
const nl = txt.includes("\r\n") ? "\r\n" : "\n";
const ind = "        ";
const add = [
  `"windowS": 60,`,
  `"windowS_why": "DECIDE42 D-7 (GG G6): a level's first minute; counted on the HUD from a run's start or a campaign level change (UiTeach42.cs).",`,
  `"maxLines": 2,`,
  `"maxLines_why": "DECIDE42 D-7: at most two teaching lines (teach35 + the rooms' lesson lines, a boss's own line excepted) START in the window; the rest wait in their queue / stay wanted and show after it (CODE-FACTS: level 1 showed 7 in ~52 s).",`,
  `"skip": ["MOVE. LOOK. {jump} JUMP."],`,
  `"skip_why": "Lesson formats never shown with the row on: level 1's opener teaches what every player does in the first second (a1, n42-foundry), so the two lines the window allows go to the next verbs (by CODE-FACTS order dash and slide; not checked in a Player)."`,
];
// insert before the closing brace of the row: after the last property line (enabled_why)
const lastLineEnd = txt.lastIndexOf('"', end) + 1;
const ins = "," + nl + add.map((l, i) => ind + l.replace(/,$/, i === add.length - 1 ? "" : ",")).join(nl);
txt = txt.slice(0, lastLineEnd) + ins + txt.slice(lastLineEnd);
JSON.parse(txt);
fs.writeFileSync(SRC, txt);
fs.writeFileSync(DST, txt);
console.log("design.n42.teach42: windowS 60, maxLines 2, skip 1; both copies written");
