#!/usr/bin/env node
// NIGHT74 (the lead): appends the slices started after the seed (leftover rooms and rows) to Logs/n74/slate.json, todo, with doneWhen written
// before their verdicts. Refuses an id already present. Run once.
'use strict';
const fs = require('fs');
const path = require('path');
const f = path.join(__dirname, '..', '..', 'Logs', 'n74', 'slate.json');
const s = JSON.parse(fs.readFileSync(f, 'utf8'));
const room = (id, owner, level, row, why) => ({ state: 'todo', passes: false, verdict: null, spot: null, passSha: null, passFiles: [], id, track: 'rooms', owner, est: 90,
  title: owner + ' ' + id + ' rebuilt (leftover room, ' + level + ')', why, row, on: row.replace('design.', ''), red: 'tour65 planted pose on the shipped room', visible: 'planted-pose pair (D-3)',
  doneWhen: ["as a2's doneWhen, for " + id + ' on ' + level] });
const rowS = (id, owner, why, dw) => ({ state: 'todo', passes: false, verdict: null, spot: null, passSha: null, passFiles: [], id, track: 'leftover', owner, est: 60,
  title: owner + ' ' + id, why, row: 'design.n74.' + id, on: 'n74.' + id, red: 'same-build pair', visible: 'same-build pair', doneWhen: dw });
const add = [
  room('g1', 'S', 'n67-iceline', 'design.n67.iceline67', 'gg-proxy-start #1: g1 a bare floor'),
  room('g6', 'S', 'n67-iceline', 'design.n67.iceline67', 'gg-proxy-start #1: g6 THE ARMOURY an empty floor'),
  room('g2', 'S', 'n67-iceline', 'design.n67.iceline67', 'NIGHT74 section 7 leftover 1'),
  room('g7', 'S', 'n67-iceline', 'design.n67.iceline67', 'NIGHT74 section 7 leftover 1'),
  room('g3', 'R', 'n67-iceline', 'design.n67.iceline67', 'NIGHT74 section 7 leftover 1'),
  room('g5', 'R', 'n67-iceline', 'design.n67.iceline67', 'NIGHT74 section 7 leftover 1'),
  room('a4', 'B', 'n65-foundry', 'design.n65.foundry65', 'NIGHT74 section 7 leftover 1 (B after its list)'),
  rowS('spring74', 'B', 'leftover 3: enemy hit squash a glance sees (T-159)', ['ON height <= 85 % of OFF at hit+1 and hit+2, back within 8 % by hit+12 (n74-squash.py)', 'the blind SPOT names it']),
  rowS('crownroom74', 'B', "leftover 2: THE CROWN's room hues to one palette", ['violet+magenta down >= 60 %, red-orange not up > 2 pt, no more hue families than OFF (n74-palette.py)', 'the blind SPOT names it and the lead sees no new cast']),
  rowS('ice74', 'L', "leftover 4: THE ICEBREAKER's dark arena, a local fill", ['dark (< 30) <= 15 % on every frame, idle down >= 5 pt, bright +3 or less, danger +2 or less (n74-ice.py)', 'the blind SPOT names it']),
];
for (const e of add) { if (s.slices.some((x) => x.id === e.id)) { console.log('slate-add-late: ' + e.id + ' already present (skipped)'); continue; } s.slices.push(e); }
fs.writeFileSync(f, JSON.stringify(s, null, 1) + '\n');
console.log('slate-add-late: ' + s.slices.length + ' slices');
