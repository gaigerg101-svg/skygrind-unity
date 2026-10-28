// NIGHT38 builder F (NIGHT38-BRIEF card F; PLAN.md section 2 L1; DECIDE38 D-4): level 1's two NEW rooms for n38-foundry, from the beat sheet
// Design/levels/n38/foundry.beats.md (written first). Called by Tools/night/gen-n36-foundry.cjs --n38 after the CRUCIBLE (a5) and before the rest (a6).
//   b1 1.6 THE YARD    an OPEN ROOFED ARENA (36 x 40 x 12): two long ride walls up the middle lane (the line), two side galleries (2.5 m, ramps), enemies
//                      switched on AHEAD on the line (arena36 flow); KESTREL from the walls.
//   b2 1.7 THE GANTRY  a FLIGHT ROOM (20 x 77 x 14): two gates (dry pits GANTRY_GAP long, 4 m deep), each spanned by four ride walls whose faces
//                      alternate (a chain); a ledge L1 between them; no jump the level owns crosses a gate (E's checker proves it: `gate` tags).
// Every solid carries job + why (n36-lib throws otherwise); room-local (lx east of the centre line, lz north of the room's south edge).
const { r3 } = require("./n36-lib.cjs");

/// The gate's gap (m, floor edge to floor edge): beyond every jump level 1 owns (Design/levels/envelopes38.json: the best ground try without the
/// boost, jump + dash + double, 18.83 m), inside the ride's reach. Re-set from E's checker margins (`n38 gates`).
const GANTRY_GAP = Number(process.env.N38F_GAP || 37); // round 3: 37 (was 30): under the ceiling the dash jump + double lands at up to 31.84 m
/// The gantry's ride walls: planes PLANE m either side of the centre line (8 m apart: envelopes38.json facingGapsThatChain 3..11).
/// round 2 (E's checker: its held-ride tries ride the FIRST wall until 1-12 m remain to the landing, then jump off and re-press; four 12 m walls, the
/// first ending 17 m short of the landing, read "ride: does NOT cross" though the walk bot crossed 3 / 3): two walls per gate, a chain of two the
/// checker can fly: W1 (west, its east face) from the lip to 11 m short of the landing, W2 (east, its west face) overlapping its end to the far floor.
/// [side (-1 west / +1 east), from (m past the lip), to] per wall.
// W1 starts 6 m back along the takeoff floor (the run-up beside it: a ride caught at the lip, as a person runs at it)
const WALLS = [[-1, -6, GANTRY_GAP - 11], [1, GANTRY_GAP - 17, GANTRY_GAP - 0.5]];
const PLANE = 4, PIT_D = 4;
/// The checker's takeoff point: this many metres off W1's face (on its run line); N38F_FROM_OFF overrides it for a probe.
const FROM_OFF = Number(process.env.N38F_FROM_OFF || 0.8);
/// round 3 (E's stricter checker, 198ae14d: a DASH JUMP, the jump pressed during a ground dash, plus the double crosses 30.5 m on the +2 m line, landing at
/// 31.6-37.6 m with its apex 2.9-3.4 m up): a CEILING over each gap, its underside GANTRY_CEIL m over the floor (a jump's 1.58 m clears under it, the
/// double's and the dash jump's lift do not), solid up to the roof (no top to stand on); the rides run under it. N38F_CEIL overrides it for a probe.
const GANTRY_CEIL = Number(process.env.N38F_CEIL || 3.6);

module.exports = function n38FoundryRooms(L, MOVE) {
  const ARENA = (r) => { r.row.arena36 = true; };
  if (!MOVE.faceGaps.includes(2 * PLANE)) throw new Error(`a ${2 * PLANE} m facing gap is not in the measured list`);

  // ---------------------------------------------------------------- 1.6 THE YARD (combine, in the open: ride the lane, fire from the walls)
  {
    const r = L.room({ id: "b1", name: "1.6 THE YARD", job: "combine", verb: "KESTREL from the ride walls; ride between the fights", w: 36, l: 40, h: 12, par: 45, spawnLz: 2, exitFromN: 2 });
    ARENA(r);
    r.panel("E", -PLANE, 8, 24, 0, 7, `ride wall W (its east face, ${16} m: about 1.5 s held at ${MOVE.rideSpeed} m/s) up the centre lane: the line; fire the KESTREL from it down the lane (a charged shot pierces the husk line)`);
    r.panel("W", PLANE, 20, 34, 0, 7, `ride wall E (its west face), ${2 * PLANE} m from wall W plane to plane: jump off W across onto it, ride on to the far end of the lane and off onto the floor by the exit`);
    r.box("deck", -18, 12, -12, 32, 0, 2.5, "line", "the west GALLERY (2.5 m): where spitters switch on, seen from the door; the high ground with the health pickup");
    r.box("deck", 12, 12, 18, 32, 0, 2.5, "line", "the east GALLERY (2.5 m): where spitters switch on across the yard; the other high ground");
    r.ramp(-15, 6, 12, 3, 2.5, "climb", "the west gallery's ramp (2.5 m over 6 m): the walkers' way up and down, and the player's");
    r.ramp(15, 6, 12, 3, 2.5, "climb", "the east gallery's ramp (2.5 m over 6 m): the walkers' way up and down, and the player's");
    // the nodes: on the line, AHEAD (lz 14-32 of 40), and on the galleries; every one 12 m or more from the door (spawn31)
    [[0, 14], [-1.5, 18], [1.5, 22], [0, 26], [0, 30], [-8, 28], [8, 28], [-2, 32], [2, 32], [-8, 16]].forEach(([lx, lz]) => r.nodeAt(lx, lz));
    [[-15, 22], [15, 24], [-15, 28], [15, 18]].forEach(([lx, lz]) => r.highNodeAt(lx, lz, 2.5));
    r.row.waves = [["husk", "husk", "husk"], ["spitter", "spitter", "husk", "husk", "husk"], ["spitter", "spitter", "husk", "husk", "husk", "husk"]];
    r.pickup("healthPickup", -15, 30, 3.4, "the west gallery pays health: the high ground you ride or climb to");
    r.ceil(0, 6, 4, 3, "white", 11, 1.1); r.ceil(0, 18, 4, 3, "white", 11, 1.1); r.ceil(0, 30, 4, 3, "white", 11, 1.1);
    r.ceil(-15, 22, 4, 6, "amber", 10, 1.0); r.ceil(15, 22, 4, 6, "white", 10, 1.0); // where the spitters will be (one accent per room: the west one is amber)
    r.trim("W", 20, 6, 3.4, "white", 8, 0.9); r.trim("E", 20, 6, 3.4, "white", 8, 0.9);
    r.pool(0, 38, 2.5, "cyan", 8, 1.6); // the exit, seen down the lane from the door
    r.walkSE([[0, 0, 10], [0, 0, 30]]);
    r.walk([[-15, 0, 4], [-15, 2.5, 14], [-15, 2.5, 30]]);
    r.walk([[15, 0, 4], [15, 2.5, 14], [15, 2.5, 30]]);
    r.frames([-10, 0, 5], [0, 3, 24], [0, 0.5, 34], [0, 1, 40]); // mid: from the door's west side up the lane, both walls and the galleries; exit: the lane's end to the door
  }

  // ---------------------------------------------------------------- 1.7 THE GANTRY (test: the ride chain; two gates no jump crosses)
  {
    // round 2 (E's GATES38.md: the `gate` tag goes on the LANDING solid, the run-up `runUp` 6 m of flat floor behind `from`): the takeoff floor and L1 are
    // 8 m deep (6 m of run-up behind a lip 0.5 m short of the edge), and each landing (L1, the far floor) is a deck box flush with the floor, tagged
    const TAKEOFF = 8, LEDGE = 12, LAND = 10; // LAND 10 (round 2): a chain arrives at up to ~20 m/s; LEDGE 12 (round 3): L1 deep enough to land a 17 m/s hop on
    const g1 = TAKEOFF, g1e = g1 + GANTRY_GAP, g2 = g1e + LEDGE, g2e = g2 + GANTRY_GAP, len = g2e + LAND;
    const r = L.room({ id: "b2", name: "1.7 THE GANTRY", kind: "link", job: "test", verb: "the ride chain (two gates no jump crosses)", w: 20, l: len, h: 14, par: 25, spawnLz: 2, exitFromN: 2 });
    L.plan.owns38 = ["jump", "double", "dash", "slide", "ride", "walljump"]; // level 1: the KESTREL only (no THUNDERCLAP boost), no grapple (design.n35.grapple35.fromLevel 4)
    const PER_GATE = WALLS.length;
    const gate = (lz0, lz1, n, landTo) => {
      r.pit(-10, lz0, 10, lz1, -PIT_D, `gate ${n}: a dry pit ${GANTRY_GAP} m long and ${PIT_D} m deep, wall to wall: no jump level 1 owns crosses it (E's checker); a fall costs the climb back to the gate`);
      const land = r.box("deck", -10, lz1, 10, landTo, -PIT_D, 0, "line", `gate ${n}'s LANDING (${n === 1 ? "L1, the checkpoint ledge between the gates" : "the far floor and the door"}): flush with the floor, the pit's far wall; only the ride chain reaches it`);
      // `from` on W1's run line (lx -PLANE + 0.8, beside its face): the checker's try sprints straight at the lip with the stick toward the wall
      land.gate = { verb: "ride", from: r.pt(-PLANE + FROM_OFF, 0, lz0 - 0.5), to: r.pt(-PLANE + FROM_OFF, 0, lz1 + 2), runUp: 6, id: `b2-g${n}`, why: `gate ${n} of THE GANTRY: only the ride (${PER_GATE} walls, faces alternating) crosses ${GANTRY_GAP} m` };
      WALLS.forEach(([side, a, b], i) => {
        const west = side < 0;
        r.panel(west ? "E" : "W", west ? -PLANE : PLANE, r3(lz0 + a), r3(lz0 + b), -PIT_D, 14,
          `gate ${n} ride wall ${i + 1} of ${PER_GATE} (${west ? "west, its east face" : "east, its west face"}; planes ${2 * PLANE} m apart, ${r3(b - a)} m): the chain alternates faces, pit floor to roof (no top to stand on)`);
      });
      // round 3: the gate's CEILING (E's `ceiling` tag): wall to wall over the gap, its underside GANTRY_CEIL m up, solid to the roof
      const ceil = r.box("hull", -10, lz0, 10, lz1, GANTRY_CEIL, 14, "line", `gate ${n}'s CEILING (underside ${GANTRY_CEIL} m, solid to the roof): it caps the double jump's and the dash jump's lift over the gap, so only the ride carries you across; the walls run up through it`);
      ceil.ceiling = true;
      // the way back up: three 1.2 m steps up the EAST wall at the pit's south end (round 2: W1 stands in the west half from the takeoff floor; east, no wall blocks the way to them) (jump.height 1.5 clears each), then 0.4 m onto the floor above
      r.box("deck", 7.8, lz0, 10, lz0 + 1.5, -PIT_D, -0.4, "climb", `pit ${n}'s way back, step 3 (0.4 m under the floor): a fall costs a climb back to the gate, never health`);
      r.box("deck", 7.8, lz0 + 1.5, 10, lz0 + 3, -PIT_D, -1.6, "climb", `pit ${n}'s way back, step 2 (1.2 m under step 3)`);
      r.box("deck", 7.8, lz0 + 3, 10, lz0 + 4.5, -PIT_D, -2.8, "climb", `pit ${n}'s way back, step 1 (1.2 m over the pit floor)`);
    };
    gate(g1, g1e, 1, g2);
    gate(g2, g2e, 2, len);
    r.row.routeVia = [[r.pt(0, 0, g1 - 1), r.pt(0, 0, g1e + 2)], [r.pt(0, 0, g2 - 1), r.pt(0, 0, g2e + 2)]]; // the two gates: only the real Mover's ride chain crosses
    r.row.flight38 = true; // E's flight-room tag (GATES38.md): a flight room with no gate is RED
    r.row.flight38Why = `NIGHT38 F: a flight room (PLAN.md section 2): two ${GANTRY_GAP} m gaps that only open to the ride; the goal in sight; a fall costs a climb`;
    // round 1 (n38f walk seed 1 flew from gate 1's last wall straight into gate 2 over L1: a `reached` lesson on L1 needs the feet grounded there, and the
    // lessons go in order, so the room never cleared): one lesson, the far floor; its prompt shows from the door
    r.lesson({ kind: "reached", rect: [-10, g2e, 10, len], minY: -0.2, prompt: "HOLD {jump}: CHAIN THE WALLS." });
    r.ceil(0, 3, 4, 2, "white", 10, 1.0); r.ceil(0, g1e + 3, 4, 2, "white", 10, 1.0); r.ceil(0, len - 3, 4, 2, "white", 10, 1.0);
    // round 3: the roof over the gaps is inside the ceilings now: the gaps lit from the pit floor instead
    r.pool(0, g1 + GANTRY_GAP / 2, 3, "white", 10, 0.8, undefined, -PIT_D); r.pool(0, g2 + GANTRY_GAP / 2, 3, "white", 10, 0.8, undefined, -PIT_D);
    r.pool(0, g1e + 3, 2.5, "cyan", 8, 1.6); // L1: the checkpoint ledge between the gates
    r.pool(0, len - 2, 2.5, "cyan", 8, 1.6); // the exit, seen from the lip down the line of walls
    r.walk([[0, 0, 1], [0, 0, g1 - 1]]);
    r.walk([[0, 0, g1e + 1], [0, 0, g2 - 1]]);
    r.walk([[0, 0, g2e + 1], [0, 0, len - 2]]);
    r.walk([[9, -PIT_D, g1 + 8], [9, 0, g1 - 2]], { jump: true, maxSeconds: 8 }); // pit 1's steps back to the takeoff floor
    r.walk([[9, -PIT_D, g2 + 8], [9, 0, g2 - 2]], { jump: true, maxSeconds: 8 }); // pit 2's steps back up to L1
    r.frames([3, 0, 2], [0, 4, g1e + 3], [0, 0.5, g1e + 1], [0, 2, len - 2]); // mid: from the lip down gate 1's chain to L1; exit: from L1 down gate 2 to the exit
  }
};
module.exports.GANTRY_GAP = GANTRY_GAP;
module.exports.WALLS = WALLS;
