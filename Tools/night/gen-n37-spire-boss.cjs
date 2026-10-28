// NIGHT37 builder C (NIGHT37.md C1; Research/2026-10-12-wave37/BOSSES.md "L5 THE CROWN"): LEVEL 5 with THE CROWN's descent.
// Writes Design/levels/n37/spire.plan.json:
//   - rooms v1-v8 of level 5 COPIED AS THEY ARE from the plan the game's n33-spire was built from (Design/levels/n33/spire.plan.json, read as
//     json: no older generator is required). Their solids get job + why tags (n36-jobs) naming them as copies; nothing else changes.
//   - two NEW rehearsal rooms (BOSSES.md rule 9, DECIDE37 D-4): v9 FALLING STONES (jump-dash across platforms that drop 2 s after you land)
//     and v10 THE RING (a spinning hoop over a gap: jump-dash through it).
//   - v11 THE SPIRE (replaces the n33 crown room v9): a 68 x 68 m chasm (a pit 74 m deep), the spire's core (8 x 8 m, 60 m tall) on its
//     centre line, the walk-in bridge, the leg checkpoints, ride panels on the core's faces, and the room row `spire37`: the moving and
//     dropping pieces the sim builds as its own world layer (Core/Orbit37.cs): the summit (shattered at the start), the orbiting ring
//     platforms (leg 1), the gallery's broken spiral of slabs (leg 2), the plaza's outer sectors (leg 3). Numbers (speeds, timings) are
//     design.n37.crown37's; this file holds only the places.
// Web coordinates (x east, y up, z south; a rect is [x0, z0, x1, z1]). Run: node Tools/night/gen-n37-spire-boss.cjs, then
// bash Tools/night/n37c-regen.sh (levelgen, the checker, the copies).
// NIGHT38 builder C2: rooms v9-v11 are built by addBossRooms(plan) (exported), so Tools/night/gen-n38-spire.cjs (level 5 rebuilt for flight, n38-spire)
// puts the very same three rooms after its new v1-v6. Run as a script, this file writes n37-spire exactly as before (the plan's bytes unchanged).
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const SRC = path.join(ROOT, "Design/levels/n33/spire.plan.json");
const OUT = path.join(ROOT, "Design/levels/n37/spire.plan.json");
const r3 = (v) => Math.round(v * 1000) / 1000;

/// n37-spire's plan: the copied rooms v1-v8 (no v9-v11 yet)
function n37Base() {
  const src = JSON.parse(fs.readFileSync(SRC, "utf8"));
  const KEEP = new Set(["v1", "v2", "v3", "v4", "v5", "v6", "v7", "v8"]);
  const plan = JSON.parse(JSON.stringify(src));
  plan._why = "NIGHT37 builder C (design.n37.crown37 swaps n33-spire for this file while the row is on): Tools/night/gen-n37-spire-boss.cjs. Rooms v1-v8 are "
    + "level 5's rooms copied as they are from Design/levels/n33/spire.plan.json; v9 FALLING STONES and v10 THE RING rehearse the jump-dash (BOSSES.md rule 9); "
    + "v11 THE SPIRE is THE CROWN's descent (BOSSES.md 'L5 THE CROWN'). Source of the copied rooms: " + (src._why || "").slice(0, 200);
  plan.id = "n37-spire";
  plan.name = "LEVEL 5 THE SPIRE";
  plan.killPlaneY = -95; // the chasm is 74 m deep: the descent's own fall rule (a leg's floor) catches a fall long before this
  plan.rooms = src.rooms.filter((r) => KEEP.has(r.id));
  plan.doors = src.doors.filter((d) => KEEP.has(d.between[0]) && KEEP.has(d.between[1]) || d.id === "d8");
  const keepZone = (o) => KEEP.has(o.zone || o.room);
  const COPY = "level 5 as it was (n33-spire, copied unchanged): ";
  plan.boxes = src.boxes.filter(keepZone).map((b) => Object.assign({}, b, { job: b.job || (b.kind === "deck" ? "line" : "cover"), why: b.why || COPY + (b.note || b.kind) }));
  plan.ramps = src.ramps.filter(keepZone).map((b) => Object.assign({}, b, { job: b.job || "climb", why: b.why || COPY + (b.note || "a ramp") }));
  plan.pads = src.pads.filter(keepZone);
  plan.grapple = (src.grapple || []).filter(keepZone);
  plan.pits = src.pits.filter(keepZone).map((b) => Object.assign({}, b, { job: b.job || "hazard", why: b.why || COPY + (b.note || "a pit") }));
  plan.tunnels = (src.tunnels || []).filter(keepZone);
  plan.surfaces = (src.surfaces || []).filter(keepZone);
  plan.lights = src.lights.filter(keepZone);
  plan.props = src.props.filter(keepZone).map((p) => (p.collider ? Object.assign({}, p, { job: p.job || "cover", why: p.why || COPY + p.kind }) : p));
  return { plan, src };
}

// ------------------------------------------------------------------------------------------------------------------------------ helpers
let plan = null; // the plan addBossRooms writes into
let rooms = [];
let lightN = {};
function light(room, kind, at, normal, size, color, range, intensity, extra) {
  lightN[room] = (lightN[room] || 0) + 1;
  plan.lights.push(Object.assign({ id: `${room}-L${lightN[room]}`, room, kind, at: at.map(r3), normal, size, color, range, intensity }, extra || {}));
}
function box(zone, kind, x0, y0, z0, x1, y1, z1, job, why, extra) {
  if (!why || why.length < 8) throw new Error("a solid needs a why");
  const b = Object.assign({ zone, kind, min: [r3(Math.min(x0, x1)), r3(y0), r3(Math.min(z0, z1))], max: [r3(Math.max(x0, x1)), r3(y1), r3(Math.max(z0, z1))], job, why, note: why }, extra || {});
  plan.boxes.push(b);
  return b;
}
function pit(zone, rect, floorY, why) { plan.pits.push({ zone, rect, floorY, job: "hazard", why, note: why }); }
function frames(row, mid, midAt, exit, exitAt) {
  const one = (a, b) => {
    const dx = b[0] - a[0], dz = b[2] - a[2];
    const yaw = Math.atan2(-dx, -dz) * 180 / Math.PI;
    const pitch = Math.max(-0.6, Math.min(0.6, Math.atan2(b[1] - (a[1] + 1.6), Math.hypot(dx, dz))));
    return [...a.map(r3), r3(yaw), r3(pitch)];
  };
  row.frames36 = { mid: one(mid, midAt), exit: one(exit, exitAt) };
}
/// a recovery stair along the room's west wall: steps <= 1.3 m from a pit floor back to the floor at the pit's north edge (z1 = the edge)
function stair(zone, x0, x1, zEdge, floorY, why) {
  const tops = [floorY + 1.25, floorY + 2.5, floorY + 3.75];
  tops.forEach((t, i) => {
    if (t >= -0.05) return;
    const zN = zEdge - 4 * (2 - i), zS = zN - 4; // the last step touches the edge
    box(zone, "deck", x0, floorY - 1, zS, x1, t, zN, "climb", `${why}: step ${i + 1} (top ${r3(t)} m; each step <= 1.3 m: the way back after a missed jump)`);
  });
}

/// rooms v9-v11 (and doors d9, d10) into `p`; returns the three room rows (the caller pushes them after its own rooms)
function addBossRooms(p) {
plan = p;
rooms = [];
lightN = {};
// ------------------------------------------------------------------------------------------------------------------------------ v9 FALLING STONES
{
  const id = "v9";
  const row = {
    id, name: "5.9 FALLING STONES", kind: "link", rect: [-22, -211, 2, -170], height: 10, roof: true, par: 12,
    spawn: { at: [-10, 0, -171.5], yawDeg: 0 }, exit: [-10, 0, -209.5],
    job36: "teach", verb36: "jump-dash (dash, then jump inside the dash) across stones that drop 2 s after you land",
    lessons: [{ kind: "reached", rect: [-22, -211, 2, -208], minY: -0.5, prompt: "", promptPad: "" }],
    lessons31: [{ kind: "reached", rect: [-22, -211, 2, -208], minY: -0.5, prompt: "", promptPad: "" }],
    // the falling stones (Core/Orbit37.cs kind "fall": the top centre, the size; numbers design.n37.crown37.pieces.fall*). Gaps of 9 m: the
    // real sim's run jump from 8 m/s reaches 7.08 m on the flat, the jump-dash 21 m (Harness crown37 jumps)
    fall37: [
      { at: [-13, 0, -184], size: [4, 1, 4], why: "stone 1: 9 m past the entry edge (a run jump falls short at 7.1 m; a jump-dash clears it): it drops 2 s after you land" },
      { at: [-7, 0, -197], size: [4, 1, 4], why: "stone 2: 9 m on, 6 m east: the jump-dash's line bends, then 9 m to the exit floor" },
    ],
    proofs: [{ kind: "walk", points: [[-10, 0, -171.5], [-10, 0, -172.5]] }],
    // the crossing only the stones make (they are the sim's layer, not levelgen's): the route graph goes edge to edge
    routeVia: [[[-10, 0, -172.5], [-10, 0, -208.5]]],
  };
  pit(id, [-22, -208, 2, -173], -5, "the FALL: a 35 m pit (5 m deep) crossed only on the two falling stones; a miss costs the stair's climb, never health");
  stair(id, -22, -19, -173, -5, "v9 recovery stair");
  light(id, "pool", [-10, 0.025, -171.5], [0, 1, 0], [2.5, 2.5, 0.05], "cyan", 6, 1.4);
  light(id, "pool", [-10, 0.025, -209.5], [0, 1, 0], [2.5, 2.5, 0.05], "cyan", 7, 1.6);
  light(id, "wall", [-21.95, 1.6, -190], [1, 0, 0], [30, 0.2, 0.1], "amber", 8, 1.0);
  light(id, "wall", [1.95, 1.6, -190], [-1, 0, 0], [30, 0.2, 0.1], "white", 8, 1.0);
  frames(row, [-10, 0, -172], [-10, 0, -197], [-10, 0, -209], [-10, 0, -213]);
  rooms.push(row);
  plan.doors.push({ id: "d9", between: ["v9", "v10"], rect: [-12, -215, -8, -211], height: 4.5, checkpoint: true });
}

// ------------------------------------------------------------------------------------------------------------------------------ v10 THE RING
{
  const id = "v10";
  const row = {
    id, name: "5.10 THE RING", kind: "link", rect: [-22, -251, 2, -215], height: 10, roof: true, par: 10,
    spawn: { at: [-10, 0, -217], yawDeg: 0 }, exit: [-10, 0, -248],
    job36: "teach", verb36: "jump-dash THROUGH a spinning ring (the pass: a cyan flash), as the Crown's ring will be passed",
    lessons: [{ kind: "reached", rect: [-22, -237, 2, -231], minY: -0.5, prompt: "", promptPad: "" }],
    lessons31: [{ kind: "reached", rect: [-22, -237, 2, -231], minY: -0.5, prompt: "", promptPad: "" }],
    // the ring target (Core/Orbit37.cs Hoop37: the pass test the Crown uses; no collider): its centre, its facing (the normal), the radius of the pass
    hoop37: [{ at: [-10, 1.6, -226], normal: [0, 0, 1], why: "the RING over the 10 m gap: the jump-dash from the edge passes through its centre (a cyan flash); the Crown's damage rule rehearsed" }],
    proofs: [{ kind: "walk", points: [[-10, 0, -217], [-10, 0, -220]] }, { kind: "walk", points: [[-10, 0, -233], [-10, 0, -248]] }],
  };
  pit(id, [-22, -231, 2, -221], -5, "the RING's gap: 10 m (a run jump falls short; the jump-dash through the ring clears it); a miss costs the stair's climb");
  stair(id, -22, -19, -221, -5, "v10 recovery stair");
  light(id, "pool", [-10, 0.025, -218.5], [0, 1, 0], [2.5, 2.5, 0.05], "cyan", 6, 1.4);
  light(id, "pool", [-10, 0.025, -248], [0, 1, 0], [2.5, 2.5, 0.05], "cyan", 7, 1.6);
  light(id, "wall", [-21.95, 1.6, -233], [1, 0, 0], [30, 0.2, 0.1], "amber", 8, 1.0);
  light(id, "wall", [1.95, 1.6, -233], [-1, 0, 0], [30, 0.2, 0.1], "white", 8, 1.0);
  frames(row, [-10, 0, -219], [-10, 1.6, -226], [-10, 0, -247], [-10, 0, -252]);
  rooms.push(row);
  plan.doors.push({ id: "d10", between: ["v10", "v11"], rect: [-12, -255, -8, -251], height: 4.5, checkpoint: true });
}

// ------------------------------------------------------------------------------------------------------------------------------ v11 THE SPIRE
{
  const id = "v11";
  const AX = -10, AZ = -289; // the spire's centre line
  const CORE = 4; // the core's half width (8 x 8 m)
  const TOP = 0, PLAZA = -60;
  const row = {
    id, name: "5.11 THE SPIRE", kind: "boss", rect: [-44, -323, 24, -255], height: 12, roof: false, par: 40,
    spawn: { at: [-10, 0, -258], yawDeg: 0 }, exit: [-10, 0, -258], boss: "crown37",
    job36: "boss", verb36: "the descent: drop onto orbiting platforms, jump-dash through the Crown's ring, down the broken gallery to the plaza",
    proofs: [{ kind: "walk", points: [[-10, 0, -258], [-10, 0, -263]] }],
  };
  pit(id, [-44, -323, 24, -255], -74, "THE CHASM: the spire stands in a 74 m deep pit; there is no floor but the spire's pieces (a fall is caught by the leg's floor rule: back to its checkpoint, -20 HP)");
  // the static pieces (the level's solids)
  box(id, "deck", -12, -1, -265, -8, 0, -255, "line", "the WALK-IN bridge's stub (the door to the chasm): leg 1's checkpoint; the rest of the bridge is the summit's and shatters");
  box(id, "deck", AX - CORE, PLAZA, AZ - CORE, AX + CORE, TOP, AZ + CORE, "line", "the SPIRE's core: 8 x 8 m, 60 m tall from the plaza to the summit; every face rideable; the axis the fight descends");
  box(id, "deck", AX - 15, -28, AZ + 7, AX - 9, -27, AZ + 13, "line", "leg 2's CHECKPOINT: a broken buttress off the spire's SW (top -27), clear of the core: the gallery's first slabs lie in line from it (a fall in the gallery puts you back here)");
  box(id, "deck", AX - 10, PLAZA - 1, AZ - 10, AX + 10, PLAZA, AZ + 10, "line", "the PLAZA's inner square (20 x 20 m at -60): leg 3's floor and checkpoint; its outer ring is eight sectors that drop");
  // ride panels on the core's faces in the gallery band (-50..-29): cyan, the ride from slab to slab past a cut one
  const PT = 0.3, P0 = -50, P1 = -29;
  box(id, "wall", AX - CORE - PT, P0, AZ - 3, AX - CORE, P1, AZ + 3, "line", "ride panel, the core's west face: ride past a cut slab", { ride: true });
  box(id, "wall", AX + CORE, P0, AZ - 3, AX + CORE + PT, P1, AZ + 3, "line", "ride panel, the core's east face: ride past a cut slab", { ride: true });
  box(id, "wall", AX - 3, P0, AZ - CORE - PT, AX + 3, P1, AZ - CORE, "line", "ride panel, the core's north face: ride past a cut slab", { ride: true });
  box(id, "wall", AX - 3, P0, AZ + CORE, AX + 3, P1, AZ + CORE + PT, "line", "ride panel, the core's south face: ride past a cut slab", { ride: true });

  // the moving / dropping pieces (Core/Orbit37.cs): each {id, kind, leg, size [w, thick, d], at (top centre) | orbit {centre, radius, phase, dir}}
  const pieces = [];
  const piece = (o) => { pieces.push(o); return o; };
  // the summit: four slabs round the core's top and the bridge's far half (shattered by the Crown at the fight's start)
  piece({ id: "sumN", kind: "summit", leg: 1, at: [AX, TOP, AZ - 7], size: [20, 1, 6], why: "the summit's north slab" });
  piece({ id: "sumS", kind: "summit", leg: 1, at: [AX, TOP, AZ + 7], size: [20, 1, 6], why: "the summit's south slab (the bridge lands here)" });
  piece({ id: "sumW", kind: "summit", leg: 1, at: [AX - 7, TOP, AZ], size: [6, 1, 8], why: "the summit's west slab" });
  piece({ id: "sumE", kind: "summit", leg: 1, at: [AX + 7, TOP, AZ], size: [6, 1, 8], why: "the summit's east slab" });
  piece({ id: "bridge", kind: "summit", leg: 1, at: [AX, TOP, -272], size: [4, 1, 14], why: "the bridge's far half (z -279..-265): it falls with the summit" });
  // leg 1: the orbiting ring platforms (fragments of the summit), 3 heights x 4 (5 x 5 m; neighbours 18 m apart: a jump-dash), radius 13 m round the axis, alternate turns
  [[-8, 1, 0], [-15, -1, 1 / 8], [-22, 1, 1 / 4]].forEach(([y, dir, ph], h) => {
    for (let k = 0; k < 4; k++)
      piece({ id: `orb${h}${k}`, kind: "orbit", leg: 1, size: [5, 1, 5], orbit: { centre: [AX, y, AZ], radius: 13, phase: r3(ph + k / 4), dir },
        why: `leg 1 orbit ring ${h + 1} (top ${y}): a summit fragment circling the spire; stand on it and it carries you` });
  });
  // leg 2: the gallery: a broken square spiral of slabs 10 m out from the axis, 3 m down each quarter turn (corner gaps: 7 m + the drop)
  const sides = [[0, 10], [10, 0], [0, -10], [-10, 0]]; // S, E, N, W (dx, dz from the axis)
  for (let i = 0; i < 8; i++) {
    const [dx, dz] = sides[i % 4];
    const along = dz !== 0 ? [6, 1, 4] : [4, 1, 6];
    piece({ id: `gal${i}`, kind: "gallery", leg: 2, at: [AX + dx, -30 - 3 * i, AZ + dz], size: along,
      why: `leg 2 gallery slab ${i + 1} (top ${-30 - 3 * i}): the broken spiral; the Crown's beam drops the one you head for` });
  }
  // leg 3: the plaza's outer ring: eight sectors round the inner square (dropped one by one)
  const P = PLAZA;
  [["plN1", AX - 9, AZ - 14, 18, 8], ["plN2", AX + 9, AZ - 14, 18, 8], ["plS1", AX - 9, AZ + 14, 18, 8], ["plS2", AX + 9, AZ + 14, 18, 8],
    ["plW1", AX - 14, AZ - 5, 8, 10], ["plW2", AX - 14, AZ + 5, 8, 10], ["plE1", AX + 14, AZ - 5, 8, 10], ["plE2", AX + 14, AZ + 5, 8, 10]]
    .forEach(([pid, x, z, w, d]) => piece({ id: pid, kind: "plaza", leg: 3, at: [x, P, z], size: [w, 1, d], why: "leg 3 plaza sector: drops into the chasm one by one" }));

  row.spire37 = {
    axis: [AX, TOP, AZ], coreHalf: CORE,
    // the legs: each leg's checkpoint (feet, yaw) and its floor (feet under it = a fall: back to the checkpoint)
    checkpoints: [[-10, 0, -258, 0], [AX - 12, -27, AZ + 10, 270], [AX, PLAZA, AZ + 8, 0]],
    legFloors: [-26, -56, -66],
    pieces,
    _why: "builder C: the descent's places (Core/Orbit37.cs builds them as the world layer crown37Spire); timings and speeds are design.n37.crown37's",
  };
  // lights: the checkpoints cyan, amber trims up the core
  light(id, "pool", [-10, 0.025, -259], [0, 1, 0], [3, 3, 0.05], "cyan", 8, 1.6);
  light(id, "pool", [AX - 12, -26.975, AZ + 10], [0, 1, 0], [2.4, 2.4, 0.05], "cyan", 8, 1.6);
  light(id, "pool", [AX, PLAZA + 0.025, AZ + 8], [0, 1, 0], [3, 3, 0.05], "cyan", 10, 1.8);
  frames(row, [-10, 0, -262], [AX, -10, AZ], [AX - 12, -27, AZ + 10], [AX, -42, AZ]);
  rooms.push(row);
}
return rooms;
}

/// the plan's floor: every room rect inside it (n30-lib's even size), never smaller than `min`
function floorFor(p, min) {
  let maxAbs = 0;
  for (const r of p.rooms) for (const v of r.rect) maxAbs = Math.max(maxAbs, Math.abs(v) + 4);
  return Math.max(min, Math.ceil((maxAbs * 2 + 16) / 2) * 2);
}

function writePlan(p, out) {
  fs.mkdirSync(path.dirname(out), { recursive: true });
  fs.writeFileSync(out, JSON.stringify(p, null, 1) + "\n");
  console.log("wrote", path.relative(ROOT, out), p.rooms.length, "rooms", p.boxes.length, "boxes", p.ramps.length, "ramps", p.pits.length, "pits",
    p.lights.length, "lights", p.props.length, "props", "floorSize", p.floorSize);
}

module.exports = { addBossRooms, floorFor, writePlan };

if (require.main === module) {
  const { plan: p, src } = n37Base();
  p.rooms.push(...addBossRooms(p));
  p.floorSize = floorFor(p, src.floorSize);
  writePlan(p, OUT);
}
