// FORGE72: for the harvest candidates, print why each row is OFF (enabled_why, or the tail of _why), so no known-bad row is flipped.
//   node Research/2026-10-27-forge72/off-why.cjs > Research/2026-10-27-forge72/off-why.txt
const fs = require("fs");
const path = require("path");
const root = path.resolve(__dirname, "..", "..");
const d = JSON.parse(fs.readFileSync(path.join(root, "Design", "unity-config.json"), "utf8")).design;
const ROWS = ["n63.colours63", "n64.goal64", "n56.yellow56", "n55.decor55", "n61.fixture61", "n63.exit63", "n61.exitFrame61", "n60.lamp60",
  "n38.doors38", "n55.ssao55", "n63.eye63", "n55.fov55", "n55.step55", "n71.wyrm71", "n71.voice71", "n71.words71", "n71.cyan71",
  "n55.bossText55", "n55.finish55", "n55.speed55", "n47.hitzone47", "n41.models41", "n47.finale47", "n71.kicks71", "n63.press63",
  "n42.reticle42", "n40.pump40", "n71.wash71", "n71.glow71", "n41.select41", "n45.look45", "n46.shift46"];
for (const k of ROWS) {
  const [a, b] = k.split(".");
  const x = d[a] && d[a][b];
  if (!x) { console.log("## " + k + " MISSING"); continue; }
  const ew = String(x.enabled_why || "").replace(/\s+/g, " ");
  const w = String(x._why || "").replace(/\s+/g, " ");
  console.log("## " + k + " enabled=" + x.enabled);
  console.log("  enabled_why: " + (ew ? ew.slice(0, 600) : "(none)"));
  console.log("  _why tail: " + w.slice(Math.max(0, w.length - 500)));
}
