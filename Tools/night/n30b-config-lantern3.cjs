// NIGHT30 builder B (round 2, the Player's findings): adds design.n30.lantern30.floorScale to Design/unity-config.json by text insertion before the block's "arena" object
// (keeps the line endings and the layout; idempotent). Run from the repo root: node Tools/night/n30b-config-lantern3.cjs
const fs = require('fs');
const path = 'Design/unity-config.json';
let text = fs.readFileSync(path, 'utf8');
const eol = text.includes('\r\n') ? '\r\n' : '\n';
const at = text.indexOf('"lantern30": {', text.indexOf('"n30": {'));
if (at < 0) throw new Error('no lantern30 block');
const arena = text.indexOf('"arena": {', at);
if (text.slice(at, arena).includes('"floorScale"')) { console.log('already present'); process.exit(0); }
const I = '        ';
const rows = [
  `"floorScale": 0.5`,
  `"floorScale_why": "Shots from the FLOOR land this share of Lantern29's light damage (DESIGN30 C1: light damage from the floor; the platform lands none, the wall in the open core lands full). Round 1's Player reel projected a floor camper at 143 s against a 66.6 s rider (under 3x); x0.5 doubles the camper's time (harness: see the commit)."`,
];
let ins = '';
for (const r of rows) ins += r + ',' + eol + I;
text = text.slice(0, arena) + ins + text.slice(arena);
fs.writeFileSync(path, text);
console.log('lantern30: floorScale added');
