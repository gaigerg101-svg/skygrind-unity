// NIGHT65 builder T (iso65; branch C: no Player tonight, so the reviewers, the blind spotter and the critic judge heights, planes, the ride
// wall and the dressing from this picture): one PNG per room of a level PLAN with three panels: an isometric view from the entry, one from the
// far corner, and an elevation along the room's long axis with its standing planes labelled. Drawn: every box / ramp / pit / tunnel / prop of
// the room (zone- / room-keyed, as the plan holds them: kind, job, ride flag), its lights, the pads with their arcs, the ride faces, the
// grapple anchors, the enemy spawns (nodes, highNodes, waveNodes43, standIns), the player spawn, the exit, the doors, and the room row's
// line65 (the card's verb line) and frames65 (the card's poses).
// THE CAMERAS ARE FIXED PER SLOT: derived only from the room's rect (locked by gen-n65-foundry.cjs: the doors fix it) and its doors, never from
// the room's contents, so n52's room and its rebuild are drawn from exactly the same views (the GATE's blind SPOT compares those two pictures).
// Same input, same bytes: no clock, no randomness, numbers fixed to 0.1 px. SVG rasterized by headless Edge (Tools/night/n52-svg-png.sh's
// converter, the same flags).
//   node Tools/night/n65-iso.cjs <plan.json | level.json> <roomId> <out.png> [--svg <out.svg>] [--plant-box x0,y0,z0,x1,y1,z1] [--no-card-marks]
//     --no-card-marks leaves out line65 and frames65 (the GATE's blind pair: n52's rooms carry neither)
//     a level file (Design/levels/<px>-<level>.json) is drawn from its plan (Design/levels/<px>/<level>.plan.json: the pieces' kind / job /
//     ride flags live there); --svg keeps the SVG; --plant-box adds one hull box to the room in memory (the proof's planted case).
// Colours by role (CLAUDE.md rule 8): cyan what the player uses (pads and arcs, ride walls and faces, grapple anchors, pickups, the verb line),
// red-orange danger (lava, pits, enemy spawns, danger lights), pale violet-white the boundary (the room's walls, doors, exit, roof line),
// neutral stone the structure (decks, slabs, hulls, plain walls), muted stone with a dashed edge the dressing (decor props).
const fs = require("fs");
const path = require("path");
const os = require("os");
const cp = require("child_process");
const crypto = require("crypto");
const ROOT = path.join(__dirname, "../..");
const abs = (p) => (path.isAbsolute(p) ? p : path.join(ROOT, p));

// ---------------------------------------------------------------- arguments
const argv = process.argv.slice(2);
const flags = {};
const pos = [];
for (let i = 0; i < argv.length; i++) {
  if (argv[i] === "--svg" || argv[i] === "--plant-box" || argv[i] === "--plant-strip") flags[argv[i]] = argv[++i];
  else if (argv[i].startsWith("--")) flags[argv[i]] = true;
  else pos.push(argv[i]);
}
if (pos.length < 3) { console.error("usage: node Tools/night/n65-iso.cjs <plan.json | level.json> <roomId> <out.png> [--svg <out.svg>] [--plant-box x0,y0,z0,x1,y1,z1]"); process.exit(2); }
const [inFile, roomId, outPng] = pos;

// ---------------------------------------------------------------- the plan (a level file is drawn from its plan)
function loadPlan(p) {
  const j = JSON.parse(fs.readFileSync(abs(p), "utf8"));
  if (Array.isArray(j.rooms)) return { plan: j, src: p.replace(/\\/g, "/") };
  if (j.campaign) {
    const m = /^(n\d+)-([a-z]+)$/.exec(j.name || "");
    // NIGHT66 T1c fix (Logs/n66/verdict-T1c.md): a level file with its plan beside it (a round's Logs/n66/<tag>/, gen-n65-foundry.cjs --out)
    // is drawn from that plan, never from the tracked one its name names
    const beside = m ? path.join(path.dirname(abs(p)), m[2] + ".plan.json") : null;
    if (beside && fs.existsSync(beside)) return { plan: JSON.parse(fs.readFileSync(beside, "utf8")), src: beside.replace(/\\/g, "/") + " (the plan beside " + p.replace(/\\/g, "/") + ")" };
    const pp = m ? "Design/levels/" + m[1] + "/" + m[2] + ".plan.json" : null;
    if (!pp || !fs.existsSync(abs(pp))) throw new Error(p + " is a level file (" + j.name + ") and its plan " + pp + " is missing");
    return { plan: JSON.parse(fs.readFileSync(abs(pp), "utf8")), src: pp + " (the plan of " + p.replace(/\\/g, "/") + ")" };
  }
  throw new Error(p + ": neither a plan (rooms) nor a level file (campaign)");
}
const { plan, src } = loadPlan(inFile);
const row = (plan.rooms || []).find((r) => r.id === roomId);
if (!row) { console.error("n65-iso: no room " + roomId + " in " + src); process.exit(1); }
const mineZ = (o) => o.zone === roomId, mineR = (o) => o.room === roomId;
const boxes = (plan.boxes || []).filter(mineZ).map((b) => Object.assign({}, b));
if (flags["--plant-box"]) {
  const v = String(flags["--plant-box"]).split(",").map(Number);
  if (v.length !== 6 || v.some((n) => !isFinite(n))) throw new Error("--plant-box x0,y0,z0,x1,y1,z1");
  boxes.push({ zone: roomId, kind: "hull", min: v.slice(0, 3), max: v.slice(3), job: "PLANTED", why: "n65-iso's planted box", planted: true });
}
const ramps = (plan.ramps || []).filter(mineZ), pads = (plan.pads || []).filter(mineZ), grapple = (plan.grapple || []).filter(mineZ);
const pits = (plan.pits || []).filter(mineZ), tunnels = (plan.tunnels || []).filter(mineZ);
const lights = (plan.lights || []).filter(mineR), props = (plan.props || []).filter(mineR);
if (flags["--plant-strip"]) { // the proof's planted case: a cyan floor strip over [x0, z0, x1, z1]
  const v = String(flags["--plant-strip"]).split(",").map(Number);
  if (v.length !== 4 || v.some((n) => !isFinite(n))) throw new Error("--plant-strip x0,z0,x1,z1");
  props.push({ id: "planted-strip", room: roomId, kind: "floorStrip", at: [(v[0] + v[2]) / 2, 0.02, (v[1] + v[3]) / 2], size: [Math.abs(v[2] - v[0]), 0.02, Math.abs(v[3] - v[1])], yawDeg: 0, collider: false, job: "PLANTED", why: "n65-iso's planted strip" });
}
const doors = (plan.doors || []).filter((d) => (d.between || []).includes(roomId));
const entry = doors.find((d) => d.between[1] === roomId) || null; // the room's entry door (n46flow's F1EntryDoors: between[1])
const exitDoor = doors.find((d) => d.between[0] === roomId) || null;

// ---------------------------------------------------------------- the slot's fixed frame (rect + doors only)
const [X0, Z0, X1, Z1] = row.rect;
const CX = (X0 + X1) / 2, CZ = (Z0 + Z1) / 2;
const FRAME_LO = -4.5, FRAME_HI = 14; // every slot's vertical frame (a8's 4 m pit to the plan's wallHeight 14), never the room's own heights
const dcen = (d) => [(d.rect[0] + d.rect[2]) / 2, (d.rect[1] + d.rect[3]) / 2];
const E = entry ? dcen(entry) : [CX, Z1];
const norm = (x, z) => { const l = Math.hypot(x, z); return l < 1e-9 ? [0, -1] : [x / l, z / l]; };
const rot = (v, deg) => { const a = (deg * Math.PI) / 180; return [v[0] * Math.cos(a) - v[1] * Math.sin(a), v[0] * Math.sin(a) + v[1] * Math.cos(a)]; };
const F0 = norm(CX - E[0], CZ - E[1]);
const FA = rot(F0, 30); // from the entry, 30 deg off the room's axis (one side wall in view)
const camA = [CX - FA[0] * 100, CZ - FA[1] * 100];
const corners = [[X0, Z0], [X1, Z0], [X0, Z1], [X1, Z1]];
const far = Math.max(...corners.map((k) => Math.hypot(k[0] - E[0], k[1] - E[1])));
const farC = corners.filter((k) => Math.hypot(k[0] - E[0], k[1] - E[1]) > far - 1e-6).sort((a, b) => Math.hypot(b[0] - camA[0], b[1] - camA[1]) - Math.hypot(a[0] - camA[0], a[1] - camA[1]))[0];
const FB = norm(CX - farC[0], CZ - farC[1]); // from the far corner (the one farther from view A's camera), at the room's centre
const ELEV = Math.atan(1 / Math.SQRT2); // 35.26 deg: the true isometric elevation
const view = (f, name) => ({ fx: f[0], fz: f[1], rx: -f[1], rz: f[0], ce: Math.cos(ELEV), se: Math.sin(ELEV), name });
const VA = view(FA, "A"), VB = view(FB, "B");
const compass = (f) => { const a = (Math.atan2(f[0], -f[1]) * 180) / Math.PI; const n = ((Math.round(a) % 360) + 360) % 360; return n + " deg (0 = north)"; };

// ---------------------------------------------------------------- the canvas
const W = 1600, H = 1100;
const PA = { x: 10, y: 64, w: 785, h: 560 }, PB = { x: 805, y: 64, w: 785, h: 560 }, PE = { x: 10, y: 634, w: 1580, h: 380 };
const C = { bg: "#15131d", panel: "#1c1a26", floor: "#2c2a33", cyan: "#36e3ff", cyanDim: "#1f8ea3", cyanLine: "#8af4ff", red: "#ff5a1f", lava: "#ff4d12", void: "#07070a",
  bound: "#e6ddff", deck: "#a39c8c", slab: "#958e7f", hull: "#6f6a62", wall: "#857f75", decor: "#bdb5a5", text: "#e9e4f5", dim: "#9c95ad", planted: "#ffffff" };
const LCOL = { cyan: "#36e3ff", green: "#5cff8a", amber: "#ffb43a", white: "#f2efff", orange: "#ff5a1f" };
const f1 = (n) => (Math.round(n * 10) / 10).toFixed(1);
const esc = (s) => String(s).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
const out = [];
const shade = (hex, k) => { const n = parseInt(hex.slice(1), 16); const c = [n >> 16, (n >> 8) & 255, n & 255].map((v) => Math.max(0, Math.min(255, Math.round(v * k)))); return "#" + c.map((v) => v.toString(16).padStart(2, "0")).join(""); };
const text = (x, y, s, size, fill, extra) => out.push(`<text x="${f1(x)}" y="${f1(y)}" font-size="${size}" fill="${fill}" font-family="Consolas,monospace" ${extra || ""}>${esc(s)}</text>`);

// ---------------------------------------------------------------- the pieces as 3D solids (boxes, ramps, props, tunnels)
function boxRole(b) {
  if (b.planted) return { fill: C.planted, stroke: "#000000", role: "planted" };
  if (b.ride) return { fill: C.cyanDim, stroke: C.cyan, role: "ride" };
  return { fill: C[b.kind] || C.wall, stroke: "#2a2730", role: b.kind || "box" };
}
function propRole(p) {
  const k = String(p.kind || "");
  if (p.pickup || /pickup/i.test(k)) return { fill: C.cyan, stroke: "#0b3c46", role: "pickup", dash: false };
  // NIGHT65 (Logs/n65/verdict-a5-r2.md): a floor strip is the game's cyan lane line (PropsView29's floorStrip), and a prop that says cyan is
  // the player-usable colour: drawn in the cyan role, never as beige dressing
  if (/floorStrip|laneLine|padMarker/i.test(k) || p.color === "cyan") return { fill: C.cyan, stroke: "#e9fdff", role: "player line", dash: false, alpha: 0.95 };
  if (/lava|scorch|hazard|danger/i.test(k)) return { fill: C.red, stroke: "#5a1a05", role: "danger dressing", dash: true };
  if (p.collider) return { fill: C.slab, stroke: "#2a2730", role: "prop (collider)", dash: false };
  return { fill: C.decor, stroke: "#5f5970", role: "dressing", dash: true };
}
// a solid: its AABB (the sort) and its faces (3D polygons with an outward normal)
function boxSolid(min, max, style, extra) {
  const [a, b, c] = min, [d, e, f] = max;
  const P = (x, y, z) => [x, y, z];
  const faces = [
    { n: [0, 1, 0], p: [P(a, e, c), P(d, e, c), P(d, e, f), P(a, e, f)], k: 1 },
    { n: [1, 0, 0], p: [P(d, b, c), P(d, b, f), P(d, e, f), P(d, e, c)], k: 0.74, side: "E" },
    { n: [-1, 0, 0], p: [P(a, b, c), P(a, e, c), P(a, e, f), P(a, b, f)], k: 0.74, side: "W" },
    { n: [0, 0, 1], p: [P(a, b, f), P(a, e, f), P(d, e, f), P(d, b, f)], k: 0.58, side: "S" },
    { n: [0, 0, -1], p: [P(a, b, c), P(d, b, c), P(d, e, c), P(a, e, c)], k: 0.58, side: "N" },
  ];
  return Object.assign({ lo: [a, b, c], hi: [d, e, f], faces, style }, extra || {});
}
function rampSolid(r) {
  const [fx, fz] = r.from, [tx, tz] = r.to;
  const d = norm(tx - fx, tz - fz), s = [-d[1] * r.width / 2, d[0] * r.width / 2];
  const L0 = [fx + s[0], 0, fz + s[1]], R0 = [fx - s[0], 0, fz - s[1]], LH0 = [tx + s[0], 0, tz + s[1]], RH0 = [tx - s[0], 0, tz - s[1]];
  const LH = [tx + s[0], r.rise, tz + s[1]], RH = [tx - s[0], r.rise, tz - s[1]];
  const all = [L0, R0, LH0, RH0, LH, RH];
  const lo = [0, 1, 2].map((i) => Math.min(...all.map((p) => p[i]))), hi = [0, 1, 2].map((i) => Math.max(...all.map((p) => p[i])));
  const cross = (u, v) => [u[1] * v[2] - u[2] * v[1], u[2] * v[0] - u[0] * v[2], u[0] * v[1] - u[1] * v[0]];
  const sub = (u, v) => [u[0] - v[0], u[1] - v[1], u[2] - v[2]];
  const cen = [0, 1, 2].map((i) => all.reduce((t, p) => t + p[i], 0) / 6);
  const face = (p, k) => { let n = cross(sub(p[1], p[0]), sub(p[2], p[0])); const m = [0, 1, 2].map((i) => p.reduce((t, q) => t + q[i], 0) / p.length); if (n[0] * (m[0] - cen[0]) + n[1] * (m[1] - cen[1]) + n[2] * (m[2] - cen[2]) < 0) n = n.map((v) => -v); return { n, p, k }; };
  const faces = [face([L0, R0, RH, LH], 1), face([LH0, RH0, RH, LH], 0.62), face([L0, LH0, LH], 0.74), face([R0, RH0, RH], 0.74)];
  return { lo, hi, faces, style: { fill: C.slab, stroke: "#2a2730", role: "ramp" }, ramp: r };
}
const solids = [];
for (const b of boxes) solids.push(boxSolid(b.min, b.max, boxRole(b), { box: b }));
for (const r of ramps) solids.push(rampSolid(r));
for (const t of tunnels) solids.push(boxSolid([t.rect[0], t.ceiling, t.rect[1]], [t.rect[2], t.ceiling + 0.25, t.rect[3]], { fill: C.hull, stroke: C.bound, role: "tunnel ceiling", alpha: 0.45 }));
for (const p of props) {
  const sz = p.size || [0.6, 0.6, 0.6], at = p.at || [0, 0, 0];
  const q = Math.round((((p.yawDeg || 0) % 180) + 180) % 180 / 90) % 2 === 1; // a 90 deg turn swaps the footprint (other yaws drawn axis-aligned)
  const hx = (q ? sz[2] : sz[0]) / 2, hz = (q ? sz[0] : sz[2]) / 2, hy = sz[1] / 2;
  solids.push(boxSolid([at[0] - hx, at[1] - hy, at[2] - hz], [at[0] + hx, at[1] + hy, at[2] + hz], propRole(p), { prop: p }));
}

// ---------------------------------------------------------------- the isometric panel
function proj(V, p) { const h = p[0] * V.fx + p[2] * V.fz; return { sx: p[0] * V.rx + p[2] * V.rz, sy: p[1] * V.ce + h * V.se, d: h * V.ce - p[1] * V.se }; }
function fit(V, P) {
  // the frame: the slot's rect from FRAME_LO to FRAME_HI (contents never move it)
  const pts = [];
  for (const [x, z] of corners) for (const y of [FRAME_LO, FRAME_HI]) pts.push(proj(V, [x, y, z]));
  const minx = Math.min(...pts.map((q) => q.sx)), maxx = Math.max(...pts.map((q) => q.sx)), miny = Math.min(...pts.map((q) => q.sy)), maxy = Math.max(...pts.map((q) => q.sy));
  const s = Math.min((P.w - 40) / (maxx - minx), (P.h - 60) / (maxy - miny));
  const ox = P.x + P.w / 2 - ((minx + maxx) / 2) * s, oy = P.y + 30 + (P.h - 30) / 2 + ((miny + maxy) / 2) * s;
  return (p) => { const q = proj(V, p); return [ox + q.sx * s, oy - q.sy * s]; };
}
// a door's gap on the room's wall: the side of the door's rect [x0, z0, x1, z1] that lies on the room's rect, as a quad up to the door's height
function doorGap(d) {
  const [a, b, c, e] = d.rect, h = d.height || 4.5, on = (u, v) => Math.abs(u - v) < 1e-6;
  const zz = on(b, Z1) || on(e, Z1) ? Z1 : on(b, Z0) || on(e, Z0) ? Z0 : null;
  if (zz !== null) return [[a, 0, zz], [c, 0, zz], [c, h, zz], [a, h, zz]];
  const xx = on(a, X1) || on(c, X1) ? X1 : on(a, X0) || on(c, X0) ? X0 : null;
  if (xx !== null) return [[xx, 0, b], [xx, 0, e], [xx, h, e], [xx, h, b]];
  return null;
}
const ptsAttr = (S, poly) => poly.map((p) => S(p)).map(([x, y]) => f1(x) + "," + f1(y)).join(" ");
// draw order: a separating plane between two AABBs puts the one on the camera's side in front (consistent for convex disjoint pieces);
// overlapping AABBs fall back to the centres' depth; Kahn's order, ties by the centre's depth then the list index (deterministic)
function order(V, S) {
  const n = solids.length, eps = 1e-6;
  const bb = solids.map((s) => { const xs = [], ys = []; for (const x of [s.lo[0], s.hi[0]]) for (const y of [s.lo[1], s.hi[1]]) for (const z of [s.lo[2], s.hi[2]]) { const [a, b] = S([x, y, z]); xs.push(a); ys.push(b); } return [Math.min(...xs), Math.min(...ys), Math.max(...xs), Math.max(...ys)]; });
  const depth = solids.map((s) => proj(V, [(s.lo[0] + s.hi[0]) / 2, (s.lo[1] + s.hi[1]) / 2, (s.lo[2] + s.hi[2]) / 2]).d);
  const before = Array.from({ length: n }, () => []); // before[i]: the pieces drawn before i (behind it)
  const indeg = new Array(n).fill(0);
  const front = (i, j) => { // 1: i in front of j, -1: j in front of i, 0: no separating plane
    const A = solids[i], B = solids[j];
    const cam = [-V.fx, 1, -V.fz]; // the camera's side along x, y, z (it sits at -f horizontally and above)
    for (let k = 0; k < 3; k++) {
      if (A.hi[k] <= B.lo[k] + eps) { if (Math.abs(cam[k]) < 1e-9) continue; return cam[k] < 0 ? 1 : -1; } // A on the low side of the plane
      if (B.hi[k] <= A.lo[k] + eps) { if (Math.abs(cam[k]) < 1e-9) continue; return cam[k] < 0 ? -1 : 1; }
    }
    return 0;
  };
  for (let i = 0; i < n; i++) for (let j = i + 1; j < n; j++) {
    const a = bb[i], b = bb[j];
    if (a[2] <= b[0] || b[2] <= a[0] || a[3] <= b[1] || b[3] <= a[1]) continue; // no overlap on screen: no constraint
    let fr = front(i, j);
    if (fr === 0) fr = depth[i] < depth[j] ? 1 : depth[i] > depth[j] ? -1 : (i < j ? 1 : -1);
    if (fr === 1) { before[i].push(j); indeg[i]++; } else { before[j].push(i); indeg[j]++; }
  }
  const after = Array.from({ length: n }, () => []);
  for (let i = 0; i < n; i++) for (const j of before[i]) after[j].push(i);
  const done = new Array(n).fill(false), res = [];
  const pick = () => { let best = -1; for (let i = 0; i < n; i++) if (!done[i] && indeg[i] === 0 && (best < 0 || depth[i] > depth[best] || (depth[i] === depth[best] && i < best))) best = i; return best; };
  while (res.length < n) {
    let i = pick();
    if (i < 0) { for (let k = 0; k < n; k++) if (!done[k] && (i < 0 || depth[k] > depth[i])) i = k; } // a cycle: the farthest first
    done[i] = true; res.push(i);
    for (const j of after[i]) indeg[j]--;
  }
  return res;
}
function isoPanel(P, V, title) {
  const S = fit(V, P);
  const clip = "clip" + V.name;
  out.push(`<clipPath id="${clip}"><rect x="${P.x}" y="${P.y}" width="${P.w}" height="${P.h}"/></clipPath>`);
  out.push(`<rect x="${P.x}" y="${P.y}" width="${P.w}" height="${P.h}" fill="${C.panel}" stroke="#3a3648"/>`);
  out.push(`<g clip-path="url(#${clip})">`);
  const hgt = row.height || 8;
  // the floor and the room's boundary (pale violet-white wireframe)
  out.push(`<polygon points="${ptsAttr(S, [[X0, 0, Z0], [X1, 0, Z0], [X1, 0, Z1], [X0, 0, Z1]])}" fill="${C.floor}" stroke="${C.bound}" stroke-width="1.5"/>`);
  // 1 m floor grid (fixed per slot: the rect's own metres)
  const g = [];
  for (let x = Math.ceil(X0); x <= X1; x += 2) g.push(ptsAttr(S, [[x, 0, Z0], [x, 0, Z1]]));
  for (let z = Math.ceil(Z0); z <= Z1; z += 2) g.push(ptsAttr(S, [[X0, 0, z], [X1, 0, z]]));
  for (const l of g) out.push(`<polyline points="${l}" stroke="#3b3844" stroke-width="0.6" fill="none"/>`);
  for (const [x, z] of corners) out.push(`<polyline points="${ptsAttr(S, [[x, 0, z], [x, hgt, z]])}" stroke="${C.bound}" stroke-width="1" stroke-opacity="0.55" fill="none"/>`);
  out.push(`<polygon points="${ptsAttr(S, [[X0, hgt, Z0], [X1, hgt, Z0], [X1, hgt, Z1], [X0, hgt, Z1]])}" fill="none" stroke="${C.bound}" stroke-width="1" stroke-opacity="0.55" stroke-dasharray="${row.roof ? "none" : "6 4"}"/>`);
  for (const d of doors) {
    const poly = doorGap(d);
    if (!poly) continue;
    out.push(`<polygon points="${ptsAttr(S, poly)}" fill="${C.bound}" fill-opacity="0.18" stroke="${C.bound}" stroke-width="2"/>`);
    const m = [(poly[0][0] + poly[1][0]) / 2, poly[2][1] + 0.4, (poly[0][2] + poly[1][2]) / 2];
    const [tx, ty] = S(m);
    text(tx - 10, ty, d.id + (d === entry ? " in" : " out"), 13, C.bound);
  }
  // pits: the sunk rect at its floor (lava red-orange, else void) and its rim on the floor
  for (const p of pits) {
    const [a, b, c, e] = p.rect, y = p.floorY !== undefined ? p.floorY : -1;
    out.push(`<polygon points="${ptsAttr(S, [[a, y, b], [c, y, b], [c, y, e], [a, y, e]])}" fill="${p.lava ? C.lava : C.void}" fill-opacity="0.92" stroke="${C.red}" stroke-width="1"/>`);
    out.push(`<polygon points="${ptsAttr(S, [[a, 0, b], [c, 0, b], [c, 0, e], [a, 0, e]])}" fill="none" stroke="${C.red}" stroke-width="2"/>`);
  }
  // the solids in order, each visible face (outward normal toward the camera)
  const toCam = [-V.fx * V.ce, V.se, -V.fz * V.ce];
  for (const i of order(V, S)) {
    const s = solids[i], st = s.style;
    for (const fc of s.faces) {
      if (fc.n[0] * toCam[0] + fc.n[1] * toCam[1] + fc.n[2] * toCam[2] <= 1e-9) continue;
      let fill = shade(st.fill, fc.k), stroke = st.stroke, sw = 1;
      const rf = s.box && s.box.ride36 && s.box.ride36.face;
      if (rf && fc.side === rf) { fill = C.cyan; stroke = "#e9fdff"; sw = 1.6; } // the ride face
      out.push(`<polygon points="${ptsAttr(S, fc.p)}" fill="${fill}" fill-opacity="${st.alpha || 0.93}" stroke="${stroke}" stroke-width="${sw}"${st.dash ? ' stroke-dasharray="3 2"' : ""}/>`);
    }
  }
  overlays(S, (p) => S(p));
  out.push(`</g>`);
  text(P.x + 10, P.y + 20, title, 15, C.text);
}
// markers over the solids (they read through walls: this is a diagram, not a frame)
function overlays(S) {
  const mark = (p, r, fill, stroke, sw) => { const [x, y] = S(p); out.push(`<circle cx="${f1(x)}" cy="${f1(y)}" r="${r}" fill="${fill}" stroke="${stroke}" stroke-width="${sw || 1}"/>`); };
  const stem = (p, col) => { if (Math.abs(p[1]) < 0.05) return; out.push(`<polyline points="${ptsAttr(S, [[p[0], 0, p[2]], p])}" stroke="${col}" stroke-width="1" stroke-dasharray="2 2" fill="none"/>`); };
  for (const l of lights) {
    const c = l.danger ? C.red : LCOL[l.color] || C.text;
    // a cyan light (a ride wall's line, a pool under a pad or a slot) is drawn as its own strip / panel in the cyan role, not a dot
    const sz = l.size || [0, 0, 0], n = l.normal || [0, 1, 0];
    if (l.color === "cyan" && Math.max(sz[0], sz[1]) >= 0.5) {
      let q;
      if (Math.abs(n[1]) > 0.5) q = [[l.at[0] - sz[0] / 2, l.at[1], l.at[2] - sz[1] / 2], [l.at[0] + sz[0] / 2, l.at[1], l.at[2] - sz[1] / 2], [l.at[0] + sz[0] / 2, l.at[1], l.at[2] + sz[1] / 2], [l.at[0] - sz[0] / 2, l.at[1], l.at[2] + sz[1] / 2]];
      else { const ax = Math.abs(n[0]) > 0.5 ? [0, 1] : [1, 0], h = Math.max(sz[1], 0.25) / 2, w = sz[0] / 2; q = [[l.at[0] - ax[0] * w, l.at[1] - h, l.at[2] - ax[1] * w], [l.at[0] + ax[0] * w, l.at[1] - h, l.at[2] + ax[1] * w], [l.at[0] + ax[0] * w, l.at[1] + h, l.at[2] + ax[1] * w], [l.at[0] - ax[0] * w, l.at[1] + h, l.at[2] - ax[1] * w]]; }
      out.push(`<polygon points="${ptsAttr(S, q)}" fill="${C.cyan}" fill-opacity="0.85" stroke="#e9fdff" stroke-width="1.2"/>`);
    } else mark(l.at, 3, c, "#000000", 0.6);
  }
  for (const p of pads) {
    stem(p.at, C.cyan);
    const ring = []; for (let k = 0; k < 16; k++) { const a = (k / 16) * Math.PI * 2; ring.push([p.at[0] + Math.cos(a) * 0.8, p.at[1] + 0.03, p.at[2] + Math.sin(a) * 0.8]); }
    out.push(`<polygon points="${ptsAttr(S, ring)}" fill="${C.cyan}" fill-opacity="0.75" stroke="#e9fdff" stroke-width="1.2"/>`);
    out.push(`<polyline points="${ptsAttr(S, arc(p))}" fill="none" stroke="${C.cyan}" stroke-width="2" stroke-dasharray="6 4"/>`);
    mark(p.target, 3.5, "none", C.cyan, 1.6);
  }
  for (const g of grapple) { stem(g.at, C.cyan); mark(g.at, 6, "none", C.cyan, 2.5); mark(g.at, 2, C.cyan, C.cyan); }
  for (const s of spawns()) { stem(s.at, C.red); if (s.kind === "stand") { const [x, y] = S(s.at); out.push(`<path d="M${f1(x - 5)},${f1(y - 5)}L${f1(x + 5)},${f1(y + 5)}M${f1(x - 5)},${f1(y + 5)}L${f1(x + 5)},${f1(y - 5)}" stroke="${C.red}" stroke-width="2"/>`); } else mark(s.at, s.w1 ? 5 : 4, s.w1 ? C.red : "none", C.red, 2); }
  if (row.spawn && row.spawn.at) { const [x, y] = S(row.spawn.at); out.push(`<polygon points="${f1(x)},${f1(y - 9)} ${f1(x - 6)},${f1(y + 4)} ${f1(x + 6)},${f1(y + 4)}" fill="${C.cyan}" stroke="#ffffff" stroke-width="1"/>`); }
  if (row.exit) { mark(row.exit, 7, "none", C.bound, 2); const [x, y] = S(row.exit); text(x + 9, y + 4, "EXIT", 12, C.bound); }
  const line = line65();
  if (line.length) {
    for (const p of line) stem(p, C.cyanLine);
    out.push(`<polyline points="${ptsAttr(S, line)}" fill="none" stroke="#0c2a30" stroke-width="6" stroke-linejoin="round"/>`);
    out.push(`<polyline points="${ptsAttr(S, line)}" fill="none" stroke="${C.cyanLine}" stroke-width="3" stroke-linejoin="round"/>`);
    line.forEach((p, i) => { const [x, y] = S(p); out.push(`<circle cx="${f1(x)}" cy="${f1(y)}" r="7" fill="#0c2a30" stroke="${C.cyanLine}" stroke-width="1.5"/>`); text(x - 4, y + 4, String(i + 1), 11, C.cyanLine); });
  }
  for (const [name, f] of frames65()) {
    const [x, y] = S([f[0], f[1] + 1.6, f[2]]);
    const yaw = ((f[3] || 0) * Math.PI) / 180, dir = [-Math.sin(yaw), -Math.cos(yaw)];
    const [tx, ty] = S([f[0] + dir[0] * 1.6, f[1] + 1.6, f[2] + dir[1] * 1.6]);
    out.push(`<polyline points="${f1(x)},${f1(y)} ${f1(tx)},${f1(ty)}" stroke="#ffffff" stroke-width="2"/><circle cx="${f1(x)}" cy="${f1(y)}" r="3.5" fill="#ffffff"/>`);
    text(x + 6, y - 6, name, 12, "#ffffff");
  }
}
function arc(p) {
  const a = p.at, t = p.target, top = Math.max(a[1], t[1]) + (p.apexAbove || 1.5), pts = [];
  // a parabola through at, the apex and target (the drawn line, not the sim's integration)
  const h1 = top - a[1], h2 = top - t[1], k = Math.sqrt(h1) / (Math.sqrt(h1) + Math.sqrt(h2)); // the apex's fraction along the line
  for (let i = 0; i <= 24; i++) {
    const u = i / 24, x = a[0] + (t[0] - a[0]) * u, z = a[2] + (t[2] - a[2]) * u;
    const y = u <= k ? top - h1 * Math.pow((k - u) / k, 2) : top - h2 * Math.pow((u - k) / (1 - k), 2);
    pts.push([x, y, z]);
  }
  return pts;
}
function spawns() {
  const res = [], seen = new Set();
  const w1 = new Set(((row.waveNodes43 || [])[0] || []).map((p) => JSON.stringify(p)));
  const add = (p, kind) => { const k = JSON.stringify(p); if (seen.has(k)) return; seen.add(k); res.push({ at: p, kind, w1: w1.has(k) }); };
  for (const p of row.nodes || []) add(p, "node");
  for (const p of row.highNodes || []) add(p, "high");
  for (const w of row.waveNodes43 || []) for (const p of w || []) add(p, "wave");
  for (const s of row.standIns || []) if (s && s.at) add(s.at, "stand");
  return res;
}
// --no-card-marks: no line65 and no frames65 drawn (the blind pair: n52's rooms have neither, so the marks alone would name the rebuild)
function line65() {
  if (flags["--no-card-marks"]) return [];
  let l = row.line65;
  if (l && !Array.isArray(l)) l = l.points || l.waypoints || [];
  return (l || []).filter((p) => Array.isArray(p) && p.length >= 3).map((p) => [Number(p[0]), Number(p[1]), Number(p[2])]);
}
function frames65() {
  if (flags["--no-card-marks"]) return [];
  const f = row.frames65 || {};
  return Object.keys(f).filter((k) => !k.startsWith("_") && Array.isArray(f[k]) && f[k].length >= 3).sort().map((k) => [k, f[k]]);
}

// ---------------------------------------------------------------- the elevations: along the long axis (seen from the side, the entry at the
// left) and across it (seen from the entry), at one scale fixed by the slot's rect
function elevation(P) {
  const alongZ = Z1 - Z0 >= X1 - X0;
  const entryHigh = alongZ ? E[1] >= CZ : E[0] >= CX; // the entry on the rect's high side of the axis
  const L = alongZ ? Z1 - Z0 : X1 - X0, Wd = alongZ ? X1 - X0 : Z1 - Z0;
  const sOf = (p) => (alongZ ? (entryHigh ? Z1 - p[2] : p[2] - Z0) : (entryHigh ? X1 - p[0] : p[0] - X0)); // metres from the entry wall
  // across, as the entry sees it (its left at 0): the right vector of the entry's forward
  const r0 = [-F0[1], F0[0]];
  const xOf = (p) => { const left = [CX - r0[0] * (alongZ ? (X1 - X0) / 2 : (Z1 - Z0) / 2), CZ - r0[1] * (alongZ ? (X1 - X0) / 2 : (Z1 - Z0) / 2)]; return (p[0] - left[0]) * r0[0] + (p[2] - left[1]) * r0[1]; };
  const LAB = 175, GAP = 50;
  const sc = Math.min((P.w - 2 * LAB - GAP - 60) / (L + Wd), (P.h - 70) / (FRAME_HI - FRAME_LO));
  out.push(`<rect x="${P.x}" y="${P.y}" width="${P.w}" height="${P.h}" fill="${C.panel}" stroke="#3a3648"/>`);
  out.push(`<clipPath id="clipE"><rect x="${P.x}" y="${P.y}" width="${P.w}" height="${P.h}"/></clipPath><g clip-path="url(#clipE)">`);
  const oy = P.y + P.h - 30 + FRAME_LO * sc;
  // drawKey: the piece's distance from the section's viewer (drawn far first): the long one seen from the low side of the cross axis, the
  // cross one from the entry
  const planes = section(P.x + 40, oy, sc, L, sOf, (s) => (alongZ ? (s.lo[0] + s.hi[0]) / 2 : (s.lo[2] + s.hi[2]) / 2), true, true, xOf);
  section(P.x + 40 + L * sc + LAB + GAP, oy, sc, Wd, xOf, (s) => sOf([(s.lo[0] + s.hi[0]) / 2, 0, (s.lo[2] + s.hi[2]) / 2]), true, false, xOf);
  out.push(`</g>`);
  const axis = alongZ ? (entryHigh ? "z, north from the entry wall" : "z, south from the entry wall") : (entryHigh ? "x, west from the entry wall" : "x, east from the entry wall");
  text(P.x + 10, P.y + 20, "ELEVATIONS: along the long axis (" + axis + ", seen from the side) | across it (seen from the entry) | half see-through", 15, C.text);
  text(P.x + 10, P.y + 40, "standing planes: " + planes.join(" / ") + " m (deck / slab tops, ramp tops, the floor)", 14, C.dim);
  return planes;
}
// one section: the horizontal coordinate hOf(p) over [0, span], the solids drawn by drawKey (far first), the planes labelled when `labels`
function section(ox, oy, sc, span, hOf, drawKey, labels, long, xOf) {
  const T = (s, y) => [ox + s * sc, oy - y * sc];
  const tp = (p) => T(hOf(p), p[1]);
  const pl = (pts) => pts.map(([x, y]) => f1(x) + "," + f1(y)).join(" ");
  const L = span;
  const hgt = row.height || 8;
  // the planes: every walkable top (deck / slab boxes, ramp tops) and the floor, labelled at the right
  const tops = new Map();
  const addTop = (y, what) => { const k = (Math.round(y * 10) / 10).toFixed(1); tops.set(k, (tops.get(k) || []).concat([what])); };
  addTop(0, "floor");
  for (const b of boxes) if ((b.kind === "deck" || b.kind === "slab") && Math.abs(b.max[1]) >= 0.3) addTop(b.max[1], b.kind + (b.max[1] < 0 ? " (in a pit)" : ""));
  for (const p of pits) if (p.floorY !== undefined && p.floorY <= -0.3) addTop(p.floorY, p.lava ? "lava" : "pit floor");
  for (const r of ramps) addTop(r.rise, "ramp top");
  const planeKeys = [...tops.keys()].sort((a, b) => Number(a) - Number(b));
  for (const k of planeKeys) {
    const y = Number(k), [xa, ya] = T(-0.5, y), [xb] = T(L + 0.5, y);
    out.push(`<polyline points="${f1(xa)},${f1(ya)} ${f1(xb)},${f1(ya)}" stroke="#4c4760" stroke-width="1" stroke-dasharray="5 4" fill="none"/>`);
    const n = tops.get(k), cnt = {}; for (const w of n) cnt[w] = (cnt[w] || 0) + 1;
    if (labels) text(xb + 8, ya + 4, k + " m" + (long ? ": " + Object.keys(cnt).sort().map((w) => (cnt[w] > 1 ? cnt[w] + " " : "") + w).join(", ") : ""), 13, C.text);
  }
  // the room's wall line, roof and doors
  const [r0x, r0y] = T(0, hgt), [r1x] = T(L, hgt), [f0x, f0y] = T(0, 0), [f1x] = T(L, 0);
  out.push(`<polyline points="${f1(f0x)},${f1(f0y)} ${f1(f1x)},${f1(f0y)}" stroke="${C.bound}" stroke-width="2" fill="none"/>`);
  out.push(`<polyline points="${f1(r0x)},${f1(r0y)} ${f1(r1x)},${f1(r0y)}" stroke="${C.bound}" stroke-width="1.5" stroke-dasharray="${row.roof ? "none" : "6 4"}" fill="none"/>`);
  text(r1x + 8, r0y + 4, (row.roof ? "roof " : "wall top ") + hgt + " m", 13, C.bound);
  for (const x of [f0x, f1x]) out.push(`<polyline points="${f1(x)},${f1(f0y)} ${f1(x)},${f1(r0y)}" stroke="${C.bound}" stroke-width="1" stroke-opacity="0.6" fill="none"/>`);
  for (const d of doors) {
    const h = d.height || 4.5;
    if (long) {
      const s = d === entry ? 0 : L, [x, y0] = T(s, 0), [, y1] = T(s, h);
      out.push(`<rect x="${f1(x - 4)}" y="${f1(y1)}" width="8" height="${f1(y0 - y1)}" fill="${C.bound}" fill-opacity="0.35" stroke="${C.bound}"/>`);
      text(d === entry ? x + 8 : x - 58, y1 - 6, d.id + (d === entry ? " in" : " out"), 12, C.bound);
    } else {
      // across: the door's opening as the entry sees it (the exit's behind the entry's when they line up)
      const xs = [xOf([d.rect[0], 0, d.rect[1]]), xOf([d.rect[2], 0, d.rect[3]])];
      const [xa, y0] = T(Math.min(...xs), 0), [xb, y1] = T(Math.max(...xs), h);
      out.push(`<rect x="${f1(xa)}" y="${f1(y1)}" width="${f1(xb - xa)}" height="${f1(y0 - y1)}" fill="${C.bound}" fill-opacity="0.12" stroke="${C.bound}" stroke-dasharray="${d === entry ? "none" : "4 3"}"/>`);
      text(xa + 2, y1 - 6 - (d === entry ? 0 : 14), d.id + (d === entry ? " in" : " out"), 12, C.bound);
    }
  }
  // metres along the axis
  for (let s = 0; s <= L + 1e-6; s += 2) { const [x, y] = T(s, FRAME_LO); text(x - 6, y + 14, String(s), 11, C.dim); }
  // pits, then the solids (hulls of their projected corners, drawn far side first, half see-through)
  for (const p of pits) {
    const [a, b, c, e] = p.rect, y = p.floorY !== undefined ? p.floorY : -1;
    const ss = [hOf([a, 0, b]), hOf([c, 0, e])];
    const [xa, ya] = T(Math.min(...ss), 0), [xb, yb] = T(Math.max(...ss), y);
    out.push(`<rect x="${f1(xa)}" y="${f1(ya)}" width="${f1(xb - xa)}" height="${f1(yb - ya)}" fill="${p.lava ? C.lava : C.void}" stroke="${C.red}" stroke-width="1.5"/>`);
  }
  const idx = solids.map((s, i) => i).sort((i, j) => drawKey(solids[j]) - drawKey(solids[i]) || i - j);
  for (const i of idx) {
    const s = solids[i], pts = [];
    for (const fc of s.faces) for (const p of fc.p) pts.push(tp(p));
    const h = hull(pts), st = s.style;
    out.push(`<polygon points="${pl(h)}" fill="${st.fill}" fill-opacity="${s.box && s.box.ride ? 0.75 : 0.5}" stroke="${s.box && s.box.ride ? C.cyan : st.stroke === "#2a2730" ? "#d8d0c0" : st.stroke}" stroke-width="1"${st.dash ? ' stroke-dasharray="3 2"' : ""}/>`);
  }
  for (const p of pads) {
    out.push(`<polyline points="${pl(arc(p).map(tp))}" fill="none" stroke="${C.cyan}" stroke-width="2" stroke-dasharray="6 4"/>`);
    const [x, y] = tp(p.at); out.push(`<rect x="${f1(x - 7)}" y="${f1(y - 3)}" width="14" height="3" fill="${C.cyan}"/>`);
  }
  for (const g of grapple) { const [x, y] = tp(g.at); out.push(`<circle cx="${f1(x)}" cy="${f1(y)}" r="6" fill="none" stroke="${C.cyan}" stroke-width="2.5"/>`); }
  for (const s of spawns()) { const [x, y] = tp(s.at); out.push(`<circle cx="${f1(x)}" cy="${f1(y - 4)}" r="${s.w1 ? 5 : 4}" fill="${s.w1 ? C.red : "none"}" stroke="${C.red}" stroke-width="2"/>`); }
  if (row.spawn && row.spawn.at) { const [x, y] = tp(row.spawn.at); out.push(`<polygon points="${f1(x)},${f1(y - 14)} ${f1(x - 6)},${f1(y)} ${f1(x + 6)},${f1(y)}" fill="${C.cyan}" stroke="#ffffff"/>`); }
  const line = line65();
  if (line.length) {
    out.push(`<polyline points="${pl(line.map(tp))}" fill="none" stroke="${C.cyanLine}" stroke-width="3" stroke-linejoin="round"/>`);
    line.forEach((p, i) => { const [x, y] = tp(p); out.push(`<circle cx="${f1(x)}" cy="${f1(y)}" r="7" fill="#0c2a30" stroke="${C.cyanLine}" stroke-width="1.5"/>`); text(x - 4, y + 4, String(i + 1), 11, C.cyanLine); });
  }
  return planeKeys;
}
function hull(pts) {
  const p = pts.map(([x, y]) => [Math.round(x * 10) / 10, Math.round(y * 10) / 10]).sort((a, b) => a[0] - b[0] || a[1] - b[1]);
  const cr = (o, a, b) => (a[0] - o[0]) * (b[1] - o[1]) - (a[1] - o[1]) * (b[0] - o[0]);
  const lo = [], up = [];
  for (const q of p) { while (lo.length >= 2 && cr(lo[lo.length - 2], lo[lo.length - 1], q) <= 0) lo.pop(); lo.push(q); }
  for (let i = p.length - 1; i >= 0; i--) { const q = p[i]; while (up.length >= 2 && cr(up[up.length - 2], up[up.length - 1], q) <= 0) up.pop(); up.push(q); }
  return lo.slice(0, -1).concat(up.slice(0, -1));
}

// ---------------------------------------------------------------- the page
out.push(`<rect x="0" y="0" width="${W}" height="${H}" fill="${C.bg}"/>`);
const w = X1 - X0, d = Z1 - Z0;
text(14, 26, (plan.id || "?") + "  " + roomId + "  " + (row.name || "") + "  |  slot rect " + w + " x " + d + " m, height " + (row.height || "?") + " m" + (row.roof ? ", roofed" : "") + "  |  entry " + (entry ? entry.id : "none") + ", exit " + (exitDoor ? exitDoor.id : "none"), 18, C.text);
text(14, 50, "pieces: " + boxes.length + " boxes (" + boxes.filter((b) => b.ride).length + " ride), " + ramps.length + " ramps, " + pads.length + " pads, " + grapple.length + " grapple, " + pits.length + " pits, " + props.length + " props, " + lights.length + " lights, " + spawns().length + " enemy spots; line65 " + line65().length + " points, frames65 " + frames65().length + (flags["--plant-box"] ? "  |  PLANTED BOX " + flags["--plant-box"] : ""), 14, C.dim);
isoPanel(PA, VA, "A: from the entry (" + (entry ? entry.id : "the south wall") + "), looking " + compass(FA) + ", isometric 35.3 deg");
isoPanel(PB, VB, "B: from the far corner (" + farC.join(", ") + "), looking " + compass(FB) + ", isometric 35.3 deg");
const planes = elevation(PE);
// the legend
const LG = [[C.cyan, "player-usable: pads + arcs, ride walls (bright face), grapple, pickups, spawn"], [C.cyanLine, "line65: the card's verb line (numbered)"], [C.red, "danger: lava, pits, enemy spots (filled = wave 1, x = stand-in)"], [C.bound, "boundary: walls, doors, exit, roof"], [C.deck, "structure: deck / slab / hull / wall"], [C.decor, "dressing (dashed: no collider)"], ["#ffffff", "frames65 poses / a planted box"]];
let lx = 14, ly = 1026;
for (const [c, s] of LG) {
  const wd = 20 + s.length * 7.2 + 18;
  if (lx + wd > W - 10) { lx = 14; ly += 22; }
  out.push(`<rect x="${lx}" y="${ly}" width="14" height="14" fill="${c}"/>`);
  text(lx + 20, ly + 12, s, 13, C.text);
  lx += wd;
}
text(14, 1088, "iso65 (Tools/night/n65-iso.cjs): the cameras are fixed per slot (the rect and its doors only), so n52's room and its rebuild are drawn from the same views; source " + src, 13, C.dim);
const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="${W}" height="${H}" viewBox="0 0 ${W} ${H}">\n${out.join("\n")}\n</svg>\n`;

// ---------------------------------------------------------------- SVG -> PNG (headless Edge, as Tools/night/n52-svg-png.sh)
const svgPath = flags["--svg"] ? abs(flags["--svg"]) : path.join(fs.mkdtempSync(path.join(os.tmpdir(), "n65iso-")), roomId + ".svg");
fs.mkdirSync(path.dirname(svgPath), { recursive: true });
fs.writeFileSync(svgPath, svg);
const png = abs(outPng);
fs.mkdirSync(path.dirname(png), { recursive: true });
if (fs.existsSync(png)) fs.rmSync(png);
const EDGE = "C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe";
const url = "file:///" + path.resolve(svgPath).replace(/\\/g, "/");
const r = cp.spawnSync(EDGE, ["--headless=new", "--disable-gpu", "--hide-scrollbars", "--screenshot=" + png, "--window-size=" + W + "," + H, url], { encoding: "utf8", timeout: 60000 });
if (!flags["--svg"]) fs.rmSync(path.dirname(svgPath), { recursive: true, force: true });
if (!fs.existsSync(png)) { console.error("n65-iso: Edge wrote no PNG (exit " + r.status + "): " + String(r.stderr || "").slice(-400)); process.exit(1); }
const sha = crypto.createHash("sha256").update(fs.readFileSync(png)).digest("hex");
const svgSha = crypto.createHash("sha256").update(svg).digest("hex");
console.log("n65-iso: " + outPng + " (" + W + "x" + H + ", png sha256 " + sha.slice(0, 16) + ", svg sha256 " + svgSha.slice(0, 16) + ") " + (plan.id || "?") + " " + roomId + ": slot rect " + w + " x " + d + " m, entry " + (entry ? entry.id : "none") +
  "; views A " + compass(FA) + ", B " + compass(FB) + " from corner (" + farC.join(", ") + "); planes " + planes.join(" / ") + " m; " + boxes.length + " boxes, " + ramps.length + " ramps, " + pads.length + " pads, " + grapple.length + " grapple, " + pits.length + " pits, " + props.length + " props, " + spawns().length + " enemy spots, line65 " + line65().length + ", frames65 " + frames65().length);
