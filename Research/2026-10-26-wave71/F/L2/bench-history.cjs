// NIGHT71 builder F (L2): every saved bench log's tail, oldest first, to bracket when the worst-1 % engine spikes appeared. Read only: prints.
// usage (from the main tree): node bench-history.cjs <dir> [<dir> ...]   (e.g. Logs/player Logs/n63)
'use strict';
const fs = require('fs');
const path = require('path');
const rows = [];
for (const dir of process.argv.slice(2)) {
  let names = [];
  try { names = fs.readdirSync(dir); } catch (e) { continue; }
  for (const n of names) {
    if (!/bench.*\.log$/.test(n)) continue;
    const p = path.join(dir, n);
    let t;
    try { t = fs.readFileSync(p, 'utf8'); } catch (e) { continue; }
    const tail = /bench tail: p99 ([0-9.]+) ms, p99\.9 ([0-9.]+) ms, max ([0-9.]+) ms, frames over [0-9.]+ ms \d+ of (\d+) \(([0-9.]+)%\)/.exec(t);
    if (!tail) continue;
    const res = /bench result: \d+ frames, alive at end (\d+), median ([0-9.]+) ms.*vsync (\d+), cap (-?\d+)/.exec(t);
    const scen = /bench: \d+ husks alive, [0-9.]+ s, scenario ([a-z-]+)/.exec(t);
    const eng = /bench breakdown\s+interval - Update \(engine\)\s+([0-9.]+) \|\s+([0-9.]+)/.exec(t);
    const gpu = /\[skygrind\] check 'bench' starting; (\d+x\d+), ([^,]+),/.exec(t);
    rows.push({
      when: fs.statSync(p).mtime.toISOString().slice(0, 16).replace('T', ' '), file: p,
      scen: scen ? scen[1] : '?', median: res ? res[2] : '?', vsync: res ? res[3] : '?', cap: res ? res[4] : '?',
      p99: tail[1], p999: tail[2], max: tail[3], over: tail[5], eng: eng ? `${eng[1]} | ${eng[2]}` : '?', screen: gpu ? `${gpu[1]} ${gpu[2]}` : '?',
    });
  }
}
rows.sort((a, b) => a.when.localeCompare(b.when));
for (const r of rows) console.log(`${r.when} ${r.file} [${r.scen}, vsync ${r.vsync}, cap ${r.cap}, ${r.screen}] median ${r.median} p99 ${r.p99} p99.9 ${r.p999} max ${r.max} over6.94 ${r.over}% engine worst|median ${r.eng}`);
