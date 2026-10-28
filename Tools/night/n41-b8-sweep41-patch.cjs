// NIGHT41 builder B8: adds design.n41.sweep41's numbers (each with its _why) after the row's enabled_why line, in Design/unity-config.json and
// its Assets/StreamingAssets copy (text-level: the file is CRLF with hand-kept inline arrays, so JSON.stringify would rewrite it). Idempotent.
const fs = require('fs');
const path = require('path');
const root = path.resolve(__dirname, '..', '..');
const files = ['Design/unity-config.json', 'Assets/StreamingAssets/Design/unity-config.json'];
const rows = [
  ['windup', '1.2', 'the wind-up (the red-orange floor decal tell): boss_rules 1 asks at least 0.9 s; 1.2 s as THE LOOM\'s lash and spool (design.n37.loom37.attacks), time to reach a tower top or throw a hook'],
  ['active', '0.9', 'the line crosses the canyon wall to wall (44 m) in 0.9 s: ~49 m/s, too fast to outrun along the floor, so the answer is height, not a sidestep'],
  ['recover', '0.7', 'the tail after the pass (a punish beat), as the web wall\'s 0.6 s plus a little: the line is long'],
  ['cost', '2', 'Boss37 budget points (1-3), as the lash (a full-damage red attack)'],
  ['weight', '1', 'the pick\'s weight: an even roll with lash (1) and spool (1.2), so about one attack in three or four is a sweep'],
  ['legs', '[1, 2, 3]', 'every leg: in leg 1 it teaches (the tower tops answer), in legs 2-3 (two thirds of the ride walls refuse a ride) it is the reason to hook'],
  ['damage', '25', 'as the spool (design.n37.loom37.spoolDamage 25), under the lash\'s 30: a floor cue should cost, not kill'],
  ['widthM', '2.5', 'the line\'s half thickness for the pass test, as the lash (lashWidthM 2.5)'],
  ['halfZ', '18', 'the band along the canyon either side of where you stood at the wind-up: 36 m, one tower pair\'s spacing (36 m), so the decal covers your part of the canyon, not all 140 m'],
  ['topY', '[3, 11, 11]', 'the sweep\'s height over the ledges (y 0) per leg: leg 1 covers the start / end ledges (a jump\'s ~1.3 m does not clear 3) and leaves the tower tops (y 8); legs 2-3 cover the tower tops too (a jump from y 8 does not clear 11): a hook (the knees hang at ~14) or a ride on a wall that still allows one'],
  ['liftUp', '7', 'the thread scoops you up on a hit (m/s up, the Mover\'s Impulse): a readable hit, not a fall into the canyon'],
];
for (const rel of files) {
  const p = path.join(root, rel);
  let t = fs.readFileSync(p, 'utf8');
  const start = t.indexOf('"sweep41": {');
  if (start < 0) throw new Error('no sweep41 row in ' + rel);
  const end = t.indexOf('\r\n      }', start);
  const block = t.slice(start, end);
  if (block.includes('"windup"')) { console.log(rel + ': already patched'); continue; }
  const ew = block.indexOf('"enabled_why"');
  const lineEnd = t.indexOf('\r\n', start + ew);
  let add = '';
  // the enabled_why line has no trailing comma (it was the last key): add one, then the numbers, the last without a comma
  const lines = rows.map(([k, v, why], i) => `        "${k}": ${v},\r\n        "${k}_why": ${JSON.stringify(why)}${i < rows.length - 1 ? ',' : ''}`);
  add = ',\r\n' + lines.join('\r\n');
  t = t.slice(0, lineEnd) + add + t.slice(lineEnd);
  JSON.parse(t);
  fs.writeFileSync(p, t);
  console.log(rel + ': patched');
}
