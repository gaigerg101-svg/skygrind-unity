// NIGHT30 builder B: adds the KILN WYRM head-punch's extra numbers (design.n30.wyrm30) to Design/unity-config.json by text insertion after the block's "recoilSeconds" line
// (keeps the line endings and the layout; idempotent: does nothing when "recoilBack" is already in the block). Run from the repo root: node Tools/night/n30b-config-wyrm.cjs
const fs = require('fs');
const path = 'Design/unity-config.json';
let text = fs.readFileSync(path, 'utf8');
const eol = text.includes('\r\n') ? '\r\n' : '\n';
const at = text.indexOf('"wyrm30": {', text.indexOf('"n30": {'));
if (at < 0) throw new Error('no wyrm30 block');
const end = text.indexOf(eol + '      }', at);
if (text.slice(at, end).includes('"recoilBack"')) { console.log('wyrm30 numbers already present'); process.exit(0); }
const key = '"recoilSeconds": 1.2';
const k = text.indexOf(key, at);
if (k < 0 || k > end) throw new Error('no recoilSeconds line in wyrm30');
const lineEnd = k + key.length;
const I = '        ';
const rows = [
  `"recoilBack": 6`,
  `"recoilBack_why": "How far the staggered head recoils, away from the player along the ground (m): the recoil arc's end, clamped inside the room (D-8: the head lodges backward and the body coils back)."`,
  `"recoilUp": 2.5`,
  `"recoilUp_why": "How high the recoil arc lifts the head at its middle (m): a knockback ARC, not a slide."`,
  `"headReachMargin": 0.6`,
  `"headReachMargin_why": "The swing counts as ON THE HEAD when the head's box is within weapons.gauntlet.range plus this margin of the eye (the swing already picked the worm; this tells the head from the body)."`,
  `"laneMargin": 0.5`,
  `"laneMargin_why": "How far off the head's line of travel the player may stand and still be 'charged at' (m, beyond the head's half-width and the capsule's radius): the window opens only for a charge that will hit him."`,
  `"lateSeconds": 0.05`,
  `"lateSeconds_why": "The window stays open this long past the predicted contact (the contact cooldown's first step: a punch in the same step as the touch still counts)."`,
];
let ins = '';
for (const r of rows) ins += ',' + eol + I + r;
text = text.slice(0, lineEnd) + ins + text.slice(lineEnd);
fs.writeFileSync(path, text);
console.log('wyrm30: added', rows.length / 2, 'numbers');
