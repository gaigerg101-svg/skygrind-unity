// NIGHT38 builder C2 (NIGHT38.md L5; PLAN.md section 2 "L5 SPIRE"): LEVEL 5 THE SPIRE rebuilt for flight, from its beat sheet
// (Design/levels/n38/spire.beats.md, committed before this file). Rooms v1-v8 are new (n36-lib.cjs: every solid has a job and a why); rooms v9-v11
// (FALLING STONES, THE RING, THE SPIRE) are NIGHT37's, built by the same code as n37-spire (gen-n37-spire-boss.cjs addBossRooms), unchanged.
// The level's verb is the JUMP-DASH (dash, then jump inside the dash); its weapon the TORCH (picked up in v1).
// A GATE ROOM holds one gate (levelgen's pit rule: a miss climbs back to the room's spawn, which is the gate's pad; the door before it is the gate's
// checkpoint): the takeoff PAD (the room row's meter38: the spire38 gate meter sets the stamina bar to one jump-dash there), a dry PIT the room's width
// with a recovery stair along the takeoff face, a CEILING 4.0 m over the takeoff (the jump-dash apex 1.85 m + the 1.8 m capsule + 0.35 m), noride
// LINERS on both side walls over the gap, the landing floor. The ceiling slab carries `gate` {id, verb, from, to, gap, meter} for builder E's checker
// (`n38 gates`). Gate rooms carry `flight38: true` (spire38: the bar does not refill by itself there, only a pad fills it; the hook is off) and the
// LIMIT `pistolOnly` (Campaign24's room limit: the THUNDERCLAP's mid-air boost is not a way across a jump-dash gate).
// Web coordinates (x east, y up, z south). Run: node Tools/night/gen-n38-spire.cjs, then bash Tools/night/n38c-regen.sh.
const path = require("path");
const fs = require("fs");
const { Level, MOVE, r3 } = require("./n36-lib.cjs");
const { addBossRooms, floorFor, writePlan } = require("./gen-n37-spire-boss.cjs");

const ROOT = path.join(__dirname, "../..");
const OUT = path.join(ROOT, "Design/levels/n38/spire.plan.json");
const CFG = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/unity-config.json"), "utf8"));
const N33 = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/levels/n33/spire.plan.json"), "utf8"));
const CAPSULE = JSON.parse(fs.readFileSync(path.join(ROOT, "PortExport/config.json"), "utf8")).collision.capsuleHeight;

// the gate's shape (design.n38.spire38.gate: Tools/night/n38c-config.cjs writes them with their _why; the defaults are the beat sheet's)
const G = (CFG.design.n38.spire38 && CFG.design.n38.spire38.gate) || {};
// every edge on a whole metre (the nav grid's cell centres sit on half metres: an edge on a half metre puts a cell on it, levelgen's pit rule fails)
const GAP_TEACH = +(process.env.C2_GAP_TEACH || G.gapTeach || 17); // m, BELFRY I's gap (the first gate: 19 % under the jump-dash's 21 m)
const GAP = +(process.env.C2_GAP || G.gap || 18); // m, every other gate's (D-6: >= 10 % under the jump-dash's 21 m = at most 18.9 m, over every other line's reach)
const CEIL = +(process.env.C2_CEIL || G.ceilingOverTakeoff || 4.0); // m, the ceiling's underside over the takeoff
const PIT = G.pitDepth || 5; // m under the takeoff
const PAD = G.pad || 10; // m of pad before the edge
const LAND = +(process.env.C2_LAND || G.landing || 7); // m of landing floor past the gap

const CX = -10; // every room on v9's centre line (the door d8 into v9 is x -12..-8)
const V9_Z1 = -170, DOOR = 4;
const gateRoom = (id, name, verb, gap, par) => ({ id, name, kind: "link", job: "test", verb, w: 12, l: PAD + gap + LAND, h: 9, par, spawnLz: 2, exitFromN: 2, gap38: gap });

// the rooms' lengths first: the level is placed so its last room's north wall is the door d8 into v9 (z -166)
const SPECS = [
  { id: "v1", name: "5.1 THE LANDING", kind: "link", job: "teach", verb: "the TORCH (pick up, burn close)", w: 14, l: 22, h: 9, par: 12, spawnLz: 2, exitFromN: 2 },
  { id: "v2", name: "5.2 THE NAVE", job: "practise", verb: "an open roofed arena: TORCH close, jump-dash to the aisles", w: 30, l: 38, h: 14, par: 45, spawnLz: 2, exitFromN: 2 },
  gateRoom("v3", "5.3 BELFRY I", "the jump-dash gate, taught: dash, then jump inside the dash, under the ceiling", GAP_TEACH, 8),
  gateRoom("v4", "5.4 BELFRY II", "a jump-dash gate (one per pad)", GAP, 6),
  gateRoom("v5", "5.5 BELFRY III", "a jump-dash gate (one per pad)", GAP, 6),
  { id: "v6", name: "5.6 THE CHOIR", job: "combine", verb: "an open roofed arena: jump-dash between three heights, TORCH, parry", w: 34, l: 42, h: 16, par: 60, spawnLz: 2, exitFromN: 2 },
  gateRoom("v7", "5.7 THE LONG BELL", "the last jump-dash gate (the exam before the rehearsals)", GAP, 8),
  { id: "v8", name: "5.8 THE VESTRY", kind: "link", job: "rest", verb: "none", w: 12, l: 12, h: 6, par: 4, spawnLz: 1.5, exitFromN: 1.5 },
];
SPECS.forEach((s) => { s.cx = CX; });
const TOTAL = SPECS.reduce((a, s) => a + s.l, 0) + DOOR * (SPECS.length - 1);
const START = r3(V9_Z1 + DOOR + TOTAL); // the first room's south wall (z): the last room's north wall lands on z -166

const L = new Level("n38-spire", "LEVEL 5 THE SPIRE",
  "NIGHT38 builder C2 (design.n38.spire38 swaps n37-spire for this file while the row is on): Tools/night/gen-n38-spire.cjs from the beat sheet "
  + "Design/levels/n38/spire.beats.md. Eight new rooms (v1 THE LANDING: the TORCH; v2 THE NAVE and v6 THE CHOIR: open roofed arenas; v3-v5 BELFRY I-III "
  + "and v7 THE LONG BELL: one jump-dash gate each under a ceiling, a gate meter on each pad; v8 THE VESTRY: a rest), then NIGHT37's v9 FALLING STONES, "
  + "v10 THE RING and v11 THE SPIRE (THE CROWN), unchanged. Spacing from " + MOVE.src + " and the real sim's jumps (Harness crown37 jumps).",
  { killPlaneY: -95, rank: N33.rank, n29: true, n30: true });
L.cursor = START;
const ARENA = (r) => { r.row.arena36 = true; };

/// the room's one jump-dash gate: the pad (lz 0..PAD), the pit, the ceiling, the liners, the recovery stair, the pad row and the gate tag
const gates = [];
function gateIn(r, gap) {
  const id = "G", w = r.w, x0 = -w / 2, x1 = w / 2, lz0 = PAD, lz1 = lz0 + gap, pitY = -PIT;
  r.row.flight38 = true;
  r.row.limit = ["pistolOnly"];
  r.row.limitReason = "ONE JUMP-DASH PER PAD";
  r.pit(x0, lz0, x1, lz1, pitY, `the GAP, ${gap} m (a jump-dash crosses; nothing else the player owns here does); a dry pit, a miss costs the stair's climb back to the pad`);
  const cy = CEIL;
  // the far CEILING step (C2_SPLIT m past the lip, C2_CEIL2 m over the takeoff): low over the gap's far part, where the jump-dash is already coming
  // down and a late double jump's second rise is what carries the other lines (round 3 experiment; off unless both are set)
  const split = +(process.env.C2_SPLIT || G.lowFrom || 0), cy2 = +(process.env.C2_CEIL2 || G.lowCeiling || 0);
  if (split > 0 && cy2 > 0) {
    const low = r.box("hull", x0, lz0 + split, x1, lz1, cy2, cy, "line", `the far CEILING step (${cy2} m over the takeoff, from ${split} m past the lip): the jump-dash is already coming down there; it stops a late double jump's second rise`);
    low.ceiling = true;
  }
  const slab = r.box("hull", x0, lz0 - 1, x1, lz1, cy, r.h, "line", `the CEILING (${cy} m over the takeoff; ${CAPSULE} m capsule): the jump-dash's apex (1.85 m) passes under it; it caps a double jump's and a jump's rise`);
  slab.ceiling = true;
  // the LANDING solid (E's checker judges the gate on the solid it carries: GATES38.md): a 1 m deck flush with the floor over the landing
  const land = r.box("deck", x0 + 1, lz1, x1 - 1, r.l - 1, -1, 0, "line", `the LANDING past the gap (flush with the floor): the gate's landing solid (builder E's checker judges the jump-dash on it)`);
  const from = r.pt(0, 0, lz0 - 0.1), to = r.pt(0, 0, lz1 + 2);
  land.gate = { id: `${r.id}.${id}`, verb: "dashjump", from, to, runUp: 8, gap, meter: 1, why: `room ${r.id}'s gate: the jump-dash, one per pad (spire38's meter)` };
  gates.push(land.gate);
  for (const [a, b, side] of [[x0, x0 + 1, "west"], [x1 - 1, x1, "east"]]) {
    const liner = r.box("hull", a, lz0 - 1, b, lz1, pitY, cy, "line", `the ${side} LINER over the gap (1 m thick): a dull wall that can't be ridden (noride): a wall ride is not a way across`);
    liner.noride = true;
  }
  // the recovery stair: steps of at most 1.25 m (jump.height is MOVE.jumpH) along the takeoff face, each 3 m wide (x) and 3 m deep (z): within 3 m
  // of the edge, so no step is a way across (from the top step's far end the gap is still gap - 3 m, 1.25 m up)
  const n = Math.ceil(PIT / 1.25 - 1e-9) - 1, d = PIT / (n + 1);
  for (let k = 1, lx = x0 + 1; k <= n; k++, lx += 3) {
    const top = r3(-k * d);
    r.box("deck", lx, lz0, lx + 3, lz0 + 3, pitY, top, "climb", `the recovery stair, step ${k} (top ${top} m): the way back up to the pad after a miss (each step <= 1.25 m)`);
  }
  // the pad: the gate meter (spire38) sets the bar to one jump-dash while the feet are on it
  r.row.meter38 = [{ gate: `${r.id}.${id}`, rect: r.rect(x0, 0, x1, lz0), y: 0, dashJumps: 1, why: "the PAD: the bar set to one jump-dash (spire38's meter)" }];
  r.pool(0, lz0 / 2 + 1, 4, "cyan", 7, 1.5);
  r.light("ceiling", [r.X(0), r3(cy - 0.05), r.Z(lz0 + 0.5)], [0, -1, 0], [w - 1, 0.4, 0.1], "white", 6, 0.9); // the slab's lit front edge: the gate reads
  r.row.routeVia = [[r.pt(0, 0, lz0 - 0.5), r.pt(0, 0, lz1 + 1)]];
  r.pool(0, r.l - 2.5, 2.5, "cyan", 7, 1.6);
  r.ceil(0, 4, 3, 3, "white", 9, 1.0);
  r.walk([[0, 0, 2], [0, 0, lz0 - 1]]);
  r.walk([[0, 0, lz1 + 1.5], [0, 0, r.l - 2]]);
  r.frames([0, 0, 2], [0, 1, lz1], [0, 0, lz1 + 2], [0, 1, r.l + 2]);
  return { lz0, lz1 };
}

// ---------------------------------------------------------------- 5.1 THE LANDING (teach, calm: the TORCH)
{
  const r = L.room(SPECS[0]);
  r.pickup("weaponPickup", 0, 6, 1.0, "the TORCH on the axis, in view from the spawn: walk onto it (the level's weapon, VERBS row 33)");
  r.propsOut[r.propsOut.length - 1].weapon = "torch";
  [[-1.5, 11], [0, 12], [1.5, 11]].forEach(([lx, lz]) => r.standIn(lx, 0, lz, "a stand-in in a tight row, 5 m past the TORCH: burn it up close"));
  // the alcove: the stand-ins stand between two short walls (the TORCH's close range reads; a gun from the door is the long way)
  r.box("hull", -7, 9, -3, 14, 0, 9, "cover", "the alcove's west wall: the stand-ins are met up close, as the TORCH wants");
  r.box("hull", 3, 9, 7, 14, 0, 9, "cover", "the alcove's east wall");
  r.lesson({ kind: "reached", rect: [-7, 16, 7, r.l], minY: -0.2, prompt: "THE TORCH: HOLD {fire} CLOSE" });
  r.ceil(0, 6, 3, 2, "white", 8, 1.1); r.ceil(0, 11.5, 4, 2, "amber", 7, 1.0);
  r.pool(0, r.l - 2.5, 2.5, "cyan", 7, 1.6);
  r.walkSE([[0, 0, 8], [0, 0, 16]]);
  r.frames([0, 0, 3], [0, 1, 12], [0, 0, 16], [0, 1, r.l + 2]);
}

// ---------------------------------------------------------------- 5.2 THE NAVE (practise: the open roofed arena)
{
  const r = L.room(SPECS[1]);
  ARENA(r);
  const DY = 2.4; // the aisles' deck height: a ramp of 0.4 m per metre (the walkers' slope) over 6 m
  for (const s of [-1, 1]) {
    const a = s < 0 ? -15 : 9, b = s < 0 ? -9 : 15, side = s < 0 ? "west" : "east";
    r.box("deck", a, 6, b, 32, 0, DY, "line", `the ${side} AISLE (${DY} m): the spitters' stand and the high ground a jump-dash from the nave reaches`);
    r.ramp(s * 12, 0, 6, 3, DY, "climb", `the ${side} aisle's south ramp (the walkers' way up and down: ${DY} m over 6 m)`);
    r.ramp(s * 12, 38, 32, 3, DY, "climb", `the ${side} aisle's north ramp (the walkers' way down to the exit end)`);
    for (const lz of [12, 24]) r.solid("pillar", s * 5, lz, 1.6, 14, 1.6, "cover", `a nave PILLAR (${side}, lz ${lz}): cover to break a spitter's line, floor to roof`);
  }
  [[0, 14], [-4, 18], [4, 18], [0, 26], [-4, 30], [4, 30]].forEach(([lx, lz]) => r.nodeAt(lx, lz));
  [[-12, 16], [12, 16], [-12, 26], [12, 26]].forEach(([lx, lz]) => r.highNodeAt(lx, lz, DY));
  r.row.waves = [["husk", "husk", "husk", "swarmer", "swarmer"], ["spitter", "spitter", "husk", "husk"], ["heavy", "swarmer", "swarmer", "swarmer"]];
  r.pickup("healthPickup", 12, 30, DY + 0.9, "the high ground pays health: the east aisle's north end");
  r.ceil(0, 8, 4, 3, "white", 12, 1.2); r.ceil(0, 20, 4, 3, "white", 12, 1.2); r.ceil(0, 32, 4, 3, "white", 12, 1.2);
  r.trim("W", 19, 24, DY + 1.0, "amber", 8, 1.0); r.trim("E", 19, 24, DY + 1.0, "white", 8, 1.0); // one accent per room (levelgen)
  r.pool(0, 36, 2.5, "cyan", 7, 1.6);
  r.walkSE([[0, 0, 8], [-7, 0, 18], [-7, 0, 30], [0, 0, 34]]);
  r.walk([[12, 0, 1], [12, DY, 8], [12, DY, 28]]);
  r.frames([0, 0, 4], [0, 2, 30], [12, DY, 28], [0, 1, 37]);
}

// ---------------------------------------------------------------- 5.3-5.5 BELFRY I-III (test: the flight rooms, one gate each)
{
  const r = L.room(SPECS[2]);
  const g = gateIn(r, SPECS[2].gap38);
  r.lesson({ kind: "reached", rect: [-6, g.lz1, 6, r.l], minY: -0.2, prompt: "{dash} DASH, THEN {jump}: FLY" });
}
for (const i of [3, 4]) {
  const r = L.room(SPECS[i]);
  gateIn(r, SPECS[i].gap38);
}

// ---------------------------------------------------------------- 5.6 THE CHOIR (combine: the big roofed arena, three heights)
{
  const r = L.room(SPECS[5]);
  ARENA(r);
  const GY = 2.4, DAIS = 1.5; // the galleries at 2.4 m: a 0.4 m-per-metre ramp over 6 m (the walkers' slope; 3.2 m over 8 m left the galleries unreached by walkers in levelgen)
  for (const s of [-1, 1]) {
    const a = s < 0 ? -17 : 11, b = s < 0 ? -11 : 17, side = s < 0 ? "west" : "east";
    r.box("deck", a, 6, b, 36, 0, GY, "line", `the ${side} GALLERY (${GY} m): spitters' stand; a jump-dash from the dais's edge (5 m across, 0.9 m up) lands on it`);
    r.ramp(s * 14, 0, 6, 3, GY, "climb", `the ${side} gallery's south ramp (the walkers' way up: ${GY} m over 6 m)`);
    r.ramp(s * 14, 42, 36, 3, GY, "climb", `the ${side} gallery's north ramp (the walkers' way down to the exit end)`);
    for (const lz of [6, 36]) r.solid("pillar", s * 8, lz, 1.8, 16, 1.8, "cover", `a CHOIR pillar (${side}, lz ${lz}): cover off the dais's line, floor to roof`);
  }
  r.box("deck", -6, 14, 6, 26, 0, DAIS, "line", `the DAIS (${DAIS} m) in the middle: the high centre to jump-dash from onto either gallery`);
  r.ramp(0, 10, 14, 4, DAIS, "climb", "the dais's south ramp (1.5 m over 4 m: the walkers' way up)");
  r.ramp(0, 30, 26, 4, DAIS, "climb", "the dais's north ramp");
  [[-8, 20], [8, 20], [-4, 32], [4, 32], [0, 37]].forEach(([lx, lz]) => r.nodeAt(lx, lz));
  [[-14, 18], [14, 18], [-14, 28], [14, 28]].forEach(([lx, lz]) => r.highNodeAt(lx, lz, GY));
  r.highNodeAt(0, 20, DAIS);
  r.row.waves = [["husk", "husk", "husk", "husk", "swarmer", "swarmer"], ["spitter", "spitter", "husk", "husk", "heavy"], ["heavy", "heavy", "swarmer", "swarmer", "swarmer", "swarmer"]];
  r.pickup("healthPickup", 0, 20, DAIS + 0.9, "the dais pays health: the centre you fight from");
  r.ceil(0, 20, 6, 6, "white", 14, 1.3); r.ceil(-14, 21, 3, 10, "amber", 10, 1.0); r.ceil(14, 21, 3, 10, "white", 10, 1.0);
  r.pool(0, 40, 2.5, "cyan", 7, 1.6);
  r.walkSE([[0, 0, 9], [0, DAIS, 18], [0, DAIS, 25], [0, 0, 31], [0, 0, 38]]);
  r.walk([[14, 0, 1], [14, GY, 12], [14, GY, 30]]);
  r.frames([0, DAIS, 16], [14, GY + 1, 22], [0, 0, 36], [0, 1, 43]);
}

// ---------------------------------------------------------------- 5.7 THE LONG BELL (test: the longest gate)
{
  const r = L.room(SPECS[6]);
  gateIn(r, SPECS[6].gap38);
}

// ---------------------------------------------------------------- 5.8 THE VESTRY (rest: the checkpoint before the rehearsals)
{
  const r = L.room(SPECS[7]);
  r.pickup("healthPickup", 0, 6, 0.9, "the last rest before FALLING STONES, THE RING and THE CROWN: full health");
  r.ceil(0, 6, 3, 3, "white", 8, 1.0);
  r.pool(0, 10.5, 2, "cyan", 6, 1.4);
  r.walkSE([]);
  r.frames([0, 0, 2], [0, 0.5, 8], [2, 0, 6], [0, 1, 12]);
}

const plan = L.finish();
plan.surfaces = [];
const last = L.rooms[L.rooms.length - 1];
if (Math.abs(last.z0 - (V9_Z1 + DOOR)) > 1e-6) throw new Error(`the last room ends at z ${last.z0}, not ${V9_Z1 + DOOR}: the door into v9 would not fit`);
plan.doors.push({ id: "d8", between: [last.id, "v9"], rect: [CX - 2, V9_Z1, CX + 2, r3(last.z0)], height: 4.5, checkpoint: true });
plan.rooms.push(...addBossRooms(plan));
plan.floorSize = floorFor(plan, plan.floorSize);
plan.gates38 = { _why: "builder C2: every gate in the level (the landing solids' `gate` tags), for builder E's checker and the report", list: gates };
// the verbs the player has AT THE GATES (E's checker, GATES38.md): every gate stands in a flight38 room, where the LIMIT pistolOnly takes the THUNDERCLAP
// (no boost) and spire38's meter.noHook turns the grapple off (Game/SurvivalSim.N38.Spire.cs; harness `spire38 meter` proves both)
plan.owns38 = ["jump", "double", "dash", "slide", "ride", "walljump"];
plan.owns38_why = "builder C2: the gates are all in flight38 rooms: no boost (LIMIT pistolOnly) and no grapple (spire38 meter.noHook); the shockwave is off game-wide (shockwaveOff25)";
writePlan(plan, OUT);
console.log(`gates: ${gates.length} (${gates.map((g) => `${g.id} ${g.gap} m`).join(", ")}); level z ${START} .. ${V9_Z1 + DOOR} then v9-v11`);
