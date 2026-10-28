// NIGHT70 builder T, slice Z3: the byte-identity check of the planted pair (read-only; prints only).
//   node Research/2026-10-25-wave70/T/z3-compare.cjs <RED file> <GREEN file> <NOCOMMIT file>
// (1) RED (the t0 build) vs GREEN (the Z3 build, the committed run on): the three runs' lines must be byte-identical: the header
//     `n65verb skip v2:` and the first `  runs:` line, every `  seed N verb-aware:` (the plain verb-aware run), `  seed N refused-in:`,
//     `  seed N walker:`, `  seed N touches` line, and the `n65verb skip touch:` / `n65verb skip launchkill:` tables (read from the plain run).
//     Lines that may differ, by design: `  seed N => ratio` (the verb side), the M1 / STYLE+15 / M2 / totals lines; new: `  seed N committed:`
//     and the second `  runs:` line.
// (2) RED vs NOCOMMIT (the Z3 build with --no-commit): every line byte-identical but the `# ` headers, the exit line's clock and the totals'
//     wall time.
// (3) The committed lines: fire held steps, closed steps, gun shots in a held step (must be 0), launch36 per run beside the plain run's.
'use strict';
const fs = require('fs');
const [red, green, noc] = process.argv.slice(2);
const L = (f) => fs.readFileSync(f, 'utf8').replace(/\r\n/g, '\n').split('\n');
const R = L(red), G = L(green), N = noc ? L(noc) : null;
const kinds = [
  ['header', (l) => /^n65verb skip v2: level /.test(l)],
  ['runs (the three)', (l) => /^  runs: verb-aware = /.test(l)],
  ['seed verb-aware', (l) => /^  seed \d+ verb-aware: /.test(l)],
  ['seed refused-in', (l) => /^  seed \d+ refused-in: /.test(l)],
  ['seed walker', (l) => /^  seed \d+ walker: /.test(l)],
  ['seed touches', (l) => /^  seed \d+ touches /.test(l)],
  ['touch table', (l) => /^n65verb skip touch: /.test(l)],
  ['launchkill', (l) => /^n65verb skip launchkill: /.test(l)],
];
let bad = 0;
console.log(`z3-compare: ${red} (RED) vs ${green} (GREEN)`);
for (const [k, f] of kinds) {
  const a = R.filter(f), b = G.filter(f);
  const same = a.length === b.length && a.every((x, i) => x === b[i]);
  if (!same) bad++;
  console.log(`  ${k.padEnd(18)} RED ${String(a.length).padStart(3)} lines, GREEN ${String(b.length).padStart(3)}: ${same ? 'IDENTICAL' : 'DIFFERENT'}`);
  if (!same) for (let i = 0; i < Math.max(a.length, b.length); i++) if (a[i] !== b[i]) { console.log('    RED:   ' + (a[i] || '(none)').slice(0, 300)); console.log('    GREEN: ' + (b[i] || '(none)').slice(0, 300)); break; }
}
const newG = G.filter((l) => /^  seed \d+ committed: /.test(l));
console.log(`  new in GREEN: ${newG.length} committed seed lines, ${G.filter((l) => /^  runs: committed/.test(l)).length} committed runs line`);
for (const l of newG) {
  const s = (/^  seed (\d+)/.exec(l) || [])[1];
  const plain = G.find((x) => new RegExp('^  seed ' + s + ' verb-aware: ').test(x)) || '';
  const lc = (x) => ((/\| launch36 (\d+)/.exec(x) || [])[1] || '-');
  const ck = (x) => ((/cleared (yes in [\d.]+ s|NO[^|]*)/.exec(x) || [])[1] || '-');
  console.log(`    seed ${s}: verb-aware launch36 ${lc(plain)}, ${ck(plain)} | committed launch36 ${lc(l)}, ${ck(l)} | ${(/fire held .*$/.exec(l) || ['-'])[0]}`);
  if (!/gun shots in a held step 0(\D|$)/.test(l)) bad++;
}
const m2 = (A) => (A.find((l) => /^n65verb skip: M2 VERB/.test(l)) || '').slice(0, 900);
console.log('  RED   M2: ' + m2(R)); console.log('  GREEN M2: ' + m2(G));
if (N) {
  const norm = (A) => A.filter((l) => !/^# /.test(l)).map((l) => l.replace(/ \| wall time \d+ s$/, ' | wall time <t> s').replace(/^exit (\d+) at .*/, 'exit $1'));
  const a = norm(R), b = norm(N);
  const diffs = []; for (let i = 0; i < Math.max(a.length, b.length); i++) if (a[i] !== b[i]) diffs.push(i);
  if (diffs.length) bad++;
  console.log(`z3-compare: ${red} (RED) vs ${noc} (NOCOMMIT, --no-commit): ${a.length} / ${b.length} lines, ${diffs.length ? diffs.length + ' DIFFERENT (first: RED "' + (a[diffs[0]] || '').slice(0, 200) + '" / NOCOMMIT "' + (b[diffs[0]] || '').slice(0, 200) + '")' : 'IDENTICAL'} (the # headers, the exit clock and the totals wall time aside)`);
}
console.log(`z3-compare: ${bad === 0 ? 'PASS' : 'FAIL (' + bad + ')'}`);
process.exit(bad === 0 ? 0 : 1);
