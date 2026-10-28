#!/usr/bin/env node
// NIGHT73: adds ONE more view row, OFF, to the existing design.n73 block in both config copies (byte-identical after) and to FrameRows, for a
// slice that starts after the shared commit (n73-rows.cjs runs once). The block is inserted as text right after the `"n73": {` line's _why, so
// the files keep their format; JSON-parsed after to check the row is there and false.
//   node Tools/night/n73-row-add.cjs <row> "<slice>: <what>"
'use strict';
const fs = require('fs');
const path = require('path');
const ROOT = path.resolve(__dirname, '..', '..');
const CFG = ['Design/unity-config.json', 'Assets/StreamingAssets/Design/unity-config.json'];
const FR = 'Assets/Scripts/Game/SameRun63Math.N63D.cs';
const [row, why] = process.argv.slice(2);
if (!row || !why || !/^[a-z]+73$/.test(row)) { console.log('usage: n73-row-add.cjs <row ending in 73> "<slice>: <what>"'); process.exit(2); }
const WHY_OFF = "OFF until the lead's ON flip (NIGHT72 D-1, kept by NIGHT73: its same-build pair, a SPOT naming the change, the guards)";
const out = [];
for (const rel of CFG) {
  const s = fs.readFileSync(path.join(ROOT, rel), 'utf8');
  const nl = s.includes('\r\n') ? '\r\n' : '\n';
  const m = s.match(/^( *)"n73": \{\r?\n( *)"_why": .*,\r?\n/m);
  if (!m) { console.log('n73-row-add: ' + rel + ': no "n73": { block with a _why line'); process.exit(1); }
  if (JSON.parse(s).design.n73[row]) { console.log('n73-row-add: ' + row + ' already in ' + rel); process.exit(1); }
  const ind = m[2];
  const block = ind + '"' + row + '": {' + nl + ind + '  "enabled": false,' + nl + ind + '  "enabled_why": ' + JSON.stringify(WHY_OFF) + ',' + nl + ind + '  "_why": ' + JSON.stringify(why) + nl + ind + '},' + nl;
  const at = m.index + m[0].length;
  const t = s.slice(0, at) + block + s.slice(at);
  if (JSON.parse(t).design.n73[row].enabled !== false) { console.log('n73-row-add: not false after'); process.exit(1); }
  out.push([path.join(ROOT, rel), t]);
}
if (out[0][1] !== out[1][1]) { console.log('n73-row-add: the copies differ (refused)'); process.exit(1); }
const frf = path.join(ROOT, FR);
const fr = fs.readFileSync(frf, 'utf8');
const anchor = '"kiln73" };';
if (fr.split(anchor).length !== 2) { console.log('n73-row-add: FrameRows anchor not found once'); process.exit(1); }
for (const [f, t] of out) fs.writeFileSync(f, t);
fs.writeFileSync(frf, fr.replace(anchor, '"kiln73", "' + row + '" };'));
console.log('n73-row-add: design.n73.' + row + ' OFF in both copies (byte-identical); FrameRows + ' + row);
