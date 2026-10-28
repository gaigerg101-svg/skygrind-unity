#!/usr/bin/env node
// NIGHT74 (FORGE74, n74-tools.cjs): a copy of n73-pair.cjs with the n74 names.
// NIGHT72 (FORGE72 red team 9: pair preparation by hand cost ~1.5 min x ~35 pairs): one same-build pair in one call.
//   node Tools/night/n74-pair.cjs <row> <on.png> <off.png>
// Writes Logs/n74/pairs/<row>/on.png and off.png (copies), Logs/n74/spot/<row>/A.png and B.png (the same two in a random order, for the
// blind spotter, who is given only those two paths), the order in Logs/n74/pairs/<row>/key.txt (never in the spot folder), and a sheet
// with each frame's mean luma and dark share (Research/2026-10-27-forge72/sheet.py) as Logs/n74/pairs/<row>/sheet.jpg + numbers.txt.
// Refuses when either frame is missing or when the two files are byte-identical (a toggle that changed nothing is not a pair).
'use strict';
const fs = require('fs');
const path = require('path');
const crypto = require('crypto');
const { spawnSync } = require('child_process');
const ROOT = path.resolve(__dirname, '..', '..');
const [row, on, off] = process.argv.slice(2);
if (!row || !on || !off) { console.log('usage: n74-pair.cjs <row> <on.png> <off.png>'); process.exit(2); }
for (const f of [on, off]) if (!fs.existsSync(f)) { console.log('n74-pair: missing ' + f); process.exit(1); }
const hash = (f) => crypto.createHash('sha1').update(fs.readFileSync(f)).digest('hex');
if (hash(on) === hash(off)) { console.log('n74-pair: ' + row + ': the ON and OFF frames are byte-identical (the toggle changed nothing)'); process.exit(1); }
const P = path.join(ROOT, 'Logs', 'n74', 'pairs', row), S = path.join(ROOT, 'Logs', 'n74', 'spot', row);
fs.mkdirSync(P, { recursive: true }); fs.mkdirSync(S, { recursive: true });
fs.copyFileSync(on, path.join(P, 'on.png')); fs.copyFileSync(off, path.join(P, 'off.png'));
const onIsA = crypto.randomInt(2) === 0;
fs.copyFileSync(onIsA ? on : off, path.join(S, 'A.png')); fs.copyFileSync(onIsA ? off : on, path.join(S, 'B.png'));
fs.writeFileSync(path.join(P, 'key.txt'), 'A = ' + (onIsA ? 'ON' : 'OFF') + ', B = ' + (onIsA ? 'OFF' : 'ON') + '\non: ' + on + '\noff: ' + off + '\n' + new Date().toISOString() + '\n');
const r = spawnSync('python', ['-I', path.join(ROOT, 'Research', '2026-10-27-forge72', 'sheet.py'), path.join(P, 'sheet.jpg'), path.join(P, 'on.png'), path.join(P, 'off.png')], { encoding: 'utf8' });
fs.writeFileSync(path.join(P, 'numbers.txt'), (r.stdout || '') + (r.stderr || ''));
console.log('n74-pair ' + row + ': Logs/n74/pairs/' + row + '/ (on, off, sheet.jpg, numbers.txt, key.txt); spotter: Logs/n74/spot/' + row + '/A.png and B.png');
process.stdout.write((r.stdout || '').replace(/^/gm, '  '));
