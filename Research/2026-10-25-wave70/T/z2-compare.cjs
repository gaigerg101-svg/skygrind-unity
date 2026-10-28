// NIGHT70 builder T, slice Z2: the print-only proof (read-only; prints only).
//   node Research/2026-10-25-wave70/T/z2-compare.cjs <BEFORE file> <AFTER file>
// BEFORE = skip v2 on the build before Z2 (140b0241d); AFTER = the Z2 build, the same command. Every line must be identical (the # headers,
// the exit clock and the totals' wall time aside) except: (a) the new `n65verb skip launchend:` lines; (b) a seed line whose ONLY difference
// is its `falls N` field (Z2 counts fallReturn43), and then AFTER's N >= BEFORE's. Also prints the chart against each seed's launch36 count.
'use strict';
const fs = require('fs');
const [bf, af] = process.argv.slice(2);
const norm = (f) => fs.readFileSync(f, 'utf8').replace(/\r\n/g, '\n').split('\n').filter((l) => !/^# /.test(l)).map((l) => l.replace(/ \| wall time \d+ s$/, ' | wall time <t> s').replace(/^exit (\d+) at .*/, 'exit $1'));
const B = norm(bf), A0 = norm(af);
const ends = A0.filter((l) => /^n65verb skip launchend: /.test(l));
const A = A0.filter((l) => !/^n65verb skip launchend: /.test(l));
let bad = 0, fallLines = 0, same = 0;
if (B.length !== A.length) { bad++; console.log(`line counts differ (the launchend lines aside): BEFORE ${B.length}, AFTER ${A.length}`); }
for (let i = 0; i < Math.min(B.length, A.length); i++) {
  if (B[i] === A[i]) { same++; continue; }
  const fb = /falls (\d+)/.exec(B[i]), fa = /falls (\d+)/.exec(A[i]);
  if (fb && fa && B[i].replace(/falls \d+/, 'falls <n>') === A[i].replace(/falls \d+/, 'falls <n>') && +fa[1] >= +fb[1]) { fallLines++; console.log(`  falls only: line ${i + 1}: falls ${fb[1]} -> ${fa[1]}: ${A[i].slice(0, 60)}`); continue; }
  bad++; console.log(`  DIFFERENT line ${i + 1}:\n    BEFORE: ${B[i].slice(0, 260)}\n    AFTER:  ${A[i].slice(0, 260)}`);
}
console.log(`z2-compare: ${bf} vs ${af}: ${same} lines identical, ${fallLines} differ only by their falls count, ${bad} other differences; ${ends.length} new launchend lines`);
// the chart against the plain verb-aware run's launch36 per seed
const seeds = {};
for (const l of A) { const m = /^  seed (\d+) verb-aware: .*\| launch36 (\d+) \|/.exec(l); if (m) seeds[m[1]] = { l36: +m[2], ends: 0 }; }
const kinds = {};
for (const l of ends) { const m = /\| seed (\d+) \|.*\| end (\S+)/.exec(l); if (m) { if (seeds[m[1]]) seeds[m[1]].ends++; kinds[m[2]] = (kinds[m[2]] || 0) + 1; } }
for (const [s, v] of Object.entries(seeds)) console.log(`  seed ${s}: verb-aware launch36 ${v.l36}, launchend lines ${v.ends}${v.l36 === v.ends ? '' : ' (DIFFERENT: a launch counted in the room but charted outside it, or the reverse)'}`);
console.log(`  launch ends: ${Object.entries(kinds).map(([k, n]) => k + ' ' + n).join(', ') || 'none'}; in pit ${ends.filter((l) => /in pit yes/.test(l)).length}, in lava ${ends.filter((l) => /in lava yes/.test(l)).length}`);
console.log(`z2-compare: ${bad === 0 ? 'PASS (print only: every existing line identical but the falls counts)' : 'FAIL (' + bad + ')'}`);
process.exit(bad === 0 ? 0 : 1);
