// NIGHT30 builder B (round 5): THE LOOM's SWEEP bar at shin height: sweepHeight 1.0 -> 0.6 (measured: a standing jump's feet peak at 1.18 m, so a 1.0 m bar was cleared only at the apex),
// in design.n30.span.loom of Design/unity-config.json (text replacement; keeps the line endings; idempotent). Run: node Tools/night/n30b-config-sweep3.cjs
const fs = require('fs');
const path = 'Design/unity-config.json';
let text = fs.readFileSync(path, 'utf8');
const eol = text.includes('\r\n') ? '\r\n' : '\n';
const rx = /"sweepHeight": 1,(\r?\n)(\s*)"sweepHeight_why": "The bar's height over the deck \(m\): [^"]*",/;
if (!rx.test(text)) { console.log('already 0.6 (or not found)'); process.exit(0); }
text = text.replace(rx, (s, a, i) => `"sweepHeight": 0.6,${eol}${i}"sweepHeight_why": "The bar's height over the deck (m), shin height: a standing jump's feet peak at 1.18 m (harness), so a jump timed about 0.35 s before the bar clears it; a grapple off the deck too.",`);
fs.writeFileSync(path, text);
console.log('span.loom.sweepHeight 0.6');
