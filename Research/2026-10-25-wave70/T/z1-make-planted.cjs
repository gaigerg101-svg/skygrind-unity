// NIGHT70 builder T, slice Z1: writes the planted cases of Tools/night/n70-m3-cases.sh (run once; its outputs are committed beside it):
//   Z1-m3-planted.txt   saved probe blocks (the shape n65verb launch prints, replayed by Tools/night/n70-m3-dotnet.sh) for rooms kb3, P6-P9
//   Z1-chart-P6.txt     9 GREEN candidates (bursts killing 2) + 11 RED probes whose bursts killed 3: 9 / 20 (a RED probe's burst never counts)
//   Z1-chart-P7.txt     exactly 10 candidates (4 pit, 3 lava, 3 bursts killing 2) + 10 that are not (NIGHT69 Y2's probe rules: a RED line in a
//                       pit, a GREEN launch36 0 "in a pit YES", a burst killing 1, a timeout with nothing): 10 / 20, at the bar
//   Z1-chart-P8.txt     P6's probes with a summary line that claims 20 / 20 (a chart whose summary differs from its probes)
//   Z1-chart-P9.txt     3 GREEN pit probes, 3 / 3 (a chart over 3 seeds, not 20)
//   node Research/2026-10-25-wave70/T/z1-make-planted.cjs
'use strict';
const fs = require('fs');
const path = require('path');
const T = require(path.join(__dirname, '../../../Tools/night/n70-throws-sum.cjs'));
const D = __dirname;
const probe = (room, o) => `n65verb launch: ${o.red ? 'RED' : 'GREEN'} | room ${room} | launch36 ${o.l36 ?? 1}, flight end ${o.end || 'body'} thrown ${o.th ?? 1.0} m (max ${o.th ?? 1.0} m), bursts ${o.kills ? 1 : 0} (hits ${o.kills || 0}, kills ${o.kills || 0}) | launched body husk #1 (the target: yes), landed (0.0, ${o.pit ? '-2.1' : '0.0'}, 0.0), in a pit ${o.pit ? 'YES' : 'no'}, in lava ${o.lava ? 'YES' : 'no'}, target dead (planted) | pose line65[0] (planted)`;
const block = (room, o) => `n65verb launch: level planted (Z1 planted), room ${room} "planted", seed 1; pose (0.0, 0.0, 0.0) yaw 0.0 from line65[0]; off switch none\n${probe(room, o)}\nn65verb: exit 0\n`;
// the replay blocks: each room's one probe (what NIGHT69's single-seed reader read)
const planted = '# NIGHT70 Z1 planted probe blocks (z1-make-planted.cjs): one saved `n65verb launch` run per room, replayed by n70-m3-dotnet.sh\n' +
  block('kb3', { th: 9.0, end: 'timeout', pit: true }) +   // the one probe fell into the quench; the room's 20-seed chart is the shipped kb3's 7 / 20
  block('P6', { kills: 3 }) + block('P7', { lava: true, end: 'timeout', th: 8.0 }) + block('P8', { pit: true, end: 'timeout', th: 12.8 }) + block('P9', { pit: true, end: 'timeout', th: 12.8 });
fs.writeFileSync(path.join(D, 'Z1-m3-planted.txt'), planted);
const chart = (room, list, summary) => { const head = `# command: planted (Research/2026-10-25-wave70/T/z1-make-planted.cjs), room ${room}\n`; let s = ''; list.forEach((o, i) => { s += `## seed ${i + 1}\n` + probe(room, o) + '\n'; }); return head + s + (summary || T.summarize(s).line) + '\n'; };
const rep = (n, o) => Array.from({ length: n }, () => o);
const p6 = [...rep(9, { kills: 2 }), ...rep(11, { red: true, kills: 3 })];
fs.writeFileSync(path.join(D, 'Z1-chart-P6.txt'), chart('P6', p6));
const p7 = [...rep(4, { pit: true, end: 'timeout', th: 12.8 }), ...rep(3, { lava: true, end: 'timeout', th: 8.0 }), ...rep(3, { kills: 2 }),
  ...rep(3, { red: true, pit: true, end: 'timeout', th: 12.8 }), ...rep(3, { l36: 0, pit: true, end: '', th: 0 }), ...rep(2, { kills: 1, end: 'wall', th: 3.0 }), ...rep(2, { end: 'timeout', th: 12.8 })];
fs.writeFileSync(path.join(D, 'Z1-chart-P7.txt'), chart('P7', p7));
const p8s = T.summarize(p6.map((o) => probe('P8', o)).join('\n')).line.replace(/M3 candidates \(D-3\) \d+ \/ 20/, 'M3 candidates (D-3) 20 / 20');
fs.writeFileSync(path.join(D, 'Z1-chart-P8.txt'), chart('P8', p6, p8s));
fs.writeFileSync(path.join(D, 'Z1-chart-P9.txt'), chart('P9', rep(3, { pit: true, end: 'timeout', th: 12.8 })));
for (const f of ['P6', 'P7', 'P8', 'P9']) { const t = fs.readFileSync(path.join(D, 'Z1-chart-' + f + '.txt'), 'utf8'); console.log(f + ': ' + t.trim().split('\n').pop() + '  =>  ' + JSON.stringify(T.m3(t, f, 'Z1-chart-' + f).why)); }
