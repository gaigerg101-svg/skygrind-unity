#!/usr/bin/env node
// NIGHT70 builder T, slice Y2: the GATE pack's M3 reader (launchM3 in Tools/night/n70-gate-pack.cjs, this tree's or a commit's copy) on a SAVED
// launch run, for Tools/night/n70-m3-cases.sh. The function's own text is taken from the pack file (from `function launchM3(<params>) {` to
// its closing brace at the same indent) and run on the room's last `n65verb launch: GREEN|RED` line of the saved file. Prints one line:
//   n70-m3-pack-case: <room> PASS|FAIL <the reader's M3 text>
//   node Tools/night/n70-m3-pack-case.cjs <gate-pack.cjs> <saved launch output> <room> [<the round's throw chart> | none]
// NIGHT70 Z1: the pack's launchM3 now reads the room's throw chart (`launchM3(l, chart, T, where)`: the chart's text or null, the shared
// reader Tools/night/n70-throws-sum.cjs, the chart's name); an older pack's `launchM3(l)` ignores the extra arguments (the t0 RED).
'use strict';
const fs = require('fs');
const path = require('path');
const [packFile, savedFile, room, chartFile] = process.argv.slice(2);
if (!packFile || !savedFile || !room) { console.log('n70-m3-pack-case: usage: <gate-pack.cjs> <saved launch output> <room> [<chart> | none]'); process.exit(2); }
const src = fs.readFileSync(packFile, 'utf8').replace(/\r\n/g, '\n');
const m = /\n( *)function launchM3\(([^)]*)\) \{\n([\s\S]*?)\n\1\}\n/.exec(src);
if (!m) { console.log('n70-m3-pack-case: ' + room + ' ERROR no launchM3 in ' + packFile); process.exit(2); }
const params = m[2].split(',').map((s) => s.trim()).filter(Boolean);
const launchM3 = new Function(...params, m[3]);
const lines = fs.readFileSync(savedFile, 'utf8').replace(/\r\n/g, '\n').split('\n');
const vl = lines.filter((l) => /^n65verb launch( --[a-z-]+)*: (GREEN|RED)/.test(l) && new RegExp('\\| room ' + room + ' \\|').test(l)).pop();
if (!vl) { console.log('n70-m3-pack-case: ' + room + ' ERROR no launch verdict line in ' + savedFile); process.exit(2); }
const chart = chartFile && chartFile !== 'none' && fs.existsSync(chartFile) ? fs.readFileSync(chartFile, 'utf8') : null;
const out = launchM3(vl, chart, require(path.join(__dirname, 'n70-throws-sum.cjs')), chartFile && chartFile !== 'none' ? chartFile : 'no chart');
const v = /M3 \([^)]*\)[^:]*: (PASS|FAIL)/.exec(out) || /: (PASS|FAIL)/.exec(out);
console.log('n70-m3-pack-case: ' + room + ' ' + (v ? v[1] : '?') + ' ' + out.trim());
process.exit(v ? 0 : 2);
