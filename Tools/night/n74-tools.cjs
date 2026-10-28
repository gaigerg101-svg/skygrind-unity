#!/usr/bin/env node
// FORGE74 (2026-10-27, GG: "45 mins to build 8 hour night goal prompt"): writes NIGHT74's night tools as copies of NIGHT72 / NIGHT73's,
// with the n74 names, so every command NIGHT74.md names exists before the night and was run in the forge. Run once; refuses to overwrite.
//   node Tools/night/n74-tools.cjs [--force]
// Writes:
//   n74-clock.cjs      (n73-clock.cjs; FREEZE 7:15, CAP 8:00, close tags n74-freeze / n74-play)
//   n74-reels.sh       (n73-reels.sh; n74-* tags only; --keep kept)
//   n74-prune.sh       (n73-prune.sh; n74-* tags only)
//   n74-pair.cjs       (n73-pair.cjs; Logs/n74/pairs and Logs/n74/spot)
//   n74-slate-set.cjs  (n73-slate-set.cjs; Logs/n74/slate.json)
//   n74-caps.cjs       (n72-caps.cjs; n74-* builds / reels / smokes, Logs/n74/spot; caps builds 12, sides 260, smokes 12, spotter 60)
//   n74-row-add.cjs    (n73-row-add.cjs; adds one OFF row to design.n74, which n74-rows.cjs creates)
//   n74-rows.cjs       (n73-rows.cjs; design.n74 before the "n73" block, its rows from Research/2026-10-27-forge74/rows74.json)
//   n74-slate.cjs, n74-slate-add.cjs (n72-slate.cjs / n72-slate-add.cjs; default slate Logs/n74/slate.json: the close check runs
//                      Tools/night/<night>-slate.cjs with no argument, FORGE74 red team 1)
// Unchanged tools the night calls with an n74 path: n72-freeze-rows.cjs --slate=Logs/n74/slate.json,
// n72-close-check.cjs --night n74 (its windows follow Logs/clock.json's capMin), n72-gates.sh, n41-build.sh, n73-strip.py, n73-hue.py.
'use strict';
const fs = require('fs');
const path = require('path');
const D = __dirname;
const force = process.argv.includes('--force');
const rd = (f) => fs.readFileSync(path.join(D, f), 'utf8');
const out = [];
const put = (f, s) => {
  const p = path.join(D, f);
  if (fs.existsSync(p) && !force) { console.log('n74-tools: ' + f + ' exists (refused; --force to rewrite)'); process.exitCode = 1; return; }
  fs.writeFileSync(p, s); out.push(f);
};
const swap = (s, from, to) => { if (!s.includes(from)) throw new Error('anchor not found: ' + from); return s.split(from).join(to); };
const head = (s, line) => s.replace(/^(#!.*\n)?/, (m) => m + line + '\n');

// the clock
let c = rd('n73-clock.cjs');
c = swap(c, "const DEF = { night: 'n73', freezeMin: 180, capMin: 210, staleMin: 720, closeTags: ['n73-freeze'] };",
  "const DEF = { night: 'n74', freezeMin: 435, capMin: 480, staleMin: 720, closeTags: ['n74-freeze', 'n74-play'] };");
c = c.split('n72-clock.cjs').join('n74-clock.cjs');
put('n74-clock.cjs', head(c, '// NIGHT74 (FORGE74, n74-tools.cjs): a copy of n73-clock.cjs with FREEZE at 7:15 and CAP at 8:00 (an 8-hour night).'));

// reels and prune
for (const f of ['n73-reels.sh', 'n73-prune.sh']) {
  let s = rd(f).split('n73').join('n74');
  put(f.replace('n73', 'n74'), head(s, '# NIGHT74 (FORGE74, n74-tools.cjs): a copy of ' + f + ' with the n74 names.'));
}

// pair, slate-set, row-add
for (const f of ['n73-pair.cjs', 'n73-slate-set.cjs', 'n73-row-add.cjs']) {
  let s = rd(f).split('n73').join('n74');
  put(f.replace('n73', 'n74'), head(s, '// NIGHT74 (FORGE74, n74-tools.cjs): a copy of ' + f + ' with the n74 names.'));
}

// slate print / slate add (FORGE74 red team 1: n72-close-check.cjs --night n74 runs Tools/night/n74-slate.cjs with no argument)
for (const f of ['n72-slate.cjs', 'n72-slate-add.cjs']) {
  let s = rd(f).split('Logs/n72/slate.json').join('Logs/n74/slate.json');
  if (!s.includes('Logs/n74/slate.json')) throw new Error('n74-tools: no Logs/n72/slate.json default in ' + f);
  put(f.replace('n72', 'n74'), head(s, '// NIGHT74 (FORGE74, n74-tools.cjs): a copy of ' + f + ' whose default slate is Logs/n74/slate.json.'));
}

// caps
let k = rd('n72-caps.cjs').split('n72').join('n74');
k = swap(k, 'const CAPS = { builds: 10, sides: 220, smokes: 10, spotter: 50 };', 'const CAPS = { builds: 12, sides: 260, smokes: 12, spotter: 60 };');
put('n74-caps.cjs', head(k, '// NIGHT74 (FORGE74, n74-tools.cjs): a copy of n72-caps.cjs with the n74 names and NIGHT74.md section 5\'s caps.'));

// rows: design.n74 placed before the "n73" block, rows read from the forge's rows74.json ([[id, why, frameRow(bool)], ...])
let r = rd('n73-rows.cjs');
r = r.replace(/const ROWS = \[\r?\n[\s\S]*?\r?\n\];\r?\n/, "const SPEC = JSON.parse(fs.readFileSync(path.join(ROOT, 'Research/2026-10-27-forge74/rows74.json'), 'utf8'));\nconst ROWS = SPEC.rows;\n");
if (!/rows74\.json/.test(r)) throw new Error('n74-tools: the ROWS block of n73-rows.cjs was not found');
r = r.replace(/JSON\.stringify\("NIGHT73 \(MOTION[^\n]*?Rows72\.cs\)\."\)/, 'JSON.stringify(SPEC.why)');
if (!/JSON\.stringify\(SPEC\.why\)/.test(r)) throw new Error('n74-tools: the block _why of n73-rows.cjs was not found');
// first the night's own names (n73 -> n74), then the anchor block (n72 -> n73: design.n74 goes right before "n73")
r = r.split('n73').join('n74').split('n72').join('n73');
r = swap(r, "'\"goal72\", \"cam72\" };'", "'\"core73\" };'");
r = swap(r, "'\"goal72\", \"cam72\", '", "'\"core73\", '");
put('n74-rows.cjs', head(r, '// NIGHT74 (FORGE74, n74-tools.cjs): a copy of n73-rows.cjs: design.n74 before the "n73" block, rows from rows74.json.'));

console.log('n74-tools: wrote ' + out.join(', '));
