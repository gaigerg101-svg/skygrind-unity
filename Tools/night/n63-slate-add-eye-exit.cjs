#!/usr/bin/env node
// NIGHT63 (the lead): adds two built leftovers to tonight's slate so the report and the close count them: eye63 (builder D, DECIDE63 D-10,
// M1's proposal 2) and exit63 (builder V, the doorway itself turns cyan). Their doneWhen are copied from the builders' own commits written
// BEFORE any Player run: eye63 from Research/2026-10-22-wave63/D-EYE.md (d13257ab, 01:30:35; its first Player run was n63-b8, 02:02);
// exit63 from 038f833d (01:05:06, V-EXIT.md, with V's correction of its SPOT pose, also before any Player run). States from the Player
// evidence already logged in NIGHT63-LOG.md (b8): eye63 blocked (measure MET; a camera ease has no still-frame SPOT, so it waits OFF for
// GG's blind A / B: D-10), exit63 short (OWN GAIN 0 / 5). Refuses an id already present.
//   node Tools/night/n63-slate-add-eye-exit.cjs
'use strict';
const fs = require('fs');
const file = 'Logs/n63/slate.json';
const slate = JSON.parse(fs.readFileSync(file, 'utf8'));
const add = [
  {
    id: 'EYE63', owner: 'D', state: 'blocked', passes: false,
    verdict: 'measure MET on a Player pair (n63-b8: `node Tools/night/n63-m1-chop.cjs --pair n63-b8`: events 64, the eye\'s own residual halved at 64, worse at 0; view only 1.00 mm; Logs/n63/eye63-pair-b8.txt); proof lines on e3f / e3f-off; verbs37 x5 27/29/31/31/31 on and off. BLOCKED: a camera ease has no still-frame SPOT, so the visible rule cannot pass it; it waits OFF for GG\'s blind paired A / B (DECIDE63 D-10, L-001)',
    spot: null,
    title: 'eye63, the eased eye: the slide and landing eye moves eased instead of snapping (M1\'s proposal 2)',
    why: 'GG\'s "choppy movement" (gg-proxy-start #2, 7 asks); M1 (Logs/n63/m1.md): the slide eye is MoveTowards at 7 m/s, 11.67 cm a frame, starting and stopping at full speed in one frame',
    row: 'design.n63.eye63 (new, OFF; -skygrindNoEye63)',
    red: 'M1 on n62-freeze verbs37: 64 slide and landing events with the eye\'s own residual over the 2 cm chop line (Logs/n63/m1-chop-n62-freeze.txt)',
    doneWhen: [
      'n63-m1-chop.cjs --pair on a Player build: the eye\'s own residual halved at every counted slide and landing event (the last line MET)',
      'view only: the feet / x / z within 1.1 mm of the off run',
      'proof lines: "[eye63] eye63 ON" and one "fired" line on the on side; "[eye63] eye63 OFF" and no "fired" line off',
      'verbs37 x5 unchanged',
      'no still-frame SPOT (a camera ease is not a still-frame change): it waits OFF for GG\'s blind paired A / B'
    ],
    visible: 'sliding and landing, the view eases instead of jumping',
    spotPose: 'none (telemetry, not a frame pair)'
  },
  {
    id: 'EXIT63', owner: 'V', state: 'short', passes: false,
    verdict: 'SHORT on doneWhen 1 (n63-b8 e63, same-run, n60-v-lamp.py on e63-on vs e63-off: OWN GAIN 1.05 / 1.02 / 1.01 / 1.00 / 1.03x = 0 / 5 rooms; GUARD 5 / 5; SHOTS 5 / 5 measured; Logs/n63/exit63-owngain-b8.txt); the kb3d exit63 SPOT pose changed 14,648 px (< 41,472); no SPOT and no GATE run (rule 7: the MAIN failed first)',
    spot: null,
    title: 'exit63: the doorway itself turns cyan when a room\'s clear opens it (P1\'s successor, DECIDE63 D-6)',
    why: 'GG 2026-10-13: doors "not visible now and you just run through it and it\'s like dark"; P1 (exitFrame61) SHORT: its own gain reached 1.5x in 1 of 5 rooms and a brighter frame lit the gun',
    row: 'design.n63.exit63 (new, OFF; -skygrindNoExit63)',
    red: 'P1 on n63-b3: exitFrame61\'s own gain 1.07-1.56x, 1.5x in 1 of 5 rooms (Logs/n63/p1-owngain-b3.txt)',
    doneWhen: [
      'OWN GAIN (MAIN): on n57-kiln\'s a157 clear shots, the door\'s luma on / off (n60-v-lamp.py, same-run -skygrindSameRun63 n63.exit63) >= 1.5x in >= 4 of 5 rooms',
      'GUARD: the room\'s p95 luma outside the door box within 0.02 of off in 5 / 5 rooms; no light used',
      'ONE RISE: one "[exit63] door lit: room <id> (door <door>, frame <n>)" line per exit a clear opens on the a157 route, 0 off; the drawn level rises once and holds',
      'SPOT: the same-run pair at the a157 n57-kiln kb3d exit63 shot (shot-N29-L57-n57-kiln-kb3d-exit63.png) changes >= 41,472 px and the SPOT names the door or the way out',
      'verbs37 x5 unchanged (27 / 29 / 31 / 31 / 31)',
      'off exact: with the row off nothing is built or drawn'
    ],
    visible: 'when a room is cleared, its exit doorway glows cyan from inside',
    spotPose: 'a157 n57-kiln kb3d exit63 shot (same-run)'
  }
];
for (const a of add) {
  if (slate.slices.some(x => x.id === a.id)) { console.log(a.id + ' is already in ' + file); process.exit(1); }
  slate.slices.push(a);
}
fs.writeFileSync(file, JSON.stringify(slate, null, 1) + '\n');
console.log('added ' + add.map(a => a.id).join(', ') + ' to ' + file);
