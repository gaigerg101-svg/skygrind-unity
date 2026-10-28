// NIGHT49 builder V round 2 (DECIDE49 D-17 and the lead's sheet read: saturation 5-9 % on the combat frames, span's pink floor, kiln's olive mud):
// rewrites design.n49.look49.numbers (written by n49-v-look49-numbers.cjs in round 1) with the round-2 values of UnityTuning.V49.cs, in both config
// copies. Touches ONLY that row's `numbers` (checked: everything else round-trips unchanged). Run from the worktree root:
//   node Tools/night/n49-v-look49-numbers-r2.cjs
const fs = require("fs");
const path = require("path");
const R = (p) => path.join(__dirname, "..", "..", p);

const order = "order = LightPlan33.Themes: foundry, iceline, kiln, span, spire";
const numbers = {
  hueDeg: [158, 216, 66, 272, 124],
  hueDeg_why: "The level's one dominant environment hue (deg; " + order + "). Each is >= 20 deg (Look49AccentMinHueGapDeg) from every role token in UnityTuning.Look49.cs (usable 186, danger 12, parry 44, RIVAL 312, boundary 250): gaps 27.6 / 30.4 / 22.4 / 21.8 / 61.6. Round 2: kiln 72 -> 66 (the lead read n49-r1's kiln as olive mud: 66 is the warmest hue the rule allows), span 280 -> 272 (n49-r1's span floor went pink, near RIVAL: now 39.6 deg from RIVAL). Mirror of UnityTuning.V49HueDeg.",
  envSaturation: [0.25, 0.65],
  envSaturation_why: "The saturation band of every environment colour, under the roles' 0.77-0.89 (the boundary is a pale, 0.18: judged by hue). Round 2: the walls' rim 0.65 (was 0.55; kiln's 0.40), the grade tints that reach the floors weaker (0.25-0.50, were 0.5): the floors less saturated than the walls (the lead's ask). UnityTuning.V49.cs comments give each hex's (hue, saturation, value).",
  fogDensity: [0.010, 0.010, 0.008, 0.006, 0.010],
  fogDensity_why: "URP exponential fog density per level (look33's were 0.002-0.007): the far room sinks into the level's dark hue; at 20 m 0.010 keeps 82 % of a foe's colour. Unchanged in round 2. Mirror of UnityTuning.V49FogDensity.",
  contrast: [25, 25, 15, 15, 22],
  contrast_why: "ColorAdjustments contrast per level (URP units; look33 had none): the lift that pulls SPIKE.md's 61-86 % mid band apart; kiln and span were already 61-63 % dark. Unchanged in round 2 (the lead: dark and mid in range on n49-r1). Mirror of UnityTuning.V49Contrast.",
  exposureEv: [-0.30, -0.20, 0.00, 0.00, -0.20],
  exposureEv_why: "Post exposure per level, absolute EV (look33's base 0 + this). Unchanged in round 2 (dark and mid in range on n49-r1). Mirror of UnityTuning.V49ExposureEv.",
  saturation: [23, 18, -10, 0, 28],
  saturation_why: "ColorAdjustments saturation added to the config grade's, per level (round 1: 8 / 8 / 4 / 8 / 8). Round 2 from the look meter on the n49-r1 look-on frames (saturated %, room / floor / arena / boss / combat): foundry 7/5/12/33/9, iceline 22/10/57/32/6, kiln 55/62/49/46/55 (over the 30 % top), span 36/43/35/40/5, spire 6/1/15/26/25. The builder's estimate on those frames (a saturation lerp in linear light; not a Player measure) puts each level's median frame at 15-25 %. Mirror of UnityTuning.V49Saturation.",
  shadowTint: 0.35,
  shadowTint_why: "ShadowsMidtonesHighlights: the shadows' share of the level tint's luminance-neutral multiplier; the same on every level (one grade). Unchanged; round 2 weakened the per-level tint colours instead. Mirror of UnityTuning.V49ShadowTint.",
  midTint: 0.18,
  midTint_why: "The midtones' share of the tint (highlights stay neutral so the emissive role colours keep their hue). Unchanged. Mirror of UnityTuning.V49MidTint.",
  shadowOffset: -0.12,
  shadowOffset_why: "The shadows' lightness (URP's w): deepens the dark base (the meter's dark share). Unchanged. Mirror of UnityTuning.V49ShadowOffset.",
  rimScale: 0.6,
  rimScale_why: "The stone's unshadowed rim (low, 18 deg up) in the level hue at lights.rimIntensity x LightScale x 3 x this (look33: 0.3; round 1: 0.4). Round 2: 0.6, so the walls carry the level hue and the floors (lit from above) do not. Mirror of UnityTuning.V49RimScale.",
  bloom: 0.5,
  bloom_why: "One bloom intensity on every level (UNIFY.md: global, never per level, so an emissive tier glows the same everywhere); the threshold stays lights35's 1.5 (L's emissive tiers read it). Unchanged. Mirror of UnityTuning.V49Bloom.",
};

for (const p of ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"]) {
  const raw = fs.readFileSync(R(p), "utf8");
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";
  const d = JSON.parse(raw);
  const row = d.design && d.design.n49 && d.design.n49.look49;
  if (!row || !row.numbers || !row.numbers.hueDeg) throw new Error(p + ": look49.numbers from round 1 not found");
  const start = raw.indexOf("\"look49\": {");
  if (start < 0 || raw.indexOf("\"look49\": {", start + 1) >= 0) throw new Error(p + ": look49 not found exactly once");
  const key = "\"numbers\": {";
  const at = raw.indexOf(key, start);
  const next = raw.indexOf("\"lights49\": {", start);
  if (at < 0 || (next >= 0 && at > next)) throw new Error(p + ": look49's numbers not found");
  // the numbers object ends at its matching brace (no braces inside its strings: checked)
  let depth = 0, end = -1;
  for (let i = at + key.length - 1; i < raw.length; i++) {
    if (raw[i] === "{") depth++;
    else if (raw[i] === "}") { depth--; if (depth === 0) { end = i + 1; break; } }
  }
  if (end < 0) throw new Error(p + ": numbers' end not found");
  const indent = "        ";
  const block = JSON.stringify(numbers, null, 2).split("\n").map((l, i) => (i === 0 ? l : indent + l)).join(eol);
  const out = raw.slice(0, at) + "\"numbers\": " + block + raw.slice(end);
  const back = JSON.parse(out);
  if (JSON.stringify(back.design.n49.look49.numbers) !== JSON.stringify(numbers)) throw new Error(p + ": numbers did not round-trip");
  const strip = (o) => { const c = JSON.parse(JSON.stringify(o)); delete c.design.n49.look49.numbers; return JSON.stringify(c); };
  if (strip(back) !== strip(d)) throw new Error(p + ": something other than look49.numbers changed");
  fs.writeFileSync(R(p), out);
  console.log(p + ": look49.numbers rewritten (round 2)");
}
