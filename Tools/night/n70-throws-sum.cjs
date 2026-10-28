#!/usr/bin/env node
// NIGHT70 builder T, slice Z1 (Logs/n70/slate.json Z1; DECIDE70 D-3; scout.md B): the throw chart's summary and its M3 verdict, ONE copy
// for every reader. A chart is Tools/night/n70-throws.sh's file: one `n65verb launch: GREEN|RED | room R | launch36 N, flight end E thrown T m
// (max M m), bursts B (hits H, kills K) | ... in a pit YES|no, in lava YES|no ...` probe line per seed, then its summary line `n70-throws: ...
// M3 candidates (D-3) N / S`. A probe is an M3 candidate when its line is `: GREEN`, launch36 1 or more, AND (in a pit YES, in lava YES, or a
// burst that killed >= 2 incl. the flier): a RED probe's burst never counts (the forge's rule, moved here unchanged from n70-throws.sh).
// M3 (D-3): PASS at N >= 10 of a 20-seed chart. The verdict re-counts the probe lines; the chart's own summary line must agree (a chart whose
// summary differs from its probes, or a chart of another room, or over other than 20 seeds, FAILs, said). No file: FAIL "no throw chart".
//   node Tools/night/n70-throws-sum.cjs --append <chart>          n70-throws.sh: append the summary line to the chart and print it
//   node Tools/night/n70-throws-sum.cjs --m3 <chart> [--room R]   one line: "M3 PASS|FAIL (why) | <the summary>"; exit 0 PASS, 1 FAIL
//   require('./n70-throws-sum.cjs').m3(text|null, room, where)    the readers (n70-gate-pack.cjs launchM3, n70-next.cjs M3)
'use strict';
const fs = require('fs');
const NEED = 10, SEEDS = 20;   // DECIDE70 D-3 (written before any measurement): PASS at N >= 10 of 20 probe seeds
const PROBE = /^n65verb launch( --[a-z-]+)*: (GREEN|RED)/;
function summarize(text) {
  const L = String(text).split(/\r?\n/).filter((l) => PROBE.test(l));
  let g = 0, pit = 0, lava = 0, b2 = 0, m3 = 0; const th = []; const ends = {}; const rooms = new Set();
  for (const l of L) {
    const green = /: GREEN /.test(l); if (green) g++;
    const rm = l.match(/\| room (\S+) \|/); if (rm) rooms.add(rm[1]);
    const t = l.match(/thrown ([\d.]+) m/); if (t) th.push(+t[1]);
    const e = l.match(/flight end (\S*)/); const ek = e ? (e[1] || '-') : '-'; ends[ek] = (ends[ek] || 0) + 1;
    const k = l.match(/kills (\d+)\)/); const kills = k ? +k[1] : 0;
    const p = /in a pit YES/.test(l), v = /in lava YES/.test(l); if (p) pit++; if (v) lava++; if (kills >= 2) b2++;
    if (green && /\| launch36 [1-9]/.test(l) && (p || v || kills >= 2)) m3++;
  }
  th.sort((a, b) => a - b); const med = th.length ? (th.length % 2 ? th[(th.length - 1) / 2] : (th[th.length / 2 - 1] + th[th.length / 2]) / 2) : NaN;
  const line = `n70-throws: ${L.length} probes | GREEN ${g} | thrown median ${Number.isNaN(med) ? '-' : med.toFixed(1)} m (min ${th[0] ?? '-'}, max ${th[th.length - 1] ?? '-'}) | ends ${Object.entries(ends).map(([k, n]) => k + ' ' + n).join(', ')} | in a pit ${pit}, in lava ${lava}, bursts killing >= 2: ${b2} | M3 candidates (D-3) ${m3} / ${L.length}`;
  return { probes: L.length, green: g, m3, pit, lava, b2, rooms: [...rooms], line };
}
// the M3 verdict of a chart's text (null = no file) for one room; `where` names the file in the reason
function m3(text, room, where) {
  const w = where || 'the chart';
  if (text == null) return { pass: false, n: 0, of: 0, why: 'no throw chart (' + w + ': bash Tools/night/n70-throws.sh <level> ' + (room || '<room>') + ' ' + w + ' 20)', line: '' };
  const s = summarize(text);
  const printed = (String(text).split(/\r?\n/).filter((l) => /^n70-throws: /.test(l)).pop() || '').trim();
  const fail = (why) => ({ pass: false, n: s.m3, of: s.probes, why, line: s.line });
  if (!s.probes) return fail('no probe line in ' + w);
  if (room && (s.rooms.length !== 1 || s.rooms[0] !== room)) return fail('the chart is of room ' + (s.rooms.join(', ') || 'none') + ', not ' + room + ' (' + w + ')');
  if (!printed) return fail('the chart has no summary line (an unfinished n70-throws.sh run: ' + w + ')');
  if (printed !== s.line) return fail('the chart\'s summary line differs from its probe lines (printed "' + printed.slice(-40) + '", counted ' + s.m3 + ' / ' + s.probes + ')');
  if (s.probes !== SEEDS) return fail('a chart over ' + s.probes + ' seeds, not ' + SEEDS + ' (D-3)');
  if (s.m3 < NEED) return fail(s.m3 + ' / ' + s.probes + ' candidates, under ' + NEED);
  return { pass: true, n: s.m3, of: s.probes, why: s.m3 + ' / ' + s.probes + ' candidates, >= ' + NEED, line: s.line };
}
module.exports = { summarize, m3, NEED, SEEDS };
if (require.main === module) {
  const a = process.argv.slice(2);
  const opt = (k) => { const i = a.indexOf(k); return i >= 0 && i + 1 < a.length ? a[i + 1] : null; };
  if (opt('--append')) {
    const f = opt('--append'); const s = summarize(fs.readFileSync(f, 'utf8'));
    fs.appendFileSync(f, s.line + '\n'); console.log(s.line);
  } else if (opt('--m3')) {
    const f = opt('--m3'); const text = fs.existsSync(f) ? fs.readFileSync(f, 'utf8') : null;
    const v = m3(text, opt('--room'), f);
    console.log('M3 ' + (v.pass ? 'PASS' : 'FAIL') + ' (' + v.why + ')' + (v.line ? ' | ' + v.line : ''));
    process.exit(v.pass ? 0 : 1);
  } else { console.log('usage: n70-throws-sum.cjs --append <chart> | --m3 <chart> [--room R]'); process.exit(2); }
}
