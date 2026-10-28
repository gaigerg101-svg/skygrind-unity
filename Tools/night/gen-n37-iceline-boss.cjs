// NIGHT37 builder S (NIGHT37.md S1; BOSSES.md "L2 THE STAG"; DECIDE37 D-4): LEVEL 2 THE ICE LINE with THE STAG from scratch. design.n37.stag37 swaps
// n35-iceline for this level (levelFrom -> levelTo). It writes Design/levels/n37/iceline.plan.json:
//   1. n35-iceline's rooms up to its last rest (i1a .. i7), COPIED AS THEY ARE from its plan (Design/levels/n35/iceline.plan.json: read as data; no older
//      generator is required). Their rows keep every key and coordinate; each copied solid gains a `job` + `why` (the n36 checker's tags; the why names the
//      source); i7's lesson prompt is the only text changed (it named the old Stag's charge).
//   2. two REHEARSAL rooms (BOSSES.md rule 9, "taught before it is tested"; D-4), built here on Tools/night/n36-lib.cjs (every solid a job + why):
//      2.8 LANES       three 5 m skate lanes with lane-wall runs (ride panels) and a spitter deck at the far end: its globs come down your lane, so you
//                      DASH SIDEWAYS to change lanes (the Stag's shard kick);
//      2.9 SLIPSTREAM  a 90 m three-lane ice run with a PACER (the sim's stag37 pacer row: a cyan skater that bolts when you cross its line, faster
//                      than you on open ice): sit behind it (the slipstream: +25 %), catch it (4 m) and punch it.
//   3. 2.10 THE GATE (the walk-in hall) and 2.11 THE CANAL: the boss room. Its row carries stag37.centre: Game/SurvivalSim.Stag37.cs builds the canal
//      loop there at load (Core/Enemies/Stag37Arena.cs, design.n37.stag37.arena): this file reserves the room (the rect, the door to the loop's entry
//      gap, a pit under straight B's water so the floes are the only floor there, the ice surface) and the boss id "stag37".
// Run: node Tools/night/gen-n37-iceline-boss.cjs  (then bash Tools/night/n37s-regen.sh: levelgen + the copies + the checker)
// NIGHT38 builder S2 (NIGHT38-BRIEF card S2; PLAN.md section 2 L2; the beat sheet Design/levels/n38/iceline.beats.md, committed first): `--n38` writes
// n38-iceline instead (Design/levels/n38/iceline.plan.json): NO copied room: four new rooms j1..j4 (2.1 THE GAUNTLET, 2.2 THE RINK, 2.3 THE RUN (a
// flight38 room: a skate gap, a ride gate, a ride chain), 2.4 REST) built on n36-lib from the beat sheet, then the SAME s1-s4 code as n37 (the two
// rehearsal rooms, the walk-in, THE CANAL). Gaps are spaced from builder E's measured envelope (Design/levels/envelopes38.json, read: nothing typed);
// gates carry E's `gate` tag (`n38 gates` judges them) and the skate gap `skate38` (harness `iceline38 skate`: E's box world has no ice).
// Without --n38 the output is NIGHT37's file byte for byte.  Run: node Tools/night/gen-n37-iceline-boss.cjs --n38  (then bash Tools/night/n38s2-regen.sh)
const fs = require("fs");
const path = require("path");
const { Level, r3 } = require("./n36-lib.cjs");

const N38 = process.argv.includes("--n38");
const ROOT = path.join(__dirname, "../..");
const SRC = path.join(ROOT, "Design/levels/n35/iceline.plan.json");
const OUT = N38 ? "Design/levels/n38/iceline.plan.json" : "Design/levels/n37/iceline.plan.json";
const KEEP = ["i1a", "i1b", "i1r", "i2", "i3", "i4", "i5", "i6", "i6b", "i7"];
const src = JSON.parse(fs.readFileSync(SRC, "utf8"));
const cfg = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/unity-config.json"), "utf8"));
const A = cfg.design.n37.stag37.arena; // the canal's numbers (the same the sim builds from)

// ---------------------------------------------------------------- 1. the copied rooms
const copied = {};
const inKeep = (z) => KEEP.includes(z);
const zoneOf = (o) => o.zone || o.room;
const why0 = (o, what) => `n35-iceline's ${what}, copied as it is (Design/levels/n35/iceline.plan.json)` + (o.note ? `: ${o.note}` : "");
copied.rooms = src.rooms.filter((r) => inKeep(r.id)).map((r) => JSON.parse(JSON.stringify(r)));
if (copied.rooms.length !== KEEP.length) throw new Error("gen-n37-iceline-boss: n35-iceline is missing a room of " + KEEP.join(","));
const i7 = copied.rooms.find((r) => r.id === "i7");
for (const k of ["lessons", "lessons31"]) for (const l of i7[k] || []) { l.prompt = "THE STAG WAITS. LEARN THE LANES."; /* NIGHT37 lead: at most 6 words (Lesson25TextTests) */ l.promptPad = l.prompt; }
copied.doors = src.doors.filter((d) => inKeep(d.between[0]) && inKeep(d.between[1]));
copied.boxes = src.boxes.filter((b) => inKeep(b.zone)).map((b) => Object.assign({}, b, { job: b.kind === "deck" || b.kind === "hull" || b.kind === "wall" ? "line" : "cover", why: why0(b, "box") }));
copied.ramps = src.ramps.filter((b) => inKeep(b.zone)).map((b) => Object.assign({}, b, { job: "climb", why: why0(b, "ramp") }));
copied.pits = src.pits.filter((b) => inKeep(b.zone)).map((b) => Object.assign({}, b, { job: "hazard", why: why0(b, b.lava ? "lava" : "pit") }));
copied.tunnels = (src.tunnels || []).filter((b) => inKeep(b.zone)).map((b) => Object.assign({}, b, { job: "line", why: why0(b, "tunnel") }));
copied.surfaces = (src.surfaces || []).filter((s) => inKeep(zoneOf(s)));
copied.lights = src.lights.filter((l) => inKeep(zoneOf(l)));
copied.props = src.props.filter((p) => inKeep(zoneOf(p))).map((p) => (p.collider ? Object.assign({}, p, { job: p.job || "cover", why: p.why || `n35-iceline's dressing (${p.kind}), copied as it is` }) : p));
copied.teach35 = (src.teach35 || []).filter((t) => inKeep(t.room));
for (const k of ["pads", "grapple"]) copied[k] = (src[k] || []).filter((o) => inKeep(zoneOf(o)));

// ---------------------------------------------------------------- 2-3. the new rooms (n36-lib)
const L = N38
  ? new Level("n38-iceline", src.name,
    "NIGHT38 builder S2 (design.n38.iceline38 swaps n37-iceline for this file): Tools/night/gen-n37-iceline-boss.cjs --n38 from the beat sheet Design/levels/n38/iceline.beats.md: "
    + "no copied room: 2.1 THE GAUNTLET (the PULSE gauntlet, a tank alone, an open roofed arena), 2.2 THE RINK (an ice loop round a ride-wall island, enemies on the lane ahead), "
    + "2.3 THE RUN (flight38: a skate gap, a ride gate, a ride chain; gaps from Design/levels/envelopes38.json), 2.4 REST, then NIGHT37's 2.8 LANES, 2.9 SLIPSTREAM, 2.10 THE GATE and "
    + "2.11 THE CANAL (THE STAG's loop at stag37.centre; stag38's walls with that row on). GG 2026-10-13 (Research/2026-10-13-wave38/GG-REQUEST.md).",
    { rank: RANK38(), n27: src.n27 })
  : new Level("n37-iceline", src.name,
  "NIGHT37 builder S (design.n37.stag37 swaps n35-iceline for this file): Tools/night/gen-n37-iceline-boss.cjs = n35-iceline's rooms i1a..i7 copied as they are + two rehearsal rooms "
  + "(2.8 LANES: change lanes under fire; 2.9 SLIPSTREAM: draft a pacer and catch it) + 2.10 THE GATE + 2.11 THE CANAL (THE STAG's loop, built at load by Core/Enemies/Stag37Arena.cs at stag37.centre). "
  + "BOSSES.md 'L2 THE STAG'; DECIDE37 D-4.",
  { rank: src.rank, n27: src.n27 });
const srcI7 = src.rooms.find((r) => r.id === "i7");
if (!N38) L.prev = { id: "i7", z0: srcI7.rect[1], w: srcI7.rect[2] - srcI7.rect[0], h: srcI7.height, outX: srcI7.exit[0], s: {} };
const ICE = [];
function ice(r, lx0, lz0, lx1, lz1, note, laneLx, half) {
  const row = { kind: "ice", rect: r.rect(lx0, lz0, lx1, lz1), minY: -0.5, maxY: 0.5, room: r.id, note };
  if (laneLx !== undefined) row.lane = { axis: "z", line: r.X(laneLx), halfWidth: half, dir: [0, -1] };
  ICE.push(row);
}
const ridePanel = (r, lx, lz0, lz1, hgt, why) => {
  const b = r.box("wall", lx - 0.2, lz0, lx + 0.2, lz1, 0, hgt, "line", why);
  b.ride = true;
  b.ride36 = { face: "both", planeX: r.X(lx), lz: [lz0, lz1] };
  return b;
};
const CX = N38 ? n38Rooms() : srcI7.exit[0];

// 2.8 LANES (rehearse: change lanes; the Stag's shard kick)
{
  const r = L.room({ id: "s1", name: "2.8 LANES", job: "rehearse", verb: "change lanes (dash sideways) under fire", w: 15, l: 44, h: 7, par: 25, cx: CX, spawnLz: 2, exitFromN: 2, exitY: 1.5 });
  [-5, 0, 5].forEach((lx, i) => ice(r, lx - 2.5, 0, lx + 2.5, 38, ["the west lane", "the middle lane", "the east lane"][i] + " (5 m, as THE CANAL's)", lx, 2.5));
  ridePanel(r, -2.5, 10, 26, 2.6, "a lane wall (ride panel, as straight B's): a run between the west and middle lanes; its ends are the gaps you change lanes at");
  ridePanel(r, 2.5, 16, 32, 2.6, "a lane wall between the middle and east lanes, offset from the west one: the lane you need is open at a different place");
  r.box("deck", -7.5, 38, 7.5, 44, 0, 1.5, "line", "the SPITTERS' deck across the far end: their globs fly down your lane; dash sideways to change lanes (the Stag's shard kick, rehearsed)");
  r.ramp(-6, 33, 38, 2.5, 1.5, "climb", "the deck's west ramp (the spitters' way down and yours up)");
  r.ramp(6, 33, 38, 2.5, 1.5, "climb", "the deck's east ramp");
  r.highNodeAt(-4, 41, 1.5); r.highNodeAt(0, 41.5, 1.5); r.highNodeAt(4, 41, 1.5);
  r.row.waves = [["spitter"], ["spitter", "spitter"]];
  r.lesson({ kind: "husksDead", n: 3, prompt: "DASH SIDEWAYS: CHANGE LANES" });
  r.ceil(-5, 20, 1, 30, "cyan", 8, 0.8); r.ceil(5, 20, 1, 30, "cyan", 8, 0.8); // the lane lines overhead
  r.ceil(0, 41, 6, 2, "amber", 8, 1.0);
  r.pool(0, 42.5, 2, "cyan", 6, 1.4, undefined, 1.5);
  r.walkSE([[-5, 0, 8], [-5, 0, 32], [-6, 1.5, 39], [CX - CX, 1.5, 42]]);
  r.frames([0, 0, 3], [0, 1, 40], [-5, 0, 28], [5, 1.5, 41]);
}

// 2.9 SLIPSTREAM (rehearse: draft behind a faster skater, catch it, punch it)
{
  const r = L.room({ id: "s2", name: "2.9 SLIPSTREAM", job: "rehearse", verb: "the slipstream: skate behind it, catch it, punch it", w: 15, l: 90, h: 7, par: 20, cx: CX, spawnLz: 2, exitFromN: 2 });
  [-5, 0, 5].forEach((lx, i) => ice(r, lx - 2.5, 0, lx + 2.5, 90, ["the west lane", "the middle lane", "the east lane"][i] + " of the run (5 m)", lx, 2.5));
  r.row.pacer37 = { from: r.pt(0, 0, 10), to: r.pt(0, 0, 84), startLz: 6, _why: "Game/SurvivalSim.Stag37.cs's pacer: a cyan skater on the middle lane that bolts when the player crosses lz 6 (speed design.n37.stag37.pacer); the slipstream rule behind it as behind the Stag; catch it within 4 m and punch it" };
  r.lesson({ kind: "reached", rect: [-6, 78, 6, 86], minY: -0.5, prompt: "SLIPSTREAM: CATCH IT. PUNCH IT." });
  r.ceil(0, 20, 2, 2, "white", 9, 1.0); r.ceil(0, 45, 2, 2, "white", 9, 1.0); r.ceil(0, 70, 2, 2, "white", 9, 1.0);
  r.trim("W", 45, 80, 1.2, "cyan"); r.trim("E", 45, 80, 1.2, "cyan");
  r.pool(0, 87.5, 2.5, "cyan", 7, 1.6);
  r.walkSE([[0, 0, 45]]);
  r.frames([0, 0, 12], [0, 1, 60], [2, 0, 70], [0, 1, 89]);
}

// 2.10 THE GATE (the walk-in: the loop's light through the gap)
{
  const r = L.room({ id: "s3", name: "2.10 THE GATE", kind: "link", job: "link", verb: "walk in", w: 6, l: 6, h: 4.5, par: 2, cx: CX, spawnLz: 0.5, exitFromN: 0.5 });
  r.pool(0, 4.5, 1.5, "cyan", 4, 0.8);
  r.ceil(0, 2, 2, 2, "white", 5, 0.5);
  r.walkSE([]);
  r.frames([0, 0, 0.8], [0, 1, 40], [0, 0, 5], [10, 1, 60]);
}

// 2.11 THE CANAL (boss: the loop is built at load by the sim at stag37.centre)
{
  const half = A.straight / 2, outer = A.turnR + (A.lanes * A.laneWidth) / 2, inner = A.turnR - (A.lanes * A.laneWidth) / 2;
  const rf = outer / Math.cos(Math.PI / A.turnFacets / 2) + 0.05;
  const W = 2 * Math.ceil(rf + 1), lenS = 0.5 + rf + half; // the gap's wall sits 0.5 m inside the room's south edge
  const l = Math.ceil(lenS + half + rf + 1);
  const r = L.room({ id: "s4", name: "2.11 THE CANAL", kind: "boss", job: "boss", verb: "the chase: slipstream, lanes, catch and punch, the head-on parry", w: W, l, h: 8, par: 60, cx: CX, spawnLz: 1.5, exitFromN: l / 2 });
  r.row.roof = false;
  r.row.boss = "stag37";
  const C = [r.X(0), 0, r3(r.z1 - lenS)];
  r.row.spawn = { at: [C[0], 0, r3(C[2] + half + outer - 1.5)], yawDeg: 0 };
  r.row.exit = [C[0], 0, C[2]];
  r.row.stag37 = { centre: C, _why: "Core/Enemies/Stag37Arena.cs builds THE CANAL here at load (design.n37.stag37.arena): the loop's centre on the ice; its entry gap at the south apex meets this room's door" };
  // straight B's water: no floor under the floes (they are the sim's layer); a dry pit far down (a fall restarts the leg: D-6)
  const zS = lenS; // local lz of the loop centre
  r.pit(-outer - 0.05, zS - half, -inner + 0.05, zS + half, -2, "straight B's WATER under the floes (Stag37Arena's floe layer is the only ice there): a floe that drops opens it; a fall below design.n37.stag37.fallY restarts the leg (D-6)");
  r.box("deck", -outer - 0.05, zS - half, -inner + 0.05, zS - half + 1.5, -2, -1, "climb", "the water's step at B's south end (levelgen's pit recovery: 1 m, then 1 m to the turn's ice); in play a fall restarts the leg before it is reached (D-6)");
  ice(r, -W / 2, 0, W / 2, l, "THE CANAL's ice (the sim's Stag37 surface says the same): every metre of the loop glides (design.n30.glide)");
  // mid: from the gap up straight A (east, x +R); exit: from straight B's south end (room lz = zS - half + 2: 13 m short of the mouth) north into the tunnel's south mouth (arena z = -half + tunnelTo is room lz zS + half - tunnelTo)
  r.frames([0, 0, 3], [A.turnR, 1, zS + half], [-A.turnR, 0, zS - half + 2], [-A.turnR, 2, zS + half - A.tunnelTo]);
}

const plan = L.finish();
if (N38) { writeN38(plan); process.exit(0); }
// the new doors after the copied ones (n36-lib numbers its own d1..)
let dn = copied.doors.length;
for (const d of plan.doors) d.id = `d${++dn}`;
plan.id = "n37-iceline";
plan.name = src.name;
plan.n29 = src.n29; plan.n30 = src.n30;
plan.wallHeight = src.wallHeight;
plan.teach35 = copied.teach35.concat(plan.teach35 || []);
plan.rooms = copied.rooms.concat(plan.rooms);
plan.doors = copied.doors.concat(plan.doors);
for (const k of ["boxes", "ramps", "pits", "tunnels", "lights", "props", "pads", "grapple"]) plan[k] = (copied[k] || []).concat(plan[k] || []);
plan.surfaces = copied.surfaces.concat(ICE);
let maxAbs = 0;
for (const r of plan.rooms) maxAbs = Math.max(maxAbs, ...r.rect.map(Math.abs));
plan.floorSize = Math.max(src.floorSize, Math.ceil((maxAbs * 2 + 16 + 8) / 2) * 2);
fs.mkdirSync(path.join(ROOT, path.dirname(OUT)), { recursive: true });
fs.writeFileSync(path.join(ROOT, OUT), JSON.stringify(plan, null, 1) + "\n");
console.log("wrote", OUT, plan.rooms.length, "rooms (", copied.rooms.length, "copied ),", plan.boxes.length, "boxes", plan.ramps.length, "ramps", plan.pits.length, "pits", plan.surfaces.length, "surfaces", plan.lights.length, "lights", plan.props.length, "props; floorSize", plan.floorSize);
const canal = plan.rooms.find((r) => r.id === "s4");
console.log("THE CANAL: rect", JSON.stringify(canal.rect), "centre", JSON.stringify(canal.stag37.centre), "spawn", JSON.stringify(canal.spawn.at));

// ================================================================ NIGHT38 S2: n38-iceline (--n38)

/// The S/A/B/C rank cuts: the beat sheet's room times (j1 30 s, j2 50, j3 30, j4 5, s1-s3 about 45) + THE STAG's bot band p50 (21 s, harness
/// `iceline38 band`) = about 181 s, x [1.3, 2, 3, 4.5] as NIGHT36's; kills / style as n35-iceline's (NOT calibrated: no walk bot medians yet).
function RANK38() {
  return { time: [235, 360, 545, 815], kills: [100, 85, 65, 40], style: [2500, 1500, 700, 250],
    _why: "Unity-only (builder S2, NIGHT38): S/A/B/C cuts: time = the beat sheet's room times (about 160 s) + THE STAG's bot band p50 21 s (harness iceline38 band), x [1.3, 2, 3, 4.5] as NIGHT36's; kills and style as n35-iceline's (NOT calibrated: no walk bot medians yet)." };
}

/// The level-2 movement envelope (builder E's measured table, Design/levels/envelopes38.json): the best ground reach of every try level 2 owns
/// (no THUNDERCLAP boost: level 3; no grapple: level 4) and the facing-wall gaps that chain.
function ENV38() {
  const e = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/levels/envelopes38.json"), "utf8"));
  let best = 0, by = "", best2 = 0;
  for (const [k, v] of Object.entries(e.ground)) {
    if (k.includes("boost")) continue;
    if (v.reach > best) { best = v.reach; by = k; }
    best2 = Math.max(best2, v.reach2m);
  }
  return { best, by, best2, chain: e.facingGapsThatChain, rideOff0: e.ride["rideoff p0"].out, src: "Design/levels/envelopes38.json (builder E, measured on the real sim)" };
}

/// j1..j4 (the beat sheet's rooms 1-4). Returns the centre line x the s rooms continue on (j2's exit at its NE corner).
function n38Rooms() {
  const E = ENV38();
  const ARENA = (r) => { r.row.arena36 = true; };
  const noride = (b) => { b.noride = true; return b; };
  // ---------------------------------------------------------------- 2.1 THE GAUNTLET (teach: the PULSE gauntlet; an open roofed arena)
  {
    const r = L.room({ id: "j1", name: "2.1 THE GAUNTLET", job: "teach", verb: "the PULSE gauntlet: a tank dies to one punch", w: 26, l: 30, h: 10, par: 30, spawnLz: 2, exitFromN: 1.5 });
    ARENA(r);
    r.row.pickup31 = { id: "j1-P31", kind: "weaponPickup", weapon: "gauntlet", at: r.pt(0, 1, 4.5) };
    r.box("deck", -13, 24, -3, 30, 0, 2.0, "line", "the north deck's west half (2 m): wave 2 switches on here, in view from the door, and comes down its ramp at you");
    r.box("deck", 3, 24, 13, 30, 0, 2.0, "line", "the north deck's east half (2 m): the exit runs between the halves at floor level");
    r.ramp(-5.5, 18, 24, 3, 2.0, "climb", "the west half's ramp: the husks' way down to you (2 m over 6 m) and yours up");
    r.ramp(5.5, 18, 24, 3, 2.0, "climb", "the east half's ramp");
    r.panel("E", -9, 8, 22, 0, 5, "ride panel W (5 m tall, 14 m): the line over the floor's fight: ride past the pack toward the deck (THE CANAL's walls are the same height)");
    r.panel("W", 9, 8, 22, 0, 5, "ride panel E, 18 m from W plane to plane (no chain: each is its own line over the floor)");
    r.nodeAt(0, 16);
    r.highNodeAt(-8, 27, 2.0); r.highNodeAt(-6, 27.5, 2.0); r.highNodeAt(8, 27, 2.0); r.highNodeAt(6, 27.5, 2.0);
    r.row.waves = [["tank"], ["husk", "husk", "husk", "tank"]];
    r.row.lessons = [{ kind: "husksDead", n: 1, prompt: "THE TANK DIES TO ONE PUNCH", promptPad: "THE TANK DIES TO ONE PUNCH" }];
    r.row.lessons31 = [{ kind: "husksDead", n: 1, prompt: "GRAB THE GAUNTLET. PUNCH THE TANK.", promptPad: "GRAB THE GAUNTLET. PUNCH THE TANK." }];
    r.pickup("healthPickup", -8, 27, 2.9, "the high ground pays health after the tank wave");
    r.ceil(0, 4.5, 3, 3, "white", 9, 1.1); r.ceil(0, 16, 4, 4, "amber", 10, 1.1); r.ceil(-8, 27, 4, 3, "white", 9, 1.0); r.ceil(8, 27, 4, 3, "white", 9, 1.0);
    r.pool(0, 4.5, 1.5, "cyan", 5, 1.2); // the gauntlet's plinth
    r.pool(0, 28, 2.5, "cyan", 7, 1.6); // the exit, between the deck halves
    r.walkSE([[3, 0, 10], [0, 0, 27]]);
    r.walk([[5.5, 0, 16], [5.5, 2.0, 25], [8, 2.0, 27]]);
    r.frames([10, 0, 3], [0, 1, 16], [-8, 2.0, 27], [0, 1.0, 29]); // mid: from the SE corner over the tank's spot to the deck; exit: from the deck to the gap
  }
  // ---------------------------------------------------------------- 2.2 THE RINK (combine: skate, ride, punch; an open roofed ice arena)
  {
    const OUT_X = 14;
    const r = L.room({ id: "j2", name: "2.2 THE RINK", job: "combine", verb: "skate the loop, ride the island, punch the tanks", w: 36, l: 40, h: 12, par: 50, spawnLz: 2, exitFromN: 2, outX: OUT_X });
    ARENA(r);
    ice(r, -18, 0, 18, 40, "THE RINK's ice, wall to wall: every metre glides (design.n30.glide): the loop is a skate");
    r.panel("E", 6, 10, 30, 0, 5, "the ISLAND's east face (5 m tall, 20 m): the ride wall beside the east lane, over the husks that meet you on it (THE CANAL's island, rehearsed)");
    r.panel("W", -6, 10, 30, 0, 5, "the ISLAND's west face: the ride wall beside the west lane, past the spitters' globs");
    r.box("hull", -5.4, 10, 5.4, 30, 0, 5, "line", "the ISLAND's core between its two ride faces: the loop's inside (you skate round it, never through)");
    r.box("deck", -18, 34, -12, 40, 0, 1.5, "line", "the NW perch (1.5 m): the spitters' stand in wave 3, seen across the rink from the door");
    r.ramp(-15, 28, 34, 3, 1.5, "climb", "the perch's ramp down to the west lane (the spitters' route and yours up)");
    [[13, 18], [12, 24], [14, 30], [10, 35], [0, 35], [-3, 36.5], [-12, 20], [-13, 12]].forEach(([x, z]) => r.nodeAt(x, z));
    r.highNodeAt(-15, 37, 1.5); r.highNodeAt(-13.5, 38, 1.5);
    r.row.waves = [["husk", "husk", "husk", "husk"], ["husk", "husk", "tank"], ["spitter", "spitter", "husk", "husk"]];
    r.lesson({ kind: "husksDead", n: 4, prompt: "SKATE THE LOOP. RIDE THE ISLAND." });
    r.pickup("healthPickup", 0, 33, 0.9, "the north end between the waves: a lap pays health");
    r.ceil(12, 8, 3, 3, "white", 10, 1.0); r.ceil(12, 22, 3, 3, "white", 10, 1.0); r.ceil(12, 34, 3, 3, "white", 10, 1.0);
    r.ceil(-12, 8, 3, 3, "white", 10, 0.9); r.ceil(-12, 22, 3, 3, "white", 10, 0.9);
    r.ceil(-15, 37, 3, 3, "amber", 9, 1.0);
    r.pool(OUT_X, 38, 2.5, "cyan", 8, 1.6);
    r.walkSE([[12, 0, 6], [12, 0, 34]]);
    r.walk([[0, 0, 34], [-12, 0, 33], [-15, 0, 27], [-15, 1.5, 36]]);
    r.frames([12, 0, 3], [12, 1, 30], [-12, 0, 6], [12, 1, 38]); // mid: up the east lane beside the island; exit: from the west lane round to the NE exit
  }
  // ---------------------------------------------------------------- 2.3 THE RUN (test, flight38: the skate gap, the ride gate, the ride chain)
  const CXR = 14;
  {
    // round 3 (E's stricter checker, port/stage3 198ae14d: the Mover's dash jump and the PULSE gauntlet's LT blast are owned tries; envelopes38: dash jump +
    // double 37.44 m, dash jump + blast 57.26 m, 60.77 m to a landing 2 m lower): the ride gates' voids are longer than the best other try's 2 m-lower reach
    // + 5 %, so no ceiling or wall trick is needed and a fall into the void is the same climb. G1 stays the 24 m skate gap of round 1 (a flow beat: the
    // dash jump and the blast cross it too; E's box world has no ice, so it is not a D-6 gate: harness iceline38 gates prints it).
    const GR = Math.ceil(E.best2 * 1.05);
    const LANE = 56, G1 = 24, D1 = 12, G2 = GR, D2 = 10, G3 = GR, D3 = 10; // round 3: D3 10 m and W2 6 m over it (the chain cleared by +9 % with 2 m: under the 10 %)
    if (G2 < E.best2 * 1.05 || G3 < E.best2 * 1.05) throw new Error("a ride gate's void is inside the best other reach");
    const CH = 8; // the chain's plane gap (W1 / E1 / W2): in E's measured facing gaps that chain
    if (!E.chain.includes(CH)) throw new Error(`an ${CH} m facing gap is not in envelopes38's chain list`);
    const z1 = LANE, z2 = z1 + G1, z3 = z2 + D1, z4 = z3 + G2, z5 = z4 + D2, z6 = z5 + G3, Lr = z6 + D3;
    const PIT = -4, ROOF = 12;
    // the ride line: 0.2 m between the capsule (PortExport body.capsuleRadius) and the west walls' face at lx -4, inside the ride's attach reach
    // (design.n35.wallride35.attachReach 0.5): the gates' takeoff line
    const RX = r3(-4 + JSON.parse(fs.readFileSync(path.join(ROOT, "PortExport/config.json"), "utf8")).body.capsuleRadius + 0.2);
    const r = L.room({ id: "j3", name: "2.3 THE RUN", job: "test", verb: "skate gap, ride gate, ride chain", w: 16, l: Lr, h: ROOF, par: 30, cx: CXR, spawnLz: 2, exitFromN: 2 });
    r.row.flight38 = true;
    ice(r, -8, 2, 8, LANE, `the RUN-UP: a ${LANE} m ice lane from the door to the first lip: pump dashes to the skating speed (the canal's pumped lap average 21.2 m/s, harness stag37 speeds)`, 0, 5);
    // the voids: one dry pit, the decks rise from its floor; a fall costs a climb back up the stair at each void's south end
    r.pit(-5, z1, 5, Lr, PIT, `THE RUN's void under all three gates (a dry pit ${-PIT} m deep): the decks stand on its floor; a fall costs the climb back to the gate, never health`);
    // the void's sides: noride hulls, floor of the pit to the roof (no ride along a skate gap; the ride gates have their own panels)
    noride(r.box("hull", -8, z1, -5, Lr, PIT, ROOF, "line", "the void's WEST side, pit floor to roof: a noride wall (dull, no cyan trim): no ride along the skate gap; the ride gates use their own panels"));
    noride(r.box("hull", 5, z1, 8, Lr, PIT, ROOF, "line", "the void's EAST side, pit floor to roof: a noride wall"));
    // the decks (each gate's landing; each the next gate's takeoff)
    const d1 = r.box("deck", -5, z2, 5, z3, PIT, 0, "line", `D1: G1's landing (the ${G1} m skate gap past the lane's lip: a flow beat, not a D-6 gate: the dash jump and the PULSE blast clear it too, ${E.src}) and G2's takeoff`);
    const d2 = r.box("deck", -5, z4, 5, z5, PIT, 0, "line", `D2: G2's landing (${G2} m of void: a ride wall only) and G3's takeoff`);
    const d3 = r.box("deck", -5, z6, 5, Lr, PIT, 0, "line", `D3: the exit deck, G3's landing (${G3} m of void: the ride chain only)`);
    // G1 THE SKATE GAP: the lane's speed carried into a jump (+ double, + dash). E's box world has no ice: this builder's harness proves it (iceline38 skate)
    d1.skate38 = { from: r.pt(0, 0, z1 - 0.5), to: r.pt(0, 0, z2 + 3), laneFrom: r.pt(0, 0, 3), id: "j3-g1",
      _why: `the SKATE gap (${G1} m): flown by harness iceline38 gates on the real ice (not a D-6 gate since round 3: the best other try off dry ground is ${E.best} m, ${E.by}; E's box world has no ice)` };
    // G2 THE RIDE WALL: a 5 m wall along the void's west side, from D1 over to D2
    r.panel("E", -4, z3 - 2, z4 + 2, PIT, 5, `G2's RIDE WALL (face east, ${G2 + 4} m): jump at it from D1, hold, ride the ${G2} m void, jump off onto D2`);
    d2.gate = { verb: "ride", from: r.pt(RX, 0, z3 - 0.5), to: r.pt(RX, 0, z4 + 4), runUp: 6, id: "j3-g2" };
    // G3 THE CHAIN: three short walls, alternating sides, CH m apart plane to plane; none spans the void alone
    // three walls over the void + 2 m each end, overlapping 4 m along it (the release 3 m before a wall's end sees the next one beside it)
    const WL = (G3 + 4 + 2 * 4) / 3;
    const W1 = [z5 - 2, r3(z5 - 2 + WL)], E1 = [r3(W1[1] - 4), r3(W1[1] - 4 + WL)], W2 = [r3(E1[1] - 4), z6 + 6];
    r.panel("E", -CH / 2, W1[0], W1[1], PIT, 5, `G3 chain wall W1 (face east, ${W1[1] - W1[0]} m): ride it off D2, jump off across ${CH} m to E1`);
    r.panel("W", CH / 2, E1[0], E1[1], PIT, 5, `G3 chain wall E1 (face west, ${CH} m from W1 plane to plane: in E's chain list ${E.chain[0]}-${E.chain[E.chain.length - 1]} m)`);
    r.panel("E", -CH / 2, W2[0], W2[1], PIT, 5, "G3 chain wall W2 (face east): the last link onto the exit deck");
    d3.gate = { verb: "ride", from: r.pt(RX, 0, z5 - 0.5), to: r.pt(RX, 0, z6 + 3), runUp: 6, id: "j3-g3" };
    // the harness's map of the run (iceline38 gates: the skate gap and the chain flown on the real level): world points and z ranges (north = -z)
    const zr = (a, b) => [r.Z(b), r.Z(a)];
    r.row.run38 = {
      _why: "Read by Tools/SimHarness/Harness.N38.Iceline.cs (iceline38 gates): the skate gap G1 and the chain G3 flown by the real Mover on this level (E's box world has no ice; its ride try releases only near the landing, so it never chains).",
      laneStart: r.pt(0, 0, 3), lip1: r.pt(0, 0, z1), d1: zr(z2, z3), d1Lip: r.pt(RX, 0, z3), d2: zr(z4, z5), d2Lip: r.pt(RX, 0, z5), d3: zr(z6, Lr),
      stairs: [r.pt(-3, 0, z1 + 5), r.pt(3, 0, z3 + 5), r.pt(1.7, 0, z5 + 5)], // each void's stair foot (step 1), the way back up to its takeoff
      chain: [{ planeX: r.X(-CH / 2), side: -1, z: zr(W1[0], W1[1]) }, { planeX: r.X(CH / 2), side: 1, z: zr(E1[0], E1[1]) }, { planeX: r.X(-CH / 2), side: -1, z: zr(W2[0], W2[1]) }],
    };
    // the stairs back up (south end of each void): 1.2 m steps (jump.height 1.5 clears each), the last 0.4 m under the lip
    const stair = (lz, lx0, lx1, takeoff) => {
      r.box("deck", lx0, lz, lx1, lz + 2, PIT, -0.4, "climb", `${takeoff}'s way back from the void, step 3 (0.4 m under the lip): a fall costs a climb`);
      r.box("deck", lx0, lz + 2, lx1, lz + 4, PIT, -1.6, "climb", `${takeoff}'s way back, step 2`);
      r.box("deck", lx0, lz + 4, lx1, lz + 6, PIT, -2.8, "climb", `${takeoff}'s way back, step 1 (1.2 m over the pit floor)`);
    };
    stair(z1, -5, -1, "the lane");
    stair(z3, 1, 5, "D1");
    stair(z5, 0, 3.4, "D2");
    r.row.owns38note = "level 2 owns jump, double, dash, slide, ride, walljump (plan.owns38)";
    r.row.routeVia = [[r.pt(0, 0, z1 - 1), r.pt(0, 0, z2 + 2)], [r.pt(-3, 0, z3 - 1), r.pt(-3, 0, z4 + 2)], [r.pt(-3, 0, z5 - 1), r.pt(-3, 0, z6 + 2)]];
    r.lesson({ kind: "reached", rect: [-5, z6, 5, Lr], minY: -0.5, prompt: "SKATE. DASH OFF THE ICE." });
    for (const z of [10, 25, 40, 52]) r.ceil(0, z, 2, 2, "white", 9, 1.0);
    r.ceil(0, z2 + D1 / 2, 3, 3, "white", 10, 1.0); r.ceil(0, z4 + D2 / 2, 3, 3, "white", 10, 1.0);
    r.pool(0, z2 + D1 / 2, 2.5, "cyan", 8, 1.3); r.pool(0, z4 + D2 / 2, 2.5, "cyan", 8, 1.3);
    r.pool(0, Lr - 2, 2.5, "cyan", 9, 1.8); // the goal, in sight from the lane
    r.walk([[0, 0, 2], [0, 0, z1 - 2]]);
    r.walk([[0, 0, z2 + 2], [0, 0, z3 - 2]]);
    r.walk([[0, 0, z4 + 2], [0, 0, z5 - 2]]);
    r.walk([[0, 0, z6 + 1], [0, 0, Lr - 2]]);
    r.frames([0, 0, 30], [0, 1, z2], [-3, 0, z5 - 1], [0, 2, z6]); // mid: down the lane to the first lip and D1; exit: from D2 across the chain to the exit deck
  }
  // ---------------------------------------------------------------- 2.4 REST (a breath before THE STAG's rehearsals)
  {
    const r = L.room({ id: "j4", name: "2.4 REST", kind: "link", job: "rest", verb: "none", w: 10, l: 10, h: 5, par: 3, cx: CXR, spawnLz: 1.5, exitFromN: 1.5 });
    r.pickup("healthPickup", 0, 5, 0.9, "the rest before THE STAG's rehearsals: full health");
    r.ceil(0, 5, 3, 3, "white", 8, 1.0);
    r.pool(0, 8.5, 2, "cyan", 6, 1.4);
    r.walkSE([]);
    r.frames([0, 0, 2], [0, 0.5, 6], [2, 0, 5], [0, 1, 10]);
  }
  return CXR;
}

/// The n38 plan: n36-lib's own rows (no copied room), the ice, the verbs level 2 owns (builder E's owns38), written.
function writeN38(p) {
  p.id = "n38-iceline";
  p.name = src.name;
  p.n29 = src.n29; p.n30 = src.n30;
  p.wallHeight = src.wallHeight;
  p.surfaces = ICE;
  p.owns38 = ["jump", "double", "dash", "slide", "ride", "walljump"];
  p.owns38_why = "Builder E's gate checker (Design/levels/n38/GATES38.md): level 2 owns the movement verbs, the KESTREL and the PULSE gauntlet; the THUNDERCLAP boost is level 3's, the grapple level 4's (design.n35.grapple35.fromLevel).";
  for (const r of p.rooms) delete r.owns38note;
  fs.mkdirSync(path.join(ROOT, path.dirname(OUT)), { recursive: true });
  fs.writeFileSync(path.join(ROOT, OUT), JSON.stringify(p, null, 1) + "\n");
  console.log("wrote", OUT, p.rooms.length, "rooms (0 copied),", p.boxes.length, "boxes", p.ramps.length, "ramps", p.pits.length, "pits", p.surfaces.length, "surfaces", p.lights.length, "lights", p.props.length, "props; floorSize", p.floorSize);
  const j3 = p.rooms.find((r) => r.id === "j3"), c = p.rooms.find((r) => r.id === "s4");
  console.log("THE RUN: rect", JSON.stringify(j3.rect), "; gates", p.boxes.filter((b) => b.gate).map((b) => b.gate.id).join(","), "; skate", p.boxes.filter((b) => b.skate38).map((b) => b.skate38.id).join(","));
  console.log("THE CANAL: rect", JSON.stringify(c.rect), "centre", JSON.stringify(c.stag37.centre));
}
