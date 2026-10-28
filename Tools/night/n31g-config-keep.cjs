// NIGHT31 builder G round 3: design.n31.grapplePull.arriveKeepMax (Core/GrappleTap31.cs GrapplePull31Rules.Arrive, applied in SurvivalSim.Grapple31.cs right after
// the zip's "zip" release), with its _why, inserted into the block by text (layout and line endings kept). Re-runnable: an existing value is replaced in place.
// Run from the repo root: node Tools/night/n31g-config-keep.cjs [value]   (default 12; the measuring runs pass 10 / 12 / 16)
const fs = require('fs');
const path = 'Design/unity-config.json';
let text = fs.readFileSync(path, 'utf8');
const eol = text.includes('\r\n') ? '\r\n' : '\n';
const val = process.argv[2] !== undefined ? Number(process.argv[2]) : 12;
if (!Number.isFinite(val)) { console.error('n31g-config-keep: not a number'); process.exit(1); }
const key = 'arriveKeepMax';
const why = 'm/s: the HORIZONTAL speed kept after a pull lets go at its anchor (the Mover\'s own "zip" release), clamped from outside the Mover before the arrival pop; 0 = no clamp. Unity-only (the web\'s enemy zip kept everything; the web had no pull-in). Why: builder L\'s harness pull31 (round 3) measured that with the full carry (42-51 m/s) every level-4 gap was crossed by ONE tap from the door (s2 landed at the north wall, s3 at lz 35.6 past the gap\'s end at 22, s6 past the deck). MEASURED (pull31 and grapple31 at 10 / 12 / 16: see NIGHT31 G round 3): 12, about a dash, drops you short of s3\'s gap after its first node so the second node is the chain. A cancel BEFORE the arrival (a jump, a dash, a second press) keeps the whole speed: the skilful line.';
const n31At = text.indexOf('"n31": {');
const blockAt = text.indexOf('"grapplePull": {', n31At);
if (n31At < 0 || blockAt < 0) { console.error('n31g-config-keep: design.n31.grapplePull not found'); process.exit(1); }
const end = text.indexOf('}', blockAt);
const block = text.slice(blockAt, end);
const re = new RegExp('"' + key + '": [-0-9.]+');
if (re.test(block)) text = text.slice(0, blockAt) + block.replace(re, '"' + key + '": ' + val) + text.slice(end);
else {
  const insertAt = text.lastIndexOf(eol, end);
  const ind = '        ';
  text = text.slice(0, insertAt) + ',' + eol + ind + '"' + key + '_why": ' + JSON.stringify(why) + ',' + eol + ind + '"' + key + '": ' + val + text.slice(insertAt);
}
fs.writeFileSync(path, text);
console.log('n31g-config-keep: grapplePull = ' + JSON.stringify(JSON.parse(text).design.n31.grapplePull, (k, v) => (k.endsWith('_why') ? undefined : v)));
