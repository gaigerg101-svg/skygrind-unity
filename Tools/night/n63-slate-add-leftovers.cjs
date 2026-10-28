#!/usr/bin/env node
// NIGHT63 (the lead): adds leftovers 6 (S7, builder G) and 7 (C2, builder V) to tonight's slate so the GATE judges them and
// n63-freeze-rows.cjs can flip them by the rule. Their doneWhen are taken from the builders' own Player-steps notes, written BEFORE any Player
// measurement (Research/2026-10-22-wave63/V.md "Leftover 7" committed 00:11:41 on n63-C2; G-S7.md committed 00:23:26 on n63-S7; the first
// Player run of either row began on n63-b5 at ~00:55), plus the night's standing "verbs37 x5 unchanged". Refuses an id already present.
//   node Tools/night/n63-slate-add-leftovers.cjs
'use strict';
const fs = require('fs');
const file = 'Logs/n63/slate.json';
const slate = JSON.parse(fs.readFileSync(file, 'utf8'));
const add = [
  {
    id: 'C2', owner: 'V', state: 'measuring', passes: false, verdict: null, spot: null,
    title: 'The enemy glow up close: a live enemy part dissolves as the eye comes within 1.0 m (and THE ICEBREAKER\'s body stops covering the camera) (leftover 7)',
    why: 'gg-proxy-start.md #9 (enemies read as glow domes and flat beans: an enemy\'s own body filling the screen at the eye, the x-ray pass drawing the one behind); the boss63 reels: THE ICEBREAKER\'s rigid body covers the camera at 1.7 m',
    row: 'design.n63.nearFade63 (new, OFF; -skygrindNoNearFade63)',
    red: 'NIGHT62 freeze verbs37: an enemy at the eye in 136 / 98 / 0 / 52 / 62 of 1800 frames (vf / vi / vk / vs / vp), vf/f0400 77.3 % enemy colour (n63-v-near.py); boss63 b63i stagger / flinch off frames nearly black',
    doneWhen: [
      'the near63 reel: the SPOT pose shot-N30-N63-near-0.5.png (a husk 0.5 m from the eye) changes on vs off; the far control N63-near-3.0 about 0 changed',
      'the near63 verdict GREEN on, OFF off; the [near63] lines print each placement\'s dissolve',
      'THE ICEBREAKER\'s boss63 stagger frame (B63-brawl40-stagger) shows the room and the boss\'s legs / head / arms instead of a dark screen with the row on',
      'the same-run SPOT pair names the near enemy\'s change; >= 41,472 px on it',
      'verbs37 x5 unchanged'
    ],
    visible: 'an enemy right in your face turns see-through instead of filling the screen, and the Icebreaker no longer blacks out your view up close',
    spotPose: 'near63 shot-N30-N63-near-0.5.png (same-run)'
  },
  {
    id: 'S7', owner: 'G', state: 'measuring', passes: false, verdict: null, spot: null,
    title: 'Kiln\'s presses read as machines (leftover 6)',
    why: 'gg-proxy #1 (empty boxy rooms; kiln\'s presses still plain blocks: vk f0100 / f0400 / f1000 / f1650); NIGHT62-REPORT section 1 item 1',
    row: 'design.n63.press63 (new, OFF; -skygrindNoPress63)',
    red: 'the kb3 entry frame: three presses in view as plain dark boxes with the hall\'s bands (n62-p0 t61off a157 kb3-entry; vk/f1000)',
    doneWhen: [
      'the SPOT pose a157 n57-kiln shot-N29-L57-n57-kiln-kb3-entry.png on vs off (same-run): the dark pillars become presses (rails, a chrome piston strip, a ram head, a striped plinth); >= 41,472 px changed',
      'proof: one "[press63] presses on n57-kiln: 4 dressed" line on the on side, none off',
      'colliders unchanged: n63-gates.sh --flow EQUAL (n46flow n57-kiln byte-equal) and verbs37 n57-kiln 31 / 31 on the build',
      'verbs37 x5 unchanged'
    ],
    visible: 'walking into THE PRESS HALL you see stamping machines, not blank pillars',
    spotPose: 'a157 n57-kiln kb3 entry (same-run)'
  }
];
for (const a of add) {
  if (slate.slices.some(x => x.id === a.id)) { console.log(a.id + ' is already in ' + file); process.exit(1); }
  slate.slices.push(a);
}
fs.writeFileSync(file, JSON.stringify(slate, null, 1) + '\n');
console.log('added ' + add.map(a => a.id).join(', ') + ' to ' + file);
