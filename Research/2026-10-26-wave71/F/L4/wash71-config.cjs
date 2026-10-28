// NIGHT71 builder F (L4): design.n71.wash71's numbers block, in both config copies (byte-identical). Touches ONLY the wash71 line of design.n71
// (the shared commit's one-line row): its `enabled` stays false; a "numbers" object is appended inside the same line. Idempotent, CRLF-safe,
// so the lead can re-apply it after a config conflict (L-049 / L-065).
// Usage (from the tree's root): node Research/2026-10-26-wave71/F/L4/wash71-config.cjs [--dry]
'use strict';
const fs = require('fs');
const path = require('path');

const dry = process.argv.includes('--dry');
const files = ['Design/unity-config.json', 'Assets/StreamingAssets/Design/unity-config.json'];
const key = '      "wash71": {';
const head = '      "wash71": {"enabled": false, "enabled_why": "OFF until GG\'s keep (NIGHT71: ON only in the play build)", "_why": "L4 (T-105): the kill wash f3-f4 source"';
const numbers = {
  _why: "Builder F (NIGHT71 L4; Game/Wash71.cs, LightPools.Light54.cs, Polish53.Item). With the row on (polish53 on): polish53's killWash item is live with its shipped numbers (design.n53.polish53.numbers.killWash: whiteFrames 2, silhouetteFrames 2, burstK 0.5, ringInWash 0), and a KILL's pop light (effects.js pop: fx.popIntensity 60 over fx.popDistance 9 m for fx.popTime 0.12 s, k^2, the enemy's colour) fades on frame time from the kill instead of world time, so a kill's freeze no longer holds the room lit at the pop's peak (EXT53's crops: the foundry box's f1 -> f3 rise is the room lit red-orange, warm-lit 0.011 -> 0.214 of the box's luma; the reel's tail f5 +0.236 .. f8 +0.126 is that light's world-time fade after the hold). A blast's pop and the impact glow unchanged. View only: no sim read, no tape bit. The web has no such numbers: its pop faded on its own clock with no freeze to hold it.",
  popFrameClock: 1,
  popFrameClock_why: '1: a kill pop fades on the frame dt from the kill (the freeze does not hold it); 0: on the world dt (held through a freeze), as shipped',
  popK: 1,
  popK_why: "x the kill pop's peak (fx.popIntensity 60): 1 keeps the kill's own pop on its first frames; under 1 dims it (a second value for the lead's proof slot)",
};
const body = JSON.stringify(numbers).replace(/":/g, '": ').replace(/,"/g, ', "');
const line = `${head}, "numbers": ${body}},`;

const out = [];
for (const rel of files) {
  const p = path.join(process.cwd(), rel);
  const lines = fs.readFileSync(p, 'utf8').split('\n');
  const at = lines.findIndex((l) => l.startsWith(key));
  if (at < 0 || lines.findIndex((l, i) => i > at && l.startsWith(key)) >= 0) throw new Error(`${rel}: the wash71 line is missing or not unique`);
  const cr = lines[at].endsWith('\r') ? '\r' : '';
  const was = cr ? lines[at].slice(0, -1) : lines[at];
  if (!was.startsWith(head)) throw new Error(`${rel}: the wash71 line is not the shared commit's: ${was.slice(0, 160)}`);
  if (!was.endsWith('},')) throw new Error(`${rel}: the wash71 line does not end with "},"`);
  lines[at] = line + cr;
  out.push([p, lines.join('\n')]);
}
if (out[0][1] !== out[1][1]) throw new Error('the two copies would differ (they must be byte-identical)');
JSON.parse(out[0][1]);
if (dry) console.log(line);
else for (const [p, t] of out) fs.writeFileSync(p, t);
console.log(`${dry ? 'dry: ' : ''}wash71 numbers: 2 numbers in ${files.length} copies`);
