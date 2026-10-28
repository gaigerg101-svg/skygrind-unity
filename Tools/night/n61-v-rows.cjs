// NIGHT61 builder V (slate S1, the exit frame): writes ONLY design.n61.exitFrame61 (a VIEW row: no `sim`, no tape bit) as the last key of the
// existing design.n61 block (the lead's A1 commit wrote it with a _why only; another builder's n61 row may stand before it) into
// Design/unity-config.json and its byte copy Assets/StreamingAssets/Design/unity-config.json, then asserts nothing else changed (the parsed file
// minus design.n61.exitFrame61 equals the old one, keys, values and order) and the two copies are byte-identical (L-049). ExitFrame61Math.OffFor
// reads it with NightRows.ViewOn(cfg, "exitFrame61"); -skygrindOff61 (or older), -skygrindNoExitFrame61 and -skygrindNoExit58 turn it off. No
// number in the config: the exit frame's numbers are Unity-only (UnityTuning.N61V.cs). The web has no such row (it has no exit lamp).
//   node Tools/night/n61-v-rows.cjs [--dry]   (refuses to run twice; after a pick that conflicts in the config, re-run it on the merged copies)
const fs = require("fs");
const path = require("path");

const dry = process.argv.includes("--dry");
const root = path.join(__dirname, "..", "..");
const files = [path.join(root, "Design", "unity-config.json"), path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json")];

const EXITFRAME61 = {
  _why: "View row (builder V, NIGHT61 slate S1; GG 2026-10-13 doors 'not visible now and you just run through it and it's like dark'; NIGHT60's lamp60 own gain on the door 1.02-1.14x, Logs/n60/b8a2-lamp.txt): THE EXIT FRAME, lamp60 redesigned as a new view path (Game/ExitFrame61.N61V.cs; Exit58.N58V.cs, lamp60's path, and its numbers unchanged). On n57-kiln each door a room's clear opens (a gated campaign door, not into the boss room) lights cyan when it opens: a frame of emissive edge bars (two posts and a lintel, UnityTuning.ExitFrame61BarW wide) just in front of room A's face of the passage, a point light in the passage hung from a lit gate at its far mouth (its fixture), and a dashed floor line from room A's floor through the doorway, brightening toward the door and clipped short of the first thing on its path. Dark while the door is locked; one rise over UnityTuning.ExitFrame61RiseS when it opens (never a flash); it holds until the door locks again behind the player. Cyan = player-usable (CLAUDE.md rule 8). Off: -skygrindOff61 (or older), -skygrindNoExitFrame61 (this row alone) or -skygrindNoExit58 (no exit lamp at all), so the slate's 'on vs -skygrindNoExit58' measures this row as written in a build where lamp60 is off; turn this row on alone (both rows on draw both). Proof line in the Player log: '[exit61] door lit: room <id> (door <door>, frame <n>)'. View only: no tape bit; its numbers are Unity-only (UnityTuning.N61V.cs). The web has no exit lamp.",
  enabled: false,
  enabled_why: "Off until slate S1 passes NIGHT61.md section 2's visible rule in a Player: the lamp's own gain on the door (Tools/night/n60-v-lamp.py, door luma on / off) >= 1.5x in >= 4 of n57-kiln's 5 rooms, on vs -skygrindNoExit58 in the same build; its GUARD (the room's p95 luma outside the door's box within 0.02 of off) 5 / 5; every clear shot's a157 line reading 'room cleared yes'; the SPOT naming the door or the exit; verbs37 x5 27 / 29 / 31 / 31 / 31 with 0 NREs; then a skygrind-critic PASS.",
  numbers: {},
};

const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const before = JSON.parse(raw);
if (!before.design.n61) throw new Error("no design.n61 (the lead's A1 commit writes the block first)");
if (before.design.n61.exitFrame61) throw new Error("design.n61.exitFrame61 already exists (already run?)");
if (Object.keys(before.design).pop() !== "n61") throw new Error("design.n61 is not design's last key");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const tail = eol + "    }" + eol + "  }" + eol + "}"; // the end of design.n61, of design, of the file
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
const block = JSON.stringify(EXITFRAME61, null, 2).split("\n").map((l, i) => (i === 0 ? l : "      " + l)).join(eol);
const out = raw.slice(0, raw.length - tail.length) + "," + eol + "      \"exitFrame61\": " + block + tail;

const after = JSON.parse(out);
const keys = Object.keys(after.design.n61);
if (keys[keys.length - 1] !== "exitFrame61") throw new Error("exitFrame61 is not design.n61's last key");
if (JSON.stringify(after.design.n61.exitFrame61) !== JSON.stringify(EXITFRAME61)) throw new Error("design.n61.exitFrame61 not written as the script says");
delete after.design.n61.exitFrame61;
if (JSON.stringify(after) !== JSON.stringify(before)) throw new Error("something outside design.n61.exitFrame61 changed (keys, values or order)");
if (dry) {
  console.log("n61-v-rows --dry: design.n61.exitFrame61 would be added after design.n61." + keys[keys.length - 2] + " (" + out.length + " chars, was " + raw.length + ")");
  process.exit(0);
}
for (const f of files) fs.writeFileSync(f, out);
if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
console.log("n61-v-rows: design.n61.exitFrame61 written after design.n61." + keys[keys.length - 2] + ": a view row, enabled false (both copies byte-identical; nothing else changed)");
