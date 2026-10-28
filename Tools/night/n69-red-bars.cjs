#!/usr/bin/env node
// NIGHT69 (the lead's): each room's bar and guard ceilings from its red (NIGHT69.md section 1 step 6), read from the files, never typed:
//   redRatio   the M2 VERB line of Logs/n69/red-<L>/<id>/skip.txt ("pooled ratio <x> vs mainBar", skip v2 on seeds 1-20; NIGHT69 I4)
//   mainBar    max(1.25, redRatio + 0.20), to 2 decimals
//   deathsAMax / stallAMax   flow-A.txt's SUMMARY (seeds 1-10); deathsBMax   flow-B.txt's SUMMARY (seeds 11-20)
// Rooms: every slate slice with a `level` (or the ids given). Prints one line per room; with --write sets the fields in Logs/n69/slate.json
// through n69-slate-set.cjs (only the lead runs it, never while a round runs: NIGHT69.md rule 6).
//   node Tools/night/n69-red-bars.cjs [--write] [--slate <file>] [--logs <dir>] [<id> ...]
//   NIGHT69 I4: --slate (default Logs/n69/slate.json) and --logs (default Logs/n69: the reds under <logs>/red-<L>/<id>/) for a dry run in a
//   builder's tree; --write refuses either (it writes the night's slate only, from the night's reds)
'use strict';
const fs = require('fs');
const { execFileSync } = require('child_process');
const args = process.argv.slice(2);
const write = args.includes('--write');
const optv = (k, d) => { const i = args.indexOf(k); return i >= 0 && i + 1 < args.length ? args[i + 1] : d; };
const SLATE = optv('--slate', 'Logs/n69/slate.json'), LOGS = optv('--logs', 'Logs/n69');
if (write && (SLATE !== 'Logs/n69/slate.json' || LOGS !== 'Logs/n69')) { console.log('n69-red-bars: REFUSED: --write takes neither --slate nor --logs'); process.exit(2); }
const slate = JSON.parse(fs.readFileSync(SLATE, 'utf8'));
const ids = args.filter((a, i) => !a.startsWith('--') && !['--slate', '--logs'].includes(args[i - 1]));
const rooms = slate.slices.filter((s) => s.level && (!ids.length || ids.includes(s.id)));
const rd = (p) => { try { return fs.readFileSync(p, 'utf8'); } catch (e) { return null; } };
const summary = (txt) => {
  const m = /SUMMARY: (\S+) (\d+)\/(\d+); stall ([\d.]+) %.*?deaths (\d+)/.exec(txt || '');
  return m ? { verdict: m[1], cleared: +m[2], of: +m[3], stall: +m[4], deaths: +m[5] } : null;
};
let bad = 0;
for (const s of rooms) {
  const dir = LOGS + '/red-' + s.level + '/' + s.id + '/';
  const A = summary(rd(dir + 'flow-A.txt')), B = summary(rd(dir + 'flow-B.txt'));
  // NIGHT69 I4: the POOLED ratio of the M2 VERB line (skip v2 on seeds 1-20: the median of the exercised seeds' ratios over both sets); a red
  // skip without that line (NIGHT67's 10-seed M1 only) or not on seeds 1-20 is not read (MISSING: re-run it on the I4 harness)
  const sk = rd(dir + 'skip.txt') || '';
  const k = /M2 VERB \w+ \| room \S+, verb \S+, pooled ratio ([\d.]+|-) vs mainBar.*?exercised (\d+) \/ (\d+)/.exec(sk);
  const seeds120 = /seeds 1\.\.20 \(--seeds 20\)/.test(sk);
  const verb = /verb65 (\S+) \(planted/.exec(sk);
  if (!A || !B || !k || !seeds120) { bad++; console.log(s.id + ': MISSING ' + [!A && 'flow-A', !B && 'flow-B', !k && 'skip M2 VERB pooled ratio', k && !seeds120 && 'skip on seeds 1..20'].filter(Boolean).join(', ') + ' under ' + dir); continue; }
  if (verb && verb[1] !== s.verb) { bad++; console.log(s.id + ': the red skip planted ' + verb[1] + ' but the slate verb is ' + s.verb); continue; }
  // NIGHT69 I4 fix (verdict-I4.md P4): a red exercised on fewer than 70 % of its seeds (14 / 20) does not test the verb: its ratio is not
  // used; the bar sits at the floor 1.25, said on the line and in the slate (redNote)
  const ex = +k[2], of = +k[3], needEx = Math.ceil(0.7 * of);
  const ratio = k[1] === '-' ? null : +k[1];
  const under = ex < needEx || ratio === null;
  const bar = under ? 1.25 : Math.max(1.25, Math.round((ratio + 0.20) * 100) / 100);
  const note = under ? 'bar at the floor 1.25, red exercised ' + ex + '/' + of : null;
  const f = { redRatio: ratio, mainBar: bar, deathsAMax: A.deaths, deathsBMax: B.deaths, stallAMax: A.stall };
  if (note) f.redNote = note;
  console.log(s.id + ' (' + s.level + ', ' + s.verb + '): red ' + (ratio === null ? '-' : ratio) + ' (exercised ' + ex + '/' + of + ') -> mainBar ' + bar + (note ? ' (' + note + ': under ' + needEx + ' exercised, the ratio not used)' : '') + '; deaths A ' + A.deaths + ' / B ' + B.deaths + ', stall A ' + A.stall + ' % (A ' + A.verdict + ' ' + A.cleared + '/' + A.of + ', B ' + B.verdict + ' ' + B.cleared + '/' + B.of + ')');
  if (write) execFileSync(process.execPath, ['Tools/night/n69-slate-set.cjs', s.id, ...Object.entries(f).map(([a, b]) => a + '=' + b), 'redSkip=' + dir + 'skip.txt'], { stdio: 'inherit' });
}
console.log('n69-red-bars: ' + (rooms.length - bad) + ' / ' + rooms.length + ' rooms read' + (write ? ', written to Logs/n69/slate.json' : ' (dry: --write sets them)'));
process.exit(bad ? 1 : 0);
