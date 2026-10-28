// NIGHT37 builder W (NIGHT37.md W1): writes builder W's own row design.n37.wick37 in Design/unity-config.json and copies the file to
// Assets/StreamingAssets/Design/unity-config.json. L-049: text edits only (never a whole-file JSON.stringify); idempotent. Three edits:
//   1. design.n37.wick37: the row's text replaced (`enabled`, levelFrom, levelTo and any key the lead added are kept): the stack, the slag, the hot
//      wall, the boss, the pilot's numbers, each with its _why;
//   2. design.maps.list: an n37-foundry line right after n36-foundry's (mirror the level it replaces);
//   3. design.n31.unlocks.levels.rows: an n37-foundry row right after n36-foundry's (n 1: the KESTREL only).
// Run from the repo (or worktree) root: node Tools/night/n37w-config.cjs [root]
const fs = require("fs");
const path = require("path");
const ROOT = process.argv[2] || ".";
const CFG = path.join(ROOT, "Design/unity-config.json");
const SA = path.join(ROOT, "Assets/StreamingAssets/Design/unity-config.json");
const ROW = require("./n37w-row.cjs");

function eolOf(t) { return t.includes("\r\n") ? "\r\n" : "\n"; }
function closeOf(t, open, o = "{", c = "}") {
  let depth = 0, inStr = false;
  for (let i = open; i < t.length; i++) {
    const ch = t[i];
    if (inStr) { if (ch === "\\") i++; else if (ch === "\"") inStr = false; continue; }
    if (ch === "\"") inStr = true;
    else if (ch === o) depth++;
    else if (ch === c && --depth === 0) return i;
  }
  throw new Error("unbalanced");
}

let t = fs.readFileSync(CFG, "utf8");
const eol = eolOf(t);

// 1. the wick37 row
{
  const n37 = t.indexOf("\"n37\": {");
  if (n37 < 0) throw new Error("no design.n37 block: merge port/stage3 first");
  const n37Close = closeOf(t, t.indexOf("{", n37));
  const key = t.indexOf("\"wick37\": {", n37);
  if (key < 0 || key > n37Close) throw new Error("no wick37 row inside design.n37");
  const open = t.indexOf("{", key), close = closeOf(t, open);
  const old = JSON.parse(t.slice(open, close + 1));
  const indent = t.slice(t.lastIndexOf("\n", key) + 1, key);
  const row = { _why: old._why, enabled: old.enabled === true, levelFrom: old.levelFrom, levelTo: old.levelTo };
  for (const [k, v] of Object.entries(ROW)) if (!(k in row)) row[k] = v;
  for (const [k, v] of Object.entries(old)) if (!(k in row)) row[k] = v;
  const body = JSON.stringify(row, null, 2).split("\n").map((l, i) => (i === 0 ? l : indent + l)).join(eol);
  t = t.slice(0, open) + body + t.slice(close + 1);
}

// 2. design.maps.list
if (!t.includes("{ \"id\": \"n37-foundry\"")) {
  const a = "{ \"id\": \"n36-foundry\", \"label\": \"LEVEL 1 THE FOUNDRY\", \"file\": \"Design/levels/n36-foundry.json\", \"campaignOnly\": true },";
  const at = t.indexOf(a);
  if (at < 0 || t.indexOf(a, at + 1) >= 0) throw new Error("maps.list: n36-foundry's line not found exactly once");
  const indent = t.slice(t.lastIndexOf("\n", at) + 1, at);
  const line = "{ \"id\": \"n37-foundry\", \"label\": \"LEVEL 1 THE FOUNDRY\", \"file\": \"Design/levels/n37-foundry.json\", \"campaignOnly\": true },";
  t = t.slice(0, at + a.length) + eol + indent + line + t.slice(at + a.length);
}

// 3. design.n31.unlocks.levels.rows
if (!/"id": "n37-foundry",\s*"note"/.test(t)) {
  const idAt = t.search(/"n": 1,\s*"id": "n36-foundry",\s*"note"/);
  if (idAt < 0) throw new Error("unlocks: n36-foundry's row not found");
  const open = t.lastIndexOf("{", idAt), close = closeOf(t, open);
  const objIndent = t.slice(t.lastIndexOf("\n", open) + 1, open);
  const inner = objIndent + "  ";
  const row = ["{", `${inner}"n": 1,`, `${inner}"id": "n37-foundry",`, `${inner}"note": "LEVEL 1 THE FOUNDRY with THE WICK (NIGHT37 W, design.n37.wick37 swaps n36-foundry for it): n36-foundry's six rooms, two rehearsal rooms (the flue climb, the pierce from the wall) and THE STACK. The KESTREL only."`, `${objIndent}}`].join(eol);
  t = t.slice(0, close + 1) + "," + eol + objIndent + row + t.slice(close + 1);
}

JSON.parse(t);
fs.writeFileSync(CFG, t);
fs.writeFileSync(SA, t);
const c = JSON.parse(t);
console.log("wick37:", JSON.stringify(Object.keys(c.design.n37.wick37)), "enabled", c.design.n37.wick37.enabled);
console.log("maps.list n37-foundry:", c.design.maps.list.some((m) => m.id === "n37-foundry"));
console.log("unlocks row n37-foundry:", c.design.n31.unlocks.levels.rows.some((r) => r.id === "n37-foundry"));
console.log("copied to", SA);
