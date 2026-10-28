// NIGHT29 (builder R): the room-building helpers shared by gen-n29-foundry.cjs, gen-n29-iceline.cjs and gen-n29-kiln.cjs.
// A level is a line of rooms going north (-z); each room is placed 4 m (two wall thicknesses) beyond the one before, and the door between them
// is exactly that 4 m gap (doorFit). Room-local coordinates: lx = metres east of the room's centre line, lz = metres NORTH of its south edge.
const fs = require("fs");
const path = require("path");

const WALL = 2;
const r3 = (v) => Math.round(v * 1000) / 1000;

class Room {
  constructor(level, s, z1, spawnX) {
    this.level = level;
    this.id = s.id;
    this.s = s;
    this.w = s.w; this.l = s.l; this.h = s.h;
    this.cx = s.cx || 0;
    this.x0 = this.cx - this.w / 2; this.x1 = this.cx + this.w / 2;
    this.z1 = z1; this.z0 = z1 - this.l;
    this.outX = s.outX !== undefined ? s.outX : this.cx;
    this.spawnX = spawnX;
    this.row = {
      id: s.id, name: s.name, kind: s.kind || "room",
      rect: [this.x0, r3(this.z0), this.x1, r3(this.z1)],
      height: this.h, roof: true, par: s.par || 10,
    };
    this.boxesOut = []; this.rampsOut = []; this.padsOut = []; this.pitsOut = []; this.tunnelsOut = []; this.surfacesOut = [];
    this.lightsOut = []; this.propsOut = [];
    this.nodes = []; this.highNodes = [];
    this.n = 0;
  }
  X(lx) { return r3(this.cx + lx); }
  Z(lz) { return r3(this.z1 - lz); }
  top() { return this.h; }
  pt(lx, y, lz) { return [this.X(lx), r3(y), this.Z(lz)]; }

  box(kind, lx0, lz0, lx1, lz1, y0, y1, note) {
    this.boxesOut.push({ zone: this.id, kind, min: [this.X(lx0), r3(y0), this.Z(lz1)], max: [this.X(lx1), r3(y1), this.Z(lz0)], note: note || "" });
  }
  ramp(lx, lzLow, lzHigh, width, rise, note) {
    this.rampsOut.push({ zone: this.id, from: [this.X(lx), this.Z(lzLow)], to: [this.X(lx), this.Z(lzHigh)], width, rise, note: note || "" });
  }
  pit(lx0, lz0, lx1, lz1, floorY, note) {
    this.pitsOut.push({ zone: this.id, rect: [this.X(lx0), this.Z(lz1), this.X(lx1), this.Z(lz0)], floorY, note: note || "" });
  }
  // the recovery stair of a pit: two steps against the pit's south edge, on the side (+1 east / -1 west), each at most 1.3 m
  stair(lxEdge, side, lzSouth, floorY) {
    // lxEdge: the room's wall-side x (local) the stair starts at; steps run toward the middle, 2.5 m wide each, 2 m deep (north of lzSouth)
    const dir = side;
    const a = lxEdge, b = lxEdge - dir * 4, c = lxEdge - dir * 8;
    const rise1 = floorY + 1.15, rise2 = floorY + 2.3;
    const lo = (p, q) => [Math.min(p, q), Math.max(p, q)];
    let [p0, p1] = lo(a, b);
    this.box("deck", p0, lzSouth, p1, lzSouth + 2, floorY - 1, rise1, "recovery stair, step 1 (from the pit floor)");
    [p0, p1] = lo(b, c);
    this.box("deck", p0, lzSouth, p1, lzSouth + 2, floorY - 1, rise2, "recovery stair, step 2 (then up to the floor)");
  }
  // ---- completability proofs (LevelValidate29.Proofs: the real Mover walks them in the level report)
  proof(p) { (this.row.proofs = this.row.proofs || []).push(p); }
  // a walk through local points [lx, y, lz]; the room's spawn and exit are added at the ends unless `bare`
  walk(pts, extra) {
    const w = pts.map((q) => this.pt(q[0], q[1], q[2]));
    this.proof(Object.assign({ kind: "walk", points: w }, extra || {}));
  }
  walkSE(via, extra) {
    const a = this.row.spawn.at, e = this.row.exit;
    const mid = (via || []).map((q) => this.pt(q[0], q[1], q[2]));
    this.proof(Object.assign({ kind: "walk", points: [a, ...mid, e] }, extra || {}));
  }
  fall(fromLx, fromLz, toLz) { this.proof({ kind: "fall", from: this.pt(fromLx, 0, fromLz), to: this.pt(fromLx, 0, toLz) }); }
  // a staircase of decks (walkers climb 0.5 m a metre): from lzLow (the low end, height y0) to lzHigh (height y1); every step is a solid column from the floor
  stairs(lx0, lx1, lzLow, lzHigh, y0, y1, note) {
    const n = Math.ceil((y1 - y0) / 0.5 - 1e-9);
    const dir = Math.sign(lzHigh - lzLow);
    const depth = Math.abs(lzHigh - lzLow) / n;
    for (let k = 1; k <= n; k++) {
      const a = lzLow + dir * depth * (k - 1), b = lzLow + dir * depth * k;
      this.box("deck", lx0, Math.min(a, b), lx1, Math.max(a, b), 0, r3(y0 + (y1 - y0) * k / n), `${note || "stairs"} step ${k}`);
    }
  }
  surface(kind, lx0, lz0, lx1, lz1, extra) {
    this.surfacesOut.push(Object.assign({ kind, rect: [this.X(lx0), this.Z(lz1), this.X(lx1), this.Z(lz0)], minY: -0.5, maxY: 6, room: this.id }, extra || {}));
  }
  nodeAt(lx, lz, y) { this.nodes.push([this.X(lx), 0, this.Z(lz)]); }
  highNodeAt(lx, lz, y) { this.highNodes.push([this.X(lx), r3(y), this.Z(lz)]); }

  // ---- light sockets (flush to a surface; at most 0.1 m deep)
  light(kind, at, normal, size, color, range, intensity, extra) {
    this.n++;
    this.lightsOut.push(Object.assign({ id: `${this.id}-L${this.n}`, room: this.id, kind, at, normal, size, color, range, intensity }, extra || {}));
  }
  // a strip on a side wall: side 'W' or 'E', centred lz, length len, height y
  trim(side, lz, len, y, color, range = 7, intensity = 1.1, extra) {
    const d = 0.1;
    if (side === "W") this.light("wall", [r3(this.x0 + d / 2), y, this.Z(lz)], [1, 0, 0], [len, 0.2, d], color, range, intensity, extra);
    else this.light("wall", [r3(this.x1 - d / 2), y, this.Z(lz)], [-1, 0, 0], [len, 0.2, d], color, range, intensity, extra);
  }
  // a strip on the north / south end wall at lx
  endTrim(side, lx, len, y, color, range = 7, intensity = 1.1, extra) {
    const d = 0.1;
    if (side === "N") this.light("wall", [this.X(lx), y, r3(this.z0 + d / 2)], [0, 0, 1], [len, 0.2, d], color, range, intensity, extra);
    else this.light("wall", [this.X(lx), y, r3(this.z1 - d / 2)], [0, 0, -1], [len, 0.2, d], color, range, intensity, extra);
  }
  ceil(lx, lz, w, d2, color, range = 9, intensity = 1.2, extra) {
    const d = 0.1;
    this.light("ceiling", [this.X(lx), r3(this.h - d / 2), this.Z(lz)], [0, -1, 0], [w, d2, d], color, range, intensity, extra);
  }
  pool(lx, lz, size, color, range = 6, intensity = 1.4, extra, topY = 0) {
    this.light("pool", [this.X(lx), r3(topY + 0.025), this.Z(lz)], [0, 1, 0], [size, size, 0.05], color, range, intensity, extra);
  }
  // ---- props (authored)
  prop(kind, at, size, collider, extra) {
    const id = `${this.id}-P${this.propsOut.length + 1}`;
    this.propsOut.push(Object.assign({ id, room: this.id, kind, at, size, yawDeg: 0, collider }, extra || {}));
  }
  // a solid crate / pillar / console standing on the floor (or a deck top at baseY)
  solid(kind, lx, lz, w, hgt, d, baseY = 0, yaw = 0) {
    this.prop(kind, [this.X(lx), r3(baseY + hgt / 2), this.Z(lz)], [w, hgt, d], true, { yawDeg: yaw });
  }
  // a flat decal on a surface (floor top at topY): thinner than the wall depth, no collider
  decal(kind, lx, lz, w, d, color, topY = 0, extra) {
    this.prop(kind, [this.X(lx), r3(topY + 0.025), this.Z(lz)], [w, 0.05, d], false, Object.assign({ color }, extra || {}));
  }
  // a cable / pipe flat on a side wall
  wallCable(side, lz, len, y, color) {
    const d = 0.1;
    const x = side === "W" ? this.x0 + d / 2 : this.x1 - d / 2;
    this.prop("cable", [r3(x), y, this.Z(lz)], [d, 0.1, len], false, { color });
  }
  pickup(kind, lx, lz, y, extra) {
    this.prop(kind, [this.X(lx), r3(y), this.Z(lz)], [0.6, 0.6, 0.6], false, Object.assign({ pickup: true }, extra || {}));
  }
}

class Level {
  constructor(id, name, why, extra) {
    this.plan = Object.assign({ _why: why, id, name, n29: true, floorSize: 0, killPlaneY: -30, wallHeight: 14 }, extra || {});
    this.rooms = [];
    this.doors = [];
    this.cursor = 0;
    this.prev = null;
  }
  room(s) {
    const z1 = this.prev ? this.prev.z0 - 2 * WALL : this.cursor;
    const spawnX = this.prev ? this.prev.outX : (s.cx || 0);
    const r = new Room(this, s, z1, spawnX);
    if (this.prev) {
      const p = this.prev;
      const dw = Math.min(s.doorW || p.s.outDoorW || 4, p.w - 1, r.w - 1);
      const dx = p.outX;
      const dh = Math.min(4.5, p.h, r.h);
      this.doors.push({ id: `d${this.doors.length + 1}`, between: [p.id, r.id], rect: [r3(dx - dw / 2), r3(r.z1 - 0), r3(dx + dw / 2), r3(p.z0)], height: dh, checkpoint: true });
      // the door rect runs from this room's rect edge (z1) to the previous room's rect edge (z0): exactly the 4 m gap
      this.doors[this.doors.length - 1].rect = [r3(dx - dw / 2), r3(r.z1), r3(dx + dw / 2), r3(p.z0)];
    }
    r.row.spawn = { at: [r.spawnX, 0, r3(r.z1 - (s.spawnLz !== undefined ? s.spawnLz : 2.5))], yawDeg: 0 };
    const ex = s.exitX !== undefined ? s.exitX : r.outX;
    r.row.exit = [ex, s.exitY || 0, r3(r.z0 + (s.exitFromN !== undefined ? s.exitFromN : 2.5))];
    this.rooms.push(r);
    this.prev = r;
    return r;
  }
  finish() {
    const p = this.plan;
    p.rooms = []; p.doors = this.doors; p.boxes = []; p.ramps = []; p.pads = []; p.grapple = []; p.pits = []; p.tunnels = []; p.surfaces = []; p.lights = []; p.props = [];
    let maxAbs = 0;
    for (const r of this.rooms) {
      if (r.nodes.length) r.row.nodes = r.nodes;
      if (r.highNodes.length) r.row.highNodes = r.highNodes;
      p.rooms.push(r.row);
      p.boxes.push(...r.boxesOut); p.ramps.push(...r.rampsOut); p.pads.push(...r.padsOut); p.pits.push(...r.pitsOut); p.tunnels.push(...r.tunnelsOut);
      p.surfaces.push(...r.surfacesOut); p.lights.push(...r.lightsOut); p.props.push(...r.propsOut);
      maxAbs = Math.max(maxAbs, Math.abs(r.z0) + 4, Math.abs(r.z1) + 4, Math.abs(r.x0) + 4, Math.abs(r.x1) + 4);
    }
    if (!p.surfaces.length) delete p.surfaces;
    p.floorSize = Math.ceil((maxAbs * 2 + 16) / 2) * 2; // EVEN: the nav grid then has cell centres on half metres, never on a box edge (whole-metre edges)
    return p;
  }
  write(file) {
    const p = this.finish();
    fs.mkdirSync(path.dirname(file), { recursive: true });
    fs.writeFileSync(file, JSON.stringify(p, null, 1) + "\n");
    console.log("wrote", file, p.rooms.length, "rooms", p.lights.length, "lights", p.props.length, "props");
  }
}

module.exports = { Level, Room, WALL, r3 };
