#!/usr/bin/env node
// FORGE74: applies the red team's slate / rows fixes (redteam.md 3, 11) to slate-seed.json and rows74.json. Run once in the forge.
'use strict';
const fs = require('fs');
const path = require('path');
const D = __dirname;
const swap = (s, a, b) => { if (!s.includes(a)) throw new Error('not found: ' + a); return s.split(a).join(b); };

let s = fs.readFileSync(path.join(D, 'slate-seed.json'), 'utf8');
s = swap(s, 'both copies byte-identical; slots65.json b1 and a8 new via n65-slot-set.cjs',
  'both copies byte-identical, sha1 logged; slots65.json untouched (every slot old: list mode, spike.md S1); every generated companion committed');
s = swap(s, 'boss63 on THE CROWN (n52-spire) and the WICK / LOOM frames of bosses-d on n74-h0',
  'boss63 on THE CROWN (n52-spire) on n74-h0; wick37 on n37-foundry and loom37 on n37-span in the first train B asks for them');
fs.writeFileSync(path.join(D, 'slate-seed.json'), s);

let r = fs.readFileSync(path.join(D, 'rows74.json'), 'utf8');
r = swap(r, '["reveal74", "R4 (', '["reveal74", "L2 (');
r = swap(r, '["crown74", "L3 (', '["crown74", "L3 (');
r = swap(r, '["flick74", "L2 (', '["flick74", "L4 (');
r = swap(r, 'Meter: n74-hue.py --cyan share outside the weak point box.', 'Meter: the cyan share outside the open weak point\'s screen box (B\'s meter).');
fs.writeFileSync(path.join(D, 'rows74.json'), r);
JSON.parse(s); JSON.parse(r);
console.log('redteam-fix: slate-seed.json and rows74.json updated, both parse');
