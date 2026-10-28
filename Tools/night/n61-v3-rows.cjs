// NIGHT61 builder V (slate C3, no yellow on the guns): writes ONLY design.n61.gunYellow61 (a VIEW row: no `sim`, no tape bit) as the last key of the
// existing design.n61 block (the lead's A1 _why, then the rows already there: exitFrame61, bossRule61, text61, fixture61, ... are left as they are)
// into Design/unity-config.json and its byte copy Assets/StreamingAssets/Design/unity-config.json, then asserts nothing else changed (the parsed file
// minus design.n61.gunYellow61 equals the old one, keys, values and order) and the two copies are byte-identical (L-049). GunYellow61.OffFor reads it
// with NightRows.ViewOn(cfg, "gunYellow61"); -skygrindOff61 (or older) and -skygrindNoGunYellow61 turn it off. No number in the config: the colour
// a gun takes is the config's own colors.usable, and the arc's numbers are Unity-only (UnityTuning.N61V3.cs). The web has no such row.
//   node Tools/night/n61-v3-rows.cjs [--dry]   (refuses to run twice; after a pick that conflicts in the config, re-run it on the merged copies)
const fs = require("fs");
const path = require("path");

const dry = process.argv.includes("--dry");
const root = path.join(__dirname, "..", "..");
const files = [path.join(root, "Design", "unity-config.json"), path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json")];

const GUNYELLOW61 = {
  _why: "View row (builder V, NIGHT61 slate C3, gg-proxy's candidate; GG 2026-10-19 'the colors and stuff are way too bland ... it all needs to be unified'; yellow means a parry tell, CLAUDE.md rule 8 / DESIGN-RULES C-5; yellow56, which also moved the guns, is OFF and needs a tells mask): NO YELLOW ON THE GUNS (Game/GunYellow61.N61V3.cs, its two hook lines in Viewmodel.cs: FamilyOf and the THUNDERCLAP's build). A viewmodel gun whose family colour (the web's weapons.families) is in the yellow arc (UnityTuning.GunYellow61HueLo..HueHi, S over GunYellow61MinS: edge #ffb000 the THUNDERCLAP, fire #ff8c1a the TORCH, blast #ffe14a the BOUNCER) takes colors.usable, the dusk palette's player-usable cyan: its accent plate, its gem and the TORCH's pilot glow. The guns' shapes are unchanged; the wall buys, pickups, the mystery box, muzzle flashes and every enemy and tell colour are untouched. Read when the viewmodel is built (before PlayerChecks, so the command line is read too). With yellow56 also on, this rule runs first. Off: -skygrindOff61 (or older) or -skygrindNoGunYellow61 (this row alone). Proof lines in the Player log, once per gun: '[gunyellow61] <weapon> #<from> -> #<to> (colors.usable)'. View only: no tape bit; no number in the config. The web has no such row.",
  enabled: false,
  enabled_why: "Off until slate C3 passes NIGHT61.md section 2's visible rule in a Player: yellow-hue pixels on the viewmodel (yellow56's tells band) = 0 in the four poses (THUNDERCLAP: kb2c-entry, vs/verbs37/f0600; TORCH: vp/verbs37/f0300, f0900) on vs off in one build; the guns' shapes unchanged; the tells' yellow untouched; the SPOT on the THUNDERCLAP pair naming the gun's colour; verbs37 x5 unchanged; then a skygrind-critic PASS.",
  numbers: {},
};

const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const before = JSON.parse(raw);
if (!before.design.n61) throw new Error("no design.n61 (the lead's A1 commit writes the block first)");
if (before.design.n61.gunYellow61) throw new Error("design.n61.gunYellow61 already exists (already run?)");
if (Object.keys(before.design).pop() !== "n61") throw new Error("design.n61 is not design's last key");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const tail = eol + "    }" + eol + "  }" + eol + "}"; // the end of design.n61, of design, of the file
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
const block = JSON.stringify(GUNYELLOW61, null, 2).split("\n").map((l, i) => (i === 0 ? l : "      " + l)).join(eol);
const out = raw.slice(0, raw.length - tail.length) + "," + eol + "      \"gunYellow61\": " + block + tail;

const after = JSON.parse(out);
const keys = Object.keys(after.design.n61);
if (keys[keys.length - 1] !== "gunYellow61") throw new Error("gunYellow61 is not design.n61's last key");
if (JSON.stringify(after.design.n61.gunYellow61) !== JSON.stringify(GUNYELLOW61)) throw new Error("design.n61.gunYellow61 not written as the script says");
delete after.design.n61.gunYellow61;
if (JSON.stringify(after) !== JSON.stringify(before)) throw new Error("something outside design.n61.gunYellow61 changed (keys, values or order)");
if (dry) {
  console.log("n61-v3-rows --dry: design.n61.gunYellow61 would be added after design.n61." + keys[keys.length - 2] + " (" + out.length + " chars, was " + raw.length + ")");
  process.exit(0);
}
for (const f of files) fs.writeFileSync(f, out);
if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
console.log("n61-v3-rows: design.n61.gunYellow61 written after design.n61." + keys[keys.length - 2] + ": a view row, enabled false (both copies byte-identical; nothing else changed)");
