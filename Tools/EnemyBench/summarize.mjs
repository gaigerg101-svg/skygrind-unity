// Tables for SPIKE-RESULTS.md from Tools/EnemyBench/results/*.json (one Player run each), with the web bench's
// statistics: the median of the passes is sorted[floor(n / 2)] per column (skygrind port-export/bench/summarize.mjs),
// and the spread is the min-max across passes. usage: node Tools/EnemyBench/summarize.mjs > Tools/EnemyBench/results/tables.md
import { readdirSync, readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const dir = join(dirname(fileURLToPath(import.meta.url)), 'results');
const runs = readdirSync(dir).filter((f) => f.endsWith('.json')).flatMap((f) => JSON.parse(readFileSync(join(dir, f), 'utf8')));
const med = (a) => [...a].sort((x, y) => x - y)[Math.floor(a.length / 2)];
const f = (v, d) => (v == null ? '-' : v.toFixed(d));
const spread = (a, d) => `${f(med(a), d)} (${f(Math.min(...a), d)}-${f(Math.max(...a), d)})`;
const order = ['closer-web', 'closer-unity', 'plain', 'burst-full', 'burst-plain'];
const groups = new Map();
for (const r of runs) {
  const k = `${r.variant}|${r.n}`;
  if (!groups.has(k)) groups.set(k, []);
  groups.get(k).push(r);
}
const keys = [...groups.keys()].sort((a, b) => {
  const [va, na] = a.split('|');
  const [vb, nb] = b.split('|');
  return order.indexOf(va) - order.indexOf(vb) || na - nb;
});
const web = { 24: [0.0609, 0.2757], 40: [0.0791, 0.3396], 100: [0.2956, 0.9506], 200: [0.6026, 1.5794] };
const out = [];
const tags = [...new Set(runs.map((r) => r.tag))].sort();
out.push(`Passes: ${tags.join(', ')}. Each value: median of the passes (min-max across passes).`, '');
out.push('### Sim: ms per 120 Hz step (whole enemy update)', '');
out.push('| variant | N | passes | avg | p99 | worst 1% (mean) | max | share of the 8.33 ms step | web Node avg / worst 1% | LOS rays/s |');
out.push('|---|---|---|---|---|---|---|---|---|---|');
for (const k of keys) {
  const rs = groups.get(k);
  const [v, n] = [rs[0].variant, rs[0].n];
  const s = (key) => rs.map((r) => r.stepMs[key]);
  const w = web[n];
  out.push(`| ${v} | ${n} | ${rs.length} | ${spread(s('mean'), 4)} | ${spread(s('p99'), 4)} | ${spread(s('worst1pctMean'), 4)} | ${spread(s('max'), 3)} | ${f((100 * med(s('mean'))) / 8.3333, 1)}% | ${w ? `${w[0]} / ${w[1]}` : '-'} | ${f(med(rs.map((r) => r.losRaysPerSecond)), 0)} |`);
}
out.push('', '### Frames: built Player, 1920 x 1080 windowed', '');
out.push('| variant | N | alive at end | fps | avg ms | p99 ms | worst 1% (mean) ms | max ms | steps/s |');
out.push('|---|---|---|---|---|---|---|---|---|');
for (const k of keys) {
  const rs = groups.get(k);
  const m = (fn) => rs.map(fn);
  out.push(`| ${rs[0].variant} | ${rs[0].n} | ${m((r) => r.aliveAtEnd).join('/')} | ${spread(m((r) => r.fps), 1)} | ${spread(m((r) => r.frameMs.mean), 2)} | ${spread(m((r) => r.frameMs.p99), 2)} | ${spread(m((r) => r.frameMs.worst1pctMean), 2)} | ${spread(m((r) => r.frameMs.max), 1)} | ${spread(m((r) => r.stepsPerSecond), 1)} |`);
}
out.push('', '#### Every pass (fps / avg / p99 / worst 1% frame ms; first -> last 5 s window mean ms; sim avg / worst 1% ms)', '');
out.push(`| variant | N | ${tags.join(' | ')} |`);
out.push(`|---|---|${tags.map(() => '---').join('|')}|`);
for (const k of keys) {
  const rs = groups.get(k);
  const cell = (t) => {
    const r = rs.find((x) => x.tag === t);
    if (!r) return '-';
    const w = r.frameWindowMeansMs;
    return `${f(r.fps, 1)} / ${f(r.frameMs.mean, 2)} / ${f(r.frameMs.p99, 2)} / ${f(r.frameMs.worst1pctMean, 2)}; ${f(w[0], 1)} -> ${f(w[w.length - 1], 1)}; ${f(r.stepMs.mean, 4)} / ${f(r.stepMs.worst1pctMean, 4)}`;
  };
  out.push(`| ${rs[0].variant} | ${rs[0].n} | ${tags.map(cell).join(' | ')} |`);
}
const silent = runs.filter((r) => /^Silent x\d+$/.test(r.powerPlans ?? ''));
out.push('', `Power plan (powercfg, polled every 2-4 s during each run): Silent throughout in ${silent.length} of ${runs.length} runs${silent.length < runs.length ? `; not: ${runs.filter((r) => !silent.includes(r)).map((r) => `${r.tag} ${r.variant} ${r.n} (${r.powerPlans})`).join(', ')}` : ''}.`);
let setAside = [];
try {
  setAside = readdirSync(join(dir, 'not-silent')).filter((f) => f.endsWith('.json')).flatMap((f) => JSON.parse(readFileSync(join(dir, 'not-silent', f), 'utf8')));
} catch {}
if (setAside.length) out.push(`Set aside and rerun (the plan switched to another scheme mid-pass; not in the tables): ${setAside.map((r) => `${r.tag} ${r.variant} ${r.n} (${r.powerPlans}; ${f(r.fps, 1)} fps, sim ${f(r.stepMs.mean, 4)} ms)`).join(', ')}.`);
let prof = [];
try {
  prof = readdirSync(join(dir, 'profile')).filter((f) => f.endsWith('.json')).flatMap((f) => JSON.parse(readFileSync(join(dir, 'profile', f), 'utf8')));
} catch {}
if (prof.length) {
  prof.sort((x, y) => order.indexOf(x.variant) - order.indexOf(y.variant) || x.n - y.n);
  out.push('', '### Where the step goes (one run each with -benchProfile timers; not in the tables above)', '');
  out.push('| variant | N | step avg (timed) | grid + flow field | line of sight | steering + separation | move + push-out | separation pairs / step | power plan |');
  out.push('|---|---|---|---|---|---|---|---|---|');
  for (const r of prof) {
    const p = r.phaseMsPerStep;
    const pc = (v) => `${f(v, 4)} (${f((100 * v) / r.stepMs.mean, 0)}%)`;
    out.push(`| ${r.variant} | ${r.n} | ${f(r.stepMs.mean, 4)} | ${pc(p.gridAndFlow)} | ${pc(p.lineOfSight)} | ${pc(p.steerAndSeparation)} | ${pc(p.moveAndPushOut)} | ${f(r.neighbourChecksPerStep, 0)} | ${r.powerPlans} |`);
  }
}
const r0 = runs[0];
if (r0) {
  out.push('', `Every run: ${[...new Set(runs.map((r) => r.gpu))].join(', ')}; ${[...new Set(runs.map((r) => `${r.graphicsApi}, screen ${r.screen} ${r.fullScreenMode}, vsync ${r.vSyncCount}, target frame rate ${r.targetFrameRate}, quality '${r.quality}', ${r.backend}${r.developmentBuild ? ' development' : ''} build, Unity ${r.unity}`))].join('; ')}.`);
  out.push(`CPU ${r0.cpu} (${r0.logicalCores} logical), RAM ${r0.ramMB} MB, ${r0.os}.`);
}
console.log(out.join('\n'));
