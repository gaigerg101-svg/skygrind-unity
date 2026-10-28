// FORGE66 (2026-10-23, the lead): writes NIGHT66's tools as renamed copies of NIGHT65's (FORGE63 D-8's method), so NIGHT66 starts from tools
// that ran all of NIGHT65. Changes beyond the rename: the clock is an 8-hour box for GG's "next detailed night prompt that will go for 8
// hours" (BUILD 0:00-7:15, FREEZE 7:15-8:00 = the close, CAP 8:00, stale after 10 h so a long usage stop still reads CAP); the gates'
// fingerprint loop gains --off66; the flip tool binds a LEVEL row's PASS to its level files and slot set (the NIGHT65 integrated review's
// S2, Logs/n65/review-n65.md), with a fifth planted self-test case. Refuses to overwrite an existing n66 file. Prints each file and the lines
// that still name 65.
//   node Tools/night/forge66-make-n66.cjs
'use strict';
const fs = require('fs');
const path = require('path');
const DIR = path.join(__dirname);
const FILES = ['n65-clock.cjs', 'n65-freeze-rows.cjs', 'n65-gates.sh', 'n65-prune.sh', 'n65-reels.sh', 'n65-slate.cjs', 'n65-spot-pair.cjs',
  'n65-slate-set.cjs', 'n65-slate-add.cjs'];

function common(s) {
  return s
    .replace(/the n65 names/g, 'the n66 names')
    .replace(/n65-/g, 'n66-')
    .replace(/Logs\/n65/g, 'Logs/n66')
    .replace(/'Logs', 'n65'/g, "'Logs', 'n66'")
    .replace(/NIGHT65/g, 'NIGHT66')
    .replace(/flow-off65/g, 'flow-off66')
    .replace(/--off65/g, '--off66');
}

const LEVEL_FN = [
  '// FORGE66 (the NIGHT65 integrated review\'s S2, Logs/n65/review-n65.md): a LEVEL row\'s PASS binds its level, so passFiles must name the',
  '// level file in both copies and its slot set (Design/levels/<px>/slotsNN.json); else the level could be re-assembled after the PASS',
  'function levelFilesMissing(r, list) {',
  '  if (!r || !r.levelTo) return [];',
  '  const have = list.map((f) => String(f).replace(/\\\\/g, "/"));',
  '  const need = ["Design/levels/" + r.levelTo + ".json", "Assets/StreamingAssets/Design/levels/" + r.levelTo + ".json"];',
  '  const miss = need.filter((p) => !have.includes(p));',
  '  if (!have.some((p) => /^Design\\/levels\\/.+\\/slots\\d+\\.json$/.test(p))) miss.push("Design/levels/<px>/slotsNN.json");',
  '  return miss;',
  '}',
].join('\n');

const special = {
  'n65-clock.cjs': s => s
    .replace(/^\/\/ NIGHT66 clock[^\n]*/m, "// NIGHT66 clock (FORGE66, 2026-10-23, for GG's \"next detailed night prompt that will go for 8 hours\"; NIGHT65's clock renamed): the run's t0 and its phases, kept in")
    .replace(/prints the date, h:mm \/ 7:00,/g, 'prints the date, h:mm / 8:00,')
    .replace(/BUILD \(0:00-6:15\) anything; FREEZE \(6:15-7:00: the close\)/g, 'BUILD (0:00-7:15) anything; FREEZE (7:15-8:00: the close)')
    .replace(/CAP \(7:00 on\)/g, 'CAP (8:00 on)')
    .replace(/night: 'n65', freezeMin: 375, capMin: 420, staleMin: 600, closeTags: \['n66-freeze'\]/,
      "night: 'n66', freezeMin: 435, capMin: 480, staleMin: 720, closeTags: ['n66-freeze']")
    .replace(/The guard ignores a clock older than 10 hours \(stale\)/, 'The guard ignores a clock older than 12 hours (stale: FORGE66, the code scout)'),
  'n65-gates.sh': s => s.replace('"--off64" "--off66"; do', '"--off64" "--off65" "--off66"; do'),
  'n65-freeze-rows.cjs': s => s
    .replace(/(function ownFiles\(list\)[^\n]*\n)/, '$1' + LEVEL_FN + '\n')
    // FORGE66 (the code scout: a newer, unreviewed assembly left uncommitted passed the commit-only check): compare passSha to the WORKING TREE
    .replace('execFileSync("git", ["diff", "--name-only", sha, "HEAD", "--", ...files]', 'execFileSync("git", ["diff", "--name-only", sha, "--", ...files]')
    .replace(/(  const files = ownFiles\(hit\.passFiles\);\n)/,
      '$1  { const [nn, rr] = key.split("."); const cur = JSON.parse(fs.readFileSync(A, "utf8")).design; const miss = levelFilesMissing(cur[nn] && cur[nn][rr], hit.passFiles);\n' +
      '    if (miss.length) throw new Error(key + ": a level row\'s PASS must bind its level files: passFiles lacks " + miss.join(", ") + " (FORGE66; the NIGHT65 review\'s S2)"); }\n')
    .replace(/(    \["the config copies are never own files",[^\n]*\n)/,
      '$1    ["a level row\'s PASS without its level files refuses; with them it passes", levelFilesMissing({ levelTo: "n65-foundry" }, ["Logs/x.md"]).length === 3 && levelFilesMissing({ levelTo: "n65-foundry" }, ["Design/levels/n65-foundry.json", "Assets/StreamingAssets/Design/levels/n65-foundry.json", "Design/levels/n65/slots65.json"]).length === 0 && levelFilesMissing({ enabled: false }, []).length === 0],\n'),
};

for (const f of FILES) {
  const src = path.join(DIR, f);
  const dst = path.join(DIR, f.replace('n65-', 'n66-'));
  if (fs.existsSync(dst)) { console.log('exists, not overwritten: ' + dst); process.exitCode = 1; continue; }
  let s = common(fs.readFileSync(src, 'utf8'));
  if (special[f]) {
    const before = s;
    s = special[f](s);
    if (s === before) { console.log('SPECIAL DID NOT APPLY: ' + f); process.exitCode = 1; }
  }
  fs.writeFileSync(dst, s);
  if (f.endsWith('.sh')) fs.chmodSync(dst, 0o755);
  const left = s.split('\n').map((l, i) => [i + 1, l]).filter(([, l]) => /65/.test(l));
  console.log(dst + ': ' + s.split('\n').length + ' lines; lines still naming 65: ' + (left.length ? left.map(([n, l]) => n + ' ' + l.trim().slice(0, 90)).join(' || ') : 'none'));
}
