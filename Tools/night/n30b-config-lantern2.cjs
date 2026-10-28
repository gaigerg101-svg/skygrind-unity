// NIGHT30 builder B (round 2): adds the platform camper's arcing orb and the rain decals' ring inset to design.n30.lantern30 in Design/unity-config.json, by text insertion
// before the block's "arena" object (keeps the line endings and the layout; idempotent: does nothing when "platformOrbSeconds" is already there).
// Run from the repo root: node Tools/night/n30b-config-lantern2.cjs
const fs = require('fs');
const path = 'Design/unity-config.json';
let text = fs.readFileSync(path, 'utf8');
const eol = text.includes('\r\n') ? '\r\n' : '\n';
const at = text.indexOf('"lantern30": {', text.indexOf('"n30": {'));
if (at < 0) throw new Error('no lantern30 block');
const arena = text.indexOf('"arena": {', at);
if (arena < 0) throw new Error('no lantern30.arena');
if (text.slice(at, arena).includes('"platformOrbSeconds"')) { console.log('already present'); process.exit(0); }
const I = '        ';
const rows = [
  `"platformOrbSeconds": 2.5`,
  `"platformOrbSeconds_why": "A player standing on a platform this long gets a slow ARCING orb (DESIGN30 C1: the refuge is temporary, the threat that sends him back to the wall). The wait restarts when he steps off."`,
  `"platformOrbTellSeconds": 1`,
  `"platformOrbTellSeconds_why": "The lob's landing spot is marked red-orange this long before it is thrown (tells 1-1.5 s, never over 2 s); the spot is where he stood when the tell began, so stepping aside dodges it."`,
  `"platformOrbFlightSeconds": 1.6`,
  `"platformOrbFlightSeconds_why": "The lob's flight from the lantern to the marked spot: slow enough to see it coming (the orb's own damage, design.n27.boss1.orb.damage 14)."`,
  `"platformOrbGravity": 9`,
  `"platformOrbGravity_why": "The lob's fall (m/s^2) that bends it into an arc over the floor (the world's gravity is the player's; an orb has none of its own)."`,
  `"rainInset": 1.5`,
  `"rainInset_why": "Phase 3's rain decals stay this far inside the ring's panel faces (the NIGHT27 clamp was a square, whose corners sit outside the ring)."`,
];
let ins = '';
for (const r of rows) ins += r + ',' + eol + I;
text = text.slice(0, arena) + ins + text.slice(arena);
fs.writeFileSync(path, text);
console.log('lantern30: added', rows.length / 2, 'numbers');
