// NIGHT41 builder B8 round 2: design.n41.sweep41's measured changes (text-level, CRLF kept; both copies; idempotent):
// the damage 25 -> 15 and the tell's hook refill (tellRefill, refillOverY), each with its _why.
const fs = require('fs');
const path = require('path');
const root = path.resolve(__dirname, '..', '..');
const files = ['Design/unity-config.json', 'Assets/StreamingAssets/Design/unity-config.json'];
const damage = process.argv[2] || '15';
const damageWhy = 'MEASURED round 2 (`n41loom sweep --casual`): at 25 (the spool\'s) the casual pilot (who misses every second spool, L-058) died in 2 of 10 fights to a sweep on top of a lash and two spools; a floor cue should cost, not kill: ' + damage + ', half the lash\'s 30';
const add = [
  ['tellRefill', 'true', 'MEASURED round 2 (`n41loom sweep --trace`, seed 4): in leg 2 the pilot met the sweep on a tower top with 0 grapple charges (two across-pulls spent both, 4 s recharge): no answer existed; a sweep over the tower tops gives one hook back at its tell when you have none (as loom37\'s tearRefill), so the answer it asks for is in hand. Stay pilot 9/10 -> 10/10'],
  ['refillOverY', '8', 'the tower tops\' y (n40-span o3 towers top 8): only a sweep that covers them (legs 2-3, topY 11) refills; leg 1\'s (topY 3) is answered by standing on a tower top'],
];
for (const rel of files) {
  const p = path.join(root, rel);
  let t = fs.readFileSync(p, 'utf8');
  const start = t.indexOf('"sweep41": {');
  const end = t.indexOf('\r\n      }', start);
  let block = t.slice(start, end);
  block = block.replace(/"damage": [0-9.]+,\r\n        "damage_why": "(?:[^"\\]|\\.)*"/, `"damage": ${damage},\r\n        "damage_why": ${JSON.stringify(damageWhy)}`);
  for (const [k, v, why] of add) {
    if (block.includes(`"${k}"`)) continue;
    block += `,\r\n        "${k}": ${v},\r\n        "${k}_why": ${JSON.stringify(why)}`;
  }
  t = t.slice(0, start) + block + t.slice(end);
  JSON.parse(t);
  fs.writeFileSync(p, t);
  console.log(rel + ': patched');
}
