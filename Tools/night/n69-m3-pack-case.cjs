#!/usr/bin/env node
// NIGHT69 builder T, slice Y2: the GATE pack's M3 reader (launchM3 in Tools/night/n69-gate-pack.cjs, this tree's or a commit's copy) on a SAVED
// launch run, for Tools/night/n69-m3-cases.sh. The function's own text is taken from the pack file (from `function launchM3(l) {` to its closing
// brace at the same indent) and run on the room's last `n65verb launch: GREEN|RED` line of the saved file. Prints one line:
//   n69-m3-pack-case: <room> PASS|FAIL <the reader's M3 text>
//   node Tools/night/n69-m3-pack-case.cjs <gate-pack.cjs> <saved launch output> <room>
'use strict';
const fs = require('fs');
const [packFile, savedFile, room] = process.argv.slice(2);
if (!packFile || !savedFile || !room) { console.log('n69-m3-pack-case: usage: <gate-pack.cjs> <saved launch output> <room>'); process.exit(2); }
const src = fs.readFileSync(packFile, 'utf8').replace(/\r\n/g, '\n');
const m = /\n( *)function launchM3\(l\) \{\n([\s\S]*?)\n\1\}\n/.exec(src);
if (!m) { console.log('n69-m3-pack-case: ' + room + ' ERROR no launchM3 in ' + packFile); process.exit(2); }
const launchM3 = new Function('l', m[2]);
const lines = fs.readFileSync(savedFile, 'utf8').replace(/\r\n/g, '\n').split('\n');
const vl = lines.filter((l) => /^n65verb launch( --[a-z-]+)*: (GREEN|RED)/.test(l) && new RegExp('\\| room ' + room + ' \\|').test(l)).pop();
if (!vl) { console.log('n69-m3-pack-case: ' + room + ' ERROR no launch verdict line in ' + savedFile); process.exit(2); }
const out = launchM3(vl);
const v = /M3 \([^)]*\)[^:]*: (PASS|FAIL)/.exec(out) || /: (PASS|FAIL)/.exec(out);
console.log('n69-m3-pack-case: ' + room + ' ' + (v ? v[1] : '?') + ' ' + out.trim());
process.exit(v ? 0 : 2);
