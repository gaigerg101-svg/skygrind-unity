// NIGHT30 builder S: design.n30.lava's two extra numbers (enemyBurnSeconds, touchMargin) inserted after graceSeconds, by text (the file's layout and line
// endings kept). Idempotent: does nothing when enemyBurnSeconds is already there. Run from the repo root: node Tools/night/n30s-config-lava.cjs
const fs = require('fs');
const path = 'Design/unity-config.json';
let text = fs.readFileSync(path, 'utf8');
if (text.includes('"enemyBurnSeconds"')) { console.log('n30s-config-lava: already patched'); process.exit(0); }
const eol = text.includes('\r\n') ? '\r\n' : '\n';
const anchor = '"graceSeconds": 0.5' ;
const at = text.indexOf(anchor, text.indexOf('"lava": {', text.indexOf('"n30": {')));
if (at < 0) { console.error('n30s-config-lava: anchor not found'); process.exit(1); }
const ind = '        ';
const add = ',' + eol +
  ind + '"enemyBurnSeconds_why": "Unity-only (the web has no lava): an enemy whose feet touch lava burns (event lavaEnemyBurn, a visible burn) for this long and then dies (lavaEnemyDeath), so it never vanishes silently; 0.8 s reads as a burn without the body lingering.",' + eol +
  ind + '"enemyBurnSeconds": 0.8,' + eol +
  ind + '"touchMargin_why": "Unity-only: the feet count as touching the lava at or below its surface y + touchMargin m (NIGHT30.md / DESIGN30: 0.05), so a body standing on the lava collider always reads as in it.",' + eol +
  ind + '"touchMargin": 0.05';
text = text.slice(0, at + anchor.length) + add + text.slice(at + anchor.length);
fs.writeFileSync(path, text);
console.log('n30s-config-lava: added enemyBurnSeconds 0.8, touchMargin 0.05');
