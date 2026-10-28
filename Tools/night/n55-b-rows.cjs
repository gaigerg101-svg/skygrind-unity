// NIGHT55 builder B: writes the numbers of B's rows (design.n55.grade55 / wick55 / bossText55 .numbers) into both config copies
// (Design/unity-config.json and Assets/StreamingAssets/Design/unity-config.json, byte-identical), replacing only those `numbers` objects.
// Idempotent: run it again after editing the objects below. Asserts the two copies match before and after, and that the parsed result differs
// from the input only inside the named rows' `numbers`. The rows stay `enabled: false` (the lead flips them in a build's copy).
//   node Tools/night/n55-b-rows.cjs [row ...]      (no row: every row below)
const fs = require("fs");

const ROWS = {
  grade55: {
    _why: "Builder B (BossGrade55.cs, Wick37View.N55.cs, FeelReel.N55.Boss.cs a155). THE WICK's and THE CROWN's palettes graded in the SHADOWS only (URP split toning: a cool shadow colour, neutral highlights, a positive balance so the tint's weight 1 - luma - balance falls to ~0 on emissive tells, the core, the embers and the player's cyan), exposure / contrast deltas and a cool light layer; THE WICK's hot walls a state (cold seams at rest, the heat where the player is, a burn the hottest thing on the wall). The web has no such numbers: the boss palettes are Unity-only (NIGHT54 grade54). Round 2 (the lead's n55-r1 a155: THE WICK's walls S 0.70 and the burn patch S 0.48 under them; THE CROWN's grade kept 282 of 533 ring tell cells): every shadow colour about half as chromatic and THE CROWN's balance up to 36-42, the heat's gains up. Round 1's starting values came from an offline split-toning model on n54-freeze2's p1-raw (THE WICK) and n54-r3's p1-raw (THE CROWN): walls H 72 / 37+123 -> ~220-240, the tells' hue moved <= 8 deg, S >= 0.7 (not the Player's numbers: a155 measures them).",
    common: {
      priority: 51, priority_why: "over grade54's volume (50) and Look's (0): URP lerps each overridden parameter toward this volume's by its weight",
      fadeInS: 0.8, fadeInS_why: "s: the grade blends in after the fight's first frame (grade54's)",
      fadeOutS: 1.6, fadeOutS_why: "s: the grade leaves after the boss dies (grade54's)",
      turnS: 0.55, turnS_why: "s: one turn's envelope (grade54's; GG's 0.3-0.8 s)",
      turnPeak: 0.3, turnPeak_why: "the envelope's peak as a share of turnS (a fast attack, a slower release)",
      turnGapS: 1, turnGapS_why: "s: the least time between two turns (one light-dark pair a second at most: under WCAG 2.3.1's 3 flashes)",
      turnEv: -0.4, turnEv_why: "EV at a turn's peak: a turn darkens, never brightens (a value move, web.md item 1)",
      turnContrast: 30, turnContrast_why: "contrast points added at a turn's peak",
      turnFlip: 1, turnFlip_why: "at a turn's peak the shadow colour swings this share of the way to the phase's `turn` colour (the JoJo swing: 40-60 deg inside the cool half of the wheel)",
      openScale: 0.5, openScale_why: "a stagger window opening turns at this share of a phase turn's strength",
    },
    wick: {
      _why: "L1 THE WICK at night: a deep indigo / violet shaft lit cool, the slag, the core and the tells the only heat in it (dark and hot: value contrast). The phases step round the cool half: P1 indigo-blue (~225), P2 violet (~255, the climb), P3 a black-blue cap (~235, the most contrast).",
      on: 1, on_why: "1: THE WICK graded with the row; 0: ungraded (NIGHT54's look) while THE CROWN keeps its grade",
      p1: { split: [0.26, 0.32, 0.76], balance: 30, ev: -0.35, contrast: 15, fog: [0.04, 0.05, 0.12], fogMix: 0.5, ambient: [0.8, 0.85, 1], key: [0.85, 0.9, 1], lights: [0.7, 0.76, 1], lightsMix: 0.8, turn: [0.38, 0.22, 0.8],
        _why: "P1 the floor: indigo-blue shadows (split toning, highlights neutral), balance +28 (the tint's weight ~0.4 on the walls, ~0.2 on a red tell, 0 on the core and the cyan), EV -0.35, the stack lights 80 % of the way to a cool white; the start turn swings to violet" },
      p2: { split: [0.42, 0.22, 0.8], balance: 28, ev: -0.42, contrast: 20, fog: [0.06, 0.04, 0.12], fogMix: 0.55, ambient: [0.84, 0.8, 1], key: [0.88, 0.85, 1], lights: [0.78, 0.72, 1], lightsMix: 0.8, turn: [0.2, 0.36, 0.8],
        _why: "P2 the climb: violet shadows (~255: 30 deg round from P1), a step darker; its turn swings to blue (~205)" },
      p3: { split: [0.24, 0.28, 0.66], balance: 20, ev: -0.5, contrast: 30, fog: [0.03, 0.03, 0.07], fogMix: 0.65, ambient: [0.7, 0.72, 0.9], key: [0.8, 0.84, 1], lights: [0.68, 0.72, 1], lightsMix: 0.85, turn: [0.36, 0.22, 0.74],
        _why: "P3 the cap: black-blue shadows, the most contrast and the darkest: the core and the CHARGE's tells carry the frame; its turn swings to violet" },
    },
    crown37: {
      _why: "L5 THE CROWN at height: cold steel. Steel-blue shadows (~210) with a light touch (EV near 0, low contrast): pale and high, where THE WICK is dark and low. The steel leans green enough that the spire's amber wall goes toward gold, never toward red (the model: amber 37 -> 43-46 with these; a bluer split took it to 5-28: too close to the danger hue).",
      on: 1, on_why: "1: THE CROWN graded with the row; 0: ungraded (NIGHT54's look) while THE WICK keeps its grade",
      p1: { split: [0.36, 0.46, 0.66], balance: 42, ev: -0.05, contrast: 8, fog: [0.4, 0.45, 0.55], fogMix: 0.35, ambient: [0.88, 0.93, 1], key: [0.9, 0.95, 1], turn: [0.34, 0.4, 0.72],
        _why: "P1 ring one: steel shadows (~205), the gold of the arena kept in the highlights; the start turn swings to slate-indigo" },
      p2: { split: [0.4, 0.34, 0.72], balance: 40, ev: -0.12, contrast: 12, fog: [0.33, 0.37, 0.5], fogMix: 0.45, ambient: [0.86, 0.9, 1], key: [0.9, 0.93, 1], turn: [0.34, 0.52, 0.64],
        _why: "P2 the gallery: a bluer steel (~215), a step darker; its turn swings to sea-steel (~190, held dark: never the player's cyan's value)" },
      p3: { split: [0.38, 0.36, 0.72], balance: 36, ev: -0.2, contrast: 18, fog: [0.22, 0.24, 0.38], fogMix: 0.5, ambient: [0.82, 0.85, 1], key: [0.88, 0.9, 1], turn: [0.36, 0.46, 0.68],
        _why: "P3 the plaza: slate-indigo (~228), the darkest of the three; its turn swings back to steel" },
    },
    hot: {
      _why: "THE WICK's hot wall as a state (Wick37View.N55.cs; the sim's rule unchanged: the shaft wall still burns a rider or a brush)",
      seam: [0.3, 0.32, 0.4], seam_why: "the seams at rest: cold iron, a value step on the dark iron (HSV S 0.25: low chroma, hue ~228: 144 deg from the danger hue)",
      seamGain: 0.35, seamGain_why: "the cold seams' additive gain (dim: a line in the iron, not a glow)",
      nearM: 2.5, nearM_why: "m: the player's capsule this close to the shaft wall starts its heat (the iron glows before the touch: the warning)",
      contact: 0.7, contact_why: "the heat at the wall (0..1) from the approach alone",
      burn: 1, burn_why: "the heat of a burn (sim.wick37HotBurns rose): the brightest, most saturated thing on the wall",
      burnS: 0.8, burnS_why: "s: a burn's glow eases out over this",
      spanM: 3, spanM_why: "m: the heat patch's half width along the wall",
      rows: 3, rows_why: "seam rows lit above and below the heat's height (x Stack37SeamStep 1.2 m)",
      seamGainHot: 3.4, seamGainHot_why: "the seams' HDR gain at heat 1 (Stack37HotHex: the danger red-orange; NIGHT37's seams were 1.1 everywhere)",
      washGainHot: 1.3, washGainHot_why: "the iron's own glow behind the seams at heat 1 (NIGHT37's wash was 0.12 on every wall)",
      rippleHz: 1.2, rippleHz_why: "Hz: the lit rows' heat ripples upward (a pattern as well as a hue: XAG 103's second channel); far under 3 flashes a second",
      rippleAmp: 0.25, rippleAmp_why: "the ripple's depth (+-25 %: a shimmer, never a flash)",
    },
    proof: {
      _why: "a155's thresholds (FeelReel.N55.Boss.cs): the slate's doneWhen for P0-wickGrade / P0-hotWall, with the pre-mortem's sharper checks",
      wallMinDeg: 30, wallMinDeg_why: "the render's wall cells (tells out): mean hue at least this far from colors.enemy at p1, the turn and p2",
      frameMinDeg: 20, frameMinDeg_why: "D-16's whole-frame check (a254's measure, the HUD in) at p1, the turn and p2",
      nearDeg: 30, nearDeg_why: "a sample within this of the danger hue (S >= 0.2) counts as near it",
      nearMaxShare: 0.25, nearMaxShare_why: "B's sharper check: at most this share of the wall cells near the danger hue (a mean of two opposite hues can hide a red wall)",
      minHue: 25, minHue_why: "graded vs raw walls: the hue moved this much ...",
      minS: 0.1, minS_why: "... or the saturation this much",
      tellBandDeg: 18, tellBandDeg_why: "the danger-tell class: HSV hue within this of colors.enemy ...",
      tellSat: 0.5, tellSat_why: "... S at least this ...",
      tellVal: 0.25, tellVal_why: "... V at least this",
      tellMinS: 0.6, tellMinS_why: "the graded tells' mean S (the contract)",
      tellKeep: 0.8, tellKeep_why: "the graded frame keeps at least this share of the raw frame's tell cells (the grade does not desaturate them out of the class)",
      tellMaxShiftDeg: 10, tellMaxShiftDeg_why: "the tells' mean hue moves at most this graded vs raw",
      cyanMinRatio: 4.5, cyanMinRatio_why: "colors.usable vs the graded p1 frame's mean, WCAG luminance ratio",
      phaseMinHue: 20, phaseMinHue_why: "p1 -> p2 walls: the hue moves this much ...",
      phaseMinV: 0.1, phaseMinV_why: "... or the value this much (the lead's P1-wyrmTurn line for every graded boss)",
      coldMaxShare: 0.1, coldMaxShare_why: "no wall burning: the red-orange (danger-tell) cells at most this share of the world cells (the off run prints the shipped look's share: ON must also be at most half of it)",
      burnSatGap: 0.15, burnSatGap_why: "a burning wall's patch: mean S at least this over the other walls'",
      stripeMinDeg: 30, stripeMinDeg_why: "the cold seams' hue at least this far from the danger hue (or value-only: S < 0.15)",
      classMinN: 20, classMinN_why: "a class with fewer cells is left out of 'the most saturated class'",
      windowShare: 0.5, windowShare_why: "a155 FIGHT MODE (wick55): the damage dealt while the bar read OPEN, as a share of the damage dealt (n47human's measure; the slate's 50 %)",
    },
  },
};

ROWS.wick55 = {
  _why: "Builder B (Core/Enemies/Wick37.N55.cs, SurvivalSim.N55.Wick.cs; measured with n47human --boss wick --seeds 40 --rows wick55, the skilled human-model bot, and n55wick, the casual bot: L-058). THE ENCOUNTER only (DECIDE55 D-7): every attack keeps its id, kind, wind-up / active / recover, cost, weight, legs, habit, damage, knock and radius (Wick55Tests pins them). On top of wick54 as it ships. The web has no such numbers: THE WICK is Unity-only (NIGHT37).",
  chip: 0.3, chip_why: "the share a hit lands outside a window (wick54's 0.6; wick47's 0.3): the steady fire was 51 % of the damage (n47human, wick55 stats: chip 51 %, stagger windows 0 %)",
  stagger: [1, 1, 1], stagger_why: "the stagger meter per leg (wick37.kit [3, 3, 3]): one parry or pierce opens the window (read the yellow tell, parry, punish) (the baseline opened 17 windows in 40 fights)",
  windowSeconds: 4, windowSeconds_why: "s: the stagger window (design.n37.boss37 2.5): long enough to land the KESTREL's shots after the parry",
  windowGain: 2, windowGain_why: "a hit inside the stagger window lands this x full (the punish pays)",
  hot: {
    _why: "the hot wall's touch (SurvivalSim.Wick37.cs Wick37Rules: the throw-off unchanged)",
    brushBurns: false, brushBurns_why: "a brush in the air pushes the player off without a burn; a ride on the hot iron still burns (all 298 baseline burns were brushes while leaping between panels: 75 % of the hits taken came from the wall, not from a tell)",
  },
};

const files = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];
const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const before = JSON.parse(raw);
const want = process.argv.slice(2).length ? process.argv.slice(2) : Object.keys(ROWS);

function fmt(v, ind) {
  const pad = " ".repeat(ind);
  if (Array.isArray(v)) {
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

let lines = raw.split(eol);
for (const row of want) {
  if (!ROWS[row]) throw new Error("not one of B's rows: " + row);
  if (!before.design.n55[row]) throw new Error("no design.n55." + row);
  const open = lines.findIndex((l) => l.trim() === '"' + row + '": {');
  if (open < 0) throw new Error("no row line for " + row);
  const ind = lines[open].match(/^ */)[0].length;
  const end = lines.findIndex((l, k) => k > open && l.match(/^ */)[0].length === ind && l.trim().startsWith("}"));
  const n = lines.findIndex((l, k) => k > open && k < end && l.match(/^ */)[0].length === ind + 2 && l.trim().startsWith('"numbers": '));
  if (n < 0) throw new Error(row + ": its numbers key not found");
  // the numbers object runs from line n to its closing brace at the same indent (or is `{}` on one line)
  let last = n;
  if (!/^"numbers": \{\},?$/.test(lines[n].trim())) {
    last = lines.findIndex((l, k) => k > n && l.match(/^ */)[0].length === ind + 2 && l.trim().startsWith("}"));
    if (last < 0) throw new Error(row + ": the numbers object's end not found");
  }
  const comma = lines[last].trim().endsWith(",") ? "," : "";
  const text = (" ".repeat(ind + 2) + '"numbers": ' + fmt(ROWS[row], ind + 2) + comma).split(eol);
  lines = lines.slice(0, n).concat(text, lines.slice(last + 1));
}
const out = lines.join(eol);
const after = JSON.parse(out);
for (const row of want) {
  if (JSON.stringify(after.design.n55[row].numbers) !== JSON.stringify(ROWS[row])) throw new Error(row + ": written numbers differ from the script's");
  after.design.n55[row].numbers = before.design.n55[row].numbers;
}
const norm = (o) => JSON.stringify(o, (k, v) => (v && typeof v === "object" && !Array.isArray(v) ? Object.fromEntries(Object.entries(v).sort()) : v));
if (norm(after) !== norm(before)) throw new Error("something outside the rows' numbers changed");
for (const f of files) fs.writeFileSync(f, out);
console.log("design.n55 numbers written for " + want.join(", ") + " (both copies; enabled untouched)");
