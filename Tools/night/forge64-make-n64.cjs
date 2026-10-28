#!/usr/bin/env node
// FORGE64 (2026-10-23, the lead): writes NIGHT64's tools as renamed copies of NIGHT63's (FORGE63 D-8's method), so NIGHT64 starts from
// tools that ran all of NIGHT63. Changes beyond the rename: the clock is a 3-hour box (the close from 2:15, CAP 3:00, stale after 5 h); the
// gates' fingerprint loop gains --off64; the pruner accepts capitalised side names (NIGHT63: sides b63fS / b63kA were never pruned and
// Logs/feel/n63-b4 grew to 11 GB, NIGHT63-LOG.md "Disk"). Refuses to overwrite an existing n64 file. Prints each file and its leftovers.
//   node Tools/night/forge64-make-n64.cjs
'use strict';
const fs = require('fs');
const path = require('path');
const DIR = path.join(__dirname);
const FILES = ['n63-clock.cjs', 'n63-freeze-rows.cjs', 'n63-gates.sh', 'n63-prune.sh', 'n63-reels.sh', 'n63-slate.cjs', 'n63-spot-pair.cjs',
  'n63-slate-set.cjs'];

function common(s) {
  return s
    .replace(/the n63 names/g, 'the n64 names')
    .replace(/n63-/g, 'n64-')
    .replace(/Logs\/n63/g, 'Logs/n64')
    .replace(/'Logs', 'n63'/g, "'Logs', 'n64'")
    .replace(/NIGHT63/g, 'NIGHT64')
    .replace(/flow-off63/g, 'flow-off64')
    .replace(/--off63/g, '--off64');
}

const special = {
  'n63-clock.cjs': s => s
    .replace("// NIGHT64 clock (the forge FORGE62b, 2026-10-22, for GG's \"8 hours 7 minimum\")",
      "// NIGHT64 clock (FORGE64, 2026-10-23, for GG's \"next prompt that will go for 3 hours\"; NIGHT63's clock renamed)")
    .replace("//   node Tools/night/n64-clock.cjs          prints the date, h:mm / 8:00,", "//   node Tools/night/n64-clock.cjs          prints the date, h:mm / 3:00,")
    .replace('// Phases: BUILD (0:00-7:00) anything; FREEZE (7:00-8:00: the close)', '// Phases: BUILD (0:00-2:15) anything; FREEZE (2:15-3:00: the close)')
    .replace('// no new agent, no test run; CAP (8:00 on) no heavy step and no agent starts. The guard ignores a clock older than 6 hours (stale).',
      '// no new agent, no test run; CAP (3:00 on) no heavy step and no agent starts. The guard ignores a clock older than 5 hours (stale).')
    .replace("const DEF = { night: 'n63', freezeMin: 420, capMin: 480, staleMin: 600, closeTags: ['n64-freeze'] };",
      "const DEF = { night: 'n64', freezeMin: 135, capMin: 180, staleMin: 300, closeTags: ['n64-freeze'] };"),
  'n63-gates.sh': s => s.replace('"--off62" "--off64"; do', '"--off62" "--off63" "--off64"; do'),
  'n63-prune.sh': s => s
    .replace('The side pattern takes hyphenated names (L-113).', 'The side pattern takes hyphenated names (L-113) and capitals (FORGE64: NIGHT63-LOG.md "Disk").')
    .replace('grep -E "^== [a-z0-9-]+ [0-9]"', 'grep -E "^== [A-Za-z0-9-]+ [0-9]"'),
};

for (const f of FILES) {
  const src = path.join(DIR, f);
  const dst = path.join(DIR, f.replace('n63-', 'n64-'));
  if (fs.existsSync(dst)) { console.log('exists, not overwritten: ' + dst); process.exitCode = 1; continue; }
  let s = common(fs.readFileSync(src, 'utf8'));
  if (special[f]) s = special[f](s);
  fs.writeFileSync(dst, s);
  if (f.endsWith('.sh')) fs.chmodSync(dst, 0o755);
  const left = s.split('\n').map((l, i) => [i + 1, l]).filter(([, l]) => /63/.test(l));
  console.log(dst + ': ' + s.split('\n').length + ' lines; lines still naming 63: ' + (left.length ? left.map(([n, l]) => n + ' ' + l.trim().slice(0, 110)).join(' || ') : 'none'));
}
