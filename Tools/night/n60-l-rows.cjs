// NIGHT60 builder L (B9 attempt 2, T-138): writes ONLY design.n60.focus60 (a VIEW row: no `sim`, no tape bit) as the last key of the existing
// design.n60 block (builder V's n60-v-rows.cjs wrote it with lamp60) into Design/unity-config.json and its byte copy
// Assets/StreamingAssets/Design/unity-config.json, then asserts nothing else changed (the parsed file minus design.n60.focus60 equals the old
// one, keys, values and order) and the two copies are byte-identical (L-049). SurvivalGame.N60L.cs reads it with NightRows.ViewOn(cfg,
// "focus60"); -skygrindOff60 (or older) and -skygrindNoFocus60 turn it off. No number. (Builder L's unpicked n60-b10 branch has a script of
// this name for idle60: the two touch different rows.)
//   node Tools/night/n60-l-rows.cjs [--dry]   (refuses to run twice)
const fs = require("fs");
const path = require("path");

const dry = process.argv.includes("--dry");
const root = path.join(__dirname, "..", "..");
const files = [path.join(root, "Design", "unity-config.json"), path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json")];

const FOCUS60 = {
  _why: "View row (builder L, B9 attempt 2; T-138): the window losing the focus pauses a Playing run (SurvivalGame.N60L.cs: PausesOnFocusLoss, called beside SurvivalGame.cs's lock-drop rule; not in the attract, a reel or -skygrindBench; the smoke's SmokeGuard55 resolves it as it resolves any pause it did not make). The Player probe (-skygrindFocusProbe60) found the lock-drop rule blind in a standalone Player: Cursor.lockState still reads Locked after a focus loss, so alt-tab never paused (Logs/n60/b9/focus-n60-b7-green.txt: focus LOST, cursor Locked, pauses 0). Game layer only: the sim never reads it (no tape bit). Off: -skygrindOff60 (or older) or -skygrindNoFocus60. The web has no such row: it has no window-focus rule (a browser tab's pointer lock is released by the page).",
  enabled: false,
  enabled_why: "Off until its MAIN (FocusProbe60's DONE line True with focus60 on and False with it off in the same build: n60-focus-probe.sh with the row on in the build's copy, then with -skygrindNoFocus60) and its GUARDS (the planted -skygrindFocusProbe60NoPause run False with the row on; the Player smoke DONE with the row on; 0 NREs) pass, then the one-row critic (NIGHT57.md section 0's rule).",
  numbers: {},
};

const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const before = JSON.parse(raw);
if (!before.design.n60) throw new Error("no design.n60 (builder V's n60-v-rows.cjs writes the block first)");
if (before.design.n60.focus60) throw new Error("design.n60.focus60 already exists (already run?)");
if (Object.keys(before.design).pop() !== "n60") throw new Error("design.n60 is not design's last key");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const tail = eol + "    }" + eol + "  }" + eol + "}"; // the end of design.n60, of design, of the file
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
const block = JSON.stringify(FOCUS60, null, 2).split("\n").map((l, i) => (i === 0 ? l : "      " + l)).join(eol);
const out = raw.slice(0, raw.length - tail.length) + "," + eol + "      \"focus60\": " + block + tail;

const after = JSON.parse(out);
const keys = Object.keys(after.design.n60);
if (keys[keys.length - 1] !== "focus60") throw new Error("focus60 is not design.n60's last key");
if (JSON.stringify(after.design.n60.focus60) !== JSON.stringify(FOCUS60)) throw new Error("design.n60.focus60 not written as the script says");
delete after.design.n60.focus60;
if (JSON.stringify(after) !== JSON.stringify(before)) throw new Error("something outside design.n60.focus60 changed (keys, values or order)");
if (dry) {
  console.log("n60-l-rows --dry: design.n60.focus60 would be added after design.n60." + keys[keys.length - 2] + " (" + out.length + " chars, was " + raw.length + ")");
  process.exit(0);
}
for (const f of files) fs.writeFileSync(f, out);
if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
console.log("n60-l-rows: design.n60.focus60 written after design.n60." + keys[keys.length - 2] + ": a view row, enabled false (both copies byte-identical; nothing else changed)");
