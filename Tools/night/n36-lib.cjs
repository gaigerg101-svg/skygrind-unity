// NIGHT36 builder L (NIGHT36.md L1, PLAN.md section 3): the low-level emit helpers for the n36 levels (gen-n36-*.cjs). It requires NO older
// generator or lib: the plan-row shapes below are copied from Tools/night/n30-lib.cjs (Room / Level: boxes, ramps, pits, lava, tunnels, lights,
// props, nodes, proofs; Design/levels/LEVEL-FORMAT.md) and changed in one way: EVERY SOLID CARRIES `job` AND `why` (PLAN.md 3.3). A solid
// emitter without a job from JOBS, or with an empty why, throws. Tools/night/n36-jobs.cjs checks the written plan again.
// A level is a line of rooms going north (-z); each room sits 4 m (two wall thicknesses) beyond the one before, the door is that 4 m gap.
// Room-local coordinates: lx = metres east of the room's centre line, lz = metres NORTH of its south edge; rows are in web coordinates.
// The metrics kit (MOVE) holds the movement numbers rooms are spaced from, each with its source.
const fs = require("fs");
const path = require("path");

const WALL = 2;
const r3 = (v) => Math.round(v * 1000) / 1000;
const JOBS = ["line", "cover", "climb", "hazard", "marker", "reward"];

/// The movement numbers the rooms are spaced from (read, not typed, where a file has them).
const MOVE = (() => {
  const cfg = JSON.parse(fs.readFileSync(path.join(__dirname, "../../PortExport/config.json"), "utf8"));
  let ride = null;
  try { ride = JSON.parse(fs.readFileSync(path.join(__dirname, "../../Design/levels/n35-ride-distances.json"), "utf8")); } catch (e) { ride = null; }
  const m = {
    walk: cfg.ground.walkSpeed, jumpH: cfg.jump.height, dashM: cfg.dash.speed * cfg.dash.duration, slideH: cfg.slide.colliderHeight,
    rideSpeed: ride ? ride.rideSpeed : 10.45, jumpOut0: ride ? ride.jumpOff.pitch0.outAtTakeoffHeight : 4.84,
    faceGaps: ride ? ride.facingGapsThatWork : [3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
    src: "PortExport/config.json (ground.walkSpeed, jump.height, dash.speed x dash.duration, slide.colliderHeight) + Design/levels/n35-ride-distances.json (rideSpeed, jumpOff.pitch0, facingGapsThatWork)",
  };
  m.panel = r3(Math.round(m.rideSpeed * 1.0 * 2) / 2); // one second of held ride
  return m;
})();

function need(job, why, what) {
  if (!JOBS.includes(job)) throw new Error(`n36-lib: ${what}: job "${job}" is not one of ${JOBS.join(", ")}`);
  if (!why || String(why).trim().length < 8) throw new Error(`n36-lib: ${what}: a solid needs a why`);
}

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
    this.row = { id: s.id, name: s.name, kind: s.kind || "room", rect: [this.x0, r3(this.z0), this.x1, r3(this.z1)], height: this.h, roof: true, par: s.par || 10 };
    if (s.job) this.row.job36 = s.job;
    if (s.verb) this.row.verb36 = s.verb;
    this.boxesOut = []; this.rampsOut = []; this.pitsOut = []; this.tunnelsOut = []; this.lightsOut = []; this.propsOut = [];
    this.nodes = []; this.highNodes = [];
    this.n = 0;
  }
  X(lx) { return r3(this.cx + lx); }
  Z(lz) { return r3(this.z1 - lz); }
  pt(lx, y, lz) { return [this.X(lx), r3(y), this.Z(lz)]; }
  /// a local rect -> a web rect [x0, z0, x1, z1] (min, max)
  rect(lx0, lz0, lx1, lz1) { return [this.X(Math.min(lx0, lx1)), this.Z(Math.max(lz0, lz1)), this.X(Math.max(lx0, lx1)), this.Z(Math.min(lz0, lz1))]; }

  // ---- solids (every one: job + why)
  box(kind, lx0, lz0, lx1, lz1, y0, y1, job, why) {
    need(job, why, `${this.id} box`);
    const b = { zone: this.id, kind, min: [this.X(Math.min(lx0, lx1)), r3(y0), this.Z(Math.max(lz0, lz1))], max: [this.X(Math.max(lx0, lx1)), r3(y1), this.Z(Math.min(lz0, lz1))], job, why, note: why };
    this.boxesOut.push(b);
    return b;
  }
  /// a freestanding WALL-RIDE panel (kind "wall": the ride-wall trim view marks it cyan): along z from lz0 to lz1, `face` the ridden side ("E" or "W"),
  /// the ridden plane at lx `plane`, thickness t, from y0 (under a lava surface when it stands in lava) to y1
  panel(face, plane, lz0, lz1, y0, y1, why, t = 0.6) {
    const a = face === "E" ? plane - t : plane, b = face === "E" ? plane : plane + t;
    const p = this.box("wall", a, lz0, b, lz1, y0, y1, "line", why);
    p.ride = true; // builder M's RideTrim36 marks it cyan (the colour rule: cyan = player-usable)
    p.ride36 = { face, planeX: this.X(plane), lz: [lz0, lz1] };
    return p;
  }
  ramp(lx, lzLow, lzHigh, width, rise, job, why) {
    need(job, why, `${this.id} ramp`);
    this.rampsOut.push({ zone: this.id, from: [this.X(lx), this.Z(lzLow)], to: [this.X(lx), this.Z(lzHigh)], width, rise, job, why, note: why });
  }
  /// a dry pit with its own floor `floorY` (a fall costs a climb, never health); its recovery stair is the caller's boxes
  pit(lx0, lz0, lx1, lz1, floorY, why) {
    need("hazard", why, `${this.id} pit`);
    this.pitsOut.push({ zone: this.id, rect: this.rect(lx0, lz0, lx1, lz1), floorY, job: "hazard", why, note: why });
  }
  /// LAVA (LEVEL-FORMAT.md): the surface `depth` m under the floor; it burns the player (knock up, no teleport) and kills an enemy (Core/Lava30.cs)
  lava(lx0, lz0, lx1, lz1, depth, why) {
    need("hazard", why, `${this.id} lava`);
    const p = { zone: this.id, rect: this.rect(lx0, lz0, lx1, lz1), floorY: -depth, lava: true, job: "hazard", why, note: why };
    this.pitsOut.push(p);
    return p;
  }
  /// a slide slot (the tunnel kit): a ceiling `ceiling` m over the rect (levelgen adds the 2 m ceiling slab); its side walls are the caller's boxes
  tunnel(lx0, lz0, lx1, lz1, ceiling, why) {
    need("line", why, `${this.id} tunnel`);
    this.tunnelsOut.push({ zone: this.id, rect: this.rect(lx0, lz0, lx1, lz1), ceiling, job: "line", why, note: why });
  }
  /// a solid prop standing on the floor (or on a top at baseY): becomes a hull box in levelgen
  solid(kind, lx, lz, w, hgt, d, job, why, baseY = 0) {
    need(job, why, `${this.id} solid ${kind}`);
    const id = `${this.id}-P${this.propsOut.length + 1}`;
    this.propsOut.push({ id, room: this.id, kind, at: [this.X(lx), r3(baseY + hgt / 2), this.Z(lz)], size: [w, hgt, d], yawDeg: 0, collider: true, job, why });
  }
  pickup(kind, lx, lz, y, why) {
    const id = `${this.id}-P${this.propsOut.length + 1}`;
    this.propsOut.push({ id, room: this.id, kind, at: [this.X(lx), r3(y), this.Z(lz)], size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true, job: "reward", why });
  }
  /// a static (or strafing) stand-in target (Campaign24 standIns / standIns31: dies to shots, no attack)
  standIn(lx, y, lz, why) {
    for (const k of ["standIns", "standIns31"]) (this.row[k] = this.row[k] || []).push({ at: this.pt(lx, y, lz), mode: "static" });
    (this.row.standIns36 = this.row.standIns36 || []).push({ at: this.pt(lx, y, lz), job: "marker", why });
  }

  // ---- enemies: spawn spots (ground / raised), each named for the beat sheet's "where enemies enter"
  nodeAt(lx, lz) { this.nodes.push([this.X(lx), 0, this.Z(lz)]); }
  highNodeAt(lx, lz, y) { this.highNodes.push([this.X(lx), r3(y), this.Z(lz)]); }

  // ---- teaching: Campaign24 lessons (lessons and lessons31 alike: design.n31.unlocks reads lessons31) and teach35 trigger rects
  lesson(l) {
    const row = Object.assign({ promptPad: l.prompt || "" }, l);
    if (row.rect) row.rect = this.rect(...row.rect);
    if (row.via) row.via = this.rect(...row.via);
    for (const k of ["lessons", "lessons31"]) (this.row[k] = this.row[k] || []).push(Object.assign({}, row));
  }
  teach(id, lx0, lz0, lx1, lz1, minY = -1) {
    const p = this.level.plan;
    (p.teach35 = p.teach35 || []).push({ id, room: this.id, rect: this.rect(lx0, lz0, lx1, lz1), minY: r3(minY) });
  }

  // ---- light sockets (n30-lib's shapes: flush to a surface, at most 0.1 m deep)
  light(kind, at, normal, size, color, range, intensity, extra) {
    this.n++;
    this.lightsOut.push(Object.assign({ id: `${this.id}-L${this.n}`, room: this.id, kind, at, normal, size, color, range, intensity }, extra || {}));
  }
  trim(side, lz, len, y, color, range = 7, intensity = 1.1, extra) {
    const d = 0.1;
    if (side === "W") this.light("wall", [r3(this.x0 + d / 2), y, this.Z(lz)], [1, 0, 0], [len, 0.2, d], color, range, intensity, extra);
    else this.light("wall", [r3(this.x1 - d / 2), y, this.Z(lz)], [-1, 0, 0], [len, 0.2, d], color, range, intensity, extra);
  }
  ceil(lx, lz, w, d2, color, range = 9, intensity = 1.2, extra) {
    const d = 0.1;
    this.light("ceiling", [this.X(lx), r3(this.h - d / 2), this.Z(lz)], [0, -1, 0], [w, d2, d], color, range, intensity, extra);
  }
  pool(lx, lz, size, color, range = 6, intensity = 1.4, extra, topY = 0) {
    this.light("pool", [this.X(lx), r3(topY + 0.025), this.Z(lz)], [0, 1, 0], [size, size, 0.05], color, range, intensity, extra);
  }

  // ---- the reel's viewpoints (FeelReel.N36.Level.cs: the entry frame is the room's spawn; these are the middle and exit frames the lead looks at):
  // each a stand point [lx, y, lz] and a LOOK-AT point [lx, y, lz]; written as [x, y, z, yawDeg, pitchRad] (yaw 0 faces north, +yaw turns west, as the
  // spawn's and FeelReel's YawTo33; pitch up positive, from the eye 1.6 m over the feet to the look-at point)
  frames(mid, midAt, exit, exitAt) {
    const one = (p, q) => {
      const a = this.pt(p[0], p[1], p[2]), b = this.pt(q[0], q[1], q[2]);
      const dx = b[0] - a[0], dz = b[2] - a[2];
      const yaw = Math.atan2(-dx, -dz) * 180 / Math.PI;
      const pitch = Math.max(-0.4, Math.min(0.4, Math.atan2(b[1] - (a[1] + 1.6), Math.hypot(dx, dz))));
      return [...a, r3(yaw), r3(pitch)];
    };
    this.row.frames36 = { mid: one(mid, midAt), exit: one(exit, exitAt) };
  }

  // ---- completability proofs (LevelValidate29.Proofs: the real Mover walks them in the level report)
  proof(p) { (this.row.proofs = this.row.proofs || []).push(p); }
  walk(pts, extra) { this.proof(Object.assign({ kind: "walk", points: pts.map((q) => this.pt(q[0], q[1], q[2])) }, extra || {})); }
  walkSE(via, extra) {
    const mid = (via || []).map((q) => this.pt(q[0], q[1], q[2]));
    this.proof(Object.assign({ kind: "walk", points: [this.row.spawn.at, ...mid, this.row.exit] }, extra || {}));
  }
}

class Level {
  constructor(id, name, why, extra) {
    // n29 / n30: levelgen's lights, props, boxes and lava rows ride into the map JSON's campaign block (LevelPlan29.Wanted); n36: this file's flag
    this.plan = Object.assign({ _why: why, id, name, n29: true, n30: true, n36: true, floorSize: 0, killPlaneY: -30, wallHeight: 14 }, extra || {});
    this.rooms = [];
    this.doors = [];
    this.cursor = 0;
    this.prev = null;
    this.extraBoxes = []; // solids between rooms (a hall's sides): job + why, as every box
  }
  room(s) {
    const z1 = this.prev ? this.prev.z0 - (s.gap !== undefined ? s.gap : 2 * WALL) : this.cursor; // s.gap: a longer door (a walk-in hall)
    const spawnX = this.prev ? this.prev.outX : (s.cx || 0);
    const r = new Room(this, s, z1, spawnX);
    if (this.prev) {
      const p = this.prev;
      const dw = Math.min(s.doorW || p.s.outDoorW || 4, p.w - 1, r.w - 1);
      const dx = p.outX;
      const dh = Math.min(4.5, p.h, r.h);
      this.doors.push({ id: `d${this.doors.length + 1}`, between: [p.id, r.id], rect: [r3(dx - dw / 2), r3(r.z1), r3(dx + dw / 2), r3(p.z0)], height: dh, checkpoint: true });
    }
    r.row.spawn = { at: [r.spawnX, 0, r3(r.z1 - (s.spawnLz !== undefined ? s.spawnLz : 2))], yawDeg: 0 };
    const ex = s.exitX !== undefined ? s.exitX : r.outX;
    r.row.exit = [ex, s.exitY || 0, r3(r.z0 + (s.exitFromN !== undefined ? s.exitFromN : 2))];
    this.rooms.push(r);
    this.prev = r;
    return r;
  }
  finish() {
    const p = this.plan;
    p.rooms = []; p.doors = this.doors; p.boxes = []; p.ramps = []; p.pads = []; p.grapple = []; p.pits = []; p.tunnels = []; p.lights = []; p.props = [];
    let maxAbs = 0;
    for (const r of this.rooms) {
      if (r.nodes.length) r.row.nodes = r.nodes;
      if (r.highNodes.length) r.row.highNodes = r.highNodes;
      p.rooms.push(r.row);
      p.boxes.push(...r.boxesOut); p.ramps.push(...r.rampsOut); p.pits.push(...r.pitsOut); p.tunnels.push(...r.tunnelsOut);
      p.lights.push(...r.lightsOut); p.props.push(...r.propsOut);
      maxAbs = Math.max(maxAbs, Math.abs(r.z0) + 4, Math.abs(r.z1) + 4, Math.abs(r.x0) + 4, Math.abs(r.x1) + 4);
    }
    for (const b of this.extraBoxes) need(b.job, b.why, "extra box");
    p.boxes.push(...this.extraBoxes);
    p.floorSize = Math.ceil((maxAbs * 2 + 16) / 2) * 2; // EVEN: nav cell centres on half metres, never on a whole-metre box edge (n30-lib's rule)
    return p;
  }
  write(file) {
    const p = this.finish();
    fs.mkdirSync(path.dirname(file), { recursive: true });
    fs.writeFileSync(file, JSON.stringify(p, null, 1) + "\n");
    console.log("wrote", file, p.rooms.length, "rooms", p.boxes.length, "boxes", p.ramps.length, "ramps", p.pits.length, "pits", p.lights.length, "lights", p.props.length, "props");
  }
}

module.exports = { Level, Room, WALL, JOBS, MOVE, r3 };
