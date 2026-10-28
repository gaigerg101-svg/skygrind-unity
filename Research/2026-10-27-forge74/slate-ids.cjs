#!/usr/bin/env node
// FORGE74: renames the room slices of slate-seed.json to their room ids (the room tools look a slice up by the room's id: n70-red.sh's
// SLATE=, n65-slot-set.cjs), keeping the builder letter and slice number in the title. Run once in the forge.
'use strict';
const fs = require('fs');
const path = require('path');
const f = path.join(__dirname, 'slate-seed.json');
const s = JSON.parse(fs.readFileSync(f, 'utf8'));
const MAP = { R1: 'a2', R2: 'a5', R3: 'b2', S1: 'kb2c', S2: 'v5', S3: 'v2' };
for (const x of s.slices) if (MAP[x.id]) { x.title = x.id + ' ' + x.title; x.id = MAP[x.id]; }
for (const x of s.slices) x.doneWhen = x.doneWhen.map((d) => d.replace("as R1's doneWhen", "as a2's doneWhen"));
fs.writeFileSync(f, JSON.stringify(s, null, 1) + '\n');
console.log('slate-ids: ' + s.slices.map((x) => x.id).join(' '));
