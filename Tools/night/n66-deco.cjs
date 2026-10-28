// NIGHT66 builder T (T1c; slate T1c line 2, M4; FORGE66 redteam.md finding 9, research.md section 3's decoration test): THE DECORATION
// CHECKER v2. Every piece of a room added or moved since <sha> must do a job, by four tests, else it FAILs:
//   (a) TOUCHED: skip v2's verb-aware run touched it on >= 3 of 10 seeds (its `n65verb skip touch:` lines; the key is the piece's geometry)
//   (b) SIGHT: it blocks a ranged spawn's line of sight to >= 1 sample of the walk line (static: from each spot a ranged enemy of the room's
//       waves spawns at, eye 1.2 m up, to the spawn -> exit line sampled every 1 m at eye 1.6 m; NIGHT66 T1c fix: a sight counts only when no
//       OTHER solid of the room cuts it and this piece does)
//   (c) LAUNCH: a launch36 kill on it (skip v2's `n65verb skip launchkill:` lines: the burst within 1 m of the piece's box)
//   (d) MARKER: a cyan marker (a light or prop of colour cyan), one of the room's first 2, within 6 m on the floor plan of the room's exit
//       (row.exit), or of the piece its `marks` field names (a piece id, its why head or its key) when that piece does a job by (a)-(c)
//       (NIGHT66 T1c fix, Logs/n66/verdict-T1c.md: "6 m of any solid" held on 88-100 % of every room's floor)
//   node Tools/night/n66-deco.cjs --selftest [room] [touch file]   the planted cases: B+ / B- (sight), C+ (a launch kill), A- (a corner
//       crate), D- (a corner light, no marks) / D+ (a light by the exit); test (a)'s planted pair is t1c-deco-plant.sh's (a skip v2 run)
// Pieces: the room file's boxes, ramps and pads (zone = the room) and its cyan lights / props (room = the room). Other lights and the
// colliderless props are DRESSING: listed, not judged (M4 judges pieces), with a WARN when a round added dressing and no piece.
//   node Tools/night/n66-deco.cjs --base <sha> <room> [--touches <skip v2 output>]... [--file <room file>]
//       --file: the room file judged (default Design/levels/n65/rooms/<room>.json in the tree); the base is that path at <sha>
//   node Tools/night/n66-deco.cjs --plant <room> <out.json>     the planted case's room file: the room's file plus PLANTED crate A (1 m, in a
//       free corner of the rect) and PLANTED crate B (a 0.3 m step 2-4 m ahead of the spawn, on the walk line): A must FAIL, B must PASS
//       once skip v2 has run on a level holding them (Research/2026-10-23-wave66/T/t1c-deco-plant.sh)
// Exit 0 when every judged piece passes, 1 when one FAILs, 2 on a usage error. Probe numbers (3 / 10, 1 m, 6 m, eye heights), not gameplay.
const fs = require("fs");
const path = require("path");
const cp = require("child_process");
const ROOT = path.join(__dirname, "../..");
const args = process.argv.slice(2);
const opt = (n) => { const i = args.indexOf(n); return i >= 0 ? args[i + 1] : null; };
const opts = (n) => args.map((a, i) => (a === n ? args[i + 1] : null)).filter((x) => x);
const rel = (p) => (path.isAbsolute(p) ? p : path.join(ROOT, p));
const R2 = (v) => { const r = Math.round(v * 100) / 100; return Object.is(r, -0) ? "0" : String(r); };
const hyp = (a, b) => Math.sqrt(a * a + b * b);

// ---------------------------------------------------------------- the pieces of a room file
function pieces(file, id) {
  const out = [];
  for (const b of file.boxes || []) {
    if (b.zone !== id || b.kind === "floor") continue;
    const head = String(b.why || "").split(/[:(]/)[0].trim().slice(0, 48);
    out.push({ kind: "box", key: `box(${b.min.map(R2).join(",")}..${b.max.map(R2).join(",")})`, nums: b.min.concat(b.max), min: b.min, max: b.max, label: (b.job || "-") + ": " + head, ident: head, src: b });
  }
  for (const r of file.ramps || []) {
    if (r.zone !== id) continue;
    const x = r.from[0], z = r.from[1], dx = r.to[0] - x, dz = r.to[1] - z, len = hyp(dx, dz);
    const yaw = Math.atan2(-dx === 0 ? 0 : -dx, -dz === 0 ? 0 : -dz); // Build_drydock.Wedge
    const w = { x, z, yaw, length: len, width: r.width, rise: r.rise };
    const c = wedgeCorners(w);
    out.push({ kind: "ramp", key: `ramp(${R2(x)},${R2(z)},${R2(len)},${R2(r.width)},${R2(r.rise)})`, nums: [x, z, len, r.width, r.rise], wedge: w,
      min: [Math.min(...c.map((p) => p[0])), 0, Math.min(...c.map((p) => p[2]))], max: [Math.max(...c.map((p) => p[0])), r.rise, Math.max(...c.map((p) => p[2]))],
      label: (r.job || "-") + ": " + String(r.why || "").split(/[:(]/)[0].trim().slice(0, 48), ident: String(r.why || "").split(/[:(]/)[0].trim().slice(0, 48), src: r });
  }
  for (const p of file.pads || []) {
    if (p.zone !== id) continue;
    out.push({ kind: "pad", key: `pad(${p.at.map(R2).join(",")})`, nums: p.at.slice(), min: [p.at[0] - 1, p.at[1], p.at[2] - 1], max: [p.at[0] + 1, p.at[1] + 0.2, p.at[2] + 1],
      label: (p.job || "pad") + ": " + String(p.why || "").split(/[:(]/)[0].trim().slice(0, 48), ident: String(p.why || "").split(/[:(]/)[0].trim().slice(0, 48), src: p });
  }
  const marker = (o) => String(o.color || o.role || "").toLowerCase() === "cyan";
  for (const l of file.lights || []) {
    if (l.room !== id) continue;
    out.push({ kind: marker(l) ? "marker" : "dressing", what: "light", key: `light(${l.id || "?"}@${(l.at || []).map(R2).join(",")})`, ident: l.id, at: l.at, src: l, label: "light " + (l.id || "?") + (l.color ? " " + l.color : "") });
  }
  for (const p of file.props || []) {
    if (p.room !== id) continue;
    const solid = p.collider === true;
    const half = (p.size || [1, 1, 1]).map((s) => s / 2);
    const at = p.at || [0, 0, 0];
    out.push({ kind: marker(p) ? "marker" : solid ? "propbox" : "dressing", what: "prop", key: `prop(${p.id || "?"}@${at.map(R2).join(",")})`, ident: p.id, at,
      min: [at[0] - half[0], at[1] - half[1], at[2] - half[2]], max: [at[0] + half[0], at[1] + half[1], at[2] + half[2]], src: p, label: "prop " + (p.id || "?") + " " + (p.kind || "") + (p.decor ? " (decor)" : "") });
  }
  return out;
}
function wedgeCorners(w) { // ArenaMap.WedgeCorners
  const ax = -Math.sin(w.yaw), az = -Math.cos(w.yaw), rx = Math.cos(w.yaw), rz = -Math.sin(w.yaw), hw = w.width / 2;
  const P = (a, c, y) => [w.x + ax * a + rx * c, y, w.z + az * a + rz * c];
  return [P(0, -hw, 0), P(0, hw, 0), P(w.length, -hw, 0), P(w.length, hw, 0), P(w.length, -hw, w.rise), P(w.length, hw, w.rise)];
}
function inside(pc, p) {
  if (pc.kind === "ramp") {
    const w = pc.wedge, ax = -Math.sin(w.yaw), az = -Math.cos(w.yaw), rx = Math.cos(w.yaw), rz = -Math.sin(w.yaw);
    const dx = p[0] - w.x, dz = p[2] - w.z, al = dx * ax + dz * az, ac = dx * rx + dz * rz;
    return w.length > 1e-9 && al > 0 && al < w.length && Math.abs(ac) < w.width / 2 && p[1] > 0 && p[1] < w.rise * al / w.length;
  }
  return p[0] > pc.min[0] && p[0] < pc.max[0] && p[1] > pc.min[1] && p[1] < pc.max[1] && p[2] > pc.min[2] && p[2] < pc.max[2];
}
function boxDist(pc, p) { // distance from a point to the piece's box (0 inside)
  const d = [0, 1, 2].map((k) => Math.max(pc.min[k] - p[k], 0, p[k] - pc.max[k]));
  return Math.sqrt(d[0] * d[0] + d[1] * d[1] + d[2] * d[2]);
}
const solidKinds = ["box", "ramp", "propbox"];
/// NIGHT66 T1c fix 2: a bounding wall of the room: a box on an edge of its rect (within 0.1 m) along half that side or more (test d never
/// takes one as a marker's target)
function boundingWall(q, rect) {
  if (!rect || q.kind !== "box") return false;
  const [x0, z0, x1, z1] = rect;
  return ((q.min[0] <= x0 + 0.1 || q.max[0] >= x1 - 0.1) && (q.max[2] - q.min[2]) >= 0.5 * (z1 - z0)) || ((q.min[2] <= z0 + 0.1 || q.max[2] >= z1 - 0.1) && (q.max[0] - q.min[0]) >= 0.5 * (x1 - x0));
}

// ---------------------------------------------------------------- skip v2's touch table and launch kills
function readTouches(files, id) {
  const touch = [], kills = [];
  for (const f of files) {
    for (const l of fs.readFileSync(rel(f), "utf8").split(/\r?\n/)) {
      let m = l.match(/^n65verb skip touch: room (\S+) \| (\S+) \| seeds (\d+)\/(\d+) \| steps (\d+)/);
      if (m && m[1] === id) { touch.push({ key: m[2], nums: (m[2].match(/-?\d+(\.\d+)?/g) || []).map(Number), seeds: +m[3], of: +m[4], steps: +m[5], file: f }); continue; }
      m = l.match(/^n65verb skip launchkill: room (\S+) \| seed (\d+) \| at \((-?[\d.]+), (-?[\d.]+), (-?[\d.]+)\) kills (\d+)/);
      if (m && m[1] === id) kills.push({ seed: +m[2], at: [+m[3], +m[4], +m[5]], kills: +m[6] });
    }
  }
  return { touch, kills };
}
const sameNums = (a, b) => a.length === b.length && a.every((v, i) => Math.abs(v - b[i]) < 0.006);

// ---------------------------------------------------------------- the ranged spots and the walk line (the room row)
function rangedSpots(row) {
  const ranged = new Set(["spitter"]); // level 1's ranged kind (the spitter's glob; husks, swarmers, chargers are melee)
  const waves = row.waves || [], wn = row.waveNodes43 || [];
  const spots = [];
  waves.forEach((w, i) => {
    if (!(w || []).some((k) => ranged.has(k))) return;
    const ns = wn[i] && wn[i].length ? wn[i] : (row.nodes || []).concat(row.highNodes || []);
    for (const n of ns) spots.push([n[0], n[1] + 1.2, n[2]]);
  });
  return spots;
}
function walkLine(row) {
  const a = row.spawn && row.spawn.at, b = row.exit;
  if (!a || !b) return [];
  const n = Math.max(1, Math.round(hyp(b[0] - a[0], b[2] - a[2])));
  const out = [];
  for (let k = 0; k <= n; k++) { const t = k / n; out.push([a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t + 1.6, a[2] + (b[2] - a[2]) * t]); }
  return out;
}
function blocks(pc, from, to) { // the piece's solid on the segment (sampled every 0.1 m, the ends' 0.3 m left out)
  const d = Math.sqrt((to[0] - from[0]) ** 2 + (to[1] - from[1]) ** 2 + (to[2] - from[2]) ** 2);
  for (let s = 0.3; s < d - 0.3; s += 0.1) { const t = s / d; if (inside(pc, [from[0] + (to[0] - from[0]) * t, from[1] + (to[1] - from[1]) * t, from[2] + (to[2] - from[2]) * t])) return true; }
  return false;
}

// ---------------------------------------------------------------- the planted case's room file
function plant(id, out) {
  const fp = rel("Design/levels/n65/rooms/" + id + ".json");
  const file = JSON.parse(fs.readFileSync(fp, "utf8"));
  const row = file.row, [x0, z0, x1, z1] = row.rect;
  const solids = pieces(file, id).filter((p) => solidKinds.includes(p.kind));
  const free = (mn, mx) => !solids.some((p) => mn[0] < p.max[0] && mx[0] > p.min[0] && mn[2] < p.max[2] && mx[2] > p.min[2] && mn[1] < p.max[1] && mx[1] > p.min[1]);
  let A = null;
  for (const [cx, cz] of [[x0 + 0.7, z0 + 0.7], [x1 - 1.7, z0 + 0.7], [x0 + 0.7, z1 - 1.7], [x1 - 1.7, z1 - 1.7]]) {
    const mn = [cx, 0, cz], mx = [cx + 1, 1, cz + 1];
    if (free(mn, mx)) { A = { zone: id, kind: "hull", min: mn, max: mx, job: "cover: PLANTED", why: "PLANTED crate A (n66-deco.cjs --plant): a 1 m crate in a free corner of the rect: no job, must FAIL" }; break; }
  }
  const a = row.spawn.at, yaw = (row.spawn.yawDeg || 0) * Math.PI / 180, fx = -Math.sin(yaw), fz = -Math.cos(yaw);
  let B = null;
  for (const ahead of [2, 2.5, 3, 3.5, 4]) {
    const cx = a[0] + fx * ahead, cz = a[2] + fz * ahead, mn = [cx - 0.8, 0, cz - 0.8], mx = [cx + 0.8, 0.3, cz + 0.8];
    if (free(mn, mx)) { B = { zone: id, kind: "hull", min: mn.map((v) => Math.round(v * 100) / 100), max: mx.map((v) => Math.round(v * 100) / 100), job: "step: PLANTED", why: "PLANTED crate B (n66-deco.cjs --plant): a 0.3 m step " + ahead + " m ahead of the spawn, on the walk line: the pilot stands on it, must PASS" }; break; }
  }
  if (!A || !B) { console.log("n66-deco --plant: REFUSED: no free spot for crate " + (!A ? "A (corner)" : "B (ahead of the spawn)")); process.exit(2); }
  file.boxes.push(A, B);
  fs.writeFileSync(rel(out), JSON.stringify(file, null, 1) + "\n");
  console.log(`n66-deco --plant: ${id}'s room file + crate A box(${A.min.map(R2).join(",")}..${A.max.map(R2).join(",")}) (corner) + crate B box(${B.min.map(R2).join(",")}..${B.max.map(R2).join(",")}) (step) -> ${out}`);
}

// ---------------------------------------------------------------- main
function main() {
  if (args[0] === "--plant") { plant(args[1], args[2]); return 0; }
  const base = opt("--base");
  const id = args.find((a, i) => !a.startsWith("--") && !(i > 0 && args[i - 1].startsWith("--")));
  if (!base || !id) { console.log("usage: node Tools/night/n66-deco.cjs --base <sha> <room> [--touches <skip v2 output>]... [--file <room file>]  |  --plant <room> <out.json>"); return 2; }
  const fileArg = opt("--file"), trackPath = "Design/levels/n65/rooms/" + id + ".json";
  const cur = JSON.parse(fs.readFileSync(rel(fileArg || trackPath), "utf8"));
  let baseFile;
  try { baseFile = JSON.parse(cp.execFileSync("git", ["show", base + ":" + trackPath], { cwd: ROOT, encoding: "utf8", maxBuffer: 64 << 20 })); } catch (e) { console.log("n66-deco: REFUSED: " + trackPath + " is not in " + base); return 2; }
  const T = readTouches(opts("--touches"), id);
  const J = judge(cur, baseFile, T, id);
  console.log(`n66-deco: room ${id}, ${fileArg || trackPath} against ${trackPath} at ${base}: ${J.head}`);
  for (const l of J.lines) console.log(l);
  console.log(`n66-deco: ${J.fails ? "FAIL" : "PASS"} | room ${id}, base ${base}: ${J.tail}`);
  return J.fails ? 1 : 0;
}

/// NIGHT66 T1c fix (Logs/n66/verdict-T1c.md): the judging, shared by the command and --selftest. Its tests:
///   (a) touched on >= 3 / 10 seeds (skip v2's table)
///   (b) SIGHT: a ranged spot -> walk-line sample sight that is CLEAR of every other solid of the room and cut by this piece (the critic: a
///       piece behind a wall cut sights the wall had cut already)
///   (c) a launch36 kill burst within 1 m
///   (d) MARKER: a cyan marker, one of the room's first 2, within 6 m (on the floor plan) of the room's exit (row.exit), or of the piece it
///       names in its `marks` field ("exit", or a piece's id / why head / key) when that piece does a job by (a), (b) or (c) and is within 6 m
///       (the critic: "within 6 m of any solid" held for 88-100 % of every room's floor)
function judge(cur, baseFile, T, id) {
  const now = pieces(cur, id), was = pieces(baseFile, id);
  const row = cur.row || {};
  const spots = rangedSpots(row), line = walkLine(row);
  const exit = row.exit;
  const isNew = (p) => !was.some((q) => q.key === p.key && (p.kind !== "marker" && p.kind !== "dressing" ? true : JSON.stringify(q.src) === JSON.stringify(p.src)));
  const fresh = now.filter(isNew);
  const judged = fresh.filter((p) => p.kind !== "dressing"), dressing = fresh.filter((p) => p.kind === "dressing");
  const markersAll = now.filter((p) => p.kind === "marker");
  const solidsNow = now.filter((p) => solidKinds.includes(p.kind));
  const memo = new Map();
  function jobTests(p) { // (a), (b), (c) for any piece of the room (memoized: (d) asks them of a marker's target too)
    if (memo.has(p)) return memo.get(p);
    const res = [];
    if (["box", "ramp", "pad"].includes(p.kind)) {
      const t = T.touch.find((x) => x.key.startsWith(p.kind + "(") && sameNums(x.nums, p.nums));
      const need = t ? Math.ceil(0.3 * t.of) : 3;
      res.push(["a touched", t ? t.seeds >= need : false, t ? `${t.seeds}/${t.of} seeds (need ${need}), ${t.steps} steps` : T.touch.length ? "not in the touch table" : "no touch table"]);
    } else res.push(["a touched", false, "n/a (" + (p.what || p.kind) + ")"]);
    if (solidKinds.includes(p.kind)) {
      const others = solidsNow.filter((q) => q !== p);
      let cut = 0, clear = 0, pairs = 0;
      for (const s of spots) for (const w of line) {
        pairs++;
        if (others.some((q) => blocks(q, s, w))) continue; // already cut without this piece
        clear++;
        if (blocks(p, s, w)) cut++;
      }
      res.push(["b sight", cut >= 1, `cuts ${cut} of the ${clear} sights clear without it (${pairs} spot-to-walk-line sights)`]);
    } else res.push(["b sight", false, "n/a (no solid)"]);
    if (solidKinds.includes(p.kind) || p.kind === "pad") {
      const k = T.kills.filter((x) => boxDist(p, x.at) <= 1.0);
      res.push(["c launch", k.length >= 1, k.length ? `${k.length} launch36 kill burst(s) within 1 m (seeds ${[...new Set(k.map((x) => x.seed))].join(", ")})` : "no launch36 kill on it"]);
    } else res.push(["c launch", false, "n/a"]);
    memo.set(p, res);
    return res;
  }
  const flat = (a, b) => Math.sqrt((a[0] - b[0]) ** 2 + (a[2] - b[2]) ** 2);
  // NIGHT66 T1c fix 2 (Logs/n66/verdict-T1c-r2.md): `marks` names its target EXACTLY (a piece's key, e.g. "box(1,0,-166.7..2.2,2.2,-165.5)",
  // or a prop's id; no prefix, no why head), and the room's bounding walls (a box on an edge of the rect, along half that side or more) are
  // never a marker's target: they are next to every corner
  const bounding = (q) => boundingWall(q, row.rect);
  function findTarget(name) {
    if (!name || name === "exit") return null;
    return now.find((q) => q.kind !== "marker" && q.kind !== "dressing" && (q.key === name || (q.src && q.src.id !== undefined && q.src.id === name)));
  }
  const lines = [];
  let fails = 0;
  for (const p of judged) {
    const moved = was.some((q) => q.kind === p.kind && q.ident && q.ident === p.ident);
    const res = jobTests(p).slice();
    if (p.kind === "marker") {
      const rank = markersAll.indexOf(p) + 1;
      const dExit = exit ? flat(p.at, exit) : Infinity;
      const marks = p.src && p.src.marks;
      const tgt = findTarget(marks);
      const tgtJob = tgt ? jobTests(tgt).filter((r) => r[1]).map((r) => r[0].split(" ")[0]) : [];
      const dTgt = tgt ? boxDist(tgt, p.at) : Infinity;
      const okExit = dExit <= 6, okTgt = tgt && !bounding(tgt) && tgtJob.length > 0 && dTgt <= 6;
      const ok = rank <= 2 && (okExit || okTgt);
      const why = `cyan marker ${rank} of ${markersAll.length} in the room (at most 2); the exit ${dExit === Infinity ? "-" : dExit.toFixed(1)} m off (6 m)`
        + (marks && marks !== "exit" ? `; marks '${marks}': ${tgt ? (bounding(tgt) ? `${tgt.key} is a bounding wall of the room (never a target)` : `${tgt.key} ${dTgt.toFixed(1)} m off, its job ${tgtJob.length ? tgtJob.join("+") : "none (it passes no test a-c)"}`) : "no piece of the room has that exact key or id"}` : marks === "exit" ? "; marks the exit" : "; no marks (only the exit counts)");
      res.push(["d marker", ok, why]);
    } else res.push(["d marker", false, "n/a (not a cyan marker)"]);
    const pass = res.some((r) => r[1]);
    if (!pass) fails++;
    lines.push(`  ${pass ? "PASS" : "FAIL"} ${moved ? "moved" : "added"} ${p.key} | ${p.label} | ` + res.map((r) => `${r[0]} ${r[1] ? "YES" : "no"} (${r[2]})`).join("; "));
    p.verdict = { pass, res };
  }
  for (const p of dressing) lines.push(`  ----  ${was.some((q) => q.ident && q.ident === p.ident) ? "moved" : "added"} ${p.key} | ${p.label} | dressing: not judged (no collider, not a cyan marker)`);
  const removed = was.filter((q) => !now.some((p) => p.key === q.key));
  if (removed.length) lines.push(`  removed since the base: ${removed.map((q) => q.key).join(", ")}`);
  const warn = judged.length === 0 && dressing.length > 0 ? " | WARN: dressing added and no piece: a decoration-only change" : "";
  const head = `${now.length} items now (${was.length} at the base); new or moved: ${fresh.length} (${judged.length} pieces judged, ${dressing.length} dressing not judged)`
    + ` | touches: ${T.touch.length ? T.touch.length + " pieces from " + [...new Set(T.touch.map((t) => t.file))].join(", ") : "none given (test a not run)"}; launch kills ${T.kills.length}; ranged spots ${spots.length}${spots.length ? "" : " (no ranged enemy in the waves: test b cannot pass)"}; walk-line samples ${line.length}; cyan markers in the room ${markersAll.length}`;
  const tail = `${judged.length} piece(s) added or moved, ${judged.length - fails} pass, ${fails} FAIL; dressing ${dressing.length} not judged${warn}`;
  return { head, lines, tail, fails, judged, now, spots, line, solidsNow };
}

// ---------------------------------------------------------------- the planted cases (NIGHT66 T1c fix): one PASS and one FAIL per test
function selftest(id, touchFile) {
  const fp = rel("Design/levels/n65/rooms/" + id + ".json");
  const base = JSON.parse(fs.readFileSync(fp, "utf8"));
  // the base without the room's cyan markers (the planted markers are then the room's first two: the cap of 2 does not decide them)
  const strip = (f) => { const c = JSON.parse(JSON.stringify(f)); c.lights = (c.lights || []).filter((l) => String(l.color || "").toLowerCase() !== "cyan"); c.props = (c.props || []).filter((q) => String(q.color || q.role || "").toLowerCase() !== "cyan"); return c; };
  const B = strip(base), P = strip(base);
  const T = readTouches([touchFile], id);
  const row = B.row, [x0, z0, x1, z1] = row.rect, exit = row.exit;
  const ctx = judge(B, B, T, id); // the base's own pieces (no new piece): its solids, spots and walk line
  const solids = ctx.solidsNow;
  // B+ : a box on a clear sight from a ranged spot to the walk line (its middle), 1 x 1 m, from the floor to 0.5 m over the sight
  let bPlus = null;
  for (const s of ctx.spots) { for (const w of ctx.line) {
    if (solids.some((q) => blocks(q, s, w))) continue;
    const m = [(s[0] + w[0]) / 2, (s[1] + w[1]) / 2, (s[2] + w[2]) / 2];
    const box = { zone: id, kind: "hull", min: [m[0] - 0.5, 0, m[2] - 0.5], max: [m[0] + 0.5, m[1] + 0.5, m[2] + 0.5], job: "cover: PLANTED", why: "PLANTED B+ cover on a clear spitter sight (test b must read YES)" };
    const pc = pieces({ boxes: [box] }, id)[0];
    if (!solids.some((q) => q.min && pc.min[0] < q.max[0] && pc.max[0] > q.min[0] && pc.min[2] < q.max[2] && pc.max[2] > q.min[2])) { bPlus = box; break; }
  } if (bPlus) break; }
  // B- : a box inside an existing wall (every sight it cuts is cut by the wall already)
  const host = solids.filter((q) => q.kind === "box").sort((a, b) => (b.max[1] - b.min[1]) * (b.max[0] - b.min[0] + b.max[2] - b.min[2]) - (a.max[1] - a.min[1]) * (a.max[0] - a.min[0] + a.max[2] - a.min[2]))[0];
  const hc = [(host.min[0] + host.max[0]) / 2, (host.min[2] + host.max[2]) / 2];
  const hw = [Math.min(0.4, (host.max[0] - host.min[0]) / 4), Math.min(0.4, (host.max[2] - host.min[2]) / 4)];
  const bMinus = { zone: id, kind: "hull", min: [hc[0] - hw[0], host.min[1] + 0.1, hc[1] - hw[1]], max: [hc[0] + hw[0], host.max[1] - 0.1, hc[1] + hw[1]], job: "cover: PLANTED", why: "PLANTED B- a box inside " + host.label + " (test b must read no)" };
  // C+ : a box around a recorded launch36 kill burst (the touch file's first launchkill)
  const k0 = T.kills[0];
  const cPlus = k0 ? { zone: id, kind: "hull", min: [k0.at[0] - 0.5, 0, k0.at[2] - 0.5], max: [k0.at[0] + 0.5, Math.max(0.6, k0.at[1] - 0.3), k0.at[2] + 0.5], job: "anvil: PLANTED", why: "PLANTED C+ a block under a launch36 kill burst (test c must read YES)" } : null;
  // A- : a 1 m crate in a free corner (no touch entry, no sight, no kill)
  const aMinus = { zone: id, kind: "hull", min: [x0 + 0.7, 0, z1 - 1.7], max: [x0 + 1.7, 1, z1 - 0.7], job: "cover: PLANTED", why: "PLANTED A- a 1 m crate in a corner (every test must read no)" };
  // D- : a cyan light in the rect's far corner, 6 m up, no marks;  D+ : a cyan light 1 m from the exit
  const far = [[x0 + 0.5, z0 + 0.5], [x1 - 0.5, z0 + 0.5], [x0 + 0.5, z1 - 0.5], [x1 - 0.5, z1 - 0.5]].sort((a, b) => Math.hypot(b[0] - exit[0], b[1] - exit[2]) - Math.hypot(a[0] - exit[0], a[1] - exit[2]))[0];
  const dMinus = { id: id + "-plantD-", room: id, kind: "wall", at: [far[0], 6, far[1]], normal: [0, -1, 0], size: [1, 0.2, 0.1], color: "cyan", range: 6, intensity: 1, why: "PLANTED D- a cyan light in the far corner pointing at nothing (test d must read no)" };
  const dPlus = { id: id + "-plantD+", room: id, kind: "pool", at: [exit[0] + 1, 0.03, exit[2]], normal: [0, 1, 0], size: [1, 1, 0.05], color: "cyan", range: 6, intensity: 1, why: "PLANTED D+ a cyan pool 1 m from the exit (test d must read YES)" };
  // D+m / D-m (the cap of 2 makes these a second judge): a light whose `marks` names a piece with a job, beside it (PASS), and one naming the
  // planted corner crate A- (no job), beside it (FAIL)
  const ctxJ = judge(B, B, T, id);
  const touchedOk = (q) => T.touch.some((t) => t.key === q.key && t.seeds >= Math.ceil(0.3 * t.of));
  const working = ctxJ.solidsNow.find((q) => q.kind === "box" && !boundingWall(q, row.rect) && touchedOk(q));
  const wall = ctxJ.solidsNow.find((q) => boundingWall(q, row.rect) && touchedOk(q));
  const over = (q, nm, marks, why) => ({ id: id + "-plant" + nm, room: id, kind: "wall", at: [(q.min[0] + q.max[0]) / 2, q.max[1] + 0.5, (q.min[2] + q.max[2]) / 2], normal: [0, -1, 0], size: [1, 0.2, 0.1], color: "cyan", marks, why });
  const aKey = pieces({ boxes: [aMinus] }, id)[0].key;
  const dPlusM = working ? over(working, "Dm+", working.key, "PLANTED D+m a cyan light over " + working.label + ", marks its exact key (test d must read YES)") : null;
  const dMinusM = over(pieces({ boxes: [aMinus] }, id)[0], "Dm-", aKey, "PLANTED D-m a cyan light over the corner crate A-, marks its exact key (no job: test d must read no)");
  // round 2 (verdict-T1c-r2.md): a prefix is no name, and a bounding wall is never a target
  const dMinusP = working ? over(working, "Dp-", "P", "PLANTED D-p a cyan light over " + working.label + ", marks 'P' (a prefix: test d must read no)") : null;
  const dMinusW = wall ? over(wall, "Dw-", wall.key, "PLANTED D-w a cyan light by " + wall.label + " (a bounding wall), marks its exact key (test d must read no)") : null;
  P.boxes = (P.boxes || []).concat([bPlus, bMinus, cPlus, aMinus].filter((x) => x));
  P.lights = (P.lights || []).concat([dMinus, dPlus]);
  const P2 = JSON.parse(JSON.stringify(P));
  P2.lights = P2.lights.filter((l) => !String(l.why || "").startsWith("PLANTED D")).concat([dPlusM, dMinusM].filter((x) => x));
  const J2 = judge(P2, B, T, id);
  const P3 = JSON.parse(JSON.stringify(P));
  P3.lights = P3.lights.filter((l) => !String(l.why || "").startsWith("PLANTED D")).concat([dMinusP, dMinusW].filter((x) => x));
  const J3 = judge(P3, B, T, id);
  const J = judge(P, B, T, id);
  console.log(`n66-deco --selftest: room ${id} (its cyan markers stripped), touches ${touchFile}: ${J.head}`);
  for (const l of J.lines) console.log(l);
  for (const l of J2.lines.filter((l) => /PLANTED D|plantDm/.test(l))) console.log(l + "   (second judge: the marks cases)");
  for (const l of J3.lines.filter((l) => /plantD[pw]/.test(l))) console.log(l + "   (third judge: a prefix, a bounding wall)");
  const byWhy = (w, JJ) => (JJ || J).judged.find((p) => p.src && String(p.src.why || "").startsWith(w));
  const T_ = (p, name) => p && p.verdict.res.find((r) => r[0].startsWith(name))[1];
  // [name, planted (null: n/a in this room), the check]
  const cases = [
    ["B+ test b reads YES on a clear sight", bPlus, () => T_(byWhy("PLANTED B+"), "b") === true],
    ["B- test b reads no inside a wall", bMinus, () => T_(byWhy("PLANTED B-"), "b") === false],
    ["C+ test c reads YES under a launch kill", cPlus, () => T_(byWhy("PLANTED C+"), "c") === true],
    ["A- every test no, the piece FAILs", aMinus, () => byWhy("PLANTED A-").verdict.pass === false],
    ["D- a corner light with no marks FAILs", dMinus, () => byWhy("PLANTED D-").verdict.pass === false],
    ["D+ a light beside the exit PASSes", dPlus, () => byWhy("PLANTED D+").verdict.pass === true],
    ["D+m a light marking a working piece beside it PASSes", dPlusM, () => byWhy("PLANTED D+m", J2).verdict.pass === true],
    ["D-m a light marking a piece with no job FAILs", dMinusM, () => byWhy("PLANTED D-m", J2).verdict.pass === false],
    ["D-p a light marking 'P' (a prefix, not an exact key / id) FAILs", dMinusP, () => byWhy("PLANTED D-p", J3).verdict.pass === false],
    ["D-w a light marking a bounding wall of the room FAILs", dMinusW, () => byWhy("PLANTED D-w", J3).verdict.pass === false],
  ];
  let n = 0, ok = 0, na = 0;
  for (const [name, planted, check] of cases) {
    if (!planted) { na++; console.log(`  n/a  ${name} (nothing to plant in ${id}: ${/B\+/.test(name) ? "no clear ranged sight" : /C\+/.test(name) ? "no launch kill in the table" : "no working piece"})`); continue; }
    n++;
    let r = false;
    try { r = check(); } catch (e) { r = false; }
    if (r) ok++;
    console.log(`  ${r ? "PASS" : "FAIL"} ${name}`);
  }
  const good = ok === n;
  console.log(`n66-deco --selftest: ${good ? "PASS" : "FAIL"} (${ok} / ${n} planted cases${na ? ", " + na + " n/a in " + id : ""}; test a's planted pair is t1c-deco-plant.sh's, on a skip v2 run)`);
  return good ? 0 : 1;
}
process.exit(args[0] === "--selftest" ? selftest(args[1] || "b1", args[2] || "Research/2026-10-23-wave66/T/t1c-deco-plant-touch.txt") : main());
