#!/usr/bin/env node
// NIGHT70 (the lead): appends one slice to the night's slate (Logs/n70/slate.json) from a JSON file holding the entry, so a leftover that
// turns into a row gets its own doneWhen BEFORE its GATE measurement (NIGHT70.md section 2: a doneWhen is written before any measurement and
// never loosened). Refuses a duplicate id or an entry missing one of the fields n70-slate.cjs needs; writes nothing else.
//   node Tools/night/n70-slate-add.cjs <entry.json> [slate path]
'use strict';
const fs = require('fs');
const NEED = ['id', 'title', 'why', 'owner', 'row', 'red', 'doneWhen', 'visible', 'passes', 'state'];
const [entryFile, file = 'Logs/n70/slate.json'] = process.argv.slice(2);
if (!entryFile) { console.log('usage: n70-slate-add.cjs <entry.json> [slate path]'); process.exit(2); }
const entry = JSON.parse(fs.readFileSync(entryFile, 'utf8'));
const miss = NEED.filter(k => !(k in entry));
if (miss.length) { console.log('refused: the entry misses ' + miss.join(', ')); process.exit(1); }
const slate = JSON.parse(fs.readFileSync(file, 'utf8'));
if (!Array.isArray(slate.slices)) { console.log('refused: no "slices" array in ' + file); process.exit(1); }
if (slate.slices.some(s => s.id === entry.id)) { console.log('refused: ' + entry.id + ' is already on the slate'); process.exit(1); }
for (const k of ['verdict', 'spot', 'passSha', 'passFiles']) if (!(k in entry)) entry[k] = null;
slate.slices.push(entry);
fs.writeFileSync(file, JSON.stringify(slate, null, 2) + '\n');
console.log(entry.id + ': added to ' + file + ' (' + slate.slices.length + ' slices)');
