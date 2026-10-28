// NIGHT33 builder K (design.n33.kit33; K1-arena-kit.md sections 6-8): the dressing pass. `node Tools/night/dress33.cjs <plan.json> [--out <file>]`
// dresses a level plan IN PLACE (or into --out): it removes every row it wrote before (`dress33: true`) and writes the kit's decor rows into
// `props` again, so running it twice gives the same file (idempotent; the seed is Mulberry32 from the plan id + the area id, K1 8.15).
// Builder R's regen (Tools/night/n33-regen.sh) runs it between a generator and levelgen when it exists. It never edits rooms, boxes, ramps, pads,
// doors or pits (R's shapes): decor rows are `decor: true, collider: false` (view only, PropsView29 -> Kit33Props) and add no colliders.
// Rules (checked by `dotnet run --project Tools/SimHarness -c Release -- kit33 <plan>`):
//  - every wall run (an area edge minus its door gaps): a skirt (floor) and a cornice (ceiling) along its whole length; a rib / recess / pilaster
//    (or the theme's piece) every <= 7 m; a pilaster in every corner where two runs meet
//  - every door: a frame on both faces (jambs beside the opening, never in it) and a warm lintel line above it when the area is tall enough
//  - one landmark per area on the wall facing the entry (flat on it: off the walkable floor), visible from the spawn
//  - above 3 m: an edge light line per run at 3 m, banners, trusses with hanging lamps on chains, pipe runs, the theme's ceiling pieces, added
//    until >= 45 % of the area's decor is above 3 m; then low pieces by the walls (debris, scorch, the theme's floor pieces) until 10 per 100 m2
//  - low pieces stay within 1.5 m of a wall, out of every pit, out of the 3 m lanes from each door to the exit and the ramps
//  - colours: the theme tones only (Kit33Props, UnityTuning.Kit33*): no `color` key on decor rows (never cyan)
'use strict';
const fs = require('fs');

const args = process.argv.slice(2);
const inPath = args.find((a) => !a.startsWith('--'));
if (!inPath) { console.error('usage: node Tools/night/dress33.cjs <plan.json> [--out file]'); process.exit(2); }
const oi = args.indexOf('--out');
const outPath = oi >= 0 ? args[oi + 1] : inPath;
const plan = JSON.parse(fs.readFileSync(inPath, 'utf8'));

// --- numbers (view / layout numbers; the checker's are UnityTuning.Kit33*) ---
const r3 = (v) => Math.round(v * 1000) / 1000;
const PIECE_EVERY = 6.0;     // a wall piece at least every this many metres (the check allows 8; 6 leaves room for a socket slide)
const BAND = 1.5;            // low decor within this of a wall
const LANE_HALF = 1.6;       // clear lane half width (the check uses 1.5)
const DENSITY_TARGET = 10.5; // decor per 100 m2 to fill to (the check wants 8-14)
const DENSITY_CAP = 13.0;    // fill to at most this per 100 m2 (areas >= 150 m2) ...
const HARD_CAP = 17.0;       // ... or this while the high share is still short (the check fails above 18)
const HIGH_SHARE = 0.46;     // add high pieces until this share (the check wants 0.40)
const FLAT = 0.65;           // a piece whose far face is within this of its wall is "flat on the wall" (UnityTuning.Kit33WallFlat)
// round 2: the numbers other systems own, read from the config (never typed here): the wall-ride strip (WallRideStrips26: a cyan strip on every
// long wall at stripHeight above its base, stripThickness square, stripOffset off the face) and L's floor trim (LightPlan33 Trim: a strip
// trim.width wide at trim.inset from each wall, trim.height tall)
const ucfg = JSON.parse(fs.readFileSync(require('path').join(__dirname, '..', '..', 'Design', 'unity-config.json'), 'utf8')).design;
const RIDE = ucfg.n26.wallRide, TRIM = ucfg.n24.lighting.trim;
const RIDE_GAP = 0.2;        // clearance above and below the ride strip that no wall piece covers (UnityTuning.Kit33RideGap)
const BAND_LO = r3(RIDE.stripHeight - RIDE.stripThickness / 2 - RIDE_GAP), BAND_HI = r3(RIDE.stripHeight + RIDE.stripThickness / 2 + RIDE_GAP);
const TRIM_NEAR = TRIM.inset - TRIM.width / 2; // a floor piece's far face stays inside this (0.35 m): L's trim line stays in view

function mulberry32(a) {
  return function () {
    a |= 0; a = (a + 0x6d2b79f5) | 0;
    let t = Math.imul(a ^ (a >>> 15), 1 | a);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}
function fnv(s) { let h = 2166136261; for (const ch of s) { h ^= ch.charCodeAt(0); h = Math.imul(h, 16777619) >>> 0; } return h; }

const id = plan.id || 'level';
const theme = /kiln/.test(id) ? 'kiln' : /ice/.test(id) ? 'ice' : /span/.test(id) ? 'span' : /spire/.test(id) ? 'spire' : 'foundry';
const wallH = plan.wallHeight || 14;
plan.props = (plan.props || []).filter((p) => !p.dress33);
const doors = plan.doors || [];
const pits = plan.pits || [];
const ramps = plan.ramps || [];
const out = [];
const counts = {};

function row(room, kind, x, y, z, w, h, d, yawDeg, extra) {
  const n = (counts[room.id] = (counts[room.id] || 0) + 1);
  const r = { id: `k33-${room.id}-${kind}-${n}`, room: room.id, kind, at: [r3(x), r3(y), r3(z)], size: [r3(w), r3(h), r3(d)], yawDeg: yawDeg || 0, collider: false, decor: true, dress33: true };
  if (extra) Object.assign(r, extra);
  return r;
}
// AABB of a row (yaw 0 or 90 only)
function aabb(r) {
  const turned = Math.abs(Math.sin((r.yawDeg * Math.PI) / 180)) > 0.5;
  const ex = (turned ? r.size[2] : r.size[0]) / 2, ez = (turned ? r.size[0] : r.size[2]) / 2;
  return { x0: r.at[0] - ex, x1: r.at[0] + ex, y0: r.at[1] - r.size[1] / 2, y1: r.at[1] + r.size[1] / 2, z0: r.at[2] - ez, z1: r.at[2] + ez };
}
function segDist(ax, az, bx, bz, b) {
  const len = Math.hypot(bx - ax, bz - az), n = Math.max(1, Math.ceil(len / 0.25));
  let best = Infinity;
  for (let i = 0; i <= n; i++) {
    const x = ax + ((bx - ax) * i) / n, z = az + ((bz - az) * i) / n;
    const dx = Math.max(b.x0 - x, 0, x - b.x1), dz = Math.max(b.z0 - z, 0, z - b.z1);
    best = Math.min(best, Math.hypot(dx, dz));
  }
  return best;
}
// L's light sockets (the plan's `lights`: a wall / ceiling fixture's box, LevelPlan29's axes) grown by SOCKET_CLEAR: no decor covers them
const SOCKET_CLEAR = 0.05;
const sockets = (plan.lights || []).map((l) => {
  const [w, h, d] = l.size, nm = l.normal || [0, 0, 1];
  let ex, ey, ez;
  if (Math.abs(nm[0]) > 0.5) { ex = d; ey = h; ez = w; } else if (Math.abs(nm[1]) > 0.5) { ex = w; ey = d; ez = h; } else { ex = w; ey = h; ez = d; }
  const c = SOCKET_CLEAR;
  return { floor: nm[1] > 0.5, x0: l.at[0] - ex / 2 - c, x1: l.at[0] + ex / 2 + c, y0: l.at[1] - ey / 2 - c, y1: l.at[1] + ey / 2 + c, z0: l.at[2] - ez / 2 - c, z1: l.at[2] + ez / 2 + c };
});
const hits = (b, o) => b.x0 < o.x1 && b.x1 > o.x0 && b.y0 < o.y1 && b.y1 > o.y0 && b.z0 < o.z1 && b.z1 > o.z0;
const STRIPS = ['skirt', 'cornice', 'edgeLight', 'pipes', 'wallPipe', 'inlay'];
const SLIDERS = ['rib', 'pilaster', 'fin', 'recess', 'kilnArch', 'banner', 'debris', 'scorch', 'drift', 'crystal', 'ingots'];
/** The rows with L's sockets kept clear: a strip is cut around each socket it crosses, a wall piece slides up to 2.4 m along its wall, anything else
 *  over a socket is dropped (the wash is 2 cm behind a socket's 10 cm face: it stays). */
function clearSockets(list, top, rect) {
  const outRows = [];
  for (const r of list) {
    if (r.kind === 'wash') { outRows.push(r); continue; }
    const b = aabb(r);
    const mine = sockets.filter((sk) => hits(b, sk) && !(r.kind === 'skirt' && sk.floor));
    if (mine.length === 0) { outRows.push(r); continue; }
    const turned = Math.abs(Math.sin((r.yawDeg * Math.PI) / 180)) > 0.5; // the row's w runs along z
    const ax = turned ? 2 : 0;
    if (STRIPS.includes(r.kind)) {
      let segs = [[r.at[ax] - r.size[0] / 2, r.at[ax] + r.size[0] / 2]];
      for (const sk of mine) {
        const lo = ax === 0 ? sk.x0 : sk.z0, hi = ax === 0 ? sk.x1 : sk.z1;
        segs = segs.flatMap(([s0, s1]) => (hi <= s0 || lo >= s1 ? [[s0, s1]] : [[s0, lo], [hi, s1]].filter(([q0, q1]) => q1 - q0 > 0.3)));
      }
      segs.forEach(([s0, s1], i) => {
        const c = JSON.parse(JSON.stringify(r));
        c.id = r.id + (segs.length > 1 ? '-' + String.fromCharCode(97 + i) : '');
        c.at[ax] = r3((s0 + s1) / 2); c.size[0] = r3(s1 - s0);
        outRows.push(c);
      });
      continue;
    }
    if (['rib', 'pilaster', 'fin', 'kilnArch'].includes(r.kind)) {
      // a vertical wall piece keeps its place: each socket it crosses becomes one more clear band (Kit33Props draws the piece around it)
      const c = JSON.parse(JSON.stringify(r));
      const g = c.gap ? c.gap.slice() : [];
      for (const sk of mine) g.push(r3(sk.y0), r3(sk.y1));
      c.gap = g;
      outRows.push(c);
      continue;
    }
    if (SLIDERS.includes(r.kind)) {
      let moved = null;
      // a recess keeps the wall's rhythm: it slides at most 0.8 m, else the theme's vertical piece takes its place (below)
      for (const dv of r.kind === 'recess' ? [0.8, -0.8] : [0.8, -0.8, 1.6, -1.6, 2.0, -2.0, 2.4, -2.4]) {
        const c = JSON.parse(JSON.stringify(r));
        c.at[ax] = r3(r.at[ax] + dv);
        if (!sockets.some((sk) => hits(aabb(c), sk))) { moved = c; break; }
      }
      if (moved) outRows.push(moved);
      else if (r.kind === 'recess') {
        // the theme's vertical piece in its place, full height, each socket a clear band (the wall keeps its rhythm)
        const wp = theme === 'foundry' || theme === 'kiln' ? 'rib' : theme === 'spire' ? 'fin' : 'pilaster';
        const c = JSON.parse(JSON.stringify(r));
        c.kind = wp; c.id = r.id.replace('-recess-', '-' + wp + '-');
        const pd = wp === 'fin' ? 0.32 : wp === 'pilaster' ? 0.335 : 0.34;
        const ac = turned ? 0 : 2; // the axis across the wall
        const [lo, hi] = turned ? [rect[0], rect[2]] : [rect[1], rect[3]];
        const wallC = Math.abs(r.at[ac] - lo) < Math.abs(r.at[ac] - hi) ? lo : hi, inward = wallC === lo ? 1 : -1;
        c.size = [wp === 'pilaster' ? 0.6 : 0.5, r3(top), pd];
        c.at[1] = r3(top / 2);
        c.at[ac] = r3(wallC + inward * pd / 2);
        const cb = aabb(c);
        const g = [BAND_LO, BAND_HI];
        for (const sk of sockets) if (hits(cb, sk)) g.push(r3(sk.y0), r3(sk.y1));
        c.gap = g;
        outRows.push(c);
      }
      continue;
    }
    // anything else (a frame, a landmark, a lamp, a truss, a stalactite ...) over a socket: dropped
  }
  return outRows;
}
function overPit(b, roomId) {
  return pits.some((p) => (!p.zone || p.zone === roomId) && b.x0 < p.rect[2] && b.x1 > p.rect[0] && b.z0 < p.rect[3] && b.z1 > p.rect[1]);
}

for (const room of plan.rooms) {
  const [x0, z0, x1, z1] = room.rect;
  const H = room.height || wallH;
  const area = (x1 - x0) * (z1 - z0);
  const rnd = mulberry32(fnv(id + '/' + room.id));
  const mand = [], high = [], low = [];
  const myDoors = doors.filter((d) => d.between.includes(room.id));
  const top = H - 0.4; // under the cornice
  // the clear lanes (door centre -> exit, door centre -> ramp feet)
  const targets = [];
  if (room.exit) targets.push([room.exit[0], room.exit[2]]);
  for (const rp of ramps) if (rp.zone === room.id) targets.push([rp.from[0], rp.from[1]]);
  const lanes = [];
  for (const d of myDoors) { const cx = (d.rect[0] + d.rect[2]) / 2, cz = (d.rect[1] + d.rect[3]) / 2; for (const t of targets) lanes.push([cx, cz, t[0], t[1]]); }
  // a piece flat on a wall (its far face within FLAT of the wall plane) never clutters a lane (decor has no collider); a door frame stands beside
  // its opening; the landmark sits on a wall run (never in an opening)
  const flat = (b) => Math.min(b.x1 - x0, x1 - b.x0, b.z1 - z0, z1 - b.z0) <= FLAT + 1e-6;
  const okLow = (r) => {
    const b = aabb(r);
    if (b.y0 >= 3) return true;
    const exempt = r.kind === 'doorFrame' || r.kind === 'hero' || flat(b);
    if (!exempt && b.y1 - b.y0 > 0.05 && lanes.some((l) => segDist(l[0], l[1], l[2], l[3], b) < LANE_HALF)) return false;
    if (flat(b) && r.kind !== 'skirt' && r.kind !== 'debris' && r.kind !== 'drift' && r.kind !== 'crystal' && r.kind !== 'ingots') return true;
    return !overPit(b, room.id);
  };
  // boxes of this level for the landmark's sight line
  const solids = (plan.boxes || []).map((b) => ({ x0: b.min[0], y0: b.min[1], z0: b.min[2], x1: b.max[0], y1: b.max[1], z1: b.max[2] }))
    .concat(plan.props.filter((p) => p.collider).map(aabb));
  const seen = (p, q) => !solids.some((b) => {
    if (q[0] >= b.x0 && q[0] <= b.x1 && q[1] >= b.y0 && q[1] <= b.y1 && q[2] >= b.z0 && q[2] <= b.z1) return false;
    let t0 = 0, t1 = 1;
    const lo = [b.x0, b.y0, b.z0], hi = [b.x1, b.y1, b.z1];
    for (let a = 0; a < 3; a++) {
      const d = q[a] - p[a];
      if (Math.abs(d) < 1e-12) { if (p[a] < lo[a] || p[a] > hi[a]) return false; continue; }
      let ta = (lo[a] - p[a]) / d, tb = (hi[a] - p[a]) / d;
      if (ta > tb) [ta, tb] = [tb, ta];
      t0 = Math.max(t0, ta); t1 = Math.min(t1, tb);
      if (t0 > t1) return false;
    }
    const len = Math.hypot(q[0] - p[0], q[1] - p[1], q[2] - p[2]);
    return t1 > 0.05 / len && t0 < 1 - 0.05 / len;
  });

  // --- the four edges and their wall runs ---
  const edges = [
    { name: 'n', alongX: true, c: z0, a: x0, b: x1, inward: +1 },
    { name: 's', alongX: true, c: z1, a: x0, b: x1, inward: -1 },
    { name: 'w', alongX: false, c: x0, a: z0, b: z1, inward: +1 },
    { name: 'e', alongX: false, c: x1, a: z0, b: z1, inward: -1 },
  ];
  const runs = [];
  for (const e of edges) {
    const gaps = [];
    for (const d of myDoors) {
      const [dx0, dz0, dx1, dz1] = d.rect;
      const touches = e.alongX ? e.c >= dz0 - 0.01 && e.c <= dz1 + 0.01 : e.c >= dx0 - 0.01 && e.c <= dx1 + 0.01;
      if (touches) gaps.push(e.alongX ? [dx0, dx1, d] : [dz0, dz1, d]);
    }
    gaps.sort((p, q) => p[0] - q[0]);
    let cur = e.a;
    for (const g of gaps) { if (g[0] > cur + 0.01) runs.push({ e, a: cur, b: Math.min(g[0], e.b) }); cur = Math.max(cur, g[1]); }
    if (cur < e.b - 0.01) runs.push({ e, a: cur, b: e.b });
    e.gaps = gaps;
  }
  // a piece on a wall: `off` = distance of its centre from the wall plane into the room; `along` its centre along the run; w along the wall
  function onWall(e, kind, along, y, w, h, d, extra) {
    if (y - h / 2 < BAND_HI && y + h / 2 > BAND_LO && d >= RIDE.stripOffset && kind !== 'wash' && kind !== 'scorch') extra = Object.assign({ gap: [BAND_LO, BAND_HI] }, extra || {});
    const off = d / 2 + 0.0;
    const pos = e.c + e.inward * off;
    return e.alongX ? row(room, kind, along, y, pos, w, h, d, 0, extra) : row(room, kind, pos, y, along, w, h, d, 90, extra);
  }
  const wallPiece = theme === 'foundry' || theme === 'kiln' ? 'rib' : theme === 'spire' ? 'fin' : 'pilaster';
  for (const run of runs) {
    const L = run.b - run.a, mid = (run.a + run.b) / 2, e = run.e;
    if (L < 0.3) continue;
    // skirt: split around pits that touch this wall
    let segs = [[run.a, run.b]];
    for (const p of pits) {
      if (p.zone && p.zone !== room.id) continue;
      const [px0, pz0, px1, pz1] = p.rect;
      const near = e.alongX ? Math.min(Math.abs(pz0 - e.c), Math.abs(pz1 - e.c)) <= BAND || (e.c >= pz0 && e.c <= pz1) : Math.min(Math.abs(px0 - e.c), Math.abs(px1 - e.c)) <= BAND || (e.c >= px0 && e.c <= px1);
      if (!near) continue;
      const lo = e.alongX ? px0 : pz0, hi = e.alongX ? px1 : pz1;
      segs = segs.flatMap(([s0, s1]) => (hi <= s0 || lo >= s1 ? [[s0, s1]] : [[s0, lo], [hi, s1]].filter(([q0, q1]) => q1 - q0 > 0.3)));
    }
    // a strip along a west / east wall stops short of the corners by the north / south strip's depth (no two strips share a corner: no z-fight)
    const strip = (kind, s0, s1, y, h, d, trim) => {
      if (!e.alongX) { if (Math.abs(s0 - z0) < 0.01) s0 += d + 0.01; if (Math.abs(s1 - z1) < 0.01) s1 -= d + 0.01; }
      if (trim) { s0 += trim; s1 -= trim; }
      if (s1 - s0 > 0.3) return onWall(e, kind, (s0 + s1) / 2, y, s1 - s0, h, d);
      return null;
    };
    const add = (list, r) => { if (r) list.push(r); };
    for (const [s0, s1] of segs) add(mand, strip('skirt', s0, s1, 0.11, 0.22, 0.15)); // under L's low wall sockets (0.3-0.5 m)
    add(mand, strip('cornice', run.a, run.b, H - 0.2, 0.4, 0.3));
    // the wall wash: a 2 cm skin from the floor to the cornice, dark at the floor and light up high (K1 5.2: no wall reads as one flat colour)
    add(mand, strip('wash', run.a, run.b, top / 2, top, 0.02));
    // wall pieces: both run ends are stops (a corner pilaster or a door frame's jamb), so the pieces go between them, <= PIECE_EVERY apart
    const nInt = Math.max(0, Math.ceil(L / PIECE_EVERY) - 1);
    const step = L / (nInt + 1);
    for (let i = 1; i <= nInt; i++) {
      const at = run.a + i * step;
      const recess = i % 2 === 0 && step >= 3.6 && H >= 5;
      const rTop = Math.min(top - 0.3, BAND_HI + 0.15 + 4);
      if (recess && rTop - (BAND_HI + 0.15) >= 1.5) mand.push(onWall(e, 'recess', at, (BAND_HI + 0.15 + rTop) / 2, 3, rTop - (BAND_HI + 0.15), 0.2));
      else if (theme === 'kiln' && i % 3 === 0 && step >= 4.6 && H >= 5) mand.push(onWall(e, 'kilnArch', at, 2, 4, 4, 0.325));
      else {
        const pw = wallPiece === 'pilaster' ? 0.6 : 0.5, pd = wallPiece === 'fin' ? 0.32 : wallPiece === 'pilaster' ? 0.335 : 0.34; // inside L's trim inset
        const probe = onWall(e, wallPiece, at, top / 2, pw, top, pd);
        counts[room.id]--;
        const pit = pits.find((p) => (!p.zone || p.zone === room.id) && (() => { const b = aabb(probe); return b.x0 < p.rect[2] && b.x1 > p.rect[0] && b.z0 < p.rect[3] && b.z1 > p.rect[1]; })());
        const base = pit ? pit.floorY : 0;
        mand.push(onWall(e, wallPiece, at, (base + top) / 2, pw, top - base, pd));
      }
    }
    // high: the edge light at 3 m, banners between the pieces, a pipe run (a single wall pipe in a low area)
    if (H >= 4) add(high, strip('edgeLight', run.a, run.b, 3.05, 0.1, 0.06));
    if (H >= 8) for (let i = 0; i <= nInt; i++) { const at = run.a + (i + 0.5) * step; if (at - run.a > 1 && run.b - at > 1) high.push(onWall(e, 'banner', at, Math.min(H - 1.2, 7) - 1.8, 1, 3.6, 0.05)); }
    if (H >= 6 && L >= 6 && (theme === 'foundry' || theme === 'kiln' || theme === 'span')) add(high, strip('pipes', run.a, run.b, 5.25, 0.6, 0.55, 0.3));
    else if (H >= 4.4 && L >= 4) add(high, strip('wallPipe', run.a, run.b, Math.min(H - 0.8, 3.6), 0.3, 0.27, 0.3));
    // low: debris every 30 m of wall, a scorch every other run
    for (let k = 0; k < Math.max(1, Math.floor(L / 12)); k++) { const at = run.a + (L * (k + 0.5)) / Math.max(1, Math.floor(L / 12)) + (rnd() - 0.5); if (at > run.a + 0.8 && at < run.b - 0.8) low.push(onWall(e, 'debris', at, 0.3, 1.2, 0.6, 0.33)); }
    if (L >= 4) low.push(onWall(e, 'scorch', run.a + L * (0.25 + 0.5 * rnd()), 0.65, 1.6, 0.9, 0.04));
    if (theme === 'ice') { low.push(onWall(e, 'drift', run.a + L * (0.2 + 0.6 * rnd()), 0.25, 2, 0.5, 0.33)); low.push(onWall(e, 'crystal', run.a + L * (0.2 + 0.6 * rnd()), 0.6, 1, 1.2, 0.33)); }
    if (theme === 'kiln' && L >= 5) low.push(onWall(e, 'ingots', run.a + L * (0.2 + 0.6 * rnd()), 0.3, 1.6, 0.6, 0.3));
  }
  // corners where two runs meet: a pilaster
  const corners = [[x0, z0], [x1, z0], [x0, z1], [x1, z1]];
  for (const [cx, cz] of corners) {
    const onRun = (alongX, c, v) => runs.some((r) => r.e.alongX === alongX && Math.abs(r.e.c - c) < 0.01 && v >= r.a - 0.01 && v <= r.b + 0.01);
    if (!(onRun(true, cz, cx) && onRun(false, cx, cz))) continue;
    const s = 0.345;
    mand.push(row(room, 'pilaster', cx + (cx === x0 ? s / 2 : -s / 2), top / 2, cz + (cz === z0 ? s / 2 : -s / 2), s, top, s, 0, { gap: [BAND_LO, BAND_HI] }));
  }
  // doors: a frame on this face of each door (jambs beside the opening) and a lintel line above it
  for (const e of edges) for (const [g0, g1, d] of e.gaps) {
    const dh = d.height || 4.5;
    const fw = g1 - g0 + 0.8, fh = Math.min(dh + 0.4, H - 0.45), mid = (g0 + g1) / 2;
    mand.push(onWall(e, 'doorFrame', mid, fh / 2, fw, fh, 0.31));
    if (fh + 0.45 <= H - 0.4) high.push(onWall(e, 'lintelLight', mid, fh + 0.375, g1 - g0, 0.15, 0.1));
  }
  // the landmark: flat on the wall the spawn faces (the far wall), its centre run
  {
    const yaw = room.spawn && typeof room.spawn.yawDeg === 'number' ? room.spawn.yawDeg : 0;
    // yaw 0 faces north (-z): the far wall is n; 180 s; +90 (left / west) w; -90 e
    const yn = ((Math.round(yaw / 90) % 4) + 4) % 4;
    const far = ['n', 'w', 's', 'e'][yn];
    // candidates: the far wall's runs first (longest first), then every other run; the first whose centre the spawn's eye sees past every plan box
    const order = runs.filter((r) => r.e.name === far).sort((p, q) => q.b - q.a - (p.b - p.a))
      .concat(runs.filter((r) => r.e.name !== far).sort((p, q) => q.b - q.a - (p.b - p.a)));
    const eye = room.spawn ? [room.spawn.at[0], room.spawn.at[1] + 1.7, room.spawn.at[2]] : [(x0 + x1) / 2, 1.7, (z0 + z1) / 2];
    let placed = null, first = null;
    for (const run of order) {
      const L = run.b - run.a;
      if (L < 2.2) continue;
      const y0 = 3.05, y1 = H >= 6 ? H - 0.65 : H - 0.45;
      if (y1 - y0 < 0.45) continue;
      const hw = H >= 6 ? Math.min(6, Math.max(1.5, L - 1.4)) : Math.min(9, Math.max(1.5, L - 1.4)); // a low area's frieze runs wider
      const hh = y1 - y0;
      const r = onWall(run.e, 'hero', (run.a + run.b) / 2, y0 + hh / 2, hw, hh, 1.0);
      first = first || r;
      if (seen(eye, r.at)) { placed = r; break; }
    }
    if (placed || first) mand.push(placed || first);
  }
  // low areas: lamps under the roof along the long axis every 6 m (above 3 m: they carry the high share where no truss fits)
  if (H >= 3.9 && H < 6) {
    const alongX = x1 - x0 >= z1 - z0, long = alongX ? x1 - x0 : z1 - z0, l0 = alongX ? x0 : z0;
    const n = Math.max(1, Math.round(long / 6));
    for (let i = 0; i < n; i++) {
      const at = l0 + ((i + 0.5) * long) / n;
      high.push(row(room, 'lamp', alongX ? at : (x0 + x1) / 2, H - 0.3, alongX ? (z0 + z1) / 2 : at, 0.5, 0.5, 0.5, 0));
    }
  }
  // the ceiling: trusses across the short side every 8 m with a lamp on a chain under each; the theme's ceiling pieces
  if (H >= 6) {
    const alongX = x1 - x0 <= z1 - z0; // the truss spans the short side
    const span = alongX ? x1 - x0 : z1 - z0, long = alongX ? z1 - z0 : x1 - x0, l0 = alongX ? z0 : x0;
    const n = Math.max(1, Math.floor(long / 8));
    for (let i = 0; i < n; i++) {
      const at = l0 + ((i + 0.5) * long) / n;
      const ty = H - 0.4 - 0.5;
      const cx = alongX ? (x0 + x1) / 2 : at, cz = alongX ? at : (z0 + z1) / 2;
      high.push(row(room, 'truss', cx, ty, cz, span - 0.2, 0.8, 0.56, alongX ? 0 : 90));
      const chainTop = ty - 0.4, lampY = Math.max(4.4, chainTop - 2.2);
      if (chainTop - lampY >= 0.6) {
        high.push(row(room, 'chain', cx, (chainTop + lampY + 0.25) / 2, cz, 0.12, chainTop - lampY - 0.25, 0.12, 0));
        high.push(row(room, 'lamp', cx, lampY, cz, 0.5, 0.5, 0.5, 0));
      }
    }
  }
  if (H >= 6 && area >= 900) {
    // the cross trusses (the long way) every 8 m across the short side
    const alongX = x1 - x0 > z1 - z0;
    const span = alongX ? x1 - x0 : z1 - z0, short = alongX ? z1 - z0 : x1 - x0, s0 = alongX ? z0 : x0;
    const n = Math.max(1, Math.floor(short / 8));
    for (let i = 0; i < n; i++) {
      const at = s0 + ((i + 0.5) * short) / n, ty = H - 0.4 - 0.5 - 0.9;
      high.push(row(room, 'truss', alongX ? (x0 + x1) / 2 : at, ty, alongX ? at : (z0 + z1) / 2, span - 1.4, 0.8, 0.5, alongX ? 0 : 90));
      const m = Math.max(1, Math.floor(span / 8)), l0 = alongX ? x0 : z0;
      for (let k = 0; k < m; k++) {
        const along = l0 + ((k + 0.5) * span) / m;
        const lx = alongX ? along + 1.3 : at, lz = alongX ? at : along + 1.3; // off the first trusses' lamps
        const chainTop = ty - 0.4, lampY = Math.max(4.4, chainTop - 1.6);
        if (chainTop - lampY >= 0.6) { high.push(row(room, 'chain', lx, (chainTop + lampY + 0.25) / 2, lz, 0.12, chainTop - lampY - 0.25, 0.12, 0)); high.push(row(room, 'lamp', lx, lampY, lz, 0.5, 0.5, 0.5, 0)); }
      }
    }
  }
  if (area >= 300) {
    // floor inlays: pairs of flush strips along the long axis, off the walls, out of pits (<= 10 % of the floor)
    const alongX = x1 - x0 >= z1 - z0, long = alongX ? x1 - x0 : z1 - z0, cMid = alongX ? (z0 + z1) / 2 : (x0 + x1) / 2;
    for (const off of [-3, 3, -5.5, 5.5]) {
      const r = alongX ? row(room, 'inlay', (x0 + x1) / 2, 0.01, cMid + off, long * 0.5, 0.02, 0.3, 0) : row(room, 'inlay', cMid + off, 0.01, (z0 + z1) / 2, long * 0.5, 0.02, 0.3, 90);
      const ib = aabb(r);
      if (Math.min(ib.x0 - x0, x1 - ib.x1, ib.z0 - z0, z1 - ib.z1) < TRIM.inset + TRIM.width / 2 + 0.3) continue;
      if (!overPit(aabb(r), room.id) && !solids.some((b) => b.y0 < 0.1 && b.y1 > 0.05 && aabb(r).x0 < b.x1 && aabb(r).x1 > b.x0 && aabb(r).z0 < b.z1 && aabb(r).z1 > b.z0)) low.push(r);
    }
  }
  if (theme === 'ice' && H >= 6) for (let k = 0; k < Math.max(2, Math.round(area / 60)); k++) {
    const sh = Math.min(3, H - 4);
    const st = row(room, 'stalactite', x0 + 1.5 + (x1 - x0 - 3) * rnd(), H - sh / 2, z0 + 1.5 + (z1 - z0 - 3) * rnd(), 1, sh, 1, 0);
    const sb = aabb(st);
    if (!high.some((o) => o.kind === 'stalactite' && (() => { const b = aabb(o); return b.x0 < sb.x1 + 0.2 && b.x1 > sb.x0 - 0.2 && b.z0 < sb.z1 + 0.2 && b.z1 > sb.z0 - 0.2; })())) high.push(st);
  }
  if (theme === 'spire' && H >= 12) { const rad = Math.min(x1 - x0, z1 - z0) / 2 - 1.5; if (rad >= 3) high.push(row(room, 'halo', (x0 + x1) / 2, H - 1.5, (z0 + z1) / 2, rad * 2, 0.5, rad * 2, 0)); }

  // --- the landmark owns its stretch of wall: drop the pieces that would cross it (strips run behind it) ---
  const hero = mand.find((r) => r.kind === 'hero');
  if (hero) {
    const hb = aabb(hero);
    const keep = (r) => { if (r === hero || ['skirt', 'cornice', 'wash', 'edgeLight', 'pipes', 'wallPipe', 'truss', 'chain', 'lamp', 'stalactite', 'halo', 'inlay'].includes(r.kind)) return true; const b = aabb(r); return !(b.x0 < hb.x1 && b.x1 > hb.x0 && b.z0 < hb.z1 && b.z1 > hb.z0 && b.y0 < hb.y1 && b.y1 > hb.y0); };
    for (const list of [mand, high, low]) { const kept = list.filter(keep); list.length = 0; list.push(...kept); }
  }
  // the washes are wall surfaces, not pieces (the checker does not count them): set aside, written after the count
  const washes = mand.filter((r) => r.kind === 'wash');
  { const rest = mand.filter((r) => r.kind !== 'wash'); mand.length = 0; mand.push(...rest); }
  out.push(...washes);
  // --- L's sockets stay in view (strips cut, wall pieces slid, the rest dropped) ---
  for (const list of [mand, high, low]) { const kept = clearSockets(list, top, room.rect); list.length = 0; list.push(...kept); }
  // --- assemble: mandatory, then high until the share, then low until the density ---
  const take = mand.filter(okLow);
  const graded = area >= 150;
  const cap = graded ? Math.floor((DENSITY_CAP * area) / 100) : Infinity;
  const hardCap = graded ? Math.floor((HARD_CAP * area) / 100) : Infinity;
  const highNow = () => take.filter((r) => aabb(r).y0 >= 3).length;
  for (const r of high) {
    const share = highNow() / take.length;
    if (take.length >= hardCap) break;
    if (r.kind === 'banner' && take.some((o) => o.kind === 'recess' && hits(aabb(o), aabb(r)))) continue; // a banner never hangs over a recess
    if (share >= HIGH_SHARE && (take.length >= cap || take.length >= (DENSITY_TARGET * area) / 100)) break;
    take.push(r);
  }
  for (const r of low) {
    if (take.length >= Math.min(cap, Math.ceil((DENSITY_TARGET * area) / 100))) break;
    if (highNow() / (take.length + 1) < 0.41) break;
    // a low piece never overlaps another taken piece of a different kind (two clusters in one spot share faces: a z-fight)
    const b = aabb(r);
    const clash = take.some((o) => o.kind !== r.kind && !['skirt', 'cornice', 'wash', 'edgeLight', 'pipes', 'wallPipe'].includes(o.kind) && (() => { const c = aabb(o); return b.x0 < c.x1 && b.x1 > c.x0 && b.z0 < c.z1 && b.z1 > c.z0 && b.y0 < c.y1 && b.y1 > c.y0; })());
    if (!clash && okLow(r)) take.push(r);
  }
  out.push(...take);
}

plan.props.push(...out);
fs.writeFileSync(outPath, JSON.stringify(plan, null, 2) + '\n');
const per = {};
for (const r of out) per[r.room] = (per[r.room] || 0) + 1;
console.log(`dress33: ${id} (${theme}): ${out.length} decor rows in ${Object.keys(per).length} areas -> ${outPath}`);
