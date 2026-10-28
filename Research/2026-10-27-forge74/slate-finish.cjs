#!/usr/bin/env node
// NIGHT74 (the lead, before the freeze): adds the last slices (a3, g8, kb3, pads74) and binds every room / level-row PASS to the t9 build
// (b1275711b), where the final level files passed verbs37 on all six close sides and the smoke. Run once.
'use strict';
const fs = require('fs');
const path = require('path');
const f = path.join(__dirname, '..', '..', 'Logs', 'n74', 'slate.json');
const s = JSON.parse(fs.readFileSync(f, 'utf8'));
const T9 = 'b1275711b';
const LV = {
  foundry: ['Design/levels/n65-foundry.json', 'Assets/StreamingAssets/Design/levels/n65-foundry.json', 'Design/levels/n65/slots65.json'],
  iceline: ['Design/levels/n67-iceline.json', 'Assets/StreamingAssets/Design/levels/n67-iceline.json', 'Design/levels/n67/iceline/slots67.json'],
  kiln: ['Design/levels/n67-kiln.json', 'Assets/StreamingAssets/Design/levels/n67-kiln.json', 'Design/levels/n67/kiln/slots67.json'],
  spire: ['Design/levels/n67-spire.json', 'Assets/StreamingAssets/Design/levels/n67-spire.json', 'Design/levels/n67/spire/slots67.json'],
};
const ROOM = { a2: 'foundry', a5: 'foundry', a3: 'foundry', 'LV-foundry65': 'foundry', g1: 'iceline', g2: 'iceline', g3: 'iceline', g4: 'iceline',
  g5: 'iceline', g6: 'iceline', g7: 'iceline', g8: 'iceline', 'LV-iceline67': 'iceline', kb2c: 'kiln', kb3: 'kiln', 'LV-kiln67': 'kiln',
  v2: 'spire', v5: 'spire', 'LV-spire67': 'spire' };
const room = (id, owner, lv, row) => ({ state: 'pass', passes: true, verdict: 'Logs/n74/verdict-' + id + '.md', spot: 'Logs/n74/spot/' + id + '/spot.md',
  passSha: T9, passFiles: [], id, track: 'rooms', owner, est: 90, title: owner + ' ' + id + ' rebuilt (late room, ' + lv + ')', why: 'NIGHT74 section 7 leftover 1',
  row, on: row.replace('design.', ''), red: 'tour65 planted pose on the shipped room', visible: 'planted-pose pair (D-3)', doneWhen: ["as a2's doneWhen, for " + id] });
const add = [room('a3', 'R', 'n65-foundry', 'design.n65.foundry65'), room('g8', 'S', 'n67-iceline', 'design.n67.iceline67'), room('kb3', 'S', 'n67-kiln', 'design.n67.kiln67'),
  { state: 'short', passes: false, verdict: 'Logs/n74/verdict-pads74.md', spot: null, passSha: null, passFiles: [], id: 'pads74', track: 'light', owner: 'L', est: 60,
    title: 'L pads74: the pad stream steadied and the beam softened', why: 'moving-flicker heat maps; gg-proxy-start #5', row: 'design.n74.pads74', on: 'n74.pads74',
    red: 'pan74 pd0', visible: 'pan74 numbers', doneWhen: ["the pad regions' flagged blocks down >= 50 %, the pooled share not worse"] }];
for (const e of add) if (!s.slices.some((x) => x.id === e.id)) s.slices.push(e);
for (const x of s.slices) if (ROOM[x.id] && x.passes) { const room = x.id.startsWith('LV-') ? [] : (x.id[0] === 'a' ? ['Design/levels/n65/rooms/' + x.id + '.json'] : []);
  x.passSha = T9; x.passFiles = LV[ROOM[x.id]].concat(room); }
fs.writeFileSync(f, JSON.stringify(s, null, 1) + '\n');
console.log('slate-finish: ' + s.slices.length + ' slices; room / level passes bound to ' + T9);
