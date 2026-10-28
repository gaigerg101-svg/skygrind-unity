#!/usr/bin/env node
// NIGHT72 (FORGE72 red team 1: the close's RED branch): turns ONE row OFF in the repo's config (both copies, byte-identical), with an
// enabled_why starting "OFF at NIGHT72: <reason>", and asserts nothing else changes. Used when a guard goes RED with a row ON and the row is
// found by re-running the side with `-skygrindRows72 <row>:0`. Its slice goes SHORT with the RED line (n72-slate-set.cjs).
//   node Tools/night/n72-off-row.cjs <night.row>=<the RED it caused, with its file> [--dry] [--root <dir>]
'use strict';
const fs = require('fs');
const path = require('path');
const argv = process.argv.slice(2);
const ROOT = path.resolve(argv.includes('--root') ? argv[argv.indexOf('--root') + 1] : path.join(__dirname, '..', '..'));
const dry = argv.includes('--dry');
const spec = argv.find((a) => /^[a-z0-9]+\.[A-Za-z0-9]+=/.test(a));
if (!spec) { console.log('usage: n72-off-row.cjs <night.row>=<reason> [--dry]'); process.exit(2); }
const key = spec.slice(0, spec.indexOf('=')), reason = spec.slice(spec.indexOf('=') + 1);
if (reason.length < 20) { console.log('n72-off-row: give the measured reason (the RED line and its file)'); process.exit(1); }
const A = path.join(ROOT, 'Design/unity-config.json'), B = path.join(ROOT, 'Assets/StreamingAssets/Design/unity-config.json');
let raw = fs.readFileSync(A, 'utf8');
if (raw !== fs.readFileSync(B, 'utf8')) { console.log('n72-off-row: the two config copies differ (refused)'); process.exit(1); }
const before = JSON.parse(raw);
const [night, row] = key.split('.');
const r0 = before.design[night] && before.design[night][row];
if (!r0 || r0.enabled !== true) { console.log('n72-off-row: design.' + key + ' is not ON now (refused)'); process.exit(1); }
const nightAt = raw.indexOf('\n    "' + night + '": {');
const rowAt = nightAt < 0 ? -1 : raw.indexOf('\n      "' + row + '": {', nightAt);
if (rowAt < 0) { console.log('n72-off-row: ' + key + ': block not found in the text'); process.exit(1); }
const end = raw.indexOf('\n      }', rowAt);
const LABEL = 'OFF at NIGHT72: ';
let block = raw.slice(rowAt, end);
block = block.replace('"enabled": true', '"enabled": false').replace(/"enabled_why": "(?:[^"\\]|\\.)*"/, '"enabled_why": ' + JSON.stringify(LABEL + reason));
const out = raw.slice(0, rowAt) + block + raw.slice(end);
const after = JSON.parse(out);
if (after.design[night][row].enabled !== false) { console.log('n72-off-row: the text edit did not turn it off (refused)'); process.exit(1); }
const chk = JSON.parse(out); chk.design[night][row].enabled = true; chk.design[night][row].enabled_why = r0.enabled_why;
if (JSON.stringify(chk) !== JSON.stringify(before)) { console.log('n72-off-row: something beyond the row changed (refused)'); process.exit(1); }
if (dry) { console.log('DRY: would turn OFF ' + key); process.exit(0); }
fs.writeFileSync(A, out); fs.writeFileSync(B, out);
console.log('OFF in both config copies: ' + key + ' (commit the two copies alone)');
