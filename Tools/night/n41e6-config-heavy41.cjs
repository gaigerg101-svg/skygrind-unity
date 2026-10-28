// NIGHT41 builder E6: adds heavy41's numbers to design.n41.heavy41 in Design/unity-config.json by text insertion before the row's "enabled"
// line (keeps the line endings and the layout; idempotent), then copies the file to Assets/StreamingAssets/Design/unity-config.json (the two
// stay byte-identical). Patches only E6's own row. Run from the repo root: node Tools/night/n41e6-config-heavy41.cjs
const fs = require('fs');
const path = 'Design/unity-config.json';
const copy = 'Assets/StreamingAssets/Design/unity-config.json';
let text = fs.readFileSync(path, 'utf8');
const eol = text.includes('\r\n') ? '\r\n' : '\n';
const n41 = text.indexOf('"n41": {');
if (n41 < 0) throw new Error('no design.n41');
const at = text.indexOf('"heavy41": {', n41);
if (at < 0) throw new Error('no heavy41 row');
const enabled = text.indexOf('"enabled": false', at);
const end = text.indexOf('}', at);
if (enabled < 0 || enabled > end) throw new Error('no heavy41.enabled');
if (text.slice(at, end).includes('"knockMult"')) { console.log('already present'); }
else {
  const I = '        ';
  const rows = [
    `"kinds": ["tank", "warden", "charger", "brawler"]`,
    `"kinds_why": "The big kinds the card names (NIGHT41.md E6, C7): the level-2 TANK (a husk body with Enemy.tank29, keyed 'tank'), the WARDEN, the CHARGER and THE ICEBREAKER's body (kind 'brawler'). A per-kind flag: a kind off this list takes the gauntlet as before."`,
    `"knockMult": 2`,
    `"knockMult_why": "The gauntlet's knock-back speed x2 on a big kind (weapons.gauntlet.knockback 7 m/s -> 14 m/s before the kind's own knockbackScale: the CHARGER 0.3, the WARDEN 0.2, which made the swing move them 0.3-0.4 m, a hit that reads as nothing). x2 with knockTime below roughly triples the shove (about 1-1.5 m on a CHARGER): visible, but not a launch that carries a body out of a small room."`,
    `"knockTime": 0.35`,
    `"knockTime_why": "The knock lasts at least 0.35 s on a big kind (weapons.gauntlet.knockbackTime 0.2): the body is stunned while knocked (Husk.Step), so the big hit also buys the player a beat, the answer GG asked the gauntlet to be against large enemies (w40). Under the gauntlet's cycle 0.42 s, so a swing chain cannot stun-lock it."`,
    `"hitstop": 0.14`,
    `"hitstop_why": "A big-kind gauntlet hit freezes 0.14 s, longer than every punch hit (design.n36.crunch36.punchHitS 0.1, ULTRAKILL's Punch HitStop) and under ULTRAKILL's major hitstop 0.25 s and design.impact.budget.maxFreeze 0.5: the weight of the hit is felt in time, not only distance. One swing per 0.42 s cycle keeps it inside the budget's rolling second (perSecond 0.2) at most every other hit."`,
  ];
  let ins = '';
  for (const r of rows) ins += r + ',' + eol + I;
  text = text.slice(0, enabled) + ins + text.slice(enabled);
  fs.writeFileSync(path, text);
  console.log('heavy41: added', rows.length / 2, 'numbers');
}
fs.copyFileSync(path, copy);
console.log('copied to', copy);
