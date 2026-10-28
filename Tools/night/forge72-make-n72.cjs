// FORGE72 (2026-10-27, the lead, after the w71 quick-fix session): writes NIGHT72's night-bound tools as renamed copies of NIGHT71's (the
// forges' method: forge71-make-n71.cjs). Beyond the rename (n71 -> n72, NIGHT71 -> NIGHT72, Logs/n71 -> Logs/n72, u3-n71 -> u3-n72; data
// names stay: n67 levels, design.n67.*67, slots67.json, --off67) it changes:
//   - n72-clock.cjs: GG 2026-10-27 "6-7.5 hours hard cap": FREEZE at 6:45, CAP at 7:30 (the close check's windows follow: the close starts
//     at capMin - 60 = 6:30 or later, the clock stops at capMin - 30 = 7:00 or later);
//   - n72-close-check.cjs: a `seen` check (FORGE72 E72, DECISIONS D-1: ship ON by default): every slate slice in state pass that names a row
//     in its optional `on` field has that row enabled true in BOTH config copies; two self-test cases for it;
//   - n72-freeze-rows.cjs: the label "ON at NIGHT72 by the rule: " (the rename does it) and nothing else.
// Each special edit must apply exactly once, else the tool stops without writing that file. Refuses to overwrite an n72 file.
//   node Tools/night/forge72-make-n72.cjs
'use strict';
const fs = require('fs');
const path = require('path');
const DIR = __dirname;
const FILES = ['n71-clock.cjs', 'n71-close-check.cjs', 'n71-slate.cjs', 'n71-slate-set.cjs', 'n71-slate-add.cjs', 'n71-runner.sh',
  'n71-gates.sh', 'n71-reels.sh', 'n71-prune.sh', 'n71-freeze-rows.cjs'];

const common = (s) => s.replace(/\bn71\b/g, 'n72').replace(/NIGHT71/g, 'NIGHT72').replace(/PLAY71/g, 'PLAY72');

const SEEN = [
  "// running\nR.running = running();",
  "// FORGE72 (E72, DECISIONS D-1: ship ON by default): every slate slice in state pass that names a row in its `on` field (\"n63.colours63\")\n" +
  "// has that row enabled true in BOTH config copies, so a slice cannot pass while GG's build still has it OFF\n" +
  "R.seen = (() => {\n" +
  "  try {\n" +
  "    const sj = JSON.parse(rd('Logs/' + NIGHT + '/slate.json')); const sl = Array.isArray(sj) ? sj : sj.slices;\n" +
  "    const cA = JSON.parse(A).design, cB = JSON.parse(B).design;\n" +
  "    const on = (c, k) => { const [n, r] = String(k).replace(/^design\\./, '').split('.'); return !!(c[n] && c[n][r] && c[n][r].enabled === true); };\n" +
  "    const want = sl.filter((s) => s.state === 'pass' && s.on);\n" +
  "    const off = want.filter((s) => !(on(cA, s.on) && on(cB, s.on)));\n" +
  "    return [off.length === 0, want.length + ' passed slice(s) naming a row to turn ON' + (want.length ? ' (' + want.map((s) => s.on).join(' ') + ')' : '') + (off.length ? '; NOT ON in both copies: ' + off.map((s) => s.on).join(' ') : '; each ON in both copies')];\n" +
  "  } catch (e) { return [false, 'unreadable: ' + e.message]; }\n" +
  "})();\n" +
  "// running\nR.running = running();",
];

const EDITS = {
  'n71-clock.cjs': [
    ["const DEF = { night: 'n72', freezeMin: 435, capMin: 480,", "const DEF = { night: 'n72', freezeMin: 405, capMin: 450,"],
    ['h:mm / 8:00', 'h:mm / 7:30'],
    ['Phases: BUILD (0:00-7:15) anything; FREEZE (7:15-8:00: the close)', 'Phases (FORGE72: a 7:30 cap): BUILD (0:00-6:45) anything; FREEZE (6:45-7:30: the close)'],
    ['CAP (8:00 on)', 'CAP (7:30 on)'],
  ],
  'n71-close-check.cjs': [
    SEEN,
    ["const PRE = ['start', 'files', 'frozen', 'player', 'gates', 'play', 'slate', 'row', 'running'];", "const PRE = ['start', 'files', 'frozen', 'player', 'gates', 'play', 'slate', 'row', 'seen', 'running'];"],
    ["const FULL = ['clock', 'start', 'files', 'frozen', 'tag', 'player', 'gates', 'play', 'slate', 'row', 'running'];", "const FULL = ['clock', 'start', 'files', 'frozen', 'tag', 'player', 'gates', 'play', 'slate', 'row', 'seen', 'running'];"],
    ["const sl = (id, state, passes, visible, verdict) => ({ id, title: 't', why: 'w', owner: 'lead', row: 'r', red: 'r', doneWhen: ['d'], visible, passes, state, verdict });",
     "const sl = (id, state, passes, visible, verdict) => Object.assign({ id, title: 't', why: 'w', owner: 'lead', row: 'r', red: 'r', doneWhen: ['d'], visible, passes, state, verdict }, o.seenRow && id === 'T1' ? { on: 'n65.foundry65' } : {});"],
    ["    ['the guard\\'s refusal of the play build quoted under Blocked, the card committed, passes (FORGE71)',",
     "    ['a passed slice whose `on` row is ON in both copies passes seen (FORGE72)', { seenRow: true }, null, (r) => r.last === 'CLOSE-CHECK n72: PASS' && /PASS seen +1 passed slice/.test(r.out)],\n" +
     "    ['a passed slice whose `on` row is OFF fails seen (FORGE72)', { seenRow: true, rowOff: true, player: 'no-vn' }, null, (r) => r.last === 'CLOSE-CHECK n72: FAIL seen'],\n" +
     "    ['a 7:30 night (cap 450): a close started at 6:35 and a clock stopped at 7:05 pass (FORGE72)', { cap: 450, startMin: 395, frozenMin: 360, closeMin: 410, stopMin: 425 }, null, (r) => r.last === 'CLOSE-CHECK n72: PASS'],\n" +
     "    ['a 7:30 night (cap 450): a clock stopped at 6:55 fails the clock (FORGE72)', { cap: 450, startMin: 395, frozenMin: 360, closeMin: 410, stopMin: 415 }, null, (r) => r.last === 'CLOSE-CHECK n72: FAIL clock'],\n" +
     "    ['the guard\\'s refusal of the play build quoted under Blocked, the card committed, passes (FORGE71)',"],
    ['//   running   none started since t0:', '//   seen      (FORGE72) every slate slice in state pass with an `on` row has that row enabled true in both config copies\n//   running   none started since t0:'],
  ],
};

let wrote = 0;
for (const f of FILES) {
  const src = path.join(DIR, f), dst = path.join(DIR, f.replace('n71-', 'n72-'));
  if (fs.existsSync(dst)) { console.log('refused: ' + path.basename(dst) + ' exists'); process.exit(1); }
  let s = common(fs.readFileSync(src, 'utf8'));
  for (const [a, b] of EDITS[f] || []) {
    const n = s.split(a).length - 1;
    if (n !== 1) { console.log('edit not applied exactly once in ' + f + ' (' + n + 'x): ' + a.slice(0, 80)); process.exit(1); }
    s = s.replace(a, () => b);
  }
  fs.writeFileSync(dst, s);
  try { fs.chmodSync(dst, fs.statSync(src).mode); } catch (e) { /* Windows */ }
  wrote++;
}
console.log('forge72-make-n72: wrote ' + wrote + ' n72 tools from the n71 ones');
