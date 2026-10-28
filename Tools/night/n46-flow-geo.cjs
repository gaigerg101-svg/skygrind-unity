// NIGHT46 builder F1: the flow meter's geometry (n46flow, Tools/SimHarness/Harness.N46.Flow.cs, calls it; it also runs alone).
// Ported unchanged from the NIGHT46 forge's scratch rasteriser (Research/2026-10-17-wave46/FLOW-AUDIT.md "How this was measured"): each
// fight room's rect rasterised at 0.25 m from the level file's collision boxes + wedges + lava; walkable = a box top with 1.8 m headroom, lava
// and anything under -6 m (voids) not; one-cell erosion for the capsule. Read-only.
//   node Tools/night/n46-flow-geo.cjs <levelId> [repoRoot]   (reads <repoRoot>/Design/levels/<levelId>.json)
// One line per fight room (kind room / boss): "<roomId>\t<text>":
//   floor m2 (walkable, lava left out) · tiers (box-top heights, 0.5 m bins, >= 4 m2, reachable from the spawn with a 2.8 m climb) ·
//   loop: the largest hole of the walk graph (step 0.55 m); "y" when >= 50 m2 (a central circuit: FLOW-AUDIT's y rooms are 105-177 m2, its
//   edge-only rooms <= 22.6 m2) · pockets (>= 2 m2 where 3 of 4 axis rays hit within 3 m) · sight: the longest clear eye-height sightline ·
//   w1: wave 1's nodes (waveNodes43[0], else nodes + highNodes) in view from the spawn (bearing <= 53 deg off the spawn's facing, its
//   yawDeg, and a clear line) / all. NIGHT60 B12: the bearing was taken off -z whatever the yaw (right only for yaw-0 spawns; n57-kiln's kb2c,
//   yaw 45, printed 4 / 4 where the real facing holds 1 / 4: NIGHT60-LOG B1).
const path = require("path");
const fs = require("fs");
const CELL = 0.25, HEAD = 1.8, STEP = 0.55, HOP = 1.5, DJ = 2.8, EYE = 1.6;
const HFOV_HALF = 53; // 74 deg vertical at 16:9 -> ~106 deg horizontal
const r1 = (v) => Math.round(v * 10) / 10;

function wedgeHeight(w, x, z) {
  // yaw 0 runs toward -z, yaw pi toward +z (matches the plan's ramps from -> to); rises 0 -> rise along length
  const dx = -Math.sin(w.yaw), dz = -Math.cos(w.yaw);
  const px = x - w.x, pz = z - w.z;
  const s = px * dx + pz * dz; // along
  const t = px * -dz + pz * dx; // across
  if (s < 0 || s > w.length || Math.abs(t) > w.width / 2) return null;
  return (w.rise * s) / w.length;
}

function analyseRoom(J, room, lvl) {
  const [X0, Z0, X1, Z1] = room.rect;
  const boxes = J.collision.boxes.filter((b) => b.max[0] > X0 - 1 && b.min[0] < X1 + 1 && b.max[2] > Z0 - 1 && b.min[2] < Z1 + 1);
  const wedges = J.collision.wedges.filter((w) => Math.abs(w.x - (X0 + X1) / 2) < (X1 - X0) / 2 + w.length + 1 && Math.abs(w.z - (Z0 + Z1) / 2) < (Z1 - Z0) / 2 + w.length + 1);
  const lava = (J.lava || []).filter((l) => l.rect[2] > X0 && l.rect[0] < X1 && l.rect[3] > Z0 && l.rect[1] < Z1);
  const NX = Math.round((X1 - X0) / CELL), NZ = Math.round((Z1 - Z0) / CELL);
  const cx = (i) => X0 + (i + 0.5) * CELL, cz = (k) => Z0 + (k + 0.5) * CELL;
  const colAt = (x, z) => boxes.filter((b) => x >= b.min[0] && x <= b.max[0] && z >= b.min[2] && z <= b.max[2]);
  const blockedAbove = (col, h) => col.some((b) => b.min[1] < h + HEAD && b.max[1] > h + 0.05);
  const ceilTop = 0 + room.height + 0.5;
  // nodes per cell: [{h, onBox, lava}]
  const cells = new Array(NX * NZ);
  for (let i = 0; i < NX; i++) for (let k = 0; k < NZ; k++) {
    const x = cx(i), z = cz(k);
    const col = colAt(x, z);
    const cand = [];
    for (const b of col) cand.push({ h: b.max[1], box: true });
    for (const w of wedges) { const h = wedgeHeight(w, x, z); if (h !== null) cand.push({ h, box: false }); }
    const list = [];
    for (const c of cand) {
      if (c.h > ceilTop - 1.0) continue;
      if (c.h < -6) continue; // void (span's -14 pits)
      if (blockedAbove(col, c.h)) continue;
      // a wedge surface under a box top: keep only if headroom
      if (list.some((o) => Math.abs(o.h - c.h) < 0.05)) continue;
      const inLava = lava.some((l) => x >= l.rect[0] && x <= l.rect[2] && z >= l.rect[1] && z <= l.rect[3] && c.h <= l.y + 0.3);
      list.push({ h: c.h, box: c.box, lava: inLava });
    }
    cells[i * NZ + k] = list;
  }
  const idx = (i, k) => i * NZ + k;
  const ok = (n) => n && !n.lava;
  // midpoint clearance for an edge
  const midClear = (i, k, i2, k2, h) => {
    const x = (cx(i) + cx(i2)) / 2, z = (cz(k) + cz(k2)) / 2;
    return !blockedAbove(colAt(x, z), h);
  };
  // erosion: a node is clear if all 8 neighbours have a non-lava node at h' <= h + STEP (lower is fine: a drop is not a wall)
  for (let i = 0; i < NX; i++) for (let k = 0; k < NZ; k++) for (const n of cells[idx(i, k)]) {
    if (n.lava) { n.clear = false; continue; }
    let c = true;
    for (let di = -1; di <= 1 && c; di++) for (let dk = -1; dk <= 1 && c; dk++) {
      if (!di && !dk) continue;
      const i2 = i + di, k2 = k + dk;
      if (i2 < 0 || k2 < 0 || i2 >= NX || k2 >= NZ) continue; // the rect's edge (doors / walls beyond): judged by the walls' own cells
      const l2 = cells[idx(i2, k2)];
      if (!l2.some((m) => !m.lava && m.h <= n.h + STEP && m.h >= n.h - 6)) c = false;
    }
    n.clear = c;
  }
  // edge lists
  function edge(i, k, i2, k2, T) {
    const a = cells[idx(i, k)], b = cells[idx(i2, k2)];
    for (const n of a) if (n.clear && n.reach) for (const m of b) if (m.clear && m.reach && Math.abs(n.h - m.h) <= T && midClear(i, k, i2, k2, Math.max(n.h, m.h))) return true;
    return false;
  }
  // reachability from the spawn: directed, up <= DJ (a double jump), drop any
  const sp = room.spawn ? room.spawn.at : [(X0 + X1) / 2, 0, Z1 - 1];
  let si = Math.min(NX - 1, Math.max(0, Math.floor((sp[0] - X0) / CELL))), sk = Math.min(NZ - 1, Math.max(0, Math.floor((sp[2] - Z0) / CELL)));
  // find nearest cell with a clear node
  let best = null;
  for (let i = 0; i < NX; i++) for (let k = 0; k < NZ; k++) if (cells[idx(i, k)].some((n) => n.clear)) { const d = (i - si) ** 2 + (k - sk) ** 2; if (!best || d < best.d) best = { i, k, d }; }
  const q = [];
  if (best) { const n = cells[idx(best.i, best.k)].filter((n) => n.clear).sort((a, b) => Math.abs(a.h - sp[1]) - Math.abs(b.h - sp[1]))[0]; n.reach = true; q.push([best.i, best.k, n]); }
  const reachUp = (T) => {
    while (q.length) {
      const [i, k, n] = q.pop();
      for (const [di, dk] of [[1, 0], [-1, 0], [0, 1], [0, -1]]) {
        const i2 = i + di, k2 = k + dk;
        if (i2 < 0 || k2 < 0 || i2 >= NX || k2 >= NZ) continue;
        for (const m of cells[idx(i2, k2)]) {
          if (!m.clear || m.reach) continue;
          if (m.h - n.h > T) continue;
          if (!midClear(i, k, i2, k2, Math.max(n.h, m.h))) continue;
          m.reach = true; q.push([i2, k2, m]);
        }
      }
    }
  };
  reachUp(DJ);
  // tiers: distinct box-top heights (0.5 m bins) with >= 4 m2 of reachable clear area
  const tierArea = {}, allTierArea = {};
  let floorArea = 0, lavaArea = 0;
  for (let i = 0; i < NX; i++) for (let k = 0; k < NZ; k++) {
    const l = cells[idx(i, k)];
    if (l.some((n) => n.lava)) lavaArea += CELL * CELL;
    if (l.some((n) => !n.lava)) floorArea += CELL * CELL;
    for (const n of l) if (n.box && !n.lava) { const b = (Math.round(n.h * 2) / 2).toFixed(1); allTierArea[b] = (allTierArea[b] || 0) + CELL * CELL; if (n.reach) tierArea[b] = (tierArea[b] || 0) + CELL * CELL; }
  }
  const tiers = Object.entries(allTierArea).filter(([, a]) => a >= 4).map(([h, a]) => `${h}m:${Math.round(a)}m2${tierArea[h] ? "" : "(unreached)"}`).sort((a, b) => parseFloat(a) - parseFloat(b));
  // loops: holes of the 2x upsampled graph image
  function holes(T) {
    const W = 2 * NX + 1, H = 2 * NZ + 1;
    const img = new Uint8Array(W * H);
    const P = (u, v) => u * H + v;
    const has = new Uint8Array(NX * NZ);
    for (let i = 0; i < NX; i++) for (let k = 0; k < NZ; k++) if (cells[idx(i, k)].some((n) => n.clear && n.reach)) { has[idx(i, k)] = 1; img[P(2 * i + 1, 2 * k + 1)] = 1; }
    const eX = new Uint8Array(NX * NZ), eZ = new Uint8Array(NX * NZ);
    for (let i = 0; i < NX; i++) for (let k = 0; k < NZ; k++) {
      if (i + 1 < NX && has[idx(i, k)] && has[idx(i + 1, k)] && edge(i, k, i + 1, k, T)) { eX[idx(i, k)] = 1; img[P(2 * i + 2, 2 * k + 1)] = 1; }
      if (k + 1 < NZ && has[idx(i, k)] && has[idx(i, k + 1)] && edge(i, k, i, k + 1, T)) { eZ[idx(i, k)] = 1; img[P(2 * i + 1, 2 * k + 2)] = 1; }
    }
    for (let i = 0; i + 1 < NX; i++) for (let k = 0; k + 1 < NZ; k++) if (eX[idx(i, k)] && eX[idx(i, k + 1)] && eZ[idx(i, k)] && eZ[idx(i + 1, k)]) img[P(2 * i + 2, 2 * k + 2)] = 1;
    // components of 0 pixels
    const seen = new Uint8Array(W * H), out = [];
    for (let u = 0; u < W; u++) for (let v = 0; v < H; v++) {
      if (img[P(u, v)] || seen[P(u, v)]) continue;
      let border = false, cnt = 0, lavaCnt = 0, umin = u, umax = u, vmin = v, vmax = v;
      const st = [[u, v]]; seen[P(u, v)] = 1;
      while (st.length) {
        const [a, b] = st.pop(); cnt++;
        if (a === 0 || b === 0 || a === W - 1 || b === H - 1) border = true;
        if (a % 2 === 1 && b % 2 === 1) { const l = cells[idx((a - 1) / 2, (b - 1) / 2)]; if (l.some((n) => n.lava)) lavaCnt++; }
        umin = Math.min(umin, a); umax = Math.max(umax, a); vmin = Math.min(vmin, b); vmax = Math.max(vmax, b);
        for (const [da, db] of [[1, 0], [-1, 0], [0, 1], [0, -1]]) { const a2 = a + da, b2 = b + db; if (a2 < 0 || b2 < 0 || a2 >= W || b2 >= H) continue; if (img[P(a2, b2)] || seen[P(a2, b2)]) continue; seen[P(a2, b2)] = 1; st.push([a2, b2]); }
      }
      if (border) continue;
      const area = cnt * (CELL / 2) ** 2;
      if (area < 1) continue;
      const wx = r1(X0 + (umax - umin) / 2 * CELL), wz = r1(Z0 + (vmax - vmin) / 2 * CELL);
      out.push({ area: r1(area), lava: r1((lavaCnt * CELL * CELL)), x: [r1(X0 + umin / 2 * CELL), r1(X0 + umax / 2 * CELL)], z: [r1(Z0 + vmin / 2 * CELL), r1(Z0 + vmax / 2 * CELL)] });
    }
    return out.sort((a, b) => b.area - a.area);
  }
  const holesWalk = holes(STEP), holesHop = holes(HOP);
  // directed straight rays on the walk graph (up <= STEP, drop any)
  function ray(i, k, n, di, dk, cap) {
    let d = 0, h = n.h;
    while (d < cap) {
      const i2 = i + di, k2 = k + dk;
      if (i2 < 0 || k2 < 0 || i2 >= NX || k2 >= NZ) return { d, out: true };
      const c = cells[idx(i2, k2)].filter((m) => m.clear && m.h <= h + STEP && midClear(i, k, i2, k2, Math.max(h, m.h)));
      if (!c.length) return { d, out: false };
      const m = c.sort((a, b) => b.h - a.h)[0];
      d += CELL * Math.hypot(di, dk); i = i2; k = k2; h = m.h;
    }
    return { d, out: false };
  }
  // dead-end pockets: >= 3 of the 4 axis rays blocked within 3 m (not by the rect's edge)
  const flag = new Uint8Array(NX * NZ);
  let longest = { d: 0 };
  for (let i = 0; i < NX; i++) for (let k = 0; k < NZ; k++) {
    const n = cells[idx(i, k)].filter((m) => m.clear && m.reach).sort((a, b) => a.h - b.h)[0];
    if (!n) continue;
    let blocked = 0;
    for (const [di, dk] of [[1, 0], [-1, 0], [0, 1], [0, -1]]) { const r = ray(i, k, n, di, dk, 3.0); if (r.d < 3.0 && !r.out) blocked++; }
    if (blocked >= 3) flag[idx(i, k)] = 1;
    if ((i % 2 === 0) && (k % 2 === 0)) for (const [di, dk] of [[1, 0], [-1, 0], [0, 1], [0, -1], [1, 1], [1, -1], [-1, 1], [-1, -1]]) {
      const r = ray(i, k, n, di, dk, 200);
      if (r.d > longest.d) longest = { d: r1(r.d), from: [r1(cx(i)), r1(n.h), r1(cz(k))], dir: [di, dk] };
    }
  }
  const pockets = [];
  { const seen = new Uint8Array(NX * NZ);
    for (let i = 0; i < NX; i++) for (let k = 0; k < NZ; k++) {
      if (!flag[idx(i, k)] || seen[idx(i, k)]) continue;
      const st = [[i, k]]; seen[idx(i, k)] = 1; let cnt = 0, sx = 0, sz = 0;
      while (st.length) { const [a, b] = st.pop(); cnt++; sx += cx(a); sz += cz(b);
        for (let da = -1; da <= 1; da++) for (let db = -1; db <= 1; db++) { const a2 = a + da, b2 = b + db; if (a2 < 0 || b2 < 0 || a2 >= NX || b2 >= NZ) continue; if (!flag[idx(a2, b2)] || seen[idx(a2, b2)]) continue; seen[idx(a2, b2)] = 1; st.push([a2, b2]); } }
      const area = cnt * CELL * CELL;
      if (area >= 2) pockets.push({ area: r1(area), at: [r1(sx / cnt), r1(sz / cnt)] });
    } }
  // open floor: the largest disc of clear reachable nodes (Chebyshev-ish distance transform on 'has')
  // sightline: 1 m (2 m for > 2500 m2) grid of reachable nodes, eye height, segment vs boxes
  const step = floorArea > 2500 ? 8 : 4; // cells
  const pts = [];
  for (let i = 0; i < NX; i += step) for (let k = 0; k < NZ; k += step) { const n = cells[idx(i, k)].filter((m) => m.reach && !m.lava).sort((a, b) => a.h - b.h)[0]; if (n) pts.push([cx(i), n.h + EYE, cz(k)]); }
  const segHit = (a, b) => {
    for (const bx of boxes) {
      let t0 = 0, t1 = 1, hit = true;
      for (let ax = 0; ax < 3; ax++) {
        const d = b[ax] - a[ax];
        if (Math.abs(d) < 1e-9) { if (a[ax] <= bx.min[ax] || a[ax] >= bx.max[ax]) { hit = false; break; } continue; }
        let ta = (bx.min[ax] - a[ax]) / d, tb = (bx.max[ax] - a[ax]) / d; if (ta > tb) [ta, tb] = [tb, ta];
        t0 = Math.max(t0, ta); t1 = Math.min(t1, tb); if (t0 >= t1) { hit = false; break; }
      }
      if (hit) return true;
    }
    return false;
  };
  let sight = { d: 0 };
  const diag = Math.hypot(X1 - X0, Z1 - Z0);
  // sort pairs by distance desc and stop at first clear (fast)
  const pairs = [];
  for (let a = 0; a < pts.length; a++) for (let b = a + 1; b < pts.length; b++) { const d = Math.hypot(pts[a][0] - pts[b][0], pts[a][1] - pts[b][1], pts[a][2] - pts[b][2]); pairs.push([d, a, b]); }
  pairs.sort((p, q2) => q2[0] - p[0]);
  for (const [d, a, b] of pairs) { if (!segHit(pts[a], pts[b])) { sight = { d: r1(d), a: pts[a].map(r1), b: pts[b].map(r1) }; break; } }
  // spawn-relative LOS
  const los = (p) => !segHit([sp[0], sp[1] + EYE, sp[2]], [p[0], p[1] + 1.0, p[2]]);
  return { NX, NZ, floorArea: Math.round(floorArea), lavaArea: Math.round(lavaArea), tiers, holesWalk: holesWalk.slice(0, 5), nHolesWalk: holesWalk.length, holesHop: holesHop.slice(0, 5), nHolesHop: holesHop.length, pockets, longest, sight, diag: r1(diag), los, cells, idx, X0, Z0, NZ: NZ, segHit, boxes };
}

function bearing(from, to) {
  // facing -z (yaw 0): angle of (to - from) from -z, degrees, + = right (+x)
  const dx = to[0] - from[0], dz = to[2] - from[2];
  return r1(Math.atan2(dx, -dz) * 180 / Math.PI);
}

function offFacing(b, yawDeg) {
  // NIGHT60 B12: a spawn facing yawDeg (0 = -z; + turns toward -x, the plan's and the game's yaw) looks along bearing() = -yawDeg, so a
  // node's bearing off that facing is b + yawDeg, wrapped to (-180, 180]
  let d = b + yawDeg;
  while (d > 180) d -= 360;
  while (d <= -180) d += 360;
  return d;
}

const LEVEL = process.argv[2];
const ROOT = process.argv[3] || process.cwd();
if (!LEVEL) { console.error("usage: node Tools/night/n46-flow-geo.cjs <levelId> [repoRoot]"); process.exit(2); }
const J = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/levels", LEVEL + ".json"), "utf8"));
for (const room of J.campaign.rooms) {
  if (room.kind !== "room" && room.kind !== "boss") continue;
  const A = analyseRoom(J, room, LEVEL);
  const sp = room.spawn ? room.spawn.at : [(room.rect[0] + room.rect[2]) / 2, 0, room.rect[3] - 1];
  const big = A.holesWalk.length ? A.holesWalk[0].area : 0;
  let w1 = "-";
  if (room.waves && room.waves.length) {
    const own = (room.waveNodes43 || [])[0];
    const nodes = own && own.length ? own : (room.nodes || []).concat(room.highNodes || []);
    const yaw = room.spawn && Number.isFinite(room.spawn.yawDeg) ? room.spawn.yawDeg : 0;
    const seen = nodes.filter((p) => Math.abs(offFacing(bearing(sp, p), yaw)) <= HFOV_HALF && A.los(p)).length;
    w1 = `${seen}/${nodes.length}`;
  }
  console.log(`${room.id}\tfloor ${A.floorArea} m2, tiers ${A.tiers.length}, loop ${big >= 50 ? "y" : "n"} (${big} m2), pockets ${A.pockets.length}, sight ${A.sight.d} m, w1 in view ${w1}`);
}
