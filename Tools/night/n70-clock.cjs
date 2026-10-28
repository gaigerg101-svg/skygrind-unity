#!/usr/bin/env node
// NIGHT70 clock (FORGE66, 2026-10-23, for GG's "next detailed night prompt that will go for 8 hours"; NIGHT65's clock renamed): the run's t0 and its phases, kept in
// Logs/clock.json and read by the guard hook's `clock` rule (.claude/hooks/guard.cjs), so the cap holds even when a turn forgets the time.
//   node Tools/night/n70-clock.cjs start    writes Logs/clock.json from now (refuses while a fresh, running clock exists)
//   node Tools/night/n70-clock.cjs          prints the date, h:mm / 8:00, the phase, what the guard allows now and C:'s free GB (every turn)
//   node Tools/night/n70-clock.cjs stop     marks the clock stopped (the close's last step; the guard ignores a stopped clock)
// Phases: BUILD (0:00-7:15) anything; FREEZE (7:15-8:00: the close) only the close's heavy steps (a build, reel set or smoke whose tag is n70-freeze),
// no new agent, no test run; CAP (8:00 on) no heavy step and no agent starts. The guard ignores a clock older than 12 hours (stale: FORGE66, the code scout).
const fs = require('fs');
const path = require('path');
const ROOT = path.resolve(__dirname, '..', '..');
const FILE = path.join(ROOT, 'Logs', 'clock.json');
const DEF = { night: 'n70', freezeMin: 435, capMin: 480, staleMin: 720, closeTags: ['n70-freeze'] };   // FORGE64 red team 9: stale only after 10 h, so a long usage stop still reads CAP
const hm = m => Math.floor(m / 60) + ':' + String(Math.floor(m % 60)).padStart(2, '0');
const read = () => { try { return JSON.parse(fs.readFileSync(FILE, 'utf8')); } catch (e) { return null; } };
const nowS = () => Math.floor(Date.now() / 1000);
const cmd = process.argv[2] || 'status';

function phase(c, min) {
  if (!c || c.stopped || min >= c.staleMin) return 'NONE';
  if (min >= c.capMin) return 'CAP';
  if (min >= c.freezeMin) return 'FREEZE';
  return 'BUILD';
}

if (cmd === 'start') {
  const old = read();
  if (old && !old.stopped && (nowS() - old.t0) / 60 < (old.staleMin || DEF.staleMin)) {
    console.log('clock start REFUSED: a running clock exists (t0 ' + old.t0Text + ', ' + hm((nowS() - old.t0) / 60) + ' ago). Resume it; never restart t0.');
    process.exit(1);
  }
  const t0 = nowS();
  const c = Object.assign({}, DEF, { t0, t0Text: new Date(t0 * 1000).toString(), stopped: false });
  fs.mkdirSync(path.dirname(FILE), { recursive: true });
  fs.writeFileSync(FILE, JSON.stringify(c, null, 1) + '\n');
  console.log('clock started: t0 ' + c.t0Text + ' (epoch ' + t0 + '); FREEZE at ' + hm(c.freezeMin) + ', CAP at ' + hm(c.capMin) + ' (Logs/clock.json)');
  process.exit(0);
}

const c = read();
if (cmd === 'stop') {
  if (!c) { console.log('clock stop: no Logs/clock.json'); process.exit(1); }
  c.stopped = true; c.stoppedText = new Date().toString();
  fs.writeFileSync(FILE, JSON.stringify(c, null, 1) + '\n');
  console.log('clock stopped at ' + hm((nowS() - c.t0) / 60) + ' since t0 (' + c.stoppedText + ')');
  process.exit(0);
}

console.log(new Date().toString());
if (!c) { console.log('clock: none (run `node Tools/night/n70-clock.cjs start` at the first bearings)'); process.exit(1); }
const min = (nowS() - c.t0) / 60;
const p = phase(c, min);
const allows = {
  BUILD: 'anything (builds, reels, tests, agents)',
  FREEZE: 'only the close: a build, reel set or smoke tagged ' + c.closeTags.join(' / ') + '; no new agent, no test run',
  CAP: 'no build, reel set, smoke, test run or agent; commit what is done, log the rest under Blocked, end',
  NONE: 'the guard ignores this clock (' + (c.stopped ? 'stopped' : 'stale') + ')',
}[p];
console.log('clock ' + c.night + ': ' + hm(min) + ' / ' + hm(c.capMin) + ' since t0 ' + c.t0Text.slice(16, 24) + '; phase ' + p + ' (FREEZE ' + hm(c.freezeMin) + ', CAP ' + hm(c.capMin) + '): the guard allows ' + allows);
// the disk beside the clock (the red team 2h, finding 5: a background disk watcher would keep the /goal from being evaluated all run)
try { const s = fs.statfsSync(ROOT); console.log('disk: C: ' + (s.bavail * s.bsize / 2 ** 30).toFixed(1) + ' GB free (the guard refuses a build or reel set under 12)'); } catch (e) { console.log('disk: not read (' + e.message + ')'); }
