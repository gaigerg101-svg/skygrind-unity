// FORGE69 (2026-10-25, the lead; the forge experiment: "the next-action printer"): read-only. Prints, from the files alone, the night's clock,
// each slate room's GATE pieces (the ROOM list: verb, bar, the card, the latest round's M2 VERB line, the lint, the decoration checker, the
// fresh set, the level review, the room file unchanged since the round, the critic's verdict) with the first one missing, and the next
// steps. NIGHT68's lead missed pieces by hand (a pick without a line, reds beside a newer harness, a GATE with no scheduled level review);
// this prints them instead. It decides nothing and writes nothing. A window-aware line says how long is left before the usage window's likely
// stop (about 2:20 after t0 when the night starts in a fresh window: NIGHT66-68 measured 2:20 / 2:25 / 2:24).
//   node Tools/night/n70-next.cjs                 the table and the next steps
//   node Tools/night/n70-next.cjs --root <dir>    another checkout (the self-test)
//   node Tools/night/n70-next.cjs --selftest      a planted slate: every missing piece named, a FAILed fresh set read as missing, all present
'use strict';
const fs = require('fs');
const path = require('path');
const crypto = require('crypto');
const THROWS = require('./n70-throws-sum.cjs');   // NIGHT70 Z1: the throw chart's one reader (M3, DECIDE70 D-3)
const { execFileSync } = require('child_process');
const argv = process.argv.slice(2);
const arg = (k, d) => { const i = argv.indexOf(k); return i >= 0 && i + 1 < argv.length ? argv[i + 1] : d; };
const ROOT = path.resolve(arg('--root', path.join(__dirname, '..', '..')));
const NIGHT = 'n70';
// FORGE70: an 8-hour night is two usage windows (NIGHT66-69 measured 2:10-2:25 of work per fresh window; the window resets 5:00 after
// its first use): work 1 to 2:20, the stop (only detached chains compute) to 5:00, work 2 to 7:20, the close. A stop's real time is in
// Logs/n70/stops.txt when the lead writes it; this is the plan, not a measurement.
const STOP_MIN = 140, RESUME_MIN = 300, STOP2_MIN = 440;
const rd = (p) => { try { return fs.readFileSync(path.join(ROOT, p), 'utf8').replace(/\r\n/g, '\n'); } catch (e) { return null; } };
const exists = (p) => fs.existsSync(path.join(ROOT, p));
const sha = (t) => crypto.createHash('sha256').update(String(t).replace(/\r\n/g, '\n')).digest('hex');
const hm = (m) => Math.floor(m / 60) + ':' + String(Math.floor(m % 60)).padStart(2, '0');

function run() {
  const out = [];
  const clock = JSON.parse(rd('Logs/clock.json') || 'null');
  if (clock && clock.night === NIGHT && !clock.stopped) {
    const m = (Date.now() / 1000 - clock.t0) / 60;
    out.push('clock ' + NIGHT + ': ' + hm(m) + ' / ' + hm(clock.capMin) + ' (FREEZE ' + hm(clock.freezeMin) + ', the close starts at ' + hm(clock.capMin - 60) + ')');
    if (m >= STOP_MIN && m < STOP2_MIN) out.push(m < RESUME_MIN ? 'window: inside the planned stop (2:20-5:00): if you are reading this, the window resumed early: work 2 starts now (agents resumed by id from STATE.md)' : 'window 2: the likely second stop is at about ' + hm(STOP2_MIN) + ' (' + hm(STOP2_MIN - m) + ' left): critics and agents before it; the close at 7:00');
    else out.push(m < STOP_MIN ? 'window: the likely usage stop is at about ' + hm(STOP_MIN) + ' (' + hm(STOP_MIN - m) + ' left): critics, reviews and agents only before it; after it only files and docs' : 'window: past the likely stop (' + hm(STOP_MIN) + '): if the session is back after CAP, only the minimum close (files and docs) remains');
  } else out.push('clock: none running for ' + NIGHT);
  const slate = JSON.parse(rd('Logs/' + NIGHT + '/slate.json') || 'null');
  if (!slate) { out.push('no Logs/' + NIGHT + '/slate.json'); return out; }
  const rooms = slate.slices.filter((s) => s.level);
  const tools = slate.slices.filter((s) => !s.level && !['LV', 'PC'].includes(s.id));
  const logs = path.join(ROOT, 'Logs', NIGHT);
  const latest = (L, id) => {   // the newest round folder of level L that holds a verbs-skip.txt line for the room
    let best = null;
    if (!fs.existsSync(logs)) return null;
    for (const f of fs.readdirSync(logs)) {
      if (!f.endsWith('-' + L) || !/^(r\d|alone)/.test(f)) continue;
      const v = rd('Logs/' + NIGHT + '/' + f + '/verbs-skip.txt'); if (!v || !new RegExp('room ' + id + ',').test(v)) continue;
      const t = fs.statSync(path.join(logs, f, 'verbs-skip.txt')).mtimeMs;
      if (!best || t > best.t) best = { f, t };
    }
    return best && best.f;
  };
  out.push('', 'tools: ' + (tools.map((s) => s.id + ' ' + s.state).join(', ') || '-'), '');
  const missingAll = [];
  for (const s of rooms) {
    const id = s.id, L = s.level, P = [];
    const ok = (name, good, why) => { P.push([name, !!good, why || '']); };
    ok('verb', s.verb, 'node Tools/night/' + NIGHT + '-slate-set.cjs ' + id + ' verb=<the card verb>');
    ok('bar', s.mainBar != null, 'the reds then ' + NIGHT + '-red-bars.cjs --write');
    ok('red', exists('Logs/' + NIGHT + '/red-' + (L === 'kiln' ? 'kiln' : L) + '/' + id + '/skip.txt'), 'cp -r Logs/n68/red-' + L + ' Logs/' + NIGHT + '/red-' + L + ' (the GATE pack and table read only Logs/' + NIGHT + '/red-<L>)');
    ok('card', exists('Research/2026-10-24-wave67/cards/' + id + '.md'), 'the builder commits it; the lead picks it');
    const rnd = latest(L, id);
    ok('round', rnd, 'run a round with this room (alone: SLOTS=' + id + ')');
    const dir = rnd ? 'Logs/' + NIGHT + '/' + rnd : null;
    const line = dir && (rd(dir + '/verbs-skip.txt') || '').split('\n').find((l) => /^n65verb skip: M2 VERB/.test(l) && new RegExp('room ' + id + ',').test(l));
    ok('M2', line && /^n65verb skip: M2 VERB PASS/.test(line), line ? line.slice(15, 75) : 'no M2 VERB line');
    const area = dir && rd(dir + '/area-' + id + '.txt'); ok('M1 lint', area && /n\d+-area: PASS/.test(area), area ? area.trim().split('\n').pop().slice(0, 50) : 'no area file');
    const deco = dir && rd(dir + '/deco-' + id + '.txt'); ok('M4 deco', deco && /n\d+-deco: PASS/.test(deco), deco ? '' : 'no deco file');
    const patch = dir && rd(dir + '/patch-' + id + '.txt'); ok('patch', patch && /^n\d+-patch-share: PASS/m.test(patch), patch ? patch.trim().split('\n').pop().slice(0, 60) : 'no patch share (<= 25 % of the shipped pieces kept)');
    // FORGE70: a room-check item that FAILs in the round but PASSes in the red is a GUARD lost (NIGHT69: kb3 R5 / I4, g8 I2)
    const rcF = (t) => new Set(((t || '').match(/^\s*FAIL (\S+)/gm) || []).map((x) => x.trim().split(/\s+/)[1]));
    const rcNow = dir && rd(dir + '/' + id + '/room-check.txt'), rcRed = rd('Logs/' + NIGHT + '/red-' + L + '/' + id + '/room-check.txt');
    const lost = rcNow ? [...rcF(rcNow)].filter((k) => !rcF(rcRed).has(k)) : ['no room-check.txt'];
    ok('room check', rcNow && !lost.length, lost.length ? 'FAIL here, PASS in the red: ' + lost.join(' ') : '');
    // NIGHT70 Z1 (slate Z1; DECIDE70 D-3): a launch room's M3 is its round's 20-seed throw chart (<round>/throws-<id>.txt), >= 10 / 20
    // candidates, re-counted by n70-throws-sum.cjs (no chart: 'no throw chart'); every other verb's M3 as before (the pilot's line in verbs.md)
    if (/^launch/.test(s.verb || '')) { const cw = (dir || 'Logs/' + NIGHT + '/<round>') + '/throws-' + id + '.txt'; const v3 = THROWS.m3(dir ? rd(cw) : null, id, cw); ok('M3', v3.pass, v3.why); }
    else { const vm = dir && (rd(dir + '/verbs.md') || ''); ok('M3', vm && new RegExp('M3: ' + id + ' PASS').test(vm), 'the verb pilot (a pit only for a launched body: Y2)'); }
    const fresh = dir && rd(dir + '/fresh-' + id + '.txt');   // the verdict line only: the script's summary line quotes "M2 FRESH PASS" in its legend
    const freshLine = fresh && fresh.split('\n').find((l) => /^n65verb skip: M2 FRESH (PASS|FAIL|UNSURE)/.test(l));
    ok('fresh', freshLine && /^n65verb skip: M2 FRESH PASS/.test(freshLine), freshLine ? freshLine.slice(15, 75) : 'no fresh set (seeds 21-30)');
    const rv = rd('Logs/' + NIGHT + '/' + (rnd ? rnd.split('-')[0] : 'r0') + '/review-level.md'); ok('review', rv && new RegExp('^## ' + id + '\\b', 'm').test(rv), 'a fresh reviewer, the moment the round ends');
    let same = false; if (dir) { const rec = rd(dir + '/room-files/' + id + '.json'), cur = rd('Design/levels/n67/' + L + '/rooms/' + id + '.json'); same = rec && cur && sha(rec) === sha(cur); }
    ok('unchanged', dir ? same : false, 'the room file differs from the round\'s record: a new round');
    const pk = rd('Logs/' + NIGHT + '/gate-' + id + '-pack.txt'); ok('pack', pk && !/MISSING/.test(pk), pk ? (pk.split('\n').find((l) => /MISSING/.test(l)) || '').slice(0, 90) : 'run the GATE pack AFTER the level review: L67=' + L + ' bash Tools/night/' + NIGHT + '-gate-pack.sh ' + id + ' r<n>-' + L + ' --fresh <the fresh file> > Logs/' + NIGHT + '/gate-' + id + '-pack.txt');
    const v = rd('Logs/' + NIGHT + '/verdict-' + id + '.md'); ok('critic', v && new RegExp('^' + id + ': PASS').test(v), v ? v.split('\n')[0].slice(0, 60) : 'no verdict file');
    const miss = P.filter((p) => !p[1]);
    out.push(id.padEnd(5) + (s.verb || '-').padEnd(7) + ' ' + s.state.padEnd(10) + P.map((p) => (p[1] ? 'ok' : '--') + ' ' + p[0]).join(' | '));
    if (miss.length && s.state !== 'short' && s.state !== 'pass') missingAll.push(id + ': ' + miss[0][0] + ' (' + miss[0][2] + ')');
  }
  out.push('', 'next (the first missing piece of each open room):');
  if (!missingAll.length) out.push('  every open room holds all its pieces: the batched critic, then the slots');
  for (const m of missingAll.slice(0, 7)) out.push('  - ' + m);
  const stale = tools.filter((t) => t.state === 'building' || t.state === 'critic');
  if (stale.length) out.push('  - tools in progress: ' + stale.map((t) => t.id).join(', '));
  return out;
}

if (argv.includes('--selftest')) {
  const os = require('os');
  const d = fs.mkdtempSync(path.join(os.tmpdir(), 'n70next-'));
  const W = (p, t) => { fs.mkdirSync(path.dirname(path.join(d, p)), { recursive: true }); fs.writeFileSync(path.join(d, p), t); };
  W('Logs/n70/slate.json', JSON.stringify({ slices: [{ id: 'X1', level: 'kiln', verb: null, mainBar: null, state: 'building' }, { id: 'T1', state: 'building' }] }));
  const bare = execFileSync(process.execPath, [__filename, '--root', d]).toString();
  const a = /X1 .*-- verb \| -- bar \| -- red \| -- card \| -- round/.test(bare) && /X1: verb/.test(bare);
  W('Research/2026-10-24-wave67/cards/X1.md', 'c');
  W('Design/levels/n67/kiln/rooms/X1.json', '{"id":"X1"}');
  W('Logs/n70/slate.json', JSON.stringify({ slices: [{ id: 'X1', level: 'kiln', verb: 'ride', mainBar: 1.25, state: 'measuring' }, { id: 'Y1', level: 'kiln', verb: 'ride', mainBar: 1.25, state: 'measuring' }] }));
  W('Logs/n70/r2-kiln/verbs-skip.txt', 'n65verb skip: M2 VERB PASS | room X1, verb ride, pooled ratio 1.4\nn65verb skip: M2 VERB FAIL | room Y1, verb ride, pooled ratio 0.9\n');
  W('Logs/n70/r2-kiln/area-X1.txt', 'n70-area: PASS X1 (6/6)\n'); W('Logs/n70/r2-kiln/deco-X1.txt', 'n70-deco: PASS | room X1\n');
  W('Logs/n70/r2-kiln/fresh-X1.txt', 'n65verb skip: M2 FRESH PASS | room X1\nn65-verbs skip on x (X1): GREEN 1 (skip v2: GREEN = M2 VERB PASS, or M2 FRESH PASS on seeds 21+)\n');
  W('Logs/n70/r2-kiln/fresh-Y1.txt', 'n65verb skip: M2 FRESH FAIL | room Y1\nn65-verbs skip on x (Y1): GREEN 0 (skip v2: GREEN = M2 VERB PASS, or M2 FRESH PASS on seeds 21+)\n');
  W('Logs/n70/r2/review-level.md', '## X1\n');
  W('Logs/n70/r2-kiln/room-files/X1.json', '{"id":"X1"}'); W('Logs/n70/verdict-X1.md', 'X1: PASS\n');
  W('Logs/n70/red-kiln/X1/skip.txt', 'red'); W('Logs/n70/r2-kiln/patch-X1.txt', 'row\nn69-patch-share: PASS X1 (10 % kept)\n');
  W('Logs/n70/r2-kiln/X1/room-check.txt', '  PASS R1 x\n  FAIL R5 foothold x\nROOM X1: FAIL R5 foothold\n'); W('Logs/n70/red-kiln/X1/room-check.txt', '  FAIL R5 foothold x\n');
  W('Logs/n70/r2-kiln/verbs.md', 'n65-verbs ride M3: X1 PASS (x)\n'); W('Logs/n70/gate-X1-pack.txt', 'n70-gate-pack X1 r2-kiln: complete\n');
  W('Logs/n70/gate-Y1-pack.txt', 'n70-gate-pack Y1 r2-kiln: MISSING 1: level review: none\n');
  // NIGHT70 Z1: launch rooms W1 (a 14 / 20 chart), W2 (7 / 20), W3 (no chart), each with a round
  const chart = (room, cand, n) => { let s = ''; for (let i = 0; i < n; i++) s += 'n65verb launch: ' + (i < cand ? 'GREEN' : 'RED') + ' | room ' + room + ' | launch36 1, flight end body thrown 1.0 m (max 1.0 m), bursts 1 (hits 2, kills ' + (i < cand ? 2 : 1) + ') | launched body husk #1, in a pit no, in lava no\n'; return s + THROWS.summarize(s).line + '\n'; };
  const sl = JSON.parse(fs.readFileSync(path.join(d, 'Logs/n70/slate.json'), 'utf8')); for (const w of ['W1', 'W2', 'W3']) sl.slices.push({ id: w, level: 'spire', verb: 'launch', mainBar: 1.25, state: 'measuring' }); W('Logs/n70/slate.json', JSON.stringify(sl));
  W('Logs/n70/r2-spire/verbs-skip.txt', 'n65verb skip: M2 VERB PASS | room W1, verb launch\nn65verb skip: M2 VERB PASS | room W2, verb launch\nn65verb skip: M2 VERB PASS | room W3, verb launch\n');
  W('Logs/n70/r2-spire/throws-W1.txt', chart('W1', 14, 20)); W('Logs/n70/r2-spire/throws-W2.txt', chart('W2', 7, 20));
  const full = execFileSync(process.execPath, [__filename, '--root', d]).toString();
  const z1 = /W1 .*ok M3/.test(full) && /W2 .*-- M3/.test(full) && /W3 .*-- M3/.test(full) && THROWS.m3(null, 'W3', 'x').why.startsWith('no throw chart');
  const c = /Y1 .*-- M2 .*-- fresh/.test(full) && /Y1 .*-- pack/.test(full);   // a FAILed fresh set and a pack that says MISSING must read missing
  const b = c && z1 && /X1 .*ok verb \| ok bar \| ok red \| ok card \| ok round \| ok M2 \| ok M1 lint \| ok M4 deco \| ok patch \| ok room check \| ok M3 \| ok fresh \| ok review \| ok unchanged \| ok pack \| ok critic/.test(full) && /Y1: red/.test(full) && !/X1: /.test(full.split('next (')[1] || '');
  fs.rmSync(d, { recursive: true, force: true });
  console.log((a && b) ? 'SELFTEST PASS (3 cases: every piece missing named; a FAILed fresh set read as missing, a complete room read as complete; Z1: a launch room\'s M3 from its throw chart, 14 / 20 ok, 7 / 20 and none missing)' : 'SELFTEST FAIL a=' + a + ' b=' + b + ' z1=' + z1 + '\n' + bare + '\n' + full);
  process.exit(a && b ? 0 : 1);
} else console.log(run().join('\n'));
