// NIGHT46 builder A2 (design.n46.spire46; NIGHT46.md card A2; GG 2026-10-17: "need to feel and flow better ... study real game deisgn and areana
// layouts"; DESIGN-RULES A-9 identity before geometry, A-1 every piece has a reason; arena-flow-web.md Part D, the flow plan):
// Level 5 as n46-spire: n41-spire's plan (Design/levels/n41/spire.plan.json, written by Tools/night/gen-n41-spire.cjs, which this file does not run or
// edit) copied whole, with ONLY the rooms named in CHANGED rebuilt from their identity cards and flow plans (docs/ROOMS.md, "NIGHT46 identity cards",
// Level 5: v2 THE FALLEN BELL, v6 THE ROPE CHOIR, the belfry gates v4 / v5 / v7). Every other room, door, light and piece is n41-spire's bit for bit (the
// guard at the end throws if a row outside CHANGED would differ), so `--off46` (n41-spire) and this file differ in those rooms only.
//
// Slice 0 (this commit): the copy, the id and the guard; CHANGED is empty, so the plan equals n41-spire's except its id / _why. The rooms come in
// their own slices after the lead's card gate (PASS).
// Coordinates: web (x east, y up, z south); level 5 runs toward -z; every door is at y 0 (levelgen doors carry no height).
// Run from the repo (or worktree) root: bash Tools/night/n46-spire-regen.sh
const path = require("path");
const fs = require("fs");

const ROOT = path.join(__dirname, "../..");
const SRC = path.join(ROOT, "Design/levels/n41/spire.plan.json");
const OUT = path.join(ROOT, "Design/levels/n46/spire.plan.json");

const plan = JSON.parse(fs.readFileSync(SRC, "utf8"));
if (plan.id !== "n41-spire") throw new Error(`unexpected source id ${plan.id}`);
const before = JSON.parse(JSON.stringify(plan));
plan.id = "n46-spire";
plan._why = "NIGHT46 builder A2 (design.n46.spire46 swaps n41-spire for this file while the row is on): Tools/night/gen-n46-spire.cjs copies n41-spire's "
  + "plan whole and rebuilds only the rooms its CHANGED list names, each from its identity card + flow plan (docs/ROOMS.md, NIGHT46 identity cards). "
  + "n41-spire's _why: " + plan._why;

/// the rooms this generator rebuilds (each added by its own slice, after the card gate)
const CHANGED = new Set(["v2", "v6", "v4", "v5", "v7"]);

// ---- shared helpers ---------------------------------------------------------------------------------------------------------------------------------
const ownerOf = (e) => e.zone || e.room || null;
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const roomOf = (id) => {
  const r = plan.rooms.find((x) => x.id === id);
  if (!r) throw new Error(`room ${id} missing`);
  return r;
};
const why = (o, job, text) => Object.assign(o, { job, why: text, note: text });
/// take a room's rows out of every array (the cut list), returning how many went per array
const cutRoom = (id) => {
  const n = {};
  for (const key of ["boxes", "ramps", "pits", "lights", "props", "surfaces"]) {
    const a = plan[key] || [];
    const n0 = a.length;
    plan[key] = a.filter((e) => ownerOf(e) !== id);
    n[key] = n0 - plan[key].length;
  }
  return n;
};
/// put a room's new rows where its old rows stood (the other rooms' rows keep their order)
const insertAt = (key, id, rows) => {
  const old = before[key] || [];
  const first = old.findIndex((e) => ownerOf(e) === id);
  const at = first < 0 ? plan[key].length : old.slice(0, first).filter((e) => !CHANGED.has(ownerOf(e))).length;
  plan[key].splice(at, 0, ...rows);
};
/// segment vs axis-aligned box (slab test): does the line of sight a -> b pass through the box?
const segHitsBox = (a, b, min, max) => {
  let t0 = 0, t1 = 1;
  for (let i = 0; i < 3; i++) {
    const d = b[i] - a[i];
    if (Math.abs(d) < 1e-9) { if (a[i] <= min[i] || a[i] >= max[i]) return false; continue; }
    let u = (min[i] - a[i]) / d, v = (max[i] - a[i]) / d;
    if (u > v) [u, v] = [v, u];
    t0 = Math.max(t0, u); t1 = Math.min(t1, v);
    if (t0 >= t1) return false;
  }
  return true;
};
const EYE = 1.6, BODY = 1.0; // m: body.eyeHeight (PortExport/config.json) and FLOW-AUDIT's body-centre height for a line of sight
const FOV_HALF = 53; // deg: FLOW-AUDIT's "in view" half angle (74 deg vertical FOV at 16:9)
/// a node in view from an eye (facing -z): inside the half angle and no box of the room in the way
const inView = (eye, node, boxes) => {
  const tgt = [node[0], node[1] + BODY, node[2]];
  const bearing = Math.abs(Math.atan2(node[0] - eye[0], eye[2] - node[2]) * 180 / Math.PI);
  if (bearing > FOV_HALF) return false;
  return !boxes.some((b) => segHitsBox(eye, tgt, b.min, b.max));
};

// ==== v2 5.2 THE NAVE -> THE FALLEN BELL (docs/ROOMS.md, NIGHT46 identity cards, level 5; the card's lines are quoted in each `why`) =================
// Room rect x -25..5, z 51..89 (unchanged), roof 14; the door d1 at z 89 and the exit d2 at z 51, both on x -10; the spawn (-10, 0, 87).
// Build change from the card (logged in ROOMS.md): the narthex / lip at 2.4 m, flush with the aisles, not 3.6 m. GG NIGHT20 ("there always really needs
// to be a route that enemies can climb up") and nav.stepHeight 0.4 m (PortExport/config.json): a 1.2 m step from the aisles up to a 3.6 m lip is a step
// no walker takes, so the lip would be a camp nobody reaches (F8). At 2.4 m the walkers reach it along the aisles; the drop in is 2.4 m (a double jump,
// 2.7 m, climbs it: one-way by intent: the door behind locks, A-3, and nothing on the lip is worth going back for).
{
  const ID = "v2";
  const r = roomOf(ID);
  if (JSON.stringify(r.rect) !== JSON.stringify([-25, 51, 5, 89]) || r.height !== 14) throw new Error("v2's rect / roof changed upstream: " + JSON.stringify(r.rect));
  if (JSON.stringify(r.waves) !== JSON.stringify([["husk", "husk", "husk", "swarmer", "swarmer"], ["spitter", "spitter", "husk", "husk"], ["heavy", "swarmer", "swarmer", "swarmer"]]))
    throw new Error("v2's waves changed upstream: " + JSON.stringify(r.waves));
  const cut = cutRoom(ID);
  if (cut.boxes !== 2 || cut.ramps !== 4 || cut.lights !== 6 || cut.props !== 5 || cut.pits !== 0) throw new Error("v2's n41 pieces changed upstream: " + JSON.stringify(cut));

  // the card's numbers (m)
  const AISLE = 2.4; // the aisles' and the narthex's top (n41's aisle height; the walkers' 6 m ramps climb it at 0.4)
  const AW = [-25, -19], AE = [-1, 5]; // the aisles' x spans (n41's): 18 m apart, the jump-dash's span (~21 m flat)
  const AISLE_Z0 = 57; // the aisles' north end (the north ramps z 51..57, kept)
  const NARTHEX_Z0 = 74; // the lip's edge: the narthex is z 74..89
  const CH = [-12, -8]; // the channel's x span (the door's width)
  const RAMP_FOOT = 86, RAMP_TOP = 80; // the channel ramp: 2.4 m over 6 m (the aisles' slope); the landing z 86..89 holds the spawn (z 87)
  // The bell's footprint: x -12.5..-7.5, z 58.6..64.6, dead centre on the door's axis. Round 3 (the lead's a246-v2-lip frame, build r1: "the bell does
  // NOT read as a bell: from the lip it is a dark box"; A-6 / A-9): it lies on its side with its MOUTH toward the lip: a 5 m rim ring round a dark
  // 2.4 x 2.8 m mouth, then the body (4 m), the shoulder (3 m) and the crown narrowing away to the north: a bell's profile in one frame from the lip.
  // h = the body's top (the perch, the health).
  // Round 3b (F1's meter): the 5 m rim (x -12.5..-7.5) left a 3.5 m west lane and v2's clear went 60.3 / 65.3 -> 66.6 / 77.5 s (seeds 1-10 / 11-20);
  // the rim is 4.4 m and the arcade wall 0.6 m further west: the west lane is 4.4 m.
  // Round 3c: the 4.4 m rim cleared 59.5 / 69.3 s but stalled 20.3 / 22.3 % (n41: 14.2 / 17.1); the profile at the old block's 4 m width, 5 m long
  // (the block, 4 x 4 m: 60.3 / 65.3 s, 16.0 / 16.9 %).
  // Round 4 (the lead's build r2 frame A246-v2-lip: the side-lying bell still read as "a dark box with a darker rectangle on its face"; the lead: "in
  // level boxes a bell is a stack: a wide low skirt at the floor, a narrower waist block, a narrow crown ... the outline must step in as it rises"):
  // the bell stands UPRIGHT, mouth-down, as four stacked boxes centred on (-10, 62.5); its outline from the lip camera (eye 4.0 m, 11.5 m away) is
  // 5.0 m wide at 0.35 m (the skirt), 3.8 m at 2 m (the waist), 2.8 m at 3.4 m (the shoulder), 1.2 m at 4.2 m (the crown): it steps in as it rises.
  // A warm edge line (Kit33 edgeLight, 0xFFD7A0) on each step's front edge draws the steps in light; the amber pool lights the skirt from the front.
  const STACK = [ // [half width, top y]: skirt, waist, shoulder, crown
    { name: "SKIRT", hw: 2.5, top: 0.7 }, { name: "WAIST", hw: 1.9, top: 3.0 }, { name: "SHOULDER", hw: 1.4, top: 3.7 }, { name: "CROWN", hw: 0.6, top: 4.6 },
  ];
  const BELL_C = [-10, 62.5];
  const BELL = { x: [BELL_C[0] - STACK[0].hw, BELL_C[0] + STACK[0].hw], z: [BELL_C[1] - STACK[0].hw, BELL_C[1] + STACK[0].hw], h: STACK[2].top }; // h: the shoulder's ledge (the health)
  // Tried and cut in round 2 (F1's meter; the cut list): a 1.0 m CRATER round the bell's foot (7 x 7.5 m, then 6 x 8.5 m) so the meter's loop hole
  // passed its 50 m2 (the bell alone: 21.4 m2, "loop n"). It read "loop y" (62 / 60.5 m2) but the pilot cleared v2 slower and stalled more:
  // clear 60.3 / 65.3 s (no crater, seeds 1-10 / 11-20) -> 70.1 / 81.2 and 79.2 / 73.8; stall 16.0 / 16.9 -> 16.0 / 19.3 and 18.2 / 18.8 %.
  // The bell stands alone (the ring round it is real; the meter's threshold is the meter's).
  const ARC = { x: [-17.2, -16.6], z: [60, 72], h: 4.5 }; // round 3b: 0.6 m west (the slot to the aisle 1.8 m, the west lane 4.4 m) // the arcade wall: rides on its west face (the slot to the aisle), 2 m south of the jump-dash line z 58
  const PEW_H = 1.0, PEW_D = 0.8; // the pews: half walls one jump (1.5 m) mantles, a dash-jump clears
  const cx = -10, bellZ = 63.5; // the shoulder's ledge on the lip side (z 63.1..63.9, south of the crown): the health and the ring

  const box = (kind, min, max, job, text, extra) => why(Object.assign({ zone: ID, kind, min, max }, extra || {}), job, text);
  const boxes = [
    box("deck", [AW[0], 0, AISLE_Z0], [AW[1], AISLE, 89], "line", "the west AISLE (2.4 m, now to the south wall; card: Every-solid rule 'aisles W / E'): the air loop's west leg and a spitter stand; it joins the narthex flush at the south (the walkers reach the lip along it: GG NIGHT20)"),
    box("deck", [AE[0], 0, AISLE_Z0], [AE[1], AISLE, 89], "line", "the east AISLE (the west's mirror; card: 'aisles W / E'): the air loop's east leg, the grapple side (its ring), a spitter stand"),
    box("deck", [AW[1], 0, NARTHEX_Z0], [CH[0], AISLE, 89], "line", "the NARTHEX, west block (card: 'narthex blocks W / E'; flow plan 1): the channel's west wall (compression: the door's view is the channel) and the lip's west half"),
    box("deck", [CH[1], 0, NARTHEX_Z0], [AE[0], AISLE, 89], "line", "the NARTHEX, east block (the west's mirror): the channel's east wall and the lip's east half"),
    box("deck", [CH[0], 0, NARTHEX_Z0], [CH[1], AISLE, RAMP_TOP], "line", "the LIP (card: Molecule L; flow plan 1): the channel's top, 15 m in, where the whole nave, the bell and wave 1 show; its north edge is the drop in"),
    ...STACK.map((s, i) => box("hull", [BELL_C[0] - s.hw, 0, BELL_C[1] - s.hw], [BELL_C[0] + s.hw, s.top, BELL_C[1] + s.hw], "cover",
      ["THE BELL's SKIRT (round 4: the bell upright, mouth-down; card: Landmark / Hero prop): the lip ring flaring at the floor, 5 m wide, 0.7 m: the silhouette's widest step, and the ground loop's centre (a 4.1 m west lane by the arcade, a 6.5 m east lane)",
        "THE BELL's WAIST (3.8 m wide, to 3 m): the silhouette's second step; cover from both aisles",
        "THE BELL's SHOULDER (2.8 m, to 3.7 m): the third step; its ledge round the crown is the room's one exposed perch (the health)",
        "THE BELL's CROWN (the canons it hung by; 1.2 m, to 4.6 m): the silhouette's top step"][i])),
    box("wall", [ARC.x[0], 0, ARC.z[0]], [ARC.x[1], ARC.h, ARC.z[1]], "line", "the ARCADE WALL (card: Every-solid rule; flow plan 3 'F-W -> AW'): the nave's surviving arcade, 1.8 m off the west aisle's face; ride its west face north and jump off west onto the aisle (the floor -> air cut-through; the west is the ride side)",
      { ride: true, ride36: { face: "W", planeX: ARC.x[0], lz: [89 - ARC.z[1], 89 - ARC.z[0]] } }),
    box("hull", [-14, 0, 70], [-10, PEW_H, 70 + PEW_D], "cover", "the SW PEW (1.0 m half wall; flow plan 6): the first foothold 4 m past the lip's foot, cover from the west aisle"),
    box("hull", [-7, 0, 70], [-3, PEW_H, 70 + PEW_D], "cover", "the SE PEW (1.0 m; flow plan 6): the east lane's south cover"),
    box("hull", [-14, 0, 55], [-10, PEW_H, 55 + PEW_D], "cover", "the NW PEW (1.0 m; flow plan 6): the north lane's cover from the aisles' north ends (wave 2's spitters)"),
    box("hull", [-7, 0, 55], [-3, PEW_H, 55 + PEW_D], "cover", "the NE PEW (1.0 m; flow plan 6): the north-east quadrant's cover"),
  ];
  insertAt("boxes", ID, boxes);
  insertAt("ramps", ID, [
    why({ zone: ID, from: [cx, RAMP_FOOT], to: [cx, RAMP_TOP], width: CH[1] - CH[0], rise: AISLE }, "climb", "the CHANNEL RAMP (card: Molecule C, 'one way in'): 2.4 m over 6 m from the door's landing up to the lip, between the narthex blocks (tight, unlit)"),
    why({ zone: ID, from: [-22, 51], to: [-22, 57], width: 3, rise: AISLE }, "climb", "the west aisle's north ramp (kept from n41): the walkers' way up at the far end (GG NIGHT20) and flow plan 3's 'F-N -> a north ramp up'"),
    why({ zone: ID, from: [2, 51], to: [2, 57], width: 3, rise: AISLE }, "climb", "the east aisle's north ramp (kept from n41): the walkers' way up, and wave 3's swarmers' way down"),
  ]);
  insertAt("props", ID, [
    why({ id: "v2-P1", room: ID, kind: "healthPickup", at: [cx, BELL.h + 0.9, bellZ], size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true }, "reward",
      "the health on the BELL's shoulder ledge, its lip side (card: Hero prop; flow plan 11; N-9 a fixed pickup moved to an exposed spot): open, no cover, in every lane's line: the ring, the arcade ride + a jump, or a jump-dash off the lip reach it"),
    ...STACK.slice(0, 3).map((s, i) => why({ id: `v2-bell-edge${i + 1}`, room: ID, kind: "edgeLight", at: [BELL_C[0], s.top - 0.05, BELL_C[1] + s.hw + 0.03], size: [2 * s.hw, 0.08, 0.05], yawDeg: 0, collider: false, decor: true }, "marker",
      `the bell's ${s.name} edge line (round 4; Kit33 edgeLight, warm white 0xFFD7A0, no cyan): the step's front edge drawn in light from the lip, so the outline reads as it steps in`)),
  ]);
  insertAt("lights", ID, [
    why({ id: "v2-L1", room: ID, kind: "ceiling", at: [cx, 13.95, 70], normal: [0, -1, 0], size: [4, 3, 0.1], color: "white", range: 12, intensity: 1.2 }, "light", "the nave's south roof panel (card: Light story 'the lip's release lit white'): over the lip's drop, not over the channel"),
    why({ id: "v2-L2", room: ID, kind: "ceiling", at: [cx, 13.95, 55], normal: [0, -1, 0], size: [4, 3, 0.1], color: "white", range: 12, intensity: 1.2 }, "light", "the nave's north roof panel: the far end and the exit apse (the captain's end) read"),
    why({ id: "v2-L3", room: ID, kind: "ceiling", at: [cx, 13.95, bellZ], normal: [0, -1, 0], size: [3, 3, 0.1], color: "white", range: 13, intensity: 1.2 }, "light", "the hole in the vault the bell fell through (round 3: white; the amber moved under the bell's lip): top-lights the bell's profile"),
    why({ id: "v2-bell-lamp", room: ID, kind: "pool", at: [cx, 0.025, BELL.z[1] + 1.5], normal: [0, 1, 0], size: [6, 2.5, 0.05], color: "amber", range: 10, intensity: 2.2 }, "light", "the BELL's warm lamp at its skirt (round 4, the lead's frame: 'the lamp's warm pool lighting the skirt'; the room's one accent): a 6 x 2.5 m pool on the lip side lighting the skirt and the waist's face; shift46's fixture (amber -> red-orange as the TANK enters)"),
    why({ id: "v2-L4", room: ID, kind: "wall", at: [-24.95, 3.4, 70], normal: [1, 0, 0], size: [24, 0.2, 0.1], color: "white", range: 8, intensity: 1 }, "light", "the west aisle's wall strip (n41's, white now: one accent per room): the spitters' stand reads"),
    why({ id: "v2-L5", room: ID, kind: "wall", at: [4.95, 3.4, 70], normal: [-1, 0, 0], size: [24, 0.2, 0.1], color: "white", range: 8, intensity: 1 }, "light", "the east aisle's wall strip (n41's): the spitters' stand reads"),
    why({ id: "v2-L6", room: ID, kind: "pool", at: [cx, 0.025, 53], normal: [0, 1, 0], size: [2.5, 2.5, 0.05], color: "cyan", range: 7, intensity: 1.6 }, "light", "the exit's cyan pool (A-6, kept from n41)"),
  ]);

  // spawn spots (card flow plan 9-10); room.nodes keeps the ground spots, highNodes the aisles (Campaign24 appends them to nodes)
  const W1 = [[-16, 0, 58], [-4, 0, 58], [-15, 0, 67], [-5, 0, 67], [-3, 0, 62]]; // z 58: clear of the north pews (z 55..55.8) and the arcade wall (z 60..72)
  const W2 = [[-22, AISLE, 61], [2, AISLE, 61], [-22, AISLE, 66], [2, AISLE, 66]];
  const W3 = [[cx, 0, 53], [-14, 0, 53], [-6, 0, 53], [2, AISLE, 59]];
  r.nodes = W1.concat([[-14, 0, 53], [-6, 0, 53]]);
  r.highNodes = W2;
  r.waveNodes43 = [W1, W2, W3];
  r.waveNodes43_why = "A2 (the card's flow plan 9-10): wave 1 on the floor round the bell, all in view from the lip (ray-tested in gen-n46-spire.cjs); wave 2 on the aisles' north halves (the spitters in the open, the husks walk down the north ramps: 12-16 m ahead of a player at the bell); wave 3 at the far end (the TANK, the captain, across the arena from the entry, 22 m from the lip; the swarmers down the east aisle's north ramp)";
  r.grappleNodes = [[cx, BELL.h + 3, bellZ], [AE[0] + 3, AISLE + 3, 66]];
  r.grapple = "TWO rings (the grapple side is the east): one 3 m over the BELL's top (a pull from the floor lands you on the perch and its health), one over the east aisle (3 m over it, 3 m inside its edge): a pull from the east lane lands you among the aisle's spitters";
  r.grapple46_why = "A2 (card: Molecule 'the rings (F-E -> B, AE -> B)'): the west aisle's n41 ring is cut (the west is the ride side: the arcade wall); the first ring's Node35 spot 10 m south is the south lane's open floor";
  r.proofs = [
    { kind: "walk", points: [[cx, 0, 87], [cx, 0, 86.5]] },
    { kind: "walk", points: [[-2, 0, 72], [-2, 0, 66], [-2, 0, 58], [-2, 0, 53], [cx, 0, 53]] },
    { kind: "walk", points: [[-22, AISLE, 85], [-22, AISLE, 70], [-22, AISLE, 60]] },
  ];
  r.frames36 = { mid: [cx, AISLE, 75.5, 0, -0.12], exit: [-4, 0, 60, 20, -0.05] };
  r.spire46_why = "A2 (NIGHT46, design.n46.spire46): v2 rebuilt as THE FALLEN BELL from its identity card + flow plan (docs/ROOMS.md, NIGHT46 identity cards, level 5); the roster is n41's";

  // checks: wave 1 in view from the lip (5 / 5), every spot inside the room and on no solid, the first ring's Node35 spot on open floor
  const lipEye = [cx, AISLE + EYE, NARTHEX_Z0 + 1];
  const vis = W1.filter((n) => inView(lipEye, n, boxes));
  if (vis.length !== W1.length) throw new Error(`v2: wave 1 in view from the lip ${vis.length} / ${W1.length}: ${JSON.stringify(W1.filter((n) => !vis.includes(n)))}`);
  const inRectR = (p) => p[0] > r.rect[0] && p[0] < r.rect[2] && p[2] > r.rect[1] && p[2] < r.rect[3];
  for (const n of W1.concat(W2, W3)) {
    if (!inRectR(n)) throw new Error(`v2: spot ${n} outside the room`);
    const inside = boxes.find((b) => n[0] > b.min[0] - 0.4 && n[0] < b.max[0] + 0.4 && n[2] > b.min[2] - 0.4 && n[2] < b.max[2] + 0.4 && n[1] < b.max[1] - 0.05);
    if (inside) throw new Error(`v2: spot ${n} inside / against ${inside.why.slice(0, 40)}`);
  }
  const spot = [r.grappleNodes[0][0], 0, r.grappleNodes[0][2] + 10];
  const onSolid = boxes.some((b) => spot[0] >= b.min[0] && spot[0] <= b.max[0] && spot[2] >= b.min[2] && spot[2] <= b.max[2]);
  if (onSolid || !inRectR(spot) || spot[2] >= RAMP_TOP) throw new Error(`v2: the first ring's Node35 spot ${spot} is not open floor`);
  console.log(`v2 THE FALLEN BELL: ${boxes.length} boxes, 3 ramps, 7 lights; wave 1 in view from the lip ${vis.length} / ${W1.length}; Node35 spot ${JSON.stringify(spot)}`);
}

// ==== v6 5.6 THE CHOIR -> THE ROPE CHOIR (docs/ROOMS.md, NIGHT46 identity cards, level 5) =========================================================
// Room rect x -27..1 (narrowed 6 m from the east: the card's cut list), z -111..-69, roof 16; the door d5 at z -69 and the exit d6 at z -111, both on
// x -10; the spawn (-10, 0, -71). Law: islands. Tiers: 0 the floor; 2.4 the cantors' island; 3.2 the high stalls + the organ loft (an L, north-west).
// The entry is at the floor on purpose (the card gate's ruling 1): wave 1 stands on the island's top, in view from the door.
if (CHANGED.has("v6")) {
  const ID = "v6";
  const r = roomOf(ID);
  if (JSON.stringify(r.rect) !== JSON.stringify([-27, -111, 7, -69]) || r.height !== 16) throw new Error("v6's rect / roof changed upstream: " + JSON.stringify(r.rect));
  if (JSON.stringify(r.waves) !== JSON.stringify([["husk", "husk", "husk", "husk", "swarmer", "swarmer"], ["heavy", "spitter", "drone", "drone"], ["heavy", "heavy", "swarmer", "swarmer", "swarmer", "swarmer"]]))
    throw new Error("v6's waves changed upstream: " + JSON.stringify(r.waves));
  const cut = cutRoom(ID);
  if (cut.boxes !== 3 || cut.ramps !== 6 || cut.lights !== 4 || cut.props !== 5 || cut.pits !== 0) throw new Error("v6's n41 pieces changed upstream: " + JSON.stringify(cut));
  const RX1 = 1; // the narrowed east wall (was 7): the card's cut list (the east gallery and the room's east 6 m)
  r.rect = [-27, -111, RX1, -69];

  const ISLE = 2.4, HIGH = 3.2; // the island's top (a double jump, 2.7 m, reaches it); the top tier (past the double jump: the loft's drop is one-way)
  const IS = { x: [-15, -5], z: [-92, -82] }; // the cantors' island, 10 x 10 m on the axis
  const HS = { x: [-27, -21], z: [-104, -77] }; // the high stalls along the west wall
  const OL = { x: [-27, -14], z: [-111, -104] }; // the organ loft across the north-west, joined to the stalls
  const cx = -10, DOOR_Z = -69;
  const box = (kind, min, max, job, text, extra) => why(Object.assign({ zone: ID, kind, min, max }, extra || {}), job, text);
  const boxes = [
    box("deck", [IS.x[0], 0, IS.z[0]], [IS.x[1], ISLE, IS.z[1]], "line", "the CANTORS' ISLAND (card: Every-solid rule; flow plan 9): wave 1's stand in view from the door, the mid tier, the ground loop's centre"),
    box("deck", [HS.x[0], 0, HS.z[0]], [HS.x[1], HIGH, HS.z[1]], "line", "the HIGH STALLS (3.2 m; card: Molecule HS): the ranged stand in the open, the air loop's west leg, 6 m across from the island (a jump-dash, 0.8 m up)"),
    box("deck", [OL.x[0], 0, OL.z[0]], [OL.x[1], HIGH, OL.z[1]], "line", "the ORGAN LOFT (3.2 m; card: Molecule OL; flow plan 5): the top tier's north leg, joined to the stalls; its east end drops one way (3.2 m) to the exit apse"),
    box("hull", [OL.x[0], HIGH, OL.z[0]], [OL.x[1], 16, OL.z[0] + 1], "cover", "THE ORGAN (card: Landmark): the pipe stack on the loft's back from 3.2 m to the 16 m roof, the only roof-high solid, seen from the door 14 deg left of the axis and from every tier"),
    // set flush INTO the room's east wall (x 1..1.6, inside its shell box x 1..3): its cyan face is the wall's own face at x 1. Round 4 (the lead's
    // verbs37, build r2): standing 0.6 m proud (x 0.4..1) it put the verbs reel's wall-pick start (0.7 m off the room wall, x 0.3) inside the panel
    box("wall", [RX1, 0, -102], [RX1 + 0.6, 6, -78], "line", "the EAST RIDE WALL (cyan; card: Every-solid rule, flow plan 3 'F-E: the ride wall up'; the east is low and fast): 24 m of the east wall marked for riding, flush (nothing proud of the wall), ride north and jump off west onto the island (6 m)",
      { ride: true, ride36: { face: "W", planeX: RX1, lz: [DOOR_Z - -78, DOOR_Z - -102] } }), // lz = metres in from the entry edge (z -69): 9..33
    box("hull", [-19, 0, -76.8], [-15, 1, -76], "cover", "the SW HALF WALL (1.0 m; flow plan 1 / 6): the first foothold 7 m in, cover from the stalls"),
    box("hull", [-5, 0, -76.8], [-1, 1, -76], "cover", "the SE HALF WALL (1.0 m; flow plan 6): the east lane's first foothold, cover from the island's top"),
  ];
  insertAt("boxes", ID, boxes);
  insertAt("ramps", ID, [
    why({ zone: ID, from: [cx, -76], to: [cx, IS.z[1]], width: 4, rise: ISLE }, "climb", "the island's SOUTH RAMP (card: 'the fodder's file into the TORCH'; 2.4 m over 6 m): wave 1 comes down it at you; the walkers' way up"),
    why({ zone: ID, from: [-25.5, DOOR_Z], to: [-25.5, HS.z[1]], width: 3, rise: HIGH }, "climb", "the stalls' RAMP (3.2 m over 8 m, the south-west corner; GG NIGHT20: the walkers' way to the top tier)"),
  ]);
  insertAt("props", ID, [
    why({ id: "v6-P1", room: ID, kind: "healthPickup", at: [-15.5, HIGH + 0.9, -107], size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true }, "reward",
      "the health on the ORGAN LOFT's east end (card flow plan 11; N-9): the top tier's most exposed spot, over the one-way drop to the exit (forward bait)"),
  ]);
  insertAt("lights", ID, [
    why({ id: "v6-L1", room: ID, kind: "ceiling", at: [cx, 15.95, -74], normal: [0, -1, 0], size: [4, 3, 0.1], color: "white", range: 12, intensity: 0.9 }, "light", "the door side's one dim panel (card: Light story)"),
    why({ id: "v6-L2", room: ID, kind: "ceiling", at: [cx, 15.95, -87], normal: [0, -1, 0], size: [6, 6, 0.1], color: "white", range: 16, intensity: 1.4 }, "light", "the bell shaft's light on the island (the brightest pool: wave 1's stand reads from the door)"),
    why({ id: "v6-organ-lamp", room: ID, kind: "wall", at: [-20.5, 9, OL.z[0] + 1.05], normal: [0, 0, 1], size: [6, 0.3, 0.1], color: "amber", range: 14, intensity: 1.3 }, "light", "THE ORGAN's lamp (the room's one accent; shift46's fixture: amber -> red-orange as wave 3 drops)"),
    why({ id: "v6-L4", room: ID, kind: "pool", at: [cx, 0.025, -109], normal: [0, 1, 0], size: [2.5, 2.5, 0.05], color: "cyan", range: 7, intensity: 1.6 }, "light", "the exit's cyan pool (A-6)"),
  ]);
  r.waves = [["husk", "husk", "husk", "husk", "swarmer", "swarmer"], ["heavy", "spitter", "drone", "drone"], ["heavy", "swarmer", "swarmer", "swarmer", "swarmer", "husk", "husk"]];
  r.waves46_why = "A2 (card: one captain per wave, Part D 8): wave 3's second heavy cut for two husks: heavy + swarmer x4 + husk x2 = 10 of wave 3's 16 points (design.n27.budget), two roles";
  const W1 = [[-13, ISLE, -84], [-7, ISLE, -84], [-13, ISLE, -90], [-7, ISLE, -90], [cx, ISLE, -87], [cx, ISLE, -91]];
  const W2 = [[-24, HIGH, -100], [-3, 0, -104], [-24, HIGH, -96], [-5, 0, -100]];
  const W3 = [[-20, HIGH, -107], [-24, HIGH, -107], [-17, HIGH, -106], [cx, 0, -108], [-6, 0, -106], [-3, 0, -108]];
  r.nodes = W1.concat([[-3, 0, -104], [-5, 0, -100], [cx, 0, -108]]);
  r.highNodes = [[-24, HIGH, -100], [-24, HIGH, -96], [-20, HIGH, -107]];
  r.waveNodes43 = [W1, W2, W3];
  r.waveNodes43_why = "A2 (card flow plan 9-10): wave 1 on the island's top, in view from the door (ray-tested in gen-n46-spire.cjs); wave 2 on the far half (the stalls' north end, the north lane's east: the spitter / the TANK across from the door; the drones rise from there); wave 3 from the far side (the organ loft and the exit apse): the island is attacked from above and the way out is past them";
  r.grappleNodes = [[-6, ISLE + 3, -87], [HS.x[0] + 3, HIGH + 3, -90], [-17, HIGH + 3, -107]];
  r.grapple = "THREE rings (the card's BELL ROPES' iron grips): one 3 m over the island's east half (floor -> island), one over the high stalls (3 m over them, 3 m inside their edge), one over the organ loft's east end (island -> loft)";
  r.grapple46_why = "A2 (card: Hero prop 'the BELL ROPES'): the n41 rings over the dais / galleries moved with the islands; the first ring's Node35 spot 10 m south is the east lane's open floor";
  r.proofs = [
    { kind: "walk", points: [[cx, 0, -71], [cx, 0, -75]] },
    { kind: "walk", points: [[-2, 0, -79], [-2, 0, -95], [-2, 0, -105], [cx, 0, -107], [cx, 0, -109]] },
    { kind: "walk", points: [[-24, HIGH, -80], [-24, HIGH, -100], [-20, HIGH, -106]] },
  ];
  r.frames36 = { mid: [cx, 0, -72, 0, 0.06], exit: [-4, 0, -100, -20, -0.05] };
  r.spire46_why = "A2 (NIGHT46, design.n46.spire46): v6 rebuilt as THE ROPE CHOIR from its identity card + flow plan (docs/ROOMS.md, NIGHT46 identity cards, level 5)";

  const doorEye = [cx, EYE, -71];
  const vis = W1.filter((n) => inView(doorEye, n, boxes));
  if (vis.length !== W1.length) throw new Error(`v6: wave 1 in view from the door ${vis.length} / ${W1.length}: ${JSON.stringify(W1.filter((n) => !vis.includes(n)))}`);
  const inRectR = (p) => p[0] > r.rect[0] && p[0] < r.rect[2] && p[2] > r.rect[1] && p[2] < r.rect[3];
  for (const n of W1.concat(W2, W3)) {
    if (!inRectR(n)) throw new Error(`v6: spot ${n} outside the room`);
    const inside = boxes.find((b) => n[0] > b.min[0] - 0.4 && n[0] < b.max[0] + 0.4 && n[2] > b.min[2] - 0.4 && n[2] < b.max[2] + 0.4 && n[1] < b.max[1] - 0.05 && n[1] + 1.8 > b.min[1]);
    if (inside) throw new Error(`v6: spot ${n} inside / against ${inside.why.slice(0, 40)}`);
  }
  for (const g of r.grappleNodes) if (!inRectR(g) || g[1] > r.height - 2) throw new Error(`v6: ring ${g} outside the room or within 2 m of the roof`);
  const spot = [r.grappleNodes[0][0], 0, r.grappleNodes[0][2] + 10];
  const onSolid = boxes.some((b) => b.min[1] < 1.8 && spot[0] >= b.min[0] && spot[0] <= b.max[0] && spot[2] >= b.min[2] && spot[2] <= b.max[2]);
  const onRamp = Math.abs(spot[0] - cx) <= 2 && spot[2] <= -76 && spot[2] >= IS.z[1];
  if (onSolid || onRamp || !inRectR(spot)) throw new Error(`v6: the first ring's Node35 spot ${spot} is not open floor`);
  console.log(`v6 THE ROPE CHOIR: ${boxes.length} boxes, 2 ramps, 4 lights; wave 1 in view from the door ${vis.length} / ${W1.length}; Node35 spot ${JSON.stringify(spot)}`);
}

// ==== the belfry gates (docs/ROOMS.md, NIGHT46 identity cards, level 5: "four identical gates become three, each with its own job") ==================
// v3 BELFRY I is untouched (the lesson). Measured on n41's gates (`n38 gates <plan> --ride40`): the jump-dash lands 37 % past an 18.1 m gap (~24.8 m
// reach in the rig), the best other try (jump + air dash + double) falls 9 % short. So the card's v4 (a 24 m gap closed by a ride) cannot be built in
// a 35 m room (a 24 m gap is crossed by the jump-dash alone, under the checker's 10 % margin); v4 takes a job the rig can prove instead: THE STEP UP.
const gateBox = (id) => plan.boxes.find((b) => b.zone === id && b.gate);
const gateRow = (id) => plan.gates38.list.find((g) => g.id === id + ".G");
// ---- v4 BELFRY II -> THE STEP UP: the same 18 m gap, the landing 1.0 m up: the jump-dash's apex (1.85 m) carries it, a flat arc does not; the
// rehearsal for v6, whose air loop jump-dashes 0.8 m up onto the high stalls
if (CHANGED.has("v4")) {
  const r = roomOf("v4");
  const RISE = 1.0;
  const land = gateBox("v4");
  if (!land || land.max[1] !== 0 || land.min[2] !== -25 || land.max[2] !== -19) throw new Error("v4's landing changed upstream");
  land.max[1] = RISE;
  land.gate.to = [-10, RISE, -21];
  land.gate.rise = RISE;
  land.why = land.note = "the LANDING, 1.0 m UP past the 18 m gap (A2, THE STEP UP): the jump-dash's apex (1.85 m) lands on it, a flat arc hits its face; the rehearsal for v6's 0.8 m jump-dashes up onto the high stalls";
  const g = gateRow("v4");
  g.to = [-10, RISE, -21]; g.rise = RISE; g.why = land.gate.why = "room v4's gate (A2, THE STEP UP): the jump-dash up 1.0 m over 18 m, one per pad (spire38's meter)";
  r.name = "5.4 THE STEP UP";
  r.verb36 = "the jump-dash UP: an 18 m gap onto a landing 1 m higher (the arc's apex, v6's rehearsal)";
  r.exit = [-10, RISE, -23];
  r.proofs = [{ kind: "walk", points: [[-10, 0, 7], [-10, 0, 0]] }, { kind: "walk", points: [[-10, RISE, -20.5], [-10, RISE, -24]] }];
  r.frames36 = { mid: [-10, 0, 7, 0, -0.023], exit: [-10, RISE, -21, 0, -0.086] };
  r.spire46_why = "A2 (the belfry gates plan): v4's job is the jump-dash up a step (the card's ride gate does not fit a 35 m room: n38 gates' reach, logged in ROOMS.md)";
}
// ---- v5 BELFRY III -> THE RINGERS' STAIR: no gate; a short fight with the TORCH back, so v2 -> v6 is not 104 m without a fight
if (CHANGED.has("v5")) {
  const ID = "v5";
  const r = roomOf(ID);
  const cut = cutRoom(ID);
  if (cut.boxes !== 7 || cut.pits !== 1 || cut.lights !== 4) throw new Error("v5's n41 gate pieces changed upstream: " + JSON.stringify(cut));
  plan.gates38.list = plan.gates38.list.filter((g) => g.id !== "v5.G");
  for (const k of ["flight38", "limit", "limitReason", "meter38", "routeVia"]) delete r[k];
  r.kind = "room";
  r.name = "5.5 THE RINGERS' STAIR";
  r.job36 = "practise";
  r.verb36 = "the TORCH back: a short file down the ringers' stair at you (the breather fight between the nave and the choir)";
  r.par = 10;
  r.waves = [["husk", "husk", "swarmer", "swarmer"]];
  r.waves46_why = "A2 (the belfry gates plan): one wave, 3 of 10 points (design.n27.budget): fodder only, the TORCH's lane; the 104 m of gates between v2 and v6 broken by a ~10 s fight. Round 2 (F1's meter): 5 bodies cleared in 13.3 / 13.3 s (seeds 1-10 / 11-20), 3.0 s of it the commit gate's wait; a swarmer cut";
  // round 2 (F1's meter: v5 clear 13.3 s on both seed sets vs the ~10 s target; the file walked 26-30 m): the spots 14-18 m in (past
  // design.n24.campaign.minSpawnDist 12 m from the spawn), between the two half walls
  r.nodes = [[-13, 0, -48], [-7, 0, -48], [-10, 0, -46], [-10, 0, -50], [-6, 0, -46]];
  r.proofs = [{ kind: "walk", points: [[-10, 0, -32], [-10, 0, -63]] }];
  r.frames36 = { mid: [-10, 0, -33, 0, 0.0], exit: [-10, 0, -60, 0, -0.086] };
  r.spire46_why = "A2 (the belfry gates plan): v5 BELFRY III's gate (the third identical one) cut; the room is a short fight";
  const box = (min, max, text) => why({ zone: ID, kind: "hull", min, max }, "cover", text);
  insertAt("boxes", ID, [
    box([-16, 0, -46], [-11, 1, -45.2], "the west HALF WALL (1.0 m): cover from the file, 13 m in; the axis stays open (the walkers' lane)"),
    box([-9, 0, -52], [-4, 1, -51.2], "the east HALF WALL (1.0 m): the second cover, staggered, 19 m in: the file bends round it into the TORCH"),
    // the lead's verbs37 on build r2 (rows on): rows 9 / 11 / 13 RED on n46-spire, the pilot never moved. VerbsWall39.Pick took v5's clear 30 m west wall
    // (the gate's noride liners and pit gone) and its start (-16.7, 0.2, -33.0) is inside that wall's collision box (x -18..-16): the hit normal on this
    // face points out of the room (the pick's lane runs inside the wall box; pilasters in the room did not move it: tried, measured, cut). On n41-spire
    // the gate's noride liners kept the pick off this wall (VerbsWall39's ride pick and VerbsWall41.Row13Start skip a noride face): the liners return
    // on both side walls, 0.2 m thick. v5 is the TORCH's lane (dull dressed stone, no cyan strip: V-3), not a ride room.
    why({ zone: ID, kind: "hull", min: [-16, 0, -64], max: [-15.8, 9, -31], noride: true }, "line", "the west LINER (noride, 0.2 m; n41's gate liners' rule): the stair's dull stone wall, not a ride (V-3); keeps the verbs reel's wall pick off this wall (its start fell inside the wall box: rows 9 / 11 / 13 RED in build r2)"),
    why({ zone: ID, kind: "hull", min: [-4.2, 0, -64], max: [-4, 9, -31], noride: true }, "line", "the east LINER (noride, 0.2 m): the west's mirror"),
  ]);
  insertAt("lights", ID, [
    why({ id: "v5-L1", room: ID, kind: "ceiling", at: [-10, 8.95, -45], normal: [0, -1, 0], size: [3, 3, 0.1], color: "white", range: 9, intensity: 1 }, "light", "the stair's one white panel (the file reads)"),
    why({ id: "v5-L2", room: ID, kind: "ceiling", at: [-10, 8.95, -58], normal: [0, -1, 0], size: [3, 3, 0.1], color: "white", range: 9, intensity: 1 }, "light", "the far end's panel: wave 1's spots in view from the door"),
    why({ id: "v5-L3", room: ID, kind: "pool", at: [-10, 0.025, -62.5], normal: [0, 1, 0], size: [2.5, 2.5, 0.05], color: "cyan", range: 7, intensity: 1.6 }, "light", "the exit's cyan pool (A-6)"),
  ]);
}
// ---- v7 THE LONG BELL: the gap 18 -> 20 m (the exam near the jump-dash's reach, before the rehearsals v9 / v10)
if (CHANGED.has("v7")) {
  const r = roomOf("v7");
  const GAP_Z1 = -145; // the far edge (was -143)
  const land = gateBox("v7");
  if (!land || land.min[2] !== -149 || land.max[2] !== -143) throw new Error("v7's landing changed upstream");
  land.min[2] = -150; land.max[2] = GAP_Z1;
  land.gate.to = [-10, 0, -147]; land.gate.gap = 20;
  land.why = land.note = "the LANDING past the 20 m gap (A2, THE LONG BELL: the exam near the jump-dash's reach), flush with the floor";
  const g = gateRow("v7");
  g.to = [-10, 0, -147]; g.gap = 20; g.why = land.gate.why = "room v7's gate (A2): the jump-dash over 20 m, one per pad (spire38's meter)";
  const pit = plan.pits.find((p) => p.zone === "v7");
  pit.rect = [-16, GAP_Z1, -4, -125];
  pit.why = pit.note = "the GAP, 20 m (A2: the exam, 2 m longer than v3 / v4); a dry pit, a miss costs the stair's climb back to the pad";
  for (const b of plan.boxes.filter((q) => q.zone === "v7" && (q.ceiling || q.noride))) if (b.min[2] === -143) b.min[2] = GAP_Z1;
  r.routeVia = [[[-10, 0, -124.5], [-10, 0, -146]]];
  r.proofs = [{ kind: "walk", points: [[-10, 0, -117], [-10, 0, -124]] }, { kind: "walk", points: [[-10, 0, -146.5], [-10, 0, -148]] }];
  r.frames36 = { mid: [-10, 0, -117, 0, -0.023], exit: [-10, 0, -147, 0, -0.086] };
  r.spire46_why = "A2 (the belfry gates plan): v7's gap 18 -> 20 m";
}

// ---- the guard: nothing outside CHANGED moves -------------------------------------------------------------------------------------------------------
const ARRAYS = ["boxes", "ramps", "pads", "grapple", "pits", "tunnels", "lights", "props", "surfaces"];
for (const key of ARRAYS) {
  const keepA = (before[key] || []).filter((e) => !CHANGED.has(ownerOf(e)));
  const keepB = (plan[key] || []).filter((e) => !CHANGED.has(ownerOf(e)));
  if (!same(keepA, keepB)) throw new Error(`guard: ${key} outside ${[...CHANGED].join(",") || "(none)"} changed`);
}
for (const r of before.rooms) {
  if (CHANGED.has(r.id)) continue;
  const now = plan.rooms.find((q) => q.id === r.id);
  if (!same(r, now)) throw new Error(`guard: room ${r.id} changed (not in CHANGED)`);
}
if (plan.rooms.length !== before.rooms.length) throw new Error("guard: the room count changed");
if (!same(before.doors, plan.doors)) throw new Error("guard: the doors changed");
const gatesOutside = (p) => ((p.gates38 && p.gates38.list) || []).filter((g) => !CHANGED.has(String(g.id).split(".")[0]));
if (!same(gatesOutside(before), gatesOutside(plan))) throw new Error("guard: a gate outside CHANGED changed");
for (const k of Object.keys(before)) {
  if (ARRAYS.includes(k) || k === "rooms" || k === "doors" || k === "gates38" || k === "id" || k === "_why") continue;
  if (!same(before[k], plan[k])) throw new Error(`guard: top-level ${k} changed`);
}

fs.mkdirSync(path.dirname(OUT), { recursive: true });
fs.writeFileSync(OUT, JSON.stringify(plan, null, 1) + "\n");
console.log("wrote", path.relative(ROOT, OUT), plan.rooms.length, "rooms; rebuilt:", [...CHANGED].join(", ") || "(none: slice 0)");
