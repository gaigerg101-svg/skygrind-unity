// NIGHT72 builder L (L5; GG 2026-10-26 "textures overlapping that bug out"): the offline coplanar-face finder over a level's DRAWN stone boxes, and the grow plan that
// separates what it finds. Two faces that lie on one plane, face the same way and overlap both draw there, so the depth test flips between them as the view moves.
// What is drawn (SurvivalBuilder.Build): the floor box (collision.boxes kind floor, ungrown) and every data.boxes entry in order, each grown outward by Bright71.Grow
// (NIGHT71: 2 / 4 / 6 mm by index mod 3). Only boxes can be scanned here: wedges, neon, decor and the room views are other geometry (not covered; said in the output).
//   node Tools/night/n72-zscan.cjs <level.json>... [--tol 0.001] [--min-area 0.01] [--json out.json] [--classes] [--list N]
//   node Tools/night/n72-zscan.cjs --selftest
// For each level it prints three counts of coplanar overlapping same-facing face pairs (a pair is flagged when its gap AFTER growth is under --tol, default 1 mm, and the
// rectangles overlap by at least --min-area, default 0.01 m2; a gap of 1 cm is NOT a pair):
//   raw        the boxes as authored, no growth
//   mod3       after the shipped growth (index mod 3: 2 / 4 / 6 mm): what GG sees today
//   plan       after the grow plan (GrowPlan72.cs is this algorithm in C#: the same classes): the pairs left, which the slate wants at 0, each listed with its reason
// THE PLAN: boxes in index order get a class 0..K-1 (grown by step x (1 + class)); a box takes the first class whose separation from every earlier conflicting
// box is at least SEP_GOOD (4 mm), else the class with the best separation (a pair under SEP_MIN is reported "left"). A conflict is a same-side face pair, overlap >= min-area,
// raw gap within BAND. Boxes with no conflict stay at class 0 (2 mm). Constants are Bright71's (UnityTuning.Bright71GrowStep 0.002) and GrowPlan72's.
'use strict';
const fs = require('fs');

const STEP = 0.002, K = 6, SEP_GOOD = 0.004, SEP_MIN = 0.002;
const BAND = (K - 1) * STEP + SEP_GOOD + 1e-9; // a raw gap farther than this can never be closed by a class difference

function load(file) {
  const j = JSON.parse(fs.readFileSync(file, 'utf8'));
  const data = j.data || {};
  const boxes = (data.boxes || []).map((b, i) => ({ i, min: b.min.slice(), max: b.max.slice(), kind: b.kind || '' }));
  let floor = null;
  for (const b of ((j.collision || {}).boxes || [])) if (b.kind === 'floor') floor = { min: b.min.slice(), max: b.max.slice(), kind: 'floor' };
  return { boxes, floor, name: j.name || file };
}

// every same-side face pair of boxes a < b whose rectangles overlap by >= minArea and whose raw gap is within `band`: {a, b, ax, side, g (b - a), area}
function conflicts(boxes, minArea, band) {
  const out = [];
  for (let a = 0; a < boxes.length; a++) {
    const A = boxes[a];
    for (let b = a + 1; b < boxes.length; b++) {
      const B = boxes[b];
      for (let ax = 0; ax < 3; ax++) {
        const o = [0, 1, 2].filter((k) => k !== ax);
        let area = 1;
        for (const k of o) area *= Math.max(0, Math.min(A.max[k], B.max[k]) - Math.max(A.min[k], B.min[k]));
        if (area < minArea) continue;
        for (const side of ['min', 'max']) {
          const g = B[side][ax] - A[side][ax];
          if (Math.abs(g) <= band) out.push({ a, b, ax, side, g, area });
        }
      }
    }
  }
  return out;
}

// the gap of a pair after growth: p' = p + s e (s = +1 for a max face, -1 for a min face; e = the box's growth)
function gapAfter(c, ea, eb) {
  const s = c.side === 'max' ? 1 : -1;
  return Math.abs(c.g + s * (eb - ea));
}

function growMod3(n) {
  const e = new Array(n);
  for (let i = 0; i < n; i++) e[i] = STEP * (1 + (i % 3));
  return e;
}

// the plan: classes by index order, greedy (the C# twin is Assets/Scripts/Game/GrowPlan72.cs)
function plan(boxes, minArea) {
  const conf = conflicts(boxes, minArea, BAND);
  const byB = boxes.map(() => []);
  for (const c of conf) byB[c.b].push(c);
  const cls = new Array(boxes.length).fill(0);
  for (let b = 0; b < boxes.length; b++) {
    if (byB[b].length === 0) continue;
    let best = -1, bestSep = -1;
    for (let c = 0; c < K; c++) {
      let sep = Infinity;
      for (const p of byB[b]) sep = Math.min(sep, gapAfter(p, STEP * (1 + cls[p.a]), STEP * (1 + c)));
      if (sep >= SEP_GOOD - 1e-12) { best = c; bestSep = sep; break; }
      if (sep > bestSep + 1e-12) { best = c; bestSep = sep; }
    }
    cls[b] = best;
  }
  return { cls, conf };
}

function flagged(conf, e, tol) {
  const out = [];
  for (const c of conf) {
    const gap = gapAfter(c, e[c.a], e[c.b]);
    if (gap < tol) out.push({ ...c, gapAfter: gap });
  }
  return out;
}

function scan(file, opt) {
  const L = load(file);
  const n = L.boxes.length;
  const zero = new Array(n).fill(0);
  // the raw conflicts are pairs with raw gap under tol; the drawn conflicts need the wider band (a class difference can cancel a small raw gap)
  const conf = conflicts(L.boxes, opt.minArea, BAND);
  const raw = flagged(conf, zero, opt.tol);
  const m3 = flagged(conf, growMod3(n), opt.tol);
  const pl = plan(L.boxes, opt.minArea);
  const e = pl.cls.map((c) => STEP * (1 + c));
  const left = flagged(pl.conf, e, opt.tol);
  const soft = pl.conf.filter((c) => { const g = gapAfter(c, e[c.a], e[c.b]); return g >= opt.tol && g < SEP_GOOD - 1e-12; });
  // the drawn floor (ungrown) against boxes: a box face at the floor's top plane is grown above it by >= 2 mm, so it never ties; counted only to say so
  let floorTies = 0;
  if (L.floor) for (let i = 0; i < n; i++) if (Math.abs(L.boxes[i].max[1] - L.floor.max[1]) < 1e-9 && L.boxes[i].min[1] < L.floor.max[1]) floorTies++;
  const hist = pl.cls.reduce((h, c) => { h[c] = (h[c] || 0) + 1; return h; }, {});
  return { name: L.name, file, boxes: n, raw, mod3: m3, plan: left, soft, hist, cls: pl.cls, area: (list) => list.reduce((s, c) => s + c.area, 0), floorTies, nonFloorBoxes: L.boxes.length };
}

function describe(r, c, boxes) {
  const A = boxes[c.a], B = boxes[c.b];
  return 'boxes ' + c.a + ' (' + A.kind + ') & ' + c.b + ' (' + B.kind + ') ' + c.side + '-' + 'xyz'[c.ax] + ' at ' + (c.side === 'max' ? A.max[c.ax] : A.min[c.ax]) + ' / ' + (c.side === 'max' ? B.max[c.ax] : B.min[c.ax]) + ', overlap ' + c.area.toFixed(2) + ' m2, gap after ' + c.gapAfter.toFixed(4);
}

function selftest() {
  let ok = true;
  const mk = (list) => list.map((b, i) => ({ i, min: b[0], max: b[1], kind: 't' }));
  const check = (name, cond, extra) => { console.log((cond ? 'ok   ' : 'FAIL ') + name + (extra ? ' (' + extra + ')' : '')); ok = ok && cond; };
  // 1. a planted pair: two boxes whose tops lie on y = 3 and overlap 4 x 4 m
  let boxes = mk([[[0, 0, 0], [4, 3, 4]], [[2, 1, 2], [8, 3, 8]]]);
  let conf = conflicts(boxes, 0.01, BAND);
  let raw = flagged(conf, [0, 0], 0.001);
  check('a planted coplanar pair (tops on y = 3, 4 m2 overlap) is found raw', raw.length === 1 && raw[0].ax === 1 && raw[0].side === 'max' && Math.abs(raw[0].area - 4) < 1e-9, raw.length + ' found');
  // 2. under mod 3 boxes 0 and 1 grow 2 and 4 mm: the planted pair is separated (4 - 2 = 2 mm): not flagged
  check('mod 3 separates indices 0 and 1 (2 mm)', flagged(conf, growMod3(2), 0.001).length === 0);
  // 3. indices 0 and 3 share a class under mod 3: still coplanar after the shipped growth; the plan separates them
  boxes = mk([[[0, 0, 0], [4, 3, 4]], [[10, 0, 0], [12, 1, 2]], [[20, 0, 0], [22, 1, 2]], [[2, 1, 2], [8, 3, 8]]]);
  conf = conflicts(boxes, 0.01, BAND);
  check('indices 0 and 3 stay coplanar under mod 3 (the left pair)', flagged(conf, growMod3(4), 0.001).length === 1);
  const pl = plan(boxes, 0.01);
  check('the plan separates them by at least 4 mm', flagged(pl.conf, pl.cls.map((c) => STEP * (1 + c)), 0.001).length === 0 && pl.cls[3] !== pl.cls[0], 'classes ' + pl.cls.join(','));
  // 4. a 1 cm gap is not a pair
  boxes = mk([[[0, 0, 0], [4, 3, 4]], [[2, 1, 2], [8, 3.01, 8]]]);
  conf = conflicts(boxes, 0.01, BAND);
  check('a 0.01 m gap is not found', flagged(conf, [0, 0], 0.001).length === 0);
  // 5. a near pair a class difference would cancel: tops at 3.000 and 3.002 (box 1 is 2 mm farther out): under classes (0, 1) the grown gap is 0 + 2 - ... = 4 mm; (1, 0) would cancel to 0
  boxes = mk([[[0, 0, 0], [4, 3, 4]], [[2, 1, 2], [8, 3.002, 8]]]);
  conf = conflicts(boxes, 0.01, BAND);
  check('a 2 mm raw gap is closed by growth class (1, 0) and is flagged there', flagged(conf, [STEP * 2, STEP], 0.001).length === 1 && flagged(conf, [STEP, STEP * 2], 0.001).length === 0);
  const p2 = plan(boxes, 0.01);
  check('the plan keeps it open at least 4 mm', flagged(p2.conf, p2.cls.map((c) => STEP * (1 + c)), 0.001).length === 0 && gapAfter(p2.conf[0], STEP * (1 + p2.cls[0]), STEP * (1 + p2.cls[1])) >= SEP_GOOD - 1e-9, 'classes ' + p2.cls.join(','));
  // 6. opposite sides (A.max.x == B.min.x: touching) are not a same-side pair
  boxes = mk([[[0, 0, 0], [4, 3, 4]], [[4, 0, 0], [8, 3, 4]]]);
  conf = conflicts(boxes, 0.01, BAND);
  check('touching boxes (A max x = B min x) share no same-facing face on x', conf.every((c) => c.ax !== 0));
  console.log('SELFTEST n72-zscan: ' + (ok ? 'PASS' : 'FAIL'));
  return ok ? 0 : 1;
}

function main(argv) {
  if (argv.includes('--selftest')) process.exit(selftest());
  const opt = { tol: 0.001, minArea: 0.01, list: 6 };
  const files = [];
  let jsonOut = null, classes = false;
  for (let i = 0; i < argv.length; i++) {
    const a = argv[i];
    if (a === '--tol') opt.tol = parseFloat(argv[++i]);
    else if (a === '--min-area') opt.minArea = parseFloat(argv[++i]);
    else if (a === '--json') jsonOut = argv[++i];
    else if (a === '--list') opt.list = parseInt(argv[++i], 10);
    else if (a === '--classes') classes = true;
    else files.push(a);
  }
  if (!files.length) { console.log('usage: n72-zscan.cjs <level.json>... [--tol m] [--min-area m2] [--json out] [--classes] [--list N]   |   --selftest'); process.exit(2); }
  const all = {};
  let totalLeft = 0;
  for (const f of files) {
    const r = scan(f, opt);
    const L = load(f);
    console.log(r.name + ' (' + f.replace(/\\/g, '/').split('/').pop() + '): ' + r.boxes + ' drawn boxes | coplanar overlapping pairs: raw ' + r.raw.length + ' (' + r.area(r.raw).toFixed(0) + ' m2), after mod 3 ' + r.mod3.length + ' (' + r.area(r.mod3).toFixed(0) + ' m2), after the plan ' + r.plan.length + ' (' + r.area(r.plan).toFixed(0) + ' m2) | classes ' + JSON.stringify(r.hist) + (r.soft.length ? ' | ' + r.soft.length + ' pairs at 1-4 mm' : ''));
    for (const c of r.plan.slice(0, opt.list)) console.log('  LEFT ' + describe(r, c, L.boxes) + ' (no class of ' + K + ' separates it: reason = more than ' + K + ' mutually coplanar boxes on that plane, or a cancelling raw gap)');
    if (classes) console.log('  classes: ' + r.cls.join(''));
    totalLeft += r.plan.length;
    all[r.name] = { file: f, boxes: r.boxes, raw: r.raw.length, mod3: r.mod3.length, plan: r.plan.length, soft: r.soft.length, classes: r.cls };
  }
  console.log('TOTAL pairs left after the plan: ' + totalLeft + ' (tol ' + opt.tol + ' m, min overlap ' + opt.minArea + ' m2, K ' + K + ', step ' + STEP + ' m)');
  if (jsonOut) fs.writeFileSync(jsonOut, JSON.stringify({ tol: opt.tol, minArea: opt.minArea, K, STEP, SEP_GOOD, SEP_MIN, levels: all }, null, 1));
  process.exit(0);
}

main(process.argv.slice(2));
