// NIGHT35 builder T: writes ONLY design.n35.teach35 (L-049) in Design/unity-config.json (the text of `"teach35": { ... }` inside `"n35": {`
// is replaced; the file is never re-serialised), then copies the file to Assets/StreamingAssets/Design/unity-config.json.
// Run from the repo root (the worktree or the main tree): node Tools/night/n35t-teach-row.cjs [--on]
//   --on: the row's enabled true (the lead flips it at integration); default keeps whatever `enabled` the file holds.
// The LINES are the teaching text table builder A reads (the trigger ids per level: A writes a rect per id into the level file's
// "campaign": {"teach35": [{"id", "rect": [x0, z0, x1, z1], "minY"?}]}; with no rect a line fires on its `room`'s first entry).
const fs = require("fs");
const path = require("path");
const file = path.join(process.cwd(), "Design", "unity-config.json");
const copy = path.join(process.cwd(), "Assets", "StreamingAssets", "Design", "unity-config.json");
const raw = fs.readFileSync(file, "utf8");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const d = JSON.parse(raw);
if (!d.design || !d.design.n35 || !d.design.n35.teach35) throw new Error("design.n35.teach35 missing (merge the shared commit first)");
const on = process.argv.includes("--on") ? true : d.design.n35.teach35.enabled === true;

const row = {
  _why: "Builder T (NIGHT35.md T1, D-7, PRINCIPLES 2): ULTRAKILL-style teaching text (Core/Lesson25.cs Teach35, Game/SurvivalSim.Teach35.cs, Ui teach35 in the hud33 message slot). A line fires once per PROFILE (Profile.teach35; a fresh profile sees every line, D-7) when the feet enter its rect (the level file's campaign.teach35, builder A) or, with no rect, on the first entry into its fallback `room`; a `weapon` line on that weapon's first weaponEarned. {action} tokens are the player's live button (pad glyph words or the bound key). Format (teach.md section 1, ULTRAKILL 0-1): the verb in capitals, the button, at most one rule. `needs`: the NIGHT35 block the words describe (off: textOff, or no line). The web has no tutorial (Unity-only words). Off (row off, --off35 / -skygrindOff35): nothing fires, NIGHT34's lessons only.",
  enabled: on,
  showS: 5,
  showS_why: "ULTRAKILL HudMessage timerTime 5 s (Research/2026-10-11-wave35/RESEARCH.md table: teaching text time 5 s, once).",
  maxChars: 52,
  maxChars_why: "The message slot is one 640 px line at 24 px (UnityTuning.Hud33SlotW, the slot's px): about 52 characters of the label face fit; the harness check teach35 resolves every line with the keyboard words (LEFT CLICK, L-SHIFT) and refuses a longer one.",
  lines: [
    { id: "l1.dash", level: "foundry", room: "f3", text: "PRESS {dash} to DASH. Works in the air." },
    { id: "l1.slide", level: "foundry", room: "f7", text: "HOLD {slide} to SLIDE. You keep your speed." },
    { id: "l1.fire", level: "foundry", room: "f8", text: "PRESS {fire} to FIRE the KESTREL." },
    { id: "l1.charge", level: "foundry", room: "f8", needs: "charge35", text: "HOLD {fire} to CHARGE. Release to fire.", textOff: "HOLD {fire} to CHARGE. The shot pierces." },
    { id: "l1.zoom", level: "foundry", room: "f9", text: "HOLD {altFire} to ZOOM. Aim on a wall slows time." },
    { id: "l2.wallride", level: "iceline", room: "i1a", needs: "wallride35", text: "HOLD {jump} on a WALL to WALL RIDE. Let go to leap.", textOff: "PRESS {jump} at a WALL to WALL RIDE." },
    { id: "l2.walljump", level: "iceline", room: "i2", text: "PRESS {jump} on a WALL to WALL JUMP. Chain them." },
    { id: "l3.charge", level: "kiln", room: "k1", needs: "charge35", text: "HOLD {fire} to CHARGE a SLUG. Release to fire.", textOff: "HOLD {fire} to CHARGE a SLUG. It cracks armour." },
    { id: "l4.grapple", level: "span", room: "s2", needs: "grapple35", text: "PRESS {grapple} to GRAPPLE. It pulls you to the hit.", textOff: "TAP {grapple} to PULL. HOLD to SWING." },
    { id: "l4.bend", level: "span", room: "s3", text: "LATCH with {grapple} to BEND time. SWAP guns in it." },
    { id: "l5.jumpdash", level: "spire", room: "v1", text: "PRESS {dash} then {jump} to JUMP DASH." },
    { id: "l5.whip", level: "spire", room: "v2", text: "AIM {grapple} at an enemy to WHIP. Then PUNCH." },
    { id: "w.shotgun", weapon: "shotgun", text: "THE THUNDERCLAP. PRESS {fire} to BLAST." },
    { id: "w.nailgun", weapon: "nailgun", text: "THE STITCHER. HOLD {fire} to STREAM nails." },
    { id: "w.gauntlet", weapon: "gauntlet", text: "PRESS {fire} to PUNCH. {altFire} to PARRY." },
    { id: "w.torch", weapon: "torch", text: "THE TORCH. HOLD {fire} to BURN." },
  ],
};

// the row's text: from `"teach35": {` inside "n35" to its matching brace
const n35At = raw.indexOf('"n35": {');
if (n35At < 0) throw new Error("no n35 block");
const keyAt = raw.indexOf('"teach35": {', n35At);
if (keyAt < 0) throw new Error("no teach35 row");
const open = raw.indexOf("{", keyAt);
let depth = 0, end = -1, inStr = false;
for (let i = open; i < raw.length; i++) {
  const c = raw[i];
  if (inStr) {
    if (c === "\\") i++;
    else if (c === '"') inStr = false;
    continue;
  }
  if (c === '"') inStr = true;
  else if (c === "{" || c === "[") depth++;
  else if (c === "}" || c === "]") {
    depth--;
    if (depth === 0) { end = i; break; }
  }
}
if (end < 0) throw new Error("unbalanced teach35 row");
const lineStart = raw.lastIndexOf("\n", keyAt) + 1;
const indent = raw.slice(lineStart, keyAt);
const text = JSON.stringify(row, null, 2).split("\n").map((l, i) => (i === 0 ? l : indent + l)).join(eol);
const out = raw.slice(0, open) + text + raw.slice(end + 1);
const check = JSON.parse(out);
if (JSON.stringify(check.design.n35.teach35) !== JSON.stringify(row)) throw new Error("patched row reads back different");
for (const k of Object.keys(d.design)) if (k !== "n35" && JSON.stringify(check.design[k]) !== JSON.stringify(d.design[k])) throw new Error("touched " + k);
for (const k of Object.keys(d.design.n35)) if (k !== "teach35" && JSON.stringify(check.design.n35[k]) !== JSON.stringify(d.design.n35[k])) throw new Error("touched n35." + k);
fs.writeFileSync(file, out);
fs.copyFileSync(file, copy);
console.log(`design.n35.teach35 written (enabled ${on}, ${row.lines.length} lines) and copied to StreamingAssets`);
