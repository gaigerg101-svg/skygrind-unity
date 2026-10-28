// NIGHT60 builder V (B8 the exit lamp): writes ONLY design.n60 = { _why, lamp60: { _why, enabled: false, enabled_why, numbers: {} } } after the
// last design key into Design/unity-config.json and its byte copy Assets/StreamingAssets/Design/unity-config.json, then asserts nothing else
// changed (the parsed file minus design.n60 equals the old one, keys, values and order) and the two copies are byte-identical. lamp60 is a VIEW
// row (no `sim`: no tape bit); Exit58.N58V.cs reads it with NightRows.ViewOn(cfg, "lamp60"). The web has no such row (no exit lamp).
//   node Tools/night/n60-v-rows.cjs [--dry]   (refuses to run twice; a later night-60 row is added to design.n60 by its own script)
const fs = require("fs");
const path = require("path");

const dry = process.argv.includes("--dry");
const root = path.join(__dirname, "..", "..");
const files = [path.join(root, "Design", "unity-config.json"), path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json")];

const N60_WHY = "NIGHT60 (GG's Overnight Run, NIGHT60.md): the night's design rows, each off until it passes NIGHT57.md section 0's rule in a Player. First: lamp60 (backlog B8).";
const LAMP60 = {
  _why: "View row (builder V, backlog B8; NIGHT58 V3's exit lamp, Exit58.N58V.cs): on n57-kiln each door a room's clear opens gets a cyan frame and a cyan point light on the room being left, dark while the door is locked and rising once over UnityTuning.Exit58FadeS when it opens (cyan = player-usable, CLAUDE.md rule 8: go here). This row replaces the compile-time UnityTuning.Exit58On (removed), so a build copy can turn it on (node Tools/night/n46-build-rows.cjs <tag> on n60.lamp60) and -skygrindOff60 (or older) turns it off; -skygrindNoExit58 still turns this lamp alone off. View only: no tape bit. Its numbers are Unity-only (UnityTuning.N58V.cs). The web has no exit lamp.",
  enabled: false,
  enabled_why: "Off until its MAIN (the exit door's luma / the room's median at the clear frame >= 1.5x on n57-kiln's new rooms; Tools/night/n60-v-lamp.py on a157's -skygrindClearShots60 frames) and its GUARD (the room's p95 luma outside the door's mask within the planted tolerance of the off side) pass in a Player, by NIGHT57.md section 0's rule.",
  numbers: {},
};

const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const before = JSON.parse(raw);
if (before.design.n60) throw new Error("design.n60 already exists (already run?)");
const last = Object.keys(before.design).pop();
if (!/^n5\d$/.test(last)) throw new Error("design's last key is " + last + ", expected a night block before n60");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const tail = eol + "    }" + eol + "  }" + eol + "}";
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
const n60 = { _why: N60_WHY, lamp60: LAMP60 };
const block = JSON.stringify(n60, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
const out = raw.slice(0, raw.length - tail.length) + eol + "    }," + eol + "    \"n60\": " + block + eol + "  }" + eol + "}";

const after = JSON.parse(out);
if (Object.keys(after.design).pop() !== "n60") throw new Error("design.n60 is not the last key");
if (JSON.stringify(after.design.n60) !== JSON.stringify(n60)) throw new Error("design.n60 not written as the script says");
delete after.design.n60;
if (JSON.stringify(after) !== JSON.stringify(before)) throw new Error("something outside design.n60 changed (keys, values or order)");
if (dry) {
  console.log("n60-v-rows --dry: design.n60.lamp60 would be added after design." + last + " (" + out.length + " chars, was " + raw.length + ")");
  process.exit(0);
}
for (const f of files) fs.writeFileSync(f, out);
if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
console.log("n60-v-rows: design.n60 written after design." + last + ": lamp60 (view row, enabled false) (both copies byte-identical; nothing else changed)");
