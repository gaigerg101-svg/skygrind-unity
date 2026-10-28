// NIGHT38 builder F (NIGHT38-BRIEF card F): the Player's rows for builder F's two levels in Design/unity-config.json, then the file copied to
// Assets/StreamingAssets/Design/unity-config.json (byte-identical). L-049: text edits only, never a whole-file JSON.stringify; idempotent. Edits:
//   1. design.maps.list: an n38-foundry line right after n37-foundry's, an n38-finale line right after n35-finale's (the level each replaces);
//   2. design.n31.unlocks.levels.rows: an n38-foundry row (n 1, the KESTREL only) after n37-foundry's, an n38-finale row (n 6) after n35-finale's;
//   3. `--on <row>` / `--off <row>` (foundry38 / finale38 only): that row's `enabled` (turned on only when its gates are GREEN on E's checker).
// Run from the repo (or worktree) root: node Tools/night/n38f-config.cjs [--on foundry38] [--on finale38] [--off ...]
const fs = require("fs");
const path = require("path");
const CFG = path.join(".", "Design/unity-config.json");
const SA = path.join(".", "Assets/StreamingAssets/Design/unity-config.json");
const MINE = ["foundry38", "finale38"];

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
const eol = t.includes("\r\n") ? "\r\n" : "\n";

// 1. design.maps.list
function mapLine(afterId, id, label) {
  if (t.includes(`{ "id": "${id}"`)) return;
  if (!fs.existsSync(path.join(".", `Design/levels/${id}.json`))) { console.log(`maps.list: ${id} skipped (no Design/levels/${id}.json yet)`); return; }
  const re = new RegExp(`\\{ "id": "${afterId}", "label": "[^"]*", "file": "Design/levels/${afterId}\\.json", "campaignOnly": true \\},?`);
  const m = re.exec(t);
  if (!m || t.split(m[0]).length !== 2) throw new Error(`maps.list: ${afterId}'s line not found exactly once`);
  const at = m.index, a = m[0];
  const indent = t.slice(t.lastIndexOf("\n", at) + 1, at);
  const line = `{ "id": "${id}", "label": "${label}", "file": "Design/levels/${id}.json", "campaignOnly": true }`;
  if (a.endsWith(",")) t = t.slice(0, at + a.length) + eol + indent + line + "," + t.slice(at + a.length);
  else t = t.slice(0, at + a.length) + "," + eol + indent + line + t.slice(at + a.length);
}
mapLine("n37-foundry", "n38-foundry", "LEVEL 1 THE FOUNDRY");
mapLine("n35-finale", "n38-finale", "LEVEL 6 THE LAST LINE");

// 2. design.n31.unlocks.levels.rows
function unlockRow(afterId, n, id, note) {
  if (new RegExp(`"id": "${id}",\\s*"note"`).test(t)) return;
  if (!fs.existsSync(path.join(".", `Design/levels/${id}.json`))) { console.log(`unlocks: ${id} skipped (no level file yet)`); return; }
  const idAt = t.search(new RegExp(`"n": ${n},\\s*"id": "${afterId}",\\s*"note"`));
  if (idAt < 0) throw new Error(`unlocks: ${afterId}'s row not found`);
  const open = t.lastIndexOf("{", idAt), close = closeOf(t, open);
  const objIndent = t.slice(t.lastIndexOf("\n", open) + 1, open);
  const inner = objIndent + "  ";
  const row = ["{", `${inner}"n": ${n},`, `${inner}"id": "${id}",`, `${inner}"note": ${JSON.stringify(note)}`, `${objIndent}}`].join(eol);
  t = t.slice(0, close + 1) + "," + eol + objIndent + row + t.slice(close + 1);
}
unlockRow("n37-foundry", 1, "n38-foundry", "LEVEL 1 THE FOUNDRY, flight (NIGHT38 F, design.n38.foundry38 swaps n37-foundry for it): n37-foundry's rooms a1-a6 and THE WICK's a7-a9, with THE YARD (an open roofed arena) and THE GANTRY (two ride-chain gates) after the CRUCIBLE. The KESTREL only.");
unlockRow("n35-finale", 6, "n38-finale", "LEVEL 6 THE LAST LINE rebuilt for flight (NIGHT38 F, design.n38.finale38 swaps n35-finale for it): two open arenas and four gates, one per verb in sequence (ride, boost, grapple, dash jump); THE STAG's room as n35-finale's. Starts with the earns of n 1-5, as n35-finale; earns nothing.");

// 3. --on / --off <row>
const args = process.argv.slice(2);
for (let i = 0; i < args.length - 1; i++) {
  if (args[i] !== "--on" && args[i] !== "--off") continue;
  const row = args[i + 1];
  if (!MINE.includes(row)) throw new Error(`not builder F's row: ${row}`);
  const n38 = t.indexOf("\"n38\": {");
  if (n38 < 0) throw new Error("no design.n38 block: merge port/stage3 first");
  const n38Close = closeOf(t, t.indexOf("{", n38));
  const key = t.indexOf(`"${row}": {`, n38);
  if (key < 0 || key > n38Close) throw new Error(`no ${row} row inside design.n38`);
  const open = t.indexOf("{", key), close = closeOf(t, open);
  const body = t.slice(open, close + 1);
  const want = args[i] === "--on" ? "true" : "false";
  const nb = body.replace(/"enabled": (true|false)/, `"enabled": ${want}`);
  if (nb === body && !body.includes(`"enabled": ${want}`)) throw new Error(`${row}: no enabled key`);
  t = t.slice(0, open) + nb + t.slice(close + 1);
}

const c = JSON.parse(t);
fs.writeFileSync(CFG, t);
fs.writeFileSync(SA, t);
console.log("maps.list n38-foundry:", c.design.maps.list.some((m) => m.id === "n38-foundry"), "n38-finale:", c.design.maps.list.some((m) => m.id === "n38-finale"));
console.log("unlocks rows n38-foundry:", c.design.n31.unlocks.levels.rows.some((r) => r.id === "n38-foundry"), "n38-finale:", c.design.n31.unlocks.levels.rows.some((r) => r.id === "n38-finale"));
console.log("foundry38 enabled", c.design.n38.foundry38.enabled, "; finale38 enabled", c.design.n38.finale38.enabled, "; copied to", SA);
