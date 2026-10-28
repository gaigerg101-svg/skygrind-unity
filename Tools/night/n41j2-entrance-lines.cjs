// NIGHT41 builder J2: adds `lines` (+ `lines_why`) to design.n41.entrance41 ONLY (L-049: a builder patches only its own row), as text
// inserted after the row's enabled_why, then copies the config to Assets/StreamingAssets/Design/ (byte-identical before and after).
// The lines are the five bosses' rule on entry, keyed by the boss id their bossRise carries (BossEvent.boss): THE LOOM, THE CROWN and
// THE ICEBREAKER copy their NIGHT40 BossLine40 entry lines (design.n40.loom40.lineRule, crown40.line.entry, brawl40.line.entry); THE WICK
// and THE STAG never had one, so theirs are written here from their damage rules (Core/Enemies/Wick37.cs and Stag37.cs file heads).
// Refuses to run twice. Run from the worktree root: node Tools/night/n41j2-entrance-lines.cjs
const fs = require("fs");
const path = require("path");
const root = path.resolve(__dirname, "../..") + "/";
const file = root + "Design/unity-config.json";
const copy = root + "Assets/StreamingAssets/Design/unity-config.json";
let raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ before the patch");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const d0 = JSON.parse(raw);
const row = d0.design.n41.entrance41;
if (!row) throw new Error("design.n41.entrance41 missing");
if (row.lines) throw new Error("entrance41.lines already present");

const lines = {
  wick: "RIDE A WALL, HOLD {fire}: SHOOT THROUGH A CRACK",
  stag37: "CATCH IT ON THE ICE: {punch} ITS LEGS",
  loom37: d0.design.n40.loom40.lineRule,
  crown37: d0.design.n40.crown40.line.entry,
  brawl40: d0.design.n40.brawl40.line.entry,
};
for (const k in lines) if (typeof lines[k] !== "string" || !lines[k]) throw new Error("no line for " + k);
const why =
  "The rule line under the name card in the entrance beat (UiJuice38.Beat41Line), keyed by the bossRise's boss id. loom37 / crown37 / brawl40: their NIGHT40 BossLine40 entry lines word for word (design.n40.loom40.lineRule, crown40.line.entry, brawl40.line.entry). wick / stag37 had no rule line (no BossLine40): written from their damage rules (Wick37.cs: a charged shot from a wall ride through a cracked plate; Stag37.cs: catch it on the ice and punch its legs). {action} names resolve to the bound buttons (the lesson widget's resolver). Text only: no sim reads it.";

const anchor = /("entrance41": \{\r?\n(\s*)"_why": [^\n]*\n\s*"enabled": false,\r?\n\s*"enabled_why": "[^"]*")/;
const m = raw.match(anchor);
if (!m) throw new Error("the entrance41 row was not found");
const ind = m[2];
let add = "," + eol + ind + '"lines": {';
const keys = Object.keys(lines);
keys.forEach((k, i) => {
  add += eol + ind + "  " + JSON.stringify(k) + ": " + JSON.stringify(lines[k]) + (i < keys.length - 1 ? "," : "");
});
add += eol + ind + "}," + eol + ind + '"lines_why": ' + JSON.stringify(why);
raw = raw.replace(anchor, m[1] + add);

const d1 = JSON.parse(raw); // still valid JSON
const got = d1.design.n41.entrance41.lines;
for (const k of keys) if (got[k] !== lines[k]) throw new Error("line mismatch " + k);
// nothing else changed: drop the two new keys and compare
delete d1.design.n41.entrance41.lines;
delete d1.design.n41.entrance41.lines_why;
if (JSON.stringify(d1) !== JSON.stringify(d0)) throw new Error("something besides entrance41 changed");
fs.writeFileSync(file, raw);
fs.copyFileSync(file, copy);
console.log("entrance41.lines: " + keys.map((k) => k + " = " + lines[k]).join(" | "));
