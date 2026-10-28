// NIGHT57 builder G: compare n46flow outputs (a base, its seeds 11-20 run for the band, and row-on runs) on G's numbers (bot):
//   stall %, deaths, clear (sum of room medians), attack starts in the sim cone, RANGED hit rates, IDLE (pooled, over-20 % share), and the
//   press57 / tokens56 lines when present. Reads only.
//   node Tools/night/n57-g-cmp.cjs <base.txt> <base-s11.txt|-> <on.txt> [<on2.txt> ...]
// The band (NIGHT57.md section 0): bot numbers, seeds 1-10 vs 11-20 of the baseline (the largest gap), or 10 % relative for the clear time.
'use strict';
const fs = require('fs');
const files = process.argv.slice(2);
if (files.length < 3) { console.log('usage: n57-g-cmp.cjs <base> <base-s11|-> <on> [...]'); process.exit(2); }

function parse(f) {
  if (f === '-') return null;
  const t = fs.readFileSync(f, 'utf8').split(/\r?\n/);
  const lv = {};
  for (const l of t) {
    let m = l.match(/^n46flow (\S+) SUMMARY: .*?stall ([\d.]+) %.*?deaths (\d+), falls (\d+), clear ([\d.]+) s/);
    if (m) { (lv[m[1]] ||= {}); Object.assign(lv[m[1]], { stall: +m[2], deaths: +m[3], clear: +m[5] }); continue; }
    m = l.match(/^n46flow (\S+) TOKENS: .*? (\d+)\/(\d+) = ([\d.]+) %.*?waited out of view (\d+), advances (\d+)/);
    if (m) { (lv[m[1]] ||= {}); Object.assign(lv[m[1]], { coneIn: +m[2], coneAll: +m[3], cone: +m[4], waits: +m[5], adv: +m[6] }); continue; }
    m = l.match(/^n46flow (\S+) RANGED: .*?stand (\d+)\/(\d+) = [^,]*, run (\d+)\/(\d+) = [^,]*, fast (\d+)\/(\d+) = [^,]*, dash (\d+)\/(\d+)/);
    if (m) { (lv[m[1]] ||= {}); Object.assign(lv[m[1]], { rng: m.slice(2, 10).map(Number) }); continue; }
    m = l.match(/^n46flow (\S+) IDLE: idle in place ([\d.]+) %.*?p90 ([\d.]+) %, over 20 %: (\d+) \(([\d.]+) %\)/);
    if (m) { (lv[m[1]] ||= {}); Object.assign(lv[m[1]], { idle: +m[2], idleP90: +m[3], over20: +m[5] }); continue; }
    m = l.match(/^n46flow (\S+) PRESS57: (.*)$/);
    if (m) { (lv[m[1]] ||= {}); lv[m[1]].press = m[2]; continue; }
  }
  return lv;
}

const base = parse(files[0]), band = parse(files[1]), ons = files.slice(2).map(parse);
const pct = (a, b) => b ? (100 * a / b).toFixed(1) : '-';
let pool = { base: [0, 0], on: ons.map(() => [0, 0]) };
for (const id of Object.keys(base)) {
  const b = base[id], s = band ? band[id] : null;
  console.log(`== ${id} (bot)`);
  const dBand = s ? Math.abs(b.deaths - s.deaths) : NaN, stallBand = s ? Math.abs(b.stall - s.stall) : NaN;
  console.log(`  base: stall ${b.stall} %  deaths ${b.deaths}  clear ${b.clear} s  cone ${b.cone} % (${b.coneIn}/${b.coneAll})  idle ${b.idle} % (over20 ${b.over20} %)  ranged hits stand ${b.rng ? pct(b.rng[0], b.rng[1]) : '-'} run ${b.rng ? pct(b.rng[2], b.rng[3]) : '-'} fast+dash ${b.rng ? pct(b.rng[4] + b.rng[6], b.rng[5] + b.rng[7]) : '-'} %`);
  if (s) console.log(`  base s11-20: stall ${s.stall} %  deaths ${s.deaths}  clear ${s.clear} s  cone ${s.cone} %   band: deaths +-${dBand}, stall +-${stallBand.toFixed(1)}, clear +10 % = ${(b.clear * 1.1).toFixed(1)} s`);
  pool.base[0] += b.coneIn; pool.base[1] += b.coneAll;
  ons.forEach((o, k) => {
    const x = o[id];
    if (!x) { console.log(`  on${k + 1}: (no level)`); return; }
    pool.on[k][0] += x.coneIn; pool.on[k][1] += x.coneAll;
    const dOk = s ? (x.deaths <= Math.max(b.deaths, s.deaths) + 0 || x.deaths - b.deaths <= dBand) : x.deaths <= b.deaths;
    const cOk = x.clear <= b.clear * 1.1;
    const hitsMove = x.rng ? `run ${pct(x.rng[2], x.rng[3])} fast+dash ${pct(x.rng[4] + x.rng[6], x.rng[5] + x.rng[7])}` : '-';
    console.log(`  on${k + 1} (${files[2 + k]}): stall ${x.stall} % (${(x.stall - b.stall).toFixed(1)})  deaths ${x.deaths} [${dOk ? 'in band' : 'OUT'}]  clear ${x.clear} s (${(100 * (x.clear / b.clear - 1)).toFixed(1)} %) [${cOk ? 'ok' : 'OUT'}]  cone ${x.cone} %  idle ${x.idle} % (over20 ${x.over20} %)  ranged stand ${x.rng ? pct(x.rng[0], x.rng[1]) : '-'} ${hitsMove} %  waits ${x.waits} adv ${x.adv}`);
    if (x.press) console.log(`      PRESS57: ${x.press}`);
  });
}
console.log(`pooled cone: base ${pct(pool.base[0], pool.base[1])} %` + ons.map((o, k) => `, on${k + 1} ${pct(pool.on[k][0], pool.on[k][1])} %`).join(''));
