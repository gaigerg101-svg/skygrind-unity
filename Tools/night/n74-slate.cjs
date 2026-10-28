#!/usr/bin/env node
// NIGHT74 (FORGE74, n74-tools.cjs): a copy of n72-slate.cjs whose default slate is Logs/n74/slate.json.
// NIGHT72 (the forge FORGE62, 2026-10-22): prints the night's slate (Logs/n74/slate.json) so the transcript shows it, and checks its shape.
// Read-only. One line per slice (id, state, passes, owner, title), then the totals the /goal reads:
//   slate: <n> slices: <p> PASS (passes true), <s> SHORT, <b> BLOCKED, <i> in progress, <t> todo
// A slice is "in progress" while its state is building / measuring / critic. It exits 1 when the slate is malformed: a missing field, a
// state outside the list below, or `passes: true` without a verdict file whose first line is "<id>: PASS" (and, for a player-facing
// slice, a `spot` file with a SPOT line that names a change).
//   node Tools/night/n72-slate.cjs [path]
'use strict';
const fs = require('fs');
const file = process.argv[2] || 'Logs/n74/slate.json';
const STATES = ['todo', 'building', 'measuring', 'critic', 'pass', 'short', 'blocked'];
const NEED = ['id', 'title', 'why', 'owner', 'row', 'red', 'doneWhen', 'visible', 'passes', 'state'];
let slate;
try { slate = JSON.parse(fs.readFileSync(file, 'utf8')); } catch (e) { console.log('slate: cannot read ' + file + ': ' + e.message); process.exit(1); }
const slices = Array.isArray(slate) ? slate : slate.slices;
if (!Array.isArray(slices)) { console.log('slate: no "slices" array in ' + file); process.exit(1); }
let bad = 0;
const n = { pass: 0, short: 0, blocked: 0, prog: 0, todo: 0 };
for (const s of slices) {
  const miss = NEED.filter(k => !(k in s));
  if (miss.length) { bad++; console.log('MALFORMED ' + (s.id || '?') + ': missing ' + miss.join(', ')); }
  if (!STATES.includes(s.state)) { bad++; console.log('MALFORMED ' + s.id + ': state "' + s.state + '" not in ' + STATES.join(' / ')); }
  if (s.passes === true) {
    // the verdict's first line must be "<id>: PASS"; a player-facing slice also needs a SPOT file whose line is not NOTHING OBVIOUS
    const first = s.verdict && fs.existsSync(s.verdict) ? fs.readFileSync(s.verdict, 'utf8').split(/\r?\n/)[0].trim() : '';
    if (first !== s.id + ': PASS') { bad++; console.log('MALFORMED ' + s.id + ': passes true but the verdict file\'s first line is "' + first + '", not "' + s.id + ': PASS"'); }
    if (!/^n\/a/.test(String(s.visible))) {
      const spot = s.spot && fs.existsSync(s.spot) ? fs.readFileSync(s.spot, 'utf8') : '';
      if (!/SPOT: /.test(spot) || /SPOT: NOTHING OBVIOUS/i.test(spot)) { bad++; console.log('MALFORMED ' + s.id + ': passes true without a SPOT line naming a change (field "spot")'); }
    }
  }
  if (s.passes === true && s.state !== 'pass') { bad++; console.log('MALFORMED ' + s.id + ': passes true but state ' + s.state); }
  if (s.state === 'pass') n.pass++; else if (s.state === 'short') n.short++; else if (s.state === 'blocked') n.blocked++;
  else if (s.state === 'todo') n.todo++; else n.prog++;
  console.log(`${String(s.id).padEnd(5)} ${String(s.state).padEnd(9)} passes ${String(s.passes).padEnd(5)} ${String(s.owner).padEnd(5)} ${s.title}`);
}
console.log(`slate: ${slices.length} slices: ${n.pass} PASS (passes true), ${n.short} SHORT, ${n.blocked} BLOCKED, ${n.prog} in progress, ${n.todo} todo${bad ? '; MALFORMED ' + bad : ''}`);
process.exit(bad ? 1 : 0);
