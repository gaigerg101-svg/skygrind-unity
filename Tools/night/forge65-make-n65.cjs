#!/usr/bin/env node
// FORGE65 (2026-10-23, the lead): writes NIGHT65's tools as renamed copies of NIGHT64's (FORGE63 D-8's method), so NIGHT65 starts from tools
// that ran all of NIGHT64. Changes beyond the rename: the clock is a 7-hour box for GG's "next 7 hours 6.5 min 7 hard stop" (the close from
// 6:15 at the latest, CAP 7:00, stale after 10 h so a long usage stop still reads CAP); the gates' fingerprint loop gains --off65. Refuses to
// overwrite an existing n65 file. Prints each file and the lines that still name 64.
//   node Tools/night/forge65-make-n65.cjs
'use strict';
const fs = require('fs');
const path = require('path');
const DIR = path.join(__dirname);
const FILES = ['n64-clock.cjs', 'n64-freeze-rows.cjs', 'n64-gates.sh', 'n64-prune.sh', 'n64-reels.sh', 'n64-slate.cjs', 'n64-spot-pair.cjs',
  'n64-slate-set.cjs', 'n64-slate-add.cjs'];

function common(s) {
  return s
    .replace(/the n64 names/g, 'the n65 names')
    .replace(/n64-/g, 'n65-')
    .replace(/Logs\/n64/g, 'Logs/n65')
    .replace(/'Logs', 'n64'/g, "'Logs', 'n65'")
    .replace(/NIGHT64/g, 'NIGHT65')
    .replace(/flow-off64/g, 'flow-off65')
    .replace(/--off64/g, '--off65');
}

const special = {
  'n64-clock.cjs': s => s
    .replace(/^\/\/ NIGHT65 clock[^\n]*/m, "// NIGHT65 clock (FORGE65, 2026-10-23, for GG's \"next 7 hours 6.5 min 7 hard stop\"; NIGHT64's clock renamed): the run's t0 and its phases, kept in")
    .replace(/prints the date, h:mm \/ 3:00,/g, 'prints the date, h:mm / 7:00,')
    .replace(/BUILD \(0:00-2:15\) anything; FREEZE \(2:15-3:00: the close\)/g, 'BUILD (0:00-6:15) anything; FREEZE (6:15-7:00: the close)')
    .replace(/CAP \(3:00 on\)/g, 'CAP (7:00 on)')
    .replace(/night: 'n64', freezeMin: 135, capMin: 180, staleMin: 600, closeTags: \['n65-freeze'\]/,
      "night: 'n65', freezeMin: 375, capMin: 420, staleMin: 600, closeTags: ['n65-freeze']"),
  'n64-gates.sh': s => s.replace('"--off63" "--off65"; do', '"--off63" "--off64" "--off65"; do'),
};

for (const f of FILES) {
  const src = path.join(DIR, f);
  const dst = path.join(DIR, f.replace('n64-', 'n65-'));
  if (fs.existsSync(dst)) { console.log('exists, not overwritten: ' + dst); process.exitCode = 1; continue; }
  let s = common(fs.readFileSync(src, 'utf8'));
  if (special[f]) s = special[f](s);
  fs.writeFileSync(dst, s);
  if (f.endsWith('.sh')) fs.chmodSync(dst, 0o755);
  const left = s.split('\n').map((l, i) => [i + 1, l]).filter(([, l]) => /64/.test(l));
  console.log(dst + ': ' + s.split('\n').length + ' lines; lines still naming 64: ' + (left.length ? left.map(([n, l]) => n + ' ' + l.trim().slice(0, 100)).join(' || ') : 'none'));
}
