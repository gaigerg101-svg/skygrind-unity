#!/usr/bin/env node
// NIGHT64 builder G (slate U1 "One order at a time", design.n64.order64): a second meter beside the in-game counter ("[order64]" lines), read
// from a reel's telemetry.jsonl (FeelProbe: every recorded frame's UI texts with their alpha). Per file: the frames where hud47's fight line
// ("UI/HUD 33/Fight line 47/Text") and bossbar43's hint ("UI/HUD 33/Boss bar 43/Hint/Text") are BOTH drawn (alpha > 0, text not empty) with
// the same text (case and edge spaces ignored): the duplicate order gg-proxy #3 saw (Logs/n63/gg-proxy.md), by text.
//   node Tools/night/n64-u1-dup.cjs <telemetry.jsonl> ...        one line per file
//   node Tools/night/n64-u1-dup.cjs --selftest                  the planted cases (PLANTED GREEN / PLANTED RED)
// A same-run held frame is never in telemetry.jsonl (FeelProbe skips it), so a row-on same-run run reads its on frames only.
// The red (n63-b12's boss63 telemetry, NIGHT63's HUD): bi 1173, bk 391, bp 205, bs 205, bf 0 of 1529 frames.
const fs = require('fs');

function count(lines) {
  let frames = 0, lineFrames = 0, hintFrames = 0, dup = 0;
  const texts = {};
  for (const l of lines) {
    let o;
    try { o = JSON.parse(l); } catch (e) { continue; }
    if (!o || o.type !== 'frame' || !Array.isArray(o.ui)) continue;
    frames++;
    let fl = null, hi = null;
    for (const u of o.ui) {
      if (!u || typeof u.path !== 'string' || u.text == null || !(u.alpha > 0) || String(u.text).trim() === '') continue;
      if (/\/Fight line 47\/Text$/.test(u.path)) fl = String(u.text);
      if (/\/Boss bar 43\/Hint\/Text$/.test(u.path)) hi = String(u.text);
    }
    if (fl) lineFrames++;
    if (hi) hintFrames++;
    if (fl && hi && fl.trim().toLowerCase() === hi.trim().toLowerCase()) { dup++; texts[hi] = (texts[hi] || 0) + 1; }
  }
  return { frames, lineFrames, hintFrames, dup, texts };
}

function line(name, c) {
  return `[u1dup] ${name}: frames ${c.frames} | fight line drawn ${c.lineFrames} | bar hint drawn ${c.hintFrames} | both drawn with the same text ${c.dup}`
    + ` | by text ${JSON.stringify(c.texts)}`;
}

if (process.argv[2] === '--selftest') {
  const fr = (ui) => JSON.stringify({ type: 'frame', ui });
  const L = (t, a) => ({ path: 'UI/HUD 33/Fight line 47/Text', alpha: a, text: t });
  const H = (t, a) => ({ path: 'UI/HUD 33/Boss bar 43/Hint/Text', alpha: a, text: t });
  const S = 'STAGGERED: HOLD LEFT CLICK, SLUG IT';
  const planted = [
    JSON.stringify({ type: 'scenario', name: 'planted' }),
    fr([L(S, 1), H(S, 1)]),                         // 1 a duplicate
    fr([L('Staggered: hold left click, slug it', 0.5), H(S + ' ', 1)]), // 2 a duplicate (case, an edge space)
    fr([L(S, 0), H(S, 1)]),                         // the line faded: no
    fr([L(S, 1), H(S, 0)]),                         // the hint hidden (order64 on): no
    fr([L('KEEP MOVING!', 1), H(S, 1)]),            // another line: no
    fr([L(S, 1)]),                                  // no hint drawn: no
    fr([{ path: 'UI/HUD 33/Fight line 47/Shadow', alpha: 1, text: S }, H(S, 1)]), // a shadow twin is not the line: no
    'not json',
  ];
  const c = count(planted);
  const ok = c.frames === 7 && c.dup === 2 && c.lineFrames === 5 && c.hintFrames === 5 && c.texts[S] === 1 && c.texts[S + ' '] === 1;
  console.log(line('planted', c));
  console.log(ok ? 'PLANTED GREEN (7 frames, 2 duplicates)' : 'PLANTED RED (want 7 frames, 2 duplicates, line 5, hint 5)');
  process.exit(ok ? 0 : 1);
}

const files = process.argv.slice(2);
if (files.length === 0) { console.log('usage: node Tools/night/n64-u1-dup.cjs <telemetry.jsonl> ... | --selftest'); process.exit(2); }
for (const f of files) {
  let txt;
  try { txt = fs.readFileSync(f, 'utf8'); } catch (e) { console.log(`[u1dup] ${f}: NOT READ (${e.message})`); continue; }
  console.log(line(f, count(txt.split(/\r?\n/).filter(Boolean))));
}
