// NIGHT14 U3-27.6: THE FARM's report. Reads the JSONL lines Farm.Run writes (one AUTOPILOT run each: Game/Bot/Farm.cs) and
// writes a Markdown report plus a self-contained HTML page (inline SVG, no network, the dusk palette from
// PortExport/config.json: colors.usable is the player's cyan, colors.enemy the red-orange of enemies and danger,
// barrier.boundaryColor the pale violet-white of boundaries; never RIVAL's magenta). Node, no packages.
//   node Tools/farm-report.cjs [--name n14] [--out Reports] a.jsonl [b.jsonl ...]
//        -> <out>/FARM-<name>.md and <out>/farm-<name>.html (charts: round reached per difficulty, a survival curve,
//           points per round, kills by gun, downs by enemy kind, buy timing (perks, guns, THE FORGE, gates))
//   node Tools/farm-report.cjs diff [--name diff] [--out Reports] a.jsonl b.jsonl
//        -> <out>/FARM-<name>.md and <out>/farm-<name>.html: per difficulty, A against B (mean round, the survival curves,
//           kills by gun, causes) with the deltas (what NIGHT15's guns will use: a farm diff after a gameplay change)
// NIGHT20 P0-3 (builder P): a line's `pace` (one row per round reached: its type, break and fight seconds, the fight by the
// director's phase, downs, damage taken) gives the report its pacing tables: by round type (median fight, the BUILD / PEAK /
// FADE / RELAX shares, downs per round) and by round number. Lines from before NIGHT20 have no `pace`: the tables say so.
// NIGHT20 A3 (builder P2): a line's `beacons` (THE BEACON: round, taken, elites, eliteKilled, cleared) gives the beacon table:
// stood, taken, the reward earned per taken beacon, downs per taken-beacon round against the same rounds without one.
// The findings are OBSERVATIONS for GG, never changes: GG owns enemy design and the numbers (D-55). The header states the
// JSONL line count; the report's run count is that count (checked: the script throws if they differ).
'use strict';
const fs = require('fs');
const path = require('path');

const repo = path.resolve(__dirname, '..');

function palette() {
  const hex = (n) => '#' + Number(n).toString(16).padStart(6, '0');
  let cfg = {};
  try {
    cfg = JSON.parse(fs.readFileSync(path.join(repo, 'PortExport', 'config.json'), 'utf8'));
  } catch (e) {
    // the report still renders with the documented values (config.json read failed: say so in the page)
  }
  const c = cfg.colors || {};
  return {
    usable: hex(c.usable ?? 0x19d3e6), // cyan: the player's things
    enemy: hex(c.enemy ?? 0xff4a1c), // red-orange: enemies and danger
    boundary: hex(cfg.barrier?.boundaryColor ?? 0xc9bbff), // pale violet-white: axes and frames
    bg: hex(c.skyZenith ?? 0x100a2c), // the dusk sky's zenith: the page
    panel: '#1a1438',
    text: '#ece8f6',
    dim: '#a69fc0',
    fromConfig: !!cfg.colors,
  };
}
const P = palette();
// three cyan steps for easy / normal / hard (the player's runs: one hue, the difficulty by lightness)
const DIFF_COLORS = { easy: '#8ff0fa', normal: P.usable, hard: '#0e8a99' };
const DIFFS = ['easy', 'normal', 'hard'];

function args(argv) {
  const o = { name: null, out: path.join(repo, 'Reports'), files: [], diff: false };
  for (let i = 0; i < argv.length; i++) {
    const a = argv[i];
    if (a === 'diff' && i === 0) o.diff = true;
    else if (a === '--name') o.name = argv[++i];
    else if (a === '--out') o.out = path.resolve(argv[++i]);
    else o.files.push(a);
  }
  if (!o.name) o.name = o.diff ? 'diff' : 'n14';
  return o;
}

function readRuns(files) {
  const runs = [];
  let lines = 0;
  for (const f of files) {
    const text = fs.readFileSync(f, 'utf8');
    text.split(/\r?\n/).forEach((line, i) => {
      if (!line.trim()) return;
      lines++;
      let r;
      try {
        r = JSON.parse(line);
      } catch (e) {
        throw new Error(`${f}:${i + 1}: not JSON (${e.message})`);
      }
      r._file = path.basename(f);
      runs.push(r);
    });
  }
  if (runs.length !== lines) throw new Error(`read ${lines} JSONL lines but ${runs.length} runs`);
  return { runs, lines };
}

// --- numbers -------------------------------------------------------------------------------------------------------
const sum = (a) => a.reduce((s, x) => s + x, 0);
const mean = (a) => (a.length ? sum(a) / a.length : 0);
function median(a) {
  if (!a.length) return 0;
  const s = [...a].sort((x, y) => x - y);
  const m = s.length >> 1;
  return s.length % 2 ? s[m] : (s[m - 1] + s[m]) / 2;
}
const f1 = (x) => (Math.round(x * 10) / 10).toFixed(1);
const f2 = (x) => (Math.round(x * 100) / 100).toFixed(2);
const pct = (x) => `${Math.round(x * 100)}%`;
const signed = (x, f = f2) => (x > 0 ? '+' : x < 0 ? '−' : '±') + f(Math.abs(x));
const esc = (s) => String(s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');

function byDifficulty(runs) {
  const g = {};
  for (const r of runs) (g[r.difficulty] ??= []).push(r);
  const order = [...DIFFS.filter((d) => g[d]), ...Object.keys(g).filter((d) => !DIFFS.includes(d)).sort()];
  return order.map((d) => [d, g[d]]);
}

function count(list, key) {
  const m = new Map();
  for (const x of list) m.set(key(x), (m.get(key(x)) || 0) + 1);
  return [...m.entries()].sort((a, b) => b[1] - a[1] || String(a[0]).localeCompare(String(b[0])));
}

function killsByGun(runs) {
  const m = new Map();
  for (const r of runs) for (const [g, n] of Object.entries(r.killsByGun || {})) m.set(g, (m.get(g) || 0) + n);
  return [...m.entries()].sort((a, b) => b[1] - a[1] || a[0].localeCompare(b[0]));
}

const isDown = (r) => r.cause !== 'timeout' && r.cause !== 'exfil';

/** share of runs that reached round k (k = 1..max): the survival curve (a timeout or exfil stops at its round, not a death) */
function curve(runs, max) {
  const out = [];
  for (let k = 1; k <= max; k++) out.push(runs.length ? runs.filter((r) => r.round >= k).length / runs.length : 0);
  return out;
}

/** mean points earned / spent in round k over the runs that reached it */
function pointsPerRound(runs, max) {
  const e = [];
  const s = [];
  for (let k = 0; k < max; k++) {
    const reached = runs.filter((r) => (r.earnedByRound || []).length > k);
    e.push(mean(reached.map((r) => r.earnedByRound[k])));
    s.push(mean(reached.map((r) => r.spentByRound[k] ?? 0)));
  }
  return { e, s };
}

/** buy timing: per item ("perk:x", "gun:x", "forge:x", "gate:x"), the rounds it was bought in */
function buys(runs) {
  const m = new Map();
  const add = (k, round) => (m.get(k) ?? m.set(k, []).get(k)).push(round);
  for (const r of runs) {
    for (const p of r.perks || []) add(`perk:${p.id}`, p.round);
    for (const g of r.guns || []) add(`gun:${g.id}`, g.round);
    for (const f of r.forge || []) add(`forge:${f.id}`, f.round);
    for (const g of r.gates || []) add(`gate:${g.id}`, g.round);
  }
  return [...m.entries()].sort((a, b) => mean(a[1]) - mean(b[1]) || a[0].localeCompare(b[0]));
}

// --- SVG -----------------------------------------------------------------------------------------------------------
const W = 720;
function frame(h, title) {
  return [`<svg viewBox="0 0 ${W} ${h}" width="100%" role="img" aria-label="${esc(title)}" xmlns="http://www.w3.org/2000/svg">`];
}
function axis(lines, x0, y0, x1, y1) {
  lines.push(`<line x1="${x0}" y1="${y1}" x2="${x1}" y2="${y1}" stroke="${P.boundary}" stroke-opacity="0.6"/>`);
  lines.push(`<line x1="${x0}" y1="${y0}" x2="${x0}" y2="${y1}" stroke="${P.boundary}" stroke-opacity="0.6"/>`);
}
function text(lines, x, y, s, opts = {}) {
  const a = opts.anchor ? ` text-anchor="${opts.anchor}"` : '';
  const fill = opts.fill || P.dim;
  lines.push(`<text x="${x}" y="${y}"${a} fill="${fill}" font-size="${opts.size || 12}">${esc(s)}</text>`);
}
function legend(lines, items, x, y) {
  let cx = x;
  for (const [label, color, dash] of items) {
    if (dash) lines.push(`<line x1="${cx}" y1="${y - 4}" x2="${cx + 18}" y2="${y - 4}" stroke="${color}" stroke-width="2" stroke-dasharray="${dash}"/>`);
    else lines.push(`<rect x="${cx}" y="${y - 10}" width="12" height="12" fill="${color}"/>`);
    text(lines, cx + 22, y, label, { fill: P.text });
    cx += 30 + label.length * 7.5;
  }
}

/** grouped bars: groups (x labels) x series [{label, color, values}] */
function barChart(title, xs, series, opts = {}) {
  const h = 260;
  const L = 48, R = 12, T = 30, B = 40;
  const max = Math.max(1, ...series.flatMap((s) => s.values));
  const lines = frame(h, title);
  axis(lines, L, T, W - R, h - B);
  const gw = (W - L - R) / Math.max(1, xs.length);
  const bw = Math.max(2, (gw * 0.8) / series.length);
  for (let t = 0; t <= 4; t++) {
    const v = (max * t) / 4;
    const y = h - B - ((h - T - B) * t) / 4;
    text(lines, L - 6, y + 4, opts.fmt ? opts.fmt(v) : String(Math.round(v)), { anchor: 'end' });
    if (t) lines.push(`<line x1="${L}" y1="${y}" x2="${W - R}" y2="${y}" stroke="${P.boundary}" stroke-opacity="0.12"/>`);
  }
  xs.forEach((x, i) => {
    series.forEach((s, j) => {
      const v = s.values[i] || 0;
      const bh = ((h - T - B) * v) / max;
      const bx = L + i * gw + gw * 0.1 + j * bw;
      lines.push(`<rect x="${f1(bx)}" y="${f1(h - B - bh)}" width="${f1(bw - 1)}" height="${f1(bh)}" fill="${s.color}"><title>${esc(`${s.label} ${x}: ${opts.fmt ? opts.fmt(v) : v}`)}</title></rect>`);
    });
    if (xs.length <= 30 || i % Math.ceil(xs.length / 30) === 0) text(lines, L + i * gw + gw / 2, h - B + 16, x, { anchor: 'middle' });
  });
  if (opts.xLabel) text(lines, (L + W - R) / 2, h - 6, opts.xLabel, { anchor: 'middle' });
  legend(lines, series.map((s) => [s.label, s.color]), L, 18);
  lines.push('</svg>');
  return lines.join('\n');
}

/** lines over x = 1..n: series [{label, color, values, dash}] (values 0..1 when opts.share) */
function lineChart(title, n, series, opts = {}) {
  const h = 260;
  const L = 48, R = 12, T = 30, B = 40;
  const max = opts.share ? 1 : Math.max(1, ...series.flatMap((s) => s.values));
  const lines = frame(h, title);
  axis(lines, L, T, W - R, h - B);
  const xAt = (i) => L + ((W - L - R) * i) / Math.max(1, n - 1);
  const yAt = (v) => h - B - ((h - T - B) * v) / max;
  for (let t = 0; t <= 4; t++) {
    const v = (max * t) / 4;
    text(lines, L - 6, yAt(v) + 4, opts.share ? pct(v) : String(Math.round(v)), { anchor: 'end' });
    if (t) lines.push(`<line x1="${L}" y1="${yAt(v)}" x2="${W - R}" y2="${yAt(v)}" stroke="${P.boundary}" stroke-opacity="0.12"/>`);
  }
  for (let i = 0; i < n; i++) if (n <= 30 || i % Math.ceil(n / 30) === 0) text(lines, xAt(i), h - B + 16, String(i + 1), { anchor: 'middle' });
  for (const s of series) {
    const pts = s.values.map((v, i) => `${f1(xAt(i))},${f1(yAt(v))}`).join(' ');
    lines.push(`<polyline points="${pts}" fill="none" stroke="${s.color}" stroke-width="2"${s.dash ? ` stroke-dasharray="${s.dash}"` : ''}/>`);
    s.values.forEach((v, i) => lines.push(`<circle cx="${f1(xAt(i))}" cy="${f1(yAt(v))}" r="2.5" fill="${s.color}"><title>${esc(`${s.label} round ${i + 1}: ${opts.share ? pct(v) : f1(v)}`)}</title></circle>`));
  }
  if (opts.xLabel) text(lines, (L + W - R) / 2, h - 6, opts.xLabel, { anchor: 'middle' });
  legend(lines, series.map((s) => [s.label, s.color, s.dash]), L, 18);
  lines.push('</svg>');
  return lines.join('\n');
}

/** horizontal bars: rows [[label, value]] */
function hbarChart(title, rows, color, opts = {}) {
  const rowH = 22;
  const T = 10, B = 10, L = 170, R = 60;
  const h = T + B + rowH * Math.max(1, rows.length);
  const max = Math.max(1, ...rows.map((r) => r[1]));
  const lines = frame(h, title);
  rows.forEach(([label, v], i) => {
    const y = T + i * rowH;
    const bw = ((W - L - R) * v) / max;
    text(lines, L - 8, y + 15, label, { anchor: 'end', fill: P.text });
    lines.push(`<rect x="${L}" y="${y + 3}" width="${f1(bw)}" height="${rowH - 6}" fill="${color}"><title>${esc(`${label}: ${v}`)}</title></rect>`);
    text(lines, L + bw + 6, y + 15, opts.fmt ? opts.fmt(v) : String(v));
  });
  lines.push(`<line x1="${L}" y1="${T}" x2="${L}" y2="${h - B}" stroke="${P.boundary}" stroke-opacity="0.6"/>`);
  lines.push('</svg>');
  return lines.join('\n');
}

/** buy timing: per item, its rounds as dots (size by count), the mean marked */
function timingChart(title, rows, maxRound) {
  const rowH = 22;
  const T = 26, B = 30, L = 170, R = 70;
  const h = T + B + rowH * Math.max(1, rows.length);
  const lines = frame(h, title);
  const xAt = (r) => L + ((W - L - R) * (r - 1)) / Math.max(1, maxRound - 1);
  for (let r = 1; r <= maxRound; r++) {
    if (maxRound > 30 && r % Math.ceil(maxRound / 30)) continue;
    lines.push(`<line x1="${f1(xAt(r))}" y1="${T}" x2="${f1(xAt(r))}" y2="${h - B}" stroke="${P.boundary}" stroke-opacity="0.12"/>`);
    text(lines, xAt(r), h - B + 16, String(r), { anchor: 'middle' });
  }
  text(lines, (L + W - R) / 2, h - 2, 'round bought', { anchor: 'middle' });
  rows.forEach(([label, rounds], i) => {
    const y = T + i * rowH + rowH / 2;
    text(lines, L - 8, y + 4, label, { anchor: 'end', fill: P.text });
    const c = count(rounds, (x) => x);
    const top = Math.max(...c.map((x) => x[1]));
    for (const [round, n] of c) lines.push(`<circle cx="${f1(xAt(round))}" cy="${y}" r="${f1(2 + (6 * n) / top)}" fill="${P.usable}" fill-opacity="0.55"><title>${esc(`${label}: ${n} in round ${round}`)}</title></circle>`);
    const m = mean(rounds);
    lines.push(`<line x1="${f1(xAt(m))}" y1="${y - 8}" x2="${f1(xAt(m))}" y2="${y + 8}" stroke="${P.text}" stroke-width="2"/>`);
    text(lines, W - R + 6, y + 4, `${rounds.length}×`);
  });
  lines.push('</svg>');
  return lines.join('\n');
}

// --- the report ----------------------------------------------------------------------------------------------------
function summary(runs) {
  const rounds = runs.map((r) => r.round);
  return {
    n: runs.length,
    mean: mean(rounds),
    median: median(rounds),
    max: Math.max(0, ...rounds),
    time: mean(runs.map((r) => r.simTime)),
    timeouts: runs.filter((r) => r.cause === 'timeout').length,
    exfil: runs.filter((r) => r.cause === 'exfil').length,
    kills: mean(runs.map((r) => r.kills)),
    accuracy: sum(runs.map((r) => r.hits)) / Math.max(1, sum(runs.map((r) => r.shots))),
  };
}

function observations(runs, groups) {
  const obs = [];
  const brains = count(runs, (r) => r.brain);
  if (brains.some(([b]) => b === 'stand-in')) obs.push('These runs used the AUTOPILOT\'s STAND-IN brain (not the web\'s botBrain.js): it walks straight, has no reaction delay, no EVADE / RECOVER and no nav graph. Read every number below as the stand-in\'s, not as a player\'s.');
  for (const [d, rs] of groups) {
    const s = summary(rs);
    const mode = count(rs, (r) => r.round)[0];
    const downs = rs.filter(isDown);
    const top = count(downs, (r) => r.cause)[0];
    obs.push(`${d}: ${s.n} runs reached a median round ${f1(s.median)} (mean ${f2(s.mean)}, max ${s.max}); the most common last round is ${mode[0]} (${mode[1]} runs, ${pct(mode[1] / s.n)}).` + (top ? ` The most common cause of DOWN is ${top[0]} (${top[1]} of ${downs.length} downs).` : ''));
  }
  const timeouts = runs.filter((r) => r.cause === 'timeout');
  if (timeouts.length) {
    const at = count(timeouts, (r) => r.round).map(([k, n]) => `round ${k} ×${n}`).join(', ');
    obs.push(`${timeouts.length} of ${runs.length} runs hit the sim-time cap alive (a stall, not a win): ${at}.`);
  }
  const kg = killsByGun(runs);
  const totalKills = sum(kg.map((x) => x[1]));
  if (kg.length) obs.push(`Kills by gun: ${kg.slice(0, 4).map(([g, n]) => `${g} ${pct(n / Math.max(1, totalKills))}`).join(', ')} of ${totalKills} kills.`);
  const noBuy = runs.filter((r) => !(r.perks || []).length && !(r.guns || []).length).length;
  obs.push(`${noBuy} of ${runs.length} runs bought no gun and no perk.`);
  const firstPerk = runs.filter((r) => (r.perks || []).length).map((r) => r.perks[0].round);
  if (firstPerk.length) obs.push(`The first perk came in round ${f1(mean(firstPerk))} on average (${firstPerk.length} runs bought one).`);
  const bank = runs.map((r) => r.earned - r.spent);
  obs.push(`Points left unspent at the end: mean ${Math.round(mean(bank))} of a mean ${Math.round(mean(runs.map((r) => r.earned)))} earned.`);
  const holds = timeouts.filter((r) => r.holding).length;
  if (holds) obs.push(`${holds} of the ${timeouts.length} timeouts ended in a HOLD round with the ring not held (objectives.js: the round can't clear until it is).`);
  const holdDowns = runs.filter((r) => isDown(r) && r.roundType === 'hold').length;
  if (holdDowns) obs.push(`${holdDowns} of ${runs.filter(isDown).length} downs came in a HOLD round.`);
  return obs;
}

// --- NIGHT20 P0-3 (builder P): the pacing trace (each line's `pace`: one row per round reached, Game/Bot/Farm.cs) ---------
const PHASES = ['build', 'peak', 'fade', 'relax'];

/** the pace rows of every run (a line from before NIGHT20 has none) */
const paceRows = (runs) => runs.flatMap((r) => (r.pace || []).map((p) => ({ ...p, _run: r })));

/** per round type: rounds played, median fight and break (s), the phases' shares of the fight, downs and their rate */
function pacingByType(runs) {
  const g = new Map();
  for (const p of paceRows(runs)) {
    if (!g.has(p.type)) g.set(p.type, []);
    g.get(p.type).push(p);
  }
  const order = ['normal', 'hold', 'gold', 'swarm', 'boss', 'exfil'];
  const keys = [...order.filter((k) => g.has(k)), ...[...g.keys()].filter((k) => !order.includes(k)).sort()];
  return keys.map((type) => {
    const rows = g.get(type);
    const fought = rows.filter((p) => p.fight > 0);
    const fight = sum(rows.map((p) => p.fight));
    const share = Object.fromEntries(PHASES.map((ph) => [ph, fight > 0 ? sum(rows.map((p) => p[ph])) / fight : 0]));
    const downs = sum(rows.map((p) => p.downs));
    return { type, rounds: rows.length, fight: median(fought.map((p) => p.fight)), brk: median(rows.map((p) => p.break)), share, downs, downRate: rows.length ? downs / rows.length : 0, hurt: median(fought.map((p) => p.hurt)) };
  });
}

/** per round number: the type (the most common), runs that reached it, median fight (s), RELAX share, downs */
function pacingByRound(runs) {
  const out = [];
  const rows = paceRows(runs);
  const max = Math.max(0, ...rows.map((p) => p.round));
  for (let k = 1; k <= max; k++) {
    const at = rows.filter((p) => p.round === k);
    const fought = at.filter((p) => p.fight > 0);
    const fight = sum(at.map((p) => p.fight));
    out.push({ round: k, type: count(at, (p) => p.type)[0]?.[0] ?? '-', runs: at.length, fight: median(fought.map((p) => p.fight)), relax: fight > 0 ? sum(at.map((p) => p.relax)) / fight : 0, downs: sum(at.map((p) => p.downs)) });
  }
  return out;
}

// --- NIGHT20 A3 (builder P2): THE BEACON (each line's `beacons`: round, taken, elites, eliteKilled, cleared) ----------------
/**
 * Beacons stood and taken; the reward earned (the beacon's elite killed and its round cleared: the reveal opens in the next
 * break) per taken beacon; downs per taken-beacon round against the same rounds' normal / gold rounds without a taken beacon
 * (pooled from the first beacon round, and matched round by round: each taken round weighted by its round's own rate).
 */
function beaconTable(runs) {
  const withRows = runs.filter((r) => Array.isArray(r.beacons));
  if (!withRows.length) return null;
  const all = withRows.flatMap((r) => r.beacons.map((b) => ({ ...b, _run: r })));
  const taken = all.filter((b) => b.taken);
  const rewarded = taken.filter((b) => b.eliteKilled && b.cleared);
  const downsAt = (r, k) => sum((r.pace || []).filter((p) => p.round === k).map((p) => p.downs));
  const takenKey = new Set(taken.map((b) => `${b._run.seed}:${b._run.difficulty}:${b.round}`));
  const from = all.length ? Math.min(...all.map((b) => b.round)) : 1;
  // the baseline: normal / gold rounds played from the first beacon round with no taken beacon
  const base = withRows.flatMap((r) => (r.pace || []).filter((p) => p.round >= from && (p.type === 'normal' || p.type === 'gold') && !takenKey.has(`${r.seed}:${r.difficulty}:${p.round}`)));
  const takenPlayed = taken.filter((b) => (b._run.pace || []).some((p) => p.round === b.round));
  const takenDowns = sum(takenPlayed.map((b) => downsAt(b._run, b.round)));
  const baseDowns = sum(base.map((p) => p.downs));
  const rate = (d, n) => (n ? d / n : 0);
  let matchedBase = 0;
  for (const b of takenPlayed) {
    const at = base.filter((p) => p.round === b.round);
    matchedBase += at.length ? rate(sum(at.map((p) => p.downs)), at.length) : rate(baseDowns, base.length);
  }
  return {
    runs: withRows.length, stood: all.length, taken: taken.length, played: takenPlayed.length, rewarded: rewarded.length,
    elitesCalled: sum(taken.map((b) => b.elites || 0)), eliteKilled: taken.filter((b) => b.eliteKilled).length, cleared: taken.filter((b) => b.cleared).length,
    takenDowns, takenRate: rate(takenDowns, takenPlayed.length), baseRounds: base.length, baseDowns, baseRate: rate(baseDowns, base.length),
    matchedRate: rate(matchedBase, takenPlayed.length), from,
  };
}

function writeReport(o) {
  const { runs, lines } = readRuns(o.files);
  if (!runs.length) throw new Error('no runs in ' + o.files.join(', '));
  const groups = byDifficulty(runs);
  const maxRound = Math.max(...runs.map((r) => r.round));
  const brains = count(runs, (r) => r.brain).map(([b, n]) => `${b} ×${n}`).join(', ');
  const builds = count(runs, (r) => r.build).map(([b, n]) => `${b} ×${n}`).join(', ');
  const files = o.files.map((f) => path.basename(f)).join(', ');
  if (runs.length !== lines) throw new Error('run count != JSONL count');
  const stamp = new Date().toISOString().replace('T', ' ').slice(0, 16) + ' UTC';

  // charts
  const roundsXs = Array.from({ length: maxRound }, (_, i) => String(i + 1));
  const histo = barChart('Round reached per difficulty', roundsXs, groups.map(([d, rs]) => ({ label: d, color: DIFF_COLORS[d] || P.usable, values: roundsXs.map((_, i) => rs.filter((r) => r.round === i + 1).length) })), { xLabel: 'round reached (runs)' });
  const surv = lineChart('Survival curve', maxRound, groups.map(([d, rs]) => ({ label: d, color: DIFF_COLORS[d] || P.usable, values: curve(rs, maxRound) })), { share: true, xLabel: 'share of runs that reached the round' });
  const ppr = groups.map(([d, rs]) => {
    const m = Math.max(...rs.map((r) => r.round));
    const { e, s } = pointsPerRound(rs, m);
    const xs = Array.from({ length: m }, (_, i) => String(i + 1));
    return [d, barChart(`Points per round (${d})`, xs, [{ label: 'earned', color: DIFF_COLORS[d] || P.usable, values: e }, { label: 'spent', color: P.boundary, values: s }], { xLabel: `round (mean over the runs that reached it, ${d})` }), e, s];
  });
  const kg = killsByGun(runs);
  const kills = hbarChart('Kills by gun', kg, P.usable);
  const downs = count(runs.filter(isDown), (r) => r.cause);
  const downsSvg = downs.length ? hbarChart('Downs by enemy kind', downs, P.enemy) : '<p class="dim">No run ended in a DOWN.</p>';
  const timing = buys(runs);
  const timingSvg = timing.length ? timingChart('Buy timing', timing, maxRound) : '<p class="dim">Nothing was bought.</p>';
  const obs = observations(runs, groups);

  // markdown
  const md = [];
  md.push(`# THE FARM: ${runs.length} AUTOPILOT runs`);
  md.push('');
  md.push(`- **JSONL:** ${lines} lines in ${files}; **runs in this report: ${runs.length}** (equal to the line count).`);
  md.push(`- **Brain:** ${brains}. **Build:** ${builds}. Written ${stamp} by \`Tools/farm-report.cjs\`.`);
  md.push(`- **The page:** [farm-${o.name}.html](farm-${o.name}.html) (self-contained: the charts are inline SVG).`);
  md.push('- Real HP, no god mode (NIGHT14 rule 6); the meta records nothing (rule 7). A "timeout" is a run alive at `design.farm.maxMinutes`.');
  md.push('');
  md.push('## Observations for GG');
  md.push('');
  md.push('Observations only: GG owns enemy design and the numbers. Nothing here is a change.');
  md.push('');
  for (const x of obs) md.push(`- ${x}`);
  md.push('');
  md.push('## Per difficulty');
  md.push('');
  md.push('| difficulty | runs | mean round | median | max | mean sim time (s) | timeouts | exfil | mean kills | accuracy |');
  md.push('|---|---|---|---|---|---|---|---|---|---|');
  for (const [d, rs] of groups) {
    const s = summary(rs);
    md.push(`| ${d} | ${s.n} | ${f2(s.mean)} | ${f1(s.median)} | ${s.max} | ${f1(s.time)} | ${s.timeouts} | ${s.exfil} | ${f1(s.kills)} | ${pct(s.accuracy)} |`);
  }
  md.push('');
  md.push('## Round reached (runs per round)');
  md.push('');
  md.push(`| difficulty | ${roundsXs.join(' | ')} |`);
  md.push(`|---|${roundsXs.map(() => '---').join('|')}|`);
  for (const [d, rs] of groups) md.push(`| ${d} | ${roundsXs.map((_, i) => rs.filter((r) => r.round === i + 1).length).join(' | ')} |`);
  md.push('');
  md.push('## Survival curve (share of runs that reached the round)');
  md.push('');
  md.push(`| difficulty | ${roundsXs.join(' | ')} |`);
  md.push(`|---|${roundsXs.map(() => '---').join('|')}|`);
  for (const [d, rs] of groups) md.push(`| ${d} | ${curve(rs, maxRound).map(pct).join(' | ')} |`);
  md.push('');
  md.push('## Causes (the recap\'s killer name; "timeout", "exfil")');
  md.push('');
  md.push('| cause | runs | ' + groups.map(([d]) => d).join(' | ') + ' |');
  md.push('|---|---|' + groups.map(() => '---').join('|') + '|');
  for (const [c, n] of count(runs, (r) => r.cause)) md.push(`| ${c} | ${n} | ${groups.map(([, rs]) => rs.filter((r) => r.cause === c).length).join(' | ')} |`);
  md.push('');
  md.push('## Kills by gun');
  md.push('');
  md.push('| gun | kills | share | ' + groups.map(([d]) => d).join(' | ') + ' |');
  md.push('|---|---|---|' + groups.map(() => '---').join('|') + '|');
  const totalKills = sum(kg.map((x) => x[1]));
  for (const [g, n] of kg) md.push(`| ${g} | ${n} | ${pct(n / Math.max(1, totalKills))} | ${groups.map(([, rs]) => sum(rs.map((r) => (r.killsByGun || {})[g] || 0))).join(' | ')} |`);
  md.push('');
  md.push('## Points per round (mean earned / spent over the runs that reached the round)');
  md.push('');
  for (const [d, , e, s] of ppr) {
    md.push(`- **${d}:** ` + e.map((x, i) => `R${i + 1} ${Math.round(x)} / ${Math.round(s[i])}`).join(', '));
  }
  md.push('');
  md.push('## Buy timing (perks, guns, THE FORGE, gates: the round bought)');
  md.push('');
  md.push('| item | times bought | share of runs | mean round | first | last |');
  md.push('|---|---|---|---|---|---|');
  for (const [k, rounds] of timing) md.push(`| ${k} | ${rounds.length} | ${pct(rounds.length / runs.length)} | ${f1(mean(rounds))} | ${Math.min(...rounds)} | ${Math.max(...rounds)} |`);
  const mods = count(runs.flatMap((r) => r.mods || []), (m) => m.id);
  if (mods.length) {
    md.push('');
    md.push('MODS taken (the draft\'s first card, design.autopilot.takeFirstMod): ' + mods.map(([m, n]) => `${m} ×${n}`).join(', ') + '.');
  }
  md.push('');
  // NIGHT20 P0-3: the pacing trace
  const paceType = pacingByType(runs);
  const paceRound = pacingByRound(runs);
  const paceRuns = runs.filter((r) => (r.pace || []).length).length;
  md.push('## Pacing (the director\'s curve, by round type and by round)');
  md.push('');
  if (!paceRuns) md.push('No line carries a pace trace (lines from before NIGHT20 P0-3).');
  else {
    md.push(`From ${paceRuns} of ${runs.length} runs. Seconds are sim seconds: the fight (median over the rounds fought), the break before the round; BUILD / PEAK / FADE / RELAX are the director's phases (\`Core/Director.cs\`: BUILD_UP, PEAK, PEAK_FADE, RELAX) as shares of all the fight seconds of that type; downs per round played.`);
    md.push('');
    md.push('| round type | rounds | median fight (s) | median break (s) | BUILD | PEAK | FADE | RELAX | downs | downs / round | median damage taken |');
    md.push('|---|---|---|---|---|---|---|---|---|---|---|');
    for (const t of paceType) md.push(`| ${t.type} | ${t.rounds} | ${f1(t.fight)} | ${f1(t.brk)} | ${pct(t.share.build)} | ${pct(t.share.peak)} | ${pct(t.share.fade)} | ${pct(t.share.relax)} | ${t.downs} | ${f2(t.downRate)} | ${f1(t.hurt)} |`);
    md.push('');
    md.push('| round | type | runs | median fight (s) | RELAX share | downs |');
    md.push('|---|---|---|---|---|---|');
    for (const x of paceRound) md.push(`| ${x.round} | ${x.type} | ${x.runs} | ${f1(x.fight)} | ${pct(x.relax)} | ${x.downs} |`);
  }
  md.push('');
  // NIGHT20 A3: THE BEACON
  const bt = beaconTable(runs);
  const beaconRows = bt ? [
    ['beacons stood', bt.stood], ['taken by the bot', bt.taken], ['taken rounds played', bt.played],
    ['elites called', bt.elitesCalled], ['the elite killed', bt.eliteKilled], ['the round cleared', bt.cleared],
    ['reward earned (elite killed and round cleared)', `${bt.rewarded} of ${bt.taken} (${pct(bt.taken ? bt.rewarded / bt.taken : 0)})`],
    ['downs / taken-beacon round', `${bt.takenDowns} / ${bt.played} = ${f2(bt.takenRate)}`],
    [`downs / normal or gold round from round ${bt.from} without one`, `${bt.baseDowns} / ${bt.baseRounds} = ${f2(bt.baseRate)}`],
    ['the same rounds\' rate, matched round by round', f2(bt.matchedRate)],
    ['taken vs matched (x)', bt.matchedRate > 0 ? f2(bt.takenRate / bt.matchedRate) : '-'],
  ] : [];
  md.push('## THE BEACON (design.adds.beacon)');
  md.push('');
  if (!bt) md.push('No line carries beacons (lines from before NIGHT20 A3).');
  else {
    md.push(`From ${bt.runs} runs. A beacon's reward is its elite's kill (the reveal in the next break) in a round the run cleared; downs per round from each line's pace rows.`);
    md.push('');
    md.push('| | |');
    md.push('|---|---|');
    for (const [k, v] of beaconRows) md.push(`| ${k} | ${v} |`);
  }
  md.push('');

  // html
  const css = `body{margin:0;background:${P.bg};color:${P.text};font:14px/1.5 system-ui,-apple-system,"Segoe UI",sans-serif}
main{max-width:780px;margin:0 auto;padding:24px 16px 48px}h1{font-size:24px;margin:0 0 8px}h2{font-size:17px;margin:28px 0 8px;color:${P.usable}}
.card{background:${P.panel};border:1px solid ${P.boundary}33;border-radius:8px;padding:12px 14px;margin:10px 0}
table{border-collapse:collapse;width:100%;font-size:13px}th,td{border-bottom:1px solid ${P.boundary}26;padding:4px 6px;text-align:right}
th:first-child,td:first-child{text-align:left}th{color:${P.dim};font-weight:600}.dim{color:${P.dim}}ul{padding-left:20px}
.scroll{overflow-x:auto}`;
  const tbl = (head, rows) => `<div class="scroll"><table><tr>${head.map((h) => `<th>${esc(h)}</th>`).join('')}</tr>${rows.map((r) => `<tr>${r.map((c) => `<td>${esc(c)}</td>`).join('')}</tr>`).join('')}</table></div>`;
  const html = `<!doctype html>
<html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>SKYGRIND farm ${esc(o.name)}</title><style>${css}</style></head>
<body><main>
<h1>THE FARM: ${runs.length} AUTOPILOT runs</h1>
<p class="dim">JSONL: ${lines} lines in ${esc(files)}; runs in this report: ${runs.length}. Brain: ${esc(brains)}. Build: ${esc(builds)}. ${esc(stamp)}.${P.fromConfig ? '' : ' (PortExport/config.json not read: the palette is the documented one.)'}</p>
<div class="card"><h2 style="margin-top:0">Observations for GG</h2><p class="dim">Observations only: GG owns enemy design and the numbers. Nothing here is a change.</p><ul>${obs.map((x) => `<li>${esc(x)}</li>`).join('')}</ul></div>
<h2>Per difficulty</h2>${tbl(['difficulty', 'runs', 'mean round', 'median', 'max', 'mean sim time (s)', 'timeouts', 'exfil', 'mean kills', 'accuracy'], groups.map(([d, rs]) => {
    const s = summary(rs);
    return [d, s.n, f2(s.mean), f1(s.median), s.max, f1(s.time), s.timeouts, s.exfil, f1(s.kills), pct(s.accuracy)];
  }))}
<h2>Round reached per difficulty</h2><div class="card">${histo}</div>
<h2>Survival curve</h2><div class="card">${surv}</div>
<h2>Points per round</h2>${ppr.map(([, svg]) => `<div class="card">${svg}</div>`).join('')}
<h2>Kills by gun</h2><div class="card">${kills}</div>
<h2>Downs by enemy kind</h2><div class="card">${downsSvg}</div>
<h2>Buy timing</h2><div class="card">${timingSvg}<p class="dim">Dots: runs that bought it in that round (larger: more). The bar: the mean round.</p></div>
<h2>Pacing by round type</h2>${paceRuns ? tbl(['round type', 'rounds', 'median fight (s)', 'median break (s)', 'BUILD', 'PEAK', 'FADE', 'RELAX', 'downs', 'downs / round', 'median damage'], paceType.map((t) => [t.type, t.rounds, f1(t.fight), f1(t.brk), pct(t.share.build), pct(t.share.peak), pct(t.share.fade), pct(t.share.relax), t.downs, f2(t.downRate), f1(t.hurt)])) : '<p class="dim">No line carries a pace trace.</p>'}
<h2>Pacing by round</h2>${paceRuns ? tbl(['round', 'type', 'runs', 'median fight (s)', 'RELAX share', 'downs'], paceRound.map((x) => [x.round, x.type, x.runs, f1(x.fight), pct(x.relax), x.downs])) : ''}
<h2>THE BEACON</h2>${bt ? tbl(['', ''], beaconRows) : '<p class="dim">No line carries beacons.</p>'}
</main></body></html>
`;
  fs.mkdirSync(o.out, { recursive: true });
  const mdPath = path.join(o.out, `FARM-${o.name}.md`);
  const htmlPath = path.join(o.out, `farm-${o.name}.html`);
  fs.writeFileSync(mdPath, md.join('\n'));
  fs.writeFileSync(htmlPath, html);
  console.log(`farm-report: ${lines} JSONL lines -> ${runs.length} runs (${groups.map(([d, rs]) => `${d} ${rs.length}`).join(', ')}) -> ${mdPath}, ${htmlPath}`);
}

function writeDiff(o) {
  if (o.files.length !== 2) throw new Error('diff takes two JSONL files: a.jsonl b.jsonl');
  const A = readRuns([o.files[0]]);
  const B = readRuns([o.files[1]]);
  const an = path.basename(o.files[0]);
  const bn = path.basename(o.files[1]);
  const ga = new Map(byDifficulty(A.runs));
  const gb = new Map(byDifficulty(B.runs));
  const diffs = [...new Set([...ga.keys(), ...gb.keys()])].sort((x, y) => DIFFS.indexOf(x) - DIFFS.indexOf(y));
  const md = [];
  const sections = [];
  md.push(`# THE FARM, a diff: A ${an} (${A.lines} lines) against B ${bn} (${B.lines} lines)`);
  md.push('');
  md.push(`- A: ${A.runs.length} runs, brain ${count(A.runs, (r) => r.brain).map(([b, n]) => `${b} ×${n}`).join(', ')}, build ${count(A.runs, (r) => r.build).map(([b]) => b).join(', ')}.`);
  md.push(`- B: ${B.runs.length} runs, brain ${count(B.runs, (r) => r.brain).map(([b, n]) => `${b} ×${n}`).join(', ')}, build ${count(B.runs, (r) => r.build).map(([b]) => b).join(', ')}.`);
  md.push(`- The page: [farm-${o.name}.html](farm-${o.name}.html). Deltas are B − A. Observations only: GG owns the numbers.`);
  md.push('');
  md.push('| difficulty | runs A / B | mean round A | B | Δ | median A | B | timeouts A / B |');
  md.push('|---|---|---|---|---|---|---|---|');
  for (const d of diffs) {
    const a = summary(ga.get(d) || []);
    const b = summary(gb.get(d) || []);
    md.push(`| ${d} | ${a.n} / ${b.n} | ${f2(a.mean)} | ${f2(b.mean)} | ${signed(b.mean - a.mean)} | ${f1(a.median)} | ${f1(b.median)} | ${a.timeouts} / ${b.timeouts} |`);
  }
  for (const d of diffs) {
    const ra = ga.get(d) || [];
    const rb = gb.get(d) || [];
    const max = Math.max(1, ...ra.map((r) => r.round), ...rb.map((r) => r.round));
    const ca = curve(ra, max);
    const cb = curve(rb, max);
    md.push('');
    md.push(`## ${d}`);
    md.push('');
    md.push('Survival curve (share of runs that reached the round):');
    md.push('');
    md.push(`| | ${Array.from({ length: max }, (_, i) => i + 1).join(' | ')} |`);
    md.push(`|---|${ca.map(() => '---').join('|')}|`);
    md.push(`| A | ${ca.map(pct).join(' | ')} |`);
    md.push(`| B | ${cb.map(pct).join(' | ')} |`);
    md.push(`| Δ | ${cb.map((v, i) => signed((v - ca[i]) * 100, (x) => `${Math.round(x)} pt`)).join(' | ')} |`);
    md.push('');
    const ka = new Map(killsByGun(ra));
    const kb = new Map(killsByGun(rb));
    const ta = Math.max(1, sum([...ka.values()]));
    const tb = Math.max(1, sum([...kb.values()]));
    const guns = [...new Set([...ka.keys(), ...kb.keys()])].sort((x, y) => (kb.get(y) || 0) + (ka.get(y) || 0) - (kb.get(x) || 0) - (ka.get(x) || 0));
    md.push('| gun | kills share A | B | Δ |');
    md.push('|---|---|---|---|');
    const rowsK = guns.map((g) => [g, (ka.get(g) || 0) / ta, (kb.get(g) || 0) / tb]);
    for (const [g, sa, sb] of rowsK) md.push(`| ${g} | ${pct(sa)} | ${pct(sb)} | ${signed((sb - sa) * 100, (x) => `${Math.round(x)} pt`)} |`);
    md.push('');
    const causes = [...new Set([...ra.map((r) => r.cause), ...rb.map((r) => r.cause)])].sort();
    md.push('| cause | A | B |');
    md.push('|---|---|---|');
    for (const c of causes) md.push(`| ${c} | ${ra.filter((r) => r.cause === c).length} | ${rb.filter((r) => r.cause === c).length} |`);
    const svg = lineChart(`Survival curve, ${d}: A against B`, max, [
      { label: `A ${an}`, color: P.boundary, values: ca, dash: '5 4' },
      { label: `B ${bn}`, color: DIFF_COLORS[d] || P.usable, values: cb },
    ], { share: true, xLabel: 'share of runs that reached the round' });
    const kbars = barChart(`Kills by gun (share), ${d}`, guns, [
      { label: 'A', color: P.boundary, values: rowsK.map((r) => r[1]) },
      { label: 'B', color: DIFF_COLORS[d] || P.usable, values: rowsK.map((r) => r[2]) },
    ], { fmt: pct });
    sections.push(`<h2>${esc(d)}: A ${esc(f2(summary(ra).mean))} → B ${esc(f2(summary(rb).mean))} mean round (${esc(signed(summary(rb).mean - summary(ra).mean))})</h2><div class="card">${svg}</div><div class="card">${kbars}</div>`);
  }
  md.push('');
  const css = `body{margin:0;background:${P.bg};color:${P.text};font:14px/1.5 system-ui,-apple-system,"Segoe UI",sans-serif}
main{max-width:780px;margin:0 auto;padding:24px 16px 48px}h1{font-size:22px}h2{font-size:16px;margin:24px 0 8px;color:${P.usable}}
.card{background:${P.panel};border:1px solid ${P.boundary}33;border-radius:8px;padding:12px 14px;margin:10px 0}.dim{color:${P.dim}}`;
  const html = `<!doctype html>
<html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>SKYGRIND farm ${esc(o.name)}</title><style>${css}</style></head>
<body><main><h1>THE FARM, a diff</h1>
<p class="dim">A: ${esc(an)} (${A.lines} JSONL lines, ${A.runs.length} runs). B: ${esc(bn)} (${B.lines} lines, ${B.runs.length} runs). Deltas are B − A. Observations only: GG owns the numbers.</p>
${sections.join('\n')}
</main></body></html>
`;
  fs.mkdirSync(o.out, { recursive: true });
  const mdPath = path.join(o.out, `FARM-${o.name}.md`);
  const htmlPath = path.join(o.out, `farm-${o.name}.html`);
  fs.writeFileSync(mdPath, md.join('\n'));
  fs.writeFileSync(htmlPath, html);
  console.log(`farm-report diff: A ${A.lines} lines (${A.runs.length} runs), B ${B.lines} lines (${B.runs.length} runs) -> ${mdPath}, ${htmlPath}`);
}

const o = args(process.argv.slice(2));
if (!o.files.length) {
  console.error('usage: node Tools/farm-report.cjs [--name n14] [--out Reports] a.jsonl [b.jsonl ...]\n       node Tools/farm-report.cjs diff [--name diff] [--out Reports] a.jsonl b.jsonl');
  process.exit(2);
}
if (o.diff) writeDiff(o);
else writeReport(o);
