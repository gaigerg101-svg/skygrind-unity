// NIGHT43 builder K1 (kiln43): writes design.n43.kiln43 (level 3 as n43-kiln; its boost chain) in Design/unity-config.json and copies the file
// to Assets/StreamingAssets/Design/unity-config.json (the two byte-identical before and after). Patches ONLY the kiln43 row's text (L-049): the
// row from `"kiln43": {` to its closing brace at the same indent is replaced; `enabled` stays false (the lead turns it on after the frames).
// Re-runnable. Run from the repo (or a worktree) root: node Tools/night/k1-kiln43-config.cjs
const fs = require("fs");
const path = require("path");
const root = path.resolve(__dirname, "..", "..");
const file = path.join(root, "Design", "unity-config.json");
const copy = path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json");
let raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ before the patch");

const row = {
  _why: "Builder K1 (G25, G26, G27, G28; GG 2026-10-16: level 3's first room 'is confusing ... a strange ramp'; the boost tutorial 'should introduce the mechanic using smaller, easier jumps first'; 'Use more flying enemies, including the charging variants'; 'worm-like enemies added to appropriate Level 3 arenas'): level 3 as n43-kiln (Tools/night/gen-n43-kiln.cjs; from n41-kiln, untouched): 3.1 THE STEPS (three boost steps, small then big, no ramp, no fight), the rosters by role (drones and the CHARGER in 3.2, the BURROWER introduced alone in the new 3.2c THE ASH BED and combined in 3.3), THE RING. Older systems read n43-kiln as n40-kiln (SurvivalSim.N43BaseId -> n41-kiln -> N41BaseId). The boost chain below replaces kiln40's on n43-kiln (SurvivalSim.N43.Kiln.cs). Research/2026-10-16-wave43/K1.md.",
  enabled: false,
  enabled_why: "Off until its check passes and the lead has looked at its before / after Player frames and iterated once (NIGHT43.md method 2). Best judged with boost43 on (the steps stand under both; boost43 makes them forgiving).",
  levelFrom: "n41-kiln",
  levelTo: "n43-kiln",
  chain: [
    { id: "jump", text: "STEP 1 · PRESS {jump} TO JUMP", done: "jump" },
    { id: "look", text: "STEP 1 · IN THE AIR, LOOK STRAIGHT DOWN", done: "lookDown" },
    { id: "blast", text: "STEP 1 · PRESS {fire}: THE BLAST LIFTS YOU", done: "top", room: "kb1", top: "s1" },
    { id: "together", text: "STEP 2 · {jump} AND {fire} TOGETHER: HIGHER", done: "top", room: "kb1", top: "s2" },
    { id: "again", text: "STEP 3 · AT THE TOP, PRESS {fire} AGAIN", done: "top", room: "kb1", top: "s3" },
    { id: "summary", text: "THE BOOST: JUMP, LOOK DOWN, FIRE, FIRE AGAIN", done: "seen" },
    { id: "flue", text: "THE FLUE · {fire}, {jump} AT THE TOP, {fire} AGAIN", done: "top", room: "kb1b", top: "climb" },
    { id: "slug", text: "HOLD {fire} ON THE GROUND: CHARGE A SLUG", done: "seen" },
    { id: "ash", text: "IT HUNTS THE FLOOR: JUMP, BOOST, OR A PLATE", done: "seen", at: "kb2c" },
    { id: "ash2", text: "HIT IT AS IT SURFACES: IT REELS", done: "seen", at: "kb2c" },
  ],
  chain_why: "K1 (G26; kiln40's chain shape, PRINCIPLES 2: one step shown when the last is done): the THUNDERCLAP's boost taught on THE STEPS: step 1 split into its three verbs (jump, look down, fire: kiln40's lines), step 2 asks the shot WITH the jump, step 3 the second shot at the top (two shots; one boost and the boost + the double fall short; a double pressed against the face is a wall kick-off backwards, so the double is taught in the open shaft of 3.1b), then the summary, 3.1b THE FLUE (THE CLIMB: shot, double, shot, 12 m) and the slug (kiln40's last line); then, on entering 3.2c THE ASH BED (`at`: a line waits for its room), the BURROWER's two lines (its room has no Campaign24 lesson: a lesson there could hold the door after the fight, G5). Each 'top' line is done when the feet stand on that step (room `room`, the level row's tops43[`top`], written by gen-n43-kiln.cjs), so the words follow the geometry. At most 52 characters resolved (teach35.maxChars); {jump} / {fire} resolve to the bound buttons.",
  lookDownDeg: 60,
  lookDownDeg_why: "kiln40's lookDownDeg (S1, NIGHT40): 'LOOK STRAIGHT DOWN' is done this far below level.",
  topSlackM: 0.3,
  topSlackM_why: "K1: a 'top' line is done with the feet within this many metres under the step's top (the grounded capsule stands on it; a hypothesis: the Mover's ground snap is well under 0.3 m).",
  lineFallbackSeconds: 20,
  lineFallbackSeconds_why: "kiln40's lineFallbackSeconds: a step not done in this time moves the chain on, so the text never sticks.",
  minLineSeconds: 1.2,
  minLineSeconds_why: "kiln40's minLineSeconds: a line stands at least this long even when its step is already done.",
  seenSeconds: 6,
  seenSeconds_why: "kiln40's seenSeconds: a line with nothing to do stands this long.",
};

const key = '"kiln43": {';
const at = raw.indexOf(key);
if (at < 0) throw new Error("design.n43.kiln43 not found");
if (raw.indexOf(key, at + 1) >= 0) throw new Error("two kiln43 rows");
const lineStart = raw.lastIndexOf("\n", at) + 1;
const indent = raw.slice(lineStart, at);
if (!/^ *$/.test(indent)) throw new Error("unexpected text before the row's key");
const eol = raw.slice(at + key.length, at + key.length + 2) === "\r\n" ? "\r\n" : "\n";
const closeRe = new RegExp("\\r?\\n" + indent + "\\}", "g");
closeRe.lastIndex = at;
const cm = closeRe.exec(raw);
if (!cm) throw new Error("the row's closing brace was not found");
const end = cm.index + cm[0].length;
const body = JSON.stringify(row, null, 2).split("\n").map((l, i) => (i === 0 ? l : indent + l)).join(eol);
const out = raw.slice(0, at) + '"kiln43": ' + body + raw.slice(end);
const parsed = JSON.parse(out); // still valid JSON
if (parsed.design.n43.kiln43.levelTo !== "n43-kiln") throw new Error("the row did not land at design.n43.kiln43");
for (const l of row.chain) if (l.text.replace(/\{jump\}/g, "SPACE").replace(/\{fire\}/g, "LMB").length > 52) throw new Error("line too long: " + l.text);
fs.writeFileSync(file, out);
fs.writeFileSync(copy, out);
console.log("design.n43.kiln43 written (" + (end - at) + " -> " + ('"kiln43": '.length + body.length) + " chars), copied to StreamingAssets");
