#!/usr/bin/env node
// night-context: the session-start brief and the close check for a project built in nightly waves.
// Node only, no npm dependencies. Reads (never writes) the project's files:
//   docs/memory/{OWNER,THREADS,LESSONS,CORE}.md, docs/memory/ledger/wave-NN.md,
//   Research/<date>-waveNN/GG-REQUEST.md, NIGHTnn-LOG.md (the close table), NIGHTnn.md (the night's prompt, cards),
//   HANDOFF.md, DECIDEnn.md.
// Usage:
//   node night-context.cjs brief [--root DIR] [--night N] [--card ID] [--prompt FILE] [--text "card words"]
//   node night-context.cjs close [--root DIR] [--night N] [--log FILE]
// brief exits 0. close exits 1 when something is missing (so a script can gate on it), 0 when everything landed.
'use strict';
const fs = require('fs');
const path = require('path');

// ---------- args ----------
const argv = process.argv.slice(2);
const mode = argv[0] && !argv[0].startsWith('--') ? argv[0] : 'brief';
function opt(name, dflt) {
  const i = argv.indexOf('--' + name);
  return i >= 0 && i + 1 < argv.length ? argv[i + 1] : dflt;
}
const ROOT = path.resolve(opt('root', process.cwd()));
// One page: lines are cut at this width so the brief stays readable in a terminal and cheap in context.
const WIDTH = Number(opt('width', 160));
// A thread open this many nights or more is listed (wave-memory's rule: aged 2+ goes in the next P0 or is parked).
const AGED = Number(opt('aged', 2));
// Threads older than this are folded into one id line (they are the never-consolidated w18-w21 table rows).
const ANCIENT = Number(opt('ancient', 10));
// Lessons printed for a card (the top matches by keyword weight).
const MAX_LESSONS = Number(opt('lessons', 6));

// ---------- small helpers ----------
function rd(p) {
  try { return fs.readFileSync(p, 'utf8').replace(/\r\n/g, '\n'); } catch (e) { return null; }
}
function ls(dir) {
  try { return fs.readdirSync(dir); } catch (e) { return []; }
}
function cut(s, n) {
  s = String(s).replace(/\s+/g, ' ').trim();
  n = n || WIDTH;
  return s.length > n ? s.slice(0, n - 3) + '...' : s;
}
const out = [];
function say(s) { out.push(s === undefined ? '' : s); }
function rel(p) { return path.relative(ROOT, p).split(path.sep).join('/') || p; }

// Split at a separator only at parenthesis / backtick depth 0.
function splitDepth0(text, isSep) {
  const parts = [];
  let depth = 0, tick = false, start = 0;
  for (let i = 0; i < text.length; i++) {
    const c = text[i];
    if (c === '`') tick = !tick;
    else if (!tick && c === '(') depth++;
    else if (!tick && c === ')') depth = Math.max(0, depth - 1);
    else if (!tick && depth === 0 && isSep(text, i)) {
      parts.push(text.slice(start, i + 1));
      start = i + 1;
    }
  }
  if (start < text.length) parts.push(text.slice(start));
  return parts.map((s) => s.trim()).filter(Boolean);
}
const sentenceSep = (t, i) => t[i] === '.' && /\s/.test(t[i + 1] || ' ') && !/\b(e\.g|i\.e|vs)$/.test(t.slice(Math.max(0, i - 4), i));
const clauseSep = (t, i) => t[i] === ';';
// Ids at depth 0 only (an id inside parentheses is evidence, not the subject).
function depth0(text) {
  let depth = 0, tick = false, s = '';
  for (const c of text) {
    if (c === '`') { tick = !tick; continue; }
    if (!tick && c === '(') { depth++; continue; }
    if (!tick && c === ')') { depth = Math.max(0, depth - 1); continue; }
    if (depth === 0 && !tick) s += c;
  }
  return s;
}
function tid(n) { return 'T-' + String(n).padStart(2, '0'); }
function threadIds(text) {
  const ids = [];
  const re = /T-(\d+)(?:\.\.T-(\d+))?/g;
  let m;
  while ((m = re.exec(text))) {
    const a = +m[1], b = m[2] ? +m[2] : a;
    for (let k = a; k <= b; k++) ids.push(k);
  }
  return ids;
}

// ---------- the night number ----------
function nightNumber() {
  if (opt('night')) return Number(opt('night'));
  let n = 0;
  for (const f of ls(ROOT)) { const m = /^NIGHT(\d+)(?:-LOG)?\.md$/.exec(f); if (m) n = Math.max(n, +m[1]); }
  for (const d of ls(path.join(ROOT, 'Research'))) { const m = /wave(\d+)$/.exec(d); if (m) n = Math.max(n, +m[1]); }
  return n;
}
const NIGHT = nightNumber();

// ---------- GG's words ----------
function ggRequests() {
  const dirs = ls(path.join(ROOT, 'Research'))
    .filter((d) => /wave\d+$/.test(d) && fs.existsSync(path.join(ROOT, 'Research', d, 'GG-REQUEST.md')))
    .sort((a, b) => (+/wave(\d+)$/.exec(a)[1]) - (+/wave(\d+)$/.exec(b)[1]) || a.localeCompare(b));
  return dirs.map((d) => path.join(ROOT, 'Research', d, 'GG-REQUEST.md'));
}
function parseRequest(file) {
  const t = rd(file) || '';
  const title = (t.match(/^# (.*)$/m) || [, ''])[1];
  const quote = t.split('\n').filter((l) => l.startsWith('>')).map((l) => l.replace(/^>\s?/, '')).join(' ');
  const asks = [];
  for (const l of t.split('\n')) {
    const m = /^\|\s*(G\d+)\s*\|\s*(.*?)\s*\|\s*(.*?)\s*\|/.exec(l);
    if (m) asks.push({ id: m[1], words: m[2], area: m[3] });
  }
  return { file, title, quote, asks };
}
function ownerTail(n) {
  const t = rd(path.join(ROOT, 'docs/memory/OWNER.md')) || '';
  const rows = t.split('\n').filter((l) => /^\|\s*20\d\d-\d\d-\d\d/.test(l));
  return rows.slice(-n).map((l) => l.split('|').map((c) => c.trim()).filter(Boolean));
}

// ---------- threads ----------
function parseThreads() {
  const file = path.join(ROOT, 'docs/memory/THREADS.md');
  const t = rd(file);
  const map = new Map(); // id -> {id, item, first, status, note}
  if (!t) return { file, map, untracked: [], missing: true };
  const get = (id) => { if (!map.has(id)) map.set(id, { id, item: '', first: null, status: 'open', note: '' }); return map.get(id); };
  const untracked = [];
  // 1) the table rows
  for (const l of t.split('\n')) {
    const m = /^\|\s*T-(\d+)\s*\|\s*(.*?)\s*\|\s*(.*?)\s*\|/.exec(l);
    if (!m) continue;
    const th = get(+m[1]);
    th.item = m[2];
    const w = /w(\d+)/.exec(m[3]);
    th.first = w ? (/before/.test(m[3]) ? +w[1] - 1 : +w[1]) : null;
    th.firstText = m[3];
  }
  // 2) the per-wave paragraphs
  const paras = t.split(/\n\s*\n/).map((p) => p.replace(/\n/g, ' ').trim()).filter((p) => p && !p.startsWith('|') && !p.startsWith('#'));
  for (const p of paras) {
    const head = /^w(\d+)((?:\s*\+\s*w\d+)*)/.exec(p);
    const wave = head ? Math.max(...(head[0].match(/\d+/g) || []).map(Number)) : null;
    for (const s of splitDepth0(p, sentenceSep)) {
      const top = depth0(s);
      const ids = threadIds(top);
      if (/\bclosed\b|\bmet in w\d+|\bsuperseded\b/i.test(top) && !/^\s*New:/i.test(top)) {
        for (const id of ids) { const th = get(id); th.status = 'closed'; th.note = cut(s, 90); }
        continue;
      }
      if (/\bparked\b/i.test(s)) {
        for (const id of ids) { const th = get(id); if (th.status !== 'closed') th.status = 'parked'; }
      }
      if (/\bNew:|new threads/i.test(s)) {
        const body = s.replace(/^.*?(New:|new threads\.)/i, '');
        for (const c of splitDepth0(body, clauseSep)) {
          const m = /T-(\d+)\s+(.*)$/.exec(c);
          if (!m) continue;
          const th = get(+m[1]);
          if (!th.item) th.item = m[2].replace(/[;.]\s*$/, '');
          if (th.first === null && wave !== null) th.first = wave;
        }
      }
      const ren = /T-(\d+) renamed:\s*(.*)$/.exec(s);
      if (ren) { const th = get(+ren[1]); th.item = ren[2].replace(/[.]\s*$/, ''); }
      const also = /also open:\s*(.*)$/i.exec(s);
      if (also && wave !== null) {
        for (const c of splitDepth0(also[1], (x, i) => x[i] === ',')) untracked.push({ item: c.replace(/[,.]\s*$/, ''), first: wave });
      }
    }
  }
  return { file, map, untracked, text: t };
}

// ---------- close tables ----------
function logs() {
  return ls(ROOT).map((f) => /^NIGHT(\d+)-LOG\.md$/.exec(f)).filter(Boolean)
    .map((m) => ({ n: +m[1], file: path.join(ROOT, m[0]) })).sort((a, b) => b.n - a.n);
}
function closeLog(maxNight) {
  if (opt('log')) return { n: NIGHT, file: path.resolve(opt('log')) };
  for (const l of logs()) {
    if (maxNight && l.n > maxNight) continue;
    if (/^##\s.*close table/im.test(rd(l.file) || '')) return l;
  }
  return null;
}
function parseClose(file) {
  const t = rd(file) || '';
  const lines = t.split('\n');
  const start = lines.findIndex((l) => /^##\s.*close table/i.test(l));
  const rows = [];
  const fates = [];
  if (start < 0) return { rows, fates, found: false };
  let i = start + 1;
  for (; i < lines.length; i++) {
    const l = lines[i];
    if (/^##\s/.test(l)) break;
    if (!l.startsWith('|') || /^\|\s*-+/.test(l)) continue;
    const cells = l.split('|').slice(1, -1).map((c) => c.trim());
    if (cells.length < 3 || /^card$/i.test(cells[0])) continue;
    rows.push({ card: cells[0], result: cells[1], reason: cells.slice(2).join(' | ') });
  }
  // the fates block (INVENTORY fates / GG asks) right after the close table: "A6 SHORT (...) · A7 ..."
  const rest = lines.slice(i).join('\n');
  const fateText = rest.split(/\n##\s/).slice(0, 2).join(' ');
  for (const f of fateText.replace(/\n/g, ' ').split(/\s·\s/)) {
    const m = /\b([A-Z]\d+)\s+(SHORT|OFF|RETIRED|DONE|ON)\b(.*)$/.exec(f.trim());
    if (m) fates.push({ id: m[1], state: m[2], text: f.trim() });
  }
  return { rows, fates, found: true, text: t };
}
function shortItems(close) {
  const items = [];
  for (const r of close.rows) {
    if (!/SHORT/.test(r.result + ' ' + r.reason)) continue;
    const clauses = splitDepth0(r.reason, clauseSep);
    const inner = /\(([^)]*SHORT[^)]*)\)/.exec(r.result);
    const whole = /^SHORT/.test(r.result.trim());
    let label = r.card, focus = '';
    if (inner) {
      const key = inner[1].replace(/\bSHORT\b/, '').replace(/^\s*the\s+/, '').trim();
      label = r.card + ': ' + inner[1];
      focus = clauses.find((c) => key && c.toLowerCase().includes(key.toLowerCase().split(/\s+/)[0])) || clauses[clauses.length - 1] || '';
      items.push({ label, text: key + ' ' + focus, show: focus, subject: key, src: 'close table' });
    } else if (whole) {
      items.push({ label: r.card + ': SHORT', text: clauses[0] || r.reason, show: clauses[0] || r.reason, src: 'close table' });
    } else {
      for (const c of clauses.filter((c) => /SHORT/.test(c))) items.push({ label: r.card + ': ' + cut(c, 60), text: c, show: c, src: 'close table' });
    }
  }
  for (const f of close.fates.filter((f) => f.state === 'SHORT')) items.push({ id: f.id, label: 'fate ' + cut(f.text, 90), text: f.text, show: f.text, src: 'fates' });
  return items;
}
// A fate repeats a close-table row when its id is named in the row, its keys overlap the row's, or three of its words are the row's.
function dedupe(items) {
  const rows = items.filter((x) => x.src !== 'fates');
  const kept = [...rows];
  for (const f of items.filter((x) => x.src === 'fates')) {
    const fk = keysOf(f.text);
    const fw = new Set(words(f.text.replace(/^\S+\s+SHORT/, '')));
    const dup = kept.some((r) => (f.id && new RegExp('\\b' + f.id + '\\b').test(r.label + ' ' + r.text)) ||
      fk.some((k) => keysOf(r.text, r.subject).includes(k)) ||
      [...fw].filter((w) => r.text.toLowerCase().includes(w)).length >= 3);
    if (!dup) kept.push(f);
  }
  return kept;
}

// ---------- HANDOFF ----------
// Two shapes are read: the night-block shape ("## NIGHTnn closed" with a "- Open:" bullet; older blocks kept below it) and the
// handoff-skill shape ("## Not done" bullets, older blocks only in git history). `scope` is the text a check searches: the newest
// night block + Not done + Next steps when old blocks are kept in the file, else the whole file.
function parseHandoff(t) {
  t = (t || '').replace(/\r\n/g, '\n');
  const parts = t.split(/\n(?=## )/);
  const title = (t.match(/^# (.*)$/m) || [, ''])[1];
  const nightBlocks = parts.filter((p) => /^## NIGHT\d+ closed/.test(p));
  const newest = nightBlocks[0] || '';
  const notDone = parts.find((p) => /^## (Not done|Open)\b/i.test(p)) || '';
  const next = parts.find((p) => /^## Next steps/i.test(p)) || '';
  const open = [];
  const add = (s) => { for (const c of splitDepth0(s.replace(/\n\s*/g, ' '), clauseSep)) { const x = c.replace(/[.;]\s*$/, '').trim(); if (x) open.push(x); } };
  const m = /- Open:\s*([\s\S]*?)(?=\n- |\n## |$)/.exec(newest);
  if (m) add(m[1]);
  for (const b of notDone.split(/\n- /).slice(1)) add(b);
  const scope = nightBlocks.length > 1 ? [newest, notDone, next].join('\n') : t;
  const blockTitle = (newest.match(/^## (.*)$/m) || [, ''])[1];
  return { text: t, scope, open, title: blockTitle ? title + ' / ' + blockTitle : title };
}
function handoffBlock() {
  return parseHandoff(rd(path.join(ROOT, 'HANDOFF.md')));
}
// The committed HANDOFF (git HEAD in ROOT): an open item there that the working copy and THREADS both lack was dropped by a rewrite.
function committedHandoff() {
  try {
    const cp = require('child_process');
    const t = cp.execFileSync('git', ['-C', ROOT, 'show', opt('handoff-ref', 'HEAD') + ':HANDOFF.md'], { encoding: 'utf8', stdio: ['ignore', 'pipe', 'ignore'] });
    return parseHandoff(t);
  } catch (e) { return null; }
}

// ---------- keys for "did it land" ----------
const STOP = new Set(('the a an and or of to in on at for with by from as is are was were be been it its this that these those not no ' +
  'into over under one two three each every per than then when after before while only also still more most less very same other ' +
  'their there here what which who whom why how all any some such can could would should will may might must done short open off ' +
  'player frame frames night builder builders lead row rows harness checks check measured since need needs needing never seen ' +
  'built tonight round rounds level levels first last next fight reel reels card cards').split(/\s+/));
// Keys in tiers: the first tier that finds anything wins, so a row name (aim41) beats a boss named only as evidence (THE LOOM).
function keysOf(text, subject) {
  const t = text.replace(/(?:[\w.-]+\/)+/g, ''); // drop directory prefixes: "Research/2026-10-14-wave42/X.md" -> "X.md"
  const tiers = [
    (t.match(/\b[a-z][a-zA-Z]*\d{2}[a-zA-Z]*\b/g) || []).filter((m) => m.length >= 5 && !/^n\d+$/.test(m)),
    (t.match(/\b[A-Z][A-Za-z0-9]+Tests\b/g) || []),
    (t.match(/\bTHE [A-Z]{3,}(?: [A-Z]{3,})?\b/g) || []).map((m) => m.replace(/ (HOLD|SHOOT|RIDE|CATCH)$/, '')),
    (t.match(/\b[A-Z][A-Z0-9]+(?:-[A-Z0-9]+)*\.md\b/g) || []).filter((m) => !/^NIGHT\d+(-LOG)?\.md$/.test(m)),
    // weak: the SHORT's own word ("suite"), a night-local id ("A11", "D-3") and the level ids it names
    [...(subject && /^([a-z]{4,}|[A-Z]\d{1,2})$/i.test(subject) ? [subject] : []),
      ...((/^([A-Z]\d{1,2})\b/.exec(t.trim()) || []).slice(1)), ...(t.match(/\bD-\d+\b/g) || []),
      ...(t.match(/\bn\d{2}-[a-z]+\b/g) || [])],
  ];
  for (let i = 0; i < tiers.length; i++) {
    if (!tiers[i].length) continue;
    const keys = [...new Set(tiers[i])];
    keys.weak = i >= 4; // a plain word or a level id: a hit must share another word of the item too
    return keys;
  }
  return [];
}
function words(text) {
  return (text.toLowerCase().match(/[a-z][a-z0-9'-]{3,}/g) || []).map((w) => w.replace(/'s$/, '')).filter((w) => !STOP.has(w));
}
function findIn(text, item) {
  if (!text) return null;
  const keys = keysOf(item.text, item.subject);
  const paras = text.split(/\n\s*\n|\n(?=\|)|\n(?=- )|\n(?=\d+\. )/);
  const others = [...new Set(lwords(item.text))].filter((w) => !keys.some((k) => k.toLowerCase().includes(w)) && w.length >= 5);
  for (const k of keys) {
    const re = new RegExp('\\b' + k.replace(/[.*+?^${}()|[\]\\-]/g, '\\$&') + '\\b', 'i');
    const co = (x) => others.some((w) => x.toLowerCase().includes(w)) || keys.some((k2) => k2 !== k && x.includes(k2));
    const p = paras.find((x) => re.test(x) && (!keys.weak || co(x)));
    if (p) return { key: k, where: cut(p.slice(Math.max(0, p.search(re) - 40)), 110) };
  }
  if (keys.length) return null;
  // no identifier: two of the clause's longest words in one paragraph
  const ws = [...new Set(words(item.text))].sort((a, b) => b.length - a.length).slice(0, 4);
  if (ws.length < 2) return null;
  for (const p of paras) {
    const lp = p.toLowerCase();
    const hit = ws.filter((w) => lp.includes(w));
    if (hit.length >= 2) return { key: hit.join('+'), where: cut(p, 110) };
  }
  return null;
}

// ---------- lessons ----------
// Lesson matching keeps the domain words (level, player, reel, walk...) that the close check's STOP drops, and drops the
// glue words that matched every card in the first test run (they, count, show, keep, land, change...).
const LSTOP = new Set(('the a an and or of to in on at for with by from as is are was were be been it its this that these those not no ' +
  'into over under one two three each every per than then when after before while only also still more most less very same other ' +
  'their there here what which who whom why how all any some such can could would should will may might must they them count show ' +
  'later through cross keep land lands change changes make made does done short open need needs needing owns read write first last ' +
  'next night tonight builder builders card cards gg\'s like just even back down long them that\'s give gives took take takes ' +
  'fail fails failed list already code never stay wait night nights').split(/\s+/));
function lwords(text) {
  return (text.toLowerCase().match(/[a-z][a-z0-9'-]{3,}/g) || []).map((w) => w.replace(/'s$/, '')).filter((w) => !LSTOP.has(w));
}
function stem(w) { return w.replace(/(ing|ed|es|s)$/, '').replace(/[^a-z0-9]/g, ''); }
function parseLessons() {
  const t = rd(path.join(ROOT, 'docs/memory/LESSONS.md')) || '';
  return t.split('\n').map((l) => /^\|\s*(L-\d+)\s*\|\s*(.*?)\s*\|\s*(.*?)\s*\|\s*(.*?)\s*\|\s*(.*?)\s*\|\s*(.*?)\s*\|/.exec(l))
    .filter(Boolean).map((m) => ({ id: m[1], rule: m[2], evidence: m[3], scope: m[4], status: m[5], confirmed: m[6] }));
}
function cardText() {
  if (opt('text')) return opt('text');
  const card = opt('card');
  if (!card) return '';
  const pf = opt('prompt') ? path.resolve(opt('prompt')) : path.join(ROOT, 'NIGHT' + NIGHT + '.md');
  const t = rd(pf) || '';
  const lines = t.split('\n');
  const i = lines.findIndex((l) => new RegExp('^\\*\\*' + card + '\\b').test(l));
  if (i < 0) return '';
  const body = [lines[i]];
  for (let j = i + 1; j < lines.length; j++) {
    if (/^\*\*[A-Z]\d+\b|^<\/|^## |^<[a-z]/.test(lines[j])) break;
    body.push(lines[j]);
  }
  return body.join(' ');
}
function lessonsFor(text, lessons) {
  const q = new Set(lwords(text).map(stem).filter((w) => w.length >= 4));
  const df = new Map();
  const docs = lessons.map((l) => new Set(lwords(l.rule + ' ' + l.scope).map(stem)));
  const scopes = lessons.map((l) => new Set(lwords(l.scope).map(stem)));
  for (const d of docs) for (const w of d) df.set(w, (df.get(w) || 0) + 1);
  const N = lessons.length || 1;
  return lessons.map((l, i) => {
    const hit = [...q].filter((w) => docs[i].has(w));
    // idf weight; a word in the lesson's scope column (who it is for) counts double
    const score = hit.reduce((s, w) => s + Math.log(1 + N / df.get(w)) * (scopes[i].has(w) ? 2 : 1), 0) + (/retired/.test(l.status) ? -99 : 0);
    return { l, hit, score };
  }).filter((x) => x.hit.length >= 2 && x.score > 0).sort((a, b) => b.score - a.score).slice(0, MAX_LESSONS);
}

// ---------- memory freshness ----------
function memoryFreshness() {
  const led = ls(path.join(ROOT, 'docs/memory/ledger')).map((f) => /^wave-(\d+)\.md$/.exec(f)).filter(Boolean).map((m) => +m[1]);
  const newestLedger = led.length ? Math.max(...led) : 0;
  const closed = logs().filter((l) => /^##\s.*close table/im.test(rd(l.file) || '')).map((l) => l.n);
  const covered = new Set(led);
  const threads = rd(path.join(ROOT, 'docs/memory/THREADS.md')) || '';
  for (const m of threads.match(/^w\d+(?:\s*\+\s*w\d+)*/gm) || []) for (const n of m.match(/\d+/g)) covered.add(+n);
  const unharvested = closed.filter((n) => n > newestLedger && !covered.has(n)).sort((a, b) => a - b);
  const core = rd(path.join(ROOT, 'docs/memory/CORE.md')) || '';
  const state = (core.match(/^\*\*Its?\s*State:\*\*.*$|^State:.*$|State: .*?;/m) || [''])[0];
  return { newestLedger, unharvested, state: cut(state, 120) };
}

// =====================================================================================================
function brief() {
  say(`# night-context brief: NIGHT${NIGHT} (root ${ROOT.split(path.sep).join('/')})`);
  // 0. memory freshness (the reason we forget: an unharvested night is invisible to every later brief)
  const mf = memoryFreshness();
  if (mf.unharvested.length) say(`!! memory not harvested for ${mf.unharvested.map((n) => 'w' + n).join(', ')} (newest ledger wave-${mf.newestLedger}; THREADS has no paragraph for them): run wave-memory harvest before trusting the threads below`);
  say();
  // 1. GG's latest words
  const reqs = ggRequests();
  say('## GG, latest words');
  if (reqs.length) {
    const r = parseRequest(reqs[reqs.length - 1]);
    say(`${rel(r.file)}: ${cut(r.title, 120)}`);
    if (r.quote) say('> ' + cut(r.quote, 700));
    if (r.asks.length) say('asks: ' + r.asks.map((a) => `${a.id} ${a.area}`).join(' · '));
  } else say('(no Research/*-waveNN/GG-REQUEST.md found)');
  for (const row of ownerTail(3)) say('- OWNER ' + cut(row.join(' | '), WIDTH - 8));
  say();
  // 2. open threads aged >= AGED
  const th = parseThreads();
  say(`## Open threads aged ${AGED}+ nights (THREADS.md, age = ${NIGHT} - first seen)`);
  if (th.missing) say('(docs/memory/THREADS.md missing)');
  const open = [...th.map.values()].filter((x) => x.status !== 'closed').map((x) => ({ ...x, age: x.first === null ? null : NIGHT - x.first }));
  const aged = open.filter((x) => x.age === null || x.age >= AGED).sort((a, b) => (b.age ?? 99) - (a.age ?? 99));
  const ancient = aged.filter((x) => x.age === null || x.age >= ANCIENT);
  const recent = aged.filter((x) => x.age !== null && x.age < ANCIENT);
  for (const x of recent.sort((a, b) => a.id - b.id)) say(cut(`- ${tid(x.id)} (w${x.first}, ${x.age} nights${x.status === 'parked' ? ', parked' : ''}) ${x.item}`));
  if (ancient.length) say(cut(`- ${ancient.length} more open ${ANCIENT}+ nights, never closed or parked: ` + ancient.sort((a, b) => a.id - b.id).map((x) => tid(x.id)).join(' '), 400));
  for (const u of th.untracked.filter((u) => NIGHT - u.first >= AGED)) say(cut(`- (no T-id, w${u.first}, ${NIGHT - u.first} nights) ${u.item}`));
  // aged threads the night's prompt / DECIDE page never names
  const pf = opt('prompt') ? path.resolve(opt('prompt')) : path.join(ROOT, 'NIGHT' + NIGHT + '.md');
  const named = (rd(pf) || '') + (rd(path.join(ROOT, 'DECIDE' + NIGHT + '.md')) || '');
  if (named) {
    const namedIds = new Set(threadIds(named));
    const unnamed = recent.filter((x) => !namedIds.has(x.id));
    say(`- not named in ${path.basename(pf)} / DECIDE${NIGHT}.md (wave-memory: aged 2+ goes in P0 or is parked): ${unnamed.length} of ${recent.length} (${unnamed.map((x) => tid(x.id)).join(' ')})`);
  }
  say();
  // 3. the last close table's SHORT rows
  const cl = closeLog(opt('night') ? NIGHT - 1 : null) || closeLog();
  say('## The last close table: SHORT');
  if (cl) {
    const c = parseClose(cl.file);
    say(`${rel(cl.file)} (${c.rows.length} rows)`);
    for (const it of dedupe(shortItems(c))) say(cut(`- ${it.label}${it.src === 'close table' ? ' :: ' + it.show : ''}`));
  } else say('(no NIGHTnn-LOG.md with a "## Close table" heading)');
  const hb = handoffBlock();
  if (hb.open.length) {
    say(`HANDOFF "${cut(hb.title, 90)}", open:`);
    for (const o of hb.open) say(cut('- ' + o));
  }
  say();
  // 4. lessons for the card
  const text = cardText();
  say('## Lessons for ' + (opt('card') ? 'card ' + opt('card') : opt('text') ? '--text' : '(no --card / --text given)'));
  if (text) {
    const ls2 = lessonsFor(text, parseLessons());
    if (!ls2.length) say('(no lesson shares two keywords with the card)');
    for (const x of ls2) say(cut(`- ${x.l.id} [${x.l.scope}] ${x.l.rule}`) + `  {${x.hit.slice(0, 5).join(', ')}}`);
  } else if (opt('card')) say(`(card ${opt('card')} not found in ${path.basename(pf)}; pass --prompt FILE or --text)`);
  else say('pass --card <id> (reads the night prompt) or --text "<the card>"');
  print();
  return 0;
}

function close() {
  const cl = closeLog(opt('night') ? NIGHT : null);
  say(`# night-context close check: NIGHT${NIGHT} (root ${ROOT.split(path.sep).join('/')})`);
  let missing = 0;
  if (!cl) { say('!! MISSING: no NIGHTnn-LOG.md with a "## Close table" heading'); print(); return 1; }
  const c = parseClose(cl.file);
  say(`close table: ${rel(cl.file)} (${c.rows.length} rows, ${c.fates.length} fates)`);
  if (cl.n !== NIGHT && !opt('log')) { say(`!! MISSING: NIGHT${NIGHT}-LOG.md has no close table yet (checked NIGHT${cl.n}'s instead)`); missing++; }
  const th = parseThreads();
  const hb = handoffBlock();
  const IND = '      ';
  say(`HANDOFF: "${cut(hb.title, 110)}"`);
  if (!new RegExp('NIGHT' + cl.n + '\\b').test(hb.title)) { say(`!! MISSING: HANDOFF.md's title / newest block does not name NIGHT${cl.n}`); missing++; }
  say();
  // 1. every SHORT row and fate -> THREADS + HANDOFF
  say('## SHORT items -> THREADS.md and HANDOFF.md');
  for (const it of dedupe(shortItems(c))) {
    const keys = keysOf(it.text, it.subject);
    const inT = findIn(th.text, it), inH = findIn(hb.scope, it);
    const ok = inT && inH;
    if (!ok) missing++;
    say(cut(`${ok ? 'ok' : '!!'} ${it.label}  [keys: ${keys.join(', ') || 'words'}]`));
    say(IND + cut(`THREADS: ${inT ? inT.key + ' :: ' + inT.where : 'MISSING'}`));
    say(IND + cut(`HANDOFF: ${inH ? inH.key + ' :: ' + inH.where : 'MISSING'}`));
  }
  say();
  // 2. HANDOFF's open items -> THREADS
  say('## HANDOFF open items ("- Open:" / "## Not done") -> THREADS.md');
  if (!hb.open.length) say('(HANDOFF.md has no "- Open:" line in its newest night block and no "## Not done" section)');
  for (const o of hb.open) {
    const f = findIn(th.text, { text: o });
    if (!f) missing++;
    say(cut(`${f ? 'ok' : '!!'} ${o}`));
    if (f) say(IND + cut(`THREADS: ${f.key} :: ${f.where}`));
  }
  say();
  // 3. open items the committed HANDOFF had that a rewrite dropped (in neither the working HANDOFF nor THREADS)
  const old = committedHandoff();
  say(`## Dropped by a HANDOFF rewrite (git ${opt('handoff-ref', 'HEAD')}:HANDOFF.md open items vs the working HANDOFF + THREADS)`);
  if (!old) say('(git show failed: not a repo, or no committed HANDOFF.md)');
  else if (old.text === hb.text) say(`(the working HANDOFF.md equals ${opt('handoff-ref', 'HEAD')}'s; pass --handoff-ref <the last close's tag> to compare)`);
  else {
    let dropped = 0;
    for (const o of old.open) {
      if (findIn(hb.text, { text: o }) || findIn(th.text, { text: o })) continue;
      dropped++; missing++;
      say(cut(`!! ${o}`));
    }
    if (!dropped) say(`ok every one of ${old.open.length} committed open items is still in HANDOFF or THREADS`);
  }
  say();
  // 3. GG's asks each given a fate in the log
  const reqs = ggRequests();
  const req = reqs.map(parseRequest).find((r) => new RegExp('wave' + cl.n + '\\b').test(r.file)) || null;
  say('## GG asks -> a fate in the log');
  if (!req) say(`(no Research/*-wave${cl.n}/GG-REQUEST.md)`);
  else {
    const absent = req.asks.filter((a) => !new RegExp('\\b' + a.id + '\\b').test(c.text));
    if (absent.length) missing += absent.length;
    if (!req.asks.length) say(`(${rel(req.file)} has no "| Gn |" ask table: nothing to check)`);
    else say(`${absent.length ? '!!' : 'ok'} ${req.asks.length} asks in ${rel(req.file)}; ${absent.length ? 'no fate in the log for ' + absent.map((a) => a.id).join(' ') : 'every ask named in the log'}`);
  }
  say();
  // 4. memory harvested for this night
  say('## Memory');
  const mf = memoryFreshness();
  const ledger = fs.existsSync(path.join(ROOT, 'docs/memory/ledger', 'wave-' + cl.n + '.md'));
  if (!ledger) missing++;
  say(`${ledger ? 'ok' : '!!'} docs/memory/ledger/wave-${cl.n}.md ${ledger ? 'exists' : 'MISSING (run wave-memory harvest ' + cl.n + ')'}`);
  if (mf.unharvested.length) { missing++; say(`!! closed nights with no ledger and no THREADS paragraph: ${mf.unharvested.map((n) => 'w' + n).join(', ')}`); }
  const coreOk = new RegExp('\\bw' + cl.n + '\\b').test(mf.state);
  if (!coreOk) missing++;
  say(`${coreOk ? 'ok' : '!!'} CORE.md state line names w${cl.n}: ${mf.state || '(none)'}`);
  say();
  say(missing ? `RESULT: ${missing} missing (fix THREADS.md / HANDOFF.md / the harvest, then re-run)` : 'RESULT: every SHORT and open item landed');
  print();
  return missing ? 1 : 0;
}

function print() { process.stdout.write(out.join('\n') + '\n'); }

if (mode === 'brief') process.exitCode = brief();
else if (mode === 'close') process.exitCode = close();
else { console.error('usage: night-context.cjs brief|close [--root DIR] [--night N] [--card ID] [--prompt FILE] [--text "..."] [--log FILE]'); process.exitCode = 2; }
