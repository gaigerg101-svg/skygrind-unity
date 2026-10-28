// NIGHT41 builder J2: adds the level select preview's text to design.n41.select41 ONLY (L-049): `bosses` (a level family -> its boss's name),
// `rebuildSlot` and `rebuildText` (+ their _why), inserted after the row's enabled_why; then copies the config to Assets/StreamingAssets/Design/
// (byte-identical before and after). Refuses to run twice. Run from the worktree root: node Tools/night/n41j2-select-rows.cjs
const fs = require("fs");
const path = require("path");
const root = path.resolve(__dirname, "../..") + "/";
const file = root + "Design/unity-config.json";
const copy = root + "Assets/StreamingAssets/Design/unity-config.json";
let raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ before the patch");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const d0 = JSON.parse(raw);
const row = d0.design.n41.select41;
if (!row) throw new Error("design.n41.select41 missing");
if (row.bosses) throw new Error("select41.bosses already present");

// the campaign's bosses by level family (the level ids' part after "nNN-"): level 1 foundry THE WICK (design.n37.wick37 -> n38-foundry),
// level 2 iceline THE ICEBREAKER (design.n40.iceline40 / brawl40 on n40-iceline), level 3 kiln THE KILN WYRM, level 4 span THE LOOM
// (loom37 / loom40), level 5 spire THE CROWN (crown37 / crown40)
const bosses = { foundry: "THE WICK", iceline: "THE ICEBREAKER", kiln: "THE KILN WYRM", span: "THE LOOM", spire: "THE CROWN" };
const add = {
  bosses,
  bosses_why:
    "The boss shown after each level's name in the level select (red-orange: the boss is danger), keyed by the level id's family (n41-foundry -> foundry): the boss each level ends on tonight (WICK on foundry, ICEBREAKER on n40-iceline, KILN WYRM on kiln, LOOM on span, CROWN on spire). A drawn silhouette needs art the project has no file for: the name stands in (SHORT, J2).",
  rebuildSlot: 6,
  rebuildSlot_why: "The slot of level 6 (the finale, OFF tonight: design.n41.finale41): while locked it reads REBUILDING instead of LOCKED (DECIDE41 D-7).",
  rebuildText: "REBUILDING",
  rebuildText_why: "DECIDE41 D-7's word for level 6's locked slot: it is being rebuilt, not missing.",
};

const anchor = /("select41": \{\r?\n(\s*)"_why": [^\n]*\n\s*"enabled": false,\r?\n\s*"enabled_why": "[^"]*")/;
const m = raw.match(anchor);
if (!m) throw new Error("the select41 row was not found");
const ind = m[2];
let text = "";
for (const k of Object.keys(add)) {
  const v = add[k];
  if (v && typeof v === "object") {
    const ks = Object.keys(v);
    text += "," + eol + ind + JSON.stringify(k) + ": {";
    ks.forEach((q, i) => (text += eol + ind + "  " + JSON.stringify(q) + ": " + JSON.stringify(v[q]) + (i < ks.length - 1 ? "," : "")));
    text += eol + ind + "}";
  } else text += "," + eol + ind + JSON.stringify(k) + ": " + JSON.stringify(v);
}
raw = raw.replace(anchor, m[1] + text);

const d1 = JSON.parse(raw);
if (JSON.stringify(d1.design.n41.select41.bosses) !== JSON.stringify(bosses)) throw new Error("bosses mismatch");
for (const k of Object.keys(add)) delete d1.design.n41.select41[k];
if (JSON.stringify(d1) !== JSON.stringify(d0)) throw new Error("something besides select41 changed");
fs.writeFileSync(file, raw);
fs.copyFileSync(file, copy);
console.log("select41: " + Object.keys(add).filter((k) => !k.endsWith("_why")).join(", "));
