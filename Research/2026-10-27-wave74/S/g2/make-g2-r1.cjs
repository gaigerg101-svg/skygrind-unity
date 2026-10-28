// NIGHT74 builder S, slice g2 (the lead's list: iceline g2 after g1 / g6). g2 2.2 THE SHIELD is level 2's warden room (job36 teach): wave 1 two wardens + two
// husks, wave 2 a charger + two husks, the lesson "{fire} THE SHIELD, OR FLANK IT". GG 2026-10-09 ("every single level needs a complete arena overhaul from scratch,
// don't preserve anything") is the reason for a rebuild rather than a patch: n70-patch-share reads the shipped room's 100 % kept as a copy. Writes
// Design/levels/n67/iceline/rooms/g2.json from t0b's file (the shipped export: idempotent, from the git blob).
//   node Research/2026-10-27-wave74/S/g2/make-g2-r1.cjs        run from the worktree root
// Kept exactly (PRINCIPLES rule 1): the rect, the door, the spawn, the exit, both waves, the lesson, the verb (ride), the row's other keys. Every piece is new
// (the shipped room: a 3 x 3 m full-height anvil, a thin free-standing ride panel, three corner / north decks, a 1.2 m tank, a 0.8 m step, three ramps, four pads, a
// prop pillar, over an empty door half): the flank is now a wall ride onto the west gantry, the east side climbs in tiers (ramp, terrace 2 m, gantry 4 m), the anvil
// is a 3.2 m mass to circle, and the door half has rails and a post.
const fs = require("fs");
const cp = require("child_process");
const OUT = "Design/levels/n67/iceline/rooms/g2.json";
const base = JSON.parse(cp.execSync("git show a354c0e94:" + OUT, { maxBuffer: 1 << 26 }).toString("utf8"));
const room = JSON.parse(JSON.stringify(base));
const W = "NIGHT74 S g2 r1 (Research/2026-10-27-wave74/S/card-g2.md): ";
const row = room.row;

const box = (kind, min, max, job, why, note, extra) => Object.assign({ zone: "g2", kind, min, max, job, why: W + why, note }, extra || {});
room.boxes = [
  box("wall", [-12, 0, -50], [-11.4, 10, -33], "ride: THE FLANK WALL (ride north along the west wall onto THE WEST GANTRY, past the wardens' shields)",
    "the shipped free-standing 18 m panel (x -9.6..-9) becomes a 17 m ride face ON the west wall: the flank is the wall, and its landing is the gantry at its north end.",
    "THE FLANK WALL (ride, cyan, 17 m x 10 m on the west wall)", { ride: true, ride36: { face: "E", planeX: -11.4, lz: [8, 25] } }),
  box("deck", [-12, 0, -58], [-8, 4, -50], "line: THE WEST GANTRY (4 m: the flank wall's landing, wave 1's west high ground, the health)",
    "a 4 x 8 m tier in the north-west corner (rimmed): the ride, the stair and the west spring all end on it.", "THE WEST GANTRY (4 m, 4 x 8 m, north-west)"),
  box("deck", [4, 0, -48], [9, 2, -36], "line: THE EAST TERRACE (2 m: the east side's first tier, the way up to the east gantry)",
    "a 5 x 12 m tier on the east wall (rimmed, 2 m): the second floor plane; a ramp at its door end and the east spring land on it.", "THE EAST TERRACE (2 m, 5 x 12 m, east wall)"),
  box("deck", [5.5, 0, -58], [9, 4, -53], "line: THE EAST GANTRY (4 m: the terrace's top, wave 1's east high ground)",
    "a 3.5 x 5 m tier in the north-east corner (rimmed): reached up the terrace and its second ramp.", "THE EAST GANTRY (4 m, 3.5 x 5 m, north-east)"),
  box("hull", [-5, 0, -47], [-1, 3.2, -43], "line: THE ANVIL (4 x 4 m, 3.2 m: the mass a warden turns on: circle it to a warden's back)",
    "the shipped anvil was 3 x 3 m to the roof in the middle of the axis; this one is off the axis to the west (the entry's sight line to wave 1's east nodes is clear), 3.2 m high, 4 x 4 m.",
    "THE ANVIL (3.2 m, 4 x 4 m, west of the axis)"),
  box("hull", [1.5, 0, -40], [3.5, 1.2, -38.5], "cover: THE QUENCH RAIL (1.2 m low cover east of the axis in the door half: hop it, shelter behind it)",
    "2 x 1.5 x 1.2 m low cover (1.0-1.25 m): the shipped 3.5 x 2.5 m quench tank, smaller and nearer the axis.", "THE QUENCH RAIL (1.2 m, 2 x 1.5 m)"),
  box("hull", [-9, 0, -37.5], [-7, 1.2, -36], "cover: THE RUN-UP RAIL (1.2 m low cover at the stair's and the wall's foot, west door half)",
    "2 x 1.5 x 1.2 m low cover west of the lane, clear of the flank wall's lane (x -10.7) by 1.3 m and of the stair (x -9.5..-6.5, from z -42).", "THE RUN-UP RAIL (1.2 m, 2 x 1.5 m)"),
  box("hull", [3.8, 0, -36], [5.3, 3.5, -34.5], "cover: THE DOOR POST (a 3.5 m post 6.5-8 m inside the door, east of the axis: the entry's foothold and high cover)",
    "a 1.5 x 1.5 x 3.5 m free-standing post (high cover) in R5's foothold band (4-8 m inside the door), 3.8 m east of the spawn, 0.2 m west of the terrace step's ramp (x 5.5).", "THE DOOR POST (3.5 m, 1.5 x 1.5 m)"),
  box("hull", [-8.2, 0, -44], [-6.7, 3.5, -42.5], "cover: THE WARDEN'S POST (a 3.5 m post west of the anvil: high cover on the lane's west side)",
    "a 1.5 x 1.5 x 3.5 m free-standing post (high cover), 1.1 m east of the flank wall ride pilot's target line and clear of the entry's sight lines to wave 1 (a first try east of the axis hid the east gantry's spitter).", "THE WARDEN'S POST (3.5 m, 1.5 x 1.5 m)"),
];
room.ramps = [
  { zone: "g2", from: [-0.5, -54], to: [-8, -54], width: 3, rise: 4,
    job: "climb: THE WEST STAIR (the walkers' way onto the west gantry from the far floor: 4 m over 7.5 m: the long way round the flank wall's ride)",
    why: W + "a route enemies can climb to every floor (GG, NIGHT20): along the north wall, so the walk to the gantry is longer than the ride (I2); a first try beside the wall's lane stood on the ride pilot's target and the walk was 1.16 x the ride.", note: "THE WEST STAIR" },
  { zone: "g2", from: [7, -31], to: [7, -36], width: 3, rise: 2,
    job: "climb: THE TERRACE STEP (the door end's ramp onto the east terrace: 2 m over 5 m, its top flush with the terrace's south edge z -36)",
    why: W + "the east tier's first way up, in the door's view on the right.", note: "THE TERRACE STEP" },
  { zone: "g2", from: [7, -45], to: [7, -53], width: 3, rise: 4,
    job: "climb: THE GANTRY STEP (from the terrace's top onto the east gantry: a 4 m wedge whose first half is buried in the terrace, so it climbs 2 m over 4 m from the terrace)",
    why: W + "a wedge cannot start above the floor: it starts at the floor under the terrace (z -45..-48, buried) and clears the terrace's 2 m at z -49.", note: "THE GANTRY STEP" },
];
room.pads = [
  { zone: "g2", at: [-4, 0, -37], target: [-10, 4, -54.5], apexAbove: 3,
    job: "line: THE WEST SPRING (the floor onto the west gantry over the anvil: the flank without the wall)",
    why: W + "the shipped west spring's job (floor to the west gantry): from the door half's west lane, 3 m from the walk line.", note: "THE WEST SPRING" },
  { zone: "g2", at: [2.2, 0, -41], target: [6.5, 2, -45], apexAbove: 3,
    job: "line: THE TERRACE SPRING (the door half onto the east terrace)",
    why: W + "the shipped east spring's job (floor to the east side), onto the terrace.", note: "THE TERRACE SPRING" },
  { zone: "g2", at: [3.5, 0, -55.5], target: [-9.5, 4, -55.5], apexAbove: 3,
    job: "line: THE CROSS SPRING (the far floor, east of the exit, over the stair onto the west gantry)",
    why: W + "the shipped far-floor spring's own geometry and job (the floor onto the west gantry across the far end, 13 m; hold52 PASS on the shipped room): first tries from the terrace (2.4 m short) and from the east gantry (apexAbove is capped by the pad's apex 8 m: 3.3 m short) did not land.", note: "THE CROSS SPRING" },
];
// the shipped room's decor prop (a pillar) leaves with its place; the health moves onto the west gantry
room.props = room.props.filter((p) => p.kind !== "pillar");
const hp = room.props.find((p) => p.kind === "healthPickup");
if (!hp) throw new Error("no health prop");
hp.at = [-10, 4.9, -55];
room.lights.push({ id: "g2-RW", room: "g2", kind: "wall", at: [-11.35, 1.2, -41.5], normal: [1, 0, 0], size: [17, 0.2, 0.1], color: "cyan", range: 6, intensity: 1.1,
  why: W + "THE FLANK WALL's cyan ride strip (player-usable: cyan)" });

// ---- nodes: wave 1 (the wardens and husks) stands on the far floor and both gantries, in the entry's view and 16 m or more out
row.nodes = [[1.5, 0, -50.5], [4.2, 0, -52], [-3, 0, -51.5], [0, 0, -53.5]];
row.highNodes = [[-10, 4, -54.5], [8.2, 4, -55]];
row.waveNodes43 = [[[1.5, 0, -50.5], [4.2, 0, -52], [-10, 4, -54.5], [8.2, 4, -55]]];
row.waveNodes43_why = W + "wave 1 on the far floor and the two gantries (the shipped nodes stood on decks that are gone).";
row.nodes_why = W + "the ground nodes on the far floor, clear of the new pieces (the east gantry step's footprint starts at x 5.5).";

row.bot43 = false;
row.verb65 = "ride";
row.line65 = [[-10.7, 0, -34], [-10.7, 2, -40], [-10.7, 3.5, -48], [-10, 4, -53]];
row.line65_why = W + "THE FLANK WALL's ride: along its east face 0.7 m out from the door end to the west gantry";
row.card65 = "Research/2026-10-27-wave74/S/card-g2.md";
row.proofs = [
  { kind: "walk", points: [[0, 0, -34.5], [0, 0, -40], [0, 0, -47], [0, 0, -51], [0, 0, -56]] },
  { kind: "walk", points: [[7, 0, -30.5], [7, 2, -37.5], [7, 2, -46], [7, 4, -52.5], [7.5, 4, -53.8]] },
  { kind: "walk", points: [[-0.5, 0, -52], [-3, 1.5, -54], [-8, 4, -54], [-10, 4, -55]] },
];
row.frames52 = { _why: W + "the room frames: the door just crossed (the anvil, the terrace, the wall), the fight from the west gantry", entry: [0, 0, -34.5, 0, 0.04], mid: [-4, 0, -40, 10, 0.05], high: [-10, 4, -55, -140, -0.12] };
room._why = W + "every piece new: THE FLANK WALL (ride) + THE WEST GANTRY + THE WEST STAIR, THE EAST TERRACE + its step + THE EAST GANTRY + its step, THE ANVIL (3.2 m), two rails and a post, three springs; the rect, the waves, the lesson and the verb are the shipped room's. " + room._why;
fs.writeFileSync(OUT, JSON.stringify(room, null, 1) + "\n");
console.log("wrote " + OUT + ": boxes " + room.boxes.length + ", ramps " + room.ramps.length + ", pads " + room.pads.length);
