// NIGHT65 builder T (air65; the measure for round 1's level-design finding, Logs/n65/r1/review-ld.md: "every flight line is a single piece
// along the room's edge that ends on a terminal deck in a corner ... No room hands one air piece to another"): a STATIC checker of a room's
// AIR HAND-OFFS. No sim: closed-form flights stepped at the sim's 120 Hz with the game's own movement numbers (cited below).
//   node Tools/night/n65-air.cjs <plan.json | level.json> <roomId> [--json]       one room's hand-off graph
//   node Tools/night/n65-air.cjs --selftest                                       the planted pair (two ride walls 4.8 m apart: a wall-jump
//                                                                                 edge both ways; 9.6 m apart: none)
// AIR PIECES (the plan's rows of the room; a level file is read through its plan, as n65-iso.cjs does):
//   ride    a box with ride: true and its ride36 face (E / W / N / S): a vertical band; ridden at RIDE_Y over its base (clamped under its top)
//   deck    a deck / slab top at 1 m or more
//   ledge   a hull top at 1 m or more with 1.8 m of headroom under the room's height (a post or a wall you can stand on)
//   pad     a jump pad (at -> target): its own flight lands on the piece that holds its target; a pad on a deck is walked onto from it
// MOVES from a piece (feet at the departure height; the target at its own height):
//   from a deck / ledge edge: jump, jump + double, dash (off the edge, then a fall), dash-jump, dash-jump + double
//   from a ride: jump-off (ride39's), jump-off + double, wall jump (wallfling42's; it refreshes the double jump), wall jump + double, dash
// An edge P -> Q holds when a move's flight, aimed straight at Q's nearest part, covers the horizontal gap (plus the capsule radius) while its
// feet are still above Q's top (a deck / ledge: a landing) or inside Q's ride band (a ride: an attach). Its margin is the spare horizontal
// distance (a deck) or the spare height (a ride). The flight is a straight line in plan: no obstacle test, no air control, no apex hang
// (both help a real player), no line of sight. The longest floor-free chain is the longest simple path; a DEAD END is an air piece (not a
// pad) that no move leaves for another air piece (only a drop).
// NUMBERS (every one read from the config, none typed; a design row's value only when its row is ON):
//   PortExport/config.json jump.height, jump.apexTime -> v0 = 2h/t, g = 2h/t^2 (MovementMath.cs:99 / 102); jump.fallGravityMult (falling);
//   jump.doubleJumpMult (Mover.cs:1129); dash.speed, dash.duration; dashJump.v0Mult, gravityMult, windowTime (Mover.cs:1080-1088: "Keep the
//   dash's horizontal velocity; lighter gravity for the wind window"); body.capsuleRadius; ground.walkSpeed (the run-up); wallRide.minSpeed;
//   Design/unity-config.json design.n42.wallfling42 {outSpeed, v0Mult} (ON: the wall jump, Mover.cs:1111-1113);
//   design.n35.wallride35 {outSpeed} and design.n39.ride39 {speedBuff, exitBoost, upLow, upHigh} (ON: the ride speed minSpeed x (1 + speedBuff),
//   Mover.cs:1594; the jump-off = the along speed kept + outSpeed out, boosted by exitBoost (Ride35.Boost), up at a level look = the mean of
//   upLow and upHigh).
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const abs = (p) => (path.isAbsolute(p) ? p : path.join(ROOT, p));
const f1 = (n) => (Math.round(n * 10) / 10).toFixed(1);
const f2 = (n) => (Math.round(n * 100) / 100).toFixed(2);

// ---------------------------------------------------------------- the movement numbers
function numbers() {
  const c = JSON.parse(fs.readFileSync(abs("PortExport/config.json"), "utf8"));
  const d = JSON.parse(fs.readFileSync(abs("Design/unity-config.json"), "utf8")).design;
  const on = (r) => r && r.enabled === true;
  const v0 = (2 * c.jump.height) / c.jump.apexTime, g = (2 * c.jump.height) / (c.jump.apexTime * c.jump.apexTime);
  const w42 = d.n42 && d.n42.wallfling42, r35 = d.n35 && d.n35.wallride35, r39 = d.n39 && d.n39.ride39;
  const speedBuff = on(r39) ? r39.speedBuff : on(r35) ? r35.speedBuff : 0;
  const exitBoost = on(r39) ? r39.exitBoost : on(r35) ? r35.exitBoost : 0;
  const upLow = on(r39) ? r39.upLow : on(r35) ? r35.upLow : v0 * c.wallJump.v0Mult, upHigh = on(r39) ? r39.upHigh : on(r35) ? r35.upHigh : v0 * c.wallJump.v0Mult;
  return {
    v0, g, fall: c.jump.fallGravityMult, dj: v0 * c.jump.doubleJumpMult, doubles: c.jump.doubleJumps,
    dash: c.dash.speed, dashT: c.dash.duration, djV0: v0 * c.dashJump.v0Mult, djG: c.dashJump.gravityMult, djT: c.dashJump.windowTime,
    walk: c.ground.walkSpeed, cap: c.body.capsuleRadius,
    wjOut: on(w42) ? w42.outSpeed : c.wallJump.outSpeed, wjUp: v0 * (on(w42) ? w42.v0Mult : c.wallJump.v0Mult),
    ride: c.wallRide.minSpeed * (1 + speedBuff), offOut: on(r35) ? r35.outSpeed : c.wallJump.outSpeed, exitBoost, offUp: (upLow + upHigh) / 2,
    rows: { wallfling42: on(w42), wallride35: on(r35), ride39: on(r39) },
  };
}
const RIDE_Y = 2.5; // m over a ride wall's base where the ride is held (a jump + double jump's reach 2.7 m: the attach height a player gets on foot)

// ---------------------------------------------------------------- one flight: the feet's height (over the departure) at horizontal distance s
// phases: { vh, vy, lightG (the dash-jump's gravity mult), lightT, double (a double jump at the apex), flat (a dash: s metres at height 0 first) }
function flight(N, m) {
  const dt = 1 / 120, pts = [];
  let s = 0, y = 0, vy = m.vy, t = 0, dbl = m.double ? 1 : 0, flatS = m.flat || 0;
  if (flatS > 0) { pts.push([0, 0]); s = flatS; pts.push([s, 0]); }
  for (let k = 0; k < 600; k++) {
    const light = m.lightT && t < m.lightT ? m.lightG : 1;
    const gk = vy > 0 ? N.g * light : N.g * N.fall * light;
    if (dbl && vy <= 0) { vy = N.dj; dbl = 0; }
    vy -= gk * dt; y += vy * dt; s += m.vh * dt; t += dt;
    pts.push([s, y]);
    if (y < -16) break;
  }
  return pts;
}
// the farthest s at which the feet are still at or above dy (a landing at height dy), or -Infinity
function reachAt(pts, dy) { let best = -Infinity; for (let i = 1; i < pts.length; i++) if (pts[i][1] >= dy) best = pts[i][0]; return best; }
function heightAt(pts, s) { for (let i = 1; i < pts.length; i++) if (pts[i][0] >= s) return pts[i][1]; return -Infinity; }

// ---------------------------------------------------------------- pieces
function loadPlan(p) {
  const j = JSON.parse(fs.readFileSync(abs(p), "utf8"));
  if (Array.isArray(j.rooms)) return { plan: j, src: p };
  const m = /^(n\d+)-([a-z]+)$/.exec(j.name || "");
  const pp = m ? "Design/levels/" + m[1] + "/" + m[2] + ".plan.json" : null;
  if (!pp || !fs.existsSync(abs(pp))) throw new Error(p + " is a level file and its plan " + pp + " is missing");
  return { plan: JSON.parse(fs.readFileSync(abs(pp), "utf8")), src: pp + " (the plan of " + p + ")" };
}
function pieces(plan, id) {
  const row = plan.rooms.find((r) => r.id === id);
  if (!row) throw new Error("no room " + id);
  const H = row.height || 10;
  const out = [];
  let k = 0;
  SOLIDS = (plan.boxes || []).filter((o) => o.zone === id);
  // a top a player can stand on: at least the capsule's diameter across, and under 80 % of it covered by boxes whose bottom is within the
  // body's 1.8 m over it (a gantry's posts under its deck: covered; a skim step with a ladle hung over one corner: standable)
  const standable = (b) => {
    const [x0, , z0] = b.min, [x1, y1, z1] = b.max;
    if (Math.min(x1 - x0, z1 - z0) < 2 * 0.35) return false;
    let covered = 0;
    for (const o of SOLIDS) {
      if (o === b || !(o.min[1] >= y1 - 0.01 && o.min[1] < y1 + 1.8)) continue;
      const w = Math.min(x1, o.max[0]) - Math.max(x0, o.min[0]), d = Math.min(z1, o.max[2]) - Math.max(z0, o.min[2]);
      if (w > 0 && d > 0) covered += w * d;
    }
    return covered < 0.8 * (x1 - x0) * (z1 - z0);
  };
  for (const b of SOLIDS) {
    const [x0, y0, z0] = b.min, [x1, y1, z1] = b.max;
    if (!(b.ride && b.ride36) && !standable(b)) continue;
    const tag = (b.job ? b.job + ": " : "") + String(b.why || b.note || "").slice(0, 40);
    if (b.ride && b.ride36) {
      const face = b.ride36.face || "E";
      const axis = face === "E" || face === "W" ? "x" : "z";
      const plane = face === "E" ? x1 : face === "W" ? x0 : face === "S" ? z1 : z0;
      const sign = face === "E" || face === "S" ? 1 : -1;
      const lo = axis === "x" ? z0 : x0, hi = axis === "x" ? z1 : x1;
      const yRide = Math.min(y0 + RIDE_Y, y1 - 0.5);
      out.push({ id: "R" + ++k, type: "ride", axis, plane, sign, lo, hi, base: y0, top: y1, y: yRide, len: hi - lo, tag, box: b });
    } else if ((b.kind === "deck" || b.kind === "slab") && y1 >= 1) out.push({ id: "D" + ++k, type: "deck", rect: [x0, z0, x1, z1], y: y1, tag, box: b });
    else if (b.kind === "hull" && y1 >= 1 && y1 <= H - 1.8) out.push({ id: "L" + ++k, type: "ledge", rect: [x0, z0, x1, z1], y: y1, tag, box: b });
  }
  // a pad on the floor (under 0.5 m) starts a flight from the floor: it may open a chain, never sit inside one (landing on it is the floor)
  for (const p of (plan.pads || []).filter((o) => o.zone === id)) out.push({ id: "P" + ++k, type: "pad", at: p.at, target: p.target, y: p.at[1], onFloor: p.at[1] < 0.5, rect: [p.at[0] - 0.5, p.at[2] - 0.5, p.at[0] + 0.5, p.at[2] + 0.5], tag: (p.job || "pad") });
  return { row, pieces: out };
}

// ---------------------------------------------------------------- geometry: nearest points between a piece's footprint and another's
const segOf = (r) => (r.axis === "x" ? [[r.plane, r.lo], [r.plane, r.hi]] : [[r.lo, r.plane], [r.hi, r.plane]]);
function rectPoint(rc, x, z) { return [Math.max(Math.min(rc[0], rc[2]), Math.min(Math.max(rc[0], rc[2]), x)), Math.max(Math.min(rc[1], rc[3]), Math.min(Math.max(rc[1], rc[3]), z))]; }
function segPoint(sg, x, z) { const [a, b] = sg, dx = b[0] - a[0], dz = b[1] - a[1], l2 = dx * dx + dz * dz; const t = l2 < 1e-12 ? 0 : Math.max(0, Math.min(1, ((x - a[0]) * dx + (z - a[1]) * dz) / l2)); return [a[0] + dx * t, a[1] + dz * t]; }
function shapeOf(p) { return p.type === "ride" ? { seg: segOf(p) } : { rect: p.rect }; }
function nearestOn(sh, x, z) { return sh.seg ? segPoint(sh.seg, x, z) : rectPoint(sh.rect, x, z); }
// the closest pair of points (from A, to B): sampled along A's boundary and refined against B
function closest(A, B) {
  const a = shapeOf(A), b = shapeOf(B);
  const samples = [];
  if (a.seg) { for (let i = 0; i <= 40; i++) { const t = i / 40; samples.push([a.seg[0][0] + (a.seg[1][0] - a.seg[0][0]) * t, a.seg[0][1] + (a.seg[1][1] - a.seg[0][1]) * t]); } }
  else { const [x0, z0, x1, z1] = a.rect; for (let i = 0; i <= 20; i++) { const t = i / 20; samples.push([x0 + (x1 - x0) * t, z0], [x0 + (x1 - x0) * t, z1], [x0, z0 + (z1 - z0) * t], [x1, z0 + (z1 - z0) * t]); } }
  let best = null;
  for (const [x, z] of samples) { const q = nearestOn(b, x, z); const d = Math.hypot(q[0] - x, q[1] - z); if (!best || d < best.d) best = { from: [x, z], to: q, d }; }
  return best;
}

// ---------------------------------------------------------------- the moves
function movesFrom(N, P) {
  if (P.type === "ride") {
    // the jump-off's horizontal velocity toward a direction (ux, uz): the along speed kept (either way), outSpeed out, boosted by exitBoost
    const jumpOff = (ux, uz) => {
      const ax = P.axis === "x" ? 0 : 1, az = P.axis === "x" ? 1 : 0, nx = P.axis === "x" ? P.sign : 0, nz = P.axis === "x" ? 0 : P.sign;
      let best = 0;
      for (const s of [1, -1]) {
        const vx = ax * s * N.ride + nx * N.offOut, vz = az * s * N.ride + nz * N.offOut, h = Math.hypot(vx, vz), k = (h + N.exitBoost) / h;
        best = Math.max(best, (vx * ux + vz * uz) * k);
      }
      return best;
    };
    const wallJump = (ux, uz) => {
      const ax = P.axis === "x" ? 0 : 1, az = P.axis === "x" ? 1 : 0, nx = P.axis === "x" ? P.sign : 0, nz = P.axis === "x" ? 0 : P.sign;
      let best = 0;
      for (const s of [1, -1]) { const vx = ax * s * N.ride + nx * N.wjOut, vz = az * s * N.ride + nz * N.wjOut; best = Math.max(best, vx * ux + vz * uz); }
      return best;
    };
    return [
      { name: "jump-off", vh: jumpOff, vy: N.offUp },
      { name: "jump-off + double", vh: jumpOff, vy: N.offUp, double: true },
      { name: "wall jump", vh: wallJump, vy: N.wjUp },
      { name: "wall jump + double", vh: wallJump, vy: N.wjUp, double: true },
      { name: "dash", vh: () => N.walk, vy: 0, flat: N.dash * N.dashT, double: true },
    ];
  }
  return [
    { name: "jump", vh: () => N.walk, vy: N.v0 },
    { name: "jump + double", vh: () => N.walk, vy: N.v0, double: true },
    { name: "dash", vh: () => N.walk, vy: 0, flat: N.dash * N.dashT, double: true },
    { name: "dash-jump", vh: () => N.dash, vy: N.djV0, lightG: N.djG, lightT: N.djT },
    { name: "dash-jump + double", vh: () => N.dash, vy: N.djV0, lightG: N.djG, lightT: N.djT, double: true },
  ];
}
// a ride is only ridden from its face's side: the departure (or the arrival) must be out on the normal side
const outSide = (R, x, z) => (R.axis === "x" ? (x - R.plane) * R.sign : (z - R.plane) * R.sign);

// the room's solids (every box of the room: decks, slabs, hulls, walls) for the obstacle test: a flight is blocked when the body's low point
// (feet + 0.3 m) or its eye (feet + 1.5 m) passes inside a box that is neither the departure nor the target, sampled every 0.25 m
let SOLIDS = [];
function blocked(N, P, Q, from, dirX, dirZ, pts, until) {
  const skip = (b) => (P.box === b) || (Q.box === b);
  let next = 0;
  for (const [s, y] of pts) {
    if (s > until) break;
    if (s < next) continue;
    next = s + 0.25;
    const x = from[0] + dirX * s, z = from[1] + dirZ * s, fy = P.y + y;
    for (const b of SOLIDS) {
      if (skip(b)) continue;
      for (const h of [0.3, 1.5]) {
        const py = fy + h;
        if (x > b.min[0] + 0.02 && x < b.max[0] - 0.02 && z > b.min[2] + 0.02 && z < b.max[2] - 0.02 && py > b.min[1] + 0.02 && py < b.max[1] - 0.02) return b;
      }
    }
  }
  return null;
}

function edge(N, P, Q) {
  if (P === Q || P.type === "pad" || (Q.type === "pad" && Q.onFloor)) return null;
  const c = closest(P, Q);
  let dirX = c.to[0] - c.from[0], dirZ = c.to[1] - c.from[1];
  const dl = Math.hypot(dirX, dirZ);
  if (dl > 1e-6) { dirX /= dl; dirZ /= dl; }
  else if (P.type === "ride") { dirX = P.axis === "x" ? P.sign : 0; dirZ = P.axis === "x" ? 0 : P.sign; }
  if (P.type === "ride" && dl > 1e-6 && (dirX * (P.axis === "x" ? P.sign : 0) + dirZ * (P.axis === "x" ? 0 : P.sign)) < 0.1) return null; // into its own wall
  if (Q.type === "ride" && outSide(Q, c.from[0], c.from[1]) < 0.2) return null; // Q's face does not look at P
  const gap = Q.type === "ride" ? Math.max(0, c.d - N.cap) : c.d + N.cap; // the capsule's radius: short of a wall's face, past a deck's edge
  const dy = Q.y - P.y; // the target's height over the departure (a deck's top, a ride's held height)
  const ok = [];
  let stopped = null;
  for (const m of movesFrom(N, P)) {
    const vh = m.vh(dirX, dirZ);
    if (!(vh > 0.5)) continue;
    const pts = flight(N, Object.assign({}, m, { vh }));
    const hit = blocked(N, P, Q, c.from, dirX, dirZ, pts, Math.max(0, gap - N.cap));
    if (hit) { stopped = stopped || hit; continue; }
    if (Q.type === "ride") {
      const y = heightAt(pts, gap) + P.y; // the feet's height at the wall
      const lo = Q.base + 0.5, hi = Q.top - 0.3;
      if (y >= lo && y <= hi) ok.push({ move: m.name, margin: Math.min(y - lo, hi - y), unit: "m of height" });
    } else {
      const r = reachAt(pts, dy);
      if (r >= gap) ok.push({ move: m.name, margin: r - gap, unit: "m" });
    }
  }
  if (!ok.length) return null;
  return { move: ok[0].move, margin: ok[0].margin, unit: ok[0].unit, gap, also: ok.slice(1).map((o) => o.move + " " + f2(o.margin)) };
}

function analyse(plan, id) {
  const N = numbers();
  const { row, pieces: ps } = pieces(plan, id);
  const edges = [];
  for (const P of ps) for (const Q of ps) { const e = edge(N, P, Q); if (e) edges.push(Object.assign({ from: P.id, to: Q.id }, e)); }
  // a pad: its flight lands on the piece whose footprint holds its target (a deck / ledge within 0.6 m in height, or a ride band)
  for (const P of ps.filter((p) => p.type === "pad")) {
    const [tx, ty, tz] = P.target;
    const Q = ps.find((q) => q !== P && q.type !== "pad" && q.rect && tx >= Math.min(q.rect[0], q.rect[2]) - 0.3 && tx <= Math.max(q.rect[0], q.rect[2]) + 0.3 && tz >= Math.min(q.rect[1], q.rect[3]) - 0.3 && tz <= Math.max(q.rect[1], q.rect[3]) + 0.3 && Math.abs(q.y - ty) < 0.6);
    if (Q) edges.push({ from: P.id, to: Q.id, move: "pad", margin: 0, unit: "", gap: Math.hypot(tx - P.at[0], tz - P.at[2]) });
    // a pad on a deck: walked onto from that deck
    const D = ps.find((q) => q.rect && q.type !== "pad" && Math.abs(q.y - P.at[1]) < 0.3 && P.at[0] >= Math.min(q.rect[0], q.rect[2]) && P.at[0] <= Math.max(q.rect[0], q.rect[2]) && P.at[2] >= Math.min(q.rect[1], q.rect[3]) && P.at[2] <= Math.max(q.rect[1], q.rect[3]));
    if (D) edges.push({ from: D.id, to: P.id, move: "walk onto the pad", margin: 0, unit: "", gap: 0 });
  }
  // the longest floor-free chain: the longest simple path (the graphs are small: a DFS)
  const graph = (es) => {
    const out = new Map(ps.map((p) => [p.id, es.filter((e) => e.from === p.id).map((e) => e.to)]));
    let longest = [], flights = [];
    // the review's own question (Logs/n65/r1/review-ld.md: "No room hands one air piece to another"): the most FLIGHT pieces (rides and pads)
    // on one floor-free path; 1 means each flight ends where the next one cannot be reached without the floor
    const isFlight = (id) => { const p = ps.find((q) => q.id === id); return p.type === "ride" || p.type === "pad"; };
    const nf = (path) => path.filter(isFlight).length;
    const dfs = (path) => {
      if (path.length > longest.length) longest = path.slice();
      if (nf(path) > nf(flights) || (nf(path) === nf(flights) && path.length > flights.length)) flights = path.slice();
      for (const n of out.get(path[path.length - 1]) || []) if (!path.includes(n)) { path.push(n); dfs(path); path.pop(); }
    };
    for (const p of ps) dfs([p.id]);
    const dead = ps.filter((p) => p.type !== "pad" && (out.get(p.id) || []).length === 0); // no move (nor a pad on it) leaves it for another air piece
    return { longest, dead, flights, nFlights: nf(flights) };
  };
  const all = graph(edges);
  // without the dash-jump (its 24 m/s flight reaches most of a 32 m room): the hand-offs a jump, a double jump, a wall jump, a jump-off, a dash
  // or a pad makes; an edge whose easiest move is a dash-jump is left out
  const basic = graph(edges.filter((e) => !e.move.startsWith("dash-jump")));
  return { N, row, pieces: ps, edges, longest: all.longest, dead: all.dead, flights: all.flights, nFlights: all.nFlights, basic };
}

function print(plan, id, src, json) {
  const r = analyse(plan, id);
  if (json) { console.log(JSON.stringify({ room: id, plan: src, numbers: r.N, pieces: r.pieces, edges: r.edges, longest: r.longest, deadEnds: r.dead.map((p) => p.id) }, null, 1)); return r; }
  const N = r.N;
  console.log(`n65-air ${plan.id || "?"} ${id} "${r.row.name || ""}" (${src}):`);
  console.log(`  numbers: jump v0 ${f2(N.v0)} m/s, g ${f2(N.g)} m/s2 (fall x${N.fall}), double v0 ${f2(N.dj)}, dash ${N.dash} m/s x ${N.dashT} s = ${f1(N.dash * N.dashT)} m, dash-jump v0 ${f2(N.djV0)} with g x${N.djG} for ${N.djT} s at ${N.dash} m/s, run-up ${N.walk} m/s, wall jump out ${N.wjOut} up ${f2(N.wjUp)} (wallfling42 ${N.rows.wallfling42 ? "on" : "off"}), ride ${f2(N.ride)} m/s, jump-off out ${N.offOut} +${N.exitBoost} up ${f2(N.offUp)} (ride39 ${N.rows.ride39 ? "on" : "off"}), capsule ${N.cap} m; a ride held ${RIDE_Y} m over its base`);
  console.log(`  air pieces ${r.pieces.length}:`);
  for (const p of r.pieces) {
    const where = p.type === "ride" ? `face ${p.axis} = ${f1(p.plane)} (${p.sign > 0 ? "+" : "-"}${p.axis}), ${f1(p.lo)}..${f1(p.hi)} (${f1(p.len)} m), band ${f1(p.base)}..${f1(p.top)}, held at ${f1(p.y)}`
      : p.type === "pad" ? `at (${p.at.map(f1).join(", ")}) -> (${p.target.map(f1).join(", ")})${p.onFloor ? " (on the floor)" : ""}` : `top ${f1(p.y)} m over [${p.rect.map(f1).join(", ")}]`;
    console.log(`    ${p.id.padEnd(4)} ${p.type.padEnd(5)} ${where} | ${p.tag}`);
  }
  console.log(`  hand-offs ${r.edges.filter((e) => !e.move.startsWith("walk")).length} (piece -> piece without touching the floor, the easiest move first):`);
  for (const e of r.edges) console.log(`    ${e.from} -> ${e.to}  ${e.move}${e.unit ? `, margin ${f2(e.margin)} ${e.unit}` : ""} (gap ${f1(e.gap)} m)${e.also && e.also.length ? "; also " + e.also.join(", ") : ""}`);
  const airTypes = r.pieces.filter((p) => p.type !== "pad").length;
  console.log(`  longest floor-free chain: ${r.longest.length} piece(s): ${r.longest.join(" -> ") || "-"}${r.longest.length && r.pieces.find((p) => p.id === r.longest[0]).onFloor ? " (opened by a floor pad)" : ""}`);
  console.log(`  note: a dash-jump keeps the dash's 24 m/s for its whole flight (Mover.cs:1080; air accel never slows a body: MovementMath.Accelerate), so its reach is long; no obstacle, line-of-sight or air-control test`);
  const nb = r.edges.filter((e) => !e.move.startsWith("walk") && !e.move.startsWith("dash-jump")).length;
  const nFl = r.pieces.filter((p) => p.type === "ride" || p.type === "pad").length;
  console.log(`  flight pieces (rides + pads) on one floor-free path: ${r.nFlights} of ${nFl}: ${r.flights.join(" -> ") || "-"}; without the dash-jump: ${r.basic.nFlights}: ${r.basic.flights.join(" -> ") || "-"}`);
  console.log(`  without the dash-jump: hand-offs ${nb}, longest chain ${r.basic.longest.length}: ${r.basic.longest.join(" -> ") || "-"}, dead ends ${r.basic.dead.length}/${airTypes}${r.basic.dead.length ? " (" + r.basic.dead.map((p) => p.id).join(", ") + ")" : ""}`);
  console.log(`n65-air ${id}: air pieces ${r.pieces.length} (rides ${r.pieces.filter((p) => p.type === "ride").length}, decks ${r.pieces.filter((p) => p.type === "deck").length}, ledges ${r.pieces.filter((p) => p.type === "ledge").length}, pads ${r.pieces.filter((p) => p.type === "pad").length}), hand-offs ${r.edges.filter((e) => !e.move.startsWith("walk")).length}, longest floor-free chain ${r.longest.length}, dead ends ${r.dead.length}/${airTypes}${r.dead.length ? " (" + r.dead.map((p) => p.id).join(", ") + ")" : ""} | without the dash-jump: hand-offs ${nb}, chain ${r.basic.longest.length}, dead ends ${r.basic.dead.length}/${airTypes} | flights handed on: ${r.nFlights} of ${nFl} on one path (${r.basic.nFlights} without the dash-jump)`);
  return r;
}

function selftest() {
  const mk = (gap) => ({
    id: "air65-selftest", rooms: [{ id: "t", name: "PLANTED PAIR " + gap + " m", rect: [-8, -20, 8, 0], height: 10 }],
    boxes: [
      { zone: "t", kind: "wall", min: [-gap / 2 - 0.6, 0, -18], max: [-gap / 2, 8, -2], ride: true, ride36: { face: "E" }, job: "ride", why: "west wall" },
      { zone: "t", kind: "wall", min: [gap / 2, 0, -18], max: [gap / 2 + 0.6, 8, -2], ride: true, ride36: { face: "W" }, job: "ride", why: "east wall" },
    ], pads: [],
  });
  let bad = 0;
  for (const [gap, want] of [[4.8, true], [9.6, false]]) {
    const r = print(mk(gap), "t", "planted", false);
    const wj = (a, b) => r.edges.some((e) => e.from === a && e.to === b && (e.move.startsWith("wall jump") || (e.also || []).some((m) => m.startsWith("wall jump"))));
    const both = wj("R1", "R2") && wj("R2", "R1");
    const ok = want ? both : !wj("R1", "R2") && !wj("R2", "R1");
    console.log((ok ? "PASS " : "FAIL ") + `planted: two ride walls ${gap} m apart: a wall-jump edge both ways ${both ? "yes" : "no"} (want ${want ? "yes" : "no"})`);
    if (!ok) bad++;
  }
  console.log("n65-air selftest: " + (bad ? bad + " FAIL" : "PLANTED GREEN"));
  process.exit(bad ? 1 : 0);
}

const args = process.argv.slice(2);
if (args.includes("--selftest")) selftest();
else {
  const pos = args.filter((a) => !a.startsWith("--"));
  if (pos.length < 2) { console.error("usage: node Tools/night/n65-air.cjs <plan.json | level.json> <roomId> [--json] | --selftest"); process.exit(2); }
  const { plan, src } = loadPlan(pos[0]);
  print(plan, pos[1], src, args.includes("--json"));
}
