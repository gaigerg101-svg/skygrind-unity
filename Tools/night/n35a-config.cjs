// NIGHT35 builder A (NIGHT35.md A1, DECIDE35 D-1): writes builder A's own row design.n35.campaign6 in Design/unity-config.json and copies the file to
// Assets/StreamingAssets/Design/unity-config.json. L-049: it replaces ONLY the text of `"campaign6": { ... }` inside `"n35": {` (never a whole-file
// JSON.stringify), keeps the row's `enabled` as it finds it (the lead turns rows on), and checks the file still parses. Idempotent.
// Run from the repo (or worktree) root: node Tools/night/n35a-config.cjs [root]
const fs = require("fs");
const path = require("path");
const ROOT = process.argv[2] || ".";
const CFG = path.join(ROOT, "Design/unity-config.json");
const SA = path.join(ROOT, "Assets/StreamingAssets/Design/unity-config.json");

// the row's content (enabled is kept from the file)
const ROW = {
  _why: "NIGHT35 A (NIGHT35.md A1, DECIDE35 D-1; GG 2026-10-11: \"six levels total and we just have to polish them a ton\"; docs/PRINCIPLES.md 4: six levels, one new idea each). "
    + "Curriculum25.N35Levels (the lead's shared commit): applies levelSwap after N34Levels, inserts finaleId right after `after`, then drops every id after the finale (the legacy "
    + "levels leave the order; their files stay). The order with every older row on: L1 n35-foundry (fire, zoom, charge, slide, dash; the KESTREL), L2 n35-iceline (the Lucio "
    + "wall ride + wall jump; the PULSE gauntlet), L3 n35-kiln (the THUNDERCLAP), L4 n33-span (the grapple + the bend: design.n35.grapple35.fromLevel 4; the NAILGUN), L5 "
    + "n33-spire (the jump dash; THE CROWN; earns the TORCH), L6 n35-finale (three wall-ride platforming rooms and THE STAG on the rink: the exam, every verb). "
    + "A flow row (the level a run loads is on its tape); off: NIGHT34's order. Level files: Tools/night/gen-n35-*.cjs, bash Tools/night/n35-regen.sh.",
  after_why: "The last level before the finale, AFTER levelSwap (level 5: n33-spire; not swapped tonight).",
  after: "n33-spire",
  finaleId_why: "Level 6 (D-1): a SHORT finale, Design/levels/n35-finale.json from Tools/night/gen-n35-finale.cjs: three wall-ride platforming rooms built with the gen-n34 "
    + "generators' helpers (n30-lib / n33r-lib) and one boss room reusing THE STAG on the rink (boss id glacier: Core/Enemies/Rink28Arena.cs, SurvivalSim.Boss31; "
    + "not level 5's CROWN). A boss rush is a leftover.",
  finaleId: "n35-finale",
  levelSwap_why: "{old id: new id} after N34Levels: levels 1-3 get NIGHT35 A's files (the n34 file + a Lucio semi-platforming room + the teach35 trigger rects + the "
    + "D-3 grapple-node decisions, logged in each file's _why).",
  levelSwap: { "n34-foundry": "n35-foundry", "n34-iceline": "n35-iceline", "n34-kiln": "n35-kiln" },
};

function eolOf(t) { return t.includes("\r\n") ? "\r\n" : "\n"; }

function closeOf(t, open) {
  let depth = 0, inStr = false;
  for (let i = open; i < t.length; i++) {
    const c = t[i];
    if (inStr) { if (c === "\\") i++; else if (c === "\"") inStr = false; continue; }
    if (c === "\"") inStr = true;
    else if (c === "{") depth++;
    else if (c === "}" && --depth === 0) return i;
  }
  throw new Error("unbalanced braces");
}

let t = fs.readFileSync(CFG, "utf8");
const eol = eolOf(t);
const n35 = t.indexOf("\"n35\": {");
if (n35 < 0) throw new Error("no \"n35\": { block yet (the lead's shared commit adds it): merge port/stage3 first");
const n35Close = closeOf(t, t.indexOf("{", n35));
const key = t.indexOf("\"campaign6\": {", n35);
if (key < 0 || key > n35Close) throw new Error("no \"campaign6\": { row inside design.n35");
const open = t.indexOf("{", key);
const close = closeOf(t, open);
const old = JSON.parse(t.slice(open, close + 1));
const lineStart = t.lastIndexOf("\n", key) + 1;
const indent = t.slice(lineStart, key);
const row = {};
row._why = ROW._why;
row.enabled = old.enabled === true;
for (const [k, v] of Object.entries(ROW)) if (k !== "_why") row[k] = v;
for (const [k, v] of Object.entries(old)) if (!(k in row)) row[k] = v; // a key the lead added stays
const text = JSON.stringify(row, null, 2).split("\n").map((l, i) => (i === 0 ? l : indent + l)).join(eol);
t = t.slice(0, open) + text + t.slice(close + 1);
const parsed = JSON.parse(t);
const got = parsed.design.n35.campaign6;
if (got.finaleId !== ROW.finaleId || got.after !== ROW.after) throw new Error("campaign6 did not land");
fs.writeFileSync(CFG, t);
fs.mkdirSync(path.dirname(SA), { recursive: true });
fs.copyFileSync(CFG, SA);
if (fs.readFileSync(SA, "utf8") !== t) throw new Error("COPY MISMATCH " + SA);
console.log(`design.n35.campaign6 written (enabled ${row.enabled}), levelSwap ${Object.keys(row.levelSwap).length} ids, finale ${row.finaleId} after ${row.after}; copied to ${SA}`);
