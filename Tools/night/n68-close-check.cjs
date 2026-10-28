#!/usr/bin/env node
// NIGHT68 (FORGE66 D-10, research.md finding 10: "grade what the agent produced, not the path it took"): the close's END STATE in one
// check, so the /goal judge reads one line instead of reconstructing an order from history (NIGHT65's judge looped on "in order" and
// "first committed"). FORGE66's red team (redteam.md findings 4, 5, 12) hardened it: the process check works from PowerShell and Git Bash
// and counts only this night's jobs; the Player, gates and slate lines need the files, not words in the log; a --pre pass and a --tag step
// come before the tag, so nothing the final check can fail needs a commit; a --selftest walks every verdict in scratch repos.
//   node Tools/night/n68-close-check.cjs --selftest   the planted cases (scratch git repos under the OS temp folder, deleted after)
//   node Tools/night/n68-close-check.cjs --start      at the close's start: records Logs/n68/close-start.txt (refused before 7:00 on the clock)
//   node Tools/night/n68-close-check.cjs --pre        every check but clock and tag: PRE-CHECK n68: PASS | FAIL <checks> (before the tag)
//   node Tools/night/n68-close-check.cjs --tag        --pre, then `git tag u3-n68` on HEAD only if it passed (never moves a tag); with
//                                                     --minimum (section 7's minimum close) the Player and gates lines may be missing
//                                                     when the close started at 7:30 or later or the clock is past 8:00
//   node Tools/night/n68-close-check.cjs              the checks, one line each, then the last line is exactly one of:
//       CLOSE-CHECK n68: PASS              every check below
//       CLOSE-CHECK n68: MINIMUM PASS      (after a "failed: ..." line) a stop ate the close: close-start.txt reads 7:30 or later, or the clock
//                                          stopped at 8:00 or later; still needs the clock stopped at 7:30 or later, the four files clean
//                                          with sections 2 and 8, the tag, the slate, the row line and nothing running
//       CLOSE-CHECK n68: FAIL <checks>     otherwise
//   options: --night nNN (default n68), --rows row=slotsfile,... (default level 1's row and NIGHT68's three: n67.iceline67, n67.kiln67,
//   n67.spire67, each with Design/levels/n67/<level>/slots67.json; one ROW line per row), or --level-row <row> --slots <file> for one; for dry runs and the self-test:
//   --root <dir> (the repo), --clock <file> (a saved clock.json), --procs <file> (a planted process list, JSON)
// Run the final check alone, in the foreground, not redirected and not chained after another script (NIGHT68.md section 9 (l)).
// The checks (FULL):
//   clock     the clock is this night's, stopped at 7:30 or later since t0 and before 8:00
//   start     Logs/<night>/close-start.txt exists and reads 7:00 or later since t0
//   files     NIGHT<NN>-REPORT.md (with "## 2." and "## 8."), NIGHT<NN>-LOG.md, HANDOFF.md, docs/memory/CORE.md: clean in git, and each
//             last committed at or after the close's start
//   frozen    Design/levels/n65/slots65.json and both config copies: clean, and last committed before the close's start (no GATE, slot or
//             flip in the close)
//   tag       u3-<night> points at HEAD, and HEAD was committed at or before the clock's stop (nothing committed after either)
//   player    Logs/feel/<night>-freeze/summary47.txt: each of the sides vf vn vi vk vs vp has "verbs37: PASS: total N/N" in its last
//             section, and Logs/player/n56-smoke-steal-<night>-freeze.log has "DONE: all steps passed"; OR NIGHT<NN>-LOG.md's
//             "## Blocked" quotes the guard ("SKYGRIND guard blocked this") and names the freeze build, verbs37 and the smoke
//   gates     Logs/<night>/gates-flow-close.txt's last "gates:" line reads "gates: all GREEN"; OR there is no such file and "## Blocked"
//             quotes the guard and names the gates
//   slate     node Tools/night/<night>-slate.cjs exits 0 and prints "0 in progress" and "0 todo"
//   row       both config copies byte-identical; the report's section 8 holds the line
//             "ROW: <row> enabled=<bool>; SLOTS: <id>=<old|new> ..." equal to the config and the slot file
//   running   none started since t0: a dotnet / SimHarness / CoreHarness process of this repo's harnesses, a SKYGRIND.exe under
//             Builds\<night>-* (one started from Explorer is a person's and is ignored), a bash running Tools/night/ (each printed with its
//             PID; the check's own parent chain is excluded)
'use strict';
const fs = require('fs');
const os = require('os');
const path = require('path');
const { execFileSync, spawnSync } = require('child_process');
const argv = process.argv.slice(2);
const arg = (k, d) => { const i = argv.indexOf(k); return i >= 0 && i + 1 < argv.length ? argv[i + 1] : d; };
const has = (k) => argv.includes(k);

if (has('--selftest')) { selftest(); return; }

const ROOT = path.resolve(arg('--root', path.join(__dirname, '..', '..')));
const NIGHT = arg('--night', 'n68'), NN = NIGHT.replace(/^n/, '');
const LOGS = path.join(ROOT, 'Logs', NIGHT);
const git = (...a) => execFileSync('git', a, { cwd: ROOT, encoding: 'utf8', maxBuffer: 64 << 20 }).trim();
const rd = (p) => { try { return fs.readFileSync(path.join(ROOT, p), 'utf8'); } catch (e) { return null; } };
// FORGE67b: several level rows, each with its slot file (`--rows row=slots,...`; `--level-row` / `--slots` give one row); the vn side's
// row is level 1's (n65.foundry65) whatever the list
// FORGE67b red team 1: by default, level 1's row plus every design.n67.<x>67 row that EXISTS in the config (its slots file
// Design/levels/n67/<x>/slots67.json, which the shared commit writes with every slot old); a row in the config without its slots file fails
const n67Rows = () => { const raw = rd('Design/unity-config.json'); if (raw === null) return []; const n67 = (JSON.parse(raw).design || {}).n67 || {};
  return Object.keys(n67).filter((k) => /^[a-z]+67$/.test(k) && n67[k] && typeof n67[k] === 'object' && 'levelTo' in n67[k]).map((k) => ['n67.' + k, 'Design/levels/n67/' + k.replace(/67$/, '') + '/slots67.json']); };
const ROWS = has('--level-row') ? [[arg('--level-row'), arg('--slots', 'Design/levels/n65/slots65.json')]]
  : has('--rows') ? arg('--rows').split(',').map((p) => p.split('='))
  : [['n65.foundry65', 'Design/levels/n65/slots65.json'], ...n67Rows()];
const VN_ROW = 'n65.foundry65';
const CLOCK = arg('--clock', path.join(ROOT, 'Logs', 'clock.json'));
const clock = (() => { try { return JSON.parse(fs.readFileSync(path.resolve(ROOT, CLOCK), 'utf8')); } catch (e) { return null; } })();
const minSince = (ms) => clock ? (ms / 1000 - clock.t0) / 60 : NaN;
const hm = (m) => Number.isFinite(m) ? Math.floor(m / 60) + ':' + String(Math.floor(m % 60)).padStart(2, '0') : '?';
const GUARD = /SKYGRIND guard blocked this/;

if (has('--start')) {
  if (!clock || clock.night !== NIGHT) { console.log('close-check --start: the clock is not ' + NIGHT + "'s"); process.exit(1); }
  const m = minSince(Date.now());
  if (m < (clock.capMin - 60)) { console.log('close-check --start: refused at ' + hm(m) + ' (the close starts at ' + hm(clock.capMin - 60) + ' or later)'); process.exit(1); }
  fs.mkdirSync(LOGS, { recursive: true });
  fs.writeFileSync(path.join(LOGS, 'close-start.txt'), new Date().toISOString() + ' ' + hm(m) + '\n');
  console.log('close-check --start: the close starts at ' + hm(m) + ' since t0 (Logs/' + NIGHT + '/close-start.txt)');
  process.exit(0);
}

const R = {};
// clock
let stopM = NaN;
if (!clock || clock.night !== NIGHT) R.clock = [false, 'the clock is not ' + NIGHT + "'s"];
else if (!clock.stopped) R.clock = [false, 'the clock is not stopped'];
else {
  stopM = minSince(new Date(clock.stoppedText).getTime());
  const lo = clock.capMin - 30;
  R.clock = [stopM >= lo && stopM < clock.capMin, 'stopped at ' + hm(stopM) + ' (' + String(clock.stoppedText).slice(0, 24) + '; want ' + hm(lo) + ' to ' + hm(clock.capMin) + ')'];
}
const clockMin = clock && clock.stopped && stopM >= clock.capMin - 30;
// start
const startTxt = rd('Logs/' + NIGHT + '/close-start.txt');
const startT = startTxt ? new Date(startTxt.split(' ')[0]) : null;
const startM = startT ? minSince(startT.getTime()) : NaN;
R.start = [!!startT && !!clock && startM >= clock.capMin - 60, startT ? 'the close started at ' + hm(startM) : 'no Logs/' + NIGHT + '/close-start.txt'];
const lastCommit = (f) => { const t = git('log', '-1', '--format=%ct', '--', f); return t ? Number(t) * 1000 : NaN; };
// files
const FILES = ['NIGHT' + NN + '-REPORT.md', 'NIGHT' + NN + '-LOG.md', 'HANDOFF.md', 'docs/memory/CORE.md'];
const dirty = git('status', '--short', '--', ...FILES);
const report = rd(FILES[0]) || '';
const sections = /^## 2\./m.test(report) && /^## 8\./m.test(report);
const late = startT ? FILES.filter((f) => !(lastCommit(f) >= startT.getTime())) : FILES;
R.files = [!dirty && sections && late.length === 0, (dirty ? 'not clean: ' + dirty.replace(/\n/g, ', ') + '; ' : 'clean; ') + (sections ? 'the report has sections 2 and 8; ' : 'the report lacks section 2 or 8; ') + (late.length ? 'last committed before the close start: ' + late.join(', ') : 'each last committed in the close')];
const filesMin = !dirty && sections;
// frozen
const FROZEN = [...ROWS.map((x) => x[1]), 'Design/unity-config.json', 'Assets/StreamingAssets/Design/unity-config.json'];
const fdirty = git('status', '--short', '--', ...FROZEN);
const fLate = startT ? FROZEN.filter((f) => !(lastCommit(f) < startT.getTime())) : FROZEN;
R.frozen = [!fdirty && fLate.length === 0, (fdirty ? 'not clean: ' + fdirty.replace(/\n/g, ', ') + '; ' : 'clean; ') + (fLate.length ? 'committed in the close (or no start): ' + fLate.join(', ') : 'slots and configs last committed before the close start')];
// tag
let tagSha = ''; try { tagSha = git('rev-parse', '--short', 'u3-' + NIGHT + '^{commit}'); } catch (e) { tagSha = ''; }
const head = git('rev-parse', '--short', 'HEAD');
const headT = Number(git('log', '-1', '--format=%ct', 'HEAD')) * 1000;
const stopT = clock && clock.stopped ? new Date(clock.stoppedText).getTime() : NaN;
const afterStop = Number.isFinite(stopT) && headT > stopT + 1000;
R.tag = [!!tagSha && tagSha === head && !afterStop, (tagSha ? 'u3-' + NIGHT + ' -> ' + tagSha + ' (HEAD ' + head + ')' : 'no tag u3-' + NIGHT) + (afterStop ? '; HEAD was committed after the clock stop' : '')];
// player / gates: the files, or the guard's refusal quoted under ## Blocked
const log = rd(FILES[1]) || '';
const blocked = (log.split(/^## Blocked/m)[1] || '').split(/^## /m)[0];
const guardQuoted = GUARD.test(blocked);
const summary = rd('Logs/feel/' + NIGHT + '-freeze/summary47.txt') || '';
const lastSide = {};
for (const sec of summary.split(/^== /m).slice(1)) { const side = sec.split(/\s/)[0]; lastSide[side] = sec; }
// FORGE67 (NIGHT66's close: vn RED 22 / 27 with the row OFF, the grapple rows): the n65-foundry side counts only when the level row is ON
const rowOn = (() => { try { const [n, r] = VN_ROW.split('.'); return JSON.parse(rd('Design/unity-config.json')).design[n][r].enabled === true; } catch (e) { return false; } })();
const SIDES = rowOn ? ['vf', 'vn', 'vi', 'vk', 'vs', 'vp'] : ['vf', 'vi', 'vk', 'vs', 'vp'];
const sidesOk = SIDES.filter((s) => lastSide[s] && /verbs37: PASS: total (\d+)\/\1\b/.test(lastSide[s]));
const smoke = /DONE: all steps passed/.test(rd('Logs/player/n56-smoke-steal-' + NIGHT + '-freeze.log') || '');
const blockedPlayer = guardQuoted && /freeze build/i.test(blocked) && /verbs37/i.test(blocked) && /smoke/i.test(blocked);
R.player = [(sidesOk.length === SIDES.length && smoke) || blockedPlayer, sidesOk.length === SIDES.length && smoke ? 'verbs37 PASS on ' + SIDES.join(' ') + ' and the smoke DONE (' + NIGHT + '-freeze)' : (blockedPlayer ? "the guard's refusal of the freeze build, verbs37 and the smoke quoted under ## Blocked" : 'verbs37 PASS on [' + sidesOk.join(' ') + '] of ' + SIDES.join(' ') + ', smoke ' + smoke + ', no quoted guard refusal under ## Blocked')];
const gatesTxt = rd('Logs/' + NIGHT + '/gates-flow-close.txt');
const gLast = ((gatesTxt || '').match(/^gates: .*$/gm) || []).pop() || '';
const blockedGates = gatesTxt === null && guardQuoted && /gates/i.test(blocked);
R.gates = [/^gates: all GREEN/.test(gLast) || blockedGates, gLast || (blockedGates ? "the guard's refusal of the gates quoted under ## Blocked" : (gatesTxt === null ? 'no Logs/' + NIGHT + '/gates-flow-close.txt' : 'no "gates:" line'))];
// slate
const sl = spawnSync(process.execPath, [path.join('Tools', 'night', NIGHT + '-slate.cjs')], { cwd: ROOT, encoding: 'utf8' });
const slate = String(sl.stdout || '').trim().split('\n').pop() || 'the slate tool printed nothing';
R.slate = [sl.status === 0 && / 0 in progress/.test(slate) && / 0 todo/.test(slate), slate + (sl.status === 0 ? '' : ' (exit ' + sl.status + ')')];
// row vs slots vs the report
const A = rd('Design/unity-config.json'), B = rd('Assets/StreamingAssets/Design/unity-config.json');
const rowLineOf = (ROW, SLOTS) => {
  try {
    const [n, r] = ROW.split('.');
    const en = JSON.parse(A).design[n][r].enabled;
    const raw = rd(SLOTS); if (raw === null) return 'unreadable: no ' + SLOTS;
    const sj = JSON.parse(raw); const slots = sj.slots || sj;
    const list = Object.keys(slots).filter((k) => !k.startsWith('_') && slots[k] && typeof slots[k] === 'object').map((k) => k + '=' + slots[k].use).join(' ');
    return 'ROW: ' + ROW + ' enabled=' + en + '; SLOTS: ' + list;
  } catch (e) { return 'unreadable: ' + ROW + ': ' + e.message; }
};
const rowLines = ROWS.map(([r, s]) => rowLineOf(r, s));
const sec8 = (report.split(/^## 8\./m)[1] || '');
const rowsIn = rowLines.every((l) => !/^unreadable/.test(l) && sec8.includes(l));
R.row = [A !== null && A === B && rowsIn, (A === B ? 'copies identical; ' : 'copies DIFFER; ') + rowLines.map((l) => l + (sec8.includes(l) ? ' (in the report)' : ' (NOT in the report section 8)')).join(' | ')];
// running
R.running = running();

const PRE = ['start', 'files', 'frozen', 'player', 'gates', 'slate', 'row', 'running'];
const FULL = ['clock', 'start', 'files', 'frozen', 'tag', 'player', 'gates', 'slate', 'row', 'running'];
const ks = has('--pre') || has('--tag') ? PRE : FULL;
for (const k of ks) console.log((R[k][0] ? 'PASS ' : 'FAIL ') + k.padEnd(8) + R[k][1]);
const failed = ks.filter((k) => !R[k][0]);
if (has('--pre') || has('--tag')) {
  // --tag --minimum (NIGHT68.md section 7's minimum close): only the Player and gates lines may be missing, and only when a stop ate the
  // close (it started at 7:30 or later, or the clock is past 8:00)
  const ateClose = clock && ((Number.isFinite(startM) && startM >= clock.capMin - 30) || minSince(Date.now()) >= clock.capMin);
  const minTag = has('--tag') && has('--minimum') && ateClose && failed.length > 0 && failed.every((k) => k === 'player' || k === 'gates');
  console.log('PRE-CHECK ' + NIGHT + ': ' + (!failed.length ? 'PASS' : minTag ? 'MINIMUM (' + failed.join(', ') + ' missing after a stop)' : 'FAIL ' + failed.join(', ')));
  if (failed.length && !minTag) process.exit(1);
  if (has('--tag')) {
    if (tagSha) { console.log('close-check --tag: u3-' + NIGHT + ' already exists -> ' + tagSha + (tagSha === head ? ' (HEAD)' : ' (NOT HEAD ' + head + ': a tag is never moved)')); process.exit(tagSha === head ? 0 : 1); }
    git('tag', 'u3-' + NIGHT);
    console.log('close-check --tag: u3-' + NIGHT + ' -> ' + git('rev-parse', '--short', 'u3-' + NIGHT + '^{commit}') + ' (HEAD)');
  }
  process.exit(0);
}
const stopEvidence = (Number.isFinite(startM) && clock && startM >= clock.capMin - 30) || (clock && stopM >= clock.capMin);
const minOk = !!(stopEvidence && clockMin && filesMin && tagSha && R.running[0] && R.slate[0] && R.row[0]);
if (!failed.length) console.log('CLOSE-CHECK ' + NIGHT + ': PASS');
else if (minOk) { console.log('failed: ' + failed.join(', ') + ' (a stop ate the close: ' + (clock && stopM >= clock.capMin ? 'the clock stopped at ' + hm(stopM) : 'the close started at ' + hm(startM)) + ')'); console.log('CLOSE-CHECK ' + NIGHT + ': MINIMUM PASS'); }
else console.log('CLOSE-CHECK ' + NIGHT + ': FAIL ' + failed.join(', '));
process.exit(failed.length && !minOk ? 1 : 0);

function procList() {
  const f = arg('--procs', null);
  if (f) return JSON.parse(fs.readFileSync(path.resolve(f), 'utf8'));
  const ps = "$ProgressPreference = 'SilentlyContinue'; Get-CimInstance Win32_Process | ForEach-Object { [pscustomobject]@{ p = $_.ProcessId; pp = $_.ParentProcessId; n = $_.Name; c = $_.CommandLine; e = $_.ExecutablePath; t = $(if ($_.CreationDate) { $_.CreationDate.ToUniversalTime().ToString('o') } else { '' }) } } | ConvertTo-Json -Compress";
  const exe = path.join(process.env.SystemRoot || 'C:\\Windows', 'System32', 'WindowsPowerShell', 'v1.0', 'powershell.exe');
  const out = execFileSync(exe, ['-NoProfile', '-NonInteractive', '-EncodedCommand', Buffer.from(ps, 'utf16le').toString('base64')], { encoding: 'utf8', maxBuffer: 256 << 20, stdio: ['ignore', 'pipe', 'pipe'] });
  const j = JSON.parse(out); return Array.isArray(j) ? j : [j];
}
function running() {
  let list;
  try { list = procList(); } catch (e) { return [false, 'the process list failed: ' + String(e.message).split('\n')[0]]; }
  if (!clock) return [false, 'no clock: cannot tell this night\'s processes'];
  const byPid = new Map(list.map((p) => [p.p, p]));
  const mine = new Set(); for (let cur = process.pid; byPid.has(cur) && !mine.has(cur); cur = byPid.get(cur).pp) mine.add(cur);
  const rootWin = ROOT.replace(/\//g, '\\').toLowerCase();
  const counted = [], ignored = [];
  for (const p of list) {
    if (mine.has(p.p)) continue;
    const n = String(p.n || '').toLowerCase(), c = String(p.c || '').replace(/\//g, '\\').toLowerCase(), e = String(p.e || '').replace(/\//g, '\\').toLowerCase();
    let kind = null;
    if (/^(dotnet|simharness|coreharness)\.exe$/.test(n) && /(tools|temp)\\(simharness|coreharness)/.test(c + ' ' + e)) kind = 'harness';
    else if (n === 'skygrind.exe' && e.startsWith(rootWin + '\\builds\\' + NIGHT + '-')) kind = 'player';
    else if (n === 'bash.exe' && /tools\\night\\/.test(c)) kind = 'script';
    if (!kind) continue;
    const tag = kind + ' PID ' + p.p + ' (' + String(p.c || p.e || n).slice(0, 90) + ')';
    if (!(p.t && Date.parse(p.t) >= clock.t0 * 1000)) { ignored.push(tag + ': started before t0'); continue; }
    const parent = byPid.get(p.pp);
    if (kind === 'player' && parent && /^explorer\.exe$/i.test(parent.n || '')) { ignored.push(tag + ': started from Explorer (a person)'); continue; }
    counted.push(tag);
  }
  return [counted.length === 0, (counted.length ? 'running: ' + counted.join('; ') : 'none of this night\'s jobs running') + (ignored.length ? ' | ignored: ' + ignored.join('; ') : '')];
}

function selftest() {
  const me = __filename, slateTool = path.join(__dirname, 'n68-slate.cjs');
  const nowS = Math.floor(Date.now() / 1000), t0 = nowS - (7 * 60 + 39) * 60;   // the close commit "now" is 7:39 since t0
  const iso = (m) => new Date((t0 + m * 60) * 1000).toISOString();
  const str = (m) => new Date((t0 + m * 60) * 1000).toString();
  const W = (d, f, s) => { fs.mkdirSync(path.dirname(path.join(d, f)), { recursive: true }); fs.writeFileSync(path.join(d, f), s); };
  const G = (d, env, ...a) => execFileSync('git', ['-c', 'user.name=selftest', '-c', 'user.email=selftest@example.invalid', '-c', 'core.autocrlf=false', ...a], { cwd: d, encoding: 'utf8', env: Object.assign({}, process.env, env || {}) });
  const SUM = (sides) => sides.map((s) => '== ' + s + ' 07:10:00 (map x, scenario verbs37)\n[reel] verbs37: total 27/27\n[reel] verbs37: verbs37: PASS: total 27/27 on x; RED rows []\n').join('');
  const GUARDQ = '"SKYGRIND guard blocked this (disk): 8.3 GB free on the project drive, under 12 GB"';
  function plant(o) {
    o = Object.assign({ stopMin: 460, stopped: true, tag: true, start: true, procs: [], slate: 'ok', gates: 'green', player: 'ok', rowReport: 'match', slotsLate: false, afterTag: false, blocked: '' }, o);
    const d = fs.mkdtempSync(path.join(os.tmpdir(), 'n68cc-'));
    G(d, null, 'init', '-q');
    // the frozen files, committed an hour before the close commit (before the close start at 7:05)
    const cfg = '{"design":{"n65":{"foundry65":{"enabled":' + (o.rowOff ? 'false' : 'true') + '}}' + (o.multi ? ',"n67":{"iceline67":{"enabled":false,"levelFrom":"n52-iceline","levelTo":"n67-iceline"}}' : '') + '}}\n';
    if (o.multi && o.multi !== 'noslots') W(d, 'Design/levels/n67/iceline/slots67.json', '{"slots":{"g7":{"use":"old"}}}\n');   // FORGE67b: a second level row
    W(d, 'Design/unity-config.json', cfg); W(d, 'Assets/StreamingAssets/Design/unity-config.json', cfg);
    W(d, 'Design/levels/n65/slots65.json', '{"slots":{"a2":{"use":"new"},"a3":{"use":"old"}}}\n');
    const at = (m) => ({ GIT_COMMITTER_DATE: iso(m), GIT_AUTHOR_DATE: iso(m) });
    if (o.coreEarly) W(d, 'docs/memory/CORE.md', '# c\n');   // FORGE67 red team 7: CORE.md last committed before the close
    G(d, null, 'add', '--', 'Design', 'Assets', ...(o.coreEarly ? ['docs/memory/CORE.md'] : []));
    G(d, at(o.slotsLate ? 437 : 399), 'commit', '-q', '-m', 'frozen');
    // the close files
    W(d, 'NIGHT68-REPORT.md', '# r\n\n## 2. play card\n\n## 8. close\n' + (o.multi === true || o.multi === 'noslots' ? 'ROW: n67.iceline67 enabled=false; SLOTS: g7=old\n' : '') + (o.rowReport === 'match' ? 'ROW: n65.foundry65 enabled=' + (o.rowOff ? 'false' : 'true') + '; SLOTS: a2=new a3=old\n' : 'ROW: n65.foundry65 enabled=false; SLOTS: a2=old a3=old\n'));
    W(d, 'NIGHT68-LOG.md', '# log\n' + (o.blocked ? '\n## Blocked\n' + o.blocked + '\n' : ''));
    W(d, 'HANDOFF.md', '# h\n'); if (!o.coreEarly) W(d, 'docs/memory/CORE.md', '# c\n');
    G(d, null, 'add', '--', 'NIGHT68-REPORT.md', 'NIGHT68-LOG.md', 'HANDOFF.md', 'docs/memory/CORE.md');
    G(d, at(o.closeMin || 459), 'commit', '-q', '-m', 'close');
    if (o.tag) G(d, null, 'tag', 'u3-n68');
    if (o.afterTag) { W(d, 'HANDOFF.md', '# h2\n'); G(d, null, 'add', '--', 'HANDOFF.md'); G(d, at(459), 'commit', '-q', '-m', 'after the tag'); }
    // the logs (untracked here, as Logs/ is gitignored in the repo)
    W(d, 'Logs/clock.json', JSON.stringify(Object.assign({ night: 'n68', freezeMin: 435, capMin: 480, staleMin: 720, closeTags: ['n68-freeze'], t0, t0Text: str(0), stopped: o.stopped }, o.stopped ? { stoppedText: str(o.stopMin) } : {})));
    if (o.start) W(d, 'Logs/n68/close-start.txt', iso(o.startMin || 425) + ' ' + Math.floor((o.startMin || 425) / 60) + ':' + String((o.startMin || 425) % 60).padStart(2, '0') + '\n');
    W(d, 'Tools/night/n68-slate.cjs', fs.readFileSync(slateTool, 'utf8'));
    const sl = (id, state, passes, visible, verdict) => ({ id, title: 't', why: 'w', owner: 'lead', row: 'r', red: 'r', doneWhen: ['d'], visible, passes, state, verdict });
    W(d, 'Logs/n68/verdict-T1.md', 'T1: PASS\n');
    const slices = [sl('T1', 'pass', true, o.slate === 'visible' ? 'none (tools)' : 'n/a (tools)', 'Logs/n68/verdict-T1.md'), sl('a2', o.slate === 'progress' ? 'building' : o.slate === 'todo' ? 'todo' : 'short', false, 'the room', null)];
    W(d, 'Logs/n68/slate.json', JSON.stringify({ slices }));
    if (o.gates === 'green') W(d, 'Logs/n68/gates-flow-close.txt', 'gate config: ok\ngates: all GREEN 07:20:00\n');
    if (o.gates === 'fail') W(d, 'Logs/n68/gates-flow-close.txt', 'gates: all GREEN 05:00:00\nGATE FAIL: fingerprint --off67\ngates: FAIL\n');
    if (o.player === 'ok' || o.player === 'no-vn') { W(d, 'Logs/feel/n68-freeze/summary47.txt', SUM(o.player === 'ok' ? ['vf', 'vn', 'vi', 'vk', 'vs', 'vp'] : ['vf', 'vi', 'vk', 'vs', 'vp'])); W(d, 'Logs/player/n56-smoke-steal-n68-freeze.log', '[smoke] 232.0s DONE: all steps passed\n'); }
    W(d, 'procs.json', JSON.stringify(o.procs));
    return d;
  }
  const run = (d, extra) => { const r = spawnSync(process.execPath, [me, '--root', d, '--procs', path.join(d, 'procs.json'), ...(extra || [])], { encoding: 'utf8' }); const ls = String(r.stdout || '').trim().split('\n'); return { last: ls[ls.length - 1] || '', out: r.stdout + (r.stderr || ''), code: r.status }; };
  const rootWin = (d) => d.replace(/\//g, '\\');
  const proc = (d, p, n, c, m, pp) => ({ p, pp: pp || 1, n, c, e: n === 'SKYGRIND.exe' ? rootWin(d) + '\\Builds\\n68-play-latest\\SKYGRIND.exe' : 'C:\\Program Files\\dotnet\\dotnet.exe', t: iso(m) });
  const cases = [
    ['a correct close passes', {}, null, (r) => r.last === 'CLOSE-CHECK n68: PASS'],
    ['--pre passes before the tag and the clock stop', { tag: false, stopped: false }, ['--pre'], (r) => r.last === 'PRE-CHECK n68: PASS'],
    ['--tag creates the tag on HEAD after --pre', { tag: false, stopped: false }, ['--tag'], (r) => /^close-check --tag: u3-n68 -> [0-9a-f]+ \(HEAD\)$/.test(r.last)],
    ['--tag refuses when --pre fails (a slice in progress)', { tag: false, stopped: false, slate: 'progress' }, ['--tag'], (r) => r.last === 'PRE-CHECK n68: FAIL slate' && r.code === 1],
    ['--tag --minimum tags a close started at 7:30 or later without the Player lines', { tag: false, stopped: false, startMin: 455, player: 'none', gates: 'none' }, ['--tag', '--minimum'], (r) => /^close-check --tag: u3-n68 -> [0-9a-f]+ \(HEAD\)$/.test(r.last)],
    ['--tag --minimum refuses a minimum close whose CORE.md was not re-committed after --start (FORGE67 red team 7)', { tag: false, stopped: false, startMin: 455, player: 'none', gates: 'none', coreEarly: true }, ['--tag', '--minimum'], (r) => r.last === 'PRE-CHECK n68: FAIL files, player, gates' && r.code === 1],
    ['--tag --minimum refuses an on-time close without the Player lines', { tag: false, stopped: false, player: 'none', gates: 'none' }, ['--tag', '--minimum'], (r) => r.last === 'PRE-CHECK n68: FAIL player, gates' && r.code === 1],
    ['a stop past 8:00 is the minimum close', { stopMin: 490 }, null, (r) => r.last === 'CLOSE-CHECK n68: MINIMUM PASS' && /^failed: clock /m.test(r.out)],
    ['a close started at 7:30 or later is the minimum close (the Player lines missing)', { startMin: 455, player: 'none', gates: 'none' }, null, (r) => r.last === 'CLOSE-CHECK n68: MINIMUM PASS'],
    ['an early clock stop fails', { stopMin: 445, closeMin: 444 }, null, (r) => r.last === 'CLOSE-CHECK n68: FAIL clock'],
    ['a late close without a stop is no minimum (the Player lines skipped)', { player: 'none', gates: 'none' }, null, (r) => r.last === 'CLOSE-CHECK n68: FAIL player, gates'],
    ['a harness started since t0 fails running', { procs: null }, null, (r) => r.last === 'CLOSE-CHECK n68: FAIL running'],
    ['old harnesses and a Player started from Explorer are ignored', { procs: 'ignored' }, null, (r) => r.last === 'CLOSE-CHECK n68: PASS' && /ignored: /.test(r.out)],
    ['a night script since t0 fails running', { procs: 'script' }, null, (r) => r.last === 'CLOSE-CHECK n68: FAIL running'],
    ['a slice in progress fails, even after a stop past 8:00', { slate: 'progress', stopMin: 490 }, null, (r) => r.last === 'CLOSE-CHECK n68: FAIL clock, slate'],
    ['a todo slice fails the slate', { slate: 'todo' }, null, (r) => r.last === 'CLOSE-CHECK n68: FAIL slate'],
    ['a player-facing tool slice makes the slate malformed', { slate: 'visible' }, null, (r) => r.last === 'CLOSE-CHECK n68: FAIL slate'],
    ['a FAIL gates file fails, Blocked words or not', { gates: 'fail', blocked: '- ' + GUARDQ + ' the gates' }, null, (r) => r.last === 'CLOSE-CHECK n68: FAIL gates'],
    ['"gates" under Blocked without the guard\'s words fails', { gates: 'none', blocked: '- the gates were not run' }, null, (r) => r.last === 'CLOSE-CHECK n68: FAIL gates'],
    ['the guard\'s refusal quoted under Blocked passes the Player and the gates', { gates: 'none', player: 'none', blocked: '- (b) the freeze build: refused: ' + GUARDQ + '\n- (c) verbs37: the same\n- (d) the smoke: the same\n- (e) the gates: the same' }, null, (r) => r.last === 'CLOSE-CHECK n68: PASS'],
    ['the row OFF: the five shipped sides pass without n65-foundry (FORGE67)', { rowOff: true, player: 'no-vn' }, null, (r) => r.last === 'CLOSE-CHECK n68: PASS'],
    ['a verbs37 side missing fails the Player', { player: 'no-vn' }, null, (r) => r.last === 'CLOSE-CHECK n68: FAIL player'],
    ['a ROW line unlike the config fails', { rowReport: 'mismatch' }, null, (r) => r.last === 'CLOSE-CHECK n68: FAIL row'],
    ['two level rows found in the config, both ROW lines in the report, pass (FORGE67b)', { multi: true }, null, (r) => r.last === 'CLOSE-CHECK n68: PASS' && /n67\.iceline67/.test(r.out)],
    ['two level rows, one ROW line missing from the report, fails (FORGE67b)', { multi: 'missing' }, null, (r) => r.last === 'CLOSE-CHECK n68: FAIL row'],
    ['an n67 row in the config without its slots file fails (FORGE67b red team 1)', { multi: 'noslots' }, null, (r) => r.last === 'CLOSE-CHECK n68: FAIL frozen, row'],
    ['slots committed in the close fail frozen', { slotsLate: true }, null, (r) => r.last === 'CLOSE-CHECK n68: FAIL frozen'],
    ['a commit after the tag fails the tag', { afterTag: true }, null, (r) => r.last === 'CLOSE-CHECK n68: FAIL tag'],
  ];
  let bad = 0;
  for (const [name, o, extra, ok] of cases) {
    const d = plant(Object.assign({}, o, { procs: [] }));
    if (o.procs === null) fs.writeFileSync(path.join(d, 'procs.json'), JSON.stringify([proc(d, 4242, 'dotnet.exe', 'dotnet run --project Tools/SimHarness -c Release -- n46flow', 300)]));
    if (o.procs === 'ignored') fs.writeFileSync(path.join(d, 'procs.json'), JSON.stringify([proc(d, 4243, 'dotnet.exe', 'dotnet Temp/SimHarness/bin/Release/net10.0/SimHarness.dll n46flow', -30), { p: 77, pp: 4, n: 'explorer.exe', c: 'C:\\Windows\\Explorer.EXE', e: 'C:\\Windows\\explorer.exe', t: iso(-600) }, proc(d, 4244, 'SKYGRIND.exe', '"SKYGRIND.exe"', 455, 77), proc(d, 4245, 'dotnet.exe', 'dotnet C:\\x\\VBCSCompiler.dll', 300)]));
    if (o.procs === 'script') fs.writeFileSync(path.join(d, 'procs.json'), JSON.stringify([{ p: 4246, pp: 1, n: 'bash.exe', c: '"C:\\Program Files\\Git\\usr\\bin\\bash.exe" Tools/night/n68-prune.sh n68-r3 --loop', e: 'C:\\Program Files\\Git\\usr\\bin\\bash.exe', t: iso(320) }]));
    const r = run(d, extra);
    const pass = ok(r);
    if (!pass) bad++;
    console.log((pass ? 'ok   ' : 'BAD  ') + name + ' -> ' + r.last);
    if (!pass) console.log(r.out.split('\n').map((l) => '       ' + l).join('\n'));
    fs.rmSync(d, { recursive: true, force: true });
  }
  console.log(bad ? 'SELFTEST FAIL (' + bad + ' of ' + cases.length + ')' : 'SELFTEST PASS (' + cases.length + ' cases)');
  process.exit(bad ? 1 : 0);
}
