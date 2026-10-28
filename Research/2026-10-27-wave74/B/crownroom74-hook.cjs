#!/usr/bin/env node
// NIGHT74 builder B (crownroom74): the lead's three-line hook in Assets/Scripts/Game/Palette72.cs (a file outside builder B's list, so it is a script, not an edit). Run from the repo root of the tree that
// holds the crownroom74 pick:   node Research/2026-10-27-wave74/B/crownroom74-hook.cjs [--check]
// Idempotent (a second run says "already"); refuses unless each anchor is found exactly once; keeps the file's line endings; --check only reports.
//   1. before the vertex loop (its `int floor = 0, slope = 0, ...` line):     CrownRoom74.Begin(cfg);
//   2. in the loop, right after `Vector3 m = Multiplier(li, ny, y, out cls);`:  if (cls == "wall") m = CrownRoom74.Wall(m, t.TransformPoint(v[i]));
//   3. after the loop, before `held = mesh;`:                                 CrownRoom74.End(key);
'use strict';
const fs = require('fs');
const path = require('path');
const FILE = path.join(process.cwd(), 'Assets', 'Scripts', 'Game', 'Palette72.cs');
const check = process.argv.includes('--check');
let s = fs.readFileSync(FILE, 'utf8');
const nl = s.includes('\r\n') ? '\r\n' : '\n';
if (s.includes('CrownRoom74.')) { console.log('crownroom74-hook: already applied in ' + FILE); process.exit(0); }
const edits = [
  {
    name: 'Begin before the loop',
    anchor: '            int floor = 0, slope = 0, deck = 0, pit = 0, wall = 0, over = 0, structured = 0;',
    insert: '            CrownRoom74.Begin(cfg); // NIGHT74 B crownroom74 (CrownRoom74.cs): THE CROWN room\'s wall multiplier, read once for this stone mesh' + nl,
    before: true,
  },
  {
    name: 'Wall in the loop',
    anchor: '                    Vector3 m = Multiplier(li, ny, y, out cls);',
    insert: '                    if (cls == "wall") m = CrownRoom74.Wall(m, t.TransformPoint(v[i])); // NIGHT74 B crownroom74: inside THE CROWN\'s room the slate\'s own multiplier, not the spire\'s blue boost' + nl,
    before: false,
  },
  {
    name: 'End after the loop',
    anchor: '            held = mesh;' + nl + '            offCols = (Color[])off.Clone();',
    insert: '            CrownRoom74.End(key); // NIGHT74 B crownroom74: the proof line for this stone mesh' + nl,
    before: true,
  },
];
for (const e of edits) {
  const a = e.anchor.split(/\r?\n/).join(nl);
  const n = s.split(a).length - 1;
  if (n !== 1) { console.log('crownroom74-hook: anchor "' + e.name + '" found ' + n + ' times (need 1): refused, nothing written'); process.exit(1); }
}
for (const e of edits) {
  const a = e.anchor.split(/\r?\n/).join(nl);
  s = e.before ? s.replace(a, () => e.insert + a) : s.replace(a, () => a + nl + e.insert.replace(/\r?\n$/, ''));
}
if (!check) fs.writeFileSync(FILE, s);
console.log('crownroom74-hook: ' + (check ? 'would apply (--check) ' : 'applied ') + edits.length + ' lines in ' + FILE);
