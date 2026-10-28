#!/usr/bin/env node
// NIGHT69 builder T, slice Y3 (Logs/n69/slate.json Y3; the ROOM list's M4 line; FORGE69 scout.md X5; gg-proxy NIGHT68 #1 "the old room with a
// patch"): how much of the SHIPPED room a rebuilt room keeps. Node only (no dotnet, writes nothing).
//   node Tools/night/n69-patch-share.cjs --level <L> <id> [--at <sha>] [--base <file>] [--room <file>]
//       reads Design/levels/n67/<L>/rooms-base/<id>.json (the shipped room, gen-slots --export: "n57-kiln's room exactly") and
//       Design/levels/n67/<L>/rooms/<id>.json (the rebuild); --at <sha> reads both from git at that commit; --base / --room take other files.
//       Prints the row (pieces kept / total, boxes / ramps / pads / pits kept / total, rect / spawn / exit same, waves same), each kept piece,
//       and the last line `n69-patch-share: PASS|PATCH <id> (<share> % kept)`: PASS at <= 25 % of the shipped room's pieces kept (exit 0),
//       PATCH over it (exit 1); exit 2 on a missing file or a bad argument.
//   node Tools/night/n69-patch-share.cjs --cases
//       the planted cases: a copy of rooms-base as the room (text changed, pieces reordered: 100 %, PATCH); the same copy with one box moved
//       1 m (one piece fewer kept); NIGHT68's shipped g7 / v2 at the t0 sha (0 %, PASS); the seven NIGHT68 rooms at the t0 sha against the
//       scout's table (scout.md X5). Exit 0 when every case is as expected.
// Pieces: boxes, ramps, pads and pits (the four kinds a room file builds with; grapple / tunnels are printed when present, never counted).
// A shipped piece is KEPT when the rebuild has a geometry-equal piece of its kind (each rebuilt piece matches at most one shipped piece):
//   box  min, max                 ramp  from, to, width, rise
//   pad  at, target               pit   rect, floorY, lava (absent = false)
// Everything else (job, why, note, kind, zone, apexAbove, the order) is ignored. The share is kept / the shipped room's pieces.
// Inside a slot the rect, the doors (the links) and the id / kind are fixed by gen-slots (gen-slots.cjs:118, :189, :348-357), so "from scratch"
// means 0 % kept inside the fixed rect; the spawn is not locked by the tool (the slate's GUARD forbids moving it toward the verb's line).
'use strict';
const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const ROOT = path.join(__dirname, '../..');
const T0 = 'd9e22d48a1c37364d92227eda020adaa706c9bb2'; // NIGHT69's t0 sha: NIGHT68's final room files (unchanged since d82f6b09)
const MAX_SHARE = 25; // the ROOM list's M4: a rebuilt room keeps at most 25 % of the shipped room's pieces
const KINDS = ['boxes', 'ramps', 'pads', 'pits'];
const EPS = 1e-9;

const numEq = (a, b) => typeof a === 'number' && typeof b === 'number' && Math.abs(a - b) <= EPS;
const vecEq = (a, b) => Array.isArray(a) && Array.isArray(b) && a.length === b.length && a.every((v, i) => numEq(v, b[i]));
const GEOM = {
  boxes: (a, b) => vecEq(a.min, b.min) && vecEq(a.max, b.max),
  ramps: (a, b) => vecEq(a.from, b.from) && vecEq(a.to, b.to) && numEq(a.width, b.width) && numEq(a.rise, b.rise),
  pads: (a, b) => vecEq(a.at, b.at) && vecEq(a.target, b.target),
  pits: (a, b) => vecEq(a.rect, b.rect) && numEq(a.floorY, b.floorY) && !!a.lava === !!b.lava,
};
const fmtV = (v) => (Array.isArray(v) ? '(' + v.join(', ') + ')' : String(v));
const DESC = {
  boxes: (p) => 'box ' + fmtV(p.min) + '..' + fmtV(p.max),
  ramps: (p) => 'ramp ' + fmtV(p.from) + '->' + fmtV(p.to) + ' w ' + p.width + ' rise ' + p.rise,
  pads: (p) => 'pad ' + fmtV(p.at) + '->' + fmtV(p.target),
  pits: (p) => 'pit ' + fmtV(p.rect) + ' floorY ' + p.floorY + (p.lava ? ' lava' : ''),
};
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);

function readFile(rel) {
  return JSON.parse(fs.readFileSync(path.isAbsolute(rel) ? rel : path.join(ROOT, rel), 'utf8'));
}
function readAt(sha, rel) {
  const txt = cp.execFileSync('git', ['show', sha + ':' + rel], { cwd: ROOT, encoding: 'utf8', maxBuffer: 64 * 1024 * 1024 });
  return JSON.parse(txt);
}

// the comparison: { id, kinds: {k: {kept, total}}, kept, total, share, rect, spawn, exit, waves, keptList, extra }
function compare(id, base, room) {
  const res = { id, kinds: {}, kept: 0, total: 0, keptList: [], extra: [] };
  for (const k of KINDS) {
    const bs = base[k] || [], rs = (room[k] || []).slice();
    const used = new Array(rs.length).fill(false);
    let kept = 0;
    for (const b of bs) {
      const j = rs.findIndex((r, i) => !used[i] && GEOM[k](b, r));
      if (j >= 0) { used[j] = true; kept++; res.keptList.push(DESC[k](b) + (b.job ? ' (shipped job: ' + String(b.job).slice(0, 60) + ')' : '')); }
    }
    res.kinds[k] = { kept, total: bs.length, rebuilt: rs.length };
    res.kept += kept; res.total += bs.length;
  }
  for (const k of ['grapple', 'tunnels']) {
    const nb = (base[k] || []).length, nr = (room[k] || []).length;
    if (nb || nr) res.extra.push(k + ' ' + nb + ' shipped / ' + nr + ' rebuilt (not counted)');
  }
  const br = base.row || {}, rr = room.row || {};
  res.rect = same(br.rect, rr.rect); res.spawn = same(br.spawn, rr.spawn); res.exit = same(br.exit, rr.exit); res.waves = same(br.waves, rr.waves);
  res.share = res.total > 0 ? (100 * res.kept) / res.total : 0;
  res.verdict = res.share <= MAX_SHARE + EPS ? 'PASS' : 'PATCH';
  return res;
}

function row(r) {
  const k = r.kinds;
  return '| ' + r.id + ' | ' + r.kept + ' / ' + r.total + ' (' + Math.round(r.share) + ' %) | boxes ' + k.boxes.kept + ' / ' + k.boxes.total
    + ', ramps ' + k.ramps.kept + ' / ' + k.ramps.total + ', pads ' + k.pads.kept + ' / ' + k.pads.total + ', pits ' + k.pits.kept + ' / ' + k.pits.total
    + ' | rect ' + (r.rect ? 'same' : 'CHANGED') + ', spawn ' + (r.spawn ? 'same' : 'changed') + ', exit ' + (r.exit ? 'same' : 'changed')
    + ' | waves ' + (r.waves ? 'same' : 'changed') + ' |';
}
function last(r) { return 'n69-patch-share: ' + r.verdict + ' ' + r.id + ' (' + Math.round(r.share) + ' % kept)'; }
function header() {
  return '| room | shipped pieces kept | by kind (kept / shipped) | rect / spawn / exit | waves |\n|---|---|---|---|---|';
}

function print(r, label) {
  console.log('n69-patch-share: room ' + r.id + ' | ' + label);
  console.log(header());
  console.log(row(r));
  for (const s of r.keptList) console.log('  kept: ' + s);
  for (const s of r.extra) console.log('  note: ' + s);
  console.log('  rebuilt room pieces: boxes ' + r.kinds.boxes.rebuilt + ', ramps ' + r.kinds.ramps.rebuilt + ', pads ' + r.kinds.pads.rebuilt + ', pits ' + r.kinds.pits.rebuilt
    + ' | the rule: PASS at <= ' + MAX_SHARE + ' % of the shipped room\'s pieces kept (geometry-equal; job / why text ignored)');
  console.log(last(r));
}

function cases() {
  let ok = 0, n = 0;
  const check = (name, got, want) => { n++; const good = got === want; if (good) ok++; console.log('  case ' + name + ': ' + got + ' (expected ' + want + ': ' + (good ? 'OK' : 'WRONG') + ')'); };
  console.log('n69-patch-share --cases: the planted cases (rooms read from git at the t0 sha ' + T0.slice(0, 8) + ')');
  // 1. a copy of the shipped room as the rebuild: every piece kept, the text and the order changed (ignored)
  const base = readAt(T0, 'Design/levels/n67/kiln/rooms-base/kb3.json');
  const copy = JSON.parse(JSON.stringify(base));
  for (const k of KINDS) { (copy[k] || []).forEach((p) => { p.job = 'rewritten'; p.why = 'rewritten'; }); copy[k] = (copy[k] || []).reverse(); }
  const c1 = compare('kb3', base, copy);
  console.log('  ' + row(c1)); console.log('  ' + last(c1));
  check('a copy of rooms-base/kb3.json as the room (text rewritten, order reversed)', Math.round(c1.share) + ' % ' + c1.verdict, '100 % PATCH');
  // 2. the copy with one box moved 1 m east: that box is no longer kept
  const moved = JSON.parse(JSON.stringify(copy));
  moved.boxes[0].min[0] += 1; moved.boxes[0].max[0] += 1;
  const c2 = compare('kb3', base, moved);
  check('the copy with one box moved 1 m', c2.kept + ' / ' + c2.total, (c1.total - 1) + ' / ' + c1.total);
  // 3. one pit's lava flag flipped on a copy of kb3c (a pit is rect, floorY and lava)
  const b3c = readAt(T0, 'Design/levels/n67/kiln/rooms-base/kb3c.json');
  const lavaOff = JSON.parse(JSON.stringify(b3c)); lavaOff.pits[0].lava = !lavaOff.pits[0].lava;
  const c3 = compare('kb3c', b3c, lavaOff);
  check('a copy of rooms-base/kb3c.json with its pit\'s lava flag flipped', c3.kinds.pits.kept + ' / ' + c3.kinds.pits.total + ' pits', '0 / 1 pits');
  // 4. NIGHT68's g7 / v2 (rebuilt from scratch): 0 %, PASS
  for (const [L, id] of [['iceline', 'g7'], ['spire', 'v2']]) {
    const r = compare(id, readAt(T0, 'Design/levels/n67/' + L + '/rooms-base/' + id + '.json'), readAt(T0, 'Design/levels/n67/' + L + '/rooms/' + id + '.json'));
    check('NIGHT68\'s ' + id + ' at t0', Math.round(r.share) + ' % ' + r.verdict, '0 % PASS');
  }
  // 5. the seven NIGHT68 rooms against the scout's table (scout.md X5: pieces kept / total, boxes kept / total, rect / spawn / exit, waves)
  const scout = [
    ['kiln', 'kb3c', '7 / 16', '5 / 10', 'same', 'changed'], ['kiln', 'kb2', '3 / 14', '3 / 10', 'same', 'same'],
    ['kiln', 'kb3d', '11 / 15', '9 / 11', 'same', 'changed'], ['kiln', 'kb3', '9 / 16', '6 / 12', 'same', 'same'],
    ['iceline', 'g8', '1 / 13', '1 / 8', 'same', 'changed'], ['iceline', 'g7', '0 / 24', '0 / 15', 'same', 'changed'],
    ['spire', 'v2', '0 / 17', '0 / 11', 'same', 'changed'],
  ];
  console.log('  the seven NIGHT68 rooms at t0:');
  console.log(header().replace(/^/gm, '  '));
  for (const [L, id, pk, bk, rse, wv] of scout) {
    const r = compare(id, readAt(T0, 'Design/levels/n67/' + L + '/rooms-base/' + id + '.json'), readAt(T0, 'Design/levels/n67/' + L + '/rooms/' + id + '.json'));
    console.log('  ' + row(r));
    const got = r.kept + ' / ' + r.total + ', boxes ' + r.kinds.boxes.kept + ' / ' + r.kinds.boxes.total + ', rect / spawn / exit ' + (r.rect && r.spawn && r.exit ? 'same' : 'not all same') + ', waves ' + (r.waves ? 'same' : 'changed');
    check(id + ' vs the scout', got, pk + ', boxes ' + bk + ', rect / spawn / exit ' + rse + ', waves ' + wv);
  }
  console.log('n69-patch-share --cases: ' + ok + ' / ' + n + ' as expected' + (ok === n ? ' (GREEN)' : ' (RED)'));
  return ok === n ? 0 : 1;
}

function main() {
  const args = process.argv.slice(2);
  const KNOWN = ['--level', '--at', '--base', '--room', '--cases'];
  const bad = args.filter((a) => a.startsWith('--') && !KNOWN.includes(a));
  if (bad.length) { console.log('n69-patch-share: REFUSED: unknown option ' + bad.join(', ') + ' (known: ' + KNOWN.join(' ') + ')'); return 2; }
  if (args.includes('--cases')) return cases();
  const opt = (k) => { const i = args.indexOf(k); return i >= 0 && i + 1 < args.length && !args[i + 1].startsWith('--') ? args[i + 1] : null; };
  const L = opt('--level') || process.env.L67, at = opt('--at'), baseF = opt('--base'), roomF = opt('--room');
  const ids = args.filter((a, i) => !a.startsWith('--') && !['--level', '--at', '--base', '--room'].includes(args[i - 1]));
  if (!L || ids.length !== 1) { console.log('n69-patch-share: usage: node Tools/night/n69-patch-share.cjs --level <L> <id> [--at <sha>] [--base <file>] [--room <file>] | --cases'); return 2; }
  const id = ids[0];
  const baseRel = baseF || 'Design/levels/n67/' + L + '/rooms-base/' + id + '.json';
  const roomRel = roomF || 'Design/levels/n67/' + L + '/rooms/' + id + '.json';
  let base, room;
  try {
    base = at && !baseF ? readAt(at, baseRel) : readFile(baseRel);
    room = at && !roomF ? readAt(at, roomRel) : readFile(roomRel);
  } catch (e) { console.log('n69-patch-share: ERROR ' + id + ': ' + String(e.message).split('\n')[0]); return 2; }
  if (room.room && room.room !== id) { console.log('n69-patch-share: ERROR ' + id + ': the room file names room ' + room.room); return 2; }
  const r = compare(id, base, room);
  print(r, 'shipped ' + baseRel + (at && !baseF ? '@' + at.slice(0, 8) : '') + ' vs rebuilt ' + roomRel + (at && !roomF ? '@' + at.slice(0, 8) : ''));
  return r.verdict === 'PASS' ? 0 : 1;
}

process.exitCode = main();
