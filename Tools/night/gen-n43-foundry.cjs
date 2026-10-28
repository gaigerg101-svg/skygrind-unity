// NIGHT43 builder L1 (foundry43; NIGHT43.md L1: GG G1, G2, G4, G11, + G9's rules on level 1; DECIDE43 D-5 the lava's cost, D-8 backtracking;
// docs/DESIGN-RULES.md A-1 "every piece has a reason", A-3 one-way, A-4 a fall costs time and a little health, M-5 flight times, T-3 / T-4 / T-5).
// Level 1 as n43-foundry: n42-foundry's plan (Design/levels/n42/foundry.plan.json, written by Tools/night/gen-n42-foundry.cjs, which this file does
// not run or edit) copied whole, with three rooms rebuilt on the copy, so `--off43` keeps n42-foundry bit for bit:
//   b2 THE GANTRY (G1): the two pits are LAVA (the surface 0.8 m under the floor: LEVEL-FORMAT's lava band); a fall costs FALL_DAMAGE and puts you
//      back at the lip you jumped from (E1's room field `fallReturn43`, read only with rooms43 on; its `by` rows pick the lip per pit). Gate 1 is ONE
//      long wall (learn the hold: ride it to the far landing). Gate 2 is the chain: walls alternating across a GATE2_LANE m lane (the next wall in
//      view, <= ~0.5 s of flight between walls at ride speed, overlaps >= 4 m) with ONE DASH GAP: the east line breaks for DASH_GAP m (24 m/s x
//      0.25 s = 6 m) and continues; the card "{jump} WALL TO WALL. {dash} ACROSS". Every rideable face has a cyan strip; the lava glows.
//   a7 THE FLUE (G2): a CHIMNEY: two continuous facing ride walls FLUE_LANE m apart and FLUE_H m tall (no short panels, no 1 m overlaps:
//      CODE-FACTS-A B6), so wall to wall carries you up (chimney43 refills each new wall) and held rides with the look up climb too; the room's own
//      side walls are noride (only the chimney climbs); the top deck leads forward only: its north edge is an 8 m drop to the door (the drop gate,
//      G11); the exit lit.
//   a8 PIERCE (G4): no floating stand-ins. The lesson in order (T-4): "HOLD {jump} ON A WALL: RIDE" -> the ride is performed (a jump-off) -> the
//      husks arrive (the wave waits for lesson 0: E1's `afterLesson43`, read only with rooms43 on) out of a hatch on the far floor, spawning in a
//      line ahead of P2 -> "HOLD {altFire}: SLOW. {fire}: PIERCE" (a shot from the wall). P2 moved 4 m south so the far floor is 7 m deep: d10 is a
//      straight approach. A walkers' stair up the far side (enemies reach every floor).
// Every piece written here carries a one-line gameplay reason in `why` (n36-jobs.cjs checks it).
// The checks: bash Tools/night/n43-foundry-regen.sh (n36-jobs, levelgen, the copies), then
//   dotnet run --project Tools/SimHarness -c Release -- n43level gates | cross | walk | teach   (Tools/SimHarness/Harness.N43.Level.cs)
// Probes: N43F_<KNOB> env vars override the numbers below; a probe must write to N43F_OUT (never the shipped plan).
// Run from the repo (or worktree) root: bash Tools/night/n43-foundry-regen.sh
const path = require("path");
const fs = require("fs");

const ROOT = path.join(__dirname, "../..");
const SRC = path.join(ROOT, "Design/levels/n42/foundry.plan.json");
const OUT = path.join(ROOT, "Design/levels/n43/foundry.plan.json");
const env = (k, d) => (process.env["N43F_" + k] !== undefined ? Number(process.env["N43F_" + k]) : d);
const KNOBS = {
  /// the lava surface (m under the floor): LEVEL-FORMAT's band 0.6-1.0 (a burn's knock-up clears the rim); a3 / a5 use 0.8
  LAVA_Y: env("LAVA_Y", -1.0),
  /// THE GANTRY's ceilings' underside (n42: 3.6)
  CEIL_Y: env("CEIL_Y", 4.1),
  /// feet under this (m) inside THE GANTRY's pits: back to the lip (E1's fallReturn43.belowY), 0.5 m over the lava (the return comes before the burn)
  FALL_BELOW: env("FALL_BELOW", -0.3),
  /// DECIDE43 D-5: a fall into the lava costs this much health and returns you to the lip (never a death)
  FALL_DAMAGE: env("FALL_DAMAGE", 10),
  /// gate 1's single wall: its east face at x -GATE1_PLANE (n42's west plane), from 6 m before the lip to GATE1_TO m past it
  GATE1_PLANE: env("GATE1_PLANE", 3),
  GATE1_TO: env("GATE1_TO", 36.5),
  /// gate 2's lane: the walls' planes at x +-GATE2_LANE / 2 (4.5 m: (4.5 - 0.7) / 8 m/s out ~ 0.48 s between walls: M-5)
  GATE2_LANE: env("GATE2_LANE", 4.5),
  /// gate 2's walls, m past the lip (west W1, east W2, the dash gap, east W3, west W4)
  G2_W1_TO: env("G2_W1_TO", 12),
  G2_W2_FROM: env("G2_W2_FROM", 8),
  G2_W2_TO: env("G2_W2_TO", 18),
  /// the dash gap: the east line breaks for this many metres (one dash: 24 m/s x 0.25 s = 6 m)
  DASH_GAP: env("DASH_GAP", 5),
  G2_W3_TO: env("G2_W3_TO", 31),
  G2_W4_FROM: env("G2_W4_FROM", 27),
  G2_W4_TO: env("G2_W4_TO", 36.5),
  /// THE FLUE: the chimney's lane (plane to plane) and its walls' height; the walls from FLUE_FROM to FLUE_TO m north of the room's south edge
  FLUE_LANE: env("FLUE_LANE", 3.5),
  FLUE_H: env("FLUE_H", 12),
  /// THE FLUE's lid over the chimney (its underside, m; 0: none)
  LID_Y: env("LID_Y", 10.3),
  FLUE_FROM: env("FLUE_FROM", 1.5),
  FLUE_TO: env("FLUE_TO", 25),
  /// THE FLUE's top deck (its top, m) and its depth (m north of the chimney's end)
  DECK_Y: env("DECK_Y", 8),
  DECK_D: env("DECK_D", 4),
  /// PIERCE: the pit's north edge (z), P1's north end, P2's south end, the perch [x0, z0, x1, z1] (an island at P1's end, 8 m past the lip)
  PIT_N: env("PIT_N", -360),
  P1_N: env("P1_N", -349.5),
  P2_S: env("P2_S", -353),
  PERCH: [env("PERCH_X0", -1), env("PERCH_Z0", -357.5), env("PERCH_X1", 4), env("PERCH_Z1", -353)],
};
const OUT_FILE = process.env.N43F_OUT ? path.resolve(ROOT, process.env.N43F_OUT) : OUT;
const probe = Object.keys(process.env).some((k) => k.startsWith("N43F_") && k !== "N43F_OUT");
if (probe && OUT_FILE === OUT) throw new Error("a probe (N43F_* knobs) must write to N43F_OUT, not the shipped plan");

const r2 = (v) => +v.toFixed(2);
const plan = JSON.parse(fs.readFileSync(SRC, "utf8"));
if (plan.id !== "n42-foundry") throw new Error(`unexpected source id ${plan.id}`);
plan.id = "n43-foundry";
plan._why = "NIGHT43 builder L1 (design.n43.foundry43 swaps n42-foundry for this file while the row is on; GG G1 / G2 / G4 / G11, DECIDE43 D-5 / D-8): "
  + "Tools/night/gen-n43-foundry.cjs copies n42-foundry's plan whole and rebuilds THE GANTRY (b2: lava, one long wall, then the chain with a dash gap), "
  + "THE FLUE (a7: a chimney) and PIERCE (a8: the lesson in order, the husks arrive after the ride). n42-foundry's _why: " + plan._why;

const room = (id) => {
  const r = plan.rooms.find((q) => q.id === id);
  if (!r) throw new Error(`room ${id} missing`);
  return r;
};
const dropZone = (zone, keep) => { plan.boxes = plan.boxes.filter((b) => b.zone !== zone || keep(b)); };
const box = (zone, kind, min, max, job, why, extra) => {
  const b = Object.assign({ zone, kind, min: min.map(r2), max: max.map(r2), job, why, note: why }, extra || {});
  plan.boxes.push(b);
  return b;
};
const light = (o) => { plan.lights.push(Object.assign({ normal: [0, 1, 0] }, o)); };
const dropLights = (roomId, ids) => { plan.lights = plan.lights.filter((l) => !(l.room === roomId && ids.includes(l.id))); };
const lesson = (kind, prompt, extra) => Object.assign({ promptPad: prompt, kind }, extra || {}, { prompt });
const changed = [];

// ============================================================================================ b2 THE GANTRY (G1)
{
  const b2 = room("b2");
  const RZ1 = b2.rect[3]; // -184: lz (ride36) is metres north of it
  const lands = ["b2-g1", "b2-g2"].map((gid) => {
    const land = plan.boxes.find((b) => b.zone === "b2" && b.gate && b.gate.id === gid);
    if (!land) throw new Error(`gate ${gid} missing`);
    const lipZ = land.gate.from[2] - 0.5; // as gen-n42-foundry.cjs: the plan's `from` is 0.5 m short of the lip
    return { gid, land, lipZ, landZ: land.max[2], gap: r2(land.gate.from[2] - 0.5 - land.max[2]) };
  });
  // everything n42 built in the pits goes: the ride walls (rebuilt below) and the climb steps / return stairs (the lava's rim is the way out, and
  // with rooms43 on the fall puts you back at the lip). The landings (gate solids), the ceilings and the noride liners stay.
  const before = plan.boxes.filter((b) => b.zone === "b2").length;
  dropZone("b2", (b) => !(b.ride || b.job === "climb"));
  const removed = before - plan.boxes.filter((b) => b.zone === "b2").length;
  for (const p of plan.pits.filter((q) => q.zone === "b2")) {
    const gi = p.rect[3] > -230 ? 1 : 2;
    p.floorY = KNOBS.LAVA_Y;
    p.lava = true;
    p.why = `gate ${gi}'s LAVA (NIGHT43 L1, foundry43; GG G1, DECIDE43 D-5): 37 m wall to wall, the surface ${-KNOBS.LAVA_Y} m under the floor: a miss reads `
      + `as a miss (it glows and lights the walls from below: A-5) and costs ${KNOBS.FALL_DAMAGE} health and the lip (fallReturn43), never a long walk back`;
    p.note = p.why;
  }
  // the ceilings over the pits (n38's anti-cheat: no jump / double / dash jump crosses 37 m under them): their underside at CEIL_Y
  for (const c of plan.boxes.filter((b) => b.zone === "b2" && b.ceiling)) {
    c.min = [c.min[0], KNOBS.CEIL_Y, c.min[2]];
    c.why = `the gate's CEILING (underside ${KNOBS.CEIL_Y} m, solid to the roof; NIGHT43 L1: n42's 3.6 m bonked a jump-off from a ride at 1.8 m, `
      + "the head at the slab): it still caps the double jump's and the dash jump's lift over the lava, so only the ride carries you across";
    c.note = c.why;
  }
  const thick = 0.6;
  const at = (g, m) => r2(g.lipZ - m); // world z m past the lip
  const lz = (z) => r2(RZ1 - z);
  const wall = (g, face, planeX, fromM, toM, why) => {
    const x0 = face === "E" ? planeX - thick : planeX, x1 = face === "E" ? planeX : planeX + thick;
    const zA = at(g, toM), zB = at(g, fromM);
    box("b2", "wall", [x0, -1.8, zA], [x1, 14, zB], "line", why, { ride: true, ride36: { face, planeX, lz: [lz(zB), lz(zA)] } });
    // its cyan strip (V-3: a ridable wall looks ridable), 0.9 m over the floor along the whole face
    light({ id: `b2-R${plan.lights.filter((l) => l.room === "b2" && /^b2-R/.test(l.id)).length + 1}`, room: "b2", kind: "wall", at: [r2(planeX + (face === "E" ? 0.05 : -0.05)), 0.9, r2((zA + zB) / 2)],
      normal: [face === "E" ? 1 : -1, 0, 0], size: [r2(zB - zA), 0.2, 0.1], color: "cyan", range: 6, intensity: 1.1 });
  };
  const [g1, g2] = lands;
  // gate 1: ONE long wall (learn the hold)
  const P1 = KNOBS.GATE1_PLANE;
  wall(g1, "E", -P1, -6, KNOBS.GATE1_TO, `b2-g1's ONE wall (NIGHT43 L1, foundry43; GG G1: "the first span a single long wall"): its east face at x ${-P1}, from 6 m before `
    + `the lip to ${KNOBS.GATE1_TO} m past it (the landing at ${g1.gap} m): hold {jump} into it and it carries you all the way; the first span teaches the hold, nothing else (T-5)`);
  // gate 2: the chain, a narrower lane, the dash gap on the east line
  const H = KNOBS.GATE2_LANE / 2;
  const gapFrom = KNOBS.G2_W2_TO, gapTo = r2(KNOBS.G2_W2_TO + KNOBS.DASH_GAP);
  wall(g2, "E", -H, -6, KNOBS.G2_W1_TO, `b2-g2 W1 (west, face x ${-H}; NIGHT43 L1): from the landing to ${KNOBS.G2_W1_TO} m: the chain's first wall; the lane is ${KNOBS.GATE2_LANE} m `
    + `so a jump-off reaches the next wall in ~0.5 s at ride speed (M-5)`);
  wall(g2, "W", H, KNOBS.G2_W2_FROM, KNOBS.G2_W2_TO, `b2-g2 W2 (east, face x ${H}): ${KNOBS.G2_W2_FROM}-${KNOBS.G2_W2_TO} m, ${r2(KNOBS.G2_W1_TO - KNOBS.G2_W2_FROM)} m of overlap `
    + `with W1 (the next wall always in view: >= 4 m); its north end is the DASH GAP's lip`);
  wall(g2, "W", H, gapTo, KNOBS.G2_W3_TO, `b2-g2 W3 (east, W2's line): ${gapTo}-${KNOBS.G2_W3_TO} m, after the ${KNOBS.DASH_GAP} m DASH GAP (${gapFrom}-${gapTo} m: no wall, lava under): `
    + "a dash along W2's line (24 m/s x 0.25 s) carries you onto it, a jump-off from W2's end finds no west wall there (W4 starts later)");
  wall(g2, "E", -H, KNOBS.G2_W4_FROM, KNOBS.G2_W4_TO, `b2-g2 W4 (west): ${KNOBS.G2_W4_FROM}-${KNOBS.G2_W4_TO} m, ${r2(KNOBS.G2_W3_TO - KNOBS.G2_W4_FROM)} m of overlap with W3: `
    + "the last hop, its end over the far landing's lip");
  // the gates' run-up lines 0.8 m off the first wall's face
  g1.land.gate = Object.assign({}, g1.land.gate, { from: [r2(-P1 + 0.8), 0, g1.land.gate.from[2]], to: [r2(-P1 + 0.8), 0, g1.land.gate.to[2]],
    why: "gate 1 of THE GANTRY (NIGHT43): ONE long wall: only the held ride crosses 37 m of lava" });
  g2.land.gate = Object.assign({}, g2.land.gate, { from: [r2(-H + 0.8), 0, g2.land.gate.from[2]], to: [r2(-H + 0.8), 0, g2.land.gate.to[2]],
    why: "gate 2 of THE GANTRY (NIGHT43): the chain (walls alternating) with one dash gap: only the ride (and its dash) crosses 37 m of lava" });
  g1.land.why = "gate 1's LANDING (the ledge between the lavas): flush with the floor; only the held ride on the one long wall reaches it";
  g2.land.why = "gate 2's LANDING (the far floor and the door d7): flush with the floor; only the chain (and the dash across its gap) reaches it";
  g1.land.note = g1.land.why;
  g2.land.note = g2.land.why;
  // the lava lights the room from below (orange danger pools on the surface), replacing n42's white pit-floor pools
  dropLights("b2", ["b2-L4", "b2-L5"]);
  // (one accent light per room: LevelValidate29's lights row; gate 1's, the first seen; the lava surface glows by itself in both pits)
  light({ id: "b2-lava", room: "b2", kind: "pool", at: [0, r2(KNOBS.LAVA_Y + 0.025), r2((g1.lipZ + g1.landZ) / 2)], size: [6, 10, 0.05],
    color: "orange", range: 12, intensity: 1.3, danger: true });
  // the dash gap's far wall start lit (the place to aim the dash at)
  light({ id: "b2-dash", room: "b2", kind: "wall", at: [r2(H - 0.05), 2.2, at(g2, gapTo + 1)], normal: [-1, 0, 0], size: [2, 0.4, 0.1], color: "cyan", range: 7, intensity: 1.6 });
  // the lessons (the centre card, T-2 / T-3): gate 1 "hold"; gate 2 "wall to wall, dash across"; each a `reached` on its far landing
  const l1 = lesson("reached", "HOLD {jump} ON THE WALL", { rect: [-10, r2(g1.landZ - 12), 10, g1.landZ], minY: -0.2 });
  const l2 = lesson("reached", "{jump} WALL TO WALL. {dash} ACROSS", { rect: [-10, -288, 10, g2.landZ], minY: -0.2 });
  b2.lessons = [l1, l2];
  b2.lessons31 = JSON.parse(JSON.stringify(b2.lessons));
  // E1's room field (rooms43): a fall into the lava -> back to the lip of the pit you fell into, FALL_DAMAGE health, a short fade (DECIDE43 D-5)
  const lipSpot = (g, x) => [x, 0, r2(g.lipZ + 2)];
  b2.fallReturn43 = {
    belowY: KNOBS.FALL_BELOW, to: lipSpot(g1, r2(-P1 + 0.8)), yawDeg: 0, damage: KNOBS.FALL_DAMAGE,
    by: lands.map((g, i) => ({ rect: [-10, g.landZ, 10, g.lipZ], to: lipSpot(g, i === 0 ? r2(-P1 + 0.8) : r2(-H + 0.8)), yawDeg: 0 })),
    _why: `NIGHT43 L1 (DECIDE43 D-5, A-4): feet under ${KNOBS.FALL_BELOW} m in a pit (the lava at ${KNOBS.LAVA_Y}) -> back to that pit's lip (2 m before it, on the run-up `
      + `line), ${KNOBS.FALL_DAMAGE} health, a short fade; \`by\` picks the lip per pit (rect [x0, z0, x1, z1]); \`to\` is the default (gate 1's)`,
  };
  b2.proofs = (b2.proofs || []).filter((p) => !(p.points || []).some((q) => q[1] < -0.5)); // n42's way-back proofs (the stairs are gone)
  b2.verb36 = "the held ride (one long wall), then the chain with a dash gap";
  b2.flight38Why = (b2.flight38Why || "") + " NIGHT43 L1: lava, gate 1 one long wall, gate 2 the chain with a dash gap; a fall returns you to the lip.";
  b2.foundry43_why = `L1 (foundry43): ${removed} n42 pieces out (the walls and the stairs); lava at ${KNOBS.LAVA_Y}; g1 one wall -6..${KNOBS.GATE1_TO} m at x ${-P1}; `
    + `g2 lane ${KNOBS.GATE2_LANE} m: W1 -6..${KNOBS.G2_W1_TO}, W2 ${KNOBS.G2_W2_FROM}..${KNOBS.G2_W2_TO}, dash gap ${gapFrom}..${gapTo}, W3 ${gapTo}..${KNOBS.G2_W3_TO}, `
    + `W4 ${KNOBS.G2_W4_FROM}..${KNOBS.G2_W4_TO}`;
  changed.push("b2: " + b2.foundry43_why);
}

// ============================================================================================ a7 THE FLUE (G2, G11)
{
  const a7 = room("a7");
  const [RX0, RZ0, RX1, RZ1] = a7.rect; // -6, -338, 6, -306
  dropZone("a7", () => false);
  const H = KNOBS.FLUE_LANE / 2, T = 0.6;
  const zS = r2(RZ1 - KNOBS.FLUE_FROM), zN = r2(RZ1 - KNOBS.FLUE_TO);
  const deckN = r2(zN - KNOBS.DECK_D);
  for (const [face, x0, x1, px] of [["E", -H - T, -H, -H], ["W", H, H + T, H]]) {
    const why = `THE FLUE's ${face === "E" ? "west" : "east"} chimney wall (NIGHT43 L1, foundry43; GG G2): one continuous ${KNOBS.FLUE_H} m face, ${KNOBS.FLUE_LANE} m from the other `
      + `(no short panels, no 1 m overlaps): wall to wall carries you up (chimney43: a new wall refills the wall jump) and a held ride looking up climbs too`;
    box("a7", "wall", [x0, 0, zN], [x1, KNOBS.FLUE_H, zS], "line", why, { ride: true, ride36: { face, planeX: px, lz: [KNOBS.FLUE_FROM, KNOBS.FLUE_TO] } });
    for (const y of [1.2, 5.2, 9.2]) light({ id: `a7-R${face}${y}`, room: "a7", kind: "wall", at: [r2(px + (face === "E" ? 0.05 : -0.05)), y, r2((zS + zN) / 2)], normal: [face === "E" ? 1 : -1, 0, 0],
      size: [r2(zS - zN), 0.2, 0.1], color: "cyan", range: 5, intensity: 1 });
  }
  // the room's own side walls dull: the chimney is the only climb (a ride on the room wall with the look up rises ~5 m in 20 m: never the deck)
  for (const [x0, x1] of [[RX0, RX0 + 0.6], [RX1 - 0.6, RX1]])
    box("a7", "wall", [x0, 0, zN], [x1, 13, zS], "line", "THE FLUE's side LINER (NIGHT43 L1): the room's wall, noride and dull, so the chimney is the climb (V-3: a wall that "
      + "cannot be ridden looks it)", { noride: true });
  box("a7", "deck", [RX0, 0, deckN], [RX1, KNOBS.DECK_Y, zN], "line", `THE FLUE's top DECK (${KNOBS.DECK_Y} m), wall to wall at the chimney's top: only the climb reaches it; its north `
    + `edge is an ${KNOBS.DECK_Y} m DROP to the door (G11, DECIDE43 D-8: the way forward only, the climb behind cannot be walked back to from the door)`);
  if (KNOBS.LID_Y > 0)
    box("a7", "hull", [-H - T, KNOBS.LID_Y, zN], [H + T, 13, zS], "line", `THE FLUE's LID (underside ${KNOBS.LID_Y} m over the chimney; NIGHT43 L1): measured (n43level flue), `
      + `a climb that tops the 12 m walls mid-flue falls off their top and back to the floor (12 of 30): under the lid the climb levels out at the deck's height `
      + "and the ride carries you along to it; the flue's cap reads as its top");
  // a lip on the deck's back edge at the chimney's mouth would block the arrival: the drop is the gate. The exit lit (A-6).
  dropLights("a7", ["a7-L3", "a7-L4", "a7-L5"]);
  light({ id: "a7-L3", room: "a7", kind: "pool", at: [0, 0.025, r2(zS - 1)], size: [2, 2, 0.05], color: "cyan", range: 6, intensity: 1.2 });
  light({ id: "a7-L5", room: "a7", kind: "pool", at: [0, r2(KNOBS.DECK_Y + 0.025), r2((zN + deckN) / 2)], size: [2.5, 2.5, 0.05], color: "cyan", range: 7, intensity: 1.6 });
  a7.lessons = [
    lesson("reached", "LOOK UP. {jump} WALL TO WALL", { rect: [-1.5, r2(RZ1 - 1.4), 1.5, RZ1], minY: -0.2 }), // the floor at the lane's mouth (the plates and the lid start 1.5 m in)
    // the deck or the floor beyond it (only the climb gets over the deck; a fast climb can fly over its 4 m and land by the door: 2 of 30 measured)
    lesson("reached", "", { rect: [RX0, RZ0, RX1, zN], minY: -0.2 }),
  ];
  a7.lessons31 = JSON.parse(JSON.stringify(a7.lessons));
  a7.routeVia = [[[0, 0, r2(zS + 1)], [0, KNOBS.DECK_Y, r2((zN + deckN) / 2)]]];
  a7.proofs = [{ kind: "walk", points: [[0, 0, -308], [0, 0, r2(zS + 1)]] }, { kind: "walk", points: [[0, KNOBS.DECK_Y, r2((zN + deckN) / 2)], [0, 0, -336.5]] }];
  a7.frames36 = { mid: [0, 0, r2(zS + 2), 0, 0.45], exit: [0, KNOBS.DECK_Y, r2((zN + deckN) / 2), 0, -0.35] };
  a7.verb36 = "the chimney: wall to wall up (presses or held rides, the look up)";
  a7.foundry43_why = `L1 (foundry43): the chimney x +-${H} (lane ${KNOBS.FLUE_LANE}), z ${zN}..${zS}, ${KNOBS.FLUE_H} m tall; deck ${KNOBS.DECK_Y} m z ${deckN}..${zN}; noride side liners`;
  changed.push("a7: " + a7.foundry43_why);
}

// ============================================================================================ a8 PIERCE (G4)
// Room x -7..7, z -364..-342 (the entry floor z -345..-342, the far floor z -364..-360, d10 north of it). Three beats (T-4: teach, confirm, test):
//   1. "HOLD {jump} ON A WALL: RIDE": P1 (west) carries you from the lip to the PERCH (an island in the pit at P1's end) = lesson 0 (reached);
//   2. the husks come out of the HATCH over the far floor (the wave waits for lesson 0: E1's afterLesson43), in a line ahead of P2's rider;
//   3. "HOLD {altFire}: SLOW. {fire}: PIERCE": from the perch onto P2 (east), riding north at the line: a shot from the wall (lesson 1).
{
  const a8 = room("a8");
  const pit = plan.pits.find((p) => p.zone === "a8");
  pit.rect = [-7, KNOBS.PIT_N, 7, -345];
  pit.why = `the PIERCE pit (4 m deep, dry; NIGHT43 L1: z ${KNOBS.PIT_N}..-345): crossed by two rides with a perch between; a fall costs the climb up the entry side's steps`;
  pit.note = pit.why;
  const p1 = plan.boxes.find((b) => b.zone === "a8" && b.ride && b.ride36 && b.ride36.face === "E");
  const p2 = plan.boxes.find((b) => b.zone === "a8" && b.ride && b.ride36 && b.ride36.face === "W");
  const RZ1 = a8.rect[3]; // -342
  const setZ = (b, zA, zB) => { b.min = [b.min[0], b.min[1], zA]; b.max = [b.max[0], b.max[1], zB]; b.ride36 = Object.assign({}, b.ride36, { lz: [r2(RZ1 - zB), r2(RZ1 - zA)] }); };
  setZ(p1, KNOBS.P1_N, -345); // from the lip (measured: with n42's 1 m gap at its south end the run-up rode round it onto its back face)
  p1.why = `ride panel P1 (west, z ${KNOBS.P1_N}..-345; NIGHT43 L1): beat 1, the room's first ride: hold {jump} on it from the lip, let go near its end: the jump-off lands on the PERCH (measured: a ride jump-off carries ~6 m along, ~5 m out)`;
  p1.note = p1.why;
  setZ(p2, KNOBS.PIT_N, KNOBS.P2_S);
  p2.why = `ride panel P2 (east, z ${KNOBS.PIT_N}..${KNOBS.P2_S}; NIGHT43 L1): beat 3: from the perch onto it, riding north you face the hatch's line of husks: `
    + "hold {altFire} to slow, {fire} to pierce the line; it ends over the far floor, the door ahead";
  p2.note = p2.why;
  const [px0, pz0, px1, pz1] = KNOBS.PERCH;
  box("a8", "deck", [px0, -4, pz0], [px1, 0, pz1], "line", "the PERCH (NIGHT43 L1): an island in the pit where P1's jump-off lands, flush with the floor, P2 along its east edge: beat 1's confirm (you rode "
    + "there: lesson 0), the place you stand when the hatch opens, and the run-up onto P2");
  // no floating stand-ins (G4, E-6): the husks arrive after the ride, out of a hatch over the far floor, in a line ahead of P2's rider
  delete a8.standIns;
  delete a8.standIns31;
  delete a8.standIns36;
  a8.kind = "room";
  a8.arena36 = true;
  const rideX = r2(p2.min[0] - 0.35), rideZ = r2((p2.min[2] + p2.max[2]) / 2);
  const line = [4.5, 5.7, 6.9].map((d) => [r2(rideX - d * 0.15), 0, r2(rideZ - d)]); // on a ray from P2's mid-panel rider, 0.15 m west per metre north
  a8.nodes = line;
  a8.reachFrom = [0, 0, -362]; // the far floor: the hatch's husks walk from there (no walker crosses the pit), LevelValidate24's walkers row
  a8.waves = [["husk", "husk", "husk"]];
  // E1's room field (rooms43; Core/Campaign24.cs RoomDef.afterLesson43: a number = wave 0 waits for that lesson index)
  a8.afterLesson43 = 0;
  a8.afterLesson43_why = "NIGHT43 L1 (G4, T-4): wave 0 waits for lesson 0 (the ride to the perch): teach, confirm, then the enemies out of the hatch";
  const perchRect = [px0, pz0, px1, pz1];
  a8.lessons = [
    lesson("reached", "HOLD {jump} ON A WALL: RIDE", { rect: perchRect, minY: -0.2 }),
    // a shot (NIGHT42's a8 kind): measured, the `wallRiddenAndFired` kind needs a `ride` proof, and LevelValidate29's bare web Mover never rides P1 (0 s riding,
    // three run-ups tried): the room's shape asks for the shot from P2 (the husks in a line ahead of its rider)
    lesson("shots", "HOLD {altFire}: SLOW. {fire}: PIERCE", { n: 1 }),
  ];
  a8.lessons31 = JSON.parse(JSON.stringify(a8.lessons));
  const perchMid = [r2((px0 + px1) / 2), 0, r2((pz0 + pz1) / 2)];
  a8.routeVia = [[[0, 0, -344.5], perchMid], [perchMid, [0, 0, -362]]];
  a8.proofs = [
    { kind: "walk", points: [[0, 0, -343.5], [3, 0, -344.5]] },
    { kind: "walk", points: [[0, 0, -361], [0, 0, -363]] },
  ];
  a8.frames36 = { mid: [r2(p1.max[0] + 0.35), 0.6, -347, -8, 0], exit: [0, 0, -361, 0, -0.05] };
  a8.verb36 = "the ride to a perch, then a shot from the wall pierces the husks' line";
  a8.par = Math.max(a8.par || 0, 24);
  dropLights("a8", ["a8-L3"]);
  light({ id: "a8-L3", room: "a8", kind: "pool", at: [perchMid[0], 0.025, perchMid[2]], size: [2, 2, 0.05], color: "cyan", range: 6, intensity: 1.2 });
  light({ id: "a8-hatch", room: "a8", kind: "ceiling", at: [line[1][0], 8.95, line[1][2]], normal: [0, -1, 0], size: [2.5, 3, 0.1], color: "orange", range: 8, intensity: 1.2, danger: true });
  a8.foundry43_why = `L1 (foundry43): pit z ${KNOBS.PIT_N}..-345, P1 z ${KNOBS.P1_N}..-345, P2 z ${KNOBS.PIT_N}..${KNOBS.P2_S}, perch ${JSON.stringify(perchRect)}; stand-ins out; `
    + `wave [husk x3] at ${JSON.stringify(line)} after lesson 0`;
  changed.push("a8: " + a8.foundry43_why);
}


fs.mkdirSync(path.dirname(OUT_FILE), { recursive: true });
fs.writeFileSync(OUT_FILE, JSON.stringify(plan, null, 1) + "\n");
console.log("wrote", path.relative(ROOT, OUT_FILE), "|", changed.join(" | "));
