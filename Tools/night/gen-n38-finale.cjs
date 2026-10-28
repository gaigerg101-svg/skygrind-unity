// NIGHT38 builder F (NIGHT38-BRIEF card F; PLAN.md section 2 L6; DECIDE38 D-5): LEVEL 6 THE LAST LINE rebuilt whole, from its beat sheet
// Design/levels/n38/finale.beats.md (written first). It requires ONLY ./n36-lib.cjs: no n29-n35 generator, lib or room. The exam: two open arenas
// (x2 THE FLOOR, x5 THE SPINE) and four gates, one per verb in sequence (x3 ride, x4 boost, x6 grapple, x7 dash jump), each tagged `gate: {verb, from,
// to}` for builder E's checker (`n38 gates`); the boss room x9 is n35-finale's x6 row as it was (boss "glacier", THE STAG on its rink) until THE RIVAL.
// Every solid carries job + why (n36-lib throws otherwise; Tools/night/n36-jobs.cjs checks the plan). Fight rooms carry arena36 (the arena flow).
// Room-local: lx east of the room's centre line, lz north of its south edge; rows in web coordinates (x east, y up, z south: the player walks -z).
// Run: node Tools/night/gen-n38-finale.cjs   (then bash Tools/night/n38f-regen.sh finale)
const { Level, MOVE, r3 } = require("./n36-lib.cjs");

// ---- the gates' numbers (metres), each re-set from E's measured envelopes (Design/levels/envelopes38.json) and the checker's margins
/// x3: the ride gate's gap (floor edge to floor edge) and its two walls (n38f-foundry-rooms.cjs's gantry gate: W1 west from 6 m back on the takeoff floor to
/// 11 m short of the landing, W2 east overlapping its end to the far floor; E's held-ride tries ride the first wall to within 1-12 m of the landing).
const RIDE_GAP = 30, RIDE_WALLS = [[-1, -6, RIDE_GAP - 11], [1, RIDE_GAP - 17, RIDE_GAP - 0.5]];
/// x4: the boost step's height over the floor: above the jump + double jump apex (PLAN.md: jump 1.5 m, double jump x0.9), under it + one boost.
const STEP_H = 3.6;
/// x6: the grapple gate's void: longer than any jump or ride chain (the walls are noride), inside two pulls (~40 m each: arcline.zip 34 m/s x 1.2 s).
const VOID_GAP = 44;
/// x7: the dash-jump gate's gap and its low roof: beyond the slide-hop (12.8 m), inside the jump-dash (21 m); the roof caps the double jump's height.
/// round 2 (E's checker under the 3.6 m roof, x7 --verbose: the roof caps every apex at 1.80 m; the best dash try, jump-dash + double, reaches 16.52 m, the
/// best try without the dash 10.78 m (double@0.6); at 18 m only a boost crossed): the gap 13.5 m (14.0 m from the checker's lip), the dash's margin ~18 %.
const LEAP_GAP = 13.5, LEAP_ROOF = 3.6;
const NORIDE = "noride (builder M's flag: a dull, ridged wall with no cyan trim the held ride cannot use)";

const L = new Level("n38-finale", "LEVEL 6 THE LAST LINE",
  "NIGHT38 builder F (design.n38.finale38 swaps n35-finale for this file): Tools/night/gen-n38-finale.cjs from the beat sheet Design/levels/n38/finale.beats.md. "
  + "The exam rebuilt for flight: a threshold, THE FLOOR (open arena), four gates one per verb (THE RIDE, THE STEP, THE VOID, THE LEAP) around THE SPINE (open arena), a rest, "
  + "and THE STAG's room as n35-finale's (boss glacier) until THE RIVAL. Spacing from " + MOVE.src + " and PLAN.md's envelope guesses until E's envelopes38.json.",
  {
    rank: {
      time: [210, 320, 480, 720], kills: [100, 85, 65, 40], style: [2500, 1500, 700, 250],
      _why: "Unity-only (builder F, NIGHT38): S/A/B/C cuts: time = the beat sheet's summed room times up to the boss (about 160 s) x [1.3, 2, 3, 4.5]; kills and style as n35-finale (NOT calibrated: the walk bot's medians replace them).",
    },
  });
L.plan.owns38 = ["jump", "double", "dash", "slide", "boost", "ride", "walljump", "grapple"]; // the exam: the earns of levels 1-5 and the grapple (fromLevel 4)
const ARENA = (r) => { r.row.arena36 = true; };
const nodes = (r, xs) => xs.forEach(([lx, lz]) => r.nodeAt(lx, lz));
const hook = (r, lx, y, lz) => { (r.row.grappleNodes = r.row.grappleNodes || []).push(r.pt(lx, y, lz)); };
/// the room's own walls (levelgen's ring) must not carry a held ride past the gate: builder M's flag works on campaign boxes only (Research/2026-10-13-
/// wave38/M.md), so each side wall gets a 0.2 m noride LINER box over its whole length and height (dull, no trim: a wall you cannot ride)
const noRideRoom = (r, why) => {
  r.row.noride38 = why;
  for (const [a, b, side] of [[-r.w / 2, -r.w / 2 + 0.2, "west"], [r.w / 2 - 0.2, r.w / 2, "east"]]) {
    const lo = r.pitsOut.reduce((m, p) => Math.min(m, p.floorY), 0);
    const bx = r.box("wall", a, 0, b, r.l, lo, r.h, "line", `the ${side} wall's ${NORIDE} liner: ${why}`);
    bx.noride = true;
  }
};
/// the gate's tag on its LANDING solid (E's GATES38.md): a deck flush with the floor past the gap (its top is the judged height); `from` on the lip, 6 m of run-up
function landing(r, lz0, lz1, depth, verb, from, to, id, why, noride) {
  const d = r.box("deck", -r.w / 2, lz0, r.w / 2, lz1, -depth, 0, "line", `the ${verb} gate's LANDING: flush with the floor, the pit's far wall${noride ? "; its face " + NORIDE : ""}; ${why}`);
  d.gate = { verb, from: r.pt(...from), to: r.pt(...to), runUp: 6, id, why };
  if (noride) d.noride = true;
  return d;
}
/// three 1.2 m steps up the west wall at a pit's south end (jump.height 1.5 clears each), then the rest onto the floor: a fall costs a climb, never health
function stepsBack(r, lz0, depth, n, east) {
  const tops = []; for (let y = -0.4; y > -depth + 0.6; y -= 1.2) tops.push(r3(y));
  const x0 = east ? r.w / 2 - 2.2 : -r.w / 2, x1 = east ? r.w / 2 : -r.w / 2 + 2.2; // east: where no ride wall stands between the pit and the steps (x3)
  tops.forEach((top, i) => r.box("deck", x0, lz0 + i * 1.5, x1, lz0 + (i + 1) * 1.5, -depth, top, "climb",
    `${n}'s way back, step ${tops.length - i} (top ${top} m): a fall costs a climb back to the gate, never health`));
}

// ---------------------------------------------------------------- x1 THRESHOLD (link: the name)
{
  const r = L.room({ id: "x1", name: "6.1 THRESHOLD", kind: "link", job: "link", verb: "walk", w: 10, l: 12, h: 6, par: 4, spawnLz: 2, exitFromN: 2.5 });
  r.lesson({ kind: "reached", rect: [-5, 7, 5, 12], minY: -0.5, prompt: "THE LAST LINE. EVERYTHING YOU LEARNED." });
  r.trim("W", 6, 4, 0.4, "cyan"); r.trim("E", 6, 4, 0.4, "cyan");
  r.ceil(0, 6, 3, 3, "white", 8, 0.9); r.pool(0, 9.5, 3, "cyan", 6, 1.6);
  r.walkSE([]);
  r.frames([0, 0, 2], [0, 1, 12], [0, 0, 8], [0, 1.5, 14]);
}

// ---------------------------------------------------------------- x2 THE FLOOR (combine: open arena 1)
{
  const r = L.room({ id: "x2", name: "6.2 THE FLOOR", job: "combine", verb: "ride the zig-zag, shoot and hook on the move", w: 36, l: 40, h: 14, par: 50, spawnLz: 2, exitFromN: 2 });
  ARENA(r);
  if (!MOVE.faceGaps.includes(10)) throw new Error("a 10 m facing gap is not in the measured list");
  r.panel("E", -5, 8, 22, 0, 7, "ride wall A (its east face) up the floor: the line's first half; fire from it at the husks running at you");
  r.panel("W", 5, 18, 32, 0, 7, "ride wall B (its west face), 10 m from A plane to plane: jump off A across onto it, on to the catwalks");
  r.box("deck", -18, 32, -7, 40, 0, 3, "line", "the west CATWALK wing (3 m): the spitters' stand, seen from the door; the ride's landing; the health pickup");
  r.box("deck", 7, 32, 18, 40, 0, 3, "line", "the east CATWALK wing (3 m): spitters across from the west one; the other landing");
  r.ramp(-12, 26, 32, 3, 3, "climb", "the west wing's ramp (3 m over 6 m): the walkers' way up and down, and the player's");
  r.ramp(12, 26, 32, 3, 3, "climb", "the east wing's ramp (3 m over 6 m)");
  nodes(r, [[0, 14], [-2, 18], [2, 18], [0, 24], [-9, 20], [9, 20], [-14, 24], [14, 24], [0, 30], [-3, 28], [3, 28]]);
  [[-12, 36], [12, 36], [-15, 34], [15, 34]].forEach(([lx, lz]) => r.highNodeAt(lx, lz, 3));
  hook(r, -11, 9, 16); hook(r, 11, 9, 26); // two rings over the floor: a hooked line onto a catwalk
  r.row.waves = [["husk", "husk", "husk", "husk", "swarmer", "swarmer", "swarmer", "swarmer"], ["spitter", "spitter", "husk", "husk", "husk", "husk"], ["swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "spitter", "spitter"]];
  r.pickup("healthPickup", -12, 38, 3.9, "the west catwalk pays health: the high ground the ride lands you on");
  r.ceil(0, 6, 4, 3, "white", 12, 1.1); r.ceil(0, 20, 4, 4, "white", 12, 1.1); r.ceil(-12, 36, 6, 4, "amber", 11, 1.0); r.ceil(12, 36, 6, 4, "white", 11, 1.0);
  r.trim("W", 16, 8, 3.6, "white", 9, 0.9); r.trim("E", 16, 8, 3.6, "white", 9, 0.9);
  r.pool(0, 38, 2.5, "cyan", 8, 1.6);
  r.walkSE([[0, 0, 10], [0, 0, 30]]);
  r.walk([[-12, 0, 22], [-12, 3, 33], [-12, 3, 38]]);
  r.walk([[12, 0, 22], [12, 3, 33], [12, 3, 38]]);
  r.frames([-14, 0, 4], [0, 3, 26], [0, 0.5, 34], [0, 1.5, 41]);
}

// ---------------------------------------------------------------- x3 THE RIDE (gate: the ride chain)
{
  const T = 8, LAND = 6, PLANE = 4, D = 4, PER = RIDE_WALLS.length;
  const r = L.room({ id: "x3", name: "6.3 THE RIDE", kind: "link", job: "test", verb: "ride (the chain)", w: 20, l: T + RIDE_GAP + LAND, h: 14, par: 12, spawnLz: 2, exitFromN: 2 });
  if (!MOVE.faceGaps.includes(2 * PLANE)) throw new Error("an 8 m facing gap is not in the measured list");
  r.pit(-10, T, 10, T + RIDE_GAP, -D, `THE RIDE's gap: ${RIDE_GAP} m, a dry pit ${D} m deep wall to wall: only the ride crosses (E's checker)`);
  landing(r, T + RIDE_GAP, T + RIDE_GAP + LAND, D, "ride", [-PLANE + 0.8, 0, T - 0.5], [-PLANE + 0.8, 0, T + RIDE_GAP + 2], "x3-g1", `level 6's ride gate: ${PER} walls, faces alternating, over ${RIDE_GAP} m`, false);
  RIDE_WALLS.forEach(([side, a, b], i) => {
    const west = side < 0;
    r.panel(west ? "E" : "W", west ? -PLANE : PLANE, r3(T + a), r3(T + b), -D, 14, `the ride gate's wall ${i + 1} of ${PER} (${west ? "west, its east face" : "east, its west face"}, ${r3(b - a)} m): faces alternate, pit floor to roof (no top)`);
  });
  stepsBack(r, T, D, "THE RIDE's pit", true);
  r.row.routeVia = [[r.pt(0, 0, T - 1), r.pt(0, 0, T + RIDE_GAP + 2)]];
  r.row.flight38 = true;
  r.row.flight38Why = "NIGHT38 F: level 6's ride gate (PLAN.md section 2: mixed gates, one per verb)";
  r.lesson({ kind: "reached", rect: [-10, T + RIDE_GAP, 10, T + RIDE_GAP + LAND], minY: -0.2, prompt: "" });
  r.ceil(0, 2.5, 4, 2, "white", 10, 1.0); r.ceil(0, T + RIDE_GAP / 2, 3, 8, "white", 12, 0.8); r.ceil(0, T + RIDE_GAP + 3, 4, 2, "white", 10, 1.0);
  r.pool(0, T + RIDE_GAP + LAND - 2, 2.5, "cyan", 8, 1.6);
  r.walk([[0, 0, 1], [0, 0, T - 1]]);
  r.walk([[0, 0, T + RIDE_GAP + 1], [0, 0, T + RIDE_GAP + LAND - 2]]);
  r.walk([[9, -D, T + 8], [9, 0, T - 2]], { jump: true, maxSeconds: 8 });
  r.frames([3, 0, 2], [0, 4, T + RIDE_GAP + 3], [0, 0.5, T + RIDE_GAP + 1], [0, 1, T + RIDE_GAP + LAND + 1]);
}

// ---------------------------------------------------------------- x4 THE STEP (gate: the THUNDERCLAP's mid-air boost)
{
  // round 1 (levelgen: a step running to the north wall left 0.9 m under the 4.5 m door): the step is 6 m deep, then a drop to the north floor and the door
  const S0 = 12, S1 = 18, L4 = 24;
  const r = L.room({ id: "x4", name: "6.4 THE STEP", kind: "link", job: "test", verb: "boost (jump, double jump, fire the THUNDERCLAP down)", w: 16, l: L4, h: 10, par: 8, spawnLz: 2, exitFromN: 2 });
  noRideRoom(r, "THE STEP's walls: a held ride (ride38) would drift up any wall onto the step; only the boost may lift you there");
  const st = r.box("deck", -8, S0, 8, S1, 0, STEP_H, "line", `THE STEP (${STEP_H} m, 6 m deep) wall to wall: over the jump + double jump's apex, under it + one THUNDERCLAP shot fired down in the air (the boost gate); its face ${NORIDE}; past it a drop to the door`);
  st.noride = true;
  st.gate = { verb: "boost", from: r.pt(0, 0, S0 - 1.2), to: r.pt(0, STEP_H, S0 + 3), runUp: 6, id: "x4-g1", why: "level 6's boost gate: only the mid-air shotgun boost lifts you onto the step" };
  r.row.routeVia = [[r.pt(0, 0, S0 - 1), r.pt(0, STEP_H, S0 + 2)]];
  r.row.flight38 = true;
  r.row.flight38Why = "NIGHT38 F: level 6's boost gate (PLAN.md section 2)";
  // round 2 (levelgen: with the noride liners the player graph no longer rides the side walls past the gate, so a `reached` lesson past it reads
  // "NOT reachable"; the gate is the real Mover's crossing, routeVia): no lesson, the link clears on reaching its exit (level 6 is the exam: no text)
  // r.lesson({ kind: "reached", rect: [-8, S0, 8, S1], minY: STEP_H - 0.3, prompt: "JUMP. FIRE THE THUNDERCLAP DOWN." });
  r.ceil(0, 5, 4, 3, "white", 10, 1.0); r.ceil(0, S0 + 3, 4, 3, "white", 10, 1.0); r.ceil(0, L4 - 3, 3, 2, "white", 8, 0.9);
  r.pool(0, S0 + 3, 2, "cyan", 7, 1.4, undefined, STEP_H); // the step's top
  r.pool(0, L4 - 2, 2.5, "cyan", 8, 1.6); // the door, past the drop
  r.walk([[0, 0, 1], [0, 0, S0 - 1]]);
  r.walk([[0, STEP_H, S0 + 1], [0, 0, L4 - 2]]);
  r.frames([3, 0, 3], [0, STEP_H + 1, S0 + 4], [0, STEP_H, S0 + 2], [0, 1, L4 + 2]);
}

// ---------------------------------------------------------------- x5 THE SPINE (combine: open arena 2, the one rule-breaker)
{
  const r = L.room({ id: "x5", name: "6.5 THE SPINE", job: "combine", verb: "everything, on the move; one heavy", w: 40, l: 40, h: 16, par: 60, spawnLz: 2, exitFromN: 2 });
  ARENA(r);
  const sp = r.panel("E", 0.3, 8, 30, 0, 7, "THE SPINE: one long wall (22 m, about 2 s held) down the arena's middle, both faces ridden: it splits the floor into two lanes; fire from it into either lane");
  sp.ride36.both = true;
  r.box("deck", -20, 32, -8, 40, 0, 2.5, "line", "the north-west DECK (2.5 m): spitters' stand, the spine's west landing, the health pickup");
  r.box("deck", 8, 32, 20, 40, 0, 2.5, "line", "the north-east DECK (2.5 m): spitters' stand, the spine's east landing");
  r.ramp(-14, 26, 32, 3, 2.5, "climb", "the north-west deck's ramp (2.5 m over 6 m): the walkers' way up, and the player's");
  r.ramp(14, 26, 32, 3, 2.5, "climb", "the north-east deck's ramp (2.5 m over 6 m)");
  nodes(r, [[-8, 14], [8, 14], [-6, 20], [6, 20], [-10, 24], [10, 24], [-5, 28], [5, 28], [0, 33], [-3, 35], [3, 35]]);
  [[-14, 36], [14, 36], [-17, 34], [17, 34]].forEach(([lx, lz]) => r.highNodeAt(lx, lz, 2.5));
  hook(r, -10, 10, 18); hook(r, 10, 10, 22); // a ring over each lane: hook across the spine
  r.row.waves = [["husk", "husk", "husk", "husk", "spitter", "spitter"], ["swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "husk", "husk"], ["heavy", "husk", "husk", "husk", "husk"]];
  r.pickup("healthPickup", -14, 38, 3.4, "the north-west deck pays health before the last wave's heavy");
  r.ceil(-10, 10, 4, 4, "white", 13, 1.1); r.ceil(10, 10, 4, 4, "white", 13, 1.1); r.ceil(0, 20, 3, 8, "white", 13, 1.0); r.ceil(-14, 36, 6, 4, "amber", 12, 1.0); r.ceil(14, 36, 6, 4, "white", 12, 1.0);
  r.pool(0, 38, 2.5, "cyan", 8, 1.6);
  r.walkSE([[-6, 0, 6], [-6, 0, 31], [0, 0, 34]]);
  r.walk([[-14, 0, 22], [-14, 2.5, 33], [-14, 2.5, 38]]);
  r.walk([[14, 0, 22], [14, 2.5, 33], [14, 2.5, 38]]);
  r.frames([-16, 0, 4], [0, 3, 22], [0, 0.5, 34], [0, 1.5, 41]);
}

// ---------------------------------------------------------------- x6 THE VOID (gate: the grapple)
{
  const T = 8, LAND = 7, D = 6;
  const r = L.room({ id: "x6", name: "6.6 THE VOID", kind: "link", job: "test", verb: "grapple (hook the rings over the void)", w: 20, l: T + VOID_GAP + LAND, h: 16, par: 12, spawnLz: 2, exitFromN: 2 });
  r.pit(-10, T, 10, T + VOID_GAP, -D, `THE VOID: ${VOID_GAP} m, a dry pit ${D} m deep wall to wall: longer than any jump; no wall to ride (the walls are noride)`);
  noRideRoom(r, "THE VOID's walls: a held ride (ride38) along a side wall would carry you over the void; only the hook may");
  landing(r, T + VOID_GAP, T + VOID_GAP + LAND, D, "grapple", [0, 0, T - 0.5], [0, 0, T + VOID_GAP + 2], "x6-g1", `level 6's grapple gate: ${VOID_GAP} m, two rings over it`, true);
  hook(r, 0, 8, T + 13); hook(r, 0, 8, T + 29);
  stepsBack(r, T, D, "THE VOID's pit");
  r.row.routeVia = [[r.pt(0, 0, T - 1), r.pt(0, 0, T + VOID_GAP + 2)]];
  r.row.flight38 = true;
  r.row.flight38Why = "NIGHT38 F: level 6's grapple gate (PLAN.md section 2)";
  // r.lesson({ kind: "reached", rect: [-10, T + VOID_GAP, 10, T + VOID_GAP + LAND], minY: -0.2, prompt: "" });
  r.ceil(0, 2.5, 4, 2, "white", 10, 1.0); r.ceil(0, T + VOID_GAP + 3, 4, 2, "white", 10, 1.0);
  r.ceil(0, T + 13, 2, 2, "cyan", 10, 0.9); // the rings read from the lip
  r.pool(0, T + VOID_GAP + LAND - 2, 2.5, "cyan", 8, 1.6);
  r.walk([[0, 0, 1], [0, 0, T - 1]]);
  r.walk([[0, 0, T + VOID_GAP + 1], [0, 0, T + VOID_GAP + LAND - 2]]);
  r.walk([[-9, -D, T + 10], [-9, 0, T - 2]], { jump: true, maxSeconds: 10 });
  r.frames([3, 0, 2], [0, 8, T + 13], [0, 0.5, T + VOID_GAP + 1], [0, 1, T + VOID_GAP + LAND + 1]);
}

// ---------------------------------------------------------------- x7 THE LEAP (gate: the dash jump under a low roof)
{
  const T = 8, LAND = 10, D = 3;
  const r = L.room({ id: "x7", name: "6.7 THE LEAP", kind: "link", job: "test", verb: "dash jump (jump, then dash, under a low roof)", w: 14, l: T + LEAP_GAP + LAND, h: LEAP_ROOF, par: 8, spawnLz: 2, exitFromN: 2 });
  r.pit(-7, T, 7, T + LEAP_GAP, -D, `THE LEAP: ${LEAP_GAP} m, a dry pit ${D} m deep, under the room's ${LEAP_ROOF} m roof (the ceiling under the double jump's apex): beyond the slide-hop, inside the jump-dash`);
  noRideRoom(r, "THE LEAP's walls: a held ride along a side wall would carry you over the gap; only the dash jump may");
  landing(r, T + LEAP_GAP, T + LEAP_GAP + LAND, D, "dash", [0, 0, T - 0.5], [0, 0, T + LEAP_GAP + 2], "x7-g1", `level 6's dash-jump gate: ${LEAP_GAP} m under a ${LEAP_ROOF} m roof`, true);
  stepsBack(r, T, D, "THE LEAP's pit");
  r.row.routeVia = [[r.pt(0, 0, T - 1), r.pt(0, 0, T + LEAP_GAP + 2)]];
  r.row.flight38 = true;
  r.row.flight38Why = `NIGHT38 F: level 6's dash-jump gate (PLAN.md section 2: the ${LEAP_ROOF} m roof is the ceiling under the double jump's apex)`;
  // r.lesson({ kind: "reached", rect: [-7, T + LEAP_GAP, 7, T + LEAP_GAP + LAND], minY: -0.2, prompt: "JUMP, THEN DASH." });
  r.ceil(0, 3, 3, 2, "white", 7, 0.9); r.ceil(0, T + LEAP_GAP / 2, 2, 6, "white", 8, 0.7); r.ceil(0, T + LEAP_GAP + 4, 3, 2, "white", 7, 0.9);
  r.pool(0, T + LEAP_GAP + LAND - 2, 2.5, "cyan", 7, 1.6);
  r.walk([[0, 0, 1], [0, 0, T - 1]]);
  r.walk([[0, 0, T + LEAP_GAP + 1], [0, 0, T + LEAP_GAP + LAND - 2]]);
  r.walk([[-6, -D, T + 6], [-6, 0, T - 2]], { jump: true, maxSeconds: 8 });
  r.frames([3, 0, 2], [0, 1.5, T + LEAP_GAP + 2], [0, 0.5, T + LEAP_GAP + 1], [0, 1, T + LEAP_GAP + LAND + 1]);
}

// ---------------------------------------------------------------- x8 REST (the checkpoint; the boss hint)
{
  const r = L.room({ id: "x8", name: "6.8 REST", kind: "link", job: "rest", verb: "none", w: 6, l: 10, h: 4, par: 4, spawnLz: 1.5, exitFromN: 1.5 });
  r.pickup("healthPickup", 0, 4, 0.9, "the last rest before THE STAG: full health for the exam's end");
  // round 2 (levelgen: a `reached` lesson past the gates reads "NOT reachable": the player graph does not make the real Mover's crossings): no lesson
  // r.lesson({ kind: "reached", rect: [-3, 5, 3, 10], minY: -0.5, prompt: "IT CHARGES. DODGE. PUNCH ITS BACK." });
  r.trim("W", 4, 3, 0.4, "amber", 5, 1.1); r.trim("E", 4, 3, 0.4, "white", 5, 1.0);
  r.ceil(0, 4, 2, 2, "white", 6, 0.9); r.pool(0, 8.5, 3, "cyan", 5, 1.4);
  r.walkSE([]);
  r.frames([0, 0, 1.5], [0, 1, 10], [0, 0, 7], [0, 1.5, 12]);
}

// ---------------------------------------------------------------- x9 THE STAG (boss: n35-finale's x6 row as it was, until THE RIVAL: D-5)
{
  const r = L.room({ id: "x9", name: "6.9 THE STAG", kind: "boss", job: "boss", verb: "everything (the exam's end)", w: 28, l: 52, h: 12, par: 120, spawnLz: 4, exitFromN: 2.5 });
  r.row.boss = "glacier";
  r.row.limitReason = "";
  r.trim("W", 10, 6, 1.5, "cyan", 9, 1.2); r.trim("E", 10, 6, 1.5, "cyan", 9, 1.2);
  r.ceil(0, 20, 6, 6, "cyan", 14, 1.0); r.pool(0, 48, 6, "cyan", 10, 1.4);
  r.walkSE([]);
}

L.write("Design/levels/n38/n38-finale.plan.json");
module.exports = { RIDE_GAP, STEP_H, VOID_GAP, LEAP_GAP, LEAP_ROOF };
