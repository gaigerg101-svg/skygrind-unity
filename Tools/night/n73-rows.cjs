#!/usr/bin/env node
// NIGHT73's shared commit (a copy of n72-rows.cjs with the n73 names): pre-places NIGHT73's rows OFF so builders never add the block.
//   - `design.n73` in BOTH config copies, inserted as text just before the `"n72": {` block, byte-identical after; each row
//     {"enabled": false, "enabled_why": "...", "_why": "<slice>: <what>"};
//   - the view rows appended to SameRun63Math.FrameRows, so -skygrindSameRun63 pairs work.
// View rows only. Rows72.cs already reads design.n<NN>.<row> by the row's digits (`-skygrindRows72 kiln73:1`); NightRows.ViewOn too.
//   node Tools/night/n73-rows.cjs [--dry]
'use strict';
const fs = require('fs');
const path = require('path');
const argv = process.argv.slice(2);
const ROOT = path.resolve(path.join(__dirname, '..', '..'));
const dry = argv.includes('--dry');
const CFG = ['Design/unity-config.json', 'Assets/StreamingAssets/Design/unity-config.json'];
const FR = 'Assets/Scripts/Game/SameRun63Math.N63D.cs';

const ROWS = [
  ['kiln73', 'K1 (CLAUDE.md rule 8; GG 2026-10-22 "effect colores everything doesnt feel alligned"): THE KILN WYRM\'s arena read 88-96 % danger-hue pixels in n72-play (Tools/night/n73-hue.py); its surfaces take the kiln\'s palette72 family, red-orange kept for the wyrm, its tells and the lava', true],
];

const WHY_OFF = "OFF until the lead's ON flip (NIGHT72 D-1, kept by NIGHT73: its same-build pair, a SPOT naming the change, the guards)";
const rowsJson = (indent) => ROWS.map(([id, why]) => indent + '  "' + id + '": {\n' + indent + '    "enabled": false,\n' + indent + '    "enabled_why": ' + JSON.stringify(WHY_OFF) + ',\n' + indent + '    "_why": ' + JSON.stringify(why) + '\n' + indent + '  }').join(',\n');
const block = (indent) => indent + '"n73": {\n' + indent + '  "_why": ' + JSON.stringify("NIGHT73 (MOTION, GG 2026-10-27 \"real changes, imporvemnts and itterations\"): this session's rows, all VIEW rows (no sim, no tape bit), pre-placed by Tools/night/n73-rows.cjs. ON in the repo by the lead (D-1); off: -skygrindRows72 <row>:0 (Rows72.cs).") + ',\n' + rowsJson(indent) + '\n' + indent + '},\n';

const out = [];
for (const rel of CFG) {
  const f = path.join(ROOT, rel);
  const s = fs.readFileSync(f, 'utf8');
  if (/"n73": \{/.test(s)) { console.log('n73-rows: ' + rel + ' already holds "n73" (refused: run once)'); process.exit(1); }
  const m = s.match(/^( *)"n72": \{/m);
  if (!m || (s.match(/^ *"n72": \{/gm) || []).length !== 1) { console.log('n73-rows: ' + rel + ': no single "n72": { line'); process.exit(1); }
  const t = s.slice(0, m.index) + block(m[1]).replace(/\n/g, s.includes('\r\n') ? '\r\n' : '\n') + s.slice(m.index);
  const j = JSON.parse(t);
  for (const [id] of ROWS) if (j.design.n73[id].enabled !== false) { console.log('n73-rows: ' + id + ' not false'); process.exit(1); }
  out.push([f, t]);
}
if (out[0][1] !== out[1][1]) { console.log('n73-rows: the two config copies differ (refused: fix that first)'); process.exit(1); }
const frf = path.join(ROOT, FR);
const fr = fs.readFileSync(frf, 'utf8');
const anchor = '"goal72", "cam72" };';
if (fr.split(anchor).length !== 2) { console.log('n73-rows: the FrameRows anchor is not found exactly once in ' + FR); process.exit(1); }
const views = ROWS.filter((r) => r[2]).map((r) => '"' + r[0] + '"');
const fr2 = fr.replace(anchor, '"goal72", "cam72", ' + views.join(', ') + ' };');
if (dry) { console.log('n73-rows --dry: would add design.n73 (' + ROWS.length + ' rows, all OFF) to both copies and ' + views.length + ' FrameRows'); process.exit(0); }
for (const [f, t] of out) fs.writeFileSync(f, t);
fs.writeFileSync(frf, fr2);
console.log('n73-rows: design.n73 (' + ROWS.length + ' rows, all OFF) in both config copies (byte-identical); FrameRows + ' + views.join(' '));
