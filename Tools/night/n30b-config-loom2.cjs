// NIGHT30 builder B: THE LOOM's hip joints move outside the deck's edge and under it (a player on the deck could stand within fullReach of a hip placed over the deck):
// replaces design.n30.span.loom.hipShare with hipOut / hipDrop in Design/unity-config.json (text replacement; keeps the line endings; idempotent). Run: node Tools/night/n30b-config-loom2.cjs
const fs = require('fs');
const path = 'Design/unity-config.json';
let text = fs.readFileSync(path, 'utf8');
const eol = text.includes('\r\n') ? '\r\n' : '\n';
const at = text.indexOf('"loom": {', text.indexOf('"span": {', text.indexOf('"n30": {')));
if (at < 0) throw new Error('no span.loom block');
if (text.indexOf('"hipOut"', at) >= 0 && text.indexOf('"hipOut"', at) < text.indexOf('}', at)) { console.log('already present'); process.exit(0); }
const rx = /"hipShare": [0-9.]+,(\r?\n)(\s*)"hipShare_why": "[^"]*",/;
const seg = text.slice(at);
const m = rx.exec(seg);
if (!m) throw new Error('hipShare rows not found');
const ind = m[2];
const rep = `"hipOut": 2,${eol}${ind}"hipOut_why": "The hip joint stands this far OUT past its grip (m), just beyond the deck's edge.",${eol}${ind}"hipDrop": 2.5,${eol}${ind}"hipDrop_why": "...and this far UNDER the deck (m): over the lava, so only a swing reaches it; from the deck's edge it is more than fullReach away (the harness prints the distance).",`;
text = text.slice(0, at + m.index) + rep + text.slice(at + m.index + m[0].length);
fs.writeFileSync(path, text);
console.log('span.loom: hipOut / hipDrop');
