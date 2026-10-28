#!/usr/bin/env node
// NIGHT74 (the lead): which collision boxes / pads differ between two level files inside one room's rect, so a pair's planted pose can face
// the rebuilt part (LV-spire67's entry pose read NOTHING OBVIOUS). Read-only.
//   node Tools/night/n74-roomdiff.cjs <levelA.json> <levelB.json> <room>
'use strict';
const fs = require('fs');
const [a, b, room] = process.argv.slice(2);
const A = JSON.parse(fs.readFileSync(a, 'utf8')), B = JSON.parse(fs.readFileSync(b, 'utf8'));
const r = A.campaign.rooms.find((x) => x.id === room);
const [x0, z0, x1, z1] = r.rect;
const inRect = (bx) => { const mn = bx.min || (bx.c ? bx.c : null), mx = bx.max || mn; if (!mn) return false;
  const cx = (mn[0] + mx[0]) / 2, cz = (mn[2] + mx[2]) / 2; return cx >= x0 && cx <= x1 && cz >= z0 && cz <= z1; };
const key = (bx) => JSON.stringify([bx.min, bx.max]);
const boxes = (L) => ((L.collision && L.collision.boxes) || (L.data && L.data.boxes) || []).filter(inRect);
const ka = new Set(boxes(A).map(key)), kb = new Set(boxes(B).map(key));
const onlyA = boxes(A).filter((x) => !kb.has(key(x))), onlyB = boxes(B).filter((x) => !ka.has(key(x)));
console.log(room + ' rect ' + JSON.stringify(r.rect) + ': boxes A ' + ka.size + ', B ' + kb.size + '; only in A ' + onlyA.length + ', only in B ' + onlyB.length);
for (const x of onlyA.slice(0, 12)) console.log('  A+ ' + key(x));
for (const x of onlyB.slice(0, 12)) console.log('  B+ ' + key(x));
