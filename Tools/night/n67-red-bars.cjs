#!/usr/bin/env node
// NIGHT67 (the lead's): each room's bar and guard ceilings from its red (NIGHT67.md section 1 step 6), read from the files, never typed:
//   redRatio   the M1 line of Logs/n67/red-<L>/<id>/skip.txt ("ratio <x> vs bar")
//   mainBar    max(1.25, redRatio + 0.20), to 2 decimals
//   deathsAMax / stallAMax   flow-A.txt's SUMMARY (seeds 1-10); deathsBMax   flow-B.txt's SUMMARY (seeds 11-20)
// Rooms: every slate slice with a `level` (or the ids given). Prints one line per room; with --write sets the fields in Logs/n67/slate.json
// through n67-slate-set.cjs (only the lead runs it, never while a round runs: NIGHT67.md rule 6).
//   node Tools/night/n67-red-bars.cjs [--write] [<id> ...]
'use strict';
const fs = require('fs');
const { execFileSync } = require('child_process');
const args = process.argv.slice(2);
const write = args.includes('--write');
const slate = JSON.parse(fs.readFileSync('Logs/n67/slate.json', 'utf8'));
const ids = args.filter((a) => !a.startsWith('--'));
const rooms = slate.slices.filter((s) => s.level && (!ids.length || ids.includes(s.id)));
const rd = (p) => { try { return fs.readFileSync(p, 'utf8'); } catch (e) { return null; } };
const summary = (txt) => {
  const m = /SUMMARY: (\S+) (\d+)\/(\d+); stall ([\d.]+) %.*?deaths (\d+)/.exec(txt || '');
  return m ? { verdict: m[1], cleared: +m[2], of: +m[3], stall: +m[4], deaths: +m[5] } : null;
};
let bad = 0;
for (const s of rooms) {
  const dir = 'Logs/n67/red-' + s.level + '/' + s.id + '/';
  const A = summary(rd(dir + 'flow-A.txt')), B = summary(rd(dir + 'flow-B.txt'));
  const k = /M1 \w+ \| room \S+, ratio ([\d.]+) vs bar.*?exercised (\d+)\/(\d+)/.exec(rd(dir + 'skip.txt') || '');
  const verb = /verb65 (\w+) \(planted/.exec(rd(dir + 'skip.txt') || '');
  if (!A || !B || !k) { bad++; console.log(s.id + ': MISSING ' + [!A && 'flow-A', !B && 'flow-B', !k && 'skip M1'].filter(Boolean).join(', ') + ' under ' + dir); continue; }
  if (verb && verb[1] !== s.verb) { bad++; console.log(s.id + ': the red skip planted ' + verb[1] + ' but the slate verb is ' + s.verb); continue; }
  const ratio = +k[1];
  const bar = Math.max(1.25, Math.round((ratio + 0.20) * 100) / 100);
  const f = { redRatio: ratio, mainBar: bar, deathsAMax: A.deaths, deathsBMax: B.deaths, stallAMax: A.stall };
  console.log(s.id + ' (' + s.level + ', ' + s.verb + '): red ' + ratio + ' (exercised ' + k[2] + '/' + k[3] + ') -> mainBar ' + bar + '; deaths A ' + A.deaths + ' / B ' + B.deaths + ', stall A ' + A.stall + ' % (A ' + A.verdict + ' ' + A.cleared + '/' + A.of + ', B ' + B.verdict + ' ' + B.cleared + '/' + B.of + ')');
  if (write) execFileSync(process.execPath, ['Tools/night/n67-slate-set.cjs', s.id, ...Object.entries(f).map(([a, b]) => a + '=' + b), 'redSkip=' + dir + 'skip.txt'], { stdio: 'inherit' });
}
console.log('n67-red-bars: ' + (rooms.length - bad) + ' / ' + rooms.length + ' rooms read' + (write ? ', written to Logs/n67/slate.json' : ' (dry: --write sets them)'));
process.exit(bad ? 1 : 0);
