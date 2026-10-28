// NIGHT32 builder B -> the lead (UnityTuning.Boss30.cs is not B's file): THE LOOM's drawn joint at 0.75 x its hit diameter (was 0.8), so the hit sphere is 1.33x the
// drawing (DESIGN32 B: hurtboxes at least 1.3x the model). View only (no sim, tape or fingerprint change). Run from the repo root: node Tools/night/n32b-loomjoint.cjs
const fs = require("fs");
const f = "Assets/Scripts/Game/UnityTuning.Boss30.cs";
let s = fs.readFileSync(f, "utf8");
const from = "Loom30JointScale = 0.8f";
const to = "Loom30JointScale = 0.75f";
if (s.indexOf(to) >= 0) { console.log("already patched"); process.exit(0); }
if (s.indexOf(from) < 0) { console.error("pattern not found: " + from); process.exit(1); }
s = s.replace(from, to);
fs.writeFileSync(f, s);
console.log("patched " + f + ": " + from + " -> " + to);
