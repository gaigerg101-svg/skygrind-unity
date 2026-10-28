// NIGHT70 builder T, Z3 critic P3: the STYLE5 v2 skip against GREEN4, line by line (read-only; prints only). Every line but the # headers, the
// exit clock and the wall time must be identical, and the M2 VERB line may differ ONLY by the inserted " (plain STYLE x %: ...)" text.
//   node Research/2026-10-25-wave70/T/z3-style5-check.cjs <GREEN4 file> <STYLE5 file>
'use strict';
const fs = require('fs');
const [g4, s5] = process.argv.slice(2);
const norm = (f) => fs.readFileSync(f, 'utf8').replace(/\r\n/g, '\n').split('\n').filter((l) => !/^# /.test(l)).map((l) => l.replace(/ \| wall time \d+ s$/, ' | wall time <t> s').replace(/^exit (\d+) at .*/, 'exit $1'));
const a = norm(g4), b = norm(s5);
const INS = / \(plain STYLE (?:[\d.]+|-) %: the verb-aware run alone, over its \d+ exercised seeds\)/;
let bad = 0, m2 = '';
if (a.length !== b.length) { bad++; console.log('line counts differ: ' + a.length + ' / ' + b.length); }
for (let i = 0; i < Math.min(a.length, b.length); i++) {
  if (a[i] === b[i]) continue;
  const isM2 = /^n65verb skip: M2 VERB /.test(b[i]);
  const m = INS.exec(b[i]);
  if (isM2 && m && b[i].replace(INS, '') === a[i]) { m2 = m[0]; console.log('line ' + (i + 1) + ' (the M2 VERB line): differs ONLY by the inserted text "' + m[0].trim() + '"'); }
  else { bad++; console.log('line ' + (i + 1) + ' DIFFERS: GREEN4 "' + a[i].slice(0, 160) + '" / STYLE5 "' + b[i].slice(0, 160) + '"'); }
}
console.log(`z3-style5-check: ${a.length} / ${b.length} lines; ${bad === 0 && m2 ? 'PASS: every other line identical (the # headers, the exit clock and the wall time aside)' : 'FAIL (' + bad + ')'}`);
process.exit(bad === 0 && m2 ? 0 : 1);
