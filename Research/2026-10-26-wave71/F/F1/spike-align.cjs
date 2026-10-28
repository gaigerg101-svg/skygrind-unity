// NIGHT71 F1 (builder F): the bench's interval[n] (Time.unscaledDeltaTime read in frame n's coroutine) is the length of frame n-1's
// whole cycle; update_ms[n] (SurvivalGame.UpdateMsThisFrame read in the same coroutine) is frame n's own Update. This script re-splits
// the worst-1 % frames with the PREVIOUS row's Update (the frame the interval measured) beside the bench's own pairing. Read only.
// usage: node spike-align.cjs <bench.log>
const fs = require('fs');
const base = process.argv[2];
const fr = fs.readFileSync(base + '.frames.csv', 'utf8').trim().split(/\r?\n/).slice(1).map((l) => l.split(',').map(Number));
const warm = 30;
const n = fr.length;
const iv = (i) => fr[i][1], steps = (i) => fr[i][2], stepMs = (i) => fr[i][3], upd = (i) => fr[i][4];
const ids = [];
for (let i = warm + 1; i < n; i++) ids.push(i);
const byIv = [...ids].sort((a, b) => iv(a) - iv(b));
const m = byIv.length;
const worst = byIv.slice(Math.floor(m * 0.99));
const band = byIv.slice(Math.floor(m * 0.45), Math.floor(m * 0.55));
const mean = (set, fn) => set.reduce((s, i) => s + fn(i), 0) / Math.max(1, set.length);
console.log(`file ${base}; frames ${m}; worst-1% ${worst.length}`);
const rows = [
  ['interval[n] ms (frame n-1 cycle)', (i) => iv(i)],
  ['Update[n] ms (bench pairing)', (i) => upd(i)],
  ['interval[n] - Update[n] (bench "engine")', (i) => iv(i) - upd(i)],
  ['Update[n-1] ms (the measured frame)', (i) => upd(i - 1)],
  ['sim step ms[n-1]', (i) => stepMs(i - 1)],
  ['sim steps[n-1]', (i) => steps(i - 1)],
  ['Update[n-1] - step ms[n-1] (render half + input)', (i) => upd(i - 1) - stepMs(i - 1)],
  ['interval[n] - Update[n-1] (engine, aligned)', (i) => iv(i) - upd(i - 1)],
  ['interval[n+1] ms (the frame after)', (i) => (i + 1 < n ? iv(i + 1) : 0)],
];
for (const [name, fn] of rows) console.log(`  ${name.padEnd(50)} worst ${mean(worst, fn).toFixed(3)} | median band ${mean(band, fn).toFixed(3)}`);
// share of the worst frames whose aligned Update is itself over 4 ms (a script spike) vs whose aligned engine part is over 6 ms
const scr = worst.filter((i) => upd(i - 1) > 4).length;
const eng = worst.filter((i) => iv(i) - upd(i - 1) > 6).length;
console.log(`  worst frames with Update[n-1] > 4 ms: ${scr} of ${worst.length}; with aligned engine > 6 ms: ${eng} of ${worst.length}`);
// the distribution of Update[n-1] over the worst frames
const u = worst.map((i) => upd(i - 1)).sort((a, b) => a - b);
console.log(`  Update[n-1] in worst frames: min ${u[0].toFixed(2)} p25 ${u[Math.floor(u.length / 4)].toFixed(2)} median ${u[Math.floor(u.length / 2)].toFixed(2)} p75 ${u[Math.floor(u.length * 3 / 4)].toFixed(2)} max ${u[u.length - 1].toFixed(2)}`);
