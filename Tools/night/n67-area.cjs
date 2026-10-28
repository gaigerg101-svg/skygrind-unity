// NIGHT67 builder T (T1b; slate T1b line 2, the ROOM list's M1 AREA; FORGE67b research.md section 1, redteam.md 3): THE AREA LINT. One room of
// one level, judged from its plan (the pieces, the row: waves, nodes, line65), its level file (the nav grid) and, when given, skip v2's output
// (the touch table and launch kills): one line per rule with its numbers and PASS / FAIL / N/A, then `n67-area: PASS|FAIL <room> (<p>/<a>)`.
// A rule with nothing to judge prints N/A and never counts toward the passes. Static: no dotnet, no sim.
//   node Tools/night/n67-area.cjs --level <iceline|kiln|spire|foundry> [--dir <the round's folder>] [--touches <skip v2 output>]... [--card <md>] <room>
//       --dir: <dir>/<L>.plan.json and <dir>/<id>.json (gen-slots.cjs --out: the round's own files), else the tracked
//       Design/levels/n67/<L>.plan.json and Design/levels/<id>.json; the header prints which files it read
//   node Tools/night/n67-area.cjs --plan <plan.json> --file <level.json> [--touches f]... [--card <md>] <room>     any level (the baseline)
//   node Tools/night/n67-area.cjs --baseline     the 25 shipped fight rooms (n52-foundry, n52-iceline, n57-kiln, n52-span, n52-spire): each
//       rule's line per room, then per rule PASS / FAIL / N/A counts and the rules that do not discriminate (every shipped room the same)
//   node Tools/night/n67-area.cjs --selftest     a planted PASS and a planted FAIL for every rule (a synthetic 30 x 30 m room)
// THE RULES (the ROOM list's M1 text, then this tool's reading of it, stated so a builder and a critic read the same thing):
//   OPEN  "no open floor square larger than 12 x 12 m with no piece in it": a 0.5 m grid over the room's rect; a cell is taken by any box,
//         ramp, pad (2 x 2 m), pit, tunnel or collider prop footprint; the largest square of free cells (its side and corner printed);
//         PASS when the side is 12 m or less.
//   P1    "a plane 3.5 m or higher holds the last wave's ranged enemies (N/A with no ranged enemy)": ranged = spitter and drone (redteam.md 3,
//         the same set the decoration checker's SIGHT test uses here); the last wave's spawn set is its waveNodes43, else the room's nodes +
//         highNodes (Campaign24.SpawnWave puts EACH enemy on a random node of that set), so the plane holds them only when every node of the
//         set stands 3.5 m or more over the room's floor (the spawn's y): PASS then; the high share h / n printed.
//   P4    "every solid 1.75 m or taller does a job by the decoration checker": a standing solid (a box or collider prop from within 0.5 m of
//         the floor to 1.75 m or more over it; the room's bounding walls left out) passes by (a) touched on >= 3 of the seeds (the touch
//         table of --touches), (b) SIGHT (it alone cuts a ranged spawn's sight, eye 1.2 m, to a walk-line sample, eye 1.6 m, at any range),
//         (c) a launch kill within 1 m (--touches' launchkill lines). Without --touches, (a) and (c) are NOT READ (said on the line).
//   P5    "at least half the walk line's samples in a ranged spawn's sight at 6-24 m and line65's share at least 0.3 lower, or line65 within
//         6 m of an enemy": the walk line = spawn -> exit, a sample a metre, eye 1.6 m; line65 sampled a metre along its waypoints, eye 1.6 m
//         over each point; a sample is in sight when a ranged spawn (eye 1.2 m) 6-24 m away sees it past every solid; "an enemy" = any
//         spawn node of any wave. N/A with no line65. (redteam.md 3: line65 is the builder's drawing; the critic may read P5 as a guard.)
//   P9    "every plane 6 m or wider has at least 2 enemy climb routes with tops 8 m or more apart": a plane = a deck / hull box top 1 m or
//         more over the floor whose SHORT side is 6 m or more (builder R's question 2: "wider" is the short side, so a 5 x 18 m gallery is
//         not a plane for P9 and is not judged); a climb route's top = a nav node on the plane (the level's nav grid) reached by a walk or
//         jump link from a lower node off it; PASS when two such tops are 8 m or more apart. N/A with no such plane.
//   P11   "for a TOWER, GALLERY or SPLIT LEVEL card, every ranged spawn on a high node 2.5 m or more up and 12-24 m from the walk lane": the
//         card's archetype from --card (default Research/2026-10-24-wave67/cards/<room>.md: the first of TOWER / GALLERY / SPLIT LEVEL it
//         names); N/A for any other card or none. Builder R's question 1: THE WALK LANE IS THE WALK LINE, spawn -> exit (the same line P5 and
//         the decoration checker's SIGHT test sample; a room's `proofs` walks are not used: they are drawn per room and a rebuilt room
//         could draw them anywhere, while spawn and exit are fixed by the slot). A ranged spawn = each node of each wave's spawn set that
//         holds a ranged enemy (random placement, as P1); each must be 2.5 m or more over the floor and 12-24 m (floor plan) from the line.
//   P17   "every ride wall 4 m or taller and 8 m or longer with the cyan strip, no non-ride wall in that band": a ride wall = a box with
//         ride: true (the flag the view draws with the cyan trim; ride36 names the ridden face; the colour on screen is not checked: branch
//         C); each must stand 4 m or more over the floor and run 8 m or more; no other wall-like box (2 m or thinner, not a bounding wall)
//         4 m tall and 8 m long. N/A with neither.
// Probe numbers (0.5 m grid, 1 m samples, the eye heights, 0.25 m sight steps), not gameplay.
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const rel = (p) => (path.isAbsolute(p) ? p : path.join(ROOT, p));
const R2 = (v) => { const r = Math.round(v * 100) / 100; return Object.is(r, -0) ? "0" : String(r); };
const F1 = (v) => (Math.round(v * 10) / 10).toFixed(1);
const RANGED = new Set(["spitter", "drone"]);
const ARCH = ["TOWER", "GALLERY", "SPLIT LEVEL"];

// ---------------------------------------------------------------- the room: row, pieces, solids, nav
function roomOf(plan, level, id) {
  const row = plan.rooms.find((r) => r.id === id);
  if (!row) throw new Error("no room " + id + " in the plan");
  const floorY = row.spawn && row.spawn.at ? row.spawn.at[1] : 0;
  const pieces = [];
  for (const b of plan.boxes || []) if (b.zone === id && b.kind !== "floor") pieces.push({ kind: "box", src: b, key: "box(" + b.min.map(R2).join(",") + ".." + b.max.map(R2).join(",") + ")", min: b.min, max: b.max, label: (b.job || "-") + ": " + String(b.why || "").split(/[:(]/)[0].trim().slice(0, 40) });
  for (const r of plan.ramps || []) if (r.zone === id) {
    const x = r.from[0], z = r.from[1], dx = r.to[0] - x, dz = r.to[1] - z, len = Math.hypot(dx, dz);
    const w = { x, z, yaw: Math.atan2(-dx === 0 ? 0 : -dx, -dz === 0 ? 0 : -dz), length: len, width: r.width, rise: r.rise };
    const c = wedgeCorners(w);
    pieces.push({ kind: "ramp", src: r, wedge: w, key: "ramp(" + R2(x) + "," + R2(z) + "," + R2(len) + "," + R2(r.width) + "," + R2(r.rise) + ")", min: [Math.min(...c.map((p) => p[0])), floorY, Math.min(...c.map((p) => p[2]))], max: [Math.max(...c.map((p) => p[0])), floorY + r.rise, Math.max(...c.map((p) => p[2]))], label: (r.job || "ramp") });
  }
  for (const p of plan.pads || []) if (p.zone === id) pieces.push({ kind: "pad", src: p, key: "pad(" + p.at.map(R2).join(",") + ")", min: [p.at[0] - 1, p.at[1], p.at[2] - 1], max: [p.at[0] + 1, p.at[1] + 0.2, p.at[2] + 1], label: "pad" });
  for (const p of plan.pits || []) if (p.zone === id) pieces.push({ kind: "pit", src: p, key: "pit(" + p.rect.map(R2).join(",") + ")", min: [p.rect[0], (p.floorY || 0), p.rect[1]], max: [p.rect[2], floorY, p.rect[3]], label: p.lava ? "lava" : "pit" });
  for (const t of plan.tunnels || []) if (t.zone === id) pieces.push({ kind: "tunnel", src: t, key: "tunnel(" + t.rect.map(R2).join(",") + ")", min: [t.rect[0], floorY, t.rect[1]], max: [t.rect[2], floorY + (t.ceiling || 2), t.rect[3]], label: "tunnel" });
  for (const p of plan.props || []) if (p.room === id && p.collider === true) {
    const h = (p.size || [1, 1, 1]).map((s) => s / 2), at = p.at || [0, 0, 0];
    pieces.push({ kind: "propbox", src: p, key: "prop(" + (p.id || "?") + "@" + at.map(R2).join(",") + ")", min: [at[0] - h[0], at[1] - h[1], at[2] - h[2]], max: [at[0] + h[0], at[1] + h[1], at[2] + h[2]], label: "prop " + (p.id || "?") });
  }
  const solids = pieces.filter((p) => p.kind === "box" || p.kind === "ramp" || p.kind === "propbox");
  return { id, row, floorY, rect: row.rect, pieces, solids, nav: level && level.navGrid ? level.navGrid : null };
}
function wedgeCorners(w) {
  const ax = -Math.sin(w.yaw), az = -Math.cos(w.yaw), rx = Math.cos(w.yaw), rz = -Math.sin(w.yaw), hw = w.width / 2;
  const P = (a, c, y) => [w.x + ax * a + rx * c, y, w.z + az * a + rz * c];
  return [P(0, -hw, 0), P(0, hw, 0), P(w.length, -hw, 0), P(w.length, hw, 0), P(w.length, -hw, w.rise), P(w.length, hw, w.rise)];
}
function inside(pc, p) {
  if (pc.kind === "ramp") {
    const w = pc.wedge, ax = -Math.sin(w.yaw), az = -Math.cos(w.yaw), rx = Math.cos(w.yaw), rz = -Math.sin(w.yaw);
    const dx = p[0] - w.x, dz = p[2] - w.z, al = dx * ax + dz * az, ac = dx * rx + dz * rz;
    return w.length > 1e-9 && al > 0 && al < w.length && Math.abs(ac) < w.width / 2 && p[1] - pc.min[1] > 0 && p[1] - pc.min[1] < w.rise * al / w.length;
  }
  return p[0] > pc.min[0] && p[0] < pc.max[0] && p[1] > pc.min[1] && p[1] < pc.max[1] && p[2] > pc.min[2] && p[2] < pc.max[2];
}
function footprint(pc, x, z) {
  if (pc.kind === "ramp") { const w = pc.wedge, ax = -Math.sin(w.yaw), az = -Math.cos(w.yaw), rx = Math.cos(w.yaw), rz = -Math.sin(w.yaw); const dx = x - w.x, dz = z - w.z, al = dx * ax + dz * az, ac = dx * rx + dz * rz; return al > 0 && al < w.length && Math.abs(ac) < w.width / 2; }
  return x > pc.min[0] && x < pc.max[0] && z > pc.min[2] && z < pc.max[2];
}
function boundingWall(q, rect) {
  if (q.kind !== "box") return false;
  const [x0, z0, x1, z1] = rect;
  return ((q.min[0] <= x0 + 0.1 || q.max[0] >= x1 - 0.1) && (q.max[2] - q.min[2]) >= 0.5 * (z1 - z0)) || ((q.min[2] <= z0 + 0.1 || q.max[2] >= z1 - 0.1) && (q.max[0] - q.min[0]) >= 0.5 * (x1 - x0));
}
// the solids that cut the segment (sampled every 0.25 m, the ends' 0.3 m left out)
function blockers(R, a, b, only) {
  const d = Math.hypot(b[0] - a[0], b[1] - a[1], b[2] - a[2]);
  const hit = new Set();
  for (const pc of only || R.solids) {
    if (Math.max(a[0], b[0]) < pc.min[0] || Math.min(a[0], b[0]) > pc.max[0] || Math.max(a[2], b[2]) < pc.min[2] || Math.min(a[2], b[2]) > pc.max[2] || Math.max(a[1], b[1]) < pc.min[1] || Math.min(a[1], b[1]) > pc.max[1]) continue;
    for (let s = 0.3; s < d - 0.3; s += 0.25) { const t = s / d; if (inside(pc, [a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t, a[2] + (b[2] - a[2]) * t])) { hit.add(pc); break; } }
  }
  return hit;
}
// the waves' spawn sets (Campaign24.SpawnWave: waveNodes43[w], else nodes + highNodes)
function waveSets(row) {
  const all = (row.nodes || []).concat(row.highNodes || []);
  return (row.waves || []).map((w, i) => { const own = (row.waveNodes43 || [])[i]; return { kinds: w || [], nodes: own && own.length ? own : all, own: !!(own && own.length) }; });
}
function walkLine(row) {
  const a = row.spawn && row.spawn.at, b = row.exit;
  if (!a || !b) return [];
  const n = Math.max(1, Math.round(Math.hypot(b[0] - a[0], b[2] - a[2])));
  const out = [];
  for (let k = 0; k <= n; k++) { const t = k / n; out.push([a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t + 1.6, a[2] + (b[2] - a[2]) * t]); }
  return out;
}
function polyLine(pts) {
  const out = [];
  for (let i = 0; i + 1 < pts.length; i++) {
    const a = pts[i], b = pts[i + 1], n = Math.max(1, Math.round(Math.hypot(b[0] - a[0], b[2] - a[2])));
    for (let k = 0; k < n; k++) { const t = k / n; out.push([a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t + 1.6, a[2] + (b[2] - a[2]) * t]); }
  }
  if (pts.length) { const z = pts[pts.length - 1]; out.push([z[0], z[1] + 1.6, z[2]]); }
  return out;
}
function segDist(p, a, b) { // floor plan
  const vx = b[0] - a[0], vz = b[2] - a[2], L2 = vx * vx + vz * vz;
  const t = L2 > 0 ? Math.max(0, Math.min(1, ((p[0] - a[0]) * vx + (p[2] - a[2]) * vz) / L2)) : 0;
  return Math.hypot(p[0] - (a[0] + vx * t), p[2] - (a[2] + vz * t));
}
const uniq = (pts) => { const s = new Set(); return pts.filter((p) => { const k = p.join(","); if (s.has(k)) return false; s.add(k); return true; }); };
function rangedSpots(row) { return uniq([].concat(...waveSets(row).filter((w) => w.kinds.some((k) => RANGED.has(k))).map((w) => w.nodes))); }

// ---------------------------------------------------------------- the touches (skip v2's output)
function readTouches(files, id) {
  const touch = [], kills = [];
  for (const f of files) for (const l of fs.readFileSync(rel(f), "utf8").split(/\r?\n/)) {
    let m = l.match(/^n65verb skip touch: room (\S+) \| (\S+) \| seeds (\d+)\/(\d+) \| steps (\d+)/);
    if (m && m[1] === id) { touch.push({ key: m[2], seeds: +m[3], of: +m[4] }); continue; }
    m = l.match(/^n65verb skip launchkill: room (\S+) \| seed (\d+) \| at \((-?[\d.]+), (-?[\d.]+), (-?[\d.]+)\) kills (\d+)/);
    if (m && m[1] === id) kills.push({ at: [+m[3], +m[4], +m[5]] });
  }
  return { touch, kills, read: files.length > 0 };
}

// ---------------------------------------------------------------- the rules: each returns { v: PASS|FAIL|N/A, n: the numbers }
const RULES = {
  OPEN(R) {
    const [x0, z0, x1, z1] = R.rect, c = 0.5, nx = Math.round((x1 - x0) / c), nz = Math.round((z1 - z0) / c);
    const occ = R.pieces.filter((p) => p.kind !== "pad").concat(R.pieces.filter((p) => p.kind === "pad"));
    let best = 0, bi = 0, bj = 0;
    let prev = new Array(nx + 1).fill(0);
    for (let j = 0; j < nz; j++) {
      const cur = new Array(nx + 1).fill(0);
      for (let i = 0; i < nx; i++) {
        const x = x0 + (i + 0.5) * c, z = z0 + (j + 0.5) * c;
        const taken = occ.some((p) => footprint(p, x, z));
        cur[i + 1] = taken ? 0 : 1 + Math.min(prev[i + 1], cur[i], prev[i]);
        if (cur[i + 1] > best) { best = cur[i + 1]; bi = i; bj = j; }
      }
      prev = cur;
    }
    const side = best * c;
    return { v: side <= 12 ? "PASS" : "FAIL", n: "largest open square " + F1(side) + " m (x " + F1(x0 + (bi + 1) * c - side) + ".." + F1(x0 + (bi + 1) * c) + ", z " + F1(z0 + (bj + 1) * c - side) + ".." + F1(z0 + (bj + 1) * c) + "; " + R.pieces.length + " pieces on a " + F1(x1 - x0) + " x " + F1(z1 - z0) + " m rect; 12 m or less passes)" };
  },
  P1(R) {
    const ws = waveSets(R.row);
    if (!ws.length) return { v: "N/A", n: "no waves" };
    const last = ws[ws.length - 1], rc = last.kinds.filter((k) => RANGED.has(k)).length;
    if (!rc) return { v: "N/A", n: "the last wave (" + last.kinds.join(" ") + ") has no ranged enemy (spitter / drone)" };
    const hi = last.nodes.filter((n) => n[1] - R.floorY >= 3.5).length;
    return { v: hi === last.nodes.length && hi > 0 ? "PASS" : "FAIL", n: "the last wave's " + rc + " ranged of " + last.kinds.length + "; its spawn set (" + (last.own ? "waveNodes43" : "nodes + highNodes") + ") " + hi + " / " + last.nodes.length + " nodes 3.5 m or more over the floor (y " + F1(R.floorY) + "; each enemy takes a random node: all must be high)" };
  },
  P4(R, X) {
    const tall = R.solids.filter((p) => p.kind !== "ramp" && !boundingWall(p, R.rect) && p.min[1] <= R.floorY + 0.5 && p.max[1] - R.floorY >= 1.75);
    if (!tall.length) return { v: "N/A", n: "no standing solid 1.75 m or taller (bounding walls aside)" };
    const spots = rangedSpots(R.row).map((s) => [s[0], s[1] + 1.2, s[2]]), line = walkLine(R.row);
    const sole = new Set();
    for (const s of spots) for (const q of line) { const b = blockers(R, s, q); if (b.size === 1) sole.add([...b][0]); }
    const fails = [], how = { a: 0, b: 0, c: 0 };
    for (const p of tall) {
      const t = X.touch.find((x) => x.key === p.key);
      const a = !!(t && t.seeds >= 3), b = sole.has(p), c = X.kills.some((k) => Math.hypot(...[0, 1, 2].map((i) => Math.max(p.min[i] - k.at[i], 0, k.at[i] - p.max[i]))) <= 1);
      if (a) how.a++; if (b) how.b++; if (c) how.c++;
      if (!(a || b || c)) fails.push(p.key + " " + p.label.slice(0, 30));
    }
    return { v: fails.length ? "FAIL" : "PASS", n: (tall.length - fails.length) + " / " + tall.length + " standing solids >= 1.75 m do a job ((a) touched " + (X.read ? how.a : "NOT READ") + ", (b) sight " + how.b + ", (c) launch kill " + (X.read ? how.c : "NOT READ") + ")" + (fails.length ? "; no job: " + fails.slice(0, 4).join("; ") + (fails.length > 4 ? " (+" + (fails.length - 4) + ")" : "") : "") + (X.read ? "" : "; no --touches: (a) / (c) not read") };
  },
  P5(R) {
    const l65 = R.row.line65;
    if (!Array.isArray(l65) || l65.length < 2) return { v: "N/A", n: "no line65 in the row" };
    const spots = rangedSpots(R.row).map((s) => [s[0], s[1] + 1.2, s[2]]);
    const seen = (q) => spots.some((s) => { const d = Math.hypot(s[0] - q[0], s[2] - q[2]); return d >= 6 && d <= 24 && blockers(R, s, q).size === 0; });
    const share = (pts) => (pts.length ? pts.filter(seen).length / pts.length : 0);
    const W = share(walkLine(R.row)), V = share(polyLine(l65));
    const enemies = uniq([].concat(...waveSets(R.row).map((w) => w.nodes)));
    let near = Infinity;
    for (const q of polyLine(l65)) for (const e of enemies) near = Math.min(near, Math.hypot(q[0] - e[0], q[2] - e[2]));
    const a = W >= 0.5 && V <= W - 0.3, b = near <= 6;
    return { v: a || b ? "PASS" : "FAIL", n: "walk line in a ranged spawn's sight at 6-24 m " + F1(W * 100) + " % (>= 50 %), line65 " + F1(V * 100) + " % (<= walk - 30 points: " + (a ? "yes" : "no") + "); line65's nearest enemy spawn " + (near === Infinity ? "-" : F1(near)) + " m (<= 6: " + (b ? "yes" : "no") + "); " + spots.length + " ranged spawns" };
  },
  P9(R) {
    const planes = R.solids.filter((p) => p.kind === "box" && (p.src.kind === "deck" || p.src.kind === "hull") && !boundingWall(p, R.rect) && p.max[1] - R.floorY >= 1 && Math.min(p.max[0] - p.min[0], p.max[2] - p.min[2]) >= 6);
    if (!planes.length) return { v: "N/A", n: "no plane 1 m or more up with a short side of 6 m or more" };
    if (!R.nav) return { v: "FAIL", n: planes.length + " plane(s) but no nav grid read (give --file / --dir): not checked" };
    const N = R.nav.nodes, L = R.nav.links, kinds = R.nav.linkKinds || { walk: 0, jump: 1 };
    const ok = new Set([kinds.walk, kinds.jump]);
    const out = [];
    let pass = 0;
    for (const p of planes) {
      const top = p.max[1], on = (i) => N.x[i] >= p.min[0] - 0.01 && N.x[i] <= p.max[0] + 0.01 && N.z[i] >= p.min[2] - 0.01 && N.z[i] <= p.max[2] + 0.01 && Math.abs(N.y[i] - top) <= 0.3;
      const tops = [];
      for (let i = 0; i < N.x.length; i++) {
        if (N.x[i] < R.rect[0] - 1 || N.x[i] > R.rect[2] + 1 || N.z[i] < R.rect[1] - 1 || N.z[i] > R.rect[3] + 1) continue;
        for (let k = L.linkStart[i]; k < L.linkStart[i + 1]; k++) { const j = L.linkTo[k]; if (ok.has(L.linkKind[k]) && on(j) && !on(i) && N.y[i] < top - 0.1) tops.push([N.x[j], N.z[j]]); }
      }
      let far = 0;
      for (let a = 0; a < tops.length; a++) for (let b = a + 1; b < tops.length; b++) far = Math.max(far, Math.hypot(tops[a][0] - tops[b][0], tops[a][1] - tops[b][1]));
      const good = tops.length >= 2 && far >= 8;
      if (good) pass++;
      out.push(p.key + " " + F1(p.max[0] - p.min[0]) + " x " + F1(p.max[2] - p.min[2]) + " m at " + F1(top - R.floorY) + " m: " + tops.length + " climb top(s), farthest apart " + F1(far) + " m" + (good ? "" : " (NO)"));
    }
    return { v: pass === planes.length ? "PASS" : "FAIL", n: pass + " / " + planes.length + " planes with 2 climb tops >= 8 m apart: " + out.join("; ") };
  },
  P11(R, X) {
    if (!X.arch) return { v: "N/A", n: "the card names no TOWER / GALLERY / SPLIT LEVEL (" + (X.cardFrom || "no card") + ")" };
    const line = walkLine(R.row), a = line[0], b = line[line.length - 1];
    const spots = rangedSpots(R.row);
    if (!spots.length) return { v: "N/A", n: X.arch + " card, but no wave holds a ranged enemy" };
    const bad = [];
    let hiN = 0, bandN = 0;
    for (const s of spots) {
      const h = s[1] - R.floorY, d = segDist(s, a, b), hi = h >= 2.5, band = d >= 12 && d <= 24;
      if (hi) hiN++; if (band) bandN++;
      if (!(hi && band)) bad.push("(" + s.map(F1).join(", ") + ") " + F1(h) + " m up, " + F1(d) + " m from the lane");
    }
    return { v: bad.length ? "FAIL" : "PASS", n: X.arch + " card (" + X.cardFrom + "): " + spots.length + " ranged spawns (each node of a wave's set that holds a ranged enemy): " + hiN + " 2.5 m or more up, " + bandN + " 12-24 m from the walk lane (the spawn -> exit line)" + (bad.length ? "; failing: " + bad.slice(0, 4).join("; ") + (bad.length > 4 ? " (+" + (bad.length - 4) + ")" : "") : "") };
  },
  P17(R) {
    const lenOf = (p) => Math.max(p.max[0] - p.min[0], p.max[2] - p.min[2]), thin = (p) => Math.min(p.max[0] - p.min[0], p.max[2] - p.min[2]);
    const rides = R.solids.filter((p) => p.kind === "box" && p.src.ride === true);
    const others = R.solids.filter((p) => p.kind === "box" && p.src.ride !== true && !boundingWall(p, R.rect) && thin(p) <= 2 && p.max[1] - R.floorY >= 4 && lenOf(p) >= 8);
    if (!rides.length && !others.length) return { v: "N/A", n: "no ride wall and no wall-like box 4 m x 8 m" };
    const short = rides.filter((p) => !(p.max[1] - R.floorY >= 4 && lenOf(p) >= 8));
    return { v: short.length || others.length ? "FAIL" : "PASS", n: (rides.length - short.length) + " / " + rides.length + " ride walls 4 m tall and 8 m long" + (short.length ? " (short: " + short.map((p) => p.key + " " + F1(p.max[1] - R.floorY) + " x " + F1(lenOf(p)) + " m").slice(0, 3).join("; ") + ")" : "") + "; " + others.length + " non-ride wall(s) in that band" + (others.length ? " (" + others.map((p) => p.key).slice(0, 3).join("; ") + ")" : "") + "; the strip = ride: true (ride36 faces " + rides.filter((p) => p.src.ride36).length + ")" };
  },
};
const ORDER = ["OPEN", "P1", "P4", "P5", "P9", "P11", "P17"];
// NIGHT67 T1b fix (Logs/n67/verdict-cards.md item A: "Card v1 was a GALLERY" read as GALLERY): the archetype is read from the card's
// Archetype line ONLY (`**Archetype: CHUTE** (...)`: the words after "Archetype:" up to "**", "(" or the line's end); a card with no such line
// reads none (P11 N/A, said on the line)
function archetypeOf(text) {
  const line = text.split(/\r?\n/).find((l) => /archetype\s*:/i.test(l));
  if (!line) return { arch: null, read: "no Archetype: line" };
  const words = line.replace(/^.*?archetype\s*:\s*/i, "").replace(/\*\*.*$/, "").replace(/\(.*$/, "").trim().toUpperCase();
  const hit = ARCH.find((a) => words === a || words.startsWith(a + " ") || words.startsWith(a + ",") || words.startsWith(a + ":"));
  return { arch: hit || null, read: "Archetype: " + (words || "?") };
}
function cardOf(file, id) {
  const f = file || path.join("Research/2026-10-24-wave67/cards", id + ".md");
  if (!fs.existsSync(rel(f))) return { arch: null, cardFrom: null };
  const a = archetypeOf(fs.readFileSync(rel(f), "utf8"));
  return { arch: a.arch, cardFrom: f.replace(/\\/g, "/") + " (" + a.read + ")" };
}
function lint(R, X) {
  const res = ORDER.map((k) => Object.assign({ rule: k }, RULES[k](R, X)));
  const app = res.filter((r) => r.v !== "N/A"), passed = app.filter((r) => r.v === "PASS");
  return { res, app: app.length, passed: passed.length, verdict: app.length && passed.length === app.length ? "PASS" : "FAIL" };
}
function print(id, L) {
  for (const r of L.res) console.log((r.rule + "     ").slice(0, 5) + " " + (r.v + "    ").slice(0, 4) + " " + r.n);
  console.log("n67-area: " + L.verdict + " " + id + " (" + L.passed + "/" + L.app + ")");
}

// ---------------------------------------------------------------- the baseline: the 25 shipped fight rooms
const SHIPPED = [
  ["n52-foundry", "Design/levels/n52/foundry.plan.json", "Design/levels/n52-foundry.json", ["a2", "a3", "a4", "a5", "b1", "a8"]],
  ["n52-iceline", "Design/levels/n52/iceline.plan.json", "Design/levels/n52-iceline.json", null],
  ["n57-kiln", "Design/levels/n57/kiln.plan.json", "Design/levels/n57-kiln.json", null],
  ["n52-span", "Design/levels/n52/span.plan.json", "Design/levels/n52-span.json", null],
  ["n52-spire", "Design/levels/n52/spire.plan.json", "Design/levels/n52-spire.json", null],
];
function baseline() {
  const tally = Object.fromEntries(ORDER.map((k) => [k, { PASS: [], FAIL: [], "N/A": [] }]));
  let n = 0;
  console.log("n67-area --baseline: the 25 shipped fight rooms, each from its plan + level file; no --touches (P4's (a) / (c) not read: 25 skip v2 runs not made), no cards (P11 N/A); " + new Date().toISOString());
  for (const [lv, pp, lp, only] of SHIPPED) {
    const plan = JSON.parse(fs.readFileSync(rel(pp), "utf8")), level = JSON.parse(fs.readFileSync(rel(lp), "utf8"));
    for (const id of only || plan.rooms.filter((r) => r.kind === "room").map((r) => r.id)) {
      const L = lint(roomOf(plan, level, id), { touch: [], kills: [], read: false, arch: null, cardFrom: null });
      n++;
      console.log("\n## " + lv + " " + id);
      print(id, L);
      for (const r of L.res) tally[r.rule][r.v].push(id);
    }
  }
  console.log("\n## per rule over " + n + " rooms");
  for (const k of ORDER) {
    const t = tally[k], disc = t.PASS.length && t.FAIL.length;
    console.log((k + "     ").slice(0, 5) + " PASS " + t.PASS.length + ", FAIL " + t.FAIL.length + " (" + t.FAIL.join(" ") + "), N/A " + t["N/A"].length + (disc ? "" : t.PASS.length + t.FAIL.length === 0 ? ": NOT DISCRIMINATING (N/A on every shipped room)" : ": NOT DISCRIMINATING (every applicable shipped room " + (t.PASS.length ? "PASSes" : "FAILs") + ")"));
  }
}

// ---------------------------------------------------------------- the self-test: a planted PASS and FAIL per rule
function selftest() {
  // a synthetic 30 x 30 m room on x 0..30, z -30..0, the spawn at the south (z -1), the exit north (z -29); the walk line runs x 15
  const base = () => ({ rooms: [{ id: "t1", kind: "room", rect: [0, -30, 30, 0], spawn: { at: [15, 0, -1], yawDeg: 0 }, exit: [15, 0, -29], waves: [["husk", "husk"]], nodes: [[5, 0, -15], [25, 0, -15]] }], boxes: [], ramps: [], pads: [], pits: [], tunnels: [], props: [] });
  const grid = (pl, ...rects) => { for (const [x, z] of rects) pl.boxes.push({ zone: "t1", kind: "hull", min: [x, 0, z], max: [x + 1, 1, z + 1], job: "cover", why: "planted" }); };
  const pillars = (pl) => { for (let x = 4; x < 30; x += 9) for (let z = -26; z < 0; z += 9) grid(pl, [x, z]); };
  const navOf = (nodes, links) => { const x = nodes.map((n) => n[0]), y = nodes.map((n) => n[1]), z = nodes.map((n) => n[2]); const start = [0], to = [], kind = []; nodes.forEach((_, i) => { for (const [a, b, k] of links) if (a === i) { to.push(b); kind.push(k); } start.push(to.length); }); return { nodes: { x, y, z }, links: { linkStart: start, linkTo: to, linkKind: kind }, linkKinds: { walk: 0, jump: 1, drop: 2, gap: 3 } }; };
  const X0 = { touch: [], kills: [], read: true, arch: null, cardFrom: null };
  const run = (plan, nav, X) => lint(Object.assign(roomOf(plan, null, "t1"), { nav }), Object.assign({}, X0, X || {}));
  const cases = [];
  const want = (name, rule, plan, v, nav, X) => { const L = run(plan, nav, X); const r = L.res.find((q) => q.rule === rule); cases.push([r.v === v, rule + " PLANTED " + v + " (" + name + "): " + r.v + " | " + r.n]); };
  // OPEN: the bare room (a 30 m square of open floor) FAILs; pillars every 9 m PASS
  want("the bare room", "OPEN", base(), "FAIL");
  { const p = base(); pillars(p); want("pillars every 9 m", "OPEN", p, "PASS"); }
  // P1: the last wave's spitter on floor nodes FAILs; on a waveNodes43 set all 4 m up PASSes; no ranged: N/A
  { const p = base(); p.rooms[0].waves = [["husk"], ["husk", "spitter"]]; want("the last wave's spitter on floor nodes", "P1", p, "FAIL"); }
  { const p = base(); p.rooms[0].waves = [["husk"], ["husk", "spitter"]]; p.rooms[0].waveNodes43 = [null, [[5, 4, -20], [25, 4, -20]]]; want("its own set 4 m up", "P1", p, "PASS"); }
  want("no ranged enemy", "P1", base(), "N/A");
  // P4: a 3 m pillar in a corner with no job FAILs; the same pillar cutting a spitter's sight to the walk line PASSes; touched 5/10 PASSes
  { const p = base(); p.boxes.push({ zone: "t1", kind: "hull", min: [2, 0, -28], max: [3, 3, -27], job: "cover", why: "planted" }); want("a corner pillar with no job", "P4", p, "FAIL"); }
  { const p = base(); p.rooms[0].waves = [["spitter"]]; p.rooms[0].nodes = [[3, 0, -15]]; p.boxes.push({ zone: "t1", kind: "hull", min: [8, 0, -16], max: [9, 3, -14], job: "cover", why: "planted" }); want("a pillar between a spitter and the walk line", "P4", p, "PASS"); }
  { const p = base(); p.boxes.push({ zone: "t1", kind: "hull", min: [2, 0, -28], max: [3, 3, -27], job: "cover", why: "planted" }); want("the corner pillar touched on 5 / 10 seeds", "P4", p, "PASS", null, { touch: [{ key: "box(2,0,-28..3,3,-27)", seeds: 5, of: 10 }] }); }
  // P5: line65 in a spitter's open sight FAILs; line65 behind a wall (and the walk line open) PASSes; no line65: N/A
  { const p = base(); p.rooms[0].waves = [["spitter"]]; p.rooms[0].nodes = [[15, 0, -15]]; p.rooms[0].line65 = [[27, 0, -5], [27, 0, -25]]; want("line65 in the spitter's open sight, 12 m from it", "P5", p, "FAIL"); }
  { const p = base(); p.rooms[0].waves = [["spitter"]]; p.rooms[0].nodes = [[5, 0, -15]]; p.rooms[0].line65 = [[27, 0, -5], [27, 0, -25]]; p.boxes.push({ zone: "t1", kind: "wall", min: [24, 0, -28], max: [25, 4, -2], job: "cover", why: "planted" }); want("line65 behind a wall, the walk line open", "P5", p, "PASS"); }
  want("no line65", "P5", base(), "N/A");
  // P9: an 8 x 10 m deck with one ramp FAILs; with ramps at both ends (tops 9 m apart) PASSes; a 5 x 18 m gallery: N/A
  { const p = base(); p.boxes.push({ zone: "t1", kind: "deck", min: [10, 0, -20], max: [18, 3, -10], job: "plane", why: "planted" }); const nav = navOf([[12, 0, -9], [12, 3, -11], [16, 3, -19]], [[0, 1, 0]]); want("a deck with one climb", "P9", p, "FAIL", nav); }
  { const p = base(); p.boxes.push({ zone: "t1", kind: "deck", min: [10, 0, -20], max: [18, 3, -10], job: "plane", why: "planted" }); const nav = navOf([[12, 0, -9], [12, 3, -11], [16, 3, -19], [16, 0, -21]], [[0, 1, 0], [3, 2, 1]]); want("a deck with climbs at both ends (tops 8.9 m apart)", "P9", p, "PASS", nav); }
  { const p = base(); p.boxes.push({ zone: "t1", kind: "deck", min: [1, 0, -24], max: [6, 4, -6], job: "plane", why: "planted" }); want("a 5 x 18 m gallery (short side 5 m)", "P9", p, "N/A"); }
  // P11: a GALLERY card with a spitter on the floor near the lane FAILs; 4 m up, 13 m from the lane PASSes; no card: N/A
  { const p = base(); p.rooms[0].waves = [["spitter"]]; p.rooms[0].nodes = [[10, 0, -15]]; want("GALLERY, a spitter on the floor 5 m from the lane", "P11", p, "FAIL", null, { arch: "GALLERY", cardFrom: "planted" }); }
  { const p = base(); p.rooms[0].waves = [["spitter"]]; p.rooms[0].nodes = [[2, 4, -15]]; want("GALLERY, a spitter 4 m up, 13 m from the lane", "P11", p, "PASS", null, { arch: "GALLERY", cardFrom: "planted" }); }
  want("no card", "P11", base(), "N/A");
  // the card's Archetype line only (verdict-cards.md item A): a CHUTE card that mentions "Card v1 was a GALLERY" reads CHUTE; a GALLERY line reads GALLERY
  {
    const chute = "# g7 THE COAL CHUTE\n\n**Archetype: CHUTE** (research.md section 2): every melee path runs down one chute. (Card v1 was a GALLERY: ...)\n**Verb: parry.**\n";
    const gal = "# x\n**Archetype: GALLERY** (research.md section 2): the ranged up high.\n";
    const split = "# y\n**Archetype: SPLIT LEVEL** (a plane over a plane)\n";
    const a1 = archetypeOf(chute), a2 = archetypeOf(gal), a3 = archetypeOf(split), a4 = archetypeOf("# no archetype line\nGALLERY TOWER\n");
    cases.push([a1.arch === null && a2.arch === "GALLERY" && a3.arch === "SPLIT LEVEL" && a4.arch === null, "P11 PLANTED card lines: '**Archetype: CHUTE** ... (Card v1 was a GALLERY)' reads " + (a1.arch || "none") + " (" + a1.read + "); '**Archetype: GALLERY**' reads " + a2.arch + "; '**Archetype: SPLIT LEVEL**' reads " + a3.arch + "; no Archetype line (GALLERY in the text) reads " + (a4.arch || "none")]);
    const p = base(); p.rooms[0].waves = [["spitter"]]; p.rooms[0].nodes = [[10, 0, -15]];
    const L = run(p, null, { arch: a1.arch, cardFrom: "planted chute card" }); const r = L.res.find((q) => q.rule === "P11");
    cases.push([r.v === "N/A", "P11 PLANTED N/A (the CHUTE card that mentions a GALLERY, a floor spitter 5 m from the lane): " + r.v + " | " + r.n]);
  }
  // P17: a 3 m ride wall FAILs; a 5 x 10 m ride wall PASSes; the same plus a 5 x 10 m plain wall FAILs; neither: N/A
  { const p = base(); p.boxes.push({ zone: "t1", kind: "wall", min: [20, 0, -20], max: [21, 3, -10], ride: true, job: "line", why: "planted" }); want("a 3 m ride wall", "P17", p, "FAIL"); }
  { const p = base(); p.boxes.push({ zone: "t1", kind: "wall", min: [20, 0, -20], max: [21, 5, -10], ride: true, job: "line", why: "planted" }); want("a 5 x 10 m ride wall", "P17", p, "PASS"); }
  { const p = base(); p.boxes.push({ zone: "t1", kind: "wall", min: [20, 0, -20], max: [21, 5, -10], ride: true, job: "line", why: "planted" }, { zone: "t1", kind: "wall", min: [8, 0, -20], max: [9, 5, -10], job: "cover", why: "planted" }); want("a ride wall and a plain wall in its band", "P17", p, "FAIL"); }
  want("neither", "P17", base(), "N/A");
  // the last line: N/A never counts; one FAIL fails the room
  { const p = base(); pillars(p); const L = run(p); cases.push([L.verdict === "PASS" && L.app === 1 && L.passed === 1, "the last line, a room where only OPEN applies and PASSes: n67-area: " + L.verdict + " (" + L.passed + "/" + L.app + ")"]); }
  { const L = run(base()); cases.push([L.verdict === "FAIL" && L.passed === 0, "the last line, the bare room: n67-area: " + L.verdict + " (" + L.passed + "/" + L.app + ")"]); }
  for (const [ok, m] of cases) console.log((ok ? "PASS " : "FAIL ") + m);
  const bad = cases.filter((c) => !c[0]).length;
  console.log("n67-area --selftest: " + (bad ? bad + " FAIL" : "PLANTED GREEN") + " (" + cases.length + " cases: a planted PASS and FAIL for each of " + ORDER.join(" ") + ", N/A where a rule has nothing to judge)");
  process.exit(bad ? 1 : 0);
}

// ---------------------------------------------------------------- main
function main() {
  const args = process.argv.slice(2);
  const KNOWN = ["--level", "--dir", "--touches", "--card", "--plan", "--file", "--baseline", "--selftest"];
  for (const a of args) if (a.startsWith("--") && !KNOWN.includes(a)) throw new Error("unknown option " + a + " (" + KNOWN.join(" ") + ")");
  if (args.includes("--selftest")) return selftest();
  if (args.includes("--baseline")) return baseline();
  const opt = (n) => { const i = args.indexOf(n); return i >= 0 ? args[i + 1] : null; };
  const opts = (n) => args.map((a, i) => (a === n ? args[i + 1] : null)).filter(Boolean);
  const id = args.find((a, i) => !a.startsWith("--") && !(i > 0 && args[i - 1].startsWith("--") && !["--baseline", "--selftest"].includes(args[i - 1])));
  if (!id) throw new Error("usage: n67-area.cjs --level <L> [--dir <folder>] [--touches <file>]... [--card <md>] <room> | --plan <p> --file <f> <room> | --baseline | --selftest");
  let planP = opt("--plan"), levelP = opt("--file");
  const L = opt("--level"), dir = opt("--dir");
  if (L) {
    const T = JSON.parse(fs.readFileSync(rel("Tools/night/levels67.json"), "utf8"))[L];
    if (!T || L.startsWith("_")) throw new Error("--level " + L + ": not in Tools/night/levels67.json");
    planP = planP || (dir ? path.join(dir, L + ".plan.json") : T.dir + "/" + L + ".plan.json");
    levelP = levelP || (dir ? path.join(dir, T.id + ".json") : "Design/levels/" + T.id + ".json");
  }
  if (!planP || !levelP) throw new Error("give --level <L> (with --dir for a round) or --plan and --file");
  for (const f of [planP, levelP].concat(opts("--touches"))) if (!fs.existsSync(rel(f))) throw new Error("no file " + f);
  const plan = JSON.parse(fs.readFileSync(rel(planP), "utf8")), level = JSON.parse(fs.readFileSync(rel(levelP), "utf8"));
  const X = Object.assign(readTouches(opts("--touches"), id), cardOf(opt("--card"), id));
  console.log("n67-area: room " + id + "; plan " + String(planP).replace(/\\/g, "/") + ", level " + String(levelP).replace(/\\/g, "/") + (dir ? " (the round's folder " + dir + ")" : " (the tracked files: no --dir)") + "; touches " + (X.read ? opts("--touches").join(", ") + " (" + X.touch.length + " touch rows, " + X.kills.length + " launch kills)" : "none (P4's (a) / (c) not read)") + "; card " + (X.cardFrom ? X.cardFrom + " (" + (X.arch || "no TOWER / GALLERY / SPLIT LEVEL") + ")" : "none"));
  const res = lint(roomOf(plan, level, id), X);
  print(id, res);
  process.exit(res.verdict === "PASS" ? 0 : 1);
}
try { main(); } catch (e) { console.error("n67-area: REFUSED: " + e.message); process.exit(2); }
