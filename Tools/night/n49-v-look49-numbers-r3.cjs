// NIGHT49 builder V round 3 (DECIDE49 D-18; the lead's n49-r2 meter): updates contrast, exposureEv and saturation (and their _why lines) in
// design.n49.look49.numbers to UnityTuning.V49.cs's round-3 values, in both config copies. Every other key of the row, and everything else in the
// config, is checked to round-trip unchanged. Run from the worktree root:  node Tools/night/n49-v-look49-numbers-r3.cjs
const fs = require("fs");
const path = require("path");
const R = (p) => path.join(__dirname, "..", "..", p);

const est = "from the builder's estimate on the n49-r2 look-on room and combat frames (contrast in log space round URP's 0.18 pivot, EV x 2^ev, a saturation lerp in linear light: not a Player measure)";
const set = {
  contrast: [25, 35, 45, 35, 42],
  contrast_why: "ColorAdjustments contrast per level (URP units; look33 had none; round 1-2: 25 / 25 / 15 / 15 / 22). Round 3 (DECIDE49 D-18; n49-r2 combat frame: foundry 4/4, iceline 1/4, kiln 2/4, span 0/4, spire 0/4): foundry kept, iceline 35, kiln 45, span 35, spire 42, " + est + ". Mirror of UnityTuning.V49Contrast.",
  exposureEv: [-0.30, -0.20, -0.30, -0.10, -0.65],
  exposureEv_why: "Post exposure per level, absolute EV (look33's base 0 + this; round 1-2: -0.30 / -0.20 / 0 / 0 / -0.20). Round 3 (n49-r2: span room dark 25 % mid 73 %, spire combat dark 7 % mid 82 %, kiln combat mid 63 %): kiln -0.30, span -0.10, spire -0.65; foundry and iceline kept, " + est + ". Mirror of UnityTuning.V49ExposureEv.",
  saturation: [23, 18, -30, 0, 28],
  saturation_why: "ColorAdjustments saturation added to the config grade's, per level (round 1: 8 / 8 / 4 / 8 / 8; round 2: 23 / 18 / -10 / 0 / 28 from the meter on n49-r1's look-on frames). Round 3: kiln -10 -> -30 (n49-r2 room 48 % saturated; with contrast 45 the estimate gives room 26 %, combat 12 %, and less of the olive cast); the others kept. Mirror of UnityTuning.V49Saturation.",
};

for (const p of ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"]) {
  const raw = fs.readFileSync(R(p), "utf8");
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";
  const d = JSON.parse(raw);
  const row = d.design && d.design.n49 && d.design.n49.look49;
  if (!row || !row.numbers || !row.numbers.contrast) throw new Error(p + ": look49.numbers from round 2 not found");
  const numbers = Object.assign({}, row.numbers, set); // key order kept
  const start = raw.indexOf("\"look49\": {");
  if (start < 0 || raw.indexOf("\"look49\": {", start + 1) >= 0) throw new Error(p + ": look49 not found exactly once");
  const key = "\"numbers\": {";
  const at = raw.indexOf(key, start);
  const next = raw.indexOf("\"lights49\": {", start);
  if (at < 0 || (next >= 0 && at > next)) throw new Error(p + ": look49's numbers not found");
  let depth = 0, end = -1;
  for (let i = at + key.length - 1; i < raw.length; i++) {
    if (raw[i] === "{") depth++;
    else if (raw[i] === "}") { depth--; if (depth === 0) { end = i + 1; break; } }
  }
  if (end < 0) throw new Error(p + ": numbers' end not found");
  const block = JSON.stringify(numbers, null, 2).split("\n").map((l, i) => (i === 0 ? l : "        " + l)).join(eol);
  const out = raw.slice(0, at) + "\"numbers\": " + block + raw.slice(end);
  const back = JSON.parse(out);
  if (JSON.stringify(back.design.n49.look49.numbers) !== JSON.stringify(numbers)) throw new Error(p + ": numbers did not round-trip");
  for (const k of Object.keys(row.numbers)) if (!(k in set) && JSON.stringify(back.design.n49.look49.numbers[k]) !== JSON.stringify(row.numbers[k])) throw new Error(p + ": " + k + " changed");
  const strip = (o) => { const c = JSON.parse(JSON.stringify(o)); delete c.design.n49.look49.numbers; return JSON.stringify(c); };
  if (strip(back) !== strip(d)) throw new Error(p + ": something other than look49.numbers changed");
  fs.writeFileSync(R(p), out);
  console.log(p + ": look49.numbers contrast / exposureEv / saturation updated (round 3)");
}
