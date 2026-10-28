#!/usr/bin/env node
// NIGHT63 builder G (leftover 2: text61, NIGHT62 P3, judged on a two-run pair with the hint pinned): the meter for text61's doneWhen 1-2 on a157's
// entry frames. It reads a Player log of an a157 reel run with -skygrindPinHint63 (UiPinHint63.cs prints "[pin63] text: blocks <n> | title
// "..." | card "..." | clear "..."" on each change of the HUD's centre text) and, at each "[reel] a157: <room> "<card>" entry ..." line, takes the
// state in effect: the centre text blocks on that entry frame (the top slot's title, the teach card's instruction, the ROOM CLEAR card).
// Prints one line per entry and a summary: entries with two or more blocks k of n (doneWhen 1: <= 1 of 5 with text61 on), the ROOM CLEAR card on
// m of n (doneWhen 2: 0 of 5), the instruction card on c of n. No "[pin63]" line at all: the pin did not fire, said RED (the run is not a
// pinned run and its frames are not a fair pair).
//   node Tools/night/n63-p3-blocks.cjs <Player.log> [<Player.log> ...]
//   node Tools/night/n63-p3-blocks.cjs --selftest      the planted case (a log with known states; exits 1 on a wrong count)
'use strict';
const fs = require('fs');

function measure(text) {
  const lines = text.split(/\r?\n/);
  let state = { blocks: 0, title: '', card: '', clear: '' };
  let pinned = 0, rooms = 0;
  const entries = [];
  const reText = /\[pin63\] text: blocks (\d+) \| title "(.*)" \| card "(.*)" \| clear "(.*)"$/;
  const reEntry = /\[reel\] a157: (\S+) "([^"]*)" entry /;
  for (const l of lines) {
    if (l.includes('[pin63] hint pinned:')) pinned++;
    if (l.includes('[pin63] room ')) rooms++;
    const t = reText.exec(l);
    if (t) { state = { blocks: +t[1], title: t[2], card: t[3], clear: t[4] }; continue; }
    const e = reEntry.exec(l);
    if (e) entries.push({ room: e[1], name: e[2], ...state });
  }
  const two = entries.filter(x => x.blocks >= 2).length;
  const clear = entries.filter(x => x.clear.length > 0).length;
  const card = entries.filter(x => x.card.length > 0).length;
  return { pinned, rooms, entries, two, clear, card };
}

function report(file, m) {
  const out = [];
  out.push(`== ${file}`);
  if (m.pinned === 0) out.push('RED: no "[pin63] hint pinned" line: the pin did not fire (boot the reel with -skygrindPinHint63 and -skygrindReelOnly)');
  for (const x of m.entries) out.push(`${x.room} "${x.name}" entry: blocks ${x.blocks} | title "${x.title}" | card "${x.card}" | clear "${x.clear}"`);
  out.push(`entries ${m.entries.length}: two or more blocks ${m.two} of ${m.entries.length}; ROOM CLEAR ${m.clear} of ${m.entries.length}; the instruction card ${m.card} of ${m.entries.length} | pin: pinned lines ${m.pinned}, room entries ${m.rooms}`);
  return out.join('\n');
}

function selftest() {
  // the planted case: five entries; kb2 title + card (2), kb2c title + card + clear (3), kb3 title only (1), kb3c card only (1), kb3d nothing (0)
  const log = [
    '[pin63] hint pinned: 1 line(s) "X" -> "Midair, aim down, then LEFT CLICK." | the sim\'s lesson and teach lines are not drawn this reel (map n57-kiln)',
    '[pin63] text: blocks 0 | title "" | card "" | clear ""',
    '[pin63] room kb2 entered: "Midair" after 1 empty frame(s) | sim lines hidden so far 0 frames, fed 0 frames',
    '[pin63] text: blocks 2 | title "THE CHARGING FLOOR" | card "Midair, aim down, then LEFT CLICK." | clear ""',
    '[reel] a157: kb2 "THE CHARGING FLOOR" entry shot-N29-L57-n57-kiln-kb2-entry.png | wave1 7',
    '[pin63] text: blocks 3 | title "THE CLINKER BRIDGES" | card "Midair, aim down, then LEFT CLICK." | clear "ROOM CLEAR · UNDER PAR"',
    '[reel] a157: kb2c "THE CLINKER BRIDGES" entry shot-N29-L57-n57-kiln-kb2c-entry.png | wave1 5',
    '[pin63] text: blocks 1 | title "THE PRESS HALL" | card "" | clear ""',
    '[reel] a157: kb3 "THE PRESS HALL" entry shot-N29-L57-n57-kiln-kb3-entry.png | wave1 7',
    '[pin63] text: blocks 1 | title "" | card "Midair, aim down, then LEFT CLICK." | clear ""',
    '[reel] a157: kb3c "THE TAP RUN" entry shot-N29-L57-n57-kiln-kb3c-entry.png | wave1 6',
    '[pin63] text: blocks 0 | title "" | card "" | clear ""',
    '[reel] a157: kb3d "THE CRACKED FLOOR" entry shot-N29-L57-n57-kiln-kb3d-entry.png | wave1 5',
    '[reel] a157: kb3d clear shot-N29-L57-n57-kiln-kb3d-clear.png | room cleared yes',
  ].join('\n');
  const m = measure(log);
  const want = { entries: 5, two: 2, clear: 1, card: 3, pinned: 1, rooms: 1 };
  const got = { entries: m.entries.length, two: m.two, clear: m.clear, card: m.card, pinned: m.pinned, rooms: m.rooms };
  const blocks = m.entries.map(x => x.blocks).join(',');
  const ok = JSON.stringify(want) === JSON.stringify(got) && blocks === '2,3,1,1,0';
  const none = measure('[reel] a157: kb2 "A" entry x.png');
  const okNone = none.pinned === 0 && report('planted-unpinned', none).includes('RED: no "[pin63] hint pinned"');
  console.log(report('planted', m));
  console.log(`selftest: ${ok && okNone ? 'PASS' : 'FAIL'} (want ${JSON.stringify(want)} blocks 2,3,1,1,0; got ${JSON.stringify(got)} blocks ${blocks}; an unpinned log says RED: ${okNone})`);
  process.exit(ok && okNone ? 0 : 1);
}

const args = process.argv.slice(2);
if (args[0] === '--selftest') selftest();
else if (args.length === 0) { console.log('usage: node Tools/night/n63-p3-blocks.cjs <Player.log> [...] | --selftest'); process.exit(2); }
else for (const f of args) console.log(report(f, measure(fs.readFileSync(f, 'utf8'))));
