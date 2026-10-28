// NIGHT55 builder C (decor55): writes design.n55.decor55.numbers in both config copies (Design/unity-config.json and its StreamingAssets byte
// copy), nothing else: the row's `"numbers": {}` line is replaced by the block below at the same indent; the parsed result is checked to
// differ only there. Idempotent (a second run finds no empty numbers line and stops). Colour numbers are view-only (the web never had a
// per-level decor script; decor54's levels block is the model). Hexes are sRGB, written as numbers (0x.. in each _why).
//   node Tools/night/n55c-decor-config.cjs
const fs = require("fs");
const files = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];
const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const lines = raw.split(eol);
const before = JSON.parse(raw);
const open = lines.findIndex((l) => l.trim() === '"decor55": {');
if (open < 0) throw new Error("no decor55 row");
const ind = lines[open].match(/^ */)[0].length;
const end = lines.findIndex((l, k) => k > open && l.match(/^ */)[0].length === ind && l.trim().startsWith("}"));
const num = lines.findIndex((l, k) => k > open && k < end && l.trim() === '"numbers": {}');
if (num < 0) throw new Error("decor55's numbers is not the empty object (already written?)");
const numbers = {
  _why: "Builder C (Game/Decor55.cs, view only). Measured first on NIGHT54's a452 entry frames (Tools/night/n55-decor-share.py): decor54's accent hue was the environment's own hue on two levels (the foundry's pale brass, hue 41: 24-34 % of each entry frame is that hue, the tan floors and umber walls; the ice line's cobalt, 222: 46-56 %, the slate-blue walls), and the kiln's patina (137) is its own hue but covers 0.16-0.25 %. levels: each level's accent in place of decor54's (every decor54 piece takes it at decor54's luminance rules); bandKinds: the map box kinds that carry the string course (Game/Decor55.cs; its size and height in UnityTuning.N55C.cs).",
  bandKinds: ["wall"],
  bandKinds_why: "the room walls (map box kind wall: 100-128 per level); ledges, floors, cover and spires keep their own lines (you stand on those).",
  levels: {
    _why: "keyed as decor54's (the level id after its 'nNN-' prefix). Only accentHex: the wall tones stay decor54's. Role token hues (UnityTuning.Look49): usable cyan 186, danger 12, parry yellow 44, RIVAL magenta 312, boundary 250.",
    foundry: {
      accentHex: 0xc0db53,
      _why: "THE FOUNDRY: antique green-brass (0xc0db53, hue 72, s 0.62): 28 deg off the parry yellow (O-08: decor54's brass sat 3 deg from it), 60 off the danger red-orange, ~40 off the tan floors and umber walls (decor54's 41 was their hue: the accent vanished into them), 114 off cyan. Brass gone green in a foundry's soot.",
    },
    iceline: {
      accentHex: 0x52d996,
      _why: "THE ICE LINE: aurora green (0x52d996, hue 150, s 0.62) over the slate-blue walls (~218: decor54's cobalt 222 was their hue): 36 deg off cyan, 106 off parry, 138 off danger, 162 off RIVAL. 13 deg from the kiln's patina: two levels apart, never in one frame.",
    },
    kiln: {
      accentHex: 0x56cc77,
      _why: "THE KILN: the oxidised copper patina kept at its hue (0x56cc77, hue 137) and saturated (s 0.40 -> 0.58) so it reads as a colour, not a tint, against the fired clay (hue 330-340) and the heat's red-orange: 49 deg off cyan, 93 off parry.",
    },
  },
};
const pad = " ".repeat(lines[num].match(/^ */)[0].length);
const block = JSON.stringify(numbers, null, 2).split("\n").map((l, i) => (i === 0 ? l : pad + l));
const replaced = lines.slice(0, num).concat([pad + '"numbers": ' + block[0]], block.slice(1), lines.slice(num + 1));
const out = replaced.join(eol);
const after = JSON.parse(out);
if (JSON.stringify(after.design.n55.decor55.numbers) !== JSON.stringify(numbers)) throw new Error("the block did not land");
after.design.n55.decor55.numbers = {};
const norm = (o) => JSON.stringify(o, (k, v) => (v && typeof v === "object" && !Array.isArray(v) ? Object.fromEntries(Object.entries(v).sort()) : v));
if (norm(after) !== norm(before)) throw new Error("something else changed");
for (const f of files) fs.writeFileSync(f, out);
console.log("design.n55.decor55.numbers written (foundry, iceline, kiln accents; bandKinds); both copies");
