#!/usr/bin/env node
// NIGHT66 (the lead): sets fields of one slate entry in Logs/n66/slate.json (only the lead edits the slate; NIGHT66.md section 2). A
// doneWhen is never touched by this tool (written before any measurement, never loosened). Values are JSON when they parse, else strings.
//   node Tools/night/n66-slate-set.cjs <id> <field>=<value> [<field>=<value> ...]
//   e.g. node Tools/night/n66-slate-set.cjs M1 state=pass passes=true verdict=Logs/n66/verdict-M1.md
'use strict';
const fs = require('fs');
const file = 'Logs/n66/slate.json';
const [id, ...pairs] = process.argv.slice(2);
if (!id || !pairs.length) { console.log('usage: n66-slate-set.cjs <id> <field>=<value> ...'); process.exit(2); }
const slate = JSON.parse(fs.readFileSync(file, 'utf8'));
const s = slate.slices.find(x => x.id === id);
if (!s) { console.log('no slate entry ' + id); process.exit(1); }
for (const p of pairs) {
  const k = p.slice(0, p.indexOf('='));
  const raw = p.slice(p.indexOf('=') + 1);
  if (!k || k === 'doneWhen' || k === 'id') { console.log('refused: ' + (k || p) + ' (doneWhen and id are never edited here)'); process.exit(1); }
  let v;
  // FORGE64 red team 16: a sha field stays a string (an all-digit short sha would parse as a number; "0e123456" as 0)
  if (/Sha$/.test(k)) v = raw; else { try { v = JSON.parse(raw); } catch (e) { v = raw; } }
  s[k] = v;
}
fs.writeFileSync(file, JSON.stringify(slate, null, 1) + '\n');
console.log(id + ': ' + pairs.map(p => p.slice(0, p.indexOf('='))).join(', ') + ' set (' + file + ')');
