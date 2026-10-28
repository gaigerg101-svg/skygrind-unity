// NIGHT40 builder W1 (charge40): writes ONLY builder W1's numbers inside design.n40.charge40 in Design/unity-config.json (text edit, the file's
// layout kept), then copies the file to Assets/StreamingAssets/Design/unity-config.json (byte-identical). The row's `enabled` is left as it is
// (the lead turns it on after the frames). Idempotent: every key after `enabled_why` is rewritten each run.
// Run: node Tools/night/n40w-config.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const CFG = path.join(ROOT, "Design/unity-config.json");
const SA = path.join(ROOT, "Assets/StreamingAssets/Design/unity-config.json");

const ROW = [
  ["revolverFullS", 0.9, "The KESTREL's FULL charge, seconds from the press (overrides design.n31.charge.revolver.fullSeconds 0.5 while on; whole steps: 108 at 120 Hz). GG 2026-10-13: 'charge shots for both weapons should take longer to load'. research/feel-web.md section 3: ULTRAKILL's Piercer 0.5 s (today's number), Apex's charge rifle 0.85 s, Destiny 2's fusion rifles 0.53-0.97 s (Precision 0.80 s), Splatoon's Splat Charger 1.0 s: 0.9 s sits at the fusion rifles' slow end, a hypothesis. p39's tapS 0.15 s is unchanged, so the charge reads 0.75 s of filling (was 0.35 s). A shop33 KESTREL charge multiplier still scales it (FullSteps: this x the shop's ratio)."],
  ["shotgunFullS", 1.2, "The THUNDERCLAP's FULL charge (the slug), seconds from the press (overrides design.n31.charge.shotgun.fullSeconds 0.7 while on; 144 steps). research/feel-web.md section 3: Splatoon's Splat Charger 1.0 s, the E-liter 4K 1.53 s, Destiny's High Impact fusion 0.97 s: 1.2 s is between the Splat Charger and the E-liter, kept longer than the KESTREL's 0.9 s as the slug is a whole volley x3 on one line (n31 kept the same order: 0.5 / 0.7). A hypothesis."],
  ["shotgunTapS", 0.15, "On the GROUND only (D-6): the THUNDERCLAP takes p39's hold rule (Charge35.StepP39). A press released inside this fires ONE volley on the release, aimed from the press (the correction below); held past it, NO volley fires and the hold charges the slug. Copied from design.n39.p39.tapS (0.15 s, 18 steps; its _why: above a deliberate 50-100 ms click, under tap36's holdStartS 0.18 s), so both guns split a tap from a hold at the same moment. In the AIR a press fires at once (tap36 as today) so the boost keeps its timing."],
  ["partialFires", true, "A ground hold released past shotgunTapS but before FULL fires the volley on the release (true: the trigger always answers, as design.n39.p39.partialFires) or drains (false: the Piercer's wind-down, feel-web.md section 3)."],
  ["correctFullS", 0.11, "A ground release volley fired up to this long after its press leaves from the PRESS's eye along the PRESS's aim (INVERSUS: full correction 0-0.11 s; research/feel-web.md section 3), as design.n39.p39.correctFullS for the KESTREL."],
  ["correctFadeS", 0.19, "The correction fades linearly to none at this age (INVERSUS: none by 0.19 s), as design.n39.p39.correctFadeS; shotgunTapS 0.15 s is inside it."],
];

let s = fs.readFileSync(CFG, "utf8");
const nl = s.includes("\r\n") ? "\r\n" : "\n";
const at = s.indexOf('"charge40": {');
if (at < 0) throw new Error("no design.n40.charge40 block");
const end = s.indexOf("}", at);
let block = s.slice(at, end);
const key = '"enabled_why": ';
const k = block.indexOf(key);
if (k < 0) throw new Error("charge40 has no enabled_why");
const lineEnd = block.indexOf('"', block.indexOf('"', k + key.length) + 1) + 1; // the end of the enabled_why string
let ind;
{
  const ls = block.lastIndexOf("\n", k) + 1;
  ind = block.slice(ls, k).match(/^\s*/)[0];
}
const closeInd = ind.slice(0, Math.max(0, ind.length - 2));
let tail = "";
for (const [name, val, why] of ROW) {
  tail += "," + nl + ind + JSON.stringify(name) + ": " + JSON.stringify(val);
  tail += "," + nl + ind + JSON.stringify(name + "_why") + ": " + JSON.stringify(why);
}
block = block.slice(0, lineEnd) + tail + nl + closeInd;
// --enable / --disable: flip the row's `enabled` (a local check with the row on; the lead turns it on for real after the frames)
if (process.argv.includes("--enable")) block = block.replace(/"enabled": (true|false)/, '"enabled": true');
if (process.argv.includes("--disable")) block = block.replace(/"enabled": (true|false)/, '"enabled": false');
s = s.slice(0, at) + block + s.slice(end);
JSON.parse(s); // still JSON
fs.writeFileSync(CFG, s);
fs.writeFileSync(SA, s);
console.log("n40w-config: design.n40.charge40 written (" + ROW.length + " numbers); StreamingAssets copy identical");
