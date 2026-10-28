// NIGHT63 builder V (exit63, the doorway itself is the way out): writes ONLY builder V's view row design.n63.exit63 (no `sim`, no tape bit) as
// the last key of design.n63 in Design/unity-config.json and its byte copy Assets/StreamingAssets/Design/unity-config.json, then asserts
// nothing else changed (the parsed file minus the new row equals the old one: keys, values and order) and that the two copies are
// byte-identical (L-049, L-065). Text splices only. Run once; a row already there is refused.
//   node Tools/night/n63-v-exit-rows.cjs [--dry]
// The row is read every frame by Exit63.N63V.cs (NightRows.ViewOn); -skygrindOff63 (or older) and -skygrindNoExit63 turn it off.
const fs = require("fs");
const path = require("path");

const dry = process.argv.includes("--dry");
const name = "exit63";
const root = path.join(__dirname, "..", "..");
const files = [path.join(root, "Design", "unity-config.json"), path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json")];

const ROW = {
  _why: "View row (builder V, NIGHT63 exit63; GG 2026-10-13: doors 'not visible now and you just run through it and it's like dark'; tonight's P1, exitFrame61's frame, passage light and floor line, was SHORT: its own gain on the door 1.07-1.56x, 1 of 5 rooms at 1.5x, and its brighter frame lit the gun): the doorway itself turns the player-usable cyan when a room's clear opens it, drawn by the door's own geometry, never a light. On n57-kiln, inside each opened exit's passage only (Exit63Math: every part clear of the passage's walls, floor and ceiling, none in the room): THE VEIL, a thin additive sheet over the opening just inside the room's face, stepping aside as the eye reaches it; THE LINERS, both jambs and the lintel lined for the passage's first metres; THE THRESHOLD, a lit strip across the floor just inside the face; THE DASHES along the passage floor toward the next room. One rise when the door opens (never a flash), held until it locks again; a door open since the map was read stays dark; doors into a boss room keep their boundary tone. Every gain 1 or under (no bloom past the door's box), no Light (nothing lights the room or the gun). Read every frame (Exit63.N63V.cs, NightRows.ViewOn: D1's override). Game layer only: the sim never reads it (no tape bit). Off: -skygrindOff63 (or older) or -skygrindNoExit63. The web has no such row: it has no campaign doors (Campaign24 is a Unity night's, after the web froze).",
  enabled: false,
  enabled_why: "Off until it passes its doneWhen (Research/2026-10-22-wave63/V-EXIT.md, written before any Player run) in a Player: OWN GAIN (door luma on / off, Tools/night/n60-v-lamp.py on a157's -skygrindClearShots60 kiln clear shots, a same-run pair) >= 1.5x in >= 4 of 5 rooms; the GUARD (the p95 luma outside the door's box within 0.02 of off) 5 / 5; one [exit63] door-lit line per lit door and 0 with the row off; the SPOT pair (a157 -skygrindExitShots63 kb3d) >= 41,472 px and named; verbs37 x5 unchanged; then a skygrind-critic PASS.",
  numbers: {
    _why: "The doorway's parts (m: n57-kiln's doors are 4 m wide, 4 m deep, 4.5 m tall), their additive share of colors.usable at full, the rise and the veil's step aside. Every gain is 1 or under: colors.usable in linear light is about (0.01, 0.65, 0.79), so a part over the dark passage stays under the bloom threshold (post.bloomThreshold 1) and no halo spills past the door's box.",
    riseS: 0.6,
    riseS_why: "s: the one rise from dark to full when the door opens (smoothstep on the unscaled clock), then held: a door that comes on, never a flash or a blink (exitFrame61's ExitFrame61RiseS reads the same way).",
    veilGain: 0.4,
    veilGain_why: "The veil's share: the opening reads cyan (Doors38View's open curtain is 0.16 x its 2.4 glow, about 0.38: this doubles the opening's cyan) and the next room stays readable through it.",
    linerGain: 0.5,
    linerGain_why: "The jambs' and the lintel's share: the passage's own surfaces read cyan from an angle, where the veil is seen edge-on.",
    stripGain: 0.7,
    stripGain_why: "The threshold strip's share: the brightest part, a line across the floor at the way in (small on screen, so under the bloom threshold over the dark floor even with the veil in front).",
    dashGain: 0.6,
    dashGain_why: "The dashes' share: the way on along the passage floor, a step under the threshold.",
    inset: 0.03,
    inset_why: "m: every part's clearance from the passage's walls, floor, ceiling and faces (never coplanar: no z-fighting with the door's own surfaces).",
    veilT: 0.02,
    veilT_why: "m: the veil's thickness (a sheet).",
    linerT: 0.04,
    linerT_why: "m: the liners' thickness off the jambs and the lintel.",
    linerDepthM: 4,
    linerDepthM_why: "m: how far into the passage the liners run, capped by the passage less an inset at each face (kiln's 4 m passages: the whole of it).",
    stripDepthM: 0.3,
    stripDepthM_why: "m: the threshold strip's depth along the passage.",
    stripT: 0.02,
    stripT_why: "m: the strip's and the dashes' thickness over the floor.",
    dashLenM: 0.5,
    dashLenM_why: "m: a dash's length along the passage.",
    dashW: 0.25,
    dashW_why: "m: a dash's width across the passage.",
    dashGapM: 0.35,
    dashGapM_why: "m: the gap before each dash (after the strip, then between dashes).",
    dashCount: 4,
    dashCount_why: "The dashes; those that would reach past the far face are left out (kiln's 4 m passages hold all 4: the last ends 3.73 m in).",
    veilNearM: 1.0,
    veilNearM_why: "m from the eye to the veil's sheet: the veil is gone this near, so running through the door never washes the screen cyan (Doors38View's shimmer marks the crossing).",
    veilFarM: 3.0,
    veilFarM_why: "m: the veil is whole from this far out (a157's clear shots stand 18-27 m from the door; its exit63 shot 10 m), easing between.",
  },
};

const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const before = JSON.parse(raw);
if (!before.design.n63) throw new Error("no design.n63 (the lead's A1 commit writes the block first)");
if (before.design.n63[name]) throw new Error("design.n63." + name + " already exists (already run?)");
if (Object.keys(before.design).pop() !== "n63") throw new Error("design.n63 is not design's last key");
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
  console.log("n63-v-exit-rows --dry: design.n63." + name + " would be added after design.n63." + keys[keys.length - 2] + " (" + out.length + " chars, was " + raw.length + ")");
  process.exit(0);
}
for (const f of files) fs.writeFileSync(f, out);
if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
console.log("n63-v-exit-rows: design.n63." + name + " written after design.n63." + keys[keys.length - 2] + ": a view row, enabled false (both copies byte-identical; nothing else changed)");
