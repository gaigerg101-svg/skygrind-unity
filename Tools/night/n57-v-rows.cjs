// NIGHT57 builder V: THE ONE NUMBERS WRITER of V's rows tonight (builder brief "Your numbers"; model: Tools/night/n56-v-rows.cjs). Writes into both
// config copies (Design/unity-config.json and Assets/StreamingAssets/Design/unity-config.json, byte-identical), only the keys listed in SETS inside
// the named rows' `numbers` objects (added or replaced; every other key of those objects kept as it is). V's rows: design.n56.colour56 / rim56 /
// yellow56, design.n55.grade55 / brief55. Asserts the copies match before and after, that each written numbers object parses back to what this
// script built, and that nothing outside those numbers objects changed. The rows' `enabled` is never touched (the lead flips rows in a build's copy).
//   node Tools/night/n57-v-rows.cjs            (every row below)      node Tools/night/n57-v-rows.cjs colour56   (only those)
const fs = require("fs");
const path = require("path");

const root = path.join(__dirname, "..", "..");
const files = [path.join(root, "Design", "unity-config.json"), path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json")];

const SETS = {
  // V1 colour56: THE WORLD SATURATION CAP (Game/Render/Cap57Pass.cs, Assets/Shaders/Resources/Cap57.shader; Tools/night/n57-v-cap.py simulates it)
  "n56.colour56": [
    [["cap"], {
      _why: "NIGHT57 builder V (slate V1-colour56, NIGHT57.md section 5 V1: 'cap the world first'; research/design.md lesson 7, Valve's Left 4 Dead: 'a saturation threshold. Anything below that moves towards gray'; colors.md method 3: no world pixel over S 0.8, the tells' median >= 0.75). While colour56 is on (and no -skygrindOff57 or lower), a post pass after the CRT moves every pixel whose hue is OUTSIDE roleArcs and whose saturation (HSV on the sRGB frame) is over the level's cap to the cap toward the grey of its own luminance (Rec. 709 on the sRGB value: hue and luminance kept, so never a tint and the greyscale frame unchanged); the role families keep theirs, so the tells, the usable cyan and RIVAL stay the most saturated things on screen and colour56's key / accent (non-role hues) sit under them. NIGHT56 measured the world out-saturating the tells on 28 / 36 of colour56's a356 frames (n56-r5; 6 / 6 and 6 / 6 on the no-colour56 foundry / spire sides too: the grade and the lights, not only colour56). The web has no such number (no grade, one palette).",
      levels: {
        _why: "the cap per level (keyed as decor54's: the level id after its 'nNN-' prefix; a level not listed is not capped). Chosen by Tools/night/n57-v-cap.py simulate on n56-r5's a356 ON frames (36; the tells' median S per frame 0.50-0.99, 0.77 the token when no tell is in view): 0.70 everywhere took the 'world over the tells' frames 28 -> 3 (simulated, not rendered) with the key|accent cover unchanged (S >= 0.35 is under the cap) and the cap moving a median 2-3 % of the frame (foundry, kiln, span) / 21 % (spire: its lime-olive walls) / 17 % (iceline at 0.65); the key|accent at S >= 0.35 (V1's MAIN fallback) is untouched by any cap >= 0.35.",
        foundry: 0.7,
        foundry_why: "the jade-lit oxide greens (hue 130-150, S 1.00 on ON and OFF frames: look49's jade light and grade) to S 0.70; simulated sat FAIL 5 / 6 -> 0 / 6",
        iceline: 0.65,
        iceline_why: "the azure-lit walls (hue 210-230 on ~45 % of the frame at S ~0.85-0.9) under the line's tells, which sit lower than elsewhere (the yellow-shielded husks' tells median S 0.62-0.74 on g2 / g3); simulated sat FAIL 7 / 12 at 0.70 -> 2 / 12 at 0.65 (0.55 took it to 0 but moved a median 27 % of the frame)",
        kiln: 0.7,
        kiln_why: "the kiln's violet light (hue 260-280) and the S 1.0 greens at ~143-154; simulated 6 / 6 -> 1 / 6 (kb2c entry: its tells' median S 0.50 under any cap that keeps the rose key)",
        span: 0.7,
        span_why: "the violet / blue open hall (hue 190-260 at S ~0.8); simulated 4 / 6 -> 0 / 6",
        spire: 0.7,
        spire_why: "the emerald-lit stone and the absinthe frieze (hue 70-130 at S 1.00: the 'one olive-green on every surface' GG named on spire v6, and colour56's own frieze rendering S 0.92); simulated 6 / 6 -> 0 / 6, moving a median 21 % of the frame (the lime walls toward grey: the opposite of a tint)"
      },
      roleArcs: [350, 70, 150, 200, 285, 335],
      roleArcs_why: "deg, three (lo, hi) arcs the cap never touches (lo > hi wraps through 0): 350-70 danger red-orange and the parry yellow (CLAUDE.md rule 8; n56-colour.py's danger family runs on to pure red at 350 and its yellow band to 70), 150-200 the usable cyan (its bloom and chromatic fringes render at 150-165: n56-colour.py's cyan family), 285-335 RIVAL's magenta and FUSION's hot pink (DESIGN-RULES V-2). The same arcs n56-colour.py leaves out of the world's saturation check (it counts 330-335 as world: the cap protects 5 deg more than the meter does, so a 330-335 pixel can still read as world there).",
      featherDeg: 6,
      featherDeg_why: "deg: the protection ramps from 0 at each arc's edge to 1 this far inside it (a smoothstep), so no hard seam where a gradient crosses a family's edge; outside the arcs the cap is whole"
    }, "NIGHT57 builder V: the world saturation cap (Game/Render/Cap57Pass.cs)"],
    // r2 (the lead's r1 numbers, Logs/n57/r1-colour-counts.txt: MAIN 36/36, world over the tells 7/36, deuteranopia 4/4, GREYSCALE FAIL: v1none
    // 25/35 -> v1on 18/36, v1off 19/36): colour56 lifted the world's luminance p75 over the enemies and hazards. The A/B on n57-r1 (my read):
    // the frieze renders at luma 0.23-0.42 on 2-19 % of the frame over a world p75 of 0.17-0.29 (foundry a3 entry p75 0.369 with it, 0.287
    // without; spire v6 mid 0.300 / 0.166); span's and iceline's neutral walls kept decor54's VALUE, which for a violet / blue ramp is ~1.65x its
    // luminance (span p75 none 0.233-0.314 -> on 0.280-0.371 with or without the frieze). All gated off by -skygrindOff57 (NIGHT56's colour56).
    [["friezeLumaMax"], 0.22, "NIGHT57 V r2: the frieze's key colour scaled (sRGB, hue and saturation kept) so its Rec. 709 luma is at most this (Colour56.Build; the authored keys' lumas: foundry 0.514, iceline 0.383, kiln 0.357, span 0.316, spire 0.546 -> x0.43 / 0.57 / 0.62 / 0.70 / 0.40); n57-r1 rendered the frieze at 0.6-0.85x its authored luma, so ~0.14-0.19 on the frame, under the world's p75. The key reads by hue at a low value (value first: colors.md method 5)."],
    [["levels", "iceline", "wallBaseHex57"], 0x1d1e20, "NIGHT57 V r2: 0x1d1e20, the neutral foot at decor54's ramp LUMINANCE (0x161e2c: luma 0.115; this 0.117; NIGHT56's 0x262729 0.153)"],
    [["levels", "iceline", "wallUpperHex57"], 0x6a6c71, "NIGHT57 V r2: 0x6a6c71, the neutral band at decor54's luminance (0x5c6c84: 0.417; this 0.423; NIGHT56's 0x7d8085 0.501)"],
    [["levels", "span", "wallBaseHex57"], 0x171619, "NIGHT57 V r2: 0x171619, at decor54's luminance (0x161530: 0.091; this 0.088; NIGHT56's 0x262529 0.147)"],
    [["levels", "span", "wallUpperHex57"], 0x48474e, "NIGHT57 V r2: 0x48474e, at decor54's luminance (0x46447e: 0.285; this 0.281; NIGHT56's 0x797780 0.471: the span's p75 lift)"]
  ]
};

// V4 yellow56: THE DIM YELLOW (slate V4 MAIN: non-parry yellow <= 0.5 % per frame; NIGHT56: spire verbs37 28 / 36 frames over, median 8.54 %,
// most of it dim mustard / olive at V 0.2-0.5 that the meter's S > 0.5 counts; the parry tells are bright emissives, V >= ~0.85). Rides
// colour56's cap pass (Render/Cap57Pass.cs) as its own row's numbers: a pixel in the yellow arc with a value under vNone moves toward sMax
// (whole under vFull), luminance and hue kept. colour56's role arcs keep 350-70 whole, so the cap alone never touches yellow.
SETS["n56.yellow56"] = [
  [["dimYellow"], {
    _why: "NIGHT57 builder V (slate V4 yellow56, one attempt): the frame's DIM yellow held under the meter's 'yellow' saturation (n56-colour.py: hue 45-70, S > 0.5, V >= 0.15), toward the grey of its own luminance (Cap57.shader, hue and luminance kept); bright yellow (the parry tells, V >= vNone) untouched. Drawn on the main camera after the CRT: the viewmodel (the spire's gun share, 2-6 %) and the HUD draw after it and keep theirs. The web has no such number (no parry colour rule).",
    sMax: 0.45,
    sMax_why: "the dim yellow's S cap: under the meter's 0.5 with a margin (the PS2 quantise and the PNG round a few 1 / 255)",
    vFull: 0.5,
    vFull_why: "value (sRGB max channel) at or under which the cap is whole: n56-r5 spire's mustard / olive walls and ochre floors V 0.2-0.5 (V.md 2026-10-20: 'mostly V 0.25-0.4')",
    vNone: 0.75,
    vNone_why: "value at or over which nothing moves (a smoothstep between): the parry token #FFC93A is V 1.0, the orbs' emissive V 0.93 (the meter's 'parry orb by shape' rule takes V >= 0.85)",
    hueLo: 40,
    hueLo_why: "deg: the arc's start (whole from 44 with the 4 deg feather: the meter's yellow band 45-70 whole; the danger family's 35-40 untouched)",
    hueHi: 76,
    hueHi_why: "deg: the arc's end (whole to 72)",
    featherDeg: 4,
    featherDeg_why: "deg: the smoothstep inside each end of the arc"
  }, "NIGHT57 builder V: the dim-yellow cap (Game/Render/Cap57Pass.cs)"]
];

// V3 grade55: THE CROWN's palette LIGHT-ONLY (slate V3-grade55 MAIN: its tells kept >= the no-grade count). NIGHT56's two attempts toned the
// shadows (SplitToning, a steel-blue at balance 44-50), darkened (EV) and raised contrast, and lerped the fog to a cool grey: the ring's tells
// (the neon material is URP Unlit: no light reaches it, but the fog and the post do) lost their marginal cells: kept 400 / 527 (0.76), then
// 362 / 533 (0.68) on n56-r5. Light-only: the post identity (a neutral split and turn, no EV / contrast / saturation delta) and the fog as it is;
// only the ambient and the key light keep their cool tint, which reach the lit stone and never an unlit tell. The turn keeps common's
// darkening beat (turnEv / turnContrast, 0.55 s). THE WICK and THE WYRM unchanged.
const CROWN_WHY = "NIGHT57 V (V3-grade55, light-only): ";
SETS["n55.grade55"] = [];
for (const p of ["p1", "p2", "p3"]) {
  SETS["n55.grade55"].push(
    [["crown37", p, "split"], [0.5, 0.5, 0.5], CROWN_WHY + "neutral (SoftLight with 0.5 grey is the identity): the shadows untoned, so the ring's dim tells keep their hue and saturation (n56-r5: 0.68 of the tells kept with the steel-blue split)"],
    [["crown37", p, "turn"], [0.5, 0.5, 0.5], CROWN_WHY + "neutral: the turn is common's darkening beat only (no colour swing over the tells)"],
    [["crown37", p, "ev"], 0, CROWN_WHY + "0: no darkening at rest (the tells' value floor proof.tellVal 0.25)"],
    [["crown37", p, "contrast"], 0, CROWN_WHY + "0: no contrast delta at rest (contrast pushes the dim tells under the value floor)"],
    [["crown37", p, "fogMix"], 0, CROWN_WHY + "0: the fog colour untouched (URP Unlit fogs the ring: a cool fog took the far tells out of the danger band)"],
    [["crown37", p, "sat"], 0, CROWN_WHY + "0 (as NIGHT56's attempt 2)"]
  );
}
SETS["n55.grade55"].push(
  [["proof", "panelCyanMin"], 0.1, "NIGHT57 V (V3-grade55 guard: THE WICK's ride panels carry the cyan ride cue): a155's share of the ride panels' wall cells in the usable cyan band, on the shots where the panels are OUT (sim.Wick47PanelsOut: P2 / P3 and the turn into P2; in P1 the panels are folded into the wall and leave the world: no ride panel, no cue, V-3), at least this. n56-r2 b1off (grade55 off, the 0.16 m trims) 8 %; n56-r5 b1on (0.32 m, hot.panelTrimScale 2) turn 13 %, p2 14 %."]
);

const want = process.argv.slice(2);
const pick = (k) => !want.length || want.includes(k.split(".")[1]);
const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const before = JSON.parse(raw);

function fmt(v, ind) {
  const pad = " ".repeat(ind);
  if (Array.isArray(v)) {
    if (!v.length) return "[]";
    if (v.every((x) => typeof x !== "object" || x === null)) return "[" + v.map((x) => JSON.stringify(x)).join(", ") + "]";
    return "[" + eol + v.map((x) => pad + "  " + fmt(x, ind + 2)).join("," + eol) + eol + pad + "]";
  }
  if (v && typeof v === "object") {
    const keys = Object.keys(v);
    if (!keys.length) return "{}";
    return "{" + eol + keys.map((k) => pad + "  " + JSON.stringify(k) + ": " + fmt(v[k], ind + 2)).join("," + eol) + eol + pad + "}";
  }
  return JSON.stringify(v);
}

function setPath(o, keys, value, why) {
  let t = o;
  for (let i = 0; i < keys.length - 1; i++) {
    if (!t[keys[i]] || typeof t[keys[i]] !== "object") throw new Error("no object at " + keys.slice(0, i + 1).join("."));
    t = t[keys[i]];
  }
  const k = keys[keys.length - 1];
  t[k] = value;
  if (why) t[k + "_why"] = why;
}

const targets = {};
for (const [row, sets] of Object.entries(SETS)) {
  if (!pick(row)) continue;
  const [grp, name] = row.split(".");
  if (!before.design[grp] || !before.design[grp][name]) throw new Error("no design." + row);
  const num = JSON.parse(JSON.stringify(before.design[grp][name].numbers || {}));
  for (const [keys, value, why] of sets) setPath(num, keys, value, why);
  targets[row] = num;
}

let lines = raw.split(eol);
for (const [row, num] of Object.entries(targets)) {
  const [grp, name] = row.split(".");
  const g = lines.findIndex((l) => l.trim() === '"' + grp + '": {');
  if (g < 0) throw new Error("no group line for " + grp);
  const open = lines.findIndex((l, k) => k > g && l.trim() === '"' + name + '": {');
  if (open < 0) throw new Error("no row line for " + row);
  const ind = lines[open].match(/^ */)[0].length;
  const end = lines.findIndex((l, k) => k > open && l.match(/^ */)[0].length === ind && l.trim().startsWith("}"));
  const n = lines.findIndex((l, k) => k > open && k < end && l.match(/^ */)[0].length === ind + 2 && l.trim().startsWith('"numbers": '));
  if (n < 0) throw new Error(row + ": its numbers key not found");
  let last = n;
  if (!/^"numbers": \{\},?$/.test(lines[n].trim())) {
    last = lines.findIndex((l, k) => k > n && l.match(/^ */)[0].length === ind + 2 && l.trim().startsWith("}"));
    if (last < 0) throw new Error(row + ": the numbers object's end not found");
  }
  const comma = lines[last].trim().endsWith(",") ? "," : "";
  const text = (" ".repeat(ind + 2) + '"numbers": ' + fmt(num, ind + 2) + comma).split(eol);
  lines = lines.slice(0, n).concat(text, lines.slice(last + 1));
}
const out = lines.join(eol);
const after = JSON.parse(out);
for (const [row, num] of Object.entries(targets)) {
  const [grp, name] = row.split(".");
  if (JSON.stringify(after.design[grp][name].numbers) !== JSON.stringify(num)) throw new Error(row + ": written numbers differ from the script's");
  if (after.design[grp][name].enabled !== before.design[grp][name].enabled) throw new Error(row + ": enabled changed");
  after.design[grp][name].numbers = before.design[grp][name].numbers;
}
const norm = (o) => JSON.stringify(o, (k, v) => (v && typeof v === "object" && !Array.isArray(v) ? Object.fromEntries(Object.entries(v).sort()) : v));
if (norm(after) !== norm(before)) throw new Error("something outside the rows' numbers changed");
for (const f of files) fs.writeFileSync(f, out);
if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
console.log("n57-v-rows: numbers written for " + Object.keys(targets).join(", ") + " (both copies; enabled untouched)");
