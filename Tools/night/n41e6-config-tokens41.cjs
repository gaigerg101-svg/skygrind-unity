// NIGHT41 builder E6: adds tokens41's number (the cap) to design.n41.tokens41 in Design/unity-config.json by text insertion before the row's
// "enabled" line (keeps the line endings and the layout; idempotent), then copies the file to Assets/StreamingAssets/Design/unity-config.json
// (the two stay byte-identical). Patches only E6's own row. Run from the repo root: node Tools/night/n41e6-config-tokens41.cjs
const fs = require('fs');
const path = 'Design/unity-config.json';
const copy = 'Assets/StreamingAssets/Design/unity-config.json';
let text = fs.readFileSync(path, 'utf8');
const eol = text.includes('\r\n') ? '\r\n' : '\n';
const n41 = text.indexOf('"n41": {');
if (n41 < 0) throw new Error('no design.n41');
const at = text.indexOf('"tokens41": {', n41);
if (at < 0) throw new Error('no tokens41 row');
const enabled = text.indexOf('"enabled": false', at);
const end = text.indexOf('}', at);
if (enabled < 0 || enabled > end) throw new Error('no tokens41.enabled');
if (text.slice(at, end).includes('"cap"')) { console.log('already present'); }
else {
  const I = '        ';
  const rows = [
    `"cap": 2`,
    `"cap_why": "DECIDE41 D-5 (GG's default: 'at most two enemies winding up at once in the campaign'): the most enemies the brain lets be in an attack at once (Husk.committed: a wind-up, a pounce, a spit, a charge, a tell), across the token pools (melee 2, ranged 3, heavy 1, swarm 3, each +1 per round step and camping: their sum let 5 commit at once, more than 2 for 3.5 % of n40-iceline's fight time, E3 NIGHT40). Doom 2016's token pool (enemies-web.md section 3): the player reads two tells, not five. Survival keeps the web's pools (no cap)."`,
  ];
  let ins = '';
  for (const r of rows) ins += r + ',' + eol + I;
  text = text.slice(0, enabled) + ins + text.slice(enabled);
  fs.writeFileSync(path, text);
  console.log('tokens41: added cap');
}
fs.copyFileSync(path, copy);
console.log('copied to', copy);
