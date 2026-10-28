// NIGHT30 builder B (round 3): the glide retune of THE GLACIER REBUILT: exposedScaleGlide and windowSecondsGlide in design.n30.glacier30 of Design/unity-config.json,
// inserted after the block's "speedMult_why" row (keeps the line endings; idempotent). Run from the repo root: node Tools/night/n30b-config-glacier3.cjs
const fs = require('fs');
const path = 'Design/unity-config.json';
let text = fs.readFileSync(path, 'utf8');
const eol = text.includes('\r\n') ? '\r\n' : '\n';
const at = text.indexOf('"glacier30": {', text.indexOf('"n30": {'));
if (at < 0) throw new Error('no glacier30 block');
const end = text.indexOf(eol + '      }', at);
if (text.slice(at, end).includes('"exposedScaleGlide"')) { console.log('already present'); process.exit(0); }
const m = /"speedMult_why": "[^"]*"/.exec(text.slice(at, end));
if (!m) throw new Error('no speedMult_why in glacier30');
const lineEnd = at + m.index + m[0].length;
const I = '        ';
const rows = [
  `"exposedScaleGlide": 0.35`,
  `"exposedScaleGlide_why": "With builder S's glide on (design.n30.glide) the exposed window lands this share of NIGHT29's damage (the gauntlet's third, the guns' exposed fraction). Measured with the chaser lag bot (boss30 glacier, 6 seeds): glide makes the chase easy (x1.0 / 2.5 s: median 48 s); x0.5 / 2.5 s: 76 s; x0.35 / 2.0 s: 111 s (101-118), inside the 80-130 s band (D-5). Glide off: x1 (NIGHT29's)."`,
  `"windowSecondsGlide": 2`,
  `"windowSecondsGlide_why": "With glide on the exposed window lasts this long (design.n29.boss2.exposedSeconds 2.5 otherwise): a glider reaches the boss more often, so each catch is shorter."`,
];
let ins = '';
for (const r of rows) ins += ',' + eol + I + r;
text = text.slice(0, lineEnd) + ins + text.slice(lineEnd);
fs.writeFileSync(path, text);
console.log('glacier30: glide retune added');
