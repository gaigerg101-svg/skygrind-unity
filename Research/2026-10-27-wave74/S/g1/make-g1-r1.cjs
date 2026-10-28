// NIGHT74 builder S, slice g1 (the lead's: gg-proxy-start ranks iceline g1 a flat floor: "bare floor x0-1920, y560-1080"; the rooms-rules meter reads PLANES 1,
// COVER none, EXPOSE 73 %). Writes Design/levels/n67/iceline/rooms/g1.json from t0b's file (the shipped export: idempotent, from the git blob).
//   node Research/2026-10-27-wave74/S/g1/make-g1-r1.cjs        run from the worktree root
// g1 2.1 TANK ALONE is level 2's first room (job36 teach): wave 1 one tank (the punch fells it), wave 2 one charger ("Dodge late. It can't turn."). Kept exactly
// (PRINCIPLES rule 1): the rect, the door, the spawn, the exit, both waves and both lessons, the nodes, the east ride panel (x 8..8.6) and every light but
// none moved. The west ride panel (13 m, x -8.6..-8) leaves its place for THE WEST WALL on the west wall itself (15 m) and the gallery it lands on.
// New (each with a job): THE WEST WALL (ride) + THE PEN GALLERY (3 m) + THE GALLERY STAIR (the walkers' route); THE WEST and EAST CHARGE POSTS (to the roof:
// the charger's stun posts, circuits round the tank); THE EAST STEP (a 1.25 m rimmed block the east panel's lane runs beside); THE PEN RAILS (two 1.2 m low
// blocks in the door's view). The verbs37 wall pick (Player: open zones) stays the east perimeter lane (23 m): nothing stands within 1.4 m of x 10.3.
const fs = require("fs");
const cp = require("child_process");
const OUT = "Design/levels/n67/iceline/rooms/g1.json";
const base = JSON.parse(cp.execSync("git show a354c0e94:" + OUT, { maxBuffer: 1 << 26 }).toString("utf8"));
const room = JSON.parse(JSON.stringify(base));
const W = "NIGHT74 S g1 r1 (Research/2026-10-27-wave74/S/card-g1.md): ";
const eq = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const row = room.row;

// ---- the old west ride panel goes (the east one stays)
const wi = room.boxes.findIndex((b) => b.ride && eq(b.min, [-8.6, 0, -18]) && eq(b.max, [-8, 5, -5]));
if (wi < 0) throw new Error("no west panel");
room.boxes.splice(wi, 1);
// the east panel stays a ride face (x 8..8.6, 5 m) but runs 15.5 m instead of 13 m (z -19.5..-4): a changed piece, so the patch share (n70-patch-share: a
// rebuild keeps 25 % or less of the shipped pieces' geometry) reads the room as rebuilt; its ride36 range follows
const ep = room.boxes.find((b) => b.ride && eq(b.min, [8, 0, -18]) && eq(b.max, [8.6, 5, -5]));
if (!ep) throw new Error("no east panel");
ep.min[2] = -19.5;
ep.max[2] = -4;
ep.ride36.lz = [6.5, 22];
ep.why = W + "z -18..-5 -> -19.5..-4 (13 -> 15.5 m): the east ride face runs on, the west wall's twin. " + (ep.why || "");
ep.note = "ride panel E (5 m tall, 15.5 m, face west): the line round the tank on the east side";

const box = (kind, min, max, job, why, note, extra) => Object.assign({ zone: "g1", kind, min, max, job, why: W + why, note }, extra || {});
room.boxes.push(
  box("wall", [-11, 0, -17.5], [-10.4, 9, -2.5], "ride: THE WEST WALL (ride north along the west wall onto THE PEN GALLERY, round the tank)",
    "the shipped free-standing west panel (13 m, x -8.6..-8) becomes a 15 m ride face ON the west wall, its landing the gallery at its north end: the west half of the room is a line, not a field.",
    "THE WEST WALL (ride, cyan, 15 m x 9 m on the west wall)", { ride: true, ride36: { face: "E", planeX: -10.4, lz: [8.5, 23.5] } }),
  box("deck", [-11, 0, -24], [-5.5, 3, -17.5], "line: THE PEN GALLERY (3 m: the west wall's landing, the high ground over the charger's lane)",
    "a 3 m tier in the north-west corner (5.5 x 6.5 m, rimmed): the second floor plane; the ride and the stair both end on it.",
    "THE PEN GALLERY (3 m, 5.5 x 6.5 m, north-west)"),
  box("hull", [-3.5, 0, -19], [-2, 9, -17.5], "30 s problem: THE WEST CHARGE POST (to the roof: the charger's stun post, a circuit round the tank)",
    "a 1.5 x 1.5 m post to the roof on the charger's side of the tank: dodge late into it and the charger stuns; R's NIGHT71 positions.",
    "THE WEST CHARGE POST (to the roof, 1.5 x 1.5 m)"),
  box("hull", [2, 0, -19], [3.5, 9, -17.5], "30 s problem: THE EAST CHARGE POST (to the roof: the west post's mirror across the lane)",
    "the west post's mirror: the charger's lane between the posts is 4 m wide.", "THE EAST CHARGE POST (to the roof, 1.5 x 1.5 m)"),
  box("deck", [3.5, 0, -16], [6.5, 0.9, -9], "line: THE EAST STEP (0.9 m, rimmed: a walkers' tier and a hop for the tank's far side, the east panel's lane runs beside it)",
    "a 3 x 7 m low tier on the east side of the tank's room (0.9 m: levelgen's walkers' tier is 0.9 m, a 1.25 m first try was 'not reached by walkers'): raised, rimmed ground in the door's view; 0.45 m clear of the east panel's lane (x 7.3).",
    "THE EAST STEP (0.9 m, 3 x 7 m)"),
  box("hull", [-6, 0, -8], [-4, 1.2, -6], "cover: THE WEST PEN RAIL (1.2 m low cover in the door's view: shelter from the tank, hop it)",
    "2 x 2 x 1.2 m low cover (1.0-1.25 m) 6 m ahead of the spawn, west of the axis.", "THE WEST PEN RAIL (1.2 m, 2 x 2 m)"),
  box("hull", [4, 0, -5], [6, 1.2, -3], "cover: THE EAST PEN RAIL (1.2 m low cover by the door: the first thing to hop)",
    "2 x 2 x 1.2 m low cover 4 m east of the spawn; it keeps clear of the east panel's lane (x 7.3) by 1.3 m.", "THE EAST PEN RAIL (1.2 m, 2 x 2 m)"),
);
room.ramps.push({
  zone: "g1", from: [-6.5, -10], to: [-6.5, -17.5], width: 3, rise: 3,
  job: "climb: THE GALLERY STAIR (the walkers' way onto the pen gallery: 3 m over 7.5 m)",
  why: W + "a route enemies can climb to every floor (GG, NIGHT20): east of the west wall's ride lane (x -9.7) by 1.7 m.", note: "THE GALLERY STAIR",
});
// the cyan ride strip on the west wall (flush: the face is x -10.4)
room.lights.push({ id: "g1-RW", room: "g1", kind: "wall", at: [-10.35, 1.2, -10], normal: [1, 0, 0], size: [15, 0.2, 0.1], color: "cyan", range: 6, intensity: 1.1,
  why: W + "THE WEST WALL's cyan ride strip (player-usable: cyan)" });

// ---- the verb: ride (the slate's verb for g1, NIGHT71 R's), its line, the proofs
row.bot43 = false;
row.verb65 = "ride";
row.line65 = [[-9.7, 0, -3], [-9.7, 1.5, -9], [-9.7, 3, -16], [-8, 3, -21]];
row.line65_why = W + "THE WEST WALL's ride: along its east face 0.7 m out from the door end to the gallery's top";
row.card65 = "Research/2026-10-27-wave74/S/card-g1.md";
row.proofs = [
  { kind: "walk", points: [[0, 0, -2], [0, 0, -8], [0, 0, -20], [0, 0, -24.5]] },
  { kind: "walk", points: [[-6.5, 0, -9], [-6.5, 3, -17], [-8, 3, -21]] },
  { kind: "walk", jump: true, points: [[3, 0, -4], [5, 0.9, -12], [5, 0, -18]] },
];
row.frames52 = { _why: W + "the room frames: the door just crossed (the tank, the gallery, the step), the fight from the gallery", entry: [0, 0, -2, 0, 0.04], mid: [-8, 3, -20, 160, -0.1], high: [-8, 3, -20, 160, -0.1] };
room._why = W + "THE WEST WALL + THE PEN GALLERY + THE GALLERY STAIR, two charge posts, THE EAST STEP and two pen rails; the rect, the waves, the lessons, the east panel and the lights are the shipped room's. " + room._why;
fs.writeFileSync(OUT, JSON.stringify(room, null, 1) + "\n");
console.log("wrote " + OUT + ": boxes " + room.boxes.length + ", ramps " + room.ramps.length);
