// NIGHT35 builder A (NIGHT35.md A1): the helpers shared by Tools/night/gen-n35-*.cjs.
//   - rideRoom(L, base, spec): a LUCIO SEMI-PLATFORMING ROOM (research/lucio.md section 4 and spec rule 12: chains of ride walls with faces that alternate,
//     a gap only a ride crosses, a pillar to circle; cyan strips mark the ride walls: every box of kind "wall" gets WallRideStrips26's strip);
//   - teach(r, id, ...): a teach35 trigger rect in the level's `campaign.teach35` (builder T's format, Core/Lesson25.cs Teach35Table.Rects; the words are
//     design.n35.teach35.lines by id, "l1.fire"; a line with no rect falls back to its room's first entry);
//   - noNodes(r, why): DECIDE35 D-3, a room's grapple nodes removed (the grapple is owned from level 4: design.n35.grapple35.fromLevel), logged on the row.
// Room-local coordinates as n30-lib: lx = metres east of the room's centre line, lz = metres NORTH of its south edge; web coordinates in the rows.
// Spacing numbers (round 2): builder M's wallride35 distances, Design/levels/n35-ride-distances.json (`n35move ride --write` on the real Mover): a held
// ride runs rideSpeed 10.45 m/s (a 10.5 m panel is about 1 s); a jump-off at look pitch 0 lands 4.84 m out from the wall's plane (3.00 at -45, 6.68 at
// +45); two FACING walls 3-12 m apart all chain (5 m: the second ride starts 2.0 m above the release); a wall facing within 30 deg of the last one gives no
// ride (reRideDistance 3 m): chains alternate the faces. Round 1 used the 1.2 s ride (lines34 measure, 9.60 m).
const { r3 } = require("./n30-lib.cjs");

const RIDE = (() => {
  try {
    const d = require("../../Design/levels/n35-ride-distances.json");
    return { speed: d.rideSpeed, out0: d.jumpOff.pitch0.outAtTakeoffHeight, gaps: d.facingGapsThatWork, src: "Design/levels/n35-ride-distances.json (builder M)" };
  } catch (e) { return { speed: 8, out0: 4.8, gaps: [3, 4, 5, 6], src: "fallback (M's file missing)" }; }
})();
/// the facing gap every chain here uses (m, plane to plane): 5, inside M's working list and about one pitch-0 jump-off (4.84 m)
const FACE_GAP = (() => { const g = 5; if (!RIDE.gaps.includes(g)) throw new Error("n35a-lib: a 5 m facing gap is not in M's working list " + RIDE.gaps); return g; })();
/// a panel's length (m): one second of held ride at M's ride speed
const PANEL = r3(Math.round(RIDE.speed * 1.0 * 2) / 2);

/// a teach35 trigger: builder T's format (Core/Lesson25.cs Teach35Table.Rects): the level's `campaign.teach35` rows {id, rect [x0, z0, x1, z1], minY}, the id
/// a design.n35.teach35.lines id ("l1.fire"); the feet inside the rect (at or above minY) show the line once. `room` is noted for the log.
function teach(r, id, lx0, lz0, lx1, lz1, minY = -1) {
  const p = r.level.plan;
  (p.teach35 = p.teach35 || []).push({ id, room: r.id, rect: [r.X(Math.min(lx0, lx1)), r.Z(Math.max(lz0, lz1)), r.X(Math.max(lx0, lx1)), r.Z(Math.min(lz0, lz1))], minY: r3(minY) });
}

function noNodes(r, why) {
  const n = (r.row.grappleNodes || []).length;
  delete r.row.grappleNodes;
  r.row.d3 = `NIGHT35 A D-3: ${n} grapple node(s) removed: ${why}`;
  return n;
}

function dimNodes(r, why) {
  const n = (r.row.grappleNodes || []).length;
  if (n) r.row.d3 = `NIGHT35 A D-3: ${n} grapple node(s) kept as dim locked rings (a tease: the grapple is owned from level 4, design.n35.grapple35.fromLevel; the view dims a ring while it is not owned): ${why}`;
  return n;
}

/// The ride hall (W 18 x L 36 x H 10), built with `base` (Level.prototype.room before every hook) so it lands in the level's line of rooms right where it is called.
/// Round 2 (M's distances; the lead: "a ride line over lava needs a floor to fall to, or the gaps inside 8 m"):
///   entry floor lz 0..6; TWO lava pools west of the lane, lx -9..1: P1 lz 6..13 and P2 lz 16..23 (7 m each), the DRY STRIP lz 13..16 between them (a floor
///   to fall to in the middle of the line: no lava point more than 3.5 m from dry ground); the dry EAST LANE lx 1..9 (the walkers' and the bot's way, the bhop
///   lane); two freestanding RIDE PANELS, PANEL m long (1 s of held ride), FACE_GAP apart plane to plane, staggered: A (lx -7.6..-7.0, lz 5..5+PANEL, ride its
///   EAST face) and B (lx -2.0..-1.4, lz 14..14+PANEL, ride its WEST face): A north > jump off at pitch 0 (4.84 m out) onto B > B north > jump off west onto the
///   DECK (lx -9..-4, lz 24..31, 1.5 m: the ride's carry north lands about 5 m along), the high ground; the deck's ramp (lx -6.5, lz 35 -> 31) is the enemies'
///   climb; a 3 x 3 m PILLAR on the north floor (lx -1..2, lz 27..30) to circle.
///   spec: { id, name, par, waves, prompt, cx (default: the room before's outX) }
function rideRoom(L, base, spec) {
  const prev = L.prev;
  const cx = spec.cx !== undefined ? spec.cx : (prev ? prev.outX : 0);
  const r = base.call(L, { id: spec.id, name: spec.name, kind: "room", w: 18, l: 36, h: 10, cx, outX: cx, par: spec.par || 25, spawnLz: 2, exitFromN: 2.5 });
  const AF = -7.0, BF = AF + FACE_GAP; // A's east face, B's west face (lx)
  r.lava(-9, 6, -3.5, 13, 0.8, "NIGHT35 A ride hall pool 1 (5.5 x 7 m: under panel A only; a jump clears its 0.8 m rim)");
  r.lava(-9, 16, -3.5, 23, 0.8, "NIGHT35 A ride hall pool 2 (5.5 x 7 m: west of panel B, which stands on dry floor); the dry strip lz 13..16 and the floor east of lx -3.5 are the floors to fall to");
  r.box("wall", AF - 0.6, 5, AF, 5 + PANEL, -0.8, 7, `NIGHT35 A ride panel A (${PANEL} m: 1 s of held ride at ${RIDE.speed} m/s; ride its EAST face north)`);
  r.box("wall", BF, 14, BF + 0.6, 14 + PANEL, -0.8, 7, `NIGHT35 A ride panel B (${FACE_GAP} m east of A plane to plane, 9 m north: jump off A across onto its WEST face, ride north, jump off west onto the deck)`);
  r.box("deck", -9, 24, -4, 31, 0, 1.5, "NIGHT35 A the deck (1.5 m): the ride line's landing, the high ground");
  r.ramp(-6.5, 35, 31, 3, 1.5, "NIGHT35 A up to the deck from the north floor (walkers: 4 m for 1.5 m)");
  r.solid("pillar", 0.5, 28.5, 3, 8, 3); // the pillar to circle (3 x 3 m, 8 m tall: its faces ride with wallride35's corner following)
  // cover on the east lane's edges (off the lane's middle lx 4.5..7.5)
  r.solid("crate", 8.2, 9, 1.2, 1.2, 1.2); r.solid("crate", 2.6, 19, 0.8, 1.2, 1.4);
  // enemies: the east lane, the dry strip's east end and the north floor (none on the deck: the bot chased a deck spitter into the lava in round 1)
  [[7, 14], [6, 19], [5, 26], [6, 33], [2.5, 33.5], [4, 14.5]].forEach(([lx, lz]) => r.nodeAt(lx, lz));
  r.row.waves = spec.waves;
  const total = spec.waves.reduce((a, w) => a + w.length, 0);
  r.row.lessons = [{ kind: "husksDead", n: total, prompt: spec.prompt, promptPad: spec.prompt }];
  r.row.reachFrom = r.pt(6, 0, 3);
  r.row.ride35 = { panels: ["A", "B"], panelM: PANEL, faceGapM: FACE_GAP, rideSrc: RIDE.src, line: "A east face north > jump off (pitch 0) across to B west face > B north > jump off west onto the 1.5 m deck", note: "two 7 m pools with a dry strip between them (no lava point more than 3.5 m from dry ground); the east lane is the long way round" };
  // light: the panels carry the cyan ride strips (kind wall); white overheads; the pools' danger glow; cyan pools on the strip and the deck say "land here"
  r.ceil(-3, 10, 3, 3, "white", 10, 0.9); r.ceil(4, 22, 3, 3, "white", 10, 0.9); r.ceil(0, 33, 3, 2, "white", 8, 0.9);
  r.pool(-6.25, 9.5, 2.5, "orange", 7, 1.1, { danger: true }, -0.8); // one accent per room (the level report): pool 1 glows, pool 2 reads by its lava
  r.pool(-4, 14.5, 2, "cyan", 6, 1.3); // the dry strip
  r.pool(2.5, 33, 2.5, "cyan", 7, 1.6); // the exit's pool (the goal)
  r.pool(-6.5, 27.5, 2.5, "cyan", 7, 1.6, undefined, 1.5);
  r.trim("E", 15, 4, 0.4, "cyan");
  // completability (LevelValidate29.Proofs): the east lane to the exit; the ramp onto the deck; out of each pool onto dry ground
  r.walkSE([[6, 0, 4], [6, 0, 24], [3, 0, 33]]);
  r.walk([[-6.5, 0, 35.5], [-6.5, 1.5, 28]]);
  r.walk([[-4, -0.8, 8], [-4, 0, 4]], { jump: true, maxSeconds: 8 });
  r.walk([[-4, -0.8, 19], [-4, 0, 14.5]], { jump: true, maxSeconds: 8 });
  return r;
}

/// hook Level.prototype.room so `make(level, base)` builds a new room right BEFORE the room with id `beforeId` (base = the room() in place when the hook was set)
function insertBefore(P, beforeId, make) {
  const base = P.room;
  P.room = function (s) {
    if (s.id === beforeId) make(this, base);
    return base.call(this, s);
  };
}

module.exports = { RIDE, FACE_GAP, PANEL, teach, noNodes, dimNodes, rideRoom, insertBefore };
