// NIGHT30 builder B: THE GLACIER REBUILT's measured numbers (boss30 glacier, 6 seeds): farMeters 22 -> 40 and a new speedMult 0.92, in design.n30.glacier30 of Design/unity-config.json
// (text replacement inside the block; keeps the line endings; idempotent: does nothing when "speedMult" is already there). Run from the repo root: node Tools/night/n30b-config-glacier2.cjs
const fs = require('fs');
const path = 'Design/unity-config.json';
let text = fs.readFileSync(path, 'utf8');
const eol = text.includes('\r\n') ? '\r\n' : '\n';
const at = text.indexOf('"glacier30": {', text.indexOf('"n30": {'));
if (at < 0) throw new Error('no glacier30 block');
const end = text.indexOf(eol + '      }', at);
let block = text.slice(at, end);
if (block.includes('"speedMult"')) { console.log('already present'); process.exit(0); }
const oldFar = /"farMeters": 22,\r?\n(\s*)"farMeters_why": "[^"]*"/;
if (!oldFar.test(block)) throw new Error('farMeters 22 row not found');
block = block.replace(oldFar, (m, ind) =>
  `"farMeters": 40,${eol}${ind}"farMeters_why": "Distance is punished (DESIGN30 C2): a player farther than this (straight line, chest to the boss) fills the patience gauge. Measured (boss30 glacier): DESIGN30's guess of 22 m sat under the chase itself (the boss holds design.n28.glacier.speed.targetGap 48 m along the lap while the glove is ready), so the chaser was volleyed for chasing: 4-10 deaths, kills at 165-233 s; at 40 m the chaser is never far and the bank camper is still reached (out of sight behind the island, and the sweep)."`
  + `,${eol}${ind}"speedMult": 0.92,${eol}${ind}"speedMult_why": "The boss's pace on the bigger lap (x design.n29.boss2.speedMult): the 124 m lap is half again the old 84 m. Measured with the chaser lag bot (6 seeds): 0.80 -> median 63 s, 0.88 -> 80, 0.90 -> 74, 0.92 -> 95 (60-135), 0.95 -> 104 with one seed not killed in 240 s; 0.92 sits in the 80-130 s band (D-5)."`);
text = text.slice(0, at) + block + text.slice(end);
fs.writeFileSync(path, text);
console.log('glacier30: farMeters 40, speedMult 0.92');
