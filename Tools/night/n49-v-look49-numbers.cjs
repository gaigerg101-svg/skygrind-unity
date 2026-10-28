// NIGHT49 builder V: design.n49.look49.numbers (the colour script's numbers, each with a _why), in both config copies. Touches ONLY that row's
// `numbers` (the other rows, n27-n48 and look49's enabled stay byte-for-byte). The code reads UnityTuning.V49.cs (render numbers, CLAUDE.md rule 5);
// these are the same values as data, for the lead's tables and the checker (V49Tests pins that the two agree). Run from the worktree root:
//   node Tools/night/n49-v-look49-numbers.cjs
const fs = require("fs");
const path = require("path");
const R = (p) => path.join(__dirname, "..", "..", p);

const order = "order = LightPlan33.Themes: foundry, iceline, kiln, span, spire";
const numbers = {
  hueDeg: [158, 216, 72, 280, 124],
  hueDeg_why: "The level's one dominant environment hue (deg; " + order + "). Each is >= 20 deg (Look49AccentMinHueGapDeg) from every role token in UnityTuning.Look49.cs (usable 186, danger 12, parry 44, RIVAL 312, boundary 250): gaps 27.6 / 30.4 / 28.4 / 29.8 / 61.6. Foundry jade (the lava's orange complement), iceline azure, kiln sulphur-olive, span dusk violet, spire emerald. Mirror of UnityTuning.V49HueDeg.",
  envSaturation: [0.45, 0.55],
  envSaturation_why: "The saturation band of every environment colour (fog, stone fog, ambient 0.45-0.5; rim 0.55), under the roles' 0.77-0.89 (the boundary is a pale, 0.18: judged by hue). UnityTuning.V49.cs comments give each hex's (hue, saturation, value).",
  fogDensity: [0.010, 0.010, 0.008, 0.006, 0.010],
  fogDensity_why: "URP exponential fog density per level (look33's were 0.002-0.007): the far room sinks into the level's dark hue; at 20 m 0.010 keeps 82 % of a foe's colour. Mirror of UnityTuning.V49FogDensity.",
  contrast: [25, 25, 15, 15, 22],
  contrast_why: "ColorAdjustments contrast per level (URP units; look33 had none): the lift that pulls SPIKE.md's 61-86 % mid band apart; kiln and span were already 61-63 % dark. Mirror of UnityTuning.V49Contrast.",
  exposureEv: [-0.30, -0.20, 0.00, 0.00, -0.20],
  exposureEv_why: "Post exposure per level, absolute EV (look33's base 0 + this). Foundry's WICK arena was 2 % dark / 29 % bright, iceline 32 % dark, spire 25 % bright: down; kiln and span unchanged. Mirror of UnityTuning.V49ExposureEv.",
  saturation: [8, 8, 4, 8, 8],
  saturation_why: "ColorAdjustments saturation added to the config grade's, per level (look33's 5 / -10 / 5 / 0 / 10); the environment inputs are low-saturation, so this mostly lifts the role colours over them. Mirror of UnityTuning.V49Saturation.",
  shadowTint: 0.35,
  shadowTint_why: "ShadowsMidtonesHighlights: the shadows' share of the level tint's luminance-neutral multiplier; the same on every level (one grade, only the hue moves). Mirror of UnityTuning.V49ShadowTint.",
  midTint: 0.18,
  midTint_why: "The midtones' share of the tint (the lit walls lean to the hue; highlights stay neutral so the emissive role colours keep their hue). Mirror of UnityTuning.V49MidTint.",
  shadowOffset: -0.12,
  shadowOffset_why: "The shadows' lightness (URP's w): deepens the dark base (the meter's dark share). Mirror of UnityTuning.V49ShadowOffset.",
  rimScale: 0.4,
  rimScale_why: "The stone's unshadowed rim (low, 18 deg up) in the level hue at lights.rimIntensity x LightScale x 3 x this (look33: 0.3 in the config's violet): the walls carry the hue, the floors stay darker. Mirror of UnityTuning.V49RimScale.",
  bloom: 0.5,
  bloom_why: "One bloom intensity on every level (UNIFY.md: global, never per level, so an emissive tier glows the same everywhere); the threshold stays lights35's 1.5 (L's emissive tiers read it). Mirror of UnityTuning.V49Bloom.",
};

for (const p of ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"]) {
  const raw = fs.readFileSync(R(p), "utf8");
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";
  const d = JSON.parse(raw);
  if (!d.design || !d.design.n49 || !d.design.n49.look49) throw new Error(p + ": design.n49.look49 is missing");
  const start = raw.indexOf("\"look49\": {");
  if (start < 0 || raw.indexOf("\"look49\": {", start + 1) >= 0) throw new Error(p + ": look49 not found exactly once");
  const key = "\"numbers\": {}";
  const at = raw.indexOf(key, start);
  const next = raw.indexOf("\"lights49\": {", start);
  if (at < 0 || (next >= 0 && at > next)) throw new Error(p + ": look49's empty numbers not found (already run?)");
  const indent = "        "; // the row's keys sit at 8 spaces
  const block = JSON.stringify(numbers, null, 2).split("\n").map((l, i) => (i === 0 ? l : indent + l)).join(eol);
  const out = raw.slice(0, at) + "\"numbers\": " + block + raw.slice(at + key.length);
  const back = JSON.parse(out);
  if (JSON.stringify(back.design.n49.look49.numbers) !== JSON.stringify(numbers)) throw new Error(p + ": numbers did not round-trip");
  const strip = (o) => { const c = JSON.parse(JSON.stringify(o)); delete c.design.n49.look49.numbers; return JSON.stringify(c); };
  if (strip(back) !== strip(d)) throw new Error(p + ": something other than look49.numbers changed");
  fs.writeFileSync(R(p), out);
  console.log(p + ": look49.numbers written (" + Object.keys(numbers).filter((k) => !k.endsWith("_why")).length + " numbers)");
}
