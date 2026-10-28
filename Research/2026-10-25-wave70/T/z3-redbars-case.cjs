// NIGHT70 builder T, slice Z3: a planted dry run of Tools/night/n70-red-bars.cjs's Z3 bar (read-only on the tree; writes a scratch folder only).
// The reds' flow files are the main tree's Logs/n70/red-<L>/<id>/flow-A.txt / flow-B.txt; the skip is this pair's 3-seed skip file, its header's
// "seeds 1..3 (--seeds 3)" rewritten to "seeds 1..20 (--seeds 20)" (PLANTED: the tool reads only seeds 1-20 reds) so the formula is exercised.
//   node Research/2026-10-25-wave70/T/z3-redbars-case.cjs <tag> <scratch dir>     (tag: GREEN3 = the committed run, NOCOMMIT3 = the plain build)
'use strict';
const fs = require('fs');
const path = require('path');
const cp = require('child_process');
const [tag, scratch] = process.argv.slice(2);
const MAIN = 'C:/Users/gaige/dev/SKYGRIND-unity';
for (const [L, id] of [['kiln', 'kb3'], ['spire', 'v2']]) {
  const d = path.join(scratch, 'logs', 'red-' + L, id); fs.mkdirSync(d, { recursive: true });
  for (const f of ['flow-A.txt', 'flow-B.txt']) fs.copyFileSync(path.join(MAIN, 'Logs/n70/red-' + L, id, f), path.join(d, f));
  const sk = fs.readFileSync(path.join(__dirname, 'Z3-skip-' + id + '-' + tag + '.txt'), 'utf8').replace('seeds 1..3 (--seeds 3)', 'seeds 1..20 (--seeds 20)');
  fs.writeFileSync(path.join(d, 'skip.txt'), sk);
}
const r = cp.spawnSync(process.execPath, ['Tools/night/n70-red-bars.cjs', '--slate', MAIN + '/Logs/n70/slate.json', '--logs', path.join(scratch, 'logs').replace(/\\/g, '/')], { encoding: 'utf8' });
console.log('# planted: the ' + tag + ' 3-seed skips as the reds (header rewritten to seeds 1..20), the main tree\'s red flow files');
console.log((r.stdout || '') + (r.stderr || ''));
