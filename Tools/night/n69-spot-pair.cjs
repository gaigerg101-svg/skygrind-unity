#!/usr/bin/env node
// NIGHT69 (the lead, 2026-10-22): one blind SPOT pair for the visible rule (NIGHT69.md section 2). Copies the on and off frames to
// Logs/n69/spot/<6 random letters>/A.png and B.png in a coin-flip order and prints ONLY the two paths to give the skygrind-spotter.
// The mapping goes to Logs/n69/spot/<id>-<tag>.mapping.txt (read it and log it AFTER the spotter's answer, never before giving the paths).
//   node Tools/night/n69-spot-pair.cjs <id> <on frame> <off frame> [tag]     e.g. node Tools/night/n69-spot-pair.cjs P5 <on png> <off png>
const fs = require('fs');
const path = require('path');
const crypto = require('crypto');
const [id, onF, offF, tag = 'p0'] = process.argv.slice(2);
if (!id || !onF || !offF) { console.log('usage: n69-spot-pair.cjs <id> <on frame> <off frame> [tag]'); process.exit(2); }
for (const f of [onF, offF]) if (!fs.existsSync(f)) { console.log('no such frame: ' + f); process.exit(1); }
const root = path.resolve(__dirname, '..', '..');
const letters = Array.from(crypto.randomBytes(6), b => String.fromCharCode(97 + (b % 26))).join('');
const dir = path.join(root, 'Logs', 'n69', 'spot', letters);
fs.mkdirSync(dir, { recursive: true });
const onIsA = crypto.randomBytes(1)[0] % 2 === 0;
fs.copyFileSync(onIsA ? onF : offF, path.join(dir, 'A.png'));
fs.copyFileSync(onIsA ? offF : onF, path.join(dir, 'B.png'));
fs.writeFileSync(path.join(root, 'Logs', 'n69', 'spot', id + '-' + tag + '.mapping.txt'),
  id + ' ' + tag + ' ' + new Date().toString() + '\ndir ' + letters + '\nA = ' + (onIsA ? 'ON ' + onF : 'OFF ' + offF) +
  '\nB = ' + (onIsA ? 'OFF ' + offF : 'ON ' + onF) + '\n');
console.log(path.join(dir, 'A.png').replace(/\\/g, '/'));
console.log(path.join(dir, 'B.png').replace(/\\/g, '/'));
