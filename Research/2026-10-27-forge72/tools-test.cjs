// FORGE72: the forge's dry run of the new n72 tools on a scratch copy (never the repo's files).
//   node Research/2026-10-27-forge72/tools-test.cjs <scratch dir>
// 1) n72-rows.cjs on scratch copies of both configs and FrameRows; 2) calm72 planted ON (as n72-freeze-rows would), then
// n72-off-row.cjs turns it OFF and the copies stay identical; 3) n72-pair.cjs refuses identical frames and accepts a real pair (written
// under the REPO's Logs/n72/pairs/forge-test, removed after); 4) n72-caps.cjs prints its line.
const fs = require('fs');
const path = require('path');
const { spawnSync } = require('child_process');
const ROOT = path.resolve(__dirname, '..', '..');
const S = path.resolve(process.argv[2]);
const run = (args, cwd) => { const r = spawnSync(process.execPath, args, { cwd: cwd || ROOT, encoding: 'utf8' }); return { code: r.status, out: (r.stdout || '') + (r.stderr || '') }; };
let bad = 0;
const ok = (name, pass, extra) => { if (!pass) bad++; console.log((pass ? 'ok   ' : 'BAD  ') + name + (extra ? ' -> ' + extra : '')); };
fs.rmSync(S, { recursive: true, force: true });
for (const [src, dst] of [['Design/unity-config.json', 'Design/unity-config.json'], ['Assets/StreamingAssets/Design/unity-config.json', 'Assets/StreamingAssets/Design/unity-config.json'], ['Assets/Scripts/Game/SameRun63Math.N63D.cs', 'Assets/Scripts/Game/SameRun63Math.N63D.cs']]) {
  fs.mkdirSync(path.dirname(path.join(S, dst)), { recursive: true }); fs.copyFileSync(path.join(ROOT, src), path.join(S, dst));
}
let r = run([path.join(ROOT, 'Tools/night/n72-rows.cjs'), '--root', S]);
ok('n72-rows writes 11 rows', r.code === 0 && /11 rows, all OFF/.test(r.out), r.out.split('\n')[0]);
const A = path.join(S, 'Design/unity-config.json'), B = path.join(S, 'Assets/StreamingAssets/Design/unity-config.json');
let t = fs.readFileSync(A, 'utf8');
const j = JSON.parse(t);
ok('the rows parse, all OFF, multi-line', Object.keys(j.design.n72).filter((k) => k !== '_why').every((k) => j.design.n72[k].enabled === false) && /"calm72": \{\r?\n\s+"enabled": false,/.test(t));
t = t.replace(/("calm72": \{\r?\n\s*"enabled": )false/, '$1true'); fs.writeFileSync(A, t); fs.writeFileSync(B, t);
r = run([path.join(ROOT, 'Tools/night/n72-off-row.cjs'), 'n72.calm72=planted RED for the forge test (no file)', '--root', S]);
const after = JSON.parse(fs.readFileSync(A, 'utf8'));
ok('n72-off-row turns a planted ON row OFF', r.code === 0 && after.design.n72.calm72.enabled === false && /^OFF at NIGHT72: /.test(after.design.n72.calm72.enabled_why), r.out.trim());
ok('the copies stay identical', fs.readFileSync(A, 'utf8') === fs.readFileSync(B, 'utf8'));
r = run([path.join(ROOT, 'Tools/night/n72-off-row.cjs'), 'n72.calm72=again, the row is OFF now (refused)', '--root', S]);
ok('n72-off-row refuses a row that is already OFF', r.code === 1, r.out.trim());
const shots = path.join(ROOT, 'Logs/feel/n71-forge72/bk/boss63');
const a = path.join(shots, 'shot-N30-B63-wyrm-idle.png'), b = path.join(shots, 'shot-N30-B63-wyrm-windup.png');
r = run([path.join(ROOT, 'Tools/night/n72-pair.cjs'), 'forge-test', a, a]);
ok('n72-pair refuses byte-identical frames', r.code === 1, r.out.trim());
r = run([path.join(ROOT, 'Tools/night/n72-pair.cjs'), 'forge-test', a, b]);
const P = path.join(ROOT, 'Logs/n72/pairs/forge-test'), SP = path.join(ROOT, 'Logs/n72/spot/forge-test');
ok('n72-pair writes on / off / key / sheet and the spotter copies', r.code === 0 && ['on.png', 'off.png', 'key.txt', 'sheet.jpg', 'numbers.txt'].every((f) => fs.existsSync(path.join(P, f))) && ['A.png', 'B.png'].every((f) => fs.existsSync(path.join(SP, f))) && !fs.existsSync(path.join(SP, 'key.txt')), r.out.split('\n').slice(0, 3).join(' | '));
fs.rmSync(P, { recursive: true, force: true }); fs.rmSync(SP, { recursive: true, force: true });
for (const d of ['Logs/n72/pairs', 'Logs/n72/spot', 'Logs/n72']) { try { fs.rmdirSync(path.join(ROOT, d)); } catch (e) { /* not empty or missing */ } }
r = run([path.join(ROOT, 'Tools/night/n72-caps.cjs')]);
ok('n72-caps prints its line', r.code === 0 && /^caps n72: builds \d+\/10, reel sides \d+\/220/.test(r.out), r.out.trim());
console.log(bad ? 'TOOLS TEST FAIL (' + bad + ')' : 'TOOLS TEST PASS');
process.exit(bad ? 1 : 0);
