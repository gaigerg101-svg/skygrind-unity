#!/usr/bin/env node
// NIGHT74 close (wave-memory harvest 74): rewrites THREADS.md's T-159 row and appends one FEATURES.md row (LF bytes). Run once.
'use strict';
const fs = require('fs');
const path = require('path');
const M = path.join(__dirname, '..', '..', 'docs', 'memory');
const rd = (n) => fs.readFileSync(path.join(M, n), 'utf8').replace(/\r\n/g, '\n');
const wr = (n, s) => fs.writeFileSync(path.join(M, n), s.endsWith('\n') ? s : s + '\n');

let t = rd('THREADS.md').split('\n');
const i = t.findIndex((l) => /^\| T-159 \|/.test(l));
if (i < 0) throw new Error('no T-159 row');
t[i] = "| T-159 | THE NIGHTS' SHORT ITEMS (w71's list in ledger/wave-71.md). w73's closed at w74: kill74 the beat a glance sees, spring74 the squash, the moving-camera flicker meter. Open after w74 (NIGHT74-REPORT.md section 4): crown74 (THE CROWN's dark ceiling: the fill washes the frame), crownroom74 (the room's violet: vertex multipliers cast it olive / green), ice74 (THE ICEBREAKER 17.6 % dark vs 15), flick74 / pads74 (no post source holds a third; pad streams -8.2 %), a4 / b2 not rebuilt, gg-proxy's faults on g8 / a3 / g3 / kb2 / kb3-line4 | w71, w73, w74 | Logs/n74/verdict-*.md, Logs/n74/gg-proxy-close.md | a room judge with GG's words per room before the flip (L-159); a grade-level look for the boss rooms (L-157) | open |";
wr('THREADS.md', t.join('\n'));

let f = rd('FEATURES.md').replace(/\n+$/, '');
f += "\n| NIGHT74'S ROOMS AND ROWS ON (w74: the level rows foundry65 / iceline67 / kiln67 / spire67 with GG's NIGHT71 rooms + 15 rebuilt (a2, a5, a3; g1-g8; kb2c, kb3; v5, v2); weak74 rule W, target74, kill74, spring74, reveal74) | Design/levels/n65, n67 room files; Weak74 / Target74 / Kill74 / EnemyViews.Spring74 / Reveal74.cs | `-skygrindRows72 <row>:0` (a room: its level row) | w74 | ON | not played yet | planted-pose pairs + blind SPOT; verbs37 6/6; Logs/n74/verdict-*.md |";
wr('FEATURES.md', f);
console.log('memory-rows: THREADS ' + t.length + ' lines, FEATURES ' + f.split('\n').length + ' lines');
