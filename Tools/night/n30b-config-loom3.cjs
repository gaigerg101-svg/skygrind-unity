// NIGHT30 builder B: THE LOOM's pool _why, measured (boss30 loom's lag bot), in design.n30.span.loom of Design/unity-config.json (text replacement; keeps the line endings; idempotent).
// Run from the repo root: node Tools/night/n30b-config-loom3.cjs
const fs = require('fs');
const path = 'Design/unity-config.json';
let text = fs.readFileSync(path, 'utf8');
const old = /"pool_why": "Hit points \(x the fight's hp scale\)\. A first pick: [^"]*"/;
if (!old.test(text)) { console.log('pool_why already measured (or not found)'); process.exit(0); }
text = text.replace(old, `"pool_why": "Hit points (x the fight's hp scale). Measured with boss30 loom's lag bot (a scripted model: hang on a leg, nails at its joints with a 0.6 hit chance and the nailgun's heat, a 0.35 s reaction to the CUT tell, 15 % of tells missed; 6 seeds): kills at 103.5-115.1 s, median 111, inside the 90-150 s band (D-5) at 2400."`);
fs.writeFileSync(path, text);
console.log('span.loom.pool_why: measured');
