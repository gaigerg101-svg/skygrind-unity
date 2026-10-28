// NIGHT63 builder V (leftover 7, C2 the enemy glow up close): writes ONLY builder V's view row design.n63.nearFade63 (no `sim`, no tape bit) as
// the last key of design.n63 (after colours63, the A1 block's last row) in Design/unity-config.json and its byte copy
// Assets/StreamingAssets/Design/unity-config.json, then asserts nothing else changed (the parsed file minus the new row equals the old one, keys,
// values and order) and that the two copies are byte-identical (L-049, L-065). Run once; a row already there is refused.
//   node Tools/night/n63-v-c2-rows.cjs [--dry]
//   node Tools/night/n63-v-c2-rows.cjs --numbers [--dry]   NIGHT63 (the Icebreaker at the eye): replaces ONLY design.n63.nearFade63.numbers with
//                                                         this script's (other rows may follow nearFade63 by then: press63); nothing else changes
// The row is read with NearFade63.On(cfg) (NightRows.ViewOn) once a frame in EnemyLook.Frame; -skygrindOff63 (or older) and -skygrindNoNearFade63
// turn it off.
const fs = require("fs");
const path = require("path");

const dry = process.argv.includes("--dry");
const numbersOnly = process.argv.includes("--numbers");
const name = "nearFade63";
const root = path.join(__dirname, "..", "..");
const files = [path.join(root, "Design", "unity-config.json"), path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json")];

const ROW = {
  _why: "View row (builder V, NIGHT63 leftover 7, C2 'the enemy glow up close'; gg-proxy-start.md #9: 'enemies read as glow domes and flat beans', GG 2026-10-06 'effects ... still look a little cheesy' and 2026-10-16 'better-looking visors'): an enemy never fills the screen from inside. Measured red (Tools/night/n63-v-near.py on n62-freeze's verbs37): an enemy at the eye in 348 of 9000 frames, in runs up to 43 frames; vf/f0400 is 77 % enemy hue: the eye at husk 2's capsule (a screen-filling dark-red dome with an orange rim) and husk 3 behind it drawn as its x-ray silhouette (a flat red bean: the x-ray pass draws where a nearer body hides it). The rule: each live enemy part dissolves through the enemy shader's own cell-noise dissolve (_Dissolve: the death's, 2.5 cm cells, clipped in every pass, the depth passes included) as the eye comes within numbers.farM of the part's world bounds, down to numbers.floor at numbers.nearM or nearer (inside the bounds: 0 m), so the room shows through it, and an enemy behind it draws as itself, not as an x-ray bean (the dissolved body writes no depth). Read once a frame (EnemyLook.Frame), set per part (EnemyLook.Tint). Game layer only: the sim never reads it (no tape bit; rays and hitboxes untouched). Off: -skygrindOff63 (or older) or -skygrindNoNearFade63. The web has no such row: its enemies never pass this close in its reels.",
  enabled: false,
  enabled_why: "Off until it passes NIGHT63.md's rules in a Player: the near63 reel's frame N63-near-0.5 (a husk 0.5 m from the eye) changes >= 41,472 px on its pair (n63-v-near.py --pair; the enemy-hue share falls on vs off), the SPOT names the enemy or its see-through; the far frames (N63-near-3.0) unchanged; verbs37 x5 unchanged; then a skygrind-critic PASS whose SPOT line names its visible change.",
  numbers: {
    _why: "The near fade's distances (m, from the eye to the part's world bounds: 0 inside) and its strongest dissolve. A husk is 1.8 m tall with a 0.4 m body radius; the eye is at 1.6 m; a punch lands from about 1.5 m, so the fade starts inside that.",
    farM: 1.0,
    farM_why: "m: the part is whole at this distance from its bounds or more (the punch's range and every shot stay untouched).",
    nearM: 0.3,
    nearM_why: "m: the part is at its floor at this distance or nearer, and inside its bounds (vf/f0400's eye was about 0.45 m from husk 2's bounds: dissolve 0.21 there).",
    floor: 0.1,
    floor_why: "The shader's _Dissolve at the eye (1 whole, 0 gone): a tenth of the body's 2.5 cm cells stay, so the enemy reads as a sparse screen-door ghost at the eye, never a wall of colour and never a vanished threat.",
    bossHideM: 0.6,
    bossHideM_why: "m: a boss part drawn with a material that has no dissolve (THE ICEBREAKER's Stone / URP Lit boxes) stops drawing while the eye is within this of its world bounds, and draws again once the eye is clear (NearFade63.HideNear). The lead's real-play bug: on b1-b4 boss63's iceline flinch / stagger frames were black but for the HUD (its leap lands on the player; its stuck pose leans the dark violet torso 50 deg toward the eye: the torso's centre about 0.9 m and the hump's about 0.46 m from the eye). 0.6 m hides the part the eye is in or against and keeps the rest of the body (the legs, the head, the stuck fist) and the room in view.",
  },
};

if (numbersOnly) {
  // replace ONLY design.n63.nearFade63.numbers (brace-matched, strings skipped), then assert nothing else changed
  const raw0 = fs.readFileSync(files[0], "utf8");
  if (raw0 !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
  const before0 = JSON.parse(raw0);
  if (!before0.design.n63 || !before0.design.n63[name]) throw new Error("no design.n63." + name + " (run without --numbers first)");
  const rowAt = raw0.indexOf('"' + name + '": {');
  if (rowAt < 0 || raw0.indexOf('"' + name + '": {', rowAt + 1) >= 0) throw new Error("design.n63." + name + " not found exactly once");
  const key = '"numbers": ';
  const numAt = raw0.indexOf(key, rowAt);
  if (numAt < 0) throw new Error("its numbers not found");
  const open = numAt + key.length;
  if (raw0[open] !== "{") throw new Error("numbers does not open with {");
  let depth = 0, inStr = false, end = -1;
  for (let k = open; k < raw0.length; k++) {
    const ch = raw0[k];
    if (inStr) { if (ch === "\\") { k++; continue; } if (ch === '"') inStr = false; continue; }
    if (ch === '"') { inStr = true; continue; }
    if (ch === "{") depth++;
    else if (ch === "}") { depth--; if (depth === 0) { end = k + 1; break; } }
  }
  if (end < 0) throw new Error("numbers' closing brace not found");
  const eol0 = raw0.includes("\r\n") ? "\r\n" : "\n";
  const lineStart = raw0.lastIndexOf("\n", numAt) + 1;
  const indent = raw0.slice(lineStart, numAt);
  if (!/^ *$/.test(indent)) throw new Error("unexpected text before the numbers key on its line");
  const blk = JSON.stringify(ROW.numbers, null, 2).split("\n").map((l, n) => (n === 0 ? l : indent + l)).join(eol0);
  const out0 = raw0.slice(0, open) + blk + raw0.slice(end);
  const after0 = JSON.parse(out0);
  if (JSON.stringify(after0.design.n63[name].numbers) !== JSON.stringify(ROW.numbers)) throw new Error("the numbers not written as the script says");
  after0.design.n63[name].numbers = before0.design.n63[name].numbers;
  if (JSON.stringify(after0) !== JSON.stringify(before0)) throw new Error("something outside design.n63." + name + ".numbers changed");
  if (dry) { console.log("n63-v-c2-rows --numbers --dry: design.n63." + name + ".numbers would be written (" + out0.length + " chars, was " + raw0.length + ")"); process.exit(0); }
  for (const f of files) fs.writeFileSync(f, out0);
  if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
  console.log("n63-v-c2-rows --numbers: design.n63." + name + ".numbers written (" + Object.keys(ROW.numbers).filter((k) => !k.endsWith("_why")).join(", ") + "); enabled " + after0.design.n63[name].enabled + "; both copies byte-identical; nothing else changed");
  process.exit(0);
}

const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const before = JSON.parse(raw);
if (!before.design.n63) throw new Error("no design.n63 (the lead's A1 commit writes the block first)");
if (before.design.n63[name]) throw new Error("design.n63." + name + " already exists (already run?)");
if (Object.keys(before.design).pop() !== "n63") throw new Error("design.n63 is not design's last key");
const keys0 = Object.keys(before.design.n63);
if (keys0.indexOf("colours63") < 0) throw new Error("design.n63 has no colours63 (A1's last row)");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const tail = eol + "    }" + eol + "  }" + eol + "}"; // the end of design.n63, of design, of the file
const rawTrim = raw.endsWith(eol) ? raw.slice(0, raw.length - eol.length) : raw;
const trailing = raw.length - rawTrim.length;
if (!rawTrim.endsWith(tail)) throw new Error("unexpected config ending");
const block = JSON.stringify(ROW, null, 2).split("\n").map((l, i) => (i === 0 ? l : "      " + l)).join(eol);
const out = rawTrim.slice(0, rawTrim.length - tail.length) + "," + eol + "      \"" + name + "\": " + block + tail + raw.slice(raw.length - trailing);

const after = JSON.parse(out);
const keys = Object.keys(after.design.n63);
if (keys[keys.length - 1] !== name) throw new Error(name + " is not design.n63's last key");
if (JSON.stringify(after.design.n63[name]) !== JSON.stringify(ROW)) throw new Error("design.n63." + name + " not written as the script says");
delete after.design.n63[name];
if (JSON.stringify(after) !== JSON.stringify(before)) throw new Error("something outside design.n63." + name + " changed (keys, values or order)");
if (dry) {
  console.log("n63-v-c2-rows --dry: design.n63." + name + " would be added after design.n63." + keys[keys.length - 2] + " (" + out.length + " chars, was " + raw.length + ")");
  process.exit(0);
}
for (const f of files) fs.writeFileSync(f, out);
if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
console.log("n63-v-c2-rows: design.n63." + name + " written after design.n63." + keys[keys.length - 2] + ": a view row, enabled false (both copies byte-identical; nothing else changed)");
