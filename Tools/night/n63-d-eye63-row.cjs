// NIGHT63 builder D (DECIDE63 D-10, M1's proposal 2: THE EASED EYE): writes design.n63.eye63 as design.n63's LAST key into
// Design/unity-config.json and its byte copy Assets/StreamingAssets/Design/unity-config.json (a VIEW row, enabled false, numbers {}: its numbers
// are Unity-only view numbers in UnityTuning.N63Eye.cs), then asserts nothing else changed (the parsed file minus design.n63.eye63 equals the
// old one: keys, values and order), eye63 is design.n63's last key, and the copies are byte-identical (L-049, L-065).
// Run once on a tree whose design.n63 has no eye63; a second run is refused. On a pick onto a tree whose design.n63 gained other rows since,
// take HEAD's copies and run this script again (L-065).
//   node Tools/night/n63-d-eye63-row.cjs [--dry]
const fs = require("fs");
const path = require("path");

const dry = process.argv.includes("--dry");
const root = path.join(__dirname, "..", "..");
const files = [path.join(root, "Design", "unity-config.json"), path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json")];

const ROW = {
  _why:
    "View row (builder D, NIGHT63 DECIDE63 D-10, M1's proposal 2; GG 2026-10-20: 'the movement needs to feel cleaner, more fluid, and less choppy'): THE EASED EYE. M1 (Logs/n63/m1.md) measured the camera's designed events on verbs37: the slide's eye ease (the web's 0.70 m, slide.cameraDrop, in 0.10 s, camera.eyeEaseTime: CameraFeel's MoveTowards) starts and stops at full speed in one frame (11.67 cm a frame at 60 fps), and so do the slide dip (UnityTuning.SlideDip, 0.15 m in 0.04 s) and the landing dip (UnityTuning.LandDipMaxN18, in 0.03 s): each a peak in n63-m1-chop.cjs's residual. With the row on, the view's eye height above the feet follows today's eye less today's dips through a critically damped spring (UnityTuning.Eye63FollowRate), with the eye's own ease a little quicker (UnityTuning.Eye63EaseTimeMult of camera.eyeEaseTime) so the crouch stays as quick (90 % of a slide's exit in 0.10 s as today, half-way one frame later). Its start and stop ease in: on NIGHT62's freeze telemetry replayed offline, the eye's own residual halved at every slide entry, slide exit and landing dip (144 of 153 events; of the other 9, 5 are the landing that ends the reel's placement at f267, which M1 leaves out (1.36 -> 0.83 cm), and 4 are under 1 cm today (0.81 -> 0.45); none worse). A tail: within 1 cm of the rest at 0.15 s (today 0.10 s); no shape over exactly 0.10 s halves the exit's residual (the best, a smoothstep: 4.55 -> 3.54 cm). View only: the sim's eye height (body.eyeHeight: Combat's ray, the bots), the hitboxes and every fingerprint are untouched; the row is read every frame (D1's contract: the follow advances whether the row is on or off, a held frame moves nothing, off draws today's exact expression). Off: the row off, -skygrindOff63 (or older), -skygrindNoEye63. The web has no such row: its eye ease is the MoveTowards this follows.",
  enabled: false,
  enabled_why:
    "Off until GG's blind paired A / B (L-001: feel is done when the screen measurably changes, judged blind). A camera ease is not a still-frame change, so its SPOT is the telemetry, not a pair of frames: verbs37 on the five levels with the row on, again with -skygrindNoEye63, then node Tools/night/n63-m1-chop.cjs --pair <on tag> <off tag>: the eye's own residual (cam.height through the tool's fit) halved at every slide and landing event (the reel's placements left out), the feet, x and z the same on both runs (view only).",
  numbers: {},
};

const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const before = JSON.parse(raw);
if (!before.design || !before.design.n63) throw new Error("no design.n63 (NIGHT63's A1 first)");
if (before.design.n63.eye63) throw new Error("design.n63.eye63 already exists (already run?)");
const dk = Object.keys(before.design);
if (dk[dk.length - 1] !== "n63") throw new Error("design's last key is " + dk[dk.length - 1] + ", not n63");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const tail = eol + "    }" + eol + "  }" + eol + "}"; // the end of design.n63, of design, of the file
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
const block = JSON.stringify(ROW, null, 2).split("\n").map((l, i) => (i === 0 ? l : "      " + l)).join(eol);
const out = raw.slice(0, raw.length - tail.length) + "," + eol + "      \"eye63\": " + block + tail;

const after = JSON.parse(out);
const nk = Object.keys(after.design.n63);
if (nk[nk.length - 1] !== "eye63") throw new Error("eye63 is not design.n63's last key");
if (JSON.stringify(after.design.n63.eye63) !== JSON.stringify(ROW)) throw new Error("design.n63.eye63 not written as the script says");
if (after.design.n63.eye63.enabled !== false) throw new Error("eye63 must be OFF here");
delete after.design.n63.eye63;
if (JSON.stringify(after) !== JSON.stringify(before)) throw new Error("something outside design.n63.eye63 changed (keys, values or order)");
if (dry) {
  console.log("n63-d-eye63-row --dry: design.n63.eye63 would be added after design.n63." + Object.keys(before.design.n63).pop() + " (" + out.length + " chars, was " + raw.length + ")");
  process.exit(0);
}
for (const f of files) fs.writeFileSync(f, out);
if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
console.log("n63-d-eye63-row: design.n63.eye63 written as design.n63's last key after " + Object.keys(before.design.n63).pop() + " (a view row, enabled false, numbers {}; both copies byte-identical; nothing else changed)");
