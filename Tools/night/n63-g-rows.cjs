// NIGHT63 builder G (leftover 6, S7: kiln's presses read as machines): writes design.n63.press63 as design.n63's LAST key into
// Design/unity-config.json and its byte copy Assets/StreamingAssets/Design/unity-config.json (a VIEW row, enabled false, numbers {}: its
// numbers are Unity-only view geometry in UnityTuning.N63GS7.cs), then asserts nothing else changed (the parsed file minus
// design.n63.press63 equals the old one: keys, values and order), press63 is design.n63's last key, and the copies are byte-identical.
// Run once on a tree whose design.n63 has no press63; a second run is refused. On a pick onto a tree whose design.n63 gained other rows since,
// take HEAD's copies and run this script again (L-065).
//   node Tools/night/n63-g-rows.cjs [--dry]
const fs = require("fs");
const path = require("path");

const dry = process.argv.includes("--dry");
const root = path.join(__dirname, "..", "..");
const files = [path.join(root, "Design", "unity-config.json"), path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json")];

const ROW = {
  _why: "View row (builder G, NIGHT63 leftover 6 = NIGHT61's S7; gg-proxy at NIGHT63's start, item 1: 'kiln's presses still plain blocks' (Logs/n63/gg-proxy-start.md; NIGHT62-REPORT section 1 item 1); the N57 critic: 'the presses read as plain dark pillars ... the shape of GG's random pillars that serve no purpose'; GG 2026-10-12: 'random lava pits random pillars that serve no purpose'): KILN'S PRESSES READ AS MACHINES. Each drop press of n57-kiln (the level's own campaign boxes whose job is 'hero prop: a drop press', THE PRESS HALL's four charger stun posts) gets a machine dressed round its unchanged box: corner rails and a crossbeam (the frame), a chrome piston strip on each face, a heavy ram head riding up and down the column on its own clock (a slow lift, a hold, a hard drop, a short dwell: FeelClock time, each press out of phase), a striped plinth in bone and steel (no red-orange: a press does not hurt the player, CLAUDE.md rule 8; no yellow: yellow is the parry tell, DECIDE63 D-1) and a lamp in a housing on the crossbeam that lights while the head is up. View geometry only (Game/Press63.cs; numbers in UnityTuning.N63GS7.cs): no collider, no sim box, no nav or flow change; every part stays within the press's box grown by its proud margin. Read every frame (D1's contract). Off: -skygrindOff63 (or older) or -skygrindNoPress63. View only: no tape bit. The web has no such row: it has no n57-kiln.",
  enabled: false,
  enabled_why: "Off until it passes NIGHT63.md's rules in a Player: the SPOT on a same-run pair (an a157 n57-kiln kb3 entry frame, or a verbs37 vk frame in THE PRESS HALL) naming the presses or machines with >= 41,472 px changed; colliders unchanged (n46flow n57-kiln byte-equal with the row on: the sim never reads it); verbs37 n57-kiln 31 / 31; then a skygrind-critic PASS.",
  numbers: {},
};

const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const before = JSON.parse(raw);
if (!before.design || !before.design.n63) throw new Error("no design.n63 (NIGHT63's A1 first)");
if (before.design.n63.press63) throw new Error("design.n63.press63 already exists (already run?)");
const dk = Object.keys(before.design);
if (dk[dk.length - 1] !== "n63") throw new Error("design's last key is " + dk[dk.length - 1] + ", not n63");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const tail = eol + "    }" + eol + "  }" + eol + "}"; // the end of design.n63, of design, of the file
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
const block = JSON.stringify(ROW, null, 2).split("\n").map((l, i) => (i === 0 ? l : "      " + l)).join(eol);
const out = raw.slice(0, raw.length - tail.length) + "," + eol + "      \"press63\": " + block + tail;

const after = JSON.parse(out);
const nk = Object.keys(after.design.n63);
if (nk[nk.length - 1] !== "press63") throw new Error("press63 is not design.n63's last key");
if (JSON.stringify(after.design.n63.press63) !== JSON.stringify(ROW)) throw new Error("design.n63.press63 not written as the script says");
delete after.design.n63.press63;
if (JSON.stringify(after) !== JSON.stringify(before)) throw new Error("something outside design.n63.press63 changed (keys, values or order)");
if (dry) {
  console.log("n63-g-rows --dry: design.n63.press63 would be added after design.n63." + Object.keys(before.design.n63).pop() + " (" + out.length + " chars, was " + raw.length + ")");
  process.exit(0);
}
for (const f of files) fs.writeFileSync(f, out);
if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
console.log("n63-g-rows: design.n63.press63 written as design.n63's last key (a view row, enabled false, numbers {}; both copies byte-identical; nothing else changed)");
