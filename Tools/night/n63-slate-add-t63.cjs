#!/usr/bin/env node
// NIGHT63 (the lead): adds text63 (builder G's follow-up to P3's critic points, DECIDE63 D-7) to tonight's slate. Its doneWhen is taken from
// G's own Player-steps notes, written BEFORE any Player run of the row (Research/2026-10-22-wave63/G-T63.md lines 42-56, committed 00:59:38
// on n63-T63; the first Player run of text63 began on n63-b6 at ~01:11), plus the night's standing "verbs37 x5 unchanged".
//   node Tools/night/n63-slate-add-t63.cjs
'use strict';
const fs = require('fs');
const file = 'Logs/n63/slate.json';
const slate = JSON.parse(fs.readFileSync(file, 'utf8'));
if (slate.slices.some(x => x.id === 'T63')) { console.log('T63 is already in ' + file); process.exit(1); }
slate.slices.push({
  id: 'T63', owner: 'G', state: 'measuring', passes: false, verdict: null, spot: 'Logs/n63/spot/T63.txt',
  title: 'text63: an instruction is taught once per technique, and it waits for the room title to clear (text61\'s critic points)',
  why: 'GG 2026-10-14 "way too much instruction at the tsrat of the level" (OWNER.md:110); P3 critic problems 3-5 (Logs/n63/verdict-P3.md): the 3 s fallback stacked text 4 times in b3\'s verbs37 runs, kiln taught 10 lines (7 the boost)',
  row: 'design.n63.text63 (new, OFF; -skygrindNoText63)',
  red: 'n63-b3 verbs37: "(LATE: the room\'s title stood 3 s)" 4 times (vi, vk, vs, vp); kiln 10 instruction lines, 7 of them the boost',
  doneWhen: [
    'verbs37 x5 on the build: no "(LATE" line (or one naming 10 s, if a title stands that long); kiln\'s "[text63] withheld:" lines for "Midair, aim down, then ...", "Fire ... down to lift off." and "Boost: ... twice."; the step lines ("Press SPACE and LEFT CLICK together.", "At the top, press LEFT CLICK again.") still shown; verbs37 31 / 31 on kiln and the rest unchanged',
    'the SPOT on the two-run pair t63on vs t63off (a157 n57-kiln -skygrindClearShots60; -skygrindNoText63), frame shot-N29-L57-n57-kiln-kb2-clear.png, names the card withheld (with text61 alone the card reads "Midair, aim down, then LEFT CLICK." under DOOR OPEN; with text63 no card)'
  ],
  visible: 'the kiln stops repeating the same boost lesson in new words; a line waits for the room title instead of stacking under it',
  spotPose: 'a157 n57-kiln kb2 clear (two-run)'
});
fs.writeFileSync(file, JSON.stringify(slate, null, 1) + '\n');
console.log('T63 added to ' + file);
