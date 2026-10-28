// NIGHT74 builder S, slice g7 (the lead's list, after g2). g7 2.7 THE MIX is level 2's combine room (job36 combine): wave 1 a tank + two spitters, wave 2 six swarmers and a
// spitter, wave 3 a tank, a spitter and four swarmers; the lesson "{quickSwap}: KESTREL FAR, GLOVE CLOSE". The shipped room (the level's slot is still old) has three lava
// trenches across its middle (GG 2026-10-12: "random lava pits random pillars that serve no purpose"), a full-height boiler, two ride walls standing IN the trenches, bunkers
// behind the player's spawn that wave 3 comes out of (a spawn in the rear arc), and one 10 m pillar. NIGHT68's melee-only coal chute (rooms/g7.json at t0b) went SHORT at
// NIGHT71 (stall, I2, air65). This is a third try from the shipped room's export: dry, jungle-gym (pattern C), and every piece new (GG 2026-10-09: "don't preserve anything").
// Writes Design/levels/n67/iceline/rooms/g7.json from the shipped EXPORT (rooms-base/g7.json at t0b: idempotent, from the git blob).
//   node Research/2026-10-27-wave74/S/g7/make-g7-r1.cjs        run from the worktree root
// Kept exactly (PRINCIPLES rule 1): the rect, the door, the spawn, the exit, the three waves, the lesson (KESTREL far, glove close: tank and spitters need both), the verb
// the shipped room's rides gave (ride). The two ride faces stay (west 16 m on the wall, east as the tower's lane wall); the pads stay as two hoists; the enemies' climb
// stays on both tiers; the coal bunkers' job (a tier to fight from) is the boiler stage; the lava leaves (a dry room: nothing on the walk).
const fs = require("fs");
const cp = require("child_process");
const OUT = "Design/levels/n67/iceline/rooms/g7.json";
const SRC = "Design/levels/n67/iceline/rooms-base/g7.json";
const base = JSON.parse(cp.execSync("git show a354c0e94:" + SRC, { maxBuffer: 1 << 26 }).toString("utf8"));
const room = JSON.parse(JSON.stringify(base));
const W = "NIGHT74 S g7 r1 (Research/2026-10-27-wave74/S/card-g7.md): ";
const row = room.row;

const box = (kind, min, max, job, why, note, extra) => Object.assign({ zone: "g7", kind, min, max, job, why: W + why, note }, extra || {});
room.boxes = [
  box("wall", [-12, 0, -210], [-11.4, 10, -194], "ride: THE WEST SLUICE WALL (ride north along the west wall onto THE WEST GANTRY, over the yard)",
    "the shipped west sluice wall (18 m, standing in a lava trench) is a 16 m ride face ON the west wall, dry: its landing is the gantry at its north end.",
    "THE WEST SLUICE WALL (ride, cyan, 16 m x 10 m on the west wall)", { ride: true, ride36: { face: "E", planeX: -11.4, lz: [6, 22] } }),
  box("deck", [-12, 0, -216], [-7, 3.5, -210], "line: THE WEST GANTRY (3.5 m: wave 1's west spitter, the ride's landing)",
    "a 5 x 6 m tier in the north-west corner (rimmed): the shipped west coal gantry was 2 m; 3.5 m, not 4, because the stair (6.5 m long, the exit lane's limit) was 'not reached by walkers' at 4 m (0.615 per metre).", "THE WEST GANTRY (3.5 m, 5 x 6 m, north-west)"),
  box("deck", [7.5, 0, -216], [12, 4, -209], "line: THE KEEPER'S TOWER (4 m: wave 1's east spitter, the east stair's landing, the health)",
    "a 4.5 x 7 m tier in the north-east corner (rimmed), as the shipped tower, with a stair along the east wall.", "THE KEEPER'S TOWER (4 m, 4.5 x 7 m, north-east)"),
  box("deck", [-3, 0, -204], [3, 2, -198], "line: THE BOILER STAGE (2 m: the middle tier; a circuit round it, a step between the floor and the gantries)",
    "the shipped boiler was a full-height 3 x 3 m stack in a lava trench; this is a walkable 6 x 6 m tier (2 m, rimmed) in the middle of the yard, dry, with a ramp from the door side.",
    "THE BOILER STAGE (2 m, 6 x 6 m, mid-yard)"),
  box("hull", [-7.2, 0, -193.5], [-5.7, 3.5, -192], "cover: THE WEST MOORING POST (a 3.5 m post by the door half's west: high cover from the far spitters)",
    "a 1.5 x 1.5 x 3.5 m free-standing post (high cover), 3.7 m west of the ramp's foot, off the entry's sight lines to the gantries.", "THE WEST MOORING POST (3.5 m, 1.5 x 1.5 m)"),
  box("hull", [5.7, 0, -193.5], [7.2, 3.5, -192], "cover: THE EAST MOORING POST (the west post's mirror across the axis)",
    "the west post's mirror: the entry yard between them is 11.4 m wide.", "THE EAST MOORING POST (3.5 m, 1.5 x 1.5 m)"),
  box("hull", [3.5, 0, -207.5], [5.5, 1.2, -206], "cover: THE EAST RAIL (1.2 m low cover on the far floor: hop it, shelter behind it from the spitters)",
    "2 x 1.5 x 1.2 m low cover (1.0-1.25 m) on the far floor's east half.", "THE EAST RAIL (1.2 m, 2 x 1.5 m)"),
  box("hull", [-5.5, 0, -207.5], [-3.5, 1.2, -206], "cover: THE WEST RAIL (1.2 m low cover: the east rail's twin across the far floor)",
    "2 x 1.5 x 1.2 m low cover on the far floor's west half.", "THE WEST RAIL (1.2 m, 2 x 1.5 m)"),
];
room.ramps = [
  { zone: "g7", from: [-0.5, -213], to: [-7, -213], width: 3, rise: 3.5,
    job: "climb: THE WEST STAIR (the walkers' way onto the west gantry from the far floor: 3.5 m over 6.5 m: the long way round the sluice wall's ride)",
    why: W + "a route enemies can climb to every floor (GG, NIGHT20): along the north wall, so the walk to the gantry is longer than the ride (I2) and the ride pilot's target line (x -9.6, 10 m along the wall) stays clear.", note: "THE WEST STAIR" },
  { zone: "g7", from: [9.5, -194], to: [9.5, -209], width: 3, rise: 4,
    job: "climb: THE EAST STAIR (the walkers' way onto the keeper's tower: 4 m over 15 m along the east wall)",
    why: W + "the shipped east stair's job (the climb onto the tower): along the east wall, x 8..11.", note: "THE EAST STAIR" },
  { zone: "g7", from: [0, -193], to: [0, -198], width: 3, rise: 2,
    job: "climb: THE STAGE STEP (the door half's ramp onto the boiler stage: 2 m over 5 m, 3.5 m ahead of the spawn)",
    why: W + "the stage's walkers' route; the exit's lane (x 0) runs over it or round it.", note: "THE STAGE STEP" },
];
room.pads = [
  { zone: "g7", at: [6, 0, -203], target: [9.5, 4, -212], apexAbove: 3,
    job: "line: THE COAL HOIST (the yard's east side onto the keeper's tower: the east way up without the stair)",
    why: W + "the shipped coal hoist's job (the floor onto the tower), 10 m from the tower.", note: "THE COAL HOIST" },
  { zone: "g7", at: [-7.8, 0, -203], target: [-10.5, 3.5, -212], apexAbove: 3,
    job: "line: THE WEST HOIST (the yard's west side onto the west gantry: the west way up without the wall)",
    why: W + "the shipped return hoist's job (a way up from the floor), the west mirror of the coal hoist: 3.3 m from the walk line, 1.9 m from the ride lane.", note: "THE WEST HOIST" },
];
room.pits = [];
room.props = room.props.filter((p) => p.kind === "healthPickup");
const hp = room.props[0];
if (!hp) throw new Error("no health prop");
hp.at = [0, 2.9, -201];
// the lights that stood in the lava trench and on the boiler's face leave with them
room.lights = room.lights.filter((l) => !["g7-L2", "g7-L3"].includes(l.id));
// the exit moves 2 m east: the west stair's low end (x -0.5) sat 0.5 m from the old exit (0, 0, -214) and levelgen read "g7 exit to g8 spawn has NO route"
row.exit = [2, 0, -214];
const L6 = room.lights.find((l) => l.id === "g7-L6");
if (!L6) throw new Error("no light L6");
L6.at = [2, L6.at[1], -214];
room.lights.push({ id: "g7-RW", room: "g7", kind: "wall", at: [-11.35, 1.2, -202], normal: [1, 0, 0], size: [16, 0.2, 0.1], color: "cyan", range: 6, intensity: 1.1,
  why: W + "THE WEST SLUICE WALL's cyan ride strip (player-usable: cyan)" });

// ---- nodes: wave 1's tank on the far floor, its spitters on the gantries (seen from the entry, 16 m or more out); wave 3 no longer comes out behind the spawn
row.nodes = [[-2, 0, -209], [2.5, 0, -209], [-5, 0, -210.5], [5.5, 0, -211], [2, 0, -212.5]];
row.highNodes = [[-10, 3.5, -213], [9.5, 4, -212.5]];
row.waveNodes43 = [
  [[-10, 3.5, -213], [9.5, 4, -212.5], [-2, 0, -209]],
  null,
  [[-2, 0, -209], [2.5, 0, -209], [-5, 0, -210.5], [5.5, 0, -211], [-10, 3.5, -213], [2, 0, -212.5]],
];
row.waveNodes43_why = W + "wave 1: the spitters on the two gantries, the tank on the far floor; wave 3's six on the far floor and the west gantry (the shipped wave 3 came out of the bunkers behind the spawn: a spawn in the entry's rear arc).";
row.nodes_why = W + "the ground nodes on the far floor, 14-21 m from the commit feet, clear of the new pieces.";
delete row.fallReturn43;   // the shipped room's lava return (a fallen player came back for 10 health): there is no lava or pit any more
room.drop = Object.assign({}, room.drop, { fallReturn43: W + "fallReturn43 (a fallen player is returned for 10 health) belongs to the shipped room's lava trenches; the rebuilt room is dry and has no pit." });

row.bot43 = false;
row.verb65 = "ride";
row.line65 = [[-10.7, 0, -195], [-10.7, 2, -201], [-10.7, 3, -207], [-10, 3.5, -211]];
row.line65_why = W + "THE WEST SLUICE WALL's ride: along its east face 0.7 m out from the door end to the west gantry";
row.card65 = "Research/2026-10-27-wave74/S/card-g7.md";
row.proofs = [
  { kind: "walk", points: [[0, 0, -186], [-1, 0, -190], [-4.5, 0, -196], [-4.5, 0, -205], [-1, 0, -209], [1, 0, -211], [2, 0, -214]] },
  { kind: "walk", points: [[1, 0, -212.5], [-3.5, 2, -213], [-7, 3.5, -213], [-10, 3.5, -213.5]] },
  { kind: "walk", points: [[9.5, 0, -193], [9.5, 4, -209.5], [9.5, 4, -212.5]] },
  { kind: "walk", points: [[0, 0, -192], [0, 2, -198.5], [0, 2, -202]] },
];
row.frames52 = { _why: W + "the room frames: the door just crossed (the stage, the gantries, the wall), the fight from the stage", entry: [0, 0, -186, 0, 0.04], mid: [0, 2, -201, 0, 0.05], high: [9.5, 4, -212.5, 150, -0.12] };
room._why = W + "every piece new and dry: THE WEST SLUICE WALL (ride) + THE WEST GANTRY + THE WEST STAIR, THE KEEPER'S TOWER + THE EAST STAIR, THE BOILER STAGE + its step, two mooring posts and two rails, two hoists; the rect, the waves, the lesson and the verb are the shipped room's. " + room._why;
fs.writeFileSync(OUT, JSON.stringify(room, null, 1) + "\n");
console.log("wrote " + OUT + ": boxes " + room.boxes.length + ", ramps " + room.ramps.length + ", pads " + room.pads.length);
