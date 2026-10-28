// NIGHT71 F1 (builder F): read a bench's two CSVs (PlayerChecks.Report writes <log>.frames.csv and <log>.ft.csv) and print where the
// worst-1 % frames sit, what they share and what they do not. Read only: prints to stdout.
// usage: node spike-read.cjs <bench.log>
const fs = require('fs');
const base = process.argv[2];
if (!base) throw new Error('usage: node spike-read.cjs <bench.log>');
const rows = (f) => fs.readFileSync(f, 'utf8').trim().split(/\r?\n/).slice(1).map((l) => l.split(',').map(Number));
const fr = rows(base + '.frames.csv'); // frame, interval_ms, steps, step_ms, update_ms, gc_count, gc_used_bytes
const ft = rows(base + '.ft.csv'); // ft, cpu_ms, main_ms, render_ms, present_wait_ms, gpu_ms
const warm = 30; // PlayerChecks.Report's warm-up
const f = fr.slice(warm);
const m = f.length;
const iv = f.map((r) => r[1]);
const sorted = [...iv].sort((a, b) => a - b);
const Q = (q) => sorted[Math.min(m - 1, Math.floor(m * q))];
const p99 = Q(0.99);
console.log(`file ${base}`);
console.log(`frames ${m} (after ${warm} warm); median ${Q(0.5).toFixed(2)} p99 ${p99.toFixed(2)} p99.9 ${Q(0.999).toFixed(2)} max ${sorted[m - 1].toFixed(2)} ms`);
// The worst 1 % by rank (PlayerChecks' rule: rank >= floor(0.99 m)).
const idx = [...Array(m).keys()].sort((a, b) => iv[a] - iv[b]);
const worst = idx.slice(Math.floor(m * 0.99)).sort((a, b) => a - b);
// time of each frame since the bench's first counted frame
const t = new Array(m);
let acc = 0;
for (let i = 0; i < m; i++) { t[i] = acc / 1000; acc += iv[i]; }
const total = acc / 1000;
console.log(`worst-1% frames ${worst.length}; bench length ${total.toFixed(1)} s`);
// 1. When: by 6 s bins
const bins = new Array(Math.ceil(total / 6)).fill(0);
for (const w of worst) bins[Math.min(bins.length - 1, Math.floor(t[w] / 6))]++;
console.log(`when (worst-1% count per 6 s bin): ${bins.join(' ')}`);
// 2. Gaps between them (frames, s)
const gaps = [];
for (let i = 1; i < worst.length; i++) gaps.push(t[worst[i]] - t[worst[i - 1]]);
const gs = [...gaps].sort((a, b) => a - b);
const G = (q) => gs.length ? gs[Math.min(gs.length - 1, Math.floor(gs.length * q))] : 0;
console.log(`gap between worst frames (s): p10 ${G(0.1).toFixed(3)} median ${G(0.5).toFixed(3)} p90 ${G(0.9).toFixed(3)}; runs of adjacent worst frames: ${gaps.filter((g, i) => worst[i + 1] - worst[i] === 1).length}`);
// 3. What they share: sim steps, step ms, Update ms, engine = interval - Update, a GC count move, the heap falling
const mean = (a) => a.reduce((s, x) => s + x, 0) / Math.max(1, a.length);
const col = (set, c) => set.map((i) => f[i][c]);
const band = idx.slice(Math.floor(m * 0.45), Math.floor(m * 0.55));
const all = [...Array(m).keys()];
for (const [name, fn] of [
  ['interval ms', (i) => f[i][1]],
  ['sim steps', (i) => f[i][2]],
  ['sim step ms', (i) => f[i][3]],
  ['Update ms (SurvivalGame.Update only)', (i) => f[i][4]],
  ['engine ms (interval - Update)', (i) => f[i][1] - f[i][4]],
  ['GC count moved this frame', (i) => (i + warm > 0 && fr[i + warm][5] !== fr[i + warm - 1][5] ? 1 : 0)],
  ['heap fell this frame (a sweep)', (i) => (i + warm > 0 && fr[i + warm][6] < fr[i + warm - 1][6] ? 1 : 0)],
  ['steps >= 2 this frame', (i) => (f[i][2] >= 2 ? 1 : 0)],
  ['next frame under the median', (i) => (i + 1 < m && iv[i + 1] < Q(0.5) ? 1 : 0)],
]) console.log(`  ${name.padEnd(40)} worst ${mean(worst.map(fn)).toFixed(3)} | median band ${mean(band.map(fn)).toFixed(3)} | all ${mean(all.map(fn)).toFixed(3)}`);
// 4. Engine-time spike size distribution in the worst frames
const eng = worst.map((i) => f[i][1] - f[i][4]).sort((a, b) => a - b);
console.log(`engine ms in worst frames: min ${eng[0].toFixed(2)} median ${eng[Math.floor(eng.length / 2)].toFixed(2)} max ${eng[eng.length - 1].toFixed(2)}`);
// 5. Heap: allocation rate (rises) and the sweeps
let rise = 0, falls = 0;
for (let i = warm + 1; i < fr.length; i++) { const d = fr[i][6] - fr[i - 1][6]; if (d > 0) rise += d; else if (d < 0) falls++; }
console.log(`heap: rises ${(rise / 1048576).toFixed(1)} MB over ${total.toFixed(1)} s (${(rise / 1048576 / total).toFixed(2)} MB/s, ${(rise / m / 1024).toFixed(1)} KB/frame); frames where it fell ${falls}; GC count ${fr[warm][5]} -> ${fr[fr.length - 1][5]}`);
// 6. The FrameTiming CSV: render thread and present wait in its own worst 1 % (by cpu)
const g = ft.slice(warm);
const fm = g.length;
const fidx = [...Array(fm).keys()].sort((a, b) => g[a][1] - g[b][1]);
const fw = fidx.slice(Math.floor(fm * 0.99));
const fb = fidx.slice(Math.floor(fm * 0.45), Math.floor(fm * 0.55));
for (const [name, c] of [['cpu', 1], ['main', 2], ['render thread', 3], ['present wait', 4], ['gpu', 5]])
  console.log(`  ft ${name.padEnd(14)} worst ${mean(fw.map((i) => g[i][c])).toFixed(3)} | median band ${mean(fb.map((i) => g[i][c])).toFixed(3)} ms`);
// main minus present wait minus render-thread-bound share: worst frames where the render thread exceeds the main thread's busy time
const rtBound = fw.filter((i) => g[i][3] > g[i][2] - g[i][4]).length;
console.log(`  ft worst frames where the render thread > main busy: ${rtBound} of ${fw.length}`);
// 7. The top 15 frames, with neighbours
console.log('top 15 frames (counted index, t s, interval, steps, step ms, Update ms, engine ms, gc moved, heap MB):');
for (const i of [...idx].slice(-15).reverse()) {
  const r = f[i];
  const gm = i + warm > 0 && fr[i + warm][5] !== fr[i + warm - 1][5] ? 'GC' : '';
  console.log(`  ${String(i).padStart(6)} ${t[i].toFixed(2).padStart(6)} ${r[1].toFixed(2).padStart(7)} ${r[2]} ${r[3].toFixed(2)} ${r[4].toFixed(2)} ${(r[1] - r[4]).toFixed(2)} ${gm} ${(r[6] / 1048576).toFixed(1)}`);
}
