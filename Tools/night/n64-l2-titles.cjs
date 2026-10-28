#!/usr/bin/env node
// NIGHT64 builder G (leftover 2, design.n64.fightLine64): a second meter beside the in-game counter ("[fight64]" lines), read from a reel's
// telemetry.jsonl (FeelProbe: every recorded frame's UI texts with their alpha). Per file: the frames where hud47's fight line ("UI/HUD 33/
// Fight line 47/Text") draws a ROOM TITLE (alpha > 0) while the boss bar is drawn ("UI/HUD 33/Boss bar 43/Name/Text"), by text. A room title
// is a room's name in the level file's campaign.rooms[].name, as posted ("1.11 THE STACK") or without its number ("THE STACK"), case and
// edge spaces ignored.
//   node Tools/night/n64-l2-titles.cjs --level Design/levels/n52-foundry.json <telemetry.jsonl> ...
//   node Tools/night/n64-l2-titles.cjs --selftest              the planted cases (PLANTED GREEN / PLANTED RED)
// A same-run held frame is never in telemetry.jsonl (FeelProbe skips it). The red (n63-b12's boss63, NIGHT63's HUD, 1529 frames each):
// bf THE WICK "THE STACK" 170, bp THE CROWN "THE SPIRE" 170, bs THE LOOM "THE NARROWS" 170, bk 0, bi 0.
const fs = require('fs');

function strip(n) { // Copy47.StripNumber: "1.11 THE STACK" -> "THE STACK", "3.2b THE STACK" -> "THE STACK"
  const m = /^(\d+)\.(\d+)([A-Za-z])? (.+)$/.exec(n);
  return m && m[4].trim().length > 0 ? m[4].trim() : n;
}

function titlesOf(names) {
  const s = new Set();
  for (const n of names) if (n && n.trim()) { s.add(n.trim().toLowerCase()); s.add(strip(n.trim()).toLowerCase()); }
  return s;
}

function count(lines, titles) {
  let frames = 0, barFrames = 0, title = 0;
  const texts = {};
  for (const l of lines) {
    let o;
    try { o = JSON.parse(l); } catch (e) { continue; }
    if (!o || o.type !== 'frame' || !Array.isArray(o.ui)) continue;
    frames++;
    let bar = false, fl = null;
    for (const u of o.ui) {
      if (!u || typeof u.path !== 'string' || !(u.alpha > 0) || u.text == null || String(u.text).trim() === '') continue;
      if (/\/Boss bar 43\/Name\/Text$/.test(u.path)) bar = true;
      if (/\/Fight line 47\/Text$/.test(u.path)) fl = String(u.text);
    }
    if (bar) barFrames++;
    if (bar && fl && titles.has(fl.trim().toLowerCase())) { title++; texts[fl] = (texts[fl] || 0) + 1; }
  }
  return { frames, barFrames, title, texts };
}

const line = (name, c) => `[l2titles] ${name}: frames ${c.frames} | boss bar drawn ${c.barFrames} | a room title drawn as the fight line with the bar ${c.title} | by text ${JSON.stringify(c.texts)}`;

const argv = process.argv.slice(2);
if (argv[0] === '--selftest') {
  const fr = (ui) => JSON.stringify({ type: 'frame', ui });
  const B = { path: 'UI/HUD 33/Boss bar 43/Name/Text', alpha: 1, text: 'THE WICK' };
  const L = (t, a) => ({ path: 'UI/HUD 33/Fight line 47/Text', alpha: a, text: t });
  const titles = titlesOf(['1.11 THE STACK', '3.2b THE SPIRE']);
  const planted = [
    fr([B, L('THE STACK', 1)]),            // 1 a title mid-fight
    fr([B, L(' the spire ', 0.4)]),        // 2 a title (copy47 stripped "3.2b", case, edge spaces)
    fr([B, L('1.11 THE STACK', 1)]),       // 3 a title as posted (copy47 off)
    fr([B, L('THE STACK', 0)]),            // hidden (fightLine64 on): no
    fr([L('THE STACK', 1)]),               // no bar (outside the fight): no
    fr([B, L('STAGGERED: EVERY HIT COUNTS', 1)]), // the boss's order: no
    fr([B, { path: 'UI/HUD 33/Fight line 47/Shadow', alpha: 1, text: 'THE STACK' }]), // not the line: no
    'not json',
  ];
  const c = count(planted, titles);
  const ok = c.frames === 7 && c.barFrames === 6 && c.title === 3 && strip('3.2b THE STACK') === 'THE STACK' && strip('THE STACK') === 'THE STACK' && strip('1.x A') === '1.x A';
  console.log(line('planted', c));
  console.log(ok ? 'PLANTED GREEN (7 frames, bar 6, 3 titles mid-fight)' : 'PLANTED RED (want 7 frames, bar 6, 3 titles)');
  process.exit(ok ? 0 : 1);
}
const li = argv.indexOf('--level');
if (li < 0 || li + 1 >= argv.length) { console.log('usage: node Tools/night/n64-l2-titles.cjs --level <level.json> <telemetry.jsonl> ... | --selftest'); process.exit(2); }
let names = [];
try {
  const j = JSON.parse(fs.readFileSync(argv[li + 1], 'utf8'));
  names = ((j.campaign && j.campaign.rooms) || []).map(r => r && r.name).filter(Boolean);
} catch (e) { console.log(`[l2titles] level ${argv[li + 1]}: NOT READ (${e.message})`); process.exit(2); }
const titles = titlesOf(names);
console.log(`[l2titles] level ${argv[li + 1]}: ${names.length} rooms`);
for (const f of argv.filter((a, i) => i !== li && i !== li + 1)) {
  let txt;
  try { txt = fs.readFileSync(f, 'utf8'); } catch (e) { console.log(`[l2titles] ${f}: NOT READ (${e.message})`); continue; }
  console.log(line(f, count(txt.split(/\r?\n/).filter(Boolean), titles)));
}
