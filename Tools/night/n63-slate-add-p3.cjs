#!/usr/bin/env node
// NIGHT63 (the lead, leftover 2): adds NIGHT62's P3 (text61) to tonight's slate, its doneWhen copied VERBATIM from Logs/n62/slate.json
// (written before NIGHT62's measurement; never loosened), so the night's GATE judges it and n63-freeze-rows.cjs can flip it by the rule.
// Fields reset for tonight: state measuring, passes false, verdict null; spot = tonight's SPOT file. Refuses if P3 is already there.
//   node Tools/night/n63-slate-add-p3.cjs
'use strict';
const fs = require('fs');
const src = JSON.parse(fs.readFileSync('Logs/n62/slate.json', 'utf8')).slices.find(x => x.id === 'P3');
const file = 'Logs/n63/slate.json';
const slate = JSON.parse(fs.readFileSync(file, 'utf8'));
if (!src) { console.log('no P3 in Logs/n62/slate.json'); process.exit(1); }
if (slate.slices.some(x => x.id === 'P3')) { console.log('P3 is already in ' + file); process.exit(1); }
const p3 = Object.assign({}, src, {
  owner: 'G (pin) / lead',
  state: 'measuring',
  passes: false,
  verdict: null,
  spot: 'Logs/n63/spot/P3.txt',
  title: src.title + ' (NIGHT63 leftover 2: judged on a two-run pair with the hint pinned, -skygrindPinHint63)',
  spotPose: 'a157 n57-kiln kb2c entry (p3on vs p3off, both -skygrindPinHint63)',
});
slate.slices.push(p3);
fs.writeFileSync(file, JSON.stringify(slate, null, 1) + '\n');
console.log('P3 added to ' + file + ' with NIGHT62\'s doneWhen (' + p3.doneWhen.length + ' lines, verbatim)');
