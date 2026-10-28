// NIGHT36 builder F: patches ONLY F's rows (design.n36.tap36 / crunch36 / muzzle36 / fist36) in Design/unity-config.json, in place (L-049), then
// copies the file to Assets/StreamingAssets/Design/unity-config.json. Each row keeps its own `enabled` and `_why` from the file; the keys below are
// added or replaced. Run from the worktree root: node Tools/night/n36-guns-config.cjs
const fs = require("fs");
const path = require("path");
const root = path.resolve(__dirname, "..", "..");
const file = path.join(root, "Design", "unity-config.json");
const copy = path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json");
let raw = fs.readFileSync(file, "utf8");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";

const rows = {
  tap36: {
    holdStartS: 0.18,
    holdStartS_why:
      "NIGHT36.md F1 (start 0.18 s): a press held longer than this (whole steps: 22 at 120 Hz) is a real hold and starts the charge (charge35Start, the HUD's CHARGING); shorter is a tap, which already fired ON its press. Above a deliberate click (50-100 ms, design.n35.charge35.tapS_why) with room for a slow pad trigger; under the KESTREL's 0.5 s full charge (design.n31.charge.revolver.fullSeconds, counted from the press as before), so the charge reads 0.32 s of filling. ULTRAKILL keeps the tap instant and the charge on its own input (rea-ultrakill-combat.md section 1, Revolver.cs:351-392): here one button, split by this time.",
  },
  crunch36: {
    bodyHitS: 0.018,
    bodyHitS_why:
      "NIGHT36.md F1 / REVIEW.md root cause 3 (a KESTREL body hit had no freeze, ImpactRules.cs:19-30): a 15-20 ms freeze on every non-kill gun body hit (18 ms: about one 60 fps frame, so it reads as a catch, not a stall). ULTRAKILL freezes 0.05 s on the revolver beam (REVIEW.md root cause 3); ours is shorter because it fires far more often (every pellet volley, every nail cycle).",
    bodyHitCooldownS: 0.15,
    bodyHitCooldownS_why:
      "The shortest real time between two body-hit freezes, so the STITCHER's stream (about 10 nails a second) freezes at most ~6.7 times a second: 6.7 x 0.018 = 0.12 s/s, inside design.impact.budget.perSecond 0.2 with room for a kill. The KESTREL (0.4 s interval) never meets it.",
    bodyBudgetPerSecond: 0.1,
    bodyBudgetPerSecond_why:
      "Body-hit freezes take at most this many real seconds in any rolling second (their own cap, half of design.impact.budget.perSecond 0.2, which still caps every freeze together); they never count toward the rolling minute's perMinute, so a long fight's body hits never starve the kill / headshot / punch freezes. Guess, logged.",
    punchHitS: 0.1,
    punchHitS_why:
      "ULTRAKILL: every punch hit freezes 0.1 s (REVIEW.md root cause 3; design.n36.crunch36._why: Punch.cs HitStop 0.1). Replaces design.impact.freeze.punchHit (0.06) while the row is on; the punch kill (punchKill 0.108) stays longer.",
  },
};

// the row's text span inside design.n36: from `"<row>": {` to its closing brace (strings skipped)
function span(text, row) {
  const n36 = text.indexOf('"n36": {');
  if (n36 < 0) throw new Error("no design.n36");
  const key = text.indexOf('"' + row + '": {', n36);
  if (key < 0) throw new Error("no design.n36." + row);
  const open = text.indexOf("{", key);
  let depth = 0, inStr = false;
  for (let i = open; i < text.length; i++) {
    const ch = text[i];
    if (inStr) {
      if (ch === "\\") i++;
      else if (ch === '"') inStr = false;
      continue;
    }
    if (ch === '"') inStr = true;
    else if (ch === "{") depth++;
    else if (ch === "}") {
      depth--;
      if (depth === 0) return [key, i + 1];
    }
  }
  throw new Error("unclosed row " + row);
}

const d = JSON.parse(raw);
for (const [row, keys] of Object.entries(rows)) {
  const obj = Object.assign({}, d.design.n36[row], keys);
  const [a, b] = span(raw, row);
  const lineStart = raw.lastIndexOf("\n", a) + 1;
  const indent = raw.slice(lineStart, a);
  const body = JSON.stringify(obj, null, 2).split("\n").map((l, i) => (i === 0 ? l : indent + l)).join(eol);
  raw = raw.slice(0, a) + '"' + row + '": ' + body + raw.slice(b);
}
const check = JSON.parse(raw);
for (const k of Object.keys(d.design)) if (k !== "n36" && JSON.stringify(d.design[k]) !== JSON.stringify(check.design[k])) throw new Error("touched design." + k);
for (const k of Object.keys(d.design.n36)) if (!(k in rows) && JSON.stringify(d.design.n36[k]) !== JSON.stringify(check.design.n36[k])) throw new Error("touched n36." + k);
fs.writeFileSync(file, raw);
fs.copyFileSync(file, copy);
console.log("patched design.n36: " + Object.keys(rows).join(", ") + "; copied to StreamingAssets");
