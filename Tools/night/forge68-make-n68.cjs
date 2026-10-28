// FORGE68 (2026-10-24, the lead, right after NIGHT67's close): writes NIGHT68's tools as renamed copies of NIGHT67's (the forges' method),
// so NIGHT68 starts from tools that ran all of NIGHT67 and never writes NIGHT67's tracked evidence. NIGHT68 continues NIGHT67's rooms on
// NIGHT67's data, so the rename touches only the night's names (n67- tool names -> n68-, Logs/n67 -> Logs/n68, NIGHT67 -> NIGHT68,
// u3-n67 -> u3-n68) and keeps every data name: the level ids n67-iceline / n67-kiln / n67-spire, the rows design.n67.*67, the room files
// Design/levels/n67/, slots67.json, levels67.json, the cards Research/2026-10-24-wave67/cards/, --off67 / flow-off67 (no new rows tonight).
// Not copied: n67-a1.cjs (NIGHT67's shared commit; it writes the config rows and must not run twice).
// Each planned special edit must apply exactly once, or the tool stops without writing that file. Refuses to overwrite an n68 file.
//   node Tools/night/forge68-make-n68.cjs
'use strict';
const fs = require('fs');
const path = require('path');
const DIR = __dirname;
const SKIP = new Set(['n67-a1.cjs']);
const FILES = fs.readdirSync(DIR).filter((f) => /^n67-.*\.(cjs|sh)$/.test(f) && !SKIP.has(f)).sort();

const common = (s) => s
  .replace(/\bn67-(?!iceline|kiln|spire|foundry)/g, 'n68-')
  .replace(/Logs\/n67\b/g, 'Logs/n68')
  .replace(/'Logs', 'n67'/g, "'Logs', 'n68'")
  .replace(/NIGHT67/g, 'NIGHT68');

const SPECIAL = {
  'n67-clock.cjs': [
    ["night: 'n67'", "night: 'n68'"],
  ],
  'n67-close-check.cjs': [
    ["(default n67)", "(default n68)"],
    ["JSON.stringify(Object.assign({ night: 'n67',", "JSON.stringify(Object.assign({ night: 'n68',"],
  ],
};

let bad = 0;
for (const f of FILES) {
  const dst = path.join(DIR, f.replace(/^n67-/, 'n68-'));
  if (fs.existsSync(dst)) { console.log('exists, not overwritten: ' + dst); bad++; continue; }
  let s = fs.readFileSync(path.join(DIR, f), 'utf8').replace(/\r\n/g, '\n');
  s = common(s);
  if (f === 'n67-close-check.cjs') s = s.replace(/arg\('--night', 'n67'\)/g, "arg('--night', 'n68')").replace(/n67cc-/g, 'n68cc-').replace(/u3-n67/g, 'u3-n68').replace(/CLOSE-CHECK n67/g, 'CLOSE-CHECK n68').replace(/PRE-CHECK n67/g, 'PRE-CHECK n68');
  const missed = [];
  for (const [from, to] of (SPECIAL[f] || [])) {
    const n = s.split(from).length - 1;
    if (n !== 1) { missed.push('(' + n + 'x) ' + from.slice(0, 70)); continue; }
    s = s.replace(from, () => to);
  }
  if (missed.length) { console.log('NOT WRITTEN ' + f + ': ' + missed.join(' || ')); bad++; continue; }
  fs.writeFileSync(dst, s);
  if (f.endsWith('.sh')) fs.chmodSync(dst, 0o755);
  const left = s.split('\n').filter((l) => /\bn67\b|n67-(?!iceline|kiln|spire|foundry)|Logs\/n67/.test(l)).length;
  console.log(path.relative(path.join(DIR, '..', '..'), dst).replace(/\\/g, '/') + ': ' + s.split('\n').length + ' lines' + (SPECIAL[f] ? ', ' + SPECIAL[f].length + ' special edits' : '') + '; lines still naming n67: ' + left);
}
process.exit(bad ? 1 : 0);
