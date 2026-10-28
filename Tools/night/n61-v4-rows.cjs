// NIGHT61 builder V (slate S4, lights hang from real fixtures): writes ONLY design.n61.fixture61 (a VIEW row: no `sim`, no tape bit) as the last key
// of the existing design.n61 block (the lead's A1 _why, then the rows already there: exitFrame61, bossRule61, ... are left as they are) into
// Design/unity-config.json and its byte copy Assets/StreamingAssets/Design/unity-config.json, then asserts nothing else changed (the parsed file
// minus design.n61.fixture61 equals the old one, keys, values and order) and the two copies are byte-identical (L-049). Fixture61Math.OffFor reads
// it with NightRows.ViewOn(cfg, "fixture61"); -skygrindOff61 (or older) and -skygrindNoFixture61 turn it off. No number in the config: the mounts'
// numbers are Unity-only (UnityTuning.N61V4.cs). The web has no such row (it draws no light fixture).
//   node Tools/night/n61-v4-rows.cjs [--dry]   (refuses to run twice; after a pick that conflicts in the config, re-run it on the merged copies)
const fs = require("fs");
const path = require("path");

const dry = process.argv.includes("--dry");
const root = path.join(__dirname, "..", "..");
const files = [path.join(root, "Design", "unity-config.json"), path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json")];

const FIXTURE61 = {
  _why: "View row (builder V, NIGHT61 slate S4; GG 2026-10-11 lights look 'placed and floating' and 'crappy' (OWNER.md); the N55 critic: 'bare white rectangles hang in the ceilings', fixed looks like 'a dark frame >= 4 px at 1080p and a hanger to the beam'; PRINCIPLES 7: lights hang from real fixtures): LIGHTS HANG FROM REAL FIXTURES (Game/Fixture61.N61V4.cs, Fixture61Math.N61V4.cs). On every campaign level with the light plan (look33), each ceiling light the plan draws (a room's key panel, a pool's downlight, a ceiling socket's panel) gets a housing (four dark walls UnityTuning.Fixture61RimW outside its frame, hanging Fixture61Drop under the ceiling) and a hanger rail to the nearest joist on each side (lights35's beams), else to the nearest wall face. View geometry only: no light moves, dims or flickers (the plan's lights and fixtures are untouched; the mounts cast no shadow), and every part stays within Rods40WallClear under its ceiling (rods40's play volume never meets one). Off: -skygrindOff61 (or older) or -skygrindNoFixture61 (this row alone). Proof line in the Player log: '[fixture61] mounts on <level>: ceiling lights <n>, housed <n>, held by a beam <n>, by a wall <n>, unmounted <n>'. View only: no tape bit; its numbers are Unity-only (UnityTuning.N61V4.cs). The web draws no light fixture.",
  enabled: false,
  enabled_why: "Off until slate S4 passes NIGHT61.md section 2's visible rule in a Player: unmounted light sources on the a357 entry frames of the five levels <= 10 % of the red (each light with a housing and a hanger to a beam or a wall); no light moves, dims or flickers more (the light54 log lines and the frame-to-frame luma of a still pose unchanged within 0.01); the SPOT on two a357 entry pairs naming the lamps or fixtures; verbs37 x5 unchanged; then a skygrind-critic PASS.",
  numbers: {},
};

const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const before = JSON.parse(raw);
if (!before.design.n61) throw new Error("no design.n61 (the lead's A1 commit writes the block first)");
if (before.design.n61.fixture61) throw new Error("design.n61.fixture61 already exists (already run?)");
if (Object.keys(before.design).pop() !== "n61") throw new Error("design.n61 is not design's last key");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const tail = eol + "    }" + eol + "  }" + eol + "}"; // the end of design.n61, of design, of the file
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
const block = JSON.stringify(FIXTURE61, null, 2).split("\n").map((l, i) => (i === 0 ? l : "      " + l)).join(eol);
const out = raw.slice(0, raw.length - tail.length) + "," + eol + "      \"fixture61\": " + block + tail;

const after = JSON.parse(out);
const keys = Object.keys(after.design.n61);
if (keys[keys.length - 1] !== "fixture61") throw new Error("fixture61 is not design.n61's last key");
if (JSON.stringify(after.design.n61.fixture61) !== JSON.stringify(FIXTURE61)) throw new Error("design.n61.fixture61 not written as the script says");
delete after.design.n61.fixture61;
if (JSON.stringify(after) !== JSON.stringify(before)) throw new Error("something outside design.n61.fixture61 changed (keys, values or order)");
if (dry) {
  console.log("n61-v4-rows --dry: design.n61.fixture61 would be added after design.n61." + keys[keys.length - 2] + " (" + out.length + " chars, was " + raw.length + ")");
  process.exit(0);
}
for (const f of files) fs.writeFileSync(f, out);
if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
console.log("n61-v4-rows: design.n61.fixture61 written after design.n61." + keys[keys.length - 2] + ": a view row, enabled false (both copies byte-identical; nothing else changed)");
