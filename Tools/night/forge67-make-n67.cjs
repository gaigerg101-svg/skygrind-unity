// FORGE67 (2026-10-24, the lead, right after NIGHT66's close): writes NIGHT67's tools as renamed copies of NIGHT66's (the forges' method),
// so NIGHT67 starts from tools that ran all of NIGHT66 and never writes NIGHT66's tracked evidence. Beyond the rename (n66- -> n67-,
// Logs/n66 -> Logs/n67, NIGHT66 -> NIGHT67, --off66 -> --off67 with --off66 kept in the fingerprint loop):
//   - n67-close-check.cjs reads the verbs37 side on n65-foundry (vn) only when the level row is ON: with the row OFF, n65-foundry has no
//     campaign place, so the reel gives the grapple and its rows find no node (NIGHT66's close: vn RED 22 / 27, rows 17-23; NIGHT66-LOG.md
//     "The close" (c)); a planted case proves the row-OFF close passes on the five shipped sides.
// Comments that cite NIGHT66 evidence are renamed too: that evidence stays under Logs/n66 and Research/2026-10-23-wave66.
// Each planned special edit must apply exactly once, or the tool stops without writing that file. Refuses to overwrite an n67 file.
//   node Tools/night/forge67-make-n67.cjs
'use strict';
const fs = require('fs');
const path = require('path');
const DIR = __dirname;
const FILES = fs.readdirSync(DIR).filter((f) => /^n66-.*\.(cjs|sh)$/.test(f)).sort();

const common = (s) => s
  .replace(/\bn66-/g, 'n67-')
  .replace(/Logs\/n66\b/g, 'Logs/n67')
  .replace(/'Logs', 'n66'/g, "'Logs', 'n67'")
  .replace(/NIGHT66/g, 'NIGHT67')
  .replace(/flow-off66/g, 'flow-off67')
  .replace(/--off66/g, '--off67');

const SPECIAL = {
  'n66-clock.cjs': [
    ["night: 'n66'", "night: 'n67'"],
  ],
  'n66-gates.sh': [
    ['"--off65" "--off67"; do', '"--off65" "--off66" "--off67"; do'],
  ],
  'n66-close-check.cjs': [
    ["const SIDES = ['vf', 'vn', 'vi', 'vk', 'vs', 'vp'];",
      "// FORGE67 (NIGHT66's close: vn RED 22 / 27 with the row OFF, the grapple rows): the n65-foundry side counts only when the level row is ON\n" +
      "const rowOn = (() => { try { const [n, r] = ROW.split('.'); return JSON.parse(rd('Design/unity-config.json')).design[n][r].enabled === true; } catch (e) { return false; } })();\n" +
      "const SIDES = rowOn ? ['vf', 'vn', 'vi', 'vk', 'vs', 'vp'] : ['vf', 'vi', 'vk', 'vs', 'vp'];"],
    ["const cfg = '{\"design\":{\"n65\":{\"foundry65\":{\"enabled\":true}}}}\\n';",
      "const cfg = '{\"design\":{\"n65\":{\"foundry65\":{\"enabled\":' + (o.rowOff ? 'false' : 'true') + '}}}}\\n';"],
    ["(o.rowReport === 'match' ? 'ROW: n65.foundry65 enabled=true; SLOTS: a2=new a3=old\\n'",
      "(o.rowReport === 'match' ? 'ROW: n65.foundry65 enabled=' + (o.rowOff ? 'false' : 'true') + '; SLOTS: a2=new a3=old\\n'"],
    ["    ['a verbs37 side missing fails the Player',",
      "    ['the row OFF: the five shipped sides pass without n65-foundry (FORGE67)', { rowOff: true, player: 'no-vn' }, null, (r) => r.last === 'CLOSE-CHECK n67: PASS'],\n    ['a verbs37 side missing fails the Player',"],
  ],
};

let bad = 0;
for (const f of FILES) {
  const dst = path.join(DIR, f.replace(/^n66-/, 'n67-'));
  if (fs.existsSync(dst)) { console.log('exists, not overwritten: ' + dst); bad++; continue; }
  let s = fs.readFileSync(path.join(DIR, f), 'utf8').replace(/\r\n/g, '\n');
  s = common(s);
  if (f === 'n66-close-check.cjs') s = s.replace(/'n66'/g, "'n67'").replace(/n66cc-/g, 'n67cc-').replace(/u3-n66/g, 'u3-n67').replace(/CLOSE-CHECK n66/g, 'CLOSE-CHECK n67').replace(/PRE-CHECK n66/g, 'PRE-CHECK n67');
  const missed = [];
  for (const [from, to] of (SPECIAL[f] || [])) {
    const n = s.split(from).length - 1;
    if (n !== 1) { missed.push('(' + n + 'x) ' + from.slice(0, 70)); continue; }
    s = s.replace(from, () => to);
  }
  if (missed.length) { console.log('NOT WRITTEN ' + f + ': ' + missed.join(' || ')); bad++; continue; }
  fs.writeFileSync(dst, s);
  if (f.endsWith('.sh')) fs.chmodSync(dst, 0o755);
  const left = s.split('\n').filter((l) => /\bn66\b|n66-|Logs\/n66/.test(l)).length;
  console.log(path.relative(path.join(DIR, '..', '..'), dst).replace(/\\/g, '/') + ': ' + s.split('\n').length + ' lines' + (SPECIAL[f] ? ', ' + SPECIAL[f].length + ' special edits' : '') + '; lines still naming n66: ' + left);
}
process.exit(bad ? 1 : 0);
