#!/usr/bin/env node
// NIGHT74 (FORGE74, n74-tools.cjs): a copy of n73-rows.cjs: design.n74 before the "n73" block, rows from rows74.json.
// NIGHT73's shared commit (a copy of n73-rows.cjs with the n74 names): pre-places NIGHT73's rows OFF so builders never add the block.
//   - `design.n74` in BOTH config copies, inserted as text just before the `"n73": {` block, byte-identical after; each row
//     {"enabled": false, "enabled_why": "...", "_why": "<slice>: <what>"};
//   - the view rows appended to SameRun63Math.FrameRows, so -skygrindSameRun63 pairs work.
// View rows only. Rows72.cs already reads design.n<NN>.<row> by the row's digits (`-skygrindRows72 kiln74:1`); NightRows.ViewOn too.
//   node Tools/night/n74-rows.cjs [--dry]
'use strict';
const fs = require('fs');
const path = require('path');
const argv = process.argv.slice(2);
const ROOT = path.resolve(path.join(__dirname, '..', '..'));
const dry = argv.includes('--dry');
const CFG = ['Design/unity-config.json', 'Assets/StreamingAssets/Design/unity-config.json'];
const FR = 'Assets/Scripts/Game/SameRun63Math.N63D.cs';

const SPEC = JSON.parse(fs.readFileSync(path.join(ROOT, 'Research/2026-10-27-forge74/rows74.json'), 'utf8'));
const ROWS = SPEC.rows;

const WHY_OFF = "OFF until the lead's ON flip (NIGHT72 D-1, kept by NIGHT73: its same-build pair, a SPOT naming the change, the guards)";
const rowsJson = (indent) => ROWS.map(([id, why]) => indent + '  "' + id + '": {\n' + indent + '    "enabled": false,\n' + indent + '    "enabled_why": ' + JSON.stringify(WHY_OFF) + ',\n' + indent + '    "_why": ' + JSON.stringify(why) + '\n' + indent + '  }').join(',\n');
const block = (indent) => indent + '"n74": {\n' + indent + '  "_why": ' + JSON.stringify(SPEC.why) + ',\n' + rowsJson(indent) + '\n' + indent + '},\n';

const out = [];
for (const rel of CFG) {
  const f = path.join(ROOT, rel);
  const s = fs.readFileSync(f, 'utf8');
  if (/"n74": \{/.test(s)) { console.log('n74-rows: ' + rel + ' already holds "n74" (refused: run once)'); process.exit(1); }
  const m = s.match(/^( *)"n73": \{/m);
  if (!m || (s.match(/^ *"n73": \{/gm) || []).length !== 1) { console.log('n74-rows: ' + rel + ': no single "n73": { line'); process.exit(1); }
  const t = s.slice(0, m.index) + block(m[1]).replace(/\n/g, s.includes('\r\n') ? '\r\n' : '\n') + s.slice(m.index);
  const j = JSON.parse(t);
  for (const [id] of ROWS) if (j.design.n74[id].enabled !== false) { console.log('n74-rows: ' + id + ' not false'); process.exit(1); }
  out.push([f, t]);
}
if (out[0][1] !== out[1][1]) { console.log('n74-rows: the two config copies differ (refused: fix that first)'); process.exit(1); }
const frf = path.join(ROOT, FR);
const fr = fs.readFileSync(frf, 'utf8');
const anchor = '"core73" };';
if (fr.split(anchor).length !== 2) { console.log('n74-rows: the FrameRows anchor is not found exactly once in ' + FR); process.exit(1); }
const views = ROWS.filter((r) => r[2]).map((r) => '"' + r[0] + '"');
const fr2 = fr.replace(anchor, '"core73", ' + views.join(', ') + ' };');
if (dry) { console.log('n74-rows --dry: would add design.n74 (' + ROWS.length + ' rows, all OFF) to both copies and ' + views.length + ' FrameRows'); process.exit(0); }
for (const [f, t] of out) fs.writeFileSync(f, t);
fs.writeFileSync(frf, fr2);
console.log('n74-rows: design.n74 (' + ROWS.length + ' rows, all OFF) in both config copies (byte-identical); FrameRows + ' + views.join(' '));
