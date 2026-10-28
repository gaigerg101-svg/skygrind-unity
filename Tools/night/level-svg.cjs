// NIGHT30 (builder L): a top-down SVG of a LEVEL plan (Design/levels/n29|n30/*.plan.json, the file the generators write and levelgen reads).
// Usage: node Tools/night/level-svg.cjs <plan.json> <out.svg> [--rooms f5,f5b] [--scale 10]
// Draws north UP (the plan's z runs SOUTH, so svg y = z): rooms (rect + id + height), doors, boxes (shaded by top height, labelled), ramps (arrow
// up-slope + rise), pits (void: black, with floorY) and LAVA (red-orange, the surface y), hazard floors (hatched), ice surfaces (pale blue), lights
// (dots in their palette colour), props (outlines; solid when they have a collider), pickups (green diamonds), spawn (cyan triangle), exit
// (white ring), encounter nodes (small red-orange dots) and high nodes (squares), grapple nodes (cyan rings), proofs (dotted paths).
// (Tools/map-plan-svg.cjs reads only Design/maps plans and crashes on a level plan.)
const fs = require("fs");

const args = process.argv.slice(2);
const inFile = args[0], outFile = args[1];
if (!inFile || !outFile) { console.error("usage: node Tools/night/level-svg.cjs <plan.json> <out.svg> [--rooms a,b] [--scale n]"); process.exit(2); }
const opt = (k, d) => { const i = args.indexOf(k); return i >= 0 ? args[i + 1] : d; };
const plan = JSON.parse(fs.readFileSync(inFile, "utf8"));
const only = opt("--rooms", "") ? new Set(opt("--rooms", "").split(",")) : null;
const S = parseFloat(opt("--scale", "8"));

const COL = { cyan: "#36e3ff", green: "#5cff8a", amber: "#ffb43a", white: "#f2efff", orange: "#ff5a1f" };
const LAVA = "#ff4d12", VOID = "#050508";
const rooms = (plan.rooms || []).filter((r) => !only || only.has(r.id));
const inRoom = (zone) => !only || only.has(zone);
if (!rooms.length) { console.error("no rooms match"); process.exit(1); }

let x0 = 1e9, z0 = 1e9, x1 = -1e9, z1 = -1e9;
for (const r of rooms) { x0 = Math.min(x0, r.rect[0]); z0 = Math.min(z0, r.rect[1]); x1 = Math.max(x1, r.rect[2]); z1 = Math.max(z1, r.rect[3]); }
// NIGHT31 (builder L): a boss room's ARENA overlay (sink31: the Lantern ring; crumble31: the Wyrm arena) widens the frame to the arena
const grow = (x, z) => { x0 = Math.min(x0, x); z0 = Math.min(z0, z); x1 = Math.max(x1, x); z1 = Math.max(z1, z); };
for (const r of rooms) {
  if (r.sink31 && r.sink31.draw) r.sink31.draw.ring.forEach(([x, z]) => grow(x, z));
  if (r.crumble31 && r.crumble31.draw) { const d = r.crumble31.draw; grow(d.center[0] - d.half, d.center[1] - d.half); grow(d.center[0] + d.half, d.center[1] + d.half); }
}
const PAD = 6, LEG = 150;
x0 -= PAD; z0 -= PAD; x1 += PAD; z1 += PAD;
const W = Math.ceil((x1 - x0) * S) + LEG, H = Math.ceil((z1 - z0) * S);
const X = (x) => ((x - x0) * S).toFixed(1), Y = (z) => ((z - z0) * S).toFixed(1);
const esc = (s) => String(s).replace(/&/g, "&amp;").replace(/</g, "&lt;");
const out = [];
const rectEl = (rc, attrs) => `<rect x="${X(rc[0])}" y="${Y(rc[1])}" width="${((rc[2] - rc[0]) * S).toFixed(1)}" height="${((rc[3] - rc[1]) * S).toFixed(1)}" ${attrs}/>`;
const text = (x, z, s, size, fill, extra) => `<text x="${X(x)}" y="${Y(z)}" font-size="${size}" fill="${fill}" font-family="Consolas,monospace" ${extra || ""}>${esc(s)}</text>`;
// box tops: grey ramp from dark (low) to light (high)
const shade = (y) => { const t = Math.max(0, Math.min(1, y / 8)); const v = Math.round(70 + 140 * t); return `rgb(${v},${v},${Math.round(v * 1.05)})`; };

out.push(`<svg xmlns="http://www.w3.org/2000/svg" width="${W}" height="${H}" viewBox="0 0 ${W} ${H}">`);
out.push(`<defs><pattern id="hatch" width="8" height="8" patternUnits="userSpaceOnUse" patternTransform="rotate(45)"><line x1="0" y1="0" x2="0" y2="8" stroke="${COL.orange}" stroke-width="2" opacity="0.55"/></pattern>`
  + `<marker id="arr" markerWidth="8" markerHeight="8" refX="6" refY="4" orient="auto"><path d="M0,0 L8,4 L0,8 z" fill="#ffe08a"/></marker></defs>`);
out.push(`<rect width="${W}" height="${H}" fill="#14121c"/>`);

// rooms: the floor and the wall ring (2 m)
for (const r of rooms) {
  const rc = r.rect;
  out.push(rectEl([rc[0] - 2, rc[1] - 2, rc[2] + 2, rc[3] + 2], `fill="#3a3550"`));
  out.push(rectEl(rc, `fill="#26232f" stroke="#8a84a8" stroke-width="1"`));
}
for (const d of plan.doors || []) {
  if (!d.between.some(inRoom)) continue;
  out.push(rectEl(d.rect, `fill="#26232f" stroke="#cfc6ff" stroke-dasharray="3,2" stroke-width="1"`));
}
// surfaces (ice) and hazards
for (const s of plan.surfaces || []) if (inRoom(s.room)) out.push(rectEl(s.rect, `fill="#9fd8ff" opacity="${s.kind === "ice" ? 0.28 : 0.2}"`));
for (const r of rooms) if (r.hazard) out.push(rectEl(r.hazard.rect, `fill="url(#hatch)" stroke="${COL.orange}" stroke-width="1" opacity="0.9"`));
// pits and lava
for (const p of plan.pits || []) {
  if (!inRoom(p.zone)) continue;
  if (p.lava) {
    out.push(rectEl(p.rect, `fill="${LAVA}" opacity="0.85" stroke="#ffd36a" stroke-width="2"`));
    out.push(text((p.rect[0] + p.rect[2]) / 2, (p.rect[1] + p.rect[3]) / 2, `LAVA ${p.floorY}`, 11, "#fff3c0", `text-anchor="middle" dominant-baseline="middle"`));
  } else {
    out.push(rectEl(p.rect, `fill="${VOID}" stroke="#666" stroke-width="1"`));
    out.push(text((p.rect[0] + p.rect[2]) / 2, (p.rect[1] + p.rect[3]) / 2, `VOID ${p.floorY}`, 11, "#bbb", `text-anchor="middle" dominant-baseline="middle"`));
  }
}
// boxes (sorted low to high so a high one draws on top)
const boxes = (plan.boxes || []).filter((b) => inRoom(b.zone)).slice().sort((a, b) => a.max[1] - b.max[1]);
for (const b of boxes) {
  const rc = [b.min[0], b.min[2], b.max[0], b.max[2]];
  const top = b.max[1];
  out.push(rectEl(rc, `fill="${shade(top)}" stroke="#0c0b10" stroke-width="1" opacity="0.95"`));
  const w = (rc[2] - rc[0]) * S, h = (rc[3] - rc[1]) * S;
  if (w > 18 && h > 10) out.push(text((rc[0] + rc[2]) / 2, (rc[1] + rc[3]) / 2, `${b.kind[0]} ${+top.toFixed(2)}`, Math.min(11, Math.max(8, h * 0.6)), "#0c0b10", `text-anchor="middle" dominant-baseline="middle"`));
}
// ramps: a band of their width along from->to, an arrow up-slope
for (const r of plan.ramps || []) {
  if (!inRoom(r.zone)) continue;
  const [fx, fz] = r.from, [tx, tz] = r.to, hw = r.width / 2;
  const alongZ = Math.abs(tz - fz) >= Math.abs(tx - fx);
  const rc = alongZ ? [fx - hw, Math.min(fz, tz), fx + hw, Math.max(fz, tz)] : [Math.min(fx, tx), fz - hw, Math.max(fx, tx), fz + hw];
  out.push(rectEl(rc, `fill="#6d6250" stroke="#ffe08a" stroke-width="1" opacity="0.9"`));
  out.push(`<line x1="${X(fx)}" y1="${Y(fz)}" x2="${X(tx)}" y2="${Y(tz)}" stroke="#ffe08a" stroke-width="2" marker-end="url(#arr)"/>`);
  out.push(text((fx + tx) / 2 + hw + 0.3, (fz + tz) / 2, `ramp ${r.rise}`, 9, "#ffe08a"));
}
// tunnels
for (const t of plan.tunnels || []) if (inRoom(t.zone)) out.push(rectEl(t.rect, `fill="none" stroke="#5cff8a" stroke-width="2" stroke-dasharray="5,3"`));
// props
for (const p of plan.props || []) {
  if (!inRoom(p.room)) continue;
  const [px, py, pz] = p.at, [w, h, d] = p.size;
  const rc = [px - w / 2, pz - d / 2, px + w / 2, pz + d / 2];
  if (p.pickup) {
    const cx = X(px), cy = Y(pz);
    out.push(`<path d="M${cx},${(+cy - 6).toFixed(1)} L${(+cx + 6).toFixed(1)},${cy} L${cx},${(+cy + 6).toFixed(1)} L${(+cx - 6).toFixed(1)},${cy} z" fill="${p.weapon ? COL.cyan : COL.green}" stroke="#fff" stroke-width="1"/>`);
    out.push(text(px + 0.8, pz, p.weapon || (p.kind || "pickup").replace("Pickup", ""), 9, "#fff"));
  } else if (p.collider) {
    out.push(rectEl(rc, `fill="${shade(py + h / 2)}" stroke="#ffffff" stroke-width="1"`));
    if (w * S > 16) out.push(text(px, pz, `${p.kind[0]}${+(py + h / 2).toFixed(1)}`, 8, "#0c0b10", `text-anchor="middle" dominant-baseline="middle"`));
  } else {
    out.push(rectEl(rc, `fill="${COL[p.color] || "#888"}" opacity="0.35" stroke="none"`));
  }
}
// lights
for (const l of plan.lights || []) {
  if (!inRoom(l.room)) continue;
  const c = COL[l.color] || "#fff";
  const [lx, , lz] = l.at;
  if (l.kind === "pool") out.push(`<circle cx="${X(lx)}" cy="${Y(lz)}" r="${(l.size[0] / 2 * S).toFixed(1)}" fill="${c}" opacity="0.18"/>`);
  out.push(`<circle cx="${X(lx)}" cy="${Y(lz)}" r="3" fill="${c}"/>`);
}
// NIGHT31 (builder L): the arena rows of item 7 (DESIGN31 D): the Lantern ring and its sinking platforms, the Wyrm arena's boxes and its crumbling
// platforms (dashed red-orange = goes at half health), the Loom deck's breakable middle (hatched red-orange over the lava it opens)
const poly = (pts, attrs) => `<polygon points="${pts.map(([x, z]) => `${X(x)},${Y(z)}`).join(" ")}" ${attrs}/>`;
for (const r of rooms) {
  if (r.sink31 && r.sink31.draw) {
    const d = r.sink31.draw;
    out.push(poly(d.ring, `fill="#1d1a26" fill-opacity="0.55" stroke="${COL.white}" stroke-width="2" stroke-opacity="0.8"`));
    for (const p of d.platforms) {
      out.push(poly(p.poly, `fill="${shade(r.sink31.topFrom)}" stroke="${COL.orange}" stroke-width="2" stroke-dasharray="5,3"`));
      const c = p.poly.reduce((a, q) => [a[0] + q[0] / 4, a[1] + q[1] / 4], [0, 0]);
      out.push(text(c[0], c[1], `P${p.panel} ${r.sink31.topFrom}->${r.sink31.topTo}`, 9, "#fff3c0", `text-anchor="middle" dominant-baseline="middle"`));
    }
    out.push(text(d.ring[0][0] - 2, d.ring[0][1] + 2.5, "LANTERN RING (arena)", 10, COL.white, `text-anchor="end"`));
  }
  if (r.crumble31 && r.crumble31.draw) {
    const d = r.crumble31.draw;
    out.push(rectEl([d.center[0] - d.half, d.center[1] - d.half, d.center[0] + d.half, d.center[1] + d.half], `fill="#1d1a26" fill-opacity="0.45" stroke="${COL.white}" stroke-width="2" stroke-opacity="0.8"`));
    for (const b of d.boxes) {
      out.push(rectEl(b.rect, `fill="${shade(b.top)}" stroke="${b.crumble ? COL.orange : "#0c0b10"}" stroke-width="${b.crumble ? 2 : 1}" ${b.crumble ? `stroke-dasharray="5,3"` : ""}`));
      if (b.kind === "platform") out.push(text((b.rect[0] + b.rect[2]) / 2, (b.rect[1] + b.rect[3]) / 2, `#${b.index} top ${b.top}${b.crumble ? " CRUMBLES" : ""}`, 9, b.crumble ? "#fff3c0" : "#0c0b10", `text-anchor="middle" dominant-baseline="middle"`));
    }
    out.push(text(d.center[0] - d.half + 0.5, d.center[1] - d.half + 1.5, "WYRM ARENA (wyrm-arena.json)", 10, COL.white));
  }
  if (r.deckBreak31) {
    out.push(rectEl(r.deckBreak31.rect, `fill="${LAVA}" fill-opacity="0.35" stroke="${COL.orange}" stroke-width="2" stroke-dasharray="5,3"`));
    out.push(rectEl(r.deckBreak31.rect, `fill="url(#hatch)" stroke="none"`));
    const rc = r.deckBreak31.rect;
    out.push(text((rc[0] + rc[2]) / 2, (rc[1] + rc[3]) / 2, `BREAKS AT HALF -> LAVA ${r.deckBreak31.lavaY}`, 10, "#fff3c0", `text-anchor="middle" dominant-baseline="middle"`));
  }
}
// nodes, high nodes, grapple nodes, spawn, exit, proofs
for (const r of rooms) {
  for (const n of r.nodes || []) out.push(`<circle cx="${X(n[0])}" cy="${Y(n[2])}" r="2.5" fill="${COL.orange}"/>`);
  for (const n of r.highNodes || []) out.push(`<rect x="${(+X(n[0]) - 3).toFixed(1)}" y="${(+Y(n[2]) - 3).toFixed(1)}" width="6" height="6" fill="none" stroke="${COL.orange}" stroke-width="1.5"/>`);
  for (const n of r.grappleNodes || []) { out.push(`<circle cx="${X(n[0])}" cy="${Y(n[2])}" r="6" fill="none" stroke="${COL.cyan}" stroke-width="2"/>`); out.push(text(n[0] + 0.8, n[2], `g${n[1]}`, 9, COL.cyan)); }
  for (const p of r.proofs || []) {
    const pts = p.kind === "walk" ? p.points : p.kind === "fall" ? [p.from, p.to] : p.from ? [p.from, [p.from[0], 0, p.from[2] - (p.gap || 3)]] : [];
    if (pts.length > 1) out.push(`<polyline points="${pts.map((q) => `${X(q[0])},${Y(q[2])}`).join(" ")}" fill="none" stroke="#ffffff" stroke-width="1" stroke-dasharray="2,3" opacity="0.6"/>`);
  }
  if (r.spawn) { const [sx, , sz] = r.spawn.at; out.push(`<path d="M${X(sx)},${(+Y(sz) - 7).toFixed(1)} L${(+X(sx) + 5).toFixed(1)},${(+Y(sz) + 4).toFixed(1)} L${(+X(sx) - 5).toFixed(1)},${(+Y(sz) + 4).toFixed(1)} z" fill="${COL.cyan}"/>`); }
  if (r.exit) out.push(`<circle cx="${X(r.exit[0])}" cy="${Y(r.exit[2])}" r="5" fill="none" stroke="#ffffff" stroke-width="2"/>`);
  const waves = (r.waves || []).map((w) => {
    const c = {}; w.forEach((k) => { c[k] = (c[k] || 0) + 1; });
    return Object.entries(c).map(([k, n]) => `${n}${k[0]}`).join("+");
  }).join(" / ");
  out.push(text(r.rect[0] + 0.4, r.rect[1] + 1.4, `${r.id} ${r.name || ""}`, 12, "#ffffff", `font-weight="bold"`));
  out.push(text(r.rect[0] + 0.4, r.rect[1] + 2.9, `${r.rect[2] - r.rect[0]}x${+(r.rect[3] - r.rect[1]).toFixed(1)} h${r.height}${waves ? "  " + waves : ""}`, 10, "#cfc6ff"));
  if (r.lessons && r.lessons[0] && r.lessons[0].prompt) out.push(text(r.rect[0] + 0.4, r.rect[3] - 0.5, `"${r.lessons[0].prompt}"`, 9, "#9fe9ff"));
}
// legend
const lg = [["lava", LAVA], ["void pit", VOID], ["hazard", COL.orange], ["ice", "#9fd8ff"], ["ramp (arrow up)", "#6d6250"], ["box: kind top-y", shade(4)], ["spawn", COL.cyan], ["exit", "#ffffff"], ["node / high node", COL.orange], ["pickup", COL.green], ["grapple node", COL.cyan]];
const lx0 = W - LEG + 8;
out.push(`<text x="${lx0}" y="18" font-size="12" fill="#fff" font-family="Consolas,monospace" font-weight="bold">${esc(plan.id)}</text>`);
lg.forEach(([k, c], i) => { out.push(`<rect x="${lx0}" y="${30 + i * 16}" width="12" height="10" fill="${c}" stroke="#888"/>`); out.push(`<text x="${lx0 + 16}" y="${39 + i * 16}" font-size="10" fill="#ddd" font-family="Consolas,monospace">${esc(k)}</text>`); });
out.push(`<text x="${lx0}" y="${44 + lg.length * 16}" font-size="10" fill="#aaa" font-family="Consolas,monospace">north up, ${S} px/m</text>`);
out.push("</svg>");
fs.writeFileSync(outFile, out.join("\n") + "\n");
console.log("wrote", outFile, `${W}x${H}`, rooms.length, "rooms");
