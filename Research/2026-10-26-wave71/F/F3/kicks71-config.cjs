// NIGHT71 builder F (F3): design.n71.kicks71's numbers block, in both config copies (byte-identical). Touches ONLY the kicks71 line of
// design.n71 (the shared commit's one-line row): its `enabled` stays false; a "numbers" object is appended inside the same line. Idempotent
// (a line that already has "numbers" is replaced by this one), so the lead can re-apply it after a config conflict (L-049 / L-065).
// Usage (from the tree's root): node Research/2026-10-26-wave71/F/F3/kicks71-config.cjs [--dry]
'use strict';
const fs = require('fs');
const path = require('path');

const dry = process.argv.includes('--dry');
const files = ['Design/unity-config.json', 'Assets/StreamingAssets/Design/unity-config.json'];
const head = '      "kicks71": {"enabled": false, "enabled_why": "OFF until GG\'s keep (NIGHT71: ON only in the play build)", "_why": "F3 (T-77): feel46 kicks bigger and rarer"';
const numbers = {
  _why: "Builder F (NIGHT71 F3; Game/Kicks71.cs, Feel46View.cs). With the row on (feel46 on), feel46's G2-G4 run with these numbers whatever design.n46.feel46.numbers.kicks says. deg: view pitch-up (kicks) or FOV (the punch-in), x EFFECTS INTENSITY x CAMERA SHAKE / FOV KICK, none under REDUCED MOTION; s: KickStack's attack and recover on frame time. BIGGER: every kill kick above the KESTREL tap's own 1.2 + 1.8 deg fire kick (body < head < punch kept), recovered over 0.22 s (the fire kick's is shorter), the body kill's FOV punch-in 0.7 -> 1.2 deg (below the headshot kill's 1.5). RARER: a hit kick at most once per hitGapS (feel46 kicked every hit); a kill within killGapS of a kill kick adds only what its tier has over that kick (a multi-kill is one kick); the THUNDERCLAP's own shot keeps heavyFireKeep of its shake (feel46: 0.6). View only: no sim read, no tape bit. The web has no such numbers: its feel.js had no kill ladder on camera.",
  hitLightDeg: 0.6,
  hitHeavyDeg: 1.0,
  hitGapS: 0.35,
  hitAttackS: 0.02,
  hitRecoverS: 0.12,
  bodyKillDeg: 2.2,
  headKillDeg: 3.0,
  punchKillDeg: 3.6,
  killGapS: 0.12,
  killAttackS: 0.025,
  killRecoverS: 0.22,
  bodyKillFovDeg: 1.2,
  fovAttackS: 0.02,
  fovRecoverS: 0.16,
  heavyFireKeep: 0.35,
};
// one line, JSON.stringify's spacing made to match the row's (": " and ", ")
const body = JSON.stringify(numbers).replace(/":/g, '": ').replace(/,"/g, ', "');
const line = `${head}, "numbers": ${body}},`;

let out = [];
for (const rel of files) {
  const p = path.join(process.cwd(), rel);
  const text = fs.readFileSync(p, 'utf8');
  const lines = text.split('\n');
  const at = lines.findIndex((l) => l.startsWith('      "kicks71": {'));
  if (at < 0 || lines.findIndex((l, i) => i > at && l.startsWith('      "kicks71": {')) >= 0) throw new Error(`${rel}: the kicks71 line is missing or not unique`);
  const cr = lines[at].endsWith('\r') ? '\r' : ''; // a CRLF working copy (core.autocrlf) keeps its ending
  const was = cr ? lines[at].slice(0, -1) : lines[at];
  if (!was.startsWith(head)) throw new Error(`${rel}: the kicks71 line is not the shared commit's (its enabled / _why changed?): ${was.slice(0, 160)}`);
  if (!was.endsWith('},')) throw new Error(`${rel}: the kicks71 line does not end with "},"`);
  lines[at] = line + cr;
  out.push([p, lines.join('\n')]);
}
if (out[0][1] !== out[1][1]) throw new Error('the two copies would differ (they must be byte-identical before and after)');
JSON.parse(out[0][1]); // still JSON
if (dry) console.log(line);
else for (const [p, t] of out) fs.writeFileSync(p, t);
console.log(`${dry ? 'dry: ' : ''}kicks71 numbers: ${Object.keys(numbers).length - 1} numbers in ${files.length} copies`);
