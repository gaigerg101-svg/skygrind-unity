// NIGHT46 builder F1: adds the sim row design.n46.stuck46 (enabled: false) as the LAST row of design.n46 (appended, so every earlier row keeps
// its registry index and tape bit), as text into Design/unity-config.json, then the StreamingAssets copy as a byte copy. Refuses if it exists.
//   node Tools/night/n46-f1-stuck.cjs [--dry]
const fs = require("fs");
const path = require("path");
const ROOT = path.resolve(__dirname, "..", "..");
const A = path.join(ROOT, "Design", "unity-config.json");
const B = path.join(ROOT, "Assets", "StreamingAssets", "Design", "unity-config.json");
const dry = process.argv.includes("--dry");

const ROW = {
  sim: true,
  _why: "Builder F1 (sim row; the lead's ask in NIGHT46 round 2, FLOW-AUDIT row 11 'end a burrower that leaves its room'): a BURROWER whose head cannot move toward the player's ground contact (Burrower43.MoveHead stops at lava, a pit or a wall; it surfaces only within triggerRange) is ended after stuckS, as rooms43 ends a body under its floor: alive false, no kill event, no points. n46flow's baseline (Research/2026-10-17-wave46/F1.md): kb3's last body a burrower at y -6 by the pour's far rim for 240 s in 5 of 10 seeds, the door shut. Code: Game/SurvivalSim.N46.Flow.cs (N46FlowStep).",
  enabled: false,
  enabled_why: "Off until the lead has looked at its frames and its gates pass (NIGHT46.md method).",
  numbers: {
    stuckS: 6,
    stuckS_why: "Six seconds of a hunting head that has not moved one step with the player's contact out of its triggerRange: a burrower crossing a room moves every step (huntSpeed), so only a head held by lava, a pit or a wall waits this long; at huntSpeed 7.5 m/s six seconds is 45 m of hunting, more than any room's width, and four times the burrower's rest (restSeconds 1.4) plus its tell (tellSeconds 0.8); short against the 240 s the door was held.",
  },
};

let text = fs.readFileSync(A, "utf8");
const eol = text.includes("\r\n") ? "\r\n" : "\n";
const at = text.indexOf('"n46": {');
if (at < 0) throw new Error("design.n46 not found");
if (JSON.parse(text).design.n46.stuck46) throw new Error("design.n46.stuck46 already exists (already run?)");
// the matching closing brace of design.n46 (strings skipped)
let i = text.indexOf("{", at), depth = 0, inStr = false, end = -1;
for (; i < text.length; i++) {
  const ch = text[i];
  if (inStr) { if (ch === "\\") i++; else if (ch === '"') inStr = false; continue; }
  if (ch === '"') inStr = true;
  else if (ch === "{") depth++;
  else if (ch === "}") { depth--; if (depth === 0) { end = i; break; } }
}
if (end < 0) throw new Error("design.n46's end not found");
const closeLineStart = text.lastIndexOf("\n", end) + 1;
const closeIndent = text.slice(closeLineStart, end);
const rowIndent = closeIndent + "  ";
let before = text.slice(0, closeLineStart).replace(/\s+$/, ""); // the last row's closing brace
const body = JSON.stringify(ROW, null, 2).split("\n").map((l, k) => (k === 0 ? l : rowIndent + l)).join(eol);
text = before + "," + eol + rowIndent + '"stuck46": ' + body + eol + text.slice(closeLineStart);
const back = JSON.parse(text).design.n46;
if (JSON.stringify(back.stuck46) !== JSON.stringify(ROW)) throw new Error("stuck46 did not round-trip");
const keys = Object.keys(back);
if (keys[keys.length - 1] !== "stuck46") throw new Error("stuck46 is not the last row");
if (dry) { console.log("n46-f1-stuck: design.n46.stuck46 planned as row " + keys.length + " of design.n46 (dry run, nothing written)"); process.exit(0); }
fs.writeFileSync(A, text);
fs.writeFileSync(B, text);
console.log("n46-f1-stuck: design.n46.stuck46 written (the last of " + keys.length + " keys) to Design/unity-config.json and its StreamingAssets copy");
