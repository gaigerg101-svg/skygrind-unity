// NIGHT50 freeze (the lead's; DECIDE50 D-4, D-7, D-8, D-9): turns the four rows the night ships ON in both config copies, text-spliced so
// the rest of the file keeps its formatting. Every anchor must match exactly once; the result must parse and round-trip.
//   node Tools/night/n50-freeze.cjs [--dry]
// Rows ON: design.n50.charge50 (G1: the bar under the reticle, the pair won 3-0), design.n50.gunglow50 (G2: KESTREL and THUNDERCLAP; PULSE
// off in code), design.n49.fx49 (G4: the kit, the kill's wash still SHORT, said in enabled_why), design.n49.look49 with numbers.levels =
// [n48-foundry, n47-spire] (G3: the meter passes and the pair tied on both; kiln, iceline and span stay OFF).
const fs = require("fs");
const dry = process.argv.includes("--dry");
const files = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];
const raw0 = fs.readFileSync(files[0], "utf8");
if (raw0 !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ before the freeze");

function once(s, needle) {
  const n = s.split(needle).length - 1;
  if (n !== 1) throw new Error("anchor found " + n + " times: " + needle.slice(0, 80));
  return s.indexOf(needle);
}
// the block of a row: from its key to the end of its object (brace matching on the text)
function blockOf(s, key) {
  const start = once(s, '"' + key + '": {');
  let depth = 0, i = s.indexOf("{", start);
  for (; i < s.length; i++) {
    if (s[i] === "{") depth++;
    else if (s[i] === "}") { depth--; if (depth === 0) break; }
  }
  return [start, i + 1];
}
function setRow(s, key, enabledWhy) {
  const [a, b] = blockOf(s, key);
  let block = s.slice(a, b);
  const f = block.split('"enabled": false').length - 1;
  if (f !== 1) throw new Error(key + ": expected one enabled:false, found " + f);
  block = block.replace('"enabled": false', '"enabled": true');
  const w = block.match(/"enabled_why": "([^"]*)"/);
  if (!w) throw new Error(key + ": no enabled_why");
  block = block.replace(w[0], '"enabled_why": "' + enabledWhy + '"');
  return s.slice(0, a) + block + s.slice(b);
}

const WHY = {
  charge50: "ON at the NIGHT50 freeze (2026-10-19, G1, DECIDE50 D-5): the re-placed bar beat NIGHT46's placement 3-0 in both orders (Logs/n50/judge-keys/g1-charge.json); n50-r1 / n50-r2 a150: nearest pixel 22.5 px at 1080p (bar at least 18 px), 10 px tall (at least 4), the live fill 44 charging frames, 0 off the truth.",
  gunglow50: "ON at the NIGHT50 freeze (G2, DECIDE50 D-8): the four steps read on KESTREL and THUNDERCLAP (n50-r2 a150); PULSE is OFF in code (its steps sat inside the gauntlet's block: glow-gauntlet-0 and -75 read alike).",
  fx49: "ON at the NIGHT50 freeze (G4, DECIDE50 D-7 / D-9): the kit's pair won 3-0 (rej-4) and every tier but the kill passes the wash bar (light +0.17, heavy +0.27 with the kill, critical +0.20); the enemy kill stays SHORT at +0.27 on two frames (bar +0.25), n50-r2 a249.",
  look49: "ON at the NIGHT50 freeze (G3, DECIDE50 D-4): the grade is ON only on the levels in numbers.levels (n48-foundry, n47-spire): meter 4 of 4 on each (n50-r1 a349), pairs TIE on both (rej-5 spire, rej-6 foundry); kiln (meter 2 of 4), iceline and span (saturated band) stay OFF.",
};
const LEVELS = ["n48-foundry", "n47-spire"];

let s = raw0;
s = setRow(s, "charge50", WHY.charge50);
s = setRow(s, "gunglow50", WHY.gunglow50);
s = setRow(s, "fx49", WHY.fx49);
s = setRow(s, "look49", WHY.look49);
// the list (inserted by n50-shared.cjs as "levels": [],)
{
  const needle = '"levels": [],';
  const at = once(s, needle);
  const [a, b] = blockOf(s, "look49");
  if (at < a || at > b) throw new Error("levels is outside look49");
  s = s.slice(0, at) + '"levels": ["' + LEVELS.join('", "') + '"],' + s.slice(at + needle.length);
}

const back = JSON.parse(s);
const want = (o, p) => p.split(".").reduce((x, k) => x[k], o);
for (const [row, why] of Object.entries(WHY)) {
  const night = row === "charge50" || row === "gunglow50" ? "n50" : "n49";
  if (want(back, "design." + night + "." + row + ".enabled") !== true) throw new Error(row + " not on after the splice");
}
if (JSON.stringify(want(back, "design.n49.look49.numbers.levels")) !== JSON.stringify(LEVELS)) throw new Error("levels did not round-trip");
const before = JSON.parse(raw0);
for (const k of ["lights49", "climbTop48"]) {
  if (JSON.stringify(want(back, "design.n49." + k)) !== JSON.stringify(want(before, "design.n49." + k)) && k === "lights49") throw new Error("lights49 changed");
}
if (JSON.stringify(want(back, "design.n48")) !== JSON.stringify(want(before, "design.n48"))) throw new Error("design.n48 changed");

if (dry) { console.log("DRY RUN: four rows ON; levels " + JSON.stringify(LEVELS) + "; round-trip OK"); process.exit(0); }
for (const f of files) fs.writeFileSync(f, s);
console.log("NIGHT50 freeze written to both config copies: charge50, gunglow50, fx49, look49 (levels " + JSON.stringify(LEVELS) + ")");
