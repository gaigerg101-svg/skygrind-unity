// NIGHT46 builder V1 (round 4, the lead's ruling after n46-r2's blind pairs): design.n46.feel46.numbers.kicks = false. G1 (the off-screen
// wind-up warning) and G5 (no full-screen hurt static) earned ON; G2-G4 (the hit / kill kicks, the FOV punch, the THUNDERCLAP's shot shake)
// did not meet the blind-pair rule, so they run only when this is true (Feel46View.KicksOn). Touches ONLY design.n46.feel46: a text insert
// after its `_why` line (the rest of the file byte for byte), in Design/unity-config.json and its StreamingAssets copy (the two stay equal).
// Usage: node Tools/night/v1-feel46-kicks.cjs [rootDir] [--dry]
'use strict';
const fs = require('fs');
const path = require('path');

const args = process.argv.slice(2);
const dry = args.includes('--dry');
const root = path.resolve(args.find((a) => !a.startsWith('--')) || '.');
const files = ['Design/unity-config.json', 'Assets/StreamingAssets/Design/unity-config.json'];
const anchor = '      "feel46": {\n        "_why": "View row: camera / feedback feel (NightRows.ViewOn(cfg, \\"feel46\\")).",\n';
const insert =
  '        "numbers": {\n' +
  '          "_why": "Builder V1 (NIGHT46 round 4): feel46\'s parts. G1 (the off-screen wind-up warning) and G5 (the hurt static in the edge band, not over the whole screen) always run with the row on; the kicks only when kicks is true.",\n' +
  '          "kicks": false,\n' +
  '          "kicks_why": "G2 (a non-kill gun hit\'s view kick), G3 (the kill ladder\'s kicks and the body kill\'s FOV punch-in) and G4 (the THUNDERCLAP shot\'s shake under its kill\'s): off. n46-r2\'s blind pairs (DECIDE46 N-4: the lead and a reviewer, labels hidden) did not pick them ON in both (kill-000 lead ON / reviewer same; pumpkill-050 reviewer same), so they stay built and off for a later night."\n' +
  '        },\n';

let changed = 0;
for (const rel of files) {
  const p = path.join(root, rel);
  let text = fs.readFileSync(p, 'utf8');
  const crlf = text.includes('\r\n');
  if (crlf) text = text.replace(/\r\n/g, '\n');
  if (text.includes('"kicks_why"')) { console.log(`already in: ${rel}`); continue; }
  const at = text.indexOf(anchor);
  if (at < 0 || text.indexOf(anchor, at + 1) >= 0) throw new Error(`${rel}: the feel46 anchor is missing or not unique`);
  let out = text.slice(0, at + anchor.length) + insert + text.slice(at + anchor.length);
  JSON.parse(out); // still valid JSON
  if (crlf) out = out.replace(/\n/g, '\r\n');
  if (!dry) fs.writeFileSync(p, out);
  changed++;
  console.log(`${dry ? 'would write' : 'wrote'} ${rel}`);
}
console.log(`v1-feel46-kicks: ${changed} file(s)${dry ? ' (dry run)' : ''}`);
